using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class CurvedSlopeRepresentation : SplineObjectRepresentation
{
	public CurvedSlope curve;

	public List<BezierMesh> dupSprites;

	public new PlacedObject.LocalTerrainData Data => pObj.data as PlacedObject.LocalTerrainData;

	public CurvedSlopeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		dupSprites = new List<BezierMesh>();
		subNodes.Add(new Handle(owner, "Bottom_Handle", this, new Vector2(0f, 0f - Data.bottom)));
		for (int i = 0; i < 2; i++)
		{
			fSprites.Add(new FSprite("pixel")
			{
				anchorY = 0f
			});
			owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		}
		if (owner.room.terrain != null)
		{
			foreach (TerrainManager.ITerrain terrain in owner.room.terrain.terrainList)
			{
				if (terrain is CurvedSlope curvedSlope && curvedSlope.data == Data)
				{
					curve = curvedSlope;
					return;
				}
			}
		}
		owner.room.AddObject(curve = new CurvedSlope(owner.room, Data));
	}

	public override void Update()
	{
		base.Update();
		if (dragged || (subNodes[1] as Handle).dragged || splineControl.Alterred)
		{
			curve.RefreshCurve();
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		Handle handle = subNodes[1] as Handle;
		Data.bottom = Mathf.Max(0f, 0f - handle.pos.y);
		handle.Move(new Vector2(0f, 0f - Data.bottom));
		MoveSprite(1, absPos);
		fSprites[1].scaleY = handle.pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, handle.absPos);
		Vector2 vector = absPos + splineControl.posB;
		Vector2 vector2 = new Vector2(vector.x, vector.y - Data.bottom);
		MoveSprite(2, vector);
		fSprites[2].scaleY = Vector2.Distance(vector, vector2);
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
		while (dupSprites.Count > splineControl.Segments)
		{
			fSprites.Remove(dupSprites[dupSprites.Count - 1]);
			dupSprites[dupSprites.Count - 1].RemoveFromContainer();
			dupSprites.RemoveAt(dupSprites.Count - 1);
		}
		for (int i = 0; i < splineControl.Segments; i++)
		{
			while (dupSprites.Count < i + 1)
			{
				BezierMesh bezierMesh = new BezierMesh(10, customColor: false, pointyTip: false);
				fSprites.Add(bezierMesh);
				owner.placedObjectsContainer.AddChild(bezierMesh);
				dupSprites.Add(bezierMesh);
			}
			BezierCurve bezier = Data.spline.GetBezier(i);
			bezier += new Vector2(0f, 0f - Data.bottom);
			bezier += absPos;
			bezier = Custom.BezierAbsoluteToRelative(bezier);
			dupSprites[i].SetPosition(bezier.posA);
			dupSprites[i].handleA = bezier.handleA;
			dupSprites[i].posB = bezier.posB;
			dupSprites[i].handleB = bezier.handleB;
			dupSprites[i].ChangeSegments((int)(Data.spline.GetSegmentLength(i) / 5f));
		}
	}
}
