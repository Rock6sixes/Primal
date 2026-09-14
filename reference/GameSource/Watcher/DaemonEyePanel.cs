using DevInterface;
using UnityEngine;

namespace Watcher;

public class DaemonEyePanel : Panel, IDevUISignals
{
	public class DaemonEyeSlider : Slider
	{
		public DaemonEyeSlider(DevUI owner, string idString, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, idString, parentNode, pos, title, inheritButton: false, 110f)
		{
		}

		public override void NubDragged(float nubPos)
		{
			((parentNode.parentNode as DaemonEyeRepresentation).pObj.data as DaemonEyeData).depth = (int)(nubPos * 30f);
			parentNode.parentNode.Refresh();
			Refresh();
		}

		public override void Refresh()
		{
			base.Refresh();
			float num = ((parentNode.parentNode as DaemonEyeRepresentation).pObj.data as DaemonEyeData).depth;
			base.NumberText = (int)num + " ";
			RefreshNubPos(num / 30f);
		}
	}

	public DaemonEyePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
		: base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), "Daemon Eye")
	{
		subNodes.Add(new DaemonEyeSlider(owner, "Depth_Slider", this, new Vector2(5f, 5f), "Depth: "));
	}

	public override void Move(Vector2 newPos)
	{
		base.Move(newPos);
		parentNode.Refresh();
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
	}
}
