using System;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class TerrainHandleRepresentation : PlacedObjectRepresentation
{
	public Vector2 lastPos;

	public TerrainHandleRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		PlacedObject.TerrainHandleData terrainHandleData = pObj.data as PlacedObject.TerrainHandleData;
		subNodes.Add(new Handle(owner, "Curve_Handle", this, terrainHandleData.leftOffset));
		subNodes.Add(new Handle(owner, "Curve_Handle", this, terrainHandleData.rightOffset));
		subNodes.Add(new Handle(owner, "Curve_Back_Handle", this, new Vector2(0f, terrainHandleData.backHeight)));
		for (int i = 0; i < 3; i++)
		{
			fSprites.Add(new FSprite("pixel")
			{
				anchorY = 0f
			});
			owner.placedObjectsContainer.AddChild(fSprites[1 + i]);
		}
		fSprites.Add(new FSprite("pixel")
		{
			scaleX = 20f,
			scaleY = 2f
		});
		owner.placedObjectsContainer.AddChild(fSprites[4]);
	}

	public override void Refresh()
	{
		base.Refresh();
		PlacedObject.TerrainHandleData terrainHandleData = pObj.data as PlacedObject.TerrainHandleData;
		Handle handle = subNodes[0] as Handle;
		Handle handle2 = subNodes[1] as Handle;
		Handle handle3 = subNodes[2] as Handle;
		handle.pos.x = Math.Min(handle.pos.x, 0f);
		handle2.pos.x = Math.Max(handle2.pos.x, 0f);
		handle3.pos.x = 0f;
		bool flag = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || flag)
		{
			if (handle.pos != terrainHandleData.leftOffset)
			{
				handle2.pos = -handle.pos.normalized * (flag ? handle.pos.magnitude : handle2.pos.magnitude);
			}
			else if (handle2.pos != terrainHandleData.rightOffset)
			{
				handle.pos = -handle2.pos.normalized * (flag ? handle2.pos.magnitude : handle.pos.magnitude);
			}
		}
		bool num = terrainHandleData.leftOffset != handle.pos || terrainHandleData.rightOffset != handle2.pos || terrainHandleData.backHeight != handle3.pos.y || lastPos != pObj.pos;
		terrainHandleData.leftOffset = handle.pos;
		terrainHandleData.rightOffset = handle2.pos;
		terrainHandleData.backHeight = handle3.pos.y;
		MoveSprite(1, absPos);
		fSprites[1].scaleY = handle.pos.magnitude;
		fSprites[1].rotation = Custom.VecToDeg(handle.pos);
		MoveSprite(2, absPos);
		fSprites[2].scaleY = handle2.pos.magnitude;
		fSprites[2].rotation = Custom.VecToDeg(handle2.pos);
		MoveSprite(3, absPos);
		fSprites[3].scaleY = handle3.pos.y;
		MoveSprite(4, absPos + handle3.pos);
		if (!num || owner.room.terrain == null)
		{
			return;
		}
		lastPos = pObj.pos;
		foreach (TerrainManager.ITerrain terrain in owner.room.terrain.terrainList)
		{
			if (terrain is TerrainCurve terrainCurve)
			{
				terrainCurve.UpdateHandles();
			}
		}
	}
}
