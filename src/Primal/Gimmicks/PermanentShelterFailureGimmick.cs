using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RWCustom;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared record of which shelters a character has successfully slept in on the current save
    /// (added 2026-11-05 for Primal Rivulet's "routing" design: sleep in every shelter, and every
    /// shelter you sleep in is gone for good, so the run becomes a one-way route through every
    /// shelter with no backtracking).
    ///
    /// Used by two independent pieces that each work on their own:
    ///   - PermanentShelterFailureGimmick (this file) -- a shelter with a sleep record is forced
    ///     broken every time its region's World is built.
    ///   - SleepEveryShelterRequirement (Requirements.cs) -- the requirement is satisfied once every
    ///     shelter in every story region has a sleep record.
    ///
    /// One marker per (character, shelter room name), stored on DeathPersistentSaveData via
    /// PrimalSaveMarkers. Death-persistent is the right home: a sleep is only ever recorded from a
    /// Prefix on RainWorldGame.Win (see SurvivorEvents.OnSuccessfulHibernation for why a Prefix --
    /// Win writes the save before it returns), so the record lands in the very save that commits
    /// the sleep, and a later death can't roll it back.
    ///
    /// Only recorded while at least one of the two pieces is assigned to that character, so other
    /// Primal characters' saves get no extra markers. Consequence: turning either on mid-campaign
    /// only counts sleeps from that point on.
    /// </summary>
    internal static class ShelterSleepRecords
    {
        private const string SleptMarkerKey = "PrimalShelterSlept";

        /// <summary>Vanilla's own brokenShelters array as it stood right after World.LoadWorld,
        /// before this mod forced anything -- i.e. only the shelters the character's own timeline
        /// breaks permanently via the region map file's "Broken Shelters" line (World.LoadMapConfig).
        /// SleepEveryShelterRequirement skips these, since nobody can ever sleep in them.</summary>
        private static readonly ConditionalWeakTable<World, bool[]> _vanillaBroken = new ConditionalWeakTable<World, bool[]>();

        private static string SleptMarker(string characterID, string roomName)
        {
            return PrimalSaveMarkers.Marker(SleptMarkerKey, characterID, roomName);
        }

        public static bool HasSlept(DeathPersistentSaveData saveData, string characterID, string roomName)
        {
            return saveData != null && roomName != null && PrimalSaveMarkers.Has(saveData, SleptMarker(characterID, roomName));
        }

        /// <summary>Records that `player` is sleeping in the shelter they're standing in. Idempotent
        /// (both the gimmick and the requirement call this for the same sleep). Returns the shelter's
        /// room name, or null if the player isn't in a usable shelter.</summary>
        public static string RecordSleep(RainWorldGame game, Player player, string characterID)
        {
            AbstractRoom room = player?.room?.abstractRoom;
            if (room == null || !room.shelter || characterID == null)
            {
                return null;
            }

            DeathPersistentSaveData saveData = PrimalSaveMarkers.SaveDataFor(game);
            if (saveData == null)
            {
                return null;
            }

            if (PrimalSaveMarkers.Add(saveData, SleptMarker(characterID, room.name)))
            {
                Plugin.Log?.LogInfo($"[ShelterSleepRecords] {characterID}: slept in {room.name}");
            }
            return room.name;
        }

        public static void SnapshotVanillaBroken(World world)
        {
            if (world?.brokenShelters == null)
            {
                return;
            }
            _vanillaBroken.Remove(world);
            _vanillaBroken.Add(world, (bool[])world.brokenShelters.Clone());
        }

        public static bool IsVanillaBroken(World world, int shelterIndex)
        {
            return world != null
                && _vanillaBroken.TryGetValue(world, out bool[] broken)
                && shelterIndex >= 0
                && shelterIndex < broken.Length
                && broken[shelterIndex];
        }
    }

    /// <summary>
    /// "Permanent Shelter Failure" gimmick (added 2026-11-05): once this character successfully
    /// sleeps in a shelter, that shelter is broken for the rest of the save -- its door never
    /// closes again, so it can't be slept in a second time.
    ///
    /// Uses vanilla's own `World.brokenShelters` flag, the same one precycles and the
    /// timeline "Broken Shelters" map-file line use (see PrecycleForceShelterUnavailableHook's
    /// header comment for every system that reads it: ShelterDoor.Broken/Close, Room's rain
    /// gating, FoodMeter, Player, FastTravelScreen, OverseersWorldAI, the HUD map). The flag lives
    /// on World and is rebuilt every time a region loads, so this re-applies it from the save's
    /// sleep records (ShelterSleepRecords) in a Postfix on both of World's loaders:
    ///   - World.LoadWorld -- every real gameplay World (new cycle, region gate transition).
    ///   - World.LoadWorldForFastTravel -- the passage/fast-travel screen's own World (built with
    ///     game == null), so FastTravelScreen's existing brokenShelters check also stops you
    ///     passaging into a used shelter.
    /// Postfix, not Prefix: LoadWorld itself allocates a fresh `brokenShelters` array and then
    /// fills in the timeline's own broken shelters (LoadMapConfig), both of which would overwrite
    /// anything set earlier.
    ///
    /// Side effects worth knowing (all vanilla behaviour of a broken shelter, not new code):
    ///   - You wake up in the shelter you just slept in, and it's already broken. Same situation
    ///     as a vanilla precycle broken shelter, which the game handles.
    ///   - A broken shelter loses its shelter icon on the HUD map (HUD/Map.cs), so the map only
    ///     shows the shelters you still have left. (The one you woke in keeps its icon only on a
    ///     precycle, via brokenShelterIndexDueToPrecycle.)
    ///   - Room.cs lets rain into a broken shelter if the room's own DangerType allows rain.
    ///   - If every reachable shelter is used up, there is nowhere left to sleep. That's the point
    ///     of the design, but it can end a run.
    /// </summary>
    internal static class PermanentShelterFailureGimmick
    {
        public const string ID = "PermanentShelterFailure";

        private static bool _registered;

        /// <summary>Records each sleep while this gimmick is assigned. Subscribed from
        /// RainWorldHooks alongside the other plain event subscriptions. Idempotent, same guard
        /// and reason as RequirementRegistry.RegisterAll (OnModsInit can fire more than once).</summary>
        public static void Register()
        {
            if (_registered)
            {
                return;
            }
            _registered = true;

            SurvivorEvents.OnSuccessfulHibernation += (game, player, characterID) =>
            {
                if (GimmickGate.IsActiveFor(player, ID))
                {
                    ShelterSleepRecords.RecordSleep(game, player, characterID);
                }
            };
        }

        /// <summary>Snapshot vanilla's own broken shelters, then force-break every shelter this
        /// character has slept in. Shared by both World loader patches below.</summary>
        internal static void Apply(World world)
        {
            if (world?.shelters == null || world.brokenShelters == null)
            {
                return;
            }

            RainWorld rainWorld = world.game?.rainWorld ?? Custom.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            // A gameplay World has a live game; the fast-travel World doesn't, so fall back to the
            // save state the passage screen is working from.
            SaveState saveState = world.game != null
                ? world.game.GetStorySession?.saveState
                : rainWorld.progression?.currentSaveState;
            string characterID = world.game != null
                ? world.game.StoryCharacter?.value
                : saveState?.saveStateNumber?.value;
            if (saveState == null || characterID == null)
            {
                return;
            }

            ShelterSleepRecords.SnapshotVanillaBroken(world);

            if (!GimmickGate.IsEnabledForCharacter(rainWorld, characterID, ID))
            {
                return;
            }

            DeathPersistentSaveData saveData = saveState.deathPersistentSaveData;
            List<string> forced = null;
            foreach (int roomIndex in world.shelters)
            {
                AbstractRoom shelterRoom = world.GetAbstractRoom(roomIndex);
                if (shelterRoom == null || shelterRoom.shelterIndex < 0 || shelterRoom.shelterIndex >= world.brokenShelters.Length)
                {
                    continue;
                }
                if (ShelterSleepRecords.HasSlept(saveData, characterID, shelterRoom.name))
                {
                    world.brokenShelters[shelterRoom.shelterIndex] = true;
                    (forced ??= new List<string>()).Add(shelterRoom.name);
                }
            }

            if (forced != null)
            {
                Plugin.Log?.LogInfo($"[PermanentShelterFailure] {characterID} in {world.name}: {forced.Count} used shelter(s) broken: {string.Join(", ", forced)}");
            }
        }
    }

    [HarmonyPatch(typeof(World), nameof(World.LoadWorld), new[] { typeof(SlugcatStats.Timeline), typeof(List<AbstractRoom>), typeof(int[]), typeof(int[]), typeof(int[]) })]
    internal static class PermanentShelterFailureLoadWorldHook
    {
        private static void Postfix(World __instance)
        {
            PermanentShelterFailureGimmick.Apply(__instance);
        }
    }

    // 2026-11-10 fix: World.LoadWorldForFastTravel is overloaded (SlugcatStats.Name or
    // SlugcatStats.Timeline as the first parameter, see World.cs) -- without explicit parameter
    // types here, Harmony's AccessTools.DeclaredMethod lookup is ambiguous and throws
    // HarmonyException during Harmony.PatchAll(), which aborts ALL of this mod's patching
    // (Primal.Plugin.OnEnable's own PatchAll call has no per-patch isolation), not just this one
    // hook -- the symptom in-game was the Remix config screen reporting "no configuration
    // interface" at all, because OptionInterface itself never got registered either. Pinned to the
    // Timeline overload, matching PermanentShelterFailureLoadWorldHook's own LoadWorld pin just
    // above (the Name overload is a thin wrapper that just calls this one, per World.cs).
    [HarmonyPatch(typeof(World), nameof(World.LoadWorldForFastTravel), new[] { typeof(SlugcatStats.Timeline), typeof(List<AbstractRoom>), typeof(int[]), typeof(int[]), typeof(int[]) })]
    internal static class PermanentShelterFailureFastTravelHook
    {
        private static void Postfix(World __instance)
        {
            PermanentShelterFailureGimmick.Apply(__instance);
        }
    }
}
