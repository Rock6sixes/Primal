using System;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// A small event bus for the handful of vanilla moments the future "Survivor" community-
    /// challenge tracker gimmick needs to react to -- see COMMUNITY_CHALLENGES.md for the full
    /// challenge-by-challenge feasibility pass this was designed against.
    ///
    /// **Why an event bus and not one Harmony patch per challenge.** Of the ~50 challenges rated
    /// there, a large majority key off just a few vanilla moments: a cycle ending in a successful
    /// sleep, a cycle ending in death, one of the two game-beaten endings, or a fatal fall. Without
    /// this class, each of the eventual per-challenge tracker files would need its own Harmony
    /// patch on the same handful of hot methods (RainWorldGame.Win/GameOver/BeatGameMode,
    /// Player.TerrainImpact) -- exactly the kind of copy-pasted-patch problem GimmickGate already
    /// solved for "is this gimmick active for this player". The four patch classes below (one per
    /// vanilla method, each patching it exactly once) re-expose each moment as a plain C# event any
    /// number of future subscribers can add themselves to, at zero additional Harmony-patch cost.
    /// Each patch class is its own top-level type, not nested inside SurvivorEvents itself --
    /// matching every existing [HarmonyPatch] class in this codebase (CreatureKillListCycleWinHook
    /// and friends), rather than introducing an untested nested-patch-class structure Harmony's
    /// PatchAll has never been exercised against here.
    ///
    /// **Gating.** Every patch below still gates on the cheap, mod-wide `PrimalSlot.IsActive`
    /// check every other gimmick in this mod already gates on first -- so none of this does
    /// anything on an ordinary, non-reserved save slot. It deliberately does NOT gate on any
    /// specific gimmick assignment (unlike GimmickGate.IsActiveFor), because there is no
    /// "Survivor" gimmick ID registered yet to check against -- per the user's own request, this
    /// pass only builds the shared mechanisms, not the gimmick itself. Once Survivor exists in
    /// GimmickRegistry, every event below should be re-gated to only fire for a player it's
    /// actually assigned to (via GimmickGate.IsActiveFor(player, SurvivorGimmick.ID)) rather than
    /// firing for every player on a reserved slot regardless of their own assignments.
    ///
    /// **Nothing here has been built or tested** -- no .NET toolchain in this container; verified
    /// against the decompiled reference source only (RainWorldGame.Win/GameOver/BeatGameMode's own
    /// bodies, Player.TerrainImpact's single Die() call site, World.name) the same way every other
    /// patch in this mod is.
    /// </summary>
    internal static class SurvivorEvents
    {
        /// <summary>Fires once per assigned-slot player whose cycle just ended in a successful
        /// sleep -- a Prefix on RainWorldGame.Win, not a Postfix, for the exact reason
        /// CreatureKillListCycleWinHook is one: Win's own body synchronously triggers the real
        /// save-to-disk write before it returns (see that hook's doc comment for the full trace),
        /// so a Postfix here would suffer the identical "one save too late" bug that gimmick had
        /// until 2026-09-15. Handed the live Player (not just the AbstractCreature) since most
        /// subscribers will want to inspect real-time state -- inventory, room, stats -- as of the
        /// moment of waking, and the character ID, since almost every challenge tracker needs to
        /// key its own persisted markers off it (see PrimalSaveMarkers).</summary>
        public static event Action<RainWorldGame, Player, string> OnSuccessfulHibernation;

        /// <summary>Fires once per assigned-slot player whose cycle just ended in death -- a
        /// Postfix on RainWorldGame.GameOver, matching CreatureKillListCycleGameOverHook's own
        /// confirmed-safe timing (nothing this needs to read is affected by GameOver's own body,
        /// unlike Win's synchronous save). For any per-cycle tracker (Visions, Mycologist,
        /// Catalogue, Nomad, ...) that needs to know a cycle ended in failure so it can discard
        /// that cycle's own in-progress state, the same way Kill List already discards pending
        /// kills on death.</summary>
        public static event Action<RainWorldGame, Player, string> OnCycleFailed;

        /// <summary>Fires on every call to RainWorldGame.BeatGameMode, for every character, handing
        /// back the same `standardVoidSea` flag SlugpupGimmick's own ending hook already reads (see
        /// that file's header comment for the full trace of what this flag actually means: true is
        /// the separate void-sea/ascension ending, false is every "standard" ending). Ascension and
        /// Perseverance both care about the true case specifically; a future "beat the game the
        /// normal way" challenge would care about the false case. Left as one raw flag rather than
        /// two separate events so a subscriber that only cares about one branch just checks it and
        /// returns, the same as SlugpupGimmick's own Postfix already does.</summary>
        public static event Action<RainWorldGame, bool> OnGameEnding;

        /// <summary>Fires the instant a player dies specifically from a fatal high-speed downward
        /// terrain impact (Descension's "die by floor-collision fall damage" condition) -- see
        /// SurvivorFallDeathHook's own doc comment for exactly how this is detected. Handed the
        /// region acronym (World.name) the death happened in and the character ID, since
        /// Descension's own persistence is "have I ever died this way in region X", the same
        /// once-per-(character,key) shape PrimalSaveMarkers already exists for.</summary>
        public static event Action<Player, string, string> OnFallDamageDeath;

        internal static void RaiseSuccessfulHibernation(RainWorldGame game, Player player, string characterID)
            => OnSuccessfulHibernation?.Invoke(game, player, characterID);

        internal static void RaiseCycleFailed(RainWorldGame game, Player player, string characterID)
            => OnCycleFailed?.Invoke(game, player, characterID);

        internal static void RaiseGameEnding(RainWorldGame game, bool standardVoidSea)
            => OnGameEnding?.Invoke(game, standardVoidSea);

        internal static void RaiseFallDamageDeath(Player player, string characterID, string region)
            => OnFallDamageDeath?.Invoke(player, characterID, region);

        internal static bool HasSuccessfulHibernationSubscribers => OnSuccessfulHibernation != null;
        internal static bool HasCycleFailedSubscribers => OnCycleFailed != null;
        internal static bool HasGameEndingSubscribers => OnGameEnding != null;
        internal static bool HasFallDamageDeathSubscribers => OnFallDamageDeath != null;
    }

    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.Win))]
    internal static class SurvivorWinHook
    {
        private static void Prefix(RainWorldGame __instance)
        {
            if (!SurvivorEvents.HasSuccessfulHibernationSubscribers
                || __instance?.Players == null
                || !__instance.IsStorySession
                || !PrimalSlot.IsActive(__instance.rainWorld))
            {
                return;
            }

            foreach (AbstractCreature abstractPlayer in __instance.Players)
            {
                if (!(abstractPlayer?.state is PlayerState playerState)
                    || !(abstractPlayer.realizedCreature is Player player))
                {
                    continue;
                }

                string characterID = playerState.slugcatCharacter?.value;
                if (characterID != null)
                {
                    SurvivorEvents.RaiseSuccessfulHibernation(__instance, player, characterID);
                }
            }
        }
    }

    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.GameOver))]
    internal static class SurvivorGameOverHook
    {
        private static void Postfix(RainWorldGame __instance)
        {
            if (!SurvivorEvents.HasCycleFailedSubscribers
                || __instance?.Players == null
                || !__instance.IsStorySession
                || !PrimalSlot.IsActive(__instance.rainWorld))
            {
                return;
            }

            foreach (AbstractCreature abstractPlayer in __instance.Players)
            {
                if (!(abstractPlayer?.state is PlayerState playerState))
                {
                    continue;
                }

                string characterID = playerState.slugcatCharacter?.value;
                if (characterID == null)
                {
                    continue;
                }

                // realizedCreature can legitimately be null here for a death that already tore
                // down the Player object by the time GameOver runs (e.g. PermaDie) -- matches
                // CreatureKillListCycleGameOverHook's own null-tolerant handling of the same
                // field, so subscribers get null rather than this silently swallowing the event.
                SurvivorEvents.RaiseCycleFailed(__instance, abstractPlayer.realizedCreature as Player, characterID);
            }
        }
    }

    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.BeatGameMode))]
    internal static class SurvivorBeatGameModeHook
    {
        private static void Postfix(RainWorldGame game, bool standardVoidSea)
        {
            if (!SurvivorEvents.HasGameEndingSubscribers
                || game == null
                || !game.IsStorySession
                || !PrimalSlot.IsActive(game.rainWorld))
            {
                return;
            }

            SurvivorEvents.RaiseGameEnding(game, standardVoidSea);
        }
    }

    /// <summary>
    /// Detects "died from a fatal high-speed downward terrain impact" (Descension) without
    /// duplicating vanilla's own threshold formula for when a fall is lethal. Confirmed by reading
    /// Player.TerrainImpact's full decompiled body: it contains exactly one call to Die() in the
    /// whole method, inside the branch `speed > num &amp;&amp; immuneToFallDamage &lt;= 0 &amp;&amp;
    /// direction.y &lt; 0 &amp;&amp; ... &amp;&amp; !flag2` (the "Fall damage death" branch,
    /// confirmed via its own Custom.Log("Fall damage death") call right next to it). Rather than
    /// re-deriving that whole condition (num/num2/num3/num4/flag2 depend on isGourmand,
    /// ModManager.MSC, tongue/grasp state, and a Pomegranate check), this patches the same method
    /// with a Prefix/Postfix pair that simply asks "was this player alive before this call, and dead
    /// after it" -- since Die() is the ONLY thing in this method that can make that transition true,
    /// that transition happening at all is sufficient proof the death came from this specific
    /// fall-damage branch, not some other cause.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.TerrainImpact))]
    internal static class SurvivorFallDeathHook
    {
        private static void Prefix(Player __instance, out bool __state)
        {
            __state = __instance != null && __instance.dead;
        }

        private static void Postfix(Player __instance, bool __state)
        {
            if (!SurvivorEvents.HasFallDamageDeathSubscribers || __state || __instance == null || !__instance.dead)
            {
                // __state true means it was already dead before this call -- not a death this
                // call caused, so not a fall death regardless of what else changed.
                return;
            }

            if (!GimmickGate.TryGetCharacter(__instance, out string characterID))
            {
                return;
            }

            string region = __instance.room?.world?.name;
            if (region != null)
            {
                SurvivorEvents.RaiseFallDamageDeath(__instance, characterID, region);
            }
        }
    }
}
