using RWCustom;
using UnityEngine;

namespace Watcher;

public class MothGrubAI : ArtificialIntelligence, IUseARelationshipTracker, ILookingAtCreatures
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior Flee = new Behavior("Flee", register: true);

		public static Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public MothGrub bug;

	public CreatureLooker creatureLooker;

	public float currentUtility;

	public Behavior behavior;

	public int idlePosCounter;

	public WorldCoordinate tempIdlePos;

	public MothGrubAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		bug = creature.realizedCreature as MothGrub;
		bug.AI = this;
		AddModule(new StandardPather(this, world, creature));
		base.pathFinder.stepsPerFrame = 10;
		AddModule(new Tracker(this, 10, 10, 450, 0.5f, 5, 5, 10));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new NoiseTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.005f));
		base.utilityComparer.AddComparedModule(base.threatTracker, smoother, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
		creatureLooker = new CreatureLooker(this, base.tracker, creature.realizedCreature, 0.0025f, 30);
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		return base.threatTracker;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new RelationshipTracker.TrackedCreatureState();
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (result.type == CreatureTemplate.Relationship.Type.Afraid && !dRelation.state.alive)
		{
			result.intensity = 0f;
		}
		return result;
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		if (crit.representedCreature.creatureTemplate.smallCreature)
		{
			return -1f;
		}
		if (crit.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			return score * 0.75f;
		}
		return score;
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
	}

	public override void Update()
	{
		base.Update();
		creatureLooker.Update();
		if (bug.room == null)
		{
			return;
		}
		base.pathFinder.walkPastPointOfNoReturn = stranded || base.threatTracker.Utility() > 0.95f;
		base.noiseTracker.hearingSkill = 2f;
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is ThreatTracker)
			{
				behavior = Behavior.Flee;
			}
			else if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
		}
		if (currentUtility < 0.4f)
		{
			behavior = Behavior.Idle;
		}
		if (behavior == Behavior.Idle)
		{
			bool flag = base.pathFinder.GetDestination.room != bug.room.abstractRoom.index;
			if (!flag && idlePosCounter <= 0)
			{
				int abstractNode = bug.room.abstractRoom.RandomNodeInRoom().abstractNode;
				if (bug.room.abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit)
				{
					int num = bug.room.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, bug.Template);
					if (num > -1)
					{
						int num2 = bug.room.aimap.ExitDistanceForCreatureAndCheckNeighbours(bug.abstractCreature.pos.Tile, num, bug.Template);
						if (num2 > -1 && num2 < 400)
						{
							AbstractRoom abstractRoom = bug.room.game.world.GetAbstractRoom(bug.room.abstractRoom.connections[abstractNode]);
							if (abstractRoom != null)
							{
								WorldCoordinate worldCoordinate = abstractRoom.RandomNodeInRoom();
								if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
								{
									creature.abstractAI.SetDestination(worldCoordinate);
									idlePosCounter = Random.Range(200, 500);
									flag = true;
								}
							}
						}
					}
				}
			}
			if (!flag)
			{
				WorldCoordinate coord = new WorldCoordinate(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), Random.Range(0, bug.room.TileHeight), -1);
				if (IdleScore(coord) < IdleScore(tempIdlePos))
				{
					tempIdlePos = coord;
				}
				if (IdleScore(tempIdlePos) < IdleScore(base.pathFinder.GetDestination) + Custom.LerpMap(idlePosCounter, 0f, 300f, 100f, -300f))
				{
					SetDestination(tempIdlePos);
					idlePosCounter = Random.Range(200, 800);
					tempIdlePos = new WorldCoordinate(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), Random.Range(0, bug.room.TileHeight), -1);
				}
			}
			idlePosCounter--;
		}
		else if (behavior == Behavior.Flee)
		{
			creature.abstractAI.SetDestination(base.threatTracker.FleeTo(creature.pos, 10, 30, considerLeavingRoom: true));
		}
		else
		{
			_ = behavior == Behavior.EscapeRain;
		}
	}

	public float IdleScore(WorldCoordinate coord)
	{
		if (coord.room != creature.pos.room || !base.pathFinder.CoordinateReachableAndGetbackable(coord) || bug.room.aimap.getAItile(coord).acc >= AItile.Accessibility.Wall)
		{
			return float.MaxValue;
		}
		float num = 1f;
		if (base.pathFinder.CoordinateReachableAndGetbackable(coord + new IntVector2(0, -1)))
		{
			num += 10f;
		}
		if (bug.room.aimap.getAItile(coord).narrowSpace)
		{
			num += 50f;
		}
		num += base.threatTracker.ThreatOfTile(coord, accountThreatCreatureAccessibility: true) * 1000f;
		num += base.threatTracker.ThreatOfTile(bug.room.GetWorldCoordinate((bug.room.MiddleOfTile(coord) + bug.room.MiddleOfTile(creature.pos)) / 2f), accountThreatCreatureAccessibility: true) * 1000f;
		for (int i = 0; i < base.noiseTracker.sources.Count; i++)
		{
			num += Custom.LerpMap(Vector2.Distance(bug.room.MiddleOfTile(coord), base.noiseTracker.sources[i].pos), 40f, 400f, 100f, 0f);
		}
		return num;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
		{
			return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
		}
		return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
	}

	public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		float val = Mathf.Max(0f, base.threatTracker.ThreatOfTile(coord.destinationCoord, accountThreatCreatureAccessibility: false) - base.threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: false));
		return new PathCost(cost.resistance + Custom.LerpMap(val, 0f, 1.5f, 0f, 10000f, 5f), cost.legality);
	}
}
