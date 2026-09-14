using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class AetherRainbowRepresentation : PlacedObjectRepresentation
{
	public class AetherRainbowPanel : Panel, IDevUISignals
	{
		public class AetherRainbowSlider : Slider
		{
			public AetherRainbowSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, float titleWidth = 110f)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, titleWidth)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null)
				{
					AetherRainbow.AetherRainbowData data = (parentNode.parentNode as AetherRainbowRepresentation).data;
					switch (iDstring)
					{
					case "Grain":
						data.grain = nubPos;
						data.obj.grain = nubPos;
						break;
					case "Depth":
						data.depth = nubPos;
						data.obj.depth = nubPos;
						break;
					case "Opacity":
						data.opacity = nubPos;
						data.obj.opacity = nubPos;
						break;
					case "Fade":
						data.fade = nubPos;
						data.obj.fade = nubPos;
						break;
					case "SpecterOffset":
						data.specterOffset = nubPos;
						data.obj.specterOffset = nubPos;
						break;
					case "SpecterScale":
						data.specterScale = nubPos;
						data.obj.specterScale = nubPos;
						break;
					case "Width":
						data.width = nubPos;
						data.obj.width = nubPos;
						break;
					case "Chance":
						data.chance = nubPos;
						data.obj.chance = nubPos;
						break;
					}
					parentNode.parentNode.Refresh();
					Refresh();
				}
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				string iDstring = IDstring;
				AetherRainbow.AetherRainbowData data = (parentNode.parentNode as AetherRainbowRepresentation).data;
				switch (iDstring)
				{
				case "Grain":
					num = data.grain;
					base.NumberText = num + " ";
					break;
				case "Depth":
					num = data.depth;
					base.NumberText = num + " ";
					break;
				case "Opacity":
					num = data.opacity;
					base.NumberText = num + " ";
					break;
				case "Fade":
					num = data.fade;
					base.NumberText = num + " ";
					break;
				case "SpecterOffset":
					num = data.specterOffset;
					base.NumberText = num - 0.5f + " ";
					break;
				case "SpecterScale":
					num = data.specterScale;
					base.NumberText = num + " ";
					break;
				case "Width":
					num = data.width;
					base.NumberText = num + " ";
					break;
				case "Chance":
					num = data.chance;
					base.NumberText = (int)(num * 100f) + "%";
					break;
				}
				RefreshNubPos(num);
			}
		}

		public Button fadeButton;

		public Button chanceButton;

		public string fadeToArchName
		{
			get
			{
				if (!(parentNode as AetherRainbowRepresentation).data.fadeToArch)
				{
					return "Fade to Center: ";
				}
				return "Fade to Arch Point:";
			}
		}

		public string chanceName
		{
			get
			{
				if (!(parentNode as AetherRainbowRepresentation).data.alwaysShow)
				{
					return "Per cycle chance:";
				}
				return "No Cycle. Chance:";
			}
		}

		public AetherRainbowPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Aether Rainbow")
		{
			_ = (parentNode as AetherRainbowRepresentation).data;
			size.y = -15f;
			subNodes.Add(new AetherRainbowSlider(owner, "Grain", this, new Vector2(5f, base.panelStep), "Grain: "));
			subNodes.Add(new AetherRainbowSlider(owner, "Depth", this, new Vector2(5f, base.panelStep), "Depth: "));
			subNodes.Add(new AetherRainbowSlider(owner, "Opacity", this, new Vector2(5f, base.panelStep), "Opacity: "));
			subNodes.Add(new AetherRainbowSlider(owner, "Fade", this, new Vector2(115f, base.panelStep), "", 0f));
			subNodes.Add(fadeButton = new Button(owner, "FadeToArch", this, new Vector2(5f, size.y), 110f, fadeToArchName));
			subNodes.Add(new AetherRainbowSlider(owner, "SpecterOffset", this, new Vector2(5f, base.panelStep), "Specter offset: "));
			subNodes.Add(new AetherRainbowSlider(owner, "SpecterScale", this, new Vector2(5f, base.panelStep), "Specter scale: "));
			subNodes.Add(new AetherRainbowSlider(owner, "Width", this, new Vector2(5f, base.panelStep), "Width: "));
			subNodes.Add(new AetherRainbowSlider(owner, "Chance", this, new Vector2(115f, base.panelStep), "", 0f));
			subNodes.Add(chanceButton = new Button(owner, "AlwaysShow", this, new Vector2(5f, size.y), 110f, chanceName));
			size.y += 20f;
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			AetherRainbow.AetherRainbowData data = (parentNode as AetherRainbowRepresentation).data;
			if (sender.IDstring == "FadeToArch")
			{
				data.fadeToArch = !data.fadeToArch;
				data.obj.fadeToArch = data.fadeToArch;
				fadeButton.Text = fadeToArchName;
			}
			if (sender.IDstring == "AlwaysShow")
			{
				data.alwaysShow = !data.alwaysShow;
				data.obj.alwaysShow = data.alwaysShow;
				chanceButton.Text = chanceName;
			}
		}
	}

	public AetherRainbow obj;

	public AetherRainbow.AetherRainbowData data;

	public Handle startHandle;

	public Handle endHandle;

	public Handle archHandle;

	public AxisHandle startDepth;

	public AxisHandle endDepth;

	public AxisHandle archDepth;

	public AxisHandle startWidth;

	public AxisHandle endWidth;

	public AxisHandle archWidth;

	public AetherRainbowRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Aether Rainbow")
	{
		data = pObj.data as AetherRainbow.AetherRainbowData;
		if (data.obj == null)
		{
			data.obj = AetherRainbow.FromPlacedObject(owner.room, base.pObj);
			data.obj.pos = pObj.pos;
			owner.room.AddObject(data.obj);
		}
		obj = data.obj;
		AetherRainbowPanel aetherRainbowPanel = new AetherRainbowPanel(owner, "AetherRainbowPanel", this, default(Vector2));
		subNodes.Add(aetherRainbowPanel);
		aetherRainbowPanel.pos = data.panelPos;
		subNodes.Add(startHandle = new TitledHandle(owner, "StartHandle", this, default(Vector2), "Start Point")
		{
			pos = data.start
		});
		subNodes.Add(archHandle = new TitledHandle(owner, "ArchHandle", this, default(Vector2), "Arch Point")
		{
			pos = data.arch
		});
		subNodes.Add(endHandle = new TitledHandle(owner, "EndHandle", this, default(Vector2), "End Point")
		{
			pos = data.end
		});
		startDepth = AddDepthOffsetHandle(startHandle, data.startDepth);
		archDepth = AddDepthOffsetHandle(archHandle, data.archDepth);
		endDepth = AddDepthOffsetHandle(endHandle, data.endDepth);
		startWidth = AddWidthOffsetHandle(startHandle, data.startWidth);
		archWidth = AddWidthOffsetHandle(archHandle, data.archWidth);
		endWidth = AddWidthOffsetHandle(endHandle, data.endWidth);
		for (int i = 0; i < 4; i++)
		{
			FSprite fSprite = new FSprite("pixel")
			{
				anchorY = 0f
			};
			fSprites.Add(fSprite);
			owner.placedObjectsContainer.AddChild(fSprite);
		}
	}

	public AxisHandle AddDepthOffsetHandle(DevUINode node, float startPos)
	{
		AxisHandle axisHandle = new AxisHandle(owner, "horizontal_handle", node, Vector2.zero, Vector2.down * 30f, AxisHandle.Axis.X, "Depth Offset");
		axisHandle.pos.x = startPos * 100f;
		node.subNodes.Add(axisHandle);
		return axisHandle;
	}

	public AxisHandle AddWidthOffsetHandle(DevUINode node, float startPos)
	{
		AxisHandle axisHandle = new AxisHandle(owner, "horizontal_handle", node, Vector2.zero, Vector2.down * 60f, AxisHandle.Axis.X, "Width Offset");
		axisHandle.pos.x = startPos * 100f;
		node.subNodes.Add(axisHandle);
		return axisHandle;
	}

	public override void Refresh()
	{
		base.Refresh();
		Panel panel = subNodes[0] as AetherRainbowPanel;
		data.panelPos = panel.pos;
		fSprites[1].scaleY = (data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero)).magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero));
		fSprites[2].scaleY = data.start.magnitude;
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.start);
		fSprites[3].scaleY = data.arch.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.arch);
		fSprites[4].scaleY = data.end.magnitude;
		fSprites[4].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.end);
		if (data.pos != pObj.pos)
		{
			obj.pos = (data.pos = pObj.pos);
		}
		if (data.start != startHandle.pos)
		{
			obj.start = (data.start = startHandle.pos);
		}
		if (data.arch != archHandle.pos)
		{
			obj.arch = (data.arch = archHandle.pos);
		}
		if (data.end != endHandle.pos)
		{
			obj.end = (data.end = endHandle.pos);
		}
		if (endDepth.dragged)
		{
			obj.endDepth = (data.endDepth = endDepth.pos.x * 0.01f);
		}
		if (startDepth.dragged)
		{
			obj.startDepth = (data.startDepth = startDepth.pos.x * 0.01f);
		}
		if (archDepth.dragged)
		{
			obj.archDepth = (data.archDepth = archDepth.pos.x * 0.01f);
		}
		if (endWidth.dragged)
		{
			obj.endWidth = (data.endWidth = endWidth.pos.x * 0.01f);
		}
		if (startWidth.dragged)
		{
			obj.startWidth = (data.startWidth = startWidth.pos.x * 0.01f);
		}
		if (archWidth.dragged)
		{
			obj.archWidth = (data.archWidth = archWidth.pos.x * 0.01f);
		}
		for (int i = 1; i < 5; i++)
		{
			MoveSprite(i, absPos);
		}
	}
}
