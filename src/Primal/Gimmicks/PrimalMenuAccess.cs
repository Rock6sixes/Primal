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
    }
}
