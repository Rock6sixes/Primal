using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class Rattler : Creature
{
	public class MovedCreature
	{
		public const int minRegrabDelay = 80;

		public int regrabTimer;

		public AbstractCreature creature;

		public MovedCreature(AbstractCreature creature)
		{
			this.creature = creature;
			regrabTimer = 80;
		}
	}

	public class Arm
	{
		public class Task : ExtEnum<Task>
		{
			public static Task None = new Task("None");

			public static Task Forage = new Task("Forage");

			public static Task InvestigateSound = new Task("InvestigateSound");

			public static Task MoveCreature = new Task("MoveCreature");

			public Task(string value, bool register = true)
				: base(value, register)
			{
			}
		}

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public Vector2 basePos;

		public bool limp;

		public float pullIntoShell;

		public Rattler owner;

		public int grasp;

		public Vector2 targetPos;

		public IntVector2 targetTile;

		public List<IntVector2> idealPath = new List<IntVector2>();

		public Dictionary<IntVector2, int> idealPathIndices = new Dictionary<IntVector2, int>();

		public int currentPathIndex = -1;

		public Rope rope;

		public float targetRopeLength;

		public bool collideWithTerrain = true;

		public int taskTimer;

		public Vector2 taskPos;

		public Vector2 taskDir;

		public int timeSincePathAdvanced;

		public Tracker.CreatureRepresentation moveCreature;

		public bool grabbedCreature;

		public float extendSpeed = 2f;

		public float retractSpeed = 2f;

		public float reelInSpeed = 2f;

		public float tipRad = 3f;

		public Task CurrentTask { get; set; } = Task.None;


		public Vector2? TargetClawDir
		{
			get
			{
				if (CurrentTask != Task.Forage && CurrentTask != Task.InvestigateSound)
				{
					return null;
				}
				return -taskDir;
			}
		}

		public Vector2 TargetPos => targetPos;

		public Arm(Rattler owner, int grasp)
		{
			this.owner = owner;
			this.grasp = grasp;
		}

		public void Update()
		{
			lastPos = pos;
			if (rope != null)
			{
				DoTask();
				Move();
				ApplyVelocity();
			}
		}

		public void Retract()
		{
			SwitchTask(Task.None);
		}

		public bool Forage()
		{
			for (int i = 0; i < 100; i++)
			{
				IntVector2 tilePosition = owner.room.GetTilePosition(UnityEngine.Random.insideUnitCircle * (owner.armLength * 0.5f) + owner.room.MiddleOfTile(owner.armPather.center));
				if (owner.room.GetTile(tilePosition).Terrain != 0 || !CanReach(tilePosition) || owner.ClosestArmDistance(owner.room.MiddleOfTile(tilePosition)) < 40f || Vector2.Distance(owner.room.MiddleOfTile(tilePosition), owner.ArmBasePos) < 40f)
				{
					continue;
				}
				int num = UnityEngine.Random.Range(0, 4);
				for (int j = 0; j < 4; j++)
				{
					IntVector2 intVector = Custom.fourDirections[(j + num) % 4];
					if (owner.room.GetTile(tilePosition + intVector).Solid)
					{
						SwitchTask(Task.Forage);
						taskDir = -intVector.ToVector2();
						taskPos = owner.room.MiddleOfTile(tilePosition) + intVector.ToVector2() * 10f;
						taskTimer = UnityEngine.Random.Range(160, 400);
						SetTarget(taskPos + taskDir);
						return true;
					}
				}
			}
			return false;
		}

		public bool MoveCreature(Tracker.CreatureRepresentation creature)
		{
			IntVector2 intVector = default(IntVector2);
			float num = float.NegativeInfinity;
			for (int i = 0; i < 100; i++)
			{
				IntVector2 tilePosition = owner.room.GetTilePosition(UnityEngine.Random.insideUnitCircle * (owner.armLength * 0.9f) + owner.room.MiddleOfTile(owner.armPather.center));
				float num2 = DropCreatureScore(tilePosition);
				if (num2 > num)
				{
					intVector = tilePosition;
					num = num2;
				}
			}
			if (num > float.NegativeInfinity || (owner.safariControlled && owner.SafariArm == this))
			{
				SwitchTask(Task.MoveCreature);
				moveCreature = creature;
				taskPos = owner.room.MiddleOfTile(intVector);
				return true;
			}
			return false;
		}

		public float DropCreatureScore(IntVector2 tile)
		{
			if (!CanReach(tile))
			{
				return float.NegativeInfinity;
			}
			Vector2 b = owner.room.MiddleOfTile(tile);
			bool num = owner.room.VisualContact(owner.bodyChunks[0].pos, b);
			float num2 = Mathf.Min(owner.armPather.GetDistance(tile), owner.armLength / 20f * 0.75f);
			if (!num)
			{
				num2 += 4f;
			}
			if (owner.armPather.GetDistance(tile) > 3f)
			{
				num2 += FallRiskFactor(tile) * 10f;
			}
			if (num && Mathf.Abs(b.x - owner.attachmentPoint.x) < 40f)
			{
				num2 /= 4f;
			}
			if (owner.room.aimap != null)
			{
				int terrainProximity = owner.room.aimap.getTerrainProximity(tile);
				if (terrainProximity >= 3 && terrainProximity <= 7)
				{
					num2 *= 2f;
				}
			}
			return num2;
		}

		public float FallRiskFactor(IntVector2 tile)
		{
			if (owner.room.aimap == null)
			{
				return 0f;
			}
			if (owner.room.aimap.getAItile(tile).fallRiskTile.y < 0)
			{
				return 1f;
			}
			if (tile.y - owner.room.aimap.getAItile(tile).fallRiskTile.y <= 30)
			{
				return 0f;
			}
			return 1f;
		}

		public bool InvestigateSound(Vector2 pos)
		{
			Vector2 vector = default(Vector2);
			Vector2 vector2 = default(Vector2);
			float num = float.PositiveInfinity;
			for (int i = 0; i < 16; i++)
			{
				float f = (float)i / 16f * (float)Math.PI * 2f + UnityEngine.Random.Range(0.5f, 0.5f);
				Vector2 b = pos + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * 100f;
				FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(owner.room, pos, b);
				if (floatRect.HasValue && Vector2.Distance(pos, floatRect.Value.GetCorner(FloatRect.CornerLabel.D)) < num && CanReach(owner.room.GetTilePosition(floatRect.Value.GetCorner(FloatRect.CornerLabel.D) - floatRect.Value.GetCorner(FloatRect.CornerLabel.B))))
				{
					vector = floatRect.Value.GetCorner(FloatRect.CornerLabel.D);
					vector2 = -floatRect.Value.GetCorner(FloatRect.CornerLabel.B);
					num = Vector2.Distance(pos, vector);
				}
			}
			IntVector2 tilePosition = owner.room.GetTilePosition(vector + vector2);
			if (num < float.PositiveInfinity)
			{
				SwitchTask(Task.InvestigateSound);
				taskPos = vector;
				taskDir = vector2;
				taskTimer = UnityEngine.Random.Range(160, 400) + (int)(owner.armPather.GetDistance(tilePosition) / 4f * 40f);
				SetTarget(taskPos + taskDir);
				return true;
			}
			return false;
		}

		public void SwitchTask(Task newTask)
		{
			if (CurrentTask == Task.MoveCreature)
			{
				owner.ReleaseGrasp(grasp);
			}
			CurrentTask = newTask;
			taskTimer = 0;
			taskPos = default(Vector2);
			taskDir = default(Vector2);
			moveCreature = null;
			grabbedCreature = false;
		}

		public void DoTask()
		{
			collideWithTerrain = true;
			if (owner.safariControlled && owner.SafariArm == this)
			{
				return;
			}
			if (CurrentTask == Task.None)
			{
				SetTarget(owner.room.MiddleOfTile(owner.armPather.center));
				if (!limp && pullIntoShell == 0f)
				{
					taskTimer++;
					if (taskTimer > 240)
					{
						collideWithTerrain = false;
					}
					if (taskTimer > 480)
					{
						taskTimer = 0;
						Reset(owner.ArmBasePos);
					}
				}
			}
			else if (CurrentTask == Task.Forage)
			{
				if (UnityEngine.Random.value < 0.05f && Custom.DistLess(pos, targetPos, tipRad + 2f))
				{
					Vector2 target = taskPos + taskDir + Custom.PerpendicularVector(taskDir) * UnityEngine.Random.Range(-15f, 15f);
					if (CanReach(owner.room.GetTilePosition(target)))
					{
						SetTarget(target);
					}
				}
				taskTimer--;
				if (taskTimer <= 0)
				{
					Retract();
				}
			}
			else if (CurrentTask == Task.InvestigateSound)
			{
				if (UnityEngine.Random.value < 0.05f && Custom.DistLess(pos, targetPos, tipRad + 2f))
				{
					Vector2 target2 = taskPos + taskDir * UnityEngine.Random.Range(1f, 20f) + Custom.PerpendicularVector(taskDir) * UnityEngine.Random.Range(-15f, 15f);
					if (CanReach(owner.room.GetTilePosition(target2)))
					{
						SetTarget(target2);
					}
				}
				taskTimer--;
				if (taskTimer <= 0)
				{
					Retract();
				}
			}
			else
			{
				if (!(CurrentTask == Task.MoveCreature))
				{
					return;
				}
				taskTimer++;
				Creature realizedCreature = moveCreature.representedCreature.realizedCreature;
				if (realizedCreature == null || (taskTimer > 160 && owner.grasps[grasp] == null) || !CanReach(owner.room.GetTilePosition(realizedCreature.mainBodyChunk.pos)) || (grabbedCreature && owner.grasps[grasp]?.grabbed != realizedCreature))
				{
					Retract();
					return;
				}
				if (owner.grasps[grasp] == null)
				{
					SetTarget(realizedCreature.mainBodyChunk.pos);
					if (Custom.DistLess(pos, realizedCreature.mainBodyChunk.pos, Mathf.Max(realizedCreature.mainBodyChunk.rad, 5f)))
					{
						AttemptGrabCreature(realizedCreature, realizedCreature.mainBodyChunkIndex);
					}
					return;
				}
				SetTarget(taskPos);
				if (rope.totalLength > owner.armLength || taskTimer > 280 || Custom.DistLess(pos, targetPos, 10f))
				{
					realizedCreature.Stun(30);
					Retract();
				}
			}
		}

		public void AttemptGrabCreature(Creature crit, int chunkGrabbed)
		{
			bool num = owner.Grab(crit, grasp, chunkGrabbed, Grasp.Shareability.CanNotShare, 0.4f, overrideEquallyDominant: false, pacifying: true);
			taskTimer = 0;
			if (num)
			{
				grabbedCreature = true;
				crit.Stun(30);
			}
			else
			{
				Retract();
			}
		}

		public void Carry(BodyChunk chunk)
		{
			float num = chunk.rad * 0.5f;
			float num2 = 0.5f;
			if (!Custom.DistLess(chunk.pos, pos, num))
			{
				Vector2 vector = Custom.DirVec(chunk.pos, pos);
				float num3 = Vector2.Distance(chunk.pos, pos);
				float num4 = chunk.mass / (num2 + chunk.mass);
				if (owner.safariControlled && owner.SafariArm == this)
				{
					num4 = 0f;
				}
				Vector2 vector2 = vector * ((num - num3) * (1f - num4));
				Vector2 vector3 = vector * ((num - num3) * num4);
				chunk.pos -= vector2;
				chunk.vel -= vector2;
				pos += vector3;
				vel += vector3;
				if (num3 > num + 30f)
				{
					Retract();
				}
			}
		}

		public void Move()
		{
			Room room = owner.room;
			IntVector2 tilePosition = room.GetTilePosition(pos);
			Vector2 vector = Custom.PerpendicularVector(owner.BodyDir);
			basePos = owner.ArmBasePos + owner.BodyDir * (30f * (1f - pullIntoShell));
			basePos += vector * (Mathf.Clamp(Vector2.Dot(vector, rope.GetPosition(rope.TotalPositions - 2) - owner.ArmBasePos) / 4f, -30f, 30f) * (1f - pullIntoShell));
			if (limp)
			{
				vel *= 0.975f;
				vel.y -= owner.gravity;
				if (owner.Consious)
				{
					targetRopeLength -= reelInSpeed;
				}
				if (targetRopeLength < rope.totalLength)
				{
					Vector2 vector2 = Custom.DirVec(rope.GetPosition(0), rope.GetPosition(1));
					vel += vector2 * (rope.totalLength - targetRopeLength) / 10f;
					PullOnRope(rope.totalLength - targetRopeLength);
				}
				return;
			}
			pullIntoShell = Custom.LerpAndTick(pullIntoShell, (idealPath.Count == 1 && Custom.DistLess(pos, owner.ArmBasePos, 30f)) ? 1f : 0f, 0.05f, 0.01f);
			if (pullIntoShell == 1f)
			{
				pos = owner.ArmBasePos;
				if (rope.TotalPositions > 2)
				{
					Reset(owner.ArmBasePos);
				}
			}
			Vector2 aimFor;
			float num;
			if (owner.safariControlled && owner.SafariArm == this)
			{
				float t = 0f;
				Vector2 position = rope.GetPosition(1);
				if (Vector2.Dot(position - pos, targetPos - pos) <= 0f)
				{
					t = Mathf.InverseLerp(owner.armLength - 30f, owner.armLength, rope?.totalLength ?? 0f);
				}
				Vector2 b = position + (targetPos - position).normalized * (position - pos).magnitude;
				aimFor = Vector2.Lerp(targetPos, b, t);
				num = ((!(Vector2.Dot(pos - owner.ArmBasePos, aimFor - owner.ArmBasePos) > 0f)) ? retractSpeed : extendSpeed);
				if (idealPathIndices.TryGetValue(tilePosition, out var value))
				{
					currentPathIndex = value;
				}
			}
			else if (currentPathIndex == -1)
			{
				aimFor = room.MiddleOfTile(owner.armPather.GetPreviousTile(tilePosition));
				num = retractSpeed;
				if (idealPathIndices.TryGetValue(tilePosition, out var value2))
				{
					currentPathIndex = value2;
				}
			}
			else
			{
				if (Custom.DistLess(pos, room.MiddleOfTile(idealPath[currentPathIndex]), 5f) || Custom.DistLess(pos, targetPos, 5f))
				{
					timeSincePathAdvanced = 0;
					if (currentPathIndex < idealPath.Count - 1)
					{
						currentPathIndex++;
					}
				}
				timeSincePathAdvanced++;
				aimFor = ((currentPathIndex != idealPath.Count - 1 || !room.VisualContact(pos, targetPos)) ? room.MiddleOfTile(idealPath[currentPathIndex]) : targetPos);
				num = extendSpeed;
				DoAnimation(ref aimFor);
			}
			vel *= 0.7f;
			vel += Vector2.ClampMagnitude((aimFor - pos) / 5f, 1f) * num;
			if ((float)timeSincePathAdvanced > 80f)
			{
				currentPathIndex = -1;
			}
			else if ((float)timeSincePathAdvanced > 40f)
			{
				float num2 = Mathf.InverseLerp(40f, 80f, timeSincePathAdvanced);
				vel *= Mathf.Lerp(1f, 0.5f, num2);
				pos = Vector2.Lerp(pos, aimFor, num2 * 0.2f);
			}
			PullOnRope(targetRopeLength - rope.totalLength);
			targetRopeLength = rope.totalLength;
		}

		public void DoAnimation(ref Vector2 aimFor)
		{
			if (pullIntoShell > 0f)
			{
				aimFor = Vector2.Lerp(aimFor, owner.ArmBasePos, pullIntoShell);
			}
			else if (CurrentTask == Task.MoveCreature && owner.room.VisualContact(pos, targetPos))
			{
				aimFor = targetPos;
			}
		}

		public void ApplyVelocity()
		{
			Vector2 zero = Vector2.zero;
			if (rope.TotalPositions <= 2 && rope.totalLength < owner.armLength / 2f)
			{
				zero += owner.bodyChunks[0].vel * 0.5f;
			}
			vel += zero;
			pos += vel;
			if (collideWithTerrain)
			{
				SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.VerticalCollision(cd: new SharedPhysics.TerrainCollisionData(pos, lastPos, vel, tipRad, new IntVector2(0, 0), goThroughFloors: true), room: owner.room);
				cd2 = SharedPhysics.HorizontalCollision(owner.room, cd2);
				pos = cd2.pos;
				vel = cd2.vel;
			}
			vel -= zero;
			rope.Update(pos, basePos);
		}

		public void PullOnRope(float lengthDecrease)
		{
			Vector2 vector = Custom.DirVec(rope.GetPosition(rope.TotalPositions - 1), rope.GetPosition(rope.TotalPositions - 2));
			owner.bodyChunks[0].vel += vector * (lengthDecrease * ((lengthDecrease > 0f && !limp) ? 0.03f : 0.01f) * Mathf.InverseLerp(5f, 25f, Vector2.Distance(pos, owner.ArmBasePos)));
		}

		public bool CanReach(IntVector2 pos)
		{
			return owner.armPather.GetDistance(pos) < owner.armLength / 20f;
		}

		public void SetTarget(Vector2 target)
		{
			IntVector2 intVector = targetTile;
			targetPos = target;
			targetTile = owner.room.GetTilePosition(target);
			if (!CanReach(targetTile) && !owner.safariControlled)
			{
				targetTile = owner.armPather.center;
				targetPos = owner.room.MiddleOfTile(targetTile);
			}
			if (targetTile != intVector)
			{
				timeSincePathAdvanced = 0;
				idealPath.Clear();
				owner.armPather.GetPath(idealPath, targetTile);
				if (!CanContinuePath(idealPath, currentPathIndex, idealPathIndices))
				{
					currentPathIndex = -1;
				}
				idealPathIndices.Clear();
				for (int i = 0; i < idealPath.Count; i++)
				{
					idealPathIndices.Add(idealPath[i], i);
				}
			}
		}

		public bool CanContinuePath(List<IntVector2> newPath, int currentIndex, Dictionary<IntVector2, int> oldIndices)
		{
			if (currentIndex >= newPath.Count)
			{
				return false;
			}
			for (int i = 0; i < currentIndex; i++)
			{
				if (!oldIndices.TryGetValue(newPath[i], out var value) || i != value)
				{
					return false;
				}
			}
			return true;
		}

		public void Reset(Vector2 pos)
		{
			SwitchTask(Task.None);
			this.pos = pos;
			lastPos = pos;
			basePos = pos;
			idealPath.Clear();
			idealPathIndices.Clear();
			targetPos = pos;
			targetTile = owner.room.GetTilePosition(pos);
			timeSincePathAdvanced = 0;
			currentPathIndex = -1;
			if (rope != null)
			{
				rope.Reset(pos);
			}
			else
			{
				rope = new Rope(owner.room, pos, pos, 1f);
			}
		}

		public bool IsMovingCreature(Tracker.CreatureRepresentation rep)
		{
			if (CurrentTask == Task.MoveCreature)
			{
				return moveCreature.representedCreature == rep.representedCreature;
			}
			return false;
		}
	}

	public class ArmPather
	{
		public struct PathNode
		{
			public IntVector2 previous;

			public float distance;

			public PathNode(IntVector2 previous, float distance)
			{
				this.previous = previous;
				this.distance = distance;
			}

			public override string ToString()
			{
				return $"{previous} ({distance})";
			}
		}

		public IntVector2 center;

		public IntRect bounds;

		public PathNode[] nodes;

		public ArmPather(Room room, IntVector2 center, float maxLength)
		{
			this.center = center;
			int num = Mathf.FloorToInt(maxLength);
			bounds = new IntRect(center.x - num, center.y - num, center.x + num, center.y + num);
			nodes = Generate(GetAllowedTiles(room, bounds), bounds, center);
		}

		public float GetDistance(IntVector2 coord)
		{
			if (!bounds.Includes(coord))
			{
				return float.PositiveInfinity;
			}
			return nodes[GetIndex(coord, bounds)].distance;
		}

		public IntVector2 GetPreviousTile(IntVector2 coord)
		{
			if (!bounds.Includes(coord))
			{
				return center;
			}
			return nodes[GetIndex(coord, bounds)].previous;
		}

		public void GetPath(List<IntVector2> path, IntVector2 dest)
		{
			path.Clear();
			IntVector2 intVector = dest;
			while (intVector != center)
			{
				path.Add(intVector);
				intVector = GetPreviousTile(intVector);
			}
			path.Add(center);
			path.Reverse();
		}

		public bool[] GetAllowedTiles(Room room, IntRect bounds)
		{
			int num = bounds.right - bounds.left + 1;
			int num2 = bounds.top - bounds.bottom + 1;
			bool[] array = new bool[num * num2];
			int num3 = 0;
			for (int i = bounds.bottom; i <= bounds.top; i++)
			{
				for (int j = bounds.left; j <= bounds.right; j++)
				{
					array[num3++] = !room.HasAnySolid(j, i);
				}
			}
			return array;
		}

		public PathNode[] Generate(bool[] tiles, IntRect bounds, IntVector2 source)
		{
			PathNode[] array = new PathNode[tiles.Length];
			LinkedList<PathNode> linkedList = new LinkedList<PathNode>();
			int num = 0;
			for (int i = bounds.bottom; i <= bounds.top; i++)
			{
				for (int j = bounds.left; j <= bounds.right; j++)
				{
					array[num++] = new PathNode(source, float.PositiveInfinity);
				}
			}
			linkedList.AddLast(new PathNode(source, 0f));
			array[GetIndex(source, bounds)].distance = 0f;
			while (linkedList.Count > 0)
			{
				PathNode value = linkedList.First.Value;
				linkedList.RemoveFirst();
				IntVector2[] eightDirections = Custom.eightDirections;
				for (int k = 0; k < eightDirections.Length; k++)
				{
					IntVector2 intVector = eightDirections[k];
					IntVector2 intVector2 = value.previous + intVector;
					float num2 = intVector.ToVector2().magnitude + value.distance;
					if (bounds.Includes(intVector2))
					{
						int index = GetIndex(intVector2, bounds);
						if (tiles[index] && num2 < array[index].distance)
						{
							array[index].distance = num2;
							array[index].previous = value.previous;
							AddSorted(linkedList, new PathNode(intVector2, num2));
						}
					}
				}
			}
			return array;
		}

		public static int GetIndex(IntVector2 tile, IntRect bounds)
		{
			return tile.x - bounds.left + (tile.y - bounds.bottom) * (bounds.right - bounds.left + 1);
		}

		public static void AddSorted(LinkedList<PathNode> list, PathNode newNode)
		{
			for (LinkedListNode<PathNode> linkedListNode = list.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				if (linkedListNode.Value.distance > newNode.distance)
				{
					list.AddBefore(linkedListNode, newNode);
					break;
				}
			}
			list.AddLast(newNode);
		}
	}

	public RattlerAI ai;

	public Arm[] arms;

	public Vector2 attachmentPoint;

	public Vector2 attachmentDir;

	public float rattleIntensity;

	public List<MovedCreature> movedCreatures = new List<MovedCreature>();

	public float armLength = 280f;

	public ArmPather armPather;

	public ChunkDynamicSoundLoop soundLoop;

	public int safariArmIndex;

	public InputCircularRegion controlRegion;

	public Vector2 ArmBasePos => base.bodyChunks[0].pos;

	public Vector2 BodyDir => Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);

	public Arm SafariArm
	{
		get
		{
			if (!SafariRattleInput())
			{
				return arms[safariArmIndex];
			}
			return null;
		}
	}

	public Rattler(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed);
		float num = 1.6f;
		base.bodyChunks = new BodyChunk[3];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 17f, num * 0.5f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 12f, num * 0.4f);
		base.bodyChunks[2] = new BodyChunk(this, 1, new Vector2(0f, 0f), 5f, num * 0.1f)
		{
			collideWithObjects = false,
			collideWithTerrain = false
		};
		bodyChunkConnections = new BodyChunkConnection[2];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 20f * Custom.ClampedRandomVariation(0.5f, 0.1f, 0.3f), BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 36f * Custom.ClampedRandomVariation(0.5f, 0.1f, 0.3f), BodyChunkConnection.Type.Normal, 1f, -1f);
		base.airFriction = 0.85f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		base.CollideWithSlopes = false;
		abstractCreature.tentacleImmune = true;
		windAffectiveness = 0.25f;
		soundLoop = new ChunkDynamicSoundLoop(base.bodyChunks[0]);
		arms = new Arm[base.grasps.Length];
		for (int i = 0; i < arms.Length; i++)
		{
			arms[i] = new Arm(this, i);
		}
		UnityEngine.Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (base.safariControlled)
		{
			SafariControls();
		}
		if (base.Consious)
		{
			ai.Update();
		}
		if (base.Consious)
		{
			rattleIntensity = Custom.LerpAndTick(rattleIntensity, ai.targetRattleIntensity, 0.125f, 0.025f);
		}
		else
		{
			rattleIntensity = Mathf.Max(rattleIntensity - 0.1f, 0f);
		}
		base.bodyChunks[2].pos = attachmentPoint;
		base.bodyChunks[2].vel = Vector2.zero;
		Vector2 vector = attachmentDir * (base.Consious ? 1f : 0.2f);
		base.bodyChunks[0].vel += vector;
		base.bodyChunks[1].vel += vector * 0.5f;
		if (base.Consious)
		{
			BodyChunk[] array = base.bodyChunks;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].vel.y += base.gravity * 0.9f;
			}
		}
		for (int j = 0; j < arms.Length; j++)
		{
			arms[j].limp = !base.Consious;
			arms[j].Update();
			if (!base.Consious)
			{
				arms[j].Retract();
			}
			if (base.grasps[j] != null)
			{
				if (base.grasps[j].grabbed is Creature creature)
				{
					MarkAsRecentlyGrabbed(creature.abstractCreature);
				}
				arms[j].Carry(base.grasps[j].grabbed.bodyChunks[base.grasps[j].chunkGrabbed]);
			}
		}
		if (rattleIntensity > 0f)
		{
			soundLoop.sound = WatcherEnums.WatcherSoundID.Rattler_Shake_LOOP;
			soundLoop.Volume = rattleIntensity;
			soundLoop.Pitch = 1f + Mathf.Pow(rattleIntensity, 2f) * 0.1f;
		}
		else
		{
			soundLoop.sound = SoundID.None;
			if (UnityEngine.Random.value < 0.0016666667f && base.Consious)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Rattler_Squeal, base.mainBodyChunk, loop: false, UnityEngine.Random.Range(0.4f, 0.8f), UnityEngine.Random.Range(0.6f, 1.4f));
			}
		}
		soundLoop.Update();
		for (int num = movedCreatures.Count - 1; num >= 0; num--)
		{
			movedCreatures[num].regrabTimer--;
			if (movedCreatures[num].regrabTimer <= 0)
			{
				movedCreatures.RemoveAt(num);
			}
		}
	}

	public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
	{
		bool num = base.Grab(obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
		if (num)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Rattler_Tentacle_Grab, obj.bodyChunks[chunkGrabbed]);
			if (obj is IDrawable obj2)
			{
				GraphicsModule obj3 = base.graphicsModule;
				if (obj3 != null)
				{
					obj3.AddObjectToInternalContainer(obj2, graspUsed);
					return num;
				}
				return num;
			}
			if (obj.graphicsModule != null)
			{
				GraphicsModule obj4 = base.graphicsModule;
				if (obj4 == null)
				{
					return num;
				}
				obj4.AddObjectToInternalContainer(obj.graphicsModule, graspUsed);
			}
		}
		return num;
	}

	public override void ReleaseGrasp(int grasp)
	{
		PhysicalObject physicalObject = base.grasps[grasp]?.grabbed;
		if (physicalObject != null)
		{
			base.graphicsModule?.ReleaseSpecificInternallyContainedObjectSprites(physicalObject);
		}
		base.ReleaseGrasp(grasp);
	}

	public void MarkAsRecentlyGrabbed(AbstractCreature creature)
	{
		foreach (MovedCreature movedCreature in movedCreatures)
		{
			if (movedCreature.creature == creature)
			{
				movedCreature.regrabTimer = 80;
				return;
			}
		}
		movedCreatures.Add(new MovedCreature(creature));
	}

	public bool RecentlyGrabbedCreature(AbstractCreature creature)
	{
		foreach (MovedCreature movedCreature in movedCreatures)
		{
			if (movedCreature.creature == creature)
			{
				return true;
			}
		}
		return false;
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
	{
		if (chunk == base.bodyChunks[0] && Vector2.Angle(direction, (base.bodyChunks[1].pos - base.bodyChunks[0].pos).normalized) < 40f)
		{
			return true;
		}
		return false;
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (directionAndMomentum.HasValue && source != null && source.owner is Weapon source2 && !SpearStick(source2, damage, hitChunk, hitAppendage, directionAndMomentum.Value))
		{
			Vector2 pos = ((source != null) ? Vector2.Lerp(hitChunk.pos, source.pos, 0.75f) : hitChunk.pos);
			if (damage > 0.1f || stunBonus > 30f)
			{
				room.AddObject(new StationaryEffect(pos, new Color(1f, 1f, 1f), null, StationaryEffect.EffectType.FlashingOrb));
				for (int i = 0; i < 3 + (int)Mathf.Min(damage * 3f, 9f); i++)
				{
					room.AddObject(new Spark(pos, Custom.RNV() * (UnityEngine.Random.value * 12f), new Color(1f, 1f, 1f), null, 6, 16));
				}
			}
			damage = 0f;
		}
		if (damage > 0f && !base.dead)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Rattler_Squeal, base.mainBodyChunk);
		}
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (otherObject is Creature crit)
		{
			ai.CollidedWithCreature(crit);
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		Vector2 vector = room.MiddleOfTile(base.abstractCreature.pos);
		attachmentPoint = placeRoom.FindGroundBelow(vector, out attachmentDir);
		if (Vector2.Distance(vector, attachmentPoint) > 40f)
		{
			Vector2 normal;
			Vector2? vector2 = FindClosestSurface(placeRoom, vector, 100f, out normal);
			if (vector2.HasValue && Vector2.Distance(vector2.Value, vector) < Vector2.Distance(attachmentPoint, vector))
			{
				attachmentPoint = vector2.Value;
				attachmentDir = normal;
			}
		}
		Arm[] array = arms;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset(ArmBasePos);
		}
		IntVector2 tilePosition = room.GetTilePosition(attachmentPoint + attachmentDir * 55f);
		armPather = new ArmPather(placeRoom, tilePosition, armLength / 20f);
		controlRegion = new InputCircularRegion(400f, 0f, 360f, 8f, Vector2.zero);
	}

	public Vector2? FindClosestSurface(Room room, Vector2 pos, float maxDist, out Vector2 normal)
	{
		Vector2 value = default(Vector2);
		Vector2 vector = default(Vector2);
		float num = float.PositiveInfinity;
		for (int i = 0; i < 32; i++)
		{
			float f = (float)i / 32f * (float)Math.PI * 2f;
			FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(room, pos, pos + maxDist * new Vector2(Mathf.Cos(f), Mathf.Sin(f)));
			if (floatRect.HasValue)
			{
				Vector2 vector2 = new Vector2(floatRect.Value.left, floatRect.Value.bottom);
				Vector2 vector3 = -new Vector2(floatRect.Value.right, floatRect.Value.top);
				float num2 = Vector2.Distance(pos, vector2);
				if (num2 < num)
				{
					value = vector2;
					vector = vector3;
					num = num2;
				}
			}
		}
		normal = vector;
		if (num <= maxDist)
		{
			return value;
		}
		return null;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			GraphicsModule graphicsModule2 = (base.graphicsModule = new RattlerGraphics(this));
		}
	}

	public float ClosestArmDistance(Vector2 pos)
	{
		float num = float.PositiveInfinity;
		Arm[] array = arms;
		foreach (Arm arm in array)
		{
			if (arm.pullIntoShell < 1f)
			{
				num = Mathf.Min(num, Vector2.Distance(arm.TargetPos, pos));
				num = Mathf.Min(num, Vector2.Distance(arm.pos, pos));
			}
		}
		return num;
	}

	public void SafariControls()
	{
		controlRegion.drawDebugPos = ArmBasePos;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (SafariArm == null)
		{
			controlRegion.pos = Vector2.zero;
		}
		else
		{
			Player.InputPackage? inputPackage = inputWithDiagonals;
			if (inputPackage.HasValue)
			{
				Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
				if (valueOrDefault.jmp)
				{
					ref Player.InputPackage? reference = ref lastInputWithDiagonals;
					if (reference.HasValue && !reference.GetValueOrDefault().jmp)
					{
						SafariArm.InvestigateSound(SafariArm.pos);
						safariArmIndex++;
						safariArmIndex %= arms.Length;
						controlRegion.pos = SafariArm.pos - ArmBasePos;
					}
				}
				if (valueOrDefault.pckp)
				{
					flag2 = true;
					ref Player.InputPackage? reference2 = ref lastInputWithDiagonals;
					if (reference2.HasValue && !reference2.GetValueOrDefault().pckp)
					{
						flag = true;
					}
				}
				flag3 = valueOrDefault.AnyDirectionalInput;
			}
			controlRegion.Update(inputWithDiagonals);
			Vector2 vector = SafariArm.pos - ArmBasePos;
			float num = Custom.Dist(vector, controlRegion.pos);
			float t = Mathf.InverseLerp(armLength - 30f, armLength, SafariArm.rope?.totalLength ?? 0f);
			float num2 = Mathf.Lerp(20f, 60f, t);
			if (num > num2 || !flag3)
			{
				controlRegion.pos = Vector2.Lerp(controlRegion.pos, vector, Mathf.Lerp(0.05f, Mathf.Lerp(0.2f, 0.6f, t), (num - num2) / 200f));
			}
			SafariArm.SetTarget(ArmBasePos + controlRegion.pos);
			if (SafariArm.CurrentTask == Arm.Task.MoveCreature)
			{
				if (!flag2)
				{
					SafariArm.SwitchTask(Arm.Task.InvestigateSound);
				}
			}
			else
			{
				SafariArm.SwitchTask(Arm.Task.InvestigateSound);
			}
		}
		if (!flag)
		{
			return;
		}
		Arm[] array = arms;
		foreach (Arm arm in array)
		{
			if (arm.CurrentTask != Arm.Task.InvestigateSound)
			{
				continue;
			}
			foreach (AbstractCreature creature in room.abstractRoom.creatures)
			{
				Creature realizedCreature = creature.realizedCreature;
				if (realizedCreature == null || realizedCreature is Rattler)
				{
					continue;
				}
				bool flag4 = false;
				for (int j = 0; j < realizedCreature.bodyChunks.Length; j++)
				{
					if (Custom.DistLess(arm.pos, realizedCreature.bodyChunks[j].pos, realizedCreature.bodyChunks[j].rad + 5f))
					{
						arm.SwitchTask(Arm.Task.MoveCreature);
						arm.MoveCreature(ai.CreateTrackerRepresentationForCreature(creature));
						arm.AttemptGrabCreature(realizedCreature, j);
						flag4 = true;
						break;
					}
				}
				if (flag4)
				{
					break;
				}
			}
		}
	}

	public bool SafariRattleInput()
	{
		ref Player.InputPackage? reference = ref inputWithDiagonals;
		if (reference.HasValue && reference.GetValueOrDefault().thrw)
		{
			return ai.CountArms(Arm.Task.MoveCreature) < 1;
		}
		return false;
	}

	public static bool ValidSpawnPos(Room room, IntVector2 pos, List<Vector2> rattlerSpawnLocsSoFar)
	{
		Vector2 b = room.MiddleOfTile(pos);
		ShortcutData[] shortcuts = room.shortcuts;
		for (int i = 0; i < shortcuts.Length; i++)
		{
			ShortcutData shortcutData = shortcuts[i];
			if (shortcutData.shortCutType != ShortcutData.Type.DeadEnd && Vector2.Distance(room.MiddleOfTile(shortcutData.startCoord), b) < 160f)
			{
				return false;
			}
		}
		foreach (WarpPoint warpPoint in room.warpPoints)
		{
			if (Vector2.Distance(warpPoint.pos, b) < warpPoint.PullRadius * 1.5f)
			{
				return false;
			}
		}
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if ((placedObject.type == PlacedObject.Type.DataPearl || placedObject.type == PlacedObject.Type.UniqueDataPearl || placedObject.type == PlacedObject.Type.RippleSpawnEgg || placedObject.type == PlacedObject.Type.KarmaFlower) && Vector2.Distance(placedObject.pos, b) < 40f)
			{
				return false;
			}
			if (placedObject.type == PlacedObject.Type.RippleTree && Mathf.Abs(b.x - placedObject.pos.x) < 100f)
			{
				return false;
			}
		}
		for (int j = 0; j < rattlerSpawnLocsSoFar.Count; j++)
		{
			if (Vector2.Distance(rattlerSpawnLocsSoFar[j], b) < 160f)
			{
				return false;
			}
		}
		for (int k = -2; k <= 2; k++)
		{
			for (int l = -1; l <= 3; l++)
			{
				if (!room.GetTile(pos + new IntVector2(k, l)).Solid && room.aimap.getAItile(pos + new IntVector2(k, l)).narrowSpace)
				{
					return false;
				}
			}
		}
		for (int m = -1; m <= 1; m++)
		{
			for (int n = 1; n <= 5; n++)
			{
				if (room.HasAnySolid(pos + new IntVector2(m, n)))
				{
					return false;
				}
			}
		}
		return true;
	}
}
