using System.Collections.Generic;
using System.Globalization;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class RippleTreeRepresentation : ResizeableObjectRepresentation, IDevUISignals
{
	public class RippleTreeCosmeticPanel : Panel, IDevUISignals
	{
		public SignalSlider coilSlider;

		public SignalSlider spiralSlider;

		public SignalSlider droopySlider;

		public SignalSlider depthSlider;

		public SignalSlider sinWidthSlider;

		public SignalSlider sinDistSlider;

		public SignalSlider sinOffsetSlider;

		public PlacedObject.RippleStalkData Data;

		public RippleTreeCosmeticPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, PlacedObject.RippleStalkData data)
			: base(owner, IDstring, parentNode, pos, new Vector2(300f, 150f), title)
		{
			Data = data;
			coilSlider = new SignalSlider(owner, IDstring + "_slider", this, new Vector2(5f, 5f), "coils", inheritButton: true, 80f, (Data.spiralCoils / 10f).GetValueOrDefault());
			spiralSlider = new SignalSlider(owner, IDstring + "_slider", this, new Vector2(5f, 25f), "spiral amount", inheritButton: true, 80f, Data.spiralAmount.GetValueOrDefault());
			droopySlider = new SignalSlider(owner, IDstring + "_slider", this, new Vector2(5f, 45f), "droopy", inheritButton: true, 80f, Data.droopy.GetValueOrDefault());
			depthSlider = new SignalSlider(owner, IDstring + "_slider", this, new Vector2(5f, 65f), "depth", inheritButton: true, 80f, (Data.depth / 30f).GetValueOrDefault());
			sinWidthSlider = new SignalSlider(owner, IDstring + "_slider", this, new Vector2(5f, 85f), "wave width", inheritButton: true, 80f, (Data.sinWidth / 20f).GetValueOrDefault());
			sinDistSlider = new SignalSlider(owner, IDstring + "_slider", this, new Vector2(5f, 105f), "wave length", inheritButton: true, 80f, (Data.sinDist / 100f).GetValueOrDefault());
			sinOffsetSlider = new SignalSlider(owner, IDstring + "_slider", this, new Vector2(5f, 125f), "wave offset", inheritButton: true, 80f, (Data.sinOffset / 100f).GetValueOrDefault());
			subNodes.AddRange(new List<DevUINode> { droopySlider, spiralSlider, coilSlider, depthSlider, sinWidthSlider, sinDistSlider, sinOffsetSlider });
		}

		public override void Refresh()
		{
			base.Refresh();
			SignalSlider signalSlider = coilSlider;
			float? spiralCoils = Data.spiralCoils;
			signalSlider.NumberText = ((!spiralCoils.HasValue) ? "<R>" : spiralCoils.GetValueOrDefault().ToString("#.##"));
			SignalSlider signalSlider2 = spiralSlider;
			spiralCoils = Data.spiralAmount;
			signalSlider2.NumberText = ((!spiralCoils.HasValue) ? "<R>" : spiralCoils.GetValueOrDefault().ToString("P"));
			SignalSlider signalSlider3 = droopySlider;
			spiralCoils = Data.droopy;
			signalSlider3.NumberText = ((!spiralCoils.HasValue) ? "<R>" : spiralCoils.GetValueOrDefault().ToString("P"));
			SignalSlider signalSlider4 = depthSlider;
			spiralCoils = Data.depth;
			object numberText;
			if (spiralCoils.HasValue)
			{
				float valueOrDefault = spiralCoils.GetValueOrDefault();
				numberText = Mathf.Round(valueOrDefault).ToString();
			}
			else
			{
				numberText = "<R>";
			}
			signalSlider4.NumberText = (string)numberText;
			SignalSlider signalSlider5 = sinWidthSlider;
			spiralCoils = Data.sinWidth;
			object numberText2;
			if (spiralCoils.HasValue)
			{
				float valueOrDefault2 = spiralCoils.GetValueOrDefault();
				numberText2 = Mathf.Round(valueOrDefault2).ToString();
			}
			else
			{
				numberText2 = "<R>";
			}
			signalSlider5.NumberText = (string)numberText2;
			SignalSlider signalSlider6 = sinDistSlider;
			spiralCoils = Data.sinDist;
			object numberText3;
			if (spiralCoils.HasValue)
			{
				float valueOrDefault3 = spiralCoils.GetValueOrDefault();
				numberText3 = Mathf.Round(valueOrDefault3).ToString();
			}
			else
			{
				numberText3 = "<R>";
			}
			signalSlider6.NumberText = (string)numberText3;
			SignalSlider signalSlider7 = sinOffsetSlider;
			spiralCoils = Data.sinOffset;
			object numberText4;
			if (spiralCoils.HasValue)
			{
				float valueOrDefault4 = spiralCoils.GetValueOrDefault();
				numberText4 = Mathf.Round(valueOrDefault4).ToString();
			}
			else
			{
				numberText4 = "<R>";
			}
			signalSlider7.NumberText = (string)numberText4;
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (!(sender is SignalSlider signalSlider))
			{
				return;
			}
			float? num = ((message == "inherit") ? null : new float?(float.Parse(message, NumberStyles.Any, CultureInfo.InvariantCulture)));
			if (!num.HasValue)
			{
				signalSlider.RefreshNubPos(0f);
			}
			if (sender == coilSlider)
			{
				if (num is float)
				{
					num *= 10f;
				}
				Data.spiralCoils = num;
				coilSlider.NumberText = Data.spiralCoils.ToString();
			}
			else if (sender == spiralSlider)
			{
				Data.spiralAmount = num;
			}
			else if (sender == droopySlider)
			{
				Data.droopy = num;
			}
			else if (sender == depthSlider)
			{
				if (num is float)
				{
					num *= 30f;
				}
				Data.depth = num;
			}
			else if (sender == sinWidthSlider)
			{
				if (num is float)
				{
					num *= 20f;
				}
				Data.sinWidth = num;
			}
			else if (sender == sinDistSlider)
			{
				if (num is float)
				{
					num *= 100f;
				}
				Data.sinDist = num;
			}
			else if (sender == sinOffsetSlider)
			{
				if (num is float)
				{
					num *= 100f;
				}
				Data.sinOffset = num;
			}
			Refresh();
			Data.update = true;
		}
	}

	public class RippleTreeInfoPanel : Panel, IDevUISignals
	{
		public PlacedObject.RippleTreeData data;

		public SignalSlider testSlider;

		public int WorldStateTotalEggs
		{
			get
			{
				if (owner?.room?.world.regionState?.rippleSpawnEggState != null)
				{
					return owner.room.world.regionState.rippleSpawnEggState.totalEggs;
				}
				return 0;
			}
		}

		public int WorldStateEggsCollected
		{
			get
			{
				float? testRippleAmount = data.testRippleAmount;
				if (testRippleAmount.HasValue)
				{
					float valueOrDefault = testRippleAmount.GetValueOrDefault();
					return Mathf.FloorToInt(valueOrDefault * (float)WorldStateTotalEggs);
				}
				if (owner?.room?.world.regionState?.rippleSpawnEggState != null)
				{
					return owner.room.world.regionState.rippleSpawnEggState.eggsCollected;
				}
				return 0;
			}
		}

		public float WorldStateEggPercent
		{
			get
			{
				int worldStateTotalEggs = WorldStateTotalEggs;
				if (worldStateTotalEggs == 0)
				{
					return 0f;
				}
				return (float)WorldStateEggsCollected / (float)worldStateTotalEggs;
			}
		}

		public RippleTreeInfoPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, PlacedObject.RippleTreeData data)
			: base(owner, IDstring, parentNode, pos, new Vector2(225f, 90f), title)
		{
			this.data = data;
			subNodes.Add(testSlider = new SignalSlider(owner, "", this, new Vector2(5f, 65f), "test", inheritButton: true, 25f, 0f));
			subNodes.Add(new DevUILabel(owner, "", this, new Vector2(5f, 45f), 215f, ""));
			subNodes.Add(new DevUILabel(owner, "", this, new Vector2(5f, 25f), 215f, ""));
			subNodes.Add(new DevUILabel(owner, "", this, new Vector2(5f, 5f), 215f, ""));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (sender != testSlider)
			{
				return;
			}
			float? testRippleAmount = ((message == "inherit") ? null : new float?(float.Parse(message, NumberStyles.Any, CultureInfo.InvariantCulture)));
			if (!testRippleAmount.HasValue)
			{
				testSlider.RefreshNubPos(0f);
				data.testRippleAmount = null;
			}
			else if (testRippleAmount.HasValue)
			{
				testRippleAmount.GetValueOrDefault();
				data.testRippleAmount = testRippleAmount;
				if (owner?.room?.world.regionState?.rippleSpawnEggState != null && !Input.GetKey(KeyCode.LeftShift))
				{
					float worldStateEggPercent = WorldStateEggPercent;
					data.testRippleAmount = worldStateEggPercent;
					testSlider.RefreshNubPos(worldStateEggPercent);
				}
			}
			data.update = true;
		}

		public override void Refresh()
		{
			base.Refresh();
			float num = Mathf.InverseLerp(0.5f, 1f, data.sproutThreshold);
			float num2 = Custom.LerpMap(data.sproutEnd, 0f, 0.5f, num, 1f);
			float num3 = Mathf.Max(0f, 1f - data.sproutThreshold * 2f);
			float num4 = Mathf.Max(num3, Mathf.InverseLerp(1f, 0.5f, data.sproutEnd));
			float percent = Custom.LerpMap(WorldStateEggPercent, num, num2, num3, num4);
			SignalSlider signalSlider = testSlider;
			float? testRippleAmount = data.testRippleAmount;
			object numberText;
			if (testRippleAmount.HasValue)
			{
				float valueOrDefault = testRippleAmount.GetValueOrDefault();
				numberText = $"{valueOrDefault:P0}";
			}
			else
			{
				numberText = "<null>";
			}
			signalSlider.NumberText = (string)numberText;
			(subNodes[1] as DevUILabel).Text = $"egg progress: {WorldStateEggsCollected}/{WorldStateTotalEggs}, vision: {VisionText(percent)}";
			if (data.sproutThreshold > 0.5f)
			{
				(subNodes[2] as DevUILabel).Text = $"start growing at [{Mathf.Floor((float)WorldStateTotalEggs * num)}] eggs ({num:P0})";
			}
			else
			{
				(subNodes[2] as DevUILabel).Text = $"start at scale {num3:P0}, vision: {VisionText(num3)}";
			}
			if (data.sproutEnd < 0.5f)
			{
				(subNodes[3] as DevUILabel).Text = $"end growing at [{Mathf.Floor((float)WorldStateTotalEggs * num2)}] eggs ({num2:P0})";
			}
			else
			{
				(subNodes[3] as DevUILabel).Text = $"end at scale {num4:P0}, vision: {VisionText(num4)}";
			}
		}

		public string VisionText(float percent)
		{
			if (percent >= 0.25f)
			{
				return $"{Mathf.Floor(Custom.LerpMap(percent, 0.25f, 1f, 10f, 60f))} seconds";
			}
			return "none";
		}
	}

	public SignalSlider sproutStartSlider;

	public SignalSlider sproutEndSlider;

	public Button leftButton;

	public Button cosmeticsButton;

	public Button infoButton;

	public RippleTreeCosmeticPanel cosmeticPanel;

	public RippleTreeInfoPanel infoPanel;

	public Vector2 lastPos;

	public Vector2 lastRad;

	public bool prepUpdate;

	public int timer;

	public int timeBeforeUpdate = 5;

	public PlacedObject.RippleTreeData RippleTreeData => pObj.data as PlacedObject.RippleTreeData;

	public PlacedObject.RippleStalkData Data => pObj.data as PlacedObject.RippleStalkData;

	public bool RippleTree => Data is PlacedObject.RippleTreeData;

	public RippleTreeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name, showRing: true)
	{
		Panel panel = new Panel(owner, IDstring, this, Vector2.zero, RippleTree ? new Vector2(200f, 65f) : new Vector2(150f, 25f), name);
		subNodes.Add(panel);
		leftButton = new Button(owner, IDstring + "_button", panel, new Vector2(5f, 5f), 56f, "Left");
		cosmeticsButton = new Button(owner, IDstring + "_button", panel, new Vector2(71f, 5f), 69f, "Cosmetics");
		panel.subNodes.AddRange(new List<DevUINode> { cosmeticsButton, leftButton });
		if (RippleTree)
		{
			infoButton = new Button(owner, IDstring + "_button", panel, new Vector2(150f, 5f), 41f, "Info");
			sproutStartSlider = new SignalSlider(owner, IDstring + "_slider", panel, new Vector2(5f, 45f), "start", inheritButton: false, 56f, RippleTreeData.sproutThreshold);
			sproutEndSlider = new SignalSlider(owner, IDstring + "_slider", panel, new Vector2(5f, 25f), "end", inheritButton: false, 56f, RippleTreeData.sproutEnd);
			panel.subNodes.AddRange(new List<DevUINode> { sproutStartSlider, sproutEndSlider, infoButton });
		}
		lastPos = Data.owner.pos;
		lastRad = Data.handlePos;
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Refresh()
	{
		base.Refresh();
		if (RippleTree)
		{
			sproutStartSlider.NumberText = Mathf.FloorToInt(RippleTreeData.sproutThreshold * 100f) + "%";
			sproutEndSlider.NumberText = Mathf.FloorToInt(RippleTreeData.sproutEnd * 100f) + "%";
		}
		leftButton.Text = (Data.faceLeft ? "left" : "right");
		if (lastPos != Data.owner.pos)
		{
			prepUpdate = true;
			lastPos = Data.owner.pos;
			timer = 0;
		}
		else if (prepUpdate)
		{
			timer++;
			if (timer >= timeBeforeUpdate)
			{
				Data.update = true;
				prepUpdate = false;
				timer = 0;
			}
		}
		if (lastRad != Data.handlePos)
		{
			Data.update = true;
			lastRad = Data.handlePos;
		}
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender is SignalSlider)
		{
			if (RippleTree)
			{
				float num = float.Parse(message, NumberStyles.Any, CultureInfo.InvariantCulture);
				if (sender == sproutStartSlider)
				{
					RippleTreeData.sproutThreshold = num;
				}
				else if (sender == sproutEndSlider)
				{
					RippleTreeData.sproutEnd = num;
				}
			}
			Refresh();
			Data.update = true;
		}
		else if (sender == leftButton)
		{
			Data.faceLeft = !Data.faceLeft;
			Refresh();
			Data.update = true;
		}
		else if (sender == cosmeticsButton)
		{
			if (cosmeticPanel == null)
			{
				cosmeticPanel = new RippleTreeCosmeticPanel(owner, IDstring, this, Vector2.zero, "Ripple Tree Cosmetics", Data);
				cosmeticPanel.absPos = Custom.RestrictInRect(cosmeticPanel.absPos, new FloatRect(0f, 0f, 1366f - cosmeticPanel.size.x - 10f, 768f - cosmeticPanel.size.y - 30f));
				subNodes.Add(cosmeticPanel);
			}
			else
			{
				subNodes.Remove(cosmeticPanel);
				cosmeticPanel.ClearSprites();
				cosmeticPanel = null;
			}
		}
		else if (sender == infoButton)
		{
			if (infoPanel == null)
			{
				infoPanel = new RippleTreeInfoPanel(owner, IDstring, this, Vector2.zero, "Ripple Tree Info", Data as PlacedObject.RippleTreeData);
				infoPanel.absPos = Custom.RestrictInRect(infoPanel.absPos, new FloatRect(0f, 0f, 1366f - infoPanel.size.x - 10f, 768f - infoPanel.size.y - 30f));
				subNodes.Add(infoPanel);
			}
			else
			{
				subNodes.Remove(infoPanel);
				infoPanel.ClearSprites();
				infoPanel = null;
			}
		}
	}
}
