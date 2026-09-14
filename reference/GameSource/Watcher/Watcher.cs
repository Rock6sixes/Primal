using System.Collections.Generic;

namespace Watcher;

public class Watcher
{
	public static string MOD_ID = "watcher";

	public static Configurable<bool> cfgWatcherTutorials;

	public static Configurable<int> cfgForcedCampaignSeed;

	public static Configurable<bool> chtUnlockCampaign;

	public static Configurable<bool> chtUnlockArenas;

	public static Configurable<bool> chtUnlockCreatures;

	public static Configurable<bool> chtUnlockItems;

	public static Configurable<bool> chtUnlockArenaClass;

	public static Configurable<bool> chtUnlockSafari;

	public static Configurable<bool> cfgDisableCosmeticRipples;

	public static Configurable<bool> cfgDisableRippleTrail;

	public static Configurable<float> cfgReducePassiveRainIntensity;

	public static Configurable<bool> cfgDisableLightningFlashes;

	public static Configurable<bool> cfgDisableRainbowGrain;

	public static Configurable<float> cfgReduceFallingParticles;

	public static Configurable<float> cfgReduceFloatingDebrisDensity;

	public static List<WatcherPreset<bool>> boolPresets;

	public static List<WatcherPreset<float>> floatPresets;

	public static int CYCLE_EXPIRY_PLAYER_CREATED_WARP_POINTS = 5;

