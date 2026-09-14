using System;
using RWCustom;
using UnityEngine;
using Watcher;

public class RotWorm : BackgroundScene.BackgroundSceneElement
{
	public struct Chunk
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float connectionRad;

		public float visualRad;

		public Chunk(Vector2 pos, float connectionRad, float visualRad)
		{
			this.pos = pos;
			lastPos = pos;
			vel = Vector2.zero;
			this.connectionRad = connectionRad;
			this.visualRad = visualRad;
		}
	}

	public class Bulb
	{
		public RotWorm owner;

		public int bodySprite;

		public int firstEyeSprite;

		public Vector2 anchor;

		public Vector2 lastPos;

		public Vector2 pos;

		public Vector2 lastOriginPos;

		public Vector2 originPos;

		public float rad;

		public float rotation;

		public float eyeRad;

		public bool hasEye = true;

		public float focus;

		public float lastFocus;

		public float getToFocus;

		public float closed;

		public float lastClosed;

		public float light;

		public int reactionDelay;

		public int bubblesWait;

		public Vector2 lookDir;

		public Vector2 lastLookDir;

		public Vector2 nextLookDir;

		public bool hasHeardSound;

		public Color eyeColor = new Color(0f, 0f, 1f);

		public Color bodyColor = new Color(0.003921569f, 0.003921569f, 0.003921569f);

		public Bulb(RotWorm owner, Vector2 anchor, Vector2 pos, Vector2 originPos, float rad, bool hasEye)
		{
			this.owner = owner;
			this.anchor = anchor;
			this.pos = pos;
			this.rad = rad;
			this.hasEye = hasEye;
			this.originPos = originPos;
			lastPos = pos;
			lastOriginPos = originPos;
			nextLookDir = Custom.RNV();
			rotation = UnityEngine.Random.value * 360f;
			eyeRad = UnityEngine.Random.value;
		}

		public void Update()
		{
			lastPos = pos;
			lastOriginPos = originPos;
			lastClosed = closed;
			if (bubblesWait > 0)
			{
				bubblesWait--;
			}
			closed = Mathf.Max(0f, closed - 0.05f);
			float num = light * Mathf.InverseLerp(0f, 1f, Vector2.Distance(lastLookDir, lookDir));
			light = Mathf.Max(0f, light - 0.05f);
			if (UnityEngine.Random.value < num)
			{
				getToFocus = Mathf.Max(getToFocus, UnityEngine.Random.value);
			}
			else if (UnityEngine.Random.value < 1f / 70f)
			{
				getToFocus = 0f;
			}
			lastFocus = focus;
			if (focus < getToFocus)
			{
				focus = Mathf.Min(focus + 0.05f, getToFocus);
			}
			else
			{
				focus = Mathf.Max(focus - 0.05f, getToFocus);
			}
			lastLookDir = lookDir;
			if (reactionDelay < 1)
			{
				lookDir = nextLookDir;
				if (hasHeardSound)
				{
					light = Mathf.Max(light, UnityEngine.Random.value);
				}
				hasHeardSound = false;
				reactionDelay = UnityEngine.Random.Range(10, 20);
			}
			else
			{
				reactionDelay--;
			}
			if (UnityEngine.Random.value < 0.00125f)
			{
				nextLookDir = Custom.RNV();
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			if (rCam.room.world.region != null)
			{
				eyeColor = rCam.room.world.region.regionParams.corruptionEyeColor;
			}
			sLeaser.sprites[bodySprite] = new FSprite("Futile_White");
			sLeaser.sprites[bodySprite].scale = rad / 8f;
			sLeaser.sprites[bodySprite].shader = rCam.room.game.rainWorld.Shaders["BackgroundJaggedCircle"];
			sLeaser.sprites[bodySprite].alpha = 0.6f;
			if (hasEye)
			{
				for (int i = 0; i < 2; i++)
				{
					sLeaser.sprites[firstEyeSprite + i] = MakeSlitMesh();
					sLeaser.sprites[firstEyeSprite + i].shader = rCam.game.rainWorld.Shaders["BackgroundNoHoles"];
				}
			}
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 p = Vector2.Lerp(lastPos, pos, timeStacker);
			Vector2 vector = Vector2.Lerp(lastOriginPos, originPos, timeStacker);
			float num = Custom.AimFromOneVectorToAnother(vector, p) + rotation;
			sLeaser.sprites[bodySprite].x = p.x - camPos.x;
			sLeaser.sprites[bodySprite].y = p.y - camPos.y;
			sLeaser.sprites[bodySprite].rotation = num;
			if (hasEye)
			{
				RenderSlits(p, vector, num, sLeaser, rCam, timeStacker, camPos);
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[bodySprite].color = bodyColor;
		}

		public void RenderSlits(Vector2 pos, Vector2 middleOfBody, float rotation, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = Mathf.Lerp(rad * 0.5f, rad, eyeRad);
			float num2 = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastClosed, closed, timeStacker)), 0.6f);
			float num3 = 1f * (1f - num2);
			Vector2 b = Vector2.Lerp(lastLookDir, lookDir, timeStacker);
			float a = Mathf.Lerp(lastFocus, focus, timeStacker) * Mathf.Pow(Mathf.InverseLerp(-1f, 1f, Vector2.Dot(Custom.DirVec(middleOfBody, pos), lookDir.normalized)), 0.7f);
			a = Mathf.Max(a, num2);
			float a2 = Mathf.InverseLerp(0f, Mathf.Lerp(30f, 50f, 1f), Vector2.Distance(middleOfBody, pos + Custom.DirVec(middleOfBody, pos) * rad)) * 0.9f;
			a2 = Mathf.Lerp(a2, 1f, 0.5f * a);
			Vector2 dir = Vector2.Lerp(Custom.DirVec(middleOfBody, pos) * a2, b, b.magnitude * 0.5f);
			Color color = Color.Lerp(eyeColor, new Color(1f, 1f, 1f), Mathf.Lerp(UnityEngine.Random.value * light, 1f, num2));
			sLeaser.sprites[firstEyeSprite].color = color;
			sLeaser.sprites[firstEyeSprite + 1].color = color;
			for (int i = 0; i < 2; i++)
			{
				Vector2 vector = Custom.DegToVec(rotation + 90f * (float)i);
				Vector2 vector2 = Custom.PerpendicularVector(vector);
				(sLeaser.sprites[firstEyeSprite + i] as TriangleMesh).MoveVertice(0, pos + BulgeVertex(vector * (num * 0.9f * Mathf.Lerp(1f, 0.6f, a)), dir, rad) - camPos);
				(sLeaser.sprites[firstEyeSprite + i] as TriangleMesh).MoveVertice(9, pos + BulgeVertex(vector * ((0f - num) * 0.9f * Mathf.Lerp(1f, 0.6f, a)), dir, rad) - camPos);
				for (int j = 1; j < 5; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						float num4 = num * ((j < 3) ? 0.7f : 0.25f) * ((k == 0) ? 1f : (-1f)) * Mathf.Lerp(1f, 0.6f, a);
						int num5 = ((k == 0) ? j : (9 - j));
						float num6 = num3 * ((j < 3) ? 0.5f : 1f) * ((num5 % 2 == 0) ? 1f : (-1f)) * Mathf.Lerp(1f, 2.5f, a);
						(sLeaser.sprites[firstEyeSprite + i] as TriangleMesh).MoveVertice(num5, pos + BulgeVertex(vector * num4 + vector2 * num6, dir, rad) - camPos);
					}
				}
			}
		}

		public Vector2 BulgeVertex(Vector2 v, Vector2 dir, float rad)
		{
			return Vector2.Lerp(v, Vector2.ClampMagnitude(v + dir * rad, rad), dir.magnitude);
		}

		public TriangleMesh MakeSlitMesh()
		{
			TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[8];
			for (int i = 0; i < 8; i++)
			{
				array[i] = new TriangleMesh.Triangle(i, i + 1, i + 2);
			}
			return new TriangleMesh("Futile_White", array, customColor: false);
		}
	}

	public class Arm
	{
		public struct Bump
		{
			public Vector2 pos;

			public float size;

			public float eyeSize;

			public Bump(Vector2 pos, float size, float eyeSize)
			{
				this.pos = pos;
				this.size = size;
				this.eyeSize = eyeSize;
			}
		}

		public Vector2 anchor;

		public Vector2 dir;

		public int firstSprite;

		public int totalSprites;

		public Chunk[] chunks;

		public Bump[] bumps;

		public Color rootColor;

		public float stiffnessFac;

		public Arm(Vector2 anchor, Vector2 dir, Vector2 pos, Color rootColor, float lengthFac, float stiffnessFac)
		{
			this.anchor = anchor;
			this.dir = dir;
			this.rootColor = rootColor;
			this.stiffnessFac = stiffnessFac;
			chunks = new Chunk[(int)(20f + 50f * lengthFac)];
			for (int i = 0; i < chunks.Length; i++)
			{
				float num = (float)i / ((float)chunks.Length - 1f);
				chunks[i] = new Chunk(pos, 20f + 20f * num, Mathf.Lerp(20f, 4f, Mathf.Pow(num * 1.5f, 0.5f)));
			}
			bumps = new Bump[5 + (int)((float)UnityEngine.Random.Range(20, 30) * lengthFac)];
			totalSprites = 1;
			for (int j = 0; j < bumps.Length; j++)
			{
				float num2 = Mathf.Pow(UnityEngine.Random.value, 0.5f);
				if (j == 0)
				{
					num2 = 1f;
				}
				bumps[j] = new Bump(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3f * num2, Mathf.Lerp(Mathf.InverseLerp(0f, chunks.Length, chunks.Length - 20), 1f, num2)), Mathf.Lerp(UnityEngine.Random.value, num2, UnityEngine.Random.value), (UnityEngine.Random.value < Mathf.Lerp(0f, 0.6f, num2)) ? Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, num2))) : 0f);
				totalSprites++;
				if (bumps[j].eyeSize > 0f)
				{
					totalSprites++;
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMeshAtlased(chunks.Length, pointyTip: false, customColor: true);
			sLeaser.sprites[firstSprite].shader = rCam.game.rainWorld.Shaders["BackgroundNoHoles"];
			int num = 0;
			for (int i = 0; i < bumps.Length; i++)
			{
				sLeaser.sprites[firstSprite + 1 + i] = new FSprite("Circle20", quadType: false);
				sLeaser.sprites[firstSprite + 1 + i].scale = Mathf.Lerp(2f, 6f, bumps[i].size) / 10f;
				sLeaser.sprites[firstSprite + 1 + i].shader = rCam.game.rainWorld.Shaders["BackgroundNoHoles"];
				if (bumps[i].eyeSize > 0f)
				{
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num] = new FSprite("Circle20", quadType: false);
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num].scale = Mathf.Lerp(2f, 6f, bumps[i].size) * bumps[i].eyeSize / 10f;
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num].shader = rCam.game.rainWorld.Shaders["BackgroundNoHoles"];
					num++;
				}
			}
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		public void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(chunks[0].lastPos, chunks[0].pos, timeStacker);
			vector += Custom.DirVec(Vector2.Lerp(chunks[1].lastPos, chunks[1].pos, timeStacker), vector) * 1f;
			float num = 0f;
			for (int i = 0; i < chunks.Length; i++)
			{
				Vector2 vector2 = Vector2.Lerp(chunks[i].lastPos, chunks[i].pos, timeStacker);
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				float visualRad = chunks[i].visualRad;
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * visualRad - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * visualRad - camPos);
				vector = vector2;
				num = visualRad;
			}
			int num2 = 0;
			for (int j = 0; j < bumps.Length; j++)
			{
				Vector2 vector4 = OnTubePos(bumps[j].pos, timeStacker);
				sLeaser.sprites[firstSprite + 1 + j].x = vector4.x - camPos.x;
				sLeaser.sprites[firstSprite + 1 + j].y = vector4.y - camPos.y;
				if (bumps[j].eyeSize > 0f)
				{
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num2].x = vector4.x - camPos.x;
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num2].y = vector4.y - camPos.y;
					num2++;
				}
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color b = new Color(0.003921569f, 0.003921569f, 0.003921569f);
			Color color = new Color(0f, 0f, 0.5f);
			if (rCam.room != null && rCam.room.world.region != null)
			{
				color = rCam.room.world.region.regionParams.corruptionEffectColor;
			}
			for (int i = 0; i < (sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length; i++)
			{
				float num = (float)i / (float)((sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length - 1);
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(Color.Lerp(rootColor, b, num * 10f), color, OnTubeEffectColorFac(num));
			}
			int num2 = 0;
			for (int j = 0; j < bumps.Length; j++)
			{
				sLeaser.sprites[firstSprite + 1 + j].color = Color.Lerp(Color.Lerp(rootColor, b, bumps[j].pos.y * 10f), color, OnTubeEffectColorFac(bumps[j].pos.y));
				if (bumps[j].eyeSize > 0f)
				{
					sLeaser.sprites[firstSprite + 1 + bumps.Length + num2].color = color;
					num2++;
				}
			}
		}

		public float OnTubeEffectColorFac(float floatPos)
		{
			return Mathf.Pow(floatPos, 1.5f) * 0.4f;
		}

		public Vector2 OnTubePos(Vector2 pos, float timeStacker)
		{
			Vector2 p = OneDimensionalTubePos(pos.y - 1f / (float)(chunks.Length - 1), timeStacker);
			Vector2 p2 = OneDimensionalTubePos(pos.y + 1f / (float)(chunks.Length - 1), timeStacker);
			return OneDimensionalTubePos(pos.y, timeStacker) + Custom.PerpendicularVector(Custom.DirVec(p, p2)) * pos.x;
		}

		public Vector2 OneDimensionalTubePos(float floatPos, float timeStacker)
		{
			int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (float)(chunks.Length - 1)), 0, chunks.Length - 1);
			int num2 = Custom.IntClamp(num + 1, 0, chunks.Length - 1);
			float t = Mathf.InverseLerp(num, num2, floatPos * (float)(chunks.Length - 1));
			return Vector2.Lerp(Vector2.Lerp(chunks[num].lastPos, chunks[num2].lastPos, t), Vector2.Lerp(chunks[num].pos, chunks[num2].pos, t), timeStacker);
		}
	}

	public class Fin
	{
		public Chunk[] chunks;

		public float attachment;

		public int side;

		public Fin(Vector2 pos, float attachment, int side, float lengthFactor)
		{
			this.attachment = attachment;
			this.side = side;
			chunks = new Chunk[(int)(16f * lengthFactor)];
			for (int i = 0; i < chunks.Length; i++)
			{
				chunks[i] = new Chunk(pos, 50f, Custom.LerpMap(i, 0f, chunks.Length - 1, 50f, 2f));
			}
		}
	}

	public class RotWormVolumeConfig
	{
		public string roomName;

		public int cameraNumber;

		public float volumeMultiplier;

		public RotWormVolumeConfig(string roomName, int cameraNumber, float volumeMultiplier)
		{
			this.roomName = roomName;
			this.cameraNumber = cameraNumber;
			this.volumeMultiplier = volumeMultiplier;
		}
	}

	public Chunk[] chunks;

	public Fin[] fins;

	public Bulb[] bulbs;

	public Arm[] arms;

	public int bodySprite;

	public int firstFinSprite;

	public int totalSprites;

	public int mainBodyChunks = 30;

	public float wormLength;

	public Color skinDark = new Color(0.003921569f, 0.003921569f, 0.003921569f);

	public Color skinLight = new Color(0.06f, 0.055f, 0.06f);

	public StaticSoundLoop swimLoop;

	public DisembodiedDynamicSoundLoop glowLoop;

	public RotWormVolumeConfig[] volumeConfigs;

	public float currentVolumeMultiplier;

	public int testAnimation = 200;

	public RotWorm(BackgroundScene scene, Vector2 pos, float depth)
		: base(scene, pos, depth)
	{
		chunks = new Chunk[50];
		Vector2 vector = new Vector2(0f, 0f);
		for (int i = 0; i < chunks.Length; i++)
		{
			float num;
			float visualRad;
			if (i < mainBodyChunks)
			{
				num = Custom.LerpMap(i, 0f, mainBodyChunks - 1, 200f, 50f, 2f);
				visualRad = num;
			}
			else
			{
				num = Custom.LerpMap(i, mainBodyChunks, chunks.Length - 1, 50f, 50f);
				visualRad = Custom.LerpMap(i, mainBodyChunks, chunks.Length - 1, 50f, 10f, 0.75f);
			}
			chunks[i] = new Chunk(vector + new Vector2(num, 0f), num, visualRad);
			vector.x += num * 2f;
		}
		wormLength = 0f;
		for (int j = 1; j < chunks.Length; j++)
		{
			wormLength += chunks[j - 1].connectionRad + chunks[j].connectionRad;
		}
		fins = new Fin[16];
		for (int k = 0; k < fins.Length; k++)
		{
			fins[k] = new Fin(pos, 1f + (float)(k / 2) * 2f, k % 2 * 2 - 1, 1f - 0.5f * (float)k / (float)fins.Length);
		}
		bulbs = new Bulb[60];
		Vector2 vector2 = default(Vector2);
		for (int l = 0; l < bulbs.Length; l++)
		{
			float num3;
			bool flag;
			float num4;
			if (l < 10)
			{
				float num2 = (float)(l / 2) / 9f;
				if (l % 2 == 0)
				{
					num2 = 1f - num2;
				}
				num3 = UnityEngine.Random.Range(50f, 70f);
				vector2.x = Mathf.Lerp(0f - chunks[0].visualRad + num3, chunks[0].visualRad - num3, num2);
				vector2.y = 0f;
				vector2 += Custom.RNV() * (UnityEngine.Random.value * 20f * num2 * (1f - num2));
				flag = false;
				num4 = 1f - Mathf.Abs(num2 * 2f - 1f);
			}
			else
			{
				float num5 = UnityEngine.Random.value * (1f - (float)l / ((float)bulbs.Length - 1f) * 0.66f);
				num3 = Mathf.Lerp(10f, 60f, num5);
				vector2 = new Vector2(UnityEngine.Random.Range(0f - chunks[0].visualRad + num3 - 10f, chunks[0].visualRad - num3 + 10f), Mathf.Lerp(25f, -250f, Mathf.Pow(UnityEngine.Random.value, 2f) * (1f - num5)));
				flag = UnityEngine.Random.value < 0.5f && num3 < 40f;
				num4 = 1f - Mathf.Abs(vector2.x / chunks[0].visualRad);
				if (flag)
				{
					for (int m = 0; m < l; m++)
					{
						if (bulbs[m].hasEye && Vector2.Distance(bulbs[m].anchor, vector2) < (num3 + bulbs[m].rad) * 0.7f)
						{
							flag = false;
						}
					}
				}
			}
			bulbs[l] = new Bulb(this, vector2, pos, chunks[0].pos, num3, flag);
			bulbs[l].bodyColor = Color.Lerp(skinDark, skinLight, num4 * 0.9f);
		}
		arms = new Arm[20];
		for (int n = 0; n < arms.Length; n++)
		{
			bool flag2 = n < arms.Length / 2;
			float num6 = (float)(n % (arms.Length / 2)) / ((float)(arms.Length / 2) - 1f);
			arms[n] = new Arm(new Vector2(Mathf.Lerp(0f - chunks[0].visualRad + 35f, chunks[0].visualRad - 35f, num6), 0f) + new Vector2(UnityEngine.Random.value * 40f * num6 * (1f - num6), 0f), new Vector2((num6 * 2f - 1f) * 0.15f + UnityEngine.Random.Range(-0.05f, 0.05f), 1f).normalized, pos, flag2 ? skinDark : Color.Lerp(skinLight, skinDark, Mathf.Pow(Mathf.Abs(num6 * 2f - 1f), 2f)), Mathf.Lerp(Mathf.Sqrt(UnityEngine.Random.value), UnityEngine.Random.value * 0.5f, Mathf.Abs(num6 * 2f - 1f)), UnityEngine.Random.value);
		}
		int num7 = 0;
		for (int num8 = 0; num8 < arms.Length / 2; num8++)
		{
			arms[num8].firstSprite = num7;
			num7 += arms[num8].totalSprites;
		}
		firstFinSprite = num7;
		num7 = (bodySprite = num7 + fins.Length) + 1;
		for (int num9 = arms.Length / 2; num9 < arms.Length; num9++)
		{
			arms[num9].firstSprite = num7;
			num7 += arms[num9].totalSprites;
		}
		Bulb[] array = bulbs;
		for (int num10 = 0; num10 < array.Length; num10++)
		{
			array[num10].bodySprite = num7;
			num7++;
		}
		array = bulbs;
		foreach (Bulb bulb in array)
		{
			if (bulb.hasEye)
			{
				bulb.firstEyeSprite = num7;
				num7 += 2;
			}
		}
		totalSprites = num7;
		volumeConfigs = new RotWormVolumeConfig[14]
		{
			new RotWormVolumeConfig("wssr_abyss", 0, 0.3f),
			new RotWormVolumeConfig("wssr_abyss", 1, 0.55f),
			new RotWormVolumeConfig("wssr_abyss", 2, 0.75f),
			new RotWormVolumeConfig("wssr_abyss", 3, 1f),
			new RotWormVolumeConfig("wssr_abyss", 4, 1f),
			new RotWormVolumeConfig("wssr_abyss", 5, 1f),
			new RotWormVolumeConfig("wssr_abyss", 6, 0.6f),
			new RotWormVolumeConfig("wssr_abyss", 7, 0.45f),
			new RotWormVolumeConfig("wssr_labbig", 0, 0.3f),
			new RotWormVolumeConfig("wssr_labbig", 3, 0.3f),
			new RotWormVolumeConfig("wssr_lab6", 0, 0.3f),
			new RotWormVolumeConfig("wssr_portholes", 0, 0.3f),
			new RotWormVolumeConfig("wssr_portholes", 1, 0.15f),
			new RotWormVolumeConfig("wssr_portholes", 2, 0.3f)
		};
		scene.LoadGraphic("rotskin", crispPixels: false, clampWrapMode: false);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[bodySprite] = TriangleMesh.MakeLongMesh(chunks.Length, pointyTip: false, customColor: false, "rotskin");
		sLeaser.sprites[bodySprite].shader = rCam.game.rainWorld.Shaders["RotWormBody"];
		for (int i = 0; i < fins.Length; i++)
		{
			sLeaser.sprites[firstFinSprite + i] = TriangleMesh.MakeLongMesh(fins[i].chunks.Length, pointyTip: false, customColor: false, "rotskin");
			sLeaser.sprites[firstFinSprite + i].shader = rCam.game.rainWorld.Shaders["RotWormFin"];
		}
		Bulb[] array = bulbs;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].InitiateSprites(sLeaser, rCam);
		}
		Arm[] array2 = arms;
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].InitiateSprites(sLeaser, rCam);
		}
		base.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Bloom");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node in sprites)
		{
			newContatiner.AddChild(node);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Bulb[] array = bulbs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ApplyPalette(sLeaser, rCam, palette);
		}
		Arm[] array2 = arms;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].ApplyPalette(sLeaser, rCam, palette);
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		DrawLongMesh(chunks, (TriangleMesh)sLeaser.sprites[bodySprite], timeStacker, camPos, flip: false);
		for (int i = 0; i < fins.Length; i++)
		{
			DrawLongMesh(fins[i].chunks, (TriangleMesh)sLeaser.sprites[firstFinSprite + i], timeStacker, camPos, fins[i].side == -1);
		}
		Bulb[] array = bulbs;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		Arm[] array2 = arms;
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public static void DrawLongMesh(Chunk[] chunks, TriangleMesh mesh, float timeStacker, Vector2 camPos, bool flip)
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			int num = i * 4;
			Vector2 vector = Vector2.Lerp(chunks[Mathf.Max(i - 1, 0)].lastPos, chunks[Mathf.Max(i - 1, 0)].pos, timeStacker) - camPos;
			Vector2 vector2 = Vector2.Lerp(chunks[Mathf.Min(i + 1, chunks.Length - 1)].lastPos, chunks[Mathf.Min(i + 1, chunks.Length - 1)].pos, timeStacker) - camPos;
			Vector2 vector3 = Vector2.Lerp(chunks[i].lastPos, chunks[i].pos, timeStacker) - camPos;
			Vector2 vector4 = ((i < chunks.Length - 1) ? vector2 : (vector3 * 2f - vector));
			float num2 = ((i < chunks.Length - 1) ? chunks[i + 1].visualRad : chunks[i].visualRad);
			Vector2 normalized = (vector2 - vector).normalized;
			Vector2 vector5 = (flip ? (-1f) : 1f) * new Vector2(0f - normalized.y, normalized.x);
			mesh.MoveVertice(num, vector3 - vector5 * chunks[i].visualRad);
			mesh.MoveVertice(num + 1, vector3 + vector5 * chunks[i].visualRad);
			mesh.MoveVertice(num + 2, (vector3 + vector4) / 2f - vector5 * (chunks[i].visualRad + num2) / 2f);
			mesh.MoveVertice(num + 3, (vector3 + vector4) / 2f + vector5 * (chunks[i].visualRad + num2) / 2f);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		float b = 0f;
		if (room.game.cameras[0].room == room)
		{
			for (int i = 0; i < volumeConfigs.Length; i++)
			{
				if (room.abstractRoom.name.ToLowerInvariant() == volumeConfigs[i].roomName.ToLowerInvariant() && room.game.cameras[0].currentCameraPosition == volumeConfigs[i].cameraNumber)
				{
					b = volumeConfigs[i].volumeMultiplier;
					break;
				}
			}
		}
		currentVolumeMultiplier = Mathf.Lerp(currentVolumeMultiplier, b, 0.02f);
		if (swimLoop != null)
		{
			swimLoop.Update();
			swimLoop.pos = chunks[0].pos;
			swimLoop.volume = currentVolumeMultiplier;
		}
		else
		{
			swimLoop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Rot_Worm_Woosh_LOOP, default(Vector2), room, 1f, 1f);
			swimLoop.volume = 0f;
		}
		if (glowLoop != null)
		{
			glowLoop.Update();
			int currentCameraPosition = room.game.cameras[0].currentCameraPosition;
			if (room.game.cameras[0].room == room && currentCameraPosition >= 0 && currentCameraPosition < room.cameraPositions.Length)
			{
				glowLoop.Volume = currentVolumeMultiplier * (1f - Mathf.InverseLerp(0f, 7000f, Vector2.Distance(room.cameraPositions[currentCameraPosition] + new Vector2(room.game.rainWorld.options.ScreenSize.x / 2f, 384f), chunks[0].pos)));
			}
		}
		else
		{
			glowLoop = new DisembodiedDynamicSoundLoop(this);
			glowLoop.Volume = 0f;
			glowLoop.sound = WatcherEnums.WatcherSoundID.Rot_Worm_Aura_LOOP;
		}
		float num = (float)testAnimation / 40f * 0.25f;
		Vector2 vector = new Vector2(Mathf.Cos(num), Mathf.Sin(2f * num) / 3f) * 6000f + room.game.cameras[0].pos + room.game.cameras[0].sSize / 2f;
		testAnimation++;
		Vector2 vector2 = chunks[0].pos;
		Vector2 normalized = (chunks[0].pos - chunks[1].pos).normalized;
		Vector2 vector3 = -Custom.PerpendicularVector(normalized);
		Vector2 vel = vector - chunks[0].pos;
		chunks[0].vel = vel;
		float num2 = 0f;
		for (int j = 0; j < chunks.Length; j++)
		{
			if (j > 0)
			{
				chunks[j].vel *= Custom.LerpMap(chunks[j].vel.magnitude, 1f, 10f, 0.99f, 0.85f);
				Vector2 v = chunks[j - 1].pos - chunks[j].pos;
				float magnitude = v.magnitude;
				float num3 = chunks[j].connectionRad + chunks[j - 1].connectionRad;
				Vector2 normalized2 = v.normalized;
				num2 += num3;
				Vector2 vector4 = normalized2 * ((magnitude - num3) * 0.8f);
				chunks[j].vel += vector4;
				chunks[j - 1].vel -= vector4;
				chunks[j].vel -= 0.5f * Vector2.Dot(Custom.PerpendicularVector(v), chunks[j].vel) * Custom.PerpendicularVector(v);
				float num4 = (1f - num2 / wormLength) * Mathf.Pow(num2 / wormLength, 0.2f);
				num4 *= Mathf.Cos(num * (float)Math.PI * 4.2f - num2 / wormLength * 20f);
				chunks[j].vel += Custom.PerpendicularVector(v) * (num4 * 16f);
			}
			if (j > 1)
			{
				float num5 = Vector2.Distance(chunks[j - 2].pos, chunks[j].pos);
				float num6 = (chunks[j - 2].connectionRad + 2f * chunks[j - 1].connectionRad + chunks[j].connectionRad) / 4f;
				if (num5 < num6)
				{
					Vector2 vector5 = (chunks[j].pos - chunks[j - 2].pos).normalized * ((num6 - num5) * 0.2f);
					chunks[j].vel += vector5;
					chunks[j - 2].vel -= vector5;
				}
			}
			chunks[j].lastPos = chunks[j].pos;
			chunks[j].pos += chunks[j].vel;
		}
		for (int k = 0; k < fins.Length; k++)
		{
			Fin fin = fins[k];
			Vector2 vector6 = Vector3.Slerp(ChunkDir(Mathf.FloorToInt(fin.attachment)), ChunkDir(Mathf.CeilToInt(fin.attachment)), fin.attachment % 1f);
			Vector2 vec = new Vector2(0f - vector6.y, vector6.x) * fin.side;
			Vector2 vector7 = Vector2.Lerp(chunks[Mathf.FloorToInt(fin.attachment)].pos, chunks[Mathf.CeilToInt(fin.attachment)].pos, fin.attachment % 1f);
			float num7 = 50f + Mathf.Sin(num * (float)Math.PI * 4.2f - fin.attachment * 0.3f) * 20f;
			vec = Custom.RotateAroundOrigo(vec, (float)fin.side * num7);
			fin.chunks[0].lastPos = fin.chunks[0].pos;
			fin.chunks[0].pos = vector7 + vec * (0.85f * Mathf.Lerp(chunks[Mathf.FloorToInt(fin.attachment)].connectionRad, chunks[Mathf.CeilToInt(fin.attachment)].connectionRad, fin.attachment % 1f));
			for (int l = 1; l < fin.chunks.Length; l++)
			{
				fin.chunks[l].lastPos = fin.chunks[l].pos;
				float num8 = Vector2.Distance(fin.chunks[l].pos, fin.chunks[l - 1].pos);
				float num9 = fin.chunks[l - 1].connectionRad + fin.chunks[l].connectionRad;
				float t = Mathf.Max(1f - (float)l / (float)fin.chunks.Length * 2f, 0f);
				Vector2 vector8 = fin.chunks[0].pos + vec * ((float)l * num9);
				fin.chunks[l].vel *= Custom.LerpMap(fin.chunks[l].vel.magnitude, 3f, 20f, 0.95f, 0.9f, 2f);
				Vector2 vector9 = (fin.chunks[l - 1].pos - fin.chunks[l].pos).normalized * ((num8 - num9) * Mathf.Lerp(0.5f, 0.8f, t));
				fin.chunks[l].vel += vector9;
				fin.chunks[l - 1].vel -= vector9;
				fin.chunks[l].vel += (vector8 - fin.chunks[l].pos) * Mathf.Lerp(0f, 0.1f, t);
				fin.chunks[l].pos += fin.chunks[l].vel;
			}
		}
		for (int m = 0; m < arms.Length; m++)
		{
			Arm arm = arms[m];
			Vector2 vector10 = vector2 + vector3 * arm.anchor.x + normalized * arm.anchor.y;
			Vector2 vector11 = arm.dir.x * vector3 + arm.dir.y * normalized;
			arm.chunks[0].lastPos = arm.chunks[0].pos;
			arm.chunks[0].pos = vector10;
			arm.chunks[0].vel = Vector2.zero;
			for (int n = 1; n < arm.chunks.Length; n++)
			{
				float num10 = (float)n / ((float)arm.chunks.Length - 1f);
				Vector2 v2 = arm.chunks[n - 1].pos - arm.chunks[n].pos;
				float magnitude2 = v2.magnitude;
				float num11 = arm.chunks[n].connectionRad + arm.chunks[n - 1].connectionRad;
				float num12 = Mathf.Pow(Mathf.Lerp(1f, 0f, num10 * 3f), 2f);
				arm.chunks[n].lastPos = arm.chunks[n].pos;
				arm.chunks[n].vel *= 0.999f;
				arm.chunks[n].vel = Vector2.Lerp(arm.chunks[n].vel, chunks[0].vel, num12 * 0.75f);
				arm.chunks[n].vel -= 0.15f * Vector2.Dot(Custom.PerpendicularVector(v2), arm.chunks[n].vel) * Custom.PerpendicularVector(v2);
				Vector2 vector12 = v2.normalized * ((magnitude2 - num11) * ((magnitude2 < num11) ? num12 : 1f) * 0.5f);
				arm.chunks[n].vel += vector12;
				arm.chunks[n - 1].vel -= vector12;
				arm.chunks[n].vel += vector11 * (35f * num12 * (arm.stiffnessFac * 0.1f + 0.9f));
				arm.chunks[n].pos += arm.chunks[n].vel;
			}
		}
		Bulb[] array = bulbs;
		foreach (Bulb bulb in array)
		{
			bulb.Update();
			bulb.originPos = vector2;
			bulb.pos = vector2 + vector3 * bulb.anchor.x + normalized * bulb.anchor.y;
		}
	}

	public override void Destroy()
	{
		if (swimLoop != null && swimLoop.emitter != null)
		{
			swimLoop.emitter.Destroy();
		}
		if (glowLoop != null && glowLoop.emitter != null)
		{
			glowLoop.emitter.Destroy();
		}
		base.Destroy();
	}

	public Vector2 ChunkDir(int chunkIndex)
	{
		Vector2 vector = chunks[Mathf.Max(chunkIndex - 1, 0)].pos;
		return (chunks[Mathf.Min(chunkIndex + 1, chunks.Length - 1)].pos - vector).normalized;
	}
}
