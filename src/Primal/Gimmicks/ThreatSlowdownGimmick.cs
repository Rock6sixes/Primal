using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Threat Slowdown" gimmick: as vanilla's own threat level climbs toward "threat level 1"
    /// (the user's own term -- see below), the character progressively loses the ability to act at
    /// all, reaching complete lockout right as threat level 1 is reached.
    ///
    /// RENAMED 2026-09-13 -- this is a straight relocation, not a new mechanic. This ID
    /// ("ThreatSlowdown") used to implement a totally different effect (a percentage-based
    /// slowdown of movement/climbing/swimming/throwing stats), while the input-dropping mechanic
    /// described below used to live under the ID "ThreatPanic". The user, after testing both,
    /// asked to swap which name goes with which feel: "rename the gimmick that drops inputs on
    /// threat to threat slowdown and make the threat panic throw random inputs" -- i.e. losing
    /// control outright reads more like grinding to a halt ("slowdown") than like panicking, and
    /// "panic" should instead mean erratic, involuntary action (see ThreatPanicGimmick.cs, which
    /// now implements that). The two gimmicks' IDs were deliberately left unchanged (still
    /// "ThreatSlowdown" and "ThreatPanic") even though their DISPLAY NAMES already matched what
    /// each ID's NEW behavior needed -- only the code inside each file moved, not the
    /// GimmickRegistry.cs entries -- specifically so every character's existing per-character
    /// Remix toggle for either ID (e.g. Monk already had both flipped on) carries over to the new
    /// behavior automatically, instead of resetting because a Configurable&lt;bool&gt;'s persisted
    /// key (characterID + "_" + gimmickID) changed identity. The old percentage-based stat-slowdown
    /// implementation that used to live here is retired, not preserved elsewhere -- the user's
    /// request treats "input dropping" as the intended meaning of "Threat Slowdown" now, not an
    /// addition to it.
    ///
    /// WHAT "THREAT MUSIC PULSE" ACTUALLY IS: a Remix option, found by disassembling
    /// HUD.InitSinglePlayerHud (the method that adds every HUD element): if
    /// `ModManager.MMF &amp;&amp; MMF.cfgThreatMusicPulse.Value`, it adds a `MoreSlugcats.ThreatPulser`
    /// HUD part -- a row of 5 `HUDCircle`s (fixed array size, see ThreatPulser's own constructor)
    /// that fill in one at a time as threat rises, purely as a visual/audio-adjacent cue. That
    /// checkbox only controls whether this HUD element is ever constructed; it has no bearing on
    /// whether the underlying threat value exists or is meaningful, so this gimmick reads that raw
    /// value directly and applies its effect regardless of whether the player has that visual cue
    /// turned on -- there is no reason a gameplay effect should depend on an accessibility/HUD
    /// display setting most players may not have looked at.
    ///
    /// WHERE THE RAW THREAT VALUE COMES FROM: disassembled `ThreatPulser.get_Threat()` (the
    /// property that HUD element itself reads every frame) to find it, rather than guessing at a
    /// name. In order: if `GameOverModeActive`, threat is 0. Otherwise, if the game has a live
    /// `MusicPlayer` (`ProcessManager.musicPlayer`, true for ordinary story-mode play), the value
    /// is `MusicPlayer.threatTracker.currentMusicAgnosticThreat` (`Music.PlayerThreatTracker`, a
    /// float updated every frame by `PlayerThreatTracker.Update()`). Otherwise it falls back to
    /// `ProcessManager.fallbackThreatDetermination.currentMusicAgnosticThreat`
    /// (`ThreatDetermination`, lazily constructed by vanilla itself if missing). This gimmick
    /// mirrors that same lookup+fallback rather than only reading the primary path, so it behaves
    /// identically to the vanilla HUD element even in whatever edge case makes vanilla itself fall
    /// back (no live MusicPlayer instance) -- except it never constructs a missing fallback itself
    /// (unlike vanilla's own getter), reading threat as 0 in that specific edge case instead, since
    /// side-effecting ProcessManager state is not this gimmick's job.
    ///
    /// WHAT "THREAT LEVEL 1" MEANS AS A NUMBER: the user's own description -- "threat pulses seems
    /// to occupy a full circle before moving onto the next, so at threat level 1, there is one
    /// full red circle" -- is exactly `ThreatPulser.Update()`'s own rendering math, disassembled to
    /// pin down the real threshold rather than eyeballing it: it computes
    /// `Mathf.InverseLerp(0f, 1.1f, rawThreat) * circles.Length` (circles.Length is a hardcoded 5,
    /// see the ctor) to decide how many of the 5 circles are lit/filling. One full circle lit means
    /// that expression equals 1, i.e. `rawThreat == 1.1f / 5f` (ThreatLevelOneThreshold below,
    /// 0.22). Both the 1.1 and the 5 are vanilla's own literal constants baked into that rendering
    /// method (not anything configurable), so this is a real derived threshold, not a guess.
    ///
    /// THE EFFECT (moved here from the old ThreatPanicGimmick.cs, unchanged): every tick, the
    /// chance of that tick's already-assembled real input being wiped entirely is
    /// `panicFraction = Clamp01(rawThreat / ThreatLevelOneThreshold)` -- 0% at no threat, ramping
    /// to 100% (permanent lockout) right as the first circle finishes filling. In between, control
    /// visibly stutters/drops with increasing frequency as threat climbs -- a graduated "losing
    /// control", not a single threshold flip. See WHICH INPUT FIELDS GET ZEROED and the
    /// STRUCT-IN-ARRAY REFLECTION notes below for the mechanics.
    ///
    /// WHICH INPUT FIELDS GET ZEROED: `Player.InputPackage`'s `x`/`y` (directional movement, ints),
    /// `analogueDir` (Vector2, analog stick direction), `jmp`/`thrw`/`pckp`/`mp`/`spec`
    /// /`crouchToggle` (all bools) and `downDiagonal` (int) -- every field that represents actual
    /// input state this tick. Deliberately left untouched: `gamePad` and `controllerType`
    /// (`Options.ControlSetup.Preset`) -- these describe which physical input device/mapping is
    /// currently active, not what was pressed, and some UI (button-prompt icons) may read them
    /// expecting them to reflect real hardware even during a suppressed tick.
    ///
    /// WHY A POSTFIX ON `Player.checkInput()`, NOT `Player.Update(bool)`: disassembling
    /// `Player.Update` found it calls `checkInput()` around a third of the way through its own
    /// body, well before the later code (including `MovementUpdate`, further down) that actually
    /// reads `input[0]` to decide this tick's movement/actions. Postfixing `checkInput` directly,
    /// right where vanilla itself finishes assembling `input[0]` for the tick, is both more
    /// surgical (only this one method, not the entire Update body) and unambiguous about timing:
    /// whatever this Postfix leaves in `input[0]` is exactly what every later read this same tick
    /// will see.
    ///
    /// STRUCT-IN-ARRAY REFLECTION: `Player.input` is `InputPackage[]`, and `InputPackage` is a
    /// value type (`struct`, confirmed via its metadata: it extends `System.ValueType`). Reading
    /// `array.GetValue(0)` through reflection boxes a COPY of that struct; mutating fields on that
    /// boxed copy via `FieldInfo.SetValue` does NOT touch the original array element by itself --
    /// the modified boxed copy has to be written back with `array.SetValue(package, 0)` afterwards
    /// for the change to actually reach the live array Player.Update goes on to read. Both steps
    /// are required; skipping the `SetValue` write-back would silently do nothing.
    ///
    /// SHARED WITH ThreatPanicGimmick.cs: `ThreatLevelOneThreshold` and `GetCurrentThreat` below
    /// are `internal`, not `private`, specifically so ThreatPanicGimmick.cs -- a different facet of
    /// the exact same "how close is threat to level 1" question -- reuses these instead of
    /// re-deriving its own copy of the same threshold and lookup. This actually is the same fact,
    /// needed by two gimmicks, so sharing it avoids a real risk of the two drifting out of sync if
    /// the threshold is ever retuned.
    ///
    /// GOTCHA #1 (see AI_CONTEXT.md): `Player.input` (a property), every `Player.InputPackage`
    /// field touched below, `ProcessManager.musicPlayer`, `Music.MusicPlayer.threatTracker`,
    /// `Music.PlayerThreatTracker.currentMusicAgnosticThreat`,
    /// `ProcessManager.fallbackThreatDetermination`, and
    /// `ThreatDetermination.currentMusicAgnosticThreat` are all genuinely new surface for this
    /// codebase, so every one of them is resolved via `AccessTools`/`FieldInfo` rather than direct
    /// member access. `Player.InputPackage` itself, used directly as a compile-time type (for
    /// `typeof(...)` and the `AccessTools.Field` calls below), is trusted without reflection on the
    /// same precedent already established for other nested types in this codebase (`Player
    /// .NPCStats` in SlugpupGimmick.cs, `Menu.SlugcatSelectMenu.SlugcatPage` in
    /// PrimalSlotTabSwitchHook.cs) -- only member ACCESS needs guarding, not referencing the type
    /// itself.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.checkInput))]
    internal static class ThreatSlowdownGimmick
    {
        public const string ID = "ThreatSlowdown";

        /// <summary>Raw currentMusicAgnosticThreat value at which vanilla's ThreatPulser HUD shows
        /// exactly one full circle lit -- "threat level 1" in the user's own words. Derived from
        /// ThreatPulser's own hardcoded `InverseLerp(0f, 1.1f, threat) * circles.Length` (5) --
        /// see this file's header comment for the full trace. `internal`: shared with
        /// ThreatPanicGimmick.cs -- see this file's header comment, SHARED WITH
        /// ThreatPanicGimmick.cs.</summary>
        internal const float ThreatLevelOneThreshold = 1.1f / 5f;

        private static readonly PropertyInfo InputProperty =
            AccessTools.Property(typeof(Player), "input");
        private static readonly FieldInfo XField =
            AccessTools.Field(typeof(Player.InputPackage), "x");
        private static readonly FieldInfo YField =
            AccessTools.Field(typeof(Player.InputPackage), "y");
        private static readonly FieldInfo JmpField =
            AccessTools.Field(typeof(Player.InputPackage), "jmp");
        private static readonly FieldInfo ThrwField =
            AccessTools.Field(typeof(Player.InputPackage), "thrw");
        private static readonly FieldInfo PckpField =
            AccessTools.Field(typeof(Player.InputPackage), "pckp");
        private static readonly FieldInfo MpField =
            AccessTools.Field(typeof(Player.InputPackage), "mp");
        private static readonly FieldInfo SpecField =
            AccessTools.Field(typeof(Player.InputPackage), "spec");
        private static readonly FieldInfo CrouchToggleField =
            AccessTools.Field(typeof(Player.InputPackage), "crouchToggle");
        private static readonly FieldInfo AnalogueDirField =
            AccessTools.Field(typeof(Player.InputPackage), "analogueDir");
        private static readonly FieldInfo DownDiagonalField =
            AccessTools.Field(typeof(Player.InputPackage), "downDiagonal");

        private static readonly FieldInfo MusicPlayerField =
            AccessTools.Field(typeof(ProcessManager), "musicPlayer");
        private static readonly FieldInfo ThreatTrackerField =
            AccessTools.Field(typeof(Music.MusicPlayer), "threatTracker");
        private static readonly FieldInfo CurrentMusicAgnosticThreatField =
            AccessTools.Field(typeof(Music.PlayerThreatTracker), "currentMusicAgnosticThreat");
        private static readonly FieldInfo FallbackThreatDeterminationField =
            AccessTools.Field(typeof(ProcessManager), "fallbackThreatDetermination");
        private static readonly FieldInfo ThreatDeterminationCurrentField =
            AccessTools.Field(typeof(ThreatDetermination), "currentMusicAgnosticThreat");

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

            if (InputProperty == null || XField == null || YField == null || JmpField == null ||
                ThrwField == null || PckpField == null || MpField == null || SpecField == null ||
                CrouchToggleField == null || AnalogueDirField == null || DownDiagonalField == null)
            {
                Plugin.Log?.LogWarning("ThreatSlowdownGimmick: couldn't resolve one or more Player.input/InputPackage members via reflection -- leaving input untouched. See GOTCHA #1.");
                return;
            }

            float threat = GetCurrentThreat(rainWorld);
            float panicFraction = Mathf.Clamp01(threat / ThreatLevelOneThreshold);
            if (panicFraction <= 0f)
            {
                return;
            }

            if (UnityEngine.Random.value >= panicFraction)
            {
                // This tick's roll succeeded -- input passes through untouched.
                return;
            }

            try
            {
                Array inputArray = InputProperty.GetValue(self) as Array;
                if (inputArray == null || inputArray.Length == 0)
                {
                    return;
                }

                // Boxed copy of the struct at index 0 -- mutating it does nothing to the live
                // array by itself; see this file's header comment, STRUCT-IN-ARRAY REFLECTION.
                object package = inputArray.GetValue(0);
                XField.SetValue(package, 0);
                YField.SetValue(package, 0);
                JmpField.SetValue(package, false);
                ThrwField.SetValue(package, false);
                PckpField.SetValue(package, false);
                MpField.SetValue(package, false);
                SpecField.SetValue(package, false);
                CrouchToggleField.SetValue(package, false);
                AnalogueDirField.SetValue(package, Vector2.zero);
                DownDiagonalField.SetValue(package, 0);
                inputArray.SetValue(package, 0);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"ThreatSlowdownGimmick: suppressing this tick's input threw: {ex.Message}");
            }
        }

        /// <summary>Mirrors ThreatPulser.get_Threat()'s own lookup+fallback (see this file's
        /// header comment) -- except it never lazily constructs a missing fallback the way
        /// vanilla's own getter does, since side-effecting ProcessManager state isn't this
        /// gimmick's job; that specific edge case just reads as 0 (no debuff) instead. `internal`:
        /// shared with ThreatPanicGimmick.cs -- see this file's header comment, SHARED WITH
        /// ThreatPanicGimmick.cs.</summary>
        internal static float GetCurrentThreat(RainWorld rainWorld)
        {
            ProcessManager processManager = rainWorld.processManager;
            if (processManager == null)
            {
                return 0f;
            }

            try
            {
                if (MusicPlayerField != null)
                {
                    object musicPlayer = MusicPlayerField.GetValue(processManager);
                    if (musicPlayer != null && ThreatTrackerField != null && CurrentMusicAgnosticThreatField != null)
                    {
                        object threatTracker = ThreatTrackerField.GetValue(musicPlayer);
                        if (threatTracker != null)
                        {
                            return (float)CurrentMusicAgnosticThreatField.GetValue(threatTracker);
                        }
                    }
                }

                if (FallbackThreatDeterminationField != null && ThreatDeterminationCurrentField != null)
                {
                    object fallback = FallbackThreatDeterminationField.GetValue(processManager);
                    if (fallback != null)
                    {
                        return (float)ThreatDeterminationCurrentField.GetValue(fallback);
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"ThreatSlowdownGimmick.GetCurrentThreat threw -- treating threat as 0 for this tick: {ex.Message}");
            }

            return 0f;
        }
    }
}
