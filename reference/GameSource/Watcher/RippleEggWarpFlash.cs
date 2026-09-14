using UnityEngine;

namespace Watcher;

public class RippleEggWarpFlash : CosmeticSprite
{
	public BodyChunk followChunk;

	public float radius = 100f;

	public float lastFade;

	public float fade;

	public float lastBigArea;

	public float bigArea;

	public int age;

	public int lifetime = 640;

	public RippleEggWarpFlash(BodyChunk followChunk)
	{
		this.followChunk = followChunk;
		if (followChunk != null)
		{
			pos = followChunk.pos;
			lastPos = pos;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastFade = fade;
		lastBigArea = bigArea;
		age++;
		if (age > lifetime)
		{
			Destroy();
		}
		if (followChunk != null && followChunk.owner.room != null && followChunk.owner.room != room)
		{
			followChunk.owner.room.AddObject(this);
		}
		if (followChunk == null || followChunk.owner.room == room)
		{
			float num = (float)age / (float)lifetime;
			bigArea = Mathf.Pow(num, 0.7f) * Mathf.Pow(1f - num, 3f) / 0.16618f;
			fade = Mathf.Min(bigArea * 2f, 1f);
			if (followChunk != null)
			{
				pos = followChunk.pos;
			}
		}
		else
		{
			fade = 0f;
			bigArea = 0f;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["RippleSpawnAbsorption"];
		rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[0]);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		float r = Mathf.Lerp(lastBigArea, bigArea, timeStacker);
		float a = Mathf.Lerp(lastFade, fade, timeStacker);
		sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
		sLeaser.sprites[0].scale = radius / 8f;
		sLeaser.sprites[0].color = new Color(r, 0f, 0f, a);
	}
}
