using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class WorldLoader
{
	public struct BatMigrationBlockage
	{
		public string destRoom;

		public string fromRoom;

		public BatMigrationBlockage(string fromRoom, string destRoom)
		{
			this.destRoom = destRoom;
			this.fromRoom = fromRoom;
		}
	}

	public class Activity : ExtEnum<Activity>
	{
		public static Activity Init = new Activity("Init", register: true);

		public static Activity MappingRooms = new Activity("MappingRooms", register: true);

		public static Activity FindingCreatures = new Activity("FindingCreatures", register: true);

		public static Activity FindingBatBlockages = new Activity("FindingBatBlockages", register: true);

		public static Activity CreatingAbstractRooms = new Activity("CreatingAbstractRooms", register: true);

		public static Activity CappingBrokenExits = new Activity("CappingBrokenExits", register: true);

		public static Activity CreatingWorld = new Activity("CreatingWorld", register: true);

		public static Activity MappingSwarmRooms = new Activity("MappingSwarmRooms", register: true);

		public static Activity SimulateMovement = new Activity("SimulateMovement", register: true);

		public static Activity Finished = new Activity("Finished", register: true);

		public Activity(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public static class Preprocessing
	{
		public delegate bool? PreprocessorCondition(string text, RainWorldGame game);

		public static List<PreprocessorCondition> preprocessorConditions = new List<PreprocessorCondition> { PrinceEncounters, SpinningTopEndingConditions, WeaverEndingCondition, DaemonEndingCondition };

		public static string PreprocessLine(string line, RainWorldGame game, SlugcatStats.Timeline timelinePosition)
		{
			if (line.Length < 2 || line.Substring(0, 2) == "//")
			{
				return null;
			}
			if (line[0] == '(' && line.Contains(')'))
			{
				if (!TimelineMatch(line.Substring(1, line.IndexOf(")") - 1), timelinePosition))
				{
					return null;
				}
				line = line.Substring(line.IndexOf(")") + 1);
			}
			if (line[0] == '{' && line.Contains('}'))
			{
				if (!PreprocessCustomConditions(line.Substring(1, line.IndexOf("}") - 1), game))
				{
					return null;
				}
				line = line.Substring(line.IndexOf("}") + 1);
			}
			return line;
		}

		public static bool PreprocessCustomConditions(string text, RainWorldGame game)
		{
			string[] array = text.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				bool flag = false;
				if (text2.Contains('!'))
				{
					flag = true;
					text2 = text2.Replace("!", "");
				}
				foreach (PreprocessorCondition preprocessorCondition in preprocessorConditions)
				{
					bool? flag2 = preprocessorCondition(text2, game);
					if (flag2.HasValue && flag2 == flag)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static bool TimelineMatch(string text, SlugcatStats.Timeline timeline)
		{
			bool flag = false;
			bool flag2 = false;
			if (text.StartsWith("X-"))
			{
				text = text.Substring(2);
				flag2 = true;
			}
			if (timeline == null)
			{
				return flag2;
			}
			string[] array = text.Split(',');
			foreach (string text2 in array)
			{
				if (int.TryParse(text2.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					if (timeline == SlugcatStats.SlugcatToTimeline(BackwardsCompatibilityRemix.ParsePlayerNumber(result)))
					{
						flag = true;
						break;
					}
				}
				else if (text2.Trim() == timeline.ToString())
				{
					flag = true;
					break;
				}
			}
			return flag != flag2;
		}

		public static bool? SpinningTopEndingConditions(string text, RainWorldGame game)
		{
			bool flag = game?.GetStorySession?.saveState?.deathPersistentSaveData == null;
			if (text == "PostBathScene")
			{
				return !flag && (game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow || game.GetStorySession.finalWarpSequenceStarted);
			}
			if (text == "ToysEnding")
			{
				return game != null && game.rainWorld.progression.miscProgressionData.beaten_Watcher_SpinningTop && !game.GetStorySession.finalWarpSequenceStarted;
			}
			return null;
		}

		public static bool? PrinceEncounters(string text, RainWorldGame game)
		{
			if (!text.StartsWith("PrinceEncounters:"))
			{
				return null;
			}
			if (game == null)
			{
				return null;
			}
			string[] array = text.Split(':');
			if (array.Length != 2 || !int.TryParse(array[1], out var result))
			{
				return null;
			}
			if (!game.IsStorySession)
			{
				return false;
			}
			if (!ModManager.Watcher)
			{
				return true;
			}
			return game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters >= result;
		}

		public static bool? WeaverEndingCondition(string text, RainWorldGame game)
		{
			bool flag = game?.GetStorySession?.saveState?.deathPersistentSaveData == null;
			if (text == "WeaverEnding")
			{
				return !flag && game.rainWorld.progression.miscProgressionData.beaten_Watcher_VoidWeaver;
			}
			return null;
		}

		public static bool? DaemonEndingCondition(string text, RainWorldGame game)
		{
			bool flag = game?.GetStorySession?.saveState?.deathPersistentSaveData == null;
			if (text == "TreeSequence")
			{
				return !flag && game.GetStorySession.finalWarpSequenceStarted && game.overWorld.specialWarpCallback is WarpPoint warpPoint && warpPoint.Data.destRoom.ToLowerInvariant().StartsWith("wrsa_tree");
			}
			if (text == "PrinceSequence")
			{
				return !flag && game.GetStorySession.finalWarpSequenceStarted && game.overWorld.specialWarpCallback is WarpPoint warpPoint2 && warpPoint2.Data.destRoom.ToLowerInvariant().StartsWith("wrsa_prince");
			}
			return null;
		}
	}

	public struct ConditionalLink
	{
		public string RoomName;

		public string TargetForReplacement;

		public int DisconnectTarget;

		public string ReplacementDestination;

		public ConditionalLink(string GetRoomName, string GetTargetForReplacement, string GetReplacementDestination)
		{
			RoomName = GetRoomName;
			if (int.TryParse(GetTargetForReplacement, NumberStyles.Any, CultureInfo.InvariantCulture, out DisconnectTarget))
			{
				TargetForReplacement = null;
			}
			else
			{
				TargetForReplacement = GetTargetForReplacement;
			}
			ReplacementDestination = GetReplacementDestination;
		}
	}

	public class LoadingContext : ExtEnum<LoadingContext>
	{
		public static LoadingContext FULL = new LoadingContext("FULL", register: true);

		public static LoadingContext FASTTRAVEL = new LoadingContext("FASTTRAVEL", register: true);

		public static LoadingContext MAPMERGE = new LoadingContext("MAPMERGE", register: true);

		public LoadingContext(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public static AGLog<WorldLoader> Log = new AGLog<WorldLoader>();

	public RainWorldGame game;

	public World world;

	public bool singleRoomWorld;

	public string worldName;

	public RainWorldGame.SetupValues setupValues;

	public List<string[]> roomAdder = new List<string[]>();

	public List<List<string>> roomTags = new List<List<string>>();

	public List<int> swarmRoomsList = new List<int>();

	public SwarmRoomMapper swarmRoomMapper;

	public int[,] fliesMigrationBlockages;

	public List<int> sheltersList = new List<int>();

	public List<int> gatesList = new List<int>();

	public List<WorldCoordinate> faultyExits = new List<WorldCoordinate>();

	public List<BatMigrationBlockage> tempBatBlocks = new List<BatMigrationBlockage>();

	public List<World.CreatureSpawner> spawners = new List<World.CreatureSpawner>();

	public List<AbstractRoom> abstractRooms = new List<AbstractRoom>();

	public List<string> lines;

	public SlugcatStats.Name playerCharacter;

	public SlugcatStats.Timeline timelinePosition;

	public Activity activity = Activity.Init;

	public int cntr;

	public int threadCounter;

	public int rmcntr;

	public int startOfWorldDefinition;

	public int endOfWorldDefinition;

	public int startOfCreatures;

	public int endOfCreatures;

	public int startOfBatBlocks;

	public int endOfBatBlocks;

	public int simulateUpdateTicks;

	public int updateAbstractRoom;

	public Thread thread;

	public bool threadFinished;

	public bool requestCreateWorld;

	public bool createdWorld;

	public bool requestAddFliesAIWorldProcess;

	public bool addedFliesAIWorldProcess;

	public bool requestSimulateMovement;

	public bool simulateMovementComplete;

	public int abstractLoaderDelay;

	public LoadingContext loadContext;

	public bool finding_creatures_done;

	public bool creating_abstract_rooms_finished;

	public int fccntr;

	public int startConditionalLinksDefinition;

	public int endOfConditionalLinksDefinition;

	public bool ExtractConditionalLinks;

	public List<ConditionalLink> ConditionalLinkList;

	public Dictionary<string, string> ReplaceRoomNames;

	public float[] creatureStats;

	public bool Finished { get; set; }

	public World ReturnWorld()
	{
		if (Finished)
		{
			if (game != null)
			{
				if (ModManager.MSC && Region.IsRubiconRegion(worldName))
				{
					Shader.EnableKeyword("HR");
				}
				else
				{
					Shader.DisableKeyword("HR");
				}
			}
			return world;
		}
		return null;
	}

	[Obsolete("Use timeline parameter constructor instead.")]
	public WorldLoader(RainWorldGame game, SlugcatStats.Name playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
		: this(game, playerCharacter, SlugcatStats.SlugcatToTimeline(playerCharacter), singleRoomWorld, worldName, region, setupValues)
	{
	}

	public WorldLoader(RainWorldGame game, SlugcatStats.Name playerCharacter, SlugcatStats.Timeline timelinePosition, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
	{
		this.game = game;
		this.timelinePosition = timelinePosition;
		this.playerCharacter = playerCharacter;
		creatureStats = new float[ExtEnum<CreatureTemplate.Type>.values.Count + 5];
		ConditionalLinkList = new List<ConditionalLink>();
		ReplaceRoomNames = new Dictionary<string, string>();
		float preCycleRainPulse_Scale = 0f;
		int sunDownStartTime = 0;
		float drainWorldFlood = 0f;
		float drainWorldFlood2 = 0f;
		if (ModManager.PrecycleModule && game != null)
		{
			preCycleRainPulse_Scale = game.globalRain.preCycleRainPulse_Scale;
			if (game.overWorld != null && game.world != null)
			{
				sunDownStartTime = game.world.rainCycle.sunDownStartTime;
				drainWorldFlood = game.globalRain.drainWorldFlood;
				drainWorldFlood2 = game.globalRain.drainWorldFlood;
			}
		}
		world = new World(game, region, worldName, singleRoomWorld);
		if (game != null)
		{
			game.timeInRegionThisCycle = 0;
		}
		if (ModManager.PrecycleModule)
		{
			if (this.game != null && this.game.overWorld != null && this.game.world != null)
			{
				game.globalRain.preCycleRainPulse_Scale = preCycleRainPulse_Scale;
				world.rainCycle.sunDownStartTime = sunDownStartTime;
				game.globalRain.drainWorldFlood = drainWorldFlood;
				game.globalRain.drainWorldFlood = drainWorldFlood2;
				Custom.Log("Loaded world, transfering precycle scale", this.game.globalRain.preCycleRainPulse_Scale.ToString());
			}
			else
			{
				Custom.Log("First world loaded, holding precycle scale.");
			}
		}
		this.singleRoomWorld = singleRoomWorld;
		this.worldName = worldName;
		this.setupValues = setupValues;
		lines = new List<string>();
		if (!singleRoomWorld)
		{
			string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + worldName + Path.DirectorySeparatorChar + "world_" + worldName + ".txt"));
			for (int i = 0; i < array.Length; i++)
			{
				string text = Preprocessing.PreprocessLine(array[i], game, timelinePosition);
				if (text != null)
				{
					lines.Add(text);
				}
			}
		}
		if (!singleRoomWorld)
		{
			simulateUpdateTicks = 100;
		}
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		for (int num = lines.Count - 1; num > 0; num--)
		{
			string[] array2 = Regex.Split(lines[num], " : ");
			if (array2.Length == 3 && !(array2[1] != "EXCLUSIVEROOM"))
			{
				if (!dictionary.ContainsKey(array2[2]))
				{
					dictionary[array2[2]] = new List<string>(array2[0].Split(','));
				}
				else
				{
					dictionary[array2[2]].AddRange(array2[0].Split(','));
				}
				lines.RemoveAt(num);
			}
		}
		if (dictionary.Count > 0)
		{
			int num2 = -1;
			for (int j = 0; j < lines.Count; j++)
			{
				if (lines[j] == "END CONDITIONAL LINKS")
				{
					num2 = j - 1;
					break;
				}
			}
			if (num2 != -1)
			{
				foreach (KeyValuePair<string, List<string>> item in dictionary)
				{
					lines.Insert(num2, string.Join(",", item.Value) + " : EXCLUSIVEROOM : " + item.Key);
				}
			}
		}
		MapExporter.WorldLoader_ctor(this, lines);
	}

	[Obsolete("Use timeline parameter constructor instead.")]
	public WorldLoader(RainWorldGame game, SlugcatStats.Name playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues, LoadingContext context)
		: this(game, playerCharacter, SlugcatStats.SlugcatToTimeline(playerCharacter), singleRoomWorld, worldName, region, setupValues, context)
	{
	}

	public WorldLoader(RainWorldGame game, SlugcatStats.Name playerCharacter, SlugcatStats.Timeline timelinePosition, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues, LoadingContext context)
		: this(game, playerCharacter, timelinePosition, singleRoomWorld, worldName, region, setupValues)
	{
		loadContext = context;
	}

	public void NextActivity()
	{
		activity = new Activity(ExtEnum<Activity>.values.GetEntry(activity.Index + 1));
		if (activity == Activity.Finished)
		{
			return;
		}
		if ((loadContext == LoadingContext.FASTTRAVEL || loadContext == LoadingContext.MAPMERGE) && (activity == Activity.CappingBrokenExits || activity == Activity.FindingBatBlockages || activity == Activity.FindingCreatures || activity == Activity.MappingSwarmRooms || activity == Activity.SimulateMovement))
		{
			if (activity == Activity.FindingCreatures)
			{
				finding_creatures_done = true;
			}
			NextActivity();
		}
		else if (activity == Activity.MappingRooms)
		{
			rmcntr = 0;
			if (singleRoomWorld)
			{
				roomAdder.Add(new string[1] { worldName });
				roomTags.Add(null);
				if (setupValues.worldCreaturesSpawn)
				{
					swarmRoomsList.Add(0);
				}
				NextActivity();
				return;
			}
			startConditionalLinksDefinition = -1;
			endOfConditionalLinksDefinition = -1;
			startOfWorldDefinition = 0;
			endOfWorldDefinition = 0;
			startOfCreatures = 0;
			endOfCreatures = 0;
			startOfBatBlocks = 0;
			endOfBatBlocks = 0;
			for (int i = 0; i < lines.Count; i++)
			{
				switch (lines[i])
				{
				case "ROOMS":
					startOfWorldDefinition = i + 1;
					break;
				case "END ROOMS":
					endOfWorldDefinition = i - 1;
					break;
				case "CREATURES":
					startOfCreatures = i + 1;
					break;
				case "END CREATURES":
					endOfCreatures = i - 1;
					break;
				case "BAT MIGRATION BLOCKAGES":
					startOfBatBlocks = i + 1;
					break;
				case "END BAT MIGRATION BLOCKAGES":
					endOfBatBlocks = i - 1;
					break;
				case "CONDITIONAL LINKS":
					startConditionalLinksDefinition = i + 1;
					break;
				case "END CONDITIONAL LINKS":
					endOfConditionalLinksDefinition = i - 1;
					break;
				}
			}
			if (startConditionalLinksDefinition == -1)
			{
				ExtractConditionalLinks = false;
				cntr = startOfWorldDefinition;
			}
			else
			{
				ExtractConditionalLinks = true;
				cntr = startConditionalLinksDefinition;
			}
		}
		else if (activity == Activity.FindingCreatures)
		{
			if (setupValues.worldCreaturesSpawn && !singleRoomWorld)
			{
				fccntr = startOfCreatures;
				finding_creatures_done = false;
				new Thread(FindingCreaturesThread).Start();
			}
			else
			{
				finding_creatures_done = true;
			}
			NextActivity();
		}
		else if (activity == Activity.FindingBatBlockages)
		{
			if (setupValues.worldCreaturesSpawn && !singleRoomWorld)
			{
				cntr = startOfBatBlocks;
			}
			else
			{
				NextActivity();
			}
		}
		else if (activity == Activity.CreatingAbstractRooms)
		{
			cntr = 0;
			creating_abstract_rooms_finished = false;
			new Thread(CreatingAbstractRoomsThread).Start();
		}
		else if (activity == Activity.CappingBrokenExits)
		{
			cntr = 0;
			for (int j = 0; j < abstractRooms.Count; j++)
			{
				for (int k = 0; k < abstractRooms[j].nodes.Length && abstractRooms[j].nodes[k].type == AbstractRoomNode.Type.Exit; k++)
				{
					if (k >= abstractRooms[j].connections.Length)
					{
						faultyExits.Add(new WorldCoordinate(j + world.firstRoomIndex, -1, -1, k));
					}
				}
			}
		}
		else
		{
			if (activity == Activity.CreatingWorld)
			{
				return;
			}
			if (activity == Activity.MappingSwarmRooms)
			{
				if (game != null)
				{
					lock (this)
					{
						requestAddFliesAIWorldProcess = true;
					}
					if (!singleRoomWorld)
					{
						swarmRoomMapper = new SwarmRoomMapper(world, fliesMigrationBlockages);
						cntr = 0;
					}
					else
					{
						NextActivity();
					}
				}
				else
				{
					NextActivity();
				}
			}
			else
			{
				if (!(activity == Activity.SimulateMovement))
				{
					return;
				}
				if (game != null)
				{
					cntr = 0;
					lock (this)
					{
						threadCounter = 0;
						simulateUpdateTicks = game.clock;
						return;
					}
				}
				NextActivity();
			}
		}
	}

	public void Update()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		int num = 0;
		int num2 = 0;
		lock (this)
		{
			if (!threadFinished && thread == null)
			{
				thread = new Thread(UpdateThread);
				thread.Start();
			}
			if (requestCreateWorld)
			{
				flag = true;
			}
			if (requestAddFliesAIWorldProcess)
			{
				requestAddFliesAIWorldProcess = false;
				flag2 = true;
			}
			if (requestSimulateMovement)
			{
				flag3 = true;
			}
			num = simulateUpdateTicks;
			num2 = threadCounter;
			if (threadFinished)
			{
				if (!Finished)
				{
					Finished = true;
				}
				if (thread != null)
				{
					thread = null;
				}
			}
		}
		if (flag)
		{
			CreatingWorld();
			lock (this)
			{
				requestCreateWorld = false;
				createdWorld = true;
			}
		}
		if (flag2)
		{
			world.AddWorldProcess(new FliesWorldAI(world));
			lock (this)
			{
				addedFliesAIWorldProcess = true;
			}
		}
		if (!flag3)
		{
			return;
		}
		bool flag4 = false;
		for (int i = 0; i < 30; i++)
		{
			num2++;
			if (num2 < num && num2 < 4800)
			{
				SimulateUpdate();
				continue;
			}
			flag4 = true;
			break;
		}
		lock (this)
		{
			requestSimulateMovement = false;
			threadCounter = num2;
			if (flag4)
			{
				simulateMovementComplete = true;
			}
		}
	}

	public void UpdateThread()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		if (game == null || game.overWorld == null || game.world == null)
		{
			abstractLoaderDelay = 0;
		}
		while (true)
		{
			lock (this)
			{
				flag = requestCreateWorld;
				flag2 = createdWorld;
				flag3 = addedFliesAIWorldProcess;
				flag4 = requestSimulateMovement;
				flag5 = simulateMovementComplete;
			}
			try
			{
				if (activity == Activity.MappingRooms)
				{
					if (ExtractConditionalLinks && cntr <= endOfConditionalLinksDefinition)
					{
						while (cntr <= endOfConditionalLinksDefinition)
						{
							LoadConditionalLinks();
							cntr++;
						}
						cntr = startOfWorldDefinition;
					}
					if (abstractLoaderDelay > 0)
					{
						abstractLoaderDelay--;
						Thread.Sleep(25);
						continue;
					}
					while (cntr <= endOfWorldDefinition)
					{
						MappingRooms();
						cntr++;
					}
					NextActivity();
				}
				else
				{
					if (activity == Activity.FindingCreatures)
					{
						continue;
					}
					if (activity == Activity.FindingBatBlockages)
					{
						if (cntr <= endOfBatBlocks)
						{
							FindingBatBlockages();
							cntr++;
						}
						else
						{
							NextActivity();
						}
					}
					else if (activity == Activity.CreatingAbstractRooms)
					{
						if (finding_creatures_done && creating_abstract_rooms_finished)
						{
							NextActivity();
						}
					}
					else if (activity == Activity.CappingBrokenExits)
					{
						if (abstractLoaderDelay > 0)
						{
							abstractLoaderDelay--;
							Thread.Sleep(25);
						}
						else if (cntr < faultyExits.Count)
						{
							CappingBrokenExits();
							cntr++;
							if (cntr % 30 == 0)
							{
								abstractLoaderDelay = 10;
							}
						}
						else
						{
							NextActivity();
						}
					}
					else if (activity == Activity.CreatingWorld)
					{
						if (flag)
						{
							continue;
						}
						if (!flag2)
						{
							lock (this)
							{
								requestCreateWorld = true;
							}
						}
						else
						{
							NextActivity();
						}
					}
					else if (activity == Activity.MappingSwarmRooms)
					{
						if (!flag3)
						{
							continue;
						}
						for (int i = 0; i < 30; i++)
						{
							cntr++;
							swarmRoomMapper.Update();
							if (swarmRoomMapper.done || cntr > 50000)
							{
								world.fliesWorldAI.LoadDijkstraMaps(swarmRoomMapper);
								NextActivity();
								break;
							}
						}
					}
					else if (activity == Activity.SimulateMovement)
					{
						if (flag4)
						{
							continue;
						}
						if (!flag5)
						{
							lock (this)
							{
								requestSimulateMovement = true;
							}
							continue;
						}
						lock (this)
						{
							cntr = threadCounter;
						}
						NextActivity();
					}
					else if (activity == Activity.Finished)
					{
						lock (this)
						{
							threadFinished = true;
							break;
						}
					}
					continue;
				}
			}
			catch (Exception arg)
			{
				Custom.LogWarning($"{activity} Thread failed [{arg}]");
				lock (this)
				{
					threadFinished = true;
					break;
				}
			}
		}
	}

	public void LoadConditionalLinks()
	{
		string[] array = Regex.Split(lines[cntr], " : ");
		string[] array3;
		if (array[1] == "EXCLUSIVEROOM")
		{
			string[] array2 = Regex.Split(array[0], ",");
			bool flag = false;
			array3 = array2;
			for (int i = 0; i < array3.Length; i++)
			{
				if (array3[i] == timelinePosition.value)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				world.DisabledMapRooms.Add(array[2]);
			}
			return;
		}
		if (array[1] == "HIDEROOM")
		{
			string[] array4 = Regex.Split(array[0], ",");
			bool flag2 = false;
			array3 = array4;
			for (int i = 0; i < array3.Length; i++)
			{
				if (array3[i] == timelinePosition.value)
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				world.DisabledMapRooms.Add(array[2]);
			}
			return;
		}
		if (array[1] == "REPLACEROOM")
		{
			string[] array5 = Regex.Split(array[0], ",");
			bool flag3 = false;
			array3 = array5;
			for (int i = 0; i < array3.Length; i++)
			{
				if (array3[i] == timelinePosition.value)
				{
					flag3 = true;
					break;
				}
			}
			if (flag3)
			{
				ReplaceRoomNames[array[2]] = array[3];
			}
			return;
		}
		string[] array6 = Regex.Split(array[0], ",");
		bool flag4 = false;
		array3 = array6;
		for (int i = 0; i < array3.Length; i++)
		{
			if (array3[i] == timelinePosition.value)
			{
				flag4 = true;
				break;
			}
		}
		if (flag4)
		{
			ConditionalLinkList.Add(new ConditionalLink(array[1], array[2], array[3]));
		}
	}

	public void MappingRooms()
	{
		if (!lines[cntr].Contains(" : "))
		{
			return;
		}
		string[] array = Regex.Split(lines[cntr], " : ");
		if (array.Length < 2)
		{
			return;
		}
		string[] array2 = ((!(array[1] == "")) ? Regex.Split(Custom.ValidateSpacedDelimiter(array[1], ","), ", ") : new string[0]);
		roomTags.Add(null);
		if (array.Length > 2)
		{
			for (int i = 2; i < array.Length; i++)
			{
				if (array[i] == "SWARMROOM")
				{
					if (!world.DisabledMapRooms.Contains(array[0]))
					{
						swarmRoomsList.Add(rmcntr + world.firstRoomIndex);
						continue;
					}
					Custom.Log("Swarm room disabled due to conditional hidden", array[0]);
				}
				else if (array[i] == "SHELTER" || array[i] == "ANCIENTSHELTER")
				{
					sheltersList.Add(rmcntr + world.firstRoomIndex);
					if (array[i] == "ANCIENTSHELTER")
					{
						if (roomTags[roomTags.Count - 1] == null)
						{
							roomTags[roomTags.Count - 1] = new List<string>();
						}
						roomTags[roomTags.Count - 1].Add(array[i]);
					}
				}
				else if (array[i] == "GATE")
				{
					gatesList.Add(rmcntr + world.firstRoomIndex);
				}
				else if (array[i] == "SCAVOUTPOST" || array[i] == "SCAVTRADER")
				{
					if (!world.DisabledMapRooms.Contains(array[0]))
					{
						if (roomTags[roomTags.Count - 1] == null)
						{
							roomTags[roomTags.Count - 1] = new List<string>();
						}
						roomTags[roomTags.Count - 1].Add(array[i]);
					}
					else
					{
						Custom.Log("Scavenger room disabled due to conditional hidden", array[0]);
					}
				}
				else
				{
					if (roomTags[roomTags.Count - 1] == null)
					{
						roomTags[roomTags.Count - 1] = new List<string>();
					}
					roomTags[roomTags.Count - 1].Add(array[i]);
				}
			}
		}
		List<string> list = new List<string> { array[0] };
		for (int j = 0; j < array2.Length; j++)
		{
			list.Add(array2[j]);
		}
		roomAdder.Add(list.ToArray());
		rmcntr++;
	}

	public void FindingCreatures()
	{
		string[] array = Regex.Split(lines[fccntr], " : ");
		if (array[0] == "LINEAGE")
		{
			AddLineageFromString(array);
		}
		else
		{
			AddSpawnersFromString(array);
		}
	}

	public void AddSpawnersFromString(string[] line)
	{
		int num = -1;
		if (line[0] == "OFFSCREEN")
		{
			num = world.firstRoomIndex + roomAdder.Count;
		}
		else
		{
			for (int i = 0; i < roomAdder.Count; i++)
			{
				if (num >= 0)
				{
					break;
				}
				if (roomAdder[i][0] == line[0])
				{
					num = world.firstRoomIndex + i;
				}
			}
		}
		string[] array = Regex.Split(Custom.ValidateSpacedDelimiter(line[1], ","), ", ");
		for (int j = 0; j < array.Length; j++)
		{
			CreatureTemplate.Type type = null;
			int amount = 1;
			string text = "";
			string[] array2 = array[j].Split('-');
			type = CreatureTypeFromString(array2[1]);
			if (array2.Length > 2)
			{
				bool flag = false;
				for (int k = 2; k < array2.Length; k++)
				{
					if (array2[k].Length > 0 && array2[k][0] == '{')
					{
						text = array2[k];
						flag = true;
					}
					else if (flag)
					{
						text = text + "-" + array2[k];
					}
					else
					{
						try
						{
							amount = Convert.ToInt32(array2[k], CultureInfo.InvariantCulture);
						}
						catch
						{
							amount = 1;
						}
					}
					if (array2[k].Contains("}"))
					{
						flag = false;
					}
				}
			}
			if (text == "")
			{
				text = null;
			}
			if (type != null)
			{
				World.SimpleSpawner item = new World.SimpleSpawner(world.region.regionNumber, spawners.Count, new WorldCoordinate(num, -1, -1, Convert.ToInt32(array2[0], CultureInfo.InvariantCulture)), type, text, amount);
				spawners.Add(item);
			}
		}
	}

	public void AddLineageFromString(string[] s)
	{
		int num = -1;
		if (s[1] == "OFFSCREEN")
		{
			num = roomAdder.Count;
		}
		else
		{
			for (int i = 0; i < roomAdder.Count; i++)
			{
				if (roomAdder[i][0] == s[1])
				{
					num = i;
					break;
				}
			}
		}
		if (num < 0)
		{
			return;
		}
		num += world.firstRoomIndex;
		int num2 = int.Parse(s[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		string[] array = Regex.Split(Custom.ValidateSpacedDelimiter(s[3], ","), ", ");
		int[] array2 = new int[array.Length];
		float[] array3 = new float[array.Length];
		string[] array4 = new string[array.Length];
		try
		{
			for (int j = 0; j < array.Length; j++)
			{
				string[] array5 = Regex.Split(array[j], "-");
				if (array5[0].ToLower() == "none")
				{
					array2[j] = -1;
				}
				else
				{
					CreatureTemplate.Type type = CreatureTypeFromString(array5[0]);
					if (type != null && type.Index != -1)
					{
						array2[j] = type.Index;
					}
					else
					{
						array2[j] = -1;
					}
				}
				array4[j] = "";
				bool flag = false;
				for (int k = 1; k < array5.Length; k++)
				{
					if (array5[k][0] == '{')
					{
						array4[j] = array5[k];
						flag = true;
					}
					else if (flag)
					{
						ref string reference = ref array4[j];
						reference = reference + "-" + array5[k];
					}
					else
					{
						try
						{
							array3[j] = float.Parse(array5[k], NumberStyles.Any, CultureInfo.InvariantCulture);
						}
						catch
						{
							array3[j] = 0f;
						}
					}
					if (array5[k].Contains("}"))
					{
						flag = false;
					}
				}
				if (array4[j] == "")
				{
					array4[j] = null;
				}
			}
		}
		catch (Exception ex)
		{
			Custom.LogWarning("Failed to create lineage!", ex.ToString());
			for (int l = 0; l < array.Length; l++)
			{
				Custom.LogWarning(array[l]);
			}
			Custom.LogWarning("-------------");
			return;
		}
		bool flag2 = false;
		int num3 = 1;
		for (int m = 0; m < spawners.Count; m++)
		{
			if (spawners[m] is World.Lineage && (spawners[m] as World.Lineage).den.room == num && (spawners[m] as World.Lineage).den.abstractNode == num2)
			{
				num3++;
				flag2 = true;
			}
		}
		World.Lineage lineage = new World.Lineage(world.region.regionNumber, spawners.Count, new WorldCoordinate(num, -1, -1, num2), array2, array3, array4, flag2 ? num3 : 0);
		if (s.Length > 4)
		{
			lineage.nightCreature = true;
		}
		spawners.Add(lineage);
	}

	public static CreatureTemplate.Type CreatureTypeFromString(string s)
	{
		CreatureTemplate.Type type = MoreSlugcatsCreatures.CreatureTypeFromString(s);
		if (type != null)
		{
			return type;
		}
		if (ModManager.Watcher)
		{
			CreatureTemplate.Type type2 = WatcherCreatures.CreatureTypeFromString(s);
			if (type2 != null)
			{
				return type2;
			}
		}
		switch (s.ToLower())
		{
		case "pink":
			return CreatureTemplate.Type.PinkLizard;
		case "green":
			return CreatureTemplate.Type.GreenLizard;
		case "blue":
			return CreatureTemplate.Type.BlueLizard;
		case "yellow":
			return CreatureTemplate.Type.YellowLizard;
		case "white":
			return CreatureTemplate.Type.WhiteLizard;
		case "red":
			return CreatureTemplate.Type.RedLizard;
		case "black":
			return CreatureTemplate.Type.BlackLizard;
		case "cyan":
			return CreatureTemplate.Type.CyanLizard;
		case "leech":
			return CreatureTemplate.Type.Leech;
		case "sea leech":
		case "sealeech":
			return CreatureTemplate.Type.SeaLeech;
		case "snail":
			return CreatureTemplate.Type.Snail;
		case "vulture":
			return CreatureTemplate.Type.Vulture;
		case "cicada a":
		case "cicadaa":
			return CreatureTemplate.Type.CicadaA;
		case "cicada b":
		case "cicadab":
			return CreatureTemplate.Type.CicadaB;
		case "cicada":
			if (!(new System.Random().NextDouble() < 0.5))
			{
				return CreatureTemplate.Type.CicadaB;
			}
			return CreatureTemplate.Type.CicadaA;
		case "lantern mouse":
		case "lanternmouse":
		case "mouse":
			return CreatureTemplate.Type.LanternMouse;
		case "spider":
			return CreatureTemplate.Type.Spider;
		case "worm":
		case "garbage worm":
		case "garbageworm":
			return CreatureTemplate.Type.GarbageWorm;
		case "leviathan":
		case "lev":
		case "bigeel":
		case "big eel":
			return CreatureTemplate.Type.BigEel;
		case "tube":
		case "tube worm":
		case "tubeworm":
			return CreatureTemplate.Type.TubeWorm;
		case "daddy":
		case "daddy long legs":
		case "daddylonglegs":
			return CreatureTemplate.Type.DaddyLongLegs;
		case "bro":
		case "bro long legs":
		case "brolonglegs":
			return CreatureTemplate.Type.BrotherLongLegs;
		case "tentacleplant":
		case "tentacle plant":
		case "tentacle":
			return CreatureTemplate.Type.TentaclePlant;
		case "polemimic":
		case "pole mimic":
		case "mimic":
			return CreatureTemplate.Type.PoleMimic;
		case "mirosbird":
		case "miros bird":
		case "miros":
			return CreatureTemplate.Type.MirosBird;
		case "centipede":
		case "cent":
			return CreatureTemplate.Type.Centipede;
		case "jetfish":
		case "jet fish":
			return CreatureTemplate.Type.JetFish;
		case "eggbug":
		case "egg bug":
			return CreatureTemplate.Type.EggBug;
		case "bigspider":
		case "big spider":
			return CreatureTemplate.Type.BigSpider;
		case "spitterspider":
		case "spitter spider":
			return CreatureTemplate.Type.SpitterSpider;
		case "bigneedle":
		case "big needle":
		case "needle":
		case "needle worm":
			return CreatureTemplate.Type.BigNeedleWorm;
		case "smallneedle":
		case "small needle":
			return CreatureTemplate.Type.SmallNeedleWorm;
		case "dropbug":
		case "drop bug":
		case "dropwig":
		case "drop wig":
			return CreatureTemplate.Type.DropBug;
		case "kingvulture":
		case "king vulture":
			return CreatureTemplate.Type.KingVulture;
		case "red centipede":
		case "redcentipede":
		case "red centi":
		case "redcenti":
			return CreatureTemplate.Type.RedCentipede;
		default:
		{
			for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
			{
				if (s == StaticWorld.creatureTemplates[i].name || s == StaticWorld.creatureTemplates[i].type.ToString())
				{
					return StaticWorld.creatureTemplates[i].type;
				}
			}
			Custom.LogWarning("No matching key was found for string: '", s, "'");
			return null;
		}
		}
	}

	public void FindingBatBlockages()
	{
		string[] array = Regex.Split(lines[cntr], " : ");
		if (array.Length == 2)
		{
			tempBatBlocks.Add(new BatMigrationBlockage(array[0], array[1]));
		}
		else
		{
			tempBatBlocks.Add(new BatMigrationBlockage("", array[0]));
		}
	}

	public void CreatingAbstractRooms()
	{
		List<IntVector2> list = new List<IntVector2>();
		List<string> list2 = new List<string>();
		foreach (ConditionalLink conditionalLink in ConditionalLinkList)
		{
			if (!(roomAdder[cntr][0] == conditionalLink.RoomName))
			{
				continue;
			}
			Custom.Log("Replaced door going to", conditionalLink.TargetForReplacement, "and changed it to", conditionalLink.ReplacementDestination);
			if (conditionalLink.TargetForReplacement != null)
			{
				for (int i = 1; i < roomAdder[cntr].Length; i++)
				{
					if (roomAdder[cntr][i] == conditionalLink.TargetForReplacement)
					{
						roomAdder[cntr][i] = conditionalLink.ReplacementDestination;
						break;
					}
				}
				continue;
			}
			int num = 0;
			for (int j = 1; j < roomAdder[cntr].Length; j++)
			{
				if (roomAdder[cntr][j] == "DISCONNECTED")
				{
					num++;
					if (num == conditionalLink.DisconnectTarget)
					{
						list.Add(new IntVector2(cntr, j));
						list2.Add(conditionalLink.ReplacementDestination);
						break;
					}
				}
			}
		}
		for (int k = 0; k < list2.Count; k++)
		{
			roomAdder[list[k].x][list[k].y] = list2[k];
		}
		int[] array = new int[roomAdder[cntr].Length - 1];
		for (int l = 1; l < roomAdder[cntr].Length; l++)
		{
			array[l - 1] = -1;
			for (int m = 0; m < roomAdder.Count; m++)
			{
				if (roomAdder[cntr][l] == roomAdder[m][0])
				{
					array[l - 1] = m + world.firstRoomIndex;
				}
			}
		}
		abstractRooms.Add(new AbstractRoom(roomAdder[cntr][0], array, cntr + world.firstRoomIndex, swarmRoomsList.IndexOf(cntr + world.firstRoomIndex), sheltersList.IndexOf(cntr + world.firstRoomIndex), gatesList.IndexOf(cntr + world.firstRoomIndex)));
		if (ReplaceRoomNames.TryGetValue(roomAdder[cntr][0], out var value))
		{
			abstractRooms[cntr].altFileName = value;
		}
		if (roomTags[cntr] != null)
		{
			for (int n = 0; n < roomTags[cntr].Count; n++)
			{
				if (roomTags[cntr][n] != "" && roomTags[cntr][n] != "SWARMROOM" && roomTags[cntr][n] != "GATE" && roomTags[cntr][n] != "SHELTER")
				{
					abstractRooms[abstractRooms.Count - 1].AddTag(roomTags[cntr][n]);
				}
			}
		}
		if (loadContext != LoadingContext.FASTTRAVEL)
		{
			LoadAbstractRoom(world, roomAdder[cntr][0], abstractRooms[cntr], setupValues);
		}
	}

	public void CappingBrokenExits()
	{
		int num = faultyExits[cntr].room - world.firstRoomIndex;
		int[] array = new int[abstractRooms[num].connections.Length + 1];
		for (int i = 0; i < abstractRooms[num].connections.Length; i++)
		{
			array[i] = abstractRooms[num].connections[i];
		}
		array[abstractRooms[num].connections.Length] = -1;
		abstractRooms[num] = new AbstractRoom(roomAdder[num][0], array, faultyExits[cntr].room, swarmRoomsList.IndexOf(num + world.firstRoomIndex), sheltersList.IndexOf(num + world.firstRoomIndex), gatesList.IndexOf(num + world.firstRoomIndex));
		LoadAbstractRoom(world, roomAdder[num][0], abstractRooms[num], setupValues);
	}

	public void CreatingWorld()
	{
		world.spawners = spawners.ToArray();
		List<World.Lineage> list = new List<World.Lineage>();
		for (int i = 0; i < spawners.Count; i++)
		{
			if (spawners[i] is World.Lineage)
			{
				list.Add(spawners[i] as World.Lineage);
			}
		}
		world.lineages = list.ToArray();
		if (loadContext == LoadingContext.FASTTRAVEL || loadContext == LoadingContext.MAPMERGE)
		{
			world.LoadWorldForFastTravel(timelinePosition, abstractRooms, swarmRoomsList.ToArray(), sheltersList.ToArray(), gatesList.ToArray());
		}
		else
		{
			world.LoadWorld(timelinePosition, abstractRooms, swarmRoomsList.ToArray(), sheltersList.ToArray(), gatesList.ToArray());
			creatureStats[0] = world.NumberOfRooms;
			creatureStats[1] = world.spawners.Length;
		}
		fliesMigrationBlockages = new int[tempBatBlocks.Count, 2];
		for (int j = 0; j < tempBatBlocks.Count; j++)
		{
			int num = ((world.GetAbstractRoom(tempBatBlocks[j].fromRoom) == null) ? (-1) : world.GetAbstractRoom(tempBatBlocks[j].fromRoom).index);
			int num2 = ((world.GetAbstractRoom(tempBatBlocks[j].destRoom) == null) ? (-1) : world.GetAbstractRoom(tempBatBlocks[j].destRoom).index);
			fliesMigrationBlockages[j, 0] = num;
			fliesMigrationBlockages[j, 1] = num2;
		}
		if (ModManager.MSC && game != null && game.wasAnArtificerDream)
		{
			return;
		}
		if (game != null && setupValues.worldCreaturesSpawn && game.session is StoryGameSession && !world.singleRoomWorld)
		{
			GeneratePopulation((game.session as StoryGameSession).saveState.regionLoadStrings[world.region.regionNumber] == null);
		}
		if (game != null && game.session is StoryGameSession && !world.singleRoomWorld && world.region != null && (world.region.name == "SL" || world.region.name == "RM") && SlugcatStats.AtOrAfterTimeline(timelinePosition, SlugcatStats.Timeline.Rivulet) && game.IsMoonActive())
		{
			int num3 = UnityEngine.Random.Range(3, 8);
			if (world.region.name == "RM")
			{
				num3 = UnityEngine.Random.Range(2, 4);
			}
			for (int k = 0; k < num3; k++)
			{
				AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, new WorldCoordinate(world.offScreenDen.index, -1, -1, 0), game.GetNewID());
				abstractCreature.creatureTemplate.saveCreature = false;
				(abstractCreature.abstractAI as OverseerAbstractAI).moonHelper = true;
				(abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator = 1;
				world.offScreenDen.entitiesInDens.Add(abstractCreature);
			}
		}
	}

	public void GeneratePopulation(bool fresh)
	{
		if (world.game.setupValues.cleanSpawns)
		{
			Custom.Log("clean spawns flag set, forcing fresh spawns.");
			fresh = true;
		}
		Custom.Log("Generate population for :", world.region.name, "FRESH:", fresh.ToString());
		for (int i = 0; i < spawners.Count; i++)
		{
			if (!(spawners[i] is World.Lineage))
			{
				continue;
			}
			try
			{
				World.Lineage lineage = spawners[i] as World.Lineage;
				if (world.game != null && world.game.IsStorySession && (world.game.session as StoryGameSession).saveState != null && world.region != null)
				{
					if ((world.game.session as StoryGameSession).saveState.regionStates[world.region.regionNumber] == null)
					{
						(world.game.session as StoryGameSession).saveState.regionStates[world.region.regionNumber] = new RegionState((world.game.session as StoryGameSession).saveState, world);
					}
					if (!(world.game.session as StoryGameSession).saveState.regionStates[world.region.regionNumber].lineageCounters.ContainsKey(lineage.denString))
					{
						(world.game.session as StoryGameSession).saveState.regionStates[world.region.regionNumber].lineageCounters[lineage.denString] = 0;
					}
				}
			}
			catch (Exception ex)
			{
				Custom.LogWarning("Error initializing lineage:", ex.Message);
			}
		}
		if (world.game.setupValues.proceedLineages > 0)
		{
			for (int j = 0; j < spawners.Count; j++)
			{
				if (spawners[j] is World.Lineage)
				{
					for (int k = 0; k < world.game.setupValues.proceedLineages; k++)
					{
						(spawners[j] as World.Lineage).ChanceToProgress(world);
					}
				}
			}
		}
		int num = 0;
		bool flag = false;
		foreach (AbstractRoom abstractRoom4 in abstractRooms)
		{
			if (!abstractRoom4.shelter && (!ModManager.MSC || !abstractRoom4.isAncientShelter))
			{
				num += abstractRoom4.creatures.Count;
				num += abstractRoom4.entitiesInDens.Count;
			}
			if (flag)
			{
				continue;
			}
			for (int l = 0; l < abstractRoom4.creatures.Count; l++)
			{
				if (abstractRoom4.creatures[l].hadInvalidSpawn || (abstractRoom4.creatures[l].pos.abstractNode > 0 && abstractRoom4.creatures[l].pos.abstractNode >= abstractRoom4.nodes.Length))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			for (int m = 0; m < abstractRoom4.entitiesInDens.Count; m++)
			{
				if (abstractRoom4.entitiesInDens[m].pos.abstractNode > 0 && abstractRoom4.entitiesInDens[m].pos.abstractNode >= abstractRoom4.nodes.Length)
				{
					flag = true;
					break;
				}
			}
		}
		bool flag2 = false;
		bool flag3 = num <= (int)((float)spawners.Count * 0.3f);
		if (spawners.Count == 0)
		{
			flag2 = true;
		}
		List<int> respawnCreatures = new List<int>((game.session as StoryGameSession).saveState.respawnCreatures);
		for (int n = 0; n < spawners.Count; n++)
		{
			if (spawners[n] is World.SimpleSpawner)
			{
				World.SimpleSpawner simpleSpawner = spawners[n] as World.SimpleSpawner;
				int num2 = 0;
				if (!StaticWorld.GetCreatureTemplate(simpleSpawner.creatureType).quantified && StaticWorld.GetCreatureTemplate(simpleSpawner.creatureType).saveCreature)
				{
					num2 = ((!fresh) ? HowManyOfThisCritterShouldRespawn(simpleSpawner.SpawnerID, simpleSpawner.amount) : simpleSpawner.amount);
				}
				if (num2 > 0)
				{
					flag2 = true;
					break;
				}
			}
			else if (spawners[n] is World.Lineage)
			{
				World.Lineage lineage2 = spawners[n] as World.Lineage;
				if (fresh || ShouldThisCritterRespawn(lineage2.SpawnerID))
				{
					flag2 = true;
					break;
				}
			}
		}
		(game.session as StoryGameSession).saveState.respawnCreatures = respawnCreatures;
		if ((!flag2 && flag3) || flag)
		{
			Custom.Log("WARNING: NO SPAWNERS ACTIVE IN THIS REGION, SOMETHING LIKELY WENT WRONG, RESETTING THE REGION SPAWNS FRESH FOR " + worldName + " :: anysettospawn? " + flag2 + " :: lowcreaturecount? " + flag3 + " [" + num + "/" + spawners.Count + "] :: invalidNodes? " + flag);
			foreach (AbstractRoom abstractRoom5 in abstractRooms)
			{
				if (!abstractRoom5.shelter && (!ModManager.MSC || !abstractRoom5.isAncientShelter))
				{
					abstractRoom5.creatures.Clear();
					abstractRoom5.entitiesInDens.Clear();
				}
				else
				{
					if (world.game.IsStorySession && !(abstractRoom5.name != world.game.GetStorySession.saveState.denPosition))
					{
						continue;
					}
					for (int num3 = abstractRoom5.creatures.Count - 1; num3 >= 0; num3--)
					{
						if (!AbstractPhysicalObject.IsObjectImportant(abstractRoom5.creatures[num3], world))
						{
							abstractRoom5.creatures.RemoveAt(num3);
						}
					}
					abstractRoom5.entitiesInDens.Clear();
				}
			}
			fresh = true;
		}
		for (int num4 = 0; num4 < spawners.Count; num4++)
		{
			if (fresh)
			{
				SpawnerStabilityCheck(spawners[num4]);
			}
			if (spawners[num4] is World.SimpleSpawner)
			{
				World.SimpleSpawner simpleSpawner2 = spawners[num4] as World.SimpleSpawner;
				int num5 = 0;
				num5 = ((!fresh && !StaticWorld.GetCreatureTemplate(simpleSpawner2.creatureType).quantified && StaticWorld.GetCreatureTemplate(simpleSpawner2.creatureType).saveCreature) ? HowManyOfThisCritterShouldRespawn(simpleSpawner2.SpawnerID, simpleSpawner2.amount) : simpleSpawner2.amount);
				if (num5 <= 0)
				{
					continue;
				}
				creatureStats[simpleSpawner2.creatureType.Index + 4] += num5;
				AbstractRoom abstractRoom = world.GetAbstractRoom(simpleSpawner2.den);
				if (abstractRoom == null || simpleSpawner2.den.abstractNode >= abstractRoom.nodes.Length || (!(abstractRoom.nodes[simpleSpawner2.den.abstractNode].type == AbstractRoomNode.Type.Den) && !(abstractRoom.nodes[simpleSpawner2.den.abstractNode].type == AbstractRoomNode.Type.GarbageHoles)))
				{
					continue;
				}
				if (StaticWorld.GetCreatureTemplate(simpleSpawner2.creatureType).quantified)
				{
					abstractRoom.AddQuantifiedCreature(simpleSpawner2.den.abstractNode, simpleSpawner2.creatureType, simpleSpawner2.amount);
					continue;
				}
				for (int num6 = 0; num6 < num5; num6++)
				{
					AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(simpleSpawner2.creatureType), null, simpleSpawner2.den, world.game.GetNewID(simpleSpawner2.SpawnerID));
					abstractCreature.spawnData = simpleSpawner2.spawnDataString;
					abstractCreature.nightCreature = simpleSpawner2.nightCreature;
					abstractCreature.setCustomFlags();
					abstractRoom.MoveEntityToDen(abstractCreature);
				}
			}
			else
			{
				if (!(spawners[num4] is World.Lineage))
				{
					continue;
				}
				World.Lineage lineage3 = spawners[num4] as World.Lineage;
				creatureStats[lineage3.creatureTypes[0] + 4] += 1f;
				if (fresh || ShouldThisCritterRespawn(lineage3.SpawnerID))
				{
					AbstractRoom abstractRoom2 = world.GetAbstractRoom(lineage3.den);
					CreatureTemplate.Type type = lineage3.CurrentType((game.session as StoryGameSession).saveState);
					if (type == null)
					{
						lineage3.ChanceToProgress(world);
					}
					else if (abstractRoom2 != null && lineage3.den.abstractNode < abstractRoom2.nodes.Length && (abstractRoom2.nodes[lineage3.den.abstractNode].type == AbstractRoomNode.Type.Den || abstractRoom2.nodes[lineage3.den.abstractNode].type == AbstractRoomNode.Type.GarbageHoles))
					{
						AbstractCreature abstractCreature2 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(type), null, lineage3.den, world.game.GetNewID(lineage3.SpawnerID));
						abstractCreature2.spawnData = lineage3.CurrentSpawnData((game.session as StoryGameSession).saveState);
						abstractCreature2.nightCreature = lineage3.nightCreature;
						abstractCreature2.setCustomFlags();
						abstractRoom2.MoveEntityToDen(abstractCreature2);
					}
					if (type == null)
					{
						(game.session as StoryGameSession).saveState.respawnCreatures.Add(lineage3.SpawnerID);
						Custom.Log("add NONE creature to respawns for lineage", lineage3.SpawnerID.ToString());
					}
				}
			}
		}
		if (OverseerSpawnConditions(playerCharacter))
		{
			WorldCoordinate worldCoordinate = new WorldCoordinate(world.offScreenDen.index, -1, -1, 0);
			if (world.region.name == "SU")
			{
				worldCoordinate = new WorldCoordinate(world.GetAbstractRoom("SU_C04").index, 137, 17, 0);
			}
			AbstractCreature abstractCreature3 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, worldCoordinate, new EntityID(-1, 5));
			if (world.GetAbstractRoom(worldCoordinate).offScreenDen)
			{
				world.GetAbstractRoom(worldCoordinate).entitiesInDens.Add(abstractCreature3);
			}
			else
			{
				world.GetAbstractRoom(worldCoordinate).AddEntity(abstractCreature3);
			}
			int asPlayerGuide = 1;
			if (ModManager.MSC)
			{
				if (playerCharacter == MoreSlugcatsEnums.SlugcatStatsName.Spear)
				{
					asPlayerGuide = 3;
				}
				else if (playerCharacter == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
				{
					asPlayerGuide = 2;
				}
				else if (playerCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer || playerCharacter == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
				{
					asPlayerGuide = 0;
				}
			}
			(abstractCreature3.abstractAI as OverseerAbstractAI).SetAsPlayerGuide(asPlayerGuide);
		}
		if (ModManager.Watcher && world.regionState?.rippleSpiderRespawnRoom != null)
		{
			AbstractRoom abstractRoom3 = world.GetAbstractRoom(world.regionState.rippleSpiderRespawnRoom);
			if (abstractRoom3 != null)
			{
				AbstractCreature abstractCreature4 = new AbstractCreature(pos: new WorldCoordinate(abstractRoom3.index, -1, -1, 0), world: world, creatureTemplate: StaticWorld.GetCreatureTemplate(WatcherEnums.CreatureTemplateType.RippleSpider), realizedCreature: null, ID: world.game.GetNewID());
				(abstractCreature4.abstractAI as RippleSpiderAbstractAI).waitForPlayer = true;
				abstractRoom3.AddEntity(abstractCreature4);
			}
		}
		if (RainWorld.ShowLogs)
		{
			Custom.LogImportant("==== WORLD CREATURE DENSITY STATS ====");
			Custom.LogImportant($"Config: region:{world.name} slugcatIndex:{playerCharacter}, timelineIndex:{timelinePosition}");
			Custom.LogImportant("ROOMS:", creatureStats[0].ToString(), "SPAWNERS:", creatureStats[1].ToString());
			Custom.LogImportant("Room to spawner density:", (creatureStats[1] / creatureStats[0]).ToString());
			Custom.LogImportant("Creature spawn counts: ");
			for (int num7 = 0; num7 < ExtEnum<CreatureTemplate.Type>.values.entries.Count; num7++)
			{
				if (creatureStats[4 + num7] > 0f)
				{
					Custom.LogImportant($"{ExtEnum<CreatureTemplate.Type>.values.entries[num7]} spawns: {creatureStats[4 + num7]} Spawner Density: {creatureStats[4 + num7] / creatureStats[1]} Room Density: {creatureStats[4 + num7] / creatureStats[0]}");
				}
			}
			Custom.LogImportant("================");
		}
		if ((world.region.name == "UW" || (ModManager.MSC && (world.region.name == "LC" || world.region.name == "LM")) || UnityEngine.Random.value < world.region.regionParams.overseersSpawnChance * Mathf.InverseLerp(2f, 21f, (game.session as StoryGameSession).saveState.cycleNumber + ((game.StoryCharacter == SlugcatStats.Name.Red) ? 17 : 0))) && (!ModManager.MSC || !(playerCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer) || (game.session as StoryGameSession).saveState.cycleNumber != 0))
		{
			int num8 = UnityEngine.Random.Range(world.region.regionParams.overseersMin, world.region.regionParams.overseersMax);
			for (int num9 = 0; num9 < num8; num9++)
			{
				world.offScreenDen.entitiesInDens.Add(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, new WorldCoordinate(world.offScreenDen.index, -1, -1, 0), game.GetNewID()));
			}
		}
	}

	public bool ShouldThisCritterRespawn(int spawnerNumber)
	{
		for (int i = 0; i < (game.session as StoryGameSession).saveState.respawnCreatures.Count; i++)
		{
			if ((game.session as StoryGameSession).saveState.respawnCreatures[i] == spawnerNumber)
			{
				(game.session as StoryGameSession).saveState.respawnCreatures.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public int HowManyOfThisCritterShouldRespawn(int spawnerNumber, int max)
	{
		int num = 0;
		for (int num2 = (game.session as StoryGameSession).saveState.respawnCreatures.Count - 1; num2 >= 0; num2--)
		{
			if ((game.session as StoryGameSession).saveState.respawnCreatures[num2] == spawnerNumber)
			{
				(game.session as StoryGameSession).saveState.respawnCreatures.RemoveAt(num2);
				num++;
				if (num >= max)
				{
					return num;
				}
			}
		}
		return num;
	}

	public void SimulateUpdate()
	{
		updateAbstractRoom++;
		if (updateAbstractRoom >= world.NumberOfRooms)
		{
			updateAbstractRoom = 0;
		}
		world.GetAbstractRoom(updateAbstractRoom + world.firstRoomIndex).Update(world.NumberOfRooms);
		world.rainCycle.Update();
		world.fliesWorldAI.Update();
	}

	public static void LoadAbstractRoom(World world, string roomName, AbstractRoom room, RainWorldGame.SetupValues setupValues)
	{
		if (room.altFileName != null)
		{
			roomName = room.altFileName;
		}
		roomName = RoomNameManipulator(roomName, world.game);
		string[] roomText = File.ReadAllLines(FindRoomFile(roomName, includeRootDirectory: false, ".txt"));
		bool flag = RoomPreprocessor.VersionFix(ref roomText);
		if (int.Parse(roomText[9].Split('|')[0], NumberStyles.Any, CultureInfo.InvariantCulture) >= world.preProcessingGeneration)
		{
			room.InitNodes(RoomPreprocessor.StringToConnMap(roomText[9]), roomText[1]);
		}
		else
		{
			roomText = RoomPreprocessor.PreprocessRoom(room, roomText, world, setupValues, world.preProcessingGeneration);
			flag = true;
		}
		if (flag)
		{
			File.WriteAllLines(FindRoomFile(roomName, includeRootDirectory: false, ".txt"), roomText);
		}
	}

	public static string RoomNameManipulator(string roomName, RainWorldGame game)
	{
		return roomName;
	}

	public static string MapNameManipulator(string mapName, RainWorldGame game)
	{
		Region[] array = ((game != null) ? game.overWorld.regions : Region.LoadAllRegions(null, null));
		foreach (Region region in array)
		{
			if (region.name == mapName)
			{
				if (region.regionParams.mapName == null)
				{
					break;
				}
				return region.regionParams.mapName;
			}
		}
		return mapName;
	}

	public static string FindRoomFile(string roomName, bool includeRootDirectory, string additionalAppend, bool showWarning = true)
	{
		string text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0] + "-Rooms" + Path.DirectorySeparatorChar + roomName + additionalAppend);
		if (File.Exists(text))
		{
			if (includeRootDirectory)
			{
				return "file:///" + text;
			}
			return text;
		}
		text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + roomName + additionalAppend);
		if (Regex.Split(roomName, "_")[0].ToUpper() == "GATE" && File.Exists(text))
		{
			if (includeRootDirectory)
			{
				return "file:///" + text;
			}
			return text;
		}
		text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "gate_shelters" + Path.DirectorySeparatorChar + roomName + additionalAppend);
		if (File.Exists(text))
		{
			if (includeRootDirectory)
			{
				return "file:///" + text;
			}
			return text;
		}
		text = AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + roomName + additionalAppend);
		if (File.Exists(text))
		{
			if (includeRootDirectory)
			{
				return "file:///" + text;
			}
			return text;
		}
		if (ModManager.MSC && roomName.ToLowerInvariant().Contains("challenge"))
		{
			text = AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + "Challenges" + Path.DirectorySeparatorChar + roomName + additionalAppend);
			if (File.Exists(text))
			{
				if (includeRootDirectory)
				{
					return "file:///" + text;
				}
				return text;
			}
		}
		return null;
	}

	public void SpawnerStabilityCheck(World.CreatureSpawner spawner)
	{
		if (!RainWorld.ShowLogs)
		{
			return;
		}
		if (spawner.den.room != world.offScreenDen.index && (spawner.den.room < world.firstRoomIndex || spawner.den.room >= world.firstRoomIndex + world.NumberOfRooms))
		{
			CreatureTemplate.Type standardGroundCreature = CreatureTemplate.Type.StandardGroundCreature;
			if (spawner is World.SimpleSpawner)
			{
				standardGroundCreature = (spawner as World.SimpleSpawner).creatureType;
				Custom.LogWarning($"ERROR SPAWNER IN ROOM NOT LOADED BY REGION'S WORLD FILE. creature: {standardGroundCreature}");
			}
			if (spawner is World.Lineage)
			{
				standardGroundCreature = (spawner as World.Lineage).CurrentType((game.session as StoryGameSession).saveState);
				Custom.LogWarning($"ERROR SPAWNER LINEAGE IN ROOM NOT LOADED BY REGION'S WORLD FILE. creature: {standardGroundCreature}");
			}
		}
		else if (spawner.den.abstractNode >= world.GetAbstractRoom(spawner.den.room).nodes.Length || world.GetAbstractRoom(spawner.den.room).GetNode(spawner.den).type != AbstractRoomNode.Type.Den)
		{
			CreatureTemplate.Type type = CreatureTemplate.Type.StandardGroundCreature;
			if (spawner is World.SimpleSpawner)
			{
				type = (spawner as World.SimpleSpawner).creatureType;
			}
			if (spawner is World.Lineage)
			{
				type = (spawner as World.Lineage).CurrentType((game.session as StoryGameSession).saveState);
			}
			if (spawner.den.abstractNode >= world.GetAbstractRoom(spawner.den.room).nodes.Length || world.GetAbstractRoom(spawner.den.room).GetNode(spawner.den).type != AbstractRoomNode.Type.GarbageHoles || type != CreatureTemplate.Type.GarbageWorm)
			{
				Custom.LogWarning($"ERROR, SPAWNER IN DEN THAT DOESNT EXIST! creature:{type} room:{world.GetAbstractRoom(spawner.den.room).name} node:{spawner.den.abstractNode}/{world.GetAbstractRoom(spawner.den.room).nodes.Length}");
			}
		}
		else
		{
			if (!(spawner is World.Lineage))
			{
				return;
			}
			CreatureTemplate.Type type2 = (spawner as World.Lineage).CurrentType((game.session as StoryGameSession).saveState);
			for (int i = 0; i < (spawner as World.Lineage).progressionChances.Length; i++)
			{
				if ((spawner as World.Lineage).progressionChances[i] == 0f && i < (spawner as World.Lineage).progressionChances.Length - 1)
				{
					Custom.LogWarning($"ERROR, SPAWNER LINEAGE ENDS PREMATURELY creature:{type2} room:{world.GetAbstractRoom(spawner.den.room).name} node:{spawner.den.abstractNode}/{world.GetAbstractRoom(spawner.den.room).nodes.Length}");
					break;
				}
				if ((spawner as World.Lineage).progressionChances[i] != 0f && i == (spawner as World.Lineage).progressionChances.Length - 1)
				{
					Custom.LogWarning($"ERROR, SPAWNER LINEAGE ENDS WITH NOT 0% CHANCE creature:{type2} room:{world.GetAbstractRoom(spawner.den.room).name} node:{spawner.den.abstractNode}/{world.GetAbstractRoom(spawner.den.room).nodes.Length}");
					break;
				}
			}
		}
	}

	public bool OverseerSpawnConditions(SlugcatStats.Name character)
	{
		if (world.region == null)
		{
			return false;
		}
		bool guideOverseerDead = (game.session as StoryGameSession).saveState.guideOverseerDead;
		bool angryWithPlayer = (game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.angryWithPlayer;
		bool flag = UnityEngine.Random.value < world.region.regionParams.playerGuideOverseerSpawnChance;
		if (character == SlugcatStats.Name.Red)
		{
			return false;
		}
		if (ModManager.Watcher && character == WatcherEnums.SlugcatStatsName.Watcher)
		{
			return false;
		}
		if (ModManager.MSC)
		{
			if (character == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				if (!guideOverseerDead && world.region.name != "SS" && world.region.name != "DM")
				{
					return (game.session as StoryGameSession).saveState.miscWorldSaveData.SSaiConversationsHad <= 0;
				}
				return false;
			}
			if (character == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				bool flag2 = (game.session as StoryGameSession).saveState.cycleNumber > 9 && (game.session as StoryGameSession).saveState.miscWorldSaveData.SSaiConversationsHad <= 0 && (game.session as StoryGameSession).saveState.cycleNumber < 25;
				return !guideOverseerDead && !angryWithPlayer && flag && flag2;
			}
			if (character == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				bool flag3 = world.region.name == "OE";
				return !guideOverseerDead && !angryWithPlayer && flag && flag3;
			}
			if (character == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				bool flag4 = (game.session as StoryGameSession).saveState.miscWorldSaveData.SSaiConversationsHad > 0;
				if (world.region.name == "RM" && !(game.session as StoryGameSession).saveState.miscWorldSaveData.pebblesEnergyTaken)
				{
					flag = true;
				}
				return !guideOverseerDead && !angryWithPlayer && flag && flag4;
			}
			if (character == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return false;
			}
		}
		return !guideOverseerDead && !angryWithPlayer && flag;
	}

	public void CreatingAbstractRoomsThread()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
		try
		{
			while (cntr < roomAdder.Count)
			{
				CreatingAbstractRooms();
				cntr++;
			}
		}
		finally
		{
			creating_abstract_rooms_finished = true;
		}
	}

	public void FindingCreaturesThread()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
		try
		{
			while (fccntr <= endOfCreatures)
			{
				FindingCreatures();
				fccntr++;
			}
		}
		finally
		{
			finding_creatures_done = true;
		}
	}
}
