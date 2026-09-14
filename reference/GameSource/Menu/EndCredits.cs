using MoreSlugcats;
using Music;
using RWCustom;
using UnityEngine;

namespace Menu;

public class EndCredits : Menu
{
	public class Stage : ExtEnum<Stage>
	{
		public static Stage InitialWait = new Stage("InitialWait", register: true);

		public static Stage RainWorldLogo = new Stage("RainWorldLogo", register: true);

		public static Stage VideoCult = new Stage("VideoCult", register: true);

		public static Stage Akupara = new Stage("Akupara", register: true);

		public static Stage Downpour = new Stage("Downpour", register: true);

		public static Stage MoreSlugcats = new Stage("MoreSlugcats", register: true);

		public static Stage Watcher = new Stage("Watcher", register: true);

		public static Stage SpecialThanks = new Stage("SpecialThanks", register: true);

		public static Stage MoreSlugcatsThanks = new Stage("MoreSlugcatsThanks", register: true);

		public static Stage WatcherThanks = new Stage("WatcherThanks", register: true);

		public static Stage BetaTesters = new Stage("BetaTesters", register: true);

		public static Stage AdultSwimGames = new Stage("AdultSwimGames", register: true);

		public static Stage CarbonGames = new Stage("CarbonGames", register: true);

		public static Stage SpecialBackers = new Stage("SpecialBackers", register: true);

		public static Stage Backers = new Stage("Backers", register: true);

		public static Stage End = new Stage("End", register: true);

