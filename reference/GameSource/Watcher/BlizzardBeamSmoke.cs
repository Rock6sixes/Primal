using System;
using RWCustom;
using Smoke;
using UnityEngine;

namespace Watcher;

public class BlizzardBeamSmoke : SmokeSystem
{
	public class BlizzardParticle : SpriteSmoke
	{
		public Color fadeColor;

		public float colorLerp;

		public float moveDir;

		public int counter;

		public bool big;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float newLifeTime)
		{
			base.Reset(newOwner, pos, vel, newLifeTime);
			rad = Mathf.Lerp(28f, 46f, UnityEngine.Random.value);
			moveDir = UnityEngine.Random.value * 360f;
			counter = 0;
			colorLerp = UnityEngine.Random.value;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (resting)
			{
				return;
			}
			vel *= 0.99f;
			moveDir += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 50f;
			if (room.PointSubmerged(pos))
			{
				pos.y = room.FloatWaterLevel(pos);
			}
			counter++;
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
			float num = type switch
			{
				1 => 1.5f * Mathf.Lerp(2f, rad, Mathf.Pow(1f - useLife, 0.2f)), 
				0 => Mathf.Lerp(4f, rad, Mathf.Pow(1f - useLife, 0.2f) + useStretched), 
				_ => Mathf.Lerp(4f, rad, Mathf.Pow(1f - useLife, 0.2f)), 
			};
			num = ((!big) ? (num * 0.2f) : (num * (1f + Mathf.InverseLerp(0f, 10f, (float)counter + timeStacker))));
			return num * 2f;
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
				Color color = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), new Color(0.7f, 0.7f, 0.9f), colorLerp);
				sLeaser.sprites[0].color = color;
				sLeaser.sprites[1].color = color;
				sLeaser.sprites[0].alpha = Mathf.Pow(num, 0.25f) * (1f - stretched) * ((!big) ? 1f : (1f - 0.2f * Mathf.InverseLerp(0f, 10f, (float)counter + timeStacker)));
				sLeaser.sprites[1].alpha = (0.3f + Mathf.Pow(Mathf.Sin(num * (float)Math.PI), 0.7f) * 0.65f * (1f - stretched)) * ((!big) ? 1f : (1f - 0.2f * Mathf.InverseLerp(0f, 10f, (float)counter + timeStacker)));
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			fadeColor = Color.Lerp(palette.blackColor, palette.fogColor, 0.6f);
		}
	}

	public BlizzardBeamSmoke(Room room)
		: base(WatcherEnums.WatcherSmokeType.BlizzardBeamSmoke, room, 2, 0f)
	{
	}

	public override SmokeSystemParticle CreateParticle()
	{
		return new BlizzardParticle();
	}

	public void EmitSmoke(Vector2 pos, Vector2 vel, bool big, float maxlifeTime)
	{
		if (AddParticle(pos, vel, maxlifeTime * Mathf.Lerp(0.3f, 1f, UnityEngine.Random.value)) is BlizzardParticle blizzardParticle)
		{
			blizzardParticle.big = big;
		}
	}
}
