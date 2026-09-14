using RWCustom;
using UnityEngine;

namespace Watcher;

public class FireSprite : Creature, Weapon.INotifyOfFlyingWeapons
{
	public FireSpriteAI ai;

	public float speedBoost;

	public Counter stuckTimer = new Counter(20);

	public Counter stuckFrustration = new Counter(100, 0, countsUp: true);

	public Counter wingOpenTimer = new Counter(250);

	public Counter justSwitchedRooms = new Counter(20, 0, countsUp: true);

	public Counter shortcutCooldown = new Counter(40);

	public bool wingsOpen;

	public bool divingToHelp;

	public Counter vocalizeSoundCooldown = new Counter(200);

	public Counter helpPing = new Counter(40);

	public bool canPlayEntranceSound;

	public LightSource lightSource;

	public Vector2 ejectVector;

	public Vector2? evasionVector;

	public bool debug;

	public int lastRoom = -1;

	public int steps;

	public int maxSteps;

	public StaticSoundLoop soundLoop;

	public SoundID soundLoopID;

	public Counter boostSoundCooldown = new Counter(40);

	public Counter soundLoopTransition = new Counter(20, 0, countsUp: true);

	public ChunkSoundEmitter boostSE;

	public ChunkSoundEmitter evasionBoostSE;

	public ChunkSoundEmitter helpPingSE;

	public MovementConnection testConnection;

	public bool canExplode = true;

	public FireSpriteGraphics.Data graphicsData;

	public DebugSprite potentialTargetDbg;

	public DebugSprite destinationDbg;

	public DebugSprite currentStepDbg;

	public float lastDir;

	public float dir;

	public float speed => 1f + speedBoost;

	public bool IsFarFromCamera
	{
		get
		{
			Vector2 b = room.game.cameras[0].pos + room.game.rainWorld.screenSize * 0.5f;
			return Mathf.Min(Vector2.Distance(base.mainBodyChunk.pos, b), Vector2.Distance(room.MiddleOfTile(testConnection.destinationCoord), b)) > 2000f;
		}
	}

	public int TargetSize => 1 + (int)(base.mainBodyChunk.vel.magnitude / 10f) + (int)(stuckFrustration.normalized * 20f);

	public float DistanceToDestination => Custom.WorldCoordFloatDist(base.abstractCreature.pos, ai.pathFinder.GetDestination);

