using System.Reflection;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared reflected access to the handful of vanilla menu members more than one of this mod's
    /// menu-side patches needs.
    ///
    /// Right now that's exactly one field: Menu.KarmaLadderScreen.saveState, which the cycle-end
    /// screen (Menu.SleepAndDeathScreen, a KarmaLadderScreen subclass) carries and which is the
    /// only way to know which character's run the screen belongs to -- there's no Player to read
    /// on that screen. NoPassagesGimmick, KillListButtonHook, and KillListSignalHook each used to
    /// cache their own identical AccessTools.Field handle for it.
    ///
    /// Reflected rather than accessed directly for the usual reason -- see Gotcha #1 in
    /// AI_CONTEXT.md: this project's compile-time reference assembly is a "publicized" copy with
    /// every member forced public, so a field showing as public there (or in the decompiled
    /// reference/GameSource/ tree, decompiled from that same publicized copy) proves nothing about
    /// the real runtime assembly, and a direct access that compiles fine can still throw
    /// FieldAccessException in the actual game.
    /// </summary>
    internal static class PrimalMenuAccess
    {
        private static readonly FieldInfo SaveStateField =
            AccessTools.Field(typeof(Menu.KarmaLadderScreen), "saveState");

        /// <summary>The SaveState behind a cycle-end/karma-ladder screen, or null if it isn't
        /// available (no run in progress, or the field couldn't be resolved).</summary>
        public static SaveState GetSaveState(Menu.KarmaLadderScreen screen)
        {
            if (screen == null)
            {
                return null;
            }
            return SaveStateField?.GetValue(screen) as SaveState;
        }

        /// <summary>The real vanilla character ID whose run this screen belongs to, or null.</summary>
        public static string GetCharacterID(Menu.KarmaLadderScreen screen)
        {
            return GetSaveState(screen)?.saveStateNumber?.value;
        }

        /// <summary>Added 2026-09-17 for OutlawTracker -- the vanilla end-of-campaign score screen
        /// (Menu.StoryGameStatisticsScreen, a KarmaLadderScreen subclass, so it also carries
        /// everything above). `allDone` and `scoreKeeper`/ScoreKeeper.score are brand new territory
        /// for this mod, never accessed before this pass -- reflected the same as `saveState` above
        /// on the same Gotcha #1 reasoning (a field reading public in the decompiled/publicized
        /// reference proves nothing about the real assembly), not because either has actually been
        /// caught failing directly.</summary>
        private static readonly FieldInfo AllDoneField =
            AccessTools.Field(typeof(Menu.StoryGameStatisticsScreen), "allDone");

        private static readonly FieldInfo ScoreKeeperField =
            AccessTools.Field(typeof(Menu.StoryGameStatisticsScreen), "scoreKeeper");

        private static readonly FieldInfo ScoreField =
            AccessTools.Field(typeof(Menu.StoryGameStatisticsScreen.ScoreKeeper), "score");

        /// <summary>True once the score screen's own tally/multiplier animation has fully finished
        /// -- the point at which ScoreKeeper.score holds its real final value.</summary>
        public static bool IsScoreScreenDone(Menu.StoryGameStatisticsScreen screen)
        {
            return screen != null && AllDoneField != null && (bool)AllDoneField.GetValue(screen);
        }

        /// <summary>The final tallied score shown on this screen, or null if it (or the
        /// ScoreKeeper itself) couldn't be resolved.</summary>
        public static int? GetFinalScore(Menu.StoryGameStatisticsScreen screen)
        {
            object scoreKeeper = screen != null ? ScoreKeeperField?.GetValue(screen) : null;
            if (scoreKeeper == null || ScoreField == null)
            {
                return null;
            }
            return (int)ScoreField.GetValue(scoreKeeper);
        }
    }
}
