using RWCustom;
using UnityEngine;

namespace DevInterface;

public class HugeTurbineRepresentation : PlacedObjectRepresentation
{
	public class HugeTurbinePanel : Panel
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
					num = ((parentNode.parentNode as HugeTurbineRepresentation).pObj.data as PlacedObject.FanData).speed;
					base.NumberText = ((int)Mathf.Lerp(-100f, 100f, num)).ToString();
					break;
				case "Depth_Slider":
					num = ((parentNode.parentNode as HugeTurbineRepresentation).pObj.data as PlacedObject.FanData).depth;
					base.NumberText = ((int)Mathf.Lerp(0f, 30f, num)).ToString();
					break;
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Speed_Slider":
					((parentNode.parentNode as HugeTurbineRepresentation).pObj.data as PlacedObject.FanData).speed = nubPos;
					break;
				case "Depth_Slider":
					((parentNode.parentNode as HugeTurbineRepresentation).pObj.data as PlacedObject.FanData).depth = nubPos;
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public HugeTurbinePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 75f), "Huge Turbine")
		{
			subNodes.Add(new FanSlider(owner, "Speed_Slider", this, new Vector2(5f, 45f), "Speed: "));
			subNodes.Add(new FanSlider(owner, "Depth_Slider", this, new Vector2(5f, 25f), "Depth: "));
		}
	}

	public HugeTurbineRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Huge Turbine")
	{
		subNodes.Add(new HugeTurbinePanel(owner, "Huge_Turbine_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as HugeTurbinePanel).pos = (pObj.data as PlacedObject.FanData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites[fSprites.Count - 1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		MoveSprite(2, absPos);
		fSprites[1].scaleY = (subNodes[0] as HugeTurbinePanel).pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as HugeTurbinePanel).absPos);
	}
}
