using System.Collections.Generic;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class SpinningTop : Ghost, WarpPoint.IOpenWarps
{
	public class SpinningTopConversation : GhostConversation
	{
		public int timeSinceLastSound;

		public bool laughPlayedThisDialog;

		public SpinningTopConversation(ID id, Ghost ghost, DialogBox dialogBox)
			: base(id, ghost, dialogBox)
		{
		}

		public override void Update()
		{
			timeSinceLastSound++;
			if (ghost is SpinningTop && !paused && !laughPlayedThisDialog && events.Count > 0 && events[0] is TextEvent)
			{
				InGameTranslator.LanguageID currentLanguage = Custom.rainWorld.inGameTranslator.currentLanguage;
				bool flag = false;
				if (currentLanguage == InGameTranslator.LanguageID.English || currentLanguage == InGameTranslator.LanguageID.Italian || currentLanguage == InGameTranslator.LanguageID.Spanish || currentLanguage == InGameTranslator.LanguageID.Portuguese)
				{
					flag = dialogBox.label.text.Contains("OA");
				}
				else if (currentLanguage == InGameTranslator.LanguageID.French)
				{
					flag = dialogBox.label.text.Contains("HOUAHA");
				}
				else if (currentLanguage == InGameTranslator.LanguageID.German)
				{
					flag = dialogBox.label.text.Contains("HOA");
				}
				else if (currentLanguage == InGameTranslator.LanguageID.Korean)
				{
					flag = dialogBox.label.text.Contains("오아");
				}
				else if (currentLanguage == InGameTranslator.LanguageID.Japanese)
				{
					flag = dialogBox.label.text.Contains("おはは") || dialogBox.label.text.Contains("おあ");
				}
				else if (currentLanguage == InGameTranslator.LanguageID.Chinese || currentLanguage == InGameTranslator.LanguageID.TraditionalChinese)
				{
					flag = dialogBox.label.text.Contains("嘻") || dialogBox.label.text.Contains("哇");
				}
				else if (currentLanguage == InGameTranslator.LanguageID.Russian)
				{
					flag = dialogBox.label.text.Contains("ХО");
				}
				else if (currentLanguage == InGameTranslator.LanguageID.Thai)
				{
					flag = dialogBox.label.text.Contains("อ\u0e34\u0e4aอ\u0e34\u0e4a") || dialogBox.label.text.Contains("อ\u0e37ม");
				}
				if (flag)
				{
					int num = SpinningTopLaughLevel((events[0] as TextEvent).text);
					if (num > 0)
					{
						if (num == 1)
						{
							ghost.room.PlaySound(WatcherEnums.WatcherSoundID.Spinning_Top_Laugh_S);
						}
						else
						{
							ghost.room.PlaySound(WatcherEnums.WatcherSoundID.Spinning_Top_Laugh_L);
						}
						laughPlayedThisDialog = true;
						timeSinceLastSound = 0;
						if (ghost is SpinningTop spinningTop)
						{
							spinningTop.timeSinceLastTauntLaugh = 0;
						}
					}
				}
			}
			int count = events.Count;
			base.Update();
			if (ghost is SpinningTop && !paused && count != events.Count && events.Count > 0 && events[0] is TextEvent)
			{
				laughPlayedThisDialog = false;
				if (SpinningTopLaughLevel((events[0] as TextEvent).text) == 0 && (events[0] as TextEvent).text != "..." && timeSinceLastSound >= 120)
				{
					ghost.room.PlaySound(WatcherEnums.WatcherSoundID.Spinning_Top_Voice);
					timeSinceLastSound = 0;
				}
			}
		}

		public int SpinningTopLaughLevel(string dialog)
		{
			InGameTranslator.LanguageID currentLanguage = Custom.rainWorld.inGameTranslator.currentLanguage;
			if (currentLanguage == InGameTranslator.LanguageID.English || currentLanguage == InGameTranslator.LanguageID.Italian || currentLanguage == InGameTranslator.LanguageID.Spanish || currentLanguage == InGameTranslator.LanguageID.Portuguese)
			{
				if (dialog.Contains("OAOA"))
				{
					return 2;
				}
				if (dialog.Contains("OA!") || dialog.Contains("OA "))
				{
					return 1;
				}
			}
			else if (currentLanguage == InGameTranslator.LanguageID.French)
			{
				if (dialog.Contains("HOUAHAHA"))
				{
					return 2;
				}
				if (dialog.Contains("HOUAHA"))
				{
					return 1;
				}
			}
			else if (currentLanguage == InGameTranslator.LanguageID.German)
			{
				if (dialog.Contains("HOAHOA"))
				{
					return 2;
				}
				if (dialog.Contains("HOA"))
				{
					return 1;
				}
			}
			else if (currentLanguage == InGameTranslator.LanguageID.Korean)
			{
				if (dialog.Contains("오아오아"))
				{
					return 2;
				}
				if (dialog.Contains("오아"))
				{
					return 1;
				}
			}
			else if (currentLanguage == InGameTranslator.LanguageID.Japanese)
			{
				if (dialog.Contains("おははおはは") || dialog.Contains("おあおあ"))
				{
					return 2;
				}
				if (dialog.Contains("おはは") || dialog.Contains("おあ"))
				{
					return 1;
				}
			}
			else if (currentLanguage == InGameTranslator.LanguageID.Chinese || currentLanguage == InGameTranslator.LanguageID.TraditionalChinese)
			{
				if (dialog.Contains("嘻嘻"))
				{
					return 2;
				}
				if (dialog.Contains("哇") || dialog.Contains("嘻"))
				{
					return 1;
				}
			}
			else if (currentLanguage == InGameTranslator.LanguageID.Russian)
			{
				if (dialog.Contains("ХОAХОA"))
				{
					return 2;
				}
				if (dialog.Contains("ХОA"))
				{
					return 1;
				}
			}
			else if (currentLanguage == InGameTranslator.LanguageID.Thai)
			{
				if (dialog.Contains("อ\u0e34\u0e4aอ\u0e34\u0e4aอ\u0e34\u0e4aอ\u0e34\u0e4a"))
				{
					return 2;
				}
				if (dialog.Contains("อ\u0e34\u0e4aอ\u0e34\u0e4a") || dialog.Contains("อ\u0e37ม"))
				{
					return 1;
				}
			}
			return 0;
		}

		public override void AddEvents()
		{
			if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_V1)
			{
				LoadEventsFromFile(200);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_V2)
			{
				LoadEventsFromFile(201);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_V3)
			{
				LoadEventsFromFile(202);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_N1)
			{
				LoadEventsFromFile(203);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_N2)
			{
				LoadEventsFromFile(204);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_N3)
			{
				LoadEventsFromFile(205);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_N4)
			{
				LoadEventsFromFile(206);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_N5)
			{
				LoadEventsFromFile(211);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_N6)
			{
				LoadEventsFromFile(212);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_N7)
			{
				LoadEventsFromFile(213);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_RIP1)
			{
				LoadEventsFromFile(207);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_ROT1)
			{
				LoadEventsFromFile(208);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_AU1)
			{
				LoadEventsFromFile(210);
			}
			else if (ModManager.Watcher && id == WatcherEnums.ConversationID.Ghost_ST_AU2)
			{
				LoadEventsFromFile(209);
			}
		}
	}

	public List<UrbanCandle> candlesToMonitorInBathScene;

	public FadeOut fadeOutTransition;

	public int fadeOutFinishedCounter;

	public bool justMarkedAscended;

	public bool vanillaEncounterEnded;

	public int vanillaEncounterNumber = -1;

	public int timeSinceLastTauntLaugh;

	public float warpAnim;

	public float lastWarpAnim;

	public float warpDuration = 120f;

	public WarpPoint spawnedWarp;

	public VoidWeaverFade fadeOut2;

	public bool BathScene
	{
		get
		{
			if (room != null)
			{
				return room.abstractRoom.name.ToLowerInvariant() == "waua_bath";
			}
			return false;
		}
	}

	public bool BedroomScene
	{
		get
		{
			if (room != null)
			{
				return room.abstractRoom.name.ToLowerInvariant() == "waua_toys";
			}
			return false;
		}
	}

	public bool Ascended
	{
		get
		{
			if (room != null && room.game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow)
			{
				return !justMarkedAscended;
			}
			return false;
		}
	}

	public bool vanillaEncounter
	{
		get
		{
			if ((vanillaEncounterNumber == 1 || vanillaEncounterNumber == 2) && room != null)
			{
				return Region.IsWatcherVanillaRegion(room.world.name);
			}
			return false;
		}
	}

	public bool vanillaToRippleEncounter
	{
		get
		{
			if (vanillaEncounterNumber == 3 && room != null)
			{
				return Region.IsWatcherVanillaRegion(room.world.name);
			}
			return false;
		}
	}

	public SpinningTopData SpecialData => placedObject.data as SpinningTopData;

	public SpinningTop(Room room, PlacedObject placedObject, GhostWorldPresence worldGhost)
		: base(room, placedObject, worldGhost)
	{
		float maximumRippleLevel = (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.maximumRippleLevel;
		vanillaEncounterNumber = ((maximumRippleLevel == 0f) ? 1 : ((maximumRippleLevel == 0.25f) ? 2 : ((maximumRippleLevel != 0.5f) ? (-1) : 3)));
	}

	public override void InitializeSprites()
	{
		scale = 0.5f;
		snoutSegments = 0;
		totalStaticSprites++;
		base.InitializeSprites();
	}

	public void DespawnEcho()
	{
		if (!base.slatedForDeletetion)
		{
			if (spawnedWarp != null && spawnedWarp.nonPlayerRequestOpen == this)
			{
				spawnedWarp.nonPlayerRequestOpen = null;
			}
			base.slatedForDeletetion = true;
		}
	}

	public void FadeOutTransitionCompleted()
	{
		fadeOutFinishedCounter++;
		if (BathScene && fadeOutFinishedCounter == 120)
		{
			GoToVoidBathSlideshow();
		}
	}

	public void GoToVoidBathSlideshow()
	{
		if (!(room.game.manager.upcomingProcess != null))
		{
			room.game.manager.nextSlideshow = WatcherEnums.SlideShowID.EndingVoidBath;
			room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
		}
	}

	public override bool OnScreen()
	{
		bool flag = !SpecialData.rippleWarp || (SpecialData.rippleWarp && room.game.ActiveRippleLayer == 1);
		return room.VisibleInAnyCameraScreenBounds(pos) && flag;
	}

	public override void FadeOutFinished()
	{
		hasRequestedShutDown = true;
		room.game.GetStorySession.saveState.sessionEndingFromSpinningTopEncounter = true;
		room.game.Win(malnourished: false, fromWarpPoint: false);
		RainWorldGame.ForceSaveNewDenLocation(room.game, "HI_W05", saveWorldStates: false);
	}

	public override void Update(bool eu)
	{
		if (fadeOut2 != null)
		{
			if (fadeOut2.RampingDown)
			{
				fadeOut2.speed = 0.01f;
				DespawnEcho();
			}
			base.Update(eu);
			fadeOut = 0f;
			return;
		}
		if (spawnedWarp != null)
		{
			WarpAnimationUpdate();
			base.Update(eu);
			fadeOut = 0f;
			return;
		}
		if (!base.slatedForDeletetion)
		{
			for (int i = 0; i < room.warpPoints.Count; i++)
			{
				room.warpPoints[i].ripple?.RemoveFromRoom();
				room.warpPoints[i].warpTear?.RemoveFromRoom();
				room.warpPoints[i]?.RemoveFromRoom();
			}
		}
		if (fadeOutTransition != null && fadeOutTransition.IsDoneFading())
		{
			FadeOutTransitionCompleted();
		}
		if (!Ascended)
		{
			timeSinceLastTauntLaugh++;
			if (SpecialData.rippleWarp && room.game.ActiveRippleLayer != 1 && room.ViewedByAnyCamera(pos, 0f))
			{
				if (room.game.GetStorySession.saveState.deathPersistentSaveData.rippleLevel < 5f)
				{
					room.game.GetStorySession.insufficientTerraceEncounterThisCycle = true;
				}
				if (Random.value < 0.005f && timeSinceLastTauntLaugh >= 160)
				{
					room.PlaySound((Random.value < 0.5f) ? WatcherEnums.WatcherSoundID.Spinning_Top_Laugh_S : WatcherEnums.WatcherSoundID.Spinning_Top_Laugh_L);
					room.AddObject(new ReverseShockwave(pos, 300f, Random.Range(0.2f, 0.45f), Random.Range(80, 120)));
					timeSinceLastTauntLaugh = 0;
				}
			}
		}
		if ((conversationFinished || Ascended) && !vanillaEncounterEnded)
		{
			MarkSpinningTopEncountered();
			if (justMarkedEncountered || !room.game.GetStorySession.saveState.deathPersistentSaveData.spinningTopEncounters.Contains(SpecialData.spawnIdentifier))
			{
				if (BedroomScene)
				{
					if (fadeOut2 == null)
					{
						if (!Ascended)
						{
							room.PlaySound(WatcherEnums.WatcherSoundID.Spinning_Top_Vanish);
						}
						fadeOut2 = new VoidWeaverFade(pos);
						room.AddObject(fadeOut2);
					}
					fadeOut2.setPos = pos;
					fadeOut2.speed = 0.02f;
				}
				else if (BathScene)
				{
					bool flag = false;
					if (candlesToMonitorInBathScene == null)
					{
						UrbanCandle.SetRoomTargetBurnRate(room, 2.5f);
						candlesToMonitorInBathScene = new List<UrbanCandle>();
						for (int j = 0; j < room.updateList.Count; j++)
						{
							if (room.updateList[j] is UrbanCandle item)
							{
								candlesToMonitorInBathScene.Add(item);
							}
						}
						flag = true;
					}
					else
					{
						for (int k = 0; k < candlesToMonitorInBathScene.Count; k++)
						{
							if (candlesToMonitorInBathScene[k].height > 0f)
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag && fadeOutTransition == null)
					{
						fadeOutFinishedCounter = 0;
						fadeOutTransition = new FadeOut(room, Color.black, 200f, fadeIn: false);
						room.AddObject(fadeOutTransition);
					}
				}
				else
				{
					if (CanRaiseRippleLevel() && !vanillaToRippleEncounter)
					{
						RaiseRippleLevel(room);
					}
					if (vanillaEncounter)
					{
						VanillaRegionSpinningTopEncounter();
					}
					else if (spawnedWarp == null)
					{
						SpawnWarpPoint();
						if (Ascended)
						{
							DespawnEcho();
						}
						else
						{
							room.PlaySound(WatcherEnums.WatcherSoundID.Spinning_Top_Vanish);
						}
					}
				}
			}
		}
		base.Update(eu);
		bool flag2 = false;
		bool flag3 = false;
		for (int l = 0; l < room.game.Players.Count; l++)
		{
			Player player = null;
			if (room.game.Players[l].realizedCreature != null)
			{
				player = room.game.Players[l].realizedCreature as Player;
			}
			if (player != null)
			{
				bool flag4 = SpecialData.rippleWarp && room.game.ActiveRippleLayer == 0 && player.performingActivationTimer == 0 && player.activateCamoTimer == 0;
				if (player.room == room && !player.dead && !flag4)
				{
					flag3 = true;
				}
				if (!player.dead)
				{
					flag2 = true;
				}
			}
		}
		if (!vanillaEncounter)
		{
			fadeOut = 0f;
		}
		if (conversationActive && !flag3 && flag2 && currentConversation.events.Count > 1)
		{
			PauseConversation("LeaveSpinningTop" + InterjectionAppend());
		}
		conversationStartedButPlayerLeftCounter = 0;
		if (!conversationActive && currentConversation != null && onScreenCounter > 80 && flag2)
		{
			ResumeConversation("ReturnToSpinningTop" + InterjectionAppend());
		}
		if (conversationActive && !conversationFinished && currentConversation != null && !flag2)
		{
			PauseConversation("DieAtSpinningTop" + InterjectionAppend());
		}
	}

	public override void StartConversation()
	{
		Conversation.ID iD = Conversation.ID.None;
		Vector2 vector = NextMinMaxRippleLevel(room);
		float y = vector.y;
		float x = vector.x;
		if (BathScene)
		{
			iD = WatcherEnums.ConversationID.Ghost_ST_AU1;
			if (room.shortCutsReady)
			{
				room.shortcuts[0].shortCutType = ShortcutData.Type.DeadEnd;
			}
		}
		else if (BedroomScene)
		{
			iD = WatcherEnums.ConversationID.Ghost_ST_AU2;
		}
		else if (Region.IsVanillaSentientRotRegion(room.world.name))
		{
			iD = WatcherEnums.ConversationID.Ghost_ST_ROT1;
		}
		else if (!Region.IsAncientUrbanRegion(room.world.name))
		{
			if (SpecialData != null && SpecialData.rippleWarp)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_RIP1;
			}
			else if (y == 0.25f)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_V1;
			}
			else if (y == 0.5f)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_V2;
			}
			else if (y == 1f)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_V3;
			}
			else if (y == 1.5f)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_N1;
			}
			else if (y == 3f)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_N5;
			}
			else if (y == 4.5f)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_N2;
			}
			else if (x == 3f)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_N3;
			}
			else if (x == 4f)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_N6;
			}
			else if (x == 5f)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_N7;
			}
			int num = 0;
			bool flag = false;
			for (int i = 0; i < room.game.GetStorySession.saveState.deathPersistentSaveData.spinningTopEncounters.Count; i++)
			{
				if (room.game.GetStorySession.saveState.deathPersistentSaveData.spinningTopEncounters[i] == num)
				{
					flag = true;
					break;
				}
			}
			if (SpecialData != null && SpecialData.spawnIdentifier == num && room.world.name.ToLowerInvariant() == "wtdb")
			{
				if (iD != Conversation.ID.None)
				{
					room.game.GetStorySession.saveState.miscWorldSaveData.hasDeferredSpinningTopConversation = true;
					room.game.GetStorySession.saveState.miscWorldSaveData.deferredSpinningTopConversation = iD;
				}
				iD = WatcherEnums.ConversationID.Ghost_ST_N4;
			}
			else if (x == 4.5f && !flag)
			{
				iD = WatcherEnums.ConversationID.Ghost_ST_N7;
			}
			else if (iD == Conversation.ID.None && room.game.GetStorySession.saveState.miscWorldSaveData.hasDeferredSpinningTopConversation)
			{
				iD = room.game.GetStorySession.saveState.miscWorldSaveData.deferredSpinningTopConversation;
				room.game.GetStorySession.saveState.miscWorldSaveData.hasDeferredSpinningTopConversation = false;
			}
		}
		if (room.game.cameras[0].hud.dialogBox == null)
		{
			room.game.cameras[0].hud.InitDialogBox();
		}
		currentConversation = new SpinningTopConversation(iD, this, room.game.cameras[0].hud.dialogBox);
		if (iD == Conversation.ID.None)
		{
			currentConversation.events.Add(new Conversation.TextEvent(currentConversation, 0, "...", 300));
		}
	}

	public void PauseConversation(string s)
	{
		int extraLinger = 10;
		if (!room.game.rainWorld.inGameTranslator.TryTranslate(s, out s))
		{
			s = "...";
			extraLinger = 0;
		}
		currentConversation.Interrupt(s, extraLinger);
		conversationActive = false;
		onScreenCounter = 0;
	}

	public void ResumeConversation(string s)
	{
		if (room.game.rainWorld.inGameTranslator.TryTranslate(s, out s))
		{
			currentConversation.ForceAddMessage(s, 10);
		}
		conversationActive = true;
	}

	public string InterjectionAppend()
	{
		if (vanillaEncounterNumber != -1)
		{
			return vanillaEncounterNumber.ToString();
		}
		return room.world.name;
	}

	public void SpawnWarpPoint()
	{
		SpinningTopData specialData = SpecialData;
		if (specialData == null)
		{
			return;
		}
		PlacedObject placedObject = new PlacedObject(PlacedObject.Type.WarpPoint, specialData.CreateWarpPointData(room));
		placedObject.pos = pos;
		WarpPoint warpPoint = room.TrySpawnWarpPoint(placedObject);
		if (warpPoint == null)
		{
			return;
		}
		warpPoint.nonPlayerRequestOpen = this;
		spawnedWarp = warpPoint;
		if (!specialData.rippleWarp)
		{
			warpPoint.triggerTime = (int)(warpPoint.triggerActivationTime - 1f);
			if (!Ascended)
			{
				warpPoint.strongPull = true;
			}
			warpPoint.guaranteeTrigger = true;
		}
		if ((CanRaiseRippleLevel() || vanillaToRippleEncounter) && (warpPoint == null || !warpPoint.CheckCanWarpToVoidWeaverEnding()))
		{
			room.game.GetStorySession.spinningTopWarpsLeadingToRippleScreen.Add(warpPoint.MyIdentifyingString());
		}
	}

	public static void SpawnBackupWarpPoint(Room room, PlacedObject o)
	{
		if (room.abstractRoom.name.ToLowerInvariant() == "waua_bath" || room.abstractRoom.name.ToLowerInvariant() == "waua_toys" || Region.IsWatcherVanillaRegion(room.world.name))
		{
			return;
		}
		WarpPoint.WarpPointData warpPointData = (o.data as SpinningTopData).CreateWarpPointData(room);
		PlacedObject placedObject = new PlacedObject(PlacedObject.Type.WarpPoint, warpPointData);
		placedObject.pos = o.pos;
		bool flag = false;
		foreach (WarpPoint warpPoint in room.warpPoints)
		{
			if (warpPoint.Data.destRoom == warpPointData.destRoom && Vector2.Distance(warpPoint.pos, placedObject.pos) < 10f)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			room.TrySpawnWarpPoint(placedObject);
		}
	}

	public bool CanRaiseRippleLevel()
	{
		if (!Ascended && room.world.name.ToLowerInvariant() != "waua" && !Region.IsSentientRotRegion(room.world.name))
		{
			return !SpecialData.rippleWarp;
		}
		return false;
	}

	public static void RaiseRippleLevel(Room room)
	{
		Vector2 vector = NextMinMaxRippleLevel(room);
		(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.minimumRippleLevel = vector.x;
		(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.maximumRippleLevel = vector.y;
		(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.rippleLevel = vector.y;
		room.game.cameras[0].hud.karmaMeter.UpdateGraphic();
		room.game.cameras[0].hud.karmaMeter.forceVisibleCounter = Mathf.Max(room.game.cameras[0].hud.karmaMeter.forceVisibleCounter, 120);
	}

	public static Vector2 NextMinMaxRippleLevel(Room room)
	{
		float num = 0f;
		float num2 = 0f;
		if ((room.game.session as StoryGameSession).saveState.deathPersistentSaveData.maximumRippleLevel == 0f)
		{
			num2 = 0.25f;
			num = num2;
		}
		else if ((room.game.session as StoryGameSession).saveState.deathPersistentSaveData.rippleLevel == 0.25f)
		{
			num2 = 0.5f;
			num = num2;
		}
		else if ((room.game.session as StoryGameSession).saveState.deathPersistentSaveData.rippleLevel == 0.5f)
		{
			num2 = 1f;
			num = num2;
		}
		else
		{
			num2 = Mathf.Min(5f, (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.maximumRippleLevel + 0.5f);
			num = ((!((room.game.session as StoryGameSession).saveState.deathPersistentSaveData.maximumRippleLevel >= 5f)) ? Mathf.Max(1f, num2 - 2f) : Mathf.Min(5f, (room.game.session as StoryGameSession).saveState.deathPersistentSaveData.minimumRippleLevel + 0.5f));
		}
		return new Vector2(num, num2);
	}

	public void VanillaRegionSpinningTopEncounter()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < WatcherRoomSpecificScript.sentientRotVanillaRooms.Length; i++)
		{
			string text = WatcherRoomSpecificScript.sentientRotVanillaRooms[i];
			string text2 = text.Split('_')[0].ToLowerInvariant();
			if (!list.Contains(text2))
			{
				if (!room.game.GetStorySession.saveState.miscWorldSaveData.extraSentientRotTicks.ContainsKey(text2))
				{
					room.game.GetStorySession.saveState.miscWorldSaveData.extraSentientRotTicks[text2] = 0;
				}
				room.game.GetStorySession.saveState.miscWorldSaveData.extraSentientRotTicks[text2] += 20;
				list.Add(text2);
			}
			if (!room.game.GetStorySession.saveState.miscWorldSaveData.pendingSentientRotInfections.Contains(text))
			{
				room.game.GetStorySession.saveState.miscWorldSaveData.pendingSentientRotInfections.Add(text);
			}
		}
		vanillaEncounterEnded = true;
	}

	public void MarkSpinningTopEncountered()
	{
		if (Region.IsVanillaSentientRotRegion(room.world.name))
		{
			room.game.GetStorySession.saveState.deathPersistentSaveData.spinningTopRotEncounter = true;
		}
		if (justMarkedEncountered)
		{
			return;
		}
		SpinningTopData specialData = SpecialData;
		if (specialData == null)
		{
			return;
		}
		justMarkedEncountered = true;
		if (!room.game.GetStorySession.saveState.deathPersistentSaveData.spinningTopEncounters.Contains(specialData.spawnIdentifier))
		{
			room.game.GetStorySession.saveState.deathPersistentSaveData.spinningTopEncounters.Add(specialData.spawnIdentifier);
		}
		if (BathScene)
		{
			room.game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow = true;
			room.game.GetStorySession.saveState.skipNextCycleFoodDrain = true;
			SaveState.forcedEndRoomToAllowwSave = room.abstractRoom.name;
			room.game.GetStorySession.saveState.BringUpToDate(room.game);
			SaveState.forcedEndRoomToAllowwSave = "";
			room.game.AppendCycleToStatisticsForPlayers();
			RainWorldGame.ForceSaveNewDenLocation(room.game, room.abstractRoom.name, saveWorldStates: false);
			justMarkedAscended = true;
		}
		for (int num = room.world.spinningTopPresences.Count - 1; num >= 0; num--)
		{
			if (room.world.spinningTopPresences[num].spinningTopSpawnId == specialData.spawnIdentifier)
			{
				room.world.spinningTopPresences.RemoveAt(num);
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		bool rippleWarp = SpecialData.rippleWarp;
		sLeaser.sprites[base.BodyMeshSprite].shader = rCam.game.rainWorld.Shaders[rippleWarp ? "RippleBasicRippleSide" : "RippleBasicBothSides"];
		sLeaser.sprites[base.HeadMeshSprite].shader = rCam.game.rainWorld.Shaders[rippleWarp ? "GhostSkinRipple" : "GhostSkin"];
		sLeaser.sprites[base.NeckConnectorSprite].shader = rCam.game.rainWorld.Shaders[rippleWarp ? "RippleBasicRippleSide" : "RippleBasicBothSides"];
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			sLeaser.sprites[ThightSprite(i)].shader = rCam.game.rainWorld.Shaders[rippleWarp ? "GhostSkinRipple" : "GhostSkin"];
			sLeaser.sprites[LowerLegSprite(i)].shader = rCam.game.rainWorld.Shaders[rippleWarp ? "GhostSkinRipple" : "GhostSkin"];
			sLeaser.sprites[ButtockSprite(i)].shader = rCam.game.rainWorld.Shaders[rippleWarp ? "RippleBasicRippleSide" : "RippleBasicBothSides"];
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (!(warpAnim > 0f))
		{
			return;
		}
		float value = Mathf.Lerp(lastWarpAnim, warpAnim, timeStacker);
		float num = Mathf.InverseLerp(80f, 40f, value);
		Shader.SetGlobalFloat("_echoAlpha", num);
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (i == base.FadeSprite)
			{
				continue;
			}
			if (sLeaser.sprites[i] is TriangleMesh { customColor: not false } triangleMesh)
			{
				for (int j = 0; j < triangleMesh.verticeColors.Length; j++)
				{
					triangleMesh.verticeColors[j] = Color.Lerp(blackColor, triangleMesh.verticeColors[j], Mathf.InverseLerp(80f, 40f, value));
					if (triangleMesh.shader.name == "GhostSkin" || triangleMesh.shader.name == "GhostBall")
					{
						triangleMesh.verticeColors[j] += new Color(0f, Mathf.Pow(num * 0.5f, 2f), 0f);
					}
				}
			}
			if (i == base.DistortionSprite || i == base.LightSprite)
			{
				sLeaser.sprites[i].scale *= Mathf.Pow(Mathf.InverseLerp(40f, 10f, value), 2f);
			}
			else if (sLeaser.sprites[i].shader.name != "GhostSkin" && sLeaser.sprites[i].shader.name != "GhostBall")
			{
				sLeaser.sprites[i].alpha = Mathf.Pow(num, 2f);
			}
			else
			{
				sLeaser.sprites[i].color += new Color(0f, Mathf.Pow(Mathf.InverseLerp(0f, 60f, value) * 0.5f, 2f), 0f);
			}
		}
		for (int k = 0; k < rags.segments.Length; k++)
		{
			TriangleMesh triangleMesh2 = sLeaser.sprites[rags.firstSprite + k] as TriangleMesh;
			float num2 = 1f / (float)((rags.segments[k].GetLength(0) - 1) * 2 + 1);
			for (int l = 0; l < triangleMesh2.UVvertices.Length; l++)
			{
				triangleMesh2.UVvertices[l].x = Mathf.Lerp(1f, (l % 2 == 0) ? 0f : 1f, num);
				triangleMesh2.UVvertices[l].y = Mathf.Lerp(1f, (float)(l / 2) * num2, num);
			}
		}
	}

	public void WarpAnimationUpdate()
	{
		lastWarpAnim = warpAnim;
		if (spawnedWarp.visualOpenness > 0.5f)
		{
			warpAnim += 1f;
		}
		if (warpAnim > 10f && timeSinceLastTauntLaugh > 100)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Spinning_Top_Laugh_L);
			timeSinceLastTauntLaugh = 0;
		}
		if (warpAnim > warpDuration)
		{
			DespawnEcho();
		}
	}

	public void WarpAnimatePart(Part part)
	{
		part.vel *= 0.6f;
		part.Update();
		if (warpAnim == 1f)
		{
			part.vel += Custom.PerpendicularVector(part.pos, pos) * 5f;
		}
		float num = Mathf.Min(1f, warpAnim / 100f);
		float usePullRadiusThreshold = Mathf.Lerp(550f, 0f, Mathf.Pow(num, 5f));
		part.vel = spawnedWarp.OrbitVector(part.vel, part.pos, usePullRadiusThreshold, 1f, num, airResistance);
	}

	public Vector2 WarpAnimateChainsAndRags(Vector2 chainPos, Vector2 vel)
	{
		float num = Mathf.Min(1f, (warpAnim + 20f) / 80f);
		float usePullRadiusThreshold = Mathf.Lerp(550f, 0f, Mathf.Pow(num, 5f));
		vel = spawnedWarp.OrbitVector(vel, chainPos, usePullRadiusThreshold, 0.25f, num, airResistance);
		vel += Custom.DirVec(chainPos, pos) * Custom.LerpMap(warpAnim, 40f, 120f, 0f, 0.5f);
		return vel;
	}

	public bool Entering()
	{
		return spawnedWarp != null;
	}

	public int FadeDuration()
	{
		if (!spawnedWarp.warpTearOpenTriggered)
		{
			spawnedWarp.warpTear.fadeAnim = 0f;
			spawnedWarp.warpTear.openAnimation = 1f;
			spawnedWarp.fadeInOnSpawn = true;
		}
		else
		{
			spawnedWarp.fadeInOnSpawn = false;
		}
		return 40;
	}

	public void UpdateShaders(float timeStacker)
	{
		if (room.game.cameras[0].warpPointTimer == null)
		{
			float num = Mathf.Lerp(lastWarpAnim, warpAnim, timeStacker);
			float f = 1f - Mathf.Abs(num / warpDuration - 0.5f) * 2f;
			f = Mathf.Pow(f, 0.8f);
			if (room != null)
			{
				room.game.cameras[0].virtualMicrophone.globalSoundMuffle = Mathf.Clamp01((f - 0.5f) * 2f);
				room.game.cameras[0].virtualMicrophone.warpTransition = Mathf.Clamp01(f) * 4f;
			}
			Shader.SetGlobalFloat("_warpPointTime", f * 0.3f);
		}
	}
}
