using System.Linq;
using UnityEngine;

namespace DevInterface;

public class PlateTreeRepresentation : ResizeableObjectRepresentation
{
	public PlateTree tree;

	public Vector2 lastPos;

	public Vector2 lastHandle;

	public PlateTreeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name, PlateTree.Variant variant)
		: base(owner, IDstring, parentNode, pObj, name, showRing: false)
	{
		tree = (PlateTree)owner.room.updateList.FirstOrDefault((UpdatableAndDeletable obj) => obj is PlateTree plateTree && plateTree.pObj == pObj);
		if (tree == null)
		{
			tree = new PlateTree(pObj, owner.room, variant);
			owner.room.AddObject(tree);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		PlacedObject.ResizableObjectData resizableObjectData = (PlacedObject.ResizableObjectData)pObj.data;
		if (lastPos != pObj.pos || lastHandle != resizableObjectData.handlePos)
		{
			lastPos = pObj.pos;
			lastHandle = resizableObjectData.handlePos;
			tree.Reset();
		}
	}
}
