using RWCustom;
using UnityEngine;

namespace DevInterface;

public class PomegranateRepresentation : ConsumableRepresentation
{
	public FSprite arrowSprite;

	public FSprite arrowTipSprite;

	public Handle handle;

	public PomegranateRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, pObj.type.ToString())
	{
		arrowSprite = new FSprite("pixel");
		fSprites.Insert(1, arrowSprite);
		arrowTipSprite = new FSprite("pixel");
		arrowTipSprite.scaleY = 10f;
		fSprites.Insert(2, arrowTipSprite);
		owner.placedObjectsContainer.AddChild(arrowSprite);
		owner.placedObjectsContainer.AddChild(arrowTipSprite);
		handle = new Handle(owner, "Dir_Handle", this, new Vector2(100f, 0f));
		subNodes.Insert(0, handle);
		handle.pos = (pObj.data as PlacedObject.PomegranateData).handlePos;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		MoveSprite(2, absPos - handle.pos);
		arrowSprite.scaleY = handle.pos.magnitude * 2f;
		arrowSprite.rotation = Custom.AimFromOneVectorToAnother(absPos, handle.absPos);
		arrowTipSprite.rotation = arrowSprite.rotation + 90f;
		(pObj.data as PlacedObject.PomegranateData).handlePos = handle.pos;
	}
}
