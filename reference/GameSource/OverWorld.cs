using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using HUD;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class OverWorld
{
	public class SpecialWarpType : ExtEnum<SpecialWarpType>
	{
		public static SpecialWarpType WARP_VS_HR = new SpecialWarpType("WARP_VS_HR", register: true);

		public static SpecialWarpType WARP_SINGLEROOM = new SpecialWarpType("WARP_SINGLEROOM", register: true);

		public static SpecialWarpType WARP_WARPPOINT = new SpecialWarpType("WARP_WARPPOINT", register: true);

		public SpecialWarpType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public string FIRSTROOM;

	public RainWorldGame game;

	public World activeWorld;

	public WorldLoader worldLoader;

	public WorldLoader warpWorldLoader;

	public bool readyForWarp;

	public RegionGate reportBackToGate;

	public Region[] regions;

	public SpecialWarpType currentSpecialWarp;

	public ISpecialWarp specialWarpCallback;

	public string singleRoomWorldWarpGoal;

	public WarpPoint.WarpPointData specialWarpPointGoal;

	public bool warpingPreload;

	public bool worldLoadedFromWarp;

	public WarpPoint.WarpPointData warpData;

	public SlugcatStats.Timeline PlayerTimelinePosition
	{
		get
		{
			if (!game.IsStorySession)
			{
				return null;
			}
			return game.TimelinePoint;
		}
	}

	public SlugcatStats.Name PlayerCharacterNumber
	{
		get
		{
			if (!game.IsStorySession)
			{
				return null;
			}
			return game.StoryCharacter;
		}
	}

	public void WarpUpdate()
	{
	}

	public OverWorld(RainWorldGame game)
	{
		this.game = game;
		if (game.overWorld == null)
		{
			game.overWorld = this;
		}
		regions = Region.LoadAllRegions(PlayerTimelinePosition, game);
		LoadFirstWorld();
	}

	public void LoadFirstWorld()
	{
		string text = "SU_C04";
		bool flag = false;
		string path = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "setup.txt").ToLowerInvariant();
		bool flag2 = false;
		bool flag3 = false;
		if (game.IsArenaSession)
		{
			flag = true;
			text = ((!ModManager.ChallengeModule || !(game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge)) ? game.GetArenaGameSession.arenaSitting.GetCurrentLevel : game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.arena);
		}
		else
		{
			if ((game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Dev || ModManager.DevTools) && File.Exists(path))
			{
				string[] array = File.ReadAllLines(path);
				text = "";
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (text2.Contains(":"))
					{
						string[] array3 = Regex.Split(text2, ":");
						if (array3[0].Trim() == "level")
						{
							text = array3[1].Trim();
							break;
						}
					}
				}
				flag = !game.setupValues.world;
				flag2 = text != "";
			}
			if (!flag2)
			{
				if (game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.RegionSelect || game.manager.menuSetup.FastTravelInitCondition)
				{
					text = game.manager.menuSetup.regionSelectRoom;
					flag = false;
				}
				else
				{
					int num = -1;
					if (game.rainWorld.safariMode)
					{
						for (int j = 0; j < regions.Length; j++)
						{
							if (regions[j].name == game.rainWorld.safariRegion)
							{
								num = j;
								break;
							}
						}
					}
					if (num != -1 && regions[num].regionParams.safariStartRoom != null)
					{
						text = regions[num].regionParams.safariStartRoom;
					}
					else
					{
						text = (game.session as StoryGameSession).saveState.GetSaveStateDenToUse();
						flag = false;
						if ((game.session as StoryGameSession).saveState.warpPointTargetAfterWarpPointSave != null)
						{
							flag3 = true;
						}
					}
				}
			}
		}
		if (game.startingRoom != "" && !flag2)
		{
			text = game.startingRoom;
		}
		string text3 = Regex.Split(text, "_")[0];
		if (!ModManager.MSC || game.manager.artificerDreamNumber == -1)
		{
			if (!flag)
			{
				bool flag4 = false;
				if (Directory.Exists(AssetManager.ResolveDirectory("World" + Path.DirectorySeparatorChar + text3)))
				{
					flag4 = true;
				}
				else if (Regex.Split(text, "_").Length > 2 && Directory.Exists(AssetManager.ResolveDirectory("World" + Path.DirectorySeparatorChar + Regex.Split(text, "_")[1])))
				{
					text3 = Regex.Split(text, "_")[1];
					flag4 = true;
				}
				if (!flag4)
				{
					flag = true;
				}
				if (ModManager.MSC && game.IsStorySession && PlayerTimelinePosition == SlugcatStats.Timeline.Spear && text == "GATE_OE_SU" && (game.session as StoryGameSession).saveState.cycleNumber == 0)
				{
					text3 = "SU";
				}
			}
		}
		else if (ModManager.MSC)
		{
			text = DreamsState.ArtificerDreamRooms(game.manager.artificerDreamNumber);
			flag = true;
		}
		if (flag3)
		{
			WarpPoint.WarpPointData warpPointTargetAfterWarpPointSave = (game.session as StoryGameSession).saveState.warpPointTargetAfterWarpPointSave;
			PlacedObject placedObject = new PlacedObject(PlacedObject.Type.WarpPoint, warpPointTargetAfterWarpPointSave);
			WarpPoint warpPoint = new WarpPoint(null, placedObject);
			InitiateSpecialWarp_WarpPoint(warpPoint, warpPointTargetAfterWarpPointSave, useNormalWarpLoader: true);
			game.cameras[0].WarpMoveCameraPrecast(warpPointTargetAfterWarpPointSave.destRoom, warpPointTargetAfterWarpPointSave.destCam);
			if (game.cameras[0].warpPointTimer == null)
			{
				game.cameras[0].warpPointTimer = new WarpPoint.WarpPointTimer(warpPoint.activateAnimationTime * 2f, warpPoint);
				game.cameras[0].warpPointTimer.MoveToSecondHalf();
				game.cameras[0].BlankWarpPointHoldFrame();
			}
			while (!worldLoader.Finished)
			{
				worldLoader.Update();
				Thread.Sleep(1);
			}
			World world = worldLoader.ReturnWorld();
			activeWorld = world;
			for (int k = 0; k < (game.session as StoryGameSession).saveState.transferObjectsAfterWarpPointSave.Count; k++)
			{
				(game.session as StoryGameSession).pendingWarpPointTransferObjects.Add((game.session as StoryGameSession).saveState.transferObjectsAfterWarpPointSave[k]);
			}
			for (int l = 0; l < (game.session as StoryGameSession).saveState.transferCreaturesAfterWarpPointSave.Count; l++)
			{
				AbstractCreature item = SaveState.AbstractCreatureFromString(activeWorld, (game.session as StoryGameSession).saveState.transferCreaturesAfterWarpPointSave[l], onlyInCurrentRegion: false, new WorldCoordinate(activeWorld.offScreenDen.index, -1, -1, -1));
				(game.session as StoryGameSession).pendingWarpPointTransferObjects.Add(item);
			}
			for (int m = 0; m < (game.session as StoryGameSession).saveState.importantTransferEntitiesAfterWarpPointSave.Count; m++)
			{
				(game.session as StoryGameSession).importantWarpPointTransferedEntities.Add((game.session as StoryGameSession).saveState.importantTransferEntitiesAfterWarpPointSave[m]);
			}
			(game.session as StoryGameSession).warpsTraversedThisCycle = (game.session as StoryGameSession).saveState.preserveWarpFatigueAfterWarpPointSave;
			(game.session as StoryGameSession).saveState.transferObjectsAfterWarpPointSave.Clear();
			(game.session as StoryGameSession).saveState.transferCreaturesAfterWarpPointSave.Clear();
			(game.session as StoryGameSession).saveState.importantTransferEntitiesAfterWarpPointSave.Clear();
			(game.session as StoryGameSession).saveState.preserveWarpFatigueAfterWarpPointSave = 0;
		}
		else if (flag)
		{
			LoadWorld(text, PlayerCharacterNumber, PlayerTimelinePosition, singleRoomWorld: true);
		}
		else
		{
			LoadWorld(text3, PlayerCharacterNumber, PlayerTimelinePosition, singleRoomWorld: false);
		}
		FIRSTROOM = text;
	}

	public void Update()
	{
		if (worldLoader != null)
		{
			worldLoader.Update();
			if (worldLoader.Finished)
			{
				WorldLoaded(worldLoadedFromWarp);
			}
		}
		if (warpingPreload && Region.RegionReadyToWarp)
		{
			regions = Region.WarpRegions;
			warpingPreload = false;
			if (warpWorldLoader != null)
			{
				warpWorldLoader.ReturnWorld()?.regionState?.AdaptRegionStateToWorld(-1, -1);
			}
			warpWorldLoader = new WorldLoader(game, PlayerCharacterNumber, game.IsStorySession ? warpData.destTimeline : null, singleRoomWorld: false, warpData.RegionString?.ToUpperInvariant(), GetRegion(warpData.RegionString), game.setupValues);
			warpWorldLoader.NextActivity();
		}
		if (warpWorldLoader != null)
		{
			warpWorldLoader.Update();
			if (warpWorldLoader.Finished && readyForWarp)
			{
				readyForWarp = false;
				worldLoader = warpWorldLoader;
				WorldLoaded(warpUsed: true);
				warpWorldLoader = null;
			}
		}
		else if (readyForWarp)
		{
			readyForWarp = false;
			WorldLoaded(warpUsed: true);
		}
		WarpUpdate();
	}

	[Obsolete("Use timeline parameter function instead.")]
	public void LoadWorld(string worldName, SlugcatStats.Name playerCharacterNumber, bool singleRoomWorld)
	{
		LoadWorld(worldName, playerCharacterNumber, SlugcatStats.SlugcatToTimeline(playerCharacterNumber), singleRoomWorld);
	}

	public void LoadWorld(string worldName, SlugcatStats.Name playerCharacterNumber, SlugcatStats.Timeline time, bool singleRoomWorld)
	{
		worldLoadedFromWarp = false;
		WorldLoader worldLoader = new WorldLoader(game, playerCharacterNumber, time, singleRoomWorld, worldName, GetRegion(worldName, singleRoomWorld), game.setupValues);
		worldLoader.NextActivity();
		while (!worldLoader.Finished)
		{
			worldLoader.Update();
			Thread.Sleep(1);
		}
		World world = worldLoader.ReturnWorld();
		activeWorld = world;
	}

	public void SwitchWorlds(AbstractRoom gateRoom)
	{
		for (int i = 0; i < gateRoom.realizedRoom.updateList.Count; i++)
		{
			if (gateRoom.realizedRoom.updateList[i] is RegionGate)
			{
				(gateRoom.realizedRoom.updateList[i] as RegionGate).OPENCLOSE();
			}
		}
	}

	public void GateRequestsSwitchInitiation(RegionGate reportBackToGate)
	{
		this.reportBackToGate = reportBackToGate;
		AbstractRoom abstractRoom = reportBackToGate.room.abstractRoom;
		Custom.Log("Switch Worlds");
		Custom.Log("Gate:", abstractRoom.name, abstractRoom.index.ToString());
		string name = activeWorld.name;
		name = Region.GetVanillaEquivalentRegionAcronym(name);
		string[] array = Regex.Split(abstractRoom.name, "_");
		string baseAcronym = "ERROR!";
		if (array.Length == 3)
		{
			for (int i = 1; i < 3; i++)
			{
				if (array[i] != name)
				{
					baseAcronym = array[i];
					break;
				}
			}
		}
		baseAcronym = Region.GetProperRegionAcronym(game.IsStorySession ? game.TimelinePoint : null, baseAcronym);
		Custom.Log("Old world:", name);
		Custom.Log("New world:", baseAcronym);
		if (baseAcronym == "ERROR!")
		{
			return;
		}
		if (baseAcronym == "GW")
		{
			game.session.creatureCommunities.scavengerShyness = 0f;
		}
		if (ModManager.MSC && game.IsStorySession && WinState.TrackerAllowedOnSlugcat(MoreSlugcatsEnums.EndgameID.Nomad, game.GetStorySession.saveStateNumber))
		{
			Region region = GetRegion(name);
			WinState.ListTracker listTracker = game.GetStorySession.saveState.deathPersistentSaveData.winState.GetTracker(MoreSlugcatsEnums.EndgameID.Nomad, addIfMissing: true) as WinState.ListTracker;
			if (!listTracker.GoalAlreadyFullfilled)
			{
				Custom.Log("Journey list before gate:", listTracker.myList.Count.ToString());
				if (listTracker.myLastList.Count > listTracker.myList.Count)
				{
					Custom.Log("Stale journey max progress cleared");
					listTracker.myLastList.Clear();
				}
				if (listTracker.myList.Count == 0 || listTracker.myList[listTracker.myList.Count - 1] != GetRegion(baseAcronym).regionNumber)
				{
					Custom.Log("Journey progress updated with", region.regionNumber.ToString());
					listTracker.myList.Add(region.regionNumber);
				}
				else
				{
					Custom.Log("Journey is backtracking", listTracker.myList[listTracker.myList.Count - 1].ToString());
				}
				Custom.Log("Journey list:", listTracker.myList.Count.ToString());
				Custom.Log("Old Journey list:", listTracker.myLastList.Count.ToString());
			}
		}
		worldLoader = new WorldLoader(game, PlayerCharacterNumber, PlayerTimelinePosition, singleRoomWorld: false, baseAcronym, GetRegion(baseAcronym), game.setupValues);
		worldLoader.NextActivity();
	}

	public void WorldLoaded(bool warpUsed = false)
	{
		Custom.Log("New World loaded");
		World world = activeWorld;
		World world2 = ((worldLoader == null) ? activeWorld : worldLoader.ReturnWorld());
		AbstractRoom abstractRoom = null;
		AbstractRoom abstractRoom2 = null;
		if (reportBackToGate == null)
		{
			if (specialWarpCallback != null && specialWarpCallback.getSourceRoom() != null)
			{
				abstractRoom = specialWarpCallback.getSourceRoom().abstractRoom;
			}
			if (currentSpecialWarp == SpecialWarpType.WARP_VS_HR)
			{
				abstractRoom2 = world2.GetAbstractRoom("HR_C01");
			}
			if (currentSpecialWarp == SpecialWarpType.WARP_SINGLEROOM)
			{
				abstractRoom2 = world2.GetAbstractRoom(singleRoomWorldWarpGoal);
			}
			if (currentSpecialWarp == SpecialWarpType.WARP_WARPPOINT)
			{
				abstractRoom2 = world2.GetAbstractRoom(specialWarpPointGoal.destRoom);
			}
			activeWorld = world2;
			if (game.roomRealizer != null)
			{
				game.roomRealizer = new RoomRealizer(game.roomRealizer.followCreature, world2);
			}
			abstractRoom2.RealizeRoom(world2, game);
			int abstractNode = -1;
			for (int i = 0; i < abstractRoom2.nodes.Length; i++)
			{
				if (abstractRoom2.nodes[i].type == AbstractRoomNode.Type.Exit && i < abstractRoom2.connections.Length && abstractRoom2.connections[i] > -1)
				{
					abstractNode = i;
					break;
				}
			}
			for (int j = 0; j < game.Players.Count; j++)
			{
				if (game.Players[j].realizedCreature == null)
				{
					continue;
				}
				WorldCoordinate worldCoordinate = new WorldCoordinate(abstractRoom2.index, 0, 0, -1);
				if (currentSpecialWarp == SpecialWarpType.WARP_VS_HR)
				{
					worldCoordinate.Tile = new IntVector2(70, 42);
				}
				if (currentSpecialWarp == SpecialWarpType.WARP_WARPPOINT)
				{
					if (!specialWarpPointGoal.destPos.HasValue && abstractRoom != null)
					{
						specialWarpPointGoal.GenerateDestinationLocation(abstractRoom2.realizedRoom, world, abstractRoom);
					}
					worldCoordinate.Tile = new IntVector2((int)(specialWarpPointGoal.destPos.Value.x / 20f), (int)(specialWarpPointGoal.destPos.Value.y / 20f));
				}
				if (abstractRoom2.realizedRoom != null && game.Players[j].realizedCreature.room == abstractRoom2.realizedRoom)
				{
					game.Players[j].pos = worldCoordinate;
				}
				else
				{
					if (game.Players[j].realizedCreature.room != null)
					{
						game.Players[j].realizedCreature.room.RemoveObject(game.Players[j].realizedCreature);
					}
					world.GetAbstractRoom(game.Players[j].pos).RemoveEntity(game.Players[j]);
					game.Players[j].world = world2;
					game.Players[j].pos = worldCoordinate;
					world2.GetAbstractRoom(worldCoordinate).AddEntity(game.Players[j]);
					game.Players[j].realizedCreature.PlaceInRoom(abstractRoom2.realizedRoom);
					if (abstractRoom2.realizedRoom.game.session is StoryGameSession && world2.region != null && !(abstractRoom2.realizedRoom.game.session as StoryGameSession).saveState.regionStates[world2.region.regionNumber].roomsVisited.Contains(abstractRoom2.realizedRoom.abstractRoom.name))
					{
						(abstractRoom2.realizedRoom.game.session as StoryGameSession).saveState.regionStates[world2.region.regionNumber].roomsVisited.Add(abstractRoom2.realizedRoom.abstractRoom.name);
					}
				}
				for (int k = 0; k < game.Players[j].realizedCreature.grasps.Length; k++)
				{
					if (game.Players[j].realizedCreature.grasps[k] != null && game.Players[j].realizedCreature.grasps[k].grabbed != null)
					{
						game.Players[j].realizedCreature.grasps[k].grabbed.abstractPhysicalObject.world = world2;
						game.Players[j].realizedCreature.grasps[k].grabbed.abstractPhysicalObject.pos.room = abstractRoom2.index;
						game.Players[j].realizedCreature.grasps[k].grabbed.abstractPhysicalObject.pos.abstractNode = abstractNode;
					}
				}
			}
			for (int l = 0; l < game.cameras.Length; l++)
			{
				bool flag = currentSpecialWarp == SpecialWarpType.WARP_VS_HR;
				game.cameras[l].virtualMicrophone.AllQuiet();
				if (warpUsed)
				{
					game.cameras[l].WarpMoveCameraActual(abstractRoom2.realizedRoom, -1);
				}
				else
				{
					game.cameras[l].MoveCamera(abstractRoom2.realizedRoom, flag ? 1 : (-1));
				}
			}
		}
		else
		{
			abstractRoom = reportBackToGate.room.abstractRoom;
			abstractRoom2 = world2.GetAbstractRoom(abstractRoom.name);
			if (ModManager.PrecycleModule)
			{
				SyncFloodLevels(abstractRoom, abstractRoom2);
			}
			abstractRoom2.entities = abstractRoom.entities;
			abstractRoom2.creatures = abstractRoom.creatures;
			abstractRoom.realizedRoom.SetAbstractRoom(abstractRoom2);
			abstractRoom2.realizedRoom = abstractRoom.realizedRoom;
			abstractRoom.realizedRoom.world = world2;
			world2.activeRooms.Add(abstractRoom.realizedRoom);
			activeWorld = world2;
			if (game.roomRealizer != null)
			{
				game.roomRealizer = new RoomRealizer(game.roomRealizer.followCreature, world2);
			}
			int abstractNode2 = -1;
			for (int m = 0; m < abstractRoom2.nodes.Length; m++)
			{
				if (abstractRoom2.nodes[m].type == AbstractRoomNode.Type.Exit && m < abstractRoom2.connections.Length && abstractRoom2.connections[m] > -1)
				{
					abstractNode2 = m;
					break;
				}
			}
			for (int n = 0; n < abstractRoom2.entities.Count; n++)
			{
				if (!ShouldEntityBeMovedToNewRegion(abstractRoom2.entities[n]))
				{
					continue;
				}
				abstractRoom.entities[n].world = world2;
				abstractRoom.entities[n].pos.room = abstractRoom2.index;
				abstractRoom.entities[n].pos.abstractNode = abstractNode2;
				abstractRoom2.realizedRoom.aimap.NewWorld(abstractRoom2.index);
				if (abstractRoom.entities[n] is AbstractCreature && (abstractRoom.entities[n] as AbstractCreature).creatureTemplate.AI)
				{
					(abstractRoom.entities[n] as AbstractCreature).abstractAI.NewWorld(world2);
					(abstractRoom.entities[n] as AbstractCreature).InitiateAI();
					(abstractRoom.entities[n] as AbstractCreature).abstractAI.RealAI.NewRoom(abstractRoom2.realizedRoom);
					if ((abstractRoom.entities[n] as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Overseer && ((abstractRoom.entities[n] as AbstractCreature).abstractAI as OverseerAbstractAI).playerGuide)
					{
						KillPlayerGuideInNewWorld(world2, abstractRoom.entities[n] as AbstractCreature);
					}
				}
			}
		}
		for (int num = 0; num < game.Players.Count; num++)
		{
			if (game.Players[num].realizedCreature != null && (game.Players[num].realizedCreature as Player).objectInStomach != null)
			{
				(game.Players[num].realizedCreature as Player).objectInStomach.world = world2;
			}
		}
		for (int num2 = game.shortcuts.transportVessels.Count - 1; num2 >= 0; num2--)
		{
			if (activeWorld.region == null || !activeWorld.region.IsRoomInRegion(game.shortcuts.transportVessels[num2].room.index))
			{
				game.shortcuts.transportVessels.RemoveAt(num2);
			}
		}
		for (int num3 = game.shortcuts.betweenRoomsWaitingLobby.Count - 1; num3 >= 0; num3--)
		{
			if (activeWorld.region == null || !activeWorld.region.IsRoomInRegion(game.shortcuts.betweenRoomsWaitingLobby[num3].room.index))
			{
				game.shortcuts.betweenRoomsWaitingLobby.RemoveAt(num3);
			}
		}
		for (int num4 = game.shortcuts.borderTravelVessels.Count - 1; num4 >= 0; num4--)
		{
			if (activeWorld.region == null || !activeWorld.region.IsRoomInRegion(game.shortcuts.borderTravelVessels[num4].room.index))
			{
				game.shortcuts.borderTravelVessels.RemoveAt(num4);
			}
		}
		bool flag2 = false;
		if (reportBackToGate == null)
		{
			if (specialWarpCallback != null)
			{
				specialWarpCallback.NewWorldLoaded(abstractRoom2.realizedRoom);
			}
			specialWarpCallback = null;
			flag2 = true;
		}
		else
		{
			reportBackToGate.NewWorldLoaded(abstractRoom2.realizedRoom);
			reportBackToGate = null;
		}
		worldLoader = null;
		for (int num5 = 0; num5 < game.cameras.Length; num5++)
		{
			if (game.cameras[num5].hud != null)
			{
				game.cameras[num5].hud.ResetMap(new Map.MapData(world2, game.rainWorld));
				game.cameras[num5].dayNightNeedsRefresh = true;
				if (game.cameras[num5].hud.textPrompt.subregionTracker != null)
				{
					game.cameras[num5].hud.textPrompt.subregionTracker.lastShownRegion = 0;
				}
			}
		}
		if (world.name != world2.name)
		{
			if (!flag2)
			{
				if (world.regionState != null)
				{
					world.regionState.AdaptRegionStateToWorld(-1, abstractRoom2.index);
					world.regionState.gatesPassedThrough[world.GetAbstractRoom(abstractRoom.name).gateIndex] = true;
				}
				if (world2.regionState != null)
				{
					world2.regionState.gatesPassedThrough[world2.GetAbstractRoom(abstractRoom.name).gateIndex] = true;
				}
			}
			else if (world.regionState != null)
			{
				world.regionState.AdaptRegionStateToWorld(-1, -1);
			}
			if (world.regionState != null)
			{
				world.regionState.world = null;
			}
		}
		else if (!flag2 && world2.regionState != null)
		{
			world2.regionState.gatesPassedThrough[world2.GetAbstractRoom(abstractRoom.name).gateIndex] = true;
		}
		if (ModManager.MSC)
		{
			world2.SpawnPupNPCs();
		}
		world2.rainCycle.baseCycleLength = world.rainCycle.baseCycleLength;
		world2.rainCycle.cycleLength = world.rainCycle.cycleLength;
		world2.rainCycle.timer = world.rainCycle.timer;
		world2.rainCycle.duskPalette = world.rainCycle.duskPalette;
		world2.rainCycle.nightPalette = world.rainCycle.nightPalette;
		world2.rainCycle.dayNightCounter = world.rainCycle.dayNightCounter;
		if (ModManager.PrecycleModule)
		{
			if (world.rainCycle.timer == 0)
			{
				world2.rainCycle.preTimer = world.rainCycle.preTimer;
				world2.rainCycle.maxPreTimer = world.rainCycle.maxPreTimer;
			}
			else
			{
				world2.rainCycle.preTimer = 0;
				world2.rainCycle.maxPreTimer = 0;
			}
		}
		if (world.name != world2.name)
		{
			foreach (Room activeRoom in world.activeRooms)
			{
				if (activeRoom.world == world)
				{
					activeRoom.Unloaded();
				}
			}
		}
		if (ModManager.MMF && !warpUsed)
		{
			GC.Collect();
		}
	}

	public bool ShouldEntityBeMovedToNewRegion(AbstractWorldEntity ent)
	{
		return true;
	}

	public void KillPlayerGuideInNewWorld(World newWorld, AbstractCreature oldGuide)
	{
		Custom.Log("looking for player guide in new world");
		for (int i = 0; i < newWorld.NumberOfRooms; i++)
		{
			for (int j = 0; j < newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures.Count; j++)
			{
				if (newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures[j].creatureTemplate.type == CreatureTemplate.Type.Overseer && newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures[j] != oldGuide && (newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures[j].abstractAI as OverseerAbstractAI).playerGuide)
				{
					AbstractCreature abstractCreature = newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).creatures[j];
					if (abstractCreature.realizedCreature != null)
					{
						abstractCreature.realizedCreature.Destroy();
					}
					abstractCreature.Destroy();
					newWorld.GetAbstractRoom(i + newWorld.firstRoomIndex).RemoveEntity(abstractCreature);
					Custom.LogWarning("spawned player guide killed in offscreenden!");
					return;
				}
			}
		}
	}

	public Region GetRegion(AbstractRoom room)
	{
		string[] array = Regex.Split(room.name, "_");
		if (array.Length == 2)
		{
			return GetRegion(array[0]);
		}
		return null;
	}

	public Region GetRegion(string rName)
	{
		return GetRegion(rName, singleRoomWorld: false);
	}

	public Region GetRegion(string rName, bool singleRoomWorld)
	{
		for (int i = 0; i < regions.Length; i++)
		{
			if (regions[i].name.ToLowerInvariant() == rName.ToLowerInvariant())
			{
				return regions[i];
			}
		}
		if (singleRoomWorld)
		{
			if (File.Exists(WorldLoader.FindRoomFile(rName, includeRootDirectory: false, "_Properties.txt", showWarning: false)))
			{
				return new Region(rName, 0, -1, game, null);
			}
			return null;
		}
		Custom.LogWarning("no region by name:", rName);
		return null;
	}

	public void InitiateSpecialWarp(SpecialWarpType warp, ISpecialWarp callback)
	{
		worldLoadedFromWarp = false;
		reportBackToGate = null;
		currentSpecialWarp = warp;
		specialWarpCallback = callback;
		Custom.Log($"Switch Worlds Special! {warp}");
		if (warp == SpecialWarpType.WARP_VS_HR)
		{
			worldLoader = new WorldLoader(game, PlayerCharacterNumber, PlayerTimelinePosition, singleRoomWorld: false, "HR", GetRegion("HR"), game.setupValues);
			worldLoader.NextActivity();
		}
	}

	public void InitiateSpecialWarp_SingleRoom(ISpecialWarp callback, string roomName)
	{
		worldLoadedFromWarp = false;
		reportBackToGate = null;
		currentSpecialWarp = SpecialWarpType.WARP_SINGLEROOM;
		specialWarpCallback = callback;
		singleRoomWorldWarpGoal = roomName;
		Custom.Log("Switch Worlds Special to single room", singleRoomWorldWarpGoal);
		worldLoader = new WorldLoader(game, PlayerCharacterNumber, PlayerTimelinePosition, singleRoomWorld: true, singleRoomWorldWarpGoal, null, game.setupValues);
		worldLoader.NextActivity();
	}

	public void InitiateSpecialWarp_WarpPoint(ISpecialWarp callback, WarpPoint.WarpPointData warpData, bool useNormalWarpLoader)
	{
		reportBackToGate = null;
		if (!useNormalWarpLoader && warpWorldLoader != null && this.warpData != null && this.warpData.destRoom == warpData.destRoom)
		{
			return;
		}
		if (warpData.destTimeline == null)
		{
			warpData.destTimeline = game.TimelinePoint;
		}
		currentSpecialWarp = SpecialWarpType.WARP_WARPPOINT;
		specialWarpCallback = callback;
		if (warpData.rippleWarp && activeWorld != null && Region.IsShatteredTerraceRegion(activeWorld.name) && (warpData.destRoom == null || warpData.RegionString?.ToLowerInvariant() == "waua"))
		{
			if (game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow && !game.rainWorld.progression.miscProgressionData.beaten_Watcher_SpinningTop)
			{
				warpData.destRoom = "WAUA_BATH";
				warpData.destPos = new Vector2(1500f, 225f);
			}
			else if (game.GetStorySession.saveState.miscWorldSaveData.visitedShopRoom)
			{
				warpData.destRoom = "WAUA_B02B";
				warpData.destPos = new Vector2(550f, 350f);
			}
			else
			{
				warpData.destRoom = "WAUA_E01";
				warpData.destPos = new Vector2(770f, 390f);
			}
			warpData.destCam = WarpPoint.GetDestCam(warpData);
		}
		if (warpData.RegionString == null || warpData.destRoom == null)
		{
			bool flag = warpData.RegionString?.ToLowerInvariant() == "wora";
			bool flag2 = false;
			if (Region.IsVanillaSentientRotRegion(activeWorld.name))
			{
				flag2 = true;
				float num = (float)game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters + 2f;
				if (game.GetStorySession.saveState.deathPersistentSaveData.maximumRippleLevel >= num)
				{
					warpData.RegionString = "WORA";
				}
				else if (game.GetStorySession.saveState.deathPersistentSaveData.maximumRippleLevel >= 5f)
				{
					int highestPrinceConversationSeen = game.GetStorySession.saveState.miscWorldSaveData.highestPrinceConversationSeen;
					Conversation.ID iD = PrinceBehavior.PrinceConversation.TargetConversation(highestPrinceConversationSeen, game.GetStorySession.saveState.miscWorldSaveData.regionsInfectedBySentientRotSpread.Count);
					if (iD != null && PrinceBehavior.PrinceConversation.PrinceConversationToId(iD) > highestPrinceConversationSeen)
					{
						warpData.RegionString = "WORA";
					}
				}
			}
			if (warpData.rippleWarp)
			{
				warpData.destRoom = "WRSA_L01";
				warpData.destPos = new Vector2(1520f, 3590f);
				warpData.destCam = WarpPoint.GetDestCam(warpData);
			}
			else
			{
				string oldRoom = null;
				if (callback.getSourceRoom() != null)
				{
					AbstractRoom abstractRoom = callback.getSourceRoom().abstractRoom;
					oldRoom = abstractRoom.name;
					if (abstractRoom.world.game.GetStorySession.pendingSentientRotInfectionFromWarp || (abstractRoom.realizedRoom != null && abstractRoom.realizedRoom.roomSettings.GetEffectAmount(WatcherEnums.RoomEffectType.SentientRotInfection) > 0f))
					{
						flag2 = true;
					}
				}
				bool flag3 = warpData.RegionString?.ToLowerInvariant() == "wora";
				string chosenRoom = WarpPoint.ChooseDynamicWarpTarget(activeWorld, oldRoom, warpData.RegionString, badWarp: false, flag3 ? flag : flag2, playerCreated: false);
				WarpPoint.WarpPointData warpPointData = WarpPoint.CreateOverrideData(activeWorld, oldRoom, chosenRoom, null, limitedUse: false, playerCreated: false);
				warpData.destRoom = warpPointData.destRoom;
				warpData.destPos = warpPointData.destPos;
				warpData.destCam = warpPointData.destCam;
			}
		}
		if (warpData.RegionString != null && Region.IsVanillaSentientRotRegion(warpData.RegionString))
		{
			game.GetStorySession.saveState.miscWorldSaveData.numberOfRottedVanillaVisits++;
		}
		specialWarpPointGoal = warpData;
		Custom.Log("Switch Worlds via warp point!", warpData.ToString());
		worldLoadedFromWarp = true;
		bool flag4 = game.IsStorySession && game.GetStorySession.finalWarpSequenceStarted && warpData.RegionString.ToLowerInvariant() == "wrsa";
		if (activeWorld == null || !(warpData.RegionString.ToLowerInvariant() == activeWorld.name.ToLowerInvariant()) || flag4)
		{
			if (useNormalWarpLoader)
			{
				regions = Region.LoadAllRegions(PlayerTimelinePosition, game);
				worldLoader = new WorldLoader(game, PlayerCharacterNumber, PlayerTimelinePosition, singleRoomWorld: false, warpData.RegionString?.ToUpperInvariant(), GetRegion(warpData.RegionString), game.setupValues);
				worldLoader.NextActivity();
			}
			else
			{
				this.warpData = warpData;
				Region.RegionReadyToWarp = false;
				game.rainWorld.StartCoroutine(Region.LoadAllRegionsCoroutine(PlayerTimelinePosition, game));
				warpingPreload = true;
			}
		}
	}

	public void SyncFloodLevels(AbstractRoom oldWorldRoom, AbstractRoom newWorldRoom)
	{
		if (game.globalRain.drainWorldFlood > 0f)
		{
			float y = oldWorldRoom.world.RoomToWorldPos(default(Vector2), oldWorldRoom.index).y;
			float y2 = newWorldRoom.world.RoomToWorldPos(default(Vector2), newWorldRoom.index).y;
			float num = game.globalRain.drainWorldFlood - y;
			game.globalRain.drainWorldFlood = y2 + num;
		}
		else
		{
			game.globalRain.drainWorldFlood = 0f;
		}
		float y3 = oldWorldRoom.world.RoomToWorldPos(default(Vector2), oldWorldRoom.index).y;
		float y4 = newWorldRoom.world.RoomToWorldPos(default(Vector2), newWorldRoom.index).y;
		float num2 = game.globalRain.flood - y3;
		game.globalRain.flood = y4 + num2 + game.globalRain.drainWorldFlood;
	}
}
