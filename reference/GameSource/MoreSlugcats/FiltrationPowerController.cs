using UnityEngine;

namespace MoreSlugcats;

public class FiltrationPowerController
{
	public int counter;

	public float powerOutput;

	public World world;

	public float outtageIntensity;

	public bool deadFilter;

	public bool fullTurnOff;

	public int triggerThreshold;

	public SlugcatStats.Timeline timeline;

	public float powerGoal;

	public float ElectricPower => powerOutput;

	public bool PowerOn => powerGoal > 0.6f;

	public bool PowerOff => powerGoal < 0.2f;

	public FiltrationPowerController(World world)
	{
		this.world = world;
		powerOutput = 0f;
		counter = 0;
		timeline = (world.game.IsStorySession ? world.game.TimelinePoint : SlugcatStats.Timeline.White);
		powerGoal = Random.Range(0f, 1f);
		fullTurnOff = true;
		outtageIntensity = Random.Range(0.8f, 1f);
		if (timeline == SlugcatStats.Timeline.Spear || timeline == SlugcatStats.Timeline.Artificer || timeline == SlugcatStats.Timeline.Red)
		{
			fullTurnOff = false;
			outtageIntensity = Random.Range(0.1f, 0.3f);
			if (timeline == SlugcatStats.Timeline.Spear)
			{
				outtageIntensity = Random.Range(0f, 0.1f);
			}
			if (timeline == SlugcatStats.Timeline.Red)
			{
				outtageIntensity = Random.Range(0.51f, 0.6f);
			}
		}
		if (timeline == SlugcatStats.Timeline.White || timeline == SlugcatStats.Timeline.Yellow || timeline == SlugcatStats.Timeline.Rivulet || timeline == SlugcatStats.Timeline.Saint)
		{
			fullTurnOff = true;
			deadFilter = true;
			outtageIntensity = 1f;
		}
	}

	public void Update()
	{
		if (deadFilter)
		{
			counter = 0;
			powerGoal = 0f;
			powerOutput = 0f;
			return;
		}
		counter++;
		if (counter >= triggerThreshold)
		{
			counter = 0;
			if (timeline == SlugcatStats.Timeline.Spear)
			{
				triggerThreshold = Random.Range(600, 900);
			}
			else if (timeline == SlugcatStats.Timeline.Artificer)
			{
				triggerThreshold = Random.Range(500, 1100);
			}
			else if (timeline == SlugcatStats.Timeline.Red)
			{
				triggerThreshold = Random.Range(700, 1600);
			}
			else
			{
				triggerThreshold = Random.Range(900, 1800);
			}
			bool flag;
			if (PowerOn)
			{
				flag = false;
				if (outtageIntensity < 0.5f || Random.value >= outtageIntensity * 0.6f)
				{
					if (fullTurnOff)
					{
						powerGoal = 0f;
					}
					else
					{
						powerGoal = Random.Range(0f, 0.1f);
					}
				}
			}
			else if (PowerOff)
			{
				powerGoal = Random.Range(0.9f, 1f) * Mathf.Lerp(1f, 0.3f, outtageIntensity);
				flag = true;
			}
			else
			{
				powerGoal = Random.Range(0f, 1f);
				flag = powerGoal >= 0.5f;
			}
			if (!flag)
			{
				triggerThreshold *= (int)Mathf.Clamp(8f * outtageIntensity, 0.1f, 99999f);
			}
		}
		else
		{
			powerGoal += Random.Range(-0.01f, 0.01f) * (1f - outtageIntensity);
			powerGoal = Mathf.Clamp(powerGoal, 0f, 1f);
			powerGoal *= Mathf.Lerp(0.999f, 0.985f, outtageIntensity);
		}
		powerOutput = Mathf.Lerp(powerOutput, powerGoal, Mathf.Lerp(0.12f, 0.05f, outtageIntensity));
	}
}
