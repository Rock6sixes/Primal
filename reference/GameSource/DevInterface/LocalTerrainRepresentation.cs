using RWCustom;
using UnityEngine;

namespace DevInterface;

public class LocalTerrainRepresentation : SplineObjectRepresentation
{
	public LocalTerrainCurve terrain;

	public new PlacedObject.LocalTerrainData Data => pObj.data as PlacedObject.LocalTerrainData;

	public LocalTerrainRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Bottom_Handle", this, new Vector2(0f, 0f - Data.bottom)));
		for (int i = 0; i < 3; i++)
		{
			fSprites.Add(new FSprite("pixel")
			{
				anchorY = 0f
			});
			owner.placedObjectsContainer.AddChild(fSprites[1 + i]);
		}
		if (owner.room.terrain != null)
		{
			foreach (TerrainManager.ITerrain terrain in owner.room.terrain.terrainList)
			{
				if (terrain is LocalTerrainCurve localTerrainCurve && localTerrainCurve.data == Data)
				{
					this.terrain = localTerrainCurve;
					return;
				}
			}
		}
		owner.room.AddObject(this.terrain = new LocalTerrainCurve(owner.room, Data));
	}

	public override void Update()
	{
		base.Update();
		if (dragged || (subNodes[1] as Handle).dragged || splineControl.Alterred)
		{
			terrain.RefreshCurve();
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		Handle handle = subNodes[1] as Handle;
		Data.bottom = Mathf.Max(0f, 0f - handle.pos.y);
		handle.Move(new Vector2(0f, 0f - Data.bottom));
		MoveSprite(1, absPos);
		fSprites[1].scaleY = handle.pos.magnitude;
		fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, handle.absPos);
		Vector2 vector = absPos + splineControl.posB;
		Vector2 vector2 = new Vector2(vector.x, handle.absPos.y);
		MoveSprite(2, vector);
		fSprites[2].scaleY = Vector2.Distance(vector, vector2);
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
		MoveSprite(3, handle.absPos);
		fSprites[3].scaleY = Vector2.Distance(handle.absPos, vector2);
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(handle.absPos, vector2);
	}
}
