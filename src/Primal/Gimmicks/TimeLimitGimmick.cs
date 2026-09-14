using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Time Limit" gimmick: a hard playtime cap. Once the configured clock (see
    /// TimeLimitSettings below -- minutes, and whether that's measured per-cycle or for the whole
    /// run) has elapsed, the character dies outright, that same tick. No warning, no game-over
    /// presentation yet -- per the user's own spec, that's a later step; for now this is just the
    /// kill.
    ///
    /// Two clocks are available, both real vanilla fields:
    ///  - RainWorldGame.clock ("per run"): an int the game itself increments by exactly 1 every
    ///    Player.Update() tick, for as long as the current RainWorldGame instance exists. Resets
    ///    only when a fresh RainWorldGame is created (a brand new game, or continuing/reloading
    ///    one) -- NOT between cycles.
    ///  - World.rainCycle.timer ("per cycle"): the same kind of incrementing tick counter, but for
    ///    the CURRENT cycle only -- it's what vanilla itself compares against cycle length to
    ///    decide when rain starts, and it resets to 0 at the start of every new cycle.
    /// Both tick at MainLoopProcess's fixed simulation rate of 40 ticks/second (framesPerSecond =
    /// 40, set in MainLoopProcess's own constructor and never changed by RainWorldGame), so
    /// N minutes = N * 60 * 40 ticks either way.
    ///
    /// RainWorldGame.clock, World.rainCycle.timer, and Creature.Die() are all accessed directly
    /// here (not through reflection) despite this codebase's usual caution around direct member
    /// access against the game's real assembly (see AI_CONTEXT.md's Gotcha #1 -- the publicized
    /// reference DLL this mod compiles against marks everything public whether or not it really is
    /// in the shipped game). All three were specifically verified for this exact game install (not
    /// just assumed) by disassembling other already-installed, already-working mods with ikdasm
    /// and confirming they perform the exact same direct access without incident: Rain Meadow
    /// (workshop 3388224007) reads RainWorldGame.clock and World.rainCycle.timer via plain field
    /// access, and separately calls Creature.Die() via a plain virtual call, in several places.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.Update), new Type[] { typeof(bool) })]
    internal static class TimeLimitGimmick
    {
        public const string ID = "TimeLimit";

        /// <summary>Ticks per real-world second at RainWorldGame's fixed simulation rate.</summary>
        public const int TicksPerSecond = 40;

        // Guards against calling Die() more than once for the same player -- the relevant clock
        // keeps climbing past the threshold for as long as the character survives whatever Die()
        // itself does, so without this the Postfix below would otherwise invoke Die() again every
        // single tick.
        private static readonly ConditionalWeakTable<Player, object> _alreadyKilled =
            new ConditionalWeakTable<Player, object>();

        private static void Postfix(Player __instance)
        {
            Player self = __instance;
            if (self?.playerState == null)
            {
                return;
            }

            RainWorld rainWorld = self.room?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            int limitTicks = TimeLimitSettings.GetMinutes(characterID) * 60 * TicksPerSecond;
            bool perCycle = TimeLimitSettings.GetPerCycle(characterID);
            int currentTicks = perCycle ? (self.room.world?.rainCycle?.timer ?? 0) : self.room.game.clock;

            if (currentTicks < limitTicks)
            {
                return;
            }

            if (_alreadyKilled.TryGetValue(self, out _))
            {
                return;
            }
            _alreadyKilled.Add(self, null);

            Plugin.Log?.LogInfo($"[TimeLimit] {characterID}: limit reached ({currentTicks}/{limitTicks} ticks, {(perCycle ? "per cycle" : "per run")}) -- killing character");
            self.Die();
        }
    }

    /// <summary>Per-character settings for TimeLimitGimmick: how many minutes, and whether that's
    /// measured against the current cycle or the whole run. See GimmickDef.cs for how
    /// BindSettings/BuildSettingsUI plug into PrimalOptions's drill-down menu.</summary>
    internal static class TimeLimitSettings
    {
        private const int DefaultMinutes = 60;
        private const int MinMinutes = 1;
        private const int MaxMinutes = 999;

        private static readonly Dictionary<string, Configurable<int>> _minutes = new Dictionary<string, Configurable<int>>();
        private static readonly Dictionary<string, Configurable<bool>> _perCycle = new Dictionary<string, Configurable<bool>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why (Configurable<T> can only ever be bound to one
        // UIconfig element at a time; building fresh OpUpdown/OpCheckBox instances every time this
        // sub-view opened threw Menu.Remix.MultiuseConfigurableException the second time around).
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            _minutes[characterID] = owner.config.Bind(
                characterID + "_TimeLimit_minutes",
                DefaultMinutes,
                new ConfigurableInfo("Minutes of play before the character dies.", new ConfigAcceptableRange<int>(MinMinutes, MaxMinutes)));

            _perCycle[characterID] = owner.config.Bind(
                characterID + "_TimeLimit_perCycle",
                false,
                new ConfigurableInfo("If checked, the limit resets every cycle instead of counting the whole run."));
        }

        public static int GetMinutes(string characterID)
        {
            return _minutes.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultMinutes;
        }

        public static bool GetPerCycle(string characterID)
        {
            return _perCycle.TryGetValue(characterID, out Configurable<bool> c) && c.Value;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            var items = new List<UIelement>();
            if (!_minutes.TryGetValue(characterID, out Configurable<int> minutes) ||
                !_perCycle.TryGetValue(characterID, out Configurable<bool> perCycle))
            {
                // Not bound yet -- don't cache an incomplete/empty result, in case Bind() hasn't
                // run for this character yet and a later call could succeed.
                return items;
            }

            items.Add(new OpLabel(20f, topY, "Time limit, in minutes:", false));
            items.Add(new OpUpdown(minutes, new Vector2(220f, topY - 6f), 80f));

            items.Add(new OpCheckBox(perCycle, 20f, topY - 44f));
            items.Add(new OpLabel(56f, topY - 40f, "Measure per cycle instead of per run", false)
            {
                description = "Unchecked (default): the limit counts the whole continuous run. Checked: the limit resets every cycle."
            });

            _uiCache[characterID] = items;
            return items;
        }
    }
}
