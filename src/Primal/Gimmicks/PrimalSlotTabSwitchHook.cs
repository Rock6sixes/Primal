using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Re-adds "press Tab on the character-select screen to hop into (and back out of) Primal's
    /// reserved save slot" -- requested again after an earlier attempt at this same idea was
    /// discarded (see AI_CONTEXT.md's History section for that prior design's problems). This
    /// time it's scoped narrowly to just the slot switch itself, and deliberately reuses vanilla's
    /// own already-shipped, already-tested slot-mounting machinery rather than inventing a new
    /// one -- the same lesson PrimalSlotOptionsMenuHooks.cs's own doc comment already draws from
    /// the save-corruption incident a fully custom state machine caused previously.
    ///
    /// What "the same code that runs the normal changing save slot" means here, concretely:
    /// Menu.OptionsMenu.SetCurrentlySelectedOfSeries("SaveSlot", to) -- the method the PRIMAL SLOT
    /// options-menu button already drives (see PrimalSlotOptionsMenuHooks.cs) -- boils down (per
    /// this session's own IL disassembly of it) to exactly three calls:
    ///   options.saveSlot = to;
    ///   rainWorld.progression.Destroy(leavingSaveSlot);
    ///   rainWorld.progression = new PlayerProgression(rainWorld, true, false);
    /// followed by polling rainWorld.progression.progressionLoaded every frame (that's what
    /// OptionsMenu.Update does) until it's true, then checking progressionLoadedResult: 1
    /// (SUCCESS_LOAD_EXISTING_FILE), 2 (SUCCESS_CREATE_NEW_FILE) and 5 (ERROR_SAVE_DATA_MISSING,
    /// treated as a normal "new/empty slot" case) all mean success; anything else means
    /// OptionsMenu.HandleSaveSlotChangeFailed reverts back to the previous slot the same way.
    ///
    /// This can't literally call SetCurrentlySelectedOfSeries itself -- that's an instance method
    /// on Menu.OptionsMenu, tied to an actual OptionsMenu instance's own private waitingOnProgres
    /// -sionLoaded/leavingSaveSlot fields and its own Update() polling loop, and we're triggering
    /// this from Menu.SlugcatSelectMenu, not OptionsMenu. So PrimalSlotSwitcher below replicates
    /// the exact same three-call sequence and the exact same success/failure result handling
    /// directly, with its own polling (driven off RainWorld.Update, so it keeps running no matter
    /// which menu happens to be on screen) standing in for OptionsMenu's. Same operations, same
    /// order, same success/failure result codes -- just not routed through a live OptionsMenu
    /// instance, because there isn't one here.
    ///
    /// GOTCHA #1 (see AI_CONTEXT.md): PlayerProgression's constructor/Destroy/progressionLoaded/
    /// progressionLoadedResult and ProcessManager.RequestMainProcessSwitch/IsRunningAnyDialog are
    /// all genuinely new surface for this codebase -- nothing here has touched any of them before.
    /// The reference assembly used to confirm their signatures has every member's accessibility
    /// flag forced to "public" by whatever publicizer produced it, which is known (from this same
    /// project's own prior experience -- see PlayerGraphics.player, SlugcatStats's stat fields,
    /// ConfigHolder.Save()) to not reflect the real game's actual accessibility. So every one of
    /// them is read/invoked through HarmonyLib.AccessTools reflection below (matching that exact
    /// established pattern) instead of plain C# member access, specifically so a real private
    /// member throws nothing worse than a caught, logged exception instead of a
    /// MethodAccessException/FieldAccessException the JIT would otherwise throw for plain access
    /// compiled against the (misleading) publicized DLL. rainWorld.options.saveSlot is the one
    /// exception -- PrimalSlot.cs already reads it directly everywhere in this mod, in shipped,
    /// working code, so it's treated as proven rather than wrapped again here.
    ///
    /// Outstanding risk once this is actually built and run in-game (can only be resolved by that,
    /// not by more disassembly): whether any of the AccessTools.Field/Method/Constructor/Property
    /// lookups below come back null at runtime because the real member's name or signature
    /// doesn't match what the publicized DLL showed. Every call site checks for that and logs
    /// through Plugin.Log rather than crashing, but a null lookup means that particular Tab-press
    /// silently does nothing (or reverts) instead of switching slots -- watch the BepInEx log
    /// after testing this for the first time.
    ///
    /// TRANSITION (2026-09-13 addition, confirmed working without this first): the switch itself
    /// is bookended by a vertical slide, added once the base mechanic was confirmed working
    /// in-game. Entering Primal's slot slides the current screen up and off the top; the rebuilt
    /// screen then slides up into place from below -- one continuous "camera descending" motion.
    /// Leaving Primal reverses it exactly: slides down and off the bottom, then the restored real
    /// slot's screen slides down into place from above. This works by directly animating `.pos.y`
    /// on every `PositionedMenuObject` involved -- confirmed via disassembly that
    /// `PositionedMenuObject.Update()` (inherited unoverridden by everything below) already does
    /// `base.Update(); lastPos = pos;` every tick, and that `get_ScreenPos()` is
    /// `owner?.ScreenPos + pos` recursively -- i.e. this is the same
    /// pos/lastPos-interpolated-by-timeStacker mechanism vanilla's own UI already uses for smooth
    /// motion everywhere, not something new invented here.
    ///
    /// TRANSITION, round 2 (2026-09-13, same day -- user feedback on round 1): "the buttons moved
    /// ... but the background didnt -- gotta move the background as well" and "dont do a linear
    /// transition but a smooth one, ease in ease out." Round 2 tried moving `pages[0]` plus every
    /// illustration on `menu.scene` (`Menu.MenuScene`). **This didn't work, and round 3 (below)
    /// explains why and replaces it.**
    ///
    /// TRANSITION, round 3 (2026-09-13, same day -- user reported round 2's background fix still
    /// didn't move anything, plus "more easing"). Disassembling fresh (rather than patching round
    /// 2's approach) found the actual bug: `Menu.SlugcatSelectMenu` never builds a `Menu.MenuScene`
    /// at all -- its constructor (checked line by line) never touches the `scene` field, so
    /// `menu.scene` is just null for this menu and round 2's illustration-collecting code always
    /// had nothing to do. Whatever the player sees as "background" on this screen isn't a
    /// `MenuScene` illustration and isn't necessarily even a `PositionedMenuObject` at all, so
    /// chasing it object-by-object was the wrong level to work at.
    ///
    /// The actual fix sidesteps that question entirely. Every `Menu.MenuObject.Container` (a
    /// property, disassembled this session) resolves recursively -- `myContainer` if this object
    /// set its own, else `owner.Container`, else (the base case, for anything with no owner and no
    /// override, e.g. every `Page`) `menu.container`. `Menu.container` itself (an `FContainer`,
    /// confirmed via `Menu.Menu`'s own constructor: created once, added straight to
    /// `Futile.stage`, never touched again by `Menu.Update`/`GrafUpdate`) is therefore the single
    /// Futile display-list node that *everything* this menu ever draws -- pages, subobjects, any
    /// scene, any raw sprite someone added directly -- ends up parented under, directly or
    /// indirectly, regardless of which of those mechanisms it happens to use. So instead of
    /// collecting and moving a list of individual objects, each slide phase now just animates
    /// `menu.container.y`. The `.y` part is a plain public float property inherited from `FNode`,
    /// the same class `FSprite` -- already used directly for the black screen below -- extends, so
    /// no GOTCHA #1 reflection is needed for that part. `Menu.container` itself, though, is
    /// genuinely new surface (never read directly anywhere in this codebase before, unlike
    /// `Menu.pages`), so per GOTCHA #1 it's still fetched through `AccessTools.Field` below
    /// (`MenuContainerField`) rather than a plain `menu.container` read, even though it's an
    /// ordinary reference-type field with nothing about it that looks unusual. This moves
    /// literally everything the menu draws as one unit, with no need to know how any particular
    /// piece of it is built.
    ///
    /// Easing: also asked for "more easing." The sinusoidal curve from round 2
    /// (`0.5 - 0.5*cos(pi*t)`) is a fairly gentle ease -- over the short duration used, its slow
    /// start/end barely read as different from linear. `EaseInOutFraction` is now a cubic
    /// ease-in-out (`4t^3` for the first half, mirrored for the second) instead, which accelerates
    /// and decelerates much more sharply at each end, and `SlideDurationTicks` was raised from 16
    /// to 30 (~0.75s) so there's enough time for that shape to actually be visible rather than
    /// mostly happening within a couple of frames. Still applied as the tick-to-tick *difference*
    /// between the eased fraction at the previous and current tick, exactly as round 2 did -- only
    /// the curve and the duration changed, not the stepping mechanism.
    ///
    /// The existing black-screen-during-mount behavior is untouched and still brackets the actual
    /// save-slot swap in between the two slides -- the slide phases only run while a real,
    /// still-fully-rendered menu is on screen (old before mount, new after), so there's still no
    /// need to relayer the black cover behind anything to make the motion visible. Also, moving
    /// `container.y` directly (rather than a `PositionedMenuObject.pos`) needs none of round 1/2's
    /// `lastPos` workaround for a one-frame Lerp glitch -- `FNode.y` has no separate interpolated
    /// draw position the way `PositionedMenuObject.ScreenPos`/`DrawPos` does, so setting it once at
    /// the start of the slide-in takes effect immediately with nothing further to account for.
    ///
    /// CHARACTER PRESERVATION (2026-09-13, same day, next request): "when switching, it doesnt
    /// switch between most recent in both save files but switches to the same character -- so if
    /// you are on primal artificer, you switch to normal artificer. if you are on normal gourmand
    /// you switch to primal gourmand." Without this, switching slots lands wherever THAT slot's own
    /// save data happens to default to (see below), unrelated to whichever character was on screen
    /// before Tab was pressed.
    ///
    /// Disassembled `Menu.SlugcatSelectMenu`'s constructor and `Update()` to find how the initially-
    /// shown character is actually chosen, rather than guessing. The screen shows exactly one
    /// character at a time (`slugcatPageIndex`, an int into `slugcatPages`) and steps it by ±1 via a
    /// queued-input/scroll-swipe mechanism (`quedSideInput`, `scroll`/`lastScroll`) driven by the
    /// PREV/NEXT buttons -- there's no grid of clickable portraits. Every place that changes
    /// `slugcatPageIndex` also calls the menu's own `UpdateSelectedSlugcatInMiscProg()`, which
    /// disassembly showed does exactly `progression.miscProgressionData.currentlySelectedSinglePlayer
    /// Slugcat = slugcatPages[slugcatPageIndex].slugcatNumber` -- i.e. that persisted field is
    /// *written from* the on-screen selection, not read back to restore it: the constructor's own
    /// initial-selection logic (also disassembled) only auto-picks a character when that slot has
    /// save data for exactly one, and otherwise just leaves the field at its default (index 0).
    /// That's the actual source of "switches to most recent/whatever's default" the user described.
    ///
    /// The fix: capture which character was showing on the OLD menu (`GetSelectedSlugcat`, reading
    /// `slugcatPages[slugcatPageIndex].slugcatNumber`) right when Tab is pressed, carry it through
    /// `State.TargetSlugcat`, and once the rebuilt menu exists, find that same `SlugcatStats.Name`
    /// in its own `slugcatPages` and point `slugcatPageIndex` straight at it (`ApplyCharacterFix`) --
    /// resetting `scroll`/`lastScroll`/`quedSideInput` to a resting state (rather than queuing up N
    /// simulated button-presses to walk there one step at a time) and calling
    /// `UpdateSelectedSlugcatInMiscProg()` ourselves so the persisted field matches too. Every field
    /// this touches (`slugcatPages`, `slugcatPageIndex`, `scroll`, `lastScroll`, `quedSideInput`, and
    /// `SlugcatPage.slugcatNumber`) is new surface never read directly in this codebase before, so
    /// per GOTCHA #1 all of it goes through `AccessTools` (see the new fields below) rather than
    /// plain member access -- a failed lookup just leaves whichever character the slot defaulted to,
    /// logged, instead of crashing. (`Menu.SlugcatSelectMenu.SlugcatPage` itself, as a *type* used in
    /// field/cast declarations, is trusted directly rather than reflected around: its sibling nested
    /// class `SlugcatPageNewGame` is already used exactly that way -- including as a live Harmony
    /// postfix parameter type -- in this mod's already-shipped, already-working
    /// `PrimalCharacterNames.cs`, which wouldn't function at all if that nesting weren't genuinely
    /// accessible.)
    ///
    /// Because each `SlugcatPage`'s own on-screen position is computed incrementally frame-to-frame
    /// (its own `Update()`/`GrafUpdate()`, not reverse-engineered in full) rather than recomputed
    /// instantly from `slugcatPageIndex`, jumping the index directly (instead of animating through it
    /// one step at a time like a real button press would) risks a stale-looking frame or two right
    /// after the jump. Rather than chase that down, `OnSlugcatSelectMenuRebuilt` now defers dropping
    /// the black screen: it applies the fix, then leaves `state.PendingRevealMenu` set for a fixed
    /// `RevealSettleFrames` (6) real Update ticks -- handled by the existing `Poll` (already running
    /// every `RainWorld.Update` frame) -- before finally calling `HideBlackScreen`/`BeginSlideIn`.
    /// That's a sixth of a second of extra time already-hidden behind the loading cover, cheap
    /// insurance against a glitch nobody will ever actually see.
    ///
    /// MULTIPLE PRIMAL SLOTS (2026-09-13, same day, next request): "instead of switching between
    /// save slot 1/2/3 and primal slot (666666) lets switch between slot 1 and primal slot 1
    /// (666666) then when in slot 2 you switch between slot 2 and primal slot 2 (666667) and so
    /// for slot 3" -- previously every real slot Tab'd into the same single reserved slot
    /// (PrimalSlot.ReservedSlot, 666666), so switching from real slot 2 and switching from real
    /// slot 1 both landed on the exact same Primal save, and Tab-ing back out always returned to
    /// whichever real slot was tracked in a single process-global PrimalSlot.LastRealSlot --
    /// wrong the moment you'd used Primal from more than one real slot in the same session.
    ///
    /// Fixed at the source (PrimalSlot.cs, see its header comment) rather than here: each real
    /// slot N now has its own reserved slot, ReservedSlot + N, and the mapping is a pure function
    /// both ways (PrimalSlotFor/RealSlotFor) instead of a separately-tracked variable. This file's
    /// only change is Postfix now computing `targetSlot` from whichever slot is CURRENTLY mounted
    /// via that mapping, in both directions, rather than reading/writing LastRealSlot (removed
    /// entirely -- nothing else referenced it). Everything downstream of `targetSlot` --
    /// BeginSlideOut, BeginSwitch, the whole PlayerProgression mount/poll machinery, character
    /// preservation -- is unchanged and works identically no matter which specific slot number
    /// comes out of the mapping, since none of it ever hard-coded ReservedSlot itself.
    /// </summary>
    [HarmonyPatch(typeof(Menu.SlugcatSelectMenu), nameof(Menu.SlugcatSelectMenu.Update))]
    internal static class PrimalSlotTabSwitchHook
    {
        private static void Postfix(Menu.SlugcatSelectMenu __instance)
        {
            RainWorld rainWorld = __instance?.manager?.rainWorld;
            if (rainWorld == null)
            {
                return;
            }

            PrimalSlotSwitcher.State state = PrimalSlotSwitcher.GetState(rainWorld);

            if (state.Phase != PrimalSlotSwitcher.TransitionPhase.None &&
                (state.SlidingContainer == null || !ReferenceEquals(state.SlidingContainer, PrimalSlotSwitcher.GetMenuContainer(__instance))))
            {
                // The menu instance we were animating is gone (e.g. the player backed all the way
                // out mid-transition) -- abandon the stale transition instead of leaving Tab
                // permanently unresponsive on whatever menu shows up next.
                state.Phase = PrimalSlotSwitcher.TransitionPhase.None;
                state.SlidingContainer = null;
            }

            if (state.Phase == PrimalSlotSwitcher.TransitionPhase.SlideOut)
            {
                PrimalSlotSwitcher.StepSlideOut(rainWorld, state);
                return;
            }

            if (state.Phase == PrimalSlotSwitcher.TransitionPhase.SlideIn)
            {
                PrimalSlotSwitcher.StepSlideIn(rainWorld, state);
                return;
            }

            if (state.Waiting)
            {
                // Already mid-switch (or mid-revert) -- ignore repeated Tab presses. Poll (below,
                // hooked on RainWorld.Update so it keeps running no matter which menu is active)
                // drives this to completion on its own.
                return;
            }

            // Configurable as of 2026-09-15 (PrimalSettings.SlotSwitchKey, shown on this mod's own
            // Remix screen) -- it was a hardcoded KeyCode.Tab before that, the one player-facing
            // behavior in the mod with no way to change it if another mod wanted the same key.
            // Falls back to Tab if the Configurable hasn't been bound yet.
            if (!Input.GetKeyDown(PrimalSettings.SlotSwitchKey))
            {
                return;
            }

            if (PrimalSlotSwitcher.IsAnyDialogRunning(rainWorld))
            {
                // Don't fight whatever's already on screen (e.g. a corrupted-save notice).
                return;
            }

            // Which slot to switch TO is now a pure function of which slot we're currently on --
            // see PrimalSlot.cs's header comment, MULTIPLE PRIMAL SLOTS -- so there's no separate
            // "which real slot to return to" state to keep in sync here anymore.
            int currentSlot = rainWorld.options.saveSlot;
            int targetSlot;
            bool enteringPrimal;
            if (PrimalSlot.IsActive(rainWorld))
            {
                targetSlot = PrimalSlot.RealSlotFor(currentSlot);
                enteringPrimal = false;
            }
            else
            {
                targetSlot = PrimalSlot.PrimalSlotFor(currentSlot);
                enteringPrimal = true;
            }

            // Whichever character is showing right now -- carried through so the rebuilt menu can
            // be pointed back at the same one instead of whatever the new slot's save data would
            // otherwise default to. See this file's header comment, CHARACTER PRESERVATION.
            SlugcatStats.Name characterToPreserve = PrimalSlotSwitcher.GetSelectedSlugcat(__instance);

            PrimalSlotSwitcher.BeginSlideOut(rainWorld, state, __instance, targetSlot, enteringPrimal, characterToPreserve);
        }
    }

    /// <summary>Marks the moment a freshly-rebuilt SlugcatSelectMenu exists again after
    /// PrimalSlotSwitcher requested a process switch to rebuild it against the new slot -- the
    /// right moment to drop our black-screen cover, since it's before the new instance has ever
    /// actually been drawn.</summary>
    [HarmonyPatch(typeof(Menu.SlugcatSelectMenu), MethodType.Constructor, typeof(ProcessManager))]
    internal static class PrimalSlotTabSwitchMenuRebuiltHook
    {
        private static void Postfix(Menu.SlugcatSelectMenu __instance)
        {
            RainWorld rainWorld = __instance?.manager?.rainWorld;
            if (rainWorld == null)
            {
                return;
            }
            PrimalSlotSwitcher.OnSlugcatSelectMenuRebuilt(rainWorld, __instance);
        }
    }

    /// <summary>Drives PrimalSlotSwitcher's per-frame wait for PlayerProgression to finish
    /// mounting. Hooked on RainWorld.Update (not SlugcatSelectMenu.Update) on purpose: once a
    /// switch is requested, the character-select menu is about to be torn down and rebuilt from
    /// under itself, so nothing about waiting for that can depend on any one menu instance still
    /// being the active menu.</summary>
    [HarmonyPatch(typeof(RainWorld), nameof(RainWorld.Update))]
    internal static class PrimalSlotTabSwitchPollHook
    {
        private static void Postfix(RainWorld __instance)
        {
            PrimalSlotSwitcher.Poll(__instance);
        }
    }

    internal static class PrimalSlotSwitcher
    {
        /// <summary>None while no slide animation is in progress. SlideOut plays before the actual
        /// slot mount (old screen exiting); SlideIn plays after (new screen entering) -- see
        /// BeginSlideOut/StepSlideOut/StepSlideIn and OnSlugcatSelectMenuRebuilt below.</summary>
        internal enum TransitionPhase
        {
            None,
            SlideOut,
            SlideIn
        }

        /// <summary>How many simulation ticks (the game's fixed 40/sec rate) each slide phase
        /// takes to cross a full screen height -- ~0.75s (raised from round 2's 16/~0.4s at the
        /// user's request for "more easing": the cubic curve below needs more time on screen to
        /// actually read as eased rather than mostly happening within a couple of frames). Not
        /// exposed as a setting; adjust here if it still feels off in practice.</summary>
        private const int SlideDurationTicks = 30;

        /// <summary>How many RainWorld.Update frames Poll waits, after ApplyCharacterFix runs on
        /// the rebuilt menu, before finally dropping the black screen and starting the slide-in --
        /// see this file's header comment, CHARACTER PRESERVATION, for why. All spent behind the
        /// already-opaque black screen, so this is not perceptible as extra loading time.</summary>
        private const int RevealSettleFrames = 6;

        internal class State
        {
            /// <summary>True from the moment a switch (or a post-failure revert) begins until it
            /// finishes one way or another.</summary>
            public bool Waiting;

            /// <summary>The slot we're switching away from -- where a failed switch reverts to.</summary>
            public int LeavingSlot = -1;

            /// <summary>True once we've fallen back to reverting after the original switch's own
            /// PlayerProgression load failed -- distinguishes "waiting on the revert's own load"
            /// from "waiting on the original switch's load" so Poll knows not to request a
            /// process switch once THIS load finishes (SlugcatSelectMenu never went anywhere).</summary>
            public bool Reverting;

            /// <summary>The full-screen black cover shown for as long as Waiting is true.</summary>
            public FSprite BlackScreen;

            /// <summary>-1 until RequestMainProcessSwitch has actually been called for this
            /// switch; from then on, counts frames as a safety net (see Poll) in case
            /// OnSlugcatSelectMenuRebuilt never fires to clear the black screen on its own.</summary>
            public int FramesSinceProcessSwitchRequested = -1;

            /// <summary>None, or which slide phase is currently animating -- see TransitionPhase.</summary>
            public TransitionPhase Phase = TransitionPhase.None;

            /// <summary>True if this switch is heading INTO Primal's reserved slot (determines
            /// which edge of the screen each slide phase exits/enters from); set once, when the
            /// slide-out begins, and read again once the menu's rebuilt to pick the slide-in's
            /// starting position.</summary>
            public bool EnteringPrimal;

            /// <summary>The menu's own top-level Futile container currently being slid -- the
            /// outgoing menu's `container` during SlideOut, the freshly-rebuilt menu's `container`
            /// during SlideIn (see this file's header comment, TRANSITION round 3, for why
            /// animating this one object moves everything the menu draws). Also doubles as a
            /// staleness check (see PrimalSlotTabSwitchHook.Postfix) in case the player backs out
            /// of the menu entirely mid-transition.</summary>
            public FContainer SlidingContainer;

            /// <summary>How many ticks of the current slide phase have elapsed -- drives
            /// EaseInOutFraction; reset to 0 at the start of each phase.</summary>
            public int SlideTicksElapsed;

            /// <summary>The slot BeginSwitch should mount once SlideOut finishes crossing the
            /// screen -- set when the slide-out begins, consumed by StepSlide.</summary>
            public int PendingTargetSlot = -1;

            /// <summary>The character that was showing before this switch began -- captured by
            /// GetSelectedSlugcat when Tab is pressed, consumed by ApplyCharacterFix once the
            /// rebuilt menu exists. Null if it couldn't be read; ApplyCharacterFix then just
            /// leaves whichever character the new slot's own save data defaulted to. See this
            /// file's header comment, CHARACTER PRESERVATION.</summary>
            public SlugcatStats.Name TargetSlugcat;

            /// <summary>Non-null while Poll is running out RevealSettleFrames after
            /// ApplyCharacterFix, before finally dropping the black screen and starting the
            /// slide-in for this menu -- see this file's header comment, CHARACTER PRESERVATION.</summary>
            public Menu.SlugcatSelectMenu PendingRevealMenu;

            /// <summary>Frames elapsed since PendingRevealMenu was set; drives the same wait.</summary>
            public int PendingRevealFrames;
        }

        // There's only ever one live RainWorld per game process, but keying off it rather than
        // using a bare static State keeps this consistent with ChainsGimmick's own per-instance
        // state pattern elsewhere in this codebase, and costs nothing.
        private static readonly ConditionalWeakTable<RainWorld, State> States = new ConditionalWeakTable<RainWorld, State>();

        // See this file's header comment (GOTCHA #1) for why every one of these is resolved via
        // AccessTools instead of a plain C# member/method reference.
        private static readonly FieldInfo RainWorldProgressionField =
            AccessTools.Field(typeof(RainWorld), "progression");
        private static readonly ConstructorInfo PlayerProgressionCtor =
            AccessTools.Constructor(typeof(PlayerProgression), new[] { typeof(RainWorld), typeof(bool), typeof(bool) });
        private static readonly MethodInfo PlayerProgressionDestroyMethod =
            AccessTools.Method(typeof(PlayerProgression), "Destroy", new[] { typeof(int) });
        private static readonly PropertyInfo PlayerProgressionLoadedProperty =
            AccessTools.Property(typeof(PlayerProgression), "progressionLoaded");
        private static readonly FieldInfo PlayerProgressionLoadedResultField =
            AccessTools.Field(typeof(PlayerProgression), "progressionLoadedResult");
        private static readonly MethodInfo RequestMainProcessSwitchMethod =
            AccessTools.Method(typeof(ProcessManager), "RequestMainProcessSwitch", new[] { typeof(ProcessManager.ProcessID) });
        private static readonly PropertyInfo IsRunningAnyDialogProperty =
            AccessTools.Property(typeof(ProcessManager), "IsRunningAnyDialog");

        // Menu.container itself is new surface too (see this file's header comment, TRANSITION
        // round 3) -- FContainer.y is already safe-by-analogy to FSprite.x/y below, but the field
        // that gets us the container in the first place hasn't been read directly before.
        private static readonly FieldInfo MenuContainerField =
            AccessTools.Field(typeof(Menu.Menu), "container");

        // All new surface too (see this file's header comment, CHARACTER PRESERVATION) -- every
        // one resolved via AccessTools per GOTCHA #1, even though Menu.SlugcatSelectMenu.SlugcatPage
        // itself is trusted directly as a compile-time type (see that same header note).
        private static readonly FieldInfo SlugcatPagesField =
            AccessTools.Field(typeof(Menu.SlugcatSelectMenu), "slugcatPages");
        private static readonly FieldInfo SlugcatPageIndexField =
            AccessTools.Field(typeof(Menu.SlugcatSelectMenu), "slugcatPageIndex");
        private static readonly FieldInfo ScrollField =
            AccessTools.Field(typeof(Menu.SlugcatSelectMenu), "scroll");
        private static readonly FieldInfo LastScrollField =
            AccessTools.Field(typeof(Menu.SlugcatSelectMenu), "lastScroll");
        private static readonly FieldInfo QuedSideInputField =
            AccessTools.Field(typeof(Menu.SlugcatSelectMenu), "quedSideInput");
        private static readonly FieldInfo SlugcatPageSlugcatNumberField =
            AccessTools.Field(typeof(Menu.SlugcatSelectMenu.SlugcatPage), "slugcatNumber");
        private static readonly MethodInfo UpdateSelectedSlugcatInMiscProgMethod =
            AccessTools.Method(typeof(Menu.SlugcatSelectMenu), "UpdateSelectedSlugcatInMiscProg");

        internal static State GetState(RainWorld rainWorld)
        {
            return States.GetOrCreateValue(rainWorld);
        }

        /// <summary>Reflection-guarded read of `menu.container` (see MenuContainerField above).
        /// Returns null (and logs) if the menu is null or the field couldn't be resolved/read.</summary>
        internal static FContainer GetMenuContainer(Menu.Menu menu)
        {
            if (menu == null || MenuContainerField == null)
            {
                return null;
            }
            try
            {
                return MenuContainerField.GetValue(menu) as FContainer;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"PrimalSlotSwitcher.GetMenuContainer: reading Menu.container threw: {ex.Message}");
                return null;
            }
        }

        /// <summary>Reads whichever character is currently showing on the given menu --
        /// `slugcatPages[slugcatPageIndex].slugcatNumber` -- via the reflection fields above. See
        /// this file's header comment, CHARACTER PRESERVATION. Returns null (and logs) on any
        /// failure, in which case ApplyCharacterFix's caller just leaves the default behavior.</summary>
        internal static SlugcatStats.Name GetSelectedSlugcat(Menu.SlugcatSelectMenu menu)
        {
            if (menu == null || SlugcatPagesField == null || SlugcatPageIndexField == null || SlugcatPageSlugcatNumberField == null)
            {
                return null;
            }
            try
            {
                System.Collections.IList pages = SlugcatPagesField.GetValue(menu) as System.Collections.IList;
                object indexValue = SlugcatPageIndexField.GetValue(menu);
                if (pages == null || !(indexValue is int index) || index < 0 || index >= pages.Count)
                {
                    return null;
                }
                object page = pages[index];
                return page == null ? null : SlugcatPageSlugcatNumberField.GetValue(page) as SlugcatStats.Name;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"PrimalSlotSwitcher.GetSelectedSlugcat threw -- switching without preserving the current character: {ex.Message}");
                return null;
            }
        }

        /// <summary>Points the just-rebuilt menu at `targetSlugcat` instead of whatever its own
        /// save data defaulted `slugcatPageIndex` to -- see this file's header comment, CHARACTER
        /// PRESERVATION. No-ops (leaving the default) if `targetSlugcat` is null, the character
        /// isn't found on this menu's roster (shouldn't normally happen -- same install, same
        /// roster on every slot), or any reflection lookup fails.</summary>
        private static void ApplyCharacterFix(Menu.SlugcatSelectMenu menu, SlugcatStats.Name targetSlugcat)
        {
            if (menu == null || targetSlugcat == null)
            {
                return;
            }
            if (SlugcatPagesField == null || SlugcatPageIndexField == null || SlugcatPageSlugcatNumberField == null)
            {
                Plugin.Log?.LogWarning("PrimalSlotSwitcher.ApplyCharacterFix: couldn't resolve slugcatPages/slugcatPageIndex/slugcatNumber via reflection -- leaving whichever character the new slot defaulted to.");
                return;
            }
            try
            {
                System.Collections.IList pages = SlugcatPagesField.GetValue(menu) as System.Collections.IList;
                if (pages == null)
                {
                    return;
                }

                int foundIndex = -1;
                for (int i = 0; i < pages.Count; i++)
                {
                    object page = pages[i];
                    if (page == null)
                    {
                        continue;
                    }
                    SlugcatStats.Name candidate = SlugcatPageSlugcatNumberField.GetValue(page) as SlugcatStats.Name;
                    if (candidate != null && candidate == targetSlugcat)
                    {
                        foundIndex = i;
                        break;
                    }
                }

                if (foundIndex < 0)
                {
                    Plugin.Log?.LogWarning($"PrimalSlotSwitcher.ApplyCharacterFix: {targetSlugcat.value} not found on the rebuilt menu's roster -- leaving whichever character the new slot defaulted to.");
                    return;
                }

                SlugcatPageIndexField.SetValue(menu, foundIndex);
                ScrollField?.SetValue(menu, 0f);
                LastScrollField?.SetValue(menu, 0f);
                QuedSideInputField?.SetValue(menu, 0);
                UpdateSelectedSlugcatInMiscProgMethod?.Invoke(menu, null);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"PrimalSlotSwitcher.ApplyCharacterFix threw -- leaving whichever character the new slot defaulted to: {ex.Message}");
            }
        }

        internal static bool IsAnyDialogRunning(RainWorld rainWorld)
        {
            if (IsRunningAnyDialogProperty == null || rainWorld?.processManager == null)
            {
                return false;
            }
            try
            {
                return (bool)IsRunningAnyDialogProperty.GetValue(rainWorld.processManager, null);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"PrimalSlotSwitcher.IsAnyDialogRunning reflection failed, assuming no dialog is running: {ex.Message}");
                return false;
            }
        }

        /// <summary>Cubic ease-in-out: 0 at t=0, 1 at t=1, zero velocity at both ends, and -- unlike
        /// round 2's sinusoidal curve -- a noticeably sharper acceleration/deceleration at each end
        /// (see this file's header comment, TRANSITION round 3, "more easing"). `ticksElapsed` is
        /// clamped into [0, totalTicks] so a caller one tick past the end still gets a clean 1.0.</summary>
        private static float EaseInOutFraction(int ticksElapsed, int totalTicks)
        {
            float t = totalTicks > 0 ? Mathf.Clamp01((float)ticksElapsed / totalTicks) : 1f;
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        /// <summary>Starts the outgoing slide (see this file's header comment, TRANSITION). If
        /// `menu.container` is somehow null (defensive -- shouldn't happen for a real
        /// SlugcatSelectMenu, which gets one unconditionally from the base Menu constructor),
        /// falls straight through to BeginSwitch so the feature degrades to its pre-transition
        /// behavior instead of doing nothing.</summary>
        internal static void BeginSlideOut(RainWorld rainWorld, State state, Menu.SlugcatSelectMenu menu, int targetSlot, bool enteringPrimal, SlugcatStats.Name characterToPreserve)
        {
            // Set regardless of whether the slide itself can run below -- ApplyCharacterFix reads
            // this once the rebuilt menu exists either way. See this file's header comment,
            // CHARACTER PRESERVATION.
            state.TargetSlugcat = characterToPreserve;

            FContainer container = GetMenuContainer(menu);
            if (container == null)
            {
                Plugin.Log?.LogWarning("PrimalSlotSwitcher.BeginSlideOut: menu.container was null -- switching instantly instead.");
                BeginSwitch(rainWorld, state, targetSlot);
                return;
            }

            state.SlidingContainer = container;
            state.EnteringPrimal = enteringPrimal;
            state.PendingTargetSlot = targetSlot;
            state.SlideTicksElapsed = 0;
            state.Phase = TransitionPhase.SlideOut;
        }

        /// <summary>Advances one tick of whichever slide phase is active, nudging
        /// state.SlidingContainer.y by this tick's eased delta (the difference between the eased
        /// fraction at the previous and current tick, times the full screen height, times the
        /// direction). Moving the menu's single top-level container this way moves everything it
        /// draws -- see this file's header comment, TRANSITION round 3. `onPhaseComplete` runs
        /// once, only when this call is what pushes the phase over its duration.</summary>
        private static void StepSlide(RainWorld rainWorld, State state, Action onPhaseComplete)
        {
            if (state.SlidingContainer == null)
            {
                state.Phase = TransitionPhase.None;
                return;
            }

            float screenHeight = rainWorld.screenSize.y;
            float direction = state.EnteringPrimal ? 1f : -1f;
            float fractionBefore = EaseInOutFraction(state.SlideTicksElapsed, SlideDurationTicks);
            state.SlideTicksElapsed++;
            float fractionAfter = EaseInOutFraction(state.SlideTicksElapsed, SlideDurationTicks);
            float deltaY = (fractionAfter - fractionBefore) * screenHeight * direction;

            state.SlidingContainer.y += deltaY;

            if (state.SlideTicksElapsed >= SlideDurationTicks)
            {
                state.SlidingContainer = null;
                state.Phase = TransitionPhase.None;
                onPhaseComplete?.Invoke();
            }
        }

        /// <summary>Steps the outgoing slide; once it finishes crossing the screen, hands off to
        /// BeginSwitch -- the actual save-slot mount only starts once nothing readable is left on
        /// screen.</summary>
        internal static void StepSlideOut(RainWorld rainWorld, State state)
        {
            int targetSlot = state.PendingTargetSlot;
            StepSlide(rainWorld, state, () => BeginSwitch(rainWorld, state, targetSlot));
        }

        /// <summary>Steps the incoming slide -- started off-screen by BeginSlideIn, on whichever
        /// edge matches how StepSlideOut exited, so the whole transition reads as one continuous
        /// motion. Nothing further happens once it completes; the rebuilt menu is simply at rest.</summary>
        internal static void StepSlideIn(RainWorld rainWorld, State state)
        {
            StepSlide(rainWorld, state, null);
        }

        internal static void BeginSwitch(RainWorld rainWorld, State state, int targetSlot)
        {
            if (RainWorldProgressionField == null || PlayerProgressionCtor == null || PlayerProgressionDestroyMethod == null)
            {
                Plugin.Log?.LogError("PrimalSlotSwitcher.BeginSwitch: couldn't resolve PlayerProgression's constructor/Destroy via reflection -- aborting rather than risk a half-done switch. See this file's GOTCHA #1 note.");
                return;
            }

            state.LeavingSlot = rainWorld.options.saveSlot;
            state.Reverting = false;
            state.FramesSinceProcessSwitchRequested = -1;
            ShowBlackScreen(rainWorld, state);
            state.Waiting = true;

            try
            {
                object currentProgression = RainWorldProgressionField.GetValue(rainWorld);

                // The same three operations Menu.OptionsMenu.SetCurrentlySelectedOfSeries("SaveSlot", to)
                // performs for an ordinary slot switch -- see this file's header comment.
                rainWorld.options.saveSlot = targetSlot;
                if (currentProgression != null)
                {
                    PlayerProgressionDestroyMethod.Invoke(currentProgression, new object[] { state.LeavingSlot });
                }
                object newProgression = PlayerProgressionCtor.Invoke(new object[] { rainWorld, true, false });
                RainWorldProgressionField.SetValue(rainWorld, newProgression);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"PrimalSlotSwitcher.BeginSwitch threw -- reverting saveSlot to {state.LeavingSlot} and giving up on this Tab-press: {ex}");
                rainWorld.options.saveSlot = state.LeavingSlot;
                state.Waiting = false;
                HideBlackScreen(state);
            }
        }

        internal static void Poll(RainWorld rainWorld)
        {
            State state;
            if (!States.TryGetValue(rainWorld, out state))
            {
                return;
            }

            if (state.PendingRevealMenu != null)
            {
                // ApplyCharacterFix already ran (see OnSlugcatSelectMenuRebuilt) -- just running
                // out a short, fixed wait behind the still-up black screen. See this file's header
                // comment, CHARACTER PRESERVATION.
                state.PendingRevealFrames++;
                if (state.PendingRevealFrames >= RevealSettleFrames)
                {
                    Menu.SlugcatSelectMenu revealMenu = state.PendingRevealMenu;
                    state.PendingRevealMenu = null;
                    state.Waiting = false;
                    HideBlackScreen(state);
                    BeginSlideIn(rainWorld, state, revealMenu);
                }
                return;
            }

            if (!state.Waiting)
            {
                return;
            }

            if (state.FramesSinceProcessSwitchRequested >= 0)
            {
                // Safety net only -- normally OnSlugcatSelectMenuRebuilt clears Waiting (and the
                // black screen) the instant the rebuilt menu's constructor runs. If that somehow
                // never happens (e.g. the player quit to the desktop mid-switch instead of ever
                // landing back on SlugcatSelectMenu), don't leave a black screen up forever.
                state.FramesSinceProcessSwitchRequested++;
                if (state.FramesSinceProcessSwitchRequested > 300)
                {
                    Plugin.Log?.LogWarning("PrimalSlotSwitcher: black screen still up 5 seconds after requesting the process switch -- force-clearing it.");
                    state.Waiting = false;
                    HideBlackScreen(state);
                }
                return;
            }

            object progression = RainWorldProgressionField?.GetValue(rainWorld);
            if (progression == null || PlayerProgressionLoadedProperty == null)
            {
                return;
            }

            bool loaded;
            try
            {
                loaded = (bool)PlayerProgressionLoadedProperty.GetValue(progression, null);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"PrimalSlotSwitcher.Poll: reading progressionLoaded threw, giving up on this switch: {ex}");
                state.Waiting = false;
                HideBlackScreen(state);
                return;
            }

            if (!loaded)
            {
                return;
            }

            int result = -1;
            try
            {
                if (PlayerProgressionLoadedResultField != null)
                {
                    result = (int)PlayerProgressionLoadedResultField.GetValue(progression);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"PrimalSlotSwitcher.Poll: reading progressionLoadedResult threw, treating this load as a failure: {ex}");
            }

            // ProgressionLoadResult: 1 = SUCCESS_LOAD_EXISTING_FILE, 2 = SUCCESS_CREATE_NEW_FILE,
            // 5 = ERROR_SAVE_DATA_MISSING (treated as a normal "new/empty slot" case, exactly like
            // OptionsMenu.Update's own polling treats it) -- anything else is a real failure.
            bool success = result == 1 || result == 2 || result == 5;

            if (success)
            {
                if (state.Reverting)
                {
                    // Only reverting after a failed switch -- we're back on the original slot and
                    // SlugcatSelectMenu never went anywhere, so there's nothing left to rebuild.
                    state.Waiting = false;
                    state.Reverting = false;
                    HideBlackScreen(state);
                    return;
                }

                if (RequestMainProcessSwitchMethod != null && rainWorld.processManager != null)
                {
                    try
                    {
                        // Rebuilds the character-select screen against the newly-mounted slot.
                        // This also gives this feature a real transition "for free": it's what
                        // triggers vanilla's own black-fade-and-back
                        // (ProcessManager.InitFadeSprite/UpdateFade) around the rebuild -- our own
                        // black screen just has to survive until the rebuilt menu exists, which
                        // OnSlugcatSelectMenuRebuilt takes care of.
                        RequestMainProcessSwitchMethod.Invoke(rainWorld.processManager, new object[] { ProcessManager.ProcessID.SlugcatSelect });
                        state.FramesSinceProcessSwitchRequested = 0;
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log?.LogError($"PrimalSlotSwitcher.Poll: RequestMainProcessSwitch threw: {ex}");
                        state.Waiting = false;
                        HideBlackScreen(state);
                    }
                }
                else
                {
                    Plugin.Log?.LogError("PrimalSlotSwitcher.Poll: couldn't resolve ProcessManager.RequestMainProcessSwitch via reflection -- the new slot is mounted, but the menu won't rebuild on its own. See this file's GOTCHA #1 note.");
                    state.Waiting = false;
                    HideBlackScreen(state);
                }
            }
            else if (state.Reverting)
            {
                // Reverting itself failed -- nothing left that can safely be automated.
                Plugin.Log?.LogError($"PrimalSlotSwitcher: reverting to slot {state.LeavingSlot} after a failed switch ALSO failed (result={result}). Giving up here -- check BepInEx's log and that slot's save data by hand.");
                state.Waiting = false;
                state.Reverting = false;
                HideBlackScreen(state);
            }
            else
            {
                Plugin.Log?.LogError($"PrimalSlotSwitcher: switching to slot {rainWorld.options.saveSlot} failed (result={result}) -- reverting to slot {state.LeavingSlot}.");
                try
                {
                    object failedProgression = RainWorldProgressionField.GetValue(rainWorld);
                    rainWorld.options.saveSlot = state.LeavingSlot;
                    if (failedProgression != null)
                    {
                        PlayerProgressionDestroyMethod.Invoke(failedProgression, new object[] { 0 });
                    }
                    object revertedProgression = PlayerProgressionCtor.Invoke(new object[] { rainWorld, true, false });
                    RainWorldProgressionField.SetValue(rainWorld, revertedProgression);
                    state.Reverting = true;
                    // Waiting stays true -- Poll picks the revert's own load back up next frame.
                }
                catch (Exception ex)
                {
                    Plugin.Log?.LogError($"PrimalSlotSwitcher: reverting after a failed switch threw: {ex}");
                    state.Waiting = false;
                    HideBlackScreen(state);
                }
            }
        }

        internal static void OnSlugcatSelectMenuRebuilt(RainWorld rainWorld, Menu.SlugcatSelectMenu newMenu)
        {
            State state;
            if (!States.TryGetValue(rainWorld, out state))
            {
                return;
            }
            // Only clear if we actually requested this particular rebuild -- an ordinary
            // SlugcatSelectMenu construction unrelated to us (e.g. the player just navigating
            // there normally) shouldn't touch our state.
            if (state.FramesSinceProcessSwitchRequested >= 0)
            {
                state.FramesSinceProcessSwitchRequested = -1;
                ApplyCharacterFix(newMenu, state.TargetSlugcat);
                // Waiting stays true, and the black screen stays up, for a few more frames --
                // Poll finishes the reveal (HideBlackScreen/BeginSlideIn) once RevealSettleFrames
                // have passed. See this file's header comment, CHARACTER PRESERVATION.
                state.PendingRevealMenu = newMenu;
                state.PendingRevealFrames = 0;
            }
        }

        /// <summary>Starts the incoming slide for the just-rebuilt menu: positions its container
        /// just off the edge matching how the outgoing one exited (see this file's header comment,
        /// TRANSITION round 3) and marks it to animate back to rest. No `lastPos`-style workaround
        /// is needed here the way round 1/2 needed for `PositionedMenuObject.pos` -- `FNode.y` has
        /// no separate interpolated draw position, so setting it once takes effect immediately.</summary>
        private static void BeginSlideIn(RainWorld rainWorld, State state, Menu.SlugcatSelectMenu newMenu)
        {
            FContainer container = GetMenuContainer(newMenu);
            if (container == null)
            {
                Plugin.Log?.LogWarning("PrimalSlotSwitcher.BeginSlideIn: newMenu.container was null -- skipping the entrance animation.");
                return;
            }

            // Same direction convention as StepSlide: entering Primal means everything exited
            // upward (+screenHeight), so the incoming container starts shifted the opposite way
            // (below its natural rest position, y == 0) and gets walked back up to 0 by the eased
            // steps.
            container.y = state.EnteringPrimal ? -rainWorld.screenSize.y : rainWorld.screenSize.y;

            state.SlidingContainer = container;
            state.SlideTicksElapsed = 0;
            state.Phase = TransitionPhase.SlideIn;
        }

        private static void ShowBlackScreen(RainWorld rainWorld, State state)
        {
            if (state.BlackScreen != null)
            {
                return;
            }
            try
            {
                // Same atlas element and constructor overload vanilla's own
                // ProcessManager.InitFadeSprite uses for its own full-screen fade cover (confirmed
                // by disassembling that method, not guessed) -- "Futile_White" with the quadType
                // constructor argument set to true is what lets a single FSprite reliably cover
                // the whole screen regardless of resolution, with no explicit scaleX/scaleY needed.
                // Unlike vanilla's fade (which animates alpha in over time via UpdateFade), this
                // one is just held fully opaque for as long as we're waiting on PlayerProgression
                // to finish mounting -- a hard "black screen while loading", per what was asked
                // for, rather than a smooth fade (RequestMainProcessSwitch below supplies that
                // part, for the rebuild itself, using vanilla's own fade).
                FSprite blackScreen = new FSprite("Futile_White", true)
                {
                    color = new Color(0f, 0f, 0f),
                    x = rainWorld.screenSize.x / 2f,
                    y = rainWorld.screenSize.y / 2f,
                    alpha = 1f
                };
                Futile.stage.AddChild(blackScreen);
                state.BlackScreen = blackScreen;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"PrimalSlotSwitcher.ShowBlackScreen threw -- continuing the slot switch without a visible loading cover: {ex}");
            }
        }

        private static void HideBlackScreen(State state)
        {
            if (state.BlackScreen == null)
            {
                return;
            }
            try
            {
                state.BlackScreen.RemoveFromContainer();
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"PrimalSlotSwitcher.HideBlackScreen threw while removing the cover sprite: {ex.Message}");
            }
            finally
            {
                state.BlackScreen = null;
            }
        }
    }
}
