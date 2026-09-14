using System;
using System.Collections.Generic;
using System.Linq;
using HUD;
using MoreSlugcats;
using Music;
using RWCustom;
using UnityEngine;

namespace Watcher;

public static class WatcherRoomSpecificScript
{
	public class InfectSentientRot : UpdatableAndDeletable
	{
		public bool firstTimeRealized;

		public InfectSentientRot(Room room)
		{
			base.room = room;
			firstTimeRealized = room.abstractRoom.firstTimeRealized;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (firstTimeRealized)
			{
				room.InfectRoomWithSentientRot(0.25f);
			}
			if ((room.abstractRoom.name == "HI_W05" || room.game.GetStorySession.finalWarpSequenceStarted) && (room.game.GetStorySession.saveState.deathPersistentSaveData.maximumRippleLevel == 0f || room.game.GetStorySession.finalWarpSequenceStarted))
			{
				for (int num = room.updateList.Count - 1; num >= 0; num--)
				{
					if (room.updateList[num] is DaddyCorruption)
					{
						room.RemoveObject(room.updateList[num]);
					}
				}
			}
			Destroy();
		}
	}

	public class HI_W14 : UpdatableAndDeletable
	{
		public class Phase : ExtEnum<Phase>
		{
			public static Phase Init = new Phase("Init", register: true);

			public static Phase Delay = new Phase("Delay", register: true);

			public static Phase PlayerRun = new Phase("PlayerRun", register: true);

			public static Phase PlayerDuck = new Phase("PlayerDuck", register: true);

			public static Phase PlayerJump = new Phase("PlayerJump", register: true);

			public static Phase PlayerRoll = new Phase("PlayerRoll", register: true);

			public static Phase PlayerUnduck = new Phase("PlayerUnduck", register: true);

			public static Phase End = new Phase("End", register: true);

			public Phase(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public class StartController : Player.PlayerController
		{
			public HI_W14 owner;

			public StartController(HI_W14 owner)
			{
				this.owner = owner;
			}

			public override Player.InputPackage GetInput()
			{
				return owner.GetInput();
			}
		}

		public Player player;

		public Phase phase;

		public int timeInPhase;

		public int playerIndex;

		public bool startedRoll;

		public StartController startController;

		public HI_W14(Room room, int playerIndex)
		{
			phase = Phase.Init;
			base.room = room;
			this.playerIndex = playerIndex;
			Custom.rainWorld.progression.miscProgressionData.started_Watcher_Campaign = true;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (player == null && room.game.AllPlayersRealized)
			{
				AbstractCreature abstractCreature = room.game.Players[playerIndex];
				if (abstractCreature != null && abstractCreature.realizedCreature != null)
				{
					player = abstractCreature.realizedCreature as Player;
				}
			}
			if (player == null || player.room != room)
			{
				return;
			}
			timeInPhase++;
			if (player.mainBodyChunk.pos.y <= 80f)
			{
				for (int i = 0; i < player.bodyChunks.Length; i++)
				{
					player.bodyChunks[i].HardSetPosition(room.MiddleOfTile(44, 9));
				}
			}
			if (phase == Phase.Init)
			{
				for (int j = 0; j < player.bodyChunks.Length; j++)
				{
					player.bodyChunks[j].HardSetPosition(room.MiddleOfTile(4 * (room.game.Players.Count - 1) - 2 * playerIndex, 26));
				}
				if (player.graphicsModule != null)
				{
					player.graphicsModule.Reset();
				}
				startController = new StartController(this);
				player.controller = startController;
				player.standing = true;
				ChangePhase(Phase.Delay);
			}
			else if (phase == Phase.Delay)
			{
				if (timeInPhase > 50 + 40 * playerIndex)
				{
					ChangePhase(Phase.PlayerRun);
				}
			}
			else if (phase == Phase.PlayerRun)
			{
				if (player.mainBodyChunk.pos.x > 600f)
				{
					ChangePhase(Phase.PlayerDuck);
				}
			}
			else if (phase == Phase.PlayerDuck)
			{
				if (player.bodyMode == Player.BodyModeIndex.Crawl)
				{
					ChangePhase(Phase.PlayerJump);
				}
			}
			else if (phase == Phase.PlayerJump)
			{
				if (player.bodyMode != Player.BodyModeIndex.Crawl)
				{
					ChangePhase(Phase.PlayerDuck);
				}
				else if (player.superLaunchJump >= 20)
				{
					ChangePhase(Phase.PlayerRoll);
				}
			}
			else if (phase == Phase.PlayerRoll)
			{
				if (player.animation == Player.AnimationIndex.Roll)
				{
					startedRoll = true;
				}
				if (timeInPhase > 15 && player.canJump > 0 && !startedRoll && player.animation != Player.AnimationIndex.Roll)
				{
					room.PlaySound(SoundID.Slugcat_Roll_Init, player.mainBodyChunk, loop: false, 1f, 1f);
					player.animation = Player.AnimationIndex.Roll;
					player.rollDirection = 1;
					player.rollCounter = 0;
					player.bodyChunks[0].vel.x = Mathf.Lerp(player.bodyChunks[0].vel.x, 9f, 0.7f);
					player.bodyChunks[1].vel.x = Mathf.Lerp(player.bodyChunks[1].vel.x, 9f, 0.7f);
					player.standing = false;
					startedRoll = true;
				}
				if (timeInPhase > 70)
				{
					ChangePhase(Phase.PlayerUnduck);
				}
			}
			else if (phase == Phase.PlayerUnduck)
			{
				if (timeInPhase == 1 && playerIndex == room.game.Players.Count - 1)
				{
					GhostHunch ghostHunch = new GhostHunch(room, null);
					ghostHunch.goAt = 40;
					room.AddObject(ghostHunch);
				}
				if (timeInPhase > 20)
				{
					ChangePhase(Phase.End);
				}
			}
			else if (phase == Phase.End)
			{
				player.controller = null;
				Destroy();
			}
		}

		public void ChangePhase(Phase phase)
		{
			this.phase = phase;
			timeInPhase = 0;
		}

		public Player.InputPackage GetInput()
		{
			int x = 0;
			int y = 0;
			bool jmp = false;
			if (phase == Phase.PlayerRun)
			{
				x = 1;
				y = ((timeInPhase % 20 < 2) ? 1 : 0);
			}
			else if (phase == Phase.PlayerDuck)
			{
				y = -1;
			}
			else if (phase == Phase.PlayerJump)
			{
				jmp = true;
			}
			else if (phase == Phase.PlayerRoll)
			{
				if (timeInPhase > 15)
				{
					x = 1;
					y = -1;
				}
			}
			else if (phase == Phase.PlayerUnduck)
			{
				y = 1;
			}
			return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, x, y, jmp, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
	}

	public class HI_W05 : UpdatableAndDeletable
	{
		public int waitBeforeDrop;

		public bool showedCamoTutorial;

		public int playerOutsideRoomTime;

		public HI_W05(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!base.room.game.AllPlayersRealized)
			{
				return;
			}
			bool flag = false;
			Room room = null;
			Player player = base.room.game.FirstRealizedPlayer;
			if (ModManager.CoopAvailable)
			{
				player = base.room.game.RealizedPlayerFollowedByCamera;
			}
			for (int i = 0; i < base.room.game.Players.Count; i++)
			{
				if (base.room.game.Players[i].realizedCreature == null)
				{
					continue;
				}
				int num = 205 + 20 * i;
				Player player2 = base.room.game.Players[i].realizedCreature as Player;
				if (player2.room == base.room)
				{
					if (waitBeforeDrop < num)
					{
						player2.SuperHardSetPosition(new Vector2(440f, 840f));
						for (int j = 0; j < player2.bodyChunks.Length; j++)
						{
							player2.bodyChunks[j].vel.y = Mathf.Clamp(player2.bodyChunks[j].vel.y, -5f, 5f);
							player2.bodyChunks[j].vel.x = 0f;
						}
					}
					else if (waitBeforeDrop == num)
					{
						if (i == base.room.game.Players.Count - 1)
						{
							base.room.game.cameras[0].ExitCutsceneMode();
						}
						for (int k = 0; k < player2.bodyChunks.Length; k++)
						{
							player2.bodyChunks[k].vel.y = -35f;
							player2.bodyChunks[k].vel.x = 0f;
						}
						player2.graphicsModule.Reset();
					}
					if (i == 0 && waitBeforeDrop == 1)
					{
						base.room.game.cameras[0].EnterCutsceneMode(player2.abstractCreature, RoomCamera.CameraCutsceneType.HideJollyHudAndArrows);
					}
					flag = true;
				}
				else if (player2 == player)
				{
					room = player2.room;
				}
			}
			if (flag)
			{
				playerOutsideRoomTime = 0;
				if (waitBeforeDrop == 1)
				{
					GhostHunch ghostHunch = new GhostHunch(base.room, null);
					ghostHunch.goAt = 40;
					base.room.AddObject(ghostHunch);
				}
				if (base.room.game.manager.fadeToBlack == 0f)
				{
					waitBeforeDrop++;
				}
			}
			else if (room != null && !showedCamoTutorial && base.room.game.GetStorySession.saveState.deathPersistentSaveData.maximumRippleLevel >= 0.5f)
			{
				playerOutsideRoomTime++;
				if (playerOutsideRoomTime >= 120)
				{
					room.AddObject(new CamoTutorial(room));
					showedCamoTutorial = true;
				}
			}
		}
	}

	public class WORA_AI : UpdatableAndDeletable
	{
		public class EndingController : Player.PlayerController
		{
			public Player player;

			public int playerIndex;

			public WORA_AI owner;

			public EndingController(WORA_AI owner, Player player, int playerIndex)
			{
				this.owner = owner;
				this.player = player;
				this.playerIndex = playerIndex;
			}

			public override Player.InputPackage GetInput()
			{
				return owner.GetInput(player, playerIndex);
			}
		}

		public Prince prince;

		public float baseSongVolume;

		public int counter;

		public FadeOut fadeIn;

		public FadeOut fadeOut;

		public PrinceSpasmer[] spasmers;

		public int spasmTicker;

		public int spasmDuration;

		public OmniDirectionalSound omni;

		public float omniTargetVolume;

		public int triggerIndex;

		public int timerForFinalSpasm;

		public int waitAfterFadeoutTicker;

		public KarmaFlowerPatch activePatch;

		public bool setController;

		public int delayForFinalSpasm = 300;

		public int[] triggerPositions;

