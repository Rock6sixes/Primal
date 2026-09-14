using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Watcher;

public class VoidSpawnEgg : CosmeticSprite, INotifyWhenRoomIsReady
{
	public VoidSpawn spawn;

	public int originRoom;

	public int placedObjectIndex;

	public PlacedObject placedObject;

	public float lastRad;

	public float rad;

	public bool holdingSpawn = true;

	public int wiggleCounter;

	public float popping;

	public bool rippleRevealedFromFlower;

	public bool RippleEggHidden
	{
		get
		{
			if (!RippleSpawn || room?.game == null || !room.game.IsStorySession)
			{
				return false;
			}
			return true;
		}
	}

	public bool RippleSpawn => placedObject?.type == PlacedObject.Type.RippleSpawnEgg;

	public VoidSpawnEgg(Room room, int placedObjectIndex, PlacedObject placedObject)
	{
		base.room = room;
		originRoom = room.abstractRoom.index;
		this.placedObjectIndex = placedObjectIndex;
		this.placedObject = placedObject;
		AbstractPhysicalObject.AbstractObjectType type = (RippleSpawn ? WatcherEnums.AbstractObjectType.RippleSpawn : AbstractPhysicalObject.AbstractObjectType.VoidSpawn);
		VoidSpawn.SpawnType variant = (RippleSpawn ? VoidSpawn.SpawnType.RippleSpawn : VoidSpawn.SpawnType.VoidSpawn);
		spawn = new VoidSpawn(new AbstractPhysicalObject(room.world, type, null, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID()), room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidMelt), VoidSpawnKeeper.DayLightMode(room), variant);
		spawn.egg = this;
		spawn.abstractPhysicalObject.destroyOnAbstraction = true;
		spawn.PlaceInRoom(room);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (holdingSpawn)
		{
			wiggleCounter--;
			if (wiggleCounter < 1)
			{
				spawn.consious = !spawn.consious;
				wiggleCounter = UnityEngine.Random.Range(10, spawn.consious ? 80 : 220);
			}
			if (spawn.consious)
			{
				for (int i = 0; i < spawn.bodyChunks.Length; i++)
				{
					spawn.bodyChunks[i].vel += Custom.RNV() * (1f * UnityEngine.Random.value);
				}
			}
			spawn.inEggMode = 1f;
		}
		Vector2 a = new Vector2(0f, 0f);
		float num = 0f;
		for (int j = 0; j < spawn.bodyChunks.Length; j++)
		{
			a += spawn.bodyChunks[j].pos * spawn.bodyChunks[j].mass;
			num += spawn.bodyChunks[j].mass;
		}
		a /= num;
		a = Vector2.Lerp(a, placedObject.pos, 0.9f);
		float num2 = 0f;
		for (int k = 0; k < spawn.bodyChunks.Length; k++)
		{
			num2 += (Vector2.Distance(a, spawn.bodyChunks[k].pos) + spawn.bodyChunks[k].rad) * spawn.bodyChunks[k].mass;
		}
		num2 /= num;
		float num3 = Mathf.Lerp(num * 7f, 15f, 0.65f);
		num2 = ((!(num2 < num3)) ? Mathf.Lerp(num2, num3, 0.2f) : Mathf.Lerp(num2, num3, 0.8f));
		if (holdingSpawn)
		{
			for (int l = 0; l < spawn.bodyChunks.Length; l++)
			{
				float num4 = Mathf.Max(0f, num2 - spawn.bodyChunks[l].rad);
				if (!Custom.DistLess(spawn.bodyChunks[l].pos, a, num4))
				{
					Vector2 vector = Custom.DirVec(spawn.bodyChunks[l].pos, a) * ((num4 - Vector2.Distance(spawn.bodyChunks[l].pos, a)) * 0.5f);
					spawn.bodyChunks[l].vel -= vector;
					spawn.bodyChunks[l].pos -= vector;
				}
			}
		}
		pos = new Vector2(0f, 0f);
		for (int m = 0; m < spawn.bodyChunks.Length; m++)
		{
			pos += spawn.bodyChunks[m].pos * spawn.bodyChunks[m].mass;
		}
		pos /= num;
		pos = Vector2.Lerp(pos, a, 0.5f);
		lastRad = rad;
		rippleRevealedFromFlower = false;
		if (popping > 0f)
		{
			popping += 1f / 30f;
			rad += 3f * Mathf.Sin(popping * (float)Math.PI);
			if (popping >= 1f)
			{
				popping = 1f;
				Destroy();
			}
		}
		else
		{
			rad = num2;
			for (int n = 0; n < spawn.bodyChunks.Length; n++)
			{
				rad = Mathf.Max(rad, Vector2.Distance(pos, spawn.bodyChunks[n].pos) + spawn.bodyChunks[n].rad);
			}
			for (int num5 = 0; num5 < room.game.Players.Count && popping == 0f; num5++)
			{
				if (room.game.Players[num5].realizedCreature == null || room.game.Players[num5].realizedCreature.room != room)
				{
					continue;
				}
				if (RippleEggHidden)
				{
					Creature.Grasp[] grasps = room.game.Players[num5].realizedCreature.grasps;
					foreach (Creature.Grasp grasp in grasps)
					{
						if (grasp != null && grasp.grabbed is KarmaFlower)
						{
							rippleRevealedFromFlower = true;
							break;
						}
					}
					if ((room.game.Players[num5].realizedCreature as Player).rippleSpawnEggReveal <= 10f)
					{
						continue;
					}
					rippleRevealedFromFlower = false;
				}
				for (int num7 = 0; num7 < room.game.Players[num5].realizedCreature.bodyChunks.Length; num7++)
				{
					if (Custom.DistLess(room.game.Players[num5].realizedCreature.bodyChunks[num7].pos, pos, room.game.Players[num5].realizedCreature.bodyChunks[num7].rad + rad))
					{
						Pop();
						break;
					}
				}
			}
		}
		if (holdingSpawn && spawn.graphicsModule != null)
		{
			for (int num8 = 0; num8 < (spawn.graphicsModule as VoidSpawnGraphics).subModules.Count; num8++)
			{
				if (!((spawn.graphicsModule as VoidSpawnGraphics).subModules[num8] is VoidSpawnGraphics.Antenna))
				{
					continue;
				}
				VoidSpawnGraphics.Antenna antenna = (spawn.graphicsModule as VoidSpawnGraphics).subModules[num8] as VoidSpawnGraphics.Antenna;
				for (int num9 = 0; num9 < antenna.segments.GetLength(0); num9++)
				{
					if (!Custom.DistLess(antenna.segments[num9, 0], pos, rad))
					{
						Vector2 vector2 = Custom.DirVec(antenna.segments[num9, 0], pos);
						antenna.segments[num9, 2] -= (rad - Vector2.Distance(antenna.segments[num9, 0], pos)) * vector2;
						antenna.segments[num9, 0] -= (rad - Vector2.Distance(antenna.segments[num9, 0], pos)) * vector2;
					}
				}
			}
		}
		if (spawn.slatedForDeletetion)
		{
			Destroy();
		}
	}

	public void Pop()
	{
		popping = 0.01f;
		holdingSpawn = false;
		spawn.consious = true;
		if (RippleSpawn)
		{
			bool flag = room.world.regionState.rippleSpawnEggState.RippleEggWarpAllowed();
			room.world.regionState.rippleSpawnEggState.ReportEggCollected(room.abstractRoom.name, placedObjectIndex);
			if (room.world.regionState.rippleSpawnEggState.RippleEggWarpAllowed() && !flag && room.game.Players.Count > 0 && room.game.Players[0].realizedCreature is Player player && player.room == room)
			{
				room.AddObject(new RippleEggWarpFlash(player.mainBodyChunk));
			}
			int rippleSpiderThreshold = room.world.region.regionParams.rippleSpiderThreshold;
			if (rippleSpiderThreshold > 0 && room.world.regionState.rippleSpawnEggState.eggsCollected % rippleSpiderThreshold == 0)
			{
				SpawnRippleSpider();
			}
		}
		else
		{
			room.game.GetStorySession.saveState.ReportConsumedItem(room.world, karmaFlower: false, originRoom, placedObjectIndex, UnityEngine.Random.Range((placedObject.data as PlacedObject.VoidSpawnEggData).minRegen, (placedObject.data as PlacedObject.VoidSpawnEggData).maxRegen));
		}
	}

	public void SpawnRippleSpider()
	{
		room.AddObject(new RippleSpider.RippleSpiderSpawner(room, pos));
	}

	public static bool CanSpawnRippleEgg(Room room, int eggIndex)
	{
		if (!(room.game.session is StoryGameSession) || !room.game.GetStorySession.saveState.CanSeeVoidSpawn)
		{
			return false;
		}
		if (RegionState.RippleSpawnEggState.UseRegionEggTracker || room.world.name.ToLowerInvariant() == "wora")
		{
			return !room.world.regionState.rippleSpawnEggState.CheckCollected(room.abstractRoom.name, eggIndex);
		}
		return !room.game.GetStorySession.saveState.miscWorldSaveData.rippleEggsToRespawn.Contains((room.abstractRoom.name, eggIndex));
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["VectorCircleFadable"];
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].scale = Mathf.Lerp(lastRad, rad, timeStacker) / 8f;
		sLeaser.sprites[0].alpha = 1f / Mathf.Lerp(lastRad, rad, timeStacker);
		if (spawn.room != null && spawn.graphicsModule != null)
		{
			sLeaser.sprites[0].color = new Color(((RippleSpawn && spawn.abstractPhysicalObject.rippleLayer == spawn.room.game.ActiveRippleLayer) ? 7f : 4f) / 255f, 0f, (spawn.graphicsModule as VoidSpawnGraphics).AlphaFromGlowDist((spawn.graphicsModule as VoidSpawnGraphics).glowPos, Vector2.Lerp(lastPos, pos, timeStacker)) * Mathf.Lerp(0.9f, 0.4f, (spawn.graphicsModule as VoidSpawnGraphics).darkness) * (1f - popping));
		}
	}

	public void ShortcutsReady()
	{
	}

	public void AIMapReady()
	{
		if (room.game.StoryCharacter == SlugcatStats.Name.Red)
		{
			spawn.behavior = new VoidSpawn.EggAndAway(spawn, room);
		}
		else if (RippleSpawn)
		{
			AbstractRoom abstractRoom = FindNearestWarpRoom();
			if (abstractRoom != null && room.world.name.ToLowerInvariant() != "wora")
			{
				if (abstractRoom != room.abstractRoom)
				{
					spawn.behavior = new VoidSpawn.EggToRoom(spawn, room, abstractRoom);
					return;
				}
				if (room.spawnedSpinningTop)
				{
					foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
					{
						if (placedObject.type == WatcherEnums.PlacedObjectType.SpinningTopSpot)
						{
							spawn.behavior = new VoidSpawn.EggToPos(spawn, room, placedObject.pos);
							return;
						}
					}
				}
				if (room.warpPoints.Count > 0)
				{
					foreach (WarpPoint warpPoint in room.warpPoints)
					{
						if (warpPoint.Data.nonDynamicWarpPoint || warpPoint.Data.wasNonDynamicWarpBeforeWeaverTriggered)
						{
							spawn.behavior = new VoidSpawn.EggToPos(spawn, room, warpPoint.pos);
							return;
						}
					}
				}
			}
			if ((abstractRoom = FindNearestEggRoom()) != null)
			{
				spawn.behavior = new VoidSpawn.EggToRoom(spawn, room, abstractRoom);
			}
			else
			{
				spawn.behavior = new VoidSpawn.EggAndAway(spawn, room);
			}
		}
		else
		{
			spawn.behavior = new VoidSpawn.EggToExit(spawn, (this.placedObject.data as PlacedObject.VoidSpawnEggData).exit, room, spawn.dayLightMode);
		}
	}

	public AbstractRoom FindNearestWarpRoom()
	{
		AbstractRoom abstractRoom = null;
		if (placedObject.data is PlacedObject.RippleSpawnEggData { destRoom: not null } rippleSpawnEggData)
		{
			abstractRoom = room.world.GetAbstractRoom(rippleSpawnEggData.destRoom);
			if (abstractRoom != null)
			{
				return abstractRoom;
			}
		}
		float num = float.MaxValue;
		Vector2 a = room.world.RoomToWorldPos(pos, room.abstractRoom.index);
		try
		{
			if (!room.game.rainWorld.regionWarpRooms.TryGetValue(room.world.region.name.ToLowerInvariant(), out var value))
			{
				value = new List<string>();
			}
			if (!room.game.rainWorld.regionSpinningTopRooms.TryGetValue(room.world.region.name.ToLowerInvariant(), out var value2))
			{
				value2 = new List<string>();
			}
			for (int i = 0; i < value.Count + value2.Count; i++)
			{
				string text = ((i >= value.Count) ? value2[i - value.Count] : value[i]);
				string[] array = text.Split(':');
				if (array.Length > 3 && array[3] == "NULL")
				{
					continue;
				}
				AbstractRoom abstractRoom2 = room.world.GetAbstractRoom(array[0]);
				if (abstractRoom2 != null && !room.game.GetStorySession.saveState.miscWorldSaveData.roomsSealedByWeaverAbility.Contains(array[0].ToLowerInvariant()))
				{
					float num2 = Vector2.Distance(a, room.world.RoomToWorldPos(new Vector2((float)abstractRoom2.size.x * 20f, (float)abstractRoom2.size.y * 20f) / 2f, abstractRoom2.index));
					if (num2 < num)
					{
						num = num2;
						abstractRoom = abstractRoom2;
					}
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return abstractRoom;
	}

	public AbstractRoom FindNearestEggRoom()
	{
		AbstractRoom result = null;
		float num = float.MaxValue;
		Vector2 a = room.world.RoomToWorldPos(pos, room.abstractRoom.index);
		string[] roomsWithEggs = room.world.regionState.rippleSpawnEggState.RoomsWithEggs;
		foreach (string text in roomsWithEggs)
		{
			if (text.ToLowerInvariant() == room.abstractRoom.name.ToLowerInvariant() || !room.world.regionState.rippleSpawnEggState.UncollectedEggsInRoom(text))
			{
				continue;
			}
			AbstractRoom abstractRoom = room.world.GetAbstractRoom(text);
			if (abstractRoom != null)
			{
				float num2 = Vector2.Distance(a, room.world.RoomToWorldPos(new Vector2((float)abstractRoom.size.x * 20f, (float)abstractRoom.size.y * 20f) / 2f, abstractRoom.index));
				if (num2 < num)
				{
					num = num2;
					result = abstractRoom;
				}
			}
		}
		return result;
	}
}
