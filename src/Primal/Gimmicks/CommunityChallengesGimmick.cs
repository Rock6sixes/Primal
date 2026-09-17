using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The Community Challenges cycle-end summary. Rewritten 2026-09-17 as part of the
    /// Requirements-layer migration (see Requirement.cs's own header comment for the full "why"):
    /// this file used to OWN the 50 challenges' tracking, persistence, and per-challenge
    /// enablement (ChallengeEntry/ChallengeTracker/ChallengeCompletion, all now gone). It is now a
    /// pure CONSUMER of that layer, exactly like any other current or future Requirement consumer
    /// would be:
    ///  - It curates a fixed, ordered list of 50 Requirement IDs (CommunityChallengesRoster) that
    ///    happen to match the informal Rain World community's own list (COMMUNITY_CHALLENGES.md).
    ///  - It reads RequirementCompletion's already-persisted state for those IDs to build its own
    ///    cycle-end summary dialog.
    ///  - It subscribes to RequirementCompletion.OnSatisfied (CommunityChallengesNotifications) to
    ///    show its own popup+sound, filtered down to just its own curated IDs -- a future
    ///    consumer (a MasterSpear gate, an achievements screen, ...) would subscribe to the exact
    ///    same event independently, for whichever IDs it cares about.
    ///
    /// There is no "Community Challenges" gimmick to assign anymore -- a character's own
    /// requirement selections (the requirements column on PrimalOptions.cs's Remix config screen)
    /// decide both whether each individual challenge is tracked at all AND whether the CHALLENGES
    /// button appears on their cycle-end
    /// screen (CommunityChallengesRoster.IsAnyEnabledFor). This is the whole point of the
    /// migration: "set up the requirements needed to complete the run... everything else should be
    /// performantly and neatly automatically handled" -- there is no separate gimmick step left to
    /// perform.
    ///
    /// **Not yet built or tested** -- no .NET toolchain in this container.
    /// </summary>
    internal static class CommunityChallengesRoster
    {
        /// <summary>The 50-challenge roster's own Requirement IDs, in the same difficulty-
        /// ascending order the user's original source list (and COMMUNITY_CHALLENGES.md's table)
        /// used -- not alphabetical, not tracking-difficulty order. Each ID must match a
        /// Requirement registered in RequirementRegistry; display names come from that
        /// Requirement's own DisplayName (see Requirement.cs) rather than being duplicated here.</summary>
        public static readonly string[] IDs =
        {
            "Flytrap",
            "Visitor",
            "Chieftain",
            "Manslaughter",
            "Tasty",
            "Leukocyte",
            "Heist",
            "Fishtank",
            "Descension",
            "Visions",
            "Mycologist",
            "Catalogue",
            "Despicable",
            "Elder",
            "Feast",
            "Desperado",
            "Symbiote",
            "Vegan",
            "Friend",
            "Collector",
            "Nomad",
            "Enlightenment",
            "Journey",
            "Charged",
            "Passages",
            "Scholar",
            "Martyr",
            "Devotee",
            "Offering",
            "Sleepover",
            "Saint",
            "Safari",
            "Usurper",
            "Triplopia",
            "Ranger",
            "Spearfisher",
            "Ascension",
            "Outlaw",
            "Circuit",
            "Mother",
            "Chef",
            "Foe",
            "Ornaments",
            "Broadcast",
            "Guardian",
            "Massacre",
            "Pilgrimage",
            "Criminal",
            "UTurn",
            "Perseverance",
        };

        private static readonly HashSet<string> _idSet = new HashSet<string>(IDs);

        /// <summary>Whether `requirementID` is one of this roster's own curated 50 -- used to
        /// filter RequirementCompletion.OnSatisfied down to just the IDs this summary cares
        /// about.</summary>
        public static bool Contains(string requirementID)
        {
            return requirementID != null && _idSet.Contains(requirementID);
        }

        /// <summary>Whether `characterID` currently has at least one roster entry directly
        /// enabled (via the Requirements tab) -- what decides whether the cycle-end CHALLENGES
        /// button appears at all. Cheap enough to run at cycle-end-screen-build time (50 dictionary
        /// reads, once per cycle) without needing GimmickAssignments' own O(1)
        /// IsEnabledForAnyCharacter fast path, which is reserved for genuinely hot, per-frame/per-
        /// event checks.</summary>
        public static bool IsAnyEnabledFor(string characterID)
        {
            foreach (string id in IDs)
            {
                if (GimmickAssignments.IsEnabled(characterID, id))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>This roster's own IDs that `characterID` currently has enabled, in roster
        /// order -- what the summary dialog actually lists.</summary>
        public static List<string> EnabledFor(string characterID)
        {
            var enabled = new List<string>();
            foreach (string id in IDs)
            {
                if (GimmickAssignments.IsEnabled(characterID, id))
                {
                    enabled.Add(id);
                }
            }
            return enabled;
        }

        public static string DisplayNameFor(string requirementID)
        {
            Requirement requirement = RequirementRegistry.Find(requirementID);
            return requirement != null ? requirement.DisplayName : requirementID;
        }
    }

    /// <summary>
    /// The live, mid-cycle "challenge complete" notification -- see this class's own former home
    /// (ChallengeNotification, preserved in git history) for the full reasoning behind reusing
    /// HUD_Karma_Reinforce_Bump and HUD.TextPrompt.AddMessage. Now a thin subscriber to
    /// RequirementCompletion.OnSatisfied rather than something ReportCompletion calls directly --
    /// it fires this popup only for IDs on CommunityChallengesRoster's own curated list, and only
    /// for live (mid-cycle, real Player) completions; a save-context completion (OutlawRequirement)
    /// has no HUD to show it on and is silently skipped here, same as before.
    /// </summary>
    internal static class CommunityChallengesNotifications
    {
        private const int MessageWaitTicks = 0;
        private const int MessageDurationTicks = 100; // ~2.5s at the nominal 40 ticks/second.

        private static bool _initialized;

        /// <summary>Idempotent, matching this mod's other one-shot Init() methods (see
        /// RainWorldHooks.Postfix, which calls this alongside RequirementRegistry.RegisterAll()).</summary>
        public static void Init()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;
            RequirementCompletion.OnSatisfied += OnRequirementSatisfied;
        }

        private static void OnRequirementSatisfied(Player player, string characterID, string requirementID, bool isLive)
        {
            if (!isLive || player == null || !CommunityChallengesRoster.Contains(requirementID))
            {
                return;
            }
            ShowChallengeCompleted(player, CommunityChallengesRoster.DisplayNameFor(requirementID));
        }

        private static void ShowChallengeCompleted(Player player, string challengeDisplayName)
        {
            RainWorldGame game = GimmickGate.GameFor(player);
            RoomCamera[] cameras = game?.cameras;
            HUD.HUD hud = cameras != null && cameras.Length > 0 ? cameras[0].hud : null;
            if (hud == null)
            {
                return;
            }

            hud.PlaySound(SoundID.HUD_Karma_Reinforce_Bump);
            hud.textPrompt?.AddMessage("Challenge complete: " + challengeDisplayName, MessageWaitTicks, MessageDurationTicks, false, false);
        }
    }

    /// <summary>
    /// Adds a "CHALLENGES" button directly above the vanilla CONTINUE button on the cycle-end
    /// screen (Menu.SleepAndDeathScreen) -- explicitly requested placement, distinct from Kill
    /// List's own button (which sits above EXIT instead, one lane over). Same
    /// AddSubObjects/Singal mechanism KillListButtonHook/KillListSignalHook already established
    /// for this screen -- see that pair's own doc comment in CreatureKillListGimmick.cs for why
    /// it's safe (a Postfix here can't interfere with any of the screen's own vanilla signal
    /// handling).
    ///
    /// Gated on CommunityChallengesRoster.IsAnyEnabledFor rather than any gimmick assignment --
    /// there is no "Community Challenges" gimmick anymore (see this file's own header comment).
    ///
    /// CONTINUE's own position, confirmed via decompiling Menu/KarmaLadderScreen.cs (the base
    /// class SleepAndDeathScreen extends): `ContinueAndExitButtonsXPos - 180f - SafeScreenOffset.x`,
    /// `Mathf.Max(SafeScreenOffset.y, 15f)` -- this button reuses that same X (its own lane) and
    /// adds +40f to Y, the identical "+40f, stack directly above" offset KillListButtonHook already
    /// uses one lane over (-320f, EXIT's own lane) rather than reading either button's `.pos` back.
    /// </summary>
    [HarmonyPatch(typeof(Menu.SleepAndDeathScreen), nameof(Menu.SleepAndDeathScreen.AddSubObjects))]
    internal static class CommunityChallengesButtonHook
    {
        internal const string SignalText = "PRIMAL_COMMUNITYCHALLENGES";

        private static void Postfix(Menu.SleepAndDeathScreen __instance)
        {
            RainWorld rainWorld = __instance.manager?.rainWorld;
            string characterID = PrimalMenuAccess.GetCharacterID(__instance);
            if (!CommunityChallengesRoster.IsAnyEnabledFor(characterID))
            {
                return;
            }

            // Freeze this cycle's own fresh completions for the dialog to read if the player
            // clicks this button -- see RequirementCompletion's own header comment for why this
            // has to happen HERE (screen build), not lazily on click.
            RequirementCompletion.SnapshotAndResetForNewCheck(characterID);

            float x = __instance.ContinueAndExitButtonsXPos - 180f - rainWorld.options.SafeScreenOffset.x;
            float y = Mathf.Max(rainWorld.options.SafeScreenOffset.y, 15f) + 40f;

            var button = new Menu.SimpleButton(__instance, __instance.pages[0], "CHALLENGES", SignalText, new Vector2(x, y), new Vector2(110f, 30f));
            __instance.pages[0].subObjects.Add(button);
        }
    }

    [HarmonyPatch(typeof(Menu.SleepAndDeathScreen), nameof(Menu.SleepAndDeathScreen.Singal))]
    internal static class CommunityChallengesSignalHook
    {
        private static void Postfix(Menu.SleepAndDeathScreen __instance, Menu.MenuObject sender, string message)
        {
            if (message != CommunityChallengesButtonHook.SignalText)
            {
                return;
            }

            SaveState saveState = PrimalMenuAccess.GetSaveState(__instance);
            string characterID = saveState?.saveStateNumber?.value;
            if (characterID == null || __instance.manager == null)
            {
                return;
            }

            __instance.manager.ShowDialog(new CommunityChallengesDialog(__instance.manager, characterID, saveState.deathPersistentSaveData));
        }
    }

    /// <summary>
    /// The cycle-end challenge-progress popup. Plain text list (no icons -- unlike KillListDialog,
    /// there's no vanilla sprite per challenge to reuse), built ONLY from challenges currently
    /// enabled in this character's own requirement selections
    /// (CommunityChallengesRoster.EnabledFor), each shown in one of three styles per explicit
    /// request:
    ///  - Completed since the button's own last screen-build check
    ///    (RequirementCompletion.WasSatisfiedSinceLastCheck) -- bold (MenuLabel's own bigText, i.e.
    ///    Custom.GetDisplayFont() instead of Custom.GetFont() -- confirmed via decompiling
    ///    Menu/MenuLabel.cs to be the actual mechanism behind "bigText", there's no separate
    ///    bold-weight font in this engine) and pure white.
    ///  - Completed on some earlier cycle -- left at MenuLabel's own default (unset) color, i.e.
    ///    "default grey as with all UI elements" is simply not overriding it, the same as every
    ///    other plain label on this mod's own screens.
    ///  - Never completed -- darkened, reusing the exact dimming color (Menu.Menu.MenuRGB
    ///    (Menu.Menu.MenuColors.MediumGrey)) KillListDialog already uses for its own "not yet
    ///    killed" entries.
    ///
    /// Built as a Menu.Dialog subclass, same reasoning as KillListDialog (the full-screen
    /// backdrop/rounded panel/Continue-blocking side-process machinery "for free", see that
    /// class's own doc comment) -- row-major fill (row = i / Columns, col = i % Columns), matching
    /// KillListDialog's own RenderSection fill order.
    ///
    /// **Not yet built or tested in-game** -- no .NET toolchain in this container.
    /// </summary>
    internal sealed class CommunityChallengesDialog : Menu.Dialog
    {
        private const int Columns = 5;
        private const float ColumnWidth = 150f;
        private const float RowHeight = 32f;
        private const float Margin = 24f;
        private const float TitleHeight = 34f;
        private const float CloseButtonAreaHeight = 60f;
        private const string CloseSignal = "PRIMAL_COMMUNITYCHALLENGES_CLOSE";

        private readonly string _characterID;
        private readonly DeathPersistentSaveData _saveData;

        public CommunityChallengesDialog(ProcessManager manager, string characterID, DeathPersistentSaveData saveData)
            : base(string.Empty, ComputeSize(characterID), manager)
        {
            _characterID = characterID;
            _saveData = saveData;

            List<string> enabled = CommunityChallengesRoster.EnabledFor(characterID);
            int completedCount = 0;
            foreach (string id in enabled)
            {
                if (RequirementCompletion.IsCompleted(saveData, characterID, id))
                {
                    completedCount++;
                }
            }

            float cursorY = pos.y + size.y - Margin;
            var title = new Menu.MenuLabel(this, pages[0], $"COMMUNITY CHALLENGES -- {completedCount}/{enabled.Count}", new Vector2(pos.x, cursorY - TitleHeight), new Vector2(size.x, TitleHeight), true);
            pages[0].subObjects.Add(title);
            cursorY -= TitleHeight;

            if (enabled.Count == 0)
            {
                var placeholder = new Menu.MenuLabel(this, pages[0], "(no challenges selected in this character's Requirements tab)", new Vector2(pos.x + Margin, cursorY - RowHeight), new Vector2(size.x - Margin * 2f, RowHeight), false);
                placeholder.label.alignment = FLabelAlignment.Left;
                pages[0].subObjects.Add(placeholder);
            }
            else
            {
                for (int i = 0; i < enabled.Count; i++)
                {
                    int row = i / Columns;
                    int col = i % Columns;
                    float cellX = pos.x + Margin + col * ColumnWidth;
                    float cellCenterY = cursorY - row * RowHeight - RowHeight * 0.5f;
                    RenderEntry(enabled[i], cellX, cellCenterY);
                }
            }

            var closeButton = new Menu.SimpleButton(this, pages[0], "CLOSE", CloseSignal, new Vector2(pos.x + (size.x - 110f) * 0.5f, pos.y + Mathf.Max(size.y * 0.02f, 10f)), new Vector2(110f, 30f));
            pages[0].subObjects.Add(closeButton);
        }

        public override void Singal(Menu.MenuObject sender, string message)
        {
            if (message == CloseSignal)
            {
                manager.StopSideProcess(this);
                return;
            }
            base.Singal(sender, message);
        }

        private void RenderEntry(string requirementID, float cellX, float cellCenterY)
        {
            bool completedThisCycle = RequirementCompletion.WasSatisfiedSinceLastCheck(_characterID, requirementID);
            bool completedEver = completedThisCycle || RequirementCompletion.IsCompleted(_saveData, _characterID, requirementID);

            string displayName = CommunityChallengesRoster.DisplayNameFor(requirementID);
            var label = new Menu.MenuLabel(this, pages[0], displayName, new Vector2(cellX, cellCenterY - 10f), new Vector2(0f, 20f), completedThisCycle);
            label.label.alignment = FLabelAlignment.Left;

            if (completedThisCycle)
            {
                label.label.color = Color.white;
            }
            else if (!completedEver)
            {
                label.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            }
            // else: completed on an earlier cycle -- leave MenuLabel's own default color alone
            // ("default grey as with all UI elements").

            pages[0].subObjects.Add(label);
        }

        private static Vector2 ComputeSize(string characterID)
        {
            int count = CommunityChallengesRoster.EnabledFor(characterID).Count;
            int rows = count > 0 ? Mathf.CeilToInt(count / (float)Columns) : 1;

            float width = Columns * ColumnWidth + Margin * 2f;
            float height = Margin * 2f + TitleHeight + CloseButtonAreaHeight + rows * RowHeight;
            return new Vector2(width, height);
        }
    }
}
