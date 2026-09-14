using UnityEngine;

namespace DevInterface;

public class IntegerControl : PositionedDevUINode, IDevUISignals
{
	public string NumberLabelText
	{
		get
		{
			return subNodes[1].fLabels[0].text;
		}
		set
		{
			subNodes[1].fLabels[0].text = value;
		}
	}

	public IntegerControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
		: base(owner, IDstring, parentNode, pos)
	{
		subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0f, 0f), 110f, title));
		subNodes.Add(new DevUILabel(owner, "Number", this, new Vector2(140f, 0f), 36f, "0"));
		subNodes.Add(new ArrowButton(owner, "Less", this, new Vector2(120f, 0f), -90f));
		subNodes.Add(new ArrowButton(owner, "More", this, new Vector2(180f, 0f), 90f));
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		int num = 1;
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift))
		{
			num = 1000;
		}
		else if (Input.GetKey(KeyCode.LeftControl))
		{
			num = 100;
		}
		else if (Input.GetKey(KeyCode.LeftShift))
		{
			num = 10;
		}
		if (sender.IDstring == "Less")
		{
			Increment(-num);
		}
		if (sender.IDstring == "More")
		{
			Increment(num);
		}
	}

	public virtual void Increment(int change)
	{
	}
}
