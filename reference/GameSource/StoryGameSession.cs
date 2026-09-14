using System.Collections.Generic;
using System.Linq;
using Expedition;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class StoryGameSession : GameSession
{
	public SaveState saveState;

	public bool lastEverMetMoon;

	public bool lastEverMetPebbles;

	public SlugcatStats.Name saveStateNumber;

	public WorldCoordinate? karmaFlowerMapPos;

	public PlayerSessionRecord[] playerSessionRecords;

	public new float deltaTimer;

	public SlugcatStats[] characterStatsJollyplayer;

	public List<AbstractPhysicalObject> pendingWarpPointTransferObjects = new List<AbstractPhysicalObject>();

	public List<EntityID> importantWarpPointTransferedEntities = new List<EntityID>();

	public Dictionary<string, List<int>> warpedPlacedObjects = new Dictionary<string, List<int>>();

	public List<PhysicalObject> pendingWarpObjectsToRestoreGravityTo = new List<PhysicalObject>();

	public List<float> pendingWarpObjectsOriginalGravities = new List<float>();

	public string pendingWarpPointTransferId;

	public int pendingWarpPointSedateTime;

	public float rippleLevelAtStartOfCycle;

	public float minimumRippleLevelAtStartOfCycle;

	public float maximumRippleLevelAtStartOfCycle;

	public bool princeEncounteredThisCycle;

	public bool pendingSentientRotInfectionFromWarp;

	public bool insufficientTerraceEncounterThisCycle;

	public List<string> spinningTopWarpsLeadingToRippleScreen = new List<string>();

	public List<string> voidWeaverEncountersThisCycle = new List<string>();

	public bool finalWarpSequenceStarted;

	public int finalWarpSequenceCounter;

	public int lastVoidWeaverEncounters;

	public int warpsTraversedThisCycle;

	public int warpTraversalsLeftUntilFullWarpFatigue
	{
		get
		{
			if (saveState.miscWorldSaveData.hasVoidWeaverAbility || (game.world != null && Region.HasWarpFatigueResistance(game.world.name)))
			{
				return 9999;
			}
			return Mathf.Max(0, (int)saveState.deathPersistentSaveData.maximumRippleLevel + warpFatigueDecayLength - warpsTraversedThisCycle);
		}
	}

	public int warpFatigueDecayLength
	{
		get
		{
			if (saveState.miscWorldSaveData.hasVoidWeaverAbility || (game.world != null && Region.HasWarpFatigueResistance(game.world.name)))
			{
				return 9999;
			}
			if (saveState.deathPersistentSaveData.maximumRippleLevel < 2.5f)
			{
				return 2;
			}
			if (saveState.deathPersistentSaveData.maximumRippleLevel < 5f)
			{
				return 3;
			}
			return 4;
		}
	}

	public bool RedIsOutOfCycles
	{
		get
		{
			if (saveStateNumber == SlugcatStats.Name.Red && saveState.cycleNumber >= RedsIllness.RedsCycles(saveState.RedExtraCycles))
			{
				return !Custom.rainWorld.ExpeditionMode;
			}
			return false;
		}
	}

	public int slugPupMaxCount
	{
		get
		{
			if (!ModManager.MSC)
			{
				return 0;
			}
			if (game.setupValues.slugPupsMax > -1)
			{
				Custom.LogImportant("Slugpup OVERRIDE limits", game.setupValues.slugPupsMax.ToString());
				return game.setupValues.slugPupsMax;
			}
			if (saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
			{
				return 1000;
			}
			if ((saveState.progression.miscProgressionData.beaten_Gourmand_Full || global::MoreSlugcats.MoreSlugcats.chtUnlockSlugpups.Value) && (saveState.saveStateNumber == SlugcatStats.Name.White || saveState.saveStateNumber == SlugcatStats.Name.Red || saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
			{
				return 2;
			}
			return 0;
		}
	}

	public StoryGameSession(SlugcatStats.Name saveStateNumber, RainWorldGame game)
		: base(game)
	{
		this.saveStateNumber = saveStateNumber;
		RainWorld.lastActiveSaveSlot = saveStateNumber;
		playerSessionRecords = new PlayerSessionRecord[(this != null) ? game.StoryPlayerCount : 4];
		saveState = game.rainWorld.progression.GetOrInitiateSaveState(saveStateNumber, game, game.manager.menuSetup, !ModManager.MSC || (!game.wasAnArtificerDream && !game.manager.rainWorld.safariMode));
		saveState.deathPersistentSaveData.karma = Custom.IntClamp(saveState.deathPersistentSaveData.karma, 0, saveState.deathPersistentSaveData.karmaCap);
		saveState.deathPersistentSaveData.rippleLevel = Mathf.Clamp(saveState.deathPersistentSaveData.rippleLevel, saveState.deathPersistentSaveData.minimumRippleLevel, saveState.deathPersistentSaveData.maximumRippleLevel);
		characterStats = new SlugcatStats(saveStateNumber, saveState.malnourished);
		if (saveState.creatureCommunitiesString != null)
		{
			creatureCommunities.FromString(saveState.creatureCommunitiesString);
			difficulty = Custom.LerpMap(saveState.deathPersistentSaveData.howWellIsPlayerDoing, -1f, 1f, -1f, 1f, 2f);
			Custom.LogImportant("DYNAMIC DIFFICULTY:", difficulty.ToString());
		}
		else
		{
			creatureCommunities.LoadDefaultCommunityAlignments(saveStateNumber);
		}
		lastEverMetMoon = saveState.miscWorldSaveData.EverMetMoon;
		lastEverMetPebbles = saveState.miscWorldSaveData.SSaiConversationsHad > 0;
		if (ModManager.Watcher && saveStateNumber == WatcherEnums.SlugcatStatsName.Watcher)
		{
			lastVoidWeaverEncounters = saveState.miscWorldSaveData.numberOfVoidWeaverEncounters;
			rippleLevelAtStartOfCycle = saveState.deathPersistentSaveData.rippleLevel;
			minimumRippleLevelAtStartOfCycle = saveState.deathPersistentSaveData.minimumRippleLevel;
			maximumRippleLevelAtStartOfCycle = saveState.deathPersistentSaveData.maximumRippleLevel;
		}
		if (ModManager.MSC && game.setupValues.randomStart)
		{
			saveState.progression.SaveWorldStateAndProgression(malnourished: false);
		}
		if (ModManager.Expedition && game.rainWorld.ExpeditionMode)
		{
			saveState.deathPersistentSaveData.karma = Custom.IntClamp(saveState.deathPersistentSaveData.karma, 0, 4);
			saveState.deathPersistentSaveData.karmaCap = 4;
		}
		if (ModManager.MSC && saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear && saveState.miscWorldSaveData.SSaiConversationsHad == 0 && !saveState.deathPersistentSaveData.chatlogsRead.SequenceEqual(saveState.deathPersistentSaveData.prePebChatlogsRead))
		{
			saveState.deathPersistentSaveData.chatlogsRead = new List<ChatlogData.ChatlogID>(saveState.deathPersistentSaveData.prePebChatlogsRead);
		}
	}

	public void PlaceKarmaFlowerOnDeathSpot()
	{
		Creature realizedCreature = Players[0].realizedCreature;
		if (ModManager.CoopAvailable && realizedCreature == null)
		{
			for (int i = 0; i < game.Players.Count; i++)
			{
				if (game.Players[i].realizedCreature != null && game.Players[i].realizedCreature.dead)
				{
					realizedCreature = game.Players[i].realizedCreature;
					break;
				}
			}
		}
		if (realizedCreature != null && (realizedCreature as Player).PlaceKarmaFlower)
		{
			saveState.deathPersistentSaveData.karmaFlowerPosition = (realizedCreature as Player).karmaFlowerGrowPos;
			if (ModManager.Expedition && saveState.progression.rainWorld.ExpeditionMode)
			{
				ExpeditionGame.tempKarmaPos = (realizedCreature as Player).karmaFlowerGrowPos;
			}
		}
		if (RedIsOutOfCycles)
		{
			game.manager.rainWorld.progression.miscProgressionData.redsFlower = (realizedCreature as Player).karmaFlowerGrowPos;
		}
	}

	public override void AddPlayer(AbstractCreature player)
	{
		base.AddPlayer(player);
		playerSessionRecords[(player.state as PlayerState).playerNumber] = new PlayerSessionRecord((player.state as PlayerState).playerNumber);
		if (!game.world.singleRoomWorld)
		{
			playerSessionRecords[0].wokeUpInRegion = game.world.region.name;
		}
	}

	public void CreateJollySlugStats(bool m, bool malnourishedByCreature = false, int playerNumber = 0)
	{
		if (game.Players.Count == 0)
		{
			JollyCustom.Log("[JOLLY] NO PLAYERS IN SESSION!!");
			return;
		}
		if (characterStatsJollyplayer == null || characterStatsJollyplayer.Length != 4)
		{
			characterStatsJollyplayer = new SlugcatStats[4];
		}
		PlayerState playerState = game.Players[0].state as PlayerState;
		SlugcatStats slugcatStats = new SlugcatStats(saveState.saveStateNumber, m);
		for (int i = 0; i < game.world.game.Players.Count; i++)
		{
			playerState = game.Players[i].state as PlayerState;
			if (malnourishedByCreature && playerState.playerNumber != playerNumber)
			{
				continue;
			}
			SlugcatStats.Name playerClass = game.rainWorld.options.jollyPlayerOptionsArray[playerState.playerNumber].PlayerClass;
			if (playerClass == null)
			{
				playerClass = saveState.saveStateNumber;
				JollyCustom.Log($"Using savelot stats for p [{i}]: {playerClass} ...");
			}
			bool flag = false;
			bool malnourishedByCreature2 = false;
			bool malnourished = m;
			if (characterStatsJollyplayer[playerState.playerNumber] != null)
			{
				flag = characterStatsJollyplayer[playerState.playerNumber].malnourished;
				malnourishedByCreature2 = characterStatsJollyplayer[playerState.playerNumber].malnourishedByCreature;
				if (malnourishedByCreature && !m && flag)
				{
					malnourished = true;
				}
			}
			characterStatsJollyplayer[playerState.playerNumber] = new SlugcatStats(playerClass, malnourished);
			if (malnourishedByCreature)
			{
				characterStatsJollyplayer[playerState.playerNumber].malnourished = flag;
				characterStatsJollyplayer[playerState.playerNumber].malnourishedByCreature = m;
			}
			else
			{
				characterStatsJollyplayer[playerState.playerNumber].malnourished = m;
				characterStatsJollyplayer[playerState.playerNumber].malnourishedByCreature = malnourishedByCreature2;
				if (!m)
				{
					characterStatsJollyplayer[playerState.playerNumber].malnourishedByCreature = false;
				}
			}
			if (!characterStatsJollyplayer[playerState.playerNumber].malnourished)
			{
				characterStatsJollyplayer[playerState.playerNumber].foodToHibernate = SlugcatStats.SlugcatFoodMeter(slugcatStats.name).y;
			}
			else
			{
				characterStatsJollyplayer[playerState.playerNumber].foodToHibernate = slugcatStats.maxFood;
			}
			characterStatsJollyplayer[playerState.playerNumber].maxFood = slugcatStats.maxFood;
			characterStatsJollyplayer[playerState.playerNumber].bodyWeightFac = slugcatStats.bodyWeightFac;
		}
	}

	public void TimeTick(float dt)
	{
		if (dt >= 10f)
		{
			return;
		}
		deltaTimer += dt * 40f;
		if (deltaTimer >= 1f)
		{
			int num = Mathf.FloorToInt(deltaTimer);
			if (game.cameras[0].hud != null)
			{
				if (game.cameras[0].hud.textPrompt.gameOverMode)
				{
					if (!Players[0].state.dead || (ModManager.CoopAvailable && game.AlivePlayers.Count > 0))
					{
						playerSessionRecords[0].playerGrabbedTime += num;
					}
				}
				else if (!game.cameras[0].voidSeaMode)
				{
					playerSessionRecords[0].time += num;
				}
			}
			deltaTimer -= num;
		}
		SpeedRunTimer.CampaignTimeTracker campaignTimeTracker = SpeedRunTimer.GetCampaignTimeTracker(saveStateNumber);
		if (campaignTimeTracker != null && game.cameras[0].hud != null)
		{
			campaignTimeTracker.UndeterminedFreeTime += SpeedRunTimer.GetTimerTickIncrement(game, dt);
		}
	}

	public void SetRandomSeedToCycleSeed(int modifier)
	{
		int num = (saveState.totTime + saveState.cycleNumber * saveStateNumber.Index + modifier) * 10000;
		Random.InitState(num);
		Random.InitState(num + Random.Range(1000, 9000));
	}

	public bool AddNewPersistentTracker(AbstractPhysicalObject newObject, World world)
	{
		for (int i = 0; i < saveState.objectTrackers.Count; i++)
		{
			AbstractPhysicalObject abstractPhysicalObject = saveState.objectTrackers[i].obj;
			if (abstractPhysicalObject == null)
			{
				if (saveState.objectTrackers[i].objRepresentation.Contains("<oA>"))
				{
					abstractPhysicalObject = SaveState.AbstractPhysicalObjectFromString(world, saveState.objectTrackers[i].objRepresentation);
				}
				else if (saveState.objectTrackers[i].objRepresentation.Contains("<cA>"))
				{
					abstractPhysicalObject = SaveState.AbstractCreatureFromString(world, saveState.objectTrackers[i].objRepresentation, onlyInCurrentRegion: false);
				}
			}
			if (PersistentObjectTracker.CompatibleWithTracker(newObject, abstractPhysicalObject))
			{
				if (saveState.objectTrackers[i].obj == null)
				{
					saveState.objectTrackers[i].LinkObjectToTracker(newObject);
				}
				return false;
			}
		}
		PersistentObjectTracker persistentObjectTracker = new PersistentObjectTracker(newObject);
		saveState.objectTrackers.Add(persistentObjectTracker);
		if (game.cameras[0].hud != null)
		{
			game.cameras[0].hud.map.addTracker(persistentObjectTracker);
		}
		return true;
	}

	public void RemovePersistentTracker(AbstractPhysicalObject obj)
	{
		PersistentObjectTracker persistentObjectTracker = null;
		for (int i = 0; i < saveState.objectTrackers.Count; i++)
		{
			if (saveState.objectTrackers[i].obj != null && saveState.objectTrackers[i].CompatibleWithTracker(obj))
			{
				persistentObjectTracker = saveState.objectTrackers[i];
				break;
			}
		}
		if (persistentObjectTracker != null)
		{
			saveState.objectTrackers.Remove(persistentObjectTracker);
			if (game.cameras[0].hud != null)
			{
				game.cameras[0].hud.map.removeTracker(persistentObjectTracker);
			}
		}
	}

	public void AppendTimeOnCycleEnd(bool deathOrGhost)
	{
		SpeedRunTimer.CampaignTimeTracker campaignTimeTracker = SpeedRunTimer.GetCampaignTimeTracker(saveStateNumber);
		if (campaignTimeTracker != null && deathOrGhost)
		{
			campaignTimeTracker.ConvertUndeterminedToLostTime();
		}
		RainWorld.lockGameTimer = true;
		int num = saveState.deathPersistentSaveData.deathTime + saveState.totTime;
		if (deathOrGhost)
		{
			saveState.deathPersistentSaveData.deathTime += playerSessionRecords[0].time / 40 + playerSessionRecords[0].playerGrabbedTime / 40;
			playerSessionRecords[0].playerGrabbedTime = 0;
		}
		else
		{
			saveState.deathPersistentSaveData.deathTime += playerSessionRecords[0].playerGrabbedTime / 40;
			saveState.totTime += playerSessionRecords[0].time / 40;
			playerSessionRecords[0].playerGrabbedTime = 0;
		}
		int num2 = saveState.deathPersistentSaveData.deathTime + saveState.totTime - num;
		Custom.rainWorld.options.timeSinceLastSaveCopy += num2;
		if (Custom.rainWorld.options.timeSinceLastSaveCopy > 1800)
		{
			saveState.progression.CreateCopyOfSaves(userCreated: false);
			Custom.rainWorld.options.timeSinceLastSaveCopy = 0;
		}
		playerSessionRecords[0].time = 0;
	}
}
