using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using DevInterface;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class PlacedObject
{
	public class Type : ExtEnum<Type>
	{
		public static Type None = new Type("None", register: true);

		public static Type LightSource = new Type("LightSource", register: true);

		public static Type FlareBomb = new Type("FlareBomb", register: true);

		public static Type PuffBall = new Type("PuffBall", register: true);

		public static Type TempleGuard = new Type("TempleGuard", register: true);

		public static Type LightFixture = new Type("LightFixture", register: true);

		public static Type DangleFruit = new Type("DangleFruit", register: true);

		public static Type CoralStem = new Type("CoralStem", register: true);

		public static Type CoralStemWithNeurons = new Type("CoralStemWithNeurons", register: true);

		public static Type CoralNeuron = new Type("CoralNeuron", register: true);

		public static Type CoralCircuit = new Type("CoralCircuit", register: true);

		public static Type WallMycelia = new Type("WallMycelia", register: true);

		public static Type ProjectedStars = new Type("ProjectedStars", register: true);

		public static Type ZapCoil = new Type("ZapCoil", register: true);

		public static Type SuperStructureFuses = new Type("SuperStructureFuses", register: true);

		public static Type GravityDisruptor = new Type("GravityDisruptor", register: true);

		public static Type SpotLight = new Type("SpotLight", register: true);

		public static Type DeepProcessing = new Type("DeepProcessing", register: true);

		public static Type Corruption = new Type("Corruption", register: true);

		public static Type CorruptionTube = new Type("CorruptionTube", register: true);

		public static Type CorruptionDarkness = new Type("CorruptionDarkness", register: true);

		public static Type StuckDaddy = new Type("StuckDaddy", register: true);

		public static Type SSLightRod = new Type("SSLightRod", register: true);

		public static Type CentipedeAttractor = new Type("CentipedeAttractor", register: true);

		public static Type DandelionPatch = new Type("DandelionPatch", register: true);

		public static Type GhostSpot = new Type("GhostSpot", register: true);

		public static Type DataPearl = new Type("DataPearl", register: true);

		public static Type UniqueDataPearl = new Type("UniqueDataPearl", register: true);

		public static Type SeedCob = new Type("SeedCob", register: true);

		public static Type DeadSeedCob = new Type("DeadSeedCob", register: true);

		public static Type WaterNut = new Type("WaterNut", register: true);

		public static Type JellyFish = new Type("JellyFish", register: true);

		public static Type KarmaFlower = new Type("KarmaFlower", register: true);

		public static Type Mushroom = new Type("Mushroom", register: true);

		public static Type SlimeMold = new Type("SlimeMold", register: true);

		public static Type FlyLure = new Type("FlyLure", register: true);

		public static Type CosmeticSlimeMold = new Type("CosmeticSlimeMold", register: true);

		public static Type CosmeticSlimeMold2 = new Type("CosmeticSlimeMold2", register: true);

		public static Type FirecrackerPlant = new Type("FirecrackerPlant", register: true);

		public static Type VultureGrub = new Type("VultureGrub", register: true);

		public static Type DeadVultureGrub = new Type("DeadVultureGrub", register: true);

		public static Type VoidSpawnEgg = new Type("VoidSpawnEgg", register: true);

		public static Type ReliableSpear = new Type("ReliableSpear", register: true);

		public static Type SuperJumpInstruction = new Type("SuperJumpInstruction", register: true);

		public static Type ProjectedImagePosition = new Type("ProjectedImagePosition", register: true);

		public static Type ExitSymbolShelter = new Type("ExitSymbolShelter", register: true);

		public static Type ExitSymbolHidden = new Type("ExitSymbolHidden", register: true);

		public static Type NoSpearStickZone = new Type("NoSpearStickZone", register: true);

		public static Type LanternOnStick = new Type("LanternOnStick", register: true);

		public static Type ScavengerOutpost = new Type("ScavengerOutpost", register: true);

		public static Type TradeOutpost = new Type("TradeOutpost", register: true);

		public static Type ScavengerTreasury = new Type("ScavengerTreasury", register: true);

		public static Type ScavTradeInstruction = new Type("ScavTradeInstruction", register: true);

		public static Type CustomDecal = new Type("CustomDecal", register: true);

		public static Type InsectGroup = new Type("InsectGroup", register: true);

		public static Type PlayerPushback = new Type("PlayerPushback", register: true);

		public static Type MultiplayerItem = new Type("MultiplayerItem", register: true);

		public static Type SporePlant = new Type("SporePlant", register: true);

		public static Type GoldToken = new Type("GoldToken", register: true);

		public static Type BlueToken = new Type("BlueToken", register: true);

		public static Type GreenToken = new Type("GreenToken", register: true);

		public static Type DeadTokenStalk = new Type("DeadTokenStalk", register: true);

		public static Type NeedleEgg = new Type("NeedleEgg", register: true);

		public static Type BrokenShelterWaterLevel = new Type("BrokenShelterWaterLevel", register: true);

		public static Type BubbleGrass = new Type("BubbleGrass", register: true);

		public static Type Filter = new Type("Filter", register: true);

		public static Type CompetitiveFilter = new Type("CompetitiveFilter", register: true);

		public static Type ReliableIggyDirection = new Type("ReliableIggyDirection", register: true);

		public static Type Hazer = new Type("Hazer", register: true);

		public static Type DeadHazer = new Type("DeadHazer", register: true);

		public static Type Rainbow = new Type("Rainbow", register: true);

		public static Type LightBeam = new Type("LightBeam", register: true);

		public static Type NoLeviathanStrandingZone = new Type("NoLeviathanStrandingZone", register: true);

		public static Type WindRect = new Type("WindRect", register: true);

		public static Type TerrainHandle = new Type("TerrainHandle", register: true);

		public static Type LocalTerrain = new Type("LocalTerrain", register: true);

		public static Type SuperSlope = new Type("SuperSlope", register: true);

		public static Type CurvedSlope = new Type("CurvedSlope", register: true);

		public static Type ShelterSpawnPoint = new Type("ShelterSpawnPoint", register: true);

		public static Type FluxWaterfall = new Type("FluxWaterfall", register: true);

		public static Type FluxDrain = new Type("FluxDrain", register: true);

		public static Type TerrainRubble = new Type("TerrainRubble", register: true);

		public static Type TerrainSunOffset = new Type("TerrainSunOffset", register: true);

		public static Type WaterCycleTop = new Type("WaterCycleTop", register: true);

		public static Type WaterCycleBottom = new Type("WaterCycleBottom", register: true);

		public static Type Geyser = new Type("Geyser", register: true);

		public static Type WaterCutoff = new Type("WaterCutoff", register: true);

		public static Type AirPocket = new Type("AirPocket", register: true);

		public static Type PlateTree = new Type("PlateTree", register: true);

		public static Type RotPlateTree = new Type("RotPlateTree", register: true);

		public static Type WaterCurrent = new Type("WaterCurrent", register: true);

		public static Type NoCurrentZone = new Type("NoCurrentZone", register: true);

		public static Type BlackSpot = new Type("BlackSpot", register: true);

		public static Type MudPit = new Type("MudPit", register: true);

		public static Type TerrainGrassPatch = new Type("TerrainGrassPatch", register: true);

		public static Type FairyParticleSettings = new Type("FairyParticleSettings", register: true);

		public static Type DayNightSettings = new Type("DayNightSettings", register: true);

		public static Type EnergySwirl = new Type("EnergySwirl", register: true);

		public static Type LightningMachine = new Type("LightningMachine", register: true);

		public static Type SteamPipe = new Type("SteamPipe", register: true);

		public static Type WallSteamer = new Type("WallSteamer", register: true);

		public static Type Vine = new Type("Vine", register: true);

		public static Type VultureMask = new Type("VultureMask", register: true);

		public static Type SnowSource = new Type("SnowSource", register: true);

		public static Type DeathFallFocus = new Type("DeathFallFocus", register: true);

		public static Type CellDistortion = new Type("CellDistortion", register: true);

		public static Type LocalBlizzard = new Type("LocalBlizzard", register: true);

		public static Type NeuronSpawner = new Type("NeuronSpawner", register: true);

		public static Type HangingPearls = new Type("HangingPearls", register: true);

		public static Type Lantern = new Type("Lantern", register: true);

		public static Type ExitSymbolAncientShelter = new Type("ExitSymbolAncientShelter", register: true);

		public static Type BlinkingFlower = new Type("BlinkingFlower", register: true);

		public static Type AdjustableFan = new Type("AdjustableFan", register: true);

		public static Type HarmfulSteam = new Type("HarmfulSteam", register: true);

		public static Type ARKillRect = new Type("ARKillRect", register: true);

		public static Type RainbowNoFade = new Type("RainbowNoFade", register: true);

		public static Type WarpPoint = new Type("WarpPoint", register: true);

		public static Type DynamicWarpTarget = new Type("DynamicWarpTarget", register: true);

		public static Type RippleLevelFilter = new Type("RippleLevelFilter", register: true);

		public static Type PrinceFilter = new Type("PrinceFilter", register: true);

		public static Type RippleEggFilter = new Type("RippleEggFilter", register: true);

		public static Type WarpFilter = new Type("WarpFilter", register: true);

		public static Type CosmeticRipple = new Type("CosmeticRipple", register: true);

		public static Type Pomegranate = new Type("Pomegranate", register: true);

		public static Type PomegranateVine = new Type("PomegranateVine", register: true);

		public static Type HugeTurbine = new Type("HugeTurbine", register: true);

		public static Type FloatingDebris = new Type("FloatingDebris", register: true);

		public static Type SandGrubHole = new Type("SandGrubHole", register: true);

		public static Type SandGrubNetwork = new Type("SandGrubNetwork", register: true);

		public static Type PassiveCorruption = new Type("PassiveCorruption", register: true);

		public static Type GooDrips = new Type("GooDrips", register: true);

		public static Type LobeTree = new Type("LobeTree", register: true);

		public static Type BrainMold = new Type("BrainMold", register: true);

		public static Type Rattler = new Type("Rattler", register: true);

		public static Type LocustCloud = new Type("LocustCloud", register: true);

		public static Type FlameJet = new Type("FlameJet", register: true);

		public static Type WindDirection = new Type("WindDirection", register: true);

		public static Type SkyWhalePathfinding = new Type("SkyWhalePathfinding", register: true);

		public static Type RippleSpawnEgg = new Type("RippleSpawnEgg", register: true);

		public static Type RippleEggDestination = new Type("RippleEggDestination", register: true);

		public static Type HideVoidSpawn = new Type("HideVoidSpawn", register: true);

		public static Type SpawnMigrationStream = new Type("SpawnMigrationStream", register: true);

		public static Type RippleTree = new Type("RippleTree", register: true);

		public static Type RippleStalk = new Type("RippleStalk", register: true);

		public static Type NoRippleSproutZone = new Type("NoRippleSproutZone", register: true);

		public static Type GraffitiBomb = new Type("GraffitiBomb", register: true);

		public Type(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public abstract class Data
	{
		public PlacedObject owner;

		public string[] unrecognizedAttributes;

		public Data(PlacedObject owner)
		{
			this.owner = owner;
		}

		public override string ToString()
		{
			return "";
		}

		public virtual void FromString(string s)
		{
		}

		public virtual void RefreshLiveVisuals()
		{
		}
	}

	public class LightSourceData : Data
	{
		public class ColorType : ExtEnum<ColorType>
		{
			public static ColorType Environment = new ColorType("Environment", register: true);

			public static ColorType White = new ColorType("White", register: true);

			public static ColorType EffectColor1 = new ColorType("EffectColor1", register: true);

			public static ColorType EffectColor2 = new ColorType("EffectColor2", register: true);

			public ColorType(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public class BlinkType : ExtEnum<BlinkType>
		{
			public static BlinkType None = new BlinkType("None", register: true);

			public static BlinkType Flash = new BlinkType("Flash", register: true);

			public static BlinkType Fade = new BlinkType("Fade", register: true);

			public BlinkType(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Vector2 panelPos;

		public Vector2 handlePos;

		public float strength;

		public bool fadeWithSun;

		public bool flat;

		public ColorType colorType = ColorType.Environment;

		public BlinkType blinkType;

		public float blinkRate;

		public bool nightLight;

		public float Rad => handlePos.magnitude;

		public LightSourceData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 100f);
			panelPos = Custom.DegToVec(30f) * 100f;
			colorType = ColorType.Environment;
			strength = 1f;
			fadeWithSun = true;
			flat = false;
			blinkType = BlinkType.None;
			blinkRate = 0f;
			nightLight = false;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			strength = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			colorType = new ColorType(array[1]);
			handlePos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			fadeWithSun = int.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			flat = int.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			if (array.Length > 8)
			{
				blinkType = new BlinkType(array[8]);
				blinkRate = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			if (array.Length > 10)
			{
				nightLight = int.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			}
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 11);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}", strength, colorType.ToString(), handlePos.x, handlePos.y, panelPos.x, panelPos.y, fadeWithSun ? "1" : "0", flat ? "1" : "0", blinkType.value, blinkRate.ToString(), nightLight ? "1" : "0");
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class ResizableObjectData : Data
	{
		public Vector2 handlePos;

		public float Rad => handlePos.magnitude;

		public ResizableObjectData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 100f);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 2);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}", handlePos.x, handlePos.y);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class ScavengerOutpostData : ResizableObjectData
	{
		public Vector2 panelPos;

		public float direction;

		public int skullSeed;

		public int pearlsSeed;

		public ScavengerOutpostData(PlacedObject owner)
			: base(owner)
		{
			System.Random random = new System.Random();
			direction = (float)random.NextDouble();
			skullSeed = random.Next(0, 101);
			pearlsSeed = random.Next(0, 101);
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			direction = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			skullSeed = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			pearlsSeed = int.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}", panelPos.x, panelPos.y, direction, skullSeed, pearlsSeed);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class InsectGroupData : ResizableObjectData
	{
		public Vector2 panelPos;

		public float density;

		public CosmeticInsect.Type insectType;

		public InsectGroupData(PlacedObject owner)
			: base(owner)
		{
			insectType = CosmeticInsect.Type.RockFlea;
			density = 0.5f;
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			insectType = new CosmeticInsect.Type(array[4]);
			density = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}", panelPos.x, panelPos.y, insectType.ToString(), density);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class ConsumableObjectData : Data
	{
		public Vector2 panelPos;

		public int minRegen;

		public int maxRegen;

		public ConsumableObjectData(PlacedObject owner)
			: base(owner)
		{
			if (owner.type == Type.DataPearl || owner.type == Type.UniqueDataPearl || owner.type == Type.GoldToken || owner.type == Type.BlueToken)
			{
				minRegen = 0;
			}
			else if (owner.type == Type.SeedCob || owner.type == Type.VultureGrub || owner.type == Type.DeadVultureGrub || owner.type == Type.SporePlant || owner.type == Type.Hazer || owner.type == Type.DeadHazer || owner.type == Type.Pomegranate)
			{
				minRegen = 7;
				maxRegen = 10;
			}
			else if (owner.type == Type.KarmaFlower)
			{
				minRegen = 5;
				maxRegen = 7;
			}
			else if (owner.type == Type.FlyLure || owner.type == Type.NeedleEgg || owner.type == Type.BubbleGrass || owner.type == Type.Lantern)
			{
				minRegen = 4;
				maxRegen = 9;
			}
			else
			{
				minRegen = 2;
				maxRegen = 3;
			}
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			if (array.Length > 3)
			{
				panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
				panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
			}
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}", panelPos.x, panelPos.y, minRegen, maxRegen);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class LightFixtureData : Data
	{
		public class Type : ExtEnum<Type>
		{
			public static Type RedLight = new Type("RedLight", register: true);

			public static Type HolyFire = new Type("HolyFire", register: true);

			public static Type ZapCoilLight = new Type("ZapCoilLight", register: true);

			public static Type DeepProcessing = new Type("DeepProcessing", register: true);

			public static Type SlimeMoldLight = new Type("SlimeMoldLight", register: true);

			public static Type RedSubmersible = new Type("RedSubmersible", register: true);

			public static Type GlowWeedLight = new Type("GlowWeedLight", register: true);

			public Type(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Vector2 panelPos;

		public Type type = Type.RedLight;

		public int randomSeed;

		public LightFixtureData(PlacedObject owner, Type type)
			: base(owner)
		{
			this.type = type;
			panelPos = Custom.DegToVec(120f) * 20f;
			System.Random random = new System.Random();
			randomSeed = random.Next(0, 101);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			type = new Type(array[0]);
			panelPos.x = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			randomSeed = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}", type.ToString(), panelPos.x, panelPos.y, randomSeed);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class MultiplayerItemData : Data
	{
		public class Type : ExtEnum<Type>
		{
			public static Type Rock = new Type("Rock", register: true);

			public static Type Spear = new Type("Spear", register: true);

			public static Type ExplosiveSpear = new Type("ExplosiveSpear", register: true);

			public static Type Bomb = new Type("Bomb", register: true);

			public static Type SporePlant = new Type("SporePlant", register: true);

			public Type(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Vector2 panelPos;

		public Type type = Type.Rock;

		public float chance;

		public MultiplayerItemData(PlacedObject owner)
			: base(owner)
		{
			panelPos = Custom.DegToVec(120f) * 20f;
			chance = 1f;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			type = new Type(array[0]);
			panelPos.x = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			chance = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}", type.ToString(), panelPos.x, panelPos.y, chance);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class SSLightRodData : Data
	{
		public Vector2 panelPos;

		public float depth;

		public float rotation;

		public float length = 80f;

		public float brightness = 0.5f;

		public SSLightRodData(PlacedObject owner)
			: base(owner)
		{
			panelPos = Custom.DegToVec(120f) * 20f;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			depth = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			rotation = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			length = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			brightness = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", panelPos.x, panelPos.y, depth, rotation, length, brightness);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class GridRectObjectData : Data
	{
		public Vector2 handlePos;

		public IntRect Rect => new IntRect(Mathf.FloorToInt(Mathf.Min(owner.pos.x, owner.pos.x + handlePos.x) / 20f), Mathf.FloorToInt(Mathf.Min(owner.pos.y, owner.pos.y + handlePos.y) / 20f), Mathf.FloorToInt(Mathf.Max(owner.pos.x, owner.pos.x + handlePos.x) / 20f), Mathf.FloorToInt(Mathf.Max(owner.pos.y, owner.pos.y + handlePos.y) / 20f));

		public GridRectObjectData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(40f, 40f);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 2);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}", handlePos.x, handlePos.y);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class QuadObjectData : Data
	{
		public Vector2[] handles;

		public QuadObjectData(PlacedObject owner)
			: base(owner)
		{
			handles = new Vector2[3];
			handles[0] = new Vector2(0f, 40f);
			handles[1] = new Vector2(40f, 40f);
			handles[2] = new Vector2(40f, 0f);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handles[0].x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handles[0].y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			handles[1].x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			handles[1].y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			handles[2].x = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			handles[2].y = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", handles[0].x, handles[0].y, handles[1].x, handles[1].y, handles[2].x, handles[2].y);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class DeepProcessingData : QuadObjectData
	{
		public Vector2 panelPos;

		public float fromDepth;

		public float toDepth = 1f;

		public float intensity = 0.5f;

		public DeepProcessingData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			fromDepth = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			toDepth = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			intensity = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 11);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}", panelPos.x, panelPos.y, fromDepth, toDepth, intensity);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class CustomDecalData : QuadObjectData
	{
		public Vector2 panelPos;

		public float fromDepth;

		public float toDepth = 1f;

		public float noise;

		public float[,] vertices;

		public string imageName = "ph";

		public CustomDecalData(PlacedObject owner)
			: base(owner)
		{
			vertices = new float[4, 2];
			for (int i = 0; i < vertices.GetLength(0); i++)
			{
				vertices[i, 0] = 0.5f;
			}
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			fromDepth = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			toDepth = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			noise = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			imageName = array[11].ToLowerInvariant();
			if (array.Length >= 20)
			{
				for (int i = 12; i < array.Length; i++)
				{
					if ((i - 12) % 2 == 0)
					{
						vertices[(i - 12) / 2, 0] = float.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					else
					{
						vertices[(i - 12) / 2, 1] = float.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
				}
				unrecognizedAttributes = null;
			}
			else
			{
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 12);
			}
		}

		public override string ToString()
		{
			string text = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}", panelPos.x, panelPos.y, fromDepth, toDepth, noise, imageName);
			for (int i = 0; i < vertices.GetLength(0); i++)
			{
				for (int j = 0; j < vertices.GetLength(1); j++)
				{
					text += string.Format(CultureInfo.InvariantCulture, "~{0}", vertices[i, j]);
				}
			}
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
		}
	}

	public class DataPearlData : ConsumableObjectData
	{
		public DataPearl.AbstractDataPearl.DataPearlType pearlType = DataPearl.AbstractDataPearl.DataPearlType.Misc;

		public bool hidden;

		public DataPearlData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			if (array.Length >= 5)
			{
				if (int.TryParse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					pearlType = BackwardsCompatibilityRemix.ParseDataPearl(result);
				}
				else
				{
					pearlType = new DataPearl.AbstractDataPearl.DataPearlType(array[4]);
				}
				hidden = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
			}
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}", pearlType, hidden ? "1" : "0");
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class VoidSpawnEggData : ConsumableObjectData
	{
		public int exit;

		public VoidSpawnEggData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			if (array.Length >= 5)
			{
				exit = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
			}
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}", exit);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class RippleSpawnEggData : Data
	{
		public float threshold = -1f;

		public string destRoom;

		public RippleSpawnEggData(PlacedObject owner)
			: base(owner)
		{
		}
	}

	public abstract class GenericFilterData : ResizableObjectData
	{
		public GenericFilterData(PlacedObject owner)
			: base(owner)
		{
		}

		public abstract bool Active(RoomSettings roomSettings, SlugcatStats.Timeline timelinePoint);

		public virtual void DeactivatePlacedObject(PlacedObject pObj)
		{
			pObj.active = false;
		}
	}

	public class FilterData : GenericFilterData
	{
		public Vector2 panelPos;

		public List<SlugcatStats.Name> availableToPlayers;

		public List<SlugcatStats.Timeline> availableOnTimelines;

		public FilterData(PlacedObject owner)
			: base(owner)
		{
			availableToPlayers = new List<SlugcatStats.Name>();
			for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
			{
				SlugcatStats.Name name = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(i));
				if (!SlugcatStats.HiddenOrUnplayableSlugcat(name))
				{
					availableToPlayers.Add(name);
				}
			}
			RefreshTimelineList();
		}

		public override bool Active(RoomSettings roomSettings, SlugcatStats.Timeline timelinePoint)
		{
			if (!(timelinePoint == null))
			{
				return availableOnTimelines.Contains(timelinePoint);
			}
			return true;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			if (Custom.IsDigitString(array[4]))
			{
				BackwardsCompatibilityRemix.ParsePlayerAvailability(array[4], availableToPlayers);
			}
			else
			{
				availableToPlayers.Clear();
				List<string> list = new List<string>(array[4].Split('|'));
				for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
				{
					string entry = ExtEnum<SlugcatStats.Name>.values.GetEntry(i);
					SlugcatStats.Name name = new SlugcatStats.Name(entry);
					if (!SlugcatStats.HiddenOrUnplayableSlugcat(name) && !list.Contains(entry))
					{
						availableToPlayers.Add(name);
					}
				}
			}
			RefreshTimelineList();
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		}

		public override string ToString()
		{
			List<string> list = new List<string>();
			for (int i = 0; i < ExtEnum<SlugcatStats.Name>.values.Count; i++)
			{
				string entry = ExtEnum<SlugcatStats.Name>.values.GetEntry(i);
				SlugcatStats.Name name = new SlugcatStats.Name(entry);
				if (!SlugcatStats.HiddenOrUnplayableSlugcat(name) && !availableToPlayers.Contains(name))
				{
					list.Add(entry);
				}
			}
			string text = string.Join("|", list.ToArray());
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, text);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public void RefreshTimelineList()
		{
			availableOnTimelines = new List<SlugcatStats.Timeline>();
			for (int i = 0; i < availableToPlayers.Count; i++)
			{
				availableOnTimelines.Add(SlugcatStats.SlugcatToTimeline(availableToPlayers[i]));
			}
		}
	}

	public class CompetitiveFilterData : GenericFilterData
	{
		public Vector2 panelPos;

		public string name = "NONE";

		public CompetitiveFilterData(PlacedObject owner)
			: base(owner)
		{
		}

		public override bool Active(RoomSettings roomSettings, SlugcatStats.Timeline timelinePoint)
		{
			if (!(roomSettings.game.session is CompetitiveGameSession competitiveGameSession))
			{
				return false;
			}
			foreach (ArenaCreatureSpawner.FilterSpawner spawnFilter in competitiveGameSession.spawnFilters)
			{
				if (spawnFilter.name == name)
				{
					return spawnFilter.rolled;
				}
			}
			return false;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			name = array[4];
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, name);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class RippleLevelFilterData : GenericFilterData
	{
		public Vector2 panelPos;

		public float minimumRippleLevel;

		public float maximumRippleLevel;

		public RippleLevelFilterData(PlacedObject owner)
			: base(owner)
		{
		}

		public override bool Active(RoomSettings roomSettings, SlugcatStats.Timeline timelinePoint)
		{
			if (roomSettings.game == null || !roomSettings.game.IsStorySession)
			{
				return false;
			}
			if (roomSettings.game.GetStorySession.saveState.deathPersistentSaveData.rippleLevel >= minimumRippleLevel)
			{
				return roomSettings.game.GetStorySession.saveState.deathPersistentSaveData.rippleLevel <= maximumRippleLevel;
			}
			return false;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			minimumRippleLevel = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			maximumRippleLevel = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, minimumRippleLevel, maximumRippleLevel);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class PrinceFilterData : GenericFilterData
	{
		public Vector2 panelPos;

		public int stage;

		public bool greaterOrEqual;

		public bool postGame;

		public PrinceFilterData(PlacedObject owner)
			: base(owner)
		{
		}

		public override bool Active(RoomSettings roomSettings, SlugcatStats.Timeline timelinePoint)
		{
			if (roomSettings.game == null || !roomSettings.game.IsStorySession)
			{
				return false;
			}
			if (postGame)
			{
				if (!roomSettings.game.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot)
				{
					return roomSettings.game.GetStorySession.finalWarpSequenceStarted;
				}
				return true;
			}
			if (greaterOrEqual)
			{
				return roomSettings.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters >= stage;
			}
			return roomSettings.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters <= stage;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			stage = ((array.Length > 4) ? int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture) : stage);
			greaterOrEqual = ((array.Length > 5) ? (array[5] == "1") : greaterOrEqual);
			postGame = ((array.Length > 6) ? (array[6] == "1") : postGame);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, stage, greaterOrEqual ? "1" : "0", postGame ? "1" : "0");
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class RippleEggFilterData : GenericFilterData
	{
		public Vector2 panelPos;

		public float threshold;

		public RippleEggFilterData(PlacedObject owner)
			: base(owner)
		{
		}

		public override bool Active(RoomSettings roomSettings, SlugcatStats.Timeline timelinePoint)
		{
			if (roomSettings.game == null || !roomSettings.game.IsStorySession)
			{
				return false;
			}
			if (roomSettings.game.GetStorySession.saveState.miscWorldSaveData.hasRippleEggWarpAbility)
			{
				return true;
			}
			return false;
		}

		public override void DeactivatePlacedObject(PlacedObject pObj)
		{
			if (pObj.data is RippleSpawnEggData rippleSpawnEggData)
			{
				rippleSpawnEggData.threshold = threshold;
			}
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			threshold = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, threshold);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class RippleEggDestinationData : GenericFilterData
	{
		public Vector2 panelPos;

		public string destRoom;

		public bool always;

		public RippleEggDestinationData(PlacedObject owner)
			: base(owner)
		{
		}

		public override bool Active(RoomSettings roomSettings, SlugcatStats.Timeline timelinePoint)
		{
			if (always || roomSettings.game == null || !roomSettings.game.IsStorySession)
			{
				return false;
			}
			foreach (string key in roomSettings.game.GetStorySession.saveState.miscWorldSaveData.discoveredWarpPoints.Keys)
			{
				if (key.Split(':')[0].ToLowerInvariant() == destRoom)
				{
					return true;
				}
			}
			return false;
		}

		public override void DeactivatePlacedObject(PlacedObject pObj)
		{
			if (pObj.data is RippleSpawnEggData rippleSpawnEggData)
			{
				rippleSpawnEggData.destRoom = destRoom;
			}
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			destRoom = array[4];
			always = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, destRoom, always ? "1" : "0");
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class CellDistortionData : Data
	{
		public Vector2 handlePos;

		public Vector2 panelPos;

		public float intensity;

		public float scale;

		public float chromaticIntensity;

		public float timeMult;

		public float Rad => handlePos.magnitude;

		public CellDistortionData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 100f);
			panelPos = Custom.DegToVec(30f) * 100f;
			intensity = 1f;
			scale = 0.5f;
			chromaticIntensity = 0f;
			timeMult = 0f;
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, intensity, scale, chromaticIntensity, timeMult);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			intensity = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			scale = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			chromaticIntensity = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			timeMult = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8);
		}
	}

	public class OEsphereData : Data
	{
		public Vector2 handlePos;

		public Vector2 panelPos;

		public int depth;

		public float lIntensity;

		public float Rad => handlePos.magnitude;

		public OEsphereData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 100f);
			panelPos = Custom.DegToVec(30f) * 100f;
			depth = 0;
			lIntensity = 1f;
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, depth, lIntensity);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			depth = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			lIntensity = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
		}
	}

	public class SnowSourceData : Data
	{
		public class Shape : ExtEnum<Shape>
		{
			public static Shape None = new Shape("None", register: true);

			public static Shape Radial = new Shape("Radial", register: true);

			public static Shape Strip = new Shape("Strip", register: true);

			public static Shape Column = new Shape("Column", register: true);

			public static Shape UnSnow = new Shape("UnSnow", register: true);

			public Shape(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Shape shape;

		public Vector2 handlePos;

		public Vector2 panelPos;

		public float intensity;

		public float noisiness;

		public float Rad => handlePos.magnitude;

		public SnowSourceData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 100f);
			panelPos = Custom.DegToVec(30f) * 100f;
			shape = Shape.Radial;
			intensity = 1f;
			noisiness = 0f;
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", shape.value, handlePos.x, handlePos.y, panelPos.x, panelPos.y, intensity, noisiness);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			shape = new Shape(array[0]);
			handlePos.x = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			intensity = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			noisiness = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
		}
	}

	public class LocalBlizzardData : Data
	{
		public Vector2 handlePos;

		public Vector2 panelPos;

		public float intensity;

		public float scale;

		public float angle;

		public float Rad => handlePos.magnitude;

		public LocalBlizzardData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 100f);
			panelPos = Custom.DegToVec(30f) * 100f;
			intensity = 1f;
			scale = 0.5f;
			angle = 0f;
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, intensity, scale, angle);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			intensity = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			scale = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			angle = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
		}
	}

	public class LightningMachineData : Data
	{
		public Vector2 panelPos;

		public Vector2 pos;

		public Vector2 startPoint;

		public Vector2 endPoint;

		public float chance;

		public bool permanent;

		public bool radial;

		public float width;

		public float intensity;

		public float lifeTime;

		public float lightningParam;

		public float lightningType;

		public int impact;

		public float volume;

		public int soundType;

		public bool random;

		public bool light;

		public LightningMachineData(PlacedObject owner)
			: base(owner)
		{
			panelPos = Custom.DegToVec(90f) * 50f;
			startPoint = Custom.DegToVec(-30f) * 100f;
			endPoint = Custom.DegToVec(-90f) * 100f;
			lightningParam = 1f;
			lightningType = 1f;
			width = 0.3f;
			intensity = 0.8f;
			lifeTime = 0.3f;
			chance = 0.1f;
			volume = 0.5f;
			random = false;
			light = false;
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}~{17}~{18}~{19}~{20}", panelPos.x, panelPos.y, pos.x, pos.y, startPoint.x, startPoint.y, endPoint.x, endPoint.y, chance, permanent ? "1" : "0", radial ? "1" : "0", width, intensity, lifeTime, lightningParam, lightningType, impact, volume, soundType, random ? "1" : "0", light ? "1" : "0");
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			pos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			pos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			startPoint.x = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			startPoint.y = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			endPoint.x = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			endPoint.y = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			chance = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			permanent = int.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			radial = int.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			width = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
			intensity = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
			lifeTime = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
			lightningParam = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
			lightningType = float.Parse(array[15], NumberStyles.Any, CultureInfo.InvariantCulture);
			impact = int.Parse(array[16], NumberStyles.Any, CultureInfo.InvariantCulture);
			volume = float.Parse(array[17], NumberStyles.Any, CultureInfo.InvariantCulture);
			soundType = int.Parse(array[18], NumberStyles.Any, CultureInfo.InvariantCulture);
			random = int.Parse(array[19], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			light = int.Parse(array[20], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 21);
		}
	}

	public class EnergySwirlData : Data
	{
		public class ColorType : ExtEnum<ColorType>
		{
			public static ColorType Environment = new ColorType("Environment", register: true);

			public static ColorType White = new ColorType("White", register: true);

			public static ColorType EffectColor1 = new ColorType("EffectColor1", register: true);

			public static ColorType EffectColor2 = new ColorType("EffectColor2", register: true);

			public ColorType(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public ColorType colorType;

		public Vector2 handlePos;

		public Vector2 panelPos;

		public float depth;

		public float Rad => handlePos.magnitude;

		public EnergySwirlData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 100f);
			panelPos = Custom.DegToVec(30f) * 100f;
			colorType = ColorType.EffectColor1;
			depth = 0f;
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", colorType.value, handlePos.x, handlePos.y, panelPos.x, panelPos.y, depth);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			colorType = new ColorType(array[0]);
			handlePos.x = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.x = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			depth = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
		}
	}

	public class SteamPipeData : Data
	{
		public Vector2 handlePos;

		public float Rad => handlePos.magnitude;

		public SteamPipeData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 100f);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}", handlePos.x, handlePos.y);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 2);
		}
	}

	public class DayNightData : Data
	{
		public Vector2 panelPos;

		public int duskPalette;

		public int nightPalette;

		public DayNightData(PlacedObject owner)
			: base(owner)
		{
			panelPos = Custom.DegToVec(30f) * 10f;
			duskPalette = 23;
			nightPalette = 10;
		}

		public void Apply(Room room)
		{
			room.world.rainCycle.duskPalette = duskPalette;
			room.world.rainCycle.nightPalette = nightPalette;
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}", panelPos.x, panelPos.y, duskPalette, nightPalette);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			duskPalette = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			nightPalette = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
		}
	}

	public class FairyParticleData : Data
	{
		public class SpriteType : ExtEnum<SpriteType>
		{
			public static SpriteType Pixel = new SpriteType("Pixel", register: true);

			public static SpriteType Circle = new SpriteType("Circle", register: true);

			public static SpriteType Dandelion = new SpriteType("Dandelion", register: true);

			public static SpriteType Star_Thin = new SpriteType("Star_Thin", register: true);

			public static SpriteType Star_Thick = new SpriteType("Star_Thick", register: true);

			public static SpriteType Bubble_Thick = new SpriteType("Bubble_Thick", register: true);

			public static SpriteType Bubble_Thin = new SpriteType("Bubble_Thin", register: true);

			public static SpriteType Bubble_Thinnest = new SpriteType("Bubble_Thinnest", register: true);

			public static SpriteType Leaf = new SpriteType("Leaf", register: true);

			public static SpriteType Glyph = new SpriteType("Glyph", register: true);

			public static SpriteType AncientGlyph = new SpriteType("AncientGlyph", register: true);

			public SpriteType(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public SpriteType spriteType;

		public Vector2 panelPos;

		public bool absPulse;

		public float scaleMin;

		public float scaleMax;

		public float dirMin;

		public float dirMax;

		public float dirDevMin;

		public float dirDevMax;

		public float colorHmin;

		public float colorHmax;

		public float colorSmin;

		public float colorSmax;

		public float colorLmin;

		public float colorLmax;

		public float alphaTrans;

		public float numKeyframes;

		public float interpDistMin;

		public float interpDistMax;

		public float interpDurMin;

		public float interpDurMax;

		public float interpTrans;

		public float pulseMin;

		public float pulseMax;

		public float pulseRate;

		public float glowRad;

		public float glowStrength;

		public float rotationRate;

		public FairyParticle.LerpMethod dirLerpType;

		public FairyParticle.LerpMethod speedLerpType;

		public FairyParticleData(PlacedObject owner)
			: base(owner)
		{
			panelPos = Custom.DegToVec(30f) * 10f;
			spriteType = SpriteType.Pixel;
			absPulse = false;
			dirLerpType = FairyParticle.LerpMethod.SIN_IO;
			speedLerpType = FairyParticle.LerpMethod.SIN_IO;
			pulseMin = 1f;
			pulseMax = 1f;
			pulseRate = 0f;
			scaleMin = 1f;
			scaleMax = 4f;
			interpDurMin = 60f;
			interpDurMax = 180f;
			interpDistMin = 40f;
			interpDistMax = 100f;
			dirDevMin = 5f;
			dirDevMax = 30f;
			dirMin = 0f;
			dirMax = 360f;
			colorHmin = 0.5f;
			colorSmin = 1f;
			colorLmin = 0.5f;
			colorHmax = 0.7f;
			colorSmax = 1f;
			colorLmax = 1f;
			interpTrans = 0.5f;
			alphaTrans = 0.75f;
			numKeyframes = 4f;
			glowRad = 80f;
			glowStrength = 0.5f;
			rotationRate = 0f;
		}

		public void Apply(Room room)
		{
			foreach (UpdatableAndDeletable update in room.updateList)
			{
				if (!(update is FairyParticle))
				{
					continue;
				}
				FairyParticle fairyParticle = update as FairyParticle;
				SpriteType spriteType = this.spriteType;
				if (spriteType == SpriteType.Pixel)
				{
					fairyParticle.spriteName = "pixel";
					fairyParticle.scale_multiplier = 1f;
				}
				else if (spriteType == SpriteType.Circle)
				{
					fairyParticle.spriteName = "Circle20";
					fairyParticle.scale_multiplier = 0.05f;
				}
				else if (spriteType == SpriteType.Dandelion)
				{
					fairyParticle.spriteName = "SkyDandelion";
					fairyParticle.scale_multiplier = 0.1f;
				}
				else if (spriteType == SpriteType.Star_Thick)
				{
					fairyParticle.spriteName = "mouseEyeA5";
					fairyParticle.scale_multiplier = 0.1f;
				}
				else if (spriteType == SpriteType.Star_Thin)
				{
					fairyParticle.spriteName = "mouseSparkB";
					fairyParticle.scale_multiplier = 0.1f;
				}
				else if (spriteType == SpriteType.Bubble_Thick)
				{
					fairyParticle.spriteName = "LizardBubble3";
					fairyParticle.scale_multiplier = 0.1f;
				}
				else if (spriteType == SpriteType.Bubble_Thin)
				{
					fairyParticle.spriteName = "LizardBubble6";
					fairyParticle.scale_multiplier = 0.1f;
				}
				else if (spriteType == SpriteType.Bubble_Thinnest)
				{
					fairyParticle.spriteName = "LizardBubble7";
					fairyParticle.scale_multiplier = 0.1f;
				}
				else if (spriteType == SpriteType.Leaf)
				{
					float value = UnityEngine.Random.value;
					if (value < 0.17f)
					{
						fairyParticle.spriteName = "deerEyeA";
					}
					else if (value < 0.34f)
					{
						fairyParticle.spriteName = "Pebble12";
					}
					else if (value < 0.51f)
					{
						fairyParticle.spriteName = "LizardArm_10";
					}
					else if (value < 0.68f)
					{
						fairyParticle.spriteName = "LizardArm_20";
					}
					else if (value < 0.85f)
					{
						fairyParticle.spriteName = "JellyFish2A";
					}
					else
					{
						fairyParticle.spriteName = "DangleFruit2A";
					}
					fairyParticle.scale_multiplier = 0.1f;
				}
				else if (this.spriteType == SpriteType.Glyph)
				{
					fairyParticle.spriteName = "haloGlyph" + UnityEngine.Random.Range(0, 7);
					fairyParticle.scale_multiplier = 0.1f;
				}
				else if (this.spriteType == SpriteType.AncientGlyph)
				{
					fairyParticle.spriteName = "BigGlyph" + UnityEngine.Random.Range(0, 13);
					fairyParticle.scale_multiplier = 0.1f;
				}
				fairyParticle.scale_min = scaleMin;
				fairyParticle.scale_max = scaleMax;
				fairyParticle.direction_min = dirMin;
				fairyParticle.direction_max = dirMax;
				fairyParticle.dir_deviation_min = dirDevMin;
				fairyParticle.dir_deviation_max = dirDevMax;
				fairyParticle.interp_dir_method = dirLerpType;
				fairyParticle.minHSL = new Vector3(colorHmin, colorSmin, colorLmin);
				fairyParticle.maxHSL = new Vector3(colorHmax, colorSmax, colorLmax);
				fairyParticle.alpha_trans_ratio = alphaTrans;
				fairyParticle.num_keyframes = (int)numKeyframes;
				fairyParticle.interp_speed_method = speedLerpType;
				fairyParticle.interp_dist_min = interpDistMin;
				fairyParticle.interp_dist_max = interpDistMax;
				fairyParticle.interp_duration_min = interpDurMin;
				fairyParticle.interp_duration_max = interpDurMax;
				fairyParticle.interp_trans_ratio = interpTrans;
				fairyParticle.pulse_min = pulseMin;
				fairyParticle.pulse_max = pulseMax;
				fairyParticle.pulse_rate = pulseRate;
				fairyParticle.abs_pulse = absPulse;
				fairyParticle.glowRadius = glowRad;
				fairyParticle.glowIntensity = glowStrength;
				fairyParticle.rotation_rate = rotationRate;
				fairyParticle.ResetNoPositionChange();
			}
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}~{17}~{18}~{19}~{20}~{21}~{22}~{23}~{24}~{25}~{26}~{27}~{28}~{29}~{30}", panelPos.x, panelPos.y, absPulse ? "1" : "0", pulseMin, pulseMax, pulseRate, scaleMin, scaleMax, interpDurMin, interpDurMax, interpDistMin, interpDistMax, dirDevMin, dirDevMax, dirMin, dirMax, colorHmin, colorHmax, colorSmin, colorSmax, colorLmin, colorLmax, interpTrans, alphaTrans, numKeyframes, spriteType, dirLerpType, speedLerpType, glowRad, glowStrength, rotationRate);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			absPulse = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
			pulseMin = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			pulseMax = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			pulseRate = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			scaleMin = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			scaleMax = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
			interpDurMin = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
			interpDurMax = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
			interpDistMin = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
			interpDistMax = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
			dirDevMin = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
			dirDevMax = float.Parse(array[13], NumberStyles.Any, CultureInfo.InvariantCulture);
			dirMin = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
			dirMax = float.Parse(array[15], NumberStyles.Any, CultureInfo.InvariantCulture);
			colorHmin = float.Parse(array[16], NumberStyles.Any, CultureInfo.InvariantCulture);
			colorHmax = float.Parse(array[17], NumberStyles.Any, CultureInfo.InvariantCulture);
			colorSmin = float.Parse(array[18], NumberStyles.Any, CultureInfo.InvariantCulture);
			colorSmax = float.Parse(array[19], NumberStyles.Any, CultureInfo.InvariantCulture);
			colorLmin = float.Parse(array[20], NumberStyles.Any, CultureInfo.InvariantCulture);
			colorLmax = float.Parse(array[21], NumberStyles.Any, CultureInfo.InvariantCulture);
			interpTrans = float.Parse(array[22], NumberStyles.Any, CultureInfo.InvariantCulture);
			alphaTrans = float.Parse(array[23], NumberStyles.Any, CultureInfo.InvariantCulture);
			numKeyframes = float.Parse(array[24], NumberStyles.Any, CultureInfo.InvariantCulture);
			spriteType = new SpriteType(array[25]);
			dirLerpType = new FairyParticle.LerpMethod(array[26]);
			speedLerpType = new FairyParticle.LerpMethod(array[27]);
			glowRad = float.Parse(array[28], NumberStyles.Any, CultureInfo.InvariantCulture);
			glowStrength = float.Parse(array[29], NumberStyles.Any, CultureInfo.InvariantCulture);
			rotationRate = float.Parse(array[30], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 31);
		}
	}

	public class FanData : Data
	{
		public Vector2 panelPos;

		public float speed;

		public float depth;

		public FanData(PlacedObject owner)
			: base(owner)
		{
			panelPos = Custom.DegToVec(30f) * 10f;
			speed = 0.4f;
			depth = 1f;
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}", panelPos.x, panelPos.y, speed, depth);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			speed = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			depth = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
		}
	}

	public class AdjustableFanData : FanData
	{
		public int spriteNum;

		public float scale;

		public AdjustableFanData(PlacedObject owner)
			: base(owner)
		{
			spriteNum = 0;
			scale = 0.5f;
		}

		public new string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", panelPos.x, panelPos.y, spriteNum, speed, scale, depth);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			spriteNum = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			speed = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			scale = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			depth = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
		}
	}

	public class HarmfulSteamData : Data
	{
		public Vector2 panelPos;

		public float duration;

		public float frequency;

		public float lifetime;

		public Vector2 handlePos;

		public HarmfulSteamData(PlacedObject owner)
			: base(owner)
		{
			panelPos = Custom.DegToVec(30f) * 10f;
			duration = 0.2f;
			frequency = 0.5f;
			lifetime = 0.3f;
			handlePos = new Vector2(0f, 100f);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", panelPos.x, panelPos.y, duration, frequency, lifetime, handlePos.x, handlePos.y);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			duration = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			frequency = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			lifetime = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.x = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
			Custom.Log($"Panel Pos: {panelPos.x} , {panelPos.y}");
		}
	}

	public class WindData : QuadObjectData
	{
		public class AffectGroup : ExtEnum<AffectGroup>
		{
			public static AffectGroup Visuals = new AffectGroup("Visuals", register: true);

			public static AffectGroup Objects = new AffectGroup("Objects", register: true);

			public static AffectGroup Creatures = new AffectGroup("Creatures", register: true);

			public AffectGroup(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public float velocity;

		public AffectGroup affectGroup;

		public WindData(PlacedObject owner)
			: base(owner)
		{
			affectGroup = AffectGroup.Visuals;
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			if (array.Length > 6 && float.TryParse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				velocity = result;
			}
			if (array.Length > 7)
			{
				affectGroup = new AffectGroup(array[7]);
			}
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}", velocity, affectGroup.value);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class TerrainHandleData : Data
	{
		public Vector2 leftOffset;

		public Vector2 rightOffset;

		public float backHeight;

		public TerrainHandleData(PlacedObject owner)
			: base(owner)
		{
			leftOffset = new Vector2(-40f, 0f);
			rightOffset = new Vector2(40f, 0f);
			backHeight = 20f;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			leftOffset.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			leftOffset.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			rightOffset.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			rightOffset.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			if (array.Length > 4)
			{
				backHeight = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", leftOffset.x, leftOffset.y, rightOffset.x, rightOffset.y, backHeight);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class LocalTerrainData : SplineObjectData
	{
		public float bottom;

		public LocalTerrainData(PlacedObject owner)
			: base(owner)
		{
			spline = new BezierSpline(Vector2.zero, new Vector2(0f, 100f), new Vector2(100f, 0f), new Vector2(100f, -100f));
			bottom = 100f;
			splineIndexInSaveString = 1;
		}

		public override void FromString(string s)
		{
			string[] array = s.Split('~');
			bottom = float.Parse(array[0], CultureInfo.InvariantCulture);
			base.FromString(s);
		}

		public override string ToString()
		{
			return string.Join("~", bottom.ToString(), base.ToString());
		}
	}

	public class SuperSlopeData : ResizableObjectData
	{
		public float bottom;

		public SuperSlopeData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(100f, 100f);
			bottom = 20f;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			if (array.Length > 2)
			{
				bottom = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 3);
		}

		public override string ToString()
		{
			string baseString = string.Join("~", handlePos.x, handlePos.y, bottom);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class SplineObjectData : Data
	{
		public BezierSpline spline;

		public int splineIndexInSaveString;

		public bool handleUnrecognized = true;

		public SplineObjectData(PlacedObject owner)
			: base(owner)
		{
			spline = new BezierSpline(Vector2.zero, new Vector2(0f, 100f), new Vector2(100f, 100f), new Vector2(100f, 0f));
		}

		public override void FromString(string s)
		{
			string[] array = s.Split('~');
			int num = splineIndexInSaveString;
			List<BezierSpline.Midpoint> list = new List<BezierSpline.Midpoint>();
			string[] array2 = array[num + 3].Split('|');
			foreach (string text in array2)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					list.Add(BezierSpline.Midpoint.FromString(text));
				}
			}
			spline = new BezierSpline(Vector2.zero, ParseVector(array[num + 1]), ParseVector(array[num]), ParseVector(array[num + 2]), list.ToArray());
			if (handleUnrecognized)
			{
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, num + 3);
			}
		}

		public override string ToString()
		{
			string text = string.Join("~", SerializeVector(spline.posB), SerializeVector(spline.handleA), SerializeVector(spline.handleB), string.Join("|", spline.midpoints));
			if (!handleUnrecognized)
			{
				return text;
			}
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
		}

		public string SerializeVector(Vector2 v)
		{
			return v.x + "^" + v.y;
		}

		public Vector2 ParseVector(string s)
		{
			string[] array = s.Split('^');
			return new Vector2(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture));
		}
	}

	public class WaterFlowData : Data
	{
		public int width;

		public float flow;

		public bool dynamic;

		public float preDelay;

		public float postDelay;

		public WaterFlowData(PlacedObject owner)
			: base(owner)
		{
			width = 3;
			flow = 0.5f;
			dynamic = true;
			preDelay = 0f;
			postDelay = 0f;
		}

		public override void FromString(string s)
		{
			try
			{
				string[] array = Regex.Split(s, "~");
				width = ((array[0] != string.Empty) ? int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture) : width);
				flow = ((array.Length > 1) ? float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture) : flow);
				dynamic = ((array.Length > 2) ? (array[2] == "D") : dynamic);
				preDelay = ((array.Length > 3) ? float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture) : preDelay);
				postDelay = ((array.Length > 4) ? float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture) : postDelay);
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
			}
			catch (Exception)
			{
			}
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", width, flow, dynamic ? "D" : "S", preDelay, postDelay);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class PomegranateData : ConsumableObjectData
	{
		public Vector2 handlePos;

		public PomegranateData(PlacedObject owner)
			: base(owner)
		{
			panelPos = Custom.DegToVec(120f) * 20f;
			handlePos = new Vector2(60f, 0f);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			if (array.Length > 4)
			{
				handlePos.x = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
				handlePos.y = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
			}
			else
			{
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
			}
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}", handlePos.x, handlePos.y);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class SkyWhalePathfindingData : Data
	{
		public Vector2 handlePos;

		public Vector2 panelPos;

		public int index;

		public SkyWhalePathfindingData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(0f, 100f);
			panelPos = new Vector2(20f, 100f);
		}

		public Vector2 ClosestPoint(Vector2 pos)
		{
			return Custom.ClosestPointOnLineSegment(owner.pos, owner.pos + handlePos, pos);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}", handlePos.x, handlePos.y, index);
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			if (array.Length > 2)
			{
				index = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
			else
			{
				index = 0;
			}
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 2);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class SpawnMigrationStreamData : SplineObjectData
	{
		public float width = 70f;

		public int maxCapacity = 100;

		public int rate = 120;

		public VoidSpawn.SpawnType type = VoidSpawn.SpawnType.RippleSpawn;

		public SpawnMigrationStreamData(PlacedObject owner)
			: base(owner)
		{
			handleUnrecognized = false;
		}

		public override void FromString(string s)
		{
			string[] array = s.Split('~');
			base.FromString(s);
			width = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			maxCapacity = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			rate = int.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
			type = new VoidSpawn.SpawnType(array[7]);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
		}

		public override string ToString()
		{
			string baseString = string.Join("~", base.ToString(), width, maxCapacity, rate, type.value);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class HideVoidSpawnData : ResizableObjectData
	{
		public Vector2 panelPos = Vector2.zero;

		public VoidSpawn.SpawnType type = VoidSpawn.SpawnType.VoidSpawn;

		public HideVoidSpawnData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			type = new VoidSpawn.SpawnType(array[2]);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 2);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + "~" + type.value;
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class RippleTreeData : RippleStalkData
	{
		public float sproutThreshold;

		public float sproutEnd = 0.5f;

		public RippleTreeData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			sproutThreshold = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			sproutEnd = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			faceLeft = array[4] == "left";
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
			LoadCosmeticValues();
		}

		public override string ToString()
		{
			string text = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}", BaseSaveString(), sproutThreshold, sproutEnd, faceLeft ? "left" : "right");
			if (SaveCosmetics(out var result))
			{
				text = text + "~" + string.Join("~", result);
			}
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
		}
	}

	public class RippleStalkData : ResizableObjectData
	{
		public float? testRippleAmount;

		public bool faceLeft;

		public bool update;

		public float? spiralCoils;

		public float? spiralAmount;

		public float? droopy;

		public float? depth;

		public float? sinWidth;

		public float? sinDist;

		public float? sinOffset;

		public RippleStalkData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			faceLeft = array[2] == "left";
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 3);
			LoadCosmeticValues();
		}

		public override string ToString()
		{
			string text = string.Format(CultureInfo.InvariantCulture, "{0}~{1}", BaseSaveString(), faceLeft ? "left" : "right");
			if (SaveCosmetics(out var result))
			{
				text = text + "~" + string.Join("~", result);
			}
			text = SaveState.SetCustomData(this, text);
			return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
		}

		public void LoadCosmeticValues()
		{
			if (unrecognizedAttributes == null)
			{
				return;
			}
			List<string> list = new List<string>();
			string[] array = unrecognizedAttributes;
			foreach (string text in array)
			{
				bool flag = false;
				if (text.Contains("|"))
				{
					string[] array2 = text.Split('|');
					if (array2.Length >= 2 && float.TryParse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
					{
						switch (array2[0])
						{
						case "spiralCoils":
							spiralCoils = result;
							break;
						case "spiralAmount":
							spiralAmount = result;
							break;
						case "droopy":
							droopy = result;
							break;
						case "depth":
							depth = result;
							break;
						case "sinWidth":
							sinWidth = result;
							break;
						case "sinDist":
							sinDist = result;
							break;
						case "sinOffset":
							sinOffset = result;
							break;
						default:
							list.Add(text);
							continue;
						}
						flag = true;
					}
				}
				if (!flag)
				{
					list.Add(text);
				}
			}
			unrecognizedAttributes = list.ToArray();
		}

		public bool SaveCosmetics(out List<string> result)
		{
			result = new List<string>();
			if (spiralCoils.HasValue)
			{
				List<string> obj = result;
				float? num = spiralCoils;
				obj.Add("spiralCoils|" + num);
			}
			if (spiralAmount.HasValue)
			{
				List<string> obj2 = result;
				float? num = spiralAmount;
				obj2.Add("spiralAmount|" + num);
			}
			if (droopy.HasValue)
			{
				List<string> obj3 = result;
				float? num = droopy;
				obj3.Add("droopy|" + num);
			}
			if (depth.HasValue)
			{
				List<string> obj4 = result;
				float? num = depth;
				obj4.Add("depth|" + num);
			}
			if (sinWidth.HasValue)
			{
				List<string> obj5 = result;
				float? num = sinWidth;
				obj5.Add("sinWidth|" + num);
			}
			if (sinDist.HasValue)
			{
				List<string> obj6 = result;
				float? num = sinDist;
				obj6.Add("sinDist|" + num);
			}
			if (sinOffset.HasValue)
			{
				List<string> obj7 = result;
				float? num = sinOffset;
				obj7.Add("sinOffset|" + num);
			}
			return result.Count > 0;
		}
	}

	public Vector2 pos;

	public bool active = true;

	public bool deactivatedByWarpFilter;

	public bool save = true;

	public string[] unrecognizedAttributes;

	public Type type;

	public Data data;

	public bool deactivattable
	{
		get
		{
			if (type != Type.CustomDecal && type != Type.Filter && type != Type.RippleLevelFilter && type != Type.PrinceFilter && type != Type.RippleEggFilter && type != Type.RippleEggDestination && type != Type.WarpFilter && type != Type.SuperSlope && type != Type.CurvedSlope && type != Type.LocalTerrain && type != Type.CompetitiveFilter)
			{
				return type != Type.TerrainHandle;
			}
			return false;
		}
	}

	public PlacedObject(Type type, Data data)
	{
		this.type = type;
		this.data = data;
		if (data != null)
		{
			data.owner = this;
		}
		if (data == null)
		{
			GenerateEmptyData();
		}
	}

	public override string ToString()
	{
		string baseString = string.Format(CultureInfo.InvariantCulture, "{0}><{1}><{2}><{3}", type.ToString(), pos.x, pos.y, (data != null) ? data.ToString() : "");
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "><", unrecognizedAttributes);
	}

	public void GenerateEmptyData()
	{
		if (type == Type.LightSource)
		{
			data = new LightSourceData(this);
		}
		else if (type == Type.LightFixture)
		{
			data = new LightFixtureData(this, LightFixtureData.Type.RedLight);
		}
		else if (type == Type.CoralCircuit || type == Type.CoralNeuron || type == Type.CoralStem || type == Type.CoralStemWithNeurons || type == Type.Corruption || type == Type.CorruptionTube || type == Type.CorruptionDarkness || type == Type.StuckDaddy || type == Type.WallMycelia || type == Type.ProjectedStars || type == Type.CentipedeAttractor || type == Type.DandelionPatch || type == Type.NoSpearStickZone || type == Type.LanternOnStick || type == Type.TradeOutpost || type == Type.ScavengerTreasury || type == Type.ScavTradeInstruction || type == Type.CosmeticSlimeMold || type == Type.CosmeticSlimeMold2 || type == Type.PlayerPushback || type == Type.DeadTokenStalk || type == Type.NoLeviathanStrandingZone || type == Type.Vine || type == Type.NeuronSpawner || type == Type.TerrainRubble || type == Type.TerrainSunOffset || type == Type.PlateTree || type == Type.NoCurrentZone || type == Type.RotPlateTree || type == Type.BrainMold || type == Type.LocustCloud || type == Type.WindDirection || type == Type.WarpFilter || (ModManager.Watcher && (type == WatcherEnums.PlacedObjectType.SKLightningRod || type == WatcherEnums.PlacedObjectType.PrinceBulb)) || (ModManager.MSC && type == MoreSlugcatsEnums.PlacedObjectType.KarmaShrine) || (ModManager.DLCShared && (type == DLCSharedEnums.PlacedObjectType.MSArteryPush || type == DLCSharedEnums.PlacedObjectType.BigJellyFish || type == DLCSharedEnums.PlacedObjectType.RotFlyPaper || type == DLCSharedEnums.PlacedObjectType.Stowaway)))
		{
			data = new ResizableObjectData(this);
		}
		else if (type == Type.ZapCoil || type == Type.SuperStructureFuses || type == Type.ARKillRect || (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.ARZapperOmni) || (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.ARZapperVertical) || (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.ARZapperHorizontal))
		{
			data = new GridRectObjectData(this);
		}
		else if (type == Type.SpotLight || type == Type.SuperJumpInstruction || type == Type.BlackSpot)
		{
			data = new QuadObjectData(this);
		}
		else if (type == Type.DeepProcessing)
		{
			data = new DeepProcessingData(this);
		}
		else if (type == Type.SSLightRod)
		{
			data = new SSLightRodData(this);
		}
		else if (type == Type.ScavengerOutpost)
		{
			data = new ScavengerOutpostData(this);
		}
		else if (type == Type.SeedCob || type == Type.DangleFruit || type == Type.FlareBomb || type == Type.PuffBall || type == Type.GraffitiBomb || type == Type.WaterNut || type == Type.JellyFish || type == Type.KarmaFlower || type == Type.Mushroom || type == Type.FirecrackerPlant || type == Type.VultureGrub || type == Type.DeadVultureGrub || type == Type.Lantern || type == Type.SlimeMold || type == Type.FlyLure || type == Type.SporePlant || type == Type.NeedleEgg || type == Type.BubbleGrass || type == Type.Hazer || type == Type.DeadHazer || (ModManager.DLCShared && (type == DLCSharedEnums.PlacedObjectType.GooieDuck || type == DLCSharedEnums.PlacedObjectType.LillyPuck || type == DLCSharedEnums.PlacedObjectType.GlowWeed || type == DLCSharedEnums.PlacedObjectType.DandelionPeach)) || (ModManager.MSC && (type == MoreSlugcatsEnums.PlacedObjectType.Germinator || type == MoreSlugcatsEnums.PlacedObjectType.MoonCloak || type == MoreSlugcatsEnums.PlacedObjectType.HRGuard)) || (ModManager.Watcher && (type == WatcherEnums.PlacedObjectType.RottenDangleFruit || type == WatcherEnums.PlacedObjectType.RottenSeedCob)))
		{
			data = new ConsumableObjectData(this);
		}
		else if (type == Type.DataPearl || type == Type.UniqueDataPearl)
		{
			data = new DataPearlData(this);
		}
		else if (type == Type.VoidSpawnEgg)
		{
			data = new VoidSpawnEggData(this);
		}
		else if (type == Type.CustomDecal)
		{
			data = new CustomDecalData(this);
		}
		else if (type == Type.InsectGroup)
		{
			data = new InsectGroupData(this);
		}
		else if (type == Type.MultiplayerItem)
		{
			data = new MultiplayerItemData(this);
		}
		else if (type == Type.GoldToken)
		{
			data = new CollectToken.CollectTokenData(this, isBlue: false);
		}
		else if (type == Type.BlueToken)
		{
			data = new CollectToken.CollectTokenData(this, isBlue: true);
		}
		else if (type == Type.Filter)
		{
			data = new FilterData(this);
		}
		else if (type == Type.CompetitiveFilter)
		{
			data = new CompetitiveFilterData(this);
		}
		else if (type == Type.RippleLevelFilter)
		{
			data = new RippleLevelFilterData(this);
		}
		else if (type == Type.PrinceFilter)
		{
			data = new PrinceFilterData(this);
		}
		else if (type == Type.RippleEggFilter)
		{
			data = new RippleEggFilterData(this);
		}
		else if (type == Type.RippleEggDestination)
		{
			data = new RippleEggDestinationData(this);
		}
		else if (type == Type.ReliableIggyDirection)
		{
			data = new ReliableIggyDirection.ReliableIggyDirectionData(this);
		}
		else if (type == Type.Rainbow)
		{
			data = new Rainbow.RainbowData(this);
		}
		else if (type == Type.RainbowNoFade)
		{
			data = new RainbowNoFade.RainbowNoFadeData(this);
		}
		else if (type == Type.LightBeam)
		{
			data = new LightBeam.LightBeamData(this);
		}
		else if (type == Type.FairyParticleSettings)
		{
			data = new FairyParticleData(this);
		}
		else if (type == Type.DayNightSettings)
		{
			data = new DayNightData(this);
		}
		else if (type == Type.SnowSource)
		{
			data = new SnowSourceData(this);
		}
		else if (type == Type.LightningMachine)
		{
			data = new LightningMachineData(this);
		}
		else if (type == Type.EnergySwirl)
		{
			data = new EnergySwirlData(this);
		}
		else if (type == Type.SteamPipe || type == Type.WallSteamer)
		{
			data = new SteamPipeData(this);
		}
		else if (ModManager.DLCShared && type == DLCSharedEnums.PlacedObjectType.OEsphere)
		{
			data = new OEsphereData(this);
		}
		else if (type == Type.CellDistortion)
		{
			data = new CellDistortionData(this);
		}
		else if (type == Type.LocalBlizzard)
		{
			data = new LocalBlizzardData(this);
		}
		else if (type == Type.AdjustableFan)
		{
			data = new AdjustableFanData(this);
		}
		else if (type == Type.HugeTurbine)
		{
			data = new FanData(this);
		}
		else if (type == Type.HarmfulSteam)
		{
			data = new HarmfulSteamData(this);
		}
		else if (type == Type.WarpPoint)
		{
			data = new WarpPoint.WarpPointData(this);
		}
		else if (type == Type.SkyWhalePathfinding)
		{
			data = new SkyWhalePathfindingData(this);
		}
		else if (type == Type.WindRect)
		{
			data = new WindData(this);
		}
		else if (type == Type.CosmeticRipple)
		{
			data = new CosmeticRippleData(this);
		}
		else if (type == Type.TerrainHandle)
		{
			data = new TerrainHandleData(this);
		}
		else if (type == Type.LocalTerrain || type == Type.CurvedSlope)
		{
			data = new LocalTerrainData(this);
		}
		else if (type == Type.SuperSlope)
		{
			data = new SuperSlopeData(this);
		}
		else if (type == Type.Pomegranate)
		{
			data = new PomegranateData(this);
		}
		else if (type == Type.PomegranateVine)
		{
			data = new ResizableObjectData(this);
		}
		else if (type == Type.FluxWaterfall || type == Type.FluxDrain)
		{
			data = new WaterFlowData(this);
		}
		else if (type == Type.Geyser)
		{
			data = new GeyserData(this);
		}
		else if (type == Type.WaterCurrent)
		{
			data = new WaterCurrent.WaterCurrentData(this);
		}
		else if (type == Type.WaterCutoff)
		{
			data = new WaterCutoffData(this);
		}
		else if (type == Type.AirPocket)
		{
			data = new AirPocketData(this);
		}
		else if (type == Type.MudPit)
		{
			data = new MudPit.MudPitData(this);
		}
		else if (type == Type.TerrainGrassPatch)
		{
			data = new GrassBlade.TerrainGrassPatchData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.SpinningTopSpot)
		{
			data = new SpinningTopData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.WeaverSpot)
		{
			data = new ResizableObjectData(this)
			{
				handlePos = new Vector2(0f, -100f)
			};
		}
		else if (type == Type.DynamicWarpTarget)
		{
			data = new DynamicWarpTargetData(this);
		}
		else if (type == Type.FloatingDebris)
		{
			data = new FloatingDebrisData(this);
		}
		else if (type == Type.SandGrubNetwork)
		{
			data = new SandGrubNetwork.NetworkData(this);
		}
		else if (type == Type.GooDrips)
		{
			data = new GooDripSource.GooDripsData(this);
		}
		else if (type == Type.LobeTree)
		{
			data = new LobeTree.LobeTreeData(this);
		}
		else if (type == Type.PassiveCorruption)
		{
			data = new DaddyCorruption.CustomRotData(this);
		}
		else if (type == Type.FlameJet)
		{
			data = new FlameJet.FlameJetData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.AetherRainbow)
		{
			data = new AetherRainbow.AetherRainbowData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.WallLight)
		{
			data = new WallLight.WallLightData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.UrbanLife)
		{
			data = new UrbanLife.UrbanLifeData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.UrbanLifePath)
		{
			data = new UrbanLifePath.UrbanLifePathData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.SpinToy)
		{
			data = new ResizableObjectData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.BallToy)
		{
			data = new ResizableObjectData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.SoftToy)
		{
			data = new ResizableObjectData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.WeirdToy)
		{
			data = new ResizableObjectData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.PearlReader)
		{
			data = new ResizableObjectData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.UrbanCandles)
		{
			data = new UrbanCandlePlacer.UrbanCandlePlacerData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.UrbanCandleHolder)
		{
			data = new UrbanCandleHolder.UrbanCandleHolderData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.KarmaFlowerPatch)
		{
			data = new KarmaFlowerPatch.KarmaFlowerPatchData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.DaemonClaw)
		{
			data = new SteamPipeData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.DaemonEye)
		{
			data = new DaemonEyeData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.DaemonCrown)
		{
			data = new DaemonCrownData(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.BigSkyWhaleSpawner)
		{
			data = new BigSkyWhaleSpawner.Data(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.BigSkyWhaleTrigger)
		{
			data = new BigSkyWhaleTrigger.Data(this);
		}
		else if (ModManager.Watcher && type == WatcherEnums.PlacedObjectType.TowerCrabSpawner)
		{
			data = new TowerCrabSpawner.Data(this);
		}
		else if (ModManager.NewSlugcatsModule && type == Type.GreenToken)
		{
			data = new CollectToken.CollectTokenData(this, isBlue: false);
			(data as CollectToken.CollectTokenData).isGreen = true;
		}
		else if (ModManager.MSC && type == MoreSlugcatsEnums.PlacedObjectType.WhiteToken)
		{
			data = new CollectToken.CollectTokenData(this, isBlue: false);
			(data as CollectToken.CollectTokenData).isWhite = true;
		}
		else if (ModManager.MSC && type == MoreSlugcatsEnums.PlacedObjectType.RedToken)
		{
			data = new CollectToken.CollectTokenData(this, isBlue: false);
			(data as CollectToken.CollectTokenData).isRed = true;
		}
		else if (ModManager.MSC && type == MoreSlugcatsEnums.PlacedObjectType.DevToken)
		{
			data = new CollectToken.CollectTokenData(this, isBlue: false);
			(data as CollectToken.CollectTokenData).isDev = true;
		}
		else if (type == Type.SpawnMigrationStream)
		{
			data = new SpawnMigrationStreamData(this);
		}
		else if (type == Type.HideVoidSpawn)
		{
			data = new HideVoidSpawnData(this);
		}
		else if (type == Type.RippleSpawnEgg)
		{
			data = new RippleSpawnEggData(this);
		}
		else if (type == Type.RippleTree)
		{
			data = new RippleTreeData(this);
		}
		else if (type == Type.RippleStalk)
		{
			data = new RippleStalkData(this);
		}
		else if (type == Type.NoRippleSproutZone)
		{
			data = new QuadObjectData(this);
		}
	}

	public void FromString(string[] s)
	{
		type = new Type(s[0]);
		pos.x = float.Parse(s[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		pos.y = float.Parse(s[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		GenerateEmptyData();
		if (data != null)
		{
			data.FromString(s[3]);
		}
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(s, 4);
	}

	public void Refresh()
	{
		data.RefreshLiveVisuals();
	}
}
