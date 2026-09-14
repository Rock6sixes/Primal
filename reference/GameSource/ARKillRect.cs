using RWCustom;
using UnityEngine;

public class ARKillRect : UpdatableAndDeletable
{
	public PlacedObject po;

	public IntRect rect;

	public ARKillRect(Room room, PlacedObject pObj)
	{
		base.room = room;
		po = pObj;
		rect = (po.data as PlacedObject.GridRectObjectData).Rect;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
				{
					Vector2 vector = room.physicalObjects[i][j].bodyChunks[k].ContactPoint.ToVector2();
					Vector2 pos = room.physicalObjects[i][j].bodyChunks[k].pos + vector * (room.physicalObjects[i][j].bodyChunks[k].rad + 30f);
					if (Custom.InsideRect(room.GetTilePosition(pos), rect) && room.physicalObjects[i][j] is Creature && !(room.physicalObjects[i][j] as Creature).dead)
					{
						(room.physicalObjects[i][j] as Creature).Die();
					}
				}
			}
		}
	}
}
