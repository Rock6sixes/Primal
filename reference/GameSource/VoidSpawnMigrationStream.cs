using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Watcher;

public class VoidSpawnMigrationStream : UpdatableAndDeletable
{
	public List<VoidSpawn> voidSpawn = new List<VoidSpawn>();

	public int spawnTimer;

	public PlacedObject.SpawnMigrationStreamData data;

	public WorldSpawnMigrationStream worldStream;

	public static float ArbitrarySpawnSpeedEstimate = 3.75f;

	public BezierSpline _spline;

	public int spawnRate
	{
		get
		{
			int num = 100;
			if (data != null)
			{
				num = data.rate;
			}
			if (room.world.regionState != null)
			{
				if (room.world.game.devUI != null)
				{
					num = desiredSpawnRate;
				}
				else if (room.world.regionState.rippleSpawnEggState.percentEggsCollected < 0.5f)
				{
					num = 0;
				}
				else
				{
					float toB = Spline.GetFullLength / (float)(MaxCapacity - 20) / ArbitrarySpawnSpeedEstimate;
					if (room.world.name.ToLowerInvariant() == "wora")
					{
						AbstractRoom abstractRoom = room.world.GetAbstractRoom("WORA_EGG");
						float percentEggsCollected = room.world.regionState.rippleSpawnEggState.percentEggsCollected;
						float num2 = Vector2.Distance(room.abstractRoom.mapPos, abstractRoom.mapPos) / 3f;
						percentEggsCollected = ((!(num2 > 1000f)) ? (percentEggsCollected * Custom.LerpMap(num2, 1000f, 100f, 0.8f, 1f)) : (percentEggsCollected * Custom.LerpMap(num2, 3000f, 1000f, 0.1f, 0.8f)));
						num = ((room.world.regionState.rippleSpawnEggState.percentEggsCollected != 1f) ? ((int)Custom.LerpMap(percentEggsCollected, 0.5f, 1f, 3000f, 1000f, 5f)) : ((int)Custom.LerpMap(percentEggsCollected, 0f, 1f, 300f, toB, 0.75f)));
					}
					else
					{
						num = (int)Custom.LerpMap(room.world.regionState.rippleSpawnEggState.percentEggsCollected, 0.5f, 1f, 2400f, toB, 0.35f);
					}
				}
			}
			if (worldStream != null && worldStream.DestRoomSealed(room.game))
			{
				num /= 4;
			}
			return num;
		}
	}

	public int desiredSpawnRate
	{
		get
		{
			if (data != null)
			{
				return data.rate;
			}
			if (worldStream != null)
			{
				return worldStream.rate;
			}
			return 30;
		}
	}

	public int MaxCapacity
	{
		get
		{
			if (data != null)
			{
				return data.maxCapacity;
			}
			return Mathf.Min(500, (int)(Spline.GetFullLength / (float)desiredSpawnRate / ArbitrarySpawnSpeedEstimate) + 20);
		}
	}

	public float Width
	{
		get
		{
			if (data != null)
			{
				return data.width;
			}
			if (worldStream != null)
			{
				return worldStream.width;
			}
			return 70f;
		}
	}

	public VoidSpawn.SpawnType Type
	{
		get
		{
			if (data != null)
			{
				return data.type;
			}
			return VoidSpawn.SpawnType.RippleSpawn;
		}
	}

	public BezierSpline Spline
	{
		get
		{
			if (data != null)
			{
				return data.spline;
			}
			return _spline;
		}
	}

	public VoidSpawnMigrationStream(Room room, BezierSpline spline, WorldSpawnMigrationStream worldStream)
	{
		base.room = room;
		_spline = spline;
		this.worldStream = worldStream;
		Populate();
	}

	public VoidSpawnMigrationStream(Room room, PlacedObject.SpawnMigrationStreamData data)
	{
		base.room = room;
		this.data = data;
		Populate();
	}

