using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kittehface.Framework20;
using MoreSlugcats;
using RWCustom;
using Steamworks;
using UnityEngine;

namespace Menu;

public class InitializationScreen : Menu
{
	public enum InitializationStep
	{
		MOUNT_AOC,
		AOCUpdateAPI,
		WAIT_FOR_OPTIONS_READY,
		LOAD_FONTS,
		PORT_LEGACY_SAVE_FILES,
		CHECK_GAME_VERSION_CHANGED,
		CHECK_WORKSHOP_CONTENT,
		START_DOWNLOAD_WORKSHOP_CONTENT,
		WAIT_DOWNLOAD_WORKSHOP_CONTENT,
		VALIDATE_WORKSHOP_CONTENT,
		VALIDATE_MODS,
		APPLY_MODS,
		REQUIRE_RESTART,
		WAIT_FOR_ASYNC_LOAD,
		MOD_INIT,
		WAIT_FOR_MOD_INIT_ASYNC,
		RELOAD_PROGRESSION,
		WRAP_UP,
		WAIT_FOR_PROCESS_CHANGE,
		WAIT_STARTUP_DIALOGS,
		LOCK_ON_PROGRESSION_FAILED_ERROR,
		WAIT_FOR_BACKUP_RESTORE,
		WAIT_FOR_BACKUP_RECREATION,
		SAVE_FILE_FAIL_WARN_PROCEED,
		LOCALIZATION_DEBUG
	}

	public DialogBoxNotify needsReapplyDialog;

	public DialogBoxAsyncWait downloadingWorkshopItemsDialog;

	public DialogBoxAsyncWait applyingModsDialog;

	public DialogBoxAsyncWait restoringBackupDialog;

	public DialogBoxNotify requiresRestartDialog;

	public DialogBoxNotify restoreBackupDialog;

	public ModManager.ModApplyer modApplyer;

	public string modApplyerError;

	public DialogBoxNotify applyingModsErrorDialog;

	public DialogBoxNotify gameVersionChangedDialog;

	public DialogConfirm progressionFailedError;

	public bool modApplyerRequiresRestart;

	public InitializationStep currentStep;

	public InitializationStep stepBeforeProgressionFailed;

	public bool ignoreProgressionError;

	public PlayerProgression legacySaveFileHandler;

	public int rememberSaveSlot = -1;

	public int legacySaveFileIndex;

	public bool needsRelaunch;

	public bool reloadedProgression;

	public bool filesInBadState;

	public bool checkingForBackup;

	public bool checkedForBackup;

	public bool backupExists;

	public bool waitingForBackupRestore;

	public bool backupRestoreSuccess;

	public int lastLocalizationDebugIndex = -1;

	public int localizationDebugIndex;

	public string[] localizationTestMessages = new string[15]
	{
		"mod_menu_restoring_backup", "backup_restore_failed", "mod_menu_restore_backup", "ps4_load_expedition_failed", "ps4_load_save_slot_failed", "ps4_load_save_slot_restore_corrupt_failed", "ps4_load_expedition_run_restore_corrupt_failed", "ps4_load_expedition_restore_corrupt_failed", "ps4_load_save_slot_restore_corrupt", "ps4_load_expedition_run_restore_corrupt",
		"ps4_load_expedition_restore_corrupt", "ps4_load_save_slot_load_failed", "ps4_load_expedition_run_failed", "save_file_error_continue", "save_file_corrupted"
	};

	public PublishedFileId_t[] subscribedItems;

	public List<PublishedFileId_t> updatedItems;

	public static event Action<RainWorld> onAOCMounted;

	public InitializationScreen(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.Initialization)
	{
		pages.Add(new Page(this, null, "main", 0));
		ExtEnumInitializer.InitTypes();
		currentStep = InitializationStep.MOUNT_AOC;
	}

