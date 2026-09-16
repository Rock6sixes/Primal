using System.Collections.Generic;

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

    /// <summary>Added 2026-09-16, wired up now that SurvivorEvents.OnPlayerDeath exists (see that
    /// event's own doc comment). "Get pulled into a den and eaten by a centipede" -- the exact
    /// wording is confirmed against community_challenges_source.csv, saved 2026-09-16 after the
    /// original 50-challenge table needed re-pasting (the condensed notes in this doc's own
    /// ratings table didn't retain enough detail to implement this precisely). "A centipede" isn't
    /// qualified by color/size in the challenge text, and CreatureTemplate.Type confirms
    /// Centipede/SmallCentipede/RedCentipede are three genuinely distinct types (not one type with
    /// a runtime size stat) -- **judgment call, flagged explicitly:** counted all three as
    /// satisfying "a centipede" rather than guessing the community means only the base
    /// (mature-sized) one.</summary>
    internal sealed class TastyTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Tasty");
        public override ChallengeEntry Challenge => _entry;

        private static readonly string[] CentipedeTypes =
        {
            CreatureTemplate.Type.Centipede.value,
            CreatureTemplate.Type.SmallCentipede.value,
            CreatureTemplate.Type.RedCentipede.value,
        };

        public override void Register()
        {
            SurvivorEvents.OnPlayerDeath += (player, eventCharacterID, killerType, inDen, inShelter) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || killerType == null || !inDen)
                {
                    return;
                }

                foreach (string type in CentipedeTypes)
                {
                    if (killerType == type)
                    {
                        // A death is as "live" a moment as this mod ever sees -- same reasoning
                        // DescensionTracker's own fall-death completion already uses.
                        ReportCompletion(player, characterID, isLive: true);
                        return;
                    }
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, wired up now that SurvivorEvents.OnPlayerDeath exists. "Get
    /// eaten by a Daddy Long Legs inside of a shelter" -- a direct read of the same two flags
    /// TastyTracker uses, just the other location check (inShelter instead of inDen) and a single
    /// killer type (CreatureTemplate.Type.DaddyLongLegs, confirmed via decompiling
    /// CreatureTemplate.cs).</summary>
    internal sealed class LeukocyteTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Leukocyte");
        public override ChallengeEntry Challenge => _entry;

        public override void Register()
        {
            SurvivorEvents.OnPlayerDeath += (player, eventCharacterID, killerType, inDen, inShelter) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                if (inShelter && killerType == CreatureTemplate.Type.DaddyLongLegs.value)
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, wired up now that SurvivorEvents.OnObjectEaten exists. "Eat 10
    /// Karma Flowers in one cycle" -- a per-character, per-cycle counter reset at every cycle
    /// boundary (both a successful sleep and a death end "this cycle", so both clear the count for
    /// the next one), checked live the instant the 10th flower lands. KarmaFlower is a plain
    /// vanilla class (`public class KarmaFlower : ... IPlayerEdible ...`) -- a direct `is`
    /// type-check needs no reflection, same as every other concrete-type check already in this
    /// file (e.g. FoeTracker's CreatureTemplate.Type comparisons). **Known gap, flagged
    /// explicitly:** the community rules' own "No Hunter Flower!" exclusion (a specific
    /// Hunter-campaign Karma Flower that apparently shouldn't count) isn't checked -- no
    /// vanilla flag for it was found on a first pass, and confirming one exists would need
    /// dedicated research beyond this batch's "wire up existing mechanics" scope.</summary>
    internal sealed class VisionsTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Visions");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredFlowers = 10;

        // In-memory only, per characterID -- never persisted, matching every other "this cycle"
        // counter in this file (see FoeTracker/MassacreTracker's own use of PrimalKillTracker's
        // equivalent in-memory tables).
        private static readonly Dictionary<string, int> _eatenThisCycle = new Dictionary<string, int>();

        public override void Register()
        {
            SurvivorEvents.OnObjectEaten += (player, eventCharacterID, edible) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || !(edible is KarmaFlower))
                {
                    return;
                }

                int count = _eatenThisCycle.TryGetValue(characterID, out int existing) ? existing + 1 : 1;
                _eatenThisCycle[characterID] = count;

                if (count >= RequiredFlowers)
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _eatenThisCycle.Remove(eventCharacterID);
                }
            };
            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _eatenThisCycle.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, wired up now that SurvivorEvents.OnObjectEaten exists. "Eat a
    /// Mushroom from 4 different regions on the starting cycle" -- a per-character, per-cycle
    /// distinct-region set (same reset-at-cycle-boundary shape as VisionsTracker), gated
    /// additionally on SaveState.cycleNumber == 0 the same way MotherTracker already establishes
    /// "starting cycle" (checked at the moment of eating here, not at Win time, since this
    /// challenge cares about the eating itself happening during cycle 0 -- reading cycleNumber
    /// mid-cycle is safe the same way it's safe at Win-Prefix time, since RainCycleTick's own
    /// increment only ever happens inside a successful Win call, never mid-cycle).</summary>
    internal sealed class MycologistTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Mycologist");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredRegions = 4;

        private static readonly Dictionary<string, HashSet<string>> _regionsThisCycle = new Dictionary<string, HashSet<string>>();

        public override void Register()
        {
            SurvivorEvents.OnObjectEaten += (player, eventCharacterID, edible) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || !(edible is Mushroom))
                {
                    return;
                }

                RainWorldGame game = GimmickGate.GameFor(player);
                if (game?.GetStorySession?.saveState?.cycleNumber != 0)
                {
                    return;
                }

                string region = player.room?.world?.name;
                if (region == null)
                {
                    return;
                }

                if (!_regionsThisCycle.TryGetValue(characterID, out HashSet<string> regions))
                {
                    regions = new HashSet<string>();
                    _regionsThisCycle[characterID] = regions;
                }
                regions.Add(region);

                if (regions.Count >= RequiredRegions)
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _regionsThisCycle.Remove(eventCharacterID);
                }
            };
            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _regionsThisCycle.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, wired up now that SurvivorEvents.OnNonLethalDemask exists. "In
    /// one cycle, demask two live King Vultures and hibernate with their Masks" -- two conditions,
    /// both checked: a per-cycle count of King-Vulture-specific non-lethal demasks (via the new
    /// event's own `isKing` flag), and at hibernation, at least 2 King Vulture masks physically
    /// present in the shelter. The king/non-king distinction on a dropped mask is a plain public
    /// bool (`VultureMask.AbstractVultureMask.king`, confirmed via decompiling VultureMask.cs,
    /// set from the same `IsKing` flag Vulture.DropMask already passes when constructing it) --
    /// trusted directly, matching VultureState.mask's own direct-trust precedent in
    /// SurvivorEvents.cs. Reuses SurvivorShelterContents.EntitiesInShelter directly rather than
    /// adding a new CountObjectsOfType-shaped helper, since the "king" filter needs a field
    /// access CountObjectsOfType's own AbstractObjectType-equality signature doesn't expose. Two
    /// masks acquired in EARLIER cycles and merely still sitting in the shelter don't count on
    /// their own -- the per-cycle demask count must independently reach 2 first. **Known gap,
    /// flagged explicitly:** the challenge's own "No Echo pacification!" exclusion isn't checked,
    /// left as an honor-system note, the same treatment Saint/Vegan's own unenforceable clauses
    /// already get in this doc.</summary>
    internal sealed class UsurperTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Usurper");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredDemasks = 2;

        private static readonly Dictionary<string, int> _kingDemasksThisCycle = new Dictionary<string, int>();

        public override void Register()
        {
            SurvivorEvents.OnNonLethalDemask += (player, eventCharacterID, vulture, isKing) =>
            {
                if (!isKing || !TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }
                _kingDemasksThisCycle[characterID] = _kingDemasksThisCycle.TryGetValue(characterID, out int existing) ? existing + 1 : 1;
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                int demasks = _kingDemasksThisCycle.TryGetValue(characterID, out int count) ? count : 0;
                _kingDemasksThisCycle.Remove(characterID);
                if (demasks < RequiredDemasks)
                {
                    return;
                }

                int kingMasksInShelter = 0;
                foreach (AbstractWorldEntity entity in SurvivorShelterContents.EntitiesInShelter(player))
                {
                    if (entity is VultureMask.AbstractVultureMask mask && mask.king)
                    {
                        kingMasksInShelter++;
                    }
                }

                if (kingMasksInShelter >= RequiredDemasks)
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _kingDemasksThisCycle.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, wired up now that SurvivorEvents.OnGameEnding is finally used by
    /// something (gap-list item #19 -- it was already built and free, just never consumed).
    /// "Reach the Depths and ascend while starting on cycle zero" -- reuses the exact
    /// `standardVoidSea` flag SlugpupGimmick's own ending hook already relies on for its
    /// "true is the separate void-sea/ascension ending" meaning (see SurvivorEvents.OnGameEnding's
    /// own doc comment). **Known gap, flagged explicitly, same treatment as Saint/Vegan's own
    /// unenforceable clauses:** "starting on cycle zero" refers to the Remix assist configuration
    /// (per community_challenges_source.csv's own Notes column: "Hunter cycles must be set to 1
    /// and Bonus cycles to 0 via remix assists") rather than any in-run state this mod could check
    /// -- reaching this ending at all already implies a completed run, so no separate cycle-number
    /// check is applied here. "No Guardian skip!" is likewise left as an honor-system note.</summary>
    internal sealed class PerseveranceTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Perseverance");
        public override ChallengeEntry Challenge => _entry;

        public override void Register()
        {
            SurvivorEvents.OnGameEnding += (game, standardVoidSea) =>
            {
                if (!standardVoidSea || game?.Players == null)
                {
                    return;
                }

                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    if (!(abstractPlayer?.realizedCreature is Player player))
                    {
                        continue;
                    }
                    if (TryGetEnabledCharacter(player, out string characterID))
                    {
                        ReportCompletion(player, characterID, isLive: false);
                    }
                }
            };
        }
    }
}
