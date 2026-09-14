using System;
using UnityEngine;

namespace LizardCosmetics;

[Obsolete("LizardCosmetics.MudOverlay is replaced with MudOverlay. Use it by adding the IMuddableGraphics interface.")]
public class MudOverlay : Template
{
	public int firstLimbSprite;

	public int firstCircleSprite;

	public int CircleCount => lGraphics.SpriteBodyCirclesEnd - lGraphics.SpriteBodyCirclesStart;

	public MudOverlay(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.InFront;
		numberOfSprites = 4;
		firstLimbSprite = startSprite + numberOfSprites;
		numberOfSprites += lGraphics.limbs.Length;
		firstCircleSprite = startSprite + numberOfSprites;
		numberOfSprites += lGraphics.SpriteBodyCirclesEnd - lGraphics.SpriteBodyCirclesStart;
	}

	public override void Update()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		FSprite[] sprites = sLeaser.sprites;
		sprites[startSprite] = MudUtils.MakeMudSprite(rCam, sprites[lGraphics.SpriteHeadStart]);
		sprites[startSprite + 1] = MudUtils.MakeMudSprite(rCam, sprites[lGraphics.SpriteHeadStart + 3]);
		sprites[startSprite + 2] = MudUtils.MakeMudSprite(rCam, sprites[lGraphics.SpriteTail]);
		sprites[startSprite + 3] = MudUtils.MakeMudSprite(rCam, sprites[lGraphics.SpriteBodyMesh]);
		for (int i = 0; i < lGraphics.limbs.Length; i++)
		{
			sprites[startSprite + 4 + i] = MudUtils.MakeMudSprite(rCam, sprites[lGraphics.SpriteLimbsStart + i]);
		}
		for (int j = 0; j < CircleCount; j++)
		{
			sprites[firstCircleSprite + j] = MudUtils.MakeMudSprite(rCam, sLeaser.sprites[lGraphics.SpriteBodyCirclesStart + j]);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		FSprite[] sprites = sLeaser.sprites;
		Lizard lizard = lGraphics.lizard;
		MudUtils.UpdateAndLayerMudSprite(lizard, sprites[startSprite], sprites[lGraphics.SpriteHeadStart]);
		MudUtils.UpdateAndLayerMudSprite(lizard, sprites[startSprite + 1], sprites[lGraphics.SpriteHeadStart + 3]);
		MudUtils.UpdateAndLayerMudSprite(lizard, sprites[startSprite + 2], sprites[lGraphics.SpriteTail]);
		MudUtils.UpdateAndLayerMudSprite(lizard, sprites[startSprite + 3], sprites[lGraphics.SpriteBodyMesh]);
		for (int i = 0; i < lGraphics.limbs.Length; i++)
		{
			MudUtils.UpdateAndLayerMudSprite(lizard, sprites[firstLimbSprite + i], sprites[lGraphics.SpriteLimbsStart + i]);
		}
		for (int j = 0; j < CircleCount; j++)
		{
			MudUtils.UpdateAndLayerMudSprite(lizard, sLeaser.sprites[firstCircleSprite + j], sLeaser.sprites[lGraphics.SpriteBodyCirclesStart + j]);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		float rad = lGraphics.lizard.bodyChunks[lGraphics.lizard.bodyChunks.Length - 1].rad;
		MudUtils.SetUpTail(sLeaser.sprites[startSprite + 2], lGraphics.tailLength, rad * 2f);
		MudUtils.SetUpTube(sLeaser.sprites[startSprite + 3], lGraphics.bodyLength, rad * 2f);
	}
}
