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
    // PRIMAL ARTIFICER PORTRAIT
    //
    // Replaces the character-select scene art of Artificer, but ONLY while a Primal slot is mounted
    // (PrimalSlot.IsActive) and ONLY for the plain Slugcat_Artificer scene. The Robo / End variants,
    // every other character, and Artificer on normal slots keep vanilla art untouched.
    //
    // HOW THE GAME BUILDS THIS SCENE (all confirmed by disassembling the real assembly, not guessed):
    //  - MenuScene.BuildArtificerScene builds 6 MenuDepthIllustrations from folder "Scenes\Slugcat - Artificer":
    //      "Artificer Background - 5" (depth 3.6), "Artificer Vines - 4" (3.4), "Artificer MidBg - 3" (3.1),
    //      "Artificer Smoke - 2" (2.8), "Artificer Slugcat - 1" (2.7; or "... - 1 - Dark" while locked),
    //      "Artificer Fg - 0" (2.1).  In flat mode (medium/low quality) it builds one MenuIllustration instead:
    //      "Slugcat - Artificer - Flat" (or "Slugcat - Artificer Dark - Flat" while locked).
    //  - Every one of those goes through the MenuIllustration constructor, which loads
    //    <folderName>\<fileName>.png via AssetManager.ResolveFilePath (lowercased, searched across all active
    //    mod folders). So this file only has to rewrite (folderName, fileName) on the way in, and the
    //    layer count, depths, shaders and parallax all stay exactly as vanilla.
    //  - MenuScene.RefreshPositions then reads positions_ims.txt (positions.txt for non-interactive scenes)
    //    from scene.sceneFolder, one "x, y" line per depth layer. We redirect sceneFolder to our folder
    //    first so OUR positions file is the one applied (all "0, 0": the art is pre-composed on a
    //    1366x768 canvas). Edit those lines to nudge a layer without touching the PNGs.
    //  - MenuDepthIllustration draws at HALF height (scaleY 0.5), so the depth PNGs are stored vertically
    //    doubled (1366x1536). The flat PNGs are plain 1366x768.
    //
    // ASSET LAYOUT (inside the installed mod folder):
    //    mods\Primal\scenes\primal artificer\primal artificer *.png, positions_ims.txt, positions.txt
    //
    // Gotcha #1 note: the two scene fields read here (sceneID, sceneFolder) go through AccessTools rather than
    // direct access, since the publicized reference DLL can't be trusted about accessibility. The patched
    // methods are resolved by Harmony (not subject to that problem).
    internal static class PrimalArtificerScene
    {
        internal static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("PrimalArtificerScene");

        private const string VanillaFolderSuffix = "Slugcat - Artificer";
        private const string Prefix = "primal artificer ";

        // scenes\primal artificer  (AssetManager lowercases the whole path anyway)
        internal static readonly string PrimalFolder =
            "scenes" + Path.DirectorySeparatorChar + "primal artificer";

        // vanilla layer name -> our file name (no extension). Keys must match vanilla exactly.
        internal static readonly Dictionary<string, string> FileMap = new Dictionary<string, string>
        {
            { "Artificer Background - 5",        Prefix + "background - 5" },
            { "Artificer Vines - 4",             Prefix + "vines - 4" },
            { "Artificer MidBg - 3",             Prefix + "midbg - 3" },
            { "Artificer Smoke - 2",             Prefix + "smoke - 2" },
            { "Artificer Slugcat - 1",           Prefix + "slugcat - 1" },
            { "Artificer Slugcat - 1 - Dark",    Prefix + "slugcat - 1 - dark" },
            { "Artificer Fg - 0",                Prefix + "fg - 0" },
            { "Slugcat - Artificer - Flat",      Prefix + "flat" },
            { "Slugcat - Artificer Dark - Flat", Prefix + "dark flat" },
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
                string dll = typeof(PrimalArtificerScene).Assembly.Location;
                if (string.IsNullOrEmpty(dll)) { _assetsPresent = true; return true; }
                string root = Directory.GetParent(Path.GetDirectoryName(dll)).FullName;
                string probe = Path.Combine(Path.Combine(root, PrimalFolder), Prefix + "slugcat - 1.png");
                _assetsPresent = File.Exists(probe);
                if (!_assetsPresent.Value)
                    Log.LogWarning("Primal Artificer art not found at '" + probe + "'. Falling back to vanilla Artificer art.");
            }
            catch (Exception e)
            {
                Log.LogError("AssetsPresent check failed, assuming present: " + e);
                _assetsPresent = true;
            }
            return _assetsPresent.Value;
        }

        // Is this scene the plain Artificer select-screen scene, on a Primal slot?
        internal static bool ShouldReskin(MenuScene scene, Menu.Menu menu)
        {
            if (scene == null || menu == null) return false;
            RainWorld rw = menu.manager != null ? menu.manager.rainWorld : null;
            if (rw == null || !PrimalSlot.IsActive(rw)) return false;
            if (SceneIdField == null) return false;
            MenuScene.SceneID id = SceneIdField.GetValue(scene) as MenuScene.SceneID;
            if (id == null || id != MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer) return false;
            return AssetsPresent();
        }

        internal static void NoteFirstUse()
        {
            if (_loggedFirstUse) return;
            _loggedFirstUse = true;
            Log.LogInfo("Using Primal Artificer scene art from '" + PrimalFolder + "'.");
        }

        internal static string GetSceneFolder(MenuScene scene)
        {
            return SceneFolderField != null ? SceneFolderField.GetValue(scene) as string : null;
        }

        internal static void SetSceneFolder(MenuScene scene, string value)
        {
            if (SceneFolderField != null) SceneFolderField.SetValue(scene, value);
        }

        internal static bool IsVanillaArtificerFolder(string folder)
        {
            return !string.IsNullOrEmpty(folder) &&
                   folder.EndsWith(VanillaFolderSuffix, StringComparison.OrdinalIgnoreCase);
        }
    }

    // Rewrites (folderName, fileName) of every MenuIllustration (and so MenuDepthIllustration, which
    // calls this base constructor) that belongs to Primal's Artificer scene.
    [HarmonyPatch(typeof(MenuIllustration))]
    internal static class PrimalArtificerIllustrationHook
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
                if (!PrimalArtificerScene.IsVanillaArtificerFolder(folderName)) return;

                string mapped;
                if (fileName == null || !PrimalArtificerScene.FileMap.TryGetValue(fileName, out mapped)) return;
                if (!PrimalArtificerScene.ShouldReskin(scene, menu)) return;

                folderName = PrimalArtificerScene.PrimalFolder;
                fileName = mapped;
                PrimalArtificerScene.NoteFirstUse();
            }
            catch (Exception e)
            {
                PrimalArtificerScene.Log.LogError("Illustration redirect failed (vanilla art will load): " + e);
            }
        }
    }

    // Points the scene at our folder just before it applies layer positions, so our
    // positions_ims.txt (not vanilla's) is the one read.
    [HarmonyPatch(typeof(MenuScene), nameof(MenuScene.RefreshPositions))]
    internal static class PrimalArtificerPositionsHook
    {
        private static void Prefix(MenuScene __instance)
        {
            try
            {
                if (!PrimalArtificerScene.IsVanillaArtificerFolder(PrimalArtificerScene.GetSceneFolder(__instance))) return;
                if (!PrimalArtificerScene.ShouldReskin(__instance, __instance.menu)) return;
                PrimalArtificerScene.SetSceneFolder(__instance, PrimalArtificerScene.PrimalFolder);
            }
            catch (Exception e)
            {
                PrimalArtificerScene.Log.LogError("Positions redirect failed: " + e);
            }
        }
    }
}
