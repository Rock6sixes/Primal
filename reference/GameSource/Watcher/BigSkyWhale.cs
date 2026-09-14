using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BigSkyWhale : UpdatableAndDeletable, IDrawable
{
	public class Fin
	{
		public BigSkyWhale owner;

		public int firstSprite;

		public Vector2 onBodyPos;

		public SimpleSegment end;

		public float length;

		public bool playedSound;

		public Vector2 AnchorPos => owner.GetBodyPos(onBodyPos.x, onBodyPos.y, 1f);

		public Fin(BigSkyWhale owner, int firstSprite, Vector2 onBodyPos, float length)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			this.onBodyPos = onBodyPos;
			this.length = length;
			end.Reset(AnchorPos + Vector2.down * length);
		}

		public void Update()
		{
			end.lastPos = end.pos;
			Vector2 anchorPos = AnchorPos;
			Vector2 vector = end.pos - anchorPos;
			float magnitude = vector.magnitude;
			Vector2 vector2 = vector.normalized * ((length - magnitude) * 0.5f);
			end.vel += vector2;
			end.pos += vector2;
			float num = onBodyPos.x * (float)Math.PI * 12f - (float)owner.room.game.clock / 15f;
			float num2 = Mathf.Sin(num);
			end.vel.x += num2 * 0.75f * Mathf.Sin(onBodyPos.x * (float)Math.PI) * Mathf.Sign(owner.segments[0].pos.x - owner.segments[1].pos.x);
			end.vel.y -= 1f;
			end.vel *= 0.95f;
			end.pos += end.vel;
			if (num % ((float)Math.PI * 2f) < (float)Math.PI)
			{
				if (!playedSound)
				{
					playedSound = true;
					owner.room.PlaySound(WatcherEnums.WatcherSoundID.Sky_Whale_Fin_Flap, end.pos, 0.8f, Mathf.Lerp(1f, 2f, length / 75f));
				}
			}
			else
			{
				playedSound = false;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(8, pointyTip: false, customColor: true);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
			Vector3 bodyPos = owner.GetBodyPos(onBodyPos.x, onBodyPos.y, timeStacker);
			Vector2 vector = end.DrawPos(timeStacker);
			Vector2 vector2 = Custom.PerpendicularVector(bodyPos, vector);
			for (int i = 0; i < triangleMesh.vertices.Length; i += 2)
			{
				float num = (float)i / (float)(triangleMesh.vertices.Length - 2);
				Vector2 vector3 = Vector2.Lerp(bodyPos, vector, num);
				float num2 = Contour(num) * length / 4f;
				float depth = bodyPos.z - Mathf.Sign(onBodyPos.y) * 10f * num;
				triangleMesh.MoveVertice(i, rCam.ApplyDepth(vector3 + vector2 * num2, depth) - camPos);
				triangleMesh.MoveVertice(i + 1, rCam.ApplyDepth(vector3 - vector2 * num2, depth) - camPos);
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
			for (int i = 0; i < triangleMesh.verticeColors.Length; i += 2)
			{
				float num = (float)i / (float)(triangleMesh.verticeColors.Length - 1);
				Color b = Color.Lerp(new Color(0.5f, 0.4f, 0.3f), new Color(0.6f, 0.5f, 0.3f), Mathf.Pow(num, 0.7f) * 0.5f);
				b = Color.Lerp(palette.blackColor, b, onBodyPos.y * 0.5f + 0.5f + num * 0.5f);
				b = Color.Lerp(b, palette.fogColor, (0.5f - onBodyPos.y * 0.5f) * palette.fogAmount);
				triangleMesh.verticeColors[i] = b;
				triangleMesh.verticeColors[i + 1] = b;
			}
			if (onBodyPos.x > 0f)
			{
				triangleMesh.verticeColors[0].a = 0f;
				triangleMesh.verticeColors[1].a = 0f;
			}
		}

		public float Contour(float t)
		{
			return 0.5f * Mathf.Pow(1f - t, 9f) + 2f * t * Mathf.Sqrt(1f - t);
		}
	}

	public Vector2 targetPos;

	public float targetSpeed = 7f;

	public List<Vector2> path;

	public int pathIndex;

	public float pathPointRadius = 40f;

	public float bodyRadius = 375f;

	public float scareRadius = 1200f;

	public float segDistance;

	public Vector2 targetOffset;

	public static float bodyLength = 4000f;

	public SimpleSegment[] segments;

	public int callDelay;

	public RectangularDynamicSoundLoop flybyLoop;

	public PositionedSoundEmitter callEmitter;

	public const int gridLength = 30;

	public const int mouthSegments = 40;

	public const float eyeWidth = 45f;

	public const float eyeHeight = 25f;

	public static Vector2 eyeOnBodyPos = new Vector2(0.15f, 0.248f);

	public static Vector2 firstGillOnBodyPos = new Vector2(0.15f, 0.4f);

	public const int gills = 4;

	public const int gillSegments = 8;

	public static Vector2 firstFinOnBodyPos = new Vector2(0.17f, 0.15f);

	public const int finsPerSide = 9;

	public const int baleenGridLength = 30;

	public const int baleenGridWidth = 3;

	public const float shadowLength = 1500f;

	public Fin[] fins;

	public Creature lookCreature;

	public int lookSwitchDelay;

	public Vector2 lastLookDir;

	public Vector2 lookDir;

	public int totalSprites;

	public int shadowSprite;

	public int backTailSprite;

	public int mouthSprite;

	public int backBaleenSprite;

	public int frontBaleenSprite;

	public int bodySprite;

	public int firstGillSprite;

	public int eyeSprite;

	public int pupilSprite;

	public int eyelidsSprite;

	public int firstWhiskerSprite;

	public int frontTailSprite;

	public float HalfCircumference => (float)Math.PI * bodyRadius;

	public Vector2 EyePos => GetBodyPos(eyeOnBodyPos.x, eyeOnBodyPos.y, 1f);

	public FloatRect SoundRect
	{
		get
		{
			float num = float.PositiveInfinity;
			float num2 = float.NegativeInfinity;
			float num3 = float.PositiveInfinity;
			float num4 = float.NegativeInfinity;
			SimpleSegment[] array = segments;
			for (int i = 0; i < array.Length; i++)
			{
				SimpleSegment simpleSegment = array[i];
				num = Mathf.Min(num, simpleSegment.pos.x);
				num2 = Mathf.Max(num2, simpleSegment.pos.x);
				num3 = Mathf.Min(num3, simpleSegment.pos.y);
				num4 = Mathf.Max(num4, simpleSegment.pos.y);
			}
			num = Mathf.Min(num + 500f, (num + num2) / 2f);
			num2 = Mathf.Max(num2 - 500f, num);
			return new FloatRect(num, num3, num2, num4);
		}
	}

	public int TailSprite(int i)
	{
		if (i != 0)
		{
			return frontTailSprite;
		}
		return backTailSprite;
	}

	public BigSkyWhale(List<Vector2> path)
	{
		this.path = path;
		segments = new SimpleSegment[40];
		segDistance = bodyLength / (float)segments.Length;
		targetOffset = new Vector2(0f, bodyRadius);
		if (path.Count > 0)
		{
			targetPos = path[0];
			Vector2 vector = ((path.Count <= 1) ? Vector2.right : (path[1] - path[0]).normalized);
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].Reset(targetPos + targetOffset + segDistance * (float)(-i) * vector);
			}
		}
		callDelay = UnityEngine.Random.Range(0, 400);
		fins = new Fin[18];
		shadowSprite = totalSprites;
		totalSprites++;
		backTailSprite = totalSprites;
		totalSprites++;
		int num = totalSprites;
		totalSprites += 9;
		mouthSprite = totalSprites;
		totalSprites++;
		backBaleenSprite = totalSprites;
		totalSprites++;
		frontBaleenSprite = totalSprites;
		totalSprites++;
		bodySprite = totalSprites;
		totalSprites++;
		firstGillSprite = totalSprites;
		totalSprites += 4;
		firstWhiskerSprite = totalSprites;
		totalSprites = totalSprites;
		eyeSprite = totalSprites;
		totalSprites++;
		pupilSprite = totalSprites;
		totalSprites++;
		eyelidsSprite = totalSprites;
		totalSprites++;
		int num2 = totalSprites;
		totalSprites += 9;
		frontTailSprite = totalSprites;
		totalSprites++;
		for (int j = 0; j < 9; j++)
		{
			float num3 = (float)j / 8f;
			Vector2 onBodyPos = firstFinOnBodyPos + new Vector2((float)j * 200f / bodyLength, 0f);
			float length = Mathf.Lerp(35f, 75f, Mathf.Sin(num3 * (float)Math.PI));
			fins[j] = new Fin(this, num2 + j, onBodyPos, length);
			onBodyPos.y = 0f - onBodyPos.y;
			fins[9 + j] = new Fin(this, num + j, onBodyPos, length);
		}
	}

	public bool MouthContains(Vector2 pos)
	{
		Vector2 pos2 = segments[0].pos;
		Vector2 normalized = (pos2 - segments[1].pos).normalized;
		Vector2 vector = Custom.PerpendicularVector(normalized);
		if (vector.y < 0f)
		{
			vector = -vector;
		}
		float num = Vector2.Dot(pos - pos2, normalized);
		float num2 = Vector2.Dot(pos - pos2, vector);
		float num3 = bodyRadius * BodyContour(Mathf.Clamp01((0f - num) / bodyLength));
		if (Mathf.Abs(num2) >= num3)
		{
			return false;
		}
		float num4 = 0f - (num2 + num3);
		if (num < num4)
		{
			return num > num4 - 40f;
		}
		return false;
	}

	public Vector2 Extrapolate(Vector2 a, Vector2 b, float x)
	{
		float t = (x - a.x) / (b.x - a.x);
		return Vector2.LerpUnclamped(a, b, t);
	}

	public float GetPathY(float x)
	{
		float num = Mathf.Sign(path[path.Count - 1].x - path[0].x);
		for (int i = 1; i < path.Count - 1; i++)
		{
			if (Mathf.Sign(path[i].x - x) == num)
			{
				return Extrapolate(path[i - 1], path[i], x).y;
			}
		}
		return Extrapolate(path[path.Count - 2], path[path.Count - 1], x).y;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastLookDir = lookDir;
		segments[0].lastPos = segments[0].pos;
		segments[0].pos = Vector2.Lerp(segments[0].pos, targetPos + targetOffset, 0.1f);
		segments[0].vel = segments[0].pos - segments[0].lastPos;
		if (flybyLoop == null)
		{
			flybyLoop = new RectangularDynamicSoundLoop(this, SoundRect, room);
			flybyLoop.sound = WatcherEnums.WatcherSoundID.Big_Sky_Whale_Flyby;
		}
		flybyLoop.Update();
		flybyLoop.Volume = 1f;
		flybyLoop.rect = SoundRect;
		if (flybyLoop.emitter is RectSoundEmitter rectSoundEmitter)
		{
			rectSoundEmitter.rect = flybyLoop.rect;
		}
		if (callDelay > 0)
		{
			callDelay--;
		}
		else
		{
			callDelay = 400 + UnityEngine.Random.Range(0, 200);
			callEmitter = new PositionedSoundEmitter(segments[0].pos, 1f, 1f);
			room.PlaySound(WatcherEnums.WatcherSoundID.Big_Sky_Whale_Call, callEmitter, loop: false, 1f, 1f, randomStartPosition: false);
		}
		if (callEmitter != null)
		{
			callEmitter.Update(eu);
			callEmitter.lastPos = callEmitter.pos;
			callEmitter.pos = segments[0].pos;
			if (callEmitter.slatedForDeletetion && !callEmitter.soundStillPlaying)
			{
				callEmitter = null;
			}
		}
		if (lookCreature != null && (lookCreature.slatedForDeletetion || lookCreature.room != room || Mathf.Abs(lookCreature.DangerPos.x - EyePos.x) > 1400f))
		{
			lookCreature = null;
		}
		if (lookSwitchDelay > 0)
		{
			lookSwitchDelay--;
		}
		if (lookSwitchDelay == 0 && room.abstractRoom.creatures.Count > 0)
		{
			Creature realizedCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)].realizedCreature;
			if (realizedCreature != null && room.VisualContact(EyePos, realizedCreature.DangerPos) && UnityEngine.Random.value < 1f + realizedCreature.VisibilityBonus && Mathf.Abs(realizedCreature.DangerPos.x - EyePos.x) < 1400f && (lookCreature == null || realizedCreature.DangerPos.y > lookCreature.DangerPos.y))
			{
				lookSwitchDelay = UnityEngine.Random.Range(20, 40);
				lookCreature = realizedCreature;
			}
		}
		Vector2 vector = new Vector2(Mathf.Sign(segments[0].pos.x - segments[segments.Length - 1].pos.x), -0.5f);
		if (lookCreature != null)
		{
			vector = Vector2.ClampMagnitude((lookCreature.DangerPos - EyePos) / 300f, 1f);
		}
		lastLookDir = lookDir;
		lookDir = Vector3.Slerp(lookDir, vector, 0.1f);
		for (int i = 1; i < segments.Length; i++)
		{
			float num = (float)i / (float)segments.Length;
			Vector2 vector2 = segments[i].pos - segments[i - 1].pos;
			float magnitude = vector2.magnitude;
			vector2 *= (segDistance - magnitude) / magnitude;
			segments[i].lastPos = segments[i].pos;
			segments[i].vel += vector2 * 0.5f;
			segments[i].pos += vector2 * 0.5f;
			segments[i - 1].vel -= vector2 * 0.5f;
			segments[i - 1].pos -= vector2 * 0.5f;
			float pathY = GetPathY(segments[i].pos.x);
			segments[i].vel.y += (pathY + targetOffset.y - segments[i].pos.y) / 300f;
			segments[i].vel.y += Mathf.Sin(num * (float)Math.PI * 6f - (float)room.game.clock / 15f) * 0.5f * Mathf.Sin(num * (float)Math.PI);
			segments[i].pos += segments[i].vel;
			segments[i].vel *= 0.95f;
		}
		Fin[] array = fins;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Update();
		}
		float num2 = Mathf.Min(segments[5].pos.x, segments[20].pos.x);
		float num3 = Mathf.Max(segments[5].pos.x, segments[20].pos.x);
		List<PhysicalObject>[] physicalObjects = room.physicalObjects;
		for (int j = 0; j < physicalObjects.Length; j++)
		{
			foreach (PhysicalObject item in physicalObjects[j])
			{
				bool flag = false;
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					float bellyY = GetBellyY(bodyChunk.pos.x);
					if (!float.IsInfinity(bellyY) && bodyChunk.pos.y < bellyY)
					{
						flag = true;
						break;
					}
				}
				bool flag2 = item.bodyChunks.Length != 0;
				bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk2 in bodyChunks)
				{
					float bellyY2 = GetBellyY(bodyChunk2.pos.x);
					if ((bodyChunk2.pos.y < bellyY2 || bodyChunk2.lastPos.y < bellyY2) && bodyChunk2.pos.y > bellyY2 - bodyChunk2.rad)
					{
						if (item is SkyWhale)
						{
							bodyChunk2.vel.y += (bellyY2 - bodyChunk2.rad - bodyChunk2.pos.y) / 5f;
						}
						else
						{
							bodyChunk2.MoveFromOutsideMyUpdate(eu, new Vector2(bodyChunk2.pos.x, bellyY2 - bodyChunk2.rad));
							bodyChunk2.vel.y = Mathf.Abs(bodyChunk2.vel.y) * -0.5f;
						}
					}
					else if ((bodyChunk2.pos.y > bellyY2 || bodyChunk2.lastPos.y > bellyY2) && bodyChunk2.pos.y < bellyY2 + bodyChunk2.rad + 15f && !flag)
					{
						bodyChunk2.MoveFromOutsideMyUpdate(eu, new Vector2(bodyChunk2.pos.x, bellyY2 + bodyChunk2.rad + 15f));
						bodyChunk2.vel.y = Mathf.Abs(bodyChunk2.vel.y) * 0.5f;
					}
					if (bodyChunk2.pos.y < bellyY2 || bodyChunk2.pos.y > bellyY2 + 200f || bodyChunk2.pos.x < num2 || bodyChunk2.pos.x > num3)
					{
						flag2 = false;
					}
				}
				if (flag2)
				{
					(item as Creature)?.Die();
					item.Destroy();
				}
			}
		}
		if (pathIndex >= path.Count)
		{
			Destroy();
			return;
		}
		targetPos = Vector2.MoveTowards(targetPos, path[pathIndex], targetSpeed);
		if (Vector2.Distance(targetPos, path[pathIndex]) <= pathPointRadius)
		{
			pathIndex++;
		}
	}

	public float GetBellyY(float x)
	{
		if (x < Mathf.Min(segments[0].pos.x, segments[segments.Length - 1].pos.x) || x > Mathf.Max(segments[0].pos.x, segments[segments.Length - 1].pos.x))
		{
			return float.PositiveInfinity;
		}
		float num = float.PositiveInfinity;
		for (int i = 0; i < segments.Length; i++)
		{
			float along = (float)i / (float)(segments.Length - 1);
			float num2 = x - segments[i].pos.x;
			float num3 = bodyRadius * BodyContour(along);
			if (Mathf.Abs(num2) < num3)
			{
				num = Mathf.Min(num, segments[i].pos.y - Mathf.Sqrt(num3 * num3 - num2 * num2));
			}
		}
		return num;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(29, segments.Length - 1), customColor: false);
		triangleMesh.shader = rCam.game.rainWorld.Shaders["BigSkyWhaleSkin"];
		sLeaser.sprites[bodySprite] = triangleMesh;
		AssignGridUVs(triangleMesh, 29, segments.Length - 1, Vector2.zero, Vector2.one);
		TriangleMesh triangleMesh2 = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(29, 39), customColor: false);
		triangleMesh2.shader = rCam.game.rainWorld.Shaders["BigSkyWhaleSkin"];
		sLeaser.sprites[mouthSprite] = triangleMesh2;
		AssignGridUVs(triangleMesh2, 29, 39, new Vector2(0f, -1f), new Vector2(39f / (float)(segments.Length - 1), 0f));
		sLeaser.sprites[eyeSprite] = new CustomFSprite("Futile_White");
		sLeaser.sprites[pupilSprite] = new FSprite("BigSkyWhalePupil");
		sLeaser.sprites[eyelidsSprite] = new CustomFSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["BigSkyWhaleEyelids"]
		};
		for (int i = 0; i < 4; i++)
		{
			TriangleMesh triangleMesh3 = TriangleMesh.MakeLongMesh(8, pointyTip: false, customColor: false);
			triangleMesh3.shader = rCam.game.rainWorld.Shaders["BigSkyWhaleGills"];
			sLeaser.sprites[firstGillSprite + i] = triangleMesh3;
		}
		Fin[] array = fins;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].InitiateSprites(sLeaser, rCam);
		}
		for (int k = 0; k < 2; k++)
		{
			sLeaser.sprites[TailSprite(k)] = TriangleMesh.MakeLongMesh(20, pointyTip: false, customColor: true);
		}
		TriangleMesh.Triangle[] tris = TriangleMesh.GridTriangles(29, 2);
		TriangleMesh triangleMesh4 = new TriangleMesh("Futile_White", tris, customColor: true);
		triangleMesh4.shader = rCam.game.rainWorld.Shaders["BigSkyWhaleBaleen"];
		sLeaser.sprites[frontBaleenSprite] = triangleMesh4;
		AssignGridUVs(triangleMesh4, 29, 2, new Vector2(1f, 0f), new Vector2(0f, 1f));
		TriangleMesh triangleMesh5 = new TriangleMesh("Futile_White", tris, customColor: true);
		triangleMesh5.shader = rCam.game.rainWorld.Shaders["BigSkyWhaleBaleen"];
		sLeaser.sprites[backBaleenSprite] = triangleMesh5;
		AssignGridUVs(triangleMesh5, 29, 2, new Vector2(0f, -1f), new Vector2(1f, 0f));
		TriangleMesh triangleMesh6 = TriangleMesh.MakeLongMesh(5, pointyTip: false, customColor: false);
		sLeaser.sprites[shadowSprite] = triangleMesh6;
		triangleMesh6.color = new Color(0.003921569f, 0f, 0f);
		AddToContainer(sLeaser, rCam, null);
	}

	public static void AssignGridUVs(TriangleMesh mesh, int length, int width, Vector2 bottomLeft, Vector2 topRight)
	{
		int num = 0;
		for (int i = 0; i <= length; i++)
		{
			float y = Mathf.Lerp(bottomLeft.y, topRight.y, (float)i / (float)length);
			for (int j = 0; j <= width; j++)
			{
				float x = Mathf.Lerp(bottomLeft.x, topRight.x, (float)j / (float)width);
				mesh.UVvertices[num].x = x;
				mesh.UVvertices[num].y = y;
				num++;
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[shadowSprite]);
		FContainer fContainer = rCam.ReturnFContainer("Background");
		for (int i = shadowSprite + 1; i < frontBaleenSprite; i++)
		{
			fContainer.AddChild(sLeaser.sprites[i]);
		}
		FContainer fContainer2 = rCam.ReturnFContainer("Water");
		for (int j = frontBaleenSprite; j < totalSprites; j++)
		{
			fContainer2.AddChild(sLeaser.sprites[j]);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[mouthSprite].color = RXUtils.GetColorFromHex(2434595u);
		CustomFSprite customFSprite = sLeaser.sprites[eyeSprite] as CustomFSprite;
		for (int i = 0; i < 4; i++)
		{
			customFSprite.verticeColors[i] = palette.blackColor;
		}
		sLeaser.sprites[pupilSprite].color = Color.Lerp(palette.blackColor, palette.fogColor, 0.3f);
		Fin[] array = fins;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].ApplyPalette(sLeaser, rCam, palette);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		TriangleMesh triangleMesh = sLeaser.sprites[bodySprite] as TriangleMesh;
		TriangleMesh triangleMesh2 = sLeaser.sprites[mouthSprite] as TriangleMesh;
		float num = Mathf.Sign(segments[0].DrawPos(timeStacker).x - segments[segments.Length - 1].DrawPos(timeStacker).x);
		for (int i = 0; i < segments.Length; i++)
		{
			float u = (float)i / (float)(segments.Length - 1);
			for (int j = 0; j < 30; j++)
			{
				float v = (float)j / 29f;
				Vector3 bodyPos = GetBodyPos(u, v, timeStacker);
				triangleMesh.MoveVertice(j * segments.Length + i, rCam.ApplyDepth(bodyPos, bodyPos.z) - camPos);
				if (i < 40)
				{
					triangleMesh2.MoveVertice((30 - j - 1) * 40 + i, rCam.ApplyDepth(bodyPos, 0f - bodyPos.z) - camPos);
				}
			}
		}
		TriangleMesh triangleMesh3 = sLeaser.sprites[shadowSprite] as TriangleMesh;
		for (int k = 0; k < triangleMesh3.vertices.Length; k += 2)
		{
			float num2 = (float)k / (float)(triangleMesh3.vertices.Length - 2);
			Vector2 vector = GetBodyPos(num2, 0.1f, timeStacker);
			triangleMesh3.MoveVertice(k, vector - camPos);
			triangleMesh3.MoveVertice(k + 1, vector + new Vector2(num * 1000f * (num2 * 2f - 1f), -1500f) - camPos);
		}
		CustomFSprite customFSprite = sLeaser.sprites[eyeSprite] as CustomFSprite;
		CustomFSprite customFSprite2 = sLeaser.sprites[eyelidsSprite] as CustomFSprite;
		for (int l = 0; l < 4; l++)
		{
			Vector2 vector2 = -Custom.eightDirections[l * 2 + 1].ToVector2();
			float num3 = eyeOnBodyPos.x + vector2.x * 45f / bodyLength;
			float num4 = eyeOnBodyPos.y + vector2.y * 25f / HalfCircumference;
			Vector3 bodyPos2 = GetBodyPos(num3, num4, timeStacker);
			Vector2 pos = rCam.ApplyDepth(bodyPos2, bodyPos2.z) - camPos;
			customFSprite2.MoveVertice(l, pos);
			customFSprite2.verticeColors[l] = new Color(num3, num4, 0f, 1f);
		}
		for (int m = 0; m < 4; m++)
		{
			Vector2 vector3 = Custom.fourDirections[m].ToVector2();
			float u2 = eyeOnBodyPos.x + vector3.x * 45f / bodyLength * 0.9f;
			float v2 = eyeOnBodyPos.y + vector3.y * 25f / HalfCircumference * 0.8f;
			Vector3 bodyPos3 = GetBodyPos(u2, v2, timeStacker);
			Vector2 pos2 = rCam.ApplyDepth(bodyPos3, bodyPos3.z) - camPos;
			customFSprite.MoveVertice(m, pos2);
		}
		Vector2 vector4 = Vector2.Lerp(lastLookDir, lookDir, timeStacker);
		float u3 = eyeOnBodyPos.x + vector4.x * 6f / bodyLength;
		float v3 = eyeOnBodyPos.y + vector4.y * 4f / HalfCircumference;
		Vector3 bodyPos4 = GetBodyPos(u3, v3, timeStacker);
		Vector2 position = rCam.ApplyDepth(bodyPos4, bodyPos4.z) - camPos;
		sLeaser.sprites[pupilSprite].SetPosition(position);
		sLeaser.sprites[pupilSprite].scaleY = 0.8f;
		for (int n = 0; n < 4; n++)
		{
			TriangleMesh triangleMesh4 = sLeaser.sprites[firstGillSprite + n] as TriangleMesh;
			float num5 = 300f - 60f * (float)Mathf.Abs(n - 1);
			for (int num6 = 0; num6 < triangleMesh4.vertices.Length; num6 += 2)
			{
				float num7 = (float)num6 / (float)(triangleMesh4.vertices.Length - 2);
				for (int num8 = 0; num8 < 2; num8++)
				{
					float num9 = firstGillOnBodyPos.y + (num7 * 2f - 1f) * num5 / HalfCircumference;
					float u4 = firstGillOnBodyPos.x + ((float)n * 100f + ((float)num8 + 0.7f) * Mathf.Sin(num7 * (float)Math.PI) * 25f * (num5 / 300f) + num9 * 450f) / bodyLength;
					Vector3 bodyPos5 = GetBodyPos(u4, num9, timeStacker);
					Vector2 pos3 = rCam.ApplyDepth(bodyPos5, bodyPos5.z) - camPos;
					triangleMesh4.MoveVertice(num6 + num8, pos3);
				}
			}
		}
		TriangleMesh triangleMesh5 = sLeaser.sprites[frontBaleenSprite] as TriangleMesh;
		TriangleMesh triangleMesh6 = sLeaser.sprites[backBaleenSprite] as TriangleMesh;
		for (int num10 = 0; num10 < 3; num10++)
		{
			Vector2 normalized = (segments[0].DrawPos(timeStacker) - (Vector2)GetBodyPos(0f, 0f, timeStacker)).normalized;
			for (int num11 = 0; num11 < 30; num11++)
			{
				float num12 = (float)num11 / 29f;
				float num13 = GetLipAlongBodyPos(num12) + 5f / bodyLength;
				switch (num10)
				{
				case 1:
					num13 -= 30f / bodyLength;
					break;
				case 2:
					num13 += 100f / bodyLength;
					break;
				}
				Vector3 bodyPos6 = GetBodyPos(num13, num12, timeStacker);
				if (num10 == 1)
				{
					float num14 = Mathf.Lerp(30f, 50f, num12) * Mathf.InverseLerp(1f, 0.8f, num12);
					bodyPos6 += (Vector3)(normalized * num14) * (1f - num12);
					bodyPos6.z *= 0.9f;
				}
				triangleMesh6.MoveVertice((30 - num11 - 1) * 3 + num10, rCam.ApplyDepth(bodyPos6, 0f - bodyPos6.z) - camPos);
				triangleMesh5.MoveVertice(num11 * 3 + (3 - num10 - 1), rCam.ApplyDepth(bodyPos6, bodyPos6.z) - camPos);
			}
		}
		Fin[] array = fins;
		for (int num15 = 0; num15 < array.Length; num15++)
		{
			array[num15].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		for (int num16 = 0; num16 < 2; num16++)
		{
			TriangleMesh triangleMesh7 = sLeaser.sprites[TailSprite(num16)] as TriangleMesh;
			float num17 = ((num16 == 0) ? (-1f) : 1f);
			Color a = new Color(0.12f, 0.12f, 0.12f);
			Color a2 = new Color(0.1f, 0.1f, 0.1f);
			RoomPalette currentPalette = rCam.currentPalette;
			for (int num18 = 0; num18 < triangleMesh7.vertices.Length; num18 += 2)
			{
				float num19 = (float)num18 / (float)(triangleMesh7.vertices.Length - 2);
				float num20 = Mathf.Pow(Mathf.Max(num19 * 2f - 1f, 0f), 4f) * 0.7f * num17 * 80f;
				float num21 = Mathf.Sqrt(num19) * (1f - 0.3f * Mathf.Pow(num19, 20f)) * num17 * 80f;
				float u5 = Mathf.Lerp(0.93f, 1.03f, num19);
				Vector3 bodyPos7 = GetBodyPos(u5, num17 * 0.5f, timeStacker);
				float num22 = bodyPos7.z + num20 * 0.3f;
				float num23 = bodyPos7.z + num21 * 0.3f;
				triangleMesh7.MoveVertice(num18, rCam.ApplyDepth((Vector2)bodyPos7 + Vector2.down * num20, num22) - camPos);
				triangleMesh7.MoveVertice(num18 + 1, rCam.ApplyDepth((Vector2)bodyPos7 + Vector2.down * num21, num23) - camPos);
				triangleMesh7.verticeColors[num18] = Color.Lerp(a, currentPalette.fogColor, (num22 / 80f + 0.5f) * currentPalette.fogAmount);
				triangleMesh7.verticeColors[num18 + 1] = Color.Lerp(a2, currentPalette.fogColor, (num23 / 80f + 0.5f) * currentPalette.fogAmount);
			}
		}
		if (base.slatedForDeletetion || rCam.room != room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public Vector2 GetSegmentDir(int segment, float timeStacker)
	{
		Vector2 vector = segments[(segment == segments.Length - 1) ? segment : (segment + 1)].DrawPos(timeStacker);
		return (segments[(segment == 0) ? segment : (segment - 1)].DrawPos(timeStacker) - vector).normalized;
	}

	public float BodyContour(float along)
	{
		return 1f - Mathf.Pow(along, 3f) - 0.2f * Mathf.Sin(along * (float)Math.PI) - 0.1f * Mathf.Pow(1f - along, 20f);
	}

	public Vector3 GetBodyPos(int segment, float v, float timeStacker)
	{
		float along = (float)segment / (float)(segments.Length - 1);
		float num = BodyContour(along);
		float num2 = bodyRadius * num;
		Vector2 vector = segments[segment].DrawPos(timeStacker);
		Vector2 segmentDir = GetSegmentDir(segment, timeStacker);
		Vector2 vector2 = Custom.PerpendicularVector(segmentDir);
		if (segmentDir.x > 0f)
		{
			vector2 = -vector2;
		}
		float z = Mathf.Sin(v * (float)Math.PI) * -80f * num;
		vector += vector2 * (num2 * Mathf.Cos(v * (float)Math.PI));
		return new Vector3(vector.x, vector.y, z);
	}

	public Vector3 GetBodyPos(float u, float v, float timeStacker)
	{
		float num = u * (float)(segments.Length - 1);
		int num2 = Mathf.Clamp((int)num, 0, segments.Length - 2);
		num -= (float)num2;
		Vector3 bodyPos = GetBodyPos(num2, v, timeStacker);
		Vector3 bodyPos2 = GetBodyPos(num2 + 1, v, timeStacker);
		return Vector3.LerpUnclamped(bodyPos, bodyPos2, num);
	}

	public float GetLipAlongBodyPos(float v)
	{
		float num = 0.5f - Mathf.Cos(v * (float)Math.PI) * 0.5f;
		return num * num / 4f;
	}

	public Vector3 GetLipPos(float v, float timeStacker)
	{
		return GetBodyPos(GetLipAlongBodyPos(v), v, timeStacker);
	}
}
