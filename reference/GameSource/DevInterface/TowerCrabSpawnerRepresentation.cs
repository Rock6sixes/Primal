using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;
using Watcher;

namespace DevInterface;

public class TowerCrabSpawnerRepresentation : PlacedObjectRepresentation
{
	public class ControlPanel : Panel
	{
		public class TowerCrabSlider : Slider
		{
			public TowerCrabSpawner.Data Data => (parentNode as ControlPanel).Data;

			public TowerCrabSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "Frequency_Slider":
					num = Data.frequency;
					base.NumberText = (num * 100f).ToString("0");
					break;
				case "Min_Layer_Slider":
					num = Data.minLayer;
					base.NumberText = (num * 30f).ToString("0");
					break;
				case "Max_Layer_Slider":
					num = Data.maxLayer;
					base.NumberText = (num * 30f).ToString("0");
					break;
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Frequency_Slider":
					Data.frequency = nubPos;
					break;
				case "Min_Layer_Slider":
					Data.minLayer = nubPos;
					Data.maxLayer = Mathf.Max(nubPos, Data.maxLayer);
					break;
				case "Max_Layer_Slider":
					Data.maxLayer = nubPos;
					Data.minLayer = Mathf.Min(nubPos, Data.minLayer);
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public Cycler rainCycler;

		public Cycler modeCycler;

		public TowerCrabSpawner.Data Data => (parentNode as TowerCrabSpawnerRepresentation).Data;

		public ControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 105f), "Tower Crab Spawner")
		{
			subNodes.Add(new TowerCrabSlider(owner, "Frequency_Slider", this, new Vector2(5f, 85f), "Frequency: "));
			rainCycler = new Cycler(owner, "Rain_Cycler", this, new Vector2(5f, 65f), 240f, "", new List<string> { "Always Active", "Stop Before Rain" });
			rainCycler.currentAlternative = (Data.ignoreRain ? 1 : 0);
			rainCycler.Text = rainCycler.baseName + rainCycler.alternatives[rainCycler.currentAlternative];
			subNodes.Add(rainCycler);
			subNodes.Add(new TowerCrabSlider(owner, "Min_Layer_Slider", this, new Vector2(5f, 45f), "Min Layer: "));
			subNodes.Add(new TowerCrabSlider(owner, "Max_Layer_Slider", this, new Vector2(5f, 25f), "Max Layer: "));
			List<string> alternatives = ExtEnum<TowerCrabSpawner.Data.Mode>.values.entries.Select(GetModeDescription).ToList();
			modeCycler = new Cycler(owner, "Rain_Cycler", this, new Vector2(5f, 5f), 240f, "", alternatives);
			modeCycler.currentAlternative = ((Data.mode.Index != -1) ? Data.mode.Index : 0);
			modeCycler.Text = modeCycler.baseName + modeCycler.alternatives[modeCycler.currentAlternative];
			subNodes.Add(modeCycler);
		}

		public static string GetModeDescription(string mode)
		{
			return GetModeDescription(new TowerCrabSpawner.Data.Mode(mode));
		}

		public static string GetModeDescription(TowerCrabSpawner.Data.Mode mode)
		{
			if (mode == TowerCrabSpawner.Data.Mode.Border)
			{
				return "Enter from nearest border";
			}
			if (mode == TowerCrabSpawner.Data.Mode.Left)
			{
				return "Move left from here";
			}
			if (mode == TowerCrabSpawner.Data.Mode.Right)
			{
				return "Move right from here";
			}
			return "Unknown Mode";
		}

		public override void Update()
		{
			base.Update();
			Data.ignoreRain = rainCycler.currentAlternative == 1;
			if (Data.mode.Index != modeCycler.currentAlternative)
			{
				Data.mode = new TowerCrabSpawner.Data.Mode(ExtEnum<TowerCrabSpawner.Data.Mode>.values.entries[modeCycler.currentAlternative]);
			}
		}
	}

	public ControlPanel controlPanel;

	public int controlPanelLine;

	public TowerCrabSpawner.Data Data => pObj.data as TowerCrabSpawner.Data;

	public TowerCrabSpawnerRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		controlPanel = new ControlPanel(owner, "Tower_Crab_Spawner_Control_Panel", this, Data.panelPos);
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
