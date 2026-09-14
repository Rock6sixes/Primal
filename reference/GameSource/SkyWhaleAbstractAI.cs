using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;
using Watcher;

public class SkyWhaleAbstractAI : AbstractCreatureAI
{
	public struct CoordAndInt
	{
		public WorldCoordinate coord;

		public int i;

		public CoordAndInt(WorldCoordinate coord, int i)
		{
			this.coord = coord;
			this.i = i;
		}
	}

	public List<WorldCoordinate> checkRooms;

	public new int lastRoom;

	public int timeInRoom;

	public int dontGoToThisRoom = -1;

	public int timeToTire;

	public SkyWhaleNodePath nodePath;

	public static string[] BLACKLISTEDSKYS = new string[1] { "WPTA_D04" };

	public static (string, int)[] BLACKLISTEDSIDES = new(string, int)[1] { ("WVWB_D01", 4) };

	public HashSet<WorldCoordinate> bannedNodes = new HashSet<WorldCoordinate>();

	public SkyWhaleNodePath NodePath
	{
		get
		{
			if (nodePath != null && (nodePath.room != parent.pos.room || nodePath.destroy))
			{
				nodePath = null;
			}
			if (nodePath == null && world.GetAbstractRoom(parent.pos)?.realizedRoom != null)
			{
				nodePath = new SkyWhaleNodePath(this, parent.pos.room);
			}
			return nodePath;
		}
	}

