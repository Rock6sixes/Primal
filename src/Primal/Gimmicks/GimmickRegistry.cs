using MoreSlugcats;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The full list of gimmicks the mod knows about. Add a new GimmickDef here (and a
    /// corresponding Harmony patch class that reads GimmickAssignments.IsEnabled(charID, ID))
    /// to introduce a new gimmick; PrimalOptions reads this list to build one Remix
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
            MiseryEffect.ID,
            "Misery",
            "Every jump or wall-jump instantly maxes out exhaustion and comes out weak -- you have to let stamina recover between jumps, and even then jumps stay short.",
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Gourmand
        );

        /// <summary>
        /// As of its 2026-09-15 rewrite, based on Player.isSlugpup (forced true via a Postfix on
        /// its getter) rather than Player.SlugCatClass -- see SlugpupGimmick.cs for the full
        /// account of why the original SlugCatClass-flip approach was wrong (it broke the real
        /// character's own SlugCatClass-gated abilities and visual fatness while pup-ified) and
        /// how forcing isSlugpup alone, the way the reference "Pupify" mod actually does it,
        /// fixes that: physics/jump height/grabbing/AI-perception all read isSlugpup and still
        /// correctly treat the player as a pup, while everything that reads SlugCatClass directly
        /// (PlayerGraphics' per-character body shape, diet, other abilities) keeps seeing the real
        /// character.
        ///
        /// Has one setting (added 2026-09-15, default ON): once this character reaches the
        /// standard Outer Expanse ending, the gimmick permanently stops applying to them for the
        /// rest of that save file -- still shown as assigned in Remix, it just no longer does
        /// anything. See SlugpupEndingHook/SlugpupEndingSettings in SlugpupGimmick.cs.
        /// </summary>
        public static readonly GimmickDef Slugpup = new GimmickDef(
            SlugpupGimmick.ID,
            "Slugpup",
            "Plays as a slugpup -- small body, pup jump height and speed, and every other creature in the world perceives and reacts to you as a pup instead of an adult slugcat.",
            SlugpupEndingSettings.Bind,
            SlugpupEndingSettings.BuildUI,
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Gourmand
        );

        /// <summary>
        /// Grants Spearmaster's real tail spear-pull -- his only way of producing a weapon,
        /// since his diet gives negative food from everything -- by flipping Player.SlugCatClass
        /// to vanilla Spear (see OneSpearGimmick.cs; Slugpup used to use this same trick but no
        /// longer does as of its 2026-09-15 rewrite -- see SlugpupGimmick.cs), which is what every
        /// one of vanilla's hardcoded Spearmaster checks in Player.Update() gates on. Unlike
        /// vanilla Spearmaster, though, the pull can only succeed a limited number of times per
        /// save file (default 1,
        /// configurable -- see OneSpearGimmick.OneSpearSettings) before it stops working for
        /// good. Shown in the UI as "Master Spear" -- the internal ID/class name stayed "OneSpear"
        /// to avoid disturbing existing default assignments and save markers.
        /// </summary>
        public static readonly GimmickDef OneSpear = new GimmickDef(
            OneSpearGimmick.ID,
            "Master Spear",
            "Gains Spearmaster's tail spear-pull, his sole means of ever producing a weapon -- but it can only be pulled off a limited number of times for the whole save file; after that, the charge-up never quite finishes again.",
            OneSpearSettings.Bind,
            OneSpearSettings.BuildUI,
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Spear
        );

        /// <summary>
        /// A hard playtime cap. Based on RainWorldGame.clock, the game's own per-tick counter for
        /// the current continuous playthrough (see TimeLimitGimmick.cs for the full trace of what
        /// that field is). Once the configured limit (minutes, per-run or per-cycle -- see
        /// TimeLimitGimmick.TimeLimitSettings) is passed, the character dies outright that same
        /// tick -- no warning yet, that's a later step.
        /// </summary>
        public static readonly GimmickDef TimeLimit = new GimmickDef(
            TimeLimitGimmick.ID,
            "Time Limit",
            "The character dies once the configured amount of time has passed, measured either for the whole run or per cycle.",
            TimeLimitSettings.Bind,
            TimeLimitSettings.BuildUI,
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Spear
        );

        /// <summary>
        /// Generalization of the old NoSpears gimmick (removed 2026-09-18) to ten independently
        /// togglable categories -- Spears, Rocks, Bombs, Lanterns, Fruits, Batflies, Squidcadas,
        /// Yeeks, Centipedes, and Other (everything else grabbable) -- see NoHoldGimmick.cs for the
        /// full category-to-class mapping. Same Player.Grabability Postfix mechanism NoSpears
        /// always used; still doesn't block OneSpear's own tail-pull (a different, lower-level
        /// vanilla method that bypasses Grabability entirely). Unlike NoSpears, individual spear
        /// sub-types (plain/explosive/electric) are no longer separately configurable -- Spears is
        /// now a single category, per explicit request when this gimmick was generalized. Also has
        /// an "inert until cycle N" setting (default 0, no delay) that keeps the whole gimmick
        /// switched off until that cycle number is reached -- see NoHoldGimmick.cs's own header
        /// comment.
        /// </summary>
        public static readonly GimmickDef NoHold = new GimmickDef(
            NoHoldGimmick.ID,
            "No Hold",
            "Blocks picking up whichever categories are checked in this gimmick's settings -- Spears, Rocks, Bombs, Lanterns, Fruits, Batflies, Squidcadas, Yeeks, Centipedes, and/or Other (everything else grabbable) -- optionally inert until a configured cycle number.",
            NoHoldSettings.Bind,
            NoHoldSettings.BuildUI,
            settingsContentHeight: 340f,
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Spear
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
            ChainsGimmick.ID,
            "Chains",
            "Entering a room with living creatures in it seals every exit shut -- chained in place by void-chains -- until everything in the room is dead (or a configurable escape timer runs out).",
            ChainsSettings.Bind,
            ChainsSettings.BuildUI,
            // ChainsSettings.BuildUI lays out an escape-timer row, two toggles, a preset picker,
            // and a 6-row/2-column, 12-checkbox banlist grid -- well over 400px tall, badly
            // overflowing the plain settings viewport (see PrimalOptions.RenderGimmickSettingsModal).
            // 480 gives it room to scroll instead of clipping/being unreachable.
            settingsContentHeight: 480f,
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Artificer
        );

        /// <summary>
        /// Blocks Player.Grabability outright for MoreSlugcats.Yeek (Watcher's small mask-wearing
        /// creature) -- see NoYeeksGimmick.cs, which reuses the exact same Postfix mechanism as
        /// NoHold (this same class is also NoHold's own Yeeks category). Plain on/off; there's
        /// only one Yeek class, so there's nothing to sub-select.
        /// </summary>
        public static readonly GimmickDef NoYeeks = new GimmickDef(
            NoYeeksGimmick.ID,
            "No Yeeks",
            "Yeeks become impossible to pick up -- they act as if they were never grabbable objects at all.",
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Artificer
        );

        /// <summary>
        /// Blocks Player.Grabability outright for Cicada (both CicadaA/CicadaB
        /// CreatureTemplate.Type variants are the same one C# class -- see NoCicadasGimmick.cs).
        /// "Squidcadas" is this project's own running nickname for them, carried over from
        /// ChainsGimmick's excludeSquids preset flag.
        /// </summary>
        public static readonly GimmickDef NoCicadas = new GimmickDef(
            NoCicadasGimmick.ID,
            "No Squidcadas",
            "Squidcadas (cicadas) become impossible to pick up -- they act as if they were never grabbable objects at all.",
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Artificer
        );

        /// <summary>
        /// Disables Artificer's own instant-kill "maul" grab -- see NoMaulingGimmick.cs, which
        /// Postfixes Player.CanMaulCreature(Creature) and forces it false. That method already
        /// gates on SlugcatStats.SlugcatCanMaul(SlugCatClass) internally, so in vanilla this is
        /// already Artificer-exclusive; this gimmick only has any effect on a character currently
        /// playing under Artificer's own SlugCatClass.
        /// </summary>
        public static readonly GimmickDef NoMauling = new GimmickDef(
            NoMaulingGimmick.ID,
            "No Mauling",
            "Disables the instant-kill maul grab outright (only ever relevant to a character playing under Artificer's own class in the first place).",
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Artificer
        );

        /// <summary>
        /// Passages are still shown as normal on the cycle-complete/death screen
        /// (Menu.SleepAndDeathScreen), but the button to actually use one is disabled -- see
        /// NoPassagesGimmick.cs, which Postfixes AddPassageButton and sets the real vanilla
        /// button's own ButtonBehavior.greyedOut flag (the same "visible but inert" mechanism
        /// vanilla itself uses elsewhere) rather than hiding or removing anything. No default
        /// assignment as of this doc -- not requested for any specific character. No homeCharacter
        /// either, for the same reason (see GimmickDef.HomeCharacter) -- its button in the Remix
        /// menu keeps OpSimpleButton's own default color instead of being tagged.
        /// </summary>
        public static readonly GimmickDef NoPassages = new GimmickDef(
            NoPassagesGimmick.ID,
            "No Passages",
            "Passages are still shown on the cycle-complete/death screen, but the button to actually use one is disabled."
        );

        /// <summary>
        /// As vanilla's own threat meter (the "Threat Music Pulse" Remix option's visual pulse --
        /// see ThreatSlowdownGimmick.cs) climbs from nothing toward one full pulse-circle ("threat
        /// level 1"), there's a rising chance that any given tick's real input gets silently
        /// dropped entirely -- 0% chance at no threat, 100% (permanent lockout) right at threat
        /// level 1. Reads the same raw threat number the vanilla HUD pulse itself reads, regardless
        /// of whether that HUD element is actually turned on.
        ///
        /// Renamed 2026-09-13 -- this ID used to implement a percentage-based movement/climbing/
        /// swimming/throwing-accuracy debuff instead; that mechanic is retired, and this is the
        /// same input-dropping effect that used to live under the "ThreatPanic" ID (see
        /// ThreatPanicGimmick's own doc comment below for what "Threat Panic" means now). The IDs
        /// were deliberately left as-is (only the underlying code moved) so existing per-character
        /// Remix assignments carried over automatically -- see ThreatSlowdownGimmick.cs's header
        /// comment for the full account. Defaulted onto Monk only via the user's own manual Remix
        /// assignment while testing, not a hardcoded default in GimmickDefaults.cs.
        /// </summary>
        public static readonly GimmickDef ThreatSlowdown = new GimmickDef(
            ThreatSlowdownGimmick.ID,
            "Threat Slowdown",
            "Every tick has a rising chance of the character's real input being dropped entirely as vanilla's own threat meter rises, reaching guaranteed lockout once one pulse-circle is lit (\"threat level 1\").",
            homeCharacter: SlugcatStats.Name.Yellow
        );

        /// <summary>
        /// A second, separate facet of the same threat number ThreatSlowdown reads (see
        /// ThreatSlowdownGimmick.cs and ThreatPanicGimmick.cs, which share that lookup and the
        /// "threat level 1" threshold rather than each deriving their own copy): as threat climbs
        /// toward that same threshold, the character starts involuntarily firing off random inputs
        /// -- one of 4 directions, grab, throw, or jump, held for a random 1-10 ticks -- on top of
        /// whatever the player is actually doing (real input is never suppressed, only added to).
        /// How often a new one starts is the "Frequency" setting below, scaled by how close threat
        /// is to level 1. Composable with ThreatSlowdown -- a character can have either, both, or
        /// neither.
        ///
        /// Renamed/replaced 2026-09-13 -- this ID used to implement the same input-dropping effect
        /// ThreatSlowdown now has; see that GimmickDef's own doc comment above, and
        /// ThreatPanicGimmick.cs's header comment, for the full account of the swap.
        /// </summary>
        public static readonly GimmickDef ThreatPanic = new GimmickDef(
            ThreatPanicGimmick.ID,
            "Threat Panic",
            "As threat rises, the character starts involuntarily firing off random inputs -- a direction, grab, throw, or jump, held briefly -- on top of the player's own real input (which is never blocked), reaching maximum frequency at threat level 1.",
            PanicSettings.Bind,
            PanicSettings.BuildUI,
            homeCharacter: SlugcatStats.Name.Yellow
        );

        /// <summary>
        /// Tracks every one of a fixed, 55-species roster (see KillListRegistry.All in
        /// CreatureKillListGimmick.cs) that the assigned character personally kills -- via
        /// vanilla's own Creature.killTag attribution, not just "died somewhere nearby" -- and
        /// only banks a cycle's kills permanently if that cycle actually ends in a successful
        /// sleep (RainWorldGame.Win); a cycle that ends in death discards that cycle's kills
        /// instead (RainWorldGame.GameOver). No goal/reward logic yet -- purely a tracker for now,
        /// per the user's own request. No default assignment and no homeCharacter -- not tied to
        /// any specific character, same reasoning as NoPassages above. No settings sub-view either
        /// -- progress is shown via a "KILL LIST" button this gimmick adds to the cycle-end screen
        /// instead (KillListButtonHook/KillListSignalHook in CreatureKillListGimmick.cs), not the
        /// Remix menu, per explicit request.
        /// </summary>
        public static readonly GimmickDef CreatureKillList = new GimmickDef(
            CreatureKillListGimmick.ID,
            "Kill List",
            "Tracks every kill this character personally lands against a fixed 55-species roster, banked at the end of any cycle that ends in a successful sleep (a cycle ending in death doesn't count). Progress is shown via a button on the cycle-end screen. No goal/reward yet -- just the tracker."
        );

        /// <summary>
        /// Rescales how long rain (or, on Saint, the region-wide blizzard -- the exact same
        /// underlying timer, see RainTimerGimmick.cs's own header comment for the full trace) takes
        /// to arrive each cycle, as a percentage of vanilla's own normal timing, all the way down
        /// to 0 (essentially immediate). Postfixes RainCycle's constructor and rescales the
        /// cycleLength it already computed, mirroring the exact same scaling points a real,
        /// already-installed third-party mod (MMF) uses for its own rain-speed config. The real
        /// vanilla rendering bug this used to just floor around -- pushing the timer below roughly
        /// 13% of its normal length used to break Saint's blizzard visuals entirely -- is now fixed
        /// at its actual source (see BlizzardVisualCycleLengthHook, same file, and the header
        /// comment's "The actual fix" section), so 0% is genuinely safe to configure. Defaulted to
        /// Saint at 0% (see GimmickDefaults.cs), per explicit request.
        /// </summary>
        public static readonly GimmickDef RainTimer = new GimmickDef(
            RainTimerGimmick.ID,
            "Rain Timer",
            "Rescales how long rain (or Saint's blizzard) takes to arrive each cycle, as a percentage of normal vanilla timing -- 0 is essentially immediate.",
            RainTimerSettings.Bind,
            RainTimerSettings.BuildUI,
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Saint
        );

        /// <summary>
        /// Controls how many minutes into the cycle (from the cycle's own start, not from whenever
        /// rain/blizzard actually hits) the blizzard's heat-drain mechanic suddenly maxes out --
        /// see BlizzardTimerGimmick.cs's own header comment for the full trace of
        /// Creature.HypothermiaUpdate()'s real formula and why a Postfix on the single-call-site
        /// static helper RainWorldGame.BlizzardHardEndTimer, rather than touching cycleLength
        /// directly, is what avoids side-effecting an unrelated wind-chill sub-term in the same
        /// method. Composes correctly with RainTimer above -- both read/derive from the character's
        /// actual live cycleLength at the moment they're asked, so assigning both to the same
        /// character always means exactly what each one's own setting says, independently.
        /// Defaulted to Saint at 60 minutes (see GimmickDefaults.cs), per explicit request.
        /// </summary>
        public static readonly GimmickDef BlizzardTimer = new GimmickDef(
            BlizzardTimerGimmick.ID,
            "Blizzard Timer",
            "Controls how many minutes into the cycle the blizzard's heat-drain mechanic suddenly jumps from a gentle background rate to its maximum severity.",
            BlizzardTimerSettings.Bind,
            BlizzardTimerSettings.BuildUI,
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Saint
        );

        /// <summary>
        /// Replaces vanilla's own precycle ("shelter failure") decision outright -- both how often
        /// one happens (a flat 0-100% for this character, in place of vanilla's own per-region
        /// chance) and how long its head start lasts before the normal rain timer even starts
        /// counting (a percentage multiplier on BOTH bounds of vanilla's own random 4800-12000 tick
        /// roll, same idea as Rain Timer's own percent, in place of a fixed value). See
        /// PrecycleTimerGimmick.cs's own header comment for the full trace of RainCycle's
        /// constructor and exactly which vanilla fields this overrides.
        /// </summary>
        public static readonly GimmickDef PrecycleTimer = new GimmickDef(
            PrecycleTimerGimmick.ID,
            "Precycle Timer",
            "Replaces vanilla's own precycle (shelter failure) chance, head-start duration (a percentage multiplier on vanilla's own roll bounds, or always the scaled upper bound), flood chance, and shelter-failure chance with configured values for this character -- with options to also force flooding on Submerged Superstructure and make a failed shelter stay broken for the rest of the run.",
            PrecycleTimerSettings.Bind,
            PrecycleTimerSettings.BuildUI,
            // Six rows now (frequency; duration+force-max; flood; force-submerged; shelter-failure
            // chance; permanent-shelter-failure) -- comfortably past the plain viewport's own
            // default clamp, same reasoning as Chains' own settingsContentHeight. Bumped from 260f
            // (2026-11-02, one more row added for the shelter-failure-chance/permanent rework).
            settingsContentHeight: 300f,
            // Rivulet, not Saint -- Precycle Timer was always Rivulet's own design-table entry
            // ("Precycle Timer (50% multiplier, 100% chance, 100% shelter failure)"), and is now
            // actually default-assigned to Rivulet (see GimmickDefaults.cs) -- the color tag was
            // left pointing at Saint before this, seemingly a holdover from the same day's Rain/
            // Blizzard Timer work, corrected here to match the real design intent.
            homeCharacter: MoreSlugcatsEnums.SlugcatStatsName.Rivulet
        );

        public static readonly GimmickDef[] All =
        {
            Misery,
            Slugpup,
            OneSpear,
            TimeLimit,
            NoHold,
            Chains,
            NoYeeks,
            NoCicadas,
            NoMauling,
            NoPassages,
            ThreatSlowdown,
            ThreatPanic,
            RainTimer,
            BlizzardTimer,
            PrecycleTimer,
            CreatureKillList,
        };
    }
}
