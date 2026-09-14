using RWCustom;
using SplashWater;
using UnityEngine;

namespace Watcher;

public class BrineDissolve : UpdatableAndDeletable
{
	public Vector2 splashPos;

	public float splashRad;

	public float progress;

	public float finalRad;

	public int life;

	public int startLife;

	public Vector2 SplashDirection => new Vector2((Random.value * 2f - 1f) * 0.25f, 1f).normalized * (1f + finalRad * (1f - Mathf.Abs(progress - 0.5f) * 2f));

	public BrineDissolve(Vector2 pos, float rad, int life)
	{
		splashPos = pos;
		splashRad = rad;
		life = Mathf.Min(life, 120);
		this.life = life;
		startLife = life;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.BeingViewed)
		{
			if (life <= 0)
			{
				Destroy();
				return;
			}
			Splash splash = new Splash();
			progress = 1f - (float)life / (float)startLife;
			finalRad = splashRad * 2f * progress;
			room.AddObject(splash);
			Vector2 pos = splashPos + Vector2.right * ((Random.value * 2f - 1f) * finalRad * 4f * progress);
			Vector2 pos2 = new Vector2(pos.x, room.FloatWaterLevel(pos));
			Vector2 splashDirection = SplashDirection;
			room.AddObject(new WaterDrip(pos2, splashDirection * 1.3f + Custom.RNV() * 1f, waterColor: true));
			splash.Reset(pos2, splashDirection, 20f, splashRad + finalRad);
			life--;
		}
	}
}
