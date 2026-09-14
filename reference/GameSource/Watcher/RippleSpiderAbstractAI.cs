using System;
using System.Collections.Generic;
using RWCustom;

namespace Watcher;

public class RippleSpiderAbstractAI : AbstractCreatureAI
{
	public WorldCoordinate webFrom;

	public WorldCoordinate webTo;

	public bool waitForPlayer;

	public List<AbstractRoom> trail = new List<AbstractRoom>();

	public int warpRoomCheckCount;

	public int warpRoomCheckMax = 3000;

	public WorldCoordinate warpRoomDestination;

	public RippleSpiderAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		webFrom = parent.pos;
		webTo = parent.pos;
	}

	public override void NewWorld(World newWorld)
	{
		base.NewWorld(newWorld);
		trail.Clear();
	}

	public override void AbstractBehavior(int time)
	{
		base.AbstractBehavior(time);
		if (trail.Count == 0 || trail[trail.Count - 1] != parent.Room)
		{
			trail.Remove(parent.Room);
			trail.Add(parent.Room);
		}
		RegionState regionState = world.regionState;
		if (regionState != null && regionState.rippleSpiderRespawnRoom != null)
		{
			AbstractRoom abstractRoom = world.GetAbstractRoom(regionState.rippleSpiderRespawnRoom);
			int num = trail.IndexOf(abstractRoom);
			foreach (AbstractCreature player in world.game.Players)
			{
				int num2 = trail.IndexOf(player.Room);
				if (num2 > num)
				{
					num = num2;
					regionState.rippleSpiderRespawnRoom = player.Room.name;
				}
			}
		}
		WorldCoordinate pos = parent.pos;
		pos = ((parent.realizedCreature == null || parent.realizedCreature.room == null) ? pos.WashTileData() : parent.realizedCreature.room.GetWorldCoordinate(parent.realizedCreature.mainBodyChunk.pos));
		if (pos.room == webFrom.room)
		{
			webTo = pos;
			return;
		}
		if (webFrom != webTo)
		{
			string name = world.GetAbstractRoom(webFrom).name;
			if (!world.regionState.rippleSpiderWebs.TryGetValue(name, out List<(WorldCoordinate, WorldCoordinate)> value))
			{
				value = (world.regionState.rippleSpiderWebs[name] = new List<(WorldCoordinate, WorldCoordinate)>());
			}
			value.Add((webFrom, webTo));
		}
		webFrom = pos;
		webTo = pos;
	}

	public override bool MigrationBehavior(int time)
	{
		if (warpRoomCheckCount <= 0)
		{
			GetNewWarpPointDestination();
			warpRoomCheckCount = warpRoomCheckMax;
			if (warpRoomDestination != default(WorldCoordinate))
			{
				world.regionState.rippleSpiderTargetRoom = world.GetAbstractRoom(warpRoomDestination).name;
			}
		}
		else
		{
			warpRoomCheckCount -= time;
		}
		if (warpRoomDestination != default(WorldCoordinate) && !waitForPlayer && parent.pos.room != warpRoomDestination.room)
		{
			SetDestination(warpRoomDestination);
			if (!base.WantToMigrate || base.MigrationDestination != warpRoomDestination)
			{
				MigrateTo(warpRoomDestination);
			}
		}
		return warpRoomDestination != default(WorldCoordinate);
	}

	public void GetNewWarpPointDestination()
	{
		List<AbstractRoom> list = new List<AbstractRoom>();
		int num = int.MaxValue;
		AbstractRoom abstractRoom = null;
		string text = world.region.name.ToLowerInvariant();
		if (text == "wora")
		{
			abstractRoom = world.GetAbstractRoom("WORA_EGG");
			warpRoomDestination = new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.RandomRelevantNode(parent.creatureTemplate));
			return;
		}
		if (Custom.rainWorld.regionSpinningTopRooms.TryGetValue(text, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				string text2 = value[i].Split(':')[0].ToLowerInvariant();
				if (!world.regionState.saveState.deathPersistentSaveData.spawnedWarpPoints.ContainsKey(text2))
				{
					list.Add(world.GetAbstractRoom(text2));
				}
			}
		}
		if (list.Count == 0)
		{
			foreach (string item in world.regionState.saveState.RoomsWithWarpsRemainingToBeSealed(includeSpinningTopWarps: true, world.region.name.ToLowerInvariant()))
			{
				list.Add(world.GetAbstractRoom(item));
			}
		}
		foreach (AbstractRoom item2 in list)
		{
			if (item2 != null)
			{
				int num2 = world.RegionTransportationDistanceBetweenNodes(parent.pos, new WorldCoordinate(item2.index, -1, -1, -1));
				if (num > num2)
				{
					abstractRoom = item2;
					num = num2;
				}
			}
		}
		if (num != int.MaxValue)
		{
			warpRoomDestination = new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.RandomRelevantNode(parent.creatureTemplate));
		}
	}

	public override IntVector2[] PlaceInRealizedRoom()
	{
		IntVector2[] array = base.PlaceInRealizedRoom();
		if (array != null)
		{
			int i;
			for (i = 0; i < array.Length - 1 && array[i] != parent.pos.Tile; i++)
			{
			}
			IntVector2[] array2 = new IntVector2[i];
			Array.Copy(array, array2, i);
			parent.Room.realizedRoom.AddRippleSpiderWeb(array2);
		}
		return array;
	}
}
