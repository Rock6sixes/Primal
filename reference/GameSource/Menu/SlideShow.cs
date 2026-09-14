using System;
using System.Collections.Generic;
using MoreSlugcats;
using UnityEngine;
using Watcher;

namespace Menu;

public class SlideShow : Menu
{
	public class SlideShowID : ExtEnum<SlideShowID>
	{
		public static SlideShowID WhiteIntro = new SlideShowID("WhiteIntro", register: true);

		public static SlideShowID YellowIntro = new SlideShowID("YellowIntro", register: true);

		public static SlideShowID WhiteOutro = new SlideShowID("WhiteOutro", register: true);

		public static SlideShowID YellowOutro = new SlideShowID("YellowOutro", register: true);

		public static SlideShowID RedOutro = new SlideShowID("RedOutro", register: true);

		public SlideShowID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Scene
	{
		public MenuScene.SceneID sceneID;

		public float startAt;

		public float fadeInDoneAt;

		public float fadeOutStartAt;

		public List<float> crossfades;

		public List<int> crossfadeDurations;

		public Scene(MenuScene.SceneID sceneID, float startAt, float fadeInDoneAt, float fadeOutStartAt)
		{
			this.sceneID = sceneID;
			this.startAt = startAt;
			this.fadeInDoneAt = fadeInDoneAt;
			this.fadeOutStartAt = fadeOutStartAt;
			crossfades = new List<float>();
			crossfadeDurations = new List<int>();
		}

		public void AddCrossFade(float timeStart, int duration)
		{
			crossfades.Add(timeStart);
			crossfadeDurations.Add(duration);
		}
	}

	public int current = -1;

	public float time;

	public List<Scene> playList;

	public SlideShowID slideShowID;

	public ProcessManager.ProcessID processAfterSlideShow;

	public string waitForMusic;

	public bool stall;

	public float inSceneTime;

	public bool exitedSlideshow;

	public SlideShowMenuScene[] preloadedScenes;

	public KarmaLadderScreen.SleepDeathScreenDataPackage endGameStatsPackage;

	public KarmaLadderScreen.SleepDeathScreenDataPackage passthroughPackage;

	public int crossfadeInd;

	public bool pauseButton;

	public bool lastPauseButton;

	public bool skip;

	public float playbackSpeedMult;

	public static bool AutobotSkipSlideShow;

	public override bool ForceNoMouseMode => true;

