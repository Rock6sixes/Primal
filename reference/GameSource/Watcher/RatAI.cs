using Noise;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RatAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
	public class AIParams
	{
		public bool fleesFromThreats = true;

		public bool huntsPrey;

		public bool respondsToNoise;
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

	public Rat realizedCreature;

	public float excitement;

	public int panic;

	public int lastDistressLength;

	public float hunger;

	public float fear;

	public float rainFear;

	public float runSpeed;

	public Tracker.CreatureRepresentation focusCreature;

	public int idleCounter;

	public int idleSpotWinStreak;

	public WorldCoordinate forbiddenIdleSpot = new WorldCoordinate(-1, -1, -1, -1);

	public int idleRestlessness;

	public int unableToFindComfortablePosition;

	public float currentUtility;

	public AIParams aiparams;

	public Behavior behavior;

	public AbstractCreature targetDeadCreature;

	public bool setOnDeadCreature;

	public int deadCreatureBoredomCount;

	public int deadCreatureInterest;

	public float CombinedFear => Mathf.Max(fear, Mathf.Pow(rainFear, 3f));

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

	public RatAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		realizedCreature = creature.realizedCreature as Rat;
		realizedCreature.AI = this;
		aiparams = new AIParams();
		AddModule(new StandardPather(this, world, creature));
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
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new DiscomfortTracker(this, base.tracker, 0f));
		AddModule(new UtilityComparer(this));
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
		if (base.denFinder.GetDenPosition().HasValue && base.denFinder.GetDenPosition().Value.CompareDisregardingTile(base.pathFinder.GetDestination))
		{
			creature.abstractAI.SetDestination(new WorldCoordinate(room.abstractRoom.index, Random.Range(0, room.TileWidth), Random.Range(0, room.TileHeight), -1));
		}
		base.NewRoom(room);
	}

	public Behavior DetermineBehavior()
	{
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

	public override void Update()
	{
		base.Update();
		if (realizedCreature.room != null)
		{
			if (ModManager.MSC && realizedCreature.LickedByPlayer != null)
			{
				base.tracker.SeeCreature(realizedCreature.LickedByPlayer.abstractCreature);
			}
			if (panic > 0)
			{
				panic--;
			}
			base.pathFinder.walkPastPointOfNoReturn = stranded || !base.denFinder.GetDenPosition().HasValue || !base.pathFinder.CoordinatePossibleToGetBackFrom(base.denFinder.GetDenPosition().Value);
			fear = ((base.threatTracker != null) ? base.utilityComparer.GetSmoothedNonWeightedUtility(base.threatTracker) : 0f);
			hunger = ((base.preyTracker != null) ? base.utilityComparer.GetSmoothedNonWeightedUtility(base.preyTracker) : 0f);
			rainFear = base.utilityComparer.GetSmoothedNonWeightedUtility(base.rainTracker);
			excitement = Mathf.Lerp(excitement, Mathf.Max(hunger, CombinedFear), 0.1f);
			if (fear > 0.8f)
			{
				lastDistressLength = Custom.IntClamp(lastDistressLength + 1, 0, 500);
			}
			else if (fear < 0.2f)
			{
				lastDistressLength = 0;
			}
			if (base.threatTracker != null)
			{
				((base.utilityComparer.GetUtilityTracker(base.threatTracker).smoother as FloatTweener.FloatTweenUpAndDown).down as FloatTweener.FloatTweenBasic).speed = 1f / ((float)(lastDistressLength + 20) * 3f);
			}
			currentUtility = base.utilityComparer.HighestUtility();
			behavior = DetermineBehavior();
			Act();
		}
	}

	public void Act()
	{
		if (realizedCreature.safariControlled)
		{
			if (realizedCreature.inputWithDiagonals.HasValue)
			{
				Player.InputPackage value = realizedCreature.inputWithDiagonals.Value;
				if (!value.AnyDirectionalInput)
				{
					runSpeed = Mathf.Lerp(runSpeed, 0f, 0.8f);
				}
				else if (value.jmp)
				{
					runSpeed = Mathf.Lerp(runSpeed, 1f, 0.1f);
				}
				else
				{
					runSpeed = Mathf.Lerp(runSpeed, 0.25f, 0.5f);
				}
			}
		}
		else if (behavior == Behavior.Flee)
		{
			targetDeadCreature = null;
			setOnDeadCreature = false;
			if (realizedCreature.packLeader != null)
			{
				realizedCreature.room.PlaySound(WatcherEnums.WatcherSoundID.Rat_Scramble, realizedCreature.mainBodyChunk);
			}
			realizedCreature.packLeader = null;
			WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 5, 20, fear > 1f / 3f);
			creature.abstractAI.SetDestination(destination);
			if (base.threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: true) > 1f)
			{
				if (panic < 40)
				{
					panic = 40;
				}
				else
				{
					panic += 2;
				}
			}
			runSpeed = Mathf.Lerp(runSpeed, Mathf.Pow(CombinedFear, 0.1f), 0.5f);
			focusCreature = base.threatTracker.mostThreateningCreature;
		}
		else if (behavior == Behavior.Idle)
		{
			bool flag = true;
			int num = idleCounter;
			int num2 = -1;
			for (int i = 0; i < creature.Room.creatures.Count; i++)
			{
				if (creature.Room.creatures[i].realizedCreature == null || !creature.Room.creatures[i].realizedCreature.dead || realizedCreature.deadBoredCreatures.Contains(creature.Room.creatures[i]) || targetDeadCreature != null || !(creature.Room.creatures[i].creatureTemplate.type != WatcherEnums.CreatureTemplateType.Rat))
				{
					continue;
				}
				if (num2 == -1)
				{
					if (DeadCreatureScore(creature.Room.creatures[i]) != float.MaxValue)
					{
						num2 = i;
						targetDeadCreature = creature.Room.creatures[i];
					}
				}
				else if (DeadCreatureScore(creature.Room.creatures[i]) > DeadCreatureScore(creature.Room.creatures[num2]))
				{
					num2 = i;
					targetDeadCreature = creature.Room.creatures[i];
				}
			}
			if (targetDeadCreature != null)
			{
				creature.abstractAI.SetDestination(new WorldCoordinate(targetDeadCreature.pos.room, targetDeadCreature.pos.x + Random.Range(-1, 1), targetDeadCreature.pos.y, targetDeadCreature.pos.abstractNode));
				setOnDeadCreature = Custom.DistLess(realizedCreature.firstChunk.pos, targetDeadCreature.realizedCreature.firstChunk.pos, 50f);
				if (Custom.DistLess(realizedCreature.firstChunk.pos, targetDeadCreature.realizedCreature.firstChunk.pos, 25f))
				{
					deadCreatureBoredomCount++;
					if (deadCreatureBoredomCount >= 100)
					{
						setOnDeadCreature = false;
						deadCreatureBoredomCount = 0;
						realizedCreature.deadBoredCreatures.Add(targetDeadCreature);
						targetDeadCreature = null;
					}
				}
				else
				{
					setOnDeadCreature = false;
					deadCreatureBoredomCount = 0;
				}
				return;
			}
			if (realizedCreature.packLeader != null && !setOnDeadCreature)
			{
				creature.abstractAI.SetDestination(new WorldCoordinate(realizedCreature.packLeader.abstractCreature.pos.room, realizedCreature.packLeader.abstractCreature.pos.x + Random.Range(-5, 5), realizedCreature.packLeader.abstractCreature.pos.y, realizedCreature.packLeader.abstractCreature.pos.abstractNode));
				return;
			}
			idleCounter--;
			if (creature.pos.room == base.pathFinder.GetDestination.room)
			{
				if (!base.pathFinder.GetDestination.TileDefined)
				{
					creature.abstractAI.SetDestination(creature.pos);
					idleCounter = 0;
				}
				else if (Custom.ManhattanDistance(creature.pos.Tile, base.pathFinder.GetDestination.Tile) < 5)
				{
					idleRestlessness--;
					flag = idleCounter < 1 || idleRestlessness > 0;
					if (!flag && !ComfortableIdlePosition())
					{
						unableToFindComfortablePosition++;
						flag = true;
					}
					else
					{
						unableToFindComfortablePosition = 0;
					}
					if (unableToFindComfortablePosition > 40)
					{
						idleCounter = 0;
						unableToFindComfortablePosition = 0;
					}
					if (idleRestlessness < 40 && Random.value < 0.0016666667f)
					{
						idleRestlessness = Random.Range(1, 40);
					}
				}
			}
			if (IdleSpotScore(creature.abstractAI.destination) == float.MaxValue)
			{
				idleCounter = 0;
			}
			if (idleCounter < 1)
			{
				if (num > 0)
				{
					forbiddenIdleSpot = base.pathFinder.GetDestination;
				}
				WorldCoordinate worldCoordinate = new WorldCoordinate(creature.Room.index, Random.Range(0, realizedCreature.room.TileWidth), Random.Range(0, realizedCreature.room.TileHeight), -1);
				if (IdleSpotScore(worldCoordinate) < IdleSpotScore(creature.abstractAI.destination) + Custom.LerpMap(idleCounter, 0f, 300f, 100f, 300f))
				{
					creature.abstractAI.SetDestination(worldCoordinate);
					idleCounter = Random.Range(200, 600);
					idleSpotWinStreak = 0;
				}
			}
			if (flag)
			{
				if (!forbiddenIdleSpot.CompareDisregardingNode(creature.abstractAI.destination))
				{
					runSpeed = Mathf.Lerp(runSpeed, 0.25f, 0.5f);
				}
			}
			else
			{
				runSpeed = Mathf.Lerp(runSpeed, 0f, 0.5f);
			}
			focusCreature = null;
		}
		else if (behavior == Behavior.EscapeRain)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			runSpeed = Mathf.Lerp(runSpeed, 1f, 0.1f);
		}
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed);
	}

	public float IdleSpotScore(WorldCoordinate coord)
	{
		if (coord.room != creature.pos.room || !coord.TileDefined)
		{
			return 50f;
		}
		if (!realizedCreature.room.aimap.WorldCoordinateAccessibleToCreature(coord, creature.creatureTemplate) || !base.pathFinder.CoordinateReachableAndGetbackable(coord) || coord.CompareDisregardingNode(forbiddenIdleSpot))
		{
			return float.MaxValue;
		}
		float num = 10f;
		num /= Mathf.Lerp(realizedCreature.room.aimap.getAItile(coord.Tile).visibility, 1f, 0.9995f);
		if (realizedCreature.room.aimap.getAItile(coord).narrowSpace)
		{
			num -= 100f;
		}
		if (realizedCreature.room.GetTile(coord).AnyWater)
		{
			num += 100f;
		}
		if (base.threatTracker != null && base.threatTracker.Utility() > 0f)
		{
			num += Mathf.Clamp(base.threatTracker.ThreatOfArea(coord, accountThreatCreatureAccessibility: true), 0f, 100f);
		}
		if (base.discomfortTracker != null)
		{
			num += 1000f * base.discomfortTracker.DiscomfortOfTile(coord);
		}
		return num;
	}

	public float DeadCreatureScore(AbstractCreature creature)
	{
		WorldCoordinate pos = creature.pos;
		if (pos.room != creature.pos.room || !pos.TileDefined)
		{
			return 50f;
		}
		if (!realizedCreature.room.aimap.WorldCoordinateAccessibleToCreature(pos, creature.creatureTemplate) || !base.pathFinder.CoordinateReachableAndGetbackable(pos) || pos.CompareDisregardingNode(forbiddenIdleSpot))
		{
			return float.MaxValue;
		}
		float num = 10f + Custom.Dist(realizedCreature.firstChunk.pos, creature.realizedCreature.firstChunk.pos);
		if (realizedCreature.room.aimap.getAItile(pos).narrowSpace)
		{
			num += 10f;
		}
		if (realizedCreature.room.GetTile(pos).AnyWater)
		{
			realizedCreature.deadBoredCreatures.Add(creature);
			return float.MaxValue;
		}
		if (base.threatTracker != null && base.threatTracker.Utility() > 0f)
		{
			num += Mathf.Clamp(base.threatTracker.ThreatOfArea(pos, accountThreatCreatureAccessibility: true), 0f, 100f);
		}
		return num;
	}

	public bool ComfortableIdlePosition()
	{
		if (realizedCreature.room.aimap.getAItile(realizedCreature.bodyChunks[0].pos).narrowSpace)
		{
			return true;
		}
		if (realizedCreature.room.GetTile(realizedCreature.bodyChunks[0].pos).AnyWater)
		{
			return false;
		}
		return true;
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		cost = base.TravelPreference(connection, cost);
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(i);
			if (Custom.ManhattanDistance(connection.startCoord, rep.BestGuessForPosition()) < 3)
			{
				if (Custom.ManhattanDistance(creature.pos, rep.BestGuessForPosition()) < 3)
				{
					cost.resistance += 100f;
				}
				else
				{
					cost.resistance += 10f + 90f * CreatureUnease(rep.representedCreature);
				}
			}
		}
		cost.resistance += Random.value;
		return cost;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return behavior == Behavior.EscapeRain;
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
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Barnacle && dRelation.trackerRep.representedCreature.state.miscWatcherSaveFlags == 1)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
		}
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Frog && dRelation.trackerRep.representedCreature.state is FrogState { creatureAttachedTo: var creatureAttachedTo } frogState && (creatureAttachedTo == creature.ID || frogState.bloodBank))
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
		}
		return creature.creatureTemplate.CreatureRelationship(dRelation.trackerRep.representedCreature.creatureTemplate);
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
	}
}
