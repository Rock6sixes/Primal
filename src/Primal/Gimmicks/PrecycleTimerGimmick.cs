using System;
using System.Collections.Generic;
using HarmonyLib;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Precycle Timer" gimmick: controls the vanilla "shelter failure"/precycle mechanic --
    /// the rare event where, after sleeping in a shelter, the next cycle starts with a storm
    /// already brewing (periodic rain pulses, extra/different creature spawns per vanilla's own
    /// existing logic -- neither of those two effects needed any new code here, see below for why)
    /// and, crucially for this gimmick, a head start added BEFORE the normal rain timer even begins
    /// counting -- giving more total time before the real death-rain hits.
    ///
    /// Traced entirely to RainCycle's own constructor (root namespace, no reflection needed):
    /// vanilla's own precycle decision is two independent rolls --
    ///   1. **Whether** a precycle happens at all this cycle: a 0-100 roll against a per-REGION
    ///      base chance (`world.region.regionParams.earlyCycleChance * 100`, matching the user's
    ///      own "2% chance in certain regions" -- it's not one constant, every region defines its
    ///      own; Rivulet/Sofanthiel/Watcher get their own special-cased overrides instead of the
    ///      plain region value). Seeded deterministically off the cycle's own seed
    ///      (`StoryGameSession.SetRandomSeedToCycleSeed(1)`, saving/restoring the ambient RNG state
    ///      around it exactly like vanilla itself does, so this doesn't disturb anything else in
    ///      the constructor that also uses `UnityEngine.Random`).
    ///   2. **How long** the head start lasts, if one triggers: `maxPreTimer =
    ///      (int)UnityEngine.Random.Range(4800f, 12000f)` -- 120 to 300 real seconds (2 to 5
    ///      minutes), rerolled fresh every time a precycle actually happens. `preTimer` starts at
    ///      that value and counts DOWN to 0 every tick (confirmed in RainCycle.Update -- the normal
    ///      `timer` field, the one that actually governs when death-rain hits, doesn't increment AT
    ///      ALL while `preTimer > 0`), which is exactly the "start the cycle at negative time" the
    ///      user described -- not a separate timer bolted on, the same `RainCycle.timer` just
    ///      doesn't move yet.
    ///
    /// This gimmick replaces BOTH rolls outright with the character's own settings
    /// (PrecycleTimerSettings, same file) -- a Postfix on the exact same RainCycle constructor
    /// RainTimerGimmick already Postfixes (touches entirely disjoint fields, so the two coexist
    /// with no ordering dependency), re-deciding "does a precycle happen" with the configured
    /// FrequencyPercent (0-100, a flat, character-wide replacement for the per-region vanilla
    /// chance) and, if so, "how long" with the configured DurationPercent -- NOT a fixed value,
    /// same idea as RainTimerGimmick's own percent: it multiplies BOTH of vanilla's own bounds
    /// (4800 and 12000 ticks) by the percent, then rolls between the two SCALED bounds exactly like
    /// vanilla rolls between its own unscaled ones (or, with "Force max precycle timer" checked,
    /// always the scaled upper bound with no roll at all -- see below) -- setting up exactly the
    /// same fields vanilla's own trigger path does (`maxPreTimer`, `preTimer`, the three
    /// `preCycleRainPulse_Wave*` floats, and `globalRain.preCycleRainPulse_Scale = 1f`) when it
    /// decides yes, or zeroing `maxPreTimer`/`preTimer` back out when it decides no (overriding
    /// whatever vanilla's own roll, which already ran earlier in the same constructor, had already
    /// set).
    ///
    /// **Why "periodic rains" and "different creatures spawn" needed no extra code:** both of those
    /// read `RainCycle.preTimer`/`maxPreTimer` live, wherever they actually happen (confirmed via
    /// `preTimer` usage in `Room.cs` and `GlobalRain.cs`'s own `preCycleRainPulse_Intensity`
    /// property this file doesn't need to touch) -- not a separately-cached decision made once and
    /// frozen. Since this gimmick's Postfix runs before any of that downstream code ever reads those
    /// fields (rooms load lazily, well after a World's RainCycle is constructed), overriding
    /// `preTimer`/`maxPreTimer` here is enough to correctly drive both effects too, without this
    /// gimmick needing to know anything about how they work internally.
    ///
    /// **Flood chance (added 2026-09-18, later round):** vanilla's constructor rolls a SEPARATE
    /// "early cycle flood" chance a few lines after the precycle decision
    /// (`Random.value < world.region.regionParams.earlyCycleFloodChance`, gated on `maxPreTimer > 0`
    /// -- i.e. whether vanilla's OWN roll, not this gimmick's, triggered a precycle) that raises
    /// `globalRain.drainWorldFlood` to 99000 (or, specifically on the "MS" region unless the
    /// character is Rivulet, forces it to 0 instead -- MS/Submerged Superstructure already manages
    /// its own water state, so vanilla deliberately suppresses this generic flood there for anyone
    /// but Rivulet, who has his own special interaction with it; this port preserves that one
    /// exception exactly, since it reads as a genuine compatibility guard rather than a balance
    /// choice). That vanilla roll already ran, gated on vanilla's OWN maxPreTimer, before this
    /// gimmick's Postfix ever executes -- so it's re-rolled independently here too, using this
    /// gimmick's own FloodChancePercent (0-100) and this gimmick's OWN triggered/not-triggered
    /// decision, exactly the same "full, independent override" treatment FrequencyPercent already
    /// gets. Only rolled/applied when this gimmick's own precycle actually triggers (mirroring
    /// vanilla's own `maxPreTimer > 0` gate, now against THIS gimmick's maxPreTimer instead).
    ///
    /// **Region coverage, checked directly rather than assumed (per explicit request to double
    /// check):**
    ///   - The precycle TIMER mechanism itself (`preTimer` counting down, delaying `timer`) is
    ///     confirmed region-agnostic -- `RainCycle.Update()`'s precycle gating and
    ///     `RainWorldGame.AllowRainCounterToTick()` (read in full) key only on
    ///     `ModManager.PrecycleModule`, `IsStorySession`/`IsArenaSession`, ghost-mode cameras, gate
    ///     transitions, and a couple of Watcher/MSC-specific session states -- nothing region-
    ///     specific at all. So yes: assigning this gimmick with FrequencyPercent at 100 WILL force a
    ///     precycle head-start in literally any real playable region, confirmed by the complete
    ///     absence of a region check anywhere in that code path (the only place this gimmick itself
    ///     checks `world.region` is the top-level `world.region == null` guard, which only excludes
    ///     non-gameplay single-room worlds like menu/dream sequences -- not real regions).
    ///   - The FLOOD specifically is NOT universally visible, and this is a genuine vanilla
    ///     limitation, not a gap in this gimmick: `Room.cs`'s own `drainWorldFlood > 0f` handling
    ///     (where water actually gets added to a room) is ADDITIONALLY gated on that room's own
    ///     `RoomRain.DangerType` being `Flood` or `FloodAndRain` -- a per-room flag baked into the
    ///     region's own level-design data, not something this mod can grant to a room that was never
    ///     built with it. Setting `drainWorldFlood` via this gimmick still always happens (the field
    ///     itself is set unconditionally on trigger, same as vanilla), but it will only ever be
    ///     VISIBLE as actual rising water in rooms the region's own designers already tagged
    ///     Flood/FloodAndRain (which is presumably exactly why vanilla's own
    ///     `earlyCycleFloodChance` is itself per-region in the first place -- regions with no
    ///     flood-capable rooms at all would have no reason to roll for it).
    ///
    /// **"Force Submerged Superstructure Precycles" (added 2026-09-18, later still):** a checkbox
    /// that skips the MS/Rivulet exception above entirely for this character -- checked, the flood
    /// chance rolled above applies on MS too, for anyone.
    ///
    /// **Shelter failure chance (2026-09-18, originally "Force shelter unavailable"; rebuilt
    /// 2026-11-02 into a percentage + a "Permanent" option, per direct request) -- see
    /// `PrecycleForceShelterUnavailableHook` below, a SEPARATE Harmony patch on `ShelterDoor`'s own
    /// constructor:** whether a shelter can be forced unusable-to-hibernate-in, independent of the
    /// (region/room-limited) visual flood. Traced to a real, existing vanilla mechanic that was
    /// simply far narrower than "any shelter, any region": `World.brokenShelters` (a plain
    /// `bool[]`), set true by `ShelterDoor`'s own constructor only when the room being constructed
    /// is the player's current den AND that specific room's own level design placed a
    /// `PlacedObject.Type.BrokenShelterWaterLevel` marker in it. Every system that actually treats a
    /// shelter as unusable (`ShelterDoor.Broken`, `Room.cs`'s danger-type gating,
    /// `HUD/FoodMeter.cs`, `Player.cs`, `Menu/FastTravelScreen.cs`, `Watcher/Sandstorm.cs`) reads
    /// that SAME flag, and none of them additionally check for visible water -- confirming the flag
    /// alone, not the flood, is what actually controls availability.
    ///
    /// **"Shelter failure chance, out of 100" (`ShelterFailureChancePercent`) replaces the old
    /// plain on/off checkbox** with an independent roll, same shape as `FloodChancePercent` --
    /// rolled once per triggered precycle (in `PrecycleTimerGimmick.Postfix`'s own already-seeded
    /// RNG block, alongside the trigger/duration/flood rolls), only ever relevant while this
    /// gimmick's own precycle is actually active this cycle. The decision is stashed per-character
    /// (`_shelterFailureActiveThisCycle`) since the roll happens inside the `RainCycle` constructor
    /// Postfix, but the flag actually needs setting later, inside `ShelterDoor`'s own separate
    /// constructor Postfix, which has no access to that roll's local variables -- same "stash on one
    /// patch, read on another" shape `BlizzardTimerGimmick.CurrentCreature` already uses elsewhere
    /// in this mod.
    ///
    /// **"Permanent shelter failure" (`PermanentShelterFailure`), new 2026-11-02:** vanilla's own
    /// `World.brokenShelters` flag, once set, is never cleared anywhere in the vanilla codebase
    /// itself (confirmed by grepping every write site) -- so by itself this mechanic would already
    /// make a broken shelter stay broken for the rest of the World's lifetime (normally the whole
    /// region visit), not just the one cycle whose precycle caused it. Per direct request, that's
    /// treated as the OPT-IN "Permanent" behavior rather than the default: by default (this
    /// checkbox unchecked), `PrecycleShelterFailureResetHook` (new, a Postfix on
    /// `RainWorldGame.Win` -- confirmed elsewhere in this codebase to fire only on a genuine
    /// successful shelter-reach) restores the one shelter THIS gimmick broke that cycle back to
    /// usable the instant the player successfully sleeps/saves anywhere, matching the requested
    /// "the shelter becomes usable after you save" framing. With "Permanent shelter failure"
    /// checked, that reset is skipped entirely -- once broken, stays broken for the rest of the run,
    /// exactly vanilla's own un-reset behavior. Only ever resets a shelter THIS gimmick marked
    /// broken (tracked via `_pendingShelterFailure`, keyed by character and cleared once consumed)
    /// -- a shelter broken by vanilla's own `BrokenShelterWaterLevel` marker mechanism is untouched
    /// either way, matching real vanilla behavior for that case.
    /// </summary>
    [HarmonyPatch(typeof(RainCycle), MethodType.Constructor, new Type[] { typeof(World), typeof(float) })]
    internal static class PrecycleTimerGimmick
    {
        public const string ID = "PrecycleTimer";

        /// <summary>Holds the World+shelterIndex this gimmick most recently marked broken for a
        /// character, so PrecycleShelterFailureResetHook (a Postfix on RainWorldGame.Win, which
        /// has no access to ShelterDoor's own constructor locals) knows what to un-break on a
        /// successful sleep. Cleared once consumed (TryTakePendingShelterFailure) -- a stale
        /// leftover entry is harmless (the next real break overwrites it), but there's nothing to
        /// reset twice either way.</summary>
        private sealed class PendingShelterFailure
        {
            public World World;
            public int ShelterIndex;
        }

        /// <summary>This cycle's independent shelter-failure roll result (see
        /// PrecycleTimerSettings.ShelterFailureChancePercent), decided once in this class's own
        /// Postfix (where the roll happens) and read by PrecycleForceShelterUnavailableHook
        /// (a separate Harmony patch on ShelterDoor's constructor, fired later, with no access to
        /// this method's own locals) -- same "stash on one patch, read on another" shape
        /// BlizzardTimerGimmick.CurrentCreature already uses elsewhere in this mod. Per-character
        /// rather than per-World since that's what both patches already have on hand without
        /// needing RainWorldGame.world (a member never touched elsewhere in this codebase, so
        /// untested for Gotcha #1 purposes -- avoided rather than risked for no real benefit).</summary>
        private static readonly Dictionary<string, bool> _shelterFailureActiveThisCycle = new Dictionary<string, bool>();

        private static readonly Dictionary<string, PendingShelterFailure> _pendingShelterFailure = new Dictionary<string, PendingShelterFailure>();

        internal static bool IsShelterFailureActiveThisCycle(string characterID)
        {
            return characterID != null && _shelterFailureActiveThisCycle.TryGetValue(characterID, out bool active) && active;
        }

        internal static void RecordBrokenShelter(string characterID, World world, int shelterIndex)
        {
            _pendingShelterFailure[characterID] = new PendingShelterFailure { World = world, ShelterIndex = shelterIndex };
        }

        internal static bool TryTakePendingShelterFailure(string characterID, out World world, out int shelterIndex)
        {
            if (characterID != null && _pendingShelterFailure.TryGetValue(characterID, out PendingShelterFailure pending))
            {
                _pendingShelterFailure.Remove(characterID);
                world = pending.World;
                shelterIndex = pending.ShelterIndex;
                return true;
            }
            world = null;
            shelterIndex = -1;
            return false;
        }

        private static void Postfix(RainCycle __instance, World world)
        {
            RainWorldGame game = world?.game;
            if (game == null || !game.IsStorySession || !PrimalSlot.IsActive(game.rainWorld))
            {
                return;
            }

            // Same top-level gate vanilla's own precycle block starts with -- if none of this
            // machinery is even active (module off, no region, e.g. a menu-only/single-room world),
            // there is nothing here to override either way.
            if (!ModManager.PrecycleModule || world.region == null)
            {
                return;
            }

            string characterID = game.StoryCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            int frequencyPercent = PrecycleTimerSettings.GetFrequencyPercent(characterID);

            // Same seeding vanilla's own roll uses, saved/restored the same way, so this doesn't
            // disturb the ambient RNG stream for anything else -- see this class's own header
            // comment for why re-deriving a fresh roll (rather than trying to read back vanilla's
            // own already-consumed one) is how a flat, character-wide percent replaces vanilla's
            // per-region chance.
            UnityEngine.Random.State savedState = UnityEngine.Random.state;
            game.GetStorySession.SetRandomSeedToCycleSeed(1);
            float triggerRoll = UnityEngine.Random.Range(0, 100);
            float floodRoll = UnityEngine.Random.value;
            // Vanilla's own duration roll (`Random.Range(4800f, 12000f)`) happens inside this same
            // reseeded window too, immediately after the trigger roll -- drawn here in the same spot
            // so this gimmick's replacement roll is seeded identically to vanilla's own.
            float durationRoll = UnityEngine.Random.value;
            // Shelter-failure chance's own independent roll (2026-11-02, new) -- doesn't need to
            // match any vanilla-consumed roll (there's no vanilla equivalent), so it's safe to draw
            // last in this reseeded block without disturbing the ordering the comments above
            // already rely on.
            float shelterFailureRoll = UnityEngine.Random.value;
            UnityEngine.Random.state = savedState;

            if (triggerRoll >= frequencyPercent)
            {
                // Overrides whatever vanilla's own (per-region) roll had already decided -- 0%
                // means a precycle can truly never happen for this character, no matter how
                // precycle-prone the region itself normally is.
                __instance.maxPreTimer = 0;
                __instance.preTimer = 0;
                _shelterFailureActiveThisCycle[characterID] = false;
                return;
            }

            int durationPercent = PrecycleTimerSettings.GetDurationPercent(characterID);
            int scaledMinTicks = Mathf.RoundToInt(PrecycleTimerSettings.VanillaMinDurationTicks * (durationPercent / 100f));
            int scaledMaxTicks = Mathf.RoundToInt(PrecycleTimerSettings.VanillaMaxDurationTicks * (durationPercent / 100f));
            scaledMaxTicks = Mathf.Max(scaledMinTicks, scaledMaxTicks);
            int durationTicks = PrecycleTimerSettings.ForceMaxDuration(characterID)
                ? scaledMaxTicks
                : scaledMinTicks + Mathf.RoundToInt(durationRoll * (scaledMaxTicks - scaledMinTicks));
            __instance.maxPreTimer = durationTicks;
            __instance.preTimer = durationTicks;
            __instance.preCycleRainPulse_WaveA = 0f;
            __instance.preCycleRainPulse_WaveB = 0f;
            __instance.preCycleRainPulse_WaveC = (float)Math.PI / 2f;
            game.globalRain.preCycleRainPulse_Scale = 1f;

            int floodChancePercent = PrecycleTimerSettings.GetFloodChancePercent(characterID);
            if (floodRoll * 100f < floodChancePercent)
            {
                bool isRivulet = characterID == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value;
                bool suppressOnMS = world.region.name == "MS" && !isRivulet && !PrecycleTimerSettings.ForceSubmergedSuperstructure(characterID);
                game.globalRain.drainWorldFlood = suppressOnMS ? 0f : 99000f;
            }

            int shelterFailureChancePercent = PrecycleTimerSettings.GetShelterFailureChancePercent(characterID);
            bool shelterFailureActive = shelterFailureRoll * 100f < shelterFailureChancePercent;
            _shelterFailureActiveThisCycle[characterID] = shelterFailureActive;

            Plugin.Log?.LogInfo($"[PrecycleTimer] {characterID}: precycle triggered (roll {triggerRoll:F1} < {frequencyPercent}%), duration {durationTicks} ticks ({durationPercent}% of vanilla's {PrecycleTimerSettings.VanillaMinDurationTicks}-{PrecycleTimerSettings.VanillaMaxDurationTicks} range), flood roll {floodRoll * 100f:F1} vs {floodChancePercent}%, shelter-failure roll {shelterFailureRoll * 100f:F1} vs {shelterFailureChancePercent}% ({(shelterFailureActive ? "active" : "inactive")} this cycle)");
        }
    }

    /// <summary>
    /// Companion to PrecycleTimerGimmick's own flood-chance override: forces the player's CURRENT
    /// shelter to be functionally unavailable (can't be used to hibernate) during an active,
    /// gimmick-triggered precycle -- independent of whether the room's own level design supports
    /// visible flood water, per the user's own explicit question ("can every shelter... be forced
    /// to be unavailable, even if the visual flooding is not present?").
    ///
    /// Traced to `ShelterDoor`'s own constructor (root namespace): vanilla itself already has this
    /// exact mechanic, just gated much more narrowly than "any shelter, any region." When
    /// `rainCycle.preTimer > 0` and the room being constructed IS the player's current den
    /// (`room.world.GetAbstractRoom(room.world.game.startingRoom) == room.abstractRoom`), vanilla
    /// checks whether that SPECIFIC room's own `roomSettings.placedObjects` includes a
    /// `PlacedObject.Type.BrokenShelterWaterLevel` marker -- a level-design object, one region
    /// designers place only in specific shelters meant to fail. Only if that marker is present does
    /// vanilla set `room.world.brokenShelters[shelterIndex] = true` (and
    /// `brokenShelterIndexDueToPrecycle = shelterIndex`) -- a plain `bool[]` on `World`, read by
    /// `ShelterDoor.Broken` and, independently, by several unrelated systems across the codebase
    /// (`Room.cs`'s own danger-type gating, `HUD/FoodMeter.cs`, `Player.cs`, `Menu/FastTravelScreen.cs`,
    /// `Watcher/Sandstorm.cs`) that all treat a broken shelter as "doesn't function as a shelter" --
    /// none of them additionally check for visible water, confirming this flag alone is what
    /// controls actual unavailability, not the flood visuals.
    ///
    /// This Prefix sets `brokenShelters[shelterIndex]`/`brokenShelterIndexDueToPrecycle` directly,
    /// BEFORE vanilla's own constructor body runs -- not gated on that room ever having a
    /// `BrokenShelterWaterLevel` placed object at all, so it works in literally any shelter room in
    /// any region. Using a Prefix rather than a Postfix specifically so vanilla's own later
    /// `if (Broken) { ... }` block (further down the same constructor, which randomizes the broken
    /// door's visual segment/flap offsets) sees the forced value too -- the door's own broken visual
    /// plays correctly, it's only the room-wide water level (a separate, DangerType-gated system,
    /// see PrecycleTimerGimmick's own header comment) that may not.
    /// </summary>
    [HarmonyPatch(typeof(ShelterDoor), MethodType.Constructor, new Type[] { typeof(Room) })]
    internal static class PrecycleForceShelterUnavailableHook
    {
        private static void Prefix(Room room)
        {
            RainWorldGame game = room?.world?.game;
            if (game == null || !game.IsStorySession || !PrimalSlot.IsActive(game.rainWorld))
            {
                return;
            }

            if (!ModManager.PrecycleModule)
            {
                return;
            }

            RainCycle rainCycle = room.world.rainCycle;
            if (rainCycle == null || rainCycle.preTimer <= 0)
            {
                return;
            }

            // Only the player's actual current den/shelter -- matches vanilla's own scope for this
            // exact mechanic (see this class's own header comment).
            if (room.world.GetAbstractRoom(game.startingRoom) != room.abstractRoom)
            {
                return;
            }

            string characterID = game.StoryCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, PrecycleTimerGimmick.ID) ||
                !PrecycleTimerGimmick.IsShelterFailureActiveThisCycle(characterID))
            {
                return;
            }

            int shelterIndex = room.abstractRoom.shelterIndex;
            if (shelterIndex < 0 || room.world.brokenShelters == null || shelterIndex >= room.world.brokenShelters.Length)
            {
                return;
            }

            room.world.brokenShelters[shelterIndex] = true;
            room.world.brokenShelterIndexDueToPrecycle = shelterIndex;
            // Remembered so PrecycleShelterFailureResetHook knows what to un-break on a
            // successful sleep, unless "Permanent shelter failure" is checked for this character.
            PrecycleTimerGimmick.RecordBrokenShelter(characterID, room.world, shelterIndex);
        }
    }

    /// <summary>
    /// Undoes a shelter PrecycleForceShelterUnavailableHook broke this cycle, the instant the
    /// player successfully sleeps/saves ANYWHERE -- not necessarily in that same shelter -- unless
    /// "Permanent shelter failure" is checked for this character, in which case this is a no-op and
    /// the shelter stays broken for the rest of the run (vanilla's own un-reset behavior; see
    /// PrecycleTimerGimmick's own header comment for the full "why" and why this is the OPT-IN
    /// behavior, not the default). A Postfix on RainWorldGame.Win -- confirmed elsewhere in this
    /// codebase (CreatureKillListGimmick.cs's own header comment) to fire only from a genuine
    /// successful shelter-reach (ShelterDoor.cs), never from a death. Postfix, not Prefix: unlike
    /// CreatureKillListGimmick's own Win hook (which had to pre-empt a synchronous save-to-disk
    /// write inside Win itself), World.brokenShelters is a pure runtime simulation flag, never
    /// persisted to the save file at all -- there's no disk-write race to beat here, so firing after
    /// Win's own body has already run is just as correct and reads more naturally ("after a
    /// successful sleep, reset").
    /// </summary>
    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.Win))]
    internal static class PrecycleShelterFailureResetHook
    {
        private static void Postfix(RainWorldGame __instance, bool malnourished, bool fromWarpPoint)
        {
            if (__instance == null || !__instance.IsStorySession || !PrimalSlot.IsActive(__instance.rainWorld))
            {
                return;
            }

            string characterID = __instance.StoryCharacter?.value;
            if (characterID == null || !GimmickAssignments.IsEnabled(characterID, PrecycleTimerGimmick.ID))
            {
                return;
            }

            if (PrecycleTimerSettings.PermanentShelterFailure(characterID))
            {
                // Permanent -- leave whatever this gimmick broke exactly as it is; don't even
                // consume the pending record, in case a later round wants to add an in-game way to
                // flip this setting mid-run without losing track of what's pending.
                return;
            }

            if (PrecycleTimerGimmick.TryTakePendingShelterFailure(characterID, out World world, out int shelterIndex))
            {
                if (world?.brokenShelters != null && shelterIndex >= 0 && shelterIndex < world.brokenShelters.Length)
                {
                    world.brokenShelters[shelterIndex] = false;
                }
                if (world != null && world.brokenShelterIndexDueToPrecycle == shelterIndex)
                {
                    world.brokenShelterIndexDueToPrecycle = -1;
                }
                Plugin.Log?.LogInfo($"[PrecycleTimer] {characterID}: shelter failure reset on successful sleep (shelter index {shelterIndex})");
            }
        }
    }

    /// <summary>Per-character settings for PrecycleTimerGimmick: FrequencyPercent (0-100, replacing
    /// vanilla's own per-region chance outright), DurationPercent/ForceMaxDuration (a percentage
    /// multiplier on BOTH of vanilla's own 4800/12000-tick roll bounds -- same idea as
    /// RainTimerGimmick's own percent setting, not a fixed replacement value -- rolling between the
    /// two SCALED bounds every time a precycle triggers, or, "Force max precycle timer" checked,
    /// always the scaled upper bound with no roll at all -- the same "always pick the maximum
    /// instead of rolling" idea as Remix's own `MMF.cfgNoRandomCycles`, which pins the normal
    /// cycle's own length to `cycleTimeMax` instead of rolling between `cycleTimeMin`/
    /// `cycleTimeMax`), FloodChancePercent (0-100, replacing vanilla's own per-region
    /// `earlyCycleFloodChance`), and ShelterFailureChancePercent/PermanentShelterFailure (2026-11-02,
    /// replacing the old plain on/off "Force shelter unavailable" checkbox -- see
    /// PrecycleTimerGimmick's own header comment for the full "why" behind both). See
    /// PrecycleTimerGimmick's own header comment for exactly what each vanilla mechanism being
    /// replaced looked like.</summary>
    internal static class PrecycleTimerSettings
    {
        /// <summary>Roughly matches vanilla's own commonly-cited baseline (the user's own "2%
        /// chance in certain regions") as this gimmick's default once assigned, rather than
        /// snapping straight to guaranteed-every-cycle.</summary>
        private const int DefaultFrequencyPercent = 2;

        private const int MinFrequencyPercent = 0;
        private const int MaxFrequencyPercent = 100;

        /// <summary>Vanilla's own precycle-duration roll bounds, in ticks (`Random.Range(4800f,
        /// 12000f)` -- 2 to 5 real minutes at 40 ticks/second). DurationPercent multiplies both of
        /// these, so 100% reproduces vanilla's own range exactly, 0% always gives a 0-tick head
        /// start, and above 100% scales past vanilla's own longest possible roll.</summary>
        internal const int VanillaMinDurationTicks = 4800;
        internal const int VanillaMaxDurationTicks = 12000;

        /// <summary>100% reproduces vanilla's own unscaled range as this gimmick's default once
        /// assigned, matching RainTimerGimmick's own DefaultPercent convention.</summary>
        private const int DefaultDurationPercent = 100;

        private const int MinDurationPercent = 0;

        /// <summary>Matches RainTimerGimmick's own MaxPercent -- 4x vanilla's own longest roll.</summary>
        private const int MaxDurationPercent = 400;

        /// <summary>Vanilla itself defaults to "not forced" (a real random roll every time a
        /// precycle happens) -- this checkbox is a convenient "always use this gimmick's own scaled
        /// upper bound instead of rolling" override, not a way to reintroduce vanilla's own
        /// randomness by default.</summary>
        private const bool DefaultForceMaxDuration = false;

        /// <summary>Vanilla's own default is per-region and usually small (the user's "2% chance in
        /// certain regions" applies to flood too, in regions that roll for it at all) -- matched
        /// here as a conservative default once this gimmick is assigned, same reasoning as
        /// DefaultFrequencyPercent.</summary>
        private const int DefaultFloodChancePercent = 2;

        private const int MinFloodChancePercent = 0;
        private const int MaxFloodChancePercent = 100;

        /// <summary>Vanilla suppresses the flood on the "MS" region (Submerged Superstructure) for
        /// anyone but Rivulet -- off by default here too, so this gimmick matches vanilla's own
        /// behavior until explicitly told otherwise.</summary>
        private const bool DefaultForceSubmergedSuperstructure = false;

        /// <summary>Generic default, matching the old "Force shelter unavailable" checkbox's own
        /// off-by-default behavior (0% == never) -- assigning this gimmick to a character doesn't
        /// unexpectedly start locking them out of shelters that were never meant to fail, same
        /// reasoning as before.</summary>
        private const int DefaultShelterFailureChancePercent = 0;

        private const int MinShelterFailureChancePercent = 0;
        private const int MaxShelterFailureChancePercent = 100;

        /// <summary>Off by default -- a broken shelter resets back to usable the instant the
        /// character next successfully sleeps/saves anywhere (PrecycleShelterFailureResetHook),
        /// matching the request's own framing of "unlike vanilla behavior where that shelter can be
        /// used after you save." Checking this reproduces vanilla's own actual un-reset behavior
        /// instead (see PrecycleTimerGimmick's own header comment) -- an opt-in harsher mode, not
        /// the default.</summary>
        private const bool DefaultPermanentShelterFailure = false;

        /// <summary>Per-character overrides for Rivulet (2026-11-02, explicit request -- the
        /// original design table's own "Precycle Timer (50% multiplier, 100% chance, 100% shelter
        /// failure)" row, completed): a precycle happens every cycle (100%), the head-start lasts
        /// half of vanilla's own range (50%), and the current shelter always fails alongside it
        /// (100%) unless/until "Permanent shelter failure" is also checked by hand. See
        /// RainTimerSettings.PerCharacterDefaultPercent for the identical pattern this mirrors.
        /// FloodChancePercent/ForceSubmergedSuperstructure/PermanentShelterFailure were not part of
        /// either request and are deliberately left at their plain generic defaults for
        /// Rivulet too -- not assumed.</summary>
        private static readonly Dictionary<string, int> PerCharacterDefaultFrequencyPercent = new Dictionary<string, int>
        {
            [MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value] = 100,
        };

        private static readonly Dictionary<string, int> PerCharacterDefaultDurationPercent = new Dictionary<string, int>
        {
            [MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value] = 50,
        };

        private static readonly Dictionary<string, int> PerCharacterDefaultShelterFailureChancePercent = new Dictionary<string, int>
        {
            [MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value] = 100,
        };

        private static readonly Dictionary<string, Configurable<int>> _frequencyPercent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<int>> _durationPercent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<bool>> _forceMaxDuration = new Dictionary<string, Configurable<bool>>();
        private static readonly Dictionary<string, Configurable<int>> _floodChancePercent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<bool>> _forceSubmergedSuperstructure = new Dictionary<string, Configurable<bool>>();
        private static readonly Dictionary<string, Configurable<int>> _shelterFailureChancePercent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<bool>> _permanentShelterFailure = new Dictionary<string, Configurable<bool>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why.
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            int defaultFrequencyPercent = PerCharacterDefaultFrequencyPercent.TryGetValue(characterID, out int overrideFrequency)
                ? overrideFrequency
                : DefaultFrequencyPercent;

            _frequencyPercent[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_frequencyPercent",
                defaultFrequencyPercent,
                new ConfigurableInfo(
                    "Chance, out of 100, that a precycle (shelter failure -- storm already brewing at the start of the next cycle) happens. Replaces vanilla's own per-region chance entirely for this character.",
                    new ConfigAcceptableRange<int>(MinFrequencyPercent, MaxFrequencyPercent)));

            int defaultDurationPercent = PerCharacterDefaultDurationPercent.TryGetValue(characterID, out int overrideDuration)
                ? overrideDuration
                : DefaultDurationPercent;

            _durationPercent[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_durationPercent",
                defaultDurationPercent,
                new ConfigurableInfo(
                    $"Percentage multiplier on BOTH bounds of vanilla's own random head-start roll ({VanillaMinDurationTicks}-{VanillaMaxDurationTicks} ticks, i.e. 2-5 real minutes). 100% rolls within vanilla's own unscaled range; 0% always gives no head start at all. Ignored entirely while \"Force max precycle timer\" is checked.",
                    new ConfigAcceptableRange<int>(MinDurationPercent, MaxDurationPercent)));

            _forceMaxDuration[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_forceMaxDuration",
                DefaultForceMaxDuration,
                new ConfigurableInfo("If checked, a precycle's head start always uses the scaled upper bound (the percent above times vanilla's own longest roll) instead of rolling between the scaled bounds."));

            _floodChancePercent[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_floodChancePercent",
                DefaultFloodChancePercent,
                new ConfigurableInfo(
                    "Chance, out of 100, that a triggered precycle also floods (raises the water level in rooms the region's own level design allows to flood -- this has no visible effect in a region/room that was never built with a flood danger type). Replaces vanilla's own per-region flood chance entirely for this character.",
                    new ConfigAcceptableRange<int>(MinFloodChancePercent, MaxFloodChancePercent)));

            _forceSubmergedSuperstructure[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_forceSubmergedSuperstructure",
                DefaultForceSubmergedSuperstructure,
                new ConfigurableInfo("If checked, the flood chance above also applies on the Submerged Superstructure region (\"MS\"), which vanilla normally exempts from precycle flooding for every character except Rivulet."));

            int defaultShelterFailureChancePercent = PerCharacterDefaultShelterFailureChancePercent.TryGetValue(characterID, out int overrideShelterFailure)
                ? overrideShelterFailure
                : DefaultShelterFailureChancePercent;

            _shelterFailureChancePercent[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_shelterFailureChancePercent",
                defaultShelterFailureChancePercent,
                new ConfigurableInfo(
                    "Chance, out of 100, that a triggered precycle ALSO makes the character's current shelter unusable to hibernate in -- independent of whether that shelter's own room visibly floods. Rolled independently from the flood chance above, only while a precycle is actually happening.",
                    new ConfigAcceptableRange<int>(MinShelterFailureChancePercent, MaxShelterFailureChancePercent)));

            _permanentShelterFailure[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_permanentShelterFailure",
                DefaultPermanentShelterFailure,
                new ConfigurableInfo("If checked, a shelter the chance above breaks stays unusable for the REST OF THE RUN. Unchecked (default), it becomes usable again the moment the character next successfully sleeps/saves anywhere."));
        }

        public static int GetFrequencyPercent(string characterID)
        {
            return _frequencyPercent.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultFrequencyPercent;
        }

        public static int GetDurationPercent(string characterID)
        {
            return _durationPercent.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultDurationPercent;
        }

        public static bool ForceMaxDuration(string characterID)
        {
            return _forceMaxDuration.TryGetValue(characterID, out Configurable<bool> c) && c.Value;
        }

        public static int GetFloodChancePercent(string characterID)
        {
            return _floodChancePercent.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultFloodChancePercent;
        }

        public static bool ForceSubmergedSuperstructure(string characterID)
        {
            return _forceSubmergedSuperstructure.TryGetValue(characterID, out Configurable<bool> c) && c.Value;
        }

        public static int GetShelterFailureChancePercent(string characterID)
        {
            return _shelterFailureChancePercent.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultShelterFailureChancePercent;
        }

        public static bool PermanentShelterFailure(string characterID)
        {
            return _permanentShelterFailure.TryGetValue(characterID, out Configurable<bool> c) && c.Value;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            if (!_frequencyPercent.TryGetValue(characterID, out Configurable<int> frequency) ||
                !_durationPercent.TryGetValue(characterID, out Configurable<int> duration) ||
                !_forceMaxDuration.TryGetValue(characterID, out Configurable<bool> forceMax) ||
                !_floodChancePercent.TryGetValue(characterID, out Configurable<int> flood) ||
                !_forceSubmergedSuperstructure.TryGetValue(characterID, out Configurable<bool> forceSubmerged) ||
                !_shelterFailureChancePercent.TryGetValue(characterID, out Configurable<int> shelterFailureChance) ||
                !_permanentShelterFailure.TryGetValue(characterID, out Configurable<bool> permanentShelterFailure))
            {
                // Not bound yet -- don't cache an incomplete/empty result, in case Bind() hasn't
                // run for this character yet and a later call could succeed.
                return new List<UIelement>();
            }

            var items = new List<UIelement>();

            items.Add(new OpLabel(20f, topY, "Precycle chance, out of 100:", false));
            items.Add(new OpUpdown(frequency, new Vector2(280f, topY - 6f), 80f));

            var durationLabel = new OpLabel(20f, topY - 44f, "Precycle head start, percent of vanilla:", false);
            var durationUpdown = new OpUpdown(duration, new Vector2(280f, topY - 50f), 80f);
            var forceMaxCheckbox = new OpCheckBox(forceMax, 380f, topY - 50f);
            var forceMaxLabel = new OpLabel(416f, topY - 46f, "Force max precycle timer", false);

            // Captured once, right after construction -- see RainTimerSettings.BuildUI (same
            // pattern, reused here) for why.
            Color enabledDurationLabelColor = durationLabel.color;

            void ApplyForceMaxVisual(bool isForced)
            {
                durationUpdown.greyedOut = isForced;
                durationLabel.color = isForced ? Color.grey : enabledDurationLabelColor;
            }

            ApplyForceMaxVisual(forceMax.Value);
            forceMax.OnChange += () => ApplyForceMaxVisual(forceMax.Value);

            items.Add(durationLabel);
            items.Add(durationUpdown);
            items.Add(forceMaxCheckbox);
            items.Add(forceMaxLabel);

            items.Add(new OpLabel(20f, topY - 88f, "Flood chance, out of 100:", false));
            items.Add(new OpUpdown(flood, new Vector2(280f, topY - 94f), 80f));

            items.Add(new OpCheckBox(forceSubmerged, 20f, topY - 132f));
            items.Add(new OpLabel(56f, topY - 128f, "Force Submerged Superstructure Precycles", false));

            items.Add(new OpLabel(20f, topY - 168f, "Shelter failure chance, out of 100:", false));
            items.Add(new OpUpdown(shelterFailureChance, new Vector2(280f, topY - 174f), 80f));

            items.Add(new OpCheckBox(permanentShelterFailure, 20f, topY - 212f));
            items.Add(new OpLabel(56f, topY - 208f, "Permanent shelter failure", false));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
