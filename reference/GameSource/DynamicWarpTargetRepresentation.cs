using DevInterface;
using UnityEngine;

public class DynamicWarpTargetRepresentation : PlacedObjectRepresentation
{
	public DynamicWarpTargetPanel controlPanel;

	public DynamicWarpTargetRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, pObj.type.ToString())
	{
		controlPanel = new DynamicWarpTargetPanel(owner, this, new Vector2(0f, 100f));
		subNodes.Add(controlPanel);
		controlPanel.pos = (pObj.data as DynamicWarpTargetData).panelPos;
	}

	public override void Refresh()
	{
		base.Refresh();
		(pObj.data as DynamicWarpTargetData).panelPos = (subNodes[subNodes.Count - 1] as Panel).pos;
	}
}
