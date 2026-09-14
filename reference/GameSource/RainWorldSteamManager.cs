using System;
using System.Collections.Generic;
using System.IO;
using DevInterface;
using Expedition;
using JollyCoop;
using MoreSlugcats;
using Steamworks;
using UnityEngine;
using Watcher;

public class RainWorldSteamManager : MainLoopProcess
{
	public static uint APP_ID = 312520u;

	public static uint DOWNPOUR_APP_ID = 1933390u;

	public static uint WATCHER_APP_ID = 2857120u;

	public bool shutdown;

	public static ulong ownerUserID;

	public ModManager.Mod creatingMod;

	public bool isCurrentlyCreating;

	public string lastCreateFail = "";

	public bool needsLegalAgreement;

	public ModManager.Mod uploadingMod;

	public bool isCurrentlyUploading;

	public string lastUploadFail = "";

	public ulong bytesProcessed;

	public ulong bytesTotal;

	public bool isCurrentlyQuerying;

	public int lastQueryCount;

	public string lastQueryFail = "";

	public UGCQueryHandle_t lastQueryHandle;

	public List<ulong> lastQueryOwners;

	public List<PublishedFileId_t> lastQueryFiles;

	public List<PublishedFileId_t> currentlyDownloading;

	public int numberItemsAddedForDownloading;

	public UGCUpdateHandle_t updateHandle;

	public CallResult<CreateItemResult_t> createItemCallback;

	public CallResult<SubmitItemUpdateResult_t> updateItemCallback;

	public CallResult<SteamUGCQueryCompleted_t> queryCallback;

	public Callback<DownloadItemResult_t> downloadCallback;

	public RainWorldSteamManager(ProcessManager manager, ProcessManager.ProcessID ID)
		: base(manager, ID)
	{
	}

