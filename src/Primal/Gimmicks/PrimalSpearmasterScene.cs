using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Menu;
using MoreSlugcats;

namespace Primal.Gimmicks
{
    // PRIMAL SPEARMASTER PORTRAIT
    //
    // Replaces the character-select scene art of Spearmaster, but ONLY while a Primal slot is
    // mounted (PrimalSlot.IsActive). Deliberately a near-copy of PrimalArtificerScene.cs rather
    // than a shared generalisation of it: that file is the only scene reskin ever confirmed working
    // in-game, and mirroring it keeps this one's risk at zero. If a THIRD character gets scene art,
    // factor the three patches below out into one table-driven reskin and port both characters onto
    // it at once -- two copies is the point at which that stops being premature.
    //
    // HOW THE GAME BUILDS THESE SCENES (read off reference/GameSource/Menu/MenuScene.cs, not guessed):
    //  - BuildSpearScene (sceneID Slugcat_Spear, folder "Scenes\Slugcat - Spearmaster") builds 4
    //    MenuDepthIllustrations: "Spearmaster Background - 4" (depth 3.2), "Spearmaster Pipes - 3"
    //    (3.1), "Spearmaster Vines - 2" (2.8), "Spearmaster Slugcat - 1" (2.7; or "... - 1 - Dark"
    //    while the character is still locked). In flat mode (medium/low quality) it builds one
    //    MenuIllustration instead: "Slugcat - Spearmaster - Flat" (or "... Dark - Flat").
    //  - BuildSpearEndScene (End_Spear, "Slugcat End - Spearmaster") builds 3: "Spearmaster Bkg"
    //    (4.5), "Spearmaster A" (2.85), "Spearmaster B" (2.7, Overlay). Flat: "Spearmaster - Flat".
    //  - BuildSpearAltEndScene (AltEnd_Spearmaster, "Slugcat End_B - Spearmaster") builds 3:
    //    "Spearmaster End Bkg" (4.5), "Spearmaster End Halo" (3.5), "Spearmaster End SRS" (2.7,
    //    Basic). Flat: "Spearmaster End B - Flat".
    //  - Every one of those goes through the MenuIllustration constructor, which loads
    //    <folderName>\<fileName>.png via AssetManager.ResolveFilePath (lowercased, searched across
    //    all active mod folders). So this file only has to rewrite (folderName, fileName) on the way
    //    in, and the layer count, depths and parallax all stay exactly as vanilla.
    //  - MenuScene.RefreshPositions then reads positions_ims.txt (positions.txt for non-interactive
    //    scenes) from scene.sceneFolder, one "x, y" line per depth layer. We redirect sceneFolder to
    //    our folder first so OUR positions file is the one applied (all "0, 0": the art is
    //    pre-composed on a 1366x768 canvas). Edit those lines to nudge a layer without touching PNGs.
    //  - MenuDepthIllustration picks its on-screen size by SHADER (its constructor does
    //    size = (texture.width, texture.height / 2) and then, for Basic/Rain only, doubles size.y
    //    back and resets scaleY to 1). So Basic = a plain image drawn 1:1; every other shader
    //    (Normal, Overlay, ...) = a STACKED image, colour over a depth/mask map, drawn at half the
    //    file height. Our art has no depth maps, so PrimalSpearmasterShaderHook forces Basic on
    //    every Primal layer and every PNG is a plain 1366x768. Shipping doubled 1366x1536 files
    //    instead is what stretched the Artificer portrait 2x once -- don't repeat it.
    //
    // ASSET LAYOUT (inside the installed mod folder):
    //    mods\Primal\scenes\primal spearmaster\primal spearmaster *.png, positions_ims.txt, positions.txt
    // Regenerate those from the hand-drawn 1024x1024 sources via scripts/build-spearmaster-scene.py.
    //
    // Gotcha #1 note: the two scene fields read here (sceneID, sceneFolder) go through AccessTools
    // rather than direct access, since the publicized reference DLL can't be trusted about
    // accessibility. The patched methods are resolved by Harmony (not subject to that problem).
    internal static class PrimalSpearmasterScene
    {
        internal static readonly ManualLogSource Log =
            BepInEx.Logging.Logger.CreateLogSource("PrimalSpearmasterScene");

        // Vanilla scene folders that belong to the Spearmaster select-screen portrait.
        private const string VanillaFolderSuffix = "Slugcat - Spearmaster";
        private const string VanillaEndFolderSuffix = "Slugcat End - Spearmaster";
        private const string VanillaAltEndFolderSuffix = "Slugcat End_B - Spearmaster";
        private const string Prefix = "primal spearmaster ";

        // scenes\primal spearmaster  (AssetManager lowercases the whole path anyway)
        internal static readonly string PrimalFolder =
            "scenes" + Path.DirectorySeparatorChar + "primal spearmaster";

