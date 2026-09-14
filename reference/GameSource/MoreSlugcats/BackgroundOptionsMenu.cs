using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Menu;
using UnityEngine;
using Watcher;

namespace MoreSlugcats;

public class BackgroundOptionsMenu : global::Menu.Menu
{
	public List<string> regionOrder;

	public bool lastPauseButton;

	public SimpleButton backButton;

	public FSprite darkSprite;

	public SimpleButton[] backgroundButtons;

	public MenuIllustration[] backgroundIllustrations;

	public SimpleButton titleButton;

	public SimpleButton submenuButton;

	public int pageNum;

	public SimpleButton nextPageButton;

	public SimpleButton prevPageButton;

	public bool downpourSceneAvailable;

	public int downpourSceneIndex;

	public bool watcherSceneAvailable;

	public int watcherSceneIndex;

	public override bool ForceNoMouseMode => false;

	public int ButtonsPerPage => 28;

	public int TotalPages => Mathf.CeilToInt((float)(NonRegionButtons + regionOrder.Count) / (float)ButtonsPerPage);

	public int NonRegionButtons
	{
		get
		{
			int num = 6;
			if (downpourSceneAvailable)
			{
				num++;
			}
			if (watcherSceneAvailable)
			{
				num++;
			}
			return num;
		}
	}

