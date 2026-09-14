using System;
using HarmonyLib;

namespace AltCharacters.Gimmicks
{
    /// <summary>
    /// The "No Squidcadas" gimmick: cicadas become impossible to pick up for the character it's
    /// assigned to. "Squidcadas"/"squids" is this project's own running nickname for them (carried
    /// over from ChainsGimmick's excludeSquids preset flag -- see that file's doc comment for the
    /// original discovery that no vanilla creature is actually called "squid"). There is only one
    /// real C# class involved, root-namespace Cicada (extends InsectoidCreature, confirmed via
    /// TypeDef in the real game assembly) -- CicadaA/CicadaB are just the two
    /// CreatureTemplate.Type enum variants of that one class, not separate types, so unlike
    /// NoSpears there's nothing to distinguish at the C# level.
    ///
    /// Same mechanism as NoSpearsGimmick/NoYeeksGimmick: Postfixes Player.Grabability, and forces
    /// the result to Player.ObjectGrabability.CantGrab when the object "is Cicada" and the asking
    /// player has this gimmick active.
    ///
    /// No settings -- plain on/off, same shape as Misery/Slugpup/NoYeeks in GimmickDef.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.Grabability), new Type[] { typeof(PhysicalObject) })]
    internal static class NoCicadasGimmick
    {
        public const string ID = "NoCicadas";

        private static void Postfix(Player __instance, PhysicalObject obj, ref Player.ObjectGrabability __result)
        {
            if (__result == Player.ObjectGrabability.CantGrab)
            {
                // Already blocked by vanilla's own logic -- nothing to add.
                return;
            }

            if (!(obj is Cicada))
            {
                return;
            }

            Player self = __instance;
            if (self?.playerState == null)
            {
                return;
            }

            RainWorld rainWorld = self.room?.game?.rainWorld;
            if (rainWorld == null || !AltSlot.IsActive(rainWorld))
            {
                return;
            }

            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            __result = Player.ObjectGrabability.CantGrab;
        }
    }
}
