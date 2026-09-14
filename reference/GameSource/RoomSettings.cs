using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;
using Watcher;

public class RoomSettings
{
	public class FadePalette
	{
		public int palette;

		public float[] fades;

		public FadePalette(int palette, int screens)
		{
			this.palette = palette;
			fades = new float[screens];
		}
	}

	public class TerrainFadePalette
	{
		public string palette;

		public float[] fades;

		public TerrainFadePalette(string palette, int screens)
		{
			this.palette = palette;
			fades = new float[screens];
		}
	}

	public class RoomEffect
	{
		public class Type : ExtEnum<Type>
		{
			public static Type None = new Type("None", register: true);

			public static Type SkyDandelions = new Type("SkyDandelions", register: true);

			public static Type SkyBloom = new Type("SkyBloom", register: true);

			public static Type LightBurn = new Type("LightBurn", register: true);

			public static Type SkyAndLightBloom = new Type("SkyAndLightBloom", register: true);

			public static Type Bloom = new Type("Bloom", register: true);

			public static Type Fog = new Type("Fog", register: true);

			public static Type Lightning = new Type("Lightning", register: true);

			public static Type BkgOnlyLightning = new Type("BkgOnlyLightning", register: true);

			public static Type ExtraLoudThunder = new Type("ExtraLoudThunder", register: true);

			public static Type GreenSparks = new Type("GreenSparks", register: true);

			public static Type VoidMelt = new Type("VoidMelt", register: true);

			public static Type ZeroG = new Type("ZeroG", register: true);

			public static Type BrokenZeroG = new Type("BrokenZeroG", register: true);

			public static Type ZeroGSpecks = new Type("ZeroGSpecks", register: true);

			public static Type SunBlock = new Type("SunBlock", register: true);

			public static Type SuperStructureProjector = new Type("SuperStructureProjector", register: true);

			public static Type ProjectedScanLines = new Type("ProjectedScanLines", register: true);

			public static Type CorruptionSpores = new Type("CorruptionSpores", register: true);

			public static Type SSSwarmers = new Type("SSSwarmers", register: true);

			public static Type SSMusic = new Type("SSMusic", register: true);

			public static Type AboveCloudsView = new Type("AboveCloudsView", register: true);

			public static Type RoofTopView = new Type("RoofTopView", register: true);

			public static Type VoidSea = new Type("VoidSea", register: true);

			public static Type ElectricDeath = new Type("ElectricDeath", register: true);

			public static Type VoidSpawn = new Type("VoidSpawn", register: true);

			public static Type BorderPushBack = new Type("BorderPushBack", register: true);

			public static Type Flies = new Type("Flies", register: true);

			public static Type FireFlies = new Type("FireFlies", register: true);

			public static Type TinyDragonFly = new Type("TinyDragonFly", register: true);

			public static Type RockFlea = new Type("RockFlea", register: true);

			public static Type RedSwarmer = new Type("RedSwarmer", register: true);

			public static Type Ant = new Type("Ant", register: true);

			public static Type Beetle = new Type("Beetle", register: true);

			public static Type WaterGlowworm = new Type("WaterGlowworm", register: true);

			public static Type Wasp = new Type("Wasp", register: true);

			public static Type Moth = new Type("Moth", register: true);

			public static Type ModifyEffectColorA = new Type("ModifyEffectColorA", register: true);

			public static Type ModifyEffectColorB = new Type("ModifyEffectColorB", register: true);

			public static Type GeyserFrequency = new Type("GeyserFrequency", register: true);

			public static Type PinkSky = new Type("PinkSky", register: true);

			public static Type SandPuffs = new Type("SandPuffs", register: true);

			public static Type LocustSwarmConfig = new Type("LocustSwarmConfig", register: true);

			public static Type WindDirection = new Type("WindDirection", register: true);

			public static Type LightRain = new Type("LightRain", register: true);

			public static Type HeavyRain = new Type("HeavyRain", register: true);

			public static Type HeavyRainFlux = new Type("HeavyRainFlux", register: true);

			public static Type BulletRain = new Type("BulletRain", register: true);

			public static Type BulletRainFlux = new Type("BulletRainFlux", register: true);

			public static Type WaterFluxFrequency = new Type("WaterFluxFrequency", register: true);

			public static Type WaterFluxMinLevel = new Type("WaterFluxMinLevel", register: true);

			public static Type WaterFluxMaxLevel = new Type("WaterFluxMaxLevel", register: true);

			public static Type WaterFluxMinDelay = new Type("WaterFluxMinDelay", register: true);

			public static Type WaterFluxMaxDelay = new Type("WaterFluxMaxDelay", register: true);

			public static Type WaterFluxSpeed = new Type("WaterFluxSpeed", register: true);

			public static Type WaterFluxOffset = new Type("WaterFluxOffset", register: true);

			public static Type WaterFluxRumble = new Type("WaterFluxRumble", register: true);

			public static Type LethalWater = new Type("LethalWater", register: true);

			public static Type HeatWave = new Type("HeatWave", register: true);

			public static Type Coldness = new Type("Coldness", register: true);

			public static Type DayNight = new Type("DayNight", register: true);

			public static Type Darkness = new Type("Darkness", register: true);