        // vanilla layer name -> our file name (no extension). Keys must match vanilla exactly.
        internal static readonly Dictionary<string, string> FileMap = new Dictionary<string, string>
        {
            // Select screen (BuildSpearScene).
            { "Spearmaster Background - 4",        Prefix + "background - 4" },
            { "Spearmaster Pipes - 3",             Prefix + "pipes - 3" },
            { "Spearmaster Vines - 2",             Prefix + "vines - 2" },
            { "Spearmaster Slugcat - 1",           Prefix + "slugcat - 1" },
            { "Spearmaster Slugcat - 1 - Dark",    Prefix + "slugcat - 1 - dark" },
            { "Slugcat - Spearmaster - Flat",      Prefix + "flat" },
            { "Slugcat - Spearmaster Dark - Flat", Prefix + "dark flat" },

            // Story ending (BuildSpearEndScene). Only 3 layers, so the pipes/vines/character art is
            // merged into the single "A" plate; "B" is an Overlay blend layer, left empty.
            { "Spearmaster Bkg",                   Prefix + "background - 4" },
            { "Spearmaster A",                     Prefix + "foreground" },
            { "Spearmaster B",                     Prefix + "empty" },
            { "Spearmaster - Flat",                Prefix + "flat" },

            // Alt ending (BuildSpearAltEndScene). 3 layers, deep enough to keep the character on its
            // own plate: backdrop, then pipes+vines as the mid plate, then the character.
            { "Spearmaster End Bkg",               Prefix + "background - 4" },
            { "Spearmaster End Halo",              Prefix + "midground" },
            { "Spearmaster End SRS",               Prefix + "slugcat - 1" },
            { "Spearmaster End B - Flat",          Prefix + "flat" },
        };

        private static readonly FieldInfo SceneIdField = AccessTools.Field(typeof(MenuScene), "sceneID");
        private static readonly FieldInfo SceneFolderField = AccessTools.Field(typeof(MenuScene), "sceneFolder");

        private static bool? _assetsPresent;
        private static bool _loggedFirstUse;

        // True if our PNGs are actually installed. Derived from this DLL's own location
        // (...\mods\Primal\plugins\Primal.dll -> ...\mods\Primal). If the location can't be
        // determined we assume present; if it can and the file is missing we fall back to vanilla art.
        private static bool AssetsPresent()
        {
            if (_assetsPresent.HasValue) return _assetsPresent.Value;
            try
            {
                string dll = typeof(PrimalSpearmasterScene).Assembly.Location;
                if (string.IsNullOrEmpty(dll)) { _assetsPresent = true; return true; }
                string root = Directory.GetParent(Path.GetDirectoryName(dll)).FullName;
                string probe = Path.Combine(Path.Combine(root, PrimalFolder), Prefix + "slugcat - 1.png");
                _assetsPresent = File.Exists(probe);
                if (!_assetsPresent.Value)
                    Log.LogWarning("Primal Spearmaster art not found at '" + probe + "'. Falling back to vanilla Spearmaster art.");
            }
            catch (Exception e)
            {
                Log.LogError("AssetsPresent check failed, assuming present: " + e);
                _assetsPresent = true;
            }
            return _assetsPresent.Value;
        }

        // Is this scene one of the Spearmaster portrait scenes, on a Primal slot?
        internal static bool ShouldReskin(MenuScene scene, Menu.Menu menu)
        {
            if (scene == null || menu == null) return false;
            RainWorld rw = menu.manager != null ? menu.manager.rainWorld : null;
            if (rw == null || !PrimalSlot.IsActive(rw)) return false;
            if (SceneIdField == null) return false;
            MenuScene.SceneID id = SceneIdField.GetValue(scene) as MenuScene.SceneID;
            if (id == null) return false;
            bool isSpearmasterPortrait =
                id == MoreSlugcatsEnums.MenuSceneID.Slugcat_Spear ||
                id == MoreSlugcatsEnums.MenuSceneID.End_Spear ||
                id == MoreSlugcatsEnums.MenuSceneID.AltEnd_Spearmaster;
            if (!isSpearmasterPortrait)
            {
                NoteSkippedScene(id.ToString());
                return false;
            }
            return AssetsPresent();
        }

        private static readonly HashSet<string> _loggedUnmapped = new HashSet<string>();
        internal static void NoteUnmapped(string folder, string file)
        {
            string key = folder + "|" + file;
            if (_loggedUnmapped.Add(key))
                Log.LogWarning("Spearmaster scene layer has no Primal mapping, vanilla art will show for it: '" + key + "'");
        }

        private static readonly HashSet<string> _loggedSkipped = new HashSet<string>();
        // A Spearmaster-folder scene on a Primal slot that we deliberately do not reskin: say so once.
        internal static void NoteSkippedScene(string sceneId)
        {
            if (_loggedSkipped.Add(sceneId))
                Log.LogWarning("Spearmaster-folder scene with unlisted sceneID '" + sceneId + "' was NOT reskinned.");
        }

        internal static void NoteFirstUse()
        {
            if (_loggedFirstUse) return;
            _loggedFirstUse = true;
            Log.LogInfo("Using Primal Spearmaster scene art from '" + PrimalFolder + "'.");
        }

        internal static string GetSceneFolder(MenuScene scene)
        {
            return SceneFolderField != null ? SceneFolderField.GetValue(scene) as string : null;
        }

