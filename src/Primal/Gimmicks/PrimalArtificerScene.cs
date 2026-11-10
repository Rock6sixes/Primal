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
    // (PrimalSlot.IsActive). Covers every Artificer select-screen state: new game / continue
    // (Slugcat_Artificer), Robo saves (_Robo, _Robo2), ascended saves (End_Artificer) and alt-ending
    // saves (AltEnd_Artificer_Portrait; SlugcatPageContinue uses this when saveGameData.altEnding is set). Every other
    // character, and Artificer on normal slots, keep vanilla art untouched.
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
    //  - MenuDepthIllustration picks its on-screen size by SHADER (verified from its constructor IL and from
    //    the vanilla files): Basic/Rain = a plain image drawn 1:1; every other shader (Normal, Overlay, ...) =
    //    a STACKED image, colour on top + depth/mask map below, drawn at half the file height. Vanilla's
    //    "Artificer Background - 5.png" is 530x988 (colour 530x494 over a red depth map) while its Basic
    //    character layer is a plain 530x494. Our art has no depth maps, so PrimalArtificerShaderHook forces
    //    the Basic shader on every Primal layer and all our depth PNGs are plain 1366x768. (Shipping doubled
    //    1366x1536 files was the earlier bug that stretched the portrait 2x.) The flat PNGs are plain 1366x768.
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

        // Vanilla scene folders that belong to the Artificer select-screen portrait:
        //   "Scenes\Slugcat - Artificer"      (normal + Robo variants)
        //   "Scenes\Slugcat End - Artificer"  (won / ascended save)
        private const string VanillaFolderSuffix = "Slugcat - Artificer";
        private const string VanillaEndFolderSuffix = "Slugcat End - Artificer";
        // alt-ending (win) save: SlugcatPageContinue -> AddAltEndingImage -> AltEnd_Artificer_Portrait
        private const string VanillaAltEndFolderSuffix = "Slugcat End_B - Artificer";
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

            // Robo saves (BuildArtificerScene with Slugcat_Artificer_Robo / _Robo2): the character layer
            // is swapped for a robot-companion layer; show the Primal character instead, drop the extra layer.
            { "Artificer Robot - 1",                   Prefix + "slugcat - 1" },
            { "Artificer Robot - 2",                   Prefix + "slugcat - 1" },
            { "Artificer Robot - 0",                   Prefix + "midbg - 3" },   // empty layer
            { "Slugcat - Artificer Robot 1 - Flat",    Prefix + "flat" },
            { "Slugcat - Artificer Robot 2 - Flat",    Prefix + "flat" },

            // Won / ascended save (BuildArtificerEndScene, folder "Slugcat End - Artificer"):
            // 3 depth layers; "B" uses the Overlay blend shader so it is left empty.
            { "Artificer Bkg",                         Prefix + "end bkg" },     // depth 4.5
            { "Artificer A",                           Prefix + "end a" },       // depth 2.85
            { "Artificer B",                           Prefix + "midbg - 3" },   // empty layer
            { "Artificer - Flat",                      Prefix + "flat" },

            // Alt-ending (win) save: BuildArtificerAltEndScene, folder "Slugcat End_B - Artificer",
            // 6 layers drawn back (5) to front (0). "Primal" order: back plate, spears, chains, character, (unused), foreground.
            { "Slugcat End_B - Artificer - 5",         Prefix + "background - 5" },
            { "Slugcat End_B - Artificer - 4",         Prefix + "vines - 4" },
            { "Slugcat End_B - Artificer - 3",         Prefix + "smoke - 2" },
            { "Slugcat End_B - Artificer - 2",         Prefix + "slugcat - 1" },
            { "Slugcat End_B - Artificer - 1",         Prefix + "midbg - 3" },   // empty layer
            { "Slugcat End_B - Artificer - 0",         Prefix + "fg - 0" },
            { "Slugcat End_B - Artificer - Flat",      Prefix + "flat" },
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
            if (id == null) return false;
            bool isArtificerPortrait =
                id == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer ||
                id == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer_Robo ||
                id == MoreSlugcatsEnums.MenuSceneID.Slugcat_Artificer_Robo2 ||
                id == MoreSlugcatsEnums.MenuSceneID.End_Artificer ||
                id == MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_Portrait;
            if (!isArtificerPortrait)
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
                Log.LogWarning("Artificer scene layer has no Primal mapping, vanilla art will show for it: '" + key + "'");
        }

        private static readonly HashSet<string> _loggedSkipped = new HashSet<string>();
        // An Artificer-folder scene on a Primal slot that we deliberately do not reskin: say so once, with the scene ID.
        internal static void NoteSkippedScene(string sceneId)
        {
            if (_loggedSkipped.Add(sceneId))
                Log.LogWarning("Artificer-folder scene with unlisted sceneID '" + sceneId + "' was NOT reskinned.");
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
                   (folder.EndsWith(VanillaFolderSuffix, StringComparison.OrdinalIgnoreCase) ||
                    folder.EndsWith(VanillaEndFolderSuffix, StringComparison.OrdinalIgnoreCase) ||
                    folder.EndsWith(VanillaAltEndFolderSuffix, StringComparison.OrdinalIgnoreCase));
        }
    }

    // Rewrites (folderName, fileName) of every MenuIllustration (and so MenuDepthIllustration, which
    // calls this base constructor) that belongs to Primal's Artificer scene.
    // Bare [HarmonyPatch] on purpose: Harmony throws "You cannot combine TargetMethod ... with
    // individual annotations" if a type/method is also given here (that crashed PatchAll once).
    [HarmonyPatch]
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
                if (fileName == null) return;
                if (!PrimalArtificerScene.ShouldReskin(scene, menu)) return;
                if (!PrimalArtificerScene.FileMap.TryGetValue(fileName, out mapped))
                {
                    PrimalArtificerScene.NoteUnmapped(folderName, fileName);
                    return;
                }

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

    // Forces the Basic shader on every Primal Artificer depth layer. Our PNGs are plain images (no depth map),
    // and MenuDepthIllustration's constructor only draws a texture 1:1 for Basic/Rain; any other shader treats it
    // as a half-height stacked colour+depth image. Runs BEFORE the base constructor, so folder/file are still the
    // vanilla names here.
    [HarmonyPatch]
    internal static class PrimalArtificerShaderHook
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
                if (!PrimalArtificerScene.IsVanillaArtificerFolder(folderName)) return;
                if (!PrimalArtificerScene.FileMap.ContainsKey(fileName)) return;
                if (!PrimalArtificerScene.ShouldReskin(scene, menu)) return;
                shader = MenuDepthIllustration.MenuShader.Basic;
            }
            catch (Exception e)
            {
                PrimalArtificerScene.Log.LogError("Shader override failed: " + e);
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
