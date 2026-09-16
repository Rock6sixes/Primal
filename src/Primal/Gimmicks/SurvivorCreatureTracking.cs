using System.Collections.Generic;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared mechanism #3 from COMMUNITY_CHALLENGES.md: cross-region persistent "carry this
    /// creature alive" tracking -- rated the single biggest recurring need there (Chieftain,
    /// Fishtank, Symbiote, Charged, Offering, Chef, Guardian, Pilgrimage all need some version of
    /// it). Session-only, in-memory state (not persisted across a save/quit -- every one of those
    /// challenges completes or fails within a bounded number of cycles while the game stays open,
    /// same as this mod's other in-progress-cycle trackers like Kill List's own pending set).
    ///
    /// **The EntityID gotcha this whole design works around, confirmed by reading its decompiled
    /// source (not guessed):** EntityID.Equals(EntityID) compares ONLY the `number` field, but
    /// EntityID.GetHashCode() falls through to the inherited ValueType.GetHashCode(), which hashes
    /// across ALL of the struct's fields (`spawner`, `number`, `altSeed`). Two EntityIDs that
    /// Equals() says are equal (same `number`, different `spawner`/`altSeed`) are consequently NOT
    /// guaranteed to produce the same hash code -- a real violation of the Equals/GetHashCode
    /// contract .NET's own Dictionary&lt;TKey,TValue&gt;/HashSet&lt;T&gt; require, baked into the
    /// base game itself. Using EntityID directly as a dictionary/hash-set key would silently produce
    /// wrong lookups. This class sidesteps it entirely by keying everything on the plain `int
    /// number` field instead -- exactly what EntityID's own Equals() already reduces identity to,
    /// so this loses nothing Equals() would have caught anyway.
    ///
    /// **Why this only tracks a "last known reference", not a live cross-region index.** There is
    /// no confirmed global "find this AbstractCreature by ID, wherever it currently is" lookup
    /// anywhere in the decompiled source -- creatures are indexed per-room (AbstractRoom.entities),
    /// per-World, not in one flat table. A creature that has genuinely left the room/region a
    /// caller last saw it in can't be freshly located by this class alone; callers are expected to
    /// call Touch(...) with a fresh reference whenever their own patch happens to see the creature
    /// again (e.g. a Chieftain-style tracker's own per-tick check of "is this scavenger still in
    /// the room with me" naturally re-Touches it), and FindInRoom(...) is provided for the one
    /// case that doesn't need a fresh sighting first: checking whether a previously-tracked
    /// creature has arrived in a room the caller DOES currently have (its destination room, most
    /// often).
    /// </summary>
    internal static class SurvivorCreatureTracking
    {
        private sealed class Watch
        {
            public readonly Dictionary<int, AbstractCreature> ByNumber = new Dictionary<int, AbstractCreature>();
        }

        private static readonly Dictionary<string, Watch> _watches = new Dictionary<string, Watch>();

        /// <summary>The stable identity key for a creature, per this class's own header comment --
        /// use this, never the EntityID struct itself, anywhere a hash-based collection is
        /// involved.</summary>
        public static int KeyOf(AbstractCreature creature) => creature.ID.number;

        /// <summary>Starts (or refreshes) tracking `creature` under `trackerKey` -- a caller-chosen
        /// name for one instance of "the set of creatures this particular challenge attempt cares
        /// about" (e.g. "Fishtank:" + characterID, so two characters' attempts never share state).
        /// Safe to call every tick a caller happens to see the creature; cheap (one dictionary
        /// write).</summary>
        public static void Touch(string trackerKey, AbstractCreature creature)
        {
            if (creature == null)
            {
                return;
            }
            GetOrCreateWatch(trackerKey).ByNumber[KeyOf(creature)] = creature;
        }

        /// <summary>Stops tracking one creature under this key -- e.g. once a challenge attempt is
        /// abandoned, or a creature is confirmed dead and no longer relevant to keep checking.</summary>
        public static void Untrack(string trackerKey, AbstractCreature creature)
        {
            if (creature != null && _watches.TryGetValue(trackerKey, out Watch watch))
            {
                watch.ByNumber.Remove(KeyOf(creature));
            }
        }

        /// <summary>Drops every creature tracked under this key -- e.g. when a challenge attempt
        /// resets (a new cycle, a fresh start after failure).</summary>
        public static void ClearTracker(string trackerKey)
        {
            _watches.Remove(trackerKey);
        }

        public static int TrackedCount(string trackerKey)
        {
            return _watches.TryGetValue(trackerKey, out Watch watch) ? watch.ByNumber.Count : 0;
        }

        /// <summary>Every creature currently tracked under this key, by last-known reference (see
        /// this class's own header comment on staleness) -- for a caller that wants to run its own
        /// alive/region logic across the whole set rather than one creature at a time.</summary>
        public static IEnumerable<AbstractCreature> TrackedCreatures(string trackerKey)
        {
            return _watches.TryGetValue(trackerKey, out Watch watch)
                ? (IEnumerable<AbstractCreature>)watch.ByNumber.Values
                : System.Array.Empty<AbstractCreature>();
        }

        /// <summary>True only if every creature tracked under this key is currently alive
        /// (CreatureState.dead, confirmed via decompilation to be `!alive`). Deliberately does NOT
        /// also require `realizedCreature != null` -- confirmed via AbstractCreature.Realize()
        /// that a creature routinely cycles between realized (in an active/loaded room) and
        /// unrealized (abstract-only, e.g. far from the player) with no relationship to whether
        /// it's alive at all; requiring a live realized body here would wrongly report a perfectly
        /// alive but merely distant creature as "not alive". False (not true-by-default) if nothing
        /// is tracked yet, since "all zero of them are alive" isn't a meaningful pass for any of
        /// these challenges.</summary>
        public static bool AllTrackedAlive(string trackerKey)
        {
            if (!_watches.TryGetValue(trackerKey, out Watch watch) || watch.ByNumber.Count == 0)
            {
                return false;
            }

            foreach (AbstractCreature creature in watch.ByNumber.Values)
            {
                if (creature == null || creature.state.dead)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Region acronym (World.name) of a creature's last-known position, or null if it
        /// can't be resolved. Same field this mod's other region checks (SurvivorRegionMarkers)
        /// use -- kept as a small static helper here rather than duplicated, since this is the
        /// class most callers tracking a creature's location will already be using.</summary>
        public static string RegionOf(AbstractCreature creature)
        {
            return creature?.world?.name;
        }

        /// <summary>Searches one specific room's own entity list for a creature previously tracked
        /// under `trackerKey`, refreshing (Touch-ing) its reference if found. Use this to check "has
        /// a creature I tracked earlier arrived in the room I currently have" without needing a
        /// separate per-tick patch watching that creature directly -- e.g. checking the destination
        /// region's shelter room for a scavenger that was tracked back at the start of a Chieftain
        /// attempt.</summary>
        public static AbstractCreature FindInRoom(string trackerKey, AbstractRoom room, int entityNumber)
        {
            if (room?.entities == null)
            {
                return null;
            }

            foreach (AbstractWorldEntity entity in room.entities)
            {
                if (entity is AbstractCreature creature && creature.ID.number == entityNumber)
                {
                    Touch(trackerKey, creature);
                    return creature;
                }
            }
            return null;
        }

        private static Watch GetOrCreateWatch(string trackerKey)
        {
            if (!_watches.TryGetValue(trackerKey, out Watch watch))
            {
                watch = new Watch();
                _watches[trackerKey] = watch;
            }
            return watch;
        }
    }
}
