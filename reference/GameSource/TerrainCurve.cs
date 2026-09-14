using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RWCustom;
using UnityEngine;

public class TerrainCurve : UpdatableAndDeletable, IDrawable, TerrainManager.ITerrain
{
	public struct Handle
	{
		public Vector2 Left;

		public Vector2 Middle;

		public Vector2 Right;

		public float Height;

		public Handle(Vector2 left, Vector2 middle, Vector2 right, float height)
		{
			Left = left;
			Middle = middle;
			Right = right;
			Height = height;
		}

		public static float Sample(Handle left, Handle right, float x)
		{
			if (x < left.Middle.x)
			{
				return Mathf.LerpUnclamped(left.Middle.y, left.Left.y, Custom.InverseLerpUnclamped(left.Middle.x, left.Left.x, x));
			}
			if (x > right.Middle.x)
			{
				return Mathf.LerpUnclamped(right.Middle.y, right.Right.y, Custom.InverseLerpUnclamped(right.Middle.x, right.Right.x, x));
			}
			float num = 0f;
			float num2 = 1f;
			for (int i = 0; i < 16; i++)
			{
				float num3 = (num + num2) / 2f;
				if (Sample(left.Middle.x, left.Right.x, right.Left.x, right.Middle.x, num3) < x)
				{
					num = num3;
				}
				else
				{
					num2 = num3;
				}
			}
			return Sample(left.Middle.y, left.Right.y, right.Left.y, right.Middle.y, (num + num2) / 2f);
		}

		public static float SampleBack(Handle left, Handle right, float x)
		{
			left.Left.y += left.Height;
			left.Middle.y += left.Height;
			left.Right.y += left.Height;
			right.Left.y += right.Height;
			right.Middle.y += right.Height;
			right.Right.y += right.Height;
			return Sample(left, right, x);
		}

		public static float Sample(float a, float b, float c, float d, float t)
		{
			float num = 1f - t;
			return num * num * num * a + 3f * num * num * t * b + 3f * num * t * t * c + t * t * t * d;
		}
	}

	public struct Rubble
	{
		public float segment;

		public float depth;

		public int sprite;

		public float scaleX;

		public float scaleY;
	}

	public float targetSegmentWidth = 5f;

	public float segmentWidth;

	public float maxStainHeight = 60f;

	public static float maxSlideNormalY = Mathf.Cos((float)Math.PI / 4f);

	public static float depthSliderMin = -10f;

	public static float depthSliderMax = 5f;

	public static float edgeRadiusSliderMax = 200f;

	public static int maxLights = 16;

	public float minDepth;

	public float maxDepth = 35f;

	public float bottom = float.NegativeInfinity;

	public List<Handle> handles = new List<Handle>();

	public int segments;

	public int drawSegments;

	public float startX;

	public float endX;

	public bool spritesDirty;

	public Vector2[] frontPoints;

	public Vector2[] backPoints;

	public Vector2[] collisionPoints;

	public Vector2[] drawFrontPoints;

	public Vector2[] drawBackPoints;

	public List<Rubble> rubble = new List<Rubble>();

	public TerrainCurveMaskSource maskSource;

	public List<LightSource> tempLights = new List<LightSource>();

	public Vector4[] tempLightColors = new Vector4[maxLights];

	public Vector4[] tempLightParams = new Vector4[maxLights];

	public static bool doLighting = false;

	public bool BurrowAllowed => true;

	public TerrainCurve(Room room)
		: this(room, -400f, room.PixelWidth + 400f, float.NegativeInfinity)
	{
	}

	public TerrainCurve(Room room, float startX, float endX, float bottom)
	{
		base.room = room;
		minDepth = room.roomSettings.TerrainDepth;
		UpdateSize(startX, endX, bottom);
		spritesDirty = false;
	}

	public override void Destroy()
	{
		base.Destroy();
		DisposeMaskSource();
	}

	public void DisposeMaskSource()
	{
		if (maskSource != null)
		{
			maskSource.Dispose();
			maskSource = null;
		}
	}

