using DevInterface;
using UnityEngine;

namespace Watcher;

public class SpinningTopSpotRepresentation : PlacedObjectRepresentation
{
	public SpinningTopPanel controlPanel;

	public SpinningTopSpotRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, pObj.type.ToString())
	{
		controlPanel = new SpinningTopPanel(owner, this, new Vector2(0f, 100f));
		subNodes.Add(controlPanel);
		controlPanel.pos = (pObj.data as SpinningTopData).panelPos;
	}

	public override void Refresh()
	{
		base.Refresh();
		(pObj.data as SpinningTopData).panelPos = (subNodes[subNodes.Count - 1] as Panel).pos;
	}
}
