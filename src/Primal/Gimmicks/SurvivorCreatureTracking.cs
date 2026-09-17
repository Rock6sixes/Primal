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

        /// <summary>Added 2026-09-16, closing the actual missing piece of this mechanism: not
        /// identity-tracking itself (already covered above), but "is the player literally holding
        /// this creature right now" -- the live-carry half Symbiote/Charged/Offering/Chef each
        /// need, combined with the new SurvivorEvents.OnPlayerEnteredRoom to answer "arrived at
        /// the destination room WHILE carrying it". Creature.grasps (confirmed public property,
        /// `Grasp[] grasps { get; set; }`) is the exact same array-of-Grasp structure
        /// PrimalKillTracker's own Grabbed hook already reads from the other side
        /// (`grasp.grabber is Player`) -- trusted directly here too, same precedent. Requires the
        /// tracked creature to be REALIZED (a creature can't be in anyone's grasp while merely
        /// abstract) -- unlike AllTrackedAlive, that's the correct behavior here, not a bug: if
        /// it's not realized, it's definitionally not being carried right now.</summary>
        public static bool IsBeingCarriedBy(Player player, AbstractCreature creature)
        {
            Creature.Grasp[] grasps = player?.grasps;
            PhysicalObject realized = creature?.realizedCreature;
            if (grasps == null || realized == null)
            {
                return false;
            }

            foreach (Creature.Grasp grasp in grasps)
            {
                if (grasp != null && grasp.grabbed == realized)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Added 2026-09-17, pulled up after this exact loop (or a one-line variant of
        /// it) turned out to be independently reimplemented in six separate trackers --
        /// PilgrimageTracker.FindCarriedLizardOfType, ChefTracker.FindCarriedAlbinoJetfish,
        /// OfferingTracker.FindCarriedTargetMouse, SymbioteTracker.FindCarriedSquidcada,
        /// ChargedTracker.FindCarriedBabyCentipede, and half of GuardianTracker.TryFindBothCarried
        /// -- each just a grasp-array scan for a Creature matching some predicate, differing only
        /// in which predicate. Takes a predicate over the REALIZED Creature (not the abstract one)
        /// since every existing caller's own match condition needs the realized object -- a
        /// concrete subtype check, a variant-identification helper call, or both -- and hands back
        /// the AbstractCreature, matching what every caller actually goes on to track via
        /// Touch/TrackedCreatures. Doesn't itself filter on `!creature.dead` -- callers that need
        /// that (GuardianTracker does, several others don't) include it in their own predicate,
        /// preserving each one's exact prior behavior rather than silently changing it here.</summary>
        public static AbstractCreature FindCarried(Player player, System.Func<Creature, bool> predicate)
        {
            Creature.Grasp[] grasps = player?.grasps;
            if (grasps == null)
            {
                return null;
            }
            foreach (Creature.Grasp grasp in grasps)
            {
                if (grasp?.grabbed is Creature creature && predicate(creature))
                {
                    return creature.abstractCreature;
                }
            }
            return null;
        }

        /// <summary>Added 2026-09-17, pulled up after this exact one-liner turned out to be
        /// independently redeclared as a private static helper in five separate trackers
        /// (Pilgrimage, Chef, Offering, Symbiote, Charged) -- each just "the first (and for four of
        /// those five, the only) creature tracked under this key". Null if nothing is tracked under
        /// `trackerKey` at all.</summary>
        public static AbstractCreature FirstTracked(string trackerKey)
        {
            foreach (AbstractCreature creature in TrackedCreatures(trackerKey))
            {
                return creature;
            }
            return null;
        }

        /// <summary>Added 2026-09-17, pulled up after this exact room-entity scan (or a close
        /// variant checking more than one type per pass) turned out to be independently
        /// reimplemented in ChieftainTracker.FindNearbyLiveScavengers and inline in
        /// FishtankTracker/SafariTracker -- "every live creature of type X currently in this
        /// room," read off AbstractRoom.entities. Takes the ABSTRACT room (not a realized Room)
        /// since that's what every existing caller already had in hand at its own call site
        /// (either directly, or one property access away via Room.abstractRoom).</summary>
        public static List<AbstractCreature> LiveCreaturesInRoom(AbstractRoom room, CreatureTemplate.Type type)
        {
            var result = new List<AbstractCreature>();
            if (room?.entities == null)
            {
                return result;
            }
            foreach (AbstractWorldEntity entity in room.entities)
            {
                if (entity is AbstractCreature creature && !creature.state.dead && creature.creatureTemplate?.type == type)
                {
                    result.Add(creature);
                }
            }
            return result;
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
