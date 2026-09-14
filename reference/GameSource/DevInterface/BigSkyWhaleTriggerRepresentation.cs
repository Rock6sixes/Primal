using RWCustom;
using UnityEngine;
using Watcher;

namespace DevInterface;

public class BigSkyWhaleTriggerRepresentation : ResizeableObjectRepresentation
{
	public class ControlPanel : Panel
	{
		public class BigSkyWhaleSlider : Slider
		{
			public BigSkyWhaleTrigger.Data Data => (parentNode.parentNode as BigSkyWhaleTriggerRepresentation).Data;

			public BigSkyWhaleSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float nubPos = 0f;
				switch (IDstring)
				{
				case "Wait_Cycles_Slider":
					nubPos = Mathf.InverseLerp(-1f, 10f, Data.waitCycles);
					base.NumberText = ((Data.waitCycles == -1) ? "N/A" : Data.waitCycles.ToString());
					break;
				case "Direction_Slider":
					nubPos = (float)Data.direction * 0.5f + 0.5f;
					if (Data.direction < 0)
					{
						base.NumberText = "L";
					}
					else if (Data.direction > 0)
					{
						base.NumberText = "R";
					}
					else
					{
						base.NumberText = "LR";
					}
					break;
				}
				RefreshNubPos(nubPos);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Wait_Cycles_Slider":
					Data.waitCycles = Mathf.RoundToInt(Mathf.Lerp(-1f, 10f, nubPos));
					break;
				case "Direction_Slider":
					Data.direction = Mathf.RoundToInt(nubPos * 2f) - 1;
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public ControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Big Sky Whale")
		{
			subNodes.Add(new BigSkyWhaleSlider(owner, "Wait_Cycles_Slider", this, new Vector2(5f, 25f), "Reset Cycles: "));
			subNodes.Add(new BigSkyWhaleSlider(owner, "Direction_Slider", this, new Vector2(5f, 5f), "Direction: "));
		}
	}

	public const int waitCyclesCap = 10;

	public ControlPanel controlPanel;

	public int controlPanelLine;

	public BigSkyWhaleTrigger.Data Data => pObj.data as BigSkyWhaleTrigger.Data;

	public BigSkyWhaleTriggerRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name, showRing: true)
	{
		controlPanel = new ControlPanel(owner, "Big_Sky_Whale_Trigger_Control_Panel", this, Data.panelPos);
		subNodes.Add(controlPanel);
		controlPanelLine = fSprites.Count;
		fSprites.Add(new FSprite("pixel")
		{
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(fSprites[controlPanelLine]);
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(controlPanelLine, absPos);
		fSprites[controlPanelLine].scaleY = controlPanel.pos.magnitude;
		fSprites[controlPanelLine].rotation = Custom.VecToDeg(controlPanel.pos);
		Data.panelPos = controlPanel.pos;
	}
}
