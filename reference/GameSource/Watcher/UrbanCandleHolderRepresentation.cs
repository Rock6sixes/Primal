using DevInterface;
using UnityEngine;

namespace Watcher;

public class UrbanCandleHolderRepresentation : PlacedObjectRepresentation
{
	public UrbanCandleHolder.UrbanCandleHolderData data;

	public UrbanCandleHolder obj;

	public bool needRespawn;

	public AxisHandle tiltHandle;

	public AxisHandle depthHandle;

	public AxisHandle heightHandle;

	public AxisHandle seedHandle;

	public AxisHandle scaleHandle;

	public AxisHandle candleWidth;

	public int seed;

	public UrbanCandleHolderRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Urban Candle Holder")
	{
		data = pObj.data as UrbanCandleHolder.UrbanCandleHolderData;
		if (data.obj == null)
		{
			data.seed = (int)(Random.value * 111111f);
			data.obj = new UrbanCandleHolder(owner.room, data);
			owner.room.AddObject(data.obj);
		}
		obj = data.obj;
		float num = 15f;
		float num2 = 0f;
		Vector2 zero = Vector2.zero;
		subNodes.Add(heightHandle = new AxisHandle(owner, "Height", this, zero, zero, AxisHandle.Axis.Y, "Height", clamp: true, 0f, 800f)
		{
			value = data.height * 100f
		});
		subNodes.Add(depthHandle = new AxisHandle(owner, "Depth", this, zero, Vector2.down * (num2 += num), AxisHandle.Axis.X, "Depth", clamp: true, 0f, 100f)
		{
			value = (1f - data.depth) * 100f
		});
		subNodes.Add(scaleHandle = new AxisHandle(owner, "Scale", this, zero, Vector2.down * (num2 += num), AxisHandle.Axis.X, "Scale", clamp: true, 0f, 800f)
		{
			value = data.scale * 100f
		});
		subNodes.Add(tiltHandle = new AxisHandle(owner, "Tilt", this, zero, Vector2.down * (num2 += num), AxisHandle.Axis.X, "Tilt", clamp: true, 0f, 80f)
		{
			value = data.tilt * 100f
		});
		subNodes.Add(candleWidth = new AxisHandle(owner, "CandleWidth", this, zero, Vector2.down * (num2 += num), AxisHandle.Axis.X, "Candle Width", clamp: true, 0f, 200f)
		{
			value = data.candleWidth * 100f
		});
		subNodes.Add(seedHandle = new AxisHandle(owner, "Seed", this, zero, Vector2.down * (num2 += num), AxisHandle.Axis.X, "Seed")
		{
			value = (float)data.seed / 111111f * 100f
		});
	}

	public FSprite Line()
	{
		FSprite fSprite = new FSprite("pixel")
		{
			anchorY = 0f
		};
		owner.placedObjectsContainer.AddChild(fSprite);
		return fSprite;
	}

	public void RespawnObj()
	{
		needRespawn = false;
		data.seed = seed;
		obj.Destroy();
		data.obj.Destroy();
		data.obj = new UrbanCandleHolder(owner.room, data);
		obj = data.obj;
		owner.room.AddObject(data.obj);
	}

	public override void Refresh()
	{
		base.Refresh();
		if (tiltHandle.dragged)
		{
			data.tilt = Mathf.Abs(tiltHandle.value) / 100f;
			obj.tilt = data.tilt;
		}
		if (depthHandle.dragged)
		{
			data.depth = 1f - Mathf.Abs(depthHandle.value) / 100f;
			obj.depth = data.depth;
		}
		if (heightHandle.dragged)
		{
			data.height = Mathf.Abs(heightHandle.value) / 100f;
			obj.height = data.height;
		}
		if (scaleHandle.dragged)
		{
			data.scale = Mathf.Abs(scaleHandle.value) / 100f;
			obj.scale = data.scale;
		}
		if (candleWidth.dragged)
		{
			data.candleWidth = Mathf.Abs(candleWidth.value) / 100f;
			obj.candleWidthScale = data.candleWidth;
		}
		if (seedHandle.dragged)
		{
			seed = (int)(Mathf.Abs(seedHandle.value) / 100f * 111111f);
			needRespawn = true;
		}
		if (needRespawn && !seedHandle.dragged)
		{
			RespawnObj();
		}
		data.pos = pObj.pos;
		obj.pos = pObj.pos;
	}
}
