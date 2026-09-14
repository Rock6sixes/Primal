using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Threat Panic" gimmick: as vanilla's own threat number climbs toward "threat level 1"
    /// (see ThreatSlowdownGimmick.cs for the full trace of what that means and where the number
    /// comes from -- this file reuses that exact same threshold and lookup, not a separate one),
    /// the character starts involuntarily firing off random inputs on top of whatever the player is
    /// actually doing -- a literal reading of "panicking": losing fine motor control, not losing
    /// control outright.
    ///
    /// REPLACED 2026-09-13 -- this ID ("ThreatPanic") used to implement a different effect: a
    /// probabilistic per-tick wipe of the player's real input (dropping it to nothing). That
    /// mechanic didn't change at all, it just moved to the "ThreatSlowdown" ID/file -- see this
    /// file's header comment there, and ThreatSlowdownGimmick.cs's own header, for the full context
    /// of why. What "Threat Panic" means was replaced instead, per the user's own follow-up once
    /// both gimmicks had been tested: "change it so that... [it] throw[s] random inputs. logic
    /// should go like this - choose input between 4 directional grab throw jump, and then keep it
    /// for 1-10 frames. Frequency sets how often these happen. Allow player to always input. Inputs
    /// can overlap."
    ///
    /// THE MECHANISM, literally following that spec: every tick, there's a chance (scaled by both
    /// threat and the per-character Frequency setting below -- see PanicSettings) of starting a new
    /// "forced input" -- one of 7 equally-likely choices (`PanicKind`): Up, Down, Left, Right, Grab,
    /// Throw, Jump ("4 directional" + the three named actions). Once chosen, it stays active for a
    /// randomly-rolled duration of 1-10 ticks (`UnityEngine.Random.Range(1, 11)`), then expires on
    /// its own. Multiple forced inputs can be active at once ("inputs can overlap") -- a new one
    /// starting doesn't cancel or replace whatever's already in flight; each one is tracked
    /// separately (`PanicInstance`, one per currently-active forced input) and all of them are
    /// applied together every tick until they individually expire.
    ///
    /// CRITICALLY, UNLIKE THE OLD "DROP INPUT" MECHANIC THIS ID USED TO HAVE: this Postfix never
    /// clears or overwrites the player's own real presses -- "allow player to always input" is
    /// taken literally. `jmp`/`thrw`/`pckp` are OR'd (a forced action can only ever ADD a press
    /// that wasn't there, never remove one the player is actually making). `x`/`y` (and
    /// `analogueDir`, kept in sync for controller users -- see WHY analogueDir IS ALSO SET below)
    /// are the one exception: forcing a direction necessarily means overwriting whatever the
    /// player's own input was on THAT axis for the duration -- there's no sensible way to "add" a
    /// second value on top of an already-single-valued int -- but only the specific axis (x or y)
    /// a currently-active directional instance targets is touched; the other axis and every other
    /// field are left exactly as real input produced them. If two active instances disagree on the
    /// same axis at once (e.g. a still-active Left instance and a newly-triggered Right instance),
    /// the more-recently-triggered one wins for that tick -- a simple, deterministic tie-break
    /// (instances are applied in list order, later entries overwriting earlier ones on the same
    /// axis), not an attempt to model "real" conflicting keypresses.
    ///
    /// WHY analogueDir IS ALSO SET: `InputPackage.analogueDir` (a Vector2) is what an analog-stick
    /// controller actually reports for directional movement, separate from the digital `x`/`y`
    /// ints keyboard input produces. The old "drop input" mechanic (now in ThreatSlowdownGimmick
    /// .cs) zeroed both, on the reasoning that a full lockout has to defeat every input method
    /// equally. The same logic applies here in reverse: forcing only `x`/`y` would be invisible to
    /// a controller player if the game reads `analogueDir` preferentially for movement -- so
    /// whichever axis a directional instance forces has its matching `analogueDir` component
    /// overwritten to +/-1 as well (leaving the other axis of `analogueDir` untouched, same
    /// per-axis-only rule as `x`/`y`). This is a judgment call by symmetry with the older mechanic,
    /// not something the user specifically asked for -- worth checking in-game whether it's
    /// actually necessary (keyboard-only testing wouldn't reveal a problem either way).
    ///
    /// FREQUENCY, CONCRETELY: `PanicSettings.Frequency` (per character, Remix-configurable, default
    /// 3) is "roughly how many forced inputs start per second, AT threat level 1" -- the game
    /// simulates at a fixed 40 ticks/second (see TimeLimitGimmick.cs's own `TicksPerSecond`
    /// constant and doc comment for where that number comes from), so the per-tick chance of
    /// starting a new forced input is `(Frequency / 40f) * panicFraction`, where `panicFraction`
    /// (0 at no threat, 1 at threat level 1) is the exact same ramp ThreatSlowdownGimmick.cs uses --
    /// i.e. Frequency is the rate AT FULL THREAT, and actually observed frequency scales down
    /// linearly as threat eases below level 1, same as every other Threat-based gimmick in this
    /// file. Deliberately phrased as "per second," not "per tick" or "ticks between triggers", so a
    /// bigger number in the Remix UI intuitively means MORE frequent panic, not less.
    ///
    /// KNOWN INTERACTION WITH ThreatSlowdownGimmick, NOT SPECIFICALLY RESOLVED: both gimmicks
    /// Postfix the exact same method (`Player.checkInput`), and Harmony does not guarantee this
    /// file's Postfix runs before or after ThreatSlowdownGimmick's when a character has both
    /// enabled (true for Monk, currently). If both happen to trigger on the same tick, whichever
    /// Postfix Harmony happens to run second wins that tick (a forced input either gets asserted
    /// and then wiped by the drop, or asserted after the wipe and survives) -- purely a same-tick
    /// coin flip between two already-probabilistic effects, not a crash or a broken interaction, but
    /// worth knowing about if testing with both gimmicks on at once produces inconsistent-seeming
    /// results on any single tick.
    ///
    /// GOTCHA #1 (see AI_CONTEXT.md): `Player.input` and every `Player.InputPackage` field touched
    /// below are genuinely new surface, so all are resolved via `AccessTools`/`FieldInfo` rather
    /// than direct member access. `Player.InputPackage` itself, used directly as a compile-time
    /// type, is trusted without reflection on the same nested-type precedent already established
    /// elsewhere in this codebase (see ThreatSlowdownGimmick.cs's own GOTCHA #1 note). The
    /// `Configurable&lt;int&gt;`/`OpUpdown` settings machinery below is Remix's own modder-facing UI
    /// API (same as every other gimmick's settings sub-view, e.g. TimeLimitSettings in
    /// TimeLimitGimmick.cs), not gated by GOTCHA #1 at all.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.checkInput))]
    internal static class ThreatPanicGimmick
    {
        public const string ID = "ThreatPanic";

        private enum PanicKind
        {
            Up,
            Down,
            Left,
            Right,
            Grab,
            Throw,
            Jump,
        }

        private sealed class PanicInstance
        {
            public PanicKind Kind;
            public int FramesRemaining;
        }

        // One list of currently-active forced inputs per Player -- see this file's header comment,
        // THE MECHANISM, for why multiple can be active at once ("inputs can overlap"). Keyed on
        // Player itself (a reference type, unlike the value-type SlugcatStats key
        // ThreatSlowdownGimmick used to use for its now-removed baseline cache) so each player's
        // in-flight forced inputs are independent and get garbage-collected along with the player.
        private static readonly ConditionalWeakTable<Player, List<PanicInstance>> ActiveInstances =
            new ConditionalWeakTable<Player, List<PanicInstance>>();

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
        private static readonly FieldInfo AnalogueDirField =
            AccessTools.Field(typeof(Player.InputPackage), "analogueDir");

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
                ThrwField == null || PckpField == null || AnalogueDirField == null)
            {
                Plugin.Log?.LogWarning("ThreatPanicGimmick: couldn't resolve one or more Player.input/InputPackage members via reflection -- leaving input untouched. See GOTCHA #1.");
                return;
            }

            // Shared with ThreatSlowdownGimmick -- see this file's header comment for why the
            // threshold and lookup are deliberately the same one, not a separate copy.
            float threat = ThreatSlowdownGimmick.GetCurrentThreat(rainWorld);
            float panicFraction = Mathf.Clamp01(threat / ThreatSlowdownGimmick.ThreatLevelOneThreshold);

            if (!ActiveInstances.TryGetValue(self, out List<PanicInstance> instances))
            {
                instances = new List<PanicInstance>();
                ActiveInstances.Add(self, instances);
            }

            // Roll for a NEW forced input starting this tick -- see this file's header comment,
            // FREQUENCY, CONCRETELY, for the (Frequency / 40) * panicFraction formula. Skipped
            // entirely at zero threat (no new ones start), but any already-active instance below
            // still plays out its remaining duration rather than being cut off abruptly.
            if (panicFraction > 0f)
            {
                int frequency = PanicSettings.GetFrequency(characterID);
                float perTickChance = (frequency / 40f) * panicFraction;
                if (UnityEngine.Random.value < perTickChance)
                {
                    PanicKind kind = (PanicKind)UnityEngine.Random.Range(0, 7);
                    int duration = UnityEngine.Random.Range(1, 11); // 1-10 ticks, inclusive
                    instances.Add(new PanicInstance { Kind = kind, FramesRemaining = duration });
                }
            }

            if (instances.Count == 0)
            {
                return;
            }

            // Accumulate this tick's forced effect across every active instance -- later entries
            // in the list (more recently triggered) win on a same-axis direction conflict; see
            // this file's header comment for why that tie-break was chosen.
            bool forceJump = false;
            bool forceThrow = false;
            bool forceGrab = false;
            int? forcedX = null;
            int? forcedY = null;

            for (int i = instances.Count - 1; i >= 0; i--)
            {
                PanicInstance instance = instances[i];
                switch (instance.Kind)
                {
                    case PanicKind.Up: forcedY = 1; break;
                    case PanicKind.Down: forcedY = -1; break;
                    case PanicKind.Left: forcedX = -1; break;
                    case PanicKind.Right: forcedX = 1; break;
                    case PanicKind.Grab: forceGrab = true; break;
                    case PanicKind.Throw: forceThrow = true; break;
                    case PanicKind.Jump: forceJump = true; break;
                }

                instance.FramesRemaining--;
                if (instance.FramesRemaining <= 0)
                {
                    instances.RemoveAt(i);
                }
            }

            if (!forceJump && !forceThrow && !forceGrab && forcedX == null && forcedY == null)
            {
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
                // array by itself; see ThreatSlowdownGimmick.cs's header comment,
                // STRUCT-IN-ARRAY REFLECTION, for the full explanation (same mechanics apply here).
                object package = inputArray.GetValue(0);

                if (forceJump)
                {
                    JmpField.SetValue(package, true);
                }
                if (forceThrow)
                {
                    ThrwField.SetValue(package, true);
                }
                if (forceGrab)
                {
                    PckpField.SetValue(package, true);
                }
                if (forcedX.HasValue)
                {
                    XField.SetValue(package, forcedX.Value);
                    Vector2 dir = (Vector2)AnalogueDirField.GetValue(package);
                    dir.x = forcedX.Value;
                    AnalogueDirField.SetValue(package, dir);
                }
                if (forcedY.HasValue)
                {
                    YField.SetValue(package, forcedY.Value);
                    Vector2 dir = (Vector2)AnalogueDirField.GetValue(package);
                    dir.y = forcedY.Value;
                    AnalogueDirField.SetValue(package, dir);
                }

                inputArray.SetValue(package, 0);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"ThreatPanicGimmick: forcing this tick's input threw: {ex.Message}");
            }
        }
    }

    /// <summary>Per-character settings for ThreatPanicGimmick: just Frequency (how many forced
    /// inputs per second, at threat level 1 -- see ThreatPanicGimmick's own header comment,
    /// FREQUENCY, CONCRETELY). Follows the exact same Bind/Get/BuildUI shape as every other
    /// gimmick's settings class (see TimeLimitSettings in TimeLimitGimmick.cs).</summary>
    internal static class PanicSettings
    {
        private const int DefaultFrequency = 3;
        private const int MinFrequency = 1;
        private const int MaxFrequency = 20;

        private static readonly Dictionary<string, Configurable<int>> _frequency = new Dictionary<string, Configurable<int>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment / Gotcha #4 for why rebuilding fresh elements every time this
        // sub-view opened would crash the menu the second time around.
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            _frequency[characterID] = owner.config.Bind(
                characterID + "_ThreatPanic_frequency",
                DefaultFrequency,
                new ConfigurableInfo("Roughly how many forced random inputs happen per second, at threat level 1 (scales down as threat eases off).", new ConfigAcceptableRange<int>(MinFrequency, MaxFrequency)));
        }

        public static int GetFrequency(string characterID)
        {
            return _frequency.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultFrequency;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            var items = new List<UIelement>();
            if (!_frequency.TryGetValue(characterID, out Configurable<int> frequency))
            {
                // Not bound yet -- don't cache an incomplete/empty result, in case Bind() hasn't
                // run for this character yet and a later call could succeed.
                return items;
            }

            items.Add(new OpLabel(20f, topY, "Forced inputs per second, at threat level 1:", false));
            items.Add(new OpUpdown(frequency, new Vector2(300f, topY - 6f), 80f));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
