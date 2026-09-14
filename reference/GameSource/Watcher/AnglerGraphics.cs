using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class AnglerGraphics : GraphicsModule
{
	public struct Spike
	{
		public float length;

		public float rad;

		public bool topSide;

		public float placement;

		public Spike(float length, float rad, bool topSide, float placement)
		{
			this.length = length;
			this.rad = rad;
			this.topSide = topSide;
			this.placement = placement;
		}
	}

	public struct Tooth
	{
		public float length;

		public float rad;

		public float curve;

		public float forwardTilt;

		public Tooth(float length, float rad, float curve, float forwardTilt)
		{
			this.length = length;
			this.rad = rad;
			this.curve = curve;
			this.forwardTilt = forwardTilt;
		}
	}

	public Angler fish;

	public float eyeSize;

	public Color eyeColor;

	public float rudderLength;

	public float coneRad;

	public float armLowLength = 25f;

	public float armHighLength = 30f;

	public float armLowExtra = 10f;

	public Spike[] spikes;

	public Tooth[] topTeeth;

	public Tooth[] bottomTeeth;

	public int numberOfProps = 5;

	public Color blackColor;

	public float propSpin;

	public Vector2 lightPos;

	public Vector2 lastLightPos;

	public Vector2 lightVel;

	public float[,] flicker;

	public int lightLinger;

	public float angerRisk;

	public float angerRiskTarget;

	public ChunkSoundEmitter shriekSound;

	public int shriekCooldown;

	public int sonarTimer;

	public int sonarSlowLoopLength = 120;

	public int sonarFastLoopLength = 16;

	public float sonarPitchVariation = 1f;

	public int swimTimer;

	public int swimLoopLength = 30;

	public bool lightsOn => lightLinger > 0;

	public Color lightColor
	{
		get
		{
			if (fish.AI.angerTimer <= 0)
			{
				return Color.Lerp(Color.Lerp(eyeColor, Color.green, 0.25f), Color.red, Mathf.Pow(angerRisk, 2f) * 0.9f);
			}
			return Color.red;
		}
	}

	public float SonarPitch => Mathf.Lerp(0.8f, 1.5f, SonarExcitement) * sonarPitchVariation;

	public int SonarLoopLength
	{
		get
		{
			if (!lightsOn)
			{
				return sonarSlowLoopLength;
			}
			return (int)Mathf.Lerp(sonarSlowLoopLength, sonarFastLoopLength, SonarExcitement);
		}
	}

	public float SonarExcitement
	{
		get
		{
			if (!lightsOn)
			{
				return 0f;
			}
			if (fish.AI.angerTimer > 0)
			{
				return 1f;
			}
			if (angerRisk == 0f)
			{
				return Custom.LerpMap(lightLinger, 0f, 70f, 0f, 0.3f);
			}
			return Mathf.Pow(angerRisk, 2f) * 0.9f;
		}
	}

	public int BodyMesh => 3;

	public int ConeMesh => 4;

	public int JawSprite => 5;

	public int HeadSprite => 6;

	public int CheekSprite => 7;

	public int RudderMesh => 8;

	public int FirstSpikeMesh => 9;

	public int FirstBottomToothMesh => 9 + spikes.Length;

	public int FirstTopToothMesh => 9 + spikes.Length + bottomTeeth.Length;

	public int EyeSprite => 9 + spikes.Length + bottomTeeth.Length + topTeeth.Length;

	public int FirstArmSprite => EyeSprite + 1;

	public int FirstPropSprite => EyeSprite + 8;

	public int TotalSprites => 17 + spikes.Length + bottomTeeth.Length + topTeeth.Length + numberOfProps;

	public AnglerGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		fish = ow as Angler;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(fish.abstractCreature.ID.RandomSeed);
		numberOfProps = UnityEngine.Random.Range(3, 7);
		spikes = new Spike[7];
		spikes[0] = new Spike(40f, 2f, topSide: true, 1f);
		spikes[1] = new Spike(60f, 2f, topSide: true, 0.5f);
		spikes[2] = new Spike(60f, 2f, topSide: false, 0.5f);
		spikes[3] = new Spike(20f, 1.5f, topSide: true, 0.25f);
		spikes[4] = new Spike(20f, 1.5f, topSide: false, 0.25f);
		spikes[5] = new Spike(20f, 1.5f, topSide: true, 0.65f);
		spikes[6] = new Spike(20f, 1.5f, topSide: false, 0.65f);
		int num = UnityEngine.Random.Range(7, 14);
		bottomTeeth = new Tooth[num - UnityEngine.Random.Range(0, 4)];
		topTeeth = new Tooth[num - UnityEngine.Random.Range(0, 4)];
		for (int i = 0; i < bottomTeeth.Length; i++)
		{
			float num2 = Mathf.Pow(1f - (float)i / ((float)bottomTeeth.Length - 1f) - 1f, 4f) * 0.65f + 0.35f;
			bottomTeeth[i] = new Tooth(21f * num2 + UnityEngine.Random.value * 5f, 1.5f - UnityEngine.Random.value * 0.5f, 0.15f + UnityEngine.Random.value * 0.07f, 0.9f + UnityEngine.Random.value * 0.7f);
		}
		for (int j = 0; j < topTeeth.Length; j++)
		{
			float num3 = Mathf.Pow(1f - (float)j / ((float)bottomTeeth.Length - 1f) - 1f, 4f) * 0.65f + 0.35f;
			topTeeth[j] = new Tooth(18f * num3 + UnityEngine.Random.value * 5f, 1.5f - UnityEngine.Random.value * 0.5f, 0.09f + UnityEngine.Random.value * 0.07f, 0.5f + UnityEngine.Random.value * 0.4f);
		}
		eyeSize = 0.5f + UnityEngine.Random.value * 0.65f;
		eyeColor = new HSLColor(0.027f + 0.153f * UnityEngine.Random.value, 1f, 0.5f).rgb;
		rudderLength = UnityEngine.Random.value;
		coneRad = UnityEngine.Random.Range(15f, 24f);
		armLowLength = UnityEngine.Random.Range(25f, 60f);
		armHighLength = UnityEngine.Random.Range(25f, 70f);
		armLowExtra = UnityEngine.Random.Range(5f, 25f);
		sonarSlowLoopLength = UnityEngine.Random.Range(100, 140);
		sonarFastLoopLength = UnityEngine.Random.Range(16, 24);
		sonarPitchVariation = UnityEngine.Random.Range(0.9f, 1.1f);
		UnityEngine.Random.state = state;
		sonarTimer = UnityEngine.Random.Range(0, sonarSlowLoopLength);
		flicker = new float[2, 3];
		float num4 = (lightsOn ? 1f : 0f);
		for (int k = 0; k < flicker.GetLength(0); k++)
		{
			flicker[k, 0] = num4;
			flicker[k, 1] = num4;
			flicker[k, 2] = num4;
		}
	}

	public override void Update()
	{
		base.Update();
		angerRisk = Custom.LerpAndTick(angerRisk, angerRiskTarget, 0.05f, 0.01f);
		if (!fish.dead && fish.safariControlled && (fish.safariLightInput || fish.AI.angerTimer > 0))
		{
			lightLinger = 70;
		}
		if (!fish.dead && !fish.safariControlled && (fish.AI.lockedCreature != null || fish.AI.angerTimer > 0))
		{
			lightLinger = 70;
		}
		else if (lightLinger > 0)
		{
			lightLinger--;
		}
		for (int i = 0; i < flicker.GetLength(0); i++)
		{
			flicker[i, 1] = flicker[i, 0];
			flicker[i, 0] += Mathf.Pow(UnityEngine.Random.value, 3f) * 0.1f * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			flicker[i, 0] = Custom.LerpAndTick(flicker[i, 0], flicker[i, 2], 0.05f, 1f / 30f);
			if (UnityEngine.Random.value < 0.2f && lightsOn)
			{
				flicker[i, 2] = 1f + Mathf.Pow(UnityEngine.Random.value, 3f) * 0.2f * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			}
			flicker[i, 2] = Mathf.Lerp(flicker[i, 2], lightsOn ? 1f : 0f, 0.03f);
			if (!lightsOn && (double)flicker[i, 0] < 0.1 && (double)flicker[i, 2] < 0.1)
			{
				flicker[i, 0] = 0f;
				flicker[i, 1] = 0f;
				flicker[i, 2] = 0f;
			}
		}
		float num = fish.swimSpeed * 0.15f;
		propSpin += num;
		Vector2 vector = Custom.DirVec(fish.bodyChunks[2].pos, fish.bodyChunks[1].pos);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		Vector2 pos = fish.bodyChunks[1].pos + vector2 * ((UnityEngine.Random.value * 2f - 1f) * 60f) + UnityEngine.Random.insideUnitCircle * 7f;
		for (int j = 0; j < 3; j++)
		{
			if (UnityEngine.Random.value < Mathf.Pow(fish.swimSpeed, 2f) * 0.9f)
			{
				if (fish.room.PointSubmerged(pos))
				{
					bool flag = UnityEngine.Random.value < 0.5f;
					fish.room.AddObject(new Bubble(pos, -vector * (num * 80f * (UnityEngine.Random.value * 0.5f + 0.5f)) + vector2 * (num * 140f * (flag ? 1f : (-1f)) * (UnityEngine.Random.value * 0.7f + 0.3f)) + Custom.RNV() * (Mathf.Pow(UnityEngine.Random.value, 2.1f) * 2.5f), bottomBubble: false, fakeWaterBubble: false, !flag));
				}
				else if ((double)fish.bodyChunks[1].submersion > 0.01)
				{
					fish.room.AddObject(new WaterDrip(pos, -vector * (num * 30f * (UnityEngine.Random.value * 0.5f + 0.5f)) + vector2 * (num * 65f * (UnityEngine.Random.value * 0.7f + 0.3f)) + Custom.RNV() * (UnityEngine.Random.value * 5f), waterColor: false));
				}
			}
		}
		Vector2 vector3 = Custom.DirVec(fish.bodyChunks[1].pos, fish.bodyChunks[0].pos);
		Vector2 vector4 = Custom.PerpendicularVector(vector3);
		Vector2 vector5 = fish.bodyChunks[0].pos + vector3 * (fish.bodyChunks[0].rad + (armLowLength + armHighLength) * Mathf.Min((armLowLength + armHighLength) / 120f, 0.87f) + Mathf.Cos(fish.flip * (float)Math.PI * 0.5f) * 10f) + vector4 * (30f + 10f * fish.mouthOpenness) * Mathf.Sin(fish.flip * (float)Math.PI * 0.5f);
		lastLightPos = lightPos;
		bool flag2 = fish.room.PointSubmerged(lightPos);
		lightVel *= 0.8f - (flag2 ? 0.2f : 0f);
		lightVel += (vector5 - lightPos) * (0.3f - (flag2 ? 0.1f : 0f));
		lightPos += lightVel;
		Vector2? vector6 = SharedPhysics.ExactTerrainRayTracePos(fish.room, fish.bodyChunks[0].pos + vector4 * (fish.flip * fish.bodyChunks[0].rad * 0.7f), lightPos);
		if (vector6.HasValue)
		{
			lightPos = vector6.Value;
			lightVel *= 0.01f;
		}
		if (fish.lightSource != null)
		{
			fish.lightSource.setPos = lightPos;
			fish.lightSource.setRad = 300f * flicker[0, 0];
			fish.lightSource.setAlpha = Mathf.Min((flicker[0, 0] + flicker[1, 0]) * 0.5f, 1f);
			fish.lightSource.color = lightColor;
			if (fish.lightSource.slatedForDeletetion || fish.lightSource.room != fish.room)
			{
				fish.lightSource = null;
			}
		}
		sonarTimer++;
		if (sonarTimer > SonarLoopLength)
		{
			float sonarExcitement = SonarExcitement;
			float pitch = SonarPitch + UnityEngine.Random.Range(-0.2f, 0.2f) * SonarExcitement;
			float num2 = Mathf.Lerp(0.6f, 1.2f, sonarExcitement);
			RoomCamera[] cameras = fish.room.game.cameras;
			foreach (RoomCamera roomCamera in cameras)
			{
				if (roomCamera.room == fish.room)
				{
					roomCamera.virtualMicrophone.PlaySound(WatcherEnums.WatcherSoundID.Angler_Pulse, fish.GlowPos, num2 * roomCamera.virtualMicrophone.underWater, pitch);
				}
			}
			sonarTimer = 0;
		}
		if (fish.AI.angerTimer > 0 && fish.firstChunk.submersion >= 1f)
		{
			swimTimer++;
			if (swimTimer > swimLoopLength)
			{
				swimTimer = 0;
				fish.room.PlaySound(WatcherEnums.WatcherSoundID.Angler_Swim, fish.firstChunk, loop: false, Custom.LerpMap(fish.firstChunk.vel.magnitude, 1f, 5f, 0.5f, 1f), 1f);
			}
		}
		if (shriekSound == null)
		{
			if (shriekCooldown > 0)
			{
				shriekCooldown--;
			}
		}
		else if (shriekSound.slatedForDeletetion)
		{
			shriekSound = null;
		}
	}

	public void Shriek()
	{
		if (shriekSound == null && shriekCooldown <= 0)
		{
			shriekSound = fish.room.PlaySound(WatcherEnums.WatcherSoundID.Angler_Shriek, fish.firstChunk);
			shriekCooldown = 200;
		}
	}

	public override void Reset()
	{
		base.Reset();
		lightPos = fish.firstChunk.pos;
		lastLightPos = lightPos;
		lightVel = Vector2.zero;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[BodyMesh] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: false);
		for (int i = 0; i < fish.bodyChunks.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("Circle20");
			sLeaser.sprites[i].scale = fish.bodyChunks[i].rad * 0.1f;
		}
		sLeaser.sprites[ConeMesh] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: false);
		sLeaser.sprites[JawSprite] = new FSprite("Circle20");
		sLeaser.sprites[JawSprite].scaleX = 0.5f;
		sLeaser.sprites[JawSprite].scaleY = 2.25f;
		sLeaser.sprites[JawSprite].anchorY = 0f;
		sLeaser.sprites[HeadSprite] = new FSprite("Circle20");
		sLeaser.sprites[HeadSprite].scaleX = 0.5f;
		sLeaser.sprites[HeadSprite].scaleY = 3f;
		sLeaser.sprites[HeadSprite].anchorY = 0f;
		sLeaser.sprites[CheekSprite] = new FSprite("Circle20");
		int num = 6;
		sLeaser.sprites[RudderMesh] = TriangleMesh.MakeLongMesh(num, pointyTip: false, customColor: false);
		Vector2 b = new Vector2(45f, 5f);
		Vector2 vector = new Vector2(-35f, 5f);
		Vector2 vector2 = new Vector2(-7f, -20f);
		Vector2 p = vector + Vector2.up;
		for (int j = 0; j < num * 2; j++)
		{
			float num2 = Mathf.InverseLerp(0f, num * 2 - 1, j);
			Vector2 vector3 = Vector2.Lerp(Vector2.Lerp(vector, vector2, num2), Vector2.Lerp(vector2, b, num2), num2);
			Vector2 vector4 = Custom.PerpendicularVector(Custom.DirVec(p, vector3));
			float num3 = (1f - Mathf.Pow(num2 * 2f - 1f, 2f)) * (2.5f + 4.5f * rudderLength);
			(sLeaser.sprites[RudderMesh] as TriangleMesh).MoveVertice(j * 2, vector3 - vector4 * num3);
			(sLeaser.sprites[RudderMesh] as TriangleMesh).MoveVertice(j * 2 + 1, vector3 + vector4 * num3);
		}
		for (int k = 0; k < spikes.Length; k++)
		{
			sLeaser.sprites[FirstSpikeMesh + k] = MakeSpikeMesh(spikes[k]);
		}
		for (int l = 0; l < bottomTeeth.Length; l++)
		{
			sLeaser.sprites[FirstBottomToothMesh + l] = MakeToothMesh(bottomTeeth[l]);
		}
		for (int m = 0; m < topTeeth.Length; m++)
		{
			sLeaser.sprites[FirstTopToothMesh + m] = MakeToothMesh(topTeeth[m]);
		}
		sLeaser.sprites[EyeSprite] = new FSprite("Circle20");
		sLeaser.sprites[EyeSprite].scale = 0.3f;
		int firstArmSprite = FirstArmSprite;
		for (int n = 0; n < 4; n++)
		{
			sLeaser.sprites[firstArmSprite + n] = new FSprite("pixel");
			sLeaser.sprites[firstArmSprite + n].anchorY = 0f;
		}
		sLeaser.sprites[firstArmSprite].scaleX = 3f;
		sLeaser.sprites[firstArmSprite].scaleY = armLowLength + armLowExtra;
		sLeaser.sprites[firstArmSprite + 1].scaleX = 2f;
		sLeaser.sprites[firstArmSprite + 1].scaleY = armHighLength;
		sLeaser.sprites[firstArmSprite + 2].scaleX = 1f;
		sLeaser.sprites[firstArmSprite + 3].scaleX = 1f;
		sLeaser.sprites[firstArmSprite + 4] = new FSprite("Circle20");
		sLeaser.sprites[firstArmSprite + 4].scaleX = 0.4f;
		sLeaser.sprites[firstArmSprite + 4].scaleY = 0.8f;
		sLeaser.sprites[firstArmSprite + 4].anchorY = 0f;
		sLeaser.sprites[firstArmSprite + 5] = new FSprite("Futile_White");
		sLeaser.sprites[firstArmSprite + 5].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		sLeaser.sprites[firstArmSprite + 6] = new FSprite("Futile_White");
		sLeaser.sprites[firstArmSprite + 6].shader = rCam.game.rainWorld.Shaders["LightSource"];
		for (int num4 = 0; num4 < numberOfProps; num4++)
		{
			sLeaser.sprites[FirstPropSprite + num4] = new FSprite("Circle20");
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public TriangleMesh MakeSpikeMesh(Spike spike)
	{
		int num = 6;
		TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(num, pointyTip: false, customColor: false);
		Vector2 up = Vector2.up;
		Vector2 vector = -Vector2.up * 5f;
		for (int i = 0; i < num * 2; i++)
		{
			Mathf.InverseLerp(0f, (float)num * 2f - 1f, i);
			float rad = spike.rad;
			Vector2 vector2 = Custom.PerpendicularVector(up);
			triangleMesh.MoveVertice(i * 2, vector + vector2 * (rad * (0.8f + UnityEngine.Random.value * 0.2f)));
			triangleMesh.MoveVertice(i * 2 + 1, vector - vector2 * (rad * (0.8f + UnityEngine.Random.value * 0.2f)));
			if (i == 0)
			{
				vector = Vector2.zero;
			}
			vector += up * (spike.length / ((float)num * 2f - 1f));
		}
		return triangleMesh;
	}

	public TriangleMesh MakeToothMesh(Tooth tooth)
	{
		int num = 6;
		TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(num, pointyTip: false, customColor: false);
		Vector2 normalized = new Vector2(1f, tooth.forwardTilt).normalized;
		Vector2 vector = -Vector2.right * 1.5f;
		for (int i = 0; i < num * 2; i++)
		{
			float num2 = Mathf.InverseLerp(0f, (float)num * 2f - 1f, i);
			float num3 = tooth.rad * Mathf.Sin((num2 + 1f) * (float)Math.PI * 0.5f);
			Vector2 vector2 = Custom.PerpendicularVector(normalized);
			triangleMesh.MoveVertice(i * 2, vector + vector2 * (num3 * (0.8f + UnityEngine.Random.value * 0.2f)));
			triangleMesh.MoveVertice(i * 2 + 1, vector - vector2 * (num3 * (0.8f + UnityEngine.Random.value * 0.2f)));
			if (i == 0)
			{
				vector = Vector2.zero;
			}
			normalized = (normalized - vector2 * tooth.curve).normalized;
			vector += normalized * (tooth.length / ((float)num * 2f - 1f));
		}
		return triangleMesh;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (!culled)
		{
			float num = ((fish.flip > 0f) ? 1f : (-1f));
			Vector2 vector = Vector2.Lerp(fish.bodyChunks[0].lastPos, fish.bodyChunks[0].pos, timeStacker) - camPos;
			Vector2 vector2 = Vector2.Lerp(fish.bodyChunks[1].lastPos, fish.bodyChunks[1].pos, timeStacker) - camPos;
			Vector2 vector3 = Vector2.Lerp(fish.bodyChunks[2].lastPos, fish.bodyChunks[2].pos, timeStacker) - camPos;
			Vector2 vector4 = Custom.DirVec(vector2, vector);
			Vector2 vector5 = Custom.DirVec(vector3, vector2);
			Vector2 vector6 = Custom.PerpendicularVector(vector4);
			Vector2 vector7 = Custom.PerpendicularVector(vector5);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[1].x = vector2.x;
			sLeaser.sprites[1].y = vector2.y;
			sLeaser.sprites[2].x = vector3.x;
			sLeaser.sprites[2].y = vector3.y;
			Vector2 vector8 = vector3 + vector5 * 19f;
			float num2 = 42f;
			for (int i = 0; i < 12; i++)
			{
				float f = Mathf.InverseLerp(0f, 11f, i);
				f = Mathf.Pow(f, 2f);
				float num3 = 1f - f;
				float num4 = (1f - Mathf.Pow(f - 1f, 2f)) * (coneRad + 2f);
				(sLeaser.sprites[ConeMesh] as TriangleMesh).MoveVertice(i * 2, vector8 - vector5 * (num2 * num3) - vector7 * num4);
				(sLeaser.sprites[ConeMesh] as TriangleMesh).MoveVertice(i * 2 + 1, vector8 - vector5 * (num2 * num3) + vector7 * num4);
			}
			Vector2 vector9 = vector - vector4 * 2f - vector6 * (25f * fish.flip);
			Vector2 normalized = (vector6 * (fish.flip * (fish.mouthOpenness * 0.3f + 0.7f)) + vector4 * 0.7f).normalized;
			Vector2 vector10 = Custom.PerpendicularVector(normalized);
			Vector2 normalized2 = (-vector6 * (0.4f * fish.flip) + vector4).normalized;
			normalized2 = Vector3.Slerp(normalized, normalized2, 0.2f + fish.mouthOpenness * 0.8f).normalized;
			Vector2 vector11 = Custom.PerpendicularVector(normalized2);
			Vector2 vector12 = new Vector2(0.5f + (1f - Mathf.Abs(fish.flip)) * 1.05f, 2.7f - (1f - Mathf.Abs(fish.flip)) * 0.5f) * 20f;
			sLeaser.sprites[JawSprite].x = vector9.x;
			sLeaser.sprites[JawSprite].y = vector9.y;
			sLeaser.sprites[JawSprite].rotation = Custom.VecToDeg(normalized2);
			sLeaser.sprites[JawSprite].scaleX = vector12.x / 20f;
			sLeaser.sprites[JawSprite].scaleY = vector12.y / 20f;
			Vector2 vector13 = new Vector2(0.5f + (1f - Mathf.Abs(fish.flip)) * 2f, 2.2f + Mathf.Abs(fish.flip) * 0.8f) * 20f;
			sLeaser.sprites[HeadSprite].x = vector9.x;
			sLeaser.sprites[HeadSprite].y = vector9.y;
			sLeaser.sprites[HeadSprite].rotation = Custom.VecToDeg(normalized);
			sLeaser.sprites[HeadSprite].scaleX = vector13.x / 20f;
			sLeaser.sprites[HeadSprite].scaleY = vector13.y / 20f;
			Vector2 vector14 = vector9 + normalized * (vector13.y * 0.3f) - vector4 * 20f;
			Vector2 vector15 = vector9 + normalized2 * (vector12.y * 0.33f);
			Vector2 vector16 = Vector2.Lerp(vector14, vector15, 0.5f);
			sLeaser.sprites[CheekSprite].x = vector16.x;
			sLeaser.sprites[CheekSprite].y = vector16.y;
			sLeaser.sprites[CheekSprite].scaleX = 1.1f - 0.4f * fish.mouthOpenness;
			sLeaser.sprites[CheekSprite].scaleY = Vector2.Distance(vector14, vector15) / 20f;
			sLeaser.sprites[CheekSprite].rotation = Custom.VecToDeg(Custom.DirVec(vector14, vector15));
			Vector2 vector17 = vector8 - vector5 * (num2 + 20f);
			sLeaser.sprites[RudderMesh].x = vector17.x;
			sLeaser.sprites[RudderMesh].y = vector17.y;
			sLeaser.sprites[RudderMesh].rotation = Custom.VecToDeg((vector5 - vector7 * (3f * fish.rudderTurnWay * Mathf.Pow(Mathf.Max(1f - Mathf.Abs(fish.flip) - 0.3f, 0f) / 0.7f, 3f))).normalized);
			float num5 = Mathf.Max(Mathf.Abs(fish.flip), 0.15f * Mathf.Abs(fish.rudderTurnWay)) * ((fish.flip > 0f) ? 1f : (-1f));
			sLeaser.sprites[RudderMesh].scaleX = Mathf.Sin(Mathf.Abs(num5) * ((num5 > 0f) ? (-1f) : 1f) * (float)Math.PI * 0.5f);
			for (int j = 0; j < spikes.Length; j++)
			{
				float num6 = (spikes[j].topSide ? 1f : (-1f)) * Mathf.Abs(fish.flip);
				Vector2 vector18 = vector8 - vector5 * (num2 * spikes[j].placement) + vector7 * (Mathf.Pow(spikes[j].placement - 1f, 2f) * 20f * num6);
				sLeaser.sprites[FirstSpikeMesh + j].x = vector18.x;
				sLeaser.sprites[FirstSpikeMesh + j].y = vector18.y;
				sLeaser.sprites[FirstSpikeMesh + j].rotation = Custom.VecToDeg((-vector5 + vector7 * ((1f - spikes[j].placement) * num6)).normalized);
			}
			for (int k = 0; k < bottomTeeth.Length; k++)
			{
				float num7 = (float)k / ((float)bottomTeeth.Length - 1f);
				num7 = num7 * 0.85f * 0.93f + 0.15f;
				Vector2 vector19 = Vector2.Lerp(vector9, vector9 + normalized2 * vector12.y, num7) + vector11 * (vector12.x * Mathf.Sin(num7 * (float)Math.PI) * 0.5f * num);
				sLeaser.sprites[FirstBottomToothMesh + k].x = vector19.x;
				sLeaser.sprites[FirstBottomToothMesh + k].y = vector19.y;
				sLeaser.sprites[FirstBottomToothMesh + k].rotation = Custom.VecToDeg(normalized2 - vector11 * (fish.mouthOpenness * 0.5f * num));
				sLeaser.sprites[FirstBottomToothMesh + k].scaleX = (0f - (1f - Mathf.Pow(1f - Mathf.Abs(fish.flip), 3f))) * num;
			}
			for (int l = 0; l < topTeeth.Length; l++)
			{
				float num8 = (float)l / ((float)topTeeth.Length - 1f);
				num8 = num8 * 0.8f * 0.935f + 0.2f;
				Vector2 vector20 = Vector2.Lerp(vector9, vector9 + normalized * vector13.y, num8) + vector10 * (vector13.x * Mathf.Lerp(Mathf.Sin(num8 * (float)Math.PI) * 0.5f, 0.5f, (num8 < 0.5f) ? (1f - Mathf.Abs(fish.flip)) : 0f) * ((fish.flip > 0f) ? (-1f) : 1f));
				sLeaser.sprites[FirstTopToothMesh + l].x = vector20.x;
				sLeaser.sprites[FirstTopToothMesh + l].y = vector20.y;
				sLeaser.sprites[FirstTopToothMesh + l].rotation = Custom.VecToDeg(normalized + vector10 * (fish.mouthOpenness * 0.5f * num));
				sLeaser.sprites[FirstTopToothMesh + l].scaleX = (1f - Mathf.Pow(1f - Mathf.Abs(fish.flip), 3f)) * num;
			}
			Vector2 vector21 = vector9 + normalized * (vector13.y * 0.85f) - vector4 * 3f + Custom.PerpendicularVector(normalized) * (20f * (1f - Mathf.Abs(fish.flip)) * ((fish.flip > 0f) ? (-1f) : 1f));
			sLeaser.sprites[EyeSprite].x = vector21.x;
			sLeaser.sprites[EyeSprite].y = vector21.y;
			sLeaser.sprites[EyeSprite].rotation = Custom.VecToDeg(normalized);
			sLeaser.sprites[EyeSprite].scaleX = (0.125f + 0.175f * Mathf.Abs(fish.flip)) * eyeSize;
			sLeaser.sprites[EyeSprite].scaleY = 0.3f * eyeSize;
			sLeaser.sprites[EyeSprite].color = Color.Lerp(blackColor, eyeColor, 0.4f);
			if (lightsOn)
			{
				sLeaser.sprites[EyeSprite].color = (rCam.room.PointSubmerged(vector21 + camPos + new Vector2(0f, 5f)) ? new Color(0f, 0.003921569f, 0f) : eyeColor);
			}
			Vector2 vector22 = Vector2.Lerp(vector9 + normalized * vector13.y, vector8 + vector7 * (coneRad * fish.flip), 0.3f);
			Vector2 vector23 = Vector2.Lerp(lastLightPos, lightPos, timeStacker) - camPos;
			Vector2 vector24 = Custom.InverseKinematic(vector22, vector23, armLowLength, armHighLength, 0f - fish.flip);
			Vector2 vector25 = Custom.DirVec(vector22, vector24);
			Vector2 vector26 = Custom.DirVec(vector24, vector23);
			vector23 = vector24 + vector26 * armHighLength;
			int firstArmSprite = FirstArmSprite;
			sLeaser.sprites[firstArmSprite].x = vector22.x;
			sLeaser.sprites[firstArmSprite].y = vector22.y;
			sLeaser.sprites[firstArmSprite].rotation = Custom.VecToDeg(vector25);
			sLeaser.sprites[firstArmSprite + 1].x = vector24.x;
			sLeaser.sprites[firstArmSprite + 1].y = vector24.y;
			sLeaser.sprites[firstArmSprite + 1].rotation = Custom.VecToDeg(vector26);
			Vector2 vector27 = vector22 - vector4 * 20f;
			Vector2 vector28 = vector24 + vector25 * armLowExtra;
			sLeaser.sprites[firstArmSprite + 2].x = vector27.x;
			sLeaser.sprites[firstArmSprite + 2].y = vector27.y;
			sLeaser.sprites[firstArmSprite + 2].scaleY = Vector2.Distance(vector27, vector28);
			sLeaser.sprites[firstArmSprite + 2].rotation = Custom.VecToDeg(Custom.DirVec(vector27, vector28));
			Vector2 vector29 = vector23 + vector26 * 7f;
			Vector2 vector30 = vector24 + vector26 * armHighLength;
			sLeaser.sprites[firstArmSprite + 3].x = vector28.x;
			sLeaser.sprites[firstArmSprite + 3].y = vector28.y;
			sLeaser.sprites[firstArmSprite + 3].scaleY = Vector2.Distance(vector28, vector30);
			sLeaser.sprites[firstArmSprite + 3].rotation = Custom.VecToDeg(Custom.DirVec(vector28, vector30));
			sLeaser.sprites[firstArmSprite + 4].x = vector23.x;
			sLeaser.sprites[firstArmSprite + 4].y = vector23.y;
			sLeaser.sprites[firstArmSprite + 4].rotation = Custom.VecToDeg(vector26);
			Color color = lightColor;
			sLeaser.sprites[firstArmSprite + 4].color = Color.Lerp(blackColor, lightColor, 0.05f + Mathf.Lerp(flicker[0, 1], flicker[0, 0], timeStacker) * 0.95f);
			if (lightsOn)
			{
				sLeaser.sprites[firstArmSprite + 4].color = (rCam.room.PointSubmerged(vector29 + camPos + new Vector2(0f, 4f)) ? new Color(0f, 0.003921569f, 0f) : Color.Lerp(color, Color.white, 0.95f));
			}
			sLeaser.sprites[firstArmSprite + 5].x = vector29.x;
			sLeaser.sprites[firstArmSprite + 5].y = vector29.y;
			sLeaser.sprites[firstArmSprite + 5].scale = Mathf.Lerp(flicker[0, 1], flicker[0, 0], timeStacker) * 2.5f;
			sLeaser.sprites[firstArmSprite + 6].x = vector29.x;
			sLeaser.sprites[firstArmSprite + 6].y = vector29.y;
			sLeaser.sprites[firstArmSprite + 6].scale = Mathf.Lerp(flicker[1, 1], flicker[1, 0], timeStacker) * 250f / 8f;
			sLeaser.sprites[FirstArmSprite + 5].color = color;
			sLeaser.sprites[FirstArmSprite + 6].color = color;
			Vector2 vector31 = Vector2.Lerp(vector2, vector, 0.5f) + vector6 * 30f;
			Vector2 vector32 = Vector2.Lerp(vector2, vector, 0.5f) - vector6 * 30f;
			float num9 = Custom.VecToDeg(normalized);
			Vector2 vec = Custom.RotateAroundOrigo(vector6, 0f - num9);
			vec *= new Vector2(vector13.x, vector13.y) * 0.5f;
			vec = Custom.RotateAroundOrigo(vec, num9);
			Vector2 b = vector9 + normalized * (vector13.y * 0.5f) - (Vector2)Vector3.Slerp(vec, normalized * (((fish.flip > 0f) ? 1f : (-1f)) * vector13.y * 0.5f), Mathf.Pow(fish.flip, 2f));
			Vector2 b2 = vector9 + normalized * (vector13.y * 0.5f) + (Vector2)Vector3.Slerp(vec, normalized * (((fish.flip > 0f) ? 1f : (-1f)) * vector13.y * 0.5f), Mathf.Pow(fish.flip, 2f));
			for (int m = 0; m < 12; m++)
			{
				float t = 1f - Mathf.InverseLerp(0f, 11f, m);
				Vector2 pos = Vector2.Lerp(Vector2.Lerp(vector8 + vector7 * coneRad, vector31, t), Vector2.Lerp(vector31, b2, t), t);
				Vector2 pos2 = Vector2.Lerp(Vector2.Lerp(vector8 - vector7 * coneRad, vector32, t), Vector2.Lerp(vector32, b, t), t);
				(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(m * 2, pos);
				(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(m * 2 + 1, pos2);
			}
			for (int n = 0; n < numberOfProps; n++)
			{
				float num10 = Mathf.Sin(propSpin + (float)Math.PI * 2f * (float)n / (float)numberOfProps);
				float num11 = Mathf.Cos(propSpin + (float)Math.PI * 2f * (float)n / (float)numberOfProps);
				Vector2 vector33 = vector8 + vector7 * num10 * coneRad;
				sLeaser.sprites[FirstPropSprite + n].x = vector33.x;
				sLeaser.sprites[FirstPropSprite + n].y = vector33.y;
				sLeaser.sprites[FirstPropSprite + n].rotation = Custom.VecToDeg(vector5);
				sLeaser.sprites[FirstPropSprite + n].anchorX = (1f + num10) * 0.5f;
				float num12 = 0.7f + (1f + num11) * 0.5f * 0.3f;
				sLeaser.sprites[FirstPropSprite + n].scaleX = Mathf.Abs(num10) * 2f * num12;
				sLeaser.sprites[FirstPropSprite + n].scaleY = num12;
			}
			sLeaser.sprites[0].isVisible = false;
			sLeaser.sprites[1].isVisible = false;
			sLeaser.sprites[2].isVisible = false;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		Color color = blackColor;
		for (int i = 0; i < TotalSprites; i++)
		{
			sLeaser.sprites[i].color = blackColor;
		}
		sLeaser.sprites[BodyMesh].color = color;
		sLeaser.sprites[HeadSprite].color = color;
		sLeaser.sprites[JawSprite].color = color;
		sLeaser.sprites[CheekSprite].color = color;
		for (int j = 0; j < bottomTeeth.Length; j++)
		{
			sLeaser.sprites[FirstBottomToothMesh + j].color = color;
		}
		for (int k = 0; k < topTeeth.Length; k++)
		{
			sLeaser.sprites[FirstTopToothMesh + k].color = color;
		}
		sLeaser.sprites[FirstArmSprite + 2].color = color;
		sLeaser.sprites[FirstArmSprite + 3].color = color;
		sLeaser.sprites[0].color = Color.red;
		sLeaser.sprites[1].color = Color.green;
		sLeaser.sprites[2].color = Color.blue;
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (i == FirstArmSprite + 5 || i == FirstArmSprite + 6)
			{
				rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
		if (sLeaser.containers != null)
		{
			FContainer[] containers = sLeaser.containers;
			foreach (FContainer node in containers)
			{
				newContatiner.AddChild(node);
			}
		}
	}
}
