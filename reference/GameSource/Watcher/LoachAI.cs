using System;
using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class LoachAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
	public class AIParams
	{
		public bool fleesFromThreats = true;

		public bool huntsPrey = true;

		public bool respondsToNoise = true;
	}

	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior Travelling = new Behavior("Travelling", register: true);

		public static Behavior Flee = new Behavior("Flee", register: true);

		public static Behavior Hunt = new Behavior("Hunt", register: true);

		public static Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);

		public static Behavior InvestigateSound = new Behavior("InvestigateSound", register: true);

		public static Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Loach realizedCreature;

	public float currentUtility;

	public AIParams aiparams;

	public Tracker.CreatureRepresentation focusCreature;

	public int timeInThisRoom;

	public Behavior behavior;

	public DebugDestinationVisualizer debugDestinationVisualizer;

	public WorldCoordinate slimeMoldPos;

	public bool hasSlimeMoldPos;

	public int idleCounter;

	public int idleSpotWinStreak;

	public WorldCoordinate forbiddenIdleSpot = new WorldCoordinate(-1, -1, -1, -1);

	public Behavior lastBehavior;

	public int slimeMoldCounter = 1000;

	public List<SlimeMold> slimeMolds = new List<SlimeMold>();

	public bool AllowMovementBetweenRooms => timeInThisRoom > 100;

	public float TravelUtility
	{
		get
		{
			if (!creature.abstractAI.WantToMigrate)
			{
				return 0f;
			}
			if (!base.pathFinder.CoordinateReachable(creature.abstractAI.MigrationDestination))
			{
				return 0f;
			}
			float num = realizedCreature.room.abstractRoom.AttractionValueForCreature(creature);
			float num2 = realizedCreature.room.world.GetAbstractRoom(creature.abstractAI.MigrationDestination).AttractionValueForCreature(creature);
			float num3 = Mathf.Lerp(0.99f, 0.1f, Mathf.Pow(num, 0.25f));
			if (num > num2)
			{
				return num3 *= 0.25f;
			}
			if (num == num2)
			{
				return num3 *= 0.5f;
			}
			return num3;
		}
	}

	public LoachAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		realizedCreature = creature.realizedCreature as Loach;
		realizedCreature.AI = this;
		aiparams = new AIParams();
		AddModule(new StandardPather(this, world, creature)
		{
			timeUntilPathCertain = 20,
			accessibilityStepsPerFrame = 40
		});
		AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10));
		if (aiparams.respondsToNoise)
		{
			AddModule(new NoiseTracker(this, base.tracker));
		}
		if (aiparams.fleesFromThreats)
		{
			AddModule(new ThreatTracker(this, 3));
		}
		if (aiparams.huntsPrey)
		{
			AddModule(new PreyTracker(this, 5, 2f, 3f, 70f, 0.5f));
		}
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: true));
		base.stuckTracker.totalTrackedLastPositions = 40;
		base.stuckTracker.checkPastPositionsFrom = 30;
		base.stuckTracker.pastPosStuckDistance = 5;
		base.stuckTracker.pastStuckPositionsCloseToIncrementStuckCounter = 10;
		base.stuckTracker.AddSubModule(new StuckTracker.MoveBacklog(base.stuckTracker));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new NoiseTracker(this, base.tracker));
		base.noiseTracker.forgetTime = 320;
		base.noiseTracker.ignoreSeenNoises = false;
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		if (base.threatTracker != null)
		{
			FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.0025f));
			base.utilityComparer.AddComparedModule(base.threatTracker, smoother, 0.4f, 1.1f);
		}
		if (base.preyTracker != null)
		{
			FloatTweener.FloatTween smoother2 = new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.15f);
			base.utilityComparer.AddComparedModule(base.preyTracker, smoother2, 0.4f, 1.1f);
		}
		if (base.noiseTracker != null)
		{
			base.utilityComparer.AddComparedModule(base.noiseTracker, null, 0.2f, 1.2f);
		}
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
	}

	public override void NewRoom(Room room)
	{
		timeInThisRoom = 0;
		base.NewRoom(room);
	}

	public Behavior DetermineBehavior()
	{
		if ((creature.realizedCreature as Loach).State.isChasing)
		{
			return Behavior.Hunt;
		}
		Behavior behavior = Behavior.Idle;
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			behavior = ((aIModule is ThreatTracker) ? Behavior.Flee : ((aIModule is RainTracker) ? Behavior.EscapeRain : ((aIModule is PreyTracker) ? Behavior.Hunt : ((aIModule is NoiseTracker) ? Behavior.InvestigateSound : ((!(aIModule is StuckTracker)) ? Behavior.Idle : Behavior.GetUnstuck)))));
		}
		if (currentUtility < 0.05f)
		{
			currentUtility = 0.05f;
			behavior = Behavior.Idle;
		}
		if (behavior != Behavior.Flee && realizedCreature.grasps != null && realizedCreature.grasps[0] != null && ((realizedCreature.grasps[0].grabbed is Creature && DynamicRelationship((realizedCreature.grasps[0].grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats) || !(realizedCreature.grasps[0].grabbed is Creature)))
		{
			behavior = Behavior.ReturnPrey;
			currentUtility = 1f;
		}
		if (behavior != Behavior.ReturnPrey && behavior != Behavior.EscapeRain && behavior != Behavior.Flee && currentUtility < TravelUtility)
		{
			currentUtility = TravelUtility;
			behavior = Behavior.Travelling;
		}
		return behavior;
	}

	public void GoToSlimeMold(SlimeMold sMold)
	{
		slimeMoldPos = sMold.abstractPhysicalObject.pos;
	}

	public override void Update()
	{
		base.noiseTracker.hearingSkill = 2f;
		if (creature.pos.x > 10 && creature.pos.x < creature.realizedCreature.room.TileWidth - 11)
		{
			timeInThisRoom++;
		}
		if (debugDestinationVisualizer != null)
		{
			debugDestinationVisualizer.Update();
		}
		focusCreature = null;
		base.Update();
		if (realizedCreature.room != null)
		{
			if (ModManager.MSC && realizedCreature.LickedByPlayer != null)
			{
				base.tracker.SeeCreature(realizedCreature.LickedByPlayer.abstractCreature);
			}
			currentUtility = base.utilityComparer.HighestUtility();
			behavior = DetermineBehavior();
			base.stuckTracker.satisfiedWithThisPosition = !realizedCreature.AtSitDestination;
			Act();
		}
	}

	public void Act()
	{
		if (behavior == Behavior.Idle)
		{
			if (!hasSlimeMoldPos)
			{
				if (!base.pathFinder.GetDestination.TileDefined)
				{
					creature.abstractAI.SetDestination(creature.pos);
					idleCounter = 0;
				}
				else
				{
					if (slimeMoldCounter == 0)
					{
						slimeMoldCounter = 1000;
						slimeMolds = new List<SlimeMold>();
						for (int i = 0; i < creature.Room.realizedRoom.physicalObjects.Length; i++)
						{
							foreach (PhysicalObject item in creature.Room.realizedRoom.physicalObjects[i])
							{
								if (item is SlimeMold)
								{
									slimeMolds.Add(item as SlimeMold);
								}
							}
						}
						if (slimeMolds.Count > 0)
						{
							hasSlimeMoldPos = true;
							slimeMoldCounter = 5000;
							slimeMoldPos = realizedCreature.room.GetWorldCoordinate(slimeMolds[UnityEngine.Random.Range(0, slimeMolds.Count - 1)].firstChunk.pos);
						}
					}
					else
					{
						slimeMoldCounter--;
					}
					idleCounter--;
					int num = idleCounter;
					if (IdleSpotScore(creature.abstractAI.destination) == float.MaxValue)
					{
						idleCounter = 0;
					}
					if (Custom.DistLess(creature.realizedCreature.room.MiddleOfTile(creature.realizedCreature.firstChunk.pos), creature.realizedCreature.room.MiddleOfTile(creature.abstractAI.destination), 250f))
					{
						idleCounter -= 40;
					}
					if (idleCounter < 1)
					{
						if (num > 0)
						{
							forbiddenIdleSpot = base.pathFinder.GetDestination;
						}
						bool num2 = realizedCreature.coord.x > realizedCreature.room.TileWidth / 2;
						WorldCoordinate worldCoordinate = IdleRoomWanderGoal();
						if (num2)
						{
							worldCoordinate = new WorldCoordinate(creature.Room.index, UnityEngine.Random.Range(0, realizedCreature.room.TileWidth / 2), UnityEngine.Random.Range(0, realizedCreature.room.TileHeight), -1);
						}
						else
						{
							worldCoordinate = new WorldCoordinate(creature.Room.index, UnityEngine.Random.Range(realizedCreature.room.TileWidth / 2, realizedCreature.room.TileWidth), UnityEngine.Random.Range(0, realizedCreature.room.TileHeight), -1);
						}
						worldCoordinate = IdleRoomWanderGoal();
						if (IdleSpotScore(worldCoordinate) < IdleSpotScore(creature.abstractAI.destination))
						{
							creature.abstractAI.SetDestination(worldCoordinate);
							idleCounter = UnityEngine.Random.Range(2000, 6000);
						}
						else
						{
							idleSpotWinStreak++;
							if (idleSpotWinStreak > 50)
							{
								idleCounter = UnityEngine.Random.Range(1000, 4000);
								idleSpotWinStreak = 0;
							}
						}
					}
				}
			}
			else
			{
				slimeMoldCounter--;
				if (slimeMoldCounter == 0)
				{
					hasSlimeMoldPos = false;
				}
				base.pathFinder.SetDestination(slimeMoldPos);
			}
		}
		else if (behavior == Behavior.Flee)
		{
			WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 1, 30, currentUtility > 0.3f);
			if (base.threatTracker.mostThreateningCreature != null)
			{
				focusCreature = base.threatTracker.mostThreateningCreature;
			}
			creature.abstractAI.SetDestination(destination);
		}
		else if (behavior == Behavior.Travelling)
		{
			creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
		}
		else if (behavior == Behavior.Hunt)
		{
			Tracker.CreatureRepresentation mostAttractivePrey = base.preyTracker.MostAttractivePrey;
			if (mostAttractivePrey != null)
			{
				creature.abstractAI.SetDestination(mostAttractivePrey.BestGuessForPosition());
				focusCreature = mostAttractivePrey;
			}
		}
		else if (behavior == Behavior.EscapeRain)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.InvestigateSound)
		{
			if (lastBehavior != Behavior.InvestigateSound)
			{
				creature.realizedCreature.room.PlaySound(WatcherEnums.WatcherSoundID.Loach_Curious, creature.realizedCreature.firstChunk);
			}
			creature.abstractAI.SetDestination(base.noiseTracker.ExaminePos);
		}
		else if (behavior == Behavior.GetUnstuck)
		{
			creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
		}
		lastBehavior = behavior;
	}

	public WorldCoordinate IdleRoomWanderGoal()
	{
		if (realizedCreature.room == null)
		{
			return realizedCreature.abstractCreature.pos;
		}
		float minInclusive = 0.85f;
		float maxInclusive = 1f;
		float num = realizedCreature.flipDir * -1f;
		if (UnityEngine.Random.value < 0.4f)
		{
			num = realizedCreature.flipDir;
		}
		int num2 = UnityEngine.Random.Range(16, 25);
		WorldCoordinate wanderCoordFromXPosition;
		do
		{
			int num3 = Mathf.Clamp((int)((float)realizedCreature.abstractCreature.pos.x + num * ((float)realizedCreature.room.TileWidth * UnityEngine.Random.Range(minInclusive, maxInclusive))), num2, realizedCreature.room.TileWidth - num2);
			if (Mathf.Abs(realizedCreature.abstractCreature.pos.x - num3) < 22)
			{
				num *= -1f;
				num3 = Mathf.Clamp((int)((float)realizedCreature.abstractCreature.pos.x + num * ((float)realizedCreature.room.TileWidth * UnityEngine.Random.Range(minInclusive, maxInclusive))), num2, realizedCreature.room.TileWidth - num2);
			}
			wanderCoordFromXPosition = GetWanderCoordFromXPosition(num3);
			num2 += 7;
		}
		while (num2 <= 80 && realizedCreature.room.GetTile(wanderCoordFromXPosition).Solid);
		return wanderCoordFromXPosition;
	}

	public WorldCoordinate GetWanderCoordFromXPosition(int tileX)
	{
		int i = 0;
		int num = realizedCreature.abstractCreature.pos.y;
		for (; i < 2000; i += 20)
		{
			if (LegalInRoomDest(realizedCreature.room.GetWorldCoordinate(new Vector2((float)tileX * 20f, i))))
			{
				num = Mathf.Clamp(i / 20 + (int)realizedCreature.preferredHeight + UnityEngine.Random.Range(-15, 15) - 2, 0, realizedCreature.room.TileHeight - 2);
				if (GoodInRoomDest(realizedCreature.room.GetWorldCoordinate(new Vector2((float)tileX * 20f, (float)num * 20f))))
				{
					break;
				}
			}
		}
		return new WorldCoordinate(realizedCreature.abstractCreature.pos.room, tileX, num, realizedCreature.abstractCreature.pos.abstractNode);
	}

	public bool GoodInRoomDest(WorldCoordinate rndm)
	{
		if (!LegalInRoomDest(rndm))
		{
			return false;
		}
		if (realizedCreature.room.TileWidth > 180)
		{
			if (Math.Abs(creature.pos.Tile.x - rndm.Tile.x) < realizedCreature.room.TileWidth / 3)
			{
				return false;
			}
			if (rndm.Tile.x > 70 && rndm.Tile.x < realizedCreature.room.TileWidth - 70)
			{
				return false;
			}
		}
		return true;
	}

	public bool LegalInRoomDest(WorldCoordinate rndm)
	{
		if (rndm.room != creature.pos.room || !rndm.TileDefined)
		{
			return false;
		}
		if (!base.pathFinder.CoordinateReachableAndGetbackable(rndm))
		{
			return false;
		}
		if (Custom.DistLess(creature.pos.Tile, rndm.Tile, 10f))
		{
			return false;
		}
		if (rndm.x < 5 || rndm.x > realizedCreature.room.TileWidth - 5)
		{
			return false;
		}
		return true;
	}

	public float IdleSpotScore(WorldCoordinate coord)
	{
		float num = 10f;
		if (coord.room != creature.pos.room || !coord.TileDefined)
		{
			return 50f;
		}
		if (!realizedCreature.room.aimap.WorldCoordinateAccessibleToCreature(coord, creature.creatureTemplate) || !base.pathFinder.CoordinateReachableAndGetbackable(coord))
		{
			return float.MaxValue;
		}
		num /= Mathf.Lerp(realizedCreature.room.aimap.getAItile(coord.Tile).visibility, 1f, 0.9995f);
		num -= Mathf.Max(0f, coord.Tile.FloatDist(creature.pos.Tile) - 30f) * 1.5f;
		return num + (float)realizedCreature.room.aimap.getAItile(coord).floorAltitude * 10f;
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed);
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		if (realizedCreature.graphicsModule != null)
		{
			(realizedCreature.graphicsModule as LoachGraphics).creatureLooker.ReevaluateLookObject(creatureRep, 2f);
		}
	}

	public float CreatureUnease(AbstractCreature crit)
	{
		if (crit.state.dead || realizedCreature.Template.CreatureRelationship(crit.creatureTemplate).type == CreatureTemplate.Relationship.Type.Ignores)
		{
			return 0f;
		}
		float num = 1f;
		num *= crit.creatureTemplate.bodySize;
		num *= Mathf.Lerp(realizedCreature.abstractCreature.creatureTemplate.CreatureRelationship(crit.creatureTemplate).intensity, 1f, 0.5f);
		if (crit.realizedCreature != null && !crit.realizedCreature.Consious)
		{
			num /= 20f;
		}
		if (realizedCreature.room.GetTile(realizedCreature.mainBodyChunk.pos).DeepWater && crit.creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.AirOnly)
		{
			num /= 3f;
		}
		else if (!realizedCreature.room.GetTile(realizedCreature.mainBodyChunk.pos).AnyWater && crit.creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.WaterOnly)
		{
			num /= 3f;
		}
		return num;
	}

	public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (base.threatTracker != null && relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		if (base.preyTracker != null && (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Attacks))
		{
			return base.preyTracker;
		}
		return null;
	}

	public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return creature.creatureTemplate.CreatureRelationship(dRelation.trackerRep.representedCreature.creatureTemplate);
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		float num = realizedCreature.room.aimap.getTerrainProximity(connection.destinationCoord);
		return new PathCost(cost.resistance + Mathf.Clamp(Mathf.Abs(realizedCreature.preferredHeight - num), 0f, 15f) * 0.3f, cost.legality);
	}
}
