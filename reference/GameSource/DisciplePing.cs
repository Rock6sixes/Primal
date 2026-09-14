using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class DisciplePing : TemplarCircle
{
	public HashSet<PhysicalObject> objectsHit = new HashSet<PhysicalObject>();

	public DisciplePing(PhysicalObject source, Vector2 pos, float rad, float radIncrement, float radAcceleration, int lifeTime)
		: base(source, pos, rad, radIncrement, radAcceleration, lifeTime, followSource: false)
	{
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				PhysicalObject physicalObject = room.physicalObjects[i][j];
				if (source == null || source == physicalObject || physicalObject.slatedForDeletetion || (source.abstractPhysicalObject.rippleLayer != physicalObject.abstractPhysicalObject.rippleLayer && !source.abstractPhysicalObject.rippleBothSides && !physicalObject.abstractPhysicalObject.rippleBothSides) || !Custom.DistLess(pos, physicalObject.firstChunk.pos, rad) || !(Custom.Dist(lastPos, physicalObject.firstChunk.lastPos) > lastRad) || !objectsHit.Add(physicalObject) || source.room == null)
				{
					continue;
				}
				if (physicalObject is Creature creature && physicalObject.firstChunk.vel.magnitude >= 2.5f)
				{
					(source as Scavenger)?.PingReturnInfo(physicalObject, isCreature: true);
					if (creature.Template.TopAncestor().type != CreatureTemplate.Type.Scavenger)
					{
						room.AddObject(new TemplarCircle(physicalObject, physicalObject.firstChunk.pos, 0f, 6f, 0f, 65, followSource: true)
						{
							maxThickness = 6f,
							radDamping = 0.03f,
							maxAlpha = 0.3f
						});
					}
				}
				if (physicalObject is Weapon)
				{
					(source as Scavenger)?.PingReturnInfo(physicalObject, isCreature: false);
				}
			}
		}
	}
}
