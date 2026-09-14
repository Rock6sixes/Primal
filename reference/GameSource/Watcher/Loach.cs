using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class Loach : AirBreatherCreature, PhysicalObject.IHaveAppendages, DaddyLongLegs.IHaveRotParts
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

		public float dominance;

		public CreatureParams()
		{
			totalMass = 20f;
			sizeMultiplier = 1f;
			huntSpeedFactor = 1f;
			travelSpeedFactor = 1f;
			fleeSpeedFactor = 1f;
			idleSpeedFactor = 1f;
			stunOnCollideVelocity = 0f;
			stunOnCollideDuration = 10f;
			stunOnCollideDamage = 0.1f;
			dominance = UnityEngine.Random.value;
		}
	}

	public class EatObject
	{
		public BodyChunk chunk;

		public float distance;

		public float progression;

		public EatObject(BodyChunk chunk, float distance)
		{
			this.chunk = chunk;
			this.distance = distance;
			progression = 0f;
		}
	}

	public LoachTentacle[] tentacle;

	public LoachLeg[] legs;

	public float resting;

	public float preferredHeight;

	public float sacLiftFactor;

	public float headLiftFactor;

	public int spasmChunk;

	public Vector2 spasmForce;

	public int vocalizeSoundCooldown;

	public Color effectColor;

	public Color eyeColor;

	public bool stayStill;

	public bool heldBackByLeg;

	public Vector2 bodDir;

	public float nextFloorHeight;

	public float legInFrontMargin = 30f;

	public int maxPreferredHeight = 12;

	public int midIdealHeight = 8;

	public int minPreferredHeight = 7;

	public float lastControlX;

	public Vector2? controlLimit;

	public InputCircularRegion controlRegion;

	public List<CreatureTemplate.Type> restrictedCreatureTemplates = new List<CreatureTemplate.Type>
	{
		WatcherEnums.CreatureTemplateType.Loach,
		WatcherEnums.CreatureTemplateType.RotLoach
	};

	public LoachAI AI;

	public CreatureParams creatureParams;

	public float rootRad = 12f;

	public float tipRad = 2f;

	public Vector2 moveDirection;

	public float flipDir;

	public int legsGrabbing;

	public int hesistCounter;

	public bool lurching;

	public bool lurchSound;

	public int tentacleCD;

	public int tentacleMoveIndex;

	public int debugInt;

	public float enterRoomForcePush;

	public int randomStumbleCounter = 500;

	public Vector2? slimeMoldGrabPos;

	public SlimeMold slimeMoldToGrab;

	public int lurchProgress;

	public float heightLerp = 1f;

	public List<EatObject> eatObjects;

	public int digestingCounter;

	public WorldCoordinate lastWC = new WorldCoordinate(-1, -1, -1, -1);

	public int writheCounter;

	public int writheMax = 250;

	public bool writhing;

	public int writheTimer;

	public bool floorDetected;

	public Vector2 HeadDir => Vector3.Slerp(Custom.PerpendicularVector(bodDir) * Mathf.Sign(bodDir.x), new Vector2(0f, 1f), Mathf.Pow(Mathf.InverseLerp(1f, 0f, Mathf.Abs(bodDir.x)), 0.5f));

	public bool Rotted => base.Template.type == WatcherEnums.CreatureTemplateType.RotLoach;

	public override Vector2 VisionPoint => base.bodyChunks[0].pos;

	public float tentacleLength
	{
		get
		{
			if (!Rotted)
			{
				return 150f;
			}
			return 250f;
		}
	}

	public new LoachState State => base.abstractCreature.state as LoachState;

	public float CloseToEdge => Mathf.Max(Mathf.InverseLerp(500f, 300f, base.mainBodyChunk.pos.x), Mathf.InverseLerp(room.PixelWidth - 500f, room.PixelWidth - 300f, base.mainBodyChunk.pos.x));

	public override bool SandstormImmune => true;

	public bool WantToSitDownAtDestination
	{
		get
		{
			if (AI.behavior == LoachAI.Behavior.Idle && AI.pathFinder.GetDestination.room == room.abstractRoom.index)
			{
				return Climbable(AI.pathFinder.GetDestination.Tile);
			}
			return false;
		}
	}

	public bool AtSitDestination
	{
		get
		{
			if (WantToSitDownAtDestination && Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetDestination) < 2)
			{
				return Climbable(AI.pathFinder.GetDestination.Tile);
			}
			return false;
		}
	}

	public float GetUnstuckForce
	{
		get
		{
			if (base.safariControlled)
			{
				return 0f;
			}
			return AI.stuckTracker.Utility();
		}
	}

	public bool isLurching => lurchProgress > 8;

	public float MostDigestedEatObject
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < eatObjects.Count; i++)
			{
				num = Mathf.Max(num, eatObjects[i].progression);
			}
			return num;
		}
	}

	DaddyGraphics.IHaveRotGraphics DaddyLongLegs.IHaveRotParts.RotGraphicsOwner => base.graphicsModule as DaddyGraphics.IHaveRotGraphics;

	DaddyTentacle[] DaddyLongLegs.IHaveRotParts.RotTentacles => Array.Empty<DaddyTentacle>();

	ArtificialIntelligence DaddyLongLegs.IHaveRotParts.RotAI => AI;

	Vector2 DaddyLongLegs.IHaveRotParts.RotMoveDirection => moveDirection;

	bool DaddyLongLegs.IHaveRotParts.RotSizeClass => true;

	Color DaddyLongLegs.IHaveRotParts.RotEyeColor => eyeColor;

	int DaddyLongLegs.IHaveRotParts.RotEyesClose => base.stun;

	bool DaddyLongLegs.IHaveRotParts.RotDigesting => digestingCounter > 0;

	int DaddyLongLegs.IHaveRotParts.RotStuckCounter => AI.stuckTracker.stuckCounter;

	public bool CanEatCreature(AbstractCreature creature)
	{
		if (restrictedCreatureTemplates.Contains(creature.creatureTemplate.type))
		{
			return false;
		}
		return AI.DynamicRelationship(creature).GoForKill;
	}

	public void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		creatureParams = new CreatureParams();
		UnityEngine.Random.state = state;
	}

	public Loach(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		base.bodyChunks = new BodyChunk[6];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 20f * creatureParams.sizeMultiplier, 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 30f * creatureParams.sizeMultiplier, 3f);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 40f * creatureParams.sizeMultiplier, 4f);
		base.bodyChunks[3] = new BodyChunk(this, 3, new Vector2(0f, 0f), 45f * creatureParams.sizeMultiplier, 4.5f);
		base.bodyChunks[4] = new BodyChunk(this, 4, new Vector2(0f, 0f), 55f * creatureParams.sizeMultiplier, 5.5f);
		base.bodyChunks[5] = new BodyChunk(this, 5, new Vector2(0f, 0f), 50f * creatureParams.sizeMultiplier, 5f);
		bodyChunkConnections = new BodyChunkConnection[5];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 50f, BodyChunkConnection.Type.Pull, 0.7f, -1f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 48f, BodyChunkConnection.Type.Pull, 0.85f, -1f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[3], 48f, BodyChunkConnection.Type.Pull, 1f, -1f);
		bodyChunkConnections[3] = new BodyChunkConnection(base.bodyChunks[3], base.bodyChunks[4], 30f, BodyChunkConnection.Type.Normal, 0.85f, -1f);
		bodyChunkConnections[4] = new BodyChunkConnection(base.bodyChunks[4], base.bodyChunks[5], 20f, BodyChunkConnection.Type.Normal, 0.7f, -1f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		collisionRange = 250f;
		base.CollideWithSlopes = false;
		windAffectiveness = 0.25f;
		abstractCreature.tentacleImmune = true;
		flipDir = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		tentacle = new LoachTentacle[5];
		appendages = new List<Appendage>();
		for (int i = 0; i < tentacle.Length; i++)
		{
			tentacle[i] = new LoachTentacle(this, base.bodyChunks[0], tentacleLength, i);
			appendages.Add(new Appendage(this, i, tentacle[i].tChunks.Length + 1));
		}
		legs = new LoachLeg[6];
		for (int j = 0; j < legs.Length; j++)
		{
			legs[j] = new LoachLeg(this, base.bodyChunks[j % 2 + 1], base.firstChunk, 200f, j);
		}
		eatObjects = new List<EatObject>();
		if (!Rotted)
		{
			effectColor = Color.gray;
			eyeColor = effectColor;
		}
		else if (world.region != null)
		{
			effectColor = world.region.regionParams.corruptionEffectColor;
			eyeColor = world.region.regionParams.corruptionEyeColor;
		}
		else
		{
			effectColor = new Color(0f, 0f, 1f);
			eyeColor = effectColor;
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new LoachGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
	}

	public override void NewRoom(Room newRoom)
	{
		LoachTentacle[] array = tentacle;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].NewRoom(room);
		}
		preferredHeight = maxPreferredHeight;
		for (int j = 0; j < legs.Length; j++)
		{
			legs[j].NewRoom(newRoom);
		}
		base.NewRoom(newRoom);
	}

	public void UpdateTentacle()
	{
		Player.InputPackage? inputPackage = inputWithDiagonals;
		if (inputPackage.HasValue)
		{
			Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
			if (valueOrDefault.pckp && controlRegion == null)
			{
				controlRegion = new InputCircularRegion(tentacleLength + 50f, 0f, 360f, 5f, new Vector2(0f, -100f));
			}
			if (!valueOrDefault.pckp && controlRegion != null)
			{
				if (controlRegion.room != null)
				{
					room.RemoveObject(controlRegion);
				}
				controlRegion = null;
			}
			if (controlRegion != null)
			{
				controlRegion.drawDebugPos = base.firstChunk.pos;
				controlRegion.Update(inputWithDiagonals);
				controlLimit = null;
			}
		}
		LoachTentacle[] array = tentacle;
		foreach (LoachTentacle obj in array)
		{
			obj.Tip.vel += Vector2.down * 0.5f;
			obj.Update();
			obj.limp = !base.Consious;
		}
		Vector2? vector = controlLimit;
		if (!vector.HasValue)
		{
			return;
		}
		Vector2 valueOrDefault2 = vector.GetValueOrDefault();
		if (controlRegion != null)
		{
			float num = Custom.Dist(valueOrDefault2, controlRegion.pos);
			if (num > 60f)
			{
				controlRegion.pos = Vector2.Lerp(controlRegion.pos, valueOrDefault2, Mathf.Lerp(0f, 0.2f, (num - 60f) / 200f));
			}
			controlLimit = null;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (base.safariControlled)
		{
			ref Player.InputPackage? reference = ref inputWithDiagonals;
			if (reference.HasValue && reference.GetValueOrDefault().jmp)
			{
				ref Player.InputPackage? reference2 = ref lastInputWithDiagonals;
				if (reference2.HasValue && !reference2.GetValueOrDefault().jmp)
				{
					StartLurch();
				}
			}
		}
		else if (State.isChasing && AI.preyTracker.TotalTrackedPrey > 0 && !isLurching)
		{
			if (AI.preyTracker.MostAttractivePrey.VisualContact)
			{
				State.UpdateInterest(1);
				if (State.interest >= 350 && UnityEngine.Random.value > 0.999f)
				{
					StartLurch();
				}
			}
			else if (State.UpdateInterest(-1))
			{
				AI.preyTracker.ForgetAllPrey();
			}
		}
		WeightedPush(0, 1, Vector3.Slerp(bodDir, new Vector2(flipDir, 0f), base.Consious ? 0.5f : 0f), 1f);
		WeightedPush(1, 2, Vector3.Slerp(bodDir, new Vector2(flipDir, 0f), base.Consious ? 0.5f : 0f), 0.65f);
		WeightedPush(2, 3, Vector3.Slerp(bodDir, new Vector2(flipDir, -0.2f), base.Consious ? 0.5f : 0f), 0.5f);
		WeightedPush(3, 4, Vector3.Slerp(bodDir, new Vector2(flipDir, 0f), base.Consious ? 0.5f : 0f), 0.5f);
		WeightedPush(0, 1, new Vector2(flipDir, 0f), 0.85f);
		WeightedPush(0, 1, bodDir, 0.6f);
		for (int i = 0; i < 4; i++)
		{
			WeightedPush(i, 5, base.Consious ? (bodDir * 0.4f) : new Vector2(0f, 0f), 1.1f - (float)i * 0.3f);
		}
		WeightedPush(4, 2, Vector2.up, sacLiftFactor * 2f);
		WeightedPush(5, 2, Vector2.up, sacLiftFactor * 2f);
		if (Mathf.Sign(bodDir.x) == Mathf.Sign(flipDir))
		{
			WeightedPush(0, 2, Vector2.down, (1f - headLiftFactor) * 1.5f);
			WeightedPush(1, 2, Vector2.down, (1f - headLiftFactor) * 0.5f);
		}
		if ((base.bodyChunks[4].ContactPoint != default(IntVector2) && base.bodyChunks[4].ContactPoint.y < 1) || (base.bodyChunks[5].ContactPoint != default(IntVector2) && base.bodyChunks[5].ContactPoint.y < 1))
		{
			sacLiftFactor = Mathf.Min(sacLiftFactor + 0.1f, 1f);
		}
		else
		{
			sacLiftFactor = Custom.LerpAndTick(sacLiftFactor, 0f, 0.02f, 0.01f);
		}
		if (AI.behavior == LoachAI.Behavior.Hunt && AI.focusCreature?.representedCreature?.realizedCreature != null)
		{
			Vector2 vector = (AI.focusCreature.VisualContact ? AI.focusCreature.representedCreature.realizedCreature.DangerPos : room.MiddleOfTile(AI.focusCreature.BestGuessForPosition()));
			Vector2 vector2 = base.bodyChunks[0].pos - vector;
			float b = Mathf.InverseLerp(-20f, 100f, vector2.y / Mathf.Max(Mathf.Abs(vector2.x) / 100f, 1f));
			headLiftFactor = Mathf.Lerp(headLiftFactor, b, 0.05f);
		}
		else
		{
			headLiftFactor = Mathf.Lerp(headLiftFactor, 0.2f, 0.05f);
		}
		legsGrabbing = 0;
		float num = 0f;
		float num2 = float.MinValue;
		LoachLeg loachLeg = null;
		for (int j = 0; j < legs.Length; j++)
		{
			legs[j].Update();
			if (legs[j].attachedAtTip)
			{
				legsGrabbing++;
			}
			if (legs[j].ReleaseScore() > num2)
			{
				num2 = legs[j].ReleaseScore();
				loachLeg = legs[j];
			}
			num += legs[j].Support();
		}
		if (!stayStill && legsGrabbing > 5)
		{
			loachLeg?.ReleaseGrip();
		}
		num = Mathf.Pow(Mathf.Min(num / 3f, 1f), 0.8f) * Mathf.Pow(1f - resting, 0.3f);
		for (int k = 0; k < 5; k++)
		{
			if (base.bodyChunks[k].ContactPoint.x != 0 || base.bodyChunks[k].ContactPoint.y < 0)
			{
				num = Mathf.Lerp(num, 1f, 0.6f);
			}
		}
		num = Mathf.Lerp(num, UnityEngine.Random.Range(0.05f, 0.085f), CloseToEdge);
		num = Mathf.Pow(num, 0.1f);
		num *= UnityEngine.Random.Range(0.85f, 1f);
		float num3 = Mathf.Lerp(Mathf.Pow((float)Mathf.Max(legsGrabbing - 2, 0) / 4f, 0.8f), num, 0.5f);
		bool flag = false;
		for (int l = 0; l < legs.Length; l++)
		{
			if (flag)
			{
				break;
			}
			if (legs[l].attachedAtTip && (legs[l].Tip.pos.x > legs[l].connectedChunk.pos.x == moveDirection.x > 0f || Mathf.Abs(legs[l].Tip.pos.x - legs[l].connectedChunk.pos.x) < legInFrontMargin))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			if (!stayStill)
			{
				num3 = Mathf.Lerp(Custom.LerpMap(hesistCounter, 20f, 150f, -0.5f, 1f), num3, CloseToEdge);
			}
			hesistCounter++;
		}
		else
		{
			hesistCounter = 0;
		}
		num3 = Mathf.Lerp(num3, 1f, GetUnstuckForce);
		bodDir *= 0f;
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (base.Consious)
		{
			Act(eu, num, num3);
		}
		for (int m = 0; m < 3; m++)
		{
			WeightedPush(m, m + 2, -Custom.DirVec(base.bodyChunks[m].pos, base.bodyChunks[m + 2].pos), 0.45f);
			bodDir += Custom.DirVec(base.bodyChunks[m + 2].pos, base.bodyChunks[m + 1].pos);
		}
		if (base.Consious)
		{
			bodDir += new Vector2(0.8f * flipDir, 0f);
		}
		bodDir.Normalize();
		if (digestingCounter > 0)
		{
			digestingCounter--;
		}
		Eat(eu);
	}

	public void Act(bool eu, float support, float forwardPower)
	{
		if (base.Submersion > 0.8f)
		{
			WeightedPush(0, 1, new Vector2(0f, 1f), 0.26f);
		}
		AI.Update();
		vocalizeSoundCooldown--;
		if (vocalizeSoundCooldown < 0)
		{
			if (base.safariControlled)
			{
				ref Player.InputPackage? reference = ref inputWithDiagonals;
				if (reference.HasValue && reference.GetValueOrDefault().thrw)
				{
					room.PlaySound(Rotted ? WatcherEnums.WatcherSoundID.Loach_Aggressive_Groan : WatcherEnums.WatcherSoundID.Loach_Passive_Groan, base.firstChunk);
					vocalizeSoundCooldown = (Rotted ? 120 : 20) + UnityEngine.Random.Range(0, 20);
				}
			}
			else if (UnityEngine.Random.value < 0.00055555557f)
			{
				room.PlaySound(Rotted ? WatcherEnums.WatcherSoundID.Loach_Aggressive_Groan : WatcherEnums.WatcherSoundID.Loach_Passive_Groan, base.firstChunk);
				vocalizeSoundCooldown = (base.safariControlled ? 100 : UnityEngine.Random.Range(500, 900));
			}
		}
		foreach (SlimeMold slimeMold in AI.slimeMolds)
		{
			if (slimeMold != null && Custom.DistLess(slimeMold.firstChunk.pos, base.firstChunk.pos, 40f))
			{
				slimeMoldToGrab = slimeMold;
				slimeMoldGrabPos = slimeMold.firstChunk.pos;
				break;
			}
			slimeMoldGrabPos = null;
		}
		UpdateTentacle();
		if (AI.AllowMovementBetweenRooms)
		{
			enterRoomForcePush = Mathf.Max(0f, enterRoomForcePush - 0.001f);
		}
		else if (Mathf.Abs(base.abstractCreature.pos.x) < 10)
		{
			if (room.VisualContact(base.mainBodyChunk.pos, base.mainBodyChunk.pos + new Vector2(400f, 0f)))
			{
				base.bodyChunks[1].vel.x += Mathf.Pow(enterRoomForcePush, 2f) * 2f;
				enterRoomForcePush = Mathf.Min(2f, enterRoomForcePush + 1f / 180f);
			}
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				if (base.bodyChunks[i].ContactPoint.x > 0)
				{
					base.bodyChunks[i].vel += Custom.DegToVec(-90f * UnityEngine.Random.value) * 15f;
				}
			}
		}
		else if (Mathf.Abs(base.abstractCreature.pos.x - (room.TileWidth - 1)) < 10)
		{
			if (room.VisualContact(base.mainBodyChunk.pos, base.mainBodyChunk.pos + new Vector2(-400f, 0f)))
			{
				base.bodyChunks[1].vel.x -= Mathf.Pow(enterRoomForcePush, 2f) * 2f;
				enterRoomForcePush = Mathf.Min(2f, enterRoomForcePush + 1f / 180f);
			}
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				if (base.bodyChunks[j].ContactPoint.x < 0)
				{
					base.bodyChunks[j].vel += Custom.DegToVec(90f * UnityEngine.Random.value) * 15f;
				}
			}
		}
		stayStill = room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile) && AI.pathFinder.GetEffectualDestination.room == room.abstractRoom.index && Custom.ManhattanDistance(base.abstractCreature.pos, AI.pathFinder.GetEffectualDestination) < 4;
		bool flag = false;
		if (stayStill && AI.pathFinder.DestInRoom)
		{
			flag = true;
			for (int k = -1; k < 2 && flag; k++)
			{
				if (room.aimap.getAItile(AI.pathFinder.GetEffectualDestination.Tile + new IntVector2(k * 2, 0)).floorAltitude > 5)
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			resting = Mathf.Min(1f, resting + 1f / 60f);
		}
		else
		{
			resting = Mathf.Max(0f, resting - 1f / 160f);
		}
		if (!base.safariControlled)
		{
			flipDir = Mathf.Sign(base.bodyChunks[0].pos.x - base.bodyChunks[4].pos.x);
		}
		if (base.safariControlled && controlRegion == null)
		{
			flipDir = lastControlX;
		}
		preferredHeight = Mathf.Lerp(minPreferredHeight, midIdealHeight, heightLerp);
		heightLerp += 0.0125f;
		heightLerp -= 1f / 120f * (float)tentacle[0].GetTentacleGrabTotal();
		heightLerp = Mathf.Clamp01(heightLerp);
		MovementConnection followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		if (followingConnection == default(MovementConnection))
		{
			for (int l = 1; l < 3; l++)
			{
				if (!(followingConnection == default(MovementConnection)))
				{
					break;
				}
				if (room == null)
				{
					break;
				}
				for (int m = 0; m < 5; m++)
				{
					if (!(followingConnection == default(MovementConnection)))
					{
						break;
					}
					if (room == null)
					{
						break;
					}
					for (int n = 0; n < 5; n++)
					{
						if (!(followingConnection == default(MovementConnection)))
						{
							break;
						}
						if (room == null)
						{
							break;
						}
						followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[m].pos + Custom.fourDirectionsAndZero[n].ToVector2() * (2f * (float)l)), actuallyFollowingThisPath: true);
					}
				}
			}
		}
		if (room == null)
		{
			return;
		}
		if (base.safariControlled)
		{
			SafariControls(ref followingConnection);
		}
		if (followingConnection != default(MovementConnection))
		{
			if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && followingConnection.startCoord.x <= 0)
			{
				moveDirection = new Vector2(-1f, 0f);
			}
			else if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && followingConnection.startCoord.x >= room.TileWidth - 1)
			{
				moveDirection = new Vector2(1f, 0f);
			}
			else
			{
				WorldCoordinate destinationCoord = followingConnection.destinationCoord;
				if (room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile))
				{
					for (int num = 0; num < 8; num++)
					{
						if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && destinationCoord.x <= 0)
						{
							destinationCoord.x--;
							hesistCounter = 0;
							continue;
						}
						if (AI.AllowMovementBetweenRooms && AI.pathFinder.GetDestination.room != room.abstractRoom.index && destinationCoord.x >= room.TileWidth - 1)
						{
							destinationCoord.x++;
							hesistCounter = 0;
							continue;
						}
						MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(destinationCoord, actuallyFollowingThisPath: false);
						if (!(movementConnection != default(MovementConnection)) || movementConnection.type != 0 || !(movementConnection.DestTile != followingConnection.startCoord.Tile) || !AI.pathFinder.RayTraceInAccessibleTiles(followingConnection.startCoord.Tile, movementConnection.DestTile))
						{
							break;
						}
						destinationCoord = movementConnection.destinationCoord;
					}
				}
				if (!base.safariControlled && destinationCoord.TileDefined && Mathf.Abs(room.MiddleOfTile(destinationCoord).x - base.bodyChunks[0].pos.x) > 100f)
				{
					flipDir = Mathf.Sign(room.MiddleOfTile(destinationCoord).x - base.bodyChunks[0].pos.x);
				}
				if (base.abstractCreature.pos.Tile.x < 0 || base.abstractCreature.pos.Tile.x >= room.TileWidth || destinationCoord.x < 0 || destinationCoord.x >= room.TileWidth)
				{
					nextFloorHeight = midIdealHeight;
				}
				else
				{
					nextFloorHeight = Mathf.Lerp(nextFloorHeight, (float)Custom.IntClamp(room.aimap.getAItile(destinationCoord).smoothedFloorAltitude + 2, 0, midIdealHeight + 2) * 20f, 0.2f);
				}
				moveDirection = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(destinationCoord));
			}
		}
		if (base.safariControlled && followingConnection == default(MovementConnection))
		{
			stayStill = true;
		}
		if (controlRegion != null)
		{
			stayStill = true;
			moveDirection = controlRegion.pos;
		}
		if (!lurching)
		{
			lurchProgress = Mathf.Max(lurchProgress - 1, 0);
			float num2 = Mathf.InverseLerp(75f, 0f, lurchProgress);
			for (int num3 = 0; num3 < base.bodyChunks.Length; num3++)
			{
				float num4 = (float)num3 / 5f;
				base.bodyChunks[num3].vel *= Mathf.Lerp(1f, stayStill ? 0.75f : 0.9f, support);
				base.bodyChunks[num3].vel *= 1f - resting * 0.5f;
				if (num3 < base.bodyChunks.Length - 1)
				{
					base.bodyChunks[num3].vel.y += base.gravity * Mathf.Lerp(1.3f, 2.5f, Mathf.Sin(Mathf.Pow(num4, 1.7f) * (float)Math.PI)) * num2 * support * 0.825f * Custom.LerpMap(room.aimap.getClampedAItile(base.bodyChunks[num3].pos).smoothedFloorAltitude, midIdealHeight, midIdealHeight + 4, 1f, 0.5f);
				}
				Vector2 vector = moveDirection.normalized * (Mathf.Lerp(0.65f, 0.15f, num4) * forwardPower * Mathf.Lerp(1f, 3f, GetUnstuckForce) * ((num3 == 4) ? (-0.2f) : 1f) * num2);
				if (controlRegion != null)
				{
					vector.x *= 0.25f;
				}
				base.bodyChunks[num3].vel += vector;
			}
			base.mainBodyChunk.vel.y += base.gravity * 1.2f * resting * num2;
			if (GetUnstuckForce > 0f && (!base.safariControlled || (base.safariControlled && inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.AnyDirectionalInput)))
			{
				base.bodyChunks[1].vel += Custom.RNV() * (UnityEngine.Random.value * 4f * GetUnstuckForce * num2);
			}
		}
		else
		{
			Vector2 vector2 = Custom.DirVec(base.bodyChunks[1].pos, base.mainBodyChunk.pos);
			vector2.y = 0f;
			if (lurchProgress < 30)
			{
				lurchProgress++;
				for (int num5 = 0; num5 < base.bodyChunks.Length; num5++)
				{
					float num6 = (float)num5 / 5f;
					base.bodyChunks[num5].vel *= Mathf.Lerp(1f, stayStill ? 0.75f : 0.9f, support);
					base.bodyChunks[num5].vel *= 1f - resting * 0.5f;
					if (num5 < base.bodyChunks.Length - 1)
					{
						base.bodyChunks[num5].vel.y += base.gravity * Mathf.Lerp(1.3f, 2.5f, Mathf.Sin(Mathf.Pow(num6, 1.7f) * (float)Math.PI)) * Mathf.Lerp(support * 0.825f * Custom.LerpMap(room.aimap.getClampedAItile(base.bodyChunks[num5].pos).smoothedFloorAltitude, midIdealHeight, midIdealHeight + 4, 1f, 0.5f) * Custom.LerpMap(moveDirection.y, -1f, 1f, 0.5f, 1f), 0.65f, CloseToEdge);
					}
					base.bodyChunks[num5].vel += -vector2 * (Mathf.Lerp(0.65f, 0.15f, num6) * 1f * Mathf.Lerp(1f, 3f, GetUnstuckForce) * ((num5 == 4) ? (-0.2f) : 1f));
				}
				base.mainBodyChunk.vel.y += base.gravity * 1.2f * resting;
				if (GetUnstuckForce > 0f && (!base.safariControlled || (base.safariControlled && inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.AnyDirectionalInput)))
				{
					base.bodyChunks[1].vel += Custom.RNV() * (UnityEngine.Random.value * 4f * GetUnstuckForce);
				}
			}
			else if (lurchProgress < 60)
			{
				if (lurchSound)
				{
					room.PlaySound(WatcherEnums.WatcherSoundID.Loach_Lunge, base.firstChunk);
					lurchSound = false;
				}
				lurchProgress++;
				for (int num7 = 0; num7 < base.bodyChunks.Length; num7++)
				{
					float num8 = (float)num7 / 5f;
					base.bodyChunks[num7].vel *= Mathf.Lerp(1f, stayStill ? 0.75f : 0.9f, support);
					base.bodyChunks[num7].vel *= 1f - resting * 0.5f;
					if (num7 < base.bodyChunks.Length - 1)
					{
						base.bodyChunks[num7].vel.y += base.gravity * Mathf.Lerp(1.3f, 2.5f, Mathf.Sin(Mathf.Pow(num8, 1.7f) * (float)Math.PI)) * Mathf.Lerp(support * 0.825f * Custom.LerpMap(room.aimap.getClampedAItile(base.bodyChunks[num7].pos).smoothedFloorAltitude, midIdealHeight, midIdealHeight + 4, 1f, 0.5f) * Custom.LerpMap(moveDirection.y, -1f, 1f, 0.5f, 1f), 0.65f, CloseToEdge);
					}
					float num9 = Mathf.Abs((float)lurchProgress - 45f);
					float num10 = Mathf.Pow((100f - (float)lurchProgress - num9) / 60f, 2.5f);
					base.bodyChunks[num7].vel += vector2 * (Mathf.Lerp(0.65f, 0.15f, num8) * 3.5f * num10 * Mathf.Lerp(1f, 3f, GetUnstuckForce) * ((num7 == 4) ? (-0.2f) : 1f) * 2f);
				}
				base.mainBodyChunk.vel.y += base.gravity * 1.2f * resting;
				if (GetUnstuckForce > 0f && (!base.safariControlled || (base.safariControlled && inputWithoutDiagonals.HasValue && inputWithoutDiagonals.Value.AnyDirectionalInput)))
				{
					base.bodyChunks[1].vel += Custom.RNV() * (UnityEngine.Random.value * 4f * GetUnstuckForce);
				}
			}
			else if ((float)lurchProgress < 75f)
			{
				for (int num11 = 0; num11 < base.bodyChunks.Length; num11++)
				{
					float num12 = (float)num11 / 3.5f;
					base.bodyChunks[num11].vel *= Mathf.Lerp(1f, stayStill ? 0.75f : 0.9f, support);
					base.bodyChunks[num11].vel *= 1f - resting * 0.5f;
					float num13 = Mathf.Pow((105 - lurchProgress) / 75, 2f);
					base.bodyChunks[num11].vel += vector2 * (Mathf.Lerp(0.65f, 0.15f, num12) * num12 * num13 * Mathf.Lerp(1f, 3f, GetUnstuckForce) * ((num11 == 4) ? (-0.2f) : 1f) * 2f);
				}
				lurchProgress++;
			}
			else
			{
				lurching = false;
			}
		}
		if (spasmForce != default(Vector2))
		{
			WeightedPush(spasmChunk, 2, spasmForce, 1f);
			spasmForce = Vector2.Lerp(spasmForce, Vector2.zero, 0.2f);
		}
		if (UnityEngine.Random.value < 0.04f)
		{
			spasmChunk = UnityEngine.Random.Range(0, base.bodyChunks.Length);
			spasmForce = UnityEngine.Random.insideUnitCircle * 5f;
		}
	}

	public void Eat(bool eu)
	{
		Vector2 pos = base.firstChunk.pos;
		if (eatObjects.Count > 0)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Loach_Eat, base.mainBodyChunk);
		}
		for (int num = eatObjects.Count - 1; num >= 0; num--)
		{
			if (eatObjects[num].progression > 1f)
			{
				if (eatObjects[num].chunk.owner is Creature)
				{
					digestingCounter = (int)Custom.LerpMap((eatObjects[num].chunk.owner as Creature).Template.bodySize, 0.2f, 5f, 50f, 250f);
					AI.tracker.ForgetCreature((eatObjects[num].chunk.owner as Creature).abstractCreature);
					if (eatObjects[num].chunk.owner is Player player)
					{
						player.PermaDie();
					}
				}
				else if (eatObjects[num].chunk.owner is SlimeMold)
				{
					(eatObjects[num].chunk.owner as SlimeMold).BitByCreature(eu);
					AI.slimeMoldCounter = 5000;
					AI.hasSlimeMoldPos = false;
				}
				eatObjects[num].chunk.owner.Destroy();
				eatObjects.RemoveAt(num);
				LoachTentacle[] array = tentacle;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SwitchTask(LoachTentacle.Task.SearchGround);
				}
			}
			else
			{
				if (eatObjects[num].chunk.owner.collisionLayer != 0)
				{
					eatObjects[num].chunk.owner.ChangeCollisionLayer(0);
				}
				if (ModManager.MMF && eatObjects[num].chunk.owner is Creature)
				{
					(eatObjects[num].chunk.owner as Creature).enteringShortCut = null;
				}
				float progression = eatObjects[num].progression;
				eatObjects[num].progression += 0.0125f;
				if (progression <= 0.5f && eatObjects[num].progression > 0.5f)
				{
					if (eatObjects[num].chunk.owner is Creature)
					{
						(eatObjects[num].chunk.owner as Creature).Die();
					}
					for (int j = 0; j < eatObjects[num].chunk.owner.bodyChunkConnections.Length; j++)
					{
						eatObjects[num].chunk.owner.bodyChunkConnections[j].type = BodyChunkConnection.Type.Pull;
					}
				}
				float num2 = eatObjects[num].distance * (1f - eatObjects[num].progression);
				eatObjects[num].chunk.vel *= 0f;
				eatObjects[num].chunk.MoveFromOutsideMyUpdate(eu, pos + Custom.DirVec(pos, eatObjects[num].chunk.pos) * num2);
				for (int k = 0; k < eatObjects[num].chunk.owner.bodyChunks.Length; k++)
				{
					eatObjects[num].chunk.owner.bodyChunks[k].vel *= 1f - eatObjects[num].progression;
					eatObjects[num].chunk.owner.bodyChunks[k].MoveFromOutsideMyUpdate(eu, Vector2.Lerp(eatObjects[num].chunk.owner.bodyChunks[k].pos, pos + Custom.DirVec(pos, eatObjects[num].chunk.owner.bodyChunks[k].pos) * num2, eatObjects[num].progression));
				}
				if (eatObjects[num].chunk.owner.graphicsModule != null && eatObjects[num].chunk.owner.graphicsModule.bodyParts != null)
				{
					for (int l = 0; l < eatObjects[num].chunk.owner.graphicsModule.bodyParts.Length; l++)
					{
						eatObjects[num].chunk.owner.graphicsModule.bodyParts[l].vel *= 1f - eatObjects[num].progression;
						eatObjects[num].chunk.owner.graphicsModule.bodyParts[l].pos = Vector2.Lerp(eatObjects[num].chunk.owner.graphicsModule.bodyParts[l].pos, pos, eatObjects[num].progression);
					}
				}
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

	public void SafariControls(ref MovementConnection followingConnection)
	{
		followingConnection = default(MovementConnection);
		moveDirection = Vector2.zero;
		Player.InputPackage? inputPackage = inputWithDiagonals;
		if (!inputPackage.HasValue)
		{
			return;
		}
		Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
		MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
		if (followingConnection != default(MovementConnection))
		{
			type = followingConnection.type;
		}
		if (valueOrDefault.AnyDirectionalInput)
		{
			Vector2 vector = Vector2.zero;
			if (valueOrDefault.analogueDir.magnitude > 0f)
			{
				vector = valueOrDefault.analogueDir.normalized;
			}
			else if (valueOrDefault.x != 0 || valueOrDefault.y != 0)
			{
				vector = new Vector2(valueOrDefault.x, valueOrDefault.y).normalized;
			}
			if (vector.x != 0f)
			{
				lastControlX = Mathf.Sign(vector.x);
			}
			followingConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(vector.x, vector.y) * 200f), 2);
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!base.Consious)
		{
			return;
		}
		if (creatureParams.stunOnCollideVelocity > 0f && Vector2.Distance(base.bodyChunks[myChunk].vel, otherObject.bodyChunks[otherChunk].vel) >= creatureParams.stunOnCollideVelocity && otherObject is Creature)
		{
			(otherObject as Creature).Violence(base.bodyChunks[myChunk], base.bodyChunks[myChunk].vel * base.bodyChunks[myChunk].mass, otherObject.bodyChunks[otherChunk], null, DamageType.Blunt, creatureParams.stunOnCollideDamage, creatureParams.stunOnCollideDuration);
		}
		Tracker.CreatureRepresentation creatureRepresentation = AI.tracker.RepresentationForObject(otherObject, AddIfMissing: false);
		Vector2 pos = base.firstChunk.pos;
		if ((otherObject == null || creatureRepresentation == null || !(otherObject is Creature creature) || !CanEatCreature(creature.abstractCreature)) && (otherObject == null || !(otherObject is SlimeMold)))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < tentacle.Length; i++)
		{
			if (flag)
			{
				break;
			}
			if (tentacle[i].grabChunk != null && tentacle[i].grabChunk.owner == otherObject)
			{
				flag = true;
			}
		}
		for (int j = 0; j < eatObjects.Count && flag; j++)
		{
			if (eatObjects[j].chunk.owner == otherObject)
			{
				flag = false;
			}
		}
		if (!flag || (base.safariControlled && (!base.safariControlled || !inputWithDiagonals.HasValue || !inputWithDiagonals.Value.pckp)))
		{
			return;
		}
		if (base.graphicsModule != null)
		{
			if (otherObject is IDrawable)
			{
				base.graphicsModule.AddObjectToInternalContainer(otherObject as IDrawable, 0);
			}
			else if (otherObject.graphicsModule != null)
			{
				base.graphicsModule.AddObjectToInternalContainer(otherObject.graphicsModule, 0);
			}
		}
		eatObjects.Add(new EatObject(otherObject.bodyChunks[otherChunk], Vector2.Distance(pos, otherObject.bodyChunks[otherChunk].pos)));
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		floorDetected = true;
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

	public void StartLurch()
	{
		lurching = true;
		lurchSound = true;
		floorDetected = false;
		State.interest -= 300;
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (RippleViolenceCheck(source))
		{
			if (!base.dead && vocalizeSoundCooldown <= 0)
			{
				room.PlaySound(Rotted ? WatcherEnums.WatcherSoundID.Loach_Aggressive_Groan : WatcherEnums.WatcherSoundID.Loach_Passive_Groan, base.firstChunk);
				vocalizeSoundCooldown = UnityEngine.Random.Range(500, 900);
			}
			if (source != null && source.owner is Weapon weapon && weapon.mode == Weapon.Mode.Thrown && AI.preyTracker.MostAttractivePrey != AI.tracker.RepresentationForObject((source.owner as Weapon).thrownBy, AddIfMissing: false) && State.SetNewTarget())
			{
				AI.preyTracker.ForgetAllPrey();
				AI.preyTracker.AddPrey(AI.tracker.RepresentationForObject((source.owner as Weapon).thrownBy, AddIfMissing: false));
			}
			base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
		}
	}

	public override void Stun(int st)
	{
		base.Stun(st);
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		if (segment == 0)
		{
			return base.firstChunk.pos;
		}
		return tentacle[appendage].tChunks[segment - 1].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		ApplyForceOnSegment(pos.appendage.appIndex, pos.prevSegment, momentum * (1f - pos.distanceToNext));
		ApplyForceOnSegment(pos.appendage.appIndex, pos.prevSegment + 1, momentum * pos.distanceToNext);
	}

	public void ApplyForceOnSegment(int appIndex, int segment, Vector2 momentum)
	{
		if (segment > 0 && segment <= tentacle[appIndex].tChunks.Length)
		{
			tentacle[appIndex].tChunks[segment - 1].pos += momentum / 0.2f;
			tentacle[appIndex].tChunks[segment - 1].vel += momentum / 0.1f;
		}
	}
}
