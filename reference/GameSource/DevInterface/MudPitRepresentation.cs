using RWCustom;
using UnityEngine;

namespace DevInterface;

public class MudPitRepresentation : ResizeableObjectRepresentation
{
	public int firstRectSprite;

	public SliderHandle decalHandle;

	public MudPit.MudPitData Data => pObj.data as MudPit.MudPitData;

	public MudPitRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name, showRing: false)
	{
		fSprites[1].isVisible = false;
		firstRectSprite = fSprites.Count;
		for (int i = 0; i < 4; i++)
		{
			FSprite fSprite = new FSprite("pixel")
			{
				color = Custom.hexToColor("7a4832"),
				anchorY = 0f
			};
			fSprites.Add(fSprite);
			owner.placedObjectsContainer.AddChild(fSprite);
		}
		decalHandle = new SliderHandle(owner, "Mud_Decal_Handle", this, Vector2.zero, Data.decalSize, vertical: false, drawLine: true);
		subNodes.Add(decalHandle);
	}

	public override void Refresh()
	{
		base.Refresh();
		Vector2 vector = absPos;
		Vector2 vector2 = absPos + Data.handlePos;
		Vector2 vector3 = absPos + new Vector2(0f, Data.handlePos.y);
		Vector2 vector4 = absPos + new Vector2(Data.handlePos.x, 0f);
		DrawLine(firstRectSprite, vector, vector3);
		DrawLine(firstRectSprite + 1, vector3, vector2);
		DrawLine(firstRectSprite + 2, vector2, vector4);
		DrawLine(firstRectSprite + 3, vector4, vector);
		Data.decalSize = decalHandle.Value;
	}

	public void DrawLine(int sprite, Vector2 from, Vector2 to)
	{
		MoveSprite(sprite, from);
		fSprites[sprite].scaleY = Vector2.Distance(from, to);
		fSprites[sprite].rotation = Custom.AimFromOneVectorToAnother(from, to);
	}
}
