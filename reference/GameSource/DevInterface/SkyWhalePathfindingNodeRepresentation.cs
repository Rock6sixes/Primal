using RWCustom;
using UnityEngine;

namespace DevInterface;

public class SkyWhalePathfindingNodeRepresentation : PlacedObjectRepresentation
{
	public class SkyWhalePathfindingPanel : Panel
	{
		public class SkyWhalePathfindingSlider : Slider
		{
			public int maxIndex = 5;

			public SkyWhalePathfindingSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Index")
				{
					base.NumberText = ((parentNode.parentNode as SkyWhalePathfindingNodeRepresentation).pObj.data as PlacedObject.SkyWhalePathfindingData).index.ToString();
				}
				RefreshNubPos((float)((parentNode.parentNode as SkyWhalePathfindingNodeRepresentation).pObj.data as PlacedObject.SkyWhalePathfindingData).index / (float)maxIndex);
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Index")
				{
					((parentNode.parentNode as SkyWhalePathfindingNodeRepresentation).pObj.data as PlacedObject.SkyWhalePathfindingData).index = (int)(nubPos * (float)maxIndex);
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public SkyWhalePathfindingPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), "SkyWhale pathfinding")
		{
			subNodes.Add(new SkyWhalePathfindingSlider(owner, "Index", this, new Vector2(5f, 5f), "Index: "));
		}
	}

	public SkyWhalePathfindingNodeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "SkyWhale Pathfinding Node")
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 0f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.SkyWhalePathfindingData).handlePos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites[fSprites.Count - 1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		(pObj.data as PlacedObject.SkyWhalePathfindingData).handlePos = (subNodes[0] as Handle).pos;
	}
}
