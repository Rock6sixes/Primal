using RWCustom;
using UnityEngine;

namespace DevInterface;

public class CustomRotRepresentation : ResizeableObjectRepresentation
{
	public class ControlPanel : Panel
	{
		public class CustomRotSlider : Slider
		{
			public DaddyCorruption.CustomRotData Data => (parentNode.parentNode as CustomRotRepresentation).Data;

			public CustomRotSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "Density_Slider":
					num = Data.density / 3f;
					break;
				case "Max_Size_Slider":
					num = Data.maxSize / 30f;
					break;
				case "Min_Size_Slider":
					num = Data.minSize / 30f;
					break;
				case "Eye_Chance_Slider":
					num = Data.eyeChance;
					break;
				case "Max_Eye_Size_Slider":
					num = Data.maxEyeSize;
					break;
				case "Min_Eye_Size_Slider":
					num = Data.minEyeSize;
					break;
				case "Max_Color_Slider":
					num = Data.maxColor;
					break;
				case "Min_Color_Slider":
					num = Data.minColor;
					break;
				case "Slowdown_Slider":
					num = Data.slowdown;
					break;
				case "Leg_Chance_Slider":
					num = Data.legChance;
					break;
				case "Darkness_Chance_Slider":
					num = Data.darknessChance;
					break;
				case "Darkness_Scale_Slider":
					num = Data.darknessScale / 2f;
					break;
				}
				base.NumberText = ((int)(num * 100f)).ToString();
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Density_Slider":
					Data.density = nubPos * 3f;
					break;
				case "Max_Size_Slider":
					Data.maxSize = nubPos * 30f;
					break;
				case "Min_Size_Slider":
					Data.minSize = nubPos * 30f;
					break;
				case "Eye_Chance_Slider":
					Data.eyeChance = nubPos;
					break;
				case "Max_Eye_Size_Slider":
					Data.maxEyeSize = nubPos;
					break;
				case "Min_Eye_Size_Slider":
					Data.minEyeSize = nubPos;
					break;
				case "Max_Color_Slider":
					Data.maxColor = nubPos;
					break;
				case "Min_Color_Slider":
					Data.minColor = nubPos;
					break;
				case "Slowdown_Slider":
					Data.slowdown = nubPos;
					break;
				case "Leg_Chance_Slider":
					Data.legChance = nubPos;
					break;
				case "Darkness_Chance_Slider":
					Data.darknessChance = nubPos;
					break;
				case "Darkness_Scale_Slider":
					Data.darknessScale = nubPos * 2f;
					break;
				}
				Refresh();
			}
		}

		public DaddyCorruption.CustomRotData Data => (parentNode as CustomRotRepresentation).Data;

		public ControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 305f), "Custom Rot")
		{
			float num = 5f;
			subNodes.Add(new CustomRotSlider(owner, "Density_Slider", this, new Vector2(5f, num), "Density: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Max_Size_Slider", this, new Vector2(5f, num), "Max Size: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Min_Size_Slider", this, new Vector2(5f, num), "Min Size: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Eye_Chance_Slider", this, new Vector2(5f, num), "Eye Chance: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Max_Eye_Size_Slider", this, new Vector2(5f, num), "Max Eye Size: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Min_Eye_Size_Slider", this, new Vector2(5f, num), "Min Eye Size: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Max_Color_Slider", this, new Vector2(5f, num), "Max Color: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Min_Color_Slider", this, new Vector2(5f, num), "Min Color: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Slowdown_Slider", this, new Vector2(5f, num), "Slowdown: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Leg_Chance_Slider", this, new Vector2(5f, num), "Legs: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Darkness_Chance_Slider", this, new Vector2(5f, num), "Goo Chance: "));
			num += 20f;
			subNodes.Add(new CustomRotSlider(owner, "Darkness_Scale_Slider", this, new Vector2(5f, num), "Goo Scale: "));
			num += 20f;
			subNodes.Add(new EnumCycler<DaddyCorruption.CustomRotData.RotColor>(owner, "Color_Cycler", this, new Vector2(5f, num), 240f, "Color: ", () => Data.color, delegate(DaddyCorruption.CustomRotData.RotColor val)
			{
				Data.color = val;
			}));
			num += 20f;
			subNodes.Add(new EnumCycler<DaddyCorruption.CustomRotData.EyeType>(owner, "Eye_Cycler", this, new Vector2(5f, num), 240f, "Eyes: ", () => Data.eyeType, delegate(DaddyCorruption.CustomRotData.EyeType val)
			{
				Data.eyeType = val;
			}));
			num += 20f;
			subNodes.Add(new EnumCycler<DaddyCorruption.CustomRotData.Behavior>(owner, "Behavior_Cycler", this, new Vector2(5f, num), 240f, "Behav: ", () => Data.behavior, delegate(DaddyCorruption.CustomRotData.Behavior val)
			{
				Data.behavior = val;
			}));
			num += 20f;
		}
	}

	public ControlPanel controlPanel;

	public int lineSprite;

	public DaddyCorruption.CustomRotData Data => pObj.data as DaddyCorruption.CustomRotData;

	public CustomRotRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name, showRing: true)
	{
		controlPanel = new ControlPanel(owner, "Custom_Rot_Panel", this, new Vector2(5f, 5f));
		controlPanel.pos = Data.panelPos;
		subNodes.Add(controlPanel);
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		Data.panelPos = controlPanel.pos;
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = controlPanel.pos.magnitude;
		fSprites[lineSprite].rotation = Custom.VecToDeg(controlPanel.pos);
	}
}
