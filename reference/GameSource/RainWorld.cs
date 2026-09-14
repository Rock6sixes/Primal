using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using AssetBundles;
using Expedition;
using JollyCoop;
using Kittehface.Build;
using Kittehface.Framework20;
using Menu;
using MoreSlugcats;
using RWCustom;
using Rewired;
using Rewired.Config;
using Rewired.Utils;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Watcher;

public class RainWorld : MonoBehaviour
{
	public enum BuildType
	{
		Distribution,
		Testing,
		Development
	}

	public delegate void RewiredDoUpdateDelegate(UpdateLoopType updateLoopType, UpdateLoopSetting updateLoopSettingBit);

	public enum AchievementID
	{
		None,
		PassageSurvivor,
		PassageHunter,
		PassageSaint,
		PassageTraveller,
		PassageChieftain,
		PassageMonk,
		PassageOutlaw,
		PassageDragonSlayer,
		PassageScholar,
		PassageFriend,
		GhostCC,
		GhostSI,
		GhostLF,
		GhostSH,
		GhostUW,
		GhostSB,
		AllGhostsEncountered,
		MoonEncounterGood,
		MoonEncounterBad,
		PebblesEncounter,
		Win,
		HunterPayload,
		HunterWin,
		GourmandEnding,
		ArtificerEnding,
		RivuletEnding,
		SpearmasterEnding,
		SaintEnding,
		ChallengeMode,
		Quests,
		PassageMartyr,
		PassageNomad,
		PassagePilgrim,
		PassageMother,
		WarpPoint,
		BadWarp,
		AncientUrban,
		SpinningTopEnding,
		RotPrince,
		RotPrinceEnding,
		RippleDimension,
		RippleCreature,
		VoidWeaver,
		WeaverAbility,
		VoidWeaverEnding,
		PearlReader,
		WatcherAscension
	}

	public ProcessManager processManager;

	public PersistentData persistentData;

	public PlayerProgression progression;

	public Dictionary<string, FShader> Shaders;

	public ComputeShader particleComputeShader;

	public RainWorldGame.SetupValues setup;

	public Options options;

	public static Dictionary<string, int> roomNameToIndex;

	public static Dictionary<int, string> roomIndexToName;

	public static int worldVersion;

	public static int loadedWorldVersion;

	public static int gameVersion;

	public const string GAME_VERSION_STRING = "v1.11.8";

	public BuildType buildType;

	public InGameTranslator inGameTranslator;

	public bool saveBackedUp;

	public bool flatIllustrations;

	public bool skipVoidSea;

	public string lastLoggedException;

	public string lastLoggedStackTrace;

	public bool started;

	public bool processManagerInitialized;

	public SlugcatStats.Name inGameSlugCat;

	public List<string> dlcsInstalled;

	public RewiredDoUpdateDelegate rewiredDelegateFunc;

	public double lastMouseActiveTime;

	public int targetPlayerForProfileAssignment;

	public List<Profile> profilesWaitingToBeAssigned;

	public List<int> targetPlayersWaitingToBeAssigned;

	public bool progressionBeingReloaded;

	public Hashtable dataOnProgressionReload;

	public byte[] rawDataOnProgressionReload;

	public static TimeSpan CurrentFreeTimeSpan = TimeSpan.Zero;

	public Camera mainCamera;

	public static Thread MainThread;

	[SerializeField]
	public PlayerHandler playerHandlerPrefab;

	public static object _loggingLock = new object();

	public Texture2D _NoiseTex;

	public Texture2D _NoiseTex2;

	public Texture2D LightMask0;

	public Texture2D maze;

	public Texture2D glyphs;

	public Texture2D corruption;

	public Texture2D _CloudsTex;

	public Texture2D _TextGradientTex;

	public Texture2D apartmentsTex;

	public Texture2D cityPalette;

	public Texture2D sootMark;

	public Texture2D sootMark2;

	public bool LoadModResourcesCheck;

	public bool recheckedDownpourDLC;

	public bool recheckedWatcherDLC;

	public bool processManagerInitializationStarted;

	public static int ShadPropNoiseTex = Shader.PropertyToID("_NoiseTex");

	public static int ShadPropNoiseTex2 = Shader.PropertyToID("_NoiseTex2");

	public static int ShadPropCloudsTex = Shader.PropertyToID("_CloudsTex");

	public static int ShadPropTextGradientTex = Shader.PropertyToID("_TextGradientTex");

	public static int ShadPropApartmentsTex = Shader.PropertyToID("_ApartmentsTex");

	public static int ShadPropCityPalette = Shader.PropertyToID("_CityPalette");

	public static int ShadPropUniNoise = Shader.PropertyToID("_UniNoise");

	public static int ShadPropSwirl = Shader.PropertyToID("_EnergySwirl");

	public static int ShadPropPAngle = Shader.PropertyToID("_pAngle");

	public static int ShadPropMapWaterCol = Shader.PropertyToID("_MapWaterCol");

	public static int ShadPropScreenSize = Shader.PropertyToID("_screenSize");

	public static int ShadPropScreenOffset = Shader.PropertyToID("_screenOffset");

	public static int ShadPropSceneOrigoPosition = Shader.PropertyToID("_SceneOrigoPosition");

	public static int ShadPropHologramThreshold = Shader.PropertyToID("_hologramThreshold");

	public static int ShadPropWorldCamPos = Shader.PropertyToID("_WorldCamPos");

	public static int ShadPropAboveCloudsAtmosphereColor = Shader.PropertyToID("_AboveCloudsAtmosphereColor");

	public static int ShadPropMultiplyColor = Shader.PropertyToID("_MultiplyColor");

	public static int ShadPropGridOffset = Shader.PropertyToID("_gridOffset");

	public static int ShadPropGrime = Shader.PropertyToID("_Grime");

	public static int ShadPropWindDir = Shader.PropertyToID("_windDir");

	public static int ShadPropWindTex = Shader.PropertyToID("_WindTex");

	public static int ShadPropWindAngle = Shader.PropertyToID("_windAngle");

	public static int ShadPropWindStrength = Shader.PropertyToID("_windStrength");

	public static int ShadPropWindTexRendered = Shader.PropertyToID("_WindTexRendered");

	public static int ShadPropRimFix = Shader.PropertyToID("_rimFix");

	public static int ShadPropRain = Shader.PropertyToID("_RAIN");

	public static int ShadPropRainDirection = Shader.PropertyToID("_rainDirection");

	public static int ShadPropRainSpriteRect = Shader.PropertyToID("_RainSpriteRect");

	public static int ShadPropRainIntensity = Shader.PropertyToID("_rainIntensity");

	public static int ShadPropRainEverywhere = Shader.PropertyToID("_rainEverywhere");

	public static int ShadPropMapFogTexture = Shader.PropertyToID("_mapFogTexture");

	public static int ShadPropMapCol = Shader.PropertyToID("_MapCol");

	public static int ShadPropMapPan = Shader.PropertyToID("_mapPan");

	public static int ShadPropMapSize = Shader.PropertyToID("_mapSize");

	public static int ShadPropTransitionColor = Shader.PropertyToID("_transitionColor");

	public static int ShadPropBlurDepth = Shader.PropertyToID("_BlurDepth");

	public static int ShadPropBlurRange = Shader.PropertyToID("_BlurRange");

	public static int ShadPropMenuCamPos = Shader.PropertyToID("_MenuCamPos");

	public static int ShadPropTileCorrection = Shader.PropertyToID("_tileCorrection");

	public static int ShadPropTopBottom = Shader.PropertyToID("_topBottom");

	public static int ShadPropMainTex = Shader.PropertyToID("_MainTex");

	public static int ShadPropOriginal = Shader.PropertyToID("_Original");

	public static int ShadPropFirstPass = Shader.PropertyToID("_firstPass");

	public static int ShadPropStep = Shader.PropertyToID("_step");

	public static int ShadPropDustFlowTex = Shader.PropertyToID("_DustFlowTex");

	public static int ShadPropDustWaveProgress = Shader.PropertyToID("_DustWaveProgress");

	public static int ShadPropTileTex = Shader.PropertyToID("_TileTex");

	public static int ShadPropEnergyCellCoreCol = Shader.PropertyToID("_EnergyCellCoreCol");

	public static int ShadPropLeviathanColorA = Shader.PropertyToID("_LeviathanColorA");

	public static int ShadPropLeviathanColorB = Shader.PropertyToID("_LeviathanColorB");

	public static int ShadPropLeviathanColorHead = Shader.PropertyToID("_LeviathanColorHead");

	public static int ShadPropWetTerrain = Shader.PropertyToID("_WetTerrain");

	public static int ShadPropSwarmRoom = Shader.PropertyToID("_SwarmRoom");

	public static int ShadPropSnowTex = Shader.PropertyToID("_SnowTex");

	public static int ShadPropSnowSources = Shader.PropertyToID("_SnowSources");

	public static int ShadPropSnowStrength = Shader.PropertyToID("_snowStrength");

	public static int ShadPropPlayerPos = Shader.PropertyToID("_PlayerPos");

	public static int ShadPropLevelTex = Shader.PropertyToID("_LevelTex");

	public static int ShadPropWaterDepth = Shader.PropertyToID("_waterDepth");

	public static int ShadPropWaterTime = Shader.PropertyToID("_waterTime");

	public static int ShadPropLightDirAndPixelSize = Shader.PropertyToID("_lightDirAndPixelSize");

	public static int ShadPropFogAmount = Shader.PropertyToID("_fogAmount");

	public static int ShadPropSpriteRect = Shader.PropertyToID("_spriteRect");

	public static int ShadPropCamInRoomRect = Shader.PropertyToID("_camInRoomRect");

	public static int ShadPropWaterLevel = Shader.PropertyToID("_waterLevel");

	public static int ShadPropLight1 = Shader.PropertyToID("_light");

	public static int ShadPropDarkness = Shader.PropertyToID("_darkness");

	public static int ShadPropBrightness = Shader.PropertyToID("_brightness");

	public static int ShadPropContrast = Shader.PropertyToID("_contrast");

	public static int ShadPropSaturation = Shader.PropertyToID("_saturation");

	public static int ShadPropHue = Shader.PropertyToID("_hue");

	public static int ShadPropCloudsSpeed = Shader.PropertyToID("_cloudsSpeed");

	public static int ShadPropPalTex = Shader.PropertyToID("_PalTex");

	public static int ShadPropRPMHideBeforeIndex = Shader.PropertyToID("_rotParticleMeshHideBeforeIndex");

	[HideInInspector]
	[SerializeField]
	public AchievementData[] achievementData;

	public Dictionary<AchievementID, bool> achievementsUnlocked = new Dictionary<AchievementID, bool>();

	public static string[] recentConsoleLog;

	public Dictionary<string, List<MultiplayerUnlocks.SandboxUnlockID>> regionBlueTokens = new Dictionary<string, List<MultiplayerUnlocks.SandboxUnlockID>>();

	public Dictionary<string, List<MultiplayerUnlocks.LevelUnlockID>> regionGoldTokens = new Dictionary<string, List<MultiplayerUnlocks.LevelUnlockID>>();

	public Dictionary<string, List<MultiplayerUnlocks.SlugcatUnlockID>> regionGreenTokens = new Dictionary<string, List<MultiplayerUnlocks.SlugcatUnlockID>>();

	public Dictionary<string, List<MultiplayerUnlocks.SafariUnlockID>> regionRedTokens = new Dictionary<string, List<MultiplayerUnlocks.SafariUnlockID>>();

	public Dictionary<string, List<ChatlogData.ChatlogID>> regionGreyTokens = new Dictionary<string, List<ChatlogData.ChatlogID>>();

	public Dictionary<string, List<DataPearl.AbstractDataPearl.DataPearlType>> regionDataPearls = new Dictionary<string, List<DataPearl.AbstractDataPearl.DataPearlType>>();

	public Dictionary<string, List<string>> regionSpinningTopRooms = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> regionWeaverRooms = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> regionWarpRooms = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> regionDynamicWarpTargets = new Dictionary<string, List<string>>();

	public Dictionary<float, List<string>> levelDynamicWarpTargets = new Dictionary<float, List<string>>();

	public List<string> levelDeadEndTargets = new List<string>();

	public List<string> levelBadWarpTargets = new List<string>();

	public Dictionary<string, Dictionary<string, int>> regionRippleSpawnEggs = new Dictionary<string, Dictionary<string, int>>();

	public Dictionary<string, List<List<SlugcatStats.Name>>> regionBlueTokensAccessibility = new Dictionary<string, List<List<SlugcatStats.Name>>>();

	public Dictionary<string, List<List<SlugcatStats.Name>>> regionGoldTokensAccessibility = new Dictionary<string, List<List<SlugcatStats.Name>>>();

	public Dictionary<string, List<List<SlugcatStats.Name>>> regionGreenTokensAccessibility = new Dictionary<string, List<List<SlugcatStats.Name>>>();

	public Dictionary<string, List<List<SlugcatStats.Name>>> regionRedTokensAccessibility = new Dictionary<string, List<List<SlugcatStats.Name>>>();

	public Dictionary<string, List<List<SlugcatStats.Name>>> regionDataPearlsAccessibility = new Dictionary<string, List<List<SlugcatStats.Name>>>();

	public Texture2D energySwirl;

	public static SlugcatStats.Name lastActiveSaveSlot;

	public bool safariMode;

	public string safariRegion;

	public SlugcatStats.Name safariSlugcat;

	public bool safariRainDisable;

	public Texture2D pAngle;

	public Texture2D uniNoise;

	public Texture2D sandNormals;

	public static Color mapWaterColorInternal;

	public static Color DefaultWaterColor = new Color(0.05f, 0.05f, 0.8f);

	public static IDrawable CurrentlyDrawingObject;

	public static Color[] PlayerObjectBodyColors = new Color[4];

	public static string ShelterBeforePassage;

	public static string ShelterAfterPassage;

	public static bool lockGameTimer;

	public Vector2 screenSize => options.ScreenSize;

	public static HSLColor AntiGold => new HSLColor(0.5861111f, 0.65f, 0.53f);

	public static HSLColor GoldHSL => new HSLColor(0.08611111f, 0.65f, 0.53f);

	public static Color GoldRGB => new Color(0.529f, 0.365f, 0.184f);

	public static Color SaturatedGold => GoldRGB * 2f;

	public static Color RippleColor => new Color(0.373f, 0.11f, 0.831f);

	public static Color RippleGold => new Color(0.355f, 0.31f, 0.87f);

	public static Color MapColor => Custom.HSL2RGB(0.73055553f, 0.2f, 0.4f);

	public int dlcVersion { get; set; }

	public bool assetBundlesInitialized { get; set; }

	public bool platformInitialized { get; set; }

	public InputManager rewiredInputManager { get; set; }

	public Camera MainCamera
	{
		get
		{
			if (mainCamera == null)
			{
				mainCamera = Camera.main;
			}
			return mainCamera;
		}
	}

	public bool OptionsReady => options.optionsLoaded;

	public static bool ShowLogs => ModManager.DevTools;

	public bool ExpeditionMode
	{
		get
		{
			if (ModManager.Expedition)
			{
				return options.saveSlot < 0;
			}
			return false;
		}
	}

	public void Awake()
	{
		dlcsInstalled = new List<string>();
		if (Custom.rainWorld == null)
		{
			Custom.rainWorld = this;
		}
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
		if (File.Exists("exceptionLog.txt"))
		{
			File.Delete("exceptionLog.txt");
		}
		if (File.Exists("consoleLog.txt"))
		{
			File.Delete("consoleLog.txt");
		}
		Application.logMessageReceivedThreaded += HandleLog;
		profilesWaitingToBeAssigned = new List<Profile>();
		targetPlayersWaitingToBeAssigned = new List<int>();
		recentConsoleLog = new string[14];
		for (int i = 0; i < recentConsoleLog.Length; i++)
		{
			recentConsoleLog[i] = "";
		}
		MultiplayerUnlocks.CreatureUnlockList = new List<MultiplayerUnlocks.SandboxUnlockID>
		{
			MultiplayerUnlocks.SandboxUnlockID.Slugcat,
			MultiplayerUnlocks.SandboxUnlockID.GreenLizard,
			MultiplayerUnlocks.SandboxUnlockID.PinkLizard,
			MultiplayerUnlocks.SandboxUnlockID.BlueLizard,
			MultiplayerUnlocks.SandboxUnlockID.WhiteLizard,
			MultiplayerUnlocks.SandboxUnlockID.BlackLizard,
			MultiplayerUnlocks.SandboxUnlockID.YellowLizard,
			MultiplayerUnlocks.SandboxUnlockID.CyanLizard,
			MultiplayerUnlocks.SandboxUnlockID.RedLizard,
			MultiplayerUnlocks.SandboxUnlockID.Salamander,
			MultiplayerUnlocks.SandboxUnlockID.Fly,
			MultiplayerUnlocks.SandboxUnlockID.CicadaA,
			MultiplayerUnlocks.SandboxUnlockID.CicadaB,
			MultiplayerUnlocks.SandboxUnlockID.Snail,
			MultiplayerUnlocks.SandboxUnlockID.Leech,
			MultiplayerUnlocks.SandboxUnlockID.SeaLeech,
			MultiplayerUnlocks.SandboxUnlockID.PoleMimic,
			MultiplayerUnlocks.SandboxUnlockID.TentaclePlant,
			MultiplayerUnlocks.SandboxUnlockID.Scavenger,
			MultiplayerUnlocks.SandboxUnlockID.VultureGrub,
			MultiplayerUnlocks.SandboxUnlockID.Vulture,
			MultiplayerUnlocks.SandboxUnlockID.KingVulture,
			MultiplayerUnlocks.SandboxUnlockID.SmallCentipede,
			MultiplayerUnlocks.SandboxUnlockID.MediumCentipede,
			MultiplayerUnlocks.SandboxUnlockID.BigCentipede,
			MultiplayerUnlocks.SandboxUnlockID.RedCentipede,
			MultiplayerUnlocks.SandboxUnlockID.Centiwing,
			MultiplayerUnlocks.SandboxUnlockID.TubeWorm,
			MultiplayerUnlocks.SandboxUnlockID.Hazer,
			MultiplayerUnlocks.SandboxUnlockID.LanternMouse,
			MultiplayerUnlocks.SandboxUnlockID.Spider,
			MultiplayerUnlocks.SandboxUnlockID.BigSpider,
			MultiplayerUnlocks.SandboxUnlockID.SpitterSpider,
			MultiplayerUnlocks.SandboxUnlockID.MirosBird,
			MultiplayerUnlocks.SandboxUnlockID.BrotherLongLegs,
			MultiplayerUnlocks.SandboxUnlockID.DaddyLongLegs,
			MultiplayerUnlocks.SandboxUnlockID.Deer,
			MultiplayerUnlocks.SandboxUnlockID.EggBug,
			MultiplayerUnlocks.SandboxUnlockID.DropBug,
			MultiplayerUnlocks.SandboxUnlockID.BigNeedleWorm,
			MultiplayerUnlocks.SandboxUnlockID.SmallNeedleWorm,
			MultiplayerUnlocks.SandboxUnlockID.JetFish,
			MultiplayerUnlocks.SandboxUnlockID.BigEel
		};
		MultiplayerUnlocks.ItemUnlockList = new List<MultiplayerUnlocks.SandboxUnlockID>
		{
			MultiplayerUnlocks.SandboxUnlockID.Rock,
			MultiplayerUnlocks.SandboxUnlockID.Spear,
			MultiplayerUnlocks.SandboxUnlockID.FireSpear,
			MultiplayerUnlocks.SandboxUnlockID.ScavengerBomb,
			MultiplayerUnlocks.SandboxUnlockID.SporePlant,
			MultiplayerUnlocks.SandboxUnlockID.Lantern,
			MultiplayerUnlocks.SandboxUnlockID.FlyLure,
			MultiplayerUnlocks.SandboxUnlockID.Mushroom,
			MultiplayerUnlocks.SandboxUnlockID.FlareBomb,
			MultiplayerUnlocks.SandboxUnlockID.PuffBall,
			MultiplayerUnlocks.SandboxUnlockID.WaterNut,
			MultiplayerUnlocks.SandboxUnlockID.FirecrackerPlant,
			MultiplayerUnlocks.SandboxUnlockID.DangleFruit,
			MultiplayerUnlocks.SandboxUnlockID.JellyFish,
			MultiplayerUnlocks.SandboxUnlockID.BubbleGrass,
			MultiplayerUnlocks.SandboxUnlockID.SlimeMold,
			MultiplayerUnlocks.SandboxUnlockID.SeedCob
		};
		ReInput.configuration.maxJoysticksPerPlayer = 99;
		ReInput.configuration.autoAssignJoysticks = false;
	}

