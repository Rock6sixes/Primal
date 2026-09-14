using System.Collections.Generic;
using UnityEngine;

namespace Watcher;

public class InnerOuterRimView : BackgroundScene
{
	public class Building : BackgroundSceneElement
	{
		public string assetName;

		public float scale;

		public float rotation;

		public float thickness;

		public int layers;

		public InnerOuterRimView auScene => scene as InnerOuterRimView;

		public Building(InnerOuterRimView scene, string assetName, Vector2 pos, float depth, float scale, float rotation, float thickness, int layers)
			: base(scene, pos, depth)
		{
			this.assetName = assetName;
			this.scale = scale;
			this.rotation = rotation;
			this.thickness = thickness;
			this.layers = layers;
			scene.LoadGraphic(assetName, crispPixels: true, clampWrapMode: false);
		}

		public float getDepthForLayer(float layer)
		{
			return depth + layer * thickness;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[layers];
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i] = new FSprite(assetName);
				sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["AncientUrbanBuilding"];
				sLeaser.sprites[i].scale = scale * (1f / getDepthForLayer(1f - (float)i / (float)layers));
				sLeaser.sprites[i].rotation = rotation;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				float depthForLayer = getDepthForLayer(1f - (float)i / (float)layers);
				Vector2 vector = scene.DrawPos(pos, depthForLayer, camPos, rCam.hDisplace);
				sLeaser.sprites[i].x = vector.x;
				sLeaser.sprites[i].y = vector.y;
				sLeaser.sprites[i].color = new Color(auScene.AtmosphereColorAtDepth(depthForLayer), 1f - (float)i / (float)layers, 0f);
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

	public List<RoofTopView.DustWave> dustWaves;

	public InnerOuterRimView(Room room, RoomSettings.RoomEffect effect)
		: base(room)
	{
		this.effect = effect;
		sceneOrigo = RoomToWorldPos(room.abstractRoom.size.ToVector2() * 10f);
		Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
		Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, atmosphereColor);
		Shader.SetGlobalVector(RainWorld.ShadPropSceneOrigoPosition, sceneOrigo);
		Shader.SetGlobalVector(RainWorld.ShadPropMultiplyColor, Color.white);
		Random.State state = Random.state;
		Random.InitState(0);
		AddElement(new Simple2DBackgroundIllustration(this, "otr_sky", new Vector2(683f, 384f)));
		AddElement(new Building(this, "otr_bigtube", new Vector2(-30f, 0f), 1f, 20f, 0f, 4.2f, 64));
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
