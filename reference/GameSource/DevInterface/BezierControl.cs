using RWCustom;
using UnityEngine;

namespace DevInterface;

public class BezierControl : PositionedDevUINode
{
	public BezierSpline syncedSpline;

	public Handle posANode;

	public Handle posBNode;

	public Handle handleANode;

	public Handle handleBNode;

	public BezierMesh bezierMesh;

	public Vector2 posA
	{
		get
		{
			return posANode?.pos ?? pos;
		}
		set
		{
			if (posANode != null)
			{
				posANode.Move(value);
			}
			else
			{
				pos = value;
			}
		}
	}

	public Vector2 posB
	{
		get
		{
			return posBNode.pos + pos;
		}
		set
		{
			posBNode.Move(value - pos);
		}
	}

	public Vector2 handleA
	{
		get
		{
			return handleANode.pos + posA;
		}
		set
		{
			handleANode.Move(value - posA);
		}
	}

	public Vector2 handleB
	{
		get
		{
			return handleBNode.pos + posB;
		}
		set
		{
			handleBNode.Move(value - posB);
		}
	}

	public BezierControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 posB, Vector2 handleA, Vector2 handleB)
		: this(owner, IDstring, parentNode, pos, new BezierCurve(Vector2.zero, handleA, posB, handleB))
	{
	}

	public BezierControl(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, BezierCurve bezier, bool originHandle = false)
		: base(owner, IDstring, parentNode, pos)
	{
		if (originHandle)
		{
			posANode = new Handle(owner, "posA", this, bezier.posA);
			subNodes.Add(posANode);
		}
		posBNode = new Handle(owner, "posB", this, bezier.posB - pos);
		posBNode.pos = bezier.posB - pos;
		subNodes.Add(posBNode);
		handleANode = new Handle(owner, "handleA", originHandle ? ((PositionedDevUINode)posANode) : ((PositionedDevUINode)this), bezier.handleA - bezier.posA);
		handleANode.pos = bezier.handleA - bezier.posA;
		subNodes.Add(handleANode);
		handleBNode = new Handle(owner, "handleB", posBNode, bezier.handleB - bezier.posB);
		handleBNode.pos = bezier.handleB - bezier.posB;
		subNodes.Add(handleBNode);
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[0]);
		fSprites[0].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].anchorY = 0f;
		bezierMesh = new BezierMesh(20, customColor: false, pointyTip: false);
		fSprites.Add(bezierMesh);
		owner.placedObjectsContainer.AddChild(bezierMesh);
	}

	public override void Update()
	{
		base.Update();
		if (posBNode.dragged || handleANode.dragged || handleBNode.dragged || (posANode != null && posANode.dragged))
		{
			Refresh();
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		Vector2 vector = ((posANode != null) ? posANode.absPos : absPos);
		MoveSprite(0, vector);
		fSprites[0].scaleY = handleANode.pos.magnitude;
		fSprites[0].rotation = Custom.AimFromOneVectorToAnother(vector, handleANode.absPos);
		MoveSprite(1, posBNode.absPos);
		fSprites[1].scaleY = handleBNode.pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(posBNode.absPos, handleBNode.absPos);
		bezierMesh.handleA = handleA - posA;
		bezierMesh.handleB = handleB - posB;
		bezierMesh.posB = posBNode.absPos - vector;
		MoveSprite(2, vector);
		bezierMesh.ChangeSegments((int)(bezierMesh.length / 5f));
	}
}
