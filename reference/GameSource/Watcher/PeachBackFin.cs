using System;
using LizardCosmetics;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class PeachBackFin : Template
{
	public int bumps;

	public float spineLength;

	public float sizeSkewExponent;

	public float sizeRangeMin;

	public float sizeRangeMax;

	public int graphic;

	public float scaleX;

	public bool colored;

	public PeachBackFin(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.BehindHead;
		spineLength = 0.08f;
		sizeRangeMin = Mathf.Lerp(0.27f, 0.35f, Mathf.Pow(UnityEngine.Random.value, 2f));
		sizeRangeMax = Mathf.Lerp(sizeRangeMin, 0.7f, UnityEngine.Random.value);
		sizeSkewExponent = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
		graphic = ((UnityEngine.Random.value < 0.5f) ? 4 : 5);
		bumps = ((UnityEngine.Random.value < 0.5f) ? 3 : 4);
		scaleX = Mathf.Lerp(1.5f, 2.5f, UnityEngine.Random.value);
		colored = true;
		numberOfSprites = (colored ? (bumps * 2) : bumps);
	}

	public override void Update()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int num = startSprite + bumps - 1; num >= startSprite; num--)
		{
			sLeaser.sprites[num] = new FSprite("LizardScaleA" + graphic);
			sLeaser.sprites[num].anchorY = 0.15f;
			if (colored)
			{
				sLeaser.sprites[num + bumps] = new FSprite("LizardScaleB" + graphic);
				sLeaser.sprites[num + bumps].anchorY = 0.15f;
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int num = startSprite + bumps - 1; num >= startSprite; num--)
		{
			float num2 = Mathf.InverseLerp(startSprite, startSprite + bumps - 1, num);
			LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(0.077f + spineLength * num2, timeStacker);
			sLeaser.sprites[num].x = lizardSpineData.outerPos.x - camPos.x;
			sLeaser.sprites[num].y = lizardSpineData.outerPos.y - camPos.y;
			sLeaser.sprites[num].rotation = Custom.VecToDeg(Vector2.Lerp(lizardSpineData.perp * lizardSpineData.depthRotation, lizardSpineData.dir, num2));
			float num3 = Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Sin(Mathf.Pow(num2, sizeSkewExponent) * (float)Math.PI));
			sLeaser.sprites[num].scaleX = Mathf.Sign(lGraphics.depthRotation) * scaleX * num3;
			sLeaser.sprites[num].scaleY = num3 * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(lGraphics.depthRotation)));
			if (colored)
			{
				sLeaser.sprites[num + bumps].x = lizardSpineData.outerPos.x - camPos.x;
				sLeaser.sprites[num + bumps].y = lizardSpineData.outerPos.y - camPos.y;
				sLeaser.sprites[num + bumps].rotation = Custom.VecToDeg(Vector2.Lerp(lizardSpineData.perp * lizardSpineData.depthRotation, lizardSpineData.dir, num2));
				sLeaser.sprites[num + bumps].scaleX = Mathf.Sign(lGraphics.depthRotation) * scaleX * num3;
				sLeaser.sprites[num + bumps].scaleY = num3 * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(lGraphics.depthRotation)));
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = startSprite; i < startSprite + bumps; i++)
		{
			sLeaser.sprites[i].color = palette.blackColor;
			if (colored)
			{
				sLeaser.sprites[i + bumps].color = lGraphics.effectColor;
			}
		}
	}
}
