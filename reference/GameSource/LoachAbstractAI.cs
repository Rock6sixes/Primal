using System.Collections.Generic;
using UnityEngine;
using Watcher;

public class LoachAbstractAI : AbstractCreatureAI
{
	public int dontGoToThisRoom = -1;

	public int timeInRoom;

	public int currentIndex = -1;

	public const int stayInRoomTime = 1500;

	public LoachAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		timeInRoom = 1500;
	}

	public override void NewWorld(World newWorld)
	{
		base.NewWorld(newWorld);
	}

	public override void AbstractBehavior(int time)
	{
		if (path.Count > 0 && parent.realizedCreature == null)
		{
			FollowPath(time);
		}
		else if (world.rainCycle.TimeUntilRain < 800)
		{
			GoToDen();
		}
		else
		{
			if (world.sideAccessNodes.Length == 0)
			{
				return;
			}
			timeInRoom += time;
			if (timeInRoom >= 1500)
			{
				dontGoToThisRoom = parent.pos.room;
				int num = PickNextDestination();
				if (num != currentIndex)
				{
					timeInRoom -= 1500;
					currentIndex = num;
					GoToRoom(num);
				}
			}
		}
	}

	public int PickNextDestination()
	{
		string[] dehardcodedRooms = GetDehardcodedRooms();
		List<(int, float)> list = new List<(int, float)>();
		float num = 0f;
		string[] array = dehardcodedRooms;
		foreach (string room in array)
		{
			AbstractRoom abstractRoom = world.GetAbstractRoom(room);
			if (dontGoToThisRoom != abstractRoom.index)
			{
				float num2 = Mathf.Pow(abstractRoom.SizeDependentAttractionValueForCreature(parent), 2f) / (float)(1 + OtherLoachesInRoom(world.GetAbstractRoom(room)));
				num += num2;
				list.Add((abstractRoom.index, num2));
			}
		}
		float num3 = Random.value * num;
		foreach (var (result, num4) in list)
		{
			if (num3 < num4)
			{
				return result;
			}
			num3 -= num4;
		}
		return currentIndex;
	}

	public int OtherLoachesInRoom(AbstractRoom room)
	{
		int num = 0;
		foreach (AbstractCreature creature in room.creatures)
		{
			if (creature.abstractAI != this && creature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Loach)
			{
				num++;
			}
		}
		return num;
	}

	public string[] GetDehardcodedRooms()
	{
		List<string> list = new List<string>();
		bool flag = true;
		if (world.singleRoomWorld && world.game.IsArenaSession)
		{
			flag = false;
		}
		for (int i = world.firstRoomIndex; i < world.firstRoomIndex + (world.NumberOfRooms - 1); i++)
		{
			if (RoomViableRoamDestination(i) && (!flag || world.GetAbstractRoom(i).offScreenDen || world.GetAbstractRoom(i).AttractionForCreature(parent) == AbstractRoom.CreatureRoomAttraction.Stay))
			{
				list.Add(world.GetAbstractRoom(i).name);
			}
		}
		return list.ToArray();
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
			if (world.GetAbstractRoom(room).nodes[i].type == AbstractRoomNode.Type.SideExit && world.GetAbstractRoom(room).nodes[i].entranceWidth > 5)
			{
				list.Add(i);
			}
		}
		if (list.Count != 0)
		{
			SetDestination(new WorldCoordinate(room, -1, -1, list[Random.Range(0, list.Count)]));
		}
	}

	public bool RoomViableRoamDestination(int room)
	{
		if (world.GetAbstractRoom(room).offScreenDen && world.game.IsArenaSession && world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge)
		{
			return false;
		}
		if (world.GetAbstractRoom(room).offScreenDen)
		{
			return true;
		}
		if (world.GetAbstractRoom(room).AttractionForCreature(parent) == AbstractRoom.CreatureRoomAttraction.Forbidden)
		{
			return false;
		}
		for (int i = 0; i < world.GetAbstractRoom(room).nodes.Length; i++)
		{
			if (world.GetAbstractRoom(room).nodes[i].type == AbstractRoomNode.Type.SideExit && world.GetAbstractRoom(room).nodes[i].entranceWidth > 5)
			{
				return true;
			}
		}
		return false;
	}
}
