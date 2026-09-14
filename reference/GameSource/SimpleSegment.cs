using UnityEngine;

public struct SimpleSegment
{
	public Vector2 pos;

	public Vector2 lastPos;

	public Vector2 vel;

	public void Reset(Vector2 pos)
	{
		this.pos = pos;
		lastPos = pos;
		vel = Vector2.zero;
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}
}
