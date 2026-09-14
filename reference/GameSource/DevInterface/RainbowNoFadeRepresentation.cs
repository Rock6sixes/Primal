using RWCustom;
using UnityEngine;

namespace DevInterface;

public class RainbowNoFadeRepresentation : ResizeableObjectRepresentation
{
	public class RainbowNoFadeControlPanel : Panel
	{
		public class FadeSlider : Slider
		{
			public int index;

			public RainbowNoFade.RainbowNoFadeData RainbowNoFadeData => (parentNode.parentNode as RainbowNoFadeRepresentation).pObj.data as RainbowNoFade.RainbowNoFadeData;

			public FadeSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, int index)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
				this.index = index;
			}

			public override void Refresh()
			{
				base.Refresh();
				base.NumberText = Mathf.RoundToInt(RainbowNoFadeData.fades[index] * 100f) + "%";
				RefreshNubPos(RainbowNoFadeData.fades[index]);
			}

			public override void NubDragged(float nubPos)
			{
				RainbowNoFadeData.fades[index] = nubPos;
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public RainbowNoFade.RainbowNoFadeData RainbowNoFadeData => (parentNode as RainbowNoFadeRepresentation).pObj.data as RainbowNoFade.RainbowNoFadeData;

		public RainbowNoFadeControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 125f), "RainbowNoFade")
		{
			for (int i = 0; i < 4; i++)
			{
				subNodes.Add(new FadeSlider(owner, "Fade_Slider", this, new Vector2(5.01f, 5f + 20f * (float)i), "Fade " + i + ":", i));
			}
			subNodes.Add(new FadeSlider(owner, "Thick_Slider", this, new Vector2(5.01f, 85f), "Thickness:", 4));
			subNodes.Add(new FadeSlider(owner, "Chance_Slider", this, new Vector2(5.01f, 105f), "Per cycle chance:", 5));
		}
	}

	public int lineSprite;

	public RainbowNoFade RainbowNoFade;

	public RainbowNoFadeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
		: base(owner, IDstring, parentNode, pObj, "RainbowNoFade", showRing: false)
	{
		subNodes.Add(new RainbowNoFadeControlPanel(owner, "RainbowNoFade_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as RainbowNoFadeControlPanel).pos = (pObj.data as RainbowNoFade.RainbowNoFadeData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
		for (int i = 0; i < owner.room.updateList.Count; i++)
		{
			if (owner.room.updateList[i] is RainbowNoFade && (owner.room.updateList[i] as RainbowNoFade).placedObject == pObj)
			{
				RainbowNoFade = owner.room.updateList[i] as RainbowNoFade;
				break;
			}
		}
		if (RainbowNoFade == null)
		{
			RainbowNoFade = new RainbowNoFade(owner.room, pObj);
			owner.room.AddObject(RainbowNoFade);
		}
		RainbowNoFade.alwaysShow = true;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = (subNodes[1] as RainbowNoFadeControlPanel).pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as RainbowNoFadeControlPanel).absPos);
		RainbowNoFade.Refresh();
		(pObj.data as RainbowNoFade.RainbowNoFadeData).panelPos = (subNodes[1] as Panel).pos;
	}
}
