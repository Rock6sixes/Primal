using System;
using DevInterface;
using UnityEngine;

public class DynamicWarpTargetPanel : Panel, IDevUISignals
{
	public class RippleLevelSlider : Slider
	{
		public float maxRippleLevelOnBar = 5f;

		public int totalPositionsOnBar = 11;

		public DynamicWarpTargetData Data => (parentNode as DynamicWarpTargetPanel).Data;

		public RippleLevelSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			if (Data.specialWarp)
			{
				RefreshNubPos(1f);
				base.NumberText = "";
				(subNodes[0] as DevUILabel).Text = "Special";
			}
			else
			{
				base.NumberText = Data.rippleLevelRequirement.ToString();
				RefreshNubPos(Data.rippleLevelRequirement / (maxRippleLevelOnBar + 0.5f));
				(subNodes[0] as DevUILabel).Text = "Ripple Tier Required:";
			}
		}

		public override void NubDragged(float nubPos)
		{
			float num = (float)Math.Round((float)totalPositionsOnBar * nubPos);
			if (num == (float)totalPositionsOnBar)
			{
				Data.specialWarp = true;
			}
			else
			{
				Data.rippleLevelRequirement = num / 2f;
			}
			parentNode.parentNode.Refresh();
			Refresh();
		}
	}

	public Button deadEndbutton;

	public Button badWarpButton;

	public DynamicWarpTargetData Data => (parentNode as DynamicWarpTargetRepresentation).pObj.data as DynamicWarpTargetData;

	public DynamicWarpTargetPanel(DevUI owner, DevUINode parentNode, Vector2 pos)
		: base(owner, "DynamicWarpTarget_Panel", parentNode, pos, new Vector2(250f, 65f), "Dynamic Warp Target")
	{
		subNodes.Add(new RippleLevelSlider(owner, "RippleLevel_Slider", this, new Vector2(5.01f, 45f), "Ripple Tier Required:"));
		deadEndbutton = new Button(owner, "dead_end_button", this, new Vector2(5f, 25f), 240f, "Dead End: " + Data.deadEnd);
		badWarpButton = new Button(owner, "bad_warp_button", this, new Vector2(5f, 5f), 240f, "Bad Warp: " + Data.badWarp);
		subNodes.Add(deadEndbutton);
		subNodes.Add(badWarpButton);
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		switch (sender.IDstring)
		{
		case "dead_end_button":
			Data.deadEnd = !Data.deadEnd;
			break;
		case "bad_warp_button":
			Data.badWarp = !Data.badWarp;
			break;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		deadEndbutton.Text = $"Dead End: {Data.deadEnd}";
		badWarpButton.Text = $"Bad Warp: {Data.badWarp}";
	}
}
