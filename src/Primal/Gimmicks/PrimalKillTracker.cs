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

        // Added 2026-09-16 for Feast ("kill AND eat the SAME Red Centipede", a lineage-tracked
        // individual, not just "killed one and ate a different one"). Every OTHER record in this
        // class is per-TYPE (a character either has or hasn't ever killed a CreatureTemplate.Type),
        // which can't answer "was THIS specific creature instance's death credited to me" --
        // that's genuinely different information, so it gets its own table rather than overloading
        // _pending's own string-keyed sets with instance numbers. Keyed on EntityID.number (the
        // plain int), per the same Equals/GetHashCode gotcha SurvivorCreatureTracking's own header
        // comment documents -- never on EntityID itself. Session-only, in-memory, unbounded but
        // trivially small in practice (bounded by total lifetime kill count of one save's
        // characters, not by anything this mod multiplies).
        private static readonly Dictionary<string, HashSet<int>> _instanceKills = new Dictionary<string, HashSet<int>>();

        // In-memory only, per characterID -- never itself persisted. See this class's own header
        // comment for why IsKilled/UniqueKillCount both fold this in alongside the permanent
        // record, not just the "pending vs. committed" story CreatureKillListGimmick's own header
        // comment already covers.
        private static readonly Dictionary<string, HashSet<string>> _pending = new Dictionary<string, HashSet<string>>();

        // Exactly what CommitPending just merged into permanent storage, for THIS character's
        // most recent successful cycle end -- overwritten (not merged) at the start of every
        // CommitPending call, so it never holds anything older than the cycle that just ended.
        // See WasKilledThisCycle/ThisCycleKillPointTotal for why this exists as a separate table
        // from _pending rather than just reading _pending directly: CommitPending clears _pending
        // as part of committing, so anything checking "what did this cycle kill" from a handler
        // that runs AFTER commit (which PrimalKillCommitHook's own [HarmonyPriority(Priority
        // .First)] guarantees happens before every other same-method Prefix -- see that hook's own
        // comment) would find _pending already empty. This table is what survives that.
        private static readonly Dictionary<string, HashSet<string>> _lastCommitted = new Dictionary<string, HashSet<string>>();

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

        /// <summary>Added 2026-09-17, pulled up after this exact resolution (KillTagField first,
        /// falling back to TryGetRecentHandler) turned out to be independently duplicated
        /// verbatim in three separate Creature.Die Postfixes -- PrimalKillDieHook (this file),
        /// SurvivorTollRosterDeathHook (Manslaughter's "no kill credit" check), and
        /// TriplopiaPoleMimicDeathHook (Triplopia's same-den kill tracking). Same shared-mechanism
        /// reasoning as everything else already pulled into this class: "who gets credit for this
        /// death" is one question every consumer answers identically, so it belongs here once, not
        /// re-derived per caller. Null if neither vanilla's own killTag nor this class's own
        /// proximity-based handler attribution can name anyone.</summary>
        public static AbstractCreature ResolveKiller(Creature creature)
        {
            AbstractCreature killer = KillTagField?.GetValue(creature) as AbstractCreature;
            if (killer != null)
            {
                return killer;
            }

            int? tick = creature?.abstractCreature?.world?.game?.clock;
            if (tick.HasValue && TryGetRecentHandler(creature, tick.Value, out AbstractCreature handler))
            {
                return handler;
            }
            return null;
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

        /// <summary>Records that this character was credited with killing this SPECIFIC creature
        /// instance (by EntityID.number) -- see this class's own header comment on _instanceKills
        /// for why this is separate from the per-type pending/permanent record above. Not gated on
        /// cycle success/failure the way _pending is -- a "kill and eat the same creature" check
        /// only ever cares about "did I kill this one", regardless of whether the cycle it happened
        /// in was later survived, since the corpse itself is what gets eaten, cycle-outcome-
        /// independent.</summary>
        internal static void RecordInstanceKill(string characterID, int instanceNumber)
        {
            if (!_instanceKills.TryGetValue(characterID, out HashSet<int> set))
            {
                set = new HashSet<int>();
                _instanceKills[characterID] = set;
            }
            set.Add(instanceNumber);
        }

        /// <summary>Whether this character was ever credited with killing this specific creature
        /// instance -- Feast's own "kill AND eat the SAME Red Centipede" need.</summary>
        public static bool WasInstanceKilledBy(string characterID, int instanceNumber)
        {
            return characterID != null
                && _instanceKills.TryGetValue(characterID, out HashSet<int> set)
                && set.Contains(instanceNumber);
        }

        /// <summary>Added 2026-09-17 for Saint ("without killing anything") -- unlike everything
        /// else in this class, which is a passive record read back later, Saint needs to know the
        /// INSTANT a kill happens (any kill permanently disqualifies that character's current
        /// attempt). Fired from PrimalKillDieHook's own Postfix, right alongside the existing
        /// per-type/per-instance recording it already does -- same raw signal, just also exposed
        /// as a live event for a consumer that cares about the moment rather than the record.
        /// Gained the `Player` parameter later the same day for UniqueKillsRequirement, which needs
        /// a live Player to report completion through (ReportCompletion) -- PrimalKillDieHook
        /// already resolves one right before raising this, it just wasn't threaded through before.
        /// Saint's own subscriber ignores it.</summary>
        public static event System.Action<string, Player> OnKillCredited;

        internal static void RaiseKillCredited(string characterID, Player player) => OnKillCredited?.Invoke(characterID, player);

        /// <summary>Merges this character's pending-this-cycle kills into the real, permanent save
        /// data, then clears the pending set. Always resets _lastCommitted for this character
        /// first, even if there's nothing to commit -- so a cycle with zero kills correctly reads
        /// as "nothing killed this cycle" rather than leaking a STALE snapshot forward from
        /// whichever earlier cycle last actually committed something.</summary>
        internal static void CommitPending(DeathPersistentSaveData saveData, string characterID)
        {
            var committedThisCall = new HashSet<string>();
            _lastCommitted[characterID] = committedThisCall;

            // 2026-11-03: logging added after a real-save audit found zero PRIMAL_KILLLIST
            // markers ever persisted for a character with 400+ cycles of actual play (sav666667,
            // Artificer) -- there was previously no way to tell from the log whether this method
            // even runs, whether saveData is unexpectedly null, or whether _pending is
            // unexpectedly already empty by the time it's called. Logs every call, not just the
            // "something to commit" branch, specifically so a run with zero kills this cycle is
            // still distinguishable from this method never being reached at all.
            if (saveData == null)
            {
                Plugin.Log?.LogInfo($"[PrimalKillTracker] {characterID}: CommitPending called with null saveData -- nothing can be persisted this cycle.");
                return;
            }
            if (!_pending.TryGetValue(characterID, out HashSet<string> set) || set.Count == 0)
            {
                Plugin.Log?.LogInfo($"[PrimalKillTracker] {characterID}: CommitPending called, nothing pending this cycle.");
                return;
            }
            foreach (string creatureID in set)
            {
                PrimalSaveMarkers.Add(saveData, Marker(characterID, creatureID));
                committedThisCall.Add(creatureID);
            }
            Plugin.Log?.LogInfo($"[PrimalKillTracker] {characterID}: committed {committedThisCall.Count} kill(s) this cycle to the save file: {string.Join(", ", committedThisCall)}");
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

        /// <summary>Whether this creature type was among the kills committed at this character's
        /// MOST RECENT successful cycle end -- i.e. killed specifically THIS cycle, not merely
        /// "ever" the way IsKilled answers. Only meaningful when checked from a
        /// SurvivorEvents.OnSuccessfulHibernation handler -- see PrimalKillCommitHook's own
        /// [HarmonyPriority(Priority.First)] for why _lastCommitted is guaranteed already
        /// populated for this cycle by the time any other Win-Prefix subscriber runs.</summary>
        public static bool WasKilledThisCycle(string characterID, string creatureID)
        {
            return characterID != null
                && creatureID != null
                && _lastCommitted.TryGetValue(characterID, out HashSet<string> set)
                && set.Contains(creatureID);
        }

        /// <summary>Sum of SurvivorVanillaProgress.KillPointValue across every DISTINCT creature
        /// type killed at this character's most recent successful cycle end (see
        /// WasKilledThisCycle's own doc comment on when this is valid to call) -- Massacre's own
        /// "kill points in one cycle" need. Each type counts once, matching how this whole tracker
        /// only ever records "did you kill at least one of these", never a running per-kill tally
        /// -- a real limitation if the community rules for a future challenge ever needed to count
        /// repeat kills of the same species, which this can't currently answer.</summary>
        public static int ThisCycleKillPointTotal(string characterID)
        {
            if (characterID == null || !_lastCommitted.TryGetValue(characterID, out HashSet<string> set))
            {
                return 0;
            }
            int total = 0;
            foreach (string creatureID in set)
            {
                total += SurvivorVanillaProgress.KillPointValue(creatureID) ?? 0;
            }
            return total;
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

            AbstractCreature killer = PrimalKillTracker.ResolveKiller(__instance);

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
            PrimalKillTracker.RecordInstanceKill(characterID, __instance.abstractCreature.ID.number);
            PrimalKillTracker.RaiseKillCredited(characterID, player);
            // 2026-11-03: see CommitPending's own new logging for why -- this confirms kills are
            // actually reaching the in-memory pending set at all, the other half of the picture
            // (whether CommitPending ever successfully moves them from there into the save file).
            Plugin.Log?.LogInfo($"[PrimalKillTracker] {characterID}: recorded pending kill of {type.value}");
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
    /// would bank kills one save too late.
    ///
    /// [HarmonyPriority(Priority.First)] added 2026-09-16: SurvivorEvents' own Win Prefix
    /// (SurvivorWinHook) is a SEPARATE Prefix on this same method, and Harmony gives no ordering
    /// guarantee between multiple Prefixes on one method unless one explicitly claims priority --
    /// without this, a this-cycle-scoped challenge check (Foe, Massacre) running from
    /// SurvivorEvents.OnSuccessfulHibernation could read this cycle's kills before they'd actually
    /// been committed, or after _pending had already been cleared by a commit it couldn't see
    /// happen first. Priority.First forces this Prefix to run before any other Prefix on Win that
    /// doesn't ALSO claim a priority (every other one in this codebase, as of this comment,
    /// doesn't) -- see WasKilledThisCycle/ThisCycleKillPointTotal for the query methods this
    /// ordering guarantee actually makes safe to use.</summary>
    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.Win))]
    internal static class PrimalKillCommitHook
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(RainWorldGame __instance)
        {
            // 2026-11-03: this whole method firing at all was previously unverifiable from the
            // log -- see CommitPending's own new logging for the full context (a real save audit
            // found zero kills ever persisted across 400+ played cycles). Logs every one of this
            // method's own early-out gates too, not just CommitPending's, since a gate failing
            // here (e.g. IsStorySession false, or no PrimalSlot active) would ALSO explain zero
            // commits ever, with CommitPending itself never even being reached to log anything.
            if (__instance?.Players == null)
            {
                Plugin.Log?.LogInfo("[PrimalKillTracker] RainWorldGame.Win fired but __instance.Players was null -- skipping commit entirely.");
                return;
            }
            if (!__instance.IsStorySession || !PrimalSlot.IsActive(__instance.rainWorld))
            {
                Plugin.Log?.LogInfo($"[PrimalKillTracker] RainWorldGame.Win fired off the Primal slot or outside story mode (IsStorySession={__instance.IsStorySession}) -- skipping commit entirely.");
                return;
            }

            DeathPersistentSaveData saveData = __instance.GetStorySession?.saveState?.deathPersistentSaveData;
            if (saveData == null)
            {
                Plugin.Log?.LogInfo("[PrimalKillTracker] RainWorldGame.Win fired but GetStorySession/saveState/deathPersistentSaveData resolved null -- skipping commit entirely.");
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
