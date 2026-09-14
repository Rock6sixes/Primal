using System;
using Noise;
using RWCustom;
using UnityEngine;
using Watcher;

public class KarmicArmor : UpdatableAndDeletable, IDrawable
{
	public class EnergyStrand : CosmeticSprite
	{
		public SimpleSegment[] segments;

		public float[] connectionLengths;

		public float thickness;

		public Color color;

		public int age;

		public int lifetime;

		public EnergyStrand(int segments, float thickness)
		{
			this.segments = new SimpleSegment[segments];
			this.thickness = thickness;
			lifetime = UnityEngine.Random.Range(30, 80);
			connectionLengths = new float[segments - 1];
			for (int i = 0; i < connectionLengths.Length; i++)
			{
				connectionLengths[i] = 5f;
			}
			color = Color.Lerp(RainWorld.SaturatedGold, Color.white, UnityEngine.Random.value);
		}

		public void UpdateConnectionLengths(float factor = 1f)
		{
			for (int i = 0; i < connectionLengths.Length; i++)
			{
				connectionLengths[i] = Vector2.Distance(segments[i].pos, segments[i + 1].pos) * factor;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			age++;
			if (age >= lifetime)
			{
				Destroy();
			}
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].lastPos = segments[i].pos;
			}
			for (int j = 0; j < segments.Length; j++)
			{
				segments[j].vel *= 0.97f;
				segments[j].vel += 0.1f * GetWind(segments[j].pos, room.game.clock);
				if (j > 0)
				{
					ConnectSegments(j - 1, j);
				}
				if (j < segments.Length - 1)
				{
					ConnectSegments(j, j + 1);
				}
			}
			for (int k = 0; k < segments.Length; k++)
			{
				segments[k].pos += segments[k].vel;
			}
		}

		public static Vector2 GetWind(Vector2 pos, int time)
		{
			float num = (float)time / 40f * 0.5f;
			return new Vector2((Mathf.PerlinNoise(pos.x * 0.02f + num, pos.y * 0.02f) - 0.46535593f) * 2f, (Mathf.PerlinNoise(pos.x * 0.02f + num, pos.y * 0.02f + 200f) - 0.46535593f) * 2f);
		}

		public void ConnectSegments(int a, int b)
		{
			Vector2 vector = segments[b].pos - segments[a].pos;
			float magnitude = vector.magnitude;
			float num = connectionLengths[a];
			if (magnitude > 0.01f)
			{
				Vector2 vector2 = vector / magnitude * ((magnitude - num) * 0.1f);
				segments[a].vel += vector2;
				segments[b].vel -= vector2;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: false);
			triangleMesh.shader = rCam.game.rainWorld.Shaders["FlatLight"];
			for (int i = 0; i < segments.Length - 1; i++)
			{
				float x = (float)i / (float)(segments.Length - 1);
				float x2 = (float)(i + 1) / (float)(segments.Length - 1);
				triangleMesh.UVvertices[i * 4] = new Vector2(x, 0f);
				triangleMesh.UVvertices[i * 4 + 1] = new Vector2(x, 1f);
				triangleMesh.UVvertices[i * 4 + 2] = new Vector2(x2, 0f);
				triangleMesh.UVvertices[i * 4 + 3] = new Vector2(x2, 1f);
			}
			sLeaser.sprites[0] = triangleMesh;
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("GrabShaders"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
			Vector2 vector = segments[0].DrawPos(timeStacker);
			float num = 0f;
			float num2 = Mathf.Clamp01(((float)(lifetime - age) - timeStacker) / (float)lifetime);
			for (int i = 0; i < segments.Length; i++)
			{
				float num3 = (float)i / (float)(segments.Length - 1);
				Vector2 vector2 = segments[i].DrawPos(timeStacker);
				Vector2 vector3 = Custom.PerpendicularVector((vector2 - vector).normalized);
				float num4 = thickness / 2f * num2 * Mathf.Sin(num3 * (float)Math.PI);
				triangleMesh.MoveVertice(i * 4, vector + vector3 * num - camPos);
				triangleMesh.MoveVertice(i * 4 + 1, vector - vector3 * num - camPos);
				triangleMesh.MoveVertice(i * 4 + 2, vector2 + vector3 * num4 - camPos);
				triangleMesh.MoveVertice(i * 4 + 3, vector2 - vector3 * num4 - camPos);
				vector = vector2;
				num = num4;
			}
			triangleMesh.alpha = num2 * Mathf.Sqrt(UnityEngine.Random.value);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[0].color = color;
		}
	}

