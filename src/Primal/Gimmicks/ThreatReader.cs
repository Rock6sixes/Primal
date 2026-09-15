using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Reads the game's current "threat" level -- the same number vanilla's own ThreatPulser HUD
    /// widget lights its circles from -- for the two gimmicks built on it (ThreatSlowdown and
    /// ThreatPanic).
    ///
    /// Extracted from ThreatSlowdownGimmick 2026-09-15. It previously lived there as an `internal`
    /// member that ThreatPanicGimmick reached across to call, which worked but put a fact both
    /// gimmicks equally depend on inside one of them, and -- more importantly -- meant the lookup
    /// ran twice per tick whenever both gimmicks were active. That isn't hypothetical: Monk has
    /// both assigned by default (see GimmickDefaults), and both gimmicks Postfix the same method
    /// (Player.checkInput), so every tick did the whole reflection chain twice for the same global
    /// number.
    ///
    /// **Per-frame memo.** Threat is global state read off ProcessManager -- not per-player -- and
    /// cannot change within a single frame, so the value is cached per Time.frameCount. Both
    /// gimmicks, and both players in co-op, now share one lookup per frame instead of one each.
    ///
    /// GOTCHA #1 (see AI_CONTEXT.md): every member below (`ProcessManager.musicPlayer`,
    /// `Music.MusicPlayer.threatTracker`, `Music.PlayerThreatTracker.currentMusicAgnosticThreat`,
    /// `ProcessManager.fallbackThreatDetermination`, `ThreatDetermination
    /// .currentMusicAgnosticThreat`) is resolved via AccessTools rather than accessed directly --
    /// the compile-time reference assembly is a publicized copy, so its `public` flags prove
    /// nothing about the real runtime assembly.
    /// </summary>
    internal static class ThreatReader
    {
        /// <summary>Raw currentMusicAgnosticThreat value at which vanilla's ThreatPulser HUD shows
        /// exactly one full circle lit -- "threat level 1" in the user's own words. Derived from
        /// ThreatPulser's own hardcoded `InverseLerp(0f, 1.1f, threat) * circles.Length` (5) --
        /// see ThreatSlowdownGimmick.cs's header comment for the full trace. Shared by both threat
        /// gimmicks so retuning it can't leave the two out of sync.</summary>
        public const float ThreatLevelOneThreshold = 1.1f / 5f;

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

        // -1 rather than 0: Time.frameCount starts at 0, so a real frame 0 must not look like
        // "already cached".
        private static int _cachedFrame = -1;
        private static float _cachedThreat;

        /// <summary>Current threat, memoized for the rest of this frame. Mirrors
        /// ThreatPulser.get_Threat()'s own lookup+fallback -- except it never lazily constructs a
        /// missing fallback the way vanilla's own getter does, since side-effecting ProcessManager
        /// state isn't this mod's job; that specific edge case just reads as 0 (no threat)
        /// instead.</summary>
        public static float CurrentThreat(RainWorld rainWorld)
        {
            int frame = Time.frameCount;
            if (frame == _cachedFrame)
            {
                return _cachedThreat;
            }

            _cachedThreat = Read(rainWorld);
            _cachedFrame = frame;
            return _cachedThreat;
        }

        /// <summary>How close threat currently is to "level 1", clamped to 0..1 -- the fraction
        /// both gimmicks actually scale their effect by.</summary>
        public static float ThreatFraction(RainWorld rainWorld)
        {
            return Mathf.Clamp01(CurrentThreat(rainWorld) / ThreatLevelOneThreshold);
        }

        private static float Read(RainWorld rainWorld)
        {
            ProcessManager processManager = rainWorld?.processManager;
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
                Plugin.Log?.LogWarning($"ThreatReader.CurrentThreat threw -- treating threat as 0 for this frame: {ex.Message}");
            }

            return 0f;
        }
    }
}
