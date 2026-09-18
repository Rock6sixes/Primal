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
    /// This gimmick replaces BOTH rolls outright with the character's own two settings
    /// (PrecycleTimerSettings, same file) -- a Postfix on the exact same RainCycle constructor
    /// RainTimerGimmick already Postfixes (touches entirely disjoint fields, so the two coexist
    /// with no ordering dependency), re-deciding "does a precycle happen" with the configured
    /// FrequencyPercent (0-100, a flat, character-wide replacement for the per-region vanilla
    /// chance) and, if so, "how long" with the configured DurationMinutes (replacing the vanilla
    /// 2-5 minute random roll with a fixed, configured value) -- setting up exactly the same fields
    /// vanilla's own trigger path does (`maxPreTimer`, `preTimer`, the three `preCycleRainPulse_Wave*`
    /// floats, and `globalRain.preCycleRainPulse_Scale = 1f`) when it decides yes, or zeroing
    /// `maxPreTimer`/`preTimer` back out when it decides no (overriding whatever vanilla's own roll,
    /// which already ran earlier in the same constructor, had already set).
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
    /// **Known, deliberate scope gap:** a few lines after the precycle decision, vanilla's
    /// constructor rolls a SEPARATE, region-specific "early cycle flood" chance
    /// (`world.region.regionParams.earlyCycleFloodChance`, gated on `maxPreTimer > 0` -- i.e.
    /// whether vanilla's OWN roll, not this gimmick's, triggered a precycle) that floods certain
    /// regions' water level during a precycle. That roll already happened, using vanilla's own
    /// decision, before this gimmick's Postfix ever runs, so it is NOT retroactively re-rolled or
    /// adjusted here -- this gimmick controls the precycle's frequency and duration, per the user's
    /// own explicit scope, not this secondary flood interaction. Flagged here rather than silently
    /// left inconsistent, in case a future session is asked to extend this gimmick to cover it too.
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
            float roll = UnityEngine.Random.Range(0, 100);
            UnityEngine.Random.state = savedState;

            if (roll >= frequencyPercent)
            {
                // Overrides whatever vanilla's own (per-region) roll had already decided -- 0%
                // means a precycle can truly never happen for this character, no matter how
                // precycle-prone the region itself normally is.
                __instance.maxPreTimer = 0;
                __instance.preTimer = 0;
                return;
            }

            int durationTicks = PrecycleTimerSettings.GetDurationMinutes(characterID) * 60 * TimeLimitGimmick.TicksPerSecond;
            __instance.maxPreTimer = durationTicks;
            __instance.preTimer = durationTicks;
            __instance.preCycleRainPulse_WaveA = 0f;
            __instance.preCycleRainPulse_WaveB = 0f;
            __instance.preCycleRainPulse_WaveC = (float)Math.PI / 2f;
            game.globalRain.preCycleRainPulse_Scale = 1f;

            Plugin.Log?.LogInfo($"[PrecycleTimer] {characterID}: precycle triggered (roll {roll:F1} < {frequencyPercent}%), duration {durationTicks} ticks");
        }
    }

    /// <summary>Per-character settings for PrecycleTimerGimmick: FrequencyPercent (0-100, replacing
    /// vanilla's own per-region chance outright) and DurationMinutes (replacing vanilla's own
    /// 2-5-minute random roll with a fixed value). See PrecycleTimerGimmick's own header comment
    /// for exactly what each vanilla mechanism being replaced looked like.</summary>
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
        private const int MaxDurationMinutes = 999;

        private static readonly Dictionary<string, Configurable<int>> _frequencyPercent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<int>> _durationMinutes = new Dictionary<string, Configurable<int>>();

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
                    "Minutes of head start added before the normal rain timer begins counting, when a precycle happens. Replaces vanilla's own random 2-5 minute roll with this fixed value.",
                    new ConfigAcceptableRange<int>(MinDurationMinutes, MaxDurationMinutes)));
        }

        public static int GetFrequencyPercent(string characterID)
        {
            return _frequencyPercent.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultFrequencyPercent;
        }

        public static int GetDurationMinutes(string characterID)
        {
            return _durationMinutes.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultDurationMinutes;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            if (!_frequencyPercent.TryGetValue(characterID, out Configurable<int> frequency) ||
                !_durationMinutes.TryGetValue(characterID, out Configurable<int> duration))
            {
                // Not bound yet -- don't cache an incomplete/empty result, in case Bind() hasn't
                // run for this character yet and a later call could succeed.
                return new List<UIelement>();
            }

            var items = new List<UIelement>();

            items.Add(new OpLabel(20f, topY, "Precycle chance, out of 100:", false));
            items.Add(new OpUpdown(frequency, new Vector2(280f, topY - 6f), 80f));

            items.Add(new OpLabel(20f, topY - 44f, "Precycle head start, in minutes:", false));
            items.Add(new OpUpdown(duration, new Vector2(280f, topY - 50f), 80f));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
