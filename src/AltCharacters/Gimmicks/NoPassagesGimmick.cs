using System.Reflection;
using HarmonyLib;

namespace AltCharacters.Gimmicks
{
    /// <summary>
    /// The "No Passages" gimmick: Passages are still shown as normal on the cycle-complete/death
    /// screen, but the button that lets you actually use one is disabled -- the player can still
    /// see which passages exist (and read their fulfilment condition via the button's own
    /// tooltip/description, untouched by this gimmick), they just can't click through to spend
    /// food pips on one.
    ///
    /// Traced by disassembling Menu.SleepAndDeathScreen.AddPassageButton(bool) in the real game
    /// assembly: it builds a single Menu.SimpleButton (guarded by "if (passageButton != null)
    /// return;", so it only ever runs the actual construction once per screen) and stores it in
    /// the screen's own `passageButton` field. Every Menu.ButtonTemplate carries a
    /// Menu.ButtonBehavior (`buttonBehav`), and that class's own Update() method (also
    /// disassembled) skips its entire click-detection block -- the part that ultimately calls
    /// `owner.Clicked()` -- whenever `buttonBehav.greyedOut` is true, while every other part of
    /// Update (the pulsing/highlight animation) still runs normally. That's the exact "visible but
    /// inert" mechanism vanilla itself already uses for menu options that exist but currently
    /// can't be chosen, so setting it is a clean, minimal way to do exactly what was asked instead
    /// of hiding the button or inventing a new disabled-visual style. Postfixing AddPassageButton
    /// (rather than the button's own click handler) means this only ever needs to run once per
    /// screen, right when the button is created.
    ///
    /// `saveState` (KarmaLadderScreen), `passageButton` (SleepAndDeathScreen), `buttonBehav`
    /// (ButtonTemplate), and `greyedOut` (ButtonBehavior) all show as `public` fields in this
    /// project's compile-time reference assembly, but per AI_CONTEXT.md's Gotcha #1 that reference
    /// is a "publicized" copy of the real game assembly (every member forced public at compile
    /// time) -- these are menu-side fields this mod has never touched before, so they get the same
    /// defensive AccessTools.Field reflection treatment PrimalCharacterNames.cs already uses for
    /// SlugcatPageNewGame's labels, rather than trusting the reference assembly's flags outright.
    /// `manager`/`rainWorld`, by contrast, are accessed directly -- PrimalCharacterNameHook already
    /// does the same (`menu.manager.rainWorld`) without issue, so there's already in-mod precedent
    /// that those two are genuinely public.
    ///
    /// Deliberately does NOT touch AddExpeditionPassageButton -- the user asked about the cycle-
    /// complete/death screen specifically, and Expedition mode is a separate game mode whose
    /// passage handling is already independent (AddPassageButton itself no-ops when
    /// rainWorld.ExpeditionMode is true, confirmed in its own disassembly).
    ///
    /// No default assignment as of this doc -- not requested for any specific character; assign it
    /// per character from the mod's Remix menu like everything else.
    /// </summary>
    [HarmonyPatch(typeof(Menu.SleepAndDeathScreen), nameof(Menu.SleepAndDeathScreen.AddPassageButton))]
    internal static class NoPassagesGimmick
    {
        public const string ID = "NoPassages";

        private static readonly FieldInfo SaveStateField =
            AccessTools.Field(typeof(Menu.KarmaLadderScreen), "saveState");

        private static readonly FieldInfo PassageButtonField =
            AccessTools.Field(typeof(Menu.SleepAndDeathScreen), "passageButton");

        private static readonly FieldInfo ButtonBehavField =
            AccessTools.Field(typeof(Menu.ButtonTemplate), "buttonBehav");

        private static readonly FieldInfo GreyedOutField =
            AccessTools.Field(typeof(Menu.ButtonBehavior), "greyedOut");

        private static void Postfix(Menu.SleepAndDeathScreen __instance)
        {
            RainWorld rainWorld = __instance.manager?.rainWorld;
            if (rainWorld == null || !AltSlot.IsActive(rainWorld))
            {
                return;
            }

            if (!(SaveStateField?.GetValue(__instance) is SaveState saveState))
            {
                return;
            }

            string characterID = saveState.saveStateNumber?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            if (!(PassageButtonField?.GetValue(__instance) is Menu.SimpleButton passageButton))
            {
                return;
            }

            if (!(ButtonBehavField?.GetValue(passageButton) is Menu.ButtonBehavior buttonBehav))
            {
                return;
            }

            GreyedOutField?.SetValue(buttonBehav, true);
        }
    }
}
