using System;
using System.Collections.Generic;
using System.IO;
using RWCustom;
using UnityEngine;

public class BrainMold : CosmeticSprite, INotifyWhenRoomIsReady
{
	public class EdgeGraphic
	{
		public BrainMold owner;

		public SharedPhysics.Edge edge;

		public float[] heights;

		public float[] fadeAlphas;

		public float[] lastHeights;

		public float[] affectedFactors;

		public float[] targetHeights;

		public const float segmentsPerTile = 2f;

		public float length;

		public EdgeGraphic(BrainMold owner, SharedPhysics.Edge edge)
		{
			this.owner = owner;
			this.edge = edge;
			length = Vector2.Distance(edge.start, edge.end);
			int num = Mathf.RoundToInt(Vector2.Distance(edge.start, edge.end) / 20f * 2f);
			heights = new float[num + 1];
			lastHeights = new float[num + 1];
			affectedFactors = new float[num + 1];
			targetHeights = new float[num + 1];
			fadeAlphas = new float[num + 1];
			for (int i = 0; i < affectedFactors.Length; i++)
			{
				float num2 = (float)i / (float)(affectedFactors.Length - 1);
				float val = Mathf.Min(num2 * length, (1f - num2) * length);
				Vector2 pos = Vector2.Lerp(edge.start, edge.end, num2);
				affectedFactors[i] = owner.PointAffectedFactor(pos);
				float num3 = GetHeightFromFactor(affectedFactors[i]) * Mathf.Pow(Mathf.PerlinNoise(pos.x / 50f, pos.y / 50f), 0.5f) * Custom.LerpMap(val, 0f, 30f, 0.25f, 1f);
				heights[i] = num3;
				lastHeights[i] = num3;
				targetHeights[i] = num3;
			}
		}

		public static float GetHeightFromFactor(float factor)
		{
			return Mathf.Pow(factor, 0.65f) * 30f + 3f;
		}

		public static float GetDepthFromFactor(float factor)
		{
			return Mathf.Pow(factor, 0.5f) * 30f + 15f;
		}

		public void Update()
		{
			Array.Copy(heights, lastHeights, heights.Length);
			for (int i = 0; i < heights.Length; i++)
			{
				heights[i] = Custom.LerpAndTick(heights[i], targetHeights[i], 0.1f, 0.01f);
			}
			Vector2 normal = edge.normal;
			Vector2 rhs = -Custom.PerpendicularVector(normal);
			Vector2 start = edge.start;
			_ = edge;
			float heightFromFactor = GetHeightFromFactor(1f);
			foreach (PhysicalObject item in owner.room.physicalObjects[1])
			{
				Vector2 lhs = item.firstChunk.pos - start;
				if (Mathf.Abs(Vector2.Dot(lhs, normal)) > item.collisionRange)
				{
					continue;
				}
				float num = Vector2.Dot(lhs, rhs);
				if (num < 0f - item.collisionRange || num > length + item.collisionRange)
				{
					continue;
				}
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					float num2 = Vector2.Dot(bodyChunk.pos - start, rhs);
					float num3 = Vector2.Dot(bodyChunk.pos - start, normal);
					if (num3 < 0f || num3 > bodyChunk.rad + heightFromFactor)
					{
						continue;
					}
					float num4 = bodyChunk.rad + 10f;
					int value = Mathf.FloorToInt((num2 - num4) / length * (float)heights.Length);
					int value2 = Mathf.CeilToInt((num2 + num4) / length * (float)heights.Length);
					int num5 = Mathf.Clamp(value, 0, heights.Length - 1);
					value2 = Mathf.Clamp(value2, 0, heights.Length - 1);
					for (int k = num5; k < value2 + 1; k++)
					{
						if (item is Player && owner.room.game.IsStorySession && owner.room.game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility)
						{
							if (targetHeights[k] > 3f)
							{
								(item as Player).SetHaloDisplayTime(10);
							}
							targetHeights[k] = Mathf.Lerp(targetHeights[k], 0f, 0.01f);
							if (targetHeights[k] < 6f)
							{
								fadeAlphas[k] = Mathf.InverseLerp(6f, 3f, targetHeights[k]);
							}
						}
						float num6 = (float)k / (float)(heights.Length - 1) * length;
						float num7 = Custom.SCurve(Mathf.Clamp01(1f - Mathf.Abs(num2 - num6) / num4), 1f) * Mathf.InverseLerp(targetHeights[k], 0f, num3 - bodyChunk.rad);
						float num8 = targetHeights[k] * (1f - num7 * 0.9f);
						if (num8 < heights[k])
						{
							heights[k] = Mathf.Lerp(heights[k], num8, 0.4f);
						}
					}
				}
			}
		}

