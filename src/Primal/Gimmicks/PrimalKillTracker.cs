using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Global, mod-wide "which creature types has this character personally killed" tracking --
    /// moved out of the Kill List gimmick 2026-09-16 at the user's own request: "the kill list
    /// gimmick is supposed to be more for forcing/rewarding the character for doing all of these
    /// kills... the tracker, which is now needed across multiple gimmicks, should be moved out as
    /// a global effect, always tracking primal characters' kills."
    ///
    /// **What actually changed vs. the original Kill List-only version.** Every mechanism here
    /// (Creature.killTag attribution, the Grabbed-based fallback handler, the pending/commit/
    /// discard cycle-success/cycle-failure split, the PrimalSaveMarkers persistence format) is
    /// carried over UNCHANGED from CreatureKillListGimmick.cs's own original implementation -- see
    /// that file's own header comment (still there) for the full trace of why each piece works the
    /// way it does. Two things actually changed:
    ///
    ///  1. **Gating.** Every hook below now checks only GimmickGate.TryGetCharacter (on a reserved
    ///     Primal slot, real character resolved) instead of GimmickGate.IsActiveFor(player,
    ///     CreatureKillListGimmick.ID, ...)/GimmickAssignments.IsEnabled(..., CreatureKillListGimmick
    ///     .ID) -- kills are now recorded for EVERY Primal-slot character unconditionally,
    ///     regardless of which gimmicks (if any) they have assigned. CreatureKillListGimmick.cs
    ///     itself keeps its own gating for its own UI (the cycle-end button only shows if Kill
    ///     List is actually assigned) -- only the underlying RECORDING became unconditional.
    ///  2. **No more roster restriction at recording time.** The original Postfix/Grabbed hooks
    ///     both skipped anything not in KillListRegistry.ByID (Kill List's own curated 55-species
    ///     display list) -- dropped entirely here. Recording every CreatureTemplate.Type a
    ///     character kills, not just a curated subset, is what actually makes this usable by a
    ///     tracker like Ranger ("any 32 unique creatures", no restriction to any particular list)
    ///     without artificially tying its own rules to Kill List's own display curation. Vanilla
    ///     registers on the order of 90 CreatureTemplate.Type values total across every DLC, so
    ///     this is still a small, bounded set of possible markers per character -- no unbounded
    ///     growth risk from dropping the restriction.
    ///
    /// **IsKilled/UniqueKillCount both also count still-pending (not yet committed) kills, and
    /// here's exactly why that matters.** SurvivorEvents.OnSuccessfulHibernation and
    /// PrimalKillCommitHook (below) are BOTH separate Prefix patches on the same method
    /// (RainWorldGame.Win) -- Harmony gives no ordering guarantee between multiple Prefixes on one
    /// method unless one explicitly declares priority relative to the other, which neither does.
    /// A ChallengeTracker subscribed to OnSuccessfulHibernation could therefore run its own check
    /// BEFORE or AFTER this file's own commit Prefix has actually moved this cycle's kills from
    /// "pending" into permanent storage -- reading only the permanent record would then be a coin
    /// flip on whether this cycle's own final kill (e.g. the one that pushes Ranger over 32, or
    /// the Leviathan Spearfisher needed) is visible yet. Rather than adding HarmonyPriority
    /// attributes to force an order (fragile, and easy to silently break if a third patch on the
    /// same method shows up later), IsKilled/UniqueKillCount both fold the in-memory pending set
    /// into their answer directly -- correct regardless of which Prefix Harmony happens to run
    /// first, since the pending set itself is already fully up to date the instant a kill lands
    /// (recorded synchronously in the Die() Postfix, long before Win is ever called).
    ///
    /// Every [HarmonyPatch] class below is top-level, not nested inside PrimalKillTracker itself --
    /// matching every existing patch class in this codebase (see SurvivorEvents.cs's own header
    /// comment for the same rule and why), rather than introducing an untested nested-patch-class
    /// structure. That means the members these hooks touch (RecordPendingKill, RecordHandler,
    /// TryGetRecentHandler, the reflected KillTagField, the _recorded/_handlers tables) are
    /// `internal`, not `private` -- still invisible outside this assembly, just visible to these
    /// sibling top-level classes in the same namespace.
    /// </summary>
    internal static class PrimalKillTracker
    {
        private const string SaveMarkerKey = "PRIMAL_KILLLIST";

        // Creature.killTag is genuinely new surface for this codebase (nothing else reads it) --
        // resolved via reflection per Gotcha #1 discipline rather than trusted directly.
        internal static readonly FieldInfo KillTagField = AccessTools.Field(typeof(Creature), "killTag");

        // Guards against double-counting: Die() can run more than once on the same instance.
        internal static readonly ConditionalWeakTable<Creature, object> Recorded = new ConditionalWeakTable<Creature, object>();

        // In-memory only, per characterID -- never itself persisted. See this class's own header
        // comment for why IsKilled/UniqueKillCount both fold this in alongside the permanent
        // record, not just the "pending vs. committed" story CreatureKillListGimmick's own header
        // comment already covers.
        private static readonly Dictionary<string, HashSet<string>> _pending = new Dictionary<string, HashSet<string>>();

        // "Who last handled this creature" -- fallback attribution for death causes that never set
        // vanilla's own Creature.killTag at all (e.g. Leech's airDrown-out-of-water death).
        internal sealed class HandlerRecord
        {
            public AbstractCreature Handler;
            public int Tick;
        }

        internal static readonly ConditionalWeakTable<Creature, HandlerRecord> Handlers = new ConditionalWeakTable<Creature, HandlerRecord>();

        // 2400 ticks = 60 seconds at the fixed 40 ticks/second simulation rate -- see
        // TimeLimitGimmick.cs's own TicksPerSecond doc comment for that constant.
        private const int HandlerAttributionWindowTicks = 2400;

        internal static void RecordHandler(Creature creature, AbstractCreature handler, int tick)
        {
            HandlerRecord record = Handlers.GetOrCreateValue(creature);
            record.Handler = handler;
            record.Tick = tick;
        }

        internal static bool TryGetRecentHandler(Creature creature, int currentTick, out AbstractCreature handler)
        {
            if (Handlers.TryGetValue(creature, out HandlerRecord record) && currentTick - record.Tick <= HandlerAttributionWindowTicks)
            {
                handler = record.Handler;
                return handler != null;
            }
            handler = null;
            return false;
        }

        private static string Marker(string characterID, string creatureID)
        {
            return PrimalSaveMarkers.Marker(SaveMarkerKey, characterID, creatureID);
        }

        internal static void RecordPendingKill(string characterID, string creatureID)
        {
            if (!_pending.TryGetValue(characterID, out HashSet<string> set))
            {
                set = new HashSet<string>();
                _pending[characterID] = set;
            }
            set.Add(creatureID);
        }

        /// <summary>Merges this character's pending-this-cycle kills into the real, permanent save
        /// data, then clears the pending set.</summary>
        internal static void CommitPending(DeathPersistentSaveData saveData, string characterID)
        {
            if (saveData == null || !_pending.TryGetValue(characterID, out HashSet<string> set) || set.Count == 0)
            {
                return;
            }
            foreach (string creatureID in set)
            {
                PrimalSaveMarkers.Add(saveData, Marker(characterID, creatureID));
            }
            set.Clear();
        }

        /// <summary>Wipes this character's pending-this-cycle kills without ever writing them to
        /// the save file -- a cycle that ends in death doesn't bank that cycle's kills.</summary>
        internal static void DiscardPending(string characterID)
        {
            if (_pending.TryGetValue(characterID, out HashSet<string> set))
            {
                set.Clear();
            }
        }

        /// <summary>Whether this character has ever killed this creature type -- permanently
        /// banked OR still pending this cycle (see this class's own header comment for why both).</summary>
        public static bool IsKilled(DeathPersistentSaveData saveData, string characterID, string creatureID)
        {
            if (characterID == null || creatureID == null)
            {
                return false;
            }
            if (PrimalSaveMarkers.Has(saveData, Marker(characterID, creatureID)))
            {
                return true;
            }
            return _pending.TryGetValue(characterID, out HashSet<string> pending) && pending.Contains(creatureID);
        }

        /// <summary>How many DISTINCT creature types this character has ever killed -- permanently
        /// banked, still pending this cycle, or both (deduplicated) -- without needing any fixed
        /// species roster to check against. Used by Ranger's "any 32 unique creatures" and
        /// available to any future tracker with a similar "how many different things has this
        /// character killed" need.</summary>
        public static int UniqueKillCount(DeathPersistentSaveData saveData, string characterID)
        {
            var unique = new HashSet<string>();

            List<string> entries = saveData?.unrecognizedSaveStrings;
            if (entries != null)
            {
                string prefix = PrimalSaveMarkers.Prefix(SaveMarkerKey, characterID);
                foreach (string entry in entries)
                {
                    if (entry != null && entry.StartsWith(prefix))
                    {
                        unique.Add(entry.Substring(prefix.Length));
                    }
                }
            }

            if (_pending.TryGetValue(characterID, out HashSet<string> pending))
            {
                foreach (string creatureID in pending)
                {
                    unique.Add(creatureID);
                }
            }

            return unique.Count;
        }
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.Die))]
    internal static class PrimalKillDieHook
    {
        private static void Postfix(Creature __instance)
        {
            if (__instance == null || !__instance.dead)
            {
                return;
            }
            if (PrimalKillTracker.Recorded.TryGetValue(__instance, out _))
            {
                return;
            }

            AbstractCreature killer = PrimalKillTracker.KillTagField?.GetValue(__instance) as AbstractCreature;
            if (killer == null)
            {
                int? tick = __instance.abstractCreature?.world?.game?.clock;
                if (tick.HasValue && PrimalKillTracker.TryGetRecentHandler(__instance, tick.Value, out AbstractCreature handler))
                {
                    killer = handler;
                }
            }

            Player player = killer?.realizedCreature as Player;
            if (!GimmickGate.TryGetCharacter(player, out string characterID))
            {
                return;
            }

            // Story-mode only: Arena/Sandbox has no DeathPersistentSaveData to eventually persist
            // into, and PrimalSlot's whole design is story-mode-only anyway.
            if (GimmickGate.GameFor(player)?.IsStorySession != true)
            {
                return;
            }

            CreatureTemplate.Type type = __instance.abstractCreature?.creatureTemplate?.type;
            if (type?.value == null)
            {
                return;
            }

            PrimalKillTracker.Recorded.Add(__instance, null);
            PrimalKillTracker.RecordPendingKill(characterID, type.value);
        }
    }

    /// <summary>Patched on the base PhysicalObject.Grabbed rather than Creature's own (of the 10
    /// files across the whole decompiled source that override Grabbed, this reaches every one that
    /// matters via base.Grabbed(grasp) calls -- see the original trace in CreatureKillListGimmick
    /// .cs's git history for the specific files checked).</summary>
    [HarmonyPatch(typeof(PhysicalObject), nameof(PhysicalObject.Grabbed))]
    internal static class PrimalKillHandlerHook
    {
        private static void Postfix(PhysicalObject __instance, Creature.Grasp grasp)
        {
            if (!(__instance is Creature creature) || !(grasp?.grabber is Player player))
            {
                return;
            }

            if (!GimmickGate.TryGetCharacter(player, out _))
            {
                return;
            }

            CreatureTemplate.Type type = creature.abstractCreature?.creatureTemplate?.type;
            if (type?.value == null)
            {
                return;
            }

            RainWorldGame game = GimmickGate.GameFor(player);
            if (game == null)
            {
                return;
            }

            PrimalKillTracker.RecordHandler(creature, player.abstractCreature, game.clock);
        }
    }

    /// <summary>Prefix (not Postfix) on RainWorldGame.Win for the same reason
    /// CreatureKillListGimmick.cs's own original version already documented at length: Win's own
    /// body synchronously triggers the real save-to-disk write before returning, so a Postfix here
    /// would bank kills one save too late.</summary>
    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.Win))]
    internal static class PrimalKillCommitHook
    {
        private static void Prefix(RainWorldGame __instance)
        {
            if (__instance?.Players == null || !__instance.IsStorySession || !PrimalSlot.IsActive(__instance.rainWorld))
            {
                return;
            }

            DeathPersistentSaveData saveData = __instance.GetStorySession?.saveState?.deathPersistentSaveData;
            if (saveData == null)
            {
                return;
            }

            foreach (AbstractCreature abstractPlayer in __instance.Players)
            {
                if (!(abstractPlayer?.state is PlayerState playerState))
                {
                    continue;
                }
                string characterID = playerState.slugcatCharacter?.value;
                if (characterID != null)
                {
                    PrimalKillTracker.CommitPending(saveData, characterID);
                }
            }
        }
    }

    /// <summary>Postfix on RainWorldGame.GameOver -- every one of its 4 call sites (confirmed via
    /// decompilation) is a real character death. Discards, does not persist, that character's
    /// pending-this-cycle kills.</summary>
    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.GameOver))]
    internal static class PrimalKillDiscardHook
    {
        private static void Postfix(RainWorldGame __instance)
        {
            if (__instance?.Players == null)
            {
                return;
            }

            foreach (AbstractCreature abstractPlayer in __instance.Players)
            {
                if (!(abstractPlayer?.state is PlayerState playerState) || !playerState.dead)
                {
                    continue;
                }
                string characterID = playerState.slugcatCharacter?.value;
                if (characterID != null)
                {
                    PrimalKillTracker.DiscardPending(characterID);
                }
            }
        }
    }
}
