using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class WallLightRepresentation : PlacedObjectRepresentation
{
	public class WallLightPanel : Panel, IDevUISignals
	{
		public class WallLightSlider : Slider
		{
			public WallLightSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, float titleWidth = 110f)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, titleWidth)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null)
				{
					WallLight.WallLightData data = (parentNode.parentNode as WallLightRepresentation).data;
					switch (iDstring)
					{
					case "Intensity":
						data.value = nubPos;
						data.obj.value = nubPos;
						break;
					case "Saturation":
						data.saturation = nubPos;
						data.obj.saturation = nubPos;
						break;
					case "Hue":
						data.hue = nubPos;
						data.obj.hue = nubPos;
						break;
					}
					parentNode.parentNode.Refresh();
					Refresh();
				}
			}

			public override void Refresh()
			{
				base.Refresh();
				float nubPos = 0f;
				string iDstring = IDstring;
				WallLight.WallLightData data = (parentNode.parentNode as WallLightRepresentation).data;
				switch (iDstring)
				{
				case "Intensity":
					nubPos = data.value;
					base.NumberText = nubPos + " ";
					break;
				case "Saturation":
					nubPos = data.saturation;
					base.NumberText = nubPos + " ";
					break;
				case "Hue":
					nubPos = data.hue;
					base.NumberText = nubPos + " ";
					break;
				}
				RefreshNubPos(nubPos);
			}
		}

		public Button fadeButton;

		public Button chanceButton;

		public bool uiDirty;

		public WallLight.WallLightData data => (parentNode as WallLightRepresentation).data;

		public string ActiveDuringButtonName => "Active " + data.activeDuring.value;

		public string FlatButtonText => $"Flat: {data.flat}";

		public string HardShadowButtonText => data.shadowType.value;

		public WallLightPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Wall Light")
		{
			AddPanelElements();
		}

		public override void Update()
		{
			base.Update();
			if (uiDirty)
			{
				RefreshUI();
			}
		}

		public void RefreshUI()
		{
			for (int num = subNodes.Count - 1; num > -1; num--)
			{
				subNodes[num].ClearSprites();
				subNodes.Pop();
			}
			AddPanelElements();
			uiDirty = false;
		}

		public void AddPanelElements()
		{
			size.y = -15f;
			subNodes.Add(new Button(owner, "ActiveDuring", this, new Vector2(125f, base.panelStep), 115f, ActiveDuringButtonName));
			subNodes.Add(new Button(owner, "ShadowType", this, new Vector2(65f, size.y), 55f, HardShadowButtonText));
			subNodes.Add(new Button(owner, "Flat", this, new Vector2(5f, size.y), 55f, FlatButtonText));
			AddPaletteButtons();
			AddColorControls();
			size.y += 20f;
		}

		public void AddColorControls()
		{
			subNodes.Add(new WallLightSlider(owner, "Intensity", this, new Vector2(5f, base.panelStep), "Intensity: "));
			if (!data.usePalette)
			{
				subNodes.Add(new WallLightSlider(owner, "Saturation", this, new Vector2(5f, base.panelStep), "Saturation: "));
				subNodes.Add(new WallLightSlider(owner, "Hue", this, new Vector2(5f, base.panelStep), "Hue: "));
			}
		}

		public void AddPaletteButtons()
		{
			if (!data.usePalette)
			{
				subNodes.Add(new Button(owner, "UsePalette", this, new Vector2(5f, base.panelStep), 245f, "Using custom color"));
				return;
			}
			subNodes.Add(new Button(owner, "PaletteSelect", this, new Vector2(125f, base.panelStep), 115f, data.colorName.value ?? ""));
			subNodes.Add(new Button(owner, "UsePalette", this, new Vector2(5f, size.y), 115f, "Using palette color: "));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (sender.IDstring == "UsePalette")
			{
				data.usePalette = !data.usePalette;
				data.obj.usePalette = data.usePalette;
				uiDirty = true;
			}
			else if (sender.IDstring == "PaletteSelect")
			{
				data.colorName = new RoomPalette.ColorName(ExtEnum<RoomPalette.ColorName>.values.GetEntry((data.colorName.Index + 1) % ExtEnum<RoomPalette.ColorName>.values.Count));
				data.obj.colorName = data.colorName;
				data.obj.UpdatePaletteColor(owner.room.game.cameras[0].currentPalette);
				(sender as Button).Text = data.colorName.value ?? "";
			}
			else if (sender.IDstring == "ActiveDuring")
			{
				data.activeDuring = new WallLight.ActiveDuring(ExtEnum<WallLight.ActiveDuring>.values.GetEntry((data.activeDuring.Index + 1) % ExtEnum<WallLight.ActiveDuring>.values.Count));
				data.obj.activeDuring = data.activeDuring;
				(sender as Button).Text = ActiveDuringButtonName;
			}
			else if (sender.IDstring == "Flat")
			{
				data.flat = !data.flat;
				data.obj.flat = data.flat;
				(sender as Button).Text = FlatButtonText;
			}
			else if (sender.IDstring == "ShadowType")
			{
				data.shadowType = new WallLight.ShadowType(ExtEnum<WallLight.ShadowType>.values.GetEntry((data.shadowType.Index + 1) % ExtEnum<WallLight.ShadowType>.values.Count));
				data.obj.shadowType = data.shadowType;
				(sender as Button).Text = HardShadowButtonText;
			}
		}
	}

	public WallLight obj;

	public WallLight.WallLightData data;

	public Handle posHandle;

	public Handle upHandle;

	public Handle rightHandle;

	public Handle oneHandle;

	public WallLightRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Wall Light")
	{
		data = pObj.data as WallLight.WallLightData;
		if (data.obj == null)
		{
			data.obj = WallLight.FromPlacedObject(owner.room, base.pObj);
			data.obj.pos = pObj.pos;
			owner.room.AddObject(data.obj);
		}
		obj = data.obj;
		WallLightPanel wallLightPanel = new WallLightPanel(owner, "WallLightPanel", this, default(Vector2));
		subNodes.Add(wallLightPanel);
		wallLightPanel.pos = data.panelPos;
		subNodes.Add(upHandle = new TitledHandle(owner, "UpHandle", this, default(Vector2), "WallLight up")
		{
			pos = data.up
		});
		subNodes.Add(rightHandle = new TitledHandle(owner, "RightHandle", this, default(Vector2), "WallLight right")
		{
			pos = data.right
		});
		subNodes.Add(oneHandle = new TitledHandle(owner, "OneHandle", this, default(Vector2), "WallLight right up")
		{
			pos = data.one
		});
		for (int i = 0; i < 4; i++)
		{
			fSprites.Add(Line());
		}
	}

	public FSprite Line()
	{
		FSprite fSprite = new FSprite("pixel")
		{
			anchorY = 0f
		};
		owner.placedObjectsContainer.AddChild(fSprite);
		return fSprite;
	}

	public override void Refresh()
	{
		base.Refresh();
		if (data.pos != pObj.pos)
		{
			obj.pos = (data.pos = pObj.pos);
		}
		if (data.up != upHandle.pos)
		{
			obj.up = (data.up = upHandle.pos);
		}
		if (data.right != rightHandle.pos)
		{
			obj.right = (data.right = rightHandle.pos);
		}
		if (data.one != oneHandle.pos)
		{
			obj.one = (data.one = oneHandle.pos);
		}
		Panel panel = subNodes[0] as WallLightPanel;
		data.panelPos = panel.pos;
		fSprites[1].scaleY = (data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero)).magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero));
		fSprites[2].scaleY = data.up.magnitude;
		fSprites[2].rotation = Custom.VecToDeg(data.up);
		fSprites[3].scaleY = data.right.magnitude;
		fSprites[3].rotation = Custom.VecToDeg(data.right);
		fSprites[4].scaleY = (data.one - data.right).magnitude;
		fSprites[4].rotation = Custom.VecToDeg(data.one - data.right);
		for (int i = 1; i < 4; i++)
		{
			MoveSprite(i, absPos);
		}
		MoveSprite(4, absPos + data.right);
	}
}