		public TriangleMesh CreateMesh(RoomCamera rCam)
		{
			TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[heights.Length * 4 + 4];
			for (int i = 0; i < heights.Length + 1; i++)
			{
				int num = i * 4;
				int num2 = i * 3;
				array[num] = new TriangleMesh.Triangle(num2, num2 + 1, num2 + 3);
				array[num + 1] = new TriangleMesh.Triangle(num2 + 3, num2 + 1, num2 + 4);
				array[num + 2] = new TriangleMesh.Triangle(num2 + 1, num2 + 2, num2 + 4);
				array[num + 3] = new TriangleMesh.Triangle(num2 + 4, num2 + 2, num2 + 5);
			}
			TriangleMesh triangleMesh = new TriangleMesh("brainmatter", array, customColor: true);
			Vector2[] uVvertices = triangleMesh.UVvertices;
			Color[] verticeColors = triangleMesh.verticeColors;
			triangleMesh.shader = rCam.game.rainWorld.Shaders["BrainMold"];
			for (int j = 0; j < heights.Length + 2; j++)
			{
				int num3 = j * 3;
				for (int k = 0; k < 3; k++)
				{
					uVvertices[num3 + k] = new Vector2(0.5f, (float)k / 2f);
				}
			}
			uVvertices[0].x = 0f;
			uVvertices[1].x = 0f;
			uVvertices[2].x = 0f;
			uVvertices[uVvertices.Length - 1].x = 1f;
			uVvertices[uVvertices.Length - 2].x = 1f;
			uVvertices[uVvertices.Length - 3].x = 1f;
			Vector2 normal = edge.normal;
			for (int l = -1; l <= heights.Length; l++)
			{
				int num4 = Mathf.Clamp(l, 0, heights.Length - 1);
				float t = (float)num4 / (float)(heights.Length - 1);
				Vector2 vector2 = Vector2.Lerp(edge.start, edge.end, t);
				float num5 = targetHeights[num4];
				float depthFromFactor = GetDepthFromFactor(affectedFactors[num4]);
				if (l == -1)
				{
					vector2 += Custom.PerpendicularVector(normal) * num5;
				}
				else if (l == heights.Length)
				{
					vector2 -= Custom.PerpendicularVector(normal) * num5;
				}
				verticeColors[(l + 1) * 3] = CoordToColor(vector2 - normal * depthFromFactor);
				verticeColors[(l + 1) * 3 + 1] = CoordToColor(vector2);
				verticeColors[(l + 1) * 3 + 2] = CoordToColor(vector2 + normal * num5);
				int num6 = Mathf.Clamp(l, 0, heights.Length - 1);
				if (fadeAlphas[num6] > 0f)
				{
					verticeColors[(l + 1) * 3].a = 1f - fadeAlphas[num6];
					verticeColors[(l + 1) * 3 + 1].a = 1f - fadeAlphas[num6];
					verticeColors[(l + 1) * 3 + 2].a = 1f - fadeAlphas[num6];
				}
			}
			return triangleMesh;
			static Color CoordToColor(Vector2 vector)
			{
				return new Color(vector.x / 20f, vector.y / 20f, 0f);
			}
		}

