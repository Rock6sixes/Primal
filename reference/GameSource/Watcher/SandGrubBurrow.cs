using RWCustom;
using UnityEngine;

namespace Watcher;

public class SandGrubBurrow : UpdatableAndDeletable, IDrawable
{
	public Vector2 pos;

	public Vector2 dir;

	public SandGrub grub;

	public SandGrubNetwork network;

	public PlacedObject placedObject;

	public SandGrubBurrow(Room room, PlacedObject placedObject)
	{
		this.placedObject = placedObject;
		if (placedObject != null)
		{
			pos = room.FindGroundBelow(placedObject.pos, out dir);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (placedObject != null)
		{
			pos = room.FindGroundBelow(placedObject.pos, out dir);
		}
	}

	public void SetNetwork(SandGrubNetwork network)
	{
		this.network = network;
	}

	public void GrubPulled()
	{
		grub = null;
	}

	public void GrubKilled()
	{
		network?.MarkConsumed();
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("rubble" + Random.Range(0, 8))
		{
			anchorY = 0f
		};
		sLeaser.sprites[1] = new FSprite("rubble" + Random.Range(0, 8))
		{
			anchorY = 0f
		};
		AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[1]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (rCam.terrainPalette != null)
		{
			sLeaser.sprites[0].color = Color.Lerp(rCam.terrainPalette.DarkDustColor, palette.blackColor, 0.2f);
			sLeaser.sprites[1].color = Color.Lerp(rCam.terrainPalette.DarkDustColor, palette.blackColor, 0.2f);
		}
		else
		{
			sLeaser.sprites[0].color = palette.texture.GetPixel(6, 3);
			sLeaser.sprites[1].color = palette.texture.GetPixel(3, 3);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float rotation = Custom.VecToDeg(dir);
		Vector2 vector = pos;
		sLeaser.sprites[0].SetPosition(vector - camPos);
		sLeaser.sprites[0].rotation = rotation;
		sLeaser.sprites[0].scaleX = 0.75f;
		sLeaser.sprites[0].scaleY = 0.5f;
		sLeaser.sprites[1].SetPosition(vector - camPos);
		sLeaser.sprites[1].rotation = rotation;
		sLeaser.sprites[1].scaleX = 0.75f;
		sLeaser.sprites[1].scaleY = -0.5f;
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}
}
