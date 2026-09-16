namespace Primal.Gimmicks
{
    /// <summary>
    /// Concrete ChallengeTracker implementations, one class per community challenge, registered in
    /// ChallengeTrackerRegistry.All at the bottom of this file. First three picked without
    /// deliberating over which were "most" easy (per explicit request) -- each one just happened
    /// to fall out of the shared mechanisms/existing gimmicks already built with essentially no
    /// new detection logic of its own:
    ///
    ///  - Descension needed nothing but the 12-region roster itself; SurvivorEvents
    ///    .OnFallDamageDeath and SurvivorRegionMarkers already do all the actual detection and
    ///    persistence.
    ///  - Ranger and Spearfisher both reduce to a straight read of PrimalKillTracker's own
    ///    already-tracked kill records -- no new detection mechanism at all, just a threshold/
    ///    single-species check against existing data.
    ///
    /// **2026-09-16 update: the cross-gimmick dependency this file originally had to flag is
    /// gone.** Ranger and Spearfisher both originally read Kill List's OWN records
    /// (KillListTracker.IsKilled), which only ever existed for a character that ALSO had the
    /// separate "Kill List" gimmick assigned -- per the user's own request ("the tracker...
    /// should be moved out as a global effect, always tracking primal characters' kills"), kill
    /// recording is now PrimalKillTracker.cs, a global, mod-wide mechanism that tracks every
    /// Primal-slot character's kills unconditionally. Both trackers below now read that instead,
    /// so either can complete independent of whether Kill List is assigned at all.
    ///
    /// **Not yet built or tested in-game** -- no .NET toolchain in this container.
    /// </summary>
    internal sealed class DescensionTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Descension");
        public override ChallengeEntry Challenge => _entry;

        /// <summary>The 12 vanilla (MSC/Watcher disabled) regions -- confirmed via decompiling
        /// Menu/RegionSelectMenu.cs, whose own `new string[12]` of display names lines up 1:1,
        /// index for index, with its own `startRooms` array's region-acronym prefixes (SU, DS, HI,
        /// CC, SI, GW, SH, SL, LF, UW, SB, SS) -- the same 12-region roster the community itself
        /// means by "all regions" for a vanilla-only challenge like this one.</summary>
        private static readonly string[] Regions =
        {
            "SU", "DS", "HI", "CC", "SI", "GW", "SH", "SL", "LF", "UW", "SB", "SS",
        };

        public override void Register()
        {
            SurvivorEvents.OnFallDamageDeath += (player, eventCharacterID, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                DeathPersistentSaveData saveData = PrimalSaveMarkers.SaveDataFor(GimmickGate.GameFor(player));
                if (saveData == null)
                {
                    return;
                }

                // Checks the region this exact death just happened in directly, rather than
                // relying on SurvivorRegionMarkers already having recorded it -- SurvivorEvents
                // fires subscribers in registration order, and SurvivorRegionMarkers.Init() is
                // registered before ChallengeTrackerRegistry.RegisterAll() runs (see
                // RainWorldHooks.Postfix), so in practice the marker for THIS death would already
                // be written by the time this handler runs -- but not depending on that ordering
                // holding is one less thing to get subtly wrong later if that registration order
                // ever changes.
                foreach (string r in Regions)
                {
                    bool diedHereByFall = r == region || SurvivorRegionMarkers.HasDiedByFallIn(saveData, characterID, r);
                    if (!diedHereByFall)
                    {
                        return;
                    }
                }

                // A death is as "live" a moment as this mod ever sees -- the player is watching it
                // happen in real time, same reasoning Tasty/Leukocyte's own future trackers will
                // use.
                ReportCompletion(player, characterID, isLive: true);
            };
        }
    }

    internal sealed class RangerTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Ranger");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredUniqueKills = 32;

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                DeathPersistentSaveData saveData = PrimalSaveMarkers.SaveDataFor(game);
                if (saveData == null)
                {
                    return;
                }

                if (PrimalKillTracker.UniqueKillCount(saveData, characterID) >= RequiredUniqueKills)
                {
                    // Checked once per successful cycle end, off PrimalKillTracker's own
                    // PERMANENT (whole-save-file) record rather than anything scoped to "this
                    // run" -- Ranger's own community rules mean a single continuous life, but
                    // this mod has no existing "run boundary" concept to check against (kill
                    // records intentionally never reset on death either). Simplified,
                    // deliberately, the same way COMMUNITY_CHALLENGES.md already flags Massacre/
                    // Outlaw's own scoring as "our own tally, not vanilla's" -- revisit if a real
                    // run-boundary mechanism gets built later.
                    ReportCompletion(player, characterID, isLive: false);
                }
            };
        }
    }

    internal sealed class SpearfisherTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Spearfisher");
        public override ChallengeEntry Challenge => _entry;

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                DeathPersistentSaveData saveData = PrimalSaveMarkers.SaveDataFor(game);
                if (saveData != null && PrimalKillTracker.IsKilled(saveData, characterID, CreatureTemplate.Type.BigEel.value))
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };
        }
    }

    /// <summary>Kill all 9 Lizard types in a single cycle and hibernate. Unlike Ranger (any 32
    /// EVER, whole save file), this is scoped to one cycle -- built on PrimalKillTracker
    /// .WasKilledThisCycle rather than IsKilled for exactly that reason.</summary>
    internal sealed class FoeTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Foe");
        public override ChallengeEntry Challenge => _entry;

        // Vanilla's full 9-color Lizard roster -- the same 9 already in KillListRegistry.All.
        private static readonly CreatureTemplate.Type[] LizardTypes =
        {
            CreatureTemplate.Type.GreenLizard,
            CreatureTemplate.Type.PinkLizard,
            CreatureTemplate.Type.WhiteLizard,
            CreatureTemplate.Type.BlueLizard,
            CreatureTemplate.Type.YellowLizard,
            CreatureTemplate.Type.RedLizard,
            CreatureTemplate.Type.BlackLizard,
            CreatureTemplate.Type.CyanLizard,
            CreatureTemplate.Type.Salamander,
        };

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                foreach (CreatureTemplate.Type lizard in LizardTypes)
                {
                    if (!PrimalKillTracker.WasKilledThisCycle(characterID, lizard.value))
                    {
                        return;
                    }
                }

                ReportCompletion(player, characterID, isLive: false);
            };
        }
    }

    /// <summary>Earn 300 kill points in one cycle. Same "this cycle only" scoping as Foe, via
    /// PrimalKillTracker.ThisCycleKillPointTotal -- see that method's own doc comment for the one
    /// real limitation (each distinct species counts once, not per-kill).</summary>
    internal sealed class MassacreTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Massacre");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredPoints = 300;

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                if (PrimalKillTracker.ThisCycleKillPointTotal(characterID) >= RequiredPoints)
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };
        }
    }

    /// <summary>Collect and sleep in the same shelter as 7 Noodlefly Eggs on the starting cycle.
    /// Uses the new SurvivorShelterContents.CountObjectsOfType (an item-instance count, not a
    /// distinct-type count -- all 7 eggs share the same AbstractObjectType) plus SaveState
    /// .cycleNumber == 0 for "starting cycle" (confirmed via decompilation: cycleNumber only
    /// increments on a SUCCESSFUL cycle end, inside RainCycleTick, which Win's own body calls
    /// AFTER this Prefix-driven check already ran -- so reading it here, at Win-Prefix time,
    /// still correctly reflects "which cycle is this cycle" rather than the next one).</summary>
    internal sealed class MotherTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Mother");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredEggs = 7;

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                if (game?.GetStorySession?.saveState?.cycleNumber != 0)
                {
                    return;
                }

                if (SurvivorShelterContents.CountObjectsOfType(player, AbstractPhysicalObject.AbstractObjectType.NeedleEgg) >= RequiredEggs)
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };
        }
    }

    /// <summary>Hibernate with the Red Lizard in Industrial Complex without taming or killing it.
    ///
    /// **2026-09-16: "without taming it" is now actually checked**, closing the gap this class's
    /// own comment previously flagged. The real mechanism, confirmed via decompiling
    /// ArtificialIntelligence.cs (not guessed): every AI, Lizards included, carries a public
    /// `friendTracker` (type `FriendTracker`) whose `friend` field is the specific Creature it's
    /// been tamed by -- null while wild. `ChainsGimmick.cs` already trusts this exact expression
    /// directly, twice (`lizard.AI.friendTracker?.friend == self`), to recognize "this is my own
    /// tamed ally, don't lock a room over it" -- same field, same access pattern, reused here
    /// rather than re-derived. "Tamed" is checked as `friend == player` specifically (tamed BY
    /// THIS character), not merely `friend != null` (tamed by anyone) -- matching the challenge's
    /// own wording ("without taming... it").</summary>
    internal sealed class SleepoverTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Sleepover");
        public override ChallengeEntry Challenge => _entry;

        // Industrial Complex's region acronym -- same roster DescensionTracker.Regions already
        // confirmed via decompiling Menu/RegionSelectMenu.cs.
        private const string IndustrialComplex = "HI";

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                AbstractRoom shelter = SurvivorShelterContents.ShelterRoomFor(player);
                if (shelter?.world?.name != IndustrialComplex)
                {
                    return;
                }

                AbstractCreature redLizard = SurvivorShelterContents.FindLiveCreatureOfType(player, CreatureTemplate.Type.RedLizard);
                if (redLizard == null)
                {
                    return;
                }

                if (redLizard.realizedCreature is Lizard lizard && lizard.AI != null && lizard.AI.friendTracker?.friend == player)
                {
                    // Tamed by this character -- doesn't count, per the challenge's own wording.
                    return;
                }

                ReportCompletion(player, characterID, isLive: false);
            };
        }
    }

    /// <summary>Obtain all vanilla Steam achievements. Checked off SurvivorEvents
    /// .OnSuccessfulHibernation the same as every other Win-scoped tracker here, even though
    /// achievements themselves can unlock at any moment, not just at a cycle end -- this just
    /// means the challenge is detected the next time this character successfully sleeps after the
    /// last achievement pops, not the instant it happens (no live popup for this one; isLive:
    /// false). A future pass could patch RainWorld.ShowAchievement (or similar) for the live
    /// case, if one exists and turns out to fire achievement-by-achievement -- not investigated
    /// this pass.</summary>
    internal sealed class PassagesTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Passages");
        public override ChallengeEntry Challenge => _entry;

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                RainWorld rainWorld = GimmickGate.RainWorldFor(player);
                if (rainWorld == null)
                {
                    return;
                }

                foreach (RainWorld.AchievementID achievement in SurvivorVanillaProgress.AllAchievements())
                {
                    if (!SurvivorVanillaProgress.HasSteamAchievement(rainWorld, achievement))
                    {
                        return;
                    }
                }

                ReportCompletion(player, characterID, isLive: false);
            };
        }
    }
}
