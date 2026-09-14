using RWCustom;
using UnityEngine;

namespace DevInterface;

public class SuperSlopeRepresentation : ResizeableObjectRepresentation
{
	public SuperSlope slope;

	public PlacedObject.SuperSlopeData Data => pObj.data as PlacedObject.SuperSlopeData;

	public SuperSlopeRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name, showRing: false)
	{
		subNodes.Add(new Handle(owner, "Bottom_Handle", this, new Vector2(0f, 0f - Data.bottom)));
		for (int i = 0; i < 3; i++)
		{
			fSprites.Add(new FSprite("pixel")
			{
				anchorY = 0f
			});
			owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		}
		if (owner.room.terrain != null)
		{
			foreach (TerrainManager.ITerrain terrain in owner.room.terrain.terrainList)
			{
				if (terrain is SuperSlope superSlope && superSlope.data == Data)
				{
					slope = superSlope;
					return;
				}
			}
		}
		owner.room.AddObject(slope = new SuperSlope(owner.room, Data));
	}

	public override void Refresh()
	{
		base.Refresh();
		Handle handle = subNodes[1] as Handle;
		Data.bottom = Mathf.Max(10f, 0f - handle.pos.y);
		handle.Move(new Vector2(0f, 0f - Data.bottom));
		slope.thickness = Data.bottom;
		Vector2 vector = absPos - new Vector2(0f, Data.bottom);
		Vector2 vector2 = absPos + Data.handlePos - new Vector2(0f, Data.bottom);
		MoveSprite(2, absPos);
		MoveSprite(3, absPos + Data.handlePos);
		MoveSprite(4, vector);
		fSprites[2].scaleY = Data.bottom;
		fSprites[2].rotation = 180f;
		fSprites[3].scaleY = Data.bottom;
		fSprites[3].rotation = 180f;
		fSprites[4].scaleY = Vector2.Distance(vector, vector2);
		fSprites[4].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
	}
}
