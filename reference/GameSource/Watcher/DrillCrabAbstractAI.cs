using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class DrillCrabAbstractAI : AbstractCreatureAI
{
	public FloodFillPather pathFinderOverride;

	public List<WorldCoordinate[]> allowedNodes;

	public int timeInRoom;

	public static HashSet<(string, int)> BLACKLISTEDSIDES = new HashSet<(string, int)>
	{
		("WBLA_B05", 4),
		("WBLA_A08", 5)
	};

	public bool Tower => parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.TowerCrab;

	public DrillCrabAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		allowedNodes = GetAllowedNodes();
	}

	public override void NewWorld(World newWorld)
	{
		base.NewWorld(newWorld);
		allowedNodes = GetAllowedNodes();
	}

	public override IntVector2[] PlaceInRealizedRoom()
	{
		if (pathFinderOverride != null)
		{
			return null;
		}
		return base.PlaceInRealizedRoom();
	}

	public override void AbstractBehavior(int time)
	{
		timeInRoom += time;
		if (parent.pos.room != lastRoom)
		{
			lastRoom = parent.pos.room;
			timeInRoom = 0;
		}
		if (path.Count > 0 && parent.realizedCreature == null)
		{
			FollowPath(time);
		}
		else if (world.rainCycle.TimeUntilRain < 800)
		{
			if (!base.denPosition.HasValue || !parent.pos.CompareDisregardingTile(base.denPosition.Value))
			{
				GoToDen();
			}
		}
		else if (allowedNodes.Count != 0)
		{
			if (path.Count == 0 && TimeInfluencedRandomRoll(0.001f, time) && timeInRoom > 1800)
			{
				SetDestination(base.denPosition.Value);
			}
			if (parent.pos.room == world.offScreenDen.index)
			{
				Raid();
			}
		}
	}

	public void SetRaidPath(WorldCoordinate entrance, WorldCoordinate exit)
	{
		SetDestination(base.denPosition.Value);
		path.Clear();
		path.Add(base.denPosition.Value);
		path.Add(exit);
		path.Add(entrance);
	}

	public void Raid()
	{
		if (allowedNodes.Count == 0)
		{
			return;
		}
		WorldCoordinate[] nodesInRandomRoom = GetNodesInRandomRoom();
		for (int i = 0; i < 3; i++)
		{
			if (Tower)
			{
				break;
			}
			if (!world.GetAbstractRoom(nodesInRandomRoom[0]).creatures.Any((AbstractCreature crit) => crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.DrillCrab))
			{
				break;
			}
			nodesInRandomRoom = GetNodesInRandomRoom();
		}
		WorldCoordinate worldCoordinate = nodesInRandomRoom[Random.Range(0, nodesInRandomRoom.Length)];
		AbstractRoom abstractRoom = world.GetAbstractRoom(worldCoordinate);
		if (Random.value < 0.5f || Tower)
		{
			WorldCoordinate item = new WorldCoordinate(worldCoordinate.room, -1, -1, -1);
			for (int j = 0; j < nodesInRandomRoom.Length; j++)
			{
				if (item.NodeDefined)
				{
					break;
				}
				if (nodesInRandomRoom[j] != worldCoordinate && abstractRoom.ConnectionPossible(worldCoordinate.abstractNode, nodesInRandomRoom[j].abstractNode, parent.creatureTemplate))
				{
					item = nodesInRandomRoom[j];
				}
			}
			if (item.NodeDefined)
			{
				SetDestination(base.denPosition.Value);
				path.Clear();
				path.Add(base.denPosition.Value);
				path.Add(item);
				path.Add(worldCoordinate);
				return;
			}
		}
		SetDestination(worldCoordinate);
		path.Clear();
		path.Add(worldCoordinate);
	}

	public List<WorldCoordinate[]> GetAllowedNodes()
	{
		bool blacklist = GetLowestAttraction() == AbstractRoom.CreatureRoomAttraction.Forbidden || !AnyAttractionsAssigned();
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		List<WorldCoordinate[]> list2 = new List<WorldCoordinate[]>();
		AbstractRoom[] abstractRooms = world.abstractRooms;
		foreach (AbstractRoom abstractRoom in abstractRooms)
		{
			if (!IsRoomAllowed(abstractRoom, blacklist))
			{
				continue;
			}
			for (int j = 0; j < abstractRoom.nodes.Length; j++)
			{
				if (IsNodeAllowed(abstractRoom, abstractRoom.nodes[j], j))
				{
					list.Add(new WorldCoordinate(abstractRoom.index, -1, -1, j));
				}
			}
			if (list.Count != 0)
			{
				list2.Add(list.ToArray());
				list.Clear();
			}
		}
		return list2;
	}

	public bool IsRoomAllowed(AbstractRoom room, bool blacklist)
	{
		if (room == room.world.offScreenDen)
		{
			return false;
		}
		AbstractRoom.CreatureRoomAttraction creatureRoomAttraction = room.AttractionForCreature(parent);
		if (blacklist)
		{
			return creatureRoomAttraction != AbstractRoom.CreatureRoomAttraction.Forbidden;
		}
		if (!(creatureRoomAttraction == AbstractRoom.CreatureRoomAttraction.Like))
		{
			return creatureRoomAttraction == AbstractRoom.CreatureRoomAttraction.Stay;
		}
		return true;
	}

	public bool IsNodeAllowed(AbstractRoom room, AbstractRoomNode node, int index)
	{
		if (node.type == AbstractRoomNode.Type.SideExit && node.entranceWidth >= 5 && node.entranceWidth != room.size.y)
		{
			return !BLACKLISTEDSIDES.Contains((room.name, index));
		}
		return false;
	}

	public AbstractRoom.CreatureRoomAttraction GetLowestAttraction()
	{
		AbstractRoom.CreatureRoomAttraction creatureRoomAttraction = AbstractRoom.CreatureRoomAttraction.Stay;
		float num = AbstractRoom.CreatureAttractionToFloat(creatureRoomAttraction);
		int index = parent.creatureTemplate.index;
		AbstractRoom[] abstractRooms = world.abstractRooms;
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			AbstractRoom.CreatureRoomAttraction creatureRoomAttraction2 = abstractRooms[i].roomAttractions[index];
			if (!(creatureRoomAttraction2 == null))
			{
				float num2 = AbstractRoom.CreatureAttractionToFloat(creatureRoomAttraction2);
				if (num2 < num)
				{
					creatureRoomAttraction = creatureRoomAttraction2;
					num = num2;
				}
			}
		}
		return creatureRoomAttraction;
	}

	public bool AnyAttractionsAssigned()
	{
		int index = parent.creatureTemplate.index;
		AbstractRoom[] abstractRooms = world.abstractRooms;
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			if (abstractRooms[i].roomAttractions[index] != null)
			{
				return true;
			}
		}
		return false;
	}

	public WorldCoordinate[] GetNodesInRandomRoom()
	{
		if (allowedNodes.Count == 0)
		{
			return null;
		}
		float num = allowedNodes.Sum((WorldCoordinate[] nodes) => GetRoomAttraction(nodes[0])) * Random.value;
		for (int i = 0; i < allowedNodes.Count; i++)
		{
			num -= GetRoomAttraction(allowedNodes[i][0]);
			if (num <= 0f)
			{
				return allowedNodes[i];
			}
		}
		return allowedNodes[allowedNodes.Count - 1];
	}

	public float GetRoomAttraction(WorldCoordinate coord)
	{
		AbstractRoom abstractRoom = world.GetAbstractRoom(coord);
		if (abstractRoom.AttractionForCreature(parent) == null)
		{
			return 0.5f;
		}
		return abstractRoom.AttractionValueForCreature(parent);
	}
}
