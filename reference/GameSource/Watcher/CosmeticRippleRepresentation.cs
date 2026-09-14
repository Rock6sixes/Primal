using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class CosmeticRippleRepresentation : PlacedObjectRepresentation
{
	public class CosmeticRipplePanel : Panel, IDevUISignals
	{
		public class CosmeticRippleSlider : Slider
		{
			public CosmeticRippleSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null)
				{
					CosmeticRippleData cosmeticRippleData = (parentNode.parentNode as CosmeticRippleRepresentation).pObj.data as CosmeticRippleData;
					if (iDstring == "Intensity")
					{
						cosmeticRippleData.intensity = nubPos;
						cosmeticRippleData.obj.intensity = nubPos;
					}
					if (iDstring == "FallOff")
					{
						cosmeticRippleData.fallOff = nubPos;
						cosmeticRippleData.obj.fallOff = nubPos;
					}
					if (iDstring == "DepthMix")
					{
						cosmeticRippleData.depthMix = nubPos;
						cosmeticRippleData.obj.depthMix = nubPos;
					}
					if (iDstring == "Squish")
					{
						cosmeticRippleData.squish = nubPos;
						cosmeticRippleData.obj.squish = nubPos;
					}
					parentNode.parentNode.Refresh();
					Refresh();
				}
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				_ = IDstring;
				CosmeticRippleData cosmeticRippleData = (parentNode.parentNode as CosmeticRippleRepresentation).pObj.data as CosmeticRippleData;
				if (IDstring == "Intensity")
				{
					num = cosmeticRippleData.intensity;
				}
				if (IDstring == "FallOff")
				{
					num = cosmeticRippleData.fallOff;
				}
				if (IDstring == "DepthMix")
				{
					num = cosmeticRippleData.depthMix;
				}
				if (IDstring == "Squish")
				{
					num = cosmeticRippleData.squish;
				}
				base.NumberText = (int)(num * 100f) + " ";
				RefreshNubPos(num);
			}
		}

		public CosmeticRipplePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Cosmetic Ripple")
		{
			CosmeticRippleData cosmeticRippleData = (base.parentNode as CosmeticRippleRepresentation).pObj.data as CosmeticRippleData;
			size.y = -15f;
			subNodes.Add(new CosmeticRippleSlider(owner, "Squish", this, new Vector2(5f, base.panelStep), "Squish: "));
			subNodes.Add(new CosmeticRippleSlider(owner, "DepthMix", this, new Vector2(5f, base.panelStep), "Depth mix: "));
			subNodes.Add(new CosmeticRippleSlider(owner, "FallOff", this, new Vector2(5f, base.panelStep), "FallOff: "));
			subNodes.Add(new CosmeticRippleSlider(owner, "Intensity", this, new Vector2(5f, base.panelStep), "Intensity: "));
			subNodes.Add(new Button(owner, "Inverted", this, new Vector2(5f, base.panelStep), 110f, "Is inverted: " + cosmeticRippleData.invert));
			subNodes.Add(new Button(owner, "Shape", this, new Vector2(125f, size.y), 110f, "Shape: " + (cosmeticRippleData.square ? "square" : "circle")));
			subNodes.Add(new Button(owner, "LeavesTrail", this, new Vector2(5f, base.panelStep), 110f, "Leaves trail: " + cosmeticRippleData.leavesTrail));
			size.y += 20f;
		}

		public override void Move(Vector2 newPos)
		{
			base.Move(newPos);
			parentNode.Refresh();
		}

		public override void Refresh()
		{
			base.Refresh();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			CosmeticRippleData cosmeticRippleData = (parentNode as CosmeticRippleRepresentation).pObj.data as CosmeticRippleData;
			if (sender.IDstring == "Inverted")
			{
				cosmeticRippleData.invert = !cosmeticRippleData.invert;
				cosmeticRippleData.obj.isInverted = cosmeticRippleData.invert;
				(sender as Button).Text = "Is inverted: " + cosmeticRippleData.invert;
			}
			if (sender.IDstring == "Shape")
			{
				cosmeticRippleData.square = !cosmeticRippleData.square;
				cosmeticRippleData.obj.square = cosmeticRippleData.square;
				(sender as Button).Text = "Shape: " + (cosmeticRippleData.square ? "square" : "circle");
			}
			if (sender.IDstring == "LeavesTrail")
			{
				cosmeticRippleData.leavesTrail = !cosmeticRippleData.leavesTrail;
				cosmeticRippleData.obj.leavesTrail = cosmeticRippleData.leavesTrail;
				(sender as Button).Text = "Leaves trail: " + cosmeticRippleData.leavesTrail;
			}
		}
	}

	public CosmeticRippleData cosmeticRippleData;

	public CosmeticRipple cosmeticRipple;

	public CosmeticRippleRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		cosmeticRippleData = pObj.data as CosmeticRippleData;
		cosmeticRipple = cosmeticRippleData.obj;
		(subNodes[subNodes.Count - 1] as Handle).pos = cosmeticRippleData.handlePos;
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
		subNodes.Add(new CosmeticRipplePanel(owner, "Cosmetic_Ripple_Control_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as CosmeticRipplePanel).pos = cosmeticRippleData.panelPos;
		if (cosmeticRippleData.obj == null)
		{
			cosmeticRippleData.obj = new CosmeticRipple(base.pObj);
			owner.room.AddObject(cosmeticRippleData.obj);
		}
		cosmeticRipple = cosmeticRippleData.obj;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scale = (subNodes[0] as Handle).pos.magnitude / 8f;
		fSprites[1].alpha = 2f / (subNodes[0] as Handle).pos.magnitude;
		MoveSprite(2, absPos);
		fSprites[2].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		cosmeticRippleData.handlePos = (subNodes[0] as Handle).pos;
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[1] as CosmeticRipplePanel).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as CosmeticRipplePanel).absPos);
		cosmeticRippleData.handlePos = (subNodes[0] as Handle).pos;
		cosmeticRippleData.panelPos = (subNodes[1] as Panel).pos;
		cosmeticRippleData.pos = pObj.pos;
		cosmeticRippleData.scale = (subNodes[0] as Handle).pos.magnitude;
		cosmeticRipple.pos = pObj.pos;
		cosmeticRipple.scale = cosmeticRippleData.scale;
		cosmeticRipple.UpdateRotation();
		cosmeticRipple.squish = cosmeticRippleData.squish;
		cosmeticRipple.leavesTrail = cosmeticRippleData.leavesTrail;
	}
}
