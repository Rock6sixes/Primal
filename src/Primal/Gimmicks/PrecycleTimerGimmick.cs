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
    /// **"Force shelter unavailable" (added 2026-09-18, later still) -- see
    /// `PrecycleForceShelterUnavailableHook` below, a SEPARATE Harmony patch on `ShelterDoor`'s own
    /// constructor:** answers the user's own follow-up question directly -- whether a shelter can be
    /// forced unusable-to-hibernate-in independent of the (region/room-limited) visual flood.
    /// Traced to a real, existing vanilla mechanic that was simply far narrower than "any shelter,
    /// any region": `World.brokenShelters` (a plain `bool[]`), set true by `ShelterDoor`'s own
    /// constructor only when the room being constructed is the player's current den AND that
    /// specific room's own level design placed a `PlacedObject.Type.BrokenShelterWaterLevel` marker
    /// in it. Every system that actually treats a shelter as unusable
    /// (`ShelterDoor.Broken`, `Room.cs`'s danger-type gating, `HUD/FoodMeter.cs`, `Player.cs`,
    /// `Menu/FastTravelScreen.cs`, `Watcher/Sandstorm.cs`) reads that SAME flag, and none of them
    /// additionally check for visible water -- confirming the flag alone, not the flood, is what
    /// actually controls availability. This checkbox forces that flag directly, without needing the
    /// room to have ever had that placed-object marker at all, so it works in any shelter.
    /// </summary>
    [HarmonyPatch(typeof(RainCycle), MethodType.Constructor, new Type[] { typeof(World), typeof(float) })]
    internal static class PrecycleTimerGimmick
    {
        public const string ID = "PrecycleTimer";

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
            UnityEngine.Random.state = savedState;

            if (triggerRoll >= frequencyPercent)
            {
                // Overrides whatever vanilla's own (per-region) roll had already decided -- 0%
                // means a precycle can truly never happen for this character, no matter how
                // precycle-prone the region itself normally is.
                __instance.maxPreTimer = 0;
                __instance.preTimer = 0;
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

            Plugin.Log?.LogInfo($"[PrecycleTimer] {characterID}: precycle triggered (roll {triggerRoll:F1} < {frequencyPercent}%), duration {durationTicks} ticks ({durationPercent}% of vanilla's {PrecycleTimerSettings.VanillaMinDurationTicks}-{PrecycleTimerSettings.VanillaMaxDurationTicks} range), flood roll {floodRoll * 100f:F1} vs {floodChancePercent}%");
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
                !PrecycleTimerSettings.ForceShelterUnavailable(characterID))
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
    /// `cycleTimeMax`), and FloodChancePercent (0-100, replacing vanilla's own per-region
    /// `earlyCycleFloodChance`). See PrecycleTimerGimmick's own header comment for exactly what
    /// each vanilla mechanism being replaced looked like.</summary>
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

        /// <summary>Vanilla's own precycle-broken-shelter mechanic only ever fires for the small set
        /// of shelters a region's own level design specifically placed a BrokenShelterWaterLevel
        /// marker in -- off by default here too, so assigning this gimmick doesn't unexpectedly
        /// start locking players out of shelters that were never meant to fail.</summary>
        private const bool DefaultForceShelterUnavailable = false;

        private static readonly Dictionary<string, Configurable<int>> _frequencyPercent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<int>> _durationPercent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<bool>> _forceMaxDuration = new Dictionary<string, Configurable<bool>>();
        private static readonly Dictionary<string, Configurable<int>> _floodChancePercent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<bool>> _forceSubmergedSuperstructure = new Dictionary<string, Configurable<bool>>();
        private static readonly Dictionary<string, Configurable<bool>> _forceShelterUnavailable = new Dictionary<string, Configurable<bool>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why.
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            _frequencyPercent[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_frequencyPercent",
                DefaultFrequencyPercent,
                new ConfigurableInfo(
                    "Chance, out of 100, that a precycle (shelter failure -- storm already brewing at the start of the next cycle) happens. Replaces vanilla's own per-region chance entirely for this character.",
                    new ConfigAcceptableRange<int>(MinFrequencyPercent, MaxFrequencyPercent)));

            _durationPercent[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_durationPercent",
                DefaultDurationPercent,
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

            _forceShelterUnavailable[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_forceShelterUnavailable",
                DefaultForceShelterUnavailable,
                new ConfigurableInfo("If checked, the character's current shelter becomes unusable to hibernate in during a triggered precycle, in any shelter -- independent of whether that shelter's own room visibly floods."));
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

        public static bool ForceShelterUnavailable(string characterID)
        {
            return _forceShelterUnavailable.TryGetValue(characterID, out Configurable<bool> c) && c.Value;
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
                !_forceShelterUnavailable.TryGetValue(characterID, out Configurable<bool> forceUnavailable))
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

            items.Add(new OpCheckBox(forceUnavailable, 20f, topY - 168f));
            items.Add(new OpLabel(56f, topY - 164f, "Force shelter unavailable", false));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
