using System;
using RWCustom;
using UnityEngine;

public class CurvedSlope : UpdatableAndDeletable, TerrainManager.ITerrain
{
	public PlacedObject.LocalTerrainData data;

	public float segmentWidth = 5f;

	public int segments;

	public float startX;

	public float endX;

	public Vector2[] collisionPoints;

	public bool BurrowAllowed => false;

	public Vector2 pos => data.owner.pos;

	public BezierSpline Spline => data.spline;

	public float thickness => data.bottom;

	public CurvedSlope(Room room, PlacedObject.LocalTerrainData data)
	{
		this.data = data;
		segments = (int)((endX - startX) / segmentWidth);
		collisionPoints = new Vector2[segments];
		RefreshCurve();
	}

	public Vector2 SnapToTerrain(Vector2 center, float radius, out Vector2 normal, Vector2? lastCenter = null)
	{
		int val = Mathf.FloorToInt((center.x - startX - radius) / segmentWidth);
		int val2 = Mathf.CeilToInt((center.x - startX + radius) / segmentWidth);
		float num = float.NaN;
		normal = default(Vector2);
		int val3 = Mathf.RoundToInt((center.x - startX) / segmentWidth);
		Vector2 vector = collisionPoints[Math.Min(Math.Max(val3, 0), segments - 1)];
		Vector2 vector2 = lastCenter ?? center;
		bool flag = vector.y - thickness / 2f > vector2.y;
		for (int i = Math.Max(val, 0); i <= Math.Min(segments - 2, val2); i++)
		{
			Vector2 vector3 = collisionPoints[i];
			Vector2 vector4 = (collisionPoints[i + 1] - vector3).normalized;
			if (flag)
			{
				vector3.y -= thickness;
				vector4 = -vector4;
			}
			float num2 = Mathf.Clamp(center.x + radius * vector4.y, vector3.x, vector3.x + segmentWidth);
			float num3 = Mathf.Sqrt(radius * radius - Mathf.Pow(center.x - num2, 2f));
			if (flag)
			{
				num3 = 0f - num3;
			}
			float num4 = (num2 - vector3.x) * vector4.y / vector4.x + vector3.y + num3;
			if (float.IsNaN(num) || (!flag && num4 > num) || (flag && num4 < num))
			{
				num = num4;
				normal = Custom.PerpendicularVector(vector4);
			}
		}
		if (!float.IsNaN(num))
		{
			center.y = num;
		}
		return center;
	}

	public bool ObstructsTile(int x, int y)
	{
		if ((float)(x + 2) * 20f < startX || endX < (float)(x - 1) * 20f)
		{
			return false;
		}
		int num = Mathf.Clamp(Mathf.FloorToInt(((float)x * 20f - startX) / segmentWidth), 0, segments - 1);
		int num2 = Mathf.Clamp(Mathf.CeilToInt(((float)(x + 1) * 20f - startX) / segmentWidth), 0, segments - 1);
		float num3 = (float)y * 20f + 10f;
		for (int i = num; i <= num2; i++)
		{
			if (collisionPoints[i].y > num3 && num3 > collisionPoints[i].y - thickness)
			{
				return true;
			}
		}
		return false;
	}

	public float GetCoverage(int x, int y)
	{
		if ((float)(x + 1) * 20f < startX || endX < (float)x * 20f)
		{
			return 0f;
		}
		float num = 0f;
		float num2 = (float)y * 20f;
		float num3 = num2 + 20f;
		int num4 = Mathf.Clamp(Mathf.FloorToInt(((float)x * 20f - startX) / segmentWidth), 0, segments - 1);
		int num5 = Mathf.Clamp(Mathf.CeilToInt(((float)(x + 1) * 20f - startX) / segmentWidth), 0, segments - 1);
		for (int i = num4; i <= num5; i++)
		{
			num = ((!(num2 > collisionPoints[i].y - thickness)) ? (num + Mathf.Clamp(num3 - (collisionPoints[i].y - thickness), 0f, 20f) / 20f) : (num + Mathf.Clamp(collisionPoints[i].y - num2, 0f, 20f) / 20f));
		}
		return num / (float)(num5 - num4 + 1);
	}

	public void UpdateSize(float startX, float endX)
	{
		this.startX = startX;
		this.endX = endX;
		segments = (int)((endX - startX) / segmentWidth);
		collisionPoints = new Vector2[segments];
	}

	public void RefreshCurve()
	{
		BezierCurve[] allBeziers = Spline.GetAllBeziers();
		for (int i = 0; i < allBeziers.Length; i++)
		{
			allBeziers[i] += pos;
		}
		float x = allBeziers[0].posA.x;
		float x2 = allBeziers[allBeziers.Length - 1].posB.x;
		if (x != startX || x2 != endX)
		{
			UpdateSize(x, x2);
		}
		int j = 0;
		for (int k = 0; k < segments; k++)
		{
			float num;
			for (num = Mathf.Lerp(startX, endX, (float)k / ((float)segments - 1f)); j < allBeziers.Length - 1 && allBeziers[j].posB.x < num; j++)
			{
			}
			collisionPoints[k] = new Vector2(num, Custom.BezierYatX(allBeziers[j], num));
			collisionPoints[k].y = Normalize(collisionPoints[k].y, -10000f);
		}
	}

	public static float Normalize(float f, float defaultValue)
	{
		if (float.IsNaN(f) || float.IsInfinity(f))
		{
			return defaultValue;
		}
		return f;
	}
}
