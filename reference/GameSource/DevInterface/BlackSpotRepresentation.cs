namespace DevInterface;

public class BlackSpotRepresentation : QuadObjectRepresentation
{
	public BlackSpotRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		BlackSpot blackSpot = null;
		foreach (UpdatableAndDeletable update in owner.room.updateList)
		{
			if (update is BlackSpot blackSpot2 && blackSpot2.pObj == pObj)
			{
				blackSpot = blackSpot2;
				break;
			}
		}
		if (blackSpot == null)
		{
			owner.room.AddObject(new BlackSpot(pObj));
		}
	}
}
