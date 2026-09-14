using System;
using UnityEngine;

public class PathfinderResourceDivider
{
	public int lastTotalRequestedUpdates;

	public int requestedUpdates;

	public int budget = 50;

	public int lastTotalRequestedAccUpdates;

	public int requestedAccUpdates;

	public int accessibilityBudget = 500;

	public PathfinderResourceDivider(RainWorldGame game)
	{
	}

	public void Update()
	{
		lastTotalRequestedUpdates = requestedUpdates;
		requestedUpdates = 0;
		lastTotalRequestedAccUpdates = requestedAccUpdates;
		requestedAccUpdates = 0;
	}

	public int RequestPathfinderUpdates(int ideal)
	{
		requestedUpdates += ideal;
		if (lastTotalRequestedUpdates <= budget)
		{
			return ideal;
		}
		float num = (float)ideal / (float)lastTotalRequestedUpdates;
		num *= (float)budget;
		return Math.Max(1, Mathf.FloorToInt(num));
	}

	public int RequesAccesibilityUpdates(int ideal)
	{
		requestedAccUpdates += ideal;
		if (lastTotalRequestedAccUpdates <= accessibilityBudget)
		{
			return ideal;
		}
		float num = (float)ideal / (float)lastTotalRequestedAccUpdates;
		num *= (float)accessibilityBudget;
		return Math.Max(1, Mathf.FloorToInt(num));
	}
}
