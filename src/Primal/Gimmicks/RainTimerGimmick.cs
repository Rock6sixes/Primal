using System;
using System.Collections.Generic;
using HarmonyLib;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Rain Timer" gimmick: speeds up or slows down how quickly rain (or, on Saint, the
    /// region-wide blizzard -- see below for why the two are the same mechanism) arrives each
    /// cycle, as a percentage of vanilla's own normal timing.
    ///
    /// Traced from RainCycle.cs (root namespace, no reflection needed -- this whole file is public
    /// in the reference assembly): a RainCycle owns one int, `cycleLength` (ticks at the fixed
    /// 40-per-second simulation rate; `World.rainCycle.timer` counts up from 0 every tick, and once
    /// `timer >= cycleLength` the death rain hits -- see RainCycle.RainGameOver/RainHit). This is
    /// computed once, in the constructor, by GetDesiredCycleLength() and never touched again for
    /// the life of that RainCycle instance (confirmed -- GetDesiredCycleLength has exactly one
    /// caller, the constructor itself).
    ///
    /// Postfixing the constructor (after vanilla's own GetDesiredCycleLength call has already run)
    /// and rescaling `cycleLength` by this gimmick's configured percentage is exactly the same
    /// mechanism a real, already-shipped third-party mod uses for the same purpose: RainCycle.cs's
    /// own source has several `if (ModManager.MMF && ...) num = (int)((float)num *
    /// MMF.cfgRainTimeMultiplier.Value);`-shaped lines (see GetDesiredCycleLength and the
    /// constructor's own sunDownStartTime calculation) -- MMF is a separate installed mod with its
    /// own "how fast does rain arrive" config, confirmed by exactly mirroring its own scaling
    /// points rather than inventing a new one. `sunDownStartTime` (when the sky starts darkening
    /// toward night) is rescaled by the same ratio alongside cycleLength, for the same reason MMF's
    /// own multiplier touches both -- otherwise day/night lighting would drift out of sync with how
    /// fast the actual rain/blizzard danger is arriving.
    ///
    /// **Why this also controls Saint's blizzard, unmodified:** BlizzardWorldActive (true only
    /// while playing Saint) is read INSIDE GetDesiredCycleLength itself, before this gimmick's own
    /// Postfix ever runs -- Saint's blizzard isn't a separate timer at all, it's the exact same
    /// RainCycle, just pre-shortened to 70% of the normal length by vanilla's own
    /// `if (BlizzardWorldActive) num = (int)((float)num * 0.7f);` line before this gimmick's
    /// percentage multiplies on top of that. So assigning Rain Timer to Saint scales the same
    /// timer the blizzard already uses, no extra plumbing required -- and it composes correctly on
    /// any other character's plain rain timer too, for the exact same reason.
    ///
    /// **The known "blizzard doesn't render" failure mode, and the floor that prevents it:** an
    /// earlier, non-mod attempt at this same feature (human-programmer implementation, before this
    /// port) found that pushing the timer below roughly 13% of its normal length made the blizzard
    /// stop rendering. Root-caused here by reading BlizzardGraphics.CycleUpdate (MoreSlugcats
    /// namespace) directly: for Saint's own home region (the "glacial wasteland" branch, which is
    /// what a normal ambient blizzard actually uses -- AerieBlizzard-flagged rooms are a separate,
    /// cycleLength-independent case and were never affected), blizzard intensity is
    /// `Mathf.InverseLerp(cycleLength * 0.932f, 3000f, TimeUntilRain)` -- a ramp meant to run from
    /// "far bound" cycleLength*0.932 down to "near bound" a hardcoded 3000 ticks. That only makes
    /// sense while cycleLength*0.932 is bigger than 3000; once cycleLength drops below
    /// 3000/0.932 ~= 3220 ticks (~80.5 seconds), the two bounds invert and the ramp breaks,
    /// collapsing blizzard intensity to (near enough) zero for the whole cycle -- exactly
    /// "the blizzard not visually appearing." A ~24000-tick (10-minute) baseline cycle reduced to
    /// 13% of itself lands at ~3120 ticks, right at that exact mathematical breaking point -- which
    /// is almost certainly the actual bug the earlier attempt hit, not a coincidence. MinCycleLength
    /// below enforces a hard floor comfortably above 3220 ticks, applied AFTER this gimmick's own
    /// percentage, so no configured value can ever reproduce it.
    /// </summary>
    [HarmonyPatch(typeof(RainCycle), MethodType.Constructor, new Type[] { typeof(World), typeof(float) })]
    internal static class RainTimerGimmick
    {
        public const string ID = "RainTimer";

        /// <summary>Hard floor on the post-percentage cycle length, in ticks (40/second) -- see
        /// this class's own header comment for exactly where the real breaking point (~3220 ticks)
        /// comes from. 4200 leaves a comfortable margin above it (~105 seconds) rather than tuning
        /// to the exact boundary.</summary>
        private const int MinCycleLengthTicks = 4200;

        private static void Postfix(RainCycle __instance, World world)
        {
            RainWorldGame game = world?.game;
            if (game == null || !PrimalSlot.IsActive(game.rainWorld))
            {
                return;
            }

            string characterID = game.StoryCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            int originalCycleLength = __instance.cycleLength;
            if (originalCycleLength <= 0)
            {
                return;
            }

            int percent = RainTimerSettings.GetPercent(characterID);
            int scaledCycleLength = Mathf.Max(MinCycleLengthTicks, (int)((float)originalCycleLength * (percent / 100f)));

            float ratio = (float)scaledCycleLength / (float)originalCycleLength;
            __instance.cycleLength = scaledCycleLength;
            __instance.sunDownStartTime = Mathf.Max(1, (int)((float)__instance.sunDownStartTime * ratio));

            Plugin.Log?.LogInfo($"[RainTimer] {characterID}: cycle length {originalCycleLength} -> {scaledCycleLength} ticks ({percent}%)");
        }
    }

    /// <summary>Per-character settings for RainTimerGimmick: what percentage of the normal
    /// (vanilla-computed) cycle length rain/blizzard actually takes to arrive. 100% is unmodified
    /// vanilla timing; below 100% is faster, above 100% is slower.</summary>
    internal static class RainTimerSettings
    {
        private const int DefaultPercent = 100;
        private const int MinPercent = 10;
        private const int MaxPercent = 400;

        private static readonly Dictionary<string, Configurable<int>> _percent = new Dictionary<string, Configurable<int>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why.
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            _percent[characterID] = owner.config.Bind(
                characterID + "_RainTimer_percent",
                DefaultPercent,
                new ConfigurableInfo(
                    "How long rain (or Saint's blizzard) takes to arrive, as a percentage of the normal vanilla timing. 100 = unmodified. Below 100 arrives faster, above 100 arrives slower. A hard floor well clear of a known rendering bug applies no matter how low this is set.",
                    new ConfigAcceptableRange<int>(MinPercent, MaxPercent)));
        }

        public static int GetPercent(string characterID)
        {
            return _percent.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultPercent;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            if (!_percent.TryGetValue(characterID, out Configurable<int> percent))
            {
                // Not bound yet -- don't cache an incomplete/empty result, in case Bind() hasn't
                // run for this character yet and a later call could succeed.
                return new List<UIelement>();
            }

            var items = new List<UIelement>();
            items.Add(new OpLabel(20f, topY, "Rain/blizzard speed, as % of normal (100 = vanilla):", false));
            items.Add(new OpUpdown(percent, new Vector2(280f, topY - 6f), 80f));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
