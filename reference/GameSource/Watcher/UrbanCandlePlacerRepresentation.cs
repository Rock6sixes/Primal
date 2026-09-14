using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanCandlePlacerRepresentation : PlacedObjectRepresentation
{
	public class UrbanCandlePlacerPanel : Panel, IDevUISignals
	{
		public class UrbanCandlePlacerSlider : Slider
		{
			public UrbanCandlePlacerRepresentation representation;

			public UrbanCandlePlacerSlider(UrbanCandlePlacerRepresentation representation, DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, float titleWidth = 110f)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, titleWidth)
			{
				this.representation = representation;
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null)
				{
					UrbanCandlePlacer.UrbanCandlePlacerData data = representation.data;
					switch (iDstring)
					{
					case "RotationVariation":
						data.rotationVariation = nubPos * 90f;
						break;
					case "Rotation":
						data.rotation = (nubPos * 2f - 1f) * 90f;
						break;
					case "WidthMax":
						data.widthMax = nubPos * 10f;
						break;
					case "WidthMin":
						data.widthMin = nubPos * 10f;
						break;
					case "HeightMax":
						data.heightMax = nubPos * 10f;
						break;
					case "HeightMin":
						data.heightMin = nubPos * 10f;
						break;
					case "DepthMax":
						data.depthMax = (int)(nubPos * 30f);
						representation.ShowDepth(0.6f);
						break;
					case "DepthMin":
						data.depthMin = (int)(nubPos * 30f);
						representation.ShowDepth(0.6f);
						break;
					case "MaxCandles":
						data.maxCandles = (int)(nubPos * 20f);
						break;
					}
					representation.Refresh();
					Refresh();
				}
			}

			public override void Refresh()
			{
				base.Refresh();
				float nubPos = 0f;
				string iDstring = IDstring;
				UrbanCandlePlacer.UrbanCandlePlacerData data = representation.data;
				switch (iDstring)
				{
				case "RotationVariation":
					nubPos = data.rotationVariation / 90f;
					base.NumberText = data.rotationVariation + " ";
					break;
				case "Rotation":
					nubPos = data.rotation / 90f * 0.5f + 0.5f;
					base.NumberText = data.rotation + " ";
					break;
				case "WidthMax":
					nubPos = data.widthMax / 10f;
					base.NumberText = data.widthMax + " ";
					break;
				case "WidthMin":
					nubPos = data.widthMin / 10f;
					base.NumberText = data.widthMin + " ";
					break;
				case "HeightMax":
					nubPos = data.heightMax / 10f;
					base.NumberText = data.heightMax + " ";
					break;
				case "HeightMin":
					nubPos = data.heightMin / 10f;
					base.NumberText = data.heightMin + " ";
					break;
				case "DepthMax":
					nubPos = (float)data.depthMax / 30f;
					base.NumberText = data.depthMax + " ";
					break;
				case "DepthMin":
					nubPos = (float)data.depthMin / 30f;
					base.NumberText = data.depthMin + " ";
					break;
				case "MaxCandles":
					nubPos = (float)data.maxCandles / 20f;
					base.NumberText = data.maxCandles + " ";
					break;
				}
				RefreshNubPos(nubPos);
			}
		}

		public UrbanCandlePlacerRepresentation representation;

		public UrbanCandlePlacer.UrbanCandlePlacerData data => representation.data;

		public UrbanCandlePlacerPanel(UrbanCandlePlacerRepresentation representation, DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Urban Candle Placer")
		{
			this.representation = representation;
			AddPanelElements();
		}

		public void AddPanelElements()
		{
			size.y = -15f;
			subNodes.Add(new Button(owner, "RemoveAllCandles", this, new Vector2(5f, base.panelStep), 115f, "Remove All Candles"));
			AddSliders();
			subNodes.Add(new Button(owner, "RemoveCandles", this, new Vector2(125f, base.panelStep), 115f, "Remove Candles"));
			subNodes.Add(new Button(owner, "SpawnCandles", this, new Vector2(5f, size.y), 115f, "Spawn Candles"));
			size.y += 20f;
		}

		public void AddSliders()
		{
			subNodes.Add(new UrbanCandlePlacerSlider(representation, owner, "RotationVariation", this, new Vector2(5f, base.panelStep), "RotationVariation: "));
			subNodes.Add(new UrbanCandlePlacerSlider(representation, owner, "Rotation", this, new Vector2(5f, base.panelStep), "Rotation: "));
			subNodes.Add(new UrbanCandlePlacerSlider(representation, owner, "WidthMax", this, new Vector2(5f, base.panelStep), "WidthMax: "));
			subNodes.Add(new UrbanCandlePlacerSlider(representation, owner, "WidthMin", this, new Vector2(5f, base.panelStep), "WidthMin: "));
			subNodes.Add(new UrbanCandlePlacerSlider(representation, owner, "HeightMax", this, new Vector2(5f, base.panelStep), "HeightMax: "));
			subNodes.Add(new UrbanCandlePlacerSlider(representation, owner, "HeightMin", this, new Vector2(5f, base.panelStep), "HeightMin: "));
			subNodes.Add(new UrbanCandlePlacerSlider(representation, owner, "DepthMax", this, new Vector2(5f, base.panelStep), "DepthMax: "));
			subNodes.Add(new UrbanCandlePlacerSlider(representation, owner, "DepthMin", this, new Vector2(5f, base.panelStep), "DepthMin: "));
			subNodes.Add(new UrbanCandlePlacerSlider(representation, owner, "MaxCandles", this, new Vector2(5f, base.panelStep), "MaxPerSpawn: "));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (sender.IDstring == "SpawnCandles")
			{
				UrbanCandlePlacer.AddCandles(data.candles, owner.room, data.brushHandlePos, data.radius, data.depthMin, data.depthMax, data.heightMin, data.heightMax, data.widthMin, data.widthMax, data.rotation, data.rotationVariation, data.maxCandles);
				UrbanCandlePlacer.RespawnCandles(data.placedCandles, data.candles, owner.room);
			}
			if (sender.IDstring == "RemoveCandles")
			{
				UrbanCandlePlacer.RemoveCandles(data.candles, data.brushHandlePos, data.radius, data.depthMin, data.depthMax);
				UrbanCandlePlacer.RespawnCandles(data.placedCandles, data.candles, owner.room);
			}
			if (sender.IDstring == "RemoveAllCandles")
			{
				data.candles.Clear();
				UrbanCandlePlacer.RespawnCandles(data.placedCandles, data.candles, owner.room);
			}
		}
	}

	public UrbanCandlePlacer.UrbanCandlePlacerData data;

	public Handle radHandle;

	public TitledHandle brushHandle;

	public FSprite depthPreview;

	public FSprite brushCircle;

	public FSprite panelLine;

	public Counter depthPreviewCounter = new Counter(40);

	public Vector2? savedBrushPos;

	public UrbanCandlePlacerRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Urban Candle Placer")
	{
		data = pObj.data as UrbanCandlePlacer.UrbanCandlePlacerData;
		UrbanCandlePlacerPanel urbanCandlePlacerPanel = new UrbanCandlePlacerPanel(this, owner, "UrbanCandlePlacerPanel", parentNode, default(Vector2));
		subNodes.Add(urbanCandlePlacerPanel);
		urbanCandlePlacerPanel.pos = data.panelPos;
		urbanCandlePlacerPanel.collapsed = true;
		subNodes.Add(brushHandle = new TitledHandle(owner, "Brush_Handle", parentNode, new Vector2(0f, 0f), "Urban Candle Brush"));
		brushHandle.pos = urbanCandlePlacerPanel.pos + Vector2.left * 50f + (urbanCandlePlacerPanel.collapsed ? (Vector2.up * urbanCandlePlacerPanel.size.y) : Vector2.zero);
		subNodes.Add(radHandle = new Handle(owner, "Rad_Handle", brushHandle, new Vector2(0f, 100f)));
		radHandle.pos = data.handlePos;
		fSprites.Add(brushCircle = new FSprite("Futile_White")
		{
			shader = Custom.rainWorld.Shaders["VectorCircle"]
		});
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites.Add(depthPreview = new FSprite("Futile_White")
		{
			shader = Custom.rainWorld.Shaders["DevUIDepthPreview"]
		});
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		depthPreviewCounter.SetToMin();
		fSprites.Add(panelLine = Line());
	}

	public void ShowDepth(float alpha)
	{
		float a = (float)data.depthMin / 30f;
		float b = (float)data.depthMax / 30f;
		UrbanCandlePlacer.CorrectOrder(ref a, ref b);
		depthPreview.color = new Color(a, b, 0f, alpha);
		depthPreviewCounter.Reset();
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

	public void ToggleBrushHandle()
	{
		Vector2? vector = savedBrushPos;
		if (vector.HasValue)
		{
			Vector2 valueOrDefault = vector.GetValueOrDefault();
			brushHandle.pos = valueOrDefault;
			savedBrushPos = null;
		}
		else
		{
			savedBrushPos = brushHandle.pos;
			brushHandle.pos += Vector2.down * 5000f;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		depthPreviewCounter.Tick();
		Vector2 vector = owner.room.game.cameras[0].pos;
		if (brushHandle.dragged || radHandle.dragged)
		{
			ShowDepth(0.2f);
		}
		if (data.pos != pObj.pos)
		{
			data.pos = pObj.pos;
		}
		data.radius = radHandle.pos.magnitude;
		brushCircle.scale = radHandle.pos.magnitude / 8f;
		brushCircle.alpha = 2f / radHandle.pos.magnitude;
		depthPreview.scale = fSprites[1].scale;
		depthPreview.isVisible = !depthPreviewCounter.isFinished;
		Panel panel = subNodes[0] as UrbanCandlePlacerPanel;
		data.panelPos = panel.pos;
		if ((panel.collapsed && !savedBrushPos.HasValue) || (!panel.collapsed && savedBrushPos.HasValue))
		{
			ToggleBrushHandle();
		}
		data.brushHandlePos = brushHandle.pos + vector;
		panelLine.scaleY = (brushHandle.pos - panel.pos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero)).magnitude;
		panelLine.rotation = Custom.AimFromOneVectorToAnother(brushHandle.pos, panel.pos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero));
		MoveSprite(3, brushHandle.absPos);
		MoveSprite(2, brushHandle.absPos);
		MoveSprite(1, brushHandle.absPos);
	}
}
