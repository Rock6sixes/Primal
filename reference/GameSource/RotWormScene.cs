using System;
using System.Collections.Generic;
using UnityEngine;

public class RotWormScene : BackgroundScene
{
	public class PebbsGrid : BackgroundSceneElement
	{
		public struct MeshInfo
		{
			public TriangleMesh.Triangle[] tris;

			public Vector2[] uvs;

			public MeshInfo(TriangleMesh.Triangle[] tris, Vector2[] uvs)
			{
				this.tris = tris;
				this.uvs = uvs;
			}
		}

		public float scale;

		public bool perpendicular;

		public Texture2D ueGridTex;

		public Dictionary<float, MeshInfo> meshCache;

		public static WeakReference<Dictionary<float, MeshInfo>> sharedMeshCache;

		public RotWormScene rwScene => scene as RotWormScene;

		public PebbsGrid(RotWormScene scene, float x, float y, float depth, float scale, bool perpendicular)
			: base(scene, new Vector2(x * sceneScale, y * sceneScale), depth)
		{
			this.scale = scale;
			this.perpendicular = perpendicular;
			ueGridTex = (Texture2D)Futile.atlasManager.GetAtlasWithName("ue_grid").texture;
			if (sharedMeshCache == null || !sharedMeshCache.TryGetTarget(out meshCache))
			{
				meshCache = new Dictionary<float, MeshInfo>();
				sharedMeshCache = new WeakReference<Dictionary<float, MeshInfo>>(meshCache);
			}
		}

		public Vector2 GetTexCoord(Vector2 uv, float precision)
		{
			Texture2D noiseTex = room.game.rainWorld._NoiseTex;
			float num = depth / fogDepth;
			float b = ueGridTex.GetPixelBilinear(uv.x * 5f + num * 100f, uv.y * 5f + num * 100f).b;
			b = b * 2f - 1f;
			float b2 = ueGridTex.GetPixelBilinear(uv.x * 5f + num * 100f + b * 0.005f, uv.y * 0.25f * 5f + num * 100f + b * 0.005f).b;
			b2 = b2 * 2f - 1f;
			float r = noiseTex.GetPixelBilinear(uv.x + num, uv.y + num).r;
			r = r * 2f - 1f;
			return uv + (b2 * 0.001f * precision + r * 0.05f) * Vector2.one;
		}

		public Vector2 GetUV(Vector2 texCoord)
		{
			Vector2 vector = texCoord;
			Vector2 vector2 = default(Vector2);
			for (int i = 0; i < 16; i++)
			{
				float precision = i / 16;
				float num = Vector2.Distance(GetTexCoord(vector, precision), texCoord);
				float num2 = 0.01f;
				vector2.x = Vector2.Distance(GetTexCoord(vector + new Vector2(num2, 0f), precision), texCoord) - num;
				vector2.y = Vector2.Distance(GetTexCoord(vector + new Vector2(0f, num2), precision), texCoord) - num;
				vector2.Normalize();
				vector -= vector2 * (num * 0.5f);
			}
			return vector;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			MeshInfo value;
			if (perpendicular)
			{
				sLeaser.sprites[0] = new FSprite("ue_grid");
			}
			else if (meshCache.TryGetValue(depth, out value))
			{
				TriangleMesh triangleMesh = new TriangleMesh("ue_grid", value.tris, customColor: false);
				value.uvs.CopyTo(triangleMesh.UVvertices, 0);
				sLeaser.sprites[0] = triangleMesh;
				for (int i = 0; i < triangleMesh.vertices.Length; i++)
				{
					triangleMesh.vertices[i] = (triangleMesh.UVvertices[i] - new Vector2(0.5f, 0.5f)) * 512f;
				}
			}
			else
			{
				TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[128];
				for (int j = 0; j < array.Length; j += 2)
				{
					int num = j * 2;
					array[j] = new TriangleMesh.Triangle(num, num + 1, num + 2);
					array[j + 1] = new TriangleMesh.Triangle(num, num + 2, num + 3);
				}
				TriangleMesh triangleMesh2 = new TriangleMesh("ue_grid", array, customColor: false);
				sLeaser.sprites[0] = triangleMesh2;
				float num2 = 0.01f;
				for (int k = 0; k < 8; k++)
				{
					for (int l = 0; l < 8; l++)
					{
						int num3 = (k * 8 + l) * 4;
						Vector2 uV = GetUV(new Vector2((float)k + 0.5f, (float)l + 0.5f) / 8f);
						uV.x += num2 * 0.5f;
						uV.y += num2 * 0.5f;
						triangleMesh2.UVvertices[num3] = new Vector2(uV.x - num2, uV.y - num2);
						triangleMesh2.UVvertices[num3 + 1] = new Vector2(uV.x - num2, uV.y + num2);
						triangleMesh2.UVvertices[num3 + 2] = new Vector2(uV.x + num2, uV.y + num2);
						triangleMesh2.UVvertices[num3 + 3] = new Vector2(uV.x + num2, uV.y - num2);
					}
				}
				for (int m = 0; m < triangleMesh2.vertices.Length; m++)
				{
					triangleMesh2.vertices[m] = (triangleMesh2.UVvertices[m] - new Vector2(0.5f, 0.5f)) * 512f;
				}
				meshCache[depth] = new MeshInfo(array, triangleMesh2.UVvertices);
			}
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["GreebleGrid"];
			sLeaser.sprites[0].scale = scale * rwScene.DrawScale(this) * sceneScale;
			sLeaser.sprites[0].color = new Color(perpendicular ? 0f : 1f, 1f, 1f, depth / fogDepth);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = rwScene.DrawPos(this, camPos);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public RoomSettings.RoomEffect effect;

	public static float fogDepth = 100f;

	public static float sceneScale = 1f;

	public static float depthScale = 1f;

	public Vector2 perspectiveCenter;

	public RotWormScene(Room room)
		: base(room)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(123);
		AddElement(new RotWorm(this, default(Vector2), 1f));
		Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, Color.black);
		perspectiveCenter = new Vector2(room.game.rainWorld.screenSize.x / 2f, room.game.rainWorld.screenSize.y / 2f);
		sceneOrigo = new Vector2(10f, -10f) * room.abstractRoom.size.ToVector2();
		sceneScale = 160f;
		depthScale = 1.5f;
		int num = 24;
		int num2 = 2;
		float num3 = 8 * (num + num2);
		int num4 = 8;
		LoadGraphic("ue_grid", crispPixels: true, clampWrapMode: false);
		for (int i = 0; i < num4; i++)
		{
			for (int j = 0; j < num + num2; j++)
			{
				float num5 = (float)(i * (num + num2) + j) / num3;
				AddElement(new PebbsGrid(this, 0f, 0f, num5 * fogDepth, 1f, j > num));
			}
		}
		UnityEngine.Random.state = state;
	}

	public Vector2 DrawPos(BackgroundSceneElement element, Vector2 camPos)
	{
		float num = DrawScale(element);
		float num2 = element.pos.x - (camPos.x + sceneOrigo.x);
		return new Vector2(y: (element.pos.y - (camPos.y + sceneOrigo.y)) * num + perspectiveCenter.y, x: num2 * num + perspectiveCenter.x);
	}

	public float DrawScale(BackgroundSceneElement element)
	{
		return 1f / (element.depth * depthScale + 1f);
	}
}
