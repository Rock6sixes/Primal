using System;
using System.Collections.Generic;
using RWCustom;
using SplashWater;
using UnityEngine;

namespace Watcher;

public class Barnacle : InsectoidCreature, IPlayerEdible
{
	public class CreatureParams
	{
		public float totalMass;

		public float sizeMultiplier;

		public float travelSpeedFactor;

		public float fleeSpeedFactor;

		public float idleSpeedFactor;

		public float travelSwimFactor;

		public float fleeSwimFactor;

		public float idleSwimFactor;

		public bool separateSwimBehavior;

		public float jumpOutOfWaterForce;

		public float legThickness;

		public Color darkShellColor;

		public Color lightShellColor;

		public float eyeSizeMultiplier;

		public int cones;

		public float coneRadVariance;

		public float coneLengthVariance;

		public float coneColorVariance;

		public float tipRadFactor;

		public CreatureParams(AbstractCreature creature)
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(creature.ID.RandomSeed);
			AbstractCreature.Personality personality = creature.personality;
			totalMass = 3.6f;
			sizeMultiplier = Mathf.Lerp(personality.dominance, UnityEngine.Random.value, UnityEngine.Random.value) * 0.4f + 0.6f;
			travelSpeedFactor = 0.5f;
			fleeSpeedFactor = 1.25f;
			idleSpeedFactor = 0.5f;
			travelSwimFactor = 0.3f;
			fleeSwimFactor = 0.6f;
			idleSwimFactor = 0.3f;
			separateSwimBehavior = true;
			jumpOutOfWaterForce = 1f;
			eyeSizeMultiplier = UnityEngine.Random.Range(0.7f, 1f);
			cones = UnityEngine.Random.Range(13, 20);
			coneRadVariance = UnityEngine.Random.value;
			coneLengthVariance = Custom.PushFromHalf(UnityEngine.Random.value, 1.5f);
			coneColorVariance = Mathf.Pow(UnityEngine.Random.value, 0.5f);
			legThickness = Mathf.Lerp(0.7f, 1.3f, Custom.PushFromHalf(UnityEngine.Random.value, 0.7f));
			tipRadFactor = 0.46153846f;
			HSLColor hSLColor;
			HSLColor hSLColor2 = (hSLColor = new HSLColor(Custom.WrappedRandomVariation(0.04f, 0.1f, 0.1f), Custom.ClampedRandomVariation(0.5f, 0.2f, 0.3f), Custom.ClampedRandomVariation(0.75f, 0.2f, 1.5f)));
			hSLColor.lightness = Mathf.Clamp01(hSLColor.lightness - UnityEngine.Random.Range(0.1f, 0.2f));
			HSLColor hSLColor3 = hSLColor2;
			hSLColor3.lightness = Mathf.Clamp01(hSLColor3.lightness + UnityEngine.Random.Range(0f, 0.1f));
			darkShellColor = hSLColor.rgb;
			lightShellColor = hSLColor3.rgb;
			UnityEngine.Random.state = state;
		}
	}

	public BarnacleAI AI;

	public CreatureParams creatureParams;

	public WaterJet waterJet;

	public int temporaryDamageImmunity;

	public Vector2 swimDir;

	public float swimSpeed;

	public float surfSpeed;

	public float slowDownForPrecision;

	public int delayBeforeJumping;

	public Vector2 jumpTarget;

	public int actCounter;

	public int moveCounter;

	public int wetCounter;

	public int timeSinceLastWetSound;

	public int shakeTime;

	public int shakeCooldown;

	public int clinkSoundCooldown;

	public int timeSinceLastSkitterSound;

	public Vector2 recentPosition;

	public int footingCounter;

	public int specialMoveCounter;

	public IntVector2 specialMoveDestination;

	public MovementConnection lastFollowedConnection;

	public MovementConnection followingConnection;

	public int bites = 5;

	public bool hasShell
	{
		get
		{
			return base.State.miscWatcherSaveFlags == 0;
		}
		set
		{
			base.State.miscWatcherSaveFlags = ((!value) ? 1 : 0);
		}
	}

	public bool Footing => footingCounter > 20;

	public bool Swimming => base.Submersion > 0.3f;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => !hasShell;

	public bool AutomaticPickUp => false;

	public void GenerateIVars()
	{
		creatureParams = new CreatureParams(base.abstractCreature);
	}

	public Barnacle(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 24f * creatureParams.sizeMultiplier, creatureParams.totalMass / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 10f * creatureParams.sizeMultiplier, creatureParams.totalMass / 2f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 12f * creatureParams.sizeMultiplier, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		recentPosition = new Vector2(base.mainBodyChunk.pos.x, base.mainBodyChunk.pos.y);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.buoyancy = 0.99f;
		currentAffectiveness = 0.1f;
		if (!hasShell)
		{
			LoseShell();
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new BarnacleGraphics(this);
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
		if (base.dead)
		{
			AI.AbandonLeader();
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].terrainSqueeze = 8f / base.bodyChunks[i].rad;
		}
		temporaryDamageImmunity--;
		clinkSoundCooldown--;
		if (base.Consious)
		{
			if (room.aimap.TileAccessibleToCreature(base.bodyChunks[0].pos, base.Template) || room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
			{
				footingCounter++;
			}
			else
			{
				footingCounter = 0;
			}
			Act();
		}
		else
		{
			footingCounter = 0;
			base.waterFriction = 0.95f;
			waterRetardationImmunity = 0f;
		}
		if (Footing)
		{
			for (int j = 0; j < 2; j++)
			{
				if (!Swimming)
				{
					base.bodyChunks[j].vel *= 0.8f;
				}
				base.bodyChunks[j].vel.y += base.gravity * (1f - base.Submersion);
			}
		}
		if (!base.Consious || Footing || !(AI.behavior == BarnacleAI.Behavior.Flee) || AI.scattering || base.safariControlled)
		{
			return;
		}
		for (int k = 0; k < 2; k++)
		{
			if (room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[k].pos), base.Template))
			{
				base.bodyChunks[k].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * (UnityEngine.Random.value * 5f);
			}
		}
	}

	public void Swim()
	{
		float num = 0f;
		if (base.Submersion > 0f && base.Submersion < 1f)
		{
			num = Mathf.Abs((base.bodyChunks[0].pos - base.bodyChunks[1].pos).normalized.y);
		}
		if (surfSpeed < num)
		{
			surfSpeed = Mathf.Min(surfSpeed + 0.05f, num);
		}
		else
		{
			surfSpeed = Mathf.Max(surfSpeed - 0.0125f, num);
		}
		base.waterFriction = 0.995f;
		waterRetardationImmunity = 0.8f;
		surfSpeed *= 0.9f;
		AI.Update();
		if (AI.behavior == BarnacleAI.Behavior.GetUnstuck && base.Submersion > 0f)
		{
			float num2 = Mathf.InverseLerp(0.5f, 1f, AI.stuckTracker.Utility());
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel += Custom.RNV() * (UnityEngine.Random.value * 10f * num2);
				base.bodyChunks[i].terrainSqueeze = Mathf.Lerp(1f, 0.1f, num2);
			}
		}
		followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		if (base.safariControlled)
		{
			SafariControls(ref followingConnection);
		}
		if (followingConnection != default(MovementConnection) && (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation))
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
			return;
		}
		if (!base.safariControlled)
		{
			if (AI.floatGoalPos.HasValue && AI.pathFinder.GetDestination.TileDefined && AI.pathFinder.GetDestination.room == room.abstractRoom.index && room.VisualContact(base.mainBodyChunk.pos, AI.floatGoalPos.Value))
			{
				swimDir = Custom.DirVec(base.mainBodyChunk.pos, AI.floatGoalPos.Value);
			}
			else if (followingConnection != default(MovementConnection))
			{
				swimDir = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(followingConnection.DestTile));
				WorldCoordinate destinationCoord = followingConnection.destinationCoord;
				for (int k = 0; k < 4; k++)
				{
					MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(destinationCoord, actuallyFollowingThisPath: false);
					if (movementConnection != default(MovementConnection) && movementConnection.destinationCoord.TileDefined && movementConnection.destinationCoord.room == room.abstractRoom.index && room.VisualContact(followingConnection.destinationCoord.Tile, movementConnection.DestTile))
					{
						swimDir += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.DestTile));
						destinationCoord = movementConnection.destinationCoord;
						if (room.aimap.getAItile(movementConnection.DestTile).narrowSpace)
						{
							slowDownForPrecision += 0.3f;
							break;
						}
					}
				}
				swimDir = swimDir.normalized;
			}
		}
		if (base.safariControlled)
		{
			Player.InputPackage? inputPackage = inputWithDiagonals;
			if (inputPackage.HasValue)
			{
				Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
				if (valueOrDefault.analogueDir.magnitude > 0f)
				{
					swimDir = valueOrDefault.analogueDir.normalized;
				}
				else if (valueOrDefault.x != 0 || valueOrDefault.y != 0)
				{
					swimDir = new Vector2(valueOrDefault.x, valueOrDefault.y).normalized;
				}
			}
		}
		slowDownForPrecision = Mathf.Clamp(slowDownForPrecision - 0.1f, 0f, 1f);
		if ((room.aimap.getAItile(base.mainBodyChunk.pos).narrowSpace || room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance) && room.GetTile(base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos) * 20f).Terrain != 0)
		{
			MovementConnection movementConnection2 = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
			if (base.safariControlled && (movementConnection2 == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection2.type)))
			{
				movementConnection2 = default(MovementConnection);
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0)
				{
					movementConnection2 = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
				}
			}
			bool flag2 = false;
			if (movementConnection2 == default(MovementConnection))
			{
				flag2 = true;
				movementConnection2 = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
			}
			if (movementConnection2 != default(MovementConnection) && movementConnection2.destinationCoord.TileDefined)
			{
				base.mainBodyChunk.vel += Custom.DirVec(base.bodyChunks[flag2 ? 1 : 0].pos, room.MiddleOfTile(movementConnection2.DestTile)) * 1.8f;
				base.bodyChunks[1].vel += Custom.RNV() * (2f * UnityEngine.Random.value) + Custom.DirVec(base.mainBodyChunk.pos, base.bodyChunks[1].pos) * UnityEngine.Random.value;
			}
			base.mainBodyChunk.terrainSqueeze = 0.1f;
			base.bodyChunks[1].terrainSqueeze = 0.1f;
			return;
		}
		float num3 = creatureParams.idleSwimFactor;
		if ((AI.behavior == BarnacleAI.Behavior.Flee && !AI.scattering) || AI.behavior == BarnacleAI.Behavior.EscapeRain)
		{
			num3 = creatureParams.fleeSwimFactor;
		}
		else if (AI.behavior == BarnacleAI.Behavior.Travelling || AI.behavior == BarnacleAI.Behavior.InvestigateSound)
		{
			num3 = creatureParams.travelSwimFactor;
		}
		swimSpeed = Mathf.Lerp(2.2f * (1f + surfSpeed * 0.5f), 1.4f, slowDownForPrecision) * 0.85f;
		if (shakeTime > 0)
		{
			num3 = 0f;
		}
		if (swimDir.magnitude > 0f)
		{
			swimDir = Vector3.Slerp(swimDir.normalized, Custom.RNV(), 0.2f);
		}
		else
		{
			swimDir = Vector3.Slerp(Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos), Custom.RNV(), 0.3f);
		}
		if (creatureParams.jumpOutOfWaterForce <= 0f && base.mainBodyChunk.submersion < base.bodyChunks[1].submersion && swimDir.y > 0f)
		{
			swimDir.y *= -1f;
			base.mainBodyChunk.vel.y -= 4f * base.bodyChunks[1].submersion;
		}
		Vector2 vector = new Vector2((Mathf.Abs(swimDir.x) > Mathf.Abs(swimDir.y)) ? (1f * Mathf.Sign(swimDir.x)) : 0f, (Mathf.Abs(swimDir.y) >= Mathf.Abs(swimDir.x)) ? (1f * Mathf.Sign(swimDir.y)) : 0f);
		base.mainBodyChunk.vel += vector * (swimSpeed * Mathf.Pow(base.bodyChunks[1].submersion, 0.2f) * num3);
		base.bodyChunks[1].vel *= Mathf.Lerp(1f, 0.8f, Mathf.Pow(base.bodyChunks[1].submersion, 0.2f));
		Vector2 vector2 = vector * (((grabbedBy.Count == 0) ? 0.7f : 1f) * 2.5f * num3);
		base.mainBodyChunk.vel += vector2;
		base.bodyChunks[1].vel -= vector2;
		if (creatureParams.jumpOutOfWaterForce > 0f && base.mainBodyChunk.submersion < base.bodyChunks[1].submersion && vector.y > 0f)
		{
			base.mainBodyChunk.vel.y += creatureParams.jumpOutOfWaterForce * base.bodyChunks[1].submersion;
		}
		if (swimSpeed > 1.5f && UnityEngine.Random.value < 0.5f)
		{
			room.AddObject(new Bubble(base.bodyChunks[1].pos, -36f * Mathf.Lerp(0f, 1f, Mathf.InverseLerp(1.5f, 2.5f, swimSpeed)) * vector, bottomBubble: false, fakeWaterBubble: false));
		}
		swimDir *= 0f;
	}

	public void Act()
	{
		actCounter++;
		if (actCounter % 200 == 0)
		{
			if (AI.leading.Count == 0 && Vector2.Distance(base.mainBodyChunk.pos, recentPosition) <= 20f && (double)AI.runSpeed > 0.5 && !AI.idleSplash)
			{
				AI.temporaryPanic = 200;
				AI.ShufflePanicSpot(scattering: true);
			}
			recentPosition = new Vector2(base.mainBodyChunk.pos.x, base.mainBodyChunk.pos.y);
		}
		if (base.Submersion > 0f)
		{
			wetCounter = 4800;
			timeSinceLastWetSound++;
			if (timeSinceLastWetSound > 40 && UnityEngine.Random.value < 0.0125f)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Suck_Water, base.mainBodyChunk);
				timeSinceLastWetSound = 0;
			}
		}
		else
		{
			timeSinceLastWetSound = 0;
		}
		shakeTime--;
		shakeCooldown--;
		if (delayBeforeJumping > 0)
		{
			delayBeforeJumping--;
			if (delayBeforeJumping == 0)
			{
				TryJump(jumpTarget);
			}
		}
		if (hasShell)
		{
			Vector2? vector = null;
			for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
			{
				if (room.abstractRoom.creatures[i].realizedCreature == null)
				{
					continue;
				}
				Creature realizedCreature = room.abstractRoom.creatures[i].realizedCreature;
				if (realizedCreature is Player && (realizedCreature as Player).outOfCamoTime == 80)
				{
					AI.SetGroupDiscomfortTick(1.1f);
				}
				else
				{
					if (realizedCreature is Barnacle || realizedCreature is Overseer)
					{
						continue;
					}
					float num = Vector2.Distance(base.mainBodyChunk.pos, realizedCreature.mainBodyChunk.pos);
					if (num <= 150f)
					{
						if (realizedCreature is Player)
						{
							(realizedCreature as Player).airInLungs = 1f;
							vector = realizedCreature.mainBodyChunk.pos;
						}
						else if (realizedCreature is AirBreatherCreature)
						{
							(realizedCreature as AirBreatherCreature).lungs = Mathf.Min(1f, (realizedCreature as AirBreatherCreature).lungs + 1f / 21f);
							if (!vector.HasValue)
							{
								vector = realizedCreature.mainBodyChunk.pos;
							}
						}
						else if (realizedCreature is Leech && !realizedCreature.dead && num <= 100f)
						{
							float num2 = Mathf.InverseLerp(100f, 60f, Vector2.Distance(base.mainBodyChunk.pos, realizedCreature.mainBodyChunk.pos)) * realizedCreature.mainBodyChunk.submersion;
							if (UnityEngine.Random.value < 0.007f * num2)
							{
								realizedCreature.Stun(16);
							}
							if (realizedCreature.Consious && realizedCreature.grasps[0] == null)
							{
								realizedCreature.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, realizedCreature.mainBodyChunk.pos) * (num2 * UnityEngine.Random.value * 12f);
							}
						}
					}
					if (num <= 200f && AI.groupDiscomfort < Mathf.InverseLerp(400f, 70f, num) && !realizedCreature.dead && room.VisualContact(base.mainBodyChunk.pos, realizedCreature.mainBodyChunk.pos) && (!(realizedCreature is Player) || !(realizedCreature as Player).IsHidden) && AI.runAwayTime <= 0)
					{
						AI.SetGroupDiscomfortTick(0.0016666667f);
					}
				}
			}
			if (vector.HasValue)
			{
				if (UnityEngine.Random.value < 0.35f)
				{
					room.AddObject(new Bubble(Vector2.Lerp(vector.Value, base.mainBodyChunk.pos, 0.5f), 10f * Custom.RNV(), bottomBubble: false, fakeWaterBubble: false));
				}
				if (UnityEngine.Random.value < 0.35f)
				{
					room.AddObject(new Bubble(vector.Value, 10f * Custom.RNV(), bottomBubble: false, fakeWaterBubble: false));
				}
			}
		}
		if (AI.idleSplash && wetCounter > 0)
		{
			if (waterJet == null)
			{
				waterJet = new WaterJet(room);
			}
			else if (waterJet.Dead)
			{
				waterJet = null;
			}
			else if (UnityEngine.Random.value < 0.015f)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Spit_Water, base.mainBodyChunk);
				waterJet.NewParticle(base.mainBodyChunk.lastPos + Custom.RNV() * base.mainBodyChunk.rad, Custom.RNV() * (UnityEngine.Random.Range(2f, 10f) * UnityEngine.Random.value), UnityEngine.Random.Range(10f, 30f), UnityEngine.Random.Range(0.5f, 5f));
			}
		}
		if (waterJet != null)
		{
			waterJet.Update();
		}
		if (Swimming && creatureParams.separateSwimBehavior)
		{
			Swim();
			return;
		}
		base.waterFriction = 0.95f;
		waterRetardationImmunity = 0f;
		AI.Update();
		timeSinceLastSkitterSound++;
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
		followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		if (followingConnection == default(MovementConnection))
		{
			followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[0].pos), actuallyFollowingThisPath: true);
		}
		if (followingConnection.startCoord == lastFollowedConnection.startCoord)
		{
			followingConnection = lastFollowedConnection;
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
				Vector2 vector2 = room.MiddleOfTile(followingConnection.DestTile);
				if (lastFollowedConnection != default(MovementConnection) && lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
				{
					base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector2) * 4f;
				}
				if (Footing)
				{
					for (int k = 0; k < 2; k++)
					{
						if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
						{
							base.bodyChunks[k].vel.x += Mathf.Min((vector2.x - base.bodyChunks[k].pos.x) / 8f, 1.2f);
						}
						else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
						{
							base.bodyChunks[k].vel.y += Mathf.Min((vector2.y - base.bodyChunks[k].pos.y) / 8f, 1.2f);
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
				MoveTowards(vector2);
			}
			lastFollowedConnection = followingConnection;
		}
		else
		{
			base.GoThroughFloors = false;
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
		float num = 1f / 160f;
		float num2 = creatureParams.idleSpeedFactor;
		if ((AI.behavior == BarnacleAI.Behavior.Flee && !AI.scattering) || AI.behavior == BarnacleAI.Behavior.EscapeRain)
		{
			num2 = creatureParams.fleeSpeedFactor;
			num = 0.025f;
		}
		else if (AI.behavior == BarnacleAI.Behavior.Travelling || AI.behavior == BarnacleAI.Behavior.InvestigateSound)
		{
			num2 = creatureParams.travelSpeedFactor;
		}
		if (shakeTime > 0)
		{
			num2 = 0f;
		}
		if (num2 == 0f || AI.runSpeed == 0f)
		{
			num = 0f;
		}
		base.mainBodyChunk.vel += vector * (4.2f * AI.runSpeed * num2);
		base.bodyChunks[1].vel -= vector * (AI.runSpeed * num2);
		base.GoThroughFloors = moveTo.y < base.mainBodyChunk.pos.y - 5f;
		if (AI.runSpeed > 0f && num2 > 0f)
		{
			moveCounter++;
		}
		if (!Swimming && timeSinceLastSkitterSound > 20 && UnityEngine.Random.value < num)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Legs_Skitter, base.mainBodyChunk);
			timeSinceLastSkitterSound = 0;
		}
		else if (!Swimming && moveCounter % 5 == 0 && AI.runSpeed > 0f && num2 > 0f)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Step, base.mainBodyChunk);
		}
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

	public void TryJump(Vector2 towardsPoint)
	{
		delayBeforeJumping = 0;
		if (base.Consious)
		{
			Vector2 vector = Custom.DirVec((base.bodyChunks[0].pos + base.bodyChunks[1].pos) / 2f, towardsPoint);
			vector += Custom.RNV() * 0.3f;
			vector.Normalize();
			vector = Vector3.Slerp(vector, new Vector2(0f, 1f), Custom.LerpMap(vector.y, -0.5f, 0.5f, 0.7f, 0.3f));
			base.bodyChunks[0].vel *= 0.5f;
			base.bodyChunks[1].vel *= 0.5f;
			base.bodyChunks[0].vel += vector * 14f + Custom.RNV() * (3f * UnityEngine.Random.value);
			base.bodyChunks[1].vel += vector * 14f + Custom.RNV() * (3f * UnityEngine.Random.value);
			footingCounter = 0;
			Vector2 vector2 = Custom.PerpendicularVector(vector) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			base.bodyChunks[0].vel += vector2 * 9f;
			base.bodyChunks[1].vel -= vector2 * 9f;
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!base.Consious)
		{
			return;
		}
		if (otherObject is Barnacle barnacle)
		{
			if (barnacle.AI.runSpeed > 0f || AI.runSpeed > 0f)
			{
				for (int i = 0; i < base.bodyChunks.Length; i++)
				{
					if (barnacle.abstractCreature.personality.dominance > base.abstractCreature.personality.dominance)
					{
						if (base.bodyChunks[i].vel.y > -3f)
						{
							base.bodyChunks[i].vel.y -= 0.5f;
						}
					}
					else if (base.bodyChunks[i].vel.y < 5f)
					{
						base.bodyChunks[i].vel.y += 2.5f;
					}
				}
			}
			if ((barnacle.mainBodyChunk.vel.magnitude >= 3f || base.mainBodyChunk.vel.magnitude >= 3f) && clinkSoundCooldown <= 0)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Shell_Clink, base.mainBodyChunk);
				clinkSoundCooldown = UnityEngine.Random.Range(5, 40);
			}
		}
		else if (base.Submersion == 0f && hasShell && otherObject is Creature && (otherObject as Creature).Consious && shakeCooldown <= 0 && (AI.behavior == BarnacleAI.Behavior.Idle || AI.behavior == BarnacleAI.Behavior.Travelling))
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Push_Away_Nearby_Creatures, base.mainBodyChunk);
			shakeTime = UnityEngine.Random.Range(10, 20);
			shakeCooldown = 160;
			Vector2 vector = Vector2.Lerp(otherObject.bodyChunks[otherChunk].pos, base.bodyChunks[myChunk].pos, 0.5f);
			(otherObject as Creature).Stun(30);
			for (int j = 0; j < otherObject.bodyChunks.Length; j++)
			{
				otherObject.bodyChunks[j].vel = Custom.DirVec(base.mainBodyChunk.pos, otherObject.bodyChunks[j].pos) * UnityEngine.Random.Range(3f, 8f);
			}
			for (int k = 0; k < 5; k++)
			{
				room.AddObject(new Spark(vector + Custom.RNV() * 10f, otherObject.bodyChunks[otherChunk].vel * -0.1f + Custom.DegToVec(UnityEngine.Random.value * 360f) * (Mathf.Lerp(0.2f, 0.4f, UnityEngine.Random.value) * otherObject.bodyChunks[otherChunk].vel.magnitude), new Color(1f, 1f, 1f), null, 20, 80));
			}
			AI.SetGroupDiscomfortTick(0.25f);
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

	public void LoseShell()
	{
		if (!hasShell && room != null)
		{
			return;
		}
		hasShell = false;
		temporaryDamageImmunity = 20;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].mass /= 8f;
			base.bodyChunks[i].rad *= ((i == 0) ? 0.4f : 0.75f);
		}
		bodyChunkConnections[0].distance *= 0.75f;
		AI?.OnLoseShell();
		if (room == null)
		{
			return;
		}
		for (int j = 0; (float)j < Mathf.Lerp(1f, 3f, creatureParams.sizeMultiplier); j++)
		{
			AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, room.GetWorldCoordinate(base.mainBodyChunk.pos + Custom.RNV() * base.mainBodyChunk.rad), room.game.GetNewID());
			room.abstractRoom.entities.Add(abstractPhysicalObject);
			abstractPhysicalObject.RealizeInRoom();
			abstractPhysicalObject.realizedObject.firstChunk.vel = Custom.RNV() * UnityEngine.Random.Range(8f, 16f);
		}
		if (base.graphicsModule != null)
		{
			for (int k = 0; (float)k < Mathf.Lerp(4f, 7f, creatureParams.sizeMultiplier); k++)
			{
				Vector2 vector = Custom.RNV() * 4f;
				vector = new Vector2(vector.x, Mathf.Abs(vector.y));
				int num = UnityEngine.Random.Range(0, 4);
				string overrideSprite = "RootBall1";
				switch (num)
				{
				case 1:
					overrideSprite = "KrakenShield0";
					break;
				case 2:
					overrideSprite = "Cicada5body";
					break;
				case 3:
					overrideSprite = "Cicada1body";
					break;
				}
				Color shellColor = (base.graphicsModule as BarnacleGraphics).GetShellColor(UnityEngine.Random.value);
				CentipedeShell centipedeShell = new CentipedeShell(base.mainBodyChunk.pos + Custom.RNV() * base.mainBodyChunk.rad, vector, shellColor, creatureParams.sizeMultiplier * 1.5f, creatureParams.sizeMultiplier * 1.5f, overrideSprite);
				centipedeShell.impactSound = WatcherEnums.WatcherSoundID.Barnacle_Shell_Fragment_Hit_Terrain;
				room.AddObject(centipedeShell);
			}
		}
		room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Shell_Explode, base.mainBodyChunk);
		room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Shell_Crack, base.mainBodyChunk);
	}

	public override Color ShortCutColor()
	{
		return base.ShortCutColor();
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (!RippleViolenceCheck(source))
		{
			return;
		}
		if (type == DamageType.Explosion && damage >= 0.05f)
		{
			LoseShell();
		}
		if (hasShell || temporaryDamageImmunity > 0)
		{
			float num = damage / base.Template.baseDamageResistance;
			int num2 = (int)Math.Min(Math.Max((damage * 30f + stunBonus) / base.Template.baseStunResistance / 2f, (int)(num * 30f)), 25f);
			BodyChunk bodyChunk = source;
			if (bodyChunk == null)
			{
				bodyChunk = hitChunk;
			}
			for (int i = 0; i < num2; i++)
			{
				room.AddObject(new Spark(bodyChunk.pos + Custom.DegToVec(UnityEngine.Random.value * 360f) * (5f * UnityEngine.Random.value), bodyChunk.vel * -0.1f + Custom.DegToVec(UnityEngine.Random.value * 360f) * (Mathf.Lerp(0.2f, 0.4f, UnityEngine.Random.value) * bodyChunk.vel.magnitude), new Color(1f, 1f, 1f), null, 10, 170));
			}
			damage = 0f;
			type = DamageType.Blunt;
			stunBonus = 0f;
			AI.SetGroupDiscomfortTick(0.65f);
			AI.temporaryPanic = UnityEngine.Random.Range(400, 1800);
			AI.ShufflePanicSpot(scattering: false);
		}
		base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos onAppendagePos, Vector2 direction)
	{
		if (chunk == null || hasShell)
		{
			if (hasShell)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Shell_Deflect_Projectile, base.mainBodyChunk);
			}
			return false;
		}
		return true;
	}

	public override void Stun(int st)
	{
		base.Stun(st);
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
		waterJet = null;
	}

	public void BitByPlayer(Grasp grasp, bool eu)
	{
		bites--;
		Die();
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
	}

	public void ThrowByPlayer()
	{
	}
}
