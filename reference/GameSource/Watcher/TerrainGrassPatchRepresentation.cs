using DevInterface;
using UnityEngine;

namespace Watcher;

public class TerrainGrassPatchRepresentation : PlacedObjectRepresentation
{
	public AxisHandle amount;

	public AxisHandle depthOffset;

	public AxisHandle depthRange;

	public AxisHandle maxHeight;

	public AxisHandle minHeight;

	public AxisHandle fallOff;

	public AxisHandle range;

	public AxisHandle width;

	public GrassBlade.TerrainGrassPatchData data;

	public bool lastDragged;

	public TerrainGrassPatchRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Terrain Grass Patch")
	{
		data = pObj.data as GrassBlade.TerrainGrassPatchData;
		int num = 30;
		AddAxisHandle(ref range, this, AxisHandle.Axis.X, num, data.range, "Range");
		AddAxisHandle(ref fallOff, this, AxisHandle.Axis.X, 2 * num, data.fallOff * 200f, "FallOff", clamp: true, 0f, 200f);
		AddAxisHandle(ref width, this, AxisHandle.Axis.X, 3 * num, data.width * 50f, "Width", clamp: true, 0f, 200f);
		AddAxisHandle(ref depthOffset, this, AxisHandle.Axis.X, 4 * num, (data.depthOffset - 0.5f) * 200f, "Depth Offset", clamp: true, -100f, 100f);
		AddAxisHandle(ref depthRange, this, AxisHandle.Axis.X, 5 * num, data.depthRange * 200f, "Depth Range", clamp: true, 0f, 100f);
		AddAxisHandle(ref amount, this, AxisHandle.Axis.Y, -1 * num, data.amount / 4, "Amount", clamp: true, 0f, 500f);
		AddAxisHandle(ref minHeight, this, AxisHandle.Axis.Y, num, data.minHeight, "Min Height", clamp: true, 0f, 1000f);
		AddAxisHandle(ref maxHeight, this, AxisHandle.Axis.Y, 2 * num, data.maxHeight, "Max Height", clamp: true, 0f, 1000f);
	}

	public void AddAxisHandle(ref AxisHandle handle, DevUINode node, AxisHandle.Axis axis, float offset, float startPos, string name, bool clamp = false, float min = 0f, float max = 0f)
	{
		handle = new AxisHandle(owner, "axis_handle", node, Vector2.zero, ((axis == AxisHandle.Axis.X) ? Vector2.down : Vector2.right) * offset, axis, name, clamp, min, max);
		handle.value = startPos;
		node.subNodes.Add(handle);
	}

	public override void Refresh()
	{
		base.Refresh();
		bool flag = dragged;
		if (data.pos != pObj.pos)
		{
			data.pos = pObj.pos;
		}
		if (range.dragged)
		{
			data.range = Mathf.Abs(range.value);
			flag = flag || true;
		}
		if (width.dragged)
		{
			data.width = width.value * 0.02f;
			flag = flag || true;
		}
		if (depthOffset.dragged)
		{
			data.depthOffset = depthOffset.value * 0.005f + 0.5f;
			flag = flag || true;
		}
		if (depthRange.dragged)
		{
			data.depthRange = depthRange.value * 0.01f;
			flag = flag || true;
		}
		if (fallOff.dragged)
		{
			data.fallOff = fallOff.value * 0.005f;
			flag = flag || true;
		}
		if (amount.dragged)
		{
			data.amount = (int)amount.value * 4;
			flag = flag || true;
		}
		if (maxHeight.dragged)
		{
			data.maxHeight = maxHeight.value;
			flag = flag || true;
		}
		if (minHeight.dragged)
		{
			data.minHeight = minHeight.value;
			flag = flag || true;
		}
		if (lastDragged && !flag)
		{
			GrassBlade.SpawnGrassPatch(owner.room, data);
		}
		lastDragged = flag;
	}
}