		public WORA_AI(Room room)
		{
			base.room = room;
			baseSongVolume = -1f;
			triggerIndex = -1;
			triggerPositions = new int[3] { 2630, 1810, 1110 };
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (prince == null)
			{
				prince = room.physicalObjects[1].OfType<Prince>().FirstOrDefault();
			}
			bool flag = false;
			float? num = null;
			Player player = null;
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				Player player2 = null;
				if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room && !room.game.Players[i].realizedCreature.dead)
				{
					player2 = room.game.Players[i].realizedCreature as Player;
				}
				if (player2 != null)
				{
					if (!num.HasValue || player2.mainBodyChunk.pos.x < num.Value)
					{
						num = player2.mainBodyChunk.pos.x;
						player = player2;
					}
					if (player2.mainBodyChunk.pos.x <= 1110f)
					{
						flag = true;
					}
				}
			}
			if (!room.game.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot && !room.game.GetStorySession.finalWarpSequenceStarted)
			{
				if (num.HasValue && room.game.manager.musicPlayer?.song != null)
				{
					if (baseSongVolume < 0f)
					{
						baseSongVolume = room.game.manager.musicPlayer.song.baseVolume;
					}
					else
					{
						room.game.manager.musicPlayer.song.baseVolume = Mathf.Lerp(0.05f, baseSongVolume, Mathf.InverseLerp(0f, 1000f, num.Value - 1110f));
					}
				}
				if (room.game.GetStorySession.saveState.miscWorldSaveData.SentientRotEndingConditionsMet && player != null)
				{
					if (flag && room.shortCutsReady)
					{
						room.shortcuts[0].shortCutType = ShortcutData.Type.DeadEnd;
						room.game.cameras[0].EnterCutsceneMode(player.abstractCreature, RoomCamera.CameraCutsceneType.HideJollyHud);
					}
					int num2 = 650;
					float num3 = -1f;
					for (int j = 0; j < triggerPositions.Length; j++)
					{
						float num4 = Mathf.Abs((float)triggerPositions[j] - num.Value);
						if (num3 == -1f || num4 < num3)
						{
							num3 = num4;
						}
						if (triggerIndex >= j || !(num.Value <= (float)triggerPositions[j]) || triggerIndex == j)
						{
							continue;
						}
						triggerIndex = j;
						if (activePatch != null)
						{
							activePatch.Destroy();
							room.RemoveObject(activePatch);
							activePatch = null;
						}
						if (j < 2)
						{
							spasmDuration = 400;
							spasmTicker = 0;
							Vector2? vector = null;
							if (j == 0)
							{
								vector = new Vector2(triggerPositions[0], 250f);
							}
							if (j == 1)
							{
								vector = new Vector2(triggerPositions[1], 310f);
							}
							if (vector.HasValue)
							{
								KarmaFlowerPatch.KarmaFlowerPatchData karmaFlowerPatchData = new KarmaFlowerPatch.KarmaFlowerPatchData(null);
								karmaFlowerPatchData.handlePos = Custom.RNV() * 300f;
								PlacedObject placedObject = new PlacedObject(WatcherEnums.PlacedObjectType.KarmaFlowerPatch, karmaFlowerPatchData);
								placedObject.pos = vector.Value;
								karmaFlowerPatchData.owner = placedObject;
								room.roomSettings.placedObjects.Add(placedObject);
								activePatch = new KarmaFlowerPatch();
								activePatch.shrinkFactor = 1f;
								room.AddObject(activePatch);
								activePatch.ShortcutsReady();
							}
						}
					}
					if (triggerIndex == 2)
					{
						if (!setController)
						{
							RainWorld.lockGameTimer = true;
							setController = true;
							for (int k = 0; k < room.game.Players.Count; k++)
							{
								Player player3 = null;
								if (room.game.Players[k].realizedCreature != null && room.game.Players[k].realizedCreature.room == room && !room.game.Players[k].realizedCreature.dead)
								{
									player3 = room.game.Players[k].realizedCreature as Player;
								}
								if (player3 != null)
								{
									player3.controller = new EndingController(this, player3, k);
								}
							}
						}
						timerForFinalSpasm++;
						if (num.Value < (float)num2 && timerForFinalSpasm < delayForFinalSpasm)
						{
							timerForFinalSpasm = delayForFinalSpasm;
						}
						if (timerForFinalSpasm >= delayForFinalSpasm)
						{
							spasmDuration = 400;
							spasmTicker = 0;
						}
						else if (num.Value > (float)triggerPositions[triggerIndex])
						{
							triggerIndex = 1;
							timerForFinalSpasm = 0;
						}
						if (timerForFinalSpasm > delayForFinalSpasm + 200 && fadeOut == null)
						{
							fadeOut = new FadeOut(room, Color.black, 400f, fadeIn: false);
							room.AddObject(fadeOut);
						}
						if (timerForFinalSpasm > delayForFinalSpasm + 100)
						{
							prince.bloom = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 350f, timerForFinalSpasm - delayForFinalSpasm - 100));
						}
					}
					if (fadeOut != null && fadeOut.IsDoneFading())
					{
						waitAfterFadeoutTicker++;
						if (waitAfterFadeoutTicker > 80)
						{
							room.game.manager.statsAfterCredits = false;
							room.game.rainWorld.progression.miscProgressionData.watcherEndingID = 2;
							room.game.cameras[0].ExitCutsceneMode();
							RainWorldGame.BeatGameMode(room.game, standardVoidSea: false);
							room.game.manager.nextSlideshow = WatcherEnums.SlideShowID.EndingRot;
							room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
							Destroy();
						}
					}
					if (spasmers == null && triggerIndex < 2)
					{
						omniTargetVolume = 0.025f + Mathf.InverseLerp(500f, 0f, num3) * 0.275f;
					}
					else if (spasmers == null && triggerIndex == 2 && timerForFinalSpasm > 0 && timerForFinalSpasm < delayForFinalSpasm)
					{
						omniTargetVolume = Mathf.Lerp(0.025f, 0.65f, (float)timerForFinalSpasm / (float)delayForFinalSpasm);
					}
					else
					{
						omniTargetVolume = 0f;
					}
				}
			}
			bool flag2 = true;
			for (int l = 0; l < room.game.Players.Count; l++)
			{
				Player player4 = null;
				if (room.game.Players[l].realizedCreature != null && room.game.Players[l].realizedCreature.room == room && !room.game.Players[l].realizedCreature.dead)
				{
					player4 = room.game.Players[l].realizedCreature as Player;
				}
				if (player4 == null)
				{
					continue;
				}
				if (!room.game.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot && !room.game.GetStorySession.finalWarpSequenceStarted && room.game.GetStorySession.saveState.miscWorldSaveData.SentientRotEndingConditionsMet)
				{
					if (spasmDuration > 0 && spasmTicker < spasmDuration)
					{
						if (flag2)
						{
							spasmTicker++;
						}
						if (spasmers == null && triggerIndex < 2)
						{
							spasmers = new PrinceSpasmer[room.game.Players.Count];
						}
						if (spasmers != null && spasmers[l] == null && triggerIndex < 2)
						{
							spasmers[l] = new PrinceSpasmer(player4, useLeftBound: true, triggerPositions[triggerIndex] - 150, useRightBound: true, triggerPositions[triggerIndex] + 150);
						}
						if (spasmers != null && spasmers[l] != null)
						{
							spasmers[l].Update(eu);
						}
						float b = 0.35f;
						if (triggerIndex == 2)
						{
							b = Mathf.Lerp(0.35f, 0.7f, Mathf.InverseLerp(delayForFinalSpasm, delayForFinalSpasm + 200, timerForFinalSpasm));
						}
						player4.warpFatigueEffect = Mathf.Lerp(player4.warpFatigueEffect, b, 0.08f);
						if (flag2)
						{
							omniTargetVolume = 0.65f;
							if (fadeOut != null)
							{
								omniTargetVolume = Mathf.Lerp(0.65f, 0f, fadeOut.fade);
							}
						}
						if (activePatch != null && flag2)
						{
							if ((float)spasmTicker < (float)spasmDuration * 1f / 3f)
							{
								activePatch.shrinkFactor = Mathf.Lerp(1f, 0f, (float)spasmTicker / ((float)spasmDuration * 1f / 3f));
							}
							else if ((float)spasmTicker < (float)spasmDuration * 2f / 3f)
							{
								activePatch.shrinkFactor = 0f;
							}
							else
							{
								activePatch.shrinkFactor = Mathf.Lerp(0f, 1f, ((float)spasmTicker - (float)spasmDuration * 2f / 3f) / ((float)spasmDuration * 1f / 3f));
							}
						}
					}
					else
					{
						if (spasmers != null)
						{
							for (int m = 0; m < spasmers.Length; m++)
							{
								if (spasmers[m] != null)
								{
									spasmers[m].Destroy();
								}
							}
							spasmers = null;
						}
						if (activePatch != null)
						{
							activePatch.Destroy();
							room.RemoveObject(activePatch);
							activePatch = null;
						}
						spasmDuration = 0;
						player4.warpFatigueEffect = Mathf.Lerp(player4.warpFatigueEffect, 0f, 0.16f);
						if (player4.warpFatigueEffect < 0.02f)
						{
							player4.warpFatigueEffect = 0f;
						}
					}
				}
				if (room.game.GetStorySession.saveState.denPosition.ToLowerInvariant() == "wora_ai" && player4 != null)
				{
					if (player4.sceneFlag)
					{
						Destroy();
						return;
					}
					player4.sleepCurlUp = 1f;
					player4.sleepCounter = 99;
					if (counter < 40)
					{
						player4.SuperHardSetPosition(new Vector2(1430f + (float)l * 40f, 330f));
					}
					if (flag2)
					{
						counter++;
						if (fadeIn == null)
						{
							fadeIn = new FadeOut(room, Color.black, 80f, fadeIn: true);
							room.AddObject(fadeIn);
						}
						if (fadeIn != null)
						{
							if (counter < 80)
							{
								fadeIn.fade = 1f;
							}
							if (fadeIn.IsDoneFading())
							{
								SetSceneFlagForAllPlayers(room.game);
								Destroy();
								return;
							}
						}
					}
				}
				flag2 = false;
			}
			if (room.game.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot || room.game.GetStorySession.finalWarpSequenceStarted || player == null || !room.game.GetStorySession.saveState.miscWorldSaveData.SentientRotEndingConditionsMet)
			{
				return;
			}
			if (omni == null)
			{
				for (int n = 0; n < room.roomSettings.ambientSounds.Count; n++)
				{
					if (room.roomSettings.ambientSounds[n].type == AmbientSound.Type.Omnidirectional && room.roomSettings.ambientSounds[n].sample.ToLowerInvariant() == "rwtw_sigil.ogg")
					{
						omni = room.roomSettings.ambientSounds[n] as OmniDirectionalSound;
						omni.volume = 0f;
						break;
					}
				}
				if (omni == null)
				{
					omni = new OmniDirectionalSound("RWTW_Sigil.ogg", inherited: false);
					omni.volume = 0f;
					room.roomSettings.ambientSounds.Add(omni);
					room.game.cameras[0].virtualMicrophone.NewRoom(room.game.cameras[0].room);
				}
			}
			if (omni != null)
			{
				omni.volume = Mathf.Lerp(omni.volume, omniTargetVolume, 0.08f);
			}
		}

		public Player.InputPackage GetInput(Player player, int playerIndex)
		{
			return new Player.InputPackage(gamePad: true, Options.ControlSetup.Preset.None, (player.firstChunk.pos.x > 880f + (float)(20 * playerIndex)) ? (-1) : 0, ((!player.standing || player.bodyMode == Player.BodyModeIndex.Crawl) && UnityEngine.Random.value < 0.5f) ? 1 : 0, ((!player.standing || player.bodyMode == Player.BodyModeIndex.Crawl) && UnityEngine.Random.value < 0.5f) ? true : false, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
	}

	public class WORA_KarmaSigils : UpdatableAndDeletable, Conversation.IOwnAConversation
	{
		public class KarmaSigilConversation : Conversation
		{
			public KarmaSigilConversation(ID id, WORA_KarmaSigils owner, DialogBox dialogBox)
				: base(owner, id, dialogBox)
			{
				currentSaveFile = owner.room.game.StoryCharacter;
				AddEvents();
			}

			public override void AddEvents()
			{
				LoadEventsFromFile(PrinceBehavior.PrinceConversation.PrinceConversationToId(id));
			}
		}

		public int counter;

		public FadeOut fadeIn;

		public KarmaSigilConversation currentConversation;

		public PrinceSpasmer[] spasmers;

		public OmniDirectionalSound omni;

		public float omniTargetVolume;

		public bool updatedTrigger;

		public bool unlockedHallConnection;

		public DialogBox dialogBox
		{
			get
			{
				if (currentConversation != null)
				{
					return currentConversation.dialogBox;
				}
				if (room.game.cameras[0].hud.dialogBox == null)
				{
					room.game.cameras[0].hud.InitDialogBox();
				}
				return room.game.cameras[0].hud.dialogBox;
			}
		}

		public WORA_KarmaSigils(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.GetStorySession.finalWarpSequenceStarted)
			{
				return;
			}
			float? num = null;
			float? num2 = null;
			Player player = null;
			Player player2 = null;
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				Player player3 = null;
				if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room && !room.game.Players[i].realizedCreature.dead)
				{
					player3 = room.game.Players[i].realizedCreature as Player;
				}
				if (player3 != null)
				{
					if (!num.HasValue || player3.mainBodyChunk.pos.x < num.Value)
					{
						num = player3.mainBodyChunk.pos.x;
						player = player3;
					}
					if (!num2.HasValue || player3.mainBodyChunk.pos.y > num2.Value)
					{
						num2 = player3.mainBodyChunk.pos.y;
						player2 = player3;
					}
				}
			}
			bool flag = num.HasValue || num2.HasValue;
			bool flag2 = room.abstractRoom.name.ToLowerInvariant() == "wora_ai";
			bool flag3 = room.abstractRoom.name.ToLowerInvariant() == "wora_throne02";
			if (flag && !room.game.GetStorySession.saveState.miscWorldSaveData.SentientRotEndingConditionsMet && (room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters > 2 || (room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters == 0 && flag2) || (room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters == 1 && room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen >= PrinceBehavior.PrinceConversation.PrinceConversationToId(WatcherEnums.ConversationID.Prince_KS_1_4)) || (room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters == 2 && room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen >= PrinceBehavior.PrinceConversation.PrinceConversationToId(WatcherEnums.ConversationID.Prince_KS_2_4))))
			{
				room.game.GetStorySession.saveState.miscWorldSaveData.lockedOuterRimProgressionFlag = true;
			}
			if (room.shortCutsReady && flag3)
			{
				int num3 = -1;
				if (room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters == 0)
				{
					num3 = 4;
				}
				else if (room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters == 1)
				{
					num3 = 3;
				}
				else if (room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters == 2)
				{
					num3 = 2;
				}
				else if (room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters == 3)
				{
					num3 = 1;
				}
				if (num3 >= 0)
				{
					for (int j = 1; j <= num3; j++)
					{
						if (j < num3)
						{
							room.shortcuts[j].shortCutType = ShortcutData.Type.DeadEnd;
						}
						else if (!room.game.GetStorySession.saveState.miscWorldSaveData.lockedOuterRimProgressionFlag)
						{
							room.shortcuts[j].shortCutType = ShortcutData.Type.DeadEnd;
						}
						else
						{
							if (unlockedHallConnection)
							{
								continue;
							}
							unlockedHallConnection = true;
							room.shortcuts[j].shortCutType = ShortcutData.Type.RoomExit;
							AbstractRoom abstractRoom = room.world.GetAbstractRoom(room.abstractRoom.connections[j]);
							if (!room.world.DisabledMapRooms.Contains(abstractRoom.name))
							{
								continue;
							}
							room.world.DisabledMapRooms.Remove(abstractRoom.name);
							RoomCamera[] cameras = room.game.cameras;
							for (int k = 0; k < cameras.Length; k++)
							{
								global::HUD.HUD hud = cameras[k].hud;
								if (hud != null && hud.map?.RegionName == room.world.name)
								{
									hud.ResetMap(new Map.MapData(room.world, room.game.rainWorld));
								}
							}
						}
					}
				}
			}
			float num4 = room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters;
			if (room.game.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot || room.game.GetStorySession.saveState.miscWorldSaveData.SentientRotEndingConditionsMet)
			{
				num4 = 0f;
			}
			if (!updatedTrigger)
			{
				for (int num5 = room.roomSettings.triggers.Count - 1; num5 >= 0; num5--)
				{
					if (room.roomSettings.triggers[num5].tEvent is MusicEvent)
					{
						ActiveTriggerChecker activeTriggerChecker = null;
						for (int l = 0; l < room.updateList.Count; l++)
						{
							if (room.updateList[l] is ActiveTriggerChecker && (room.updateList[l] as ActiveTriggerChecker).eventTrigger == room.roomSettings.triggers[num5])
							{
								activeTriggerChecker = room.updateList[l] as ActiveTriggerChecker;
							}
						}
						if (num4 == 2f)
						{
							(room.roomSettings.triggers[num5].tEvent as MusicEvent).songName = "RW_122 - Speaking Systems 2";
						}
						else if (num4 >= 3f)
						{
							(room.roomSettings.triggers[num5].tEvent as MusicEvent).songName = "RW_123 - Speaking Systems 3";
						}
						else
						{
							room.roomSettings.triggers.RemoveAt(num5);
							if (activeTriggerChecker != null)
							{
								room.RemoveObject(activeTriggerChecker);
							}
						}
						updatedTrigger = true;
					}
				}
			}
			if (omni == null && flag)
			{
				for (int m = 0; m < room.roomSettings.ambientSounds.Count; m++)
				{
					if (room.roomSettings.ambientSounds[m].type == AmbientSound.Type.Omnidirectional && room.roomSettings.ambientSounds[m].sample.ToLowerInvariant() == "rwtw_sigil.ogg")
					{
						omni = room.roomSettings.ambientSounds[m] as OmniDirectionalSound;
						omni.volume = 0f;
						break;
					}
				}
				if (omni == null)
				{
					omni = new OmniDirectionalSound("RWTW_Sigil.ogg", inherited: false);
					omni.volume = 0f;
					room.roomSettings.ambientSounds.Add(omni);
				}
			}
			if (flag)
			{
				Conversation.ID[] array = new Conversation.ID[0];
				Vector2[] array2 = new Vector2[0];
				if (!room.game.GetStorySession.saveState.miscWorldSaveData.lockedOuterRimProgressionFlag)
				{
					if (flag2)
					{
						if (num4 == 1f)
						{
							array = new Conversation.ID[2]
							{
								WatcherEnums.ConversationID.Prince_KS_1_3,
								WatcherEnums.ConversationID.Prince_KS_1_4
							};
						}
						else if (num4 == 2f)
						{
							array = new Conversation.ID[2]
							{
								WatcherEnums.ConversationID.Prince_KS_2_3,
								WatcherEnums.ConversationID.Prince_KS_2_4
							};
						}
						array2 = new Vector2[2]
						{
							new Vector2(2630f, -1f),
							new Vector2(1810f, -1f)
						};
					}
					else if (flag3)
					{
						if (num4 == 1f)
						{
							array = new Conversation.ID[2]
							{
								WatcherEnums.ConversationID.Prince_KS_1_1,
								WatcherEnums.ConversationID.Prince_KS_1_2
							};
						}
						else if (num4 == 2f)
						{
							array = new Conversation.ID[2]
							{
								WatcherEnums.ConversationID.Prince_KS_2_1,
								WatcherEnums.ConversationID.Prince_KS_2_2
							};
						}
						array2 = new Vector2[4]
						{
							new Vector2(680f, 940f),
							new Vector2(415f, 2980f),
							new Vector2(543f, 2980f),
							new Vector2(790f, 2980f)
						};
					}
				}
				float num6 = -1f;
				float num7 = -1f;
				int num8 = -1;
				for (int n = 0; n < array2.Length; n++)
				{
					num7 = ((!(array2[n].y < 0f)) ? Vector2.Distance(player2.mainBodyChunk.pos, array2[n]) : Mathf.Abs(player.mainBodyChunk.pos.x - array2[n].x));
					if (num6 == -1f || num7 < num6)
					{
						num6 = num7;
						if (array2[n].y < 0f)
						{
							num8 = (int)array2[n].x;
						}
					}
				}
				if (array.Length != 0)
				{
					int num9 = -1;
					Conversation.ID id = null;
					if (flag2)
					{
						for (int num10 = 0; num10 < array.Length; num10++)
						{
							if (player.mainBodyChunk.pos.x <= array2[num10].x)
							{
								int num11 = PrinceBehavior.PrinceConversation.PrinceConversationToId(array[num10]);
								if (room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen < num11)
								{
									num9 = num11;
									id = array[num10];
									break;
								}
							}
						}
					}
					else if (flag3)
					{
						for (int num12 = 0; num12 < array.Length; num12++)
						{
							if (!(player2.mainBodyChunk.pos.y >= array2[num12].y))
							{
								continue;
							}
							bool flag4 = false;
							if (num12 == 0 && player2.mainBodyChunk.pos.x >= array2[num12].x && player2.mainBodyChunk.pos.x <= 940f)
							{
								flag4 = true;
							}
							else if (num12 == 1 && (player2.mainBodyChunk.pos.x <= 415f || (player2.mainBodyChunk.pos.x >= 515f && player2.mainBodyChunk.pos.x <= 570f) || (player2.mainBodyChunk.pos.x >= 750f && player2.mainBodyChunk.pos.x <= 830f)))
							{
								flag4 = true;
							}
							if (flag4)
							{
								int num13 = PrinceBehavior.PrinceConversation.PrinceConversationToId(array[num12]);
								if (room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen < num13)
								{
									num9 = num13;
									id = array[num12];
									break;
								}
							}
						}
					}
					if (num9 >= 0 && currentConversation == null && room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen < num9)
					{
						currentConversation = new KarmaSigilConversation(id, this, dialogBox);
						room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen = num9;
						if (flag2 && player != null)
						{
							room.game.cameras[0].EnterCutsceneMode(player.abstractCreature, RoomCamera.CameraCutsceneType.Standard);
						}
						else if (flag3 && player2 != null)
						{
							room.game.cameras[0].EnterCutsceneMode(player2.abstractCreature, RoomCamera.CameraCutsceneType.Standard);
						}
						for (int num14 = 0; num14 < room.game.Players.Count; num14++)
						{
							if (room.game.Players[num14].realizedCreature != null && room.game.Players[num14].realizedCreature.room == room && !room.game.Players[num14].realizedCreature.dead)
							{
								room.game.Players[num14].realizedCreature.Stun(80);
							}
						}
					}
				}
				if (num4 == 1f || num4 == 2f)
				{
					omniTargetVolume = 0.025f + Mathf.InverseLerp(500f, 0f, num6) * 0.275f;
				}
				else
				{
					omniTargetVolume = 0f;
				}
				if (room.game.GetStorySession.saveState.miscWorldSaveData.lockedOuterRimProgressionFlag)
				{
					omniTargetVolume = 0f;
				}
				for (int num15 = 0; num15 < room.game.Players.Count; num15++)
				{
					Player player4 = null;
					if (room.game.Players[num15].realizedCreature != null && room.game.Players[num15].realizedCreature.room == room && !room.game.Players[num15].realizedCreature.dead)
					{
						player4 = room.game.Players[num15].realizedCreature as Player;
					}
					if (player4 == null)
					{
						continue;
					}
					if (currentConversation != null)
					{
						if (spasmers == null && num8 >= 0)
						{
							spasmers = new PrinceSpasmer[room.game.Players.Count];
						}
						if (spasmers != null && spasmers[num15] == null && num8 >= 0)
						{
							spasmers[num15] = new PrinceSpasmer(player4, useLeftBound: true, num8 - 150, useRightBound: true, num8 + 150);
						}
						if (spasmers != null && spasmers[num15] != null)
						{
							spasmers[num15].Update(eu);
						}
						player4.warpFatigueEffect = Mathf.Lerp(player4.warpFatigueEffect, 0.35f, 0.08f);
						omniTargetVolume = 0.65f;
						continue;
					}
					if (spasmers != null)
					{
						for (int num16 = 0; num16 < spasmers.Length; num16++)
						{
							if (spasmers[num16] != null)
							{
								spasmers[num16].Destroy();
							}
						}
						spasmers = null;
						room.game.cameras[0].ExitCutsceneMode();
					}
					player4.warpFatigueEffect = Mathf.Lerp(player4.warpFatigueEffect, 0f, 0.16f);
					if (player4.warpFatigueEffect < 0.02f)
					{
						player4.warpFatigueEffect = 0f;
					}
				}
			}
			if (omni != null)
			{
				omni.volume = Mathf.Lerp(omni.volume, omniTargetVolume, 0.08f);
			}
			if (currentConversation != null)
			{
				currentConversation.Update();
				if (currentConversation.slatedForDeletion)
				{
					currentConversation = null;
				}
			}
		}

		public string ReplaceParts(string s)
		{
			return s;
		}

		public void SpecialEvent(string eventName)
		{
		}
	}

	public class WORA_DESERT6 : UpdatableAndDeletable
	{
		public WORA_DESERT6(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			if (room.game.GetStorySession.saveState.miscWorldSaveData.lockedOuterRimProgressionFlag || room.world.game.GetStorySession.saveState.deathPersistentSaveData.maximumRippleLevel < 2f)
			{
				PlacedObject placedObject = new PlacedObject(PlacedObject.Type.WarpPoint, null);
				placedObject.pos = new Vector2(476.5552f, 374.4548f);
				(placedObject.data as WarpPoint.WarpPointData).oneWay = true;
				WarpPoint warpPoint = room.TrySpawnWarpPoint(placedObject, saveInRegionState: false);
				if (warpPoint != null)
				{
					warpPoint.dontSave = true;
				}
			}
			Destroy();
		}
	}

	public class WORA_ElderSpawn : UpdatableAndDeletable
	{
		public class ElderBehavior : VoidSpawn.OrbitPoint
		{
			public bool closeIn;

			public float closeInStart;

			public float closeInEnd = -315f;

			public float collapseAnimStart => closeInEnd - 360f;

			public float collapseAnimFinish => closeInEnd - 740f;

			public void StartCloseIn()
			{
				closeIn = true;
				closeInStart = orbitAngle - 20f;
				closeInEnd = closeInStart - 360f;
			}

			public ElderBehavior(VoidSpawn owner)
				: base(owner, new Vector2(470f, 1270f), 300f, clockwise: false)
			{
				orbitAngle = 0f;
			}

			public override Vector2 GetPointOnOrbit(float angle)
			{
				owner.canBeDestroyed = false;
				if (closeIn && orbitAngle < closeInStart)
				{
					float num = Mathf.Lerp(closeInStart, closeInEnd, 0.2f);
					if (orbitAngle > num)
					{
						orbitDistance = Custom.LerpMap(orbitAngle, closeInStart, num, 300f, 400f, 0.7f);
					}
					else
					{
						orbitDistance = Custom.LerpMap(orbitAngle, num, closeInEnd, 400f, 40f, 1.2f);
					}
				}
				return base.GetPointOnOrbit(angle);
			}
		}

		public VoidSpawn elder;

		public RipplesSpawnAbsorption absorption;

		public List<VoidSpawn> ambientSpawn = new List<VoidSpawn>();

		public static Vector2 orbitCenter = new Vector2(470f, 1270f);

		public int ambientCooldown;

		public int maxSpawn = 5;

		public int viewedCamTime;

		public int notViewedCamTime;

		public bool elderSpotted;

		public int playerLevitationTime;

		public bool holdTutorial = true;

		public string TranslatedTutorialText => room.game.manager.rainWorld.inGameTranslator.Translate("Hold DOWN and SPECIAL together to create a path to a chosen destination");

		public WORA_ElderSpawn(Room room)
		{
			base.room = room;
			maxSpawn = (int)Mathf.Lerp(1f, 5f, room.world.regionState.rippleSpawnEggState.percentEggsCollected);
		}

		public override void Update(bool eu)
		{
			if (ambientCooldown <= 0)
			{
				SpawnAmbient();
				ambientCooldown = ambientSpawn.Count * 2;
			}
			else if (ambientSpawn.Count < maxSpawn)
			{
				ambientCooldown--;
			}
			if (room.game.devToolsActive)
			{
				if (Input.GetKey(KeyCode.Backspace))
				{
					foreach (VoidSpawn item in ambientSpawn)
					{
						item.Destroy();
					}
					elder?.Destroy();
				}
				else if (Input.GetKey(KeyCode.N) && elder == null)
				{
					SpawnElder();
					elderSpotted = true;
					maxSpawn = 100;
					ambientCooldown = 60;
				}
			}
			for (int num = ambientSpawn.Count - 1; num >= 0; num--)
			{
				if (ambientSpawn[num].slatedForDeletetion)
				{
					ambientSpawn.RemoveAt(num);
				}
			}
			if (elder != null && elder.slatedForDeletetion)
			{
				elder = null;
			}
			if (!elderSpotted)
			{
				if (!room.world.game.GetStorySession.saveState.miscWorldSaveData.hasRippleEggWarpAbility && room.world.regionState.rippleSpawnEggState.OuterRimDialFilled())
				{
					ElderTimerUpdate();
				}
				return;
			}
			if (holdTutorial)
			{
				TextPrompt textPrompt = room.game.cameras[0].hud.textPrompt;
				if (textPrompt.messages.Count > 0 && textPrompt.messages[0].text == TranslatedTutorialText)
				{
					textPrompt.messages[0].time = 20;
				}
				foreach (Player item2 in room.PlayersInRoom)
				{
					if (item2.watcherNonDynamicWarpInput)
					{
						holdTutorial = false;
						break;
					}
				}
			}
			if (room.warpPoints.Count > 0)
			{
				return;
			}
			foreach (Player item3 in room.PlayersInRoom)
			{
				if (elder != null && item3.graphicsModule is PlayerGraphics playerGraphics)
				{
					playerGraphics.LookAtObject(elder);
				}
				if (!(item3.SlugCatClass == WatcherEnums.SlugcatStatsName.Watcher) || !(Vector2.Distance(item3.mainBodyChunk.pos, orbitCenter) < 200f))
				{
					continue;
				}
				if (Vector2.Distance(item3.mainBodyChunk.pos, orbitCenter) < 200f)
				{
					bool flag = item3.bodyMode != Player.BodyModeIndex.ClimbingOnBeam && item3.bodyMode != Player.BodyModeIndex.WallClimb && item3.bodyMode != Player.BodyModeIndex.Stand;
					if (playerLevitationTime < (flag ? int.MaxValue : 60))
					{
						playerLevitationTime++;
					}
					if (playerLevitationTime > 120 && elder?.behavior is ElderBehavior elderBehavior)
					{
						if (!elderBehavior.closeIn)
						{
							elderBehavior.StartCloseIn();
							absorption.SlowStart();
						}
						else if (elderBehavior.orbitAngle < elderBehavior.collapseAnimFinish && !item3.specialRippleEggWarpPermission)
						{
							PromptSpecialWarp(item3);
						}
						item3.WatcherGainInstability(0.006f, (elderBehavior.closeIn && elderBehavior.orbitAngle < elderBehavior.collapseAnimStart - 20f) ? 0.3f : 1f);
						if (elderBehavior.closeIn && elderBehavior.orbitAngle < elderBehavior.collapseAnimStart - 20f)
						{
							absorption.Start();
							item3.watcherInstability = 0.3f;
							if (elderBehavior.orbitAngle > elderBehavior.collapseAnimStart - 100f)
							{
								item3.Blink(10);
							}
							else if (elderBehavior.orbitAngle > elderBehavior.collapseAnimFinish - 80f)
							{
								item3.Stun(10);
							}
							else if (elderBehavior.orbitAngle > elderBehavior.collapseAnimFinish - 400f)
							{
								int num2 = (int)Custom.LerpMap(elderBehavior.orbitAngle, elderBehavior.collapseAnimFinish - 80f, elderBehavior.collapseAnimFinish - 400f, 8f, 30f);
								if (Mathf.Abs(elderBehavior.orbitAngle) % (float)num2 == 0f)
								{
									item3.Blink(5);
								}
								absorption.End();
								item3.watcherInstability = 0f;
							}
						}
					}
					if (flag)
					{
						item3.levitationActive = true;
						item3.TickLevitation(levitateUp: false, playerLevitationTime, 5f);
						item3.levitationActive = false;
						Vector2 vector = orbitCenter + new Vector2(0f, Mathf.Abs(Mathf.Sin((float)playerLevitationTime / 40f) * 20f) - 10f);
						Vector2 a = Custom.DirVec(item3.mainBodyChunk.pos, vector);
						a = Vector2.Lerp(a, Vector2.up, Mathf.InverseLerp(30f, 10f, Vector2.Distance(item3.mainBodyChunk.pos, vector)));
						float num3 = Custom.LerpMap(playerLevitationTime, 0f, 120f, 0.5f, 1.5f);
						for (int i = 0; i < item3.bodyChunks.Length; i++)
						{
							item3.bodyChunks[i].vel *= 0.3f;
							item3.bodyChunks[i].vel += a * (Mathf.Abs(Mathf.Sin((float)playerLevitationTime / 40f)) * num3);
						}
					}
				}
				else
				{
					playerLevitationTime--;
				}
				item3.watcherInstability = Mathf.Max(0f, item3.watcherInstability - 0.001f);
				item3.customPlayerGravity = Custom.LerpMap(playerLevitationTime, 0f, 120f, 0.7f, 0.1f, 0.5f);
			}
		}

		public void ElderTimerUpdate()
		{
			if (room.ViewedByAnyCamera(orbitCenter, 0f))
			{
				notViewedCamTime = 0;
				viewedCamTime++;
			}
			else
			{
				notViewedCamTime++;
			}
			if (elder == null && (float)viewedCamTime > 100f)
			{
				SpawnElder();
			}
			if (elder != null && room.ViewedByAnyCamera(elder.firstChunk.pos, 50f))
			{
				elderSpotted = true;
				maxSpawn = 100;
				ambientCooldown = 60;
			}
			else if ((float)notViewedCamTime > 40f)
			{
				viewedCamTime = 0;
				elder?.Destroy();
				elder = null;
				elderSpotted = false;
			}
		}

		public void SpawnElder()
		{
			if (elder != null)
			{
				elder.Destroy();
			}
			if (absorption == null)
			{
				absorption = new RipplesSpawnAbsorption();
			}
			room.AddObject(absorption);
			absorption.pos = orbitCenter;
			Vector2 pos = new Vector2(1150f, 1400f);
			VoidSpawn voidSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, WatcherEnums.AbstractObjectType.RippleSpawn, null, room.GetWorldCoordinate(pos), room.world.game.GetNewID()), 0f, dayLightMode: false, VoidSpawn.SpawnType.ElderRipple);
			voidSpawn.canBeDestroyed = false;
			voidSpawn.behavior = new ElderBehavior(voidSpawn);
			voidSpawn.PlaceInRoom(room);
			elder = voidSpawn;
		}

		public void SpawnAmbient()
		{
			float value = UnityEngine.Random.value;
			value = ((!(value < 0.5f)) ? (1f - Mathf.Pow((1f - value) * 2f, 0.4f) / 2f) : (Mathf.Pow(value * 2f, 0.4f) / 2f));
			value = Mathf.Lerp(70f, 500f, Mathf.Pow(value, 2f));
			float num = UnityEngine.Random.Range(0f, 360f);
			Vector2 pos = orbitCenter + Custom.DegToVec(num) * (value + 100f);
			VoidSpawn voidSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, WatcherEnums.AbstractObjectType.RippleSpawn, null, room.GetWorldCoordinate(pos), room.world.game.GetNewID()), 0f, dayLightMode: false, VoidSpawn.SpawnType.RippleSpawn);
			voidSpawn.firstChunk.vel = Custom.DegToVec(num + 90f) * voidSpawn.swimSpeed;
			voidSpawn.behavior = new VoidSpawn.OrbitPoint(voidSpawn, orbitCenter, value, clockwise: false);
			voidSpawn.timeUntilFadeout = (int)Custom.LerpMap(voidSpawn.sizeFac, 0.5f, 1.2f, 320f, 1200f, 0.5f);
			voidSpawn.PlaceInRoom(room);
			ambientSpawn.Add(voidSpawn);
		}

		public void PromptSpecialWarp(Player player)
		{
			player.specialRippleEggWarpPermission = true;
			_ = room.game.manager.rainWorld.inGameTranslator;
			room.game.cameras[0].hud.textPrompt.AddMessage(TranslatedTutorialText, 120, 160, darken: false, hideHud: true);
			holdTutorial = true;
			elder.startFadeOut = true;
			MiscWorldSaveData miscWorldSaveData = room.world.game.GetStorySession.saveState.miscWorldSaveData;
			miscWorldSaveData.hasRippleEggWarpAbility = true;
			miscWorldSaveData.rippleEggsCollected = 0;
			miscWorldSaveData.rippleEggsToRespawn.Clear();
		}
	}

	public class WORA_RippleDial : UpdatableAndDeletable
	{
		public class RippleDial : CosmeticSprite
		{
			public Spiral spiral = new Spiral(35f, 28.4f);

			public float eggArcLength;

			public float eggMargin = 1f;

			public float eggWidthLit = 18f;

			public float eggWidthUnlit = 1.5f;

			public float rotation = -75f;

			public int totalEggs;

			public int currentEggsLit => room.world.regionState.rippleSpawnEggState.eggsCollected;

			public RippleDial(Room room, Vector2 pos, int max = 25)
			{
				base.room = room;
				base.pos = pos;
				lastPos = pos;
				totalEggs = max;
				float num = (float)Mathf.Clamp(totalEggs / 5, 2, 5) + 0.08f;
				eggArcLength = (spiral.AngleToLength(num * 2f * (float)Math.PI) - eggMargin * (float)(totalEggs - 1)) / (float)totalEggs;
			}

			public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
			{
				sLeaser.sprites = new FSprite[totalEggs];
				int segments = Mathf.Max(Mathf.RoundToInt(eggArcLength / 12f), 1);
				for (int i = 0; i < totalEggs; i++)
				{
					sLeaser.sprites[i] = TriangleMesh.MakeLongMesh(segments, pointyTip: false, customColor: false);
					sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["RippleDialLevel"];
					sLeaser.sprites[i].color = Color.Lerp(RainWorld.RippleColor, Color.black, 0.3f);
					sLeaser.sprites[i].alpha = 19f / 30f;
				}
				AddToContainer(sLeaser, rCam, null);
			}

			public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
			{
				if (newContatiner == null)
				{
					newContatiner = rCam.ReturnFContainer("GrabShaders");
				}
				FSprite[] sprites = sLeaser.sprites;
				for (int i = 0; i < sprites.Length; i++)
				{
					sprites[i].RemoveFromContainer();
				}
				for (int j = 0; j < totalEggs; j++)
				{
					newContatiner.AddChild(sLeaser.sprites[j]);
				}
			}

			public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
			{
				for (int i = 0; i < totalEggs; i++)
				{
					sLeaser.sprites[i].color = Color.Lerp(RainWorld.RippleColor, Color.black, palette.darkness * 0.7f);
				}
			}

			public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
			{
				base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
				Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
				for (int i = 0; i < totalEggs; i++)
				{
					TriangleMesh triangleMesh = sLeaser.sprites[i] as TriangleMesh;
					triangleMesh.isVisible = i < currentEggsLit;
					if (triangleMesh.isVisible)
					{
						float num = (eggArcLength + eggMargin) * (float)i;
						for (int j = 0; j < triangleMesh.vertices.Length; j += 2)
						{
							float num2 = (float)j / (float)(triangleMesh.vertices.Length - 2);
							float num3 = eggWidthLit / 2f / eggArcLength;
							num2 = ((!(num2 < 0.2f)) ? ((!(num2 > 0.8f)) ? Custom.LerpMap(num2, 0.2f, 0.8f, num3, 1f - num3) : Custom.LerpMap(num2, 0.8f, 1f, 1f - num3, 1f)) : Custom.LerpMap(num2, 0f, 0.2f, 0f, num3));
							float num4 = eggWidthLit / 2f;
							float num5 = Mathf.Min(num2, 1f - num2) * eggArcLength;
							num4 *= Mathf.Sqrt(1f - Mathf.Pow(Mathf.Clamp01(1f - num5 / num4), 2f));
							Vector2 b = Custom.RotateAroundOrigo(spiral.LengthToPos(num + num2 * eggArcLength), rotation);
							b = Vector2.Lerp(b.normalized * spiral.innerRadius, b, 1f);
							Vector2 vector2 = b.normalized * num4;
							Vector2 vector3 = b + vector - camPos;
							triangleMesh.MoveVertice(j, vector3 + vector2);
							triangleMesh.MoveVertice(j + 1, vector3 - vector2);
						}
					}
				}
			}
		}

		public List<VoidSpawn> ambientSpawn = new List<VoidSpawn>();

		public static Vector2 orbitCenter = new Vector2(534f, 337f);

		public int ambientCooldown = 1;

		public int maxSpawn => room.world.regionState.rippleSpawnEggState.eggsCollected;

		public WORA_RippleDial(Room room)
		{
			base.room = room;
			room.AddObject(new RippleDial(room, orbitCenter));
			Populate();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (ambientCooldown <= 0)
			{
				SpawnAmbient(init: false);
				ambientCooldown = 800 / maxSpawn;
			}
			else if (ambientSpawn.Count < maxSpawn)
			{
				ambientCooldown--;
			}
			for (int num = ambientSpawn.Count - 1; num >= 0; num--)
			{
				if (ambientSpawn[num].slatedForDeletetion)
				{
					ambientSpawn.RemoveAt(num);
				}
			}
		}

		public VoidSpawn SpawnAmbient(bool init)
		{
			float value = UnityEngine.Random.value;
			value = ((!(value < 0.5f)) ? (1f - Mathf.Pow((1f - value) * 2f, 0.4f) / 2f) : (Mathf.Pow(value * 2f, 0.4f) / 2f));
			value = Mathf.Lerp(130f, 350f, Mathf.Pow(value, 2f));
			float num = UnityEngine.Random.Range(0f, 360f);
			Vector2 pos = orbitCenter + Custom.DegToVec(num) * (value + (init ? 0f : 100f));
			VoidSpawn voidSpawn = new VoidSpawn(new AbstractPhysicalObject(room.world, WatcherEnums.AbstractObjectType.RippleSpawn, null, room.GetWorldCoordinate(pos), room.world.game.GetNewID()), 0f, dayLightMode: false, VoidSpawn.SpawnType.RippleSpawn);
			voidSpawn.firstChunk.vel = Custom.DegToVec(num + 90f) * voidSpawn.swimSpeed;
			voidSpawn.behavior = new VoidSpawn.OrbitPoint(voidSpawn, orbitCenter, value, clockwise: false);
			voidSpawn.timeUntilFadeout = (int)Custom.LerpMap(voidSpawn.sizeFac, 0.5f, 1.2f, 320f, 1200f, 0.5f);
			if (init)
			{
				voidSpawn.timeUntilFadeout = (int)((float)voidSpawn.timeUntilFadeout * UnityEngine.Random.value);
			}
			voidSpawn.PlaceInRoom(room);
			ambientSpawn.Add(voidSpawn);
			return voidSpawn;
		}

		public void Populate()
		{
			for (int i = 0; i < UnityEngine.Random.Range(maxSpawn / 2, maxSpawn); i++)
			{
				VoidSpawn voidSpawn = SpawnAmbient(init: true);
				voidSpawn.timeUntilFadeout = (int)((float)voidSpawn.timeUntilFadeout * UnityEngine.Random.value);
			}
		}
	}

	public class WORA_EGG04 : UpdatableAndDeletable
	{
		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!room.world.game.GetStorySession.saveState.miscWorldSaveData.hasRippleEggWarpAbility && !room.world.regionState.rippleSpawnEggState.OuterRimDialFilled())
			{
				return;
			}
			foreach (WarpPoint warpPoint in room.warpPoints)
			{
				if (warpPoint != null && warpPoint.Data.destRoom.ToLowerInvariant() == "wora_dial")
				{
					if (warpPoint.Data.oneWay)
					{
						string key = warpPoint.MyIdentifyingString();
						warpPoint.Data.oneWay = false;
						warpPoint.Data.oneWayEntrance = false;
						warpPoint.Data.oneWayEntranceIdentified = false;
						warpPoint.RemoveFromSaveData();
						room.game.GetStorySession.saveState.deathPersistentSaveData.newlyDiscoveredWarpPoints[key] = warpPoint.ToString();
						room.game.GetStorySession.saveState.miscWorldSaveData.discoveredWarpPoints[key] = warpPoint.ToString();
					}
					Destroy();
				}
			}
		}
	}

	public class WAUA_SHOP : UpdatableAndDeletable
	{
		public Player player;

		public WAUA_SHOP(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room)
				{
					room.game.GetStorySession.saveState.miscWorldSaveData.visitedShopRoom = true;
					Destroy();
					break;
				}
			}
		}
	}

	public class WAUA_BATH : UpdatableAndDeletable
	{
		public int counter;

		public FadeOut fadeIn;

		public WAUA_BATH(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.GetStorySession.saveState.denPosition == "WAUA_BATH")
			{
				bool flag = true;
				for (int i = 0; i < room.game.Players.Count; i++)
				{
					Player player = null;
					if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room)
					{
						player = room.game.Players[i].realizedCreature as Player;
					}
					if (player == null)
					{
						continue;
					}
					if (player.sceneFlag)
					{
						Destroy();
						break;
					}
					player.sleepCurlUp = 1f;
					player.sleepCounter = 99;
					if (counter < 40)
					{
						player.SuperHardSetPosition(new Vector2(1500f + (float)i * 40f, 130f));
					}
					if (flag)
					{
						counter++;
						if (fadeIn == null)
						{
							fadeIn = new FadeOut(room, Color.black, 80f, fadeIn: true);
							room.AddObject(fadeIn);
						}
						if (fadeIn != null)
						{
							if (counter < 80)
							{
								fadeIn.fade = 1f;
							}
							if (fadeIn.IsDoneFading())
							{
								SetSceneFlagForAllPlayers(room.game);
								Destroy();
								break;
							}
						}
					}
					flag = false;
				}
			}
			else
			{
				Destroy();
			}
		}
	}

	public class WAUA_TOYS : UpdatableAndDeletable
	{
		public int counter;

		public FadeOut fadeIn;

		public FadeOut fadeOut;

		public UrbanToys.BallToy ballToy;

		public UrbanToys.SoftToy softToy;

		public UrbanToys.SpinToy spinToy;

		public UrbanToys.WeirdToy weirdToy;

		public int partialInteractionTime;

		public int fullInteractionTime;

		public int timeAfterFadeOut;

		public OmniDirectionalSound omni;

		public float omniTargetVolume;

		public WAUA_TOYS(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			bool flag = true;
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				Player player = null;
				if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room && !room.game.Players[i].realizedCreature.dead)
				{
					player = room.game.Players[i].realizedCreature as Player;
				}
				if (player == null)
				{
					continue;
				}
				if (room.game.GetStorySession.saveState.denPosition == "WAUA_TOYS")
				{
					if (player.sceneFlag)
					{
						Destroy();
						break;
					}
					player.sleepCurlUp = 1f;
					player.sleepCounter = 99;
					if (counter < 40)
					{
						switch (i)
						{
						case 0:
							player.SuperHardSetPosition(new Vector2(720f, 1720f));
							break;
						case 1:
							player.SuperHardSetPosition(new Vector2(640f, 1640f));
							break;
						case 2:
							player.SuperHardSetPosition(new Vector2(520f, 1620f));
							break;
						default:
							player.SuperHardSetPosition(new Vector2(320f, 1620f));
							break;
						}
					}
					if (flag)
					{
						counter++;
						if (fadeIn == null)
						{
							fadeIn = new FadeOut(room, Color.black, 80f, fadeIn: true);
							room.AddObject(fadeIn);
						}
						if (fadeIn != null)
						{
							if (counter < 80)
							{
								fadeIn.fade = 1f;
							}
							if (fadeIn.IsDoneFading())
							{
								SetSceneFlagForAllPlayers(room.game);
								Destroy();
								break;
							}
						}
					}
				}
				if (room.game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow && !room.game.rainWorld.progression.miscProgressionData.beaten_Watcher_SpinningTop)
				{
					if (flag)
					{
						if (ballToy == null || softToy == null || spinToy == null || weirdToy == null)
						{
							for (int j = 0; j < room.physicalObjects.Length; j++)
							{
								for (int k = 0; k < room.physicalObjects[j].Count; k++)
								{
									if (room.physicalObjects[j][k] is UrbanToys.BallToy)
									{
										ballToy = room.physicalObjects[j][k] as UrbanToys.BallToy;
									}
									else if (room.physicalObjects[j][k] is UrbanToys.SoftToy)
									{
										softToy = room.physicalObjects[j][k] as UrbanToys.SoftToy;
									}
									else if (room.physicalObjects[j][k] is UrbanToys.SpinToy)
									{
										spinToy = room.physicalObjects[j][k] as UrbanToys.SpinToy;
									}
									else if (room.physicalObjects[j][k] is UrbanToys.WeirdToy)
									{
										weirdToy = room.physicalObjects[j][k] as UrbanToys.WeirdToy;
									}
								}
							}
						}
						int num = 0;
						if (ballToy != null && ballToy.interactedWith)
						{
							num++;
						}
						if (softToy != null && softToy.interactedWith)
						{
							num++;
						}
						if (spinToy != null && spinToy.interactedWith)
						{
							num++;
						}
						if (weirdToy != null && weirdToy.interactedWith)
						{
							num++;
						}
						if (num >= 3)
						{
							partialInteractionTime++;
						}
						if (num == 4)
						{
							fullInteractionTime++;
						}
						if (fadeOut == null && (partialInteractionTime > 2400 || fullInteractionTime > 400))
						{
							fadeOut = new FadeOut(room, Color.black, 400f, fadeIn: false);
							room.AddObject(fadeOut);
						}
						if (num > 0)
						{
							omniTargetVolume = 0.25f;
						}
						if (omni == null)
						{
							for (int l = 0; l < room.roomSettings.ambientSounds.Count; l++)
							{
								if (room.roomSettings.ambientSounds[l].type == AmbientSound.Type.Omnidirectional && room.roomSettings.ambientSounds[l].sample.ToLowerInvariant() == "rwtw_spinningtop_loop.ogg")
								{
									omni = room.roomSettings.ambientSounds[l] as OmniDirectionalSound;
									omni.volume = 0f;
									break;
								}
							}
						}
						if (omni != null)
						{
							if (fadeOut == null)
							{
								omni.volume = Mathf.Lerp(omni.volume, omniTargetVolume, 0.01f);
							}
							else
							{
								omni.volume = Mathf.Lerp(omniTargetVolume, 0f, fadeOut.fade);
							}
						}
						if (fadeOut != null && fadeOut.IsDoneFading())
						{
							timeAfterFadeOut++;
							if (timeAfterFadeOut >= 80)
							{
								room.game.rainWorld.processManager.statsAfterCredits = false;
								room.game.rainWorld.progression.miscProgressionData.watcherEndingID = 1;
								RainWorldGame.BeatGameMode(room.game, standardVoidSea: false);
								room.game.manager.nextSlideshow = WatcherEnums.SlideShowID.EndingSpinningTop;
								room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
								Destroy();
								break;
							}
						}
					}
					flag = false;
					continue;
				}
				Destroy();
				break;
			}
		}
	}

	public class WRSA_WEAVER : UpdatableAndDeletable
	{
		public VoidWeaverEndingBehavior behav;

		public WRSA_WEAVER(Room room)
		{
			base.room = room;
			VoidWeaverEndingScene obj = new VoidWeaverEndingScene(room);
			room.AddObject(obj);
			room.abstractRoom.mapPos = Vector2.zero;
			if (!room.game.GetStorySession.finalWarpSequenceStarted)
			{
				PlacedObject placedObject = room.roomSettings.placedObjects.FirstOrDefault((PlacedObject pObj) => pObj.type == WatcherEnums.PlacedObjectType.WeaverSpot);
				if (placedObject != null)
				{
					VoidWeaver voidWeaver = new VoidWeaver(placedObject)
					{
						inEndingRoom = true
					};
					behav = new VoidWeaverEndingBehavior(voidWeaver);
					voidWeaver.behavior = behav;
					room.AddObject(voidWeaver);
				}
			}
			else
			{
				OmniDirectionalSound item = new OmniDirectionalSound("RWTW_PlateTree_A1.ogg", inherited: false)
				{
					volume = 0.2f
				};
				room.roomSettings.ambientSounds.Add(item);
				OmniDirectionalSound item2 = new OmniDirectionalSound("RWTW_PlateTree_B1.ogg", inherited: false)
				{
					volume = 0.35f
				};
				room.roomSettings.ambientSounds.Add(item2);
				room.game.cameras[0].virtualMicrophone.NewRoom(room.game.cameras[0].room);
			}
			room.AddObject(new FadeOut(room, Color.black, 10f, fadeIn: true));
			MusicPlayer musicPlayer = room.game.manager.musicPlayer;
			if (musicPlayer != null)
			{
				musicPlayer.FadeOutAllSongs(120f);
				musicPlayer.RequestVoidWeaverEndingMusic();
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			int num = 0;
			List<PhysicalObject>[] physicalObjects = room.physicalObjects;
			for (int i = 0; i < physicalObjects.Length; i++)
			{
				foreach (PhysicalObject item in physicalObjects[i])
				{
					if (item is Player player)
					{
						if (!player.inWeaverVoid)
						{
							player.SuperHardSetPosition(new Vector2(-5200f + (float)num * 20f, player.mainBodyChunk.pos.y));
						}
						player.inWeaverVoid = true;
						num++;
					}
					if (item is Creature creature && !(item is Player))
					{
						creature.stun = Math.Max(creature.stun, 20);
					}
				}
			}
			RoomCamera[] cameras = room.game.cameras;
			foreach (RoomCamera roomCamera in cameras)
			{
				if (roomCamera.room == room)
				{
					roomCamera.freeMoveRect = new Rect(-1E+09f, -50f, 2E+09f, 1E+09f);
				}
			}
		}
	}

	public class WRSA_L01 : UpdatableAndDeletable
	{
		public WRSA_L01(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!Custom.rainWorld.progression.miscProgressionData.beaten_Watcher_VoidWeaver)
			{
				if (room.shortCutsReady)
				{
					room.shortcuts[3].shortCutType = ShortcutData.Type.DeadEnd;
				}
			}
			else
			{
				Destroy();
			}
		}
	}

	public class WRSA_J01 : UpdatableAndDeletable
	{
		public bool eyesSpawned;

		public bool altarAwakeEffectsSpawned;

		public bool sequenceStartSpawned;

		public DaemonEye leftEye;

		public DaemonEye rightEye;

		public DaemonEye centerEye1;

		public DaemonEye centerEye2;

		public DaemonEye centerEye3;

		public WallLight light;

		public StaticSoundLoop heartBeat;

		public RipplesSpawnAbsorption absorption;

		public float lightBoost;

		public Counter heartBeatCooldown = new Counter(40);

		public static Vector2 activationPoint = new Vector2(3540f, 970f);

		public float lightBrightness = 0.2168161f;

		public RectangularDynamicSoundLoop hintAudioPrince;

		public RectangularDynamicSoundLoop hintAudioST;

		public RectangularDynamicSoundLoop hintAudioWeaver;

		public bool PrinceEndingReached => room.game.GetStorySession.saveState.miscWorldSaveData.SentientRotAscensionUnlock;

		public bool STEndingReached => room.game.rainWorld.progression.miscProgressionData.beaten_Watcher_SpinningTop;

		public bool WeaverEndingReached => room.game.rainWorld.progression.miscProgressionData.beaten_Watcher_VoidWeaver;

		public string TranslatedPrinceHint => room.game.manager.rainWorld.inGameTranslator.Translate("The Crown upon the thorn");

		public string TranslatedWeaverHint => room.game.manager.rainWorld.inGameTranslator.Translate("The Lantern in the dusk");

		public string TranslatedSTHint => room.game.manager.rainWorld.inGameTranslator.Translate("The Child forsaken to time");

		public bool AllEndingsReached
		{
			get
			{
				if (STEndingReached && WeaverEndingReached)
				{
					return PrinceEndingReached;
				}
				return false;
			}
		}

		public WRSA_J01(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			SpawnEyes();
			SpawnSequenceStart();
			SpawnAwakeEffects();
			UpdateObjects();
		}

		public void SpawnEyes()
		{
			if (!eyesSpawned)
			{
				room.AddObject(leftEye = new DaemonEye(room, new Vector2(3302.743f, 1168.802f), 73.92f, 9f));
				room.AddObject(rightEye = new DaemonEye(room, new Vector2(3782.044f, 1236.301f), 85.67f, 9f));
				room.AddObject(centerEye1 = new DaemonEye(room, new Vector2(3529.544f, 1339.094f), 71.84f, 19f));
				room.AddObject(centerEye2 = new DaemonEye(room, new Vector2(3491.763f, 1205.125f), 30.73f, 19f));
				room.AddObject(centerEye3 = new DaemonEye(room, new Vector2(3596.327f, 1217.919f), 30.73f, 19f));
				hintAudioST = new RectangularDynamicSoundLoop(this, new FloatRect(leftEye.position.x - 30f, 860f, leftEye.position.x + 30f, 1260f), room);
				hintAudioST.sound = WatcherEnums.WatcherSoundID.DaemonHintST;
				hintAudioPrince = new RectangularDynamicSoundLoop(this, new FloatRect(rightEye.position.x - 30f, 860f, rightEye.position.x + 30f, 1260f), room);
				hintAudioPrince.sound = WatcherEnums.WatcherSoundID.DaemonHintPrince;
				hintAudioWeaver = new RectangularDynamicSoundLoop(this, new FloatRect(centerEye1.position.x - 30f, 860f, centerEye1.position.x + 30f, 1440f), room);
				hintAudioWeaver.sound = WatcherEnums.WatcherSoundID.DaemonHintWeaver;
				eyesSpawned = true;
			}
		}

		public void SpawnSequenceStart()
		{
			if (sequenceStartSpawned || !room.game.GetStorySession.finalWarpSequenceStarted)
			{
				return;
			}
			room.AddObject(new RippleRing(activationPoint, 180, 1f, 1f));
			room.AddObject(new RippleRing(activationPoint, 180, 0.8f, 1.5f));
			room.AddObject(new RippleRing(activationPoint, 180, 0.6f, 2.5f));
			room.AddObject(new RippleRing(activationPoint, 180, 0.5f, 2f));
			Room obj = room;
			RipplesSpawnAbsorption obj2 = new RipplesSpawnAbsorption
			{
				pos = activationPoint
			};
			RipplesSpawnAbsorption obj3 = obj2;
			absorption = obj2;
			obj.AddObject(obj3);
			absorption.bigArea = 0f;
			absorption.targetFadeSpeed = 0.05f;
			absorption.targetFade = 1f;
			absorption.size = 100f;
			sequenceStartSpawned = true;
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				if (room.game.Players[i].realizedCreature != null)
				{
					room.game.Players[i].realizedCreature.Stun(40);
					(room.game.Players[i].realizedCreature as Player).watcherTargetInstability = 0.7f;
					(room.game.Players[i].realizedCreature as Player).watcherInstabilityGain = 0.001f;
				}
			}
			room.PlaySound(WatcherEnums.WatcherSoundID.Warp_Point_Distortion_Wave_Hit_Player, activationPoint, 0.5f, 1.5f);
			for (int j = 0; j < 40; j++)
			{
				SpawnSpark(10f);
			}
		}

		public void SpawnAwakeEffects()
		{
			if (AllEndingsReached && !altarAwakeEffectsSpawned)
			{
				if (heartBeat == null)
				{
					heartBeat = new StaticSoundLoop(WatcherEnums.WatcherSoundID.HeartbeatMed_LOOP, activationPoint, room, 0.4f, 0.5f)
					{
						fadeOutOnDestroyFrames = 160
					};
				}
				room.AddObject(light = new WallLight(room, new WallLight.WallLightData(null)
				{
					pos = new Vector2(3310.656f, 938.2157f),
					up = new Vector2(2.001495f, 110.0524f),
					right = new Vector2(464.2678f, 1.005234f),
					one = new Vector2(460.265f, 108.0419f),
					hue = 0.08702087f,
					saturation = 0.347714f,
					value = 0.2168161f
				}));
				altarAwakeEffectsSpawned = true;
			}
		}

		public void SpawnSpark(float velocityBoost = 0f)
		{
			room.AddObject(new WarpSpawningRipple.RippleSpark(new Vector2(3310f, 960f) + Vector2.right * (464f * UnityEngine.Random.value), Vector2.up * (UnityEngine.Random.Range(2f, 4f) + velocityBoost), 80, 200)
			{
				soundPlayed = true
			});
		}

		public void UpdateObjects()
		{
			bool allEndingsReached = AllEndingsReached;
			bool finalWarpSequenceStarted = room.game.GetStorySession.finalWarpSequenceStarted;
			heartBeatCooldown.Tick();
			lightBoost *= 0.98f;
			if (finalWarpSequenceStarted)
			{
				lightBrightness *= 0.99f;
			}
			if (light != null)
			{
				light.value = (allEndingsReached ? Mathf.Clamp01(lightBrightness - lightBoost * 0.1f) : 0f);
			}
			leftEye.active = STEndingReached;
			rightEye.active = PrinceEndingReached;
			centerEye1.active = WeaverEndingReached;
			centerEye2.active = allEndingsReached;
			centerEye3.active = allEndingsReached;
			if (!allEndingsReached)
			{
				for (int i = 0; i < room.game.cameras.Length; i++)
				{
					if (room.game.cameras[i].followAbstractCreature.realizedCreature is Player camFollowingPlayer)
					{
						UpdateTutorials(camFollowingPlayer, i);
					}
				}
			}
			hintAudioST.Update();
			hintAudioST.Volume = 1f;
			hintAudioPrince.Update();
			hintAudioPrince.Volume = 1f;
			hintAudioWeaver.Update();
			hintAudioWeaver.Volume = 1f;
			if (!allEndingsReached)
			{
				return;
			}
			if (absorption != null && absorption.fade > 0.98f)
			{
				absorption.targetFadeSpeed = 0.01f;
				absorption.targetFade = 0f;
			}
			if (finalWarpSequenceStarted)
			{
				return;
			}
			if (UnityEngine.Random.value > 0.97f)
			{
				SpawnSpark();
			}
			if ((int)heartBeatCooldown > heartBeatCooldown.max - 2)
			{
				for (int j = 0; j < UnityEngine.Random.Range(3, 6); j++)
				{
					SpawnSpark(UnityEngine.Random.Range(7f, 10f));
				}
			}
			heartBeat?.Update();
			if (heartBeat.emitter != null && heartBeat.emitter.currentSoundObject != null && heartBeat.emitter.currentSoundObject.audioSource != null && Mathf.Repeat(heartBeat.emitter.currentSoundObject.audioSource.time / 7.172f * 12f, 1f) < 0.2f && heartBeatCooldown.isFinished)
			{
				room.AddObject(new RippleRing(activationPoint, 220, 0.75f, 1.5f));
				lightBoost = 1f;
				heartBeatCooldown.Reset();
			}
		}

		public void UpdateTutorials(Player camFollowingPlayer, int cam)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (camFollowingPlayer.room == room && camFollowingPlayer.mainBodyChunk.pos.y > 800f)
			{
				if (!STEndingReached && Mathf.Abs(camFollowingPlayer.mainBodyChunk.pos.x - leftEye.position.x) < 80f)
				{
					flag = true;
				}
				else if (!WeaverEndingReached && Mathf.Abs(camFollowingPlayer.mainBodyChunk.pos.x - centerEye1.position.x) < 80f)
				{
					flag3 = true;
				}
				else if (!PrinceEndingReached && Mathf.Abs(camFollowingPlayer.mainBodyChunk.pos.x - rightEye.position.x) < 80f)
				{
					flag2 = true;
				}
			}
			TextPrompt textPrompt = room.game.cameras[cam].hud.textPrompt;
			if (textPrompt.messages.Count > 0)
			{
				if (flag && textPrompt.messages[cam].text == TranslatedSTHint)
				{
					textPrompt.messages[0].time = 20;
				}
				if (flag2 && textPrompt.messages[cam].text == TranslatedPrinceHint)
				{
					textPrompt.messages[0].time = 20;
				}
				if (flag3 && textPrompt.messages[cam].text == TranslatedWeaverHint)
				{
					textPrompt.messages[0].time = 20;
				}
			}
			else
			{
				if (flag)
				{
					textPrompt.AddMessage(TranslatedSTHint, 20, 20, darken: false, hideHud: true);
				}
				if (flag2)
				{
					textPrompt.AddMessage(TranslatedPrinceHint, 20, 20, darken: false, hideHud: true);
				}
				if (flag3)
				{
					textPrompt.AddMessage(TranslatedWeaverHint, 20, 20, darken: false, hideHud: true);
				}
			}
		}
	}

	public class WRSA_WEAVER02 : UpdatableAndDeletable
	{
		public int counter;

		public FadeOut fadeIn;

		public WRSA_WEAVER02(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.GetStorySession.saveState.denPosition == "WRSA_WEAVER02")
			{
				bool flag = true;
				for (int i = 0; i < room.game.Players.Count; i++)
				{
					Player player = null;
					if (room.game.Players[i].realizedCreature != null && room.game.Players[i].realizedCreature.room == room)
					{
						player = room.game.Players[i].realizedCreature as Player;
					}
					if (player == null)
					{
						continue;
					}
					if (player.sceneFlag)
					{
						Destroy();
						break;
					}
					player.sleepCurlUp = 1f;
					player.sleepCounter = 99;
					if (counter < 40)
					{
						player.SuperHardSetPosition(new Vector2(740f + (float)i * 40f - (float)(room.game.Players.Count - 1) * 20f, 260f));
					}
					if (flag)
					{
						counter++;
						if (fadeIn == null)
						{
							fadeIn = new FadeOut(room, Color.black, 80f, fadeIn: true);
							room.AddObject(fadeIn);
						}
						if (fadeIn != null)
						{
							if (counter < 80)
							{
								fadeIn.fade = 1f;
							}
							if (fadeIn.IsDoneFading())
							{
								SetSceneFlagForAllPlayers(room.game);
								Destroy();
								break;
							}
						}
					}
					flag = false;
				}
			}
			else
			{
				Destroy();
			}
		}
	}

	public class SB_D07 : UpdatableAndDeletable
	{
		public SB_D07(Room room)
		{
			base.room = room;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room.game.GetStorySession.finalWarpSequenceStarted && room.shortCutsReady)
			{
				room.shortcuts[0].shortCutType = ShortcutData.Type.DeadEnd;
			}
			else
			{
				Destroy();
			}
		}
	}

	public static string[] sentientRotVanillaRooms = new string[14]
	{
		"CC_B10W", "CC_A06W", "CC_SUMP03W", "HI_A17W", "HI_C15W", "HI_W05", "SU_B09W", "SH_B12W", "SH_E05W", "SH_A19",
		"SH_OVERHEADW", "LF_A13W", "LF_B05W", "LF_B01W"
	};

	public static void AddRoomSpecificScript(Room room)
	{
		if (!room.game.IsStorySession || room.game.StoryCharacter != WatcherEnums.SlugcatStatsName.Watcher)
		{
			return;
		}
		string name = room.abstractRoom.name;
		if (name == "HI_W14" && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0)
		{
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				room.AddObject(new HI_W14(room, i));
			}
		}
		if (name == "HI_W05" && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.denPosition == "HI_W05")
		{
			room.AddObject(new HI_W05(room));
		}
		if (name == "WORA_AI")
		{
			room.AddObject(new WORA_AI(room));
		}
		if (name == "WORA_THRONE02" || name == "WORA_AI")
		{
			room.AddObject(new WORA_KarmaSigils(room));
		}
		if (name == "WORA_DESERT6")
		{
			room.AddObject(new WORA_DESERT6(room));
		}
		if (name == "WORA_EGG" && !room.world.game.GetStorySession.saveState.miscWorldSaveData.hasRippleEggWarpAbility)
		{
			room.AddObject(new WORA_ElderSpawn(room));
		}
		if (name == "WORA_DIAL")
		{
			room.AddObject(new WORA_RippleDial(room));
		}
		if (name == "WORA_EGG04")
		{
			room.AddObject(new WORA_EGG04());
		}
		if (name == "WAUA_BATH")
		{
			room.AddObject(new WAUA_BATH(room));
		}
		if (name == "WAUA_TOYS")
		{
			room.AddObject(new WAUA_TOYS(room));
		}
		if (name == "WAUA_SHOP")
		{
			room.AddObject(new WAUA_SHOP(room));
		}
		if (name == "WRSA_L01")
		{
			room.AddObject(new WRSA_L01(room));
		}
		if (name == "WRSA_J01")
		{
			room.AddObject(new WRSA_J01(room));
		}
		if (name == "WRSA_WEAVER02" && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.denPosition == "WRSA_WEAVER02")
		{
			room.AddObject(new WRSA_WEAVER02(room));
		}
		if (name == "WRSA_WEAVER")
		{
			room.AddObject(new WRSA_WEAVER(room));
		}
		if (name == "SB_D07")
		{
			room.AddObject(new SB_D07(room));
		}
		for (int j = 0; j < sentientRotVanillaRooms.Length; j++)
		{
			if (name == sentientRotVanillaRooms[j])
			{
				room.AddObject(new InfectSentientRot(room));
			}
		}
	}

	public static string PrinceRoomRemapping(string roomName, RainWorldGame game)
	{
		if (game == null)
		{
			return roomName;
		}
		int numberOfPrinceEncounters = game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters;
		if (numberOfPrinceEncounters == 0)
		{
			return roomName;
		}
		return roomName + "-" + Custom.IntClamp(numberOfPrinceEncounters, 1, 3);
	}

	public static void SetSceneFlagForAllPlayers(RainWorldGame game)
	{
		for (int i = 0; i < game.Players.Count; i++)
		{
			if (game.Players[i].realizedCreature != null)
			{
				(game.Players[i].realizedCreature as Player).sceneFlag = true;
			}
		}
	}

	public static void FinalWarpSequenceUpdate(RainWorldGame game)
	{
		if (!Custom.rainWorld.progression.miscProgressionData.beaten_Watcher_SpinningTop || !Custom.rainWorld.progression.miscProgressionData.beaten_Watcher_VoidWeaver || !game.GetStorySession.saveState.miscWorldSaveData.SentientRotAscensionUnlock)
		{
			return;
		}
		bool flag = false;
		if (game.GetStorySession.finalWarpSequenceStarted || (game.world != null && Region.IsDaemonRegion(game.world.name)))
		{
			for (int i = 0; i < game.Players.Count; i++)
			{
				if (game.Players[i].realizedCreature != null && (game.Players[i].realizedCreature as Player).room != null && (game.Players[i].realizedCreature as Player).room.abstractRoom.name.ToLowerInvariant() == "wrsa_tree02")
				{
					(game.Players[i].realizedCreature as Player).inWeaverVoid = false;
					RoomCamera[] cameras = game.cameras;
					for (int j = 0; j < cameras.Length; j++)
					{
						cameras[j].freeMoveRect = null;
					}
				}
				if (game.Players[i].realizedCreature == null || (game.Players[i].realizedCreature as Player).SlugCatClass != WatcherEnums.SlugcatStatsName.Watcher || (game.Players[i].realizedCreature as Player).dead || flag)
				{
					continue;
				}
				Player player = game.Players[i].realizedCreature as Player;
				float a = 0.0006f;
				if (player.watcherForceOriginalLook)
				{
					a = 0f;
				}
				if (game.GetStorySession.finalWarpSequenceStarted)
				{
					player.WatcherGainInstability(Mathf.Max(a, player.watcherInstabilityGain), Mathf.Max(0.3f, player.watcherTargetInstability));
				}
				if (player.pendingForcedWarpRoom != null)
				{
					game.GetStorySession.finalWarpSequenceCounter = 0;
					break;
				}
				string text = ((player.room == null) ? "" : player.room.abstractRoom.name.ToLowerInvariant());
				if (text == "wrsa_j01" && Vector2.Distance(player.mainBodyChunk.pos, WRSA_J01.activationPoint) < 100f)
				{
					flag = true;
					if (game.GetStorySession.finalWarpSequenceCounter > 80)
					{
						game.GetStorySession.finalWarpSequenceStarted = true;
						player.StartPendingForcedWarp("HI_W05", new Vector2(440f, 290f), 700);
						game.GetStorySession.finalWarpSequenceCounter = 0;
					}
				}
				else if ((text == "hi_b04w" && player.mainBodyChunk.pos.x >= 1360f) || (text == "hi_w14" && player.mainBodyChunk.pos.x <= 1140f) || (text == "hi_w09" && player.mainBodyChunk.pos.y >= 1550f))
				{
					flag = true;
					if (game.GetStorySession.finalWarpSequenceCounter > 1)
					{
						player.StartPendingForcedWarp("SU_A40", new Vector2(550f, 370f), 660);
						game.GetStorySession.finalWarpSequenceCounter = 0;
					}
				}
				else if ((text == "su_a63" && player.mainBodyChunk.pos.x <= 660f) || (text == "su_a53" && player.mainBodyChunk.pos.x <= 720f))
				{
					flag = true;
					if (game.GetStorySession.finalWarpSequenceCounter > 1)
					{
						player.StartPendingForcedWarp("WAUA_TOYS", new Vector2(380f, 1830f), 620);
						game.GetStorySession.finalWarpSequenceCounter = 0;
					}
				}
				else if (text == "waua_toys" && game.GetStorySession.finalWarpSequenceStarted)
				{
					flag = true;
					player.watcherTargetInstability = 0.7f;
					for (int k = 0; k < player.room.shortcuts.Length; k++)
					{
						player.room.shortcuts[k].shortCutType = ShortcutData.Type.DeadEnd;
					}
					if (game.GetStorySession.finalWarpSequenceCounter > 1200)
					{
						player.room.game.GetStorySession.princeEncounteredThisCycle = false;
						player.StartPendingForcedWarp("WRSA_PRINCE01", new Vector2(690f, 220f), 580);
						game.GetStorySession.finalWarpSequenceCounter = 0;
					}
				}
				else if (text == "wrsa_prince03" && game.GetStorySession.finalWarpSequenceStarted)
				{
					Prince prince = player.room.physicalObjects[1].OfType<Prince>().FirstOrDefault();
					if (!player.dead && prince != null && prince.behavior != null && prince.behavior.startedAscensionDialog && !prince.behavior.ConversationActive)
					{
						flag = true;
					}
					if (game.GetStorySession.finalWarpSequenceCounter > 40)
					{
						player.StartPendingForcedWarp("WRSA_WEAVER", new Vector2(-5200f, 320f), 540);
						game.GetStorySession.finalWarpSequenceCounter = 0;
						player.watcherTargetInstability = 0.8f;
						player.watcherInstabilityGain = 0.01f;
					}
				}
				else if (text == "WRSA_WEAVER".ToLowerInvariant() && game.GetStorySession.finalWarpSequenceStarted)
				{
					flag = true;
					if (game.GetStorySession.finalWarpSequenceCounter > 1200)
					{
						player.watcherTargetInstability = 0.6f;
						player.StartPendingForcedWarp("WRSA_TREE02", new Vector2(1570f, 350f), 500);
						game.GetStorySession.finalWarpSequenceCounter = 0;
					}
				}
				else if (text == "wrsa_tree06" && player.mainBodyChunk.pos.y >= 970f && player.mainBodyChunk.pos.x >= 250f && player.mainBodyChunk.pos.x < 750f && game.GetStorySession.finalWarpSequenceStarted)
				{
					flag = true;
					player.watcherTargetInstability = 1f;
					player.watcherInstabilityGain = 0.006f;
					if (game.GetStorySession.finalWarpSequenceCounter > 160)
					{
						player.forceMonolithWarp = true;
						player.StartPendingForcedWarp("SB_D07", new Vector2(460f, 990f), 460);
						game.GetStorySession.finalWarpSequenceCounter = 0;
					}
				}
				else if (IsWatcherTreeRoom(text))
				{
					player.watcherForceOriginalLook = true;
					player.watcherTargetInstability = 0f;
					player.watcherInstability = 0f;
					player.watcherInstabilityGain = 0f;
				}
				if (flag)
				{
					game.GetStorySession.finalWarpSequenceCounter++;
				}
			}
		}
		if (!flag)
		{
			game.GetStorySession.finalWarpSequenceCounter = 0;
		}
		if (game.cameras[0].warpPointTimer != null)
		{
			game.GetStorySession.finalWarpSequenceCounter = 0;
		}
	}

	public static bool IsWatcherTreeRoom(string room)
	{
		switch (room)
		{
		default:
			return room == "wrsa_tree06";
		case "wrsa_tree01":
		case "wrsa_tree02":
		case "wrsa_tree03":
		case "wrsa_tree04":
		case "wrsa_tree05":
			return true;
		}
	}
}
