using System.Collections.Generic;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Manslaughter's own specific need on top of SurvivorTollState: "cause every Scavenger ever
    /// assigned to this toll to die, without the player getting kill credit for any of them, and
    /// without the outpost ever going hostile." ScavengerOutpost.team (SurvivorTollState.GuardsOf)
    /// only ever holds CURRENTLY-tracked guards -- confirmed via decompiling ScavengerOutpost
    /// .Update: a dead/gone Scavenger is removed from `team` the same tick it stops being trackable
    /// (its own ScavToBeTracked check), and a replacement Scavenger simply gets appended in its
    /// place. That means GuardsOf/team alone can never answer "is everyone who was EVER assigned
    /// here now dead" -- by the time the last one dies, earlier casualties have already fallen out
    /// of the list entirely. This file keeps its own persistent roster instead, keyed on the
    /// outpost's own room name (stable across however many times the room loads/unloads, unlike
    /// the ScavengerOutpost instance itself, which is recreated fresh on every room load).
    ///
    /// The "no kill credit" half reuses PrimalKillTracker's own killTag/handler attribution
    /// (KillTagField, TryGetRecentHandler) -- the exact same signal PrimalKillDieHook already
    /// trusts to decide whether a kill counts as "the player's own" -- applied here per-instance
    /// at the moment a specific guard dies, rather than PrimalKillTracker's own per-species record
    /// (which can't answer "was THIS specific creature's death credited to the player").
    ///
    /// Session-only, in-memory state, matching SurvivorCreatureTracking's own "not persisted across
    /// a save/quit" scoping -- every Manslaughter-shaped attempt completes or fails within a
    /// bounded stretch of play while the game stays open.
    /// </summary>
    internal static class SurvivorTollRoster
    {
        private sealed class RosterState
        {
            public readonly HashSet<int> EverSeen = new HashSet<int>();
            public readonly HashSet<int> ConfirmedDead = new HashSet<int>();
            public bool PlayerCreditedForAGuardKill;
        }

        private static readonly Dictionary<string, RosterState> _rosters = new Dictionary<string, RosterState>();

        /// <summary>The stable identity key this whole file tracks against -- the outpost's own
        /// room name (AbstractRoom.name), not the ScavengerOutpost instance itself. Null if the
        /// outpost (or its room) can't be resolved.</summary>
        public static string RoomKeyFor(ScavengerOutpost outpost) => outpost?.room?.abstractRoom?.name;

        /// <summary>Call whenever a caller has a live ScavengerOutpost reference (this file's own
        /// SurvivorTollRosterObserveHook does this every Update tick) -- records every CURRENTLY-
        /// tracked guard's identity into this outpost's permanent roster and refreshes each one's
        /// dead/alive status. Cheap and safe to call repeatedly (HashSet.Add is a no-op for an
        /// already-seen ID); EntityID.number is used as the key rather than EntityID itself, per
        /// the same Equals/GetHashCode gotcha SurvivorCreatureTracking's own header comment
        /// documents.</summary>
        public static void Observe(ScavengerOutpost outpost)
        {
            string key = RoomKeyFor(outpost);
            if (key == null)
            {
                return;
            }

            RosterState state = GetOrCreate(key);
            foreach (Scavenger scavenger in SurvivorTollState.GuardsOf(outpost))
            {
                if (scavenger?.abstractCreature == null)
                {
                    continue;
                }
                int id = scavenger.abstractCreature.ID.number;
                state.EverSeen.Add(id);
                if (scavenger.abstractCreature.state.dead)
                {
                    state.ConfirmedDead.Add(id);
                }
            }
        }

        /// <summary>Marks this outpost as permanently disqualified for a Manslaughter-shaped
        /// challenge -- the player got kill credit for one of its guards. Stays set until
        /// ResetRoster clears the whole entry.</summary>
        internal static void MarkPlayerCreditedKill(string roomKey)
        {
            if (roomKey != null)
            {
                GetOrCreate(roomKey).PlayerCreditedForAGuardKill = true;
            }
        }

        /// <summary>True if the player has EVER been credited (killTag/handler attribution) with
        /// killing one of this outpost's own guards.</summary>
        public static bool PlayerEverCreditedAGuardKill(string roomKey)
        {
            return roomKey != null && _rosters.TryGetValue(roomKey, out RosterState state) && state.PlayerCreditedForAGuardKill;
        }

        /// <summary>True only if at least one guard was ever recorded at this outpost AND every
        /// single one of them is now confirmed dead (false, not vacuously true, if nothing's been
        /// observed yet -- matches SurvivorCreatureTracking.AllTrackedAlive's own "zero isn't a
        /// meaningful pass" reasoning).</summary>
        public static bool AllEverSeenGuardsDead(string roomKey)
        {
            return roomKey != null
                && _rosters.TryGetValue(roomKey, out RosterState state)
                && state.EverSeen.Count > 0
                && state.ConfirmedDead.Count >= state.EverSeen.Count;
        }

        /// <summary>Drops everything recorded for this outpost -- e.g. starting a fresh attempt
        /// after a failed cycle.</summary>
        public static void ResetRoster(string roomKey)
        {
            if (roomKey != null)
            {
                _rosters.Remove(roomKey);
            }
        }

        private static RosterState GetOrCreate(string roomKey)
        {
            if (!_rosters.TryGetValue(roomKey, out RosterState state))
            {
                state = new RosterState();
                _rosters[roomKey] = state;
            }
            return state;
        }
    }

    /// <summary>Keeps every active outpost's roster fresh -- a Postfix on ScavengerOutpost's own
    /// Update(bool), the same method whose body confirms `team` drops dead/gone guards
    /// (SurvivorTollRoster's own header comment traces this). Gated only on PrimalSlot.IsActive
    /// (no specific character/gimmick check) -- matching SurvivorEvents' own broad-gating
    /// philosophy, since this is shared observation infrastructure, not a specific challenge's own
    /// logic.</summary>
    [HarmonyPatch(typeof(ScavengerOutpost), nameof(ScavengerOutpost.Update))]
    internal static class SurvivorTollRosterObserveHook
    {
        private static void Postfix(ScavengerOutpost __instance)
        {
            RainWorld rainWorld = __instance?.room?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }
            SurvivorTollRoster.Observe(__instance);
            if (SurvivorTollState.HasOutpostTickSubscribers)
            {
                SurvivorTollState.RaiseOutpostTick(__instance);
            }
        }
    }

    /// <summary>Records "the player got kill credit for this specific guard" the instant it
    /// happens -- an independent Postfix on Creature.Die (PrimalKillDieHook already patches this
    /// same method independently for its own, unrelated per-species record; two independent
    /// Postfixes on one method need no ordering between them since neither reads the other's
    /// result). Reuses PrimalKillTracker.ResolveKiller (pulled up 2026-09-17 once this exact
    /// resolution turned out to be duplicated a third time, by TriplopiaPoleMimicDeathHook) rather
    /// than re-deriving attribution.</summary>
    [HarmonyPatch(typeof(Creature), nameof(Creature.Die))]
    internal static class SurvivorTollRosterDeathHook
    {
        private static void Postfix(Creature __instance)
        {
            if (!(__instance is Scavenger scavenger) || !scavenger.dead)
            {
                return;
            }

            ScavengerOutpost outpost = scavenger.AI?.outpostModule?.outpost;
            string roomKey = SurvivorTollRoster.RoomKeyFor(outpost);
            if (roomKey == null)
            {
                return;
            }

            AbstractCreature killer = PrimalKillTracker.ResolveKiller(scavenger);

            if (killer?.realizedCreature is Player player && GimmickGate.TryGetCharacter(player, out _))
            {
                SurvivorTollRoster.MarkPlayerCreditedKill(roomKey);
            }
        }
    }
}
