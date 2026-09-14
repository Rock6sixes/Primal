using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreSlugcats;

namespace Primal.Gimmicks
{
    /// <summary>
    /// While playing on this mod's reserved alt slot (PrimalSlot.ReservedSlot), Gourmand and
    /// Spearmaster show up on the "choose your character" screen as "PRIMAL GOURMAND" /
    /// "PRIMAL SPEARMASTER" instead of their vanilla names, with a description that lists
    /// whichever of this mod's gimmicks are currently assigned to them (see GimmickAssignments).
    /// Off the alt slot, both characters are shown completely unmodified -- this only ever
    /// touches how they're *labeled* on this one screen; nothing about SlugcatStats.Name,
    /// StoryGameSession identity, or actual gameplay changes because of it (same "no faked
    /// identity" rule the gimmicks themselves follow -- see PrimalSlot.cs).
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
                    "This Gourmand has been changed by something primal -- their old habits no longer tell the whole story.");
            }
            else if (slugcatNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                name = "PRIMAL SPEARMASTER";
                description = BuildDescription(
                    slugcatNumber.value,
                    "This Spearmaster has been changed by something primal -- their old habits no longer tell the whole story.");
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
            else
            {
                // Every other character (Survivor, Monk, Hunter, Rivulet, Saint, Watcher) keeps
                // its ordinary vanilla name/description on the alt slot too -- only Gourmand,
                // Spearmaster, and Artificer have gimmicks assigned by default, and only they were
                // asked for here. If a gimmick is ever added for another character, extend this
                // switch rather than assuming every character should get a "Primal" name.
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
