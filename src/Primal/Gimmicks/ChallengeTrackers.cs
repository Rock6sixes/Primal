using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;

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

    /// <summary>Added 2026-09-16. "Pin 12 unique, live creatures against any surface in a single
    /// cycle" -- "unique" read as 12 distinct INDIVIDUAL creatures (not 12 distinct species; the
    /// CSV's own notes never mention species diversity the way Foe's own "9 Lizard types" wording
    /// does), and "Creatures don't need to be left pinned" (confirmed in the CSV's own Notes
    /// column) means only the pinning MOMENT matters, not whether the creature is still pinned
    /// later -- matching this tracker's own instant, per-event counting rather than any ongoing
    /// state check. Consumes the new SurvivorPinning.OnCreaturePinnedToWall directly: "live"
    /// means the creature must not be dead at the exact moment it gets pinned (a pin can
    /// definitely also be lethal, so this doesn't assume every pin qualifies).</summary>
    internal sealed class OrnamentsTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Ornaments");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredPins = 12;

        // In-memory only, per characterID -- distinct creature instances (EntityID.number) pinned
        // alive so far THIS cycle.
        private static readonly Dictionary<string, HashSet<int>> _pinnedThisCycle = new Dictionary<string, HashSet<int>>();

        public override void Register()
        {
            SurvivorPinning.OnCreaturePinnedToWall += (spear, creature) =>
            {
                if (creature == null || creature.dead || creature.abstractCreature == null)
                {
                    return;
                }
                if (!(spear.thrownBy is Player player) || !TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                if (!_pinnedThisCycle.TryGetValue(characterID, out HashSet<int> pinned))
                {
                    pinned = new HashSet<int>();
                    _pinnedThisCycle[characterID] = pinned;
                }
                pinned.Add(creature.abstractCreature.ID.number);

                if (pinned.Count >= RequiredPins)
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _pinnedThisCycle.Remove(eventCharacterID);
                }
            };
            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _pinnedThisCycle.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own direction: "all means all that are
    /// collectible by either monk, survivor or hunter... select all of the unlockables for
    /// sandbox, arena and safari that are intended for vanilla." Consumes the new
    /// SurvivorVanillaProgress.AllSandboxUnlockIDs() directly -- with MSC disabled, that list is
    /// already naturally scoped to the vanilla roster (see that method's own doc comment), so no
    /// extra category filtering is applied here.</summary>
    internal sealed class CollectorTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Collector");
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

                foreach (MultiplayerUnlocks.SandboxUnlockID unlock in SurvivorVanillaProgress.AllSandboxUnlockIDs())
                {
                    if (!SurvivorVanillaProgress.HasSandboxUnlock(rainWorld, unlock))
                    {
                        return;
                    }
                }

                ReportCompletion(player, characterID, isLive: false);
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own confirmation: "reaching the Depths" means
    /// arriving in room SB_E05 (the first room of the Depths subregion). Consumes the new
    /// SurvivorEchoEncounter.OnGhostEncountered (a LIVE per-Echo signal, not a reactive read of
    /// the permanent HasMetGhost flag) specifically to avoid crediting an Echo met long ago, under
    /// a different shelter chain, to a fresh attempt -- see that event's own doc comment. The 6
    /// Echo regions (GhostWorldPresence.GhostID: CC, SI, LF, SH, UW, SB) are a small, fixed,
    /// compile-time roster, confirmed via decompilation to be declared directly as static fields
    /// (no dynamic registration to worry about, unlike SandboxUnlockID).
    ///
    /// "All from one shelter": a per-character "home shelter" locks in on the first hibernation
    /// once this tracker starts paying attention, and resets (starting a fresh attempt, discarding
    /// prior encounters) the moment a hibernation happens from a DIFFERENT shelter -- an Echo
    /// encountered before a home shelter is established, or during a broken chain, simply isn't
    /// credited to anything. Per the CSV's own Notes column ("Does not need to be completed in
    /// consecutive successful cycles"), a cycle ending in DEATH does NOT reset the chain (only a
    /// hibernation from a different shelter does) -- Devotee's own OnCycleFailed handler is
    /// deliberately absent for this reason.</summary>
    internal sealed class DevoteeTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Devotee");
        public override ChallengeEntry Challenge => _entry;

        private const string DepthsRoom = "SB_E05";

        private static readonly GhostWorldPresence.GhostID[] EchoRegions =
        {
            GhostWorldPresence.GhostID.CC,
            GhostWorldPresence.GhostID.SI,
            GhostWorldPresence.GhostID.LF,
            GhostWorldPresence.GhostID.SH,
            GhostWorldPresence.GhostID.UW,
            GhostWorldPresence.GhostID.SB,
        };

        private sealed class AttemptState
        {
            public string HomeShelter;
            public readonly HashSet<string> MetThisAttempt = new HashSet<string>();
            public bool DepthsReached;
        }

        private static readonly Dictionary<string, AttemptState> _attempts = new Dictionary<string, AttemptState>();

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                string currentShelter = SurvivorShelterContents.ShelterRoomFor(player)?.name;
                if (currentShelter == null)
                {
                    return;
                }

                if (!_attempts.TryGetValue(characterID, out AttemptState state) || state.HomeShelter != currentShelter)
                {
                    // First hibernation this tracker has seen for this character, or the shelter
                    // chain just broke -- start a fresh attempt from here.
                    _attempts[characterID] = new AttemptState { HomeShelter = currentShelter };
                }
            };

            SurvivorEchoEncounter.OnGhostEncountered += (characterID, ghost) =>
            {
                if (_attempts.TryGetValue(characterID, out AttemptState state))
                {
                    state.MetThisAttempt.Add(ghost.value);
                }
            };

            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || !_attempts.TryGetValue(characterID, out AttemptState state))
                {
                    return;
                }

                if (roomName == DepthsRoom)
                {
                    state.DepthsReached = true;
                }

                if (state.DepthsReached && state.MetThisAttempt.Count >= EchoRegions.Length)
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own confirmation: the rot-less Memory Conflux
    /// rooms on this route are SS_LAB7 and SS_A01. **Known simplifications, flagged explicitly:**
    /// this doesn't verify the cycle started at the specific "first Five Pebbles shelter" (only
    /// that the WHOLE attempt stays inside Five Pebbles), and doesn't verify the return route
    /// specifically passes back through Unfortunate Development (the CSV's own "must go through
    /// UD" clause) -- neither UD's own room list nor the exact starting shelter's room ID were
    /// available for this pass. What IS checked: the entire cycle stays within Five Pebbles (SS)
    /// from the moment this tracker starts paying attention, touches one of the two rot-less
    /// rooms at some point, and ends in a successful hibernation -- leaving Five Pebbles at any
    /// point abandons the attempt.</summary>
    internal sealed class MartyrTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Martyr");
        public override ChallengeEntry Challenge => _entry;

        private const string FivePebbles = "SS";
        private static readonly string[] RotlessMemoryConfluxRooms = { "SS_LAB7", "SS_A01" };

        private sealed class AttemptState
        {
            public bool ReachedRotlessConflux;
        }

        private static readonly Dictionary<string, AttemptState> _attempts = new Dictionary<string, AttemptState>();

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || region == null)
                {
                    return;
                }

                if (region != FivePebbles)
                {
                    _attempts.Remove(characterID);
                    return;
                }

                if (!_attempts.TryGetValue(characterID, out AttemptState state))
                {
                    state = new AttemptState();
                    _attempts[characterID] = state;
                }

                foreach (string room in RotlessMemoryConfluxRooms)
                {
                    if (roomName == room)
                    {
                        state.ReachedRotlessConflux = true;
                    }
                }
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || !_attempts.TryGetValue(characterID, out AttemptState state))
                {
                    return;
                }

                _attempts.Remove(characterID);
                if (state.ReachedRotlessConflux)
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _attempts.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own confirmation: the "Guardians room" is
    /// SB_D03, and the real class behind "Grapple Worm" is TubeWorm (confirmed via decompiling
    /// TubeWorm.cs -- it has its own grappleToPos/GrappleToPosition()/grapplingCounter fields, the
    /// exact zipline/grapple mechanic the community nickname refers to). "Carry a live Grapple
    /// Worm and a Snail together" tracks BOTH simultaneously under one SurvivorCreatureTracking
    /// key (that class's own multi-creature support handles this without any special-casing).
    /// "Survive that cycle without starving" reads Player.Malnourished (confirmed public bool
    /// property) on every room-entry while the attempt is active -- flagged as checked
    /// opportunistically per room-transition, not continuously every tick, so a starve-then-
    /// instantly-refeed within the same room could theoretically be missed. **Known
    /// simplification:** doesn't verify the "highest room in Communication Arrays" waypoint (its
    /// exact room ID wasn't available this pass) -- only the final Guardians-room arrival with
    /// both creatures alive, uninterrupted carrying, and no starving is checked.</summary>
    internal sealed class GuardianTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Guardian");
        public override ChallengeEntry Challenge => _entry;

        private const string Subterranean = "SB";
        private const string GuardiansRoom = "SB_D03";

        private static readonly HashSet<string> _starvedThisAttempt = new HashSet<string>();

        private static string TrackerKeyFor(string characterID) => "Guardian:" + characterID;

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
                    if (!TryFindBothCarried(player, out AbstractCreature worm, out AbstractCreature snail))
                    {
                        return;
                    }
                    SurvivorCreatureTracking.Touch(trackerKey, worm);
                    SurvivorCreatureTracking.Touch(trackerKey, snail);
                    _starvedThisAttempt.Remove(characterID);
                }

                if (!SurvivorCreatureTracking.AllTrackedAlive(trackerKey) || !StillCarryingBoth(player, trackerKey))
                {
                    Abandon(trackerKey, characterID);
                    return;
                }

                if (player.Malnourished)
                {
                    _starvedThisAttempt.Add(characterID);
                }

                if (!_starvedThisAttempt.Contains(characterID) && region == Subterranean && roomName == GuardiansRoom)
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

        private static bool TryFindBothCarried(Player player, out AbstractCreature worm, out AbstractCreature snail)
        {
            worm = null;
            snail = null;
            Creature.Grasp[] grasps = player?.grasps;
            if (grasps == null)
            {
                return false;
            }

            foreach (Creature.Grasp grasp in grasps)
            {
                if (!(grasp?.grabbed is Creature creature) || creature.dead)
                {
                    continue;
                }
                CreatureTemplate.Type type = creature.abstractCreature?.creatureTemplate?.type;
                if (type == CreatureTemplate.Type.TubeWorm)
                {
                    worm = creature.abstractCreature;
                }
                else if (type == CreatureTemplate.Type.Snail)
                {
                    snail = creature.abstractCreature;
                }
            }
            return worm != null && snail != null;
        }

        private static bool StillCarryingBoth(Player player, string trackerKey)
        {
            foreach (AbstractCreature creature in SurvivorCreatureTracking.TrackedCreatures(trackerKey))
            {
                if (!SurvivorCreatureTracking.IsBeingCarriedBy(player, creature))
                {
                    return false;
                }
            }
            return true;
        }

        private static void Abandon(string trackerKey, string characterID)
        {
            SurvivorCreatureTracking.ClearTracker(trackerKey);
            _starvedThisAttempt.Remove(characterID);
        }
    }

    /// <summary>Added 2026-09-16. "Trigger the Stolen Enlightenment dream on the starting cycle."
    /// Consumes the new SurvivorDreamState.OnCycleEndDream, checking for
    /// DreamsState.DreamID.MoonThief (confirmed to be "Stolen Enlightenment" -- see that event's
    /// own doc comment). **The cycleNumber check here is deliberately `== 1`, not `== 0`** --
    /// this event is a Postfix on RainWorldGame.Win (the only one in this whole event bus that has
    /// to be), so by the time it fires, SaveState.cycleNumber has ALREADY incremented past the
    /// cycle that just ended. Every OTHER "starting cycle" check in this codebase
    /// (MotherTracker, MycologistTracker) reads cycleNumber from a PREFIX, where it's still 0 --
    /// getting this backwards here would silently detect the SECOND cycle instead of the
    /// first.</summary>
    internal sealed class DespicableTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Despicable");
        public override ChallengeEntry Challenge => _entry;

        public override void Register()
        {
            SurvivorDreamState.OnCycleEndDream += (player, eventCharacterID, dream, cycleNumberAfterIncrement) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                if (cycleNumberAfterIncrement != 1 || dream != DreamsState.DreamID.MoonThief)
                {
                    return;
                }

                ReportCompletion(player, characterID, isLive: false);
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own confirmation: "SL_AI contains the Looks to
    /// the Moon iterator entity and if you throw a neuron to her, she stores them." Confirmed via
    /// decompilation: `SLOracleBehavior.ConvertingSSSwarmer()` increments
    /// `SLOrcacleState.neuronsLeft` (the same field DespicableTracker's own MoonThief detection
    /// already reads, confirming this is one persistent, real vanilla counter, not a guess) --
    /// this is Moon accepting an SSOracleSwarmer (a Five Pebbles neuron) delivered to her.
    /// Per the CSV's own explicit numeric requirement ("Total neurons at the end of the challenge
    /// must be 12 or more"), checked as a plain threshold on that persistent counter rather than
    /// re-deriving "+7 from wherever the player started" -- simpler, and matches the stated
    /// mechanical requirement exactly rather than the vaguer "bring 7" framing in the main
    /// description.
    ///
    /// "Without passaging" is read as an absolute, permanent, save-file-wide disqualification
    /// (never, not just during one narrow attempt) -- SurvivorEvents.OnPassageUsed writes a
    /// permanent PrimalSaveMarkers flag the instant a passage is ever taken, checked here before
    /// allowing completion. This required extending OnPassageUsed's own signature to also hand
    /// back DeathPersistentSaveData (see that event's own updated doc comment) -- trivially
    /// available at its raise site and, since nothing else consumed that event yet, cheaper than
    /// adding a second event just to carry one already-available value.</summary>
    internal sealed class EnlightenmentTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Enlightenment");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredNeurons = 12;
        private const string PassagedMarkerKey = "PRIMAL_ENLIGHTENMENT_PASSAGED";

        public override void Register()
        {
            SurvivorEvents.OnPassageUsed += (characterID, endgameID, saveData) =>
            {
                if (saveData != null)
                {
                    PrimalSaveMarkers.Add(saveData, PrimalSaveMarkers.Marker(PassagedMarkerKey, characterID));
                }
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                DeathPersistentSaveData saveData = PrimalSaveMarkers.SaveDataFor(game);
                if (saveData == null || PrimalSaveMarkers.Has(saveData, PrimalSaveMarkers.Marker(PassagedMarkerKey, characterID)))
                {
                    return;
                }

                int neuronsLeft = game?.GetStorySession?.saveState?.miscWorldSaveData?.SLOracleState?.neuronsLeft ?? 0;
                if (neuronsLeft >= RequiredNeurons)
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own confirmed technique. Consumes the new
    /// SurvivorPoleMimicPredation.OnLiveVultureGrabbed -- see that event's own doc comment for
    /// why "grabbed" (not a separate kill) is treated as "fed", and why "survive the encounter" is
    /// checked as "not currently dead" rather than a longer post-event survival window. Since the
    /// event fires from an autonomous creature-vs-creature Collide call with no player directly
    /// involved, this iterates the game's own Players list (the same pattern
    /// PerseveranceTracker's own OnGameEnding consumption already uses) to find whichever gated
    /// character is present.</summary>
    internal sealed class FlytrapTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Flytrap");
        public override ChallengeEntry Challenge => _entry;

        public override void Register()
        {
            SurvivorPoleMimicPredation.OnLiveVultureGrabbed += (mimic, vulture) =>
            {
                RainWorldGame game = mimic.abstractCreature?.world?.game;
                if (game?.Players == null)
                {
                    return;
                }

                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    if (!(abstractPlayer?.realizedCreature is Player player) || player.dead)
                    {
                        continue;
                    }
                    if (TryGetEnabledCharacter(player, out string characterID))
                    {
                        ReportCompletion(player, characterID, isLive: true);
                    }
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, the first consumer of the toll mechanism's own persistent
    /// roster (SurvivorTollRoster, built earlier alongside SurvivorTollState). "Cause all of the
    /// Scavengers at a Scavenger Toll to die without gaining kill credit or angering any
    /// Scavengers" -- three independent conditions, all already answerable by existing
    /// mechanisms: SurvivorTollRoster.AllEverSeenGuardsDead (every guard EVER assigned here,
    /// including replacements, confirmed dead), SurvivorTollRoster.PlayerEverCreditedAGuardKill
    /// (must be false), and a small local "ever went hostile against this character" set fed by
    /// SurvivorTollState.OnOutpostTransgression (must never have fired with becameHostile==true
    /// for this outpost+character pair). No cycle-scoping in the challenge's own wording -- unlike
    /// Foe/Massacre, this can span as many cycles as it takes, matching how the roster itself is
    /// already persistent (session-only, not per-cycle). Checked on every room arrival at the
    /// outpost's own room, so it fires the next time the player happens to walk back through
    /// after the last guard dies.</summary>
    internal sealed class ManslaughterTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Manslaughter");
        public override ChallengeEntry Challenge => _entry;

        // Per (characterID, roomKey) -- has this outpost ever gone hostile against this character.
        private static readonly HashSet<string> _everHostile = new HashSet<string>();

        private static string HostileKey(string characterID, string roomKey) => characterID + "|" + roomKey;

        public override void Register()
        {
            SurvivorTollState.OnOutpostTransgression += (outpost, player, becameHostile) =>
            {
                if (!becameHostile || !TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }
                string roomKey = SurvivorTollRoster.RoomKeyFor(outpost);
                if (roomKey != null)
                {
                    _everHostile.Add(HostileKey(characterID, roomKey));
                }
            };

            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                ScavengerOutpost outpost = SurvivorTollState.OutpostIn(player.room);
                string roomKey = SurvivorTollRoster.RoomKeyFor(outpost);
                if (roomKey == null)
                {
                    return;
                }

                if (!SurvivorTollRoster.AllEverSeenGuardsDead(roomKey)
                    || SurvivorTollRoster.PlayerEverCreditedAGuardKill(roomKey)
                    || _everHostile.Contains(HostileKey(characterID, roomKey)))
                {
                    return;
                }

                ReportCompletion(player, characterID, isLive: true);
            };
        }
    }

    /// <summary>Added 2026-09-16. "Bring live Scavengers to the Looks to the Moon subregion." The
    /// target room -- SL_AI, the room Moon's own iterator entity lives in -- was confirmed
    /// investigating Enlightenment's own "throw a neuron to Moon" mechanism
    /// (SLOracleBehavior.ConvertingSSSwarmer runs on the Oracle in that room), and lines up with
    /// the challenge's own verification note ("Scavengers at the Karma Gate into Shoreline + the
    /// same Scavengers in Looks to the Moon" -- Shoreline, SL, is exactly the region SL_AI is in).
    /// Scavengers here aren't grabbed/carried (unlike Symbiote/Charged/Chef) -- they're tamed
    /// allies that follow the player on their own -- so this reuses SurvivorCreatureTracking's
    /// identity layer directly (Touch/AllTrackedAlive/FindInRoom) without IsBeingCarriedBy at all.
    /// Tracking starts the first time the character is seen in Shoreline with 2+ live Scavengers
    /// in the same room (the "Karma Gate" moment); "2+" is a judgment call, flagged explicitly --
    /// the challenge's own text just says "Scavengers" (plural) with no explicit count given
    /// anywhere in its own CSV row.</summary>
    internal sealed class ChieftainTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Chieftain");
        public override ChallengeEntry Challenge => _entry;

        private const string Shoreline = "SL";
        private const string MoonRoom = "SL_AI";
        private const int RequiredScavengers = 2;

        private static string TrackerKeyFor(string characterID) => "Chieftain:" + characterID;

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
                    if (region != Shoreline)
                    {
                        return;
                    }
                    List<AbstractCreature> scavengers = FindNearbyLiveScavengers(player);
                    if (scavengers.Count < RequiredScavengers)
                    {
                        return;
                    }
                    foreach (AbstractCreature scav in scavengers)
                    {
                        SurvivorCreatureTracking.Touch(trackerKey, scav);
                    }
                    return;
                }

                if (!SurvivorCreatureTracking.AllTrackedAlive(trackerKey))
                {
                    SurvivorCreatureTracking.ClearTracker(trackerKey);
                    return;
                }

                if (roomName != MoonRoom)
                {
                    return;
                }

                AbstractRoom moonRoom = player.abstractCreature?.Room;
                if (moonRoom == null)
                {
                    return;
                }

                int foundAlive = 0;
                foreach (AbstractCreature scav in SurvivorCreatureTracking.TrackedCreatures(trackerKey))
                {
                    if (SurvivorCreatureTracking.FindInRoom(trackerKey, moonRoom, SurvivorCreatureTracking.KeyOf(scav)) != null)
                    {
                        foundAlive++;
                    }
                }

                if (foundAlive >= RequiredScavengers)
                {
                    ReportCompletion(player, characterID, isLive: true);
                    SurvivorCreatureTracking.ClearTracker(trackerKey);
                }
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    SurvivorCreatureTracking.ClearTracker(TrackerKeyFor(eventCharacterID));
                }
            };
        }

        private static List<AbstractCreature> FindNearbyLiveScavengers(Player player)
        {
            var result = new List<AbstractCreature>();
            AbstractRoom room = player.abstractCreature?.Room;
            if (room?.entities == null)
            {
                return result;
            }
            foreach (AbstractWorldEntity entity in room.entities)
            {
                if (entity is AbstractCreature creature && !creature.state.dead
                    && creature.creatureTemplate?.type == CreatureTemplate.Type.Scavenger)
                {
                    result.Add(creature);
                }
            }
            return result;
        }
    }

    /// <summary>Added 2026-09-16. "Travel to the Garbage Wastes Stronghold after passing three
    /// Scavenger Tolls without paying on the starting cycle." Consumes the new
    /// SurvivorTollState.OnTollPassed (paid == false) directly, deduplicated by
    /// SurvivorTollRoster.RoomKeyFor so passing the SAME toll multiple times only counts once.
    /// **Known simplification, flagged explicitly:** doesn't verify arrival at the actual
    /// "Garbage Wastes Stronghold" room -- its exact room ID wasn't available this pass --
    /// completes as soon as the 3rd distinct unpaid toll is passed while the whole attempt has
    /// stayed inside Garbage Wastes (GW) during the starting cycle (cycleNumber == 0, checked
    /// during ordinary room-entry, not a Win-Prefix/Postfix -- no timing gotcha here, cycleNumber
    /// is stable for the whole cycle once set).</summary>
    internal sealed class CriminalTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Criminal");
        public override ChallengeEntry Challenge => _entry;

        private const string GarbageWastes = "GW";
        private const int RequiredUnpaidTolls = 3;

        private sealed class AttemptState
        {
            public readonly HashSet<string> UnpaidTollsPassed = new HashSet<string>();
        }

        private static readonly Dictionary<string, AttemptState> _attempts = new Dictionary<string, AttemptState>();

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || region == null)
                {
                    return;
                }

                if (region != GarbageWastes)
                {
                    _attempts.Remove(characterID);
                    return;
                }

                RainWorldGame game = GimmickGate.GameFor(player);
                if (game?.GetStorySession?.saveState?.cycleNumber != 0)
                {
                    _attempts.Remove(characterID);
                    return;
                }

                if (!_attempts.ContainsKey(characterID))
                {
                    _attempts[characterID] = new AttemptState();
                }
            };

            SurvivorTollState.OnTollPassed += (outpost, player, paid) =>
            {
                if (paid || !TryGetEnabledCharacter(player, out string characterID) || !_attempts.TryGetValue(characterID, out AttemptState state))
                {
                    return;
                }

                string roomKey = SurvivorTollRoster.RoomKeyFor(outpost);
                if (roomKey == null)
                {
                    return;
                }

                state.UnpaidTollsPassed.Add(roomKey);
                if (state.UnpaidTollsPassed.Count >= RequiredUnpaidTolls)
                {
                    ReportCompletion(player, characterID, isLive: true);
                    _attempts.Remove(characterID);
                }
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _attempts.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own room names: the 4 Industrial Complex Karma
    /// Gates are GATE_HI_GW, GATE_SU_HI, GATE_HI_CC, GATE_HI_SH. "You do not need to hibernate! You
    /// only need to cross the last Karma Gate" -- completes LIVE the instant the 4th distinct gate
    /// room is entered, no hibernation required, matching that note exactly. **Known
    /// simplification:** treats "entered the gate room" as "crossed the gate" -- the challenge's
    /// own wording is more precise ("crossing the middle line"), but gate rooms are typically
    /// simple single-purpose transition rooms where entering and crossing are practically the
    /// same event; Primal has no in-room-position data to check the literal middle line.</summary>
    internal sealed class NomadTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Nomad");
        public override ChallengeEntry Challenge => _entry;

        private static readonly string[] KarmaGates = { "GATE_HI_GW", "GATE_SU_HI", "GATE_HI_CC", "GATE_HI_SH" };

        private static readonly Dictionary<string, HashSet<string>> _gatesThisCycle = new Dictionary<string, HashSet<string>>();

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                bool isGate = false;
                foreach (string gate in KarmaGates)
                {
                    if (roomName == gate)
                    {
                        isGate = true;
                        break;
                    }
                }
                if (!isGate)
                {
                    return;
                }

                if (!_gatesThisCycle.TryGetValue(characterID, out HashSet<string> gates))
                {
                    gates = new HashSet<string>();
                    _gatesThisCycle[characterID] = gates;
                }
                gates.Add(roomName);

                if (gates.Count >= KarmaGates.Length)
                {
                    ReportCompletion(player, characterID, isLive: true);
                    _gatesThisCycle.Remove(characterID);
                }
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _gatesThisCycle.Remove(eventCharacterID);
                }
            };
            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _gatesThisCycle.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own room names: start/end shelter is SS_S05,
    /// checkpoints (already known from the original CSV's own Console Verification column) are
    /// SS_D04, SS_I03, SS_A03 in order. The first real consumer of SurvivorCheckpointRun, built
    /// during the original 8-mechanism batch specifically for this shape of challenge and never
    /// used until now. 3m45s = 225s = 9000 ticks at the confirmed 40 ticks/second nominal
    /// rate.</summary>
    internal sealed class CircuitTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Circuit");
        public override ChallengeEntry Challenge => _entry;

        private const string StartShelter = "SS_S05";
        private const int TimeLimitTicks = 225 * 40;

        private static readonly Dictionary<string, SurvivorCheckpointRun> _runs = new Dictionary<string, SurvivorCheckpointRun>();

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                if (!_runs.TryGetValue(characterID, out SurvivorCheckpointRun run))
                {
                    run = new SurvivorCheckpointRun("SS_D04", "SS_I03", "SS_A03");
                    _runs[characterID] = run;
                }

                if (roomName != StartShelter)
                {
                    run.NotifyRoomEntered(roomName);
                    return;
                }

                RainWorldGame game = GimmickGate.GameFor(player);
                if (run.IsRunning && run.AllCheckpointsHit)
                {
                    int? elapsed = run.ElapsedTicksIfComplete(game);
                    if (elapsed.HasValue && elapsed.Value <= TimeLimitTicks)
                    {
                        ReportCompletion(player, characterID, isLive: true);
                    }
                }
                run.Begin(game);
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null && _runs.TryGetValue(eventCharacterID, out SurvivorCheckpointRun run))
                {
                    run.Abort();
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own room name: "Deer Pearl room" is LF_D07.
    /// "Popcorn Plant" is the community nickname for vanilla's SeedCob (see
    /// SurvivorSeedCobEaten.cs's own doc comment for the full trace of why this needed a dedicated
    /// new hook -- SeedCob is never eaten through the ordinary Player.ObjectEaten path at all).
    /// Tracks 2 DISTINCT SeedCob instances eaten while inside LF_D07; "escape alive" is read as
    /// "successfully transitioned to a different room" (Player.NewRoom firing at all implies the
    /// player is alive and moving) -- checked the moment they leave, with progress reset on any
    /// departure, matching this as one continuous encounter rather than several separate
    /// visits.</summary>
    internal sealed class DesperadoTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Desperado");
        public override ChallengeEntry Challenge => _entry;

        private const string DeerPearlRoom = "LF_D07";
        private const int RequiredPlants = 2;

        private static readonly Dictionary<string, HashSet<int>> _eatenThisAttempt = new Dictionary<string, HashSet<int>>();

        public override void Register()
        {
            SurvivorSeedCobEaten.OnSeedCobEaten += (player, eventCharacterID, seedCob) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || player.room?.abstractRoom?.name != DeerPearlRoom)
                {
                    return;
                }

                if (!_eatenThisAttempt.TryGetValue(characterID, out HashSet<int> eaten))
                {
                    eaten = new HashSet<int>();
                    _eatenThisAttempt[characterID] = eaten;
                }
                eaten.Add(seedCob.abstractPhysicalObject.ID.number);
            };

            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || roomName == DeerPearlRoom)
                {
                    return;
                }

                if (_eatenThisAttempt.TryGetValue(characterID, out HashSet<int> eaten) && eaten.Count >= RequiredPlants)
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
                _eatenThisAttempt.Remove(characterID);
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _eatenThisAttempt.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own room name: the northeast Outskirts shelter is
    /// SU_S04. Same "loop return within a time limit" shape as Circuit, but with NO named
    /// checkpoints -- SurvivorCheckpointRun's own AllCheckpointsHit is vacuously true for a
    /// zero-length checkpoint array, so this falls out of the exact same code path Circuit uses
    /// with no special-casing needed. 1 minute = 2400 ticks, the same 40-ticks/second constant
    /// used throughout this codebase.</summary>
    internal sealed class UTurnTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("UTurn");
        public override ChallengeEntry Challenge => _entry;

        private const string StartShelter = "SU_S04";
        private const int TimeLimitTicks = 2400;

        private static readonly Dictionary<string, SurvivorCheckpointRun> _runs = new Dictionary<string, SurvivorCheckpointRun>();

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || roomName != StartShelter)
                {
                    return;
                }

                if (!_runs.TryGetValue(characterID, out SurvivorCheckpointRun run))
                {
                    run = new SurvivorCheckpointRun();
                    _runs[characterID] = run;
                }

                RainWorldGame game = GimmickGate.GameFor(player);
                if (run.IsRunning)
                {
                    int? elapsed = run.ElapsedTicksIfComplete(game);
                    if (elapsed.HasValue && elapsed.Value <= TimeLimitTicks)
                    {
                        ReportCompletion(player, characterID, isLive: true);
                    }
                }
                run.Begin(game);
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null && _runs.TryGetValue(eventCharacterID, out SurvivorCheckpointRun run))
                {
                    run.Abort();
                }
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own room names: westernmost Underhang shelter is
    /// UW_S01, shelter at the top of The Leg is UW_S07. "Squidcada" is the community nickname for
    /// vanilla's Cicada (CreatureTemplate.Type.CicadaA/CicadaB) -- both variants accepted, since
    /// the challenge doesn't specify which. "While starving" (Player.Malnourished) is checked
    /// continuously at every room transition during the carry, not just at the start or end --
    /// becoming well-fed at any point during the journey abandons the attempt, matching the
    /// literal "while starving" framing.</summary>
    internal sealed class SymbioteTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Symbiote");
        public override ChallengeEntry Challenge => _entry;

        private const string StartShelter = "UW_S01";
        private const string EndShelter = "UW_S07";

        private static string TrackerKeyFor(string characterID) => "Symbiote:" + characterID;

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
                    if (roomName != StartShelter || !player.Malnourished)
                    {
                        return;
                    }
                    AbstractCreature squidcada = FindCarriedSquidcada(player);
                    if (squidcada == null)
                    {
                        return;
                    }
                    SurvivorCreatureTracking.Touch(trackerKey, squidcada);
                    return;
                }

                if (!player.Malnourished || !SurvivorCreatureTracking.AllTrackedAlive(trackerKey))
                {
                    SurvivorCreatureTracking.ClearTracker(trackerKey);
                    return;
                }

                AbstractCreature tracked = FirstTracked(trackerKey);
                if (tracked == null || !SurvivorCreatureTracking.IsBeingCarriedBy(player, tracked))
                {
                    SurvivorCreatureTracking.ClearTracker(trackerKey);
                    return;
                }

                if (roomName == EndShelter)
                {
                    ReportCompletion(player, characterID, isLive: true);
                    SurvivorCreatureTracking.ClearTracker(trackerKey);
                }
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    SurvivorCreatureTracking.ClearTracker(TrackerKeyFor(eventCharacterID));
                }
            };
            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    SurvivorCreatureTracking.ClearTracker(TrackerKeyFor(eventCharacterID));
                }
            };
        }

        private static AbstractCreature FindCarriedSquidcada(Player player)
        {
            Creature.Grasp[] grasps = player?.grasps;
            if (grasps == null)
            {
                return null;
            }
            foreach (Creature.Grasp grasp in grasps)
            {
                if (!(grasp?.grabbed is Creature creature))
                {
                    continue;
                }
                CreatureTemplate.Type type = creature.abstractCreature?.creatureTemplate?.type;
                if (type == CreatureTemplate.Type.CicadaA || type == CreatureTemplate.Type.CicadaB)
                {
                    return creature.abstractCreature;
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
    }

    /// <summary>Added 2026-09-16, per the user's own room name: the eastmost Drainage System
    /// shelter is DS_S02. "Baby centipede" is CreatureTemplate.Type.SmallCentipede specifically
    /// (the "youth" size class -- distinct from plain Centipede and RedCentipede, per Ranger's own
    /// CSV note that these three count as distinct). Completion is checked at the moment of
    /// hibernation, verifying the SPECIFIC tracked instance is present in the shelter's own entity
    /// list (SurvivorShelterContents.EntitiesInShelter) rather than re-checking IsBeingCarriedBy at
    /// that exact instant -- more robust against however grasp state behaves during the sleep
    /// transition, the same reasoning Mother/Sleepover's own shelter-presence checks already
    /// use.</summary>
    internal sealed class ChargedTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Charged");
        public override ChallengeEntry Challenge => _entry;

        private const string TargetShelter = "DS_S02";

        private static string TrackerKeyFor(string characterID) => "Charged:" + characterID;

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
                    AbstractCreature baby = FindCarriedBabyCentipede(player);
                    if (baby == null)
                    {
                        return;
                    }
                    SurvivorCreatureTracking.Touch(trackerKey, baby);
                    return;
                }

                if (!SurvivorCreatureTracking.AllTrackedAlive(trackerKey))
                {
                    SurvivorCreatureTracking.ClearTracker(trackerKey);
                    return;
                }

                AbstractCreature tracked = FirstTracked(trackerKey);
                if (tracked == null || !SurvivorCreatureTracking.IsBeingCarriedBy(player, tracked))
                {
                    SurvivorCreatureTracking.ClearTracker(trackerKey);
                }
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                string trackerKey = TrackerKeyFor(characterID);
                bool completed = false;

                if (SurvivorShelterContents.ShelterRoomFor(player)?.name == TargetShelter
                    && SurvivorCreatureTracking.AllTrackedAlive(trackerKey))
                {
                    AbstractCreature tracked = FirstTracked(trackerKey);
                    if (tracked != null)
                    {
                        foreach (AbstractWorldEntity entity in SurvivorShelterContents.EntitiesInShelter(player))
                        {
                            if (entity == tracked)
                            {
                                completed = true;
                                break;
                            }
                        }
                    }
                }

                if (completed)
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
                SurvivorCreatureTracking.ClearTracker(trackerKey);
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    SurvivorCreatureTracking.ClearTracker(TrackerKeyFor(eventCharacterID));
                }
            };
        }

        private static AbstractCreature FindCarriedBabyCentipede(Player player)
        {
            Creature.Grasp[] grasps = player?.grasps;
            if (grasps == null)
            {
                return null;
            }
            foreach (Creature.Grasp grasp in grasps)
            {
                if (grasp?.grabbed is Creature creature
                    && creature.abstractCreature?.creatureTemplate?.type == CreatureTemplate.Type.SmallCentipede)
                {
                    return creature.abstractCreature;
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
    }

    /// <summary>Added 2026-09-16. "Place 15 unique objects/creatures into a shelter in one
    /// cycle." Direct consumer of SurvivorShelterContents.CountDistinct (shared mechanism #2,
    /// built in the original batch, this is its own first real consumer alongside Mother's
    /// CountObjectsOfType). Deduplicates by a combined key -- creature type (prefixed "C:") or
    /// item type (prefixed "I:") -- rather than by species/object roster position, so any 15
    /// distinct types (mixing creatures and items freely) satisfy it. **Known simplification,
    /// flagged explicitly:** the challenge's own rule is "every unique ICON is a unique object"
    /// -- this dedupes by raw type instead, since this mod has no icon-grouping data. The two
    /// could diverge if vanilla's own icon system groups some distinct types under one shared
    /// icon (or vice versa) -- not verified either way this pass.</summary>
    internal sealed class CatalogueTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Catalogue");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredUniqueObjects = 15;

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                if (SurvivorShelterContents.CountDistinct(player, KeyFor) >= RequiredUniqueObjects)
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };
        }

        private static string KeyFor(AbstractWorldEntity entity)
        {
            if (entity is AbstractCreature creature)
            {
                return creature.creatureTemplate?.type?.value == null ? null : "C:" + creature.creatureTemplate.type.value;
            }
            if (entity is AbstractPhysicalObject obj)
            {
                return obj.type?.value == null ? null : "I:" + obj.type.value;
            }
            return null;
        }
    }

    /// <summary>Added 2026-09-16. "Travel to Five Pebbles and then reach the final camera
    /// transition in the Void Sea in under 38 minutes IGT from game start." Reuses the same
    /// `standardVoidSea` ending flag PerseveranceTracker already consumes -- reaching this ending
    /// at all already implies having traveled through Five Pebbles, so no separate waypoint check
    /// is needed (same reasoning Perseverance's own doc comment already gives for "reaching
    /// Depths"). Confirmed via decompilation that RainWorldGame.clock (a plain public int,
    /// incremented exactly once per RawUpdate call, confirmed via `grep` to have no reset call
    /// anywhere in RainWorldGame.cs or SaveState.cs) accumulates for the WHOLE GAME SESSION from
    /// creation, never resetting per cycle -- exactly "IGT from game start", no extra state needed
    /// to track a start time separately. 38 minutes converts to 91200 ticks at the confirmed 40
    /// ticks/second nominal rate. **Known simplification, flagged explicitly:** the CSV's own
    /// notes say completion is really measured by "the Remix speedrun timer's value when it
    /// stops" -- this uses the mod's own raw `game.clock` read instead, which could diverge from
    /// that real timer (e.g. if the Remix timer excludes paused time this mod's own reading
    /// doesn't). "No skipping Guardians" is left as an honor-system note, the same treatment
    /// Perseverance's own "no Guardian skip" gets.</summary>
    internal sealed class AscensionTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Ascension");
        public override ChallengeEntry Challenge => _entry;

        private const int TimeLimitTicks = 38 * 60 * 40;

        public override void Register()
        {
            SurvivorEvents.OnGameEnding += (game, standardVoidSea) =>
            {
                if (!standardVoidSea || game?.Players == null || game.clock > TimeLimitTicks)
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

    /// <summary>Added 2026-09-16, per the user's own clarification: "story mode refers to game
    /// mode and we have only been in story mode -- basically just track if all lizard types got
    /// tamed in one run of one character." This is PER-CHARACTER, this mod's usual scope --
    /// confirming the original ratings pass's own "needs save-file-wide tracking across every
    /// Primal character" assumption was simply wrong, not a real architectural gap. No new
    /// persistence infrastructure needed: the existing per-character PrimalSaveMarkers is exactly
    /// right. Consumes the new SurvivorTaming.OnCreatureTamed, checking against the same 9-Lizard
    /// roster FoeTracker's own kill checklist already uses. Named `FriendChallengeTracker` rather
    /// than `FriendTracker` specifically to avoid colliding with vanilla's own `FriendTracker`
    /// class (the AI module SurvivorTaming.cs itself patches) -- both would otherwise share this
    /// namespace, and a same-named type here would shadow the vanilla one for any file in
    /// Primal.Gimmicks that references it by its bare name.</summary>
    internal sealed class FriendChallengeTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Friend");
        public override ChallengeEntry Challenge => _entry;

        private const string TamedMarkerKey = "PRIMAL_FRIEND_TAMED";

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
            SurvivorTaming.OnCreatureTamed += (tamer, tamed) =>
            {
                if (!(tamer is Player player) || !TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                CreatureTemplate.Type type = tamed?.abstractCreature?.creatureTemplate?.type;
                if (type?.value == null || System.Array.IndexOf(LizardTypes, type) < 0)
                {
                    return;
                }

                DeathPersistentSaveData saveData = PrimalSaveMarkers.SaveDataFor(GimmickGate.GameFor(player));
                if (saveData == null)
                {
                    return;
                }

                PrimalSaveMarkers.Add(saveData, PrimalSaveMarkers.Marker(TamedMarkerKey, characterID, type.value));

                foreach (CreatureTemplate.Type lizard in LizardTypes)
                {
                    if (!PrimalSaveMarkers.Has(saveData, PrimalSaveMarkers.Marker(TamedMarkerKey, characterID, lizard.value)))
                    {
                        return;
                    }
                }

                ReportCompletion(player, characterID, isLive: true);
            };
        }
    }

    /// <summary>Added 2026-09-16, per the user's own direction: "do not worry about uncompletable
    /// challenges... for all colors, use all colors accessible in vanilla, for survivor, monk and
    /// hunter." "Collect all unique coloured Pearls in one shelter" -- a direct consumer of
    /// SurvivorShelterContents.CountDistinct, keyed on AbstractDataPearl.dataPearlType (the
    /// pearl's own color/identity field, confirmed distinct from the generic AbstractObjectType
    /// every pearl shares) against the full roster from the new
    /// SurvivorVanillaProgress.AllDataPearlTypes(). Some registered colors may belong to one-off
    /// story pearls that can never appear as a loose collectible -- per the user's own explicit
    /// instruction, not worked around here (matching how several other challenges are only
    /// completable on a starting cycle and become permanently uncompletable afterward -- an
    /// accepted, not a fixed, property of some of these challenges).</summary>
    internal sealed class ScholarTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Scholar");
        public override ChallengeEntry Challenge => _entry;

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                int totalColors = SurvivorVanillaProgress.AllDataPearlTypes().Length;
                if (totalColors == 0)
                {
                    return;
                }

                int distinctInShelter = SurvivorShelterContents.CountDistinct(
                    player,
                    entity => (entity as DataPearl.AbstractDataPearl)?.dataPearlType?.value,
                    entity => entity is DataPearl.AbstractDataPearl);

                if (distinctInShelter >= totalColors)
                {
                    ReportCompletion(player, characterID, isLive: false);
                }
            };
        }
    }

    /// <summary>Added 2026-09-17. "Kit out any 6 Scavengers at any toll with Vulture Masks in one
    /// cycle" -- the "Outstanding items" list originally flagged this as needing a wholly new
    /// "is this Scavenger wearing a mask" mechanism (nothing like an equip/cosmetic slot exists for
    /// an ordinary Scavenger, only the King's own permanent mask sprite). Re-reading the CSV's own
    /// evidence requirement dissolves that gap instead: "a screenshot/short clip of 6 Scavengers
    /// HOLDING the Masks at a toll" -- this is a grasp, the same thing ScavengerAI.cs's own item-
    /// desirability table already scores VultureMask for (confirmed via decompilation -- masks are
    /// just another item Scavengers evaluate picking up, nothing cosmetic). Consumes the new
    /// SurvivorTollState.OnOutpostTick (see that event's own doc comment) to check, every tick,
    /// whether 6+ distinct live Scavengers physically present in an active outpost's own room
    /// (Room.updateList, the same list SurvivorTollState.OutpostIn already scans to find the
    /// outpost itself) are each currently grasping a VultureMask. A plain snapshot check, so
    /// "in one cycle" needs no separate scoping -- any single tick is inherently within one cycle --
    /// and "the masks can be gathered on any cycle" needs no handling either, since this only ever
    /// looks at the masks' CURRENT holders, never when or how they got there.</summary>
    internal sealed class ElderTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Elder");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredMaskedScavengers = 6;

        public override void Register()
        {
            SurvivorTollState.OnOutpostTick += (outpost) =>
            {
                Room room = outpost?.room;
                List<UpdatableAndDeletable> updateList = room?.updateList;
                if (updateList == null)
                {
                    return;
                }

                int maskedScavengers = 0;
                foreach (UpdatableAndDeletable obj in updateList)
                {
                    if (!(obj is Scavenger scavenger) || scavenger.dead)
                    {
                        continue;
                    }

                    Creature.Grasp[] grasps = scavenger.grasps;
                    if (grasps == null)
                    {
                        continue;
                    }

                    foreach (Creature.Grasp grasp in grasps)
                    {
                        if (grasp?.grabbed is VultureMask)
                        {
                            maskedScavengers++;
                            break;
                        }
                    }
                }

                if (maskedScavengers < RequiredMaskedScavengers)
                {
                    return;
                }

                RainWorldGame game = room.game;
                if (game?.Players == null)
                {
                    return;
                }

                foreach (AbstractCreature abstractPlayer in game.Players)
                {
                    if (!(abstractPlayer?.realizedCreature is Player player) || player.room != room)
                    {
                        continue;
                    }
                    if (TryGetEnabledCharacter(player, out string characterID))
                    {
                        ReportCompletion(player, characterID, isLive: true);
                    }
                }
            };
        }
    }

    /// <summary>Added 2026-09-17, room ID confirmed directly by the user: "Steal a pearl from the
    /// lowest Garbage Wastes Scavenger Toll without paying and hibernate with it before the rain
    /// starts." The lowest GW toll is room GW_C11 -- no simplification needed, unlike Criminal's
    /// still-unresolved "Stronghold" room. Consumes SurvivorTollState.OnItemStolen (a real
    /// EventID.Theft, independent of whether it happened to also cross the separate hostility-
    /// report threshold -- see that event's own doc comment) filtered to a DataPearl stolen
    /// specifically from GW_C11, then waits for OnSuccessfulHibernation with that EXACT pearl
    /// instance (AbstractDataPearl.ID.number, not just any pearl of any kind) present in the
    /// shelter -- the same "specific item instance, not just any item of that type" correlation
    /// FeastTracker/UsurperTracker already use. "Toll scavengers must become visibly aggressive"
    /// needs no separate check -- a real Theft SocialEvent already IS vanilla's own hostility
    /// trigger. "Must show The Chieftain progress at under half, requiring The Survivor passage" is
    /// a video-proof-format requirement for the community's own verification process, not
    /// observable game state -- left unenforced, same treatment as the CSV's other pure-
    /// documentation notes (e.g. Manslaughter's kill-credit guide link).</summary>
    internal sealed class HeistTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Heist");
        public override ChallengeEntry Challenge => _entry;

        private const string LowestGarbageWastesToll = "GW_C11";

        // Per characterID -- the stolen pearl's own EntityID.number, awaiting a hibernation with it
        // still in the shelter.
        private static readonly Dictionary<string, int> _stolenPearlByCharacter = new Dictionary<string, int>();

        public override void Register()
        {
            SurvivorTollState.OnItemStolen += (outpost, player, item) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || !(item is DataPearl pearl))
                {
                    return;
                }

                string roomKey = SurvivorTollRoster.RoomKeyFor(outpost);
                if (roomKey != LowestGarbageWastesToll)
                {
                    return;
                }

                _stolenPearlByCharacter[characterID] = pearl.AbstractPearl.ID.number;
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID)
                    || !_stolenPearlByCharacter.TryGetValue(characterID, out int pearlID))
                {
                    return;
                }

                foreach (AbstractWorldEntity entity in SurvivorShelterContents.EntitiesInShelter(player))
                {
                    if (entity is DataPearl.AbstractDataPearl pearl && pearl.ID.number == pearlID)
                    {
                        ReportCompletion(player, characterID, isLive: false);
                        _stolenPearlByCharacter.Remove(characterID);
                        return;
                    }
                }
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _stolenPearlByCharacter.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-17. "Triple-spawn Chimney Canopy, then survive the starve cycle" --
    /// the doc's own "Outstanding items" research pass had concluded this was "likely a signature-
    /// less emergent bug" needing either a concrete observable side effect or a decision to leave
    /// it untracked. Per the user's own direction (2026-09-17): the tracker IS the kill -- "you
    /// killing those 3 pole plants that all come from the same den," matching the CSV's own
    /// secondary evidence line ("short clip of killing 3 Pole Plants spawned in the same den").
    /// A triple-spawned region manifests as its own normally-unique objects (a den's one Pole
    /// Plant -- CreatureTemplate.Type.PoleMimic, the same class FlytrapTracker/
    /// SurvivorPoleMimicPredation already use for the community's "Pole Plant" name) existing 3
    /// times at once, all anchored to the SAME den. "Same den" is identified by
    /// (room name, AbstractCreature.pos.abstractNode) -- abstractNode is the specific den/burrow
    /// node a creature's WorldCoordinate resolves to, and a triple-spawn's 3 layered copies of one
    /// den's content all resolve to that identical node, unlike 3 incidental, unrelated PoleMimics
    /// which would each belong to their own separate den node.
    ///
    /// Detected via its own Postfix on Creature.Die (TriplopiaPoleMimicDeathHook, below) rather
    /// than reusing PrimalKillTracker's own per-type/per-instance records (those answer "was type X
    /// / instance N killed", not "which den did it belong to", which this challenge specifically
    /// needs) -- the killer-attribution logic (Creature.killTag, falling back to
    /// PrimalKillTracker.TryGetRecentHandler) is duplicated from SurvivorTollRosterDeathHook's own
    /// copy of the same pattern rather than depending on execution order against
    /// PrimalKillDieHook's own independent Postfix on the same method. Own ConditionalWeakTable
    /// guards against Creature.Die's own documented "can run more than once per instance"
    /// behavior double-counting a single death.
    ///
    /// **Known simplification, flagged explicitly:** does not verify "survive the starve cycle" (no
    /// malnourishment/forced-cycle check) and does not require the original triple-spawn/starve
    /// cycle and the 3 kills to happen on separate cycles the way the CSV's own two-part
    /// verification implies -- per the user's own framing, the kill IS the signal this mod
    /// tracks.</summary>
    internal sealed class TriplopiaTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Triplopia");
        public override ChallengeEntry Challenge => _entry;

        internal const string ChimneyCanopy = "CC";
        private const int RequiredPoleMimicKills = 3;

        // Per "characterID|denKey" -- distinct PoleMimic instances (EntityID.number) killed from
        // that one den so far. Session-only, in-memory, matching SurvivorTollRoster's own scoping
        // for the same kind of persistent-until-completed roster.
        private static readonly Dictionary<string, HashSet<int>> _killedByDen = new Dictionary<string, HashSet<int>>();

        public override void Register()
        {
            // No SurvivorEvents subscription -- detection happens in
            // TriplopiaPoleMimicDeathHook's Postfix, below, which calls straight into
            // OnPoleMimicKilled.
        }

        internal static void OnPoleMimicKilled(Player player, string denKey, int instanceNumber)
        {
            if (!GimmickGate.IsActiveFor(player, CommunityChallengesGimmick.ID, out string characterID)
                || !CommunityChallengesSettings.IsChallengeEnabled(characterID, _entry.ID))
            {
                return;
            }

            string key = characterID + "|" + denKey;
            if (!_killedByDen.TryGetValue(key, out HashSet<int> killed))
            {
                killed = new HashSet<int>();
                _killedByDen[key] = killed;
            }
            killed.Add(instanceNumber);

            if (killed.Count >= RequiredPoleMimicKills)
            {
                ChallengeCompletion.ReportCompletion(player, characterID, _entry.ID, isLive: true);
            }
        }
    }

    /// <summary>See TriplopiaTracker's own doc comment for the full trace of why this duplicates
    /// SurvivorTollRosterDeathHook's own killer-attribution logic rather than depending on
    /// execution order against PrimalKillDieHook, another independent Postfix on this same
    /// method.</summary>
    [HarmonyPatch(typeof(Creature), nameof(Creature.Die))]
    internal static class TriplopiaPoleMimicDeathHook
    {
        private static readonly ConditionalWeakTable<Creature, object> _processed =
            new ConditionalWeakTable<Creature, object>();

        private static void Postfix(Creature __instance)
        {
            if (!(__instance is PoleMimic mimic) || !mimic.dead || _processed.TryGetValue(mimic, out _))
            {
                return;
            }

            AbstractCreature abstractMimic = mimic.abstractCreature;
            if (abstractMimic?.world?.name != TriplopiaTracker.ChimneyCanopy)
            {
                return;
            }

            AbstractCreature killer = PrimalKillTracker.KillTagField?.GetValue(mimic) as AbstractCreature;
            if (killer == null)
            {
                int? tick = abstractMimic.world?.game?.clock;
                if (tick.HasValue)
                {
                    PrimalKillTracker.TryGetRecentHandler(mimic, tick.Value, out killer);
                }
            }

            if (!(killer?.realizedCreature is Player player))
            {
                return;
            }

            _processed.Add(mimic, null);

            string denKey = abstractMimic.Room?.name + "#" + abstractMimic.pos.abstractNode;
            TriplopiaTracker.OnPoleMimicKilled(player, denKey, abstractMimic.ID.number);
        }
    }

    /// <summary>Added 2026-09-17. "Get 10k+ points in a Hunter run" -- the doc's own earlier
    /// research pass found no lead at all after searching WinState.CycleCompleted and
    /// RedsIllness.cs, and flagged the real scoring formula as possibly living in an undecompiled
    /// DLC assembly. It doesn't -- it's in Menu.StoryGameStatisticsScreen.ScoreKeeper, the vanilla
    /// end-of-campaign "final score screen" the CSV's own Notes column already points at
    /// ("Screenshot of final score screen"). Traced via decompilation
    /// (TickerIsDone/ScoreKeeper.AddMulti/ScoreKeeper.Update): the displayed number is
    /// sum(food*1 + survivedCycles*10 + deaths*-3 + quits*-3 + (totalSeconds/60)*-1 +
    /// perKillCreatureValue) -- vanilla's own GetNonSandboxKillscore/killScores table, a large
    /// hardcoded list not worth re-deriving -- then MULTIPLIED by (1 + however many endgame
    /// passages are already fulfilled when the screen tallies up), confirming the CSV's own "follow
    /// other standard scorerunning rules" hint. Rather than re-implementing that formula (and its
    /// large kill-value table) from scratch, this reads vanilla's own already-computed answer
    /// straight off ScoreKeeper.score once the screen's own tally/multiplier animation finishes
    /// (StoryGameStatisticsScreen.allDone) -- the same "ride vanilla's own tested machinery instead
    /// of reimplementing it" preference this whole mod follows elsewhere.
    ///
    /// This is the first ChallengeTracker whose own detection point has no live Player to hang the
    /// usual TryGetEnabledCharacter/ReportCompletion pair off -- by the time this screen exists, the
    /// story session is fully over. Uses PrimalMenuAccess (extended this same pass with
    /// allDone/scoreKeeper/ScoreKeeper.score reflected accessors, on the same Gotcha #1 reasoning
    /// its existing GetSaveState/GetCharacterID already apply to this exact screen family) plus the
    /// new GimmickGate.IsEnabledForCharacter/ChallengeCompletion.ReportCompletionForSave menu-
    /// context pair instead -- CommunityChallengesButtonHook already established this same
    /// no-Player-available pattern one screen earlier in the story-end flow (the cycle-end
    /// CHALLENGES button). No live notification fires here (no HUD exists on this screen) --
    /// completion here is silent until the player's next actual cycle-end summary reads the
    /// persisted flag.
    ///
    /// Registered via its own Harmony patch (OutlawScoreScreenHook, below) rather than a
    /// SurvivorEvents subscription, since Register() has nothing gameplay-side to subscribe to.</summary>
    internal sealed class OutlawTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Outlaw");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredScore = 10000;

        public override void Register()
        {
            // Nothing to subscribe to here -- see this class's own doc comment. Detection happens
            // in OutlawScoreScreenHook's Postfix, below.
        }

        internal static void CheckScoreScreen(Menu.StoryGameStatisticsScreen screen)
        {
            if (screen == null || !PrimalMenuAccess.IsScoreScreenDone(screen))
            {
                return;
            }

            string characterID = PrimalMenuAccess.GetCharacterID(screen);
            RainWorld rainWorld = screen.manager?.rainWorld;
            if (characterID != SlugcatStats.Name.Red.value || rainWorld == null)
            {
                return;
            }

            if (!GimmickGate.IsEnabledForCharacter(rainWorld, characterID, CommunityChallengesGimmick.ID)
                || !CommunityChallengesSettings.IsChallengeEnabled(characterID, _entry.ID))
            {
                return;
            }

            int? score = PrimalMenuAccess.GetFinalScore(screen);
            if (!score.HasValue || score.Value < RequiredScore)
            {
                return;
            }

            DeathPersistentSaveData saveData = PrimalMenuAccess.GetSaveState(screen)?.deathPersistentSaveData;
            ChallengeCompletion.ReportCompletionForSave(saveData, characterID, _entry.ID);
        }
    }

    /// <summary>See OutlawTracker's own doc comment for the full trace of why this challenge is
    /// detected from a menu screen Postfix rather than a SurvivorEvents subscription.</summary>
    [HarmonyPatch(typeof(Menu.StoryGameStatisticsScreen), nameof(Menu.StoryGameStatisticsScreen.Update))]
    internal static class OutlawScoreScreenHook
    {
        private static void Postfix(Menu.StoryGameStatisticsScreen __instance)
        {
            OutlawTracker.CheckScoreScreen(__instance);
        }
    }

    /// <summary>Added 2026-09-17, room IDs confirmed by the user: "Reach the Depths from the
    /// highest shelter in Five Pebbles in one cycle" -- the highest shelter is SS_S04, the Depths
    /// entry room is SB_E05 (already established by DevoteeTracker). Closest existing analog is
    /// actually DevoteeTracker's own HomeShelter/DepthsReached shape, not UTurnTracker as originally
    /// guessed in the doc -- Journey is a one-way "started this cycle at shelter X, reach checkpoint
    /// Y before the cycle ends" check, with no round trip and no time limit, so
    /// SurvivorCheckpointRun's loop-timing machinery (built for U-Turn/Circuit) doesn't apply here.
    /// "Started this cycle at the highest shelter" is read off the hibernation that ends the
    /// PREVIOUS cycle: hibernating in SS_S04 arms the NEXT cycle's watch for SB_E05, mirroring
    /// exactly how DevoteeTracker's own HomeShelter locks in off OnSuccessfulHibernation. Persists
    /// across cycles until either Depths is reached or a hibernation happens somewhere else
    /// (disarming it) -- matching "on any cycle" wording (no CSV cycle-number requirement, unlike
    /// e.g. Criminal's explicit "starting cycle").</summary>
    internal sealed class JourneyTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Journey");
        public override ChallengeEntry Challenge => _entry;

        private const string HighestFivePebblesShelter = "SS_S04";
        private const string DepthsRoom = "SB_E05";

        // Per characterID -- true once a cycle has begun (via hibernation) at the highest shelter,
        // awaiting Depths this same cycle.
        private static readonly HashSet<string> _armed = new HashSet<string>();

        public override void Register()
        {
            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                string shelter = SurvivorShelterContents.ShelterRoomFor(player)?.name;
                if (shelter == HighestFivePebblesShelter)
                {
                    _armed.Add(characterID);
                }
                else
                {
                    _armed.Remove(characterID);
                }
            };

            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || !_armed.Contains(characterID))
                {
                    return;
                }

                if (roomName == DepthsRoom)
                {
                    ReportCompletion(player, characterID, isLive: true);
                    _armed.Remove(characterID);
                }
            };

            SurvivorEvents.OnCycleFailed += (game, player, eventCharacterID) =>
            {
                if (eventCharacterID != null)
                {
                    _armed.Remove(eventCharacterID);
                }
            };
        }
    }

    /// <summary>Added 2026-09-17, per the user's own confirmation and a follow-up decompilation
    /// pass: "After leaving the tutorial area, re-enter and return to the platform you first spawn
    /// on before the rain starts, on any cycle." The user identified the spawn room as SU_C04 and
    /// asked for more on it -- decompiling turned up the full picture: SU_C04 is vanilla's own
    /// canonical story-mode starting den for every character except Hunter (LF_H01) and MSC's
    /// Sofanthiel (SH_E01) (SaveState.GetStoryDenPosition's own fallback case), and it hosts a
    /// dedicated startup script (RoomSpecificScript.SU_C04StartUp) that pushes the player rightward
    /// and tracks when they "stand up" -- the literal "out of control, moving out of the top-left
    /// corner" intro sequence the user described. Even better, vanilla defines the WHOLE tutorial
    /// area's own room list directly: OverseerAbstractAI.DefineTutorialRooms's vanilla (non-MMF)
    /// branch is exactly `{ "SU_C04", "SU_A41", "SU_A42", "SU_A43", "SU_A44", "SU_A22" }` -- the
    /// MMF-only 2 extra gate rooms are skipped since MSC/MMF must be disabled for every one of
    /// these challenges anyway (community_challenges_source.csv's own header note). "Left the
    /// tutorial area" is checked as "entered a room outside that 6-room list" (Outskirts, SU, is a
    /// much bigger region than just these 6 tutorial rooms, so this is a real, meaningful
    /// boundary, not just "left SU entirely"). Persists across cycles ("on any cycle" -- no
    /// per-cycle reset), matching Journey's own "on any cycle" treatment above.</summary>
    internal sealed class VisitorTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Visitor");
        public override ChallengeEntry Challenge => _entry;

        private const string SpawnRoom = "SU_C04";

        private static readonly HashSet<string> TutorialRooms = new HashSet<string>
        {
            "SU_C04", "SU_A41", "SU_A42", "SU_A43", "SU_A44", "SU_A22",
        };

        // Per characterID -- has this character ever entered a room outside the tutorial area
        // since this tracker started watching.
        private static readonly HashSet<string> _hasLeftTutorial = new HashSet<string>();

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID) || roomName == null)
                {
                    return;
                }

                if (!TutorialRooms.Contains(roomName))
                {
                    _hasLeftTutorial.Add(characterID);
                    return;
                }

                if (roomName == SpawnRoom && _hasLeftTutorial.Contains(characterID))
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
            };
        }
    }

    /// <summary>Added 2026-09-17, per the user's own clarification: "Bring 7 Hazers from unique
    /// spawn locations to the same body of water, alive." Rather than needing a Hazer den map (the
    /// original blocker), the user confirmed distinct Hazer INSTANCES are the right proxy for
    /// "unique spawn locations" -- explicitly including the case of grabbing 2 Hazers from the same
    /// room and bringing both, which still counts as 2 of the 7 (no per-origin-room dedup).
    /// Checked as a plain snapshot (the same "N creatures alive together in one room" shape
    /// Safari/Elder already use) on every room arrival: does the player's CURRENT room have water
    /// (Room.water, confirmed public) and contain 7+ live Hazers (CreatureTemplate.Type.Hazer)?
    /// Doesn't require a single trip -- if Hazers accumulate in one water room across several
    /// separate carries until 7 are alive there together, that satisfies "brought 7 ... to the same
    /// body of water" exactly as well as one trip with all 7 in hand.</summary>
    internal sealed class FishtankTracker : ChallengeTracker
    {
        private static readonly ChallengeEntry _entry = CommunityChallengesRegistry.Find("Fishtank");
        public override ChallengeEntry Challenge => _entry;

        private const int RequiredHazers = 7;

        public override void Register()
        {
            SurvivorEvents.OnPlayerEnteredRoom += (player, eventCharacterID, roomName, region) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }

                Room room = player.room;
                if (room == null || !room.water)
                {
                    return;
                }

                List<AbstractWorldEntity> entities = room.abstractRoom?.entities;
                if (entities == null)
                {
                    return;
                }

                int liveHazers = 0;
                foreach (AbstractWorldEntity entity in entities)
                {
                    if (entity is AbstractCreature creature && !creature.state.dead
                        && creature.creatureTemplate?.type == CreatureTemplate.Type.Hazer)
                    {
                        liveHazers++;
                    }
                }

                if (liveHazers >= RequiredHazers)
                {
                    ReportCompletion(player, characterID, isLive: true);
                }
            };
        }
    }
}
