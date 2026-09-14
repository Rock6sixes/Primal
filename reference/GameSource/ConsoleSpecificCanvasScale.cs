using RWCustom;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleSpecificCanvasScale : MonoBehaviour
{
	public CanvasScaler scaler;

	public void Start()
	{
		scaler = base.gameObject.GetComponent<CanvasScaler>();
		scaler.referenceResolution = new Vector2(1366f, 768f);
	}

	public void Update()
	{
		if (Custom.rainWorld != null && Custom.rainWorld.options != null && Custom.rainWorld.options.optionsLoaded)
		{
			if (Custom.rainWorld.options.fullScreen && Options.IsSteamDeckResolution(Custom.rainWorld.options.ScreenSize))
			{
				scaler.referenceResolution = new Vector2(Display.main.systemWidth, Display.main.systemHeight);
			}
			else
			{
				scaler.referenceResolution = new Vector2(1366f, 768f);
			}
		}
	}
}
