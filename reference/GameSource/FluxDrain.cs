using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class FluxDrain : UpdatableAndDeletable, IDrawable
{
	public struct ParticleInfo
	{
		public Vector2 start;

		public Vector2 end;

		public Vector2 curl;

		public float life;

		public float maxLife;
	}

	public PlacedObject pObj;

	public ParticleInfo[] particles;

	public float minLifetime = 0.3f;

	public float maxLifetime = 0.5f;

	public float trailDuration = 0.05f;

	public float flow;

	public float pullOuterRadius = 250f;

	public float pullInnerRadius = 150f;

	public float pullForce = 0.6f;

	public float tilt;

	public float lastFlow;

	public StaticSoundLoop loop;

	public PlacedObject.WaterFlowData Data => pObj.data as PlacedObject.WaterFlowData;

	public FluxDrain(Room room, PlacedObject placedObject)
	{
		pObj = placedObject;
		particles = new ParticleInfo[Mathf.CeilToInt(40f * (maxLifetime + trailDuration))];
		Random.State state = Random.state;
		Random.InitState((int)pObj.pos.x + (int)pObj.pos.y + (int)(Data.preDelay * 1000f) + (int)(Data.postDelay * 1000f));
		tilt = (((double)Random.value < 0.5) ? 1f : (-1f)) * Random.Range(0.9f, 1.1f);
		Random.state = state;
	}

	public float GetFlow()
	{
		PlacedObject.WaterFlowData data = Data;
		if (!data.dynamic)
		{
			return data.flow;
		}
		WaterLevelCycle waterCycle = room.world.rainCycle.waterCycle;
		WaterLevelCycle.Stage stage = waterCycle.stage;
		float? num = null;
		float? num2 = null;
		switch (stage)
		{
		case WaterLevelCycle.Stage.Rise:
			return 0f;
		case WaterLevelCycle.Stage.Max:
			num = waterCycle.TimeLeftInStage;
			break;
		case WaterLevelCycle.Stage.Fall:
			num = 0f - waterCycle.timeInStage;
			break;
		}
		switch (stage)
		{
		case WaterLevelCycle.Stage.Fall:
			num2 = waterCycle.TimeLeftInStage;
			break;
		case WaterLevelCycle.Stage.Min:
			num2 = 0f - waterCycle.timeInStage;
			break;
		}
		float num3 = 1f;
		if (num.HasValue)
		{
			num3 *= Mathf.Clamp01((num.Value + data.preDelay) / -1f);
		}
		if (num2.HasValue)
		{
			num3 *= Mathf.Pow(Mathf.Clamp01((num2.Value + data.postDelay) / 2f), 3f);
		}
		num3 = Mathf.Max(num3 - room.game.globalRain.floodSpeed, 0f);
		return Mathf.SmoothStep(0f, data.flow, num3);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[particles.Length + 1];
		for (int i = 0; i < particles.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("pixel")
			{
				anchorY = 0f
			};
		}
		sLeaser.sprites[particles.Length] = new CustomFSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["Whirlpool"]
		};
		AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		newContatiner.AddChild(sLeaser.sprites[particles.Length]);
		for (int i = 0; i < particles.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = ((i % 20 == 0) ? new Color(0f, 0.003921569f, 0f) : palette.blackColor);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < particles.Length; i++)
		{
			ParticleInfo particleInfo = particles[i];
			float num = Mathf.Sqrt(Mathf.Clamp01((particleInfo.life - timeStacker / 40f) / particleInfo.maxLife));
			float num2 = Mathf.Sqrt(Mathf.Clamp01((particleInfo.life - timeStacker / 40f + trailDuration) / particleInfo.maxLife));
			sLeaser.sprites[i].isVisible = num > 0f || num2 > 0f;
			if (sLeaser.sprites[i].isVisible)
			{
				Vector2 vector = Vector2.Lerp(particleInfo.end, particleInfo.start, num) + (1f - num) * 4f * num * particleInfo.curl;
				Vector2 vector2 = Vector2.Lerp(particleInfo.end, particleInfo.start, num2) + (1f - num2) * 4f * num2 * particleInfo.curl;
				sLeaser.sprites[i].SetPosition(vector - camPos);
				sLeaser.sprites[i].scaleY = Vector2.Distance(vector, vector2);
				sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
				if (!room.PointSubmerged(vector) || !room.PointSubmerged(vector2))
				{
					sLeaser.sprites[i].isVisible = false;
				}
			}
		}
		CustomFSprite customFSprite = sLeaser.sprites[particles.Length] as CustomFSprite;
		customFSprite.MoveVertice(0, pObj.pos - camPos + new Vector2((float)Data.width * 20f + 10f, 200f));
		customFSprite.MoveVertice(1, pObj.pos - camPos + new Vector2(-10f, 200f));
		customFSprite.MoveVertice(2, pObj.pos - camPos + new Vector2(-10f, 0f));
		customFSprite.MoveVertice(3, pObj.pos - camPos + new Vector2((float)Data.width * 20f + 10f, 0f));
		Water waterObject = room.waterObject;
		float num3 = -1000f;
		if (waterObject != null)
		{
			num3 = waterObject.DetailedWaterLevel(pObj.pos);
			for (int j = 0; j <= 10; j++)
			{
				num3 = Mathf.Min(num3, room.FloatWaterLevel(pObj.pos + Vector2.right * ((float)Data.width * 20f * (float)j) / 10f));
			}
		}
		Color color = new Color(tilt, (num3 - pObj.pos.y) / 200f, 0f, Mathf.Lerp(lastFlow, flow, timeStacker));
		for (int k = 0; k < 4; k++)
		{
			customFSprite.verticeColors[k] = color;
		}
	}

	public override void Update(bool eu)
	{
		lastFlow = flow;
		flow = GetFlow();
		base.Update(eu);
		if (loop == null)
		{
			loop = new StaticSoundLoop(SoundID.Flux_Drain_LOOP, pObj.pos, room, 0f, 0f)
			{
				randomStartPosition = true,
				pitch = Random.Range(0.9f, 1.1f)
			};
		}
		loop.Update();
		loop.volume = flow;
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].life -= 0.025f;
		}
		if (flow > 0f && room.waterObject != null)
		{
			if (Random.value < flow)
			{
				SpawnParticle(room.game.clock % particles.Length);
			}
			float num = room.waterObject.DetailedWaterLevel(pObj.pos + new Vector2((float)Data.width * 10f, 0f));
			float num2 = num - pObj.pos.y;
			loop.pos.x = pObj.pos.x + (float)Data.width * 10f;
			loop.pos.y = num;
			if (num2 > 0f)
			{
				float num3 = Mathf.Clamp01(num2 / 10f) * Mathf.InverseLerp(400f, 200f, num2);
				num3 = num3 * 0.6f + 0.4f;
				num3 *= flow;
				room.waterObject.DrainAffectSurface(pObj.pos + new Vector2((float)Data.width * 10f, 0f), (float)Data.width * 20f, num3);
			}
			if (room.ViewedByAnyCamera(pObj.pos + new Vector2((float)Data.width * 10f, pullOuterRadius / 2f), Mathf.Max(pullOuterRadius, (float)Data.width * 20f)))
			{
				PullInVisuals();
			}
			PullInObjects();
		}
	}

	public void PullInVisuals()
	{
	}

	public void PullInObjects()
	{
		List<PhysicalObject>[] physicalObjects = room.physicalObjects;
		foreach (List<PhysicalObject> list in physicalObjects)
		{
			for (int j = 0; j < list.Count; j++)
			{
				BodyChunk[] bodyChunks = list[j].bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					Vector2 vector = bodyChunk.pos - pObj.pos - new Vector2((float)Data.width * 10f, 0f);
					if (!(vector.y < 0f) && !(vector.y > pullOuterRadius) && !(Mathf.Abs(vector.x) > (float)Data.width * 10f) && !(bodyChunk.submersion <= 0f))
					{
						float num = pullForce * bodyChunk.submersion * (0.5f + 0.5f * flow);
						bodyChunk.vel.y -= Custom.LerpMap(vector.y, pullOuterRadius, pullInnerRadius, 0f, num);
						bodyChunk.vel.x -= Mathf.Sign(vector.x) * 0.1f * num;
					}
				}
			}
		}
	}

	public void SpawnParticle(int i)
	{
		particles[i].end = pObj.pos + new Vector2(Random.value * 20f * (float)Data.width, Random.value * 10f - 5f);
		particles[i].start = particles[i].end + new Vector2(0f, Random.Range(30f, 130f));
		particles[i].curl = Random.value * 10f * Custom.RNV();
		particles[i].maxLife = Random.Range(minLifetime, maxLifetime);
		particles[i].life = particles[i].maxLife;
		if (!room.PointSubmerged(particles[i].start))
		{
			particles[i].life = 0f;
		}
	}
}
