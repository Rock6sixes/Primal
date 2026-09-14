using UnityEngine;

namespace DevInterface;

public class WindRepresentation : QuadObjectRepresentation
{
	public WindRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		WindControlPanel windControlPanel = new WindControlPanel(owner, "Wind_Control_Panel", this, new Vector2(0f, 100f));
		subNodes.Add(windControlPanel);
		windControlPanel.pos = Vector2.zero;
		bool flag = false;
		foreach (UpdatableAndDeletable update in owner.room.updateList)
		{
			if (update is WindRect windRect && windRect.placedObj == pObj)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			owner.room.AddObject(new WindRect(pObj));
		}
	}
}
