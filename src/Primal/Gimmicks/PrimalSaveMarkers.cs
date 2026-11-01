using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared read/write access to this mod's own markers inside
    /// DeathPersistentSaveData.unrecognizedSaveStrings -- the game's own designed-for-this
    /// passthrough list for save-file content it doesn't recognize, which round-trips anything put
    /// there through save/load untouched, forever.
    ///
    /// Three separate features independently grew their own copy of this pattern before 2026-09-15
    /// (OneSpearGimmick's lifetime pull counter, the Kill List's per-species kill record, and the
    /// Slugpup gimmick's "beat the game, stop applying" flag), each with its own marker-string
    /// formatting and its own linear scan of the list. This consolidates all three: the "&lt;dpB&gt;"
    /// delimiter convention is documented in exactly one place, and every lookup goes through one
    /// cache instead of walking the list.
    ///
    /// **Why the cache matters.** These markers used to be checked with List&lt;string&gt;.Contains
    /// (or a foreach doing the same thing), i.e. O(n) string comparisons per lookup. That was fine
    /// while the only caller was OneSpear's once-per-spear-pull check, but it stopped being fine
    /// once (a) the Kill List started adding up to 55 markers of its own to the same list, and
    /// (b) the Slugpup gimmick started checking its own marker from a Postfix on Player.isSlugpup's
    /// getter -- a property vanilla reads 53 separate times in Player.cs alone, many of them in
    /// per-tick update paths. That combination meant a full ~56-entry scan running dozens of times
    /// per frame. Now each DeathPersistentSaveData gets one HashSet built lazily on first use.
    ///
    /// **Why the cache can't go stale.** It's keyed on the DeathPersistentSaveData instance itself
    /// via ConditionalWeakTable, so loading a different save (a fresh instance) naturally gets a
    /// fresh cache and the old one is collected with the old save data. On top of that, every
    /// lookup compares the cached entry count against the live list's Count and rebuilds if they
    /// differ, so anything else mutating the list -- vanilla, another mod, or a reload that
    /// repopulates an existing instance -- self-heals on the next read. The only mutation that
    /// could slip past is one that replaces an entry without changing the list's length, which
    /// nothing does to this list in practice and which could only ever affect this mod's own
    /// markers anyway.
    /// </summary>
    internal static class PrimalSaveMarkers
    {
        /// <summary>The delimiter this mod puts between a marker's own key and its parts. Chosen
        /// (originally by OneSpearGimmick) to match nothing DeathPersistentSaveData.FromString
        /// itself parses, so every marker written this way lands in -- and stays in -- the
        /// unrecognized-strings passthrough list rather than being consumed as real save data.</summary>
        public const string Delimiter = "<dpB>";

        private sealed class MarkerCache
        {
            public HashSet<string> Set;
            public int SourceCount;
        }

        private static readonly ConditionalWeakTable<DeathPersistentSaveData, MarkerCache> _caches =
            new ConditionalWeakTable<DeathPersistentSaveData, MarkerCache>();

        /// <summary>The DeathPersistentSaveData for this game's current story session, or null off
        /// story mode (Arena, or too early/late in a session's life for a save state to exist).
        /// SlugpupGimmick and OneSpearGimmick each already had their own private copy of this exact
        /// one-liner before the 2026-09-16 Survivor shared-mechanisms pass added a third caller
        /// (SurvivorEvents' consumers) -- pulled up here rather than adding a fourth.</summary>
        public static DeathPersistentSaveData SaveDataFor(RainWorldGame game)
        {
            return game?.GetStorySession?.saveState?.deathPersistentSaveData;
        }

        /// <summary>Builds a marker string. Allocates, so callers on a hot path should build theirs
        /// once and hold onto it (see SlugpupEndingMarker) rather than calling this per frame.</summary>
        public static string Marker(string key, string part)
        {
            return key + Delimiter + part;
        }

        /// <summary>Two-part overload, for markers scoped to both a character and something else
        /// (a creature type, a use index).</summary>
        public static string Marker(string key, string part1, string part2)
        {
            return key + Delimiter + part1 + Delimiter + part2;
        }

        /// <summary>The prefix shared by every marker with this key and first part -- for counting
        /// a family of markers (see CountWithPrefix).</summary>
        public static string Prefix(string key, string part)
        {
            return key + Delimiter + part + Delimiter;
        }

        /// <summary>Whether this exact marker is already on the save file. O(1) after the first
        /// call for a given save data -- see this class's header comment.</summary>
        public static bool Has(DeathPersistentSaveData saveData, string marker)
        {
            HashSet<string> set = GetSet(saveData);
            return set != null && marker != null && set.Contains(marker);
        }

        /// <summary>Adds the marker if it isn't already present. Returns true only the first time a
        /// given marker is ever written, so callers can log/act on the real transition.</summary>
        public static bool Add(DeathPersistentSaveData saveData, string marker)
        {
            if (saveData?.unrecognizedSaveStrings == null || marker == null)
            {
                return false;
            }

            HashSet<string> set = GetSet(saveData);
            if (set == null || !set.Add(marker))
            {
                return false;
            }

            saveData.unrecognizedSaveStrings.Add(marker);
            // Keep the cache's own view of the list length in step with the write we just made, so
            // this Add doesn't itself look like an external mutation on the next read.
            if (_caches.TryGetValue(saveData, out MarkerCache cache))
            {
                cache.SourceCount = saveData.unrecognizedSaveStrings.Count;
            }
            return true;
        }

        /// <summary>How many markers currently start with this prefix -- used by OneSpearGimmick,
        /// whose markers are indexed per use so their count is the number of pulls already spent.
        /// Walks the live list rather than the cache (the cache is built for exact-match lookups,
        /// and this is a cold path called once per spear-pull attempt).</summary>
        public static int CountWithPrefix(DeathPersistentSaveData saveData, string prefix)
        {
            List<string> entries = saveData?.unrecognizedSaveStrings;
            if (entries == null || prefix == null)
            {
                return 0;
            }

            int count = 0;
            foreach (string entry in entries)
            {
                if (entry != null && entry.StartsWith(prefix))
                {
                    count++;
                }
            }
            return count;
        }

        private static HashSet<string> GetSet(DeathPersistentSaveData saveData)
        {
            List<string> entries = saveData?.unrecognizedSaveStrings;
            if (entries == null)
            {
                return null;
            }

            MarkerCache cache = _caches.GetOrCreateValue(saveData);
            if (cache.Set == null || cache.SourceCount != entries.Count)
            {
                // First use for this save data, or something outside this class changed the list
                // (vanilla, another mod, a reload into an existing instance) -- rebuild from
                // scratch rather than trusting a possibly-stale view.
                cache.Set = new HashSet<string>(entries);
                cache.SourceCount = entries.Count;
            }
            return cache.Set;
        }
    }
}
