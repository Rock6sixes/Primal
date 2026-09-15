using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreSlugcats;

namespace Primal.Gimmicks
{
    /// <summary>
    /// While playing on this mod's reserved alt slot (PrimalSlot.ReservedSlot), every character
    /// shows up on the "choose your character" screen as "PRIMAL &lt;NAME&gt;" instead of their
    /// vanilla name. Gourmand, Spearmaster, Sofanthiel, and Artificer -- the four this mod
    /// currently has real content for -- get a written intro line plus a description that lists
    /// whichever of this mod's gimmicks are currently assigned to them (see GimmickAssignments;
    /// Sofanthiel has no gimmick of its own as of this writing, so its description will just read
    /// "No primal traits assigned yet"). Every other character (Survivor, Monk, Hunter, Rivulet,
    /// Saint, Watcher -- added 2026-09-15) gets the same "PRIMAL X" name treatment but a plain,
    /// static description: "Primal version not implemented". Off the alt slot, every character is
    /// shown completely unmodified -- this only ever touches how they're *labeled* on this one
    /// screen; nothing about SlugcatStats.Name, StoryGameSession identity, or actual gameplay
    /// changes because of it (same "no faked identity" rule the gimmicks themselves follow -- see
    /// PrimalSlot.cs).
    ///
    /// Only Menu.SlugcatSelectMenu.SlugcatPageNewGame shows a name/description at all (the
    /// "continue" page for a character that already has save data on this slot,
    /// SlugcatPageContinue, shows cycle/karma/time stats instead and has no equivalent labels)
    /// -- so this is a Postfix on that page's constructor, which is where vanilla itself sets
    /// difficultyLabel.text/infoLabel.text based on which vanilla character is being shown
    /// (see reference/GameSource/Menu/SlugcatSelectMenu.cs's SlugcatPageNewGame class).
    ///
    /// difficultyLabel/infoLabel are read via reflection (AccessTools.Field), not a direct
    /// `__instance.difficultyLabel` field access -- see AI_CONTEXT.md's Gotcha #1. The compile-
    /// time reference assembly is a "publicized" copy of the game's real one (every member forced
    /// public), so a field that shows up as `public` in that reference, or in the decompiled
    /// reference/GameSource/ tree (which is decompiled from that same publicized assembly, so it
    /// carries the exact same false "public" markings -- confirmed by checking
    /// PlayerGraphics.player there, which Gotcha #1 already proved is non-public in the real
    /// game, and which still shows as `public` in reference/GameSource/PlayerGraphics.cs), can
    /// still be genuinely private/internal in the actual assembly running on the user's machine,
    /// and a direct field access that compiles fine here would then throw FieldAccessException at
    /// runtime -- exactly what happened the first time around with PlayerGraphics.player.
    /// AccessTools.Field's reflection-based GetValue bypasses that JIT-level accessibility check
    /// entirely, so it's used here defensively even though these two fields are very likely
    /// genuinely public in practice (MenuLabel is Rain World's ordinary, widely-reused menu-label
    /// class, and SlugcatPageNewGame is not a tightly-encapsulated simulation object the way
    /// Player/PlayerGraphics are) -- better to pay the small reflection cost once than repeat the
    /// Gotcha #1 investigation on a screen this container's tools can't even open to check.
    /// Setting `.text` on the MenuLabel we get back, once we have a properly-typed reference to
    /// it, is a normal property call (MenuLabel.text is a plain public property used identically,
    /// the exact same way, throughout dozens of unrelated vanilla menu classes -- see
    /// reference/GameSource/Menu/MenuLabel.cs), not a field access, so that part is safe as
    /// ordinary compiled code.
    ///
    /// **Not yet tested in-game as of this doc** -- see AI_CONTEXT.md's Outstanding items.
    /// </summary>
    [HarmonyPatch(typeof(Menu.SlugcatSelectMenu.SlugcatPageNewGame), MethodType.Constructor, typeof(Menu.Menu), typeof(Menu.MenuObject), typeof(int), typeof(SlugcatStats.Name))]
    internal static class PrimalCharacterNameHook
    {
        // 2026-09-15: added for the six characters with no Primal-specific writeup (Survivor, Monk,
        // Hunter, Rivulet, Saint, Watcher) -- a plain, static description, deliberately not run
        // through BuildDescription (no "Active traits" line): these characters have no gimmicks of
        // their own to report, and the request was for this exact literal text.
        private const string NotImplementedDescription = "Primal version not implemented";

