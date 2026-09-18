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
    /// chance) and, if so, "how long" with the configured DurationMinutes (or, with "Force max
    /// precycle timer" checked, always the longest configurable duration -- see below) -- setting
    /// up exactly the same fields vanilla's own trigger path does (`maxPreTimer`, `preTimer`, the
    /// three `preCycleRainPulse_Wave*` floats, and `globalRain.preCycleRainPulse_Scale = 1f`) when
    /// it decides yes, or zeroing `maxPreTimer`/`preTimer` back out when it decides no (overriding
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

            int durationTicks = PrecycleTimerSettings.ForceMaxDuration(characterID)
                ? PrecycleTimerSettings.MaxDurationMinutes * 60 * TimeLimitGimmick.TicksPerSecond
                : PrecycleTimerSettings.GetDurationMinutes(characterID) * 60 * TimeLimitGimmick.TicksPerSecond;
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
                game.globalRain.drainWorldFlood = (world.region.name == "MS" && !isRivulet) ? 0f : 99000f;
            }

            Plugin.Log?.LogInfo($"[PrecycleTimer] {characterID}: precycle triggered (roll {triggerRoll:F1} < {frequencyPercent}%), duration {durationTicks} ticks, flood roll {floodRoll * 100f:F1} vs {floodChancePercent}%");
        }
    }

    /// <summary>Per-character settings for PrecycleTimerGimmick: FrequencyPercent (0-100, replacing
    /// vanilla's own per-region chance outright), DurationMinutes/ForceMaxDuration (replacing
    /// vanilla's own 2-5-minute random roll with either a fixed value or, "Force max precycle
    /// timer" checked, always the longest configurable duration -- the same "always pick the
    /// maximum instead of rolling" idea as Remix's own `MMF.cfgNoRandomCycles`, which pins the
    /// normal cycle's own length to `cycleTimeMax` instead of rolling between
    /// `cycleTimeMin`/`cycleTimeMax`), and FloodChancePercent (0-100, replacing vanilla's own
    /// per-region `earlyCycleFloodChance`). See PrecycleTimerGimmick's own header comment for
    /// exactly what each vanilla mechanism being replaced looked like.</summary>
    internal static class PrecycleTimerSettings
    {
        /// <summary>Roughly matches vanilla's own commonly-cited baseline (the user's own "2%
        /// chance in certain regions") as this gimmick's default once assigned, rather than
        /// snapping straight to guaranteed-every-cycle.</summary>
        private const int DefaultFrequencyPercent = 2;

        private const int MinFrequencyPercent = 0;
        private const int MaxFrequencyPercent = 100;

        /// <summary>The middle of vanilla's own 2-5 minute (4800-12000 tick) random range, as a
        /// fixed default once assigned.</summary>
        private const int DefaultDurationMinutes = 3;

        private const int MinDurationMinutes = 0;

        /// <summary>Also what "Force max precycle timer" pins duration to when checked -- see this
        /// class's own header comment.</summary>
        internal const int MaxDurationMinutes = 999;

        /// <summary>Vanilla itself defaults to "not forced" (a real random roll every time a
        /// precycle happens) -- this gimmick's own duration is already a fixed, non-random value by
        /// default (DefaultDurationMinutes), so this checkbox only matters as a convenient
        /// "whatever the number is, always use this gimmick's own maximum instead" override, not as
        /// a way to reintroduce vanilla's own randomness.</summary>
        private const bool DefaultForceMaxDuration = false;

        /// <summary>Vanilla's own default is per-region and usually small (the user's "2% chance in
        /// certain regions" applies to flood too, in regions that roll for it at all) -- matched
        /// here as a conservative default once this gimmick is assigned, same reasoning as
        /// DefaultFrequencyPercent.</summary>
        private const int DefaultFloodChancePercent = 2;

        private const int MinFloodChancePercent = 0;
        private const int MaxFloodChancePercent = 100;

        private static readonly Dictionary<string, Configurable<int>> _frequencyPercent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<int>> _durationMinutes = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<bool>> _forceMaxDuration = new Dictionary<string, Configurable<bool>>();
        private static readonly Dictionary<string, Configurable<int>> _floodChancePercent = new Dictionary<string, Configurable<int>>();

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

            _durationMinutes[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_durationMinutes",
                DefaultDurationMinutes,
                new ConfigurableInfo(
                    "Minutes of head start added before the normal rain timer begins counting, when a precycle happens. Replaces vanilla's own random 2-5 minute roll with this fixed value. Ignored entirely while \"Force max precycle timer\" is checked.",
                    new ConfigAcceptableRange<int>(MinDurationMinutes, MaxDurationMinutes)));

            _forceMaxDuration[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_forceMaxDuration",
                DefaultForceMaxDuration,
                new ConfigurableInfo($"If checked, a precycle's head start always uses the longest configurable duration ({MaxDurationMinutes} minutes) instead of the minutes above."));

            _floodChancePercent[characterID] = owner.config.Bind(
                characterID + "_PrecycleTimer_floodChancePercent",
                DefaultFloodChancePercent,
                new ConfigurableInfo(
                    "Chance, out of 100, that a triggered precycle also floods (raises the water level in rooms the region's own level design allows to flood -- this has no visible effect in a region/room that was never built with a flood danger type). Replaces vanilla's own per-region flood chance entirely for this character.",
                    new ConfigAcceptableRange<int>(MinFloodChancePercent, MaxFloodChancePercent)));
        }

        public static int GetFrequencyPercent(string characterID)
        {
            return _frequencyPercent.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultFrequencyPercent;
        }

        public static int GetDurationMinutes(string characterID)
        {
            return _durationMinutes.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultDurationMinutes;
        }

        public static bool ForceMaxDuration(string characterID)
        {
            return _forceMaxDuration.TryGetValue(characterID, out Configurable<bool> c) && c.Value;
        }

        public static int GetFloodChancePercent(string characterID)
        {
            return _floodChancePercent.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultFloodChancePercent;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            if (!_frequencyPercent.TryGetValue(characterID, out Configurable<int> frequency) ||
                !_durationMinutes.TryGetValue(characterID, out Configurable<int> duration) ||
                !_forceMaxDuration.TryGetValue(characterID, out Configurable<bool> forceMax) ||
                !_floodChancePercent.TryGetValue(characterID, out Configurable<int> flood))
            {
                // Not bound yet -- don't cache an incomplete/empty result, in case Bind() hasn't
                // run for this character yet and a later call could succeed.
                return new List<UIelement>();
            }

            var items = new List<UIelement>();

            items.Add(new OpLabel(20f, topY, "Precycle chance, out of 100:", false));
            items.Add(new OpUpdown(frequency, new Vector2(280f, topY - 6f), 80f));

            var durationLabel = new OpLabel(20f, topY - 44f, "Precycle head start, in minutes:", false);
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

            _uiCache[characterID] = items;
            return items;
        }
    }
}
