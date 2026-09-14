using UnityEngine;

namespace Watcher;

public class AncientUrbanView : BackgroundScene
{
	public class Sky : Simple2DBackgroundIllustration
	{
		public Color skyColor = Color.white;

		public Sky(BackgroundScene scene, string illustrationName, Vector2 pos)
			: base(scene, illustrationName, pos)
		{
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].color = skyColor;
			sLeaser.sprites[0].alpha = alpha;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			skyColor = palette.skyColor;
		}
	}

	public class Building : BackgroundSceneElement
	{
		public string assetName;

		public float scale;

		public float rotation;

		public float thickness;

		public AncientUrbanView auScene => scene as AncientUrbanView;

		public Building(AncientUrbanView scene, string assetName, Vector2 pos, float depth, float scale, float rotation, float thickness)
			: base(scene, pos, depth)
		{
			this.assetName = assetName;
			this.scale = scale;
			this.rotation = rotation;
			this.thickness = thickness;
		}

		public float getDepthForLayer(float layer)
		{
			return depth + layer * thickness;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[3];
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i] = new FSprite(assetName);
				sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["AncientUrbanBuilding"];
				sLeaser.sprites[i].anchorY = 0f;
				sLeaser.sprites[i].scale = scale * (1f / getDepthForLayer(1f - (float)i / 3f));
				sLeaser.sprites[i].rotation = rotation;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				float depthForLayer = getDepthForLayer(1f - (float)i / 3f);
				Vector2 vector = scene.DrawPos(pos, depthForLayer, camPos, rCam.hDisplace);
				sLeaser.sprites[i].x = vector.x;
				sLeaser.sprites[i].y = vector.y;
				sLeaser.sprites[i].color = new Color(auScene.AtmosphereColorAtDepth(depthForLayer), 1f - (float)i / 3f, depthForLayer / 8.2f);
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class Smoke : BackgroundSceneElement
	{
		public float flattened;

		public float alpha;

		public float shaderInputColor;

		public float randomOffset;

		public bool shaderType;

		public Smoke(AncientUrbanView scene, Vector2 pos, float depth, int index, float flattened, float alpha, float shaderInputColor, bool shaderType)
			: base(scene, pos, depth)
		{
			this.flattened = flattened;
			this.alpha = alpha;
			this.shaderInputColor = shaderInputColor;
			this.shaderType = shaderType;
			randomOffset = Random.value;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("smoke1");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders[shaderType ? "Dust" : "CloudDistant"];
			sLeaser.sprites[0].anchorY = 0f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = 2f;
			float y = DrawPos(camPos, rCam.hDisplace).y;
			sLeaser.sprites[0].scaleY = flattened * num;
			sLeaser.sprites[0].scaleX = num;
			sLeaser.sprites[0].color = new Color(shaderInputColor, randomOffset, Mathf.Lerp(flattened, 1f, 0.5f), alpha);
			sLeaser.sprites[0].x = 683f - rCam.hDisplace;
			sLeaser.sprites[0].y = y;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class SmokeGradient : BackgroundSceneElement
	{
		public string assetName;

		public float randomOffset;

		public AncientUrbanView auScene => scene as AncientUrbanView;

		public SmokeGradient(AncientUrbanView scene, Vector2 pos, float depth)
			: base(scene, pos, depth)
		{
			randomOffset = Random.value;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("au_dustgradient");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DustGradient"];
			sLeaser.sprites[0].anchorY = 0.5f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = 2f;
			float y = DrawPos(camPos, rCam.hDisplace).y;
			sLeaser.sprites[0].scaleY = num * 1.5f;
			sLeaser.sprites[0].scaleX = num * 12f;
			sLeaser.sprites[0].color = new Color(randomOffset, camPos.x, auScene.AtmosphereColorAtDepth(depth), 1f);
			sLeaser.sprites[0].x = 683f + rCam.hDisplace;
			sLeaser.sprites[0].y = y;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public RoomSettings.RoomEffect effect;

	public float floorLevel = -2000f;

	public Color atmosphereColor = new Color(0.16078432f, 0.23137255f, 27f / 85f);

	public AncientUrbanView(Room room, RoomSettings.RoomEffect effect)
		: base(room)
	{
		this.effect = effect;
		float num = 300f;
		sceneOrigo = RoomToWorldPos(room.abstractRoom.size.ToVector2() * 10f);
		Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
		Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, atmosphereColor);
		Shader.SetGlobalVector(RainWorld.ShadPropSceneOrigoPosition, sceneOrigo);
		Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
		Random.State state = Random.state;
		Random.InitState(0);
		AddElement(new Sky(this, "au_sky", new Vector2(683f, 384f)));
		float num2 = 6500f;
		for (int i = 0; i < 400; i++)
		{
			float num3 = Random.Range(1.2f, 8f);
			Building element = new Building(pos: new Vector2(PosFromDrawPosAtNeutralCamPos(new Vector2(Random.Range(0f - num2, num2), 0f), 1f).x, floorLevel + num * num3 + Random.Range(-300f, 0f)), scene: this, assetName: "au_buildingp_" + Random.Range(0, 16), depth: num3, scale: Random.Range(0.75f, 1.75f), rotation: Random.Range(-1.5f, 1.5f), thickness: 0.1f);
			AddElement(element);
		}
		LoadGraphic("smoke1", crispPixels: false, clampWrapMode: false);
		LoadGraphic("au_dustgradient", crispPixels: false, clampWrapMode: false);
		int num4 = 13;
		float num5 = floorLevel;
		for (int j = 1; j < num4; j++)
		{
			float num6 = (float)j / 13f;
			AddElement(new Smoke(this, new Vector2(0f, num5 + 14f + num * num6 + 1000f), num6 * 8f, 0, 3f, 0.1f, 0f, shaderType: true));
			AddElement(new SmokeGradient(this, new Vector2(0f, floorLevel + num * num6 + 1000f), num6 * 8f + 0.1f));
		}
		AddElement(new Smoke(this, new Vector2(0f, num5 + 560f), 9f, 0, 2.5f, 0.1f, 0.8f, shaderType: false));
		AddElement(new SmokeGradient(this, new Vector2(0f, -200f), 1f));
		Random.state = state;
	}

	public float AtmosphereColorAtDepth(float depth)
	{
		return Mathf.Clamp(depth / 8.2f, 0f, 1f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}

	public override void Destroy()
	{
		base.Destroy();
	}
}
