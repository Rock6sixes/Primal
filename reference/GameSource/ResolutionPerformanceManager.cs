using System;
using UnityEngine;

public class ResolutionPerformanceManager : MonoBehaviour
{
	public static ResolutionPerformanceManager instance;

	public const int NormalModeScreenWidth = 1280;

	public const int NormalModeScreenHeight = 720;

	public const int BoostModeScreenWidth = 1920;

	public const int BoostModeScreenHeight = 1080;

	public static event Action OnResolutionChanged;

	public void Awake()
	{
		if (instance != null)
		{
			if (instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
		}
		else
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		_ = Screen.currentResolution;
	}

	public void OnDestroy()
	{
	}

	public static void EnableEnhancedPerformanceConfiguration(bool enabled)
	{
	}

	public void UpdateResolution()
	{
	}
}
