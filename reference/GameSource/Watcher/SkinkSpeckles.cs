using System;
using LizardCosmetics;
using UnityEngine;

namespace Watcher;

public class SkinkSpeckles : Template
{
	public int spots;

	public Vector3[] spotInfo;

	public SkinkSpeckles(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.BodySurface;
		spots = UnityEngine.Random.Range(UnityEngine.Random.Range(0, 20), 50);
		spotInfo = new Vector3[spots];
		for (int i = 0; i < spots; i++)
		{
			spotInfo[i] = new Vector3(Mathf.Sin(UnityEngine.Random.value * (float)Math.PI * 0.5f), UnityEngine.Random.value * 2f - 1f, UnityEngine.Random.value * 0.6f + 0.4f);
		}
		numberOfSprites = spots;
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < spots; i++)
		{
			sLeaser.sprites[startSprite + i] = new FSprite("Circle20");
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < spots; i++)
		{
			LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(spotInfo[i].x, timeStacker);
			float num = lizardSpineData.depthRotation + spotInfo[i].y;
			if (num < -1f)
			{
				num += 2f;
			}
			else if (num > 1f)
			{
				num -= 2f;
			}
			Vector2 vector = lizardSpineData.pos + lizardSpineData.perp * (num * lizardSpineData.rad * 0.9f);
			sLeaser.sprites[startSprite + i].x = vector.x - camPos.x;
			sLeaser.sprites[startSprite + i].y = vector.y - camPos.y;
			sLeaser.sprites[startSprite + i].scale = spotInfo[i].z * (1f - Mathf.Abs(num) * 0.3f) * lizardSpineData.rad * 0.03f;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < spots; i++)
		{
			sLeaser.sprites[startSprite + i].color = lGraphics.BodyColor(spotInfo[i].x);
		}
	}
}
