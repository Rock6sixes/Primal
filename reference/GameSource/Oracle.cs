using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Oracle : PhysicalObject
{
	public class OracleID : ExtEnum<OracleID>
	{
		public static OracleID SS = new OracleID("SS", register: true);

		public static OracleID SL = new OracleID("SL", register: true);

		public OracleID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class OracleArm
	{
		public class Joint
		{
			public OracleArm arm;

			public Joint previous;

			public Joint next;

			public int index;

			public Vector2 pos;

			public Vector2 lastPos;

			public Vector2 vel;

			public float totalLength;

			public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

			public float currentInvKinFlip;

			public Vector2 ElbowPos(float timeStacker, Vector2 Tip)
			{
				Vector2 vc = Vector2.Lerp(lastPos, pos, timeStacker);
				if (next != null)
				{
					return Custom.InverseKinematic(Tip, vc, totalLength * (1f / 3f), totalLength * (2f / 3f), (index % 2 == 0) ? 1f : (-1f));
				}
				return Custom.InverseKinematic(Tip, vc, totalLength / 3f, totalLength / 3f, (index % 2 == 0) ? 1f : (-1f));
			}

			public Joint(OracleArm arm, int index)
			{
				this.arm = arm;
				this.index = index;
				currentInvKinFlip = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
				switch (index)
				{
				case 0:
					totalLength = 300f;
					break;
				case 1:
					totalLength = 150f;
					break;
				case 2:
					totalLength = 90f;
					break;
				case 3:
					totalLength = 30f;
					break;
				}
				pos = arm.BasePos(1f);
				lastPos = pos;
			}

			public void Update()
			{
				lastPos = pos;
				pos += vel;
				vel *= (ModManager.MSC ? 0.7f : 0.8f);
				if ((float)index == 0f)
				{
					pos = arm.BasePos(1f);
				}
				else if (index < arm.joints.Length - 1)
				{
					if (index == 1 && (arm.baseMoving || arm.oracle.room.GetTile(previous.ElbowPos(1f, pos)).Solid))
					{
						Vector2 vector = Custom.InverseKinematic(previous.pos, next.pos, previous.totalLength, totalLength, currentInvKinFlip);
						Vector2 a = Custom.InverseKinematic(previous.pos, next.pos, previous.totalLength, totalLength, 0f - currentInvKinFlip);
						float num = (arm.oracle.room.GetTile(vector).Solid ? 10f : 0f);
						num += (arm.oracle.room.GetTile(previous.ElbowPos(1f, Vector2.Lerp(vector, previous.pos, 0.2f))).Solid ? 1f : 0f);
						float num2 = (arm.oracle.room.GetTile(a).Solid ? 10f : 0f);
						num2 += (arm.oracle.room.GetTile(previous.ElbowPos(1f, Vector2.Lerp(a, previous.pos, 0.2f))).Solid ? 1f : 0f);
						if (num > num2)
						{
							currentInvKinFlip *= -1f;
							vector = Custom.InverseKinematic(previous.pos, next.pos, previous.totalLength, totalLength, currentInvKinFlip);
						}
						else if (num == 0f)
						{
							vel += Vector2.ClampMagnitude(vector - pos, 100f) / 100f * 1.8f;
						}
					}
					SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, 1f, new IntVector2(0, 0), goThroughFloors: true);
					cd = SharedPhysics.VerticalCollision(arm.oracle.room, cd);
					cd = SharedPhysics.HorizontalCollision(arm.oracle.room, cd);
					cd = SharedPhysics.SlopesVertically(arm.oracle.room, cd);
					pos = cd.pos;
					vel = cd.vel;
				}
				if (next != null)
				{
					Vector2 vector2 = Custom.DirVec(pos, next.pos);
					float num3 = Vector2.Distance(pos, next.pos);
					float num4 = 0.5f;
					if (index == 0)
					{
						num4 = 0f;
					}
					else if (index == arm.joints.Length - 2)
					{
						num4 = 1f;
					}
					float num5 = -1f;
					float num6 = 0.5f;
					if (previous != null)
					{
						num6 = Custom.LerpMap(Vector2.Dot(Custom.DirVec(previous.pos, pos), vector2), -1f, 1f, 1f, 0.2f);
					}
					if (num3 > totalLength)
					{
						num5 = totalLength;
					}
					else if (num3 < totalLength * num6)
					{
						num5 = totalLength * num6;
					}
					if (num5 > 0f)
					{
						Vector2 vector3 = vector2 * ((num3 - num5) * num4);
						pos += vector3;
						vel += vector3;
						Vector2 vector4 = vector2 * ((num3 - num5) * (1f - num4));
						next.vel -= vector4;
					}
				}
				else
				{
					Vector2 vector5 = arm.oracle.bodyChunks[1].pos;
					if (arm.oracle.ID == OracleID.SS || (ModManager.MSC && arm.oracle.ID == MoreSlugcatsEnums.OracleID.DM))
					{
						vector5 -= arm.oracle.oracleBehavior.GetToDir * totalLength / 2f;
					}
					else
					{
						vector5 -= Custom.PerpendicularVector(arm.oracle.oracleBehavior.GetToDir) * totalLength / 2f;
					}
					vector5 += Custom.DirVec(arm.oracle.bodyChunks[1].pos, pos) * totalLength / 2f;
					vel += Vector2.ClampMagnitude(vector5 - pos, 50f) / 50f * 1.2f;
					pos += Vector2.ClampMagnitude(vector5 - pos, 50f) / 50f * 1.2f;
					Vector2 vector6 = Custom.DirVec(pos, arm.oracle.bodyChunks[0].pos);
					float num7 = Vector2.Distance(pos, arm.oracle.bodyChunks[0].pos);
					pos += vector6 * (num7 - totalLength);
					vel += vector6 * (num7 - totalLength);
				}
			}
		}

		public Oracle oracle;

		public Joint[] joints;

		public Vector2[] cornerPositions;

		public float lastFramePos;

		public float framePos;

		public bool baseMoving;

		public StaticSoundLoop baseMoveSoundLoop;

		public bool isActive;

		public Vector2 BasePos(float timeStacker)
		{
			return OnFramePos(Mathf.Lerp(lastFramePos, framePos, timeStacker));
		}

		public OracleArm(Oracle oracle)
		{
			this.oracle = oracle;
			if ((oracle.ID == OracleID.SS || (ModManager.MSC && (oracle.ID == MoreSlugcatsEnums.OracleID.DM || oracle.ID == MoreSlugcatsEnums.OracleID.ST))) && (!ModManager.MSC || oracle.room.world.name != "RM"))
			{
				baseMoveSoundLoop = new StaticSoundLoop(SoundID.SS_AI_Base_Move_LOOP, oracle.firstChunk.pos, oracle.room, 1f, 1f);
			}
			cornerPositions = new Vector2[4];
			cornerPositions[0] = oracle.room.MiddleOfTile(10, 31);
			cornerPositions[1] = oracle.room.MiddleOfTile(38, 31);
			cornerPositions[2] = oracle.room.MiddleOfTile(38, 3);
			cornerPositions[3] = oracle.room.MiddleOfTile(10, 3);
			if (oracle.ID == OracleID.SL)
			{
				for (int i = 0; i < cornerPositions.Length; i++)
				{
					cornerPositions[i] += new Vector2(1040f, -20f);
				}
			}
			if (ModManager.MSC)
			{
				if (oracle.ID == MoreSlugcatsEnums.OracleID.CL)
				{
					for (int j = 0; j < cornerPositions.Length; j++)
					{
						cornerPositions[j] += new Vector2(2810f, 370f);
					}
				}
				if (oracle.ID == OracleID.SS && oracle.room.world.name == "RM")
				{
					cornerPositions[0] = oracle.room.MiddleOfTile(61, 68);
					cornerPositions[1] = oracle.room.MiddleOfTile(89, 68);
					cornerPositions[2] = oracle.room.MiddleOfTile(89, 40);
					cornerPositions[3] = oracle.room.MiddleOfTile(61, 40);
				}
				if (Region.IsRubiconRegion(oracle.room.world.name))
				{
					cornerPositions[0] = oracle.room.MiddleOfTile(10, 34);
					cornerPositions[1] = oracle.room.MiddleOfTile(38, 34);
					cornerPositions[2] = oracle.room.MiddleOfTile(38, 6);
					cornerPositions[3] = oracle.room.MiddleOfTile(10, 6);
				}
			}
			joints = new Joint[4];
			for (int k = 0; k < joints.Length; k++)
			{
				joints[k] = new Joint(this, k);
				if (k > 0)
				{
					joints[k].previous = joints[k - 1];
					joints[k - 1].next = joints[k];
				}
			}
			framePos = 10002.5f;
			if (ModManager.MSC && Region.IsRubiconRegion(oracle.room.world.name))
			{
				if (oracle.ID == MoreSlugcatsEnums.OracleID.DM)
				{
					framePos = 10001.5f;
				}
				else
				{
					framePos = 10003.5f;
				}
			}
			lastFramePos = framePos;
		}

		public void Update()
		{
			if (ModManager.MSC && oracle.room.world.name == "RM")
			{
				joints[1].vel += Vector2.ClampMagnitude(new Vector2(1289f, 854f) - joints[1].pos, 50f) / 50f * 5f;
				oracle.bodyChunks[1].vel *= 0.9f;
				oracle.bodyChunks[0].vel *= 0.9f;
				if (!(oracle.oracleBehavior as SSOracleRotBehavior).InSitPosition)
				{
					oracle.bodyChunks[0].vel += Vector2.ClampMagnitude(oracle.oracleBehavior.OracleGetToPos - oracle.bodyChunks[0].pos, 100f) / 100f * 0.5f;
				}
				if (Custom.Dist(oracle.bodyChunks[1].pos, oracle.oracleBehavior.OracleGetToPos) > 48f)
				{
					oracle.bodyChunks[1].vel += Vector2.ClampMagnitude(oracle.oracleBehavior.OracleGetToPos - oracle.oracleBehavior.GetToDir * oracle.bodyChunkConnections[0].distance - oracle.bodyChunks[0].pos, 100f) / 100f * 0.2f;
				}
			}
			else if (oracle.ID == OracleID.SS || (ModManager.MSC && (oracle.ID == MoreSlugcatsEnums.OracleID.DM || oracle.ID == MoreSlugcatsEnums.OracleID.ST || isActive)))
			{
				if (oracle.Consious)
				{
					float num = 1f;
					if (ModManager.MSC)
					{
						num = ((!(oracle.dazed > 240f)) ? (1f - oracle.dazed / 240f) : 0f);
					}
					for (int i = 0; i < oracle.bodyChunks.Length; i++)
					{
						oracle.bodyChunks[i].vel *= 0.4f;
					}
					oracle.bodyChunks[0].vel += Vector2.ClampMagnitude(oracle.oracleBehavior.OracleGetToPos - oracle.bodyChunks[0].pos, 100f) / 100f * (6.2f * num);
					for (int j = 1; j < oracle.bodyChunks.Length; j++)
					{
						oracle.bodyChunks[j].vel += Vector2.ClampMagnitude(oracle.oracleBehavior.OracleGetToPos - oracle.oracleBehavior.GetToDir * oracle.bodyChunkConnections[0].distance - oracle.bodyChunks[0].pos, 100f) / 100f * (3.2f * num);
					}
				}
				Vector2 baseGetToPos = oracle.oracleBehavior.BaseGetToPos;
				Vector2 vector = new Vector2(Mathf.Clamp(baseGetToPos.x, cornerPositions[0].x, cornerPositions[1].x), cornerPositions[0].y);
				float num2 = Vector2.Distance(vector, baseGetToPos);
				float num3 = Mathf.InverseLerp(cornerPositions[0].x, cornerPositions[1].x, baseGetToPos.x);
				for (int k = 1; k < 4; k++)
				{
					if ((ModManager.MSC && k == 3 && Region.IsRubiconRegion(oracle.room.world.name) && oracle.ID == MoreSlugcatsEnums.OracleID.DM) || (ModManager.MSC && k == 1 && Region.IsRubiconRegion(oracle.room.world.name) && oracle.ID == OracleID.SS))
					{
						continue;
					}
					Vector2 vector2 = ((k % 2 != 0) ? new Vector2(cornerPositions[k].x, Mathf.Clamp(baseGetToPos.y, cornerPositions[2].y, cornerPositions[0].y)) : ((ModManager.MSC && Region.IsRubiconRegion(oracle.room.world.name) && oracle.ID == OracleID.SS) ? new Vector2(Mathf.Clamp(baseGetToPos.x, cornerPositions[0].x, cornerPositions[0].x + (cornerPositions[1].x - cornerPositions[0].x) * 0.4f), cornerPositions[k].y) : ((!ModManager.MSC || !Region.IsRubiconRegion(oracle.room.world.name) || !(oracle.ID == MoreSlugcatsEnums.OracleID.DM)) ? new Vector2(Mathf.Clamp(baseGetToPos.x, cornerPositions[0].x, cornerPositions[1].x), cornerPositions[k].y) : new Vector2(Mathf.Clamp(baseGetToPos.x, cornerPositions[0].x + (cornerPositions[1].x - cornerPositions[0].x) * 0.6f, cornerPositions[1].x), cornerPositions[k].y))));
					float num4 = Vector2.Distance(vector2, baseGetToPos);
					if (num4 < num2)
					{
						vector = vector2;
						num2 = num4;
						switch (k)
						{
						case 1:
							num3 = (float)k + Mathf.InverseLerp(cornerPositions[0].y, cornerPositions[2].y, baseGetToPos.y);
							break;
						case 2:
							num3 = (float)k + Mathf.InverseLerp(cornerPositions[1].x, cornerPositions[0].x, baseGetToPos.x);
							break;
						case 3:
							num3 = (float)k + Mathf.InverseLerp(cornerPositions[2].y, cornerPositions[0].y, baseGetToPos.y);
							break;
						}
					}
				}
				baseMoving = Vector2.Distance(BasePos(1f), vector) > (baseMoving ? 50f : 350f) && oracle.oracleBehavior.consistentBasePosCounter > 30;
				lastFramePos = framePos;
				if (baseMoving)
				{
					framePos = Mathf.MoveTowardsAngle(framePos * 90f, num3 * 90f, 1f) / 90f;
					if (baseMoveSoundLoop != null)
					{
						baseMoveSoundLoop.volume = Mathf.Min(baseMoveSoundLoop.volume + 0.1f, 1f);
						baseMoveSoundLoop.pitch = Mathf.Min(baseMoveSoundLoop.pitch + 0.025f, 1f);
					}
				}
				else if (baseMoveSoundLoop != null)
				{
					baseMoveSoundLoop.volume = Mathf.Max(baseMoveSoundLoop.volume - 0.1f, 0f);
					baseMoveSoundLoop.pitch = Mathf.Max(baseMoveSoundLoop.pitch - 0.025f, 0.5f);
				}
				if (baseMoveSoundLoop != null)
				{
					baseMoveSoundLoop.pos = BasePos(1f);
					baseMoveSoundLoop.Update();
					if (ModManager.MSC)
					{
						baseMoveSoundLoop.volume *= 1f - oracle.noiseSuppress;
					}
				}
			}
			else
			{
				joints[1].vel += Vector2.ClampMagnitude(new Vector2(1774f, 66f) - joints[1].pos, 50f) / 50f * 5f;
				oracle.bodyChunks[1].vel *= 0.9f;
				oracle.bodyChunks[0].vel *= 0.9f;
				if (!oracle.Consious || (oracle.oracleBehavior.player != null && oracle.oracleBehavior.player.DangerPos.x < oracle.firstChunk.pos.x))
				{
					oracle.bodyChunks[0].vel += Vector2.ClampMagnitude(oracle.oracleBehavior.OracleGetToPos - oracle.bodyChunks[0].pos, 100f) / 100f * 0.5f;
					oracle.bodyChunks[1].vel += Vector2.ClampMagnitude(oracle.oracleBehavior.OracleGetToPos - oracle.oracleBehavior.GetToDir * oracle.bodyChunkConnections[0].distance - oracle.bodyChunks[0].pos, 100f) / 100f * 0.2f;
				}
				else if (oracle.bodyChunks[1].pos.y > 140f && (!ModManager.MSC || !oracle.room.game.IsMoonActive()))
				{
					oracle.bodyChunks[1].vel.x -= 1f;
				}
			}
			for (int l = 0; l < joints.Length; l++)
			{
				joints[l].Update();
			}
		}

		public Vector2 BaseDir(float timeStacker)
		{
			if (oracle.ID == OracleID.SL)
			{
				if (oracle.room.game.IsStorySession && oracle.room.game.TimelinePoint == SlugcatStats.Timeline.Sofanthiel)
				{
					return new Vector2(0f, -1f);
				}
				return new Vector2(-1f, 0f);
			}
			if (ModManager.MSC && oracle.ID == MoreSlugcatsEnums.OracleID.CL)
			{
				return new Vector2(-1f, 0.4f);
			}
			if (ModManager.MSC && oracle.room.world.name == "RM")
			{
				return new Vector2(1f, 0f);
			}
			float num = Mathf.Lerp(lastFramePos, framePos, timeStacker) % 4f;
			float num2 = 0.1f;
			if (num < num2)
			{
				return Vector3.Slerp(new Vector2(1f, 0f), new Vector2(0f, -1f), 0.5f + Mathf.InverseLerp(0f, num2, num) * 0.5f);
			}
			if (num < 1f - num2)
			{
				return new Vector2(0f, -1f);
			}
			if (num < 1f + num2)
			{
				return Vector3.Slerp(new Vector2(0f, -1f), new Vector2(-1f, 0f), Mathf.InverseLerp(1f - num2, 1f + num2, num));
			}
			if (num < 2f - num2)
			{
				return new Vector2(-1f, 0f);
			}
			if (num < 2f + num2)
			{
				return Vector3.Slerp(new Vector2(-1f, 0f), new Vector2(0f, 1f), Mathf.InverseLerp(2f - num2, 2f + num2, num));
			}
			if (num < 3f - num2)
			{
				return new Vector2(0f, 1f);
			}
			if (num < 3f + num2)
			{
				return Vector3.Slerp(new Vector2(0f, 1f), new Vector2(1f, 0f), Mathf.InverseLerp(3f - num2, 3f + num2, num));
			}
			if (num < 4f - num2)
			{
				return new Vector2(1f, 0f);
			}
			return Vector3.Slerp(new Vector2(1f, 0f), new Vector2(0f, -1f), Mathf.InverseLerp(4f - num2, 4f, num) * 0.5f);
		}

		public Vector2 OnFramePos(float timeStacker)
		{
			if (oracle.ID == OracleID.SL)
			{
				if (oracle.room.game.IsStorySession && oracle.room.game.TimelinePoint == SlugcatStats.Timeline.Sofanthiel)
				{
					return new Vector2(1670f, 605f);
				}
				return new Vector2(1810f, 356f);
			}
			if (ModManager.MSC && oracle.ID == MoreSlugcatsEnums.OracleID.CL)
			{
				return new Vector2(2810f, 370f);
			}
			if (ModManager.MSC && oracle.room.world.name == "RM")
			{
				return new Vector2(1210f, 1140f);
			}
			float num = Mathf.Lerp(lastFramePos, framePos, timeStacker) % 4f;
			float num2 = 0.1f;
			float num3 = Mathf.Abs(cornerPositions[0].x - cornerPositions[1].x) * num2;
			Vector2 vector = default(Vector2);
			float num4 = 0f;
			if (num < num2)
			{
				vector = new Vector2(cornerPositions[0].x + num3, cornerPositions[1].y - num3);
				num4 = -45f + Mathf.InverseLerp(0f, num2, num) * 45f;
			}
			else
			{
				if (num < 1f - num2)
				{
					return Vector2.Lerp(cornerPositions[0], cornerPositions[1], Mathf.InverseLerp(0f, 1f, num));
				}
				if (num < 1f + num2)
				{
					vector = new Vector2(cornerPositions[1].x - num3, cornerPositions[1].y - num3);
					num4 = Mathf.InverseLerp(1f - num2, 1f + num2, num) * 90f;
				}
				else
				{
					if (num < 2f - num2)
					{
						return Vector2.Lerp(cornerPositions[1], cornerPositions[2], Mathf.InverseLerp(1f, 2f, num));
					}
					if (num < 2f + num2)
					{
						vector = new Vector2(cornerPositions[2].x - num3, cornerPositions[2].y + num3);
						num4 = 90f + Mathf.InverseLerp(2f - num2, 2f + num2, num) * 90f;
					}
					else
					{
						if (num < 3f - num2)
						{
							return Vector2.Lerp(cornerPositions[2], cornerPositions[3], Mathf.InverseLerp(2f, 3f, num));
						}
						if (num < 3f + num2)
						{
							vector = new Vector2(cornerPositions[3].x + num3, cornerPositions[3].y + num3);
							num4 = 180f + Mathf.InverseLerp(3f - num2, 3f + num2, num) * 90f;
						}
						else
						{
							if (num < 4f - num2)
							{
								return Vector2.Lerp(cornerPositions[3], cornerPositions[0], Mathf.InverseLerp(3f, 4f, num));
							}
							vector = new Vector2(cornerPositions[0].x + num3, cornerPositions[0].y - num3);
							num4 = 270f + Mathf.InverseLerp(4f - num2, 4f, num) * 45f;
						}
					}
				}
			}
			return vector + Custom.DegToVec(num4) * num3;
		}
	}

	public OracleBehavior oracleBehavior;

	public OracleArm arm;

	public OracleID ID;

	public OracleProjectionScreen myScreen;

	public List<PebblesPearl> marbles;

	public StarMatrix starMatrix;

	public List<OracleSwarmer> mySwarmers;

	public float health = 1f;

	public int glowers;

	public int spasms;

	public int stun;

	public int pearlCounter;

	public LightSource MoonLight;

	public float dazed;

	public float noiseSuppress;

	public bool marbleOrbiting;

	public int behaviorTime;

	public bool suppressConnectionFires;

	public bool Alive
	{
		get
		{
			if (ModManager.MSC && room.game.session is StoryGameSession)
			{
				if (Region.IsRubiconRegion(room.world.name))
				{
					return true;
				}
				if (ID == OracleID.SS && (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripPebbles)
				{
					return false;
				}
				if (ID == OracleID.SL && (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripMoon)
				{
					return false;
				}
				if (ID == MoreSlugcatsEnums.OracleID.CL && (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripPebbles)
				{
					return false;
				}
			}
			return health > 0f;
		}
	}

	public bool Consious
	{
		get
		{
			if (ModManager.MSC && room.game.session is StoryGameSession && Region.IsRubiconRegion(room.world.name))
			{
				return true;
			}
			if (Alive)
			{
				return stun < 1;
			}
			return false;
		}
	}

	public Oracle(AbstractPhysicalObject abstractPhysicalObject, Room room)
		: base(abstractPhysicalObject)
	{
		base.room = room;
		base.bodyChunks = new BodyChunk[2];
		if (ModManager.MSC && room.abstractRoom.name == "Chal_AI")
		{
			ID = MoreSlugcatsEnums.OracleID.ST;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(350f, 350f), 6f, 0.5f);
			}
		}
		else if (ModManager.MSC && room.abstractRoom.name == "CL_AI")
		{
			ID = MoreSlugcatsEnums.OracleID.CL;
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				base.bodyChunks[j] = new BodyChunk(this, j, new Vector2(2570f, 160f), 6f, 0.5f);
			}
		}
		else if (ModManager.MSC && Region.IsRubiconRegion(room.world.name))
		{
			ID = room.oracleWantToSpawn;
			for (int k = 0; k < base.bodyChunks.Length; k++)
			{
				Vector2 pos = new Vector2(650f, 280f);
				if (ID == OracleID.SS)
				{
					pos = new Vector2(320f, 590f);
				}
				base.bodyChunks[k] = new BodyChunk(this, k, pos, 6f, 0.5f);
				base.bodyChunks[k].restrictInRoomRange = float.MaxValue;
			}
		}
		else
		{
			ID = ((room.abstractRoom.name == "SS_AI" || (ModManager.MSC && room.world.name == "RM")) ? OracleID.SS : OracleID.SL);
			if (ModManager.MSC && room.abstractRoom.name == "DM_AI")
			{
				ID = MoreSlugcatsEnums.OracleID.DM;
			}
			for (int l = 0; l < base.bodyChunks.Length; l++)
			{
				if (ModManager.MSC && room.world.name == "RM")
				{
					base.bodyChunks[l] = new BodyChunk(this, l, new Vector2(1395f, 850f), 6f, 0.5f);
				}
				else
				{
					base.bodyChunks[l] = new BodyChunk(this, l, (ID == OracleID.SS || (ModManager.MSC && ID == MoreSlugcatsEnums.OracleID.DM)) ? new Vector2(350f, 350f) : new Vector2(1585f + 5f * (float)l, (ModManager.MSC ? 240f : 148f) - 5f * (float)l), 6f, 0.5f);
				}
			}
		}
		bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length - 1];
		for (int m = 0; m < bodyChunkConnections.Length; m++)
		{
			bodyChunkConnections[m] = new BodyChunkConnection(base.bodyChunks[m], base.bodyChunks[m + 1], 9f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		}
		mySwarmers = new List<OracleSwarmer>();
		base.airFriction = 0.99f;
		base.gravity = ((ID == OracleID.SL || (ModManager.MSC && ID == MoreSlugcatsEnums.OracleID.CL)) ? 0.9f : 0f);
		bounce = 0.1f;
		surfaceFriction = 0.17f;
		collisionLayer = 1;
		base.waterFriction = 0.92f;
		base.buoyancy = 0.95f;
		if (ModManager.MSC && ID == MoreSlugcatsEnums.OracleID.ST)
		{
			oracleBehavior = new STOracleBehavior(this);
			arm = new OracleArm(this);
			return;
		}
		if (ModManager.MSC && ID == MoreSlugcatsEnums.OracleID.DM)
		{
			MoonLight = new LightSource(base.bodyChunks[0].pos, environmentalLight: false, new Color(0f, 0f, 1f), this);
			base.room.AddObject(MoonLight);
			MoonLight.HardSetAlpha(0.8f);
			MoonLight.HardSetRad(200f);
		}
		if (ModManager.MSC && (ID == OracleID.SS || ID == MoreSlugcatsEnums.OracleID.DM) && Region.IsRubiconRegion(room.world.name))
		{
			oracleBehavior = new SSOracleBehavior(this);
			arm = new OracleArm(this);
			marbles = new List<PebblesPearl>();
			room.gravity = 0f;
			for (int n = 0; n < room.updateList.Count; n++)
			{
				if (room.updateList[n] is AntiGravity)
				{
					(room.updateList[n] as AntiGravity).active = false;
					break;
				}
			}
			return;
		}
		if (ModManager.MSC && ID == OracleID.SS && room.world.name == "RM")
		{
			oracleBehavior = new SSOracleRotBehavior(this);
			arm = new OracleArm(this);
			marbles = new List<PebblesPearl>();
			SetUpMarbles();
			return;
		}
		if (ID == OracleID.SS || (ModManager.MSC && ID == MoreSlugcatsEnums.OracleID.DM))
		{
			oracleBehavior = new SSOracleBehavior(this);
			myScreen = new OracleProjectionScreen(room, oracleBehavior);
			room.AddObject(myScreen);
			marbles = new List<PebblesPearl>();
			SetUpMarbles();
			room.gravity = 0f;
			for (int num = 0; num < room.updateList.Count; num++)
			{
				if (room.updateList[num] is AntiGravity)
				{
					(room.updateList[num] as AntiGravity).active = false;
					break;
				}
			}
			arm = new OracleArm(this);
		}
		if (ID == OracleID.SL)
		{
			if (room.game.session is StoryGameSession && ((room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark || (ModManager.MSC && room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)))
			{
				oracleBehavior = new SLOracleBehaviorHasMark(this);
			}
			else
			{
				oracleBehavior = new SLOracleBehaviorNoMark(this);
			}
			SetUpSwarmers();
			arm = new OracleArm(this);
		}
		else if (ModManager.MSC && ID == MoreSlugcatsEnums.OracleID.CL)
		{
			oracleBehavior = new CLOracleBehavior(this);
			marbles = new List<PebblesPearl>();
			SetUpMarbles();
			arm = new OracleArm(this);
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new OracleGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (dazed > 0f)
		{
			dazed -= 1f;
		}
		if (MoonLight != null)
		{
			MoonLight.HardSetPos(base.bodyChunks[0].pos);
		}
		if (Consious)
		{
			behaviorTime++;
			oracleBehavior.Update(eu);
		}
		else
		{
			oracleBehavior.UnconciousUpdate();
		}
		if (base.graphicsModule != null && !(base.graphicsModule as OracleGraphics).initiated)
		{
			(base.graphicsModule as OracleGraphics).initiated = true;
			for (int i = 0; i < 100; i++)
			{
				(base.graphicsModule as OracleGraphics).Update();
			}
		}
		if (arm != null)
		{
			arm.Update();
		}
		if (spasms > 0)
		{
			spasms--;
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				base.bodyChunks[j].vel += Custom.RNV() * (UnityEngine.Random.value * 5f);
			}
		}
		if (stun > 0)
		{
			stun--;
		}
		if (health < 1f && UnityEngine.Random.value > Mathf.Pow(health, 0.2f) * 2.5f)
		{
			stun = Math.Max(stun, UnityEngine.Random.Range(2, 8));
		}
		if (!(ID == OracleID.SL))
		{
			return;
		}
		for (int k = 0; k < mySwarmers.Count; k++)
		{
			if (mySwarmers[k].room != room || mySwarmers[k].slatedForDeletetion)
			{
				Custom.Log("SL SWARMER EATEN OR LEFT WITH");
				(oracleBehavior as SLOracleBehavior).State.neuronsLeft--;
				mySwarmers.RemoveAt(k);
				if ((oracleBehavior as SLOracleBehavior).State.neuronsLeft == 0 && room.game.session is ArenaGameSession)
				{
					room.game.GetArenaGameSession.exitManager.challengeCompletedA = true;
					room.game.GetArenaGameSession.exitManager.challengeCompletedB = true;
				}
				if (room.game.session is StoryGameSession)
				{
					(room.game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.likesPlayer = -1f;
					(room.game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.angryWithPlayer = true;
				}
				break;
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!Consious || !(otherObject is Player) || !(oracleBehavior is SLOracleBehaviorHasMark))
		{
			return;
		}
		bool flag = true;
		if (oracleBehavior is SLOracleBehaviorHasMark && (oracleBehavior as SLOracleBehaviorHasMark).State.SpeakingTerms)
		{
			for (int num = (otherObject as Player).grasps.Length - 1; num >= 0; num--)
			{
				if ((otherObject as Player).grasps[num] != null && (oracleBehavior as SLOracleBehaviorHasMark).currentConversation == null && (oracleBehavior as SLOracleBehaviorHasMark).WillingToInspectItem((otherObject as Player).grasps[num].grabbed))
				{
					bool flag2 = false;
					for (int i = 0; i < (oracleBehavior as SLOracleBehaviorHasMark).pickedUpItemsThisRealization.Count; i++)
					{
						if ((oracleBehavior as SLOracleBehaviorHasMark).pickedUpItemsThisRealization[i] == (otherObject as Player).grasps[num].grabbed.abstractPhysicalObject.ID)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						(oracleBehavior as SLOracleBehaviorHasMark).GrabObject((otherObject as Player).grasps[num].grabbed);
						(otherObject as Player).ReleaseGrasp(num);
					}
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			(oracleBehavior as SLOracleBehaviorHasMark).playerAnnoyingCounter++;
		}
	}

	public void SetUpMarbles()
	{
		Vector2 vector = new Vector2(200f, 100f);
		PhysicalObject physicalObject = this;
		if (ModManager.MSC && room.world.name == "RM")
		{
			vector = new Vector2(1220f, 840f);
			physicalObject = null;
		}
		if (ModManager.MSC && room.world.name == "CL")
		{
			vector = new Vector2(1815f, 350f);
			physicalObject = null;
		}
		for (int i = 0; i < 6; i++)
		{
			PhysicalObject orbitObj = physicalObject;
			Vector2 ps = new Vector2(vector.x + 300f, vector.y + 200f) + Custom.RNV() * 20f;
			int color;
			switch (i)
			{
			default:
				color = 0;
				break;
			case 5:
				color = 2;
				break;
			case 2:
			case 3:
				color = 1;
				break;
			}
			CreateMarble(orbitObj, ps, 0, 35f, color);
		}
		for (int j = 0; j < 2; j++)
		{
			CreateMarble(physicalObject, new Vector2(vector.x + 300f, vector.y + 200f) + Custom.RNV() * 20f, 1, 100f, (j == 1) ? 2 : 0);
		}
		CreateMarble(null, new Vector2(vector.x + 20f, vector.y + 200f), 0, 0f, 1);
		Vector2 vector2 = new Vector2(vector.x + 80f, vector.y + 30f);
		Vector2 vector3 = Custom.DegToVec(-32.7346f);
		Vector2 vector4 = Custom.PerpendicularVector(vector3);
		if (!ModManager.MSC || room.world.name != "CL")
		{
			for (int k = 0; k < 3; k++)
			{
				for (int l = 0; l < 5; l++)
				{
					if (k != 2 || l != 2)
					{
						CreateMarble(null, vector2 + vector4 * ((float)k * 17f) + vector3 * ((float)l * 17f), 0, 0f, ((k != 2 || l != 0) && (k != 1 || l != 3)) ? 1 : 2);
					}
				}
			}
		}
		CreateMarble(null, new Vector2(vector.x + 487f, vector.y + 218f), 0, 0f, 1);
		if (!ModManager.MSC || room.world.name != "CL")
		{
			CreateMarble(marbles[marbles.Count - 1], new Vector2(vector.x + 487f, vector.y + 218f), 0, 18f, 0);
		}
		CreateMarble(null, new Vector2(vector.x + 450f, vector.y + 467f), 0, 0f, 2);
		CreateMarble(marbles[marbles.Count - 1], new Vector2(vector.x + 440f, vector.y + 477f), 0, 38f, 1);
		CreateMarble(marbles[marbles.Count - 2], new Vector2(vector.x + 440f, vector.y + 477f), 0, 38f, 2);
		CreateMarble(null, new Vector2(vector.x + 117f, vector.y), 0, 0f, 2);
		if (!ModManager.MSC || room.world.name != "CL")
		{
			CreateMarble(null, new Vector2(vector.x + 547f, vector.y + 374f), 0, 0f, 0);
			CreateMarble(null, new Vector2(vector.x + 114f, vector.y + 500f), 0, 0f, 2);
			CreateMarble(null, new Vector2(vector.x + 108f, vector.y + 511f), 0, 0f, 2);
			CreateMarble(null, new Vector2(vector.x + 551f, vector.y + 131f), 0, 0f, 1);
			CreateMarble(null, new Vector2(vector.x + 560f, vector.y + 124f), 0, 0f, 1);
			CreateMarble(null, new Vector2(vector.x + 520f, vector.y + 134f), 0, 0f, 0);
		}
		CreateMarble(null, new Vector2(vector.x + 109f, vector.y + 352f), 0, 0f, 0);
		if (!ModManager.MSC || room.world.name != "CL")
		{
			CreateMarble(marbles[marbles.Count - 1], new Vector2(vector.x + 109f, vector.y + 352f), 0, 42f, 1);
			marbles[marbles.Count - 1].orbitSpeed = 0.8f;
		}
		CreateMarble(marbles[marbles.Count - 1], new Vector2(vector.x + 109f, vector.y + 352f), 0, 12f, 0);
	}

	public void CreateMarble(PhysicalObject orbitObj, Vector2 ps, int circle, float dist, int color)
	{
		if (pearlCounter == 0)
		{
			pearlCounter = 1;
		}
		AbstractPhysicalObject abstractPhysicalObject = new PebblesPearl.AbstractPebblesPearl(room.world, null, room.GetWorldCoordinate(ps), room.game.GetNewID(), -1, -1, null, color, pearlCounter * ((!ModManager.MSC || !(room.world.name == "DM")) ? 1 : (-1)));
		pearlCounter++;
		room.abstractRoom.entities.Add(abstractPhysicalObject);
		PebblesPearl pebblesPearl = new PebblesPearl(abstractPhysicalObject, room.world);
		pebblesPearl.oracle = this;
		pebblesPearl.firstChunk.HardSetPosition(ps);
		pebblesPearl.orbitObj = orbitObj;
		if (orbitObj == null)
		{
			pebblesPearl.hoverPos = ps;
		}
		pebblesPearl.orbitCircle = circle;
		pebblesPearl.orbitDistance = dist;
		pebblesPearl.marbleColor = (abstractPhysicalObject as PebblesPearl.AbstractPebblesPearl).color;
		if (ModManager.MSC)
		{
			pebblesPearl.marbleIndex = marbles.Count;
		}
		room.AddObject(pebblesPearl);
		marbles.Add(pebblesPearl);
	}

	public void SetUpSwarmers()
	{
		glowers = 5;
		if (room.game.session is StoryGameSession)
		{
			glowers = (room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.neuronsLeft;
			if (ModManager.MSC && (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripMoon)
			{
				glowers = 0;
			}
		}
		Custom.Log("Moon spawning", glowers.ToString(), "swarmers");
		for (int i = 0; i < glowers; i++)
		{
			Vector2 vector = oracleBehavior.OracleGetToPos + new Vector2(0f, 100f) + Custom.RNV() * (UnityEngine.Random.value * 50f);
			SLOracleSwarmer sLOracleSwarmer = new SLOracleSwarmer(new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer, null, room.GetWorldCoordinate(vector), room.game.GetNewID()), room.world);
			room.abstractRoom.entities.Add(sLOracleSwarmer.abstractPhysicalObject);
			sLOracleSwarmer.firstChunk.HardSetPosition(vector);
			room.AddObject(sLOracleSwarmer);
			mySwarmers.Add(sLOracleSwarmer);
			if (i == 0 && room.game.session is StoryGameSession && (room.game.session as StoryGameSession).saveState.miscWorldSaveData.SLOracleState.playerEncounters == 0)
			{
				sLOracleSwarmer.hoverAtGrabablePos = true;
			}
		}
		health = Mathf.InverseLerp(0f, 5f, glowers);
	}

	public void GlowerEaten()
	{
		if (glowers > 0)
		{
			health -= 1f / (float)glowers;
		}
		base.firstChunk.vel += Custom.DegToVec(-45f + UnityEngine.Random.value * 90f) * 4f;
		spasms = 44;
		stun = Math.Max(stun, 183);
		if (ID == OracleID.SL)
		{
			(oracleBehavior as SLOracleBehavior).Pain();
			if (ModManager.MSC && oracleBehavior is SLOracleBehaviorHasMark && room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && room.game.IsMoonActive())
			{
				(oracleBehavior as SLOracleBehaviorHasMark).DeathDenial();
			}
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (!Alive)
		{
			return;
		}
		if (ModManager.MSC && ID == MoreSlugcatsEnums.OracleID.DM)
		{
			room.PlaySound(SoundID.SL_AI_Pain_2, base.firstChunk).requireActiveUpkeep = false;
		}
		if (ID == OracleID.SS && (!ModManager.MSC || !Region.IsRubiconRegion(room.world.name)))
		{
			if (ModManager.MSC && oracleBehavior is SSOracleRotBehavior)
			{
				if ((oracleBehavior as SSOracleRotBehavior).conversation != null)
				{
					if (UnityEngine.Random.value < 0.5f)
					{
						room.PlaySound(SoundID.SS_AI_Talk_1, base.firstChunk).requireActiveUpkeep = false;
					}
					else
					{
						room.PlaySound(SoundID.SS_AI_Talk_4, base.firstChunk).requireActiveUpkeep = false;
					}
					if (!(oracleBehavior as SSOracleRotBehavior).tolleratedHit)
					{
						(oracleBehavior as SSOracleRotBehavior).tolleratedHit = true;
						(oracleBehavior as SSOracleRotBehavior).conversation.Interrupt(oracleBehavior.Translate("Stop. If your intent is to damage my systems further, then I have no reason to continue this conversation."), 0);
					}
					else
					{
						(oracleBehavior as SSOracleRotBehavior).conversation.Interrupt(oracleBehavior.Translate("Very well, you are on your own, creature. Please leave."), 0);
						(oracleBehavior as SSOracleRotBehavior).conversation.Destroy();
					}
				}
				else if (!(oracleBehavior as SSOracleRotBehavior).tolleratedHit)
				{
					(oracleBehavior as SSOracleRotBehavior).tolleratedHit = true;
					(oracleBehavior as SSOracleRotBehavior).dialogBox.Interrupt(oracleBehavior.Translate("Don't. My systems have already been damaged enough as it is. Go away."), 0);
				}
			}
			else if (ModManager.MSC && room.game.IsStorySession && room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				(oracleBehavior as SSOracleBehavior).ReactToHitWeapon();
			}
			else
			{
				(oracleBehavior as SSOracleBehavior).NewAction(SSOracleBehavior.Action.ThrowOut_KillOnSight);
			}
		}
		else if (ID == OracleID.SL)
		{
			(oracleBehavior as SLOracleBehavior).Pain();
			if (ModManager.MSC && oracleBehavior is SLOracleBehaviorHasMark && room.game.IsStorySession && room.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && room.game.IsMoonActive())
			{
				(oracleBehavior as SLOracleBehaviorHasMark).PainDenial();
			}
		}
		else if (ModManager.MSC && ID == MoreSlugcatsEnums.OracleID.CL)
		{
			(oracleBehavior as CLOracleBehavior).Pain();
		}
	}

	public override void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
	{
		base.GraphicsModuleUpdated(actuallyViewed, eu);
		if (oracleBehavior is SSOracleBehavior && (oracleBehavior as SSOracleBehavior).currSubBehavior is SSOracleBehavior.SSOracleGetGreenNeuron)
		{
			((oracleBehavior as SSOracleBehavior).currSubBehavior as SSOracleBehavior.SSOracleGetGreenNeuron).HoldingNeuronUpdate(eu);
		}
	}

	public void setGravity(float gravity)
	{
		base.gravity = gravity;
	}

	public override void Destroy()
	{
		base.Destroy();
		if (room != null)
		{
			for (int num = room.drawableObjects.Count - 1; num >= 0; num--)
			{
				if (room.drawableObjects[num] == base.graphicsModule)
				{
					RemoveGraphicsModule();
					room.game.cameras[0].ReplaceDrawable(room.drawableObjects[num], null);
					room.drawableObjects.RemoveAt(num);
					break;
				}
			}
		}
		base.graphicsModule = null;
		arm = null;
	}
}