	public FireSprite(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.05f)
		{
			collideWithObjects = false,
			goThroughFloors = true
		};
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.97f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.9f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		windAffectiveness = 0.1f;
		lightSource = new LightSource(Vector2.zero, environmentalLight: false, Color.white, null)
		{
			setRad = 100f,
			setAlpha = 1f
		};
		abstractCreature.HypothermiaImmune = true;
		justSwitchedRooms.SetToMax();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			GraphicsModule graphicsModule2 = (base.graphicsModule = new FireSpriteGraphics(this));
		}
		base.graphicsModule.Reset();
	}

	public void UpdateTimers()
	{
		justSwitchedRooms.Tick();
		shortcutCooldown.Tick();
	}

	public override Color ShortCutColor()
	{
		return BoxWormGraphics.BaseColor(ai.creature.Room.realizedRoom);
	}

	public void RoomSwitched()
	{
		if (lastRoom == -1 || lastRoom == room.abstractRoom.index)
		{
			return;
		}
		if (lightSource != null)
		{
			lightSource.RemoveFromRoom();
			room.AddObject(lightSource);
			lightSource.HardSetPos(base.mainBodyChunk.pos);
		}
		soundLoop = null;
		base.mainBodyChunk.lastPos = base.mainBodyChunk.pos;
		base.mainBodyChunk.lastLastPos = base.mainBodyChunk.pos;
		if (debug)
		{
			if (potentialTargetDbg != null)
			{
				potentialTargetDbg.RemoveFromRoom();
				room.AddObject(potentialTargetDbg);
			}
			if (currentStepDbg != null)
			{
				currentStepDbg.RemoveFromRoom();
				room.AddObject(currentStepDbg);
			}
			if (destinationDbg != null)
			{
				destinationDbg.RemoveFromRoom();
				room.AddObject(destinationDbg);
			}
		}
		justSwitchedRooms.Reset();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		UpdateTimers();
		if (room == null)
		{
			return;
		}
		RoomSwitched();
		lastRoom = room.abstractRoom.index;
		if (debug && potentialTargetDbg == null)
		{
			FSprite fSprite = new FSprite("buttonSquareB");
			fSprite.scale = 2f;
			fSprite.color = Color.green;
			fSprite.alpha = 0.5f;
			potentialTargetDbg = new DebugSprite(Vector2.left * 1000f, fSprite, room);
			room.AddObject(potentialTargetDbg);
			fSprite = new FSprite("buttonCrossB");
			fSprite.scale = 1f;
			fSprite.color = Color.blue;
			fSprite.alpha = 0.5f;
			currentStepDbg = new DebugSprite(Vector2.left * 1000f, fSprite, room);
			room.AddObject(currentStepDbg);
			fSprite = new FSprite("FlowerMarker");
			fSprite.scale = 1f;
			fSprite.color = Color.red;
			fSprite.alpha = 0.5f;
			destinationDbg = new DebugSprite(Vector2.left * 1000f, fSprite, room);
			room.AddObject(destinationDbg);
		}
		base.gravity = 0.9f;
		if (lightSource.room == null)
		{
			room.AddObject(lightSource);
		}
		lightSource.color = BoxWormGraphics.BaseColor(base.abstractCreature.Room.realizedRoom);
		lightSource.setPos = base.mainBodyChunk.pos;
		if (!base.dead && graphicsData != null)
		{
			float b = 0f;
			float b2 = 0f;
			if (ai.behavior != FireSpriteAI.Behavior.Helping)
			{
				float val = Mathf.Clamp01(graphicsData.wingsOpen + graphicsData.speed);
				b2 = Custom.LerpMap(val, 0.5f, 0f, 100f, 200f);
				b = Custom.LerpMap(val, 0.5f, 0f, 0.5f, 1f);
			}
			if (enteringShortCut.HasValue)
			{
				b = 0f;
			}
			lightSource.setPos = base.mainBodyChunk.pos + Vector2.up * (40f * Mathf.Lerp(0.5f, 0f, graphicsData.wingsOpen));
			lightSource.setRad = Mathf.Lerp(lightSource.rad, b2, 0.2f);
			lightSource.setAlpha = Mathf.Lerp(lightSource.alpha, b, 0.2f);
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 4f;
			Stun(12);
			ai.currentRestingPoint = -1;
		}
		if (soundLoop != null)
		{
			if (base.stun > 0 || base.dead)
			{
				soundLoop.volume = 0f;
			}
			soundLoop.pos = Vector2.Lerp(soundLoop.pos, base.mainBodyChunk.pos, 9f);
			soundLoop.Update();
		}
		if (base.dead)
		{
			return;
		}
		SoundUpdate();
		if (base.stun <= 0)
		{
			ai.Update();
			StayInAir();
			if (enteringShortCut.HasValue || base.inShortcut)
			{
				shortcutCooldown.Reset();
			}
		}
		if (graphicsData != null)
		{
			graphicsData.Update();
		}
	}

	public void SoundUpdate()
	{
		bool flag = base.stun > 0;
		helpPing.Tick();
		if (ai.behavior != FireSpriteAI.Behavior.ActivateRoot)
		{
			helpPing.Reset();
		}
		else if ((int)helpPing == 1 && !flag)
		{
			helpPingSE = room.PlaySound(WatcherEnums.WatcherSoundID.Fire_Sprite_Chirp, base.firstChunk, loop: false, 0.9f, Random.Range(0.8f, 1f));
		}
		if (helpPingSE != null && helpPingSE.soundStillPlaying)
		{
			helpPingSE.pos = base.firstChunk.pos;
		}
		if (ai.behavior != FireSpriteAI.Behavior.Helping)
		{
			canPlayEntranceSound = true;
		}
		else if (canPlayEntranceSound)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Activate_Spin, base.firstChunk, loop: false, 0.4f, Random.Range(0.8f, 1f));
			canPlayEntranceSound = false;
		}
		vocalizeSoundCooldown.Tick();
		if (!flag && vocalizeSoundCooldown.isFinished && Random.value < 0.00083333335f && ai.behavior != FireSpriteAI.Behavior.Helping)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Fire_Sprite_Chirp, base.firstChunk, loop: false, 0.5f, Random.Range(0.5f, 0.9f));
			vocalizeSoundCooldown.SetMax(Random.Range(80, 200));
			vocalizeSoundCooldown.Reset();
		}
		if (boostSE != null)
		{
			if (boostSE.currentSoundObject != null && boostSE.currentSoundObject.audioSource != null && boostSE.currentSoundObject.audioSource.clip != null)
			{
				boostSE.volume = (0.3f + speedBoost * 0.3f) * Mathf.InverseLerp(0f, 8f, base.mainBodyChunk.vel.magnitude) * (1f - boostSE.currentSoundObject.audioSource.time / boostSE.currentSoundObject.audioSource.clip.length);
			}
			if (!boostSE.soundStillPlaying)
			{
				boostSE = null;
			}
		}
		if (evasionBoostSE != null)
		{
			if (evasionBoostSE.currentSoundObject != null && evasionBoostSE.currentSoundObject.audioSource != null && evasionBoostSE.currentSoundObject.audioSource.clip != null)
			{
				evasionBoostSE.volume = (0.3f + speedBoost * 0.3f) * (1f - evasionBoostSE.currentSoundObject.audioSource.time / evasionBoostSE.currentSoundObject.audioSource.clip.length);
			}
			if (!evasionBoostSE.soundStillPlaying)
			{
				evasionBoostSE = null;
			}
		}
		if (ai.collectingEnergy && !flag && Random.value > 0.96f)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Fire_Sprite_Chirp, base.firstChunk.pos + Custom.RNV() * (Random.value * 60f), Custom.LerpMap(wingOpenTimer.normalized, 0.5f, 0f, 0f, Random.Range(0.05f, 0.15f)), Random.Range(3f, 5f));
		}
		SoundID soundID = (ai.collectingEnergy ? WatcherEnums.WatcherSoundID.Fire_Sprite_Wing_Spread_Idle_LOOP : WatcherEnums.WatcherSoundID.Fire_Sprite_Flying_LOOP);
		if (soundLoop == null)
		{
			soundLoop = new StaticSoundLoop(soundID, base.mainBodyChunk.pos, room, 1f, 1f);
			soundLoopID = soundID;
		}
		if (soundID != soundLoopID)
		{
			soundLoopTransition.Tick();
			if ((int)soundLoopTransition > 10)
			{
				soundLoopTransition.Reset();
			}
			if ((int)soundLoopTransition == 10)
			{
				soundLoop = new StaticSoundLoop(soundID, base.mainBodyChunk.pos, room, 1f, 1f);
				soundLoopID = soundID;
				soundLoopTransition.Tick();
			}
		}
		else
		{
			if (soundLoopTransition.isFinished)
			{
				soundLoopTransition.Reset();
			}
			if ((int)soundLoopTransition != 0)
			{
				soundLoopTransition.Tick();
			}
		}
		float num = 0f;
		float num2 = Mathf.Abs(soundLoopTransition.normalized * 2f - 1f);
		if (soundLoopID == WatcherEnums.WatcherSoundID.Fire_Sprite_Wing_Spread_Idle_LOOP)
		{
			num = (flag ? 0f : Mathf.InverseLerp(0f, 2f, base.mainBodyChunk.vel.magnitude));
			soundLoop.volume = Mathf.Lerp(soundLoop.volume, (0.8f + num * 0.2f) * 0.5f * num2, 0.3f);
			soundLoop.pitch = Mathf.Lerp(soundLoop.pitch, 1f + num * 0.2f, 5f);
		}
		else if (soundLoopID == WatcherEnums.WatcherSoundID.Fire_Sprite_Flying_LOOP)
		{
			num = (flag ? 0f : Mathf.InverseLerp(0f, 15f, base.mainBodyChunk.vel.magnitude));
			soundLoop.volume = Mathf.Lerp(soundLoop.volume, num * 0.5f * num2, 0.3f);
			soundLoop.pitch = Mathf.Lerp(soundLoop.pitch, 1f + num * 0.4f, 2f);
		}
	}

	public bool SurroundingTilesArePassable(IntVector2 tilePos)
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			num += (room.GetTile(tilePos + Custom.eightDirections[i]).IsSolid() ? 1 : 0);
		}
		return num == 0;
	}

	public void FollowPath()
	{
		if (room == null || room.abstractRoom.index != base.abstractCreature.Room.index)
		{
			return;
		}
		stuckTimer.Tick();
		StandardPather standardPather = ai.pathFinder as StandardPather;
		MovementConnection movementConnection = standardPather.FollowPath(base.abstractCreature.pos, actuallyFollowingThisPath: true);
		Vector2? vector = null;
		if (base.safariControlled)
		{
			Player.InputPackage? inputPackage = inputWithDiagonals;
			if (inputPackage.HasValue)
			{
				Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
				if ((!(ai.behavior == FireSpriteAI.Behavior.Helping) || !divingToHelp) && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
				{
					vector = Vector2.zero;
					MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
					if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					{
						type = MovementConnection.MovementType.ShortCut;
					}
					if (valueOrDefault.analogueDir.magnitude > 0f)
					{
						vector = valueOrDefault.analogueDir.normalized;
					}
					else if (valueOrDefault.x != 0 || valueOrDefault.y != 0)
					{
						vector = new Vector2(valueOrDefault.x, valueOrDefault.y).normalized;
					}
					if (vector.HasValue)
					{
						Vector2 valueOrDefault2 = vector.GetValueOrDefault();
						movementConnection = ((!(valueOrDefault2 == Vector2.zero)) ? new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + valueOrDefault2 * 40f), 2) : default(MovementConnection));
					}
				}
			}
		}
		if (base.safariControlled)
		{
			ShortCutTravel(movementConnection);
		}
		else
		{
			ShortCutTravel(standardPather.FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false));
		}
		steps = 0;
		maxSteps = ((standardPather.creatureFollowingGeneration != standardPather.pathGeneration && standardPather.creatureFollowingGeneration > 2) ? 1 : 20);
		testConnection = standardPather.FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
		if (!ai.collectingEnergy && !base.safariControlled)
		{
			while (steps < maxSteps && SurroundingTilesArePassable(testConnection.DestTile) && Custom.WorldCoordFloatDist(testConnection.startCoord, standardPather.GetDestination) != 0f && room.RayTraceTilesForTerrain(ai.creature.pos.Tile.x, ai.creature.pos.Tile.y, testConnection.DestTile.x, testConnection.DestTile.y))
			{
				testConnection = standardPather.FollowPath(testConnection.destinationCoord, actuallyFollowingThisPath: false);
				steps++;
			}
		}
		base.airFriction = 0.9f + (float)steps / (float)maxSteps * Mathf.Clamp01(Mathf.InverseLerp(0f, 3f, room.aimap.getTerrainProximity(testConnection.destinationCoord))) * 0.08f;
		speedBoost = Mathf.InverseLerp(0.98f, 0.82f, base.airFriction);
		if (speedBoost <= 0.3f && base.mainBodyChunk.vel.magnitude > 1f)
		{
			if (boostSoundCooldown.isFinished)
			{
				boostSE = room.PlaySound(WatcherEnums.WatcherSoundID.Fire_Sprite_Flying_Boost, base.mainBodyChunk, loop: false, 0.5f + speedBoost * 0.5f, Random.Range(0.5f, 1f));
				boostSoundCooldown.Reset();
			}
		}
		else
		{
			boostSoundCooldown.Tick();
		}
		if (ai.collectingEnergy || ai.lastPos != base.abstractCreature.pos)
		{
			stuckTimer.Reset();
			stuckFrustration.Reset();
		}
		bool flag = room.aimap.getTerrainProximity(base.abstractCreature.pos) > 2;
		bool flag2 = false;
		Room obj = room;
		MovementConnection obj2 = (flag ? testConnection : movementConnection);
		Vector2 vector2 = obj.MiddleOfTile(obj2.destinationCoord);
		if (vector.HasValue)
		{
			Vector2 valueOrDefault3 = vector.GetValueOrDefault();
			if (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type))
			{
				vector2 = base.mainBodyChunk.pos + valueOrDefault3 * 40f;
			}
		}
		if (vector2 == Vector2.one * 10f)
		{
			if (standardPather.GetDestination.room != room.abstractRoom.index)
			{
				return;
			}
			vector2 = room.MiddleOfTile(standardPather.GetDestination);
			flag2 = true;
		}
		if (!room.BeingViewed || (IsFarFromCamera && !flag2))
		{
			if (vector2 == Vector2.one * 10f)
			{
				return;
			}
			if (ai.behavior == FireSpriteAI.Behavior.ActivateRoot)
			{
				base.mainBodyChunk.HardSetPosition(vector2);
			}
		}
		else
		{
			base.mainBodyChunk.vel += MoveToPoint(base.mainBodyChunk.pos, vector2, ai.collectingEnergy ? 0.1f : speed);
			base.mainBodyChunk.vel += StuckSpasm();
		}
		if (debug)
		{
			destinationDbg.pos = room.MiddleOfTile(standardPather.GetDestination);
			currentStepDbg.pos = vector2;
			potentialTargetDbg.pos = room.MiddleOfTile(testConnection.destinationCoord);
		}
	}

	public void OpenWings()
	{
		if (!ai.collectingEnergy)
		{
			if (wingOpenTimer.isFinished)
			{
				wingOpenTimer.Reset();
			}
			return;
		}
		if (!wingsOpen)
		{
			wingOpenTimer.Reset();
			wingsOpen = true;
		}
		wingOpenTimer.Tick();
	}

	public void ShortCutTravel(MovementConnection connection)
	{
		if ((!(ai.behavior != FireSpriteAI.Behavior.ActivateRoot) || shortcutCooldown.isFinished) && !ai.collectingEnergy && (connection.type == MovementConnection.MovementType.ShortCut || connection.type == MovementConnection.MovementType.BetweenRooms) && !(room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.CreatureHole))
		{
			enteringShortCut = connection.StartTile;
		}
	}

	public Vector2 StuckSpasm()
	{
		if (stuckTimer.isFinished)
		{
			stuckFrustration.Tick();
			return Custom.RNV() * (2f * stuckFrustration.normalized);
		}
		return Vector2.zero;
	}

	public override void Die()
	{
		base.Die();
		if (base.dead && canExplode)
		{
			Explosion();
			canExplode = false;
		}
		base.mainBodyChunk.collideWithTerrain = false;
		base.mainBodyChunk.vel = Vector2.down * 1000f;
	}

	public void Explosion()
	{
		if (room.ViewedByAnyCamera(base.mainBodyChunk.pos, 400f))
		{
			room.AddObject(new ShockWave(base.mainBodyChunk.pos, 120f, 0.1f, 10));
			room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Activate_Ping, base.mainBodyChunk.pos, 0.5f, Random.Range(3f, 4f));
			room.PlaySound(WatcherEnums.WatcherSoundID.Fire_Sprite_Chirp, base.mainBodyChunk.pos, 0.5f, Random.Range(1.5f, 2f));
			for (int i = 0; i < 20; i++)
			{
				room.AddObject(new Spark(base.mainBodyChunk.pos, Vector2.up + Custom.RNV() * (60f * Random.value), Color.Lerp(BoxWormGraphics.BaseColor(room), Color.white, Random.value), null, 30, 60));
			}
		}
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (RippleViolenceCheck(source) && !(ai.behavior == FireSpriteAI.Behavior.Helping))
		{
			if (!base.dead)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Fire_Sprite_Hit, base.mainBodyChunk);
			}
			base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
		}
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos onAppendagePos, Vector2 direction)
	{
		return false;
	}

	public void StayInAir()
	{
		base.gravity = 0f;
	}

	public Vector2 MoveToPoint(Vector2 from, Vector2 to, float maxMagnitude)
	{
		float t = Mathf.Clamp01(Vector2.Distance(from, to) / 500f);
		Vector2 a = Vector2.ClampMagnitude(to - from, maxMagnitude);
		if (ejectVector.magnitude > 0.1f)
		{
			a += ejectVector;
			ejectVector *= 0.9f;
		}
		lastDir = dir;
		dir = Mathf.LerpAngle(lastDir, Custom.VecToDeg(to - from), 0.1f);
		Vector2 b = Custom.DegToVec(dir) * Mathf.Clamp((to - from).magnitude, Mathf.Max(maxMagnitude, 10f), maxMagnitude);
		return Vector2.Lerp(a, b, t);
	}

	public void DiveToHelp(Vector2 helpeeEntrance, Vector2 helpeeEntranceVec)
	{
		divingToHelp = true;
		base.mainBodyChunk.pos = Custom.MoveTowards(base.mainBodyChunk.pos, helpeeEntrance, 8f);
	}

	public void FlyingWeapon(Weapon weapon)
	{
		if (base.stun > 0 || base.dead || base.safariControlled)
		{
			return;
		}
		if (!Custom.DistLess(base.mainBodyChunk.pos, weapon.firstChunk.pos + weapon.firstChunk.vel, 300f))
		{
			evasionVector = null;
			return;
		}
		if (!evasionVector.HasValue)
		{
			ai.creature.state.socialMemory.GetOrInitiateRelationship(weapon.thrownBy.abstractCreature.ID).fear += 0.3f;
			ai.uncomfortable.Reset();
			if (ai.behavior == FireSpriteAI.Behavior.Helping)
			{
				return;
			}
			Vector2 vector = Custom.PerpendicularVector(weapon.firstChunk.vel);
			Vector2 vector2 = -vector;
			if (room.aimap.getTerrainProximity(base.mainBodyChunk.pos + vector * 20f) > room.aimap.getTerrainProximity(base.mainBodyChunk.pos + vector2 * 20f))
			{
				evasionVector = vector;
			}
			else
			{
				evasionVector = vector2;
			}
			evasionBoostSE = room.PlaySound(WatcherEnums.WatcherSoundID.Fire_Sprite_Flying_Boost, base.mainBodyChunk, loop: false, 0.6f, Random.Range(0.5f, 1f));
			room.PlaySound(WatcherEnums.WatcherSoundID.Fire_Sprite_Chirp, base.mainBodyChunk.pos, 0.4f, Random.Range(0.4f, 0.6f));
		}
		base.mainBodyChunk.vel += Vector2.Lerp((base.mainBodyChunk.pos + evasionVector.Value * 0.5f - weapon.firstChunk.pos + weapon.firstChunk.vel).normalized * 3f, evasionVector.Value, Mathf.Clamp01(Custom.Dist(base.mainBodyChunk.pos, weapon.firstChunk.pos) / 200f)) * 5f;
	}
}
