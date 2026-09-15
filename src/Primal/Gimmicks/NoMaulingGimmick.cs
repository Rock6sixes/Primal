using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "No Mauling" gimmick: disables Artificer's own "maul" instant-kill grab outright for
    /// the character it's assigned to.
    ///
    /// Traced by disassembling Player.CanMaulCreature(Creature) in the real game assembly: that
    /// method already starts by checking SlugcatStats.SlugcatCanMaul(this.SlugCatClass) and bails
    /// out false immediately if that's false, before any of its other checks (dead/stunned/
    /// friendly-fire/food-full/IsCreatureLegalToHoldWithoutStun/etc. -- all visible in its
    /// disassembly) even run. In other words, mauling is already exclusively an Artificer ability
    /// in vanilla; this gimmick only ever does anything for a character currently playing under
    /// Artificer's own SlugCatClass (whether that's really Artificer, or another character
    /// gimmick-swapped into Artificer's class the way OneSpearGimmick swaps into Spear's -- see
    /// that file's own doc comment; Slugpup no longer works this way as of its 2026-09-15 rewrite,
    /// see SlugpupGimmick.cs), and is a silent no-op for anyone SlugcatCanMaul already says no to.
    ///
    /// Postfixes CanMaulCreature and forces the result to false whenever the asking player has
    /// this gimmick active -- vanilla's own eligibility checks still run first and can
    /// independently return false; this only ever tightens the result, never loosens it.
    ///
    /// No settings -- plain on/off, same shape as Misery/Slugpup/NoYeeks/NoCicadas in GimmickDef.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.CanMaulCreature))]
    internal static class NoMaulingGimmick
    {
        public const string ID = "NoMauling";

        private static void Postfix(Player __instance, ref bool __result)
        {
            if (!__result)
            {
                // Vanilla already said no (dead/stunned/not-Artificer/etc.) -- nothing to add.
                return;
            }

            if (!GimmickGate.IsActiveFor(__instance, ID))
            {
                return;
            }

            __result = false;
        }
    }
}
