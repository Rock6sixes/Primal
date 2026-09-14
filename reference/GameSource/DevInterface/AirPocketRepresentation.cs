using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class AirPocketRepresentation : PlacedObjectRepresentation
{
	public class AirPocketControlPanel : Panel
	{
		public Cycler floodCycler;

		public AirPocketData Data => (parentNode as PlacedObjectRepresentation).pObj.data as AirPocketData;

		public AirPocketControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 25f), title)
		{
			floodCycler = new Cycler(owner, "Mode_Cycler", this, new Vector2(5f, 5f), 150f, "Flood: ", new List<string> { "No", "Yes" });
			floodCycler.currentAlternative = (Data.flood ? 1 : 0);
			floodCycler.Text = floodCycler.baseName + floodCycler.alternatives[floodCycler.currentAlternative];
			subNodes.Add(floodCycler);
		}

		public override void Update()
		{
			base.Update();
			Data.flood = floodCycler.alternatives[floodCycler.currentAlternative] == "Yes";
		}
	}

	public AirPocketControlPanel controlPanel;

	public int firstRectSprite;

	public int waterLevelSprite;

	public int panelLineSprite;

	public Handle handle;

	public SliderHandle slider;

	public AirPocketData Data => pObj.data as AirPocketData;

	public AirPocketRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		firstRectSprite = fSprites.Count;
		for (int i = 0; i < 4; i++)
		{
			FSprite fSprite = new FSprite("pixel")
			{
				color = Color.blue,
				anchorY = 0f
			};
			fSprites.Add(fSprite);
			owner.placedObjectsContainer.AddChild(fSprite);
		}
		waterLevelSprite = fSprites.Count;
		FSprite fSprite2 = new FSprite("pixel")
		{
			color = Color.cyan,
			anchorY = 0f
		};
		fSprites.Add(fSprite2);
		owner.placedObjectsContainer.AddChild(fSprite2);
		handle = new Handle(owner, "Air_Pocket_Handle", this, Data.handlePos);
		subNodes.Add(handle);
		slider = new SliderHandle(owner, "Water_Level_Slider", this, new Vector2(0f, 0f), Data.waterLevel, vertical: true, drawLine: false);
		subNodes.Add(slider);
		controlPanel = new AirPocketControlPanel(owner, "Air_Pocket_Panel", this, new Vector2(0f, 100f), "Air Pocket");
		subNodes.Add(controlPanel);
		controlPanel.pos = Data.panelPos;
		panelLineSprite = fSprites.Count;
		fSprites.Add(new FSprite("pixel")
		{
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(fSprites[panelLineSprite]);
	}

	public override void Refresh()
	{
		base.Refresh();
		Data.handlePos = handle.pos;
		Data.waterLevel = slider.Value;
		Data.panelPos = controlPanel.pos;
		Vector2 vector = absPos;
		Vector2 vector2 = absPos + Data.handlePos;
		Vector2 vector3 = absPos + new Vector2(0f, Data.handlePos.y);
		Vector2 vector4 = absPos + new Vector2(Data.handlePos.x, 0f);
		DrawLine(firstRectSprite, vector, vector3);
		DrawLine(firstRectSprite + 1, vector3, vector2);
		DrawLine(firstRectSprite + 2, vector2, vector4);
		DrawLine(firstRectSprite + 3, vector4, vector);
		DrawLine(waterLevelSprite, vector + new Vector2(0f, Data.waterLevel), vector4 + new Vector2(0f, Data.waterLevel));
		DrawLine(panelLineSprite, absPos, absPos + controlPanel.pos);
	}

	public void DrawLine(int sprite, Vector2 from, Vector2 to)
	{
		MoveSprite(sprite, from);
		fSprites[sprite].scaleY = Vector2.Distance(from, to);
		fSprites[sprite].rotation = Custom.AimFromOneVectorToAnother(from, to);
	}
}
