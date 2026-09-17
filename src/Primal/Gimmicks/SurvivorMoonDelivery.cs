using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Added 2026-09-17 for Saint -- specifically the Survivor/manual half of that tracker's own
    /// "delivered a neuron to Moon" objective. EnlightenmentTracker already established the real
    /// mechanism (see its own doc comment): SLOracleBehavior.ConvertingSSSwarmer() is the exact
    /// moment Moon finishes accepting a held SSOracleSwarmer (a Five Pebbles neuron) and increments
    /// her own permanent SLOracleState.neuronsLeft counter. Enlightenment reads that counter
    /// passively at hibernation, which is fine for its own "12 or more total" threshold -- but
    /// Saint needs the LIVE moment instead, per the user's own explicit direction that Survivor has
    /// "no save-slot-saved tracked values" to lean on: neuronsLeft is a lifetime, save-file-wide
    /// total that could already be primed by deliveries made long before this specific Saint
    /// attempt began, which a passive threshold read can't distinguish from "happened during THIS
    /// run." A Postfix here fires once per real delivery instead, scoped correctly the same way
    /// every other SurvivorEvents-driven detection in this mod already is.
    /// </summary>
    internal static class SurvivorMoonDelivery
    {
        public static event System.Action<Player> OnNeuronDelivered;

        internal static bool HasSubscribers => OnNeuronDelivered != null;

        internal static void Raise(Player player) => OnNeuronDelivered?.Invoke(player);
    }

    [HarmonyPatch(typeof(SLOracleBehavior), nameof(SLOracleBehavior.ConvertingSSSwarmer))]
    internal static class SurvivorMoonDeliveryHook
    {
        private static void Postfix(SLOracleBehavior __instance)
        {
            if (!SurvivorMoonDelivery.HasSubscribers || __instance?.player == null)
            {
                return;
            }

            RainWorld rainWorld = __instance.player.abstractCreature?.world?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            SurvivorMoonDelivery.Raise(__instance.player);
        }
    }
}
