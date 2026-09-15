namespace Primal.Gimmicks
{
    /// <summary>
    /// The one place that answers "should gimmick G do anything for this player right now".
    ///
    /// Every gimmick patch in this mod needs the same four things, in the same order: resolve the
    /// live RainWorld off the player, check PrimalSlot.IsActive, read the player's real (unfaked)
    /// vanilla identity, and ask GimmickAssignments whether that gimmick is assigned to it. Before
    /// 2026-09-15 that block was copy-pasted into roughly twenty patch bodies, with two separate
    /// inconsistencies that had no reason to exist:
    ///
    ///  - **Two different ways of reaching RainWorld.** Eight gimmicks went through
    ///    `player.room.game.rainWorld`; three (Chains, OneSpear, Slugpup) went through
    ///    `player.abstractCreature.world.game.rainWorld`. The second is strictly more robust:
    ///    Player.room is null while the Player object is still being constructed (which is exactly
    ///    why SlugpupGimmick/OneSpearGimmick had to use the abstractCreature path -- both hook
    ///    methods that run mid-constructor) and can be null in other transitional states, whereas
    ///    abstractCreature.world is already set by Creature's own constructor. The eight
    ///    room-based ones weren't wrong in practice (Jump, Grabability, checkInput etc. only ever
    ///    fire with the player in a room), but they'd silently no-op rather than degrade if that
    ///    ever stopped holding. GameFor below tries the abstractCreature path first and falls back
    ///    to room, so every caller now gets the robust behavior without having to think about it.
    ///  - **Inconsistent null-guarding.** Some copies null-checked playerState, some didn't; some
    ///    guarded the RainWorld, some assumed it. All of that is now in one place.
    ///
    /// Menu-side patches (NoPassages, the Kill List button, the character-select ones) can't use
    /// the Player overloads -- there is no Player on those screens -- so they go through
    /// IsEnabledForCharacter with a characterID they've resolved themselves (usually from
    /// SaveState.saveStateNumber, see PrimalMenuAccess).
    /// </summary>
    internal static class GimmickGate
    {
        /// <summary>The live RainWorldGame this player belongs to, preferring the abstract
        /// creature's own world (valid from Creature's constructor onward) over the realized
        /// room (null until the player is actually placed in one). See this class's header
        /// comment.</summary>
        public static RainWorldGame GameFor(Player player)
        {
            return player?.abstractCreature?.world?.game ?? player?.room?.game;
        }

        public static RainWorld RainWorldFor(Player player)
        {
            return GameFor(player)?.rainWorld;
        }

        /// <summary>True while this player is on one of this mod's reserved save slots, handing
        /// back their real, unfaked vanilla character ID. False (with a null ID) off the reserved
        /// slots, or if anything needed to answer the question isn't available yet.</summary>
        public static bool TryGetCharacter(Player player, out string characterID)
        {
            characterID = null;
            if (player?.playerState == null)
            {
                return false;
            }

            RainWorld rainWorld = RainWorldFor(player);
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return false;
            }

            characterID = player.playerState.slugcatCharacter?.value;
            return characterID != null;
        }

        /// <summary>The full gate: on a reserved slot AND this gimmick assigned to this player's
        /// real character. This is what almost every gimmick patch wants.</summary>
        public static bool IsActiveFor(Player player, string gimmickID)
        {
            return IsActiveFor(player, gimmickID, out _);
        }

        /// <summary>Same as the two-argument overload, but also hands back the character ID -- for
        /// the gimmicks that need it afterwards (per-character settings lookups, save markers,
        /// log lines).</summary>
        public static bool IsActiveFor(Player player, string gimmickID, out string characterID)
        {
            return TryGetCharacter(player, out characterID)
                && GimmickAssignments.IsEnabled(characterID, gimmickID);
        }

        /// <summary>The menu-side equivalent, for screens that have a RainWorld and a character ID
        /// but no Player object (the cycle-end screen, the character-select screen).</summary>
        public static bool IsEnabledForCharacter(RainWorld rainWorld, string characterID, string gimmickID)
        {
            return rainWorld != null
                && characterID != null
                && PrimalSlot.IsActive(rainWorld)
                && GimmickAssignments.IsEnabled(characterID, gimmickID);
        }
    }
}
