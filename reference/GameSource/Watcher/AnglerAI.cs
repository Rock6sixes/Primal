using RWCustom;
using UnityEngine;

namespace Watcher;

public class AnglerAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior Flee = new Behavior("Flee", register: true);

		public static Behavior Hunt = new Behavior("Hunt", register: true);

		public static Behavior Stalk = new Behavior("Stalk", register: true);

		public static Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Angler fish;

	public Vector2? fgp;

	public WorldCoordinate? exploreCoordinate;

	public DebugDestinationVisualizer debugDestinationVisualizer;

	public float currentUtility;

	public Tracker.CreatureRepresentation lockedCreature;

	public int angerTimer;

	public int preyAwayTimer;

	public Behavior behavior;

	public DebugSprite dbSprite;

	public Vector2? floatGoalPos
	{
		get
		{
			return fgp;
		}
		set
		{
			fgp = value;
			if (value.HasValue)
			{
				creature.abstractAI.SetDestination(fish.room.GetWorldCoordinate(value.Value));
			}
		}
	}

	public override float CurrentPlayerAggression(AbstractCreature player)
	{
		if (behavior == Behavior.Idle)
		{
			return 0.2f;
		}
		if (behavior == Behavior.Stalk)
		{
			return 0f;
		}
		if (behavior == Behavior.Hunt)
		{
			return 1f;
		}
		return 0f;
	}

	public AnglerAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		fish = creature.realizedCreature as Angler;
		fish.AI = this;
		AddModule(new FishPather(this, world, creature));
		AddModule(new Tracker(this, 10, 10, 1000, 0.5f, 5, 5, 20));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: false));
		AddModule(new DiscomfortTracker(this, base.tracker, 0f));
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		base.stuckTracker.AddSubModule(new StuckTracker.StuckCloseToShortcutModule(base.stuckTracker));
		base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new RelationshipTracker.TrackedCreatureState();
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
	}

	public override void Update()
	{
		if (fish.graphicsModule != null)
		{
			(fish.graphicsModule as AnglerGraphics).angerRiskTarget = 0f;
		}
		base.Update();
		if (angerTimer > 0 && (lockedCreature == null || !lockedCreature.VisualContact))
		{
			angerTimer--;
			if (angerTimer == 0)
			{
				lockedCreature = null;
			}
		}
		base.utilityComparer.GetUtilityTracker(base.threatTracker).weight = ((behavior == Behavior.Stalk) ? 0f : 1f);
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
			else if (aIModule is StuckTracker)
			{
				behavior = Behavior.GetUnstuck;
			}
		}
		if (currentUtility < 0.1f)
		{
			behavior = Behavior.Idle;
		}
		if (lockedCreature != null)
		{
			if (lockedCreature.deleteMeNextFrame)
			{
				lockedCreature = null;
			}
			else if (lockedCreature.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Antagonizes)
			{
				behavior = Behavior.Stalk;
			}
			else if (behavior != Behavior.GetUnstuck)
			{
				behavior = Behavior.Hunt;
			}
		}
		else
		{
			preyAwayTimer = 0;
		}
		if (fish.safariControlled)
		{
			if (angerTimer > 0)
			{
				fish.swimSpeedGoal = 1f;
			}
			else
			{
				Angler angler = fish;
				ref Player.InputPackage? inputWithDiagonals = ref fish.inputWithDiagonals;
				angler.swimSpeedGoal = ((inputWithDiagonals.HasValue && inputWithDiagonals.GetValueOrDefault().AnyDirectionalInput) ? 0.25f : 0f);
			}
		}
		else if (behavior == Behavior.Idle)
		{
			if (angerTimer > 0)
			{
				if (fish.jawState == 0)
				{
					fish.jawState++;
				}
			}
			else if (lockedCreature == null && fish.jawState == 2)
			{
				fish.jawState = 0;
			}
			fish.swimSpeedGoal = ((angerTimer > 0) ? 1f : 0.25f);
			if (exploreCoordinate.HasValue)
			{
				creature.abstractAI.SetDestination(exploreCoordinate.Value);
				if (Custom.ManhattanDistance(creature.pos, exploreCoordinate.Value) < 5 || (Random.value < 0.0125f && base.pathFinder.DoneMappingAccessibility && fish.room.aimap.TileAccessibleToCreature(creature.pos.x, creature.pos.y, creature.creatureTemplate) && !base.pathFinder.CoordinateReachableAndGetbackable(exploreCoordinate.Value)))
				{
					exploreCoordinate = null;
				}
			}
			else if (Custom.ManhattanDistance(creature.pos, base.pathFinder.GetDestination) < 5 || !base.pathFinder.CoordinateReachableAndGetbackable(base.pathFinder.GetDestination))
			{
				WorldCoordinate worldCoordinate = fish.room.GetWorldCoordinate(Custom.RestrictInRect(fish.mainBodyChunk.pos, new FloatRect(0f, 0f, fish.room.PixelWidth, fish.room.PixelHeight)) + Custom.RNV() * 200f);
				if (fish.room.IsPositionInsideBoundries(worldCoordinate.Tile) && base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate) && fish.room.VisualContact(creature.pos.Tile, worldCoordinate.Tile))
				{
					creature.abstractAI.SetDestination(worldCoordinate);
				}
			}
			if (Random.value < 1f / (exploreCoordinate.HasValue ? 1600f : 80f))
			{
				WorldCoordinate worldCoordinate2 = new WorldCoordinate(fish.room.abstractRoom.index, Random.Range(0, fish.room.TileWidth), Random.Range(0, fish.room.TileHeight), -1);
				if (fish.room.aimap.TileAccessibleToCreature(worldCoordinate2.Tile, creature.creatureTemplate) && base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate2))
				{
					exploreCoordinate = worldCoordinate2;
				}
			}
			if (Custom.DistLess(creature.pos, base.pathFinder.GetDestination, 20f) && fish.room.VisualContact(creature.pos, base.pathFinder.GetDestination))
			{
				floatGoalPos = fish.room.MiddleOfTile(base.pathFinder.GetDestination);
			}
			else
			{
				floatGoalPos = null;
			}
		}
		else if (behavior == Behavior.Flee)
		{
			fish.swimSpeedGoal = 0.5f;
			WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 3, 30, currentUtility > 0.3f);
			creature.abstractAI.SetDestination(destination);
			floatGoalPos = null;
		}
		else if (behavior == Behavior.EscapeRain)
		{
			fish.swimSpeedGoal = 0.5f;
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			floatGoalPos = null;
		}
		else if (behavior == Behavior.Hunt)
		{
			fish.swimSpeedGoal = 1f;
			if (lockedCreature.VisualContact)
			{
				floatGoalPos = lockedCreature.representedCreature.realizedCreature.mainBodyChunk.pos;
			}
			else
			{
				creature.abstractAI.SetDestination(lockedCreature.BestGuessForPosition());
			}
			if (fish.jawState == 0)
			{
				fish.jawState = 1;
			}
		}
		else if (behavior == Behavior.Stalk)
		{
			if (behavior == Behavior.Stalk && lockedCreature.representedCreature.realizedCreature != null)
			{
				Vector2 pos = lockedCreature.representedCreature.realizedCreature.firstChunk.pos;
				fish.swimSpeedGoal = 0.29f * Mathf.Min(Vector2.Distance(fish.firstChunk.pos, pos) - 80f, 85f) / 85f;
			}
			if (lockedCreature.VisualContact)
			{
				floatGoalPos = lockedCreature.representedCreature.realizedCreature.mainBodyChunk.pos;
			}
			else
			{
				creature.abstractAI.SetDestination(lockedCreature.BestGuessForPosition());
			}
		}
		else if (behavior == Behavior.GetUnstuck)
		{
			fish.swimSpeedGoal = 0.4f;
			creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
			if (Random.value < Custom.LerpMap(base.stuckTracker.Utility(), 0.9f, 1f, 0f, 0.1f) && fish.room.GetTile(fish.mainBodyChunk.pos).AnyWater && !fish.enteringShortCut.HasValue && base.stuckTracker.stuckCloseToShortcutModule.foundShortCut.HasValue)
			{
				fish.enteringShortCut = base.stuckTracker.stuckCloseToShortcutModule.foundShortCut;
				base.stuckTracker.Reset();
			}
			floatGoalPos = null;
		}
		if (creature.Room.world.game.devToolsActive && Input.GetKey(KeyCode.M))
		{
			fish.swimSpeedGoal = 0.3f;
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			floatGoalPos = null;
		}
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed) - Mathf.Pow(Mathf.InverseLerp(1f, -1f, Vector2.Dot((fish.bodyChunks[1].pos - fish.bodyChunks[0].pos).normalized, (fish.bodyChunks[1].pos - lookAtPoint).normalized)), 0.85f);
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
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

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		return null;
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.VisualContact)
		{
			dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Barnacle && dRelation.trackerRep.representedCreature.state.miscWatcherSaveFlags == 1)
		{
			result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.2f);
		}
		CreatureTemplate.Relationship.Type type = result.type;
		if (type == CreatureTemplate.Relationship.Type.Antagonizes)
		{
			if (lockedCreature == null || lockedCreature == dRelation.trackerRep)
			{
				bool flag = false;
				float num = 80f;
				float num2 = 0f;
				if (dRelation.trackerRep.representedCreature.realizedCreature == null)
				{
					preyAwayTimer = 0;
				}
				else if (!fish.safariControlled || fish.safariLightInput)
				{
					bool flag2 = false;
					if (dRelation.trackerRep.representedCreature.realizedCreature is Player { IsHidden: not false })
					{
						flag2 = true;
					}
					if (lockedCreature == dRelation.trackerRep)
					{
						result.intensity = 1f;
					}
					flag = fish.room == dRelation.trackerRep.representedCreature.realizedCreature.room && !dRelation.trackerRep.representedCreature.realizedCreature.inShortcut;
					bool flag3 = fish.room == dRelation.trackerRep.representedCreature.realizedCreature.room && fish.room.VisualContact(fish.GlowPos, dRelation.trackerRep.representedCreature.realizedCreature.mainBodyChunk.pos) && !flag2;
					num = ((dRelation.trackerRep.VisualContact || flag3) ? 170f : 80f);
					bool flag4 = preyAwayTimer >= 44;
					num2 = Vector2.Distance(fish.GlowPos, dRelation.trackerRep.representedCreature.realizedCreature.firstChunk.pos);
					if (flag4 || angerTimer > 0)
					{
						preyAwayTimer = 0;
						result.type = CreatureTemplate.Relationship.Type.Eats;
						if (flag4)
						{
							angerTimer = Random.Range(200, 800);
							(fish.graphicsModule as AnglerGraphics)?.Shriek();
						}
					}
					else if (num2 >= num || flag2)
					{
						if (lockedCreature != null)
						{
							preyAwayTimer++;
						}
					}
					else if (flag3)
					{
						lockedCreature = dRelation.trackerRep;
						preyAwayTimer = 0;
					}
					if (!flag)
					{
						preyAwayTimer = 0;
					}
				}
				if (fish.graphicsModule != null && lockedCreature == dRelation.trackerRep)
				{
					if (!flag)
					{
						(fish.graphicsModule as AnglerGraphics).angerRiskTarget = 0f;
					}
					else
					{
						(fish.graphicsModule as AnglerGraphics).angerRiskTarget = Mathf.Clamp01(Mathf.Max(num2 / num, (float)preyAwayTimer / 44f));
					}
				}
			}
			else
			{
				result.intensity *= ((lockedCreature == null) ? 1f : 0f);
			}
		}
		if (type == CreatureTemplate.Relationship.Type.Afraid && !dRelation.state.alive)
		{
			result.intensity = 0f;
		}
		return result;
	}
}
