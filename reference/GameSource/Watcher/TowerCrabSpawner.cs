using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class TowerCrabSpawner : UpdatableAndDeletable
{
	public class Data : PlacedObject.Data
	{
		public class Mode : ExtEnum<Mode>
		{
			public static Mode Border = new Mode("Border", register: true);

			public static Mode Left = new Mode("Right", register: true);

			public static Mode Right = new Mode("Left", register: true);

			public Mode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public float frequency = 0.5f;

		public bool ignoreRain;

		public float minLayer;

		public float maxLayer;

		public Mode mode = Mode.Border;

		public Vector2 panelPos;

		public Data(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			try
			{
				string[] array = Regex.Split(s, "~");
				panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
				panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				frequency = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				ignoreRain = array[3] == "Y";
				minLayer = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
				maxLayer = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
				mode = new Mode(array[6]);
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
			}
			catch
			{
			}
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", panelPos.x, panelPos.y, frequency, ignoreRain ? 'Y' : 'N', minLayer, maxLayer, mode.value);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public PlacedObject pObj;

	public Vector2 lastPos;

	public Data.Mode lastMode;

	public WorldCoordinate entranceCoord;

	public WorldCoordinate exitCoord;

	public int spawnCooldown;

	public int maxSpawnCooldown = 40;

	public FloodFillPather prePathfinder;

	public bool shouldSpawnInitialCrabs;

	public List<DrillCrab> spawnedCrabs = new List<DrillCrab>();

	public const float averageCrabSpeed = 3.5f;

	public const float sideExitMargin = 600f;

	public float initialCrabX;

	public Dictionary<int, int> heightPerTile;

	public Data SpawnerData => pObj.data as Data;

	public float CrowdingSpawnMultiplier
	{
		get
		{
			float num = (room.PixelWidth + 1200f) / 3.5f;
			return Custom.LerpMap((float)spawnedCrabs.Count / num, 0.9f, 1.3f, 1f, 0f);
		}
	}

	public float RainSpawnMultiplier
	{
		get
		{
			if (SpawnerData.ignoreRain)
			{
				return 1f;
			}
			int num = (int)((room.PixelWidth + 1200f) / 3.5f);
			return Custom.LerpMap(room.world.rainCycle.TimeUntilRain - num, 2400f, 1200f, 1f, 0f);
		}
	}

	public float PanicSpawnMultiplier
	{
		get
		{
			int num = 0;
			foreach (AbstractCreature creature in room.abstractRoom.creatures)
			{
				if (creature.realizedCreature is DrillCrab { SpinningOut: not false })
				{
					num++;
					if (num >= 5)
					{
						break;
					}
				}
			}
			return 1f - (float)num / 5f;
		}
	}

	public float CombinedSpawnChance => SpawnerData.frequency * RainSpawnMultiplier * PanicSpawnMultiplier * CrowdingSpawnMultiplier;

	public TowerCrabSpawner(Room room, PlacedObject pObj)
	{
		base.room = room;
		this.pObj = pObj;
		spawnCooldown = Random.Range(0, maxSpawnCooldown);
		shouldSpawnInitialCrabs = true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if ((pObj.pos != lastPos || SpawnerData.mode != lastMode) && room.readyForAI)
		{
			lastPos = pObj.pos;
			lastMode = SpawnerData.mode;
			FindPathEndpoints();
		}
		if (shouldSpawnInitialCrabs)
		{
			room.waitToEnterAfterFullyLoaded = Mathf.Max(room.waitToEnterAfterFullyLoaded, 40);
		}
		if (prePathfinder == null || !prePathfinder.Done)
		{
			if (prePathfinder != null)
			{
				for (int i = 0; i < 500; i++)
				{
					prePathfinder.Update();
				}
			}
		}
		else
		{
			if (shouldSpawnInitialCrabs)
			{
				SpawnInitialCrabs();
			}
			if (spawnCooldown > 0)
			{
				spawnCooldown--;
			}
			if (spawnCooldown <= 0)
			{
				spawnCooldown = maxSpawnCooldown;
				if (entranceCoord != default(WorldCoordinate) && exitCoord != default(WorldCoordinate) && Random.value < CombinedSpawnChance)
				{
					if (SpawnerData.mode == Data.Mode.Border)
					{
						SpawnCrab();
					}
					else
					{
						SpawnCrab(entranceCoord);
					}
				}
			}
		}
		for (int num = spawnedCrabs.Count - 1; num >= 0; num--)
		{
			if (spawnedCrabs[num].slatedForDeletetion || spawnedCrabs[num].room != room || spawnedCrabs[num].dead)
			{
				spawnedCrabs.RemoveAt(num);
			}
		}
	}

	public List<IntVector2> GetPath()
	{
		int num = room.TileWidth * 2 + 100;
		IntVector2 intVector = entranceCoord.Tile;
		_ = room.aimap;
		List<IntVector2> list = new List<IntVector2> { intVector };
		while (intVector != exitCoord.Tile && num-- > 0)
		{
			IntVector2? nextPos = prePathfinder.GetNextPos(intVector);
			if (!nextPos.HasValue)
			{
				break;
			}
			IntVector2 valueOrDefault = nextPos.GetValueOrDefault();
			if (valueOrDefault.x != intVector.x)
			{
				list.Add(valueOrDefault);
			}
			intVector = valueOrDefault;
		}
		return list;
	}

	public void SpawnInitialCrabs()
	{
		float num;
		float num2;
		if (SpawnerData.mode == Data.Mode.Right)
		{
			num = (float)entranceCoord.x * 20f;
			num2 = (float)exitCoord.x * 20f;
		}
		else if (SpawnerData.mode == Data.Mode.Left)
		{
			num = (float)exitCoord.x * 20f;
			num2 = (float)entranceCoord.x * 20f;
		}
		else
		{
			bool num3 = exitCoord.x > entranceCoord.x;
			num = (num3 ? ((float)entranceCoord.x * 20f - 600f) : ((float)exitCoord.x * 20f));
			num2 = (num3 ? ((float)exitCoord.x * 20f) : ((float)entranceCoord.x * 20f + 600f));
		}
		if (heightPerTile == null)
		{
			initialCrabX = num;
			heightPerTile = new Dictionary<int, int>();
			foreach (IntVector2 item in GetPath())
			{
				heightPerTile[item.x] = item.y;
			}
		}
		while (initialCrabX < num2)
		{
			initialCrabX += 3.5f * (float)maxSpawnCooldown;
			if (Random.value < SpawnerData.frequency)
			{
				int num4 = Mathf.Clamp(Mathf.FloorToInt(initialCrabX / 20f), 0, room.TileWidth - 1);
				if (heightPerTile.TryGetValue(num4, out var value))
				{
					SpawnCrab(new WorldCoordinate(room.abstractRoom.index, num4, value, entranceCoord.abstractNode));
					break;
				}
			}
		}
		if (initialCrabX >= num2)
		{
			shouldSpawnInitialCrabs = false;
			heightPerTile = null;
		}
	}

	public void FindPathEndpoints()
	{
		WorldCoordinate worldCoordinate = new WorldCoordinate(room.abstractRoom.index, -1, -1, -1);
		WorldCoordinate worldCoordinate2 = new WorldCoordinate(room.abstractRoom.index, -1, -1, -1);
		if (SpawnerData.mode == Data.Mode.Border)
		{
			worldCoordinate.abstractNode = GetClosestSideExit(pObj.pos);
			worldCoordinate2.abstractNode = GetClosestSideExit(new Vector2((pObj.pos.x < room.PixelWidth / 2f) ? room.PixelWidth : 0f, pObj.pos.y));
			if (entranceCoord.abstractNode != worldCoordinate.abstractNode)
			{
				worldCoordinate.Tile = GetIdealBorderTile(worldCoordinate.abstractNode);
			}
			if (exitCoord.abstractNode != worldCoordinate2.abstractNode)
			{
				worldCoordinate2.Tile = GetIdealBorderTile(worldCoordinate2.abstractNode);
			}
		}
		else if (SpawnerData.mode == Data.Mode.Left)
		{
			worldCoordinate.Tile = room.GetTilePosition(pObj.pos);
			worldCoordinate2.abstractNode = GetClosestSideExit(new Vector2(0f, pObj.pos.y));
			worldCoordinate2.Tile = GetIdealBorderTile(worldCoordinate2.abstractNode);
		}
		else if (SpawnerData.mode == Data.Mode.Right)
		{
			worldCoordinate.Tile = room.GetTilePosition(pObj.pos);
			worldCoordinate2.abstractNode = GetClosestSideExit(new Vector2(room.PixelWidth, pObj.pos.y));
			worldCoordinate2.Tile = GetIdealBorderTile(worldCoordinate2.abstractNode);
		}
		if (entranceCoord != worldCoordinate || exitCoord != worldCoordinate2)
		{
			entranceCoord = worldCoordinate;
			exitCoord = worldCoordinate2;
			if (entranceCoord != default(WorldCoordinate) && exitCoord != default(WorldCoordinate))
			{
				prePathfinder = new FloodFillPather(exitCoord.Tile, room.aimap, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.TowerCrab));
			}
			else
			{
				prePathfinder = null;
			}
		}
	}

	public IntVector2 GetIdealBorderTile(int node)
	{
		RoomBorderExit roomBorderExit = room.borderExits[node - room.exitAndDenIndex.Length];
		int num = roomBorderExit.borderTiles[0].y;
		for (int i = 1; i < roomBorderExit.borderTiles.Length; i++)
		{
			num = Mathf.Min(num, roomBorderExit.borderTiles[i].y);
		}
		CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.TowerCrab);
		IntVector2? intVector = null;
		IntVector2[] borderTiles = roomBorderExit.borderTiles;
		for (int j = 0; j < borderTiles.Length; j++)
		{
			IntVector2 intVector2 = borderTiles[j];
			if (room.aimap.TileAccessibleToCreature(intVector2, creatureTemplate) && (!intVector.HasValue || Mathf.Abs(intVector2.y - num - 17) < Mathf.Abs(intVector.Value.y - num - 17)))
			{
				intVector = intVector2;
			}
		}
		return intVector ?? roomBorderExit.borderTiles[roomBorderExit.borderTiles.Length / 2];
	}

	public int GetClosestSideExit(Vector2 pos)
	{
		int result = -1;
		float num = float.PositiveInfinity;
		for (int i = 0; i < room.borderExits.Length; i++)
		{
			RoomBorderExit roomBorderExit = room.borderExits[i];
			if (roomBorderExit.type != AbstractRoomNode.Type.SideExit)
			{
				continue;
			}
			IntVector2[] borderTiles = roomBorderExit.borderTiles;
			foreach (IntVector2 pos2 in borderTiles)
			{
				float num2 = Vector2.Distance(room.MiddleOfTile(pos2), pos);
				if (num2 < num)
				{
					result = room.exitAndDenIndex.Length + i;
					num = num2;
				}
			}
		}
		return result;
	}

	public DrillCrab SpawnCrab(WorldCoordinate? pos = null)
	{
		WorldCoordinate worldCoordinate = pos ?? new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0);
		EntityID newID = room.game.GetNewID();
		CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.TowerCrab);
		AbstractCreature abstractCreature = new AbstractCreature(room.world, creatureTemplate, null, worldCoordinate, newID);
		abstractCreature.destroyOnAbstraction = true;
		abstractCreature.saveCreature = false;
		room.world.GetAbstractRoom(worldCoordinate).AddEntity(abstractCreature);
		DrillCrabAbstractAI obj = abstractCreature.abstractAI as DrillCrabAbstractAI;
		obj.SetRaidPath(entranceCoord, exitCoord);
		obj.pathFinderOverride = prePathfinder;
		(abstractCreature.state as DrillCrabState).layer = Random.Range(SpawnerData.minLayer, SpawnerData.maxLayer);
		if (worldCoordinate.room != entranceCoord.room)
		{
			abstractCreature.Move(entranceCoord);
		}
		else
		{
			abstractCreature.RealizeInRoom();
		}
		spawnedCrabs.Add(abstractCreature.realizedCreature as DrillCrab);
		return abstractCreature.realizedCreature as DrillCrab;
	}
}
