using UnityEngine;

namespace Watcher;

public class RippleFullScreen : CosmeticSprite
{
	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.maskSources = new MaskSource[1];
		sLeaser.maskSources[0] = MaskMaker.MakeSource("GameplayRippleGrab", "Basic", isSecondLayer: false, null, rCam.pos, null, Vector3.one * 3000f);
		sLeaser.maskSources[0].SetProperty(0, 1f);
		sLeaser.maskSources[0].SetProperty(1, 1f);
		sLeaser.maskSources[0].SetProperty(2, 1f);
		sLeaser.maskSources[0].SetProperty(3, 1f);
	}

	public override void Destroy()
	{
		base.Destroy();
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.maskSources[0].setGameObjectPos = camPos;
		sLeaser.maskSources[0].DrawUpdate(timeStacker, rCam, camPos);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
