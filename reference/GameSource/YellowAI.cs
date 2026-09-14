using System;
using System.Collections.Generic;
using UnityEngine;

public class YellowAI : AIModule
{
	public class YellowPack
	{
		public class Role : ExtEnum<Role>
		{
			public static Role Leader = new Role("Leader", register: true);

			public static Role Support = new Role("Support", register: true);

			public Role(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public class PackMember
		{
			public AbstractCreature lizard;

			public Role role;

			public PackMember(AbstractCreature lizard, Role role)
			{
				this.lizard = lizard;
				this.role = role;
			}
		}

		public string packName;

		public List<PackMember> members;

		public AbstractCreature PackLeader => members[0].lizard;

		public float LeaderDominance => PackLeader.personality.dominance;

		public YellowPack(AbstractCreature firstLizard)
		{
			members = new List<PackMember>
			{
				new PackMember(firstLizard, Role.Leader)
			};
			packName = "Pack " + UnityEngine.Random.Range(0, 1000);
		}

		public void Update()
		{
		}

		public void AddLizard(AbstractCreature newLizard)
		{
			members.Add(new PackMember(newLizard, Role.Support));
			FindLeader();
		}

		public void RemoveLizard(AbstractCreature removeLizard)
		{
			for (int num = members.Count - 1; num >= 0; num--)
			{
				if (members[num].lizard == removeLizard)
				{
					members.RemoveAt(num);
				}
			}
			FindLeader();
		}

		public void RemoveLizard(int index)
		{
			members.RemoveAt(index);
			FindLeader();
		}

		public void FindLeader()
		{
			if (members.Count == 0)
			{
				return;
			}
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < members.Count; i++)
			{
				members[i].role = Role.Support;
				if (members[i].lizard.personality.dominance > num)
				{
					num = members[i].lizard.personality.dominance;
					num2 = i;
				}
			}
			PackMember packMember = members[num2];
			packMember.role = Role.Leader;
			if (num2 > 0)
			{
				members.RemoveAt(num2);
				members.Insert(0, packMember);
			}
		}
	}

	public Lizard lizard;

	public YellowPack pack;

	public int communicating;

	public float commFlicker;

	public List<AbstractCreature> deadLizardsToRemoveFromPack = new List<AbstractCreature>();

	public YellowAI(ArtificialIntelligence AI)
		: base(AI)
	{
		lizard = AI.creature.realizedCreature as Lizard;
		pack = new YellowPack(lizard.abstractCreature);
	}

	public override void Update()
	{
		base.Update();
		if (communicating > 0)
		{
			communicating--;
		}
		bool flag = false;
		if (this.lizard.safariControlled && this.lizard.inputWithDiagonals.HasValue && !this.lizard.inputWithDiagonals.Value.AnyDirectionalInput && this.lizard.inputWithDiagonals.Value.jmp)
		{
			communicating = 14;
			flag = true;
		}
		float num = Mathf.InverseLerp(0f, 14f, communicating);
		if (commFlicker < num)
		{
			commFlicker = Mathf.Min(num, commFlicker + 0.25f);
		}
		else
		{
			commFlicker = Mathf.Max(num, commFlicker - 0.025f);
		}
		for (int i = 0; i < this.lizard.room.abstractRoom.creatures.Count; i++)
		{
			if (this.lizard.room.abstractRoom.creatures[i].creatureTemplate.CreatureRelationship(this.lizard.Template).type == CreatureTemplate.Relationship.Type.Pack && this.lizard.room.abstractRoom.creatures[i].realizedCreature != null && this.lizard.room.abstractRoom.creatures[i].realizedCreature is Lizard lizard && lizard.AI.yellowAI != null && (this.lizard.room.abstractRoom.creatures[i].rippleLayer == AI.creature.rippleLayer || this.lizard.room.abstractRoom.creatures[i].rippleBothSides || AI.creature.rippleBothSides) && this.lizard.room.abstractRoom.creatures[i].realizedCreature.Consious && this.lizard.room.abstractRoom.creatures[i] != AI.creature)
			{
				ConsiderOtherYellowLizard(this.lizard.room.abstractRoom.creatures[i]);
			}
		}
		bool flag2 = false;
		for (int j = 0; j < pack.members.Count; j++)
		{
			if (pack.members[j].lizard.abstractAI.RealAI != null)
			{
				for (int k = 0; k < pack.members[j].lizard.abstractAI.RealAI.tracker.CreaturesCount; k++)
				{
					if (pack.members[j].lizard.realizedCreature != null && pack.members[j].lizard.realizedCreature.Consious)
					{
						PackMemberIsSeeingCreature(pack.members[j].lizard.realizedCreature as Lizard, pack.members[j].lizard.abstractAI.RealAI.tracker.GetRep(k));
						if (flag)
						{
							(pack.members[j].lizard.realizedCreature as Lizard).AI.excitement = 1f;
							(pack.members[j].lizard.realizedCreature as Lizard).AI.runSpeed = 1f;
							pack.members[j].lizard.abstractAI.SetDestination(this.lizard.abstractCreature.pos);
						}
					}
				}
			}
			if (pack.members[j].lizard.realizedCreature != null && pack.members[j].lizard.realizedCreature.dead)
			{
				if (!flag2)
				{
					deadLizardsToRemoveFromPack.Clear();
					flag2 = true;
				}
				deadLizardsToRemoveFromPack.Add(pack.members[j].lizard);
			}
		}
		if (flag2)
		{
			for (int l = 0; l < deadLizardsToRemoveFromPack.Count; l++)
			{
				pack.RemoveLizard(deadLizardsToRemoveFromPack[l]);
			}
		}
	}

