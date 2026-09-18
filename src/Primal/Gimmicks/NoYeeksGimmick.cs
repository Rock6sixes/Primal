using System;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "No Yeeks" gimmick: Yeeks (the small mask-wearing creature added with the Watcher
    /// campaign -- MoreSlugcats.Yeek, extends AirBreatherCreature, confirmed via TypeDef in the
    /// real game assembly) become impossible to pick up for the character it's assigned to.
    ///
    /// This is the exact same mechanism as NoHoldGimmick (see that file's own doc comment for
    /// the full trace of Player.Grabability(PhysicalObject) and why Postfixing it is safe and
    /// scoped to one character; NoHoldGimmick's own Yeeks category uses this exact same "is Yeek"
    /// check): if the object being asked about "is Yeek" and the asking player has this gimmick
    /// active, the result is forced to Player.ObjectGrabability.CantGrab, overriding whatever
    /// vanilla would have said.
    ///
    /// No settings -- there's only one Yeek class, so this is a plain on/off gimmick (same shape
    /// as Misery/Slugpup in GimmickDef).
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.Grabability), new Type[] { typeof(PhysicalObject) })]
    internal static class NoYeeksGimmick
    {
        public const string ID = "NoYeeks";

        private static void Postfix(Player __instance, PhysicalObject obj, ref Player.ObjectGrabability __result)
        {
            if (__result == Player.ObjectGrabability.CantGrab)
            {
                // Already blocked by vanilla's own logic -- nothing to add.
                return;
            }

            if (!(obj is MoreSlugcats.Yeek))
            {
                return;
            }

            if (!GimmickGate.IsActiveFor(__instance, ID))
            {
                return;
            }

            __result = Player.ObjectGrabability.CantGrab;
        }
    }
}
