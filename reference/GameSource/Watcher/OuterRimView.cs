using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class OuterRimView : BackgroundScene
{
	public class DustGradient : BackgroundSceneElement
	{
		public DustGradient(OuterRimView scene, Vector2 pos)
			: base(scene, pos, 0f)
		{
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("au_dustgradient");
			sLeaser.sprites[0].x = pos.x;
			sLeaser.sprites[0].y = pos.y;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["OuterRimDustGradient"];
			sLeaser.sprites[0].width = rCam.game.rainWorld.screenSize.x;
			sLeaser.sprites[0].height = rCam.game.rainWorld.screenSize.y;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			sLeaser.sprites[0].color = palette.skyColor;
			base.ApplyPalette(sLeaser, rCam, palette);
		}
	}

	public class Fog : FullScreenSingleColor
	{
		public OuterRimView orScene => scene as OuterRimView;

		public Fog(OuterRimView scene)
			: base(scene, default(Color), 1f, singlePixelTexture: true, float.MaxValue)
		{
			depth = 0f;
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			alpha = 1f - orScene.effect.amount;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			color = palette.skyColor;
			base.ApplyPalette(sLeaser, rCam, palette);
		}
	}

	public class PlaceholderElement : BackgroundSceneElement
	{
		public float scaleX;

		public float scaleY;

		public float anchorX;

		public float anchorY;

		public float rotation;

		public Color color;

		public bool applyPerspectiveScale;

		public OuterRimView orScene => scene as OuterRimView;

		public PlaceholderElement(OuterRimView scene, float x, float y, float depth, float scaleX, float scaleY, float anchorX, float anchorY, float rotation, Color color, bool applyPerspectiveScale)
			: base(scene, new Vector2(x * sceneScale, y * sceneScale), depth)
		{
			this.scaleX = scaleX * sceneScale;
			this.scaleY = scaleY * sceneScale;
			this.anchorX = anchorX;
			this.anchorY = anchorY;
			this.rotation = rotation;
			this.color = color;
			this.applyPerspectiveScale = applyPerspectiveScale;
		}

		public PlaceholderElement(OuterRimView scene, float x, float y, float depth, float scaleX, float scaleY, float anchorY, float rotation, Color color, bool applyPerspectiveScale)
			: this(scene, x, y, depth, scaleX, scaleY, 0.5f, anchorY, rotation, color, applyPerspectiveScale)
		{
		}

		public PlaceholderElement(OuterRimView scene, float x, float y, float depth, float scaleX, float scaleY, float rotation, Color color, bool applyPerspectiveScale)
			: this(scene, x, y, depth, scaleX, scaleY, 0.5f, 0.5f, rotation, color, applyPerspectiveScale)
		{
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("pixel");
			float num = (applyPerspectiveScale ? orScene.DrawScale(this) : 1f);
			sLeaser.sprites[0].scaleX = scaleX * num;
			sLeaser.sprites[0].scaleY = scaleY * num;
			sLeaser.sprites[0].anchorX = anchorX;
			sLeaser.sprites[0].anchorY = anchorY;
			sLeaser.sprites[0].rotation = rotation;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["OuterRimBackgroundBuilding"];
			sLeaser.sprites[0].color = new Color(color.r, color.g, color.b, Mathf.Min(depth / fogDepth, 1f));
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = orScene.DrawPos(this, new Vector2(camPos.x, camPos.y), rCam);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class Building : BackgroundSceneElement
	{
		public float scale;

		public float anchorX;

		public float anchorY;

		public float rotation;

		public bool applyPerspectiveScale;

		public string assetName;

		public OuterRimView orScene => scene as OuterRimView;

		public Building(OuterRimView scene, string assetName, float x, float y, float depth, float scale, float anchorX, float anchorY, float rotation, bool applyPerspectiveScale)
			: base(scene, new Vector2(x * sceneScale, y * sceneScale), depth)
		{
			this.scale = scale * sceneScale;
			this.anchorX = anchorX;
			this.anchorY = anchorY;
			this.rotation = rotation;
			this.applyPerspectiveScale = applyPerspectiveScale;
			this.assetName = assetName;
			scene.LoadGraphic(assetName, crispPixels: true, clampWrapMode: false);
		}

		public Building(OuterRimView scene, string assetName, float x, float y, float depth, float scale, float anchorY, float rotation, bool applyPerspectiveScale)
			: this(scene, assetName, x, y, depth, scale, 0.5f, anchorY, rotation, applyPerspectiveScale)
		{
		}

		public Building(OuterRimView scene, string assetName, float x, float y, float depth, float scale, float rotation, bool applyPerspectiveScale)
			: this(scene, assetName, x, y, depth, scale, 0.5f, 0.5f, rotation, applyPerspectiveScale)
		{
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite(assetName);
			float num = (applyPerspectiveScale ? orScene.DrawScale(this) : 1f);
			sLeaser.sprites[0].scale = scale * num;
			sLeaser.sprites[0].anchorX = anchorX;
			sLeaser.sprites[0].anchorY = anchorY;
			sLeaser.sprites[0].rotation = rotation;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["OuterRimBackgroundBuilding"];
			sLeaser.sprites[0].color = new Color(1f, 1f, 1f, Mathf.Min(depth / fogDepth, 1f));
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = orScene.DrawPos(this, new Vector2(camPos.x, camPos.y), rCam);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class DustDune : BackgroundSceneElement
	{
		public float scaleX;

		public float scaleY;

		public OuterRimView orScene => scene as OuterRimView;

		public DustDune(OuterRimView scene, float x, float y, float depth, float scaleX, float scaleY)
			: base(scene, new Vector2(x * sceneScale, y * sceneScale), depth)
		{
			this.scaleX = scaleX * sceneScale;
			this.scaleY = scaleY * sceneScale;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("otr_dustdunes");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["DustDunes"];
			float num = orScene.DrawScale(this);
			sLeaser.sprites[0].scaleX = scaleX * num / sLeaser.sprites[0].textureRect.width;
			sLeaser.sprites[0].scaleY = scaleY * num / sLeaser.sprites[0].textureRect.height;
			sLeaser.sprites[0].anchorX = 0f;
			sLeaser.sprites[0].anchorY = 0.5f;
			sLeaser.sprites[0].color = new Color(1f, 1f, 1f, Mathf.Min(depth / fogDepth, 1f));
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = orScene.DrawPos(this, new Vector2(camPos.x, camPos.y), rCam);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class FirmamentCloud : BackgroundSceneElement
	{
		public float scaleX;

		public float scaleY;

		public OuterRimView orScene => scene as OuterRimView;

		public FirmamentCloud(OuterRimView scene, float x, float y, float depth, float scaleX, float scaleY)
			: base(scene, new Vector2(x * sceneScale, y * sceneScale), depth)
		{
			this.scaleX = scaleX * sceneScale;
			this.scaleY = scaleY * sceneScale;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("otr_firmamentcloud");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FirmamentCloud"];
			float num = orScene.DrawScale(this);
			sLeaser.sprites[0].scaleX = num * scaleX;
			sLeaser.sprites[0].scaleY = num * scaleY;
			sLeaser.sprites[0].anchorX = 1f;
			sLeaser.sprites[0].anchorY = 0.25f;
			sLeaser.sprites[0].color = new Color(1f, 1f, 1f, Mathf.Min(depth / fogDepth, 1f));
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = orScene.DrawPos(this, new Vector2(camPos.x, camPos.y), rCam);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class FallingStar : BackgroundSceneElement
	{
		public float initialX;

		public float initialY;

		public float prevX;

		public float prevY;

		public float x;

		public float y;

		public float prevRotation;

		public float rotation;

		public float scale;

		public int timeAlive;

		public float randomValue;

		public OuterRimView orScene => scene as OuterRimView;

		public FallingStar(OuterRimView scene, float x, float y, float depth, float scale, float randomValue)
			: base(scene, new Vector2(x * sceneScale, y * sceneScale), depth)
		{
			initialX = x * sceneScale;
			initialY = y * sceneScale;
			reset();
			this.scale = scale * sceneScale;
			this.randomValue = randomValue;
			prevRotation = (rotation = randomValue * 360f);
			this.y = Mathf.Lerp(initialY, 150f, randomValue);
		}

		public void reset()
		{
			x = initialX;
			y = initialY;
			prevX = initialX;
			prevY = initialY;
			timeAlive = 0;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("otr_fallingstar");
			sLeaser.sprites[0].scale = scale;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FallingStar"];
			sLeaser.sprites[0].color = new Color(1f, 1f, 1f, Mathf.Min(depth / fogDepth, 1f));
			AddToContainer(sLeaser, rCam, null);
		}

		public void Update()
		{
			timeAlive++;
			prevX = x;
			prevY = y;
			prevRotation = rotation;
			float num = Mathf.PerlinNoise(x / TurbulenceScale, y / TurbulenceScale + depth);
			num *= Custom.MapRangeClamped(num, -1f, 1f, -0.2f, 1f);
			x += StarSpeed.x * sceneScale * num;
			y += StarSpeed.y * sceneScale;
			rotation += Mathf.Sin((float)timeAlive / 40f * StarSpeed.w) * StarSpeed.z;
			if (y <= 150f)
			{
				reset();
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			pos.x = Mathf.Lerp(prevX, x, timeStacker);
			pos.y = Mathf.Lerp(prevY, y, timeStacker);
			Vector2 vector = orScene.DrawPos(this, new Vector2(camPos.x, camPos.y), rCam);
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].rotation = Mathf.Lerp(prevRotation, rotation, timeStacker);
			sLeaser.sprites[0].color = new Color(1f, randomValue, Mathf.InverseLerp(initialY, 150f, pos.y), Mathf.Min(depth / fogDepth, 1f));
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public RoomSettings.RoomEffect effect;

	public static Color atmosphereColor = new Color(22f / 85f, 0.2509804f, 0.2784314f);

	public static Vector2 ViewOffset = new Vector2(-50000f, 6000f);

	public static Vector2 ConvergenceMult = new Vector2(0.4f, 0.5f);

	public static Vector2 CloudPos = new Vector2(-2000f, 600f);

	public static Vector2 CloudScale = new Vector2(12f, 8f);

	public static Vector2 CloudOffset = new Vector2(20000f, -300f);

	public static float ViewScale = 20f;

	public static float ViewDepthMultiplier = 8f;

	public static int ViewSeed = 1;

	public static float TurbulenceScale = 3000f;

	public static Vector4 StarSpeed = new Vector4(2f, -1f, 1f, 0.1f);

	public static float startDepth = 2f;

	public static float fogDepth = 30f;

	public static float sceneScale = 1f;

	public static float depthScale = 1f;

	public Vector2 perspectiveCenter;

	public List<FallingStar> fallingStars = new List<FallingStar>();

	public OuterRimView(Room room, RoomSettings.RoomEffect effect)
		: base(room)
	{
		Random.State state = Random.state;
		Random.InitState(ViewSeed);
		this.effect = effect;
		Shader.SetGlobalVector(RainWorld.ShadPropAboveCloudsAtmosphereColor, atmosphereColor);
		perspectiveCenter = new Vector2(room.game.rainWorld.screenSize.x * ConvergenceMult.x, room.game.rainWorld.screenSize.y * ConvergenceMult.y);
		sceneOrigo = ViewOffset;
		sceneScale = ViewScale;
		depthScale = ViewDepthMultiplier;
		AddElement(new Simple2DBackgroundIllustration(this, "otr_sky", new Vector2(683f, 384f)));
		LoadGraphic("au_dustgradient", crispPixels: false, clampWrapMode: true);
		AddElement(new DustGradient(this, new Vector2(683f, 384f)));
		AddElement(new Fog(this));
		int num = 32;
		LoadGraphic("otr_dustdunes", crispPixels: false, clampWrapMode: false);
		for (int i = 0; i < num; i++)
		{
			float num2 = Mathf.Pow((float)i / (float)(num - 1), 1.75f);
			float num3 = Mathf.Lerp(startDepth, fogDepth, num2);
			float num4 = Mathf.PerlinNoise(num3 / 4f, 0f) * 2000f * num2;
			AddElement(new DustDune(this, -2000f + num4, 0f, num3, 8000f + num4, 400f));
		}
		for (int j = 0; j < 4; j++)
		{
			starCatcher(Random.Range(-100f, 7000f), 0f, Random.Range(fogDepth * 0.25f, fogDepth));
		}
		LoadGraphic("otr_fallingstar", crispPixels: true, clampWrapMode: true);
		for (int k = 0; k < 64; k++)
		{
			float scale = Random.Range(5f, 12f) / 512f;
			FallingStar fallingStar = new FallingStar(this, Random.Range(-1800f, 1500f), Random.Range(3000f, 4000f), Random.Range(fogDepth * 0.2f, fogDepth), scale, Random.value);
			AddElement(fallingStar);
			fallingStars.Add(fallingStar);
		}
		LoadGraphic("otr_firmamentcloud", crispPixels: false, clampWrapMode: false);
		float num5 = 12f;
		for (int l = 0; (float)l < num5; l++)
		{
			float depth = (float)l / num5 * fogDepth;
			AddElement(new FirmamentCloud(this, CloudPos.x + CloudOffset.x * ((float)l / num5), CloudPos.y + CloudOffset.y * ((float)l / num5), depth, CloudScale.x, CloudScale.y));
		}
		Random.state = state;
	}

	public static float randomGaussian()
	{
		float num;
		float num3;
		do
		{
			num = 2f * Random.value - 1f;
			float num2 = 2f * Random.value - 1f;
			num3 = num * num + num2 * num2;
		}
		while ((double)num3 >= 1.0);
		float num4 = Mathf.Sqrt(-2f * Mathf.Log(num3) / num3);
		return num * num4;
	}

	public void starCatcher(float x, float y, float depth)
	{
		AddElement(new Building(this, "otr_starcatcher" + Random.Range(1, 3), x, y, depth, 5f, 0f, 0f, applyPerspectiveScale: true));
	}

	public Vector2 DrawPos(BackgroundSceneElement element, Vector2 camPos, RoomCamera camera)
	{
		Vector2 vector = RoomToWorldPos(camera.pos);
		float num = DrawScale(element);
		float num2 = element.pos.x - (vector.x + camPos.x + sceneOrigo.x);
		return new Vector2(y: (element.pos.y - (vector.y + camPos.y + sceneOrigo.y)) * num + perspectiveCenter.y, x: num2 * num + perspectiveCenter.x);
	}

	public float DrawScale(BackgroundSceneElement element)
	{
		return 1f / (element.depth * depthScale + 1f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		foreach (FallingStar fallingStar in fallingStars)
		{
			fallingStar.Update();
		}
	}
}
