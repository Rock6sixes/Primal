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

    /// <summary>Added 2026-09-16, the first consumer of the color-variant mechanism
    /// (SurvivorCreatureVariant.cs / COMMUNITY_CHALLENGES.md's own confirmation that RedCentipede
    /// is its own distinct CreatureTemplate.Type, no runtime-field check needed). "Lineage, kill
    /// and eat one Red Centipede" -- "Lineage" means the SAME creature instance must be both
    /// killed and eaten, not merely "killed one and separately ate a different one". That needed
    /// one small new capability, added to PrimalKillTracker itself rather than bespoke to this
    /// tracker (see PrimalKillTracker.RecordInstanceKill's own doc comment): per-instance kill
    /// attribution, keyed on EntityID.number, alongside its existing per-TYPE record. Centipede
    /// implements IPlayerEdible directly (confirmed via decompilation -- its own dead body is what
    /// SurvivorEvents.OnObjectEaten eventually reports being eaten), so checking `edible is
    /// Centipede` and cross-referencing its own abstractCreature.ID.number against
    /// WasInstanceKilledBy is a direct, non-guessed correlation, not an approximation.</summary>
    internal sealed class FeastTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Feast");
        public override ChallengeEntry Challenge => _entry;

        public override void Register()
        {
            SurvivorEvents.OnObjectEaten += (player, eventCharacterID, edible) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                if (!(edible is Centipede centipede)
                    || centipede.abstractCreature?.creatureTemplate?.type != CreatureTemplate.Type.RedCentipede)
                {
                    return;
                }

                if (PrimalKillTracker.WasInstanceKilledBy(characterID, centipede.abstractCreature.ID.number))
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, the second consumer of the color-variant mechanism -- and the
    /// one that closed out Safari's own remaining gap: CreatureTemplate.Type.SpitterSpider is
    /// confirmed its own distinct registered Type (SurvivorCreatureVariant.cs), so all three of
    /// Safari's creatures (Red Lizard, Red Centipede, Spitter Spider) are plain Type comparisons,
    /// no runtime color/variant field needed for any of them. "Record ... on the same screen, then
    /// sleep" -- checked once per room arrival (SurvivorEvents.OnPlayerEnteredRoom) against the
    /// player's own CURRENT room contents (AbstractRoom.entities directly, not
    /// SurvivorShelterContents -- that class deliberately only looks at SHELTER rooms, and this
    /// challenge cares about any ordinary room). **Known simplification, flagged explicitly:**
    /// only re-checked on the player's OWN room transitions, not on a continuous per-tick basis --
    /// if a third creature wanders into an already-occupied room while the player stays put
    /// without changing rooms themselves, this won't catch that until the player's next room
    /// change. "Red Lizard spit doesn't count as on-screen" needs no special handling here at all
    /// -- this checks real AbstractCreature entities of each type directly, never anything a spit
    /// projectile could be mistaken for. Scoped to one cycle -- the community's own verification
    /// method (a video of the full cycle) implies the sighting and the sleep happen in the same
    /// attempt, so the "seen together" flag resets on both a successful sleep (whether or not it
    /// completed the challenge) and a failed cycle.</summary>
    internal sealed class SafariTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Safari");
        public override ChallengeEntry Challenge => _entry;

        // In-memory only, per characterID -- has this character seen all three alive together at
        // some point THIS cycle.
        private static readonly HashSet<string> _seenTogetherThisCycle = new HashSet<string>();

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                AbstractRoom room = player.abstractCreature?.Room;
                if (room?.entities == null)
                {
                    return;
                }

                bool hasRedLizard = false;
                bool hasRedCentipede = false;
                bool hasSpitterSpider = false;
                foreach (AbstractWorldEntity entity in room.entities)
                {
                    if (!(entity is AbstractCreature creature) || creature.state.dead)
                    {
                        continue;
                    }

                    CreatureTemplate.Type type = creature.creatureTemplate?.type;
                    if (type == CreatureTemplate.Type.RedLizard)
                    {
                        hasRedLizard = true;
                    }
                    else if (type == CreatureTemplate.Type.RedCentipede)
                    {
                        hasRedCentipede = true;
                    }
                    else if (type == CreatureTemplate.Type.SpitterSpider)
                    {
                        hasSpitterSpider = true;
                    }
                }

                if (hasRedLizard && hasRedCentipede && hasSpitterSpider)
                {
                    _seenTogetherThisCycle.Add(characterID);
                }
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (TryGetEnabledCharacter(player, out string characterID) && _seenTogetherThisCycle.Remove(characterID))
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };
            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _seenTogetherThisCycle.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, requested directly after clarifying the exact route: "Memory
    /// Crypts" is a subregion of Shaded Citadel (SH), "The Leg" and "Underhang" are subregions of
    /// The Exterior (UW, confirmed via Menu/RegionSelectMenu.cs's own debug region list --
    /// UW="THE WALL", which lines up with this challenge's own "No climbing The Wall!" note).
    /// Subregions aren't separately trackable (Primal has no sub-area data, only World.name, the
    /// whole-region acronym) -- tracked at REGION granularity instead: visit SH and visit UW (in
    /// either order) at some point while continuously carrying the tracked Cyan Lizard alive, then
    /// arrive in SS (Five Pebbles) still carrying it alive. **Known simplification, flagged
    /// explicitly:** this can't confirm the player specifically passed through Memory Crypts/The
    /// Leg/Underhang as opposed to some other part of SH/UW, and doesn't enforce SH-before-UW
    /// ordering (the challenge's own phrasing implies a specific route, but the completion INTENT
    /// -- carry the lizard alive across this whole span -- is what's actually checked).
    ///
    /// Reuses SurvivorCreatureTracking (identity/alive) + IsBeingCarriedBy (added this session for
    /// exactly this kind of challenge) + SurvivorEvents.OnPlayerEnteredRoom -- no new mechanism
    /// needed here at all, only the region-name research the user provided. "Lineage a Cyan Lizard
    /// in Outskirts" means tracking starts the first time this character is seen carrying a live
    /// Cyan Lizard while in SU specifically -- CreatureTemplate.Type.CyanLizard is already a
    /// confirmed distinct Type (reused from FoeTracker's own roster).</summary>
    internal sealed class PilgrimageTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Pilgrimage");
        public override ChallengeEntry Challenge => _entry;

        private const string Outskirts = "SU";
        private const string ShadedCitadel = "SH";
        private const string TheExterior = "UW";
        private const string FivePebbles = "SS";

        // In-memory only, per characterID -- which of {SH, UW} this character has passed through
        // so far THIS carry attempt, while continuously carrying the tracked Cyan Lizard alive.
        private static readonly Dictionary<string, HashSet<string>> _regionsVisited = new Dictionary<string, HashSet<string>>();

        private static string TrackerKeyFor(string characterID) => "Pilgrimage:" + characterID;

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || region == null)
                {
                    return;
                }

                string trackerKey = TrackerKeyFor(characterID);

                if (SurvivorCreatureTracking.TrackedCount(trackerKey) == 0)
                {
                    if (region != Outskirts)
                    {
                        return;
                    }
                    AbstractCreature carried = FindCarriedLizardOfType(player, CreatureTemplate.Type.CyanLizard);
                    if (carried == null)
                    {
                        return;
                    }
                    SurvivorCreatureTracking.Touch(trackerKey, carried);
                    _regionsVisited[characterID] = new HashSet<string>();
                    return;
                }

                if (!SurvivorCreatureTracking.AllTrackedAlive(trackerKey))
                {
                    Abandon(trackerKey, characterID);
                    return;
                }

                AbstractCreature tracked = FirstTracked(trackerKey);
                if (tracked == null || !SurvivorCreatureTracking.IsBeingCarriedBy(player, tracked))
                {
                    Abandon(trackerKey, characterID);
                    return;
                }

                if (!_regionsVisited.TryGetValue(characterID, out HashSet<string> visited))
                {
                    visited = new HashSet<string>();
                    _regionsVisited[characterID] = visited;
                }

                if (region == ShadedCitadel || region == TheExterior)
                {
                    visited.Add(region);
                }

                if (region == FivePebbles && visited.Contains(ShadedCitadel) && visited.Contains(TheExterior))
                {
                    ReportCompletion(player, characterID, isLive: true);
                    Abandon(trackerKey, characterID);
                }
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    Abandon(TrackerKeyFor(eventCharacterID), eventCharacterID);
                }
            };
        }

        private static AbstractCreature FindCarriedLizardOfType(Player player, CreatureTemplate.Type type)
        {
            Creature.Grasp[] grasps = player?.grasps;
            if (grasps == null)
            {
                return null;
            }
            foreach (Creature.Grasp grasp in grasps)
            {
                if (grasp?.grabbed is Lizard lizard && lizard.abstractCreature?.creatureTemplate?.type == type)
                {
                    return lizard.abstractCreature;
                }
            }
            return null;
        }

        private static AbstractCreature FirstTracked(string trackerKey)
        {
            foreach (AbstractCreature creature in SurvivorCreatureTracking.TrackedCreatures(trackerKey))
            {
                return creature;
            }
            return null;
        }

        private static void Abandon(string trackerKey, string characterID)
        {
            SurvivorCreatureTracking.ClearTracker(trackerKey);
            _regionsVisited.Remove(characterID);
        }
    }

    /// <summary>Added 2026-09-16. "Carry a live albino Jetfish from Subterranean into Five
    /// Pebbles, kill it on a zapper and eat it." Carry-tracking is the same SU-style shape as
    /// PilgrimageTracker (start tracking the first time this character is seen carrying a live
    /// albino Jetfish while in Subterranean specifically, abandon if ever not-carried or dead).
    /// The zap-kill half consumes the new SurvivorHazardDeath.OnZapCoilDeath (see that class's own
    /// doc comment for why this is a room-proximity proxy, not a guaranteed per-kill signal) --
    /// gated on the death happening in Five Pebbles specifically, matching "kill it ON THE WAY
    /// INTO Five Pebbles" rather than anywhere else a zap coil might exist. Since
    /// OnZapCoilDeath only hands back the dead Creature (no player/character -- a zap coil kills
    /// autonomously, nobody's killTag is ever set), a small reverse index
    /// (creature instance number -&gt; characterID) resolves who was carrying it, the same
    /// shape-of-problem OfferingTracker's own OnLeviathanSwallow integration has.</summary>
    internal sealed class ChefTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Chef");
        public override ChallengeEntry Challenge => _entry;

        private const string Subterranean = "SB";
        private const string FivePebbles = "SS";

        private static readonly Dictionary<int, string> _instanceToCharacter = new Dictionary<int, string>();
        private static readonly Dictionary<string, int> _awaitingEatInstance = new Dictionary<string, int>();

        private static string TrackerKeyFor(string characterID) => "Chef:" + characterID;

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || region == null)
                {
                    return;
                }

                string trackerKey = TrackerKeyFor(characterID);

                if (SurvivorCreatureTracking.TrackedCount(trackerKey) == 0)
                {
                    if (region != Subterranean)
                    {
                        return;
                    }
                    AbstractCreature carried = FindCarriedAlbinoJetfish(player);
                    if (carried == null)
                    {
                        return;
                    }
                    SurvivorCreatureTracking.Touch(trackerKey, carried);
                    _instanceToCharacter[SurvivorCreatureTracking.KeyOf(carried)] = characterID;
                    return;
                }

                if (!SurvivorCreatureTracking.AllTrackedAlive(trackerKey))
                {
                    Abandon(trackerKey);
                    return;
                }

                AbstractCreature tracked = FirstTracked(trackerKey);
                if (tracked == null || !SurvivorCreatureTracking.IsBeingCarriedBy(player, tracked))
                {
                    Abandon(trackerKey);
                }
            };

            SurvivorHazardDeath.OnZapCoilDeath += creature =>
            {
                if (!(creature is JetFish fish) || fish.abstractCreature == null || fish.room?.world?.name != FivePebbles)
                {
                    return;
                }
                if (SurvivorCreatureVariant.IsAlbinoJetfish(fish) != true)
                {
                    return;
                }

                int instanceNumber = fish.abstractCreature.ID.number;
                if (!_instanceToCharacter.TryGetValue(instanceNumber, out string characterID))
                {
                    return;
                }

                _instanceToCharacter.Remove(instanceNumber);
                SurvivorCreatureTracking.ClearTracker(TrackerKeyFor(characterID));
                _awaitingEatInstance[characterID] = instanceNumber;
            };

            SurvivorEvents.OnObjectEaten += (player, eventCharacterID, edible) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID)
                    || !_awaitingEatInstance.TryGetValue(characterID, out int expectedInstance))
                {
                    return;
                }

                if (edible is JetFish fish && fish.abstractCreature?.ID.number == expectedInstance)
                {
                    _awaitingEatInstance.Remove(characterID);
                    ReportCompletion(player, characterID, isLive: true);
                }
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    Abandon(TrackerKeyFor(eventCharacterID));
                    _awaitingEatInstance.Remove(eventCharacterID);
                }
            };
        }

        private static AbstractCreature FindCarriedAlbinoJetfish(Player player)
        {
            Creature.Grasp[] grasps = player?.grasps;
            if (grasps == null)
            {
                return null;
            }
            foreach (Creature.Grasp grasp in grasps)
            {
                if (grasp?.grabbed is JetFish fish && SurvivorCreatureVariant.IsAlbinoJetfish(fish) == true)
                {
                    return fish.abstractCreature;
                }
            }
            return null;
        }

        private static AbstractCreature FirstTracked(string trackerKey)
        {
            foreach (AbstractCreature creature in SurvivorCreatureTracking.TrackedCreatures(trackerKey))
            {
                return creature;
            }
            return null;
        }

        private static void Abandon(string trackerKey)
        {
            foreach (AbstractCreature creature in SurvivorCreatureTracking.TrackedCreatures(trackerKey))
            {
                _instanceToCharacter.Remove(SurvivorCreatureTracking.KeyOf(creature));
            }
            SurvivorCreatureTracking.ClearTracker(trackerKey);
        }
    }

    /// <summary>Added 2026-09-16. "Carry a live purple or green Lantern Mouse to Subterranean then
    /// feed it to the albino Leviathan and survive the encounter." Carry-tracking starts the first
    /// time this character is seen carrying a matching Lantern Mouse (any region -- the challenge
    /// only names Subterranean as the DESTINATION, not where the mouse itself must be found).
    /// Consumes the new SurvivorPredation.OnLeviathanSwallow (see that class's own doc comment for
    /// why BigEel.Swallow, not the taming/ItemOffering social event the user pointed at, is the
    /// real hook -- BigEelAI doesn't implement IReactToSocialEvents at all), gated on the swallow
    /// happening in Subterranean specifically and the eel being the albino variant
    /// (SurvivorCreatureVariant.IsAlbinoLeviathan). Since OnLeviathanSwallow only hands back the
    /// prey Creature (no player/character), the player's own AbstractCreature is captured at the
    /// moment tracking starts and re-resolved to a live Player at swallow time -- simpler than
    /// ChefTracker's characterID-keyed reverse index since ReportCompletion needs an actual Player
    /// reference anyway. "Survive the encounter" is checked as "the player is not ALSO currently
    /// dead at the exact moment of the swallow" -- not a longer post-encounter survival window,
    /// flagged as a deliberate simplification.</summary>
    internal sealed class OfferingTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Offering");
        public override ChallengeEntry Challenge => _entry;

        private const string Subterranean = "SB";

        private static readonly Dictionary<int, AbstractCreature> _instanceToPlayerAbstract = new Dictionary<int, AbstractCreature>();

        private static string TrackerKeyFor(string characterID) => "Offering:" + characterID;

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                string trackerKey = TrackerKeyFor(characterID);

                if (SurvivorCreatureTracking.TrackedCount(trackerKey) == 0)
                {
                    AbstractCreature carried = FindCarriedTargetMouse(player);
                    if (carried == null)
                    {
                        return;
                    }
                    SurvivorCreatureTracking.Touch(trackerKey, carried);
                    _instanceToPlayerAbstract[SurvivorCreatureTracking.KeyOf(carried)] = player.abstractCreature;
                    return;
                }

                if (!SurvivorCreatureTracking.AllTrackedAlive(trackerKey))
                {
                    Abandon(trackerKey);
                    return;
                }

                AbstractCreature tracked = FirstTracked(trackerKey);
                if (tracked == null || !SurvivorCreatureTracking.IsBeingCarriedBy(player, tracked))
                {
                    Abandon(trackerKey);
                }
            };

            SurvivorPredation.OnLeviathanSwallow += (eel, prey) =>
            {
                if (SurvivorCreatureVariant.IsAlbinoLeviathan(eel) != true || eel.room?.world?.name != Subterranean)
                {
                    return;
                }
                if (!(prey is LanternMouse) || prey.abstractCreature == null)
                {
                    return;
                }

                int instanceNumber = prey.abstractCreature.ID.number;
                if (!_instanceToPlayerAbstract.TryGetValue(instanceNumber, out AbstractCreature playerAbstract))
                {
                    return;
                }

                _instanceToPlayerAbstract.Remove(instanceNumber);

                if (!(playerAbstract?.realizedCreature is Player player) || !TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                SurvivorCreatureTracking.ClearTracker(TrackerKeyFor(characterID));
                if (!player.dead)
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    Abandon(TrackerKeyFor(eventCharacterID));
                }
            };
        }

        private static AbstractCreature FindCarriedTargetMouse(Player player)
        {
            Creature.Grasp[] grasps = player?.grasps;
            if (grasps == null)
            {
                return null;
            }
            foreach (Creature.Grasp grasp in grasps)
            {
                if (grasp?.grabbed is LanternMouse mouse && SurvivorCreatureVariant.IsPurpleOrGreenLanternMouse(mouse) == true)
                {
                    return mouse.abstractCreature;
                }
            }
            return null;
        }

        private static AbstractCreature FirstTracked(string trackerKey)
        {
            foreach (AbstractCreature creature in SurvivorCreatureTracking.TrackedCreatures(trackerKey))
            {
                return creature;
            }
            return null;
        }

        private static void Abandon(string trackerKey)
        {
            foreach (AbstractCreature creature in SurvivorCreatureTracking.TrackedCreatures(trackerKey))
            {
                _instanceToPlayerAbstract.Remove(SurvivorCreatureTracking.KeyOf(creature));
            }
            SurvivorCreatureTracking.ClearTracker(trackerKey);
        }
    }
}
