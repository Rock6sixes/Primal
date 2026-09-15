using System;
using BepInEx;
using HarmonyLib;

namespace Primal
{
    // No [BepInDependency] on purpose. This mod used to declare a hard dependency on SlugBase
    // ("slime-cubed.slugbase"), left over from the two abandoned designs that tried to create
    // custom SlugBase characters (see AI_CONTEXT.md's History -- both were scrapped by explicit
    // user decision). Nothing in this assembly has referenced SlugBase in any form since; the
    // csproj doesn't even reference it. A bare BepInDependency defaults to HardDependency, so
    // that leftover attribute meant BepInEx refused to load Primal at all unless the player also
    // had SlugBase installed -- a real install requirement for zero functionality. Removed
    // 2026-09-15 along with the matching "requirements" entry in mod/modinfo.json.
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony;

        /// <summary>Static access to this plugin's BepInEx logger, for the Harmony patch
        /// classes elsewhere in the mod (they're static and have no Logger of their own) to log
        /// through -- ends up in BepInEx/LogOutput.log under the "Primal" source name.</summary>
        internal static BepInEx.Logging.ManualLogSource Log;

        public void OnEnable()
        {
            Log = Logger;

            try
            {
                // Patches every [HarmonyPatch] class in this assembly, including the gimmicks
                // (MiseryGimmick, SlugpupGimmick, OneSpearGimmick), the PRIMAL SLOT
                // options-menu button (PrimalSlotOptionsMenuHooks), and the RainWorld.OnModsInit
                // hook (Gimmicks/RainWorldHooks.cs) that runs the rest of this mod's setup once
                // the game itself is far enough along (see that file's doc comment for why some
                // of this mod's setup can't happen here in Plugin.OnEnable at all).
                //
                // Deliberately doesn't reference any game-content enum (MoreSlugcatsEnums,
                // WatcherEnums, etc.) anywhere in this method or anything it calls -- those
                // aren't safely readable this early (BepInEx's Chainloader stage, well before the
                // game creates its first RainWorld instance) and an uncaught exception here would
                // silently skip every patch below it with no visible error. If PatchAll() itself
                // still throws for some other reason, it's caught and logged rather than left to
                // vanish the same way.
                _harmony = new Harmony(PluginInfo.GUID);
                _harmony.PatchAll();
                Logger.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{PluginInfo.Name} v{PluginInfo.Version} FAILED TO LOAD: {ex}");
            }
        }
    }

    internal static class PluginInfo
    {
        public const string GUID = "rokas.primal";
        public const string Name = "Primal";
        public const string Version = "0.1.0";
    }
}
