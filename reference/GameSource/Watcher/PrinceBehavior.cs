using System;
using System.Collections.Generic;
using System.Linq;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class PrinceBehavior : Conversation.IOwnAConversation
{
	public class Pose : ExtEnum<Pose>
	{
		public static Pose Vertical = new Pose("Vertical", register: true);

		public static Pose Lean = new Pose("Lean", register: true);

		public static Pose Hunch = new Pose("Hunch", register: true);

		public static Pose Away = new Pose("Away", register: true);

		public static Pose Paralyzed = new Pose("Paralyzed", register: true);

		public Pose(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class PrinceConversation : Conversation
	{
		public PrinceBehavior owner;

		public PrinceConversation(PrinceBehavior owner, ID id, DialogBox dialogBox)
			: base(owner, id, dialogBox)
		{
			this.owner = owner;
			AddEvents();
		}

		public override void AddEvents()
		{
			if (id == WatcherEnums.ConversationID.Prince_1)
			{
				if (owner.prince.room.game.GetStorySession.saveState.miscWorldSaveData.numberOfVoidWeaverEncounters >= 3 && owner.prince.room.game.GetStorySession.saveState.miscWorldSaveData.princeWeaverGrowthAcknowledgement == 0)
				{
					events.Add(new TextEvent(this, 0, owner.Translate("Oh, my friend, you have grown!"), 60));
					owner.prince.room.game.GetStorySession.saveState.miscWorldSaveData.princeWeaverGrowthAcknowledgement = 1;
				}
				else
				{
					LoadEventsFromFile(PrinceConversationToId(id));
				}
			}
			else
			{
				LoadEventsFromFile(PrinceConversationToId(id));
			}
		}

		public static int PrinceConversationToId(ID id)
		{
			if (id == WatcherEnums.ConversationID.Prince_1)
			{
				return 222;
			}
			if (id == WatcherEnums.ConversationID.Prince_2)
			{
				return 223;
			}
			if (id == WatcherEnums.ConversationID.Prince_3)
			{
				return 224;
			}
			if (id == WatcherEnums.ConversationID.Prince_4)
			{
				return 225;
			}
			if (id == WatcherEnums.ConversationID.Prince_5)
			{
				return 226;
			}
			if (id == WatcherEnums.ConversationID.Prince_6)
			{
				return 227;
			}
			if (id == WatcherEnums.ConversationID.Prince_7)
			{
				return 228;
			}
			if (id == WatcherEnums.ConversationID.Prince_KS_1_1)
			{
				return 214;
			}
			if (id == WatcherEnums.ConversationID.Prince_KS_1_2)
			{
				return 215;
			}
			if (id == WatcherEnums.ConversationID.Prince_KS_1_3)
			{
				return 216;
			}
			if (id == WatcherEnums.ConversationID.Prince_KS_1_4)
			{
				return 217;
			}
			if (id == WatcherEnums.ConversationID.Prince_KS_2_1)
			{
				return 218;
			}
			if (id == WatcherEnums.ConversationID.Prince_KS_2_2)
			{
				return 219;
			}
			if (id == WatcherEnums.ConversationID.Prince_KS_2_3)
			{
				return 220;
			}
			if (id == WatcherEnums.ConversationID.Prince_KS_2_4)
			{
				return 221;
			}
			if (id == WatcherEnums.ConversationID.Prince_Weaver_1)
			{
				return 233;
			}
			if (id == WatcherEnums.ConversationID.Prince_Weaver_2)
			{
				return 234;
			}
			if (id == WatcherEnums.ConversationID.Prince_Ascension)
			{
				return 235;
			}
			if (id == WatcherEnums.ConversationID.Prince_Ascension_Marked)
			{
				return 242;
			}
			return 0;
		}

		public static ID TargetConversation(int highestConversationSeen, int infections)
		{
			ID result = null;
			if (highestConversationSeen < PrinceConversationToId(WatcherEnums.ConversationID.Prince_2))
			{
				result = WatcherEnums.ConversationID.Prince_2;
			}
			else if (infections > 4 && highestConversationSeen < PrinceConversationToId(WatcherEnums.ConversationID.Prince_3))
			{
				result = WatcherEnums.ConversationID.Prince_3;
			}
			else if (infections > 6 && highestConversationSeen < PrinceConversationToId(WatcherEnums.ConversationID.Prince_4))
			{
				result = WatcherEnums.ConversationID.Prince_4;
			}
			else if (infections > 8 && highestConversationSeen < PrinceConversationToId(WatcherEnums.ConversationID.Prince_5))
			{
				result = WatcherEnums.ConversationID.Prince_5;
			}
			else if (infections > 10 && highestConversationSeen < PrinceConversationToId(WatcherEnums.ConversationID.Prince_6))
			{
				result = WatcherEnums.ConversationID.Prince_6;
			}
			else if (infections > 12 && highestConversationSeen < PrinceConversationToId(WatcherEnums.ConversationID.Prince_7))
			{
				result = WatcherEnums.ConversationID.Prince_7;
			}
			return result;
		}
	}

	public bool hasNoticedPlayer;

	public bool saidHello;

	public int helloDelay;

	public bool readyForDeadTalk;

	public int[] playersLastDeadState;

	public bool giftTalk;

	public bool needToClearConversation;

	public int noConversationTime;

	public PrinceConversation currentConversation;

	public Vector2 idleSpot;

	public int timeUntilNextIdleSpot;

	public int getToOrbitRadius;

	public float getToOrbitSpeed;

	public int poseChangeTimer;

	public Conversation.ID conversationAfterHello;

	public int timeSincePlayerGrabbedByTentacles;

	public int tickCamoDialogTimer;

	public bool playerLastCamo;

	public string lastDialogBoxMessage;

	public int painCooldown;

	public bool paralyzed;

	public bool triggeredEncounterAchievement;

	public bool givingMark;

	public int givingMarkTimer;

	public float givingMarkProgression;

	public OmniDirectionalSound markOmni;

	public PrinceSpasmer[] spasmers;

	public float hasNoticedPlayerX = 1110f;

	public Prince prince;

	public Player bestPlayer;

	public Vector2 lookPoint;

	public ChunkSoundEmitter voice;

	public Pose pose = Pose.Vertical;

	public bool startedAscensionDialog;

	public Vector2 GetToPos { get; set; }

	public Vector2 GetToDir { get; set; }

	public bool AscensionPrince => Region.IsDaemonRegion(prince.room.world.name);

	public bool ConversationActive => currentConversation != null;

	public DialogBox dialogBox
	{
		get
		{
			if (currentConversation != null)
			{
				return currentConversation.dialogBox;
			}
			if (prince.room.game.cameras[0].hud == null)
			{
				return null;
			}
			if (prince.room.game.cameras[0].hud.dialogBox == null)
			{
				prince.room.game.cameras[0].hud.InitDialogBox();
			}
			return prince.room.game.cameras[0].hud.dialogBox;
		}
	}

	public List<Player> PlayersInRoom => (from x in prince?.room?.game?.NonPermaDeadPlayers?.Where((AbstractCreature x) => x.Room == prince.room.abstractRoom)
		select x?.realizedCreature as Player into x
		orderby x.dead
		select x).ToList();

	public PrinceBehavior(Prince prince)
	{
		this.prince = prince;
		timeSincePlayerGrabbedByTentacles = 1000;
		playerLastCamo = true;
		lastDialogBoxMessage = "";
	}

	public void StunCoopPlayers(int st)
	{
		foreach (Player item in PlayersInRoom)
		{
			item.Stun(st);
			if (item.slugOnBack != null && item.slugOnBack.HasASlug && !item.slugOnBack.slugcat.dead)
			{
				item.slugOnBack.DropSlug();
			}
		}
	}

	public void Update()
	{
		if (!prince.Consious)
		{
			return;
		}
		if (AscensionPrince)
		{
			hasNoticedPlayerX = 840f;
		}
		if (playersLastDeadState == null)
		{
			playersLastDeadState = new int[prince.room.game.Players.Count];
			for (int i = 0; i < prince.room.game.Players.Count; i++)
			{
				playersLastDeadState[i] = -1;
			}
		}
		bestPlayer = null;
		float num = 0f;
		Player player = null;
		float num2 = 0f;
		Player player2 = null;
		bool flag = false;
		bool flag2 = false;
		float rippleLevel = 0f;
		for (int j = 0; j < prince.room.game.Players.Count; j++)
		{
			Player player3 = prince.room.game.Players[j].realizedCreature as Player;
			if (prince.room.game.Players[j].realizedCreature != null && prince.room.game.Players[j].realizedCreature.room == prince.room)
			{
				float num3 = Mathf.Abs(hasNoticedPlayerX - player3.mainBodyChunk.pos.x);
				if (player == null || num3 < num)
				{
					player = player3;
					num = num3;
				}
				if (player3.mainBodyChunk.pos.x <= hasNoticedPlayerX && (player2 == null || num3 < num2))
				{
					player2 = player3;
					num2 = num3;
				}
				if (player3.GrabbedByDaddyCorruption)
				{
					flag = true;
				}
				if (player3.isCamo)
				{
					flag2 = true;
					rippleLevel = player3.rippleLevel;
				}
			}
			if (player3.dead && playersLastDeadState[j] == 0)
			{
				readyForDeadTalk = true;
				bestPlayer = player3;
			}
			playersLastDeadState[j] = (player3.dead ? 1 : 0);
		}
		if (player2 != null && !readyForDeadTalk)
		{
			bestPlayer = player2;
		}
		else if (player != null && !readyForDeadTalk)
		{
			bestPlayer = player;
		}
		if (AscensionPrince && bestPlayer == null && currentConversation != null && !needToClearConversation)
		{
			readyForDeadTalk = true;
			TalkToDeadPlayer();
		}
		if (flag)
		{
			timeSincePlayerGrabbedByTentacles = 0;
		}
		else
		{
			timeSincePlayerGrabbedByTentacles++;
		}
		paralyzed = prince.room.game.GetStorySession.saveState.miscWorldSaveData.SentientRotEndingConditionsMet && !AscensionPrince;
		if (dialogBox != null && dialogBox.messages.Count > 0 && dialogBox.CurrentMessage.text != lastDialogBoxMessage && dialogBox.CurrentMessage.text != "...")
		{
			AirVoice(WatcherEnums.WatcherSoundID.Prince_Talk);
		}
		lastDialogBoxMessage = ((dialogBox == null || dialogBox.CurrentMessage == null) ? "" : dialogBox.CurrentMessage.text);
		if (paralyzed)
		{
			lookPoint = new Vector2(GetToPos.x, GetToPos.y + 500f);
		}
		else if (!hasNoticedPlayer)
		{
			lookPoint = GetToPos;
			if (bestPlayer != null && bestPlayer.mainBodyChunk.pos.x <= hasNoticedPlayerX)
			{
				hasNoticedPlayer = true;
			}
		}
		else if (!saidHello)
		{
			if (prince.room.game.GetStorySession.princeEncounteredThisCycle && !AscensionPrince)
			{
				saidHello = true;
			}
			else if (helloDelay < 40)
			{
				InitateConversation();
				if (!AscensionPrince)
				{
					prince.room.game.GetStorySession.princeEncounteredThisCycle = true;
					prince.room.game.GetStorySession.saveState.miscWorldSaveData.lockedOuterRimProgressionFlag = true;
					prince.room.game.GetStorySession.saveState.miscWorldSaveData.cycleLastEncounteredPrince = prince.room.game.GetStorySession.saveState.cycleNumber;
				}
				saidHello = true;
			}
			else
			{
				helloDelay++;
			}
		}
		else if (hasNoticedPlayer && (bestPlayer != null || readyForDeadTalk) && saidHello)
		{
			if (bestPlayer != null)
			{
				lookPoint = bestPlayer.firstChunk.pos;
			}
			if (needToClearConversation && noConversationTime >= 35)
			{
				needToClearConversation = false;
			}
			if (readyForDeadTalk && !needToClearConversation)
			{
				TalkToDeadPlayer();
			}
			else
			{
				if (noConversationTime == 30 && conversationAfterHello != null)
				{
					currentConversation = new PrinceConversation(this, conversationAfterHello, dialogBox);
					conversationAfterHello = null;
				}
				if (currentConversation != null)
				{
					tickCamoDialogTimer = 0;
				}
				else if (!needToClearConversation && !AscensionPrince)
				{
					for (int k = 0; k < prince.room.socialEventRecognizer.ownedItemsOnGround.Count; k++)
					{
						if (!giftTalk && WillingToInspectItem(prince.room.socialEventRecognizer.ownedItemsOnGround[k].item))
						{
							AcknowledgePlayerGift();
						}
					}
				}
				if (tickCamoDialogTimer > 0)
				{
					tickCamoDialogTimer++;
				}
				if (tickCamoDialogTimer >= 80 && !needToClearConversation && !AscensionPrince)
				{
					AcknowledgePlayerCamouflage(rippleLevel);
					tickCamoDialogTimer = 0;
				}
				if (flag2 && !playerLastCamo)
				{
					tickCamoDialogTimer = 1;
				}
				playerLastCamo = flag2;
			}
		}
		if (currentConversation != null)
		{
			currentConversation.Update();
			if (currentConversation.slatedForDeletion)
			{
				currentConversation = null;
			}
		}
		if (currentConversation == null && (dialogBox == null || dialogBox.messages.Count == 0))
		{
			noConversationTime++;
		}
		else
		{
			noConversationTime = 0;
		}
		if (noConversationTime >= 40 && saidHello && !triggeredEncounterAchievement)
		{
			Custom.rainWorld.processManager.CueAchievement(RainWorld.AchievementID.RotPrince, 5f);
			triggeredEncounterAchievement = true;
		}
		if (givingMark && bestPlayer != null)
		{
			if (markOmni == null)
			{
				for (int l = 0; l < prince.room.roomSettings.ambientSounds.Count; l++)
				{
					if (prince.room.roomSettings.ambientSounds[l].type == AmbientSound.Type.Omnidirectional && prince.room.roomSettings.ambientSounds[l].sample.ToLowerInvariant() == "rwtw_sigil.ogg")
					{
						markOmni = prince.room.roomSettings.ambientSounds[l] as OmniDirectionalSound;
						markOmni.volume = 0f;
						break;
					}
				}
				if (markOmni == null)
				{
					markOmni = new OmniDirectionalSound("RWTW_Sigil.ogg", inherited: false);
					markOmni.volume = 0f;
					prince.room.roomSettings.ambientSounds.Add(markOmni);
				}
			}
			givingMarkTimer++;
			if (givingMarkTimer > 20)
			{
				if (!prince.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
				{
					givingMarkProgression = Mathf.Lerp(givingMarkProgression, 1f, 0.01f);
					if (givingMarkProgression > 0.98f)
					{
						if (ModManager.CoopAvailable)
						{
							StunCoopPlayers(200);
						}
						else
						{
							bestPlayer.Stun(200);
						}
						if (ModManager.CoopAvailable)
						{
							foreach (Player item in PlayersInRoom)
							{
								for (int m = 0; m < 20; m++)
								{
									prince.room.AddObject(new Spark(item.mainBodyChunk.pos, Custom.RNV() * (UnityEngine.Random.value * 40f), new Color(1f, 1f, 1f), null, 30, 120));
								}
							}
						}
						else
						{
							for (int n = 0; n < 20; n++)
							{
								prince.room.AddObject(new Spark(bestPlayer.mainBodyChunk.pos, Custom.RNV() * (UnityEngine.Random.value * 40f), new Color(1f, 1f, 1f), null, 30, 120));
							}
						}
						prince.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1f);
						prince.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark = true;
						givingMarkTimer = 0;
					}
				}
				else
				{
					givingMarkProgression = Mathf.Lerp(givingMarkProgression, 0f, 0.02f);
					if (givingMarkProgression < 0.03f)
					{
						givingMarkProgression = 0f;
						if (currentConversation != null)
						{
							currentConversation.paused = false;
						}
						givingMark = false;
					}
					if (ModManager.CoopAvailable)
					{
						foreach (Player item2 in PlayersInRoom)
						{
							if (item2.graphicsModule != null)
							{
								(item2.graphicsModule as PlayerGraphics).markAlpha = 1f - givingMarkProgression;
							}
						}
					}
					else if (bestPlayer.graphicsModule != null)
					{
						(bestPlayer.graphicsModule as PlayerGraphics).markAlpha = 1f - givingMarkProgression;
					}
				}
			}
			for (int num4 = 0; num4 < prince.room.game.Players.Count; num4++)
			{
				Player player4 = null;
				if (prince.room.game.Players[num4].realizedCreature != null && prince.room.game.Players[num4].realizedCreature.room == prince.room && !prince.room.game.Players[num4].realizedCreature.dead)
				{
					player4 = prince.room.game.Players[num4].realizedCreature as Player;
				}
				if (player4 == null)
				{
					continue;
				}
				float b = Mathf.Lerp(0f, 0.35f, givingMarkProgression);
				player4.warpFatigueEffect = Mathf.Lerp(player4.warpFatigueEffect, b, prince.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark ? 0.16f : 0.08f);
				if (player4.warpFatigueEffect < 0.02f || !givingMark)
				{
					player4.warpFatigueEffect = 0f;
				}
				if (!prince.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark && givingMarkProgression > 0.4f)
				{
					if (spasmers == null)
					{
						spasmers = new PrinceSpasmer[prince.room.game.Players.Count];
					}
					if (spasmers != null && spasmers[num4] == null)
					{
						spasmers[num4] = new PrinceSpasmer(player4, useLeftBound: true, (int)player4.mainBodyChunk.pos.x - 10, useRightBound: true, (int)player4.mainBodyChunk.pos.x + 10);
					}
					if (spasmers != null && spasmers[num4] != null)
					{
						spasmers[num4].Update(eu: false);
					}
				}
				else
				{
					if (!prince.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark || spasmers == null)
					{
						continue;
					}
					for (int num5 = 0; num5 < spasmers.Length; num5++)
					{
						if (spasmers[num5] != null)
						{
							spasmers[num5].Destroy();
						}
					}
					spasmers = null;
				}
			}
		}
		if (markOmni != null)
		{
			markOmni.volume = Mathf.Lerp(markOmni.volume, Mathf.Lerp(0f, 0.65f, givingMarkProgression), 0.08f);
		}
		if (paralyzed)
		{
			pose = Pose.Paralyzed;
		}
		else
		{
			poseChangeTimer--;
			if (poseChangeTimer <= 0)
			{
				pose = GetRandomPose();
				poseChangeTimer = UnityEngine.Random.Range(80, 400);
			}
		}
		if (painCooldown > 0)
		{
			painCooldown--;
		}
		if (paralyzed)
		{
			GetToPos = new Vector2(630f, 710f);
			Vector2 targetDirection = GetTargetDirection();
			GetToDir = Vector3.RotateTowards(GetToDir, targetDirection, (float)Math.PI / 40f, 1f);
			return;
		}
		timeUntilNextIdleSpot--;
		if (timeUntilNextIdleSpot <= 0)
		{
			timeUntilNextIdleSpot = UnityEngine.Random.Range(40, 240);
			float num6 = prince.stem.area.xMin + 100f;
			float num7 = prince.stem.area.xMax - 100f;
			float minInclusive = prince.stem.area.yMin + 100f;
			float maxInclusive = prince.stem.area.yMax - 100f;
			idleSpot = new Vector2(UnityEngine.Random.Range(num6, num6 + (num7 - num6) * 0.5f), UnityEngine.Random.Range(minInclusive, maxInclusive));
			float value = UnityEngine.Random.value;
			if (value < 0.33f)
			{
				getToOrbitRadius = UnityEngine.Random.Range(0, 6);
				getToOrbitSpeed = UnityEngine.Random.Range(2f, 8f);
			}
			else if (value < 0.67f)
			{
				getToOrbitRadius = UnityEngine.Random.Range(30, 70);
				getToOrbitSpeed = UnityEngine.Random.Range(0f, 0.35f);
			}
			else
			{
				getToOrbitRadius = UnityEngine.Random.Range(0, 60);
				getToOrbitSpeed = UnityEngine.Random.Range(0f, 0.05f);
			}
		}
		float f = 1f / (2f * (float)Math.PI) * (float)timeUntilNextIdleSpot * getToOrbitSpeed;
		GetToPos = new Vector2(idleSpot.x + Mathf.Sin(f) * (float)getToOrbitRadius, idleSpot.y + Mathf.Cos(f) * (float)getToOrbitRadius);
		Vector2 targetDirection2 = GetTargetDirection();
		GetToDir = Vector3.RotateTowards(GetToDir, targetDirection2, (float)Math.PI / 40f, 1f);
	}

	public Vector2 GetTargetDirection()
	{
		if (pose == Pose.Vertical || pose == Pose.Paralyzed)
		{
			return Vector2.up;
		}
		if (pose == Pose.Lean)
		{
			return Vector2.Perpendicular(lookPoint - prince.bodyChunks[0].pos).normalized;
		}
		if (pose == Pose.Hunch)
		{
			return (lookPoint - prince.bodyChunks[0].pos).normalized;
		}
		if (pose == Pose.Away)
		{
			return -(lookPoint - prince.bodyChunks[0].pos).normalized;
		}
		return Vector2.up;
	}

	public Pose GetRandomPose()
	{
		return UnityEngine.Random.Range(0, 4) switch
		{
			1 => Pose.Lean, 
			2 => Pose.Hunch, 
			3 => Pose.Away, 
			_ => Pose.Vertical, 
		};
	}

	public void InitateConversation()
	{
		int count = prince.room.game.GetStorySession.saveState.miscWorldSaveData.regionsInfectedBySentientRotSpread.Count;
		int princeWeaverDialogProgression = prince.room.game.GetStorySession.saveState.miscWorldSaveData.princeWeaverDialogProgression;
		bool hasVoidWeaverAbility = prince.room.game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility;
		conversationAfterHello = null;
		if (AscensionPrince)
		{
			bool theMark = prince.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark;
			currentConversation = new PrinceConversation(this, theMark ? WatcherEnums.ConversationID.Prince_Ascension_Marked : WatcherEnums.ConversationID.Prince_Ascension, dialogBox);
			startedAscensionDialog = true;
		}
		else if (hasVoidWeaverAbility)
		{
			Conversation.ID iD = null;
			switch (princeWeaverDialogProgression)
			{
			case 0:
				iD = WatcherEnums.ConversationID.Prince_Weaver_1;
				break;
			case 1:
				iD = WatcherEnums.ConversationID.Prince_Weaver_2;
				break;
			}
			if (iD != null)
			{
				currentConversation = new PrinceConversation(this, iD, dialogBox);
				int highestPrinceConversationSeen = prince.room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen;
				prince.room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen = Mathf.Max(highestPrinceConversationSeen, PrinceConversation.PrinceConversationToId(iD));
			}
			else
			{
				dialogBox.NewMessage(Translate("..."), 200);
			}
			prince.room.game.GetStorySession.saveState.miscWorldSaveData.princeWeaverDialogProgression++;
		}
		else
		{
			currentConversation = new PrinceConversation(this, WatcherEnums.ConversationID.Prince_1, dialogBox);
			int highestPrinceConversationSeen2 = prince.room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen;
			Conversation.ID iD2 = PrinceConversation.TargetConversation(highestPrinceConversationSeen2, count);
			if (iD2 != null)
			{
				prince.room.game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen = Mathf.Max(highestPrinceConversationSeen2, PrinceConversation.PrinceConversationToId(iD2));
				conversationAfterHello = iD2;
			}
		}
	}

	public void TalkToDeadPlayer()
	{
		if (!readyForDeadTalk || !prince.room.ViewedByAnyCamera(prince.firstChunk.pos, 0f) || paralyzed)
		{
			return;
		}
		if (AscensionPrince)
		{
			dialogBox.Interrupt(Translate("..."), 60);
			if (currentConversation != null)
			{
				currentConversation.slatedForDeletion = true;
			}
			currentConversation = null;
		}
		else if (timeSincePlayerGrabbedByTentacles < 200)
		{
			switch (UnityEngine.Random.Range(0, 2))
			{
			case 0:
				dialogBox.Interrupt(Translate("How I have wished for this day. My dear friend, welcome."), 60);
				break;
			case 1:
				dialogBox.Interrupt(Translate("Delight! For you will be exalted in this new kingdom!"), 60);
				break;
			}
		}
		else
		{
			switch (UnityEngine.Random.Range(0, 2))
			{
			case 0:
				dialogBox.Interrupt(Translate("Oh such enthusiasm! I am honored!"), 60);
				break;
			case 1:
				dialogBox.Interrupt(Translate("Never again will you be alone."), 60);
				break;
			}
		}
		readyForDeadTalk = false;
		needToClearConversation = true;
	}

	public void AcknowledgePlayerCamouflage(float rippleLevel)
	{
		if (paralyzed)
		{
			return;
		}
		if (rippleLevel >= 5f)
		{
			switch (UnityEngine.Random.Range(0, 2))
			{
			case 0:
				dialogBox.Interrupt(Translate("Ahh yes! The paths! How wonderous!"), 60);
				break;
			case 1:
				dialogBox.Interrupt(Translate("How auspicious it was, our meeting..."), 60);
				break;
			}
		}
		else
		{
			switch (UnityEngine.Random.Range(0, 2))
			{
			case 0:
				dialogBox.Interrupt(Translate("Marvelous!"), 60);
				break;
			case 1:
				dialogBox.Interrupt(Translate("Oh, show me again!"), 60);
				break;
			}
		}
		needToClearConversation = true;
	}

	public void AcknowledgePlayerGift()
	{
		if (!giftTalk && !paralyzed)
		{
			dialogBox.Interrupt(Translate("Oh, you have brought sundries?"), 60);
			dialogBox.NewMessage(Translate("You can leave them here if you wish, as a contribution."), 60);
			dialogBox.NewMessage(Translate("My processes will dissolve them to their base components and those components will be added to the mass."), 60);
			dialogBox.NewMessage(Translate("Nothing to waste, every bit counts!"), 60);
			giftTalk = true;
			needToClearConversation = true;
		}
	}

	public virtual void Pain()
	{
		if (painCooldown <= 0 && !paralyzed && !AscensionPrince)
		{
			switch (UnityEngine.Random.Range(0, 4))
			{
			case 0:
				dialogBox.Interrupt(Translate("Ah! So spirited!"), 60);
				break;
			case 1:
				dialogBox.Interrupt(Translate("So vital! Hah ha!"), 60);
				break;
			case 2:
				dialogBox.Interrupt(Translate("It is a joy to see you play!"), 60);
				break;
			case 3:
				dialogBox.Interrupt(Translate("Forgive me if I cannot respond in kind. This body is so limited..."), 60);
				break;
			}
			painCooldown = 400;
		}
	}

	public void AirVoice(SoundID line)
	{
		if (paralyzed)
		{
			return;
		}
		if (voice != null)
		{
			if (voice.currentSoundObject != null)
			{
				voice.currentSoundObject.Stop();
			}
			voice.Destroy();
		}
		voice = prince.room.PlaySound(line, prince.firstChunk);
		voice.requireActiveUpkeep = false;
	}

	public bool isCurrentlyCommunicating()
	{
		return voice != null;
	}

	public virtual string Translate(string s)
	{
		return prince.room.game.rainWorld.inGameTranslator.Translate(s);
	}

	public Vector2 RandomRoomPoint()
	{
		float num = 370f;
		float num2 = 1130f;
		float num3 = 770f;
		float num4 = 150f;
		return new Vector2(num + UnityEngine.Random.value * (num2 - num), num4 + UnityEngine.Random.value * (num3 - num4));
	}

	public bool WillingToInspectItem(PhysicalObject item)
	{
		if (item is Rock || item is Spear || item is Boomerang || item is ScavengerBomb || item is SingularityBomb)
		{
			return false;
		}
		return true;
	}

	public string ReplaceParts(string s)
	{
		return s;
	}

	public void SpecialEvent(string eventName)
	{
		if (eventName == "mark")
		{
			if (currentConversation != null)
			{
				currentConversation.paused = true;
			}
			givingMark = true;
		}
	}
}
