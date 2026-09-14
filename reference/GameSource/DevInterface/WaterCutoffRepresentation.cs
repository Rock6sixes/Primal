using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class WaterCutoffRepresentation : PlacedObjectRepresentation
{
	public class WaterCutoffControlPanel : Panel
	{
		public Cycler floodCycler;

		public WaterCutoffData Data => (parentNode as PlacedObjectRepresentation).pObj.data as WaterCutoffData;

		public WaterCutoffControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
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

	public WaterCutoffControlPanel controlPanel;

	public int handleLineSprite;

	public int panelLineSprite;

	public Handle handle;

	public WaterCutoffData Data => pObj.data as WaterCutoffData;

	public WaterCutoffRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		fSprites.Add(new FSprite("pixel")
		{
			color = Color.blue
		});
		handleLineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[handleLineSprite]);
		fSprites[handleLineSprite].anchorY = 0f;
		handle = new Handle(owner, "Water_Cutoff_End", this, Data.handlePos);
		subNodes.Add(handle);
		controlPanel = new WaterCutoffControlPanel(owner, "Water_Cutoff_Panel", this, new Vector2(0f, 100f), "Water Cutoff");
		subNodes.Add(controlPanel);
		controlPanel.pos = Data.panelPos;
		fSprites.Add(new FSprite("pixel"));
		panelLineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[panelLineSprite]);
		fSprites[panelLineSprite].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if (handle.pos != Data.handlePos && !flag)
		{
			handle.pos.y = 0f;
		}
		Data.handlePos = handle.pos;
		MoveSprite(handleLineSprite, absPos);
		fSprites[handleLineSprite].scaleY = handle.pos.magnitude;
		fSprites[handleLineSprite].rotation = Custom.VecToDeg(handle.pos);
		MoveSprite(panelLineSprite, absPos);
		fSprites[panelLineSprite].scaleY = controlPanel.pos.magnitude;
		fSprites[panelLineSprite].rotation = Custom.VecToDeg(controlPanel.pos);
		Data.panelPos = controlPanel.pos;
	}
}
