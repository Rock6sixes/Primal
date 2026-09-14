using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Menu;
using On;
using On.Menu;

namespace SlugBase.Assets;

/// <summary>
/// Helpers for adding custom dreams.
/// </summary>
public static class CustomDreams
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static hook_InitiateEventDream _003C0_003E__DreamsState_InitiateEventDream;

		public static hook_SceneFromDream _003C1_003E__DreamScreen_SceneFromDream;
	}

	private static readonly Dictionary<DreamID, SceneID> _dreamScenes = new Dictionary<DreamID, SceneID>();

	public static void SetDreamScene(DreamID dream, SceneID scene)
	{
		if ((ExtEnum<DreamID>)(object)dream == (ExtEnum<DreamID>)null)
		{
			throw new ArgumentNullException("dream");
		}
		if ((ExtEnum<SceneID>)(object)scene == (ExtEnum<SceneID>)null)
		{
			_dreamScenes.Remove(dream);
		}
		else
		{
			_dreamScenes[dream] = scene;
		}
	}

	public static void QueueDream(StoryGameSession storySession, DreamID dreamID)
	{
		if (!_dreamScenes.ContainsKey(dreamID))
		{
			throw new ArgumentException("dreamID must be registered with RegisterDream before use!", "dreamID");
		}
		if (storySession == null)
		{
			throw new ArgumentNullException("storySession");
		}
		if ((ExtEnum<DreamID>)(object)dreamID == (ExtEnum<DreamID>)null)
		{
			throw new ArgumentNullException("dreamID");
		}
		DreamsState val = storySession.saveState?.dreamsState;
		if (val == null)
		{
			throw new ArgumentException("The current save state doesn't have a dream state!", "storySession");
		}
		val.InitiateEventDream(dreamID);
	}

	internal static void Apply()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		object obj = _003C_003EO._003C0_003E__DreamsState_InitiateEventDream;
		if (obj == null)
		{
			hook_InitiateEventDream val = DreamsState_InitiateEventDream;
			_003C_003EO._003C0_003E__DreamsState_InitiateEventDream = val;
			obj = (object)val;
		}
		DreamsState.InitiateEventDream += (hook_InitiateEventDream)obj;
		object obj2 = _003C_003EO._003C1_003E__DreamScreen_SceneFromDream;
		if (obj2 == null)
		{
			hook_SceneFromDream val2 = DreamScreen_SceneFromDream;
			_003C_003EO._003C1_003E__DreamScreen_SceneFromDream = val2;
			obj2 = (object)val2;
		}
		DreamScreen.SceneFromDream += (hook_SceneFromDream)obj2;
	}

	private static void DreamsState_InitiateEventDream(orig_InitiateEventDream orig, DreamsState self, DreamID evDreamID)
	{
		SceneID value;
		CustomScene value2;
		bool flag = (ExtEnum<DreamID>)(object)self.eventDream != (ExtEnum<DreamID>)null && _dreamScenes.TryGetValue(self.eventDream, out value) && CustomScene.Registry.TryGet(value, out value2) && value2.OverrideDream;
		bool flag2 = (ExtEnum<DreamID>)(object)evDreamID != (ExtEnum<DreamID>)null && _dreamScenes.TryGetValue(evDreamID, out value) && CustomScene.Registry.TryGet(value, out value2) && value2.OverrideDream;
		if (!flag || flag2)
		{
			orig.Invoke(self, evDreamID);
		}
	}

	private static SceneID DreamScreen_SceneFromDream(orig_SceneFromDream orig, DreamScreen self, DreamID dreamID)
	{
		SceneID value;
		return _dreamScenes.TryGetValue(dreamID, out value) ? value : orig.Invoke(self, dreamID);
	}
}