	public static void OnInit(RainWorld rainWorld)
	{
		OptionInterface optionInterface = new WatcherOptionInterface();
		MachineConnector.SetRegisteredOI(MOD_ID, optionInterface);
		boolPresets = new List<WatcherPreset<bool>>();
		floatPresets = new List<WatcherPreset<float>>();
		optionInterface.config.configurables.Clear();
		cfgWatcherTutorials = optionInterface.config.Bind("cfgWatcherTutorials", defaultValue: true, new ConfigurableInfo("Show tutorial messages for Watcher's mechanics during the story campaign", null, "", "Enable tutorials"));
		cfgForcedCampaignSeed = optionInterface.config.Bind("cfgForcedCampaignSeed", 0, new ConfigurableInfo("Seed the determinations of warp point destinations to create less randomness in runs", new ConfigAcceptableRange<int>(0, 99999), "", "Fixed campaign seed"));
		cfgDisableCosmeticRipples = optionInterface.config.Bind("cfgDisableCosmeticRipples", defaultValue: false, new ConfigurableInfo("Disable cosmetic ripple effects that appear in the vicinity of some warp locations", null, "", "Disable Cosmetic Ripples"));
		boolPresets.Add(new WatcherPreset<bool>(cfgDisableCosmeticRipples, standardValue: false, reducedValue: false, minimalValue: true));
		cfgDisableRippleTrail = optionInterface.config.Bind("cfgDisableRippleTrail", defaultValue: false, new ConfigurableInfo("Disable the visual trail that follows the player once achieving higher ripple attunement", null, "", "Disable Player Ripple Trail"));
		boolPresets.Add(new WatcherPreset<bool>(cfgDisableRippleTrail, standardValue: false, reducedValue: false, minimalValue: true));
		cfgDisableLightningFlashes = optionInterface.config.Bind("cfgDisableLightningFlashes", defaultValue: false, new ConfigurableInfo("Disable fullscreen flash and darkening effect that occurs when lightning strikes", null, "", "Disable Lightning Flashes"));
		boolPresets.Add(new WatcherPreset<bool>(cfgDisableLightningFlashes, standardValue: false, reducedValue: true, minimalValue: true));
		cfgReducePassiveRainIntensity = optionInterface.config.Bind("cfgReducePassiveRainIntensity", 0f, new ConfigurableInfo("Reduce the intensity of rain in regions where it appears as a cosmetic effect all cycle long", null, "", "Reduce Passive Rain Intensity"));
		floatPresets.Add(new WatcherPreset<float>(cfgReducePassiveRainIntensity, 0f, 0.75f, 0.95f));
		cfgReduceFloatingDebrisDensity = optionInterface.config.Bind("cfgReduceFloatingDebrisDensity", 0f, new ConfigurableInfo("Reduce the density and visibility of other effects like wind trails, stardust, floating debris, etc.", null, "", "Reduce Other Effects Density"));
		floatPresets.Add(new WatcherPreset<float>(cfgReduceFloatingDebrisDensity, 0f, 0.3f, 0.7f));
		cfgReduceFallingParticles = optionInterface.config.Bind("cfgReduceFallingParticles", 0f, new ConfigurableInfo("Reduce the density of floating/falling particle effects like rot spores, Aether Ridge salt flakes, etc.", null, "", "Reduce Floating Particles Density"));
		floatPresets.Add(new WatcherPreset<float>(cfgReduceFallingParticles, 0f, 0.5f, 0.8f));
		cfgDisableRainbowGrain = optionInterface.config.Bind("cfgDisableRainbowGrain", defaultValue: false, new ConfigurableInfo("Remove the grainy effect that appears in the background of Fetid Glen areas", null, "", "Disable Fetid Glen Rainbow Grain"));
		boolPresets.Add(new WatcherPreset<bool>(cfgDisableRainbowGrain, standardValue: false, reducedValue: false, minimalValue: true));
		chtUnlockCampaign = optionInterface.config.Bind("chtUnlockCampaign", defaultValue: false, new ConfigurableInfo("(Normally requires completing the game as Survivor or Monk to unlock)", null, "", "Unlock Watcher campaign"));
		chtUnlockArenas = optionInterface.config.Bind("chtUnlockArenas", defaultValue: false, new ConfigurableInfo("(Normally requires finding Orange unlock tokens in the campaign maps)", null, "", "Unlock all arenas"));
		chtUnlockCreatures = optionInterface.config.Bind("chtUnlockCreatures", defaultValue: false, new ConfigurableInfo("(Normally requires finding Blue unlock tokens in the campaign maps)", null, "", "Unlock all sandbox creatures"));
		chtUnlockItems = optionInterface.config.Bind("chtUnlockItems", defaultValue: false, new ConfigurableInfo("(Normally requires finding Blue unlock tokens in the campaign maps)", null, "", "Unlock all sandbox items"));
		chtUnlockArenaClass = optionInterface.config.Bind("chtUnlockArenaClass", defaultValue: false, new ConfigurableInfo("(Normally requires finding Green unlock tokens in the campaign maps)", null, "", "Unlock Watcher arena character"));
		chtUnlockSafari = optionInterface.config.Bind("chtUnlockSafari", defaultValue: false, new ConfigurableInfo("(Normally requires finding Orange unlock tokens in the campaign maps)", null, "", "Unlock all safari regions"));
		WatcherUnlocks.AddUnlocks();
		List<CreatureTemplate.Type> list = new List<CreatureTemplate.Type>(WinState.lizardsOrder);
		if (!list.Contains(CreatureTemplate.Type.RedLizard))
		{
			list.Add(CreatureTemplate.Type.RedLizard);
		}
		if (!list.Contains(CreatureTemplate.Type.CyanLizard))
		{
			list.Add(CreatureTemplate.Type.CyanLizard);
		}
		if (!list.Contains(DLCSharedEnums.CreatureTemplateType.SpitLizard))
		{
			list.Add(DLCSharedEnums.CreatureTemplateType.SpitLizard);
		}
		if (!list.Contains(DLCSharedEnums.CreatureTemplateType.ZoopLizard))
		{
			list.Add(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
		}
		if (!list.Contains(WatcherEnums.CreatureTemplateType.IndigoLizard))
		{
			list.Add(WatcherEnums.CreatureTemplateType.IndigoLizard);
		}
		if (!list.Contains(WatcherEnums.CreatureTemplateType.BlizzardLizard))
		{
			list.Add(WatcherEnums.CreatureTemplateType.BlizzardLizard);
		}
		if (!list.Contains(WatcherEnums.CreatureTemplateType.BasiliskLizard))
		{
			list.Add(WatcherEnums.CreatureTemplateType.BasiliskLizard);
		}
		if (!list.Contains(WatcherEnums.CreatureTemplateType.PeachLizard))
		{
			list.Add(WatcherEnums.CreatureTemplateType.PeachLizard);
		}
		WinState.lizardsOrder = list.ToArray();
		FloatingDebris.AddWatcherFloaters();
		WarpPoint.ResetBlacklist();
	}

	public static void OnDisable(ProcessManager manager)
	{
		WatcherUnlocks.RemoveUnlocks();
		List<CreatureTemplate.Type> list = new List<CreatureTemplate.Type>(WinState.lizardsOrder);
		if (list.Contains(CreatureTemplate.Type.RedLizard))
		{
			list.Remove(CreatureTemplate.Type.RedLizard);
		}
		if (list.Contains(CreatureTemplate.Type.CyanLizard))
		{
			list.Remove(CreatureTemplate.Type.CyanLizard);
		}
		if (list.Contains(DLCSharedEnums.CreatureTemplateType.SpitLizard))
		{
			list.Remove(DLCSharedEnums.CreatureTemplateType.SpitLizard);
		}
		if (list.Contains(DLCSharedEnums.CreatureTemplateType.ZoopLizard))
		{
			list.Remove(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
		}
		if (list.Contains(WatcherEnums.CreatureTemplateType.IndigoLizard))
		{
			list.Remove(WatcherEnums.CreatureTemplateType.IndigoLizard);
		}
		if (list.Contains(WatcherEnums.CreatureTemplateType.BlizzardLizard))
		{
			list.Remove(WatcherEnums.CreatureTemplateType.BlizzardLizard);
		}
		if (list.Contains(WatcherEnums.CreatureTemplateType.BasiliskLizard))
		{
			list.Remove(WatcherEnums.CreatureTemplateType.BasiliskLizard);
		}
		if (list.Contains(WatcherEnums.CreatureTemplateType.PeachLizard))
		{
			list.Remove(WatcherEnums.CreatureTemplateType.PeachLizard);
		}
		WinState.lizardsOrder = list.ToArray();
		FloatingDebris.RemoveWatcherFloaters();
	}
}
