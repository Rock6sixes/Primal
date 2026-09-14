using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class FireSpriteAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior CollectEnergy = new Behavior("CollectEnergy", register: true);

		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior ActivateRoot = new Behavior("ActivateRoot", register: true);

		public static Behavior AvoidDanger = new Behavior("AvoidDanger", register: true);

		public static Behavior Helping = new Behavior("Helping", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Counter checkForHelpeesTimer = new Counter(80);

	public Counter helpCooldown = new Counter(100);

	public Counter lagCounter = new Counter(10);

	public Counter idleCounter;

	public Counter waitBeforeDive = new Counter(20);

	public Counter uncomfortable = new Counter(200);

	public Counter randomFlyDelay = new Counter(10);

	public bool canGoToRestingPointAgain;

	public WorldCoordinate lastPos;

	public AbstractCreature helpee;

	public EntityID nowHelping;

	public List<WorldCoordinate> restingPoints;

	public int lastUncomfortablePoint = -2;

	public int currentRestingPoint = -1;

	public bool collectingEnergy;

	public int restingPointDesperation;

	public DebugSprite[] restingDebug;

	public DebugSprite randomFlyTarget;

	public AbstractRoom homeRoom;

	public int maxRestingSpots = 7;

	public bool debug;

	public Vector2 helpeeEntrance;

	public Vector2 helpeeEntranceVec;

	public Behavior behavior;

	public FireSprite ThisCreature => creature.realizedObject as FireSprite;

	public float distanceToHelpee
	{
		get
		{
			if (helpee == null || helpee.realizedObject == null)
			{
				return 2.1474836E+09f;
			}
			if (helpee.Room != creature.Room)
			{
				return Custom.WorldCoordFloatDist(helpee.realizedObject.room.GetWorldCoordinate((helpee.realizedObject as ICallForHelp).HelpPoint()), creature.pos);
			}
			return Vector2.Distance((helpee.realizedObject as ICallForHelp).HelpPoint(), ThisCreature.mainBodyChunk.pos) * 0.05f;
		}
	}

	public FireSpriteAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		ThisCreature.ai = this;
		AddModule(new StandardPather(this, world, creature));
		AddModule(new Tracker(this, 10, 10, -1, 0.35f, 5, 5, 10)
		{
			visualize = false
		});
		AddModule(new RelationshipTracker(this, base.tracker));
		base.pathFinder.stepsPerFrame = 100;
		base.pathFinder.accessibilityStepsPerFrame = 60;
		helpCooldown.SetToMin();
		restingPoints = new List<WorldCoordinate>();
		idleCounter = new Counter(Random.Range(200, 400));
		SetBehavior(Behavior.Idle);
		randomFlyDelay.SetToMin();
		if (debug)
		{
			restingDebug = new DebugSprite[maxRestingSpots];
		}
	}

	public void UpdateTimers()
	{
		checkForHelpeesTimer.Tick();
		helpCooldown.Tick();
		lagCounter.Tick();
		idleCounter.Tick();
		uncomfortable.Tick();
		randomFlyDelay.Tick();
	}

	public override void NewRoom(Room room)
	{
		if (debug && randomFlyTarget != null)
		{
			randomFlyTarget.RemoveFromRoom();
			room.AddObject(randomFlyTarget);
		}
		base.NewRoom(room);
	}

	public override void Update()
	{
		lastPos = creature.pos;
		base.Update();
		if (debug)
		{
			for (int i = 0; i < restingDebug.Length; i++)
			{
				if (restingDebug[i] == null)
				{
					FSprite sp = new FSprite("buttonYB");
					restingDebug[i] = new DebugSprite(Vector2.zero, sp, ThisCreature.room);
				}
			}
			if (randomFlyTarget == null)
			{
				FSprite fSprite = new FSprite("LizardBubble6");
				fSprite.color = Color.red;
				randomFlyTarget = new DebugSprite(Vector2.zero, fSprite, ThisCreature.room);
				ThisCreature.room.AddObject(randomFlyTarget);
			}
		}
		UpdateTimers();
		NoticeProttectedCreatures();
		SeeIfHelpNeeded();
		if (helpee != null && helpee.realizedObject != null)
		{
			helpeeEntrance = (helpee.realizedObject as ICallForHelp).HelpPoint();
			helpeeEntranceVec = (helpee.realizedObject as ICallForHelp).HelpEntranceVec();
		}
		if (ThisCreature.safariControlled)
		{
			SafariBehavior();
		}
		else
		{
			DecideBehavior();
		}
		ExecuteBehavior();
	}

	public void SafariBehavior()
	{
		if (!lagCounter.isFinished)
		{
			return;
		}
		Player.InputPackage? inputWithDiagonals = ThisCreature.inputWithDiagonals;
		if (!inputWithDiagonals.HasValue)
		{
			return;
		}
		Player.InputPackage valueOrDefault = inputWithDiagonals.GetValueOrDefault();
		if (behavior == Behavior.Helping)
		{
			if (helpee != null)
			{
				helpee.controlled = true;
			}
			AbstractCreature abstractCreature = helpee;
			if (abstractCreature == null || !(abstractCreature.realizedCreature is ICallForHelp callForHelp) || callForHelp.CanLetGo())
			{
				helpCooldown.Reset();
				SetBehavior(Behavior.Idle);
				helpee.controlled = false;
			}
		}
		else if (valueOrDefault.jmp)
		{
			if (behavior != Behavior.CollectEnergy)
			{
				SetBehavior(Behavior.CollectEnergy);
			}
		}
		else if (valueOrDefault.pckp || ThisCreature.divingToHelp)
		{
			if (behavior != Behavior.ActivateRoot)
			{
				FindHelpeeInRangeToForceHelp();
				if (helpee != null)
				{
					SetBehavior(Behavior.ActivateRoot);
				}
			}
			else if (distanceToHelpee < 0.05f && ThisCreature.divingToHelp && helpCooldown.isFinished)
			{
				ThisCreature.room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Fire_Sprite_Enter, ThisCreature.mainBodyChunk);
				SetBehavior(Behavior.Helping);
			}
		}
		else if (behavior != Behavior.Idle)
		{
			SetBehavior(Behavior.Idle);
			collectingEnergy = false;
		}
	}

	public void DecideBehavior()
	{
		if (!lagCounter.isFinished)
		{
			return;
		}
		if (behavior == Behavior.CollectEnergy)
		{
			if (helpee != null && helpee.realizedObject is ICallForHelp callForHelp && !callForHelp.IsHelpComing())
			{
				collectingEnergy = false;
				SetBehavior(Behavior.ActivateRoot);
			}
			else if (!uncomfortable.isFinished)
			{
				SetBehavior(Behavior.AvoidDanger);
			}
		}
		else if (behavior == Behavior.Idle)
		{
			if (helpee != null && helpee.realizedObject is ICallForHelp callForHelp2 && !callForHelp2.IsHelpComing())
			{
				SetBehavior(Behavior.ActivateRoot);
			}
			else if (idleCounter.isFinished)
			{
				SetBehavior(Behavior.CollectEnergy);
			}
		}
		else if (behavior == Behavior.ActivateRoot)
		{
			if (helpee == null)
			{
				SetBehavior(Behavior.Idle);
			}
			else if (!(helpee.realizedCreature as ICallForHelp).CanAcceptHelp())
			{
				helpee = null;
				SetBehavior(Behavior.Idle);
			}
			else if (distanceToHelpee < 0.05f && ThisCreature.divingToHelp && helpCooldown.isFinished)
			{
				ThisCreature.room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Fire_Sprite_Enter, ThisCreature.mainBodyChunk);
				SetBehavior(Behavior.Helping);
			}
		}
		else if (behavior == Behavior.Helping)
		{
			AbstractCreature abstractCreature = helpee;
			if (abstractCreature == null || !(abstractCreature.realizedCreature is ICallForHelp callForHelp3) || callForHelp3.CanLetGo())
			{
				helpCooldown.Reset();
				SetBehavior(Behavior.Idle);
			}
		}
		else if (behavior == Behavior.AvoidDanger)
		{
			if (helpee != null && helpee.realizedObject is ICallForHelp callForHelp4 && !callForHelp4.IsHelpComing())
			{
				SetBehavior(Behavior.ActivateRoot);
			}
			else if (uncomfortable.isFinished)
			{
				SetBehavior(Behavior.Idle);
			}
		}
	}

	public void SetBehavior(Behavior bhvr)
	{
		_ = debug;
		currentRestingPoint = -1;
		if (bhvr == Behavior.Helping)
		{
			nowHelping = helpee.ID;
		}
		if (bhvr == Behavior.AvoidDanger)
		{
			ThisCreature.stuckTimer.Set(0);
		}
		if (bhvr == Behavior.Helping)
		{
			nowHelping = creature.ID;
			idleCounter.Reset();
		}
		if (behavior == Behavior.Helping)
		{
			ThisCreature.divingToHelp = false;
			ThisCreature.ejectVector = helpeeEntranceVec.normalized * 5f;
		}
		if (behavior == Behavior.CollectEnergy)
		{
			ThisCreature.wingsOpen = false;
			ThisCreature.wingOpenTimer.Reset();
		}
		if (bhvr == Behavior.ActivateRoot)
		{
			ICallForHelp callForHelp = helpee.realizedObject as ICallForHelp;
			if (!callForHelp.IsHelpComing() && helpee.Room != creature.Room && Random.value < 0.9f)
			{
				helpee = null;
				return;
			}
			callForHelp.AggreToHelp();
			waitBeforeDive.Reset();
		}
		behavior = bhvr;
		Lag();
	}

	public void ExecuteBehavior()
	{
		if (behavior == Behavior.Idle)
		{
			LookForRestingPoints();
			FlyRandomly();
			ThisCreature.FollowPath();
		}
		else if (behavior == Behavior.AvoidDanger)
		{
			collectingEnergy = false;
			FlyRandomly();
			ThisCreature.FollowPath();
		}
		else if (behavior == Behavior.CollectEnergy)
		{
			LookForRestingPoints();
			TryGoToRestingPoint();
			ThisCreature.FollowPath();
			if (ThisCreature.safariControlled)
			{
				collectingEnergy = true;
			}
			else if (currentRestingPoint != -1)
			{
				collectingEnergy = Custom.WorldCoordFloatDist(restingPoints[currentRestingPoint], creature.pos) <= 1f;
			}
			else
			{
				collectingEnergy = false;
			}
			ThisCreature.OpenWings();
		}
		else
		{
			if (helpee == null)
			{
				return;
			}
			if (behavior == Behavior.ActivateRoot)
			{
				(helpee.realizedCreature as ICallForHelp).AggreToHelp();
				WorldCoordinate worldCoordinate = helpee.Room.realizedRoom.GetWorldCoordinate(helpeeEntrance + helpeeEntranceVec);
				if (waitBeforeDive.isFinished)
				{
					ThisCreature.DiveToHelp(helpeeEntrance, helpeeEntranceVec);
					return;
				}
				if (Vector2.Distance(helpeeEntrance, ThisCreature.mainBodyChunk.pos) < 80f)
				{
					waitBeforeDive.Tick();
				}
				SetDestination(worldCoordinate);
				ThisCreature.FollowPath();
			}
			else if (behavior == Behavior.Helping)
			{
				ThisCreature.mainBodyChunk.pos = (helpee.realizedObject as ICallForHelp).HelpPoint();
				if ((helpee.realizedCreature as ICallForHelp).NeedsHelp() || (ThisCreature.safariControlled && !helpee.controlled))
				{
					(helpee.realizedCreature as ICallForHelp).RecieveHelp();
				}
			}
		}
	}

	public void Lag()
	{
		lagCounter.Set(20 + (int)(Random.value * 40f));
	}

	public void NoticeProttectedCreatures()
	{
		if (homeRoom == null || !checkForHelpeesTimer.isFinished)
		{
			return;
		}
		checkForHelpeesTimer.Reset();
		for (int i = 0; i < homeRoom.creatures.Count; i++)
		{
			if (homeRoom.creatures[i].realizedCreature is ICallForHelp)
			{
				base.tracker.SeeCreature(homeRoom.creatures[i]);
			}
		}
		int[] connections = homeRoom.connections;
		for (int j = 0; j < connections.Length; j++)
		{
			AbstractRoom abstractRoom = creature.Room.world.GetAbstractRoom(connections[j]);
			if (abstractRoom == null)
			{
				continue;
			}
			for (int k = 0; k < abstractRoom.creatures.Count; k++)
			{
				if (abstractRoom.creatures[k].realizedCreature is ICallForHelp)
				{
					base.tracker.SeeCreature(abstractRoom.creatures[k]);
				}
			}
		}
	}

	public void SeeIfHelpNeeded()
	{
		AbstractCreature abstractCreature = helpee;
		if (abstractCreature != null && abstractCreature.realizedCreature is ICallForHelp callForHelp && (!callForHelp.CanLetGo() || callForHelp.IsHelpComing()))
		{
			return;
		}
		helpee = null;
		float num = 0f;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(i);
			if (rep?.representedCreature?.realizedCreature is ICallForHelp callForHelp2 && callForHelp2.NeedsHelp() && callForHelp2.HelpPriority() > num && callForHelp2.CanAcceptHelp() && !callForHelp2.IsHelpComing())
			{
				helpee = rep.representedCreature;
				num = callForHelp2.HelpPriority();
			}
		}
	}

	public void FindHelpeeInRangeToForceHelp()
	{
		float num = float.MaxValue;
		AbstractCreature abstractCreature = null;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(i);
			if (rep?.representedCreature?.realizedCreature is ICallForHelp callForHelp)
			{
				float num2 = Vector2.Distance(ThisCreature.mainBodyChunk.pos, callForHelp.HelpPoint());
				if (callForHelp.CanAcceptHelp() && num2 < num)
				{
					abstractCreature = rep.representedCreature;
					num = num2;
				}
			}
		}
		helpee = abstractCreature;
	}

	public void FlyRandomly()
	{
		if (!randomFlyDelay.isFinished || ThisCreature.DistanceToDestination > 2f)
		{
			return;
		}
		Room realizedRoom = creature.Room.realizedRoom;
		WorldCoordinate worldCoordinate = creature.pos;
		int num = 10;
		float num2 = 5f + uncomfortable.normalized * 0.1f;
		float maxInclusive = num2 + 10f;
		for (int i = 0; i < 5; i++)
		{
			IntVector2 intVector = IntVector2.FromVector2(new Vector2(Random.Range(num2, maxInclusive) * Mathf.Sign(Random.Range(-1, 1)), Random.Range(num2, maxInclusive) * Mathf.Sign(Random.Range(-1, 1))));
			WorldCoordinate worldCoordinate2 = creature.pos + intVector;
			if (!realizedRoom.IsPositionInsideBoundries(worldCoordinate2.Tile) || realizedRoom.GetTile(worldCoordinate2).Terrain != 0)
			{
				continue;
			}
			int terrainProximity = realizedRoom.aimap.getTerrainProximity(worldCoordinate2);
			if (!((float)Custom.ManhattanDistance(base.pathFinder.GetDestination.Tile, worldCoordinate2.Tile) < 4f) && terrainProximity > 3)
			{
				if (Mathf.Abs(10 - terrainProximity) > num)
				{
					num = 10;
					continue;
				}
				num = terrainProximity;
				worldCoordinate = worldCoordinate2;
			}
		}
		if (!(worldCoordinate == default(WorldCoordinate)) && base.pathFinder.CoordinateReachable(worldCoordinate))
		{
			if (debug)
			{
				randomFlyTarget.pos = ThisCreature.room.MiddleOfTile(worldCoordinate);
			}
			SetDestination(worldCoordinate);
			randomFlyDelay.Reset();
		}
	}

	public void TryGoToRestingPoint()
	{
		if (restingPoints.Count == 0)
		{
			return;
		}
		if (currentRestingPoint == -1)
		{
			currentRestingPoint = Random.Range(0, restingPoints.Count);
			if (restingPoints.Count > 1 && currentRestingPoint == lastUncomfortablePoint)
			{
				currentRestingPoint = -1;
			}
			else
			{
				base.pathFinder.SetDestination(restingPoints[currentRestingPoint]);
			}
			return;
		}
		base.pathFinder.SetDestination(restingPoints[currentRestingPoint]);
		if (Custom.WorldCoordFloatDist(creature.pos, restingPoints[currentRestingPoint]) < 5f)
		{
			canGoToRestingPointAgain = true;
		}
		else if (canGoToRestingPointAgain)
		{
			canGoToRestingPointAgain = false;
			currentRestingPoint = -1;
		}
	}

	public void LookForRestingPoints()
	{
		if (restingPoints.Count >= maxRestingSpots)
		{
			return;
		}
		if (homeRoom == null)
		{
			homeRoom = creature.Room;
		}
		int num = 80;
		IntVector2? intVector = null;
		Room realizedRoom = creature.Room.realizedRoom;
		while (num > 0)
		{
			num--;
			IntVector2 pos = realizedRoom.RandomTile();
			if (realizedRoom.GetTile(pos).Terrain == Room.Tile.TerrainType.Air && base.pathFinder.CoordinateReachableAndGetbackable(realizedRoom.GetWorldCoordinate(pos)))
			{
				IntVector2 fallRiskTile = realizedRoom.aimap.getAItile(pos).fallRiskTile;
				if (realizedRoom.aimap.WorldCoordinateAccessibleToCreature(realizedRoom.GetWorldCoordinate(fallRiskTile), creature.creatureTemplate) && pos.y - fallRiskTile.y >= Mathf.Max(9 - restingPointDesperation / 3, 2))
				{
					intVector = fallRiskTile;
				}
			}
		}
		if (!intVector.HasValue)
		{
			restingPointDesperation++;
			return;
		}
		for (int i = 1; i <= 3 && realizedRoom.GetTile(intVector.Value + Custom.eightDirections[i]).Terrain == Room.Tile.TerrainType.Solid; i++)
		{
		}
		for (int j = 0; j < Mathf.Max(4 - restingPointDesperation / 3, 1); j++)
		{
			if (realizedRoom.GetTile(intVector.Value + Custom.fourDirections[0] * j).IsSolid() || realizedRoom.GetTile(intVector.Value + Custom.fourDirections[2] * j).IsSolid())
			{
				restingPointDesperation++;
				return;
			}
		}
		if (!(intVector == Custom.eightDirections[1]))
		{
			restingPoints.Add(realizedRoom.GetWorldCoordinate(intVector.Value));
			if (debug)
			{
				restingDebug[restingPoints.Count - 1].pos = realizedRoom.MiddleOfTile(intVector.Value);
				realizedRoom.AddObject(restingDebug[restingPoints.Count - 1]);
			}
			restingPointDesperation = 0;
		}
	}

	public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Attacks)
		{
			return base.preyTracker;
		}
		return null;
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (StaticRelationship(otherCreature).type == CreatureTemplate.Relationship.Type.SocialDependent)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 1f, forgetWhenNotVisible: false);
		}
		return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0.5f, forgetWhenNotVisible: true);
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return rel.state;
	}

	public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (behavior == Behavior.Helping)
		{
			return dRelation.currentRelationship;
		}
		CreatureTemplate.Relationship result = DynamicRelationship(dRelation.trackerRep.representedCreature);
		if (result.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			SocialMemory.Relationship relationship = creature.state.socialMemory?.GetRelationship(dRelation.trackerRep.representedCreature.ID);
			float num = 0f;
			if (relationship != null)
			{
				result.intensity = relationship.fear;
			}
			if (dRelation.trackerRep.representedCreature.realizedCreature != null && dRelation.trackerRep.representedCreature.realizedCreature is Player player)
			{
				num += 0f - creature.world.game.session.creatureCommunities.LikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (player.abstractCreature.state as PlayerState).playerNumber);
			}
			if ((float)Custom.ManhattanDistance(creature.pos, dRelation.trackerRep.representedCreature.pos) < 30f * Mathf.Clamp01(result.intensity + num * 0.3f))
			{
				int num2 = (int)(200f * result.intensity);
				if (currentRestingPoint != -1)
				{
					lastUncomfortablePoint = currentRestingPoint;
				}
				if (uncomfortable.isFinished)
				{
					uncomfortable.SetMax(num2);
				}
				else
				{
					uncomfortable.SetMax(Mathf.Max(uncomfortable.max, num2));
				}
				uncomfortable.Reset();
			}
			return result;
		}
		return dRelation.currentRelationship;
	}
}
