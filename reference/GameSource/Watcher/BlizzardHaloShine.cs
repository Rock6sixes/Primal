using LizardCosmetics;
using UnityEngine;

namespace Watcher;

public class BlizzardHaloShine : Template
{
	public float heat;

	public int heatWiwble;

	public BlizzardHaloShine(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.InFront;
		numberOfSprites = 1;
	}

	public override void Update()
	{
		heatWiwble++;
		if (lGraphics.lizard.blizzardModule.HasBeam)
		{
			heat += 0.02f;
			return;
		}
		heat *= 0.97f + Mathf.Clamp01(heat) * 0.03f;
		heat = Mathf.Max(heat - 0.0019f, 0f);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[startSprite] = new FSprite("pixel");
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float f = Mathf.Lerp(lGraphics.lastHeadDepthRotation, lGraphics.headDepthRotation, timeStacker);
		int num = 3 - (int)(Mathf.Abs(f) * 3.9f);
		sLeaser.sprites[startSprite].color = Color.Lerp(Color.white, Color.Lerp(new Color(0.9f, 0.05f, 0f), Color.Lerp(new Color(1f, 0.4f, 0f), Color.white, Mathf.Abs(Mathf.Sin((float)heatWiwble * 0.07f)) * 0.3f), heat - 1f), heat);
		sLeaser.sprites[startSprite].element = Futile.atlasManager.GetElementWithName("BlizardHaloShine" + num);
		int num2 = lGraphics.SpriteHeadEnd - 2;
		sLeaser.sprites[startSprite].x = sLeaser.sprites[num2].x;
		sLeaser.sprites[startSprite].y = sLeaser.sprites[num2].y;
		sLeaser.sprites[startSprite].rotation = sLeaser.sprites[num2].rotation;
		sLeaser.sprites[startSprite].anchorY = sLeaser.sprites[num2].anchorY;
		sLeaser.sprites[startSprite].scaleX = sLeaser.sprites[num2].scaleX;
		sLeaser.sprites[startSprite].scaleY = sLeaser.sprites[num2].scaleY;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[startSprite].color = Color.red;
	}
}
