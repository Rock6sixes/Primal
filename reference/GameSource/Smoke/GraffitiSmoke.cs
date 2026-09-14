using System;
using RWCustom;
using UnityEngine;

namespace Smoke;

public class GraffitiSmoke : SmokeSystem
{
	public class GraffitiSmokeParticle : SpriteSmoke
	{
		public Color effectColor;

		public Color colorA;

		public float col;

		public float lastCol;

		public int colorFadeTime;

		public float moveDir;

		public float startDir;

		public float dirChange = 2f;

		public float maxDeviance = -1f;

		public float goalSpeed = 0.5f;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
		{
			base.Reset(newOwner, pos, vel, lifeTime);
			col = 0f;
			lastCol = 0f;
			rad = Mathf.Lerp(35f, 50f, UnityEngine.Random.value);
			moveDir = UnityEngine.Random.value * 360f;
			startDir = Custom.VecToDeg(vel);
			moveDir = Custom.VecToDeg(vel) + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * dirChange;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (resting)
			{
				return;
			}
			vel *= 0.7f;
			moveDir += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * dirChange;
			if (maxDeviance != -1f)
			{
				moveDir = Mathf.Clamp(moveDir, startDir - maxDeviance, startDir + maxDeviance);
			}
			vel += Custom.DegToVec(moveDir) * (0.6f * Mathf.Lerp(vel.magnitude, goalSpeed, 0.6f));
			if (room.PointSubmerged(pos))
			{
				pos.y = room.FloatWaterLevel(pos);
			}
			lastCol = col;
			col += 1f;
			if (room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
			{
				IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
				FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
				pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
				if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
				{
					vel.x = Mathf.Abs(vel.x);
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
				{
					vel.x = 0f - Mathf.Abs(vel.x);
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
				{
					vel.y = Mathf.Abs(vel.y);
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
				{
					vel.y = 0f - Mathf.Abs(vel.y);
				}
			}
		}

		public override float Rad(int type, float useLife, float useStretched, float timeStacker)
		{
			return type switch
			{
				0 => Mathf.Lerp(10f, rad, Mathf.Pow(1f - useLife, 0.6f) + useStretched), 
				1 => 1.5f * Mathf.Lerp(5f, rad, Mathf.Pow(1f - useLife, 0.6f)), 
				_ => Mathf.Lerp(10f, rad, Mathf.Pow(1f - useLife, 0.6f)), 
			};
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].shader = room.game.rainWorld.Shaders["FireSmoke"];
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (!resting)
			{
				float num = Mathf.Lerp(lastLife, life, timeStacker);
				float t = Mathf.InverseLerp(colorFadeTime, 0.5f, Mathf.Lerp(lastCol, col, timeStacker));
				sLeaser.sprites[0].color = Color.Lerp(colorA, effectColor, t);
				sLeaser.sprites[1].color = Color.Lerp(colorA, effectColor, t);
				sLeaser.sprites[0].alpha = Mathf.Pow(num, 0.25f) * (1f - stretched);
				sLeaser.sprites[1].alpha = 0.3f + Mathf.Pow(Mathf.Sin(num * (float)Math.PI), 0.7f) * 0.65f * (1f - stretched);
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			colorA = Color.Lerp(palette.blackColor, palette.fogColor, 0.2f);
		}
	}

	public GraffitiSmoke(Room room)
		: base(SmokeType.FireSmoke, room, 2, 4f)
	{
	}

	public override SmokeSystemParticle CreateParticle()
	{
		return new GraffitiSmokeParticle();
	}

	public void EmitSmoke(Vector2 pos, Vector2 vel, Color effectColor, int colorFadeTime, bool connect = false)
	{
		Vector2 vector = ((!connect || particles.Count <= 0) ? pos : particles[particles.Count - 1].pos);
		float num = Vector2.Distance(pos, vector);
		if (num > minParticleDistance * 5f)
		{
			vector = pos;
			num = 0f;
		}
		for (float num2 = minParticleDistance; num2 < num || num2 == minParticleDistance; num2 += minParticleDistance)
		{
			if (AddParticle(Vector2.Lerp(vector, pos, num2 / num), vel, colorFadeTime) is GraffitiSmokeParticle graffitiSmokeParticle)
			{
				graffitiSmokeParticle.effectColor = effectColor;
				graffitiSmokeParticle.colorFadeTime = UnityEngine.Random.Range(60, 100);
				graffitiSmokeParticle.maxDeviance = UnityEngine.Random.Range(15f, 30f);
				graffitiSmokeParticle.life = 1f;
			}
		}
	}
}