		public void DrawMesh(TriangleMesh mesh, float timeStacker, Vector2 camPos)
		{
			Vector2 normal = edge.normal;
			for (int i = -1; i <= heights.Length; i++)
			{
				int num = Mathf.Clamp(i, 0, heights.Length - 1);
				float t = (float)num / (float)(heights.Length - 1);
				Vector2 vector = Vector2.Lerp(edge.start, edge.end, t);
				float num2 = Mathf.Lerp(lastHeights[num], heights[num], timeStacker);
				float depthFromFactor = GetDepthFromFactor(affectedFactors[num]);
				if (i == -1)
				{
					vector += Custom.PerpendicularVector(normal) * num2;
				}
				else if (i == heights.Length)
				{
					vector -= Custom.PerpendicularVector(normal) * num2;
				}
				mesh.MoveVertice((i + 1) * 3, vector - normal * depthFromFactor - camPos);
				mesh.MoveVertice((i + 1) * 3 + 1, vector - camPos);
				mesh.MoveVertice((i + 1) * 3 + 2, vector + normal * num2 - camPos);
				int num3 = Mathf.Clamp(i, 0, heights.Length - 1);
				if (fadeAlphas[num3] > 0f)
				{
					mesh.alpha = 1f - fadeAlphas[num3];
				}
			}
		}
	}

	public List<PlacedObject> spots = new List<PlacedObject>();

	public EdgeGraphic[] edgeGraphics;

	public void AddSpot(PlacedObject placedObject)
	{
		spots.Add(placedObject);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (edgeGraphics != null)
		{
			EdgeGraphic[] array = edgeGraphics;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Update();
			}
		}
	}

	public void AIMapReady()
	{
		if (spots.Count == 0)
		{
			return;
		}
		IntVector2 tilePosition = room.GetTilePosition(spots[0].pos);
		IntRect bounds = new IntRect(tilePosition.x, tilePosition.y, tilePosition.x, tilePosition.y);
		foreach (PlacedObject spot in spots)
		{
			PlacedObject.ResizableObjectData resizableObjectData = spot.data as PlacedObject.ResizableObjectData;
			tilePosition = room.GetTilePosition(spot.pos);
			bounds.ExpandToInclude(tilePosition - new IntVector2(1, 1) * Mathf.CeilToInt(resizableObjectData.Rad));
			bounds.ExpandToInclude(tilePosition + new IntVector2(1, 1) * Mathf.CeilToInt(resizableObjectData.Rad));
		}
		List<SharedPhysics.Edge> list = SharedPhysics.FindEdges(room, bounds, IsTileAffected);
		edgeGraphics = new EdgeGraphic[list.Count];
		for (int i = 0; i < edgeGraphics.Length; i++)
		{
			edgeGraphics[i] = new EdgeGraphic(this, list[i]);
		}
	}

	public bool IsTileAffected(IntVector2 tile)
	{
		foreach (PlacedObject spot in spots)
		{
			PlacedObject.ResizableObjectData resizableObjectData = spot.data as PlacedObject.ResizableObjectData;
			if (Custom.DistLess(room.MiddleOfTile(tile), spot.pos, resizableObjectData.Rad))
			{
				return true;
			}
		}
		return false;
	}

	public float PointAffectedFactor(Vector2 pos)
	{
		float num = 0f;
		foreach (PlacedObject spot in spots)
		{
			num = Mathf.Max(Mathf.InverseLerp((spot.data as PlacedObject.ResizableObjectData).Rad, 0f, Vector2.Distance(pos, spot.pos)), num);
		}
		return num;
	}

	public void ShortcutsReady()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (!Futile.atlasManager.DoesContainAtlas("brainmatter"))
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "brainmatter.png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: false, crispPixels: false);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture("brainmatter", texture2D, textureFromAsset: false);
		}
		if (edgeGraphics == null)
		{
			sLeaser.sprites = new FSprite[0];
			return;
		}
		sLeaser.sprites = new FSprite[edgeGraphics.Length];
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i] = edgeGraphics[i].CreateMesh(rCam);
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (sLeaser.sprites.Length == 0)
		{
			if (edgeGraphics == null)
			{
				return;
			}
			InitiateSprites(sLeaser, rCam);
		}
		for (int i = 0; i < edgeGraphics.Length; i++)
		{
			edgeGraphics[i].DrawMesh(sLeaser.sprites[i] as TriangleMesh, timeStacker, camPos);
		}
	}
}
