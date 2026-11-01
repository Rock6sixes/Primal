using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Flytrap's own "feed a live vulture to a pole plant" need. Confirmed via the user's own
    /// gameplay knowledge (the standard technique: anger the PoleMimic into its active/awake
    /// state, then have a Vulture pass over it so its own ambush-grab logic catches it) and
    /// decompiling PoleMimic.cs to find the real hook: PoleMimic.Collide(PhysicalObject
    /// otherObject, int myChunk, int otherChunk) is the exact method containing the ambush-grab
    /// condition (`base.Consious &amp;&amp; WakeUp > 0.3f &amp;&amp; ... Grab(otherObject, 0,
    /// otherChunk, ...)`) -- "Consious/WakeUp > 0.3f" is the real state behind "angered/active"
    /// the user described.
    ///
    /// No explicit prey-killing method was found on PoleMimic itself (unlike BigEel.Swallow or
    /// ZapCoil's own direct Creature.Die() call) -- it appears to just HOLD what it grabs via the
    /// same Grab()/tentacle-rope mechanism as any other creature-grab, with no separate "digest"
    /// step visible in this class. This tracker therefore treats the GRAB ITSELF (confirmed
    /// successful via a before/after check on `grasps[0]`) as "fed", not a subsequent kill --
    /// matching "feed X to Y" more literally as "gave it something to hold", and reads "survive
    /// the encounter" as "the player is not currently dead at the moment the grab lands" (the same
    /// live-encounter framing OfferingTracker's own "survive" clause uses).
    /// </summary>
    internal static class SurvivorPoleMimicPredation
    {
        public static event System.Action<PoleMimic, Vulture> OnLiveVultureGrabbed;

        internal static bool HasSubscribers => OnLiveVultureGrabbed != null;

        internal static void Raise(PoleMimic mimic, Vulture vulture) => OnLiveVultureGrabbed?.Invoke(mimic, vulture);
    }

    [HarmonyPatch(typeof(PoleMimic), nameof(PoleMimic.Collide))]
    internal static class SurvivorPoleMimicGrabHook
    {
        private static void Postfix(PoleMimic __instance, PhysicalObject otherObject)
        {
            if (!SurvivorPoleMimicPredation.HasSubscribers || !(otherObject is Vulture vulture) || vulture.dead)
            {
                return;
            }

            // Collide can run without resulting in a grab at all (several conditions gate the
            // real Grab() call inside it) -- checking grasps[0] afterward confirms THIS call is
            // what actually caught the vulture, not some earlier or unrelated grab.
            if (__instance.grasps == null || __instance.grasps[0]?.grabbed != vulture)
            {
                return;
            }

            RainWorld rainWorld = __instance.abstractCreature?.world?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            SurvivorPoleMimicPredation.Raise(__instance, vulture);
        }
    }
}
