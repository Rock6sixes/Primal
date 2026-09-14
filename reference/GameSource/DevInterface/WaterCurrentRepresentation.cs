using RWCustom;
using UnityEngine;

namespace DevInterface;

public class WaterCurrentRepresentation : PlacedObjectRepresentation
{
	public int firstRectSprite;

	public int lineSprite;

	public Handle endHandle;

	public Handle widthHandle;

	public Handle velocityHandle;

	public WaterCurrent.WaterCurrentData Data => (WaterCurrent.WaterCurrentData)pObj.data;

	public WaterCurrentRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		firstRectSprite = fSprites.Count;
		for (int i = 0; i < 4; i++)
		{
			fSprites.Add(new FSprite("pixel")
			{
				anchorX = 0f,
				anchorY = 0f
			});
			owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		}
		lineSprite = fSprites.Count;
		fSprites.Add(new FSprite("pixel")
		{
			anchorX = 0.5f,
			anchorY = 0f
		});
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		endHandle = new Handle(owner, "Current_End", this, Data.handlePos);
		subNodes.Add(endHandle);
		widthHandle = new Handle(owner, "Current_Side", this, Data.width / 2f * Custom.PerpendicularVector(Data.handlePos));
		subNodes.Add(widthHandle);
		velocityHandle = new Handle(owner, "Current_Velocity", this, Data.handlePos / 2f + Data.handlePos.normalized * Data.velocity);
		subNodes.Add(velocityHandle);
	}

	public override void Refresh()
	{
		base.Refresh();
		bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		Data.velocity = Vector2.Dot(velocityHandle.pos - Data.handlePos / 2f, Data.handlePos.normalized);
		if (endHandle.pos != Data.handlePos && flag)
		{
			Vector2 rhs = endHandle.pos;
			float num = Mathf.Abs(rhs.x);
			float num2 = Mathf.Abs(rhs.y);
			if (num * 0.4142f < num2 && num2 * 0.4142f < num)
			{
				Vector2 normalized = new Vector2(Mathf.Sign(rhs.x), Mathf.Sign(rhs.y)).normalized;
				rhs = normalized * Vector2.Dot(normalized, rhs);
			}
			else if (num < num2)
			{
				rhs.x = 0f;
			}
			else
			{
				rhs.y = 0f;
			}
			endHandle.pos = rhs;
		}
		Data.handlePos = endHandle.pos;
		velocityHandle.pos = Data.handlePos / 2f + Data.handlePos.normalized * Data.velocity;
		Data.width = widthHandle.pos.magnitude * 2f;
		widthHandle.pos = Data.width / 2f * Custom.PerpendicularVector(Data.handlePos);
		Vector2 vector = Custom.PerpendicularVector(Data.handlePos.normalized);
		Vector2 vector2 = absPos;
		Vector2 vector3 = endHandle.absPos;
		MoveSprite(firstRectSprite, vector2 + Data.width / 2f * vector);
		fSprites[firstRectSprite].width = Data.width;
		MoveSprite(firstRectSprite + 1, vector3 + Data.width / 2f * vector);
		fSprites[firstRectSprite + 1].width = Data.width;
		MoveSprite(firstRectSprite + 2, vector2 - Data.width / 2f * vector);
		fSprites[firstRectSprite + 2].height = Data.handlePos.magnitude;
		MoveSprite(firstRectSprite + 3, vector2 + Data.width / 2f * vector);
		fSprites[firstRectSprite + 3].height = Data.handlePos.magnitude;
		for (int i = 0; i < 4; i++)
		{
			fSprites[firstRectSprite + i].rotation = Custom.VecToDeg(Data.handlePos);
		}
		MoveSprite(lineSprite, vector2 + Data.handlePos / 2f);
		fSprites[lineSprite].rotation = Custom.VecToDeg(Data.handlePos);
		fSprites[lineSprite].scaleY = Data.velocity;
		fSprites[lineSprite].color = new Color(0.5f, 0.5f, 1f);
	}
}
