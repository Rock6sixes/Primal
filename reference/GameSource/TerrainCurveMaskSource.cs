using System;
using UnityEngine;
using Watcher;

public class TerrainCurveMaskSource : UpdatableAndDeletable, IDrawable, INotifyWhenRoomUnloaded
{
	public Mesh mesh;

	public Vector3[] vertices;

	public Vector2[] uvs;

	public int[] indices;

	public bool disposed;

	public float MinDepth { get; set; }

	public float MaxDepth { get; set; }

	public TerrainCurveMaskSource(Vector2[] frontPoints, Vector2[] backPoints, float minDepth, float maxDepth, float bottom = float.NegativeInfinity)
	{
		mesh = new Mesh
		{
			name = "TerrainCurveMask Mesh"
		};
		MinDepth = minDepth;
		MaxDepth = maxDepth;
		SetVertices(frontPoints, backPoints, bottom);
	}

	public void Dispose()
	{
		if (mesh != null)
		{
			UnityEngine.Object.Destroy(mesh);
			disposed = true;
			mesh = null;
		}
	}

	public void RoomUnloaded()
	{
		Dispose();
	}

	public void SetVertices(Vector2[] frontPoints, Vector2[] backPoints, float bottom = float.NegativeInfinity)
	{
		if (disposed)
		{
			return;
		}
		Array.Resize(ref vertices, frontPoints.Length * 3);
		Array.Resize(ref uvs, vertices.Length);
		Array.Resize(ref indices, (frontPoints.Length - 1) * 4 * 3);
		int num = 0;
		int ind = 0;
		for (int i = 0; i < frontPoints.Length; i++)
		{
			if (i < frontPoints.Length - 1)
			{
				AddQuad(indices, ref ind, num);
				AddQuad(indices, ref ind, num + 1);
			}
			float y = Mathf.Max(frontPoints[i].y - 2000f, bottom);
			vertices[num++] = To3D(new Vector2(frontPoints[i].x, y), 0f);
			vertices[num++] = To3D(frontPoints[i], 0f);
			vertices[num++] = To3D(backPoints[i], 1f);
		}
		for (int j = 0; j < uvs.Length; j += 3)
		{
			float a = frontPoints[j / 3].y - Mathf.Max(frontPoints[j / 3].y - 2000f, bottom);
			uvs[j].y = Mathf.Max(a, 0f);
			uvs[j + 1].y = 0f;
			uvs[j + 2].y = 0f;
		}
		mesh.SetVertices(vertices, 0, vertices.Length);
		mesh.SetUVs(0, uvs, 0, uvs.Length);
		mesh.SetIndices(indices, MeshTopology.Triangles, 0);
	}

	public static Vector3 To3D(Vector2 pos, float z)
	{
		return new Vector3(pos.x, pos.y, z);
	}

	public static void AddQuad(int[] indices, ref int ind, int vert)
	{
		indices[ind++] = vert;
		indices[ind++] = vert + 1;
		indices[ind++] = vert + 3;
		indices[ind++] = vert + 3;
		indices[ind++] = vert + 1;
		indices[ind++] = vert + 4;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[0];
		sLeaser.maskSources = new MaskSource[1];
		sLeaser.maskSources[0] = MaskMaker.MakeSource("SlopedTerrainMaskGrab", "SlopedTerrainMask", isSecondLayer: false, mesh);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (disposed)
		{
			if (sLeaser.maskSources.Length != 0)
			{
				sLeaser.maskSources = new MaskSource[0];
			}
			return;
		}
		MaskSource obj = sLeaser.maskSources[0];
		obj.setGameObjectPos = new Vector3(0f, 0f, MinDepth / 30f) - (Vector3)camPos;
		obj.setGameObjectRotation = new Vector3(0f, 0f, 0f);
		obj.setGameObjectScale = new Vector3(1f, 1f, MaxDepth / 30f - MinDepth / 30f) / 16f;
		obj.ApplyVertexColor();
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
	}
}
