using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RatGraphics : GraphicsModule, ILookingAtCreatures, IMuddableGraphics
{
	public class Spine
	{
		public float alongBody;

		public float roll;

		public float stickUp;

		public float length;

		public int variant;

		public bool flip;

		public float fearStickUp;

		public float lastFearStickUp;

		public RatGraphics owner;

		public Spine(RatGraphics owner, float alongBody, float roll, float stickUp, float length)
		{
			this.owner = owner;
			this.alongBody = alongBody;
			this.roll = roll;
			this.stickUp = stickUp;
			this.length = length;
			variant = UnityEngine.Random.Range(0, 5);
			flip = UnityEngine.Random.value < 0.5f;
		}

		public Vector2 GetBasePos(Vector2 shoulder, Vector2 hip, float timeStacker)
		{
			Vector2 vector = Custom.PerpendicularVector((shoulder - hip).normalized);
			float f = roll - Mathf.Lerp(owner.lastGrabSide, owner.grabSide, timeStacker) * (float)Math.PI / 3f;
			return Vector2.Lerp(shoulder, hip, alongBody) + vector * (Mathf.Sin(f) * 2f);
		}

		public Vector2 GetEndPos(Vector2 shoulder, Vector2 hip, float timeStacker)
		{
			Vector2 normalized = (shoulder - hip).normalized;
			Vector2 vector = Custom.PerpendicularVector(normalized);
			float f = roll - Mathf.Lerp(owner.lastGrabSide, owner.grabSide, timeStacker) * (float)Math.PI / 3f;
			float f2 = stickUp * (1f + Mathf.Lerp(lastFearStickUp, fearStickUp, timeStacker) * 0.5f);
			return Vector2.Lerp(shoulder, hip, alongBody) + vector * (Mathf.Sin(f) * (length * Mathf.Sin(f2) + 2f)) - normalized * (length * Mathf.Cos(f2));
		}

		public void Update()
		{
			lastFearStickUp = fearStickUp;
			fearStickUp = Mathf.Lerp(fearStickUp, Mathf.Sqrt(owner.Rat.AI.CombinedFear), 0.1f);
		}
	}

	public CreatureLooker creatureLooker;

	public Limb[] limbs;

	public TailSegment[] tail;

	public Vector2 bodyDir;

	public float grabSide;

	public float lastGrabSide;

	public float[] bodyRads;

	public float breath;

	public float lastBreath;

	public Spine[] spines;

	public GenericBodyPart[] whiskers;

	public int whiskerTwitch;

	public float whiskerLength;

	public bool bigEyes;

	public int blink;

	public float coatDarkness;

	public Color coatColor;

	public Color headColor;

	public Color footColor;

	public Color tailColor;

	public Color blackColor;

	public float legLength = 14f;

	public int totalSprites;

	public int firstLegSprite;

	public int firstBodySprite;

	public int firstHeadSprite;

	public int firstSpineSprite;

	public int firstEyeSprite;

	public int tailSprite;

	public int firstWhiskerSprite;

	public Rat Rat => base.owner as Rat;

	public RatGraphics(PhysicalObject po)
		: base(po, internalContainers: false)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(Rat.abstractCreature.ID.RandomSeed);
		bigEyes = UnityEngine.Random.value < 0.05f;
		whiskerLength = Mathf.Lerp(20f, 35f, Rat.abstractCreature.personality.dominance);
		coatDarkness = 1f - Mathf.Pow(UnityEngine.Random.value, 2f) * 0.4f;
		coatColor = Color.Lerp(new Color(0.29f, 0.31f, 0.33f), new Color(0.42f, 0.26f, 0.07f), UnityEngine.Random.value);
		headColor = new HSLColor(Custom.WrappedRandomVariation(0f, 0.1f, 0.1f), Custom.ClampedRandomVariation(0.2f, 0.2f, 0.1f), Custom.ClampedRandomVariation(0.3f, 0.1f, 0.1f)).rgb;
		tailColor = headColor;
		creatureLooker = new CreatureLooker(this, Rat.AI.tracker, Rat, 0f, 30);
		limbs = new Limb[4];
		for (int i = 0; i < limbs.Length; i++)
		{
			limbs[i] = new Limb(this, Rat.bodyChunks[0], i, 0.5f, 0.5f, 0.98f, 5f, 0.5f);
		}
		tail = new TailSegment[(UnityEngine.Random.value < 0.3f) ? 4 : 5];
		for (int j = 0; j < tail.Length; j++)
		{
			tail[j] = new TailSegment(this, TailRad(j), TailConnectionLength(j), (j == 0) ? null : tail[j - 1], 0.75f, 0.95f, 0.4f, pullInPreviousPosition: true);
		}
		spines = new Spine[12];
		for (int k = 0; k < spines.Length; k++)
		{
			float num = (float)k / (float)(spines.Length - 1);
			float num2 = num * 0.7f;
			float roll = UnityEngine.Random.Range(-1f, 1f) * (float)Math.PI / 2f * Mathf.InverseLerp(1f, 0.7f, num);
			float stickUp = UnityEngine.Random.Range(20f, 65f) * ((float)Math.PI / 180f) + Custom.LerpMap(num, 1f, 0.6f, -(float)Math.PI / 2f, 0f);
			float length = num2 * (1f - num) * 5f + 10f;
			spines[k] = new Spine(this, num2, roll, stickUp, length);
		}
		whiskers = new GenericBodyPart[2];
		for (int l = 0; l < 2; l++)
		{
			whiskers[l] = new GenericBodyPart(this, 1f, 0.8f, 0.9f, Rat.mainBodyChunk);
		}
		List<BodyPart> list = new List<BodyPart>();
		list.AddRange(limbs);
		list.AddRange(tail);
		list.AddRange(whiskers);
		bodyParts = list.ToArray();
		bodyRads = new float[4] { 3f, 4f, 5.5f, 4.5f };
		firstLegSprite = totalSprites;
		totalSprites += limbs.Length * 2;
		firstBodySprite = totalSprites;
		totalSprites += 4;
		firstHeadSprite = totalSprites;
		totalSprites += 2;
		firstSpineSprite = totalSprites;
		totalSprites += spines.Length;
		firstEyeSprite = totalSprites;
		totalSprites += 2;
		tailSprite = totalSprites;
		totalSprites++;
		firstWhiskerSprite = totalSprites;
		totalSprites += 2;
		UnityEngine.Random.state = state;
	}

	public override void Update()
	{
		base.Update();
		lastBreath = breath;
		if (!Rat.dead)
		{
			breath += (1f + Rat.AI.CombinedFear) / 40f * Mathf.Lerp(0.6f, 1.4f, Rat.abstractCreature.personality.energy);
		}
		if (breath > 1f)
		{
			breath -= 1f;
			lastBreath -= 1f;
		}
		if (Rat.Consious)
		{
			blink++;
			if (blink > 10)
			{
				blink = UnityEngine.Random.Range(-320, -80);
			}
		}
		else
		{
			blink = 1;
		}
		lastGrabSide = grabSide;
		Vector2 vector = default(Vector2);
		for (int i = 0; i < 2; i++)
		{
			IntVector2 tilePosition = Rat.room.GetTilePosition(Rat.bodyChunks[i].pos);
			IntVector2[] fourDirections = Custom.fourDirections;
			for (int j = 0; j < fourDirections.Length; j++)
			{
				IntVector2 intVector = fourDirections[j];
				if (Rat.room.HasAnySolid(tilePosition + intVector))
				{
					vector += intVector.ToVector2();
				}
			}
		}
		vector = Vector2.ClampMagnitude(vector, 1f);
		if (Rat.Footing)
		{
			grabSide = Mathf.Lerp(grabSide, Vector2.Dot(vector, Custom.PerpendicularVector(bodyDir)), 0.7f);
		}
		else
		{
			grabSide = Mathf.Lerp(grabSide, 0f, 0.1f);
		}
		creatureLooker.Update();
		Vector2 vector2 = Custom.PerpendicularVector(bodyDir);
		tail[0].connectedPoint = Rat.bodyChunks[1].pos;
		for (int k = 0; k < tail.Length; k++)
		{
			tail[k].Update();
			tail[k].vel.y -= 0.5f;
			tail[k].vel = Vector2.Lerp(tail[k].vel, base.owner.bodyChunks[1].vel, 0.1f);
		}
		for (int l = 0; l < limbs.Length; l++)
		{
			Limb limb = limbs[l];
			float num = ((l % 2 == 0) ? (-1f) : 1f);
			bool flag = l / 2 == 1;
			BodyChunk bodyChunk = Rat.bodyChunks[(!flag) ? 1u : 0u];
			Vector2 pos = bodyChunk.pos;
			pos += vector2 * ((bodyChunk.rad + 1f) * Mathf.Clamp(num + grabSide * 2f, -1f, 1f));
			pos += bodyDir * 3f + bodyChunk.vel * 3f;
			if (Rat.Consious && !limb.reachedSnapPosition && (limb.mode != Limb.Mode.HuntAbsolutePosition || Vector2.Distance(limb.pos, pos) > Vector2.Distance(limb.lastPos, pos)))
			{
				limb.FindGrip(Rat.room, Rat.mainBodyChunk.pos, Rat.mainBodyChunk.pos, legLength * 1.5f, pos, -2, -2, behindWalls: true);
			}
			if (!Rat.Consious || !Custom.DistLess(limb.pos, limb.absoluteHuntPos, legLength * 1.5f))
			{
				limb.mode = Limb.Mode.Dangle;
				limb.vel += (pos - bodyChunk.pos).normalized * 13f;
				limb.vel = Vector2.Lerp(limb.vel, pos - limb.pos, 0.5f);
			}
			else
			{
				limb.vel += (pos - bodyChunk.pos).normalized * 5f;
			}
			if (limb.mode == Limb.Mode.Dangle)
			{
				limb.vel.y -= 0.9f * (1f - Rat.mainBodyChunk.submersion);
			}
			limb.Update();
			limb.ConnectToPoint(Rat.mainBodyChunk.pos, legLength, push: false, 0f, Rat.mainBodyChunk.vel, 0.5f, 0f);
		}
		Spine[] array = spines;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Update();
		}
		if (whiskerTwitch > 0)
		{
			whiskerTwitch--;
		}
		if (whiskerTwitch < 0)
		{
			whiskerTwitch++;
		}
		if (UnityEngine.Random.value < 0.05f && Rat.Consious)
		{
			whiskerTwitch = UnityEngine.Random.Range(-5, 5);
		}
		if (whiskerTwitch != 0)
		{
			whiskers[(whiskerTwitch > 0) ? 1u : 0u].vel += Custom.RNV() * 5f;
		}
		for (int m = 0; m < 2; m++)
		{
			float num2 = m * 2 - 1;
			whiskers[m].Update();
			whiskers[m].ConnectToPoint(Rat.mainBodyChunk.pos, whiskerLength, push: true, 0.2f, Rat.mainBodyChunk.vel, 0.1f, 0f);
			whiskers[m].vel += bodyDir * -5f + vector2 * ((num2 * 0.25f - grabSide * 0.75f) * 5f * (whiskerLength / 30f));
		}
		bodyDir = Custom.DirVec(Rat.bodyChunks[1].pos, Rat.bodyChunks[0].pos);
	}

	public float TailConnectionLength(int i)
	{
		return 10f;
	}

	public float TailRad(int i)
	{
		float num = (float)i / (float)(tail.Length - 1);
		return (1f - num) * 1.5f + 0.5f;
	}

	public override void Reset()
	{
		base.Reset();
		BodyPart[] array = bodyParts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset(Rat.mainBodyChunk.pos);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		for (int i = 0; i < 4; i++)
		{
			sLeaser.sprites[firstBodySprite + i] = new FSprite("Circle20");
		}
		for (int j = 0; j < 4; j++)
		{
			sLeaser.sprites[firstLegSprite + j * 2] = new FSprite("RatLegA")
			{
				anchorY = 1f
			};
			sLeaser.sprites[firstLegSprite + j * 2 + 1] = new FSprite("RatLegB")
			{
				anchorY = 1f
			};
		}
		sLeaser.sprites[firstHeadSprite] = new FSprite("RatHead0A")
		{
			anchorX = 0f,
			scale = 1.5f
		};
		sLeaser.sprites[firstHeadSprite + 1] = new FSprite("RatHead0B")
		{
			anchorX = 0f,
			scale = 1.5f
		};
		for (int k = 0; k < 2; k++)
		{
			sLeaser.sprites[firstEyeSprite + k] = new FSprite(bigEyes ? "Circle4" : "tinyStar");
		}
		for (int l = 0; l < spines.Length; l++)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[2]
			{
				new TriangleMesh.Triangle(0, 1, 3),
				new TriangleMesh.Triangle(0, 3, 2)
			};
			TriangleMesh triangleMesh = new TriangleMesh("RainWorld_White", tris, customColor: false, atlasedImage: true);
			triangleMesh.MoveVertice(0, new Vector2(-2f, 0f));
			triangleMesh.MoveVertice(1, new Vector2(-0.5f, 1f));
			triangleMesh.MoveVertice(2, new Vector2(2f, 0f));
			triangleMesh.MoveVertice(3, new Vector2(0.5f, 1f));
			sLeaser.sprites[firstSpineSprite + l] = triangleMesh;
		}
		sLeaser.sprites[tailSprite] = TriangleMesh.MakeLongMesh(tail.Length, pointyTip: false, customColor: true);
		for (int m = 0; m < 2; m++)
		{
			sLeaser.sprites[firstWhiskerSprite + m] = TriangleMesh.MakeLongMesh(7, pointyTip: false, customColor: true);
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float num = Mathf.Lerp(lastGrabSide, grabSide, timeStacker);
		float num2 = Mathf.Lerp(lastBreath, breath, timeStacker);
		Vector2 vector = Vector2.Lerp(Rat.bodyChunks[0].lastPos, Rat.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(Rat.bodyChunks[1].lastPos, Rat.bodyChunks[1].pos, timeStacker);
		Vector2 normalized = (vector - vector2).normalized;
		Vector2 vector3 = Custom.PerpendicularVector(normalized);
		Vector2 vector4 = vector3 * (num * 2f);
		vector += vector4;
		vector2 += vector4;
		vector += LimbPushback(0, timeStacker) + LimbPushback(1, timeStacker);
		vector2 += LimbPushback(2, timeStacker) + LimbPushback(3, timeStacker);
		float num3 = rCam.room.Darkness((vector + vector2) / 2f);
		num3 *= 1f - 0.5f * rCam.room.LightSourceExposure((vector + vector2) / 2f);
		Color color = Color.Lerp(coatColor, blackColor, Mathf.Lerp(num3, 1f, coatDarkness));
		Vector2 vector5 = vector;
		Vector2 vector6 = normalized * 0.5f;
		if (creatureLooker.lookCreature != null && Rat.room != null)
		{
			if (creatureLooker.lookCreature.VisualContact)
			{
				vector6 += Custom.DirVec(Rat.mainBodyChunk.pos, creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos);
			}
			else if (creatureLooker.lookCreature.EstimatedChanceOfFinding * Rat.AI.CreatureUnease(creatureLooker.lookCreature.representedCreature) > 0.2f)
			{
				vector6 += Custom.DirVec(Rat.mainBodyChunk.pos, Rat.room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition()));
			}
		}
		vector5 += normalized * 0.5f + vector3 * (num * 2f);
		Vector2 normalized2 = (normalized + vector6 * 0.2f).normalized;
		Vector2 vector7 = Custom.PerpendicularVector(normalized2);
		int num4 = Mathf.RoundToInt(Mathf.Clamp01(Mathf.Abs(num)) * 2f);
		float scaleY = ((num > 0f) ? (-1f) : 1f);
		sLeaser.sprites[firstHeadSprite].SetPosition(vector5 - camPos);
		sLeaser.sprites[firstHeadSprite].SetElementByName($"RatHead{num4}A");
		sLeaser.sprites[firstHeadSprite].scaleY = scaleY;
		sLeaser.sprites[firstHeadSprite].rotation = Custom.VecToDeg(normalized2) - 90f;
		sLeaser.sprites[firstHeadSprite].rotation = Custom.VecToDeg(normalized2) - 90f;
		sLeaser.sprites[firstHeadSprite].color = color;
		sLeaser.sprites[firstHeadSprite + 1].SetPosition(sLeaser.sprites[firstHeadSprite].GetPosition());
		sLeaser.sprites[firstHeadSprite + 1].SetElementByName($"RatHead{num4}B");
		sLeaser.sprites[firstHeadSprite + 1].rotation = sLeaser.sprites[firstHeadSprite].rotation;
		sLeaser.sprites[firstHeadSprite + 1].scaleY = scaleY;
		sLeaser.sprites[firstHeadSprite + 1].color = Color.Lerp(headColor, blackColor, num3);
		string elementByName = ((blink <= 0) ? (bigEyes ? "Circle4" : "tinyStar") : (bigEyes ? "tinyStar" : "pixel"));
		for (int i = 0; i < 2; i++)
		{
			float num5 = (float)(i * 2 - 1) * 90f - num * 90f;
			float num6 = Mathf.Sin((float)Math.PI / 180f * num5) * 2f;
			sLeaser.sprites[firstEyeSprite + i].SetPosition(vector5 + normalized2 * 6f + vector7 * num6 - camPos);
			sLeaser.sprites[firstEyeSprite + i].SetElementByName(elementByName);
			sLeaser.sprites[firstEyeSprite + i].isVisible = Mathf.Cos((float)Math.PI / 180f * num5) > -0.2f && Rat.bites > 1;
		}
		for (int j = 0; j < 4; j++)
		{
			float num7 = (float)j / 3f;
			float num8 = num7 * (1f - num7) * 4f * Mathf.Sin(num2 * (float)Math.PI);
			sLeaser.sprites[firstBodySprite + j].color = color;
			sLeaser.sprites[firstBodySprite + j].scale = bodyRads[j] / 10f * (1f + num8 * 0.2f);
		}
		sLeaser.sprites[firstBodySprite].SetPosition(Vector2.Lerp(vector, vector5, 0.25f) - vector3 * num - camPos);
		sLeaser.sprites[firstBodySprite + 1].SetPosition(vector - camPos);
		sLeaser.sprites[firstBodySprite + 2].SetPosition((vector + vector2) / 2f - vector3 * (num * 2f * Rat.abstractCreature.personality.nervous) - camPos);
		sLeaser.sprites[firstBodySprite + 2].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
		sLeaser.sprites[firstBodySprite + 2].scaleY = Mathf.Max(sLeaser.sprites[firstBodySprite + 2].scaleX, Vector2.Distance(vector2, vector) / 20f);
		sLeaser.sprites[firstBodySprite + 3].SetPosition(vector2 - camPos);
		for (int k = 0; k < 4; k++)
		{
			FSprite fSprite = sLeaser.sprites[firstLegSprite + k * 2];
			FSprite obj = sLeaser.sprites[firstLegSprite + k * 2 + 1];
			bool flag = k / 2 == 1;
			Vector2 vector8 = (flag ? vector : vector2);
			Vector2 v = Vector2.Lerp(limbs[k].lastPos, limbs[k].pos, timeStacker) - vector8;
			fSprite.SetPosition(vector8 + v.normalized * Mathf.Clamp(v.magnitude - 9f, -5f, 3.5f) - camPos);
			fSprite.rotation = Custom.VecToDeg(v) + 180f;
			fSprite.scaleY = 1f;
			fSprite.scaleX = (flag ? (-1f) : 1f);
			fSprite.color = color;
			obj.SetPosition(fSprite.GetPosition());
			obj.rotation = fSprite.rotation;
			obj.scaleY = fSprite.scaleY;
			obj.scaleX = fSprite.scaleX;
			obj.color = Color.Lerp(footColor, blackColor, num3);
		}
		TriangleMesh triangleMesh = sLeaser.sprites[tailSprite] as TriangleMesh;
		Vector2 vector9 = vector2;
		vector9 += Vector2.ClampMagnitude((Vector2.Lerp(tail[0].lastPos, tail[0].pos, timeStacker) - vector2) * 0.5f, 3f);
		float num9 = 0f;
		Color b = Color.Lerp(tailColor, blackColor, num3);
		for (int l = 0; l < tail.Length; l++)
		{
			float f = (float)l / (float)(tail.Length - 1);
			Vector2 vector10 = Vector2.Lerp(tail[l].lastPos, tail[l].pos, timeStacker);
			Vector2 normalized3 = (vector10 - vector9).normalized;
			Vector2 vector11 = Custom.PerpendicularVector(normalized3);
			float num10 = Vector2.Distance(vector9, vector10) / 5f;
			float num11 = TailRad(l);
			Vector2 vector12 = vector11 * ((num9 + num11) * 0.5f);
			Vector2 vector13 = vector11 * num11;
			triangleMesh.MoveVertice(l * 4, vector9 - vector12 + normalized3 * num10 - camPos);
			triangleMesh.MoveVertice(l * 4 + 1, vector9 + vector12 + normalized3 * num10 - camPos);
			triangleMesh.MoveVertice(l * 4 + 2, vector10 - vector13 - normalized3 * num10 - camPos);
			triangleMesh.MoveVertice(l * 4 + 3, vector10 + vector13 - normalized3 * num10 - camPos);
			Color color2 = Color.Lerp(color, b, Mathf.Pow(f, 0.75f));
			for (int m = 0; m < 4; m++)
			{
				triangleMesh.verticeColors[l * 4 + m] = color2;
			}
			vector9 = vector10;
			num9 = num11;
		}
		for (int n = 0; n < spines.Length; n++)
		{
			Vector2 basePos = spines[n].GetBasePos(vector, vector2, timeStacker);
			Vector2 endPos = spines[n].GetEndPos(vector, vector2, timeStacker);
			FSprite obj2 = sLeaser.sprites[firstSpineSprite + n];
			obj2.SetPosition(basePos - camPos);
			obj2.rotation = Custom.AimFromOneVectorToAnother(basePos, endPos);
			obj2.scaleY = Vector2.Distance(basePos, endPos);
			obj2.scaleX = (spines[n].flip ? (-1f) : 1f);
			obj2.color = color;
		}
		for (int num12 = 0; num12 < whiskers.Length; num12++)
		{
			TriangleMesh triangleMesh2 = sLeaser.sprites[firstWhiskerSprite + num12] as TriangleMesh;
			float num13 = num12 * 2 - 1;
			float num14 = num13 * 0.25f - num * 0.75f;
			Vector2 vector14 = vector + vector3 * (Mathf.Clamp01(num13 * 0.7f - num) * 3.5f);
			Vector2 cA = vector14 + vector3 * (num14 * 12f);
			Vector2 vector15 = Vector2.Lerp(whiskers[num12].lastPos, whiskers[num12].pos, timeStacker);
			Vector2 cB = vector15 + normalized * 5f + vector3 * (num14 * 5f);
			for (int num15 = 0; num15 < triangleMesh2.vertices.Length; num15 += 2)
			{
				float num16 = (float)num15 / (float)(triangleMesh2.vertices.Length - 2);
				Vector2 vector16 = Custom.Bezier(vector14, cA, vector15, cB, num16);
				Vector2 vector17 = Custom.PerpendicularVector(Custom.BezierDerivative(vector14, cA, vector15, cB, num16));
				Color color3 = Color.Lerp(blackColor, Color.white, Mathf.Pow(num16 * num16 * (1f - num16) * 3f, 2f) * (1f - num3));
				if (num15 == 0)
				{
					color3 = color;
				}
				triangleMesh2.MoveVertice(num15, vector16 + vector17 * 0.5f - camPos);
				triangleMesh2.MoveVertice(num15 + 1, vector16 - vector17 * 0.5f - camPos);
				triangleMesh2.verticeColors[num15] = color3;
				triangleMesh2.verticeColors[num15 + 1] = color3;
			}
		}
		sLeaser.sprites[firstBodySprite].isVisible = Rat.bites > 1;
		sLeaser.sprites[firstBodySprite + 1].isVisible = Rat.bites > 1;
		for (int num17 = 4; num17 < 8; num17++)
		{
			sLeaser.sprites[firstLegSprite + num17].isVisible = Rat.bites > 1;
		}
		for (int num18 = 0; num18 < 2; num18++)
		{
			sLeaser.sprites[firstWhiskerSprite + num18].isVisible = Rat.bites > 1;
		}
		sLeaser.sprites[firstHeadSprite].isVisible = Rat.bites > 1;
		sLeaser.sprites[firstHeadSprite + 1].isVisible = Rat.bites > 1;
		triangleMesh.isVisible = Rat.bites > 2;
	}

	public Vector2 LimbPushback(int i, float timeStacker)
	{
		BodyChunk bodyChunk = Rat.bodyChunks[1 - i / 2];
		Vector2 vector = Vector2.Lerp(bodyChunk.lastPos, bodyChunk.pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(limbs[i].lastPos, limbs[i].pos, timeStacker);
		return (vector - vector2).normalized * ((legLength - Vector2.Distance(vector, vector2)) * 0.13f - 1f);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		footColor = palette.blackColor;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[firstEyeSprite + i].color = Color.Lerp(palette.fogColor, Color.white, 0.75f);
		}
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
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score * (1f + Rat.AI.CreatureUnease(crit.representedCreature));
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}

	public bool MuddableSprite(RoomCamera.SpriteLeaser sleaser, int sprite)
	{
		if (sprite == firstEyeSprite || sprite == firstEyeSprite + 1)
		{
			return false;
		}
		return true;
	}

	public void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay)
	{
	}
}
