using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class KarmaFlowerPatchRepresentation : ResizeableObjectRepresentation
{
	public class KarmaFlowerPatchControlPanel : Panel
	{
		public class GlowSlider : Slider
		{
			public KarmaFlowerPatch.KarmaFlowerPatchData Data => (parentNode as KarmaFlowerPatchControlPanel).Data;

			public GlowSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "Strength_Slider":
					num = Data.glowStrength;
					break;
				case "Radius_Slider":
					num = Data.glowRadius;
					break;
				}
				base.NumberText = (int)(num * 100f) + "%";
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Strength_Slider":
					Data.glowStrength = nubPos;
					break;
				case "Radius_Slider":
					Data.glowRadius = nubPos;
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public KarmaFlowerPatch.KarmaFlowerPatchData Data => (parentNode as KarmaFlowerPatchRepresentation).Data;

		public KarmaFlowerPatchControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Karma Flower Patch")
		{
			subNodes.Add(new GlowSlider(owner, "Strength_Slider", this, new Vector2(5f, 25f), "Glow Strength: "));
			subNodes.Add(new GlowSlider(owner, "Radius_Slider", this, new Vector2(5f, 5f), "Glow Radius: "));
		}

		public override void Move(Vector2 newPos)
		{
			base.Move(newPos);
			parentNode.Refresh();
		}
	}

	public Handle tiltHandle;

	public int panelLineSprite;

	public KarmaFlowerPatchControlPanel controlPanel;

	public KarmaFlowerPatch.KarmaFlowerPatchData Data => pObj.data as KarmaFlowerPatch.KarmaFlowerPatchData;

	public KarmaFlowerPatchRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name, showRing: true)
	{
		tiltHandle = new Handle(owner, "KarmaFlowerPatch_Tilt", this, Data.handlePos * Data.tilt);
		subNodes.Add(tiltHandle);
		panelLineSprite = fSprites.Count;
		fSprites.Add(new FSprite("pixel")
		{
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(fSprites[panelLineSprite]);
		controlPanel = new KarmaFlowerPatchControlPanel(owner, "Patch_Control_Panel", this, Data.panelPos);
		subNodes.Add(controlPanel);
	}

	public override void Update()
	{
		float glowStrength = Data.glowStrength;
		float glowRadius = Data.glowRadius;
		float tilt = Data.tilt;
		Vector2 handlePos = Data.handlePos;
		Vector2 vector = pObj.pos;
		base.Update();
		if (Data.glowStrength == glowStrength && Data.glowRadius == glowRadius && Data.tilt == tilt && !(Data.handlePos != handlePos) && !(pObj.pos != vector))
		{
			return;
		}
		foreach (UpdatableAndDeletable update in owner.room.updateList)
		{
			if (update is KarmaFlowerPatch karmaFlowerPatch)
			{
				karmaFlowerPatch.PlaceFlowers();
				break;
			}
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (tiltHandle.dragged)
		{
			Data.tilt = Mathf.Clamp01(Vector2.Dot(tiltHandle.pos, Data.handlePos / Vector2.SqrMagnitude(Data.handlePos)));
		}
		tiltHandle.Move(Data.handlePos * Data.tilt);
		Data.panelPos = controlPanel.pos;
		MoveSprite(panelLineSprite, absPos);
		fSprites[panelLineSprite].scaleY = Data.panelPos.magnitude;
		fSprites[panelLineSprite].rotation = Custom.VecToDeg(Data.panelPos);
	}
}