	public BackgroundOptionsMenu(ProcessManager manager)
		: base(manager, MMFEnums.ProcessID.BackgroundOptions)
	{
		int num = 6;
		downpourSceneAvailable = File.Exists(AssetManager.ResolveFilePath("Scenes" + Path.DirectorySeparatorChar + "main menu - downpour" + Path.DirectorySeparatorChar + "main menu - downpour - flat.png"));
		downpourSceneIndex = num;
		watcherSceneAvailable = ModManager.Watcher;
		watcherSceneIndex = num + (downpourSceneAvailable ? 1 : 0);
		List<string> fullRegionOrder = Region.GetFullRegionOrder();
		regionOrder = new List<string>();
		for (int i = 0; i < fullRegionOrder.Count; i++)
		{
			if (File.Exists(AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "Safari_" + fullRegionOrder[i] + ".png")))
			{
				regionOrder.Add(fullRegionOrder[i]);
			}
		}
		pages.Add(new Page(this, null, "main", 0));
		scene = new InteractiveMenuScene(this, pages[0], manager.rainWorld.options.subBackground);
		pages[0].subObjects.Add(scene);
		darkSprite = new FSprite("pixel");
		darkSprite.color = new Color(0f, 0f, 0f);
		darkSprite.anchorX = 0f;
		darkSprite.anchorY = 0f;
		darkSprite.scaleX = 1368f;
		darkSprite.scaleY = 770f;
		darkSprite.x = -1f;
		darkSprite.y = -1f;
		darkSprite.alpha = 0.85f;
		pages[0].Container.AddChild(darkSprite);
		backButton = new SimpleButton(this, pages[0], Translate("BACK"), "BACK", new Vector2(200f, 50f), new Vector2(110f, 30f));
		pages[0].subObjects.Add(backButton);
		backObject = backButton;
		titleButton = new SimpleButton(this, pages[0], Translate("CONFIGURE TITLE SCREEN"), "TITLE", new Vector2(741f, 50f), new Vector2(210f, 30f));
		titleButton.toggled = true;
		pages[0].subObjects.Add(titleButton);
		submenuButton = new SimpleButton(this, pages[0], Translate("CONFIGURE SUB MENUS"), "SUBMENU", new Vector2(961f, 50f), new Vector2(210f, 30f));
		pages[0].subObjects.Add(submenuButton);
		mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
		if (TotalPages > 1)
		{
			prevPageButton = new SimpleButton(this, pages[0], Translate("PREVIOUS"), "PREV", new Vector2(400f, 50f), new Vector2(110f, 30f));
			pages[0].subObjects.Add(prevPageButton);
			nextPageButton = new SimpleButton(this, pages[0], Translate("NEXT"), "NEXT", new Vector2(530f, 50f), new Vector2(110f, 30f));
			pages[0].subObjects.Add(nextPageButton);
		}
		PopulateButtons();
	}

	public override string UpdateInfoText()
	{
		if (selectedObject == backButton)
		{
			return Translate("Back to options");
		}
		if (nextPageButton != null && selectedObject == nextPageButton)
		{
			return Translate("Next Page");
		}
		if (prevPageButton != null && selectedObject == prevPageButton)
		{
			return Translate("Previous Page");
		}
		if (selectedObject == titleButton)
		{
			return Translate("Configure the background image used on the title screen");
		}
		if (selectedObject == submenuButton)
		{
			return Translate("Configure the background image used in the sub menus");
		}
		return base.UpdateInfoText();
	}

	public override void Update()
	{
		base.Update();
		bool flag = RWInput.CheckPauseButton(0);
		if (flag && !lastPauseButton && manager.dialog == null && !backButton.buttonBehav.greyedOut)
		{
			Singal(backButton, backButton.signalText);
		}
		lastPauseButton = flag;
	}

	public override void Singal(MenuObject sender, string message)
	{
		switch (message)
		{
		case "BACK":
			manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
			PlaySound(SoundID.MENU_Switch_Page_Out);
			manager.rainWorld.options.Save();
			return;
		case "NEXT":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			pageNum++;
			if (pageNum >= TotalPages)
			{
				pageNum = 0;
			}
			PopulateButtons();
			return;
		case "PREV":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			pageNum--;
			if (pageNum < 0)
			{
				pageNum = TotalPages - 1;
			}
			PopulateButtons();
			return;
		}
		if (message.Contains("BACKGROUND"))
		{
			int num = int.Parse(message.Substring("BACKGROUND".Length), NumberStyles.Any, CultureInfo.InvariantCulture);
			int num2 = ButtonsPerPage * pageNum;
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			for (int i = 0; i < backgroundButtons.Length; i++)
			{
				if (i == num - num2)
				{
					backgroundButtons[i].toggled = true;
				}
				else
				{
					backgroundButtons[i].toggled = false;
				}
			}
			if (titleButton.toggled)
			{
				manager.rainWorld.options.titleBackground = IndexToOption(num);
			}
			else if (submenuButton.toggled)
			{
				manager.rainWorld.options.subBackground = IndexToOption(num);
			}
			return;
		}
		if (message == "TITLE")
		{
			titleButton.toggled = true;
			submenuButton.toggled = false;
			int num3 = OptionToIndex(manager.rainWorld.options.titleBackground);
			int num4 = ButtonsPerPage * pageNum;
			for (int j = 0; j < backgroundButtons.Length; j++)
			{
				if (j == num3 - num4)
				{
					backgroundButtons[j].toggled = true;
				}
				else
				{
					backgroundButtons[j].toggled = false;
				}
			}
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			return;
		}
		if (!(message == "SUBMENU"))
		{
			return;
		}
		titleButton.toggled = false;
		submenuButton.toggled = true;
		int num5 = OptionToIndex(manager.rainWorld.options.subBackground);
		int num6 = ButtonsPerPage * pageNum;
		for (int k = 0; k < backgroundButtons.Length; k++)
		{
			if (k == num5 - num6)
			{
				backgroundButtons[k].toggled = true;
			}
			else
			{
				backgroundButtons[k].toggled = false;
			}
		}
		PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
	}

	public override void ShutDownProcess()
	{
		base.ShutDownProcess();
		darkSprite.RemoveFromContainer();
	}

	public bool IndexUnlocked(int ind, List<string> regions)
	{
		if (IndexToOption(ind) == MenuScene.SceneID.Outro_2_Up_Swim && !manager.rainWorld.progression.miscProgressionData.redUnlocked)
		{
			return false;
		}
		if (ind <= NonRegionButtons - 1)
		{
			return true;
		}
		if (ind > NonRegionButtons - 1)
		{
			string text = regions[ind - NonRegionButtons];
			if (text == "SU")
			{
				return true;
			}
			for (int i = 0; i < manager.rainWorld.progression.regionNames.Length; i++)
			{
				if (manager.rainWorld.progression.regionNames[i] == text && manager.rainWorld.progression.miscProgressionData.GetDiscoveredShelterStringsInRegion(text).Count > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public MenuScene.SceneID IndexToOption(int ind)
	{
		if (ind > NonRegionButtons - 1)
		{
			return Region.GetRegionLandscapeScene(regionOrder[ind - NonRegionButtons]);
		}
		switch (ind)
		{
		case 0:
			return MenuScene.SceneID.MainMenu;
		case 1:
			return MenuScene.SceneID.Intro_1_Tree;
		case 2:
			return MenuScene.SceneID.Intro_3_In_Tree;
		case 3:
			return MenuScene.SceneID.Intro_4_Walking;
		case 4:
			return MenuScene.SceneID.Intro_5_Hunting;
		case 5:
			return MenuScene.SceneID.Outro_2_Up_Swim;
		default:
			if (ind == downpourSceneIndex && downpourSceneAvailable)
			{
				return MenuScene.SceneID.MainMenu_Downpour;
			}
			if (ind == watcherSceneIndex && watcherSceneAvailable)
			{
				return WatcherEnums.MenuSceneID.MainMenu_Watcher;
			}
			return MenuScene.SceneID.MainMenu;
		}
	}

	public int OptionToIndex(MenuScene.SceneID sceneOption)
	{
		if (sceneOption == MenuScene.SceneID.MainMenu)
		{
			return 0;
		}
		if (sceneOption == MenuScene.SceneID.Intro_1_Tree)
		{
			return 1;
		}
		if (sceneOption == MenuScene.SceneID.Intro_3_In_Tree)
		{
			return 2;
		}
		if (sceneOption == MenuScene.SceneID.Intro_4_Walking)
		{
			return 3;
		}
		if (sceneOption == MenuScene.SceneID.Intro_5_Hunting)
		{
			return 4;
		}
		if (sceneOption == MenuScene.SceneID.Outro_2_Up_Swim)
		{
			return 5;
		}
		if (sceneOption == MenuScene.SceneID.MainMenu_Downpour)
		{
			if (!downpourSceneAvailable)
			{
				return 0;
			}
			return downpourSceneIndex;
		}
		if (ModManager.Watcher && sceneOption == WatcherEnums.MenuSceneID.MainMenu_Watcher)
		{
			if (!watcherSceneAvailable)
			{
				return 0;
			}
			return watcherSceneIndex;
		}
		for (int i = 0; i < regionOrder.Count; i++)
		{
			if (sceneOption == Region.GetRegionLandscapeScene(regionOrder[i]))
			{
				return i + NonRegionButtons;
			}
		}
		return 0;
	}

	public int ButtonsOnPage(int num)
	{
		if (num == 0)
		{
			return Math.Min(ButtonsPerPage, regionOrder.Count + NonRegionButtons);
		}
		if (num == TotalPages - 1)
		{
			return (regionOrder.Count - (ButtonsPerPage - NonRegionButtons)) % ButtonsPerPage;
		}
		return ButtonsPerPage;
	}

	public void PopulateButtons()
	{
		if (backgroundButtons != null)
		{
			for (int i = 0; i < backgroundButtons.Length; i++)
			{
				backgroundButtons[i].RemoveSprites();
				pages[0].RemoveSubObject(backgroundButtons[i]);
			}
			backgroundButtons = null;
		}
		if (backgroundIllustrations != null)
		{
			for (int j = 0; j < backgroundIllustrations.Length; j++)
			{
				backgroundIllustrations[j].RemoveSprites();
				pages[0].RemoveSubObject(backgroundIllustrations[j]);
			}
			backgroundIllustrations = null;
		}
		float num = 615f;
		float num2 = 120f;
		float num3 = 120f;
		float num4 = 8f;
		int num5 = 7;
		int num6 = OptionToIndex(manager.rainWorld.options.TitleBackground);
		if (submenuButton.toggled)
		{
			num6 = OptionToIndex(manager.rainWorld.options.SubBackground);
		}
		backgroundButtons = new SimpleButton[ButtonsOnPage(pageNum)];
		backgroundIllustrations = new MenuIllustration[backgroundButtons.Length];
		int num7 = ButtonsPerPage * pageNum;
		for (int k = 0; k < ButtonsOnPage(pageNum); k++)
		{
			Vector2 vector = new Vector2(299f - num2 * 0.5f + (float)(k % num5) * (num2 + num4), num - num3 * 0.5f - (num3 + num4) * (float)(k / num5));
			backgroundButtons[k] = new SimpleButton(this, pages[0], "", "BACKGROUND" + (k + num7), vector, new Vector2(num2, num3));
			if (IndexUnlocked(num7 + k, regionOrder))
			{
				if (k > NonRegionButtons - 1 || pageNum > 0)
				{
					backgroundIllustrations[k] = new MenuIllustration(this, pages[0], string.Empty, "Safari_" + regionOrder[k - NonRegionButtons + num7], vector + new Vector2(10f, 10f), crispPixels: true, anchorCenter: false);
				}
				else
				{
					string fileName = "Background_" + (k + num7 + 1);
					if (downpourSceneAvailable && k + num7 == downpourSceneIndex)
					{
						fileName = "Background_7";
					}
					else if (watcherSceneAvailable && k + num7 == watcherSceneIndex)
					{
						fileName = "Background_Watcher_Button";
					}
					backgroundIllustrations[k] = new MenuIllustration(this, pages[0], string.Empty, fileName, vector + new Vector2(10f, 10f), crispPixels: true, anchorCenter: false);
				}
			}
			else
			{
				backgroundButtons[k].buttonBehav.greyedOut = true;
				backgroundIllustrations[k] = new MenuIllustration(this, pages[0], string.Empty, "Safari_Locked", vector + new Vector2(10f, 10f), crispPixels: true, anchorCenter: false);
			}
			if (k == num6 - num7)
			{
				backgroundButtons[k].toggled = true;
			}
			pages[0].subObjects.Add(backgroundButtons[k]);
			pages[0].subObjects.Add(backgroundIllustrations[k]);
		}
	}
}
