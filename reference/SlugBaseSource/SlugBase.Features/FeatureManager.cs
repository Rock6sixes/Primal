using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using SlugBase.Interface;

namespace SlugBase.Features;

internal static class FeatureManager
{
	private static readonly Dictionary<string, Feature> _features = new Dictionary<string, Feature>();

	private static readonly List<string> _queuedErrors = new List<string>();

	public static void Register(Feature feature)
	{
		if (feature == null)
		{
			throw new ArgumentNullException("Feature");
		}
		string iD = feature.ID;
		if (!_features.ContainsKey(iD))
		{
			_features.Add(iD, feature);
			return;
		}
		string text = "Duplicate feature: " + iD + "!";
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: false);
		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			MethodBase method = stackTrace.GetFrame(i).GetMethod();
			Assembly assembly = method?.ReflectedType?.Assembly;
			if (assembly != typeof(RainWorld).Assembly && assembly != typeof(SlugBasePlugin).Assembly)
			{
				text = text + "\nRegistered by: " + method.ReflectedType.Name + " in " + assembly.GetName().Name + ".dll";
				break;
			}
		}
		SlugBasePlugin.Logger.LogDebug(text);
		if (ErrorList.Instance != null)
		{
			ErrorList.Instance.AddError(ErrorList.ErrorIcon.Plugin, text, null, null);
		}
		else
		{
			_queuedErrors.Add(text);
		}
	}

	public static bool TryGetFeature(string id, out Feature feature)
	{
		return _features.TryGetValue(id, out feature);
	}

	public static void LogErrors()
	{
		foreach (string queuedError in _queuedErrors)
		{
			ErrorList.Instance.AddError(ErrorList.ErrorIcon.Plugin, queuedError, null, null);
		}
		_queuedErrors.Clear();
	}
}
