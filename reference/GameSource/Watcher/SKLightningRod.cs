using RWCustom;
using UnityEngine;

namespace Watcher;

public class SKLightningRod : UpdatableAndDeletable
{
	public Counter delay;

	public Vector2 pos;

	public float safeRadius;

	public bool safePosAdded;

	public SKLightningRod(Room room, Vector2 pos, float safeRad)
	{
		base.room = room;
		this.pos = pos;
		safeRadius = safeRad;
		delay = new Counter(1);
		delay.SetToMin();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room.lethalThunderStorm != null)
		{
			AddSafeSpot();
			delay.Tick();
			TrySpawnLethalStrike();
		}
	}

	public void AddSafeSpot()
	{
		if (!safePosAdded && room.lightningMaker != null)
		{
			room.lightningMaker.safePlaces.Add(new Vector3(pos.x, pos.y, safeRadius));
		}
	}

	public void TrySpawnLethalStrike()
	{
		if (delay.isFinished)
		{
			if (room.lightningMaker == null)
			{
				room.AddObject(room.lightningMaker = new LightningMaker(room));
			}
			room.lightningMaker.Strike(pos, 0f, 1f, 100f, 0f, 400);
			delay.SetMax(room.lethalThunderStorm.GetLethalDelay(1f - room.lethalThunderStorm.amount));
			delay.SetToMax();
		}
	}
}
