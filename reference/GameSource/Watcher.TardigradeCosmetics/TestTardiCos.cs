using RWCustom;
using UnityEngine;

namespace Watcher.TardigradeCosmetics;

public class TestTardiCos : Template
{
	public TestTardiCos(TardigradeGraphics tGraphics, int startSprite)
		: base(tGraphics, startSprite)
	{
		numberOfSprites = 1;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[startSprite] = new FSprite("Circle20");
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, TardigradeGraphics.Spine spine)
	{
		TardigradeGraphics.SpineData spineData = spine.GetSpineData(0.2f);
		Vector2 vector = spineData.pos - Custom.PerpendicularVector(spineData.dir) * (20f * bGraphics.bunny.flip);
		sLeaser.sprites[startSprite].x = vector.x - camPos.x;
		sLeaser.sprites[startSprite].y = vector.y - camPos.y;
	}
}
