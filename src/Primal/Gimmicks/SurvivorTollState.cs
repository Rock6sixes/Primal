using System.Collections.Generic;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared mechanism #5 from COMMUNITY_CHALLENGES.md: Scavenger toll/reputation/payment state --
    /// needed by Heist, Manslaughter, Elder, Usurper, and Criminal. All five turned out to share one
    /// class, ScavengerOutpost (confirmed via decompilation, not guessed), which already tracks
    /// everything these challenges need to read:
    ///
    ///  - `team` (List&lt;Scavenger&gt;) -- the outpost's own guard roster, confirmed public.
    ///  - `worldOutpost.feePayed` (int) -- vanilla's own running payment total for this outpost.
    ///    Confirmed this is the real "has paid enough" signal, not guessed: ScavengerOutpost
    ///    .PlayerTracker.AllowedToPass (vanilla's own gate for whether a player may cross) reads
    ///    exactly `outpost.worldOutpost.feePayed > 9`, so this class reuses that same threshold
    ///    rather than inventing its own.
    ///  - `playerTrackers` (List&lt;PlayerTracker&gt;), each with a `killOrder` bool -- vanilla's
    ///    own per-player hostility flag. Set (when not blocked by a grace-period check) from
    ///    ScavengerReportTransgression(Player), which SurvivorOutpostTransgressionHook (this
    ///    file, below) patches directly rather than trying to infer hostility from anything
    ///    indirect.
    ///
    /// Not yet covered by this pass: distinguishing "stole a pearl without paying" specifically
    /// from any other transgression (attacking a scavenger also calls the same method, as far as
    /// this file's own research went) -- ScavengerReportTransgression's call sites weren't traced
    /// exhaustively. OnOutpostTransgression fires for any transgression the game itself reports;
    /// a future per-challenge tracker that needs to tell "stole" apart from "attacked" will need
    /// that follow-up research first.
    /// </summary>
    internal static class SurvivorTollState
    {
        /// <summary>The ScavengerOutpost governing this room, if it's a toll room at all. Room
        /// .updateList (confirmed public property) is the same list every UpdatableAndDeletable in
        /// the room lives in, ScavengerOutpost included.</summary>
        public static ScavengerOutpost OutpostIn(Room room)
        {
            List<UpdatableAndDeletable> updateList = room?.updateList;
            if (updateList == null)
            {
                return null;
            }

            foreach (UpdatableAndDeletable obj in updateList)
            {
                if (obj is ScavengerOutpost outpost)
                {
                    return outpost;
                }
            }
            return null;
        }

        /// <summary>The outpost's own guard roster (Scavenger, the realized creature type) --
        /// Manslaughter/Elder both need to enumerate every guard, including ones replaced mid-
        /// attempt after an original dies (this returns the LIVE list, so a replacement scavenger
        /// added to `team` shows up automatically on the next call, no extra tracking needed).</summary>
        public static IReadOnlyList<Scavenger> GuardsOf(ScavengerOutpost outpost)
        {
            return (IReadOnlyList<Scavenger>)outpost?.team ?? System.Array.Empty<Scavenger>();
        }

        /// <summary>Vanilla's own "has paid enough to be let through" check (see this class's own
        /// header comment for where the `&gt; 9` threshold comes from) -- Heist and Criminal both
        /// need "did NOT pay", i.e. the negation of this.</summary>
        public static bool HasPaidToll(ScavengerOutpost outpost)
        {
            return outpost?.worldOutpost != null && outpost.worldOutpost.feePayed > 9;
        }

        /// <summary>Whether this outpost currently has a standing kill order against this specific
        /// player -- vanilla's own per-player hostility flag, confirmed distinct from a global
        /// "outpost is hostile" state (each player tracked separately, matching how reputation is
        /// genuinely per-player in vanilla, e.g. co-op).</summary>
        public static bool IsHostileToPlayer(ScavengerOutpost outpost, Player player)
        {
            if (outpost?.playerTrackers == null || player?.abstractCreature == null)
            {
                return false;
            }

            foreach (ScavengerOutpost.PlayerTracker tracker in outpost.playerTrackers)
            {
                if (tracker.player == player.abstractCreature)
                {
                    return tracker.killOrder;
                }
            }
            return false;
        }

        /// <summary>Fires whenever an outpost reports a transgression against a specific player --
        /// a Postfix on ScavengerOutpost.ScavengerReportTransgression (SurvivorOutpostTransgression
        /// Hook, below), checked afterwards against IsHostileToPlayer rather than assuming the call
        /// always takes effect, since the original method silently no-ops during the player's own
        /// scavengerImmunity grace period. Handed `becameHostile` precomputed so subscribers don't
        /// each have to re-derive it.</summary>
        public static event System.Action<ScavengerOutpost, Player, bool> OnOutpostTransgression;

        internal static bool HasOutpostTransgressionSubscribers => OnOutpostTransgression != null;

        internal static void RaiseOutpostTransgression(ScavengerOutpost outpost, Player player, bool becameHostile)
            => OnOutpostTransgression?.Invoke(outpost, player, becameHostile);
    }

    /// <summary>Top-level patch class -- see SurvivorEvents.cs's header comment for why every patch
    /// in this Survivor-mechanisms batch is a top-level class rather than nested inside the static
    /// helper it serves.</summary>
    [HarmonyPatch(typeof(ScavengerOutpost), nameof(ScavengerOutpost.ScavengerReportTransgression))]
    internal static class SurvivorOutpostTransgressionHook
    {
        private static void Postfix(ScavengerOutpost __instance, Player player)
        {
            if (!SurvivorTollState.HasOutpostTransgressionSubscribers || __instance == null || player == null)
            {
                return;
            }
            if (!GimmickGate.TryGetCharacter(player, out _))
            {
                // Cheap-ish gate (one PrimalSlot check via TryGetCharacter) -- transgressions are
                // rare events, not a hot path, so this doesn't need GimmickGate's Player-overload
                // gymnastics, just the same "only on a reserved slot" rule as everything else in
                // this file.
                return;
            }
            SurvivorTollState.RaiseOutpostTransgression(__instance, player, SurvivorTollState.IsHostileToPlayer(__instance, player));
        }
    }
}
