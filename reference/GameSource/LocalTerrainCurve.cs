using RWCustom;
using UnityEngine;

public class LocalTerrainCurve : TerrainCurve, IDrawable, TerrainManager.ITerrain
{
	public BezierSpline spline;

	public PlacedObject.LocalTerrainData data;

	public Vector2 pos;

	public LocalTerrainCurve(Room room, PlacedObject.LocalTerrainData data)
		: base(room)
	{
		spline = data.spline;
		this.data = data;
		pos = data.owner.pos;
		RefreshCurve();
	}

	public void RefreshCurve()
	{
		pos = data.owner.pos;
		BezierCurve[] allBeziers = spline.GetAllBeziers();
		for (int i = 0; i < allBeziers.Length; i++)
		{
			allBeziers[i] += pos;
		}
		float num = pos.y - data.bottom;
		float x = allBeziers[0].posA.x;
		float x2 = allBeziers[allBeziers.Length - 1].posB.x;
		if (x != startX || x2 != endX || num != bottom)
		{
			UpdateSize(x, x2, num);
		}
		int j = 0;
		for (int k = 0; k < segments; k++)
		{
			float num2;
			for (num2 = Mathf.Lerp(startX, endX, (float)k / ((float)segments - 1f)); j < allBeziers.Length - 1 && allBeziers[j].posB.x < num2; j++)
			{
			}
			frontPoints[k] = new Vector2(num2, Custom.BezierYatX(allBeziers[j], num2));
			BezierCurve bezier = allBeziers[j];
			float num3 = 50f;
			bezier.posA.y += num3;
			bezier.posB.y += num3;
			backPoints[k] = new Vector2(num2, Custom.BezierYatX(bezier, num2));
			frontPoints[k].y = TerrainCurve.Normalize(frontPoints[k].y, -10000f);
			backPoints[k].y = TerrainCurve.Normalize(backPoints[k].y, -10000f);
		}
		UpdateCollision();
		maskSource?.SetVertices(frontPoints, backPoints, num);
	}

	public override TerrainCurveMaskSource CreateMaskSource()
	{
		return new TerrainCurveMaskSource(frontPoints, backPoints, minDepth, maxDepth, bottom);
	}
}
