using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using RWCustom;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shelter counter for SleepEveryShelterRequirement (added 2026-11-17, explicit request): when
    /// the player is in a shelter, a bare "slept / total" counter fades in at the TOP RIGHT of the
    /// screen, and fades back out on leaving. Shown only on the Primal slot, for a character that
    /// has Sleep In Every Shelter assigned (Rivulet by default).
    ///
    /// It read "SHELTERS  slept / total" centred at the top until later the same day, when both
    /// the caption and the centre position were dropped by explicit request -- see BuildText and
    /// TopInset/RightInset, which also record the campaign-timer collision the centre position
    /// caused.
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
            public bool LoadedFromCache;
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

                // The whole point of ShelterCountCache: if the mod set hasn't changed since the
                // last run, the answer is already known and not one World gets built.
                if (ShelterCountCache.TryGet(characterID, out HashSet<string> cached))
                {
                    job.Shelters.UnionWith(cached);
                    job.RegionIndex = job.Regions.Count;
                    job.Done = true;
                    job.LoadedFromCache = true;
                    Plugin.Log?.LogInfo($"[ShelterCounter] {characterID}: {job.Shelters.Count} shelter(s) from cache -- skipping the region scan");
                    return;
                }

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

            // Only persist a complete count. A run that skipped regions would otherwise cache a
            // number that is wrong and, worse, sticky -- it would survive every later launch
            // until some unrelated mod changed the fingerprint.
            if (job.FailedRegions.Count == 0)
            {
                ShelterCountCache.Store(job.CharacterID, job.Shelters);
            }
            else
            {
                Plugin.Log?.LogWarning($"[ShelterCounter] {job.CharacterID}: not caching an incomplete count -- it will be recounted next launch");
            }
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

    /// <summary>
    /// On-disk cache for ShelterTotals (2026-11-17, explicit request: "can we cache the whole
    /// shelter analysis thing, since it lags every cycle. run it only after detecting mods
    /// changed").
    ///
    /// **What was actually slow.** ShelterTotals already cached its result in a static for the
    /// life of the process, so it did NOT recount every cycle -- confirmed from
    /// BepInEx/LogOutput.log, which shows exactly one "counting shelters in 14 story regions"
    /// block and one "total 109" per launch. What it did do is rebuild all 14 regions' Worlds
    /// with WorldLoader on every GAME LAUNCH, which is the hitch that gets noticed early in a
    /// session. This cache removes that: after the first run the counts come back from a file and
    /// no World is ever built.
    ///
    /// **When it recomputes.** Only when the mod set changes. The fingerprint is built from
    /// `ModManager.ActiveMods` -- each active mod's `id`, `version`, `checksum` and `loadOrder`,
    /// sorted by id -- so installing, removing, updating, reordering or editing the CONTENTS of
    /// any mod invalidates it. `checksum` is the game's own per-mod content hash (ModManager.cs
    /// computes it and uses it for its own `checksumChanged` flag), so a mod edited in place is
    /// caught too, not just a version bump. Load order is in there because region merging is
    /// order-dependent.
    ///
    /// Deliberately NOT narrowed to mods with `modifiesRegions` set: that flag is self-reported
    /// by each mod's own metadata, and a wrong count here is silent and sticky (it persists
    /// across launches until something else changes), which is a far worse failure than
    /// occasionally recounting after an unrelated mod updates. Fingerprinting everything costs
    /// one extra count on launches where any mod changed at all.
    ///
    /// **Where.** `Application.persistentDataPath/Primal/shelter-counts.txt` -- Unity's
    /// per-user writable location, used instead of the game folder (which is under Program Files
    /// on a default Windows install and not writable without elevation) and instead of the
    /// game's own `UserData.GetPersistentDataPath()`, which isn't in the reference assembly this
    /// project compiles against.
    ///
    /// Every failure path here is non-fatal: a missing, unreadable, truncated or stale file just
    /// means "no cache", and the normal background count runs exactly as it did before.
    /// </summary>
    internal static class ShelterCountCache
    {
        private const string FileFormatVersion = "v1";

        private static string _fingerprint;
        private static Dictionary<string, HashSet<string>> _loaded;
        private static string _loadedFingerprint;

        private static string CacheFilePath =>
            Path.Combine(Path.Combine(Application.persistentDataPath, "Primal"), "shelter-counts.txt");

        /// <summary>Identifies the current mod set. Computed once per process -- the active mod
        /// list can't change without a restart.</summary>
        private static string Fingerprint()
        {
            if (_fingerprint != null)
            {
                return _fingerprint;
            }

            var parts = new List<string>();
            try
            {
                List<ModManager.Mod> mods = ModManager.ActiveMods;
                if (mods != null)
                {
                    foreach (ModManager.Mod mod in mods)
                    {
                        if (mod != null)
                        {
                            parts.Add($"{mod.id}:{mod.version}:{mod.checksum}:{mod.loadOrder}");
                        }
                    }
                }
                parts.Sort(StringComparer.Ordinal);
            }
            catch (Exception ex)
            {
                // A fingerprint we can't compute must never silently match a stored one, or a
                // stale count would stick forever. A unique value forces a recount instead.
                Plugin.Log?.LogWarning($"[ShelterCounter] could not fingerprint the mod list ({ex.Message}) -- not using the cache this run");
                _fingerprint = "unavailable-" + Guid.NewGuid().ToString("N");
                return _fingerprint;
            }

            _fingerprint = Hash(string.Join("\n", parts.ToArray()));
            return _fingerprint;
        }

        private static string Hash(string value)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
                var sb = new System.Text.StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>Reads the file once per process. Returns false for a missing file, an
        /// unreadable one, or one written under a different mod set.</summary>
        private static bool EnsureLoaded()
        {
            string fingerprint = Fingerprint();
            if (_loaded != null)
            {
                return _loadedFingerprint == fingerprint;
            }

            _loaded = new Dictionary<string, HashSet<string>>();
            _loadedFingerprint = null;

            try
            {
                string path = CacheFilePath;
                if (!File.Exists(path))
                {
                    Plugin.Log?.LogInfo("[ShelterCounter] no shelter-count cache yet -- counting this launch, then saving it");
                    return false;
                }

                string[] lines = File.ReadAllLines(path);
                if (lines.Length < 2 || lines[0] != FileFormatVersion)
                {
                    Plugin.Log?.LogInfo("[ShelterCounter] shelter-count cache is in an older format -- recounting");
                    return false;
                }
                if (lines[1] != fingerprint)
                {
                    Plugin.Log?.LogInfo("[ShelterCounter] mods changed since the shelter-count cache was written -- recounting");
                    return false;
                }

                for (int i = 2; i < lines.Length; i++)
                {
                    int split = lines[i].IndexOf('=');
                    if (split <= 0)
                    {
                        continue;
                    }
                    string characterID = lines[i].Substring(0, split);
                    var shelters = new HashSet<string>();
                    foreach (string name in lines[i].Substring(split + 1).Split(','))
                    {
                        if (name.Length > 0)
                        {
                            shelters.Add(name);
                        }
                    }
                    _loaded[characterID] = shelters;
                }

                _loadedFingerprint = fingerprint;
                Plugin.Log?.LogInfo($"[ShelterCounter] shelter-count cache is current ({_loaded.Count} character(s) cached, mods unchanged)");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[ShelterCounter] could not read the shelter-count cache ({ex.Message}) -- recounting");
                return false;
            }
        }

        /// <summary>A cached count for this character under the current mod set, if there is one.</summary>
        public static bool TryGet(string characterID, out HashSet<string> shelters)
        {
            shelters = null;
            if (characterID == null || !EnsureLoaded())
            {
                return false;
            }
            return _loaded.TryGetValue(characterID, out shelters);
        }

        /// <summary>Stores one character's counted shelters, keeping any other characters already
        /// cached under the same mod set (a save file can have several Primal characters, and
        /// recounting one shouldn't throw the others away).</summary>
        public static void Store(string characterID, HashSet<string> shelters)
        {
            if (characterID == null || shelters == null)
            {
                return;
            }

            try
            {
                string fingerprint = Fingerprint();
                if (_loaded == null || _loadedFingerprint != fingerprint)
                {
                    // Either nothing was loaded, or what was loaded belongs to a different mod
                    // set -- in both cases this character's count starts the new file.
                    _loaded = new Dictionary<string, HashSet<string>>();
                    _loadedFingerprint = fingerprint;
                }
                _loaded[characterID] = shelters;

                var lines = new List<string> { FileFormatVersion, fingerprint };
                foreach (KeyValuePair<string, HashSet<string>> entry in _loaded)
                {
                    var names = new List<string>(entry.Value);
                    names.Sort(StringComparer.Ordinal);
                    lines.Add(entry.Key + "=" + string.Join(",", names.ToArray()));
                }

                string path = CacheFilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllLines(path, lines.ToArray());
                Plugin.Log?.LogInfo($"[ShelterCounter] saved {characterID}'s {shelters.Count} shelter(s) to {path} -- it won't be recounted until the mod set changes");
            }
            catch (Exception ex)
            {
                // Purely an optimisation; failing to persist costs a recount next launch.
                Plugin.Log?.LogWarning($"[ShelterCounter] could not save the shelter-count cache ({ex.Message})");
            }
        }
    }

    /// <summary>The fading "slept / total" label in the top-right corner. See ShelterTotals for
    /// the numbers.</summary>
    internal sealed class ShelterCounterHud : HUD.HudPart
    {
        private const float FadeSpeed = 1f / 30f;   // ~0.75 s fade at 40 ticks/s
        private const int RecountIntervalTicks = 40;

        /// <summary>Insets from the top-RIGHT corner of the screen (2026-11-17, explicit request:
        /// "I want the counter to be to the top right of the screen").
        ///
        /// This label used to be centred at a fixed `screenSize.y - 40f`, which put it head-on
        /// into vanilla's own campaign timer (`MoreSlugcats.SpeedRunTimer`, at
        /// `ScreenSize.y - (30f + 10f * fade)` and also centred on screen centre). Inside a
        /// shelter -- the only place this label ever shows -- `hud.showKarmaFoodRain` is true, so
        /// that timer always sits at full fade, i.e. exactly `y - 40f`, and the two strings
        /// printed on top of each other. A first fix stacked this two text rows lower; moving it
        /// out of the centre column entirely supersedes that, because the whole top-centre strip
        /// (campaign timer, then RainTimerHud's readout one row below it) is centre-aligned and
        /// this corner is nobody else's.
        ///
        /// TopInset matches the campaign timer's own 40px so the two sit on the same line, which
        /// reads as deliberate rather than nearly-aligned.</summary>
        private const float TopInset = 40f;
        private const float RightInset = 30f;

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
                // Right-aligned so the label hangs off a fixed inset from the screen's right
                // edge; see TopInset/RightInset and Draw.
                alignment = FLabelAlignment.Right,
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

        /// <summary>Just the numbers -- the "SHELTERS" caption was dropped 2026-11-17 by explicit
        /// request. The counting placeholder loses the caption with it and becomes a bare "...",
        /// which still shows the label is alive and working rather than stuck at a wrong count;
        /// it's only on screen for the first second or two of a cycle while the background count
        /// finishes.</summary>
        private static string BuildText(RainWorldGame game, string characterID)
        {
            if (!ShelterTotals.TryGetShelters(characterID, out HashSet<string> shelters))
            {
                return "...";
            }
            int slept = ShelterTotals.CountSlept(PrimalSaveMarkers.SaveDataFor(game), characterID, shelters);
            return $"{slept} / {shelters.Count}";
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);
            // Right-aligned, so x is the label's RIGHT edge -- the text then grows leftwards as
            // the counts get wider ("1 / 109" vs "109 / 109") and the corner inset stays put.
            _label.x = hud.rainWorld.screenSize.x - RightInset
                - hud.rainWorld.options.SafeScreenOffset.x + 0.01f;
            _label.y = hud.rainWorld.screenSize.y - TopInset
                - hud.rainWorld.options.SafeScreenOffset.y + 0.01f;
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