			public static Type Brightness = new Type("Brightness", register: true);

			public static Type Contrast = new Type("Contrast", register: true);

			public static Type Desaturation = new Type("Desaturation", register: true);

			public static Type Hue = new Type("Hue", register: true);

			public static Type FairyParticles = new Type("FairyParticles", register: true);

			public static Type FakeGate = new Type("FakeGate", register: true);

			public static Type Dustpuffs = new Type("Dustpuffs", register: true);

			public static Type SilenceWater = new Type("SilenceWater", register: true);

			public static Type WaterViscosity = new Type("WaterViscosity", register: true);

			public static Type DarkenLights = new Type("DarkenLights", register: true);

			public static Type WaterLights = new Type("WaterLights", register: true);

			public static Type WaterDepth = new Type("WaterDepth", register: true);

			public static Type DirtyWater = new Type("DirtyWater", register: true);

			public static Type LavaSurface = new Type("LavaSurface", register: true);

			public static Type PixelShift = new Type("PixelShift", register: true);

			public static Type FastFloodDrain = new Type("FastFloodDrain", register: true);

			public static Type FastFloodPullDown = new Type("FastFloodPullDown", register: true);

			public Type(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Type type;

		public float amount;

		public float[] extraAmounts;

		public bool inherited;

		public bool overWrite;

		public Vector2 panelPosition;

		public string[] unrecognizedAttributes;

		public bool save;

		public RoomEffect(Type type, float amount, bool inherited)
		{
			this.type = type;
			this.amount = amount;
			this.inherited = inherited;
			save = true;
			int sliderCount = GetSliderCount(type);
			if (sliderCount > 1)
			{
				extraAmounts = new float[sliderCount - 1];
				for (int i = 1; i < sliderCount; i++)
				{
					extraAmounts[i - 1] = GetSliderDefault(type, i);
				}
			}
		}

		public float GetAmount(int sliderIndex)
		{
			if (extraAmounts != null && sliderIndex > 0 && sliderIndex <= extraAmounts.Length)
			{
				return extraAmounts[sliderIndex - 1];
			}
			return amount;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(amount.ToString(CultureInfo.InvariantCulture));
			float[] array = extraAmounts;
			if (array != null && array.Length != 0)
			{
				for (int i = 0; i < extraAmounts.Length; i++)
				{
					stringBuilder.Append(',');
					stringBuilder.Append(extraAmounts[i].ToString(CultureInfo.InvariantCulture));
				}
			}
			return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-{3}", type.ToString(), stringBuilder.ToString(), panelPosition.x, panelPosition.y), "-", unrecognizedAttributes);
		}

		public void FromString(string[] s)
		{
			try
			{
				type = new Type(s[0]);
				int sliderCount = GetSliderCount(type);
				if (sliderCount <= 1)
				{
					amount = float.Parse(s[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					extraAmounts = null;
				}
				else
				{
					string[] array = s[1].Split(',');
					amount = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
					extraAmounts = new float[sliderCount - 1];
					for (int i = 1; i <= extraAmounts.Length; i++)
					{
						if (i < array.Length)
						{
							extraAmounts[i - 1] = float.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
						}
						else
						{
							extraAmounts[i - 1] = GetSliderDefault(type, i);
						}
					}
				}
				panelPosition.x = float.Parse(s[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				panelPosition.y = float.Parse(s[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(s, 4);
			}
			catch
			{
				Custom.LogWarning("Wrong syntax effect loaded:", s[0]);
			}
		}

		public static int GetSliderCount(Type type)
		{
			if (type == Type.ModifyEffectColorA || type == Type.ModifyEffectColorB)
			{
				return 3;
			}
			if (type == Type.LocustSwarmConfig)
			{
				return 5;
			}
			if (type == Type.WindDirection)
			{
				return 2;
			}
			if (type == WatcherEnums.RoomEffectType.SKLightning)
			{
				return 3;
			}
			if (type == WatcherEnums.RoomEffectType.FlowerWind)
			{
				return 3;
			}
			return 1;
		}

		public static string GetSliderName(Type type, int index)
		{
			if (type == Type.ModifyEffectColorA || type == Type.ModifyEffectColorB)
			{
				switch (index)
				{
				case 0:
					return "Hue";
				case 1:
					return "Saturation";
				case 2:
					return "Value";
				}
			}
			if (type == Type.LocustSwarmConfig)
			{
				switch (index)
				{
				case 0:
					return "Density";
				case 1:
					return "Volume";
				case 2:
					return "Sound Proximity";
				case 3:
					return "Fg Shadows";
				case 4:
					return "Grounded";
				}
			}
			if (type == Type.WindDirection)
			{
				switch (index)
				{
				case 0:
					return "Angle";
				case 1:
					return "Strength";
				}
			}
			if (type == WatcherEnums.RoomEffectType.SKLightning)
			{
				switch (index)
				{
				case 0:
					return "Amount";
				case 1:
					return "Lethal Frequency";
				case 2:
					return "Cosmetic Frequency";
				}
			}
			if (type == WatcherEnums.RoomEffectType.FlowerWind)
			{
				switch (index)
				{
				case 0:
					return "Minimum";
				case 1:
					return "Maximum";
				case 2:
					return "Turbulence";
				}
			}
			return "Amount";
		}

		public static float GetSliderDefault(Type type, int index)
		{
			if (type == Type.ModifyEffectColorA || type == Type.ModifyEffectColorB)
			{
				switch (index)
				{
				case 0:
					return 0f;
				case 1:
					return 0.5f;
				case 2:
					return 0.5f;
				}
			}
			else if (type == Type.LocustSwarmConfig)
			{
				switch (index)
				{
				case 0:
					return 0.5f;
				case 1:
					return 0.5f;
				case 2:
					return 1f;
				case 3:
					return 0f;
				case 4:
					return 0f;
				}
			}
			else if (type == Type.WindDirection)
			{
				switch (index)
				{
				case 0:
					return 0f;
				case 1:
					return 0.25f;
				}
			}
			else if (type == WatcherEnums.RoomEffectType.SKLightning)
			{
				switch (index)
				{
				case 0:
					return 1f;
				case 1:
					return 0.1f;
				case 2:
					return 0.1f;
				}
			}
			else if (type == WatcherEnums.RoomEffectType.FlowerWind)
			{
				return 0.5f;
			}
			return 0f;
		}
	}

	public bool isAncestor;

	public bool isTemplate;

	public bool isFirstTemplate;

	public RoomSettings parent;

	public RainWorldGame game;

	public Room room;

	public RoomRain.DangerType dType;

	public float? rInts;

	public float? rumInts;

	public float? cDrips;

	public float? wSpeed;

	public float? wAmp;

	public float? wLength;

	public float? swAmp;

	public float? swLength;

	public float? clds;

	public float? grm;

	public float? bkgDrnVl;

	public float? bkgDrnNoThreatVol;

	public float? rndItmDns;

	public float? rndItmSprChnc;

	public float? wtrRflctAlpha;

	public int? pal;

	public int? eColA;

	public int? eColB;

	public bool roomSpecificScript;

	public bool wetTerrain = true;

	public float? terrainLight;

	public float? terrainStainAmount;

	public float? terrainStainBrightness;

	public float? terrainStainHeight;

	public float? terrainWaves;

	public float? terrainEdgeRadius;

	public float? terrainGooHeight;

	public float? terrainGrain;

	public float? terrainDepth;

	public float? terrainSkyFade;

	public string terrainPalette;

	public FadePalette fadePalette;

	public TerrainFadePalette terrainFadePalette;

	public List<RoomEffect> effects;

	public List<AmbientSound> ambientSounds;

	public List<EventTrigger> triggers;

	public List<PlacedObject> placedObjects;

	public string name;

	public string filePath;

	public RoomRain.DangerType DangerType
	{
		get
		{
			if (dType != null)
			{
				return dType;
			}
			return parent.DangerType;
		}
		set
		{
			dType = value;
		}
	}

	public float RainIntensity
	{
		get
		{
			if (rInts.HasValue)
			{
				return rInts.Value;
			}
			return parent.RainIntensity;
		}
		set
		{
			rInts = value;
		}
	}

	public float RumbleIntensity
	{
		get
		{
			if (rumInts.HasValue)
			{
				return rumInts.Value;
			}
			return parent.RumbleIntensity;
		}
		set
		{
			rumInts = value;
		}
	}

	public float CeilingDrips
	{
		get
		{
			if (cDrips.HasValue)
			{
				return cDrips.Value;
			}
			return parent.CeilingDrips;
		}
		set
		{
			cDrips = value;
		}
	}

	public float WaveSpeed
	{
		get
		{
			if (wSpeed.HasValue)
			{
				return wSpeed.Value;
			}
			return parent.WaveSpeed;
		}
		set
		{
			wSpeed = value;
		}
	}

	public float WaveAmplitude
	{
		get
		{
			if (wAmp.HasValue)
			{
				return wAmp.Value;
			}
			return parent.WaveAmplitude;
		}
		set
		{
			wAmp = value;
		}
	}

	public float WaveLength
	{
		get
		{
			if (wLength.HasValue)
			{
				return wLength.Value;
			}
			return parent.WaveLength;
		}
		set
		{
			wLength = value;
		}
	}

	public float SecondWaveAmplitude
	{
		get
		{
			if (swAmp.HasValue)
			{
				return swAmp.Value;
			}
			return parent.SecondWaveAmplitude;
		}
		set
		{
			swAmp = value;
		}
	}

	public float SecondWaveLength
	{
		get
		{
			if (swLength.HasValue)
			{
				return swLength.Value;
			}
			return parent.SecondWaveLength;
		}
		set
		{
			swLength = value;
		}
	}

	public float Clouds
	{
		get
		{
			if (clds.HasValue)
			{
				return clds.Value;
			}
			return parent.Clouds;
		}
		set
		{
			clds = value;
		}
	}

	public float Grime
	{
		get
		{
			if (grm.HasValue)
			{
				return grm.Value;
			}
			return parent.Grime;
		}
		set
		{
			grm = value;
		}
	}

	public float BkgDroneVolume
	{
		get
		{
			if (bkgDrnVl.HasValue)
			{
				return bkgDrnVl.Value;
			}
			return parent.BkgDroneVolume;
		}
		set
		{
			bkgDrnVl = value;
		}
	}

	public float BkgDroneNoThreatVolume
	{
		get
		{
			if (bkgDrnNoThreatVol.HasValue)
			{
				return bkgDrnNoThreatVol.Value;
			}
			return parent.BkgDroneNoThreatVolume;
		}
		set
		{
			bkgDrnNoThreatVol = value;
		}
	}

	public float RandomItemDensity
	{
		get
		{
			if (rndItmDns.HasValue)
			{
				return rndItmDns.Value;
			}
			return parent.RandomItemDensity;
		}
		set
		{
			rndItmDns = value;
		}
	}

	public float RandomItemSpearChance
	{
		get
		{
			if (rndItmSprChnc.HasValue)
			{
				return rndItmSprChnc.Value;
			}
			return parent.RandomItemSpearChance;
		}
		set
		{
			rndItmSprChnc = value;
		}
	}

	public float WaterReflectionAlpha
	{
		get
		{
			if (wtrRflctAlpha.HasValue)
			{
				return wtrRflctAlpha.Value;
			}
			return parent.WaterReflectionAlpha;
		}
		set
		{
			wtrRflctAlpha = value;
		}
	}

	public int Palette
	{
		get
		{
			if (pal.HasValue)
			{
				return pal.Value;
			}
			return parent.Palette;
		}
		set
		{
			pal = value;
		}
	}

	public int EffectColorA
	{
		get
		{
			if (eColA.HasValue)
			{
				return eColA.Value;
			}
			return parent.EffectColorA;
		}
		set
		{
			eColA = value;
		}
	}

	public int EffectColorB
	{
		get
		{
			if (eColB.HasValue)
			{
				return eColB.Value;
			}
			return parent.EffectColorB;
		}
		set
		{
			eColB = value;
		}
	}

	public float TerrainLight
	{
		get
		{
			return terrainLight ?? parent.TerrainLight;
		}
		set
		{
			terrainLight = value;
		}
	}

	public float TerrainStainAmount
	{
		get
		{
			return terrainStainAmount ?? parent.TerrainStainAmount;
		}
		set
		{
			terrainStainAmount = value;
		}
	}

	public float TerrainStainBrightness
	{
		get
		{
			return terrainStainBrightness ?? parent.TerrainStainBrightness;
		}
		set
		{
			terrainStainBrightness = value;
		}
	}

	public float TerrainStainHeight
	{
		get
		{
			return terrainStainHeight ?? parent.TerrainStainHeight;
		}
		set
		{
			terrainStainHeight = value;
		}
	}

	public float TerrainWaves
	{
		get
		{
			return terrainWaves ?? parent.TerrainWaves;
		}
		set
		{
			terrainWaves = value;
		}
	}

	public float TerrainEdgeRadius
	{
		get
		{
			return terrainEdgeRadius ?? parent.TerrainEdgeRadius;
		}
		set
		{
			terrainEdgeRadius = value;
		}
	}

	public float TerrainGooHeight
	{
		get
		{
			return terrainGooHeight ?? parent.TerrainGooHeight;
		}
		set
		{
			terrainGooHeight = value;
		}
	}

	public float TerrainGrain
	{
		get
		{
			return terrainGrain ?? parent.TerrainGrain;
		}
		set
		{
			terrainGrain = value;
		}
	}

	public float TerrainDepth
	{
		get
		{
			return terrainDepth ?? parent.TerrainDepth;
		}
		set
		{
			terrainDepth = value;
		}
	}

	public float TerrainSkyFade
	{
		get
		{
			return terrainSkyFade ?? parent.TerrainSkyFade;
		}
		set
		{
			terrainSkyFade = value;
		}
	}

	public string TerrainPalette
	{
		get
		{
			return terrainPalette ?? parent.TerrainPalette;
		}
		set
		{
			terrainPalette = value;
		}
	}

	public void Reset()
	{
		dType = null;
		cDrips = null;
		pal = null;
		eColA = null;
		eColB = null;
		effects = new List<RoomEffect>();
		ambientSounds = new List<AmbientSound>();
		placedObjects = new List<PlacedObject>();
		triggers = new List<EventTrigger>();
	}

	[Obsolete("Use timeline parameter constructor instead.")]
	public RoomSettings(string name, Region region, bool template, bool firstTemplate, SlugcatStats.Name playerChar)
		: this(null, name, region, template, firstTemplate, SlugcatStats.SlugcatToTimeline(playerChar), null)
	{
	}

	public RoomSettings(string name, Region region, bool template, bool firstTemplate, SlugcatStats.Timeline timelinePoint, RainWorldGame game)
		: this(null, name, region, template, firstTemplate, timelinePoint, null)
	{
	}

	public RoomSettings(Room room, string name, Region region, bool template, bool firstTemplate, SlugcatStats.Timeline timelinePoint, RainWorldGame game)
	{
		this.name = name.ToLowerInvariant();
		this.game = game;
		this.room = room;
		effects = new List<RoomEffect>();
		ambientSounds = new List<AmbientSound>();
		placedObjects = new List<PlacedObject>();
		triggers = new List<EventTrigger>();
		isTemplate = template;
		isFirstTemplate = firstTemplate;
		if (this.name == "roottemplate")
		{
			filePath = "";
			return;
		}
		if (template)
		{
			filePath = AssetManager.ResolveFilePath("world" + Path.DirectorySeparatorChar + region.name + Path.DirectorySeparatorChar + name + ".txt");
		}
		else
		{
			if (timelinePoint != null)
			{
				filePath = WorldLoader.FindRoomFile(name, includeRootDirectory: false, "_settings-" + timelinePoint.value + ".txt", showWarning: false);
			}
			if (filePath == null && ModManager.MSC && name.EndsWith("-2"))
			{
				filePath = WorldLoader.FindRoomFile(name.Substring(0, name.Length - 2), includeRootDirectory: false, "-2_settings.txt");
				if (filePath == null)
				{
					filePath = WorldLoader.FindRoomFile(name.Substring(0, name.Length - 2), includeRootDirectory: false, "_settings.txt");
				}
			}
			else if (filePath == null)
			{
				filePath = WorldLoader.FindRoomFile(name, includeRootDirectory: false, "_settings.txt");
			}
		}
		Reset();
		FindParent(region);
		if (!Load(timelinePoint))
		{
			string text = WorldLoader.FindRoomFile(name, includeRootDirectory: false, ".txt");
			if (text != null)
			{
				filePath = text.Substring(0, text.Length - 4) + "_settings.txt";
				Load((SlugcatStats.Timeline)null);
			}
		}
	}

	public void FindParent(Region region)
	{
		parent = DefaultRoomSettings.ancestor;
		if (region == null)
		{
			return;
		}
		if (!isTemplate && !isAncestor && region.roomSettingsTemplates.Length != 0)
		{
			parent = region.roomSettingsTemplates[0];
		}
		if (!isTemplate && region != null && File.Exists(filePath))
		{
			string[] array = File.ReadAllLines(filePath);
			for (int i = 0; i < array.Length; i++)
			{
				if (Regex.Split(array[i], ": ")[0] == "Template")
				{
					if (Regex.Split(array[i], ": ")[1] == "NONE")
					{
						parent = DefaultRoomSettings.ancestor;
					}
					else
					{
						parent = region.GetRoomSettingsTemplate(Regex.Split(Regex.Split(array[i].ToLowerInvariant(), ": ")[1], "_")[2]);
					}
					break;
				}
			}
		}
		InheritEffects();
		InheritAmbientSounds();
	}

	public void Save()
	{
		Save(filePath, saveAsTemplate: false);
	}

	[Obsolete("Use timeline parameter function instead.")]
	public void Save(SlugcatStats.Name slugcat)
	{
		Save(SlugcatStats.SlugcatToTimeline(slugcat));
	}

	public void Save(SlugcatStats.Timeline time)
	{
		Save(SpecificPath(time), saveAsTemplate: false);
		filePath = WorldLoader.FindRoomFile(name, includeRootDirectory: false, "_settings-" + time.value + ".txt");
	}

	[Obsolete("Use timeline parameter function instead.")]
	public string SpecificPath(SlugcatStats.Name id)
	{
		return SpecificPath(SlugcatStats.SlugcatToTimeline(id));
	}

	public string SpecificPath(SlugcatStats.Timeline time)
	{
		if (!filePath.Contains(".txt"))
		{
			return "";
		}
		return filePath.Substring(0, filePath.IndexOf("settings") + "settings".Length) + "-" + time.value + ".txt";
	}

	public void Save(string path, bool saveAsTemplate)
	{
		using StreamWriter streamWriter = File.CreateText(path);
		if (!saveAsTemplate)
		{
			if (parent.isAncestor)
			{
				streamWriter.WriteLine("Template: NONE");
			}
			else if (parent.isTemplate && !parent.isFirstTemplate)
			{
				streamWriter.WriteLine("Template: " + parent.name);
			}
		}
		if (dType != null || saveAsTemplate)
		{
			streamWriter.WriteLine("DangerType: " + DangerType.ToString());
		}
		if (cDrips.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("CeilingDrips: " + CeilingDrips);
		}
		if (rInts.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("RainIntensity: " + RainIntensity);
		}
		if (rumInts.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("RumbleIntensity: " + RumbleIntensity);
		}
		if (wSpeed.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("WaveSpeed: " + WaveSpeed);
		}
		if (wLength.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("WaveLength: " + WaveLength);
		}
		if (wAmp.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("WaveAmplitude: " + WaveAmplitude);
		}
		if (wLength.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("SecondWaveLength: " + SecondWaveLength);
		}
		if (wAmp.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("SecondWaveAmplitude: " + SecondWaveAmplitude);
		}
		if (clds.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("Clouds: " + Clouds);
		}
		if (grm.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("Grime: " + Grime);
		}
		if (bkgDrnVl.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("BkgDroneVolume: " + BkgDroneVolume);
		}
		if (bkgDrnNoThreatVol.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("BkgDroneNoThreatVolume: " + BkgDroneNoThreatVolume);
		}
		if (rndItmDns.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("RandomItemDensity: " + RandomItemDensity);
		}
		if (rndItmSprChnc.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("RandomItemSpearChance: " + RandomItemSpearChance);
		}
		if (wtrRflctAlpha.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("WaterReflectionAlpha: " + WaterReflectionAlpha);
		}
		if (pal.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("Palette: " + Palette);
		}
		if (eColA.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("EffectColorA: " + EffectColorA);
		}
		if (eColB.HasValue || saveAsTemplate)
		{
			streamWriter.WriteLine("EffectColorB: " + EffectColorB);
		}
		if (terrainLight.HasValue)
		{
			streamWriter.WriteLine("TerrainLight: " + TerrainLight);
		}
		if (terrainStainAmount.HasValue)
		{
			streamWriter.WriteLine("TerrainStainAmount: " + TerrainStainAmount);
		}
		if (terrainStainBrightness.HasValue)
		{
			streamWriter.WriteLine("TerrainStainBrightness: " + TerrainStainBrightness);
		}
		if (terrainStainHeight.HasValue)
		{
			streamWriter.WriteLine("TerrainStainHeight: " + TerrainStainHeight);
		}
		if (terrainWaves.HasValue)
		{
			streamWriter.WriteLine("TerrainWaves: " + TerrainWaves);
		}
		if (terrainEdgeRadius.HasValue)
		{
			streamWriter.WriteLine("TerrainEdgeRadius: " + TerrainEdgeRadius);
		}
		if (terrainGooHeight.HasValue)
		{
			streamWriter.WriteLine("TerrainGooHeight: " + TerrainGooHeight);
		}
		if (terrainGrain.HasValue)
		{
			streamWriter.WriteLine("TerrainGrain: " + TerrainGrain);
		}
		if (terrainDepth.HasValue)
		{
			streamWriter.WriteLine("TerrainDepth: " + TerrainDepth);
		}
		if (terrainSkyFade.HasValue)
		{
			streamWriter.WriteLine("TerrainSkyFade: " + TerrainSkyFade);
		}
		if (terrainPalette != null)
		{
			streamWriter.WriteLine("TerrainPalette: " + TerrainPalette.ToString());
		}
		if (effects.Count > 0)
		{
			string text = "";
			for (int i = 0; i < effects.Count; i++)
			{
				if (effects[i].save && (!effects[i].inherited || saveAsTemplate))
				{
					text = text + effects[i].ToString() + ", ";
				}
			}
			if (text != "")
			{
				streamWriter.WriteLine("Effects: " + text);
			}
		}
		if (!saveAsTemplate && placedObjects.Count > 0)
		{
			string text2 = "";
			for (int j = 0; j < placedObjects.Count; j++)
			{
				if (placedObjects[j].save)
				{
					text2 = text2 + placedObjects[j].ToString() + ", ";
				}
			}
			streamWriter.WriteLine("PlacedObjects: " + text2);
		}
		if (ambientSounds.Count > 0)
		{
			string text3 = "";
			for (int k = 0; k < ambientSounds.Count; k++)
			{
				if ((!ambientSounds[k].inherited || saveAsTemplate) && (ambientSounds[k].type != AmbientSound.Type.Spot || !saveAsTemplate))
				{
					text3 = text3 + ambientSounds[k].ToString() + ", ";
				}
			}
			if (text3 != "")
			{
				streamWriter.WriteLine("AmbientSounds: " + text3);
			}
		}
		if (triggers.Count > 0)
		{
			string text4 = "";
			for (int l = 0; l < triggers.Count; l++)
			{
				text4 = text4 + triggers[l].ToString() + ", ";
			}
			if (text4 != "")
			{
				streamWriter.WriteLine("Triggers: " + text4);
			}
		}
		if (fadePalette != null && !saveAsTemplate)
		{
			string text5 = "FadePalette: " + fadePalette.palette;
			for (int m = 0; m < fadePalette.fades.Length; m++)
			{
				text5 = text5 + ", " + fadePalette.fades[m];
			}
			streamWriter.WriteLine(text5);
		}
		if (terrainFadePalette != null && !saveAsTemplate)
		{
			string text6 = "TerrainFadePalette: " + terrainFadePalette.palette;
			for (int n = 0; n < terrainFadePalette.fades.Length; n++)
			{
				text6 = text6 + ", " + terrainFadePalette.fades[n];
			}
			streamWriter.WriteLine(text6);
		}
		if (roomSpecificScript && !saveAsTemplate)
		{
			streamWriter.WriteLine("RoomSpecificScriptActive: ON");
		}
		if (!wetTerrain && !saveAsTemplate)
		{
			streamWriter.WriteLine("WetTerrain: OFF");
		}
	}

	[Obsolete("Use timeline parameter function instead.")]
	public bool Load(SlugcatStats.Name playerChar)
	{
		return Load(SlugcatStats.SlugcatToTimeline(playerChar));
	}

	public bool Load(SlugcatStats.Timeline timelinePoint)
	{
		if (!File.Exists(filePath))
		{
			return false;
		}
		Reset();
		string[] array = File.ReadAllLines(filePath);
		List<string[]> list = new List<string[]>();
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], ": ");
			if (array2.Length == 2)
			{
				list.Add(array2);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			switch (list[j][0])
			{
			case "DangerType":
				DangerType = new RoomRain.DangerType(list[j][1]);
				break;
			case "CeilingDrips":
				CeilingDrips = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "RainIntensity":
				RainIntensity = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "RumbleIntensity":
				RumbleIntensity = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "WaveSpeed":
				WaveSpeed = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "WaveLength":
				WaveLength = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "WaveAmplitude":
				WaveAmplitude = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "SecondWaveLength":
				SecondWaveLength = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "SecondWaveAmplitude":
				SecondWaveAmplitude = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Clouds":
				Clouds = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Grime":
				Grime = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "BkgDroneVolume":
				BkgDroneVolume = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "BkgDroneNoThreatVolume":
				BkgDroneNoThreatVolume = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Palette":
				Palette = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "EffectColorA":
				EffectColorA = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "EffectColorB":
				EffectColorB = int.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Effects":
				LoadEffects(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "));
				break;
			case "PlacedObjects":
				LoadPlacedObjects(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "), timelinePoint);
				break;
			case "AmbientSounds":
				LoadAmbientSounds(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "));
				break;
			case "Triggers":
				LoadTriggers(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "));
				break;
			case "FadePalette":
				LoadFadePalette(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "));
				break;
			case "RoomSpecificScriptActive":
				roomSpecificScript = true;
				break;
			case "WetTerrain":
				wetTerrain = false;
				break;
			case "RandomItemDensity":
				RandomItemDensity = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "RandomItemSpearChance":
				RandomItemSpearChance = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "WaterReflectionAlpha":
				WaterReflectionAlpha = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainLight":
				TerrainLight = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainStainAmount":
				TerrainStainAmount = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainStainBrightness":
				TerrainStainBrightness = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainStainHeight":
				TerrainStainHeight = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainWaves":
				TerrainWaves = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainEdgeRadius":
				TerrainEdgeRadius = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainGooHeight":
				TerrainGooHeight = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainGrain":
				TerrainGrain = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainDepth":
				TerrainDepth = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainSkyFade":
				TerrainSkyFade = float.Parse(list[j][1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "TerrainPalette":
				TerrainPalette = list[j][1];
				break;
			case "TerrainFadePalette":
				LoadTerrainFadePalette(Regex.Split(Custom.ValidateSpacedDelimiter(list[j][1], ","), ", "));
				break;
			}
		}
		InheritEffects();
		InheritAmbientSounds();
		return true;
	}

	public void LoadEffects(string[] s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "-");
			string text = array[0];
			if (text != null)
			{
				if (text != null && text.Length == 0)
				{
					continue;
				}
				if (text == "Ripple Settings")
				{
					RippleEffectSettings rippleEffectSettings = new RippleEffectSettings(RoomEffect.Type.None, 0f, name);
					rippleEffectSettings.FromString(array);
					effects.Add(rippleEffectSettings);
					continue;
				}
			}
			effects.Add(new RoomEffect(RoomEffect.Type.None, 0f, isTemplate || isAncestor));
			effects[effects.Count - 1].FromString(array);
		}
	}

	[Obsolete("Use timeline parameter function instead.")]
	public void LoadPlacedObjects(string[] s, SlugcatStats.Name playerChar)
	{
		LoadPlacedObjects(s, SlugcatStats.SlugcatToTimeline(playerChar));
	}

	public void LoadPlacedObjects(string[] s, SlugcatStats.Timeline timelinePoint)
	{
		List<PlacedObject.GenericFilterData> list = new List<PlacedObject.GenericFilterData>();
		List<PlacedObject> list2 = new List<PlacedObject>();
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i].Trim(), "><");
			if (!(array[0] == ""))
			{
				placedObjects.Add(new PlacedObject(PlacedObject.Type.None, null));
				placedObjects[placedObjects.Count - 1].FromString(array);
				if (placedObjects[placedObjects.Count - 1].data is PlacedObject.GenericFilterData genericFilterData && !genericFilterData.Active(this, timelinePoint))
				{
					list.Add(genericFilterData);
				}
				else if (placedObjects[placedObjects.Count - 1].type == PlacedObject.Type.WarpFilter)
				{
					list2.Add(placedObjects[placedObjects.Count - 1]);
				}
			}
		}
		for (int j = 0; j < placedObjects.Count; j++)
		{
			if (!placedObjects[j].deactivattable)
			{
				continue;
			}
			for (int k = 0; k < list.Count; k++)
			{
				if (Custom.DistLess(placedObjects[j].pos, list[k].owner.pos, list[k].Rad))
				{
					list[k].DeactivatePlacedObject(placedObjects[j]);
					if (!placedObjects[j].active)
					{
						break;
					}
				}
			}
			for (int l = 0; l < list2.Count; l++)
			{
				if (Custom.DistLess(placedObjects[j].pos, list2[l].pos, (list2[l].data as PlacedObject.ResizableObjectData).Rad))
				{
					placedObjects[j].deactivatedByWarpFilter = true;
					break;
				}
			}
		}
	}

	public void LoadAmbientSounds(string[] s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "><");
			string text = array[0];
			if (text != null && (text == null || text.Length != 0))
			{
				switch (text)
				{
				case "OMNI":
					ambientSounds.Add(new OmniDirectionalSound(array[1], isTemplate || isAncestor));
					ambientSounds[ambientSounds.Count - 1].FromString(array);
					break;
				case "DIR":
					ambientSounds.Add(new DirectionalSound(array[1], isTemplate || isAncestor));
					ambientSounds[ambientSounds.Count - 1].FromString(array);
					break;
				case "SPOT":
					ambientSounds.Add(new SpotSound(array[1], isTemplate || isAncestor));
					ambientSounds[ambientSounds.Count - 1].FromString(array);
					break;
				}
			}
		}
	}

	public void LoadTriggers(string[] s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "<tA>");
			string text = array[0];
			if (text != null)
			{
				if (text != null && text.Length == 0)
				{
					continue;
				}
				if (text == "Spot")
				{
					triggers.Add(new SpotTrigger());
					triggers[triggers.Count - 1].FromString(array);
					continue;
				}
			}
			triggers.Add(new EventTrigger(new EventTrigger.TriggerType(array[0])));
			triggers[triggers.Count - 1].FromString(array);
		}
	}

	public void LoadFadePalette(string[] s)
	{
		fadePalette = new FadePalette(int.Parse(s[0], NumberStyles.Any, CultureInfo.InvariantCulture), s.Length - 1);
		for (int i = 0; i < s.Length - 1; i++)
		{
			fadePalette.fades[i] = float.Parse(s[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
	}

	public void LoadTerrainFadePalette(string[] s)
	{
		terrainFadePalette = new TerrainFadePalette(s[0], s.Length - 1);
		for (int i = 0; i < s.Length - 1; i++)
		{
			terrainFadePalette.fades[i] = float.Parse(s[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
	}

	public void InheritEffects()
	{
		if (isTemplate)
		{
			return;
		}
		for (int num = effects.Count - 1; num >= 0; num--)
		{
			if (effects[num].inherited)
			{
				effects.RemoveAt(num);
			}
		}
		for (int i = 0; i < parent.effects.Count; i++)
		{
			bool flag = true;
			for (int j = 0; j < effects.Count; j++)
			{
				if (effects[j].type == parent.effects[i].type)
				{
					effects[j].overWrite = true;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				effects.Add(parent.effects[i]);
			}
		}
	}

	public void InheritAmbientSounds()
	{
		if (isTemplate)
		{
			return;
		}
		for (int num = ambientSounds.Count - 1; num >= 0; num--)
		{
			if (ambientSounds[num].inherited)
			{
				ambientSounds.RemoveAt(num);
			}
		}
		for (int i = 0; i < parent.ambientSounds.Count; i++)
		{
			bool flag = true;
			for (int j = 0; j < ambientSounds.Count; j++)
			{
				if (ambientSounds[j].sample == parent.ambientSounds[i].sample && ambientSounds[j].type == parent.ambientSounds[i].type)
				{
					ambientSounds[j].overWrite = true;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ambientSounds.Add(parent.ambientSounds[i]);
			}
		}
	}

	public void RemoveEffect(RoomEffect.Type type)
	{
		for (int num = effects.Count - 1; num >= 0; num--)
		{
			if (effects[num].inherited || effects[num].type == type)
			{
				effects.RemoveAt(num);
			}
		}
		InheritEffects();
	}

	public void RemoveAmbientSound(AmbientSound.Type type, string sampleName)
	{
		for (int num = ambientSounds.Count - 1; num >= 0; num--)
		{
			if (ambientSounds[num].inherited || (ambientSounds[num].type == type && ambientSounds[num].sample == sampleName))
			{
				ambientSounds.RemoveAt(num);
			}
		}
		InheritAmbientSounds();
	}

	public void SetTemplate(string buttonText, Region region)
	{
		if (buttonText == "NONE")
		{
			parent = DefaultRoomSettings.ancestor;
		}
		else
		{
			parent = region.GetRoomSettingsTemplate(Regex.Split(buttonText, " - ")[1]);
		}
		InheritEffects();
		InheritAmbientSounds();
	}

	public void SaveAsTemplate(string buttonText, Region region)
	{
		SetTemplate(buttonText, region);
		if (parent.isAncestor || !parent.isTemplate)
		{
			Custom.LogWarning("Not a template!");
			return;
		}
		Save(parent.filePath, saveAsTemplate: true);
		parent.Load((SlugcatStats.Timeline)null);
		for (int i = 0; i < parent.effects.Count; i++)
		{
			parent.effects[i].inherited = true;
		}
		for (int j = 0; j < parent.ambientSounds.Count; j++)
		{
			parent.ambientSounds[j].inherited = true;
		}
		Reset();
		InheritEffects();
		InheritAmbientSounds();
	}

	public RoomEffect GetEffect(RoomEffect.Type type)
	{
		for (int i = 0; i < effects.Count; i++)
		{
			if (effects[i].type == type)
			{
				return effects[i];
			}
		}
		return null;
	}

	public float GetEffectAmount(RoomEffect.Type type)
	{
		for (int i = 0; i < effects.Count; i++)
		{
			if (effects[i].type == type)
			{
				return effects[i].amount;
			}
		}
		return 0f;
	}
}
