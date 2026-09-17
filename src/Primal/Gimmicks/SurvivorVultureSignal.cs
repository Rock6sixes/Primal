using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Added 2026-09-17 for Broadcast, per the user's own trace of the real mechanism: "when
    /// thrown, the grub visually shows a yellow laser as it begins a timer. When the timer expires,
    /// the game checks if the grub has open air above it -- if it does, the laser will turn green
    /// and a vulture will enter the room. After this, or if the grub had no air above it, the laser
    /// will turn red." Confirmed via decompiling VultureGrub.cs against exactly that description:
    ///
    ///  - `AttemptCallVulture()` is the method that actually finds and summons a Vulture/
    ///    KingVulture/MirosVulture once the countdown (`singalCounter`) has run down far enough
    ///    with a valid sky path found (`skyPosition.HasValue`) -- it sets `vultureCalled = true`
    ///    ONLY on the branch that actually finds a creature and a valid path to send it through
    ///    (every other branch returns early without touching it). This is the "laser turns green"
    ///    moment -- OnSignalSucceeded, below, is a Postfix on this exact method checking that flag.
    ///  - The "laser turns red" failure case is set inline in Act() (called every tick from
    ///    Update): `if (singalCounter < 5 && !vultureCalled) { callingMode = -1; ... }` -- the
    ///    timer ran out with no vulture ever called. Since this isn't its own method, OnSignalFailed
    ///    is a Postfix on VultureGrub.Update(bool) instead, comparing `callingMode` against a
    ///    per-instance "already reported" guard so the failure fires once per attempt, not every
    ///    tick `callingMode` stays at -1. That guard is cleared by a third Postfix, on
    ///    InitiateSignal() (the method that resets `singalCounter`/`callingMode` to start a fresh
    ///    attempt), the natural "a new attempt has begun" moment.
    ///
    /// This is deliberately a general "any VultureGrub's signal, anywhere" mechanism, not scoped to
    /// Sky Islands/Communications Array specifically -- narrowing to the right region/room is each
    /// consumer's own job, the same way SurvivorEvents.OnPlayerEnteredRoom fires everywhere and
    /// every tracker filters by region itself.
    /// </summary>
    internal static class SurvivorVultureSignal
    {
        public static event System.Action<VultureGrub> OnSignalSucceeded;
        public static event System.Action<VultureGrub> OnSignalFailed;

        internal static bool HasSucceededSubscribers => OnSignalSucceeded != null;
        internal static bool HasFailedSubscribers => OnSignalFailed != null;

        internal static void RaiseSucceeded(VultureGrub grub) => OnSignalSucceeded?.Invoke(grub);
        internal static void RaiseFailed(VultureGrub grub) => OnSignalFailed?.Invoke(grub);

        // Per-instance -- has this grub's CURRENT signal attempt already reported failure, so a
        // continuously-true callingMode==-1 state doesn't refire every tick until InitiateSignal
        // starts a fresh attempt.
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<VultureGrub, object> _failureReported =
            new System.Runtime.CompilerServices.ConditionalWeakTable<VultureGrub, object>();

        internal static bool AlreadyReportedFailure(VultureGrub grub) => _failureReported.TryGetValue(grub, out _);

        internal static void MarkFailureReported(VultureGrub grub)
        {
            if (!_failureReported.TryGetValue(grub, out _))
            {
                _failureReported.Add(grub, null);
            }
        }

        internal static void ClearFailureReported(VultureGrub grub) => _failureReported.Remove(grub);

        /// <summary>Added 2026-09-17 for Broadcast's own "activate a Vulture Grub" step. Fires the
        /// instant a player throws a VultureGrub they were holding -- Player.ThrowObject(int
        /// grasp, bool eu) is confirmed via decompilation to be the one call site that invokes
        /// VultureGrub.InitiateSignalCountDown() (the method that starts the yellow-laser
        /// countdown), so this is the real "thrown" moment, not an approximation off release/drop
        /// in general (which wouldn't distinguish a deliberate throw from simply letting go).</summary>
        public static event System.Action<Player, VultureGrub> OnGrubThrown;

        internal static bool HasGrubThrownSubscribers => OnGrubThrown != null;

        internal static void RaiseGrubThrown(Player player, VultureGrub grub) => OnGrubThrown?.Invoke(player, grub);
    }

    /// <summary>A Prefix, not a Postfix -- by the time ThrowObject returns, the object has already
    /// left the grasp being thrown from, so this reads base.grasps[grasp] (the same field the real
    /// method itself reads first) before that happens.</summary>
    [HarmonyPatch(typeof(Player), nameof(Player.ThrowObject))]
    internal static class SurvivorVultureGrubThrownHook
    {
        private static void Prefix(Player __instance, int grasp)
        {
            if (!SurvivorVultureSignal.HasGrubThrownSubscribers || __instance?.grasps == null
                || grasp < 0 || grasp >= __instance.grasps.Length)
            {
                return;
            }

            if (!(__instance.grasps[grasp]?.grabbed is VultureGrub grub))
            {
                return;
            }

            if (!GimmickGate.TryGetCharacter(__instance, out _))
            {
                return;
            }

            SurvivorVultureSignal.RaiseGrubThrown(__instance, grub);
        }
    }

    [HarmonyPatch(typeof(VultureGrub), nameof(VultureGrub.AttemptCallVulture))]
    internal static class SurvivorVultureSignalSuccessHook
    {
        private static void Postfix(VultureGrub __instance)
        {
            if (__instance == null || !__instance.vultureCalled || !SurvivorVultureSignal.HasSucceededSubscribers)
            {
                return;
            }

            RainWorld rainWorld = __instance.abstractCreature?.world?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            SurvivorVultureSignal.RaiseSucceeded(__instance);
        }
    }

    [HarmonyPatch(typeof(VultureGrub), nameof(VultureGrub.Update))]
    internal static class SurvivorVultureSignalFailureHook
    {
        private static void Postfix(VultureGrub __instance)
        {
            if (__instance == null || __instance.callingMode != -1 || !SurvivorVultureSignal.HasFailedSubscribers
                || SurvivorVultureSignal.AlreadyReportedFailure(__instance))
            {
                return;
            }

            RainWorld rainWorld = __instance.abstractCreature?.world?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            SurvivorVultureSignal.MarkFailureReported(__instance);
            SurvivorVultureSignal.RaiseFailed(__instance);
        }
    }

    [HarmonyPatch(typeof(VultureGrub), nameof(VultureGrub.InitiateSignal))]
    internal static class SurvivorVultureSignalResetHook
    {
        private static void Postfix(VultureGrub __instance)
        {
            if (__instance != null)
            {
                SurvivorVultureSignal.ClearFailureReported(__instance);
            }
        }
    }
}
