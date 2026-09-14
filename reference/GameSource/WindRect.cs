using System;
using System.Collections.Generic;
using RWCustom;
using Smoke;
using UnityEngine;
using Watcher;

public class WindRect : UpdatableAndDeletable
{
	public PlacedObject placedObj;

	public const float _sqrt2 = 1.4142135f;

	public Vector2[] _testPoints = new Vector2[8]
	{
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(0f, -1f),
		new Vector2(-1f, 0f),
		new Vector2(1.4142135f, 1.4142135f) * 0.5f,
		new Vector2(1.4142135f, -1.4142135f) * 0.5f,
		new Vector2(-1.4142135f, -1.4142135f) * 0.5f,
		new Vector2(-1.4142135f, 1.4142135f) * 0.5f
	};

	public PlacedObject.WindData Data => (PlacedObject.WindData)placedObj.data;

	public WindRect(PlacedObject pObj)
	{
		placedObj = pObj;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		float num = Data.velocity * (80f + 10f * Mathf.Sin((float)room.game.clock / 40f * 2f));
		float num2 = 0.02f;
		List<UpdatableAndDeletable> updateList = room.updateList;
		RoomCamera roomCamera = room.game.cameras[0];
		Rect rect = default(Rect);
		rect.xMin = -40f + Mathf.Min(0f, roomCamera.pos.x);
		rect.yMin = -40f + Mathf.Min(0f, roomCamera.pos.y);
		rect.xMax = 40f + Mathf.Max(room.PixelWidth, roomCamera.pos.x + roomCamera.sSize.x);
		rect.yMax = 40f + Mathf.Max(room.PixelHeight, roomCamera.pos.y + roomCamera.sSize.y);
		PlacedObject.WindData.AffectGroup affectGroup = Data.affectGroup;
		for (int num3 = updateList.Count - 1; num3 >= 0; num3--)
		{
			if (WindAffectsObject(updateList[num3], affectGroup))
			{
				if (updateList[num3] is PhysicalObject physicalObject)
				{
					bool flag = physicalObject.abstractPhysicalObject.type != AbstractPhysicalObject.AbstractObjectType.Creature;
					BodyChunk[] bodyChunks = physicalObject.bodyChunks;
					for (int num4 = bodyChunks.Length - 1; num4 >= 0; num4--)
					{
						float num5 = 1f;
						if (bodyChunks[num4].ContactPoint.y == -1)
						{
							if (flag)
							{
								continue;
							}
							num5 = 0.5f;
						}
						if ((float)bodyChunks[num4].ContactPoint.x == 0f - Mathf.Sign(num))
						{
							num5 *= 0.1f;
						}
						num5 *= physicalObject.windAffectiveness;
						float num6 = EstimateOverlap(bodyChunks[num4]);
						num5 *= num2 * num6 / (bodyChunks[num4].mass + 0.5f);
						bodyChunks[num4].vel.x = ApplyWind(bodyChunks[num4].vel.x, num, num5);
					}
					if (physicalObject is Player { graphicsModule: not null } player)
					{
						PlayerGraphics playerGraphics = (PlayerGraphics)player.graphicsModule;
						for (int i = 0; i < playerGraphics.tail.Length; i++)
						{
							if (WindAffectsPoint(playerGraphics.tail[i].pos))
							{
								playerGraphics.tail[i].vel.x = ApplyWind(playerGraphics.tail[i].vel.x, num, num2);
							}
						}
					}
				}
				else if (updateList[num3] is SkyDandelions.SkyDandelion { pos: var pos } skyDandelion)
				{
					if (pos.x < rect.xMin)
					{
						pos.x += rect.width;
					}
					if (pos.x > rect.xMax)
					{
						pos.x -= rect.width;
					}
					skyDandelion.lastPos += pos - skyDandelion.pos;
					skyDandelion.pos = pos;
					if (WindAffectsPoint(skyDandelion.pos))
					{
						skyDandelion.vel.x = ApplyWind(skyDandelion.vel.x, num, num2);
					}
				}
				else if (updateList[num3] is SmokeSystem.SmokeSystemParticle smokeSystemParticle)
				{
					if (WindAffectsPoint(smokeSystemParticle.pos))
					{
						smokeSystemParticle.vel.x = ApplyWind(smokeSystemParticle.vel.x, num, num2);
					}
				}
				else if (updateList[num3] is SaltFlakes.SaltFlake saltFlake && WindAffectsPoint(saltFlake.pos))
				{
					saltFlake.vel.x = ApplyWind(saltFlake.vel.x, num, num2);
				}
			}
		}
	}

	public static bool WindAffectsObject(UpdatableAndDeletable obj, PlacedObject.WindData.AffectGroup group)
	{
		if (!(group == PlacedObject.WindData.AffectGroup.Creatures) && !(obj is SkyDandelions.SkyDandelion) && !(obj is SmokeSystem.SmokeSystemParticle) && !(obj is SaltFlakes.SaltFlake))
		{
			if (obj is PhysicalObject && !(obj is Creature))
			{
				return group == PlacedObject.WindData.AffectGroup.Objects;
			}
			return false;
		}
		return true;
	}

	public float ApplyWind(float vel, float target, float force)
	{
		if (Mathf.Sign(vel) == Mathf.Sign(target) && Mathf.Abs(vel) > Mathf.Abs(target))
		{
			return vel;
		}
		return Mathf.Lerp(vel, target, force);
	}

	public float EstimateOverlap(BodyChunk c)
	{
		float num = 0f;
		for (int num2 = _testPoints.Length - 1; num2 >= 0; num2--)
		{
			Vector2 p = c.pos + c.rad * _testPoints[num2];
			if (WindAffectsPoint(p))
			{
				num += 1f;
			}
		}
		return num / (float)_testPoints.Length;
	}

	public bool WindAffectsPoint(Vector2 p)
	{
		PlacedObject.WindData windData = (PlacedObject.WindData)placedObj.data;
		p -= placedObj.pos;
		return Custom.QuadPolyContainsPoint(p, Vector2.zero, windData.handles[0], windData.handles[1], windData.handles[2]);
	}

	[Obsolete("moved to Custom.TryContainsPoint")]
	public static bool TriContainsPoint(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
	{
		return Custom.TriContainsPoint(p, a, b, c);
	}
}
