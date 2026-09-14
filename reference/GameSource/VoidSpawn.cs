using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Watcher;

public class VoidSpawn : PhysicalObject, IRippleSpawn
{
	public class SpawnType : ExtEnum<SpawnType>
	{
		public static SpawnType VoidSpawn = new SpawnType("VoidSpawn", register: true);

		public static SpawnType RippleSpawn = new SpawnType("RippleSpawn", register: true);

		public static SpawnType RippleAmoeba = new SpawnType("RippleAmoeba", register: true);

		public static SpawnType RippleJelly = new SpawnType("RippleJelly", register: true);

		public static SpawnType RippleNoodle = new SpawnType("RippleNoodle", register: true);

		public static SpawnType ElderRipple = new SpawnType("ElderRipple", register: true);

		public SpawnType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public abstract class Behavior
	{
		public VoidSpawn owner;

		public virtual Vector2 SwimTowards => owner.mainBody[0].pos;

		public virtual Vector2 Direction => Custom.DirVec(owner.mainBody[0].pos, SwimTowards);

		public Behavior(VoidSpawn owner)
		{
			this.owner = owner;
		}
	}

	public class PassThrough : Behavior
	{
		public int toRoom;

		public Vector2 pnt;

		public Vector2 finalDest;

		public override Vector2 SwimTowards => Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(500f, 10f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));

