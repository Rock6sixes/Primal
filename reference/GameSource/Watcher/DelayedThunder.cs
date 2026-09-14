using RWCustom;
using UnityEngine;

namespace Watcher;

public class DelayedThunder : UpdatableAndDeletable
{
	public Counter time;

	public float strength;

	public DelayedThunder(int delayMinFrames, int delayMaxFrames)
	{
		int num = Random.Range(delayMinFrames, delayMaxFrames);
		strength = Mathf.InverseLerp(delayMaxFrames, delayMinFrames, num);
		time = new Counter(num);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!time.isFinished)
		{
			time.Tick();
			if (time.isFinished)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Natural_Thunder, 0f, Mathf.Lerp(0.6f, 1.2f, strength), Random.Range(0.8f, 1.2f));
				Destroy();
			}
		}
	}
}
