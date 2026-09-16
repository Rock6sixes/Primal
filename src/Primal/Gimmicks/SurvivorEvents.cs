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
    /// matching every existing [HarmonyPatch] class in this codebase (PrimalKillCommitHook and
    /// friends), rather than introducing an untested nested-patch-class structure Harmony's
    /// PatchAll has never been exercised against here.
    ///
    /// **Gating.** Every patch below still gates on the cheap, mod-wide `PrimalSlot.IsActive`
    /// check every other gimmick in this mod already gates on first -- so none of this does
    /// anything on an ordinary, non-reserved save slot. It deliberately does NOT gate on any
    /// specific gimmick assignment (unlike GimmickGate.IsActiveFor) -- CommunityChallenges now
    /// exists in GimmickRegistry, but these events fire for every player on a reserved slot
    /// regardless of whether that specific gimmick is assigned to them, on purpose: individual
    /// ChallengeTracker subscribers (see CommunityChallengesGimmick.cs) are each responsible for
    /// their own narrower gating via TryGetEnabledCharacter, which checks BOTH the gimmick
    /// assignment and the specific challenge's own settings checkbox -- a blanket gate here would
    /// only duplicate that, one layer too early to know which challenge (if any) a given
    /// subscriber even cares about.
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
        /// PrimalKillCommitHook is one: Win's own body synchronously triggers the real save-to-
        /// disk write before it returns (see that hook's doc comment for the full trace), so a
        /// Postfix here would suffer the identical "one save too late" bug Kill List's own
        /// kill-commit had until 2026-09-15. Handed the live Player (not just the
        /// AbstractCreature) since most subscribers will want to inspect real-time state --
        /// inventory, room, stats -- as of the moment of waking, and the character ID, since
        /// almost every challenge tracker needs to key its own persisted markers off it (see
        /// PrimalSaveMarkers).</summary>
        public static event Action<RainWorldGame, Player, string> OnSuccessfulHibernation;

        /// <summary>Fires once per assigned-slot player whose cycle just ended in death -- a
        /// Postfix on RainWorldGame.GameOver, matching PrimalKillDiscardHook's own
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

        /// <summary>Added 2026-09-16 (shared mechanism #1 from COMMUNITY_CHALLENGES.md's gap
        /// list): fires every time an assigned-slot player arrives in a new room -- a Postfix on
        /// Player.NewRoom(Room), confirmed via decompilation to be Player's own override of
        /// Creature.NewRoom (itself overriding the virtual PhysicalObject.NewRoom), the exact
        /// moment vanilla's own code uses for tongue repositioning, MSC's item-tracker spawn
        /// logic, and transition-ripple reassignment -- the standard "just arrived" hook, not
        /// something this mod has to infer from position deltas. Hands back the room and region
        /// as plain name strings (AbstractRoom.name, World.name) rather than the Room/AbstractRoom
        /// objects themselves, since every currently-planned consumer (SurvivorCheckpointRun
        /// .NotifyRoomEntered, any future region-visited tracker) only ever keys off the name.
        /// Region can legitimately be null (e.g. a single-room world) even when the room name
        /// isn't.</summary>
        public static event Action<Player, string, string, string> OnPlayerEnteredRoom;

        /// <summary>Added 2026-09-16 (gap #2): fires once per fully-eaten object -- a Postfix on
        /// Player.ObjectEaten(IPlayerEdible), confirmed via decompilation to be THE single
        /// universal "finished eating this" hook (14 separate edible classes across the source
        /// tree -- KarmaFlower, Mushroom, DangleFruit, Centipede, EggBugEgg, Fly, Hazer, JellyFish,
        /// OracleSwarmer, SLOracleSwarmer, SlimeMold, SmallNeedleWorm, SwollenWaterNut,
        /// VultureGrub -- each call it themselves via `(grasp.grabber as Player).ObjectEaten(this)`
        /// once the last bite is taken). Hands back the raw IPlayerEdible rather than trying to
        /// resolve a single common "type" up front -- that interface alone exposes no type info,
        /// and the concrete classes it's implemented by span both AbstractPhysicalObject items and
        /// creatures (e.g. Hazer), so each subscriber is expected to pattern-match for whatever it
        /// actually cares about (a specific object type, a specific creature type, etc.).</summary>
        public static event Action<Player, string, IPlayerEdible> OnObjectEaten;

        /// <summary>Added 2026-09-16 (gap #3): a generalized "died to creature type X in location
        /// Y" event, for any future tracker with the same shape as the existing fall-death one but
        /// keyed on killer type and den/shelter location instead of terrain impact (Tasty:
        /// "pulled into a den" by a specific killer type; Leukocyte: "died inside a shelter room"
        /// to a specific killer type). A Postfix on Player.Die() using the identical wasDead/
        /// isDead transition trick SurvivorFallDeathHook already proves (see that hook's own doc
        /// comment) -- confirmed via decompilation that Player.Die() is Player's own override of
        /// the virtual Creature.Die(), and that (outside an MSC/Coop-only branch this mod doesn't
        /// target) it does NOT call base.Die(), so PrimalKillDieHook's own patch on the base
        /// Creature.Die does not separately fire for player deaths -- this is the only hook that
        /// sees a player's own death. Killer type is read off the same Creature.killTag field
        /// PrimalKillTracker.KillTagField already reflects (trusted here via that existing handle,
        /// not a second reflection lookup) -- null if nothing set it (e.g. starvation, or a killer
        /// whose attribution already decayed). Den/shelter location comes straight off
        /// AbstractWorldEntity.InDen (confirmed public, inherited by AbstractCreature via
        /// AbstractPhysicalObject) and AbstractRoom.shelter (already trusted elsewhere in this
        /// codebase, e.g. SurvivorShelterContents.ShelterRoomFor).</summary>
        public static event Action<Player, string, string, bool, bool> OnPlayerDeath;

        /// <summary>Added 2026-09-16 (gap #6): fires when a player actually commits to a passage
        /// from the cycle-end screen. A Postfix/Prefix pair on Menu.SleepAndDeathScreen.Singal,
        /// gated on the exact "PASSAGE" message case's own body (confirmed via decompilation:
        /// `proceedWithEndgameID == null` on entry, set from `winState.GetNextEndGame()`, and only
        /// followed by the real `endgameTokens.Passage(...)` call if that came back non-null) --
        /// this is a deliberately conservative proxy for "used a passage", chosen so a stricter
        /// reading is what challenges like Enlightenment's "without ever passaging" get: clicking
        /// the button with nothing left to passage into does nothing and correctly does not fire
        /// this. "EXPPASSAGE" (Expedition mode) is a completely separate switch case and is never
        /// matched by this hook. No live Player exists on this screen -- characterID is resolved
        /// via PrimalMenuAccess.GetCharacterID, the same menu-side identity path
        /// CommunityChallengesButtonHook/KillListButtonHook already use. Hands back
        /// DeathPersistentSaveData too (2026-09-16 addition, for EnlightenmentTracker's own
        /// permanent "has this character EVER passaged" marker -- trivially available at the
        /// raise site via PrimalMenuAccess.GetSaveState, and extending this signature before any
        /// other consumer existed is far cheaper than a second event just to carry one more
        /// already-available value).</summary>
        public static event Action<string, WinState.EndgameID, DeathPersistentSaveData> OnPassageUsed;

        /// <summary>Added 2026-09-16 (gap #7): fires when a live Vulture (regular or King) loses
        /// its mask to player violence WITHOUT dying from that same hit -- Usurper's core
        /// interaction. A Postfix on Vulture.Violence comparing mask-before/mask-after/dead-after,
        /// rather than patching Vulture.DropMask (Vulture.cs:1624) directly: DropMask has exactly
        /// one call site in the whole decompiled source, inside Violence, BEFORE Violence's own
        /// trailing base.Violence(...) call actually applies the damage that could kill the
        /// creature -- so a Postfix on DropMask itself would run too early to know whether this
        /// same hit turned out to be fatal. Attribution reuses PrimalKillTracker.KillTagField
        /// (Creature.killTag) rather than parsing Violence's own BodyChunk-ownership chain: Weapon
        /// hit-resolution calls Creature.SetKillTag on every connecting hit, not only fatal ones
        /// (confirmed via Weapon.cs's own TraceProjectileAgainstBodyChunks handling, unconditional
        /// on death), decaying only after 200 ticks (killTagCounter) if the creature survives --
        /// so killTag is already exactly "who most recently hit this creature" at the moment
        /// Violence's Postfix runs, the same signal PrimalKillTracker's own kill-time recording
        /// already trusts. VultureState.mask is a plain public bool (Vulture.cs:188), trusted
        /// directly here rather than reflected, the same direct-trust precedent already set for
        /// ArtificialIntelligence.FriendTracker.friend in ChainsGimmick.cs.
        ///
        /// **Known gap, flagged explicitly:** this is ONLY the mask-falls-off-a-live-Vulture
        /// mechanic (Usurper's need). Elder's "6 Scavengers simultaneously wearing Vulture Masks"
        /// is Scavengers picking up and equipping a VultureMask ITEM as decoration -- a completely
        /// different, not-yet-researched mechanic with no relationship to this event at all.
        /// </summary>
        public static event Action<Player, string, AbstractCreature, bool> OnNonLethalDemask;

        internal static void RaiseSuccessfulHibernation(RainWorldGame game, Player player, string characterID)
            => OnSuccessfulHibernation?.Invoke(game, player, characterID);

        internal static void RaiseCycleFailed(RainWorldGame game, Player player, string characterID)
            => OnCycleFailed?.Invoke(game, player, characterID);

        internal static void RaiseGameEnding(RainWorldGame game, bool standardVoidSea)
            => OnGameEnding?.Invoke(game, standardVoidSea);

        internal static void RaiseFallDamageDeath(Player player, string characterID, string region)
            => OnFallDamageDeath?.Invoke(player, characterID, region);

        internal static void RaisePlayerEnteredRoom(Player player, string characterID, string roomName, string region)
            => OnPlayerEnteredRoom?.Invoke(player, characterID, roomName, region);

        internal static void RaiseObjectEaten(Player player, string characterID, IPlayerEdible edible)
            => OnObjectEaten?.Invoke(player, characterID, edible);

        internal static void RaisePlayerDeath(Player player, string characterID, string killerType, bool inDen, bool inShelter)
            => OnPlayerDeath?.Invoke(player, characterID, killerType, inDen, inShelter);

        internal static void RaisePassageUsed(string characterID, WinState.EndgameID endgameID, DeathPersistentSaveData saveData)
            => OnPassageUsed?.Invoke(characterID, endgameID, saveData);

        internal static void RaiseNonLethalDemask(Player attacker, string characterID, AbstractCreature vulture, bool isKing)
            => OnNonLethalDemask?.Invoke(attacker, characterID, vulture, isKing);

        internal static bool HasSuccessfulHibernationSubscribers => OnSuccessfulHibernation != null;
        internal static bool HasCycleFailedSubscribers => OnCycleFailed != null;
        internal static bool HasGameEndingSubscribers => OnGameEnding != null;
        internal static bool HasFallDamageDeathSubscribers => OnFallDamageDeath != null;
        internal static bool HasPlayerEnteredRoomSubscribers => OnPlayerEnteredRoom != null;
        internal static bool HasObjectEatenSubscribers => OnObjectEaten != null;
        internal static bool HasPlayerDeathSubscribers => OnPlayerDeath != null;
        internal static bool HasPassageUsedSubscribers => OnPassageUsed != null;
        internal static bool HasNonLethalDemaskSubscribers => OnNonLethalDemask != null;
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
                // PrimalKillDiscardHook's own null-tolerant handling of the same
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

    [HarmonyPatch(typeof(Player), nameof(Player.NewRoom))]
    internal static class SurvivorRoomEnterHook
    {
        private static void Postfix(Player __instance, Room newRoom)
        {
            if (!SurvivorEvents.HasPlayerEnteredRoomSubscribers || newRoom == null)
            {
                return;
            }
            if (!GimmickGate.TryGetCharacter(__instance, out string characterID))
            {
                return;
            }

            string roomName = newRoom.abstractRoom?.name;
            if (roomName == null)
            {
                return;
            }
            string region = newRoom.world?.name;
            SurvivorEvents.RaisePlayerEnteredRoom(__instance, characterID, roomName, region);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.ObjectEaten))]
    internal static class SurvivorObjectEatenHook
    {
        private static void Postfix(Player __instance, IPlayerEdible edible)
        {
            if (!SurvivorEvents.HasObjectEatenSubscribers || edible == null)
            {
                return;
            }
            if (!GimmickGate.TryGetCharacter(__instance, out string characterID))
            {
                return;
            }

            SurvivorEvents.RaiseObjectEaten(__instance, characterID, edible);
        }
    }

    /// <summary>See SurvivorEvents.OnPlayerDeath's own doc comment for why this patches
    /// Player.Die() directly rather than the base Creature.Die() PrimalKillDieHook already
    /// patches.</summary>
    [HarmonyPatch(typeof(Player), nameof(Player.Die))]
    internal static class SurvivorPlayerDeathHook
    {
        private static void Prefix(Player __instance, out bool __state)
        {
            __state = __instance != null && __instance.dead;
        }

        private static void Postfix(Player __instance, bool __state)
        {
            if (!SurvivorEvents.HasPlayerDeathSubscribers || __state || __instance == null || !__instance.dead)
            {
                // __state true means this player was already dead before Die() ran -- not a
                // death this call caused.
                return;
            }
            if (!GimmickGate.TryGetCharacter(__instance, out string characterID))
            {
                return;
            }

            AbstractCreature killer = PrimalKillTracker.KillTagField?.GetValue(__instance) as AbstractCreature;
            string killerType = killer?.creatureTemplate?.type?.value;

            AbstractCreature abstractPlayer = __instance.abstractCreature;
            bool inDen = abstractPlayer != null && abstractPlayer.InDen;
            bool inShelter = abstractPlayer?.Room?.shelter ?? false;

            SurvivorEvents.RaisePlayerDeath(__instance, characterID, killerType, inDen, inShelter);
        }
    }

    /// <summary>See SurvivorEvents.OnPassageUsed's own doc comment for the exact "PASSAGE" case
    /// body this mirrors.</summary>
    [HarmonyPatch(typeof(Menu.SleepAndDeathScreen), nameof(Menu.SleepAndDeathScreen.Singal))]
    internal static class SurvivorPassageUsedHook
    {
        private static void Prefix(Menu.SleepAndDeathScreen __instance, string message, out bool __state)
        {
            __state = message == "PASSAGE" && __instance != null && __instance.proceedWithEndgameID == null;
        }

        private static void Postfix(Menu.SleepAndDeathScreen __instance, bool __state)
        {
            if (!__state || !SurvivorEvents.HasPassageUsedSubscribers || __instance?.proceedWithEndgameID == null)
            {
                // __state false means this wasn't a "PASSAGE" click that had a passage still
                // available -- either a different message, or the button did nothing.
                return;
            }

            RainWorld rainWorld = __instance.manager?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            string characterID = PrimalMenuAccess.GetCharacterID(__instance);
            if (characterID != null)
            {
                DeathPersistentSaveData saveData = PrimalMenuAccess.GetSaveState(__instance)?.deathPersistentSaveData;
                SurvivorEvents.RaisePassageUsed(characterID, __instance.proceedWithEndgameID, saveData);
            }
        }
    }

    /// <summary>See SurvivorEvents.OnNonLethalDemask's own doc comment for the full trace of why
    /// this patches Vulture.Violence rather than Vulture.DropMask.</summary>
    [HarmonyPatch(typeof(Vulture), nameof(Vulture.Violence))]
    internal static class SurvivorVultureDemaskHook
    {
        private static void Prefix(Vulture __instance, out bool __state)
        {
            __state = __instance != null && !__instance.dead
                && (__instance.State as Vulture.VultureState)?.mask == true;
        }

        private static void Postfix(Vulture __instance, bool __state)
        {
            if (!__state || !SurvivorEvents.HasNonLethalDemaskSubscribers || __instance == null || __instance.dead)
            {
                return;
            }

            // Default to "still masked" if the state cast ever fails, so a bad cast can't
            // falsely report a demask.
            bool stillMasked = (__instance.State as Vulture.VultureState)?.mask ?? true;
            if (stillMasked)
            {
                return;
            }

            AbstractCreature striker = PrimalKillTracker.KillTagField?.GetValue(__instance) as AbstractCreature;
            if (!(striker?.realizedCreature is Player player) || !GimmickGate.TryGetCharacter(player, out string characterID))
            {
                return;
            }

            SurvivorEvents.RaiseNonLethalDemask(player, characterID, __instance.abstractCreature, __instance.IsKing);
        }
    }
}