	public PhysicalObject sourceObject;

	public bool subtleMode = true;

	public int karmaLevel;

	public float shakeAmount;

	public Vector2 pos;

	public SoundID[] tickSounds = new SoundID[9]
	{
		WatcherEnums.WatcherSoundID.Templar_Shield_Tick_1,
		WatcherEnums.WatcherSoundID.Templar_Shield_Tick_2,
		WatcherEnums.WatcherSoundID.Templar_Shield_Tick_3,
		WatcherEnums.WatcherSoundID.Templar_Shield_Tick_4,
		WatcherEnums.WatcherSoundID.Templar_Shield_Tick_5,
		WatcherEnums.WatcherSoundID.Templar_Shield_Tick_6,
		WatcherEnums.WatcherSoundID.Templar_Shield_Tick_7,
		WatcherEnums.WatcherSoundID.Templar_Shield_Tick_8,
		WatcherEnums.WatcherSoundID.Templar_Shield_Tick_9
	};

	public float rad;

	public float lastShakeAmount;

	public Vector2 lastPos;

	public float alpha;

	public float lastAlpha;

	public int OwnerKarma => (sourceObject as Scavenger).karmaLevels;

	public bool ExplosionImminent => karmaLevel == 0;

	public bool IsTicking => (sourceObject as Scavenger).karmicArmorTriggered;

