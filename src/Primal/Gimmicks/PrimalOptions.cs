using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreSlugcats;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The mod's Remix config screen. Rewritten 2026-09-17, twice: first replaced the old 5x2
    /// character-button grid with a single OpComboBox character select, then (round 3, per direct
    /// request -- the combo box's own live-update bug is fixed, but a grid was still preferred)
    /// replaced it again with dedicated, colored, name-only character buttons -- see the CharGrid*
    /// constants and BuildPermanentItems. Also replaces the old two-tab (Gimmicks / Requirements)
    /// layout with one tab whose detail area is split vertically -- gameplay-altering gimmicks on
    /// the left, run-completion requirements on the right -- since both apply to the same selected
    /// character and the split tabs made switching between them more friction than it needed to be.
    ///
    /// Permanent (added once in Initialize, rebuilt fresh on every ShowNormal()): title, the
    /// "Change Mode" button, and the character button grid. Everything else is the swappable
    /// "detail area" (_detailItems), driven by DetailView:
    ///
    ///  Split -- the normal view: the selected character's assigned gimmicks (left) and enabled
    ///     requirements (right), each its own scrollable list with a "+ Add" button at the TOP (no
    ///     header label above it -- round 3 also moved these up from the bottom and dropped the
    ///     "GIMMICKS"/"REQUIREMENTS" captions, per direct request). A gimmick's name button opens
    ///     its settings; a requirement has no settings yet, so its row is a plain label. Both sides
    ///     have their own "x" remove button per row.
    ///  GimmickSettings -- one gimmick's own settings sub-view (GimmickDef.BuildSettingsUI),
    ///     rendered as a full-width overlay ("modal") over the whole detail area.
    ///  AddGimmick / AddRequirement -- a 4-column, scrollable grid of everything NOT currently
    ///     assigned/enabled for this character; clicking an entry assigns it and closes straight
    ///     back to Split.
    ///
    /// **Why these "modals" are still just OpTab detail-view swaps, not real separate windows.**
    /// Remix's own modder-facing UI controls (OpCheckBox, OpScrollBox, OpComboBox, and every
    /// existing gimmick's own settings UI, e.g. ChainsSettings' banlist) only exist inside an
    /// OpTab/ConfigContainer -- they cannot be hosted inside a true Menu.Dialog side-process
    /// (the mechanism CommunityChallengesDialog/KillListDialog use), which only understands plain
    /// Menu.MenuObject-derived controls (Menu.SimpleButton, Menu.MenuLabel, ...). Rebuilding every
    /// existing gimmick's settings screen in that vanilla-menu vocabulary just to get true floating
    /// windows would be a much larger rewrite than "change up the Remix menu UI" asked for, so
    /// instead a "modal" here is the detail area's normal split content being replaced wholesale by
    /// full-width overlay content with its own bordered backdrop (AddModalBackdrop) and a "< Back"
    /// button -- visually a distinct window, structurally still the same ShowDetail swap the old
    /// Settings/AddGimmick sub-views already used.
    ///
    /// Every (character, gimmick) and (character, requirement) "enabled" Configurable&lt;bool&gt;
    /// is still bound eagerly for every pair up front (EnsureConfigBound), same as before this UI
    /// pass -- only how they're *displayed* changed. GimmickAssignments.IsEnabled (read every frame
    /// by the gimmick/requirement patches themselves) is completely unaffected by which detail view
    /// happens to be open.
    ///
    /// A gimmick only ever takes effect while playing on this mod's reserved save slot
    /// (PrimalSlot.ReservedSlot) -- every other slot plays every character exactly as vanilla
    /// intends, unmodified.
    ///
    /// There are no separate "alt" SlugBase characters: the character select always lists the same,
    /// real vanilla roster, on every slot. This screen assigns gimmicks/requirements straight to
    /// those real characters (e.g. "Misery on Gourmand"), so a gimmick-modified playthrough still
    /// keeps every vanilla property, cutscene, and world-state interaction completely intact --
    /// nothing about the character's own identity is ever faked or substituted.
    ///
    /// Registered with the game's Remix menu from RainWorldHooks.RainWorld_OnModsInit via
    /// MachineConnector.SetRegisteredOI -- see that file for why it has to happen there rather
    /// than in Plugin.OnEnable.
    ///
    /// **2026-09-17: a mode-selection gate sits in front of all of the above.** The very first
    /// time this screen is opened (ScreenMode's own persisted Configurable&lt;string&gt; is still
    /// empty), it shows a title/credits/description blurb (sourced straight from `mod.name`/
    /// `mod.authors`/`mod.description`, i.e. modinfo.json, rather than a second hand-typed copy of
    /// the same text) and two choices: Primal mode (view every character's current assignments,
    /// nothing editable) or Custom mode (exactly the screen described above, fully editable). Once
    /// a mode is chosen it's persisted the same way every other setting on this screen is, so
    /// reopening Remix later skips straight to the Split view; a "Change Mode" button in the
    /// normal view's own top-right corner returns to this chooser without touching the persisted
    /// choice until a new one is actually clicked. Primal mode is enforced two ways: every control
    /// that would change an assignment (`+ Add`/`x` buttons, and everything a gimmick's own
    /// settings sub-view returns) has `greyedOut` set true, which is Remix's own real
    /// interactivity gate (confirmed via OpSimpleButton/OpCheckBox's own Update() -- a greyed-out
    /// control's click handling never runs, it's not just dimmed), and SetEnabledFor itself
    /// no-ops while read-only as a second, UI-independent line of defense.
    ///
    /// **Not yet built or tested** -- no .NET toolchain in this container.
    /// </summary>
    internal sealed class PrimalOptions : OptionInterface
    {
        private struct CharacterEntry
        {
            public readonly SlugcatStats.Name Name;
            public readonly string ID;
            public readonly string DisplayName;

            public CharacterEntry(SlugcatStats.Name name, string displayName)
            {
                Name = name;
                ID = name.value;
                DisplayName = displayName;
            }
        }

        // The real vanilla roster. Display names match the characters' in-game names, not their
        // internal SlugcatStats.Name values, so the Remix screen reads naturally (e.g. "Hunter",
        // not "Red").
        //
        // Built by a method rather than a fixed array initializer, and gated by ModManager.MSC/
        // .Watcher, because MoreSlugcatsEnums.SlugcatStatsName.*/Watcher.WatcherEnums
        // .SlugcatStatsName.Watcher are only meaningfully registered while their owning DLC is
        // actually enabled -- every OTHER place in this codebase that touches either enum already
        // guards it the same way (see PrimalCharacterNames.cs's Watcher branch,
        // PrimalUnimplementedPortraits.cs's MSC/Watcher checks). This array used to reference all
        // six DLC characters unconditionally, which is a static field initializer -- evaluated
        // once, unconditionally, the instant this type is first touched (from
        // RainWorldHooks.Postfix registering the Remix screen) -- so with MoreSlugcats and/or
        // Watcher disabled in the in-game Mods menu, this threw and the whole PrimalOptions type
        // failed to initialize, which is exactly why Remix reported "this mod does not have a
        // remix config menu" for Primal even though RainWorldHooks.cs did call
        // MachineConnector.SetRegisteredOI -- the call itself never returned successfully.
        private static readonly CharacterEntry[] AllCharacters = BuildAllCharacters();

        private static CharacterEntry[] BuildAllCharacters()
        {
            List<CharacterEntry> characters = new List<CharacterEntry>
            {
                new CharacterEntry(SlugcatStats.Name.White, "Survivor"),
                new CharacterEntry(SlugcatStats.Name.Yellow, "Monk"),
                new CharacterEntry(SlugcatStats.Name.Red, "Hunter"),
            };

            if (ModManager.MSC)
            {
                characters.Add(new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Gourmand, "Gourmand"));
                characters.Add(new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Artificer, "Artificer"));
                characters.Add(new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Rivulet, "Rivulet"));
                characters.Add(new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Saint, "Saint"));
                characters.Add(new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Spear, "Spearmaster"));
                // "Inv" -- More Slugcats' secret/non-canon slugcat (unlocked via the "sofanthiel"
                // main-menu code). Internally still SlugcatStats.Name.Sofanthiel -- that's the
                // actual field name in MoreSlugcatsEnums.SlugcatStatsName (confirmed via the real
                // game assembly's metadata), the community just calls the character "Inv"
                // everywhere else. Confirmed via disassembling SlugcatStats.SlugcatCanMaul that
                // Sofanthiel is *also* one of the two classes (with Artificer) mauling is already
                // restricted to in vanilla -- see NoMaulingGimmick.cs -- which is why NoMauling was
                // worth giving him too.
                characters.Add(new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel, "Inv"));
            }

            if (ModManager.Watcher)
            {
                characters.Add(new CharacterEntry(Watcher.WatcherEnums.SlugcatStatsName.Watcher, "Watcher"));
            }

            return characters.ToArray();
        }

        // What the detail area, directly below the (permanent) title/character-select row, is
        // currently showing.
        private enum DetailView
        {
            Split,
            GimmickSettings,
            AddGimmick,
            AddRequirement,
        }

        // 2026-09-17 (UI redesign round 4): which category tab the AddGimmick/AddRequirement
        // pickers currently show. "General" for a gimmick means GimmickDef.HomeCharacter == null
        // (not tied to any one character, e.g. Kill List/No Passages); "Specialized" means it has
        // one. "General" for a requirement means it's NOT one of the 50 curated Community
        // Challenges (CommunityChallengesRoster); "CommunityChallenges" means it is. Both are pure
        // filters over existing data, not a new field on GimmickDef/Requirement.
        private enum GimmickPickerCategory
        {
            General,
            Specialized,
        }

        private enum RequirementPickerCategory
        {
            General,
            CommunityChallenges,
        }

        private const float TitleY = 570f;

        // Remix's OptionInterface screen is a fixed ~600x600 coordinate space (confirmed by
        // disassembling the game's own MoreSlugcatsOptionInterface, whose own elements never sit
        // outside roughly that range). Every view's horizontal layout is sized against this usable
        // content width. Bumped from 10f/580f (same width on both sides, 600 - 2*10 = 580) to
        // 18f/564f the same day icons were removed, per direct request for a bit more left/right
        // breathing room around the whole screen.
        private const float ContentLeft = 18f;
        private const float ContentWidth = 564f;

        // ---- title row: "Primal" / mode indicator (left) + the single "Change Mode" button (right) ----
        // 2026-09-17 (UI redesign round 2, per direct request): the old two stacked buttons
        // (Change Mode, then a separate slot-switch-key rebinder below it) are now ONE button
        // occupying their combined footprint -- the slot-switch key is no longer rebindable from
        // this screen at all (PrimalSettings.SlotSwitchKey still works, just at whatever key was
        // last bound or its hardcoded default -- see PrimalSettings.cs). Its label is split across
        // two lines ("Change\nMode") to visually fill the taller button -- FLabel (the underlying
        // Futile label every Op* text element renders through) honors literal newlines, but this is
        // the first place in this codebase that relies on that, so give it a look on the next build.
        private const float ChangeModeButtonWidth = 140f;
        private const float ChangeModeButtonHeight = 72f;
        private const float ChangeModeButtonX = ContentLeft + ContentWidth - ChangeModeButtonWidth;

        // ---- the character button grid: dedicated, colored, per-character buttons replacing the
        // 2026-09-17 OpComboBox (round 3, per direct request -- the combo box's own live-update bug
        // is fixed, see AI_CONTEXT.md, but a grid was still preferred). Sized to fit entirely
        // alongside the Change Mode button, in the same vertical span the title row itself used to
        // share with the (now-removed) slot-switch-key row -- five columns, two rows, comfortably
        // covering the current 10-entry roster (3 base + 6 MSC + 1 Watcher) with room to spare if a
        // future character is ever added as an 11th entry (it just starts a 3rd row).
        private const int CharGridColumns = 5;
        private const float CharGridButtonHeight = 32f;
        private const float CharGridRowGap = 8f;
        private const float CharGridColGap = 8f;
        private const float CharGridTopY = TitleY - 44f;
        private const float CharGridBottomY = CharGridTopY - (CharGridButtonHeight + CharGridRowGap);
        // Leaves a gap before the Change Mode button starts.
        private const float CharGridAreaWidth = ChangeModeButtonX - ContentLeft - 20f;
        private const float CharGridButtonWidth = (CharGridAreaWidth - (CharGridColumns - 1) * CharGridColGap) / CharGridColumns;

        // The Change Mode button's own vertical span exactly matches the character grid's (same
        // top, same bottom) so the two visually read as one consolidated top area.
        private const float ChangeModeButtonY = CharGridBottomY;

        // The detail area starts a comfortable gap below the character grid.
        private const float DetailTopY = CharGridBottomY - 20f;
        private const float DetailBottomY = 10f;

        // ---- the Split view's two columns -----------------------------------------------------
        private const float ColumnGap = 20f;
        private const float ColumnWidth = (ContentWidth - ColumnGap) / 2f;
        private const float LeftColumnX = ContentLeft;
        private const float RightColumnX = LeftColumnX + ColumnWidth + ColumnGap;

        // Added the same day icons were removed, per direct request ("hovering over a gimmick makes
        // it go a bit out of bounds"): a name button/Add button built flush to a column's own edge
        // (LOCAL x=0) has nowhere to grow into when OpSimpleButton's own focused/hovered rect grows
        // (`_rect.addSize = new Vector2(6f, 6f) * bumpBehav.AddSize`, confirmed via decompiling
        // OpSimpleButton.GrafUpdate and DyeableRect -- addSize expands the rect symmetrically, so
        // ~half of it, up to a few px, pushes past whichever edge has no headroom), so it visibly
        // pokes past the column's true boundary. Insetting the Add button AND the OpScrollBox itself
        // by this much on both sides (rather than just padding individual rows, which are already
        // positioned relative to the scroll box's own now-inset origin) gives that bump room to grow
        // into without leaving the visible column area at all. First tried at 8f -- reported as
        // still visibly poking out, so bumped to 16f; BumpBehaviour.AddSize's own ramp-up curve
        // (sizeBump approaching 1 PLUS a transient extraSizeBump-driven sine bump that can push the
        // combined value above 1 mid-animation, not just a flat +1) makes the worst case larger than
        // a first read of the formula suggests, so this is deliberately generous rather than
        // computed to an exact minimum.
        private const float ColumnSidePadding = 16f;
        private const float ColumnContentWidth = ColumnWidth - 2f * ColumnSidePadding;

        // 2026-09-17 (UI redesign round 3): "+ Add Gimmick"/"+ Add Requirement" moved to the TOP of
        // each column (per direct request, so they're reachable without scrolling down first), and
        // the "GIMMICKS"/"REQUIREMENTS" header labels above them were dropped entirely -- the two
        // Add buttons themselves, plus the character grid right above the whole Split view, already
        // make each column's purpose obvious without a redundant caption.
        private const float ColumnAddButtonHeight = 30f;
        private const float ColumnAddButtonY = DetailTopY - ColumnAddButtonHeight;
        private const float ColumnListViewportTop = ColumnAddButtonY - 10f;
        private const float ColumnListViewportBottom = DetailBottomY;
        private const float ColumnListViewportHeight = ColumnListViewportTop - ColumnListViewportBottom;

        private const float ColumnRowHeight = 36f;
        private const float ColumnRemoveButtonW = 32f;
        private const float ColumnRowGap = 8f;
        // 2026-09-17 (UI redesign round 3, per direct request -- rows were visually running into the
        // OpScrollBox's own slide bar): a vertical OpScrollBox draws its slide bar inset from its
        // OWN right edge (confirmed via decompiling OpScrollBox -- the bar sits at `size.x - 20f`),
        // but AddItems has no idea that reservation exists -- a row built to the scroll box's full
        // (now ColumnSidePadding-inset) own width ends with its "x" remove button sitting flush
        // against, and effectively under, that slide bar. Reserving this clearance up front keeps
        // every row's rightmost element short of the scroll box's true right edge, same as
        // ModalListViewportWidth already does for the wider modal scroll boxes.
        // 30f, not a round 20f matching the slide bar's own width exactly (confirmed via
        // decompiling OpScrollBox: with the (pos, size, ...) constructor this codebase always uses,
        // _isTab is false, so the bar sits at exactly `size.x - 20f` to `size.x`) -- a small extra
        // margin so this doesn't degrade to a paper-thin few-pixel gap from one more rounding
        // adjustment somewhere in this row's own math.
        private const float ColumnScrollbarClearance = 30f;
        private const float ColumnNameButtonW = ColumnContentWidth - ColumnScrollbarClearance - ColumnRemoveButtonW - ColumnRowGap;

        // ---- the GimmickSettings/AddGimmick/AddRequirement "modal" overlays --------------------
        // Each spans the full detail area (both Split columns combined) rather than half of it.
        private const float ModalTop = DetailTopY;
        private const float ModalBottom = DetailBottomY;
        private const float ModalHeaderHeight = 40f;
        private const float ModalListViewportTop = ModalTop - ModalHeaderHeight;
        private const float ModalListViewportBottom = ModalBottom;
        private const float ModalListViewportHeight = ModalListViewportTop - ModalListViewportBottom;
        // Narrower than the full content width so the scroll box itself doesn't run edge-to-edge --
        // NOT the same thing as leaving room for its own slide bar (that's PickerScrollbarClearance
        // below; this margin alone was not enough, see that constant's own comment).
        private const float ModalListViewportWidth = ContentWidth - 40f;
        private const float ModalContentTopPadding = 20f;

        // Added the same day icons were removed, per direct request for more left/right padding and
        // to fix hover bump-highlight poking past a picker button's own edge -- same reasoning as
        // ColumnSidePadding above (including that constant's own note on why 8f wasn't enough and
        // this is 16f too), applied to the picker's own content area (both its category tabs and its
        // grid) instead of the Split view's columns. ModalListViewportWidth itself (and so the
        // modal's own backdrop/frame) is unchanged -- only what's laid out inside it shrinks and
        // shifts inward.
        private const float PickerSidePadding = 16f;
        private const float PickerAreaX = ContentLeft + PickerSidePadding;
        private const float PickerAreaWidth = ModalListViewportWidth - 2f * PickerSidePadding;

        // 2026-09-17 (UI redesign round 4, per direct request -- reported with a screenshot showing
        // the Add Requirement picker's own slide bar drawn directly on top of its 4th column):
        // same root cause as ColumnScrollbarClearance above -- PickerColumnWidth used to be sized to
        // fill the ENTIRE ModalListViewportWidth (the scroll box's own width), so the last column's
        // buttons landed exactly where OpScrollBox draws its slide bar (`size.x - 20f`, confirmed via
        // decompilation). Subtracted here, before the per-column math, rather than shrinking
        // ModalListViewportWidth itself (which also sizes the OpScrollBox's own visual frame/
        // backdrop and shouldn't change).
        // Same 30f-not-20f reasoning as ColumnScrollbarClearance above.
        private const float PickerScrollbarClearance = 30f;
        private const float PickerContentWidth = PickerAreaWidth - PickerScrollbarClearance;

        private const int PickerColumns = 4;
        private const float PickerColumnGap = 10f;
        private const float PickerColumnWidth = (PickerContentWidth - (PickerColumns - 1) * PickerColumnGap) / PickerColumns;
        private const float PickerRowHeight = 40f;

        // ---- category tabs atop the AddGimmick/AddRequirement pickers (UI redesign round 4) -----
        // Two full-width toggle buttons (General/Specialized for gimmicks, General/Community
        // Challenges for requirements) sit between the "< Back"/title row and the picker grid,
        // filtering which not-yet-assigned entries the grid below shows. Both categorizations are
        // derived from data the picker already has (GimmickDef.HomeCharacter presence, and
        // CommunityChallengesRoster membership) -- no new fields needed on GimmickDef/Requirement.
        private const float PickerCategoryTabHeight = 30f;
        private const float PickerCategoryTabGap = 10f;
        private const float PickerCategoryTabY = ModalListViewportTop - PickerCategoryTabHeight;
        private const float PickerCategoryTabWidth = (PickerAreaWidth - PickerCategoryTabGap) / 2f;
        // The grid's own viewport shrinks to make room for the tab row above it.
        private const float PickerListViewportTop = PickerCategoryTabY - PickerCategoryTabGap;
        private const float PickerListViewportHeight = PickerListViewportTop - ModalListViewportBottom;

        // ---- the mode-selection screen (shown before ScreenMode has ever been chosen) ----------
        private const float ModeAuthorY = TitleY - 26f;
        private const float ModeDescriptionHeight = 100f;
        private const float ModeDescriptionY = ModeAuthorY - 16f - ModeDescriptionHeight;
        private const float ModeChoiceHeadingY = ModeDescriptionY - 20f;
        private const float ModeButtonWidth = 160f;
        private const float ModeButtonHeight = 50f;
        private const float ModeChoiceTextGap = 20f;
        private const float ModeChoiceRow1Y = ModeChoiceHeadingY - 50f;
        private const float ModeChoiceRowGap = 90f;
        private const float ModeChoiceRow2Y = ModeChoiceRow1Y - ModeChoiceRowGap;

        // The screen's own top-level state -- ModeSelect (the credits/description blurb plus the
        // Primal/Custom choice) or Normal (title/slot-switch/character-select plus the Split view
        // and its modals, i.e. everything this class already did before the mode gate existed).
        private enum ScreenState
        {
            ModeSelect,
            Normal,
        }

        private OpTab _tab;
        private ScreenState _screenState = ScreenState.ModeSelect;

        // The Normal state's own permanent row (title, "Change Mode" button, character button grid)
        // -- tracked separately from _detailItems since it has to be torn down too when switching
        // back to ModeSelect (unlike a plain Split<->modal switch within Normal, which only ever
        // touches _detailItems).
        private readonly List<UIelement> _permanentItems = new List<UIelement>();

        // The ModeSelect state's own content -- title/credits/description plus the two mode-choice
        // rows. Never coexists with _permanentItems/_detailItems; see ClearAllScreens.
        private readonly List<UIelement> _modeSelectItems = new List<UIelement>();

        // Everything in the detail area below the (permanent) title/character-select row, tracked
        // so it can be wiped and replaced wholesale every time the selected character or detail
        // view changes.
        private readonly List<UIelement> _detailItems = new List<UIelement>();

        private DetailView _detailView = DetailView.Split;

        // Reset to General each time a picker is (re)opened -- see OpenAddGimmickPicker/
        // OpenAddRequirementPicker -- rather than remembered across sessions; a fresh "+ Add" click
        // starting on the same tab it always has is simpler to reason about than remembering the
        // last-viewed category from a previous, possibly long-past, add.
        private GimmickPickerCategory _gimmickPickerCategory = GimmickPickerCategory.General;
        private RequirementPickerCategory _requirementPickerCategory = RequirementPickerCategory.General;
        private string _charID;
        private string _gimmickID;

        // Backs the character button grid -- also what persists "which character this screen last
        // had selected" across restarts, the same Remix-config mechanism every other setting on
        // this screen already uses. 2026-09-17 (UI redesign round 3): the grid's own buttons are
        // plain OpSimpleButtons (not UIconfig/Configurable-bound controls), so unlike the OpComboBox
        // this replaced, they can safely be rebuilt fresh on every BuildPermanentItems() call --
        // Gotcha #4's "construct a Configurable-backed control at most once" rule specifically does
        // not apply to them (see that section in AI_CONTEXT.md). Each button's OnClick writes this
        // Configurable directly and force-saves (SelectCharacter), exactly like every other write on
        // this screen (SetEnabledFor, SelectMode) already does.
        private Configurable<string> _selectedCharacterConfig;

        // Persists which mode this screen is in, across restarts -- "" (never chosen, show
        // ModeSelect), "Primal" (read-only), or "Custom" (fully editable). See IsReadOnly.
        private Configurable<string> _modeConfig;

        // True while ScreenMode is "Primal" -- every control that would change an assignment
        // checks this (or is built with greyedOut set from it) before allowing an edit. See this
        // class's own header comment for the two-layer enforcement (UI greyedOut + this same check
        // again inside SetEnabledFor). Internal (not private), and Instance below exists, so
        // PrimalPortraitOverride.cs (2026-09-17) -- a Harmony patch on the character-select menu,
        // with no relationship to this specific OptionInterface instance otherwise -- can read
        // "is the Remix screen currently set to read-only Primal mode" from a totally different
        // call context. There is exactly one PrimalOptions instance for the process's whole
        // lifetime (RainWorldHooks constructs it once and hands it to MachineConnector
        // .SetRegisteredOI), so a plain static self-reference is safe and never stale.
        internal bool IsReadOnly => _modeConfig != null && _modeConfig.Value == "Primal";

        internal static PrimalOptions Instance { get; private set; }

        public PrimalOptions()
        {
            Instance = this;
        }

        // Guards EnsureConfigBound() against running its config.Bind() loop more than once on
        // this instance -- see that method's own doc comment for why it now gets called from two
        // different places (RainWorldHooks, eagerly, and here in Initialize(), lazily) and why
        // ConfigHolder.Bind() would throw ArgumentException("key [...] is already in use") the
        // second time around without this guard.
        private bool _configBound;

        public override void Initialize()
        {
            base.Initialize();

            EnsureConfigBound();

            _tab = new OpTab(this, "Primal");
            Tabs = new OpTab[] { _tab };

            if (string.IsNullOrEmpty(_modeConfig.Value))
            {
                ShowModeSelect();
            }
            else
            {
                _charID = _selectedCharacterConfig.Value;
                ShowNormal();
            }
        }

        // ---- top-level screen state: mode selection vs. the normal character/gimmicks/requirements view ----

        // Tears down whichever of the three item lists is currently attached to the tab -- called
        // before switching between ModeSelect and Normal, since (unlike a plain Split<->modal
        // switch within Normal) that transition has to replace the permanent row too, not just
        // _detailItems.
        private void ClearAllScreens()
        {
            if (_modeSelectItems.Count > 0)
            {
                _tab.RemoveItems(_modeSelectItems.ToArray());
                _modeSelectItems.Clear();
            }
            if (_detailItems.Count > 0)
            {
                _tab.RemoveItems(_detailItems.ToArray());
                _detailItems.Clear();
            }
            if (_permanentItems.Count > 0)
            {
                _tab.RemoveItems(_permanentItems.ToArray());
                _permanentItems.Clear();
            }
        }

        private void ShowModeSelect()
        {
            ClearAllScreens();
            _screenState = ScreenState.ModeSelect;
            RenderModeSelect();
        }

        private void ShowNormal()
        {
            ClearAllScreens();
            _screenState = ScreenState.Normal;
            BuildPermanentItems();
            _detailView = DetailView.Split;
            RenderDetail();
        }

        // Both mode-choice buttons on the ModeSelect screen call this. Persists immediately (same
        // "don't rely on Remix's own easy-to-miss SAVE button" reasoning SetEnabledFor's own doc
        // comment traces in full) and drops straight into the Normal view.
        private void SelectMode(string mode)
        {
            _modeConfig.Value = mode;
            SaveConfigNow("the screen mode (" + mode + ")");
            _charID = _selectedCharacterConfig.Value;
            ShowNormal();
        }

        // The Normal state's own permanent row: title + mode indicator, the single "Change Mode"
        // button in the top-right corner, and the character button grid below them. 2026-09-17 (UI
        // redesign round 3, per direct request): dropped the slot-switch-key rebinder entirely and
        // replaced the character OpComboBox with dedicated, colored, name-only buttons -- see this
        // method's own inline comments and the constants block above for the reasoning on each.
        private void BuildPermanentItems()
        {
            var items = new List<UIelement>
            {
                new OpLabel(ContentLeft, TitleY, "Primal", true),
                new OpLabel(ChangeModeButtonX - 150f, TitleY, IsReadOnly ? "Mode: Primal (read-only)" : "Mode: Custom", false)
            };

            OpSimpleButton changeModeButton = new OpSimpleButton(
                new Vector2(ChangeModeButtonX, ChangeModeButtonY),
                new Vector2(ChangeModeButtonWidth, ChangeModeButtonHeight),
                "Change\nMode")
            {
                description = "Return to the Primal/Custom mode selection screen."
            };
            changeModeButton.OnClick += _ => ShowModeSelect();
            items.Add(changeModeButton);

            // The character button grid -- one colored button per real character (row-major, 5
            // columns). Plain OpSimpleButtons, safe to rebuild fresh every call (see
            // _selectedCharacterConfig's own doc comment on why Gotcha #4 doesn't apply here).
            for (int i = 0; i < AllCharacters.Length; i++)
            {
                CharacterEntry entry = AllCharacters[i];
                int row = i / CharGridColumns;
                int col = i % CharGridColumns;
                float x = ContentLeft + col * (CharGridButtonWidth + CharGridColGap);
                float y = CharGridTopY - row * (CharGridButtonHeight + CharGridRowGap);

                OpSimpleButton charButton = new OpSimpleButton(
                    new Vector2(x, y),
                    new Vector2(CharGridButtonWidth, CharGridButtonHeight),
                    entry.DisplayName)
                {
                    description = "Edit " + entry.DisplayName + "'s gimmicks and requirements."
                };
                Color? color = GetHomeCharacterColor(entry.Name);
                if (color.HasValue)
                {
                    charButton.colorEdge = color.Value;
                }
                string capturedID = entry.ID;
                charButton.OnClick += _ => SelectCharacter(capturedID);
                items.Add(charButton);
            }

            _permanentItems.AddRange(items);
            _tab.AddItems(_permanentItems.ToArray());
        }

        // Writes the selected-character Configurable directly and force-saves, exactly like every
        // other write on this screen (SetEnabledFor, SelectMode) -- setting .Value fires
        // Configurable.OnChange synchronously (OnCharacterSelectionChanged), which re-renders the
        // detail area immediately. Deliberately never greyed/blocked in Primal mode -- switching
        // which character is shown isn't "changing the configuration," same as Change Mode itself.
        private void SelectCharacter(string id)
        {
            if (_selectedCharacterConfig.Value == id)
            {
                return;
            }
            _selectedCharacterConfig.Value = id;
            SaveConfigNow("the selected character (" + id + ")");
        }

        // The screen shown before ScreenMode has ever been chosen (and reachable again afterward
        // via the Normal view's own "Change Mode" button). Title/credits/description sourced
        // straight from `mod` (modinfo.json) rather than a second hand-typed copy of the same text,
        // so it can't drift out of sync with it.
        private void RenderModeSelect()
        {
            var items = new List<UIelement>();

            items.Add(new OpLabel(ContentLeft, TitleY, mod.name + " v" + mod.version, true));
            items.Add(new OpLabel(ContentLeft, ModeAuthorY, "by " + mod.authors, false));

            items.Add(new OpLabelLong(
                new Vector2(ContentLeft, ModeDescriptionY),
                new Vector2(ContentWidth, ModeDescriptionHeight),
                mod.description,
                true,
                FLabelAlignment.Left));

            items.Add(new OpLabel(ContentLeft, ModeChoiceHeadingY, "Choose how this screen behaves:", true));

            AddModeChoice(
                items,
                ModeChoiceRow1Y,
                "PRIMAL MODE",
                "View every character's current gimmick and requirement assignments. Nothing can be changed while in this mode.",
                () => SelectMode("Primal"));

            AddModeChoice(
                items,
                ModeChoiceRow2Y,
                "CUSTOM MODE",
                "Freely assign gimmicks and requirements to any character, exactly like before.",
                () => SelectMode("Custom"));

            _modeSelectItems.AddRange(items);
            _tab.AddItems(_modeSelectItems.ToArray());
        }

        // One mode-choice row: a button on the left, its own explanatory text to the right --
        // exactly the layout requested.
        private static void AddModeChoice(List<UIelement> items, float y, string buttonLabel, string description, Action onClick)
        {
            OpSimpleButton button = new OpSimpleButton(new Vector2(ContentLeft, y), new Vector2(ModeButtonWidth, ModeButtonHeight), buttonLabel);
            button.OnClick += _ => onClick();
            items.Add(button);

            items.Add(new OpLabelLong(
                new Vector2(ContentLeft + ModeButtonWidth + ModeChoiceTextGap, y - 10f),
                new Vector2(ContentWidth - ModeButtonWidth - ModeChoiceTextGap, ModeButtonHeight + 20f),
                description,
                true,
                FLabelAlignment.Left));
        }

        /// <summary>
        /// Binds every (character, gimmick) and (character, requirement) "enabled"
        /// Configurable&lt;bool&gt; (plus each gimmick's own extra settings, via
        /// GimmickDef.BindSettings), and the mod-wide "which character is selected" and
        /// "which mode this screen is in" Configurable&lt;string&gt;s -- pure config-holder
        /// bookkeeping, no UI construction.
        /// Idempotent: the actual binding only ever runs once per instance (guarded by
        /// _configBound); every call after the first is a silent no-op, which is required since
        /// ConfigHolder.Bind() throws if the same key is bound twice.
        ///
        /// Called from two places, and BOTH matter:
        ///
        ///  1. RainWorldHooks, eagerly, right after this same instance is registered via
        ///     MachineConnector.SetRegisteredOI -- i.e. during RainWorld.OnModsInit, long before
        ///     the player has ever opened this mod's Remix screen.
        ///  2. Initialize() above, lazily -- the framework only ever calls Initialize() on an
        ///     OptionInterface when the player actually selects this mod's page in the in-game
        ///     Mods menu (confirmed by disassembling ConfigContainer._InitializeItf/_LoadItfs in
        ///     the real game assembly: newly-registered OIs are just stored in the OptItfs array,
        ///     and Initialize() is only ever invoked on whichever one is currently "active" --
        ///     i.e. on-screen).
        ///
        /// Call #1 is the actual fix for a real persistence bug (reported 2026-09-12: "on game
        /// reset, gimmicks dont get saved on which characters they got applied to"). Traced by
        /// disassembling the real game assembly's Remix/config plumbing end to end:
        ///   - RainWorld.PostModsInit (which runs automatically, right after OnModsInit, for every
        ///     game launch -- no player action needed) calls MachineConnector._LoadAllConfigs(),
        ///     which calls OptionInterface._LoadConfigFile() on every registered OI, including
        ///     ours.
        ///   - _LoadConfigFile() is a no-op unless HasConfigurables() is already true for that OI
        ///     -- i.e. unless config.Bind() has already been called for at least one key. Only
        ///     then does it call config.Reload(), which is the method that actually reads this
        ///     mod's saved config file off disk (ConfigHolder.GetConfigPath()) and applies the
        ///     saved values.
        ///   - Before this fix, config.Bind() only ever happened inside Initialize() -- which, per
        ///     point 2 above, only runs once the player opens this mod's Remix page. So on every
        ///     game launch, by the time PostModsInit's automatic load pass ran, HasConfigurables()
        ///     was still false (nothing had been Bound yet), _LoadConfigFile() silently did
        ///     nothing, and the previous session's saved (character, gimmick) assignments were
        ///     never read off disk into memory at all for that game session -- regardless of
        ///     whether the player went on to open Remix later. Every gimmick's own Harmony patch
        ///     reads GimmickAssignments.IsEnabled(), which falls back to the hardcoded
        ///     GimmickDefaults whenever a Configurable was never Registered for that key -- so in
        ///     practice this looked exactly like "gimmick assignments silently reset to their
        ///     defaults every time the game restarts," matching the user's report precisely.
        ///   - Fix: bind everything as early as RainWorldHooks itself (RainWorld.OnModsInit's own
        ///     Postfix), which is guaranteed to run before RainWorld.PostModsInit in the normal
        ///     mod-load sequence. That makes HasConfigurables() true in time for PostModsInit's
        ///     automatic _LoadAllConfigs() pass to actually call Reload() and load last session's
        ///     saved values -- no dependency on the player ever opening this mod's Remix screen.
        /// This method only ever fixed the *load* side of the persistence bug. The *save* side
        /// turned out to be broken too, in two separate ways of its own -- see SetEnabledFor()'s
        /// own doc comment below for that whole (longer) trace, including the final root cause:
        /// ConfigHolder.Save() is genuinely non-public at runtime despite showing public flags in
        /// this project's compile-time reference assembly (the same Gotcha #1 class of bug already
        /// documented for fields, now confirmed to apply to methods too).
        /// </summary>
        internal void EnsureConfigBound()
        {
            if (_configBound)
            {
                return;
            }
            _configBound = true;

            // Mod-wide settings (the slot-switch key -- still read by PrimalSlotTabSwitchHook even
            // though this screen no longer exposes a way to rebind it, see the CharGrid* constants'
            // own comment -- and the selected-character value) bind here too, for exactly the same
            // reason everything else does -- see Gotcha #7: an unbound Configurable is skipped by
            // Remix's own config-load pass, so its saved value never survives a restart.
            PrimalSettings.Bind(this);

            string defaultCharacterID = AllCharacters.Length > 0 ? AllCharacters[0].ID : "";
            _selectedCharacterConfig = config.Bind(
                "SelectedCharacter",
                defaultCharacterID,
                new ConfigurableInfo("Character currently shown on the Primal config screen."));
            // Fires after _selectedCharacterConfig.Value already reflects the new selection (see
            // Configurable<T>.Value's own setter) -- both when a character button's own OnClick
            // (SelectCharacter) writes it directly, and, just as importantly, when Remix's own
            // config.Reload() applies a persisted value from a previous session, so a reopened
            // screen starts on whichever character was last selected without any extra wiring here.
            _selectedCharacterConfig.OnChange += OnCharacterSelectionChanged;

            // "" means no mode has ever been chosen -- Initialize() reads this directly (not via
            // OnChange) to decide whether to show ModeSelect or jump straight to Normal, so no
            // change-handler subscription is needed here the way the character select needed one.
            _modeConfig = config.Bind(
                "ScreenMode",
                "",
                new ConfigurableInfo("Whether this screen is in read-only Primal mode or fully editable Custom mode."));

            foreach (CharacterEntry character in AllCharacters)
            {
                foreach (GimmickDef gimmick in GimmickRegistry.All)
                {
                    string key = character.ID + "_" + gimmick.ID;
                    bool defaultValue = GimmickAssignments.GetDefault(character.ID, gimmick.ID);
                    Configurable<bool> configurable = config.Bind(
                        key,
                        defaultValue,
                        new ConfigurableInfo(gimmick.DisplayName + " on " + character.DisplayName + ". " + gimmick.Description));

                    GimmickAssignments.Register(character.ID, gimmick.ID, configurable);

                    // Bind this gimmick's own extra settings (if it has any) for this character,
                    // eagerly, same as the plain enabled/disabled flag above -- so the values
                    // exist before the settings sub-view is ever opened.
                    gimmick.BindSettings?.Invoke(this, character.ID);
                }

                // One Configurable<bool> per (character, requirement), bound through the exact
                // same GimmickAssignments store as the loop above -- a requirement ID and a
                // gimmick ID are both just strings to it (see GimmickAssignments' own header
                // comment). Defaults to false (GimmickAssignments.GetDefault returns false for any
                // key nothing has called SetDefault for) -- unlike gimmicks, no requirement ships
                // pre-enabled for any character; a player opts into each one explicitly.
                foreach (Requirement requirement in RequirementRegistry.All)
                {
                    string key = character.ID + "_Requirement_" + requirement.ID;
                    bool defaultValue = GimmickAssignments.GetDefault(character.ID, requirement.ID);
                    Configurable<bool> configurable = config.Bind(
                        key,
                        defaultValue,
                        new ConfigurableInfo("Track the \"" + requirement.DisplayName + "\" requirement for " + character.DisplayName + "."));

                    GimmickAssignments.Register(character.ID, requirement.ID, configurable);
                }
            }
        }

        // ---- navigation plumbing ----------------------------------------------------------

        private static bool TryFindCharacter(string id, out CharacterEntry entry)
        {
            foreach (CharacterEntry c in AllCharacters)
            {
                if (c.ID == id)
                {
                    entry = c;
                    return true;
                }
            }
            entry = default;
            return false;
        }

        private static GimmickDef FindGimmick(string id)
        {
            foreach (GimmickDef g in GimmickRegistry.All)
            {
                if (g.ID == id)
                {
                    return g;
                }
            }
            return null;
        }

        // ---- home-character coloring for gimmick/requirement rows -----------------------------

        // PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name) -- vanilla's own plain,
        // state-free per-character palette (Gourmand's tan/pink, Rivulet's blue, etc.; see
        // "Slugpup gimmick stats rescaling, in detail" in AI_CONTEXT.md for several of these
        // literal values, traced there for an unrelated fix). Deliberately NOT
        // PlayerGraphics.SlugcatColor -- that one also factors in the player's own Jolly Co-op
        // custom/auto color settings, which would make a gimmick's color tag drift depending on
        // unrelated Options rather than just reflecting the character's own identity color.
        // Resolved via reflection rather than a direct call, same as every other gameplay-class
        // member this codebase touches -- see Gotcha #1 in AI_CONTEXT.md: showing "public" in the
        // compile-time (publicized) reference assembly proves nothing about the real one.
        private static readonly MethodInfo DefaultSlugcatColorMethod =
            AccessTools.Method(typeof(PlayerGraphics), "DefaultSlugcatColor", new Type[] { typeof(SlugcatStats.Name) });

        private static readonly Dictionary<string, Color> _homeCharacterColorCache = new Dictionary<string, Color>();

        // 2026-09-17: generalized from GetHomeCharacterColor(GimmickDef) so Requirement.HomeCharacter
        // (added the same day for the four character-specific requirements) can share this exact
        // lookup/cache/try-catch instead of a second copy of it -- neither type actually needed
        // GimmickDef itself, just the SlugcatStats.Name.
        //
        // Null if `homeCharacter` is null (a gimmick/requirement with no specific character tie,
        // e.g. NoPassages) or the color lookup fails for any reason (unresolved method,
        // unrecognized character) -- callers should just leave the row's own default look alone in
        // that case rather than force one.
        private static Color? GetHomeCharacterColor(SlugcatStats.Name homeCharacter)
        {
            if (homeCharacter == null)
            {
                return null;
            }

            string key = homeCharacter.value;
            if (_homeCharacterColorCache.TryGetValue(key, out Color cached))
            {
                return cached;
            }

            // Wrapped in try/catch, unlike this codebase's other AccessTools reflection calls --
            // this one runs while building the Remix menu itself (not a per-tick gameplay patch),
            // so an uncaught MethodAccessException here (same failure mode Gotcha #1 already
            // documents for ConfigHolder.Save()) would take the whole screen down with Remix's own
            // error screen instead of just leaving this one row uncolored.
            try
            {
                object boxed = DefaultSlugcatColorMethod?.Invoke(null, new object[] { homeCharacter });
                if (!(boxed is Color color))
                {
                    return null;
                }

                _homeCharacterColorCache[key] = color;
                return color;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"PlayerGraphics.DefaultSlugcatColor threw while coloring for {key} -- leaving it uncolored: {ex}");
                return null;
            }
        }

        // 2026-09-17: this used to also draw a small creature-symbol icon (always the same
        // "Kill_Slugcat"/slugpup-ish silhouette, since there's no distinct icon SHAPE per playable
        // character in vanilla's own kill-icon atlas -- confirmed via decompiling
        // CreatureSymbol.SpriteNameOfCreature) next to a colored row's name. Removed the same day
        // per direct request ("remove the slugpup icons from all gimmicks and requirements") --
        // color alone (this method, plus the white-for-Community-Challenges rule below) is what
        // tags a row now, matching how this screen worked before icons were ever added.
        //
        // Community Challenges (CommunityChallengesRoster's own curated 50 IDs) get plain white
        // regardless of HomeCharacter (none of them set one); any other requirement with a
        // HomeCharacter (the character-specific ones) gets that character's own color; anything
        // else is left uncolored.
        private static Color? GetRequirementColor(Requirement requirement)
        {
            if (CommunityChallengesRoster.Contains(requirement.ID))
            {
                return Color.white;
            }

            return GetHomeCharacterColor(requirement.HomeCharacter);
        }

        // Generic over any (characterID, ID) pair -- a gimmick ID and a requirement ID are both
        // just strings to GimmickAssignments, so this one pair of helpers serves both columns of
        // the Split view and both Add*Modal pickers.
        private bool IsEnabledFor(string characterID, string id)
        {
            return GimmickAssignments.IsEnabled(characterID, id);
        }

        // Every assignment change goes through here (both the Add*Modal pickers and the "x" remove
        // buttons call it), so this is also the one place that writes the change to disk
        // immediately -- see this method's own doc-comment reasoning below for why that's now done
        // here rather than left to whatever Remix's own per-mod config screen chrome normally does
        // on its own "SAVE" button.
        private void SetEnabledFor(string characterID, string id, bool value)
        {
            // Second, UI-independent line of defense (see this class's own header comment): the
            // controls that call this are already greyedOut while read-only, but this guard means
            // "Primal mode changes nothing" holds even if some future control forgets to check.
            if (IsReadOnly)
            {
                return;
            }

            Configurable<bool> c = GimmickAssignments.GetConfigurable(characterID, id);
            if (c == null)
            {
                return;
            }

            c.Value = value;

            // Reported by the user (2026-09-12, second round): assigning a gimmick, then fully
            // quitting/relaunching the game, showed no change at all -- and separately, "the game
            // did not ... give me an option to apply the saved config of the remix settings."
            // EnsureConfigBound() (see its own doc comment / Gotcha #7) fixed the *load* side, but
            // that alone can't help if nothing was ever written to disk in the first place.
            // Disassembling ModdingMenu.Singal() further (specifically the block right around its
            // call to OptionInterface._SaveConfigFile()) turned up the real second half of this
            // bug: Remix's per-mod config screen chrome has its own dedicated "SAVE" button
            // (ConfigConnector.MutualVerticalSaveButtonBind is the actual UI wiring for it) --
            // config is NOT saved just by backing out of the screen normally, and there is a
            // *separate*, apparently easy-to-miss "RESET" button right next to it
            // (ConfigContainer._ResetCurrentConfig, wired the same way). Given this mod's own
            // screen fills virtually the entire ~600x600 area with its own permanent character
            // grid, that native SAVE button is easy to never notice or press at all -- matching
            // the user's report exactly. Rather than rely on the player finding and clicking a
            // button this mod's own UI doesn't call out or control the layout of, every assignment
            // change calls config.Save() immediately and unconditionally, right here, so saving is
            // fully self-contained and a crash/alt-F4 immediately after clicking +/x can't lose it.
            //
            // Round 2 shipped that as a direct `config.Save();` call and it STILL didn't work
            // ("still dont work man, rebuild copy, open game, add no passage to inv, reopen, did
            // not save"). Rather than theorize a fourth time, this was root-caused with direct
            // evidence: (a) the user's actual ModConfigs folder
            // (%AppData%\..\LocalLow\Videocult\Rain World\ModConfigs\) was checked and had no
            // rokas.altcharacters.txt at all -- proving Save() had never once succeeded, not even
            // partially -- and (b) BepInEx's own LogOutput.log was checked and had the exact
            // exception: `System.MethodAccessException: Method
            // 'OptionInterface+ConfigHolder.Save()' is inaccessible from method
            // 'AltCharacters.Gimmicks.AltCharactersOptions.SetEnabledFor(string,string,bool)'`.
            // Root cause: this is the exact Gotcha #1 class of bug, just for a method instead of a
            // field -- ConfigHolder.Save() shows public flags (0x86, Public | HideBySig) in this
            // project's compile-time reference assembly (lib/PUBLIC-Assembly-CSharp.dll, which
            // forces every member public at compile time -- see Gotcha #1), but is genuinely
            // non-public in the real runtime assembly, so calling it directly throws every single
            // time, unconditionally -- explaining why literally nothing was ever written to disk,
            // not even intermittently. The fix already established for fields elsewhere in this
            // codebase (NoPassagesGimmick.cs, PrimalCharacterNames.cs) applies the same way to
            // methods: go through HarmonyLib.AccessTools.Method(...).Invoke(...) instead of calling
            // it directly, which bypasses the runtime accessibility check entirely. See
            // ConfigSaveMethod below.
            SaveConfigNow("an assignment (" + characterID + "/" + id + " -> " + value + ")");
        }

        // Shared by SetEnabledFor and SelectMode -- both need the exact same immediate,
        // unconditional config.Save() (see the long comment above for why it can't be a direct
        // `config.Save()` call), just with different text for the error log line if it throws.
        private void SaveConfigNow(string whatChanged)
        {
            try
            {
                ConfigSaveMethod?.Invoke(config, null);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"ConfigHolder.Save() threw while persisting {whatChanged} -- the change is still in effect this session, but may not survive a restart: {ex}");
            }
        }

        // Reflection handle for OptionInterface.ConfigHolder.Save() -- see the long comment inside
        // SetEnabledFor() above for why this can't just be a direct `config.Save()` call: that
        // compiles fine and even shows public flags in this project's compile-time reference
        // assembly, but throws MethodAccessException every single time at actual runtime, because
        // ConfigHolder.Save() is genuinely non-public there. AccessTools.Method(...).Invoke(...)
        // bypasses that runtime accessibility check, the same way AccessTools.Field(...) already
        // does for non-public fields elsewhere in this codebase. Resolved once, statically, rather
        // than on every call.
        private static readonly MethodInfo ConfigSaveMethod = AccessTools.Method(typeof(ConfigHolder), "Save");

        // Wipes and replaces only the detail area (below the permanent title/character-select row)
        // -- those permanent items are never part of this list, so switching detail views never
        // touches them; it only overwrites what's below.
        private void ShowDetail(List<UIelement> items)
        {
            if (_detailItems.Count > 0)
            {
                _tab.RemoveItems(_detailItems.ToArray());
                _detailItems.Clear();
            }
            _detailItems.AddRange(items);
            _tab.AddItems(_detailItems.ToArray());
        }

        private void RenderDetail()
        {
            switch (_detailView)
            {
                case DetailView.Split:
                    RenderSplit();
                    break;
                case DetailView.GimmickSettings:
                    RenderGimmickSettingsModal();
                    break;
                case DetailView.AddGimmick:
                    RenderAddGimmickModal();
                    break;
                case DetailView.AddRequirement:
                    RenderAddRequirementModal();
                    break;
            }
        }

        // Fired by the character-select Configurable itself -- see Initialize() and
        // EnsureConfigBound(). Always drops back to Split: mid-modal state (a specific gimmick's
        // settings, an add picker) belonged to whichever character was previously selected and has
        // no meaning for a newly-selected one.
        private void OnCharacterSelectionChanged()
        {
            _charID = _selectedCharacterConfig.Value;

            // EnsureConfigBound() (and this subscription) can run well before Initialize() ever
            // does -- RainWorldHooks binds everything eagerly at RainWorld.OnModsInit, and Remix's
            // own automatic config.Reload() pass (see EnsureConfigBound's own doc comment) can fire
            // this OnChange the moment it applies a persisted value, long before the player has
            // ever opened this mod's Remix page. _tab (and _detailItems) only exist once
            // Initialize() has actually run at least once; _charID is still updated above so
            // Initialize() picks up the right value when it eventually does run. Also bail while
            // ModeSelect is showing -- the character button grid itself doesn't exist then (it's
            // part of _permanentItems, only built by BuildPermanentItems/ShowNormal), so this can
            // only legitimately fire from real interaction while _screenState is Normal.
            if (_tab == null || _screenState != ScreenState.Normal)
            {
                return;
            }

            _detailView = DetailView.Split;
            RenderDetail();
        }

        private void OpenGimmickSettings(string characterID, string gimmickID)
        {
            _charID = characterID;
            _gimmickID = gimmickID;
            _detailView = DetailView.GimmickSettings;
            RenderDetail();
        }

        private void OpenAddGimmickPicker(string characterID)
        {
            _charID = characterID;
            _gimmickPickerCategory = GimmickPickerCategory.General;
            _detailView = DetailView.AddGimmick;
            RenderDetail();
        }

        private void OpenAddRequirementPicker(string characterID)
        {
            _charID = characterID;
            _requirementPickerCategory = RequirementPickerCategory.General;
            _detailView = DetailView.AddRequirement;
            RenderDetail();
        }

        // Every modal's own "< Back" button returns here.
        private void CloseModal()
        {
            _detailView = DetailView.Split;
            RenderDetail();
        }

        private void RemoveGimmick(string characterID, string gimmickID)
        {
            SetEnabledFor(characterID, gimmickID, false);
            RenderDetail();
        }

        private void AssignGimmick(string characterID, string gimmickID)
        {
            SetEnabledFor(characterID, gimmickID, true);
            _charID = characterID;
            _detailView = DetailView.Split;
            RenderDetail();
        }

        private void RemoveRequirement(string characterID, string requirementID)
        {
            SetEnabledFor(characterID, requirementID, false);
            RenderDetail();
        }

        private void AssignRequirement(string characterID, string requirementID)
        {
            SetEnabledFor(characterID, requirementID, true);
            _charID = characterID;
            _detailView = DetailView.Split;
            RenderDetail();
        }

        // A translucent bordered panel behind a modal's own content -- purely cosmetic, but what
        // makes a modal actually read as a distinct window rather than just "the detail area
        // changed" (see this class's own header comment for why it can't be a genuinely separate
        // ProcessManager window).
        private static void AddModalBackdrop(List<UIelement> items)
        {
            items.Add(new OpRect(
                new Vector2(ContentLeft - 6f, ModalBottom - 6f),
                new Vector2(ContentWidth + 12f, (ModalTop - ModalBottom) + 12f),
                0.9f)
            {
                colorEdge = new Color(0.6f, 0.6f, 0.7f)
            });
        }

        // ---- the Split view: selected character's gimmicks (left) and requirements (right) -----

        private void RenderSplit()
        {
            if (!TryFindCharacter(_charID, out CharacterEntry character))
            {
                // Shouldn't happen once _selectedCharacterConfig has ever been bound (it always
                // carries a valid selection), but guard the same way the rest of this file already
                // does, in case a persisted SelectedCharacter value went stale (e.g. MSC/Watcher got
                // disabled since it was saved).
                _charID = AllCharacters.Length > 0 ? AllCharacters[0].ID : null;
                if (_charID == null || !TryFindCharacter(_charID, out character))
                {
                    ShowDetail(new List<UIelement>());
                    return;
                }
            }

            var items = new List<UIelement>();

            // ---- left column: gameplay-altering gimmicks ----
            // 2026-09-17 (UI redesign round 3): "+ Add Gimmick" now sits at the TOP of the column,
            // above the list, with no "GIMMICKS" caption above it -- both per direct request.
            string capturedCharIDForGimmickAdd = character.ID;
            OpSimpleButton addGimmickButton = new OpSimpleButton(new Vector2(LeftColumnX + ColumnSidePadding, ColumnAddButtonY), new Vector2(ColumnContentWidth, ColumnAddButtonHeight), "+ Add Gimmick")
            {
                description = "Add a gameplay-altering gimmick to " + character.DisplayName,
                greyedOut = IsReadOnly
            };
            addGimmickButton.OnClick += _ => OpenAddGimmickPicker(capturedCharIDForGimmickAdd);
            items.Add(addGimmickButton);

            var gimmickRows = new List<GimmickDef>();
            foreach (GimmickDef gimmick in GimmickRegistry.All)
            {
                if (IsEnabledFor(character.ID, gimmick.ID))
                {
                    gimmickRows.Add(gimmick);
                }
            }
            float gimmickContentHeight = Mathf.Max(ColumnListViewportHeight, gimmickRows.Count * ColumnRowHeight);
            OpScrollBox gimmickScroll = new OpScrollBox(
                new Vector2(LeftColumnX + ColumnSidePadding, ColumnListViewportBottom),
                new Vector2(ColumnContentWidth, ColumnListViewportHeight),
                gimmickContentHeight,
                false,  // horizontal
                false,  // hasBack
                true    // hasSlideBar
            );
            items.Add(gimmickScroll);

            // ---- right column: run-completion requirements ----
            string capturedCharIDForReqAdd = character.ID;
            OpSimpleButton addRequirementButton = new OpSimpleButton(new Vector2(RightColumnX + ColumnSidePadding, ColumnAddButtonY), new Vector2(ColumnContentWidth, ColumnAddButtonHeight), "+ Add Requirement")
            {
                description = "Add a run-completion requirement to track for " + character.DisplayName,
                greyedOut = IsReadOnly
            };
            addRequirementButton.OnClick += _ => OpenAddRequirementPicker(capturedCharIDForReqAdd);
            items.Add(addRequirementButton);

            var requirementRows = new List<Requirement>();
            foreach (Requirement requirement in RequirementRegistry.All)
            {
                if (IsEnabledFor(character.ID, requirement.ID))
                {
                    requirementRows.Add(requirement);
                }
            }
            float requirementContentHeight = Mathf.Max(ColumnListViewportHeight, requirementRows.Count * ColumnRowHeight);
            OpScrollBox requirementScroll = new OpScrollBox(
                new Vector2(RightColumnX + ColumnSidePadding, ColumnListViewportBottom),
                new Vector2(ColumnContentWidth, ColumnListViewportHeight),
                requirementContentHeight,
                false,  // horizontal
                false,  // hasBack
                true    // hasSlideBar
            );
            items.Add(requirementScroll);

            // Both scroll boxes must already be attached to the tab before their own AddItems is
            // called (OpScrollBox.AddItems throws "must be added to an OpTab before items are
            // added" otherwise, confirmed the hard way in-game pre-2026-09-17) -- commit everything
            // built so far in one ShowDetail call, then fill each scroll box afterward.
            ShowDetail(items);

            // OpScrollBox.AddItems forwards every row straight to _tab.AddItems internally, not to
            // the scroll box's own children as far as ShowDetail's teardown is concerned -- so
            // every row from BOTH scroll boxes has to end up in _detailItems too, or the next
            // ShowDetail teardown leaves them permanently attached and clickable (the exact bug
            // this file's RenderGimmickSettings/RenderAddGimmick already had to fix once before).
            var gimmickRowItems = new List<UIelement>();
            float gimmickRowY = gimmickContentHeight - ColumnRowHeight;
            foreach (GimmickDef gimmick in gimmickRows)
            {
                string capturedCharID = character.ID;
                string capturedGimmickID = gimmick.ID;

                Color? color = GetHomeCharacterColor(gimmick.HomeCharacter);

                OpSimpleButton nameButton = new OpSimpleButton(new Vector2(0f, gimmickRowY), new Vector2(ColumnNameButtonW, ColumnRowHeight - 4f), gimmick.DisplayName)
                {
                    description = gimmick.Description
                };
                if (color.HasValue)
                {
                    // colorEdge drives both the button's border AND its label text color (see
                    // OpSimpleButton.GrafUpdate) -- coloring just this, not colorFill, tags the
                    // gimmick by its home character without turning the whole button into a
                    // possibly-hard-to-read solid color block.
                    nameButton.colorEdge = color.Value;
                }
                nameButton.OnClick += _ => OpenGimmickSettings(capturedCharID, capturedGimmickID);
                gimmickRowItems.Add(nameButton);

                OpSimpleButton removeButton = new OpSimpleButton(new Vector2(ColumnNameButtonW + ColumnRowGap, gimmickRowY), new Vector2(ColumnRemoveButtonW, ColumnRowHeight - 4f), "x")
                {
                    description = "Remove " + gimmick.DisplayName + " from " + character.DisplayName,
                    greyedOut = IsReadOnly
                };
                removeButton.OnClick += _ => RemoveGimmick(capturedCharID, capturedGimmickID);
                gimmickRowItems.Add(removeButton);

                gimmickRowY -= ColumnRowHeight;
            }
            if (gimmickRows.Count == 0)
            {
                gimmickRowItems.Add(new OpLabel(0f, gimmickContentHeight - ColumnRowHeight, "No gimmicks assigned yet.", false));
            }
            gimmickScroll.AddItems(gimmickRowItems.ToArray());
            _detailItems.AddRange(gimmickRowItems);

            var requirementRowItems = new List<UIelement>();
            float requirementRowY = requirementContentHeight - ColumnRowHeight;
            foreach (Requirement requirement in requirementRows)
            {
                string capturedCharID = character.ID;
                string capturedRequirementID = requirement.ID;

                Color? color = GetRequirementColor(requirement);

                // No settings to drill into yet (see this class's own header comment), so this
                // still isn't wired to anything -- no OnClick is ever attached, so a click plays no
                // sound and does nothing (confirmed via decompiling OpSimpleButton.Update -- the
                // click branch only calls PlaySound when OnClick is non-null). Deliberately NOT
                // greyedOut (that was tried first and reverted per direct feedback: greyedOut routes
                // color through BumpBehaviour.GetColor, which forces
                // MenuColorEffect.Greyscale(MidToVeryDark(...)) unconditionally -- confirmed via
                // decompilation -- so every row read as washed-out grey regardless of colorEdge,
                // "requirements look greyed out"). A plain, ungreyed OpSimpleButton with no OnClick
                // renders and colors exactly like a gimmick's own name button -- full color, normal
                // hover/focus -- while still doing nothing when pressed.
                OpSimpleButton nameButton = new OpSimpleButton(new Vector2(0f, requirementRowY), new Vector2(ColumnNameButtonW, ColumnRowHeight - 4f), requirement.DisplayName);
                if (color.HasValue)
                {
                    nameButton.colorEdge = color.Value;
                }
                requirementRowItems.Add(nameButton);

                OpSimpleButton removeButton = new OpSimpleButton(new Vector2(ColumnNameButtonW + ColumnRowGap, requirementRowY), new Vector2(ColumnRemoveButtonW, ColumnRowHeight - 4f), "x")
                {
                    description = "Stop tracking " + requirement.DisplayName + " for " + character.DisplayName,
                    greyedOut = IsReadOnly
                };
                removeButton.OnClick += _ => RemoveRequirement(capturedCharID, capturedRequirementID);
                requirementRowItems.Add(removeButton);

                requirementRowY -= ColumnRowHeight;
            }
            if (requirementRows.Count == 0)
            {
                requirementRowItems.Add(new OpLabel(0f, requirementContentHeight - ColumnRowHeight, "No requirements tracked yet.", false));
            }
            requirementScroll.AddItems(requirementRowItems.ToArray());
            _detailItems.AddRange(requirementRowItems);
        }

        // ---- modal: one gimmick's settings for the selected character --------------------------

        private void RenderGimmickSettingsModal()
        {
            if (!TryFindCharacter(_charID, out CharacterEntry character))
            {
                CloseModal();
                return;
            }

            GimmickDef gimmick = FindGimmick(_gimmickID);
            if (gimmick == null)
            {
                CloseModal();
                return;
            }

            var items = new List<UIelement>();
            AddModalBackdrop(items);

            OpSimpleButton back = new OpSimpleButton(new Vector2(ContentLeft, ModalTop - 30f), new Vector2(120f, 30f), "< Back");
            back.OnClick += _ => CloseModal();
            items.Add(back);

            items.Add(new OpLabel(ContentLeft + 130f, ModalTop - 30f, gimmick.DisplayName + " settings -- " + character.DisplayName, true)
            {
                description = gimmick.Description
            });

            if (!gimmick.HasSettings)
            {
                items.Add(new OpLabel(ContentLeft + 10f, ModalListViewportTop, "This gimmick has no additional settings.", false));
                ShowDetail(items);
                return;
            }

            // Wrapped in a scrollable OpScrollBox -- some gimmicks' settings (Chains, notably: a
            // 12-checkbox banlist plus a preset picker and two toggles) lay out well past 400px
            // tall. For a gimmick whose settings already fit (TimeLimit, NoSpears, OneSpear,
            // ThreatPanic -- GimmickDef.SettingsContentHeight left at its default of 0),
            // Mathf.Max clamps the content size up to the viewport's own height, so this is a
            // no-op wrapper for them, not a visible behavior change.
            float contentHeight = Mathf.Max(ModalListViewportHeight, gimmick.SettingsContentHeight);
            float contentTopY = contentHeight - ModalContentTopPadding;

            OpScrollBox scrollBox = new OpScrollBox(
                new Vector2(ContentLeft, ModalListViewportBottom),
                new Vector2(ModalListViewportWidth, ModalListViewportHeight),
                contentHeight,
                false,  // horizontal
                false,  // hasBack -- no visible frame, matches this screen's plain look elsewhere
                true    // hasSlideBar
            );
            items.Add(scrollBox);

            ShowDetail(items);

            // See RenderSplit's own note on this: OpScrollBox.AddItems forwards every element
            // straight to _tab.AddItems, so these controls must also be tracked in _detailItems, or
            // the next ShowDetail teardown leaves them permanently attached and clickable.
            List<UIelement> settingsControls = gimmick.BuildSettingsUI(character.ID, contentTopY);
            if (IsReadOnly)
            {
                // A gimmick's own settings UI (banlist checkboxes, sliders, ...) writes straight to
                // its own bound Configurable on interaction -- it never goes through
                // SetEnabledFor's own read-only guard, so Primal mode has to be enforced here
                // instead, directly on whatever controls this specific gimmick happens to return.
                // OpLabel (not a UIfocusable) has no greyedOut and needs none -- it isn't
                // interactive to begin with.
                foreach (UIelement element in settingsControls)
                {
                    if (element is UIfocusable focusable)
                    {
                        focusable.greyedOut = true;
                    }
                }
            }
            scrollBox.AddItems(settingsControls.ToArray());
            _detailItems.AddRange(settingsControls);
        }

        // ---- modal: add-a-gimmick picker for the selected character ----------------------------

        private void RenderAddGimmickModal()
        {
            // Shouldn't be reachable while read-only -- the "+ Add Gimmick" button that opens this
            // is itself greyedOut in that case -- but guarded anyway, the same defense-in-depth
            // spirit as SetEnabledFor's own read-only check.
            if (IsReadOnly)
            {
                CloseModal();
                return;
            }

            if (!TryFindCharacter(_charID, out CharacterEntry character))
            {
                CloseModal();
                return;
            }

            var candidates = new List<GimmickDef>();
            foreach (GimmickDef gimmick in GimmickRegistry.All)
            {
                if (IsEnabledFor(character.ID, gimmick.ID))
                {
                    continue;
                }
                bool isGeneral = gimmick.HomeCharacter == null;
                if (isGeneral == (_gimmickPickerCategory == GimmickPickerCategory.General))
                {
                    candidates.Add(gimmick);
                }
            }

            var items = new List<UIelement>();
            AddModalBackdrop(items);

            OpSimpleButton back = new OpSimpleButton(new Vector2(ContentLeft, ModalTop - 30f), new Vector2(120f, 30f), "< Back");
            back.OnClick += _ => CloseModal();
            items.Add(back);

            items.Add(new OpLabel(ContentLeft + 130f, ModalTop - 30f, "Add a gimmick to " + character.DisplayName + ":", true));

            // Category tabs (2026-09-17, UI redesign round 4, per direct request): General (no tied
            // HomeCharacter, e.g. Kill List/No Passages) vs. Specialized (tied to one character's
            // own mechanics, e.g. Misery/Chains) -- filters `candidates` above. Clicking a tab
            // re-renders this whole modal, the same "click -> rebuild the detail area" interaction
            // every other control on this screen already uses.
            OpSimpleButton generalTab = new OpSimpleButton(new Vector2(PickerAreaX, PickerCategoryTabY), new Vector2(PickerCategoryTabWidth, PickerCategoryTabHeight), "General");
            if (_gimmickPickerCategory == GimmickPickerCategory.General)
            {
                generalTab.colorEdge = Color.white;
            }
            generalTab.OnClick += _ =>
            {
                _gimmickPickerCategory = GimmickPickerCategory.General;
                RenderDetail();
            };
            items.Add(generalTab);

            OpSimpleButton specializedTab = new OpSimpleButton(new Vector2(PickerAreaX + PickerCategoryTabWidth + PickerCategoryTabGap, PickerCategoryTabY), new Vector2(PickerCategoryTabWidth, PickerCategoryTabHeight), "Specialized");
            if (_gimmickPickerCategory == GimmickPickerCategory.Specialized)
            {
                specializedTab.colorEdge = Color.white;
            }
            specializedTab.OnClick += _ =>
            {
                _gimmickPickerCategory = GimmickPickerCategory.Specialized;
                RenderDetail();
            };
            items.Add(specializedTab);

            if (candidates.Count == 0)
            {
                items.Add(new OpLabel(ContentLeft + 10f, PickerListViewportTop, "No gimmicks in this category are available to add.", false));
                ShowDetail(items);
                return;
            }

            // 4 columns, scrollable, row-major fill (row = i / 4, col = i % 4) -- matching
            // CommunityChallengesDialog's own established row-major convention for a multi-column
            // roster display. **Exact pixel fit for the longest gimmick display names
            // ("Threat Slowdown") at 4-column width is unverified in-game as of this pass** -- same
            // honest caveat this file's settings viewports already carry elsewhere.
            int rows = Mathf.CeilToInt(candidates.Count / (float)PickerColumns);
            float contentHeight = Mathf.Max(PickerListViewportHeight, rows * PickerRowHeight);

            OpScrollBox scrollBox = new OpScrollBox(
                new Vector2(PickerAreaX, ModalListViewportBottom),
                new Vector2(PickerAreaWidth, PickerListViewportHeight),
                contentHeight,
                false,  // horizontal
                false,  // hasBack
                true    // hasSlideBar
            );
            items.Add(scrollBox);

            ShowDetail(items);

            var scrollItems = new List<UIelement>();
            string capturedCharID = character.ID;
            for (int i = 0; i < candidates.Count; i++)
            {
                GimmickDef gimmick = candidates[i];
                int row = i / PickerColumns;
                int col = i % PickerColumns;
                float x = col * (PickerColumnWidth + PickerColumnGap);
                float y = contentHeight - (row + 1) * PickerRowHeight + 4f;

                string capturedGimmickID = gimmick.ID;
                Color? color = GetHomeCharacterColor(gimmick.HomeCharacter);
                OpSimpleButton addButton = new OpSimpleButton(new Vector2(x, y), new Vector2(PickerColumnWidth, PickerRowHeight - 8f), gimmick.DisplayName)
                {
                    description = gimmick.Description
                };
                if (color.HasValue)
                {
                    addButton.colorEdge = color.Value;
                }
                addButton.OnClick += _ => AssignGimmick(capturedCharID, capturedGimmickID);
                scrollItems.Add(addButton);
            }
            // See RenderSplit's own note on this.
            scrollBox.AddItems(scrollItems.ToArray());
            _detailItems.AddRange(scrollItems);
        }

        // ---- modal: add-a-requirement picker for the selected character ------------------------

        private void RenderAddRequirementModal()
        {
            // See RenderAddGimmickModal's own note on this.
            if (IsReadOnly)
            {
                CloseModal();
                return;
            }

            if (!TryFindCharacter(_charID, out CharacterEntry character))
            {
                CloseModal();
                return;
            }

            var candidates = new List<Requirement>();
            foreach (Requirement requirement in RequirementRegistry.All)
            {
                if (IsEnabledFor(character.ID, requirement.ID))
                {
                    continue;
                }
                bool isCommunityChallenge = CommunityChallengesRoster.Contains(requirement.ID);
                if (isCommunityChallenge == (_requirementPickerCategory == RequirementPickerCategory.CommunityChallenges))
                {
                    candidates.Add(requirement);
                }
            }

            var items = new List<UIelement>();
            AddModalBackdrop(items);

            OpSimpleButton back = new OpSimpleButton(new Vector2(ContentLeft, ModalTop - 30f), new Vector2(120f, 30f), "< Back");
            back.OnClick += _ => CloseModal();
            items.Add(back);

            items.Add(new OpLabel(ContentLeft + 130f, ModalTop - 30f, "Add a requirement to " + character.DisplayName + ":", true));

            // Category tabs (2026-09-17, UI redesign round 4, per direct request): General (every
            // registered requirement NOT in the 50 curated Community Challenges, e.g. the
            // character-specific ones and Unique Kills) vs. Community Challenges (CommunityChallengesRoster's
            // own 50 IDs) -- filters `candidates` above, same interaction as RenderAddGimmickModal's
            // own tabs.
            OpSimpleButton generalTab = new OpSimpleButton(new Vector2(PickerAreaX, PickerCategoryTabY), new Vector2(PickerCategoryTabWidth, PickerCategoryTabHeight), "General");
            if (_requirementPickerCategory == RequirementPickerCategory.General)
            {
                generalTab.colorEdge = Color.white;
            }
            generalTab.OnClick += _ =>
            {
                _requirementPickerCategory = RequirementPickerCategory.General;
                RenderDetail();
            };
            items.Add(generalTab);

            OpSimpleButton communityTab = new OpSimpleButton(new Vector2(PickerAreaX + PickerCategoryTabWidth + PickerCategoryTabGap, PickerCategoryTabY), new Vector2(PickerCategoryTabWidth, PickerCategoryTabHeight), "Community Challenges");
            if (_requirementPickerCategory == RequirementPickerCategory.CommunityChallenges)
            {
                communityTab.colorEdge = Color.white;
            }
            communityTab.OnClick += _ =>
            {
                _requirementPickerCategory = RequirementPickerCategory.CommunityChallenges;
                RenderDetail();
            };
            items.Add(communityTab);

            if (candidates.Count == 0)
            {
                items.Add(new OpLabel(ContentLeft + 10f, PickerListViewportTop, "No requirements in this category are available to add.", false));
                ShowDetail(items);
                return;
            }

            // Same 4-column, scrollable, row-major grid as RenderAddGimmickModal -- driven off
            // RequirementRegistry.All, so any registered requirement (not just the curated 50
            // Community Challenges ones) is pickable here.
            int rows = Mathf.CeilToInt(candidates.Count / (float)PickerColumns);
            float contentHeight = Mathf.Max(PickerListViewportHeight, rows * PickerRowHeight);

            OpScrollBox scrollBox = new OpScrollBox(
                new Vector2(PickerAreaX, ModalListViewportBottom),
                new Vector2(PickerAreaWidth, PickerListViewportHeight),
                contentHeight,
                false,  // horizontal
                false,  // hasBack
                true    // hasSlideBar
            );
            items.Add(scrollBox);

            ShowDetail(items);

            var scrollItems = new List<UIelement>();
            string capturedCharID = character.ID;
            for (int i = 0; i < candidates.Count; i++)
            {
                Requirement requirement = candidates[i];
                int row = i / PickerColumns;
                int col = i % PickerColumns;
                float x = col * (PickerColumnWidth + PickerColumnGap);
                float y = contentHeight - (row + 1) * PickerRowHeight + 4f;

                string capturedRequirementID = requirement.ID;
                Color? color = GetRequirementColor(requirement);
                OpSimpleButton addButton = new OpSimpleButton(new Vector2(x, y), new Vector2(PickerColumnWidth, PickerRowHeight - 8f), requirement.DisplayName);
                if (color.HasValue)
                {
                    addButton.colorEdge = color.Value;
                }
                addButton.OnClick += _ => AssignRequirement(capturedCharID, capturedRequirementID);
                scrollItems.Add(addButton);
            }
            // See RenderSplit's own note on this.
            scrollBox.AddItems(scrollItems.ToArray());
            _detailItems.AddRange(scrollItems);
        }
    }
}
