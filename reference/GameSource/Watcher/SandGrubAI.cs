using Noise;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class SandGrubAI : ArtificialIntelligence, IUseItemTracker, IUseARelationshipTracker
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Hide = new Behavior("Hide", register: true);

		public static Behavior Peek = new Behavior("Peek", register: true);

		public static Behavior Hunt = new Behavior("Hunt", register: true);

		public static Behavior MoveBetweenBurrows = new Behavior("MoveBetweenBurrows", register: true);

		public static Behavior OutOfBurrow = new Behavior("OutOfBurrow", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Behavior behavior;

	public float targetBurrowDepth = 25f;

	public AbstractPhysicalObject huntTarget;

	public Vector2? huntPos;

	public int timeInState;

	public int peekDuration;

	public int scaredCounter;

	public int peeksInThisBurrow;

	public int giveUpOnEatingCounter;

	public Vector2 noisePos;

	public int investigateNoiseCounter;

	public SandGrubBurrow nextPendingSafariBurrow;

	public Vector2? ThreatPos
	{
		get
		{
			Tracker.CreatureRepresentation mostThreateningCreature = base.threatTracker.mostThreateningCreature;
			if (mostThreateningCreature == null)
			{
				return null;
			}
			if (mostThreateningCreature.VisualContact && mostThreateningCreature.representedCreature.realizedCreature != null)
			{
				return mostThreateningCreature.representedCreature.realizedCreature.DangerPos;
			}
			return Grub.room.MiddleOfTile(mostThreateningCreature.BestGuessForPosition());
		}
	}

	public float HuntGracePeriod { get; set; }

	public SandGrub Grub => creature.realizedCreature as SandGrub;

	public bool HuntTargetHoldingGrub
	{
		get
		{
			if (!(huntTarget?.realizedObject is Creature creature))
			{
				return false;
			}
			if (creature.grasps == null)
			{
				return false;
			}
			Creature.Grasp[] grasps = creature.grasps;
			for (int i = 0; i < grasps.Length; i++)
			{
				if (grasps[i]?.grabbed is Creature creature2 && DynamicRelationship(creature2.abstractCreature).type == CreatureTemplate.Relationship.Type.Pack)
				{
					return true;
				}
			}
			return false;
		}
	}

	public SandGrubAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		Grub.ai = this;
		AddModule(new Tracker(this, 4, 1, 20, 0.5f, 1, 1, 3));
		AddModule(new PreyTracker(this, 1, 1.2f, 0f, 400f, 0.75f));
		AddModule(new ItemTracker(this, 40, 2, 4000, -1, stopTrackingCarried: true));
		AddModule(new ThreatTracker(this, 1));
		AddModule(new RelationshipTracker(this, base.tracker));
		behavior = Behavior.Hide;
	}

	public override void Update()
	{
		base.Update();
		if (scaredCounter > 0)
		{
			scaredCounter--;
		}
		if (giveUpOnEatingCounter > 0)
		{
			giveUpOnEatingCounter--;
		}
		if (investigateNoiseCounter > 0)
		{
			investigateNoiseCounter--;
		}
		timeInState++;
		if (Grub.room.VoidWeaverActive)
		{
			Scare(120);
		}
		if (giveUpOnEatingCounter > 0)
		{
			investigateNoiseCounter = 0;
		}
		huntTarget = GetBestHuntTarget();
		if (investigateNoiseCounter > 0 && Custom.DistLess(Grub.HeadChunk.pos, noisePos, 20f))
		{
			investigateNoiseCounter = Mathf.Max(investigateNoiseCounter - 3, 0);
			if (Random.value < 0.05f)
			{
				Vector2 vector = noisePos + new Vector2(Random.value * 2f - 1f, Random.value * 2f - 1f) * 80f;
				if (!Grub.room.GetTile(vector).Solid && (Grub.room.terrain == null || !Grub.room.terrain.Contains(vector)) && Custom.DistLess(vector, Grub.burrow.pos, Grub.tailLength))
				{
					noisePos = vector;
				}
			}
		}
		if (huntTarget?.realizedObject != null)
		{
			huntPos = ClosestChunk(huntTarget.realizedObject, Grub.firstChunk.pos).pos;
			investigateNoiseCounter = 0;
		}
		else if (investigateNoiseCounter > 0)
		{
			huntPos = noisePos;
		}
		else
		{
			huntPos = null;
		}
		if (behavior != Behavior.OutOfBurrow)
		{
			HuntGracePeriod = 1f;
		}
		float b = (huntPos.HasValue ? Vector2.Distance(huntPos.Value, Grub.firstChunk.pos) : float.PositiveInfinity);
		float num = (ThreatPos.HasValue ? Vector2.Distance(ThreatPos.Value, Grub.firstChunk.pos) : float.PositiveInfinity);
		bool flag = num < Mathf.Min(160f, b) || scaredCounter > 0;
		if (Grub.burrow == null)
		{
			SwitchBehavior(Behavior.OutOfBurrow);
		}
		bool flag2 = false;
		bool flag3 = false;
		Vector2 vector2 = Vector2.zero;
		if (Grub.safariControlled)
		{
			Player.InputPackage? inputWithDiagonals = Grub.inputWithDiagonals;
			if (inputWithDiagonals.HasValue)
			{
				Player.InputPackage valueOrDefault = inputWithDiagonals.GetValueOrDefault();
				inputWithDiagonals = Grub.lastInputWithDiagonals;
				if (inputWithDiagonals.HasValue)
				{
					Player.InputPackage valueOrDefault2 = inputWithDiagonals.GetValueOrDefault();
					flag2 = valueOrDefault.pckp && valueOrDefault2.pckp;
					flag3 = valueOrDefault.jmp && !valueOrDefault2.jmp;
					if (valueOrDefault.thrw)
					{
						_ = !valueOrDefault2.thrw;
					}
					else
						_ = 0;
					if (valueOrDefault.AnyDirectionalInput && !valueOrDefault2.AnyDirectionalInput)
					{
						vector2 = new Vector2(valueOrDefault.x, valueOrDefault.y);
					}
				}
			}
			if (behavior == Behavior.Hide)
			{
				if (flag3)
				{
					SwitchBehavior(Grub.Big ? Behavior.OutOfBurrow : Behavior.Hunt);
				}
				if (flag2)
				{
					SwitchBehavior(Behavior.Peek);
				}
				if (vector2 != Vector2.zero)
				{
					nextPendingSafariBurrow = PickNewSafariBurrow(vector2);
					if (nextPendingSafariBurrow != null)
					{
						SwitchBehavior(Behavior.MoveBetweenBurrows);
					}
				}
			}
			else if (behavior == Behavior.Peek)
			{
				if (flag3)
				{
					SwitchBehavior(Grub.Big ? Behavior.OutOfBurrow : Behavior.Hunt);
				}
				else if (!flag2)
				{
					SwitchBehavior(Behavior.Hide);
				}
			}
			else if (behavior == Behavior.Hunt)
			{
				if (Grub.grasps[0] == null)
				{
					if (Grub.Big)
					{
						if (flag3)
						{
							SwitchBehavior(Behavior.OutOfBurrow);
						}
						if (timeInState > 60)
						{
							SwitchBehavior(Behavior.Hide);
						}
					}
					else if (flag3)
					{
						SwitchBehavior(Behavior.Hide);
					}
				}
			}
			else if (behavior == Behavior.OutOfBurrow)
			{
				if (flag3)
				{
					Grub.safariRetract = true;
				}
				huntPos = Grub.HeadChunk.pos + vector2 * 20f;
			}
			if (Grub.safariRetract && behavior != Behavior.OutOfBurrow)
			{
				Grub.safariRetract = false;
			}
		}
		if (behavior == Behavior.Hide)
		{
			targetBurrowDepth = 23f;
			if (!Grub.safariControlled)
			{
				if ((timeInState > 40 && Random.value < 0.01f && !flag) || (huntPos.HasValue && Custom.DistLess(huntPos.Value, Grub.burrow.pos, 100f) && Random.value < 0.05f && !flag) || (Grub.Big && HuntTargetHoldingGrub && Custom.DistLess(huntPos.Value, Grub.burrow.pos, Grub.tailLength - 20f) && Random.value < 0.1f))
				{
					SwitchBehavior(Behavior.Peek);
					peekDuration = Random.Range(40, 120);
					peeksInThisBurrow++;
				}
				else if ((peeksInThisBurrow >= 1 && timeInState > 80 && num < 60f && Random.value < 0.01f) || (peeksInThisBurrow >= 3 && Random.value < 0.001f))
				{
					SwitchBehavior(Behavior.MoveBetweenBurrows);
				}
			}
		}
		else if (behavior == Behavior.Peek)
		{
			targetBurrowDepth = 13f;
			if (Random.value < 0.05f)
			{
				if (huntPos.HasValue && Random.value < 0.7f)
				{
					Grub.stretchDir = Mathf.Sign(huntPos.Value.x - Grub.burrow.pos.x);
				}
				else
				{
					Grub.stretchDir = Random.value * 2f - 1f;
				}
			}
			float dst = ((!Grub.Big) ? 60f : (HuntTargetHoldingGrub ? (Grub.tailLength - 20f) : 100f));
			if (!Grub.safariControlled)
			{
				if (flag)
				{
					SwitchBehavior(Behavior.Hide);
				}
				else if (huntPos.HasValue && Custom.DistLess(huntPos.Value, Grub.burrow.pos, dst))
				{
					SwitchBehavior(Grub.Big ? Behavior.OutOfBurrow : Behavior.Hunt);
				}
				else if (timeInState > peekDuration && (!huntPos.HasValue || !Custom.DistLess(huntPos.Value, Grub.burrow.pos, 120f)))
				{
					SwitchBehavior(Behavior.Hide);
				}
			}
		}
		else if (behavior == Behavior.MoveBetweenBurrows)
		{
			targetBurrowDepth = 27f;
			if (timeInState > 120)
			{
				SwitchBehavior(Behavior.Hide);
			}
			else if (nextPendingSafariBurrow != null)
			{
				SandGrub grub = nextPendingSafariBurrow.grub;
				if (grub != null && grub != Grub)
				{
					if (grub.ai.behavior == Behavior.OutOfBurrow)
					{
						grub.safariRetract = true;
					}
					else if (grub.ai.behavior == Behavior.Hunt)
					{
						GiveUpOnEating();
					}
					else if (grub.ai.behavior == Behavior.Peek)
					{
						grub.ai.SwitchBehavior(Behavior.Hide);
					}
					else if (grub.ai.behavior == Behavior.Hide && timeInState > 40)
					{
						SandGrubBurrow burrow = Grub.burrow;
						Grub.SwitchBurrow(nextPendingSafariBurrow);
						nextPendingSafariBurrow = null;
						grub.SwitchBurrow(burrow);
					}
				}
				else if (timeInState > 40)
				{
					Grub.SwitchBurrow(nextPendingSafariBurrow);
					nextPendingSafariBurrow = null;
				}
			}
			else if (timeInState > 80)
			{
				SandGrubBurrow sandGrubBurrow = PickNewBurrow();
				if (sandGrubBurrow != null)
				{
					Grub.SwitchBurrow(sandGrubBurrow);
				}
			}
		}
		else if (behavior == Behavior.Hunt)
		{
			if (Grub.grasps[0] != null)
			{
				if (Grub.Big)
				{
					targetBurrowDepth = 25f + 15f * Grub.Burying;
				}
				else
				{
					targetBurrowDepth = ((Grub.timeBiting < 15) ? 15f : 25f);
				}
			}
			else if (Grub.safariControlled)
			{
				Grub.stretchDir = vector2.x;
				targetBurrowDepth = (vector2.y + 2f) * 5f;
			}
			else if (timeInState > 160 && Random.value < 0.02f)
			{
				GiveUpOnEating();
			}
			else if (flag && timeInState > 60 && Random.value < 0.02f)
			{
				SwitchBehavior(Behavior.Hide);
			}
			else if (huntPos.HasValue && Custom.DistLess(huntPos.Value, Grub.burrow.pos, 70f))
			{
				Grub.stretchDir = Mathf.Sign(huntPos.Value.x - Grub.burrow.pos.x);
				targetBurrowDepth = Custom.LerpMap(Vector2.Distance(Grub.burrow.pos, huntPos.Value), 40f, 0f, 5f, 15f);
			}
			else
			{
				SwitchBehavior(Behavior.Peek);
				peekDuration = Random.Range(80, 160);
			}
		}
		else if (behavior == Behavior.OutOfBurrow && !Grub.Big)
		{
			if (Random.value < 0.15f)
			{
				Grub.stretchDir = ((Grub.stretchDir < 0f) ? 1f : (-1f));
			}
		}
		else if (behavior == Behavior.OutOfBurrow && Grub.Big)
		{
			HuntGracePeriod = Mathf.Max(HuntGracePeriod - 1f / 60f, 0f);
			if (flag && Random.value < 0.1f)
			{
				giveUpOnEatingCounter = Mathf.Max(Mathf.Max(giveUpOnEatingCounter, scaredCounter), 60);
			}
			if (timeInState > 120 && huntPos.HasValue && !Custom.DistLess(huntPos.Value, Grub.burrow.pos, Grub.tailLength))
			{
				giveUpOnEatingCounter = 20;
			}
		}
	}

	public void Scare(int ticks)
	{
		scaredCounter = Mathf.Max(scaredCounter, ticks);
	}

	public void TouchedCreature(Creature crit)
	{
		if (Grub.Big)
		{
			base.tracker.SeeCreature(crit.abstractCreature);
			Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForCreature(crit.abstractCreature, addIfMissing: true);
			if (creatureRepresentation != null && creatureRepresentation.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Eats && HuntGracePeriod < 0.75f)
			{
				HuntGracePeriod = Mathf.Max(HuntGracePeriod - 0.1f, 0f);
			}
		}
	}

	public void LeaveBurrow()
	{
		SwitchBehavior(Behavior.OutOfBurrow);
	}

	public void ReturnToBurrow()
	{
		if (!Grub.Big || Grub.grasps[0] == null)
		{
			SwitchBehavior(Behavior.Hide);
		}
		else
		{
			SwitchBehavior(Behavior.Hunt);
		}
		Grub.safariRetract = false;
	}

	public void SwitchedBurrow()
	{
		peeksInThisBurrow = 0;
		SwitchBehavior((Grub.burrow == null) ? Behavior.OutOfBurrow : Behavior.Hide);
	}

	public void GiveUpOnEating()
	{
		giveUpOnEatingCounter = Random.Range(200, 320);
	}

	public bool WantsToEat(AbstractPhysicalObject obj)
	{
		if (obj is AbstractCreature absCrit)
		{
			return DynamicRelationship(absCrit).GoForKill;
		}
		return IsObjectEdible(obj);
	}

	public void SwitchBehavior(Behavior newBehav)
	{
		timeInState = 0;
		behavior = newBehav;
	}

	public SandGrubBurrow PickNewBurrow()
	{
		if (Grub.burrow == null || Grub.burrow.network.FreeBurrowCount == 0)
		{
			return null;
		}
		SandGrubBurrow randomFreeBurrow = Grub.burrow.network.GetRandomFreeBurrow();
		if (base.threatTracker.mostThreateningCreature?.representedCreature.realizedCreature == null)
		{
			return randomFreeBurrow;
		}
		Vector2 b = Grub.room.MiddleOfTile(base.threatTracker.mostThreateningCreature.BestGuessForPosition());
		float num = Vector2.Distance(Grub.burrow.pos, b);
		if (Vector2.Distance(randomFreeBurrow.pos, b) > num)
		{
			return randomFreeBurrow;
		}
		return null;
	}

	public SandGrubBurrow PickNewSafariBurrow(Vector2 dir)
	{
		if (Grub.burrow == null)
		{
			return null;
		}
		float num = float.MaxValue;
		SandGrubBurrow result = null;
		foreach (SandGrubBurrow allBurrow in Grub.burrow.network.GetAllBurrows())
		{
			if (allBurrow != Grub.burrow)
			{
				float num2 = Vector2.Dot(dir, allBurrow.pos - Grub.burrow.pos);
				if (num > num2 && num2 > 0f)
				{
					num = num2;
					result = allBurrow;
				}
			}
		}
		return result;
	}

	public AbstractPhysicalObject GetBestHuntTarget()
	{
		if (giveUpOnEatingCounter > 0)
		{
			return null;
		}
		AbstractPhysicalObject result = null;
		float dst = float.PositiveInfinity;
		Vector2 vector = Grub.burrow?.pos ?? Grub.firstChunk.pos;
		for (int i = 0; i < base.itemTracker.ItemCount; i++)
		{
			ItemTracker.ItemRepresentation rep = base.itemTracker.GetRep(i);
			if (rep.VisualContact && rep.representedItem.realizedObject != null && Custom.DistLess(rep.representedItem.realizedObject.firstChunk.pos, vector, dst))
			{
				result = rep.representedItem;
				dst = Vector2.Distance(rep.representedItem.realizedObject.firstChunk.pos, vector);
			}
		}
		Tracker.CreatureRepresentation mostAttractivePrey = base.preyTracker.MostAttractivePrey;
		if (mostAttractivePrey != null && mostAttractivePrey.VisualContact && mostAttractivePrey.representedCreature.realizedCreature != null && Custom.DistLess(ClosestChunk(mostAttractivePrey.representedCreature.realizedObject, vector).pos, vector, dst))
		{
			dst = Vector2.Distance(ClosestChunk(mostAttractivePrey.representedCreature.realizedObject, vector).pos, vector);
			result = mostAttractivePrey.representedCreature;
		}
		return result;
	}

	public bool IsObjectEdible(AbstractPhysicalObject obj)
	{
		if (obj.Room != creature.Room || (!obj.rippleBothSides && !creature.rippleBothSides && obj.rippleLayer != creature.rippleLayer))
		{
			return false;
		}
		AbstractPhysicalObject.AbstractObjectType type = obj.type;
		if (!(type == AbstractPhysicalObject.AbstractObjectType.DangleFruit) && !(type == AbstractPhysicalObject.AbstractObjectType.EggBugEgg) && !(type == AbstractPhysicalObject.AbstractObjectType.NeedleEgg) && !(type == AbstractPhysicalObject.AbstractObjectType.FlyLure) && !(type == AbstractPhysicalObject.AbstractObjectType.JellyFish) && !(type == AbstractPhysicalObject.AbstractObjectType.SlimeMold))
		{
			if (type == AbstractPhysicalObject.AbstractObjectType.WaterNut)
			{
				return (obj as WaterNut.AbstractWaterNut).swollen;
			}
			return false;
		}
		return true;
	}

	public BodyChunk ClosestChunk(PhysicalObject obj, Vector2 pos)
	{
		float num = float.PositiveInfinity;
		BodyChunk result = null;
		BodyChunk[] bodyChunks = obj.bodyChunks;
		foreach (BodyChunk bodyChunk in bodyChunks)
		{
			float num2 = Vector2.Distance(bodyChunk.pos, pos);
			if (num2 < num)
			{
				num = num2;
				result = bodyChunk;
			}
		}
		return result;
	}

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, StaticRelationship(otherCreature).intensity, forgetWhenNotVisible: false);
	}

	public override void HeardNoise(InGameNoise noise)
	{
		if ((noise.sourceObject != null && noise.sourceObject.abstractPhysicalObject.rippleLayer != creature.rippleLayer && !noise.sourceObject.abstractPhysicalObject.rippleBothSides && !creature.rippleBothSides) || giveUpOnEatingCounter > 0)
		{
			return;
		}
		float num = 0.5f;
		if (noise.sourceObject is Creature crit)
		{
			CreatureTemplate.Relationship relationship = creature.creatureTemplate.CreatureRelationship(crit);
			num = ((!(relationship.type == (Grub.Big ? CreatureTemplate.Relationship.Type.Eats : CreatureTemplate.Relationship.Type.Afraid))) ? 0f : relationship.intensity);
		}
		float num2 = 1f;
		if (!Custom.DistLess(Grub.HeadChunk.pos, noise.pos, noise.strength * (1f - Grub.Deaf) * num2 * num * (1f - Grub.room.BackgroundNoise)))
		{
			return;
		}
		if (Grub.Big)
		{
			if (Custom.DistLess(noise.pos, Grub.burrow.pos, Grub.tailLength))
			{
				noisePos = noise.pos;
				investigateNoiseCounter = Random.Range(160, 280);
			}
		}
		else
		{
			Scare((int)(80f * noise.interesting));
		}
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
		CreatureTemplate.Relationship result = StaticRelationship(dRelation.trackerRep.representedCreature);
		if (dRelation.trackerRep.representedCreature.state.dead && result.type != CreatureTemplate.Relationship.Type.Ignores)
		{
			return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Mathf.InverseLerp(5f, 0f, dRelation.trackerRep.representedCreature.creatureTemplate.bodySize));
		}
		return result;
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	public bool TrackItem(AbstractPhysicalObject obj)
	{
		return IsObjectEdible(obj);
	}

	public void SeeThrownWeapon(PhysicalObject obj, Creature thrower)
	{
	}
}