	public Vector2 SnapToTerrain(Vector2 center, float radius, out Vector2 normal, Vector2? lastCenter = null)
	{
		int val = Mathf.FloorToInt((center.x - startX - radius) / segmentWidth);
		int val2 = Mathf.CeilToInt((center.x - startX + radius) / segmentWidth);
		float num = float.NaN;
		normal = default(Vector2);
		Vector2 obj = lastCenter ?? center;
		if (obj.y < bottom + radius && startX - radius < center.x && center.x < endX + radius)
		{
			if (center.y + radius >= bottom)
			{
				normal = new Vector2(0f, -1f);
				return new Vector2(center.x, bottom - radius);
			}
			return center;
		}
		for (int i = Math.Max(val, 0); i <= Math.Min(segments - 2, val2); i++)
		{
			Vector2 vector = collisionPoints[i];
			Vector2 normalized = (collisionPoints[i + 1] - vector).normalized;
			float num2 = Mathf.Clamp(center.x + radius * normalized.y, vector.x, vector.x + segmentWidth);
			float num3 = (num2 - vector.x) * normalized.y / normalized.x + vector.y + Mathf.Sqrt(radius * radius - Mathf.Pow(center.x - num2, 2f));
			if (float.IsNaN(num) || num3 > num)
			{
				num = num3;
				normal = Custom.PerpendicularVector(normalized);
			}
		}
		if (!float.IsNaN(num))
		{
			center.y = num;
		}
		return center;
	}

	public Vector2 SnapToTerrain(Vector2 center)
	{
		if (center.y < bottom)
		{
			return center;
		}
		return SnapToTerrain(center.x);
	}

	public Vector2 SnapToTerrain(float x)
	{
		int num = Mathf.Clamp((int)((x - startX) / segmentWidth), 0, segments - 2);
		Vector2 a = collisionPoints[num];
		Vector2 b = collisionPoints[num + 1];
		return Vector2.Lerp(a, b, Mathf.InverseLerp(a.x, b.x, x));
	}

	public Vector2 SnapToTerrain(float x, float depth)
	{
		int num = Mathf.Clamp((int)((x - startX) / segmentWidth), 0, segments - 2);
		Vector2 a = Vector2.Lerp(frontPoints[num], backPoints[num], depth);
		Vector2 b = Vector2.Lerp(frontPoints[num + 1], backPoints[num + 1], depth);
		return Vector2.Lerp(a, b, Mathf.InverseLerp(a.x, b.x, x));
	}

	public Vector2 Cast(Vector2 center, float radius, float endX, out Vector2 normal)
	{
		normal = default(Vector2);
		int num;
		int num2;
		int num3;
		if (endX > center.x)
		{
			num = Mathf.FloorToInt((center.x - startX) / segmentWidth);
			num2 = Mathf.CeilToInt((endX - startX + radius) / segmentWidth);
			num3 = 1;
		}
		else
		{
			num = Mathf.CeilToInt((center.x - startX) / segmentWidth);
			num2 = Mathf.FloorToInt((endX - startX - radius) / segmentWidth);
			num3 = -1;
		}
		if (num != num2 && num2 > num != num3 > 0)
		{
			return new Vector2(endX, center.y);
		}
		float num4 = float.NaN;
		for (int i = num; i != num2 + num3; i += num3)
		{
			if (i < 0 || i >= segments - 1)
			{
				continue;
			}
			Vector2 vector = collisionPoints[i];
			Vector2 vector2 = collisionPoints[i + 1];
			if (vector2.y < vector.y)
			{
				Vector2 vector3 = vector;
				Vector2 vector4 = vector2;
				vector2 = vector3;
				vector = vector4;
			}
			Vector2 normalized = (vector - vector2).normalized;
			if (Mathf.Sign(normalized.x) == (float)(-num3))
			{
				float num5 = Mathf.Clamp(center.y - (float)num3 * radius * normalized.x, vector.y, vector2.y);
				float num6 = (num5 - vector.y) * normalized.x / normalized.y + vector.x - (float)num3 * Mathf.Sqrt(radius * radius - Mathf.Pow(center.y - num5, 2f));
				if (float.IsNaN(num4) || ((num3 > 0) ? (num4 < num6) : (num4 > num6)))
				{
					num4 = num6;
					normal = Custom.PerpendicularVector(normalized);
				}
			}
		}
		return new Vector2(float.IsNaN(num4) ? endX : num4, center.y);
	}

