using RWCustom;
using UnityEngine;

namespace DevInterface;

public class PrinceFilterRepresentation : ResizeableObjectRepresentation
{
	public class PrinceFilterControlPanel : Panel, IDevUISignals
	{
		public class PrinceStageSlider : Slider
		{
			public PlacedObject.PrinceFilterData PrinceFilterData => (parentNode.parentNode as PrinceFilterRepresentation).pObj.data as PlacedObject.PrinceFilterData;

			public PrinceStageSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public string StageText(int stage)
			{
				return stage switch
				{
					0 => "Not Bloomed", 
					1 => "Slightly Bloomed", 
					2 => "Almost Bloomed", 
					_ => "Fully Bloomed", 
				};
			}

			public override void Refresh()
			{
				base.Refresh();
				base.NumberText = StageText(PrinceFilterData.stage);
				RefreshNubPos((float)PrinceFilterData.stage / 3f);
			}

			public override void NubDragged(float nubPos)
			{
				PrinceFilterData.stage = (int)(nubPos * 3f);
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public PrinceFilterControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 65f), "Placable Prince Filter")
		{
			subNodes.Add(new PrinceStageSlider(owner, "Stage_Slider", this, new Vector2(5.01f, 45f), "Stage:"));
			subNodes.Add(new Button(owner, "Mode_Button", this, new Vector2(5f, 25f), 100f, ModeButtonText(((parentNode as PrinceFilterRepresentation).pObj.data as PlacedObject.PrinceFilterData).greaterOrEqual)));
			subNodes.Add(new Button(owner, "PostGame_Button", this, new Vector2(5f, 5f), 100f, PostGameButtonText(((parentNode as PrinceFilterRepresentation).pObj.data as PlacedObject.PrinceFilterData).postGame)));
		}

		public string ModeButtonText(bool greaterOrEqual)
		{
			if (!greaterOrEqual)
			{
				return "Less or Equal";
			}
			return "Greater or Equal";
		}

		public string PostGameButtonText(bool postGame)
		{
			if (!postGame)
			{
				return "Any Time";
			}
			return "Postgame Only";
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			PlacedObject.PrinceFilterData princeFilterData = (parentNode as PrinceFilterRepresentation).pObj.data as PlacedObject.PrinceFilterData;
			switch (sender.IDstring)
			{
			case "Mode_Button":
				princeFilterData.greaterOrEqual = !princeFilterData.greaterOrEqual;
				(sender as Button).Text = ModeButtonText(princeFilterData.greaterOrEqual);
				break;
			case "PostGame_Button":
				princeFilterData.postGame = !princeFilterData.postGame;
				(sender as Button).Text = PostGameButtonText(princeFilterData.postGame);
				break;
			}
			Refresh();
		}
	}

	public int lineSprite;

	public PrinceFilterRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "PrinceFilter", showRing: true)
	{
		subNodes.Add(new PrinceFilterControlPanel(owner, "PrinceFilter_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as PrinceFilterControlPanel).pos = (pObj.data as PlacedObject.PrinceFilterData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = (subNodes[1] as PrinceFilterControlPanel).pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as PrinceFilterControlPanel).absPos);
		(pObj.data as PlacedObject.PrinceFilterData).panelPos = (subNodes[1] as Panel).pos;
	}
}
