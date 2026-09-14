using UnityEngine;

namespace DevInterface;

public class SignalSlider : Slider
{
	public SignalSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, bool inheritButton, float titleWidth, float defaultPos)
		: base(owner, IDstring, parentNode, pos, title, inheritButton, titleWidth)
	{
		RefreshNubPos(defaultPos);
	}

	public override void NubDragged(float nubPos)
	{
		RefreshNubPos(nubPos);
		DevUINode devUINode = this;
		while (devUINode != null)
		{
			devUINode = devUINode.parentNode;
			if (devUINode is IDevUISignals)
			{
				(devUINode as IDevUISignals).Signal(null, this, nubPos.ToString());
				break;
			}
		}
	}

	public override void ClickedResetToInherent()
	{
		if (!(subNodes[2] is Button))
		{
			return;
		}
		DevUINode devUINode = this;
		while (devUINode != null)
		{
			devUINode = devUINode.parentNode;
			if (devUINode is IDevUISignals)
			{
				(devUINode as IDevUISignals).Signal(null, this, "inherit");
				break;
			}
		}
	}
}