	public void HandleLog(string logString, string stackTrace, LogType type)
	{
		lock (_loggingLock)
		{
			if (type == LogType.Error || type == LogType.Exception)
			{
				if (logString != lastLoggedException && stackTrace != lastLoggedStackTrace)
				{
					File.AppendAllText("exceptionLog.txt", logString + Environment.NewLine);
					File.AppendAllText("exceptionLog.txt", stackTrace + Environment.NewLine);
					lastLoggedException = logString;
					lastLoggedStackTrace = stackTrace;
				}
				return;
			}
			if (ModManager.ModdingEnabled)
			{
				File.AppendAllText("consoleLog.txt", logString + Environment.NewLine);
			}
			if (ModManager.DevTools)
			{
				for (int i = 0; i < recentConsoleLog.Length - 1; i++)
				{
					recentConsoleLog[i] = recentConsoleLog[i + 1];
				}
				recentConsoleLog[recentConsoleLog.Length - 1] = logString;
			}
		}
	}

	public void PreModsDisabledEnabled()
	{
	}

	public void OnModsEnabled(ModManager.Mod[] newlyEnabledMods)
	{
	}

	public void OnModsDisabled(ModManager.Mod[] newlyDisabledMods)
	{
		bool flag = false;
		for (int i = 0; i < newlyDisabledMods.Length; i++)
		{
			if (!flag && (newlyDisabledMods[i].id == global::MoreSlugcats.MoreSlugcats.MOD_ID || newlyDisabledMods[i].id == global::Watcher.Watcher.MOD_ID))
			{
				DLCSharedEnums.UnregisterAllEnumExtensions();
				flag = true;
			}
			if (newlyDisabledMods[i].id == global::MoreSlugcats.MoreSlugcats.MOD_ID)
			{
				global::MoreSlugcats.MoreSlugcats.OnDisable(processManager);
				MoreSlugcatsEnums.UnregisterAllEnumExtensions();
			}
			if (newlyDisabledMods[i].id == MMF.MOD_ID)
			{
				MMF.OnDisable(processManager);
				MMFEnums.UnregisterAllEnumExtensions();
			}
			if (newlyDisabledMods[i].id == global::JollyCoop.JollyCoop.MOD_ID)
			{
				JollyEnums.UnregisterAllEnumExtensions();
			}
			if (newlyDisabledMods[i].id == global::Expedition.Expedition.MOD_ID)
			{
				global::Expedition.Expedition.OnDisable();
				ExpeditionEnums.UnregisterAllEnumExtensions();
			}
			if (newlyDisabledMods[i].id == global::Watcher.Watcher.MOD_ID)
			{
				global::Watcher.Watcher.OnDisable(processManager);
				WatcherEnums.UnregisterAllEnumExtensions();
			}
		}
	}

	public void PostModsDisabledEnabled()
	{
	}

	public void PreModsInit()
	{
		MachineConnector._Initialize();
	}

	public void OnModsInit()
	{
		ExtEnumInitializer.InitTypes();
		if (ModManager.DLCShared)
		{
			DLCSharedEnums.RegisterAllEnumExtensions();
		}
		if (ModManager.MSC)
		{
			MoreSlugcatsEnums.RegisterAllEnumExtensions();
			global::MoreSlugcats.MoreSlugcats.OnInit();
		}
		if (ModManager.MMF)
		{
			MMFEnums.RegisterAllEnumExtensions();
			MMF.OnInit();
		}
		if (ModManager.JollyCoop)
		{
			JollyEnums.RegisterAllEnumExtensions();
			JollyCustom.CreateJollyLog();
		}
		if (ModManager.Expedition)
		{
			ExpeditionEnums.RegisterAllEnumExtensions();
			global::Expedition.Expedition.OnInit(this);
		}
		if (ModManager.Watcher)
		{
			WatcherEnums.RegisterAllEnumExtensions();
			global::Watcher.Watcher.OnInit(this);
		}
	}

	public void PostModsInit()
	{
		Platform.BeginInitialUserDataRead();
		MachineConnector._LoadAllConfigs();
		inGameTranslator.LoadShortStrings();
	}

