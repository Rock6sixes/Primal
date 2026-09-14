using UnityEngine;

namespace DevInterface;

public class SliderHandle : Handle
{
	public bool vertical;

	public Vector2 origin;

	public float Value
	{
		get
		{
			if (!vertical)
			{
				return pos.x - origin.x;
			}
			return pos.y - origin.y;
		}
		set
		{
			if (vertical)
			{
				pos.y = origin.y + value;
			}
			else
			{
				pos.x = origin.x + value;
			}
		}
	}

	public SliderHandle(DevUI owner, string IDstring, DevUINode parentNode, Vector2 origin, float value, bool vertical, bool drawLine)
		: base(owner, IDstring, parentNode, origin)
	{
		this.vertical = vertical;
		this.origin = origin;
		Value = value;
		fSprites.Add(new FSprite("pixel")
		{
			anchorY = 0f,
			rotation = (vertical ? 0f : 90f),
			isVisible = drawLine
		});
		owner?.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
	}

	public override void Refresh()
	{
		if (vertical)
		{
			pos.x = origin.x;
		}
		else
		{
			pos.y = origin.y;
		}
		base.Refresh();
		MoveSprite(1, absPos - pos + origin);
		fSprites[1].scaleY = Value;
	}
}
