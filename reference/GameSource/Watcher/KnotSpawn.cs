using System;
using UnityEngine;

namespace Watcher;

public class KnotSpawn : PhysicalObject
{
	public static int maxKarma = 6;

	public static float tailSegmentSize = 20f;

	public int Karma;

	public (Vector2 pos, Vector2 lastPos)[] tail;

	public KnotSpawn(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		Karma = Mathf.RoundToInt(Mathf.Lerp(0f, 10f, UnityEngine.Random.value * UnityEngine.Random.value));
		float t = (float)Karma / (float)maxKarma;
		base.bodyChunks = new BodyChunk[1];
		float num = Mathf.Lerp(1f, 1.5f, UnityEngine.Random.value) * Mathf.Lerp(20f, 32f, t);
		base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), num, num * 0.1f);
		bodyChunkConnections = Array.Empty<BodyChunkConnection>();
		base.airFriction = 1f;
		base.gravity = 0f;
		bounce = 0f;
		surfaceFriction = 0.4f;
		collisionLayer = 0;
		base.waterFriction = 1f;
		base.buoyancy = 0f;
		base.CollideWithTerrain = false;
		base.CollideWithObjects = false;
		canBeHitByWeapons = false;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new KnotSpawnGraphics(this);
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		BodyChunk[] array = base.bodyChunks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (Input.GetKey(KeyCode.M))
		{
			base.bodyChunks[0].pos = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + room.game.cameras[0].pos;
		}
	}
}
