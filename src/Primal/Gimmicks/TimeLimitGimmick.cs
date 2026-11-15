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
    /// Two clocks are available:
    ///  - "per run": vanilla's own persistent campaign play time, `SaveState.totTime +
    ///    DeathPersistentSaveData.deathTime` (seconds) plus the cycle in progress. See RunSeconds
    ///    below for the full derivation. This is the same number the character-select "continue"
    ///    page and the end-of-campaign statistics screen show.
    ///  - "per cycle": World.rainCycle.timer, an incrementing tick counter for the CURRENT cycle
    ///    only -- it's what vanilla itself compares against cycle length to decide when rain
    ///    starts, and it resets to 0 at the start of every new cycle.
    ///
    /// **Fixed 2026-11-15: "per run" previously did not measure a run.** It read
    /// RainWorldGame.clock, and this comment claimed that field resets "only when a fresh
    /// RainWorldGame is created (a brand new game, or continuing/reloading one) -- NOT between
    /// cycles". The first half is true and the conclusion was false, because *continuing from a
    /// shelter is itself a fresh RainWorldGame*: ProcessManager does `if (ID == ProcessID.Game)
    /// currentMainLoop = new RainWorldGame(this)` unconditionally (ProcessManager.cs:1004), and
    /// KarmaLadderScreen -- the base class of BOTH the sleep and death screens -- continues by
    /// requesting exactly that switch (KarmaLadderScreen.cs:278). `clock` is a plain int field with
    /// no save/restore, so it restarted from 0 every cycle AND after every death, making "per run"
    /// behave almost identically to "per cycle" and contradicting its own UI description. That was
    /// the root cause of the long-standing report that the timer "acted cycle-based" and that
    /// "respawning didn't instantly re-kill, had to wait another minute". `game.clock` is no longer
    /// read by this gimmick at all -- don't reintroduce it for anything run-scoped.
    ///
    /// Tick rate (still relevant for the per-cycle path): RainWorldGame sets framesPerSecond = 40
    /// at the top of every RawUpdate. It can be lowered from there (Adrenaline, VoidMelt,
    /// redsIllness, MMF slow-time, ghostMode) or forced to 10/400 by dev-tools keys, but it is
    /// never raised above 40 in ordinary play -- and a lower rate means fewer ticks per real
    /// second, i.e. the limit takes LONGER in real time, never shorter. So a too-early kill is not
    /// a tick-rate problem; check the character's saved `_TimeLimit_minutes` value first, which is
    /// what actually caused the 2026-11-15 report (it was 1, not the source default of 60 -- see
    /// AI_CONTEXT.md's Gotcha #9).
    ///
    /// World.rainCycle.timer and Creature.Die() are accessed directly here (not through
    /// reflection) despite this codebase's usual caution around direct member
    /// access against the game's real assembly (see AI_CONTEXT.md's Gotcha #1 -- the publicized
    /// reference DLL this mod compiles against marks everything public whether or not it really is
    /// in the shipped game). They were specifically verified for this exact game install (not
    /// just assumed) by disassembling other already-installed, already-working mods with ikdasm
    /// and confirming they perform the exact same direct access without incident: Rain Meadow
    /// (workshop 3388224007) reads RainWorldGame.clock and World.rainCycle.timer via plain field
    /// access, and separately calls Creature.Die() via a plain virtual call, in several places.
    ///
    /// The per-run path additionally reads StoryGameSession.saveState / .playerSessionRecords and
    /// SaveState.totTime / .deathPersistentSaveData.deathTime directly. The
    /// `GetStorySession?.saveState?...` chain is already load-bearing elsewhere in this mod
    /// (NoHoldGimmick, OneSpearGimmick, PermanentShelterFailureGimmick, PrimalKillTracker) and is
    /// confirmed working in-game, so it carries no new Gotcha #1 risk; totTime/deathTime/
    /// playerSessionRecords are new surface, and if they ever throw FieldAccessException it will
    /// show up in exceptionLog.txt as a per-tick exception rather than a BepInEx error.
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
            if (!GimmickGate.IsActiveFor(self, ID, out string characterID))
            {
                return;
            }

            // The gate above can resolve its RainWorld through the abstract creature rather than
            // the realized room (see GimmickGate), so don't assume a room exists just because the
            // gate passed -- both tick sources below need one.
            RainWorldGame game = GimmickGate.GameFor(self);
            if (game == null)
            {
                return;
            }

            bool perCycle = TimeLimitSettings.GetPerCycle(characterID);
            int limitSeconds = TimeLimitSettings.GetMinutes(characterID) * 60;
            int elapsedSeconds = perCycle
                ? (self.room?.world?.rainCycle?.timer ?? 0) / TicksPerSecond
                : RunSeconds(game);

            if (elapsedSeconds < limitSeconds)
            {
                return;
            }

            if (_alreadyKilled.TryGetValue(self, out _))
            {
                return;
            }
            _alreadyKilled.Add(self, null);

            // Logs the LIVE limit, not just that something happened -- per Gotcha #9, a saved
            // Remix value silently outranks the source default forever, and this line is what
            // makes "it killed me way too early" answerable without guessing.
            Plugin.Log?.LogInfo($"[TimeLimit] {characterID}: limit reached ({elapsedSeconds}/{limitSeconds} s, {(perCycle ? "per cycle" : "per run")}) -- killing character");
            self.Die();
        }

        /// <summary>Total play time for this save, in seconds -- vanilla's own persistent campaign
        /// clock, i.e. the number the character-select "continue" page and the end-of-campaign
        /// statistics screen both report.
        ///
        /// `SaveState.totTime` accumulates survived cycles and
        /// `DeathPersistentSaveData.deathTime` accumulates died/ghosted ones; StoryGameSession
        /// .AppendTimeOnCycleEnd commits one or the other at every cycle end (seconds =
        /// ticks / 40), and neither is reset to 0 anywhere in the game's own code -- a fresh save
        /// simply starts at 0. That is what makes this a genuine per-RUN measure, unlike
        /// RainWorldGame.clock, which this used to read and which restarts from 0 every cycle (see
        /// the known-bug note in this class's header).
        ///
        /// Those two fields only advance at cycle END, so the cycle in progress is added from
        /// `playerSessionRecords[0].time` -- the same live tick counter AppendTimeOnCycleEnd is
        /// about to divide by 40 and commit, incremented in Player.Update. Without it the limit
        /// could only ever trigger in the first moments after a cycle began.
        ///
        /// Deliberately NOT MoreSlugcats' SpeedRunTimer, even though its HUD readout is the
        /// toggleable in-game clock showing this same time: that tracker is MSC-only and is seeded
        /// FROM these two fields (SlugcatSelectMenu.cs's LoadOldTimings(gameTimeAlive,
        /// gameTimeDead) call), so these are the more robust authority for the same number.
        ///
        /// Returns 0 (never kill) rather than guessing if there is no story save state -- e.g. an
        /// arena session, where GetStorySession is null.</summary>
        private static int RunSeconds(RainWorldGame game)
        {
            StoryGameSession session = game.GetStorySession;
            SaveState saveState = session?.saveState;
            if (saveState == null)
            {
                return 0;
            }

            int seconds = saveState.totTime + (saveState.deathPersistentSaveData?.deathTime ?? 0);

            PlayerSessionRecord[] records = session.playerSessionRecords;
            if (records != null && records.Length > 0 && records[0] != null)
            {
                seconds += records[0].time / TicksPerSecond;
            }

            return seconds;
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
