using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Desperado's own "eat from a Popcorn Plant" need. Confirmed via decompilation that "Popcorn
    /// Plant" is the community nickname for vanilla's own SeedCob -- MoreSlugcats/SlugNPCAI.cs's
    /// own food-categorization code (`GetFoodType`) explicitly maps a `SeedCob` to `Food.Popcorn`.
    ///
    /// SeedCob does NOT implement IPlayerEdible and is never eaten through Player.ObjectEaten at
    /// all (confirmed -- SeedCob.cs has no such interface and never calls it). Instead,
    /// SeedCob.Update's own body calls `PlayerSessionRecord.AddEat(this)` directly once a nearby,
    /// hungry, food-source-eligible player stands close enough to it for long enough (the whole
    /// interaction is guarded by `player.eatExternalFoodSourceCounter`, which -- once set --
    /// blocks the same block from running again until it decays, so AddEat fires exactly once per
    /// real eating session, not once per frame of contact). PlayerSessionRecord.AddEat is
    /// therefore the real universal "player ate an external food source" signal
    /// SurvivorEvents.OnObjectEaten's own Player.ObjectEaten hook structurally cannot see for this
    /// object -- patched directly here rather than trying to reverse-engineer SeedCob's own
    /// per-frame proximity/cooldown logic.
    ///
    /// PlayerSessionRecord.playerNumber (confirmed public int, set once in its own constructor,
    /// and the same field AddEat's own body already uses internally via
    /// `eatenObject.room.game.Players[playerNumber]`) is what resolves which player ate it --
    /// AddEat itself takes no Player parameter.
    /// </summary>
    internal static class SurvivorSeedCobEaten
    {
        public static event System.Action<Player, string, SeedCob> OnSeedCobEaten;

        internal static bool HasSubscribers => OnSeedCobEaten != null;

        internal static void Raise(Player player, string characterID, SeedCob seedCob)
            => OnSeedCobEaten?.Invoke(player, characterID, seedCob);
    }

    [HarmonyPatch(typeof(PlayerSessionRecord), nameof(PlayerSessionRecord.AddEat))]
    internal static class SurvivorSeedCobEatenHook
    {
        private static void Postfix(PlayerSessionRecord __instance, PhysicalObject eatenObject)
        {
            if (!SurvivorSeedCobEaten.HasSubscribers || !(eatenObject is SeedCob seedCob))
            {
                return;
            }

            RainWorldGame game = eatenObject.room?.game;
            if (game?.Players == null || __instance.playerNumber < 0 || __instance.playerNumber >= game.Players.Count)
            {
                return;
            }

            if (!(game.Players[__instance.playerNumber].realizedCreature is Player player)
                || !GimmickGate.TryGetCharacter(player, out string characterID))
            {
                return;
            }

            SurvivorSeedCobEaten.Raise(player, characterID, seedCob);
        }
    }
}
