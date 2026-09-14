using UnityEngine;

namespace DevInterface;

public class ButtonWithSelectPanel : Button, IDevUISignals
{
	public delegate SelectPanel MakeSelectPanel(ButtonWithSelectPanel button);

	public SelectPanel selectPanel;

	public MakeSelectPanel makeSelectPanel;

	public ButtonWithSelectPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width, string text, MakeSelectPanel makeSelectPanel)
		: base(owner, IDstring, parentNode, pos, width, text)
	{
		this.makeSelectPanel = makeSelectPanel;
	}

	public override void Clicked()
	{
		if (selectPanel != null)
		{
			subNodes.Remove(selectPanel);
			selectPanel.ClearSprites();
			selectPanel = null;
		}
		else
		{
			selectPanel = makeSelectPanel(this);
			subNodes.Add(selectPanel);
		}
	}

	public virtual void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender.IDstring == "BackPage99289..?/~")
		{
			selectPanel.PrevPage();
		}
		else if (sender.IDstring == "NextPage99289..?/~")
		{
			selectPanel.NextPage();
		}
		else if (sender.parentNode == selectPanel)
		{
			if (selectPanel != null)
			{
				subNodes.Remove(selectPanel);
				selectPanel.ClearSprites();
				selectPanel = null;
			}
			OnValueChange(sender.IDstring);
		}
	}

	public virtual void OnValueChange(string value)
	{
		DevUINode devUINode = this;
		while (devUINode != null)
		{
			devUINode = devUINode.parentNode;
			if (devUINode is IDevUISignals)
			{
				(devUINode as IDevUISignals).Signal(DevUISignalType.ButtonClick, this, value);
				break;
			}
		}
	}
}
