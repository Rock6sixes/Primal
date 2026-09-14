using RWCustom;
using UnityEngine;

namespace DevInterface;

public class AdjustableFanRepresentation : PlacedObjectRepresentation
{
	public class AdjustableFanPanel : Panel
	{
		public class FanSlider : Slider
		{
			public FanSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "Speed_Slider":
					num = ((parentNode.parentNode as AdjustableFanRepresentation).pObj.data as PlacedObject.AdjustableFanData).speed;
					base.NumberText = ((int)Mathf.Lerp(-100f, 100f, num)).ToString();
					break;
				case "Scale_Slider":
					num = ((parentNode.parentNode as AdjustableFanRepresentation).pObj.data as PlacedObject.AdjustableFanData).scale;
					base.NumberText = ((int)Mathf.Lerp(0f, 100f, num)).ToString();
					break;
				case "Depth_Slider":
					num = ((parentNode.parentNode as AdjustableFanRepresentation).pObj.data as PlacedObject.AdjustableFanData).depth;
					base.NumberText = ((int)Mathf.Lerp(0f, 100f, num)).ToString();
					break;
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Speed_Slider":
					((parentNode.parentNode as AdjustableFanRepresentation).pObj.data as PlacedObject.AdjustableFanData).speed = nubPos;
					break;
				case "Scale_Slider":
					((parentNode.parentNode as AdjustableFanRepresentation).pObj.data as PlacedObject.AdjustableFanData).scale = nubPos;
					break;
				case "Depth_Slider":
					((parentNode.parentNode as AdjustableFanRepresentation).pObj.data as PlacedObject.AdjustableFanData).depth = nubPos;
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public AdjustableFanPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 75f), "Adjustable Fan")
		{
			subNodes.Add(new FanSlider(owner, "Speed_Slider", this, new Vector2(5f, 45f), "Speed: "));
			subNodes.Add(new FanSlider(owner, "Scale_Slider", this, new Vector2(5f, 25f), "Scale: "));
			subNodes.Add(new FanSlider(owner, "Depth_Slider", this, new Vector2(5f, 5f), "Depth: "));
		}
	}

	public AdjustableFanRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Adjustable Fan")
	{
		subNodes.Add(new AdjustableFanPanel(owner, "Adjustable_Fan_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as AdjustableFanPanel).pos = (pObj.data as PlacedObject.AdjustableFanData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites[fSprites.Count - 1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		MoveSprite(2, absPos);
		fSprites[1].scaleY = (subNodes[0] as AdjustableFanPanel).pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as AdjustableFanPanel).absPos);
	}
}
