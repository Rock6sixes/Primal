using System.Collections.Generic;
using UnityEngine;
using Watcher;

public class BigMothAbstractAI : AbstractCreatureAI
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

	public int timeInRoom;

	public int dontGoToThisRoom = -1;

	public AbstractCreature holdBaby;

	public int forceRealHoldTime;

	public BigMothAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		checkRooms = new List<WorldCoordinate>();
		AddRandomCheckRoom();
	}

	public override void NewWorld(World newWorld)
	{
		base.NewWorld(newWorld);
		checkRooms.Clear();
		AddRandomCheckRoom();
	}

	public override void AbstractBehavior(int time)
	{
		if (path.Count > 0 && parent.realizedCreature == null)
		{
			FollowPath(time);
			return;
		}
		if (world.skyAccessNodes.Length == 0)
		{
			if (parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SmallMoth)
			{
				base.AbstractBehavior(time);
			}
			return;
		}
		if (checkRooms.Count == 0)
		{
			AddRandomCheckRoom();
			return;
		}
		AbstractRoom abstractRoom = world.GetAbstractRoom(parent.pos);
		if (parent.creatureTemplate.type != WatcherEnums.CreatureTemplateType.SmallMoth && abstractRoom.shelter)
		{
			SetDestination(new WorldCoordinate(world.offScreenDen.index, -1, -1, 0));
			parent.Move(new WorldCoordinate(world.offScreenDen.index, -1, -1, 0));
			return;
		}
		if (parent.stuckObjects.Count == 0 && parent.creatureTemplate.type != WatcherEnums.CreatureTemplateType.SmallMoth)
		{
			AbstractCreature abstractCreature = null;
			for (int i = 0; i < abstractRoom.creatures.Count; i++)
			{
				if (abstractRoom.creatures[i].creatureTemplate.type == WatcherEnums.CreatureTemplateType.MothGrub && !abstractRoom.creatures[i].state.dead && abstractRoom.creatures[i].stuckObjects.Count == 0)
				{
					abstractCreature = abstractRoom.creatures[i];
					break;
				}
			}
			if (abstractCreature != null)
			{
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractCreature, 0, carry: true);
			}
		}
		bool flag = false;
		for (int j = 0; j < abstractRoom.creatures.Count; j++)
		{
			if (abstractRoom.creatures[j].state.dead && abstractRoom.creatures[j].creatureTemplate.meatPoints >= 2)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			bool flag2 = false;
			if (parent.stuckObjects.Count == 0 && parent.creatureTemplate.type != WatcherEnums.CreatureTemplateType.SmallMoth)
			{
				AbstractRoom abstractRoom2 = world.GetAbstractRoom(world.offScreenDen.index);
				for (int k = 0; k < abstractRoom2.creatures.Count; k++)
				{
					if (abstractRoom2.creatures[k].creatureTemplate.type == WatcherEnums.CreatureTemplateType.MothGrub && !abstractRoom2.creatures[k].state.dead && abstractRoom2.creatures[k].stuckObjects.Count == 0)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				return;
			}
			checkRooms.Clear();
			checkRooms.Add(new WorldCoordinate(world.offScreenDen.index, -1, -1, 0));
			checkRooms.Add(parent.pos);
			SetDestination(new WorldCoordinate(world.offScreenDen.index, -1, -1, 0));
		}
		if (parent.pos.room == checkRooms[0].room)
		{
			timeInRoom += time;
			if (timeInRoom > 720)
			{
				checkRooms.RemoveAt(0);
				timeInRoom -= 720;
			}
			return;
		}
		if (parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SmallMoth)
		{
			parent.Move(checkRooms[0]);
		}
		else if (parent.pos.NodeDefined && world.GetNode(parent.pos).type == AbstractRoomNode.Type.Den)
		{
			parent.Move(checkRooms[0]);
		}
		SetDestination(checkRooms[0]);
	}

	public void AddRandomCheckRoom()
	{
		bool flag = false;
		if (Random.value > 0.5f && parent.pos.room != dontGoToThisRoom && Random.value < world.GetAbstractRoom(parent.pos).AttractionValueForCreature(parent))
		{
			for (int i = 0; i < world.GetAbstractRoom(parent.pos).nodes.Length; i++)
			{
				if (world.GetAbstractRoom(parent.pos).nodes[i].type == AbstractRoomNode.Type.SkyExit)
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
	}

	public void AddRoomClusterToCheckList(AbstractRoom originalRoom)
	{
		List<CoordAndInt> list = new List<CoordAndInt>();
		if (originalRoom.index != parent.pos.room && originalRoom.AnySkyAccess && RoomViableRoamDestination(originalRoom.index))
		{
			list.Add(new CoordAndInt(new WorldCoordinate(originalRoom.index, -1, -1, originalRoom.RandomRelevantNodeAccountingForEntranceWidth(parent.creatureTemplate, 5)), 100000 + Random.Range(0, 50)));
		}
		for (int i = 0; i < originalRoom.connections.Length; i++)
		{
			if (originalRoom.connections[i] > -1 && RoomViableRoamDestination(originalRoom.connections[i]) && Random.value < world.GetAbstractRoom(originalRoom.connections[i]).AttractionValueForCreature(parent) * 1.5f)
			{
				AbstractRoom abstractRoom = world.GetAbstractRoom(originalRoom.connections[i]);
				if (abstractRoom.AnySkyAccess)
				{
					int num = Random.Range(50, 500 + 2000 * (int)abstractRoom.SizeDependentAttractionValueForCreature(parent));
					list.Add(new CoordAndInt(new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.RandomRelevantNodeAccountingForEntranceWidth(parent.creatureTemplate, 5)), num + Random.Range(0, 50)));
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
		if (world.GetAbstractRoom(room).AttractionForCreature(parent) == AbstractRoom.CreatureRoomAttraction.Forbidden)
		{
			return false;
		}
		for (int i = 0; i < world.GetAbstractRoom(room).nodes.Length; i++)
		{
			if (world.GetAbstractRoom(room).nodes[i].type == AbstractRoomNode.Type.SkyExit && world.GetAbstractRoom(room).nodes[i].entranceWidth > 4)
			{
				return true;
			}
		}
		return false;
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
			if (world.GetAbstractRoom(room).nodes[i].type == AbstractRoomNode.Type.SkyExit && world.GetAbstractRoom(room).nodes[i].entranceWidth > 4)
			{
				list.Add(i);
			}
		}
		if (list.Count != 0)
		{
			SetDestination(new WorldCoordinate(room, -1, -1, list[Random.Range(0, list.Count)]));
		}
	}
}
