using System.Collections;
using Kittehface.Framework20;
using RWCustom;
using Rewired;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
	public ControllerHandler controllerHandler;

	public bool signingIn;

	public bool pendingSignIn;

	public RainWorld rainWorld { get; set; }

	public bool initialized => profile != null;

	public int playerIndex { get; set; }

	public Profile profile { get; set; }

	public bool SigningIn
	{
		get
		{
			if (!signingIn)
			{
				return pendingSignIn;
			}
			return true;
		}
	}

	public ControllerHandler ControllerHandler
	{
		get
		{
			if (controllerHandler == null)
			{
				controllerHandler = GetComponent<ControllerHandler>();
			}
			return controllerHandler;
		}
	}

	public Joystick requestedJoystick { get; set; }

	public void Initialize(RainWorld rainWorld, int playerIndex, Profile profile)
	{
		if (this.profile == null)
		{
			this.rainWorld = rainWorld;
			this.playerIndex = playerIndex;
			this.profile = profile;
		}
	}

	public void RequestSignIn(Joystick requestJoystick)
	{
		if (signingIn)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			if (i != playerIndex)
			{
				PlayerHandler playerHandlerRaw = rainWorld.GetPlayerHandlerRaw(i);
				if (playerHandlerRaw != null && playerHandlerRaw.signingIn)
				{
					pendingSignIn = true;
					requestedJoystick = requestJoystick;
					StartCoroutine(DoPendingSignIn());
					return;
				}
			}
		}
		signingIn = true;
		Profiles.OnSignedIn += Profiles_OnSignedIn;
		if (requestJoystick == null)
		{
			Profiles.RequestSignIn((object[])null);
			return;
		}
		Profiles.RequestSignIn(new object[1] { ((Controller)requestJoystick).id });
	}

	public void Deactivate()
	{
		if (playerIndex != 0 && playerIndex != -1)
		{
			Custom.LogImportant("Deactivating player!!!", (profile == null) ? "NULL" : profile.GetDisplayName(), "playerindex:", playerIndex.ToString());
			if (profile != null && !Custom.rainWorld.IsPrimaryProfile(profile))
			{
				profile.Deactivate();
			}
			profile = null;
			signingIn = false;
			pendingSignIn = false;
			requestedJoystick = null;
		}
	}

	public void Awake()
	{
		Profiles.OnWillDeactivate += Profiles_OnWillDeactivate;
		Profiles.OnDeactivated += Profiles_OnDeactivated;
	}

	public void OnDestroy()
	{
		Profiles.OnWillDeactivate -= Profiles_OnWillDeactivate;
		Profiles.OnDeactivated -= Profiles_OnDeactivated;
	}

	public IEnumerator DoPendingSignIn()
	{
		bool flag = true;
		while (flag)
		{
			yield return 0;
			flag = false;
			for (int i = 0; i < 4; i++)
			{
				if (i != playerIndex)
				{
					PlayerHandler playerHandlerRaw = rainWorld.GetPlayerHandlerRaw(i);
					if (playerHandlerRaw != null && playerHandlerRaw.signingIn)
					{
						flag = true;
						break;
					}
				}
			}
		}
		pendingSignIn = false;
		Joystick val = requestedJoystick;
		requestedJoystick = null;
		signingIn = true;
		Profiles.OnSignedIn += Profiles_OnSignedIn;
		Profiles.RequestSignIn(new object[1] { (val != null) ? val.systemId : null });
	}

	public void Profiles_OnSignedIn(Profile profile, SignInResult result)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Profiles.OnSignedIn -= Profiles_OnSignedIn;
		if (signingIn)
		{
			signingIn = false;
			if ((int)result == 0)
			{
				this.profile = profile;
				ControllerHandler.ResetHandler(isPlayerDead: false);
			}
		}
	}

	public void Profiles_OnWillDeactivate(Profile profile)
	{
		if (this.profile == profile)
		{
			signingIn = false;
			pendingSignIn = false;
			requestedJoystick = null;
		}
	}

	public void Profiles_OnDeactivated(Profile profile)
	{
		if (this.profile == profile && playerIndex > 0)
		{
			Custom.LogImportant("Deactivating player!!!", (profile == null) ? "NULL" : profile.GetDisplayName());
			this.profile = null;
			ControllerHandler.ResetHandler(isPlayerDead: false);
		}
	}
}