        private static readonly FieldInfo DifficultyLabelField =
            AccessTools.Field(typeof(Menu.SlugcatSelectMenu.SlugcatPageNewGame), "difficultyLabel");

        private static readonly FieldInfo InfoLabelField =
            AccessTools.Field(typeof(Menu.SlugcatSelectMenu.SlugcatPageNewGame), "infoLabel");

        private static void Postfix(Menu.SlugcatSelectMenu.SlugcatPageNewGame __instance, Menu.Menu menu, SlugcatStats.Name slugcatNumber)
        {
            if (!PrimalSlot.IsActive(menu.manager.rainWorld))
            {
                return;
            }

            string name;
            string description;

            if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                name = "PRIMAL GOURMAND";
                description = BuildDescription(
                    slugcatNumber.value,
                    "A glutton reduced to the simplest pleasures of life. Capable only of sluggish rolling, one must seek out their home to regain some semblance of power.");
            }
            else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                name = "PRIMAL SPEARMASTER";
                description = BuildDescription(
                    slugcatNumber.value,
                    "A deteriorating abnormality with its crucial needles running out. A traveller from a very far away land; A feeling in your hollow depths rushes you, messenger...");
            }
            else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
            {
                // Added 2026-09-15 per user rewrite -- vanilla's own name for this character is
                // "???" rather than "THE X", so the Primal label follows that same convention
                // instead of forcing a "PRIMAL SOFANTHIEL" that vanilla itself never uses.
                name = "PRIMAL ???";
                description = BuildDescription(
                    slugcatNumber.value,
                    "Thanks Rockman.");
            }
            else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                // Added 2026-09-12 once Artificer got its own default gimmick (Chains) -- see
                // GimmickDefaults.cs.
                name = "PRIMAL ARTIFICER";
                description = BuildDescription(
                    slugcatNumber.value,
                    "A violent combatant, master of explosives, cursed to seek out any prey in sight. A journey of endless bloodshed and warfare.");
            }
            else if (slugcatNumber == SlugcatStats.Name.White)
            {
                name = "PRIMAL SURVIVOR";
                description = NotImplementedDescription;
            }
            else if (slugcatNumber == SlugcatStats.Name.Yellow)
            {
                name = "PRIMAL MONK";
                description = NotImplementedDescription;
            }
            else if (slugcatNumber == SlugcatStats.Name.Red)
            {
                name = "PRIMAL HUNTER";
                description = NotImplementedDescription;
            }
            else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
            {
                name = "PRIMAL RIVULET";
                description = NotImplementedDescription;
            }
            else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                name = "PRIMAL SAINT";
                description = NotImplementedDescription;
            }
            else if (ModManager.Watcher && slugcatNumber == Watcher.WatcherEnums.SlugcatStatsName.Watcher)
            {
                name = "PRIMAL WATCHER";
                description = NotImplementedDescription;
            }
            else
            {
                // Every character this mod actually has something to say about is handled by one
                // of the branches above. Anything else (a future DLC character not accounted for
                // yet, say) keeps its ordinary vanilla name/description rather than guessing at a
                // "Primal" label for it.
                return;
            }

            if (DifficultyLabelField?.GetValue(__instance) is Menu.MenuLabel difficultyLabel)
            {
                difficultyLabel.text = name;
            }

            if (InfoLabelField?.GetValue(__instance) is Menu.MenuLabel infoLabel)
            {
                infoLabel.text = description;
            }
        }

        /// <summary>Intro line + a line naming whichever of this mod's gimmicks are currently
        /// assigned to this character (GimmickAssignments), so the description on this screen
        /// always reflects live Remix state instead of a hardcoded snapshot of today's defaults.
        /// </summary>
        private static string BuildDescription(string characterID, string introLine)
        {
            var activeNames = new List<string>();
            foreach (GimmickDef gimmick in GimmickRegistry.All)
            {
                if (GimmickAssignments.IsEnabled(characterID, gimmick.ID))
                {
                    activeNames.Add(gimmick.DisplayName);
                }
            }

            string traitsLine = (activeNames.Count > 0)
                ? "Active traits: " + string.Join(", ", activeNames) + "."
                : "No primal traits assigned yet -- set some in this mod's Remix options.";

            return introLine + Environment.NewLine + traitsLine;
        }
    }
}
