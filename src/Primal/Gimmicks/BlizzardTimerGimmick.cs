using System.Collections.Generic;
using HarmonyLib;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Blizzard Timer" gimmick: controls how many minutes into a cycle -- counted from the
    /// very start of the cycle, the way a player actually experiences it, not from whenever
    /// rain/blizzard happens to hit -- the blizzard's heat-drain mechanic suddenly jumps from a
    /// gentle background rate to its maximum possible severity.
    ///
    /// Traced to Creature.HypothermiaUpdate() (root namespace, no reflection needed):
    /// <code>
    /// float num3 = (float)room.world.rainCycle.cycleLength + (float)RainWorldGame.BlizzardHardEndTimer(room.game.IsStorySession);
    /// HypothermiaGain += Mathf.Lerp(0f, RainWorldGame.BlizzardMaxColdness, Mathf.InverseLerp(0f, num3, room.world.rainCycle.timer));
    /// HypothermiaGain += Mathf.Lerp(0f, 50f, Mathf.InverseLerp(num3, num3 * 5f, room.world.rainCycle.timer));
    /// ...
    /// HypothermiaGain = Mathf.Clamp(HypothermiaGain, -1f, 0.0055f);
    /// </code>
    /// `RainWorldGame.BlizzardHardEndTimer(storyMode)` is a hardcoded constant -- 32000 ticks
    /// (800 seconds, 13m20s) in story mode -- added on top of `cycleLength` to get `num3`, the tick
    /// count (measured from cycle START, since `RainCycle.timer` keeps counting up even after
    /// rain/blizzard has already hit -- `RainCycle.Update()`'s `timer++` is unconditional) at which
    /// the third line's `+50` term explodes past the final `Clamp(..., 0.0055f)` ceiling. The moment
    /// `timer` crosses `num3`, per-tick cold gain jumps from a gentle ramp (capped near
    /// `BlizzardMaxColdness ~= 0.0016`/tick) to the hard maximum the game allows -- effectively
    /// instant, not a gradual escalation -- which is exactly the "suddenly gets significantly
    /// harsher" the user described. Confirmed `BlizzardHardEndTimer` has exactly one call site in
    /// the whole game (this one), so patching it can't have any effect anywhere else.
    ///
    /// **Unit, per explicit user choice after being shown how the timer actually works: "Total
    /// minutes into the cycle"** -- i.e. this gimmick's setting is the number of minutes since the
    /// cycle started (RainCycle.timer == 0), not the raw post-rain delay vanilla hardcodes. Since
    /// `num3 = cycleLength + BlizzardHardEndTimer(...)`, and this gimmick knows the character's
    /// live `cycleLength` at the moment it's asked (including whatever RainTimerGimmick may have
    /// separately rescaled it to -- the two compose correctly with no double-counting: this always
    /// resolves to "harshens at minute N of the cycle," regardless of how fast the cycle itself is
    /// currently running), it overrides ONLY BlizzardHardEndTimer's return value, computed as
    /// `(configured total ticks) - cycleLength`, clamped to never go negative (if the configured
    /// total is earlier than rain/blizzard even arrives, harshening simply starts right when it
    /// does, at tick 0 of the post-rain delay, rather than going negative/nonsensical).
    ///
    /// **Why a Postfix on the static BlizzardHardEndTimer instead of touching cycleLength
    /// directly:** `cycleLength` is read a SECOND time a few lines later in the very same method,
    /// for a completely unrelated wind-chill sub-term
    /// (`Mathf.InverseLerp(0f, (float)room.world.rainCycle.cycleLength + 4300f, ...)`) -- temporarily
    /// substituting `cycleLength` for the duration of the call (the more obvious-looking approach)
    /// would have silently thrown that second, unrelated term off too. Patching the one hardcoded
    /// constant that's actually the point of this gimmick, and nothing else, avoids that entirely.
    ///
    /// **How the per-character gate reaches a call with no Player/Creature parameter:**
    /// `BlizzardHardEndTimer(bool storyMode)` is a bare static helper -- it has no way to know which
    /// creature is asking. `BlizzardTimerHypothermiaGateHook` (a Prefix + Postfix pair on
    /// Creature.HypothermiaUpdate(), the method's own single caller) stashes/clears __instance in
    /// `CurrentCreature` below around the call, so `BlizzardHardEndTimerHook`'s Postfix can read
    /// back "which creature is this for" despite the static method itself carrying no such context.
    /// Safe because Rain World's simulation is single-threaded and HypothermiaUpdate never calls
    /// itself reentrantly (no creature's own HypothermiaUpdate triggers another creature's).
    /// </summary>
    internal static class BlizzardTimerGimmick
    {
        public const string ID = "BlizzardTimer";

        /// <summary>Which Creature's HypothermiaUpdate() is currently executing, if any -- set by
        /// BlizzardTimerHypothermiaGateHook, read by BlizzardHardEndTimerHook. See this class's own
        /// header comment for why this indirection exists.</summary>
        internal static Creature CurrentCreature;
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.HypothermiaUpdate))]
    internal static class BlizzardTimerHypothermiaGateHook
    {
        private static void Prefix(Creature __instance)
        {
            BlizzardTimerGimmick.CurrentCreature = __instance;
        }

        private static void Postfix()
        {
            BlizzardTimerGimmick.CurrentCreature = null;
        }
    }

    [HarmonyPatch(typeof(RainWorldGame), nameof(RainWorldGame.BlizzardHardEndTimer))]
    internal static class BlizzardHardEndTimerHook
    {
        private static void Postfix(ref int __result)
        {
            Creature creature = BlizzardTimerGimmick.CurrentCreature;
            if (!(creature is Player player))
            {
                return;
            }

            if (!GimmickGate.IsActiveFor(player, BlizzardTimerGimmick.ID, out string characterID))
            {
                return;
            }

            RainCycle rainCycle = creature.room?.world?.rainCycle;
            if (rainCycle == null)
            {
                return;
            }

            int totalTicks = BlizzardTimerSettings.GetMinutes(characterID) * 60 * TimeLimitGimmick.TicksPerSecond;
            __result = Mathf.Max(0, totalTicks - rainCycle.cycleLength);
        }
    }

    /// <summary>Per-character settings for BlizzardTimerGimmick: how many minutes into the cycle
    /// (counted from cycle start, not from whenever rain/blizzard actually hits) the blizzard's
    /// heat-drain mechanic suddenly maxes out. Default (25 min) approximates vanilla's own typical
    /// timing for a Saint-length cycle, per this gimmick's own header comment -- vanilla has no
    /// single constant for this (it depends on the region's own cycle length), so this is a
    /// reasonable starting point rather than an exact reproduction of any specific region. Saint
    /// himself (see GimmickDefaults.cs) gets his own explicitly-requested default of 60 minutes
    /// instead of the generic 25.</summary>
    internal static class BlizzardTimerSettings
    {
        private const int DefaultMinutes = 25;

        /// <summary>Saint's own default (see GimmickDefaults.cs), per explicit request: "the
        /// blizzard timer set to 1 hour."</summary>
        private const int SaintDefaultMinutes = 60;

        private const int MinMinutes = 1;
        private const int MaxMinutes = 999;

        private static readonly Dictionary<string, Configurable<int>> _minutes = new Dictionary<string, Configurable<int>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why.
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            int defaultMinutes = characterID == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint.value
                ? SaintDefaultMinutes
                : DefaultMinutes;

            _minutes[characterID] = owner.config.Bind(
                characterID + "_BlizzardTimer_minutes",
                defaultMinutes,
                new ConfigurableInfo(
                    "Minutes into the cycle (counted from the cycle's own start, not from whenever rain/blizzard actually hits) before the blizzard's heat-drain mechanic suddenly maxes out.",
                    new ConfigAcceptableRange<int>(MinMinutes, MaxMinutes)));
        }

        public static int GetMinutes(string characterID)
        {
            return _minutes.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultMinutes;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            if (!_minutes.TryGetValue(characterID, out Configurable<int> minutes))
            {
                // Not bound yet -- don't cache an incomplete/empty result, in case Bind() hasn't
                // run for this character yet and a later call could succeed.
                return new List<UIelement>();
            }

            var items = new List<UIelement>();
            items.Add(new OpLabel(20f, topY, "Minutes into the cycle before it turns harsh:", false));
            items.Add(new OpUpdown(minutes, new Vector2(280f, topY - 6f), 80f));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