	public RainWorldSteamManager(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.RainWorldSteamManager)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (SteamManager.Initialized)
		{
			SteamFriends.GetPersonaName();
			ownerUserID = SteamUser.GetSteamID().m_SteamID;
			SteamUserStats.RequestCurrentStats();
			currentlyDownloading = new List<PublishedFileId_t>();
			createItemCallback = CallResult<CreateItemResult_t>.Create((APIDispatchDelegate<CreateItemResult_t>)OnCreateItemResult);
			updateItemCallback = CallResult<SubmitItemUpdateResult_t>.Create((APIDispatchDelegate<SubmitItemUpdateResult_t>)OnUpdateItemResult);
			queryCallback = CallResult<SteamUGCQueryCompleted_t>.Create((APIDispatchDelegate<SteamUGCQueryCompleted_t>)OnQueryResult);
			downloadCallback = Callback<DownloadItemResult_t>.Create((DispatchDelegate<DownloadItemResult_t>)OnDownloadItemResult);
		}
		else
		{
			ShutDown();
		}
	}

	public override void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (SteamManager.Initialized)
		{
			if (isCurrentlyUploading)
			{
				SteamUGC.GetItemUpdateProgress(updateHandle, ref bytesProcessed, ref bytesTotal);
			}
			SteamAPI.RunCallbacks();
		}
	}

	public void ShutDown()
	{
		manager.StopSideProcess(this);
		shutdown = true;
	}

	public string Translate(string t)
	{
		return manager.rainWorld.inGameTranslator.Translate(t);
	}

	public bool HasAchievement(string name)
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return false;
		}
		bool result = default(bool);
		SteamUserStats.GetAchievement(name, ref result);
		return result;
	}

	public void SetAchievement(string name)
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return;
		}
		SteamUserStats.SetAchievement(name);
		SteamUserStats.StoreStats();
	}

	public void ClearAchievement(string name)
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return;
		}
		SteamUserStats.ClearAchievement(name);
		SteamUserStats.StoreStats();
	}

	public static bool ValidateModApplicableForWorkshopUpload(ModManager.Mod mod)
	{
		if (mod.id == global::MoreSlugcats.MoreSlugcats.MOD_ID || mod.id == MMF.MOD_ID || mod.id == global::Expedition.Expedition.MOD_ID || mod.id == global::JollyCoop.JollyCoop.MOD_ID || mod.id == DevTools.MOD_ID || mod.id == global::Watcher.Watcher.MOD_ID)
		{
			return false;
		}
		if (mod.id == "RainWorld_BaseGame" || mod.id == "_TestDummy_")
		{
			return false;
		}
		if (mod.workshopMod)
		{
			return false;
		}
		return true;
	}

	public string ValidateWorkshopModForProblems(ModManager.Mod mod)
	{
		string thumbnailPath = mod.GetThumbnailPath();
		if (File.Exists(thumbnailPath))
		{
			try
			{
				if (new FileInfo(thumbnailPath).Length >= 1000000)
				{
					return Translate("Mod's thumbnail image must be less than 1 MB in size.");
				}
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
				AssetManager.SafeWWWLoadTexture(ref texture2D, thumbnailPath, clampWrapMode: false, crispPixels: true);
				double num = (double)texture2D.height / (double)texture2D.width;
				if (num < 0.5616 || num > 0.5634)
				{
					return Translate("Mod's thumbnail image should have a 16:9 aspect ratio.");
				}
			}
			catch
			{
			}
		}
		if (!ValidateModApplicableForWorkshopUpload(mod))
		{
			return Translate("This mod cannot be uploaded to the Workshop.");
		}
		return null;
	}

	public bool InitCreate()
	{
		if (!SteamManager.Initialized)
		{
			isCurrentlyCreating = false;
			lastCreateFail = Translate("Steam manager not initialized");
			ShutDown();
			return false;
		}
		lastCreateFail = "";
		needsLegalAgreement = false;
		isCurrentlyCreating = true;
		return true;
	}

	public void CreateWorkshopMod(ModManager.Mod mod)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (InitCreate())
		{
			creatingMod = mod;
			SteamAPICall_t val = SteamUGC.CreateItem(new AppId_t(APP_ID), (EWorkshopFileType)0);
			createItemCallback.Set(val, (APIDispatchDelegate<CreateItemResult_t>)null);
		}
	}

	public bool InitUpload()
	{
		if (!SteamManager.Initialized)
		{
			isCurrentlyUploading = false;
			lastUploadFail = Translate("Steam manager not initialized");
			ShutDown();
			return false;
		}
		lastUploadFail = "";
		bytesTotal = 0uL;
		needsLegalAgreement = false;
		isCurrentlyUploading = true;
		return true;
	}

	public bool UploadWorkshopMod(ModManager.Mod mod, bool unlisted)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		if (!InitUpload())
		{
			return false;
		}
		if (mod.workshopId == 0L)
		{
			isCurrentlyUploading = false;
			lastUploadFail = Translate("Cannot update mod with no pre-existing workshop ID.");
			return false;
		}
		uploadingMod = mod;
		updateHandle = SteamUGC.StartItemUpdate(new AppId_t(APP_ID), new PublishedFileId_t(mod.workshopId));
		SteamUGC.SetItemTitle(updateHandle, mod.name);
		SteamUGC.SetItemDescription(updateHandle, mod.description.Replace("<LINE>", Environment.NewLine));
		if (unlisted)
		{
			SteamUGC.SetItemVisibility(updateHandle, (ERemoteStoragePublishedFileVisibility)3);
		}
		SteamUGC.RemoveItemKeyValueTags(updateHandle, "id");
		SteamUGC.RemoveItemKeyValueTags(updateHandle, "version");
		SteamUGC.RemoveItemKeyValueTags(updateHandle, "targetGameVersion");
		SteamUGC.RemoveItemKeyValueTags(updateHandle, "authors");
		SteamUGC.RemoveItemKeyValueTags(updateHandle, "requirements");
		SteamUGC.RemoveItemKeyValueTags(updateHandle, "requirementNames");
		SteamUGC.AddItemKeyValueTag(updateHandle, "id", mod.id);
		SteamUGC.AddItemKeyValueTag(updateHandle, "version", mod.version);
		SteamUGC.AddItemKeyValueTag(updateHandle, "targetGameVersion", mod.targetGameVersion);
		SteamUGC.AddItemKeyValueTag(updateHandle, "authors", mod.authors.Replace("<LINE>", Environment.NewLine));
		SteamUGC.AddItemKeyValueTag(updateHandle, "requirements", string.Join(",", mod.requirements));
		SteamUGC.AddItemKeyValueTag(updateHandle, "requirementNames", string.Join(",", mod.requirementsNames));
		SteamUGC.SetItemTags(updateHandle, (IList<string>)mod.tags);
		SteamUGC.SetItemContent(updateHandle, mod.path);
		string thumbnailPath = mod.GetThumbnailPath();
		if (File.Exists(thumbnailPath))
		{
			try
			{
				if (new FileInfo(thumbnailPath).Length < 1000000)
				{
					SteamUGC.SetItemPreview(updateHandle, thumbnailPath);
				}
			}
			catch
			{
			}
		}
		if (mod.trailerID != null && mod.trailerID != "")
		{
			SteamUGC.AddItemPreviewVideo(updateHandle, mod.trailerID);
		}
		SteamAPICall_t val = SteamUGC.SubmitItemUpdate(updateHandle, "");
		updateItemCallback.Set(val, (APIDispatchDelegate<SubmitItemUpdateResult_t>)null);
		return true;
	}

	public bool InitQuery()
	{
		if (!SteamManager.Initialized)
		{
			isCurrentlyQuerying = false;
			lastQueryFail = Translate("Steam manager not initialized");
			ShutDown();
			return false;
		}
		lastQueryCount = -1;
		lastQueryFail = "";
		isCurrentlyQuerying = true;
		return true;
	}

	public void FindWorkshopItemsWithKeyValue(string key, string value)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (InitQuery())
		{
			AppId_t val = default(AppId_t);
			((AppId_t)(ref val))._002Ector(APP_ID);
			lastQueryHandle = SteamUGC.CreateQueryAllUGCRequest((EUGCQuery)1, (EUGCMatchingUGCType)2, val, val, (string)null);
			SteamUGC.AddRequiredKeyValueTag(lastQueryHandle, key, value);
			SteamAPICall_t val2 = SteamUGC.SendQueryUGCRequest(lastQueryHandle);
			queryCallback.Set(val2, (APIDispatchDelegate<SteamUGCQueryCompleted_t>)null);
		}
	}

	public void ShowLegalAgreement()
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
		}
		else
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/workshoplegalagreement", (EActivateGameOverlayToWebPageMode)0);
		}
	}

	public void ShowWorkshopDetails(ulong workshopId)
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
		}
		else
		{
			SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + workshopId, (EActivateGameOverlayToWebPageMode)0);
		}
	}

	public void ShowStorePage()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamManager.Initialized)
		{
			ShutDown();
		}
		else
		{
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(APP_ID), (EOverlayToStoreFlag)0);
		}
	}

	public void ShowDownpourStorePage()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamManager.Initialized)
		{
			ShutDown();
		}
		else
		{
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(DOWNPOUR_APP_ID), (EOverlayToStoreFlag)0);
		}
	}

	public void ShowWatcherStorePage()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamManager.Initialized)
		{
			ShutDown();
		}
		else
		{
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(WATCHER_APP_ID), (EOverlayToStoreFlag)0);
		}
	}

	public PublishedFileId_t[] GetSubscribedItems()
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return (PublishedFileId_t[])(object)new PublishedFileId_t[0];
		}
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = (PublishedFileId_t[])(object)new PublishedFileId_t[numSubscribedItems];
		if (numSubscribedItems != 0)
		{
			SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		}
		return array;
	}

	public bool DownloadWorkshopMod(PublishedFileId_t fileid)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return false;
		}
		currentlyDownloading.Add(fileid);
		numberItemsAddedForDownloading++;
		bool num = SteamUGC.DownloadItem(fileid, true);
		if (!num)
		{
			currentlyDownloading.Remove(fileid);
			numberItemsAddedForDownloading--;
		}
		return num;
	}

	public void ResetDownloadBatch()
	{
		numberItemsAddedForDownloading = 0;
	}

	public double GetWorkshopDownloadProgress()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return 0.0;
		}
		double num = 1.0 / (double)numberItemsAddedForDownloading;
		double num2 = 0.0;
		int num3 = numberItemsAddedForDownloading - currentlyDownloading.Count;
		num2 += (double)num3 * num;
		ulong num4 = default(ulong);
		ulong num5 = default(ulong);
		for (int i = 0; i < currentlyDownloading.Count; i++)
		{
			if (SteamUGC.GetItemDownloadInfo(currentlyDownloading[i], ref num4, ref num5) && num5 != 0)
			{
				num2 += (double)num4 / (double)num5 * num;
			}
		}
		return num2;
	}

	public void OnCreateItemResult(CreateItemResult_t callback, bool ioFailure)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Invalid comparison between Unknown and I4
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Invalid comparison between Unknown and I4
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Invalid comparison between Unknown and I4
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Invalid comparison between Unknown and I4
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Invalid comparison between Unknown and I4
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Invalid comparison between Unknown and I4
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Invalid comparison between Unknown and I4
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Invalid comparison between Unknown and I4
		if ((int)callback.m_eResult == 1)
		{
			PublishedFileId_t nPublishedFileId = callback.m_nPublishedFileId;
			creatingMod.workshopId = nPublishedFileId.m_PublishedFileId;
			if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				needsLegalAgreement = true;
			}
		}
		else if ((int)callback.m_eResult == 17 || (int)callback.m_eResult == 24)
		{
			lastCreateFail = Translate("Your account is locked or has a community or VAC ban.");
		}
		else if ((int)callback.m_eResult == 16)
		{
			lastCreateFail = Translate("The operation timed out. Try again later.");
		}
		else if ((int)callback.m_eResult == 21)
		{
			lastCreateFail = Translate("You are not currently logged into Steam.");
		}
		else if ((int)callback.m_eResult == 20)
		{
			lastCreateFail = Translate("The workshop service is unavailable. Try again later.");
		}
		else if ((int)callback.m_eResult == 8)
		{
			lastCreateFail = Translate("Some of the metadata for this mod was invalid and cannot be accepted.");
		}
		else if ((int)callback.m_eResult == 15)
		{
			lastCreateFail = Translate("Access was denied. Do you own the game on Steam?");
		}
		else if ((int)callback.m_eResult == 25)
		{
			lastCreateFail = Translate("Limit exceeded. You may need to reduce the filesize or remove prior workshop content.");
		}
		else if ((int)callback.m_eResult == 9)
		{
			lastCreateFail = Translate("Could not find the files to upload.");
		}
		else if ((int)callback.m_eResult == 29)
		{
			lastCreateFail = Translate("This item already exists on the workshop.");
		}
		else if ((int)callback.m_eResult == 14)
		{
			lastCreateFail = Translate("One of your workshop mods already has the same name as this one.");
		}
		else if ((int)callback.m_eResult == 44)
		{
			lastCreateFail = Translate("Your account is in read-only mode due to a recent password or email change.");
		}
		else
		{
			lastCreateFail = ((object)(EResult)(ref callback.m_eResult)).ToString();
		}
		isCurrentlyCreating = false;
	}

	public void OnUpdateItemResult(SubmitItemUpdateResult_t callback, bool ioFailure)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Invalid comparison between Unknown and I4
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Invalid comparison between Unknown and I4
		isCurrentlyUploading = false;
		if ((int)callback.m_eResult == 1)
		{
			if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				needsLegalAgreement = true;
			}
		}
		else if ((int)callback.m_eResult == 15)
		{
			lastUploadFail = Translate("Access was denied. Do you own the game on Steam?");
		}
		else if ((int)callback.m_eResult == 8)
		{
			lastUploadFail = Translate("Some of the metadata for this mod was invalid and cannot be accepted.");
		}
		else if ((int)callback.m_eResult == 9)
		{
			lastUploadFail = Translate("Could not find the files to upload.");
		}
		else if ((int)callback.m_eResult == 25)
		{
			lastUploadFail = Translate("Limit exceeded. You may need to reduce the filesize or remove prior workshop content.");
		}
		else if ((int)callback.m_eResult == 33)
		{
			lastUploadFail = Translate("Failed to acquire User Generated Content lock. Try again later.");
		}
		else if ((int)callback.m_eResult == 2)
		{
			lastUploadFail = Translate("Generic failure.");
		}
		else
		{
			lastUploadFail = ((object)(EResult)(ref callback.m_eResult)).ToString();
		}
		isCurrentlyUploading = false;
	}

	public void OnQueryResult(SteamUGCQueryCompleted_t callback, bool ioFailure)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		lastQueryOwners = new List<ulong>();
		lastQueryFiles = new List<PublishedFileId_t>();
		if ((int)callback.m_eResult == 1)
		{
			lastQueryCount = (int)callback.m_unTotalMatchingResults;
			SteamUGCDetails_t val = default(SteamUGCDetails_t);
			for (int i = 0; i < lastQueryCount; i++)
			{
				if (SteamUGC.GetQueryUGCResult(lastQueryHandle, (uint)i, ref val))
				{
					lastQueryOwners.Add(val.m_ulSteamIDOwner);
					lastQueryFiles.Add(val.m_nPublishedFileId);
				}
			}
		}
		else
		{
			lastQueryFail = ((object)(EResult)(ref callback.m_eResult)).ToString();
			lastQueryCount = -1;
		}
		isCurrentlyQuerying = false;
	}

	public void OnDownloadItemResult(DownloadItemResult_t callback)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (callback.m_unAppID.m_AppId == APP_ID)
		{
			PublishedFileId_t nPublishedFileId = callback.m_nPublishedFileId;
			_ = callback.m_eResult;
			_ = 1;
			if (currentlyDownloading.Contains(nPublishedFileId))
			{
				currentlyDownloading.Remove(nPublishedFileId);
			}
		}
	}
}
