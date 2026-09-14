using UnityEngine;

public class LoachState : HealthState
{
	public int interest;

	public bool isChasing => interest > 0;

	public LoachState(AbstractCreature crit)
		: base(crit)
	{
	}

	public bool SetNewTarget(float bonus = 0f)
	{
		bonus = Mathf.Clamp(bonus, -1f, 1f);
		float num = Mathf.InverseLerp(0f, 350f, interest);
		if (Random.value + bonus > num)
		{
			interest = 250;
			return true;
		}
		return false;
	}

	public bool UpdateInterest(int change)
	{
		interest = Mathf.Clamp(interest + change, 0, 400);
		if (interest <= 0)
		{
			return true;
		}
		return false;
	}
}
