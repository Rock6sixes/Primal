using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class AxisHandle : Handle
{
	public enum Axis
	{
		X,
		Y
	}

	public Vector2 offset = Vector2.zero;

	public string defaultTitle;

	public Axis axis;

	public float min;

	public float max;

	public bool clamp;

	public bool disabled;

	public float value
	{
		get
		{
			if (axis != 0)
			{
				return pos.y;
			}
			return pos.x;
		}
		set
		{
			if (axis == Axis.X)
			{
				pos.x = value;
			}
			else
			{
				pos.y = value;
			}
		}
	}

	public AxisHandle(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 offset, Axis axis, string title, bool clamp = false, float min = 0f, float max = 0f)
		: base(owner, IDstring, parentNode, pos)
	{
		this.offset = offset;
		this.axis = axis;
		defaultTitle = title;
		this.clamp = clamp;
		this.min = min;
		this.max = max;
		fSprites[0].RemoveFromContainer();
		FSprite node = new FSprite("Futile_White")
		{
			scale = 1f,
			shader = Custom.rainWorld.Shaders[(axis == Axis.X) ? "DevUI_TwinArrowHorizontal" : "DevUI_TwinArrowVertical"]
		};
		fSprites[0] = node;
		FSprite fSprite = new FSprite("pixel")
		{
			anchorY = 0f
		};
		fSprites.Add(fSprite);
		fLabels.Add(new FLabel(Custom.GetFont(), title));
		owner.placedObjectsContainer.AddChild(fLabels[0]);
		if (owner != null)
		{
			owner.placedObjectsContainer.AddChild(node);
			owner.placedObjectsContainer.AddChild(fSprite);
		}
	}

	public void SetVisibility(bool isDisabled)
	{
		disabled = isDisabled;
		for (int i = 0; i < fSprites.Count; i++)
		{
			fSprites[i].isVisible = !isDisabled;
		}
	}

	public override void Update()
	{
		if (disabled)
		{
			fLabels[0].isVisible = false;
			return;
		}
		base.Update();
		fLabels[0].isVisible = base.MouseOver;
		if (axis == Axis.X)
		{
			if (clamp)
			{
				pos.x = Mathf.Clamp(pos.x, min, max);
			}
			if (pos.y + offset.y != 0f)
			{
				Move(Vector2.right * pos.x + offset);
			}
		}
		else
		{
			if (clamp)
			{
				pos.y = Mathf.Clamp(pos.y, min, max);
			}
			if (pos.x + offset.x != 0f)
			{
				Move(Vector2.up * pos.y + offset);
			}
		}
		if (owner != null && dragged)
		{
			fLabels[0].color = Color.blue;
		}
		else
		{
			fLabels[0].color = (base.MouseOver ? Color.red : (Input.GetKey("b") ? Color.black : Color.white));
		}
		MoveSprite(1, (parentNode as PositionedDevUINode).absPos);
		fSprites[1].scaleY = pos.magnitude;
		fSprites[1].rotation = Custom.VecToDeg(pos);
		MoveLabel(0, absPos + Vector2.up * 15f);
	}

	public void SetName(string text)
	{
		if (text == null)
		{
			text = defaultTitle;
		}
		fLabels[0].text = text;
	}
}
