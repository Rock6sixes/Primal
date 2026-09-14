using Noise;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class FrogAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
	public class AIParams
	{
		public bool fleesFromThreats = true;

		public bool huntsPrey = true;

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

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class FrogTrackState : RelationshipTracker.TrackedCreatureState
	{
	}

	public Frog realizedCreature;

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

	public Tracker.CreatureRepresentation packLeader;

	public int findLeaderTick;

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

	public FrogAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		realizedCreature = creature.realizedCreature as Frog;
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
		if (realizedCreature.safariControlled)
		{
			ref Player.InputPackage? inputWithDiagonals = ref realizedCreature.inputWithDiagonals;
			if (inputWithDiagonals.HasValue && inputWithDiagonals.GetValueOrDefault().AnyDirectionalInput)
			{
				behavior = Behavior.Travelling;
			}
			return behavior;
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			behavior = ((aIModule is ThreatTracker) ? Behavior.Flee : ((aIModule is RainTracker) ? Behavior.EscapeRain : ((aIModule is PreyTracker) ? Behavior.Hunt : ((!(aIModule is NoiseTracker)) ? Behavior.Idle : Behavior.InvestigateSound))));
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
		if (realizedCreature.grasps[0] != null)
		{
			return;
		}
		if (behavior == Behavior.Flee)
		{
			WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 7, 30, considerLeavingRoom: true);
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
			runSpeed = Mathf.Lerp(runSpeed, Mathf.Pow(panic, 0.1f), 0.5f);
			focusCreature = base.threatTracker.mostThreateningCreature;
		}
		else if (behavior == Behavior.Travelling)
		{
			runSpeed = Mathf.Lerp(runSpeed, 0.5f + 0.5f * TravelUtility, 0.01f);
			creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);
		}
		else if (behavior == Behavior.Hunt)
		{
			if (base.preyTracker.MostAttractivePrey == null)
			{
				return;
			}
			if (base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.grabbedBy.Count > 0 || (base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature is Player && (base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature as Player).Malnourished))
			{
				base.preyTracker.ForgetPrey(base.preyTracker.MostAttractivePrey.representedCreature);
				realizedCreature.huntPrey = null;
				return;
			}
			Tracker.CreatureRepresentation mostAttractivePrey = base.preyTracker.MostAttractivePrey;
			realizedCreature.huntPrey = mostAttractivePrey.representedCreature.realizedCreature;
			BodyChunk bodyChunk = base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks[Random.Range(0, base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks.Length)];
			if (Custom.DistLess(realizedCreature.mainBodyChunk.pos, bodyChunk.pos, 120f) && (realizedCreature.room.aimap.TileAccessibleToCreature(realizedCreature.room.GetTilePosition(realizedCreature.bodyChunks[1].pos - Custom.DirVec(realizedCreature.bodyChunks[1].pos, bodyChunk.pos) * 30f), realizedCreature.Template) || realizedCreature.room.GetTile(realizedCreature.bodyChunks[1].pos - Custom.DirVec(realizedCreature.bodyChunks[1].pos, bodyChunk.pos) * 30f).Solid) && realizedCreature.room.VisualContact(realizedCreature.mainBodyChunk.pos, bodyChunk.pos))
			{
				if (Vector2.Dot((realizedCreature.mainBodyChunk.pos - bodyChunk.pos).normalized, (realizedCreature.bodyChunks[1].pos - realizedCreature.mainBodyChunk.pos).normalized) > 0.2f)
				{
					realizedCreature.InitiateJump(bodyChunk);
				}
				else
				{
					realizedCreature.mainBodyChunk.vel += Custom.DirVec(realizedCreature.mainBodyChunk.pos, bodyChunk.pos) * 2f;
					realizedCreature.bodyChunks[1].vel -= Custom.DirVec(realizedCreature.mainBodyChunk.pos, bodyChunk.pos) * 2f;
				}
			}
			if (mostAttractivePrey != null)
			{
				creature.abstractAI.SetDestination(mostAttractivePrey.BestGuessForPosition());
				focusCreature = mostAttractivePrey;
			}
			runSpeed = Mathf.Lerp(runSpeed, Mathf.Max(hunger, 0.3f), 0.1f);
		}
		else if (behavior == Behavior.Idle)
		{
			findLeaderTick--;
			if (packLeader != null && packLeader.representedCreature.state.alive)
			{
				if (packLeader.representedCreature.Room == creature.Room)
				{
					runSpeed = Mathf.Lerp(runSpeed, 0.25f, 0.5f);
					creature.abstractAI.SetDestination(packLeader.representedCreature.pos);
					return;
				}
				creature.abstractAI.MigrateTo(packLeader.representedCreature.pos);
			}
			else if (findLeaderTick <= 0)
			{
				findLeaderTick = 1600;
				packLeader = PackLeader();
				findLeaderTick -= 30;
			}
			bool flag = true;
			int num = idleCounter;
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
				if (IdleSpotScore(worldCoordinate) < IdleSpotScore(creature.abstractAI.destination))
				{
					creature.abstractAI.SetDestination(worldCoordinate);
					idleSpotWinStreak = 0;
				}
				else
				{
					idleSpotWinStreak++;
					if (idleSpotWinStreak > 50)
					{
						idleCounter = Random.Range(1000, 4000);
						idleSpotWinStreak = 0;
					}
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
		else if (behavior == Behavior.ReturnPrey)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			runSpeed = Mathf.Lerp(runSpeed, Mathf.Pow(Mathf.Max(CombinedFear, 0.5f), 0.5f), 0.1f);
		}
		else if (behavior == Behavior.InvestigateSound)
		{
			creature.abstractAI.SetDestination(base.noiseTracker.ExaminePos);
			runSpeed = Mathf.Lerp(runSpeed, 0.6f, 0.2f);
		}
		if (base.noiseTracker != null)
		{
			base.noiseTracker.hearingSkill = Custom.LerpMap(runSpeed, 0f, 0.6f, 1.3f, 0.6f);
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
			num += 10f;
		}
		if (realizedCreature.room.GetTile(coord).AnyWater)
		{
			if (realizedCreature.lungs < 0.35f)
			{
				return float.MaxValue;
			}
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

	public bool ComfortableIdlePosition()
	{
		if (realizedCreature.room.GetTile(realizedCreature.bodyChunks[0].pos).AnyWater)
		{
			return realizedCreature.abstractCreature.creatureTemplate.waterRelationship != CreatureTemplate.WaterRelationship.AirOnly;
		}
		return true;
	}

	public Tracker.CreatureRepresentation PackLeader()
	{
		Tracker.CreatureRepresentation result = null;
		int num = 0;
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			if (base.tracker.GetRep(i).dynamicRelationship.state.alive && base.tracker.GetRep(i).representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Frog && (base.tracker.GetRep(i).representedCreature.realizedCreature as Frog).bloodBank && (base.tracker.GetRep(i).representedCreature.realizedCreature as Frog).FrogState.feedSize > num)
			{
				num = (base.tracker.GetRep(i).representedCreature.realizedCreature as Frog).FrogState.feedSize;
				result = base.tracker.GetRep(i);
			}
		}
		return result;
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
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Barnacle && dRelation.trackerRep.representedCreature.state.miscWatcherSaveFlags == 1)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
		}
		if (realizedCreature.bloodBank && result.type == CreatureTemplate.Relationship.Type.Eats)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f);
		}
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && (dRelation.trackerRep.representedCreature.realizedCreature as Player).Malnourished)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f);
		}
		return result;
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new FrogTrackState();
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
	}
}
