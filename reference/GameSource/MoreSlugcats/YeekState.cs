using UnityEngine;

namespace MoreSlugcats;

public class YeekState : HealthState
{
	public int hungerCounterMax;

	public int PreviousCycleHungerStart;

	public YeekState(AbstractCreature creature)
		: base(creature)
	{
		PreviousCycleHungerStart = 0;
		NewMaxHunger();
	}

	public float HungerIntensity(int CycleTimer)
	{
		return Mathf.InverseLerp(PreviousCycleHungerStart, PreviousCycleHungerStart + hungerCounterMax, CycleTimer);
	}

	public void Feed(int CycleTimer)
	{
		PreviousCycleHungerStart = CycleTimer;
		NewMaxHunger();
	}

	public void NewMaxHunger()
	{
		hungerCounterMax = Random.Range(2000, 5000);
	}
}
