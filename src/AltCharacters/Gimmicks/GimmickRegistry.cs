namespace AltCharacters.Gimmicks
{
    /// <summary>
    /// The full list of gimmicks the mod knows about. Add a new GimmickDef here (and a
    /// corresponding Harmony patch class that reads GimmickAssignments.IsEnabled(charID, ID))
    /// to introduce a new gimmick; AltCharactersOptions reads this list to build one Remix
    /// checkbox per (character, gimmick) pair automatically.
    /// </summary>
    internal static class GimmickRegistry
    {
        /// <summary>
        /// Traced from the third-party "Gourmand Stamina Display" mod's own "Misery Mode": every
        /// jump or wall-jump instantly maxes out exhaustion (aerobicLevel = 1, gourmandExhausted =
        /// true) and comes out at vanilla's reduced "PainJumps" height/distance -- see
        /// MiseryGimmick.cs for the full trace. Vanilla's own recovery is what lets stamina drain
        /// back down between jumps, so this reads as "jumping is expensive and weak", not a
        /// lowered exhaustion threshold.
        /// </summary>
        public static readonly GimmickDef Misery = new GimmickDef(
            "Misery",
            "Misery",
            "Every jump or wall-jump instantly maxes out exhaustion and comes out weak -- you have to let stamina recover between jumps, and even then jumps stay short."
        );

        /// <summary>
        /// Based on Player.SlugCatClass -- the single field vanilla itself uses everywhere
        /// (physics, jump height, grabbing, AI perception, and every visual in PlayerGraphics)
        /// to decide "is this player currently a slugpup". Vanilla already flips this per-slot
        /// for Jolly Co-op's own "make this player a pup" option; this gimmick reuses the exact
        /// same switch for a single-player story character instead.
        /// </summary>
        public static readonly GimmickDef Slugpup = new GimmickDef(
            "Slugpup",
            "Slugpup",
            "Plays as a slugpup -- small body, pup jump height and speed, and every other creature in the world perceives and reacts to you as a pup instead of an adult slugcat."
        );

        /// <summary>
        /// Grants Spearmaster's real tail spear-pull -- his only way of producing a weapon,
        /// since his diet gives negative food from everything -- by flipping Player.SlugCatClass
        /// to vanilla Spear (the same trick Slugpup uses), which is what every one of vanilla's
        /// hardcoded Spearmaster checks in Player.Update() gates on. Unlike vanilla Spearmaster,
        /// though, the pull can only succeed a limited number of times per save file (default 1,
        /// configurable -- see OneSpearGimmick.OneSpearSettings) before it stops working for
        /// good. Shown in the UI as "Master Spear" -- the internal ID/class name stayed "OneSpear"
        /// to avoid disturbing existing default assignments and save markers.
        /// </summary>
        public static readonly GimmickDef OneSpear = new GimmickDef(
            "OneSpear",
            "Master Spear",
            "Gains Spearmaster's tail spear-pull, his sole means of ever producing a weapon -- but it can only be pulled off a limited number of times for the whole save file; after that, the charge-up never quite finishes again.",
            OneSpearSettings.Bind,
            OneSpearSettings.BuildUI
        );

        /// <summary>
        /// A hard playtime cap. Based on RainWorldGame.clock, the game's own per-tick counter for
        /// the current continuous playthrough (see TimeLimitGimmick.cs for the full trace of what
        /// that field is). Once the configured limit (minutes, per-run or per-cycle -- see
        /// TimeLimitGimmick.TimeLimitSettings) is passed, the character dies outright that same
        /// tick -- no warning yet, that's a later step.
        /// </summary>
        public static readonly GimmickDef TimeLimit = new GimmickDef(
            "TimeLimit",
            "Time Limit",
            "The character dies once the configured amount of time has passed, measured either for the whole run or per cycle.",
            TimeLimitSettings.Bind,
            TimeLimitSettings.BuildUI
        );

        /// <summary>
        /// Spears can never be picked up by the character it's assigned to (Player.Grabability
        /// forced to CantGrab for anything that "is Spear") -- see NoSpearsGimmick.cs. Doesn't
        /// block OneSpear's own tail-pull, which attaches its spear through a different, lower-
        /// level vanilla method that bypasses Grabability entirely. Which of the three spear
        /// types are still allowed can be configured -- see NoSpearsGimmick.NoSpearsSettings.
        /// </summary>
        public static readonly GimmickDef NoSpears = new GimmickDef(
            "NoSpears",
            "No Spears",
            "Spears become impossible to pick up -- they act as if they were never grabbable objects at all (individual spear types can be re-allowed in this gimmick's settings).",
            NoSpearsSettings.Bind,
            NoSpearsSettings.BuildUI
        );

        /// <summary>
        /// Ported from a local, not-on-the-Workshop BepInEx mod already installed on the user's
        /// own machine ("Artificer Rubicon Hell Bonanza (STUPID HALF BAKED VERSION)", mod id
        /// "lockeveryroom", by dannad -- see ChainsGimmick.cs for the full reverse-engineering
        /// trace). Entering a room with any living, non-banned creature in it seals every one of
        /// that room's ordinary exits shut -- visualized with glowing void-chains running from
        /// every sealed doorway to a few of the room's creatures -- until everything in the room
        /// is dead (or a configurable escape timer runs out; see ChainsGimmick.ChainsSettings).
        /// </summary>
        public static readonly GimmickDef Chains = new GimmickDef(
            "Chains",
            "Chains",
            "Entering a room with living creatures in it seals every exit shut -- chained in place by void-chains -- until everything in the room is dead (or a configurable escape timer runs out).",
            ChainsSettings.Bind,
            ChainsSettings.BuildUI
        );

        /// <summary>
        /// Blocks Player.Grabability outright for MoreSlugcats.Yeek (Watcher's small mask-wearing
        /// creature) -- see NoYeeksGimmick.cs, which reuses the exact same Postfix mechanism as
        /// NoSpears. Plain on/off; there's only one Yeek class, so unlike NoSpears there's nothing
        /// to sub-select.
        /// </summary>
        public static readonly GimmickDef NoYeeks = new GimmickDef(
            "NoYeeks",
            "No Yeeks",
            "Yeeks become impossible to pick up -- they act as if they were never grabbable objects at all."
        );

        /// <summary>
        /// Blocks Player.Grabability outright for Cicada (both CicadaA/CicadaB
        /// CreatureTemplate.Type variants are the same one C# class -- see NoCicadasGimmick.cs).
        /// "Squidcadas" is this project's own running nickname for them, carried over from
        /// ChainsGimmick's excludeSquids preset flag.
        /// </summary>
        public static readonly GimmickDef NoCicadas = new GimmickDef(
            "NoCicadas",
            "No Squidcadas",
            "Squidcadas (cicadas) become impossible to pick up -- they act as if they were never grabbable objects at all."
        );

        /// <summary>
        /// Disables Artificer's own instant-kill "maul" grab -- see NoMaulingGimmick.cs, which
        /// Postfixes Player.CanMaulCreature(Creature) and forces it false. That method already
        /// gates on SlugcatStats.SlugcatCanMaul(SlugCatClass) internally, so in vanilla this is
        /// already Artificer-exclusive; this gimmick only has any effect on a character currently
        /// playing under Artificer's own SlugCatClass.
        /// </summary>
        public static readonly GimmickDef NoMauling = new GimmickDef(
            "NoMauling",
            "No Mauling",
            "Disables the instant-kill maul grab outright (only ever relevant to a character playing under Artificer's own class in the first place)."
        );

        /// <summary>
        /// Passages are still shown as normal on the cycle-complete/death screen
        /// (Menu.SleepAndDeathScreen), but the button to actually use one is disabled -- see
        /// NoPassagesGimmick.cs, which Postfixes AddPassageButton and sets the real vanilla
        /// button's own ButtonBehavior.greyedOut flag (the same "visible but inert" mechanism
        /// vanilla itself uses elsewhere) rather than hiding or removing anything. No default
        /// assignment as of this doc -- not requested for any specific character.
        /// </summary>
        public static readonly GimmickDef NoPassages = new GimmickDef(
            "NoPassages",
            "No Passages",
            "Passages are still shown on the cycle-complete/death screen, but the button to actually use one is disabled."
        );

        public static readonly GimmickDef[] All =
        {
            Misery,
            Slugpup,
            OneSpear,
            TimeLimit,
            NoSpears,
            Chains,
            NoYeeks,
            NoCicadas,
            NoMauling,
            NoPassages,
        };
    }
}
