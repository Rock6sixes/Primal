using HarmonyLib;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Adds a 4th "PRIMAL SLOT" button to Rain World's own built-in Options menu, right below
    /// the vanilla "SAVE SLOT 1/2/3" buttons -- so switching into (and back out of) this mod's
    /// reserved save slots (see PrimalSlot.cs), where every Primal-modified character's own
    /// separate saves live, is just another save-slot pick,
    /// using the game's own already-shipped, already-tested slot-switch machinery end to end:
    /// Menu.OptionsMenu.SetCurrentlySelectedOfSeries("SaveSlot", ...) does the actual
    /// Destroy()-old/mount-new swap, OptionsMenu.ProgressionBusy greys out the whole menu while
    /// the async mount is in flight (the "takes a second or two, not a freeze" UX this was asked
    /// for), and OptionsMenu.HandleSaveSlotChangeFailed automatically reverts to the previous
    /// slot if the mount fails -- all of that is vanilla code this mod never has to touch.
    ///
    /// This intentionally does NOT reimplement any of that switching logic (a previous, since
    /// reverted, approach tried driving RainWorld.ReloadProgression() from a custom state machine
    /// and corrupted a save file the first time it was tested -- that approach, and the file that
    /// carried it, were removed entirely once the mod moved to its current "gimmicks assigned to
    /// real vanilla characters" design; see AI_CONTEXT.md's "File map" / removal history for that
    /// incident's full writeup). Instead:
    ///
    /// - Menu.SelectOneButton's selection logic (AmISelected/Clicked) never touches the 3 vanilla
    ///   buttons' own buttonArray -- each button just asks its owner
    ///   (GetCurrentlySelectedOfSeries) which index is selected, every frame -- so a 4th button
    ///   with buttonArrayIndex 3 slots in without altering the original 3 at all.
    /// - GetCurrentlySelectedOfSeries("SaveSlot") normally returns options.saveSlot directly, so
    ///   it would return one of the (now several -- see MULTIPLE PRIMAL SLOTS below)
    ///   Primal-reserved slot numbers rather than the virtual index 3 our button expects -- fixed
    ///   by a small Prefix that special-cases any of them (PrimalSlot.IsActive).
    /// - SetCurrentlySelectedOfSeries("SaveSlot", to) is left to run *unmodified*; a Prefix just
    ///   rewrites the incoming "to" from our virtual index 3 to the real target slot number
    ///   before vanilla's own switch-case ever sees it, so the exact same code path vanilla uses
    ///   for switching between slots 0/1/2 is what performs our swap too.
    ///
    /// MULTIPLE PRIMAL SLOTS (2026-09-13 addition -- see PrimalSlot.cs's header comment and
    /// Gimmicks/PrimalSlotTabSwitchHook.cs's, both updated the same day for the same request):
    /// there is no longer one single reserved slot every real slot's "PRIMAL SLOT" click lands
    /// on -- each real slot 0/1/2 has its own (PrimalSlot.ReservedSlot + realSlot). This button
    /// stays a single toggle rather than becoming three separate buttons: clicking it now maps to
    /// *whichever real slot is currently mounted's own* Primal counterpart
    /// (PrimalSlotSetSelectedHook.Prefix below reads the currently-selected real slot off
    /// __instance before rewriting `to`), so from the player's perspective it still reads as one
    /// "PRIMAL SLOT" option that always means "the Primal version of whatever I'm on now" --
    /// consistent with how Tab already behaves on the character-select screen.
    /// </summary>
    [HarmonyPatch(typeof(Menu.OptionsMenu), MethodType.Constructor, typeof(ProcessManager))]
    internal static class PrimalSlotButtonConstructionHook
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

            Menu.SelectOneButton primalButton = new Menu.SelectOneButton(
                self,
                self.pages[0],
                "PRIMAL SLOT",
                "SaveSlot",
                newButtonPos,
                buttonSize,
                self.saveSlotButtons,
                VirtualIndex);
            self.pages[0].subObjects.Add(primalButton);

            // Make room by nudging BACKUPS (and, on console builds where it doesn't exist,
            // nothing) down by one more slot-height so the two don't overlap.
            if (self.saveBackupsButton != null)
            {
                self.saveBackupsButton.pos += new Vector2(0f, -40f);

                // Keyboard/controller nav: splice our new button in between the last save-slot
                // button and BACKUPS instead of leaving BACKUPS's old up/down links pointing
                // past us.
                Menu.SelectOneButton lastSlotButton = self.saveSlotButtons[self.saveSlotButtons.Length - 1];
                primalButton.nextSelectable[1] = lastSlotButton; // up
                primalButton.nextSelectable[3] = self.saveBackupsButton; // down
                lastSlotButton.nextSelectable[3] = primalButton;
                self.saveBackupsButton.nextSelectable[1] = primalButton;
            }
        }
    }

    /// <summary>Translates any of PrimalSlot's reserved slots (PrimalSlot.IsActive) back to our
    /// button's virtual index (3) so Menu.SelectOneButton.AmISelected reports our button -- not
    /// any of the vanilla ones -- as selected while a Primal slot is mounted, whichever real
    /// slot it belongs to. Every other slot value passes through to vanilla's own logic
    /// completely unchanged.</summary>
    [HarmonyPatch(typeof(Menu.OptionsMenu), nameof(Menu.OptionsMenu.GetCurrentlySelectedOfSeries))]
    internal static class PrimalSlotGetSelectedHook
    {
        private static bool Prefix(Menu.OptionsMenu __instance, string series, ref int __result)
        {
            if (series == "SaveSlot" && PrimalSlot.IsActive(__instance.manager.rainWorld))
            {
                __result = PrimalSlotButtonConstructionHook.VirtualIndex;
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
    internal static class PrimalSlotInfoTextHook
    {
        private static bool Prefix(Menu.OptionsMenu __instance, ref string __result)
        {
            if (__instance.selectedObject is Menu.SelectOneButton button
                && button.signalText == "SaveSlot"
                && button.buttonArrayIndex == PrimalSlotButtonConstructionHook.VirtualIndex)
            {
                __result = "Switch to this save slot's own Primal save";
                return false;
            }
            return true;
        }
    }

    /// <summary>Rewrites our button's virtual index (3) to the CURRENTLY-mounted real slot's own
    /// Primal counterpart (PrimalSlot.PrimalSlotFor) before vanilla's own
    /// SetCurrentlySelectedOfSeries switch-case runs, so that unmodified vanilla code -- the same
    /// code that already safely handles switching between slots 0/1/2, complete with its own
    /// busy-state UI and failure/revert handling -- is what actually performs the swap into (and,
    /// when a normal slot button is clicked afterward, back out of) that slot's reserved
    /// counterpart. See this file's header comment, MULTIPLE PRIMAL SLOTS, for why this reads the
    /// current slot rather than always targeting one fixed reserved slot.</summary>
    [HarmonyPatch(typeof(Menu.OptionsMenu), nameof(Menu.OptionsMenu.SetCurrentlySelectedOfSeries))]
    internal static class PrimalSlotSetSelectedHook
    {
        private static void Prefix(Menu.OptionsMenu __instance, string series, ref int to)
        {
            if (series == "SaveSlot" && to == PrimalSlotButtonConstructionHook.VirtualIndex)
            {
                RainWorld rainWorld = __instance.manager.rainWorld;
                int currentSlot = rainWorld.options.saveSlot;
                // Already on a Primal slot (shouldn't normally reach here -- clicking an
                // already-selected SelectOneButton is a vanilla no-op -- but handled rather than
                // assumed): re-derive its real slot first so we don't stack ReservedSlot twice.
                int realSlot = PrimalSlot.IsActive(rainWorld) ? PrimalSlot.RealSlotFor(currentSlot) : currentSlot;
                to = PrimalSlot.PrimalSlotFor(realSlot);
            }
        }
    }
}
