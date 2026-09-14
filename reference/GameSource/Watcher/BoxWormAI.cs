using RWCustom;
using UnityEngine;

namespace Watcher;

public class BoxWormAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Dormant = new Behavior("Dormant", register: true);

		public static Behavior ApproachEnemy = new Behavior("ApproachEnemy", register: true);

		public static Behavior Attack = new Behavior("Attack", register: true);

		public static Behavior ReturnHome = new Behavior("ReturnHome", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Counter patienceTimer = new Counter(200);

	public Counter lagBetweenActionsTimer = new Counter(10);

	public Counter memoryCounter = new Counter(600);

	public bool hasFireSprite = true;

	public bool keepCallingForHelp;

	public bool debug;

	public float helpPriority;

	public Behavior behavior;

	public BoxWorm ThisCreature => creature.realizedObject as BoxWorm;

	public Tracker.CreatureRepresentation AttackTarget
	{
		get
		{
			if (!ThisCreature.safariControlled)
			{
				return base.preyTracker.MostAttractivePrey;
			}
			return null;
		}
	}

	public Vector2 TargetPosition
	{
		get
		{
			if (ThisCreature.safariControlled)
			{
				return ThisCreature.movementTarget;
			}
			if (AttackTarget == null)
			{
				return ThisCreature.movementTarget;
			}
			return ThisCreature.room.MiddleOfTile(AttackTarget.lastSeenCoord);
		}
	}

	public bool ArrivedAtRoot => Vector2.Distance(ThisCreature.mainBodyChunk.pos, ThisCreature.rootPoint) < ThisCreature.totalWormLength / 4f;

	public bool wantToCallForHelp
	{
		get
		{
			if (ThisCreature.safariControlled)
			{
				return behavior != Behavior.Dormant;
			}
			if (AttackTarget == null || !(behavior == Behavior.Dormant) || !((double)memoryCounter.normalized > 0.05))
			{
				return keepCallingForHelp;
			}
			return true;
		}
	}

	public BoxWormAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		ThisCreature.ai = this;
		AddModule(new StandardPather(this, world, creature));
		AddModule(new Tracker(this, 10, 10, -1, 0.35f, 5, 5, 10)
		{
			visualize = false
		});
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new PreyTracker(this, 10, 0f, ThisCreature.attackRange, creature.creatureTemplate.visualRadius, 1f));
		base.pathFinder.stepsPerFrame = 5;
		behavior = Behavior.Dormant;
		memoryCounter.SetToMin();
	}

	public void UpdateTimers()
	{
		patienceTimer.Tick();
		lagBetweenActionsTimer.Tick();
		memoryCounter.Tick();
	}

	public void Lag()
	{
		lagBetweenActionsTimer.Set(20 + (int)(Random.value * 40f));
	}

	public override void Update()
	{
		base.Update();
		UpdateTimers();
		if (AttackTarget != null)
		{
			if (AttackTarget.VisualContact)
			{
				if (memoryCounter.isFinished)
				{
					memoryCounter.Add(400);
				}
				int num = ((behavior != Behavior.Dormant) ? 2 : 0);
				int num2 = (base.pathFinder.CoordinateReachableAndGetbackable(AttackTarget.lastSeenCoord) ? 2 : 0);
				memoryCounter.Add(1 + num + num2);
			}
			if (AttackTarget.representedCreature.state.dead)
			{
				memoryCounter.SetToMin();
			}
			if (memoryCounter.isFinished)
			{
				ForgetCurrentTarget();
			}
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
		if (!lagBetweenActionsTimer.isFinished)
		{
			ThisCreature.movementTarget = ThisCreature.bodyChunks[0].pos;
			return;
		}
		Vector2 vector = Vector2.zero;
		Player.InputPackage? inputWithDiagonals = ThisCreature.inputWithDiagonals;
		if (inputWithDiagonals.HasValue)
		{
			Player.InputPackage valueOrDefault = inputWithDiagonals.GetValueOrDefault();
			if (valueOrDefault.analogueDir.magnitude > 0f)
			{
				vector = valueOrDefault.analogueDir.normalized;
			}
			else if (valueOrDefault.x != 0 || valueOrDefault.y != 0)
			{
				vector = new Vector2(valueOrDefault.x, valueOrDefault.y).normalized;
			}
			if (valueOrDefault.jmp)
			{
				if (behavior != Behavior.Dormant)
				{
					SetBehavior(Behavior.Dormant);
					int num = Mathf.Min(ThisCreature.minSteamRequired + 133, ThisCreature.steamAvailable);
					ThisCreature.steamAvailable.Set(num);
					lagBetweenActionsTimer.Set(Mathf.Max(num - ThisCreature.minSteamRequired, 20) + 1);
				}
			}
			else if (valueOrDefault.pckp)
			{
				if (behavior != Behavior.ApproachEnemy)
				{
					SetBehavior(Behavior.ApproachEnemy);
				}
			}
			else if (behavior != Behavior.Attack)
			{
				SetBehavior(Behavior.Attack);
			}
		}
		float num2 = ((behavior == Behavior.Attack) ? 300f : 100f);
		if (vector == Vector2.zero)
		{
			num2 = 50f;
		}
		Vector2 b = ThisCreature.bodyChunks[0].pos + ((vector != Vector2.zero) ? vector : Custom.DirVec(ThisCreature.bodyChunks[1].pos, ThisCreature.bodyChunks[0].pos)) * num2;
		float speed = Mathf.Max(Vector2.Distance(ThisCreature.movementTarget, b) / 25f, 0.5f);
		ThisCreature.movementTarget = Custom.MoveTowards(ThisCreature.movementTarget, b, speed);
	}

	public void DecideBehavior()
	{
		if (!lagBetweenActionsTimer.isFinished)
		{
			return;
		}
		if (behavior == Behavior.Dormant)
		{
			if (AttackTarget != null && ThisCreature.CanMove && base.pathFinder.DoneMappingAccessibility)
			{
				if (WithinAttackRange(TargetPosition) && AttackTarget != null)
				{
					SetBehavior(Behavior.Attack);
				}
				else
				{
					SetBehavior(Behavior.ApproachEnemy);
				}
			}
		}
		else if (!ThisCreature.CanMove)
		{
			SetBehavior(Behavior.Dormant);
		}
		else if (behavior == Behavior.Attack)
		{
			if (AttackTarget == null)
			{
				SetBehavior(Behavior.ReturnHome);
			}
			else if (!AttackTarget.VisualContact && memoryCounter.isFinished)
			{
				SetBehavior(Behavior.ReturnHome);
			}
			else if (!WithinAttackRange(TargetPosition) && ThisCreature.attackTimer.isFinished)
			{
				SetBehavior(Behavior.ApproachEnemy);
			}
		}
		else if (behavior == Behavior.ApproachEnemy)
		{
			bool flag = WithinAttackRange(TargetPosition);
			patienceTimer.SetMax(200);
			if (AttackTarget == null)
			{
				SetBehavior(Behavior.ReturnHome);
			}
			else if (flag && FinishedStep(flag) && AttackTarget.VisualContact && ThisCreature.IsBodyChunkOnGround(6))
			{
				SetBehavior(Behavior.Attack);
			}
		}
		else
		{
			if (!(behavior == Behavior.ReturnHome))
			{
				return;
			}
			patienceTimer.SetMax(40);
			bool arrivedAtRoot = ArrivedAtRoot;
			bool flag2 = WithinAttackRange(TargetPosition);
			if (arrivedAtRoot && FinishedStep(arrivedAtRoot))
			{
				SetBehavior(Behavior.Dormant);
			}
			else if (AttackTarget != null && !ThisCreature.helpRecievedTimer.isFinished)
			{
				if (flag2 && FinishedStep(flag2))
				{
					SetBehavior(Behavior.Attack);
				}
				else
				{
					SetBehavior(Behavior.ApproachEnemy);
				}
			}
		}
	}

	public void ExecuteBehavior()
	{
		helpPriority = 0.5f;
		if (behavior == Behavior.Dormant)
		{
			helpPriority = 0.8f;
			keepCallingForHelp = false;
			ThisCreature.DormantStance();
			if (wantToCallForHelp && !ThisCreature.helpCallTimer.isFinished)
			{
				ThisCreature.HelpCallEffect();
			}
			else if (memoryCounter.isFinished)
			{
				ThisCreature.helpCallTimer.Reset();
			}
			if (!ThisCreature.releaseSteamTimer.isFinished)
			{
				ThisCreature.ReleaseSteam();
			}
			return;
		}
		keepCallingForHelp = true;
		if (behavior == Behavior.ApproachEnemy)
		{
			if (AttackTarget != null)
			{
				helpPriority = 0.9f;
				WorldCoordinate lastSeenCoord = AttackTarget.lastSeenCoord;
				if (base.pathFinder.PathingCellAtWorldCoordinate(lastSeenCoord).possibleToGetBackFrom)
				{
					SetDestination(lastSeenCoord);
				}
				if (!ThisCreature.safariControlled)
				{
					ThisCreature.movementTarget = ThisCreature.room.MiddleOfTile(lastSeenCoord);
				}
			}
			ThisCreature.FollowPath();
		}
		else if (behavior == Behavior.Attack)
		{
			if (AttackTarget != null || ThisCreature.safariControlled)
			{
				helpPriority = 1f;
				ThisCreature.PerformAttack();
			}
		}
		else if (behavior == Behavior.ReturnHome)
		{
			if (!base.pathFinder.CoordinateReachableAndGetbackable(ThisCreature.room.GetWorldCoordinate(ThisCreature.rootPoint)))
			{
				ThisCreature.rootPoint = ThisCreature.bodyChunks[0].pos;
			}
			SetDestination(ThisCreature.room.GetWorldCoordinate(ThisCreature.rootPoint));
			ThisCreature.FollowPath();
		}
	}

	public void ForgetCurrentTarget()
	{
		if (AttackTarget != null)
		{
			base.tracker.ForgetCreature(AttackTarget.representedCreature);
		}
	}

	public void SetBehavior(Behavior newBehavior)
	{
		_ = debug;
		ThisCreature.ReleaseAllGrabbers();
		if (behavior == Behavior.Dormant || (behavior == Behavior.ApproachEnemy && newBehavior == Behavior.Attack))
		{
			ThisCreature.transitionTimer.Reset();
		}
		if (newBehavior == Behavior.ApproachEnemy)
		{
			memoryCounter.Add(400);
		}
		if (newBehavior == Behavior.Attack)
		{
			ThisCreature.attackTimer.Reset();
		}
		if (newBehavior == Behavior.Dormant)
		{
			ThisCreature.helpCallTimer.Reset();
			ThisCreature.releaseSteamTimer.Reset();
		}
		behavior = newBehavior;
		Lag();
	}

	public override float CurrentPlayerAggression(AbstractCreature player)
	{
		if (behavior == Behavior.Dormant)
		{
			return 0f;
		}
		if (behavior == Behavior.Attack)
		{
			return 1f;
		}
		return 0.5f;
	}

	public bool WithinAttackRange(Vector2 target)
	{
		return Vector2.Distance(ThisCreature.bodyChunks[6].pos, target) < ThisCreature.attackRange;
	}

	public bool ArrivedAtPosition(Vector2 target)
	{
		return Vector2.Distance(ThisCreature.bodyChunks[6].pos, target) < ThisCreature.totalWormLength * 2f;
	}

	public bool FinishedStep(bool frustrationCountdown = false)
	{
		if (frustrationCountdown)
		{
			patienceTimer.ResetIfFinished();
			if ((int)patienceTimer == 1)
			{
				return true;
			}
		}
		if (!(ThisCreature.archAmount <= 0.1f))
		{
			return (int)ThisCreature.movementTimer <= 0;
		}
		return true;
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
		if (StaticRelationship(otherCreature).type == CreatureTemplate.Relationship.Type.Attacks || otherCreature.realizedObject is Player)
		{
			return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
		}
		return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0.5f, forgetWhenNotVisible: true);
	}

	public void CreatureGrabsLarva(float force, AbstractCreature creature)
	{
		if (creature.realizedObject is Player player)
		{
			ModifyPlayerRelationship(player, force);
		}
	}

	public void ModifyPlayerRelationship(Player player, float amount)
	{
		if (!player.IsHidden)
		{
			creature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (player.abstractCreature.state as PlayerState).playerNumber, amount, 0.2f, 0f);
		}
	}

	public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		DynamicRelationship(dRelation.trackerRep.representedCreature);
		if (dRelation.trackerRep.representedCreature.realizedObject != null && dRelation.trackerRep.representedCreature.realizedObject is Player player && creature.world.game.session.creatureCommunities.LikeOfPlayer(creature.creatureTemplate.communityID, creature.world.RegionNumber, (player.abstractCreature.state as PlayerState).playerNumber) < -0.4f)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f);
		}
		return dRelation.currentRelationship;
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}
}
