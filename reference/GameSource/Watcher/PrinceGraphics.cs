using System;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class PrinceGraphics : GraphicsModule
{
	public struct Feather
	{
		public float alongBody;

		public float perpOffset;

		public float length;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public Feather(float alongBody, float perpOffset, float length)
		{
			this.alongBody = alongBody;
			this.perpOffset = perpOffset;
			this.length = length;
			pos = default(Vector2);
			lastPos = default(Vector2);
			vel = default(Vector2);
		}
	}

	public class Root
	{
		public RootConnection[] connections;

		public SimpleSegment[] segments;

		public float lightness;

		public float perpOffset;

		public Root(float length, float lightness, float perpOffset)
		{
			this.lightness = lightness;
			this.perpOffset = perpOffset;
			int num = Mathf.Max(Mathf.CeilToInt(length / 17f) + 1, 2);
			segments = new SimpleSegment[num];
			connections = new RootConnection[num - 1];
			float num2 = 0f;
			Vector2 scale = default(Vector2);
			for (int i = 0; i < connections.Length; i++)
			{
				float num3 = num2;
				bool flag = i == connections.Length - 1;
				int variant = UnityEngine.Random.Range(0, 5);
				scale.x = ((UnityEngine.Random.value < 0.5f) ? 1f : (-1f)) * UnityEngine.Random.Range(0.5f, 1f);
				scale.y = ((UnityEngine.Random.value < 0.5f && !flag) ? 1f : (-1f));
				num2 = UnityEngine.Random.Range(-45f, 45f);
				connections[i] = new RootConnection(variant, scale, length / (float)connections.Length, num2 - num3);
			}
		}

		public void Reset(Vector2 pos)
		{
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].Reset(pos);
				if (i < connections.Length)
				{
					pos.y -= connections[i].length;
				}
			}
		}
	}

	public struct RootConnection
	{
		public int variant;

		public Vector2 scale;

		public float length;

		public float angle;

		public RootConnection(int variant, Vector2 scale, float length, float angle)
		{
			this.variant = variant;
			this.scale = scale;
			this.length = length;
			this.angle = angle;
		}
	}

	public class Bloom
	{
		public const int stem = 0;

		public const int firstPetal = 1;

		public const int ring = 5;

		public const int pistil = 6;

		public const int firstStamen = 7;

		public const int stamens = 3;

		public PrinceGraphics owner;

		public Vector2 root;

		public Vector2 rootHandle;

		public Vector2 head;

		public Vector2 headHandle;

		public float radius;

		public bool big;

		public float rotation;

		public float[] petalStickOut = new float[4];

		public static Vector2[] ringQuad = new Vector2[4];

		public int TotalSprites
		{
			get
			{
				if (!big)
				{
					return 6;
				}
				return 10;
			}
		}

		public float ScaleFactor => radius / 13.5f;

		public Bloom(PrinceGraphics owner, Vector2 root, Vector2 rootHandle, Vector2 head, Vector2 headHandle, float radius, bool big)
		{
			this.owner = owner;
			this.root = root;
			this.rootHandle = rootHandle;
			this.head = head;
			this.headHandle = headHandle;
			this.radius = radius;
			this.big = big;
			rotation = UnityEngine.Random.value * 360f;
			for (int i = 0; i < 4; i++)
			{
				petalStickOut[i] = 1f - 1.5f * Mathf.Pow(UnityEngine.Random.value, 1.5f);
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite)
		{
			Color glowColor = owner.glowColor;
			Color color = RainWorld.SaturatedGold * 1.5f;
			Color goldRGB = RainWorld.GoldRGB;
			TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(5, pointyTip: false, customColor: true);
			sLeaser.sprites[firstSprite] = triangleMesh;
			for (int i = 0; i < triangleMesh.verticeColors.Length; i += 2)
			{
				float num = (float)i / (float)(triangleMesh.verticeColors.Length - 2);
				Color a = Color.Lerp(glowColor, color, num);
				a = Color.Lerp(a, goldRGB, num * (1f - num) * 3f);
				triangleMesh.verticeColors[i] = (triangleMesh.verticeColors[i + 1] = a);
			}
			for (int j = 0; j < 4; j++)
			{
				sLeaser.sprites[firstSprite + 1 + j] = new FSprite("KarmaPetal")
				{
					anchorY = 0f,
					color = color
				};
			}
			TriangleMesh triangleMesh2 = TriangleMesh.MakeGridMesh(big ? "EndGameCircle" : "LizardBubble6", 5);
			sLeaser.sprites[firstSprite + 5] = triangleMesh2;
			triangleMesh2.color = color;
			if (big)
			{
				sLeaser.sprites[firstSprite + 6] = TriangleMesh.MakeLongMesh(3, pointyTip: false, customColor: true);
				for (int k = 0; k < 3; k++)
				{
					sLeaser.sprites[firstSprite + 7 + k] = TriangleMesh.MakeLongMesh(3, pointyTip: false, customColor: true);
				}
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
			Vector2 vector = owner.Prince.head.DrawPos(timeStacker);
			Vector2 normalized = (vector - Vector2.Lerp(owner.Prince.bodyChunks[0].lastPos, owner.Prince.bodyChunks[0].pos, timeStacker)).normalized;
			Vector2 vector2 = -Custom.PerpendicularVector(normalized);
			float num = Mathf.Lerp(owner.Prince.lastBloom, owner.Prince.bloom, timeStacker);
			Vector2 vector3 = vector + root.x * vector2 + root.y * normalized;
			Vector2 vector4 = vector3 + rootHandle.x * vector2 + rootHandle.y * normalized;
			Vector2 vector5 = vector + head.x * vector2 + head.y * normalized;
			Vector2 vector6 = vector5 + headHandle.x * vector2 + headHandle.y * normalized;
			if (num < 1f)
			{
				Vector2 vector7 = Custom.Bezier(vector3, vector4, vector5, vector6, num);
				Vector2 vector8 = -Custom.BezierDerivative(vector3, vector4, vector5, vector6, num).normalized * headHandle.magnitude * num + vector7;
				Vector2 vector9 = Vector2.Lerp(vector3, vector4, num);
				vector5 = vector7;
				vector6 = vector8;
				vector4 = vector9;
			}
			for (int i = 0; i < 2; i++)
			{
				float num2 = 2f * (Mathf.PerlinNoise(((float)rCam.game.clock + timeStacker) / 40f, (float)i + root.x * 10f) * 2f - 1f);
				vector5[i] += num2;
				vector6[i] += num2 * 0.9f;
			}
			Vector2 normalized2 = (vector5 - vector6).normalized;
			Vector2 vector10 = -Custom.PerpendicularVector(normalized2);
			for (int j = 0; j < triangleMesh.vertices.Length; j += 2)
			{
				float num3 = (float)j / (float)(triangleMesh.vertices.Length - 2);
				Vector2 vector11 = Custom.Bezier(vector3, vector4, vector5, vector6, num3);
				Vector2 vector12 = Custom.PerpendicularVector(Custom.BezierDerivative(vector3, vector4, vector5, vector6, num3));
				float num4 = Mathf.Lerp(1.25f, 0.8f, num3) * ScaleFactor * num;
				triangleMesh.MoveVertice(j, vector11 + vector12 * num4 - camPos);
				triangleMesh.MoveVertice(j + 1, vector11 - vector12 * num4 - camPos);
			}
			for (int k = 0; k < 4; k++)
			{
				FSprite obj = sLeaser.sprites[firstSprite + 1 + k];
				float num5 = (float)k * 90f + rotation;
				float num6 = (big ? 0.8f : 0.6f);
				Vector2 vector13 = normalized2 * (Mathf.Sin(num5 * ((float)Math.PI / 180f)) * num6 + petalStickOut[k] * 0.5f) + vector10 * Mathf.Cos(num5 * ((float)Math.PI / 180f));
				vector13 *= radius * num;
				obj.SetPosition(vector5 - camPos);
				obj.rotation = Custom.VecToDeg(vector13);
				obj.scaleY = vector13.magnitude / 20f;
				obj.scaleX = ScaleFactor * 0.5f * 0.75f;
				ringQuad[k] = vector5 + vector13 * 1.41f - camPos;
			}
			TriangleMesh.QuadGridMesh(ringQuad, sLeaser.sprites[firstSprite + 5] as TriangleMesh, 5);
			for (int l = 0; l < TotalSprites; l++)
			{
				sLeaser.sprites[firstSprite + l].isVisible = num > 0f;
			}
			sLeaser.sprites[firstSprite + 6].isVisible = false;
			for (int m = 0; m < 3; m++)
			{
				sLeaser.sprites[firstSprite + 7 + m].isVisible = false;
			}
		}
	}

	public Feather[] feathers;

	public Root[] longRoots;

	public Bloom[] blooms;

	public Color effectColor;

	public Color glowColor;

	public int fadeSprite;

	public int firstLongRootSprite;

	public int firstChunkSprite;

	public int bodyMeshSprite;

	public int firstDenseRootSprite;

	public int firstFeatherSprite;

	public int firstHeadSprite;

	public int firstBloomSprite;

	public int firstKarmaLightSprite;

	public int totalSprites;

	public float headTilt;

	public float lastHeadTilt;

	public static float featherStickOut = 1f;

	public static float featherAlign = 1.2f;

	public static float featherGravity = 0.5f;

	public static float featherDrag = 0.95f;

	public Prince Prince => base.owner as Prince;

	public int TotalSegments => Prince.bodyChunks.Length + 1;

	public Vector2 GetNeckDir(float timeStacker)
	{
		return (Prince.head.DrawPos(timeStacker) - Vector2.Lerp(Prince.bodyChunks[0].lastPos, Prince.bodyChunks[0].pos, timeStacker)).normalized;
	}

	public Vector2 GetLookDir(float timeStacker)
	{
		return (Prince.behavior.lookPoint + Vector2.down * 100f - Prince.head.DrawPos(timeStacker)).normalized;
	}

	public PrinceGraphics(PhysicalObject ow, bool internalContainers)
		: base(ow, internalContainers)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(1);
		feathers = new Feather[100];
		for (int i = 0; i < feathers.Length; i++)
		{
			float alongBody = (Mathf.Clamp01((float)(i / 2) / (float)(feathers.Length / 2) * 0.95f + UnityEngine.Random.value * 0.05f) * 0.92f + 0.08f) * (float)(TotalSegments - 1);
			float perpOffset = (float)(i % 2 * 2 - 1) * UnityEngine.Random.Range(0.95f, 1f);
			float length = UnityEngine.Random.Range(7f, 11f);
			feathers[i] = new Feather(alongBody, perpOffset, length);
		}
		longRoots = new Root[20];
		float[] array = new float[longRoots.Length];
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = (float)j / (float)(array.Length - 1) * 2f - 1f;
		}
		Shuffle(array);
		for (int k = 0; k < longRoots.Length; k++)
		{
			float lightness = 1f - (float)k / (float)(longRoots.Length - 1);
			float num = array[k];
			float length2 = UnityEngine.Random.Range(30f, 50f) + 50f * Mathf.Cos(num * (float)Math.PI / 2f);
			longRoots[k] = new Root(length2, lightness, num);
		}
		int num2 = 7;
		blooms = new Bloom[num2 + 1];
		for (int l = 0; l < num2; l++)
		{
			float num3 = (float)l / (float)(num2 - 1) * 2f - 1f;
			float num4 = UnityEngine.Random.Range(0.7f, 1f);
			Vector2 vector = new Vector2(num3 * 8f, 6f);
			Vector2 rootHandle = Custom.DegToVec(UnityEngine.Random.Range(-20f, 20f) + num3 * 30f) * 10f;
			Vector2 head = vector + Custom.DegToVec(num3 * 80f) * (UnityEngine.Random.Range(10f, 20f) * num4) + UnityEngine.Random.insideUnitCircle * 5f;
			Vector2 headHandle = -Custom.DegToVec(UnityEngine.Random.Range(-20f, 20f) + num3 * 60f) * 10f;
			float radius = num4 * 7f;
			blooms[l] = new Bloom(this, vector, rootHandle, head, headHandle, radius, big: false);
		}
		Vector2 vector2 = new Vector2(0f, 6f);
		Vector2 rootHandle2 = Custom.DegToVec(45f) * 20f;
		Vector2 head2 = vector2 + new Vector2(0f, 33f);
		Vector2 headHandle2 = new Vector2(0f, -5f);
		float radius2 = 12f;
		blooms[num2] = new Bloom(this, vector2, rootHandle2, head2, headHandle2, radius2, big: true);
		effectColor = ow.abstractPhysicalObject.world.region?.regionParams.corruptionEffectColor ?? Color.blue;
		glowColor = new Color(0.89f, 0.89f, 0.79f);
		fadeSprite = totalSprites;
		totalSprites++;
		firstLongRootSprite = totalSprites;
		totalSprites += longRoots.Sum((Root root) => root.connections.Length);
		firstChunkSprite = totalSprites;
		totalSprites += Prince.bodyChunks.Length - 1;
		bodyMeshSprite = totalSprites;
		totalSprites++;
		firstDenseRootSprite = totalSprites;
		totalSprites += 3;
		firstFeatherSprite = totalSprites;
		totalSprites += feathers.Length;
		firstHeadSprite = totalSprites;
		totalSprites += 2;
		firstBloomSprite = totalSprites;
		totalSprites += blooms.Sum((Bloom bloom) => bloom.TotalSprites);
		firstKarmaLightSprite = totalSprites;
		totalSprites += 5;
		UnityEngine.Random.state = state;
	}

	public void Shuffle(float[] values)
	{
		for (int i = 0; i < values.Length - 1; i++)
		{
			int num = UnityEngine.Random.Range(i, values.Length);
			ref float reference = ref values[i];
			ref float reference2 = ref values[num];
			float num2 = values[num];
			float num3 = values[i];
			reference = num2;
			reference2 = num3;
		}
	}

	public override void Reset()
	{
		base.Reset();
		Vector2 pos = Prince.bodyChunks[0].pos;
		for (int i = 0; i < feathers.Length; i++)
		{
			feathers[i].lastPos = pos;
			feathers[i].pos = pos;
			feathers[i].vel = Vector2.zero;
		}
		pos = Prince.bodyChunks[0].pos;
		for (int j = 0; j < longRoots.Length; j++)
		{
			longRoots[j].Reset(pos);
		}
	}

	public override void Update()
	{
		base.Update();
		lastHeadTilt = headTilt;
		float num = Vector2.SignedAngle(GetLookDir(1f), -GetNeckDir(1f));
		if (Mathf.Abs(Mathf.DeltaAngle(headTilt, num)) > 5f)
		{
			headTilt = Mathf.LerpAngle(headTilt, num, 0.2f);
		}
		for (int i = 0; i < feathers.Length; i++)
		{
			Vector2 bodyDir;
			Vector2 featherAnchor = GetFeatherAnchor(feathers[i].alongBody, feathers[i].perpOffset, 1f, out bodyDir);
			ref Feather reference = ref feathers[i];
			reference.lastPos = reference.pos;
			reference.vel *= featherDrag;
			reference.vel += featherAlign * (GetFeatherAnchor(feathers[i].alongBody + 0.1f, feathers[i].perpOffset, 1f, out var bodyDir2) - featherAnchor).normalized;
			reference.vel += Custom.PerpendicularVector(bodyDir) * (featherStickOut * reference.perpOffset);
			reference.vel.y -= featherGravity;
			ref Vector2 vel = ref reference.vel;
			Vector2 vector = vel;
			bodyDir2 = featherAnchor - reference.pos;
			vel = vector + bodyDir2.normalized * ((Vector2.Distance(featherAnchor, reference.pos) - reference.length) * 0.2f);
			reference.pos = Vector2.ClampMagnitude(reference.pos + reference.vel - featherAnchor, reference.length) + featherAnchor;
		}
		Vector2 segmentPos = GetSegmentPos(TotalSegments - 1, 1f);
		Vector2 segmentDir = GetSegmentDir(TotalSegments - 1, 1f);
		Vector2 vector2 = Custom.PerpendicularVector(segmentDir);
		float bodyRad = GetBodyRad(1f, 1f);
		Root[] array = longRoots;
		foreach (Root root in array)
		{
			RootConnection[] connections = root.connections;
			SimpleSegment[] segments = root.segments;
			segments[0].lastPos = segments[0].pos;
			segments[0].vel = Vector2.zero;
			segments[0].pos = segmentPos + vector2 * (root.perpOffset * bodyRad);
			Vector2 vector3 = segmentDir;
			for (int k = 1; k < segments.Length; k++)
			{
				RootConnection rootConnection = connections[k - 1];
				ref SimpleSegment reference2 = ref segments[k];
				ref SimpleSegment reference3 = ref segments[k - 1];
				vector3 = Custom.RotateAroundOrigo(vector3, rootConnection.angle);
				Vector2 vector4 = reference3.pos + vector3 * (rootConnection.length * 0.75f) - reference2.pos;
				reference2.lastPos = reference2.pos;
				reference2.vel *= 0.95f;
				reference2.vel.y -= 0.5f;
				reference2.vel += vector4 * 0.25f;
				reference2.pos += vector4 * 0.25f;
				reference3.vel -= vector4 * 0.25f;
				reference3.pos -= vector4 * 0.25f;
				reference2.pos += reference2.vel;
			}
			segments[0].pos = segmentPos + vector2 * (root.perpOffset * bodyRad);
			for (int l = 0; l < connections.Length; l++)
			{
				Vector2 vector5 = segments[l + 1].pos - segments[l].pos;
				segments[l + 1].pos = Vector2.ClampMagnitude(vector5, connections[l].length) + segments[l].pos;
				vector5.Normalize();
				segments[l + 1].vel -= vector5 * Vector2.Dot(segments[l + 1].vel, vector5);
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[fadeSprite] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"]
		};
		int num = firstLongRootSprite;
		Root[] array = longRoots;
		foreach (Root root in array)
		{
			for (int j = 0; j < root.connections.Length; j++)
			{
				bool flag = j == root.connections.Length - 1;
				int variant = root.connections[j].variant;
				sLeaser.sprites[num++] = new FSprite(flag ? $"PrinceRootB{variant}" : $"PrinceRootA{variant}");
			}
		}
		sLeaser.sprites[firstDenseRootSprite] = new FSprite("PrinceFeather2")
		{
			anchorY = 1f
		};
		sLeaser.sprites[firstDenseRootSprite + 1] = new FSprite("PrinceFeather3")
		{
			anchorY = 1f
		};
		sLeaser.sprites[firstDenseRootSprite + 2] = new FSprite("PrinceFeather4")
		{
			anchorY = 1f
		};
		for (int k = 0; k < Prince.bodyChunks.Length - 1; k++)
		{
			sLeaser.sprites[firstChunkSprite + k] = new FSprite("Futile_White")
			{
				shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
				alpha = 0.9f
			};
		}
		sLeaser.sprites[bodyMeshSprite] = TriangleMesh.MakeLongMesh(15, pointyTip: false, customColor: false);
		for (int l = 0; l < feathers.Length; l++)
		{
			sLeaser.sprites[firstFeatherSprite + l] = new FSprite("PrinceFeatherB0")
			{
				scaleX = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * UnityEngine.Random.Range(0.5f, 1f),
				anchorY = 0.8f
			};
		}
		sLeaser.sprites[firstHeadSprite] = new FSprite("PrincePetals0");
		sLeaser.sprites[firstHeadSprite + 1] = new FSprite("PrinceHead0");
		int num2 = firstBloomSprite;
		Bloom[] array2 = blooms;
		foreach (Bloom bloom in array2)
		{
			bloom.InitiateSprites(sLeaser, rCam, num2);
			num2 += bloom.TotalSprites;
		}
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[8]
		{
			new TriangleMesh.Triangle(0, 4, 1),
			new TriangleMesh.Triangle(1, 4, 2),
			new TriangleMesh.Triangle(2, 4, 5),
			new TriangleMesh.Triangle(5, 4, 8),
			new TriangleMesh.Triangle(8, 4, 7),
			new TriangleMesh.Triangle(7, 4, 6),
			new TriangleMesh.Triangle(6, 4, 3),
			new TriangleMesh.Triangle(3, 4, 0)
		};
		for (int m = 0; m < 2; m++)
		{
			sLeaser.sprites[firstKarmaLightSprite + m] = new TriangleMesh("Futile_White", tris, customColor: false)
			{
				shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"]
			};
		}
		sLeaser.sprites[firstKarmaLightSprite + 2] = new TriangleMesh("Futile_White", tris, customColor: false)
		{
			shader = rCam.game.rainWorld.Shaders["FlatLight"]
		};
		sLeaser.sprites[firstKarmaLightSprite + 3] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"]
		};
		sLeaser.sprites[firstKarmaLightSprite + 4] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["FlatLight"]
		};
		for (int n = 0; n < 3; n++)
		{
			Vector2[] uVvertices = (sLeaser.sprites[firstKarmaLightSprite + n] as TriangleMesh).UVvertices;
			for (int num3 = 0; num3 < 9; num3++)
			{
				uVvertices[num3] = new Vector2(0.5f, (num3 == 4) ? 0.4f : 0f);
			}
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public TriangleMesh.Triangle[] MakeRingTris(int quads)
	{
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[quads * 2];
		int num = quads * 2;
		for (int i = 0; i < array.Length; i += 2)
		{
			array[i] = new TriangleMesh.Triangle(i, i + 1, (i + 3) % num);
			array[i + 1] = new TriangleMesh.Triangle(i, (i + 3) % num, (i + 2) % num);
		}
		return array;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Prince.head.DrawPos(timeStacker);
		float num = Mathf.Lerp(Prince.lastBloom, Prince.bloom, timeStacker);
		sLeaser.sprites[fadeSprite].SetPosition(vector - camPos);
		sLeaser.sprites[fadeSprite].scale = 25f;
		sLeaser.sprites[fadeSprite].alpha = 0.1f;
		int num2 = firstLongRootSprite;
		Root[] array = longRoots;
		foreach (Root obj in array)
		{
			RootConnection[] connections = obj.connections;
			SimpleSegment[] segments = obj.segments;
			for (int j = 0; j < connections.Length; j++)
			{
				RootConnection rootConnection = connections[j];
				Vector2 vector2 = segments[j].DrawPos(timeStacker);
				Vector2 vector3 = segments[j + 1].DrawPos(timeStacker);
				sLeaser.sprites[num2].SetPosition((vector2 + vector3) / 2f - camPos);
				sLeaser.sprites[num2].rotation = Custom.AimFromOneVectorToAnother(vector2, vector3);
				sLeaser.sprites[num2].scaleX = rootConnection.scale.x;
				sLeaser.sprites[num2].scaleY = rootConnection.scale.y * Vector2.Distance(vector2, vector3) / 17f;
				num2++;
			}
		}
		Vector2 segmentPos = GetSegmentPos(TotalSegments - 1, timeStacker);
		Vector2 segmentDir = GetSegmentDir(TotalSegments - 1, timeStacker);
		float num3 = GetBodyRad(1f, timeStacker) / 20f;
		sLeaser.sprites[firstDenseRootSprite].SetPosition(segmentPos + segmentDir * 6f - camPos);
		sLeaser.sprites[firstDenseRootSprite].rotation = Custom.VecToDeg(-segmentDir);
		sLeaser.sprites[firstDenseRootSprite].scaleX = 1f;
		sLeaser.sprites[firstDenseRootSprite].scaleY = 1.5f;
		sLeaser.sprites[firstDenseRootSprite + 1].SetPosition(segmentPos - camPos);
		sLeaser.sprites[firstDenseRootSprite + 1].rotation = Custom.VecToDeg(-segmentDir);
		sLeaser.sprites[firstDenseRootSprite + 1].scale = Mathf.Lerp(num3, 1f, 0.5f);
		sLeaser.sprites[firstDenseRootSprite + 2].SetPosition(segmentPos - camPos);
		sLeaser.sprites[firstDenseRootSprite + 2].rotation = Custom.VecToDeg(-segmentDir);
		sLeaser.sprites[firstDenseRootSprite + 2].scale = num3;
		for (int k = 0; k < Prince.bodyChunks.Length - 1; k++)
		{
			sLeaser.sprites[firstChunkSprite + k].SetPosition(Vector2.Lerp(Prince.bodyChunks[k].lastPos, Prince.bodyChunks[k].pos, timeStacker) - camPos);
			sLeaser.sprites[firstChunkSprite + k].scale = GetBodyRad((float)(k + 1) / (float)(TotalSegments - 1), timeStacker) / 8f;
			sLeaser.sprites[firstChunkSprite + k].rotation = Custom.VecToDeg(Prince.bodyChunks[k].Rotation) + (float)k * 45f;
		}
		TriangleMesh triangleMesh = sLeaser.sprites[bodyMeshSprite] as TriangleMesh;
		Vector2 vector4 = vector + GetNeckDir(timeStacker);
		Vector2 bodyDir;
		for (int l = 0; l < triangleMesh.vertices.Length; l += 2)
		{
			float num4 = (float)l / (float)(triangleMesh.vertices.Length - 2);
			Vector2 posAlongBody = GetPosAlongBody(num4 * (float)(TotalSegments - 1), timeStacker);
			float bodyRad = GetBodyRad(num4, timeStacker);
			bodyDir = posAlongBody - vector4;
			Vector2 vector5 = Custom.PerpendicularVector(bodyDir.normalized);
			triangleMesh.MoveVertice(l, posAlongBody + vector5 * bodyRad - camPos);
			triangleMesh.MoveVertice(l + 1, posAlongBody - vector5 * bodyRad - camPos);
			vector4 = posAlongBody;
		}
		for (int m = 0; m < feathers.Length; m++)
		{
			Vector2 featherAnchor = GetFeatherAnchor(feathers[m].alongBody, feathers[m].perpOffset, timeStacker, out bodyDir);
			Vector2 vector6 = Vector2.Lerp(feathers[m].lastPos, feathers[m].pos, timeStacker);
			sLeaser.sprites[firstFeatherSprite + m].SetPosition(featherAnchor - camPos);
			sLeaser.sprites[firstFeatherSprite + m].rotation = Custom.VecToDeg(featherAnchor - vector6);
			sLeaser.sprites[firstFeatherSprite + m].scaleY = Vector2.Distance(vector6, featherAnchor) / 11f;
		}
		Vector2 normalized = (vector - Vector2.Lerp(Prince.bodyChunks[0].lastPos, Prince.bodyChunks[0].pos, timeStacker)).normalized;
		Vector2 vector7 = vector + Vector2.ClampMagnitude(Prince.behavior.lookPoint - vector, 100f) / 100f * 5f;
		float rotation = Custom.VecToDeg(normalized);
		int num5 = Mathf.RoundToInt(Mathf.Repeat(Mathf.LerpAngle(lastHeadTilt, headTilt, timeStacker), 360f) / 180f * 16f);
		float scaleX = 1f;
		if (num5 > 16)
		{
			num5 = 32 - num5;
			scaleX = -1f;
		}
		sLeaser.sprites[firstHeadSprite].SetElementByName($"PrincePetals{num5}");
		sLeaser.sprites[firstHeadSprite].SetPosition(vector7 - camPos);
		sLeaser.sprites[firstHeadSprite].rotation = rotation;
		sLeaser.sprites[firstHeadSprite].scaleX = scaleX;
		sLeaser.sprites[firstHeadSprite + 1].SetElementByName($"PrinceHead{num5}");
		sLeaser.sprites[firstHeadSprite + 1].SetPosition(vector7 - camPos);
		sLeaser.sprites[firstHeadSprite + 1].rotation = rotation;
		sLeaser.sprites[firstHeadSprite + 1].scaleX = scaleX;
		int num6 = firstBloomSprite;
		Bloom[] array2 = blooms;
		foreach (Bloom bloom in array2)
		{
			bloom.DrawSprites(sLeaser, rCam, timeStacker, camPos, num6);
			num6 += bloom.TotalSprites;
		}
		for (int n = 0; n < 3; n++)
		{
			TriangleMesh triangleMesh2 = sLeaser.sprites[firstKarmaLightSprite + n] as TriangleMesh;
			for (int num7 = 0; num7 < 3; num7++)
			{
				float num8 = 1 - num7;
				float num9 = 200f - (float)n * 40f;
				Vector2 vector8 = vector - Custom.PerpendicularVector(normalized) * (num8 * 40f) - camPos;
				Vector2 vector9 = Custom.RotateAroundOrigo(normalized, 40f * num8);
				triangleMesh2.MoveVertice(num7 * 3, vector8 - vector9 * 20f);
				triangleMesh2.MoveVertice(num7 * 3 + 1, vector8);
				triangleMesh2.MoveVertice(num7 * 3 + 2, vector8 + vector9 * num9);
			}
		}
		sLeaser.sprites[firstKarmaLightSprite + 3].SetPosition(vector - camPos);
		sLeaser.sprites[firstKarmaLightSprite + 3].scale = 17.5f;
		sLeaser.sprites[firstKarmaLightSprite + 4].SetPosition(vector - camPos);
		sLeaser.sprites[firstKarmaLightSprite + 4].scale = 10f;
		UnityEngine.Random.Range(num, Mathf.Pow(num, 2f));
		sLeaser.sprites[firstKarmaLightSprite].alpha = 0.4f * num;
		sLeaser.sprites[firstKarmaLightSprite + 1].alpha = 0.25f * num;
		sLeaser.sprites[firstKarmaLightSprite + 2].alpha = 0.3f * num;
		sLeaser.sprites[firstKarmaLightSprite + 3].alpha = 0.25f * num;
		sLeaser.sprites[firstKarmaLightSprite + 4].alpha = 0.15f * num;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		rCam.ReturnFContainer("Shortcuts").AddChild(sLeaser.sprites[fadeSprite]);
		FContainer fContainer = rCam.ReturnFContainer("Foreground");
		fContainer.AddChild(sLeaser.sprites[firstKarmaLightSprite + 3]);
		fContainer.AddChild(sLeaser.sprites[firstKarmaLightSprite]);
		fContainer.AddChild(sLeaser.sprites[firstKarmaLightSprite + 1]);
		fContainer.AddChild(sLeaser.sprites[firstKarmaLightSprite + 2]);
		fContainer.AddChild(sLeaser.sprites[firstKarmaLightSprite + 4]);
		for (int i = 0; i < firstKarmaLightSprite; i++)
		{
			if (i != fadeSprite)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public Vector2 GetFeatherAnchor(float seg, float perpOffset, float timeStacker, out Vector2 bodyDir)
	{
		Vector2 posAlongBody = GetPosAlongBody(seg - 0.25f, timeStacker);
		Vector2 posAlongBody2 = GetPosAlongBody(seg + 0.25f, timeStacker);
		Vector2 posAlongBody3 = GetPosAlongBody(seg, timeStacker);
		bodyDir = (posAlongBody2 - posAlongBody).normalized;
		Vector2 vector = Custom.PerpendicularVector(bodyDir);
		return posAlongBody3 + vector * (perpOffset * (GetBodyRad(seg / (float)(TotalSegments - 1), timeStacker) - 1.5f));
	}

	public Vector2 GetPosAlongBody(float seg, float timeStacker)
	{
		float num = Mathf.Clamp(seg, 0f, TotalSegments);
		int num2 = Mathf.Min(Mathf.FloorToInt(num), TotalSegments - 2);
		num -= (float)num2;
		Vector2 segmentPos = GetSegmentPos(num2, timeStacker);
		Vector2 segmentPos2 = GetSegmentPos(num2 + 1, timeStacker);
		Vector2 cA = segmentPos + GetSegmentDir(num2, timeStacker) * (Vector2.Distance(segmentPos, segmentPos2) * 0.25f);
		Vector2 cB = segmentPos2 - GetSegmentDir(num2 + 1, timeStacker) * (Vector2.Distance(segmentPos, segmentPos2) * 0.25f);
		return Custom.Bezier(segmentPos, cA, segmentPos2, cB, num);
	}

	public float GetBodyRad(float t, float timeStacker)
	{
		float num = 0.5f - 0.5f * Mathf.Cos((float)Math.PI * Mathf.Sqrt(Mathf.Clamp01(t * 3f - 0.1f)));
		return 25f * Mathf.Sqrt(t) * num + 3f;
	}

	public Vector2 GetSegmentPos(int seg, float timeStacker)
	{
		if (seg == 0)
		{
			return Prince.head.DrawPos(timeStacker);
		}
		return Vector2.Lerp(Prince.bodyChunks[seg - 1].lastPos, Prince.bodyChunks[seg - 1].pos, timeStacker);
	}

	public Vector2 GetSegmentDir(int seg, float timeStacker)
	{
		if (seg == 0)
		{
			return (GetSegmentPos(1, timeStacker) - GetSegmentPos(0, timeStacker)).normalized;
		}
		if (seg == TotalSegments - 1)
		{
			return (GetSegmentPos(TotalSegments - 1, timeStacker) - GetSegmentPos(TotalSegments - 2, timeStacker)).normalized;
		}
		Vector2 segmentPos = GetSegmentPos(seg - 1, timeStacker);
		Vector2 normalized = (GetSegmentPos(seg - 1, timeStacker) - segmentPos).normalized;
		return ((GetSegmentPos(seg + 1, timeStacker) - segmentPos).normalized - normalized).normalized;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[fadeSprite].color = glowColor;
		int num = firstLongRootSprite;
		Root[] array = longRoots;
		foreach (Root root in array)
		{
			for (int j = 0; j < root.connections.Length; j++)
			{
				float num2 = (float)j / (float)(root.connections.Length - 1);
				sLeaser.sprites[num++].color = Color.Lerp(palette.blackColor, effectColor, root.lightness * 0.5f * num2);
			}
		}
		for (int k = 0; k < 3; k++)
		{
			sLeaser.sprites[firstDenseRootSprite + k].color = Color.Lerp(palette.blackColor, effectColor, (1f - (float)k / 2f) * 0.15f);
		}
		for (int l = 0; l < Prince.bodyChunks.Length - 1; l++)
		{
			sLeaser.sprites[firstChunkSprite + l].color = palette.blackColor;
		}
		for (int m = 0; m < feathers.Length; m++)
		{
			sLeaser.sprites[firstFeatherSprite + m].color = palette.blackColor;
		}
		sLeaser.sprites[bodyMeshSprite].color = palette.blackColor;
		sLeaser.sprites[firstHeadSprite].color = palette.blackColor;
		sLeaser.sprites[firstHeadSprite + 1].color = glowColor;
		sLeaser.sprites[firstKarmaLightSprite].color = Custom.HSL2RGB(RainWorld.AntiGold.hue, 0.6f, 0.2f);
		sLeaser.sprites[firstKarmaLightSprite + 1].color = RainWorld.SaturatedGold;
		sLeaser.sprites[firstKarmaLightSprite + 2].color = Color.white;
		sLeaser.sprites[firstKarmaLightSprite + 3].color = RainWorld.SaturatedGold;
		sLeaser.sprites[firstKarmaLightSprite + 4].color = Color.white;
	}
}
