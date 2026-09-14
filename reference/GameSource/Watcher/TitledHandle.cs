using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class TitledHandle : Handle
{
	public string defaultTitle;

	public TitledHandle(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
		: base(owner, IDstring, parentNode, pos)
	{
		defaultTitle = title;
		fLabels.Add(new FLabel(Custom.GetFont(), title));
		owner.placedObjectsContainer.AddChild(fLabels[0]);
	}

	public override void Update()
	{
		base.Update();
		fLabels[0].isVisible = base.MouseOver;
		fLabels[0].SetPosition(absPos + Vector2.up * 20f);
		fLabels[0].color = (Input.GetKey("b") ? Color.black : Color.white);
	}

	public void SetName(string text)
	{
		if (text == null)
		{
			text = defaultTitle;
		}
		fLabels[0].text = text;
	}
}
