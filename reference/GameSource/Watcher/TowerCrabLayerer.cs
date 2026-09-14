using UnityEngine;

namespace Watcher;

public class TowerCrabLayerer : UpdatableAndDeletable, IDrawable
{
	public void Add(RoomCamera rCam, IDrawable crab)
	{
		rCam.MoveObjectToInternalContainer(crab, this, 0);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[0];
		sLeaser.containers = new FContainer[1]
		{
			new FContainer
			{
				shouldSortByZ = true
			}
		};
		AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("ForegroundLights");
		}
		sLeaser.RemoveAllSpritesFromContainer();
		newContatiner.AddChild(sLeaser.containers[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}
}
