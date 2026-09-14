using System.Globalization;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class RippleEggFilterRepresentation : ResizeableObjectRepresentation, IDevUISignals
{
	public int lineSprite;

	public SignalSlider slider;

	public PlacedObject.RippleEggFilterData Data => pObj.data as PlacedObject.RippleEggFilterData;

	public Panel panel => subNodes[subNodes.Count - 1] as Panel;

	public string NumberText => Mathf.FloorToInt(Data.threshold * 100f) + "%";

	public RippleEggFilterRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "RippleEggFilter", showRing: true)
	{
		subNodes.Add(new Panel(owner, "RippleEggFilter_Panel", this, new Vector2(0f, 100f), new Vector2(198f, 26f), "Ripple Egg Filter"));
		panel.pos = Data.panelPos;
		slider = new SignalSlider(owner, IDstring, panel, new Vector2(5f, 5f), "threshold", inheritButton: false, 56f, 0f)
		{
			NumberText = Mathf.FloorToInt(Data.threshold * 100f) + "%"
		};
		slider.RefreshNubPos(Data.threshold);
		panel.subNodes.Add(slider);
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = panel.pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, panel.absPos);
		Data.panelPos = panel.pos;
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender == slider)
		{
			float threshold = float.Parse(message, NumberStyles.Any, CultureInfo.InvariantCulture);
			Data.threshold = threshold;
			slider.NumberText = NumberText;
		}
	}
}
