using UnityEngine;

namespace Watcher;

public class DaemonCrown : UpdatableAndDeletable
{
	public PlacedObject placedObj;

	public Vector2 position;

	public float radius;

	public int depth;

	public DynamicLevelElement[] crownSegments;

	public float[] crownSegmentOffsets;

	public DaemonCrown(PlacedObject placedObj, Room room, Vector2 position, float radius, int depth)
	{
		this.placedObj = placedObj;
		base.room = room;
		this.position = position;
		this.radius = radius;
		this.depth = depth;
		int num = Mathf.Max(Mathf.FloorToInt(radius / 20f), 1);
		crownSegmentOffsets = new float[num];
		crownSegments = new DynamicLevelElement[num];
		for (int i = 0; i < crownSegments.Length; i++)
		{
			crownSegments[i] = new DynamicLevelElement(CrownSegmentPosition(i), new Vector2(2f, 2f), null, 0, null, "DynamicLevelDaemonCrown")
			{
				dontSetProps = true
			};
			room.AddObject(crownSegments[i]);
		}
	}

	public Vector2 CrownSegmentPosition(int index)
	{
		float t = (float)index / ((float)crownSegments.Length - 1f);
		return position + Vector2.right * Mathf.Lerp(0f - radius, radius, t);
	}

	public override void Update(bool eu)
	{
		DaemonCrownData daemonCrownData = placedObj.data as DaemonCrownData;
		position = placedObj.pos;
		depth = daemonCrownData.depth;
		radius = daemonCrownData.Rad;
		if (room.PlayersInRoom.Count == 0)
		{
			return;
		}
		for (int i = 0; i < crownSegments.Length; i++)
		{
			Vector2 vector = CrownSegmentPosition(i);
			float num = float.MaxValue;
			foreach (Player item in room.PlayersInRoom)
			{
				if (item?.mainBodyChunk != null)
				{
					num = Mathf.Min((item.mainBodyChunk.pos - vector).magnitude, num);
				}
			}
			float b = Mathf.Lerp(30f, 0f, num / 60f);
			crownSegmentOffsets[i] = Mathf.Lerp(crownSegmentOffsets[i], b, 0.1f);
			crownSegments[i].pos = vector + Vector2.up * crownSegmentOffsets[i] + Vector2.up * (Mathf.Sin((float)room.game.clock * 0.05f + (float)(i * 100)) * 2f);
			crownSegments[i].element?.SetProperty(0, 1f - (float)depth / 30f);
		}
	}
}
