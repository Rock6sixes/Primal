using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BarnacleGraphics : GraphicsModule, ILookingAtCreatures
{
	public struct Cone
	{
		public Vector3 basePos;

		public Vector3 tipPos;

		public float baseRad;

		public float tipRad;

		public float eyeRad;

		public float lightness;

		public Cone(Vector3 basePos, Vector3 tipPos, float baseRad, float tipRad, float eyeRad, float lightness)
		{
			this.basePos = basePos;
			this.tipPos = tipPos;
			this.baseRad = baseRad;
			this.tipRad = tipRad;
			this.eyeRad = eyeRad;
			this.lightness = lightness;
		}
	}

	public struct ConeDrawData
	{
		public int index;

		public Vector3 from;

		public Vector3 to;
	}

	public CreatureLooker creatureLooker;

	public Limb[] legs;

	public Cone[] cones;

	public float legLength;

	public Color darkShellColor;

	public Color lightShellColor;

	public Color blackColor;

	public float roll;

	public float lastRoll;

	public float walkSway;

	public float darkness;

	public int blink;

	public const int spritesPerCone = 5;

	public float innerRad = 13f;

	public float outerRad = 35f;

	public int firstLegSprite;

	public int firstBodySprite;

	public int firstEyeSprite;

	public int firstConeSprite;

	public int totalSprites;

	public static TriangleMesh.Triangle[] coneTris = new TriangleMesh.Triangle[2]
	{
		new TriangleMesh.Triangle(0, 1, 2),
		new TriangleMesh.Triangle(2, 1, 3)
	};

	public static List<ConeDrawData> tempData = new List<ConeDrawData>();

	public Barnacle Barnacle => base.owner as Barnacle;

	public Color GetShellColor(float lightness)
	{
		return Color.Lerp(darkShellColor, lightShellColor, lightness);
	}

	public BarnacleGraphics(PhysicalObject po)
		: base(po, internalContainers: false)
	{
		legs = new Limb[6];
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i] = new Limb(this, Barnacle.bodyChunks[0], i, 1f, 0.5f, 0.9f, 7f, 0.8f)
			{
				mode = Limb.Mode.Dangle
			};
		}
		BodyPart[] array = legs.ToArray();
		bodyParts = array;
		creatureLooker = new CreatureLooker(this, Barnacle.AI.tracker, Barnacle, 0f, 30);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(Barnacle.abstractCreature.ID.RandomSeed);
		legLength = 22f;
		GenerateCones();
		UnityEngine.Random.state = state;
		firstLegSprite = totalSprites;
		totalSprites += legs.Length * 2;
		firstBodySprite = totalSprites;
		totalSprites += 2;
		firstEyeSprite = totalSprites;
		totalSprites += 4;
		firstConeSprite = totalSprites;
		totalSprites += cones.Length * 5;
	}

	public void GenerateCones()
	{
		Barnacle.CreatureParams creatureParams = Barnacle.creatureParams;
		Cone[] array = new Cone[creatureParams.cones];
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
			onUnitSphere.y = Mathf.Abs(onUnitSphere.y);
			float num = 1f + UnityEngine.Random.Range(-0.3f, 0.3f) * creatureParams.coneRadVariance;
			num /= (float)creatureParams.cones / 15f;
			float num2 = 6.5f * num;
			array[i] = new Cone(onUnitSphere * innerRad, Vector3.zero, num2, num2 * creatureParams.tipRadFactor, num2 * creatureParams.tipRadFactor * creatureParams.eyeSizeMultiplier * UnityEngine.Random.Range(0.5f, 0.9f), Mathf.Lerp(0.5f, UnityEngine.Random.value, creatureParams.coneColorVariance));
		}
		int num3 = 50;
		for (int j = 0; j < num3; j++)
		{
			for (int k = 0; k < array.Length; k++)
			{
				for (int l = k + 1; l < array.Length; l++)
				{
					float num4 = array[k].baseRad + array[l].baseRad;
					Vector3 vector = array[l].basePos - array[k].basePos;
					float magnitude = vector.magnitude;
					if (magnitude < num4)
					{
						Vector3 vector2 = vector / magnitude * (num4 - magnitude);
						array[k].basePos -= vector2 * 0.2f;
						array[l].basePos += vector2 * 0.2f;
						array[k].basePos = array[k].basePos.normalized * innerRad;
						array[l].basePos = array[l].basePos.normalized * innerRad;
					}
				}
			}
			for (int m = 0; m < array.Length; m++)
			{
				array[m].basePos = Vector3.Slerp(array[m].basePos, new Vector3(0f, 1f, 0f), 0.1f);
				if (array[m].basePos.y < 0f)
				{
					array[m].basePos.y = 0f;
				}
			}
		}
		for (int n = 0; n < array.Length; n++)
		{
			array[n].tipPos = array[n].basePos.normalized * Mathf.Lerp(innerRad, outerRad, Mathf.Lerp(0.5f, UnityEngine.Random.Range(0.2f, 1f), creatureParams.coneLengthVariance));
		}
		cones = array;
	}

	public override void Update()
	{
		base.Update();
		creatureLooker.Update();
		if (blink > 0)
		{
			blink--;
		}
		if (!Barnacle.Consious)
		{
			blink = Mathf.Max(blink, 30);
		}
		if (blink == 0 && UnityEngine.Random.value < 0.005f)
		{
			blink = Mathf.Max(blink, UnityEngine.Random.Range(10, 25));
		}
		lastRoll = roll;
		Vector2 normalized = (Barnacle.bodyChunks[0].pos - Barnacle.bodyChunks[1].pos).normalized;
		Vector2 lhs = Custom.PerpendicularVector(normalized);
		int num = 0;
		Vector2 vector = default(Vector2);
		IntVector2 tilePosition = Barnacle.room.GetTilePosition(Barnacle.mainBodyChunk.pos);
		IntVector2[] eightDirections = Custom.eightDirections;
		for (int i = 0; i < eightDirections.Length; i++)
		{
			IntVector2 intVector = eightDirections[i];
			if (Barnacle.room.HasAnySolid(tilePosition + intVector))
			{
				num++;
				vector -= intVector.ToVector2().normalized;
			}
		}
		if (Barnacle.Submersion > 0f)
		{
			walkSway += 0.05f;
			walkSway %= 1f;
		}
		else if (Barnacle.AI.runSpeed < 0.1f)
		{
			walkSway = Custom.LerpAndTick(walkSway, 0.5f, 0.05f, 0.02f);
		}
		else
		{
			walkSway += Barnacle.AI.runSpeed / 12f;
			walkSway %= 1f;
		}
		float num2 = roll;
		if (Barnacle.Swimming && Barnacle.Submersion < 1f)
		{
			num2 = 0f - normalized.x;
		}
		else if (Barnacle.Swimming)
		{
			num2 = 0f;
		}
		else if (num > 0)
		{
			num2 = ((!(vector == Vector2.zero)) ? (0f - Mathf.Clamp(Vector2.Dot(lhs, vector.normalized) * 2f, -1f, 1f)) : 0f);
		}
		else if (Barnacle.room.GetTile(Barnacle.mainBodyChunk.pos).wallbehind)
		{
			num2 = normalized.x * 0.1f;
		}
		num2 += Mathf.Sin(walkSway * (float)Math.PI * 2f) * 0.15f * Custom.LerpMap((Barnacle.bodyChunks[0].pos - Barnacle.bodyChunks[0].lastPos).magnitude, 0.1f, 5f, 0f, 1f);
		roll = Custom.LerpAndTick(roll, num2, 0.1f, 0.04f);
		for (int j = 0; j < legs.Length; j++)
		{
			Limb limb = legs[j];
			Vector2 legAttachment = GetLegAttachment(j, 1f);
			Vector2 legTarget = GetLegTarget(j);
			limb.Update();
			limb.ConnectToPoint(legAttachment, legLength, push: false, 0f, base.owner.bodyChunks[0].vel, 0.1f, 0f);
			if (Barnacle.Consious && !limb.reachedSnapPosition && !Barnacle.Swimming)
			{
				limb.FindGrip(Barnacle.room, legAttachment, legAttachment, legLength * 1.5f, legTarget, -2, -2, behindWalls: true);
			}
			if (Barnacle.Consious && Barnacle.Swimming)
			{
				float num3 = legLength / 2f - 5f;
				float f = (walkSway * (float)Math.PI * 2f + (float)j / 5f * (float)Math.PI * 2f) * Mathf.Sign(normalized.x);
				limb.mode = Limb.Mode.HuntAbsolutePosition;
				limb.absoluteHuntPos = Vector2.MoveTowards(legTarget, legAttachment, num3) + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * num3;
			}
			else if (!Barnacle.Consious || !Custom.DistLess(limb.pos, limb.absoluteHuntPos, legLength * 1.5f))
			{
				limb.mode = Limb.Mode.Dangle;
				limb.vel += (legTarget - legAttachment).normalized * 13f;
				limb.vel = Vector2.Lerp(limb.vel, legTarget - limb.pos, 0.5f);
			}
			else
			{
				limb.vel += (legTarget - legAttachment).normalized * 5f;
			}
		}
	}

	public Vector2 GetShellBottom(float timeStacker)
	{
		float num = Mathf.Lerp(lastRoll, roll, timeStacker);
		Vector2 vector = Vector2.Lerp(Barnacle.bodyChunks[0].lastPos, Barnacle.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(Barnacle.bodyChunks[1].lastPos, Barnacle.bodyChunks[1].pos, timeStacker);
		Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
		return vector + vector3 * (Barnacle.bodyChunks[0].rad * 0.2f * Mathf.Sin(num * (float)Math.PI / 2f));
	}

	public Vector2 GetLegAttachment(int index, float timeStacker)
	{
		float num = ((index % 2 == 0) ? 1f : (-1f));
		float num2 = (float)(index / 2) / 2f;
		float num3 = Mathf.Lerp(lastRoll, roll, timeStacker);
		Vector2 vector = Vector2.Lerp(Barnacle.bodyChunks[0].lastPos, Barnacle.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(Barnacle.bodyChunks[1].lastPos, Barnacle.bodyChunks[1].pos, timeStacker);
		Vector2 normalized = (vector - vector2).normalized;
		Vector2 vector3 = Custom.PerpendicularVector(normalized);
		return GetShellBottom(timeStacker) + normalized * (Barnacle.bodyChunks[0].rad * 0.5f * num2) + vector3 * (Barnacle.bodyChunks[0].rad * 0.15f * Mathf.Sin(num * (float)Math.PI * 0.25f + num3 * (float)Math.PI * 0.5f));
	}

	public Vector2 GetLegTarget(int index)
	{
		float num = ((index % 2 == 0) ? 1f : (-1f));
		float num2 = (float)(index / 2) / 2f;
		Vector2 pos = Barnacle.bodyChunks[0].pos;
		Vector2 pos2 = Barnacle.bodyChunks[1].pos;
		Vector2 normalized = (pos - pos2).normalized;
		Vector2 vector = Custom.PerpendicularVector(normalized);
		return GetLegAttachment(index, 1f) + normalized * (legLength * num2) + vector * (legLength * Mathf.Sin(num * (float)Math.PI * 0.25f + roll * (float)Math.PI * 0.5f));
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[firstBodySprite + i] = new FSprite("Futile_White")
			{
				shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
				alpha = 0.5f
			};
		}
		for (int j = 0; j < legs.Length; j++)
		{
			int num = firstLegSprite + j * 2;
			sLeaser.sprites[num] = new FSprite("CentipedeLegA")
			{
				anchorY = 0.1f
			};
			sLeaser.sprites[num + 1] = new FSprite("CentipedeLegB")
			{
				anchorY = 0.1f
			};
		}
		for (int k = 0; k < 2; k++)
		{
			sLeaser.sprites[firstEyeSprite + k] = new FSprite("LizardScaleA5")
			{
				anchorY = 0f
			};
			sLeaser.sprites[firstEyeSprite + k + 2] = new FSprite("pixel");
		}
		for (int l = 0; l < cones.Length; l++)
		{
			int num2 = firstConeSprite + l * 5;
			sLeaser.sprites[num2] = new TriangleMesh("RainWorld_White", coneTris, customColor: false, atlasedImage: true);
			sLeaser.sprites[num2 + 1] = new FSprite("Circle20", quadType: false);
			sLeaser.sprites[num2 + 2] = new FSprite("Circle20", quadType: false);
			sLeaser.sprites[num2 + 3] = new FSprite("Circle20", quadType: false);
			sLeaser.sprites[num2 + 4] = new FSprite("Circle20", quadType: false);
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled || sLeaser.deleteMeNextFrame)
		{
			return;
		}
		float num = Mathf.Lerp(lastRoll, roll, timeStacker);
		Vector2 vector = Vector2.Lerp(Barnacle.bodyChunks[0].lastPos, Barnacle.bodyChunks[0].pos, timeStacker);
		Vector2 shellBottom = GetShellBottom(timeStacker);
		Vector2 normalized = (vector - Vector2.Lerp(Barnacle.bodyChunks[1].lastPos, Barnacle.bodyChunks[1].pos, timeStacker)).normalized;
		Quaternion quaternion = Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.right, normalized), Vector3.forward) * Quaternion.AngleAxis(num * 90f + 90f, Vector3.right);
		float num2 = 0f;
		if (Barnacle.shakeTime > 0)
		{
			num2 = Mathf.Sqrt((float)Barnacle.shakeTime / 20f) * 5f;
		}
		if (Barnacle.AI.groupDiscomfort > 0.25f)
		{
			float t = (Barnacle.AI.groupDiscomfort - 0.25f) / 0.75f;
			num2 = Mathf.Max(num2, Mathf.Lerp(0f, 1.5f, t));
		}
		if (num2 > 0f && Barnacle.hasShell)
		{
			shellBottom += UnityEngine.Random.insideUnitCircle * num2;
		}
		float num3 = darkness;
		darkness = rCam.room.Darkness(vector);
		if (darkness > 0f)
		{
			darkness *= 1f - rCam.room.LightSourceExposure(vector);
		}
		if (darkness != num3)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		sLeaser.sprites[firstBodySprite].SetPosition(shellBottom - camPos + normalized * 4f);
		sLeaser.sprites[firstBodySprite].rotation = Custom.VecToDeg(normalized);
		sLeaser.sprites[firstBodySprite].scaleY = innerRad / 8f * 0.9f;
		sLeaser.sprites[firstBodySprite].scaleX = innerRad / 8f * Mathf.Lerp(0.5f, 0.7f, Mathf.Abs(Mathf.Cos(num * (float)Math.PI / 2f)));
		sLeaser.sprites[firstBodySprite + 1].SetPosition(shellBottom - camPos - normalized * 3f);
		sLeaser.sprites[firstBodySprite + 1].rotation = Custom.VecToDeg(normalized);
		sLeaser.sprites[firstBodySprite + 1].scaleY = innerRad / 8f * 0.6f;
		sLeaser.sprites[firstBodySprite + 1].scaleX = innerRad / 8f * Mathf.Lerp(0.4f, 0.6f, Mathf.Abs(Mathf.Cos(num * (float)Math.PI / 2f)));
		for (int i = 0; i < 2; i++)
		{
			if (Barnacle.hasShell)
			{
				sLeaser.sprites[firstEyeSprite + i].isVisible = false;
				sLeaser.sprites[firstEyeSprite + i + 2].isVisible = false;
				continue;
			}
			sLeaser.sprites[firstEyeSprite + i + 2].isVisible = true;
			Vector3 vector2 = new Vector3(innerRad * 0.8f, innerRad * 0.1f, ((float)i * 2f - 1f) * 1.5f);
			vector2 = quaternion * vector2 + (Vector3)shellBottom;
			Vector3 vector3 = new Vector3(innerRad * 1.3f, innerRad * 0.3f, ((float)i * 2f - 1f) * 2.5f);
			vector3 = quaternion * vector3 + (Vector3)shellBottom;
			if (creatureLooker.lookCreature != null)
			{
				Vector2 vector4 = ((!creatureLooker.lookCreature.VisualContact) ? Vector2.ClampMagnitude((Barnacle.room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition()) - shellBottom) / 70f, 1f) : Vector2.ClampMagnitude((creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos - shellBottom) / 70f, 1f));
				vector3 += (Vector3)vector4 * 4f;
			}
			sLeaser.sprites[firstEyeSprite + i + 2].SetPosition((Vector2)vector3 - camPos);
			sLeaser.sprites[firstEyeSprite + i + 2].scale = ((blink > 0) ? 1f : 2f);
			sLeaser.sprites[firstEyeSprite + i + 2].rotation = Custom.VecToDeg(normalized);
			sLeaser.sprites[firstEyeSprite + i].isVisible = true;
			sLeaser.sprites[firstEyeSprite + i].SetPosition((Vector2)vector3 - camPos);
			sLeaser.sprites[firstEyeSprite + i].rotation = Custom.AimFromOneVectorToAnother(vector3, vector2);
			sLeaser.sprites[firstEyeSprite + i].scaleY = Vector2.Distance(vector3, vector2) / 29f;
		}
		for (int j = 0; j < legs.Length; j++)
		{
			Limb limb = legs[j];
			float num4 = ((j % 2 == 0) ? 1f : (-1f));
			if (Mathf.Abs(num) > 0.5f)
			{
				num4 = Mathf.Sign(num);
			}
			Vector2 legAttachment = GetLegAttachment(j, timeStacker);
			Vector2 vector5 = Vector2.Lerp(limb.lastPos, limb.pos, timeStacker);
			Vector2 vector6 = Custom.InverseKinematic(legAttachment, vector5, legLength / 2f, legLength / 2f, num4);
			int num5 = firstLegSprite + j * 2;
			sLeaser.sprites[num5].SetPosition(legAttachment - camPos);
			sLeaser.sprites[num5].rotation = Custom.AimFromOneVectorToAnother(legAttachment, vector6);
			sLeaser.sprites[num5].scaleY = Vector2.Distance(legAttachment, vector6) / 27f;
			sLeaser.sprites[num5].scaleX = (0f - Mathf.Sign(num4)) * Barnacle.creatureParams.legThickness;
			sLeaser.sprites[num5 + 1].SetPosition(vector6 - camPos);
			sLeaser.sprites[num5 + 1].rotation = Custom.AimFromOneVectorToAnother(vector6, vector5);
			sLeaser.sprites[num5 + 1].scaleY = Vector2.Distance(vector6, vector5) / 25f;
			sLeaser.sprites[num5 + 1].scaleX = (0f - Mathf.Sign(num4)) * Barnacle.creatureParams.legThickness;
		}
		for (int k = 0; k < cones.Length * 5; k++)
		{
			sLeaser.sprites[firstConeSprite + k].isVisible = Barnacle.hasShell;
		}
		if (!Barnacle.hasShell)
		{
			return;
		}
		tempData.Clear();
		for (int l = 0; l < cones.Length; l++)
		{
			tempData.Add(new ConeDrawData
			{
				index = l,
				from = quaternion * cones[l].basePos,
				to = quaternion * cones[l].tipPos
			});
		}
		tempData.Sort((ConeDrawData a, ConeDrawData b) => a.from.z.CompareTo(b.from.z));
		int num6 = firstConeSprite;
		foreach (ConeDrawData tempDatum in tempData)
		{
			Cone cone = cones[tempDatum.index];
			Vector2 vector7 = shellBottom + (Vector2)tempDatum.from;
			Vector2 vector8 = shellBottom + (Vector2)tempDatum.to;
			Vector2 vector9 = Custom.PerpendicularVector((vector8 - vector7).normalized);
			Color color = Color.Lerp(darkShellColor, lightShellColor, cone.lightness);
			float rotation = Custom.AimFromOneVectorToAnother(vector7, vector8);
			Vector3 from = tempDatum.from;
			float z = from.normalized.z;
			bool flag = z < 0f;
			int num7 = ((!flag) ? 1 : 2);
			sLeaser.sprites[num6 + num7].SetPosition(vector7 - camPos);
			sLeaser.sprites[num6 + num7].rotation = rotation;
			sLeaser.sprites[num6 + num7].scaleX = cone.baseRad / 10f;
			sLeaser.sprites[num6 + num7].scaleY = cone.baseRad / 10f * Mathf.Abs(z);
			sLeaser.sprites[num6 + num7].color = (flag ? blackColor : color);
			int num8 = (flag ? 1 : 2);
			sLeaser.sprites[num6 + num8].SetPosition(vector8 - camPos);
			sLeaser.sprites[num6 + num8].rotation = rotation;
			sLeaser.sprites[num6 + num8].scaleX = cone.tipRad / 10f;
			sLeaser.sprites[num6 + num8].scaleY = cone.tipRad / 10f * Mathf.Abs(z);
			sLeaser.sprites[num6 + num8].color = color;
			sLeaser.sprites[num6 + 3].SetPosition(vector8 - camPos);
			sLeaser.sprites[num6 + 3].rotation = rotation;
			sLeaser.sprites[num6 + 3].scaleX = cone.eyeRad / 10f;
			sLeaser.sprites[num6 + 3].scaleY = cone.eyeRad / 10f * Mathf.Max(0f, z);
			sLeaser.sprites[num6 + 3].color = Color.Lerp(color, blackColor, 0.5f);
			sLeaser.sprites[num6 + 4].SetPosition(Vector2.Lerp(vector8, vector7, 0.1f) - camPos);
			sLeaser.sprites[num6 + 4].rotation = rotation;
			sLeaser.sprites[num6 + 4].scaleX = cone.eyeRad / 10f;
			sLeaser.sprites[num6 + 4].scaleY = cone.eyeRad / 10f * Mathf.Max(0f, z);
			sLeaser.sprites[num6 + 4].color = blackColor;
			TriangleMesh triangleMesh = sLeaser.sprites[num6] as TriangleMesh;
			triangleMesh.color = color;
			for (int m = 0; m < triangleMesh.vertices.Length; m += 2)
			{
				float t2 = (float)m / (float)(triangleMesh.vertices.Length - 2);
				float num9 = Mathf.Lerp(cone.baseRad, cone.tipRad, ConeContour(t2));
				Vector2 vector10 = Vector2.Lerp(vector7, vector8, t2);
				triangleMesh.MoveVertice(m, vector10 + vector9 * num9 - camPos);
				triangleMesh.MoveVertice(m + 1, vector10 - vector9 * num9 - camPos);
			}
			num6 += 5;
		}
	}

	public float ConeContour(float t)
	{
		return Mathf.Pow(t, 1.5f);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		darkShellColor = Color.Lerp(Barnacle.creatureParams.darkShellColor, palette.blackColor, darkness);
		lightShellColor = Color.Lerp(Barnacle.creatureParams.lightShellColor, palette.blackColor, darkness);
		blackColor = palette.blackColor;
		sLeaser.sprites[firstBodySprite].color = blackColor;
		sLeaser.sprites[firstBodySprite + 1].color = blackColor;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[firstEyeSprite + i].color = blackColor;
			sLeaser.sprites[firstEyeSprite + i + 2].color = Color.Lerp(blackColor, Color.white, 0.25f);
		}
		for (int j = 0; j < legs.Length; j++)
		{
			sLeaser.sprites[firstLegSprite + j * 2].color = blackColor;
			sLeaser.sprites[firstLegSprite + j * 2 + 1].color = blackColor;
		}
	}

	public override void Reset()
	{
		base.Reset();
		Limb[] array = legs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset(Barnacle.bodyChunks[0].pos);
		}
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score * (1f + Barnacle.AI.CreatureUnease(crit.representedCreature));
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}
}
