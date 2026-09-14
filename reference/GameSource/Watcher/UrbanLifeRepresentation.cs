using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanLifeRepresentation : PlacedObjectRepresentation
{
	public class UrbanLifePanel : Panel, IDevUISignals
	{
		public class UrbanLifeSlider : Slider
		{
			public UrbanLifeSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, float titleWidth = 110f)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, titleWidth)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null)
				{
					UrbanLife.UrbanLifeData data = (parentNode.parentNode as UrbanLifeRepresentation).data;
					if (iDstring == "Intensity")
					{
						data.intensity = nubPos;
					}
					else if (iDstring == "NLayers")
					{
						data.nLayers = nubPos;
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
				UrbanLife.UrbanLifeData data = (parentNode.parentNode as UrbanLifeRepresentation).data;
				if (iDstring == "Intensity")
				{
					nubPos = data.intensity;
					base.NumberText = nubPos + " ";
				}
				else if (iDstring == "NLayers")
				{
					nubPos = data.nLayers;
					base.NumberText = (int)(data.nLayers * (float)UrbanLife.maxLayers) + " ";
				}
				RefreshNubPos(nubPos);
			}
		}

		public UrbanLife.UrbanLifeData data => (parentNode as UrbanLifeRepresentation).data;

		public string TypeToggleText
		{
			get
			{
				if (!data.isShadow)
				{
					return "Window";
				}
				return "Shadow";
			}
		}

		public UrbanLifePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Urban Life")
		{
			AddPanelElements();
		}

		public void AddPanelElements()
		{
			size.y = -15f;
			subNodes.Add(new Button(owner, "TypeToggle", this, new Vector2(125f, base.panelStep), 115f, TypeToggleText));
			AddColorControls();
			size.y += 20f;
		}

		public void AddColorControls()
		{
			subNodes.Add(new UrbanLifeSlider(owner, "Intensity", this, new Vector2(5f, base.panelStep), "Intensity: "));
			subNodes.Add(new UrbanLifeSlider(owner, "NLayers", this, new Vector2(5f, base.panelStep), "Layers: "));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (sender.IDstring == "TypeToggle")
			{
				data.isShadow = !data.isShadow;
				(sender as Button).Text = TypeToggleText;
			}
		}
	}

	public UrbanLife obj;

	public UrbanLife.UrbanLifeData data;

	public Handle upLeftHandle;

	public Handle upRightHandle;

	public Handle downLeftHandle;

	public Handle downRightHandle;

	public Handle directionHandle;

	public Handle perScreenOffsetHandle;

	public int screenIndex = -1;

	public AxisHandle scaleHandle;

	public UrbanLifeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Urban Life")
	{
		data = pObj.data as UrbanLife.UrbanLifeData;
		if (data.obj == null)
		{
			data.obj = UrbanLife.FromPlacedObject(owner.room, base.pObj);
			owner.room.AddObject(data.obj);
		}
		obj = data.obj;
		UrbanLifePanel urbanLifePanel = new UrbanLifePanel(owner, "UrbanLifePanel", this, default(Vector2));
		subNodes.Add(urbanLifePanel);
		urbanLifePanel.pos = data.panelPos;
		subNodes.Add(upLeftHandle = new TitledHandle(owner, "upLeftHandle", this, default(Vector2), "UrbanLife up left")
		{
			pos = data.upLeft
		});
		subNodes.Add(downRightHandle = new TitledHandle(owner, "downRightHandle", this, default(Vector2), "UrbanLife down right")
		{
			pos = data.downRight
		});
		subNodes.Add(directionHandle = new TitledHandle(owner, "directionHandle", this, default(Vector2), "UrbanLife direction")
		{
			pos = data.direction
		});
		scaleHandle = AddScaleHandle(directionHandle, data.scale);
		for (int i = 0; i < 1; i++)
		{
			fSprites.Add(Line());
		}
	}

	public AxisHandle AddScaleHandle(DevUINode node, float startPos)
	{
		AxisHandle axisHandle = new AxisHandle(owner, "horizontal_handle", node, Vector2.zero, Vector2.down * 30f, AxisHandle.Axis.X, "Scale");
		axisHandle.pos.x = startPos;
		node.subNodes.Add(axisHandle);
		return axisHandle;
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
			data.pos = pObj.pos;
		}
		if (data.upLeft != upLeftHandle.pos)
		{
			data.upLeft = upLeftHandle.pos;
		}
		if (data.downRight != downRightHandle.pos)
		{
			data.downRight = downRightHandle.pos;
		}
		if (data.direction != directionHandle.pos)
		{
			data.direction = directionHandle.pos;
		}
		if (scaleHandle.dragged)
		{
			data.scale = scaleHandle.pos.x;
		}
		Panel panel = subNodes[0] as UrbanLifePanel;
		data.panelPos = panel.pos;
		fSprites[1].scaleY = (data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero)).magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero));
		for (int i = 1; i < 2; i++)
		{
			MoveSprite(i, absPos);
		}
	}
}
