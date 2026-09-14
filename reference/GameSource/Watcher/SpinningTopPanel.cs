using System.Collections.Generic;
using System.IO;
using DevInterface;
using UnityEngine;

namespace Watcher;

public class SpinningTopPanel : Panel, IDevUISignals
{
	public class SpawnIdentifierController : IntegerControl
	{
		public SpawnIdentifierController(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title)
		{
		}

		public override void Refresh()
		{
			base.NumberLabelText = (parentNode as SpinningTopPanel).Data.spawnIdentifier.ToString();
			base.Refresh();
		}

		public override void Increment(int change)
		{
			(parentNode as SpinningTopPanel).Data.spawnIdentifier = Mathf.Max(0, (parentNode as SpinningTopPanel).Data.spawnIdentifier + change);
			Refresh();
		}
	}

	public CustomDecalRepresentation.SelectDecalPanel regionSelectPanel;

	public CustomDecalRepresentation.SelectDecalPanel roomSelectPanel;

	public CustomDecalRepresentation.SelectDecalPanel timelineSelectPanel;

	public RoomPositionPanel roomPositionPanel;

	public Button rippleWarpButton;

	public SpinningTopData Data => (parentNode as SpinningTopSpotRepresentation).pObj.data as SpinningTopData;

	public SpinningTopPanel(DevUI owner, DevUINode parentNode, Vector2 pos)
		: base(owner, "SpinningTop_Panel", parentNode, pos, new Vector2(250f, 125f), "Spinning Top Spot")
	{
		subNodes.Add(new Button(owner, "Select_Timeline_Panel_Button", this, new Vector2(5f, 105f), 240f, "Timeline : "));
		subNodes.Add(new Button(owner, "Select_Region_Panel_Button", this, new Vector2(5f, 85f), 240f, "Region : "));
		subNodes.Add(new Button(owner, "Select_Room_Panel_Button", this, new Vector2(5f, 65f), 240f, "Room : "));
		subNodes.Add(new Button(owner, "Select_Position_Panel_Button", this, new Vector2(5f, 45f), 240f, "Position : "));
		subNodes.Add(new SpawnIdentifierController(owner, "SpawnIdentifier", this, new Vector2(5f, 25f), "Spawn ID : "));
		rippleWarpButton = new Button(owner, "ripple_warp_button", this, new Vector2(5f, 5f), 240f, "Ripple Warp: " + Data.rippleWarp);
		subNodes.Add(rippleWarpButton);
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

	public override void Refresh()
	{
		base.Refresh();
		rippleWarpButton.Text = $"Ripple Warp: {Data.rippleWarp}";
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender.IDstring == "ripple_warp_button")
		{
			Data.rippleWarp = !Data.rippleWarp;
		}
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
			roomPositionPanel = new RoomPositionPanel(owner, this, new Vector2(200f, 15f) - absPos, Data.destTimeline, Data.destRoom);
			subNodes.Add(roomPositionPanel);
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
}
