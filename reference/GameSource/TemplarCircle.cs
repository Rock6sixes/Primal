using UnityEngine;

public class TemplarCircle : UpdatableAndDeletable, IDrawable
{
	public Vector2 pos;

	public Vector2 lastPos;

	public bool followSource;

	public PhysicalObject source;

	public float maxThickness = 15f;

	public float maxAlpha = 0.7f;

	public float rad;

	public float lastRad;

	public float radIncrement;

	public float radAcceleration;

	public float radDamping;

	public int lifeTime;

	public int frame;

	public TemplarCircle(PhysicalObject source, Vector2 pos, float rad, float radIncrement, float radAcceleration, int lifeTime, bool followSource)
	{
		this.source = source;
		this.pos = pos;
		this.rad = rad;
		this.radIncrement = radIncrement;
		this.radAcceleration = radAcceleration;
		this.lifeTime = lifeTime;
		this.followSource = followSource;
		lastPos = pos;
		lastRad = rad;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastPos = pos;
		lastRad = rad;
		rad += radIncrement;
		radIncrement += radAcceleration;
		radIncrement *= 1f - radDamping;
		if (source != null && followSource)
		{
			if (room == source.room)
			{
				pos = source.firstChunk.pos;
			}
			else
			{
				followSource = false;
			}
		}
		frame++;
		if (frame >= lifeTime)
		{
			Destroy();
		}
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["TemplarCircle"]
		};
		AddToContainer(sLeaser, rCam, null);
	}

	public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
	{
		if (newContainer == null)
		{
			newContainer = rCam.ReturnFContainer("Foreground");
		}
		newContainer.AddChild(sLeaser.sprites[0]);
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		float num = Mathf.Lerp(lastRad, rad, timeStacker);
		float num2 = ((float)(lifeTime - frame) - timeStacker) / (float)lifeTime;
		float num3 = Mathf.Pow(num2, 0.5f) * maxThickness;
		sLeaser.sprites[0].SetPosition(vector - camPos);
		sLeaser.sprites[0].scale = num / 8f;
		sLeaser.sprites[0].color = new Color(num3 / num, 0f, 0f, num2 * maxAlpha);
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}
}
