using System;
using LizardCosmetics;
using UnityEngine;

namespace Watcher;

public class BasiliskSlowField : Template
{
	public float intensity;

	public float lastIntensity;

	public Vector2 smoothedVel;

	public float sinCounter;

	public Lizard lizard => lGraphics.lizard;

	public BasiliskSlowField(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.HUD;
		numberOfSprites = 1;
		intensity = 0f;
		lastIntensity = 0f;
	}

	public override void Update()
	{
		smoothedVel = Vector2.Lerp(smoothedVel, lizard.mainBodyChunk.vel, 0.2f);
		lastIntensity = intensity;
		sinCounter += 1f / Mathf.Lerp(50f, 10f, Mathf.InverseLerp(3f, 20f, smoothedVel.magnitude));
		if (sinCounter > 1f)
		{
			sinCounter -= 1f;
		}
		float num = 0.1f + 0.9f * Mathf.Pow(Mathf.InverseLerp(0.1f, 20f, smoothedVel.magnitude), 2f);
		intensity = (1f - num) * Mathf.InverseLerp(0.1f, 20f, smoothedVel.magnitude);
		intensity += num * (0.5f + 0.5f * Mathf.Sin(sinCounter * (float)Math.PI * 2f));
		intensity = Mathf.Pow(intensity, Mathf.InverseLerp(18f, 3f, smoothedVel.magnitude));
		intensity *= lizard.BasiliskMushroomField * 10f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[startSprite] = new FSprite("Futile_White");
		sLeaser.sprites[startSprite].shader = rCam.room.game.rainWorld.Shaders["Adrenaline"];
		sLeaser.sprites[startSprite].color = new Color(0f, 0.5f, 1f);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lizard.firstChunk.lastPos, lizard.firstChunk.pos, timeStacker);
		sLeaser.sprites[startSprite].x = vector.x - camPos.x;
		sLeaser.sprites[startSprite].y = vector.y - camPos.y;
		Vector2 normalized = smoothedVel.normalized;
		sLeaser.sprites[startSprite].color = new Color(Mathf.InverseLerp(-1f, 1f, normalized.x), Mathf.InverseLerp(-1f, 1f, 0f - normalized.y), Mathf.InverseLerp(0.2f, 6f, smoothedVel.magnitude));
		sLeaser.sprites[startSprite].alpha = Mathf.Lerp(lastIntensity, intensity, timeStacker);
		sLeaser.sprites[startSprite].scale = Mathf.Lerp(110f, 80f, Mathf.Lerp(lastIntensity, intensity, timeStacker)) / 4f;
		sLeaser.sprites[startSprite].isVisible = intensity > 0f;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}
}
