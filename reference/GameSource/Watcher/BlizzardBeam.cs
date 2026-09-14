using LizardCosmetics;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BlizzardBeam : Template
{
	public Vector2 hitPos;

	public Vector2 lastHitPos;

	public float size;

	public float lastSize;

	public int time;

	public Lizard lizard => lGraphics.lizard;

	public BlizzardBeam(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.Behind;
		numberOfSprites = 3;
	}

	public override void Update()
	{
		lastHitPos = hitPos;
		lastSize = size;
		time++;
		size = Mathf.Abs(Mathf.Sin((float)time * 0.44f));
		hitPos = lGraphics.lizard.blizzardModule.RayTraceBeamHitPos();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[startSprite] = new FSprite("pixel");
		sLeaser.sprites[startSprite].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
		sLeaser.sprites[startSprite].scaleX = 44f;
		sLeaser.sprites[startSprite].anchorY = 0f;
		sLeaser.sprites[startSprite].alpha = 0.5f;
		sLeaser.sprites[startSprite + 1] = new FSprite("pixel");
		sLeaser.sprites[startSprite + 1].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
		sLeaser.sprites[startSprite + 1].scaleX = 18f;
		sLeaser.sprites[startSprite + 1].anchorY = 0f;
		sLeaser.sprites[startSprite + 1].alpha = 0.8f;
		sLeaser.sprites[startSprite + 2] = new FSprite("pixel");
		sLeaser.sprites[startSprite + 2].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
		sLeaser.sprites[startSprite + 2].scaleX = 5f;
		sLeaser.sprites[startSprite + 2].anchorY = 0f;
		sLeaser.sprites[startSprite + 2].alpha = 1f;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lizard.firstChunk.lastPos, lizard.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(lastHitPos, hitPos, timeStacker);
		Vector2 v = Custom.DirVec(vector, vector2);
		float num = Mathf.Lerp(lastSize, size, timeStacker);
		for (int i = 0; i < numberOfSprites; i++)
		{
			sLeaser.sprites[startSprite + i].x = vector.x - camPos.x;
			sLeaser.sprites[startSprite + i].y = vector.y - camPos.y;
			sLeaser.sprites[startSprite + i].rotation = Custom.VecToDeg(v);
			sLeaser.sprites[startSprite + i].scaleY = Vector2.Distance(vector, vector2);
			sLeaser.sprites[startSprite + i].isVisible = lGraphics.lizard.blizzardModule.HasBeam;
		}
		sLeaser.sprites[startSprite].scaleX = 40f + 20f * num;
		sLeaser.sprites[startSprite + 1].scaleX = 25f - 10f * num;
		sLeaser.sprites[startSprite + 2].scaleX = 5f + 5f * num;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = new Color(0.9f, 0.85f, 1f);
		sLeaser.sprites[startSprite + 2].color = color;
		sLeaser.sprites[startSprite + 1].color = Color.Lerp(color, palette.fogColor, 0.2f);
		sLeaser.sprites[startSprite].color = Color.Lerp(color, palette.fogColor, 0.5f);
	}
}
