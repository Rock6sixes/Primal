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
    ///  - Ranger and Spearfisher both reduce to a straight read of the Kill List gimmick's own
    ///    already-tracked permanent kill records (KillListTracker.IsKilled) -- no new detection
    ///    mechanism at all, just a threshold/single-species check against existing data.
    ///
    /// **Cross-gimmick dependency worth remembering:** both Ranger and Spearfisher only ever see
    /// any kills at all for a character that ALSO has the separate "Kill List" gimmick assigned --
    /// CreatureKillListCycleWinHook itself gates every kill it commits on
    /// GimmickAssignments.IsEnabled(characterID, CreatureKillListGimmick.ID), independently of
    /// whether CommunityChallenges is assigned to that same character. This isn't a bug in either
    /// tracker below; it just means a character needs BOTH gimmicks assigned for these two
    /// challenges to ever be able to complete -- has no completion path with Kill List
    /// unassigned, since KillListTracker's own records would simply stay empty.
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

                int uniqueKills = 0;
                foreach (KillListEntry entry in KillListRegistry.All)
                {
                    if (KillListTracker.IsKilled(saveData, characterID, entry.ID))
                    {
                        uniqueKills++;
                        if (uniqueKills >= RequiredUniqueKills)
                        {
                            break;
                        }
                    }
                }

                if (uniqueKills >= RequiredUniqueKills)
                {
                    // Checked once per successful cycle end, off Kill List's own PERMANENT
                    // (whole-save-file) record rather than anything scoped to "this run" --
                    // Ranger's own community rules mean a single continuous life, but this mod
                    // has no existing "run boundary" concept to check against (Kill List's own
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
                if (saveData != null && KillListTracker.IsKilled(saveData, characterID, CreatureTemplate.Type.BigEel.value))
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };
        }
    }
}
