using RWCustom;
using UnityEngine;

namespace Watcher;

public class RipplesSpawnAbsorption : CosmeticSprite
{
	public float targetFade;

	public float targetBigArea;

	public float targetFadeSpeed;

	public float targetBigAreaSpeed;

	public float fade;

	public float bigArea;

	public float size = 20f;

	public RipplesSpawnAbsorption()
	{
		fade = 0f;
		bigArea = 0f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		fade = Mathf.Lerp(fade, targetFade, targetFadeSpeed);
		bigArea = Mathf.Lerp(bigArea, targetBigArea, targetBigAreaSpeed);
	}

	public void Start()
	{
		targetBigAreaSpeed = 0.05f;
		targetFadeSpeed = 0.05f;
		targetBigArea = 1f;
		targetFade = 2f;
	}

	public void SlowStart()
	{
		targetBigAreaSpeed = 0.03f;
		targetFadeSpeed = 0.01f;
		targetBigArea = 0.8f;
		targetFade = 0.95f;
	}

	public void End()
	{
		targetBigArea = 0f;
		targetFade = 0f;
		targetBigAreaSpeed = 0.008f;
		targetFadeSpeed = 0.002f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["RippleSpawnAbsorption"];
		sLeaser.sprites[0].scale = size;
		rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[0]);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 position = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
		sLeaser.sprites[0].SetPosition(position);
		sLeaser.sprites[0].scale = size;
		sLeaser.sprites[0].color = new Color(bigArea, 0f, 0f, Mathf.Min(1f, fade));
	}
}