	public PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		if (lizard == null || lizard.slatedForDeletetion || lizard.room == null || lizard.AI == null || pack == null)
		{
			return cost;
		}
		if (pack.PackLeader == lizard.abstractCreature)
		{
			return cost;
		}
		if (lizard.AI.behavior == LizardAI.Behavior.Hunt)
		{
			Vector2 a = lizard.room.MiddleOfTile(connection.destinationCoord);
			for (int i = 0; i < pack.members.Count; i++)
			{
				if (pack.members[i].lizard != null && !pack.members[i].lizard.slatedForDeletion && pack.members[i].lizard != lizard.abstractCreature && pack.members[i].lizard.Room == lizard.room.abstractRoom && pack.members[i].lizard.realizedCreature != null)
				{
					cost.resistance += Mathf.InverseLerp(300f, 0f, Vector2.Distance(a, pack.members[i].lizard.realizedCreature.mainBodyChunk.pos)) * 200f / (float)(pack.members.Count - 1);
					if (connection.destinationCoord.Tile == pack.members[i].lizard.pos.Tile)
					{
						cost.resistance += 300f / (float)(pack.members.Count - 1);
					}
				}
			}
		}
		return cost;
	}

	public void PackMemberIsSeeingCreature(Lizard packMember, Tracker.CreatureRepresentation rep)
	{
		if (packMember == lizard || rep.representedCreature.realizedCreature == null || !rep.VisualContact)
		{
			return;
		}
		Tracker.CreatureRepresentation creatureRepresentation = lizard.AI.tracker.RepresentationForObject(rep.representedCreature.realizedCreature, AddIfMissing: false);
		if (creatureRepresentation == null)
		{
			RecieveInfoOnCritter(packMember, rep);
		}
		else
		{
			if (creatureRepresentation.VisualContact)
			{
				return;
			}
			for (int i = 0; i < rep.representedCreature.realizedCreature.bodyChunks.Length; i++)
			{
				if (packMember.AI.VisualContact(rep.representedCreature.realizedCreature.bodyChunks[i]))
				{
					RecieveInfoOnCritter(packMember, rep);
					break;
				}
			}
		}
	}

	public void RecieveInfoOnCritter(Lizard packMember, Tracker.CreatureRepresentation rep)
	{
		lizard.AI.tracker.SeeCreature(rep.representedCreature);
		communicating = Math.Max(communicating, UnityEngine.Random.Range(3, (int)Mathf.Lerp(3f, 50f, rep.dynamicRelationship.currentRelationship.intensity)));
		packMember.AI.yellowAI.communicating = Math.Max(packMember.AI.yellowAI.communicating, UnityEngine.Random.Range(3, (int)Mathf.Lerp(3f, 50f, rep.dynamicRelationship.currentRelationship.intensity)));
	}

	public void ConsiderOtherYellowLizard(AbstractCreature otherLiz)
	{
		if (otherLiz.rippleLayer != lizard.abstractCreature.rippleLayer && !otherLiz.rippleBothSides && !lizard.abstractCreature.rippleBothSides)
		{
			return;
		}
		AI.tracker.SeeCreature(otherLiz);
		if (otherLiz.CanTravelTogether(AI.creature) && Pack(otherLiz.realizedCreature) != pack)
		{
			if (Pack(otherLiz.realizedCreature).LeaderDominance > pack.LeaderDominance)
			{
				pack.RemoveLizard(lizard.abstractCreature);
				Pack(otherLiz.realizedCreature).AddLizard(lizard.abstractCreature);
				pack = Pack(otherLiz.realizedCreature);
			}
			else
			{
				(otherLiz.abstractAI.RealAI as LizardAI).yellowAI.pack.RemoveLizard(otherLiz);
				pack.AddLizard(otherLiz);
				(otherLiz.abstractAI.RealAI as LizardAI).yellowAI.pack = pack;
			}
		}
	}

	public YellowPack Pack(Creature liz)
	{
		return (liz.abstractCreature.abstractAI.RealAI as LizardAI).yellowAI.pack;
	}
}
