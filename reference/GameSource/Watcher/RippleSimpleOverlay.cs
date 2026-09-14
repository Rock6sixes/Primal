using RWCustom;

namespace Watcher;

public class RippleSimpleOverlay : CosmeticSprite
{
	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["RippleSimpleOverlay"];
		sLeaser.sprites[0].scaleX = Custom.rainWorld.options.ScreenSize.x / 16f + 0.5f;
		sLeaser.sprites[0].scaleY = Custom.rainWorld.options.ScreenSize.y / 16f + 0.5f;
		sLeaser.sprites[0].anchorX = 0f;
		sLeaser.sprites[0].anchorY = 0f;
		rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[0]);
	}
}
