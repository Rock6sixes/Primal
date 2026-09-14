using RWCustom;
using UnityEngine;

namespace DevInterface;

public class HarmfulSteamRepresentation : PlacedObjectRepresentation
{
	public class HarmfulSteamPanel : Panel
	{
		public class HarmfulSteamSlider : Slider
		{
			public HarmfulSteamSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "Duration_Slider":
					num = ((parentNode.parentNode as HarmfulSteamRepresentation).pObj.data as PlacedObject.HarmfulSteamData).duration;
					base.NumberText = ((int)Mathf.Lerp(0f, 10f, num)).ToString();
					break;
				case "Frequency_Slider":
					num = ((parentNode.parentNode as HarmfulSteamRepresentation).pObj.data as PlacedObject.HarmfulSteamData).frequency;
					base.NumberText = ((int)Mathf.Lerp(0f, 10f, num)).ToString();
					break;
				case "Lifetime_Slider":
					num = ((parentNode.parentNode as HarmfulSteamRepresentation).pObj.data as PlacedObject.HarmfulSteamData).lifetime;
					base.NumberText = ((int)Mathf.Lerp(0f, 1f, num)).ToString();
					break;
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Duration_Slider":
					((parentNode.parentNode as HarmfulSteamRepresentation).pObj.data as PlacedObject.HarmfulSteamData).duration = nubPos;
					break;
				case "Frequency_Slider":
					((parentNode.parentNode as HarmfulSteamRepresentation).pObj.data as PlacedObject.HarmfulSteamData).frequency = nubPos;
					break;
				case "Lifetime_Slider":
					((parentNode.parentNode as HarmfulSteamRepresentation).pObj.data as PlacedObject.HarmfulSteamData).lifetime = nubPos;
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public HarmfulSteamPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 75f), "Harmful Steam")
		{
			subNodes.Add(new HarmfulSteamSlider(owner, "Duration_Slider", this, new Vector2(5f, 45f), "Duration: "));
			subNodes.Add(new HarmfulSteamSlider(owner, "Frequency_Slider", this, new Vector2(5f, 25f), "Frequency: "));
			subNodes.Add(new HarmfulSteamSlider(owner, "Lifetime_Slider", this, new Vector2(5f, 5f), "Lifetime: "));
		}
	}

	public HarmfulSteamRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Harmful Steam")
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.HarmfulSteamData).handlePos;
		subNodes.Add(new HarmfulSteamPanel(owner, "Harmful_Steam_Panel", this, (pObj.data as PlacedObject.HarmfulSteamData).panelPos));
		(subNodes[subNodes.Count - 1] as HarmfulSteamPanel).pos = (pObj.data as PlacedObject.HarmfulSteamData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites[fSprites.Count - 1].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites[fSprites.Count - 1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		MoveSprite(2, absPos);
		fSprites[1].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		MoveSprite(3, absPos);
		fSprites[2].scaleY = (subNodes[1] as HarmfulSteamPanel).pos.magnitude;
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as HarmfulSteamPanel).absPos);
		(pObj.data as PlacedObject.HarmfulSteamData).handlePos = (subNodes[0] as Handle).pos;
	}
}
