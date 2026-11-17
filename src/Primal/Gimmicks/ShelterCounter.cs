using System;
using System.Collections.Generic;
using HarmonyLib;
using RWCustom;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shelter counter for SleepEveryShelterRequirement (added 2026-11-17, explicit request): when
    /// the player is in a shelter, a "SHELTERS  slept / total" counter fades in at the top of the
    /// screen, and fades back out on leaving. Shown only on the Primal slot, for a character that
    /// has Sleep In Every Shelter assigned (Rivulet by default).
    ///
    /// **The total** (ShelterTotals) is every shelter in every one of this character's story regions
    /// (`SlugcatStats.SlugcatStoryRegions` -- 14 for Rivulet: SU HI DS CC GW SH VS SL SI LF UW RM SB
    /// MS), counted by exactly the same rules SleepEveryShelterRequirement uses, so the two always
    /// agree: `World.shelters`, minus `World.DisabledMapRooms` (rooms the timeline hides), minus
    /// shelters the timeline itself keeps permanently broken (the region's "Broken Shelters" map
    /// config line). Shelter names are de-duplicated across regions (a gate-shelter can belong to
    /// two regions; one sleep covers both).
    ///
    /// Rather than re-parsing world files by hand (conditional/timeline lines, EXCLUSIVEROOM /
    /// HIDEROOM, gate shelters, mod-merged files -- easy to get subtly wrong), it builds each
    /// region's World with vanilla's own WorldLoader in LoadingContext.FASTTRAVEL -- exactly what
    /// the passage screen (FastTravelScreen) does, with game == null, which skips creatures and
    /// other gameplay setup. Driven one Update() per frame from a RainWorldGame.Update Postfix,
    /// one region after another, the same non-blocking way OverWorld drives a region-gate load
    /// (`NextActivity()` once, then `Update()` each frame until `Finished`), so it never hitches
    /// the game. Starts as soon as a qualifying game session runs, and is cached for the rest of
    /// the process (world files don't change while the game is running).
    ///
    /// Building those Worlds also fires PermanentShelterFailureFastTravelHook (World
    /// .LoadWorldForFastTravel Postfix), which force-breaks slept-in shelters on them -- harmless
    /// here: the vanilla-broken check below reads ShelterSleepRecords' pre-forcing snapshot.
    ///
    /// **Slept** is how many of those counted shelters have a ShelterSleepRecords marker for this
    /// character on this save. Re-counted on entering a shelter and once a second while shown.
    /// </summary>
    internal static class ShelterTotals
    {
        private sealed class Job
        {
            public string CharacterID;
            public SlugcatStats.Name Character;
            public List<string> Regions;
            public int RegionIndex;
            public WorldLoader Loader;
            public readonly HashSet<string> Shelters = new HashSet<string>();
            public readonly List<string> FailedRegions = new List<string>();
            public bool Done;
        }

        private static readonly Dictionary<string, Job> _jobs = new Dictionary<string, Job>();

        /// <summary>True once every region has been counted. `shelters` is the de-duplicated set
        /// of counted shelter room names.</summary>
        public static bool TryGetShelters(string characterID, out HashSet<string> shelters)
        {
            if (characterID != null && _jobs.TryGetValue(characterID, out Job job) && job.Done)
            {
                shelters = job.Shelters;
                return true;
            }
            shelters = null;
            return false;
        }

        /// <summary>Advances the background count by one step. Called every frame.</summary>
        internal static void Step(RainWorldGame game)
        {
            if (game == null || !game.IsStorySession || game.rainWorld == null || !PrimalSlot.IsActive(game.rainWorld))
            {
                return;
            }

            SlugcatStats.Name character = game.StoryCharacter;
            string characterID = character?.value;
            if (characterID == null || !GimmickAssignments.IsEnabled(characterID, SleepEveryShelterRequirement.RequirementID))
            {
                return;
            }

            if (!_jobs.TryGetValue(characterID, out Job job))
            {
                job = new Job
                {
                    CharacterID = characterID,
                    Character = character,
                    Regions = SlugcatStats.SlugcatStoryRegions(character),
                };
                _jobs[characterID] = job;
                Plugin.Log?.LogInfo($"[ShelterCounter] {characterID}: counting shelters in {job.Regions.Count} story regions: {string.Join(" ", job.Regions)}");
            }
            if (job.Done)
            {
                return;
            }

            try
            {
                StepJob(game, job);
            }
            catch (Exception ex)
            {
                string region = job.RegionIndex < job.Regions.Count ? job.Regions[job.RegionIndex] : "?";
                Plugin.Log?.LogError($"[ShelterCounter] {characterID}: counting {region} failed, skipping it: {ex}");
                job.FailedRegions.Add(region);
                job.Loader = null;
                job.RegionIndex++;
                FinishIfDone(job);
            }
        }

        private static void StepJob(RainWorldGame game, Job job)
        {
            if (job.Loader == null)
            {
                if (job.RegionIndex >= job.Regions.Count)
                {
                    FinishIfDone(job);
                    return;
                }

                string regionName = job.Regions[job.RegionIndex];
                Region region = FindRegion(game, regionName);
                if (region == null)
                {
                    Plugin.Log?.LogWarning($"[ShelterCounter] {job.CharacterID}: region {regionName} not found in this game's region list -- skipped");
                    job.FailedRegions.Add(regionName);
                    job.RegionIndex++;
                    FinishIfDone(job);
                    return;
                }

                job.Loader = new WorldLoader(null, job.Character, SlugcatStats.SlugcatToTimeline(job.Character), false, region.name, region, game.setupValues, WorldLoader.LoadingContext.FASTTRAVEL);
                job.Loader.NextActivity();
                return;
            }

            job.Loader.Update();
            if (!job.Loader.Finished)
            {
                return;
            }

            World world = job.Loader.ReturnWorld();
            job.Loader = null;
            int added = CountWorld(world, job.Shelters);
            Plugin.Log?.LogInfo($"[ShelterCounter] {job.CharacterID}: {job.Regions[job.RegionIndex]} has {added} counted shelter(s)");
            job.RegionIndex++;
            FinishIfDone(job);
        }

        private static void FinishIfDone(Job job)
        {
            if (job.RegionIndex < job.Regions.Count)
            {
                return;
            }
            job.Done = true;
            string failed = job.FailedRegions.Count > 0 ? $" (could not count: {string.Join(" ", job.FailedRegions)})" : "";
            Plugin.Log?.LogInfo($"[ShelterCounter] {job.CharacterID}: total {job.Shelters.Count} shelter(s){failed}");
        }

        private static Region FindRegion(RainWorldGame game, string regionName)
        {
            Region[] regions = game.overWorld?.regions;
            if (regions == null)
            {
                return null;
            }
            foreach (Region region in regions)
            {
                if (region != null && string.Equals(region.name, regionName, StringComparison.OrdinalIgnoreCase))
                {
                    return region;
                }
            }
            return null;
        }

        /// <summary>Same exclusions as SleepEveryShelterRequirement.AllSheltersSleptInLoadedRegion.
        /// Returns how many new names were added.</summary>
        private static int CountWorld(World world, HashSet<string> into)
        {
            int added = 0;
            if (world?.shelters == null)
            {
                return 0;
            }
            foreach (int roomIndex in world.shelters)
            {
                AbstractRoom shelterRoom = world.GetAbstractRoom(roomIndex);
                if (shelterRoom == null)
                {
                    continue;
                }
                if (world.DisabledMapRooms != null && world.DisabledMapRooms.Contains(shelterRoom.name))
                {
                    continue;
                }
                if (ShelterSleepRecords.IsVanillaBrokenOrUnforced(world, shelterRoom.shelterIndex))
                {
                    continue;
                }
                if (into.Add(shelterRoom.name))
                {
                    added++;
                }
            }
            return added;
        }

        public static int CountSlept(DeathPersistentSaveData saveData, string characterID, HashSet<string> shelters)
        {
            int slept = 0;
            foreach (string name in shelters)
            {
                if (ShelterSleepRecords.HasSlept(saveData, characterID, name))
                {
                    slept++;
                }
            }
            return slept;
        }
    }

    /// <summary>The fading "SHELTERS  slept / total" label. See ShelterTotals for the numbers.</summary>
    internal sealed class ShelterCounterHud : HUD.HudPart
    {
        private const float FadeSpeed = 1f / 30f;   // ~0.75 s fade at 40 ticks/s
        private const int RecountIntervalTicks = 40;

        private readonly FLabel _label;
        private float _alpha;
        private float _lastAlpha;
        private bool _wasInShelter;
        private int _recountCounter;

        public ShelterCounterHud(HUD.HUD hud) : base(hud)
        {
            _label = new FLabel(Custom.GetDisplayFont(), "")
            {
                alpha = 0f,
                color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White),
            };
            hud.fContainers[1].AddChild(_label);
        }

        public override void Update()
        {
            base.Update();
            _lastAlpha = _alpha;

            Player player = hud.owner as Player;
            RainWorldGame game = player?.abstractCreature?.world?.game;
            string characterID = game?.StoryCharacter?.value;
            bool active = game != null
                && game.IsStorySession
                && PrimalSlot.IsActive(game.rainWorld)
                && characterID != null
                && GimmickAssignments.IsEnabled(characterID, SleepEveryShelterRequirement.RequirementID);
            bool inShelter = active
                && !player.inShortcut
                && player.room?.abstractRoom != null
                && player.room.abstractRoom.shelter;

            if (inShelter && (!_wasInShelter || ++_recountCounter >= RecountIntervalTicks))
            {
                _recountCounter = 0;
                _label.text = BuildText(game, characterID);
            }
            _wasInShelter = inShelter;

            _alpha = Mathf.Clamp01(_alpha + (inShelter ? FadeSpeed : -FadeSpeed));
        }

        private static string BuildText(RainWorldGame game, string characterID)
        {
            if (!ShelterTotals.TryGetShelters(characterID, out HashSet<string> shelters))
            {
                return "SHELTERS  counting...";
            }
            int slept = ShelterTotals.CountSlept(PrimalSaveMarkers.SaveDataFor(game), characterID, shelters);
            return $"SHELTERS  {slept} / {shelters.Count}";
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            _label.x = hud.rainWorld.screenSize.x / 2f + 0.01f;
            _label.y = hud.rainWorld.screenSize.y - 40f - hud.rainWorld.options.SafeScreenOffset.y + 0.01f;
            _label.alpha = Mathf.Lerp(_lastAlpha, _alpha, timeStacker);
        }

        public override void ClearSprites()
        {
            base.ClearSprites();
            _label.RemoveFromContainer();
        }
    }

    /// <summary>Adds ShelterCounterHud to a single-player story HUD on the Primal slot. Whether it
    /// actually shows is decided live in its own Update, so toggling the requirement in Remix
    /// works without a restart.</summary>
    [HarmonyPatch(typeof(HUD.HUD), nameof(HUD.HUD.InitSinglePlayerHud))]
    internal static class ShelterCounterHudHook
    {
        private static void Postfix(HUD.HUD __instance)
        {
            if (__instance.owner is Player player
                && player.abstractCreature?.world?.game is RainWorldGame game
                && game.IsStorySession
                && PrimalSlot.IsActive(game.rainWorld))
            {
                __instance.AddPart(new ShelterCounterHud(__instance));
            }
        }
    }

    /// <summary>Drives ShelterTotals' background count one step per frame.</summary>
    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.Update))]
    internal static class ShelterTotalsUpdateHook
    {
        private static void Postfix(RainWorldGame __instance)
        {
            ShelterTotals.Step(__instance);
        }
    }
}
