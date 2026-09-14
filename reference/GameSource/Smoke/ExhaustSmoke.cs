using RWCustom;
using UnityEngine;

namespace Smoke;

public class ExhaustSmoke : MeshSmoke
{
	public class ExhaustSmokeSegment : SmokeSegment
	{
		public int colorCounter;

		public float startConDist;

		public float radFac;

		public float alphaFac;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
		{
			base.Reset(newOwner, pos, vel, lifeTime);
			colorCounter = 0;
			radFac = 0.5f + Random.value;
			alphaFac = 0.6f + 0.4f * Random.value;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			vel.y += 0.1f * Mathf.InverseLerp(0.9f, 1f, life);
			colorCounter++;
		}

		public override float ConDist(float timeStacker)
		{
			return Mathf.Lerp(16f, startConDist, Mathf.Lerp(lastLife, life, timeStacker));
		}

		public override void WindAndDrag(Room rm, ref Vector2 v, Vector2 p)
		{
			v *= Custom.LerpMap(v.magnitude, 5f, 20f, 0.97f, 0.4f);
			v.y += 0.02f;
			v += SmokeSystem.PerlinWind(p, rm);
			if (!rm.readyForAI || rm.aimap.getTerrainProximity(p) >= 3)
			{
				return;
			}
			int terrainProximity = rm.aimap.getTerrainProximity(p);
			Vector2 vector = default(Vector2);
			for (int i = 0; i < 8; i++)
			{
				if (rm.aimap.getTerrainProximity(p + Custom.eightDirections[i].ToVector2() * 20f) > terrainProximity)
				{
					vector += Custom.eightDirections[i].ToVector2();
				}
			}
			v += Vector2.ClampMagnitude(vector, 1f) * 0.015f;
		}

		public override float MyRad(float timeStacker)
		{
			if (resting)
			{
				return 0f;
			}
			return 15f + radFac * (1f - Mathf.Lerp(lastLife, life, timeStacker)) * 15f;
		}

		public override float MyOpactiy(float timeStacker)
		{
			if (resting)
			{
				return 0f;
			}
			return alphaFac * Mathf.Pow(Mathf.Max(Mathf.Lerp(lastLife, life, timeStacker), 0f), 0.15f);
		}

		public override Color MyColor(float timeStacker)
		{
			ExhaustSmoke exhaustSmoke = (ExhaustSmoke)owner;
			return Color.Lerp(exhaustSmoke.startColor, exhaustSmoke.endColor, Mathf.InverseLerp(2f, 5f, (float)colorCounter + timeStacker));
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["SmokeTrail"];
		}
	}

	public PhysicalObject owner;

	public Color startColor;

	public Color endColor;

	public override float ParticleLifeTime => Mathf.Lerp(50f, 80f, Random.value);

	public ExhaustSmoke(Room room, PhysicalObject owner, Color startColor, Color endColor)
		: base(SmokeType.ExhaustSmoke, room, 2, 6f)
	{
		this.owner = owner;
		this.startColor = startColor;
		this.endColor = endColor;
		objectWind = 1f;
	}

	public override void Update(bool eu)
	{
		if (owner != null && owner.room != room && particles.Count == 0)
		{
			Destroy();
		}
		base.Update(eu);
	}

	public void EmitParticle(Vector2 emissionPoint, Vector2 emissionForce)
	{
		AddParticle(emissionPoint, emissionForce, ParticleLifeTime);
	}

	public override SmokeSystemParticle AddParticle(Vector2 emissionPoint, Vector2 emissionForce, float lifeTime)
	{
		if (room.PointSubmerged(emissionPoint))
		{
			if (Random.value < 0.1f)
			{
				room.AddObject(new Bubble(emissionPoint, emissionForce, bottomBubble: false, fakeWaterBubble: false));
			}
			return null;
		}
		ExhaustSmokeSegment exhaustSmokeSegment = base.AddParticle(emissionPoint, emissionForce, lifeTime) as ExhaustSmokeSegment;
		if (exhaustSmokeSegment != null)
		{
			if (exhaustSmokeSegment.nextParticle == null)
			{
				exhaustSmokeSegment.startConDist = 5f;
			}
			else
			{
				exhaustSmokeSegment.startConDist = Vector2.Distance(exhaustSmokeSegment.pos, exhaustSmokeSegment.nextParticle.pos) * 0.8f;
			}
			exhaustSmokeSegment.life = 1f;
			exhaustSmokeSegment.lastLife = exhaustSmokeSegment.life;
		}
		return exhaustSmokeSegment;
	}

	public override SmokeSystemParticle CreateParticle()
	{
		return new ExhaustSmokeSegment();
	}
}
