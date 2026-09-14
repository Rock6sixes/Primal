using RWCustom;
using UnityEngine;

public class SuperSlope : UpdatableAndDeletable, TerrainManager.ITerrain
{
	public Vector2 pos;

	public Vector2 pos2;

	public float thickness = 20f;

	public PlacedObject.ResizableObjectData data;

	public bool BurrowAllowed => false;

	public SuperSlope(Room room, PlacedObject.SuperSlopeData data)
	{
		this.data = data;
		thickness = data.bottom;
		pos = data.owner.pos;
		pos2 = pos + data.handlePos;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		pos = data.owner.pos;
		pos2 = pos + data.handlePos;
	}

	public Vector2 SnapToTerrain(Vector2 center, float radius, out Vector2 normal, Vector2? lastCenter = null)
	{
		normal = -Custom.PerpendicularVector(pos, pos2);
		if (pos2.x < pos.x)
		{
			normal = -normal;
		}
		if (center.x - radius < Mathf.Min(pos.x, pos2.x) || center.x + radius > Mathf.Max(pos.x, pos2.x))
		{
			return center;
		}
		Vector2 vector = Vector2.Lerp(pos, pos2, Mathf.InverseLerp(pos.x, pos2.x, center.x));
		Vector2 obj = lastCenter ?? center;
		if (obj.y < vector.y - thickness / 2f)
		{
			normal = -normal;
			return Vector2.Lerp(pos - new Vector2(0f, thickness), pos2 - new Vector2(0f, thickness), Mathf.InverseLerp(pos.x, pos2.x, center.x)) - new Vector2(0f, radius);
		}
		return vector + new Vector2(0f, radius);
	}

	public float GetCoverage(int x, int y)
	{
		int num = 0;
		for (int i = 0; i < 20; i++)
		{
			Vector2 vector = new Vector2((float)x * 20f + (float)i, (float)y * 20f);
			if (Mathf.Min(pos.x, pos2.x) <= vector.x && vector.x <= Mathf.Max(pos.x, pos2.x))
			{
				Vector2 vector2 = Vector2.Lerp(pos, pos2, Mathf.InverseLerp(pos.x, pos2.x, vector.x));
				float num2 = vector.y - vector2.y;
				num = ((!(num2 >= 0f - thickness)) ? (num + (int)Mathf.Clamp(20f - (0f - thickness - num2), 0f, 20f)) : (num + (int)Mathf.Clamp(20f - num2, 0f, 20f)));
			}
		}
		return (float)num / 400f;
	}

	public bool ObstructsTile(int x, int y)
	{
		Vector2 vector = new Vector2((float)x * 20f, (float)y * 20f);
		if (vector.x + 20f < Mathf.Min(pos.x, pos2.x) || Mathf.Max(pos.x, pos2.x) < vector.x)
		{
			return false;
		}
		Vector2 vector2 = Vector2.Lerp(pos, pos2, Mathf.InverseLerp(pos.x, pos2.x, vector.x));
		Vector2 vector3 = Vector2.Lerp(pos, pos2, Mathf.InverseLerp(pos.x, pos2.x, vector.x + 20f));
		float num = Mathf.Min(vector2.y, vector3.y) - thickness + 8f;
		float num2 = Mathf.Max(vector2.y, vector3.y) - 8f;
		if (vector.y + 20f > num2 && vector.y < num2)
		{
			return true;
		}
		if (vector.y > num && vector.y + 20f < num2)
		{
			return true;
		}
		if (vector.y < num && vector.y + 20f > num)
		{
			return true;
		}
		return false;
	}
}
