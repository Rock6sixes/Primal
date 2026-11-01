using System;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Ornaments' own "pin 12 unique, live creatures against a surface" need. Confirmed via
    /// decompiling Spear.cs, specifically Spear.LodgeInCreature (the method that runs when a
    /// thrown spear embeds in a creature it hit): if the spear's own damage bonus is high enough
    /// AND there's a solid wall directly behind the creature in the throw direction, the spear
    /// sets `stuckInWall` (a `Vector2?`, null otherwise) and `pinToWallCounter = 300` -- this is
    /// the exact, real "creature pinned to a surface" mechanic, not inferred or guessed. Both
    /// fields are plain public fields on Spear.
    ///
    /// A Prefix/Postfix pair on LodgeInCreature comparing `stuckInWall.HasValue` before/after (the
    /// same wasX/isX transition trick used throughout SurvivorEvents.cs) rather than checking
    /// `pinToWallCounter == 300` directly -- the transition check needs no assumption about 300
    /// being a value nothing else could coincidentally produce.
    /// </summary>
    internal static class SurvivorPinning
    {
        /// <summary>Fires the instant a thrown spear pins a creature to a wall/surface -- `spear
        /// .thrownBy` (confirmed public `Creature` field on the base `Weapon` class) identifies who
        /// threw it, for callers that only care about player-caused pins.</summary>
        public static event System.Action<Spear, Creature> OnCreaturePinnedToWall;

        internal static bool HasCreaturePinnedSubscribers => OnCreaturePinnedToWall != null;

        internal static void RaiseCreaturePinnedToWall(Spear spear, Creature creature)
            => OnCreaturePinnedToWall?.Invoke(spear, creature);
    }

    // Spear.LodgeInCreature is overloaded (a real 3-arg implementation plus a 2-arg wrapper that
    // just forwards to it with isJellyFish: false, confirmed via decompiling Spear.cs) -- naming it
    // via plain nameof() leaves Harmony's own AccessTools.DeclaredMethod to resolve it with no
    // argument types at all, which throws AmbiguousMatchException (surfaced as a HarmonyException
    // that aborts the rest of this assembly's Harmony.PatchAll() -- confirmed in a live
    // BepInEx/LogOutput.log, "Primal v0.1.0 FAILED TO LOAD"). Patching the 3-arg version explicitly
    // covers both call paths, since the 2-arg wrapper always delegates into it.
    [HarmonyPatch(typeof(Spear), nameof(Spear.LodgeInCreature),
        new Type[] { typeof(SharedPhysics.CollisionResult), typeof(bool), typeof(bool) })]
    internal static class SurvivorPinningHook
    {
        private static void Prefix(Spear __instance, out bool __state)
        {
            __state = __instance != null && __instance.stuckInWall.HasValue;
        }

        private static void Postfix(Spear __instance, bool __state)
        {
            if (__state || !SurvivorPinning.HasCreaturePinnedSubscribers || __instance == null || !__instance.stuckInWall.HasValue)
            {
                // __state true means this spear was already pinning something before this call --
                // not a NEW pin this call caused.
                return;
            }

            RainWorld rainWorld = __instance.abstractPhysicalObject?.world?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            if (__instance.stuckInChunk?.owner is Creature creature)
            {
                SurvivorPinning.RaiseCreaturePinnedToWall(__instance, creature);
            }
        }
    }
}
