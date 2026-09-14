using RWCustom;
using UnityEngine;

namespace DevInterface;

public class GeyserRepresentation : ResizeableObjectRepresentation
{
	public class GeyserControlPanel : Panel
	{
		public class GeyserSlider : Slider
		{
			public GeyserSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 80f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				if (parentNode.parentNode is GeyserRepresentation geyserRepresentation && geyserRepresentation.pObj.data is GeyserData geyserData && IDstring == "Geyser_Timing")
				{
					base.NumberText = geyserData.timing.ToString("0.00");
					RefreshNubPos(geyserData.timing);
				}
			}

			public override void NubDragged(float nubPos)
			{
				base.NubDragged(nubPos);
				if (parentNode.parentNode is GeyserRepresentation geyserRepresentation && geyserRepresentation.pObj.data is GeyserData geyserData && IDstring == "Geyser_Timing")
				{
					geyserData.timing = nubPos;
					base.NumberText = geyserData.timing.ToString("0.00");
				}
			}
		}

		public GeyserControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), title)
		{
			subNodes.Add(new GeyserSlider(owner, "Geyser_Timing", this, new Vector2(5f, 5f), "Timing"));
		}
	}

	public Panel controlPanel;

	public GeyserRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name, showRing: false)
	{
		subNodes.Add(controlPanel = new GeyserControlPanel(owner, "Geyser_Panel", this, new Vector2(0f, 0f), "Geyser"));
		fSprites.Add(new FSprite("pixel")
		{
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(2, absPos);
		fSprites[2].scaleY = controlPanel.pos.magnitude;
		fSprites[2].rotation = Custom.VecToDeg(controlPanel.pos);
	}
}
