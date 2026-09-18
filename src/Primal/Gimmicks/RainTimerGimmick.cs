using System;
using System.Collections.Generic;
using HarmonyLib;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Rain Timer" gimmick: speeds up or slows down how quickly rain (or, on Saint, the
    /// region-wide blizzard -- see below for why the two are the same mechanism) arrives each
    /// cycle, as a percentage of vanilla's own normal timing -- all the way down to 0% (rain hits
    /// essentially immediately) if configured that way.
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
    /// **The known "blizzard doesn't render" bug -- root-caused and FIXED, 2026-09-18 (previously
    /// only floored around):** an earlier, non-mod attempt at this same feature (human-programmer
    /// implementation, before this port) found that pushing the timer below roughly 13% of its
    /// normal length made the blizzard stop rendering. First pass at this gimmick (also 2026-09-18)
    /// only kept `cycleLength` above a floor (4200 ticks) comfortably clear of the breaking point --
    /// which worked, but meant "0% / immediate rain" wasn't actually reachable. Root-caused (not
    /// just avoided) here by reading BlizzardGraphics.CycleUpdate (MoreSlugcats namespace) directly:
    /// for Saint's own home region (the "glacial wasteland" branch, which is what a normal ambient
    /// blizzard actually uses -- AerieBlizzard-flagged rooms take a separate, cycleLength-INdependent
    /// branch using fixed absolute-tick thresholds instead, and were never at risk), blizzard
    /// intensity is `Mathf.InverseLerp(cycleLength * 0.932f, 3000f, TimeUntilRain)`. Worked through
    /// the actual arithmetic (not just eyeballing the shape) for `cycleLength` under ~3220 ticks
    /// (3000/0.932): the ramp's "far" bound (`cycleLength * 0.932`) drops BELOW its "near" bound
    /// (a hardcoded 3000), which doesn't merely clamp to a flat 0 or 1 -- Mathf.InverseLerp still
    /// computes a valid fraction either way, but with the bounds inverted the ramp now runs
    /// BACKWARDS: intensity is a small nonzero value right at cycle START (when `TimeUntilRain` is
    /// still large, near `cycleLength`) and collapses to a clamped 0 well BEFORE rain/blizzard
    /// actually hits (once `TimeUntilRain` drops below `cycleLength * 0.932`) -- i.e. the blizzard
    /// visibly fades out just when it should be most intense, exactly "not visually appearing."
    /// (Confirmed by hand-computing both an above-threshold case, where the ramp behaves normally
    /// just compressed into a short window near the end, and a below-threshold case, where it
    /// inverts as described -- this was not a guess.)
    ///
    /// **The actual fix: `BlizzardVisualCycleLengthHook` below**, a *separate*, *unconditional*
    /// Harmony patch on `BlizzardGraphics.CycleUpdate` itself (not gated on this gimmick, or on any
    /// gimmick -- it's a correctness fix for a real vanilla bug, and has literally zero effect on
    /// unmodified vanilla, since default cycle lengths are always far above the breaking point;
    /// it only ever does anything once something -- this gimmick, another mod's own multiplier,
    /// whatever -- has pushed `cycleLength` below the safe rendering floor). It temporarily
    /// substitutes a safe floor for `cycleLength` for the exact duration of one `CycleUpdate` call
    /// (the ONE method in the whole game that ever reads `cycleLength` for rendering purposes),
    /// then restores the real value immediately after -- so the actual GAMEPLAY timer (when rain
    /// mechanically hits, hypothermia math, etc.) is free to go all the way down to
    /// `RainTimerGimmick.MinCycleLengthTicks` (now just large enough to avoid literal
    /// division-by-zero elsewhere in RainCycle's own math -- CycleProgression, AmountLeft, and
    /// several others all divide directly by `cycleLength`), while the purely-cosmetic blizzard
    /// rendering always sees a value comfortably clear of the ~3220-tick breaking point, no matter
    /// how aggressively this gimmick's percentage has been turned down.
    /// </summary>
    [HarmonyPatch(typeof(RainCycle), MethodType.Constructor, new Type[] { typeof(World), typeof(float) })]
    internal static class RainTimerGimmick
    {
        public const string ID = "RainTimer";

        /// <summary>Hard floor on the post-percentage cycle length, in ticks (40/second) --
        /// purely to avoid literal division-by-zero elsewhere in RainCycle's own math
        /// (CycleProgression, AmountLeft, ProximityToMiddleOfCycle, etc. all divide directly by
        /// `cycleLength`; a value of exactly 0 would produce NaN/Infinity there). 40 ticks (1
        /// second) is as close to "immediate" as can safely be configured -- the separate blizzard
        /// RENDERING bug this used to also guard against is now fixed at its actual source (see
        /// this class's own header comment, "The actual fix"), so this floor no longer needs to be
        /// anywhere near as conservative as it originally was.</summary>
        private const int MinCycleLengthTicks = 40;

        /// <summary>Used instead of the percentage math when RainTimerSettings' "Disable timer"
        /// checkbox is checked for this character -- see that class's own header comment for the
        /// UI, and why this is a large-but-bounded sentinel rather than trying to truly suppress
        /// rain forever (which would mean patching the onset check itself, a second, separate piece
        /// of vanilla logic, for no real player-facing difference). ~100 million ticks is roughly
        /// 29 days of continuous, uninterrupted simulated time -- rain effectively never arrives in
        /// any real play session, while staying comfortably clear of int32 overflow if anything
        /// downstream ever multiplies it by a small factor (unlike sunDownStartTime's own ratio
        /// scaling below, which is skipped entirely in this branch for exactly that reason).</summary>
        private const int UnlimitedCycleLengthTicks = 100_000_000;

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

            if (RainTimerSettings.IsDisabled(characterID))
            {
                // Deliberately does NOT scale sunDownStartTime here (unlike the normal branch
                // below) -- day/night keeps behaving exactly like vanilla while only rain itself is
                // pushed out to practically-never, rather than also stretching sunset out to an
                // equally huge, meaningless value.
                __instance.cycleLength = UnlimitedCycleLengthTicks;
                Plugin.Log?.LogInfo($"[RainTimer] {characterID}: timer disabled -- rain effectively never arrives");
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

    /// <summary>
    /// General bugfix for BlizzardGraphics.CycleUpdate's own "glacial wasteland" intensity ramp,
    /// which breaks (see RainTimerGimmick's own header comment, "The known 'blizzard doesn't
    /// render' bug," for the full derivation) once RainCycle.cycleLength drops below roughly 3220
    /// ticks. NOT gated on this gimmick, PrimalSlot, or any GimmickAssignments check -- it's a
    /// correctness fix that only ever changes behavior once cycleLength is already abnormally
    /// short (never true in untouched vanilla), so it's safe and correct to apply unconditionally
    /// rather than trying to scope it to exactly the characters/situations that might trigger it.
    ///
    /// Mechanism: `CycleUpdate` is the ONLY method anywhere in the game that reads `cycleLength`
    /// for a rendering purpose (confirmed by reading the whole method -- every other field it uses,
    /// `TimeUntilRain`/`CycleProgression`, are themselves derived from `cycleLength` too, so
    /// substituting the one field covers all of them for the span of this one call). Prefix swaps
    /// in a safe floor if the real value is below it; Postfix restores the real value immediately
    /// after. Nothing outside this one call (RainCycle.Update, Creature.HypothermiaUpdate, the
    /// actual rain-onset check, etc.) ever sees the substituted value -- only this method's own
    /// wind/blizzard/snow/whiteout intensity math for this one frame does.
    /// </summary>
    [HarmonyPatch(typeof(BlizzardGraphics), nameof(BlizzardGraphics.CycleUpdate))]
    internal static class BlizzardVisualCycleLengthHook
    {
        /// <summary>Comfortably above the real ~3220-tick breaking point (3000 / 0.932) derived in
        /// RainTimerGimmick's own header comment, so the "glacial wasteland" ramp always has a sane
        /// window to work with no matter how short the real gameplay cycleLength has been
        /// configured.</summary>
        private const int VisualMinCycleLengthTicks = 4200;

        // Rain World's simulation is single-threaded and CycleUpdate doesn't call itself
        // reentrantly (it's not recursive, and one BlizzardGraphics instance's Update -> CycleUpdate
        // call never triggers another instance's), so a simple pair of static fields -- save before,
        // restore after -- is safe here the same way BlizzardTimerGimmick's own current-creature
        // stash is.
        private static int _savedCycleLength;
        private static bool _substituted;

        private static void Prefix(BlizzardGraphics __instance)
        {
            _substituted = false;

            RainCycle rainCycle = __instance.room?.world?.rainCycle;
            if (rainCycle == null || rainCycle.cycleLength >= VisualMinCycleLengthTicks)
            {
                return;
            }

            _savedCycleLength = rainCycle.cycleLength;
            rainCycle.cycleLength = VisualMinCycleLengthTicks;
            _substituted = true;
        }

        private static void Postfix(BlizzardGraphics __instance)
        {
            if (!_substituted)
            {
                return;
            }

            RainCycle rainCycle = __instance.room?.world?.rainCycle;
            if (rainCycle != null)
            {
                rainCycle.cycleLength = _savedCycleLength;
            }
            _substituted = false;
        }
    }

    /// <summary>Per-character settings for RainTimerGimmick: what percentage of the normal
    /// (vanilla-computed) cycle length rain/blizzard actually takes to arrive. 100% is unmodified
    /// vanilla timing; below 100% is faster (down to 0% -- essentially immediate, clamped only by
    /// RainTimerGimmick.MinCycleLengthTicks' anti-divide-by-zero floor), above 100% is slower.
    /// A separate "Disable timer" checkbox (per user request, in place of the originally-discussed
    /// idea of a special "Unlimited" notch at the end of the numeric range) overrides the percent
    /// entirely -- checked, rain/the blizzard effectively never arrives (see
    /// RainTimerGimmick.UnlimitedCycleLengthTicks); the numeric field and its label grey out to
    /// show they're inert while it's checked, the same "visible but disabled" convention
    /// NoPassagesGimmick already uses elsewhere in this mod.</summary>
    internal static class RainTimerSettings
    {
        private const int DefaultPercent = 100;

        /// <summary>Saint's own default (see GimmickDefaults.cs) -- rain/the blizzard should start
        /// essentially immediately, per explicit request. 0% clamps to
        /// RainTimerGimmick.MinCycleLengthTicks (40 ticks, 1 second) rather than a literal instant
        /// zero, since a true 0 would divide-by-zero elsewhere in RainCycle's own math.</summary>
        private const int SaintDefaultPercent = 0;

        private const int MinPercent = 0;
        private const int MaxPercent = 400;

        private static readonly Dictionary<string, Configurable<int>> _percent = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<bool>> _disabled = new Dictionary<string, Configurable<bool>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why.
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            int defaultPercent = characterID == MoreSlugcatsEnums.SlugcatStatsName.Saint.value
                ? SaintDefaultPercent
                : DefaultPercent;

            _percent[characterID] = owner.config.Bind(
                characterID + "_RainTimer_percent",
                defaultPercent,
                new ConfigurableInfo(
                    "How long rain (or Saint's blizzard) takes to arrive, as a percentage of the normal vanilla timing. 100 = unmodified. Below 100 arrives faster (0 = essentially immediate), above 100 arrives slower. Ignored entirely while \"Disable timer\" is checked.",
                    new ConfigAcceptableRange<int>(MinPercent, MaxPercent)));

            _disabled[characterID] = owner.config.Bind(
                characterID + "_RainTimer_disabled",
                false,
                new ConfigurableInfo("If checked, rain (or Saint's blizzard) effectively never arrives -- the percentage above is ignored while this is checked."));
        }

        public static int GetPercent(string characterID)
        {
            return _percent.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultPercent;
        }

        public static bool IsDisabled(string characterID)
        {
            return _disabled.TryGetValue(characterID, out Configurable<bool> c) && c.Value;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            if (!_percent.TryGetValue(characterID, out Configurable<int> percent) ||
                !_disabled.TryGetValue(characterID, out Configurable<bool> disabled))
            {
                // Not bound yet -- don't cache an incomplete/empty result, in case Bind() hasn't
                // run for this character yet and a later call could succeed.
                return new List<UIelement>();
            }

            var items = new List<UIelement>();

            var label = new OpLabel(20f, topY, "Timer multiplier (% of normal, 0 = immediate):", false);
            var updown = new OpUpdown(percent, new Vector2(280f, topY - 6f), 80f);
            var disableCheckbox = new OpCheckBox(disabled, 380f, topY - 6f);
            var disableLabel = new OpLabel(416f, topY - 2f, "Disable timer", false);

            // Captured once, right after construction -- the label's own default color, so
            // "re-enabling" restores exactly what it started as rather than guessing at a
            // hardcoded "normal" color.
            Color enabledLabelColor = label.color;

            void ApplyDisabledVisual(bool isDisabled)
            {
                updown.greyedOut = isDisabled;
                label.color = isDisabled ? Color.grey : enabledLabelColor;
            }

            ApplyDisabledVisual(disabled.Value);
            disabled.OnChange += () => ApplyDisabledVisual(disabled.Value);

            items.Add(label);
            items.Add(updown);
            items.Add(disableCheckbox);
            items.Add(disableLabel);

            _uiCache[characterID] = items;
            return items;
        }
    }
}
