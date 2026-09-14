using System;
using System.Collections.Generic;
using System.Linq;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class FloatingDebrisRepresentation : PlacedObjectRepresentation
{
	public class FloatingDebrisPanel : Panel, IDevUISignals
	{
		public class EmptyNode : DevUINode
		{
			public EmptyNode(DevUI owner, string IDstring, DevUINode parentNode)
				: base(owner, IDstring, parentNode)
			{
			}
		}

		public class FloatingDebrisSlider : Slider
		{
			public string defaultTitle;

			public FloatingDebrisSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
				defaultTitle = title;
			}

			public void ChangeTitle(string text)
			{
				if (text == null)
				{
					text = defaultTitle;
				}
				(subNodes[0] as DevUILabel).Text = text;
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring == null)
				{
					return;
				}
				FloatingDebrisData data = (parentNode.parentNode as FloatingDebrisRepresentation).data;
				switch (iDstring)
				{
				case "Amount":
					if (data.numberOfFloaters != (int)(nubPos * 100f))
					{
						data.numberOfFloaters = (int)(nubPos * 100f);
						data.obj.numberOfFloaters = (int)(nubPos * 100f);
						data.obj.Init();
					}
					break;
				case "DepthNear":
					data.depthNear = (int)(nubPos * 30f);
					data.obj.depthNear = (int)(nubPos * 30f);
					data.obj.UpdateSeededRandomValues();
					data.obj.RefreshFloaters();
					break;
				case "DepthFar":
					data.depthFar = (int)(nubPos * 30f);
					data.obj.depthFar = (int)(nubPos * 30f);
					data.obj.UpdateSeededRandomValues();
					data.obj.RefreshFloaters();
					break;
				case "ScaleMin":
					data.scaleMinimum = nubPos * 3f;
					data.obj.scaleMinimum = nubPos * 3f;
					data.obj.UpdateSeededRandomValues();
					data.obj.RefreshFloaters();
					break;
				case "ScaleMax":
					data.scaleMaximum = nubPos * 3f;
					data.obj.scaleMaximum = nubPos * 3f;
					data.obj.UpdateSeededRandomValues();
					data.obj.RefreshFloaters();
					break;
				case "MovementSpeed":
					data.movement = nubPos;
					data.obj.movementAmount = nubPos;
					data.obj.RefreshFloaters();
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}

			public override void Refresh()
			{
				base.Refresh();
				float nubPos = 0f;
				_ = IDstring;
				FloatingDebrisData data = (parentNode.parentNode as FloatingDebrisRepresentation).data;
				if (IDstring == "Amount")
				{
					nubPos = data.numberOfFloaters;
					base.NumberText = nubPos + "%";
					nubPos /= 100f;
				}
				else if (IDstring == "DepthNear")
				{
					nubPos = data.depthNear;
					base.NumberText = nubPos + " ";
					nubPos /= 30f;
				}
				else if (IDstring == "DepthFar")
				{
					nubPos = data.depthFar;
					base.NumberText = nubPos + " ";
					nubPos /= 30f;
				}
				else if (IDstring == "ScaleMin")
				{
					nubPos = data.scaleMinimum;
					base.NumberText = nubPos + " ";
					nubPos /= 3f;
				}
				else if (IDstring == "ScaleMax")
				{
					nubPos = data.scaleMaximum;
					base.NumberText = nubPos + " ";
					nubPos /= 3f;
				}
				else if (IDstring == "MovementSpeed")
				{
					nubPos = data.movement;
					base.NumberText = nubPos + " ";
				}
				RefreshNubPos(nubPos);
			}
		}

		public CustomDecalRepresentation.SelectDecalPanel floaterTypeSelectPannel;

		public bool slidersNeedRefresh;

		public FloatingDebrisPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 45f), "Floating Debris")
		{
			size.y = -15f;
			subNodes.Add(new Button(owner, "NewSeed", this, new Vector2(5f, base.panelStep), 110f, "New Seed"));
			subNodes.Add(new Button(owner, "Type", this, new Vector2(125f, size.y), 110f, "Select Type"));
			subNodes.Add(new Button(owner, "RemoveLeft", this, new Vector2(5f, base.panelStep), 110f, "Remove left"));
			subNodes.Add(new Button(owner, "RemoveRight", this, new Vector2(125f, size.y), 110f, "Remove right"));
			subNodes.Add(new Button(owner, "AddLeft", this, new Vector2(5f, base.panelStep), 110f, "Add left"));
			subNodes.Add(new Button(owner, "AddRight", this, new Vector2(125f, size.y), 110f, "Add right"));
			size.y += 20f;
			AddSliders();
		}

		public void AddSliders()
		{
			size.y -= 20f;
			int num = 6;
			while (num < subNodes.Count)
			{
				DevUINode devUINode = subNodes.Pop();
				if (devUINode is FloatingDebrisSlider)
				{
					size.y -= 20f;
				}
				devUINode.ClearSprites();
			}
			string name = (base.Title = "Floating Debris (" + (parentNode as FloatingDebrisRepresentation).data.type + ")");
			(parentNode as FloatingDebrisRepresentation).Name = name;
			Func<EmptyNode> func = () => new EmptyNode(owner, "Hidden", this);
			FloatingDebris.UIText uiText = (parentNode as FloatingDebrisRepresentation).floatingDebris.uiText;
			if (uiText == null || uiText.movementAmount?.hide != true)
			{
				subNodes.Add(new FloatingDebrisSlider(owner, "MovementSpeed", this, new Vector2(5f, base.panelStep), uiText?.movementAmount?.title ?? "Movement Amount:"));
			}
			else
			{
				subNodes.Add(func());
			}
			if (uiText == null || uiText.scaleMax?.hide != true)
			{
				subNodes.Add(new FloatingDebrisSlider(owner, "ScaleMax", this, new Vector2(5f, base.panelStep), uiText?.scaleMax?.title ?? "Scale Max: "));
			}
			else
			{
				subNodes.Add(func());
			}
			if (uiText == null || uiText.scaleMin?.hide != true)
			{
				subNodes.Add(new FloatingDebrisSlider(owner, "ScaleMin", this, new Vector2(5f, base.panelStep), uiText?.scaleMin?.title ?? "Scale Min: "));
			}
			else
			{
				subNodes.Add(func());
			}
			if (uiText == null || uiText.amount?.hide != true)
			{
				subNodes.Add(new FloatingDebrisSlider(owner, "Amount", this, new Vector2(5f, base.panelStep), uiText?.amount?.title ?? "Amount: "));
			}
			else
			{
				subNodes.Add(func());
			}
			if (uiText == null || uiText.depthNear?.hide != true)
			{
				subNodes.Add(new FloatingDebrisSlider(owner, "DepthNear", this, new Vector2(5f, base.panelStep), uiText?.depthNear?.title ?? "Depth, Near: "));
			}
			else
			{
				subNodes.Add(func());
			}
			if (uiText == null || uiText.depthFar?.hide != true)
			{
				subNodes.Add(new FloatingDebrisSlider(owner, "DepthFar", this, new Vector2(5f, base.panelStep), uiText?.depthFar?.title ?? "Depth, Far: "));
			}
			else
			{
				subNodes.Add(func());
			}
			size.y += 20f;
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			FloatingDebrisRepresentation floatingDebrisRepresentation = parentNode as FloatingDebrisRepresentation;
			FloatingDebrisData data = floatingDebrisRepresentation.data;
			if (sender.IDstring == "AddLeft")
			{
				data.AddControlPointLeft();
				floatingDebrisRepresentation.AddControlPoint();
			}
			else if (sender.IDstring == "AddRight")
			{
				data.AddControlPointRight();
				floatingDebrisRepresentation.AddControlPoint();
			}
			else if (sender.IDstring == "RemoveLeft")
			{
				data.RemoveControlPointLeft();
				floatingDebrisRepresentation.RemoveControlPoint();
			}
			else if (sender.IDstring == "RemoveRight")
			{
				data.RemoveControlPointRight();
				floatingDebrisRepresentation.RemoveControlPoint();
			}
			else if (sender.IDstring == "NewSeed")
			{
				int seed = (data.seed = UnityEngine.Random.Range(0, int.MaxValue));
				data.obj.seed = seed;
				data.obj.RemoveAllFloaters();
				data.obj.Init();
			}
			else if (sender.IDstring == "Type")
			{
				if (floaterTypeSelectPannel != null)
				{
					HideFloaterTypeSelectPannel();
					return;
				}
				floaterTypeSelectPannel = new CustomDecalRepresentation.SelectDecalPanel(owner, this, new Vector2(200f, 15f) - absPos, FloatingDebris.types.Keys.ToArray());
				subNodes.Add(floaterTypeSelectPannel);
			}
			else if (sender.IDstring == "BackPage99289..?/~")
			{
				if (sender.parentNode == floaterTypeSelectPannel)
				{
					floaterTypeSelectPannel.PrevPage();
				}
			}
			else if (sender.IDstring == "NextPage99289..?/~")
			{
				if (sender.parentNode == floaterTypeSelectPannel)
				{
					floaterTypeSelectPannel.NextPage();
				}
			}
			else if (sender.parentNode == floaterTypeSelectPannel)
			{
				HideFloaterTypeSelectPannel();
				data.type = sender.IDstring;
				data.obj.type = sender.IDstring;
				AddSliders();
				(parentNode as FloatingDebrisRepresentation).UpdateNames();
			}
		}

		public void HideFloaterTypeSelectPannel()
		{
			if (floaterTypeSelectPannel != null)
			{
				subNodes.Remove(floaterTypeSelectPannel);
				floaterTypeSelectPannel.ClearSprites();
				floaterTypeSelectPannel = null;
			}
		}
	}

	public FloatingDebris floatingDebris;

	public FloatingDebrisData data;

	public List<Handle> controlPoints = new List<Handle>();

	public FloatingDebrisRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "FloatingDebris")
	{
		data = pObj.data as FloatingDebrisData;
		if (data.obj == null)
		{
			data.seed = UnityEngine.Random.Range(0, int.MaxValue);
			data.obj = FloatingDebris.TrySpawnFloatingDebris(owner.room, base.pObj);
			if (data.obj == null)
			{
				return;
			}
			owner.room.AddObject(data.obj);
		}
		floatingDebris = data.obj;
		FloatingDebrisPanel floatingDebrisPanel = new FloatingDebrisPanel(owner, "FloatingDebrisPanel", this, new Vector2(0f, 100f));
		subNodes.Add(floatingDebrisPanel);
		floatingDebrisPanel.pos = data.panelPos;
		FSprite fSprite = new FSprite("pixel")
		{
			anchorY = 0f
		};
		fSprites.Add(fSprite);
		owner.placedObjectsContainer.AddChild(fSprite);
		for (int i = 0; i < data.controlPointOffsetAmount.Count; i++)
		{
			AddControlPoint();
		}
		UpdateNames();
	}

	public override void Update()
	{
		if (data.obj == null)
		{
			ClearSprites();
			parentNode.subNodes.Remove(this);
		}
		else
		{
			base.Update();
		}
	}

	public override void Refresh()
	{
		if (data.obj == null)
		{
			return;
		}
		base.Refresh();
		bool flag = false;
		MoveSprite(1, absPos);
		Panel panel = subNodes[0] as FloatingDebrisPanel;
		data.panelPos = panel.pos;
		fSprites[1].scaleY = (data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero)).magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, absPos + data.panelPos + (panel.collapsed ? (Vector2.up * panel.size.y) : Vector2.zero));
		for (int i = 0; i < controlPoints.Count; i++)
		{
			if (i > data.controlPointPosX.Count)
			{
				return;
			}
			Handle handle = controlPoints[i];
			Handle handle2 = handle.subNodes[0] as Handle;
			Handle handle3 = handle.subNodes[1] as Handle;
			Handle handle4 = handle.subNodes[2] as Handle;
			Handle handle5 = handle.subNodes[3] as Handle;
			Handle handle6 = handle.subNodes[4] as Handle;
			Handle handle7 = handle.subNodes[5] as Handle;
			Handle handle8 = handle.subNodes[6] as Handle;
			float num = 0.01f;
			if (handle2.pos.x != 0f || handle2.dragged || handle3.dragged || handle.dragged || handle4.dragged || handle5.dragged || handle6.dragged || handle7.dragged || handle8.dragged)
			{
				data.controlPointPosX[i] = handle.pos.x;
				data.controlPointPosY[i] = handle.pos.y;
				data.controlPointOffsetAmount[i] = handle2.pos.y * num;
				data.controlPointDepthOffset[i] = handle3.pos.x * num;
				data.controlPointScaleOffset[i] = handle4.pos.x * num;
				data.controlPointExtraOffset[i] = new Vector4(handle5.pos.x * num, handle6.pos.x * num, handle7.pos.x * num, handle8.pos.x * num);
				flag = true;
			}
			handle.MoveSprite(1, handle.absPos);
			if (i < controlPoints.Count - 1)
			{
				handle.fSprites[1].scaleY = Vector2.Distance(handle.pos, controlPoints[i + 1].pos);
				handle.fSprites[1].rotation = Custom.AimFromOneVectorToAnother(handle.pos, controlPoints[i + 1].pos);
			}
			else
			{
				handle.fSprites[1].scaleY = 0f;
			}
		}
		if (dragged)
		{
			floatingDebris.pos = pObj.pos;
			flag = true;
		}
		if (flag)
		{
			RefreshObject();
		}
	}

	public void RefreshObject()
	{
		floatingDebris.UpdateControlPointInfluence();
		floatingDebris.UpdatePositions();
		floatingDebris.UpdateOffsetAmounts();
		floatingDebris.UpdateDepthOffsets();
		floatingDebris.UpdateScaleOffsets();
		floatingDebris.UpdateExtraOffsets();
		floatingDebris.RefreshFloaters();
	}

	public void AddControlPoint()
	{
		if (controlPoints.Count < data.controlPointPosX.Count)
		{
			Handle handle = new Handle(owner, "Control_Point", this, Vector2.zero);
			controlPoints.Add(handle);
			subNodes.Add(handle);
			FSprite fSprite = new FSprite("pixel");
			fSprite.anchorY = 0f;
			handle.fSprites.Add(fSprite);
			owner.placedObjectsContainer.AddChild(fSprite);
			AxisHandle item = new AxisHandle(owner, "vertical_handle", handle, Vector2.up * 100f, Vector2.zero, AxisHandle.Axis.Y, "Scatter");
			handle.subNodes.Add(item);
			float num = 0f;
			AxisHandle item2 = new AxisHandle(owner, "horizontal_handle", handle, Vector2.zero, Vector2.down * 30f * (num += 1f), AxisHandle.Axis.X, "Depth Offset");
			handle.subNodes.Add(item2);
			AxisHandle item3 = new AxisHandle(owner, "horizontal_handle", handle, Vector2.zero, Vector2.down * 30f * (num += 1f), AxisHandle.Axis.X, "Scale Offset");
			handle.subNodes.Add(item3);
			AxisHandle item4 = new AxisHandle(owner, "horizontal_handle", handle, Vector2.zero, Vector2.down * 30f * (num += 1f), AxisHandle.Axis.X, "Extra Offset 1");
			handle.subNodes.Add(item4);
			AxisHandle item5 = new AxisHandle(owner, "horizontal_handle", handle, Vector2.zero, Vector2.down * 30f * (num += 1f), AxisHandle.Axis.X, "Extra Offset 2");
			handle.subNodes.Add(item5);
			AxisHandle item6 = new AxisHandle(owner, "horizontal_handle", handle, Vector2.zero, Vector2.down * 30f * (num += 1f), AxisHandle.Axis.X, "Extra Offset 3");
			handle.subNodes.Add(item6);
			AxisHandle item7 = new AxisHandle(owner, "horizontal_handle", handle, Vector2.zero, Vector2.down * 30f * (num += 1f), AxisHandle.Axis.X, "Extra Offset 4");
			handle.subNodes.Add(item7);
			UpdateNames();
			RepositionHandles();
			RefreshObject();
		}
	}

	public void RemoveControlPoint()
	{
		int count = controlPoints.Count;
		if (count > 1)
		{
			controlPoints[count - 1].ClearSprites();
			subNodes.Pop();
			controlPoints.Pop();
			RepositionHandles();
			RefreshObject();
		}
	}

	public void RepositionHandles()
	{
		for (int i = 0; i < controlPoints.Count; i++)
		{
			controlPoints[i].Move(new Vector2(data.controlPointPosX[i], data.controlPointPosY[i]));
			(controlPoints[i].subNodes[0] as Handle).pos.y = data.controlPointOffsetAmount[i] * 100f;
			(controlPoints[i].subNodes[1] as Handle).pos.x = data.controlPointDepthOffset[i] * 100f;
			(controlPoints[i].subNodes[2] as Handle).pos.x = data.controlPointScaleOffset[i] * 100f;
			(controlPoints[i].subNodes[3] as Handle).pos.x = data.controlPointExtraOffset[i].x * 100f;
			(controlPoints[i].subNodes[4] as Handle).pos.x = data.controlPointExtraOffset[i].y * 100f;
			(controlPoints[i].subNodes[5] as Handle).pos.x = data.controlPointExtraOffset[i].z * 100f;
			(controlPoints[i].subNodes[6] as Handle).pos.x = data.controlPointExtraOffset[i].w * 100f;
		}
	}

	public void UpdateNames()
	{
		_ = subNodes[0];
		FloatingDebris.UIText uiText = floatingDebris.uiText;
		for (int i = 0; i < controlPoints.Count; i++)
		{
			(controlPoints[i].subNodes[0] as AxisHandle).SetName(uiText?.scatter?.title);
			(controlPoints[i].subNodes[1] as AxisHandle).SetName(uiText?.depthOffset?.title);
			(controlPoints[i].subNodes[2] as AxisHandle).SetName(uiText?.scaleOffset?.title);
			(controlPoints[i].subNodes[3] as AxisHandle).SetName(uiText?.extraSlider1?.title);
			(controlPoints[i].subNodes[3] as AxisHandle).SetVisibility(uiText?.extraSlider1?.hide ?? true);
			(controlPoints[i].subNodes[4] as AxisHandle).SetName(uiText?.extraSlider2?.title);
			(controlPoints[i].subNodes[4] as AxisHandle).SetVisibility(uiText?.extraSlider2?.hide ?? true);
			(controlPoints[i].subNodes[5] as AxisHandle).SetName(uiText?.extraSlider3?.title);
			(controlPoints[i].subNodes[5] as AxisHandle).SetVisibility(uiText?.extraSlider3?.hide ?? true);
			(controlPoints[i].subNodes[6] as AxisHandle).SetName(uiText?.extraSlider4?.title);
			(controlPoints[i].subNodes[6] as AxisHandle).SetVisibility(uiText?.extraSlider4?.hide ?? true);
		}
	}
}
