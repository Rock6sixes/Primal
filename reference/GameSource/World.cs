using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class World
{
	public struct RoomPath
	{
		public AbstractRoom rm;

		public AbstractRoom prevRm;

		public RoomPath(AbstractRoom rm, AbstractRoom prevRm)
		{
			this.rm = rm;
			this.prevRm = prevRm;
		}
	}

	public abstract class CreatureSpawner
	{
		public int region;

		public int inRegionSpawnerIndex;

		public WorldCoordinate den;

		public bool nightCreature;

		public int SpawnerID => region * 1000 + inRegionSpawnerIndex;

		public CreatureSpawner(int region, int inRegionSpawnerIndex, WorldCoordinate den)
		{
			this.region = region;
			this.inRegionSpawnerIndex = inRegionSpawnerIndex;
			this.den = den;
		}
	}

	public class SimpleSpawner : CreatureSpawner
	{
		public CreatureTemplate.Type creatureType;

		public string spawnDataString;

		public int amount;

		public SimpleSpawner(int region, int inRegionSpawnerIndex, WorldCoordinate den, CreatureTemplate.Type creatureType, string spawnDataString, int amount)
			: base(region, inRegionSpawnerIndex, den)
		{
			this.creatureType = creatureType;
			this.spawnDataString = spawnDataString;
			this.amount = amount;
		}

		public override string ToString()
		{
			return "Simple Spawner " + ((amount == 1) ? "" : ("(" + amount + " creatures)"));
		}
	}

	public class Lineage : CreatureSpawner
	{
		public int[] creatureTypes;

		public float[] progressionChances;

		public string[] spawnData;

		public string denString;

		public Lineage(int region, int inRegionSpawnerIndex, WorldCoordinate den, int[] types, float[] progChances, string[] spwnDt, int conflictNumber)
			: base(region, inRegionSpawnerIndex, den)
		{
			creatureTypes = types;
			progressionChances = progChances;
			spawnData = spwnDt;
			denString = den.SaveToString();
			if (conflictNumber > 0)
			{
				denString = denString + ";" + conflictNumber;
			}
		}

		public CreatureTemplate.Type CurrentType(SaveState saveState)
		{
			if (saveState.regionStates[region] == null)
			{
				Custom.LogWarning("lineage couldn't find region state");
				return null;
			}
			if (!saveState.regionStates[region].lineageCounters.ContainsKey(denString))
			{
				Custom.LogWarning("couldn't find lineage for", denString);
				return null;
			}
			int num = saveState.regionStates[region].lineageCounters[denString];
			if (num < 0 || num >= creatureTypes.Length)
			{
				Custom.LogWarning("lineage progression out of range:", num.ToString(), "/", creatureTypes.Length.ToString());
				return null;
			}
			if (creatureTypes[num] < 0)
			{
				return null;
			}
			return new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(creatureTypes[num]));
		}

		public string CurrentSpawnData(SaveState saveState)
		{
			if (saveState.regionStates[region] == null)
			{
				Custom.LogWarning("lineage couldn't find region state");
				return null;
			}
			if (!saveState.regionStates[region].lineageCounters.ContainsKey(denString))
			{
				Custom.LogWarning("couldn't find lineage for", denString);
				return null;
			}
			int num = saveState.regionStates[region].lineageCounters[denString];
			if (num < 0 || num >= creatureTypes.Length)
			{
				Custom.LogWarning("lineage progression out of range:", num.ToString(), "/", creatureTypes.Length.ToString());
				return null;
			}
			if (creatureTypes[num] < 0)
			{
				return null;
			}
			return spawnData[num];
		}

		public bool LastWasNONE(SaveState saveState)
		{
			if (saveState.regionStates[region] == null)
			{
				Custom.LogWarning("lineage couldn't find region state");
				return false;
			}
			if (!saveState.regionStates[region].lineageCounters.ContainsKey(denString))
			{
				Custom.LogWarning("couldn't find lineage for", denString);
				return false;
			}
			int num = saveState.regionStates[region].lineageCounters[denString];
			if (num < 1)
			{
				return true;
			}
			if (num - 1 >= creatureTypes.Length)
			{
				return false;
			}
			return creatureTypes[num - 1] < 0;
		}

		public void ChanceToProgress(World world)
		{
			if (!(world.game.session is StoryGameSession))
			{
				return;
			}
			if (!(world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters.ContainsKey(denString))
			{
				(world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters[denString] = 0;
			}
			int num = (world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters[denString];
			if (num >= 0 && num < progressionChances.Length && num < progressionChances.Length - 1 && UnityEngine.Random.value < progressionChances[num])
			{
				num++;
				(world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters[denString] = num;
				string text = ExtEnum<CreatureTemplate.Type>.values.GetEntry(creatureTypes[(world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters[denString]]);
				if (text == null)
				{
					text = "NONE";
				}
				Custom.Log($"Lineage {base.SpawnerID} progressed to {text}");
			}
		}

		public override string ToString()
		{
			string text = "LINEAGE ";
			for (int i = 0; i < creatureTypes.Length; i++)
			{
				text = text + new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(creatureTypes[i]))?.ToString() + "-" + progressionChances[i] + ((i < creatureTypes.Length - 1) ? ", " : "");
			}
			return text;
		}
	}

	public interface IMigrationInfluence
	{
		float AttractionValueForCreature(AbstractRoom room, CreatureTemplate.Type tp, float defValue);

		float AttractionValueForCreature(AbstractRoom room, string namedAttr, float defValue);
	}

	public abstract class WorldProcess
	{
		public World world;

		public WorldProcess(World world)
		{
			this.world = world;
		}

		public virtual void Update()
		{
		}
	}

	public static AGLog<World> Log = new AGLog<World>();

	public AbstractRoom offScreenDen;

	public AbstractRoom[] abstractRooms;

	public List<AbstractRoom> sentientRotRoomsInitiated = new List<AbstractRoom>();

	public List<Room> activeRooms;

	public List<Room> unloadingRooms;

	public List<RoomPreparer> loadingRooms;

	public int preProcessingGeneration;

	public int[] swarmRooms;

	public int[] shelters;

	public int[] gates;

	public bool[] brokenShelters;

	public int brokenShelterIndexDueToPrecycle = -1;

	public WorldCoordinate[] sideAccessNodes;

	public WorldCoordinate[] skyAccessNodes;

	public WorldCoordinate[] seaAccessNodes;

	public WorldCoordinate[] regionAccessNodes;

	public Dictionary<CreatureTemplate.Type, string> defaultRoomAttractions;

	public Dictionary<string, string> defaultNamedAttractions;

	public int mostNodesInARoom;

	public string name;

	public bool singleRoomWorld;

	public List<WorldProcess> worldProcesses;

	public FliesWorldAI fliesWorldAI;

	public ScavengersWorldAI scavengersWorldAI;

	public OverseersWorldAI overseersWorldAI;

	public VoidSpawnWorldAI voidSpawnWorldAI;

	public RainCycle rainCycle;

	public Region region;

	public CreatureSpawner[] spawners = new CreatureSpawner[0];

	public Lineage[] lineages = new Lineage[0];

	public List<IMigrationInfluence> migrationInfluences = new List<IMigrationInfluence>();

	public GhostWorldPresence worldGhost;

	public bool logCreatures = true;

	public List<string> DisabledMapRooms;

	public List<int> DisabledMapIndices;

	public List<GhostWorldPresence> spinningTopPresences = new List<GhostWorldPresence>();

	public VoidWeaverPresence weaverPresence;

	public RainWorldGame game { get; set; }

	public int NumberOfRooms => abstractRooms.Length;

	public int RegionNumber
	{
		get
		{
			if (region == null)
			{
				return -1;
			}
			return region.regionNumber;
		}
	}

	public RegionState regionState
	{
		get
		{
			if (game.session is StoryGameSession && !singleRoomWorld)
			{
				return (game.session as StoryGameSession).saveState.regionStates[region.regionNumber];
			}
			return null;
		}
	}

	public int firstRoomIndex
	{
		get
		{
			if (region == null)
			{
				return 0;
			}
			return region.firstRoomIndex;
		}
	}

	public World(RainWorldGame game, Region region, string name, bool singleRoomWorld)
	{
		this.game = game;
		this.region = region;
		this.name = name;
		this.singleRoomWorld = singleRoomWorld;
		activeRooms = new List<Room>();
		unloadingRooms = new List<Room>();
		loadingRooms = new List<RoomPreparer>();
		DisabledMapRooms = new List<string>();
		DisabledMapIndices = new List<int>();
		worldProcesses = new List<WorldProcess>();
		defaultRoomAttractions = new Dictionary<CreatureTemplate.Type, string>();
		defaultNamedAttractions = new Dictionary<string, string>();
		preProcessingGeneration = 20;
		if (game == null)
		{
			return;
		}
		if (game.IsStorySession)
		{
			float minutes = ((!(game.GetStorySession.characterStats.name == SlugcatStats.Name.Yellow) && (!ModManager.MSC || (!(game.GetStorySession.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) && !(game.GetStorySession.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand) && !(game.GetStorySession.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint)))) ? (Mathf.Lerp(game.rainWorld.setup.cycleTimeMin, game.rainWorld.setup.cycleTimeMax, UnityEngine.Random.value) / 60f) : (Mathf.Lerp(game.rainWorld.setup.cycleTimeMin, game.rainWorld.setup.cycleTimeMax, 0.35f + 0.65f * Mathf.Pow(UnityEngine.Random.value, 1.2f)) / 60f));
			if (ModManager.MMF && MMF.cfgNoRandomCycles.Value)
			{
				minutes = (float)game.rainWorld.setup.cycleTimeMax / 60f;
			}
			rainCycle = new RainCycle(this, minutes);
			if (ModManager.TimelineModule && name == "SB")
			{
				rainCycle.filtrationPowerBehavior = new FiltrationPowerController(this);
			}
		}
		else
		{
			rainCycle = new RainCycle(this, game.GetArenaGameSession.rainCycleTimeInMinutes);
		}
	}

	public void AddWorldProcess(WorldProcess process)
	{
		worldProcesses.Add(process);
		if (process is FliesWorldAI)
		{
			fliesWorldAI = process as FliesWorldAI;
		}
		else if (process is ScavengersWorldAI)
		{
			scavengersWorldAI = process as ScavengersWorldAI;
		}
		else if (process is OverseersWorldAI)
		{
			overseersWorldAI = process as OverseersWorldAI;
		}
		else if (process is VoidSpawnWorldAI)
		{
			voidSpawnWorldAI = process as VoidSpawnWorldAI;
		}
	}

	[Obsolete("Use timeline parameter function instead.")]
	public void LoadWorld(SlugcatStats.Name slugcatNumber, List<AbstractRoom> abstractRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
	{
		LoadWorld(SlugcatStats.SlugcatToTimeline(slugcatNumber), abstractRoomsList, swarmRooms, shelters, gates);
	}

	public void LoadWorld(SlugcatStats.Timeline timelinePosition, List<AbstractRoom> abstractRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
	{
		GenerateOffScreenDen(abstractRoomsList.Count);
		abstractRoomsList.Add(offScreenDen);
		abstractRooms = abstractRoomsList.ToArray();
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			abstractRooms[i].world = this;
		}
		this.shelters = shelters;
		brokenShelters = new bool[shelters.Length];
		LoadMapConfig(timelinePosition);
		this.swarmRooms = swarmRooms;
		this.gates = gates;
		mostNodesInARoom = 0;
		for (int j = 0; j < abstractRooms.Length; j++)
		{
			if (abstractRooms[j].nodes.Length > mostNodesInARoom)
			{
				mostNodesInARoom = abstractRooms[j].nodes.Length;
			}
		}
		foreach (string disabledMapRoom in DisabledMapRooms)
		{
			AbstractRoom abstractRoom = GetAbstractRoom(disabledMapRoom);
			if (abstractRoom != null)
			{
				DisabledMapIndices.Add(abstractRoom.index);
			}
		}
		MapBorderConnections();
		if (game != null && !singleRoomWorld && game.session is StoryGameSession)
		{
			if ((game.session as StoryGameSession).saveState.regionStates[region.regionNumber] == null)
			{
				Custom.Log("Creating region state for world:", name);
				(game.session as StoryGameSession).saveState.regionStates[region.regionNumber] = new RegionState((game.session as StoryGameSession).saveState, this);
			}
			else
			{
				(game.session as StoryGameSession).saveState.regionStates[region.regionNumber].world = this;
				Custom.Log("World assigned to existing region state:", name);
			}
			regionState.AdaptWorldToRegionState();
			SpawnGhost();
		}
	}

	public void SpawnGhost()
	{
		if (ModManager.Watcher && game.IsStorySession && game.StoryCharacter == WatcherEnums.SlugcatStatsName.Watcher)
		{
			spinningTopPresences.Clear();
			List<string> list = new List<string>();
			if (!game.rainWorld.safariMode && game.rainWorld.regionSpinningTopRooms.ContainsKey(region.name.ToLowerInvariant()))
			{
				list = game.rainWorld.regionSpinningTopRooms[region.name.ToLowerInvariant()];
				for (int i = 0; i < list.Count; i++)
				{
					string[] array = list[i].Split(':');
					if (array.Length >= 2)
					{
						string room = array[0];
						int num = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						AbstractRoom abstractRoom = GetAbstractRoom(room);
						if (abstractRoom != null && !game.GetStorySession.saveState.deathPersistentSaveData.spinningTopEncounters.Contains(num))
						{
							GhostWorldPresence ghostWorldPresence = new GhostWorldPresence(this, WatcherEnums.GhostID.SpinningTop, num);
							ghostWorldPresence.ghostRoom = abstractRoom;
							spinningTopPresences.Add(ghostWorldPresence);
						}
					}
				}
			}
		}
		if (game.setupValues.ghosts < 0 || !CheckForRegionGhost(game.StoryCharacter, region.name))
		{
			return;
		}
		GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(region.name);
		if ((ModManager.MSC && ((ghostID == MoreSlugcatsEnums.GhostID.SL && game.session is StoryGameSession && game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Saint) || (ghostID == MoreSlugcatsEnums.GhostID.MS && game.session is StoryGameSession && game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Saint))) || ghostID == GhostWorldPresence.GhostID.NoGhost)
		{
			return;
		}
		int num2 = 0;
		if ((game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID))
		{
			num2 = (game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID];
		}
		Custom.Log("Save state properly loaded:", (game.session as StoryGameSession).saveState.loaded.ToString());
		Custom.Log($"GHOST TALKED TO {ghostID} {num2}");
		Custom.Log("Karma:", (game.session as StoryGameSession).saveState.deathPersistentSaveData.karma.ToString());
		bool flag = game.setupValues.ghosts > 0 || GhostWorldPresence.SpawnGhost(ghostID, (game.session as StoryGameSession).saveState.deathPersistentSaveData.karma, (game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap, num2, game.StoryCharacter == SlugcatStats.Name.Red);
		if (ModManager.MSC && (!ModManager.Expedition || (ModManager.Expedition && !game.rainWorld.ExpeditionMode)))
		{
			if (game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer && !flag)
			{
				if (!(game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) || (game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] != 1)
				{
					flag = false;
				}
				else if (((game.session as StoryGameSession).saveState.deathPersistentSaveData.karma == (game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap && (game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma) || (ModManager.Expedition && game.rainWorld.ExpeditionMode && (game.session as StoryGameSession).saveState.deathPersistentSaveData.karma == (game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap))
				{
					flag = true;
				}
			}
			if (game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				flag = false;
				if ((game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) && (game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] == 2)
				{
					flag = false;
				}
				else if ((game.session as StoryGameSession).saveState.cycleNumber > 0)
				{
					flag = true;
				}
			}
		}
		if (game.rainWorld.safariMode)
		{
			flag = false;
		}
		if (flag)
		{
			worldGhost = new GhostWorldPresence(this, ghostID);
			migrationInfluences.Add(worldGhost);
			Custom.Log("Ghost in region");
		}
		else
		{
			Custom.Log("No ghost in region");
		}
	}

	public bool InitiateWeaverPresence(AbstractRoom triggerRoom)
	{
		if (weaverPresence == null && game.IsStorySession && game.GetStorySession.voidWeaverEncountersThisCycle.Count == 0 && region != null && game.rainWorld.regionWeaverRooms.TryGetValue(region.name.ToLowerInvariant(), out var value) && value.Count > 0)
		{
			List<AbstractRoom> targetRooms = value.Select(GetAbstractRoom).ToList();
			return InitiateGeneralWeaverHintTrail(triggerRoom, targetRooms);
		}
		return false;
	}

	public bool InitiateWeaverHintTrailToOpenWarpPoints(AbstractRoom triggerRoom)
	{
		if (weaverPresence == null && game.IsStorySession)
		{
			List<string> list = game.GetStorySession.saveState.RoomsWithWarpsRemainingToBeSealed(includeSpinningTopWarps: true, name);
			if (list.Count > 0)
			{
				List<AbstractRoom> list2 = new List<AbstractRoom>();
				for (int i = 0; i < list.Count; i++)
				{
					AbstractRoom abstractRoom = GetAbstractRoom(list[i]);
					if (abstractRoom != null)
					{
						list2.Add(abstractRoom);
					}
				}
				if (list2.Count > 0)
				{
					return InitiateGeneralWeaverHintTrail(triggerRoom, list2);
				}
			}
		}
		return false;
	}

	public bool InitiateGeneralWeaverHintTrail(AbstractRoom triggerRoom, List<AbstractRoom> targetRooms)
	{
		if (weaverPresence == null && game.IsStorySession)
		{
			List<AbstractRoom> list = FindPathToAnyRoom(triggerRoom, targetRooms);
			if (list == null)
			{
				return false;
			}
			weaverPresence = new VoidWeaverPresence(this, list[list.Count - 1]);
			weaverPresence.presenceRooms.AddRange(list);
			migrationInfluences.Add(weaverPresence);
			return true;
		}
		return false;
	}

	public List<AbstractRoom> FindPathToAnyRoom(AbstractRoom fromRoom, List<AbstractRoom> toRooms)
	{
		Queue<AbstractRoom> queue = new Queue<AbstractRoom>();
		Dictionary<AbstractRoom, AbstractRoom> dictionary = new Dictionary<AbstractRoom, AbstractRoom>();
		queue.Enqueue(fromRoom);
		dictionary.Add(fromRoom, null);
		while (queue.Count > 0)
		{
			AbstractRoom abstractRoom = queue.Dequeue();
			if (toRooms.Contains(abstractRoom))
			{
				AbstractRoom abstractRoom2 = abstractRoom;
				List<AbstractRoom> list = new List<AbstractRoom> { abstractRoom2 };
				AbstractRoom value;
				while (dictionary.TryGetValue(abstractRoom2, out value) && value != null)
				{
					list.Add(value);
					abstractRoom2 = value;
				}
				list.Reverse();
				return list;
			}
			int[] connections = abstractRoom.connections;
			foreach (int room in connections)
			{
				AbstractRoom abstractRoom3 = GetAbstractRoom(room);
				if (abstractRoom3 != null && !dictionary.ContainsKey(abstractRoom3))
				{
					dictionary.Add(abstractRoom3, abstractRoom);
					queue.Enqueue(abstractRoom3);
				}
			}
		}
		return null;
	}

	public void DeactivateWeaverPresence()
	{
		migrationInfluences.Remove(weaverPresence);
		weaverPresence = null;
	}

	public void ActivateRoom(int room)
	{
		ActivateRoom(GetAbstractRoom(room));
	}

	public void ActivateRoom(string room)
	{
		for (int i = 0; i < abstractRooms.Length - 1; i++)
		{
			if (abstractRooms[i].name.ToLowerInvariant() == room.ToLowerInvariant())
			{
				ActivateRoom(abstractRooms[i]);
				break;
			}
		}
	}

	public void ActivateRoom(AbstractRoom abstractRoom)
	{
		if (abstractRoom.realizedRoom == null)
		{
			abstractRoom.RealizeRoom(this, game);
		}
	}

	public AbstractRoom GetSwarmRoom(int swarmRoomIndex)
	{
		return GetAbstractRoom(swarmRooms[swarmRoomIndex]);
	}

	public AbstractRoom GetAbstractRoom(WorldCoordinate coord)
	{
		return GetAbstractRoom(coord.room);
	}

	public AbstractRoom GetAbstractRoom(int room)
	{
		if (room < firstRoomIndex || room >= firstRoomIndex + NumberOfRooms)
		{
			return null;
		}
		return abstractRooms[room - firstRoomIndex];
	}

	public AbstractRoom GetAbstractRoom(string room)
	{
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			if (abstractRooms[i].name.ToLowerInvariant() == room.ToLowerInvariant())
			{
				return abstractRooms[i];
			}
		}
		if (room == "OFFSCREEN")
		{
			return abstractRooms[abstractRooms.Length - 1];
		}
		return null;
	}

	public AbstractRoomNode GetNode(WorldCoordinate c)
	{
		return GetAbstractRoom(c.room).nodes[c.abstractNode];
	}

	public bool VisualContactBetweenWorldCoordinates(WorldCoordinate a, WorldCoordinate b)
	{
		if (a.room != b.room)
		{
			return false;
		}
		if (GetAbstractRoom(a.room).realizedRoom == null)
		{
			return false;
		}
		return GetAbstractRoom(a.room).realizedRoom.VisualContact(GetAbstractRoom(a.room).realizedRoom.MiddleOfTile(a.Tile), GetAbstractRoom(a.room).realizedRoom.MiddleOfTile(b.Tile));
	}

	public int TotalShortCutLengthBetweenTwoConnectedRooms(int room1, int room2)
	{
		return TotalShortCutLengthBetweenTwoConnectedRooms(GetAbstractRoom(room1), GetAbstractRoom(room2));
	}

	public int TotalShortCutLengthBetweenTwoConnectedRooms(AbstractRoom room1, AbstractRoom room2)
	{
		if (room1 != null && room2 != null && room1.ExitIndex(room2.index) != -1 && room2.ExitIndex(room1.index) != -1)
		{
			return room1.nodes[room1.ExitIndex(room2.index)].shortCutLength + room2.nodes[room2.ExitIndex(room1.index)].shortCutLength;
		}
		return -1;
	}

	public bool IsRoomInRegion(int room)
	{
		if (room >= firstRoomIndex)
		{
			return room < firstRoomIndex + NumberOfRooms;
		}
		return false;
	}

	public WorldCoordinate NodeInALeadingToB(int roomA, int roomB)
	{
		return NodeInALeadingToB(GetAbstractRoom(roomA), GetAbstractRoom(roomB));
	}

	public WorldCoordinate NodeInALeadingToB(AbstractRoom roomA, AbstractRoom roomB)
	{
		if (roomA != null && roomB != null && roomA.ExitIndex(roomB.index) != -1)
		{
			return new WorldCoordinate(roomA.index, -1, -1, roomA.ExitIndex(roomB.index));
		}
		return new WorldCoordinate(-1, -1, -1, -1);
	}

	public void MoveQuantifiedCreatureFromAbstractRoom(CreatureTemplate.Type crit, AbstractRoom fromRoom, AbstractRoom toRoom)
	{
		if (fromRoom.realizedRoom != null)
		{
			Custom.LogWarning("TRYING TO USE MoveQuantifiedCreatureFromAbstractRoom FROM REALIZED ROOM!");
		}
		else
		{
			if (fromRoom.NumberOfQuantifiedCreatureInRoom(crit) == 0)
			{
				return;
			}
			int abstractNode = NodeInALeadingToB(fromRoom, toRoom).abstractNode;
			if (fromRoom.NumberOfQuantifiedCreatureInNode(crit, abstractNode) > 0)
			{
				fromRoom.RemoveQuantifiedCreature(abstractNode, crit);
			}
			else
			{
				int num = 0;
				int num2 = -1;
				for (int i = 0; i < fromRoom.nodes.Length; i++)
				{
					if (i != abstractNode && fromRoom.ConnectionPossible(i, abstractNode, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)) && fromRoom.NumberOfQuantifiedCreatureInNode(crit, i) > num)
					{
						num = fromRoom.NumberOfQuantifiedCreatureInNode(crit, i);
						num2 = i;
					}
				}
				if (num2 == -1)
				{
					return;
				}
				fromRoom.RemoveQuantifiedCreature(num2, crit);
			}
			int abstractNode2 = NodeInALeadingToB(toRoom, fromRoom).abstractNode;
			toRoom.AddQuantifiedCreature(abstractNode2, crit);
			if (toRoom.realizedRoom != null)
			{
				new AbstractCreature(this, StaticWorld.GetCreatureTemplate(crit), null, new WorldCoordinate(fromRoom.index, -1, -1, abstractNode), game.GetNewID()).Move(new WorldCoordinate(toRoom.index, -1, -1, abstractNode2));
			}
		}
	}

	public int SideHighwayDistanceBetweenNodes(WorldCoordinate A, WorldCoordinate B)
	{
		return (int)(Mathf.Max(0f, Vector2.Distance(GetAbstractRoom(A).mapPos, GetAbstractRoom(B).mapPos) - 100f) * 0.15f) + 1;
	}

	public int SkyHighwayDistanceBetweenNodes(WorldCoordinate A, WorldCoordinate B)
	{
		return (int)(Mathf.Max(0f, Vector2.Distance(GetAbstractRoom(A).mapPos, GetAbstractRoom(B).mapPos) - 100f) * 0.15f) + 1;
	}

	public int SeaHighwayDistanceBetweenNodes(WorldCoordinate A, WorldCoordinate B)
	{
		return (int)(Mathf.Max(0f, Vector2.Distance(GetAbstractRoom(A).mapPos, GetAbstractRoom(B).mapPos) - 100f) * 0.15f) + 1;
	}

	public int RegionTransportationDistanceBetweenNodes(WorldCoordinate A, WorldCoordinate B)
	{
		if (A.room == B.room || GetAbstractRoom(A).offScreenDen || GetAbstractRoom(B).offScreenDen)
		{
			return 1;
		}
		return (int)(Mathf.Max(0f, Vector2.Distance(GetAbstractRoom(A).mapPos + GetAbstractRoom(A).size.ToVector2() * 0.5f, GetAbstractRoom(B).mapPos + GetAbstractRoom(A).size.ToVector2() * 0.5f) - 100f) * 0.15f) + 1;
	}

	public Vector2 RoomToWorldPos(Vector2 inRoomPos, int roomIndex)
	{
		AbstractRoom abstractRoom = GetAbstractRoom(roomIndex);
		if (abstractRoom == null)
		{
			return Vector2.zero;
		}
		return (abstractRoom.mapPos / 3f + new Vector2(10f, 10f)) * 20f + inRoomPos - new Vector2((float)abstractRoom.size.x * 20f, (float)abstractRoom.size.y * 20f) / 2f;
	}

	public void MapBorderConnections()
	{
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		List<WorldCoordinate> list2 = new List<WorldCoordinate>();
		List<WorldCoordinate> list3 = new List<WorldCoordinate>();
		List<WorldCoordinate> list4 = new List<WorldCoordinate>();
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			for (int j = 0; j < abstractRooms[i].nodes.Length; j++)
			{
				if (!DisabledMapIndices.Contains(i + firstRoomIndex))
				{
					if (abstractRooms[i].nodes[j].type == AbstractRoomNode.Type.SideExit)
					{
						list.Add(new WorldCoordinate(i + firstRoomIndex, -1, -1, j));
					}
					else if (abstractRooms[i].nodes[j].type == AbstractRoomNode.Type.SkyExit)
					{
						list2.Add(new WorldCoordinate(i + firstRoomIndex, -1, -1, j));
					}
					else if (abstractRooms[i].nodes[j].type == AbstractRoomNode.Type.SeaExit)
					{
						list3.Add(new WorldCoordinate(i + firstRoomIndex, -1, -1, j));
					}
					else if (abstractRooms[i].nodes[j].type == AbstractRoomNode.Type.RegionTransportation)
					{
						list4.Add(new WorldCoordinate(i + firstRoomIndex, -1, -1, j));
					}
				}
			}
		}
		sideAccessNodes = list.ToArray();
		skyAccessNodes = list2.ToArray();
		seaAccessNodes = list3.ToArray();
		regionAccessNodes = list4.ToArray();
	}

	[Obsolete("Use timeline parameter function instead.")]
	public void LoadMapConfig(SlugcatStats.Name slugcatNumber)
	{
		LoadMapConfig(SlugcatStats.SlugcatToTimeline(slugcatNumber));
	}

	public void LoadMapConfig(SlugcatStats.Timeline timelinePosition)
	{
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "map_" + WorldLoader.MapNameManipulator(name, game) + ".txt");
		if (timelinePosition != null)
		{
			Custom.Log("-- mapconfig as timeline: " + timelinePosition.value);
			string text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "map_" + WorldLoader.MapNameManipulator(name, game) + "-" + timelinePosition.value + ".txt");
			if (File.Exists(text))
			{
				path = text;
			}
		}
		if (!File.Exists(path))
		{
			return;
		}
		Dictionary<string, Dictionary<int, BezierSpline.Midpoint>> dictionary = new Dictionary<string, Dictionary<int, BezierSpline.Midpoint>>();
		string[] array = File.ReadAllLines(path);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ":"), ": ");
			if (array2.Length != 2)
			{
				continue;
			}
			switch (array2[0])
			{
			case "SpawnMigrationStream":
				if (voidSpawnWorldAI == null)
				{
					voidSpawnWorldAI = new VoidSpawnWorldAI(this);
				}
				voidSpawnWorldAI.worldMigrationStreams.Add(WorldSpawnMigrationStream.Parse(array2[1], this));
				continue;
			case "SpawnMigrationStreamMidpoint":
			{
				string[] array3 = Regex.Split(array2[1], ",");
				string key = array3[0];
				int key2 = int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				string s = string.Join(",", array3.Skip(2));
				if (!dictionary.ContainsKey(key))
				{
					dictionary[key] = new Dictionary<int, BezierSpline.Midpoint>();
				}
				dictionary[key][key2] = BezierSpline.Midpoint.FromString(s);
				continue;
			}
			case "Connection":
				continue;
			}
			for (int j = 0; j < NumberOfRooms; j++)
			{
				if (abstractRooms[j].name.ToLowerInvariant() == array2[0].ToLowerInvariant())
				{
					string[] array4 = Regex.Split(array2[1], "><");
					abstractRooms[j].mapPos.x = float.Parse(array4[0], NumberStyles.Any, CultureInfo.InvariantCulture);
					abstractRooms[j].mapPos.y = float.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					if (Regex.Split(array2[1], "><").Length >= 5)
					{
						abstractRooms[j].layer = int.Parse(array4[4], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					if (Regex.Split(array2[1], "><").Length >= 6)
					{
						abstractRooms[j].subregionName = ((array4[5].Trim() == "") ? null : array4[5]);
						abstractRooms[j].altSubregionName = null;
					}
					break;
				}
			}
		}
		if (voidSpawnWorldAI != null)
		{
			foreach (KeyValuePair<string, Dictionary<int, BezierSpline.Midpoint>> item in dictionary)
			{
				WorldSpawnMigrationStream worldMigrationStream = voidSpawnWorldAI.GetWorldMigrationStream(item.Key);
				if (worldMigrationStream != null)
				{
					worldMigrationStream.spline.midpoints = (from x in item.Value
						orderby x.Key
						select x.Value).ToList();
				}
			}
			foreach (WorldSpawnMigrationStream worldMigrationStream3 in voidSpawnWorldAI.worldMigrationStreams)
			{
				WorldSpawnMigrationStream worldMigrationStream2 = voidSpawnWorldAI.GetWorldMigrationStream(worldMigrationStream3.nextStreamName);
				if (worldMigrationStream2 != null)
				{
					worldMigrationStream3.nextStream = worldMigrationStream2;
				}
			}
		}
		bool flag = false;
		List<string> list = new List<string>();
		list.Add("");
		path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "Properties.txt");
		if (timelinePosition != null)
		{
			string text2 = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "Properties-" + timelinePosition.value + ".txt");
			if (File.Exists(text2))
			{
				path = text2;
				flag = true;
			}
		}
		if (!File.Exists(path))
		{
			return;
		}
		array = File.ReadAllLines(path);
		for (int k = 0; k < array.Length; k++)
		{
			string[] array5 = Regex.Split(Custom.ValidateSpacedDelimiter(array[k], ":"), ": ");
			if (array5.Length == 2 && flag && array5[0] == "Subregion")
			{
				list.Add(array5[1]);
			}
			if (array5.Length != 3)
			{
				continue;
			}
			if (array5[0] == "Room_Attr")
			{
				if (array5[1] == "Default")
				{
					string[] array6 = Regex.Split(array5[2], ",");
					for (int l = 0; l < array6.Length; l++)
					{
						if (array6[l] != "")
						{
							string[] array7 = Regex.Split(array6[l], "-");
							CreatureTemplate.Type type = new CreatureTemplate.Type(array7[0]);
							if (type.Index >= 0)
							{
								defaultRoomAttractions[type] = array7[1];
							}
							else
							{
								defaultNamedAttractions[array7[0]] = array7[1];
							}
						}
					}
					continue;
				}
				for (int m = 0; m < NumberOfRooms; m++)
				{
					if (!(abstractRooms[m].name == array5[1]))
					{
						continue;
					}
					string[] array6 = Regex.Split(array5[2], ",");
					for (int n = 0; n < array6.Length; n++)
					{
						if (!(array6[n] != ""))
						{
							continue;
						}
						string[] array8 = Regex.Split(array6[n], "-");
						CreatureTemplate.Type type2 = new CreatureTemplate.Type(array8[0]);
						if (type2.Index >= 0)
						{
							if (int.TryParse(array8[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
							{
								abstractRooms[m].roomAttractions[type2.Index] = BackwardsCompatibilityRemix.ParseRoomAttraction(result);
							}
							else
							{
								abstractRooms[m].roomAttractions[type2.Index] = new AbstractRoom.CreatureRoomAttraction(array8[1]);
							}
						}
						else
						{
							abstractRooms[m].namedRoomAttractions[array8[0]] = new AbstractRoom.CreatureRoomAttraction(array8[1]);
						}
					}
					break;
				}
			}
			else
			{
				if (!(timelinePosition != null) || !(array5[0] == "Broken Shelters") || !(timelinePosition.ToString() == array5[1].Trim()))
				{
					continue;
				}
				string[] array6 = Regex.Split(Custom.ValidateSpacedDelimiter(array5[2], ","), ", ");
				for (int num = 0; num < array6.Length; num++)
				{
					if (GetAbstractRoom(array6[num]) != null && GetAbstractRoom(array6[num]).shelter)
					{
						Custom.Log($"--timeline position {timelinePosition} has a broken shelter at:{array6[num]} (shelter index {GetAbstractRoom(array6[num]).shelterIndex})");
						brokenShelters[GetAbstractRoom(array6[num]).shelterIndex] = true;
					}
				}
			}
		}
		if (flag)
		{
			string path2 = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "Properties.txt");
			List<string> list2 = new List<string> { "" };
			if (File.Exists(path2))
			{
				string[] array9 = File.ReadAllLines(path);
				for (int num2 = 0; num2 < array9.Length; num2++)
				{
					string[] array10 = Regex.Split(Custom.ValidateSpacedDelimiter(array9[num2], ":"), ": ");
					if (array10.Length == 2 && array10[0] == "Subregion")
					{
						list2.Add(array10[1]);
					}
				}
				for (int num3 = 0; num3 < NumberOfRooms; num3++)
				{
					if (abstractRooms[num3].subregionName == null)
					{
						continue;
					}
					for (int num4 = 1; num4 < list2.Count; num4++)
					{
						if (abstractRooms[num3].subregionName == list2[num4] && num4 < list.Count)
						{
							abstractRooms[num3].altSubregionName = list[num4];
							break;
						}
					}
				}
			}
		}
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		for (int num5 = 0; num5 < NumberOfRooms; num5++)
		{
			if (abstractRooms[num5].mapPos.x - (float)abstractRooms[num5].size.x * 3f * 0.5f < vector.x)
			{
				vector.x = abstractRooms[num5].mapPos.x - (float)abstractRooms[num5].size.x * 3f * 0.5f;
			}
			if (abstractRooms[num5].mapPos.y - (float)abstractRooms[num5].size.y * 3f * 0.5f < vector.y)
			{
				vector.y = abstractRooms[num5].mapPos.y - (float)abstractRooms[num5].size.y * 3f * 0.5f;
			}
		}
		for (int num6 = 0; num6 < NumberOfRooms; num6++)
		{
			abstractRooms[num6].mapPos -= vector;
			if (DisabledMapRooms.Contains(abstractRooms[num6].name))
			{
				for (int num7 = 0; num7 < abstractRooms[num6].roomAttractions.Length; num7++)
				{
					abstractRooms[num6].roomAttractions[num7] = AbstractRoom.CreatureRoomAttraction.Forbidden;
				}
			}
		}
		if (voidSpawnWorldAI == null)
		{
			return;
		}
		foreach (WorldSpawnMigrationStream worldMigrationStream4 in voidSpawnWorldAI.worldMigrationStreams)
		{
			worldMigrationStream4.lowerLeft = vector;
		}
	}

	public void GenerateOffScreenDen(int index)
	{
		offScreenDen = new AbstractRoom("OffScreenDen_" + name, new int[0], firstRoomIndex + index, -1, -1, -1);
		List<AbstractRoomNode> list = new List<AbstractRoomNode>();
		AbstractRoomNode.Type[] array = new AbstractRoomNode.Type[5]
		{
			AbstractRoomNode.Type.Den,
			AbstractRoomNode.Type.RegionTransportation,
			AbstractRoomNode.Type.SideExit,
			AbstractRoomNode.Type.SkyExit,
			AbstractRoomNode.Type.SeaExit
		};
		foreach (AbstractRoomNode.Type type in array)
		{
			AbstractRoomNode item = new AbstractRoomNode(type, 0, 5, type == AbstractRoomNode.Type.SeaExit, 0, 100);
			for (int j = 0; j < item.connectivity.GetLength(0); j++)
			{
				for (int k = 0; k < item.connectivity.GetLength(1); k++)
				{
					for (int l = 0; l < item.connectivity.GetLength(2); l++)
					{
						item.connectivity[j, k, l] = 1;
					}
				}
			}
			list.Add(item);
		}
		offScreenDen.nodes = list.ToArray();
		offScreenDen.offScreenDen = true;
	}

	public CreatureSpawner GetSpawner(EntityID ID)
	{
		if (ID.spawner < 0 || singleRoomWorld)
		{
			return null;
		}
		int num = ID.spawner % 1000;
		int num2 = (ID.spawner - num) / 1000;
		if (spawners.Length < 1 || num2 != region.regionNumber || num < 0 || num >= spawners.Length)
		{
			return null;
		}
		return spawners[num];
	}

	public int RegionNumberOfSpawner(EntityID ID)
	{
		if (ID.spawner < 0)
		{
			return -1;
		}
		int num = ID.spawner % 1000;
		return (ID.spawner - num) / 1000;
	}

	public float GetAttractionValueForRoom(WorldCoordinate coord, AbstractCreature creature)
	{
		if (coord.room < firstRoomIndex || coord.room >= firstRoomIndex + NumberOfRooms)
		{
			return 0f;
		}
		return GetAbstractRoom(coord).AttractionValueForCreature(creature);
	}

	public AbstractRoom.CreatureRoomAttraction GetAttractionForRoom(WorldCoordinate coord, AbstractCreature creature)
	{
		if (coord.room < firstRoomIndex || coord.room >= firstRoomIndex + NumberOfRooms)
		{
			return AbstractRoom.CreatureRoomAttraction.Forbidden;
		}
		return GetAbstractRoom(coord).AttractionForCreature(creature);
	}

	public float GetAttractionValueForRoom(WorldCoordinate coord, CreatureTemplate.Type tp)
	{
		if (coord.room < firstRoomIndex || coord.room >= firstRoomIndex + NumberOfRooms)
		{
			return 0f;
		}
		return GetAbstractRoom(coord).AttractionValueForCreature(tp);
	}

	public AbstractRoom.CreatureRoomAttraction GetAttractionForRoom(WorldCoordinate coord, CreatureTemplate.Type tp)
	{
		if (coord.room < firstRoomIndex || coord.room >= firstRoomIndex + NumberOfRooms)
		{
			return AbstractRoom.CreatureRoomAttraction.Forbidden;
		}
		return GetAbstractRoom(coord).AttractionForCreature(tp);
	}

	public void LogCreatures()
	{
		logCreatures = false;
		List<string> list = new List<string>();
		for (int i = firstRoomIndex; i < firstRoomIndex + NumberOfRooms; i++)
		{
			for (int j = 0; j < GetAbstractRoom(i).creatures.Count; j++)
			{
				list.Add(GetAbstractRoom(i).name + " " + GetAbstractRoom(i).creatures[j].pos.abstractNode + " " + GetAbstractRoom(i).creatures[j].creatureTemplate.type?.ToString() + " ~ " + PrintSpawner(GetAbstractRoom(i).creatures[j].ID.spawner));
			}
			for (int k = 0; k < GetAbstractRoom(i).entitiesInDens.Count; k++)
			{
				if (GetAbstractRoom(i).entitiesInDens[k] is AbstractCreature)
				{
					list.Add(GetAbstractRoom(i).name + " " + GetAbstractRoom(i).entitiesInDens[k].pos.abstractNode + " " + (GetAbstractRoom(i).entitiesInDens[k] as AbstractCreature).creatureTemplate.type?.ToString() + " ~ " + PrintSpawner(GetAbstractRoom(i).entitiesInDens[k].ID.spawner));
				}
			}
		}
		list.Sort();
		using StreamWriter streamWriter = File.CreateText(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "loggedCreatures.txt").ToLowerInvariant());
		for (int l = 0; l < list.Count; l++)
		{
			streamWriter.Write(list[l] + "\r\n");
		}
	}

	public string PrintSpawner(int spawner)
	{
		if (spawner < 0 || spawner >= spawners.Length || spawners[spawner] == null)
		{
			return "no spawner";
		}
		return spawners[spawner].ToString();
	}

	public int SpawnPupNPCs()
	{
		if (game.world.singleRoomWorld || game.rainWorld.safariMode || game.wasAnArtificerDream || !game.IsStorySession || game.GetStorySession.Players.Count == 0)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		AbstractRoom room = game.GetStorySession.Players[0].Room;
		foreach (AbstractCreature creature in room.creatures)
		{
			if (creature.state.alive && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				num++;
				num2++;
			}
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		game.GetStorySession.SetRandomSeedToCycleSeed(region.regionNumber);
		if (UnityEngine.Random.value >= region.regionParams.slugPupSpawnChance && game.StoryCharacter != MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && game.GetStorySession.saveState.forcePupsNextCycle != 1)
		{
			UnityEngine.Random.state = state;
			return num;
		}
		if (num < game.GetStorySession.slugPupMaxCount)
		{
			AbstractRoom abstractRoom = null;
			List<AbstractRoom> list = new List<AbstractRoom>();
			AbstractRoom[] array = abstractRooms;
			foreach (AbstractRoom abstractRoom2 in array)
			{
				if (abstractRoom2 != room && abstractRoom2.shelter && abstractRoom2.name != "SU_S05" && !game.world.DisabledMapRooms.Contains(abstractRoom2.name, StringComparer.InvariantCultureIgnoreCase))
				{
					list.Add(abstractRoom2);
				}
			}
			int num3 = 1;
			if (game.GetStorySession.saveState.forcePupsNextCycle == 1)
			{
				abstractRoom = room;
				num3 = game.GetStorySession.slugPupMaxCount - num;
				game.GetStorySession.saveState.forcePupsNextCycle = 2;
			}
			else
			{
				if (list.Count == 0)
				{
					Custom.LogWarning("No shelters for pup spawns");
					return num;
				}
				if (list.Count == 1)
				{
					Custom.LogWarning("only a SINGLE shelter for pup spawns");
					abstractRoom = list[0];
				}
				else
				{
					abstractRoom = list[UnityEngine.Random.Range(0, list.Count)];
				}
			}
			for (int j = 0; j < num3; j++)
			{
				AbstractCreature abstractCreature = new AbstractCreature(this, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new WorldCoordinate(abstractRoom.index, -1, -1, 0), game.GetNewID());
				abstractRoom.AddEntity(abstractCreature);
				if (abstractRoom.realizedRoom != null)
				{
					abstractCreature.RealizeInRoom();
				}
				(abstractCreature.state as PlayerNPCState).foodInStomach = 1;
				num++;
				Custom.Log($"Created slugpup! {abstractCreature} at {abstractRoom.name} ({abstractRoom.index})");
			}
		}
		UnityEngine.Random.state = state;
		return num2;
	}

	[Obsolete("Use timeline parameter function instead.")]
	public void LoadWorldForFastTravel(SlugcatStats.Name slugcatNumber, List<AbstractRoom> abstractRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
	{
		LoadWorldForFastTravel(SlugcatStats.SlugcatToTimeline(slugcatNumber), abstractRoomsList, swarmRooms, shelters, gates);
	}

	public void LoadWorldForFastTravel(SlugcatStats.Timeline timelinePosition, List<AbstractRoom> abstractRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
	{
		GenerateOffScreenDen(abstractRoomsList.Count);
		abstractRoomsList.Add(offScreenDen);
		abstractRooms = abstractRoomsList.ToArray();
		this.shelters = shelters;
		brokenShelters = new bool[shelters.Length];
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "map_" + WorldLoader.MapNameManipulator(name, game) + ".txt");
		if (timelinePosition != null)
		{
			string text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "map_" + WorldLoader.MapNameManipulator(name, game) + "-" + timelinePosition.value + ".txt");
			if (File.Exists(text))
			{
				path = text;
			}
		}
		if (File.Exists(path))
		{
			List<string> list = new List<string>(File.ReadAllLines(path));
			for (int i = 0; i < NumberOfRooms; i++)
			{
				bool flag = false;
				for (int j = 0; j < list.Count; j++)
				{
					string[] array = Regex.Split(Custom.ValidateSpacedDelimiter(list[j], ":"), ": ");
					if (array.Length == 2 && abstractRooms[i].name == array[0])
					{
						list.RemoveAt(j);
						string[] array2 = Regex.Split(array[1], "><");
						if (array2.Length >= 8)
						{
							abstractRooms[i].size.x = int.Parse(array2[6], NumberStyles.Any, CultureInfo.InvariantCulture);
							abstractRooms[i].size.y = int.Parse(array2[7], NumberStyles.Any, CultureInfo.InvariantCulture);
							flag = true;
						}
						break;
					}
				}
				if (!flag)
				{
					Room room = new Room(null, this, abstractRooms[i]);
					new RoomPreparer(room, loadAiHeatMaps: false, falseBake: false, shortcutsOnly: false);
					abstractRooms[i].size.x = room.TileWidth;
					abstractRooms[i].size.y = room.TileHeight;
				}
			}
		}
		foreach (string disabledMapRoom in DisabledMapRooms)
		{
			AbstractRoom abstractRoom = GetAbstractRoom(disabledMapRoom);
			if (abstractRoom != null)
			{
				DisabledMapIndices.Add(abstractRoom.index);
			}
		}
		LoadMapConfig(timelinePosition);
		this.swarmRooms = swarmRooms;
		this.gates = gates;
		mostNodesInARoom = 0;
	}

	public static bool CheckForRegionGhost(SlugcatStats.Name slugcatIndex, string regionString)
	{
		GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(regionString);
		if (!ModManager.MSC)
		{
			return ghostID != GhostWorldPresence.GhostID.NoGhost;
		}
		if (ghostID != GhostWorldPresence.GhostID.NoGhost && (ghostID != MoreSlugcatsEnums.GhostID.SL || slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Saint))
		{
			if (!(ghostID != MoreSlugcatsEnums.GhostID.MS))
			{
				return slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Saint;
			}
			return true;
		}
		return false;
	}

	public void ToggleCreatureAccessFromCutscene(string roomName, CreatureTemplate.Type CritterType, bool allowAccess)
	{
		AbstractRoom abstractRoom = GetAbstractRoom(roomName);
		for (int i = 0; i < abstractRoom.roomAttractions.Length; i++)
		{
			if (StaticWorld.creatureTemplates[i].type == CreatureTemplate.Type.Scavenger || (ModManager.DLCShared && StaticWorld.creatureTemplates[i].type == DLCSharedEnums.CreatureTemplateType.ScavengerElite))
			{
				abstractRoom.roomAttractions[i] = (allowAccess ? AbstractRoom.CreatureRoomAttraction.Neutral : AbstractRoom.CreatureRoomAttraction.Forbidden);
			}
		}
	}

	public void LogDebugStats()
	{
		_ = abstractRooms.Length;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			AbstractRoom abstractRoom = abstractRooms[i];
			num4 += abstractRoom.nodes.Length;
			foreach (AbstractCreature creature in abstractRoom.creatures)
			{
				num2++;
				if (creature.realizedCreature != null)
				{
					num3++;
				}
			}
			if (abstractRoom.realizedRoom != null)
			{
				num++;
			}
		}
	}
}
