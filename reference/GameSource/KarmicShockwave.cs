using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class KarmicShockwave : TemplarCircle
{
	public HashSet<PhysicalObject> creaturesHit = new HashSet<PhysicalObject>();

	public float maxRad;

	public float minRad;

	public KarmicShockwave(PhysicalObject sourceObject, Vector2 pos, int lifeTime, float rad, float maxRad)
		: base(sourceObject, pos, rad, 0f, 0f, lifeTime, followSource: true)
	{
		minRad = rad;
		this.maxRad = maxRad;
		maxThickness = 20f;
	}

	public override void Update(bool eu)
	{
		if (frame == 0)
		{
			room.AddObject(new ShockWave(pos, 200f, 0.08f, 4));
		}
		base.Update(eu);
		rad = Custom.LerpMap(frame, 0f, lifeTime, minRad, maxRad, 0.5f);
		StunCreatures();
	}

	public void StunCreatures()
	{
		float num = rad * 0.7f;
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				if (!(room.physicalObjects[i][j] is Creature creature) || source == null || source == creature || creature.slatedForDeletetion || (source.abstractPhysicalObject.rippleLayer != creature.abstractPhysicalObject.rippleLayer && !source.abstractPhysicalObject.rippleBothSides && !creature.abstractPhysicalObject.rippleBothSides))
				{
					continue;
				}
				BodyChunk closestChunk = GetClosestChunk(creature, pos);
				if (!Custom.DistLess(pos, creature.mainBodyChunk.pos, num + closestChunk.rad) || !creaturesHit.Add(creature))
				{
					continue;
				}
				float num2 = 8f;
				if (ModManager.MSC && room.physicalObjects[i][j] is Player && (room.physicalObjects[i][j] as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
				{
					num2 *= 0.25f;
				}
				BodyChunk[] bodyChunks = creature.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					float num3 = Mathf.InverseLerp(maxRad, maxRad * 0.75f, Vector2.Distance(bodyChunk.pos, pos));
					Vector2 vector = PushAngle(pos, bodyChunk.pos) * (num2 / bodyChunk.mass * num3);
					bodyChunk.vel += vector;
					bodyChunk.pos += vector * 0.1f;
				}
				creature.Violence(null, null, creature.mainBodyChunk, null, Creature.DamageType.Explosion, 0.01f, 2f);
				creature.Stun(50);
				if (creature.graphicsModule?.bodyParts == null)
				{
					continue;
				}
				BodyPart[] bodyParts = creature.graphicsModule.bodyParts;
				foreach (BodyPart bodyPart in bodyParts)
				{
					float num4 = Mathf.InverseLerp(maxRad, maxRad * 0.75f, Vector2.Distance(bodyPart.pos, pos));
					Vector2 vector2 = PushAngle(pos, bodyPart.pos) * (num4 * num2 * 5f);
					bodyPart.pos += vector2;
					bodyPart.vel += vector2;
					if (bodyPart is Limb limb)
					{
						limb.mode = Limb.Mode.Dangle;
					}
				}
			}
		}
	}

	public static BodyChunk GetClosestChunk(PhysicalObject obj, Vector2 pos)
	{
		float num = float.PositiveInfinity;
		BodyChunk result = null;
		BodyChunk[] bodyChunks = obj.bodyChunks;
		foreach (BodyChunk bodyChunk in bodyChunks)
		{
			float num2 = Vector2.Distance(bodyChunk.pos, pos) - bodyChunk.rad;
			if (num2 < num)
			{
				num = num2;
				result = bodyChunk;
			}
		}
		return result;
	}

	public Vector2 PushAngle(Vector2 A, Vector2 B)
	{
		return Vector3.Slerp((B - A).normalized, new Vector2(0f, 1f), 0.2f);
	}
}
