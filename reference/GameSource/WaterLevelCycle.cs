using UnityEngine;

public class WaterLevelCycle
{
	public enum Stage
	{
		Rise,
		Max,
		Fall,
		Min
	}

	public Vector2 riseTimeRange = new Vector2(7f, 7f);

	public Vector2 maxTimeRange = new Vector2(8f, 10f);

	public Vector2 fallTimeRange = new Vector2(8f, 8f);

	public Vector2 minTimeRange = new Vector2(8f, 10f);

	public Stage stage;

	public float stageDuration;

	public float timeInStage;

	public float lastTimeInStage;

	public float TimeLeftInStage => stageDuration - timeInStage;

	public float StageFraction => timeInStage / stageDuration;

	public float WaterHeightFraction => stage switch
	{
		Stage.Rise => Mathf.SmoothStep(0f, 1f, StageFraction), 
		Stage.Max => 1f, 
		Stage.Fall => Mathf.SmoothStep(1f, 0f, StageFraction), 
		_ => 0f, 
	};

	public WaterLevelCycle()
	{
		stage = ((Random.value < 0.5f) ? Stage.Max : Stage.Min);
		Vector2 timeRange = GetTimeRange(stage);
		stageDuration = Random.Range(timeRange.x, timeRange.y);
		timeInStage = Random.value * stageDuration;
	}

	public Vector2 GetTimeRange(Stage stage)
	{
		return stage switch
		{
			Stage.Max => maxTimeRange, 
			Stage.Fall => fallTimeRange, 
			Stage.Min => minTimeRange, 
			_ => riseTimeRange, 
		};
	}

	public void NextStage()
	{
		if (stage == Stage.Min)
		{
			stage = Stage.Rise;
		}
		else
		{
			stage++;
		}
		lastTimeInStage = 0f;
		timeInStage = 0f;
		Vector2 timeRange = GetTimeRange(stage);
		stageDuration = Random.Range(timeRange.x, timeRange.y);
	}

	public void Update()
	{
		lastTimeInStage = timeInStage;
		timeInStage += 0.025f;
		if (timeInStage >= stageDuration)
		{
			NextStage();
		}
	}
}
