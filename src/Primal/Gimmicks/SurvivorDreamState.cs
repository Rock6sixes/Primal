using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Despicable's own "Trigger the Stolen Enlightenment dream on the starting cycle" need.
    /// Confirmed via decompiling DreamsState.cs: DreamsState.DreamID.MoonThief is the real ID --
    /// RainWorldGame.Win's own body (`GetStorySession.lastEverMetMoon`/`SLOracleState.neuronsLeft`
    /// branch) picks it specifically when the player has taken 5+ of Moon's neurons, exactly the
    /// "stole from Moon" narrative "Stolen Enlightenment" describes, confirmed by its sibling ID
    /// (`MoonFriend`, chosen when enough neurons are left) rather than guessed from the name alone.
    /// `SaveState.dreamsState.eventDream` (both plain public fields) holds whichever dream vanilla
    /// decided on for the cycle that just ended.
    ///
    /// **A real timing gotcha, specific to this event alone in this codebase.** Every OTHER
    /// SurvivorEvents hook on RainWorldGame.Win is a Prefix (to run before Win's own synchronous
    /// disk-save -- see SurvivorEvents.OnSuccessfulHibernation's own doc comment). This one has to
    /// be a POSTFIX instead: confirmed via decompilation that both `dreamsState.EndOfCycleProgress`
    /// (which finalizes `eventDream` for the ending cycle) and `SaveState.SessionEnded` (which,
    /// for the survived path, calls `RainCycleTick` -- the same method that increments
    /// `SaveState.cycleNumber`) run INSIDE Win's own body, with `EndOfCycleProgress` running
    /// FIRST. A Prefix would read `eventDream` before it's updated for this cycle at all; a
    /// Postfix reads it correctly, but pays a real cost for that: by Postfix time,
    /// `cycleNumber` has ALREADY been incremented past the cycle that just ended. Consumers that
    /// need "was this the starting cycle" must therefore check `cycleNumber == 1`, not `== 0` the
    /// way every Prefix-timed check elsewhere in this mod (MotherTracker, MycologistTracker) does
    /// -- this is called out explicitly in DespicableTracker's own doc comment too, since getting
    /// it backwards here is an easy, silent mistake.
    /// </summary>
    internal static class SurvivorDreamState
    {
        /// <summary>Fires once per assigned-slot player at the end of every successful cycle,
        /// handing back whichever dream vanilla decided on (possibly null -- most cycles have no
        /// dream at all) and SaveState.cycleNumber AS ALREADY INCREMENTED past the cycle that just
        /// ended (see this class's own header comment).</summary>
        public static event System.Action<Player, string, DreamsState.DreamID, int> OnCycleEndDream;

        internal static bool HasSubscribers => OnCycleEndDream != null;

        internal static void Raise(Player player, string characterID, DreamsState.DreamID dream, int cycleNumberAfterIncrement)
            => OnCycleEndDream?.Invoke(player, characterID, dream, cycleNumberAfterIncrement);
    }

    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.Win))]
    internal static class SurvivorDreamStateHook
    {
        private static void Postfix(RainWorldGame __instance)
        {
            if (!SurvivorDreamState.HasSubscribers
                || __instance?.Players == null
                || !__instance.IsStorySession
                || !PrimalSlot.IsActive(__instance.rainWorld))
            {
                return;
            }

            SaveState saveState = __instance.GetStorySession?.saveState;
            DreamsState dreamsState = saveState?.dreamsState;
            if (dreamsState == null)
            {
                return;
            }

            int cycleNumberAfterIncrement = saveState.cycleNumber;

            foreach (AbstractCreature abstractPlayer in __instance.Players)
            {
                if (!(abstractPlayer?.state is PlayerState playerState) || !(abstractPlayer.realizedCreature is Player player))
                {
                    continue;
                }

                string characterID = playerState.slugcatCharacter?.value;
                if (characterID != null)
                {
                    SurvivorDreamState.Raise(player, characterID, dreamsState.eventDream, cycleNumberAfterIncrement);
                }
            }
        }
    }
}