	public SlideShow(ProcessManager manager, SlideShowID slideShowID)
		: base(manager, ProcessManager.ProcessID.SlideShow)
	{
		this.slideShowID = slideShowID;
		pages.Add(new Page(this, null, "main", 0));
		playList = new List<Scene>();
		if (slideShowID == SlideShowID.WhiteIntro || slideShowID == SlideShowID.YellowIntro)
		{
			if (manager.musicPlayer != null)
			{
				waitForMusic = "RW_Intro_Theme";
				stall = true;
				manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Intro_1_Tree, ConvertTime(0, 0, 20), ConvertTime(0, 3, 26), ConvertTime(0, 8, 6)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 9, 6), 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Intro_2_Branch, ConvertTime(0, 9, 19), ConvertTime(0, 10, 19), ConvertTime(0, 16, 2)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_3_In_Tree, ConvertTime(0, 17, 21), ConvertTime(0, 18, 10), ConvertTime(0, 24, 3)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_4_Walking, ConvertTime(0, 24, 26), ConvertTime(0, 25, 19), ConvertTime(0, 32, 2)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 34, 6), 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Intro_5_Hunting, ConvertTime(0, 35, 50), ConvertTime(0, 36, 54), ConvertTime(0, 42, 15)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_6_7_Rain_Drop, ConvertTime(0, 43, 0), ConvertTime(0, 44, 0), ConvertTime(0, 49, 29)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_8_Climbing, ConvertTime(0, 50, 19), ConvertTime(0, 51, 9), ConvertTime(0, 55, 21)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 56, 24), 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Intro_9_Rainy_Climb, ConvertTime(0, 57, 2), ConvertTime(0, 57, 80), ConvertTime(1, 1, 1)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_10_Fall, ConvertTime(1, 1, 1), ConvertTime(1, 1, 1), ConvertTime(1, 4, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Intro_10_5_Separation, ConvertTime(1, 4, 29), ConvertTime(1, 5, 18), ConvertTime(1, 11, 10)));
			if (slideShowID == SlideShowID.WhiteIntro)
			{
				playList.Add(new Scene(MenuScene.SceneID.Intro_11_Drowning, ConvertTime(1, 11, 25), ConvertTime(1, 12, 10), ConvertTime(1, 17, 28)));
				playList.Add(new Scene(MenuScene.SceneID.Intro_12_Waking, ConvertTime(1, 19, 2), ConvertTime(1, 20, 6), ConvertTime(1, 21, 29)));
				playList.Add(new Scene(MenuScene.SceneID.Intro_13_Alone, ConvertTime(1, 22, 22), ConvertTime(1, 23, 15), ConvertTime(1, 26, 24)));
			}
			else if (slideShowID == SlideShowID.YellowIntro)
			{
				playList.Add(new Scene(MenuScene.SceneID.Yellow_Intro_A, ConvertTime(1, 11, 25), ConvertTime(1, 14, 0), ConvertTime(1, 21, 80)));
				playList.Add(new Scene(MenuScene.SceneID.Yellow_Intro_B, ConvertTime(1, 22, 60), ConvertTime(1, 23, 0), ConvertTime(1, 26, 80)));
			}
			playList.Add(new Scene(MenuScene.SceneID.Intro_14_Title, ConvertTime(1, 27, 24), ConvertTime(1, 31, 34), ConvertTime(1, 33, 60)));
			for (int i = 1; i < playList.Count; i++)
			{
				playList[i].startAt += 0.6f;
				playList[i].fadeInDoneAt += 0.6f;
				playList[i].fadeOutStartAt += 0.6f;
			}
			processAfterSlideShow = ProcessManager.ProcessID.Game;
		}
		else if (slideShowID == SlideShowID.WhiteOutro || (ModManager.MSC && (slideShowID == MoreSlugcatsEnums.SlideShowID.SpearmasterOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.InvOutro)))
		{
			if (manager.musicPlayer != null)
			{
				waitForMusic = "RW_Outro_Theme";
				stall = true;
				manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Outro_1_Left_Swim, ConvertTime(0, 1, 20), ConvertTime(0, 5, 0), ConvertTime(0, 17, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_2_Up_Swim, ConvertTime(0, 21, 0), ConvertTime(0, 25, 0), ConvertTime(0, 37, 0)));
			if (slideShowID == SlideShowID.WhiteOutro)
			{
				playList.Add(new Scene(MenuScene.SceneID.Outro_3_Face, ConvertTime(0, 41, 10), ConvertTime(0, 45, 20), ConvertTime(0, 46, 60)));
				playList.Add(new Scene(MenuScene.SceneID.Outro_4_Tree, ConvertTime(0, 48, 20), ConvertTime(0, 51, 0), ConvertTime(0, 55, 0)));
			}
			else
			{
				playList.Add(new Scene(MenuScene.SceneID.Outro_3_Face, ConvertTime(0, 41, 10), ConvertTime(0, 51, 20), ConvertTime(0, 55, 60)));
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(1, 1, 0), ConvertTime(1, 1, 0), ConvertTime(1, 6, 0)));
			for (int j = 1; j < playList.Count; j++)
			{
				playList[j].startAt -= 1.1f;
				playList[j].fadeInDoneAt -= 1.1f;
				playList[j].fadeOutStartAt -= 1.1f;
			}
			if (ModManager.MSC && slideShowID == MoreSlugcatsEnums.SlideShowID.InvOutro)
			{
				processAfterSlideShow = MoreSlugcatsEnums.ProcessID.DatingSim;
			}
			else
			{
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
			if (ModManager.MSC)
			{
				manager.statsAfterCredits = true;
			}
		}
		else if (slideShowID == SlideShowID.YellowOutro)
		{
			if (manager.musicPlayer != null)
			{
				waitForMusic = "RW_Outro_Theme";
				stall = true;
				manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Outro_1_Left_Swim, ConvertTime(0, 1, 20), ConvertTime(0, 5, 0), ConvertTime(0, 17, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_2_Up_Swim, ConvertTime(0, 21, 0), ConvertTime(0, 25, 0), ConvertTime(0, 27, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Monk_1_Swim, ConvertTime(0, 30, 0), ConvertTime(0, 35, 0), ConvertTime(0, 37, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Monk_2_Reach, ConvertTime(0, 41, 10), ConvertTime(0, 45, 20), ConvertTime(0, 46, 60)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Monk_3_Stop, ConvertTime(0, 48, 20), ConvertTime(0, 51, 0), ConvertTime(0, 55, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(1, 1, 0), ConvertTime(1, 1, 0), ConvertTime(1, 6, 0)));
			for (int k = 1; k < playList.Count; k++)
			{
				playList[k].startAt -= 1.1f;
				playList[k].fadeInDoneAt -= 1.1f;
				playList[k].fadeOutStartAt -= 1.1f;
			}
			processAfterSlideShow = ProcessManager.ProcessID.Credits;
			if (ModManager.MSC)
			{
				manager.statsAfterCredits = true;
			}
		}
		else if (slideShowID == SlideShowID.RedOutro)
		{
			if (manager.musicPlayer != null)
			{
				waitForMusic = "RW_Outro_Theme";
				stall = true;
				manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
			}
			playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
			playList.Add(new Scene(MenuScene.SceneID.Outro_1_Left_Swim, ConvertTime(0, 1, 20), ConvertTime(0, 5, 0), ConvertTime(0, 17, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_2_Up_Swim, ConvertTime(0, 21, 0), ConvertTime(0, 25, 0), ConvertTime(0, 27, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Hunter_1_Swim, ConvertTime(0, 30, 0), ConvertTime(0, 35, 0), ConvertTime(0, 37, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Hunter_2_Sink, ConvertTime(0, 41, 10), ConvertTime(0, 45, 20), ConvertTime(0, 46, 60)));
			playList.Add(new Scene(MenuScene.SceneID.Outro_Hunter_3_Embrace, ConvertTime(0, 48, 20), ConvertTime(0, 51, 0), ConvertTime(0, 55, 0)));
			playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(1, 1, 0), ConvertTime(1, 1, 0), ConvertTime(1, 6, 0)));
			for (int l = 1; l < playList.Count; l++)
			{
				playList[l].startAt -= 1.1f;
				playList[l].fadeInDoneAt -= 1.1f;
				playList[l].fadeOutStartAt -= 1.1f;
			}
			processAfterSlideShow = ProcessManager.ProcessID.Credits;
			if (ModManager.MSC)
			{
				manager.statsAfterCredits = true;
			}
		}
		if (ModManager.MSC)
		{
			if (slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerOutro)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_Outro_Theme";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 8, 0)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer2, ConvertTime(0, 10, 0), ConvertTime(0, 13, 0), ConvertTime(0, 17, 0)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer3, ConvertTime(0, 21, 10), ConvertTime(0, 25, 20), ConvertTime(0, 37, 0)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer4, ConvertTime(0, 41, 10), ConvertTime(0, 45, 20), ConvertTime(0, 46, 60)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Artificer5, ConvertTime(0, 48, 20), ConvertTime(0, 51, 0), ConvertTime(0, 55, 60)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(1, 1, 0), ConvertTime(1, 1, 0), ConvertTime(1, 6, 0)));
				for (int m = 1; m < playList.Count; m++)
				{
					playList[m].startAt -= 1.1f;
					playList[m].fadeInDoneAt -= 1.1f;
					playList[m].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
				manager.statsAfterCredits = true;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.SaintIntro)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "BM_SS_DOOR";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Intro_S1, ConvertTime(0, 0, 20), ConvertTime(0, 3, 26), ConvertTime(0, 6, 26)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Intro_S2, ConvertTime(0, 10, 26), ConvertTime(0, 15, 0), ConvertTime(0, 20, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 21, 0), 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Intro_S4, ConvertTime(0, 21, 15), ConvertTime(0, 24, 0), ConvertTime(0, 27, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 28, 0), ConvertTime(0, 28, 0), ConvertTime(0, 30, 0)));
				for (int n = 1; n < playList.Count; n++)
				{
					playList[n].startAt += 0.6f;
					playList[n].fadeInDoneAt += 0.6f;
					playList[n].fadeOutStartAt += 0.6f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Game;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					if (manager.pebblesHasHalcyon)
					{
						waitForMusic = "NA_19 - Halcyon Memories";
					}
					else
					{
						waitForMusic = "NA_43 - Isolation";
					}
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				int num = 13;
				int num2 = 2;
				int num3 = 5;
				int num4 = 7;
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Rivulet1, ConvertTime(0, 0, 20), ConvertTime(0, 3, 26), ConvertTime(0, num, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, num + num2, 0), 0f, 0f));
				Scene scene = new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Rivulet2L0, ConvertTime(0, num + num2, 15), ConvertTime(0, num + num2 + 4, 15), ConvertTime(0, num + num2 + num3 + num4 + 3, 0));
				scene.AddCrossFade(ConvertTime(0, num + num2 + num3, 15), 20);
				scene.AddCrossFade(ConvertTime(0, num + num2 + num3 + 1, 85), 20);
				scene.AddCrossFade(ConvertTime(0, num + num2 + num3 + 3, 55), 20);
				playList.Add(scene);
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, num + num2 + num3 + num4 + 4, 0), ConvertTime(0, num + num2 + num3 + num4 + 4, 15), ConvertTime(0, num + num2 + num3 + num4 + 6, 0)));
				for (int num5 = 1; num5 < playList.Count; num5++)
				{
					playList[num5].startAt += 0.6f;
					playList[num5].fadeInDoneAt += 0.6f;
					playList[num5].fadeOutStartAt += 0.6f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
				manager.statsAfterCredits = true;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "BM_CC_CANOPY";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 7, 15)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_2, ConvertTime(0, 9, 15), ConvertTime(0, 12, 15), ConvertTime(0, 15, 0)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Artificer_3, ConvertTime(0, 19, 0), ConvertTime(0, 23, 20), ConvertTime(0, 26, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 29, 0), ConvertTime(0, 29, 0), ConvertTime(0, 33, 0)));
				for (int num6 = 1; num6 < playList.Count; num6++)
				{
					playList[num6].startAt -= 1.1f;
					playList[num6].fadeInDoneAt -= 1.1f;
					playList[num6].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.SurvivorAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_Outro_Theme_B";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 16, 2)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_2, ConvertTime(0, 17, 21), ConvertTime(0, 18, 10), ConvertTime(0, 32, 2)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_4, ConvertTime(0, 33, 21), ConvertTime(0, 34, 10), ConvertTime(0, 50, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 53, 0), ConvertTime(0, 53, 0), ConvertTime(0, 57, 0)));
				for (int num7 = 1; num7 < playList.Count; num7++)
				{
					playList[num7].startAt -= 1.1f;
					playList[num7].fadeInDoneAt -= 1.1f;
					playList[num7].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.MonkAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_Outro_Theme_B";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 16, 2)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_3, ConvertTime(0, 17, 21), ConvertTime(0, 18, 10), ConvertTime(0, 32, 2)));
				playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.AltEnd_Vanilla_4, ConvertTime(0, 33, 21), ConvertTime(0, 34, 10), ConvertTime(0, 50, 0)));
				playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 53, 0), ConvertTime(0, 53, 0), ConvertTime(0, 57, 0)));
				for (int num8 = 1; num8 < playList.Count; num8++)
				{
					playList[num8].startAt -= 1.1f;
					playList[num8].fadeInDoneAt -= 1.1f;
					playList[num8].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
			else if (slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandAltEnd)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = (manager.foodTrackerCompletedThisSession ? "RW_Outro_Theme_B" : "RW_Outro_Theme_B_Short");
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 10f);
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				if (!manager.foodTrackerCompletedThisSession)
				{
					Scene scene2 = new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 32, 2));
					scene2.AddCrossFade(ConvertTime(0, 16, 2), 20);
					playList.Add(scene2);
					playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 34, 0), ConvertTime(0, 34, 0), ConvertTime(0, 36, 0)));
				}
				else
				{
					Scene scene3 = new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand1, ConvertTime(0, 1, 20), ConvertTime(0, 4, 0), ConvertTime(0, 20, 2));
					scene3.AddCrossFade(ConvertTime(0, 10, 2), 30);
					playList.Add(scene3);
					Scene scene4 = new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand2, ConvertTime(0, 21, 21), ConvertTime(0, 22, 10), ConvertTime(0, 41, 2));
					scene4.AddCrossFade(ConvertTime(0, 31, 15), 20);
					playList.Add(scene4);
					playList.Add(new Scene(MoreSlugcatsEnums.MenuSceneID.Outro_Gourmand3, ConvertTime(0, 42, 21), ConvertTime(0, 43, 10), ConvertTime(0, 59, 0)));
					playList.Add(new Scene(MenuScene.SceneID.Empty, ConvertTime(0, 61, 0), ConvertTime(0, 61, 0), ConvertTime(0, 63, 0)));
				}
				for (int num9 = 1; num9 < playList.Count; num9++)
				{
					playList[num9].startAt -= 1.1f;
					playList[num9].fadeInDoneAt -= 1.1f;
					playList[num9].fadeOutStartAt -= 1.1f;
				}
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
			}
		}
		if (ModManager.Watcher)
		{
			if (slideShowID == WatcherEnums.SlideShowID.DreamWatcher)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_161 - Childhoods End";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
				}
				float num10 = ConvertTime(0, 1, 20);
				float num11 = 1.5f;
				float num12 = 2.7f;
				float num13 = 1.5f;
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				float num14 = num10;
				for (int num15 = 0; num15 < 6; num15++)
				{
					MenuScene.SceneID sceneID = null;
					switch (num15)
					{
					case 0:
						sceneID = WatcherEnums.MenuSceneID.Dream_Watcher1;
						break;
					case 1:
						sceneID = WatcherEnums.MenuSceneID.Dream_Watcher2;
						break;
					case 2:
						sceneID = WatcherEnums.MenuSceneID.Dream_Watcher3;
						break;
					case 3:
						sceneID = WatcherEnums.MenuSceneID.Dream_Watcher4;
						break;
					case 4:
						sceneID = WatcherEnums.MenuSceneID.Dream_Watcher5;
						break;
					case 5:
						sceneID = WatcherEnums.MenuSceneID.Dream_Watcher6;
						break;
					}
					int num16;
					float num17;
					if (num15 != 0)
					{
						num16 = ((num15 == 4) ? 1 : 0);
						if (num16 == 0)
						{
							num17 = 0f;
							goto IL_1b37;
						}
					}
					else
					{
						num16 = 1;
					}
					num17 = num11 + num12 + num13;
					goto IL_1b37;
					IL_1b37:
					float num18 = num17;
					if (num15 == 5)
					{
						num18 = num12;
					}
					Scene scene5 = new Scene(sceneID, num14, num14 + num11 + num18, num14 + num11 + num12 + num18);
					if (num16 != 0)
					{
						scene5.AddCrossFade(num14 + num11 + num12 + num13, 20);
					}
					playList.Add(scene5);
					num14 += num11 + num12 + num13 + num18;
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, num14, num14, num14 + 5f));
				processAfterSlideShow = ProcessManager.ProcessID.Game;
			}
			else if (slideShowID == WatcherEnums.SlideShowID.DreamSpinningTop)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_170 - In Dreams";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
				}
				float num19 = ConvertTime(0, 1, 20);
				float num20 = 1.5f;
				float num21 = 2.7f;
				float num22 = 1.5f;
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				float num23 = num19;
				for (int num24 = 0; num24 < 7; num24++)
				{
					MenuScene.SceneID sceneID2 = null;
					switch (num24)
					{
					case 0:
						sceneID2 = WatcherEnums.MenuSceneID.Dream_SpinningTop1;
						break;
					case 1:
						sceneID2 = WatcherEnums.MenuSceneID.Dream_SpinningTop2;
						break;
					case 2:
						sceneID2 = WatcherEnums.MenuSceneID.Dream_SpinningTop3;
						break;
					case 3:
						sceneID2 = WatcherEnums.MenuSceneID.Dream_SpinningTop4;
						break;
					case 4:
						sceneID2 = WatcherEnums.MenuSceneID.Dream_SpinningTop5;
						break;
					case 5:
						sceneID2 = WatcherEnums.MenuSceneID.Dream_SpinningTop6;
						break;
					case 6:
						sceneID2 = WatcherEnums.MenuSceneID.Dream_SpinningTop7;
						break;
					}
					bool num25 = num24 == 2;
					float num26 = (num25 ? (num20 + num21 + num22) : 0f);
					Scene scene6 = new Scene(sceneID2, num23, num23 + num20 + num26, num23 + num20 + num21 + num26);
					if (num25)
					{
						scene6.AddCrossFade(num23 + num20 + num21 + num22, 20);
					}
					playList.Add(scene6);
					num23 += num20 + num21 + num22 + num26;
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, num23, num23, num23 + 5f));
				processAfterSlideShow = ProcessManager.ProcessID.SleepScreen;
			}
			else if (slideShowID == WatcherEnums.SlideShowID.DreamRot)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_170 - In Dreams";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
				}
				float num27 = ConvertTime(0, 1, 20);
				float num28 = 1f;
				float num29 = 2.56f;
				float num30 = 1f;
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				float num31 = num27;
				for (int num32 = 0; num32 < 10; num32++)
				{
					MenuScene.SceneID sceneID3 = null;
					switch (num32)
					{
					case 0:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot1;
						break;
					case 1:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot2;
						break;
					case 2:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot3;
						break;
					case 3:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot4;
						break;
					case 4:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot5;
						break;
					case 5:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot6;
						break;
					case 6:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot7;
						break;
					case 7:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot8;
						break;
					case 8:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot9;
						break;
					case 9:
						sceneID3 = WatcherEnums.MenuSceneID.Dream_Rot10;
						break;
					}
					float num33 = 0f;
					if (num32 == 9)
					{
						num33 = num29;
					}
					playList.Add(new Scene(sceneID3, num31, num31 + num28 + num33, num31 + num28 + num29 + num33));
					num31 += num28 + num29 + num30 + num33;
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, num31, num31, num31 + 5f));
				processAfterSlideShow = ProcessManager.ProcessID.SleepScreen;
			}
			else if (slideShowID == WatcherEnums.SlideShowID.DreamVoidWeaver)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_170 - In Dreams";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
				}
				float num34 = ConvertTime(0, 1, 20);
				float num35 = 1.5f;
				float num36 = 2.7f;
				float num37 = 1.5f;
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				float num38 = num34;
				for (int num39 = 0; num39 < 7; num39++)
				{
					MenuScene.SceneID sceneID4 = null;
					switch (num39)
					{
					case 0:
						sceneID4 = WatcherEnums.MenuSceneID.Dream_VoidWeaver1;
						break;
					case 1:
						sceneID4 = WatcherEnums.MenuSceneID.Dream_VoidWeaver2;
						break;
					case 2:
						sceneID4 = WatcherEnums.MenuSceneID.Dream_VoidWeaver3;
						break;
					case 3:
						sceneID4 = WatcherEnums.MenuSceneID.Dream_VoidWeaver4;
						break;
					case 4:
						sceneID4 = WatcherEnums.MenuSceneID.Dream_VoidWeaver5;
						break;
					case 5:
						sceneID4 = WatcherEnums.MenuSceneID.Dream_VoidWeaver6;
						break;
					case 6:
						sceneID4 = WatcherEnums.MenuSceneID.Dream_VoidWeaver7;
						break;
					}
					bool num40 = num39 == 5;
					float num41 = (num40 ? (num35 + num36 + num37) : 0f);
					if (num39 == 6)
					{
						num41 = num36;
					}
					Scene scene7 = new Scene(sceneID4, num38, num38 + num35 + num41, num38 + num35 + num36 + num41);
					if (num40)
					{
						scene7.AddCrossFade(num38 + num35 + num36 + num37, 20);
					}
					playList.Add(scene7);
					num38 += num35 + num36 + num37 + num41;
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, num38, num38, num38 + 5f));
				processAfterSlideShow = ProcessManager.ProcessID.SleepScreen;
			}
			else if (slideShowID == WatcherEnums.SlideShowID.DreamTerrace)
			{
				if (manager.musicPlayer != null)
				{
					float value = MenuScene.GetRippleLevels()[0];
					value = Mathf.Clamp(value, 1f, 5f);
					waitForMusic = ((value == 5f) ? "NA_44 - Terrace Vision" : "NA_44x - Terrace Vision");
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
				}
				float num42 = ConvertTime(0, 1, 20);
				float num43 = 1.5f;
				float num44 = 12f;
				float num45 = 2.5f;
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				float num46 = num42;
				_ = WatcherEnums.MenuSceneID.Dream_Terrace;
				float num47 = 7f;
				Scene scene8 = new Scene(WatcherEnums.MenuSceneID.Dream_Terrace, num46, num46 + num43 + num47, num46 + num43 + num44 + num47);
				scene8.AddCrossFade(num46 + num47, 120);
				playList.Add(scene8);
				num46 += num43 + num44 + num45 + num47;
				playList.Add(new Scene(MenuScene.SceneID.Empty, num46, num46, num46 + 5f));
				processAfterSlideShow = ProcessManager.ProcessID.SleepScreen;
			}
			else if (slideShowID == WatcherEnums.SlideShowID.EndingVoidBath)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_172 - Bathing";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
				}
				float num48 = ConvertTime(0, 1, 20);
				float num49 = 2.5f;
				float num50 = 9.2f;
				float num51 = 2.5f;
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				float num52 = num48;
				for (int num53 = 0; num53 < 3; num53++)
				{
					MenuScene.SceneID sceneID5 = null;
					switch (num53)
					{
					case 0:
						sceneID5 = WatcherEnums.MenuSceneID.Ending_VoidBath1;
						break;
					case 1:
						sceneID5 = WatcherEnums.MenuSceneID.Ending_VoidBath2;
						break;
					case 2:
						sceneID5 = WatcherEnums.MenuSceneID.Ending_VoidBath3;
						break;
					}
					bool num54 = num53 == 1 || num53 == 2;
					if (num53 == 0)
					{
						num50 = 12.2f;
					}
					if (num53 == 1)
					{
						num50 = 6.2f;
					}
					float num55 = (num54 ? (num49 + num50 + num51) : 0f);
					Scene scene9 = new Scene(sceneID5, num52, num52 + num49 + num55, num52 + num49 + num50 + num55);
					if (num54)
					{
						scene9.AddCrossFade(num52 + num49 + num50 + num51, 20);
					}
					playList.Add(scene9);
					num52 += num49 + num50 + num51 + num55;
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, num52, num52, num52 + 5f));
				processAfterSlideShow = ProcessManager.ProcessID.Game;
			}
			else if (slideShowID == WatcherEnums.SlideShowID.EndingSpinningTop)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_173 - Spinning Top Reprise";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
				}
				float num56 = ConvertTime(0, 1, 20);
				float num57 = 2.5f;
				float num58 = 9.2f;
				float num59 = 2.5f;
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				float num60 = num56;
				for (int num61 = 0; num61 < 3; num61++)
				{
					MenuScene.SceneID sceneID6 = null;
					switch (num61)
					{
					case 0:
						sceneID6 = WatcherEnums.MenuSceneID.Ending_SpinningTop1;
						break;
					case 1:
						sceneID6 = WatcherEnums.MenuSceneID.Ending_SpinningTop2;
						break;
					case 2:
						sceneID6 = WatcherEnums.MenuSceneID.Ending_SpinningTop3;
						break;
					}
					float num62 = 0f;
					if (num61 == 2)
					{
						num62 = num58;
					}
					Scene item = new Scene(sceneID6, num60, num60 + num57 + num62, num60 + num57 + num58 + num62);
					playList.Add(item);
					num60 += num57 + num58 + num59 + num62;
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, num60, num60, num60 + 5f));
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
				manager.desiredCreditsSong = "RW_124 - Theme of Youth";
			}
			else if (slideShowID == WatcherEnums.SlideShowID.EndingRot)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_171 - Efflorescence";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
				}
				float num63 = ConvertTime(0, 0, 0);
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				float num64 = num63;
				for (int num65 = 0; num65 < 6; num65++)
				{
					MenuScene.SceneID sceneID7 = null;
					switch (num65)
					{
					case 0:
						sceneID7 = WatcherEnums.MenuSceneID.Ending_Prince1;
						break;
					case 1:
						sceneID7 = WatcherEnums.MenuSceneID.Ending_Prince2;
						break;
					case 2:
						sceneID7 = WatcherEnums.MenuSceneID.Ending_Prince3;
						break;
					case 3:
						sceneID7 = WatcherEnums.MenuSceneID.Ending_Prince4;
						break;
					case 4:
						sceneID7 = WatcherEnums.MenuSceneID.Ending_Prince5;
						break;
					case 5:
						sceneID7 = WatcherEnums.MenuSceneID.Ending_Prince6;
						break;
					}
					bool num66 = num65 == 0 || num65 == 1 || num65 == 2 || num65 == 4;
					bool flag = num65 == 0 || num65 == 4;
					float num67 = 1f;
					float num68 = 3.2f;
					float num69 = 1f;
					float num70 = 0f;
					if (num65 == 0)
					{
						num67 = 0f;
					}
					if (num65 >= 1 && num65 < 4)
					{
						num67 = 2f;
					}
					if (num65 == 4)
					{
						num67 = 1.75f;
					}
					if (num65 == 1)
					{
						num68 = 4f;
					}
					if (num65 == 2)
					{
						num68 = 1f;
					}
					if (num65 == 3)
					{
						num68 = 2.85f;
					}
					if (num65 == 4)
					{
						num68 = 0.9f;
					}
					if (num65 == 5)
					{
						num68 = 10.2f;
					}
					if (num66)
					{
						num70 += num67 + num68 + num69;
					}
					if (flag)
					{
						num70 += num67 + num68 + num69;
					}
					Scene scene10 = new Scene(sceneID7, num64, num64 + num67 + num70, num64 + num67 + num68 + num70);
					if (num66)
					{
						scene10.AddCrossFade(num64 + num67 + num68 + num69, 60);
					}
					if (flag)
					{
						scene10.AddCrossFade(num64 + (num67 + num68 + num69) * 2f, 60);
					}
					playList.Add(scene10);
					num64 += num67 + num68 + num69 + num70;
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, num64, num64, num64 + 5f));
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
				manager.desiredCreditsSong = "RW_151 - Condemned";
			}
			else if (slideShowID == WatcherEnums.SlideShowID.EndingVoidWeaver)
			{
				if (manager.musicPlayer != null)
				{
					waitForMusic = "RW_174 - A Path A Purpose";
					stall = true;
					manager.musicPlayer.MenuRequestsSong(waitForMusic, 1.5f, 40f);
				}
				float num71 = ConvertTime(0, 1, 20);
				playList.Add(new Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
				float num72 = num71;
				for (int num73 = 0; num73 < 6; num73++)
				{
					float num74 = 2.5f;
					float num75 = 7.5f;
					float num76 = 2.5f;
					MenuScene.SceneID sceneID8 = null;
					switch (num73)
					{
					case 0:
						sceneID8 = WatcherEnums.MenuSceneID.Ending_Weaver1;
						break;
					case 1:
						sceneID8 = WatcherEnums.MenuSceneID.Ending_Weaver2;
						break;
					case 2:
						sceneID8 = WatcherEnums.MenuSceneID.Ending_Weaver3;
						break;
					case 3:
						sceneID8 = WatcherEnums.MenuSceneID.Ending_Weaver4;
						break;
					case 4:
						sceneID8 = WatcherEnums.MenuSceneID.Ending_Weaver5;
						break;
					case 5:
						sceneID8 = WatcherEnums.MenuSceneID.Ending_Weaver6;
						break;
					}
					int num77 = 0;
					switch (num73)
					{
					case 0:
						num77 = 1;
						break;
					case 2:
						num77 = 3;
						break;
					}
					switch (num73)
					{
					case 1:
						num74 = 1.5f;
						num75 = 5.75f;
						num76 = 1.5f;
						break;
					case 2:
						num74 = 1.5f;
						num75 = 0.9f;
						break;
					case 3:
						num75 = 8.25f;
						break;
					case 4:
						num74 = 1f;
						num75 = 8f;
						break;
					default:
						num75 = 7.5f;
						break;
					}
					float num78 = ((num73 == 2) ? (num74 + num75) : (num74 + num75 + num76));
					if (num73 == 0)
					{
						num74 = 1.5f;
						num75 = 6.75f;
						num78 = num74 + 6.75f;
					}
					float num79 = num78 * (float)num77;
					if (num73 == 5)
					{
						num79 = num75;
					}
					Scene scene11 = new Scene(sceneID8, num72, num72 + num74 + num79, num72 + num74 + num75 + num79);
					for (int num80 = 0; num80 < num77; num80++)
					{
						scene11.AddCrossFade(num72 + num78 * (float)(num80 + 1), (num73 == 0) ? 30 : 60);
					}
					playList.Add(scene11);
					num72 += num74 + num75 + num76 + num79;
				}
				playList.Add(new Scene(MenuScene.SceneID.Empty, num72, num72, num72 + 5f));
				processAfterSlideShow = ProcessManager.ProcessID.Credits;
				manager.desiredCreditsSong = "RW_108 - Explorer";
			}
		}
		preloadedScenes = new SlideShowMenuScene[playList.Count];
		manager.RemoveLoadingLabel();
		NextScene();
	}

	public float ConvertTime(int minutes, int seconds, int pps)
	{
		return (float)minutes * 60f + (float)seconds + (float)pps / 100f;
	}

	public override void RawUpdate(float dt)
	{
		base.RawUpdate(dt);
		bool flag = RWInput.CheckPauseButton(0, inMenu: false);
		if (AutobotSkipSlideShow || (flag && !lastPauseButton))
		{
			AutobotSkipSlideShow = false;
			skip = true;
			if (!exitedSlideshow)
			{
				if (ModManager.Watcher && slideShowID == WatcherEnums.SlideShowID.EndingSpinningTop)
				{
					manager.CueAchievement(RainWorld.AchievementID.SpinningTopEnding, 5f);
				}
				if (ModManager.Watcher && slideShowID == WatcherEnums.SlideShowID.EndingRot)
				{
					manager.CueAchievement(RainWorld.AchievementID.RotPrinceEnding, 5f);
				}
				if (ModManager.Watcher && slideShowID == WatcherEnums.SlideShowID.EndingVoidWeaver)
				{
					manager.CueAchievement(RainWorld.AchievementID.VoidWeaverEnding, 5f);
				}
				manager.RequestMainProcessSwitch(processAfterSlideShow);
				if (manager.musicPlayer != null && manager.musicPlayer.song != null)
				{
					manager.musicPlayer.FadeOutAllSongs(40f);
				}
				exitedSlideshow = true;
			}
			return;
		}
		if (waitForMusic != null && stall && manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song.name == waitForMusic)
		{
			stall = false;
		}
		if (preloadedScenes[current].moveEditor != null)
		{
			stall = true;
		}
		if (!stall)
		{
			if (manager.musicPlayer != null && manager.musicPlayer.song != null && manager.musicPlayer.song.startedPlaying && manager.musicPlayer.song.subTracks.Count > 0 && manager.musicPlayer.song.subTracks[0].source != null && manager.musicPlayer.song.subTracks[0].source.isPlaying && manager.musicPlayer.song.subTracks[0].source.clip != null && manager.musicPlayer.song.subTracks[0].source.time < manager.musicPlayer.song.subTracks[0].source.clip.length)
			{
				float num = 1f;
				playbackSpeedMult = Mathf.Clamp(playbackSpeedMult, 1f, 2f);
				time += dt * playbackSpeedMult;
				if (time < manager.musicPlayer.song.subTracks[0].source.time)
				{
					num = Mathf.Clamp((manager.musicPlayer.song.subTracks[0].source.time - time) * 0.5f + 1f, 1f, 2f);
				}
				if (playbackSpeedMult < num)
				{
					playbackSpeedMult = Mathf.Lerp(playbackSpeedMult, num, 0.04f);
				}
				else
				{
					playbackSpeedMult = Mathf.Lerp(playbackSpeedMult, num, 0.08f);
				}
			}
			else
			{
				time += dt;
			}
		}
		float num2 = ((current >= playList.Count - 1) ? (playList[current].fadeOutStartAt + 1f) : playList[current + 1].startAt);
		inSceneTime = Mathf.InverseLerp(playList[current].startAt, num2, time);
		if (!skip)
		{
			manager.fadeToBlack = Math.Max(Mathf.InverseLerp(playList[current].fadeInDoneAt, playList[current].startAt, time), Mathf.InverseLerp(playList[current].fadeOutStartAt, num2, time));
		}
		if (crossfadeInd < playList[current].crossfades.Count && time >= playList[current].crossfades[crossfadeInd])
		{
			preloadedScenes[current].TriggerCrossfade(playList[current].crossfadeDurations[crossfadeInd]);
			crossfadeInd++;
		}
		if (ModManager.Watcher && preloadedScenes[current] != null && playList[current].sceneID == WatcherEnums.MenuSceneID.Dream_Terrace)
		{
			float num3 = Mathf.Sin(time * 3.65f) * 0.2f + 0.5f;
			if (preloadedScenes[current].depthIllustrations.Count > 0)
			{
				num3 *= 1f - preloadedScenes[current].depthIllustrations[11].alpha;
				preloadedScenes[current].depthIllustrations[10].setAlpha = num3;
			}
			else if (preloadedScenes[current].flatIllustrations.Count > 0)
			{
				num3 *= preloadedScenes[current].flatIllustrations[1].alpha;
				preloadedScenes[current].flatIllustrations[2].setAlpha = num3;
			}
		}
		if (!(time >= num2))
		{
			return;
		}
		if (current < playList.Count - 1)
		{
			NextScene();
		}
		else if (!exitedSlideshow)
		{
			if (ModManager.Watcher && slideShowID == WatcherEnums.SlideShowID.EndingSpinningTop)
			{
				manager.CueAchievement(RainWorld.AchievementID.SpinningTopEnding, 5f);
			}
			if (ModManager.Watcher && slideShowID == WatcherEnums.SlideShowID.EndingRot)
			{
				manager.CueAchievement(RainWorld.AchievementID.RotPrinceEnding, 5f);
			}
			if (ModManager.Watcher && slideShowID == WatcherEnums.SlideShowID.EndingVoidWeaver)
			{
				manager.CueAchievement(RainWorld.AchievementID.VoidWeaverEnding, 5f);
			}
			manager.RequestMainProcessSwitch(processAfterSlideShow);
			exitedSlideshow = true;
		}
	}

	public void NextScene()
	{
		inSceneTime = 0f;
		crossfadeInd = 0;
		if (current >= 0 && scene != null)
		{
			scene.RemoveSprites();
			pages[0].subObjects.Remove(scene);
			preloadedScenes[current].Hide();
			preloadedScenes[current].RemoveSprites();
			preloadedScenes[current].UnloadImages();
			if (preloadedScenes[current].moveEditor != null)
			{
				preloadedScenes[current].moveEditor.WriteAndClear();
				preloadedScenes[current].moveEditor = null;
			}
			pages[0].RemoveSprites();
			preloadedScenes[current] = null;
		}
		current++;
		if (current < playList.Count)
		{
			AssetManager.HardCleanFutileAssets();
			preloadedScenes[current] = new SlideShowMenuScene(this, pages[0], playList[current].sceneID);
			scene = preloadedScenes[current];
			pages[0].subObjects.Add(scene);
			preloadedScenes[current].Show();
		}
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		if (scene != null && scene is SlideShowMenuScene && (scene as SlideShowMenuScene).moveEditor != null)
		{
			(scene as SlideShowMenuScene).moveEditor.WriteAndClear();
			(scene as SlideShowMenuScene).moveEditor = null;
		}
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
		if (nextProcess is EndCredits && (slideShowID == SlideShowID.WhiteOutro || slideShowID == SlideShowID.YellowOutro))
		{
			manager.CueAchievement(RainWorld.AchievementID.Win, 5f);
		}
		else if (nextProcess is EndCredits && slideShowID == SlideShowID.RedOutro)
		{
			manager.CueAchievement(RainWorld.AchievementID.HunterWin, 5f);
		}
		else if (nextProcess is SleepAndDeathScreen)
		{
			if (slideShowID == WatcherEnums.SlideShowID.DreamTerrace)
			{
				(nextProcess as SleepAndDeathScreen).terraceAnimation = true;
			}
			(nextProcess as SleepAndDeathScreen).GetDataFromGame(passthroughPackage);
		}
		else if (ModManager.MSC && nextProcess is EndCredits && (slideShowID == MoreSlugcatsEnums.SlideShowID.SpearmasterOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.InvOutro))
		{
			manager.CueAchievement(RainWorld.AchievementID.Win, 5f);
		}
		else if (ModManager.MSC && nextProcess is EndCredits && (slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerOutro || slideShowID == MoreSlugcatsEnums.SlideShowID.ArtificerAltEnd))
		{
			manager.CueAchievement(RainWorld.AchievementID.ArtificerEnding, 5f);
		}
		else if (ModManager.MSC && nextProcess is EndCredits && slideShowID == MoreSlugcatsEnums.SlideShowID.RivuletAltEnd)
		{
			manager.CueAchievement(RainWorld.AchievementID.RivuletEnding, 5f);
		}
		else if (ModManager.MSC && nextProcess is EndCredits && slideShowID == MoreSlugcatsEnums.SlideShowID.GourmandAltEnd)
		{
			manager.CueAchievement(RainWorld.AchievementID.GourmandEnding, 5f);
		}
		else if (!ModManager.MSC && nextProcess is StoryGameStatisticsScreen)
		{
			(nextProcess as StoryGameStatisticsScreen).GetDataFromGame(endGameStatsPackage);
			(nextProcess as StoryGameStatisticsScreen).forceWatch = true;
		}
		if (ModManager.MSC)
		{
			if (ModManager.MSC && nextProcess is EndCredits)
			{
				(nextProcess as EndCredits).passthroughPackage = passthroughPackage;
			}
			if (nextProcess is DatingSim)
			{
				(nextProcess as DatingSim).passthroughPackage = passthroughPackage;
			}
			if (passthroughPackage != null && nextProcess is KarmaLadderScreen && !(nextProcess is SleepAndDeathScreen))
			{
				(nextProcess as KarmaLadderScreen).GetDataFromGame(passthroughPackage);
			}
		}
	}
}
