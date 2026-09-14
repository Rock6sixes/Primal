using UnityEngine;

public class WindDirection : UpdatableAndDeletable
{
	public Vector2 direction;

	public PlacedObject.ResizableObjectData data;

	public WindDirection(Room room, PlacedObject pObj)
	{
		data = pObj.data as PlacedObject.ResizableObjectData;
		direction = (pObj.data as PlacedObject.ResizableObjectData).handlePos;
	}

	public override void Update(bool eu)
	{
		direction = data.handlePos;
		base.Update(eu);
	}
}
