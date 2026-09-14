using RWCustom;
using UnityEngine;
using Watcher;

namespace DevInterface;

public class BigSkyWhaleSpawnerRepresentation : PlacedObjectRepresentation
{
	public class ControlPanel : Panel
	{
		public class BigSkyWhaleSlider : Slider
		{
			public BigSkyWhaleSpawner.Data Data => (parentNode.parentNode as BigSkyWhaleSpawnerRepresentation).Data;

			public BigSkyWhaleSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "Min_Delay_Slider":
					num = (float)Data.minDelay / 2400f;
					base.NumberText = ((int)(num / 40f * 2400f)).ToString("0.0");
					break;
				case "Max_Delay_Slider":
					num = (float)Data.maxDelay / 2400f;
					base.NumberText = ((int)(num / 40f * 2400f)).ToString("0.0");
					break;
				case "Direction_Slider":
					num = (float)Data.direction * 0.5f + 0.5f;
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
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Min_Delay_Slider":
					Data.minDelay = (int)(nubPos * 2400f);
					break;
				case "Max_Delay_Slider":
					Data.maxDelay = (int)(nubPos * 2400f);
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
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 65f), "Big Sky Whale")
		{
			subNodes.Add(new BigSkyWhaleSlider(owner, "Min_Delay_Slider", this, new Vector2(5f, 45f), "Min Delay: "));
			subNodes.Add(new BigSkyWhaleSlider(owner, "Max_Delay_Slider", this, new Vector2(5f, 25f), "Max Delay: "));
			subNodes.Add(new BigSkyWhaleSlider(owner, "Direction_Slider", this, new Vector2(5f, 5f), "Direction: "));
		}
	}

	public const int delayCap = 2400;

	public ControlPanel controlPanel;

	public int controlPanelLine;

	public BigSkyWhaleSpawner.Data Data => pObj.data as BigSkyWhaleSpawner.Data;

	public BigSkyWhaleSpawnerRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		controlPanel = new ControlPanel(owner, "Big_Sky_Whale_Spawner_Control_Panel", this, Data.panelPos);
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
