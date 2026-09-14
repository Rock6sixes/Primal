using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanLifePathRepresentation : PlacedObjectRepresentation
{
	public class UrbanLifePathPanel : Panel, IDevUISignals
	{
		public class UrbanLifePathSlider : Slider
		{
			public UrbanLifePathSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, float titleWidth = 110f)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, titleWidth)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null)
				{
					UrbanLifePath.UrbanLifePathData data = (parentNode.parentNode as UrbanLifePathRepresentation).data;
					if (iDstring == "Density")
					{
						data.density = nubPos;
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
				UrbanLifePath.UrbanLifePathData data = (parentNode.parentNode as UrbanLifePathRepresentation).data;
				if (iDstring == "Density")
				{
					nubPos = data.density;
					base.NumberText = nubPos + " ";
				}
				RefreshNubPos(nubPos);
			}
		}

		public UrbanLifePath.UrbanLifePathData data => (parentNode as UrbanLifePathRepresentation).data;

		public UrbanLifePathPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Urban Life Path")
		{
			AddPanelElements();
		}

		public void AddPanelElements()
		{
			size.y = -15f;
			AddColorControls();
			size.y += 20f;
		}

		public void AddColorControls()
		{
			subNodes.Add(new UrbanLifePathSlider(owner, "Density", this, new Vector2(5f, base.panelStep), "Density: "));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
		}
	}

	public UrbanLifePath obj;

	public UrbanLifePath.UrbanLifePathData data;

	public Handle pointAHandle;

	public Handle pointBHandle;

	public AxisHandle scaleHandle;

	public UrbanLifePathRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Urban Life Path")
	{
		data = pObj.data as UrbanLifePath.UrbanLifePathData;
		if (data.obj == null)
		{
			data.obj = UrbanLifePath.FromPlacedObject(owner.room, base.pObj);
			owner.room.AddObject(data.obj);
		}
		obj = data.obj;
		UrbanLifePathPanel urbanLifePathPanel = new UrbanLifePathPanel(owner, "UrbanLifePathPanel", this, default(Vector2));
		subNodes.Add(urbanLifePathPanel);
		urbanLifePathPanel.pos = data.panelPos;
		subNodes.Add(pointAHandle = new TitledHandle(owner, "pointAHandle", this, default(Vector2), "UrbanLifePath Point A")
		{
			pos = data.pointA
		});
		subNodes.Add(pointBHandle = new TitledHandle(owner, "pointBHandle", this, default(Vector2), "UrbanLifePath Point B")
		{
			pos = data.pointB
		});
		scaleHandle = AddScaleHandle(this, data.scale);
		for (int i = 0; i < 2; i++)
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
		if (data.pointA != pointAHandle.pos)
		{
			data.pointA = pointAHandle.pos;
		}
		if (data.pointB != pointBHandle.pos)
		{
			data.pointB = pointBHandle.pos;
		}
		if (scaleHandle.dragged)
		{
			data.scale = scaleHandle.pos.x;
		}
		Panel panel = subNodes[0] as UrbanLifePathPanel;
		data.panelPos = panel.pos;
		fSprites[1].scaleY = (data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero)).magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero));
		MoveSprite(1, absPos);
		fSprites[2].scaleY = (data.pointB - data.pointA).magnitude;
		fSprites[2].rotation = Custom.VecToDeg(data.pointB - data.pointA);
		MoveSprite(2, data.pointA + absPos);
	}
}
