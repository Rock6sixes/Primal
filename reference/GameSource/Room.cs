using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using CoralBrain;
using Expedition;
using MoreSlugcats;
using Noise;
using RWCustom;
using ScavTradeInstruction;
using Unity.Mathematics;
using UnityEngine;
using VoidSea;
using Watcher;

public class Room
{
	public class SlopeDirection : ExtEnum<SlopeDirection>
	{
		public static SlopeDirection UpLeft = new SlopeDirection("UpLeft", register: true);

		public static SlopeDirection UpRight = new SlopeDirection("UpRight", register: true);

		public static SlopeDirection DownLeft = new SlopeDirection("DownLeft", register: true);

		public static SlopeDirection DownRight = new SlopeDirection("DownRight", register: true);

		public static SlopeDirection Broken = new SlopeDirection("Broken", register: true);

		public SlopeDirection(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class RippleSpawnSource : ExtEnum<RippleSpawnSource>
	{
		public static RippleSpawnSource PlayerTrail = new RippleSpawnSource("PlayerTrail", register: true);

		public static RippleSpawnSource Ripple = new RippleSpawnSource("Ripple", register: true);

		public static RippleSpawnSource WarpPoint = new RippleSpawnSource("WarpPoint", register: true);

		public static RippleSpawnSource Dimension = new RippleSpawnSource("Dimension", register: true);

		public RippleSpawnSource(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Tile
	{
		public enum TerrainType
		{
			Air,
			Solid,
			Slope,
			Floor,
			ShortcutEntrance
		}

		public TerrainType Terrain;

		public bool verticalBeam;

		public bool horizontalBeam;

		public bool wallbehind;

		public int shortCut;

		public bool wormGrass;

		public int waterInt;

		public int X;

		public int Y;

		public bool hive;

		public bool AnyBeam
		{
			get
			{
				if (!verticalBeam)
				{
					return horizontalBeam;
				}
				return true;
			}
		}

		public bool DeepWater => waterInt == 1;

		public bool WaterSurface => waterInt == 2;

		public bool AnyWater => waterInt != 0;

		public bool Solid => IsSolid();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSolid()
		{
			return Terrain == TerrainType.Solid;
		}

		public Tile(int x, int y, TerrainType tType, bool vBeam, bool hBeam, bool wbhnd, int sc, int wtr)
		{
			X = x;
			Y = y;
			verticalBeam = vBeam;
			horizontalBeam = hBeam;
			Terrain = tType;
			shortCut = sc;
			wallbehind = wbhnd;
			waterInt = wtr;
		}
	}

	public class WaterFluxController
	{
		public float roomShake;

		public Room owner;

		public GlobalRain globalRain;

		public float internalFluxWaterState;

		public float overShooter;

		public float fluxWaterLevel
		{
			get
			{
				float num = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMaxLevel);
				float num2 = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMinLevel);
				if (num2 > num)
				{
					float num3 = num2;
					num2 = num;
					num = num3;
				}
				num2 += Mathf.Min(0f, overShooter);
				num += Mathf.Max(0f, overShooter);
				float num4 = num2 * (float)owner.TileHeight * 22f;
				float num5 = num * (float)owner.TileHeight * 22f;
				return Mathf.Lerp(num4 - 50f, num5 - 50f, waterFluxState());
			}
		}

		public WaterFluxController(Room room, GlobalRain globalRain)
		{
			owner = room;
			this.globalRain = globalRain;
			internalFluxWaterState = float.MinValue;
		}

		public float waterFluxState()
		{
			owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxFrequency);
			float effectAmount = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMaxDelay);
			float effectAmount2 = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMinDelay);
			float t = Mathf.Lerp(effectAmount2, effectAmount, 0.5f);
			float num = 100f + Mathf.Lerp(280f, 490f, t);
			float num2 = Mathf.Max(0f, effectAmount * 1200f);
			float num3 = Mathf.Max(0f, effectAmount2 * 1200f);
			float num4 = num + num + num2 + num3;
			float num5 = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxOffset);
			if (num5 != 0f)
			{
				num5 = ((!(num5 < 0.5f)) ? (5000f * Mathf.InverseLerp(0.5f, 1f, num5)) : (-5000f * Mathf.InverseLerp(0.5f, 0f, num5)));
			}
			float num6 = owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxSpeed);
			if (num6 <= 0f)
			{
				num6 = 1f;
			}
			float num7 = ((float)globalRain.waterFluxTicker + num5) * num6 % num4;
			float b;
			if (num7 < num)
			{
				float t2 = Mathf.InverseLerp(0f, num, num7);
				b = Mathf.Lerp(0f, 1f, t2);
				overShooter += num / 100000000f;
			}
			else if (num7 < num + num2)
			{
				b = 1f;
				overShooter *= 0.995f;
			}
			else if (num7 < num + num2 + num)
			{
				float t3 = Mathf.InverseLerp(num + num2, num + num2 + num, num7);
				b = Mathf.Lerp(1f, 0f, t3);
				overShooter -= num / 100000000f;
			}
			else
			{
				b = 0f;
				overShooter *= 0.995f;
			}
			if (internalFluxWaterState == float.MinValue)
			{
				internalFluxWaterState = b;
				roomShake = 0f;
			}
			else
			{
				float num8 = internalFluxWaterState;
				internalFluxWaterState = Mathf.Lerp(internalFluxWaterState, b, 0.06f);
				roomShake = Mathf.Abs(internalFluxWaterState - num8) * 100f * owner.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxRumble);
			}
			return internalFluxWaterState;
		}
	}

	public static AGLog<Room> Log = new AGLog<Room>();

	public List<IAccessibilityModifier> accessModifiers;

	public int Width;

	public int Height;

	public float[,] shortcutsBlinking;

	public IntVector2[][] hives;

	public IntVector2[] garbageHoles;

	public IntVector2[] spawnInRoomItems;

	public WaterFall[] waterFalls;

	public RoomBorderExit[] borderExits;

	public List<LightSource> lightSources;

	public List<VisionObscurer> visionObscurers;

	public List<PlacedObject> noCurrentZones = new List<PlacedObject>();

	public bool gameOverRoom;

	public DeathFallGraphic deathFallGraphic;

	public Lightning lightning;

	public LightningMaker lightningMaker;

	public LethalThunderStorm lethalThunderStorm;

	public RippleCreatureTracker rippleCreatureTracker;

	public bool watcherCamoTaken;

	public float lastBackgroundNoise;

	public float backgroundNoise;

	public List<ChunkGlue> chunkGlue;

	public RoomSettings roomSettings;

	public int updateIndex = int.MaxValue;

	public Tile[,] Tiles;

	public Tile DefaultTile;

	public List<PhysicalObject>[] physicalObjects;

	public RainWorldGame game;

	public World world;

	public AImap aimap;

	public SocialEventRecognizer socialEventRecognizer;

	public QuickPathFinder quickPather;

	public IntVector2[] exitAndDenIndex;

	public IntVector2[] shortcutsIndex;

	public ShortcutData[] shortcuts;

	public List<IntVector2> lockedShortcuts;

	public Vector2[] cameraPositions;

	public int camerasChangedTick;

	public Vector2 lightAngle;

	public int loadingProgress;

	public bool readyForNonAICreaturesToEnter;

	public int waitToEnterAfterFullyLoaded = -1;

	public ClimbableVinesSystem climbableVines;

	public InsectCoordinator insectCoordinator;

	public TerrainManager terrain;

	public Grass grass;

	public LocustSystem locusts;

	public FliesRoomAI fliesRoomAi;

	public IntVector2[] ceilingTiles;

	public float gravity = 1f;

	public DebugPerTileVisualizer PERTILEVISALIZER;

	public float waterGlitterCycle;

	public float lastWaterGlitterCycle;

	public Water waterObject;

	public bool water;

	public int defaultWaterLevel;

	public float floatWaterLevel;

	public bool waterInFrontOfTerrain;

	public AIdataPreprocessor aidataprepro;

	public RegionGate regionGate;

	public ShelterDoor shelterDoor;

	public BrainMold brainMold;

	public bool spawnedSpinningTop;

	public bool spawnedWeaver;

	public bool weaverPresenceInitialized;

	public List<UpdatableAndDeletable> weaverPresenceObjects = new List<UpdatableAndDeletable>();

	public bool rotPresenceInitialized;

	public bool didFirstSentientRotUpdate;

	public int ticker;

	public RoomRain roomRain;

	public Sandstorm sandstorm;

	public WindDirection windDirObject;

	public long timeAdder;

	public int timeDivider;

	public int cntr;

	public List<LightningMachine> lightningMachines;

	public List<EnergySwirl> energySwirls;

	public List<SnowSource> snowSources;

	public List<LocalBlizzard> localBlizzards;

	public List<Vector2> deathFallFocalPoints;

	public List<CellDistortion> cellDistortions;

	public Snow snowObject;

	public bool snow;

	public BlizzardGraphics blizzardGraphics;

	public bool blizzard;

	public WaterFluxController waterFlux;

	public List<IProvideWarmth> blizzardHeatSources;

	public List<LightSource> cosmeticLightSources;

	public List<ZapCoil> zapCoils;

	public List<OEsphere> oeSpheres;

	public float darkenLightsFactor;

	public int syncTicker;

	public bool dustStorm;

	public bool waterInverted;

	public bool deferred;

	public int SwarmerCount;

	public Vector2 cloudsNdarken;

	public Oracle.OracleID oracleWantToSpawn;

	public bool ripple;

	public RippleFullScreen fsRipple;

	public List<CosmeticRipple> cosmeticRipples;

	public List<WarpPoint> warpPoints;

	public int urbanLifeCount;

	public bool urbanLifeBlurSpawned;

	public List<VoidSpawn> voidSpawns;

	public int RippleSpawnCount;

	public List<RippleJelly> rippleJellies;

	public bool generateLightmap;

	public List<int> list { get; set; }

	public AbstractRoom abstractRoom { get; set; }

	public int TileWidth => Width;

	public int TileHeight => Height;

	public float PixelWidth => (float)Width * 20f;

	public float PixelHeight => (float)Height * 20f;

	public List<Player> PlayersInRoom => (from x in game.NonPermaDeadPlayers
		where x.Room == abstractRoom
		select x.realizedCreature as Player).ToList();

	public bool BeingViewed
	{
		get
		{
			for (int i = 0; i < game.cameras.Length; i++)
			{
				if (game.cameras[i].room == this)
				{
					return true;
				}
			}
			return false;
		}
	}

	public float ElectricPower
	{
		get
		{
			if (roomSettings == null || world == null || world.rainCycle == null)
			{
				return 1f;
			}
			if (roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f && (!ModManager.MMF || world.rainCycle.brokenAntiGrav != null))
			{
				return world.rainCycle.brokenAntiGrav.CurrentLightsOn;
			}
			if (ModManager.MSC && world.rainCycle.filtrationPowerBehavior != null)
			{
				return world.rainCycle.filtrationPowerBehavior.ElectricPower;
			}
			return 1f;
		}
	}

	public float RainDrownOutSounds
	{
		get
		{
			if (roomSettings.DangerType == RoomRain.DangerType.None || (ModManager.DLCShared && roomSettings.DangerType == DLCSharedEnums.RoomRainDangerType.Blizzard))
			{
				return 0f;
			}
			if (roomRain != null)
			{
				if (ModManager.PrecycleModule && world.rainCycle.preTimer > 0)
				{
					return Mathf.Lerp(0f, 0.1f, roomRain.intensity);
				}
				return Mathf.InverseLerp(0f, 0.6f, roomRain.intensity);
			}
			if (sandstorm != null)
			{
				return sandstorm.DrownOutSounds;
			}
			return 0f;
		}
	}

	public float BackgroundNoise => lastBackgroundNoise;

	public List<UpdatableAndDeletable> updateList { get; set; }

	public List<IDrawable> drawableObjects { get; set; }

	public bool shortCutsReady => loadingProgress >= 1;

	public bool readyForAI => loadingProgress >= 2;

	public bool quantifiedCreaturesPlaced => loadingProgress >= 3;

	public bool fullyLoaded => loadingProgress >= 3;

	public bool ReadyForPlayer
	{
		get
		{
			if (readyForNonAICreaturesToEnter)
			{
				return waitToEnterAfterFullyLoaded < 1;
			}
			return false;
		}
	}

	public Vector2 windDirection { get; set; }

	public FloatRect RoomRect => new FloatRect(0f, 0f, PixelWidth, PixelHeight);

	public bool AnyWarpPointBeingActivated
	{
		get
		{
			for (int i = 0; i < warpPoints.Count; i++)
			{
				if (warpPoints[i].anyPlayersInRange)
				{
					return true;
				}
			}
			return false;
		}
	}

	public float DustStormIntensity
	{
		get
		{
			if (!ModManager.DLCShared || !dustStorm)
			{
				return 0f;
			}
			return world.rainCycle.DustStormProgress * roomSettings.RainIntensity;
		}
	}

	public bool VoidWeaverActive
	{
		get
		{
			if (game.IsStorySession && !game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility)
			{
				return world?.weaverPresence?.weaverRoom == abstractRoom;
			}
			return false;
		}
	}

	public bool HasVoidWeaverEffects => (world?.weaverPresence?.presenceRooms.Contains(abstractRoom)).GetValueOrDefault();

	public void SetAbstractRoom(AbstractRoom newAbstractRoom)
	{
		abstractRoom = newAbstractRoom;
		abstractRoom.realizedRoom = this;
	}

	public IntVector2 RandomTile()
	{
		return new IntVector2(UnityEngine.Random.Range(0, TileWidth), UnityEngine.Random.Range(0, TileHeight));
	}

	public Vector2 RandomPos()
	{
		return new Vector2(UnityEngine.Random.value * PixelWidth, UnityEngine.Random.value * PixelHeight);
	}

	public void MakeBackgroundNoise(float noise)
	{
		backgroundNoise = Mathf.Clamp(noise, backgroundNoise, 1f);
	}

	public float FloatWaterLevel(Vector2 pos)
	{
		if (waterObject != null)
		{
			return waterObject.DetailedWaterLevel(pos);
		}
		return floatWaterLevel;
	}

	public bool PointSubmerged(Vector2 pos)
	{
		if (ModManager.DLCShared && waterInverted)
		{
			if (waterObject != null)
			{
				return pos.y > waterObject.DetailedWaterLevel(pos);
			}
			return pos.y > floatWaterLevel;
		}
		if (waterObject != null)
		{
			return pos.y < waterObject.DetailedWaterLevel(pos);
		}
		return pos.y < floatWaterLevel;
	}

	public Room(RainWorldGame game, World world, AbstractRoom abstractRoom, bool devUI = false)
	{
		this.game = game;
		this.world = world;
		this.abstractRoom = abstractRoom;
		if (ModManager.ChallengeModule && game != null && game.session != null && game.IsArenaSession && game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge && File.Exists(AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + "Challenges" + Path.DirectorySeparatorChar + "Challenge" + game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeID + "_Settings.txt")))
		{
			roomSettings = new RoomSettings("Challenge" + game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeID, world.region, template: false, firstTemplate: false, game?.TimelinePoint, game);
		}
		else if (ModManager.MSC && game != null && game.IsStorySession && game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken && world.region != null && world.region.name == "RM")
		{
			roomSettings = new RoomSettings(this, WorldLoader.RoomNameManipulator(abstractRoom.FileName, game) + "-2", world.region, template: false, firstTemplate: false, game?.TimelinePoint, game);
		}
		else
		{
			roomSettings = new RoomSettings(this, WorldLoader.RoomNameManipulator(abstractRoom.FileName, game), world.region, template: false, firstTemplate: false, game?.TimelinePoint, game);
		}
		if (game != null)
		{
			SlugcatGamemodeUniqueRoomSettings(game);
			if (roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMaxLevel) > 0f)
			{
				waterFlux = new WaterFluxController(this, game.globalRain);
			}
			if (ModManager.DLCShared && roomSettings.GetEffect(DLCSharedEnums.RoomEffectType.InvertedWater) != null)
			{
				waterInverted = true;
			}
		}
		physicalObjects = new List<PhysicalObject>[3];
		for (int i = 0; i < physicalObjects.Length; i++)
		{
			physicalObjects[i] = new List<PhysicalObject>();
		}
		drawableObjects = new List<IDrawable>();
		accessModifiers = new List<IAccessibilityModifier>();
		updateList = new List<UpdatableAndDeletable>();
		lightSources = new List<LightSource>();
		waterFalls = new WaterFall[0];
		visionObscurers = new List<VisionObscurer>();
		socialEventRecognizer = new SocialEventRecognizer(this);
		cellDistortions = new List<CellDistortion>();
		cosmeticLightSources = new List<LightSource>();
		zapCoils = new List<ZapCoil>();
		lightningMachines = new List<LightningMachine>();
		energySwirls = new List<EnergySwirl>();
		snowSources = new List<SnowSource>();
		localBlizzards = new List<LocalBlizzard>();
		oeSpheres = new List<OEsphere>();
		deathFallFocalPoints = new List<Vector2>();
		blizzard = false;
		dustStorm = false;
		blizzardHeatSources = new List<IProvideWarmth>();
		lockedShortcuts = new List<IntVector2>();
		ripple = false;
		cosmeticRipples = new List<CosmeticRipple>();
		warpPoints = new List<WarpPoint>();
		voidSpawns = new List<VoidSpawn>();
		rippleJellies = new List<RippleJelly>();
	}

	public void AddWater()
	{
		if (waterObject == null)
		{
			if (defaultWaterLevel < 0)
			{
				defaultWaterLevel = -10;
				floatWaterLevel = -200f;
			}
			if (ModManager.DLCShared && waterInverted && defaultWaterLevel > 0)
			{
				waterInverted = false;
			}
			waterObject = new Water(this, defaultWaterLevel);
			accessModifiers.Add(waterObject);
			drawableObjects.Add(waterObject);
			for (int i = 0; i < game.cameras.Length; i++)
			{
				if (game.cameras[i].room == this)
				{
					game.cameras[i].NewObjectInRoom(waterObject);
				}
			}
		}
		for (int j = 0; j < waterFalls.Length; j++)
		{
			waterFalls[j].ConnectToWaterObject(waterObject);
		}
		water = true;
	}

	public void Loaded()
	{
		LoadTerrainCurve();
		if (game == null)
		{
			return;
		}
		MapExporter.Room_Loaded(this);
		if (ModManager.MSC)
		{
			List<IntVector2> list = new List<IntVector2>();
			if (game.IsStorySession && game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && !abstractRoom.gate && !abstractRoom.shelter && world.region != null && world.region.name == "LF" && RainWorld.MainThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				UnityEngine.Random.State state = UnityEngine.Random.state;
				UnityEngine.Random.InitState(abstractRoom.index);
				int num = 0;
				for (int i = 0; i < Tiles.GetLength(0); i++)
				{
					for (int j = 0; j < Tiles.GetLength(1) - 2; j++)
					{
						if (!Tiles[i, j].Solid || Tiles[i, j + 1].Solid || Tiles[i, j + 2].Solid)
						{
							continue;
						}
						if (UnityEngine.Random.value < 0.25f || num > 0)
						{
							if (num <= -2 && UnityEngine.Random.value < 0.25f)
							{
								num = UnityEngine.Random.Range(2, 6);
							}
							for (int k = 0; k < UnityEngine.Random.Range(1, 4) && j + k + 1 < Tiles.GetLength(1) && !Tiles[i, j + k + 1].Solid; k++)
							{
								list.Add(new IntVector2(i, j + k + 1));
							}
						}
						num--;
					}
				}
				UnityEngine.Random.state = state;
			}
			if (list.Count > 0)
			{
				AddObject(new WormGrass(this, list));
			}
			if (abstractRoom.firstTimeRealized && abstractRoom.shelter)
			{
				for (int num2 = abstractRoom.entities.Count - 1; num2 >= 0; num2--)
				{
					if (abstractRoom.entities[num2] is AbstractSpear)
					{
						AbstractSpear abstractSpear = abstractRoom.entities[num2] as AbstractSpear;
						if (abstractSpear.needle && !abstractSpear.stuckInWall)
						{
							abstractRoom.entities.RemoveAt(num2);
						}
					}
				}
			}
		}
		if (water)
		{
			AddWater();
		}
		if (ModManager.PrecycleModule && game.globalRain.drainWorldFlood > 0f && (roomSettings.DangerType == RoomRain.DangerType.Flood || roomSettings.DangerType == RoomRain.DangerType.FloodAndRain) && waterObject == null)
		{
			defaultWaterLevel = -2000;
			AddWater();
		}
		bool flag = roomSettings.placedObjects.Any((PlacedObject obj) => obj.type == PlacedObject.Type.WaterCycleBottom || obj.type == PlacedObject.Type.WaterCycleTop);
		if (flag && waterObject == null)
		{
			AddWater();
		}
		if (roomSettings.placedObjects.Any((PlacedObject obj) => obj.type == PlacedObject.Type.LocustCloud && obj.active))
		{
			AddObject(locusts = new LocustSystem(this));
		}
		if (abstractRoom.shelter)
		{
			shelterDoor = new ShelterDoor(this);
			AddObject(shelterDoor);
		}
		else if (IsGateRoom())
		{
			string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "Egates.txt"));
			for (int l = 0; l < array.Length + 1; l++)
			{
				if (l >= array.Length)
				{
					Custom.Log(abstractRoom.name, "is a WATER GATE!");
					regionGate = new WaterGate(this);
					AddObject(regionGate);
					break;
				}
				if (array[l] == abstractRoom.name)
				{
					Custom.Log(abstractRoom.name, "is an ELECTRIC GATE!");
					regionGate = new ElectricGate(this);
					AddObject(regionGate);
					break;
				}
			}
		}
		List<IntVector2> list2 = new List<IntVector2>();
		for (int m = 0; m < TileWidth; m++)
		{
			for (int n = 0; n < TileHeight - 1; n++)
			{
				if (GetTile(m, n).Terrain != Tile.TerrainType.Solid && GetTile(m, n + 1).Terrain == Tile.TerrainType.Solid && GetTile(m, n - 1).Terrain != Tile.TerrainType.Solid && n > defaultWaterLevel)
				{
					list2.Add(new IntVector2(m, n));
				}
			}
		}
		ceilingTiles = list2.ToArray();
		if ((!abstractRoom.shelter || world.brokenShelters[abstractRoom.shelterIndex]) && (roomSettings.DangerType == RoomRain.DangerType.Rain || roomSettings.DangerType == RoomRain.DangerType.FloodAndRain || roomSettings.DangerType == RoomRain.DangerType.Flood || roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard || flag))
		{
			roomRain = new RoomRain(game.globalRain, this);
			AddObject(roomRain);
		}
		if (DeathFallGraphic.RoomRequiresDeathFall(this))
		{
			AddObject(deathFallGraphic = new DeathFallGraphic());
		}
		if (ModManager.Watcher && roomSettings.DangerType == WatcherEnums.WatcherDangerType.Sandstorm)
		{
			sandstorm = new Sandstorm(this);
			AddObject(sandstorm);
		}
		if (world.voidSpawnWorldAI != null)
		{
			foreach (WorldSpawnMigrationStream worldMigrationStream in world.voidSpawnWorldAI.worldMigrationStreams)
			{
				worldMigrationStream.ApplyToRoom(this);
			}
		}
		for (int num3 = 0; num3 < roomSettings.effects.Count; num3++)
		{
			if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.Dustpuffs)
			{
				AddObject(new RoofTopView.DustpuffSpawner());
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.Coldness)
			{
				AddObject(new ColdRoom(this));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.SkyDandelions)
			{
				AddObject(new SkyDandelions(roomSettings.effects[num3], this));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.Lightning || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.BkgOnlyLightning)
			{
				if (lightning == null)
				{
					lightning = new Lightning(this, roomSettings.effects[num3].amount, roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.BkgOnlyLightning);
					AddObject(lightning);
				}
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.GreenSparks)
			{
				AddObject(new GreenSparks(this, roomSettings.effects[num3].amount));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.VoidMelt)
			{
				AddObject(new MeltLights(roomSettings.effects[num3], this));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.SunBlock)
			{
				AddObject(new SunBlocker());
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.ZeroG || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.BrokenZeroG)
			{
				bool flag2 = false;
				for (int num4 = 0; num4 < updateList.Count; num4++)
				{
					if (flag2)
					{
						break;
					}
					if (updateList[num4] is AntiGravity)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					AddObject(new AntiGravity(this));
				}
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.SSSwarmers)
			{
				bool flag3 = true;
				int num5 = updateList.Count - 1;
				while (num5 >= 0 && flag3)
				{
					flag3 = !(updateList[num5] is CoralNeuronSystem);
					num5--;
				}
				if (flag3)
				{
					AddObject(new CoralNeuronSystem());
				}
				waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 40);
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.AboveCloudsView)
			{
				AddObject(new AboveCloudsView(this, roomSettings.effects[num3]));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.PinkSky)
			{
				AddObject(new AboveCloudsView(this, roomSettings.effects[num3]));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.RoofTopView)
			{
				AddObject(new RoofTopView(this, roomSettings.effects[num3]));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.VoidSea)
			{
				AddObject(new VoidSeaScene(this));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.ElectricDeath)
			{
				AddObject(new ElectricDeath(roomSettings.effects[num3], this));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.VoidSpawn)
			{
				if ((game.StoryCharacter != SlugcatStats.Name.Red || (world.region != null && world.region.name == "SB")) && ((game.session is StoryGameSession && (game.session as StoryGameSession).saveState.CanSeeVoidSpawn) || game.setupValues.playerGlowing))
				{
					AddObject(new VoidSpawnKeeper(this, roomSettings.effects[num3]));
				}
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.SSMusic)
			{
				AddObject(new SSMusicTrigger(roomSettings.effects[num3]));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.BorderPushBack)
			{
				AddObject(new RoomBorderPushBack(this));
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.Flies || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.FireFlies || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.TinyDragonFly || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.RockFlea || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.RedSwarmer || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.Ant || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.Beetle || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.WaterGlowworm || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.Wasp || roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.Moth)
			{
				if (insectCoordinator == null)
				{
					insectCoordinator = new InsectCoordinator(this);
					AddObject(insectCoordinator);
				}
				insectCoordinator.AddEffect(roomSettings.effects[num3]);
			}
			else if (ModManager.PrecycleModule && roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.FastFloodDrain && abstractRoom.firstTimeRealized)
			{
				world.game.globalRain.drainWorldFastDrainCounter += (int)(roomSettings.effects[num3].amount * 4800f);
			}
			else if (ModManager.PrecycleModule && roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.FastFloodPullDown)
			{
				float num6 = Mathf.InverseLerp(0.5f, 1f, roomSettings.effects[num3].amount);
				float num7 = Mathf.InverseLerp(0.49999f, 0f, roomSettings.effects[num3].amount);
				int num8 = (int)(num6 * 1000f) + (int)(num7 * -1000f);
				if (game.globalRain.drainWorldFlood > 0f && game.globalRain.DrainWorldPositionFlooded(new WorldCoordinate(abstractRoom.index, 0, num8 + 1, -1)))
				{
					Custom.Log("Pushed drainworld flood down by", num8.ToString());
					game.globalRain.DrainWorldFloodInit(new WorldCoordinate(abstractRoom.index, 0, num8, -1));
				}
			}
			else if (ModManager.DLCShared && roomSettings.effects[num3].type == DLCSharedEnums.RoomEffectType.DustWave)
			{
				AddObject(new DustWave(game.cameras[0], roomSettings.effects[num3].amount));
				dustStorm = true;
			}
			else if (ModManager.Watcher && roomSettings.effects[num3].type == WatcherEnums.RoomEffectType.OuterRimView)
			{
				AddObject(new OuterRimView(this, roomSettings.effects[num3]));
			}
			else if (ModManager.Watcher && roomSettings.effects[num3].type == WatcherEnums.RoomEffectType.SentientRotParticles)
			{
				if (roomSettings.effects[num3].amount > 0f)
				{
					AddObject(new SentientRotSpores(this));
				}
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.SandPuffs)
			{
				AddObject(new SandPuffSpawner());
			}
			else if (roomSettings.effects[num3].type == RoomSettings.RoomEffect.Type.LocustSwarmConfig)
			{
				AddObject(new LocustSystem.SoundPlayer());
				AddObject(new LocustShadows());
			}
			else if (ModManager.Watcher && roomSettings.effects[num3].type == WatcherEnums.RoomEffectType.AncientUrbanView)
			{
				AddObject(new AncientUrbanView(this, roomSettings.effects[num3]));
			}
			else if (roomSettings.effects[num3].type == WatcherEnums.RoomEffectType.SaltFlakes)
			{
				AddObject(new SaltFlakes(roomSettings.effects[num3], this));
			}
			else if (ModManager.Watcher && roomSettings.effects[num3].type == WatcherEnums.RoomEffectType.InnerOuterRimView)
			{
				AddObject(new InnerOuterRimView(this, roomSettings.effects[num3]));
			}
			else if (ModManager.Watcher && roomSettings.effects[num3].type == WatcherEnums.RoomEffectType.SurfaceSandstorm)
			{
				if (sandstorm == null)
				{
					sandstorm = new Sandstorm(this);
					AddObject(sandstorm);
				}
			}
			else if (ModManager.Watcher && roomSettings.effects[num3].type == WatcherEnums.RoomEffectType.SKLightning && lethalThunderStorm == null)
			{
				AddObject(lethalThunderStorm = new LethalThunderStorm(roomSettings.effects[num3], this));
			}
		}
		if (ModManager.Watcher && world.name.ToLowerInvariant() == "wssr")
		{
			AddObject(new RotWormScene(this));
		}
		int num9 = 0;
		for (int num10 = 1; num10 <= 2; num10++)
		{
			if (num10 == 2 && warpPoints.Count > 0)
			{
				continue;
			}
			if (num10 == 1)
			{
				LoadWarpsFromSaveState();
			}
			for (int num11 = 0; num11 < roomSettings.placedObjects.Count; num11++)
			{
				if ((num10 == 1 && roomSettings.placedObjects[num11].deactivatedByWarpFilter) || (num10 == 2 && !roomSettings.placedObjects[num11].deactivatedByWarpFilter) || !roomSettings.placedObjects[num11].active)
				{
					continue;
				}
				if (ModManager.Expedition && game.rainWorld.ExpeditionMode)
				{
					if (roomSettings.placedObjects[num11].type == PlacedObject.Type.BlueToken || roomSettings.placedObjects[num11].type == PlacedObject.Type.GoldToken || roomSettings.placedObjects[num11].type == PlacedObject.Type.GreenToken)
					{
						ExpLog.Log("Preventing Token spawn in Expedition mode");
						continue;
					}
					if (ModManager.MSC && (roomSettings.placedObjects[num11].type == MoreSlugcatsEnums.PlacedObjectType.RedToken || roomSettings.placedObjects[num11].type == MoreSlugcatsEnums.PlacedObjectType.WhiteToken || roomSettings.placedObjects[num11].type == MoreSlugcatsEnums.PlacedObjectType.DevToken))
					{
						ExpLog.Log("Preventing Token spawn in Expedition mode");
						continue;
					}
				}
				if (roomSettings.placedObjects[num11].type == PlacedObject.Type.LightSource)
				{
					LightSource lightSource = new LightSource(roomSettings.placedObjects[num11].pos, environmentalLight: true, new Color(1f, 1f, 1f), null);
					AddObject(lightSource);
					lightSource.setRad = (roomSettings.placedObjects[num11].data as PlacedObject.LightSourceData).Rad;
					lightSource.setAlpha = (roomSettings.placedObjects[num11].data as PlacedObject.LightSourceData).strength;
					lightSource.fadeWithSun = (roomSettings.placedObjects[num11].data as PlacedObject.LightSourceData).fadeWithSun;
					lightSource.colorFromEnvironment = (roomSettings.placedObjects[num11].data as PlacedObject.LightSourceData).colorType == PlacedObject.LightSourceData.ColorType.Environment;
					lightSource.flat = (roomSettings.placedObjects[num11].data as PlacedObject.LightSourceData).flat;
					lightSource.effectColor = Math.Max(-1, (int)(roomSettings.placedObjects[num11].data as PlacedObject.LightSourceData).colorType - 2);
					SetLightSourceBlink(lightSource, num11);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.LightFixture)
				{
					PlacedObject.LightFixtureData lightFixtureData = roomSettings.placedObjects[num11].data as PlacedObject.LightFixtureData;
					if (lightFixtureData.type == PlacedObject.LightFixtureData.Type.RedLight)
					{
						AddObject(new Redlight(this, roomSettings.placedObjects[num11], lightFixtureData));
					}
					else if (lightFixtureData.type == PlacedObject.LightFixtureData.Type.HolyFire)
					{
						AddObject(new HolyFire(this, roomSettings.placedObjects[num11], lightFixtureData));
					}
					else if (lightFixtureData.type == PlacedObject.LightFixtureData.Type.ZapCoilLight)
					{
						AddObject(new ZapCoilLight(this, roomSettings.placedObjects[num11], lightFixtureData));
					}
					else if (lightFixtureData.type == PlacedObject.LightFixtureData.Type.DeepProcessing)
					{
						AddObject(new DeepProcessingLight(this, roomSettings.placedObjects[num11], lightFixtureData));
					}
					else if (lightFixtureData.type == PlacedObject.LightFixtureData.Type.SlimeMoldLight)
					{
						AddObject(new SlimeMoldLight(this, roomSettings.placedObjects[num11], lightFixtureData));
					}
					else if (lightFixtureData.type == PlacedObject.LightFixtureData.Type.RedSubmersible)
					{
						AddObject(new Redlight(this, roomSettings.placedObjects[num11], lightFixtureData, submersible: true));
					}
					else if (lightFixtureData.type == PlacedObject.LightFixtureData.Type.GlowWeedLight)
					{
						AddObject(new GlowWeedLight(this, roomSettings.placedObjects[num11], lightFixtureData));
					}
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.CoralNeuron || roomSettings.placedObjects[num11].type == PlacedObject.Type.CoralCircuit || roomSettings.placedObjects[num11].type == PlacedObject.Type.CoralStem || roomSettings.placedObjects[num11].type == PlacedObject.Type.CoralStemWithNeurons || roomSettings.placedObjects[num11].type == PlacedObject.Type.WallMycelia)
				{
					bool flag4 = true;
					int num12 = updateList.Count - 1;
					while (num12 >= 0 && flag4)
					{
						flag4 = !(updateList[num12] is CoralNeuronSystem);
						num12--;
					}
					if (flag4)
					{
						AddObject(new CoralNeuronSystem());
					}
					waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 80);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.ProjectedStars)
				{
					AddObject(new StarMatrix(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.ZapCoil)
				{
					AddObject(new ZapCoil((roomSettings.placedObjects[num11].data as PlacedObject.GridRectObjectData).Rect, this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.SuperStructureFuses)
				{
					AddObject(new SuperStructureFuses(roomSettings.placedObjects[num11], (roomSettings.placedObjects[num11].data as PlacedObject.GridRectObjectData).Rect, this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.GravityDisruptor)
				{
					AddObject(new GravityDisruptor(roomSettings.placedObjects[num11], this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.SpotLight)
				{
					AddObject(new SpotLight(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.DeepProcessing)
				{
					AddObject(new DeepProcessing(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.Corruption || roomSettings.placedObjects[num11].type == PlacedObject.Type.PassiveCorruption)
				{
					DaddyCorruption daddyCorruption = null;
					int num13 = updateList.Count - 1;
					while (num13 >= 0 && daddyCorruption == null)
					{
						if (updateList[num13] is DaddyCorruption)
						{
							daddyCorruption = updateList[num13] as DaddyCorruption;
						}
						num13--;
					}
					if (daddyCorruption == null)
					{
						daddyCorruption = new DaddyCorruption(this);
						AddObject(daddyCorruption);
					}
					daddyCorruption.places.Add(roomSettings.placedObjects[num11]);
					waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 80);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.CorruptionDarkness)
				{
					AddObject(new DaddyCorruption.CorruptionDarkness(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.SSLightRod)
				{
					AddObject(new SSLightRod(roomSettings.placedObjects[num11], this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.VoidSpawnEgg)
				{
					if ((game.StoryCharacter != SlugcatStats.Name.Red || UnityEngine.Random.value < 1f / 17f) && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num11)) && (game.setupValues.playerGlowing || (game.session is StoryGameSession && (game.session as StoryGameSession).saveState.CanSeeVoidSpawn) || world.region.name == "SL"))
					{
						AddObject(new VoidSpawnEgg(this, num11, roomSettings.placedObjects[num11]));
					}
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.RippleSpawnEgg && world.regionState != null)
				{
					if (RegionState.RippleSpawnEggState.UseRegionEggTracker)
					{
						_ = 1;
					}
					else
						_ = world.name.ToLowerInvariant() == "wora";
					if (VoidSpawnEgg.CanSpawnRippleEgg(this, num9))
					{
						AddObject(new VoidSpawnEgg(this, num9, roomSettings.placedObjects[num11]));
					}
					num9++;
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.RippleTree || roomSettings.placedObjects[num11].type == PlacedObject.Type.RippleStalk)
				{
					AddObject(new RippleTree(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.GhostSpot)
				{
					if (game.world.worldGhost != null && game.world.worldGhost.ghostRoom == abstractRoom)
					{
						AddObject(new Ghost(this, roomSettings.placedObjects[num11], game.world.worldGhost));
					}
					else if (world.region != null)
					{
						GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(world.region.name);
						if (game.session is StoryGameSession && (!(game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) || (game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] == 0))
						{
							AddObject(new GhostHunch(this, ghostID));
						}
					}
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.SpinningTopSpot && game.IsStorySession && !game.rainWorld.safariMode && game.StoryCharacter == WatcherEnums.SlugcatStatsName.Watcher)
				{
					GhostWorldPresence ghostWorldPresence = null;
					for (int num14 = 0; num14 < game.world.spinningTopPresences.Count; num14++)
					{
						if (game.world.spinningTopPresences[num14].ghostRoom == abstractRoom)
						{
							ghostWorldPresence = game.world.spinningTopPresences[num14];
							break;
						}
					}
					if (ghostWorldPresence == null)
					{
						ghostWorldPresence = new GhostWorldPresence(world, WatcherEnums.GhostID.SpinningTop, (roomSettings.placedObjects[num11].data as SpinningTopData).spawnIdentifier);
						ghostWorldPresence.ghostRoom = abstractRoom;
						if (!game.GetStorySession.saveState.deathPersistentSaveData.spinningTopEncounters.Contains(ghostWorldPresence.spinningTopSpawnId))
						{
							game.world.spinningTopPresences.Add(ghostWorldPresence);
						}
					}
					if (!game.GetStorySession.saveState.deathPersistentSaveData.spinningTopEncounters.Contains(ghostWorldPresence.spinningTopSpawnId))
					{
						spawnedSpinningTop = true;
						AddObject(new SpinningTop(this, roomSettings.placedObjects[num11], ghostWorldPresence));
					}
					else
					{
						SpinningTop.SpawnBackupWarpPoint(this, roomSettings.placedObjects[num11]);
					}
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.WeaverSpot)
				{
					if (world.weaverPresence != null && game.GetStorySession.voidWeaverEncountersThisCycle.Count == 0 && world.weaverPresence.weaverRoom == abstractRoom)
					{
						spawnedWeaver = true;
						AddObject(new VoidWeaver(roomSettings.placedObjects[num11]));
					}
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.ScavengerOutpost)
				{
					AddObject(new ScavengerOutpost(roomSettings.placedObjects[num11], this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.SuperJumpInstruction)
				{
					AddObject(new SuperJumpInstruction(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.LanternOnStick)
				{
					AddObject(new LanternStick(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.TradeOutpost)
				{
					AddObject(new ScavengerTradeSpot(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.ScavengerTreasury)
				{
					AddObject(new ScavengerTreasury(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.ScavTradeInstruction)
				{
					AddObject(new ScavengerTradeInstructionTrigger(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.CosmeticSlimeMold)
				{
					AddObject(new SlimeMold.CosmeticSlimeMold(this, roomSettings.placedObjects[num11].pos, (roomSettings.placedObjects[num11].data as PlacedObject.ResizableObjectData).Rad, throughWalls: false));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.CosmeticSlimeMold2)
				{
					AddObject(new SlimeMold.CosmeticSlimeMold(this, roomSettings.placedObjects[num11].pos, (roomSettings.placedObjects[num11].data as PlacedObject.ResizableObjectData).Rad, throughWalls: true));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.SlimeMold)
				{
					float num15 = game.SeededRandom((int)(roomSettings.placedObjects[num11].pos.x + roomSettings.placedObjects[num11].pos.y));
					if (num15 > 0.3f)
					{
						AddObject(new SlimeMold.CosmeticSlimeMold(this, roomSettings.placedObjects[num11].pos, Custom.LerpMap(num15, 0.3f, 1f, 30f, 70f), throughWalls: false));
					}
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.CustomDecal)
				{
					AddObject(new CustomDecal(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.InsectGroup)
				{
					if (insectCoordinator == null)
					{
						insectCoordinator = new InsectCoordinator(this);
						AddObject(insectCoordinator);
					}
					insectCoordinator.AddGroup(roomSettings.placedObjects[num11]);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.PlayerPushback)
				{
					AddObject(new PlayerPushback(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.MultiplayerItem)
				{
					if (game.IsArenaSession)
					{
						game.GetArenaGameSession.SpawnItem(this, roomSettings.placedObjects[num11]);
					}
					if (game.IsStorySession)
					{
						SpawnMultiplayerItem(roomSettings.placedObjects[num11]);
					}
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.GoldToken || roomSettings.placedObjects[num11].type == PlacedObject.Type.BlueToken)
				{
					if (!(game.session is StoryGameSession) || world.singleRoomWorld || !(game.session as StoryGameSession).game.rainWorld.progression.miscProgressionData.GetTokenCollected((roomSettings.placedObjects[num11].data as CollectToken.CollectTokenData).tokenString, (roomSettings.placedObjects[num11].data as CollectToken.CollectTokenData).isBlue))
					{
						AddObject(new CollectToken(this, roomSettings.placedObjects[num11]));
					}
					else
					{
						AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num11].pos, roomSettings.placedObjects[num11].pos + (roomSettings.placedObjects[num11].data as CollectToken.CollectTokenData).handlePos, null, blue: false));
					}
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.DeadTokenStalk)
				{
					AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num11].pos, roomSettings.placedObjects[num11].pos + (roomSettings.placedObjects[num11].data as PlacedObject.ResizableObjectData).handlePos, null, blue: false));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.ReliableIggyDirection)
				{
					AddObject(new ReliableIggyDirection(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.Rainbow)
				{
					if ((world.rainCycle.CycleStartUp < 1f && (game.cameras[0] == null || game.cameras[0].ghostMode == 0f) && (game.SeededRandom(world.rainCycle.rainbowSeed + abstractRoom.index) < (roomSettings.placedObjects[num11].data as Rainbow.RainbowData).Chance || (ModManager.PrecycleModule && world.rainCycle.preTimer > 0))) || game.IsArenaSession)
					{
						AddObject(new Rainbow(this, roomSettings.placedObjects[num11]));
					}
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.RainbowNoFade)
				{
					AddObject(new RainbowNoFade(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.ARKillRect)
				{
					AddObject(new ARKillRect(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.LightBeam)
				{
					LightBeam lightBeam = new LightBeam(roomSettings.placedObjects[num11]);
					SetLightBeamBlink(lightBeam, num11);
					AddObject(lightBeam);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.DeathFallFocus)
				{
					deathFallFocalPoints.Add(roomSettings.placedObjects[num11].pos);
				}
				else if (ModManager.NewSlugcatsModule && roomSettings.placedObjects[num11].type == PlacedObject.Type.GreenToken)
				{
					if (!(game.session is StoryGameSession) || world.singleRoomWorld || !(game.session as StoryGameSession).game.rainWorld.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SlugcatUnlockID((roomSettings.placedObjects[num11].data as CollectToken.CollectTokenData).tokenString)))
					{
						AddObject(new CollectToken(this, roomSettings.placedObjects[num11]));
					}
					else
					{
						AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num11].pos, roomSettings.placedObjects[num11].pos + (roomSettings.placedObjects[num11].data as CollectToken.CollectTokenData).handlePos, null, blue: false));
					}
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num11].type == MoreSlugcatsEnums.PlacedObjectType.WhiteToken)
				{
					CollectToken.CollectTokenData collectTokenData = roomSettings.placedObjects[num11].data as CollectToken.CollectTokenData;
					if (!collectTokenData.availableToPlayers.Contains(MoreSlugcatsEnums.SlugcatStatsName.Spear))
					{
						collectTokenData.availableToPlayers.Add(MoreSlugcatsEnums.SlugcatStatsName.Spear);
					}
					if (game.IsStorySession && !collectTokenData.availableToPlayers.Contains(game.StoryCharacter))
					{
						continue;
					}
					if (game.IsStorySession && game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Spear)
					{
						AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num11].pos, roomSettings.placedObjects[num11].pos + (roomSettings.placedObjects[num11].data as PlacedObject.ResizableObjectData).handlePos, null, blue: false));
						continue;
					}
					ChatlogData.ChatlogID chatlogCollect = collectTokenData.ChatlogCollect;
					if (chatlogCollect == null || !game.IsStorySession || game.GetStorySession.saveState.deathPersistentSaveData.chatlogsRead.Contains(chatlogCollect))
					{
						if (!collectTokenData.availableToPlayers.Contains(MoreSlugcatsEnums.SlugcatStatsName.Spear))
						{
							collectTokenData.availableToPlayers.Add(MoreSlugcatsEnums.SlugcatStatsName.Spear);
						}
						CollectToken.TokenStalk obj2 = new CollectToken.TokenStalk(this, roomSettings.placedObjects[num11].pos, roomSettings.placedObjects[num11].pos + collectTokenData.handlePos, null, blue: false)
						{
							forceSatellite = true
						};
						AddObject(obj2);
					}
					else
					{
						AddObject(new CollectToken(this, roomSettings.placedObjects[num11]));
					}
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num11].type == MoreSlugcatsEnums.PlacedObjectType.RedToken)
				{
					if (!(game.session is StoryGameSession) || world.singleRoomWorld || !(game.session as StoryGameSession).game.rainWorld.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SafariUnlockID((roomSettings.placedObjects[num11].data as CollectToken.CollectTokenData).tokenString)))
					{
						AddObject(new CollectToken(this, roomSettings.placedObjects[num11]));
					}
					else
					{
						AddObject(new CollectToken.TokenStalk(this, roomSettings.placedObjects[num11].pos, roomSettings.placedObjects[num11].pos + (roomSettings.placedObjects[num11].data as CollectToken.CollectTokenData).handlePos, null, blue: false));
					}
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num11].type == MoreSlugcatsEnums.PlacedObjectType.DevToken && game.rainWorld.options.commentary && game.rainWorld.options.DeveloperCommentaryLocalized())
				{
					AddObject(new CollectToken(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.LightningMachine)
				{
					AddObject(new LightningMachine(roomSettings.placedObjects[num11].pos, new Vector2(0f, 0f), new Vector2(0f, 0f), 1f, permanent: false, radial: false, 1f, 1f, 10f)
					{
						startPoint = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).startPoint,
						endPoint = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).endPoint,
						chance = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).chance,
						permanent = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).permanent,
						radial = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).radial,
						width = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).width,
						intensity = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).intensity,
						lifeTime = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).lifeTime,
						lightningParam = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).lightningParam,
						lightningType = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).lightningType,
						volume = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).volume,
						impactType = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).impact,
						soundType = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).soundType,
						random = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).random,
						light = (roomSettings.placedObjects[num11].data as PlacedObject.LightningMachineData).light
					});
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.EnergySwirl)
				{
					AddObject(new EnergySwirl(roomSettings.placedObjects[num11].pos, new Color(1f, 1f, 1f), null)
					{
						setRad = (roomSettings.placedObjects[num11].data as PlacedObject.EnergySwirlData).Rad,
						setDepth = (roomSettings.placedObjects[num11].data as PlacedObject.EnergySwirlData).depth,
						colorFromEnviroment = ((roomSettings.placedObjects[num11].data as PlacedObject.EnergySwirlData).colorType == PlacedObject.EnergySwirlData.ColorType.Environment),
						effectColor = Math.Max(-1, (roomSettings.placedObjects[num11].data as PlacedObject.EnergySwirlData).colorType.Index - PlacedObject.EnergySwirlData.ColorType.EffectColor1.Index)
					});
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.SteamPipe || roomSettings.placedObjects[num11].type == PlacedObject.Type.WallSteamer)
				{
					AddObject(new SteamPipe(roomSettings.placedObjects[num11].pos, Custom.DirVec(roomSettings.placedObjects[num11].pos, roomSettings.placedObjects[num11].pos + (roomSettings.placedObjects[num11].data as PlacedObject.SteamPipeData).handlePos), Mathf.Clamp((roomSettings.placedObjects[num11].data as PlacedObject.SteamPipeData).Rad / 250f, 0f, 1f), roomSettings.placedObjects[num11].type == PlacedObject.Type.WallSteamer));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.SnowSource)
				{
					SnowSource snowSource = new SnowSource(roomSettings.placedObjects[num11].pos);
					AddObject(snowSource);
					snowSource.rad = (roomSettings.placedObjects[num11].data as PlacedObject.SnowSourceData).Rad;
					snowSource.intensity = (roomSettings.placedObjects[num11].data as PlacedObject.SnowSourceData).intensity;
					snowSource.noisiness = (roomSettings.placedObjects[num11].data as PlacedObject.SnowSourceData).noisiness;
					snowSource.shape = (roomSettings.placedObjects[num11].data as PlacedObject.SnowSourceData).shape;
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.LocalBlizzard)
				{
					LocalBlizzard localBlizzard = new LocalBlizzard(roomSettings.placedObjects[num11].pos, 100f, 1f, 0.5f);
					AddObject(localBlizzard);
					localBlizzard.rad = (roomSettings.placedObjects[num11].data as PlacedObject.LocalBlizzardData).Rad;
					localBlizzard.intensity = (roomSettings.placedObjects[num11].data as PlacedObject.LocalBlizzardData).intensity;
					localBlizzard.scale = (roomSettings.placedObjects[num11].data as PlacedObject.LocalBlizzardData).scale;
					localBlizzard.angle = (roomSettings.placedObjects[num11].data as PlacedObject.LocalBlizzardData).angle;
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.Vine)
				{
					ClimbableVineRenderer climbableVineRenderer = null;
					int num16 = updateList.Count - 1;
					while (num16 >= 0 && climbableVineRenderer == null)
					{
						if (updateList[num16] is ClimbableVineRenderer)
						{
							climbableVineRenderer = updateList[num16] as ClimbableVineRenderer;
						}
						num16--;
					}
					if (climbableVineRenderer == null)
					{
						climbableVineRenderer = new ClimbableVineRenderer(this);
						AddObject(climbableVineRenderer);
					}
					waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 80);
				}
				else if (ModManager.DLCShared && roomSettings.placedObjects[num11].type == DLCSharedEnums.PlacedObjectType.OEsphere)
				{
					OEsphere oEsphere = new OEsphere(roomSettings.placedObjects[num11].pos, 100f, 0);
					AddObject(oEsphere);
					oEsphere.rad = (roomSettings.placedObjects[num11].data as PlacedObject.OEsphereData).Rad;
					oEsphere.depth = (roomSettings.placedObjects[num11].data as PlacedObject.OEsphereData).depth;
					oEsphere.lIntensity = (roomSettings.placedObjects[num11].data as PlacedObject.OEsphereData).lIntensity;
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.CellDistortion)
				{
					CellDistortion cellDistortion = new CellDistortion(roomSettings.placedObjects[num11].pos, 100f, 1f, 0.5f, 0f, 0f);
					AddObject(cellDistortion);
					cellDistortion.rad = (roomSettings.placedObjects[num11].data as PlacedObject.CellDistortionData).Rad;
					cellDistortion.intensity = (roomSettings.placedObjects[num11].data as PlacedObject.CellDistortionData).intensity;
					cellDistortion.scale = (roomSettings.placedObjects[num11].data as PlacedObject.CellDistortionData).scale;
					cellDistortion.cromaticIntensity = (roomSettings.placedObjects[num11].data as PlacedObject.CellDistortionData).chromaticIntensity;
					cellDistortion.timeMult = (roomSettings.placedObjects[num11].data as PlacedObject.CellDistortionData).timeMult;
				}
				else if (ModManager.MSC && roomSettings.placedObjects[num11].type == MoreSlugcatsEnums.PlacedObjectType.KarmaShrine)
				{
					AddObject(new HRKarmaShrine(this, roomSettings.placedObjects[num11].pos, roomSettings.placedObjects[num11].data as PlacedObject.ResizableObjectData));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.NeuronSpawner)
				{
					AddObject(new SSSwarmerSpawner(this, roomSettings.placedObjects[num11].data as PlacedObject.ResizableObjectData));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.HangingPearls)
				{
					float length = Mathf.Lerp(60f, 180f, 0.5f + Mathf.Sin(roomSettings.placedObjects[num11].pos.x * 10f) / 2f);
					AddObject(new HangingPearlString(this, length, roomSettings.placedObjects[num11].pos));
				}
				else if (ModManager.DLCShared && roomSettings.placedObjects[num11].type == DLCSharedEnums.PlacedObjectType.MSArteryPush)
				{
					AddObject(new MSArteryWaterFlow(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.AdjustableFan)
				{
					AddObject(new AdjustableFan(roomSettings.placedObjects[num11], this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.HarmfulSteam)
				{
					AddObject(new HarmfulSteam(roomSettings.placedObjects[num11], this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.SkyWhalePathfinding)
				{
					AddObject(new SkyWhalePathfindingNode(roomSettings.placedObjects[num11], this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.WarpPoint)
				{
					if (Region.IsVanillaSentientRotRegion(world.name) && !game.GetStorySession.saveState.deathPersistentSaveData.spinningTopRotEncounter && game.StoryCharacter == WatcherEnums.SlugcatStatsName.Watcher)
					{
						PlacedObject placedObject = new PlacedObject(WatcherEnums.PlacedObjectType.SpinningTopSpot, null);
						placedObject.pos = roomSettings.placedObjects[num11].pos;
						(placedObject.data as SpinningTopData).spawnIdentifier = 666;
						(placedObject.data as SpinningTopData).destPos = (roomSettings.placedObjects[num11].data as WarpPoint.WarpPointData).destPos;
						(placedObject.data as SpinningTopData).destRoom = (roomSettings.placedObjects[num11].data as WarpPoint.WarpPointData).destRoom;
						(placedObject.data as SpinningTopData).destTimeline = (roomSettings.placedObjects[num11].data as WarpPoint.WarpPointData).destTimeline;
						(placedObject.data as SpinningTopData).RegionString = (roomSettings.placedObjects[num11].data as WarpPoint.WarpPointData).RegionString;
						(placedObject.data as SpinningTopData).panelPos = (roomSettings.placedObjects[num11].data as WarpPoint.WarpPointData).panelPos;
						(placedObject.data as SpinningTopData).rippleWarp = (roomSettings.placedObjects[num11].data as WarpPoint.WarpPointData).rippleWarp;
						(placedObject.data as SpinningTopData).owner = placedObject;
						roomSettings.placedObjects[num11] = placedObject;
						GhostWorldPresence ghostWorldPresence2 = new GhostWorldPresence(world, WatcherEnums.GhostID.SpinningTop, (placedObject.data as SpinningTopData).spawnIdentifier);
						ghostWorldPresence2.ghostRoom = abstractRoom;
						bool flag5 = false;
						for (int num17 = 0; num17 < world.spinningTopPresences.Count; num17++)
						{
							if (world.spinningTopPresences[num17].ghostRoom.name == abstractRoom.name)
							{
								flag5 = true;
								break;
							}
						}
						if (!flag5)
						{
							world.spinningTopPresences.Add(ghostWorldPresence2);
						}
						spawnedSpinningTop = true;
						AddObject(new SpinningTop(this, roomSettings.placedObjects[num11], ghostWorldPresence2));
					}
					else if (game.IsStorySession)
					{
						TrySpawnWarpPoint(roomSettings.placedObjects[num11]);
					}
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.WindRect)
				{
					AddObject(new WindRect(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.CosmeticRipple && (!ModManager.Watcher || !global::Watcher.Watcher.cfgDisableCosmeticRipples.Value))
				{
					(roomSettings.placedObjects[num11].data as CosmeticRippleData).animateOnSpawn = false;
					TrySpawnRippleRegion(roomSettings.placedObjects[num11], saveInRegionState: false, skipIfInRegionState: true);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.FluxWaterfall)
				{
					PlacedObject.WaterFlowData data = roomSettings.placedObjects[num11].data as PlacedObject.WaterFlowData;
					FluxWaterfall fluxWaterfall = new FluxWaterfall(this, GetTilePosition(roomSettings.placedObjects[num11].pos + new Vector2(10f, -10f)), data);
					Array.Resize(ref waterFalls, waterFalls.Length + 1);
					waterFalls[waterFalls.Length - 1] = fluxWaterfall;
					if (waterObject != null)
					{
						fluxWaterfall.ConnectToWaterObject(waterObject);
					}
					AddObject(fluxWaterfall);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.FluxDrain)
				{
					AddObject(new FluxDrain(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.PomegranateVine)
				{
					AddObject(new Pomegranate.CosmeticStalk(roomSettings.placedObjects[num11], this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.HugeTurbine)
				{
					AddObject(new HugeTurbine(roomSettings.placedObjects[num11], this));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.Geyser)
				{
					AddObject(new Geyser(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.PlateTree)
				{
					AddObject(new PlateTree(roomSettings.placedObjects[num11], this, PlateTree.Variant.Plate));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.RotPlateTree)
				{
					AddObject(new PlateTree(roomSettings.placedObjects[num11], this, PlateTree.Variant.Rot));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.WaterCurrent)
				{
					AddObject(new WaterCurrent(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.NoCurrentZone)
				{
					noCurrentZones.Add(roomSettings.placedObjects[num11]);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.BlackSpot)
				{
					AddObject(new BlackSpot(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.MudPit)
				{
					AddObject(new MudPit(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.TerrainGrassPatch)
				{
					GrassBlade.SpawnGrassPatch(this, roomSettings.placedObjects[num11].data as GrassBlade.TerrainGrassPatchData);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.FloatingDebris)
				{
					FloatingDebris floatingDebris = FloatingDebris.TrySpawnFloatingDebris(this, roomSettings.placedObjects[num11]);
					if (floatingDebris != null)
					{
						AddObject(floatingDebris);
						(roomSettings.placedObjects[num11].data as FloatingDebrisData).obj = floatingDebris;
					}
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == PlacedObject.Type.SandGrubHole)
				{
					AddObject(new SandGrubBurrow(this, roomSettings.placedObjects[num11]));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == PlacedObject.Type.SandGrubNetwork)
				{
					AddObject(new SandGrubNetwork(roomSettings.placedObjects[num11], num11, abstractRoom.firstTimeRealized));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.GooDrips)
				{
					AddObject(new GooDripSource(roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.BrainMold)
				{
					if (brainMold == null)
					{
						AddObject(brainMold = new BrainMold());
					}
					brainMold.AddSpot(roomSettings.placedObjects[num11]);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.FlameJet)
				{
					FlameJet obj3 = FlameJet.FromPlacedObject(this, roomSettings.placedObjects[num11]);
					AddObject(obj3);
					(roomSettings.placedObjects[num11].data as FlameJet.FlameJetData).obj = obj3;
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.WindDirection)
				{
					windDirObject = new WindDirection(this, roomSettings.placedObjects[num11]);
					AddObject(windDirObject);
				}
				else if (roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.AetherRainbow)
				{
					AetherRainbow.AetherRainbowData aetherRainbowData = roomSettings.placedObjects[num11].data as AetherRainbow.AetherRainbowData;
					if (((game.cameras[0] == null || game.cameras[0].ghostMode == 0f) && (game.SeededRandom(world.rainCycle.rainbowSeed + abstractRoom.index) < aetherRainbowData.chance || (ModManager.PrecycleModule && world.rainCycle.preTimer > 0))) || game.IsArenaSession)
					{
						AddObject(aetherRainbowData.obj = AetherRainbow.FromPlacedObject(this, roomSettings.placedObjects[num11]));
					}
				}
				else if (roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.WallLight)
				{
					WallLight.WallLightData wallLightData = roomSettings.placedObjects[num11].data as WallLight.WallLightData;
					AddObject(wallLightData.obj = WallLight.FromPlacedObject(this, roomSettings.placedObjects[num11]));
				}
				else if (roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.SKLightningRod)
				{
					AddObject(new SKLightningRod(this, roomSettings.placedObjects[num11].pos, (roomSettings.placedObjects[num11].data as PlacedObject.ResizableObjectData).Rad));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.UrbanLife)
				{
					UrbanLife.UrbanLifeData urbanLifeData = roomSettings.placedObjects[num11].data as UrbanLife.UrbanLifeData;
					urbanLifeData.index = urbanLifeCount;
					urbanLifeCount++;
					AddObject(urbanLifeData.obj = UrbanLife.FromPlacedObject(this, roomSettings.placedObjects[num11]));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.UrbanLifePath)
				{
					UrbanLifePath.UrbanLifePathData urbanLifePathData = roomSettings.placedObjects[num11].data as UrbanLifePath.UrbanLifePathData;
					AddObject(urbanLifePathData.obj = UrbanLifePath.FromPlacedObject(this, roomSettings.placedObjects[num11]));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.PrinceBulb)
				{
					SpawnPrinceBulb(roomSettings.placedObjects[num11]);
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.UrbanCandles)
				{
					UrbanCandlePlacer.UrbanCandlePlacerData urbanCandlePlacerData = roomSettings.placedObjects[num11].data as UrbanCandlePlacer.UrbanCandlePlacerData;
					for (int num18 = 0; num18 < urbanCandlePlacerData.candles.Count; num18++)
					{
						UrbanCandle urbanCandle = new UrbanCandle(this, urbanCandlePlacerData.candles[num18]);
						if (ModManager.DevTools)
						{
							urbanCandlePlacerData.placedCandles.Add(urbanCandle);
						}
						AddObject(urbanCandle);
					}
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.UrbanCandleHolder)
				{
					UrbanCandleHolder.UrbanCandleHolderData urbanCandleHolderData = roomSettings.placedObjects[num11].data as UrbanCandleHolder.UrbanCandleHolderData;
					AddObject(urbanCandleHolderData.obj = new UrbanCandleHolder(this, urbanCandleHolderData));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.KarmaFlowerPatch)
				{
					if (!updateList.OfType<KarmaFlowerPatch>().Any())
					{
						AddObject(new KarmaFlowerPatch());
					}
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.ARZapperOmni)
				{
					PlacedObject.GridRectObjectData gridRectObjectData = roomSettings.placedObjects[num11].data as PlacedObject.GridRectObjectData;
					AddObject(new ARZapper(gridRectObjectData.Rect, 2, this));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.ARZapperVertical)
				{
					PlacedObject.GridRectObjectData gridRectObjectData2 = roomSettings.placedObjects[num11].data as PlacedObject.GridRectObjectData;
					AddObject(new ARZapper(gridRectObjectData2.Rect, 0, this));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.ARZapperHorizontal)
				{
					PlacedObject.GridRectObjectData gridRectObjectData3 = roomSettings.placedObjects[num11].data as PlacedObject.GridRectObjectData;
					AddObject(new ARZapper(gridRectObjectData3.Rect, 1, this));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.DaemonClaw)
				{
					PlacedObject.SteamPipeData steamPipeData = roomSettings.placedObjects[num11].data as PlacedObject.SteamPipeData;
					AddObject(new DaemonClaw(this, roomSettings.placedObjects[num11].pos, steamPipeData.handlePos));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.DaemonEye)
				{
					DaemonEyeData daemonEyeData = roomSettings.placedObjects[num11].data as DaemonEyeData;
					AddObject(new DaemonEye(roomSettings.placedObjects[num11], this, roomSettings.placedObjects[num11].pos, daemonEyeData.Rad, daemonEyeData.depth));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.DaemonCrown)
				{
					DaemonCrownData daemonCrownData = roomSettings.placedObjects[num11].data as DaemonCrownData;
					AddObject(new DaemonCrown(roomSettings.placedObjects[num11], this, roomSettings.placedObjects[num11].pos, daemonCrownData.Rad, daemonCrownData.depth));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.PearlReader)
				{
					PlacedObject.ResizableObjectData resizableObjectData = roomSettings.placedObjects[num11].data as PlacedObject.ResizableObjectData;
					AddObject(new PearlReader(this, roomSettings.placedObjects[num11].pos, resizableObjectData.handlePos));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.BigSkyWhaleSpawner)
				{
					AddObject(new BigSkyWhaleSpawner(this, roomSettings.placedObjects[num11]));
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.BigSkyWhaleTrigger)
				{
					StoryGameSession getStorySession = game.GetStorySession;
					if (getStorySession == null || !getStorySession.saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num11))
					{
						AddObject(new BigSkyWhaleTrigger(this, roomSettings.placedObjects[num11]));
					}
				}
				else if (ModManager.Watcher && roomSettings.placedObjects[num11].type == WatcherEnums.PlacedObjectType.TowerCrabSpawner && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.TowerCrab, game))
				{
					AddObject(new TowerCrabSpawner(this, roomSettings.placedObjects[num11]));
					waitToEnterAfterFullyLoaded = Mathf.Max(waitToEnterAfterFullyLoaded, 40);
				}
				else if (roomSettings.placedObjects[num11].type == PlacedObject.Type.SpawnMigrationStream)
				{
					AddObject(new VoidSpawnMigrationStream(this, roomSettings.placedObjects[num11].data as PlacedObject.SpawnMigrationStreamData));
				}
			}
			if (num9 > 0 && world.regionState != null)
			{
				world.regionState.rippleSpawnEggState.AddRoom(abstractRoom.name, num9);
			}
			if (num10 != 1 || abstractRoom.arenaSandGrubSpots == null || abstractRoom.arenaSandGrubSpots.Count <= 0)
			{
				continue;
			}
			Vector2 zero = Vector2.zero;
			foreach (Vector2 arenaSandGrubSpot in abstractRoom.arenaSandGrubSpots)
			{
				SandGrubBurrow sandGrubBurrow = new SandGrubBurrow(this, null);
				AddObject(sandGrubBurrow);
				sandGrubBurrow.pos = FindGroundBelow(arenaSandGrubSpot, out sandGrubBurrow.dir);
				zero += sandGrubBurrow.pos;
			}
			zero /= (float)abstractRoom.arenaSandGrubSpots.Count;
			AddObject(new SandGrubNetwork(zero, abstractRoom.arenaSandGrubAdults));
		}
		if (abstractRoom == null)
		{
			Custom.LogWarning("NULL ABSTRACT ROOM");
		}
		bool flag6 = false;
		if (game.world.worldGhost != null && game.world.worldGhost.CreaturesSleepInRoom(abstractRoom))
		{
			AddObject(new GhostCreatureSedater(this));
			flag6 = true;
		}
		if (ModManager.Watcher)
		{
			for (int num19 = 0; num19 < world.spinningTopPresences.Count; num19++)
			{
				if (flag6)
				{
					break;
				}
				if (world.spinningTopPresences[num19].CreaturesSleepInRoom(abstractRoom))
				{
					AddObject(new GhostCreatureSedater(this));
					flag6 = true;
				}
			}
			if (world.weaverPresence != null && world.weaverPresence.weaverRoom == abstractRoom)
			{
				AddObject(new GhostCreatureSedater(this));
				flag6 = true;
			}
		}
		if (ModManager.Watcher && game.IsStorySession && game.StoryCharacter == WatcherEnums.SlugcatStatsName.Watcher)
		{
			rippleCreatureTracker = new RippleCreatureTracker(this);
			AddObject(rippleCreatureTracker);
			float effectAmount = roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt);
			if (effectAmount > 0f)
			{
				RippleDepths.SpawnRippleDephts(this, Mathf.InverseLerp(0.9f, 0.45f, effectAmount));
			}
		}
		if (roomSettings.roomSpecificScript)
		{
			RoomSpecificScript.AddRoomSpecificScript(this);
		}
		if (abstractRoom.firstTimeRealized)
		{
			for (int num20 = 1; num20 <= 2; num20++)
			{
				if (num20 == 2 && warpPoints.Count > 0)
				{
					continue;
				}
				for (int num21 = 0; num21 < roomSettings.placedObjects.Count; num21++)
				{
					if ((num20 == 1 && roomSettings.placedObjects[num21].deactivatedByWarpFilter) || (num20 == 2 && !roomSettings.placedObjects[num21].deactivatedByWarpFilter) || !roomSettings.placedObjects[num21].active || CheckForWarpedObjects(num21))
					{
						continue;
					}
					if (ModManager.Expedition && game.rainWorld.ExpeditionMode && roomSettings.placedObjects[num21].type == PlacedObject.Type.KarmaFlower)
					{
						ExpLog.Log("Preventing natural KarmaFlower spawn");
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.FlareBomb)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.AddEntity(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.PuffBall)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.PuffBall, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.AddEntity(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.GraffitiBomb)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new GraffitiBomb.AbstractGraffitiBomb(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.AddEntity(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.TempleGuard)
					{
						if (game.setupValues.worldCreaturesSpawn)
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TempleGuard), null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID());
							abstractRoom.AddEntity(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.DangleFruit || (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.RottenDangleFruit))
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new DangleFruit.AbstractDangleFruit(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.RottenDangleFruit, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.DataPearl || roomSettings.placedObjects[num21].type == PlacedObject.Type.UniqueDataPearl)
					{
						if (game.session is StoryGameSession && (game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							continue;
						}
						DataPearl.AbstractDataPearl.DataPearlType pearlType = (roomSettings.placedObjects[num21].data as PlacedObject.DataPearlData).pearlType;
						AbstractPhysicalObject abstractPhysicalObject;
						if (ModManager.MSC && pearlType == MoreSlugcatsEnums.DataPearlType.Spearmasterpearl)
						{
							abstractPhysicalObject = new SpearMasterPearl.AbstractSpearMasterPearl(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
						}
						else
						{
							AbstractPhysicalObject.AbstractObjectType objType = AbstractPhysicalObject.AbstractObjectType.DataPearl;
							if (ModManager.MSC && pearlType == MoreSlugcatsEnums.DataPearlType.RM)
							{
								objType = MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl;
							}
							abstractPhysicalObject = new DataPearl.AbstractDataPearl(world, objType, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData, (SlugcatStats.AtOrAfterTimeline(game.TimelinePoint, SlugcatStats.Timeline.Yellow) && !ModManager.NewSlugcatsModule) ? DataPearl.AbstractDataPearl.DataPearlType.Misc : pearlType);
						}
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						(abstractPhysicalObject as DataPearl.AbstractDataPearl).hidden = (roomSettings.placedObjects[num21].data as PlacedObject.DataPearlData).hidden;
						abstractRoom.entities.Add(abstractPhysicalObject);
						abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.SeedCob || (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.RottenSeedCob))
					{
						AbstractPhysicalObject abstractPhysicalObject = new SeedCob.AbstractSeedCob(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, dead: false, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject as SeedCob.AbstractSeedCob).rotted = ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.RottenSeedCob;
						(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject);
						abstractPhysicalObject.Realize();
						abstractPhysicalObject.realizedObject.PlaceInRoom(this);
						abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.DeadSeedCob)
					{
						AbstractPhysicalObject abstractPhysicalObject = new SeedCob.AbstractSeedCob(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, dead: true, null);
						abstractRoom.entities.Add(abstractPhysicalObject);
						abstractPhysicalObject.Realize();
						abstractPhysicalObject.realizedObject.PlaceInRoom(this);
						abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.WaterNut)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new WaterNut.AbstractWaterNut(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData, swollen: false);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.AddEntity(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.JellyFish)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.JellyFish, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.KarmaFlower)
					{
						if (KarmaFlower.CanSpawnKarmaFlower(this) && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: true, abstractRoom.index, num21)))
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.Mushroom)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.Mushroom, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.FirecrackerPlant)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.AddEntity(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.VultureGrub || roomSettings.placedObjects[num21].type == PlacedObject.Type.DeadVultureGrub)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.VultureGrub), null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID());
							(abstractCreature.state as VultureGrub.VultureGrubState).origRoom = abstractRoom.index;
							(abstractCreature.state as VultureGrub.VultureGrubState).placedObjectIndex = num21;
							abstractRoom.AddEntity(abstractCreature);
							abstractCreature.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
							if (roomSettings.placedObjects[num21].type == PlacedObject.Type.DeadVultureGrub)
							{
								(abstractCreature.state as VultureGrub.VultureGrubState).Die();
							}
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.SlimeMold)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.ReliableSpear)
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractSpear(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), explosive: false);
						abstractRoom.entities.Add(abstractPhysicalObject);
						abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.FlyLure)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FlyLure, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.SporePlant)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new SporePlant.AbstractSporePlant(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData, used: false, pacified: false);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.NeedleEgg)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.BubbleGrass)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new BubbleGrass.AbstractBubbleGrass(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), 1f, abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.Hazer || roomSettings.placedObjects[num21].type == PlacedObject.Type.DeadHazer)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractCreature abstractCreature2 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Hazer), null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID());
							(abstractCreature2.state as VultureGrub.VultureGrubState).origRoom = abstractRoom.index;
							(abstractCreature2.state as VultureGrub.VultureGrubState).placedObjectIndex = num21;
							abstractRoom.AddEntity(abstractCreature2);
							abstractCreature2.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
							if (roomSettings.placedObjects[num21].type == PlacedObject.Type.DeadHazer)
							{
								(abstractCreature2.state as VultureGrub.VultureGrubState).Die();
							}
						}
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.VultureMask)
					{
						float num22 = 0.1f;
						EntityID newID = game.GetNewID();
						AbstractPhysicalObject abstractPhysicalObject2 = new VultureMask.AbstractVultureMask(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), newID, newID.RandomSeed, UnityEngine.Random.value <= num22);
						abstractRoom.entities.Add(abstractPhysicalObject2);
						abstractPhysicalObject2.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.Lantern && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21)))
					{
						EntityID newID2 = game.GetNewID();
						AbstractPhysicalObject abstractPhysicalObject3 = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), newID2, abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject3 as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject3);
						abstractPhysicalObject3.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.BlinkingFlower)
					{
						AbstractPhysicalObject abstractPhysicalObject4 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.BlinkingFlower, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID());
						abstractRoom.entities.Add(abstractPhysicalObject4);
						abstractPhysicalObject4.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.Pomegranate)
					{
						if (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21))
						{
							AbstractPhysicalObject abstractPhysicalObject = new Pomegranate.AbstractPomegranate(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData, smashed: false, disconnected: false, spearmasterStabbed: false);
							(abstractPhysicalObject as AbstractConsumable).isConsumed = false;
							abstractRoom.entities.Add(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.SpinToy && game.IsStorySession && game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow)
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(world, WatcherEnums.AbstractObjectType.SpinToy, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID());
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.BallToy && game.IsStorySession && game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow)
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(world, WatcherEnums.AbstractObjectType.BallToy, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID());
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.SoftToy && game.IsStorySession && game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow)
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(world, WatcherEnums.AbstractObjectType.SoftToy, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID());
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.WeirdToy && game.IsStorySession && game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow)
					{
						AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(world, WatcherEnums.AbstractObjectType.WeirdToy, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID());
						abstractRoom.entities.Add(abstractPhysicalObject);
					}
					else if (roomSettings.placedObjects[num21].type == PlacedObject.Type.LobeTree)
					{
						AbstractPhysicalObject abstractPhysicalObject = new LobeTree.AbstractLobeTree(world, AbstractPhysicalObject.AbstractObjectType.LobeTree, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), num21);
						abstractRoom.entities.Add(abstractPhysicalObject);
						abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						waitToEnterAfterFullyLoaded = Mathf.Max(waitToEnterAfterFullyLoaded, 40);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == PlacedObject.Type.Rattler && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.Rattler, game))
					{
						if (game.setupValues.worldCreaturesSpawn)
						{
							AbstractPhysicalObject abstractPhysicalObject = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.Rattler), null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID());
							abstractRoom.AddEntity(abstractPhysicalObject);
							abstractPhysicalObject.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
						}
					}
					else if (ModManager.DLCShared && roomSettings.placedObjects[num21].type == DLCSharedEnums.PlacedObjectType.GooieDuck && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21)))
					{
						AbstractPhysicalObject abstractPhysicalObject5 = new AbstractConsumable(world, DLCSharedEnums.AbstractObjectType.GooieDuck, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject5 as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject5);
						abstractPhysicalObject5.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (ModManager.DLCShared && roomSettings.placedObjects[num21].type == DLCSharedEnums.PlacedObjectType.LillyPuck && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21)))
					{
						AbstractPhysicalObject abstractPhysicalObject6 = new LillyPuck.AbstractLillyPuck(world, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), 3, abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject6 as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject6);
						abstractPhysicalObject6.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (ModManager.DLCShared && roomSettings.placedObjects[num21].type == DLCSharedEnums.PlacedObjectType.GlowWeed && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21)))
					{
						AbstractPhysicalObject abstractPhysicalObject7 = new AbstractConsumable(world, DLCSharedEnums.AbstractObjectType.GlowWeed, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject7 as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject7);
						abstractPhysicalObject7.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (ModManager.DLCShared && roomSettings.placedObjects[num21].type == DLCSharedEnums.PlacedObjectType.BigJellyFish && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(DLCSharedEnums.CreatureTemplateType.BigJelly, game))
					{
						Vector2 pos = roomSettings.placedObjects[num21].pos;
						AbstractCreature abstractCreature3 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(DLCSharedEnums.CreatureTemplateType.BigJelly), null, GetWorldCoordinate(pos), new EntityID(-1, abstractRoom.index * 1000 + num21));
						(abstractCreature3.state as BigJellyState).HomePos = pos;
						(abstractCreature3.state as BigJellyState).DriftPos = pos + (roomSettings.placedObjects[num21].data as PlacedObject.ResizableObjectData).handlePos;
						abstractRoom.AddEntity(abstractCreature3);
						abstractCreature3.pos.abstractNode = 0;
						abstractCreature3.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (ModManager.DLCShared && roomSettings.placedObjects[num21].type == DLCSharedEnums.PlacedObjectType.Stowaway && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(DLCSharedEnums.CreatureTemplateType.StowawayBug, game))
					{
						Vector2 pos2 = roomSettings.placedObjects[num21].pos;
						AbstractCreature abstractCreature4 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(DLCSharedEnums.CreatureTemplateType.StowawayBug), null, GetWorldCoordinate(pos2), new EntityID(-1, abstractRoom.index * 1000 + num21));
						(abstractCreature4.state as StowawayBugState).HomePos = pos2;
						(abstractCreature4.state as StowawayBugState).aimPos = pos2 + (roomSettings.placedObjects[num21].data as PlacedObject.ResizableObjectData).handlePos;
						abstractRoom.AddEntity(abstractCreature4);
						abstractCreature4.pos.abstractNode = 0;
						abstractCreature4.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (ModManager.MSC && roomSettings.placedObjects[num21].type == MoreSlugcatsEnums.PlacedObjectType.MoonCloak && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21)))
					{
						Custom.Log("Spawned moon cloak!");
						AbstractPhysicalObject abstractPhysicalObject8 = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.MoonCloak, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject8 as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject8);
						abstractPhysicalObject8.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (ModManager.DLCShared && roomSettings.placedObjects[num21].type == DLCSharedEnums.PlacedObjectType.DandelionPeach && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21)))
					{
						AbstractPhysicalObject abstractPhysicalObject9 = new AbstractConsumable(world, DLCSharedEnums.AbstractObjectType.DandelionPeach, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject9 as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject9);
						abstractPhysicalObject9.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (ModManager.MSC && roomSettings.placedObjects[num21].type == MoreSlugcatsEnums.PlacedObjectType.HRGuard && (!(game.session is StoryGameSession) || !(game.session as StoryGameSession).saveState.ItemConsumed(world, karmaFlower: false, abstractRoom.index, num21)))
					{
						AbstractPhysicalObject abstractPhysicalObject10 = new AbstractConsumable(world, MoreSlugcatsEnums.AbstractObjectType.HRGuard, null, GetWorldCoordinate(roomSettings.placedObjects[num21].pos), game.GetNewID(), abstractRoom.index, num21, roomSettings.placedObjects[num21].data as PlacedObject.ConsumableObjectData);
						(abstractPhysicalObject10 as AbstractConsumable).isConsumed = false;
						abstractRoom.entities.Add(abstractPhysicalObject10);
						abstractPhysicalObject10.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, num21);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.PlacedBoxWorm && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.BoxWorm, game))
					{
						Vector2 pos3 = roomSettings.placedObjects[num21].pos;
						AbstractCreature abstractCreature5 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.BoxWorm), null, GetWorldCoordinate(pos3), new EntityID(-1, abstractRoom.index * 1000 + num21));
						abstractRoom.AddEntity(abstractCreature5);
						abstractCreature5.pos.abstractNode = 0;
						AssignOriginAndIteration(abstractCreature5, num21);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.PlacedFireSprite && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.FireSprite, game))
					{
						Vector2 pos4 = roomSettings.placedObjects[num21].pos;
						AbstractCreature abstractCreature6 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.FireSprite), null, GetWorldCoordinate(pos4), new EntityID(-1, abstractRoom.index * 1000 + num21));
						abstractRoom.AddEntity(abstractCreature6);
						abstractCreature6.pos.abstractNode = 0;
						AssignOriginAndIteration(abstractCreature6, num21);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.PlacedDrillCrab && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.DrillCrab, game))
					{
						Vector2 pos5 = roomSettings.placedObjects[num21].pos;
						AbstractCreature abstractCreature7 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.DrillCrab), null, GetWorldCoordinate(pos5), new EntityID(-1, abstractRoom.index * 1000 + num21));
						abstractRoom.AddEntity(abstractCreature7);
						abstractCreature7.pos.abstractNode = 0;
						AssignOriginAndIteration(abstractCreature7, num21);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.PlacedRats && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.Rat, game))
					{
						for (int num23 = 0; num23 < UnityEngine.Random.Range(1, 4); num23++)
						{
							Vector2 pos6 = roomSettings.placedObjects[num21].pos + Custom.RNV() * 20f;
							AbstractCreature abstractCreature8 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.Rat), null, GetWorldCoordinate(pos6), new EntityID(-1, abstractRoom.index * 1000 + num21));
							abstractRoom.AddEntity(abstractCreature8);
							abstractCreature8.pos.abstractNode = 0;
							AssignOriginAndIteration(abstractCreature8, num21);
						}
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.PlacedFrog && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.Frog, game))
					{
						Vector2 pos7 = roomSettings.placedObjects[num21].pos;
						AbstractCreature abstractCreature9 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.Frog), null, GetWorldCoordinate(pos7), new EntityID(-1, abstractRoom.index * 1000 + num21));
						abstractRoom.AddEntity(abstractCreature9);
						abstractCreature9.pos.abstractNode = 0;
						AssignOriginAndIteration(abstractCreature9, num21);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.PlacedLoach && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.Loach, game))
					{
						Vector2 pos8 = roomSettings.placedObjects[num21].pos;
						AbstractCreature abstractCreature10 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.Loach), null, GetWorldCoordinate(pos8), new EntityID(-1, abstractRoom.index * 1000 + num21));
						abstractRoom.AddEntity(abstractCreature10);
						abstractCreature10.pos.abstractNode = 0;
						AssignOriginAndIteration(abstractCreature10, num21);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.PlacedRotLoach && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.RotLoach, game))
					{
						Vector2 pos9 = roomSettings.placedObjects[num21].pos;
						AbstractCreature abstractCreature11 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.RotLoach), null, GetWorldCoordinate(pos9), new EntityID(-1, abstractRoom.index * 1000 + num21));
						abstractRoom.AddEntity(abstractCreature11);
						abstractCreature11.pos.abstractNode = 0;
						AssignOriginAndIteration(abstractCreature11, num21);
					}
					else if (ModManager.Watcher && roomSettings.placedObjects[num21].type == WatcherEnums.PlacedObjectType.PlacedBarnacles && MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.Barnacle, game))
					{
						for (int num24 = 0; num24 < UnityEngine.Random.Range(2, 6); num24++)
						{
							Vector2 pos10 = roomSettings.placedObjects[num21].pos + Custom.RNV() * 40f;
							AbstractCreature abstractCreature12 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.Barnacle), null, GetWorldCoordinate(pos10), new EntityID(-1, abstractRoom.index * 1000 + num21));
							abstractRoom.AddEntity(abstractCreature12);
							abstractCreature12.pos.abstractNode = 0;
							AssignOriginAndIteration(abstractCreature12, num21);
						}
					}
				}
			}
			if (!abstractRoom.shelter && !abstractRoom.gate && game != null && (!game.IsArenaSession || game.GetArenaGameSession.GameTypeSetup.levelItems) && (!ModManager.MMF || roomSettings.RandomItemDensity > 0f))
			{
				for (int num25 = (int)((float)TileWidth * (float)TileHeight * Mathf.Pow(roomSettings.RandomItemDensity, 2f) / 5f); num25 >= 0; num25--)
				{
					IntVector2 intVector = RandomTile();
					if (!GetTile(intVector).Solid)
					{
						bool flag7 = true;
						if (!ModManager.MMF || roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.ZeroG) < 1f || roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f)
						{
							for (int num26 = -1; num26 < 2; num26++)
							{
								if (!GetTile(intVector + new IntVector2(num26, -1)).Solid)
								{
									flag7 = false;
									break;
								}
								if (ModManager.MMF && GetTile(intVector).Terrain == Tile.TerrainType.Slope && GetTile(intVector + new IntVector2(0, 1)).Solid)
								{
									flag7 = false;
									break;
								}
							}
						}
						else if (ModManager.MMF)
						{
							bool flag8 = false;
							for (int num27 = -1; num27 < 2; num27++)
							{
								if (GetTile(intVector + new IntVector2(num27, 0)).Solid)
								{
									flag8 = true;
									break;
								}
							}
							bool flag9 = false;
							for (int num28 = -1; num28 < 2; num28++)
							{
								if (GetTile(intVector + new IntVector2(0, num28)).Solid)
								{
									flag9 = true;
									break;
								}
							}
							if (flag8 && flag9)
							{
								flag7 = false;
							}
							else if (!(flag8 ^ flag9) && UnityEngine.Random.value > 0.1f)
							{
								flag7 = false;
							}
						}
						if (flag7)
						{
							EntityID newID3 = game.GetNewID(-abstractRoom.index);
							AbstractPhysicalObject abstractPhysicalObject11;
							if (game != null && UnityEngine.Random.value < SlugcatStats.SpearSpawnModifier(game.TimelinePoint, roomSettings.RandomItemSpearChance))
							{
								abstractPhysicalObject11 = new AbstractSpear(world, null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), newID3, UnityEngine.Random.value < SlugcatStats.SpearSpawnExplosiveRandomChance(game.TimelinePoint));
								if (ModManager.DLCShared && (abstractPhysicalObject11 as AbstractSpear).explosive && UnityEngine.Random.value < SlugcatStats.SpearSpawnElectricRandomChance(game.TimelinePoint))
								{
									(abstractPhysicalObject11 as AbstractSpear).explosive = false;
									(abstractPhysicalObject11 as AbstractSpear).electric = true;
								}
							}
							else
							{
								abstractPhysicalObject11 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), newID3);
							}
							abstractRoom.AddEntity(abstractPhysicalObject11);
						}
					}
				}
			}
			if (ModManager.Expedition && game.rainWorld.ExpeditionMode && abstractRoom.shelter && abstractRoom.name == ExpeditionData.startingDen && game.rainWorld.progression.currentSaveState.cycleNumber == 0 && game.world.rainCycle.CycleProgression <= 0f)
			{
				WorldCoordinate pos11 = new WorldCoordinate(abstractRoom.index, shelterDoor.playerSpawnPos.x, shelterDoor.playerSpawnPos.y, 0);
				if (ExpeditionData.activeMission == "mis2" && world.region.name == "SS")
				{
					AbstractPhysicalObject abstractPhysicalObject12 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, null, pos11, game.GetNewID());
					abstractRoom.entities.Add(abstractPhysicalObject12);
					abstractPhysicalObject12.Realize();
				}
				for (int num29 = 0; num29 < ExpeditionGame.activeUnlocks.Count; num29++)
				{
					string text = ExpeditionGame.activeUnlocks[num29];
					if (text == "unl-lantern")
					{
						AbstractPhysicalObject abstractPhysicalObject13 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, pos11, game.GetNewID());
						abstractRoom.entities.Add(abstractPhysicalObject13);
						abstractPhysicalObject13.Realize();
					}
					if (text == "unl-bomb")
					{
						AbstractPhysicalObject abstractPhysicalObject14 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, pos11, game.GetNewID());
						abstractRoom.entities.Add(abstractPhysicalObject14);
						abstractPhysicalObject14.Realize();
					}
					if (text == "unl-vulture")
					{
						bool king = UnityEngine.Random.value > 0.95f;
						AbstractPhysicalObject abstractPhysicalObject15 = new VultureMask.AbstractVultureMask(world, null, pos11, game.GetNewID(), Mathf.RoundToInt(UnityEngine.Random.Range(0f, 100f)), king);
						abstractRoom.entities.Add(abstractPhysicalObject15);
						abstractPhysicalObject15.Realize();
					}
					if (ModManager.MSC && text == "unl-gun")
					{
						JokeRifle.AbstractRifle abstractRifle = new JokeRifle.AbstractRifle(game.world, null, pos11, game.GetNewID(), JokeRifle.AbstractRifle.AmmoType.Rock);
						abstractRoom.entities.Add(abstractRifle);
						abstractRifle.Realize();
					}
					if (ModManager.DLCShared)
					{
						if (text == "unl-electric")
						{
							AbstractSpear abstractSpear2 = new AbstractSpear(world, null, pos11, game.GetNewID(), explosive: false);
							abstractSpear2.electric = true;
							abstractSpear2.electricCharge = 10;
							abstractRoom.entities.Add(abstractSpear2);
							abstractSpear2.Realize();
						}
						if (text == "unl-sing")
						{
							AbstractPhysicalObject abstractPhysicalObject16 = new AbstractPhysicalObject(world, DLCSharedEnums.AbstractObjectType.SingularityBomb, null, pos11, game.GetNewID());
							abstractRoom.entities.Add(abstractPhysicalObject16);
							abstractPhysicalObject16.Realize();
						}
					}
				}
			}
		}
		abstractRoom.firstTimeRealized = false;
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode && updateList != null && updateList.Count > 0)
		{
			if (abstractRoom.name == "SB_A14" || abstractRoom.name == "SB_E05SAINT")
			{
				bool flag10 = false;
				for (int num30 = 0; num30 < updateList.Count; num30++)
				{
					if (updateList[num30] is DepthsFinishScript)
					{
						flag10 = true;
					}
				}
				if (!flag10)
				{
					AddObject(new DepthsFinishScript(this));
				}
			}
			for (int num31 = 0; num31 < updateList.Count; num31++)
			{
				if (ExpeditionGame.IsMSCRoomScript(updateList[num31]) || ExpeditionGame.IsUndesirableRoomScript(updateList[num31]))
				{
					ExpLog.Log("Remove MSCRoomSpecificScript: " + updateList[num31].ToString());
					updateList[num31].Destroy();
				}
			}
		}
		for (int num32 = 0; num32 < roomSettings.triggers.Count; num32++)
		{
			if (!(game.session is StoryGameSession) || ((game.session as StoryGameSession).saveState.cycleNumber >= roomSettings.triggers[num32].activeFromCycle && (game.StoryCharacter == null || roomSettings.triggers[num32].timelines.Contains(game.TimelinePoint)) && (roomSettings.triggers[num32].activeToCycle < 0 || (game.session as StoryGameSession).saveState.cycleNumber <= roomSettings.triggers[num32].activeToCycle)))
			{
				AddObject(new ActiveTriggerChecker(roomSettings.triggers[num32]));
			}
		}
		if (world.rainCycle.CycleStartUp < 1f && roomSettings.CeilingDrips > 0f && roomSettings.DangerType != RoomRain.DangerType.None && !abstractRoom.shelter)
		{
			AddObject(new DrippingSound());
		}
	}

	public bool WarpingToThisRoom()
	{
		WarpPoint.WarpPointTimer warpPointTimer = game.cameras[0].warpPointTimer;
		if (warpPointTimer != null)
		{
			WarpPoint origin_warpPoint = warpPointTimer.origin_warpPoint;
			if (origin_warpPoint != null && origin_warpPoint.Data.destRoom != null)
			{
				return origin_warpPoint.Data.destRoom.ToLower() == abstractRoom.name.ToLower();
			}
		}
		return false;
	}

	public void AssignOriginAndIteration(AbstractPhysicalObject obj, int iterator)
	{
		obj.placedObjectOrigin = SetAbstractRoomAndPlacedObjectNumber(abstractRoom.name, iterator);
	}

	public void LoadTerrainCurve()
	{
		foreach (PlacedObject placedObject in roomSettings.placedObjects)
		{
			if (placedObject.type == PlacedObject.Type.TerrainHandle)
			{
				if (terrain == null)
				{
					terrain = new TerrainManager();
				}
				if (terrain.roomTerrain == null)
				{
					TerrainCurve terrainCurve = new TerrainCurve(this);
					AddObject(terrainCurve);
					terrain.roomTerrain = terrainCurve;
				}
			}
			else if (placedObject.type == PlacedObject.Type.LocalTerrain)
			{
				AddObject(new LocalTerrainCurve(this, placedObject.data as PlacedObject.LocalTerrainData));
			}
			else if (placedObject.type == PlacedObject.Type.CurvedSlope)
			{
				AddObject(new CurvedSlope(this, placedObject.data as PlacedObject.LocalTerrainData));
			}
			else if (placedObject.type == PlacedObject.Type.SuperSlope)
			{
				AddObject(new SuperSlope(this, placedObject.data as PlacedObject.SuperSlopeData));
			}
		}
	}

	public bool CheckForWarpedObjects(int iterator)
	{
		if (game.IsStorySession && game.GetStorySession.warpedPlacedObjects.ContainsKey(abstractRoom.name))
		{
			return game.GetStorySession.warpedPlacedObjects[abstractRoom.name].Contains(iterator);
		}
		return false;
	}

	public string SetAbstractRoomAndPlacedObjectNumber(string roomName, int iterator)
	{
		return roomName + ":" + iterator;
	}

	public void SpawnPrinceBulb(PlacedObject pObj)
	{
		int val = game.GetStorySession?.saveState.miscWorldSaveData.numberOfPrinceEncounters ?? AbstractPrinceBulb.maxGrowthStage;
		val = Math.Min(val, AbstractPrinceBulb.maxGrowthStage);
		float t = (float)val / (float)AbstractPrinceBulb.maxGrowthStage;
		Vector2 normalized = (pObj.data as PlacedObject.ResizableObjectData).handlePos.normalized;
		float rad = (pObj.data as PlacedObject.ResizableObjectData).Rad;
		float radius = rad * Mathf.Lerp(0.5f, 1f, t);
		Vector2 pos = pObj.pos;
		Vector2 pos2 = pos - normalized * (rad * Mathf.Lerp(0.5f, 0f, t));
		Vector2 pos3 = pos - normalized * (rad * 0.75f);
		DaddyCorruption daddyCorruption = updateList.OfType<DaddyCorruption>().FirstOrDefault();
		if (daddyCorruption == null)
		{
			daddyCorruption = new DaddyCorruption(this);
			AddObject(daddyCorruption);
		}
		PlacedObject.ResizableObjectData data = new PlacedObject.ResizableObjectData(null)
		{
			handlePos = new Vector2(150f + (float)val * 50f, 0f)
		};
		PlacedObject item = new PlacedObject(PlacedObject.Type.Corruption, data)
		{
			pos = pos3
		};
		daddyCorruption.places.Add(item);
		WorldCoordinate worldCoordinate = GetWorldCoordinate(pos2);
		PrinceBulb obj = new PrinceBulb(new AbstractPrinceBulb(world, null, worldCoordinate, game.GetNewID(), val, normalized, radius));
		AddObject(obj);
		waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 80);
	}

	public void Unloaded()
	{
		Futile.atlasManager.UnloadAtlas("RainMask_" + abstractRoom.name);
		foreach (UpdatableAndDeletable update in updateList)
		{
			(update as INotifyWhenRoomUnloaded)?.RoomUnloaded();
		}
	}

	public void NowViewed()
	{
		if (snowObject == null)
		{
			Shader.DisableKeyword("SNOW_ON");
		}
		Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 0f);
		if (Region.IsRubiconRegion(world.name) || roomSettings.GetEffect(RoomSettings.RoomEffect.Type.LavaSurface) != null)
		{
			Shader.EnableKeyword("HR");
		}
		else
		{
			Shader.DisableKeyword("HR");
		}
		Shader.DisableKeyword("URBANLIFE");
		Shader.SetGlobalColor("_boxWormColor", BoxWormGraphics.BaseColor(this));
		if (fsRipple == null)
		{
			if (game.cameras[0].lastRippleState)
			{
				AddObject(fsRipple = new RippleFullScreen());
			}
		}
		else if (!game.cameras[0].lastRippleState)
		{
			fsRipple.Destroy();
			fsRipple = null;
		}
		for (int i = 0; i < roomSettings.effects.Count; i++)
		{
			if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.GreenSparks)
			{
				for (int j = 0; (float)j < (float)(TileWidth * TileHeight) * roomSettings.effects[i].amount / 50f; j++)
				{
					Vector2 pos = new Vector2(UnityEngine.Random.value * PixelWidth, UnityEngine.Random.value * PixelHeight);
					if (!GetTile(pos).Solid && Mathf.Pow(UnityEngine.Random.value, 1f - roomSettings.effects[i].amount) > (float)(readyForAI ? aimap.getTerrainProximity(pos) : 5) * 0.05f && roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.GreenSparks)
					{
						AddObject(new GreenSparks.GreenSpark(pos));
					}
				}
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.ZeroGSpecks)
			{
				for (int k = 0; (float)k < 1000f * roomSettings.effects[i].amount; k++)
				{
					AddObject(new BlinkSpeck());
				}
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.CorruptionSpores)
			{
				for (int l = 0; (float)l < 200f * roomSettings.effects[i].amount; l++)
				{
					AddObject(new CorruptionSpore());
				}
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.SuperStructureProjector)
			{
				AddObject(new SuperStructureProjector(this, roomSettings.effects[i]));
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.ProjectedScanLines)
			{
				AddObject(new ProjectedScanLines(this, roomSettings.effects[i]));
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.AboveCloudsView)
			{
				Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.RoofTopView)
			{
				Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
			}
			else if (ModManager.Watcher && (roomSettings.effects[i].type == WatcherEnums.RoomEffectType.OuterRimView || roomSettings.effects[i].type == WatcherEnums.RoomEffectType.AncientUrbanView || roomSettings.effects[i].type == WatcherEnums.RoomEffectType.InnerOuterRimView))
			{
				Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.PinkSky)
			{
				Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
			}
			else if (roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.FairyParticles)
			{
				float num = 1f;
				if (ModManager.Watcher && game.IsStorySession && game.GetStorySession.saveStateNumber == WatcherEnums.SlugcatStatsName.Watcher)
				{
					num = 1f - global::Watcher.Watcher.cfgReduceFallingParticles.Value;
				}
				for (int m = 0; (float)m < 500f * roomSettings.effects[i].amount * num; m++)
				{
					int num_keyframes = ((UnityEngine.Random.value < 0.5f) ? 3 : 4);
					FairyParticle obj = new FairyParticle(UnityEngine.Random.Range(0, 360), num_keyframes, 60f, 180f, 40f, 100f, 5f, 30f);
					AddObject(obj);
				}
				for (int n = 0; n < roomSettings.placedObjects.Count; n++)
				{
					if (roomSettings.placedObjects[n].type == PlacedObject.Type.FairyParticleSettings)
					{
						(roomSettings.placedObjects[n].data as PlacedObject.FairyParticleData).Apply(this);
						break;
					}
				}
			}
			else
			{
				if (!(roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.DayNight))
				{
					continue;
				}
				for (int num2 = 0; num2 < roomSettings.placedObjects.Count; num2++)
				{
					if (roomSettings.placedObjects[num2].type == PlacedObject.Type.DayNightSettings)
					{
						(roomSettings.placedObjects[num2].data as PlacedObject.DayNightData).Apply(this);
						break;
					}
				}
			}
		}
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("bur-blinded"))
		{
			PlacedObject.DayNightData dayNightData = new PlacedObject.DayNightData(null);
			dayNightData.nightPalette = 10;
			dayNightData.Apply(this);
			if (game.cameras[0].currentPalette.darkness < 0.8f)
			{
				game.cameras[0].effect_dayNight = 1f;
				game.cameras[0].currentPalette.darkness = 0.8f;
			}
			roomSettings.Clouds = 0.875f;
			world.rainCycle.sunDownStartTime = 0;
			world.rainCycle.dayNightCounter = 3750;
		}
		for (int num3 = 0; num3 < physicalObjects.Length; num3++)
		{
			for (int num4 = 0; num4 < physicalObjects[num3].Count; num4++)
			{
				physicalObjects[num3][num4].InitiateGraphicsModule();
				if (physicalObjects[num3][num4].graphicsModule != null && !drawableObjects.Contains(physicalObjects[num3][num4].graphicsModule))
				{
					drawableObjects.Add(physicalObjects[num3][num4].graphicsModule);
				}
			}
		}
		if (world.worldGhost != null)
		{
			for (int num5 = 0; num5 < cameraPositions.Length; num5++)
			{
				if (world.worldGhost.GhostMode(this, num5) > 0f && (!ModManager.Watcher || world.worldGhost.ghostID != WatcherEnums.GhostID.SpinningTop))
				{
					AddObject(new GoldFlakes(this));
					break;
				}
			}
		}
		if (insectCoordinator != null)
		{
			insectCoordinator.NowViewed();
		}
	}

	public void NoLongerViewed()
	{
		for (int num = drawableObjects.Count - 1; num >= 0; num--)
		{
			if (drawableObjects[num] is GraphicsModule)
			{
				(drawableObjects[num] as GraphicsModule).owner.RemoveGraphicsModule();
				drawableObjects.RemoveAt(num);
			}
		}
		if (insectCoordinator != null)
		{
			insectCoordinator.NoLongerViewed();
		}
		blizzardGraphics = null;
		watcherCamoTaken = false;
	}

	public void BlinkShortCut(int shortcut, int secondary, float blinkFac)
	{
		if (secondary > -1)
		{
			shortcutsBlinking[secondary, 0] = 1f;
			if (shortcutsBlinking[secondary, 1] == 0f)
			{
				shortcutsBlinking[secondary, 1] = 0.01f;
			}
			shortcutsBlinking[secondary, 3] = -5f * blinkFac;
		}
		if (shortcut > -1)
		{
			shortcutsBlinking[shortcut, 3] = -20f * blinkFac;
		}
	}

	public void ShortCutsReady()
	{
		shortcutsBlinking = new float[shortcuts.Length, 4];
		if (game != null)
		{
			foreach (AbstractWorldEntity entity in abstractRoom.entities)
			{
				if (entity is AbstractCreature)
				{
					continue;
				}
				bool flag = true;
				if (entity is AbstractPhysicalObject)
				{
					List<AbstractPhysicalObject> allConnectedObjects = (entity as AbstractPhysicalObject).GetAllConnectedObjects();
					for (int i = 0; i < allConnectedObjects.Count && flag; i++)
					{
						if (allConnectedObjects[i] is AbstractCreature)
						{
							flag = false;
						}
					}
				}
				if (!entity.InDen && flag)
				{
					if (entity.pos.room == abstractRoom.index)
					{
						(entity as AbstractPhysicalObject).RealizeInRoom();
						continue;
					}
					Custom.LogWarning($"Object in wrong room! {entity.ID}");
				}
			}
		}
		bool flag2 = false;
		foreach (AbstractCreature creature in abstractRoom.creatures)
		{
			if (!creature.InDen && creature.creatureTemplate.requireAImap)
			{
				flag2 = true;
				break;
			}
		}
		if (abstractRoom.quantifiedCreatures != null)
		{
			for (int j = 0; j < StaticWorld.quantifiedCreatures.Length; j++)
			{
				if (StaticWorld.quantifiedCreatures[j].requireAImap && abstractRoom.NumberOfQuantifiedCreatureInRoom(StaticWorld.quantifiedCreatures[j].type) > 0)
				{
					flag2 = true;
					break;
				}
			}
		}
		loadingProgress = 1;
		if (!flag2)
		{
			readyForNonAICreaturesToEnter = true;
			if (game != null)
			{
				foreach (AbstractCreature creature2 in abstractRoom.creatures)
				{
					if (creature2.realizedCreature == null && creature2.AllowedToExistInRoom(this))
					{
						creature2.RealizeInRoom();
					}
				}
			}
		}
		for (int k = 0; k < updateList.Count; k++)
		{
			if (updateList[k] is INotifyWhenRoomIsReady)
			{
				(updateList[k] as INotifyWhenRoomIsReady).ShortcutsReady();
			}
		}
		AddObject(new ShortcutHelper(this));
	}

	public WebChain AddRippleSpiderWeb(WorldCoordinate from, WorldCoordinate to)
	{
		IntVector2 start = (from.TileDefined ? from.Tile : LocalCoordinateOfNode(from.abstractNode).Tile);
		IntVector2 goal = (to.TileDefined ? to.Tile : LocalCoordinateOfNode(to.abstractNode).Tile);
		QuickPathFinder quickPathFinder = new QuickPathFinder(start, goal, aimap, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.RippleSpider));
		while (quickPathFinder.status == 0)
		{
			quickPathFinder.Update();
		}
		if (quickPathFinder.status != 1)
		{
			return null;
		}
		return AddRippleSpiderWeb(quickPathFinder.ReturnPath().tiles);
	}

	public WebChain AddRippleSpiderWeb(IntVector2[] tiles)
	{
		List<Vector2> path = new List<Vector2>();
		foreach (IntVector2 pos in tiles)
		{
			if (GetTile(pos).Terrain != Tile.TerrainType.Slope)
			{
				path.Add(MiddleOfTile(pos));
			}
		}
		if (path.Count <= 1)
		{
			return null;
		}
		WebChain webChain = new WebChain();
		AddObject(webChain);
		float[] distances = new float[path.Count];
		for (int j = 1; j < path.Count; j++)
		{
			distances[j] = distances[j - 1] + Vector2.Distance(path[j - 1], path[j]);
		}
		float num = distances[path.Count - 1];
		int num2 = Mathf.Max(Mathf.RoundToInt(num / 225f), 1);
		float num3 = num / (float)num2;
		for (int k = 0; k < num2; k++)
		{
			float num4 = num3 * (float)(k + 1);
			float num5 = num3 * (float)k;
			CosmicWeb cosmicWeb = new CosmicWeb(this, num3, SamplePath(num4), SamplePath(num5), stuckAtA: true, stuckAtB: true);
			for (int l = 0; l < cosmicWeb.segments.GetLength(0); l++)
			{
				float t = (float)l / (float)(cosmicWeb.segments.Length - 1);
				cosmicWeb.segments[l, 0] = SamplePath(Mathf.Lerp(num4, num5, t));
			}
			webChain.cosmicWebs.Add(cosmicWeb);
			AddObject(cosmicWeb);
		}
		return webChain;
		Vector2 SamplePath(float dist)
		{
			for (int m = 0; m < path.Count - 1; m++)
			{
				float num6 = distances[m];
				float num7 = distances[m + 1];
				if (num6 <= dist && dist <= num7)
				{
					return Vector2.Lerp(path[m], path[m + 1], (dist - num6) / (num7 - num6));
				}
			}
			return path[path.Count - 1];
		}
	}

	public void ReadyForAI()
	{
		loadingProgress = 2;
		if (game != null)
		{
			foreach (AbstractCreature creature in abstractRoom.creatures)
			{
				if (creature.realizedCreature == null && creature.AllowedToExistInRoom(this))
				{
					creature.RealizeInRoom();
				}
			}
		}
		if (abstractRoom.quantifiedCreatures != null && game != null)
		{
			for (int i = 0; i < StaticWorld.quantifiedCreatures.Length; i++)
			{
				if (abstractRoom.NumberOfQuantifiedCreatureInRoom(StaticWorld.quantifiedCreatures[i].type) > 0)
				{
					PlaceQuantifiedCreaturesInRoom(StaticWorld.quantifiedCreatures[i].type);
				}
			}
		}
		loadingProgress = 3;
		readyForNonAICreaturesToEnter = true;
		for (int j = 0; j < updateList.Count; j++)
		{
			if (updateList[j] is INotifyWhenRoomIsReady)
			{
				(updateList[j] as INotifyWhenRoomIsReady).AIMapReady();
			}
		}
		bool flag = ModManager.MSC && game != null && ((game.IsStorySession && (abstractRoom.name == "DM_AI" || abstractRoom.name == "CL_AI" || abstractRoom.name == "RM_AI" || abstractRoom.name == "HR_AI")) || abstractRoom.name == "Multi_SL_AI");
		if ((game != null && game.IsStorySession && (abstractRoom.name == "SS_AI" || abstractRoom.name == "SL_AI")) || flag)
		{
			if (ModManager.MSC && Region.IsRubiconRegion(world.name))
			{
				if (game.GetStorySession.saveState.deathPersistentSaveData.ripPebbles && !game.GetStorySession.saveState.miscWorldSaveData.hrMelted)
				{
					oracleWantToSpawn = Oracle.OracleID.SS;
					Oracle obj = new Oracle(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Oracle, null, new WorldCoordinate(abstractRoom.index, 15, 15, -1), game.GetNewID()), this);
					AddObject(obj);
				}
				if (game.GetStorySession.saveState.deathPersistentSaveData.ripMoon && !game.GetStorySession.saveState.miscWorldSaveData.hrMelted)
				{
					oracleWantToSpawn = MoreSlugcatsEnums.OracleID.DM;
					Oracle obj2 = new Oracle(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Oracle, null, new WorldCoordinate(abstractRoom.index, 15, 15, -1), game.GetNewID()), this);
					AddObject(obj2);
				}
			}
			else
			{
				Oracle obj3 = new Oracle(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Oracle, null, new WorldCoordinate(abstractRoom.index, 15, 15, -1), game.GetNewID()), this);
				AddObject(obj3);
			}
			waitToEnterAfterFullyLoaded = Math.Max(waitToEnterAfterFullyLoaded, 80);
		}
		if (world.regionState == null || !world.regionState.rippleSpiderWebs.TryGetValue(abstractRoom.name, out List<(WorldCoordinate, WorldCoordinate)> value))
		{
			return;
		}
		foreach (var (from, to) in value)
		{
			AddRippleSpiderWeb(from, to);
		}
	}

	public void LoadWarpsFromSaveState()
	{
		if (world.regionState != null)
		{
			foreach (KeyValuePair<string, string> spawnedRippleRegion in world.regionState.spawnedRippleRegions)
			{
				if (CosmeticRipple.RoomFromIdentifyingString(spawnedRippleRegion.Key) == abstractRoom.name)
				{
					PlacedObject placedObject = new PlacedObject(PlacedObject.Type.CosmeticRipple, null);
					string[] s = Regex.Split(spawnedRippleRegion.Value.Trim(), "><");
					(placedObject.data as CosmeticRippleData).owner.FromString(s);
					(placedObject.data as CosmeticRippleData).animateOnSpawn = false;
					TrySpawnRippleRegion(placedObject, saveInRegionState: false, skipIfInRegionState: false);
				}
			}
		}
		if (!game.IsStorySession)
		{
			return;
		}
		foreach (KeyValuePair<string, string> discoveredWarpPoint in game.GetStorySession.saveState.miscWorldSaveData.discoveredWarpPoints)
		{
			if (WarpPoint.RoomFromIdentifyingString(discoveredWarpPoint.Key) == abstractRoom.name)
			{
				PlacedObject placedObject2 = new PlacedObject(PlacedObject.Type.WarpPoint, null);
				string[] s2 = Regex.Split(discoveredWarpPoint.Value.Trim(), "><");
				(placedObject2.data as WarpPoint.WarpPointData).owner.FromString(s2);
				TrySpawnWarpPoint(placedObject2, saveInRegionState: false);
			}
		}
		foreach (KeyValuePair<string, string> spawnedWarpPoint in game.GetStorySession.saveState.deathPersistentSaveData.spawnedWarpPoints)
		{
			if (!game.GetStorySession.saveState.miscWorldSaveData.discoveredWarpPoints.ContainsKey(spawnedWarpPoint.Key) && WarpPoint.RoomFromIdentifyingString(spawnedWarpPoint.Key) == abstractRoom.name)
			{
				PlacedObject placedObject3 = new PlacedObject(PlacedObject.Type.WarpPoint, null);
				string[] s3 = Regex.Split(spawnedWarpPoint.Value.Trim(), "><");
				(placedObject3.data as WarpPoint.WarpPointData).owner.FromString(s3);
				TrySpawnWarpPoint(placedObject3, saveInRegionState: false);
			}
		}
	}

	[Obsolete("use 2 parameter method instead")]
	public WarpPoint TrySpawnWarpPoint(PlacedObject po, bool saveInRegionState, bool skipIfInRegionState, bool deathPersistent)
	{
		if (deathPersistent)
		{
			(po.data as WarpPoint.WarpPointData).deathPersistentWarpPoint = true;
		}
		return TrySpawnWarpPoint(po, saveInRegionState);
	}

	public WarpPoint TrySpawnWarpPoint(PlacedObject po, bool saveInRegionState = true)
	{
		WarpPoint.WarpPointData warpPointData = po.data as WarpPoint.WarpPointData;
		string text = WarpPoint.IdentifyingString(game, warpPointData, abstractRoom);
		foreach (WarpPoint warpPoint in warpPoints)
		{
			if (warpPoint.MyIdentifyingString() == text && warpPoint.Data.destRoom?.ToLowerInvariant() == warpPointData.destRoom?.ToLowerInvariant())
			{
				return warpPoint;
			}
			if (warpPoint.Data.destRoom?.ToLowerInvariant() == warpPointData.destRoom?.ToLowerInvariant())
			{
				return warpPoint;
			}
		}
		if ((warpPointData.nonDynamicWarpPoint || warpPointData.wasNonDynamicWarpBeforeWeaverTriggered) && !warpPointData.oneWayExit && !warpPointData.UpToDateWithIndexMaps(abstractRoom.name.ToLowerInvariant()))
		{
			bool flag = false;
			for (int i = 0; i < roomSettings.placedObjects.Count; i++)
			{
				if (roomSettings.placedObjects[i].type == PlacedObject.Type.WarpPoint && (roomSettings.placedObjects[i].data as WarpPoint.WarpPointData).destRoom == warpPointData.destRoom)
				{
					flag = true;
					break;
				}
				if (roomSettings.placedObjects[i].type == WatcherEnums.PlacedObjectType.SpinningTopSpot && (roomSettings.placedObjects[i].data as SpinningTopData).destRoom == warpPointData.destRoom)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return null;
			}
		}
		return ForceSpawnWarpPoint(po, saveInRegionState);
	}

	public WarpPoint ForceSpawnWarpPoint(PlacedObject po, bool saveInRegionState = true)
	{
		WarpPoint.WarpPointData warpPointData = po.data as WarpPoint.WarpPointData;
		string key = WarpPoint.IdentifyingString(game, warpPointData, abstractRoom);
		SaveState saveState = (game.IsStorySession ? game.GetStorySession.saveState : null);
		if (game.IsStorySession)
		{
			if (game.GetStorySession.finalWarpSequenceStarted)
			{
				saveInRegionState = false;
			}
			if (game.GetStorySession.saveState.deathPersistentSaveData.spawnedWarpPoints.Count == 0 && game.GetStorySession.saveState.miscWorldSaveData.discoveredWarpPoints.Count == 0)
			{
				Custom.rainWorld.progression.miscProgressionData.beaten_Watcher_Tutorial = true;
			}
			if (saveInRegionState)
			{
				if (warpPointData.deathPersistentWarpPoint)
				{
					if (!saveState.deathPersistentSaveData.spawnedWarpPoints.ContainsKey(key))
					{
						saveState.deathPersistentSaveData.newlyDiscoveredWarpPoints[key] = po.data.owner.ToString();
					}
					saveState.deathPersistentSaveData.spawnedWarpPoints[key] = po.data.owner.ToString();
				}
				else
				{
					if (!saveState.miscWorldSaveData.discoveredWarpPoints.ContainsKey(key))
					{
						saveState.deathPersistentSaveData.newlyDiscoveredWarpPoints[key] = po.data.owner.ToString();
					}
					saveState.miscWorldSaveData.discoveredWarpPoints[key] = po.data.owner.ToString();
				}
			}
		}
		WarpPoint warpPoint = new WarpPoint(this, po);
		for (int num = warpPoints.Count - 1; num >= 0; num--)
		{
			WarpPoint warpPoint2 = warpPoints[num];
			if (warpPoint2.Data.dynamicWarpPoint && Vector2.Distance(warpPoint.pos, warpPoint2.pos) < warpPoint.activationRadius + warpPoint2.activationRadius)
			{
				if (saveInRegionState)
				{
					warpPoint2.RemoveFromSaveData();
				}
				warpPoint2.Destroy();
			}
		}
		AddObject(warpPoint);
		return warpPoint;
	}

	public WarpPoint CheckForSpawnedWarp(WarpPoint.WarpPointData data)
	{
		foreach (UpdatableAndDeletable update in updateList)
		{
			if (update is WarpPoint && (update as WarpPoint).MyIdentifyingString() == WarpPoint.IdentifyingString(game, data, abstractRoom))
			{
				return (WarpPoint)update;
			}
		}
		return null;
	}

	public CosmeticRipple TrySpawnRippleRegion(PlacedObject po, bool saveInRegionState, bool skipIfInRegionState, bool changeRippleState = true, string maskShaderOverride = null)
	{
		string text = CosmeticRipple.IdentifyingString(po.data as CosmeticRippleData, abstractRoom);
		if (skipIfInRegionState && world.regionState != null && world.regionState.spawnedRippleRegions.ContainsKey(text))
		{
			return null;
		}
		foreach (UpdatableAndDeletable update in updateList)
		{
			if (skipIfInRegionState && update is CosmeticRipple && (update as CosmeticRipple).MyIdentifyingString() == text)
			{
				return null;
			}
		}
		if (saveInRegionState && world.regionState != null)
		{
			world.regionState.spawnedRippleRegions[text] = po.data.owner.ToString();
		}
		CosmeticRipple cosmeticRipple = new CosmeticRipple(po, maskShaderOverride);
		(po.data as CosmeticRippleData).obj = cosmeticRipple;
		cosmeticRipples.Add(cosmeticRipple);
		AddObject(cosmeticRipple);
		if (changeRippleState)
		{
			ripple = true;
		}
		return cosmeticRipple;
	}

	public void InitializeWeaverPresenceInRoom()
	{
		weaverPresenceInitialized = true;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(this.abstractRoom.index);
		int num = world.weaverPresence.presenceRooms.IndexOf(this.abstractRoom);
		IntVector2 intVector = new IntVector2(TileWidth / 2, TileHeight / 2);
		if (num > 0)
		{
			AbstractRoom abstractRoom = world.weaverPresence.presenceRooms[num - 1];
			int num2 = Array.IndexOf(this.abstractRoom.connections, abstractRoom.index);
			if (num2 >= 0)
			{
				intVector = LocalCoordinateOfNode(num2).Tile;
			}
		}
		else if (warpPoints.Count > 0)
		{
			intVector = GetTilePosition((warpPoints.FirstOrDefault((WarpPoint warp) => warp.warpLocked) ?? warpPoints[0]).pos);
		}
		IntVector2 intVector2 = new IntVector2(TileWidth / 2, TileHeight / 2);
		if (num < world.weaverPresence.presenceRooms.Count - 1)
		{
			AbstractRoom abstractRoom2 = world.weaverPresence.presenceRooms[num + 1];
			int num3 = Array.IndexOf(this.abstractRoom.connections, abstractRoom2.index);
			if (num3 >= 0)
			{
				intVector2 = LocalCoordinateOfNode(num3).Tile;
			}
		}
		else
		{
			PlacedObject placedObject = roomSettings.placedObjects.FirstOrDefault((PlacedObject pObj) => pObj.type == WatcherEnums.PlacedObjectType.WeaverSpot);
			if (placedObject != null)
			{
				intVector2 = GetTilePosition(placedObject.pos);
			}
		}
		List<TileTypeResistance> tileResistances = new List<TileTypeResistance>();
		List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>
		{
			new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 1f, PathCost.Legality.IllegalConnection)
		};
		CreatureTemplate creatureType = new CreatureTemplate(CreatureTemplate.Type.Fly, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly), tileResistances, connectionResistances, default(CreatureTemplate.Relationship));
		QuickPathFinder quickPathFinder = new QuickPathFinder(intVector, intVector2, aimap, creatureType);
		for (int i = 0; i < 10000; i++)
		{
			if (quickPathFinder.status != 0)
			{
				break;
			}
			quickPathFinder.Update();
		}
		IntVector2[] array;
		float num4;
		if (quickPathFinder.status == 1)
		{
			array = quickPathFinder.ReturnPath().tiles;
			num4 = (float)array.Length * 20f;
		}
		else
		{
			array = new IntVector2[2] { intVector, intVector2 };
			num4 = Vector2.Distance(MiddleOfTile(intVector), MiddleOfTile(intVector2));
		}
		int num5 = Mathf.RoundToInt(num4 / 75f);
		int num6 = UnityEngine.Random.Range(num5 / 2, num5 + 1);
		for (int j = 0; j < num6; j++)
		{
			float num7 = (float)j / (float)num6 * (float)(array.Length - 1);
			int num8 = (int)num7;
			Vector2 center = Vector2.Lerp(MiddleOfTile(array[num8]), MiddleOfTile(array[num8 + 1]), num7 - (float)num8);
			if (FindFreePoint(center, 200f, 5, out var point) && FindThreadEnds(point, 50f, 500f, 20, out var start, out var end))
			{
				float length = Vector2.Distance(start, end) * UnityEngine.Random.Range(0.5f, 1f);
				WeaverThread weaverThread = new WeaverThread(this, length, start, end, stuckAtA: true, stuckAtB: true);
				AddObject(weaverThread);
				weaverPresenceObjects.Add(weaverThread);
			}
		}
		for (int k = 0; k < cameraPositions.Length; k++)
		{
			for (int l = 0; l < 4; l++)
			{
				if ((l != 0 || !(UnityEngine.Random.value < 0.2f)) && (l <= 0 || !(UnityEngine.Random.value < 0.5f)))
				{
					FloatingDebrisData floatingDebrisData = new FloatingDebrisData(null);
					bool flag = UnityEngine.Random.value < 0.25f;
					floatingDebrisData.type = (flag ? "Aurora Foreground" : "Aurora");
					floatingDebrisData.rippleLayer = 1;
					if (flag)
					{
						floatingDebrisData.depthFar = UnityEngine.Random.Range(5, 21);
						floatingDebrisData.depthNear = UnityEngine.Random.Range(0, 6);
					}
					else
					{
						floatingDebrisData.depthFar = UnityEngine.Random.Range(15, 31);
						floatingDebrisData.depthNear = UnityEngine.Random.Range(5, 16);
					}
					floatingDebrisData.scaleMaximum = UnityEngine.Random.Range(0.01f, 1f);
					floatingDebrisData.ClearControlPoints();
					int num9 = UnityEngine.Random.Range(2, 6);
					for (int m = 0; m < num9; m++)
					{
						Vector2 zero = Vector2.zero;
						zero = ((m != 0) ? new Vector2(cameraPositions[k].x + UnityEngine.Random.Range(0f, 2049f), cameraPositions[k].y + UnityEngine.Random.Range(0f, 1152f)) : new Vector2(cameraPositions[k].x + UnityEngine.Random.Range(0f, 1366f), cameraPositions[k].y + UnityEngine.Random.Range(0f, 768f)));
						floatingDebrisData.AddControlPoint(zero.x, zero.y, UnityEngine.Random.Range(-1f, 0.5f), UnityEngine.Random.Range(0.45f, 0.6f), UnityEngine.Random.Range(-1f, 0.5f));
					}
					FloatingDebris floatingDebris = FloatingDebris.TrySpawnFloatingDebris(this, floatingDebrisData.owner = new PlacedObject(PlacedObject.Type.FloatingDebris, floatingDebrisData));
					if (floatingDebris != null)
					{
						AddObject(floatingDebris);
						weaverPresenceObjects.Add(floatingDebris);
					}
				}
			}
		}
		UnityEngine.Random.state = state;
	}

	public bool FindFreePoint(Vector2 center, float radius, int tries, out Vector2 point)
	{
		for (int i = 0; i < tries; i++)
		{
			point = center + UnityEngine.Random.insideUnitCircle * radius;
			if (!HasAnySolid(GetTilePosition(point)))
			{
				return true;
			}
		}
		point = default(Vector2);
		return false;
	}

	public bool FindThreadEnds(Vector2 center, float minLength, float maxLength, int tries, out Vector2 start, out Vector2 end)
	{
		for (int i = 0; i < tries; i++)
		{
			Vector2 vector = Custom.RNV();
			Vector2? vector2 = SharedPhysics.ExactTerrainRayTracePos(this, center, center + vector * maxLength / 2f);
			if (!vector2.HasValue)
			{
				continue;
			}
			start = vector2.Value;
			vector2 = SharedPhysics.ExactTerrainRayTracePos(this, center, center - vector * (maxLength - Vector2.Distance(start, center)));
			if (vector2.HasValue)
			{
				end = vector2.Value;
				float num = Vector2.Distance(start, end);
				if (num >= minLength && num <= maxLength)
				{
					return true;
				}
			}
		}
		start = default(Vector2);
		end = default(Vector2);
		return false;
	}

	public void DestroyWeaverPresenceInRoom()
	{
		weaverPresenceInitialized = false;
		for (int i = 0; i < weaverPresenceObjects.Count; i++)
		{
			weaverPresenceObjects[i].Destroy();
		}
		weaverPresenceObjects.Clear();
		if (this == game.cameras[0].room)
		{
			game.cameras[0].UpdateGhostMode(this, game.cameras[0].currentCameraPosition);
		}
	}

	public void MaterializeRippleSpawn(Vector2 spawnPos, RippleSpawnSource source)
	{
		float num = 3f;
		if (game.IsStorySession)
		{
			num = (game.session as StoryGameSession).saveState.deathPersistentSaveData.rippleLevel;
		}
		if (game.Players.Count > 0 && game.Players[0].realizedCreature != null && (game.Players[0].realizedCreature as Player).devMaxLevelRipple)
		{
			num = Player.devMaxLevelRippleValue;
		}
		if (num < 2f)
		{
			return;
		}
		int num2 = 50;
		if (source == RippleSpawnSource.WarpPoint)
		{
			num2 = 25;
		}
		if (source == RippleSpawnSource.Dimension)
		{
			num2 = Math.Min((int)(float)(TileWidth * TileHeight), 4000) / 80;
		}
		if (voidSpawns.Count + rippleJellies.Count >= num2)
		{
			return;
		}
		VoidSpawn.SpawnType spawnType = VoidSpawn.SpawnType.RippleSpawn;
		if (UnityEngine.Random.Range(0, 10) >= 7)
		{
			spawnType = VoidSpawn.SpawnType.RippleJelly;
		}
		if (num >= 4f && (source == RippleSpawnSource.Ripple || source == RippleSpawnSource.Dimension))
		{
			float num3 = 0f;
			num3 = ((num >= 5f) ? 0.1f : ((!(num >= 4.5f)) ? 0.02f : 0.04f));
			if (spawnedWeaver)
			{
				num3 = 0f;
			}
			for (int i = 0; i < game.world.spinningTopPresences.Count; i++)
			{
				if (game.world.spinningTopPresences[i].ghostRoom == abstractRoom)
				{
					num3 = 0f;
					break;
				}
			}
			if (Region.IsAncientUrbanRegion(world.name))
			{
				num3 = 0f;
			}
			if (UnityEngine.Random.value <= num3)
			{
				spawnType = VoidSpawn.SpawnType.RippleAmoeba;
			}
		}
		AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(world, WatcherEnums.AbstractObjectType.RippleSpawn, null, GetWorldCoordinate(spawnPos), game.GetNewID());
		PhysicalObject physicalObject;
		if (spawnType == VoidSpawn.SpawnType.RippleJelly)
		{
			physicalObject = new RippleJelly(abstractPhysicalObject);
		}
		else
		{
			physicalObject = new VoidSpawn(abstractPhysicalObject, roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt), VoidSpawnKeeper.DayLightMode(this), spawnType);
			VoidSpawn voidSpawn = physicalObject as VoidSpawn;
			if (spawnType == VoidSpawn.SpawnType.RippleAmoeba)
			{
				voidSpawn.behavior = new VoidSpawn.ChasePlayer(voidSpawn, this);
			}
			else if (spawnType == VoidSpawn.SpawnType.RippleJelly)
			{
				voidSpawn.behavior = new VoidSpawn.BezierSwarm(voidSpawn, this);
			}
			else if (source == RippleSpawnSource.Dimension)
			{
				float value = UnityEngine.Random.value;
				if (value < 0.25f)
				{
					voidSpawn.behavior = new VoidSpawn.BezierSwarm(voidSpawn, this);
				}
				else if (value < 0.55f)
				{
					int num4 = -1;
					if (abstractRoom.connections.Length != 0)
					{
						num4 = abstractRoom.connections[UnityEngine.Random.Range(0, abstractRoom.connections.Length)];
					}
					if (world.GetAbstractRoom(num4) == null)
					{
						voidSpawn.behavior = new VoidSpawn.EggAndAway(voidSpawn, this);
					}
					else
					{
						voidSpawn.behavior = new VoidSpawn.PassThrough(voidSpawn, num4, this);
					}
				}
				else
				{
					voidSpawn.behavior = new VoidSpawn.BezierSwarm(voidSpawn, this);
				}
			}
			else if (UnityEngine.Random.value < 0.5f)
			{
				voidSpawn.behavior = new VoidSpawn.ChasePlayer(voidSpawn, this);
			}
			else
			{
				voidSpawn.behavior = new VoidSpawn.EggAndAway(voidSpawn, this);
			}
		}
		physicalObject.PlaceInRoom(this);
		if (!(physicalObject is IRippleSpawn))
		{
			return;
		}
		IRippleSpawn rippleSpawn = physicalObject as IRippleSpawn;
		if (spawnType == VoidSpawn.SpawnType.RippleAmoeba)
		{
			if (num <= 5f)
			{
				rippleSpawn.SetTimeUntilFadeOut(UnityEngine.Random.Range(800, 1600));
			}
			else
			{
				rippleSpawn.SetTimeUntilFadeOut(UnityEngine.Random.Range(1200, 3600));
			}
		}
		else if (num < 3f)
		{
			rippleSpawn.SetTimeUntilFadeOut(UnityEngine.Random.Range(40, 200));
		}
		else if (num < 4f)
		{
			rippleSpawn.SetTimeUntilFadeOut(UnityEngine.Random.Range(200, 400));
		}
		else
		{
			rippleSpawn.SetTimeUntilFadeOut(UnityEngine.Random.Range(400, 1200));
		}
		if (source == RippleSpawnSource.Dimension)
		{
			rippleSpawn.ChangeRippleLayer(1, showEffect: false);
		}
		else
		{
			rippleSpawn.ChangeRippleLayer(0, showEffect: true);
		}
	}

	public void UpdateWindDirection()
	{
		this.windDirection = Vector2.zero;
		WindDirection windDirection = windDirObject;
		if (windDirection != null)
		{
			this.windDirection = windDirection.direction;
			return;
		}
		RoomSettings.RoomEffect effect = roomSettings.GetEffect(RoomSettings.RoomEffect.Type.WindDirection);
		if (effect != null)
		{
			this.windDirection = Custom.DegToVec(effect.GetAmount(0) * 360f) * (effect.GetAmount(1) * 200f);
		}
	}

	public bool InfectRoomWithSentientRot(float amount)
	{
		if (world.regionState == null)
		{
			return false;
		}
		if (world.game.GetStorySession.finalWarpSequenceStarted)
		{
			return false;
		}
		if (ModManager.Watcher && amount > 0f)
		{
			if (Region.HasSentientRotResistance(world.name))
			{
				return false;
			}
			return world.regionState.InfectRegionRoomWithSentientRot(amount, abstractRoom.name);
		}
		return false;
	}

	public void UpdateSentientRotEffect()
	{
		if (!ModManager.Watcher || game.cameras == null || game.cameras[0] == null || !BeingViewed || world.regionState == null)
		{
			return;
		}
		if (abstractRoom.gate && Region.IsWatcherVanillaRegion(world.name))
		{
			game.cameras[0].UpdateRotMode(this, 0f);
			didFirstSentientRotUpdate = true;
		}
		else
		{
			if (game != null && game.IsStorySession && game.GetStorySession.finalWarpSequenceStarted)
			{
				return;
			}
			RoomSettings.RoomEffect effect = roomSettings.GetEffect(WatcherEnums.RoomEffectType.SentientRotInfection);
			float num = effect?.amount ?? 0f;
			float amount = num;
			if (game != null && game.IsStorySession && world.regionState.sentientRotProgression.ContainsKey(abstractRoom.name))
			{
				if (didFirstSentientRotUpdate)
				{
					num = 0f;
					if (ticker % 8 == 0 && game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility && world.regionState.sentientRotProgression[abstractRoom.name].rotIntensity > 0f)
					{
						world.regionState.sentientRotProgression[abstractRoom.name].rotIntensity = Mathf.Max(0f, world.regionState.sentientRotProgression[abstractRoom.name].rotIntensity - 0.005f);
						SentientRotReduced(world.regionState.sentientRotProgression[abstractRoom.name].rotIntensity);
					}
				}
				amount = num + (1f - num) * world.regionState.sentientRotProgression[abstractRoom.name].rotIntensity;
				if (effect == null)
				{
					roomSettings.effects.Add(new RoomSettings.RoomEffect(WatcherEnums.RoomEffectType.SentientRotInfection, amount, inherited: false)
					{
						save = false
					});
				}
				else
				{
					effect.amount = amount;
				}
				if (didFirstSentientRotUpdate && game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility && world.regionState.sentientRotProgression[abstractRoom.name].rotIntensity <= 0f)
				{
					world.regionState.sentientRotProgression.Remove(abstractRoom.name);
					if (world.regionState.sentientRotProgression.Count == 0)
					{
						if (game.GetStorySession.saveState.miscWorldSaveData.regionsInfectedBySentientRot.Contains(world.name.ToLowerInvariant()))
						{
							game.GetStorySession.saveState.miscWorldSaveData.regionsInfectedBySentientRot.Remove(world.name.ToLowerInvariant());
						}
						if (game.GetStorySession.saveState.miscWorldSaveData.regionsInfectedBySentientRotSpread.Contains(world.name.ToLowerInvariant()))
						{
							game.GetStorySession.saveState.miscWorldSaveData.regionsInfectedBySentientRotSpread.Remove(world.name.ToLowerInvariant());
						}
					}
				}
			}
			game.cameras[0].UpdateRotMode(this, amount);
			didFirstSentientRotUpdate = true;
		}
	}

	public void InitializeSentientRotPresenceInRoom(float amount)
	{
		rotPresenceInitialized = true;
		if (abstractRoom.shelter || abstractRoom.gate)
		{
			return;
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractRoom.index);
		List<IntVector2> list = new List<IntVector2>();
		List<IntVector2> list2 = new List<IntVector2>();
		List<IntVector2> list3 = new List<IntVector2>();
		for (int i = 0; i < TileWidth; i++)
		{
			for (int j = 0; j < TileHeight; j++)
			{
				AItile aItile = aimap.getAItile(i, j);
				if (!aItile.narrowSpace && (aItile.acc == AItile.Accessibility.Ceiling || aItile.acc == AItile.Accessibility.Floor || aItile.acc == AItile.Accessibility.Wall || aItile.acc == AItile.Accessibility.Air))
				{
					if (aItile.acc == AItile.Accessibility.Floor)
					{
						list2.Add(new IntVector2(i, j - 1));
					}
					else if (aItile.acc == AItile.Accessibility.Ceiling)
					{
						list.Add(new IntVector2(i, j + 1));
					}
					else
					{
						list3.Add(new IntVector2(i, j));
					}
				}
			}
		}
		bool flag = roomSettings.DangerType != RoomRain.DangerType.None && roomSettings.DangerType != RoomRain.DangerType.Flood;
		if (roomSettings.parent != null && !roomSettings.parent.isAncestor && (roomSettings.parent.name.ToLowerInvariant().Contains("outside") || roomSettings.parent.name.ToLowerInvariant().Contains("surface")))
		{
			flag = true;
		}
		bool flag2 = UnityEngine.Random.value < 0.5f && flag && amount >= 0.75f;
		bool flag3 = UnityEngine.Random.value < 0.15f && amount >= 1f;
		float value = UnityEngine.Random.value;
		bool flag4 = amount >= 0.5f && (flag2 ? (value < 0.2f) : (value < 0.5f));
		float num = 0f;
		if (flag)
		{
			num += 0.2f;
			if (amount >= 1f)
			{
				num = 1f;
			}
		}
		num += 0.1f * (float)cameraPositions.Length;
		bool num2 = UnityEngine.Random.value < num && amount >= 0.5f;
		bool flag5 = UnityEngine.Random.value < Mathf.Lerp(0.2f, 0.75f, amount);
		bool flag6 = UnityEngine.Random.value < Mathf.Lerp(0.25f, 0.5f, amount);
		bool flag7 = UnityEngine.Random.value < Mathf.Lerp(0.25f, 0.5f, (amount - 0.5f) * 2f) && amount >= 0.5f;
		bool flag8 = UnityEngine.Random.value < 0.5f && amount >= 1f && Custom.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot;
		float minInclusive = UnityEngine.Random.Range(0.05f * amount, 0.1f * amount);
		float maxInclusive = UnityEngine.Random.Range(0.25f * amount, 1f * amount);
		float a = UnityEngine.Random.Range(minInclusive, maxInclusive);
		float minInclusive2 = UnityEngine.Random.Range(0f, 0.25f * amount);
		float maxInclusive2 = UnityEngine.Random.Range(0.5f * amount, amount);
		float a2 = UnityEngine.Random.Range(minInclusive2, maxInclusive2);
		float num3 = Mathf.Min(5, UnityEngine.Random.Range(1, 1 + cameraPositions.Length * 2));
		float num4 = Mathf.Min(20, UnityEngine.Random.Range(2, 3 + cameraPositions.Length * 4));
		float num5 = Mathf.Min(10, UnityEngine.Random.Range(1, 3 + cameraPositions.Length * 2));
		float num6 = Mathf.Min(5, UnityEngine.Random.Range(1, 1 + cameraPositions.Length));
		float num7 = Mathf.Min(8, UnityEngine.Random.Range(1, 1 + cameraPositions.Length * 2));
		float num8 = Mathf.Min(3, UnityEngine.Random.Range(1, 1 + cameraPositions.Length));
		if (num2)
		{
			RoomSettings.RoomEffect effect = roomSettings.GetEffect(WatcherEnums.RoomEffectType.SentientRotParticles);
			float b = 0f;
			if (effect != null)
			{
				b = effect.amount;
			}
			if (effect == null)
			{
				effect = new RoomSettings.RoomEffect(WatcherEnums.RoomEffectType.SentientRotParticles, Mathf.Max(a, b), inherited: false)
				{
					save = false
				};
				roomSettings.effects.Add(effect);
			}
			else
			{
				effect.amount = Mathf.Max(a, b);
			}
			for (int num9 = updateList.Count - 1; num9 >= 0; num9--)
			{
				if (updateList[num9] is SentientRotSpores)
				{
					updateList[num9].Destroy();
					RemoveObject(updateList[num9]);
				}
			}
			AddObject(new SentientRotSpores(this));
		}
		if (flag5)
		{
			RoomSettings.RoomEffect effect2 = roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Flies);
			float b2 = 0f;
			if (effect2 != null)
			{
				b2 = effect2.amount;
			}
			if (effect2 == null)
			{
				roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Flies, Mathf.Max(a2, b2), inherited: false)
				{
					save = false
				});
			}
			else
			{
				effect2.amount = Mathf.Max(a2, b2);
			}
		}
		if (flag3 && list2.Count > 0 && !world.sentientRotRoomsInitiated.Contains(abstractRoom))
		{
			List<Vector2> list4 = new List<Vector2>();
			for (int k = 0; (float)k < num3; k++)
			{
				for (int l = 0; l < 10; l++)
				{
					IntVector2 pos = list2[UnityEngine.Random.Range(0, list2.Count)];
					if (Rattler.ValidSpawnPos(this, pos, list4))
					{
						Vector2 vector = MiddleOfTile(pos);
						AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.Rattler), null, GetWorldCoordinate(vector), game.GetNewID());
						abstractRoom.AddEntity(abstractCreature);
						abstractCreature.RealizeInRoom();
						list4.Add(vector);
						break;
					}
				}
			}
		}
		if (flag6 && list.Count > 0)
		{
			List<Vector2> list5 = new List<Vector2>();
			for (int m = 0; (float)m < num4; m++)
			{
				IntVector2? intVector = null;
				for (int n = 0; n < 10; n++)
				{
					int index = UnityEngine.Random.Range(0, list.Count);
					intVector = list[index];
					for (int num10 = 0; num10 < list5.Count; num10++)
					{
						if (Vector2.Distance(list5[num10], MiddleOfTile(intVector.Value)) < 100f)
						{
							intVector = null;
							break;
						}
					}
					if (intVector.HasValue)
					{
						break;
					}
				}
				if (intVector.HasValue)
				{
					Vector2 vector2 = MiddleOfTile(intVector.Value);
					GooDripSource.GooDripsData gooDripsData = new GooDripSource.GooDripsData(null);
					float y = UnityEngine.Random.Range(Mathf.Lerp(20f, 40f, amount), Mathf.Lerp(80f, 200f, amount));
					gooDripsData.handlePos = new Vector2(0f, y);
					gooDripsData.panelPos = vector2;
					gooDripsData.frequency = UnityEngine.Random.Range(Mathf.Lerp(0.05f, 0.25f, amount), Mathf.Lerp(0.2f, 1f, amount));
					PlacedObject placedObject = new PlacedObject(PlacedObject.Type.GooDrips, gooDripsData);
					placedObject.pos = vector2;
					gooDripsData.owner = placedObject;
					AddObject(new GooDripSource(placedObject));
					list5.Add(vector2);
				}
			}
		}
		if (flag7 && (list.Count > 0 || list2.Count > 0))
		{
			if (brainMold == null)
			{
				AddObject(brainMold = new BrainMold());
			}
			else
			{
				brainMold.Destroy();
				RemoveObject(brainMold);
				AddObject(brainMold = new BrainMold());
				for (int num11 = 0; num11 < roomSettings.placedObjects.Count; num11++)
				{
					if (roomSettings.placedObjects[num11].type == PlacedObject.Type.BrainMold)
					{
						brainMold.AddSpot(roomSettings.placedObjects[num11]);
					}
				}
			}
			List<Vector2> list6 = new List<Vector2>();
			for (int num12 = 0; (float)num12 < num5; num12++)
			{
				IntVector2? intVector2 = null;
				for (int num13 = 0; num13 < 10; num13++)
				{
					bool flag9 = (UnityEngine.Random.value < 0.5f && list2.Count > 0) || list.Count == 0;
					int index2 = (flag9 ? UnityEngine.Random.Range(0, list2.Count) : UnityEngine.Random.Range(0, list.Count));
					intVector2 = (flag9 ? list2[index2] : list[index2]);
					for (int num14 = 0; num14 < list6.Count; num14++)
					{
						if (Vector2.Distance(list6[num14], MiddleOfTile(intVector2.Value)) < 100f)
						{
							intVector2 = null;
							break;
						}
					}
					if (intVector2.HasValue)
					{
						break;
					}
				}
				if (intVector2.HasValue)
				{
					Vector2 vector3 = MiddleOfTile(intVector2.Value);
					PlacedObject.ResizableObjectData resizableObjectData = new PlacedObject.ResizableObjectData(null);
					float y2 = UnityEngine.Random.Range(Mathf.Lerp(20f, 40f, amount), Mathf.Lerp(80f, 200f, amount));
					resizableObjectData.handlePos = new Vector2(0f, y2);
					PlacedObject placedObject2 = new PlacedObject(PlacedObject.Type.BrainMold, resizableObjectData);
					placedObject2.pos = vector3;
					resizableObjectData.owner = placedObject2;
					brainMold.AddSpot(placedObject2);
					list6.Add(vector3);
				}
			}
			brainMold.AIMapReady();
		}
		if (flag4 && (list.Count > 0 || list2.Count > 0))
		{
			List<Vector2> list7 = new List<Vector2>();
			for (int num15 = 0; (float)num15 < num6; num15++)
			{
				IntVector2? intVector3 = null;
				bool flag10 = false;
				for (int num16 = 0; num16 < 10; num16++)
				{
					flag10 = (UnityEngine.Random.value < 0.65f && list2.Count > 0) || list.Count == 0;
					int index3 = (flag10 ? UnityEngine.Random.Range(0, list2.Count) : UnityEngine.Random.Range(0, list.Count));
					intVector3 = (flag10 ? list2[index3] : list[index3]);
					for (int num17 = 0; num17 < list7.Count; num17++)
					{
						if (Vector2.Distance(list7[num17], MiddleOfTile(intVector3.Value)) < 200f)
						{
							intVector3 = null;
							break;
						}
					}
					if (intVector3.HasValue)
					{
						AItile aItile2 = aimap.getAItile(intVector3.Value.x - 1, intVector3.Value.y);
						AItile aItile3 = aimap.getAItile(intVector3.Value.x + 1, intVector3.Value.y);
						if (aItile2 == null || (aItile2.acc != AItile.Accessibility.Solid && aItile2.acc != AItile.Accessibility.Floor && aItile2.acc != AItile.Accessibility.Ceiling))
						{
							intVector3 = null;
						}
						if (aItile3 == null || (aItile3.acc != AItile.Accessibility.Solid && aItile3.acc != AItile.Accessibility.Floor && aItile3.acc != AItile.Accessibility.Ceiling))
						{
							intVector3 = null;
						}
					}
					if (!intVector3.HasValue)
					{
						continue;
					}
					Vector2 vector4 = MiddleOfTile(intVector3.Value);
					PlacedObject.ResizableObjectData resizableObjectData2 = new PlacedObject.ResizableObjectData(null);
					bool flag11 = false;
					for (int num18 = 0; num18 < 50; num18++)
					{
						float num19 = UnityEngine.Random.Range(80f, 240f);
						float ang = (flag10 ? UnityEngine.Random.Range(10, 171) : UnityEngine.Random.Range(190, 351));
						if (!SharedPhysics.ExactTerrainRayTracePos(this, vector4, vector4 + Custom.DegToVec(ang) * num19).HasValue)
						{
							resizableObjectData2.handlePos = Custom.DegToVec(ang) * num19;
							PlacedObject placedObject3 = new PlacedObject(PlacedObject.Type.PlateTree, resizableObjectData2);
							placedObject3.pos = vector4;
							resizableObjectData2.owner = placedObject3;
							AddObject(new PlateTree(placedObject3, this, PlateTree.Variant.Rot));
							list7.Add(vector4);
							flag11 = true;
							break;
						}
					}
					if (flag11)
					{
						break;
					}
				}
			}
			if (flag2)
			{
				waitToEnterAfterFullyLoaded = Mathf.Max(waitToEnterAfterFullyLoaded, 40);
			}
		}
		if (flag2 && list3.Count > 0 && !world.sentientRotRoomsInitiated.Contains(abstractRoom))
		{
			List<Vector2> list8 = new List<Vector2>();
			for (int num20 = 0; (float)num20 < num7; num20++)
			{
				for (int num21 = 0; num21 < 10; num21++)
				{
					IntVector2 pos2 = list3[UnityEngine.Random.Range(0, list3.Count)];
					if (!LobeTree.ValidSpawnPos(this, pos2, list8))
					{
						continue;
					}
					Vector2 vector5 = MiddleOfTile(pos2);
					LobeTree.LobeTreeData lobeTreeData = new LobeTree.LobeTreeData(null);
					float num22 = -1f;
					Vector2 handlePos = Vector2.zero;
					Vector2 rootOffset = Vector2.zero;
					float b3 = UnityEngine.Random.Range(60f, 140f);
					b3 = Mathf.Min((float)aimap.getTerrainProximity(vector5) * 20f - 10f, b3);
					for (int num23 = 0; num23 < 50; num23++)
					{
						float num24 = UnityEngine.Random.Range(190, 351);
						float num25 = UnityEngine.Random.Range(-45, 45);
						float ang2 = (num24 + num25) % 360f;
						Vector2 vector6 = Custom.DegToVec(num24) * b3;
						Vector2 vector7 = vector5 + vector6;
						Vector2? vector8 = SharedPhysics.ExactTerrainRayTracePos(this, vector7, vector7 + Custom.DegToVec(ang2) * 700f);
						if (!vector8.HasValue)
						{
							continue;
						}
						float num26 = Mathf.Abs(Vector2.Distance(vector8.Value, vector5) - b3 * 1.5f);
						if ((num22 < 0f || num26 < num22) && LobeTree.ValidRootPos(this, vector8.Value))
						{
							num22 = num26;
							handlePos = vector6;
							rootOffset = vector8.Value - vector5;
							if (num26 <= 50f)
							{
								break;
							}
						}
					}
					if (num22 >= 0f)
					{
						lobeTreeData.handlePos = handlePos;
						lobeTreeData.rootOffset = rootOffset;
						PlacedObject placedObject4 = new PlacedObject(PlacedObject.Type.LobeTree, lobeTreeData);
						placedObject4.pos = vector5;
						lobeTreeData.owner = placedObject4;
						LobeTree.AbstractLobeTree abstractLobeTree = new LobeTree.AbstractLobeTree(world, AbstractPhysicalObject.AbstractObjectType.LobeTree, null, GetWorldCoordinate(vector5), game.GetNewID(), placedObject4);
						abstractRoom.entities.Add(abstractLobeTree);
						abstractLobeTree.RealizeInRoom();
						list8.Add(vector5);
					}
					break;
				}
			}
		}
		if (flag8 && list2.Count > 0)
		{
			List<Vector2> list9 = new List<Vector2>();
			for (int num27 = 0; (float)num27 < num8; num27++)
			{
				IntVector2? intVector4 = null;
				for (int num28 = 0; num28 < 10; num28++)
				{
					int index4 = UnityEngine.Random.Range(0, list2.Count);
					intVector4 = list2[index4];
					for (int num29 = 0; num29 < list9.Count; num29++)
					{
						if (Vector2.Distance(list9[num29], MiddleOfTile(intVector4.Value)) < 300f)
						{
							intVector4 = null;
							break;
						}
					}
					if (intVector4.HasValue)
					{
						break;
					}
				}
				if (!intVector4.HasValue)
				{
					continue;
				}
				Vector2 vector9 = MiddleOfTile(intVector4.Value);
				KarmaFlowerPatch.KarmaFlowerPatchData karmaFlowerPatchData = new KarmaFlowerPatch.KarmaFlowerPatchData(null);
				float num30 = UnityEngine.Random.Range(60f, 150f);
				karmaFlowerPatchData.handlePos = Custom.RNV() * num30;
				karmaFlowerPatchData.tilt = UnityEngine.Random.Range(0.25f, 0.75f);
				PlacedObject placedObject5 = new PlacedObject(WatcherEnums.PlacedObjectType.KarmaFlowerPatch, karmaFlowerPatchData)
				{
					save = false
				};
				placedObject5.pos = vector9;
				karmaFlowerPatchData.owner = placedObject5;
				roomSettings.placedObjects.Add(placedObject5);
				list9.Add(vector9);
				int num31 = UnityEngine.Random.Range(0, 3);
				if (num31 <= 0)
				{
					continue;
				}
				List<IntVector2> list10 = new List<IntVector2>();
				for (int num32 = 0; num32 < list2.Count; num32++)
				{
					if (Vector2.Distance(vector9, MiddleOfTile(list2[num32])) < num30)
					{
						list10.Add(list2[num32]);
					}
				}
				for (int num33 = 0; num33 < num31; num33++)
				{
					if (list10.Count == 0)
					{
						break;
					}
					int index5 = UnityEngine.Random.Range(0, list10.Count);
					Vector2 pos3 = MiddleOfTile(list10[index5]);
					AbstractPhysicalObject abstractPhysicalObject = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, GetWorldCoordinate(pos3), game.GetNewID(), -1, -1, null);
					abstractRoom.AddEntity(abstractPhysicalObject);
					abstractPhysicalObject.RealizeInRoom();
					abstractPhysicalObject.realizedObject.firstChunk.pos = pos3;
					list10.RemoveAt(index5);
				}
			}
			if (list9.Count > 0 && !updateList.OfType<KarmaFlowerPatch>().Any())
			{
				KarmaFlowerPatch karmaFlowerPatch = new KarmaFlowerPatch();
				AddObject(karmaFlowerPatch);
				if (shortCutsReady)
				{
					karmaFlowerPatch.ShortcutsReady();
				}
			}
		}
		UnityEngine.Random.state = state;
		int num34 = -1;
		int num35 = -1;
		float num36 = 0f;
		bool flag12 = world.name.ToCharArray().Sum((char x) => x) % 2 == 0;
		bool num37 = ((abstractRoom.subregionName == null) ? flag12 : (abstractRoom.subregionName.ToCharArray().Sum((char x) => x) % 2 == 0));
		string sample = (flag12 ? "RWTW_RotAmb_Low.ogg" : "RWTW_RotAmb_Med.ogg");
		if (!flag)
		{
			sample = "RWTW_RotAmb_Deep.ogg";
		}
		string sample2 = (num37 ? "RWTW_RotAmb_Swarm.ogg" : "RWTW_RotAmb_Goo.ogg");
		for (int num38 = 0; num38 < roomSettings.ambientSounds.Count; num38++)
		{
			AmbientSound ambientSound = roomSettings.ambientSounds[num38];
			if (ambientSound.type == AmbientSound.Type.Omnidirectional && (num34 == -1 || ambientSound.volume >= num36))
			{
				num35 = num34;
				num34 = num38;
				num36 = ambientSound.volume;
			}
		}
		if (amount >= 0.5f)
		{
			if (num34 == -1)
			{
				OmniDirectionalSound omniDirectionalSound = new OmniDirectionalSound(sample, inherited: false);
				omniDirectionalSound.volume = (flag ? 0.5f : 0.2f);
				roomSettings.ambientSounds.Add(omniDirectionalSound);
				num34 = roomSettings.ambientSounds.Count - 1;
			}
			else
			{
				float volume = roomSettings.ambientSounds[num34].volume;
				float pitch = roomSettings.ambientSounds[num34].pitch;
				roomSettings.ambientSounds[num34] = new OmniDirectionalSound(sample, inherited: false);
				roomSettings.ambientSounds[num34].volume = Mathf.Min(volume, flag ? 0.5f : 0.2f);
				roomSettings.ambientSounds[num34].pitch = pitch;
			}
		}
		if (amount >= 1f)
		{
			if (num35 == -1)
			{
				OmniDirectionalSound omniDirectionalSound2 = new OmniDirectionalSound(sample2, inherited: false);
				omniDirectionalSound2.volume = 0.4f;
				roomSettings.ambientSounds.Add(omniDirectionalSound2);
				num35 = roomSettings.ambientSounds.Count - 1;
			}
			else
			{
				float volume2 = roomSettings.ambientSounds[num35].volume;
				float pitch2 = roomSettings.ambientSounds[num35].pitch;
				roomSettings.ambientSounds[num35] = new OmniDirectionalSound(sample2, inherited: false);
				roomSettings.ambientSounds[num35].volume = Mathf.Min(volume2, 0.4f);
				roomSettings.ambientSounds[num35].pitch = pitch2;
			}
		}
		if (amount >= 0.5f && game.cameras[0].room == this)
		{
			game.cameras[0].virtualMicrophone.NewRoom(game.cameras[0].room);
		}
		world.sentientRotRoomsInitiated.Add(abstractRoom);
	}

	public void SentientRotReduced(float amount)
	{
		for (int i = 0; i < game.Players.Count; i++)
		{
			if (game.Players[i].realizedCreature != null && game.Players[i].realizedCreature.room == this)
			{
				(game.Players[i].realizedCreature as Player).SetHaloDisplayTime(40);
			}
		}
		if (amount < 0.5f)
		{
			RoomSettings.RoomEffect effect = roomSettings.GetEffect(WatcherEnums.RoomEffectType.SentientRotParticles);
			if (effect != null)
			{
				effect.amount = Mathf.Min(effect.amount, Mathf.InverseLerp(0.15f, 0.5f, amount));
			}
		}
		for (int j = 0; j < updateList.Count; j++)
		{
			if (updateList[j] is GooDripSource && amount < 0.5f)
			{
				(updateList[j] as GooDripSource).SetFrequency(Mathf.Min((updateList[j] as GooDripSource).GetFrequency(), Mathf.InverseLerp(0.15f, 0.5f, amount)));
			}
		}
		for (int k = 0; k < roomSettings.ambientSounds.Count; k++)
		{
			AmbientSound ambientSound = roomSettings.ambientSounds[k];
			if (ambientSound.type == AmbientSound.Type.Omnidirectional && (ambientSound.sample == "RWTW_RotAmb_Low.ogg" || ambientSound.sample == "RWTW_RotAmb_Med.ogg" || ambientSound.sample == "RWTW_RotAmb_Deep.ogg" || ambientSound.sample == "RWTW_RotAmb_Swarm.ogg" || ambientSound.sample == "RWTW_RotAmb_Goo.ogg"))
			{
				ambientSound.volume = Mathf.Min(ambientSound.volume, amount);
			}
		}
	}

	public void AddObject(UpdatableAndDeletable obj)
	{
		if (obj is TerrainManager.ITerrain item)
		{
			if (terrain == null)
			{
				terrain = new TerrainManager();
			}
			terrain.terrainList.Add(item);
		}
		if (game == null)
		{
			return;
		}
		if (obj is DeafLoopHolder)
		{
			for (int i = 0; i < updateList.Count; i++)
			{
				if (updateList[i] is DeafLoopHolder)
				{
					(updateList[i] as DeafLoopHolder).muted = true;
				}
			}
		}
		updateList.Add(obj);
		obj.room = this;
		IDrawable drawable = null;
		if (obj is IDrawable)
		{
			drawable = obj as IDrawable;
		}
		if (obj is LightSource)
		{
			LightSource lightSource = obj as LightSource;
			if (ModManager.MMF && lightSource.noGameplayImpact)
			{
				cosmeticLightSources.Add(lightSource);
			}
			else
			{
				lightSources.Add(lightSource);
			}
		}
		if (obj is IAccessibilityModifier)
		{
			accessModifiers.Add(obj as IAccessibilityModifier);
		}
		if (obj is VisionObscurer)
		{
			visionObscurers.Add(obj as VisionObscurer);
		}
		if (obj is ZapCoil)
		{
			zapCoils.Add(obj as ZapCoil);
		}
		if (obj is LightningMachine)
		{
			lightningMachines.Add(obj as LightningMachine);
		}
		if (obj is EnergySwirl)
		{
			energySwirls.Add(obj as EnergySwirl);
		}
		if (obj is SnowSource)
		{
			snowSources.Add(obj as SnowSource);
			AddSnow();
		}
		if (ModManager.DLCShared && obj is OEsphere)
		{
			oeSpheres.Add(obj as OEsphere);
		}
		if (obj is CellDistortion)
		{
			cellDistortions.Add(obj as CellDistortion);
		}
		if (obj is LocalBlizzard)
		{
			localBlizzards.Add(obj as LocalBlizzard);
		}
		if (ModManager.HypothermiaModule && obj is IProvideWarmth)
		{
			blizzardHeatSources.Add(obj as IProvideWarmth);
		}
		if (obj is WarpPoint)
		{
			warpPoints.Add(obj as WarpPoint);
		}
		if (obj is VoidSpawn)
		{
			voidSpawns.Add(obj as VoidSpawn);
		}
		if (obj is RippleJelly)
		{
			rippleJellies.Add(obj as RippleJelly);
		}
		if (obj is PhysicalObject)
		{
			physicalObjects[(obj as PhysicalObject).collisionLayer].Add(obj as PhysicalObject);
			if (obj is OracleSwarmer)
			{
				SwarmerCount++;
			}
			if ((obj as PhysicalObject).graphicsModule != null)
			{
				drawable = (obj as PhysicalObject).graphicsModule;
			}
			else if (BeingViewed)
			{
				(obj as PhysicalObject).InitiateGraphicsModule();
				if ((obj as PhysicalObject).graphicsModule != null)
				{
					drawable = (obj as PhysicalObject).graphicsModule;
				}
			}
		}
		if (obj is RippleTree rippleTree)
		{
			for (int j = 0; j < drawableObjects.Count; j++)
			{
				if (!(drawableObjects[j] is RippleTree rippleTree2) || !(rippleTree2.cosmetics.depth < rippleTree.cosmetics.depth))
				{
					continue;
				}
				drawableObjects.Insert(j, drawable);
				for (int k = 0; k < game.cameras.Length; k++)
				{
					if (game.cameras[k].room == this)
					{
						game.cameras[k].NewObjectInRoom(drawable, rippleTree2);
					}
				}
				return;
			}
		}
		if (obj is MudOverlay mudOverlay)
		{
			drawableObjects.Add(drawable);
			for (int l = 0; l < game.cameras.Length; l++)
			{
				if (game.cameras[l].room == this)
				{
					game.cameras[l].NewObjectInRoom(drawable, mudOverlay.ownerSleaser.drawableObject);
				}
			}
		}
		else
		{
			if (drawable == null)
			{
				return;
			}
			drawableObjects.Add(drawable);
			for (int m = 0; m < game.cameras.Length; m++)
			{
				if (game.cameras[m].room == this)
				{
					game.cameras[m].NewObjectInRoom(drawable);
				}
			}
		}
	}

	public void DestroyObject(EntityID ID)
	{
		for (int i = 0; i < updateList.Count; i++)
		{
			if (updateList[i] is PhysicalObject && (updateList[i] as PhysicalObject).abstractPhysicalObject.ID == ID)
			{
				updateList[i].Destroy();
				break;
			}
		}
	}

	public void RemoveObject(UpdatableAndDeletable obj)
	{
		if (obj.room == this)
		{
			obj.RemoveFromRoom();
		}
		if (obj is CosmeticRipple)
		{
			cosmeticRipples.Remove(obj as CosmeticRipple);
		}
		if (updateList.IndexOf(obj) > updateIndex)
		{
			CleanOutObjectNotInThisRoom(obj);
		}
	}

	public void CleanOutObjectNotInThisRoom(UpdatableAndDeletable obj)
	{
		updateList.Remove(obj);
		if (obj is IDrawable)
		{
			drawableObjects.Remove(obj as IDrawable);
		}
		if (obj is LightSource)
		{
			LightSource lightSource = obj as LightSource;
			if (ModManager.MMF && lightSource.noGameplayImpact)
			{
				cosmeticLightSources.Remove(lightSource);
			}
			else
			{
				lightSources.Remove(lightSource);
			}
		}
		if (obj is IAccessibilityModifier)
		{
			accessModifiers.Remove(obj as IAccessibilityModifier);
		}
		if (obj is VisionObscurer)
		{
			visionObscurers.Remove(obj as VisionObscurer);
		}
		if (ModManager.HypothermiaModule && obj is IProvideWarmth)
		{
			blizzardHeatSources.Remove(obj as IProvideWarmth);
		}
		if (obj is WarpPoint)
		{
			warpPoints.Remove(obj as WarpPoint);
		}
		if (obj is ZapCoil)
		{
			zapCoils.Remove(obj as ZapCoil);
		}
		if (obj is VoidSpawn)
		{
			voidSpawns.Remove(obj as VoidSpawn);
		}
		if (obj is RippleJelly)
		{
			rippleJellies.Remove(obj as RippleJelly);
		}
		if (obj is PhysicalObject)
		{
			physicalObjects[(obj as PhysicalObject).collisionLayer].Remove(obj as PhysicalObject);
			if (obj is OracleSwarmer)
			{
				SwarmerCount--;
			}
			if ((obj as PhysicalObject).graphicsModule != null)
			{
				drawableObjects.Remove((obj as PhysicalObject).graphicsModule);
				if ((obj as PhysicalObject).graphicsModule is PlayerGraphics { useSimpleCamo: false })
				{
					watcherCamoTaken = false;
				}
			}
			if (obj.slatedForDeletetion || obj.room != null)
			{
				for (int i = 0; i < abstractRoom.entities.Count; i++)
				{
					if ((obj as PhysicalObject).abstractPhysicalObject == abstractRoom.entities[i])
					{
						abstractRoom.entities[i].Destroy();
						abstractRoom.entities.RemoveAt(i);
						break;
					}
				}
				if (obj is Creature)
				{
					for (int j = 0; j < abstractRoom.creatures.Count; j++)
					{
						if ((obj as Creature).abstractCreature == abstractRoom.creatures[j])
						{
							abstractRoom.creatures.RemoveAt(j);
							break;
						}
					}
				}
			}
		}
		RemoveObject(obj);
	}

	public void ChangeCollisionLayerForObject(PhysicalObject obj, int newLayer)
	{
		int collisionLayer = obj.collisionLayer;
		if (collisionLayer != newLayer)
		{
			physicalObjects[collisionLayer].Remove(obj);
			physicalObjects[newLayer].Add(obj);
			obj.collisionLayer = newLayer;
		}
	}

	public void Update()
	{
		if (game == null)
		{
			return;
		}
		ticker++;
		if (snowSources.Count == 0)
		{
			snow = false;
		}
		UpdateWindDirection();
		if (waitToEnterAfterFullyLoaded > 0 && fullyLoaded)
		{
			waitToEnterAfterFullyLoaded--;
		}
		lastBackgroundNoise = backgroundNoise;
		backgroundNoise = Mathf.Max(0f, backgroundNoise - 0.05f);
		if (game.pauseUpdate)
		{
			backgroundNoise = Mathf.Lerp(backgroundNoise, 0f, 0.05f);
		}
		if (aidataprepro != null)
		{
			aidataprepro.Update();
		}
		if (waterObject != null)
		{
			if (defaultWaterLevel == -2000 && waterObject.fWaterLevel < -400f && waterObject.fWaterLevel > -2000f)
			{
				waterObject.fWaterLevel = -2000f;
			}
			waterObject.Update();
		}
		socialEventRecognizer.Update();
		UpdateSentientRotEffect();
		if (ModManager.MSC && game.IsStorySession && game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && !abstractRoom.gate && !abstractRoom.shelter && world.region != null && world.region.name == "DS" && syncTicker % 200 == 199)
		{
			int num = 0;
			for (int i = 0; i < physicalObjects.Length; i++)
			{
				for (int j = 0; j < physicalObjects[i].Count; j++)
				{
					if (physicalObjects[i][j] is Snail)
					{
						num++;
					}
				}
			}
			if (num < 10)
			{
				int k = 0;
				IntVector2 intVector = new IntVector2(0, 0);
				for (; k < 100; k++)
				{
					int num2 = UnityEngine.Random.Range(1, Tiles.GetLength(0) - 1);
					int num3 = UnityEngine.Random.Range(1, Tiles.GetLength(1) - 1);
					if (!Tiles[num2, num3].Solid)
					{
						intVector = new IntVector2(num2, num3);
						break;
					}
				}
				if (intVector.x != 0 || intVector.y != 0)
				{
					AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Snail), null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), game.GetNewID());
					abstractCreature.saveCreature = false;
					abstractRoom.AddEntity(abstractCreature);
					abstractCreature.RealizeInRoom();
					AddObject(new ShockWave(new Vector2((float)intVector.x * 20f, (float)intVector.y * 20f), 300f, 0.2f, 15));
				}
			}
		}
		darkenLightsFactor = roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DarkenLights);
		if (DustStormIntensity > 0f)
		{
			float dustStormIntensity = DustStormIntensity;
			Shader.SetGlobalFloat(RainWorld.ShadPropDustWaveProgress, dustStormIntensity);
			dustStormIntensity = Mathf.InverseLerp(0.4f, 0.2f, dustStormIntensity);
			roomSettings.Clouds = cloudsNdarken.x + dustStormIntensity * (1f - cloudsNdarken.x);
			darkenLightsFactor = cloudsNdarken.y + dustStormIntensity * (1f - cloudsNdarken.y);
		}
		else
		{
			cloudsNdarken.x = roomSettings.Clouds;
			cloudsNdarken.y = darkenLightsFactor;
		}
		syncTicker++;
		if (ModManager.MSC && game.IsStorySession && game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && world.region != null && world.region.name == "VS" && !abstractRoom.name.ToLower().Contains("basement"))
		{
			if (roomSettings.Grime > 0.25f)
			{
				roomSettings.Grime = 0.25f;
				Shader.SetGlobalFloat(RainWorld.ShadPropGrime, roomSettings.Grime);
			}
			if (roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Hue) == null)
			{
				roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Hue, 0f, inherited: false));
			}
			if (roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Brightness) == null)
			{
				roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Brightness, 1f, inherited: false));
			}
			if (roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast) == null)
			{
				roomSettings.effects.Add(new RoomSettings.RoomEffect(RoomSettings.RoomEffect.Type.Contrast, 1f, inherited: false));
			}
			roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Hue).amount = (float)(syncTicker % 400) / 400f;
			roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Contrast).amount = 1f;
			roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Brightness).amount = 1f;
		}
		if (!game.pauseUpdate && BeingViewed && !abstractRoom.shelter && roomSettings.DangerType != RoomRain.DangerType.None && ceilingTiles.Length != 0 && UnityEngine.Random.value < gravity && (double)UnityEngine.Random.value > Math.Pow(1f - Mathf.Max((1f - world.rainCycle.CycleStartUp) * Mathf.InverseLerp(0f, 0.5f, roomSettings.CeilingDrips), Mathf.Pow(roomSettings.CeilingDrips, 7f)) * 0.05f, ceilingTiles.Length))
		{
			AddObject(new WaterDrip(MiddleOfTile(ceilingTiles[UnityEngine.Random.Range(0, ceilingTiles.Length)]) + new Vector2(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value), 9f), new Vector2(0f, 0f), waterColor: false));
		}
		lastWaterGlitterCycle = waterGlitterCycle;
		waterGlitterCycle -= 1f / 60f;
		if (shortcutsBlinking != null && !game.pauseUpdate)
		{
			for (int l = 0; l < shortcutsBlinking.GetLength(0); l++)
			{
				shortcutsBlinking[l, 0] = Mathf.Clamp(shortcutsBlinking[l, 0] - 1f / ((shortcutsBlinking[l, 1] > 0f) ? Mathf.Lerp(20f, shortcuts[l].length, 0.3f) : 10f), 0f, 1f);
				if (shortcutsBlinking[l, 1] > 0f)
				{
					shortcutsBlinking[l, 1] += 1f / Mathf.Lerp(20f, shortcuts[l].length, 0.3f);
					if (shortcutsBlinking[l, 1] > 1f)
					{
						shortcutsBlinking[l, 1] = 0f;
					}
					shortcutsBlinking[l, 2] = 0f;
				}
				if (shortcutsBlinking[l, 3] == 0f)
				{
					shortcutsBlinking[l, 2] = Mathf.Lerp(shortcutsBlinking[l, 2], 1f, 0.04f);
					if (UnityEngine.Random.value < 0.01f)
					{
						shortcutsBlinking[l, 3] = UnityEngine.Random.value * 20f;
					}
				}
				else if (shortcutsBlinking[l, 3] < 0f)
				{
					shortcutsBlinking[l, 3] = Mathf.Min(shortcutsBlinking[l, 3] + 1f, 0f);
				}
				else
				{
					shortcutsBlinking[l, 3] = Mathf.Max(shortcutsBlinking[l, 3] - 1f, 0f);
					shortcutsBlinking[l, 2] = Mathf.Lerp(shortcutsBlinking[l, 2], UnityEngine.Random.value, Mathf.Lerp(0f, 0.5f, UnityEngine.Random.value));
				}
			}
		}
		if (fliesRoomAi != null)
		{
			fliesRoomAi.Update(game.evenUpdate);
		}
		if (PERTILEVISALIZER != null)
		{
			PERTILEVISALIZER.Update(this);
		}
		if (!game.pauseUpdate)
		{
			abstractRoom.UpdateCreaturesInDens(1);
		}
		for (updateIndex = updateList.Count - 1; updateIndex >= 0; updateIndex--)
		{
			UpdatableAndDeletable updatableAndDeletable = updateList[updateIndex];
			if (updatableAndDeletable.slatedForDeletetion || updatableAndDeletable.room != this)
			{
				CleanOutObjectNotInThisRoom(updatableAndDeletable);
			}
			else
			{
				bool flag = ShouldBeDeferred(updatableAndDeletable);
				if ((!game.pauseUpdate || updatableAndDeletable is IRunDuringDialog) && !flag)
				{
					updatableAndDeletable.Update(game.evenUpdate);
				}
				if (updatableAndDeletable.slatedForDeletetion || updatableAndDeletable.room != this)
				{
					CleanOutObjectNotInThisRoom(updatableAndDeletable);
				}
				else if (updatableAndDeletable is PhysicalObject && !flag)
				{
					if ((updatableAndDeletable as PhysicalObject).graphicsModule != null)
					{
						(updatableAndDeletable as PhysicalObject).graphicsModule.Update();
						(updatableAndDeletable as PhysicalObject).GraphicsModuleUpdated(actuallyViewed: true, game.evenUpdate);
					}
					else
					{
						(updatableAndDeletable as PhysicalObject).GraphicsModuleUpdated(actuallyViewed: false, game.evenUpdate);
					}
				}
			}
		}
		if (ModManager.DLCShared && roomSettings.GetEffect(DLCSharedEnums.RoomEffectType.RoomWrap) != null)
		{
			foreach (AbstractCreature player2 in game.Players)
			{
				if (player2.realizedCreature == null || player2.realizedCreature.room != this)
				{
					continue;
				}
				Player player = player2.realizedCreature as Player;
				if (player.mainBodyChunk.pos.x < -228f)
				{
					player.SuperHardSetPosition(new Vector2(RoomRect.right + 212f, player.mainBodyChunk.pos.y));
				}
				if (player.mainBodyChunk.pos.x > RoomRect.right + 228f)
				{
					player.SuperHardSetPosition(new Vector2(-212f, player.mainBodyChunk.pos.y));
				}
				if (player.mainBodyChunk.pos.y > RoomRect.top + 48f)
				{
					player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, RoomRect.bottom - 72f));
				}
				if (player.mainBodyChunk.pos.y < RoomRect.bottom - 96f)
				{
					player.SuperHardSetPosition(new Vector2(player.mainBodyChunk.pos.x, RoomRect.top + 32f));
					for (int m = 0; m < player.bodyChunks.Length; m++)
					{
						player.bodyChunks[m].vel.y = Mathf.Clamp(player.bodyChunks[m].vel.y, -15f, 15f);
					}
				}
			}
		}
		updateIndex = int.MaxValue;
		if (chunkGlue != null)
		{
			foreach (ChunkGlue item in chunkGlue)
			{
				item.moveChunk.pos = item.otherChunk.pos + item.relativePos;
			}
		}
		chunkGlue = null;
		for (int n = 1; n < physicalObjects.Length; n++)
		{
			for (int num4 = 0; num4 < physicalObjects[n].Count; num4++)
			{
				for (int num5 = num4 + 1; num5 < physicalObjects[n].Count; num5++)
				{
					if ((physicalObjects[n][num4].abstractPhysicalObject.rippleLayer != physicalObjects[n][num5].abstractPhysicalObject.rippleLayer && !physicalObjects[n][num4].abstractPhysicalObject.rippleBothSides && !physicalObjects[n][num5].abstractPhysicalObject.rippleBothSides) || !(Mathf.Abs(physicalObjects[n][num4].bodyChunks[0].pos.x - physicalObjects[n][num5].bodyChunks[0].pos.x) < physicalObjects[n][num4].collisionRange + physicalObjects[n][num5].collisionRange) || !(Mathf.Abs(physicalObjects[n][num4].bodyChunks[0].pos.y - physicalObjects[n][num5].bodyChunks[0].pos.y) < physicalObjects[n][num4].collisionRange + physicalObjects[n][num5].collisionRange))
					{
						continue;
					}
					bool flag2 = false;
					bool flag3 = false;
					if (physicalObjects[n][num4] is Creature && (physicalObjects[n][num4] as Creature).Template.grasps > 0)
					{
						Creature.Grasp[] grasps = (physicalObjects[n][num4] as Creature).grasps;
						foreach (Creature.Grasp grasp in grasps)
						{
							if (grasp != null && grasp.grabbed == physicalObjects[n][num5])
							{
								flag3 = true;
								break;
							}
						}
					}
					if (!flag3 && physicalObjects[n][num5] is Creature && (physicalObjects[n][num5] as Creature).Template.grasps > 0)
					{
						Creature.Grasp[] grasps = (physicalObjects[n][num5] as Creature).grasps;
						foreach (Creature.Grasp grasp2 in grasps)
						{
							if (grasp2 != null && grasp2.grabbed == physicalObjects[n][num4])
							{
								flag3 = true;
								break;
							}
						}
					}
					if (flag3)
					{
						continue;
					}
					for (int num7 = 0; num7 < physicalObjects[n][num4].bodyChunks.Length; num7++)
					{
						for (int num8 = 0; num8 < physicalObjects[n][num5].bodyChunks.Length; num8++)
						{
							if (physicalObjects[n][num4].bodyChunks[num7].collideWithObjects && physicalObjects[n][num5].bodyChunks[num8].collideWithObjects && Custom.DistLess(physicalObjects[n][num4].bodyChunks[num7].pos, physicalObjects[n][num5].bodyChunks[num8].pos, physicalObjects[n][num4].bodyChunks[num7].rad + physicalObjects[n][num5].bodyChunks[num8].rad))
							{
								float num9 = physicalObjects[n][num4].bodyChunks[num7].rad + physicalObjects[n][num5].bodyChunks[num8].rad;
								float num10 = Vector2.Distance(physicalObjects[n][num4].bodyChunks[num7].pos, physicalObjects[n][num5].bodyChunks[num8].pos);
								Vector2 vector = Custom.DirVec(physicalObjects[n][num4].bodyChunks[num7].pos, physicalObjects[n][num5].bodyChunks[num8].pos);
								float num11 = physicalObjects[n][num5].bodyChunks[num8].mass / (physicalObjects[n][num4].bodyChunks[num7].mass + physicalObjects[n][num5].bodyChunks[num8].mass);
								Vector2 vector2 = vector * ((num9 - num10) * num11);
								Vector2 vector3 = vector * ((num9 - num10) * (1f - num11));
								physicalObjects[n][num4].bodyChunks[num7].pos -= vector2;
								physicalObjects[n][num4].bodyChunks[num7].vel -= vector2;
								physicalObjects[n][num5].bodyChunks[num8].pos += vector3;
								physicalObjects[n][num5].bodyChunks[num8].vel += vector3;
								if (physicalObjects[n][num4].bodyChunks[num7].pos.x == physicalObjects[n][num5].bodyChunks[num8].pos.x)
								{
									physicalObjects[n][num4].bodyChunks[num7].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.0001f;
									physicalObjects[n][num5].bodyChunks[num8].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.0001f;
								}
								if (!flag2)
								{
									physicalObjects[n][num4].Collide(physicalObjects[n][num5], num7, num8);
									physicalObjects[n][num5].Collide(physicalObjects[n][num4], num8, num7);
								}
								flag2 = true;
							}
						}
					}
				}
			}
		}
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode)
		{
			ExpeditionGame.ExSpawn(this);
		}
		if (!ModManager.Watcher)
		{
			return;
		}
		if (!spawnedWeaver && world.weaverPresence != null && game.GetStorySession.voidWeaverEncountersThisCycle.Count == 0 && world.weaverPresence.weaverRoom == abstractRoom)
		{
			foreach (PlacedObject placedObject in roomSettings.placedObjects)
			{
				if (placedObject.type == WatcherEnums.PlacedObjectType.WeaverSpot)
				{
					spawnedWeaver = true;
					AddObject(new VoidWeaver(placedObject));
					break;
				}
			}
		}
		if (HasVoidWeaverEffects && !weaverPresenceInitialized && aimap != null)
		{
			InitializeWeaverPresenceInRoom();
		}
		if (weaverPresenceInitialized && !HasVoidWeaverEffects)
		{
			DestroyWeaverPresenceInRoom();
		}
		float effectAmount = roomSettings.GetEffectAmount(WatcherEnums.RoomEffectType.SentientRotInfection);
		if (!rotPresenceInitialized && effectAmount > 0f && aimap != null)
		{
			InitializeSentientRotPresenceInRoom(effectAmount);
		}
	}

	public bool ReallyTrulyRealizedInRoom(AbstractPhysicalObject obj)
	{
		if (obj.realizedObject == null)
		{
			return false;
		}
		if (obj.realizedObject.room != this)
		{
			return false;
		}
		if (obj.InDen)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntVector2 GetTilePosition(Vector2 pos)
	{
		return StaticGetTilePosition(pos);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntVector2 StaticGetTilePosition(Vector2 pos)
	{
		return new IntVector2((int)((pos.x + 20f) / 20f) - 1, (int)((pos.y + 20f) / 20f) - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int2 StaticGetTilePosition(float2 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		return new int2((int)((pos.x + 20f) / 20f) - 1, (int)((pos.y + 20f) / 20f) - 1);
	}

	public WorldCoordinate GetWorldCoordinate(Vector2 pos)
	{
		return GetWorldCoordinate(GetTilePosition(pos));
	}

	public WorldCoordinate GetWorldCoordinate(IntVector2 pos)
	{
		return Custom.MakeWorldCoordinate(pos, abstractRoom.index);
	}

	public Vector2 MiddleOfTile(int x, int y)
	{
		return new Vector2(10f + (float)x * 20f, 10f + (float)y * 20f);
	}

	public Vector2 MiddleOfTile(IntVector2 pos)
	{
		return MiddleOfTile(pos.x, pos.y);
	}

	public Vector2 MiddleOfTile(WorldCoordinate coord)
	{
		return MiddleOfTile(coord.x, coord.y);
	}

	public Vector2 MiddleOfTile(Vector2 pos)
	{
		return MiddleOfTile(GetTilePosition(pos));
	}

	public FloatRect TileRect(IntVector2 pos)
	{
		return FloatRect.MakeFromVector2(MiddleOfTile(pos) - new Vector2(10f, 10f), MiddleOfTile(pos) + new Vector2(10f, 10f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile GetTile(Vector2 pos)
	{
		return GetTile(GetTilePosition(pos));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile GetTile(IntVector2 pos)
	{
		return GetTile(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile GetTile(WorldCoordinate pos)
	{
		return GetTile(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Tile GetTile(int x, int y)
	{
		if (x < Width && y < Height && x > -1 && y > -1)
		{
			return Tiles[x, y];
		}
		if (DefaultTile != null)
		{
			return DefaultTile;
		}
		int num = Custom.IntClamp(x, 0, Width - 1);
		int num2 = Custom.IntClamp(y, 0, Height - 1);
		Tile tile = Tiles[num, num2];
		if (tile.Terrain != Tile.TerrainType.ShortcutEntrance && tile.shortCut == 0)
		{
			return tile;
		}
		return new Tile(tile.X, tile.Y, (tile.Terrain == Tile.TerrainType.ShortcutEntrance) ? Tile.TerrainType.Solid : tile.Terrain, tile.verticalBeam, tile.horizontalBeam, tile.wallbehind, 0, tile.waterInt);
	}

	public bool HasAnySolid(IntVector2 pos)
	{
		return HasAnySolid(pos.x, pos.y);
	}

	public bool HasAnySolid(int x, int y)
	{
		if (!GetTile(x, y).Solid)
		{
			return terrain?.ObstructsTile(x, y) ?? false;
		}
		return true;
	}

	public bool IsCornerFree(int x, int y, FloatRect.CornerLabel corner)
	{
		switch (corner)
		{
		case FloatRect.CornerLabel.A:
			if (!GetTile(x - 1, y).Solid && !GetTile(x - 1, y + 1).Solid)
			{
				return !GetTile(x, y + 1).Solid;
			}
			return false;
		case FloatRect.CornerLabel.B:
			if (!GetTile(x + 1, y).Solid && !GetTile(x + 1, y + 1).Solid)
			{
				return !GetTile(x, y + 1).Solid;
			}
			return false;
		case FloatRect.CornerLabel.C:
			if (!GetTile(x + 1, y).Solid && !GetTile(x + 1, y - 1).Solid)
			{
				return !GetTile(x, y - 1).Solid;
			}
			return false;
		case FloatRect.CornerLabel.D:
			if (!GetTile(x - 1, y).Solid && !GetTile(x - 1, y - 1).Solid)
			{
				return !GetTile(x, y - 1).Solid;
			}
			return false;
		default:
			return false;
		}
	}

	public bool IsCornerFree(int x, int y, int corner)
	{
		switch (corner)
		{
		case 0:
			if (!GetTile(x - 1, y).Solid && !GetTile(x - 1, y + 1).Solid)
			{
				return !GetTile(x, y + 1).Solid;
			}
			return false;
		case 1:
			if (!GetTile(x + 1, y).Solid && !GetTile(x + 1, y + 1).Solid)
			{
				return !GetTile(x, y + 1).Solid;
			}
			return false;
		case 2:
			if (!GetTile(x + 1, y).Solid && !GetTile(x + 1, y - 1).Solid)
			{
				return !GetTile(x, y - 1).Solid;
			}
			return false;
		case 3:
			if (!GetTile(x - 1, y).Solid && !GetTile(x - 1, y - 1).Solid)
			{
				return !GetTile(x, y - 1).Solid;
			}
			return false;
		default:
			return false;
		}
	}

	public int RayTraceTilesList(int x0, int y0, int x1, int y1, ref List<IntVector2> path)
	{
		int num = Math.Abs(x1 - x0);
		int num2 = Math.Abs(y1 - y0);
		int num3 = x0;
		int num4 = y0;
		int num5 = 1 + num + num2;
		int num6 = ((x1 > x0) ? 1 : (-1));
		int num7 = ((y1 > y0) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		if (path == null)
		{
			path = new List<IntVector2>();
		}
		int num9 = 0;
		while (num5 > 0)
		{
			if (path.Count <= num9)
			{
				path.Add(new IntVector2(num3, num4));
			}
			else
			{
				path[num9] = new IntVector2(num3, num4);
			}
			num9++;
			if (num8 > 0)
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
		return num9;
	}

	public bool RayTraceTilesForTerrain(int x0, int y0, int x1, int y1)
	{
		int num = Math.Abs(x1 - x0);
		int num2 = Math.Abs(y1 - y0);
		int num3 = x0;
		int num4 = y0;
		int num5 = 1 + num + num2;
		int num6 = ((x1 > x0) ? 1 : (-1));
		int num7 = ((y1 > y0) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		while (num5 > 0)
		{
			if (HasAnySolid(num3, num4))
			{
				return false;
			}
			if (num8 > 0)
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
		return true;
	}

	public bool IsPositionInsideBoundries(IntVector2 pos)
	{
		if (pos.x >= 0 && pos.x < TileWidth && pos.y >= 0)
		{
			return pos.y < TileHeight;
		}
		return false;
	}

	public IntVector2 ShorcutEntranceHoleDirection(IntVector2 pos)
	{
		IntVector2 result = new IntVector2(0, 0);
		for (int i = 0; i < 4; i++)
		{
			if (GetTile(pos.x + Custom.fourDirections[i].x, pos.y + Custom.fourDirections[i].y).Terrain != Tile.TerrainType.Solid)
			{
				result = new IntVector2(Custom.fourDirections[i].x, Custom.fourDirections[i].y);
				break;
			}
		}
		return result;
	}

	public AbstractRoom WhichRoomDoesThisExitLeadTo(IntVector2 exit)
	{
		int num = exitAndDenIndex.IndexfOf(exit);
		if (num > -1 && num < abstractRoom.connections.Length)
		{
			return world.GetAbstractRoom(abstractRoom.connections[num]);
		}
		return null;
	}

	public ShortcutData ShortcutLeadingToNode(int node)
	{
		for (int i = 0; i < shortcuts.Length; i++)
		{
			if (shortcuts[i].destNode == node)
			{
				return shortcuts[i];
			}
		}
		Custom.LogWarning("Shortcut leading to node error!", node.ToString());
		return shortcuts[0];
	}

	public bool VisualContact(WorldCoordinate a, WorldCoordinate b)
	{
		if (a.room != abstractRoom.index || b.room != abstractRoom.index)
		{
			return false;
		}
		return VisualContact(MiddleOfTile(a), MiddleOfTile(b));
	}

	public bool VisualContact(IntVector2 a, IntVector2 b)
	{
		return VisualContact(MiddleOfTile(a), MiddleOfTile(b));
	}

	public bool VisualContact(Vector2 a, Vector2 b)
	{
		if (GetTile(a).Solid || GetTile(b).Solid)
		{
			return false;
		}
		float num = Vector2.Distance(a, b);
		for (float num2 = 20f; num2 < num; num2 += 20f)
		{
			if (GetTile(Vector2.Lerp(a, b, num2 / num)).Solid)
			{
				return false;
			}
		}
		if (terrain != null)
		{
			for (float num3 = 20f; num3 < num; num3 += 20f)
			{
				if (terrain.Contains(Vector2.Lerp(a, b, num3 / num)))
				{
					return false;
				}
			}
		}
		return true;
	}

	public SlopeDirection IdentifySlope(Vector2 pos)
	{
		return IdentifySlope(GetTilePosition(pos));
	}

	public SlopeDirection IdentifySlope(int X, int Y)
	{
		return IdentifySlope(new IntVector2(X, Y));
	}

	public SlopeDirection IdentifySlope(IntVector2 pos)
	{
		if (GetTile(pos.x, pos.y).Terrain == Tile.TerrainType.Slope)
		{
			if (GetTile(pos.x - 1, pos.y).Terrain == Tile.TerrainType.Solid)
			{
				if (GetTile(pos.x, pos.y - 1).Terrain == Tile.TerrainType.Solid)
				{
					return SlopeDirection.UpRight;
				}
				if (GetTile(pos.x, pos.y + 1).Terrain == Tile.TerrainType.Solid)
				{
					return SlopeDirection.DownRight;
				}
			}
			else if (GetTile(pos.x + 1, pos.y).Terrain == Tile.TerrainType.Solid)
			{
				if (GetTile(pos.x, pos.y - 1).Terrain == Tile.TerrainType.Solid)
				{
					return SlopeDirection.UpLeft;
				}
				if (GetTile(pos.x, pos.y + 1).Terrain == Tile.TerrainType.Solid)
				{
					return SlopeDirection.DownLeft;
				}
			}
		}
		return SlopeDirection.Broken;
	}

	[Obsolete("Use two parameter function instead.")]
	public static IntVector2 DetermineSafeSpawnTile(Room realizedRoom)
	{
		return DetermineSafeSpawnTile(realizedRoom, forWarpPoint: false);
	}

	public static IntVector2 DetermineSafeSpawnTile(Room realizedRoom, bool forWarpPoint)
	{
		World world = realizedRoom.world;
		IntVector2 intVector = new IntVector2(0, 0);
		bool flag = false;
		if (realizedRoom.abstractRoom.shelter)
		{
			for (int i = 0; i < world.game.Players.Count; i++)
			{
				if (world.game.Players[i].Room.name == realizedRoom.abstractRoom.name)
				{
					intVector = new IntVector2(world.game.Players[i].pos.Tile.x, world.game.Players[i].pos.Tile.y + 1);
					flag = true;
					break;
				}
			}
		}
		if (!flag && !forWarpPoint)
		{
			int j = 0;
			Custom.Log("DetermineSafeSpawnTile guessing a good location to spawn in", realizedRoom.abstractRoom.name, "! [dry doors]");
			for (; j < realizedRoom.abstractRoom.nodes.Length && (realizedRoom.abstractRoom.nodes[j].type == AbstractRoomNode.Type.Exit || realizedRoom.abstractRoom.nodes[j].type == AbstractRoomNode.Type.Den); j++)
			{
			}
			for (int k = 0; k < 1000; k++)
			{
				int node = (int)(UnityEngine.Random.value * (float)j);
				ShortcutData shortcutData = realizedRoom.ShortcutLeadingToNode(node);
				intVector = shortcutData.StartTile + realizedRoom.ShorcutEntranceHoleDirection(shortcutData.StartTile) * (int)(UnityEngine.Random.value * 10f);
				intVector += Custom.PerpIntVec(realizedRoom.ShorcutEntranceHoleDirection(shortcutData.StartTile) * (int)(UnityEngine.Random.value * 6f - 3f));
				if (!realizedRoom.IsPositionInsideBoundries(intVector) || intVector.y <= 0 || realizedRoom.GetTile(intVector).Solid)
				{
					continue;
				}
				if (realizedRoom.abstractRoom.shelter)
				{
					int num = 0;
					if (!realizedRoom.GetTile(intVector + new IntVector2(0, -1)).Solid)
					{
						num++;
					}
					if (!realizedRoom.GetTile(intVector + new IntVector2(0, 1)).Solid)
					{
						num++;
					}
					if (!realizedRoom.GetTile(intVector + new IntVector2(-1, 0)).Solid)
					{
						num++;
					}
					if (!realizedRoom.GetTile(intVector + new IntVector2(1, 0)).Solid)
					{
						num++;
					}
					if (num >= 3)
					{
						flag = true;
						break;
					}
					continue;
				}
				bool flag2 = true;
				bool flag3 = realizedRoom.water && intVector.y <= realizedRoom.defaultWaterLevel;
				for (int l = -1; l < 2; l++)
				{
					if (!realizedRoom.GetTile(intVector + new IntVector2(l, -1)).Solid)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2 && !flag3)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			Custom.Log("[Fallback 2] DetermineSafeSpawnTile guessing a good location to spawn in", realizedRoom.abstractRoom.name, "! [dry solids]");
			List<IntVector2> list = new List<IntVector2>();
			for (int m = 0; m < realizedRoom.TileWidth; m++)
			{
				for (int n = ((!realizedRoom.water) ? 1 : (realizedRoom.defaultWaterLevel + 1)); n < realizedRoom.TileHeight; n++)
				{
					IntVector2 intVector2 = new IntVector2(m, n);
					if (!realizedRoom.IsPositionInsideBoundries(intVector2) || intVector2.y <= 0 || realizedRoom.GetTile(intVector2).Solid)
					{
						continue;
					}
					bool flag4 = true;
					for (int num2 = -1; num2 < 2; num2++)
					{
						if (!realizedRoom.GetTile(intVector2 + new IntVector2(num2, -1)).Solid)
						{
							flag4 = false;
							break;
						}
					}
					if (flag4)
					{
						list.Add(intVector2);
					}
				}
			}
			if (list.Count > 0)
			{
				intVector = list[UnityEngine.Random.Range(0, list.Count)];
				flag = true;
			}
		}
		if (!flag)
		{
			Custom.Log("[Fallback 3] DetermineSafeSpawnTile guessing a good location to spawn in", realizedRoom.abstractRoom.name, "! [ANY solids]");
			List<IntVector2> list2 = new List<IntVector2>();
			for (int num3 = 0; num3 < realizedRoom.TileWidth; num3++)
			{
				for (int num4 = 1; num4 < realizedRoom.TileHeight; num4++)
				{
					IntVector2 intVector3 = new IntVector2(num3, num4);
					if (realizedRoom.IsPositionInsideBoundries(intVector3) && intVector3.y > 0 && !realizedRoom.GetTile(intVector3).Solid && realizedRoom.GetTile(intVector3 + new IntVector2(0, -1)).Solid)
					{
						list2.Add(intVector3);
					}
				}
			}
			if (list2.Count > 0)
			{
				intVector = list2[UnityEngine.Random.Range(0, list2.Count)];
				flag = true;
			}
		}
		if (!flag)
		{
			Custom.Log("[Fallback 4] DetermineSafeSpawnTile guessing a good location to spawn in", realizedRoom.abstractRoom.name, "! [ANYWHERE]");
			List<IntVector2> list3 = new List<IntVector2>();
			for (int num5 = 0; num5 < realizedRoom.TileWidth; num5++)
			{
				for (int num6 = 1; num6 < realizedRoom.TileHeight; num6++)
				{
					IntVector2 intVector4 = new IntVector2(num5, num6);
					if (realizedRoom.IsPositionInsideBoundries(intVector4) && !realizedRoom.GetTile(intVector4).Solid)
					{
						list3.Add(intVector4);
					}
				}
			}
			if (list3.Count > 0)
			{
				intVector = list3[UnityEngine.Random.Range(0, list3.Count)];
				flag = true;
			}
		}
		return intVector;
	}

	public void LoadFromDataString(string[] lines)
	{
		RoomPreprocessor.VersionFix(ref lines);
		string[] array = lines[1].Split('*');
		if (lines[1].Split('|')[1] == "-1")
		{
			water = false;
			defaultWaterLevel = -1;
			floatWaterLevel = float.MinValue;
		}
		else
		{
			water = true;
			defaultWaterLevel = Convert.ToInt32(lines[1].Split('|')[1], CultureInfo.InvariantCulture);
			floatWaterLevel = MiddleOfTile(new IntVector2(0, defaultWaterLevel)).y;
			waterInFrontOfTerrain = Convert.ToInt32(lines[1].Split('|')[2], CultureInfo.InvariantCulture) == 1;
		}
		array = lines[1].Split('|')[0].Split('*');
		Width = Convert.ToInt32(array[0], CultureInfo.InvariantCulture);
		Height = Convert.ToInt32(array[1], CultureInfo.InvariantCulture);
		string[] array2 = lines[2].Split('|')[0].Split('*');
		lightAngle = new Vector2(Convert.ToSingle(array2[0], CultureInfo.InvariantCulture), Convert.ToSingle(array2[1], CultureInfo.InvariantCulture));
		string[] array3 = lines[3].Split('|');
		cameraPositions = new Vector2[array3.Length];
		for (int i = 0; i < array3.Length; i++)
		{
			cameraPositions[i] = new Vector2(Convert.ToSingle(array3[i].Split(',')[0], CultureInfo.InvariantCulture), 0f - (800f - (float)Height * 20f + Convert.ToSingle(array3[i].Split(',')[1])));
		}
		DefaultTile = null;
		if (world != null && game != null && abstractRoom.firstTimeRealized && (!game.IsArenaSession || game.GetArenaGameSession.GameTypeSetup.levelItems))
		{
			string[] array4 = lines[5].Split('|');
			for (int j = 0; j < array4.Length - 1; j++)
			{
				IntVector2 intVector = new IntVector2(Convert.ToInt32(array4[j].Split(',')[1], CultureInfo.InvariantCulture) - 1, Height - Convert.ToInt32(array4[j].Split(',')[2], CultureInfo.InvariantCulture));
				bool flag = true;
				if ((Convert.ToInt32(array4[j].Split(',')[0], CultureInfo.InvariantCulture) != 1) ? (UnityEngine.Random.value < 0.75f) : (UnityEngine.Random.value < 0.6f))
				{
					EntityID newID = game.GetNewID(-abstractRoom.index);
					switch (Convert.ToInt32(array4[j].Split(',')[0], CultureInfo.InvariantCulture))
					{
					case 0:
						abstractRoom.AddEntity(new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), newID));
						break;
					case 1:
						abstractRoom.AddEntity(new AbstractSpear(world, null, new WorldCoordinate(abstractRoom.index, intVector.x, intVector.y, -1), newID, explosive: false));
						break;
					}
				}
			}
		}
		Tiles = new Tile[Width, Height];
		for (int k = 0; k < Width; k++)
		{
			for (int l = 0; l < Height; l++)
			{
				Tiles[k, l] = new Tile(k, l, Tile.TerrainType.Air, vBeam: false, hBeam: false, wbhnd: false, 0, ((l <= defaultWaterLevel) ? 1 : 0) + ((l == defaultWaterLevel) ? 1 : 0));
			}
		}
		List<IntVector2> hiveTiles = new List<IntVector2>();
		List<IntVector2> list = new List<IntVector2>();
		List<int> hiveTileIndexes = new List<int>();
		int currentHiveTileIndex = -1;
		List<IntVector2> list2 = new List<IntVector2>();
		IntVector2 intVector2 = new IntVector2(0, Height - 1);
		List<IntVector2> list3 = new List<IntVector2>();
		string[] array5 = lines[11].Split('|');
		for (int m = 0; m < array5.Length - 1; m++)
		{
			string[] array6 = array5[m].Split(',');
			Tiles[intVector2.x, intVector2.y].Terrain = (Tile.TerrainType)int.Parse(array6[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			for (int n = 1; n < array6.Length; n++)
			{
				switch (array6[n])
				{
				case "1":
					Tiles[intVector2.x, intVector2.y].verticalBeam = true;
					break;
				case "2":
					Tiles[intVector2.x, intVector2.y].horizontalBeam = true;
					break;
				case "3":
					if (Tiles[intVector2.x, intVector2.y].shortCut < 1)
					{
						Tiles[intVector2.x, intVector2.y].shortCut = 1;
					}
					break;
				case "4":
					Tiles[intVector2.x, intVector2.y].shortCut = 2;
					break;
				case "5":
					Tiles[intVector2.x, intVector2.y].shortCut = 3;
					break;
				case "9":
					Tiles[intVector2.x, intVector2.y].shortCut = 4;
					break;
				case "12":
					Tiles[intVector2.x, intVector2.y].shortCut = 5;
					break;
				case "6":
					Tiles[intVector2.x, intVector2.y].wallbehind = true;
					break;
				case "7":
					AddHiveTile(ref hiveTiles, ref hiveTileIndexes, ref currentHiveTileIndex, intVector2);
					Tiles[intVector2.x, intVector2.y].hive = true;
					break;
				case "8":
					list2.Add(intVector2);
					break;
				case "10":
					list.Add(intVector2);
					break;
				case "11":
					list3.Add(intVector2);
					break;
				}
			}
			intVector2.y--;
			if (intVector2.y < 0)
			{
				intVector2.x++;
				intVector2.y = Height - 1;
			}
		}
		ModifyTileWaterLevels();
		if (list.Count > 0)
		{
			garbageHoles = list.ToArray();
		}
		if (currentHiveTileIndex > -1)
		{
			hives = new IntVector2[currentHiveTileIndex + 1][];
			for (int num = 0; num <= currentHiveTileIndex; num++)
			{
				int num2 = 0;
				for (int num3 = 0; num3 < hiveTileIndexes.Count; num3++)
				{
					if (hiveTileIndexes[num3] == num)
					{
						num2++;
					}
				}
				hives[num] = new IntVector2[num2];
				int num4 = 0;
				for (int num5 = 0; num5 < hiveTileIndexes.Count; num5++)
				{
					if (hiveTileIndexes[num5] == num)
					{
						hives[num][num4] = hiveTiles[num5];
						num4++;
					}
				}
			}
		}
		else
		{
			hives = new IntVector2[0][];
		}
		List<WaterFall> list4 = new List<WaterFall>();
		while (list2.Count > 0)
		{
			IntVector2 intVector3 = list2[0];
			for (int num6 = 0; num6 < list2.Count; num6++)
			{
				intVector3 = list2[num6];
				if (!list2.Contains(intVector3 + new IntVector2(-1, 0)) && !list2.Contains(intVector3 + new IntVector2(0, 1)))
				{
					break;
				}
			}
			bool flag2 = list2.Contains(intVector3 + new IntVector2(0, -1));
			int num7 = 0;
			for (int num8 = intVector3.x; num8 < TileWidth && list2.Contains(new IntVector2(num8, intVector3.y)) && list2.Contains(new IntVector2(num8, intVector3.y - 1)) == flag2; num8++)
			{
				num7++;
				list2.Remove(new IntVector2(num8, intVector3.y));
				if (flag2)
				{
					list2.Remove(new IntVector2(num8, intVector3.y - 1));
				}
			}
			list4.Add(new WaterFall(this, intVector3, flag2 ? 1f : 0.5f, num7));
		}
		waterFalls = list4.ToArray();
		for (int num9 = 0; num9 < waterFalls.Length; num9++)
		{
			AddObject(waterFalls[num9]);
		}
		if (list3.Count > 0)
		{
			AddObject(new WormGrass(this, list3));
		}
		Loaded();
	}

	public void ModifyTileWaterLevels()
	{
		foreach (PlacedObject placedObject in roomSettings.placedObjects)
		{
			if (!(placedObject.type == PlacedObject.Type.WaterCutoff))
			{
				continue;
			}
			WaterCutoffData waterCutoffData = placedObject.data as WaterCutoffData;
			Vector2 pos = ((waterCutoffData.handlePos.x > 0f) ? placedObject.pos : (placedObject.pos + waterCutoffData.handlePos));
			Vector2 pos2 = ((waterCutoffData.handlePos.x > 0f) ? (placedObject.pos + waterCutoffData.handlePos) : placedObject.pos);
			int num = Mathf.Max(GetTilePosition(pos).x, 0);
			int num2 = Mathf.Min(GetTilePosition(pos2).x, TileWidth - 1);
			for (int i = num; i <= num2; i++)
			{
				int y = GetTilePosition(new Vector2((float)i * 20f, Custom.LerpMap((float)i * 20f + 10f, pos.x, pos2.x, pos.y, pos2.y))).y;
				for (int j = y; j < TileHeight; j++)
				{
					if (Tiles[i, j].AnyWater)
					{
						Tiles[i, j].waterInt = ((j == y) ? 2 : 0);
					}
				}
			}
		}
		foreach (PlacedObject placedObject2 in roomSettings.placedObjects)
		{
			if (!(placedObject2.type == PlacedObject.Type.AirPocket))
			{
				continue;
			}
			AirPocketData airPocketData = placedObject2.data as AirPocketData;
			Vector2 pos3 = Vector2.Min(placedObject2.pos, placedObject2.pos + airPocketData.handlePos);
			Vector2 pos4 = Vector2.Max(placedObject2.pos, placedObject2.pos + airPocketData.handlePos);
			float y2 = pos3.y + airPocketData.waterLevel;
			int num3 = Mathf.Max(GetTilePosition(pos3).x, 0);
			int num4 = Mathf.Min(GetTilePosition(pos4).x, TileWidth - 1);
			int num5 = Mathf.Max(GetTilePosition(pos3).y, 0);
			int num6 = Mathf.Min(GetTilePosition(pos4).y, TileHeight - 1);
			int y3 = GetTilePosition(new Vector2(pos3.x, y2)).y;
			for (int k = num5; k <= num6; k++)
			{
				for (int l = num3; l <= num4; l++)
				{
					int num7 = 0;
					num7 = ((k <= y3) ? ((k < y3) ? 1 : 2) : 0);
					Tiles[l, k].waterInt = num7;
				}
			}
		}
	}

	public static void AddHiveTile(ref List<IntVector2> hiveTiles, ref List<int> hiveTileIndexes, ref int currentHiveTileIndex, IntVector2 tile)
	{
		int num = -1;
		for (int i = 0; i < hiveTiles.Count; i++)
		{
			if (num != -1)
			{
				break;
			}
			for (int j = 0; j < 8; j++)
			{
				if (tile + Custom.eightDirections[j] == hiveTiles[i])
				{
					num = hiveTileIndexes[i];
					break;
				}
			}
		}
		if (num == -1)
		{
			currentHiveTileIndex++;
			num = currentHiveTileIndex;
		}
		hiveTiles.Add(tile);
		hiveTileIndexes.Add(num);
	}

	public void AddWaterfall(IntVector2 tile, ref List<IntVector2> usedWFtiles)
	{
	}

	public bool ViewedByAnyCamera(Rect rect, float margin)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this && game.cameras[i].RectCurrentlyVisible(rect, margin, widescreen: false))
			{
				return true;
			}
		}
		return false;
	}

	public bool ViewedByAnyCamera(Vector2 pos, float margin)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this && game.cameras[i].PositionCurrentlyVisible(pos, margin, widescreen: false))
			{
				return true;
			}
		}
		return false;
	}

	public bool VisibleInAnyCameraScreenBounds(Vector2 pos)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this && game.cameras[i].IsVisibleAtCameraPosition(game.cameras[i].currentCameraPosition, pos))
			{
				return true;
			}
		}
		return false;
	}

	public void ScreenMovement(Vector2? pos, Vector2 bump, float shake)
	{
		if (ModManager.MMF && !MMF.cfgVanillaExploits.Value)
		{
			if (bump.magnitude > 50f)
			{
				bump = bump.normalized * 50f;
			}
			shake = Mathf.Clamp(shake, 0f, 20f);
		}
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].ScreenMovement(pos, bump, shake);
			}
		}
		if (!(bump.magnitude * 0.25f + shake * 3f > 2f) || ceilingTiles.Length == 0)
		{
			return;
		}
		for (int num = Math.Min(40, Mathf.RoundToInt(Mathf.Lerp(ceilingTiles.Length, 150f, 0.25f) / 100f * roomSettings.CeilingDrips * Mathf.Clamp(bump.magnitude * 0.25f + shake * 3f, 2f, 8f))); num > 0; num--)
		{
			Vector2 pos2 = MiddleOfTile(ceilingTiles[UnityEngine.Random.Range(0, ceilingTiles.Length)]) + new Vector2(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value), 9f);
			if (ViewedByAnyCamera(pos2, 300f))
			{
				AddObject(new WaterDrip(pos2, new Vector2(Mathf.Lerp(-1.5f, 1.5f, UnityEngine.Random.value), 0f), waterColor: false));
			}
		}
	}

	public ShortcutData shortcutData(IntVector2 pos)
	{
		int num = -1;
		num = ((!(pos == default(IntVector2)) && shortcutsIndex != null) ? shortcutsIndex.IndexfOf(pos) : (-1));
		if (num < 0 || num > shortcuts.Length)
		{
			return new ShortcutData(this, ShortcutData.Type.DeadEnd, 0, pos, pos, -1, new IntVector2[0]);
		}
		return shortcuts[num];
	}

	public ShortcutData shortcutData(Vector2 pos)
	{
		return shortcutData(GetTilePosition(pos));
	}

	public WorldCoordinate ToWorldCoordinate(Vector2 vector2)
	{
		return ToWorldCoordinate(GetTilePosition(vector2));
	}

	public WorldCoordinate ToWorldCoordinate(IntVector2 intVector2)
	{
		return new WorldCoordinate(abstractRoom.index, intVector2.x, intVector2.y, -1);
	}

	public WorldCoordinate LocalCoordinateOfNode(int node)
	{
		if (node > -1 && node < abstractRoom.nodes.Length)
		{
			if (abstractRoom.nodes[node].type == AbstractRoomNode.Type.Den || abstractRoom.nodes[node].type == AbstractRoomNode.Type.Exit || abstractRoom.nodes[node].type == AbstractRoomNode.Type.RegionTransportation)
			{
				return ShortcutLeadingToNode(node).startCoord;
			}
			if (abstractRoom.nodes[node].type == AbstractRoomNode.Type.SideExit || abstractRoom.nodes[node].type == AbstractRoomNode.Type.SkyExit || abstractRoom.nodes[node].type == AbstractRoomNode.Type.SeaExit)
			{
				RoomBorderExit roomBorderExit = borderExits[node - exitAndDenIndex.Length];
				return GetWorldCoordinate(roomBorderExit.borderTiles[UnityEngine.Random.Range(0, roomBorderExit.borderTiles.Length)]);
			}
			if (abstractRoom.nodes[node].type == AbstractRoomNode.Type.BatHive)
			{
				int num = node - exitAndDenIndex.Length - borderExits.Length;
				return GetWorldCoordinate(hives[num][UnityEngine.Random.Range(0, hives[num].Length)]);
			}
			if (abstractRoom.nodes[node].type == AbstractRoomNode.Type.GarbageHoles)
			{
				return GetWorldCoordinate(garbageHoles[UnityEngine.Random.Range(0, garbageHoles.Length)]);
			}
		}
		Custom.LogWarning("Unidentified entrance node!", node.ToString(), "/", abstractRoom.nodes.Length.ToString());
		return new WorldCoordinate(-1, -1, -1, -1);
	}

	public float DarknessOfPoint(RoomCamera rCam, Vector2 pos)
	{
		float num = Mathf.InverseLerp(0.2f, 1f, Darkness(pos));
		if (num <= 0f)
		{
			return 0f;
		}
		return num * (1f - LightSourceExposure(pos));
	}

	public float LightSourceExposure(Vector2 pos)
	{
		float num = 0f;
		for (int i = 0; i < lightSources.Count; i++)
		{
			if (Custom.DistLess(pos, lightSources[i].Pos, lightSources[i].Rad))
			{
				num += Custom.SCurve(Mathf.InverseLerp(lightSources[i].Rad, 0f, Vector2.Distance(pos, lightSources[i].Pos)), 0.5f) * lightSources[i].Lightness;
			}
		}
		return Mathf.Clamp(num, 0f, 1f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Color LightSourceColor(Vector2 pos)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i < lightSources.Count; i++)
		{
			if (Custom.DistLess(pos, lightSources[i].Pos, lightSources[i].Rad))
			{
				float num4 = Custom.SCurve(Mathf.InverseLerp(lightSources[i].Rad, 0f, Vector2.Distance(pos, lightSources[i].Pos)), 0.5f) * lightSources[i].Lightness;
				num = Mathf.Max(num, lightSources[i].color.r * num4);
				num2 = Mathf.Max(num2, lightSources[i].color.g * num4);
				num3 = Mathf.Max(num3, lightSources[i].color.b * num4);
			}
		}
		return new Color(num, num2, num3);
	}

	public bool CompleteDarkness(Vector2 pos, float margin, float reqDarkness, bool checkForPlayers)
	{
		if (Darkness(pos) < reqDarkness)
		{
			return false;
		}
		if (checkForPlayers)
		{
			for (int i = 0; i < game.Players.Count; i++)
			{
				if (game.Players[i].realizedCreature?.room == this && Custom.DistLess(pos, game.Players[i].realizedCreature.mainBodyChunk.pos, 60f + margin))
				{
					return false;
				}
			}
		}
		for (int j = 0; j < lightSources.Count; j++)
		{
			if (Custom.DistLess(pos, lightSources[j].Pos, lightSources[j].Rad + margin))
			{
				return false;
			}
		}
		return true;
	}

	public float WaterShinyness(Vector2 pos, float timeStacker)
	{
		float num = pos.x - pos.y;
		return Mathf.Pow(0.5f + Mathf.Sin((0f - Mathf.Lerp(lastWaterGlitterCycle, waterGlitterCycle, timeStacker) + num / 800f) * (float)Math.PI * 2f) * 0.5f, 7f);
	}

	public void PlaceQuantifiedCreaturesInRoom(CreatureTemplate.Type critType)
	{
		if (game == null)
		{
			return;
		}
		CreatureSpecificAImap creatureSpecificAImap = aimap.CreatureSpecificAImap(StaticWorld.GetCreatureTemplate(critType));
		for (int i = 0; i < abstractRoom.nodes.Length; i++)
		{
			int num = abstractRoom.NumberOfQuantifiedCreatureInNode(critType, i);
			if (world.rainCycle.CycleStartUp < 1f && critType != CreatureTemplate.Type.Fly && abstractRoom.nodes[i].type == AbstractRoomNode.Type.Den)
			{
				num = (int)(world.rainCycle.CycleStartUp * (float)num);
				for (int j = 0; j < abstractRoom.NumberOfQuantifiedCreatureInNode(critType, i) - num; j++)
				{
					AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(critType), null, new WorldCoordinate(abstractRoom.index, -1, -1, i), game.GetNewID());
					abstractRoom.entitiesInDens.Add(abstractCreature);
					abstractCreature.remainInDenCounter = UnityEngine.Random.Range(0, game.IsStorySession ? ((int)(4800f * (1f - world.rainCycle.CycleStartUp))) : 400);
				}
			}
			List<IntVector2> list = new List<IntVector2>();
			List<IntVector2> list2 = new List<IntVector2>();
			for (int k = 0; k < creatureSpecificAImap.accessableTiles.Length; k++)
			{
				if (creatureSpecificAImap.GetDistanceToExit(creatureSpecificAImap.accessableTiles[k].x, creatureSpecificAImap.accessableTiles[k].y, i) > 0)
				{
					list2.Add(creatureSpecificAImap.accessableTiles[k]);
				}
			}
			if (list2.Count == 0)
			{
				list2.Add(LocalCoordinateOfNode(i).Tile);
			}
			for (int l = 0; l < num; l++)
			{
				list.Add(list2[UnityEngine.Random.Range(0, list2.Count)]);
			}
			for (int m = 0; m < list.Count; m++)
			{
				for (int n = 0; n < 20; n++)
				{
					IntVector2 intVector = list2[UnityEngine.Random.Range(0, list2.Count)];
					if (PlaceQCScore(intVector, creatureSpecificAImap, i, critType, list) > PlaceQCScore(list[m], creatureSpecificAImap, i, critType, list))
					{
						list[m] = intVector;
					}
				}
			}
			foreach (IntVector2 item in list)
			{
				AbstractCreature abstractCreature2 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(critType), null, GetWorldCoordinate(item), game.GetNewID());
				abstractRoom.AddEntity(abstractCreature2);
				abstractCreature2.RealizeInRoom();
				if (critType == CreatureTemplate.Type.Spider && abstractRoom.nodes[i].type == AbstractRoomNode.Type.Den)
				{
					(abstractCreature2.realizedCreature as Spider).denPos = new WorldCoordinate(abstractRoom.index, -1, -1, i);
				}
			}
		}
	}

	public float PlaceQCScore(IntVector2 pos, CreatureSpecificAImap critMap, int node, CreatureTemplate.Type critType, List<IntVector2> QCPositions)
	{
		float num = 0f;
		if (critType == CreatureTemplate.Type.Fly)
		{
			num += (float)aimap.getAItile(pos).visibility;
			num += (float)aimap.getTerrainProximity(pos) * 0.2f;
			num += (float)aimap.getAItile(pos).floorAltitude * 0.1f;
			for (int i = 0; i < abstractRoom.nodes.Length; i++)
			{
				num /= (float)(critMap.GetDistanceToExit(pos.x, pos.y, i) + 10);
			}
			num /= Mathf.Lerp(critMap.GetDistanceToExit(pos.x, pos.y, node) + 10, 1f, 0.95f);
		}
		else if (critType == CreatureTemplate.Type.Leech)
		{
			num += (float)aimap.getAItile(pos).visibility;
			num += (float)aimap.getTerrainProximity(pos) * 0.2f;
		}
		else if (critType == CreatureTemplate.Type.SeaLeech)
		{
			num -= (float)Math.Abs(ShortcutLeadingToNode(node).StartTile.x - pos.x) * 10f;
		}
		else if (critType == CreatureTemplate.Type.Spider)
		{
			num += 1000f - Mathf.Abs(600f - (float)aimap.getAItile(pos).visibility) / 250f;
			num /= Mathf.Lerp(critMap.GetDistanceToExit(pos.x, pos.y, node) + 10, 200f, 0.999f);
		}
		return num;
	}

	public int CameraViewingNode(int node)
	{
		Vector2 a = MiddleOfTile(exitAndDenIndex[node]);
		int num = -1;
		float num2 = float.MaxValue;
		num = -1;
		for (int i = 0; i < cameraPositions.Length; i++)
		{
			if (Vector2.Distance(a, cameraPositions[i] + new Vector2(700f, 402f)) < num2)
			{
				num2 = Vector2.Distance(a, cameraPositions[i] + new Vector2(700f, 402f));
				num = i;
			}
		}
		if (num == -1)
		{
			num = 0;
		}
		return num;
	}

	public int CameraViewingPoint(Vector2 p)
	{
		for (int i = 0; i < cameraPositions.Length; i++)
		{
			if (p.x > cameraPositions[i].x && p.x < cameraPositions[i].x + 1366f && p.y > cameraPositions[i].y && p.y < cameraPositions[i].y + 768f)
			{
				return i;
			}
		}
		return -1;
	}

	public float Darkness(Vector2 pos)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				return game.cameras[i].PaletteDarkness();
			}
		}
		return 0f;
	}

	public void PlaySound(SoundID soundId)
	{
		PlaySound(soundId, 0f, 1f, 1f);
	}

	public void PlaySound(SoundID soundId, float pan, float vol, float pitch)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlaySound(soundId, pan, vol, pitch);
			}
		}
	}

	public void PlaySound(SoundID soundId, Vector2 pos)
	{
		PlaySound(soundId, pos, 1f, 1f);
	}

	public void PlaySound(SoundID soundId, Vector2 pos, AbstractPhysicalObject sourceObject)
	{
		if (sourceObject == null || game.ActiveRippleLayer == sourceObject.rippleLayer || sourceObject.rippleBothSides)
		{
			PlaySound(soundId, pos, 1f, 1f);
		}
	}

	public void PlaySound(SoundID soundId, Vector2 pos, float vol, float pitch)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlaySound(soundId, pos, vol, pitch);
			}
		}
	}

	public void PlaySound(SoundID soundId, Vector2 pos, float vol, float pitch, AbstractPhysicalObject sourceObject)
	{
		if (sourceObject == null || game.ActiveRippleLayer == sourceObject.rippleLayer || sourceObject.rippleBothSides)
		{
			PlaySound(soundId, pos, vol, pitch);
		}
	}

	public ChunkSoundEmitter PlaySound(SoundID soundId, BodyChunk chunk)
	{
		return PlaySound(soundId, chunk, loop: false, 1f, 1f);
	}

	public ChunkSoundEmitter PlaySound(SoundID soundId, BodyChunk chunk, bool loop, float vol, float pitch)
	{
		return PlaySound(soundId, chunk, loop, vol, pitch, randomStartPosition: false);
	}

	public ChunkSoundEmitter PlaySound(SoundID soundId, BodyChunk chunk, bool loop, float vol, float pitch, bool randomStartPosition)
	{
		if (game.ActiveRippleLayer != chunk.owner.abstractPhysicalObject.rippleLayer && !chunk.owner.abstractPhysicalObject.rippleBothSides)
		{
			vol = 0f;
		}
		ChunkSoundEmitter chunkSoundEmitter = new ChunkSoundEmitter(chunk, vol, pitch);
		PlaySound(soundId, chunkSoundEmitter, loop, vol, pitch, randomStartPosition);
		return chunkSoundEmitter;
	}

	public DisembodiedLoopEmitter PlayDisembodiedLoop(SoundID soundID, float vol, float pitch, float pan)
	{
		DisembodiedLoopEmitter disembodiedLoopEmitter = new DisembodiedLoopEmitter(vol, pitch, pan);
		AddObject(disembodiedLoopEmitter);
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayDisembodiedLoop(soundID, disembodiedLoopEmitter, pan, vol, pitch);
			}
		}
		return disembodiedLoopEmitter;
	}

	public RectSoundEmitter PlayRectSound(SoundID soundID, FloatRect rect, bool loop, float vol, float pitch)
	{
		RectSoundEmitter rectSoundEmitter = new RectSoundEmitter(rect, vol, pitch);
		PlaySound(soundID, rectSoundEmitter, loop, vol, pitch, randomStartPosition: false);
		return rectSoundEmitter;
	}

	public void PlaySound(SoundID soundId, PositionedSoundEmitter em, bool loop, float vol, float pitch, bool randomStartPosition)
	{
		AddObject(em);
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlaySound(soundId, em, loop, vol, pitch, randomStartPosition);
			}
		}
	}

	public GenericLoopEmitter PlayGenericSound(SoundID sound, float baseVolume, float basePitch, GenericLoopEmitter.Mixer mixer)
	{
		GenericLoopEmitter genericLoopEmitter = new GenericLoopEmitter(baseVolume, basePitch, mixer);
		AddObject(genericLoopEmitter);
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlaySound(sound, genericLoopEmitter);
			}
		}
		return genericLoopEmitter;
	}

	public void InGameNoise(InGameNoise noise)
	{
		if (noise == default(InGameNoise))
		{
			return;
		}
		for (int i = 0; i < abstractRoom.creatures.Count; i++)
		{
			if (abstractRoom.creatures[i].realizedCreature != null && abstractRoom.creatures[i].realizedCreature.room == this)
			{
				abstractRoom.creatures[i].realizedCreature.HeardNoise(noise);
			}
		}
		for (int j = 0; j < updateList.Count; j++)
		{
			if (updateList[j] != null && updateList[j] is IReactToNoises)
			{
				(updateList[j] as IReactToNoises).NoiseInRoom(noise);
			}
		}
	}

	public void NewMessageInRoom(string text, int extraLinger)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].hud != null && game.cameras[i].followAbstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && game.cameras[i].followAbstractCreature.Room == abstractRoom)
			{
				if (game.cameras[i].hud.dialogBox == null)
				{
					game.cameras[i].hud.InitDialogBox();
				}
				game.cameras[i].hud.dialogBox.NewMessage(text, extraLinger);
			}
		}
	}

	public void NewMessageInRoom(string text, float xOrientation, float yPos, int extraLinger)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].hud != null && game.cameras[i].followAbstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && game.cameras[i].followAbstractCreature.Room == abstractRoom)
			{
				if (game.cameras[i].hud.dialogBox == null)
				{
					game.cameras[i].hud.InitDialogBox();
				}
				game.cameras[i].hud.dialogBox.NewMessage(text, xOrientation, yPos, extraLinger);
			}
		}
	}

	public void TriggerCombatArena()
	{
		if (abstractRoom.isBattleArena)
		{
			Custom.Log("TRIGGLERED BATTLE ROOM", abstractRoom.name);
			abstractRoom.isBattleArena = false;
			abstractRoom.battleArenaTriggeredTime = 5;
		}
	}

	public void SpawnMultiplayerItem(PlacedObject placedObj)
	{
		if (!(UnityEngine.Random.value > (placedObj.data as PlacedObject.MultiplayerItemData).chance))
		{
			AbstractPhysicalObject.AbstractObjectType abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Rock;
			bool explosive = false;
			if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.Spear)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Spear;
			}
			else if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.ExplosiveSpear)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Spear;
				explosive = true;
			}
			else if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.Bomb)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.ScavengerBomb;
			}
			else if ((placedObj.data as PlacedObject.MultiplayerItemData).type == PlacedObject.MultiplayerItemData.Type.SporePlant)
			{
				abstractObjectType = AbstractPhysicalObject.AbstractObjectType.SporePlant;
			}
			if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Spear)
			{
				AbstractSpear item = new AbstractSpear(world, null, GetWorldCoordinate(placedObj.pos), game.GetNewID(), explosive);
				abstractRoom.entities.Add(item);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.Rock)
			{
				AbstractPhysicalObject item2 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, GetWorldCoordinate(placedObj.pos), game.GetNewID());
				abstractRoom.entities.Add(item2);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
			{
				AbstractPhysicalObject item3 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, GetWorldCoordinate(placedObj.pos), game.GetNewID());
				abstractRoom.entities.Add(item3);
			}
			else if (abstractObjectType == AbstractPhysicalObject.AbstractObjectType.SporePlant)
			{
				AbstractPhysicalObject item4 = new SporePlant.AbstractSporePlant(world, null, GetWorldCoordinate(placedObj.pos), game.GetNewID(), -2, -2, null, used: false, pacified: true);
				abstractRoom.entities.Add(item4);
			}
		}
	}

	public bool ShouldBeDeferred(UpdatableAndDeletable obj)
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		if (deferred && obj is PhysicalObject && !(obj is DangleFruit) && !(obj is Vulture))
		{
			AbstractCreature firstAlivePlayer = game.FirstAlivePlayer;
			PhysicalObject physicalObject = obj as PhysicalObject;
			if (physicalObject.grabbedBy.Count > 0)
			{
				for (int i = 0; i < physicalObject.grabbedBy.Count; i++)
				{
					if (ShouldBeDeferred(physicalObject.grabbedBy[i]?.grabber))
					{
						return true;
					}
				}
			}
			else if (game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && !firstAlivePlayer.realizedCreature.inShortcut && (Math.Abs(firstAlivePlayer.realizedCreature.mainBodyChunk.pos.x - physicalObject.firstChunk.pos.x) >= 2049f || Math.Abs(firstAlivePlayer.realizedCreature.mainBodyChunk.pos.y - physicalObject.firstChunk.pos.y) >= 1152f))
			{
				return true;
			}
		}
		return false;
	}

	public bool PointDeferred(Vector2 pos)
	{
		if (!ModManager.MSC || !deferred)
		{
			return false;
		}
		AbstractCreature firstAlivePlayer = game.FirstAlivePlayer;
		if (game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null && !firstAlivePlayer.realizedCreature.inShortcut)
		{
			if (!(Math.Abs(firstAlivePlayer.realizedCreature.mainBodyChunk.pos.x - pos.x) >= 2049f))
			{
				return Math.Abs(firstAlivePlayer.realizedCreature.mainBodyChunk.pos.y - pos.y) >= 1152f;
			}
			return true;
		}
		return false;
	}

	public void PausedUpdate()
	{
		for (int i = 0; i < updateList.Count; i++)
		{
			updateList[i].PausedUpdate();
		}
	}

	public void PlayCustomSound(string soundName, Vector2 pos, float vol, float pitch)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayCustomSound(soundName, pos, vol, pitch);
			}
		}
	}

	public void PlayCustomSoundDisembodied(string soundName, float pan, float vol, float pitch)
	{
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayCustomSoundDisembodied(soundName, pan, vol, pitch);
			}
		}
	}

	public DisembodiedLoopEmitter PlayCustomDisembodiedLoop(string soundName, float pan, float vol, float pitch)
	{
		DisembodiedLoopEmitter disembodiedLoopEmitter = new DisembodiedLoopEmitter(vol, pitch, pan);
		AddObject(disembodiedLoopEmitter);
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayCustomDisembodiedLoop(soundName, disembodiedLoopEmitter, pan, vol, pitch);
			}
		}
		return disembodiedLoopEmitter;
	}

	public ChunkSoundEmitter PlayCustomChunkSound(string soundName, BodyChunk chunk, float vol, float pitch)
	{
		ChunkSoundEmitter chunkSoundEmitter = new ChunkSoundEmitter(chunk, vol, pitch);
		AddObject(chunkSoundEmitter);
		for (int i = 0; i < game.cameras.Length; i++)
		{
			if (game.cameras[i].room == this)
			{
				game.cameras[i].virtualMicrophone.PlayCustomPositionedSound(soundName, chunkSoundEmitter, vol, pitch, loop: false, randomPosition: false);
			}
		}
		return chunkSoundEmitter;
	}

	public void SetLightSourceBlink(LightSource lightSource, int ind)
	{
		lightSource.setBlinkProperties((roomSettings.placedObjects[ind].data as PlacedObject.LightSourceData).blinkType, (roomSettings.placedObjects[ind].data as PlacedObject.LightSourceData).blinkRate);
		lightSource.nightLight = (roomSettings.placedObjects[ind].data as PlacedObject.LightSourceData).nightLight;
		if (lightSource.nightLight)
		{
			lightSource.nightFade = 0f;
		}
	}

	public void SetLightBeamBlink(LightBeam lightBeam, int ind)
	{
		lightBeam.SetBlinkProperties((roomSettings.placedObjects[ind].data as LightBeam.LightBeamData).blinkType, (roomSettings.placedObjects[ind].data as LightBeam.LightBeamData).blinkRate);
		lightBeam.nightLight = (roomSettings.placedObjects[ind].data as LightBeam.LightBeamData).nightLight;
		if (lightBeam.nightLight)
		{
			lightBeam.nightFade = 0f;
		}
	}

	public bool IsGateRoom()
	{
		if (abstractRoom.gate)
		{
			return true;
		}
		return roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.FakeGate) > 0f;
	}

	public void AddSnow()
	{
		if (snowObject == null)
		{
			snowObject = new Snow(this);
			drawableObjects.Add(snowObject);
			for (int i = 0; i < game.cameras.Length; i++)
			{
				if (game.cameras[i].room == this)
				{
					game.cameras[i].NewObjectInRoom(snowObject);
				}
			}
		}
		snow = true;
	}

	public void SlugcatGamemodeUniqueRoomSettings(RainWorldGame game)
	{
		if (game != null)
		{
			if (game.IsStorySession && game.TimelinePoint == SlugcatStats.Timeline.Saint)
			{
				roomSettings.wetTerrain = false;
				roomSettings.CeilingDrips = 0f;
				roomSettings.WaveAmplitude = 0.0001f;
				roomSettings.WaveLength = 1f;
				roomSettings.WaveSpeed = 0.51f;
				roomSettings.SecondWaveAmplitude = 0.0001f;
				roomSettings.SecondWaveLength = 1f;
			}
			if (ModManager.MSC && game.rainWorld.safariMode)
			{
				roomSettings.roomSpecificScript = false;
			}
		}
	}

	public float WaterLevelDisplacement(Vector2 pos)
	{
		if (waterInverted)
		{
			return 0f - (FloatWaterLevel(pos) - pos.y);
		}
		return FloatWaterLevel(pos) - pos.y;
	}

	public bool PointSubmerged(Vector2 pos, float yDisplacement)
	{
		if (ModManager.DLCShared && waterInverted)
		{
			if (waterObject != null)
			{
				return pos.y > waterObject.DetailedWaterLevel(pos) + yDisplacement;
			}
			return pos.y > floatWaterLevel + yDisplacement;
		}
		if (waterObject != null)
		{
			return pos.y < waterObject.DetailedWaterLevel(pos) - yDisplacement;
		}
		return pos.y < floatWaterLevel - yDisplacement;
	}

	public Vector2 FindGroundBelow(Vector2 pos, out Vector2 dir, float maxDist = 200f)
	{
		dir = Vector2.up;
		pos = SharedPhysics.ExactTerrainRayTracePos(this, pos + new Vector2(0f, 20f), pos + new Vector2(0f, 0f - maxDist)) ?? (pos + new Vector2(0f, 0f - maxDist));
		if (terrain != null)
		{
			Vector2 normal;
			Vector2 vector = terrain.SnapToTerrain(pos, 0f, out normal, new Vector2(pos.x, float.MaxValue));
			if (!GetTile(vector).Solid && vector.y > pos.y - 20f)
			{
				dir = normal;
				pos = vector;
			}
		}
		return pos;
	}

	public bool HasAnySolidTileInXRange(int y, int xA, int xB)
	{
		if (xA > -1 && xA < Width && xB > -1 && xB < Width && y > -1 && y < Height)
		{
			for (int i = xA; i <= xB; i++)
			{
				if (Tiles[i, y].IsSolid())
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = xA; j <= xB; j++)
			{
				if (GetTile(j, y).IsSolid())
				{
					return true;
				}
			}
		}
		return false;
	}
}
