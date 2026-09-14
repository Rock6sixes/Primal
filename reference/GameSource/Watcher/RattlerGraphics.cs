using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RattlerGraphics : GraphicsModule
{
	public class ArmGraphic : RopeGraphic
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

		public RattlerGraphics owner;

		public Rattler.Arm arm;

		public float clawRoll;

		public CircleData[] circles;

		public int totalSprites;

		public int tentacleSprite;

		public int firstCircleSprite;

		public ArmGraphic(RattlerGraphics owner, Rattler.Arm arm, int firstSprite)
			: base(40)
		{
			this.owner = owner;
			this.arm = arm;
			airFricition = 0.8f;
			List<CircleData> list = new List<CircleData>();
			int num = UnityEngine.Random.Range(8, 11);
			for (int i = 0; i < num; i++)
			{
				float num2 = 1f - (float)i / (float)(num - 1);
				Vector2 offset = new Vector2(UnityEngine.Random.Range(-1f, 1f) * 3f * num2, (float)i * 3f);
				float num3 = Mathf.Lerp(UnityEngine.Random.value, num2, UnityEngine.Random.value) * 3.5f + 1.5f;
				if (i == 0)
				{
					num3 = Mathf.Lerp(num3, 5f, 0.5f);
				}
				list.Add(new CircleData(offset, num3, eye: false));
			}
			for (int j = 0; j < num; j++)
			{
				float num4 = 1f - (float)j / (float)(num - 1);
				if (UnityEngine.Random.value < 0.6f * num4)
				{
					float radius = list[j].radius * Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, num4)));
					list.Add(new CircleData(list[j].offset, radius, eye: true));
				}
			}
			circles = list.ToArray();
			tentacleSprite = firstSprite + totalSprites;
			totalSprites++;
			firstCircleSprite = firstSprite + totalSprites;
			totalSprites += list.Count;
			ResetSegments();
		}

		public override void Update()
		{
			for (int i = 0; i < arm.rope.TotalPositions; i++)
			{
				AddToPositionsList(i, arm.rope.GetPosition(i));
			}
			AddToPositionsList(arm.rope.TotalPositions, owner.drawHeadPos);
			AlignAndConnect(arm.rope.TotalPositions + 1);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[tentacleSprite] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: true);
			for (int i = 0; i < circles.Length; i++)
			{
				sLeaser.sprites[firstCircleSprite + i] = new FSprite("Circle20", quadType: false);
			}
			ResetSegments();
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = segments[0].pos;
			float a = 1f;
			float num = 0f;
			TriangleMesh triangleMesh = sLeaser.sprites[tentacleSprite] as TriangleMesh;
			for (int i = 0; i < segments.Length; i++)
			{
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				float num2 = 2f;
				triangleMesh.MoveVertice(i * 4, vector - vector3 * Mathf.Lerp(a, num2, 0.5f) - camPos);
				triangleMesh.MoveVertice(i * 4 + 1, vector + vector3 * Mathf.Lerp(a, num2, 0.5f) - camPos);
				triangleMesh.MoveVertice(i * 4 + 2, vector2 - vector3 * num2 - camPos);
				triangleMesh.MoveVertice(i * 4 + 3, vector2 + vector3 * num2 - camPos);
				num += Vector2.Distance(vector, vector2);
				vector = vector2;
				a = num2;
			}
			for (int j = 0; j < circles.Length; j++)
			{
				Vector2 offset = circles[j].offset;
				offset.y = Mathf.Clamp01(offset.y / Mathf.Max(num, 1f));
				sLeaser.sprites[firstCircleSprite + j].scale = circles[j].radius / 10f;
				sLeaser.sprites[firstCircleSprite + j].SetPosition(OnTubePos(offset, timeStacker) - camPos);
			}
		}

		public Vector2 OnTubePos(Vector2 pos, float timeStacker)
		{
			Vector2 p = OneDimensionalTubePos(pos.y - 1f / (float)segments.Length, timeStacker);
			Vector2 p2 = OneDimensionalTubePos(pos.y + 1f / (float)segments.Length, timeStacker);
			return OneDimensionalTubePos(pos.y, timeStacker) + Custom.PerpendicularVector(Custom.DirVec(p, p2)) * pos.x;
		}

		public Vector2 OneDimensionalTubePos(float floatPos, float timeStacker)
		{
			int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (float)(segments.Length - 1)), 0, segments.Length - 1);
			int num2 = Custom.IntClamp(num + 1, 0, segments.Length - 1);
			float t = Mathf.InverseLerp(num, num2, floatPos * (float)(segments.Length - 1));
			return Vector2.Lerp(Vector2.Lerp(segments[num].lastPos, segments[num2].lastPos, t), Vector2.Lerp(segments[num].pos, segments[num2].pos, t), timeStacker);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[tentacleSprite] as TriangleMesh;
			for (int i = 0; i < triangleMesh.verticeColors.Length; i += 2)
			{
				float f = 1f - (float)i / (float)(triangleMesh.verticeColors.Length - 2);
				triangleMesh.verticeColors[i] = (triangleMesh.verticeColors[i + 1] = Color.Lerp(palette.blackColor, owner.effectColor, Mathf.Pow(f, 2f) * 0.75f));
			}
			for (int j = 0; j < circles.Length; j++)
			{
				float num = (float)j / (float)(circles.Length - 1);
				sLeaser.sprites[firstCircleSprite + j].color = Color.Lerp(palette.blackColor, owner.effectColor, circles[j].eye ? 1f : (0.75f - 0.1f * num));
			}
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = 0; i < totalSprites; i++)
			{
				newContatiner.AddChild(sLeaser.sprites[tentacleSprite + i]);
			}
		}

		public void ResetSegments()
		{
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].pos = owner.Rattler.firstChunk.pos;
				segments[i].lastPos = segments[i].pos;
			}
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel *= 0f;
			if (owner.Rattler.room.GetTile(smoothedGoalPos).Solid && !owner.Rattler.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.Rattler.room.TileRect(owner.Rattler.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
				segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
			}
			else
			{
				segments[segment].pos = smoothedGoalPos;
			}
		}
	}

	public class Bone
	{
		public RattlerGraphics owner;

		public float pos;

		public int dots;

		public float halfWidth;

		public float halfHeight;

		public bool smallDots;

		public bool[] blackDots;

		public int TotalSprites => 2 + dots;

		public Bone(RattlerGraphics owner, float pos, float width, int dots, bool smallDots)
		{
			this.owner = owner;
			this.pos = pos;
			this.dots = dots;
			this.smallDots = smallDots;
			halfWidth = width / 2f;
			halfHeight = Mathf.Lerp(halfWidth * 0.3f, 5.4f, 0.2f);
			blackDots = new bool[dots];
			for (int i = 0; i < blackDots.Length; i++)
			{
				blackDots[i] = UnityEngine.Random.value < 0.05f;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMeshAtlased(5, pointyTip: false, customColor: false);
			sLeaser.sprites[firstSprite + 1] = TriangleMesh.MakeLongMeshAtlased(5, pointyTip: false, customColor: false);
			for (int i = 0; i < dots; i++)
			{
				sLeaser.sprites[firstSprite + 2 + i] = new FSprite(smallDots ? "JetFishEyeB" : "tinyStar", quadType: false);
			}
			for (int j = 0; j < 2; j++)
			{
				TriangleMesh triangleMesh = sLeaser.sprites[firstSprite + j] as TriangleMesh;
				for (int k = 0; k < triangleMesh.vertices.Length; k += 2)
				{
					float num = (float)k / (float)(triangleMesh.vertices.Length - 2);
					float num2 = 0.6f + 0.5f * (1f - Mathf.Sin(num * (float)Math.PI)) + 0.3f * Mathf.Max(Mathf.Sin(Mathf.InverseLerp(0f, 0.3f, num) * (float)Math.PI), Mathf.Sin(Mathf.InverseLerp(0.7f, 1f, num) * (float)Math.PI));
					if (k == 0 || k == triangleMesh.vertices.Length - 2)
					{
						num2 = 0.5f;
					}
					Vector2 vector = new Vector2(halfWidth * (num * 2f - 1f), 0f);
					Vector2 vector2 = new Vector2(0f, 1f) * (halfHeight * num2);
					triangleMesh.MoveVertice(k, vector + vector2);
					triangleMesh.MoveVertice(k + 1, vector - vector2);
				}
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int firstSprite)
		{
			float num = Custom.LerpMap(halfWidth, 19f, 9f, 0f, 0.2f);
			float t = Mathf.Lerp(owner.darkness, 1f, num);
			sLeaser.sprites[firstSprite].color = Color.Lerp(owner.darkColor, palette.blackColor, t);
			sLeaser.sprites[firstSprite + 1].color = Color.Lerp(owner.lightColor, palette.blackColor, t);
			for (int i = 0; i < dots; i++)
			{
				t = (blackDots[i] ? 0.9f : num);
				sLeaser.sprites[firstSprite + 2 + i].color = Color.Lerp(owner.effectColor, palette.blackColor, t);
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite)
		{
			Vector2 mouthPos = owner.GetMouthPos(timeStacker);
			Vector2 basePos = owner.GetBasePos(timeStacker);
			Vector2 vector = Vector2.Lerp(basePos, mouthPos, pos);
			Vector2 vector2 = Custom.DirVec(basePos, mouthPos);
			Vector2 vector3 = Custom.PerpendicularVector(vector2);
			float rotation = Custom.VecToDeg(vector2);
			sLeaser.sprites[firstSprite].SetPosition(vector - camPos);
			sLeaser.sprites[firstSprite].rotation = rotation;
			sLeaser.sprites[firstSprite + 1].SetPosition(vector + vector2 * (halfHeight * 0.2f) - camPos);
			sLeaser.sprites[firstSprite + 1].scale = 0.75f;
			sLeaser.sprites[firstSprite + 1].rotation = rotation;
			for (int i = 0; i < dots; i++)
			{
				float num = (float)(i + 1) / (float)(dots + 1) * 2f - 1f;
				sLeaser.sprites[firstSprite + 2 + i].SetPosition(vector + vector3 * (num * halfWidth) - camPos);
				sLeaser.sprites[firstSprite + 2 + i].rotation = rotation;
			}
		}
	}

	public class RootBlob
	{
		public RattlerGraphics owner;

		public Vector2 pos;

		public float radius;

		public int TotalSprites => 1;

		public RootBlob(RattlerGraphics owner, Vector2 pos, float radius)
		{
			this.owner = owner;
			this.pos = pos;
			this.radius = radius;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, int firstSprite)
		{
			sLeaser.sprites[firstSprite] = new FSprite("Futile_White")
			{
				shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
				scale = radius / 8f
			};
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette, int firstSprite)
		{
			sLeaser.sprites[firstSprite].color = palette.blackColor;
			sLeaser.sprites[firstSprite].alpha = 0.3f;
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, int firstSprite)
		{
			Vector2 vector = owner.Rattler.attachmentPoint + owner.Rattler.attachmentDir * pos.y + Custom.PerpendicularVector(owner.Rattler.attachmentDir) * pos.x;
			sLeaser.sprites[firstSprite].SetPosition(vector - camPos);
		}
	}

	public Bone[] bones;

	public RootBlob[] roots;

	public ArmGraphic[] armGraphics;

	public int teeth;

	public int stalkSegments = 3;

	public int bodySegments = 8;

	public int bodySprite;

	public int firstRootSprite;

	public int firstToothSprite;

	public int firstBoneSprite;

	public int totalSprites;

	public Color lightColor = RXUtils.GetColorFromHex(13548218u);

	public Color darkColor = RXUtils.GetColorFromHex(9929600u);

	public Color effectColor = RXUtils.GetColorFromHex(9306316u);

	public float darkness;

	public Vector2 drawBasePos;

	public Vector2 lastDrawBasePos;

	public Vector2 drawHeadPos;

	public Vector2 lastDrawHeadPos;

	public float rattleAngle;

	public Rattler Rattler => base.owner as Rattler;

	public RattlerGraphics(PhysicalObject ow)
		: base(ow, internalContainers: true)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
		drawBasePos = Rattler.bodyChunks[1].pos;
		drawHeadPos = Rattler.bodyChunks[0].pos;
		lastDrawBasePos = drawBasePos;
		lastDrawHeadPos = drawHeadPos;
		teeth = 5;
		if (UnityEngine.Random.value < 0.3f)
		{
			teeth = ((UnityEngine.Random.value < 0.5f) ? 4 : 6);
		}
		float num = Mathf.Pow(UnityEngine.Random.value, 2f);
		bones = new Bone[5];
		float[] array = new float[5] { 21f, 29f, 33f, 38f, 33f };
		for (int i = 0; i < array.Length; i++)
		{
			array[i] *= 2f * Custom.ClampedRandomVariation(0.5f, 0.05f, num * 0.5f);
		}
		int num2 = 2;
		if (UnityEngine.Random.value < 0.1f + num * 0.1f)
		{
			num2 = 3;
		}
		else if (UnityEngine.Random.value < 0.05f + num * 0.1f)
		{
			num2 = 1;
		}
		bool flag = num2 > 1 && UnityEngine.Random.value < ((num2 == 3) ? 0.6f : 0.2f);
		for (int j = 0; j < bones.Length; j++)
		{
			float pos = (float)j / (float)(bones.Length - 1);
			int num3 = num2;
			if (flag && (j == 0 || j == bones.Length - 1))
			{
				num3--;
			}
			bones[j] = new Bone(this, pos, array[j], num3, j >= 2 && j < bones.Length - 1);
		}
		Array.Sort(bones, (Bone a, Bone b) => a.halfWidth.CompareTo(b.halfWidth));
		roots = new RootBlob[UnityEngine.Random.Range(7, 12)];
		for (int k = 0; k < roots.Length; k++)
		{
			float num4 = (float)k / (float)(roots.Length - 1);
			float radius = Mathf.Lerp(3f, 10f, num4);
			float x = UnityEngine.Random.Range(-15f, 15f) * (1f - num4 * 0.8f);
			float y = UnityEngine.Random.Range(-3f, 3f);
			roots[k] = new RootBlob(this, new Vector2(x, y), radius);
		}
		armGraphics = new ArmGraphic[Rattler.arms.Length];
		for (int l = 0; l < armGraphics.Length; l++)
		{
			armGraphics[l] = new ArmGraphic(this, Rattler.arms[l], totalSprites);
			totalSprites += armGraphics[l].totalSprites;
		}
		bodySprite = totalSprites;
		totalSprites++;
		firstRootSprite = totalSprites;
		totalSprites += roots.Sum((RootBlob root) => root.TotalSprites);
		firstToothSprite = totalSprites;
		totalSprites += teeth;
		firstBoneSprite = totalSprites;
		totalSprites += bones.Sum((Bone bone) => bone.TotalSprites);
		UnityEngine.Random.state = state;
	}

	public Vector2 GetMouthPos(float timeStacker)
	{
		Vector2 vector = Vector2.Lerp(lastDrawHeadPos, drawHeadPos, timeStacker);
		Vector2 vector2 = Custom.DirVec(Vector2.Lerp(lastDrawBasePos, drawBasePos, timeStacker), vector);
		return vector + vector2 * (base.owner.bodyChunks[0].rad - 4f);
	}

	public Vector2 GetBasePos(float timeStacker)
	{
		Vector2 p = Vector2.Lerp(lastDrawHeadPos, drawHeadPos, timeStacker);
		Vector2 vector = Vector2.Lerp(lastDrawBasePos, drawBasePos, timeStacker);
		Vector2 vector2 = Custom.DirVec(vector, p);
		return vector - vector2 * (base.owner.bodyChunks[1].rad - 4f);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.containers = new FContainer[armGraphics.Length];
		for (int i = 0; i < sLeaser.containers.Length; i++)
		{
			sLeaser.containers[i] = new FContainer();
		}
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[bodySprite] = TriangleMesh.MakeLongMeshAtlased(stalkSegments + bodySegments, pointyTip: false, customColor: false);
		int num = firstRootSprite;
		RootBlob[] array = roots;
		foreach (RootBlob rootBlob in array)
		{
			rootBlob.InitiateSprites(sLeaser, rCam, num);
			num += rootBlob.TotalSprites;
		}
		ArmGraphic[] array2 = armGraphics;
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].InitiateSprites(sLeaser, rCam);
		}
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
		{
			new TriangleMesh.Triangle(0, 1, 2)
		};
		for (int k = 0; k < teeth; k++)
		{
			TriangleMesh triangleMesh = new TriangleMesh("RainWorld_White", tris, customColor: false, atlasedImage: true);
			sLeaser.sprites[firstToothSprite + k] = triangleMesh;
			triangleMesh.vertices[0] = new Vector2(-2f, 0f);
			triangleMesh.vertices[1] = new Vector2(0f, 4.5f);
			triangleMesh.vertices[2] = new Vector2(2f, 0f);
		}
		num = firstBoneSprite;
		Bone[] array3 = bones;
		foreach (Bone bone in array3)
		{
			bone.InitiateSprites(sLeaser, rCam, num);
			num += bone.TotalSprites;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < armGraphics.Length; i++)
		{
			newContatiner.AddChild(sLeaser.containers[i]);
			armGraphics[i].AddToContainer(sLeaser, rCam, newContatiner);
		}
		newContatiner.AddChild(sLeaser.sprites[bodySprite]);
		int num = roots.Sum((RootBlob root) => root.TotalSprites);
		for (int j = 0; j < num; j++)
		{
			newContatiner.AddChild(sLeaser.sprites[firstRootSprite + j]);
		}
		for (int k = 0; k < teeth; k++)
		{
			newContatiner.AddChild(sLeaser.sprites[firstToothSprite + k]);
		}
		int num2 = bones.Sum((Bone bone) => bone.TotalSprites);
		for (int l = 0; l < num2; l++)
		{
			newContatiner.AddChild(sLeaser.sprites[firstBoneSprite + l]);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 mouthPos = GetMouthPos(timeStacker);
		Vector2 basePos = GetBasePos(timeStacker);
		Vector2 vector = Vector2.Lerp(base.owner.bodyChunks[2].lastPos, base.owner.bodyChunks[2].pos, timeStacker);
		Vector2 vector2 = Custom.DirVec(basePos, mouthPos);
		Vector2 vector3 = Custom.PerpendicularVector(vector2);
		Vector2 cB = basePos - vector2 * 10f;
		float num = rCam.room.Darkness((basePos + mouthPos) / 2f) * (1f - rCam.room.LightSourceExposure((basePos + mouthPos) / 2f));
		if (darkness != num)
		{
			darkness = num;
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		TriangleMesh triangleMesh = sLeaser.sprites[bodySprite] as TriangleMesh;
		Vector2 vector4 = Rattler.attachmentPoint;
		Vector2 vector5 = Rattler.attachmentDir;
		float num2 = 0f;
		for (int i = 0; i < stalkSegments + bodySegments; i++)
		{
			Vector2 vector6;
			Vector2 v;
			float num3;
			if (i < stalkSegments)
			{
				float f = (float)i / (float)stalkSegments;
				vector6 = Custom.Bezier(vector, vector + Rattler.attachmentDir, basePos, cB, f);
				v = Custom.BezierDerivative(vector, vector + Rattler.attachmentDir, basePos, cB, f).normalized;
				num3 = Mathf.Cos(-1.125f) * 6f;
			}
			else
			{
				float num4 = (float)(i - stalkSegments) / (float)(bodySegments - 1);
				vector6 = Vector2.Lerp(basePos, mouthPos + vector2 * 6f, num4);
				v = vector2;
				num3 = Mathf.Cos((num4 - 0.75f) * 1.5f) * 17f * Mathf.Pow(Mathf.InverseLerp(1f, 0.7f, num4), 0.2f);
			}
			Vector2 vector7 = Custom.PerpendicularVector(v);
			Vector2 vector8 = vector5 * ((num3 + num2) * 0.5f);
			Vector2 vector9 = vector7 * num3;
			triangleMesh.MoveVertice(i * 4, (vector4 + vector6) / 2f + vector8 - camPos);
			triangleMesh.MoveVertice(i * 4 + 1, (vector4 + vector6) / 2f - vector8 - camPos);
			triangleMesh.MoveVertice(i * 4 + 2, vector6 + vector9 - camPos);
			triangleMesh.MoveVertice(i * 4 + 3, vector6 - vector9 - camPos);
			vector4 = vector6;
			vector5 = vector7;
			num2 = num3;
		}
		int num5 = firstRootSprite;
		RootBlob[] array = roots;
		foreach (RootBlob rootBlob in array)
		{
			rootBlob.DrawSprites(sLeaser, rCam, timeStacker, camPos, num5);
			num5 += rootBlob.TotalSprites;
		}
		ArmGraphic[] array2 = armGraphics;
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		Vector2 p = mouthPos + vector2 * 10f;
		for (int k = 0; k < teeth; k++)
		{
			float num6 = (float)k / (float)(teeth - 1) * 2f - 1f;
			Vector2 vector10 = mouthPos + vector2 * 3f + vector3 * (num6 * 11f);
			sLeaser.sprites[firstToothSprite + k].SetPosition(vector10 + vector2 * Mathf.Abs(num6 * 2f) - camPos);
			sLeaser.sprites[firstToothSprite + k].rotation = Custom.AimFromOneVectorToAnother(vector10, p);
		}
		num5 = firstBoneSprite;
		Bone[] array3 = bones;
		foreach (Bone bone in array3)
		{
			bone.DrawSprites(sLeaser, rCam, timeStacker, camPos, num5);
			num5 += bone.TotalSprites;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[bodySprite].color = palette.blackColor;
		int num = firstRootSprite;
		RootBlob[] array = roots;
		foreach (RootBlob rootBlob in array)
		{
			rootBlob.ApplyPalette(sLeaser, rCam, palette, num);
			num += rootBlob.TotalSprites;
		}
		for (int j = 0; j < teeth; j++)
		{
			sLeaser.sprites[firstToothSprite + j].color = Color.Lerp(darkColor, palette.blackColor, darkness);
		}
		ArmGraphic[] array2 = armGraphics;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].ApplyPalette(sLeaser, rCam, palette);
		}
		num = firstBoneSprite;
		Bone[] array3 = bones;
		foreach (Bone bone in array3)
		{
			bone.ApplyPalette(sLeaser, rCam, palette, num);
			num += bone.TotalSprites;
		}
	}

	public override void Update()
	{
		base.Update();
		lastDrawBasePos = drawBasePos;
		lastDrawHeadPos = drawHeadPos;
		drawBasePos = Rattler.bodyChunks[1].pos;
		drawHeadPos = Rattler.bodyChunks[0].pos;
		float rattleIntensity = Rattler.rattleIntensity;
		if (rattleIntensity > 0f)
		{
			float num = Mathf.Lerp(1f, 1.75f, Mathf.Pow(rattleIntensity, 2f));
			rattleAngle += num * (float)Math.PI * 0.42f;
			rattleAngle %= (float)Math.PI * 2f;
			Vector2 pivot = Vector2.Lerp(drawBasePos, drawHeadPos, 0.25f);
			float degAng = rattleIntensity * UnityEngine.Random.Range(0.75f, 1f) * Mathf.Sin(rattleAngle) * 15f * Mathf.Lerp(1f, 0.75f, num);
			drawBasePos = Custom.RotateAroundVector(drawBasePos, pivot, degAng);
			drawHeadPos = Custom.RotateAroundVector(drawHeadPos, pivot, degAng);
		}
		else
		{
			rattleAngle = 0f;
		}
		ArmGraphic[] array = armGraphics;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Update();
		}
	}
}
