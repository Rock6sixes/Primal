using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class Grass : CosmeticSprite, INotifyWhenRoomUnloaded
{
	public Vector3[] verts;

	public int[] tris;

	public int _maxGrassBladesVisible;

	public static int maxGrassBladesVisible = 1500;

	public List<GrassBlade> grassBlades;

	public bool needsShuffle;

	public Mesh grassMesh;

	public Grass(Room room, int maxGrassBladesVisible)
	{
		grassBlades = new List<GrassBlade>();
		base.room = room;
		_maxGrassBladesVisible = maxGrassBladesVisible;
		Init();
	}

	public override void Destroy()
	{
		base.Destroy();
		if (grassMesh != null)
		{
			Object.Destroy(grassMesh);
			grassMesh = null;
		}
	}

	public void RoomUnloaded()
	{
		Destroy();
	}

	public void Init()
	{
		BuildMesh(out verts, out tris, _maxGrassBladesVisible, GrassBlade.grassBladeVerts, GrassBlade.grassBladeTris);
	}

	public static void InitGrassInRoom(Room room)
	{
		if (room.grass == null)
		{
			room.grass = new Grass(room, maxGrassBladesVisible);
			room.AddObject(room.grass);
		}
	}

	public static void BuildMesh(out Vector3[] verts, out int[] tris, int numberOfMeshes, int meshVerts, int[] meshTris)
	{
		verts = new Vector3[meshVerts * numberOfMeshes];
		tris = new int[meshTris.Length * numberOfMeshes];
		for (int i = 0; i < numberOfMeshes; i++)
		{
			for (int j = 0; j < meshTris.Length; j++)
			{
				tris[i * meshTris.Length + j] = meshTris[j] + i * meshVerts;
			}
		}
	}

	public static void SetUVMaps(ref MaskSource ms, ref TriangleMesh spriteMesh, int nMeshes, int meshVerts)
	{
		Vector2[] array = new Vector2[meshVerts];
		int num = meshVerts / 2;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 2;
			float num3 = (float)i / (float)(num - 1);
			array[num2] = Vector2.up * num3;
			array[num2 + 1] = array[num2] + Vector2.right;
		}
		for (int j = 0; j < nMeshes; j++)
		{
			for (int k = 0; k < array.Length; k++)
			{
				spriteMesh.UVvertices[j * meshVerts + k] = array[k];
			}
		}
		spriteMesh.UVvertices.CopyTo(ms.meshFilter.sharedMesh.uv, 0);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (needsShuffle)
		{
			grassBlades.Shuffle();
			needsShuffle = false;
		}
		for (int i = 0; i < grassBlades.Count; i++)
		{
			if (grassBlades[i] != null)
			{
				grassBlades[i].Update();
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (!base.slatedForDeletetion)
		{
			sLeaser.sprites = new FSprite[1];
			int num = tris.Length / 3;
			TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[num];
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 3;
				array[i] = new TriangleMesh.Triangle(tris[num2], tris[num2 + 1], tris[num2 + 2]);
			}
			TriangleMesh spriteMesh = new TriangleMesh("Futile_White", array, customColor: true);
			sLeaser.sprites[0] = spriteMesh;
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["GrassColor"];
			rCam.ReturnFContainer("Sand").AddChild(sLeaser.sprites[0]);
			sLeaser.maskSources = new MaskSource[1];
			grassMesh = new Mesh
			{
				name = "Grass Mesh",
				vertices = verts,
				triangles = tris,
				uv = new Vector2[verts.Length],
				colors = new Color[verts.Length]
			};
			sLeaser.maskSources[0] = MaskMaker.MakeSource("DynamicLevelElementGrab", "DynamicLevelGrass", isSecondLayer: false, grassMesh);
			SetUVMaps(ref sLeaser.maskSources[0], ref spriteMesh, _maxGrassBladesVisible, GrassBlade.grassBladeVerts);
			DynamicLevelElement.AddLevelCombiner(rCam);
			if (Camera.main.depthTextureMode == DepthTextureMode.None)
			{
				Camera.main.depthTextureMode = DepthTextureMode.Depth;
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (base.slatedForDeletetion)
		{
			if (sLeaser.maskSources.Length != 0)
			{
				sLeaser.maskSources = new MaskSource[0];
			}
		}
		else
		{
			sLeaser.maskSources[0].setGameObjectPos = Vector2.zero - camPos + Vector2.down * 5f;
			sLeaser.sprites[0].SetPosition(Vector2.right - camPos + Vector2.down * 5f);
			TriangleMesh spriteMesh = sLeaser.sprites[0] as TriangleMesh;
			UpdateMesh(sLeaser.maskSources[0].meshFilter.sharedMesh, spriteMesh, timeStacker);
		}
	}

	public void UpdateMesh(Mesh mesh, TriangleMesh spriteMesh, float timeStacker)
	{
		if (base.slatedForDeletetion)
		{
			return;
		}
		int num = 0;
		Color[] col = mesh.colors;
		for (int i = 0; i < grassBlades.Count; i++)
		{
			if (num >= _maxGrassBladesVisible)
			{
				break;
			}
			if (grassBlades[i] != null && room.game.cameras[0].PositionCurrentlyVisible(grassBlades[i].pos, 20f, widescreen: true))
			{
				grassBlades[i].UpdateBladeMesh(ref verts, ref spriteMesh, ref col, num, timeStacker);
				num++;
			}
		}
		for (int j = num; j < _maxGrassBladesVisible; j++)
		{
			GrassBlade.ZeroOut(ref verts, ref spriteMesh.vertices, j);
		}
		mesh.vertices = verts;
		mesh.colors = col;
		mesh.RecalculateBounds();
	}
}
