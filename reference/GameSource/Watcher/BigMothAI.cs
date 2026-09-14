using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BigMothAI : ArtificialIntelligence, IUseARelationshipTracker, ILookingAtCreatures, IAINoiseReaction, IReactToSocialEvents
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior Flee = new Behavior("Flee", register: true);

		public static Behavior Attack = new Behavior("Attack", register: true);

		public static Behavior Eat = new Behavior("Eat", register: true);

		public static Behavior Stalk = new Behavior("Stalk", register: true);

		public static Behavior StowFood = new Behavior("StowFood", register: true);

		public static Behavior SetDownBaby = new Behavior("SetDownBaby", register: true);

		public static Behavior FeedBaby = new Behavior("FeedBaby", register: true);

		public static Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class BigMothTrackState : RelationshipTracker.TrackedCreatureState
	{
		public int thrownTimer;

		public bool vultureMasked;

		public int seenWithoutMask;
	}

	public class ChildTrackerModule : AIModule
	{
		public class TrackedChild
		{
			public ChildTrackerModule owner;

			public Tracker.CreatureRepresentation critRep;

			public TrackedChild(ChildTrackerModule owner, Tracker.CreatureRepresentation critRep)
			{
				this.owner = owner;
				this.critRep = critRep;
			}

			public void Update()
			{
			}
		}

		public List<TrackedChild> children = new List<TrackedChild>();

		public TrackedChild pickedChild;

		public BigMothAI bugAI;

		public BigMoth bug => bugAI.bug;

		public Room room => bugAI.bug.room;

		public ChildTrackerModule(BigMothAI AI)
			: base(AI)
		{
			bugAI = AI;
			children = new List<TrackedChild>();
		}

		public void AddChild(Tracker.CreatureRepresentation creature)
		{
			foreach (TrackedChild child in children)
			{
				if (child.critRep == creature)
				{
					return;
				}
			}
			children.Add(new TrackedChild(this, creature));
		}

		public TrackedChild GetChildToCarry()
		{
			if (children.Count == 0)
			{
				pickedChild = null;
			}
			else if (pickedChild == null)
			{
				for (int num = children.Count - 1; num >= 0; num--)
				{
					if (children[num].critRep.deleteMeNextFrame || children[num].critRep.representedCreature == null || children[num].critRep.representedCreature.slatedForDeletion || (children[num].critRep.representedCreature.realizedCreature != null && children[num].critRep.representedCreature.realizedCreature.slatedForDeletetion) || children[num].critRep.BestGuessForPosition().room != bug.abstractCreature.pos.room)
					{
						bugAI.tracker.ForgetCreature(children[num].critRep.representedCreature);
						children.RemoveAt(num);
					}
				}
				if (children.Count > 0)
				{
					pickedChild = children[Random.Range(0, children.Count)];
				}
			}
			if (pickedChild != null && (pickedChild.critRep == null || pickedChild.critRep.deleteMeNextFrame || pickedChild.critRep.representedCreature == null || pickedChild.critRep.representedCreature.realizedCreature == null || pickedChild.critRep.representedCreature.slatedForDeletion || pickedChild.critRep.representedCreature.realizedCreature.slatedForDeletetion || pickedChild.critRep.BestGuessForPosition().room != bug.abstractCreature.pos.room || pickedChild.critRep.representedCreature.realizedCreature.grabbedBy.Count > 0))
			{
				pickedChild = null;
			}
			return pickedChild;
		}

		public override void Update()
		{
		}
	}

	public BigMoth bug;

	public Tracker.CreatureRepresentation focusCreature;

	public Tracker.CreatureRepresentation lastFocusCreature;

	public ChildTrackerModule childTracker;

	public WorldCoordinate tempIdlePos;

	public WorldCoordinate tempCliffPos;

	public WorldCoordinate tempSafeBabyPos;

	public int giveUpOnCliffTimer;

	public int grabCoolDown;

	public AbstractCreature tiredOfHuntingCreature;

	public CreatureLooker creatureLooker;

	public float currentUtility;

	public float fear;

	public int noiseRectionDelay;

	public int idlePosCounter;

	public int timeToTire;

	public bool tryingToLeaveRoom;

	public Vector2 drinkSpot;

	public List<AbstractCreature> knownThiefs;

	public Behavior behavior;

	public override float CurrentPlayerAggression(AbstractCreature player)
	{
		bool flag = behavior == Behavior.Attack || behavior == Behavior.Eat;
		CreatureTemplate.Relationship relationship = DynamicRelationship(player);
		if (relationship.type != CreatureTemplate.Relationship.Type.Eats && relationship.type != CreatureTemplate.Relationship.Type.Attacks)
		{
			if (!flag)
			{
				return 0f;
			}
			return 0.25f;
		}
		float t = (1f - creature.state.socialMemory.GetTempLike(player.ID)) / 2f;
		return Mathf.Lerp(0.5f, 1f, t) * (flag ? 1f : 0.5f);
	}

	public BigMothAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		bug = creature.realizedCreature as BigMoth;
		bug.AI = this;
		AddModule(new BigMothPather(this, world, creature));
		base.pathFinder.accessibilityStepsPerFrame = 100;
		base.pathFinder.stepsPerFrame = 90;
		base.pathFinder.walkPastPointOfNoReturn = true;
		AddModule(new Tracker(this, 10, 10, 450, 0.5f, 5, 5, 10));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new NoiseTracker(this, base.tracker));
		AddModule(new PreyTracker(this, 5, 1f, 5f, 150f, 0.05f));
		AddModule(new UtilityComparer(this));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: false));
		AddModule(new DiscomfortTracker(this, base.tracker, 0f));
		childTracker = new ChildTrackerModule(this);
		AddModule(childTracker);
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		base.pathFinder.walkPastPointOfNoReturn = true;
		FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.005f));
		base.utilityComparer.AddComparedModule(base.threatTracker, smoother, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.preyTracker, null, 0.4f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		base.noiseTracker.hearingSkill = 0.5f;
		behavior = Behavior.Idle;
		creatureLooker = new CreatureLooker(this, base.tracker, creature.realizedCreature, 0.0025f, 30);
		knownThiefs = new List<AbstractCreature>();
	}

	public bool SmallWantsToEat(PhysicalObject obj)
	{
		if (!(obj is Rat))
		{
			return obj is Fly;
		}
		return true;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		tempCliffPos = default(WorldCoordinate);
		tempSafeBabyPos = default(WorldCoordinate);
		timeToTire = Random.Range(400, 7000);
		if (room.abstractRoom.AttractionForCreature(creature) == AbstractRoom.CreatureRoomAttraction.Forbidden)
		{
			timeToTire /= Random.Range(50, 900);
		}
		giveUpOnCliffTimer = Random.Range(200, 400);
	}

	public override void NewArea(bool strandedFromExits)
	{
		base.NewArea(strandedFromExits);
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		if (crit.representedCreature.creatureTemplate.smallCreature)
		{
			return -1f;
		}
		if (crit.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			return score * 0.5f;
		}
		if (crit == focusCreature)
		{
			return score * 10f;
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

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		creatureLooker.ReevaluateLookObject(creatureRep, (!firstSpot) ? 2f : 6f);
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
		if (Random.value < Mathf.InverseLerp(0f, 400f, noise.strength))
		{
			bug.randomLookAtCreature = false;
			bug.randomLookPos = noise.pos;
			bug.randomLookCounter = Random.Range(50, Mathf.Min(Mathf.RoundToInt(noise.strength * 0.5f), 400));
			bug.shake = Mathf.InverseLerp(50f, 800f, noise.strength) * (Random.value * 0.5f + 0.5f);
		}
	}

	RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return new BigMothTrackState();
	}

	AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		CreatureTemplate.Relationship.Type type = relationship.type;
		if (type == CreatureTemplate.Relationship.Type.Eats)
		{
			return base.preyTracker;
		}
		if (type == CreatureTemplate.Relationship.Type.Afraid || type == CreatureTemplate.Relationship.Type.Attacks || type == CreatureTemplate.Relationship.Type.Antagonizes)
		{
			return base.threatTracker;
		}
		return null;
	}

	CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		if (dRelation.trackerRep.representedCreature.creatureTemplate.smallCreature)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
		}
		AbstractCreature representedCreature = dRelation.trackerRep.representedCreature;
		BigMothTrackState bigMothTrackState = dRelation.state as BigMothTrackState;
		bigMothTrackState.thrownTimer = Mathf.Max((dRelation.state as BigMothTrackState).thrownTimer - 1, 0);
		if (dRelation.trackerRep.VisualContact)
		{
			bigMothTrackState.alive = representedCreature.state.alive;
			if (representedCreature.realizedCreature != null)
			{
				bool flag = false;
				if (representedCreature.realizedCreature.grasps != null)
				{
					for (int i = 0; i < representedCreature.realizedCreature.grasps.Length; i++)
					{
						if (representedCreature.realizedCreature.grasps[i] != null && representedCreature.realizedCreature.grasps[i].grabbed is VultureMask)
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					if (bigMothTrackState.seenWithoutMask < 80)
					{
						bigMothTrackState.vultureMasked = true;
						bigMothTrackState.seenWithoutMask = 0;
					}
				}
				else
				{
					bigMothTrackState.seenWithoutMask++;
					if (bigMothTrackState.vultureMasked && bigMothTrackState.seenWithoutMask >= 200)
					{
						bigMothTrackState.vultureMasked = false;
					}
				}
			}
		}
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Barnacle && dRelation.trackerRep.representedCreature.state.miscWatcherSaveFlags == 1)
		{
			result = ((!bug.Small) ? new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f) : new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		}
		if (dRelation.trackerRep.representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Frog && dRelation.trackerRep.representedCreature.state is FrogState { creatureAttachedTo: var creatureAttachedTo } frogState)
		{
			if (creatureAttachedTo == creature.ID)
			{
				result = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
			}
			else if (frogState.bloodBank)
			{
				result = ((!bug.Small) ? new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f) : new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
			}
		}
		if (dRelation.trackerRep.representedCreature.slatedForDeletion)
		{
			result.type = CreatureTemplate.Relationship.Type.Ignores;
			result.intensity = 0f;
			return result;
		}
		if (representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			result.type = CreatureTemplate.Relationship.Type.Ignores;
			result.intensity = 0f;
			childTracker.AddChild(dRelation.trackerRep);
			return result;
		}
		if (!bug.Small && (result.type == CreatureTemplate.Relationship.Type.Afraid || result.type == CreatureTemplate.Relationship.Type.Antagonizes))
		{
			if (!dRelation.state.alive)
			{
				result.intensity = 0f;
			}
			else
			{
				float tempLike = creature.state.socialMemory.GetTempLike(dRelation.trackerRep.representedCreature.ID);
				if (tempLike > 0.15f)
				{
					result.type = CreatureTemplate.Relationship.Type.Ignores;
				}
				else if (tempLike < -0.9f)
				{
					result.type = CreatureTemplate.Relationship.Type.Eats;
					base.preyTracker.AddPrey(dRelation.trackerRep);
				}
				else if (tempLike < -0.15f)
				{
					result.type = CreatureTemplate.Relationship.Type.Attacks;
				}
			}
			if (bigMothTrackState.vultureMasked)
			{
				result.type = CreatureTemplate.Relationship.Type.Eats;
				base.preyTracker.AddPrey(dRelation.trackerRep);
			}
			else if (bigMothTrackState.thrownTimer > 0)
			{
				result.type = CreatureTemplate.Relationship.Type.Ignores;
				result.intensity = 0f;
			}
		}
		else if (bug.Small && result.type == CreatureTemplate.Relationship.Type.Antagonizes)
		{
			if ((double)creature.state.socialMemory.GetTempLike(dRelation.trackerRep.representedCreature.ID) < -0.2)
			{
				result.type = CreatureTemplate.Relationship.Type.Afraid;
			}
			else if (bug.room.abstractRoom.AttractionForCreature(creature) == AbstractRoom.CreatureRoomAttraction.Forbidden)
			{
				base.tracker.ForgetCreature(creature);
				return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
			}
		}
		if (!dRelation.state.alive && (dRelation.trackerRep.representedCreature.creatureTemplate.meatPoints >= 2 || (representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Rat && representedCreature.realizedCreature != null && representedCreature.realizedCreature.grabbedBy.Count == 0)) && !(representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigMoth) && !(representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.MothGrub))
		{
			result.type = CreatureTemplate.Relationship.Type.Eats;
			result.intensity = Mathf.InverseLerp(0f, 5f, representedCreature.creatureTemplate.meatPoints) * ((representedCreature.state.meatLeft > 0) ? 1f : 0.5f);
			if (representedCreature.realizedCreature != null && SmallWantsToEat(representedCreature.realizedCreature))
			{
				result.intensity = 1f;
			}
			base.preyTracker.AddPrey(dRelation.trackerRep);
		}
		return result;
	}

	public override void Update()
	{
		if (focusCreature != null)
		{
			lastFocusCreature = focusCreature;
		}
		base.Update();
		creatureLooker.Update();
		if (bug.room == null)
		{
			return;
		}
		grabCoolDown = Mathf.Max(grabCoolDown - 1, 0);
		base.utilityComparer.GetUtilityTracker(base.threatTracker).weight = Custom.LerpMap(base.threatTracker.ThreatOfTile(base.creature.pos, accountThreatCreatureAccessibility: true), 0.1f, 2f, 0.1f, 1f, 0.5f);
		base.utilityComparer.GetUtilityTracker(base.stuckTracker).weight = ((base.pathFinder.GetDestination != default(WorldCoordinate) && base.pathFinder.GetDestination.room == bug.room.abstractRoom.index && behavior != Behavior.GetUnstuck && Vector2.Distance(bug.room.MiddleOfTile(base.pathFinder.GetDestination), bug.bodyChunks[2].pos) < 95f) ? 0f : 1f);
		if (bug.Small)
		{
			base.utilityComparer.GetUtilityTracker(base.threatTracker).weight = (((double)base.preyTracker.Utility() > 0.2) ? 0.3f : 1f);
			for (int i = 0; i < base.creature.state.socialMemory.relationShips.Count; i++)
			{
				base.creature.state.socialMemory.relationShips[i].tempLike = Custom.LerpAndTick(base.creature.state.socialMemory.relationShips[i].tempLike, Mathf.Max(base.creature.state.socialMemory.relationShips[i].tempLike, 0f), 0.001f, 0.001f);
			}
		}
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is ThreatTracker)
			{
				if (base.threatTracker.mostThreateningCreature != null && base.threatTracker.mostThreateningCreature.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Afraid)
				{
					if (!bug.Small && ((childTracker.GetChildToCarry() != null && (base.threatTracker.mostThreateningCreature.representedCreature == null || base.threatTracker.mostThreateningCreature.representedCreature.realizedCreature == null || !(base.threatTracker.mostThreateningCreature.representedCreature.realizedCreature is Player) || (bug.abstractCreature.state as BigMoth.BigMothState).hatchedCycle < 0)) || base.threatTracker.mostThreateningCreature.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks))
					{
						behavior = Behavior.Attack;
					}
					else if (bug.Small && base.threatTracker.mostThreateningCreature.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Antagonizes)
					{
						behavior = Behavior.Stalk;
					}
					else
					{
						behavior = Behavior.Idle;
					}
				}
				else
				{
					behavior = Behavior.Flee;
				}
			}
			else if (aIModule is RainTracker)
			{
				behavior = Behavior.EscapeRain;
			}
			else if (aIModule is PreyTracker)
			{
				behavior = Behavior.Eat;
			}
			else if (aIModule is StuckTracker)
			{
				behavior = Behavior.GetUnstuck;
			}
		}
		if (currentUtility < 0.02f && !(behavior == Behavior.Attack) && !(behavior == Behavior.Eat))
		{
			behavior = Behavior.Idle;
		}
		if (!bug.drinkingChunk)
		{
			bug.drinkChunk = null;
		}
		bool flag = false;
		if (!bug.Small && behavior == Behavior.EscapeRain)
		{
			if (childTracker.GetChildToCarry() != null && (bug.grasps[0] == null || !(bug.grasps[0].grabbed is MothGrub)))
			{
				behavior = Behavior.Idle;
				flag = true;
			}
		}
		else
		{
			if (bug.grasps[0] != null && bug.Small)
			{
				behavior = Behavior.StowFood;
			}
			if (!tryingToLeaveRoom && timeInRoom < timeToTire && bug.grasps[0] != null && bug.grasps[0].grabbed is MothGrub)
			{
				behavior = Behavior.SetDownBaby;
			}
			if (!bug.Small && (bug.State as BigMoth.BigMothState).foodStored > 0 && childTracker.GetChildToCarry() != null && (behavior == Behavior.Idle || (behavior == Behavior.Eat && base.preyTracker.MostAttractivePrey != null && base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.dead)))
			{
				behavior = Behavior.FeedBaby;
			}
			else if (bug.drinkChunk != null && bug.drinkChunk.owner is MothGrub)
			{
				bug.ReleaseTongue();
			}
		}
		bool flag2 = false;
		if (behavior == Behavior.Idle && lastFocusCreature != null && !lastFocusCreature.deleteMeNextFrame && lastFocusCreature.representedCreature != null && !lastFocusCreature.representedCreature.slatedForDeletion && lastFocusCreature.representedCreature.realizedCreature != null && lastFocusCreature.representedCreature.realizedCreature.room == bug.room && DynamicRelationship(lastFocusCreature.representedCreature).type != CreatureTemplate.Relationship.Type.Ignores)
		{
			Vector2 pos = lastFocusCreature.representedCreature.realizedCreature.mainBodyChunk.pos;
			if (Vector2.Distance(bug.bodyChunks[0].pos, pos) < 200f)
			{
				behavior = ((DynamicRelationship(lastFocusCreature.representedCreature).type == CreatureTemplate.Relationship.Type.Eats) ? Behavior.Eat : Behavior.Attack);
				focusCreature = lastFocusCreature;
				flag2 = true;
			}
		}
		if (bug.safariControlled)
		{
			Player.InputPackage? inputWithDiagonals = bug.inputWithDiagonals;
			if (inputWithDiagonals.HasValue)
			{
				Player.InputPackage valueOrDefault = inputWithDiagonals.GetValueOrDefault();
				behavior = Behavior.Idle;
				bool flag3 = false;
				bool flag4 = false;
				if (valueOrDefault.pckp && bug.grasps[0] == null)
				{
					flag3 = true;
					flag4 = true;
					bug.shake = Mathf.Max(bug.shake, 0.4f * bug.abstractCreature.personality.aggression);
					if ((double)Random.value < 0.03 * (double)(bug.abstractCreature.personality.dominance * 0.5f + 0.5f))
					{
						bug.shake = 0.2f * bug.abstractCreature.personality.dominance + Random.value * 0.8f;
					}
					focusCreature = FindSafariFocusCreature();
					if (focusCreature != null && focusCreature.representedCreature.realizedCreature != null && bug.grasps[0] == null)
					{
						TryGrab(focusCreature.representedCreature.realizedCreature, bug.safariWalkMode);
					}
				}
				else if (valueOrDefault.pckp && bug.grasps[0]?.grabbed != null)
				{
					bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
					flag3 = true;
					TryGrab(bug.grasps[0].grabbed, bug.safariWalkMode, forceOthersToLetGo: false, forceAllGrabOnGrip: true);
				}
				else if (!valueOrDefault.pckp && !valueOrDefault.thrw && bug.grasps[0]?.grabbed != null)
				{
					ThrowGrabbed((bug.firstChunk.pos.y > bug.room.PixelHeight + 800f) ? (44f + Random.value * 12f) : 0f);
				}
				if (valueOrDefault.thrw)
				{
					flag2 = true;
					flag3 = true;
					focusCreature = FindSafariFocusCreature();
					if (focusCreature != null)
					{
						behavior = ((focusCreature.representedCreature.realizedCreature != null && focusCreature.representedCreature.realizedCreature is MothGrub) ? Behavior.FeedBaby : Behavior.Eat);
					}
					else
					{
						flag4 = true;
					}
				}
				else if (bug.drinkChunk != null)
				{
					bug.ReleaseTongue();
				}
				if (flag4)
				{
					bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
					flag3 = true;
					bug.shake = Mathf.Max(bug.shake, 0.4f * bug.abstractCreature.personality.aggression);
					if ((double)Random.value < 0.03 * (double)(bug.abstractCreature.personality.dominance * 0.5f + 0.5f))
					{
						bug.shake = 0.2f * bug.abstractCreature.personality.dominance + Random.value * 0.8f;
					}
				}
				if (!flag3)
				{
					if (valueOrDefault.AnyDirectionalInput)
					{
						bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.5f, 0.01f, 0.1f);
					}
					else
					{
						bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.5f, 0.01f, 0.1f);
					}
				}
			}
		}
		if (behavior == Behavior.Idle && !bug.safariControlled)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.5f, 0.01f, 0.1f);
			WorldCoordinate coord = new WorldCoordinate(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), Random.Range(0, bug.room.TileHeight), -1);
			if (IdleScore(coord) < IdleScore(tempIdlePos))
			{
				tempIdlePos = coord;
			}
			bool flag5 = false;
			bool flag6 = false;
			if (bug.grasps[0] != null)
			{
				if (bug.grasps[0].grabbed is MothGrub mothGrub)
				{
					TryGrab(bug.grasps[0].grabbed, bug.room.GetTile(mothGrub.bodyChunks[2].pos + new Vector2(0f, -20f)).Solid || bug.room.GetTile(mothGrub.bodyChunks[2].pos + new Vector2(0f, -40f)).Solid);
					flag6 = true;
				}
				else
				{
					ThrowGrabbed();
				}
			}
			if (flag)
			{
				bug.runSpeed = 1f;
			}
			if (timeInRoom > timeToTire || flag)
			{
				if (bug.grasps[0] == null && !bug.Small)
				{
					ChildTrackerModule.TrackedChild childToCarry = childTracker.GetChildToCarry();
					if (childToCarry != null)
					{
						flag5 = true;
						base.creature.abstractAI.SetDestination(childToCarry.critRep.BestGuessForPosition());
						TryGrab(childToCarry.critRep.representedCreature.realizedObject, frontLegsOnly: false, forceOthersToLetGo: true);
					}
				}
				if (base.pathFinder.CoordinateReachable(new WorldCoordinate(bug.room.world.offScreenDen.index, -1, -1, 0)))
				{
					if (!flag5)
					{
						if (idlePosCounter <= 0)
						{
							timeInRoom = 0;
							tryingToLeaveRoom = false;
						}
						else if (!tryingToLeaveRoom)
						{
							idlePosCounter = Random.Range(500, 1000);
							timeToTire = Random.Range(400, 1000);
							timeInRoom = Mathf.Max(timeToTire + 1, timeInRoom);
							tryingToLeaveRoom = true;
						}
						if (tryingToLeaveRoom)
						{
							base.creature.abstractAI.SetDestination(new WorldCoordinate(bug.room.world.offScreenDen.index, -1, -1, 0));
						}
					}
				}
				else
				{
					timeInRoom = 0;
					tryingToLeaveRoom = false;
				}
			}
			else if (IdleScore(tempIdlePos) < IdleScore(base.pathFinder.GetDestination) + Custom.LerpMap(idlePosCounter, 0f, 300f, 100f, -300f))
			{
				SetDestination(tempIdlePos);
				idlePosCounter = Random.Range(200, 1000);
				tempIdlePos = new WorldCoordinate(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), Random.Range(0, bug.room.TileHeight), -1);
			}
			if (!flag5)
			{
				idlePosCounter -= ((!flag6 || !(Random.value < 0.4f)) ? 1 : 0);
			}
		}
		else if (behavior == Behavior.Flee)
		{
			timeToTire--;
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
			if (base.threatTracker.mostThreateningCreature != null)
			{
				focusCreature = base.threatTracker.mostThreateningCreature;
			}
			base.creature.abstractAI.SetDestination(base.threatTracker.FleeTo(base.creature.pos, 10, 30, considerLeavingRoom: true));
		}
		else if (behavior == Behavior.Eat)
		{
			if (!flag2)
			{
				focusCreature = base.preyTracker.MostAttractivePrey;
			}
			timeInRoom = 0;
			float y = 250f * bug.scale;
			Vector2? vector = SharedPhysics.ExactTerrainRayTracePos(bug.room, drinkSpot, drinkSpot - new Vector2(0f, y));
			Creature creature = focusCreature?.representedCreature.realizedCreature;
			if (creature != null && (!creature.dead || SmallWantsToEat(creature)))
			{
				TryGrab(creature, vector.HasValue);
				if (bug.grasps[0] != null)
				{
					drinkSpot = bug.grasps[0].grabbedChunk.pos;
				}
			}
			else if (bug.grasps[0] != null && bug.grasps[0].grabbed == creature && (double)Random.value < 0.05)
			{
				bug.ReleaseGrasp(0);
			}
			if (bug.drinkingChunk)
			{
				bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.3f, 0.005f, 0.05f);
				if (vector.HasValue)
				{
					y = Mathf.Abs(vector.Value.y - drinkSpot.y) * 0.9f;
				}
				base.creature.abstractAI.SetDestination(bug.room.GetWorldCoordinate(drinkSpot - new Vector2(0f, y)));
			}
			else
			{
				bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.7f, 0.01f, 0.1f);
				if (focusCreature != null)
				{
					base.creature.abstractAI.SetDestination(focusCreature.BestGuessForPosition());
				}
				if (bug.drinkChunk == null && creature != null)
				{
					Vector2 pos2 = bug.bodyChunks[0].pos;
					int num = 0;
					float num2 = Vector2.Distance(pos2, creature.bodyChunks[num].pos);
					for (int j = 1; j < creature.bodyChunks.Length; j++)
					{
						float num3 = Vector2.Distance(pos2, creature.bodyChunks[j].pos);
						if (num3 < num2)
						{
							num = j;
							num2 = num3;
						}
					}
					if (VisualContact(creature.bodyChunks[num]) && num2 < 400f * bug.scale + creature.bodyChunks[num].rad)
					{
						drinkSpot = bug.bodyChunks[2].pos;
						bug.drinkChunk = creature.bodyChunks[num];
						base.creature.abstractAI.SetDestination(bug.room.GetWorldCoordinate(bug.drinkChunk.pos + new Vector2(0f, 80f * bug.scale)));
					}
				}
			}
		}
		else if (behavior == Behavior.Attack)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
			if (!flag2)
			{
				focusCreature = base.threatTracker.mostThreateningCreature;
			}
			bug.shake = Mathf.Max(bug.shake, 0.4f * bug.abstractCreature.personality.aggression);
			if ((double)Random.value < 0.03 * (double)(bug.abstractCreature.personality.dominance * 0.5f + 0.5f))
			{
				bug.shake = 0.2f * bug.abstractCreature.personality.dominance + Random.value * 0.8f;
			}
			if (focusCreature.representedCreature.realizedCreature != null)
			{
				TryGrab(focusCreature.representedCreature.realizedCreature, frontLegsOnly: false);
			}
			if (bug.grasps[0] == null)
			{
				base.creature.abstractAI.SetDestination(focusCreature.BestGuessForPosition());
			}
			else if (giveUpOnCliffTimer > 0)
			{
				WorldCoordinate coord2 = new WorldCoordinate(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), 0, -1);
				IntVector2 intVector = CliffScore(tempCliffPos);
				IntVector2 intVector2 = CliffScore(coord2);
				if (intVector2.x < intVector.x)
				{
					tempCliffPos = coord2;
					tempCliffPos.y = intVector2.y;
					intVector = intVector2;
				}
				if (intVector.x == int.MaxValue)
				{
					giveUpOnCliffTimer--;
					base.creature.abstractAI.SetDestination(bug.room.GetWorldCoordinate(bug.bodyChunks[2].pos + new Vector2(0f, 100f)));
				}
				else
				{
					Vector2 b = bug.room.MiddleOfTile(new IntVector2(tempCliffPos.x, 0));
					if (bug.room.VisualContact(bug.bodyChunks[2].pos + new Vector2(20f, 0f), b) && bug.room.VisualContact(bug.bodyChunks[2].pos - new Vector2(20f, 0f), b) && Mathf.Abs(b.x - bug.bodyChunks[2].pos.x) < 60f && Random.value < 0.03f)
					{
						ThrowGrabbed();
					}
					base.creature.abstractAI.SetDestination(tempCliffPos);
				}
				if (tempCliffPos == default(WorldCoordinate))
				{
					giveUpOnCliffTimer--;
				}
			}
			else if (!bug.Small && base.pathFinder.CoordinateReachable(new WorldCoordinate(bug.room.world.offScreenDen.index, -1, -1, 0)))
			{
				if (bug.firstChunk.pos.y + Mathf.Pow(Random.value, 2f) * 100f > bug.room.PixelHeight + 800f)
				{
					base.creature.abstractAI.SetDestination(bug.room.GetWorldCoordinate(bug.firstChunk.pos));
					ThrowGrabbed(44f + Random.value * 12f);
				}
				else
				{
					base.creature.abstractAI.SetDestination(new WorldCoordinate(bug.room.world.offScreenDen.index, -1, -1, 0));
				}
			}
			else
			{
				ThrowGrabbed();
			}
		}
		else if (behavior == Behavior.Stalk)
		{
			if (!flag2)
			{
				focusCreature = base.threatTracker.mostThreateningCreature;
			}
			WorldCoordinate worldCoordinate = focusCreature.BestGuessForPosition();
			Creature creature2 = null;
			if (focusCreature.representedCreature.realizedCreature != null)
			{
				Creature realizedCreature = focusCreature.representedCreature.realizedCreature;
				for (int k = 0; k < realizedCreature.grasps.Length; k++)
				{
					if (realizedCreature.grasps[k] != null && SmallWantsToEat(realizedCreature.grasps[k].grabbed))
					{
						creature2 = realizedCreature.grasps[k].grabbed as Creature;
						break;
					}
				}
			}
			if (bug.abstractCreature.Room == focusCreature.representedCreature.Room && Custom.ManhattanDistance(base.creature.pos, worldCoordinate) < 10 && creature2 == null)
			{
				bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
				base.creature.abstractAI.SetDestination(base.threatTracker.FleeTo(base.creature.pos, 10, 30, considerLeavingRoom: false));
			}
			else
			{
				bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.7f, 0.01f, 0.1f);
				base.creature.abstractAI.SetDestination(worldCoordinate);
				if (creature2 != null && creature2.dead && (double)Random.value < 0.07)
				{
					TryGrab(creature2, frontLegsOnly: true, forceOthersToLetGo: true);
				}
			}
		}
		else if (behavior == Behavior.StowFood)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
			if (base.pathFinder.CoordinateReachable(new WorldCoordinate(bug.room.world.offScreenDen.index, -1, -1, 0)))
			{
				base.creature.abstractAI.SetDestination(new WorldCoordinate(bug.room.world.offScreenDen.index, -1, -1, 0));
			}
			else if (base.denFinder.GetDenPosition().HasValue)
			{
				base.creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.SetDownBaby)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.6f, 0.01f, 0.1f);
			WorldCoordinate coord3 = new WorldCoordinate(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), Random.Range(0, bug.room.TileHeight), -1);
			if (SafeBabyPlaceScore(coord3) < SafeBabyPlaceScore(tempSafeBabyPos))
			{
				tempSafeBabyPos = coord3;
			}
			if (tempSafeBabyPos != default(WorldCoordinate))
			{
				Vector2 b2 = bug.room.MiddleOfTile(tempSafeBabyPos) + new Vector2(0f, 20f);
				if (bug.room.VisualContact(bug.bodyChunks[2].pos, b2) && Vector2.Distance(bug.bodyChunks[2].pos, b2) < 50f && Random.value < 0.08f)
				{
					ThrowGrabbed();
				}
				base.creature.abstractAI.SetDestination(tempSafeBabyPos + new IntVector2(0, 1));
			}
		}
		else if (behavior == Behavior.FeedBaby)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.4f, 0.1f, 0.1f);
			MothGrub mothGrub2 = null;
			mothGrub2 = ((!bug.safariControlled) ? (childTracker.GetChildToCarry().critRep.representedCreature.realizedCreature as MothGrub) : (focusCreature.representedCreature.realizedCreature as MothGrub));
			if (bug.drinkChunk != null && bug.drinkChunk != mothGrub2.firstChunk)
			{
				bug.ReleaseTongue();
			}
			WorldCoordinate worldCoordinate2 = bug.room.GetWorldCoordinate(mothGrub2.firstChunk.pos);
			float num4 = Vector2.Distance(bug.firstChunk.pos, mothGrub2.firstChunk.pos);
			if (VisualContact(mothGrub2.firstChunk) && num4 < 400f * bug.scale)
			{
				bug.drinkChunk = mothGrub2.firstChunk;
				base.creature.abstractAI.SetDestination(bug.room.GetWorldCoordinate(bug.drinkChunk.pos + new Vector2(0f, 80f * bug.scale)));
			}
			base.creature.abstractAI.SetDestination(worldCoordinate2);
		}
		else if (behavior == Behavior.EscapeRain)
		{
			bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.7f, 0.01f, 0.1f);
			if (base.denFinder.GetDenPosition().HasValue)
			{
				base.creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			if (bug.grasps[0] != null && !(bug.grasps[0].grabbed is MothGrub))
			{
				ThrowGrabbed();
			}
		}
		else if (behavior == Behavior.GetUnstuck)
		{
			base.creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
		}
		fear = Custom.LerpAndTick(fear, Mathf.Max(base.utilityComparer.GetUtilityTracker(base.threatTracker).SmoothedUtility(), Mathf.Pow(base.threatTracker.Panic, 0.7f)), 0.07f, 1f / 30f);
		if (noiseRectionDelay > 0)
		{
			noiseRectionDelay--;
		}
	}

	public void TryGrab(PhysicalObject obj, bool frontLegsOnly, bool forceOthersToLetGo = false, bool forceAllGrabOnGrip = false)
	{
		if (grabCoolDown > 0 || obj == null || obj.room != bug.room || (obj is Creature crit && bug.CritGrabbedToMe(crit)))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < bug.legs.GetLength(0); i++)
		{
			for (int j = 0; j < bug.legs.GetLength(1); j++)
			{
				if (frontLegsOnly && i == 0)
				{
					if ((double)Random.value < 0.1)
					{
						bug.legs[i, j].grabbedChunk = null;
					}
				}
				else
				{
					if (bug.legs[i, j].grabbedChunk != null || !bug.legs[i, j].TryGrab(obj) || (bug.grasps[0] != null && bug.grasps[0].grabbed == obj))
					{
						continue;
					}
					if (forceOthersToLetGo)
					{
						for (int k = 0; k < obj.grabbedBy.Count; k++)
						{
							obj.grabbedBy[k].Release();
						}
					}
					flag = true;
					bug.holdThroughWallCounter = 0;
					bool pacifying = bug.Small && obj is Rat;
					bug.Grab(obj, 0, bug.legs[i, j].grabbedChunk.index, Creature.Grasp.Shareability.NonExclusive, 1f, overrideEquallyDominant: true, pacifying);
					bug.room.PlaySound(WatcherEnums.WatcherSoundID.Moth_Grab, bug.mainBodyChunk);
				}
			}
		}
		if (!(forceAllGrabOnGrip && bug.grasps[0] != null && bug.grasps[0].grabbed == obj && flag))
		{
			return;
		}
		for (int l = 0; l < bug.legs.GetLength(0); l++)
		{
			for (int m = 0; m < bug.legs.GetLength(1); m++)
			{
				bug.legs[l, m].grabbedChunk = bug.grasps[0].grabbedChunk;
			}
		}
	}

	public void ThrowGrabbed(float dropSpeed = 0f)
	{
		if (dropSpeed > 0f)
		{
			for (int i = 0; i < bug.grasps[0].grabbed.bodyChunks.Length; i++)
			{
				bug.grasps[0].grabbed.bodyChunks[i].vel = new Vector2(bug.grasps[0].grabbed.bodyChunks[i].vel.x, 0f - dropSpeed);
			}
			for (int j = 0; j < bug.grasps[0].grabbed.grabbedBy.Count; j++)
			{
				if (bug.grasps[0].grabbed.grabbedBy[j].grabber != bug && bug.grasps[0].grabbed.grabbedBy[j].grabber is BigMoth)
				{
					bug.grasps[0].grabbed.grabbedBy[j].Release();
				}
			}
		}
		if (bug.grasps[0].grabbed is Creature creature)
		{
			Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForCreature(creature.abstractCreature, addIfMissing: false);
			if (creatureRepresentation != null)
			{
				(creatureRepresentation.dynamicRelationship.state as BigMothTrackState).thrownTimer = Random.Range(200, 800);
				creatureRepresentation.dynamicRelationship.currentRelationship.type = CreatureTemplate.Relationship.Type.Ignores;
				base.threatTracker.RemoveThreatCreature(creatureRepresentation.representedCreature);
				base.tracker.ForgetCreature(creatureRepresentation.representedCreature);
			}
		}
		bug.room.PlaySound(SoundID.Slugcat_Throw_Creature, bug.bodyChunks[2]);
		bug.ReleaseGrasp(0);
		grabCoolDown = 40;
	}

	public float IdleScore(WorldCoordinate coord)
	{
		if (coord.room != creature.pos.room || !base.pathFinder.CoordinateReachableAndGetbackable(coord))
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
		if (bug.room.aimap.getAItile(coord).floorAltitude > (bug.Small ? 1 : 5))
		{
			bool flag = false;
			if (!bug.Small)
			{
				for (int i = 0; i < 2; i++)
				{
					IntVector2 pos = new IntVector2((i == 0) ? (-3) : 3, coord.y);
					if (!bug.room.HasAnySolid(pos) && bug.room.aimap.getAItile(pos).floorAltitude < 5)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				num += 100f;
			}
		}
		num += base.threatTracker.ThreatOfTile(coord, accountThreatCreatureAccessibility: true) * 1000f;
		num += base.threatTracker.ThreatOfTile(bug.room.GetWorldCoordinate((bug.room.MiddleOfTile(coord) + bug.room.MiddleOfTile(creature.pos)) / 2f), accountThreatCreatureAccessibility: true) * 1000f;
		for (int j = 0; j < base.noiseTracker.sources.Count; j++)
		{
			num += Custom.LerpMap(Vector2.Distance(bug.room.MiddleOfTile(coord), base.noiseTracker.sources[j].pos), 40f, 400f, 100f, 0f);
		}
		if (base.discomfortTracker != null)
		{
			num -= 1000f * base.discomfortTracker.DiscomfortOfTile(coord);
		}
		return num;
	}

	public IntVector2 CliffScore(WorldCoordinate coord)
	{
		coord.y = 0;
		if (coord.room != creature.pos.room || !base.pathFinder.CoordinateReachable(coord))
		{
			return new IntVector2(int.MaxValue, 0);
		}
		IntVector2 intVector = new IntVector2(coord.x, 0);
		int num = Mathf.Abs(intVector.x - bug.room.GetTilePosition(bug.bodyChunks[2].pos).x);
		for (int i = 1; i < 10; i++)
		{
			if (bug.room.HasAnySolid(intVector + new IntVector2(i, 0)) || bug.room.HasAnySolid(intVector - new IntVector2(i, 0)))
			{
				num += (9 - i) * 5;
				break;
			}
		}
		int num2 = 0;
		for (int j = 1; j < bug.room.TileHeight - 7 && !bug.room.HasAnySolid(intVector + new IntVector2(0, j)) && !bug.room.HasAnySolid(intVector + new IntVector2(0, j + 5)); j++)
		{
			num2 = j;
		}
		num += bug.room.TileHeight - num2;
		return new IntVector2(num, num2);
	}

	public float SafeBabyPlaceScore(WorldCoordinate coord)
	{
		if (coord.room != creature.pos.room || !base.pathFinder.CoordinateReachableAndGetbackable(coord))
		{
			return float.MaxValue;
		}
		float num = 1f;
		if (bug.room.GetTile(coord + new IntVector2(0, -2)).Solid)
		{
			for (int i = 1; i < 8; i++)
			{
				if (!bug.room.GetTile(coord + new IntVector2(i, -2)).Solid || !bug.room.GetTile(coord + new IntVector2(-i, -2)).Solid)
				{
					num += (float)((7 - i) * 5);
					break;
				}
			}
			return num;
		}
		return float.MaxValue;
	}

	public Tracker.CreatureRepresentation FindSafariFocusCreature()
	{
		AbstractCreature abstractCreature = FindSafariTargetCreature();
		if (abstractCreature != null)
		{
			return CreateTrackerRepresentationForCreature(abstractCreature);
		}
		return null;
	}

	public AbstractCreature FindSafariTargetCreature()
	{
		if (bug.grasps[0]?.grabbed is Creature creature)
		{
			return creature.abstractCreature;
		}
		float num = 200f;
		AbstractCreature result = null;
		foreach (AbstractCreature creature2 in bug.room.abstractRoom.creatures)
		{
			if (creature2 != base.creature && creature2.realizedCreature != null)
			{
				float num2 = Vector2.Distance(creature2.realizedCreature.mainBodyChunk.pos, bug.bodyChunks[2].pos);
				if (num2 < num)
				{
					result = creature2;
					num = num2;
				}
			}
		}
		return result;
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
		cost = base.TravelPreference(coord, cost);
		float val = Mathf.Max(0f, base.threatTracker.ThreatOfTile(coord.destinationCoord, accountThreatCreatureAccessibility: false) - base.threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: false));
		return new PathCost(cost.resistance + Custom.LerpMap(val, 0f, 1.5f, 0f, 10000f, 5f), cost.legality);
	}

	public void SocialEvent(SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
	{
		if ((subjectCrit != null && objectCrit != null && subjectCrit.abstractCreature.rippleLayer != objectCrit.abstractCreature.rippleLayer && !subjectCrit.abstractCreature.rippleBothSides && !objectCrit.abstractCreature.rippleBothSides) || base.tracker.RepresentationForCreature(subjectCrit.abstractCreature, addIfMissing: false) == null || base.tracker.RepresentationForCreature(subjectCrit.abstractCreature, addIfMissing: false).TicksSinceSeen > 80)
		{
			return;
		}
		if (objectCrit == bug || objectCrit.Template.type == WatcherEnums.CreatureTemplateType.BigMoth || objectCrit.Template.type == WatcherEnums.CreatureTemplateType.SmallMoth || objectCrit.Template.type == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			float num = ((bug.Template.type == WatcherEnums.CreatureTemplateType.SmallMoth) ? 1f : 0.3f) * (1f - bug.abstractCreature.personality.bravery);
			float num2 = 0f;
			if (ID == SocialEventRecognizer.EventID.NonLethalAttackAttempt)
			{
				num2 = 0.01f * num;
			}
			else if (ID == SocialEventRecognizer.EventID.NonLethalAttack)
			{
				num2 = 0.025f * num;
			}
			else if (ID == SocialEventRecognizer.EventID.LethalAttackAttempt)
			{
				num2 = 0.05f * num;
			}
			else if (ID == SocialEventRecognizer.EventID.LethalAttack)
			{
				num2 = 0.25f;
			}
			else if (ID == SocialEventRecognizer.EventID.Killing)
			{
				num2 = 0.95f;
			}
			else if (ID == SocialEventRecognizer.EventID.Theft && !bug.Small)
			{
				num2 = 0.2f;
			}
			else if (ID == SocialEventRecognizer.EventID.ItemOffering && involvedItem != null && involvedItem is Creature creature && creature.Template.meatPoints >= 2 && creature.State.meatLeft >= 2)
			{
				num2 = -0.05f;
			}
			if (bug.Small)
			{
				num2 *= 2f;
			}
			if (objectCrit != bug && objectCrit.Template.type != WatcherEnums.CreatureTemplateType.MothGrub && ID != SocialEventRecognizer.EventID.Killing)
			{
				num2 /= 3f;
			}
			if (objectCrit.dead && ID == SocialEventRecognizer.EventID.Killing)
			{
				num2 /= 3f;
			}
			base.creature.state.socialMemory.GetOrInitiateRelationship(subjectCrit.abstractCreature.ID).InfluenceTempLike(num2 * -1f);
			base.creature.state.socialMemory.GetOrInitiateRelationship(subjectCrit.abstractCreature.ID).InfluenceLike(num2 * -0.1f * (bug.Small ? 0.25f : 1f));
		}
		else if (objectCrit.Template.meatPoints >= 2)
		{
			float num3 = 0f;
			if (ID == SocialEventRecognizer.EventID.LethalAttack)
			{
				num3 = 0.005f;
			}
			else if (ID == SocialEventRecognizer.EventID.Killing)
			{
				num3 = 0.12f;
			}
			base.creature.state.socialMemory.GetOrInitiateRelationship(subjectCrit.abstractCreature.ID).InfluenceTempLike(num3);
			base.creature.state.socialMemory.GetOrInitiateRelationship(subjectCrit.abstractCreature.ID).InfluenceLike(num3 * 0.1f);
		}
	}
}
