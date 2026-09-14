using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class MothGrub : InsectoidCreature
{
	public class MothGrubState : HealthState
	{
		public int growthStage;

		public MothGrubState(AbstractCreature creature)
			: base(creature)
		{
		}

		public override string ToString()
		{
			string text = "";
			if (growthStage > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "<cB>GrowthStage<cC>{0}", growthStage);
			}
			return base.ToString() + text;
		}

		public override void LoadFromString(string[] s)
		{
			base.LoadFromString(s);
			bool flag = false;
			for (int i = 0; i < s.Length; i++)
			{
				string[] array = Regex.Split(s[i], "<cC>");
				string text = array[0];
				if (text != null && text == "GrowthStage")
				{
					flag = true;
					growthStage = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
			}
			if (flag)
			{
				unrecognizedSaveStrings.Remove("GrowthStage");
			}
		}

		public override void CycleTick()
		{
			if (!base.dead && creature != null && creature.Room != null && creature.Room.shelter)
			{
				growthStage++;
			}
			base.CycleTick();
		}
	}

	public MothGrubAI AI;

	public MovementConnection lastFollowedConnection;

	public Vector2 localHeadWay;

	public Vector2 lookAtPoint;

	public Vector2 lookOffWay;

	public int lookOffTime;

	public int lookWiggle;

	public int selfRightTimer;

	public int selfRightWasTilted;

	public int selfRightState;

	public MothGrub(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		float num = 1.1f;
		base.bodyChunks = new BodyChunk[5];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f, num / 5f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 8f, num / 5f);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 8f, num / 5f);
		base.bodyChunks[3] = new BodyChunk(this, 3, new Vector2(0f, 0f), 8f, num / 5f);
		base.bodyChunks[4] = new BodyChunk(this, 4, new Vector2(0f, 0f), 8f, num / 5f);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].restrictInRoomRange = 2400f;
			base.bodyChunks[i].defaultRestrictInRoomRange = 2400f;
		}
		bodyChunkConnections = new BodyChunkConnection[8];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 20f, BodyChunkConnection.Type.Pull, 0.4f, 0.5f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 14f, BodyChunkConnection.Type.Push, 0.4f, 0.5f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[3], base.bodyChunks[4], 14f, BodyChunkConnection.Type.Push, 0.4f, 0.5f);
		bodyChunkConnections[3] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[2], 34f, BodyChunkConnection.Type.Push, 0.1f, 0.5f);
		bodyChunkConnections[4] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[4], 10f, BodyChunkConnection.Type.Normal, 0.7f, 0.5f);
		bodyChunkConnections[5] = new BodyChunkConnection(base.bodyChunks[4], base.bodyChunks[2], 10f, BodyChunkConnection.Type.Normal, 0.7f, 0.5f);
		bodyChunkConnections[6] = new BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[3], 10f, BodyChunkConnection.Type.Normal, 0.7f, 0.5f);
		bodyChunkConnections[7] = new BodyChunkConnection(base.bodyChunks[3], base.bodyChunks[1], 10f, BodyChunkConnection.Type.Normal, 0.7f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.47f;
		surfaceFriction = 0.25f;
		collisionLayer = 1;
		base.buoyancy = 1.3f;
		base.waterFriction = 0.87f;
		base.GoThroughFloors = true;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new MothGrubGraphics(this);
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if ((base.State as MothGrubState).growthStage > 0)
		{
			AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.BigMoth), null, room.GetWorldCoordinate(base.firstChunk.pos), base.abstractCreature.ID);
			BigMoth.BigMothState obj = abstractCreature.state as BigMoth.BigMothState;
			obj.babyness = 1f;
			obj.hatchedCycle = (room.game.IsStorySession ? room.game.GetStorySession.saveState.cycleNumber : (-1));
			obj.tagged = true;
			obj.tagOwner = BigMoth.BigMothState.TagOwner.Slugcat;
			room.abstractRoom.AddEntity(abstractCreature);
			abstractCreature.RealizeInRoom();
			Destroy();
			base.abstractCreature.Destroy();
			return;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		JointBody();
		BodyChunkConnection bodyChunkConnection = bodyChunkConnections[7];
		bodyChunkConnections[7] = bodyChunkConnections[4];
		bodyChunkConnections[4] = bodyChunkConnection;
		bodyChunkConnection = bodyChunkConnections[5];
		bodyChunkConnections[5] = bodyChunkConnections[6];
		bodyChunkConnections[6] = bodyChunkConnection;
		Vector2 vector = Custom.DirVec(base.bodyChunks[2].pos, base.bodyChunks[1].pos);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		if (base.Consious)
		{
			Act();
		}
		lookWiggle++;
		bool flag = base.bodyChunks[1].ContactPoint.y == -1 || base.bodyChunks[0].ContactPoint.y == -1;
		float speed = 0.15f;
		Vector2 vector3 = new Vector2(0f, 1f);
		vector3 = Vector2.ClampMagnitude((lookAtPoint - base.bodyChunks[1].pos) / 20f, 1f);
		if (lookOffTime == 0 && (double)Random.value < 0.004)
		{
			lookOffWay = Vector2.ClampMagnitude(vector * 0.2f + Random.insideUnitCircle, 1f);
			lookOffTime = Random.Range(20, 120 / (((double)Random.value < 0.5) ? 1 : 2));
		}
		if (lookOffTime > 0)
		{
			lookOffTime--;
			vector3 = lookOffWay;
			vector3 = Vector2.ClampMagnitude(vector3 + Vector2.Perpendicular(vector3) * (Mathf.Sin((float)lookWiggle * 0.2f) * 0.2f), 1f);
			speed = 0.25f;
		}
		if (grabbedBy.Count == 0 && flag && Random.value < 0.02f && selfRightState == 0)
		{
			selfRightState = 1;
			selfRightTimer = 0;
		}
		if (selfRightState != 0)
		{
			int num = ((vector.x > 0f) ? 1 : (-1));
			if (selfRightState == 1)
			{
				speed = 0.15f;
				vector3 = new Vector2(num, 0f);
				selfRightTimer++;
				if (selfRightTimer > 30)
				{
					selfRightWasTilted = num;
					selfRightState++;
					selfRightTimer = 0;
				}
			}
			else if (selfRightState == 2)
			{
				speed = 0.7f;
				vector3 = new Vector2((float)(-selfRightWasTilted) * (1f - Mathf.Max(Vector2.Dot(vector, Vector2.up), 0f) * 1.4f), 1f).normalized;
				selfRightTimer++;
				if (selfRightTimer > 70)
				{
					selfRightState = 0;
					selfRightTimer = 0;
				}
			}
		}
		if (!base.Consious)
		{
			vector3 = Vector2.ClampMagnitude((base.bodyChunks[0].pos - base.bodyChunks[1].pos) / bodyChunkConnections[0].distance, 1f);
		}
		Vector2 b = new Vector2(Vector2.Dot(vector3, -vector2), Vector2.Dot(vector3, vector));
		if (b.y < 0.01f)
		{
			b.y = 0.01f;
			b.x = ((b.x > 0f) ? 1f : (-1f));
		}
		localHeadWay = Custom.MoveTowards(localHeadWay, b, speed);
		Vector2 vector4 = base.bodyChunks[1].pos + Vector2.ClampMagnitude(vector * localHeadWay.y - vector2 * localHeadWay.x, 1f) * bodyChunkConnections[0].distance - base.bodyChunks[0].pos;
		vector4 *= 0.2f * ((grabbedBy.Count > 0) ? 0.3f : 1f);
		base.bodyChunks[0].vel += vector4;
		base.bodyChunks[0].pos += vector4;
		for (int i = 0; i < 4; i++)
		{
			base.bodyChunks[1 + i].vel -= vector4 * 0.25f;
			base.bodyChunks[1 + i].pos -= vector4 * 0.25f;
		}
	}

	public void JointBody()
	{
		Vector2 vector = (base.bodyChunks[1].pos + base.bodyChunks[2].pos) * 0.5f;
		Vector2 vector2 = (base.bodyChunks[3].pos + base.bodyChunks[4].pos) * 0.5f;
		_ = vector * 0.5f + vector2 * 0.5f;
		Vector2 vector3 = Custom.DirVec(base.bodyChunks[2].pos, base.bodyChunks[1].pos);
		Vector2 vector4 = Custom.PerpendicularVector(Custom.DirVec(base.bodyChunks[3].pos, base.bodyChunks[4].pos));
		Vector2 normalized = (vector3 + vector4).normalized;
		Vector2 vector5 = Custom.PerpendicularVector(normalized);
		Vector2 vector6 = ((vector + normalized * 7f - base.bodyChunks[1].pos) * 0.5f - (vector - normalized * 7f - base.bodyChunks[2].pos) * 0.5f) * 0.25f;
		base.bodyChunks[1].vel += vector6;
		base.bodyChunks[1].pos += vector6;
		base.bodyChunks[2].vel -= vector6;
		base.bodyChunks[2].pos -= vector6;
		vector6 = ((vector2 + vector5 * 7f - base.bodyChunks[3].pos) * 0.5f - (vector2 - vector5 * 7f - base.bodyChunks[4].pos) * 0.5f) * 0.25f;
		base.bodyChunks[3].vel += vector6;
		base.bodyChunks[3].pos += vector6;
		base.bodyChunks[4].vel -= vector6;
		base.bodyChunks[4].pos -= vector6;
	}

	public void Act()
	{
		AI.Update();
		if (AI.creatureLooker.lookCreature != null)
		{
			if (AI.creatureLooker.lookCreature.VisualContact)
			{
				lookAtPoint = AI.creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos;
			}
			else
			{
				lookAtPoint = room.MiddleOfTile(AI.creatureLooker.lookCreature.BestGuessForPosition());
			}
		}
		Vector2 pos = base.bodyChunks[0].pos;
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

	public override void Die()
	{
		base.Die();
	}

	public override Color ShortCutColor()
	{
		return base.ShortCutColor();
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (RippleViolenceCheck(source))
		{
			if (hitChunk.index == 0)
			{
				damage *= 2f;
				stunBonus *= 2f;
			}
			base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
		}
	}

	public override void Stun(int st)
	{
		base.Stun(st);
	}
}
