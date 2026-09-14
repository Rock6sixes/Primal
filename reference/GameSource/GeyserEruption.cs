using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class GeyserEruption : UpdatableAndDeletable, IDrawable
{
	public Vector2 origin;

	public Vector2 offset;

	public float width;

	public int age;

	public GeyserEruption(Vector2 origin, Vector2 offset, float width)
	{
		this.origin = origin;
		this.offset = offset;
		this.width = width;
		age = 0;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		age++;
		if (age >= 160)
		{
			Destroy();
		}
		if (age >= 20)
		{
			return;
		}
		Vector2 normalized = offset.normalized;
		Vector2 rhs = Custom.PerpendicularVector(offset);
		float num = offset.magnitude * 0.8f;
		List<PhysicalObject>[] physicalObjects = room.physicalObjects;
		Vector2 vector = default(Vector2);
		foreach (List<PhysicalObject> list in physicalObjects)
		{
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				PhysicalObject physicalObject = list[num2];
				for (int j = 0; j < physicalObject.bodyChunks.Length; j++)
				{
					BodyChunk bodyChunk = physicalObject.bodyChunks[j];
					vector.y = Vector2.Dot(bodyChunk.pos - origin, normalized);
					vector.x = Vector2.Dot(bodyChunk.pos - origin, rhs);
					float num3 = bodyChunk.rad * 0.5f;
					if (!(vector.x < (0f - width) / 2f - num3) && !(vector.x > width / 2f + num3) && !(vector.y < 0f) && !(vector.y > num))
					{
						float num4 = Mathf.Max(2f * Mathf.Sqrt(num - vector.y) - Vector2.Dot(normalized, bodyChunk.vel), 0f) * 0.2f;
						bodyChunk.vel += normalized * (num4 / Mathf.Max(bodyChunk.mass, 1f));
						if (bodyChunk.owner is Player player)
						{
							player.diveForce = 1f;
						}
					}
				}
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1]
		{
			new FSprite("Futile_White")
			{
				anchorY = 0f,
				shader = rCam.game.rainWorld.Shaders["GeyserWater"],
				color = new Color(offset.magnitude / 200f, Random.value, Random.value)
			}
		};
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		newContatiner.AddChild(sLeaser.sprites[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = Mathf.Clamp01(((float)age + timeStacker) / 80f);
		sLeaser.sprites[0].SetPosition(origin - camPos);
		sLeaser.sprites[0].scaleX = (width + 30f * num) / 16f;
		sLeaser.sprites[0].scaleY = offset.magnitude * 2.5f * (Mathf.Pow(num, 0.5f) - Mathf.Pow(num, 1.5f)) / 16f;
		sLeaser.sprites[0].rotation = Custom.VecToDeg(offset);
		sLeaser.sprites[0].alpha = 1f - num;
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}
}
