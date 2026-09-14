using System.Linq;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class GooDripsRepresentation : ResizeableObjectRepresentation
{
	public class GooDripsControlPanel : Panel
	{
		public class GooDripsSlider : Slider
		{
			public GooDripsRepresentation Rep => parentNode.parentNode as GooDripsRepresentation;

			public GooDripSource.GooDripsData Data => Rep.pObj.data as GooDripSource.GooDripsData;

			public GooDripsSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Frequency_Slider")
				{
					num = Data.frequency;
					base.NumberText = Mathf.RoundToInt(num * 100f) + "%";
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Frequency_Slider")
				{
					Data.frequency = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public GooDripsControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), "Goo Drips")
		{
			subNodes.Add(new GooDripsSlider(owner, "Frequency_Slider", this, new Vector2(5f, 5f), "Frequency: "));
		}
	}

	public GooDripSource drips;

	public GooDripsControlPanel controlPanel;

	public int lineSprite;

	public Vector2 lastPos;

	public GooDripSource.GooDripsData Data => pObj.data as GooDripSource.GooDripsData;

	public GooDripsRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name, showRing: true)
	{
		controlPanel = new GooDripsControlPanel(owner, "Goo_Drips_Panel", this, new Vector2(0f, 100f));
		subNodes.Add(controlPanel);
		controlPanel.pos = Data.panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
		drips = owner.room.updateList.FirstOrDefault((UpdatableAndDeletable obj) => obj is GooDripSource gooDripSource && gooDripSource.placedObject == pObj) as GooDripSource;
		if (drips == null)
		{
			drips = new GooDripSource(pObj);
			owner.room.AddObject(drips);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = controlPanel.pos.magnitude;
		fSprites[lineSprite].rotation = Custom.VecToDeg(controlPanel.pos);
		Data.panelPos = controlPanel.pos;
		if (pObj.pos != lastPos)
		{
			lastPos = pObj.pos;
			drips?.RefreshCeilingTiles();
		}
	}
}
