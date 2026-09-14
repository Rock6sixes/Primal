using System;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class DrillCrabGraphics : GraphicsModule
{
	public int firstEyeSprite;

	public int blobSprite;

	public int firstDrillSprite;

	public int firstToothSprite;

	public int firstLegSprite;

	public int totalSprites;

	public float legWidth;

	public int[] toothCount = new int[3] { 9, 8, 5 };

	public float[] toothRingRadius = new float[3] { 21f, 20f, 9f };

	public float[] toothAngle = new float[3] { -10f, 30f, 45f };

	public int totalToothCount;

	public float lastDrillRotation;

	public float drillRotation;

	public bool layeringDirty;

	public DrillCrab Crab => base.owner as DrillCrab;

	public DrillCrab.IndividualVariations IVars => Crab.iVars;

	public DrillCrabGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		for (int i = 0; i < toothCount.Length; i++)
		{
			toothCount[i] += IVars.extraTeeth;
		}
		legWidth = Mathf.LerpUnclamped(3f, 6f, IVars.legThickness);
		totalToothCount = toothCount.Sum();
		totalSprites = 0;
		firstEyeSprite = totalSprites;
		totalSprites += 4;
		blobSprite = totalSprites;
		totalSprites++;
		firstDrillSprite = totalSprites;
		totalSprites += 5;
		firstToothSprite = totalSprites;
		totalSprites += totalToothCount;
		firstLegSprite = totalSprites;
		totalSprites += Crab.legs.Length * (Crab.legs[0].segments.Length * 2 - 1);
		layeringDirty = Crab.State.layer > 0f;
	}

	public override void Update()
	{
		base.Update();
		lastDrillRotation = drillRotation;
		drillRotation += Mathf.Pow(Crab.spinUp, 1.5f) * 0.5f;
		if (layeringDirty)
		{
			layeringDirty = false;
			UpdateLayering();
		}
	}

	public void UpdateLayering()
	{
		TowerCrabLayerer towerCrabLayerer = Crab.room.updateList.OfType<TowerCrabLayerer>().FirstOrDefault();
		if (towerCrabLayerer == null)
		{
			towerCrabLayerer = new TowerCrabLayerer();
			Crab.room.AddObject(towerCrabLayerer);
		}
		RoomCamera[] cameras = Crab.room.game.cameras;
		foreach (RoomCamera rCam in cameras)
		{
			towerCrabLayerer.Add(rCam, this);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.containers = new FContainer[1]
		{
			new FContainer()
		};
		sLeaser.sprites = new FSprite[totalSprites];
		FShader shader = rCam.game.rainWorld.Shaders[(Crab.State.layer > 0f) ? "CustomDepth" : "Basic"];
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[firstEyeSprite + i * 2] = TriangleMesh.MakeLongMeshAtlased(6, pointyTip: false, customColor: false);
			sLeaser.sprites[firstEyeSprite + i * 2].shader = shader;
			sLeaser.sprites[firstEyeSprite + i * 2 + 1] = new FSprite("pixel", quadType: false)
			{
				anchorY = 0.1f,
				shader = shader
			};
		}
		sLeaser.sprites[blobSprite] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders[(Crab.State.layer > 0f) ? "JaggedCircleCustomDepth" : "JaggedCircle"]
		};
		sLeaser.sprites[firstDrillSprite] = new FSprite("CrabDrillCoverA", quadType: false)
		{
			shader = shader
		};
		sLeaser.sprites[firstDrillSprite + 1] = new FSprite("CrabDrillCoverB", quadType: false)
		{
			shader = shader
		};
		sLeaser.sprites[firstDrillSprite + 2] = new FSprite("CrabDrillBodyA", quadType: false)
		{
			shader = shader
		};
		sLeaser.sprites[firstDrillSprite + 3] = new FSprite("CrabDrillBodyB", quadType: false)
		{
			shader = shader
		};
		sLeaser.sprites[firstDrillSprite + 4] = new FSprite("CrabDrillHead", quadType: false)
		{
			anchorY = 0f,
			shader = shader
		};
		for (int j = firstToothSprite; j < firstToothSprite + totalToothCount; j++)
		{
			sLeaser.sprites[j] = new FSprite("SnailTail", quadType: false)
			{
				shader = shader
			};
		}
		float num = (Crab.Tower ? 30f : 10f);
		int num2 = firstLegSprite;
		for (int k = 0; k < Crab.legs.Length; k++)
		{
			DrillCrab.Leg leg = Crab.legs[k];
			for (int l = 0; l < leg.segments.Length; l++)
			{
				sLeaser.sprites[num2] = TriangleMesh.MakeLongMeshAtlased(Mathf.CeilToInt(leg.segments[l].prevDist / num), pointyTip: false, customColor: true);
				sLeaser.sprites[num2].shader = shader;
				num2++;
				if (l < leg.segments.Length - 1)
				{
					sLeaser.sprites[num2++] = new FSprite("Circle20", quadType: false)
					{
						shader = shader
					};
				}
			}
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node in sprites)
		{
			sLeaser.containers[0].AddChild(node);
		}
		sLeaser.containers[0].sortZ = 1f - Crab.State.layer;
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer(Crab.Tower ? "Items" : "Midground");
		}
		newContatiner.AddChild(sLeaser.containers[0]);
	}

	public Rect GetBodyBounds(float timeStacker)
	{
		Rect rect = new Rect(Vector2.Lerp(Crab.bodyChunks[0].lastPos, Crab.bodyChunks[0].pos, timeStacker), Vector2.zero);
		rect = rect.CloneWithExpansion(Crab.bodyChunks[0].rad);
		for (int i = 1; i < Crab.bodyChunks.Length; i++)
		{
			Vector2 vector = Vector2.Lerp(Crab.bodyChunks[i].lastPos, Crab.bodyChunks[i].pos, timeStacker);
			float rad = Crab.bodyChunks[i].rad;
			rect.xMin = Mathf.Min(rect.xMin, vector.x - rad);
			rect.yMin = Mathf.Min(rect.yMin, vector.y - rad);
			rect.xMax = Mathf.Max(rect.xMax, vector.x + rad);
			rect.yMax = Mathf.Max(rect.yMax, vector.y + rad);
		}
		DrillCrab.Leg[] legs = Crab.legs;
		for (int j = 0; j < legs.Length; j++)
		{
			DrillCrab.Leg.Segment[] segments = legs[j].segments;
			foreach (DrillCrab.Leg.Segment segment in segments)
			{
				Vector2 vector2 = Vector2.Lerp(segment.lastPos, segment.pos, timeStacker);
				rect.xMin = Mathf.Min(rect.xMin, vector2.x);
				rect.yMin = Mathf.Min(rect.yMin, vector2.y);
				rect.xMax = Mathf.Max(rect.xMax, vector2.x);
				rect.yMax = Mathf.Max(rect.yMax, vector2.y);
			}
		}
		return rect.CloneWithExpansion(40f);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (Crab.room == null)
		{
			return;
		}
		bool flag = GetBodyBounds(timeStacker).Overlaps(new Rect(camPos, rCam.sSize));
		FSprite[] sprites = sLeaser.sprites;
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].isVisible = flag;
		}
		if (!flag)
		{
			return;
		}
		Vector2 vector = Vector2.Lerp(Crab.bodyChunks[0].lastPos, Crab.bodyChunks[0].pos, timeStacker) - camPos;
		Vector2 vector2 = Custom.DirVec(vector, Vector2.Lerp(Crab.bodyChunks[1].lastPos, Crab.bodyChunks[1].pos, timeStacker) - camPos);
		Vector2 vector3 = Custom.PerpendicularVector(vector2);
		float num = Mathf.Lerp(Crab.lastFlip, Crab.flip, timeStacker);
		float num2 = Mathf.Pow(Crab.spinUp, 2f);
		for (int j = 0; j < 2; j++)
		{
			DrillCrab.Eye eye = Crab.eyes[(Crab.flip > 0f) ? j : (1 - j)];
			TriangleMesh triangleMesh = sLeaser.sprites[firstEyeSprite + j * 2] as TriangleMesh;
			FSprite fSprite = sLeaser.sprites[firstEyeSprite + j * 2 + 1];
			float num3 = Custom.SCurve(Mathf.Lerp(Crab.lastEyesHidden, Crab.eyesHidden, timeStacker), 1f);
			Vector2 vector4 = Vector2.Lerp(eye.lastPos, eye.pos, timeStacker) - camPos;
			Vector2 vector5 = vector - vector2 * 25f;
			Vector2 vector6 = Vector3.Slerp((vector4 - vector5).normalized, Vector2.up, 0.75f);
			Vector2 cA = vector5 + vector3 * (num * (Crab.Tower ? 50f : 20f) + eye.side * 15f * Mathf.Cos(num * (float)Math.PI * 0.3f)) - vector2 * (Crab.Tower ? 45f : 20f);
			Vector2 cB = vector4 - vector6 * (Crab.Tower ? (-20f) : 40f) + vector3 * (num * -15f);
			for (int k = 0; k < triangleMesh.vertices.Length; k += 2)
			{
				float num4 = (float)k / (float)(triangleMesh.vertices.Length - 2) * (1f - num3);
				Vector2 vector7 = Custom.Bezier(vector5, cA, vector4, cB, num4);
				Vector2 vector8 = Custom.PerpendicularVector(Custom.BezierDerivative(vector5, cA, vector4, cB, num4));
				float num5 = (Mathf.Cos(num4 * (float)Math.PI) * 0.4f + 0.6f) * 5f;
				triangleMesh.MoveVertice(k, vector7 + vector8 * num5);
				triangleMesh.MoveVertice(k + 1, vector7 - vector8 * num5);
			}
			float num6 = (Crab.Tower ? 1.5f : 1f);
			fSprite.SetPosition(Custom.Bezier(vector5, cA, vector4, cB, 1f - num3) + vector3 * eye.side);
			fSprite.rotation = Custom.VecToDeg(vector6);
			fSprite.scaleX = 4f * num6;
			fSprite.scaleY = 7f * num6;
		}
		Vector2 vector9 = Vector2.zero;
		Vector2 vector10 = Vector2.zero;
		float num7 = Mathf.Lerp(0.9f, 1.1f, IVars.drillSize);
		float num8 = Mathf.LerpUnclamped(0.9f, 1.1f, IVars.bodySize);
		if (num2 > 0f)
		{
			vector9 = UnityEngine.Random.value * num2 * 4f * Custom.RNV();
			vector10 = UnityEngine.Random.value * num2 * 3f * Custom.RNV();
		}
		float num9 = Custom.VecToDeg(vector2);
		for (int l = blobSprite; l < firstLegSprite; l++)
		{
			sLeaser.sprites[l].rotation = num9;
		}
		sLeaser.sprites[blobSprite].SetPosition(vector + vector10 - vector2 * 5f);
		sLeaser.sprites[blobSprite].scale = 4.0625f * num8;
		sLeaser.sprites[firstDrillSprite].SetPosition(vector + vector2 * 3f - vector9 * 0.5f);
		sLeaser.sprites[firstDrillSprite].scale = num8;
		sLeaser.sprites[firstDrillSprite + 1].SetPosition(vector + vector2 * 3f - vector9 * 0.5f);
		sLeaser.sprites[firstDrillSprite + 1].scale = num8;
		sLeaser.sprites[firstDrillSprite + 2].SetPosition(vector + vector10 + vector2 * -10f);
		sLeaser.sprites[firstDrillSprite + 2].scale = num8;
		sLeaser.sprites[firstDrillSprite + 3].SetPosition(vector + vector10 + vector2 * -10f + vector3 * (Mathf.Sin(num * 0.2f) * 35f));
		sLeaser.sprites[firstDrillSprite + 3].scaleX = Mathf.Cos(num * 0.2f) * 0.75f * num8;
		sLeaser.sprites[firstDrillSprite + 3].scaleY = num8;
		sLeaser.sprites[firstDrillSprite + 4].SetPosition(vector + vector2 * (5.5f * num7) + vector9);
		sLeaser.sprites[firstDrillSprite + 4].scale = num7;
		int num10 = firstToothSprite;
		for (int m = 0; m < toothCount.Length; m++)
		{
			Vector2 vector11 = vector + vector2 * (Custom.LerpMap(m, 0f, toothCount.Length - 1, 7f, 30f) * num7 + 5.5f);
			int num11 = toothCount[m];
			float num12 = toothRingRadius[m] * num7;
			for (int n = 0; n < num11; n++)
			{
				float num13 = (float)n / (float)(num11 - 1) * (float)Math.PI + Mathf.Lerp(lastDrillRotation, drillRotation, timeStacker) + num * 0.5f;
				num13 = (num13 % (float)Math.PI + (float)Math.PI) % (float)Math.PI - (float)Math.PI / 2f;
				sLeaser.sprites[num10].SetPosition(vector11 + vector3 * (num12 * Mathf.Sin(num13)) - vector2 * (num12 * 0.2f * Mathf.Cos(num13)) + Custom.RNV() * (UnityEngine.Random.value * num2 * 1.5f) + vector9);
				sLeaser.sprites[num10].rotation = num9 + num13 * toothAngle[m] / (float)Math.PI * 2f;
				sLeaser.sprites[num10].scaleX = 0.15f + 0.2f * Mathf.Pow(Mathf.Cos(num13), 0.7f) + num2 * 0.6f;
				sLeaser.sprites[num10].scaleY = 0.35f - num2 * 0.15f;
				num10++;
			}
		}
		num10 = firstLegSprite;
		for (int num14 = 0; num14 < Crab.legs.Length; num14++)
		{
			DrillCrab.Leg leg = Crab.legs[num14];
			Vector2 vector12 = Vector2.Lerp(leg.lastAnchor, leg.anchor, timeStacker);
			for (int num15 = 0; num15 < leg.segments.Length; num15++)
			{
				TriangleMesh triangleMesh2 = sLeaser.sprites[num10++] as TriangleMesh;
				Vector2 vector13 = Vector2.Lerp(leg.segments[num15].lastPos, leg.segments[num15].pos, timeStacker);
				Vector2 vector14 = Custom.PerpendicularVector(vector13, vector12);
				for (int num16 = 0; num16 < triangleMesh2.vertices.Length; num16 += 2)
				{
					float t = (float)num16 / (float)(triangleMesh2.vertices.Length - 2);
					Vector2 vector15 = Vector2.Lerp(vector12, vector13, t);
					float num17 = LegContour(num14, num15, t) * legWidth;
					triangleMesh2.MoveVertice(num16, vector15 + vector14 * num17 - camPos);
					triangleMesh2.MoveVertice(num16 + 1, vector15 - vector14 * num17 - camPos);
				}
				if (num15 < leg.segments.Length - 1)
				{
					sLeaser.sprites[num10].scale = LegContour(num14, num15, 1f) / 10f * legWidth;
					sLeaser.sprites[num10++].SetPosition(vector13 - camPos);
				}
				vector12 = vector13;
			}
		}
	}

	public float LegContour(int leg, int segment, float t)
	{
		float num = Mathf.PerlinNoise((float)segment * 0.2f + (float)leg, t * 40f) * 0.5f;
		float num2;
		switch (segment)
		{
		case 0:
			num2 = Mathf.Lerp(0.3f, 1f, t) * (Mathf.Pow(1f - t, 0.4f) * 0.5f + 0.5f);
			break;
		case 1:
			num2 = (Mathf.Cos(t * (float)Math.PI * 2f) * 0.25f + 0.75f) * (1f - 0.2f * t) * (0.5f + 0.5f * Mathf.Pow(t, 0.25f));
			break;
		default:
			if (segment < Crab.legs[leg].segments.Length - 1)
			{
				num2 = (Mathf.Cos(t * (float)Math.PI * 2f) * 0.25f + 0.75f) * (0.8f - 0.2f * t - 0.2f * Mathf.Pow(1f - t, 10f));
				break;
			}
			num2 = Mathf.Pow(1f - t, 1f / 3f) * (0.6f - 0.2f * t);
			num *= 1f - t;
			break;
		}
		num2 += num * 0.5f;
		return Mathf.Pow(num2, 1.25f - IVars.legUniformity * 0.5f);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color black = palette.blackColor;
		Color a = Color.Lerp(palette.blackColor, Color.white, 0.1f);
		Color a2 = Color.Lerp(palette.blackColor, Color.white, 0.2f);
		Color rust = new Color(0.4f, 0.2f, 0.15f);
		if (Crab.Tower)
		{
			a = Color.Lerp(a, rust, 0.1f);
			a2 = Color.Lerp(Color.Lerp(a2, Color.white, 0.1f), rust, 0.1f);
		}
		float t2 = palette.fogAmount * Crab.State.layer;
		black = Color.Lerp(black, palette.fogColor, t2);
		a = Color.Lerp(a, palette.fogColor, t2);
		a2 = Color.Lerp(a2, palette.fogColor, t2);
		rust = Color.Lerp(rust, palette.fogColor, t2);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[firstEyeSprite + i * 2].color = black;
			sLeaser.sprites[firstEyeSprite + i * 2 + 1].color = Color.Lerp(palette.blackColor, Color.white, IVars.eyeLightness * 0.2f + 0.1f);
		}
		sLeaser.sprites[blobSprite].color = black;
		sLeaser.sprites[blobSprite].alpha = 0.1f;
		sLeaser.sprites[firstDrillSprite].color = black;
		sLeaser.sprites[firstDrillSprite + 1].color = a2;
		sLeaser.sprites[firstDrillSprite + 2].color = black;
		sLeaser.sprites[firstDrillSprite + 3].color = a2;
		sLeaser.sprites[firstDrillSprite + 4].color = a;
		for (int j = 0; j < totalToothCount; j++)
		{
			sLeaser.sprites[firstToothSprite + j].color = a2;
		}
		int num = firstLegSprite;
		for (int k = 0; k < Crab.legs.Length; k++)
		{
			DrillCrab.Leg leg = Crab.legs[k];
			for (int l = 0; l < leg.segments.Length; l++)
			{
				TriangleMesh triangleMesh = sLeaser.sprites[num++] as TriangleMesh;
				FSprite fSprite = ((l < leg.segments.Length - 1) ? sLeaser.sprites[num++] : null);
				if (Crab.Tower)
				{
					for (int m = 0; m < triangleMesh.verticeColors.Length; m += 2)
					{
						Color color = LegGradient(((float)l + (float)m / (float)(triangleMesh.verticeColors.Length - 2)) / (float)leg.segments.Length);
						triangleMesh.verticeColors[m] = color;
						triangleMesh.verticeColors[m + 1] = color;
					}
					if (fSprite != null)
					{
						fSprite.color = LegGradient((float)(l + 1) / (float)leg.segments.Length);
					}
				}
				else
				{
					triangleMesh.color = black;
					if (fSprite != null)
					{
						fSprite.color = black;
					}
				}
			}
		}
		if (!(Crab.State.layer > 0f))
		{
			return;
		}
		float num2 = 1f - Crab.State.layer;
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite fSprite2 in sprites)
		{
			if (fSprite2 is TriangleMesh { verticeColors: not null } triangleMesh2)
			{
				for (int num3 = 0; num3 < triangleMesh2.verticeColors.Length; num3++)
				{
					triangleMesh2.verticeColors[num3].a = num2;
				}
			}
			else
			{
				fSprite2.alpha = num2;
			}
		}
		Color LegGradient(float t)
		{
			return Color.Lerp(Color.Lerp(black, Color.white, Mathf.Pow(t, 2f) * 0.2f), rust, Mathf.Pow(t, 3f) * 0.2f);
		}
	}
}
