using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class Frog : InsectoidCreature, IHaveInjectedPoisonColor, IPlayerEdible, SharedPhysics.IProjectileTracer
{
	public class CreatureParams
	{
		public float totalMass;

		public float sizeMultiplier;

		public float huntSpeedFactor;

		public float travelSpeedFactor;

		public float fleeSpeedFactor;

		public float idleSpeedFactor;

		public bool separateSwimBehavior;

		public float stunOnCollideVelocity;

		public float stunOnCollideDamage;

		public float stunOnCollideDuration;

		public CreatureParams()
		{
			totalMass = 0.1f;
			sizeMultiplier = 1f;
			huntSpeedFactor = 1f;
			travelSpeedFactor = 1f;
			fleeSpeedFactor = 1.1f;
			idleSpeedFactor = 1f;
			separateSwimBehavior = true;
		}
	}

	public FrogAI AI;

	public CreatureParams creatureParams;

	public FrogGraphics.Umbilical umbilical;

	public Creature thrownBy;

	public IntVector2 throwDir;

	public int changeDirCounter;

	public Vector2 thrownPos;

	public Vector2? firstFrameTraceFromPos;

	public int footingCounter;

	public int specialMoveCounter;

	public IntVector2 specialMoveDestination;

	public MovementConnection lastFollowedConnection;

	public float sitting;

	public float attachSide;

	public int timeAttached;

	public float struggleDir;

	public int croakCounter;

	public int croakDuration;

	public int painSoundCooldown;

	public bool throwLatch;

	public int throwLatchGracePeriod;

	public Creature ImmuneToLatch;

	public bool jumping;

	public int noJumps = 5;

	public int jumpStun;

	public BodyChunk jumpAtChunk;

	public Vector2 jumpAtPos;

	public bool walking;

	public bool swimming;

	public int swimStrokeCooldown = 30;

	public Vector2 walkDir;

	public int maxFeeds = 5;

	public Color injectedPoisonColor = Color.white;

	public bool attachOnSpawn;

	public Creature huntPrey;

	public int SpawnAttachAttempted;

	public int currentTargetFood;

	public int feedSize = 1;

	public int feedCD;

	public bool makeNewUmbilical;

	public int kickUntilDetachAndDead;

	public bool KickUntilDetached;

	public int bites;

	public int eaten;

	public FrogState FrogState => base.abstractCreature.state as FrogState;

	public override float VisibilityBonus
	{
		get
		{
			if (attachedObject != null)
			{
				return -1f;
			}
			return base.VisibilityBonus;
		}
	}

	public bool Footing => footingCounter > 20;

	public bool graspHasPlayer => attachedObject is Player;

	public PhysicalObject attachedObject
	{
		get
		{
			Grasp obj = base.grasps[0];
			if (obj == null)
			{
				return null;
			}
			return obj.grabbed.bodyChunks[base.grasps[0].chunkGrabbed].owner;
		}
	}

	public float Croaking => Mathf.Max((float)croakCounter / (float)croakDuration, 0f);

	public float DigIn => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(20f, 120f, timeAttached));

	public float StruggleIntensity
	{
		get
		{
			if (attachedObject != null)
			{
				return 1f - DigIn;
			}
			return 0f;
		}
	}

	public bool bloodBank => FrogState.bloodBank;

	public float bloodBankBloat => Mathf.Lerp(0f, 3f, (float)FrogState.feedSize / (float)maxFeeds);

	public Color InjectedPoisonColor
	{
		get
		{
			return injectedPoisonColor;
		}
		set
		{
			injectedPoisonColor = value;
		}
	}

	public bool ShouldHaveUmbilical => attachedObject != null;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => bloodBank;

	public bool AutomaticPickUp => false;

	public void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		creatureParams = new CreatureParams();
		UnityEngine.Random.state = state;
	}

	public Frog(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 2.75f * creatureParams.sizeMultiplier, creatureParams.totalMass / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), Mathf.Lerp(3f * creatureParams.sizeMultiplier, 3f * creatureParams.sizeMultiplier * bloodBankBloat, (float)FrogState.feedSize / (float)maxFeeds), creatureParams.totalMass / 2f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 6f * creatureParams.sizeMultiplier, BodyChunkConnection.Type.Pull, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 0;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		bites = 3;
		ChangeCollisionLayer(0);
		if (FrogState.creatureAttachedTo.HasValue)
		{
			attachOnSpawn = true;
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new FrogGraphics(this);
		}
		base.graphicsModule.Reset();
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
		if (attachOnSpawn)
		{
			if (!FrogState.creatureAttachedTo.HasValue)
			{
				return;
			}
			foreach (AbstractCreature creature in room.abstractRoom.creatures)
			{
				EntityID iD = creature.ID;
				EntityID? creatureAttachedTo = FrogState.creatureAttachedTo;
				if (creatureAttachedTo.HasValue && iD == creatureAttachedTo.GetValueOrDefault() && base.abstractCreature.realizedCreature != null && !base.abstractCreature.realizedCreature.dead)
				{
					Attach(creature.realizedCreature.mainBodyChunk);
					attachOnSpawn = false;
					break;
				}
			}
			SpawnAttachAttempted++;
			if (SpawnAttachAttempted >= 40)
			{
				Die();
			}
		}
		if (throwLatch || throwLatchGracePeriod > 0)
		{
			throwLatchGracePeriod = Mathf.Max(throwLatchGracePeriod - 1, 0);
			ThrowUpdate();
		}
		if (!CheckIfGrabbedByPlayer() && !throwLatch && throwLatchGracePeriod == 0)
		{
			ImmuneToLatch = null;
		}
		if (base.grasps != null)
		{
			if (graspHasPlayer)
			{
				Player player = attachedObject as Player;
				if (player.injectedPoison > 0f)
				{
					float num = player.injectedPoison;
					float num2 = 0.0027777778f * Mathf.Max(player.injectedPoison, 0.05f);
					player.injectedPoison = Mathf.Max(player.injectedPoison - num2, 0f);
					InjectPoison(num - player.injectedPoison, player.injectedPoisonColor);
				}
				if (player.CurrentFood > currentTargetFood)
				{
					player.SetMalnourished(m: false, malnourishedByCreature: true);
				}
			}
			else if (attachedObject is Lizard lizard && lizard.Template.type == WatcherEnums.CreatureTemplateType.BasiliskLizard)
			{
				InjectPoison(0.005f, lizard.effectColor);
			}
			else if (attachedObject is Tardigrade tardigrade)
			{
				InjectPoison(0.005f, tardigrade.iVars.bodyColor);
			}
			if (ShouldHaveUmbilical && ((umbilical?.room != null && umbilical.room != room) || makeNewUmbilical) && base.Consious)
			{
				umbilical.RemoveFromRoom();
				umbilical.slatedForDeletetion = true;
				makeNewUmbilical = false;
				MakeUmbilical();
			}
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		base.gravity = ((base.grasps[0] == null) ? 0.9f : 0.2f);
		walking = false;
		swimming = false;
		if (base.grasps[0] != null)
		{
			Attached();
		}
		else if (base.Consious)
		{
			if (InAccessibleTiles(0))
			{
				footingCounter++;
				Act();
			}
			else
			{
				footingCounter = 0;
			}
		}
		else
		{
			footingCounter = 0;
			if (croakCounter < -80 && UnityEngine.Random.value < 0.01f && grabbedBy.Count > 0 && !base.dead)
			{
				croakDuration = UnityEngine.Random.Range(15, 25);
				croakCounter = croakDuration;
				room.PlaySound((AI.behavior == FrogAI.Behavior.Hunt) ? WatcherEnums.WatcherSoundID.Frog_Aggressive_Croak : WatcherEnums.WatcherSoundID.Frog_Passive_Croak, base.mainBodyChunk);
			}
		}
		if (Footing && !throwLatch && !swimming)
		{
			for (int i = 0; i < 2; i++)
			{
				base.bodyChunks[i].vel *= 0.8f;
				base.bodyChunks[i].vel.y += base.gravity * (1f - sitting);
			}
		}
		croakCounter--;
		if (painSoundCooldown > 0)
		{
			painSoundCooldown--;
		}
		if (footingCounter == 0)
		{
			sitting = 0f;
		}
		if (base.Consious && !Footing && AI.behavior == FrogAI.Behavior.Flee && !base.safariControlled)
		{
			for (int j = 0; j < 2; j++)
			{
				if (room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[j].pos), base.Template))
				{
					base.bodyChunks[j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * (UnityEngine.Random.value * 5f);
				}
			}
		}
		if (eaten > 0)
		{
			eaten--;
			if (eaten == 0)
			{
				Destroy();
			}
		}
	}

	public bool InAccessibleTiles(int chunk)
	{
		Vector2 pos = base.bodyChunks[chunk].pos + new Vector2(0f, 20f);
		if (!room.aimap.TileAccessibleToCreature(base.bodyChunks[chunk].pos, base.Template))
		{
			if (room.aimap.TileAccessibleToCreature(pos, base.Template))
			{
				return room.aimap.getAItile(pos).acc == AItile.Accessibility.CurvedFloor;
			}
			return false;
		}
		return true;
	}

	public bool CheckIfGrabbedByPlayer()
	{
		foreach (Grasp item in grabbedBy)
		{
			if (item.grabber is Player)
			{
				return true;
			}
		}
		return false;
	}

	public void setCollisions(bool collisions)
	{
		base.CollideWithObjects = collisions;
		base.CollideWithSlopes = collisions;
		base.CollideWithTerrain = collisions;
		canBeHitByWeapons = collisions;
		ChangeCollisionLayer((collisions && bloodBank) ? 1 : 0);
	}

	public void MakeUmbilical()
	{
		if (attachedObject != null)
		{
			umbilical = new FrogGraphics.Umbilical(room, this, attachedObject as Creature, base.grasps[0].chunkGrabbed, base.firstChunk.vel);
			room?.AddObject(umbilical);
		}
	}

	public bool ChunkGrabbedTooMuch(BodyChunk chunk)
	{
		if ((float)chunk.owner.grabbedBy.Count >= Mathf.Max(1f, Mathf.Pow(chunk.owner.TotalMass, 0.6f)))
		{
			return true;
		}
		int num = 0;
		foreach (Grasp item in chunk.owner.grabbedBy)
		{
			if (item.chunkGrabbed == chunk.index)
			{
				num++;
				if ((float)num >= Mathf.Pow(chunk.mass, 0.6f))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Attach(BodyChunk chunk, bool suckFood = false)
	{
		if (ImmuneToLatch == chunk.owner || !(chunk.owner is Creature creature) || ((!ModManager.MMF || !MMF.cfgVanillaExploits.Value) && (grabbedBy.Count > 0 || creature.dead)) || ChunkGrabbedTooMuch(chunk) || (creature is Player && (creature as Player).Malnourished))
		{
			return;
		}
		throwLatch = false;
		throwLatchGracePeriod = 0;
		setCollisions(collisions: false);
		timeAttached = 0;
		room.PlaySound(WatcherEnums.WatcherSoundID.Frog_Attach, base.mainBodyChunk);
		if (base.grasps[0] != null || Grab(creature, 0, chunk.index, Grasp.Shareability.NonExclusive, 0f, overrideEquallyDominant: false, pacifying: false))
		{
			Vector2 lhs = Custom.PerpendicularVector(chunk.Rotation);
			Vector2 vector = chunk.pos - base.bodyChunks[0].lastLastPos;
			float num = ((Vector2.Dot(lhs, vector) > 0f) ? 90f : 270f);
			float b = Custom.VecToDeg(-vector) - Custom.VecToDeg(chunk.Rotation);
			float num2 = Mathf.InverseLerp(10f, 50f, chunk.rad);
			float maxDelta = Mathf.Lerp(0f, 70f, num2);
			b = Mathf.LerpAngle(num, b, num2 * 0.75f);
			b = Mathf.MoveTowardsAngle(num, b, maxDelta);
			attachSide = b;
			MakeUmbilical();
			FrogState.creatureAttachedTo = (attachedObject as Creature).abstractCreature.ID;
			(base.graphicsModule as FrogGraphics)?.ChangeOverlap(attached: true);
			if (suckFood)
			{
				SuckFood();
				creature.Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass, chunk, null, DamageType.Bite, 0.02f, 50f);
			}
		}
	}

	public void Detach(bool suckFoodOnExit = false)
	{
		if (FrogState.creatureAttachedTo.HasValue)
		{
			if (suckFoodOnExit)
			{
				SuckFood();
			}
			timeAttached = 0;
			FrogState.creatureAttachedTo = null;
			LoseAllGrasps();
			setCollisions(collisions: true);
			if (room != null)
			{
				room.PlaySound(graspHasPlayer ? SoundID.Leech_Detatch_Player : SoundID.Leech_Detatch_NPC, base.mainBodyChunk);
				(base.graphicsModule as FrogGraphics)?.ChangeOverlap(attached: false);
			}
		}
	}

	public override void ReleaseGrasp(int grasp)
	{
		if (FrogState.creatureAttachedTo.HasValue && base.grasps[grasp].grabbed?.abstractPhysicalObject.ID == FrogState.creatureAttachedTo)
		{
			Detach();
		}
		base.ReleaseGrasp(grasp);
	}

	public void PushOutOfChunk(BodyChunk chunk)
	{
		if (Custom.DistLess(chunk.pos, base.mainBodyChunk.pos, chunk.rad + base.mainBodyChunk.rad))
		{
			Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, chunk.pos);
			float num = Vector2.Distance(base.mainBodyChunk.pos, chunk.pos);
			float num2 = base.mainBodyChunk.mass / (base.mainBodyChunk.mass + chunk.mass);
			base.mainBodyChunk.vel += vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * (1f - num2);
			base.mainBodyChunk.pos += vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * (1f - num2);
			chunk.vel -= vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * num2;
			chunk.pos -= vector * (num - (chunk.rad + base.mainBodyChunk.rad)) * num2;
		}
	}

	public void Attached()
	{
		if (feedCD > 0)
		{
			feedCD--;
		}
		else if (SuckFood())
		{
			return;
		}
		timeAttached++;
		if (StruggleIntensity > 0f && UnityEngine.Random.value < 0.1f)
		{
			struggleDir = UnityEngine.Random.Range(-1f, 1f);
		}
		if ((attachedObject as Creature).enteringShortCut.HasValue)
		{
			makeNewUmbilical = true;
		}
		BodyChunk bodyChunk = base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed];
		Vector2 vector = Custom.DegToVec(attachSide + Custom.VecToDeg(bodyChunk.Rotation));
		Vector2 vector2 = bodyChunk.pos;
		if (attachedObject.graphicsModule is PlayerGraphics playerGraphics)
		{
			vector2 = playerGraphics.drawPositions[bodyChunk.index, 0];
		}
		vector2 += vector * (bodyChunk.rad - DigIn * 5f);
		if (bodyChunk.owner.evenUpdate != evenUpdate)
		{
			vector2 += bodyChunk.vel;
		}
		Vector2 vector3 = Custom.DirVec(base.mainBodyChunk.pos, vector2);
		float num = Vector2.Distance(base.mainBodyChunk.pos, vector2);
		float num2 = base.mainBodyChunk.mass / (base.mainBodyChunk.mass + bodyChunk.mass);
		Vector2 vector4 = vector3 * (num * (1f - num2));
		base.mainBodyChunk.vel += vector4;
		base.mainBodyChunk.pos += vector4;
		WeightedPush(1, 0, vector, 8f);
		BodyChunk[] array = base.bodyChunks;
		foreach (BodyChunk obj in array)
		{
			obj.vel = Vector2.Lerp(obj.vel, bodyChunk.vel * 0.5f, 0.4f);
		}
		base.bodyChunks[1].vel += bodyChunk.Rotation * (struggleDir * StruggleIntensity * 2f);
		base.bodyChunks[1].vel *= 0.7f;
		if ((attachedObject as Creature).dead && (!ModManager.MMF || !MMF.cfgVanillaExploits.Value))
		{
			Detach();
		}
		else if (attachedObject is Player && (attachedObject as Player).stun >= 120 && !KickUntilDetached)
		{
			Detach((attachedObject as Creature).dead);
		}
		else if (KickUntilDetached && (attachedObject as Creature).Consious)
		{
			Detach();
			Jump(base.mainBodyChunk.pos + new Vector2(UnityEngine.Random.Range(-5, 5), 0f), ignoreNoJumps: true);
		}
		else if (KickUntilDetached && base.dead)
		{
			kickUntilDetachAndDead++;
			if (kickUntilDetachAndDead > 240)
			{
				kickUntilDetachAndDead = 0;
				Detach(suckFoodOnExit: true);
				Jump(base.mainBodyChunk.pos + new Vector2(UnityEngine.Random.Range(-5, 5), 0f), ignoreNoJumps: true);
			}
		}
		if (base.safariControlled && SafariJumpInput())
		{
			Detach();
			Jump(base.mainBodyChunk.pos + new Vector2(UnityEngine.Random.Range(-5, 5), 0f), ignoreNoJumps: true);
		}
	}

	public bool SuckFood()
	{
		if ((attachedObject as Creature).enteringShortCut.HasValue)
		{
			return false;
		}
		room.PlaySound(WatcherEnums.WatcherSoundID.Frog_Drain, base.mainBodyChunk);
		FrogState.feedSize = Mathf.Min(FrogState.feedSize + 1, maxFeeds);
		if (graspHasPlayer)
		{
			if (room.abstractRoom.shelter)
			{
				feedCD = 600;
				return false;
			}
			feedCD = UnityEngine.Random.Range(2400, 4800);
			if ((attachedObject as Player).CurrentFood - feedSize < 0 || FrogState.feedSize == maxFeeds)
			{
				(attachedObject as Player).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass, (attachedObject as Player).mainBodyChunk, null, DamageType.Bite, 0.02f, 200f);
				(attachedObject as Player).SetMalnourished(m: true, malnourishedByCreature: true);
				(attachedObject as Player).Blink(100);
				(attachedObject as Player).exhausted = true;
				(attachedObject as Player).lungsExhausted = true;
				KickUntilDetached = true;
				return true;
			}
			(attachedObject as Player).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass, (attachedObject as Player).mainBodyChunk, null, DamageType.Bite, 0.02f, 50f);
			(attachedObject as Player).SubtractFood(feedSize);
			currentTargetFood = (attachedObject as Player).CurrentFood;
		}
		else
		{
			base.bodyChunks[1].rad = Mathf.Lerp(3f * creatureParams.sizeMultiplier, 3f * creatureParams.sizeMultiplier * bloodBankBloat, (float)FrogState.feedSize / (float)maxFeeds);
			feedCD = (int)Mathf.Clamp(1600f - attachedObject.TotalMass * 100f, 650f, 1600f);
			(attachedObject as Creature).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass, (attachedObject as Creature).mainBodyChunk, null, DamageType.Bite, 0.02f, 120f);
		}
		return false;
	}

	public void Swim()
	{
		MovementConnection followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[0].pos), actuallyFollowingThisPath: true);
		if (followingConnection == default(MovementConnection))
		{
			followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
		}
		if (followingConnection == default(MovementConnection) && Math.Abs(base.abstractCreature.pos.y - room.defaultWaterLevel) < 4)
		{
			followingConnection = (AI.pathFinder as StandardPather).FollowPath(new WorldCoordinate(base.abstractCreature.pos.room, base.abstractCreature.pos.x, room.defaultWaterLevel, base.abstractCreature.pos.abstractNode), actuallyFollowingThisPath: true);
		}
		if (base.safariControlled)
		{
			SafariControls(ref followingConnection);
		}
		if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
		{
			enteringShortCut = followingConnection.StartTile;
			if (base.safariControlled)
			{
				bool flag = false;
				List<IntVector2> list = new List<IntVector2>();
				ShortcutData[] shortcuts = room.shortcuts;
				for (int i = 0; i < shortcuts.Length; i++)
				{
					ShortcutData shortcutData = shortcuts[i];
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
			return;
		}
		Vector2 vector;
		if (followingConnection != default(MovementConnection))
		{
			vector = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(followingConnection.DestTile));
			bool flag2 = false;
			if (base.Submersion < 1f)
			{
				base.mainBodyChunk.vel.y += 1.5f;
				flag2 = true;
			}
			bool flag3 = flag2 && room.PointSubmerged(room.MiddleOfTile(followingConnection.DestTile));
			Vector2 jumpDir = room.MiddleOfTile(followingConnection.DestTile);
			if (noJumps == 0)
			{
				if (base.safariControlled)
				{
					if (SafariJumpInput())
					{
						Player.InputPackage? inputPackage = inputWithDiagonals;
						if (inputPackage.HasValue)
						{
							Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
							Jump(base.mainBodyChunk.pos + valueOrDefault.analogueDir * 20f);
							noJumps = swimStrokeCooldown;
						}
					}
				}
				else if (flag3 || !UpcomingTightSpace(followingConnection.destinationCoord))
				{
					if (flag3)
					{
						jumpDir = base.mainBodyChunk.pos + new Vector2(0f, -20f);
					}
					Jump(jumpDir);
					noJumps = swimStrokeCooldown;
				}
			}
			if (noJumps > 0)
			{
				base.buoyancy = 0.91f;
				base.bodyChunks[1].vel = base.bodyChunks[1].vel.normalized * Mathf.Min(base.bodyChunks[1].vel.magnitude, base.bodyChunks[0].vel.magnitude * 0.95f);
			}
		}
		else
		{
			vector = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(AI.pathFinder.GetDestination));
		}
		if (base.safariControlled)
		{
			Player.InputPackage? inputPackage = inputWithDiagonals;
			if (inputPackage.HasValue)
			{
				vector = inputPackage.GetValueOrDefault().analogueDir;
			}
		}
		base.mainBodyChunk.vel += vector * (1.3f * Mathf.Lerp(0.5f, 1f, AI.runSpeed) * (room.GetTile(base.mainBodyChunk.pos).WaterSurface ? 0.7f : 1f) * 0.1f);
		swimming = true;
	}

	public bool UpcomingTightSpace(WorldCoordinate destinationCoord)
	{
		for (int i = 0; i < 4; i++)
		{
			if (destinationCoord == AI.pathFinder.GetDestination)
			{
				if (AI.focusCreature != null)
				{
					return !AI.focusCreature.VisualContact;
				}
				return true;
			}
			if (!destinationCoord.TileDefined || destinationCoord.room != room.abstractRoom.index)
			{
				return false;
			}
			if (room.aimap.getAItile(destinationCoord).narrowSpace)
			{
				return true;
			}
			MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(destinationCoord, actuallyFollowingThisPath: false);
			if (movementConnection == default(MovementConnection))
			{
				return false;
			}
			destinationCoord = movementConnection.destinationCoord;
		}
		return false;
	}

	public void Act()
	{
		if (noJumps > 0 && croakCounter <= 0)
		{
			noJumps--;
		}
		base.buoyancy = 0.95f;
		if (base.Submersion > 0.3f && creatureParams.separateSwimBehavior)
		{
			Swim();
			AI.Update();
			return;
		}
		AI.Update();
		if (specialMoveCounter > 0)
		{
			specialMoveCounter--;
			MoveTowards(room.MiddleOfTile(specialMoveDestination));
			if (Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination), 5f))
			{
				specialMoveCounter = 0;
			}
			return;
		}
		if (!InAccessibleTiles(0) && !InAccessibleTiles(1))
		{
			footingCounter = 0;
		}
		if (jumpStun > 0)
		{
			if (!jumping)
			{
				jumpStun = Math.Max(0, jumpStun - UnityEngine.Random.Range(6, 10));
			}
			else
			{
				jumpStun = Math.Max(0, jumpStun - 1);
			}
		}
		if (croakCounter < -80 && UnityEngine.Random.value < 0.01f && (sitting == 1f || grabbedBy.Count > 0))
		{
			croakDuration = UnityEngine.Random.Range(15, 25);
			croakCounter = croakDuration;
			room.PlaySound((AI.behavior == FrogAI.Behavior.Hunt) ? WatcherEnums.WatcherSoundID.Frog_Aggressive_Croak : WatcherEnums.WatcherSoundID.Frog_Passive_Croak, base.mainBodyChunk);
		}
		if (jumping || throwLatch)
		{
			if (base.mainBodyChunk.ContactPoint != new IntVector2(0, 0) || !base.Consious)
			{
				throwLatchGracePeriod = (throwLatch ? 40 : 0);
				if (throwLatchGracePeriod == 0 && base.graphicsModule != null)
				{
					(base.graphicsModule as FrogGraphics).DoneWithJump();
				}
				ImmuneToLatch = null;
				throwLatch = false;
				thrownBy = null;
				jumping = false;
				return;
			}
			ThrowUpdate();
		}
		if (jumpStun > 0 || throwLatch)
		{
			return;
		}
		MovementConnection followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
		if (followingConnection == default(MovementConnection))
		{
			followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[0].pos), actuallyFollowingThisPath: true);
		}
		IntVector2 tilePosition = room.GetTilePosition(base.bodyChunks[1].pos);
		if (followingConnection == default(MovementConnection) && room.terrain != null && room.terrain.ObstructsTile(tilePosition))
		{
			for (int i = 0; i < 5; i++)
			{
				tilePosition.y++;
				if (room.GetTile(tilePosition).Solid)
				{
					break;
				}
				if (room.aimap.getAItile(tilePosition).acc == AItile.Accessibility.CurvedFloor)
				{
					followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(tilePosition), actuallyFollowingThisPath: true);
					if (followingConnection.StartTile.x != followingConnection.DestTile.x)
					{
						break;
					}
				}
			}
		}
		if (base.abstractCreature.controlled && (followingConnection == default(MovementConnection) || !AllowableControlledAIOverride(followingConnection.type)))
		{
			SafariControls(ref followingConnection);
		}
		if (base.bodyChunks[1].ContactPoint.y == -1)
		{
			float num = 0f;
			if (followingConnection != default(MovementConnection))
			{
				num = (followingConnection.DestTile - followingConnection.StartTile).ToVector2().normalized.x;
			}
			float num2 = Mathf.Sqrt(sitting);
			base.bodyChunks[0].vel.x += num2 * num * 0.25f;
			base.bodyChunks[0].vel.y += num2 * 1.5f;
			base.bodyChunks[1].vel.x -= num2 * num * 0.25f;
			base.bodyChunks[1].vel.y -= num2 * 1.5f;
		}
		bool flag = false;
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
					bool flag2 = false;
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
							flag2 = true;
						}
					}
					if (flag2)
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
				if (lastFollowedConnection != default(MovementConnection) && lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
				{
					base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector) * 4f;
				}
				if (followingConnection.startCoord.y < followingConnection.destinationCoord.y || !IsTileSolid(0, 0, -1) || followingConnection.startCoord.x == followingConnection.destinationCoord.x || jumpStun > 0 || UpcomingTightSpace(followingConnection.destinationCoord))
				{
					for (int k = 0; k < 2; k++)
					{
						if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
						{
							base.bodyChunks[k].vel.x += Mathf.Min((vector.x - base.bodyChunks[k].pos.x) / 8f, 1.2f);
						}
						else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
						{
							base.bodyChunks[k].vel.y += Mathf.Min((vector.y - base.bodyChunks[k].pos.y) / 8f, 1.2f);
						}
					}
					if (lastFollowedConnection != default(MovementConnection) && (Footing || InAccessibleTiles(0)) && ((followingConnection.startCoord.x != followingConnection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x) || (followingConnection.startCoord.y != followingConnection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y)))
					{
						base.mainBodyChunk.vel *= 0.7f;
						base.bodyChunks[1].vel *= 0.5f;
					}
					if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
					{
						footingCounter = 0;
					}
					walking = true;
					MoveTowards(vector);
				}
				else if (!base.safariControlled)
				{
					Jump(vector);
					flag = true;
				}
			}
			lastFollowedConnection = followingConnection;
		}
		else
		{
			base.GoThroughFloors = false;
		}
		if (base.safariControlled && SafariJumpInput())
		{
			Vector2 jumpDir = room.MiddleOfTile(base.mainBodyChunk.pos + new Vector2(0f, 20f));
			if (followingConnection != default(MovementConnection))
			{
				jumpDir = room.MiddleOfTile(followingConnection.DestTile);
			}
			Jump(jumpDir);
			flag = true;
		}
		if ((followingConnection == default(MovementConnection) || flag) && IsTileSolid(1, 0, -1))
		{
			sitting = Custom.LerpAndTick(sitting, 1f, 0.1f, 0.05f);
		}
		else
		{
			sitting = Math.Max(sitting - 0.1f, 0f);
		}
	}

	public void InitiateJump(BodyChunk target)
	{
		jumpAtChunk = target;
		jumpAtPos = Vector2.zero;
	}

	public void Jump(Vector2 jumpDir, bool ignoreNoJumps = false)
	{
		if (noJumps <= 0 || ignoreNoJumps)
		{
			KickUntilDetached = false;
			Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, jumpDir);
			vector = Vector3.Normalize(vector);
			float num = Custom.LerpMap(jumpDir.y, -1f, 1f, 0.7f, 1.2f, 1.1f);
			if (bloodBank)
			{
				num *= 0.65f;
			}
			vector = new Vector2(vector.x * 1.3f, vector.y + ((base.Submersion > 0.3f) ? 0f : 1.25f));
			footingCounter = 0;
			base.mainBodyChunk.vel *= 0.5f;
			base.bodyChunks[1].vel -= vector * 0.5f;
			base.mainBodyChunk.vel += vector * (8f * num);
			room.PlaySound(WatcherEnums.WatcherSoundID.Frog_Jump, base.mainBodyChunk);
			jumping = true;
			jumpStun = (bloodBank ? 15 : 30);
			sitting = 0f;
			if (!base.safariControlled)
			{
				noJumps = 75;
			}
		}
	}

	public void Attack()
	{
		if (!base.Consious || noJumps > 0)
		{
			return;
		}
		Vector2? vector = null;
		if (jumpAtPos != Vector2.zero)
		{
			vector = jumpAtPos;
		}
		if (jumpAtChunk != null)
		{
			vector = jumpAtChunk.pos;
		}
		if (!vector.HasValue)
		{
			return;
		}
		Vector2 p = new Vector2(vector.Value.x, vector.Value.y);
		if (!room.GetTile(vector.Value + new Vector2(0f, 20f)).Solid)
		{
			vector += new Vector2(0f, Mathf.InverseLerp(40f, 200f, Vector2.Distance(base.mainBodyChunk.pos, vector.Value)) * 20f);
		}
		Vector2 vector2 = Custom.DirVec(base.mainBodyChunk.pos, vector.Value);
		if (!Custom.DistLess(base.mainBodyChunk.pos, p, Custom.LerpMap(Vector2.Dot(vector2, Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos)), -0.1f, 0.8f, 0f, 300f, 0.4f)))
		{
			jumpAtChunk = null;
			return;
		}
		if (!room.GetTile(base.mainBodyChunk.pos + new Vector2(0f, 20f)).Solid && !room.GetTile(base.bodyChunks[1].pos + new Vector2(0f, 20f)).Solid)
		{
			vector2 = Vector3.Slerp(vector2, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Distance(base.mainBodyChunk.pos, vector.Value), 40f, 200f, 0.05f, 0.2f));
		}
		Jump(vector2);
	}

	public void MoveTowards(Vector2 moveTo)
	{
		Vector2 vector = (walkDir = Custom.DirVec(base.mainBodyChunk.pos, moveTo));
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
		if (AI.behavior == FrogAI.Behavior.Flee || AI.behavior == FrogAI.Behavior.EscapeRain)
		{
			num = creatureParams.fleeSpeedFactor;
		}
		else if (AI.behavior == FrogAI.Behavior.ReturnPrey || AI.behavior == FrogAI.Behavior.Travelling || AI.behavior == FrogAI.Behavior.InvestigateSound)
		{
			num = creatureParams.travelSpeedFactor;
		}
		else if (AI.behavior == FrogAI.Behavior.Hunt)
		{
			num = creatureParams.huntSpeedFactor;
		}
		base.mainBodyChunk.vel += vector * (2.1f * AI.runSpeed * num);
		base.bodyChunks[1].vel -= vector * (0.5f * AI.runSpeed * num);
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

	public bool SafariJumpInput()
	{
		ref Player.InputPackage? reference = ref inputWithDiagonals;
		if (reference.HasValue && reference.GetValueOrDefault().jmp)
		{
			ref Player.InputPackage? reference2 = ref lastInputWithDiagonals;
			if (!reference2.HasValue)
			{
				return false;
			}
			return !reference2.GetValueOrDefault().jmp;
		}
		return false;
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (throwLatch && !bloodBank)
		{
			Attach(otherObject.bodyChunks[otherChunk], suckFood: true);
			throwLatch = false;
		}
		if (base.Consious && creatureParams.stunOnCollideVelocity > 0f && Vector2.Distance(base.bodyChunks[myChunk].vel, otherObject.bodyChunks[otherChunk].vel) >= creatureParams.stunOnCollideVelocity && otherObject is Creature)
		{
			(otherObject as Creature).Violence(base.bodyChunks[myChunk], base.bodyChunks[myChunk].vel * base.bodyChunks[myChunk].mass, otherObject.bodyChunks[otherChunk], null, DamageType.Blunt, creatureParams.stunOnCollideDamage, creatureParams.stunOnCollideDuration);
		}
	}

	public void ThrowUpdate()
	{
		Vector2 pos = base.bodyChunks[1].pos + base.bodyChunks[1].vel;
		SharedPhysics.CollisionResult result = SharedPhysics.TraceProjectileAgainstBodyChunks(this, room, firstFrameTraceFromPos.HasValue ? firstFrameTraceFromPos.Value : base.bodyChunks[1].pos, ref pos, base.bodyChunks[1].rad + 7.5f, 1, thrownBy, hitAppendages: true);
		firstFrameTraceFromPos = null;
		bool hit = HitSomething(result);
		if (result.obj != null)
		{
			if (thrownBy != null && result.obj is Creature)
			{
				room.socialEventRecognizer.WeaponAttack(this, thrownBy, result.obj as Creature, hit);
			}
			thrownBy = null;
		}
	}

	public bool HitSomething(SharedPhysics.CollisionResult result)
	{
		if (result.obj == null || result.chunk == null)
		{
			return false;
		}
		if (result.obj.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !result.obj.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides)
		{
			return false;
		}
		if (!bloodBank && attachedObject == null)
		{
			Attach(result.chunk, suckFood: true);
		}
		return true;
	}

	public bool HitThisObject(PhysicalObject obj)
	{
		_ = obj is Player;
		bool flag = ModManager.CoopAvailable && Custom.rainWorld.options.friendlyFire;
		bool flag2 = room.game.IsArenaSession && room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers;
		if (false)
		{
			return flag || flag2;
		}
		return true;
	}

	public bool HitThisChunk(BodyChunk chunk)
	{
		if (firstFrameTraceFromPos.HasValue)
		{
			return Vector2.Dot((chunk.pos - base.firstChunk.lastPos).normalized, throwDir.ToVector2().normalized) > 0.3f;
		}
		return true;
	}

	public void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		this.thrownBy = thrownBy;
		this.thrownPos = thrownPos;
		this.throwDir = throwDir;
		this.firstFrameTraceFromPos = firstFrameTraceFromPos;
		changeDirCounter = 3;
		base.firstChunk.MoveFromOutsideMyUpdate(eu, thrownPos);
		if (throwDir.x != 0)
		{
			base.firstChunk.vel.y = thrownBy.mainBodyChunk.vel.y * 0.5f;
			base.firstChunk.vel.x = thrownBy.mainBodyChunk.vel.x * 0.2f;
			base.firstChunk.vel.x += (float)throwDir.x * 40f * frc;
			base.firstChunk.vel.y += 3f;
		}
		else if (throwDir.y != 0)
		{
			base.firstChunk.vel.x = thrownBy.mainBodyChunk.vel.x * 0.5f;
			base.firstChunk.vel.y = (float)throwDir.y * 40f * frc;
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
	}

	public override void Die()
	{
		Detach();
		base.Die();
	}

	public override Color ShortCutColor()
	{
		return base.ShortCutColor();
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (attachedObject == null && RippleViolenceCheck(source))
		{
			base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		PlayPainSound();
	}

	public override void Stun(int st)
	{
		base.Stun(st);
		Detach();
	}

	public void PlayPainSound()
	{
		if (painSoundCooldown == 0 && base.State.alive)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Frog_Pain, base.mainBodyChunk);
			painSoundCooldown = 30;
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

	public void BitByPlayer(Grasp grasp, bool eu)
	{
		bites--;
		if (!base.dead)
		{
			Die();
		}
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Final_Bite_Fly : SoundID.Slugcat_Bite_Fly, base.mainBodyChunk);
		base.mainBodyChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites < 1 && eaten == 0)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			eaten = 3;
		}
	}

	public void ThrowByPlayer()
	{
	}
}
