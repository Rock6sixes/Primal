using RWCustom;
using UnityEngine;

namespace Watcher;

public class Millipede : InsectoidCreature
{
	public MillipedeAI AI;

	public float runSpeed;

	public Vector2? posOnTrail;

	public int timeOffground;

	public Vector2[] bodyTrail;

	public MovementConnection lastFollowedConnection;

	public Millipede(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		float num = 0.5f;
		base.bodyChunks = new BodyChunk[15];
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(0f, 0f), 8f, num / (float)base.bodyChunks.Length);
		}
		bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length - 1];
		for (int j = 0; j < bodyChunkConnections.Length; j++)
		{
			bodyChunkConnections[j] = new BodyChunkConnection(base.bodyChunks[j], base.bodyChunks[j + 1], 12f, BodyChunkConnection.Type.Normal, 1f, -1f);
		}
		bodyTrail = new Vector2[base.bodyChunks.Length];
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.35f;
		collisionLayer = 1;
		base.waterFriction = 0.9f;
		base.buoyancy = 1.2f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new MillipedeGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		for (int i = 0; i < bodyTrail.Length; i++)
		{
			bodyTrail[i] = base.bodyChunks[i].pos;
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = newRoom.MiddleOfTile(pos);
		Vector2 vector2 = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = vector + vector2 * (1f - (float)i / (float)base.bodyChunks.Length) * 30f;
			bodyTrail[i] = base.bodyChunks[i].pos;
		}
		posOnTrail = base.bodyChunks[0].pos;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		timeOffground++;
		Vector2? vector = null;
		for (int i = 0; i < bodyTrail.Length; i++)
		{
			if (room.aimap.TileAccessibleToCreature(bodyTrail[i], base.Template))
			{
				timeOffground = 0;
				if (!vector.HasValue)
				{
					vector = bodyTrail[i];
				}
			}
		}
		if (timeOffground <= 5)
		{
			if (posOnTrail.HasValue)
			{
				if (Vector2.Distance(base.bodyChunks[0].pos, posOnTrail.Value) > 25f)
				{
					posOnTrail = null;
				}
			}
			else if (vector.HasValue && base.Consious)
			{
				posOnTrail = vector.Value;
			}
		}
		else
		{
			posOnTrail = null;
		}
		bodyTrail[0] = (posOnTrail.HasValue ? posOnTrail.Value : base.bodyChunks[0].pos);
		for (int j = 0; j < base.bodyChunks.Length; j++)
		{
			if (j > 0)
			{
				Vector2 vector2 = Custom.DirVec(bodyTrail[j], bodyTrail[j - 1]);
				float num = Vector2.Distance(bodyTrail[j], bodyTrail[j - 1]);
				bodyTrail[j] += vector2 * (num - bodyChunkConnections[j - 1].distance);
			}
			if (posOnTrail.HasValue)
			{
				Vector2 vector3 = (bodyTrail[j] - base.bodyChunks[j].pos) * runSpeed;
				base.bodyChunks[j].vel *= 0.9f;
				base.bodyChunks[j].pos += vector3 * 0.2f;
				base.bodyChunks[j].vel += vector3 * 0.2f;
				base.bodyChunks[j].vel.y = base.bodyChunks[j].vel.y + Mathf.Lerp(base.gravity, base.gravity - base.buoyancy, base.bodyChunks[j].submersion);
			}
		}
		if (base.Consious)
		{
			Act();
		}
	}

	public void Act()
	{
		AI.Update();
		Vector2 pos = base.bodyChunks[0].pos;
		if (posOnTrail.HasValue)
		{
			pos = posOnTrail.Value;
		}
		if (room.GetWorldCoordinate(pos) == AI.pathFinder.GetDestination && AI.threatTracker.Utility() < 0.5f)
		{
			base.GoThroughFloors = false;
			return;
		}
		MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(pos), actuallyFollowingThisPath: true);
		if (movementConnection == default(MovementConnection))
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(pos), actuallyFollowingThisPath: true);
		}
		if (movementConnection != default(MovementConnection))
		{
			Run(movementConnection);
			return;
		}
		if (lastFollowedConnection != default(MovementConnection))
		{
			MoveTowards(room.MiddleOfTile(lastFollowedConnection.DestTile));
		}
		base.GoThroughFloors = false;
	}

	public void MoveTowards(Vector2 moveTo)
	{
		if (posOnTrail.HasValue)
		{
			bool num = !AI.pathFinder.DestInRoom || !(room.GetTilePosition(posOnTrail.Value) == AI.pathFinder.GetDestination.Tile);
			Vector2 vector = Custom.DirVec(posOnTrail.Value, moveTo);
			if (num)
			{
				posOnTrail += vector * runSpeed * 1.75f;
			}
		}
		base.GoThroughFloors = moveTo.y < base.bodyChunks[0].pos.y - 5f;
	}

	public void Run(MovementConnection followingConnection)
	{
		if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
		{
			enteringShortCut = followingConnection.StartTile;
			if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				NPCTransportationDestination = followingConnection.destinationCoord;
			}
		}
		else
		{
			Vector2 moveTo = room.MiddleOfTile(followingConnection.DestTile);
			MoveTowards(moveTo);
		}
		lastFollowedConnection = followingConnection;
	}
}