	public bool ObstructsTile(int x, int y)
	{
		if ((float)(x + 1) * 20f < startX || endX < (float)x * 20f)
		{
			return false;
		}
		int num = Mathf.Clamp(Mathf.FloorToInt(((float)x * 20f - startX) / segmentWidth), 0, segments - 1);
		int num2 = Mathf.Clamp(Mathf.CeilToInt(((float)(x + 1) * 20f - startX) / segmentWidth), 0, segments - 1);
		float num3 = (float)y * 20f + 10f;
		for (int i = num; i <= num2; i++)
		{
			if (collisionPoints[i].y > num3 && num3 > bottom)
			{
				return true;
			}
		}
		return false;
	}

	public float GetCoverage(int x, int y)
	{
		if ((float)(x + 1) * 20f < startX || endX < (float)x * 20f)
		{
			return 0f;
		}
		float num = 0f;
		float num2 = (float)y * 20f;
		int num3 = Mathf.Clamp(Mathf.FloorToInt(((float)x * 20f - startX) / segmentWidth), 0, segments - 1);
		int num4 = Mathf.Clamp(Mathf.CeilToInt(((float)(x + 1) * 20f - startX) / segmentWidth), 0, segments - 1);
		for (int i = num3; i <= num4; i++)
		{
			num += Mathf.Clamp(collisionPoints[i].y - num2, 0f, 20f) / 20f;
			if (num2 < bottom)
			{
				num -= (bottom - num2) / 20f;
			}
		}
		return num / (float)(num4 - num3 + 1);
	}

