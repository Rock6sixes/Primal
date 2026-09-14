using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using On;
using SlugBase.Assets;
using SlugBase.Features;
using SlugBase.Interface;
using SlugBase.SaveData;
using UnityEngine;

namespace SlugBase;

[BepInPlugin("slime-cubed.slugbase", "SlugBase", "2.9.2")]
internal class SlugBasePlugin : BaseUnityPlugin
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static hook_PreModsInit _003C_003E9__7_0;

		public static hook_PostModsInit _003C_003E9__7_2;

		internal void _003CAwake_003Eb__7_0(orig_PreModsInit orig, RainWorld self)
		{
			try
			{
				RuntimeHelpers.RunClassConstructor(typeof(PlayerFeatures).TypeHandle);
				RuntimeHelpers.RunClassConstructor(typeof(GameFeatures).TypeHandle);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			orig.Invoke(self);
		}

		internal void _003CAwake_003Eb__7_2(orig_PostModsInit orig, RainWorld self)
		{
			orig.Invoke(self);
			try
			{
				ErrorList.Instance.ClearFileErrors();
				ScanFiles();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	internal new static ManualLogSource Logger;

	internal static Config RemixConfig;

	internal static Color SlugBaseBlue = new Color(0.07450981f, 21f / 85f, 77f / 85f);

	internal static Color SlugBaseGray = new Color(0.57254905f, 0.5882353f, 0.6431373f);

	internal const string Version = "2.9.2";

	private bool _initialized = false;

	public SlugBasePlugin()
	{
		Logger = base.Logger;
	}

	public void Awake()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		object obj = _003C_003Ec._003C_003E9__7_0;
		if (obj == null)
		{
			hook_PreModsInit val = delegate(orig_PreModsInit orig, RainWorld self)
			{
				try
				{
					RuntimeHelpers.RunClassConstructor(typeof(PlayerFeatures).TypeHandle);
					RuntimeHelpers.RunClassConstructor(typeof(GameFeatures).TypeHandle);
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
				orig.Invoke(self);
			};
			_003C_003Ec._003C_003E9__7_0 = val;
			obj = (object)val;
		}
		RainWorld.PreModsInit += (hook_PreModsInit)obj;
		RainWorld.OnModsInit += (hook_OnModsInit)delegate(orig_OnModsInit orig, RainWorld self)
		{
			orig.Invoke(self);
			if (_initialized)
			{
				return;
			}
			_initialized = true;
			try
			{
				RemixConfig = new Config();
				Futile.atlasManager.LoadAtlas("atlases/slugbase");
				ErrorList.Instance = ErrorList.Attach();
				FeatureManager.LogErrors();
				CoreHooks.Apply();
				AssetHooks.Apply();
				FeatureHooks.Apply();
				ExpeditionHooks.Apply();
				JollyCoopHooks.Apply();
				SaveDataHooks.Apply();
				SlugBaseCharacter.Registry.WatchForChanges = true;
				CustomScene.Registry.WatchForChanges = true;
				CustomSlideshow.Registry.WatchForChanges = true;
				CustomTimeline.Registry.WatchForChanges = true;
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		};
		object obj2 = _003C_003Ec._003C_003E9__7_2;
		if (obj2 == null)
		{
			hook_PostModsInit val2 = delegate(orig_PostModsInit orig, RainWorld self)
			{
				orig.Invoke(self);
				try
				{
					ErrorList.Instance.ClearFileErrors();
					ScanFiles();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			};
			_003C_003Ec._003C_003E9__7_2 = val2;
			obj2 = (object)val2;
		}
		RainWorld.PostModsInit += (hook_PostModsInit)obj2;
	}

	public static void ScanFiles()
	{
		SlugBaseCharacter.Registry.ScanDirectory("slugbase");
		CustomScene.Registry.ScanDirectory("slugbase/scenes");
		CustomSlideshow.Registry.ScanDirectory("slugbase/slideshows");
		CustomTimeline.Registry.ScanDirectory("slugbase/timelines");
		foreach (SlugBaseCharacter value in SlugBaseCharacter.Registry.Values)
		{
			value.UpdateLegacyTimeline();
		}
	}

	public void Update()
	{
		SlugBaseCharacter.Registry.ReloadChangedFiles();
		CustomScene.Registry.ReloadChangedFiles();
		CustomSlideshow.Registry.ReloadChangedFiles();
		CustomTimeline.Registry.ReloadChangedFiles();
	}
}
