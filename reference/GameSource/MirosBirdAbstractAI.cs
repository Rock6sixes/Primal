using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class MirosBirdAbstractAI : AbstractCreatureAI
{
	public int timeInRoom;

	public List<WorldCoordinate> allowedNodes;

	public int wait;

	public static string[] UGLYHARDCODEDALLOWEDROOMS = new string[7] { "SH_E01", "SH_E03", "SH_D03", "SH_H01", "SH_E04", "SB_G03", "SB_J01" };

	public MirosBirdAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		allowedNodes = new List<WorldCoordinate>();
		PopulateAllowedNodes(world);
	}

	public void PopulateAllowedNodes(World world)
	{
		allowedNodes.Clear();
		if (world.singleRoomWorld)
		{
			for (int i = 0; i < world.GetAbstractRoom(0).nodes.Length; i++)
			{
				if (world.GetAbstractRoom(0).nodes[i].type == AbstractRoomNode.Type.SideExit && world.GetAbstractRoom(0).nodes[i].entranceWidth >= 5)
				{
					allowedNodes.Add(new WorldCoordinate(0, -1, -1, i));
				}
			}
			return;
		}
		if (ModManager.MMF || ModManager.Watcher)
		{
			string[] dehardcodedRooms = GetDehardcodedRooms();
			for (int j = 0; j < dehardcodedRooms.Length; j++)
			{
				if (world.GetAbstractRoom(dehardcodedRooms[j]) == null)
				{
					continue;
				}
				int index = world.GetAbstractRoom(dehardcodedRooms[j]).index;
				for (int k = 0; k < world.GetAbstractRoom(index).nodes.Length; k++)
				{
					if (world.GetAbstractRoom(index).nodes[k].type == AbstractRoomNode.Type.SideExit && world.GetAbstractRoom(index).nodes[k].entranceWidth >= 5)
					{
						allowedNodes.Add(new WorldCoordinate(index, -1, -1, k));
					}
				}
			}
			return;
		}
		for (int l = 0; l < UGLYHARDCODEDALLOWEDROOMS.Length; l++)
		{
			if (world.GetAbstractRoom(UGLYHARDCODEDALLOWEDROOMS[l]) == null)
			{
				continue;
			}
			int index2 = world.GetAbstractRoom(UGLYHARDCODEDALLOWEDROOMS[l]).index;
			for (int m = 0; m < world.GetAbstractRoom(index2).nodes.Length; m++)
			{
				if (world.GetAbstractRoom(index2).nodes[m].type == AbstractRoomNode.Type.SideExit && world.GetAbstractRoom(index2).nodes[m].entranceWidth >= 5)
				{
					allowedNodes.Add(new WorldCoordinate(index2, -1, -1, m));
				}
			}
		}
	}

	public override void NewWorld(World newWorld)
	{
		base.NewWorld(newWorld);
		PopulateAllowedNodes(newWorld);
	}

	public override void AbstractBehavior(int time)
	{
		if (ModManager.ChallengeModule && parent.Room.world.game.IsArenaSession && parent.Room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge)
		{
			for (int i = 0; i < parent.world.NumberOfRooms; i++)
			{
				if (parent.world.GetAbstractRoom(parent.world.firstRoomIndex + i) != parent.world.offScreenDen)
				{
					SetDestination(new WorldCoordinate(parent.world.firstRoomIndex + i, -1, -1, 0));
					break;
				}
			}
		}
		if (wait > 0)
		{
			wait -= time;
			return;
		}
		if (ModManager.MMF && base.destination.room != parent.Room.index && parent.Room.world.game.IsStorySession)
		{
			bool flag = false;
			if (base.destination.room == parent.world.offScreenDen.index)
			{
				flag = true;
			}
			else
			{
				foreach (WorldCoordinate allowedNode in allowedNodes)
				{
					if (allowedNode.room == base.destination.room)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				Custom.LogWarning("Mirobird attempted to enter illegal room " + parent.world.GetAbstractRoom(base.destination.room).name + " " + parent);
				GoToDen();
			}
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
			if (path.Count == 0)
			{
				SetDestination(base.denPosition.Value);
			}
			if (parent.pos.room == world.offScreenDen.index)
			{
				Raid();
			}
		}
	}

	public void Raid()
	{
		int num = 0;
		for (int i = 0; i < parent.Room.creatures.Count; i++)
		{
			if (parent.Room.creatures[i].creatureTemplate.type == CreatureTemplate.Type.MirosBird && (parent.Room.creatures[i].abstractAI as MirosBirdAbstractAI).wait < 1 && parent.CanTravelTogether(parent.Room.creatures[i]))
			{
				if (parent.Room.creatures[i].personality.dominance > parent.personality.dominance)
				{
					return;
				}
				num++;
			}
		}
		if (num < 3)
		{
			int num2 = 0;
			for (int j = 0; j < parent.world.NumberOfRooms; j++)
			{
				foreach (AbstractCreature creature in parent.world.GetAbstractRoom(parent.world.firstRoomIndex + j).creatures)
				{
					if (creature.creatureTemplate.type == CreatureTemplate.Type.MirosBird && parent.CanTravelTogether(creature))
					{
						num2++;
					}
				}
			}
			if (num < num2)
			{
				return;
			}
		}
		if (allowedNodes.Count == 0)
		{
			return;
		}
		WorldCoordinate worldCoordinate = allowedNodes[Random.Range(0, allowedNodes.Count)];
		WorldCoordinate item = new WorldCoordinate(worldCoordinate.room, -1, -1, -1);
		for (int k = 0; k < world.GetAbstractRoom(worldCoordinate).nodes.Length; k++)
		{
			if (item.NodeDefined)
			{
				break;
			}
			if (k != worldCoordinate.abstractNode && world.GetAbstractRoom(worldCoordinate).nodes[k].type == AbstractRoomNode.Type.SideExit && world.GetAbstractRoom(worldCoordinate).nodes[k].entranceWidth >= 5 && world.GetAbstractRoom(worldCoordinate).ConnectionPossible(worldCoordinate.abstractNode, k, parent.creatureTemplate))
			{
				item.abstractNode = k;
			}
		}
		SetDestination(parent.pos);
		path.Clear();
		path.Add(base.denPosition.Value);
		if (item.NodeDefined)
		{
			path.Add(item);
		}
		path.Add(worldCoordinate);
		num = Random.Range(2, 5);
		for (int l = 0; l < parent.Room.creatures.Count; l++)
		{
			if (num <= 0)
			{
				break;
			}
			if (parent.Room.creatures[l].creatureTemplate.type == CreatureTemplate.Type.MirosBird && (parent.Room.creatures[l].abstractAI as MirosBirdAbstractAI).wait < 1 && parent.CanTravelTogether(parent.Room.creatures[l]))
			{
				parent.Room.creatures[l].abstractAI.SetDestination(parent.pos);
				parent.Room.creatures[l].abstractAI.path.Clear();
				parent.Room.creatures[l].abstractAI.path.Add(base.denPosition.Value);
				if (item.NodeDefined)
				{
					parent.Room.creatures[l].abstractAI.path.Add(item);
				}
				parent.Room.creatures[l].abstractAI.path.Add(worldCoordinate);
				(parent.Room.creatures[l].abstractAI as MirosBirdAbstractAI).wait = Random.Range(0, Random.Range(0, 50));
				num--;
			}
		}
	}

	public string[] GetDehardcodedRooms()
	{
		List<string> list = new List<string>();
		for (int i = world.firstRoomIndex; i < world.firstRoomIndex + (world.NumberOfRooms - 1); i++)
		{
			if (world.GetAbstractRoom(i).AttractionForCreature(parent) == AbstractRoom.CreatureRoomAttraction.Stay)
			{
				list.Add(world.GetAbstractRoom(i).name);
			}
		}
		return list.ToArray();
	}
}
