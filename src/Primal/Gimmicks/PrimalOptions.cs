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
    /// The mod's Remix config screen. The character select grid is permanent -- it is added once
    /// in Initialize and never rebuilt or removed -- and a second "detail" area directly below it
    /// shows whatever is relevant to the currently-selected character. Clicking a different
    /// character in the grid does NOT navigate to a new screen: it just overwrites the detail
    /// area's contents (via OpTab.AddItems/RemoveItems on a small tracked list) while the grid
    /// itself stays exactly where it was. This replaces an earlier version of this screen where
    /// selecting a character replaced the grid entirely with a drill-down view -- the user asked
    /// for the grid to stay visible/selectable at all times instead.
    ///
    /// The detail area itself still has three sub-views, chosen by DetailView:
    ///
    ///  Gimmicks -- a table of the selected character's currently-assigned gimmicks (a name
    ///     button per row, opening that gimmick's settings, plus a small "x" button that
    ///     un-assigns it on the spot) and a "+" button spanning the table's height that opens the
    ///     add-gimmick picker. This is what shows immediately after clicking a character in the
    ///     grid, and what re-appears (for the newly-clicked character) if you click a different
    ///     character while some other detail sub-view is open.
    ///  Settings -- one specific gimmick's own settings sub-view (GimmickDef.BuildSettingsUI),
    ///     or a plain "no settings" message for gimmicks that don't have one (see GimmickDef.cs).
    ///  AddGimmick -- a button per gimmick NOT currently assigned to this character; click one
    ///     to assign it and drop back to the Gimmicks sub-view.
    ///
    /// Before anything is selected (right after opening the menu), the detail area just shows a
    /// one-line placeholder telling the user to pick a character above.
    ///
    /// Every (character, gimmick) "enabled" Configurable&lt;bool&gt; is still bound eagerly for
    /// every pair up front in Initialize (exactly as the original flat-grid version did), and so is
    /// every gimmick's own extra settings (via GimmickDef.BindSettings) -- only how they're
    /// *displayed* changed. GimmickAssignments.IsEnabled (read every frame by the gimmick patches
    /// themselves) is completely unaffected by which detail sub-view happens to be open.
    ///
    /// A gimmick only ever takes effect while playing on this mod's reserved save slot
    /// (PrimalSlot.ReservedSlot) -- every other slot plays every character exactly as vanilla
    /// intends, unmodified.
    ///
    /// There are no separate "alt" SlugBase characters: the character select screen always shows
    /// the same, real vanilla roster, on every slot. This screen assigns gimmicks straight to
    /// those real characters (e.g. "Misery on Gourmand"), so a gimmick-modified playthrough still
    /// keeps every vanilla property, cutscene, and world-state interaction completely intact --
    /// nothing about the character's own identity is ever faked or substituted.
    ///
    /// Registered with the game's Remix menu from RainWorldHooks.RainWorld_OnModsInit via
    /// MachineConnector.SetRegisteredOI -- see that file for why it has to happen there rather
    /// than in Plugin.OnEnable.
    /// </summary>
    internal sealed class PrimalOptions : OptionInterface
    {
        private struct CharacterEntry
        {
            public readonly string ID;
            public readonly string DisplayName;

            public CharacterEntry(SlugcatStats.Name name, string displayName)
            {
                ID = name.value;
                DisplayName = displayName;
            }
        }

        // The real vanilla roster. Display names match the characters' in-game names, not their
        // internal SlugcatStats.Name values, so the Remix screen reads naturally (e.g. "Hunter",
        // not "Red").
        private static readonly CharacterEntry[] AllCharacters =
        {
            new CharacterEntry(SlugcatStats.Name.White, "Survivor"),
            new CharacterEntry(SlugcatStats.Name.Yellow, "Monk"),
            new CharacterEntry(SlugcatStats.Name.Red, "Hunter"),
            new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Gourmand, "Gourmand"),
            new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Artificer, "Artificer"),
            new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Rivulet, "Rivulet"),
            new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Saint, "Saint"),
            new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Spear, "Spearmaster"),
            new CharacterEntry(Watcher.WatcherEnums.SlugcatStatsName.Watcher, "Watcher"),
            // "Inv" -- More Slugcats' secret/non-canon slugcat (unlocked via the "sofanthiel"
            // main-menu code). Internally still SlugcatStats.Name.Sofanthiel -- that's the actual
            // field name in MoreSlugcatsEnums.SlugcatStatsName (confirmed via the real game
            // assembly's metadata), the community just calls the character "Inv" everywhere else.
            // Confirmed via disassembling SlugcatStats.SlugcatCanMaul that Sofanthiel is *also*
            // one of the two classes (with Artificer) mauling is already restricted to in vanilla
            // -- see NoMaulingGimmick.cs -- which is why NoMauling was worth giving him too.
            new CharacterEntry(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel, "Inv"),
        };

        // What the (always-visible) detail area, directly below the (also always-visible)
        // character grid, is currently showing. "None" is the initial placeholder state before
        // any character has been clicked yet.
        private enum DetailView
        {
            None,
            Gimmicks,
            Settings,
            AddGimmick,
        }

        private const float TitleY = 570f;
        private const float SubtitleY = 548f;
        private const float RowHeight = 40f;

        // Remix's OptionInterface screen is a fixed ~600x600 coordinate space (confirmed by
        // disassembling the game's own MoreSlugcatsOptionInterface, whose own elements never sit
        // outside roughly that range). Every view's horizontal layout is sized against this usable
        // content width.
        private const float ContentLeft = 10f;
        private const float ContentWidth = 580f;

        // The character grid: 5 columns x 2 rows -- fits all 9 characters within the ~600-wide
        // screen (5 * 106 + 4 * 10 = 570, comfortably inside ContentWidth). This is permanent --
        // built once in Initialize -- and sits above the detail area at a fixed position.
        private const int GridColumns = 5;
        private const float GridButtonW = 106f;
        private const float GridButtonH = 40f;
        private const float GridGapX = 10f;
        private const float GridSpacingX = GridButtonW + GridGapX;
        private const float GridStartY = 460f;
        private const float GridSpacingY = -55f;

        // Bottom edge of the grid's last row (GridStartY + 1 * GridSpacingY - GridButtonH =
        // 460 - 55 - 40 = 365). The detail area starts a comfortable gap below that instead of
        // overlapping the grid, which is why it uses its own top-Y constant rather than reusing
        // the old full-screen views' ContentTopY (500) -- that value used to be safe only because
        // the grid and the detail content were never both on screen at the same time.
        private const float DetailTopY = 335f;

        private OpTab _tab;

        // Everything in the detail area below the (permanent) character grid, tracked so it can
        // be wiped and replaced wholesale every time the selected character or detail sub-view
        // changes, without touching the grid, the title, or the subtitle above it.
        private readonly List<UIelement> _detailItems = new List<UIelement>();

        // Every (character, gimmick) "is this assigned" Configurable<bool>, bound once up front
        // in Initialize -- same objects GimmickAssignments.Register hands to the runtime patches,
        // kept here too so the detail views can flip them directly (the "x" button and the
        // add-gimmick picker) without needing new lookup surface on GimmickAssignments.
        private readonly Dictionary<string, Configurable<bool>> _enabledConfigs = new Dictionary<string, Configurable<bool>>();

        private static string EnabledKey(string characterID, string gimmickID) => characterID + "|" + gimmickID;

        private DetailView _detailView = DetailView.None;
        private string _charID;
        private string _gimmickID;

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

            _tab = new OpTab(this, "Gimmicks");
            Tabs = new OpTab[] { _tab };

            // Title, subtitle, and the character grid are all permanent -- added once here and
            // never removed or rebuilt. Only _detailItems (below) is ever wiped/replaced.
            var permanentItems = new List<UIelement>
            {
                new OpLabel(10f, TitleY, "Primal -- Gimmicks", true),
                new OpLabel(10f, SubtitleY, "Select a character to edit its gimmicks:", false)
            };
            permanentItems.AddRange(BuildCharacterGrid());
            _tab.AddItems(permanentItems.ToArray());

            _detailView = DetailView.None;
            RenderDetail();
        }

        /// <summary>
        /// Binds every (character, gimmick) "enabled" Configurable&lt;bool&gt; (plus each
        /// gimmick's own extra settings, via GimmickDef.BindSettings) -- pure config-holder
        /// bookkeeping, no UI construction. Idempotent: the actual binding only ever runs once per
        /// instance (guarded by _configBound); every call after the first is a silent no-op, which
        /// is required since ConfigHolder.Bind() throws if the same key is bound twice.
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
                    _enabledConfigs[EnabledKey(character.ID, gimmick.ID)] = configurable;

                    // Bind this gimmick's own extra settings (if it has any) for this character,
                    // eagerly, same as the plain enabled/disabled flag above -- so the values
                    // exist before the settings sub-view is ever opened.
                    gimmick.BindSettings?.Invoke(this, character.ID);
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

        private bool IsEnabledFor(string characterID, string gimmickID)
        {
            return _enabledConfigs.TryGetValue(EnabledKey(characterID, gimmickID), out Configurable<bool> c) && c.Value;
        }

        // Every assignment change goes through here (both AddGimmick and RemoveGimmick call it),
        // so this is also the one place that writes the change to disk immediately -- see this
        // method's own doc-comment reasoning below for why that's now done here rather than left
        // to whatever Remix's own per-mod config screen chrome normally does on its own "SAVE"
        // button.
        private void SetEnabledFor(string characterID, string gimmickID, bool value)
        {
            if (!_enabledConfigs.TryGetValue(EnabledKey(characterID, gimmickID), out Configurable<bool> c))
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
            try
            {
                ConfigSaveMethod?.Invoke(config, null);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"ConfigHolder.Save() threw while persisting a gimmick assignment ({characterID}/{gimmickID} -> {value}) -- the change is still in effect this session, but may not survive a restart: {ex}");
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

        // Wipes and replaces only the detail area (below the permanent grid) -- the grid, title,
        // and subtitle are never part of this list, so clicking a character in the grid never
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
                case DetailView.None:
                    RenderNoSelection();
                    break;
                case DetailView.Gimmicks:
                    RenderCharacterGimmicks();
                    break;
                case DetailView.Settings:
                    RenderGimmickSettings();
                    break;
                case DetailView.AddGimmick:
                    RenderAddGimmick();
                    break;
            }
        }

        // Clicking a character in the (always-visible) grid always jumps straight to that
        // character's gimmick table, overwriting whatever the detail area was previously showing
        // -- including, deliberately, if it was mid-way through another character's settings or
        // add-gimmick picker.
        private void OpenCharacter(string characterID)
        {
            _charID = characterID;
            _detailView = DetailView.Gimmicks;
            RenderDetail();
        }

        private void OpenGimmickSettings(string characterID, string gimmickID)
        {
            _charID = characterID;
            _gimmickID = gimmickID;
            _detailView = DetailView.Settings;
            RenderDetail();
        }

        private void OpenAddGimmick(string characterID)
        {
            _charID = characterID;
            _detailView = DetailView.AddGimmick;
            RenderDetail();
        }

        private void RemoveGimmick(string characterID, string gimmickID)
        {
            SetEnabledFor(characterID, gimmickID, false);
            // Stay on the same character's table -- just refresh it.
            RenderDetail();
        }

        private void AddGimmick(string characterID, string gimmickID)
        {
            SetEnabledFor(characterID, gimmickID, true);
            _charID = characterID;
            _detailView = DetailView.Gimmicks;
            RenderDetail();
        }

        // ---- the permanent character grid ---------------------------------------------------

        private List<UIelement> BuildCharacterGrid()
        {
            var items = new List<UIelement>();

            int col = 0;
            int row = 0;
            foreach (CharacterEntry character in AllCharacters)
            {
                float x = ContentLeft + col * GridSpacingX;
                float y = GridStartY + row * GridSpacingY;
                string capturedID = character.ID;

                OpSimpleButton button = new OpSimpleButton(new Vector2(x, y), new Vector2(GridButtonW, GridButtonH), character.DisplayName)
                {
                    description = "Edit gimmicks assigned to " + character.DisplayName
                };
                button.OnClick += _ => OpenCharacter(capturedID);
                items.Add(button);

                col++;
                if (col >= GridColumns)
                {
                    col = 0;
                    row++;
                }
            }

            return items;
        }

        // ---- detail sub-view: selected character's assigned gimmicks -------------------------

        private void RenderCharacterGimmicks()
        {
            if (!TryFindCharacter(_charID, out CharacterEntry character))
            {
                _charID = null;
                _detailView = DetailView.None;
                RenderNoSelection();
                return;
            }

            var items = new List<UIelement>();

            items.Add(new OpLabel(10f, DetailTopY, character.DisplayName + " -- assigned gimmicks:", true));

            // The table (name button + "x" column) spans the full content width, with the "+"
            // button occupying its own column immediately to the right of it -- rather than a
            // narrow table with a big "+" bar below everything, which left most of the screen's
            // width empty.
            const float addColumnWidth = 90f;
            const float gapTableToAdd = 10f;
            const float tableAreaWidth = ContentWidth - addColumnWidth - gapTableToAdd;
            const float xBtnW = 40f;
            const float gapNameToX = 10f;
            const float nameBtnW = tableAreaWidth - xBtnW - gapNameToX;
            const float rowElementH = 32f;

            float firstRowY = DetailTopY - 44f;
            float rowY = firstRowY;
            float lastRowY = rowY;
            bool any = false;

            foreach (GimmickDef gimmick in GimmickRegistry.All)
            {
                if (!IsEnabledFor(character.ID, gimmick.ID))
                {
                    continue;
                }
                any = true;
                lastRowY = rowY;

                string capturedCharID = character.ID;
                string capturedGimmickID = gimmick.ID;

                OpSimpleButton nameButton = new OpSimpleButton(new Vector2(ContentLeft, rowY), new Vector2(nameBtnW, rowElementH), gimmick.DisplayName)
                {
                    description = gimmick.Description
                };
                nameButton.OnClick += _ => OpenGimmickSettings(capturedCharID, capturedGimmickID);
                items.Add(nameButton);

                OpSimpleButton removeButton = new OpSimpleButton(new Vector2(ContentLeft + nameBtnW + gapNameToX, rowY), new Vector2(xBtnW, rowElementH), "x")
                {
                    description = "Remove " + gimmick.DisplayName + " from " + character.DisplayName
                };
                removeButton.OnClick += _ => RemoveGimmick(capturedCharID, capturedGimmickID);
                items.Add(removeButton);

                rowY -= RowHeight;
            }

            if (!any)
            {
                lastRowY = rowY;
                items.Add(new OpLabel(ContentLeft, rowY, "No gimmicks assigned yet.", false));
            }

            // The "+" button sits beside the table, spanning the same vertical range as the rows
            // (or the placeholder line, if there are none yet) rather than below everything.
            float addButtonX = ContentLeft + tableAreaWidth + gapTableToAdd;
            float addButtonHeight = Mathf.Max(rowElementH, firstRowY - lastRowY + rowElementH);
            string addCharID = character.ID;
            OpSimpleButton addButton = new OpSimpleButton(new Vector2(addButtonX, lastRowY), new Vector2(addColumnWidth, addButtonHeight), "+")
            {
                description = "Add a gimmick to " + character.DisplayName
            };
            addButton.OnClick += _ => OpenAddGimmick(addCharID);
            items.Add(addButton);

            ShowDetail(items);
        }

        // ---- detail sub-view: one gimmick's settings for the selected character ---------------

        // Fixed on-screen viewport for the settings scroll box below -- same footprint the
        // settings content used to be placed at directly (DetailTopY - 84f down to a small bottom
        // margin), now the fixed *viewport* a scrollable content area sits inside instead.
        private const float SettingsViewportTop = DetailTopY - 84f;
        private const float SettingsViewportBottom = 10f;
        private const float SettingsViewportHeight = SettingsViewportTop - SettingsViewportBottom;
        // A little narrower than the full content width (unlike AddGimmickListWidth below, which
        // is narrow because its buttons are a single column by design) to leave clearance at the
        // right edge for the scroll box's own slide bar, since settings content -- Chains'
        // two-column banlist grid especially -- otherwise uses close to the full content width.
        private const float SettingsViewportWidth = ContentWidth - 40f;
        private const float SettingsContentTopPadding = 20f;

        private void RenderGimmickSettings()
        {
            if (!TryFindCharacter(_charID, out CharacterEntry character))
            {
                _charID = null;
                _detailView = DetailView.None;
                RenderNoSelection();
                return;
            }

            GimmickDef gimmick = FindGimmick(_gimmickID);
            if (gimmick == null)
            {
                _detailView = DetailView.Gimmicks;
                RenderCharacterGimmicks();
                return;
            }

            var items = new List<UIelement>();

            string capturedCharID = character.ID;
            OpSimpleButton back = new OpSimpleButton(new Vector2(10f, DetailTopY), new Vector2(180f, 30f), "< " + character.DisplayName);
            back.OnClick += _ => OpenCharacter(capturedCharID);
            items.Add(back);

            items.Add(new OpLabel(10f, DetailTopY - 40f, gimmick.DisplayName + " settings -- " + character.DisplayName, true)
            {
                description = gimmick.Description
            });

            if (!gimmick.HasSettings)
            {
                items.Add(new OpLabel(20f, SettingsViewportTop, "This gimmick has no additional settings.", false));
                ShowDetail(items);
                return;
            }

            // Wrapped in a scrollable OpScrollBox (added 2026-09-15) -- some gimmicks' settings
            // (Chains, notably: a 12-checkbox banlist plus a preset picker and two toggles) lay
            // out well past 400px tall, badly overflowing the ~241px actually available below the
            // heading -- those rows used to render off the bottom of the Remix window with no way
            // to reach them. Same OpScrollBox pattern already established for the add-gimmick
            // picker below (RenderAddGimmick) -- see that method's own comment for the exact
            // constructor arguments and the two-step-commit ordering OpScrollBox requires (it must
            // already be attached to the OpTab before AddItems is called on it, confirmed the hard
            // way in-game there). For a gimmick whose settings already fit (TimeLimit, NoSpears,
            // OneSpear, ThreatPanic -- GimmickDef.SettingsContentHeight left at its default of 0),
            // Mathf.Max clamps the content size up to the viewport's own height, so this is a
            // no-op wrapper for them, not a visible behavior change.
            float contentHeight = Mathf.Max(SettingsViewportHeight, gimmick.SettingsContentHeight);
            float contentTopY = contentHeight - SettingsContentTopPadding;

            OpScrollBox scrollBox = new OpScrollBox(
                new Vector2(ContentLeft, SettingsViewportBottom),
                new Vector2(SettingsViewportWidth, SettingsViewportHeight),
                contentHeight,
                false,  // horizontal
                false,  // hasBack -- no visible frame, matches this screen's plain look elsewhere
                true    // hasSlideBar
            );
            items.Add(scrollBox);

            ShowDetail(items);
            scrollBox.AddItems(gimmick.BuildSettingsUI(character.ID, contentTopY).ToArray());
        }

        // ---- detail sub-view: add-a-gimmick picker for the selected character ------------------

        // Fixed on-screen viewport for the add-gimmick scroll box below -- from just under the
        // "Add a gimmick to X:" heading down to a small bottom margin above the screen's edge.
        private const float AddGimmickListTop = DetailTopY - 84f;
        private const float AddGimmickListBottom = 10f;
        private const float AddGimmickListHeight = AddGimmickListTop - AddGimmickListBottom;
        private const float AddGimmickListWidth = 340f;
        private const float AddGimmickButtonWidth = 300f;

        private void RenderAddGimmick()
        {
            if (!TryFindCharacter(_charID, out CharacterEntry character))
            {
                _charID = null;
                _detailView = DetailView.None;
                RenderNoSelection();
                return;
            }

            var items = new List<UIelement>();

            string capturedCharID = character.ID;
            OpSimpleButton back = new OpSimpleButton(new Vector2(10f, DetailTopY), new Vector2(180f, 30f), "< " + character.DisplayName);
            back.OnClick += _ => OpenCharacter(capturedCharID);
            items.Add(back);

            items.Add(new OpLabel(10f, DetailTopY - 40f, "Add a gimmick to " + character.DisplayName + ":", true));

            var candidates = new List<GimmickDef>();
            foreach (GimmickDef gimmick in GimmickRegistry.All)
            {
                if (!IsEnabledFor(character.ID, gimmick.ID))
                {
                    candidates.Add(gimmick);
                }
            }

            if (candidates.Count == 0)
            {
                items.Add(new OpLabel(20f, AddGimmickListTop, "All gimmicks are already assigned to this character.", false));
                ShowDetail(items);
                return;
            }

            // Scrollable list (added 2026-09-13, request: "add a scrollwheel to the gimmick
            // picking list, because you cant see all of them at this point"). With 12 gimmicks
            // now registered, and this view only ever showing the ones NOT yet assigned to the
            // selected character, a character with few/none assigned needs up to 12 rows --
            // 12 * RowHeight (40) = 480, far more than the ~300px of screen actually available
            // below the heading, so entries used to run off the bottom of the Remix window with
            // no way to reach them.
            //
            // Uses the game's own Menu.Remix.MixedUI.OpScrollBox -- traced its real constructor
            // and AddItems usage by disassembling the one place the base game itself uses it
            // (OptionInterface.ErrorScreen, located by a raw IL token search for calls to
            // OpScrollBox's constructor across the whole assembly). OpScrollBox lives in the same
            // Menu.Remix.MixedUI namespace as OpTab/OpLabel/OpSimpleButton/Configurable<T> --
            // Remix's own modder-facing UI API, already used directly (no reflection) everywhere
            // else in this file -- so it's accessed directly here too, unlike the GOTCHA #1
            // "publicized reference assembly" concern that applies to the game's own gameplay
            // classes (Player, SlugcatStats, etc.).
            //
            // Constructor is (Vector2 pos, Vector2 size, float contentSize, bool horizontal,
            // bool hasBack, bool hasSlideBar): pos/size are the box's fixed on-screen viewport
            // (this detail area's own real estate), and contentSize is the full VIRTUAL height of
            // everything added to it -- independent of, and here taller than, the viewport height,
            // which is what makes it scrollable. Items are then added via the scroll box's OWN
            // AddItems (not the OpTab's), positioned in coordinates local to that virtual content
            // height rather than against DetailTopY -- same bottom-left-origin, Y-increases-upward
            // convention as everywhere else in this file, just measured against contentSize.
            float contentHeight = Mathf.Max(AddGimmickListHeight, candidates.Count * RowHeight);

            OpScrollBox scrollBox = new OpScrollBox(
                new Vector2(ContentLeft, AddGimmickListBottom),
                new Vector2(AddGimmickListWidth, AddGimmickListHeight),
                contentHeight,
                false,  // horizontal
                false,  // hasBack -- no visible frame, matches this screen's plain look elsewhere
                true    // hasSlideBar
            );
            items.Add(scrollBox);

            // OpScrollBox.AddItems throws ("OpScrollBox must be added to an OpTab before items
            // are added") if called before the scroll box itself has actually been added to the
            // OpTab -- confirmed the hard way in-game (2026-09-13). Every other sub-view in this
            // file builds its whole `items` list and commits it with one ShowDetail() call at the
            // end, but the scroll box specifically needs that commit to happen FIRST, so
            // ShowDetail(items) is called here -- attaching the still-empty scroll box to the tab
            // -- and the scroll box's own row buttons are added to it afterward instead.
            ShowDetail(items);

            var scrollItems = new List<UIelement>();
            float rowY = contentHeight - RowHeight;
            foreach (GimmickDef gimmick in candidates)
            {
                string capturedGimmickID = gimmick.ID;
                OpSimpleButton addButton = new OpSimpleButton(new Vector2(0f, rowY), new Vector2(AddGimmickButtonWidth, 32f), gimmick.DisplayName)
                {
                    description = gimmick.Description
                };
                addButton.OnClick += _ => AddGimmick(capturedCharID, capturedGimmickID);
                scrollItems.Add(addButton);

                rowY -= RowHeight;
            }
            scrollBox.AddItems(scrollItems.ToArray());
        }

        // ---- detail placeholder: nothing selected yet -----------------------------------------

        private void RenderNoSelection()
        {
            ShowDetail(new List<UIelement>
            {
                new OpLabel(ContentLeft, DetailTopY, "Select a character above to view or edit its gimmicks.", false)
            });
        }
    }
}