	public static void LoadIndexMapsIntoMemory(int worldVersion)
	{
		roomIndexToName = new Dictionary<int, string>();
		roomNameToIndex = new Dictionary<string, int>();
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "indexmaps" + Path.DirectorySeparatorChar + "roomindexmap" + worldVersion + ".txt"));
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(' ');
			int num = int.Parse(array2[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			string text = array2[1].Trim();
			roomIndexToName[num] = text;
			roomNameToIndex[text] = num;
		}
	}

	public void Start()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		MainThread = Thread.CurrentThread;
		AOC.Initialize();
		AOCUpdate.Initialize();
		Custom.InitializeRootFolderDirectory();
		DLCCheck();
		File.WriteAllText(Path.Combine(Custom.RootFolderDirectory(), "GameVersion.txt"), "v1.11.8");
		if (Platform.initialized)
		{
			platformInitialized = true;
			_ = Profiles.ActiveProfiles.Count;
			_ = 0;
			AchievementsLoad();
		}
		else
		{
			Platform.OnInitialized += Platform_OnInitialized;
		}
		AOC.OnAocMounted += AOC_OnMounted;
		AOCUpdate.AOCUpdate_RefreshMods = (Action)Delegate.Combine(AOCUpdate.AOCUpdate_RefreshMods, new Action(AOCUpdate_RefreshModList));
		AOCUpdate.AOCUpdate_OnNewDLCInstalled = (Action)Delegate.Combine(AOCUpdate.AOCUpdate_OnNewDLCInstalled, new Action(AOCUpdate_onDLCInstalled));
		AOCUpdate.AOCUpdate_OnVersionError = (Action)Delegate.Combine(AOCUpdate.AOCUpdate_OnVersionError, new Action(AOCUpdate_OnVersionError));
		Platform.OnRequestAchievementsLoad += Platform_OnRequestAchievementsLoad;
		UserInput.OnControllerDisconnected += new ControllerDisconnectedAction(UserInput_OnControllerDisconnected);
		RWInput.RefreshSystemControllers();
		ReInput.ControllerConnectedEvent += JoystickConnected;
		ReInput.ControllerPreDisconnectEvent += JoystickPreDisconnect;
		ReInput.ControllerDisconnectedEvent += JoystickDisconnected;
		Profiles.OnActivated += Profiles_OnActivated;
		Profiles.OnDeactivated += Profiles_OnDeactivated;
		Achievements.OnAchievementUnlocked += new AchievementUnlocked(Achievements_OnAchievementUnlocked);
		Achievements.OnAchievementRetrieved += new AchievementRetrieved(Achievements_OnAchievementRetrieved);
		FutileParams futileParams = new FutileParams(supportsLandscapeLeft: true, supportsLandscapeRight: true, supportsPortrait: true, supportsPortraitUpsideDown: true);
		futileParams.AddResolutionLevel(1366f, 1f, 1f, "");
		futileParams.origin = new Vector2(0f, 0f);
		Futile.instance.Init(futileParams);
		Futile.displayScale = 1f;
	}

	public void AOC_OnMounted()
	{
		Debug.Log("Running AOC_OnMounted!");
		AOC.OnAocMounted -= AOC_OnMounted;
		DLCCheck();
	}

	public void AOCUpdate_RefreshModList()
	{
		DLCCheck();
		ModManager.RefreshModsLists(this);
	}

	public async void AOCUpdate_OnVersionError()
	{
	}

	public void AOCUpdate_onDLCInstalled()
	{
		AOCUpdate.newDLCNotify = false;
		DialogNotify dialog = new DialogNotify("DLC has been installed or updated and can be accessed from the remix menu.", processManager, delegate
		{
		});
		processManager.ShowDialog(dialog);
	}

	public void OnDestroy()
	{
		if (Custom.rainWorld == this)
		{
			Custom.rainWorld = null;
		}
	}

	public bool CheckDLCVersions()
	{
		return true;
	}

	public void DLCCheck()
	{
		Debug.Log("Running DLCCheck!");
		DLCOwnershipCheckSwitchPC();
	}

	public void DLCOwnershipCheckConsole()
	{
		if (dlcsInstalled.Contains("downpour"))
		{
			dlcsInstalled.Remove("downpour");
		}
		if (dlcsInstalled.Contains("watcher"))
		{
			dlcsInstalled.Remove("watcher");
		}
		for (int i = 0; i < AOC.GetAOCLength(); i++)
		{
			if (File.Exists(AOC.GetAOCMount(i) + Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + "moreslugcats" + Path.DirectorySeparatorChar + "modinfo.json"))
			{
				Debug.Log("Downpour DLC was found!");
				if (!dlcsInstalled.Contains("downpour"))
				{
					dlcsInstalled.Add("downpour");
				}
			}
			if (File.Exists(AOC.GetAOCMount(i) + Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + "watcher" + Path.DirectorySeparatorChar + "modinfo.json"))
			{
				Debug.Log("Watcher DLC was found!");
				if (!dlcsInstalled.Contains("watcher"))
				{
					dlcsInstalled.Add("watcher");
				}
			}
		}
	}

	public void DLCOwnershipCheckSwitchPC()
	{
		string path = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + ("scenes" + Path.DirectorySeparatorChar + "main menu - downpour" + Path.DirectorySeparatorChar + "main menu - downpour - flat.png").ToLowerInvariant();
		string path2 = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + ("mods" + Path.DirectorySeparatorChar + "moreslugcats" + Path.DirectorySeparatorChar + "illustrations" + Path.DirectorySeparatorChar + "safari_ms.png").ToLowerInvariant();
		string path3 = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + ("mods" + Path.DirectorySeparatorChar + "jollycoop" + Path.DirectorySeparatorChar + "illustrations" + Path.DirectorySeparatorChar + "jolly_title.png").ToLowerInvariant();
		string path4 = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + ("mods" + Path.DirectorySeparatorChar + "expedition" + Path.DirectorySeparatorChar + "illustrations" + Path.DirectorySeparatorChar + "expeditiontitle.png").ToLowerInvariant();
		if (File.Exists(path) && File.Exists(path2) && File.Exists(path3) && File.Exists(path4))
		{
			Debug.Log("Downpour DLC was found!");
			if (!dlcsInstalled.Contains("downpour"))
			{
				dlcsInstalled.Add("downpour");
			}
		}
		else if (dlcsInstalled.Contains("downpour"))
		{
			dlcsInstalled.Remove("downpour");
		}
		path = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + "watcher" + Path.DirectorySeparatorChar + "illustrations" + Path.DirectorySeparatorChar + "au_building_0.png").ToLowerInvariant();
		path2 = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + "watcher" + Path.DirectorySeparatorChar + "scenes" + Path.DirectorySeparatorChar + "main menu - watcher" + Path.DirectorySeparatorChar + "main menu watcher - flat.png").ToLowerInvariant();
		if (File.Exists(path) && File.Exists(path2))
		{
			Debug.Log("Watcher DLC was found!");
			if (!dlcsInstalled.Contains("watcher"))
			{
				dlcsInstalled.Add("watcher");
			}
		}
		else if (dlcsInstalled.Contains("watcher"))
		{
			dlcsInstalled.Remove("watcher");
		}
	}

	public void LoadResources()
	{
		LightMask0 = Resources.Load("Atlases/LightMask0") as Texture2D;
		Futile.atlasManager.LoadAtlasFromTexture("LightMask0", LightMask0, textureFromAsset: true);
		maze = Resources.Load("Atlases/maze") as Texture2D;
		Futile.atlasManager.LoadAtlasFromTexture("maze", maze, textureFromAsset: true);
		glyphs = Resources.Load("Atlases/glyphs") as Texture2D;
		Futile.atlasManager.LoadAtlasFromTexture("glyphs", glyphs, textureFromAsset: true);
		corruption = Resources.Load("Atlases/corruption") as Texture2D;
		Futile.atlasManager.LoadAtlasFromTexture("corruption", corruption, textureFromAsset: true);
		sootMark = Resources.Load("Atlases/sootmark") as Texture2D;
		Futile.atlasManager.LoadAtlasFromTexture("sootmark", sootMark, textureFromAsset: true);
		sootMark2 = Resources.Load("Atlases/sootmark2") as Texture2D;
		Futile.atlasManager.LoadAtlasFromTexture("sootmark2", sootMark2, textureFromAsset: true);
		_NoiseTex = Resources.Load("Palettes/noise") as Texture2D;
		_NoiseTex2 = Resources.Load("Palettes/noise2") as Texture2D;
		_CloudsTex = Resources.Load("Illustrations/cloudsTexture") as Texture2D;
		_TextGradientTex = Resources.Load("Palettes/textGradient") as Texture2D;
		apartmentsTex = Resources.Load("Illustrations/apartments") as Texture2D;
		apartmentsTex.anisoLevel = 0;
		apartmentsTex.filterMode = FilterMode.Point;
		cityPalette = Resources.Load("Palettes/cityPalette") as Texture2D;
		cityPalette.anisoLevel = 0;
		cityPalette.filterMode = FilterMode.Point;
		Futile.atlasManager.LoadAtlas("Atlases/rainWorld");
		Futile.atlasManager.LoadAtlas("Atlases/uiSprites");
		Futile.atlasManager.LoadAtlas("Atlases/shelterGate");
		Futile.atlasManager.GetAtlasWithName("Atlases/shelterGate").texture.filterMode = FilterMode.Point;
		Futile.atlasManager.GetAtlasWithName("Atlases/shelterGate").texture.anisoLevel = 0;
		Futile.atlasManager.LoadAtlas("Atlases/regionGate");
		Futile.atlasManager.GetAtlasWithName("Atlases/regionGate").texture.filterMode = FilterMode.Point;
		Futile.atlasManager.GetAtlasWithName("Atlases/regionGate").texture.anisoLevel = 0;
		Futile.atlasManager.LoadAtlas("Atlases/waterSprites");
		SetGlobalTextures();
	}

	public void LoadModResources()
	{
		LoadModResourcesCheck = false;
		if (ModManager.MMF)
		{
			Futile.atlasManager.LoadAtlas("Atlases/uiSpritesMMF");
		}
		if (ModManager.DLCShared)
		{
			Futile.atlasManager.LoadAtlas("Atlases/rainWorldMSC");
			Futile.atlasManager.LoadAtlas("Atlases/uiSpritesMSC");
		}
		if (ModManager.Watcher)
		{
			Futile.atlasManager.LoadAtlas("atlases/rainworldwatcher");
			Futile.atlasManager.LoadAtlas("atlases/uispriteswatcher");
			Futile.atlasManager.LoadAtlas("atlases/au_buildings");
		}
		if (ModManager.Expedition)
		{
			Futile.atlasManager.LoadAtlas("atlases/expedition");
		}
		if (ModManager.JollyCoop)
		{
			Futile.atlasManager.LoadAtlas("atlases/jollycoop");
		}
	}

	public void SetGlobalTextures()
	{
		Shader.SetGlobalTexture(ShadPropNoiseTex, _NoiseTex);
		Shader.SetGlobalTexture(ShadPropNoiseTex2, _NoiseTex2);
		Shader.SetGlobalTexture(ShadPropCloudsTex, _CloudsTex);
		Shader.SetGlobalTexture(ShadPropTextGradientTex, _TextGradientTex);
		Shader.SetGlobalTexture(ShadPropApartmentsTex, apartmentsTex);
		Shader.SetGlobalTexture(ShadPropCityPalette, cityPalette);
		Shader.SetGlobalTexture("_SlopedTerrainMask", Texture2D.blackTexture);
		if (uniNoise == null)
		{
			byte[] data = File.ReadAllBytes(AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "UniNoise.png"));
			uniNoise = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false, linear: false);
			uniNoise.LoadImage(data);
			uniNoise.filterMode = FilterMode.Point;
			Shader.SetGlobalTexture(ShadPropUniNoise, uniNoise);
		}
		if (energySwirl == null)
		{
			byte[] data2 = File.ReadAllBytes(AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "energyRing.png"));
			energySwirl = new Texture2D(164, 4096, TextureFormat.Alpha8, mipChain: false, linear: true);
			energySwirl.LoadImage(data2);
			energySwirl.wrapMode = TextureWrapMode.Clamp;
			energySwirl.filterMode = FilterMode.Bilinear;
			Shader.SetGlobalTexture(ShadPropSwirl, energySwirl);
		}
		if (pAngle == null)
		{
			byte[] data3 = File.ReadAllBytes(AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "pAngle.png"));
			pAngle = new Texture2D(128, 128, TextureFormat.Alpha8, mipChain: true, linear: false);
			pAngle.LoadImage(data3);
			pAngle.wrapMode = TextureWrapMode.Clamp;
			pAngle.filterMode = FilterMode.Bilinear;
			Shader.SetGlobalTexture(ShadPropPAngle, pAngle);
		}
		if (sandNormals == null)
		{
			byte[] data4 = File.ReadAllBytes(AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "sandnormals.png"));
			sandNormals = new Texture2D(128, 128, TextureFormat.RGB24, mipChain: true, linear: true);
			sandNormals.LoadImage(data4);
			sandNormals.wrapMode = TextureWrapMode.Repeat;
			sandNormals.filterMode = FilterMode.Bilinear;
			Shader.SetGlobalTexture("_SandNormals", sandNormals);
		}
	}

	public void UnloadResources()
	{
		Futile.atlasManager.UnloadAtlas("LightMask0");
		Futile.atlasManager.UnloadAtlas("maze");
		Futile.atlasManager.UnloadAtlas("glyphs");
		Futile.atlasManager.UnloadAtlas("corruption");
		Futile.atlasManager.UnloadAtlas("sootmark");
		Futile.atlasManager.UnloadAtlas("sootmark2");
		Futile.atlasManager.UnloadAtlas("Atlases/rainWorld");
		Futile.atlasManager.UnloadAtlas("Atlases/uiSprites");
		Futile.atlasManager.UnloadAtlas("Atlases/shelterGate");
		Futile.atlasManager.UnloadAtlas("Atlases/regionGate");
		Futile.atlasManager.UnloadAtlas("Atlases/waterSprites");
		Futile.atlasManager.UnloadAtlas("Atlases/outPostSkulls");
		Futile.atlasManager.UnloadAtlas("Atlases/adjustableFan");
		Futile.atlasManager.UnloadAtlas("Atlases/hugeTurbine");
		if (ModManager.MMF)
		{
			Futile.atlasManager.UnloadAtlas("Atlases/uiSpritesMMF");
		}
		if (ModManager.DLCShared)
		{
			Futile.atlasManager.UnloadAtlas("Atlases/rainWorldMSC");
			Futile.atlasManager.UnloadAtlas("Atlases/uiSpritesMSC");
		}
		if (ModManager.Watcher)
		{
			Futile.atlasManager.UnloadAtlas("atlases/rainworldwatcher");
			Futile.atlasManager.UnloadAtlas("atlases/uispriteswatcher");
			Futile.atlasManager.UnloadAtlas("atlases/au_buildings");
		}
		if (ModManager.Expedition)
		{
			Futile.atlasManager.UnloadAtlas("atlases/expedition");
		}
		if (ModManager.JollyCoop)
		{
			Futile.atlasManager.UnloadAtlas("atlases/jollycoop");
		}
	}

	public void Update()
	{
		if (LoadModResourcesCheck)
		{
			LoadModResources();
		}
		if (UserInput.initialized && options == null)
		{
			options = new Options(this);
			inGameTranslator = new InGameTranslator(this);
			inGameTranslator.LoadShortStrings();
			InGameTranslator.LoadFonts(options.language, null);
			options.ReassignAllJoysticks();
		}
		if (options != null && options.optionsLoaded)
		{
			for (int num = profilesWaitingToBeAssigned.Count - 1; num >= 0; num--)
			{
				if (options.controls[targetPlayersWaitingToBeAssigned[num]].controlSetupInitialized)
				{
					AssignProfile(profilesWaitingToBeAssigned[num], targetPlayersWaitingToBeAssigned[num]);
					profilesWaitingToBeAssigned.RemoveAt(num);
					targetPlayersWaitingToBeAssigned.RemoveAt(num);
				}
			}
		}
		if (!started && options != null && options.optionsLoaded)
		{
			ModManager.RefreshModsLists(this);
			if ((UnityEngine.Object)(object)rewiredInputManager == null)
			{
				rewiredInputManager = UnityEngine.Object.FindObjectOfType<InputManager>();
			}
			if ((UnityEngine.Object)(object)rewiredInputManager == null)
			{
				Custom.LogWarning("Could not find Rewired InputManager!");
			}
			if (rewiredDelegateFunc == null)
			{
				MethodInfo method = ((object)rewiredInputManager).GetType().GetMethod("DoUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
				Delegate @delegate = Delegate.CreateDelegate(typeof(RewiredDoUpdateDelegate), rewiredInputManager, method);
				rewiredDelegateFunc = (RewiredDoUpdateDelegate)@delegate;
			}
			if ((UnityEngine.Object)(object)rewiredInputManager == null)
			{
				Custom.LogWarning("Could not find DoUpdate method reflection! This was implemented to avoid Unity SendMessage");
			}
			if (!Utilities.isDebugBuild)
			{
				buildType = BuildType.Distribution;
			}
			try
			{
				worldVersion = int.Parse(File.ReadAllText(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "worldVersion.txt").ToLowerInvariant()), NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			catch
			{
			}
			gameVersion = 2;
			LoadIndexMapsIntoMemory(worldVersion);
			StartCoroutine(InitializeAssetBundles());
			setup = LoadSetupValues(buildType == BuildType.Distribution);
			flatIllustrations = File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "flatmode.txt").ToLowerInvariant());
			skipVoidSea = File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "skipvoid.txt").ToLowerInvariant());
			persistentData = new PersistentData(this);
			if (!processManagerInitialized)
			{
				ProcessManagerInitializsation();
			}
			ChatlogData.rainWorld = this;
			EncryptUtility();
			started = true;
		}
		if (started)
		{
			if (progression != null)
			{
				progression.Update();
			}
			if (progressionBeingReloaded && progression != null && progression.progressionLoaded)
			{
				progressionBeingReloaded = false;
				progression.SetData(dataOnProgressionReload, rawDataOnProgressionReload);
				if (progression.CanSave)
				{
					progression.SaveProgression(saveMaps: false, saveMiscProg: true);
				}
			}
			if (((Controller)ReInput.controllers.Mouse).GetAnyButton())
			{
				lastMouseActiveTime = ReInput.time.unscaledTime;
			}
			bool flag = false;
			if (dlcsInstalled.Contains("downpour") && AOC.CheckImplementation() && !recheckedDownpourDLC && AOC.CheckMounted())
			{
				DLCCheck();
				recheckedDownpourDLC = true;
				flag = true;
			}
			if (dlcsInstalled.Contains("watcher") && AOC.CheckImplementation() && !recheckedWatcherDLC && AOC.CheckMounted())
			{
				DLCCheck();
				recheckedWatcherDLC = true;
				flag = true;
			}
			if (flag)
			{
				ModManager.RefreshModsLists(this);
			}
		}
		if (processManagerInitialized)
		{
			processManager.Update(Time.deltaTime);
			if (processManager.currentMainLoop is RainWorldGame rainWorldGame)
			{
				if (rainWorldGame.StoryCharacter != null)
				{
					SpeedRunTimer.CampaignTimeTracker campaignTimeTracker = SpeedRunTimer.GetCampaignTimeTracker(rainWorldGame.StoryCharacter);
					if (campaignTimeTracker != null)
					{
						CurrentFreeTimeSpan = campaignTimeTracker.TotalFreeTimeSpan;
					}
				}
			}
			else
			{
				CurrentFreeTimeSpan = TimeSpan.Zero;
			}
		}
		if (ModManager.CoopAvailable)
		{
			JollyCustom.WriteToLog();
		}
	}

	public void ProcessManagerInitializsation()
	{
		setup = LoadSetupValues(buildType == BuildType.Distribution);
		if (processManagerInitializationStarted)
		{
			return;
		}
		processManagerInitializationStarted = true;
		Shaders = new Dictionary<string, FShader>();
		Shaders.Add("Basic", FShader.CreateShader("Basic", Shader.Find("Futile/Basic")));
		Shaders.Add("BasicAdditive", FShader.CreateShader("BasicAdditive", Shader.Find("Futile/BasicAdditive")));
		Shaders.Add("LevelColor", FShader.CreateShader("LevelColor", Shader.Find("Futile/LevelColor")));
		Shaders.Add("Background", FShader.CreateShader("Background", Shader.Find("Futile/Background")));
		Shaders.Add("WaterSurface", FShader.CreateShader("WaterSurface", Shader.Find("Futile/WaterSurface")));
		Shaders.Add("DeepWater", FShader.CreateShader("DeepWater", Shader.Find("Futile/DeepWater")));
		Shaders.Add("Shortcuts", FShader.CreateShader("ShortCut0", Shader.Find("Futile/ShortCut0")));
		Shaders.Add("DeathRain", FShader.CreateShader("DeathRain", Shader.Find("Futile/DeathRain")));
		Shaders.Add("LizardLaser", FShader.CreateShader("LizardLaser", Shader.Find("Futile/LizardLaser")));
		Shaders.Add("WaterLight", FShader.CreateShader("WaterLight", Shader.Find("Futile/WaterLight")));
		Shaders.Add("WaterFall", FShader.CreateShader("WaterFall", Shader.Find("Futile/WaterFall")));
		Shaders.Add("ShockWave", FShader.CreateShader("ShockWave", Shader.Find("Futile/ShockWave")));
		Shaders.Add("Smoke", FShader.CreateShader("Smoke", Shader.Find("Futile/Smoke")));
		Shaders.Add("Spores", FShader.CreateShader("Spores", Shader.Find("Futile/Spores")));
		Shaders.Add("Steam", FShader.CreateShader("Steam", Shader.Find("Futile/Steam")));
		Shaders.Add("ColoredSprite", FShader.CreateShader("ColoredSprite", Shader.Find("Futile/ColoredSprite")));
		Shaders.Add("ColoredSprite2", FShader.CreateShader("ColoredSprite2", Shader.Find("Futile/ColoredSprite2")));
		Shaders.Add("ColoredSprite3", FShader.CreateShader("ColoredSprite3", Shader.Find("Futile/ColoredSprite2"), new string[1] { "no_grab_tex" }));
		Shaders.Add("LightSource", FShader.CreateShader("LightSource", Shader.Find("Futile/LightSource")));
		Shaders.Add("LightSourceBothSides", FShader.CreateShader("LightSourceBothSides", Shader.Find("Futile/LightSource"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("LightSourceRippleSide", FShader.CreateShader("LightSourceRippleSide", Shader.Find("Futile/LightSource"), new string[1] { "ripple_other_side" }));
		Shaders.Add("LightBloom", FShader.CreateShader("LightBloom", Shader.Find("Futile/LightBloom")));
		Shaders.Add("SkyBloom", FShader.CreateShader("SkyBloom", Shader.Find("Futile/SkyBloom")));
		Shaders.Add("Adrenaline", FShader.CreateShader("Adrenaline", Shader.Find("Futile/Adrenaline")));
		Shaders.Add("AdrenalineBothSides", FShader.CreateShader("AdrenalineBothSides", Shader.Find("Futile/Adrenaline"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("CicadaWing", FShader.CreateShader("CicadaWing", Shader.Find("Futile/CicadaWing")));
		Shaders.Add("BulletRain", FShader.CreateShader("BulletRain", Shader.Find("Futile/BulletRain")));
		Shaders.Add("CustomDepth", FShader.CreateShader("CustomDepth", Shader.Find("Futile/CustomDepth")));
		Shaders.Add("CustomDepthBothSides", FShader.CreateShader("CustomDepthBothSides", Shader.Find("Futile/CustomDepth"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("UnderWaterLight", FShader.CreateShader("UnderWaterLight", Shader.Find("Futile/UnderWaterLight")));
		Shaders.Add("FlatLight", FShader.CreateShader("FlatLight", Shader.Find("Futile/FlatLight")));
		Shaders.Add("FlatLightRippleSide", FShader.CreateShader("FlatLightBothSides", Shader.Find("Futile/FlatLight"), new string[1] { "ripple_other_side" }));
		Shaders.Add("FlatLightBothSides", FShader.CreateShader("FlatLightBothSides", Shader.Find("Futile/FlatLight"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("FlatLightBehindTerrain", FShader.CreateShader("FlatLightBehindTerrain", Shader.Find("Futile/FlatLightBehindTerrain")));
		Shaders.Add("VectorCircle", FShader.CreateShader("VectorCircle", Shader.Find("Futile/VectorCircle")));
		Shaders.Add("VectorCircleBothSides", FShader.CreateShader("VectorCircleBothSides", Shader.Find("Futile/VectorCircle"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("VectorCircleRippleSide", FShader.CreateShader("VectorCircleRippleSide", Shader.Find("Futile/VectorCircle"), new string[1] { "ripple_other_side" }));
		Shaders.Add("VectorCircleFadable", FShader.CreateShader("VectorCircleFadable", Shader.Find("Futile/VectorCircleFadable")));
		Shaders.Add("FlareBomb", FShader.CreateShader("FlareBomb", Shader.Find("Futile/FlareBomb")));
		Shaders.Add("FlareBombBothSides", FShader.CreateShader("FlareBombBothSides", Shader.Find("Futile/FlareBomb"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("Fog", FShader.CreateShader("Fog", Shader.Find("Futile/Fog")));
		Shaders.Add("WaterSplash", FShader.CreateShader("WaterSplash", Shader.Find("Futile/WaterSplash")));
		Shaders.Add("EelFin", FShader.CreateShader("EelFin", Shader.Find("Futile/EelFin")));
		Shaders.Add("EelBody", FShader.CreateShader("EelBody", Shader.Find("Futile/EelBody")));
		Shaders.Add("JaggedCircle", FShader.CreateShader("JaggedCircle", Shader.Find("Futile/JaggedCircle")));
		Shaders.Add("JaggedCircleBothSides", FShader.CreateShader("JaggedCircleBothSides", Shader.Find("Futile/JaggedCircle"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("JaggedCircleRippleSide", FShader.CreateShader("JaggedCircleRippleSide", Shader.Find("Futile/JaggedCircle"), new string[1] { "ripple_other_side" }));
		Shaders.Add("JaggedCircleCustomDepth", FShader.CreateShader("JaggedCircleCustomDepth", Shader.Find("Futile/JaggedCircle"), new string[1] { "custom_depth" }));
		Shaders.Add("JaggedSquare", FShader.CreateShader("JaggedSquare", Shader.Find("Futile/JaggedSquare")));
		Shaders.Add("TubeWorm", FShader.CreateShader("TubeWorm", Shader.Find("Futile/TubeWorm")));
		Shaders.Add("LizardAntenna", FShader.CreateShader("LizardAntenna", Shader.Find("Futile/LizardAntenna")));
		Shaders.Add("TentaclePlant", FShader.CreateShader("TentaclePlant", Shader.Find("Futile/TentaclePlant")));
		Shaders.Add("TentaclePlantBothSides", FShader.CreateShader("TentaclePlantBothSides", Shader.Find("Futile/TentaclePlant"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("TentaclePlantRippleSide", FShader.CreateShader("TentaclePlantRippleSide", Shader.Find("Futile/TentaclePlant"), new string[1] { "ripple_other_side" }));
		Shaders.Add("LevelMelt", FShader.CreateShader("LevelMelt", Shader.Find("Futile/LevelColor"), new string[2] { "levelheat", "levelmelt" }));
		Shaders.Add("LevelMelt2", FShader.CreateShader("LevelMelt2", Shader.Find("Futile/LevelMelt2")));
		Shaders.Add("CoralCircuit", FShader.CreateShader("CoralCircuit", Shader.Find("Futile/CoralCircuit")));
		Shaders.Add("CoralCircuitBothSides", FShader.CreateShader("CoralCircuitBothSides", Shader.Find("Futile/CoralCircuit"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("DeadCoralCircuit", FShader.CreateShader("DeadCoralCircuit", Shader.Find("Futile/DeadCoralCircuit")));
		Shaders.Add("DeadCoralCircuitBothSides", FShader.CreateShader("DeadCoralCircuitBothSides", Shader.Find("Futile/DeadCoralCircuit"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("CoralNeuron", FShader.CreateShader("CoralNeuron", Shader.Find("Futile/CoralNeuron")));
		Shaders.Add("CoralNeuronBothSides", FShader.CreateShader("CoralNeuronBothSides", Shader.Find("Futile/CoralNeuron"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("Bloom", FShader.CreateShader("Bloom", Shader.Find("Futile/Bloom")));
		Shaders.Add("GravityDisruptor", FShader.CreateShader("GravityDisruptor", Shader.Find("Futile/GravityDisruptor")));
		Shaders.Add("GlyphProjection", FShader.CreateShader("GlyphProjection", Shader.Find("Futile/GlyphProjection")));
		Shaders.Add("BlackGoo", FShader.CreateShader("BlackGoo", Shader.Find("Futile/BlackGoo")));
		Shaders.Add("BlackGooBothSides", FShader.CreateShader("BlackGooBothSides", Shader.Find("Futile/BlackGoo"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("Map", FShader.CreateShader("Map", Shader.Find("Futile/Map")));
		Shaders.Add("MapAerial", FShader.CreateShader("MapMapAerial", Shader.Find("Futile/MapAerial")));
		Shaders.Add("MapShortcut", FShader.CreateShader("MapShortcut", Shader.Find("Futile/MapShortcut")));
		Shaders.Add("MapSealedShortcut", FShader.CreateShader("MapSealedShortcut", Shader.Find("Futile/MapSealedShortcut")));
		Shaders.Add("LightAndSkyBloom", FShader.CreateShader("LightAndSkyBloom", Shader.Find("Futile/LightAndSkyBloom")));
		Shaders.Add("SceneBlur", FShader.CreateShader("SceneBlur", Shader.Find("Futile/SceneBlur")));
		Shaders.Add("EdgeFade", FShader.CreateShader("EdgeFade", Shader.Find("Futile/EdgeFade")));
		Shaders.Add("HeatDistortion", FShader.CreateShader("HeatDistortion", Shader.Find("Futile/HeatDistortion")));
		Shaders.Add("Projection", FShader.CreateShader("Projection", Shader.Find("Futile/Projection")));
		Shaders.Add("SingleGlyph", FShader.CreateShader("SingleGlyph", Shader.Find("Futile/SingleGlyph")));
		Shaders.Add("DeepProcessing", FShader.CreateShader("DeepProcessing", Shader.Find("Futile/DeepProcessing")));
		Shaders.Add("Cloud", FShader.CreateShader("Cloud", Shader.Find("Futile/Cloud")));
		Shaders.Add("CloudDistant", FShader.CreateShader("CloudDistant", Shader.Find("Futile/CloudDistant")));
		Shaders.Add("DistantBkgObject", FShader.CreateShader("DistantBkgObject", Shader.Find("Futile/DistantBkgObject")));
		Shaders.Add("BkgFloor", FShader.CreateShader("BkgFloor", Shader.Find("Futile/BkgFloor")));
		Shaders.Add("House", FShader.CreateShader("House", Shader.Find("Futile/House")));
		Shaders.Add("DistantBkgObjectRepeatHorizontal", FShader.CreateShader("DistantBkgObjectRepeatHorizontal", Shader.Find("Futile/DistantBkgObjectRepeatHorizontal")));
		Shaders.Add("Dust", FShader.CreateShader("Dust", Shader.Find("Futile/Dust")));
		Shaders.Add("RoomTransition", FShader.CreateShader("RoomTransition", Shader.Find("Futile/RoomTransition")));
		Shaders.Add("VoidCeiling", FShader.CreateShader("VoidCeiling", Shader.Find("Futile/VoidCeiling")));
		Shaders.Add("FlatLightNoisy", FShader.CreateShader("FlatLightNoisy", Shader.Find("Futile/FlatLightNoisy")));
		Shaders.Add("VoidWormBody", FShader.CreateShader("VoidWormBody", Shader.Find("Futile/VoidWormBody")));
		Shaders.Add("VoidWormFin", FShader.CreateShader("VoidWormFin", Shader.Find("Futile/VoidWormFin")));
		Shaders.Add("VoidWormPincher", FShader.CreateShader("VoidWormPincher", Shader.Find("Futile/VoidWormPincher")));
		Shaders.Add("FlatWaterLight", FShader.CreateShader("FlatWaterLight", Shader.Find("Futile/FlatWaterLight")));
		Shaders.Add("FlatWaterLightBothSides", FShader.CreateShader("FlatWaterLightBothSides", Shader.Find("Futile/FlatWaterLight"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("FlatWaterLightRippleSpawn", FShader.CreateShader("FlatWaterLightRippleSpawn", Shader.Find("Futile/FlatWaterLight"), new string[1] { "rippleSpawn" }));
		Shaders.Add("FlatWaterLightRippleSpawnRippleSide", FShader.CreateShader("FlatWaterLightRippleSpawnRippleSide", Shader.Find("Futile/FlatWaterLight"), new string[2] { "rippleSpawn", "ripple_other_side" }));
		Shaders.Add("WormLayerFade", FShader.CreateShader("WormLayerFade", Shader.Find("Futile/WormLayerFade")));
		Shaders.Add("OverseerZip", FShader.CreateShader("OverseerZip", Shader.Find("Futile/OverseerZip")));
		Shaders.Add("GhostSkin", FShader.CreateShader("GhostSkin", Shader.Find("Futile/GhostSkin"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("GhostBall", FShader.CreateShader("GhostBall", Shader.Find("Futile/GhostBall"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("GhostDistortion", FShader.CreateShader("GhostDistortion", Shader.Find("Futile/GhostDistortion"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("GhostSkinRipple", FShader.CreateShader("GhostSkinRipple", Shader.Find("Futile/GhostSkin"), new string[1] { "ripple_other_side" }));
		Shaders.Add("GhostBallRipple", FShader.CreateShader("GhostBallRipple", Shader.Find("Futile/GhostBall"), new string[1] { "ripple_other_side" }));
		Shaders.Add("GhostDistortionRipple", FShader.CreateShader("GhostDistortionRipple", Shader.Find("Futile/GhostDistortion"), new string[1] { "ripple_other_side" }));
		Shaders.Add("GateHologram", FShader.CreateShader("GateHologram", Shader.Find("Futile/GateHologram")));
		Shaders.Add("OutPostAntler", FShader.CreateShader("OutPostAntler", Shader.Find("Futile/OutPostAntler")));
		Shaders.Add("WaterNut", FShader.CreateShader("WaterNut", Shader.Find("Futile/WaterNut")));
		Shaders.Add("Hologram", FShader.CreateShader("Hologram", Shader.Find("Futile/Hologram")));
		Shaders.Add("HologramBothSides", FShader.CreateShader("HologramBothSides", Shader.Find("Futile/Hologram"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("FireSmoke", FShader.CreateShader("FireSmoke", Shader.Find("Futile/FireSmoke")));
		Shaders.Add("HoldButtonCircle", FShader.CreateShader("HoldButtonCircle", Shader.Find("Futile/HoldButtonCircle")));
		Shaders.Add("HoldButtonCircleFadable", FShader.CreateShader("HoldButtonCircleFadable", Shader.Find("Futile/HoldButtonCircleFadable")));
		Shaders.Add("GoldenGlow", FShader.CreateShader("GoldenGlow", Shader.Find("Futile/GoldenGlow")));
		Shaders.Add("ElectricDeath", FShader.CreateShader("ElectricDeath", Shader.Find("Futile/ElectricDeath")));
		Shaders.Add("VoidSpawnBody", FShader.CreateShader("VoidSpawnBody", Shader.Find("Futile/VoidSpawnBody")));
		Shaders.Add("SceneLighten", FShader.CreateShader("SceneLighten", Shader.Find("Futile/SceneLighten")));
		Shaders.Add("SceneBlurLightEdges", FShader.CreateShader("SceneBlurLightEdges", Shader.Find("Futile/SceneBlurLightEdges")));
		Shaders.Add("SceneRain", FShader.CreateShader("SceneRain", Shader.Find("Futile/SceneRain")));
		Shaders.Add("SceneOverlay", FShader.CreateShader("SceneOverlay", Shader.Find("Futile/SceneOverlay")));
		Shaders.Add("SceneSoftLight", FShader.CreateShader("SceneSoftLight", Shader.Find("Futile/SceneSoftLight")));
		Shaders.Add("SceneMultiply", FShader.CreateShader("SceneMultiply", Shader.Find("Futile/SceneMultiply")));
		Shaders.Add("HologramImage", FShader.CreateShader("HologramImage", Shader.Find("Futile/HologramImage")));
		Shaders.Add("HologramBehindTerrain", FShader.CreateShader("HologramBehindTerrain", Shader.Find("Futile/HologramBehindTerrain")));
		Shaders.Add("Decal", FShader.CreateShader("Decal", Shader.Find("Futile/Decal")));
		Shaders.Add("SpecificDepth", FShader.CreateShader("SpecificDepth", Shader.Find("Futile/SpecificDepth")));
		Shaders.Add("LocalBloom", FShader.CreateShader("LocalBloom", Shader.Find("Futile/LocalBloom")));
		Shaders.Add("MenuText", FShader.CreateShader("MenuText", Shader.Find("Futile/MenuText")));
		Shaders.Add("DeathFall", FShader.CreateShader("DeathFall", Shader.Find("Futile/DeathFall")));
		Shaders.Add("DeathFallHeavy", FShader.CreateShader("DeathFallHeavy", Shader.Find("Futile/DeathFall"), new string[1] { "strong" }));
		Shaders.Add("KingTusk", FShader.CreateShader("KingTusk", Shader.Find("Futile/KingTusk")));
		Shaders.Add("HoloGrid", FShader.CreateShader("HoloGrid", Shader.Find("Futile/HoloGrid")));
		Shaders.Add("SootMark", FShader.CreateShader("SootMark", Shader.Find("Futile/SootMark")));
		Shaders.Add("NewVultureSmoke", FShader.CreateShader("NewVultureSmoke", Shader.Find("Futile/NewVultureSmoke")));
		Shaders.Add("SmokeTrail", FShader.CreateShader("SmokeTrail", Shader.Find("Futile/SmokeTrail")));
		Shaders.Add("RedsIllness", FShader.CreateShader("RedsIllness", Shader.Find("Futile/RedsIllness")));
		Shaders.Add("HazerHaze", FShader.CreateShader("HazerHaze", Shader.Find("Futile/HazerHaze")));
		Shaders.Add("Rainbow", FShader.CreateShader("Rainbow", Shader.Find("Futile/Rainbow")));
		Shaders.Add("LightBeam", FShader.CreateShader("LightBeam", Shader.Find("Futile/LightBeam")));
		Shaders.Add("SlopedTerrainSurface", FShader.CreateShader("SlopedTerrainSurface", Shader.Find("Futile/SlopedTerrainSurface")));
		Shaders.Add("SlopedTerrainStain", FShader.CreateShader("SlopedTerrainStain", Shader.Find("Futile/SlopedTerrainStain")));
		Shaders.Add("Rubble", FShader.CreateShader("Rubble", Shader.Find("Futile/Rubble")));
		Shaders.Add("Whirlpool", FShader.CreateShader("Whirlpool", Shader.Find("Futile/Whirlpool")));
		Shaders.Add("GeyserWater", FShader.CreateShader("GeyserWater", Shader.Find("Futile/GeyserWater")));
		Shaders.Add("BackgroundAdditive", FShader.CreateShader("BackgroundAdditive", Shader.Find("Futile/BackgroundAdditive")));
		Shaders.Add("BackgroundJaggedCircle", FShader.CreateShader("BackgroundJaggedCircle", Shader.Find("Futile/BackgroundJaggedCircle")));
		Shaders.Add("BackgroundNoHoles", FShader.CreateShader("BackgroundNoHoles", Shader.Find("Futile/BackgroundNoHoles")));
		Shaders.Add("WaterCurrent", FShader.CreateShader("WaterCurrent", Shader.Find("Futile/WaterCurrent")));
		Shaders.Add("BlackSpot", FShader.CreateShader("BlackSpot", Shader.Find("Futile/BlackSpot")));
		Shaders.Add("SkyWhaleBody", FShader.CreateShader("SkyWhaleBody", Shader.Find("Futile/SkyWhaleBody")));
		Shaders.Add("SkyWhaleCuticle", FShader.CreateShader("SkyWhaleCuticle", Shader.Find("Futile/SkyWhaleCuticle")));
		Shaders.Add("KarmicShield", FShader.CreateShader("KarmicShield", Shader.Find("Futile/KarmicShield")));
		Shaders.Add("TemplarCircle", FShader.CreateShader("TemplarCircle", Shader.Find("Futile/TemplarCircle")));
		Shaders.Add("TemplarCloak", FShader.CreateShader("TemplarCloak", Shader.Find("Futile/TemplarCloak")));
		Shaders.Add("Sandstorm", FShader.CreateShader("Sandstorm", Shader.Find("Futile/Sandstorm")));
		Shaders.Add("SlopedTerrainMask", FShader.CreateShader("SlopedTerrainMask", Shader.Find("Futile/SlopedTerrainMask")));
		Shaders.Add("SlopedTerrainMaskGrab", FShader.CreateShader("SlopedTerrainMaskGrab", Shader.Find("Futile/SlopedTerrainMaskGrab")));
		Shaders.Add("DistantBkgObjectAlpha", FShader.CreateShader("DistantBkgObjectAlpha", Shader.Find("Futile/DistantBkgObjectAlpha")));
		Shaders.Add("WaterSlush", FShader.CreateShader("WaterSlush", Shader.Find("Futile/WaterSlush")));
		Shaders.Add("SporesSnow", FShader.CreateShader("SporesSnow", Shader.Find("Futile/SporesSnow")));
		Shaders.Add("SnowFall", FShader.CreateShader("SnowFall", Shader.Find("Futile/SnowFall")));
		Shaders.Add("OESphereTop", FShader.CreateShader("OESphereTop", Shader.Find("Futile/OESphereTop")));
		Shaders.Add("OESphereLight", FShader.CreateShader("OESphereLight", Shader.Find("Futile/OESphereLight")));
		Shaders.Add("OESphereBase", FShader.CreateShader("OESphereBase", Shader.Find("Futile/OESphereBase")));
		Shaders.Add("MoonProjection", FShader.CreateShader("MoonProjection", Shader.Find("Futile/MoonProjection")));
		Shaders.Add("LocalBlizzard", FShader.CreateShader("LocalBlizzard", Shader.Find("Futile/LocalBlizzard")));
		Shaders.Add("LightningBolt", FShader.CreateShader("LightningBolt", Shader.Find("Futile/LightningBolt")));
		Shaders.Add("LevelHeat", FShader.CreateShader("LevelHeat", Shader.Find("Futile/LevelColor"), new string[1] { "levelheat" }));
		Shaders.Add("FastSnowFall", FShader.CreateShader("FastSnowFall", Shader.Find("Futile/FastSnowFall")));
		Shaders.Add("FastLocalBlizzard", FShader.CreateShader("FastLocalBlizzard", Shader.Find("Futile/FastLocalBlizzard")));
		Shaders.Add("FastBlizzard", FShader.CreateShader("FastBlizzard", Shader.Find("Futile/FastBlizzard")));
		Shaders.Add("EnergySwirl", FShader.CreateShader("EnergySwirl", Shader.Find("Futile/EnergySwirl")));
		Shaders.Add("EnergyCell", FShader.CreateShader("EnergyCell", Shader.Find("Futile/EnergyCell")));
		Shaders.Add("DisplaySnowShader", FShader.CreateShader("DisplaySnowShader", Shader.Find("Futile/DisplaySnowShader")));
		Shaders.Add("BlizzardMapPrerender", FShader.CreateShader("BlizzardMapPrerender", Shader.Find("Futile/BlizzardMapPrerender")));
		Shaders.Add("Blizzard", FShader.CreateShader("Blizzard", Shader.Find("Futile/Blizzard")));
		Shaders.Add("LevelSnowShader", FShader.CreateShader("LevelSnowShader", Shader.Find("Futile/LevelSnowShader")));
		Shaders.Add("InterpolateWindMap", FShader.CreateShader("InterpolateWindMap", Shader.Find("Futile/InterpolateWindMap")));
		Shaders.Add("DustWaveLow", FShader.CreateShader("DustWaveLow", Shader.Find("Futile/DustWaveLow")));
		Shaders.Add("DustFlowRenderer", FShader.CreateShader("DustFlowRenderer", Shader.Find("Futile/DustFlowRenderer")));
		Shaders.Add("DustWave", FShader.CreateShader("DustWave", Shader.Find("Futile/DustWave")));
		Shaders.Add("DustWaveLevel", FShader.CreateShader("DustWaveLevel", Shader.Find("Futile/DustWaveLevel")));
		Shaders.Add("DustWaveLevelLow", FShader.CreateShader("DustWaveLevelLow", Shader.Find("Futile/DustWaveLevelLow")));
		Shaders.Add("BlizzardReduction", FShader.CreateShader("BlizzardReduction", Shader.Find("Futile/BlizzardReduction")));
		Shaders.Add("BlizzardMap", FShader.CreateShader("BlizzardMap", Shader.Find("Futile/BlizzardMap")));
		Shaders.Add("CellDist", FShader.CreateShader("CellDist", Shader.Find("Futile/CellDist")));
		Shaders.Add("DisplayWind", FShader.CreateShader("DisplayWind", Shader.Find("Futile/DisplayWind")));
		Shaders.Add("SingleGlyphHologram", FShader.CreateShader("SingleGlyphHologram", Shader.Find("Futile/SingleGlyphHologram")));
		Shaders.Add("WaterFallInverted", FShader.CreateShader("WaterFallInverted", Shader.Find("Futile/WaterFallInverted")));
		Shaders.Add("AquapedeBody", FShader.CreateShader("AquapedeBody", Shader.Find("Futile/AquapedeBody")));
		Shaders.Add("MenuTextGold", FShader.CreateShader("MenuTextGold", Shader.Find("Futile/MenuTextGold")));
		Shaders.Add("MenuTextCustom", FShader.CreateShader("MenuTextCustom", Shader.Find("Futile/MenuTextCustom")));
		Shaders.Add("WarpPointHoldFrame", FShader.CreateShader("WarpPointHoldFrame", Shader.Find("Futile/WarpPointHoldFrame")));
		Shaders.Add("Warp", FShader.CreateShader("Warp", Shader.Find("Futile/Warp")));
		Shaders.Add("WarpNoRing", FShader.CreateShader("WarpNoRing", Shader.Find("Futile/Warp"), new string[1] { "no_ring" }));
		Shaders.Add("WarpThroughDark", FShader.CreateShader("WarpThroughDark", Shader.Find("Futile/Warp"), new string[1] { "through_dark" }));
		Shaders.Add("WarpNoRingThroughDark", FShader.CreateShader("WarpThroughDark", Shader.Find("Futile/Warp"), new string[2] { "no_ring", "through_dark" }));
		Shaders.Add("WarpCircleBasic", FShader.CreateShader("WarpCircleBasic", Shader.Find("Futile/WarpCircle")));
		Shaders.Add("WarpCircleRipple", FShader.CreateShader("WarpCircleRipple", Shader.Find("Futile/WarpCircle"), new string[1] { "ripple_other_side" }));
		Shaders.Add("WarpPlayerDistortion", FShader.CreateShader("WarpPlayerDistortion", Shader.Find("Futile/WarpPlayerDistortion")));
		Shaders.Add("HugeTurbine", FShader.CreateShader("HugeTurbine", Shader.Find("Futile/HugeTurbine")));
		Shaders.Add("RotWormBody", FShader.CreateShader("RotWormBody", Shader.Find("Futile/RotWormBody")));
		Shaders.Add("RotWormFin", FShader.CreateShader("RotWormFin", Shader.Find("Futile/RotWormFin")));
		Shaders.Add("DynamicLevelElementGrab", FShader.CreateShader("DynamicLevelElementGrab", Shader.Find("Futile/DynamicLevelElementGrab")));
		Shaders.Add("DynamicLevelElement", FShader.CreateShader("DynamicLevelElement", Shader.Find("Futile/DynamicLevelElement")));
		Shaders.Add("DevUI_TwinArrowVertical", FShader.CreateShader("DevUI_TwinArrowVertical", Shader.Find("Futile/DevUIicons"), new string[1] { "twin_arrow_vertical" }));
		Shaders.Add("DevUI_TwinArrowHorizontal", FShader.CreateShader("DevUI_TwinArrowHorizontal", Shader.Find("Futile/DevUIicons"), new string[1] { "twin_arrow_horizontal" }));
		Shaders.Add("DevUIDepthPreview", FShader.CreateShader("DevUIDepthPreview", Shader.Find("Futile/DevUIDepthPreview")));
		Shaders.Add("DynamicLevelRock", FShader.CreateShader("DynamicLevelRock", Shader.Find("Futile/DynamicLevelRock")));
		Shaders.Add("DynamicLevelRock_NoMovement", FShader.CreateShader("DynamicLevelRock_NoMovement", Shader.Find("Futile/DynamicLevelRock"), new string[1] { "no_movement" }));
		Shaders.Add("DynamicLevelBlob", FShader.CreateShader("DynamicLevelBlob", Shader.Find("Futile/DynamicLevelBlob")));
		Shaders.Add("DynamicLevelGrass", FShader.CreateShader("DynamicLevelGrass", Shader.Find("Futile/DynamicLevelGrass")));
		Shaders.Add("GrassColor", FShader.CreateShader("GrassColor", Shader.Find("Futile/GrassColor")));
		Shaders.Add("DynamicLevelTubeSegment", FShader.CreateShader("DynamicLevelTubeSegment", Shader.Find("Futile/DynamicLevelTubeSegment")));
		Shaders.Add("DynamicLevelWire", FShader.CreateShader("DynamicLevelWire", Shader.Find("Futile/DynamicLevelWire")));
		Shaders.Add("DynamicLevelUrbanCandle", FShader.CreateShader("DynamicLevelUrbanCandle", Shader.Find("Futile/DynamicLevelUrbanCandle")));
		Shaders.Add("DynamicLevelBowl", FShader.CreateShader("DynamicLevelBowl", Shader.Find("Futile/DynamicLevelBowl")));
		Shaders.Add("DynamicLevelPole", FShader.CreateShader("DynamicLevelPole", Shader.Find("Futile/DynamicLevelPole")));
		Shaders.Add("Aurora", FShader.CreateShader("Aurora", Shader.Find("Futile/Aurora")));
		Shaders.Add("AuroraForeground", FShader.CreateShader("AuroraForeground", Shader.Find("Futile/Aurora"), new string[1] { "foreground" }));
		Shaders.Add("AuroraRipple", FShader.CreateShader("AuroraRipple", Shader.Find("Futile/Aurora"), new string[1] { "ripple_other_side" }));
		Shaders.Add("AuroraForegroundRipple", FShader.CreateShader("AuroraForegroundRipple", Shader.Find("Futile/Aurora"), new string[2] { "ripple_other_side", "foreground" }));
		Shaders.Add("WeaverFeather", FShader.CreateShader("WeaverFeather", Shader.Find("Futile/WeaverFeather")));
		Shaders.Add("WeaverGlow", FShader.CreateShader("WeaverGlow", Shader.Find("Futile/WeaverGlow")));
		Shaders.Add("WeaverGlowNoGrab", FShader.CreateShader("WeaverGlowNoGrab", Shader.Find("Futile/WeaverGlow"), new string[1] { "NO_GRAB" }));
		Shaders.Add("RippleSpawnBody", FShader.CreateShader("RippleSpawnBody", Shader.Find("Futile/RippleSpawnBody")));
		Shaders.Add("RippleSpawnAbsorption", FShader.CreateShader("RippleSpawnAbsorption", Shader.Find("Futile/RippleSpawnBody"), new string[1] { "absorption" }));
		Shaders.Add("RippleSpawnCreature", FShader.CreateShader("RippleSpawnCreature", Shader.Find("Futile/RippleSpawnCreature")));
		Shaders.Add("RippleSpawnBodyRippleSide", FShader.CreateShader("RippleSpawnBodyRippleSide", Shader.Find("Futile/RippleSpawnBody"), new string[1] { "ripple_other_side" }));
		Shaders.Add("RippleJellyBody", FShader.CreateShader("RippleJellyBody", Shader.Find("Futile/RippleJellyBody")));
		Shaders.Add("RippleJellyBodyRippleSide", FShader.CreateShader("RippleJellyBodyRippleSide", Shader.Find("Futile/RippleJellyBody"), new string[1] { "ripple_other_side" }));
		Shaders.Add("RippleGlow", FShader.CreateShader("RippleGlow", Shader.Find("Futile/RippleGlow")));
		Shaders.Add("RippleGlowRippleSide", FShader.CreateShader("RippleGlowRippleSide", Shader.Find("Futile/RippleGlow"), new string[1] { "ripple_other_side" }));
		Shaders.Add("RippleDeath", FShader.CreateShader("RippleDeath", Shader.Find("Futile/RippleDeath")));
		Shaders.Add("BrainBall", FShader.CreateShader("BrainBall", Shader.Find("Futile/BrainBall")));
		Shaders.Add("BrainStem", FShader.CreateShader("BrainStem", Shader.Find("Futile/BrainStem")));
		Shaders.Add("BrainMold", FShader.CreateShader("BrainMold", Shader.Find("Futile/BrainMold")));
		Shaders.Add("BrainBallDark", FShader.CreateShader("BrainBallDark", Shader.Find("Futile/BrainBallDark")));
		Shaders.Add("BrainStemDark", FShader.CreateShader("BrainStemDark", Shader.Find("Futile/BrainStemDark")));
		Shaders.Add("LocustCluster", FShader.CreateShader("LocustCluster", Shader.Find("Futile/LocustCluster")));
		Shaders.Add("LocustClusterShadow", FShader.CreateShader("LocustClusterShadow", Shader.Find("Futile/LocustClusterShadow")));
		Shaders.Add("AncientUrbanBuilding", FShader.CreateShader("AncientUrbanBuilding", Shader.Find("Futile/AncientUrbanBuilding")));
		Shaders.Add("DustGradient", FShader.CreateShader("DustGradient", Shader.Find("Futile/DustGradient")));
		Shaders.Add("BoxWormBody", FShader.CreateShader("BoxWormBody", Shader.Find("Futile/BoxWorm"), new string[1] { "Body" }));
		Shaders.Add("BoxWormLarvaHolder", FShader.CreateShader("BoxWormLarvaHolder", Shader.Find("Futile/BoxWorm"), new string[1] { "LarvaHolder" }));
		Shaders.Add("BoxWormBox", FShader.CreateShader("BoxWormBox", Shader.Find("Futile/BoxWorm"), new string[1] { "Box" }));
		Shaders.Add("BoxWormOpenBox", FShader.CreateShader("BoxWormOpenBox", Shader.Find("Futile/BoxWorm"), new string[1] { "OpenBox" }));
		Shaders.Add("BoxWormFakeLarva", FShader.CreateShader("BoxWormFakeLarva", Shader.Find("Futile/BoxWorm"), new string[1] { "Larva" }));
		Shaders.Add("BoxWormLarvaFood", FShader.CreateShader("BoxWormLarvaFood", Shader.Find("Futile/BoxWorm"), new string[1] { "LarvaFood" }));
		Shaders.Add("FireSpriteWing", FShader.CreateShader("FireSpriteWing", Shader.Find("Futile/BoxWorm"), new string[1] { "FireSpriteWing" }));
		Shaders.Add("FireSpriteBody", FShader.CreateShader("FireSpriteBody", Shader.Find("Futile/BoxWorm"), new string[1] { "FireSpriteBody" }));
		Shaders.Add("RippleHybrid", FShader.CreateShader("RippleHybrid", Shader.Find("Futile/RippleHybrid")));
		Shaders.Add("RippleHybridRipple", FShader.CreateShader("RippleHybridRipple", Shader.Find("Futile/RippleHybrid"), new string[1] { "ripple_other_side" }));
		Shaders.Add("RippleHybridBoth", FShader.CreateShader("RippleHybridBoth", Shader.Find("Futile/RippleHybrid"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("FlameJet", FShader.CreateShader("FlameJet", Shader.Find("Futile/FlameJet")));
		Shaders.Add("FlameJetGlow", FShader.CreateShader("FlameJetGlow", Shader.Find("Futile/FlameJetGlow"), new string[1] { "bloom" }));
		Shaders.Add("SaltFlake", FShader.CreateShader("SaltFlake", Shader.Find("Futile/SaltFlake")));
		Shaders.Add("SaltFlakeShadow", FShader.CreateShader("SaltFlakeShadow", Shader.Find("Futile/SaltFlake"), new string[1] { "shadow" }));
		Shaders.Add("AetherRainbow", FShader.CreateShader("AetherRainbow", Shader.Find("Futile/AetherRainbow")));
		Shaders.Add("AetherRainbowMinimalGrain", FShader.CreateShader("AetherRainbowMinimalGrain", Shader.Find("Futile/AetherRainbowMinimalGrain")));
		Shaders.Add("GildedWind", FShader.CreateShader("GildedWind", Shader.Find("Futile/GildedWind")));
		Shaders.Add("Stardust", FShader.CreateShader("Stardust", Shader.Find("Futile/GildedWind"), new string[1] { "stardust" }));
		Shaders.Add("BackgroundDune", FShader.CreateShader("BackgroundDune", Shader.Find("Futile/BackgroundDune")));
		Shaders.Add("OuterRimBackgroundBuilding", FShader.CreateShader("OuterRimBackgroundBuilding", Shader.Find("Futile/OuterRimBackgroundBuilding")));
		Shaders.Add("OuterRimDustGradient", FShader.CreateShader("OuterRimDustGradient", Shader.Find("Futile/OuterRimDustGradient")));
		Shaders.Add("FallingStar", FShader.CreateShader("FallingStar", Shader.Find("Futile/FallingStar")));
		Shaders.Add("WallLight", FShader.CreateShader("WallLight", Shader.Find("Futile/WallLight"), new string[1] { "softShadow" }));
		Shaders.Add("WallLightSoftEdge", FShader.CreateShader("WallLightSoftEdge", Shader.Find("Futile/WallLight"), new string[2] { "softShadow", "softEdge" }));
		Shaders.Add("WallLightStaticNoise", FShader.CreateShader("WallLightWallLightStaticNoise", Shader.Find("Futile/WallLight"), new string[2] { "softShadow", "staticNoise" }));
		Shaders.Add("WallLightHardShadow", FShader.CreateShader("WallLightHardShadow", Shader.Find("Futile/WallLight")));
		Shaders.Add("WallLightFlat", FShader.CreateShader("WallLightFlat", Shader.Find("Futile/WallLightFlat")));
		Shaders.Add("WarpTearMask", FShader.CreateShader("WarpTearMask", Shader.Find("Futile/WarpTearMask")));
		Shaders.Add("WarpTearBlocker", FShader.CreateShader("WarpTearBlocker", Shader.Find("Futile/WarpTearBlocker")));
		Shaders.Add("WarpTear", FShader.CreateShader("WarpTear", Shader.Find("Futile/WarpTear")));
		Shaders.Add("WarpTearBad", FShader.CreateShader("WarpTearBad", Shader.Find("Futile/WarpTear"), new string[1] { "bad" }));
		Shaders.Add("WarpTearOuter", FShader.CreateShader("WarpTearOuter", Shader.Find("Futile/WarpTear"), new string[1] { "outer" }));
		Shaders.Add("WarpTearRippleSide", FShader.CreateShader("WarpTearRippleSide", Shader.Find("Futile/WarpTear"), new string[1] { "ripple_other_side" }));
		Shaders.Add("WarpTearGrab", FShader.CreateShader("WarpTearGrab", Shader.Find("Futile/WarpTearGrab")));
		Shaders.Add("WeaverThread", FShader.CreateShader("WeaverThread", Shader.Find("Futile/WeaverThread")));
		Shaders.Add("WeaverThreadRipple", FShader.CreateShader("WeaverThreadRipple", Shader.Find("Futile/WeaverThread"), new string[1] { "ripple_other_side" }));
		Shaders.Add("WeaverThreadBoth", FShader.CreateShader("WeaverThreadBoth", Shader.Find("Futile/WeaverThread"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("SKLightning", FShader.CreateShader("SKLightning", Shader.Find("Futile/SKLightning")));
		Shaders.Add("SKLightningForeground", FShader.CreateShader("SKLightningForeground", Shader.Find("Futile/SKLightning"), new string[1] { "foreground" }));
		Shaders.Add("SKLightningBGFlash", FShader.CreateShader("SKLightningBGFlash", Shader.Find("Futile/SKLightning"), new string[1] { "flash" }));
		Shaders.Add("Darken", FShader.CreateShader("Darken", Shader.Find("Futile/Darken")));
		Shaders.Add("SKLightningFlash", FShader.CreateShader("SKLightningFlash", Shader.Find("Futile/LightSource"), new string[1] { "bright" }));
		Shaders.Add("PrinceStem", FShader.CreateShader("PrinceStem", Shader.Find("Futile/PrinceStem")));
		Shaders.Add("SkinkStripes", FShader.CreateShader("SkinkStripes", Shader.Find("Futile/SkinkStripes")));
		Shaders.Add("MothWing", FShader.CreateShader("MothWing", Shader.Find("Futile/MothWing")));
		Shaders.Add("MudDecal", FShader.CreateShader("MudDecal", Shader.Find("Futile/MudDecal")));
		Shaders.Add("MudPit", FShader.CreateShader("MudPit", Shader.Find("Futile/MudPit")));
		Shaders.Add("MudPitMask", FShader.CreateShader("MudPitMask", Shader.Find("Futile/MudPitMask")));
		Shaders.Add("MudOverlay", FShader.CreateShader("MudOverlay", Shader.Find("Futile/MudOverlay")));
		Shaders.Add("UrbanLife", FShader.CreateShader("UrbanLife", Shader.Find("Futile/UrbanLife")));
		Shaders.Add("UrbanLifeShadow", FShader.CreateShader("UrbanLifeShadow", Shader.Find("Futile/UrbanLife"), new string[1] { "shadow" }));
		Shaders.Add("UrbanLifeFirstLayer", FShader.CreateShader("UrbanLifeFirstLayer", Shader.Find("Futile/UrbanLife"), new string[1] { "first" }));
		Shaders.Add("UrbanShadowsGrab", FShader.CreateShader("UrbanShadowsGrab", Shader.Find("Futile/UrbanShadowsGrab")));
		Shaders.Add("UrbanShadowsBlur", FShader.CreateShader("UrbanShadowsBlur", Shader.Find("Futile/UrbanShadowsBlur")));
		Shaders.Add("UrbanShadowsBlurGrab", FShader.CreateShader("UrbanShadowsBlurGrab", Shader.Find("Futile/UrbanShadowsBlurGrab")));
		Shaders.Add("UrbanShadowGradient", FShader.CreateShader("UrbanShadowGradient", Shader.Find("Futile/UrbanShadowGradient")));
		Shaders.Add("SpinToy", FShader.CreateShader("SpinToy", Shader.Find("Futile/SpinToy")));
		Shaders.Add("SpinToyGlyph", FShader.CreateShader("SpinToyGlyph", Shader.Find("Futile/SpinToy"), new string[1] { "glyph" }));
		Shaders.Add("BallToy", FShader.CreateShader("BallToy", Shader.Find("Futile/BallToy")));
		Shaders.Add("SoftToyBody", FShader.CreateShader("SoftToyBody", Shader.Find("Futile/SoftToy")));
		Shaders.Add("SoftToyEye", FShader.CreateShader("SoftToyEye", Shader.Find("Futile/SoftToy"), new string[1] { "eye" }));
		Shaders.Add("GreebleGrid", FShader.CreateShader("GreebleGrid", Shader.Find("Futile/GreebleGrid")));
		Shaders.Add("PlaceholderBackgroundElement", FShader.CreateShader("PlaceholderBackgroundElement", Shader.Find("Futile/PlaceholderBackgroundElement")));
		Shaders.Add("FirmamentCloud", FShader.CreateShader("FirmamentCloud", Shader.Find("Futile/FirmamentCloud")));
		Shaders.Add("DustDunes", FShader.CreateShader("DustDunes", Shader.Find("Futile/DustDunes")));
		Shaders.Add("CamoMeter", FShader.CreateShader("CamoMeter", Shader.Find("Futile/CamoMeter")));
		Shaders.Add("UrbanCandleSSS", FShader.CreateShader("UrbanCandleSSS", Shader.Find("Futile/UrbanCandleSSS")));
		Shaders.Add("UrbanCandleFlame", FShader.CreateShader("UrbanCandleFlame", Shader.Find("Futile/UrbanCandleFlame")));
		Shaders.Add("DeepLightSource", FShader.CreateShader("DeepLightSource", Shader.Find("Futile/LightSource"), new string[1] { "deepLight" }));
		Shaders.Add("PoisonSpearTip", FShader.CreateShader("PoisonSpearTip", Shader.Find("Futile/PoisonSpearTip")));
		Shaders.Add("GrappleSnake", FShader.CreateShader("GrappleSnake", Shader.Find("Futile/GrappleSnake")));
		Shaders.Add("ARZapper", FShader.CreateShader("ARZapper", Shader.Find("Futile/ARZapper")));
		Shaders.Add("ARZapperOmni", FShader.CreateShader("ARZapperOmni", Shader.Find("Futile/ARZapper"), new string[1] { "omniDirectional" }));
		Shaders.Add("ARZapperOneSide", FShader.CreateShader("ARZapperOneSide", Shader.Find("Futile/ARZapper"), new string[1] { "oneEndConnected" }));
		Shaders.Add("ARZapperGlow", FShader.CreateShader("ARZapperGlow", Shader.Find("Futile/ARZapper"), new string[1] { "glow" }));
		Shaders.Add("PearlReaderGrab", FShader.CreateShader("PearlReaderGrab", Shader.Find("Futile/PearlReaderGrab")));
		Shaders.Add("PearlReaderProjection", FShader.CreateShader("PearlReaderProjection", Shader.Find("Futile/PearlReaderProjection")));
		Shaders.Add("PearlReaderProjectionOpaque", FShader.CreateShader("PearlReaderProjectionOpaque", Shader.Find("Futile/PearlReaderProjection"), new string[1] { "opaque" }));
		Shaders.Add("PearlReaderProjectionDebug", FShader.CreateShader("PearlReaderProjectionDebug", Shader.Find("Futile/PearlReaderProjection"), new string[1] { "debug" }));
		Shaders.Add("PearlReaderContent", FShader.CreateShader("PearlReaderContent", Shader.Find("Futile/PearlReaderContent")));
		Shaders.Add("PearlReaderProjectionGlow", FShader.CreateShader("PearlReaderProjectionGlow", Shader.Find("Futile/PearlReaderProjectionGlow")));
		Shaders.Add("PearlReaderProjectionRay", FShader.CreateShader("PearlReaderProjectionRay", Shader.Find("Futile/PearlReaderProjectionGlow"), new string[1] { "ray" }));
		Shaders.Add("PearlReaderArm", FShader.CreateShader("PearlReaderArm", Shader.Find("Futile/PearlReaderArm")));
		Shaders.Add("PearlReaderHolder", FShader.CreateShader("PearlReaderHolder", Shader.Find("Futile/PearlReaderHolder")));
		Shaders.Add("VWEndingFloor", FShader.CreateShader("VWEndingFloor", Shader.Find("Futile/VWEndingFloor")));
		Shaders.Add("VWEndingHaze", FShader.CreateShader("VWEndingHaze", Shader.Find("Futile/VWEndingHaze")));
		Shaders.Add("VWEndingShard", FShader.CreateShader("VWEndingShard", Shader.Find("Futile/VWEndingShard")));
		Shaders.Add("WatcherMorphBody", FShader.CreateShader("WatcherMorphBody", Shader.Find("Futile/WatcherMorphBody")));
		Shaders.Add("WatcherMorphFin", FShader.CreateShader("WatcherMorphFin", Shader.Find("Futile/WatcherMorphFin")));
		Shaders.Add("UnderWaterLightStrong", FShader.CreateShader("UnderWaterLightStrong", Shader.Find("Futile/UnderWaterLight"), new string[1] { "strong" }));
		Shaders.Add("DaemonEye", FShader.CreateShader("DaemonEye", Shader.Find("Futile/DaemonEye")));
		Shaders.Add("DynamicLevelDaemonCrown", FShader.CreateShader("DynamicLevelDaemonCrown", Shader.Find("Futile/DynamicLevelDaemonCrown")));
		Shaders.Add("BigSkyWhaleSkin", FShader.CreateShader("BigSkyWhaleSkin", Shader.Find("Futile/BigSkyWhaleSkin")));
		Shaders.Add("BigSkyWhaleEyelids", FShader.CreateShader("BigSkyWhaleEyelids", Shader.Find("Futile/BigSkyWhaleEyelids")));
		Shaders.Add("BigSkyWhaleGills", FShader.CreateShader("BigSkyWhaleGills", Shader.Find("Futile/BigSkyWhaleGills")));
		Shaders.Add("BigSkyWhaleBaleen", FShader.CreateShader("BigSkyWhaleBaleen", Shader.Find("Futile/BigSkyWhaleBaleen")));
		Shaders.Add("Monolith", FShader.CreateShader("Monolith", Shader.Find("Futile/Monolith")));
		Shaders.Add("MonolithDetailed", FShader.CreateShader("MonolithDetailed", Shader.Find("Futile/MonolithDetailed")));
		Shaders.Add("MonolithSphere", FShader.CreateShader("MonolithSphere", Shader.Find("Futile/MonolithSphere")));
		Shaders.Add("MonolithLine", FShader.CreateShader("MonolithLine", Shader.Find("Futile/MonolithLine")));
		Shaders.Add("MonolithGlimmer", FShader.CreateShader("MonolithGlimmer", Shader.Find("Futile/MonolithGlimmer")));
		Shaders.Add("MonolithFly", FShader.CreateShader("MonolithFly", Shader.Find("Futile/MonolithFly")));
		Shaders.Add("Gradient", FShader.CreateShader("Gradient", Shader.Find("Futile/Gradient")));
		Shaders.Add("MothGrubStripes", FShader.CreateShader("MothGrubStripes", Shader.Find("Futile/MothGrubStripes")));
		Shaders.Add("ShiftMask", FShader.CreateShader("ShiftMask", Shader.Find("Futile/RippleMask")));
		Shaders.Add("WavesShiftMask", FShader.CreateShader("WavesShiftMask", Shader.Find("Futile/RippleMask"), new string[1] { "waves" }));
		Shaders.Add("RippleTearMask", FShader.CreateShader("RippleTearMask", Shader.Find("Futile/RippleMask"), new string[1] { "tear" }));
		Shaders.Add("RippleRingMask", FShader.CreateShader("RippleRingMask", Shader.Find("Futile/RippleMask"), new string[1] { "ring" }));
		Shaders.Add("RippleBubbleMask", FShader.CreateShader("RippleBubbleMask", Shader.Find("Futile/RippleMask"), new string[1] { "bubble" }));
		Shaders.Add("TransitionRippleMask", FShader.CreateShader("TransitionRippleMask", Shader.Find("Futile/TransitionRippleMask")));
		Shaders.Add("RippleFlow", FShader.CreateShader("RippleFlow", Shader.Find("Futile/RippleFlow")));
		Shaders.Add("RippleGrab", FShader.CreateShader("RippleGrab", Shader.Find("Futile/RippleGrab")));
		Shaders.Add("GameplayRippleGrab", FShader.CreateShader("GameplayRippleGrab", Shader.Find("Futile/GameplayRippleGrab")));
		Shaders.Add("RippleBasic", FShader.CreateShader("RippleBasic", Shader.Find("Futile/RippleMaskedBasic")));
		Shaders.Add("RippleBasicRippleSide", FShader.CreateShader("RippleBasicRippleSide", Shader.Find("Futile/RippleMaskedBasic"), new string[1] { "ripple_other_side" }));
		Shaders.Add("RippleBasicRippleSideAlt", FShader.CreateShader("RippleBasicRippleSideAlt", Shader.Find("Futile/RippleMaskedBasic"), new string[1] { "ripple_other_side_alt" }));
		Shaders.Add("RippleBasicBothSides", FShader.CreateShader("RippleBasicBothSides", Shader.Find("Futile/RippleMaskedBasic"), new string[1] { "ripple_both_sides" }));
		Shaders.Add("RippleBasicClipDistortion", FShader.CreateShader("RippleBasicHideOnDistortion", Shader.Find("Futile/RippleMaskedBasic"), new string[1] { "ripple_clip_distortion" }));
		Shaders.Add("PlayerCamoMask", FShader.CreateShader("PlayerCamoMask", Shader.Find("Futile/PlayerCamoMask")));
		Shaders.Add("PlayerCamoMaskBeforePlayer", FShader.CreateShader("PlayerCamoMaskBeforePlayer", Shader.Find("Futile/PlayerCamoMaskBeforePlayer")));
		Shaders.Add("PlayerSimpleCamo", FShader.CreateShader("PlayerSimpleCamo", Shader.Find("Futile/PlayerSimpleCamo")));
		Shaders.Add("PlayerRippleTrail", FShader.CreateShader("PlayerRippleTrail", Shader.Find("Futile/PlayerRippleTrail")));
		Shaders.Add("RippleDepths", FShader.CreateShader("RippleDepths", Shader.Find("Futile/RippleDepths")));
		Shaders.Add("RippleVisions", FShader.CreateShader("RippleVisions", Shader.Find("Futile/RippleVisions")));
		Shaders.Add("RippleSimpleOverlay", FShader.CreateShader("RippleSimpleOverlay", Shader.Find("Futile/RippleSimpleOverlay")));
		Shaders.Add("RippleDialLevel", FShader.CreateShader("RippleDialLevel", Shader.Find("Futile/RippleDialLevel")));
		Shaders.Add("RippleTreeGlow", FShader.CreateShader("RippleTreeGlow", Shader.Find("Futile/RippleTreeGlow")));
		Shaders.Add("RippleDialSegment", FShader.CreateShader("RippleDialSegment", Shader.Find("Futile/RippleDialSegment")));
		LoadResources();
		InitGlobalShaderKeywords();
		Shader.SetGlobalColor(ShadPropMapWaterCol, MapWaterColor(DefaultWaterColor));
		Shader.SetGlobalVector(ShadPropScreenSize, screenSize);
		Shader.SetGlobalColor(ShadPropMapCol, MapColor);
		Shader.SetGlobalColor("_DevUIDepthPreviewColor", Color.red);
		Shader.SetGlobalColor("_RippleColor", RippleColor);
		Shader.SetGlobalColor("_RippleGold", RippleGold);
		Shader.SetGlobalColor("_GoldRGB", GoldRGB);
		Shader.SetGlobalFloat(ShadPropHologramThreshold, (Futile.screen.renderScale > 1) ? 0.5f : 0.65f);
		if (rewiredDelegateFunc == null)
		{
			if ((UnityEngine.Object)(object)rewiredInputManager == null)
			{
				rewiredInputManager = UnityEngine.Object.FindObjectOfType<InputManager>();
			}
			if ((UnityEngine.Object)(object)rewiredInputManager == null)
			{
				Custom.LogWarning("Could not find Rewired InputManager!");
			}
			MethodInfo method = ((object)rewiredInputManager).GetType().GetMethod("DoUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
			Delegate @delegate = Delegate.CreateDelegate(typeof(RewiredDoUpdateDelegate), rewiredInputManager, method);
			rewiredDelegateFunc = (RewiredDoUpdateDelegate)@delegate;
		}
		InGameTranslator.LoadFonts(inGameTranslator.currentLanguage, null);
		processManager = new ProcessManager(this);
		processManagerInitialized = true;
		processManagerInitializationStarted = false;
	}

	public void InitGlobalShaderKeywords()
	{
		Shader.DisableKeyword("RIPPLE");
		Shader.DisableKeyword("COMBINEDLEVEL");
		Shader.DisableKeyword("URBANLIFE");
		Shader.DisableKeyword("RoomHasBrainMold");
		Shader.DisableKeyword("RoomHasWater");
		Shader.DisableKeyword("RoomHasDeathFall");
		Shader.DisableKeyword("VOIDSEA");
		if (ModManager.MMF && Custom.rainWorld.options.quality == Options.Quality.LOW)
		{
			Shader.EnableKeyword("LOWEND");
		}
		else
		{
			Shader.DisableKeyword("LOWEND");
		}
	}

	public static FShader TryGetRippleMaskedShaderVariant(int layer, string shaderName)
	{
		FShader value = Custom.rainWorld.Shaders[shaderName];
		switch (layer)
		{
		case 0:
			return value;
		case -1:
			Custom.rainWorld.Shaders.TryGetValue(shaderName + "BothSides", out value);
			break;
		case 1:
			Custom.rainWorld.Shaders.TryGetValue(shaderName + "RippleSide", out value);
			break;
		}
		return value;
	}

	public static FShader TryGetRippleMaskedShaderVariant(AbstractCreature owner, string shaderName)
	{
		if (owner != null)
		{
			if (owner.rippleBothSides)
			{
				return TryGetRippleMaskedShaderVariant(-1, shaderName);
			}
			return TryGetRippleMaskedShaderVariant(owner.rippleLayer, shaderName);
		}
		return TryGetRippleMaskedShaderVariant(0, shaderName);
	}

	public static FShader TryGetRippleMaskedShaderVariant(AbstractPhysicalObject owner, string shaderName)
	{
		if (owner != null)
		{
			if (owner.rippleBothSides)
			{
				return TryGetRippleMaskedShaderVariant(-1, shaderName);
			}
			return TryGetRippleMaskedShaderVariant(owner.rippleLayer, shaderName);
		}
		return TryGetRippleMaskedShaderVariant(0, shaderName);
	}

	public static RainWorldGame.SetupValues LoadSetupValues(bool distributionBuild)
	{
		string path = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "setup.txt").ToLowerInvariant();
		int num = 0;
		int pink = 0;
		int green = 0;
		int blue = 0;
		int white = 0;
		int spears = 0;
		int flies = 0;
		int leeches = 0;
		int snails = 0;
		int vultures = 0;
		int lanternMice = 0;
		int cicadas = 0;
		int palette = 0;
		int num2 = 0;
		int num3 = 0;
		int fliesToWin = 4;
		int num4 = 1;
		int num5 = 0;
		int num6 = 0;
		int num7 = 1;
		int num8 = 1;
		int num9 = 0;
		int yellows = 0;
		int reds = 0;
		int spiders = 0;
		int num10 = 0;
		int garbageWorms = 0;
		int jetFish = 0;
		int black = 0;
		int seaLeeches = 0;
		int salamanders = 0;
		int bigEels = 0;
		int defaultSettingsScreen = 0;
		int num11 = 1;
		int deers = 0;
		int num12 = 0;
		int daddyLongLegs = 0;
		int tubeWorms = 0;
		int broLongLegs = 0;
		int tentaclePlants = 0;
		int poleMimics = 0;
		int mirosBirds = 0;
		int num13 = 1;
		int num14 = 0;
		int templeGuards = 0;
		int centipedes = 0;
		int num15 = 1;
		int gravityFlickerCycleMin = 8;
		int gravityFlickerCycleMax = 18;
		int num16 = 0;
		int scavengers = 0;
		int scavengersShy = 0;
		int scavengersLikePlayer = 0;
		int centiWings = 0;
		int smallCentipedes = 0;
		int num17 = 1;
		int lungs = 128;
		int num18 = 1;
		int cycleTimeMin = 400;
		int cycleTimeMax = 800;
		int cheatKarma = 0;
		int num19 = 0;
		int overseers = 0;
		int ghosts = 0;
		int fireSpears = 0;
		int scavLanterns = 0;
		int num20 = 0;
		int scavBombs = 0;
		int num21 = 0;
		int custom = 0;
		int bigSpiders = 0;
		int eggBugs = 0;
		int singlePlayerChar = -1;
		int needleWorms = 0;
		int smallNeedleWorms = 0;
		int spitterSpiders = 0;
		int dropbugs = 0;
		int cyanLizards = 0;
		int kingVultures = 0;
		int num22 = 0;
		int redCentis = 0;
		int proceedLineages = 0;
		bool forcePrecycles = false;
		string startMap = "";
		bool disableRain = false;
		bool testMoonFixed = false;
		bool testMoonCloak = false;
		bool unlockMSCCharacters = false;
		bool forcePup = false;
		bool cleanSpawns = false;
		bool cleanMaps = false;
		bool player = false;
		bool player2 = false;
		int artificerDreamTest = -1;
		bool saintInfinitePower = false;
		int slugPupsMax = -1;
		bool arenaDefaultColors = false;
		SlugcatStats.Name betaTestSlugcat = null;
		if (File.Exists(path) && (!distributionBuild || ModManager.DevTools))
		{
			num12 = 1;
			string[] array = File.ReadAllLines(path);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Contains(":"))
				{
					string[] array2 = Regex.Split(array[i], ":");
					string text = array2[1].Trim();
					short.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
					switch (array2[0].Trim())
					{
					case "player 2 active":
						num = result;
						continue;
					case "pink":
						pink = result;
						continue;
					case "green":
						green = result;
						continue;
					case "blue":
						blue = result;
						continue;
					case "white":
						white = result;
						continue;
					case "spears":
						spears = result;
						continue;
					case "flies":
						flies = result;
						continue;
					case "leeches":
						leeches = result;
						continue;
					case "snails":
						snails = result;
						continue;
					case "vultures":
						vultures = result;
						continue;
					case "lantern mice":
						lanternMice = result;
						continue;
					case "cicadas":
						cicadas = result;
						continue;
					case "palette":
						palette = result;
						continue;
					case "lizard laser eyes":
						num2 = result;
						continue;
					case "player invincibility":
						num3 = result;
						continue;
					case "cycle time min in seconds":
						cycleTimeMin = result;
						continue;
					case "cycle time max in seconds":
						cycleTimeMax = result;
						continue;
					case "flies to win":
						fliesToWin = result;
						continue;
					case "world creatures spawn":
						num4 = result;
						continue;
					case "bake":
						num5 = result;
						continue;
					case "widescreen":
						num6 = result;
						continue;
					case "start screen":
						num7 = result;
						continue;
					case "cycle startup":
						num8 = result;
						continue;
					case "full screen":
						num9 = result;
						continue;
					case "yellow":
						yellows = result;
						continue;
					case "red":
						reds = result;
						continue;
					case "spiders":
						spiders = result;
						continue;
					case "player glowing":
						num10 = result;
						continue;
					case "garbage worms":
						garbageWorms = result;
						continue;
					case "jet fish":
						jetFish = result;
						continue;
					case "black":
						black = result;
						continue;
					case "sea leeches":
						seaLeeches = result;
						continue;
					case "salamanders":
						salamanders = result;
						continue;
					case "big eels":
						bigEels = result;
						continue;
					case "default settings screen":
						defaultSettingsScreen = result;
						continue;
					case "player 1 active":
						num11 = result;
						continue;
					case "deer":
						deers = result;
						continue;
					case "dev tools active":
						num12 = result;
						continue;
					case "daddy long legs":
						daddyLongLegs = result;
						continue;
					case "tube worms":
						tubeWorms = result;
						continue;
					case "bro long legs":
						broLongLegs = result;
						continue;
					case "tentacle plants":
						tentaclePlants = result;
						continue;
					case "pole mimics":
						poleMimics = result;
						continue;
					case "miros birds":
						mirosBirds = result;
						continue;
					case "load game":
						num13 = result;
						continue;
					case "multi use gates":
						num14 = result;
						continue;
					case "temple guards":
						templeGuards = result;
						continue;
					case "centipedes":
						centipedes = result;
						continue;
					case "world":
						num15 = result;
						continue;
					case "gravity flicker cycle min":
						gravityFlickerCycleMin = result;
						continue;
					case "gravity flicker cycle max":
						gravityFlickerCycleMax = result;
						continue;
					case "reveal map":
						num16 = result;
						continue;
					case "scavengers":
						scavengers = result;
						continue;
					case "scavengers shy":
						scavengersShy = result;
						continue;
					case "scavenger like player":
						scavengersLikePlayer = result;
						continue;
					case "centiwings":
						centiWings = result;
						continue;
					case "small centipedes":
						smallCentipedes = result;
						continue;
					case "load progression":
						num17 = result;
						continue;
					case "lungs":
						lungs = result;
						continue;
					case "play music":
						num18 = result;
						continue;
					case "cheat karma":
						cheatKarma = result;
						continue;
					case "load all ambient sounds":
						num19 = result;
						continue;
					case "overseers":
						overseers = result;
						continue;
					case "ghosts":
						ghosts = result;
						continue;
					case "fire spears":
						fireSpears = result;
						continue;
					case "scavenger lanterns":
						scavLanterns = result;
						continue;
					case "always travel":
						num20 = result;
						continue;
					case "scavenger bombs":
						scavBombs = result;
						continue;
					case "the mark":
						num21 = result;
						continue;
					case "custom":
						custom = result;
						continue;
					case "big spiders":
						bigSpiders = result;
						continue;
					case "egg bugs":
						eggBugs = result;
						continue;
					case "single player character":
						singlePlayerChar = result;
						continue;
					case "needle worms":
						needleWorms = result;
						continue;
					case "small needle worms":
						smallNeedleWorms = result;
						continue;
					case "spitter spiders":
						spitterSpiders = result;
						continue;
					case "dropwigs":
						dropbugs = result;
						continue;
					case "cyan":
						cyanLizards = result;
						continue;
					case "king vultures":
						kingVultures = result;
						continue;
					case "log spawned creatures":
						num22 = result;
						continue;
					case "red centipedes":
						redCentis = result;
						continue;
					case "proceed lineages":
						proceedLineages = result;
						continue;
					case "start map":
						startMap = text;
						continue;
					case "no rain":
						disableRain = result == 1;
						continue;
					case "force moon fixed":
						testMoonFixed = result == 1;
						continue;
					case "force moon cloak":
						testMoonCloak = result == 1;
						continue;
					case "unlock msc characters":
						unlockMSCCharacters = result == 1;
						continue;
					case "force precycles":
						forcePrecycles = result == 1;
						continue;
					case "force pup":
						forcePup = result == 1;
						continue;
					case "clean spawns":
						cleanSpawns = result == 1;
						continue;
					case "clean map":
						cleanMaps = result == 1;
						continue;
					case "artificer dream":
						artificerDreamTest = result;
						continue;
					case "saint infinite power":
						saintInfinitePower = result == 1;
						continue;
					case "slugpup limit override":
						slugPupsMax = result;
						continue;
					case "beta test slugcat":
						betaTestSlugcat = new SlugcatStats.Name(text);
						continue;
					case "player 3 active":
						player = result == 1;
						continue;
					case "player 4 active":
						player2 = result == 1;
						continue;
					case "arena default colors":
						arenaDefaultColors = result == 1;
						continue;
					}
					Custom.LogWarning("Couldn't find option:", array2[0].Trim());
				}
			}
		}
		if (File.Exists((Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "arenacolors.txt").ToLowerInvariant()))
		{
			arenaDefaultColors = true;
		}
		RainWorldGame.SetupValues setupValues = new RainWorldGame.SetupValues(num > 0, pink, green, blue, white, spears, flies, leeches, snails, vultures, lanternMice, cicadas, palette, num2 != 0, num3 != 0, fliesToWin, num4 == 1, num5 == 1, num6 == 1, num7 == 1, num8 == 1, num9 == 1, yellows, reds, spiders, num10 == 1, garbageWorms, jetFish, black, seaLeeches, salamanders, bigEels, num11 == 1, defaultSettingsScreen, deers, num12 == 1, daddyLongLegs, tubeWorms, broLongLegs, tentaclePlants, poleMimics, mirosBirds, num13 == 1, num14 == 1, templeGuards, centipedes, num15 == 1, gravityFlickerCycleMin, gravityFlickerCycleMax, num16 == 1, scavengers, scavengersShy, scavengersLikePlayer, centiWings, smallCentipedes, num17 == 1, lungs, num18 == 1, cycleTimeMin, cycleTimeMax, cheatKarma, num19 == 1, overseers, ghosts, fireSpears, scavLanterns, num20 == 1, scavBombs, num21 == 1, custom, bigSpiders, eggBugs, singlePlayerChar, needleWorms, smallNeedleWorms, spitterSpiders, dropbugs, cyanLizards, kingVultures, num22 == 1, redCentis, proceedLineages);
		setupValues.betaTestSlugcat = betaTestSlugcat;
		setupValues.unlockMSCCharacters = unlockMSCCharacters;
		setupValues.testMoonCloak = testMoonCloak;
		setupValues.testMoonFixed = testMoonFixed;
		setupValues.disableRain = disableRain;
		setupValues.startMap = startMap;
		setupValues.forcePrecycles = forcePrecycles;
		setupValues.forcePup = forcePup;
		setupValues.cleanSpawns = cleanSpawns;
		setupValues.cleanMaps = cleanMaps;
		setupValues.artificerDreamTest = artificerDreamTest;
		setupValues.saintInfinitePower = saintInfinitePower;
		setupValues.slugPupsMax = slugPupsMax;
		setupValues.player3 = player;
		setupValues.player4 = player2;
		setupValues.arenaDefaultColors = arenaDefaultColors;
		RainWorldGame.SetupValues result2 = setupValues;
		MapExporter.RainWorld_LoadSetupValues(result2);
		return result2;
	}

	public PlayerHandler InstantiatePlayerHandler(int playerIndex)
	{
		PlayerHandler playerHandler = UnityEngine.Object.Instantiate(playerHandlerPrefab, base.transform, worldPositionStays: false);
		playerHandler.name = "PlayerHandler " + playerIndex;
		playerHandler.Initialize(this, playerIndex, (Profiles.ActiveProfiles.Count > playerIndex) ? Profiles.ActiveProfiles[playerIndex] : null);
		return playerHandler;
	}

	public PlayerHandler GetPlayerHandler(int playerNumber)
	{
		if (options == null || !options.optionsLoaded)
		{
			return null;
		}
		PlayerHandler handler = options.controls[Math.Max(playerNumber, 0)].handler;
		if (playerNumber <= 0 || (handler != null && !handler.SigningIn && handler.profile != null))
		{
			return handler;
		}
		return null;
	}

	public PlayerHandler GetPlayerHandlerRaw(int playerNumber)
	{
		if (options == null || !options.optionsLoaded)
		{
			return null;
		}
		return options.controls[Math.Max(playerNumber, 0)].handler;
	}

	public bool GetPlayerSigningIn(int playerNumber)
	{
		if (playerNumber <= 0 || options == null || !options.optionsLoaded || options.controls[playerNumber].handler == null)
		{
			return false;
		}
		return options.controls[playerNumber].handler.SigningIn;
	}

	public void RequestPlayerSignIn(int playerNumber, Joystick requestedJoystick)
	{
		if (options != null && options.optionsLoaded && !(options.controls[playerNumber].handler == null))
		{
			options.controls[playerNumber].SetActive(activeState: true);
			if (playerNumber > 0)
			{
				UserInput.targetPlayerIndexForNewProfile = playerNumber;
				targetPlayerForProfileAssignment = playerNumber;
				options.controls[playerNumber].handler.RequestSignIn(requestedJoystick);
			}
		}
	}

	public void ActivatePlayer(int playerNumber)
	{
		if (options != null && options.optionsLoaded && !(options.controls[playerNumber].handler == null))
		{
			options.controls[playerNumber].SetActive(activeState: true);
		}
	}

	public void DeactivatePlayer(int playerNumber)
	{
		if (playerNumber > 0 && options != null && options.optionsLoaded && !(options.controls[playerNumber].handler == null))
		{
			options.controls[playerNumber].SetActive(activeState: false);
			options.controls[playerNumber].handler.Deactivate();
		}
	}

	public void DeactivateAllPlayers()
	{
		if (options != null && options.optionsLoaded)
		{
			for (int i = 1; i < options.controls.Length; i++)
			{
				DeactivatePlayer(i);
			}
		}
	}

	public bool IsPlayerActive(int playerNumber)
	{
		if (playerNumber != 0 && !(GetPlayerHandler(playerNumber) != null))
		{
			return GetPlayerSigningIn(playerNumber);
		}
		return true;
	}

	public void ReloadProgression()
	{
		if (progression.progressionLoaded)
		{
			progressionBeingReloaded = true;
			progression.SaveWorldStateAndProgression(malnourished: false);
			dataOnProgressionReload = progression.GetCopyOfData();
			rawDataOnProgressionReload = progression.GetCopyOfRawData();
			progression.Destroy();
			progression = new PlayerProgression(this, tryLoad: true, saveAfterLoad: false);
		}
	}

	public void PingAchievement(AchievementID ID)
	{
		if (!AchievementAlreadyDisplayed(ID))
		{
			Custom.Log($"ACHIEVEMENT: {ID}");
			if (processManager.mySteamManager != null)
			{
				processManager.mySteamManager.SetAchievement(ID.ToString());
			}
		}
	}

	public void PingAchievementGOG(AchievementID ID)
	{
		if (!AchievementAlreadyDisplayedGOG(ID) && GogGalaxyManager.IsInitialized())
		{
			GogGalaxyManager.Instance.SetAchievement(ID.ToString());
		}
	}

	public void PingAchievementEpic(AchievementID ID)
	{
		if (Profiles.ActiveProfiles.Count > 0)
		{
			Achievements.UnlockAchievement(Profiles.ActiveProfiles[0], ID.ToString());
			return;
		}
		Custom.LogWarning("Couldn't set achievement as there's no active profile!");
	}

	public bool AchievementAlreadyDisplayed(AchievementID ID)
	{
		if (processManager.mySteamManager == null)
		{
			return false;
		}
		return processManager.mySteamManager.HasAchievement(ID.ToString());
	}

	public bool AchievementAlreadyDisplayedGOG(AchievementID ID)
	{
		if (!GogGalaxyManager.IsInitialized())
		{
			return false;
		}
		return GogGalaxyManager.Instance.GetAchievement(ID.ToString());
	}

	public void AchievementsLoad()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (Profiles.ActiveProfiles.Count > 0)
		{
			Achievements.OnAchievementsLoaded += new AchievementsLoaded(Achievements_OnAchievementsLoaded);
			List<AchievementData> list = new List<AchievementData>();
			for (int i = 0; i < achievementData.Length; i++)
			{
				if (i != 0)
				{
					list.Add(achievementData[i]);
				}
			}
			options.SetAchievementsFile();
			Achievements.LoadAchievements(Profiles.ActiveProfiles[0], list);
		}
		else
		{
			Custom.LogWarning("Couldn't load achievements as there is no active profile!");
			Platform.NotifyAchievementsLoadCompleted((object)this);
		}
	}

	public void Achievements_OnAchievementsLoaded(Profile profile, bool success)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		Achievements.OnAchievementsLoaded -= new AchievementsLoaded(Achievements_OnAchievementsLoaded);
		Array values = Enum.GetValues(typeof(AchievementID));
		for (int i = 0; i < values.Length; i++)
		{
			Achievements.GetAchievement(profile, ((AchievementID)values.GetValue(i)).ToString());
		}
		Platform.NotifyAchievementsLoadCompleted((object)this);
	}

	public void Achievements_OnAchievementRetrieved(Profile profile, IAchievement achievement)
	{
		AchievementID achievementID = AchievementID.None;
		try
		{
			achievementID = (AchievementID)Enum.Parse(typeof(AchievementID), achievement.id);
		}
		catch
		{
		}
		if (achievementID != 0)
		{
			if (achievementsUnlocked.ContainsKey(achievementID))
			{
				achievementsUnlocked[achievementID] = achievement.completed;
			}
			else
			{
				achievementsUnlocked.Add(achievementID, achievement.completed);
			}
		}
	}

	public void Achievements_OnAchievementUnlocked(Profile profile, IAchievement achievement)
	{
		AchievementID achievementID = AchievementID.None;
		try
		{
			achievementID = (AchievementID)Enum.Parse(typeof(AchievementID), achievement.id);
		}
		catch
		{
		}
		if (achievementID != 0)
		{
			if (achievementsUnlocked.ContainsKey(achievementID))
			{
				achievementsUnlocked[achievementID] = achievement.completed;
			}
			else
			{
				achievementsUnlocked.Add(achievementID, achievement.completed);
			}
		}
	}

	public void Platform_OnInitialized()
	{
		platformInitialized = true;
	}

	public IEnumerator InitializeAssetBundles()
	{
		AssetBundleManager.SetSourceAssetBundleDirectory("AssetBundles");
		yield return AssetBundleManager.Initialize();
		assetBundlesInitialized = true;
	}

	public void Platform_OnRequestAchievementsLoad(List<object> pendingAchievementsLoads)
	{
		pendingAchievementsLoads.Add(this);
		AchievementsLoad();
	}

	public bool IsPrimaryProfile(Profile profile)
	{
		if (profile == null)
		{
			return false;
		}
		if (Custom.rainWorld.options != null && Custom.rainWorld.options.optionsLoaded && Custom.rainWorld.options.controls[0].handler != null && Custom.rainWorld.options.controls[0].handler.profile != null)
		{
			return Custom.rainWorld.options.controls[0].handler.profile == profile;
		}
		return false;
	}

	public void Profiles_OnActivated(Profile profile)
	{
		if (options == null || !options.optionsLoaded || !options.controls[targetPlayerForProfileAssignment].controlSetupInitialized)
		{
			profilesWaitingToBeAssigned.Add(profile);
			targetPlayersWaitingToBeAssigned.Add(targetPlayerForProfileAssignment);
		}
		else
		{
			AssignProfile(profile, targetPlayerForProfileAssignment);
		}
	}

	public void AssignProfile(Profile profile, int targetPlayerIndex)
	{
		bool flag = false;
		for (int i = 0; i < options.controls.Length; i++)
		{
			if (options.controls[i].handler.profile != null && options.controls[i].handler.profile == profile)
			{
				flag = true;
				break;
			}
		}
		if (!flag && options.controls[targetPlayerIndex].handler.profile == null)
		{
			options.controls[targetPlayerIndex].handler.Initialize(this, targetPlayerIndex, profile);
		}
	}

	public void Profiles_OnDeactivated(Profile profile)
	{
		if (profilesWaitingToBeAssigned == null && profilesWaitingToBeAssigned.Count == 0)
		{
			for (int num = profilesWaitingToBeAssigned.Count - 1; num >= 0; num--)
			{
				if (profilesWaitingToBeAssigned[num] == profile)
				{
					profilesWaitingToBeAssigned.RemoveAt(num);
				}
			}
		}
		if (!options.optionsLoaded || options.controls == null || options.controls.Length == 0)
		{
			return;
		}
		for (int i = 0; i < options.controls.Length; i++)
		{
			if (options.controls[i].handler != null && options.controls[i].handler.profile == profile)
			{
				options.controls[i].handler.Deactivate();
			}
		}
	}

	public void JoystickConnected(ControllerStatusChangedEventArgs args)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (processManager != null && processManager.currentMainLoop is InputOptionsMenu)
		{
			(processManager.currentMainLoop as InputOptionsMenu).StopInputAssignment();
		}
		if (!ReInput.controllers.IsControllerAssigned(args.controllerType, args.controllerId))
		{
			ReInput.userDataStore.LoadControllerData(args.controllerType, args.controllerId);
		}
		if (options != null && options.optionsLoaded)
		{
			options.ReassignAllJoysticks();
		}
		if (processManager != null && processManager.currentMainLoop is InputOptionsMenu)
		{
			(processManager.currentMainLoop as InputOptionsMenu).UpdateConnectedControllerLabels();
			(processManager.currentMainLoop as InputOptionsMenu).RefreshInputGreyOut();
		}
		RWInput.RefreshSystemControllers();
	}

	public void JoystickPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (processManager == null || !(processManager.currentMainLoop is InputOptionsMenu))
		{
			return;
		}
		(processManager.currentMainLoop as InputOptionsMenu).StopInputAssignment();
		if (ReInput.controllers.IsControllerAssigned(args.controllerType, args.controllerId))
		{
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				if (allPlayer.id != 4 && allPlayer.id != 9999999 && allPlayer.controllers.ContainsController(args.controllerType, args.controllerId))
				{
					ReInput.userDataStore.SaveControllerData(allPlayer.id, args.controllerType, args.controllerId);
				}
			}
			return;
		}
		ReInput.userDataStore.SaveControllerData(args.controllerType, args.controllerId);
	}

	public void JoystickDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (options != null && options.optionsLoaded)
		{
			options.ReassignAllJoysticks();
		}
		if (processManager != null && processManager.currentMainLoop is InputOptionsMenu)
		{
			(processManager.currentMainLoop as InputOptionsMenu).UpdateConnectedControllerLabels();
			(processManager.currentMainLoop as InputOptionsMenu).RefreshInputGreyOut();
		}
		RWInput.RefreshSystemControllers();
	}

	public void UserInput_OnControllerDisconnected(Profile profile)
	{
		if (options == null || !options.optionsLoaded)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < options.controls.Length; i++)
		{
			if (options.controls[i].handler != null && options.controls[i].handler.profile != null && options.controls[i].handler.profile == profile)
			{
				num = i;
				break;
			}
		}
		if (num > -1 && !processManager.IsSwitchingProcesses())
		{
			if (processManager.currentMainLoop != null && processManager.currentMainLoop is RainWorldGame)
			{
				((RainWorldGame)processManager.currentMainLoop).ShowPauseMenu();
			}
			DialogControllerDisconnect dialog = new DialogControllerDisconnect(string.Format(inGameTranslator.Translate("controller_reconnect"), num + 1), processManager, num);
			processManager.ShowDialog(dialog);
		}
	}

	public List<SlugcatStats.Name> FilterTokenClearance(List<SlugcatStats.Name> newData, List<SlugcatStats.Name> oldData, List<SlugcatStats.Name> FilterSlots)
	{
		List<SlugcatStats.Name> list = new List<SlugcatStats.Name>();
		foreach (string entry in ExtEnum<SlugcatStats.Name>.values.entries)
		{
			SlugcatStats.Name item = new SlugcatStats.Name(entry);
			if (FilterSlots.Contains(item))
			{
				if (newData.Contains(item))
				{
					list.Add(item);
				}
			}
			else if (oldData.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void BuildTokenCache(bool modded, string region)
	{
		string text = (Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar).ToLowerInvariant();
		if (modded)
		{
			text = (Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "mergedmods" + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "IndexMaps").ToLowerInvariant();
			Directory.CreateDirectory(text);
			text += Path.DirectorySeparatorChar;
		}
		string fileName = region.ToLowerInvariant();
		lock (regionBlueTokens)
		{
			regionBlueTokens[fileName] = new List<MultiplayerUnlocks.SandboxUnlockID>();
			regionGoldTokens[fileName] = new List<MultiplayerUnlocks.LevelUnlockID>();
			regionGreenTokens[fileName] = new List<MultiplayerUnlocks.SlugcatUnlockID>();
			regionRedTokens[fileName] = new List<MultiplayerUnlocks.SafariUnlockID>();
			regionGreyTokens[fileName] = new List<ChatlogData.ChatlogID>();
			regionDataPearls[fileName] = new List<DataPearl.AbstractDataPearl.DataPearlType>();
			regionSpinningTopRooms[fileName] = new List<string>();
			regionWeaverRooms[fileName] = new List<string>();
			regionWarpRooms[fileName] = new List<string>();
			regionDynamicWarpTargets[fileName] = new List<string>();
			regionRippleSpawnEggs[fileName] = new Dictionary<string, int>();
			regionBlueTokensAccessibility[fileName] = new List<List<SlugcatStats.Name>>();
			regionGoldTokensAccessibility[fileName] = new List<List<SlugcatStats.Name>>();
			regionGreenTokensAccessibility[fileName] = new List<List<SlugcatStats.Name>>();
			regionRedTokensAccessibility[fileName] = new List<List<SlugcatStats.Name>>();
			regionDataPearlsAccessibility[fileName] = new List<List<SlugcatStats.Name>>();
		}
		string[] array = AssetManager.ListDirectory("World" + Path.DirectorySeparatorChar + region + "-Rooms");
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < array.Length; i++)
		{
			string fileName2 = Path.GetFileName(array[i]);
			if (fileName2.Contains("settings"))
			{
				list.Add(array[i]);
				if (fileName2.Contains("settings-"))
				{
					list2.Add(array[i]);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			List<SlugcatStats.Name> list3 = new List<SlugcatStats.Name>();
			if (list2.Contains(list[j]))
			{
				string text2 = Custom.ToTitleCase(Custom.GetBaseFileNameWithoutPrefix(list[j], "settings-"));
				if (ExtEnum<SlugcatStats.Name>.values.entries.Contains(text2))
				{
					list3.Add(new SlugcatStats.Name(text2));
				}
			}
			else
			{
				List<SlugcatStats.Name> list4 = new List<SlugcatStats.Name>();
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(list[j]);
				foreach (string item9 in list2)
				{
					if (item9 != list[j] && item9.Contains(fileNameWithoutExtension))
					{
						string text3 = Custom.ToTitleCase(Custom.GetBaseFileNameWithoutPrefix(list[j], "settings-"));
						if (ExtEnum<SlugcatStats.Name>.values.entries.Contains(text3))
						{
							list4.Add(new SlugcatStats.Name(text3));
						}
					}
				}
				foreach (string entry in ExtEnum<SlugcatStats.Name>.values.entries)
				{
					SlugcatStats.Name name = new SlugcatStats.Name(entry);
					if ((!ModManager.MSC || !(name == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)) && !list4.Contains(name))
					{
						list3.Add(name);
					}
				}
			}
			string[] array2 = File.ReadAllLines(list[j]);
			List<string[]> list5 = new List<string[]>();
			for (int k = 0; k < array2.Length; k++)
			{
				string[] array3 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[k], ":"), ": ");
				if (array3.Length == 2)
				{
					list5.Add(array3);
				}
			}
			for (int l = 0; l < list5.Count; l++)
			{
				if (!(list5[l][0] == "PlacedObjects"))
				{
					continue;
				}
				string[] array4 = Regex.Split(Custom.ValidateSpacedDelimiter(list5[l][1], ","), ", ");
				for (int m = 0; m < array4.Length; m++)
				{
					string[] array5 = Regex.Split(array4[m].Trim(), "><");
					if (array5.Length <= 1)
					{
						continue;
					}
					List<SlugcatStats.Name> oldData = new List<SlugcatStats.Name>();
					PlacedObject placedObject = new PlacedObject(PlacedObject.Type.None, null);
					try
					{
						placedObject.FromString(array5);
					}
					catch
					{
					}
					string fileName3 = Path.GetFileName(list[j].Substring(0, list[j].ToLowerInvariant().IndexOf("_settings")));
					if (placedObject.type == PlacedObject.Type.DataPearl || placedObject.type == PlacedObject.Type.UniqueDataPearl)
					{
						DataPearl.AbstractDataPearl.DataPearlType pearlType = (placedObject.data as PlacedObject.DataPearlData).pearlType;
						if (!(pearlType != DataPearl.AbstractDataPearl.DataPearlType.Misc) || !(pearlType != DataPearl.AbstractDataPearl.DataPearlType.Misc2) || !(pearlType != DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl) || !(pearlType != DataPearl.AbstractDataPearl.DataPearlType.Red_stomach) || !(pearlType != MoreSlugcatsEnums.DataPearlType.Rivulet_stomach) || !(pearlType != MoreSlugcatsEnums.DataPearlType.BroadcastMisc) || !(pearlType != MoreSlugcatsEnums.DataPearlType.Spearmasterpearl))
						{
							continue;
						}
						List<SlugcatStats.Name> newData = list3.Where((SlugcatStats.Name slugcat) => SlugcatStats.SlugcatStoryRegions(slugcat).Any((string x) => x.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))).ToList();
						if (!regionDataPearls[fileName].Contains(pearlType))
						{
							regionDataPearls[fileName].Add(pearlType);
							regionDataPearlsAccessibility[fileName].Add(FilterTokenClearance(newData, oldData, list3));
						}
						else
						{
							int index = regionDataPearls[fileName].IndexOf(pearlType);
							regionDataPearlsAccessibility[fileName][index] = FilterTokenClearance(newData, regionDataPearlsAccessibility[fileName][index], list3);
						}
					}
					else if (ModManager.Watcher && placedObject.type == WatcherEnums.PlacedObjectType.SpinningTopSpot)
					{
						int spawnIdentifier = (placedObject.data as SpinningTopData).spawnIdentifier;
						string destRoom = (placedObject.data as SpinningTopData).destRoom;
						string item = fileName3 + ":" + spawnIdentifier + ":" + ((destRoom == null) ? "NULL" : destRoom);
						if (!regionSpinningTopRooms[fileName].Contains(item))
						{
							regionSpinningTopRooms[fileName].Add(item);
						}
					}
					else if (ModManager.Watcher && placedObject.type == WatcherEnums.PlacedObjectType.WeaverSpot)
					{
						if (!regionWeaverRooms[fileName].Contains(fileName3))
						{
							regionWeaverRooms[fileName].Add(fileName3);
						}
					}
					else if (ModManager.Watcher && placedObject.type == PlacedObject.Type.WarpPoint)
					{
						string destRoom2 = (placedObject.data as WarpPoint.WarpPointData).destRoom;
						string item2 = fileName3 + ":" + ((placedObject.data as WarpPoint.WarpPointData).rippleWarp ? "1" : "0") + ":" + ((placedObject.data as WarpPoint.WarpPointData).oneWay ? "1" : "0") + ":" + ((destRoom2 == null) ? "NULL" : destRoom2);
						if (!regionWarpRooms[fileName].Contains(item2))
						{
							regionWarpRooms[fileName].Add(item2);
						}
					}
					else if (placedObject.type == PlacedObject.Type.DynamicWarpTarget)
					{
						float rippleLevelRequirement = (placedObject.data as DynamicWarpTargetData).rippleLevelRequirement;
						bool deadEnd = (placedObject.data as DynamicWarpTargetData).deadEnd;
						bool badWarp = (placedObject.data as DynamicWarpTargetData).badWarp;
						string item3 = fileName3 + ":" + rippleLevelRequirement + ":" + deadEnd + ":" + badWarp;
						if (!regionDynamicWarpTargets[fileName].Contains(item3))
						{
							regionDynamicWarpTargets[fileName].Add(item3);
						}
						if (!levelDynamicWarpTargets.ContainsKey(rippleLevelRequirement))
						{
							levelDynamicWarpTargets[rippleLevelRequirement] = new List<string>();
						}
						if (!levelDynamicWarpTargets[rippleLevelRequirement].Contains(fileName3))
						{
							levelDynamicWarpTargets[rippleLevelRequirement].Add(fileName3);
						}
						if (deadEnd && !levelDeadEndTargets.Contains(fileName3))
						{
							levelDeadEndTargets.Add(fileName3);
						}
						if (badWarp && !levelBadWarpTargets.Contains(fileName3))
						{
							levelBadWarpTargets.Add(fileName3);
						}
					}
					else if (placedObject.type == PlacedObject.Type.RippleSpawnEgg)
					{
						if (!regionRippleSpawnEggs[fileName].ContainsKey(fileName3))
						{
							regionRippleSpawnEggs[fileName][fileName3] = 1;
						}
						else
						{
							regionRippleSpawnEggs[fileName][fileName3]++;
						}
					}
					else if (placedObject.type == PlacedObject.Type.BlueToken && ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.entries.Contains((placedObject.data as CollectToken.CollectTokenData).tokenString))
					{
						MultiplayerUnlocks.SandboxUnlockID item4 = new MultiplayerUnlocks.SandboxUnlockID((placedObject.data as CollectToken.CollectTokenData).tokenString);
						if (!regionBlueTokens[fileName].Contains(item4))
						{
							regionBlueTokens[fileName].Add(item4);
							regionBlueTokensAccessibility[fileName].Add(FilterTokenClearance((placedObject.data as CollectToken.CollectTokenData).availableToPlayers, oldData, list3));
						}
						else
						{
							int index2 = regionBlueTokens[fileName].IndexOf(item4);
							regionBlueTokensAccessibility[fileName][index2] = FilterTokenClearance((placedObject.data as CollectToken.CollectTokenData).availableToPlayers, regionBlueTokensAccessibility[fileName][index2], list3);
						}
					}
					else if (placedObject.type == PlacedObject.Type.GoldToken && ExtEnum<MultiplayerUnlocks.LevelUnlockID>.values.entries.Contains((placedObject.data as CollectToken.CollectTokenData).tokenString))
					{
						MultiplayerUnlocks.LevelUnlockID item5 = new MultiplayerUnlocks.LevelUnlockID((placedObject.data as CollectToken.CollectTokenData).tokenString);
						if (!regionGoldTokens[fileName].Contains(item5))
						{
							regionGoldTokens[fileName].Add(item5);
							regionGoldTokensAccessibility[fileName].Add(FilterTokenClearance((placedObject.data as CollectToken.CollectTokenData).availableToPlayers, oldData, list3));
						}
						else
						{
							int index3 = regionGoldTokens[fileName].IndexOf(item5);
							regionGoldTokensAccessibility[fileName][index3] = FilterTokenClearance((placedObject.data as CollectToken.CollectTokenData).availableToPlayers, regionGoldTokensAccessibility[fileName][index3], list3);
						}
					}
					else if (ModManager.NewSlugcatsModule && placedObject.type == PlacedObject.Type.GreenToken && ExtEnum<MultiplayerUnlocks.SlugcatUnlockID>.values.entries.Contains((placedObject.data as CollectToken.CollectTokenData).tokenString))
					{
						MultiplayerUnlocks.SlugcatUnlockID item6 = new MultiplayerUnlocks.SlugcatUnlockID((placedObject.data as CollectToken.CollectTokenData).tokenString);
						if (!regionGreenTokens[fileName].Contains(item6))
						{
							regionGreenTokens[fileName].Add(item6);
							regionGreenTokensAccessibility[fileName].Add(FilterTokenClearance((placedObject.data as CollectToken.CollectTokenData).availableToPlayers, oldData, list3));
						}
						else
						{
							int index4 = regionGreenTokens[fileName].IndexOf(item6);
							regionGreenTokensAccessibility[fileName][index4] = FilterTokenClearance((placedObject.data as CollectToken.CollectTokenData).availableToPlayers, regionGreenTokensAccessibility[fileName][index4], list3);
						}
					}
					else
					{
						if (!ModManager.MSC)
						{
							continue;
						}
						if (placedObject.type == MoreSlugcatsEnums.PlacedObjectType.WhiteToken && ExtEnum<ChatlogData.ChatlogID>.values.entries.Contains((placedObject.data as CollectToken.CollectTokenData).tokenString))
						{
							ChatlogData.ChatlogID item7 = new ChatlogData.ChatlogID((placedObject.data as CollectToken.CollectTokenData).tokenString);
							if (!regionGreyTokens[fileName].Contains(item7))
							{
								regionGreyTokens[fileName].Add(item7);
							}
						}
						else if (placedObject.type == MoreSlugcatsEnums.PlacedObjectType.RedToken && ExtEnum<MultiplayerUnlocks.SafariUnlockID>.values.entries.Contains((placedObject.data as CollectToken.CollectTokenData).tokenString))
						{
							MultiplayerUnlocks.SafariUnlockID item8 = new MultiplayerUnlocks.SafariUnlockID((placedObject.data as CollectToken.CollectTokenData).tokenString);
							if (!regionRedTokens[fileName].Contains(item8))
							{
								regionRedTokens[fileName].Add(item8);
								regionRedTokensAccessibility[fileName].Add(FilterTokenClearance((placedObject.data as CollectToken.CollectTokenData).availableToPlayers, oldData, list3));
							}
							else
							{
								int index5 = regionRedTokens[fileName].IndexOf(item8);
								regionRedTokensAccessibility[fileName][index5] = FilterTokenClearance((placedObject.data as CollectToken.CollectTokenData).availableToPlayers, regionRedTokensAccessibility[fileName][index5], list3);
							}
						}
					}
				}
				break;
			}
		}
		string text4 = "";
		for (int n = 0; n < regionBlueTokens[fileName].Count; n++)
		{
			string text5 = string.Join("|", Array.ConvertAll(regionBlueTokensAccessibility[fileName][n].ToArray(), (SlugcatStats.Name x) => x.ToString()));
			text4 = text4 + regionBlueTokens[fileName][n]?.ToString() + "~" + text5;
			if (n != regionBlueTokens[fileName].Count - 1)
			{
				text4 += ",";
			}
		}
		text4 += "&";
		for (int num = 0; num < regionGoldTokens[fileName].Count; num++)
		{
			string text6 = string.Join("|", Array.ConvertAll(regionGoldTokensAccessibility[fileName][num].ToArray(), (SlugcatStats.Name x) => x.ToString()));
			text4 = text4 + regionGoldTokens[fileName][num]?.ToString() + "~" + text6;
			if (num != regionGoldTokens[fileName].Count - 1)
			{
				text4 += ",";
			}
		}
		text4 += "&";
		for (int num2 = 0; num2 < regionDataPearls[fileName].Count; num2++)
		{
			string text7 = string.Join("|", Array.ConvertAll(regionDataPearlsAccessibility[fileName][num2].ToArray(), (SlugcatStats.Name x) => x.ToString()));
			text4 = text4 + regionDataPearls[fileName][num2]?.ToString() + "~" + text7;
			if (num2 != regionDataPearls[fileName].Count - 1)
			{
				text4 += ",";
			}
		}
		if (ModManager.NewSlugcatsModule)
		{
			text4 += "&";
			for (int num3 = 0; num3 < regionGreenTokens[fileName].Count; num3++)
			{
				string text8 = string.Join("|", Array.ConvertAll(regionGreenTokensAccessibility[fileName][num3].ToArray(), (SlugcatStats.Name x) => x.ToString()));
				text4 = text4 + regionGreenTokens[fileName][num3]?.ToString() + "~" + text8;
				if (num3 != regionGreenTokens[fileName].Count - 1)
				{
					text4 += ",";
				}
			}
		}
		if (ModManager.MSC)
		{
			text4 += "&";
			for (int num4 = 0; num4 < regionGreyTokens[fileName].Count; num4++)
			{
				text4 += regionGreyTokens[fileName][num4];
				if (num4 != regionGreyTokens[fileName].Count - 1)
				{
					text4 += ",";
				}
			}
			text4 += "&";
			for (int num5 = 0; num5 < regionRedTokens[fileName].Count; num5++)
			{
				string text9 = string.Join("|", Array.ConvertAll(regionRedTokensAccessibility[fileName][num5].ToArray(), (SlugcatStats.Name x) => x.ToString()));
				text4 = text4 + regionRedTokens[fileName][num5].ToString() + "~" + text9;
				if (num5 != regionRedTokens[fileName].Count - 1)
				{
					text4 += ",";
				}
			}
		}
		File.WriteAllText(text + "tokencache" + fileName + ".txt", text4);
		if (ModManager.Watcher)
		{
			text4 = string.Join(",", regionSpinningTopRooms[fileName]);
			text4 = text4 + Environment.NewLine + string.Join(",", regionDynamicWarpTargets[fileName]);
			text4 = text4 + Environment.NewLine + string.Join(",", regionWeaverRooms[fileName]);
			text4 = text4 + Environment.NewLine + string.Join(",", regionWarpRooms[fileName]);
			text4 = text4 + Environment.NewLine + string.Join(",", regionRippleSpawnEggs[fileName].Select((KeyValuePair<string, int> kvp) => kvp.Key + ":" + kvp.Value));
			File.WriteAllText(text + "watchercache" + fileName + ".txt", text4);
		}
	}

	public void ClearTokenCacheInMemory()
	{
		regionBlueTokens.Clear();
		regionGoldTokens.Clear();
		regionGreenTokens.Clear();
		regionGreyTokens.Clear();
		regionRedTokens.Clear();
		regionDataPearls.Clear();
		regionSpinningTopRooms.Clear();
		regionWeaverRooms.Clear();
		regionWarpRooms.Clear();
		regionDynamicWarpTargets.Clear();
		levelDynamicWarpTargets.Clear();
		levelDeadEndTargets.Clear();
		levelBadWarpTargets.Clear();
		regionBlueTokensAccessibility.Clear();
		regionGoldTokensAccessibility.Clear();
		regionGreenTokensAccessibility.Clear();
		regionRedTokensAccessibility.Clear();
		regionDataPearlsAccessibility.Clear();
	}

	public void ReadTokenCache()
	{
		ClearTokenCacheInMemory();
		try
		{
			string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt"));
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].ToLowerInvariant();
				regionBlueTokens[text] = new List<MultiplayerUnlocks.SandboxUnlockID>();
				regionGoldTokens[text] = new List<MultiplayerUnlocks.LevelUnlockID>();
				regionGreenTokens[text] = new List<MultiplayerUnlocks.SlugcatUnlockID>();
				regionRedTokens[text] = new List<MultiplayerUnlocks.SafariUnlockID>();
				regionGreyTokens[text] = new List<ChatlogData.ChatlogID>();
				regionDataPearls[text] = new List<DataPearl.AbstractDataPearl.DataPearlType>();
				regionSpinningTopRooms[text] = new List<string>();
				regionWeaverRooms[text] = new List<string>();
				regionWarpRooms[text] = new List<string>();
				regionDynamicWarpTargets[text] = new List<string>();
				regionBlueTokensAccessibility[text] = new List<List<SlugcatStats.Name>>();
				regionGoldTokensAccessibility[text] = new List<List<SlugcatStats.Name>>();
				regionGreenTokensAccessibility[text] = new List<List<SlugcatStats.Name>>();
				regionRedTokensAccessibility[text] = new List<List<SlugcatStats.Name>>();
				regionDataPearlsAccessibility[text] = new List<List<SlugcatStats.Name>>();
				string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "indexmaps" + Path.DirectorySeparatorChar + "watchercache" + text + ".txt");
				if (ModManager.Watcher && File.Exists(path))
				{
					string[] array2 = File.ReadAllLines(path);
					if (array2.Length != 0)
					{
						regionSpinningTopRooms[text] = new List<string>(array2[0].Split(','));
						if (regionSpinningTopRooms[text].Count == 1 && regionSpinningTopRooms[text][0] == "")
						{
							regionSpinningTopRooms[text] = new List<string>();
						}
					}
					else
					{
						regionSpinningTopRooms[text] = new List<string>();
					}
					if (array2.Length > 1)
					{
						regionDynamicWarpTargets[text] = new List<string>(array2[1].Split(','));
						if (regionDynamicWarpTargets[text].Count == 1 && regionDynamicWarpTargets[text][0] == "")
						{
							regionDynamicWarpTargets[text] = new List<string>();
						}
					}
					else
					{
						regionDynamicWarpTargets[text] = new List<string>();
					}
					foreach (string item2 in regionDynamicWarpTargets[text])
					{
						if (item2.Contains(":"))
						{
							string[] array3 = Regex.Split(item2, ":");
							string item = array3[0];
							float key = float.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture);
							bool num = array3.Length > 2 && bool.Parse(array3[2]);
							bool flag = array3.Length > 3 && bool.Parse(array3[3]);
							if (!levelDynamicWarpTargets.ContainsKey(key))
							{
								levelDynamicWarpTargets[key] = new List<string>();
							}
							if (!levelDynamicWarpTargets[key].Contains(item))
							{
								levelDynamicWarpTargets[key].Add(item);
							}
							if (num && !levelDeadEndTargets.Contains(item))
							{
								levelDeadEndTargets.Add(item);
							}
							if (flag && !levelBadWarpTargets.Contains(item))
							{
								levelBadWarpTargets.Add(item);
							}
						}
					}
					if (array2.Length > 2)
					{
						regionWeaverRooms[text] = new List<string>(array2[2].Split(','));
						if (regionWeaverRooms[text].Count == 1 && regionWeaverRooms[text][0] == "")
						{
							regionWeaverRooms[text] = new List<string>();
						}
					}
					else
					{
						regionWeaverRooms[text] = new List<string>();
					}
					if (array2.Length > 3)
					{
						regionWarpRooms[text] = new List<string>(array2[3].Split(','));
						if (regionWarpRooms[text].Count == 1 && regionWarpRooms[text][0] == "")
						{
							regionWarpRooms[text] = new List<string>();
						}
					}
					else
					{
						regionWarpRooms[text] = new List<string>();
					}
					if (array2.Length > 4)
					{
						regionRippleSpawnEggs[text] = new Dictionary<string, int>();
						string[] array4 = array2[4].Split(',');
						for (int j = 0; j < array4.Length; j++)
						{
							string[] array5 = array4[j].Split(':');
							regionRippleSpawnEggs[text][array5[0]] = int.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						}
					}
					else
					{
						regionRippleSpawnEggs[text] = new Dictionary<string, int>();
					}
				}
				string path2 = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "indexmaps" + Path.DirectorySeparatorChar + "tokencache" + text + ".txt");
				if (!File.Exists(path2))
				{
					continue;
				}
				string[] array6 = File.ReadAllText(path2).Split('&');
				for (int k = 0; k < 6; k++)
				{
					if (k >= array6.Length || !(array6[k] != ""))
					{
						continue;
					}
					string[] array7 = Regex.Split(array6[k], ",");
					for (int l = 0; l < array7.Length; l++)
					{
						if ((k == 3 && !ModManager.NewSlugcatsModule) || (k >= 4 && !ModManager.MSC))
						{
							continue;
						}
						if (k != 4)
						{
							string[] array8 = Regex.Split(array7[l], "~");
							List<SlugcatStats.Name> list = new List<SlugcatStats.Name>();
							string[] array9 = array8[1].Split('|');
							for (int m = 0; m < array9.Length; m++)
							{
								list.Add(new SlugcatStats.Name(array9[m]));
							}
							switch (k)
							{
							case 0:
								regionBlueTokens[text].Add(new MultiplayerUnlocks.SandboxUnlockID(array8[0]));
								regionBlueTokensAccessibility[text].Add(list);
								break;
							case 1:
								regionGoldTokens[text].Add(new MultiplayerUnlocks.LevelUnlockID(array8[0]));
								regionGoldTokensAccessibility[text].Add(list);
								break;
							case 2:
								regionDataPearls[text].Add(new DataPearl.AbstractDataPearl.DataPearlType(array8[0]));
								regionDataPearlsAccessibility[text].Add(list);
								break;
							case 3:
								regionGreenTokens[text].Add(new MultiplayerUnlocks.SlugcatUnlockID(array8[0]));
								regionGreenTokensAccessibility[text].Add(list);
								break;
							case 5:
								regionRedTokens[text].Add(new MultiplayerUnlocks.SafariUnlockID(array8[0]));
								regionRedTokensAccessibility[text].Add(list);
								break;
							}
						}
						else
						{
							regionGreyTokens[text].Add(new ChatlogData.ChatlogID(array7[l]));
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Custom.LogWarning("FAILED TO READ THE TOKEN CACHE FILES, ARE THEY MALFORMED?", ex.Message, ex.StackTrace);
		}
	}

	public void EncryptUtility()
	{
		string path = Path.Combine(Custom.RootFolderDirectory(), "encrypt").ToLowerInvariant();
		string path2 = Path.Combine(Custom.RootFolderDirectory(), "decrypt").ToLowerInvariant();
		if (Directory.Exists(path))
		{
			string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				InGameTranslator.EncryptDecryptFile(files[i], encryptMode: true);
			}
		}
		if (Directory.Exists(path2))
		{
			string[] files2 = Directory.GetFiles(path2, "*.txt", SearchOption.AllDirectories);
			for (int j = 0; j < files2.Length; j++)
			{
				InGameTranslator.EncryptDecryptFile(files2[j], encryptMode: false);
			}
		}
	}

	public static Color MapWaterColor(Color? set)
	{
		if (set.HasValue)
		{
			mapWaterColorInternal = set.Value;
		}
		return mapWaterColorInternal;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void RunRewiredUpdate()
	{
		if (!UnityTools.isEditor || Application.isPlaying)
		{
			rewiredDelegateFunc((UpdateLoopType)0, (UpdateLoopSetting)1);
		}
	}
}
