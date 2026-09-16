using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Offering's own "feed it to the albino Leviathan" need. The taming/reputation-increase
    /// system the user pointed at (ScavengerAI/LizardAI/CicadaAI's own `EventID.ItemOffering`
    /// handling, fed by `SocialEventRecognizer.ItemOffered`) does NOT apply here -- confirmed via
    /// decompiling BigEelAI.cs that it does not implement `IReactToSocialEvents` at all (unlike
    /// LizardAI/CicadaAI/ScavengerAI, which all do), so `SocialEventRecognizer.SocialEvent`'s own
    /// dispatch loop (which filters on exactly that interface) never reaches a BigEel regardless of
    /// what gets offered to it. The real mechanism is `BigEel.Swallow()` -- confirmed via
    /// decompiling BigEel.cs to be the one method that actually consumes whatever the Leviathan has
    /// caught: it iterates its own `clampedObjects` (a public `List&lt;ClampedObject&gt;`, each
    /// with a public `chunk` -- a `BodyChunk` whose own `owner` is the caught `PhysicalObject`) and
    /// calls `.Destroy()` on each one, then clears the list -- entirely independent of
    /// Violence/killTag/SocialEventRecognizer.
    ///
    /// A Prefix (not Postfix) on Swallow, since its own body immediately Destroys every clamped
    /// object and clears the list before returning -- a Postfix would find nothing left to
    /// inspect.
    /// </summary>
    internal static class SurvivorPredation
    {
        /// <summary>Fires once per creature a BigEel (Leviathan) is about to swallow -- see this
        /// class's own header comment for why Swallow, not any social/taming event, is the real
        /// hook. Named for exactly the one predator actually traced here (BigEel), not a general
        /// "any predator eats anything" claim.</summary>
        public static event System.Action<BigEel, Creature> OnLeviathanSwallow;

        internal static bool HasLeviathanSwallowSubscribers => OnLeviathanSwallow != null;

        internal static void RaiseLeviathanSwallow(BigEel eel, Creature prey) => OnLeviathanSwallow?.Invoke(eel, prey);
    }

    [HarmonyPatch(typeof(BigEel), nameof(BigEel.Swallow))]
    internal static class SurvivorLeviathanSwallowHook
    {
        private static void Prefix(BigEel __instance)
        {
            if (!SurvivorPredation.HasLeviathanSwallowSubscribers || __instance?.clampedObjects == null)
            {
                return;
            }

            RainWorld rainWorld = __instance.abstractCreature?.world?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            foreach (BigEel.ClampedObject clamped in __instance.clampedObjects)
            {
                if (clamped?.chunk?.owner is Creature creature && !creature.dead)
                {
                    SurvivorPredation.RaiseLeviathanSwallow(__instance, creature);
                }
            }
        }
    }
}