	public SkyWhaleAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		timeToTire = Mathf.RoundToInt(1700f + 4000f * (1f - parent.personality.energy));
		checkRooms = new List<WorldCoordinate>();
		RefreshBannedNodes();
		AddRandomCheckRoom();
	}

	public override void NewWorld(World newWorld)
	{
		base.NewWorld(newWorld);
		RefreshBannedNodes();
		checkRooms.Clear();
		AddRandomCheckRoom();
	}

	public override void AbstractBehavior(int time)
	{
		if (path.Count > 0 && parent.realizedCreature == null)
		{
			FollowPath(time);
		}
		else
		{
			if (world.skyAccessNodes.Length == 0 && world.sideAccessNodes.Length == 0)
			{
				return;
			}
			if (checkRooms.Count > 0 && parent.pos.room == checkRooms[0].room)
			{
				timeInRoom += time;
				if (timeInRoom > timeToTire)
				{
					checkRooms.RemoveAt(0);
					timeInRoom -= timeToTire;
				}
			}
			else
			{
				GoToNextCheckRoom();
			}
		}
	}

	public void GoToNextCheckRoom()
	{
		if (checkRooms.Count > 0 && parent.pos.room == checkRooms[0].room)
		{
			checkRooms.RemoveAt(0);
		}
		if (checkRooms.Count == 0)
		{
			AddRandomCheckRoom();
		}
		if (checkRooms.Count != 0)
		{
			SetDestination(checkRooms[0]);
			return;
		}
		WorldCoordinate? worldCoordinate = base.denPosition;
		if (worldCoordinate.HasValue)
		{
			WorldCoordinate valueOrDefault = worldCoordinate.GetValueOrDefault();
			SetDestination(valueOrDefault);
		}
	}

	public bool CanUseSideExit(WorldCoordinate coord)
	{
		return !bannedNodes.Contains(coord.WashTileData());
	}

	public void RefreshBannedNodes()
	{
		bannedNodes.Clear();
		(string, int)[] bLACKLISTEDSIDES = BLACKLISTEDSIDES;
		for (int i = 0; i < bLACKLISTEDSIDES.Length; i++)
		{
			(string, int) tuple = bLACKLISTEDSIDES[i];
			string item = tuple.Item1;
			int item2 = tuple.Item2;
			AbstractRoom abstractRoom = world.GetAbstractRoom(item);
			if (abstractRoom != null)
			{
				bannedNodes.Add(new WorldCoordinate(abstractRoom.index, -1, -1, item2));
			}
		}
	}

	public void AddRandomCheckRoom()
	{
		bool flag = false;
		if (Random.value > 0.5f && parent.pos.room != dontGoToThisRoom && Random.value < world.GetAbstractRoom(parent.pos).AttractionValueForCreature(parent))
		{
			for (int i = 0; i < world.GetAbstractRoom(parent.pos).nodes.Length; i++)
			{
				if (world.GetAbstractRoom(parent.pos).nodes[i].type == AbstractRoomNode.Type.SideExit && CanUseSideExit(new WorldCoordinate(parent.pos.room, -1, -1, i)))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			dontGoToThisRoom = parent.pos.room;
			AddRoomClusterToCheckList(world.GetAbstractRoom(parent.pos));
			return;
		}
		float num = 0f;
		for (int j = 0; j < world.skyAccessNodes.Length; j++)
		{
			if (RoomViableRoamDestination(world.skyAccessNodes[j].room))
			{
				num += Mathf.Pow(world.GetAbstractRoom(world.skyAccessNodes[j]).SizeDependentAttractionValueForCreature(parent), 2f);
			}
		}
		float num2 = Random.value * num;
		for (int k = 0; k < world.skyAccessNodes.Length; k++)
		{
			if (RoomViableRoamDestination(world.skyAccessNodes[k].room))
			{
				float num3 = Mathf.Pow(world.GetAbstractRoom(world.skyAccessNodes[k]).SizeDependentAttractionValueForCreature(parent), 2f);
				if (num2 < num3)
				{
					AddRoomClusterToCheckList(world.GetAbstractRoom(world.skyAccessNodes[k]));
					break;
				}
				num2 -= num3;
			}
		}
		num = 0f;
		for (int l = 0; l < world.sideAccessNodes.Length; l++)
		{
			if (RoomViableRoamDestination(world.sideAccessNodes[l].room) && CanUseSideExit(world.sideAccessNodes[l]))
			{
				num += Mathf.Pow(world.GetAbstractRoom(world.sideAccessNodes[l]).SizeDependentAttractionValueForCreature(parent), 2f);
			}
		}
		num2 = Random.value * num;
		for (int m = 0; m < world.sideAccessNodes.Length; m++)
		{
			if (RoomViableRoamDestination(world.sideAccessNodes[m].room) && CanUseSideExit(world.sideAccessNodes[m]))
			{
				float num4 = Mathf.Pow(world.GetAbstractRoom(world.sideAccessNodes[m]).SizeDependentAttractionValueForCreature(parent), 2f);
				if (num2 < num4)
				{
					AddRoomClusterToCheckList(world.GetAbstractRoom(world.sideAccessNodes[m]));
					break;
				}
				num2 -= num4;
			}
		}
	}

	public WorldCoordinate GetRandomEntrance(AbstractRoom room)
	{
		int num = 0;
		for (int i = 0; i < room.nodes.Length; i++)
		{
			if (room.nodes[i].type == AbstractRoomNode.Type.SideExit && CanUseSideExit(new WorldCoordinate(room.index, -1, -1, i)))
			{
				num++;
			}
		}
		int num2 = Random.Range(0, num);
		for (int j = 0; j < room.nodes.Length; j++)
		{
			if (room.nodes[j].type == AbstractRoomNode.Type.SideExit && num2-- == 0)
			{
				return new WorldCoordinate(room.index, -1, -1, j);
			}
		}
		return new WorldCoordinate(room.index, -1, -1, room.RandomRelevantNode(parent.creatureTemplate));
	}

	public void AddRoomClusterToCheckList(AbstractRoom originalRoom)
	{
		List<CoordAndInt> list = new List<CoordAndInt>();
		if (originalRoom.index != parent.pos.room && (originalRoom.AnySkyAccess || originalRoom.AnySideAccess) && RoomViableRoamDestination(originalRoom.index))
		{
			list.Add(new CoordAndInt(GetRandomEntrance(originalRoom), 100000 + Random.Range(0, 50)));
		}
		for (int i = 0; i < originalRoom.connections.Length; i++)
		{
			if (originalRoom.connections[i] > -1 && RoomViableRoamDestination(originalRoom.connections[i]) && Random.value < world.GetAbstractRoom(originalRoom.connections[i]).AttractionValueForCreature(parent) * 1.5f)
			{
				AbstractRoom abstractRoom = world.GetAbstractRoom(originalRoom.connections[i]);
				if (abstractRoom.AnySkyAccess || originalRoom.AnySideAccess)
				{
					int num = Random.Range(50, 500 + 2000 * (int)abstractRoom.SizeDependentAttractionValueForCreature(parent));
					list.Add(new CoordAndInt(GetRandomEntrance(abstractRoom), num + Random.Range(0, 50)));
				}
			}
		}
		while (list.Count > 0)
		{
			int num2 = -1;
			int num3 = int.MinValue;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].coord.NodeDefined && list[j].i > num3)
				{
					num3 = list[j].i;
					num2 = j;
				}
			}
			if (num2 > -1)
			{
				checkRooms.Add(list[num2].coord);
			}
			list.RemoveAt(num2);
		}
	}

	public bool RoomViableRoamDestination(int room)
	{
		AbstractRoom abstractRoom = world.GetAbstractRoom(room);
		if (abstractRoom == null)
		{
			return false;
		}
		if (ModManager.ChallengeModule && abstractRoom.offScreenDen && world.game.IsArenaSession && world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge)
		{
			return false;
		}
		if (CheckBlacklist(abstractRoom.name))
		{
			return false;
		}
		if (abstractRoom.AttractionForCreature(parent) == AbstractRoom.CreatureRoomAttraction.Forbidden)
		{
			return false;
		}
		for (int i = 0; i < abstractRoom.nodes.Length; i++)
		{
			if (abstractRoom.nodes[i].type == AbstractRoomNode.Type.SideExit && abstractRoom.nodes[i].entranceWidth > 4)
			{
				return true;
			}
		}
		return false;
	}

	public override IntVector2[] PlaceInRealizedRoom()
	{
		AbstractRoom abstractRoom = world.GetAbstractRoom(parent.pos);
		if (abstractRoom == null)
		{
			Custom.LogWarning("TRYING TO PLACE IN NULL ABSTRACT ROOM");
			return null;
		}
		Room realizedRoom = abstractRoom.realizedRoom;
		if (realizedRoom == null)
		{
			Custom.LogWarning("TRYING TO PLACE IN REALIZED ROOM WITH NO REALIZED ROOM");
			return null;
		}
		WorldCoordinate pos = parent.pos;
		WorldCoordinate worldCoordinate = new WorldCoordinate(parent.pos.room, -1, -1, -1);
		if (!parent.pos.TileDefined)
		{
			pos.Tile = realizedRoom.LocalCoordinateOfNode(parent.pos.abstractNode).Tile;
		}
		pos.x = Custom.IntClamp(pos.x, 0, realizedRoom.TileWidth - 1);
		pos.y = Custom.IntClamp(pos.y, 0, realizedRoom.TileHeight - 1);
		if (NodePath.PathfindingNodes.Count == 0 || NodePath.usedPathfindingNodes.Count > 0)
		{
			NodePath.BuildPath(pos, fromEntrance: true);
		}
		if (NodePath.PathfindingNodes.Count > 0)
		{
			foreach (AbstractCreature creature in abstractRoom.creatures)
			{
				if (creature != parent && creature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SkyWhale && creature.pos.TileDefined && creature.abstractAI != null && Mathf.Abs(creature.abstractAI.timeBuffer - timeBuffer) < 10)
				{
					timeBuffer += 100;
				}
			}
			int num = (int)((float)timeBuffer * parent.creatureTemplate.offScreenSpeed * offscreenSpeedFac);
			worldCoordinate.Tile = realizedRoom.GetTilePosition(NodePath.CurrentNode.owner.pos);
			while (num > 0 && NodePath.CurrentNode != null && pos.TileDefined && worldCoordinate.TileDefined)
			{
				worldCoordinate.Tile = realizedRoom.GetTilePosition(NodePath.CurrentNode.owner.pos);
				QuickPathFinder quickPathFinder = new QuickPathFinder(pos.Tile, worldCoordinate.Tile, realizedRoom.aimap, parent.creatureTemplate);
				while (quickPathFinder.status == 0)
				{
					quickPathFinder.Update();
				}
				if (quickPathFinder.status != 1)
				{
					break;
				}
				QuickPath quickPath = quickPathFinder.ReturnPath();
				if (quickPath.Length == 0)
				{
					NodePath.MoveToNextNode();
					worldCoordinate.Tile = realizedRoom.GetTilePosition(NodePath.CurrentNode.owner.pos);
					continue;
				}
				if (quickPath.Length <= num)
				{
					num -= quickPath.Length;
					pos.Tile = realizedRoom.GetTilePosition(NodePath.CurrentNode.owner.pos);
					NodePath.MoveToNextNode();
					if (NodePath.PathfindingNodes.Count == 0)
					{
						NodePath.EndOfPath(forceReset: false);
					}
					if (NodePath.PathfindingNodes.Count == 0)
					{
						parent.pos.Tile = quickPath.tiles[quickPath.tiles.Length - 1];
						timeInRoom = 0;
						GoToNextCheckRoom();
						int num2 = DestinationExit(realizedRoom.abstractRoom.index, parent.pos.abstractNode);
						if (num2 > -1)
						{
							worldCoordinate = realizedRoom.LocalCoordinateOfNode(num2);
						}
					}
					else
					{
						worldCoordinate.Tile = realizedRoom.GetTilePosition(NodePath.CurrentNode.owner.pos);
					}
					continue;
				}
				parent.pos.Tile = quickPath.tiles[num];
				return quickPath.tiles;
			}
		}
		return null;
	}

	public void GoToRoom(int room)
	{
		if (!RoomViableRoamDestination(room))
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < world.GetAbstractRoom(room).nodes.Length; i++)
		{
			if (world.GetAbstractRoom(room).nodes[i].type == AbstractRoomNode.Type.SideExit && world.GetAbstractRoom(room).nodes[i].entranceWidth > 4)
			{
				list.Add(i);
			}
		}
		if (list.Count != 0)
		{
			SetDestination(new WorldCoordinate(room, -1, -1, list[Random.Range(0, list.Count)]));
		}
	}

	public bool CheckBlacklist(string room)
	{
		return BLACKLISTEDSKYS.Contains(room);
	}
}