	public void Populate()
	{
		int num = spawnRate;
		if (num > 0)
		{
			spawnTimer = Random.Range(0, num);
			float getFullLength = Spline.GetFullLength;
			int num2 = (int)Mathf.Min(MaxCapacity, getFullLength / (float)num / ArbitrarySpawnSpeedEstimate);
			for (int i = 0; i < num2; i++)
			{
				float num3 = (float)(i * num) * ArbitrarySpawnSpeedEstimate;
				float num4 = num3 / 4f;
				num3 += Random.Range(0f - num4, num4);
				SpawnNewSpawn(num3);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (spawnRate > 0)
		{
			if (voidSpawn.Count < MaxCapacity && spawnTimer <= 0)
			{
				if (worldStream != null && worldStream.DestRoomSealed(room.game))
				{
					SpawnNewSpawn(Random.Range(0f, Spline.GetFullLength));
				}
				else
				{
					SpawnNewSpawn(0f);
				}
				int num = spawnRate;
				int num2 = (int)((float)(num - desiredSpawnRate) * 0.25f);
				spawnTimer = num + Random.Range(-num2, num2);
			}
			spawnTimer--;
		}
		else if (spawnTimer <= 0)
		{
			spawnTimer = Random.Range(400, 800);
		}
		bool flag = room.game.devToolsActive && Input.GetKey(KeyCode.Backspace);
		for (int num3 = voidSpawn.Count - 1; num3 >= 0; num3--)
		{
			if (voidSpawn[num3].slatedForDeletetion || voidSpawn[num3].room != room)
			{
				voidSpawn.RemoveAt(num3);
			}
			else if (flag)
			{
				voidSpawn[num3].Destroy();
				voidSpawn.RemoveAt(num3);
			}
		}
	}

	public void SpawnNewSpawn(float distance)
	{
		Vector2 pointOnPath = GetPointOnPath(distance);
		Vector2 normalized = (GetPointOnPath(distance + 1f) - pointOnPath).normalized;
		float num = Random.Range(0f - Width, Width);
		pointOnPath += Custom.rotateVectorDeg(normalized, 90f) * num;
		VoidSpawn voidSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, WatcherEnums.AbstractObjectType.RippleSpawn, null, room.GetWorldCoordinate(pointOnPath), room.world.game.GetNewID()), 0f, dayLightMode: false, Type);
		if (worldStream != null && worldStream.DestRoomSealed(room.game))
		{
			float num2 = 4f;
			if (worldStream.destRoom != null)
			{
				AbstractRoom abstractRoom = room.world.GetAbstractRoom(worldStream.destRoom);
				if (abstractRoom != null)
				{
					num2 = Custom.LerpMap(Vector2.Distance(room.abstractRoom.mapPos, abstractRoom.mapPos) / 3f, 1000f, 10000f, 7f, 2f);
				}
			}
			voidSpawn.behavior = new VoidSpawn.MillAroundMigrationStream(voidSpawn, this, Width * num2);
			voidSpawn.timeUntilFadeout = Random.Range(120, 400);
		}
		else
		{
			voidSpawn.behavior = new VoidSpawn.FollowMigrationStream(voidSpawn, this, distance, num);
		}
		voidSpawn.PlaceInRoom(room);
		this.voidSpawn.Add(voidSpawn);
	}

	public Vector2 GetPointOnPath(float pos)
	{
		bool end;
		return GetPointOnPath(pos, out end);
	}

	public Vector2 GetPointOnPath(float pos, out bool end)
	{
		end = false;
		for (int i = 0; i < Spline.Segments; i++)
		{
			float segmentLength = Spline.GetSegmentLength(i);
			if (pos > segmentLength)
			{
				pos -= segmentLength;
				continue;
			}
			return Spline.GetBezier(i).GetPoint(pos / segmentLength) + (data?.owner.pos ?? Vector2.zero);
		}
		end = true;
		return Spline.posB + (data?.owner.pos ?? Vector2.zero);
	}
}
