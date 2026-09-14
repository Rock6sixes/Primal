using System;
using System.Collections;
using System.Collections.Generic;
using Expedition;
using Kittehface.Framework20;
using Menu;
using RWCustom;
using Rewired;
using UnityEngine;

public class ControllerHandler : MonoBehaviour
{
	public class RumbleState : ExtEnum<RumbleState>
	{
		public static RumbleState Off = new RumbleState("Off", register: true);

		public static RumbleState Rumbling = new RumbleState("Rumbling", register: true);

		public static RumbleState Pulsing = new RumbleState("Pulsing", register: true);

		public static RumbleState StartingRest = new RumbleState("StartingRest", register: true);

		public static RumbleState Resting = new RumbleState("Resting", register: true);

		public static RumbleState EndingRest = new RumbleState("EndingRest", register: true);

		public RumbleState(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public const int MAXRUMBLES = 3;

	public const int DEFAULT_FLICKERAMOUNT = 3;

	public const float MAXRUMBLETIMES = 45f;

	public const float RUMBLE_DELAY_TIME_S = 2f;

	public const float RUMBLE_CHANGE_RATE = 2f;

	public const float DEFAULT_PULSEINCREMENT = 1f / 160f;

	public const float FINALPULSEINCREMENT = 0.1f;

	public const float VIBRATION_RANGE_ADJUST = 0.8f;

	public PlayerHandler playerHandler;

	public bool playerDead;

	public bool isFlickering;

	public bool warningSetUp;

	public bool fadeIn;

	public float pulseIncrement;

	public RumbleState rumbleState = RumbleState.Off;

	public float targetRumbleAmount;

	public float pulseRumbleAmount;

	public float pulseRumbleTimeS;

	public float pulseRumbleStartTimeS;

	public float rumbleAmount;

	public float rumbleStartTimeS;

	public float rumbleRestStartTimeS;

	public bool rumblePaused;

	public bool rumbleNeedsUpdate = true;

	public bool requestPulseRumble;

	public int cycleLength;

	public int hudCirclesRemaining;

	public int timesToRumble;

	public int pipSize = 1200;

	public float lightBarColorPercentage = 1f;

	public bool needsInitialize = true;

	public SlugcatStats.Name lastSlugcat = SlugcatStats.Name.White;

	public bool lastRoomInDanger = true;

	public static bool configurationEventWired = false;

	public static List<ControllerHandler> controllerHandlers = new List<ControllerHandler>();

	public void Awake()
	{
		playerHandler = GetComponent<PlayerHandler>();
		pulseIncrement = 1f / 160f;
		if (!configurationEventWired)
		{
			configurationEventWired = true;
			UserInput.OnControllerConfigurationChanged += UserInput_OnControllerConfigurationChanged;
		}
		controllerHandlers.Add(this);
	}

	public void OnDestroy()
	{
		controllerHandlers.Remove(this);
	}

	public void Update()
	{
		if (playerHandler.rainWorld.processManager != null && playerHandler.rainWorld.progression != null)
		{
			if (needsInitialize && playerHandler.initialized)
			{
				needsInitialize = false;
				lightBarColorPercentage = 1f;
				lastSlugcat = GetCurrentSlugcat();
				SetLightBarColour(PlayerGraphics.SlugcatColor(lastSlugcat));
				StopRumble();
			}
			UpdateRumble();
		}
	}

	public void ResetHandler(bool isPlayerDead)
	{
		if (playerHandler.initialized)
		{
			playerDead = isPlayerDead;
			lightBarColorPercentage = 1f;
			lastSlugcat = GetCurrentSlugcat();
			SetLightBarColour(PlayerGraphics.SlugcatColor(lastSlugcat));
			StopRumble();
			targetRumbleAmount = 0f;
			pulseRumbleAmount = 0f;
			pulseRumbleTimeS = 0f;
			rumblePaused = false;
			rumbleNeedsUpdate = false;
			requestPulseRumble = false;
		}
	}

	public void WarningSetUp(int cycleLength)
	{
		if (playerHandler.initialized)
		{
			ResetHandler(isPlayerDead: false);
			lastRoomInDanger = true;
			this.cycleLength = cycleLength;
			hudCirclesRemaining = cycleLength / pipSize;
			timesToRumble = 3;
			pulseIncrement = 1f / 160f;
			warningSetUp = true;
		}
	}

	public void OnPlayerDeath()
	{
		if (playerHandler.initialized)
		{
			warningSetUp = false;
			ResetHandler(isPlayerDead: true);
		}
	}

	public void UpdateWarning(int timer, int timeUntilRain, bool roomInDanger, bool rumbleActive)
	{
		if (!playerHandler.initialized || !warningSetUp)
		{
			return;
		}
		hudCirclesRemaining = timeUntilRain / pipSize;
		if (timeUntilRain <= cycleLength / 2 && roomInDanger)
		{
			PulseLightBar();
		}
		if (timeUntilRain <= 0)
		{
			pulseIncrement = 0.1f;
		}
		if (hudCirclesRemaining <= timesToRumble && timesToRumble > 0)
		{
			pulseIncrement *= 2f;
			timesToRumble--;
			if (roomInDanger && rumbleActive)
			{
				TriggerWarningRumble();
			}
		}
		if (roomInDanger != lastRoomInDanger)
		{
			lastRoomInDanger = roomInDanger;
			if (!roomInDanger)
			{
				ResetHandler(playerDead);
			}
		}
	}

	[Obsolete("Use four parameter function instead.")]
	public void UpdateWarning(int timer, int timeUntilRain, bool roomInDanger)
	{
		UpdateWarning(timer, timeUntilRain, roomInDanger, rumbleActive: true);
	}

	public void SetRumblePaused(bool paused)
	{
		if (!paused && rumblePaused)
		{
			rumbleStartTimeS = Time.time;
			rumbleRestStartTimeS = 0f;
			if (rumbleState == RumbleState.StartingRest || rumbleState == RumbleState.Resting || rumbleState == RumbleState.EndingRest)
			{
				rumbleState = RumbleState.Rumbling;
			}
		}
		rumblePaused = paused;
		if (rumblePaused)
		{
			rumbleAmount = 0f;
			StopRumbleInternal();
		}
	}

	public SlugcatStats.Name GetCurrentSlugcat()
	{
		if (needsInitialize || playerHandler.playerIndex > 0)
		{
			return SlugcatStats.Name.ArenaColor(playerHandler.playerIndex);
		}
		MainLoopProcess currentMainLoop = playerHandler.rainWorld.processManager.currentMainLoop;
		if (currentMainLoop is MultiplayerMenu || currentMainLoop is MultiplayerResults)
		{
			return SlugcatStats.Name.White;
		}
		if (currentMainLoop is SlugcatSelectMenu)
		{
			SlugcatSelectMenu slugcatSelectMenu = currentMainLoop as SlugcatSelectMenu;
			return slugcatSelectMenu.slugcatPages[slugcatSelectMenu.slugcatPageIndex].slugcatNumber;
		}
		if (currentMainLoop is RainWorldGame)
		{
			RainWorldGame rainWorldGame = currentMainLoop as RainWorldGame;
			if (rainWorldGame.session is CompetitiveGameSession || rainWorldGame.session is ArenaGameSession)
			{
				return SlugcatStats.Name.White;
			}
			return rainWorldGame.StoryCharacter;
		}
		return playerHandler.rainWorld.progression.PlayingAsSlugcat;
	}

	public void StopRumble()
	{
		if (playerHandler.initialized)
		{
			StopRumbleInternal();
			rumbleState = RumbleState.Off;
			rumbleAmount = 0f;
			pulseRumbleStartTimeS = 0f;
			rumbleRestStartTimeS = 0f;
		}
	}

	public void TriggerWarningRumble()
	{
		if (rumbleState == RumbleState.Off || rumbleState == RumbleState.Rumbling)
		{
			requestPulseRumble = true;
			pulseRumbleAmount = 0.2f;
			pulseRumbleTimeS = 1.5f;
		}
		FlickerLightBar();
	}

	public void AttemptScreenShakeRumble(float screenShakeAmount)
	{
		if (playerHandler.initialized && !playerDead)
		{
			if (isFlickering)
			{
				FlickerLightBar();
			}
			targetRumbleAmount = screenShakeAmount;
			rumbleNeedsUpdate = true;
		}
	}

	public void UpdateRumble()
	{
		if (!playerHandler.rainWorld.options.vibration)
		{
			if (playerHandler.initialized && rumbleState != RumbleState.Off)
			{
				StopRumble();
			}
			return;
		}
		if (rumbleState == RumbleState.Off)
		{
			if (requestPulseRumble)
			{
				requestPulseRumble = false;
				if (!rumblePaused && pulseRumbleAmount > 0f)
				{
					rumbleState = RumbleState.Pulsing;
					pulseRumbleStartTimeS = Time.time;
					rumbleStartTimeS = Time.time;
				}
			}
			else if (!rumblePaused && targetRumbleAmount > 0f)
			{
				rumbleState = RumbleState.Rumbling;
				rumbleStartTimeS = Time.time;
			}
		}
		if (rumblePaused || !(rumbleState != RumbleState.Off))
		{
			return;
		}
		if ((rumbleState == RumbleState.Rumbling || rumbleState == RumbleState.Pulsing) && Time.time > rumbleStartTimeS + 45f)
		{
			rumbleState = RumbleState.StartingRest;
		}
		if (rumbleState == RumbleState.Pulsing)
		{
			if (Time.time > pulseRumbleStartTimeS + pulseRumbleTimeS)
			{
				rumbleState = RumbleState.Rumbling;
			}
			else if (rumbleAmount != pulseRumbleAmount)
			{
				SetRumbleAmount(pulseRumbleAmount);
			}
		}
		if (rumbleState == RumbleState.Rumbling)
		{
			if (rumbleAmount != targetRumbleAmount || rumbleNeedsUpdate)
			{
				rumbleNeedsUpdate = false;
				SetRumbleAmount(targetRumbleAmount);
				if (rumbleAmount <= 0f)
				{
					rumbleState = RumbleState.Off;
					StopRumbleInternal();
				}
			}
		}
		else if (rumbleState == RumbleState.StartingRest)
		{
			rumbleAmount -= 2f * Time.deltaTime;
			if (rumbleAmount <= 0f)
			{
				rumbleAmount = 0f;
				rumbleState = RumbleState.Resting;
				rumbleRestStartTimeS = Time.time;
			}
			SetRumbleAmount(rumbleAmount);
			if (rumbleState != RumbleState.StartingRest)
			{
				StopRumbleInternal();
			}
		}
		else if (rumbleState == RumbleState.Resting)
		{
			if (Time.time > rumbleRestStartTimeS + 2f)
			{
				rumbleState = RumbleState.EndingRest;
			}
		}
		else
		{
			if (!(rumbleState == RumbleState.EndingRest))
			{
				return;
			}
			if (requestPulseRumble)
			{
				requestPulseRumble = false;
				if (pulseRumbleAmount > 0f)
				{
					rumbleState = RumbleState.Pulsing;
					pulseRumbleStartTimeS = Time.time;
					rumbleStartTimeS = Time.time;
				}
			}
			if (!(rumbleState == RumbleState.EndingRest))
			{
				return;
			}
			if (targetRumbleAmount <= 0f)
			{
				rumbleState = RumbleState.Off;
				rumbleAmount = 0f;
				rumbleNeedsUpdate = false;
				StopRumbleInternal();
				return;
			}
			rumbleAmount += 2f * Time.deltaTime;
			if (rumbleAmount >= targetRumbleAmount)
			{
				rumbleAmount = targetRumbleAmount;
				rumbleStartTimeS = Time.time;
				rumbleState = RumbleState.Rumbling;
			}
			SetRumbleAmount(rumbleAmount);
		}
	}

	public void SetRumbleAmount(float rumbleAmount)
	{
		this.rumbleAmount = rumbleAmount;
		float num = Mathf.Clamp01(rumbleAmount) * 0.8f;
		if (num > 0f && num < 0.1f)
		{
			num = 0.1f;
		}
		if (playerHandler.profile == null)
		{
			return;
		}
		Player val = UserInput.GetRewiredPlayer(playerHandler.profile, playerHandler.playerIndex);
		if (val == null && playerHandler.playerIndex < playerHandler.rainWorld.options.controls.Length)
		{
			val = playerHandler.rainWorld.options.controls[playerHandler.playerIndex].player;
		}
		if (val == null)
		{
			return;
		}
		foreach (Joystick joystick in val.controllers.Joysticks)
		{
			if (joystick.supportsVibration)
			{
				joystick.SetVibration(num, num);
			}
		}
	}

	public void StopRumbleInternal()
	{
		if (playerHandler.profile == null)
		{
			return;
		}
		Player val = UserInput.GetRewiredPlayer(playerHandler.profile, playerHandler.playerIndex);
		if (val == null && playerHandler.playerIndex < playerHandler.rainWorld.options.controls.Length)
		{
			val = playerHandler.rainWorld.options.controls[playerHandler.playerIndex].player;
		}
		if (val == null)
		{
			return;
		}
		foreach (Joystick joystick in val.controllers.Joysticks)
		{
			if (joystick.supportsVibration)
			{
				joystick.StopVibration();
			}
		}
	}

	public static void UserInput_OnControllerConfigurationChanged()
	{
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			bool flag = false;
			foreach (ControllerHandler controllerHandler in controllerHandlers)
			{
				if (controllerHandler.playerHandler != null && controllerHandler.playerHandler.profile != null && UserInput.GetJoystickProfile(joystick) == controllerHandler.playerHandler.profile)
				{
					flag = true;
					controllerHandler.rumbleNeedsUpdate = true;
					break;
				}
			}
			if (!flag && joystick.supportsVibration)
			{
				joystick.StopVibration();
			}
		}
		if (Custom.rainWorld.options != null && Custom.rainWorld.options.optionsLoaded)
		{
			Custom.rainWorld.options.ReassignAllJoysticks();
		}
		else
		{
			UserInput.RefreshConsoleControllerMap();
		}
	}

