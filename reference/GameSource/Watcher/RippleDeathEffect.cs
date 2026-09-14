using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleDeathEffect : CosmeticSprite
{
	public float fade;

	public float lastFade;

	public float viableFade;

	public float lastViableFade;

	public float rot;

	public float lastRot;

	public float sin;

	public float fluc;

	public float fluc1;

	public float fluc2;

	public float fluc3;

	public Player player;

	public float TotFade(float timeStacker)
	{
		return Mathf.Lerp(lastFade, fade, timeStacker) * Mathf.Lerp(lastViableFade, viableFade, timeStacker);
	}

	public RippleDeathEffect(Room room, Player player)
	{
		base.room = room;
		this.player = player;
	}

	public static bool CanShowPlayer(Player player)
	{
		if (player.room != null)
		{
			return player.room.ViewedByAnyCamera(player.firstChunk.pos, 100f);
		}
		return true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		lastFade = fade;
		lastViableFade = viableFade;
		lastRot = rot;
		sin += 1f / Mathf.Lerp(480f, 120f, fluc3);
		fluc = Custom.LerpAndTick(fluc, fluc1, 0.02f, 1f / 60f);
		fluc1 = Custom.LerpAndTick(fluc1, fluc2, 0.02f, 1f / 60f);
		fluc2 = Custom.LerpAndTick(fluc2, fluc3, 0.02f, 1f / 60f);
		if (Mathf.Abs(fluc2 - fluc3) < 0.01f)
		{
			fluc3 = UnityEngine.Random.value;
		}
		fade = player.rippleDeathIntensity * 0.95f + player.rippleDeathIntensity * 0.05f * Mathf.Sin(sin * (float)Math.PI * 5f);
		rot = Mathf.Sin(sin * (float)Math.PI * 2f) * 45f * fade + 135f;
		if (!CanShowPlayer(player) || (player.room != room && player.room != null) || player.rippleDeathIntensity <= 0f)
		{
			viableFade = Mathf.Max(0f, viableFade - 1f / 30f);
			if (viableFade <= 0f && lastViableFade <= 0f)
			{
				Destroy();
			}
		}
		else
		{
			viableFade = Mathf.Min(1f, viableFade + 1f / 30f);
			if (player.inShortcut)
			{
				pos = player.MapOwnerInRoomPosition;
			}
			else
			{
				pos = (player.firstChunk.pos * 2f + player.bodyChunks[1].pos) / 3f;
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["RippleDeath"];
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("WarpPoint"));
		sLeaser.sprites[0].MoveToBack();
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float num = TotFade(timeStacker);
		if (num == 0f)
		{
			sLeaser.sprites[0].isVisible = false;
			return;
		}
		sLeaser.sprites[0].isVisible = true;
		sLeaser.sprites[0].x = Mathf.Clamp(Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x, 0f, rCam.sSize.x);
		sLeaser.sprites[0].y = Mathf.Clamp(Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y, 0f, rCam.sSize.y);
		sLeaser.sprites[0].rotation = Mathf.Lerp(lastRot, rot, timeStacker);
		sLeaser.sprites[0].scaleX = (rCam.sSize.x * (6f - 3f * num) + 2f) / 16f;
		sLeaser.sprites[0].scaleY = (rCam.sSize.x * (6f - 3f * num) + 2f) / 16f;
		sLeaser.sprites[0].color = new Color(num, num, 0f, 0f);
	}
}
