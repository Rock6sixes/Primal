using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;
using Watcher;

public class PlateTree : CosmeticSprite
{
	public abstract class BranchCap
	{
		public Vector2 localPos;

		public PlateTree tree;

		public Vector2 pos;

		public Vector2 lastPos;

		public float rotation;

		public float lastRotation;

		public float tilt;

		public float handleRad;

		public float attachment;

		public float size;

		public abstract int TotalSprites { get; }

		public Vector2 HandleVec => Custom.RotateAroundOrigo(new Vector2(0f - tilt, -1f) * handleRad, rotation);

		public Vector2 LastHandleVec => Custom.RotateAroundOrigo(new Vector2(0f - tilt, -1f) * handleRad, lastRotation);

		public BranchCap(PlateTree tree, Vector2 localPos, float tilt, float handleRad, float attachment, float size)
		{
			this.tree = tree;
			this.localPos = localPos;
			this.tilt = tilt;
			this.handleRad = handleRad;
			this.attachment = attachment;
			this.size = size;
			Move();
			lastPos = pos;
			lastRotation = rotation;
		}

		public virtual void Update()
		{
			lastPos = pos;
			lastRotation = rotation;
			Move();
		}

		public abstract void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite);

		public abstract void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite);

		public abstract void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int firstSprite);

		public abstract bool NeedsNewSprites(RoomCamera.SpriteLeaser sLeaser, int firstSprite);

		public virtual void ReactToChunk(BodyChunk chunk)
		{
		}

		public virtual float BranchGradient(float t)
		{
			return 0f;
		}

		public void Move()
		{
			Vector2 vector;
			Vector2 vector2;
			Vector2 vector3;
			if (attachment > 0f)
			{
				vector = tree.TrunkPos(attachment, 1f);
				vector2 = tree.TrunkDir(attachment, 1f);
				vector3 = -Custom.PerpendicularVector(vector2);
			}
			else
			{
				vector = tree.pos;
				vector2 = tree.Data.handlePos.normalized;
				vector3 = -Custom.PerpendicularVector(vector2);
			}
			pos = vector + localPos.x * vector3 + localPos.y * vector2;
			rotation = Custom.VecToDeg(vector2) + tilt * 25f;
		}
	}

	public class Plate : BranchCap
	{
		public Color color;

		public Color lastColor;

		public PlateDangler[] danglers;

		public float flashAnim = 1f;

		public Color roomColor;

		public Vector2 spinVelSeed;

		public static TriangleMesh.Triangle[] quadTris = new TriangleMesh.Triangle[2]
		{
			new TriangleMesh.Triangle(0, 1, 2),
			new TriangleMesh.Triangle(0, 2, 3)
		};

		public override int TotalSprites => 1;

		public int TotalVerts => danglers.Length * 8 + 4;

		public Plate(PlateTree tree, Vector2 localPos, float tilt, float handleRad, float attachment, float size, int danglers)
			: base(tree, localPos, tilt, handleRad, attachment, size)
		{
			this.danglers = ((danglers == 0) ? Array.Empty<PlateDangler>() : new PlateDangler[danglers]);
			Vector2 vector = pos;
			vector.y -= size / 2f;
			for (int i = 0; i < danglers; i++)
			{
				this.danglers[i].pos = vector;
				this.danglers[i].lastPos = vector;
				this.danglers[i].vel = default(Vector2);
				this.danglers[i].connectionRad = 3f;
				this.danglers[i].dead = UnityEngine.Random.value < 0.07f;
				vector.y -= this.danglers[i].connectionRad - 8f;
			}
			spinVelSeed = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value) * 200f;
		}

		public override void Update()
		{
			base.Update();
			int clock = tree.room.game.clock;
			pos += new Vector2(Mathf.PerlinNoise((float)clock * 0.002f, attachment * 20f) * 10f - 5f, Mathf.PerlinNoise((float)clock * 0.002f, attachment * 20f + 5f) * 10f - 5f);
			Vector2 vector = -base.HandleVec.normalized;
			Vector3 normalized = new Vector3(vector.x, vector.y, 1f - Mathf.Abs(tilt)).normalized;
			lastColor = this.color;
			roomColor = Color.Lerp(roomColor, tree.room.game.cameras[0].PixelColorAtCoordinate(pos + (Vector2)normalized * 300f), 0.1f + (1f - roomColor.a));
			this.color = tree.metalColor + roomColor * 0.7f;
			this.color.a = 1f;
			_ = this.color;
			if (flashAnim < 1f)
			{
				this.color = Color.Lerp(this.color, Color.white, Mathf.Sin(flashAnim * (float)Math.PI) * UnityEngine.Random.value * 0.5f);
				flashAnim += 0.08333333f;
			}
			else if (UnityEngine.Random.value < 0.001f)
			{
				flashAnim = 0f;
			}
			Vector2 vector2 = pos;
			float num = size / 2f;
			Color color = lastColor;
			for (int i = 0; i < danglers.Length; i++)
			{
				ref PlateDangler reference = ref danglers[i];
				reference.lastColor = reference.color;
				reference.lastPos = reference.pos;
				reference.lastSpin = reference.spin;
				float num2 = Vector2.Distance(vector2, reference.pos);
				float num3 = num + reference.connectionRad;
				if (num2 > num3)
				{
					Vector2 vector3 = (vector2 - reference.pos).normalized * (num2 - num3);
					reference.vel += vector3 * 0.5f;
					reference.pos += vector3 * 0.5f;
					if (i > 0)
					{
						danglers[i - 1].vel -= vector3 * 0.4f;
					}
				}
				reference.pos += reference.vel;
				reference.vel *= 0.99f;
				reference.vel.y -= 1f;
				reference.pos = PushOutOfTerrain(reference.pos, ref reference.vel);
				reference.spin += SpinVel(i);
				if (i > 0)
				{
					reference.spin = Mathf.LerpAngle(reference.spin, danglers[i - 1].spin, 0.05f);
				}
				reference.color = color;
				reference.chunkSpin *= 0.97f;
				num = reference.connectionRad;
				vector2 = reference.pos;
				color = reference.lastColor;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite)
		{
			TriangleMesh.Triangle[] array;
			if (danglers.Length == 0)
			{
				array = quadTris;
			}
			else
			{
				array = new TriangleMesh.Triangle[danglers.Length * 4 + 2];
				int num = FirstPlateTri();
				int num2 = FirstPlateVert();
				array[num] = new TriangleMesh.Triangle(num2, num2 + 1, num2 + 2);
				array[num + 1] = new TriangleMesh.Triangle(num2, num2 + 2, num2 + 3);
				for (int i = 0; i < danglers.Length; i++)
				{
					num = FirstDanglerTri(i);
					num2 = FirstDanglerVert(i);
					array[num] = new TriangleMesh.Triangle(num2, num2 + 1, num2 + 2);
					array[num + 1] = new TriangleMesh.Triangle(num2, num2 + 2, num2 + 3);
					num = FirstStringTri(i);
					num2 = FirstStringVert(i);
					array[num] = new TriangleMesh.Triangle(num2, num2 + 1, num2 + 2);
					array[num + 1] = new TriangleMesh.Triangle(num2, num2 + 2, num2 + 3);
				}
			}
			sLeaser.sprites[firstSprite] = new TriangleMesh("Futile_White", array, customColor: true);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite)
		{
			TriangleMesh triangleMesh = (TriangleMesh)sLeaser.sprites[firstSprite];
			float degAng = Mathf.LerpAngle(lastRotation, rotation, timeStacker);
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			Vector2 vector2 = Custom.RotateAroundOrigo(Vector2.up, degAng);
			Vector2 vector3 = Custom.RotateAroundOrigo(Vector2.right, degAng);
			int num = FirstPlateVert();
			triangleMesh.MoveVertice(num, vector + vector2 * size - camPos);
			triangleMesh.MoveVertice(num + 1, vector + vector3 * (size * 0.7f * (1f - 0.75f * Mathf.Abs(tilt))) - camPos);
			triangleMesh.MoveVertice(num + 2, vector + vector2 * (0f - size) - camPos);
			triangleMesh.MoveVertice(num + 3, vector + vector3 * ((0f - size) * 0.7f * (1f - 0.75f * Mathf.Abs(tilt))) - camPos);
			for (int i = 0; i < 4; i++)
			{
				triangleMesh.verticeColors[num + i] = Color.Lerp(lastColor, this.color, timeStacker);
			}
			PlateDangler[] array = danglers;
			Vector2 vector4 = vector;
			Vector2 vector5 = vector;
			for (int j = 0; j < array.Length; j++)
			{
				Vector2 vector6 = Vector2.Lerp(array[j].lastPos, array[j].pos, timeStacker);
				Color color = Color.Lerp(array[j].lastColor, array[j].color, timeStacker);
				float f = Mathf.LerpAngle(array[j].lastSpin, array[j].spin, timeStacker) * ((float)Math.PI / 180f);
				float num2 = 6f - (float)j / ((float)array.Length - 1f) * 3f;
				float num3 = Mathf.Max(1f / num2, Mathf.Abs(Mathf.Cos(f)));
				num = FirstDanglerVert(j);
				if (array[j].dead)
				{
					color.a = 0f;
				}
				vector2 = (vector4 - vector6).normalized;
				vector3 = new Vector2(vector2.y, 0f - vector2.x);
				triangleMesh.MoveVertice(num, vector6 + vector2 * (num2 * 2f) - camPos);
				triangleMesh.MoveVertice(num + 1, vector6 + vector2 * num2 + vector3 * (num2 * 0.7f * num3) - camPos);
				triangleMesh.MoveVertice(num + 2, vector6 - camPos);
				triangleMesh.MoveVertice(num + 3, vector6 + vector2 * num2 - vector3 * (num2 * 0.7f * num3) - camPos);
				for (int k = 0; k < 4; k++)
				{
					triangleMesh.verticeColors[num + k] = color;
				}
				float num4 = 0.5f;
				num = FirstStringVert(j);
				triangleMesh.MoveVertice(num, vector6 + vector2 * num2 - vector3 * num4 - camPos);
				triangleMesh.MoveVertice(num + 1, vector5 - vector3 * num4 - camPos);
				triangleMesh.MoveVertice(num + 2, vector5 + vector3 * num4 - camPos);
				triangleMesh.MoveVertice(num + 3, vector6 + vector2 * num2 + vector3 * num4 - camPos);
				vector4 = vector6;
				vector5 = vector6 + vector2 * num2;
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int firstSprite)
		{
			TriangleMesh triangleMesh = (TriangleMesh)sLeaser.sprites[firstSprite];
			for (int i = 0; i < danglers.Length; i++)
			{
				int num = FirstStringVert(i);
				for (int j = 0; j < 4; j++)
				{
					triangleMesh.verticeColors[num + j] = palette.blackColor;
				}
			}
		}

		public override bool NeedsNewSprites(RoomCamera.SpriteLeaser sLeaser, int firstSprite)
		{
			if (sLeaser.sprites[firstSprite] is TriangleMesh triangleMesh)
			{
				return triangleMesh.vertices.Length != TotalVerts;
			}
			return true;
		}

		public Vector2 PushOutOfTerrain(Vector2 pos, ref Vector2 vel)
		{
			Room room = tree.room;
			if (!room.GetTile(pos).Solid)
			{
				return pos;
			}
			Vector2 vector = new Vector2(pos.x, Mathf.Floor(pos.y / 20f) * 20f + 20f);
			Vector2 vector2 = new Vector2(pos.x, Mathf.Floor(pos.y / 20f) * 20f);
			Vector2 vector3 = new Vector2(Mathf.Floor(pos.x / 20f) * 20f + 20f, pos.y);
			Vector2 vector4 = new Vector2(Mathf.Floor(pos.x / 20f) * 20f, pos.y);
			Vector2 result = pos;
			Vector2 vector5 = vel;
			float num = float.PositiveInfinity;
			if (!room.GetTile(vector + Vector2.up).Solid && (vector - pos).sqrMagnitude < num)
			{
				result = vector;
				vector5 = new Vector2(vel.x, 0f);
				num = (vector - pos).sqrMagnitude;
			}
			if (!room.GetTile(vector2 + Vector2.down).Solid && (vector2 - pos).sqrMagnitude < num)
			{
				result = vector2;
				vector5 = new Vector2(vel.x, 0f);
				num = (vector2 - pos).sqrMagnitude;
			}
			if (!room.GetTile(vector3 + Vector2.right).Solid && (vector3 - pos).sqrMagnitude < num)
			{
				result = vector3;
				vector5 = new Vector2(0f, vel.y);
				num = (vector3 - pos).sqrMagnitude;
			}
			if (!room.GetTile(vector4 + Vector2.left).Solid && (vector4 - pos).sqrMagnitude < num)
			{
				result = vector4;
				vector5 = new Vector2(0f, vel.y);
				num = (vector4 - pos).sqrMagnitude;
			}
			vel = vector5;
			return result;
		}

		public int FirstStringVert(int dangler)
		{
			return dangler * 4;
		}

		public int FirstDanglerVert(int dangler)
		{
			return danglers.Length * 4 + dangler * 4;
		}

		public int FirstPlateVert()
		{
			return danglers.Length * 8;
		}

		public int FirstStringTri(int dangler)
		{
			return dangler * 2;
		}

		public int FirstDanglerTri(int dangler)
		{
			return danglers.Length * 2 + dangler * 2;
		}

		public int FirstPlateTri()
		{
			return danglers.Length * 4;
		}

		public float SpinVel(int dangler)
		{
			return 5f * (Mathf.PerlinNoise(spinVelSeed.x + (float)dangler * 3.45f + (float)tree.room.game.clock / 40f * 0.2f, spinVelSeed.y + (float)dangler * 6.78f) * 2f - 1f) + 5f * danglers[dangler].vel.x + danglers[dangler].chunkSpin;
		}

		public override void ReactToChunk(BodyChunk chunk)
		{
			if (danglers.Length == 0)
			{
				return;
			}
			for (int i = 0; i < danglers.Length; i++)
			{
				float t = Vector2.Dot(danglers[i].pos - chunk.lastPos, chunk.pos - chunk.lastPos) / (chunk.pos - chunk.lastPos).sqrMagnitude;
				Vector2 vector = Vector2.Lerp(chunk.lastPos, chunk.pos, t);
				if (float.IsNaN(vector.x) || float.IsInfinity(vector.x) || float.IsNaN(vector.y) || float.IsInfinity(vector.y))
				{
					vector = chunk.pos;
				}
				float magnitude = (danglers[i].pos - vector).magnitude;
				float num = Mathf.InverseLerp(chunk.rad * 1.5f, chunk.rad * 0.5f, magnitude) * Custom.LerpMap(chunk.vel.magnitude, 1f, 80f, 0f, 0.5f);
				danglers[i].chunkSpin = Mathf.Lerp(danglers[i].chunkSpin, chunk.vel.x * 4f, Mathf.Sqrt(num));
				danglers[i].vel = Vector2.Lerp(danglers[i].vel, chunk.vel, num);
			}
		}
	}

	public class Bulb : BranchCap
	{
		public struct CircleData
		{
			public Vector2 offset;

			public float radius;

			public bool eye;

			public CircleData(Vector2 offset, float radius, bool eye)
			{
				this.offset = offset;
				this.radius = radius;
				this.eye = eye;
			}
		}

		public CircleData[] circles;

		public override int TotalSprites => circles.Length;

		public Bulb(PlateTree tree, Vector2 localPos, float tilt, float handleRad, float attachment, float size)
			: base(tree, localPos, tilt, handleRad, attachment, size)
		{
			List<CircleData> list = new List<CircleData>();
			int num = (int)(size / 2f) + UnityEngine.Random.Range(5, 8);
			for (int i = 0; i < num; i++)
			{
				float num2 = 1f - (float)i / (float)(num - 1);
				Vector2 offset = new Vector2(UnityEngine.Random.Range(-1f, 1f) * 3f * num2, (float)i * 4f);
				float radius = Mathf.Lerp(UnityEngine.Random.value, num2, UnityEngine.Random.value) * 4f + 1f;
				list.Add(new CircleData(offset, radius, eye: false));
			}
			for (int j = 0; j < num; j++)
			{
				float num3 = 1f - (float)j / (float)(num - 1);
				if (UnityEngine.Random.value < 0.6f * num3)
				{
					float radius2 = list[j].radius * Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, num3)));
					list.Add(new CircleData(list[j].offset, radius2, eye: true));
				}
			}
			circles = list.ToArray();
		}

		public override void Update()
		{
			base.Update();
			int clock = tree.room.game.clock;
			pos += new Vector2(Mathf.PerlinNoise((float)clock * 0.002f, attachment * 20f) * 10f - 5f, Mathf.PerlinNoise((float)clock * 0.002f, attachment * 20f + 5f) * 10f - 5f);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite)
		{
			for (int i = 0; i < circles.Length; i++)
			{
				sLeaser.sprites[firstSprite + i] = new FSprite("Circle20", quadType: false);
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			Vector2 cB = vector + Vector2.Lerp(base.LastHandleVec, base.HandleVec, timeStacker);
			Vector2 vector2 = tree.TrunkPos(attachment, timeStacker);
			Vector2 cA = vector2 + tree.TrunkDir(attachment, timeStacker) * (Vector2.Distance(vector2, vector) * 0.25f);
			for (int i = 0; i < circles.Length; i++)
			{
				float num = 1f - circles[i].offset.y / Vector2.Distance(vector2, vector);
				sLeaser.sprites[firstSprite + i].SetPosition(Custom.Bezier(vector2, cA, vector, cB, num) + Custom.PerpendicularVector(Custom.BezierDerivative(vector2, cA, vector, cB, num)) * circles[i].offset.x - camPos);
				sLeaser.sprites[firstSprite + i].scale = circles[i].radius / 10f;
				sLeaser.sprites[firstSprite + i].color = (circles[i].eye ? tree.effectColor : Color.Lerp(tree.limbColor, tree.effectColor, BranchGradient(num)));
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int firstSprite)
		{
			for (int i = 0; i < circles.Length; i++)
			{
				sLeaser.sprites[firstSprite + i].color = tree.effectColor;
			}
		}

		public override bool NeedsNewSprites(RoomCamera.SpriteLeaser sLeaser, int firstSprite)
		{
			for (int i = 0; i < circles.Length; i++)
			{
				if (sLeaser.sprites[firstSprite + i].element.name != "Circle20")
				{
					return true;
				}
			}
			return false;
		}

		public override float BranchGradient(float t)
		{
			return t * 0.5f;
		}
	}

	public struct Root
	{
		public Vector2 pos;

		public float rotation;

		public float scale;

		public int sprite;
	}

	public struct PlateDangler
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float connectionRad;

		public Color color;

		public Color lastColor;

		public float spin;

		public float lastSpin;

		public float chunkSpin;

		public bool dead;
	}

	public enum Variant
	{
		Plate,
		Rot
	}

	public PlacedObject pObj;

	public BranchCap[] branchCaps;

	public Root[] roots;

	public Vector2 trunkHandleVec;

	public Vector2 topHandleVec;

	public Variant variant;

	public int segmentsPerLimb = 15;

	public int trunkSprite;

	public int firstRootSprite;

	public int firstLimbSprite;

	public int totalSprites;

	public Color limbColor;

	public Color effectColor;

	public Color metalColor;

	public bool culled;

	public PlacedObject.ResizableObjectData Data => (PlacedObject.ResizableObjectData)pObj.data;

	public Rect Bounds => GetBounds(pos, Data.handlePos.magnitude);

	public bool ShouldBeCulled
	{
		get
		{
			if (room != null && room.cameraPositions.Length > 2)
			{
				return !room.game.cameras[0].RectCurrentlyVisible(Bounds, culled ? 50f : 100f, widescreen: true);
			}
			return false;
		}
	}

	public PlateTree(PlacedObject pObj, Room room, Variant variant)
	{
		this.variant = variant;
		this.pObj = pObj;
		base.room = room;
		Reset();
	}

	public void Reset()
	{
		pos = pObj.pos;
		lastPos = pObj.pos;
		PlacedObject.ResizableObjectData data = Data;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((int)(data.Rad + Mathf.Abs(pObj.pos.x)));
		float num = Mathf.Min(20f, data.Rad / 10f) * UnityEngine.Random.Range(0.75f, 1.25f);
		branchCaps = new BranchCap[4 + (int)(UnityEngine.Random.Range(1f, 1.5f) * data.Rad / 40f) * 2];
		for (int i = 0; i < branchCaps.Length; i++)
		{
			float num2 = ((i % 2 == 0) ? (-1f) : 1f);
			float num3 = (float)(i / 2) / (float)(branchCaps.Length / 2 - 1);
			float num4 = UnityEngine.Random.Range(0.15f, 1f) * num2;
			float attachment = Mathf.Lerp(0.15f, 1f, num3);
			Vector2 localPos = new Vector2(num4, 1f) * (UnityEngine.Random.Range(0.85f, 1f) * (3f * num3 * (1f - num3) + 0.25f) * data.Rad * 0.35f);
			float handleRad = UnityEngine.Random.Range(0.2f, 0.25f) * localPos.magnitude;
			if (variant == Variant.Plate)
			{
				float size = num * (1.5f * num3 * (1f - num3) + 0.5f);
				int danglers = ((UnityEngine.Random.value < 0.4f) ? UnityEngine.Random.Range(1, Mathf.CeilToInt(localPos.magnitude / 8f)) : 0);
				branchCaps[i] = new Plate(this, localPos, num4, handleRad, attachment, size, danglers);
			}
			else if (variant == Variant.Rot)
			{
				float size2 = 4f + 8f * num3 * (1f - num3);
				branchCaps[i] = new Bulb(this, localPos, num4, handleRad, attachment, size2);
			}
		}
		Array.Sort(branchCaps, (BranchCap l, BranchCap r) => Mathf.Abs(r.tilt).CompareTo(Mathf.Abs(l.tilt)));
		Vector2 vector = new Vector2(0f, 0f);
		for (int j = -2; j <= 2; j++)
		{
			for (int k = -2; k <= 2; k++)
			{
				if (room.GetTile(room.GetTilePosition(pObj.pos) + new IntVector2(k, j)).Solid)
				{
					vector -= new Vector2(k, j) / (k * k + j * j);
				}
			}
		}
		if (vector == Vector2.zero)
		{
			vector = Vector2.up;
		}
		else
		{
			vector.Normalize();
		}
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		trunkHandleVec = vector * (data.Rad * 0.5f);
		topHandleVec = -data.handlePos * 0.5f + data.Rad * UnityEngine.Random.Range(-0.5f, 0.5f) * vector2;
		float num5 = Mathf.Min(data.Rad * 0.25f, 40f);
		roots = new Root[Mathf.CeilToInt(num5 / 2.5f)];
		for (int m = 0; m < roots.Length; m++)
		{
			float num6 = (float)m / ((float)roots.Length - 1f);
			roots[m].pos = pObj.pos + Mathf.Lerp(0f - num5, num5, Custom.PushFromHalf(num6, 1.5f)) * vector2 + Custom.RNV() * (UnityEngine.Random.value * 5f);
			roots[m].rotation = Custom.VecToDeg(vector) + UnityEngine.Random.Range(-30f, 30f);
			roots[m].scale = UnityEngine.Random.Range(1.5f, 1.75f) * (num6 * (1f - num6) * 2f + 0.5f) * num5 / 40f;
			roots[m].sprite = UnityEngine.Random.Range(0, 6);
			Vector2? vector3 = SharedPhysics.ExactTerrainRayTracePos(room, roots[m].pos + vector * 40f, roots[m].pos - vector * 60f);
			if (!vector3.HasValue)
			{
				roots[m].scale = 0f;
			}
			else
			{
				roots[m].pos = Vector2.Lerp(roots[m].pos, vector3.Value, Mathf.Abs(num6 * 2f - 1f));
			}
		}
		UnityEngine.Random.state = state;
		int num7 = (firstLimbSprite = (firstRootSprite = (trunkSprite = 0) + 1) + roots.Length) + (branchCaps.Sum((BranchCap cap) => cap.TotalSprites) + branchCaps.Length);
		totalSprites = num7;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[trunkSprite] = TriangleMesh.MakeLongMeshAtlased(segmentsPerLimb * 2, pointyTip: false, customColor: false);
		for (int i = 0; i < roots.Length; i++)
		{
			sLeaser.sprites[firstRootSprite + i] = new FSprite("RootBall" + roots[i].sprite, quadType: false)
			{
				rotation = roots[i].rotation,
				scaleX = roots[i].scale,
				scaleY = roots[i].scale * 1.25f,
				anchorY = 0.75f
			};
		}
		int num = firstLimbSprite;
		for (int j = 0; j < branchCaps.Length; j++)
		{
			sLeaser.sprites[num] = TriangleMesh.MakeLongMeshAtlased(segmentsPerLimb, pointyTip: false, customColor: true);
			num++;
			branchCaps[j].InitiateSprites(sLeaser, rCam, num);
			num += branchCaps[j].TotalSprites;
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		bool flag = sLeaser.sprites.Length != totalSprites;
		if (!flag)
		{
			int num = firstLimbSprite;
			BranchCap[] array = branchCaps;
			foreach (BranchCap branchCap in array)
			{
				num++;
				if (branchCap.NeedsNewSprites(sLeaser, num))
				{
					flag = true;
					break;
				}
				num += branchCap.TotalSprites;
			}
		}
		if (flag)
		{
			FSprite[] sprites = sLeaser.sprites;
			for (int i = 0; i < sprites.Length; i++)
			{
				sprites[i].RemoveFromContainer();
			}
			InitiateSprites(sLeaser, rCam);
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		bool flag2 = culled;
		culled = ShouldBeCulled;
		if (culled != flag2)
		{
			FSprite[] sprites = sLeaser.sprites;
			for (int i = 0; i < sprites.Length; i++)
			{
				sprites[i].isVisible = !culled;
			}
		}
		if (culled)
		{
			return;
		}
		TriangleMesh mesh = (TriangleMesh)sLeaser.sprites[0];
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		Vector2 cA = vector + trunkHandleVec;
		Vector2 vector2 = vector + Data.handlePos;
		Vector2 cB = vector2 + topHandleVec;
		DrawBezier(mesh, vector, cA, vector2, cB, -1, camPos);
		for (int j = 0; j < roots.Length; j++)
		{
			sLeaser.sprites[firstRootSprite + j].SetPosition(roots[j].pos - camPos);
		}
		int num2 = firstLimbSprite;
		for (int k = 0; k < branchCaps.Length; k++)
		{
			BranchCap branchCap2 = branchCaps[k];
			TriangleMesh mesh2 = (TriangleMesh)sLeaser.sprites[num2];
			Vector2 vector3 = Vector2.Lerp(branchCap2.lastPos, branchCap2.pos, timeStacker);
			Vector2 cB2 = vector3 + Vector2.Lerp(branchCap2.LastHandleVec, branchCap2.HandleVec, timeStacker);
			Vector2 vector4 = TrunkPos(branchCap2.attachment, timeStacker);
			Vector2 cA2 = vector4 + TrunkDir(branchCap2.attachment, timeStacker) * (Vector2.Distance(vector4, vector3) * 0.25f);
			if (k > 0)
			{
				cA2 += (vector3 - vector4) * 0.5f;
			}
			DrawBezier(mesh2, vector4, cA2, vector3, cB2, k, camPos);
			num2++;
			branchCap2.DrawSprites(sLeaser, rCam, timeStacker, camPos, num2);
			num2 += branchCap2.TotalSprites;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		limbColor = palette.texture.GetPixel(0, 3);
		effectColor = palette.texture.GetPixel(31, 3);
		metalColor = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), palette.blackColor, 0.25f);
		sLeaser.sprites[0].color = limbColor;
		for (int i = 0; i < roots.Length; i++)
		{
			sLeaser.sprites[firstRootSprite + i].color = limbColor;
		}
		int num = firstLimbSprite;
		for (int j = 0; j < branchCaps.Length; j++)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[num] as TriangleMesh;
			for (int k = 0; k < triangleMesh.verticeColors.Length; k += 2)
			{
				float t = (float)k / (float)(triangleMesh.verticeColors.Length - 1);
				triangleMesh.verticeColors[k] = (triangleMesh.verticeColors[k + 1] = Color.Lerp(limbColor, effectColor, branchCaps[j].BranchGradient(t)));
			}
			num++;
			branchCaps[j].ApplyPalette(sLeaser, rCam, palette, num);
			num += branchCaps[j].TotalSprites;
		}
	}

	public static Rect GetBounds(Vector2 pos, float rad)
	{
		return new Rect(pos.x - rad, pos.y - rad, rad + rad, rad + rad);
	}

	public float TrunkProfile(float f)
	{
		return Mathf.Pow(1f - f, 4f) * 20f + Mathf.Pow(f, 6f) * 2f * 0.5f + 3f * Mathf.PerlinNoise(f * 20f, 0f);
	}

	public float BranchProfile(float f, int i)
	{
		if (f >= 1f)
		{
			return 0f;
		}
		return Mathf.Pow(1f - f, 4f) * (TrunkProfile(branchCaps[i].attachment) - 0.5f) + Mathf.Pow(f, 6f) * branchCaps[i].size * 0.25f + 0.5f + 2f * Mathf.PerlinNoise(f * 10f, i);
	}

	public Vector2 TrunkPos(float f, float timeStacker)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		Vector2 cA = vector + trunkHandleVec;
		Vector2 vector2 = vector + Data.handlePos;
		Vector2 cB = vector2 + topHandleVec;
		return Custom.Bezier(vector, cA, vector2, cB, f);
	}

	public Vector2 TrunkDir(float f, float timeStacker)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		Vector2 cA = vector + trunkHandleVec;
		Vector2 vector2 = vector + Data.handlePos;
		Vector2 cB = vector2 + topHandleVec;
		return Custom.BezierDerivative(vector, cA, vector2, cB, f).normalized;
	}

	public void DrawBezier(TriangleMesh mesh, Vector2 a, Vector2 cA, Vector2 b, Vector2 cB, int index, Vector2 camPos)
	{
		int num = mesh.vertices.Length / 2;
		for (int i = 0; i < num; i++)
		{
			float f = (float)i / ((float)num - 1f);
			Vector2 vector = Custom.Bezier(a, cA, b, cB, f);
			Vector2 vector2 = Custom.PerpendicularVector(Custom.BezierDerivative(a, cA, b, cB, f));
			float num2 = ((index == -1) ? TrunkProfile(f) : BranchProfile(f, index));
			mesh.MoveVertice(i * 2, vector + vector2 * num2 - camPos);
			mesh.MoveVertice(i * 2 + 1, vector - vector2 * num2 - camPos);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (ModManager.Watcher && variant == Variant.Plate)
		{
			bool flag = false;
			float effectAmount = room.roomSettings.GetEffectAmount(WatcherEnums.RoomEffectType.SentientRotInfection);
			if (effectAmount == 1f)
			{
				flag = true;
			}
			else if (effectAmount >= 0.5f)
			{
				flag = (int)(Data.Rad + Mathf.Abs(pObj.pos.x)) % 2 == 0;
			}
			if (flag)
			{
				for (int i = 0; i < room.roomSettings.ambientSounds.Count; i++)
				{
					AmbientSound ambientSound = room.roomSettings.ambientSounds[i];
					if (ambientSound.sample.ToLowerInvariant().StartsWith("rwtw_platetree") && ambientSound.type == AmbientSound.Type.Spot && Vector2.Distance((ambientSound as SpotSound).pos, pos) <= 400f)
					{
						ambientSound.volume = 0f;
					}
				}
				variant = Variant.Rot;
				Reset();
			}
		}
		pos = pObj.pos;
		BranchCap[] array = branchCaps;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Update();
		}
		if (room.ViewedByAnyCamera(Bounds, 0f))
		{
			ReactToObjects();
		}
	}

	public void ReactToObjects()
	{
		Rect bounds = Bounds;
		List<PhysicalObject>[] physicalObjects = room.physicalObjects;
		for (int i = 0; i < physicalObjects.Length; i++)
		{
			foreach (PhysicalObject item in physicalObjects[i])
			{
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					if (bounds.Overlaps(GetBounds(bodyChunk.pos, bodyChunk.rad + bodyChunk.vel.magnitude)))
					{
						BranchCap[] array = branchCaps;
						for (int k = 0; k < array.Length; k++)
						{
							array[k].ReactToChunk(bodyChunk);
						}
					}
				}
			}
		}
	}
}
