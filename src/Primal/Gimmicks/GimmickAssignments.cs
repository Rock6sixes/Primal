using System;
using System.Collections.Generic;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Runtime source of truth for "is gimmick G enabled on character C". Every gimmick's
    /// Harmony patch calls IsEnabled() (usually via GimmickGate) to decide whether to do anything
    /// for a given player.
    ///
    /// There are two layers, cheapest first:
    ///  1. A Remix-configured Configurable&lt;bool&gt;, if PrimalOptions has built one for
    ///     this (character, gimmick) pair and the player has opened Remix at least once this
    ///     session (Configurable defaults to its bound default value either way).
    ///  2. The mod's own hardcoded default (SetDefault), used before Remix has bound anything
    ///     (e.g. if the options screen is never opened) and as the Configurable's initial value.
    ///
    /// Register()/SetDefault() are called once at load (PrimalOptions.EnsureConfigBound and
    /// this class's own GimmickDefaults pass, respectively); IsEnabled() is called every frame per
    /// player by the gimmick patches, so it deliberately does no allocation.
    ///
    /// **That last sentence used to be false.** Until 2026-09-15 the composite dictionary key was
    /// built as `characterID + "|" + gimmickID`, i.e. a fresh string allocation on every single
    /// call -- from a method the doc right above it claimed allocated nothing, called dozens of
    /// times per frame across every active gimmick (the Slugpup gimmick alone hangs off
    /// Player.isSlugpup's getter, which vanilla reads 53 times in Player.cs). Replaced with the
    /// AssignmentKey struct below: same single-lookup flat dictionary, no allocation, no
    /// dependency on ValueTuple being available in the target framework.
    /// </summary>
    internal static class GimmickAssignments
    {
        /// <summary>A (characterID, gimmickID) pair used as a dictionary key without allocating.
        /// Hand-written rather than a ValueTuple purely to avoid depending on System.ValueTuple
        /// being present in whichever net48 reference assemblies this ends up building against
        /// (this project builds against Mono's on Linux -- see Primal.csproj's
        /// FrameworkPathOverride).</summary>
        private readonly struct AssignmentKey : IEquatable<AssignmentKey>
        {
            private readonly string _characterID;
            private readonly string _gimmickID;

            public AssignmentKey(string characterID, string gimmickID)
            {
                _characterID = characterID;
                _gimmickID = gimmickID;
            }

            public bool Equals(AssignmentKey other)
            {
                return string.Equals(_characterID, other._characterID, StringComparison.Ordinal)
                    && string.Equals(_gimmickID, other._gimmickID, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return obj is AssignmentKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = _characterID != null ? _characterID.GetHashCode() : 0;
                    return (hash * 397) ^ (_gimmickID != null ? _gimmickID.GetHashCode() : 0);
                }
            }
        }

        private static readonly Dictionary<AssignmentKey, bool> _defaults = new Dictionary<AssignmentKey, bool>();
        private static readonly Dictionary<AssignmentKey, Configurable<bool>> _overrides = new Dictionary<AssignmentKey, Configurable<bool>>();

        /// <summary>Added 2026-09-17: a live, O(1)-to-query count of how many characters currently
        /// have each gimmick enabled -- kept in sync incrementally (SetDefault/Register adjust it
        /// on write, Configurable.OnChange adjusts it on every live Remix toggle) rather than
        /// recomputed by scanning _defaults/_overrides on every query. Exists so a gimmick with
        /// many always-registered listeners on hot/frequent vanilla events (Community Challenges'
        /// own 50 ChallengeTrackers, all permanently subscribed to shared events regardless of
        /// whether anyone actually uses the gimmick, was the motivating case) can fail fast with
        /// one dictionary lookup instead of every single listener separately re-deriving "is my own
        /// gimmick assigned to this specific player" from scratch on every relevant event.</summary>
        private static readonly Dictionary<string, int> _enabledCount = new Dictionary<string, int>();

        private static void AdjustEnabledCount(string gimmickID, int delta)
        {
            if (delta == 0 || gimmickID == null)
            {
                return;
            }
            _enabledCount.TryGetValue(gimmickID, out int count);
            count += delta;
            if (count <= 0)
            {
                _enabledCount.Remove(gimmickID);
            }
            else
            {
                _enabledCount[gimmickID] = count;
            }
        }

        /// <summary>Sets the mod-defined default for whether a gimmick is enabled on a
        /// character absent any Remix configuration. Call during startup, before any
        /// PrimalOptions instance is constructed (its checkboxes read this as their
        /// initial value).</summary>
        public static void SetDefault(string characterID, string gimmickID, bool enabled)
        {
            var key = new AssignmentKey(characterID, gimmickID);
            bool wasCountedByDefault = !_overrides.ContainsKey(key)
                && _defaults.TryGetValue(key, out bool oldValue) && oldValue;
            _defaults[key] = enabled;

            // Only affects the live count while no override has been bound for this key yet --
            // once Register() binds one, the override is authoritative for it instead (see
            // Register's own comment), and a default set afterward (shouldn't happen per this
            // class's own call-order contract, but cheap to guard regardless) is inert here.
            if (!_overrides.ContainsKey(key) && wasCountedByDefault != enabled)
            {
                AdjustEnabledCount(gimmickID, enabled ? 1 : -1);
            }
        }

        public static bool GetDefault(string characterID, string gimmickID)
        {
            return _defaults.TryGetValue(new AssignmentKey(characterID, gimmickID), out bool value) && value;
        }

        /// <summary>Called by PrimalOptions once it has bound a Remix checkbox for this
        /// pair, so IsEnabled() can start reading the live, user-editable value.</summary>
        public static void Register(string characterID, string gimmickID, Configurable<bool> configurable)
        {
            var key = new AssignmentKey(characterID, gimmickID);

            // Un-count whatever this key's own default was contributing -- the override is about
            // to become authoritative for it instead, and double-counting both would overstate the
            // live total.
            if (!_overrides.ContainsKey(key) && _defaults.TryGetValue(key, out bool defaultValue) && defaultValue)
            {
                AdjustEnabledCount(gimmickID, -1);
            }

            _overrides[key] = configurable;
            if (configurable != null && configurable.Value)
            {
                AdjustEnabledCount(gimmickID, 1);
            }

            if (configurable != null)
            {
                configurable.OnChange += () => AdjustEnabledCount(gimmickID, configurable.Value ? 1 : -1);
            }
        }

        /// <summary>Added 2026-09-17: true if at least one character currently has `gimmickID`
        /// enabled (whether via a live Remix override or an unbound default) -- an O(1) read of
        /// the live count above. Intended as the very first check a gimmick with many always-
        /// registered event listeners does, before any of its own per-player resolution --
        /// ChallengeTracker.TryGetEnabledCharacter is the first consumer, but this is a general
        /// primitive any gimmick's own hot-path patches can adopt the same way.</summary>
        public static bool IsEnabledForAnyCharacter(string gimmickID)
        {
            return gimmickID != null && _enabledCount.TryGetValue(gimmickID, out int count) && count > 0;
        }

        /// <summary>Like IsEnabledForAnyCharacter, but also true for any of the 50 community
        /// challenges while CommunityChallengeCountRequirement is assigned to anyone (2026-11-10):
        /// that requirement tracks all 50 without them being assigned individually. Use this (not
        /// IsEnabledForAnyCharacter) as the fast-path gate for requirement TRACKING; keep
        /// IsEnabledForAnyCharacter for "is this actually assigned" questions.</summary>
        public static bool IsTrackedForAnyCharacter(string id)
        {
            return IsEnabledForAnyCharacter(id)
                || (CommunityChallengesRoster.Contains(id) && IsEnabledForAnyCharacter(CommunityChallengeCountRequirement.RequirementID));
        }

        /// <summary>Per-character counterpart of IsTrackedForAnyCharacter: assigned directly, or a
        /// community challenge while this character has CommunityChallengeCountRequirement.</summary>
        public static bool IsTracked(string characterID, string id)
        {
            return IsEnabled(characterID, id)
                || (CommunityChallengesRoster.Contains(id) && IsEnabled(characterID, CommunityChallengeCountRequirement.RequirementID));
        }

        /// <summary>The live Configurable behind this pair, if one has been bound. Lets
        /// PrimalOptions read and flip assignments through this class rather than keeping its own
        /// parallel dictionary of the same objects under a second key format.</summary>
        public static Configurable<bool> GetConfigurable(string characterID, string gimmickID)
        {
            return _overrides.TryGetValue(new AssignmentKey(characterID, gimmickID), out Configurable<bool> configurable)
                ? configurable
                : null;
        }

        /// <summary>Whether the given gimmick is currently enabled for the given character.
        /// Safe to call every frame; does not allocate.</summary>
        public static bool IsEnabled(string characterID, string gimmickID)
        {
            if (characterID == null || gimmickID == null)
            {
                return false;
            }
            var key = new AssignmentKey(characterID, gimmickID);
            if (_overrides.TryGetValue(key, out Configurable<bool> configurable))
            {
                return configurable.Value;
            }
            return _defaults.TryGetValue(key, out bool value) && value;
        }
    }
}
