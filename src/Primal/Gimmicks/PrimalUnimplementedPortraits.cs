using System;
using HarmonyLib;
using MoreSlugcats;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Companion to PrimalCharacterNames.cs: on this mod's reserved alt slot, the six characters
    /// labeled "Primal version not implemented" (Survivor, Monk, Hunter, Rivulet, Saint, Watcher --
    /// see PrimalCharacterNameHook) also get their character-select portrait blacked out, so the
    /// "not implemented" status reads visually, not just in the description text.
    ///
    /// **How the portrait is actually loaded.** Per reference/GameSource/Menu/SlugcatSelectMenu.cs,
    /// the select screen builds a live `InteractiveMenuScene` (a `MenuScene` subclass) for the
    /// character being shown. Under normal (non-`flatIllustrations`) settings, `MenuScene.BuildScene
    /// `/`BuildMSCScene` construct that scene as several layered `MenuDepthIllustration`s loaded
    /// from a per-character folder -- e.g. `BuildSpearScene()` builds
    /// `Scenes/Slugcat - Spearmaster/` as four layers: `"Spearmaster Background - 4"`,
    /// `"Spearmaster Pipes - 3"`, `"Spearmaster Vines - 2"`, and `"Spearmaster Slugcat - 1"` (the
    /// actual character portrait, added last/on top). Every character's own scene-building method
    /// (`BuildGourmandScene`, the base-game `Slugcat_White`/`Slugcat_Yellow`/`Slugcat_Red` branches,
    /// etc.) follows the same convention: background/decoration layers are named after what they
    /// depict (Background, Vines, Pipes, Haze, Specks, BkgPlants, FgPlants, Spears, ...), and
    /// *only* the actual character-portrait layer's filename contains the literal word "Slugcat" --
    /// confirmed by checking every `AddIllustration(new MenuDepthIllustration(...))` call across
    /// `MenuScene.cs`'s character-scene builders. This is a more reliable identifier than the
    /// number suffix the user's own request used as a shorthand ("the '-1' image") -- that suffix
    /// isn't actually consistent across characters (Spearmaster's and Yellow's portrait layers are
    /// both "- 1", but White's is "- 2" and Red's is "- 1" again), while every character's portrait
    /// filename reliably contains "Slugcat" and every decoration layer reliably doesn't.
    ///
    /// **Why this is a Postfix on `MenuDepthIllustration.GrafUpdate`, not a one-time recolor at
    /// construction.** `MenuIllustration` (the base class) exposes a plain `Color color` property
    /// wrapping `sprite.color` -- setting it once would be enough for a `MenuShader.Basic`-shaded
    /// illustration (every MSC character's own portrait layer uses `Basic`, e.g. the Spearmaster
    /// call above), because `MenuDepthIllustration.GrafUpdate` explicitly skips its own per-frame
    /// `sprite.color` recompute `if (shader != MenuShader.Basic)`. But base-game Survivor/Monk/
    /// Hunter's portrait layers use `ModManager.MMF ? MenuShader.Basic : MenuShader.LightEdges` --
    /// on a game without the MMF mod installed, that recompute *does* run every frame and would
    /// silently overwrite a one-time color set back to its own size/depth-encoded value (see
    /// `MenuDepthIllustration.GrafUpdate`'s own `sprite.color = new Color(InverseLerp(...), ...)`
    /// line -- unrelated to visual display for that shader, it feeds a custom blur shader instead).
    /// Postfixing `GrafUpdate` itself re-applies black *after* that recompute runs, every frame,
    /// regardless of which shader a given character's portrait happens to use -- correct for both
    /// cases without needing to special-case shader type.
    ///
    /// **Scope.** Only fires while `PrimalSlot.IsActive` (this mod's reserved save slot), and only
    /// for the `MenuScene.SceneID`s that back the six "not implemented" characters' own portraits
    /// -- checked via the illustration's `owner` (the owning `MenuScene`, per `MenuObject.owner`)
    /// and its `sceneID`, not by character identity directly (illustrations don't carry that). This
    /// naturally covers both the "choose your character" (new-game) screen and the "continue"
    /// screen for an existing save on one of these characters, since both reuse the exact same
    /// scene-building code and portrait assets -- no separate handling needed for either.
    ///
    /// **Not covered, deliberately, for now:** `flatMode`/`flatIllustrations` (a lower-detail
    /// fallback that renders a single pre-flattened `MenuIllustration`, e.g. "Slugcat - White -
    /// Flat", instead of the four-layer scene above) -- per this mod's own earlier portrait-art
    /// investigation (see AI_CONTEXT.md's "Character-select portrait art, in detail"), that flag is
    /// off under normal graphics settings, so this was judged not worth the extra branch until
    /// there's a reason to think it matters in practice.
    ///
    /// `MenuDepthIllustration`, its inherited `fileName`/`sprite` members, `MenuObject.owner`, and
    /// `MenuScene.sceneID` are all plain `public` members on ordinary, widely-reused vanilla menu
    /// classes (not tightly-encapsulated simulation objects like `Player`/`PlayerGraphics`), so
    /// this is patched directly rather than through reflection -- the same judgment call already
    /// applied to `SlugcatPageNewGame`'s labels in PrimalCharacterNames.cs (see Gotcha #1 in
    /// AI_CONTEXT.md).
    ///
    /// **Not yet tested in-game as of this doc.**
    /// </summary>
    [HarmonyPatch(typeof(Menu.MenuDepthIllustration), nameof(Menu.MenuDepthIllustration.GrafUpdate))]
    internal static class PrimalUnimplementedPortraitHook
    {
        private static void Postfix(Menu.MenuDepthIllustration __instance, float timeStacker)
        {
            if (__instance?.fileName == null || __instance.fileName.IndexOf("Slugcat", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return;
            }

            Menu.MenuScene scene = __instance.owner as Menu.MenuScene;
            RainWorld rainWorld = scene?.menu?.manager?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            if (!IsUnimplementedPortraitScene(scene.sceneID))
            {
                return;
            }

            __instance.sprite.color = Color.black;
        }

        private static bool IsUnimplementedPortraitScene(Menu.MenuScene.SceneID sceneID)
        {
            if (sceneID == Menu.MenuScene.SceneID.Slugcat_White
                || sceneID == Menu.MenuScene.SceneID.Slugcat_Yellow
                || sceneID == Menu.MenuScene.SceneID.Slugcat_Red)
            {
                return true;
            }

            if (ModManager.MSC
                && (sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Rivulet
                    || sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Rivulet_Cell
                    || sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Saint
                    || sceneID == MoreSlugcatsEnums.MenuSceneID.Slugcat_Saint_Max))
            {
                return true;
            }

            if (ModManager.Watcher && sceneID == global::Watcher.WatcherEnums.MenuSceneID.Slugcat_Watcher)
            {
                return true;
            }

            return false;
        }
    }
}
