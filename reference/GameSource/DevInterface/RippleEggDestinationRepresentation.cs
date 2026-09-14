using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class RippleEggDestinationRepresentation : ResizeableObjectRepresentation, IDevUISignals
{
	public int lineSprite;

	public Button alwaysButton;

	public Button selectButton;

	public Panel selectPanel;

	public PlacedObject.RippleEggDestinationData Data => pObj.data as PlacedObject.RippleEggDestinationData;

	public Panel panel => subNodes[subNodes.Count - 1] as Panel;

	public string AlwaysButtonText
	{
		get
		{
			if (!Data.always)
			{
				return "conditional";
			}
			return "always";
		}
	}

	public string SelectButtonText => Data.destRoom ?? "no dest";

	public RippleEggDestinationRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "RippleEggDestination", showRing: true)
	{
		subNodes.Add(new Panel(owner, "RippleEggDestination_Panel", this, new Vector2(0f, 100f), new Vector2(198f, 26f), "Ripple Egg Destination"));
		panel.pos = Data.panelPos;
		alwaysButton = new Button(owner, "always", panel, new Vector2(95f, 5f), 80f, AlwaysButtonText);
		panel.subNodes.Add(alwaysButton);
		selectButton = new Button(owner, "select", panel, new Vector2(5f, 5f), 80f, SelectButtonText);
		panel.subNodes.Add(selectButton);
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = panel.pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, panel.absPos);
		Data.panelPos = panel.pos;
		alwaysButton.Text = AlwaysButtonText;
		selectButton.Text = SelectButtonText;
	}

	public List<string> GetWarpRoomsInRegion()
	{
		List<string> list = new List<string>();
		if (owner.room.game.rainWorld.regionWarpRooms.TryGetValue(owner.room.world.region.name.ToLowerInvariant(), out var value))
		{
			list.AddRange(value);
		}
		if (owner.room.game.rainWorld.regionSpinningTopRooms.TryGetValue(owner.room.world.region.name.ToLowerInvariant(), out var value2))
		{
			list.AddRange(value2);
		}
		return list;
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (sender == alwaysButton)
		{
			Data.always = !Data.always;
			Refresh();
		}
		else if (sender == selectButton)
		{
			if (selectPanel == null)
			{
				List<string> warpRoomsInRegion = GetWarpRoomsInRegion();
				selectPanel = new Panel(owner, "selectPanel", this, new Vector2(20f, 20f), new Vector2(100f, 10f + 20f * Mathf.Max(1f, warpRoomsInRegion.Count)), "dest");
				subNodes.Add(selectPanel);
				if (warpRoomsInRegion.Count == 0)
				{
					selectPanel.subNodes.Add(new DevUILabel(owner, "selectlabel", selectPanel, new Vector2(5f, 5f), 90f, "no warps in region"));
					return;
				}
				for (int i = 0; i < warpRoomsInRegion.Count; i++)
				{
					string text = warpRoomsInRegion[i].Split(':')[0].ToLower();
					selectPanel.subNodes.Add(new Button(owner, "DEST_" + text, selectPanel, new Vector2(5f, 5f + 20f * (float)i), 90f, text));
				}
			}
			else
			{
				subNodes.Remove(selectPanel);
				selectPanel.ClearSprites();
				selectPanel = null;
			}
		}
		else if (sender.IDstring.StartsWith("DEST_"))
		{
			if (selectPanel != null)
			{
				subNodes.Remove(selectPanel);
				selectPanel.ClearSprites();
				selectPanel = null;
			}
			Data.destRoom = sender.IDstring.Substring("DEST_".Length);
			Refresh();
		}
	}
}
