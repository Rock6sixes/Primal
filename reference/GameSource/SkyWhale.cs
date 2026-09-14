using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using Smoke;
using UnityEngine;
using Watcher;

public class SkyWhale : Creature
{
	public class CreatureParams
	{
		public float totalMass;

		public float sizeMultiplier;

		public float huntSpeedFactor;

		public float travelSpeedFactor;

		public float fleeSpeedFactor;

		public float idleSpeedFactor;

		public float stunOnCollideVelocity;

		public float stunOnCollideDamage;

		public float stunOnCollideDuration;

		public float ruggedness;

		public float finSharpness;

		public CreatureParams()
		{
			totalMass = 100f;
			sizeMultiplier = 4f;
			huntSpeedFactor = 1f;
			travelSpeedFactor = 1f;
			fleeSpeedFactor = 1f;
			idleSpeedFactor = 1f;
			stunOnCollideVelocity = 0f;
			stunOnCollideDuration = 10f;
			stunOnCollideDamage = 0.1f;
			ruggedness = 0f;
		}
	}

	public class SkyWhaleWhisker : UpdatableAndDeletable, IClimbableVine
	{
		public SimpleSegment[] segments;

		public SkyWhale owner;

		public BodyChunk attachmentChunk;

		public float angle;

		public float side;

		public float connectionRad = 12f;

		public int forbiddenSegments = 3;

		public int timeSinceWhiskersBeingClimbed;

		public float startRadius = 1.5f;

		public float endRadius = 0.1f;

		public Vector2 GlobalAttachmentPos
		{
			get
			{
				Vector2 rotation = attachmentChunk.Rotation;
				Vector2 vector = new Vector2(0f - rotation.y, rotation.x);
				return attachmentChunk.pos + vector * ((attachmentChunk.rad - 5f) * Mathf.Sin(owner.roll * (float)Math.PI / 2f + side * (float)Math.PI / 2f));
			}
		}

		public float BackgroundFactor => Mathf.InverseLerp(-0.1f, -0.4f, Mathf.Cos(owner.roll * (float)Math.PI / 2f + side * (float)Math.PI / 2f));

		public bool InBackground => BackgroundFactor > 0f;

		public SkyWhaleWhisker(SkyWhale owner, int segments, BodyChunk attachmentChunk, float angle, float side)
		{
			this.owner = owner;
			this.segments = new SimpleSegment[segments];
			this.attachmentChunk = attachmentChunk;
			this.angle = angle;
			this.side = side;
			Reset();
		}

		public bool hasCreatureAttached()
		{
			return timeSinceWhiskersBeingClimbed <= 1;
		}

