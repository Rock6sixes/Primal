using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class VoidWeaverEndingScene : BackgroundScene
{
	public class BackgroundColor : BackgroundSceneElement
	{
		public VoidWeaverEndingScene Scene => scene as VoidWeaverEndingScene;

		public BackgroundColor(VoidWeaverEndingScene scene)
			: base(scene, default(Vector2), float.PositiveInfinity)
		{
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[4]
			{
				new TriangleMesh.Triangle(0, 1, 3),
				new TriangleMesh.Triangle(0, 3, 2),
				new TriangleMesh.Triangle(2, 3, 5),
				new TriangleMesh.Triangle(2, 5, 4)
			};
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new TriangleMesh("Futile_White", tris, customColor: true);
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["RippleBasicBothSides"];
			AddToContainer(sLeaser, rCam, null);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Shadows");
			}
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Color color = Scene.SkyColor(timeStacker);
			Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, color);
			Shader.SetGlobalVector(RainWorld.ShadPropWorldCamPos, camPos - scene.sceneOrigo);
			TriangleMesh obj = sLeaser.sprites[0] as TriangleMesh;
			obj.verticeColors[0] = color;
			obj.verticeColors[1] = color;
			obj.verticeColors[2] = color;
			obj.verticeColors[3] = color;
			obj.verticeColors[4] = Color.Lerp(color, Color.white, 0.2f);
			obj.verticeColors[5] = Color.Lerp(color, Color.white, 0.2f);
			float x = rCam.sSize.x;
			float y = rCam.sSize.y;
			obj.MoveVertice(0, new Vector2(0f, 0f));
			obj.MoveVertice(1, new Vector2(x + 1f, 0f));
			obj.MoveVertice(2, new Vector2(0f, y / 2f));
			obj.MoveVertice(3, new Vector2(x + 1f, y / 2f));
			obj.MoveVertice(4, new Vector2(0f, y + 1f));
			obj.MoveVertice(5, new Vector2(x + 1f, y + 1f));
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class Floor : BackgroundSceneElement
	{
		public float minDepth;

		public float maxDepth;

		public Floor(VoidWeaverEndingScene scene, float y, float minDepth, float maxDepth)
			: base(scene, new Vector2(0f, y), 100000f)
		{
			this.minDepth = minDepth;
			this.maxDepth = maxDepth;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[2]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(0, 2, 3)
			};
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: true);
			triangleMesh.shader = rCam.game.rainWorld.Shaders["VWEndingFloor"];
			sLeaser.sprites[0] = triangleMesh;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			TriangleMesh obj = sLeaser.sprites[0] as TriangleMesh;
			float x = (0f - rCam.sSize.x) * maxDepth;
			float x2 = rCam.sSize.x * maxDepth;
			float y = scene.DrawPos(pos, 1f, camPos, rCam.hDisplace).y;
			obj.MoveVertice(0, new Vector2(x, y));
			obj.MoveVertice(1, new Vector2(x2, y));
			obj.MoveVertice(2, new Vector2(x2, y));
			obj.MoveVertice(3, new Vector2(x, y));
			obj.verticeColors[0].a = minDepth;
			obj.verticeColors[1].a = minDepth;
			obj.verticeColors[2].a = maxDepth;
			obj.verticeColors[3].a = maxDepth;
			obj.UVvertices[0] = new Vector2(0f, 0f);
			obj.UVvertices[1] = new Vector2(1f, 0f);
			obj.UVvertices[2] = new Vector2(1f, 1f);
			obj.UVvertices[3] = new Vector2(0f, 1f);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Shadows");
			}
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public class HazeCloud : BackgroundSceneElement
	{
		public float height;

		public float baseBrightness;

		public VoidWeaverEndingScene Scene => scene as VoidWeaverEndingScene;

		public HazeCloud(VoidWeaverEndingScene scene, Vector2 pos, float depth, float height)
			: base(scene, pos, depth)
		{
			this.height = height;
			baseBrightness = UnityEngine.Random.value;
		}

		public virtual Color GetColor(float timeStacker)
		{
			float a = Mathf.Lerp(Scene.lastBrightness, Scene.brightness, timeStacker);
			return Color.Lerp(darkHazeColor, lightHazeColor, Mathf.Lerp(a, baseBrightness, 0.1f));
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[2]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(0, 2, 3)
			};
			TriangleMesh triangleMesh = new TriangleMesh("clouds" + UnityEngine.Random.Range(1, 4), tris, customColor: false);
			triangleMesh.shader = rCam.game.rainWorld.Shaders["VWEndingHaze"];
			triangleMesh.UVvertices[0].y = 0f;
			triangleMesh.UVvertices[1].y = 0f;
			triangleMesh.UVvertices[2].y = 1f;
			triangleMesh.UVvertices[3].y = 1f;
			sLeaser.sprites[0] = triangleMesh;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			TriangleMesh obj = sLeaser.sprites[0] as TriangleMesh;
			float num = camPos.x - rCam.sSize.x * depth;
			float num2 = camPos.x + rCam.sSize.x * (depth + 1f);
			obj.MoveVertice(0, scene.DrawPos(new Vector2(num, pos.y), depth, camPos, rCam.hDisplace));
			obj.MoveVertice(1, scene.DrawPos(new Vector2(num2, pos.y), depth, camPos, rCam.hDisplace));
			obj.MoveVertice(2, scene.DrawPos(new Vector2(num2, pos.y + height), depth, camPos, rCam.hDisplace));
			obj.MoveVertice(3, scene.DrawPos(new Vector2(num, pos.y + height), depth, camPos, rCam.hDisplace));
			float num3 = 0.0002f * (1000f / height);
			obj.UVvertices[0].x = (pos.x + num) * num3;
			obj.UVvertices[1].x = (pos.x + num2) * num3;
			obj.UVvertices[2].x = (pos.x + num2) * num3;
			obj.UVvertices[3].x = (pos.x + num) * num3;
			obj.color = GetColor(timeStacker);
			obj.alpha = Mathf.InverseLerp(maxCloudDepth, maxCloudDepth / 2f, depth);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Shadows");
			}
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}
	}

	public class ForegroundHazeCloud : HazeCloud
	{
		public ForegroundHazeCloud(VoidWeaverEndingScene scene, Vector2 pos, float depth, float height)
			: base(scene, pos, depth, height)
		{
		}

		public override Color GetColor(float timeStacker)
		{
			return Color.Lerp(base.GetColor(timeStacker), Color.black, 0.2f);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			sLeaser.sprites[0].alpha *= 0.5f;
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
		}
	}

	public class Shard : BackgroundSceneElement
	{
		public Quaternion lastRotation;

		public Quaternion rotation;

		public Vector3 angularVelocity;

		public Vector3[] vertices;

		public static Vector2Int mosaicSize = new Vector2Int(3, 7);

		public Vector2Int mosaicPos;

		public int flash;

		public int flashFrames = 20;

		public Shard(VoidWeaverEndingScene scene, Vector2 pos, float depth)
			: base(scene, pos, depth)
		{
			angularVelocity = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(1f, 10f);
			rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.onUnitSphere);
			lastRotation = rotation;
			vertices = new Vector3[3];
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < vertices.Length; i++)
			{
				float f = (float)i / (float)vertices.Length * (float)Math.PI * 2f;
				float num = UnityEngine.Random.Range(20f, Custom.LerpMap(depth, 0f, 2f, 40f, (i == 0) ? 500f : 200f));
				vertices[i] = new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0f) * num;
				zero += vertices[i];
			}
			zero /= (float)vertices.Length;
			for (int j = 0; j < vertices.Length; j++)
			{
				vertices[j] -= zero;
			}
			ref Vector2Int reference = ref mosaicPos;
			Vector2Int vector2Int = mosaicSize;
			reference.x = UnityEngine.Random.Range(0, vector2Int.x);
			ref Vector2Int reference2 = ref mosaicPos;
			vector2Int = mosaicSize;
			reference2.y = UnityEngine.Random.Range(0, vector2Int.y);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastRotation = rotation;
			rotation = Quaternion.AngleAxis(angularVelocity.magnitude / 40f, angularVelocity) * rotation;
			rotation.Normalize();
			if (flash > 0)
			{
				flash--;
			}
			if (flash == 0 && UnityEngine.Random.value < 0.004f)
			{
				flashFrames = UnityEngine.Random.Range(20, 40);
				flash = flashFrames;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
			{
				new TriangleMesh.Triangle(0, 1, 2)
			};
			sLeaser.sprites = new FSprite[1];
			TriangleMesh triangleMesh = new TriangleMesh("watcher_mosaic", tris, customColor: true)
			{
				shader = rCam.game.rainWorld.Shaders["VWEndingShard"]
			};
			sLeaser.sprites[0] = triangleMesh;
			for (int i = 0; i < triangleMesh.verticeColors.Length; i++)
			{
				triangleMesh.verticeColors[i] = new Color(vertices[i].x / 500f * 0.5f + 0.5f, vertices[i].y / 500f * 0.5f + 0.5f, 0f, 1f - depth / 15f);
			}
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer((depth < 1f) ? "Foreground" : "Shadows"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Quaternion quaternion = Quaternion.Slerp(lastRotation, rotation, timeStacker);
			TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
			Rect rectangle = new Rect((Vector2)mosaicPos / (Vector2)mosaicSize, Vector2.one / mosaicSize);
			for (int i = 0; i < triangleMesh.vertices.Length; i++)
			{
				Vector3 vector = quaternion * vertices[i] + (Vector3)pos;
				float num = vector.z / 800f + depth;
				Vector2 vector2 = scene.DrawPos(vector, num, camPos, rCam.hDisplace);
				triangleMesh.MoveVertice(i, vector2);
				triangleMesh.UVvertices[i] = Rect.NormalizedToPoint(rectangle, vector2 / rCam.sSize);
				triangleMesh.verticeColors[i].b = Mathf.Max(0f, ((float)flash - timeStacker) / (float)flashFrames);
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class GoldenThread : BackgroundSceneElement
	{
		public Vector3 posA;

		public Vector3 posB;

		public float slack;

		public float flashOffset = UnityEngine.Random.value;

		public VoidWeaverEndingScene Scene => scene as VoidWeaverEndingScene;

		public GoldenThread(VoidWeaverEndingScene scene, Vector3 posA, Vector3 posB, float slack)
			: base(scene, Vector2.Lerp(new Vector2(posA.x, posA.y), new Vector2(posB.x, posB.y), 0.5f), Mathf.Min(posA.z, posB.z))
		{
			this.posA = posA;
			this.posB = posB;
			this.slack = slack;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(32, pointyTip: false, customColor: true);
			triangleMesh.shader = rCam.game.rainWorld.Shaders["BasicAdditive"];
			sLeaser.sprites[0] = triangleMesh;
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Shadows"));
		}

		public Vector2 EvaluateCurve(float t, Vector2 camPos, float horizontalDisplacement)
		{
			Vector3 vector = Vector3.LerpUnclamped(posA, posB, t) + Vector3.up * ((4f * t * t - 4f * t) * slack);
			return scene.DrawPos(new Vector2(vector.x, vector.y), vector.z, camPos, horizontalDisplacement);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
			Vector2 vector = EvaluateCurve(-0.1f, camPos, rCam.hDisplace);
			float num = (((float)rCam.game.clock + timeStacker) * 0.01f + flashOffset) % 2f;
			for (int i = 0; i < triangleMesh.vertices.Length; i += 2)
			{
				float num2 = (float)i / 2f / ((float)triangleMesh.vertices.Length / 2f - 1f);
				Vector2 vector2 = EvaluateCurve(num2, camPos, rCam.hDisplace);
				Vector2 vector3 = Vector2.Perpendicular((vector2 - vector).normalized);
				triangleMesh.MoveVertice(i, vector2 + vector3);
				triangleMesh.MoveVertice(i + 1, vector2 - vector3);
				float t = Mathf.Cos((float)Math.PI * 2f * Mathf.Clamp01(20f * (num2 - (1f - num)))) * -0.5f + 0.5f;
				Color goldRGB = RainWorld.GoldRGB;
				goldRGB = Color.Lerp(goldRGB, Color.white, t);
				goldRGB.a = (0f - (4f * num2 * num2 - 4f * num2)) * Mathf.Clamp01(1f - Vector3.Lerp(posA, posB, num2).z / 30f) * 0.05f;
				triangleMesh.verticeColors[i] = goldRGB;
				triangleMesh.verticeColors[i + 1] = goldRGB;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class PlayerShadow : CosmeticSprite
	{
		public Player player;

		public VoidWeaverEndingScene scene;

		public PlayerShadow(Player player, VoidWeaverEndingScene scene)
		{
			this.player = player;
			this.scene = scene;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (player.slatedForDeletetion || player.room != room)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[2]
			{
				new TriangleMesh.Triangle(0, 1, 2),
				new TriangleMesh.Triangle(0, 2, 3)
			};
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
			triangleMesh.shader = rCam.game.rainWorld.Shaders["FlatLight"];
			triangleMesh.color = Color.black;
			triangleMesh.UVvertices[0] = new Vector2(0f, 0f);
			triangleMesh.UVvertices[1] = new Vector2(1f, 0f);
			triangleMesh.UVvertices[2] = new Vector2(1f, 1f);
			triangleMesh.UVvertices[3] = new Vector2(0f, 1f);
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = triangleMesh;
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			TriangleMesh obj = sLeaser.sprites[0] as TriangleMesh;
			Vector2 vector = Vector2.Lerp(player.bodyChunks[0].lastPos, player.bodyChunks[0].pos, timeStacker) + Vector2.Lerp(player.bodyChunks[1].lastPos, player.bodyChunks[1].pos, timeStacker);
			vector /= 2f;
			float y = 800f;
			Vector2 normalized = (new Vector2(vector.x, y) - new Vector2(320f, 1500f)).normalized;
			Vector2 vector2 = Vector2.Perpendicular(normalized);
			float num = Mathf.Max(vector.y - 130f, 0f);
			float num2 = 100f;
			float num3 = 35f + Mathf.Sqrt(num / 2f);
			float num4 = num - 20f;
			obj.MoveVertice(0, FloorToScreen(new Vector2(vector.x, y) - vector2 * num3 + normalized * (num2 + num4)));
			obj.MoveVertice(1, FloorToScreen(new Vector2(vector.x, y) + vector2 * num3 + normalized * (num2 + num4)));
			obj.MoveVertice(2, FloorToScreen(new Vector2(vector.x, y) + vector2 * num3 + normalized * (0f - num3 + num4)));
			obj.MoveVertice(3, FloorToScreen(new Vector2(vector.x, y) - vector2 * num3 + normalized * (0f - num3 + num4)));
			obj.alpha = (Mathf.Lerp(scene.lastBrightness, scene.brightness, timeStacker) * 0.1f + 0.1f) / (num / 20f + 1f);
			Vector2 FloorToScreen(Vector2 pos)
			{
				Vector2 vector3 = rCam.sSize / 2f;
				return (new Vector2(pos.x, 120f) - camPos - vector3) / (pos.y / 800f) + vector3;
			}
		}
	}

	public class FallingFeather : CosmeticSprite
	{
		public Counter life;

		public Vector2 direction;

		public Vector2 lastDirection;

		public int time;

		public bool reachedFloor;

		public float speed;

		public float rotation;

		public float random;

		public FallingFeather(Room room, Vector2 pos, int life)
		{
			this.life = new Counter(life);
			base.room = room;
			base.pos = pos;
			lastPos = pos;
			direction = Custom.RNV();
			lastDirection = direction;
			random = UnityEngine.Random.value;
		}

		public override void Update(bool eu)
		{
			if (life.isFinished)
			{
				Destroy();
				return;
			}
			time++;
			base.Update(eu);
			lastDirection = direction;
			life.Tick();
			float num = Vector2.Dot(direction, Vector2.down);
			num = num + Mathf.Sign(num) * 0.4f + Mathf.Abs(num) * 0.5f;
			float num2 = FlameJet.AnimatePerlin(random * 9000f + (float)time * 0.05f);
			float num3 = Mathf.InverseLerp(80f, 0f, pos.y);
			float num4 = Mathf.Sign(direction.x);
			speed *= 0.95f + num2 * 0.05f;
			rotation *= 0.92f + random * 0.04f - num2 * 0.02f;
			speed += num * (0.4f + random * 0.3f);
			rotation += (0f - num4) * speed * (0.5f + num2);
			direction = Custom.rotateVectorDeg(direction, rotation * 0.0625f);
			direction = Vector2.Lerp(direction, Vector2.right * num4, num3 * 0.1f);
			vel = direction * ((speed + Mathf.Abs(direction.x) * 2.1f) * 0.25f) + Vector2.down * ((0.3f + random * 0.7f) * 0.7f);
			vel.x += num3 * 2f * num4;
			if (num3 >= 0.95f && !reachedFloor)
			{
				life.Set(40);
				reachedFloor = true;
			}
			vel *= Mathf.InverseLerp(0f, 10f, pos.y);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("WeaverFeather" + UnityEngine.Random.Range(0, 3));
			sLeaser.sprites[0].scale = 0.2f + random * 0.4f;
			sLeaser.sprites[0].color = Color.Lerp(Color.white, RainWorld.SaturatedGold, 0.5f);
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			Vector2 a = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
			sLeaser.sprites[0].SetPosition(Vector2.LerpUnclamped(a, Custom.rainWorld.screenSize * 0.5f, 0.8f - random * 0.85f));
			sLeaser.sprites[0].rotation = Custom.VecToDeg(Vector2.Lerp(lastDirection, direction, timeStacker));
			sLeaser.sprites[0].alpha = Mathf.InverseLerp(0f, 40f, (int)life) * Mathf.InverseLerp(life.max, life.max - 40, (int)life) * (0.1f + random * 0.3f) * (0.5f + a.y / Custom.rainWorld.screenSize.y * 0.5f);
		}
	}

	public VoidWeaver voidWeaver;

	public Player player;

	public Dictionary<Player, PlayerShadow> shadows = new Dictionary<Player, PlayerShadow>();

	public float brightness;

	public float lastBrightness;

	public float origox;

	public static Color darkSkyColor = new Color(0.1f, 0.15f, 0.25f);

	public static Color lightSkyColor = new Color(0.84f, 0.46f, 0.25f);

	public static Color lightHazeColor = Color.Lerp(new Color(0.93f, 0.61f, 0.38f), Color.white, 0.25f);

	public static Color darkHazeColor = Color.Lerp(lightHazeColor, darkSkyColor, 0.75f);

	public static float maxCloudDepth = 17f;

	public Counter featherSpawnCooldown = new Counter(6);

	public VoidWeaverEndingScene(Room room)
		: base(room)
	{
		LoadGraphic("clouds1", crispPixels: false, clampWrapMode: false);
		LoadGraphic("clouds2", crispPixels: false, clampWrapMode: false);
		LoadGraphic("clouds3", crispPixels: false, clampWrapMode: false);
		LoadGraphic("flyingClouds1", crispPixels: false, clampWrapMode: false);
		LoadGraphic("watcher_mosaic", crispPixels: false, clampWrapMode: true);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(2);
		AddElement(new BackgroundColor(this));
		AddElement(new Floor(this, 0f, 0.2f, 1000f));
		origox = -5200f;
		for (int i = 0; i < 7; i++)
		{
			float depth = Mathf.Lerp(1.1f, maxCloudDepth, Mathf.Pow((float)i / 10f, 1.75f));
			float x = UnityEngine.Random.Range(origox - 15000f, origox + 15000f);
			float height = 1000f;
			AddElement(new HazeCloud(this, new Vector2(x, 0f), depth, height));
		}
		AddElement(new ForegroundHazeCloud(this, new Vector2(UnityEngine.Random.Range(origox - 15000f, origox + 15000f), 0f), 0.4f, 300f));
		AddElement(new ForegroundHazeCloud(this, new Vector2(UnityEngine.Random.Range(origox - 15000f, origox + 15000f), 0f), 0.6f, 300f));
		for (int j = 0; j < 76; j++)
		{
			bool flag = j >= 36;
			Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
			float magnitude = insideUnitCircle.magnitude;
			Vector2 vector = insideUnitCircle * 8000f + new Vector2(310f, 0f);
			vector.y = Mathf.Abs(vector.y) + 0.2f;
			float y = 80f + UnityEngine.Random.value * Mathf.Lerp(4000f, 200f, magnitude);
			AddElement(new Shard(this, new Vector2(vector.x + (flag ? (origox * 2f) : 0f), y), vector.y / 800f));
		}
		for (int k = 0; k < 100; k++)
		{
			bool flag2 = k >= 50;
			Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
			if (normalized.y < 0f)
			{
				normalized.y *= -1f;
			}
			if (((normalized.x > 0f && !flag2) || normalized.x < 0f) && UnityEngine.Random.value > 0.3f)
			{
				normalized.x *= -1f;
			}
			float value = UnityEngine.Random.value;
			float num = Mathf.Lerp(5000f, 200f, value);
			float t = Mathf.Clamp01(value - UnityEngine.Random.value * 0.5f);
			float num2 = Mathf.Lerp(5000f, 200f, t) + 200f;
			AddElement(new GoldenThread(this, new Vector3(normalized.x * Mathf.Lerp(1000f, 20000f, value) + (flag2 ? (origox * 2f) : 0f), num, normalized.y * Mathf.Lerp(2f, 50f, value)), new Vector3(normalized.x * Mathf.Lerp(0f, 20000f, t) + (flag2 ? (origox * 2f) : 0f), num2, normalized.y * Mathf.Lerp(1.5f, 50f, t)), Mathf.Min(UnityEngine.Random.value * 4000f, Mathf.Lerp(num, num2, 0.5f) - 100f)));
		}
		sceneOrigo = new Vector2(0f, -30f);
		UnityEngine.Random.state = state;
		room.AddObject(new RippleSimpleOverlay());
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		convergencePoint = new Vector2(room.game.rainWorld.screenSize.x / 2f, room.game.rainWorld.screenSize.y / 2f);
		if (player != null && (player.slatedForDeletetion || player.room != room))
		{
			player = null;
		}
		if (!room.game.GetStorySession.finalWarpSequenceStarted && voidWeaver == null)
		{
			voidWeaver = room.updateList.OfType<VoidWeaver>().FirstOrDefault();
		}
		if (player == null)
		{
			player = room.game.FirstRealizedPlayer;
		}
		lastBrightness = brightness;
		if (player != null && voidWeaver != null)
		{
			if (player.mainBodyChunk.pos.x < origox && voidWeaver.pos.x > origox)
			{
				voidWeaver.SetBodyPos(new Vector2(origox - 5500f, voidWeaver.pos.y));
			}
			else if (player.mainBodyChunk.pos.x > origox && voidWeaver.pos.x < origox)
			{
				voidWeaver.SetBodyPos(new Vector2(origox + 5500f, voidWeaver.pos.y));
			}
			brightness = Mathf.InverseLerp(4000f, 1000f, Vector2.Distance(player.mainBodyChunk.pos, voidWeaver.pos));
		}
		if (room.game.GetStorySession.finalWarpSequenceStarted)
		{
			brightness = 1f;
			SpawnFeathers();
		}
		foreach (KeyValuePair<Player, PlayerShadow> shadow in shadows)
		{
			if (shadow.Key.slatedForDeletetion || shadow.Value.slatedForDeletetion)
			{
				shadows.Remove(shadow.Key);
				break;
			}
		}
		foreach (Player item in room.PlayersInRoom)
		{
			if (item != null && !shadows.ContainsKey(item))
			{
				PlayerShadow playerShadow = new PlayerShadow(item, this);
				room.AddObject(playerShadow);
				shadows.Add(item, playerShadow);
			}
		}
	}

	public void SpawnFeathers()
	{
		if (room != null)
		{
			featherSpawnCooldown.Tick();
			if (featherSpawnCooldown.isFinished)
			{
				featherSpawnCooldown.Reset();
				Vector2 pos = room.game.cameras[0].pos;
				Vector2 pos2 = new Vector2(UnityEngine.Random.Range(pos.x - Custom.rainWorld.screenSize.x, pos.x + Custom.rainWorld.screenSize.x * 2f), UnityEngine.Random.Range(pos.y + Custom.rainWorld.screenSize.y * 0.6f, pos.y + Custom.rainWorld.screenSize.y * 1.3f));
				room.AddObject(new FallingFeather(room, pos2, 1400));
			}
		}
	}

	public Color SkyColor(float timeStacker)
	{
		return Color.Lerp(darkSkyColor, lightSkyColor, Mathf.Lerp(lastBrightness, brightness, timeStacker));
	}
}
