using System;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class RippleLevelFilterRepresentation : ResizeableObjectRepresentation
{
	public class RippleLevelFilterControlPanel : Panel
	{
		public class RippleLevelSlider : Slider
		{
			public int index;

			public float maxRippleLevelOnBar = 5f;

			public PlacedObject.RippleLevelFilterData RippleLevelFilterData => (parentNode.parentNode as RippleLevelFilterRepresentation).pObj.data as PlacedObject.RippleLevelFilterData;

			public RippleLevelSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int index)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
				this.index = index;
			}

			public override void Refresh()
			{
				base.Refresh();
				if (index == 0)
				{
					base.NumberText = RippleLevelFilterData.minimumRippleLevel.ToString();
					RefreshNubPos(RippleLevelFilterData.minimumRippleLevel / maxRippleLevelOnBar);
				}
				else if (index == 1)
				{
					base.NumberText = RippleLevelFilterData.maximumRippleLevel.ToString();
					RefreshNubPos(RippleLevelFilterData.maximumRippleLevel / maxRippleLevelOnBar);
				}
			}

			public override void NubDragged(float nubPos)
			{
				if (index == 0)
				{
					RippleLevelFilterData.minimumRippleLevel = (float)Math.Round(maxRippleLevelOnBar * nubPos, 1);
				}
				else if (index == 1)
				{
					RippleLevelFilterData.maximumRippleLevel = (float)Math.Round(maxRippleLevelOnBar * nubPos, 1);
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public RippleLevelFilterControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Placable Ripple Level Filter")
		{
			subNodes.Add(new RippleLevelSlider(owner, "Minimum_Slider", this, new Vector2(5.01f, 25f), "MIN Ripple Level:", 0));
			subNodes.Add(new RippleLevelSlider(owner, "Maximum_Slider", this, new Vector2(5.01f, 5f), "MAX Ripple Level:", 1));
		}
	}

	public int lineSprite;

	public RippleLevelFilterRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "RippleLevelFilter", showRing: true)
	{
		subNodes.Add(new RippleLevelFilterControlPanel(owner, "RippleLevelFilter_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as RippleLevelFilterControlPanel).pos = (pObj.data as PlacedObject.RippleLevelFilterData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = (subNodes[1] as RippleLevelFilterControlPanel).pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as RippleLevelFilterControlPanel).absPos);
		(pObj.data as PlacedObject.RippleLevelFilterData).panelPos = (subNodes[1] as Panel).pos;
	}
}
