using System.Collections.Generic;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Chef's own "kill it on a zapper" need. Confirmed via decompiling ZapCoil.cs that
    /// ZapCoil.Update's own body kills a creature with a DIRECT `Creature.Die()` call the instant
    /// one of its body chunks touches the coil's contact zone (while `turnedOn > 0.5f`) --
    /// completely bypassing Violence/SetKillTag/killTag entirely. That means every killTag-based
    /// death-cause mechanism already in this mod (PrimalKillTracker, SurvivorEvents.OnPlayerDeath)
    /// is structurally blind to this cause -- there is no killer AbstractCreature to attribute a
    /// zap-coil death to at all.
    ///
    /// This is a room-proximity proxy instead, at the user's own suggestion ("checking if during
    /// the creature death, there was a zapper object nearby or in the room"): Room.zapCoils
    /// (confirmed public `List&lt;ZapCoil&gt;`, maintained by Room itself as ZapCoils are
    /// added/removed) is checked for at least one coil with `turnedOn &gt; 0.5f` -- the exact
    /// threshold ZapCoil.Update's own body gates real zapping on -- at the moment a creature dies
    /// in that room.
    ///
    /// **Known imprecision, flagged explicitly:** this can't prove the zap coil specifically
    /// caused THIS death -- a creature dying of an unrelated cause while merely sharing a room
    /// with an active coil would also read true here. There's no per-kill signal to correlate
    /// against without patching ZapCoil.Update's own per-frame body directly (which iterates every
    /// physical object in the room every tick -- not attempted this pass). Good enough for Chef's
    /// own curated, deliberate setup (the player chooses to bring one specific creature to one
    /// specific zapper) but not a general-purpose "definitely killed by this zapper" guarantee.
    /// </summary>
    internal static class SurvivorHazardDeath
    {
        public static bool WasNearActiveZapCoil(Room room)
        {
            List<ZapCoil> coils = room?.zapCoils;
            if (coils == null)
            {
                return false;
            }

            foreach (ZapCoil coil in coils)
            {
                if (coil != null && coil.turnedOn > 0.5f)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Fires whenever any creature dies in a room with at least one active ZapCoil --
        /// see this class's own header comment for the imprecision that implies. Named for exactly
        /// the one hazard actually traced here, not a general "died to any environmental hazard"
        /// claim -- no other hazard object has been investigated.</summary>
        public static event System.Action<Creature> OnZapCoilDeath;

        internal static bool HasZapCoilDeathSubscribers => OnZapCoilDeath != null;

        internal static void RaiseZapCoilDeath(Creature creature) => OnZapCoilDeath?.Invoke(creature);
    }

    /// <summary>Independent Postfix on Creature.Die -- PrimalKillDieHook and
    /// SurvivorTollRosterDeathHook already patch this same method independently for their own,
    /// unrelated purposes; a third independent Postfix needs no ordering against either, since
    /// none of the three read each other's results.</summary>
    [HarmonyPatch(typeof(Creature), nameof(Creature.Die))]
    internal static class SurvivorHazardDeathHook
    {
        private static void Postfix(Creature __instance)
        {
            if (__instance == null || !__instance.dead || !SurvivorHazardDeath.HasZapCoilDeathSubscribers)
            {
                return;
            }

            RainWorld rainWorld = __instance.abstractCreature?.world?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            if (SurvivorHazardDeath.WasNearActiveZapCoil(__instance.room))
            {
                SurvivorHazardDeath.RaiseZapCoilDeath(__instance);
            }
        }
    }
}
