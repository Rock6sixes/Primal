using System;
using System.Collections.Generic;
using HUD;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SSOracleRotBehavior : OracleBehavior, Conversation.IOwnAConversation
{
	public class RMConversation : Conversation
	{
		public SSOracleRotBehavior owner;

		public RMConversation(SSOracleRotBehavior owner, ID id, DialogBox dialogBox)
			: base(owner, id, dialogBox)
		{
			this.owner = owner;
			AddEvents();
		}

		public override void AddEvents()
		{
			if (id == MoreSlugcatsEnums.ConversationID.Pebbles_Pearl_RM)
			{
				LoadEventsFromFile(120);
			}
			else
			{
				if (!(id == MoreSlugcatsEnums.ConversationID.Pebbles_RM_FirstMeeting))
				{
					return;
				}
				if (owner.oracle.room.world.game.IsMoonActive() && !owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesRivuletPostgame)
				{
					if (owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0)
					{
						LoadEventsFromFile(152);
					}
					else
					{
						LoadEventsFromFile(153);
					}
					owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesRivuletPostgame = true;
					return;
				}
				if (owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0)
				{
					owner.gravityDuringDialog = true;
					LoadEventsFromFile(144);
					if (owner.CheckEnergyCellPresence())
					{
						LoadEventsFromFile(145);
						owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.energySeenState = 3;
					}
					else if (owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken)
					{
						LoadEventsFromFile(146);
						owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.energySeenState = 2;
					}
					else
					{
						LoadEventsFromFile(147);
						owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.energySeenState = 1;
					}
					return;
				}
				if (owner.CheckEnergyCellPresence() && owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.energySeenState <= 2)
				{
					if (owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.energySeenState == 1)
					{
						LoadEventsFromFile(148);
					}
					else if (owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.energySeenState == 2)
					{
						LoadEventsFromFile(149);
					}
					owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.energySeenState = 3;
					return;
				}
				if (owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.energySeenState == 1 && !owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesRivuletPostgame)
				{
					if (!owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesEnergyTaken)
					{
						LoadEventsFromFile(150);
						return;
					}
					LoadEventsFromFile(151);
					owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.energySeenState = 3;
					return;
				}
				float value = UnityEngine.Random.value;
				if (owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad > 5 && value < 0.8f)
				{
					events.Add(new TextEvent(this, 0, ".  .  .", 30));
					events.Add(new TextEvent(this, 0, owner.Translate("Your company is welcome little creature, but please do not stay."), 30));
				}
				else if (owner.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad < 3 && value < 0.4f)
				{
					events.Add(new TextEvent(this, 0, owner.Translate("Why have you returned? Do you have a death wish? I have nothing<LINE>left to give you... please leave, for your own sake."), 30));
				}
				else if (value < 0.8f)
				{
					events.Add(new TextEvent(this, 0, owner.Translate("You should not be here. This facility is in a constant state of decay.<LINE>Nothing in here is hospitable to the normal definition of life."), 30));
					events.Add(new TextEvent(this, 0, owner.Translate("Please, leave quickly, while you still can."), 60));
				}
				else
				{
					events.Add(new TextEvent(this, 0, owner.Translate("I used to be furious at the idea of creatures crawling through my superstructure.<LINE>However, I am actually grateful for this rare opportunity to not be alone."), 30));
					events.Add(new TextEvent(this, 0, owner.Translate("However, you really shouldn't stay. It's too dangerous."), 60));
				}
			}
		}
	}

	public Player bestPlayer;

	public int[] playersLastDeadState;

	public bool readyForDeadTalk;

	public bool needToClearConversation;

	public bool didFirstMeetingTalk;

	public EnergyCell energyCellInPlayerGrasp;

	public bool hasNoticedPlayer;

	public float crawlCounter;

	public bool holdKnees = true;

	public bool conversationAdded;

	public PhysicalObject holdingObject;

	public List<EntityID> pickedUpItemsThisRealization;

	public int dontHoldKnees;

	public bool gravityDuringDialog;

	public EnergyCell energyCell;

	public bool deadTalk;

	public HalcyonPearl halcyon;

	public RMConversation conversation;

	public int conversationCooldown;

	public bool seenHalcyonOnce;

	public bool restartConversationAfterCurrentDialoge;

	public bool ateNeuron;

	public bool tolleratedHit;

	public float Crawl => Mathf.Cos(crawlCounter * (float)Math.PI * 2f);

	public float CrawlSpeed => 0.5f + 0.5f * Mathf.Sin(crawlCounter * (float)Math.PI * 2f);

	public override Vector2 OracleGetToPos => new Vector2(1493f, 784f);

	public override Vector2 GetToDir
	{
		get
		{
			if (InSitPosition)
			{
				return new Vector2(0f, 1f);
			}
			return Custom.DirVec(oracle.firstChunk.pos, OracleGetToPos);
		}
	}

	public override bool EyesClosed
	{
		get
		{
			if (oracle.health != 0f)
			{
				if (!hasNoticedPlayer && InSitPosition)
				{
					return holdingObject == null;
				}
				return false;
			}
			return true;
		}
	}

	public bool InSitPosition
	{
		get
		{
			if (oracle.room.GetTilePosition(oracle.firstChunk.pos).x > 72 && oracle.room.GetTilePosition(oracle.firstChunk.pos).x < 75)
			{
				return oracle.room.GetTile(new Vector2(oracle.firstChunk.pos.x, oracle.firstChunk.pos.y - 20f)).Solid;
			}
			return false;
		}
	}

	public new DialogBox dialogBox
	{
		get
		{
			if (oracle.room.game.cameras[0].hud.dialogBox == null)
			{
				oracle.room.game.cameras[0].hud.InitDialogBox();
			}
			return oracle.room.game.cameras[0].hud.dialogBox;
		}
	}

	public bool FocusedOnHalcyon
	{
		get
		{
			if (halcyon != null && !halcyon.Carried && (dialogBox == null || dialogBox.messages.Count == 0) && !restartConversationAfterCurrentDialoge && conversation == null && halcyon.firstChunk.pos.x > oracle.arm.cornerPositions[0].x - 40f && halcyon.firstChunk.pos.x < oracle.arm.cornerPositions[1].x + 40f && halcyon.firstChunk.pos.y > oracle.arm.cornerPositions[2].y - 40f)
			{
				return halcyon.firstChunk.pos.y < oracle.arm.cornerPositions[0].y + 40f;
			}
			return false;
		}
	}

	public RainWorld rainWorld => oracle.room.game.rainWorld;

	public SSOracleRotBehavior(Oracle oracle)
		: base(oracle)
	{
		pickedUpItemsThisRealization = new List<EntityID>();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (playersLastDeadState == null)
		{
			playersLastDeadState = new int[oracle.room.game.Players.Count];
			for (int i = 0; i < oracle.room.game.Players.Count; i++)
			{
				playersLastDeadState[i] = -1;
			}
		}
		bestPlayer = null;
		float num = 0f;
		Player player = null;
		bool flag = false;
		bool flag2 = false;
		energyCellInPlayerGrasp = null;
		for (int j = 0; j < oracle.room.game.Players.Count; j++)
		{
			Player player2 = oracle.room.game.Players[j].realizedCreature as Player;
			if (oracle.room.game.Players[j].realizedCreature != null && oracle.room.game.Players[j].realizedCreature.room == oracle.room)
			{
				float num2 = Vector2.Distance(oracle.firstChunk.pos, player2.mainBodyChunk.pos);
				if (player == null || num2 < num)
				{
					player = player2;
					num = num2;
				}
				if (!player2.dead)
				{
					flag = true;
				}
				if ((player2.grasps[0] != null && player2.grasps[0].grabbed is OracleSwarmer && (player2.grasps[0].grabbed as OracleSwarmer).BitesLeft != 3) || (player2.grasps[1] != null && player2.grasps[1].grabbed is OracleSwarmer && (player2.grasps[1].grabbed as OracleSwarmer).BitesLeft != 3))
				{
					flag2 = true;
				}
				for (int k = 0; k < player2.grasps.Length; k++)
				{
					if (player2.grasps[k] != null && player2.grasps[k].grabbed is EnergyCell)
					{
						energyCellInPlayerGrasp = player2.grasps[k].grabbed as EnergyCell;
					}
				}
			}
			if (player2.dead && playersLastDeadState[j] == 0)
			{
				readyForDeadTalk = true;
				bestPlayer = player2;
			}
			playersLastDeadState[j] = (player2.dead ? 1 : 0);
		}
		if (player != null && !readyForDeadTalk)
		{
			bestPlayer = player;
		}
		oracle.health = 5f;
		halcyon = null;
		if (conversation == null && (dialogBox == null || dialogBox.messages.Count == 0))
		{
			conversationCooldown++;
		}
		else
		{
			conversationCooldown = 0;
		}
		if (conversation != null)
		{
			conversation.Update();
		}
		if (bestPlayer != null && bestPlayer.room != oracle.room && conversation != null)
		{
			conversation.paused = true;
		}
		if (needToClearConversation && conversationCooldown >= 35)
		{
			needToClearConversation = false;
		}
		if (dontHoldKnees > 0)
		{
			dontHoldKnees--;
		}
		if (!oracle.Consious)
		{
			return;
		}
		if (conversation != null && !restartConversationAfterCurrentDialoge && !conversation.paused && conversation.slatedForDeletion)
		{
			Custom.Log("Conversation cleanup");
			conversation.Destroy();
			conversation = null;
		}
		if (conversation != null && restartConversationAfterCurrentDialoge && conversation.paused && dialogBox.messages.Count == 0 && bestPlayer != null)
		{
			Custom.Log("Restarted conversation");
			conversation.paused = false;
			restartConversationAfterCurrentDialoge = false;
			conversation.RestartCurrent();
		}
		for (int l = 0; l < oracle.room.physicalObjects.Length; l++)
		{
			for (int m = 0; m < oracle.room.physicalObjects[l].Count; m++)
			{
				if (oracle.room.physicalObjects[l][m] is HalcyonPearl)
				{
					halcyon = oracle.room.physicalObjects[l][m] as HalcyonPearl;
					break;
				}
			}
			if (halcyon != null)
			{
				break;
			}
		}
		if (halcyon != null && halcyon.room == oracle.room)
		{
			seenHalcyonOnce = true;
			oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen = false;
		}
		if (halcyon == null || (halcyon.room != oracle.room && !oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen && seenHalcyonOnce))
		{
			oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen = true;
			if (bestPlayer != null)
			{
				StoleHalcyon();
			}
		}
		if (!ateNeuron && hasNoticedPlayer && flag2 && oracle.room.game.cameras[0].currentCameraPosition == 3)
		{
			AteNeuron();
		}
		RoomSettings.RoomEffect effect = oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG);
		if (effect != null)
		{
			if (dialogBox.messages.Count == 0)
			{
				gravityDuringDialog = false;
				effect.amount = Mathf.Lerp(effect.amount, 0.76f, 0.02f);
			}
			else if (gravityDuringDialog)
			{
				effect.amount = Mathf.Lerp(effect.amount, 0.01f, 0.02f);
			}
		}
		if (energyCell != null && dialogBox != null && dialogBox.ShowingAMessage)
		{
			energyCell.KeepOff();
		}
		if (!hasNoticedPlayer)
		{
			if (InSitPosition)
			{
				lookPoint = oracle.firstChunk.pos + new Vector2(145f, -45f);
			}
			else
			{
				lookPoint = OracleGetToPos;
			}
			if (bestPlayer != null && bestPlayer.room == oracle.room && bestPlayer.mainBodyChunk.pos.x > oracle.arm.cornerPositions[0].x + 20f && bestPlayer.mainBodyChunk.pos.x < oracle.arm.cornerPositions[1].x - 20f && bestPlayer.mainBodyChunk.pos.y < 845f && bestPlayer.mainBodyChunk.pos.y > oracle.arm.cornerPositions[2].y + 20f)
			{
				hasNoticedPlayer = true;
				if (!didFirstMeetingTalk)
				{
					TalkToNoticedPlayer();
					didFirstMeetingTalk = true;
				}
				oracle.firstChunk.vel += Custom.DegToVec(45f) * 3f;
				oracle.bodyChunks[1].vel += Custom.DegToVec(-90f) * 2f;
			}
		}
		else if (readyForDeadTalk && !needToClearConversation)
		{
			if (bestPlayer != null)
			{
				lookPoint = bestPlayer.firstChunk.pos;
			}
			TalkToDeadPlayer();
		}
		if (FocusedOnHalcyon)
		{
			halcyon.hoverPos = oracle.firstChunk.pos + new Vector2(40f, 20f);
			lookPoint = halcyon.firstChunk.pos;
		}
		else if (halcyon != null)
		{
			halcyon.hoverPos = null;
		}
		if (!flag && (dialogBox == null || !dialogBox.ShowingAMessage))
		{
			hasNoticedPlayer = false;
		}
		if (holdingObject != null)
		{
			if (!oracle.Consious || holdingObject.grabbedBy.Count > 0)
			{
				holdingObject = null;
			}
			else
			{
				holdingObject.firstChunk.MoveFromOutsideMyUpdate(eu, oracle.firstChunk.pos + new Vector2(-18f, -7f));
				holdingObject.firstChunk.vel *= 0f;
			}
		}
		if ((dialogBox == null || dialogBox.messages.Count == 0) && conversation == null && halcyon != null && halcyon.Carried && oracle.room.game.IsStorySession && !oracle.room.game.GetStorySession.saveState.miscWorldSaveData.discussedHalcyon && conversationCooldown > 120 && !restartConversationAfterCurrentDialoge)
		{
			DiscussHalcyon();
		}
		if (InSitPosition)
		{
			if (holdingObject == null && dontHoldKnees < 1 && UnityEngine.Random.value < 0.025f && (bestPlayer == null || !Custom.DistLess(oracle.firstChunk.pos, bestPlayer.DangerPos, 50f)) && oracle.health >= 1f)
			{
				holdKnees = true;
			}
		}
		else
		{
			BodyChunk firstChunk = oracle.firstChunk;
			firstChunk.vel.x = firstChunk.vel.x + ((oracle.firstChunk.pos.x >= OracleGetToPos.x) ? (-1f) : 1f) * 0.6f * CrawlSpeed;
			if (oracle.firstChunk.ContactPoint.x != 0)
			{
				oracle.firstChunk.vel.y = Mathf.Lerp(oracle.firstChunk.vel.y, 1.2f, 0.5f) + 1.2f;
			}
			if (oracle.bodyChunks[1].ContactPoint.x != 0)
			{
				oracle.firstChunk.vel.y = Mathf.Lerp(oracle.firstChunk.vel.y, 1.2f, 0.5f) + 1.2f;
			}
			if (bestPlayer != null && !Custom.DistLess(oracle.firstChunk.pos, bestPlayer.DangerPos, 50f) && (oracle.bodyChunks[1].pos.y > 140f || bestPlayer.DangerPos.x < oracle.firstChunk.pos.x || Mathf.Abs(oracle.firstChunk.pos.x - oracle.firstChunk.lastPos.x) > 2f))
			{
				crawlCounter += 0.04f;
			}
			holdKnees = false;
		}
		if (oracle.arm.joints[2].pos.y < 140f)
		{
			oracle.arm.joints[2].pos.y = 140f;
			oracle.arm.joints[2].vel.y = Mathf.Abs(oracle.arm.joints[1].vel.y) * 0.2f;
		}
		oracle.WeightedPush(0, 1, new Vector2(0f, 1f), 4f * Mathf.InverseLerp(60f, 20f, Mathf.Abs(OracleGetToPos.x - oracle.firstChunk.pos.x)));
	}

	public void AirVoice(SoundID line)
	{
		if (voice != null)
		{
			if (voice.currentSoundObject != null)
			{
				voice.currentSoundObject.Stop();
			}
			voice.Destroy();
		}
		voice = oracle.room.PlaySound(line, oracle.firstChunk);
		voice.requireActiveUpkeep = false;
	}

	public virtual void GrabObject(PhysicalObject obj)
	{
		bool flag = true;
		int num = 0;
		while (flag && num < pickedUpItemsThisRealization.Count)
		{
			if (obj.abstractPhysicalObject.ID == pickedUpItemsThisRealization[num])
			{
				flag = false;
			}
			num++;
		}
		if (flag)
		{
			pickedUpItemsThisRealization.Add(obj.abstractPhysicalObject.ID);
		}
		if (obj.graphicsModule != null)
		{
			obj.graphicsModule.BringSpritesToFront();
		}
		if (obj is IDrawable)
		{
			for (int i = 0; i < oracle.abstractPhysicalObject.world.game.cameras.Length; i++)
			{
				oracle.abstractPhysicalObject.world.game.cameras[i].MoveObjectToContainer(obj as IDrawable, null);
			}
		}
		holdingObject = obj;
	}

	public bool CheckEnergyCellPresence()
	{
		if (energyCellInPlayerGrasp != null)
		{
			energyCell = energyCellInPlayerGrasp;
			return true;
		}
		for (int i = 0; i < oracle.room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < oracle.room.physicalObjects[i].Count; j++)
			{
				if (oracle.room.physicalObjects[i][j] is EnergyCell)
				{
					energyCell = oracle.room.physicalObjects[i][j] as EnergyCell;
					return true;
				}
			}
		}
		return false;
	}

	public void TalkToNoticedPlayer()
	{
		InitateConversation(MoreSlugcatsEnums.ConversationID.Pebbles_RM_FirstMeeting);
		oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad++;
	}

	public void TalkToDeadPlayer()
	{
		if (readyForDeadTalk && oracle.room.ViewedByAnyCamera(oracle.firstChunk.pos, 0f))
		{
			float value = UnityEngine.Random.value;
			if (value <= 0.33f)
			{
				dialogBox.Interrupt(Translate("..."), 60);
				dialogBox.NewMessage(Translate("Well then..."), 60);
				dialogBox.NewMessage(Translate("Another ordeal to add to my running tally of bad luck..."), 60);
			}
			else if (value <= 0.67f)
			{
				dialogBox.Interrupt(Translate("..."), 60);
				dialogBox.NewMessage(Translate("How did it even manage to do that?..."), 60);
				dialogBox.NewMessage(Translate("What a dumb creature..."), 60);
			}
			else
			{
				dialogBox.Interrupt(Translate("..."), 60);
				dialogBox.NewMessage(Translate("That can't be good..."), 60);
			}
			readyForDeadTalk = false;
			needToClearConversation = true;
		}
	}

	public void DiscussHalcyon()
	{
		oracle.room.game.rainWorld.progression.miscProgressionData.SetPebblesPearlDeciphered(MoreSlugcatsEnums.DataPearlType.RM);
		if (oracle.room.game.IsStorySession)
		{
			oracle.room.game.GetStorySession.saveState.miscWorldSaveData.discussedHalcyon = true;
		}
		InitateConversation(MoreSlugcatsEnums.ConversationID.Pebbles_Pearl_RM);
	}

	public void InitateConversation(Conversation.ID convoId)
	{
		if (conversation != null)
		{
			conversation.Interrupt("...", 0);
			conversation.Destroy();
		}
		conversation = new RMConversation(this, convoId, dialogBox);
	}

	public new string ReplaceParts(string s)
	{
		return s;
	}

	public new void SpecialEvent(string eventName)
	{
	}

	public void StoleHalcyon()
	{
		if (conversation != null)
		{
			conversation.paused = true;
			restartConversationAfterCurrentDialoge = true;
			Custom.Log("Paused dialog because halcyon stolen!");
		}
		dialogBox.Interrupt(Translate("Wait, please don't leave with that!"), 60);
	}

	public void AteNeuron()
	{
		ateNeuron = true;
		if (conversation != null)
		{
			conversation.paused = true;
			restartConversationAfterCurrentDialoge = true;
			Custom.Log("Paused dialog because neuron eaten!");
		}
		dialogBox.Interrupt(Translate("I would appreciate if you would not eat those. My umbilical will keep me conscious, but<LINE>every neuron lost is a piece of me lost as well..."), 60);
	}
}
