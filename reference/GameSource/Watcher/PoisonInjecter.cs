using UnityEngine;

namespace Watcher;

public class PoisonInjecter : UpdatableAndDeletable
{
	public Creature crit;

	public float poisonAmount;

	public float rate;

	public Color poisonColor;

	public PoisonInjecter(Creature crit, float poisonAmount, float drainRateInSeconds, Color poisonColor)
	{
		this.crit = crit;
		this.poisonAmount = poisonAmount;
		rate = 1f / (40f * drainRateInSeconds);
		this.poisonColor = poisonColor;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (poisonAmount <= 0f)
		{
			Destroy();
			return;
		}
		float num = poisonAmount;
		poisonAmount = Mathf.Max(poisonAmount - rate, 0f);
		crit.InjectPoison(num - poisonAmount, poisonColor);
	}
}
