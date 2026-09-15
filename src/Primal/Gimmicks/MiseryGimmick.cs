using System;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Misery" gimmick -- traced directly from the third-party "Gourmand Stamina Display"
    /// mod's own "Misery Mode" toggle (decompiled from its RW_Gourmand_Stamina.dll, workshop item
    /// 2980291710, via ikdasm; internally that checkbox is a Configurable&lt;bool&gt; field named
    /// "permaStarve" -- "Misery mode:" is just its on-screen label).
    ///
    /// That mod does NOT touch Player.aerobicLevel's trip thresholds at all (which is what this
    /// gimmick used to do, and which the user correctly reported as "significantly weaker" than
    /// the real thing). What it actually does, found by disassembling its two On.Player hooks
    /// (PlayerJumpHook / PlayerWallJumpHook -- full copy-pasted-and-edited reimplementations of
    /// vanilla Player.Jump()/Player.WallJump(int), used because MonoMod's On.Player hooks are the
    /// only way that mod can intercept those methods at all) is, every single time either method
    /// fires:
    ///
    ///  1. Unconditionally sets self.gourmandExhausted = true and self.aerobicLevel = 1f, right at
    ///     the top, before doing anything else -- i.e. every jump or wall-jump instantly maxes out
    ///     exhaustion. There's no separate decay logic added anywhere; vanilla's own normal
    ///     per-tick recovery (elsewhere in Player.Update) is what lets aerobicLevel drain back down
    ///     between jumps, so in practice this reads as "jumping costs your entire stamina bar,
    ///     every time, and you have to wait it out before you can jump with any strength again".
    ///  2. ORs a "permaStarve.Value" check into every one of that method's own self.PainJumps()
    ///     checks, so every jump also comes out with PainJumps' own reduced height/distance.
    ///     PainJumps is a real, narrow vanilla mechanic (Player.get_PainJumps) that vanilla itself
    ///     only ever turns on for one unrelated case (Sofanthiel, in the "CC" region, outside gates
    ///     and shelters) -- the reference mod is just reusing that existing "make jumps weak"
    ///     branch as a convenient implementation shortcut, not because it's semantically about
    ///     stamina.
    ///
    /// This reproduces both effects, but through two small Harmony patches instead of a full
    /// method reimplementation (which the original mod only needed because MonoMod's On.Player
    /// hooks were its only way in -- Harmony can patch Player.Jump()/WallJump(int) far more
    /// surgically):
    ///
    ///  - Prefixes on Player.Jump() and Player.WallJump(int) that force gourmandExhausted/
    ///    aerobicLevel to their maxed values right before the original runs -- exactly where and
    ///    when the reference mod does it.
    ///  - A Postfix on the Player.PainJumps property getter itself (rather than only the two call
    ///    sites the reference mod could reach) so every vanilla check of it, wherever it lives,
    ///    sees a Misery character as pained/weak-jumping while the gimmick is active.
    ///
    /// As with the other gimmicks, there are no separate "alt" characters: this reads the player's
    /// real, ordinary SlugcatStats.Name (player.playerState.slugcatCharacter) directly, and only
    /// takes effect on PrimalSlot.ReservedSlot -- off that slot, every character is fully vanilla.
    /// </summary>
    internal static class MiseryEffect
    {
        public const string ID = "Misery";

        // This method used to be the only place in the mod that factored the standard
        // "on a reserved slot, and this gimmick is assigned to this player's real character" gate
        // out of a patch body -- every other gimmick copy-pasted it instead, because the name made
        // it look Misery-specific. It's now GimmickGate.IsActiveFor, shared by all of them; this
        // stays as a thin named alias purely because three separate patch classes in this file
        // read better calling MiseryEffect.IsActiveFor than repeating the gimmick ID each time.
        public static bool IsActiveFor(Player self)
        {
            return GimmickGate.IsActiveFor(self, ID);
        }
    }

    /// <summary>Mirrors the reference mod's "force full exhaustion on every jump" behavior.</summary>
    [HarmonyPatch(typeof(Player), nameof(Player.Jump))]
    internal static class MiseryJumpHook
    {
        private static void Prefix(Player __instance)
        {
            if (!MiseryEffect.IsActiveFor(__instance))
            {
                return;
            }

            __instance.gourmandExhausted = true;
            __instance.aerobicLevel = 1f;
        }
    }

    /// <summary>Same as MiseryJumpHook, for the wall-jump path (vanilla treats it separately).</summary>
    [HarmonyPatch(typeof(Player), nameof(Player.WallJump), new Type[] { typeof(int) })]
    internal static class MiseryWallJumpHook
    {
        private static void Prefix(Player __instance)
        {
            if (!MiseryEffect.IsActiveFor(__instance))
            {
                return;
            }

            __instance.gourmandExhausted = true;
            __instance.aerobicLevel = 1f;
        }
    }

    /// <summary>
    /// Mirrors the reference mod's "OR permaStarve into every PainJumps check" behavior, but
    /// patches the property itself so it applies everywhere vanilla reads it, not just the two
    /// methods the reference mod could reach via MonoMod's On.Player hooks.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.PainJumps), MethodType.Getter)]
    internal static class MiseryPainJumpsHook
    {
        private static void Postfix(Player __instance, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (MiseryEffect.IsActiveFor(__instance))
            {
                __result = true;
            }
        }
    }
}