        internal static void SetSceneFolder(MenuScene scene, string value)
        {
            if (SceneFolderField != null) SceneFolderField.SetValue(scene, value);
        }

        internal static bool IsVanillaSpearmasterFolder(string folder)
        {
            return !string.IsNullOrEmpty(folder) &&
                   (folder.EndsWith(VanillaFolderSuffix, StringComparison.OrdinalIgnoreCase) ||
                    folder.EndsWith(VanillaEndFolderSuffix, StringComparison.OrdinalIgnoreCase) ||
                    folder.EndsWith(VanillaAltEndFolderSuffix, StringComparison.OrdinalIgnoreCase));
        }
    }

    // Rewrites (folderName, fileName) of every MenuIllustration (and so MenuDepthIllustration, which
    // calls this base constructor) that belongs to Primal's Spearmaster scenes.
    // Bare [HarmonyPatch] on purpose: Harmony throws "You cannot combine TargetMethod ... with
    // individual annotations" if a type/method is also given here (that crashed PatchAll once).
    [HarmonyPatch]
    internal static class PrimalSpearmasterIllustrationHook
    {
        // MenuIllustration has exactly one instance constructor; resolve it explicitly because
        // Harmony can't infer a constructor overload from a bare type.
        private static MethodBase TargetMethod()
        {
            foreach (ConstructorInfo c in AccessTools.GetDeclaredConstructors(typeof(MenuIllustration)))
                if (!c.IsStatic) return c;
            return null;
        }

        // Parameter names (menu, owner, folderName, fileName) match the real constructor's, which is
        // how Harmony binds them.
        private static void Prefix(Menu.Menu menu, MenuObject owner, ref string folderName, ref string fileName)
        {
            try
            {
                MenuScene scene = owner as MenuScene;
                if (scene == null) return;
                if (!PrimalSpearmasterScene.IsVanillaSpearmasterFolder(folderName)) return;

                string mapped;
                if (fileName == null) return;
                if (!PrimalSpearmasterScene.ShouldReskin(scene, menu)) return;
                if (!PrimalSpearmasterScene.FileMap.TryGetValue(fileName, out mapped))
                {
                    PrimalSpearmasterScene.NoteUnmapped(folderName, fileName);
                    return;
                }

                folderName = PrimalSpearmasterScene.PrimalFolder;
                fileName = mapped;
                PrimalSpearmasterScene.NoteFirstUse();
            }
            catch (Exception e)
            {
                PrimalSpearmasterScene.Log.LogError("Illustration redirect failed (vanilla art will load): " + e);
            }
        }
    }

    // Forces the Basic shader on every Primal Spearmaster depth layer. Our PNGs are plain images (no
    // depth map), and MenuDepthIllustration's constructor only draws a texture 1:1 for Basic/Rain;
    // any other shader treats it as a half-height stacked colour+depth image. Runs BEFORE the base
    // constructor, so folder/file are still the vanilla names here.
    [HarmonyPatch]
    internal static class PrimalSpearmasterShaderHook
    {
        private static MethodBase TargetMethod()
        {
            foreach (ConstructorInfo c in AccessTools.GetDeclaredConstructors(typeof(MenuDepthIllustration)))
                if (!c.IsStatic) return c;
            return null;
        }

        // Parameter names (menu, owner, folderName, fileName, pos, depth, shader) match the real constructor.
        private static void Prefix(Menu.Menu menu, MenuObject owner, string folderName, string fileName,
                                   ref MenuDepthIllustration.MenuShader shader)
        {
            try
            {
                MenuScene scene = owner as MenuScene;
                if (scene == null || fileName == null) return;
                if (!PrimalSpearmasterScene.IsVanillaSpearmasterFolder(folderName)) return;
                if (!PrimalSpearmasterScene.FileMap.ContainsKey(fileName)) return;
                if (!PrimalSpearmasterScene.ShouldReskin(scene, menu)) return;
                shader = MenuDepthIllustration.MenuShader.Basic;
            }
            catch (Exception e)
            {
                PrimalSpearmasterScene.Log.LogError("Shader override failed: " + e);
            }
        }
    }

    // Points the scene at our folder just before it applies layer positions, so our
    // positions_ims.txt (not vanilla's) is the one read.
    [HarmonyPatch(typeof(MenuScene), nameof(MenuScene.RefreshPositions))]
    internal static class PrimalSpearmasterPositionsHook
    {
        private static void Prefix(MenuScene __instance)
        {
            try
            {
                if (!PrimalSpearmasterScene.IsVanillaSpearmasterFolder(PrimalSpearmasterScene.GetSceneFolder(__instance))) return;
                if (!PrimalSpearmasterScene.ShouldReskin(__instance, __instance.menu)) return;
                PrimalSpearmasterScene.SetSceneFolder(__instance, PrimalSpearmasterScene.PrimalFolder);
            }
            catch (Exception e)
            {
                PrimalSpearmasterScene.Log.LogError("Positions redirect failed: " + e);
            }
        }
    }
}
