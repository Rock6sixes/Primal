using System.Collections.Generic;
using System.IO;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class WarpPointToRoomRepresentation : PlacedObjectRepresentation
{
	public class WarpPointToRoomControlPanel : Panel, IDevUISignals
	{
		public CustomDecalRepresentation.SelectDecalPanel regionSelectPanel;

		public CustomDecalRepresentation.SelectDecalPanel roomSelectPanel;

		public CustomDecalRepresentation.SelectDecalPanel timelineSelectPanel;

		public Panel modifiersPanel;

		public RoomPositionPanel roomPositionPanel;

		public EffectSettingsPanel effectSettingsPanel;

		public WarpPoint.WarpPointData Data => (parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData;

		public WarpPointToRoomControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 125f), "Warp Point")
		{
			subNodes.Add(new Button(owner, "Effect_Settings_Button", this, new Vector2(5f, 105f), 240f, "Effect Settings"));
			subNodes.Add(new Button(owner, "Select_Timeline_Panel_Button", this, new Vector2(5f, 85f), 240f, "Timeline : "));
			subNodes.Add(new Button(owner, "Select_Region_Panel_Button", this, new Vector2(5f, 65f), 240f, "Region : "));
			subNodes.Add(new Button(owner, "Select_Room_Panel_Button", this, new Vector2(5f, 45f), 240f, "Room : "));
			subNodes.Add(new Button(owner, "Select_Position_Panel_Button", this, new Vector2(5f, 25f), 240f, "Position : "));
			subNodes.Add(new Button(owner, "Select_Modifiers_Button", this, new Vector2(5f, 5f), 240f, "Warp Modifiers"));
			RefreshLabels();
		}

		public override void Update()
		{
			base.Update();
			if (roomPositionPanel != null && roomPositionPanel.tileClicked)
			{
				Data.destPos = new Vector2((float)roomPositionPanel.tileClickedX * 20f + 10f, (float)roomPositionPanel.tileClickedY * 20f + 10f);
				HideRoomPositionPanel();
				RefreshLabels();
			}
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (sender.IDstring == "Select_Timeline_Panel_Button")
			{
				if (timelineSelectPanel != null)
				{
					HideTimelineSelectPanel();
					return;
				}
				HideRegionSelectPanel();
				HideRoomSelectPanel();
				HideRoomPositionPanel();
				HideEffectSettingsPanel();
				HideBadWarpPanel();
				string[] decalNames = ExtEnum<SlugcatStats.Timeline>.values.entries.ToArray();
				timelineSelectPanel = new CustomDecalRepresentation.SelectDecalPanel(owner, this, new Vector2(200f, 15f) - absPos, decalNames);
				subNodes.Add(timelineSelectPanel);
			}
			else if (sender.IDstring == "Select_Region_Panel_Button")
			{
				if (regionSelectPanel != null || Data.destTimeline == null)
				{
					HideRegionSelectPanel();
					return;
				}
				HideTimelineSelectPanel();
				HideRoomSelectPanel();
				HideRoomPositionPanel();
				HideEffectSettingsPanel();
				HideBadWarpPanel();
				string[] decalNames2 = Region.GetFullRegionOrder(Data.destTimeline).ToArray();
				regionSelectPanel = new CustomDecalRepresentation.SelectDecalPanel(owner, this, new Vector2(200f, 15f) - absPos, decalNames2);
				subNodes.Add(regionSelectPanel);
			}
			else if (sender.IDstring == "Select_Room_Panel_Button")
			{
				if (roomSelectPanel != null || Data.RegionString == null)
				{
					HideRoomSelectPanel();
					return;
				}
				HideRegionSelectPanel();
				HideTimelineSelectPanel();
				HideRoomPositionPanel();
				HideEffectSettingsPanel();
				HideBadWarpPanel();
				string regionString = Data.RegionString;
				if (regionString == null)
				{
					return;
				}
				List<string> list = new List<string>();
				string[] array = AssetManager.ListDirectory("world/" + regionString + "-rooms");
				for (int i = 0; i < array.Length; i++)
				{
					string text = Path.GetFileName(array[i]).ToLowerInvariant();
					if (text.EndsWith(".txt") && !text.Contains("_settings"))
					{
						list.Add(Path.GetFileNameWithoutExtension(array[i]));
					}
				}
				roomSelectPanel = new CustomDecalRepresentation.SelectDecalPanel(owner, this, new Vector2(200f, 15f) - absPos, list.ToArray());
				subNodes.Add(roomSelectPanel);
			}
			else if (sender.IDstring == "Select_Position_Panel_Button")
			{
				if (roomPositionPanel != null || Data.destTimeline == null || Data.destRoom == null)
				{
					HideRoomPositionPanel();
					return;
				}
				HideTimelineSelectPanel();
				HideRoomSelectPanel();
				HideRegionSelectPanel();
				HideEffectSettingsPanel();
				HideBadWarpPanel();
				roomPositionPanel = new RoomPositionPanel(owner, this, new Vector2(200f, 15f) - absPos, Data.destTimeline, Data.destRoom);
				subNodes.Add(roomPositionPanel);
			}
			else if (sender.IDstring == "Effect_Settings_Button")
			{
				if (effectSettingsPanel != null)
				{
					HideEffectSettingsPanel();
					return;
				}
				HideTimelineSelectPanel();
				HideRoomSelectPanel();
				HideRegionSelectPanel();
				HideRoomPositionPanel();
				HideBadWarpPanel();
				effectSettingsPanel = new EffectSettingsPanel(owner, this, new Vector2(200f, 15f) - absPos);
				subNodes.Add(effectSettingsPanel);
			}
			else if (sender.IDstring == "Select_Modifiers_Button")
			{
				if (modifiersPanel != null)
				{
					HideBadWarpPanel();
					return;
				}
				HideTimelineSelectPanel();
				HideRoomSelectPanel();
				HideRegionSelectPanel();
				HideRoomPositionPanel();
				HideEffectSettingsPanel();
				modifiersPanel = new WarpModifiersPanel(owner, this, new Vector2(200f, 15f));
				subNodes.Add(modifiersPanel);
			}
			else if (sender.IDstring == "BackPage99289..?/~")
			{
				if (sender.parentNode == regionSelectPanel)
				{
					regionSelectPanel.PrevPage();
				}
				else if (sender.parentNode == roomSelectPanel)
				{
					roomSelectPanel.PrevPage();
				}
				else if (sender.parentNode == timelineSelectPanel)
				{
					timelineSelectPanel.PrevPage();
				}
			}
			else if (sender.IDstring == "NextPage99289..?/~")
			{
				if (sender.parentNode == regionSelectPanel)
				{
					regionSelectPanel.NextPage();
				}
				else if (sender.parentNode == roomSelectPanel)
				{
					roomSelectPanel.NextPage();
				}
				else if (sender.parentNode == timelineSelectPanel)
				{
					timelineSelectPanel.NextPage();
				}
			}
			else
			{
				if (sender.parentNode == regionSelectPanel)
				{
					Data.RegionString = sender.IDstring;
					Data.destRoom = null;
					Data.destPos = null;
					HideRegionSelectPanel();
				}
				else if (sender.parentNode == roomSelectPanel)
				{
					Data.destRoom = sender.IDstring;
					Data.destPos = null;
					HideRoomSelectPanel();
				}
				else if (sender.parentNode == timelineSelectPanel)
				{
					Data.destTimeline = new SlugcatStats.Timeline(sender.IDstring);
					Data.RegionString = null;
					Data.destRoom = null;
					Data.destPos = null;
					HideTimelineSelectPanel();
				}
				RefreshLabels();
			}
		}

		public void RefreshLabels()
		{
			string text = Data.RegionString;
			if (text == null)
			{
				text = "UNDEFINED";
			}
			string text2 = Data.destRoom;
			if (text2 == null)
			{
				text2 = "UNDEFINED";
			}
			string text3 = ((Data.destTimeline == null) ? "UNDEFINED" : Data.destTimeline.ToString());
			string text4 = (Data.destPos.HasValue ? ("(" + Data.destPos.Value.x + ", " + Data.destPos.Value.y + ")") : "UNDEFINED");
			for (int i = 0; i < subNodes.Count; i++)
			{
				if (subNodes[i].IDstring == "Select_Region_Panel_Button")
				{
					(subNodes[i] as Button).Text = "Region : " + text;
				}
				else if (subNodes[i].IDstring == "Select_Room_Panel_Button")
				{
					(subNodes[i] as Button).Text = "Room : " + text2;
				}
				else if (subNodes[i].IDstring == "Select_Timeline_Panel_Button")
				{
					(subNodes[i] as Button).Text = "Timeline : " + text3;
				}
				else if (subNodes[i].IDstring == "Select_Position_Panel_Button")
				{
					(subNodes[i] as Button).Text = "Position : " + text4;
				}
			}
		}

		public void HideRegionSelectPanel()
		{
			if (regionSelectPanel != null)
			{
				subNodes.Remove(regionSelectPanel);
				regionSelectPanel.ClearSprites();
				regionSelectPanel = null;
			}
		}

		public void HideRoomSelectPanel()
		{
			if (roomSelectPanel != null)
			{
				subNodes.Remove(roomSelectPanel);
				roomSelectPanel.ClearSprites();
				roomSelectPanel = null;
			}
		}

		public void HideTimelineSelectPanel()
		{
			if (timelineSelectPanel != null)
			{
				subNodes.Remove(timelineSelectPanel);
				timelineSelectPanel.ClearSprites();
				timelineSelectPanel = null;
			}
		}

		public void HideRoomPositionPanel()
		{
			if (roomPositionPanel != null)
			{
				subNodes.Remove(roomPositionPanel);
				roomPositionPanel.ClearSprites();
				roomPositionPanel = null;
			}
		}

		public void HideEffectSettingsPanel()
		{
			if (effectSettingsPanel != null)
			{
				subNodes.Remove(effectSettingsPanel);
				effectSettingsPanel.ClearSprites();
				effectSettingsPanel = null;
			}
		}

		public void HideBadWarpPanel()
		{
			if (modifiersPanel != null)
			{
				subNodes.Remove(modifiersPanel);
				modifiersPanel.ClearSprites();
				modifiersPanel = null;
			}
		}
	}

	public class EffectSettingsPanel : Panel
	{
		public class WarpPointControlSlider : Slider
		{
			public WarpPointControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				int num = 0;
				WarpPoint.WarpPointData.EffectSettings effectSettings = ((parentNode.parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).effectSettings;
				switch (IDstring)
				{
				case "vignette":
					num = effectSettings.vignette;
					base.NumberText = num.ToString();
					break;
				case "darkness":
					num = effectSettings.darkness;
					base.NumberText = num.ToString();
					break;
				case "spiral_gap":
					num = effectSettings.spiralGap;
					base.NumberText = num.ToString();
					break;
				case "swirl_intensity":
					num = effectSettings.swirlIntensity;
					base.NumberText = num.ToString();
					break;
				case "lensing_intensity":
					num = effectSettings.lensingIntensity;
					base.NumberText = num.ToString();
					break;
				case "noise_intensity":
					num = effectSettings.noiseIntensity;
					base.NumberText = num.ToString();
					break;
				case "spiral_twist":
					num = effectSettings.spiralTwist;
					base.NumberText = num.ToString();
					break;
				case "agitation_speed":
					num = effectSettings.agitationSpeed;
					base.NumberText = num.ToString();
					break;
				case "spaghettification":
					num = effectSettings.spaghettification;
					base.NumberText = num.ToString();
					break;
				case "trigger_duration":
					num = effectSettings.triggerDuration;
					base.NumberText = (num + 50).ToString();
					RefreshNubPos((float)num / 400f);
					return;
				case "active_duration":
					num = effectSettings.activeDuration;
					base.NumberText = (num + 50).ToString();
					RefreshNubPos((float)num / 400f);
					return;
				}
				RefreshNubPos((float)num / 15f);
			}

			public override void NubDragged(float nubPos)
			{
				WarpPoint.WarpPointData.EffectSettings effectSettings = ((parentNode.parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).effectSettings;
				switch (IDstring)
				{
				case "vignette":
					nubPos *= 15f;
					effectSettings.vignette = (int)Mathf.Ceil(nubPos);
					break;
				case "darkness":
					nubPos *= 15f;
					effectSettings.darkness = (int)Mathf.Ceil(nubPos);
					break;
				case "spiral_gap":
					nubPos *= 15f;
					effectSettings.spiralGap = (int)Mathf.Ceil(nubPos);
					break;
				case "swirl_intensity":
					nubPos *= 15f;
					effectSettings.swirlIntensity = (int)Mathf.Ceil(nubPos);
					break;
				case "lensing_intensity":
					nubPos *= 15f;
					effectSettings.lensingIntensity = (int)Mathf.Ceil(nubPos);
					break;
				case "noise_intensity":
					nubPos *= 15f;
					effectSettings.noiseIntensity = (int)Mathf.Ceil(nubPos);
					break;
				case "spiral_twist":
					nubPos *= 15f;
					effectSettings.spiralTwist = (int)Mathf.Ceil(nubPos);
					break;
				case "agitation_speed":
					nubPos *= 15f;
					effectSettings.agitationSpeed = (int)Mathf.Ceil(nubPos);
					break;
				case "spaghettification":
					nubPos *= 15f;
					effectSettings.spaghettification = (int)Mathf.Ceil(nubPos);
					break;
				case "trigger_duration":
					nubPos *= 400f;
					effectSettings.triggerDuration = (int)Mathf.Ceil(nubPos);
					break;
				case "active_duration":
					nubPos *= 400f;
					effectSettings.activeDuration = (int)Mathf.Ceil(nubPos);
					break;
				}
				((parentNode.parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).effectSettings = effectSettings;
				parentNode.parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public EffectSettingsPanel(DevUI owner, DevUINode parentNode, Vector2 pos)
			: base(owner, "Effect_Setting_Panel", parentNode, pos, new Vector2(250f, 285f), "Warp Point effect settings")
		{
			size.y = -15f;
			subNodes.Add(new WarpPointControlSlider(owner, "spaghettification", this, new Vector2(5f, base.panelStep), "Spaghettification : "));
			subNodes.Add(new WarpPointControlSlider(owner, "agitation_speed", this, new Vector2(5f, base.panelStep), "Agitation speed : "));
			subNodes.Add(new WarpPointControlSlider(owner, "spiral_twist", this, new Vector2(5f, base.panelStep), "Spiral twist : "));
			subNodes.Add(new WarpPointControlSlider(owner, "noise_intensity", this, new Vector2(5f, base.panelStep), "Noise intensity : "));
			subNodes.Add(new WarpPointControlSlider(owner, "lensing_intensity", this, new Vector2(5f, base.panelStep), "Lensing intensity : "));
			subNodes.Add(new WarpPointControlSlider(owner, "swirl_intensity", this, new Vector2(5f, base.panelStep), "Swirl intensity : "));
			subNodes.Add(new WarpPointControlSlider(owner, "spiral_gap", this, new Vector2(5f, base.panelStep), "Spiral gap : "));
			subNodes.Add(new WarpPointControlSlider(owner, "darkness", this, new Vector2(5f, base.panelStep), "Darkness : "));
			subNodes.Add(new WarpPointControlSlider(owner, "vignette", this, new Vector2(5f, base.panelStep), "Vignette : "));
			subNodes.Add(new WarpPointControlSlider(owner, "active_duration", this, new Vector2(5f, base.panelStep), "Active duration : "));
			subNodes.Add(new WarpPointControlSlider(owner, "trigger_duration", this, new Vector2(5f, base.panelStep), "Trigger duration : "));
			size.y += 20f;
		}
	}

	public class WarpModifiersPanel : Panel, IDevUISignals
	{
		public class WarpPointModifierSlider : Slider
		{
			public WarpPointModifierSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				int num = 0;
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "use_amount")
				{
					num = ((parentNode.parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).uses;
					if (num == 0)
					{
						base.NumberText = "Unlimited";
					}
					else
					{
						base.NumberText = num.ToString();
					}
				}
				RefreshNubPos((float)num / 15f);
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "use_amount")
				{
					nubPos *= 15f;
					((parentNode.parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).limitedUse = (int)Mathf.Ceil(nubPos) != 0;
					((parentNode.parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).uses = (int)Mathf.Ceil(nubPos);
				}
				parentNode.parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public Button badWarpButton;

		public Button rippleWarpButton;

		public WarpPointModifierSlider useSlider;

		public WarpModifiersPanel(DevUI owner, DevUINode parentNode, Vector2 pos)
			: base(owner, "Select_Modifiers_Button", parentNode, pos, new Vector2(250f, 85f), "Warp Modifiers Settings")
		{
			float num = 85f;
			badWarpButton = new Button(owner, "One_Way_Button", this, new Vector2(5f, num -= 20f), 240f, "One Way: " + ((parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).oneWay);
			rippleWarpButton = new Button(owner, "Ripple_Warp_Button", this, new Vector2(5f, num -= 20f), 240f, "Ripple Warp: " + ((parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).rippleWarp);
			useSlider = new WarpPointModifierSlider(owner, "use_amount", this, new Vector2(5f, num -= 20f), "Uses : ");
			subNodes.Add(badWarpButton);
			subNodes.Add(rippleWarpButton);
			subNodes.Add(useSlider);
		}

		public override void Refresh()
		{
			base.Refresh();
			badWarpButton.Text = "One Way: " + ((parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).oneWay;
			rippleWarpButton.Text = "Ripple Warp: " + ((parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).rippleWarp;
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			switch (sender.IDstring)
			{
			case "One_Way_Button":
				((parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).oneWay = !((parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).oneWay;
				break;
			case "Ripple_Warp_Button":
				((parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).rippleWarp = !((parentNode.parentNode as WarpPointToRoomRepresentation).pObj.data as WarpPoint.WarpPointData).rippleWarp;
				break;
			}
		}
	}

	public WarpPointToRoomControlPanel controlPanel;

	public WarpPointToRoomRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, pObj.type.ToString())
	{
		controlPanel = new WarpPointToRoomControlPanel(owner, "WarpPointToRoom_Panel", this, new Vector2(0f, 100f));
		subNodes.Add(controlPanel);
		controlPanel.pos = (pObj.data as WarpPoint.WarpPointData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites[fSprites.Count - 1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(fSprites.Count - 1, absPos);
		fSprites[fSprites.Count - 1].scaleY = controlPanel.pos.magnitude;
		fSprites[fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
		(pObj.data as WarpPoint.WarpPointData).panelPos = (subNodes[subNodes.Count - 1] as Panel).pos;
	}
}
