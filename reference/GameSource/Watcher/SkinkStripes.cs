using LizardCosmetics;
using UnityEngine;

namespace Watcher;

public class SkinkStripes : Template
{
	public int segs;

	public Color color;

	public SkinkStripes(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.BodySurface;
		segs = 4 + lGraphics.tail.Length / 2 * 2 + 2;
		numberOfSprites = 2;
	}

	public Color SetColorAlpha(Color c, float a)
	{
		return new Color(c.r, c.g, c.b, a);
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
		sLeaser.sprites[startSprite] = TriangleMesh.MakeLongMesh(segs / 2, pointyTip: false, lGraphics.iVars.tailColor > 0f);
		sLeaser.sprites[startSprite + 1] = TriangleMesh.MakeLongMesh(segs / 2, pointyTip: false, customColor: true);
		sLeaser.sprites[startSprite + 1].shader = rCam.room.game.rainWorld.Shaders["SkinkStripes"];
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < segs; i++)
		{
			float s = Mathf.InverseLerp(0f, segs, i);
			LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(s, timeStacker);
			Vector2 pos = lizardSpineData.pos;
			Vector2 perp = lizardSpineData.perp;
			float rad = lizardSpineData.rad;
			(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 2, pos - perp * rad - camPos);
			(sLeaser.sprites[startSprite] as TriangleMesh).MoveVertice(i * 2 + 1, pos + perp * rad - camPos);
			(sLeaser.sprites[startSprite + 1] as TriangleMesh).MoveVertice(i * 2, pos - perp * rad - camPos);
			(sLeaser.sprites[startSprite + 1] as TriangleMesh).MoveVertice(i * 2 + 1, pos + perp * rad - camPos);
			Color color = SetColorAlpha(this.color, 0.5f + lizardSpineData.depthRotation * 0.5f);
			(sLeaser.sprites[startSprite + 1] as TriangleMesh).verticeColors[i * 2] = color;
			(sLeaser.sprites[startSprite + 1] as TriangleMesh).verticeColors[i * 2 + 1] = color;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (lGraphics.iVars.tailColor > 0f)
		{
			for (int i = 0; i < (sLeaser.sprites[startSprite] as TriangleMesh).verticeColors.Length; i++)
			{
				float f = (float)(i / 2) * 2f / (float)((sLeaser.sprites[startSprite] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[startSprite] as TriangleMesh).verticeColors[i] = lGraphics.BodyColor(f);
			}
		}
		else
		{
			sLeaser.sprites[startSprite].color = palette.blackColor;
		}
		color = Color.Lerp(lGraphics.effectColor, palette.blackColor, 0.5f);
	}
}