	public override void Update()
	{
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_06db: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (currentStep == InitializationStep.MOUNT_AOC)
		{
			currentStep = InitializationStep.WAIT_FOR_OPTIONS_READY;
		}
		else if (currentStep == InitializationStep.WAIT_FOR_OPTIONS_READY)
		{
			ProcessManager.fontHasBeenLoaded = true;
			if (manager.rainWorld.OptionsReady && manager.rainWorld.started)
			{
				currentStep = InitializationStep.LOAD_FONTS;
			}
		}
		else if (currentStep == InitializationStep.LOAD_FONTS)
		{
			InGameTranslator.UnloadFonts(manager.rainWorld.options.language);
			InGameTranslator.LoadFonts(manager.rainWorld.options.language, this);
			currentStep = InitializationStep.PORT_LEGACY_SAVE_FILES;
		}
		else if (currentStep == InitializationStep.PORT_LEGACY_SAVE_FILES)
		{
			SlugcatStats.Name[] array = new SlugcatStats.Name[3]
			{
				SlugcatStats.Name.White,
				SlugcatStats.Name.Yellow,
				SlugcatStats.Name.Red
			};
			if (rememberSaveSlot == -1)
			{
				rememberSaveSlot = manager.rainWorld.options.saveSlot;
			}
			if (legacySaveFileIndex < 3)
			{
				string text = ((legacySaveFileIndex == 0) ? "sav" : ("sav_" + (legacySaveFileIndex + 1)));
				string text2 = ((legacySaveFileIndex == 0) ? "sav" : ("sav" + (legacySaveFileIndex + 1)));
				string text3 = Custom.LegacyRootFolderDirectory() + (Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + text + ".txt").ToLowerInvariant();
				if (legacySaveFileHandler == null)
				{
					if (!File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + text2) && File.Exists(text3))
					{
						manager.rainWorld.options.saveSlot = legacySaveFileIndex;
						legacySaveFileHandler = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
					}
					else
					{
						legacySaveFileIndex++;
					}
				}
				else if (legacySaveFileHandler.progressionLoaded)
				{
					for (int i = 0; i < array.Length; i++)
					{
						StaticWorld.InitStaticWorld();
						legacySaveFileHandler.currentSaveState = new SaveState(array[i], legacySaveFileHandler);
						if (legacySaveFileHandler.LoadGameState(text3, null, saveAsDeathOrQuit: false) != null)
						{
							legacySaveFileHandler.LoadProgressionFromLegacyFile(text3);
							legacySaveFileHandler.SaveWorldStateAndProgression(malnourished: false);
						}
					}
					legacySaveFileHandler.Destroy();
					legacySaveFileHandler = null;
					legacySaveFileIndex++;
				}
				else
				{
					legacySaveFileHandler.Update();
				}
			}
			if (legacySaveFileIndex >= 3)
			{
				if (rememberSaveSlot >= 0)
				{
					manager.rainWorld.options.saveSlot = rememberSaveSlot;
				}
				manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
				if (manager.rainWorld.setup.singlePlayerChar != -1)
				{
					manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(manager.rainWorld.setup.singlePlayerChar));
				}
				RainWorld.lastActiveSaveSlot = manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;
				if (!WorldChecksumController.ControlCheckSum(manager.rainWorld.buildType))
				{
					Custom.LogWarning("World Checksum INCORRECT!");
					manager.rainWorld.progression.gameTinkeredWith = true;
				}
				currentStep = InitializationStep.CHECK_GAME_VERSION_CHANGED;
			}
		}
		else if (currentStep == InitializationStep.CHECK_GAME_VERSION_CHANGED)
		{
			if (ModManager.GameVersionChangedOnThisLaunch)
			{
				if (gameVersionChangedDialog == null)
				{
					gameVersionChangedDialog = new DialogBoxNotify(this, pages[0], Translate("remix_game_version"), "VERSIONPROMPT", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
					pages[0].subObjects.Add(gameVersionChangedDialog);
				}
			}
			else
			{
				Singal(null, "VERSIONPROMPT");
			}
		}
		else if (currentStep == InitializationStep.CHECK_WORKSHOP_CONTENT)
		{
			subscribedItems = manager.mySteamManager.GetSubscribedItems();
			if (subscribedItems.Length == 0)
			{
				currentStep = InitializationStep.VALIDATE_MODS;
			}
			else
			{
				currentStep = InitializationStep.START_DOWNLOAD_WORKSHOP_CONTENT;
			}
		}
		else if (currentStep == InitializationStep.START_DOWNLOAD_WORKSHOP_CONTENT)
		{
			updatedItems = new List<PublishedFileId_t>();
			manager.mySteamManager.ResetDownloadBatch();
			for (int j = 0; j < subscribedItems.Length; j++)
			{
				_ = ref subscribedItems[j];
				uint itemState = SteamUGC.GetItemState(subscribedItems[j]);
				if ((itemState & 0x30u) != 0)
				{
					manager.mySteamManager.currentlyDownloading.Add(subscribedItems[j]);
					manager.mySteamManager.numberItemsAddedForDownloading++;
					updatedItems.Add(subscribedItems[j]);
				}
				else if ((itemState & 4) == 0 || (itemState & 8u) != 0)
				{
					manager.mySteamManager.DownloadWorkshopMod(subscribedItems[j]);
					updatedItems.Add(subscribedItems[j]);
				}
			}
			if (manager.mySteamManager.numberItemsAddedForDownloading > 0)
			{
				ShowWorkshopDownloadingDialog();
				currentStep = InitializationStep.WAIT_DOWNLOAD_WORKSHOP_CONTENT;
			}
			else
			{
				ModManager.RefreshModsLists(manager.rainWorld);
				currentStep = InitializationStep.VALIDATE_MODS;
			}
		}
		else if (currentStep == InitializationStep.WAIT_DOWNLOAD_WORKSHOP_CONTENT)
		{
			if (downloadingWorkshopItemsDialog != null)
			{
				downloadingWorkshopItemsDialog.Update();
				downloadingWorkshopItemsDialog.SetText(Translate("Downloading Workshop Content") + " (" + (manager.mySteamManager.GetWorkshopDownloadProgress() * 100.0).ToString("0.00") + "%)");
			}
			if (manager.mySteamManager.currentlyDownloading.Count == 0)
			{
				HideWorkshopDownloadingDialog();
				currentStep = InitializationStep.VALIDATE_WORKSHOP_CONTENT;
			}
		}
		else if (currentStep == InitializationStep.VALIDATE_WORKSHOP_CONTENT)
		{
			ModManager.RefreshModsLists(manager.rainWorld);
			bool flag = false;
			needsRelaunch = false;
			ulong num = default(ulong);
			string text4 = default(string);
			uint num2 = default(uint);
			for (int k = 0; k < updatedItems.Count; k++)
			{
				if (SteamUGC.GetItemInstallInfo(updatedItems[k], ref num, ref text4, 1024u, ref num2))
				{
					for (int l = 0; l < ModManager.InstalledMods.Count; l++)
					{
						if (!(ModManager.InstalledMods[l].path == text4))
						{
							continue;
						}
						if (ModManager.InstalledMods[l].enabled)
						{
							flag = true;
							if (ModManager.ModFolderHasDLLContent(text4))
							{
								needsRelaunch = true;
							}
						}
						break;
					}
				}
				if (needsRelaunch)
				{
					break;
				}
			}
			if (flag)
			{
				if (!ModManager.GameVersionChangedOnThisLaunch)
				{
					needsReapplyDialog = new DialogBoxNotify(this, pages[0], Translate("mod_menu_reapply_mods"), "REAPPLY", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
					pages[0].subObjects.Add(needsReapplyDialog);
					needsReapplyDialog.timeOut = 0f;
				}
				else
				{
					Singal(null, "REAPPLY");
				}
				currentStep = InitializationStep.APPLY_MODS;
			}
			else
			{
				currentStep = InitializationStep.VALIDATE_MODS;
			}
		}
		else if (currentStep == InitializationStep.VALIDATE_MODS)
		{
			bool flag2 = false;
			foreach (ModManager.Mod installedMod in ModManager.InstalledMods)
			{
				if (installedMod.checksumChanged && installedMod.enabled)
				{
					Custom.LogImportant("CHECKSUM CHANGED FOR:", installedMod.name, "-- is now:", installedMod.checksum);
					flag2 = true;
				}
			}
			if (ModManager.CheckForDeletedBepinexMods(manager.rainWorld))
			{
				flag2 = true;
				filesInBadState = true;
			}
			for (int num3 = manager.rainWorld.options.enabledMods.Count - 1; num3 >= 0; num3--)
			{
				bool flag3 = false;
				for (int m = 0; m < ModManager.InstalledMods.Count; m++)
				{
					if (manager.rainWorld.options.enabledMods[num3] == ModManager.InstalledMods[m].id)
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					Custom.LogWarning("ENABLED MOD", manager.rainWorld.options.enabledMods[num3], "IS NO LONGER INSTALLED, REMOVING FROM ENABLED");
					flag2 = true;
					ModManager.Mod mod = null;
					foreach (ModManager.Mod installedMod2 in ModManager.InstalledMods)
					{
						if (installedMod2.id == manager.rainWorld.options.enabledMods[num3])
						{
							mod = installedMod2;
							break;
						}
					}
					if (mod != null)
					{
						mod.enabled = false;
					}
					manager.rainWorld.options.enabledMods.RemoveAt(num3);
					filesInBadState = true;
				}
			}
			if (filesInBadState)
			{
				bool flag4 = false;
				while (!flag4)
				{
					flag4 = true;
					for (int num4 = manager.rainWorld.options.enabledMods.Count - 1; num4 >= 0; num4--)
					{
						ModManager.Mod mod2 = null;
						foreach (ModManager.Mod installedMod3 in ModManager.InstalledMods)
						{
							if (installedMod3.id == manager.rainWorld.options.enabledMods[num4])
							{
								mod2 = installedMod3;
								break;
							}
						}
						if (mod2 != null && mod2.requirements.Length != 0)
						{
							bool flag5 = true;
							for (int n = 0; n < mod2.requirements.Length; n++)
							{
								bool flag6 = false;
								for (int num5 = 0; num5 < manager.rainWorld.options.enabledMods.Count; num5++)
								{
									if (manager.rainWorld.options.enabledMods[num5] == mod2.requirements[n])
									{
										flag6 = true;
										break;
									}
								}
								if (!flag6)
								{
									Custom.LogWarning("ENABLED MOD", mod2.id, "NO LONGER MEETS REQUIREMENT", mod2.requirements[n], ", REMOVING FROM ENABLED");
									flag5 = false;
									break;
								}
							}
							if (!flag5)
							{
								flag2 = true;
								flag4 = false;
								mod2.enabled = false;
								manager.rainWorld.options.enabledMods.RemoveAt(num4);
							}
						}
					}
				}
			}
			if (filesInBadState)
			{
				manager.rainWorld.options.Save();
				ModManager.RefreshModsLists(manager.rainWorld);
			}
			if (flag2 || filesInBadState)
			{
				if (!ModManager.GameVersionChangedOnThisLaunch)
				{
					needsReapplyDialog = new DialogBoxNotify(this, pages[0], Translate("mod_menu_reapply_mods"), "REAPPLY", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
					pages[0].subObjects.Add(needsReapplyDialog);
					needsReapplyDialog.timeOut = 0f;
				}
				else
				{
					Singal(null, "REAPPLY");
				}
				currentStep = InitializationStep.APPLY_MODS;
			}
			else
			{
				currentStep = InitializationStep.WAIT_FOR_ASYNC_LOAD;
			}
		}
		else if (currentStep == InitializationStep.APPLY_MODS)
		{
			if (applyingModsDialog != null)
			{
				applyingModsDialog.Update();
			}
			if (modApplyer == null)
			{
				return;
			}
			modApplyer.Update();
			if (modApplyer.IsFinished())
			{
				if (!modApplyer.WasSuccessful() && modApplyerError == null)
				{
					ModManager.ActiveMods = new List<ModManager.Mod>();
					bool[] source = new bool[ModManager.InstalledMods.Count];
					int[] source2 = new int[ModManager.InstalledMods.Count];
					modApplyerError = modApplyer.applyError;
					if (modApplyerError == null)
					{
						modApplyerError = "UNKNOWN ERROR";
					}
					modApplyer = new ModManager.ModApplyer(manager, source.ToList(), source2.Reverse().ToList());
					modApplyer.Start(filesInBadState);
					return;
				}
				manager.rainWorld.options.Save();
				foreach (ModManager.Mod installedMod4 in ModManager.InstalledMods)
				{
					installedMod4.checksumChanged = false;
				}
				HideApplyingModsDialog();
				modApplyerRequiresRestart = modApplyer.RequiresRestart();
				if (modApplyerError != null)
				{
					applyingModsErrorDialog = new DialogBoxNotify(this, pages[0], Translate("mod_menu_error") + " " + modApplyerError, "ERROR", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f), forceWrapping: true);
					pages[0].subObjects.Add(applyingModsErrorDialog);
				}
				else
				{
					AfterModApplyingActions();
				}
				modApplyer = null;
			}
			else
			{
				applyingModsDialog.SetText(Translate("mod_menu_apply_mods") + Environment.NewLine + modApplyer.statusText);
			}
		}
		else if (currentStep == InitializationStep.WAIT_FOR_ASYNC_LOAD)
		{
			if (manager.rainWorld.platformInitialized && manager.rainWorld.OptionsReady && manager.rainWorld.progression.progressionLoaded)
			{
				PlayerProgression.ProgressionLoadResult progressionLoadedResult = manager.rainWorld.progression.progressionLoadedResult;
				if (progressionLoadedResult == PlayerProgression.ProgressionLoadResult.SUCCESS_CREATE_NEW_FILE || progressionLoadedResult == PlayerProgression.ProgressionLoadResult.SUCCESS_LOAD_EXISTING_FILE || progressionLoadedResult == PlayerProgression.ProgressionLoadResult.ERROR_SAVE_DATA_MISSING || ignoreProgressionError)
				{
					currentStep = InitializationStep.MOD_INIT;
					return;
				}
				stepBeforeProgressionFailed = currentStep;
				currentStep = InitializationStep.LOCK_ON_PROGRESSION_FAILED_ERROR;
			}
		}
		else if (currentStep == InitializationStep.MOD_INIT)
		{
			ModManager.WrapModInitHooks();
			Action<string> onIssue = delegate(string restartText)
			{
				requiresRestartDialog = new DialogBoxNotify(this, pages[0], restartText, "RESTART", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
				pages[0].subObjects.Add(requiresRestartDialog);
				currentStep = InitializationStep.REQUIRE_RESTART;
			};
			try
			{
				manager.rainWorld.PreModsInit();
			}
			catch (Exception ex)
			{
				Custom.LogWarning("EXCEPTION IN PreModsInit", ex.Message, "::", ex.StackTrace);
			}
			if (ModManager.CheckInitIssues(onIssue))
			{
				return;
			}
			try
			{
				manager.rainWorld.OnModsInit();
			}
			catch (Exception ex2)
			{
				Custom.LogWarning("EXCEPTION IN OnModsInit", ex2.Message, "::", ex2.StackTrace);
			}
			if (!ModManager.CheckInitIssues(onIssue))
			{
				try
				{
					manager.rainWorld.PostModsInit();
				}
				catch (Exception ex3)
				{
					Custom.LogWarning("EXCEPTION IN PostModsInit", ex3.Message, "::", ex3.StackTrace);
				}
				if (!ModManager.CheckInitIssues(onIssue))
				{
					manager.InitSoundLoader();
					currentStep = InitializationStep.WAIT_FOR_MOD_INIT_ASYNC;
				}
			}
		}
		else if (currentStep == InitializationStep.WAIT_FOR_MOD_INIT_ASYNC)
		{
			if ((manager.musicPlayer == null || manager.musicPlayer.assetBundlesLoaded) && manager.soundLoader.assetBundlesLoaded)
			{
				currentStep = InitializationStep.AOCUpdateAPI;
			}
		}
		else if (currentStep == InitializationStep.AOCUpdateAPI)
		{
			AOCUpdate.Initialize();
			currentStep = InitializationStep.RELOAD_PROGRESSION;
		}
		else if (currentStep == InitializationStep.RELOAD_PROGRESSION)
		{
			if (!reloadedProgression)
			{
				manager.rainWorld.progression.Destroy();
				manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
				if (ignoreProgressionError)
				{
					manager.rainWorld.progression.suppressProgressionError = true;
				}
				reloadedProgression = true;
			}
			else
			{
				if (!manager.rainWorld.progression.progressionLoaded)
				{
					return;
				}
				PlayerProgression.ProgressionLoadResult progressionLoadedResult2 = manager.rainWorld.progression.progressionLoadedResult;
				if (progressionLoadedResult2 == PlayerProgression.ProgressionLoadResult.SUCCESS_CREATE_NEW_FILE || progressionLoadedResult2 == PlayerProgression.ProgressionLoadResult.SUCCESS_LOAD_EXISTING_FILE || progressionLoadedResult2 == PlayerProgression.ProgressionLoadResult.ERROR_SAVE_DATA_MISSING || ignoreProgressionError)
				{
					if (manager.rainWorld.setup.singlePlayerChar != -1)
					{
						manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(manager.rainWorld.setup.singlePlayerChar));
					}
					RainWorld.lastActiveSaveSlot = manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;
					currentStep = InitializationStep.WAIT_STARTUP_DIALOGS;
				}
				else
				{
					stepBeforeProgressionFailed = currentStep;
					currentStep = InitializationStep.LOCK_ON_PROGRESSION_FAILED_ERROR;
				}
			}
		}
		else if (currentStep == InitializationStep.WRAP_UP)
		{
			ModManager.InitializationScreenFinished = true;
			InGameTranslator.UnloadFonts(manager.rainWorld.options.language);
			InGameTranslator.LoadFonts(manager.rainWorld.options.language, this);
			manager.rainWorld.options.ReapplyUnrecognized();
			if (ModManager.MMF)
			{
				Application.targetFrameRate = manager.rainWorld.options.fpsCap;
				if (manager.rainWorld.options.fpsCap > 120)
				{
					Application.targetFrameRate = -1;
				}
				if (manager.rainWorld.options.vsync)
				{
					QualitySettings.vSyncCount = 1;
				}
				else
				{
					QualitySettings.vSyncCount = 0;
				}
			}
			StaticWorld.InitStaticWorld();
			manager.rainWorld.ReadTokenCache();
			manager.rainWorld.options.lastGameVersion = "v1.11.8";
			if (manager.rainWorld.setup.startScreen)
			{
				if (ModManager.MSC && manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
				{
					manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = SlugcatStats.Name.White;
				}
				RainWorld.lastActiveSaveSlot = manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.IntroRoll);
			}
			else
			{
				manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
				ModManager.CoopAvailable = ModManager.JollyCoop;
			}
			currentStep = InitializationStep.WAIT_FOR_PROCESS_CHANGE;
		}
		else if (currentStep == InitializationStep.WAIT_STARTUP_DIALOGS)
		{
			if (!manager.IsRunningAnyDialog)
			{
				currentStep = InitializationStep.WRAP_UP;
			}
		}
		else if (currentStep == InitializationStep.LOCK_ON_PROGRESSION_FAILED_ERROR)
		{
			_ = manager.rainWorld.progression.progressionLoadedResult;
			checkedForBackup = true;
			backupExists = false;
			if (!manager.IsRunningAnyDialog && checkedForBackup && !backupExists)
			{
				Custom.Log("@B@ BACKUP DID NOT EXIST, SO STOPPING AND REPORTING CORRUPTED SAVE FILE ERROR CODE");
				string text5 = Translate("save_file_corrupted");
				string text6 = manager.rainWorld.progression.progressionLoadedResult.ToString();
				if (manager.rainWorld.progression.progressionLoadedResult == PlayerProgression.ProgressionLoadResult.ERROR_READ_FAILED && manager.rainWorld.progression.SaveDataReadFailureError != null)
				{
					text6 = text6 + Environment.NewLine + manager.rainWorld.progression.SaveDataReadFailureError;
				}
				text5 = text5.Replace("{ERROR}", text6);
				progressionFailedError = new DialogConfirm(Custom.ReplaceLineDelimeters(text5), new Vector2(1024.5f, 268.8f), manager, delegate
				{
					manager.rainWorld.progression.Destroy();
					manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
					ReturnToStepBeforeProgressionFailed();
				}, delegate
				{
					currentStep = InitializationStep.SAVE_FILE_FAIL_WARN_PROCEED;
				});
				manager.ShowDialog(progressionFailedError);
			}
		}
		else if (currentStep == InitializationStep.WAIT_FOR_BACKUP_RESTORE)
		{
			if (waitingForBackupRestore)
			{
				return;
			}
			if (restoreBackupDialog != null)
			{
				Custom.Log("@B@ RESTORE BACKUP FINISHED, RESULT IS:", backupRestoreSuccess.ToString());
			}
			HideRestoringBackupDialog();
			if (backupRestoreSuccess)
			{
				manager.rainWorld.progression.Destroy();
				manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
				currentStep = InitializationStep.WAIT_FOR_BACKUP_RECREATION;
			}
			else if (!manager.IsRunningAnyDialog)
			{
				string s = Translate("backup_restore_failed");
				progressionFailedError = new DialogConfirm(Custom.ReplaceLineDelimeters(s), new Vector2(1024.5f, 268.8f), manager, delegate
				{
					manager.rainWorld.progression.Destroy();
					manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
					ReturnToStepBeforeProgressionFailed();
				}, delegate
				{
					currentStep = InitializationStep.SAVE_FILE_FAIL_WARN_PROCEED;
				});
				manager.ShowDialog(progressionFailedError);
			}
		}
		else if (currentStep == InitializationStep.WAIT_FOR_BACKUP_RECREATION)
		{
			if (manager.rainWorld.progression.progressionLoaded)
			{
				PlayerProgression.ProgressionLoadResult progressionLoadedResult3 = manager.rainWorld.progression.progressionLoadedResult;
				if (progressionLoadedResult3 == PlayerProgression.ProgressionLoadResult.SUCCESS_CREATE_NEW_FILE || progressionLoadedResult3 == PlayerProgression.ProgressionLoadResult.SUCCESS_LOAD_EXISTING_FILE || progressionLoadedResult3 == PlayerProgression.ProgressionLoadResult.ERROR_SAVE_DATA_MISSING)
				{
					Custom.Log("@B@ RECREATING SAVE FILE FROM RESTORED BACKUP DATA");
					manager.rainWorld.progression.RecreateSaveFile();
					ReturnToStepBeforeProgressionFailed();
				}
				else
				{
					Custom.Log($"@B@ PROGRESSION FAILED TO LOAD AFTER RESTORING BACKUP: {progressionLoadedResult3}");
					ReturnToStepBeforeProgressionFailed();
				}
			}
		}
		else if (currentStep == InitializationStep.SAVE_FILE_FAIL_WARN_PROCEED)
		{
			if (!manager.IsRunningAnyDialog)
			{
				string text7 = Translate("save_file_error_continue");
				DialogConfirm dialog = new DialogConfirm(size: DialogBoxNotify.CalculateDialogBoxSize(text7, dialogUsesWordWrapping: false), description: Custom.ReplaceLineDelimeters(text7), manager: manager, onOK: delegate
				{
					ignoreProgressionError = true;
					ReturnToStepBeforeProgressionFailed();
				}, onCancel: delegate
				{
					manager.rainWorld.progression.Destroy();
					manager.rainWorld.progression = new PlayerProgression(manager.rainWorld, tryLoad: true, saveAfterLoad: false);
					ReturnToStepBeforeProgressionFailed();
				});
				manager.ShowDialog(dialog);
			}
		}
		else
		{
			if (currentStep != InitializationStep.LOCALIZATION_DEBUG)
			{
				return;
			}
			if (localizationDebugIndex != lastLocalizationDebugIndex)
			{
				Custom.Log(localizationTestMessages[localizationDebugIndex]);
				string text8 = Translate(localizationTestMessages[localizationDebugIndex]);
				DialogNotify dialog2 = new DialogNotify(size: DialogBoxNotify.CalculateDialogBoxSize(text8), description: Custom.ReplaceLineDelimeters(text8), manager: manager, onOK: delegate
				{
					localizationDebugIndex++;
					if (localizationDebugIndex >= localizationTestMessages.Length)
					{
						ReturnToStepBeforeProgressionFailed();
					}
				});
				manager.ShowDialog(dialog2);
			}
			lastLocalizationDebugIndex = localizationDebugIndex;
		}
	}

	public void AfterModApplyingActions()
	{
		if (modApplyerRequiresRestart || needsRelaunch)
		{
			requiresRestartDialog = new DialogBoxNotify(this, pages[0], Translate("mod_menu_restart"), "RESTART", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
			pages[0].subObjects.Add(requiresRestartDialog);
			currentStep = InitializationStep.REQUIRE_RESTART;
		}
		else
		{
			currentStep = InitializationStep.WAIT_FOR_ASYNC_LOAD;
		}
	}

	public void ReturnToStepBeforeProgressionFailed()
	{
		checkedForBackup = false;
		backupExists = false;
		waitingForBackupRestore = false;
		backupRestoreSuccess = false;
		currentStep = stepBeforeProgressionFailed;
	}

	public override void Singal(MenuObject sender, string message)
	{
		base.Singal(sender, message);
		switch (message)
		{
		case "REAPPLY":
		{
			PlaySound(SoundID.MENU_Switch_Arena_Gametype);
			if (needsReapplyDialog != null)
			{
				pages[0].subObjects.Remove(needsReapplyDialog);
				needsReapplyDialog.RemoveSprites();
				needsReapplyDialog = null;
			}
			ShowApplyingModsDialog();
			List<bool> list = new List<bool>();
			List<int> list2 = new List<int>();
			for (int i = 0; i < ModManager.InstalledMods.Count; i++)
			{
				list.Add(ModManager.InstalledMods[i].enabled);
				list2.Add(ModManager.InstalledMods[i].loadOrder);
			}
			modApplyerError = null;
			modApplyerRequiresRestart = false;
			modApplyer = new ModManager.ModApplyer(manager, list, list2);
			modApplyer.Start(filesInBadState);
			break;
		}
		case "RESTART":
			Application.Quit();
			break;
		case "ERROR":
			PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
			pages[0].subObjects.Remove(applyingModsErrorDialog);
			applyingModsErrorDialog.RemoveSprites();
			applyingModsErrorDialog = null;
			AfterModApplyingActions();
			break;
		case "VERSIONPROMPT":
			if (gameVersionChangedDialog != null)
			{
				pages[0].subObjects.Remove(gameVersionChangedDialog);
				gameVersionChangedDialog.RemoveSprites();
				gameVersionChangedDialog = null;
				filesInBadState = true;
			}
			if (manager.mySteamManager != null)
			{
				currentStep = InitializationStep.CHECK_WORKSHOP_CONTENT;
			}
			else
			{
				currentStep = InitializationStep.VALIDATE_MODS;
			}
			break;
		case "RESTOREBACKUP":
			waitingForBackupRestore = false;
			backupRestoreSuccess = false;
			currentStep = InitializationStep.WAIT_FOR_BACKUP_RESTORE;
			checkingForBackup = false;
			break;
		}
	}

	public void OnBackupChecked(bool response)
	{
		Custom.Log("@B@ BACKUP EXISTS RETURN VALUE:", response.ToString());
		backupExists = response;
		checkedForBackup = true;
	}

	public void OnBackupRestored(bool response)
	{
		Custom.Log("@B@ RESTORE BACKUP RETURN VALUE:", response.ToString());
		backupRestoreSuccess = response;
		waitingForBackupRestore = false;
	}

	public void ShowApplyingModsDialog()
	{
		if (applyingModsDialog == null)
		{
			applyingModsDialog = new DialogBoxAsyncWait(this, pages[0], Translate("mod_menu_apply_mods") + Environment.NewLine + " ", new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
			pages[0].subObjects.Add(applyingModsDialog);
		}
	}

	public void HideApplyingModsDialog()
	{
		if (applyingModsDialog != null)
		{
			pages[0].subObjects.Remove(applyingModsDialog);
			applyingModsDialog.RemoveSprites();
			applyingModsDialog = null;
		}
	}

	public void ShowWorkshopDownloadingDialog()
	{
		if (downloadingWorkshopItemsDialog == null)
		{
			downloadingWorkshopItemsDialog = new DialogBoxAsyncWait(this, pages[0], Translate("Downloading Workshop Content"), new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
			pages[0].subObjects.Add(downloadingWorkshopItemsDialog);
		}
	}

	public void HideWorkshopDownloadingDialog()
	{
		if (downloadingWorkshopItemsDialog != null)
		{
			pages[0].subObjects.Remove(downloadingWorkshopItemsDialog);
			downloadingWorkshopItemsDialog.RemoveSprites();
			downloadingWorkshopItemsDialog = null;
		}
	}

	public void ShowRestoringBackupDialog()
	{
		if (restoringBackupDialog == null)
		{
			restoringBackupDialog = new DialogBoxAsyncWait(this, pages[0], Translate("mod_menu_restoring_backup"), new Vector2(manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
			pages[0].subObjects.Add(restoringBackupDialog);
		}
	}

	public void HideRestoringBackupDialog()
	{
		if (restoringBackupDialog != null)
		{
			pages[0].subObjects.Remove(restoringBackupDialog);
			restoringBackupDialog.RemoveSprites();
			restoringBackupDialog = null;
		}
	}

	public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
		base.CommunicateWithUpcomingProcess(nextProcess);
	}
}
