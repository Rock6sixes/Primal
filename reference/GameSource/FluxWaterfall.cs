using RWCustom;
using UnityEngine;

public class FluxWaterfall : WaterFall
{
	public PlacedObject.WaterFlowData data;

	public StaticSoundLoop topLoop;

	public StaticSoundLoop bottomLoop;

	public FluxWaterfall(Room room, IntVector2 tilePos, PlacedObject.WaterFlowData data)
		: base(room, tilePos, data.flow, data.width)
	{
		this.data = data;
	}

	public float GetFlow()
	{
		if (!data.dynamic)
		{
			return data.flow;
		}
		WaterLevelCycle waterCycle = room.world.rainCycle.waterCycle;
		WaterLevelCycle.Stage stage = waterCycle.stage;
		float? num = null;
		float? num2 = null;
		switch (stage)
		{
		case WaterLevelCycle.Stage.Fall:
			return 0f;
		case WaterLevelCycle.Stage.Min:
			num = waterCycle.TimeLeftInStage;
			break;
		case WaterLevelCycle.Stage.Rise:
			num = 0f - waterCycle.timeInStage;
			break;
		}
		switch (stage)
		{
		case WaterLevelCycle.Stage.Rise:
			num2 = waterCycle.TimeLeftInStage;
			break;
		case WaterLevelCycle.Stage.Max:
			num2 = 0f - waterCycle.timeInStage;
			break;
		}
		float num3 = 1f;
		if (num.HasValue)
		{
			num3 *= Mathf.Clamp01((num.Value + data.preDelay) / -1f);
		}
		if (num2.HasValue)
		{
			num3 *= Mathf.Pow(Mathf.Clamp01((num2.Value + data.postDelay) / 2f), 3f);
		}
		num3 = Mathf.Min(num3 + room.game.globalRain.floodSpeed, 1f);
		return Mathf.SmoothStep(0f, data.flow, num3);
	}

	public override void Update(bool eu)
	{
		originalFlow = data.flow;
		setFlow = GetFlow();
		if (topLoop == null)
		{
			topLoop = new StaticSoundLoop(SoundID.Flux_Waterfall_Top_LOOP, pos, room, 0f, 0f)
			{
				randomStartPosition = true,
				pitch = Random.Range(0.9f, 1.1f)
			};
			bottomLoop = new StaticSoundLoop(SoundID.Flux_Waterfall_Bottom_LOOP, pos, room, 0f, 0f)
			{
				randomStartPosition = true,
				pitch = Random.Range(0.9f, 1.1f)
			};
		}
		topLoop.Update();
		topLoop.pos = pos + (float)width * 10f * Vector2.right;
		topLoop.volume = setFlow;
		bottomLoop.Update();
		bottomLoop.pos.x = topLoop.pos.x;
		bottomLoop.pos.y = base.strikeLevel;
		bottomLoop.volume = setFlow;
		base.Update(eu);
	}
}
