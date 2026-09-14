using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class TerrainManager
{
	public interface ITerrain
	{
		bool BurrowAllowed { get; }

		Vector2 SnapToTerrain(Vector2 center, float radius, out Vector2 normal, Vector2? lastCenter = null);

		bool ObstructsTile(int x, int y);

		float GetCoverage(int x, int y);
	}

	public List<ITerrain> terrainList = new List<ITerrain>();

	public TerrainCurve roomTerrain;

	public Vector2 SnapToTerrain(Vector2 center, Vector2? lastCenter = null, bool burrow = false)
	{
		Vector2 normal;
		return SnapToTerrain(center, 0f, out normal, lastCenter, burrow);
	}

	public Vector2 SnapToTerrain(Vector2 center, float radius, Vector2? lastCenter = null, bool burrow = false)
	{
		Vector2 normal;
		return SnapToTerrain(center, radius, out normal, lastCenter, burrow);
	}

	public Vector2 SnapToTerrain(Vector2 center, float radius, out Vector2 normal, Vector2? lastCenter = null, bool burrow = false)
	{
		normal = default(Vector2);
		float num = float.MinValue;
		float num2 = float.MinValue;
		bool flag = false;
		foreach (ITerrain terrain in terrainList)
		{
			if (burrow && terrain.BurrowAllowed)
			{
				continue;
			}
			Vector2 normal2;
			Vector2 vector = terrain.SnapToTerrain(center, radius, out normal2, lastCenter);
			if (!(vector != center))
			{
				continue;
			}
			bool flag2 = Custom.RotateAroundVector(vector, center, Custom.VecToDeg(normal2)).y > center.y;
			if (flag2)
			{
				if (!flag)
				{
					normal = normal2;
					num2 = float.MinValue;
				}
				else
				{
					normal += normal2;
				}
				flag = true;
			}
			if (Mathf.Abs(vector.y - center.y) > num2 && (flag2 || !flag))
			{
				if (!flag)
				{
					normal = normal2;
				}
				num = vector.y;
				num2 = Mathf.Abs(vector.y - center.y);
			}
		}
		normal.Normalize();
		if (!float.IsNaN(num))
		{
			center.y = num;
		}
		return center;
	}

	public bool TrySnapToTerrain(Vector2 center, float radius, out Vector2 snapPos, out Vector2 normal, Vector2? lastCenter = null, bool burrow = false)
	{
		snapPos = SnapToTerrain(center, radius, out normal, lastCenter, burrow);
		return Custom.RotateAroundVector(snapPos, center, Custom.VecToDeg(normal)).y > center.y;
	}

	public bool TrySnapToTerrain(Vector2 center, float radius, out Vector2 snapPos, Vector2? lastCenter = null, bool burrow = false)
	{
		Vector2 normal;
		return TrySnapToTerrain(center, radius, out snapPos, out normal, lastCenter, burrow);
	}

	public bool ObstructsTile(IntVector2 pos)
	{
		return ObstructsTile(pos.x, pos.y);
	}

	public bool ObstructsTile(int x, int y)
	{
		foreach (ITerrain terrain in terrainList)
		{
			if (terrain.ObstructsTile(x, y))
			{
				return true;
			}
		}
		return false;
	}

	public AItile.Accessibility TileAccessibility(int x, int y)
	{
		foreach (ITerrain terrain in terrainList)
		{
			if (terrain.ObstructsTile(x, y))
			{
				return terrain.BurrowAllowed ? AItile.Accessibility.Sand : AItile.Accessibility.Solid;
			}
			if (terrain.ObstructsTile(x, y - 1))
			{
				return AItile.Accessibility.CurvedFloor;
			}
		}
		return (AItile.Accessibility)(-1);
	}

	public bool Contains(Vector2 pos)
	{
		Vector2 snapPos;
		return TrySnapToTerrain(pos, 0f, out snapPos);
	}

	public float GetCoverage(int x, int y)
	{
		float num = 0f;
		foreach (ITerrain terrain in terrainList)
		{
			float coverage = terrain.GetCoverage(x, y);
			if (coverage > num)
			{
				num = coverage;
			}
		}
		return num;
	}
}
