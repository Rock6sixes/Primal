using UnityEngine;

namespace DevInterface;

public class TerrainRubbleRepresentation : ResizeableObjectRepresentation
{
	public Vector2 lastPos;

	public float lastRad;

	public PlacedObject.ResizableObjectData Data => pObj.data as PlacedObject.ResizableObjectData;

	public TerrainRubbleRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name, bool showRing)
		: base(owner, IDstring, parentNode, pObj, name, showRing)
	{
		lastPos = pObj.pos;
		lastRad = Data.Rad;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (!(lastPos != pObj.pos) && lastRad == Data.Rad)
		{
			return;
		}
		lastPos = pObj.pos;
		lastRad = Data.Rad;
		if (owner.room.terrain == null)
		{
			return;
		}
		foreach (TerrainManager.ITerrain terrain in owner.room.terrain.terrainList)
		{
			if (terrain is TerrainCurve terrainCurve)
			{
				terrainCurve.UpdateRubble();
			}
		}
	}
}