	public void SetLightBarColour(Color colour)
	{
		_ = playerHandler.initialized;
	}

	public void PulseLightBar()
	{
		if (!fadeIn && lightBarColorPercentage > 0f)
		{
			lightBarColorPercentage -= pulseIncrement;
			Color lightBarColour = PlayerGraphics.SlugcatColor(lastSlugcat);
			lightBarColour.r *= Mathf.Max(0f, lightBarColorPercentage);
			lightBarColour.g *= Mathf.Max(0f, lightBarColorPercentage);
			lightBarColour.b *= Mathf.Max(0f, lightBarColorPercentage);
			SetLightBarColour(lightBarColour);
			if (lightBarColorPercentage <= 0f)
			{
				fadeIn = true;
			}
		}
		if (fadeIn && lightBarColorPercentage < 1f)
		{
			lightBarColorPercentage += pulseIncrement;
			Color lightBarColour2 = PlayerGraphics.SlugcatColor(lastSlugcat);
			lightBarColour2.r *= Mathf.Min(1f, lightBarColorPercentage);
			lightBarColour2.g *= Mathf.Min(1f, lightBarColorPercentage);
			lightBarColour2.b *= Mathf.Min(1f, lightBarColorPercentage);
			SetLightBarColour(lightBarColour2);
			if (lightBarColorPercentage >= 1f)
			{
				fadeIn = false;
			}
		}
	}

	public void FlickerLightBar()
	{
		StartCoroutine(DoFlicker());
	}

	public IEnumerator DoFlicker()
	{
		isFlickering = true;
		PlayerGraphics.SlugcatColor(ExpeditionData.slugcatPlayer);
		for (int i = 0; i < 3; i++)
		{
			SetLightBarColour(Color.black);
			yield return new WaitForSeconds(0.25f);
			Color lightBarColour = PlayerGraphics.SlugcatColor(lastSlugcat);
			lightBarColour.r *= lightBarColorPercentage;
			lightBarColour.g *= lightBarColorPercentage;
			lightBarColour.b *= lightBarColorPercentage;
			SetLightBarColour(lightBarColour);
		}
		isFlickering = false;
	}
}
