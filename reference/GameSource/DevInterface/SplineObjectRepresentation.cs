using UnityEngine;

namespace DevInterface;

public class SplineObjectRepresentation : PlacedObjectRepresentation
{
	public BezierSplineControl splineControl;

	public PlacedObject.SplineObjectData Data => pObj.data as PlacedObject.SplineObjectData;

	public SplineObjectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		splineControl = new BezierSplineControl(owner, "Spline_Control", this, new Vector2(0f, 0f), Data.spline);
		subNodes.Add(splineControl);
	}
}