		public void Reset()
		{
			Vector2 globalAttachmentPos = GlobalAttachmentPos;
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].Reset(globalAttachmentPos);
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room != owner.room)
			{
				room?.climbableVines.vines.Remove(this);
				RemoveFromRoom();
				return;
			}
			timeSinceWhiskersBeingClimbed++;
			Vector2 rotation = attachmentChunk.Rotation;
			Vector2 vector = new Vector2(0f - rotation.y, rotation.x) * Mathf.Sin(owner.roll * (float)Math.PI / 2f + (float)Math.PI / 2f * side);
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].vel.y -= 0.9f * room.gravity;
				float num = (float)i / (float)(segments.Length - 1);
				segments[i].vel = Vector2.Lerp(segments[i].vel, attachmentChunk.vel, num * 0.1f);
				segments[i].vel += (vector - rotation * (1f + angle)) * num;
				segments[i].lastPos = segments[i].pos;
				segments[i].pos += segments[i].vel;
				segments[i].vel *= 0.95f;
				if (i > 0 && (room.aimap == null || room.aimap.getTerrainProximity(segments[i].pos) < 3))
				{
					SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.VerticalCollision(cd: new SharedPhysics.TerrainCollisionData(segments[i].pos, segments[i].lastPos, segments[i].vel, 2f, new IntVector2(0, 0), goThroughFloors: true), room: room);
					cd2 = SharedPhysics.HorizontalCollision(room, cd2);
					segments[i].pos = cd2.pos;
					segments[i].vel = cd2.vel;
					if (cd2.contactPoint.x != 0)
					{
						segments[i].vel.y *= 0.6f;
					}
					if (cd2.contactPoint.y != 0)
					{
						segments[i].vel.x *= 0.6f;
					}
				}
			}
			ConnectEnds();
			for (int num2 = segments.Length - 1; num2 > 0; num2--)
			{
				ConnectSegments(num2, num2 - 1);
			}
			ConnectEnds();
			for (int j = 1; j < segments.Length; j++)
			{
				ConnectSegments(j, j - 1);
			}
			ConnectEnds();
		}

		public void ConnectEnds()
		{
			segments[0].pos = GlobalAttachmentPos;
			segments[0].vel = attachmentChunk.vel;
		}

		public void ConnectSegments(int A, int B)
		{
			Vector2 normalized = (segments[A].pos - segments[B].pos).normalized;
			float num = Vector2.Distance(segments[A].pos, segments[B].pos);
			float num2 = Mathf.InverseLerp(0f, connectionRad, num);
			if (num > connectionRad)
			{
				Vector2 vector = normalized * ((connectionRad - num) * 0.5f * num2);
				segments[A].pos += vector;
				segments[A].vel += vector;
				segments[B].pos -= vector;
				segments[B].vel -= vector;
			}
		}

		public void BeingClimbedOn(Creature crit)
		{
			timeSinceWhiskersBeingClimbed = 0;
		}

		public bool CurrentlyClimbable()
		{
			return owner.abstractCreature.IsSameRippleLayer(room.game.ActiveRippleLayer);
		}

		public int TotalPositions()
		{
			return segments.Length - forbiddenSegments;
		}

		public float Rad(int index)
		{
			return 0.1f;
		}

		public float Mass(int index)
		{
			return 0.2f;
		}

		public Vector2 Pos(int index)
		{
			return segments[index + forbiddenSegments].pos;
		}

		public void Push(int index, Vector2 movement)
		{
			segments[index + forbiddenSegments].vel += movement;
		}
	}

	public class GasJet : UpdatableAndDeletable
	{
		public BodyChunk chunk;

		public Vector2 relPos;

		public Vector2 relDir;

		public NewVultureSmoke smoke;

		public int life;

		public int maxLife;

		public GasJet(BodyChunk chunk, Vector2 pos, Vector2 dir)
		{
			this.chunk = chunk;
			life = (maxLife = UnityEngine.Random.Range(80, 120));
			Vector2 rotation = chunk.Rotation;
			Vector2 rhs = Custom.PerpendicularVector(rotation);
			relPos = new Vector2(Vector2.Dot(pos - chunk.pos, rotation), Vector2.Dot(pos - chunk.pos, rhs));
			relDir = new Vector2(Vector2.Dot(dir, rotation), Vector2.Dot(dir, rhs));
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			PhysicalObject owner = chunk.owner;
			Vector2 rotation = chunk.Rotation;
			Vector2 vector = Custom.PerpendicularVector(rotation);
			Vector2 vector2 = relPos.x * rotation + relPos.y * vector + chunk.pos;
			Vector2 vector3 = relDir.x * rotation + relDir.y * vector;
			float num = (float)life / (float)maxLife;
			if (life == maxLife)
			{
				owner.room.PlaySound(SoundID.Vulture_Jet_Start, chunk);
			}
			if (owner.room.BeingViewed && life > 0)
			{
				if (!owner.room.PointSubmerged(vector2))
				{
					if (smoke == null)
					{
						smoke = new NewVultureSmoke(owner.room, vector2, null);
						owner.room.AddObject(smoke);
					}
					smoke.MoveTo(vector2, eu);
					smoke.EmitSmoke(vector3 * (40f * num), num);
				}
				else
				{
					owner.room.AddObject(new Bubble(vector2, vector3 * (30f * num), bottomBubble: false, fakeWaterBubble: false));
				}
			}
			if (smoke != null && (smoke.slatedForDeletetion || owner.room != smoke.room))
			{
				smoke = null;
			}
			life--;
			if (life == 0)
			{
				owner.room.PlaySound(SoundID.Vulture_Jet_Stop, chunk);
			}
			if (smoke.particles.Count == 0 && life < 0)
			{
				smoke.Destroy();
				Destroy();
			}
		}
	}

	public SkyWhaleAI AI;

	public CreatureParams creatureParams;

	public bool LeviathanWhale;

	public float flyingPower;

	public bool flapping;

	public bool flappingTowardsTarget;

	public int flapCounter;

	public int flapTowardsTargetCounter;

	public bool initiateFlap;

	public int initiatedFlapCounter;

	public float lastRoll;

	public float roll;

	public float rollOffset;

	public float targetRollOffset;

	public float rollOffsetVel;

	public Vector2 flyDir;

	public float turnDegreesPerTick = 2.125f;

	public int cryCooldownTime;

	public List<SkyWhaleWhisker> whiskers = new List<SkyWhaleWhisker>();

	public Vector2 destFlyDir;

	public int maxFlapTime = 100;

	public List<Vector2> lateVelocity = new List<Vector2>();

	public bool AltVariant => base.abstractCreature.superSizeMe;

	public void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		creatureParams = new CreatureParams();
		float value = UnityEngine.Random.value;
		creatureParams.sizeMultiplier = 3.75f + 0.5f * value;
		creatureParams.ruggedness = Mathf.Pow(Mathf.Lerp(value, UnityEngine.Random.value, UnityEngine.Random.value), 1.5f);
		creatureParams.finSharpness = UnityEngine.Random.value;
		if (AltVariant)
		{
			creatureParams.sizeMultiplier *= 1.5f;
			creatureParams.finSharpness *= 0.2f;
		}
		UnityEngine.Random.state = state;
	}

	public SkyWhale(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed + 1);
		if (!LeviathanWhale)
		{
			base.bodyChunks = new BodyChunk[6];
			base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), 8f * creatureParams.sizeMultiplier, creatureParams.totalMass / (float)base.bodyChunks.Length);
			base.bodyChunks[1] = new BodyChunk(this, 1, default(Vector2), 10f * creatureParams.sizeMultiplier, creatureParams.totalMass / (float)base.bodyChunks.Length);
			base.bodyChunks[2] = new BodyChunk(this, 2, default(Vector2), 10.5f * creatureParams.sizeMultiplier, creatureParams.totalMass / (float)base.bodyChunks.Length);
			base.bodyChunks[3] = new BodyChunk(this, 3, default(Vector2), 10f * creatureParams.sizeMultiplier, creatureParams.totalMass / (float)base.bodyChunks.Length);
			base.bodyChunks[4] = new BodyChunk(this, 4, default(Vector2), 9f * creatureParams.sizeMultiplier, creatureParams.totalMass / (float)base.bodyChunks.Length);
			base.bodyChunks[5] = new BodyChunk(this, 5, default(Vector2), 8f * creatureParams.sizeMultiplier, creatureParams.totalMass / (float)base.bodyChunks.Length);
			bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length * 2 - 3];
			for (int i = 0; i < base.bodyChunks.Length - 1; i++)
			{
				bodyChunkConnections[i] = new BodyChunkConnection(base.bodyChunks[i], base.bodyChunks[i + 1], 15f * creatureParams.sizeMultiplier, BodyChunkConnection.Type.Normal, 0.7f, -1f);
			}
			for (int j = 0; j < base.bodyChunks.Length - 2; j++)
			{
				bodyChunkConnections[base.bodyChunks.Length - 1 + j] = new BodyChunkConnection(base.bodyChunks[j], base.bodyChunks[j + 2], 23f * creatureParams.sizeMultiplier, BodyChunkConnection.Type.Push, 0.7f, -1f);
			}
			if (!AltVariant)
			{
				AddWhiskers();
			}
		}
		else
		{
			base.bodyChunks = new BodyChunk[10];
			for (int k = 0; k < base.bodyChunks.Length; k++)
			{
				base.bodyChunks[k] = new BodyChunk(this, k, new Vector2(0f + (float)k * 3f, 0f), (12f - 0.8f * (float)k) * creatureParams.sizeMultiplier * 5f, creatureParams.totalMass * 4f / (1f + (float)k));
			}
			bodyChunkConnections = new BodyChunkConnection[base.bodyChunks.Length - 1];
			for (int l = 0; l < bodyChunkConnections.Length; l++)
			{
				bodyChunkConnections[l] = new BodyChunkConnection(base.bodyChunks[l], base.bodyChunks[l + 1], 15f * creatureParams.sizeMultiplier * 7f, BodyChunkConnection.Type.Normal, 0.7f - 0.05f * (float)l, 0.1f * (float)bodyChunkConnections.Length - 0.04f * (float)l);
			}
		}
		for (int m = 0; m < base.bodyChunks.Length; m++)
		{
			base.bodyChunks[m].restrictInRoomRange = 2400f;
			base.bodyChunks[m].defaultRestrictInRoomRange = 2400f;
		}
		base.GoThroughFloors = true;
		base.airFriction = 0.9f;
		base.gravity = 0f;
		bounce = 0.1f;
		surfaceFriction = 0.85f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		base.mainBodyChunkIndex = 1;
		windAffectiveness = 0.25f;
		collisionRange = bodyChunkConnections.Sum((BodyChunkConnection connection) => connection.distance) + base.bodyChunks[0].rad + base.bodyChunks[base.bodyChunks.Length - 1].rad + 80f;
		UnityEngine.Random.state = state;
	}

	public void AddWhiskers()
	{
		for (int i = 0; i < 6; i++)
		{
			bool num = UnityEngine.Random.value < creatureParams.ruggedness * 0.15f;
			float side = (float)(i % 2) * 2f - 1f;
			float num2 = i / 2;
			float angle = num2 + UnityEngine.Random.Range(-0.3f, 0.3f) * creatureParams.ruggedness;
			int num3 = ((num2 == 1f) ? 17 : 14);
			num3 = Mathf.RoundToInt((float)num3 * (1f + UnityEngine.Random.Range(-0.3f, 0.3f) * creatureParams.ruggedness));
			if (num)
			{
				num3 /= 2;
			}
			SkyWhaleWhisker skyWhaleWhisker = new SkyWhaleWhisker(this, num3, base.bodyChunks[0], angle, side);
			whiskers.Add(skyWhaleWhisker);
			if (num)
			{
				skyWhaleWhisker.endRadius = UnityEngine.Random.Range(0.3f, 1f);
			}
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new SkyWhaleGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public bool WhiskerCheck()
	{
		foreach (SkyWhaleWhisker whisker in whiskers)
		{
			if (whisker.hasCreatureAttached())
			{
				return true;
			}
		}
		return false;
	}

	[Obsolete("no longer needed because fixed the main problem")]
	public void SaveTheWhales()
	{
		WorldCoordinate pos = base.abstractCreature.pos;
		pos.x = Mathf.Clamp(pos.x, 0, room.TileWidth - 1);
		pos.y = Mathf.Clamp(pos.y, 0, room.TileHeight - 1);
		bool flag = false;
		bool flag2 = false;
		BodyChunk[] array = base.bodyChunks;
		foreach (BodyChunk bodyChunk in array)
		{
			Room.Tile tile = room.GetTile(bodyChunk.pos);
			int terrainProximity = room.aimap.getTerrainProximity(tile.X, tile.Y);
			if (terrainProximity != -1 && (float)terrainProximity < bodyChunk.rad / 20f)
			{
				flag2 = true;
			}
		}
		if (!flag2)
		{
			for (int j = 0; j < room.abstractRoom.nodes.Length; j++)
			{
				WorldCoordinate worldCoordinate = room.LocalCoordinateOfNode(j);
				if (room.abstractRoom.nodes[j].type == AbstractRoomNode.Type.SideExit && AI.pathFinder.CoordinateReachable(worldCoordinate))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			return;
		}
		string[] array2 = new string[1];
		WorldCoordinate worldCoordinate2 = pos;
		array2[0] = "whale spawned in inescapable position: " + worldCoordinate2.ToString();
		Custom.LogImportant(array2);
		SkyWhaleAbstractAI skyWhaleAbstractAI = base.abstractCreature.abstractAI as SkyWhaleAbstractAI;
		int num = room.abstractRoom.RandomRelevantNodeAccountingForEntranceWidth(base.Template, 4);
		if (num != -1)
		{
			WorldCoordinate worldCoordinate3 = new WorldCoordinate(room.abstractRoom.index, -1, -1, num);
			while (!skyWhaleAbstractAI.CanUseSideExit(worldCoordinate3) && worldCoordinate3.abstractNode > -1 && room.abstractRoom.GetNode(worldCoordinate3).type == AbstractRoomNode.Type.SideExit)
			{
				worldCoordinate3.abstractNode--;
			}
			if (!skyWhaleAbstractAI.CanUseSideExit(worldCoordinate3))
			{
				worldCoordinate3.abstractNode = num + 1;
			}
			while (!skyWhaleAbstractAI.CanUseSideExit(worldCoordinate3) && worldCoordinate3.abstractNode < room.abstractRoom.nodes.Length && room.abstractRoom.GetNode(worldCoordinate3).type == AbstractRoomNode.Type.SideExit)
			{
				worldCoordinate3.abstractNode++;
			}
			if (skyWhaleAbstractAI.CanUseSideExit(worldCoordinate3) && -1 < worldCoordinate3.abstractNode && worldCoordinate3.abstractNode < room.abstractRoom.nodes.Length && room.abstractRoom.GetNode(worldCoordinate3).type == AbstractRoomNode.Type.SideExit)
			{
				WorldCoordinate start = new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0);
				room.game.shortcuts.CreatureTakeFlight(this, AbstractRoomNode.Type.SideExit, start, worldCoordinate3);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		float num = ((AI.behavior == SkyWhaleAI.Behavior.GetUnstuck) ? 0.5f : 1f);
		if (AltVariant)
		{
			num /= 1.5f;
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].terrainSqueeze = num;
		}
		lastRoll = roll;
		rollOffset = Mathf.SmoothDamp(rollOffset, targetRollOffset, ref rollOffsetVel, 0.5f, 0.5f, 0.025f);
		if (UnityEngine.Random.value < 0.035f)
		{
			if (UnityEngine.Random.value < 0.3f)
			{
				targetRollOffset = 0f;
			}
			else
			{
				targetRollOffset = UnityEngine.Random.value * 0.75f + ((targetRollOffset > 0.5f) ? 0f : 0.25f);
			}
		}
		float to = Mathf.Lerp((base.bodyChunks[1].pos - base.bodyChunks[2].pos).normalized.x, 0f, rollOffset * 0.35f);
		roll = Custom.LerpAndTick(roll, to, 0.1f, 0.05f);
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		base.airFriction = (base.Consious ? 0.9f : 0.98f);
		if (cryCooldownTime > 0)
		{
			cryCooldownTime--;
		}
		else
		{
			bool flag = false;
			foreach (SkyWhaleWhisker whisker in whiskers)
			{
				if (whisker.timeSinceWhiskersBeingClimbed < 5)
				{
					flag = true;
					break;
				}
			}
			if (base.safariControlled)
			{
				ref Player.InputPackage? reference = ref inputWithDiagonals;
				if (reference.HasValue && reference.GetValueOrDefault().jmp)
				{
					cryCooldownTime = UnityEngine.Random.Range(150, 200);
					room.PlaySound(WatcherEnums.WatcherSoundID.Sky_Whale_Cry, base.mainBodyChunk, loop: false, 2f, 1f);
				}
			}
			else if (flag)
			{
				if (UnityEngine.Random.value < 0.0025f)
				{
					cryCooldownTime = UnityEngine.Random.Range(150, 300);
					room.PlaySound(WatcherEnums.WatcherSoundID.Sky_Whale_Cry, base.mainBodyChunk);
				}
			}
			else if (UnityEngine.Random.value < 0.00125f)
			{
				cryCooldownTime = UnityEngine.Random.Range(200, 600);
				room.PlaySound(WatcherEnums.WatcherSoundID.Sky_Whale_Click, base.mainBodyChunk);
			}
		}
		if (base.Consious)
		{
			Act();
		}
		else
		{
			flyDir = (base.bodyChunks[0].pos - base.bodyChunks[1].pos).normalized;
		}
	}

	public void Act()
	{
		AI.Update();
		float num = creatureParams.idleSpeedFactor;
		if (AI.behavior == SkyWhaleAI.Behavior.EscapeRain)
		{
			num = creatureParams.fleeSpeedFactor;
		}
		float useFlyingPower = flyingPower * num;
		Vector2 pos = base.firstChunk.pos;
		MovementConnection movementConnection = (AI.pathFinder as SkyWhalePather).FollowPath(room.GetWorldCoordinate(pos), actuallyFollowingThisPath: true);
		if (movementConnection == default(MovementConnection) && room != null)
		{
			for (int i = 1; i < 7; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					movementConnection = (AI.pathFinder as SkyWhalePather).FollowPath(room.GetWorldCoordinate(pos + Custom.eightDirections[j].ToVector2() * (20 * i)), actuallyFollowingThisPath: true);
					if (movementConnection != default(MovementConnection))
					{
						break;
					}
				}
				if (movementConnection != default(MovementConnection))
				{
					break;
				}
			}
		}
		if (room == null)
		{
			return;
		}
		if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
		{
			movementConnection = default(MovementConnection);
		}
		flyingPower = Mathf.Lerp(flyingPower, 1f, 0.1f);
		Vector2 vector = Vector2.zero;
		if (movementConnection != default(MovementConnection))
		{
			if (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				enteringShortCut = movementConnection.StartTile;
				if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
				{
					NPCTransportationDestination = movementConnection.destinationCoord;
				}
				return;
			}
			Vector2 p = room.MiddleOfTile(movementConnection.DestTile);
			MovementConnection movementConnection2 = movementConnection;
			if (AI.pathFinder.GetDestination != AI.pathFinder.GetEffectualDestination && AI.behavior == SkyWhaleAI.Behavior.FollowPath && AI.CurrentNode != null)
			{
				p = AI.CurrentNode.ClosestPoint(base.firstChunk.pos);
			}
			else if (!base.safariControlled)
			{
				int num2 = 1;
				for (int k = 0; k < 8; k++)
				{
					movementConnection2 = (AI.pathFinder as SkyWhalePather).FollowPath(movementConnection2.destinationCoord, actuallyFollowingThisPath: false);
					if (movementConnection2 == default(MovementConnection) || !room.VisualContact(room.GetTilePosition(pos), movementConnection2.DestTile))
					{
						break;
					}
					p += room.MiddleOfTile(movementConnection2.destinationCoord);
					num2++;
				}
				p /= (float)num2;
			}
			vector = Custom.DirVec(base.firstChunk.pos, p);
			if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && movementConnection.startCoord.x <= 0)
			{
				vector = new Vector2(-1f, 0f);
			}
			else if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && movementConnection.startCoord.x >= room.TileWidth - 1)
			{
				vector = new Vector2(1f, 0f);
			}
			if (AI.behavior == SkyWhaleAI.Behavior.FollowPath && !base.safariControlled)
			{
				if ((AI.pathFinder as SkyWhalePather).RayTraceInAccessibleTiles(room.GetTilePosition(base.firstChunk.pos), room.GetTilePosition(AI.currentNodePosition)))
				{
					vector = Vector3.Slerp(vector, Custom.DirVec(base.firstChunk.pos, p), 0.4f);
				}
				if (!room.IsPositionInsideBoundries(room.GetTilePosition(base.firstChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.firstChunk.pos) * 100f)) && !room.IsPositionInsideBoundries(room.GetTilePosition(AI.currentNodePosition)))
				{
					p = AI.currentNodePosition;
					vector = Custom.DirVec(base.firstChunk.pos, p);
				}
			}
		}
		else if (base.safariControlled)
		{
			vector = flyDir;
			Player.InputPackage? inputPackage = inputWithDiagonals;
			if (inputPackage.HasValue)
			{
				Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
				if (valueOrDefault.analogueDir.magnitude > 0f)
				{
					vector = valueOrDefault.analogueDir.normalized;
				}
				else if (valueOrDefault.x != 0 || valueOrDefault.y != 0)
				{
					vector = new Vector2(valueOrDefault.x, valueOrDefault.y).normalized;
				}
			}
		}
		if (vector != Vector2.zero)
		{
			flyDir = Vector3.RotateTowards(flyDir, vector, turnDegreesPerTick * ((float)Math.PI / 180f) * ((AI.behavior == SkyWhaleAI.Behavior.FollowPath) ? 0.5f : 1f), 1f);
			destFlyDir = vector;
			flapTowardsTargetCounter++;
			float angle = Vector2.Angle(Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), destFlyDir);
			if (AI.behavior == SkyWhaleAI.Behavior.GetUnstuck)
			{
				base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, room.MiddleOfTile(movementConnection.DestTile)) * 2f;
				base.bodyChunks[0].pos += Custom.DirVec(base.bodyChunks[0].pos, room.MiddleOfTile(movementConnection.DestTile)) * 2f;
				base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, room.MiddleOfTile(movementConnection.DestTile)) * 1.2f;
			}
			else
			{
				FlyTowardsTarget(useFlyingPower, flapCounter <= 50, flapCounter, angle);
			}
		}
		else
		{
			Vector2 vector2 = Custom.DirVec(base.firstChunk.pos, base.bodyChunks[1].pos);
			base.bodyChunks[0].vel += vector2 * 3f;
			base.bodyChunks[0].pos += vector2 * 3f;
			base.bodyChunks[1].vel += vector2 * 2f;
		}
		if (flapCounter >= 160)
		{
			flapping = false;
			flapCounter = 0;
		}
	}

	public void AccessSideGate(WorldCoordinate start, WorldCoordinate dest)
	{
		Debug.Log("SkyWhale Access Side Gate");
		if ((base.abstractCreature.abstractAI as SkyWhaleAbstractAI).checkRooms.Count > 0)
		{
			(base.abstractCreature.abstractAI as SkyWhaleAbstractAI).checkRooms.RemoveAt(0);
		}
		room.game.shortcuts.CreatureTakeFlight(this, AbstractRoomNode.Type.SideExit, start, dest);
	}

	public void FlyTowardsTarget(float useFlyingPower, bool ascend, int flapCounter, float angle)
	{
		base.bodyChunks[0].vel += flyDir * (Mathf.Lerp(1f, -1f, Mathf.Pow(0f, 0.5f)) * 0.4f * useFlyingPower * (float)((!LeviathanWhale) ? 1 : 8));
		Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
		base.bodyChunks[0].vel += vector * (0.5f * Mathf.Lerp(0.5f, 1f, useFlyingPower) * 3.25f * Mathf.Lerp(1f, 0.75f, angle / 30f) * (float)((!LeviathanWhale) ? 1 : 8));
		float num = Mathf.Clamp(Vector2.SignedAngle(vector, flyDir) / 90f, -1.5f, 1.5f);
		base.bodyChunks[0].vel += Custom.PerpendicularVector(vector) * num;
		base.bodyChunks[1].vel -= Custom.PerpendicularVector(vector) * num;
	}

	public void FlapTail(bool ascend, float angle)
	{
	}

	public void SafariControls(ref MovementConnection followingConnection)
	{
		if (!(followingConnection == default(MovementConnection)) && AllowableControlledAIOverride(followingConnection.type))
		{
			return;
		}
		followingConnection = default(MovementConnection);
		if (inputWithDiagonals.HasValue)
		{
			MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
			if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				type = MovementConnection.MovementType.ShortCut;
			}
			if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
			{
				followingConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
			}
		}
	}

	public bool Climbable(IntVector2 tile)
	{
		if (base.safariControlled)
		{
			if (!inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp || room.aimap.getTerrainProximity(tile) != 1)
			{
				return room.aimap.getAItile(tile).acc == AItile.Accessibility.Climb;
			}
			return true;
		}
		if (room.aimap.getTerrainProximity(tile) != 1)
		{
			return room.aimap.getAItile(tile).acc == AItile.Accessibility.Climb;
		}
		return true;
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (base.Consious && creatureParams.stunOnCollideVelocity > 0f && Vector2.Distance(base.bodyChunks[myChunk].vel, otherObject.bodyChunks[otherChunk].vel) >= creatureParams.stunOnCollideVelocity && otherObject is Creature)
		{
			(otherObject as Creature).Violence(base.bodyChunks[myChunk], base.bodyChunks[myChunk].vel * base.bodyChunks[myChunk].mass, otherObject.bodyChunks[otherChunk], null, DamageType.Blunt, creatureParams.stunOnCollideDamage, creatureParams.stunOnCollideDuration);
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		if (speed > 1.5f && firstContact)
		{
			Mathf.InverseLerp(6f, 14f, speed);
		}
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (RippleViolenceCheck(source))
		{
			if (hitChunk != null && source != null && type == DamageType.Stab)
			{
				Vector2 normalized = (source.pos - hitChunk.pos).normalized;
				Vector2 pos = Vector2.ClampMagnitude(source.pos - hitChunk.pos, hitChunk.rad * 0.95f) + hitChunk.pos;
				room.AddObject(new GasJet(hitChunk, pos, normalized.normalized));
			}
			room.PlaySound(WatcherEnums.WatcherSoundID.Sky_Whale_Groan, base.mainBodyChunk);
			base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * ((-1.5f + (float)i) * 15f);
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 2f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		if (whiskers.Count > 0 && room.climbableVines == null)
		{
			room.climbableVines = new ClimbableVinesSystem();
			room.AddObject(room.climbableVines);
		}
		foreach (SkyWhaleWhisker whisker in whiskers)
		{
			newRoom.AddObject(whisker);
			newRoom.climbableVines.vines.Add(whisker);
			whisker.Reset();
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		Vector2? vector = null;
		if (base.abstractCreature.pos.NodeDefined && placeRoom.abstractRoom.nodes[base.abstractCreature.pos.abstractNode].type == AbstractRoomNode.Type.SideExit && (base.abstractCreature.pos.x < 0 || base.abstractCreature.pos.x > placeRoom.TileWidth))
		{
			int num = base.abstractCreature.pos.abstractNode - placeRoom.exitAndDenIndex.Length;
			RoomBorderExit roomBorderExit = placeRoom.borderExits[num];
			List<IntVector2> list = new List<IntVector2>();
			IntVector2[] borderTiles = roomBorderExit.borderTiles;
			foreach (IntVector2 intVector in borderTiles)
			{
				if (placeRoom.aimap.TileAccessibleToCreature(intVector, base.Template))
				{
					list.Add(intVector);
				}
			}
			if (list.Count > 0)
			{
				base.abstractCreature.pos.y = list[UnityEngine.Random.Range(0, list.Count)].y;
			}
			vector = Custom.DirVec(placeRoom.MiddleOfTile(base.abstractCreature.pos), new Vector2((float)roomBorderExit.borderTiles[0].x * 20f, base.abstractCreature.pos.y));
		}
		if (!vector.HasValue && base.abstractCreature.abstractAI is SkyWhaleAbstractAI { NodePath: { } nodePath })
		{
			Vector2 vector2 = placeRoom.MiddleOfTile(base.abstractCreature.pos);
			Vector2? vector3 = null;
			float num2 = 0f;
			if (nodePath.PathfindingNodes.Count > 0)
			{
				Vector2 vector4 = Custom.ClosestPointOnLineSegment(nodePath.CurrentNode.owner.pos, nodePath.CurrentNode.owner.pos + nodePath.CurrentNode.handlePos, vector2);
				vector3 = (vector4 - vector2).normalized;
				num2 = Vector2.Distance(vector4, vector2);
			}
			Vector2? vector5 = null;
			float num3 = 0f;
			if (nodePath.usedPathfindingNodes.Count > 0)
			{
				Vector2 vector6 = nodePath.usedPathfindingNodes[nodePath.usedPathfindingNodes.Count - 1].ClosestPoint(vector2);
				vector5 = (vector6 - vector2).normalized;
				num3 = Vector2.Distance(vector6, vector2);
			}
			if (vector3.HasValue && vector5.HasValue)
			{
				float num4 = num2 + num3;
				vector = Custom.DegToVec(Mathf.LerpAngle(vector5.Value.GetAngle(), vector3.Value.GetAngle(), num2 / num4));
			}
			else if (vector3.HasValue)
			{
				vector = vector3.Value;
			}
			else if (vector5.HasValue)
			{
				vector = vector5.Value;
			}
		}
		base.PlaceInRoom(placeRoom);
		if (vector.HasValue)
		{
			Vector2 valueOrDefault = vector.GetValueOrDefault();
			destFlyDir = (flyDir = valueOrDefault);
			BodyChunkConnection[] array = bodyChunkConnections;
			foreach (BodyChunkConnection bodyChunkConnection in array)
			{
				bodyChunkConnection.chunk2.HardSetPosition(bodyChunkConnection.chunk1.pos + -valueOrDefault);
			}
			FlyTowardsTarget(1f, ascend: true, 0, 0f);
		}
	}
}
