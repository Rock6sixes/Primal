using System;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;
using Watcher;

public class MudPit : UpdatableAndDeletable, IDrawable, INotifyWhenRoomUnloaded
{
	public class MudBubble : CosmeticSprite
	{
		public float radius;

		public int age;

		public int popTime;

		public int riseTime;

		public int dieTime;

		public Color color;

		public MudBubble(Vector2 pos, float radius, int lifetime, Color color)
		{
			base.pos = pos;
			this.radius = radius;
			this.color = color;
			lastPos = pos;
			dieTime = lifetime;
			popTime = Mathf.Max(dieTime - 10, (dieTime + 1) / 2);
			riseTime = UnityEngine.Random.Range(lifetime / 2, lifetime);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			age++;
			if (age == popTime)
			{
				Pop();
			}
			if (age == dieTime)
			{
				Destroy();
			}
		}

		public void Pop()
		{
			int num = UnityEngine.Random.Range(0, 4);
			for (int i = 0; i < num; i++)
			{
				float y = Mathf.Lerp(5f, 12f, Mathf.Pow(UnityEngine.Random.value, 2f));
				Vector2 vector = Custom.RotateAroundOrigo(new Vector2(0f, y), UnityEngine.Random.Range(-45f, 45f));
				room.AddObject(new Spark(pos, vector, color, null, 80, 90));
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("QuarterPips2")
			{
				rotation = 90f,
				color = color
			};
			AddToContainer(sLeaser, rCam, null);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = Mathf.InverseLerp(0f, riseTime, (float)age + timeStacker);
			float num2 = Mathf.InverseLerp(popTime, dieTime, (float)age + timeStacker);
			Vector2 ps = Vector2.Lerp(lastPos, pos, timeStacker) - new Vector2(0f, radius * (1f - num));
			ps = rCam.ApplyDepth(ps, 5f);
			sLeaser.sprites[0].SetPosition(ps - camPos);
			sLeaser.sprites[0].scale = radius * num / 5.5f;
			sLeaser.sprites[0].scaleX *= 1f - num2;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class MudPitData : PlacedObject.ResizableObjectData
	{
		public float decalSize = 15f;

		public MudPitData(PlacedObject owner)
			: base(owner)
		{
			handlePos = new Vector2(200f, 30f);
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			decalSize = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 3);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}", decalSize);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public PlacedObject pObj;

	public const int maxMuddyTime = 2400;

	public const float segmentWidth = 10f;

	public float[] lastHeights;

	public float[] heights;

	public float[] disturbance;

	public float[] lastDisturbance;

	public static Color defaultColor = new Color(0.31f, 0.19f, 0.13f);

	public Color color = defaultColor;

	public Color black;

	public Mesh maskSourceMesh;

	public Vector3[] maskUvs;

	public Vector3[] maskVerts;

	public int[] indices;

	public static float slowdownMult = 0.75f;

	public static float slowdownLinear = 1.3f;

	public static float slowdownMinSpeed = 2f;

	public MudPitData Data => pObj.data as MudPitData;

	public Rect Rect => new Rect(pObj.pos, Data.handlePos);

	public int SegmentCount => Mathf.Max(2, Mathf.RoundToInt(Rect.width / 10f));

	public MudPit(PlacedObject pObj)
	{
		this.pObj = pObj;
		heights = new float[SegmentCount + 1];
		lastHeights = new float[SegmentCount + 1];
		disturbance = new float[SegmentCount + 1];
		lastDisturbance = new float[SegmentCount + 1];
		maskSourceMesh = new Mesh
		{
			name = "MudPit Mesh"
		};
	}

	public override void Destroy()
	{
		base.Destroy();
		DiposeMeshObject();
	}

	public void DiposeMeshObject()
	{
		if (maskSourceMesh != null)
		{
			UnityEngine.Object.Destroy(maskSourceMesh);
			maskSourceMesh = null;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = MakeSurfaceMesh(rCam);
		TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh("Futile_White", 3);
		triangleMesh.shader = rCam.game.rainWorld.Shaders["MudDecal"];
		for (int i = 0; i < triangleMesh.UVvertices.Length; i++)
		{
			ref Vector2 reference = ref triangleMesh.UVvertices[i];
			reference.x = ((reference.x == 1f || reference.x == 0f) ? reference.x : 0.5f);
			reference.y = ((reference.y == 1f || reference.y == 0f) ? reference.y : 0.5f);
		}
		sLeaser.sprites[1] = triangleMesh;
		sLeaser.maskSources = new MaskSource[1];
		sLeaser.maskSources[0] = MaskMaker.MakeSource("SlopedTerrainMaskGrab", "MudPitMask", isSecondLayer: false, maskSourceMesh);
		AddToContainer(sLeaser, rCam, null);
	}

	public TriangleMesh MakeSurfaceMesh(RoomCamera rCam)
	{
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[SegmentCount * 4];
		for (int i = 0; i < array.Length; i += 4)
		{
			int num = i / 4 * 3;
			array[i] = new TriangleMesh.Triangle(num, num + 1, num + 4);
			array[i + 1] = new TriangleMesh.Triangle(num, num + 4, num + 3);
			array[i + 2] = new TriangleMesh.Triangle(num + 1, num + 2, num + 5);
			array[i + 3] = new TriangleMesh.Triangle(num + 1, num + 5, num + 4);
		}
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", array, customColor: true);
		triangleMesh.shader = rCam.game.rainWorld.Shaders["MudPit"];
		for (int j = 0; j < triangleMesh.vertices.Length; j += 3)
		{
			triangleMesh.UVvertices[j] = new Vector2(0f, -1f);
			triangleMesh.UVvertices[j + 1] = new Vector2(0f, 0f);
			triangleMesh.UVvertices[j + 2] = new Vector2(0f, 1f);
		}
		return triangleMesh;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Sand");
		}
		newContatiner.AddChild(sLeaser.sprites[0]);
		newContatiner.AddChildAtIndex(sLeaser.sprites[1], 0);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = rCam.game.world.region?.regionParams.mudColor ?? defaultColor;
		black = palette.blackColor;
		sLeaser.sprites[1].color = color;
		TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
		for (int i = 0; i < triangleMesh.verticeColors.Length; i++)
		{
			triangleMesh.verticeColors[i] = color;
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (maskSourceMesh == null)
		{
			if (sLeaser.maskSources.Length != 0)
			{
				sLeaser.maskSources = new MaskSource[0];
			}
			return;
		}
		if (((TriangleMesh)sLeaser.sprites[0]).vertices.Length != SegmentCount * 3 + 3)
		{
			sLeaser.sprites[0].RemoveFromContainer();
			sLeaser.sprites[0] = MakeSurfaceMesh(rCam);
			AddToContainer(sLeaser, rCam, null);
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		Rect rect = Rect;
		TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
		for (int i = 0; i < triangleMesh.vertices.Length; i += 3)
		{
			float num = (float)i / (float)(triangleMesh.vertices.Length - 3);
			float x = Mathf.Lerp(rect.xMin, rect.xMax, num);
			int num2 = Mathf.Clamp(i / 3, 0, heights.Length - 1);
			float num3 = Mathf.Lerp(lastHeights[num2], heights[num2], timeStacker);
			triangleMesh.MoveVertice(i, new Vector2(x, rect.yMin) - camPos);
			triangleMesh.MoveVertice(i + 1, new Vector2(x, rect.yMax + num3 * 0.4f) - camPos);
			triangleMesh.MoveVertice(i + 2, rCam.ApplyDepth(new Vector2(x, rect.yMax + num3), 30f) - camPos);
			float a = Mathf.Lerp(lastDisturbance[num2], disturbance[num2], timeStacker);
			float x2 = Mathf.Min(Mathf.Min(num, 1f - num) * rect.width / 60f, 1f);
			for (int j = 0; j < 3; j++)
			{
				triangleMesh.verticeColors[i + j].a = a;
				triangleMesh.UVvertices[i + j].x = x2;
			}
		}
		UpdateMaskSourceMesh(triangleMesh);
		TriangleMesh triangleMesh2 = sLeaser.sprites[1] as TriangleMesh;
		Rect rect2 = Rect;
		Rect rect3 = rect2.CloneWithExpansion(Mathf.Abs(Data.decalSize));
		rect3.yMax -= Mathf.Abs(Data.decalSize) * 0.5f;
		for (int k = 0; k < 4; k++)
		{
			float y = k switch
			{
				1 => rect2.yMin, 
				2 => rect2.yMax, 
				3 => rect3.yMax, 
				_ => rect3.yMin, 
			};
			for (int l = 0; l < 4; l++)
			{
				triangleMesh2.MoveVertice(l + k * 4, new Vector2(l switch
				{
					1 => rect2.xMin, 
					2 => rect2.xMax, 
					3 => rect3.xMax, 
					_ => rect3.xMin, 
				}, y) - camPos);
			}
		}
	}

	public void UpdateMaskSourceMesh(TriangleMesh fMesh)
	{
		if (base.slatedForDeletetion)
		{
			return;
		}
		Vector2[] vertices = fMesh.vertices;
		TriangleMesh.Triangle[] triangles = fMesh.triangles;
		if (maskVerts?.Length != vertices.Length)
		{
			Array.Resize(ref maskVerts, vertices.Length);
			Array.Resize(ref maskUvs, vertices.Length);
			Array.Resize(ref indices, triangles.Length * 3);
			int num = 0;
			for (int i = 0; i < triangles.Length; i++)
			{
				indices[num++] = triangles[i].a;
				indices[num++] = triangles[i].b;
				indices[num++] = triangles[i].c;
			}
			for (int j = 2; j < maskUvs.Length; j += 3)
			{
				maskUvs[j].y = 1f;
			}
			maskSourceMesh.Clear();
		}
		bool flag = false;
		Vector3 vector = vertices[0];
		Vector3 vector2 = vertices[0];
		for (int k = 0; k < vertices.Length; k++)
		{
			Vector3 vector3 = vertices[k];
			vector = Vector3.Min(vector, vector3);
			vector2 = Vector3.Max(vector2, vector3);
			if (maskVerts[k] != vector3)
			{
				maskVerts[k] = vertices[k];
				flag = true;
			}
		}
		if (flag)
		{
			maskSourceMesh.SetVertices(maskVerts, 0, maskVerts.Length);
			maskSourceMesh.SetUVs(0, maskUvs, 0, maskUvs.Length);
			maskSourceMesh.SetIndices(indices, MeshTopology.Triangles, 0, calculateBounds: false);
			maskSourceMesh.bounds = new Bounds((vector2 + vector) / 2f, vector2 - vector);
		}
	}

	public override void Update(bool eu)
	{
		if (base.slatedForDeletetion)
		{
			DiposeMeshObject();
			return;
		}
		if (heights.Length != SegmentCount + 1)
		{
			Array.Resize(ref heights, SegmentCount + 1);
			Array.Resize(ref lastHeights, SegmentCount + 1);
			Array.Resize(ref disturbance, SegmentCount + 1);
			Array.Resize(ref lastDisturbance, SegmentCount + 1);
		}
		Array.Copy(heights, lastHeights, heights.Length);
		Array.Copy(disturbance, lastDisturbance, heights.Length);
		for (int i = 0; i < heights.Length; i++)
		{
			float num = lastHeights[i] * 0.9f;
			if (i > 0)
			{
				num += lastHeights[i - 1] * 0.05f;
			}
			if (i < heights.Length - 1)
			{
				num += lastHeights[i + 1] * 0.05f;
			}
			heights[i] = num * 0.997f;
		}
		for (int j = 0; j < disturbance.Length; j++)
		{
			disturbance[j] = Mathf.Max(disturbance[j] - 0.003f, 0f);
		}
		Rect rect = Rect;
		for (int k = 0; k < room.physicalObjects[1].Count; k++)
		{
			PhysicalObject physicalObject = room.physicalObjects[1][k];
			Rect other = new Rect(physicalObject.firstChunk.pos, Vector2.zero).CloneWithExpansion(physicalObject.collisionRange);
			if (!rect.Overlaps(other))
			{
				continue;
			}
			bool flag = false;
			BodyChunk[] bodyChunks = physicalObject.bodyChunks;
			foreach (BodyChunk bodyChunk in bodyChunks)
			{
				float num2 = bodyChunk.pos.y - rect.yMax;
				if (num2 > bodyChunk.rad)
				{
					continue;
				}
				float num3 = ((num2 < 0f) ? bodyChunk.rad : Mathf.Sqrt(bodyChunk.rad * bodyChunk.rad - num2 * num2));
				int num4 = Mathf.Clamp(GetPreviousPoint(bodyChunk.pos.x - num3), 0, heights.Length - 1);
				int num5 = Mathf.Clamp(GetPreviousPoint(bodyChunk.pos.x + num3) + 1, 0, heights.Length - 1);
				if (num4 != num5)
				{
					if (bodyChunk.vel.magnitude > 1f)
					{
						flag = true;
						ChunkSlowdown(bodyChunk, Mathf.InverseLerp(0f, 5f, bodyChunk.rad - num2));
					}
					if (bodyChunk.pos.y - bodyChunk.lastPos.y < -9f && bodyChunk.lastPos.y - bodyChunk.rad > rect.yMax && bodyChunk.pos.y - bodyChunk.rad <= rect.yMax)
					{
						ChunkSplash(bodyChunk);
					}
					for (int m = num4; m <= num5; m++)
					{
						float num6 = (float)(m - num4) / (float)(num5 - num4);
						heights[m] += (num6 - 0.5f) * bodyChunk.vel.x * 0.5f;
						heights[m] = Mathf.Clamp(heights[m], -10f, 10f);
						disturbance[m] = Mathf.Lerp(disturbance[m], 1f, 0.1f);
					}
				}
			}
			if (flag && physicalObject is Creature creature)
			{
				creature.AddMud(16, 2400, color);
			}
		}
		SpawnBubbles();
	}

	public void ChunkSlowdown(BodyChunk chunk, float factor)
	{
		float num = Mathf.Abs(chunk.vel.x);
		if (num > slowdownMinSpeed)
		{
			num = Mathf.Max(Mathf.Max(num * (1f - factor * slowdownMult), num - slowdownLinear * factor), slowdownMinSpeed);
		}
		chunk.vel.x = Mathf.Sign(chunk.vel.x) * num;
	}

	public void ChunkSplash(BodyChunk chunk)
	{
		int num = UnityEngine.Random.Range(3, 10);
		for (int i = 0; i < num; i++)
		{
			float y = Mathf.Lerp(5f, 12f, Mathf.Pow(UnityEngine.Random.value, 2f));
			Vector2 vel = Custom.RotateAroundOrigo(new Vector2(0f, y), UnityEngine.Random.Range(-45f, 45f));
			room.AddObject(new Spark(new Vector2(chunk.pos.x + UnityEngine.Random.Range(0f - chunk.rad, chunk.rad), Rect.yMax), vel, color, null, 80, 90));
		}
	}

	public float GetPointX(int i)
	{
		return Mathf.Lerp(Rect.xMin, Rect.xMax, (float)i / (float)(heights.Length - 1));
	}

	public int GetPreviousPoint(float x)
	{
		return (int)((x - Rect.xMin) / Rect.width * (float)heights.Length);
	}

	public void SpawnBubbles()
	{
		if (!(UnityEngine.Random.value < Rect.width / 5000f))
		{
			return;
		}
		Vector2 pos = new Vector2(UnityEngine.Random.Range(Rect.xMin, Rect.xMax), Rect.yMax);
		if (!room.GetTile(pos).Solid)
		{
			TerrainManager terrain = room.terrain;
			if (terrain == null || !terrain.Contains(pos))
			{
				float radius = UnityEngine.Random.Range(2.5f, 5.5f);
				int lifetime = UnityEngine.Random.Range(60, 180);
				Color color = Color.Lerp(this.color, black, UnityEngine.Random.Range(0.5f, 0.9f));
				room.AddObject(new MudBubble(pos, radius, lifetime, color));
			}
		}
	}

	public void RoomUnloaded()
	{
		Destroy();
	}
}
