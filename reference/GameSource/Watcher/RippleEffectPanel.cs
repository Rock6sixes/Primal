using System.Collections.Generic;
using System.IO;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleEffectPanel : Panel, IDevUISignals
{
	public class RippleEffectSlider : Slider
	{
		public RippleEffectSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
		{
		}

		public override void NubDragged(float nubPos)
		{
			string iDstring = IDstring;
			if (iDstring == null)
			{
				return;
			}
			RippleEffectSettings effect = (parentNode as RippleEffectPanel).effect;
			RoomCamera roomCamera = (Custom.rainWorld.processManager.currentMainLoop as RainWorldGame).cameras[0];
			switch (iDstring)
			{
			case "TrailAmount":
				effect.trailAmount = nubPos;
				if (roomCamera.rippleData != null)
				{
					roomCamera.rippleData.TrailAmount = nubPos;
				}
				break;
			case "PaletteAmount":
				effect.paletteAmount = nubPos;
				if (roomCamera.rippleData != null)
				{
					roomCamera.rippleData.paletteAmount = nubPos;
				}
				Shader.SetGlobalFloat("_RipplePaletteAmount", nubPos);
				break;
			}
			parentNode.Refresh();
			Refresh();
		}

		public override void Refresh()
		{
			base.Refresh();
			float num = 0f;
			_ = IDstring;
			RippleEffectSettings effect = (parentNode as RippleEffectPanel).effect;
			switch (IDstring)
			{
			case "TrailAmount":
				num = effect.trailAmount;
				break;
			case "PaletteAmount":
				num = effect.paletteAmount;
				break;
			}
			base.NumberText = (int)(num * 100f) + " ";
			RefreshNubPos(num);
		}
	}

	public class ScreenRemapController : IntegerControl
	{
		public ScreenRemapController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title)
		{
			(subNodes.Find((DevUINode x) => x.IDstring == "Title") as DevUILabel).fSprites.GetLastObject().scaleX = 38f;
			(subNodes.Find((DevUINode x) => x.IDstring == "Number") as DevUILabel).pos.x -= 80f;
			(subNodes.Find((DevUINode x) => x.IDstring == "Less") as ArrowButton).pos.x -= 80f;
			(subNodes.Find((DevUINode x) => x.IDstring == "More") as ArrowButton).pos.x -= 80f;
		}

		public override void Refresh()
		{
			RippleEffectSettings effect = (parentNode as RippleEffectPanel).effect;
			RippleEffectPanel rippleEffectPanel = parentNode as RippleEffectPanel;
			if (IDstring == "RippleScreen")
			{
				base.NumberLabelText = effect.screenMap[rippleEffectPanel.screenToEdit].ToString();
			}
			if (IDstring == "RealScreen")
			{
				base.NumberLabelText = (rippleEffectPanel.screenToEdit + 1).ToString();
				(rippleEffectPanel.subNodes.Find((DevUINode x) => x.IDstring == "RippleScreen") as ScreenRemapController).NumberLabelText = effect.screenMap[rippleEffectPanel.screenToEdit].ToString();
			}
			if (IDstring == "Palette")
			{
				base.NumberLabelText = effect.palette.ToString();
			}
			base.Refresh();
		}

		public override void Increment(int change)
		{
			RippleEffectSettings effect = (parentNode as RippleEffectPanel).effect;
			RippleEffectPanel rippleEffectPanel = parentNode as RippleEffectPanel;
			if (IDstring == "RippleScreen")
			{
				effect.screenMap[rippleEffectPanel.screenToEdit] = (effect.rippleScreens + 1 + effect.screenMap[rippleEffectPanel.screenToEdit] + change) % (effect.rippleScreens + 1);
			}
			if (IDstring == "RealScreen")
			{
				rippleEffectPanel.screenToEdit = (effect.screens + rippleEffectPanel.screenToEdit + change) % effect.screens;
			}
			if (IDstring == "Palette")
			{
				effect.palette = Mathf.Max(0, effect.palette + change);
				if (rippleEffectPanel.owner.room.game.cameras[0].rippleData != null)
				{
					rippleEffectPanel.owner.room.game.cameras[0].rippleData.paletteToLoad = effect.palette;
					rippleEffectPanel.owner.room.game.cameras[0].rippleData.LoadPalette();
					rippleEffectPanel.owner.room.game.cameras[0].UpdateRippleData(rippleEffectPanel.owner.room, rippleEffectPanel.owner.room.game.cameras[0].currentCameraPosition);
				}
			}
			Refresh();
		}
	}

	public CustomDecalRepresentation.SelectDecalPanel regionSelectPanel;

	public CustomDecalRepresentation.SelectDecalPanel roomSelectPanel;

	public CustomDecalRepresentation.SelectDecalPanel timelineSelectPanel;

	public RippleEffectSettings effect;

	public int screenToEdit;

	public RippleEffectPanel(DevUI owner, DevUINode parentNode, Vector2 pos, RippleEffectSettings effect)
		: base(owner, "Effect_Panel_" + effect.type.ToString(), parentNode, pos, new Vector2(250f, 65f), "")
	{
		this.effect = effect;
		Move(effect.panelPosition);
		size.y = -15f;
		base.pos = pos;
		subNodes.Add(new RippleEffectSlider(owner, "TrailAmount", this, new Vector2(5f, base.panelStep), "Trail Amount: "));
		subNodes.Add(new RippleEffectSlider(owner, "PaletteAmount", this, new Vector2(5f, base.panelStep), "Palette Amount: "));
		subNodes.Add(new ScreenRemapController(owner, "RealScreen", this, new Vector2(5f, base.panelStep), "Screen: "));
		subNodes.Add(new ScreenRemapController(owner, "RippleScreen", this, new Vector2(127f, size.y), "Map to: "));
		subNodes.Add(new Button(owner, "Select_Room_Panel_Button", this, new Vector2(5f, base.panelStep), 118f, "Room : "));
		subNodes.Add(new ScreenRemapController(owner, "Palette", this, new Vector2(127f, size.y), "Pal: "));
		subNodes.Add(new Button(owner, "Select_Timeline_Panel_Button", this, new Vector2(5f, base.panelStep), 118f, "Timeline : "));
		subNodes.Add(new Button(owner, "Select_Region_Panel_Button", this, new Vector2(127f, size.y), 118f, "Region : "));
		size.y += 20f;
		RefreshLabels();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Move(Vector2 newPos)
	{
		base.Move(newPos);
		effect.panelPosition = absPos;
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
			string[] decalNames = ExtEnum<SlugcatStats.Timeline>.values.entries.ToArray();
			timelineSelectPanel = new CustomDecalRepresentation.SelectDecalPanel(owner, this, new Vector2(200f, 15f) - absPos, decalNames);
			subNodes.Add(timelineSelectPanel);
		}
		else if (sender.IDstring == "Select_Region_Panel_Button")
		{
			if (regionSelectPanel != null || effect.destTimeline == null)
			{
				HideRegionSelectPanel();
				return;
			}
			HideTimelineSelectPanel();
			HideRoomSelectPanel();
			string[] decalNames2 = Region.GetFullRegionOrder(effect.destTimeline).ToArray();
			regionSelectPanel = new CustomDecalRepresentation.SelectDecalPanel(owner, this, new Vector2(200f, 15f) - absPos, decalNames2);
			subNodes.Add(regionSelectPanel);
		}
		else if (sender.IDstring == "Select_Room_Panel_Button")
		{
			if (roomSelectPanel != null || effect.destRegion == null)
			{
				HideRoomSelectPanel();
				return;
			}
			HideRegionSelectPanel();
			HideTimelineSelectPanel();
			string destRegion = effect.destRegion;
			if (destRegion == null)
			{
				return;
			}
			List<string> list = new List<string>();
			string[] array = AssetManager.ListDirectory("world/" + destRegion + "-rooms");
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
				effect.destRegion = sender.IDstring;
				effect.destRoom = null;
				HideRegionSelectPanel();
			}
			else if (sender.parentNode == roomSelectPanel)
			{
				effect.destRoom = sender.IDstring;
				HideRoomSelectPanel();
			}
			else if (sender.parentNode == timelineSelectPanel)
			{
				effect.destTimeline = new SlugcatStats.Timeline(sender.IDstring);
				effect.destRegion = null;
				effect.destRoom = null;
				HideTimelineSelectPanel();
			}
			RefreshLabels();
		}
	}

	public void RefreshLabels()
	{
		string text = effect.destRegion;
		if (text == null)
		{
			text = "UNDEFINED";
		}
		string text2 = effect.destRoom;
		if (text2 == null)
		{
			text2 = "UNDEFINED";
		}
		string text3 = ((effect.destTimeline == null) ? "UNDEFINED" : effect.destTimeline.ToString());
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

	public override void Refresh()
	{
		base.Refresh();
		base.Title = "EFFECT " + effect.type.ToString();
	}
}
