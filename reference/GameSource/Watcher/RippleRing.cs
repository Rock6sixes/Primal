using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleRing : CosmeticSprite
{
	public Counter life;

	public MaskSource ring;

	public float maxScale = 200f;

	public float speed;

	public float strength;

	public float lastStrength;

	public float scale;

	public float lastScale;

	public float intensity;

	public float lastIntensity;

	public bool reversed;

	public Vector3 getScale => Vector3.one * (scale += speed);

	public RippleRing(Vector2 pos, int life, float intensity, float speed)
	{
		base.pos = pos;
		this.speed = speed;
		reversed = speed < 0f;
		this.life = new Counter(life, 0, reversed);
		strength = (lastStrength = this.life.normalized);
		scale = (lastScale = (reversed ? ((0f - speed) * (float)life) : 0f));
		this.intensity = (lastIntensity = intensity);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		life.Tick();
		UpdateRing();
		if (life.isFinished)
		{
			Destroy();
		}
	}

	public void UpdateRing()
	{
		lastIntensity = intensity;
		lastStrength = strength;
		lastScale = scale;
		strength = life.normalized;
		if (ring != null)
		{
			ring.SetPos(pos);
			ring.SetScale(getScale);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.maskSources = new MaskSource[1];
		if (ring == null)
		{
			ring = MaskMaker.MakeSource("GameplayRippleGrab", "RippleRingMask", isSecondLayer: true, null, pos, null, Vector3.zero);
		}
		sLeaser.maskSources[0] = ring;
		UpdateRing();
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		ring.SetProperty(0, Mathf.Lerp(lastStrength * lastIntensity, strength * intensity, timeStacker));
		ring.SetProperty(2, Mathf.Lerp(lastScale, scale, timeStacker) / maxScale);
		ring.DrawUpdate(timeStacker, rCam, camPos);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