	public bool Contains(Vector2 pos)
	{
		if (SnapToTerrain(pos).y > pos.y)
		{
			return pos.y >= bottom;
		}
		return false;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[drawSegments * 2 - 2];
		for (int i = 0; i < drawSegments - 1; i++)
		{
			int num = i * 2;
			array[num] = new TriangleMesh.Triangle(num, num + 1, num + 2);
			array[num + 1] = new TriangleMesh.Triangle(num + 2, num + 1, num + 3);
		}
		if (sLeaser.sprites != null)
		{
			Array.Resize(ref sLeaser.sprites, 3 + rubble.Count);
		}
		else
		{
			sLeaser.sprites = new FSprite[3 + rubble.Count];
		}
		sLeaser.sprites[0] = new TriangleMesh("Futile_White", array, customColor: true)
		{
			shader = rCam.game.rainWorld.Shaders["SlopedTerrainStain"]
		};
		sLeaser.sprites[1] = new TriangleMesh("Futile_White", array, customColor: true)
		{
			shader = rCam.game.rainWorld.Shaders["SlopedTerrainSurface"]
		};
		sLeaser.sprites[2] = new TriangleMesh("Futile_White", array, customColor: true)
		{
			shader = rCam.game.rainWorld.Shaders["SlopedTerrainSurface"]
		};
		for (int j = 0; j < rubble.Count; j++)
		{
			string text = $"rubble{rubble[j].sprite}";
			if (sLeaser.sprites[j + 3] == null)
			{
				sLeaser.sprites[j + 3] = new FSprite(text)
				{
					shader = rCam.game.rainWorld.Shaders["Rubble"]
				};
			}
			else
			{
				sLeaser.sprites[j + 3].SetElementByName(text);
			}
		}
		sLeaser.containers = new FContainer[1]
		{
			new FContainer()
		};
		AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Sand");
		}
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
		newContatiner.AddChild(sLeaser.sprites[1]);
		for (int i = 0; i < rubble.Count; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[3 + i]);
		}
		foreach (UpdatableAndDeletable update in rCam.room.updateList)
		{
			if (update is BlackSpot obj)
			{
				rCam.MoveObjectToInternalContainer(obj, this, 0);
			}
		}
		newContatiner.AddChild(sLeaser.sprites[2]);
		newContatiner.AddChild(sLeaser.containers[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (spritesDirty)
		{
			spritesDirty = false;
			sLeaser.RemoveAllSpritesFromContainer();
			InitiateSprites(sLeaser, rCam);
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		Shader.SetGlobalFloat("_terrainStainFactor", rCam.room.roomSettings.TerrainStainAmount);
		Shader.SetGlobalFloat("_terrainStainBrightness", rCam.room.roomSettings.TerrainStainBrightness);
		Shader.SetGlobalVector("_terrainParams", new Vector4(rCam.room.roomSettings.TerrainLight, rCam.room.roomSettings.TerrainWaves, rCam.room.roomSettings.TerrainEdgeRadius, rCam.room.roomSettings.TerrainGrain));
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = (rCam.sunlightTexture ? new Vector2(rCam.sunlightTexture.width, rCam.sunlightTexture.height) : new Vector2(0f, 0f));
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if (!(placedObject.type != PlacedObject.Type.TerrainSunOffset))
			{
				vector = (placedObject.data as PlacedObject.ResizableObjectData).handlePos;
				break;
			}
		}
		Shader.SetGlobalVector("_sunlightOffset", (vector2 - new Vector2(room.PixelWidth, room.PixelHeight)) / 2f + new Vector2(140f, -120f) + vector);
		UpdateLightInfo(timeStacker);
		float terrainGooHeight = rCam.room.roomSettings.TerrainGooHeight;
		terrainGooHeight = ((terrainGooHeight == 0f) ? float.NegativeInfinity : ((terrainGooHeight != 1f) ? (Mathf.Pow(terrainGooHeight, 2f) * 1500f) : float.PositiveInfinity));
		float terrainSkyFade = room.roomSettings.TerrainSkyFade;
		Shader.SetGlobalVector("_terrainParams2", new Vector4(terrainGooHeight, terrainSkyFade, 0f, 0f));
		int num = Mathf.Clamp((int)(Mathf.InverseLerp(startX, endX, camPos.x - 400f) * (float)segments), 0, segments - drawSegments);
		for (int i = 0; i < drawSegments; i++)
		{
			drawFrontPoints[i] = frontPoints[i + num] - camPos;
			drawBackPoints[i] = backPoints[i + num] - camPos;
		}
		TriangleMesh triangleMesh = (TriangleMesh)sLeaser.sprites[0];
		TriangleMesh triangleMesh2 = (TriangleMesh)sLeaser.sprites[1];
		TriangleMesh triangleMesh3 = (TriangleMesh)sLeaser.sprites[2];
		if (handles.Count < 2 && !(this is LocalTerrainCurve))
		{
			triangleMesh.isVisible = false;
			triangleMesh2.isVisible = false;
			triangleMesh3.isVisible = false;
			return;
		}
		triangleMesh.isVisible = true;
		triangleMesh2.isVisible = true;
		triangleMesh3.isVisible = true;
		for (int j = 0; j < drawSegments; j++)
		{
			Vector2 pos = ((bottom != float.NegativeInfinity) ? new Vector2(drawFrontPoints[j].x, Mathf.Max(bottom - rCam.pos.y, -100f)) : (drawFrontPoints[j] - new Vector2(0f, 2000f)));
			triangleMesh3.MoveVertice(j * 2, pos);
			triangleMesh3.verticeColors[j * 2] = new Color(pos.y - drawFrontPoints[j].y, 0f, 0f, minDepth / 30f);
			triangleMesh3.MoveVertice(j * 2 + 1, drawFrontPoints[j]);
			triangleMesh3.verticeColors[j * 2 + 1] = new Color(0f, 0f, 0f, minDepth / 30f);
			triangleMesh2.MoveVertice(j * 2, drawFrontPoints[j]);
			triangleMesh2.verticeColors[j * 2] = new Color(0f, 0f, 0f, minDepth / 30f);
			triangleMesh2.MoveVertice(j * 2 + 1, drawBackPoints[j]);
			triangleMesh2.verticeColors[j * 2 + 1] = new Color(1f, 0f, 0f, maxDepth / 30f);
		}
		for (int k = 0; k < drawSegments; k++)
		{
			int num2 = Mathf.Clamp(k + num, 1, segments - 2);
			Vector3 rhs = frontPoints[num2 + 1] - frontPoints[num2 - 1];
			Vector3 rhs2 = backPoints[num2 + 1] - backPoints[num2 - 1];
			Vector3 lhs = (Vector3)(backPoints[num2] - frontPoints[num2]) + new Vector3(0f, 0f, maxDepth - minDepth);
			Vector3 normalized = Vector3.Cross(lhs, rhs).normalized;
			Vector3 normalized2 = Vector3.Cross(lhs, rhs2).normalized;
			triangleMesh3.UVvertices[k * 2] = normalized;
			triangleMesh3.UVvertices[k * 2 + 1] = normalized;
			triangleMesh3.verticeColors[k * 2].g = rhs.x;
			triangleMesh3.verticeColors[k * 2].b = rhs.y;
			triangleMesh3.verticeColors[k * 2 + 1].g = rhs.x;
			triangleMesh3.verticeColors[k * 2 + 1].b = rhs.y;
			triangleMesh2.UVvertices[k * 2] = normalized;
			triangleMesh2.UVvertices[k * 2 + 1] = normalized2;
			triangleMesh2.verticeColors[k * 2].g = rhs.x;
			triangleMesh2.verticeColors[k * 2].b = rhs.y;
			triangleMesh2.verticeColors[k * 2 + 1].g = rhs2.x;
			triangleMesh2.verticeColors[k * 2 + 1].b = rhs2.y;
		}
		if (rCam.room.roomSettings.TerrainStainAmount > 0f && rCam.room.roomSettings.TerrainStainHeight > 0f)
		{
			int l = 0;
			for (int m = 0; m < drawSegments; m++)
			{
				for (; l < drawSegments - 2 && ApplyDepth(rCam, drawBackPoints[l + 1], maxDepth).x < ApplyDepth(rCam, drawFrontPoints[m], minDepth).x; l++)
				{
				}
				Vector2 vector3 = ApplyDepth(rCam, drawFrontPoints[m], minDepth);
				Vector2 a = ApplyDepth(rCam, drawBackPoints[l], maxDepth);
				Vector2 b = ApplyDepth(rCam, drawBackPoints[l + 1], maxDepth);
				Vector2 b2 = Vector2.Lerp(a, b, Custom.InverseLerpUnclamped(a.x, b.x, vector3.x));
				Vector2 vector4 = Vector2.LerpUnclamped(vector3, b2, Custom.InverseLerpUnclamped(minDepth, maxDepth, 0f));
				Vector2 vector5 = Vector2.LerpUnclamped(vector3, b2, Custom.InverseLerpUnclamped(minDepth, maxDepth, 30f));
				Vector2 vector6 = vector4 + Vector2.up * (rCam.room.roomSettings.TerrainStainHeight * maxStainHeight);
				Vector2 vector7 = vector5 + Vector2.up * (rCam.room.roomSettings.TerrainStainHeight * maxStainHeight);
				Vector2 pos2 = vector3;
				Vector2 pos3 = new Vector2(vector6.x, Mathf.Max(vector6.y, vector7.y));
				triangleMesh.MoveVertice(m * 2, pos2);
				triangleMesh.MoveVertice(m * 2 + 1, pos3);
				triangleMesh.verticeColors[m * 2] = new Color(0f, 0f, 0f, minDepth / 30f);
				triangleMesh.verticeColors[m * 2 + 1] = new Color(0f, 0f, 0f, minDepth / 30f);
				triangleMesh.UVvertices[m * 2] = new Vector2(Custom.InverseLerpUnclamped(vector4.y, vector6.y, pos2.y), Custom.InverseLerpUnclamped(vector5.y, vector7.y, pos2.y));
				triangleMesh.UVvertices[m * 2 + 1] = new Vector2(Custom.InverseLerpUnclamped(vector4.y, vector6.y, pos3.y), Custom.InverseLerpUnclamped(vector5.y, vector7.y, pos3.y));
			}
			triangleMesh.isVisible = true;
		}
		else
		{
			triangleMesh.isVisible = false;
		}
		Color color = new Color(0f, 0f, 0.5f);
		for (int n = 0; n < rubble.Count; n++)
		{
			FSprite fSprite = sLeaser.sprites[3 + n];
			int num3 = (int)rubble[n].segment - num;
			if (num3 >= 0 && num3 < drawSegments - 2)
			{
				fSprite.isVisible = true;
				float num4 = Mathf.Lerp(minDepth, maxDepth, rubble[n].depth);
				Vector2 vector8 = ApplyDepth(rCam, Vector2.Lerp(drawFrontPoints[num3], drawBackPoints[num3], rubble[n].depth), num4);
				Vector2 vector9 = ApplyDepth(rCam, Vector2.Lerp(drawFrontPoints[num3 + 1], drawBackPoints[num3 + 1], rubble[n].depth), num4);
				fSprite.SetPosition(Vector2.Lerp(vector8, vector9, rubble[n].segment % 1f));
				fSprite.rotation = 0f - Vector2.SignedAngle(Vector2.right, vector9 - vector8);
				fSprite.scaleX = rubble[n].scaleX;
				fSprite.scaleY = rubble[n].scaleY;
				color.a = Mathf.Clamp(1f - num4 / 30f, 0.01f, 0.99f);
				sLeaser.sprites[n + 3].color = color;
			}
			else
			{
				fSprite.isVisible = false;
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public static Vector2 ApplyDepth(RoomCamera rCam, Vector2 screenPoint, float depth)
	{
		Vector2 depthPoint = new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * 2f / 3f);
		return Custom.ApplyDepthOnVector(screenPoint, depthPoint, depth - 6f);
	}

	public void UpdateLightInfo(float timeStacker)
	{
		foreach (LightSource item in room.lightSources.Concat(room.cosmeticLightSources))
		{
			if (room.ViewedByAnyCamera(item.Pos, item.Rad) && (item.Alpha > 0f || item.lastAlpha > 0f) && !item.flat && !Contains(item.Pos) && item.Pos.y - SnapToTerrain(item.Pos).y < item.Rad * 2f && doLighting)
			{
				tempLights.Add(item);
			}
		}
		if (tempLights.Count > maxLights)
		{
			tempLights.Sort((LightSource a, LightSource b) => Mathf.Abs(a.Pos.y - SnapToTerrain(a.Pos).y).CompareTo(Mathf.Abs(b.Pos.y - SnapToTerrain(b.Pos).y)));
		}
		Array.Clear(tempLightColors, 0, tempLightColors.Length);
		Array.Clear(tempLightParams, 0, tempLightParams.Length);
		for (int i = 0; i < maxLights && i < tempLights.Count; i++)
		{
			LightSource lightSource = tempLights[i];
			tempLightColors[i] = lightSource.color;
			tempLightColors[i].w = Mathf.Lerp(lightSource.lastAlpha, lightSource.Alpha, timeStacker) * Mathf.Lerp(1f, room.Darkness(lightSource.Pos), lightSource.affectedByPaletteDarkness);
			tempLightParams[i].x = Mathf.Lerp(lightSource.lastPos.x, lightSource.Pos.x, timeStacker);
			tempLightParams[i].y = Mathf.Lerp(lightSource.lastPos.y, lightSource.Pos.y, timeStacker);
			tempLightParams[i].z = Mathf.Lerp(lightSource.lastRad, lightSource.Rad, timeStacker);
			tempLightParams[i].w = 0f;
		}
		tempLights.Clear();
		Shader.SetGlobalVectorArray("_lightSourceColors", tempLightColors);
		Shader.SetGlobalVectorArray("_lightSourceParams", tempLightParams);
	}

	public void UpdateRubble()
	{
		if (RainWorld.MainThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
		{
			return;
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		int count = this.rubble.Count;
		this.rubble.Clear();
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if (!(placedObject.type != PlacedObject.Type.TerrainRubble))
			{
				Vector2 pos = placedObject.pos;
				float rad = (placedObject.data as PlacedObject.ResizableObjectData).Rad;
				float val = pos.y - SnapToTerrain(pos).y;
				UnityEngine.Random.InitState((int)((pos.x + pos.y + rad) * 100f));
				int num = (int)Custom.LerpMap(val, -200f, 100f, rad / 4f, 1f);
				for (int i = 0; i < num; i++)
				{
					float num2 = Mathf.Pow(UnityEngine.Random.Range(0.75f, 1f), 2f);
					float f = UnityEngine.Random.Range(-1f, 1f);
					f = Mathf.Sign(f) * (1f - Mathf.Sqrt(Mathf.Abs(f))) * 0.5f + 0.5f;
					float num3 = Mathf.Lerp(pos.x - rad, pos.x + rad, f);
					float value = UnityEngine.Random.value;
					Rubble rubble = default(Rubble);
					rubble.segment = (num3 - startX) / segmentWidth;
					rubble.depth = value;
					rubble.sprite = UnityEngine.Random.Range(0, 8);
					rubble.scaleX = num2 * ((UnityEngine.Random.value < 0.5f) ? 1f : (-1f));
					rubble.scaleY = num2;
					Rubble item = rubble;
					this.rubble.Add(item);
				}
			}
		}
		UnityEngine.Random.state = state;
		if (count != this.rubble.Count)
		{
			spritesDirty = true;
		}
	}

	public void UpdateHandles()
	{
		if (this is LocalTerrainCurve)
		{
			return;
		}
		handles.Clear();
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if (placedObject.type == PlacedObject.Type.TerrainHandle && placedObject.data is PlacedObject.TerrainHandleData terrainHandleData)
			{
				handles.Add(new Handle(terrainHandleData.leftOffset + placedObject.pos, placedObject.pos, terrainHandleData.rightOffset + placedObject.pos, terrainHandleData.backHeight));
			}
		}
		handles.Sort((Handle a, Handle b) => Math.Sign(a.Middle.x - b.Middle.x));
		if (handles.Count >= 2)
		{
			int i = 0;
			for (int j = 0; j < segments; j++)
			{
				float num;
				for (num = Mathf.Lerp(startX, endX, (float)j / ((float)segments - 1f)); i < handles.Count - 2 && handles[i + 1].Middle.x < num; i++)
				{
				}
				frontPoints[j] = new Vector2(num, Handle.Sample(handles[i], handles[i + 1], num));
				backPoints[j] = new Vector2(num, Handle.SampleBack(handles[i], handles[i + 1], num));
				frontPoints[j].y = Normalize(frontPoints[j].y, -10000f);
				backPoints[j].y = Normalize(backPoints[j].y, -10000f);
			}
			UpdateCollision();
			maskSource?.SetVertices(frontPoints, backPoints);
		}
		else
		{
			Array.Clear(frontPoints, 0, frontPoints.Length);
			Array.Clear(backPoints, 0, backPoints.Length);
		}
		spritesDirty = true;
	}

	public static float Normalize(float f, float defaultValue)
	{
		if (float.IsNaN(f) || float.IsInfinity(f))
		{
			return defaultValue;
		}
		return f;
	}

	public void UpdateCollision()
	{
		float t = (6f - minDepth) / (maxDepth - minDepth);
		for (int i = 0; i < segments; i++)
		{
			collisionPoints[i] = Vector2.Lerp(frontPoints[i], backPoints[i], t);
		}
	}

	public void UpdateSize(float startX, float endX, float bottom)
	{
		this.startX = startX;
		this.endX = endX;
		this.bottom = bottom;
		segments = (int)((endX - startX) / targetSegmentWidth) + 1;
		segmentWidth = (endX - startX) / (float)(segments - 1);
		drawSegments = Math.Min((int)(2166f / segmentWidth), segments);
		frontPoints = new Vector2[segments];
		backPoints = new Vector2[segments];
		collisionPoints = new Vector2[segments];
		drawFrontPoints = new Vector2[drawSegments];
		drawBackPoints = new Vector2[drawSegments];
		UpdateHandles();
		UpdateRubble();
		spritesDirty = true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!base.slatedForDeletetion)
		{
			if (maskSource == null)
			{
				maskSource = CreateMaskSource();
			}
			if (maskSource.room != room)
			{
				room.AddObject(maskSource);
			}
			if (minDepth != room.roomSettings.TerrainDepth)
			{
				minDepth = room.roomSettings.TerrainDepth;
				UpdateCollision();
				maskSource.MinDepth = minDepth;
			}
		}
	}

	public virtual TerrainCurveMaskSource CreateMaskSource()
	{
		return new TerrainCurveMaskSource(frontPoints, backPoints, minDepth, maxDepth);
	}
}
