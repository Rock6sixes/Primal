using System;
using HarmonyLib;
using Menu.Remix.MixedUI;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Runs the mod's own setup that has to wait until the game itself is further along than
    /// BepInEx's plugin-load stage (Plugin.OnEnable) allows:
    ///
    ///  - GimmickDefaults.Apply(), which reads MoreSlugcatsEnums.SlugcatStatsName.Gourmand/Spear
    ///    (and, via PrimalOptions below, WatcherEnums.SlugcatStatsName.Watcher) directly.
    ///    Those aren't plain compiled-in constants -- they're ExtEnum values the game registers
    ///    itself (RegisterValues()) during its own startup, and calling Plugin.OnEnable's timing
    ///    (BepInEx's Chainloader stage, well before the game has created so much as a RainWorld
    ///    instance) is too early: the fields read back null there, and dereferencing .value on a
    ///    null SlugcatStats.Name throws -- silently, from BepInEx's point of view, since an
    ///    uncaught exception this early in Plugin.OnEnable used to abort that whole method before
    ///    it ever reached _harmony.PatchAll(), which looked exactly like the mod doing nothing at
    ///    all (BepInEx's own log showed "Loading [Primal]" and then nothing further -- no
    ///    error, no "loaded" line, and every single Harmony patch in the mod silently missing,
    ///    including the PRIMAL SLOT button and every gimmick).
    ///
    ///  - Registering this mod's Remix config screen (PrimalOptions).
    ///    MachineConnector.SetRegisteredOI only succeeds once the game has already
    ///    auto-registered a placeholder OptionInterface for every installed mod ID
    ///    (MachineConnector._RefreshOIs, keyed off ModManager.InstalledMods) -- calling it any
    ///    earlier silently fails and returns false.
    ///
    ///  - Immediately calling EnsureConfigBound() on that same instance -- see its own doc comment
    ///    on PrimalOptions for the full trace, but in short: without this, every
    ///    (character, gimmick) assignment silently reverted to its hardcoded default on every game
    ///    launch, because the real game's own automatic config-load pass
    ///    (RainWorld.PostModsInit -> MachineConnector._LoadAllConfigs()) skips any OptionInterface
    ///    that hasn't had config.Bind() called for it yet -- which, before this fix, only ever
    ///    happened once the player opened this mod's Remix page. Calling it here, in OnModsInit
    ///    (guaranteed to run before PostModsInit), makes the automatic load pass actually find and
    ///    load this mod's saved assignments, with no dependency on the player ever opening Remix.
    ///
    /// RainWorld.OnModsInit is the standard, documented point after all of the above has already
    /// happened -- SlugBase-adjacent mods (e.g. Pupify) register their own options screens from
    /// the exact same hook.
    /// </summary>
    [HarmonyPatch(typeof(RainWorld), nameof(RainWorld.OnModsInit))]
    internal static class RainWorldHooks
    {
        private static void Postfix(RainWorld __instance)
        {
            try
            {
                GimmickDefaults.Apply();
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"GimmickDefaults.Apply() threw -- default gimmick assignments may be missing (still fully assignable by hand in Remix): {ex}");
            }

            var options = new PrimalOptions();
            MachineConnector.SetRegisteredOI(Primal.PluginInfo.GUID, options);

            try
            {
                // Fixes a real persistence bug -- see EnsureConfigBound()'s own doc comment for
                // the full trace. Must run after GimmickDefaults.Apply() above (it reads those
                // defaults as each Configurable's bound default) and after SetRegisteredOI (not
                // load-bearing for this call itself, but keeps the two related setup steps in
                // their natural order).
                options.EnsureConfigBound();
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"PrimalOptions.EnsureConfigBound() threw -- gimmick assignments may not persist across game restarts this session (still fully assignable by hand in Remix): {ex}");
            }
        }
    }
}
