using System.Collections.Generic;
using HarmonyLib;
using MoreSlugcats;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Kill List" gimmick: once assigned to a character, shows a cycle-end popup checking off
    /// a fixed, curated roster of 55 tracked species against every one that character has
    /// personally killed. For now this only tracks progress -- there's no win condition or reward
    /// yet, per the user's own framing ("for now the goal will be to track every creature"). See
    /// KillListRegistry.All below for the full roster, and AI_CONTEXT.md's "Kill List gimmick, in
    /// detail" for how it was assembled (curated by the user from every CreatureTemplate.Type this
    /// game registers, cross-checked against the game's own in-game display-name strings for
    /// anything ambiguous) and exactly what was deliberately left off it.
    ///
    /// **2026-09-16: the actual kill RECORDING moved to PrimalKillTracker.cs, and is now a global,
    /// mod-wide mechanism** -- every kill a Primal-slot character personally lands is tracked
    /// unconditionally, regardless of which gimmicks (if any) that character has assigned. The
    /// user's own framing for why: "the kill list gimmick is supposed to be more for forcing/
    /// rewarding the character for doing all of these kills... the tracker, which is now needed
    /// across multiple gimmicks, should be moved out as a global effect." This file is now purely
    /// a CONSUMER of that global record: its own curated 55-species roster (KillListRegistry.All,
    /// unchanged), and its own cycle-end button + popup (KillListButtonHook/KillListSignalHook/
    /// KillListDialog, unchanged in behavior) -- both still gated on THIS gimmick's own assignment
    /// (GimmickAssignments.IsEnabled(characterID, ID)), same as before. Only the underlying
    /// "did this character kill species X" data source changed, from this file's own
    /// KillListTracker to the new global PrimalKillTracker -- see that file's own header comment
    /// for the full trace of what moved, what changed, and why (gating relaxed to any Primal-slot
    /// character; the curated-roster restriction dropped from the recording step entirely, so a
    /// tracker like Ranger can ask about ANY creature type, not just this gimmick's own 55).
    /// </summary>
    internal static class CreatureKillListGimmick
    {
        public const string ID = "CreatureKillList";
    }

    /// <summary>One entry in the fixed, curated roster this gimmick tracks -- see
    /// CreatureKillListGimmick's own header comment for where the list came from and
    /// AI_CONTEXT.md for the full account of every name that needed disambiguating.</summary>
    internal readonly struct KillListEntry
    {
        public readonly string ID;
        public readonly string DisplayName;

        // Kept alongside the string ID (rather than reconstructed from it via `new
        // CreatureTemplate.Type(ID)` when needed) so the Kill List popup's icon grid can hand this
        // straight to IconSymbol.IconSymbolData/CreatureSymbol.SpriteNameOfCreature without any
        // ExtEnum round-tripping -- see KillListDialog below.
        public readonly CreatureTemplate.Type Type;

        public KillListEntry(CreatureTemplate.Type type, string displayName)
        {
            ID = type?.value;
            DisplayName = displayName;
            Type = type;
        }
    }

    internal static class KillListRegistry
    {
        // 55 entries, curated by the user from the full set of CreatureTemplate.Type values this
        // game registers (92 total across vanilla + every DLC) -- see AI_CONTEXT.md for the full
        // back-and-forth on what got matched to what and why. Deliberately excludes: the three
        // internal-only "ancestor" types (Slugcat, LizardTemplate, StandardGroundCreature) that no
        // real creature is ever actually spawned as; a handful of MSC/DLC-shared types not on the
        // user's list (HunterDaddy, FireBug, TrainLizard, TerrorLongLegs, BigJelly); Yeek and
        // SlugNPC ("Slugpup"), both originally included then explicitly removed 2026-09-15; and
        // nearly all of Watcher's own roster, none of which was requested. Easy to extend later --
        // just add another entry below plus a corresponding KillListRegistry.All array slot.
        //
        // Display names mostly match the user's own preferred community terms (e.g. "Dropwig",
        // "Leviathan", "Guardian") rather than the internal CreatureTemplate.Type identifier or
        // even the game's own official translated name where the two differ (e.g. official name
        // for SpitLizard is "Caramel Lizard" -- matches here; official name for RotLoach is "Rot
        // Behemoth" -- not on this list at all, different creature).
        public static readonly KillListEntry[] All =
        {
            // Vanilla (44)
            new KillListEntry(CreatureTemplate.Type.GreenLizard, "Green Lizard"),
            new KillListEntry(CreatureTemplate.Type.PinkLizard, "Pink Lizard"),
            new KillListEntry(CreatureTemplate.Type.WhiteLizard, "White Lizard"),
            new KillListEntry(CreatureTemplate.Type.BlueLizard, "Blue Lizard"),
            new KillListEntry(CreatureTemplate.Type.YellowLizard, "Yellow Lizard"),
            new KillListEntry(CreatureTemplate.Type.RedLizard, "Red Lizard"),
            new KillListEntry(CreatureTemplate.Type.BlackLizard, "Black Lizard"),
            new KillListEntry(CreatureTemplate.Type.CyanLizard, "Cyan Lizard"),
            new KillListEntry(CreatureTemplate.Type.Salamander, "Salamander"),
            new KillListEntry(CreatureTemplate.Type.Vulture, "Vulture"),
            new KillListEntry(CreatureTemplate.Type.KingVulture, "King Vulture"),
            new KillListEntry(CreatureTemplate.Type.MirosBird, "Miros Bird"),
            new KillListEntry(CreatureTemplate.Type.SmallCentipede, "Small Centipede"),
            new KillListEntry(CreatureTemplate.Type.Centipede, "Centipede"),
            new KillListEntry(CreatureTemplate.Type.RedCentipede, "Red Centipede"),
            new KillListEntry(CreatureTemplate.Type.Centiwing, "Centiwing"),
            new KillListEntry(CreatureTemplate.Type.Spider, "Small Spider"),
            new KillListEntry(CreatureTemplate.Type.BigSpider, "Big Spider"),
            new KillListEntry(CreatureTemplate.Type.SpitterSpider, "Spitter Spider"),
            new KillListEntry(CreatureTemplate.Type.DropBug, "Dropwig"),
            new KillListEntry(CreatureTemplate.Type.BrotherLongLegs, "Brother Long Legs"),
            new KillListEntry(CreatureTemplate.Type.DaddyLongLegs, "Daddy Long Legs"),
            new KillListEntry(CreatureTemplate.Type.PoleMimic, "Pole Plant"),
            new KillListEntry(CreatureTemplate.Type.TentaclePlant, "Monster Kelp"),
            new KillListEntry(CreatureTemplate.Type.BigNeedleWorm, "Adult Noodlefly"),
            new KillListEntry(CreatureTemplate.Type.SmallNeedleWorm, "Infant Noodlefly"),
            new KillListEntry(CreatureTemplate.Type.Snail, "Snail"),
            new KillListEntry(CreatureTemplate.Type.Scavenger, "Scavenger"),
            new KillListEntry(CreatureTemplate.Type.EggBug, "Eggbug"),
            new KillListEntry(CreatureTemplate.Type.Leech, "Red Leech"),
            new KillListEntry(CreatureTemplate.Type.SeaLeech, "Sea Leech"),
            new KillListEntry(CreatureTemplate.Type.TubeWorm, "Grapple Worm"),
            new KillListEntry(CreatureTemplate.Type.Overseer, "Overseer"),
            new KillListEntry(CreatureTemplate.Type.Fly, "Batfly"),
            new KillListEntry(CreatureTemplate.Type.VultureGrub, "Vulture Grub"),
            new KillListEntry(CreatureTemplate.Type.LanternMouse, "Lantern Mouse"),
            new KillListEntry(CreatureTemplate.Type.Hazer, "Hazer"),
            new KillListEntry(CreatureTemplate.Type.TempleGuard, "Guardian"),
            new KillListEntry(CreatureTemplate.Type.JetFish, "Jetfish"),
            new KillListEntry(CreatureTemplate.Type.GarbageWorm, "Garbage Worm"),
            new KillListEntry(CreatureTemplate.Type.Deer, "Rain Deer"),
            new KillListEntry(CreatureTemplate.Type.BigEel, "Leviathan"),
            new KillListEntry(CreatureTemplate.Type.CicadaA, "White Squid"),
            new KillListEntry(CreatureTemplate.Type.CicadaB, "Black Squid"),

            // DLC-shared (10)
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.ScavengerElite, "Elite Scavenger"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.MotherSpider, "Mother Spider"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.MirosVulture, "Miros Vulture"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.StowawayBug, "Stowaway"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.Inspector, "Inspector"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.JungleLeech, "Jungle Leech"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.EelLizard, "Eel Lizard"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.SpitLizard, "Caramel Lizard"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.ZoopLizard, "Strawberry Lizard"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.AquaCenti, "Coalescipede"),

            // MSC (1)
            new KillListEntry(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, "Chieftain Scavenger"),
        };
    }

    /// <summary>
    /// Adds a "KILL LIST" button to the cycle-end screen (Menu.SleepAndDeathScreen, shown after
    /// both a successful sleep and a death) for whichever character has this gimmick assigned --
    /// opens a popup listing every tracked species, not-yet-killed first, then a separator, then
    /// killed. Requested explicitly in place of a Remix-menu settings view: "for showing the
    /// progress, dont show it in the settings -- put a button in the cycle ending screen."
    ///
    /// Reuses Menu.SleepAndDeathScreen.AddSubObjects (Postfix, runs once per screen after every
    /// one of its own buttons -- Continue, Exit, Passage -- already exist) to add the button, and
    /// Menu.SleepAndDeathScreen.Singal (Postfix) to react to its click -- the same signal-string
    /// dispatch mechanism every other vanilla button on this screen already uses (confirmed via
    /// decompilation: SleepAndDeathScreen.Singal itself starts with "base.Singal(sender,
    /// message);" then branches on message for its own extra cases, e.g. "PASSAGE" -- a Postfix
    /// here runs after all of that regardless of which branch fired, so it can't interfere with
    /// any real vanilla signal). `saveState` (KarmaLadderScreen) is read the same reflected way
    /// NoPassagesGimmick.cs already established for this exact screen; `manager`/`rainWorld` and
    /// `ContinueAndExitButtonsXPos`/`SafeScreenOffset` (both plain public, used to compute this
    /// button's position the same way vanilla computes exitButton's own, rather than reading
    /// exitButton.pos directly -- one fewer new member access) are trusted directly, matching this
    /// same file's existing precedent.
    ///
    /// The popup itself is now `KillListDialog` (see that class, below `KillListSignalHook` in this
    /// same file) -- a 6-column icon+name grid. **This was originally a plain `Menu.DialogNotify`
    /// with names comma-joined into two paragraphs of text**; that first pass is gone as of
    /// 2026-09-15 (later session), replaced after a screenshot showed it rendering with text running
    /// off both edges of the screen (`Menu.DialogBoxNotify.CalculateDialogBoxSize` wraps at a fixed
    /// width with no regard for actual screen size) *and*, separately, having no way to close it at
    /// all (the specific `DialogNotify` constructor overload used never calls the one method that
    /// builds its OK button -- see `KillListDialog`'s own doc comment for the full trace of both
    /// bugs). If a future session finds this comment out of date, `KillListDialog` is the current
    /// source of truth for how the popup is actually built.
    /// </summary>
    [HarmonyPatch(typeof(Menu.SleepAndDeathScreen), nameof(Menu.SleepAndDeathScreen.AddSubObjects))]
    internal static class KillListButtonHook
    {
        internal const string SignalText = "PRIMAL_KILLLIST";

        private static void Postfix(Menu.SleepAndDeathScreen __instance)
        {
            RainWorld rainWorld = __instance.manager?.rainWorld;
            string characterID = PrimalMenuAccess.GetCharacterID(__instance);
            if (!GimmickGate.IsEnabledForCharacter(rainWorld, characterID, CreatureKillListGimmick.ID))
            {
                return;
            }

            // Same X/Y formula vanilla's own AddSubObjects already used for exitButton just above
            // this Postfix fires, stacked one row higher -- deliberately recomputed rather than
            // reading exitButton.pos back, so this needs no new field access beyond the two plain
            // public members (ContinueAndExitButtonsXPos, SafeScreenOffset) vanilla's own method
            // already relies on in this exact context.
            float x = __instance.ContinueAndExitButtonsXPos - 320f - rainWorld.options.SafeScreenOffset.x;
            float y = Mathf.Max(rainWorld.options.SafeScreenOffset.y, 15f) + 40f;

            var button = new Menu.SimpleButton(__instance, __instance.pages[0], "KILL LIST", SignalText, new Vector2(x, y), new Vector2(110f, 30f));
            __instance.pages[0].subObjects.Add(button);
        }
    }

    [HarmonyPatch(typeof(Menu.SleepAndDeathScreen), nameof(Menu.SleepAndDeathScreen.Singal))]
    internal static class KillListSignalHook
    {
        private static void Postfix(Menu.SleepAndDeathScreen __instance, Menu.MenuObject sender, string message)
        {
            if (message != KillListButtonHook.SignalText)
            {
                return;
            }

            SaveState saveState = PrimalMenuAccess.GetSaveState(__instance);
            string characterID = saveState?.saveStateNumber?.value;
            if (characterID == null)
            {
                return;
            }

            ShowKillListDialog(__instance.manager, characterID, saveState.deathPersistentSaveData);
        }

        private static void ShowKillListDialog(ProcessManager manager, string characterID, DeathPersistentSaveData saveData)
        {
            if (manager == null)
            {
                return;
            }

            manager.ShowDialog(new KillListDialog(manager, characterID, saveData));
        }
    }

    /// <summary>
    /// The Kill List popup itself (rewritten 2026-09-15, later session -- requested directly:
    /// "instead of this mess, lets do 6 columns of an icon followed by a name", after a screenshot
    /// showed the original plain-text version rendering with names running off both edges of the
    /// screen). Replaces Menu.DialogNotify with a proper grid of creature icons + names.
    ///
    /// Two distinct problems, both confirmed by reading the decompiled framework source rather than
    /// guessed:
    ///  1. Menu.DialogBoxNotify.CalculateDialogBoxSize (what the old code used to size the box) wraps
    ///     a single block of text at a FIXED internal width with no awareness of actual screen size
    ///     at all -- a long enough comma-joined list (up to 55 names) produced a box wider than the
    ///     visible screen, which is exactly the "mess" in the screenshot.
    ///  2. The specific constructor overload the old code called --
    ///     `Menu.DialogNotify(string, Vector2, ProcessManager)` -- is the ONE DialogNotify overload
    ///     that never calls `Init(Action onOK)`, which is the only thing that actually builds
    ///     `okButton` (confirmed in `reference/GameSource/Menu/DialogNotify.cs`: every OTHER overload
    ///     that takes an `onOK` callback calls `Init(onOK)`; this one just runs the base constructor
    ///     and stops). So the old popup never had a close button at all -- confirming exactly the
    ///     "not sure if there is a close button here" concern directly.
    ///
    /// Built as a `Menu.Dialog` subclass -- the same `ProcessManager.ShowDialog`/`StopSideProcess`
    /// side-process mechanism `DialogNotify` itself already used, and what every vanilla dialog in
    /// the game uses -- rather than a custom `Page` bolted onto `SleepAndDeathScreen` directly. This
    /// keeps the full-screen dark backdrop, centered rounded panel, and Continue/Exit-blocking
    /// behavior the base class already provides "for free" (all already proven, shipped vanilla
    /// behavior) and only replaces what's drawn inside it. Passing `string.Empty` as the description
    /// to the `Dialog(string, Vector2, ProcessManager)` base constructor still builds an (empty,
    /// invisible) `descriptionLabel` as a side effect -- harmless, not worth a special code path to
    /// avoid.
    ///
    /// Icons are vanilla's own "Kill_&lt;creature&gt;" sprites, looked up via
    /// `CreatureSymbol.SpriteNameOfCreature`/`ColorOfCreature` given an `IconSymbol.IconSymbolData` --
    /// the exact same lookup vanilla's own `KillsDisplayer` (`reference/GameSource/Menu
    /// /KillsDisplayer.cs`, the small icon+count row vanilla shows elsewhere for a cycle's kills)
    /// already uses, so all 55 tracked species already have real, existing icon art with no new
    /// assets needed. `KillListEntry` now keeps the original `CreatureTemplate.Type` object alongside
    /// its string `ID` specifically so this lookup needs no ExtEnum round-trip through the string.
    /// Not-yet-killed entries render dimmed (vanilla's own MediumGrey, both icon and name, the same
    /// helper `CreatureSymbol.ColorOfCreature`'s own unknown-creature fallback uses) rather than in
    /// their real color, so overall progress reads at a glance; a killed entry "reveals" its real
    /// color. `FSprite`/`Menu.MenuLabel`/`Menu.RoundedRect`/`Menu.SimpleButton` are all used directly
    /// (no reflection) -- ordinary, widely-reused vanilla menu/graphics classes, same judgment call
    /// already established throughout this file and `PrimalCharacterNames.cs` for this category of
    /// member.
    ///
    /// Sizing is computed once (`ComputeSize`, a static method so it can run before the base
    /// constructor -- and therefore `this` -- exists) from the same row-count formula the layout
    /// code itself uses (`SectionRowsHeight`), so the two can't silently drift apart the way a
    /// separately-guessed constant could. At the current 6-column/55-entry worst case (every species
    /// still in one bucket) this comes out to roughly 1300x650 -- comfortably inside the 1366x768
    /// virtual canvas every `Menu.Dialog` is centered against (confirmed in `Dialog.Init`, which
    /// hardcodes exactly those two numbers regardless of the player's actual screen resolution), so
    /// no scroll support was added for this pass.
    ///
    /// **2026-09-16:** data source switched from this file's own (now-removed) KillListTracker to
    /// the new global PrimalKillTracker.cs -- see that file's own header comment. No behavior
    /// change to this dialog itself; it still only ever shows for a character with Kill List
    /// assigned (KillListButtonHook's own gate), still only against this file's own 55-species
    /// KillListRegistry.All.
    ///
    /// **Not yet built or tested in-game as of this doc** -- next: `dotnet build`, then open the Kill
    /// List popup (any character with the gimmick assigned, from the cycle-end screen) and confirm
    /// (1) it compiles and opens without an error screen; (2) the grid renders fully on screen with
    /// no text running off either edge, at both extremes (a fresh character with 0 kills, and one
    /// with several banked) plus something in between; (3) each icon matches its own name and looks
    /// like the right creature; (4) not-yet-killed entries render visibly dimmed/gray, killed ones in
    /// their real color; (5) the CLOSE button is visible, clickable, and actually dismisses the
    /// dialog and returns control to the underlying Continue/Exit buttons.
    /// </summary>
    internal sealed class KillListDialog : Menu.Dialog
    {
        private const int Columns = 6;
        private const float ColumnWidth = 210f;
        private const float RowHeight = 40f;
        private const float IconSize = 32f;
        private const float IconLabelGap = 6f;
        private const float Margin = 24f;
        private const float TitleHeight = 34f;
        private const float SectionLabelHeight = 26f;
        private const float SectionGap = 14f;
        private const float CloseButtonAreaHeight = 60f;
        private const string CloseSignal = "PRIMAL_KILLLIST_CLOSE";

        public KillListDialog(ProcessManager manager, string characterID, DeathPersistentSaveData saveData)
            : base(string.Empty, ComputeSize(saveData, characterID), manager)
        {
            var notKilled = new List<KillListEntry>();
            var killed = new List<KillListEntry>();
            foreach (KillListEntry entry in KillListRegistry.All)
            {
                if (PrimalKillTracker.IsKilled(saveData, characterID, entry.ID))
                {
                    killed.Add(entry);
                }
                else
                {
                    notKilled.Add(entry);
                }
            }

            float cursorY = pos.y + size.y - Margin;

            var title = new Menu.MenuLabel(this, pages[0], $"KILL LIST -- {killed.Count}/{KillListRegistry.All.Length}", new Vector2(pos.x, cursorY - TitleHeight), new Vector2(size.x, TitleHeight), true);
            pages[0].subObjects.Add(title);
            cursorY -= TitleHeight;

            cursorY = RenderSection("NOT YET KILLED:", notKilled, "(none -- every tracked species killed!)", killedSection: false, cursorY);
            cursorY -= SectionGap;
            cursorY = RenderSection("KILLED:", killed, "(none yet)", killedSection: true, cursorY);

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

        private float RenderSection(string header, List<KillListEntry> entries, string emptyText, bool killedSection, float topY)
        {
            // MenuLabel.GrafUpdate always places the underlying FLabel at the CENTER of this
            // label's own pos/size rect (pos + size / 2) regardless of `alignment` -- alignment only
            // picks which direction the text extends FROM that anchor (Left = rightward from it,
            // Center = symmetric around it, Right = leftward from it). Leaving this at the
            // constructor's own default (Center) is what actually centers the header the same way
            // the CLOSE button's own label is centered -- setting Left here (the previous version)
            // anchored the text at the box's horizontal middle and then grew it rightward from
            // there, landing well right of true center instead of on it.
            var headerLabel = new Menu.MenuLabel(this, pages[0], header, new Vector2(pos.x + Margin, topY - SectionLabelHeight), new Vector2(size.x - Margin * 2f, SectionLabelHeight), false);
            pages[0].subObjects.Add(headerLabel);
            topY -= SectionLabelHeight;

            if (entries.Count == 0)
            {
                var placeholder = new Menu.MenuLabel(this, pages[0], emptyText, new Vector2(pos.x + Margin, topY - RowHeight), new Vector2(size.x - Margin * 2f, RowHeight), false);
                placeholder.label.alignment = FLabelAlignment.Left;
                pages[0].subObjects.Add(placeholder);
                return topY - RowHeight;
            }

            int rows = Mathf.CeilToInt(entries.Count / (float)Columns);
            for (int i = 0; i < entries.Count; i++)
            {
                int row = i / Columns;
                int col = i % Columns;
                float cellX = pos.x + Margin + col * ColumnWidth;
                float cellCenterY = topY - row * RowHeight - RowHeight * 0.5f;
                RenderEntry(entries[i], killedSection, cellX, cellCenterY);
            }
            return topY - rows * RowHeight;
        }

        private void RenderEntry(KillListEntry entry, bool killed, float cellX, float cellCenterY)
        {
            var iconData = new IconSymbol.IconSymbolData(entry.Type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
            Color color = killed ? CreatureSymbol.ColorOfCreature(iconData) : Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

            var icon = new FSprite(CreatureSymbol.SpriteNameOfCreature(iconData))
            {
                color = color,
                x = cellX + IconSize * 0.5f,
                y = cellCenterY
            };
            pages[0].Container.AddChild(icon);

            // Same MenuLabel anchor-is-always-the-box-center behavior as the section headers above
            // -- with the previous non-zero size.x, Left alignment anchored at the column's own
            // horizontal middle and grew the text rightward from there, landing well right of the
            // icon instead of right next to it. A zero-width box puts the anchor at exactly pos.x,
            // so Left-aligned text now genuinely starts right where the icon ends.
            var label = new Menu.MenuLabel(this, pages[0], entry.DisplayName, new Vector2(cellX + IconSize + IconLabelGap, cellCenterY - 10f), new Vector2(0f, 20f), false);
            label.label.alignment = FLabelAlignment.Left;
            label.label.color = color;
            pages[0].subObjects.Add(label);
        }

        private static Vector2 ComputeSize(DeathPersistentSaveData saveData, string characterID)
        {
            int killedCount = 0;
            foreach (KillListEntry entry in KillListRegistry.All)
            {
                if (PrimalKillTracker.IsKilled(saveData, characterID, entry.ID))
                {
                    killedCount++;
                }
            }
            int notKilledCount = KillListRegistry.All.Length - killedCount;

            float width = Columns * ColumnWidth + Margin * 2f;
            float height = Margin * 2f + TitleHeight + CloseButtonAreaHeight
                + SectionRowsHeight(notKilledCount) + SectionGap + SectionRowsHeight(killedCount);
            return new Vector2(width, height);
        }

        private static float SectionRowsHeight(int count)
        {
            int rows = count > 0 ? Mathf.CeilToInt(count / (float)Columns) : 1;
            return SectionLabelHeight + rows * RowHeight;
        }
    }
}
