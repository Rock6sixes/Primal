using RWCustom;
using UnityEngine;

namespace Watcher;

public class DrillCrabAI : ArtificialIntelligence, ILookingAtCreatures, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior Hide = new Behavior("Hide", register: true);

		public static Behavior Flee = new Behavior("Flee", register: true);

		public static Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public CreatureLooker creatureLooker;

	public Behavior behavior;

	public Tracker.CreatureRepresentation fleeFromCreature;

	public bool allowedToExitRoom;

	public int idleTimer;

	public int timeSinceFear;

	public int kneelTimer;

	public FloodFillPather pathFinderOverride;

	public DrillCrab Crab => creature.realizedObject as DrillCrab;

	public DrillCrabAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		pathFinderOverride = (creature.abstractAI as DrillCrabAbstractAI).pathFinderOverride;
		if (pathFinderOverride == null)
		{
			AddModule(new DrillCrabPather(this, world, creature));
			AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: true));
		}
		AddModule(new Tracker(this, 10, 5, 320, 0.3f, 5, 5, 10));
		AddModule(new ThreatTracker(this, 5));
		AddModule(new DiscomfortTracker(this, base.tracker, 1f));
		AddModule(new NoiseTracker(this, base.tracker));
		AddModule(new RainTracker(this));
		AddModule(new UtilityComparer(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new RelationshipTracker(this, base.tracker));
		creatureLooker = new CreatureLooker(this, base.tracker, creature.realizedCreature, 0.1f, 20);
		FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.05f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.025f));
		base.utilityComparer.AddComparedModule(base.threatTracker, smoother, 1f, 1f);
		base.utilityComparer.AddComparedModule(base.discomfortTracker, smoother, 1f, 1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 2f, 1f);
		Crab.ai = this;
		idleTimer = Random.Range(40, 120);
	}

	public override void Update()
	{
		base.Update();
		creatureLooker.Update();
		creature.abstractAI.AbstractBehavior(1);
		behavior = Behavior.Idle;
		if (kneelTimer > 0)
		{
			kneelTimer--;
		}
		if ((Crab.room != null && creature.pos.x > 0 && creature.pos.x < Crab.room.TileWidth - 1) || timeInRoom > 400)
		{
			allowedToExitRoom = true;
		}
		fleeFromCreature = null;
		if (base.utilityComparer.HighestUtility() > 0.1f)
		{
			AIModule aIModule = base.utilityComparer.HighestUtilityModule();
			if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
			else if (aIModule is ThreatTracker threatTracker)
			{
				fleeFromCreature = threatTracker.mostThreateningCreature;
			}
			else if (aIModule is DiscomfortTracker discomfortTracker)
			{
				fleeFromCreature = discomfortTracker.MostUncomfortableCreature();
			}
			if (fleeFromCreature != null)
			{
				Vector2 dangerPos = fleeFromCreature.representedCreature.realizedCreature.DangerPos;
				if (Custom.DistLess(dangerPos, Crab.bodyChunks[0].pos, Crab.Hiding ? 275f : 225f) && Mathf.Abs(dangerPos.x - Crab.bodyChunks[0].pos.x) < (Crab.Hiding ? 140f : 120f) && fleeFromCreature.VisualContact && fleeFromCreature.age > 20)
				{
					behavior = Behavior.Hide;
				}
				else
				{
					behavior = Behavior.Flee;
				}
			}
		}
		if (Crab.hideTimer > 0)
		{
			behavior = Behavior.Hide;
		}
		if (behavior == Behavior.Hide || behavior == Behavior.Flee)
		{
			if (timeSinceFear > 40)
			{
				Crab.room.PlaySound(WatcherEnums.WatcherSoundID.Drill_Crab_Squeal, Crab.mainBodyChunk);
			}
			timeSinceFear = 0;
		}
		else
		{
			timeSinceFear++;
		}
		float to = 1f;
		if (!Crab.safariControlled && (behavior == Behavior.Hide || behavior == Behavior.Flee) && fleeFromCreature != null && fleeFromCreature.VisualContact)
		{
			to = Custom.LerpMap(Vector2.Distance(Crab.mainBodyChunk.pos, fleeFromCreature.representedCreature.realizedCreature.DangerPos), 250f, 150f, 0.75f, 0.25f);
		}
		Crab.speed = Custom.LerpAndTick(Crab.speed, to, 0.05f, 0.05f);
		if (Crab.safariControlled)
		{
			Player.InputPackage? inputWithDiagonals = Crab.inputWithDiagonals;
			if (inputWithDiagonals.HasValue)
			{
				Player.InputPackage valueOrDefault = inputWithDiagonals.GetValueOrDefault();
				if (valueOrDefault.thrw)
				{
					ref Player.InputPackage? lastInputWithDiagonals = ref Crab.lastInputWithDiagonals;
					if (lastInputWithDiagonals.HasValue && !lastInputWithDiagonals.GetValueOrDefault().thrw && Crab.spinOutTimer == 0)
					{
						Crab.hideTimer = Mathf.Min(79, Crab.hideTimer);
						Crab.Hide(spin: true);
					}
				}
				if (!Crab.SpinningOut)
				{
					bool flag = Crab.lastInputWithDiagonals?.pckp ?? false;
					if (valueOrDefault.pckp && !flag)
					{
						if (kneelTimer > 0)
						{
							Crab.Hide(spin: false);
						}
						else
						{
							kneelTimer = 20;
						}
					}
					else if (valueOrDefault.pckp && flag)
					{
						if (Crab.hideTimer > 80)
						{
							Crab.Hide(spin: false);
						}
						else if (kneelTimer > 0)
						{
							kneelTimer = 20;
						}
					}
				}
			}
		}
		else if (behavior == Behavior.Idle && creature.abstractAI.destination.room == creature.pos.room)
		{
			if (idleTimer <= 0)
			{
				IntVector2 intVector = new IntVector2(Random.Range(20, 51), Random.Range(-8, 9));
				if (Random.value < 0.5f)
				{
					intVector.x = -intVector.x;
				}
				WorldCoordinate worldCoordinate = ((Crab.bodyChunks[0].pos.x < 0f || Crab.bodyChunks[0].pos.x > Crab.room.PixelWidth || Random.value < 0.1f) ? Crab.room.GetWorldCoordinate(Crab.room.RandomTile()) : (Crab.room.GetWorldCoordinate(Crab.bodyChunks[0].pos) + intVector));
				if (Crab.room.IsPositionInsideBoundries(worldCoordinate.Tile) && CoordinateReachableAndGetbackable(worldCoordinate) && Crab.room.aimap.getTerrainProximity(worldCoordinate) >= 3)
				{
					SetDestination(worldCoordinate);
					idleTimer = Random.Range(120, 480);
				}
			}
			else
			{
				idleTimer--;
			}
		}
		else if (behavior == Behavior.Idle && Crab.Tower)
		{
			creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
		}
		else if (behavior == Behavior.Flee)
		{
			creature.abstractAI.SetDestination(base.threatTracker.FleeTo(creature.pos, 6, 30, considerLeavingRoom: false));
		}
		else if (behavior == Behavior.Hide)
		{
			if (fleeFromCreature != null)
			{
				Crab.Hide(base.utilityComparer.HighestUtilityModule() is ThreatTracker && Crab.hideTimer == 0);
			}
		}
		else if (behavior == Behavior.EscapeRain && base.denFinder.GetDenPosition().HasValue)
		{
			creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
		}
		if (Crab.safariControlled || (base.stuckTracker != null && !(base.stuckTracker.Utility() < 0.5f)))
		{
			return;
		}
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(i);
			if (rep.representedCreature.creatureTemplate.type == creature.creatureTemplate.type && rep.representedCreature.personality.dominance > Crab.abstractCreature.personality.dominance && rep.VisualContact && rep.representedCreature.realizedCreature is DrillCrab { Hiding: false } drillCrab)
			{
				Vector2 pos = drillCrab.mainBodyChunk.pos;
				if (Mathf.Abs(Crab.mainBodyChunk.pos.x + 100f * Crab.flip - pos.x) < 150f && Mathf.Sign(drillCrab.flip) != Mathf.Sign(Crab.flip))
				{
					kneelTimer = Mathf.Max(kneelTimer, Random.Range(40, 80));
				}
			}
		}
	}

	public bool CoordinateReachableAndGetbackable(WorldCoordinate coord)
	{
		return pathFinderOverride?.IsReachable(coord) ?? base.pathFinder.CoordinateReachableAndGetbackable(coord);
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		allowedToExitRoom = false;
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score;
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}

	public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return dRelation.currentRelationship;
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Uncomfortable)
		{
			return base.discomfortTracker;
		}
		return null;
	}
}
