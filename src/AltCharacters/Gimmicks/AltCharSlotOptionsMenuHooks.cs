using HarmonyLib;
using UnityEngine;

namespace AltCharacters.Gimmicks
{
    /// <summary>
    /// Adds a 4th "ALT CHAR SLOT" button to Rain World's own built-in Options menu, right below
    /// the vanilla "SAVE SLOT 1/2/3" buttons -- so switching into (and back out of) this mod's
    /// reserved save slot (see AltSlot.ReservedSlot), where every alt character's own separate
    /// saves live, is just another save-slot pick,
    /// using the game's own already-shipped, already-tested slot-switch machinery end to end:
    /// Menu.OptionsMenu.SetCurrentlySelectedOfSeries("SaveSlot", ...) does the actual
    /// Destroy()-old/mount-new swap, OptionsMenu.ProgressionBusy greys out the whole menu while
    /// the async mount is in flight (the "takes a second or two, not a freeze" UX this was asked
    /// for), and OptionsMenu.HandleSaveSlotChangeFailed automatically reverts to the previous
    /// slot if the mount fails -- all of that is vanilla code this mod never has to touch.
    ///
    /// This intentionally does NOT reimplement any of that switching logic (a previous, since
    /// reverted, approach tried driving RainWorld.ReloadProgression() from a custom state machine
    /// and corrupted a save file the first time it was tested -- see AltCharacterIdentity.cs's
    /// class doc comment for the full incident writeup). Instead:
    ///
    /// - Menu.SelectOneButton's selection logic (AmISelected/Clicked) never touches the 3 vanilla
    ///   buttons' own buttonArray -- each button just asks its owner
    ///   (GetCurrentlySelectedOfSeries) which index is selected, every frame -- so a 4th button
    ///   with buttonArrayIndex 3 slots in without altering the original 3 at all.
    /// - GetCurrentlySelectedOfSeries("SaveSlot") normally returns options.saveSlot directly, so
    ///   it would return 666666 rather than the virtual index 3 our button expects -- fixed by a
    ///   small Prefix that special-cases exactly that value.
    /// - SetCurrentlySelectedOfSeries("SaveSlot", to) is left to run *unmodified*; a Prefix just
    ///   rewrites the incoming "to" from our virtual index 3 to the real ReservedSlot number
    ///   before vanilla's own switch-case ever sees it, so the exact same code path vanilla uses
    ///   for switching between slots 0/1/2 is what performs our swap too.
    /// </summary>
    [HarmonyPatch(typeof(Menu.OptionsMenu), MethodType.Constructor, typeof(ProcessManager))]
    internal static class AltCharSlotButtonConstructionHook
    {
        /// <summary>The virtual "SaveSlot" series index our button occupies, one past vanilla's
        /// last real save slot (indices 0/1/2).</summary>
        public const int VirtualIndex = 3;

        private static void Postfix(Menu.OptionsMenu __instance)
        {
            Menu.OptionsMenu self = __instance;

            if (self.saveSlotButtons == null || self.saveSlotButtons.Length == 0)
            {
                // Defensive -- shouldn't happen, but if vanilla's own save-slot section didn't
                // get built for some reason, there's nothing sensible to attach our button to.
                return;
            }

            float buttonWidth = self.saveSlotButtons[0].size.x;
            Vector2 buttonSize = new Vector2(buttonWidth, 30f);

            // Same column vanilla's own buttons sit in: one slot-height (40px) further down than
            // the last "SAVE SLOT" button, i.e. exactly where BACKUPS currently sits.
            Vector2 newButtonPos = self.saveSlotButtons[self.saveSlotButtons.Length - 1].pos + new Vector2(0f, -40f);

            Menu.SelectOneButton altCharButton = new Menu.SelectOneButton(
                self,
                self.pages[0],
                "ALT CHAR SLOT",
                "SaveSlot",
                newButtonPos,
                buttonSize,
                self.saveSlotButtons,
                VirtualIndex);
            self.pages[0].subObjects.Add(altCharButton);

            // Make room by nudging BACKUPS (and, on console builds where it doesn't exist,
            // nothing) down by one more slot-height so the two don't overlap.
            if (self.saveBackupsButton != null)
            {
                self.saveBackupsButton.pos += new Vector2(0f, -40f);

                // Keyboard/controller nav: splice our new button in between the last save-slot
                // button and BACKUPS instead of leaving BACKUPS's old up/down links pointing
                // past us.
                Menu.SelectOneButton lastSlotButton = self.saveSlotButtons[self.saveSlotButtons.Length - 1];
                altCharButton.nextSelectable[1] = lastSlotButton; // up
                altCharButton.nextSelectable[3] = self.saveBackupsButton; // down
                lastSlotButton.nextSelectable[3] = altCharButton;
                self.saveBackupsButton.nextSelectable[1] = altCharButton;
            }
        }
    }

    /// <summary>Translates options.saveSlot == ReservedSlot back to our button's virtual index
    /// (3) so Menu.SelectOneButton.AmISelected reports our button -- not any of the vanilla
    /// ones -- as selected while the reserved slot is mounted. Every other slot value passes
    /// through to vanilla's own logic completely unchanged.</summary>
    [HarmonyPatch(typeof(Menu.OptionsMenu), nameof(Menu.OptionsMenu.GetCurrentlySelectedOfSeries))]
    internal static class AltCharSlotGetSelectedHook
    {
        private static bool Prefix(Menu.OptionsMenu __instance, string series, ref int __result)
        {
            if (series == "SaveSlot" && __instance.manager.rainWorld.options.saveSlot == AltSlot.ReservedSlot)
            {
                __result = AltCharSlotButtonConstructionHook.VirtualIndex;
                return false;
            }
            return true;
        }
    }

    /// <summary>Cosmetic only: vanilla's UpdateInfoText hard-codes "Select save slot N" (N =
    /// buttonArrayIndex + 1) for any hovered/selected "SaveSlot" button, which would otherwise
    /// show "Select save slot 4" for our button instead of something that actually explains what
    /// it does.</summary>
    [HarmonyPatch(typeof(Menu.OptionsMenu), nameof(Menu.OptionsMenu.UpdateInfoText))]
    internal static class AltCharSlotInfoTextHook
    {
        private static bool Prefix(Menu.OptionsMenu __instance, ref string __result)
        {
            if (__instance.selectedObject is Menu.SelectOneButton button
                && button.signalText == "SaveSlot"
                && button.buttonArrayIndex == AltCharSlotButtonConstructionHook.VirtualIndex)
            {
                __result = "Switch to the alt characters' separate save slot";
                return false;
            }
            return true;
        }
    }

    /// <summary>Rewrites our button's virtual index (3) to the real ReservedSlot number before
    /// vanilla's own SetCurrentlySelectedOfSeries switch-case runs, so that unmodified vanilla
    /// code -- the same code that already safely handles switching between slots 0/1/2, complete
    /// with its own busy-state UI and failure/revert handling -- is what actually performs the
    /// swap into (and, when a normal slot button is clicked afterward, back out of) our reserved
    /// slot.</summary>
    [HarmonyPatch(typeof(Menu.OptionsMenu), nameof(Menu.OptionsMenu.SetCurrentlySelectedOfSeries))]
    internal static class AltCharSlotSetSelectedHook
    {
        private static void Prefix(string series, ref int to)
        {
            if (series == "SaveSlot" && to == AltCharSlotButtonConstructionHook.VirtualIndex)
            {
                to = AltSlot.ReservedSlot;
            }
        }
    }
}