		public Stage(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public RainWorldGame game;

	public float time;

	public RainEffect rainEffect;

	public FSprite blackSprite;

	public bool startedBlackSpriteFadeIn;

	public float scrollSpeed;

	public bool anyButton;

	public bool lastAnyButton;

	public bool quitToMenu;

	public bool disableAnyButtonStop;

	public float scrollInputSpeedSupression;

	public bool disableExiting;

	public CreditsObject currentCreditsObject;

	public int musicTimer = 400;

	public Stage currentStage;

	public KarmaLadderScreen.SleepDeathScreenDataPackage passthroughPackage;

	public string desiredCreditsSong;

	public EndCredits(ProcessManager manager, RainWorldGame game)
		: base(manager, ProcessManager.ProcessID.Credits)
	{
		this.game = game;
		pages.Add(new Page(this, null, "main", 0));
		currentStage = Stage.InitialWait;
		desiredCreditsSong = "";
		if (game == null)
		{
			rainEffect = new RainEffect(this, pages[0]);
			pages[0].subObjects.Add(rainEffect);
			desiredCreditsSong = manager.desiredCreditsSong;
			if (ModManager.MSC && desiredCreditsSong == "BLIZZARD")
			{
				mySoundLoopID = MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop;
				desiredCreditsSong = "";
			}
			else
			{
				mySoundLoopID = SoundID.MENU_End_Credits_LOOP;
			}
		}
		else
		{
			disableAnyButtonStop = true;
			disableExiting = true;
			scrollInputSpeedSupression = 1f;
			blackSprite = new FSprite("pixel");
			blackSprite.color = Menu.MenuRGB(MenuColors.Black);
			blackSprite.scaleX = 1400f;
			blackSprite.scaleY = 800f;
			blackSprite.x = manager.rainWorld.options.ScreenSize.x / 2f;
			blackSprite.y = manager.rainWorld.options.ScreenSize.y / 2f;
			blackSprite.alpha = 0f;
			pages[0].Container.AddChild(blackSprite);
		}
	}

	public void NextStage()
	{
		if (currentStage == Stage.End)
		{
			if (!quitToMenu)
			{
				ExitCredits();
			}
			quitToMenu = true;
			return;
		}
		DisposeCreditsObject();
		currentStage = new Stage(ExtEnum<Stage>.values.GetEntry(currentStage.Index + 1));
		if (currentStage == Stage.CarbonGames)
		{
			currentStage = new Stage(ExtEnum<Stage>.values.GetEntry(currentStage.Index + 1));
		}
		SpawnCreditsObject(startFromBottom: false);
	}

	public void PreviousStage()
	{
		if (currentStage.Index > Stage.VideoCult.Index && !(currentStage == Stage.End))
		{
			DisposeCreditsObject();
			currentStage = new Stage(ExtEnum<Stage>.values.GetEntry(currentStage.Index - 1));
			if (currentStage == Stage.CarbonGames)
			{
				currentStage = new Stage(ExtEnum<Stage>.values.GetEntry(currentStage.Index - 1));
			}
			SpawnCreditsObject(startFromBottom: true);
		}
	}

	public void DisposeCreditsObject()
	{
		if (currentCreditsObject != null)
		{
			currentCreditsObject.RemoveSprites();
			pages[0].subObjects.Remove(currentCreditsObject);
			currentCreditsObject = null;
		}
	}

	public void SpawnCreditsObject(bool startFromBottom)
	{
		if (blackSprite != null && currentStage == Stage.Downpour)
		{
			startedBlackSpriteFadeIn = true;
		}
		if (currentStage == Stage.RainWorldLogo || currentStage == Stage.VideoCult || currentStage == Stage.AdultSwimGames || currentStage == Stage.Akupara || currentStage == Stage.CarbonGames || currentStage == Stage.SpecialThanks || currentStage == Stage.BetaTesters || currentStage == Stage.Downpour || currentStage == Stage.Watcher || currentStage == Stage.WatcherThanks || currentStage == Stage.MoreSlugcats || currentStage == Stage.MoreSlugcatsThanks)
		{
			currentCreditsObject = new CreditsTextAndImage(this, pages[0], currentStage, startFromBottom);
			pages[0].subObjects.Add(currentCreditsObject);
		}
		else if (currentStage == Stage.SpecialBackers)
		{
			currentCreditsObject = new LongScrollingCredits(this, pages[0], currentStage, 1, "05 - SLUGCAT FAM", "EXTRA SPECIAL THANKS TO OUR SLUGCAT FAMILY...", startFromBottom);
			pages[0].subObjects.Add(currentCreditsObject);
		}
		else if (currentStage == Stage.Backers)
		{
			currentCreditsObject = new LongScrollingCredits(this, pages[0], currentStage, (Custom.rainWorld.options.ScreenSize.x < 1360f) ? 4 : 5, "06 - BACKERS", "SPECIAL THANKS TO OUR BACKERS!\r\nWITHOUT YOUR ENTHUSIASM AND PATIENCE, NONE OF THIS WOULD HAVE BEEN POSSIBLE...", startFromBottom);
			(currentCreditsObject as LongScrollingCredits).slowDownZone = 80f;
			pages[0].subObjects.Add(currentCreditsObject);
		}
	}

	public override void Update()
	{
		base.Update();
		anyButton = false;
		if (!disableAnyButtonStop)
		{
			for (int i = 0; i < manager.rainWorld.options.controls.Length; i++)
			{
				anyButton = anyButton || manager.rainWorld.options.controls[i].GetAnyButton();
			}
		}
		if (desiredCreditsSong != "" && musicTimer > 0 && manager.musicPlayer != null && (manager.musicPlayer.song == null || !(manager.musicPlayer.song is IntroRollMusic)))
		{
			musicTimer--;
			if (musicTimer == 0)
			{
				manager.musicPlayer.MenuRequestsSong(desiredCreditsSong, 1.4f, 0f);
			}
		}
		if (currentCreditsObject != null)
		{
			if (input.y > 0)
			{
				scrollSpeed = Custom.LerpAndTick(scrollSpeed, Mathf.Lerp(-4f, currentCreditsObject.CurrentDefaultScrollSpeed, scrollInputSpeedSupression), 0.12f, 1f / 11f);
			}
			else if (input.y < 0)
			{
				scrollSpeed = Custom.LerpAndTick(scrollSpeed, Mathf.Lerp(16f, currentCreditsObject.CurrentDefaultScrollSpeed, scrollInputSpeedSupression), 0.12f, 1f / 11f);
			}
			else
			{
				scrollSpeed = Custom.LerpAndTick(scrollSpeed, anyButton ? 0f : currentCreditsObject.CurrentDefaultScrollSpeed, 0.12f, 1f / 11f);
			}
			if (currentCreditsObject.OutOfScreen)
			{
				NextStage();
			}
			else if (currentCreditsObject.BeforeScreen && scrollSpeed < 0f)
			{
				PreviousStage();
			}
		}
		else
		{
			NextStage();
		}
		if (!quitToMenu && RWInput.CheckPauseButton(0, inMenu: false) && !disableExiting)
		{
			quitToMenu = true;
			ExitCredits();
		}
		lastAnyButton = anyButton;
		if (time > 14f && Random.value < 1f / 160f && rainEffect != null)
		{
			rainEffect.LightningSpike(Mathf.Pow(Random.value, 2f) * 0.85f, Mathf.Lerp(20f, 120f, Random.value));
		}
	}

	public override void RawUpdate(float dt)
	{
		base.RawUpdate(dt);
		time += dt;
		if (rainEffect != null)
		{
			rainEffect.rainFade = Custom.SCurve(Mathf.InverseLerp(0f, 6f, time), 0.8f) * 0.5f;
		}
	}

	public override void GrafUpdate(float timeStacker)
	{
		if (blackSprite != null)
		{
			if (startedBlackSpriteFadeIn)
			{
				blackSprite.alpha = Mathf.Min(0.8f, blackSprite.alpha + 0.0025f);
				game.cameras[0].virtualMicrophone.globalSoundMuffle = Mathf.Min(1f, game.cameras[0].virtualMicrophone.globalSoundMuffle + 0.0025f);
				scrollInputSpeedSupression = Mathf.Max(0f, scrollInputSpeedSupression - 0.0003125f);
				if (disableExiting && blackSprite.alpha == 0.8f)
				{
					manager.statsAfterCredits = true;
					manager.rainWorld.progression.miscProgressionData.watcherEndingID = 4;
					RainWorldGame.BeatGameMode(game, standardVoidSea: false);
					disableExiting = false;
				}
			}
			else if (blackSprite.alpha < 0.4f)
			{
				blackSprite.alpha = Mathf.Min(1f, blackSprite.alpha + 0.00025f);
			}
		}
		base.GrafUpdate(timeStacker);
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (passthroughPackage != null && nextProcess is KarmaLadderScreen)
		{
			(nextProcess as KarmaLadderScreen).GetDataFromGame(passthroughPackage);
		}
	}

	public void ExitCredits()
	{
		if (manager.statsAfterCredits)
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Statistics);
		}
		else
		{
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
		}
	}
}
