using System.Globalization;
using Kittehface.Build;
using Kittehface.Framework20;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
	public class FPS
	{
		public float fpsSum;

		public int fpsSamples;

		public float fpsMin = 1f;

		public float fpsMax;

		public float displayFPS;

		public float displayMax;

		public float displayMin;

		public string FPSDisplay { get; set; }

		public void Update(float deltaTime)
		{
			fpsSum += deltaTime;
			fpsSamples++;
			if (fpsMin > Time.deltaTime)
			{
				fpsMin = Time.deltaTime;
			}
			if (fpsMax < Time.deltaTime)
			{
				fpsMax = Time.deltaTime;
			}
			if (fpsSamples > 100)
			{
				float num = fpsSum / (float)fpsSamples;
				displayFPS = 1f / num;
				displayMax = 1f / fpsMin;
				displayMin = 1f / fpsMax;
				fpsSum = 0f;
				fpsSamples = 0;
				fpsMin = 1f;
				fpsMax = 0f;
				FPSDisplay = string.Format(CultureInfo.InvariantCulture, "FPS: {0:0.0} Max: {1:0.0} Min: {2:0.0}", displayFPS, displayMax, displayMin);
			}
		}
	}

	[SerializeField]
	public Text informationText;

	[SerializeField]
	public bool enable;

	public int needsUpdate = 1;

	public const int NeedsUpdateFrameDelay = 2;

	public string infoText = "";

	public FPS fps = new FPS();

	public void Awake()
	{
		ResolutionPerformanceManager.OnResolutionChanged += ResolutionPerformanceManager_OnResolutionChanged;
		InitializeSwitch();
		if (!Utilities.isDebugBuild || !enable)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void InitializeSwitch()
	{
	}

	public void OnDestroy()
	{
		ResolutionPerformanceManager.OnResolutionChanged -= ResolutionPerformanceManager_OnResolutionChanged;
	}

	public void Update()
	{
		fps.Update(Time.deltaTime);
	}

	public void LateUpdate()
	{
		if (!Platform.initialized)
		{
			return;
		}
		if (needsUpdate > 0)
		{
			if (needsUpdate == 1)
			{
				UpdateText();
			}
			needsUpdate--;
		}
		informationText.text = infoText + "\n" + fps.FPSDisplay;
	}

	public void UpdateText()
	{
		if (null != informationText)
		{
			string text = "";
			text += Screen.currentResolution.ToString();
			infoText = text;
		}
	}

	public void RequestUpdate()
	{
		needsUpdate = 2;
	}

	public void ResolutionPerformanceManager_OnResolutionChanged()
	{
		RequestUpdate();
	}

	public void Rewired_ControllerPreDisconnectEvent(ControllerStatusChangedEventArgs args)
	{
		RequestUpdate();
	}

	public void Rewired_ControllerConnectedEvent(ControllerStatusChangedEventArgs args)
	{
		RequestUpdate();
	}

	public void UserInput_OnControllerConfigurationChanged()
	{
		RequestUpdate();
	}
}
