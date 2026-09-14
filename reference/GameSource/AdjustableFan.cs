using UnityEngine;

public class AdjustableFan : SpinningFan
{
	public new PlacedObject.AdjustableFanData data;

	public AdjustableFan(PlacedObject placedObject, Room currentRoom)
		: base(placedObject, currentRoom, "smallfan", 6.4f)
	{
		data = placedObject.data as PlacedObject.AdjustableFanData;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		FanElement.scale = new Vector2(6.4f * Mathf.Lerp(0.25f, 1.75f, data.scale), 6.4f * Mathf.Lerp(0.25f, 1.75f, data.scale));
	}
}
