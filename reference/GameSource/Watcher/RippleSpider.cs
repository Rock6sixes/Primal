using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleSpider : Creature
{
	public class RippleSpiderSpawner : UpdatableAndDeletable, INotifyWhenRoomUnloaded
	{
		public CosmeticRipple ripple;

		public List<VoidSpawn> summonedRippleSpawn;

		public RippleSpider spider;

		public Vector2 pos;

		public int spawnCooldown = 1;

		public int tearSpawnTime = 110;

		public int timer;

		public bool spawned;

		public StaticSoundLoop tearSoundLoop;

		public float lastSpawnAngle;

		public int maxSpawn => 25;

		public int timeUntilTearSpawn => tearSpawnTime - timer;

		public int spawnDuration => 60;

		public RippleSpiderSpawner(Room room, Vector2 pos)
		{
			this.pos = pos;
			lastSpawnAngle = Random.Range(0f, 360f);
			summonedRippleSpawn = new List<VoidSpawn>();
		}

		public void SoundUpdate()
		{
			if (timer == 0)
			{
				room.PlaySound(SoundID.Generic_Charge, pos, 1f, 0.9f);
			}
			if (tearSoundLoop == null)
			{
				if (spawned)
				{
					return;
				}
				tearSoundLoop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Player_Generating_Warp_Point_LOOP, pos, room, 0f, 0.1f)
				{
					fadeOutOnDestroyFrames = 80
				};
			}
			tearSoundLoop.Update();
			tearSoundLoop.pitch = 1f;
			if (ripple == null && !spawned)
			{
				tearSoundLoop.volume = Mathf.InverseLerp(0f, tearSpawnTime, timer);
				tearSoundLoop.pitch = Custom.LerpMap(Mathf.InverseLerp(0f, tearSpawnTime, timer), 1f, 0f, 1.25f, 0.1f);
			}
			else if (ripple != null)
			{
				tearSoundLoop.volume = Mathf.InverseLerp(tearSpawnTime + 275, tearSpawnTime + 75, timer);
				tearSoundLoop.pitch = Custom.LerpMap(timer, tearSpawnTime + 275, tearSpawnTime + 75, 0.1f, 1.25f);
			}
			else
			{
				tearSoundLoop = null;
			}
			if (tearSpawnTime + 30 == timer)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Warp_Point_Close, pos, 1f, 0.9f);
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			SoundUpdate();
			timer++;
			if (spawnCooldown <= 0 && timeUntilTearSpawn > 20)
			{
				if (!LookForSpawnToInvite())
				{
					SpawnNewSpawn();
				}
				spawnCooldown = spawnDuration / maxSpawn;
			}
			else if (summonedRippleSpawn.Count < maxSpawn)
			{
				spawnCooldown--;
			}
			for (int num = summonedRippleSpawn.Count - 1; num >= 0; num--)
			{
				if (summonedRippleSpawn[num].slatedForDeletetion)
				{
					summonedRippleSpawn.RemoveAt(num);
				}
				else
				{
					UpdateSpawn(summonedRippleSpawn[num]);
				}
			}
			if (timeUntilTearSpawn == 0)
			{
				SpawnRippleTear();
			}
			if (ripple != null)
			{
				if (ripple.displayIntensity > 0.5f && !spawned)
				{
					SpawnSpider(ripple.PointToTear(pos + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)) * Random.Range(0f, ripple.Data.scale)));
					spawned = true;
				}
				else if (ripple.displayIntensity > 0.75f)
				{
					ripple.Data.animateOnSpawn = true;
					ripple.Data.animationSpeed = -0.005f;
				}
				else if (ripple.Data.animationSpeed == -0.005f && ripple.displayIntensity <= 0f)
				{
					ripple.Destroy();
					ripple = null;
					Destroy();
				}
			}
		}

		public bool LookForSpawnToInvite()
		{
			foreach (UpdatableAndDeletable update in room.updateList)
			{
				if (update is VoidSpawn spawn && InviteNewSpawn(spawn))
				{
					return true;
				}
			}
			return false;
		}

		public bool InviteNewSpawn(VoidSpawn spawn)
		{
			if (Vector2.Distance(spawn.firstChunk.pos, pos) > 500f)
			{
				return false;
			}
			if (summonedRippleSpawn.Contains(spawn))
			{
				return false;
			}
			if (spawn.variant != VoidSpawn.SpawnType.RippleSpawn)
			{
				return false;
			}
			if (spawn.behavior is VoidSpawn.FollowMigrationStream || spawn.behavior is VoidSpawn.MillAroundMigrationStream)
			{
				return false;
			}
			if (spawn.startFadeOut || spawn.slatedForDeletetion)
			{
				return false;
			}
			float num = timer / spawnDuration;
			float orbitDistance = Mathf.Lerp(150f, 400f, Mathf.Pow(Random.value, 2f)) + 200f * num;
			spawn.timeUntilFadeout = -1;
			spawn.behavior = new VoidSpawn.CollapseOrbit(spawn, pos, orbitDistance, clockwise: false, timeUntilTearSpawn + 50)
			{
				flipThreshold = Mathf.Lerp(3f, 5f, num)
			};
			summonedRippleSpawn.Add(spawn);
			return true;
		}

		public VoidSpawn SpawnNewSpawn()
		{
			float num = timer / spawnDuration;
			float num2 = Mathf.Lerp(150f, 400f, Mathf.Pow(Random.value, 2f)) + 200f * num;
			float num3 = (lastSpawnAngle += Random.Range(45f, 135f) + 45f * num);
			Vector2 vector = pos + Custom.DegToVec(num3) * (num2 + 50f);
			VoidSpawn voidSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, WatcherEnums.AbstractObjectType.RippleSpawn, null, room.GetWorldCoordinate(vector), room.world.game.GetNewID()), 0f, dayLightMode: false, VoidSpawn.SpawnType.RippleSpawn);
			voidSpawn.swimSpeed *= Mathf.Lerp(1f, 3f, num);
			voidSpawn.firstChunk.vel = Custom.DegToVec(num3 + 90f) * voidSpawn.swimSpeed;
			voidSpawn.behavior = new VoidSpawn.CollapseOrbit(voidSpawn, pos, num2, clockwise: false, timeUntilTearSpawn + 30)
			{
				flipThreshold = Mathf.Lerp(3f, 5f, num)
			};
			voidSpawn.PlaceInRoom(room);
			summonedRippleSpawn.Add(voidSpawn);
			return voidSpawn;
		}

		public void UpdateSpawn(VoidSpawn spawn)
		{
			if (spawn.behavior is VoidSpawn.CollapseOrbit collapseOrbit)
			{
				collapseOrbit.UpdateOrbit();
				if (spawn.timeUntilFadeout == 0 && ripple != null)
				{
					spawn.SetTimeUntilFadeOut(75);
				}
			}
		}

		public void SpawnRippleTear()
		{
			float maxInclusive = 100f;
			int cycleExpiry = 1;
			PlacedObject placedObject = new PlacedObject(PlacedObject.Type.CosmeticRipple, null);
			(placedObject.data as CosmeticRippleData).animateOnSpawn = true;
			(placedObject.data as CosmeticRippleData).animationSpeed = 0.01f;
			(placedObject.data as CosmeticRippleData).panelPos = Vector2.zero;
			float num = Random.Range(100f, maxInclusive);
			(placedObject.data as CosmeticRippleData).handlePos = new Vector2(0f, 1f) * num;
			(placedObject.data as CosmeticRippleData).scale = num;
			(placedObject.data as CosmeticRippleData).intensity = 1f;
			(placedObject.data as CosmeticRippleData).invert = false;
			(placedObject.data as CosmeticRippleData).square = false;
			(placedObject.data as CosmeticRippleData).leavesTrail = false;
			(placedObject.data as CosmeticRippleData).fallOff = Random.value;
			(placedObject.data as CosmeticRippleData).depthMix = 0.5f;
			(placedObject.data as CosmeticRippleData).squish = 0.36f;
			(placedObject.data as CosmeticRippleData).pos = pos;
			(placedObject.data as CosmeticRippleData).cycleExpiry = cycleExpiry;
			(placedObject.data as CosmeticRippleData).isGameplay = true;
			if (room.world.game.IsStorySession)
			{
				(placedObject.data as CosmeticRippleData).cycleSpawnedOn = room.world.game.GetStorySession.saveState.cycleNumber;
			}
			placedObject.pos = (placedObject.data as CosmeticRippleData).pos;
			room.world.game.cameras[0].UpdateRippleData(room, room.world.game.cameras[0].currentCameraPosition);
			ripple = room.TrySpawnRippleRegion(placedObject, saveInRegionState: false, skipIfInRegionState: false);
		}

		public void SpawnSpider(Vector2 spawnPos)
		{
			CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.RippleSpider);
			AbstractCreature abstractCreature = new AbstractCreature(room.world, creatureTemplate, null, room.GetWorldCoordinate(spawnPos), room.game.GetNewID());
			room.abstractRoom.AddEntity(abstractCreature);
			abstractCreature.RealizeInRoom();
			room.world.regionState.rippleSpiderRespawnRoom = room.abstractRoom.name;
			room.world.regionState.rippleSpiderTargetRoom = null;
			room.world.regionState.rippleSpiderSpawnedThisCycle = true;
			if (room.game.Players.Count <= 0 || !(room.game.Players[0].realizedCreature is Player player) || player.room != room)
			{
				(abstractCreature.abstractAI as RippleSpiderAbstractAI).waitForPlayer = true;
			}
		}

		public void RoomUnloaded()
		{
			if (!spawned)
			{
				SpawnSpider(pos);
			}
			spawned = true;
		}
	}

	public class CreatureParams
	{
		public float totalMass;

		public float sizeMultiplier;

		public float travelSpeedFactor;

		public float idleSpeedFactor;

		public float stunOnCollideVelocity;

		public float stunOnCollideDamage;

		public float stunOnCollideDuration;

		public CreatureParams()
		{
			totalMass = 0.4f;
			sizeMultiplier = 1f;
			travelSpeedFactor = 1f;
			idleSpeedFactor = 1f;
			stunOnCollideVelocity = 0f;
			stunOnCollideDuration = 10f;
			stunOnCollideDamage = 0.1f;
		}
	}

	public RippleSpiderAI AI;

	public CreatureParams creatureParams;

	public CosmicWeb _weaverThread;

	public WebChain _webChain;

	public float runCycle;

	public float walkSide;

	public Vector2 travelDir;

	public const int despawnDuration = 120;

	public int despawnTimer;

	public int footingCounter;

	public int specialMoveCounter;

	public IntVector2 specialMoveDestination;

	public MovementConnection lastFollowedConnection;

	public Vector2 lastPlacedThread = Vector2.zero;

	public bool firstThread = true;

	public bool initWarp;

	public bool Footing => footingCounter > 20;

	public Vector2 GroundNormal
	{
		get
		{
			IntVector2 intVector = default(IntVector2);
			int num = 0;
			IntVector2[] eightDirections = Custom.eightDirections;
			for (int i = 0; i < eightDirections.Length; i++)
			{
				IntVector2 intVector2 = eightDirections[i];
				if (IsTileSolid(0, intVector2.x, intVector2.y))
				{
					intVector -= intVector2;
					num++;
				}
			}
			if (num == 0)
			{
				return Vector2.zero;
			}
			return intVector.ToVector2() / num;
		}
	}

	public Vector2 ThreadAttachPos
	{
		get
		{
			Vector2 pos = base.bodyChunks[0].pos;
			Vector2 pos2 = base.bodyChunks[1].pos;
			Vector2 normalized = (pos - pos2).normalized;
			return pos2 - Custom.RotateAroundOrigo(normalized, walkSide * 45f) * (base.bodyChunks[1].rad * 2f);
		}
	}

	public float DespawnFade => Mathf.Clamp01((float)despawnTimer / 120f);

	public override bool SandstormImmune => true;

	public void GenerateIVars()
	{
		Random.State state = Random.state;
		Random.InitState(base.abstractCreature.ID.RandomSeed);
		creatureParams = new CreatureParams();
		Random.state = state;
	}

	public RippleSpider(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5.5f * creatureParams.sizeMultiplier, creatureParams.totalMass / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 6f * creatureParams.sizeMultiplier, creatureParams.totalMass / 2f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 12f * creatureParams.sizeMultiplier, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.3f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = base.airFriction;
		waterRetardationImmunity = 1f;
		base.buoyancy = 0f;
		base.CollideWithObjects = false;
		canBeHitByWeapons = false;
		ChangeRippleLayer(1);
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new RippleSpiderGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public void WeaverThreadEnteringShortcut()
	{
		if (enteringShortCut.HasValue && _weaverThread != null)
		{
			_weaverThread.stuckPosA = ThreadAttachPos;
			_weaverThread.originalStuckAtAPos = ThreadAttachPos;
			_webChain.cosmicWebs.Add(_weaverThread);
			_weaverThread = null;
			_webChain = null;
		}
	}

	public void MakeWeaverThread()
	{
		if (_webChain != null)
		{
			if (_weaverThread != null)
			{
				_weaverThread.stuckPosA = ThreadAttachPos;
				_weaverThread.originalStuckAtAPos = ThreadAttachPos;
				_webChain.cosmicWebs.Add(_weaverThread);
			}
			_weaverThread = new CosmicWeb(room, 225f, base.bodyChunks[1].pos, base.bodyChunks[1].pos, stuckAtA: true, stuckAtB: true);
			lastPlacedThread = base.bodyChunks[1].pos;
			firstThread = false;
			_weaverThread.graphic.startAnimation = 1f;
			_weaverThread.conRad = 16f;
			_weaverThread.bothSides = true;
			room.AddObject(_weaverThread);
		}
	}

	public void MakeCosmicWeb()
	{
		_webChain = new WebChain();
		room.AddObject(_webChain);
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		if (AI.WaitForPlayer)
		{
			float num = -1f;
			IntVector2 intVector = default(IntVector2);
			CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat);
			for (int i = 1; i < placeRoom.TileHeight; i++)
			{
				for (int j = 0; j < placeRoom.TileWidth; j++)
				{
					if (!placeRoom.HasAnySolid(j, i) && (placeRoom.HasAnySolid(j, i - 1) || placeRoom.GetTile(j, i - 1).Terrain == Room.Tile.TerrainType.Floor) && placeRoom.aimap.TileAccessibleToCreature(j, i, creatureTemplate))
					{
						float num2 = placeRoom.aimap.Visibility(new IntVector2(j, i));
						if (num2 > num)
						{
							num = num2;
							intVector = new IntVector2(j, i);
						}
					}
				}
			}
			if (intVector != default(IntVector2))
			{
				WorldCoordinate pos = base.coord;
				pos.Tile = intVector;
				base.abstractCreature.pos = pos;
			}
		}
		base.PlaceInRoom(placeRoom);
		foreach (UpdatableAndDeletable update in room.updateList)
		{
			if (update is WebChain webChain && webChain.cosmicWebs.Count > 0 && Custom.DistLess(webChain.cosmicWebs[webChain.cosmicWebs.Count - 1].originalStuckAtAPos, base.mainBodyChunk.pos, 40f))
			{
				_webChain = webChain;
				break;
			}
		}
		if (_webChain == null)
		{
			MakeCosmicWeb();
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (base.dead || AI.reachedTarget)
		{
			despawnTimer++;
			if (despawnTimer > 120)
			{
				base.abstractCreature.Destroy();
				Destroy();
			}
		}
		Vector2 rhs = Custom.PerpendicularVector(base.bodyChunks[1].pos - base.bodyChunks[0].pos);
		if (IsTileSolid(0, 0, -1))
		{
			walkSide = 0f - rhs.y;
		}
		else if (GroundNormal != Vector2.zero)
		{
			walkSide = 0f - Vector2.Dot(GroundNormal, rhs);
		}
		else
		{
			walkSide *= 0.98f;
		}
		if (_weaverThread != null && !enteringShortCut.HasValue)
		{
			_weaverThread.stuckPosA = ThreadAttachPos;
			if (Custom.Dist(lastPlacedThread, base.mainBodyChunk.pos) >= 150f)
			{
				MakeWeaverThread();
			}
		}
		else if (_weaverThread != null && enteringShortCut.HasValue)
		{
			WeaverThreadEnteringShortcut();
		}
		else if (!enteringShortCut.HasValue)
		{
			MakeWeaverThread();
		}
		if (room.game.devToolsActive)
		{
			if (Input.GetKeyDown(KeyCode.Comma))
			{
				MakeWeaverThread();
			}
			if (Input.GetKey("b") && room.game.cameras[0].room == room)
			{
				base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
				Stun(12);
			}
		}
		if (base.Consious)
		{
			footingCounter++;
			Act();
		}
		else
		{
			footingCounter = 0;
		}
		if (Footing)
		{
			for (int i = 0; i < 2; i++)
			{
				base.bodyChunks[i].vel *= 0.8f;
				base.bodyChunks[i].vel.y += base.gravity;
			}
			travelDir *= 0.9995f;
		}
	}

	public void Act()
	{
		AI.Update();
		if (specialMoveCounter > 0)
		{
			specialMoveCounter--;
			MoveTowards(room.MiddleOfTile(specialMoveDestination));
			travelDir = Vector2.Lerp(travelDir, Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination)), 0.4f);
			if (Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination), 5f))
			{
				specialMoveCounter = 0;
			}
			return;
		}
		if (!room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template) && !room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
		{
			footingCounter = 0;
		}
		StandardPather standardPather = AI.pathFinder as StandardPather;
		MovementConnection followingConnection = standardPather.FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		if (followingConnection == default(MovementConnection))
		{
			followingConnection = standardPather.FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
		}
		for (int i = 0; i < 8; i++)
		{
			if (!(followingConnection == default(MovementConnection)))
			{
				break;
			}
			followingConnection = standardPather.FollowPath(room.GetWorldCoordinate(base.bodyChunks[0].pos + Custom.eightDirectionsDiagonalsLast[i].ToVector2() * 20f), actuallyFollowingThisPath: true);
		}
		if (Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(standardPather.GetEffectualDestination), 10f) || AI.reachedTarget || AI.WaitForPlayer)
		{
			followingConnection = default(MovementConnection);
		}
		if (base.abstractCreature.controlled && (followingConnection == default(MovementConnection) || !AllowableControlledAIOverride(followingConnection.type)))
		{
			SafariControls(ref followingConnection);
		}
		if (followingConnection != default(MovementConnection))
		{
			if (followingConnection.type == MovementConnection.MovementType.ReachUp)
			{
				(AI.pathFinder as StandardPather).pastConnections.Clear();
			}
			if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				enteringShortCut = followingConnection.StartTile;
				if (base.safariControlled)
				{
					bool flag = false;
					List<IntVector2> list = new List<IntVector2>();
					ShortcutData[] shortcuts = room.shortcuts;
					for (int j = 0; j < shortcuts.Length; j++)
					{
						ShortcutData shortcutData = shortcuts[j];
						if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
						{
							list.Add(shortcutData.StartTile);
						}
						if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
						{
							flag = true;
						}
					}
					if (flag)
					{
						if (list.Count > 0)
						{
							list.Shuffle();
							NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
						}
						else
						{
							NPCTransportationDestination = followingConnection.destinationCoord;
						}
					}
				}
				else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
				{
					NPCTransportationDestination = followingConnection.destinationCoord;
				}
			}
			else if (followingConnection.type == MovementConnection.MovementType.OpenDiagonal || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
			{
				specialMoveCounter = 30;
				specialMoveDestination = followingConnection.DestTile;
			}
			else
			{
				Vector2 vector = room.MiddleOfTile(followingConnection.DestTile);
				bool flag2 = false;
				if (room.terrain != null && room.terrain.TrySnapToTerrain(vector, 10f, out var snapPos, base.mainBodyChunk.lastPos))
				{
					vector = snapPos;
					flag2 = true;
				}
				if (followingConnection.type == MovementConnection.MovementType.Standard && room.GetTile(followingConnection.DestTile).Terrain == Room.Tile.TerrainType.Slope)
				{
					MovementConnection movementConnection = standardPather.FollowPath(followingConnection.destinationCoord, actuallyFollowingThisPath: false);
					if (movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.Standard)
					{
						vector = room.MiddleOfTile(movementConnection.DestTile);
					}
				}
				if (lastFollowedConnection != default(MovementConnection) && lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
				{
					base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector) * 4f;
				}
				if (Footing)
				{
					if (flag2)
					{
						for (int k = 0; k < 2; k++)
						{
							float y = room.terrain.SnapToTerrain(base.bodyChunks[k].pos, 10f, base.bodyChunks[k].lastPos).y;
							base.bodyChunks[k].vel.y += Mathf.Clamp((y - base.bodyChunks[k].pos.y) / 8f, -1.2f, 1.2f);
						}
					}
					for (int l = 0; l < 2; l++)
					{
						if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
						{
							base.bodyChunks[l].vel.x += Mathf.Clamp((vector.x - base.bodyChunks[l].pos.x) / 8f, -1.2f, 1.2f);
						}
						else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
						{
							base.bodyChunks[l].vel.y += Mathf.Clamp((vector.y - base.bodyChunks[l].pos.y) / 8f, -1.2f, 1.2f);
						}
					}
				}
				if (lastFollowedConnection != default(MovementConnection) && (Footing || room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template)) && ((followingConnection.startCoord.x != followingConnection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x) || (followingConnection.startCoord.y != followingConnection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y)))
				{
					base.mainBodyChunk.vel *= 0.7f;
					base.bodyChunks[1].vel *= 0.5f;
				}
				if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
				{
					footingCounter = 0;
				}
				MoveTowards(vector);
			}
			lastFollowedConnection = followingConnection;
		}
		else
		{
			base.GoThroughFloors = false;
		}
		_ = runCycle;
		if (base.Consious && !Custom.DistLess(base.mainBodyChunk.pos, base.mainBodyChunk.lastPos, 2f))
		{
			runCycle += 0.25f;
		}
	}

	public void MoveTowards(Vector2 moveTo)
	{
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, moveTo);
		if (room.aimap.getAItile(base.bodyChunks[1].pos).acc >= AItile.Accessibility.Climb)
		{
			vector *= 0.5f;
		}
		if (!Footing)
		{
			vector *= 0.3f;
		}
		if (IsTileSolid(1, 0, -1) && (((double)vector.x < -0.5 && base.mainBodyChunk.pos.x > base.bodyChunks[1].pos.x + 5f) || ((double)vector.x > 0.5 && base.mainBodyChunk.pos.x < base.bodyChunks[1].pos.x - 5f)))
		{
			base.mainBodyChunk.vel.x -= ((vector.x < 0f) ? (-1f) : 1f) * 1.3f;
			base.bodyChunks[1].vel.x += ((vector.x < 0f) ? (-1f) : 1f) * 0.5f;
			if (!IsTileSolid(0, 0, 1))
			{
				base.mainBodyChunk.vel.y += 3.2f;
			}
		}
		float num = creatureParams.idleSpeedFactor;
		if (AI.behavior == RippleSpiderAI.Behavior.Travelling)
		{
			num = creatureParams.travelSpeedFactor;
		}
		base.mainBodyChunk.vel += vector * (3.2f * AI.runSpeed * num);
		AI.runSpeed = 1f;
		base.bodyChunks[1].vel -= vector * (AI.runSpeed * num);
		base.GoThroughFloors = moveTo.y < base.mainBodyChunk.pos.y - 5f;
	}

	public void SafariControls(ref MovementConnection followingConnection)
	{
		followingConnection = default(MovementConnection);
		if (inputWithDiagonals.HasValue)
		{
			MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
			if (followingConnection != default(MovementConnection))
			{
				type = followingConnection.type;
			}
			if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				type = MovementConnection.MovementType.ShortCut;
			}
			if (inputWithDiagonals.Value.AnyDirectionalInput)
			{
				followingConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
			}
			else if (inputWithDiagonals.Value.y < 0)
			{
				base.GoThroughFloors = true;
			}
			else
			{
				base.GoThroughFloors = false;
			}
		}
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
			base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
		}
	}

	public override void Stun(int st)
	{
		base.Stun(st);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		MakeCosmicWeb();
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
}
