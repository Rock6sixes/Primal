using LizardCosmetics;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BlizzardSteam : Template
{
	public float[] heatersHeat;

	public Lizard lizard => lGraphics.lizard;

	public BlizzardSteam(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.HUD;
		numberOfSprites = 1;
		heatersHeat = new float[3];
	}

	public override void Update()
	{
		heatersHeat[1] = heatersHeat[0];
		heatersHeat[2] = Mathf.Min(heatersHeat[2] + 0.0016666667f, 1f);
		heatersHeat[0] = Mathf.Lerp(heatersHeat[0], heatersHeat[2], 0.7f);
		if (Mathf.Pow(Random.value, 2f) < 0.3f && Mathf.Pow(Random.value, 2f) < heatersHeat[2] * 2f)
		{
			heatersHeat[0] = Mathf.Min(heatersHeat[0], (1f - Random.value * 0.3f) * heatersHeat[2]);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[startSprite] = new FSprite("Futile_White");
		sLeaser.sprites[startSprite].shader = rCam.room.game.rainWorld.Shaders["HeatDistortion"];
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lizard.firstChunk.lastPos, lizard.firstChunk.pos, timeStacker);
		float num = Mathf.InverseLerp(0.15f, 0.8f, heatersHeat[2]);
		sLeaser.sprites[startSprite].x = vector.x - camPos.x;
		sLeaser.sprites[startSprite].y = vector.y + 40f * num - camPos.y;
		sLeaser.sprites[startSprite].scaleX = Mathf.Lerp(10f, 15f, num);
		sLeaser.sprites[startSprite].scaleY = Mathf.Lerp(15f, 30f, num);
		sLeaser.sprites[startSprite].alpha = Custom.SCurve(heatersHeat[2], 1.5f);
		sLeaser.sprites[startSprite].isVisible = !lizard.dead;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}
}
