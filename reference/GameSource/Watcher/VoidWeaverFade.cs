using RWCustom;
using UnityEngine;

namespace Watcher;

public class VoidWeaverFade : CosmeticSprite
{
	public float lastFade;

	public float fade;

	public bool despawned;

	public Vector2? setPos;

	public float speed = 0.01f;

	public bool giveAchievement;

	public bool RampingDown => despawned;

	public VoidWeaverFade(Vector2 pos)
	{
		base.pos = pos;
		lastPos = pos;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastFade = fade;
		if (setPos.HasValue)
		{
			pos = setPos.Value;
		}
		if (!despawned)
		{
			fade = Mathf.MoveTowards(fade, 1f, speed);
			if (fade == 1f)
			{
				Despawn();
			}
		}
		else
		{
			fade = Custom.LerpAndTick(fade, 0f, speed, speed);
			if (fade == 0f)
			{
				Done();
			}
		}
	}

	public void Despawn()
	{
		despawned = true;
	}

	public void Done()
	{
		if (!base.slatedForDeletetion && giveAchievement)
		{
			Custom.rainWorld.processManager.CueAchievement(RainWorld.AchievementID.VoidWeaver, 5f);
			Custom.LogWarning("give achievement weaver");
		}
		Destroy();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["FlareBombBothSides"]
		};
		sLeaser.sprites[1] = new FSprite("pixel")
		{
			anchorX = 0f,
			anchorY = 0f,
			shader = rCam.game.rainWorld.Shaders["RippleBasicBothSides"]
		};
		AddToContainer(sLeaser, rCam, null);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[1]);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float num = Mathf.Lerp(lastFade, fade, timeStacker);
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		sLeaser.sprites[0].SetPosition(vector - camPos);
		sLeaser.sprites[0].scale = (Mathf.Pow(num, 2f) * 1500f + 200f) / 8f;
		sLeaser.sprites[0].color = RainWorld.SaturatedGold;
		sLeaser.sprites[0].alpha = Mathf.Pow(num, 3f) * 0.5f;
		sLeaser.sprites[0].isVisible = !despawned;
		sLeaser.sprites[1].SetPosition(-1f, -1f);
		sLeaser.sprites[1].color = Color.white;
		sLeaser.sprites[1].scaleX = rCam.sSize.x + 2f;
		sLeaser.sprites[1].scaleY = rCam.sSize.y + 2f;
		sLeaser.sprites[1].alpha = (despawned ? num : Mathf.Pow(Mathf.SmoothStep(0f, 1f, num), 2f));
	}
}