	public KarmicArmor(PhysicalObject sourceObject, int karmaLevel)
	{
		this.sourceObject = sourceObject;
		this.karmaLevel = karmaLevel;
		rad = 40f;
		pos = sourceObject.firstChunk.pos;
		lastPos = pos;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		Texture2D value = HeavyTexturesCache.LoadMiscTexture(AssetManager.ResolveFilePath("illustrations/bigkarma.png"), TextureWrapMode.Clamp, FilterMode.Bilinear);
		Shader.SetGlobalTexture("_BigKarmaTex", value);
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["KarmicShield"];
		AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer container)
	{
		if (container == null)
		{
			container = rCam.ReturnFContainer("GrabShaders");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node in sprites)
		{
			container.AddChild(node);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float g = Mathf.Lerp(lastShakeAmount, shakeAmount, timeStacker);
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].scale = rad / 8f * 1.2f;
		sLeaser.sprites[0].color = new Color((float)karmaLevel / 10f, g, 0f, Mathf.Lerp(lastAlpha, alpha, timeStacker));
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void Update(bool eu)
	{
		lastPos = pos;
		lastShakeAmount = shakeAmount;
		lastAlpha = alpha;
		if (IsTicking)
		{
			alpha = 1f;
		}
		else if (subtleMode)
		{
			alpha = Custom.LerpAndTick(alpha, 0.05f, 0.01f, 0.01f);
		}
		else
		{
			alpha = Custom.LerpAndTick(alpha, 0.5f, 0.05f, 0.02f);
		}
		if (shakeAmount > 0f && !ExplosionImminent)
		{
			shakeAmount = Mathf.Max(shakeAmount - 0.01f, 0f);
		}
		else if (ExplosionImminent)
		{
			shakeAmount = Mathf.Min(shakeAmount + 0.015f, 1f);
			if (shakeAmount >= 1f)
			{
				Explode();
			}
		}
		if (karmaLevel != OwnerKarma)
		{
			room.PlaySound(tickSounds[Custom.IntClamp(0, 8, karmaLevel - 1)]);
			karmaLevel = OwnerKarma;
			shakeAmount = Mathf.Clamp01(shakeAmount + (ExplosionImminent ? 0.5f : 0.6f));
			if (karmaLevel > 0)
			{
				room.AddObject(new TemplarCircle(sourceObject, pos, rad, 7f, 0f, 25, followSource: true)
				{
					radDamping = 0.1f
				});
			}
		}
		if (sourceObject.slatedForDeletetion || sourceObject.room != room)
		{
			Destroy();
		}
		pos = sourceObject.firstChunk.pos;
		base.Update(eu);
	}

	public void DeflectedProjectile(Vector2 hitPos)
	{
		room.PlaySound(WatcherEnums.WatcherSoundID.Templar_Shield_Deflect);
		Vector2 normalized = (hitPos - pos).normalized;
		Vector2 vector = Custom.PerpendicularVector(normalized);
		hitPos = pos + normalized * rad;
		int num = UnityEngine.Random.Range(10, 20);
		for (int i = 0; i < num; i++)
		{
			SpawnSpark(hitPos, Custom.RNV() * 6f + vector * (UnityEngine.Random.value * 10f) + normalized * (UnityEngine.Random.value * 5f));
		}
		SpawnStrand(normalized, UnityEngine.Random.Range(30f, 45f), 1f);
	}

	public void Explode()
	{
		room.PlaySound(WatcherEnums.WatcherSoundID.Templar_Shield_Explode);
		room.InGameNoise(new InGameNoise(pos, 800f, sourceObject, 1f));
		room.ScreenMovement(pos, default(Vector2), 0.9f);
		room.AddObject(new KarmicShockwave(sourceObject, pos, 20, 15f, 300f));
		int num = 30;
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = Custom.RNV();
			Vector2 vector2 = Custom.PerpendicularVector(vector);
			SpawnSpark(vector * rad + pos, Custom.RNV() * 6f + vector2 * (UnityEngine.Random.value * 5f) + vector * (UnityEngine.Random.value * 5f));
		}
		int num2 = UnityEngine.Random.Range(8, 12);
		for (int j = 0; j < num2; j++)
		{
			SpawnStrand(Custom.RNV(), UnityEngine.Random.Range(45f, 100f), UnityEngine.Random.value * 2f + 0.5f);
		}
		room.AddObject(new Explosion.ExplosionLight(pos, 200f, 1f, 5, RainWorld.SaturatedGold));
		room.AddObject(new ExplosionSpikes(room, pos, 12, rad * 0.4f, 9f, 6f, 100f, RainWorld.SaturatedGold));
		Destroy();
	}

	public Spark SpawnSpark(Vector2 pos, Vector2 vel)
	{
		if (room.GetTile(pos).Solid)
		{
			return null;
		}
		Spark spark = new Spark(pos, vel, Color.Lerp(RainWorld.SaturatedGold, Color.white, UnityEngine.Random.value), null, 20, 30);
		room.AddObject(spark);
		return spark;
	}

	public EnergyStrand SpawnStrand(Vector2 dir, float degrees, float force)
	{
		float num = Custom.VecToDeg(dir) - degrees / 2f;
		float b = num + degrees;
		EnergyStrand energyStrand = new EnergyStrand(Mathf.Max(Mathf.RoundToInt(degrees / 3f), 5), UnityEngine.Random.Range(1f, 2f));
		for (int i = 0; i < energyStrand.segments.Length; i++)
		{
			float num2 = (float)i / (float)(energyStrand.segments.Length - 1);
			Vector2 vector = Custom.DegToVec(Mathf.Lerp(num, b, num2));
			energyStrand.segments[i].Reset(vector * rad + pos);
			energyStrand.segments[i].vel = pos - lastPos + vector * ((2f + Mathf.Sin(num2 * (float)Math.PI)) * force) + UnityEngine.Random.insideUnitCircle * 2f;
		}
		energyStrand.UpdateConnectionLengths(1.1f);
		room.AddObject(energyStrand);
		return energyStrand;
	}
}
