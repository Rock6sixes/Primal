using System;

public static class RXGoKitExtensions
{
	public static TweenConfig floatProp(this TweenConfig config, string propName, float propValue)
	{
		return config.floatProp(propName, propValue, false);
	}

	public static TweenConfig removeWhenComplete(this TweenConfig config)
	{
		config.onComplete((Action<AbstractTween>)HandleRemoveWhenDoneTweenComplete);
		return config;
	}

	public static void HandleRemoveWhenDoneTweenComplete(AbstractTween tween)
	{
		(((Tween)((tween is Tween) ? tween : null)).target as FNode).RemoveFromContainer();
	}
}
