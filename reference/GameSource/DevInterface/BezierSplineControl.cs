using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class BezierSplineControl : BezierControl
{
	public class MidpointHandle : Handle
	{
		public float _perpendicularDirection;

		public Handle handle1;

		public Handle handle2;

		public bool anyDragged
		{
			get
			{
				if (!handle1.dragged && !handle2.dragged)
				{
					return dragged;
				}
				return true;
			}
		}

		public Vector2 dir1
		{
			get
			{
				return Custom.DegToVec(PerpendicularAngle + 90f) * handle1.pos.magnitude;
			}
			set
			{
				handle1.pos = value;
				PerpendicularAngle = Custom.VecToDeg(handle1.pos) - 90f;
			}
		}

		public Vector2 dir2
		{
			get
			{
				return Custom.DegToVec(PerpendicularAngle - 90f) * handle2.pos.magnitude;
			}
			set
			{
				handle2.pos = value;
				PerpendicularAngle = Custom.VecToDeg(handle2.pos) + 90f;
			}
		}

		public float PerpendicularAngle
		{
			get
			{
				return _perpendicularDirection;
			}
			set
			{
				_perpendicularDirection = value;
				Refresh();
			}
		}

		public BezierSpline.Midpoint GetSplineMidpoint => new BezierSpline.Midpoint(pos, PerpendicularAngle, dir1.magnitude, dir2.magnitude);

		public MidpointHandle(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos)
		{
			handle1 = new Handle(owner, IDstring + "_1", this, pos);
			handle1.fSprites[0].scale = 0.4f;
			subNodes.Add(handle1);
			handle2 = new Handle(owner, IDstring + "_2", this, pos);
			subNodes.Add(handle2);
			handle2.fSprites[0].scale = 0.4f;
			fSprites.Add(new FSprite("pixel"));
			owner.placedObjectsContainer.AddChild(fSprites[1]);
			fSprites[1].anchorY = 0f;
			fSprites.Add(new FSprite("pixel"));
			owner.placedObjectsContainer.AddChild(fSprites[2]);
			fSprites[2].anchorY = 0f;
		}

		public override void Update()
		{
			base.Update();
			if (handle1.dragged)
			{
				PerpendicularAngle = Custom.VecToDeg(handle1.pos) - 90f;
			}
			if (handle2.dragged)
			{
				PerpendicularAngle = Custom.VecToDeg(handle2.pos) + 90f;
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			handle1.Move(dir1);
			handle2.Move(dir2);
			MoveSprite(1, absPos);
			fSprites[1].scaleY = handle1.pos.magnitude;
			fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, handle1.absPos);
			MoveSprite(2, absPos);
			fSprites[2].scaleY = handle2.pos.magnitude;
			fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, handle2.absPos);
		}
	}

	public bool _alterred;

	public bool lastAlterred;

	public List<BezierMesh> extraBeziers = new List<BezierMesh>();

	public List<MidpointHandle> midpoints = new List<MidpointHandle>();

	public FSprite ghostSprite;

	public bool Alterred
	{
		get
		{
			if (_alterred)
			{
				return lastAlterred;
			}
			return false;
		}
		set
		{
			_alterred = value;
			lastAlterred = false;
		}
	}

	public int Segments => midpoints.Count + 1;

	public BezierSplineControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 posB, Vector2 handleA, Vector2 handleB)
		: base(owner, IDstring, parentNode, pos, posB, handleA, handleB)
	{
		ghostSprite = new FSprite("Circle20");
		ghostSprite.scale = 0.5f;
		ghostSprite.alpha = 0f;
		ghostSprite.color = Color.cyan;
		fSprites.Add(ghostSprite);
		owner?.placedObjectsContainer.AddChild(ghostSprite);
	}

	public BezierSplineControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, BezierCurve bezier, bool originHandle = false)
		: base(owner, IDstring, parentNode, pos, bezier, originHandle)
	{
		ghostSprite = new FSprite("Circle20");
		ghostSprite.scale = 0.5f;
		ghostSprite.alpha = 0f;
		ghostSprite.color = Color.cyan;
		fSprites.Add(ghostSprite);
		owner?.placedObjectsContainer.AddChild(ghostSprite);
	}

	public BezierSplineControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, BezierSpline syncedSpline, bool originHandle = false)
		: this(owner, IDstring, parentNode, pos, new BezierCurve(syncedSpline.posA, syncedSpline.handleA, syncedSpline.posB, syncedSpline.handleB), originHandle)
	{
		base.syncedSpline = syncedSpline;
		for (int i = 0; i < syncedSpline.midpoints.Count; i++)
		{
			AddMidpoint(i, syncedSpline.midpoints[i]);
		}
	}

	public override void Update()
	{
		if (_alterred && !lastAlterred)
		{
			lastAlterred = true;
		}
		else if (Alterred)
		{
			Alterred = false;
		}
		bool flag = Input.GetKey(KeyCode.LeftControl) || (Input.GetKey(KeyCode.RightControl) && owner.draggedNode == null);
		base.Update();
		if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !flag && owner.draggedNode == null)
		{
			float num = float.MaxValue;
			Vector2 position = Vector2.zero;
			int segment = 0;
			float posOnSegment = 0f;
			for (int i = 0; i < extraBeziers.Count + 1; i++)
			{
				BezierMesh bezierMesh = ((i == 0) ? base.bezierMesh : extraBeziers[i - 1]);
				for (int j = 0; j < bezierMesh.segments; j++)
				{
					float num2 = (float)(j + 1) / (float)bezierMesh.segments;
					Vector2 position2 = bezierMesh.GetPosition();
					Vector2 vector = Custom.Bezier(position2, position2 + bezierMesh.handleA, position2 + bezierMesh.posB, position2 + bezierMesh.posB + bezierMesh.handleB, num2);
					float num3 = Vector2.Distance(owner.mousePos, vector);
					if (num3 < num && num3 < 10f)
					{
						position = vector;
						num = num3;
						segment = i;
						posOnSegment = num2;
					}
				}
			}
			if (num < 10f)
			{
				ghostSprite.alpha = 0.5f;
				ghostSprite.SetPosition(position);
				if (owner.mouseClick)
				{
					SplitSegment(segment, posOnSegment);
				}
			}
			else
			{
				ghostSprite.alpha = 0f;
			}
		}
		else if (ghostSprite.alpha > 0f)
		{
			ghostSprite.alpha = 0f;
		}
		for (int k = 0; k < midpoints.Count; k++)
		{
			if (flag && midpoints[k].MouseOver)
			{
				midpoints[k].SetColor(new Color(1f, 0.5f, 0f));
				if (owner.mouseClick)
				{
					JoinSegments(k);
					break;
				}
			}
			if (midpoints[k].anyDragged)
			{
				if (!lastAlterred)
				{
					Alterred = true;
				}
				if (syncedSpline != null)
				{
					syncedSpline.SetBezier(k, GetBezier(k));
					syncedSpline.SetBezier(k + 1, GetBezier(k + 1));
				}
				Refresh();
				break;
			}
		}
		if ((posANode != null && posANode.dragged) || handleANode.dragged)
		{
			if (!lastAlterred)
			{
				Alterred = true;
			}
			syncedSpline?.SetBezier(0, GetBezier(0));
		}
		if (posBNode.dragged || handleBNode.dragged)
		{
			if (!lastAlterred)
			{
				Alterred = true;
			}
			syncedSpline?.SetBezier(Segments - 1, GetBezier(Segments - 1));
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		Vector2 vector = posANode?.absPos ?? absPos;
		for (int i = 0; i < midpoints.Count; i++)
		{
			MidpointHandle midpointHandle = midpoints[i];
			if (i == 0)
			{
				bezierMesh.posB = midpointHandle.absPos - vector;
				bezierMesh.handleB = midpointHandle.dir1;
			}
			else
			{
				extraBeziers[i - 1].posB = midpointHandle.absPos - midpoints[i - 1].absPos;
				extraBeziers[i - 1].handleB = midpointHandle.dir1;
			}
			extraBeziers[i].SetPosition(midpointHandle.absPos);
			extraBeziers[i].handleA = midpointHandle.dir2;
			extraBeziers[i].ChangeSegments((int)(extraBeziers[i].length / 5f));
		}
		if (extraBeziers.Count > 0)
		{
			extraBeziers[extraBeziers.Count - 1].posB = posBNode.absPos - midpoints[midpoints.Count - 1].absPos;
			extraBeziers[extraBeziers.Count - 1].handleB = handleBNode.pos;
		}
	}

	public virtual void AddMidpoint(int segment, BezierSpline.Midpoint midpoint)
	{
		MidpointHandle item = new MidpointHandle(owner, "MidpointHandle", this, midpoint.pos)
		{
			dir1 = midpoint.dir1,
			dir2 = midpoint.dir2
		};
		midpoints.Insert(segment, item);
		subNodes.Add(item);
		BezierMesh bezierMesh = new BezierMesh(10, customColor: false, pointyTip: false);
		extraBeziers.Insert(segment, bezierMesh);
		fSprites.Add(bezierMesh);
		owner.placedObjectsContainer.AddChild(bezierMesh);
		Alterred = true;
		Refresh();
	}

	public virtual void SplitSegment(int segment, float posOnSegment)
	{
		segment = Mathf.Clamp(segment, 0, midpoints.Count);
		posOnSegment = Mathf.Clamp(posOnSegment, 0f, 1f);
		GetBezier(segment).Split(posOnSegment, out var bezier, out var bezier2);
		BezierMesh bezierMesh = new BezierMesh(10, customColor: false, pointyTip: false);
		extraBeziers.Insert(segment, bezierMesh);
		fSprites.Add(bezierMesh);
		owner.placedObjectsContainer.AddChild(bezierMesh);
		MidpointHandle midpointHandle = new MidpointHandle(owner, "MidpointHandle", this, Vector2.zero);
		subNodes.Add(midpointHandle);
		midpoints.Insert(segment, midpointHandle);
		if (syncedSpline != null)
		{
			syncedSpline.AddMidpoint(midpointHandle.GetSplineMidpoint, segment);
			syncedSpline.SetBezier(segment, bezier);
			syncedSpline.SetBezier(segment + 1, bezier2);
		}
		SetBezier(segment, bezier);
		SetBezier(segment + 1, bezier2);
		Alterred = true;
		Refresh();
	}

	public virtual void JoinSegments(int segment)
	{
		if (0 <= segment && segment <= midpoints.Count - 1)
		{
			MidpointHandle midpointHandle = midpoints[segment];
			float num = ((segment == 0) ? base.bezierMesh.length : extraBeziers[segment - 1].length);
			float length = extraBeziers[segment].length;
			float num2 = num + length;
			if (segment == 0)
			{
				handleANode.pos *= num2 / num;
			}
			else
			{
				midpoints[segment - 1].dir2 *= Mathf.Min(num2 / num, 2f);
			}
			if (segment == midpoints.Count - 1)
			{
				handleBNode.pos *= num2 / length;
			}
			else
			{
				midpoints[segment + 1].dir1 *= Mathf.Min(num2 / length, 2f);
			}
			subNodes.Remove(midpointHandle);
			midpoints.Remove(midpointHandle);
			midpointHandle.ClearSprites();
			BezierMesh bezierMesh = extraBeziers[segment];
			bezierMesh.RemoveFromContainer();
			extraBeziers.Remove(bezierMesh);
			if (syncedSpline != null)
			{
				syncedSpline.RemoveMidpoint(segment);
				syncedSpline.SetBezier(segment, GetBezier(segment));
			}
			Alterred = true;
			Refresh();
		}
	}

	public BezierCurve GetBezier(int segment)
	{
		BezierCurve result = default(BezierCurve);
		if (segment == 0)
		{
			result.posA = base.posA;
			result.handleA = base.handleA;
		}
		else
		{
			result.posA = pos + midpoints[segment - 1].pos;
			result.handleA = result.posA + midpoints[segment - 1].dir2;
		}
		if (segment == midpoints.Count)
		{
			result.posB = base.posB;
			result.handleB = base.handleB;
		}
		else
		{
			result.posB = pos + midpoints[segment].pos;
			result.handleB = result.posB + midpoints[segment].dir1;
		}
		return result;
	}

	public void SetBezier(int segment, BezierCurve bezier)
	{
		_ = base.posA - pos;
		if (segment == 0)
		{
			if (posANode != null)
			{
				base.posA = bezier.posA;
			}
			base.handleA = bezier.handleA;
		}
		else
		{
			midpoints[segment - 1].pos = bezier.posA - pos;
			midpoints[segment - 1].dir2 = bezier.handleA - bezier.posA;
		}
		if (segment == midpoints.Count)
		{
			base.posB = bezier.posB;
			base.handleB = bezier.handleB;
		}
		else
		{
			midpoints[segment].pos = bezier.posB - pos;
			midpoints[segment].dir1 = bezier.handleB - bezier.posB;
		}
		if (syncedSpline != null && syncedSpline.Segments == Segments)
		{
			syncedSpline.SetBezier(segment, bezier);
		}
		Alterred = true;
		Refresh();
	}

	public float GetBezierLength(int segment)
	{
		if (segment == 0)
		{
			return bezierMesh.length;
		}
		return extraBeziers[segment - 1].length;
	}
}
