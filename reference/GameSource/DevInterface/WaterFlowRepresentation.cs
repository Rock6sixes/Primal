using RWCustom;
using UnityEngine;

namespace DevInterface;

public class WaterFlowRepresentation : PlacedObjectRepresentation
{
	public SliderHandle slider;

	public FluxWaterfallControlPanel controlPanel;

	public WaterFlowRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		PlacedObject.WaterFlowData waterFlowData = pObj.data as PlacedObject.WaterFlowData;
		subNodes.Add(slider = new SliderHandle(owner, "Waterfall_Width", this, new Vector2(0f, 0f), (float)waterFlowData.width * 20f, vertical: false, drawLine: true));
		controlPanel = new FluxWaterfallControlPanel(owner, "Waterfall_Control_Panel", this, new Vector2(0f, 100f), name);
		subNodes.Add(controlPanel);
		controlPanel.pos = Vector2.zero;
		fSprites.Add(new FSprite("pixel")
		{
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
	}

	public override void Refresh()
	{
		PlacedObject.WaterFlowData waterFlowData = pObj.data as PlacedObject.WaterFlowData;
		pObj.pos.x = Mathf.Round(pObj.pos.x / 20f) * 20f;
		pObj.pos.y = Mathf.Round(pObj.pos.y / 20f) * 20f;
		waterFlowData.width = Mathf.Max(Mathf.RoundToInt(slider.Value / 20f), 1);
		slider.Value = (float)waterFlowData.width * 20f;
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scaleY = controlPanel.pos.magnitude;
		fSprites[1].rotation = Custom.VecToDeg(controlPanel.pos);
	}
}
