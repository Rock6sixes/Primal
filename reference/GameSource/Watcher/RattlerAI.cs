using RWCustom;
using UnityEngine;

namespace Watcher;

public class RattlerAI : ArtificialIntelligence, IUseARelationshipTracker
{
	public Rattler rattler;

	public float targetRattleIntensity;

	public RattlerAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		rattler = creature.realizedCreature as Rattler;
		rattler.ai = this;
		AddModule(new Tracker(this, 10, 5, 500, 0.5f, 5, 5, 10));
		AddModule(new PreyTracker(this, 1, 1.2f, 0f, rattler.armLength, 0.75f));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new NoiseTracker(this, base.tracker));
		AddModule(new RelationshipTracker(this, base.tracker));
		base.noiseTracker.hearingSkill = 1f;
		AddModule(new RelationshipTracker(this, base.tracker));
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		CreatureTemplate.Relationship relationship = creature.creatureTemplate.CreatureRelationship(otherCreature.creatureTemplate);
		if (relationship.type != CreatureTemplate.Relationship.Type.Eats && relationship.type != CreatureTemplate.Relationship.Type.Afraid)
		{
			return null;
		}
		return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, otherCreature.realizedCreature.TotalMass, forgetWhenNotVisible: true);
	}

	public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats)
		{
			return base.preyTracker;
		}
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		return null;
	}

	public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return StaticRelationship(dRelation.trackerRep.representedCreature);
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	public override void Update()
	{
		base.Update();
		bool flag = CountArms(Rattler.Arm.Task.MoveCreature) > 0;
		base.noiseTracker.hearingSkill = 0f;
		if (rattler.safariControlled && rattler.SafariRattleInput())
		{
			Rattler.Arm[] arms = rattler.arms;
			for (int i = 0; i < arms.Length; i++)
			{
				arms[i].Retract();
			}
			targetRattleIntensity = Mathf.Max(targetRattleIntensity, 1f);
		}
		else if (base.rainTracker.Utility() > 0.5f && !rattler.safariControlled)
		{
			Rattler.Arm[] arms = rattler.arms;
			for (int i = 0; i < arms.Length; i++)
			{
				arms[i].Retract();
			}
		}
		else if (flag)
		{
			targetRattleIntensity = Mathf.Max(targetRattleIntensity - 0.05f, 0f);
		}
		else if (base.threatTracker.Utility() > 0.7f && !rattler.safariControlled)
		{
			Rattler.Arm[] arms = rattler.arms;
			for (int i = 0; i < arms.Length; i++)
			{
				arms[i].Retract();
			}
			Creature creature = base.threatTracker.mostThreateningCreature?.representedCreature.realizedCreature;
			if (creature != null)
			{
				targetRattleIntensity = Mathf.Max(targetRattleIntensity, GetRattleFactor(creature));
			}
		}
		else
		{
			base.noiseTracker.hearingSkill = 1f;
			int num = 0;
			if (base.noiseTracker.Utility() > 0.25f)
			{
				num = 1;
			}
			if (base.noiseTracker.Utility() > 0.5f)
			{
				num = 2;
			}
			if (!rattler.safariControlled && base.noiseTracker.soundToExamine != null && CountArms(Rattler.Arm.Task.InvestigateSound) < num && Random.value < 0.1f)
			{
				Rattler.Arm arm = rattler.arms[Random.Range(0, rattler.arms.Length)];
				if (arm.CurrentTask != Rattler.Arm.Task.InvestigateSound && rattler.ClosestArmDistance(base.noiseTracker.soundToExamine.pos) > 60f)
				{
					arm.InvestigateSound(base.noiseTracker.soundToExamine.pos);
					base.noiseTracker.soundToExamine.Destroy();
				}
			}
			if (Random.value < 0.008f)
			{
				Rattler.Arm arm2 = rattler.arms[Random.Range(0, rattler.arms.Length)];
				if (arm2.CurrentTask == Rattler.Arm.Task.None)
				{
					arm2.Forage();
				}
			}
		}
		targetRattleIntensity = Mathf.Max(targetRattleIntensity - 0.025f, 0f);
	}

	public float GetRattleFactor(Creature creature)
	{
		RelationshipTracker.DynamicRelationship dynamicRelationship = base.tracker.RepresentationForCreature(creature.abstractCreature, addIfMissing: false).dynamicRelationship;
		if (dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Afraid)
		{
			return 0f;
		}
		float intensity = dynamicRelationship.currentRelationship.intensity;
		float num = Vector2.Distance(rattler.mainBodyChunk.pos, creature.DangerPos);
		num *= Custom.LerpMap(Vector2.Dot(creature.mainBodyChunk.vel, (rattler.attachmentPoint - creature.mainBodyChunk.pos).normalized), -5f, 5f, 1.5f, 0.5f);
		if (creature is Player { standing: false })
		{
			num *= 1.5f;
		}
		return intensity * Mathf.InverseLerp(200f, 60f, num);
	}

	public int CountArms(Rattler.Arm.Task task)
	{
		int num = 0;
		Rattler.Arm[] arms = rattler.arms;
		for (int i = 0; i < arms.Length; i++)
		{
			if (arms[i].CurrentTask == task)
			{
				num++;
			}
		}
		return num;
	}

	public void CollidedWithCreature(Creature crit)
	{
		Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForCreature(crit.abstractCreature, addIfMissing: true);
		if (creatureRepresentation == null)
		{
			return;
		}
		CreatureTemplate.Relationship currentRelationship = creatureRepresentation.dynamicRelationship.currentRelationship;
		bool flag = crit is Player && rattler.room.game.IsStorySession && rattler.room.game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility;
		if (!AnyCarryingCreature(creatureRepresentation) && !rattler.safariControlled && !rattler.RecentlyGrabbedCreature(crit.abstractCreature) && !flag && crit.grabbedBy.Count == 0 && (currentRelationship.type == CreatureTemplate.Relationship.Type.Afraid || currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks) && Random.value < currentRelationship.intensity)
		{
			Rattler.Arm arm = rattler.arms[Random.Range(0, rattler.arms.Length)];
			if (arm.CurrentTask == Rattler.Arm.Task.None)
			{
				arm.MoveCreature(creatureRepresentation);
			}
		}
	}

	public bool AnyCarryingCreature(Tracker.CreatureRepresentation rep)
	{
		Rattler.Arm[] arms = rattler.arms;
		for (int i = 0; i < arms.Length; i++)
		{
			if (arms[i].IsMovingCreature(rep))
			{
				return true;
			}
		}
		return false;
	}
}