		public PassThrough(VoidSpawn owner, int toRoom, Room room)
			: base(owner)
		{
			this.toRoom = toRoom;
			pnt = room.RandomPos();
			Vector2 p = room.world.RoomToWorldPos(owner.mainBody[0].pos, room.abstractRoom.index);
			Vector2 vector = room.world.RoomToWorldPos(new Vector2((float)room.world.GetAbstractRoom(toRoom).size.x * UnityEngine.Random.value * 20f, (float)room.world.GetAbstractRoom(toRoom).size.y * UnityEngine.Random.value * 20f), toRoom);
			finalDest = vector - room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index);
			finalDest += Custom.DirVec(p, vector) * 2000f;
		}
	}

	public class MillAround : Behavior
	{
		public Room room;

		public Vector2 dest;

		public FloatRect rect;

		public int destShifts;

		public override Vector2 SwimTowards
		{
			get
			{
				if (Custom.DistLess(dest, owner.mainBody[0].pos, 100f))
				{
					NewDest();
				}
				return dest;
			}
		}

		public MillAround(VoidSpawn owner, Room room)
			: base(owner)
		{
			this.room = room;
			rect = MillRectInRoom(room.abstractRoom.name);
			NewDest();
		}

		public void NewDest()
		{
			destShifts++;
			if (destShifts > UnityEngine.Random.Range(4, 8))
			{
				dest = room.RandomPos() + Custom.RNV() * 50000f;
				dest = Custom.RectCollision(room.RandomPos(), dest, room.RoomRect.Grow(700f)).GetCorner(FloatRect.CornerLabel.D);
			}
			else
			{
				dest = Custom.RandomPointInRect(rect);
			}
		}

		public static FloatRect MillRectInRoom(string roomName)
		{
			switch (roomName)
			{
			case "SH_D02":
				return new FloatRect(900f, 260f, 3800f, 360f);
			case "SH_E02":
				return new FloatRect(380f, 260f, 2460f, 360f);
			case "SB_D06":
				if (ModManager.MSC)
				{
					return new FloatRect(-140f, 60f, 1100f, 370f);
				}
				break;
			case "SB_E05SAINT":
				if (ModManager.MSC)
				{
					return new FloatRect(-140f, 846f, 1120f, 1570f);
				}
				break;
			case "SB_L01":
				return new FloatRect(2000f, 1170f, 3500f, 1300f);
			}
			return default(FloatRect);
		}
	}

	public class VoidSeaDive : Behavior
	{
		public Vector2 pnt;

		public Vector2 finalDest;

		public override Vector2 SwimTowards => Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(700f, 100f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));

		public VoidSeaDive(VoidSpawn owner, Room room)
			: base(owner)
		{
			finalDest = new Vector2(2500f, 360f);
			pnt = Custom.RandomPointInRect(new FloatRect(2000f, 1170f, 3500f, 1300f));
		}
	}

	public class EggToExit : Behavior
	{
		public int toExit;

		public Vector2 pnt;

		public Vector2 finalDest;

		public bool allTheWayToShortcut;

		public bool phase;

		public override Vector2 SwimTowards
		{
			get
			{
				if (!phase)
				{
					if (Custom.DistLess(owner.mainBody[0].pos, pnt, allTheWayToShortcut ? 70f : 170f))
					{
						phase = true;
					}
					return pnt;
				}
				if (allTheWayToShortcut)
				{
					return finalDest;
				}
				return Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(200f, 10f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));
			}
		}

		public EggToExit(VoidSpawn owner, int toExit, Room room, bool allTheWayToShortcut)
			: base(owner)
		{
			this.toExit = toExit;
			this.allTheWayToShortcut = allTheWayToShortcut;
			pnt = room.MiddleOfTile(room.ShortcutLeadingToNode(toExit).StartTile);
			if (!allTheWayToShortcut)
			{
				pnt += Custom.RNV() * (70f * UnityEngine.Random.value);
			}
			int num = room.abstractRoom.connections[toExit];
			if (num > -1)
			{
				Vector2 p = room.world.RoomToWorldPos(owner.mainBody[0].pos, room.abstractRoom.index);
				Vector2 vector = room.world.RoomToWorldPos(new Vector2((float)room.world.GetAbstractRoom(num).size.x * UnityEngine.Random.value * 20f, (float)room.world.GetAbstractRoom(num).size.y * UnityEngine.Random.value * 20f), num);
				finalDest = vector - room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index);
				finalDest += Custom.DirVec(p, vector) * 2000f;
			}
			else
			{
				finalDest = pnt + Custom.DirVec(room.RandomPos(), pnt) * 10000f;
			}
		}
	}

	public class EggToRoom : Behavior
	{
		public Vector2 pnt;

		public override Vector2 SwimTowards => pnt;

		public EggToRoom(VoidSpawn owner, Room room, AbstractRoom targetRoom)
			: base(owner)
		{
			Vector2 vector = room.world.RoomToWorldPos(owner.firstChunk.pos, room.abstractRoom.index);
			Vector2 vector2 = room.world.RoomToWorldPos(Vector2.zero, targetRoom.index);
			Vector2 vector3 = room.world.RoomToWorldPos(new Vector2((float)targetRoom.size.x * 20f, (float)targetRoom.size.y * 20f), targetRoom.index);
			Vector2 v = new Vector2(Mathf.Lerp(vector2.x, vector3.x, UnityEngine.Random.value), Mathf.Lerp(vector2.y, vector3.y, UnityEngine.Random.value)) - vector;
			float magnitude = v.magnitude;
			float num = Custom.VecToDeg(v);
			float num2 = Custom.LerpMap(magnitude, 2000f, 8000f, 10f, 40f);
			num += UnityEngine.Random.Range(0f - num2, num2);
			pnt = owner.firstChunk.pos + Custom.DegToVec(num) * magnitude;
		}
	}

	public class EggToPos : Behavior
	{
		public Vector2 pnt;

		public override Vector2 SwimTowards => pnt;

		public EggToPos(VoidSpawn owner, Room room, Vector2 targetPoint)
			: base(owner)
		{
			pnt = targetPoint;
		}
	}

	public class EggAndAway : Behavior
	{
		public Vector2 pnt;

		public Vector2 finalDest;

		public bool phase;

		public override Vector2 SwimTowards
		{
			get
			{
				if (!phase)
				{
					if (Custom.DistLess(owner.mainBody[0].pos, pnt, 170f))
					{
						phase = true;
					}
					return pnt;
				}
				return Vector2.Lerp(pnt, finalDest, Mathf.InverseLerp(200f, 10f, Mathf.Abs(Custom.DistanceToLine(owner.mainBody[0].pos, pnt, finalDest))));
			}
		}

		public EggAndAway(VoidSpawn owner, Room room)
			: base(owner)
		{
			pnt = owner.firstChunk.pos + Custom.RNV() * (UnityEngine.Random.value * 400f);
			finalDest = owner.firstChunk.pos + Custom.RNV() * (Mathf.Max(room.PixelWidth, room.PixelHeight) * 3f);
		}
	}

	public class SwimDown : Behavior
	{
		public override Vector2 SwimTowards => new Vector2(owner.mainBody[0].pos.x, owner.mainBody[1].pos.y - 100f);

		public SwimDown(VoidSpawn owner, Room room)
			: base(owner)
		{
		}
	}

	public class ChasePlayer : Behavior
	{
		public override Vector2 SwimTowards
		{
			get
			{
				Player player = null;
				float num = 0f;
				foreach (AbstractCreature player3 in owner.room.game.Players)
				{
					Player player2 = null;
					if (player3.realizedCreature != null)
					{
						player2 = player3.realizedCreature as Player;
					}
					if (player2 != null && player2.room != null && player2.room.abstractRoom.index == owner.room.abstractRoom.index)
					{
						float num2 = Vector2.Distance(owner.firstChunk.pos, player2.mainBodyChunk.pos);
						if (player == null || num2 < num)
						{
							player = player2;
							num = num2;
						}
						if (owner.variant != SpawnType.RippleAmoeba && num2 < 400f)
						{
							owner.behavior = new CircleSwarm(owner, owner.room);
						}
					}
				}
				if (player != null)
				{
					if (player.standingInWarpPointProtectionTime > 0 || player.warpPointCooldown > 0 || (owner.room.game.IsStorySession && owner.room.game.GetStorySession.finalWarpSequenceStarted))
					{
						return owner.mainBody[0].pos + Custom.DirVec(player.mainBodyChunk.pos, owner.mainBody[0].pos) * 400f;
					}
					return player.mainBodyChunk.pos;
				}
				return new Vector2(owner.mainBody[0].pos.x, owner.mainBody[1].pos.y);
			}
		}

		public ChasePlayer(VoidSpawn owner, Room room)
			: base(owner)
		{
		}
	}

	public class JellyBehavior : Behavior
	{
		public bool direction;

		public float velocityLoss;

		public bool goingDown;

		public float normalSwimSpeed;

		public override Vector2 SwimTowards
		{
			get
			{
				velocityLoss += (goingDown ? (-0.025f) : 0.05f);
				if (velocityLoss >= 1f || velocityLoss <= 0f)
				{
					goingDown = !goingDown;
				}
				owner.swimSpeed = Mathf.Lerp(0f, normalSwimSpeed, velocityLoss);
				return owner.mainBody[0].pos + new Vector2(80f * (float)((!direction) ? 1 : (-1)) + UnityEngine.Random.Range(50f, 50f), 80f);
			}
		}

		public JellyBehavior(VoidSpawn owner, Room room)
			: base(owner)
		{
			direction = UnityEngine.Random.value < 0.5f;
			normalSwimSpeed = owner.swimSpeed;
		}
	}

	public class Flee : Behavior
	{
		public float boredom;

		public override Vector2 SwimTowards
		{
			get
			{
				Player player = null;
				float num = 0f;
				Vector2 vector = default(Vector2);
				foreach (AbstractCreature player3 in owner.room.game.Players)
				{
					Player player2 = null;
					if (player3.realizedCreature != null)
					{
						player2 = player3.realizedCreature as Player;
					}
					if (player2 != null && player2.room != null && player2.room.abstractRoom.index == owner.room.abstractRoom.index)
					{
						float num2 = Vector2.Distance(owner.firstChunk.pos, player2.mainBodyChunk.pos);
						if (player == null || num2 < num)
						{
							player = player2;
							num = num2;
						}
					}
				}
				boredom += 0.02f;
				if (UnityEngine.Random.value < boredom)
				{
					owner.behavior = new BezierSwarm(owner, owner.room);
				}
				if (player != null && num < 900f)
				{
					vector = Custom.DirVec(player.mainBodyChunk.pos, owner.firstChunk.pos);
					bool flag = vector.y > 0f;
					return new Vector2((owner.firstChunk.pos + vector.normalized * 5f).x, owner.firstChunk.pos.y + (flag ? 1f : (-1f)) * 5f);
				}
				return new Vector2(owner.mainBody[0].pos.x, owner.mainBody[1].pos.y);
			}
		}

		public Flee(VoidSpawn owner, Room room)
			: base(owner)
		{
		}
	}

	public class Jitter : Behavior
	{
		public Vector2 spot;

		public int jitterTime = 500;

		public override Vector2 SwimTowards
		{
			get
			{
				if (jitterTime <= 0)
				{
					jitterTime = 800;
					spot = owner.mainBody[0].pos + Custom.IntVector2ToVector2(Custom.eightDirections[UnityEngine.Random.Range(0, Custom.eightDirections.Length - 1)]) * 10f;
				}
				else
				{
					jitterTime--;
				}
				return spot;
			}
		}

		public Jitter(VoidSpawn owner, Room room)
			: base(owner)
		{
			spot = owner.mainBody[0].pos + Custom.IntVector2ToVector2(Custom.eightDirections[UnityEngine.Random.Range(0, Custom.eightDirections.Length - 1)]) * 20f;
		}
	}

	public class Swarm : Behavior
	{
		public VoidSpawn swarmLeader;

		public float swarmSpot;

		public int checkForNewLeader = 250;

		public int checkForNewLeaderAdditionalCD;

		public Vector2 dest;

		public override Vector2 SwimTowards
		{
			get
			{
				if (checkForNewLeader == 0 || swarmLeader == null)
				{
					GetNewLeader();
				}
				else
				{
					checkForNewLeader--;
				}
				if (Custom.Dist(owner.firstChunk.pos, dest) < 50f + 200f * Mathf.InverseLerp(1f, 0f, owner.dominance))
				{
					GetNewDest();
				}
				return dest;
			}
		}

		public Swarm(VoidSpawn owner, Room room)
			: base(owner)
		{
		}

		public void GetNewDest()
		{
			if (swarmLeader != null)
			{
				Vector2 vector = Custom.DirVec(swarmLeader.bodyChunks[1].pos, swarmLeader.bodyChunks[0].pos) + Custom.DegToVec(45f - 45f * Mathf.InverseLerp(0f, 1f, owner.dominance)) * ((UnityEngine.Random.value < 0.5f) ? 1f : (-1f));
				dest = swarmLeader.firstChunk.pos + vector * (5f - Mathf.InverseLerp(0f, 1f, owner.dominance));
			}
		}

		public void GetNewLeader()
		{
			foreach (VoidSpawn voidSpawn in owner.room.voidSpawns)
			{
				if ((swarmLeader == null || swarmLeader.dominance < voidSpawn.dominance) && voidSpawn.dominance > owner.dominance)
				{
					swarmLeader = voidSpawn;
				}
			}
			checkForNewLeaderAdditionalCD += 500;
			checkForNewLeader = 1000 + checkForNewLeaderAdditionalCD;
			GetNewDest();
		}
	}

	public class CircleSwarm : Behavior
	{
		public float currentDegree;

		public Vector2 currentCenter;

		public int currentDirection;

		public Vector2 variance;

		public float boredom;

		public int boredomTickCheck = 90;

		public Vector2 positionPlusVariance
		{
			get
			{
				float num = 1f;
				if (Mathf.Abs(Custom.eightDirections[currentDirection].x) == 1 && Mathf.Abs(Custom.eightDirections[currentDirection].y) == 1)
				{
					num = 0.75f;
				}
				return variance + Custom.IntVector2ToVector2(Custom.eightDirections[currentDirection]) * 165f * num;
			}
		}

		public override Vector2 SwimTowards
		{
			get
			{
				boredom += 0.002f;
				boredomTickCheck--;
				if (boredomTickCheck <= 0)
				{
					boredomTickCheck = 90;
					if (UnityEngine.Random.value < boredom)
					{
						owner.behavior = new Flee(owner, owner.room);
					}
				}
				using (List<AbstractCreature>.Enumerator enumerator = owner.room.game.Players.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						AbstractCreature current = enumerator.Current;
						Player player = null;
						if (current.realizedCreature != null)
						{
							player = current.realizedCreature as Player;
						}
						if (player != null && player.room != null && player.room.abstractRoom.index == owner.room.abstractRoom.index)
						{
							if (Custom.DistLess(owner.firstChunk.pos, player.mainBodyChunk.pos + positionPlusVariance, 45f))
							{
								currentDirection++;
								if (currentDirection >= Custom.eightDirections.Length)
								{
									currentDirection = 0;
								}
								GetNewVariance();
							}
							return player.mainBodyChunk.pos + positionPlusVariance;
						}
						owner.behavior = new BezierSwarm(owner, owner.room);
						return owner.mainBody[0].pos;
					}
				}
				return Vector2.zero;
			}
		}

		public CircleSwarm(VoidSpawn owner, Room room)
			: base(owner)
		{
			currentDirection = UnityEngine.Random.Range(0, 7);
			GetNewVariance();
		}

		public void GetNewVariance()
		{
			variance = new Vector2(UnityEngine.Random.Range(-45f, 45f), UnityEngine.Random.Range(-45f, 45f));
		}

		public void GetNewCenter()
		{
			_ = UnityEngine.Random.value;
			_ = 0.5f;
		}
	}

	public class BezierSwarm : Behavior
	{
		public Vector2[] BezierPoints = new Vector2[4];

		public float currentT;

		public bool hasBezierPoints;

		public int followingAmount;

		public bool isFollowing;

		public Vector2 variance;

		public BezierSwarm leader;

		public float interestInPlayer;

		public override Vector2 SwimTowards
		{
			get
			{
				if (BezierPoints == null || !hasBezierPoints)
				{
					GetNewBezier();
					return owner.firstChunk.pos;
				}
				Vector2 vector = Custom.Bezier(BezierPoints[0], BezierPoints[1], BezierPoints[2], BezierPoints[3], currentT);
				foreach (AbstractCreature player2 in owner.room.game.Players)
				{
					Player player = null;
					if (player2.realizedCreature != null)
					{
						player = player2.realizedCreature as Player;
					}
					if (player != null && Custom.DistLess(player.firstChunk.pos, owner.mainBody[0].pos, 150f))
					{
						interestInPlayer = Mathf.Clamp01(interestInPlayer + 0.01f);
						if (UnityEngine.Random.value < interestInPlayer)
						{
							owner.behavior = new ChasePlayer(owner, owner.room);
							return owner.mainBody[0].pos;
						}
					}
					else
					{
						interestInPlayer = Mathf.Clamp01(interestInPlayer - 0.005f);
					}
				}
				if (Custom.DistLess(owner.firstChunk.pos, vector + variance, 35f))
				{
					if (currentT < 1f)
					{
						variance = new Vector2(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
						currentT += 0.1f;
					}
					else
					{
						GetNewBezier();
					}
				}
				return vector + variance;
			}
		}

		public BezierSwarm(VoidSpawn owner, Room room)
			: base(owner)
		{
			variance = new Vector2(UnityEngine.Random.Range(-150, 150), UnityEngine.Random.Range(-150, 150));
		}

		public void GetNewBezier()
		{
			if (followingAmount < 0)
			{
				if (leader != null)
				{
					BezierPoints = leader.BezierPoints;
					return;
				}
				foreach (VoidSpawn voidSpawn in owner.room.voidSpawns)
				{
					if (voidSpawn.behavior is BezierSwarm)
					{
						BezierSwarm bezierSwarm = voidSpawn.behavior as BezierSwarm;
						if (bezierSwarm.hasBezierPoints && !bezierSwarm.isFollowing && bezierSwarm.followingAmount < 10 && (BezierPoints == null || (BezierPoints != null && bezierSwarm.BezierPoints[0] != BezierPoints[0])))
						{
							leader = bezierSwarm;
							isFollowing = true;
							bezierSwarm.followingAmount++;
							BezierPoints = new Vector2[4];
							BezierPoints = bezierSwarm.BezierPoints;
							hasBezierPoints = true;
							currentT = 0f;
							return;
						}
					}
				}
			}
			if (owner.mainBody[0].pos == Vector2.zero)
			{
				return;
			}
			foreach (AbstractCreature player2 in owner.room.game.Players)
			{
				Player player = null;
				if (player2.realizedCreature != null)
				{
					player = player2.realizedCreature as Player;
				}
				if (player != null && player.room != null && player.room.abstractRoom.index == owner.room.abstractRoom.index && !Custom.DistLess(player.firstChunk.pos, owner.mainBody[0].pos, 75f))
				{
					currentT = 0f;
					BezierPoints = new Vector2[4];
					BezierPoints[0] = owner.mainBody[0].pos;
					BezierPoints[1] = owner.mainBody[0].pos + new Vector2(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-200f, 1000f));
					BezierPoints[2] = player.firstChunk.pos + new Vector2(UnityEngine.Random.Range(-350f, 350f), UnityEngine.Random.Range(-200f, 600f));
					BezierPoints[3] = player.firstChunk.pos;
					hasBezierPoints = true;
					break;
				}
				bool num = owner.room.PixelWidth / 2f < owner.mainBody[0].pos.x;
				currentT = 0f;
				BezierPoints = new Vector2[4];
				BezierPoints[0] = owner.mainBody[0].pos;
				BezierPoints[1] = owner.mainBody[0].pos + new Vector2(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-200f, 1000f));
				if (num)
				{
					Vector2 vector = new Vector2(UnityEngine.Random.Range(0f, owner.room.PixelWidth / 2f), owner.room.PixelHeight / 2f);
					BezierPoints[2] = new Vector2(Mathf.Clamp(vector.x + UnityEngine.Random.Range(0f, 1000f), 0f, owner.room.PixelWidth), Mathf.Clamp(vector.y + UnityEngine.Random.Range(-500f, 500f), 0f, owner.room.PixelHeight));
					BezierPoints[3] = vector;
				}
				else
				{
					Vector2 vector2 = new Vector2(UnityEngine.Random.Range(owner.room.PixelWidth / 2f, owner.room.PixelWidth), owner.room.PixelHeight / 2f);
					BezierPoints[2] = new Vector2(Mathf.Clamp(vector2.x + UnityEngine.Random.Range(-1000f, 0f), 0f, owner.room.PixelWidth), Mathf.Clamp(vector2.y + UnityEngine.Random.Range(-500f, 500f), 0f, owner.room.PixelHeight));
					BezierPoints[3] = vector2;
				}
			}
			isFollowing = false;
		}
	}

	public class FollowMigrationStream : Behavior
	{
		public bool endOfPath;

		public float pathProgress;

		public float offset;

		public Vector2 currentPoint;

		public Vector2 nextPoint;

		public VoidSpawnMigrationStream stream;

		public override Vector2 Direction
		{
			get
			{
				Vector2 normalized = (nextPoint - currentPoint).normalized;
				if (endOfPath)
				{
					return normalized;
				}
				Vector2 direction = base.Direction;
				if (pathProgress == 0f)
				{
					return direction;
				}
				float num = Mathf.InverseLerp(50f, 10f, Vector2.Distance(owner.firstChunk.pos, currentPoint));
				return (direction + 2f * num * normalized).normalized;
			}
		}

		public override Vector2 SwimTowards
		{
			get
			{
				if (owner.room.spawnedSpinningTop)
				{
					CheckForSpinningTop();
					if (owner.behavior != this)
					{
						return currentPoint;
					}
				}
				Vector2 pointOnPath = GetPointOnPath(pathProgress);
				Vector2 pointOnPath2 = GetPointOnPath(pathProgress + 1f);
				Vector2 vector = Custom.rotateVectorDeg((pointOnPath2 - pointOnPath).normalized, 90f) * offset;
				currentPoint = pointOnPath + vector;
				nextPoint = pointOnPath2 + vector;
				if (owner.mainBody[0].pos == Vector2.zero)
				{
					return currentPoint;
				}
				while (Vector2.Distance(owner.firstChunk.pos, currentPoint) > Vector2.Distance(owner.firstChunk.pos, nextPoint) && !endOfPath)
				{
					pathProgress += 1f;
					pointOnPath = pointOnPath2;
					pointOnPath2 = GetPointOnPath(pathProgress + 1f);
					vector = Custom.rotateVectorDeg((pointOnPath2 - pointOnPath).normalized, 90f) * offset;
					currentPoint = pointOnPath + vector;
					nextPoint = pointOnPath2 + vector;
				}
				return currentPoint;
			}
		}

		public FollowMigrationStream(VoidSpawn owner, VoidSpawnMigrationStream stream, float pathProgress, float offset)
			: base(owner)
		{
			this.pathProgress = pathProgress;
			this.stream = stream;
			this.offset = offset;
		}

		public Vector2 GetPointOnPath(float pos)
		{
			bool end;
			Vector2 pointOnPath = stream.GetPointOnPath(pos, out end);
			if (end)
			{
				if (stream.worldStream?.nextStream != null)
				{
					Rect roomMapRect = stream.worldStream.GetRoomMapRect(owner.room.abstractRoom);
					Vector2 b = (stream.worldStream.spline.posB - new Vector2(roomMapRect.xMin, roomMapRect.yMin)) * 6.6666665f;
					if (Vector2.Distance(pointOnPath, b) < 5f)
					{
						foreach (UpdatableAndDeletable update in owner.room.updateList)
						{
							if (update is VoidSpawnMigrationStream voidSpawnMigrationStream && voidSpawnMigrationStream.worldStream == stream.worldStream.nextStream)
							{
								NewStream(voidSpawnMigrationStream);
								return pointOnPath;
							}
						}
					}
				}
				endOfPath = true;
				owner.startFadeOut = true;
			}
			return pointOnPath;
		}

		public void CheckForSpinningTop()
		{
			foreach (UpdatableAndDeletable update in owner.room.updateList)
			{
				if (update is Ghost ghost && Vector2.Distance(owner.firstChunk.pos, ghost.pos) < 500f + Mathf.Abs(offset))
				{
					float num = Vector2.SignedAngle((nextPoint - currentPoint).normalized, ghost.pos - currentPoint);
					owner.behavior = new OrbitPoint(owner, ghost.pos, 250f + Mathf.Abs(offset), num < 0f)
					{
						migrationVisuals = true
					};
					owner.timeUntilFadeout = UnityEngine.Random.Range(200, 400);
					break;
				}
			}
		}

		public void NewStream(VoidSpawnMigrationStream newStream)
		{
			stream = newStream;
			float num = float.MaxValue;
			float num2 = 0f;
			bool end = false;
			do
			{
				float num3 = Vector2.Distance(newStream.GetPointOnPath(num2, out end), owner.firstChunk.pos);
				if (num3 < num)
				{
					num = num3;
					pathProgress = num2;
				}
				num2 += 5f;
			}
			while (!end);
		}
	}

	public class MillAroundMigrationStream : Behavior
	{
		public float offset;

		public Vector2 dest;

		public VoidSpawnMigrationStream stream;

		public override Vector2 SwimTowards
		{
			get
			{
				if (Custom.DistLess(dest, owner.mainBody[0].pos, 100f))
				{
					NewDest();
				}
				return dest;
			}
		}

		public MillAroundMigrationStream(VoidSpawn owner, VoidSpawnMigrationStream stream, float offset)
			: base(owner)
		{
			this.stream = stream;
			this.offset = offset;
			NewDest();
		}

		public Vector2 GetPointOnPath(float pos)
		{
			return stream.GetPointOnPath(pos);
		}

		public void NewDest()
		{
			offset *= 1.5f;
			float num = UnityEngine.Random.Range(1f, stream.Spline.GetFullLength);
			Vector2 pointOnPath = stream.GetPointOnPath(num);
			Vector2 pointOnPath2 = stream.GetPointOnPath(num - 1f);
			Vector2 vector = Custom.rotateVectorDeg((pointOnPath - pointOnPath2).normalized, 90f) * UnityEngine.Random.Range(0f, offset);
			dest = pointOnPath + vector;
		}
	}

	public class OrbitPoint : Behavior
	{
		public Vector2 center;

		public float orbitDistance;

		public bool clockwise;

		public Vector2 currentPoint;

		public Vector2 nextPoint;

		public float orbitAngle;

		public float snapDistance = 50f;

		public bool migrationVisuals;

		public override Vector2 Direction
		{
			get
			{
				Vector2 direction = base.Direction;
				Vector2 normalized = (nextPoint - currentPoint).normalized;
				float num = Mathf.InverseLerp(snapDistance, 10f, Vector2.Distance(owner.firstChunk.pos, currentPoint));
				return (direction + 10f * num * normalized).normalized;
			}
		}

		public override Vector2 SwimTowards
		{
			get
			{
				int num = (clockwise ? 1 : (-1));
				currentPoint = GetPointOnOrbit(orbitAngle);
				nextPoint = GetPointOnOrbit(orbitAngle + (float)num);
				if (owner.mainBody[0].pos == Vector2.zero)
				{
					return GetPointOnOrbit(orbitAngle);
				}
				_ = (nextPoint - currentPoint).normalized;
				while (Vector2.Distance(owner.firstChunk.pos, currentPoint) > Vector2.Distance(owner.firstChunk.pos, nextPoint))
				{
					orbitAngle += num;
					currentPoint = nextPoint;
					nextPoint = GetPointOnOrbit(orbitAngle + (float)num);
					_ = (nextPoint - currentPoint).normalized;
				}
				return currentPoint;
			}
		}

		public OrbitPoint(VoidSpawn owner, Vector2 center, float orbitDistance, bool clockwise)
			: base(owner)
		{
			this.center = center;
			this.orbitDistance = orbitDistance;
			this.clockwise = clockwise;
			orbitAngle = Custom.DirVec(center, owner.firstChunk.pos * 20f).GetAngle();
		}

		public virtual Vector2 GetPointOnOrbit(float angle)
		{
			return center + Custom.DegToVec(angle) * orbitDistance;
		}
	}

	public class CollapseOrbit : OrbitPoint
	{
		public float originalOrbitDistance;

		public int timeUntilCollapse;

		public int timer;

		public float lastOrbitAngle;

		public float origSwimSpeed;

		public float flipThreshold;

		public CollapseOrbit(VoidSpawn owner, Vector2 center, float orbitDistance, bool clockwise, int timeUntilCollapse)
			: base(owner, center, orbitDistance, clockwise)
		{
			originalOrbitDistance = orbitDistance;
			this.timeUntilCollapse = timeUntilCollapse;
			lastOrbitAngle = orbitAngle;
			origSwimSpeed = owner.swimSpeed;
		}

		public void UpdateOrbit()
		{
			float num = flipThreshold / owner.sizeFac;
			float toB = (num - origSwimSpeed) * 3f + num;
			owner.swimSpeed = Custom.LerpMap(timer, 0f, timeUntilCollapse, origSwimSpeed, toB);
			timer++;
			float num2 = Vector2.Distance(owner.firstChunk.pos, center);
			float num3 = owner.swimSpeed / num2 * 57.29578f;
			num3 *= 0.75f;
			if (Mathf.DeltaAngle(orbitAngle, lastOrbitAngle) < num3)
			{
				orbitAngle = lastOrbitAngle + num3;
			}
			lastOrbitAngle = orbitAngle;
			orbitDistance = Custom.LerpMap(timer, 0f, timeUntilCollapse, originalOrbitDistance, 0f, 2f);
			if (num2 < 100f && orbitDistance < 100f)
			{
				owner.fade = Mathf.Min(owner.fade, Mathf.Max(0f, Mathf.InverseLerp(10f, 100f, num2)));
				if (owner.fade == 0f)
				{
					owner.Destroy();
				}
			}
		}
	}

	public BodyChunk[] mainBody;

	public float swimCycle;

	public float sizeFac;

	public float swimSpeed;

	public float mainChunkFactor = 0.02f;

	public Behavior behavior;

	public bool canBeDestroyed;

	public bool culled;

	public bool lastCulled;

	public float fade;

	public float lastFade;

	public float inEggMode;

	public bool consious = true;

	public VoidSpawnEgg egg;

	public float voidMeltInRoom;

	public bool dayLightMode;

	public int timeUntilFadeout;

	public bool startFadeOut;

	public SpawnType variant;

	public int playerProximityTime;

	public int tick;

	public float dominance;

	public List<PlacedObject.HideVoidSpawnData> hideZones;

	public ChunkDynamicSoundLoop soundLoop;

	public bool rippleSpawn => variant != SpawnType.VoidSpawn;

	public bool RippleMigration
	{
		get
		{
			if (rippleSpawn)
			{
				if (!(behavior is FollowMigrationStream) && !(behavior is MillAroundMigrationStream))
				{
					if (behavior is OrbitPoint orbitPoint)
					{
						return orbitPoint.migrationVisuals;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public VoidSpawn(AbstractPhysicalObject abstractPhysicalObject, float voidMeltInRoom, bool dayLightMode, SpawnType variant)
		: base(abstractPhysicalObject)
	{
		this.variant = variant;
		GenerateBody();
		this.voidMeltInRoom = voidMeltInRoom;
		this.dayLightMode = dayLightMode;
		base.airFriction = 1f;
		base.gravity = 0f;
		bounce = 0f;
		surfaceFriction = 0.4f;
		collisionLayer = 0;
		base.waterFriction = 1f;
		base.buoyancy = 0f;
		windAffectiveness = 0f;
		currentAffectiveness = 0f;
		swimCycle = UnityEngine.Random.value;
		base.CollideWithTerrain = false;
		base.CollideWithObjects = false;
		canBeHitByWeapons = false;
		hideZones = new List<PlacedObject.HideVoidSpawnData>();
		if (variant == SpawnType.ElderRipple)
		{
			soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
		}
	}

	public VoidSpawn(AbstractPhysicalObject abstractPhysicalObject, float voidMeltInRoom, bool dayLightMode)
		: this(abstractPhysicalObject, voidMeltInRoom, dayLightMode, SpawnType.VoidSpawn)
	{
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		Vector2 vector = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
		Vector2 vector2 = Custom.RNV();
		if (behavior != null)
		{
			vector2 = Custom.DirVec(vector, behavior.SwimTowards);
		}
		if (!(behavior is OrbitPoint) && !(behavior is FollowMigrationStream) && !(behavior is MillAroundMigrationStream))
		{
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].HardSetPosition(vector + vector2 * (base.bodyChunks.Length - i));
			}
		}
		else
		{
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				vector -= vector2 * base.bodyChunks[j].rad;
				base.bodyChunks[j].HardSetPosition(vector);
				vector -= vector2 * base.bodyChunks[j].rad;
			}
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
		hideZones = new List<PlacedObject.HideVoidSpawnData>();
		foreach (PlacedObject placedObject in placeRoom.roomSettings.placedObjects)
		{
			if (placedObject.data is PlacedObject.HideVoidSpawnData hideVoidSpawnData && hideVoidSpawnData.type == variant)
			{
				hideZones.Add(hideVoidSpawnData);
			}
		}
	}

	public void SetTimeUntilFadeOut(int time)
	{
		timeUntilFadeout = time;
	}

	public void ChangeRippleLayer(int rippleLayer, bool showEffect)
	{
		abstractPhysicalObject.rippleLayer = rippleLayer;
		if (showEffect)
		{
			room.AddObject(new ShockWave(base.firstChunk.pos, 50f, UnityEngine.Random.value * 0.075f + 0.05f, UnityEngine.Random.Range(10, 40)));
			room.PlaySound(WatcherEnums.WatcherSoundID.Ripple_Creature_Swap_Dimensions, base.firstChunk.pos);
		}
		else
		{
			fade = 0.01f;
			lastFade = 0.01f;
		}
	}

	public void GenerateBody()
	{
		int num = UnityEngine.Random.Range(3, UnityEngine.Random.Range(3, 16));
		int num2 = 0;
		List<BodyChunk> list = new List<BodyChunk>();
		List<BodyChunkConnection> list2 = new List<BodyChunkConnection>();
		float num3 = 1f;
		if (variant == SpawnType.RippleAmoeba)
		{
			num3 = 2f;
			num = UnityEngine.Random.Range(5, 8);
			mainChunkFactor = 0.2f;
		}
		else if (variant == SpawnType.RippleJelly)
		{
			num = UnityEngine.Random.Range(3, 4);
		}
		else if (variant == SpawnType.RippleNoodle)
		{
			num3 = 0.5f;
			num = UnityEngine.Random.Range(6, 10);
		}
		else if (variant == SpawnType.ElderRipple)
		{
			num3 = 4f;
			num = UnityEngine.Random.Range(20, 25);
			mainChunkFactor = 0.2f;
		}
		float num4 = Mathf.Lerp(3f, 8f, UnityEngine.Random.value);
		if (variant == SpawnType.RippleAmoeba)
		{
			num4 = Mathf.Lerp(8f, 12f, UnityEngine.Random.value);
		}
		else if (variant == SpawnType.RippleNoodle)
		{
			num4 = Mathf.Lerp(1f, 6f, UnityEngine.Random.value);
		}
		else if (variant == SpawnType.ElderRipple)
		{
			num4 = 20f;
		}
		float num5 = Mathf.Lerp(Mathf.Lerp(0.5f, 4f, UnityEngine.Random.value), num4 / 2f, UnityEngine.Random.value);
		float p = Mathf.Lerp(0.1f, 0.7f, UnityEngine.Random.value);
		sizeFac = Mathf.Lerp(0.5f, 1.2f, UnityEngine.Random.value) * num3;
		swimSpeed = Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value);
		dominance = Mathf.InverseLerp(0f, 2.4f, sizeFac);
		dominance *= Mathf.InverseLerp(3f, 8f, num);
		if (variant == SpawnType.ElderRipple)
		{
			num = 20;
			sizeFac = 2.5f;
			num5 = 5f;
			p = 0.4f;
			swimSpeed = 0.7f;
		}
		for (int i = 0; i < num; i++)
		{
			float num6 = (float)i / (float)(num - 1);
			float num7 = Mathf.Lerp(Mathf.Lerp(num4, num5, num6), Mathf.Lerp(num5, num4, Mathf.Sin(Mathf.Pow(num6, p) * (float)Math.PI)), 0.5f) * sizeFac;
			list.Add(new BodyChunk(this, num2, default(Vector2), num7, num7 * 0.1f));
			if (i > 0)
			{
				list2.Add(new BodyChunkConnection(list[i - 1], list[i], Mathf.Lerp((list[i - 1].rad + list[i].rad) * 1.25f, Mathf.Max(list[i - 1].rad, list[i].rad), 0.5f), BodyChunkConnection.Type.Normal, 1f, -1f));
			}
			num2++;
		}
		mainBody = list.ToArray();
		base.bodyChunks = list.ToArray();
		bodyChunkConnections = list2.ToArray();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new VoidSpawnGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		tick++;
		lastFade = fade;
		if ((!dayLightMode && RippleMigration && room.PointSubmerged(mainBody[0].pos)) || startFadeOut || (abstractPhysicalObject.rippleLayer == 1 && room.game.ActiveRippleLayer != 1))
		{
			float num = (rippleSpawn ? (1f / 120f) : 0.05f);
			if (variant == SpawnType.RippleJelly)
			{
				num = 0.05f;
			}
			fade = Mathf.Max(0f, fade - num);
			if (fade == 0f)
			{
				Destroy();
			}
		}
		else
		{
			fade = Mathf.Min(1f, fade + 0.025f);
		}
		if (timeUntilFadeout > 0)
		{
			timeUntilFadeout--;
			if (timeUntilFadeout == 0)
			{
				startFadeOut = true;
			}
		}
		if (inEggMode > 0f)
		{
			inEggMode = Mathf.Max(0f, inEggMode - 1f / 60f);
			if (inEggMode <= 0f)
			{
				egg = null;
			}
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			mainBody[i].vel *= Custom.LerpMap(mainBody[i].vel.magnitude, 0.2f * sizeFac, 6f * sizeFac, 1f, 0.7f);
		}
		if (consious)
		{
			float num2 = swimSpeed;
			float num3 = -1f;
			if (variant == SpawnType.RippleAmoeba && behavior != null)
			{
				float t = Mathf.InverseLerp(25f, 300f, Vector2.Distance(mainBody[0].pos, behavior.SwimTowards)) * Mathf.Abs(Mathf.Sin((float)tick * 1f / ((float)Math.PI * 6f)));
				num3 = Mathf.Lerp(0.5f, 10f, t);
				num2 *= Mathf.Lerp(0.2f, 1.25f, t);
				if (playerProximityTime > 0)
				{
					num3 = 0.1f;
					num2 *= 0.1f;
				}
			}
			if (variant == SpawnType.RippleJelly && behavior != null)
			{
				num2 /= 2f;
			}
			swimCycle += 0.2f * num2;
			Vector2 p = mainBody[mainBody.Length - 1].pos + Custom.DirVec(mainBody[0].pos, mainBody[mainBody.Length - 1].pos) * 100f;
			Vector2 vector = Custom.DirVec(mainBody[0].pos, p);
			if (behavior != null)
			{
				vector = behavior.Direction;
			}
			for (int j = 0; j < mainBody.Length; j++)
			{
				float num4 = (float)j / ((float)mainBody.Length - 1f);
				mainBody[j].vel += vector * (Mathf.Lerp(1f, -1f, Mathf.InverseLerp(0f, 0.5f, num4)) * Mathf.InverseLerp(0.5f, 0f, num4) * 0.06f * sizeFac * num2);
				if (j < mainBody.Length - 1)
				{
					Vector2 vector2 = Custom.DirVec(mainBody[j + 1].pos, mainBody[j].pos);
					mainBody[j].vel += (vector2 + Custom.PerpendicularVector(vector2) * (Mathf.Sin(swimCycle - (float)j * 1.2f) * 0.8f * Mathf.Pow(num4, 0.3f))).normalized * (0.2f * sizeFac * num2);
				}
				if (num3 >= 0f && mainBody[j].vel.x > num3)
				{
					mainBody[j].vel.x *= 0.8f;
				}
				if (num3 >= 0f && mainBody[j].vel.y > num3)
				{
					mainBody[j].vel.y *= 0.8f;
				}
			}
			mainBody[0].vel += vector * (mainChunkFactor * sizeFac * Mathf.Pow(num2, 2f));
		}
		for (int k = 2; k < mainBody.Length; k++)
		{
			WeightedPush(k - 2, k, Custom.DirVec(mainBody[k].pos, mainBody[k - 2].pos), 0.1f * sizeFac);
		}
		if (!room.RoomRect.Vector2Inside(mainBody[0].pos) && !room.ViewedByAnyCamera(mainBody[0].pos, 200f))
		{
			if (canBeDestroyed)
			{
				Destroy();
			}
		}
		else
		{
			canBeDestroyed = true;
		}
		if (base.graphicsModule is VoidSpawnGraphics)
		{
			culled = !room.ViewedByAnyCamera(mainBody[0].pos, 300f) || !(base.graphicsModule as VoidSpawnGraphics).VisibleAtGlowDist(mainBody[0].pos, (base.graphicsModule as VoidSpawnGraphics).glowPos, 100f);
			if (!culled && lastCulled)
			{
				(base.graphicsModule as VoidSpawnGraphics).Reset();
			}
			lastCulled = culled;
		}
		playerProximityTime--;
		if (!(variant == SpawnType.ElderRipple) || soundLoop == null)
		{
			return;
		}
		soundLoop.sound = WatcherEnums.WatcherSoundID.Elder_Ripple_Aura;
		soundLoop.Volume = 0.7f;
		if (behavior is WatcherRoomSpecificScript.WORA_ElderSpawn.ElderBehavior { closeIn: not false } elderBehavior)
		{
			float num5 = Mathf.Lerp(elderBehavior.collapseAnimStart, elderBehavior.collapseAnimFinish, 0.5f);
			if (elderBehavior.orbitAngle > num5)
			{
				soundLoop.Volume = Custom.LerpMap(elderBehavior.orbitAngle, elderBehavior.closeInStart, num5, 0.7f, 1f);
			}
			else
			{
				soundLoop.Volume = Custom.LerpMap(elderBehavior.orbitAngle, num5, elderBehavior.collapseAnimFinish - 200f, 1f, 0f, 0.7f);
			}
		}
		soundLoop.Update();
	}
}
