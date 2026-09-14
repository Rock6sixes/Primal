using LizardCosmetics;
using UnityEngine;

namespace Watcher;

public class PeachHeadStripes : Template
{
	public Color redderTint;

	public Color blackColor;

	public float alpha;

	public PeachHeadStripes(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.InFront;
		alpha = Random.value;
		redderTint = new Color(lGraphics.effectColor.r, lGraphics.effectColor.g, lGraphics.effectColor.b);
		redderTint.g *= 0.5f;
		redderTint.b *= 0.5f;
		redderTint.r = Mathf.Lerp(redderTint.r, 1f, 0.75f);
		numberOfSprites = 1;
	}

	public override void Update()
	{
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
		float num = Mathf.Pow(Random.value, 1f - 0.5f * lGraphics.lizard.AI.yellowAI.commFlicker) * lGraphics.lizard.AI.yellowAI.commFlicker;
		if (!lGraphics.lizard.Consious)
		{
			num = 0f;
		}
		float f = Mathf.Lerp(lGraphics.lastHeadDepthRotation, lGraphics.headDepthRotation, timeStacker);
		int num2 = 3 - (int)(Mathf.Abs(f) * 3.9f);
		sLeaser.sprites[startSprite].element = Futile.atlasManager.GetElementWithName("PeachLizardStripes" + num2);
		int num3 = lGraphics.SpriteHeadEnd - 2;
		sLeaser.sprites[startSprite].x = sLeaser.sprites[num3].x;
		sLeaser.sprites[startSprite].y = sLeaser.sprites[num3].y;
		sLeaser.sprites[startSprite].rotation = sLeaser.sprites[num3].rotation;
		sLeaser.sprites[startSprite].anchorY = sLeaser.sprites[num3].anchorY;
		sLeaser.sprites[startSprite].scaleX = sLeaser.sprites[num3].scaleX;
		sLeaser.sprites[startSprite].scaleY = sLeaser.sprites[num3].scaleY;
		Color color = EffectColor(num);
		if (num > 0.4f && rCam.room.PointSubmerged(lGraphics.head.pos + new Vector2(0f, 25f)))
		{
			color = new Color(0f, 0.003921569f, 0f);
		}
		sLeaser.sprites[startSprite].color = color;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
	}

	public Color EffectColor(float flicker)
	{
		return Color.Lerp(new Color(redderTint.r, redderTint.g, redderTint.b, alpha), new Color(1f, 1f, 1f, alpha), flicker);
	}
}
