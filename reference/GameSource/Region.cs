using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Region
{
	public class RegionParams
	{
		public Dictionary<string, string> unrecognizedParams;

		public int batDepleteCyclesMin;

		public int batDepleteCyclesMax;

		public int batDepleteCyclesMaxIfLessThanTwoLeft;

		public int batDepleteCyclesMaxIfLessThanFiveLeft;

		public float overseersSpawnChance;

		public float playerGuideOverseerSpawnChance;

		public int overseersMin;

		public int overseersMax;

		public int batsPerActiveSwarmRoom;

		public int batsPerInactiveSwarmRoom;

		public int scavsMin;

		public int scavsMax;

		public float scavsSpawnChance;

		public float? cloudsStart;

		public float? cloudsEnd;

		public Color corruptionEffectColor;

		public Color corruptionEyeColor;

		public Color? kelpColor;

		public bool albinos;

		public float blackSalamanderChance;

		public string proceduralMusicBank;

		public int scavengerDelayInitialMin;

		public int scavengerDelayInitialMax;

		public int scavengerDelayRepeatMin;

		public int scavengerDelayRepeatMax;

		public float slugPupSpawnChance;

		public bool glacialWasteland;

		public float earlyCycleChance;

		public float earlyCycleFloodChance;

		public string safariStartRoom;

		public int rotPalette;

		public Color mudColor;

		public bool locustRegion;

		public float fuseSize;

		public bool[] mapSkyLayers;

		public Dictionary<CreatureTemplate.Type, List<string>> globalCreatureFlags_Specific;

		public List<string> globalCreatureFlags_All;

		public bool hideTimer;

		public string mapName;

		public Color boxWormColor;

		public int rippleSpiderThreshold;

		public bool softLightRegion;

		public RegionParams()
		{
			unrecognizedParams = new Dictionary<string, string>();
			batDepleteCyclesMin = 2;
			batDepleteCyclesMax = 7;
			batDepleteCyclesMaxIfLessThanTwoLeft = 3;
			batDepleteCyclesMaxIfLessThanFiveLeft = 4;
			overseersSpawnChance = 0.8f;
			overseersMin = 1;
			overseersMax = 3;
			playerGuideOverseerSpawnChance = 1f;
			batsPerActiveSwarmRoom = 10;
			batsPerInactiveSwarmRoom = 4;
			scavsMin = 0;
			scavsMax = 5;
			scavsSpawnChance = 0.3f;
			corruptionEffectColor = new Color(0f, 0f, 1f);
			corruptionEyeColor = new Color(0f, 0f, 1f);
			kelpColor = null;
			albinos = false;
			blackSalamanderChance = 1f / 3f;
			proceduralMusicBank = "";
			scavengerDelayInitialMin = 900;
			scavengerDelayInitialMax = 1100;
			scavengerDelayRepeatMin = 4100;
			scavengerDelayRepeatMax = 8200;
			slugPupSpawnChance = 0f;
			glacialWasteland = false;
			earlyCycleChance = 0.02f;
			earlyCycleFloodChance = 0.33f;
			safariStartRoom = null;
			rotPalette = 466;
			mudColor = MudPit.defaultColor;
			locustRegion = false;
			fuseSize = 10f;
			mapSkyLayers = new bool[3];
			globalCreatureFlags_Specific = new Dictionary<CreatureTemplate.Type, List<string>>();
			globalCreatureFlags_All = new List<string>();
			hideTimer = false;
			mapName = null;
			boxWormColor = Color.cyan;
			rippleSpiderThreshold = -1;
			softLightRegion = false;
		}
	}

	public string name;

	public int numberOfRooms;

	public int firstRoomIndex;

	public int regionNumber;

	public RoomSettings[] roomSettingsTemplates;

	public string[] roomSettingTemplateNames;

	public RegionParams regionParams;

	public List<string> subRegions;

	public List<string> altSubRegions;

	public Color propertiesWaterColor;

	public static Dictionary<string, string> cachedRegionFullNames = new Dictionary<string, string>();

	public static Region[] WarpRegions;

	public static bool RegionReadyToWarp = true;

	public static List<string> GetFullRegionOrder(SlugcatStats.Timeline timeline)
	{
		List<string> list = new List<string>
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "SL", "SI", "LF", "UW",
			"SS", "SB"
		};
		if (ModManager.MSC)
		{
			list.Insert(list.IndexOf("DS") + 1, "UG");
			list.Insert(list.IndexOf("SH") + 1, "VS");
			list.Insert(list.IndexOf("SL") + 1, "LM");
			list.Insert(list.IndexOf("SS") + 1, "RM");
			list.AddRange(new List<string> { "OE", "LC", "MS", "DM", "CL", "HR" });
		}
		if (ModManager.ModdedRegionsEnabled)
		{
			(new string[1])[0] = "";
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt");
			if (File.Exists(path))
			{
				string[] array = File.ReadAllLines(path);
				foreach (string text in array)
				{
					if (!list.Contains(text.Trim()))
					{
						list.Add(text.Trim());
					}
				}
			}
		}
		if (timeline != null)
		{
			if (SlugcatStats.AtOrAfterTimeline(timeline, SlugcatStats.Timeline.Artificer))
			{
				list.Remove("DM");
			}
			if (SlugcatStats.AtOrBeforeTimeline(timeline, SlugcatStats.Timeline.Artificer))
			{
				list.Remove("SL");
				list.Remove("MS");
			}
			if (SlugcatStats.AtOrAfterTimeline(timeline, SlugcatStats.Timeline.Red))
			{
				list.Remove("LM");
			}
			if (SlugcatStats.AtOrBeforeTimeline(timeline, SlugcatStats.Timeline.Yellow) || SlugcatStats.AtOrAfterTimeline(timeline, SlugcatStats.Timeline.Saint))
			{
				list.Remove("RM");
			}
			if (SlugcatStats.AtOrAfterTimeline(timeline, SlugcatStats.Timeline.Rivulet))
			{
				list.Remove("SS");
			}
			if (SlugcatStats.AtOrBeforeTimeline(timeline, SlugcatStats.Timeline.Rivulet))
			{
				list.Remove("CL");
				list.Remove("UG");
			}
			if (SlugcatStats.AtOrAfterTimeline(timeline, SlugcatStats.Timeline.Saint))
			{
				list.Remove("UW");
				list.Remove("SH");
				list.Remove("DS");
				list.Remove("LC");
			}
			list.Remove("HR");
		}
		return list;
	}

	public static List<string> GetFullRegionOrder()
	{
		return GetFullRegionOrder(null);
	}

	public static MenuScene.SceneID GetRegionLandscapeScene(string regionAcro)
	{
		switch (regionAcro)
		{
		case "SU":
			return MenuScene.SceneID.Landscape_SU;
		case "HI":
			return MenuScene.SceneID.Landscape_HI;
		case "DS":
			return MenuScene.SceneID.Landscape_DS;
		case "CC":
			return MenuScene.SceneID.Landscape_CC;
		case "GW":
			return MenuScene.SceneID.Landscape_GW;
		case "SH":
			return MenuScene.SceneID.Landscape_SH;
		case "SL":
			return MenuScene.SceneID.Landscape_SL;
		case "SI":
			return MenuScene.SceneID.Landscape_SI;
		case "LF":
			return MenuScene.SceneID.Landscape_LF;
		case "UW":
			return MenuScene.SceneID.Landscape_UW;
		case "SS":
			return MenuScene.SceneID.Landscape_SS;
		case "SB":
			return MenuScene.SceneID.Landscape_SB;
		default:
			if (ModManager.MSC)
			{
				switch (regionAcro)
				{
				case "DM":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_DM;
				case "LM":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_LM;
				case "MS":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_MS;
				case "LC":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_LC;
				case "OE":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_OE;
				case "HR":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_HR;
				case "CL":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_CL;
				case "UG":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_UG;
				case "RM":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_RM;
				case "VS":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_VS;
				}
			}
			return MenuScene.SceneID.Empty;
		}
	}

	public static Color RegionColor(string regionName)
	{
		regionName = regionName.ToUpperInvariant();
		if (EquivalentRegion(regionName, "SU"))
		{
			return new Color(0.66f, 0.87f, 0.89f, 1f);
		}
		if (EquivalentRegion(regionName, "HI"))
		{
			return new Color(0.4f, 0.48f, 0.82f, 1f);
		}
		if (EquivalentRegion(regionName, "DS"))
		{
			return new Color(0.14f, 0.49f, 0.27f, 1f);
		}
		if (EquivalentRegion(regionName, "CC"))
		{
			return new Color(0.83f, 0.52f, 0.45f, 1f);
		}
		if (EquivalentRegion(regionName, "GW"))
		{
			return new Color(0.8f, 0.89f, 0.44f, 1f);
		}
		if (EquivalentRegion(regionName, "SH"))
		{
			return new Color(0.35f, 0.21f, 0.6f, 1f);
		}
		if (EquivalentRegion(regionName, "SL"))
		{
			return new Color(0.19f, 0.73f, 0.7f, 1f);
		}
		if (EquivalentRegion(regionName, "SI"))
		{
			return new Color(0.91f, 0.35f, 0.5f, 1f);
		}
		if (EquivalentRegion(regionName, "LF"))
		{
			return new Color(0.75f, 0.16f, 0.12f, 1f);
		}
		if (EquivalentRegion(regionName, "UW"))
		{
			return new Color(0.94f, 0.84f, 0.73f, 1f);
		}
		if (EquivalentRegion(regionName, "SS"))
		{
			return new Color(1f, 0.65f, 0.21f, 1f);
		}
		if (EquivalentRegion(regionName, "SB"))
		{
			return new Color(0.61f, 0.35f, 0.2f, 1f);
		}
		if (EquivalentRegion(regionName, "DM"))
		{
			return new Color(5f / 51f, 0.30980393f, 77f / 85f);
		}
		if (EquivalentRegion(regionName, "VS"))
		{
			return new Color(1f, 0.53f, 0.51f, 1f);
		}
		if (ModManager.Watcher)
		{
			switch (regionName)
			{
			case "WSKA":
				return new Color(0.5f, 0.55f, 0.6f);
			case "WSKB":
				return new Color(0.93f, 0.63f, 0.65f);
			case "WRFA":
				return new Color(0.18f, 0.6f, 0.85f);
			case "WRRA":
				return new Color(0.6f, 0.23f, 0.2f);
			case "WPGA":
				return new Color(0.04f, 0.8f, 0.55f);
			case "WARF":
				return new Color(0.4f, 0.53f, 0.7f);
			case "WSKD":
				return new Color(0.86f, 0.9f, 0.9f);
			case "WMPA":
				return new Color(0.6f, 0.4f, 0.37f);
			case "WTDA":
				return new Color(0.84f, 0.57f, 0.38f);
			case "WTDB":
				return new Color(0.58f, 0.6f, 0.56f);
			case "WARG":
				return new Color(0.5f, 0.333333f, 0.3f);
			case "WARD":
				return new Color(0.32f, 0.11f, 0.72f);
			case "WBLA":
				return new Color(0.93f, 0.8f, 0.48f);
			case "WARE":
				return new Color(0.7f, 0.14f, 0.16f);
			case "WRFB":
				return new Color(0.23f, 0.6f, 0.54f);
			case "WSKC":
				return new Color(0.5f, 0.5f, 0.6f);
			case "WVWA":
				return new Color(0.45f, 0.54f, 0.23f);
			case "WVWB":
				return new Color(0.52f, 0.6f, 0.47f);
			case "WARB":
				return new Color(0.22f, 0.42f, 0.6f);
			case "WARC":
				return new Color(0.7f, 0.3f, 0f);
			case "WPTA":
				return new Color(0.85f, 0.34f, 0.58f);
			case "WARA":
				return new Color(0.9f, 0.76f, 0.43f);
			case "WAUA":
				return new Color(0.7f, 0.58f, 0.5f);
			case "WORA":
				return new Color(0.5f, 0.42f, 0.7f);
			}
		}
		if (ModManager.ModdedRegionsEnabled)
		{
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionName + Path.DirectorySeparatorChar + "regioncolor.txt");
			if (File.Exists(path))
			{
				string[] array = File.ReadAllText(path).Trim().Split(',');
				if (array.Length == 3)
				{
					return new Color(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture), 1f);
				}
				if (array.Length == 4)
				{
					return new Color(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
			}
		}
		return new Color(1f, 1f, 1f, 1f);
	}

	public static bool EquivalentRegion(string regionA, string regionB)
	{
		if (regionA == regionB)
		{
			return true;
		}
		if ((regionA == "SL" || regionA == "LM") && (regionB == "SL" || regionB == "LM"))
		{
			return true;
		}
		if ((regionA == "SS" || regionA == "RM") && (regionB == "SS" || regionB == "RM"))
		{
			return true;
		}
		if ((regionA == "DS" || regionA == "UG") && (regionB == "UG" || regionB == "DS"))
		{
			return true;
		}
		if ((regionA == "SH" || regionA == "CL") && (regionB == "SH" || regionB == "CL"))
		{
			return true;
		}
		if ((regionA == "DM" || regionA == "MS") && (regionB == "DM" || regionB == "MS"))
		{
			return true;
		}
		if (ModManager.ModdedRegionsEnabled)
		{
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionA + Path.DirectorySeparatorChar + "equivalences.txt");
			if (File.Exists(path))
			{
				string[] array = File.ReadAllText(path).Trim().Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Contains("-"))
					{
						array[i] = array[i].Substring(array[i].IndexOf('-') + 1);
					}
					if (regionB == array[i])
					{
						return true;
					}
				}
			}
			string path2 = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionB + Path.DirectorySeparatorChar + "equivalences.txt");
			if (File.Exists(path2))
			{
				string[] array2 = File.ReadAllText(path2).Trim().Split(',');
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j].Contains("-"))
					{
						array2[j] = array2[j].Substring(array2[j].IndexOf('-') + 1);
					}
					if (regionA == array2[j])
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	[Obsolete("Use timeline parameter function instead.")]
	public static string GetProperRegionAcronym(SlugcatStats.Name character, string baseAcronym)
	{
		return GetProperRegionAcronym(SlugcatStats.SlugcatToTimeline(character), baseAcronym);
	}

	public static string GetProperRegionAcronym(SlugcatStats.Timeline time, string baseAcronym)
	{
		string text = baseAcronym;
		if (text == "UX")
		{
			text = "UW";
		}
		else if (text == "SX")
		{
			text = "SS";
		}
		if (time != null)
		{
			if ((time == SlugcatStats.Timeline.Spear || time == SlugcatStats.Timeline.Artificer) && text == "SL")
			{
				text = "LM";
			}
			if (time == SlugcatStats.Timeline.Saint && text == "DS")
			{
				text = "UG";
			}
			if ((time == SlugcatStats.Timeline.Saint || time == SlugcatStats.Timeline.Rivulet) && text == "SS")
			{
				text = "RM";
			}
			if (time == SlugcatStats.Timeline.Saint && text == "SH")
			{
				text = "CL";
			}
		}
		string[] array = AssetManager.ListDirectory("World", directories: true);
		for (int i = 0; i < array.Length; i++)
		{
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + Path.GetFileName(array[i]) + Path.DirectorySeparatorChar + "equivalences.txt");
			if (!File.Exists(path))
			{
				continue;
			}
			string[] array2 = File.ReadAllText(path).Trim().Split(',');
			for (int j = 0; j < array2.Length; j++)
			{
				string text2 = null;
				string text3 = array2[j];
				if (array2[j].Contains("-"))
				{
					text3 = array2[j].Split('-')[0];
					text2 = array2[j].Split('-')[1];
				}
				if (text3 == baseAcronym && (text2 == null || time.value.ToLower() == text2.ToLower()))
				{
					text = Path.GetFileName(array[i]).ToUpper();
				}
			}
		}
		return text;
	}

	public static string GetVanillaEquivalentRegionAcronym(string baseAcronym)
	{
		switch (baseAcronym)
		{
		case "UG":
			return "DS";
		case "RM":
			return "SS";
		case "CL":
			return "SH";
		case "LM":
			return "SL";
		default:
			if (ModManager.ModdedRegionsEnabled)
			{
				string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + baseAcronym + Path.DirectorySeparatorChar + "equivalences.txt");
				if (File.Exists(path))
				{
					string[] array = File.ReadAllText(path).Trim().Split(',');
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].Contains("-"))
						{
							array[i] = array[i].Substring(array[i].IndexOf('-') + 1);
						}
						switch (array[i])
						{
						case "SU":
						case "GW":
						case "CC":
						case "HI":
						case "SI":
						case "SS":
						case "SB":
						case "SL":
						case "UW":
						case "DS":
						case "SH":
						case "LF":
							return array[i];
						}
					}
					return array[0];
				}
			}
			return baseAcronym;
		}
	}

	public static string GetRegionFullName(string regionAcro, SlugcatStats.Name slugcatIndex)
	{
		string text = "";
		if (slugcatIndex != null)
		{
			text = "-" + slugcatIndex.value;
		}
		string key = (regionAcro + text).ToLowerInvariant();
		if (cachedRegionFullNames.ContainsKey(key))
		{
			return cachedRegionFullNames[key];
		}
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionAcro + Path.DirectorySeparatorChar + "DisplayName" + text + ".txt");
		if (text != "" && !File.Exists(path))
		{
			path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionAcro + Path.DirectorySeparatorChar + "DisplayName.txt");
		}
		if (File.Exists(path))
		{
			string text2 = File.ReadAllLines(path)[0];
			cachedRegionFullNames[key] = text2;
			return text2;
		}
		return "Unknown Region";
	}

	public static int NumberOfRoomsInRegion(string name)
	{
		string[] array = new string[1] { "" };
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "world_" + name + ".txt");
		if (File.Exists(path))
		{
			array = File.ReadAllLines(path);
		}
		bool flag = false;
		int num = 1;
		for (int i = 0; i < array.Length && !(array[i] == "END ROOMS"); i++)
		{
			if (flag && array[i].Contains(" : "))
			{
				num++;
			}
			if (array[i] == "ROOMS")
			{
				flag = true;
			}
		}
		return num;
	}

	public static bool IsRubiconRegion(string name)
	{
		return name.ToLowerInvariant() == "hr";
	}

	public static bool IsVanillaSentientRotRegion(string name)
	{
		string text = name.ToLowerInvariant();
		switch (text)
		{
		default:
			return text == "wdsr";
		case "wsur":
		case "whir":
		case "wgwr":
			return true;
		}
	}

	public static bool IsSentientRotRegion(string name)
	{
		string text = name.ToLowerInvariant();
		if (!IsVanillaSentientRotRegion(text) && !(text == "wssr"))
		{
			return text == "wora";
		}
		return true;
	}

	public static bool IsAncientUrbanRegion(string name)
	{
		return name.ToLowerInvariant() == "waua";
	}

	public static bool IsShatteredTerraceRegion(string name)
	{
		return name.ToLowerInvariant() == "wara";
	}

	public static bool IsDaemonRegion(string name)
	{
		return name.ToLowerInvariant() == "wrsa";
	}

	public static bool IsWatcherVanillaRegion(string name)
	{
		string text = name.ToLowerInvariant();
		switch (text)
		{
		default:
			return text == "sh";
		case "su":
		case "hi":
		case "lf":
		case "cc":
			return true;
		}
	}

	public static bool HasWarpFatigueResistance(string name)
	{
		if (!IsAncientUrbanRegion(name))
		{
			return IsDaemonRegion(name);
		}
		return true;
	}

	public static bool HasSentientRotResistance(string name)
	{
		if (!IsSentientRotRegion(name) && !IsAncientUrbanRegion(name) && !IsShatteredTerraceRegion(name))
		{
			return IsDaemonRegion(name);
		}
		return true;
	}

	[Obsolete("Use timeline parameter function instead.")]
	public static Region[] LoadAllRegions(SlugcatStats.Name storyIndex)
	{
		return LoadAllRegions(SlugcatStats.SlugcatToTimeline(storyIndex));
	}

	[Obsolete("Use RainWorldGame parameter constructor instead.")]
	public static Region[] LoadAllRegions(SlugcatStats.Timeline timelineIndex)
	{
		return LoadAllRegions(timelineIndex, null);
	}

	public static Region[] LoadAllRegions(SlugcatStats.Timeline timelineIndex, RainWorldGame game)
	{
		string[] array = new string[1] { "" };
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt");
		if (File.Exists(path))
		{
			array = File.ReadAllLines(path);
		}
		Region[] array2 = new Region[array.Length];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = new Region(array[i], num, i, game, timelineIndex);
			num += array2[i].numberOfRooms;
		}
		return array2;
	}

	[Obsolete("Use RainWorldGame parameter constructor instead.")]
	public static IEnumerator LoadAllRegionsCoroutine(SlugcatStats.Timeline timelineIndex)
	{
		yield return LoadAllRegionsCoroutine(timelineIndex, null);
	}

	public static IEnumerator LoadAllRegionsCoroutine(SlugcatStats.Timeline timelineIndex, RainWorldGame game)
	{
		RegionReadyToWarp = false;
		string[] regionLines = new string[1] { "" };
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt");
		if (File.Exists(path))
		{
			regionLines = File.ReadAllLines(path);
		}
		WarpRegions = new Region[regionLines.Length];
		int totalRooms = 0;
		for (int i = 0; i < regionLines.Length; i++)
		{
			WarpRegions[i] = new Region(regionLines[i], totalRooms, i, game, timelineIndex);
			totalRooms += WarpRegions[i].numberOfRooms;
			yield return null;
		}
		RegionReadyToWarp = true;
	}

	[Obsolete("Use timeline parameter constructor instead.")]
	public Region(string name, int firstRoomIndex, int regionNumber, SlugcatStats.Name storyIndex)
		: this(name, firstRoomIndex, regionNumber, SlugcatStats.SlugcatToTimeline(storyIndex))
	{
	}

	[Obsolete("Use RainWorldGame parameter constructor instead.")]
	public Region(string name, int firstRoomIndex, int regionNumber, SlugcatStats.Timeline timelineIndex)
		: this(name, firstRoomIndex, regionNumber, null, timelineIndex)
	{
	}

	public Region(string name, int firstRoomIndex, int regionNumber, RainWorldGame game, SlugcatStats.Timeline timelineIndex)
	{
		this.name = name;
		this.firstRoomIndex = firstRoomIndex;
		this.regionNumber = regionNumber;
		roomSettingsTemplates = new RoomSettings[0];
		roomSettingTemplateNames = new string[0];
		regionParams = new RegionParams();
		subRegions = new List<string> { "" };
		altSubRegions = new List<string> { null };
		numberOfRooms = NumberOfRoomsInRegion(name);
		propertiesWaterColor = RainWorld.DefaultWaterColor;
		if (ModManager.Watcher && timelineIndex != null && timelineIndex == SlugcatStats.Timeline.Watcher)
		{
			regionParams.corruptionEffectColor = RainWorld.RippleColor;
			regionParams.corruptionEyeColor = RainWorld.RippleColor;
		}
		string[] array = new string[1] { "" };
		string text = "";
		bool flag = false;
		bool flag2 = false;
		string path;
		if (regionNumber == -1)
		{
			path = WorldLoader.FindRoomFile(name, includeRootDirectory: false, "_Properties.txt");
			flag2 = true;
		}
		else
		{
			if (timelineIndex != null)
			{
				text = "-" + timelineIndex.value;
			}
			path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "properties" + text + ".txt");
			if (text != "" && !File.Exists(path))
			{
				path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "properties.txt");
			}
			else if (text != "")
			{
				flag = true;
			}
		}
		if (File.Exists(path))
		{
			array = File.ReadAllLines(path);
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = WorldLoader.Preprocessing.PreprocessLine(array[i], game, timelineIndex);
			if (array[i] == null)
			{
				continue;
			}
			string[] array2 = Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ":"), ": ");
			if (array2.Length < 2)
			{
				continue;
			}
			if (!flag2)
			{
				switch (array2[0])
				{
				case "Room Setting Templates":
				{
					string[] array5 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1].ToLowerInvariant(), ","), ", ");
					roomSettingsTemplates = new RoomSettings[array5.Length];
					roomSettingTemplateNames = new string[array5.Length];
					for (int k = 0; k < array5.Length; k++)
					{
						roomSettingTemplateNames[k] = array5[k];
						ReloadRoomSettingsTemplate(array5[k]);
					}
					continue;
				}
				case "batDepleteCyclesMin":
					regionParams.batDepleteCyclesMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "batDepleteCyclesMax":
					regionParams.batDepleteCyclesMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "batDepleteCyclesMaxIfLessThanTwoLeft":
					regionParams.batDepleteCyclesMaxIfLessThanTwoLeft = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "batDepleteCyclesMaxIfLessThanFiveLeft":
					regionParams.batDepleteCyclesMaxIfLessThanFiveLeft = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "overseersSpawnChance":
					regionParams.overseersSpawnChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "overseersMin":
					regionParams.overseersMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "overseersMax":
					regionParams.overseersMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "playerGuideOverseerSpawnChance":
					regionParams.playerGuideOverseerSpawnChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "scavsMin":
					regionParams.scavsMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "scavsMax":
					regionParams.scavsMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "scavsSpawnChance":
					regionParams.scavsSpawnChance = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "Subregion":
					if (flag)
					{
						altSubRegions.Add(array2[1]);
						continue;
					}
					subRegions.Add(array2[1]);
					altSubRegions.Add(null);
					continue;
				case "batsPerActiveSwarmRoom":
					regionParams.batsPerActiveSwarmRoom = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "batsPerInactiveSwarmRoom":
					regionParams.batsPerInactiveSwarmRoom = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "waterColorOverride":
				{
					string[] array4 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
					propertiesWaterColor = new Color(float.Parse(array4[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array4[2], NumberStyles.Any, CultureInfo.InvariantCulture));
					continue;
				}
				case "scavsDelayInitialMin":
					regionParams.scavengerDelayInitialMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "scavsDelayInitialMax":
					regionParams.scavengerDelayInitialMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "scavsDelayRepeatMin":
					regionParams.scavengerDelayRepeatMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "scavsDelayRepeatMax":
					regionParams.scavengerDelayRepeatMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "pupSpawnChance":
					regionParams.slugPupSpawnChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "earlyCycleChance":
					regionParams.earlyCycleChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "earlyCycleFloodChance":
					regionParams.earlyCycleFloodChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				case "safariStartRoom":
					regionParams.safariStartRoom = array2[1];
					continue;
				case "proceduralMusicBank":
					regionParams.proceduralMusicBank = array2[1];
					continue;
				case "mapSkyLayers":
				{
					string[] array3 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1].ToLowerInvariant(), ","), ", ");
					for (int j = 0; j < array3.Length; j++)
					{
						int num = int.Parse(array3[j].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture);
						if (0 <= num && num <= 2)
						{
							regionParams.mapSkyLayers[num] = true;
						}
					}
					continue;
				}
				case "hideTimer":
					regionParams.hideTimer = array2[1].Trim().ToLower() == "true";
					continue;
				case "rippleSpiderThreshold":
					regionParams.rippleSpiderThreshold = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					continue;
				}
			}
			switch (array2[0])
			{
			case "blackSalamanderChance":
				regionParams.blackSalamanderChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "corruptionEffectColor":
			{
				string[] array9 = array2[1].Split(',');
				if (array9.Length == 3)
				{
					regionParams.corruptionEffectColor = new Color(float.Parse(array9[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array9[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array9[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				break;
			}
			case "corruptionEyeColor":
			{
				string[] array10 = array2[1].Split(',');
				if (array10.Length == 3)
				{
					regionParams.corruptionEyeColor = new Color(float.Parse(array10[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array10[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array10[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				break;
			}
			case "kelpColor":
			{
				string[] array8 = array2[1].Split(',');
				if (array8.Length == 3)
				{
					regionParams.kelpColor = new Color(float.Parse(array8[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array8[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array8[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				break;
			}
			case "albinos":
				regionParams.albinos = array2[1].Trim().ToLower() == "true";
				break;
			case "GlacialWasteland":
				regionParams.glacialWasteland = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
				break;
			case "cloudsStart":
				regionParams.cloudsStart = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "cloudsEnd":
				regionParams.cloudsEnd = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "rotPalette":
				regionParams.rotPalette = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "mudColor":
			{
				string[] array7 = array2[1].Split(',');
				if (array7.Length == 3)
				{
					regionParams.mudColor = new Color(float.Parse(array7[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array7[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array7[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				else if (array7.Length == 1 && new Regex("[0-9a-fA-F]{6}").IsMatch(array7[0]))
				{
					regionParams.mudColor = Custom.hexToColor(array7[0]);
				}
				break;
			}
			case "locustRegion":
				regionParams.locustRegion = array2[1].Trim().ToLower() == "true";
				break;
			case "fuseSize":
				regionParams.fuseSize = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "globalCreatureFlags":
			{
				string[] array3 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
				foreach (string text2 in array3)
				{
					if (text2.Contains("-"))
					{
						CreatureTemplate.Type type = new CreatureTemplate.Type(text2.Split('-')[0]);
						if (type.Index != -1)
						{
							if (!regionParams.globalCreatureFlags_Specific.ContainsKey(type))
							{
								regionParams.globalCreatureFlags_Specific[type] = new List<string>();
							}
							regionParams.globalCreatureFlags_Specific[type].Add(text2.Split('-')[1]);
						}
					}
					else
					{
						regionParams.globalCreatureFlags_All.Add(text2);
					}
				}
				break;
			}
			case "mapName":
				regionParams.mapName = array2[1].Trim();
				break;
			case "boxWormColor":
			{
				string[] array6 = array2[1].Split(',');
				if (array6.Length == 3)
				{
					regionParams.boxWormColor = new Color(float.Parse(array6[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array6[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array6[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				else if (array6.Length == 1 && new Regex("[0-9a-fA-F]{6}").IsMatch(array6[0]))
				{
					regionParams.boxWormColor = Custom.hexToColor(array6[0]);
				}
				break;
			}
			default:
				regionParams.unrecognizedParams[array2[0]] = array2[1].Trim();
				break;
			case "softLightRegion":
				regionParams.softLightRegion = array2[1].Trim().ToLower() == "true";
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "properties.txt");
		array = new string[1] { "" };
		if (File.Exists(path))
		{
			array = File.ReadAllLines(path);
		}
		for (int l = 0; l < array.Length; l++)
		{
			string[] array11 = Regex.Split(Custom.ValidateSpacedDelimiter(array[l], ":"), ": ");
			if (array11.Length >= 2)
			{
				string text3 = array11[0];
				if (text3 != null && text3 == "Subregion")
				{
					subRegions.Add(array11[1]);
				}
			}
		}
	}

	public RoomSettings GetRoomSettingsTemplate(string templateName)
	{
		if (roomSettingTemplateNames == null || roomSettingsTemplates == null || roomSettingTemplateNames.Length == 0 || roomSettingsTemplates.Length == 0)
		{
			return DefaultRoomSettings.ancestor;
		}
		if (templateName.ToLowerInvariant() == "first")
		{
			return roomSettingsTemplates[0];
		}
		for (int i = 0; i < roomSettingTemplateNames.Length; i++)
		{
			if (roomSettingTemplateNames[i] == templateName.ToLowerInvariant())
			{
				return roomSettingsTemplates[i];
			}
		}
		return DefaultRoomSettings.ancestor;
	}

	public void ReloadRoomSettingsTemplate(string templateName)
	{
		for (int i = 0; i < roomSettingTemplateNames.Length; i++)
		{
			if (roomSettingTemplateNames[i] == templateName.ToLowerInvariant())
			{
				roomSettingsTemplates[i] = new RoomSettings(name + "_settingstemplate_" + templateName.ToLowerInvariant(), this, template: true, i == 0, null, null);
				break;
			}
		}
	}

	public bool IsRoomInRegion(int roomIndex)
	{
		if (roomIndex >= firstRoomIndex)
		{
			return roomIndex < firstRoomIndex + numberOfRooms;
		}
		return false;
	}

	public int CyclesToDepleteASwarmRoom(int activeOnesInRegion)
	{
		if (activeOnesInRegion < 1)
		{
			return 1;
		}
		int batDepleteCyclesMin = regionParams.batDepleteCyclesMin;
		int maxExclusive = regionParams.batDepleteCyclesMax;
		if (activeOnesInRegion < 2)
		{
			maxExclusive = regionParams.batDepleteCyclesMaxIfLessThanTwoLeft;
		}
		else if (activeOnesInRegion < 5)
		{
			maxExclusive = regionParams.batDepleteCyclesMaxIfLessThanFiveLeft;
		}
		return UnityEngine.Random.Range(batDepleteCyclesMin, maxExclusive);
	}
}
