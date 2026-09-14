using System;
using System.Text;
using AOT;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	public static bool s_EverInitialized;

	public static SteamManager s_instance;

	public bool m_bInitialized;

	public SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	public static SteamManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return s_instance;
		}
	}

	public static bool Initialized => Instance.m_bInitialized;

	[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	public static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void InitOnPlayMode()
	{
		s_EverInitialized = false;
		s_instance = null;
	}

	public virtual void Awake()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
		if (s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		Packsize.Test();
		DllCheck.Test();
		try
		{
			if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException)
		{
			Application.Quit();
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (m_bInitialized)
		{
			s_EverInitialized = true;
		}
	}

	public virtual void OnEnable()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	public virtual void OnDestroy()
	{
		if (!(s_instance != this))
		{
			s_instance = null;
			if (m_bInitialized)
			{
				SteamAPI.Shutdown();
			}
		}
	}

	public virtual void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}
}
