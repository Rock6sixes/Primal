using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;
using Watcher;

public class MapExporter
{
	public interface IJsonObject
	{
		Dictionary<string, object> ToJson();
	}

	public sealed class SlugcatFile : IJsonObject
	{
		public Dictionary<string, object> scugs = new Dictionary<string, object>();

		public void AddCurrentSlugcat(RainWorldGame game)
		{
			scugs[game.StoryCharacter.value.ToLower()] = new Slugcat(game);
		}

		public Dictionary<string, object> ToJson()
		{
			return scugs;
		}
	}

	public sealed class Slugcat : IJsonObject
	{
		public string startingRegion;

		public string startingRoom;

		public Dictionary<string, string> regions;

		public Slugcat(RainWorldGame game)
		{
			SaveState saveState = (SaveState)FormatterServices.GetUninitializedObject(typeof(SaveState));
			saveState.progression = game.rainWorld.progression;
			saveState.progression.rainWorld.safariMode = false;
			saveState.saveStateNumber = game.StoryCharacter;
			saveState.setDenPosition();
			saveState.progression.rainWorld.safariMode = true;
			startingRoom = saveState.denPosition;
			if (startingRoom.StartsWith("GATE_"))
			{
				if (!(startingRoom == "GATE_OE_SU"))
				{
					throw new Exception("Unknown starting region from room " + startingRoom + " for slugcat " + game.StoryCharacter.value);
				}
				startingRegion = "SU";
			}
			else
			{
				startingRegion = startingRoom.Split('_')[0];
			}
			regions = new Dictionary<string, string>();
			string[] directories = Directory.GetDirectories(PathOfSlugcat(game.StoryCharacter.value));
			for (int i = 0; i < directories.Length; i++)
			{
				string fileName = Path.GetFileName(directories[i]);
				regions[fileName] = Region.GetRegionFullName(fileName, game.StoryCharacter);
			}
		}

		public Dictionary<string, object> ToJson()
		{
			return new Dictionary<string, object>
			{
				{ "startingRegion", startingRegion },
				{ "startingRoom", startingRoom },
				{ "regions", regions }
			};
		}
	}

	public static class ReusedRooms
	{
		public struct RegionData
		{
			public Dictionary<string, RoomSettings> settings;
		}

		public static Dictionary<string, RegionData> regions = new Dictionary<string, RegionData>();

		public static string SlugcatRoomsToUse(string slugcat, World world, List<AbstractRoom> validRooms)
		{
			slugcat = slugcat.ToLower();
			string text = SlugcatFor(slugcat, world.name);
			string key = text + "#" + world.name;
			if (text == slugcat)
			{
				regions[key] = new RegionData
				{
					settings = validRooms.ToDictionary((AbstractRoom a) => a.name, Settings, StringComparer.InvariantCultureIgnoreCase)
				};
				return null;
			}
			if (!regions.TryGetValue(key, out var value))
			{
				Custom.LogWarning("NOT COPIED | Region settings are not stored for " + text + "/" + world.name + " coming from " + slugcat);
				return null;
			}
			if (value.settings.Count != validRooms.Count)
			{
				Custom.LogWarning("NOT COPIED | Different room count for " + world.name + " in " + text + " and " + slugcat);
				return null;
			}
			foreach (AbstractRoom validRoom in validRooms)
			{
				if (!value.settings.TryGetValue(validRoom.name, out var value2))
				{
					Custom.LogWarning("NOT COPIED | The room " + validRoom.name + " exists for " + slugcat + " but not " + text);
					return null;
				}
				if (!Identical(value2, Settings(validRoom)))
				{
					Custom.LogWarning("NOT COPIED | The room " + validRoom.name + " is different for " + slugcat + " and " + text);
					return null;
				}
			}
			Custom.Log("Copying rooms from " + text + " to " + slugcat + "/" + world.name);
			return text;
		}

		public static RoomSettings Settings(AbstractRoom a)
		{
			return new RoomSettings(a.name, a.world.region, template: false, firstTemplate: false, a.world.game.TimelinePoint, a.world.game);
		}

		public static string SlugcatFor(string slugcat, string region)
		{
			region = region.ToLower();
			if (region.Length == 4 && region.StartsWith("w"))
			{
				return "watcher";
			}
			if (slugcat == "watcher" && ((region != null && region == "su") || (region != null && region == "hi") || (region != null && region == "lf") || (region != null && region == "cc") || (region != null && region == "sh")))
			{
				return "watcher";
			}
			if ((region != null && region == "lc") || (region != null && region == "lm") || (region != null && region == "gw" && slugcat == "spear"))
			{
				return "artificer";
			}
			if ((region != null && region == "cl") || (region != null && region == "ug"))
			{
				return "saint";
			}
			if (region != null && region == "rm")
			{
				return "rivulet";
			}
			return "white";
		}

		public static bool Identical(RoomSettings one, RoomSettings two)
		{
			if (one == two)
			{
				return true;
			}
			if (one.name.ToLowerInvariant().StartsWith("gate") && two.name.ToLowerInvariant().StartsWith("gate"))
			{
				return true;
			}
			if (one == null || two == null)
			{
				return false;
			}
			if (one.isAncestor != two.isAncestor || one.isTemplate != two.isTemplate || one.clds != two.clds || one.swAmp != two.swAmp || one.swLength != two.swLength || one.wAmp != two.wAmp || one.wetTerrain != two.wetTerrain || one.eColA != two.eColA || one.eColB != two.eColB || one.grm != two.grm || one.pal != two.pal || one.wtrRflctAlpha != two.wtrRflctAlpha)
			{
				return false;
			}
			if ((one.fadePalette != null || two.fadePalette != null) && (one.fadePalette == null || two.fadePalette == null || one.fadePalette.palette != two.fadePalette.palette || !one.fadePalette.fades.SequenceEqual(two.fadePalette.fades)))
			{
				return false;
			}
			if (!one.effects.Select((RoomSettings.RoomEffect e) => e.ToString()).SequenceEqual(two.effects.Select((RoomSettings.RoomEffect e) => e.ToString())))
			{
				return false;
			}
			if (!one.placedObjects.Select((PlacedObject p) => p.ToString()).SequenceEqual(two.placedObjects.Select((PlacedObject p) => p.ToString())))
			{
				return false;
			}
			return Identical(one.parent, two.parent);
		}
	}

	public sealed class RegionInfo : IJsonObject
	{
		public sealed class RoomEntry : IJsonObject
		{
			public string roomName;

			public Vector2 devPos;

			public Vector2 canPos;

			public int canLayer;

			public string subregion;

			public bool everParsed;

			public Vector2 warpPos;

			public string warpDestination;

			public bool rippleWarp;

			public Vector2 spinningTopPos;

			public string spinningTopDestination;

			public bool spinningTopRipple;

			public Vector2 dynamicWarpPos;

			public float dynamicWarpRequirement;

			public bool dynamicWarpBad;

			public Vector2 weaverPos;

			public Vector2 karmaFlowerPos;

			public bool karmaFlowerPresent;

			public Vector2[] cameras;

			public int[] size;

			public int[,][] tiles;

			public IntVector2[] nodes;

			public RoomEntry(string roomName)
			{
				this.roomName = roomName;
			}

			public void ParseEntry(string entry)
			{
				string[] array = Regex.Split(entry, "><");
				canPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
				canPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				devPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
				devPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				canLayer = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
				subregion = array[5];
				everParsed = true;
			}

			public void UpdateEntry(Room room)
			{
				cameras = room.cameraPositions;
				size = new int[2] { room.TileWidth, room.TileHeight };
				tiles = new int[room.TileWidth, room.TileHeight][];
				for (int i = 0; i < room.TileWidth; i++)
				{
					for (int j = 0; j < room.TileHeight; j++)
					{
						tiles[i, j] = new int[3]
						{
							(int)room.Tiles[i, j].Terrain,
							(room.Tiles[i, j].verticalBeam ? 2 : 0) + (room.Tiles[i, j].horizontalBeam ? 1 : 0),
							room.Tiles[i, j].shortCut
						};
					}
				}
				nodes = room.exitAndDenIndex;
				spinningTopDestination = "";
				spinningTopPos = Vector2.zero;
				spinningTopRipple = false;
				weaverPos = Vector2.zero;
				warpDestination = "";
				warpPos = Vector2.zero;
				rippleWarp = false;
				dynamicWarpRequirement = -1f;
				dynamicWarpPos = Vector2.zero;
				dynamicWarpBad = false;
				karmaFlowerPresent = false;
				karmaFlowerPos = Vector2.zero;
				for (int k = 0; k < room.roomSettings.placedObjects.Count; k++)
				{
					PlacedObject placedObject = room.roomSettings.placedObjects[k];
					if (ModManager.Watcher && placedObject.type == WatcherEnums.PlacedObjectType.SpinningTopSpot)
					{
						spinningTopDestination = (placedObject.data as SpinningTopData).destRoom;
						spinningTopRipple = (placedObject.data as SpinningTopData).rippleWarp;
						spinningTopPos = placedObject.pos;
					}
					if (ModManager.Watcher && placedObject.type == WatcherEnums.PlacedObjectType.WeaverSpot)
					{
						weaverPos = placedObject.pos;
					}
					if (placedObject.type == PlacedObject.Type.WarpPoint)
					{
						warpDestination = (((placedObject.data as WarpPoint.WarpPointData).destRoom == null) ? "" : (placedObject.data as WarpPoint.WarpPointData).destRoom);
						rippleWarp = (placedObject.data as WarpPoint.WarpPointData).rippleWarp;
						warpPos = placedObject.pos;
					}
					if (placedObject.type == PlacedObject.Type.DynamicWarpTarget)
					{
						dynamicWarpRequirement = (placedObject.data as DynamicWarpTargetData).rippleLevelRequirement;
						dynamicWarpBad = (placedObject.data as DynamicWarpTargetData).badWarp;
						dynamicWarpPos = placedObject.pos;
					}
					if (placedObject.type == PlacedObject.Type.KarmaFlower)
					{
						karmaFlowerPresent = true;
						karmaFlowerPos = placedObject.pos;
					}
				}
			}

			public Dictionary<string, object> ToJson()
			{
				return new Dictionary<string, object>
				{
					{ "roomName", roomName },
					{
						"canPos",
						Vec2arr(canPos)
					},
					{ "canLayer", canLayer },
					{
						"devPos",
						Vec2arr(devPos)
					},
					{ "subregion", subregion },
					{
						"cameras",
						(cameras != null) ? cameras.Select((Vector2 c) => Vec2arr(c)).ToArray() : null
					},
					{
						"nodes",
						(nodes != null) ? nodes.Select((IntVector2 n) => Intvec2arr(n)).ToArray() : null
					},
					{ "size", size },
					{ "tiles", tiles },
					{
						"warpPos",
						Vec2arr(warpPos)
					},
					{ "warpDestination", warpDestination },
					{
						"rippleWarp",
						rippleWarp ? "1" : "0"
					},
					{
						"spinningTopPos",
						Vec2arr(spinningTopPos)
					},
					{ "spinningTopDestination", spinningTopDestination },
					{
						"spinningTopRipple",
						spinningTopRipple ? "1" : "0"
					},
					{
						"dynamicWarpPos",
						Vec2arr(dynamicWarpPos)
					},
					{ "dynamicWarpRequirement", dynamicWarpRequirement },
					{
						"dynamicWarpBad",
						dynamicWarpBad ? "1" : "0"
					},
					{
						"karmaFlowerPos",
						Vec2arr(karmaFlowerPos)
					},
					{
						"karmaFlowerPresent",
						karmaFlowerPresent ? "1" : "0"
					},
					{
						"weaverPos",
						Vec2arr(weaverPos)
					}
				};
			}
		}

		public sealed class ConnectionEntry : IJsonObject
		{
			public string roomA;

			public string roomB;

			public IntVector2 posA;

			public IntVector2 posB;

			public int dirA;

			public int dirB;

			public ConnectionEntry(string entry)
			{
				string[] array = Regex.Split(entry, ",");
				roomA = array[0];
				roomB = array[1];
				posA = new IntVector2(int.Parse(array[2]), int.Parse(array[3]));
				posB = new IntVector2(int.Parse(array[4]), int.Parse(array[5]));
				dirA = int.Parse(array[6]);
				dirB = int.Parse(array[7]);
			}

			public Dictionary<string, object> ToJson()
			{
				return new Dictionary<string, object>
				{
					{ "roomA", roomA },
					{ "roomB", roomB },
					{
						"posA",
						Intvec2arr(posA)
					},
					{
						"posB",
						Intvec2arr(posB)
					},
					{ "dirA", dirA },
					{ "dirB", dirB }
				};
			}
		}

		public Dictionary<string, RoomEntry> rooms;

		public List<ConnectionEntry> connections;

		public string acronym;

		public List<Color> fgcolors;

		public List<Color> bgcolors;

		public List<Color> sccolors;

		public HashSet<string> worldSpawns;

		public string copyRooms;

		public RegionInfo(World world)
		{
			acronym = world.name;
			rooms = new Dictionary<string, RoomEntry>();
			connections = new List<ConnectionEntry>();
			fgcolors = new List<Color>();
			bgcolors = new List<Color>();
			sccolors = new List<Color>();
			worldSpawns = new HashSet<string>();
			LoadMapConfig(world);
			LoadSpawns(world);
		}

		public RoomEntry GetOrCreateRoomEntry(string name)
		{
			if (!rooms.TryGetValue(name, out var value))
			{
				return rooms[name] = new RoomEntry(name);
			}
			return value;
		}

		public void LoadMapConfig(World world)
		{
			string text = AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{world.name}{Path.DirectorySeparatorChar}map_{world.name}-{world.game.GetStorySession.saveState.saveStateNumber}.txt");
			if (!File.Exists(text))
			{
				text = AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{world.name}{Path.DirectorySeparatorChar}map_{world.name}.txt");
			}
			if (!File.Exists(text))
			{
				Custom.LogWarning($"No map data for {world.game.StoryCharacter}/{world.name} at {text}");
				return;
			}
			Custom.Log($"Found map data for {world.game.StoryCharacter}/{world.name} at {text}");
			string[] array = File.ReadAllLines(text);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = Regex.Split(array[i], ": ");
				string text2 = array2[0];
				if (text2 == "Connection")
				{
					connections.Add(new ConnectionEntry(array2[1]));
				}
				else if (!HiddenRoom(world.GetAbstractRoom(text2)))
				{
					GetOrCreateRoomEntry(text2).ParseEntry(array2[1]);
				}
			}
		}

		public void LoadSpawns(World world)
		{
			string name = world.region.name;
			string text = AssetManager.ResolveFilePath("world/" + name + "/world_" + name + ".txt");
			if (File.Exists(text))
			{
				AssimilateCreatures(File.ReadAllLines(text));
				return;
			}
			Custom.LogImportant("WORLD FILE DOES NOT EXIST: " + text);
		}

		public void AssimilateCreatures(IEnumerable<string> raw)
		{
			bool flag = false;
			foreach (string item in raw)
			{
				if (item == "CREATURES")
				{
					flag = true;
				}
				else if (item == "END CREATURES")
				{
					flag = false;
				}
				else if (flag && !string.IsNullOrEmpty(item) && !item.StartsWith("//"))
				{
					worldSpawns.Add(item);
				}
			}
		}

		public static float[] Vec2arr(Vector2 vec)
		{
			return new float[2] { vec.x, vec.y };
		}

		public static float[] Vec2arr(Vector3 vec)
		{
			return new float[3] { vec.x, vec.y, vec.z };
		}

		public static int[] Intvec2arr(IntVector2 vec)
		{
			return new int[2] { vec.x, vec.y };
		}

		public void UpdateRoom(Room room)
		{
			GetOrCreateRoomEntry(room.abstractRoom.name).UpdateEntry(room);
		}

		public Dictionary<string, object> ToJson()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				["acronym"] = acronym,
				["fgcolors"] = fgcolors.Select((Color s) => Vec2arr((Vector3)(Vector4)s)).ToList(),
				["bgcolors"] = bgcolors.Select((Color s) => Vec2arr((Vector3)(Vector4)s)).ToList(),
				["sccolors"] = sccolors.Select((Color s) => Vec2arr((Vector3)(Vector4)s)).ToList()
			};
			if (copyRooms == null)
			{
				dictionary["rooms"] = rooms;
				dictionary["connections"] = connections;
			}
			else
			{
				dictionary["copyRooms"] = copyRooms;
			}
			dictionary["spawns"] = worldSpawns.ToArray();
			return dictionary;
		}

		public void LogPalette(RoomPalette currentPalette)
		{
			Color pixel = currentPalette.texture.GetPixel(0, 0);
			Color pixel2 = currentPalette.texture.GetPixel(0, 7);
			Color shortCutSymbol = currentPalette.shortCutSymbol;
			fgcolors.Add(pixel);
			bgcolors.Add(pixel2);
			sccolors.Add(shortCutSymbol);
		}
	}

	public static string[] captureSpecific = new string[0];

	public static bool mapExporterActive = false;

	public static bool screenshots = true;

	public static IEnumerator captureTask;

	public static bool NotHiddenRoom(AbstractRoom room)
	{
		return !HiddenRoom(room);
	}

	public static bool HiddenRoom(AbstractRoom room)
	{
		return false;
	}

	public static void RoomCamera_DrawUpdate(RoomCamera self)
	{
	}

	public static bool Active()
	{
		return false;
	}

	public static void BrokenAntiGravity_ctor(AntiGravity.BrokenAntiGravity self)
	{
	}

	public static void GateKarmaGlyph_DrawSprites(GateKarmaGlyph self, RoomCamera.SpriteLeaser sLeaser)
	{
	}

	public static void Ghost_Update(Ghost self)
	{
	}

	public static void RainWorld_LoadSetupValues(RainWorldGame.SetupValues setup)
	{
	}

	public static void VoidSpawnGraphics_DrawSprites(VoidSpawnGraphics self, RoomCamera.SpriteLeaser sLeaser)
	{
	}

	public static void Room_Loaded(Room self)
	{
	}

	public static void WorldLoader_ctor(WorldLoader self, List<string> lines)
	{
	}

	public static void RainWorldGame_ctor_pre(RainWorldGame self)
	{
	}

	public static void RainWorldGame_ctor_post(RainWorldGame self)
	{
	}

	public static void RainWorldGame_Update(RainWorldGame self)
	{
	}

	public static string PathOfSlugcat(string slugcat)
	{
		return Path.Combine(Custom.LegacyRootFolderDirectory(), "Documents", "Interactive Map", "py-input", slugcat.ToLower());
	}

	public static string PathOfRegion(string slugcat, string region)
	{
		return Directory.CreateDirectory(Path.Combine(PathOfSlugcat(slugcat), region.ToLower())).FullName;
	}

	public static string PathOfSlugcatData()
	{
		return Path.Combine(Path.Combine(Custom.LegacyRootFolderDirectory(), "Documents", "Interactive Map", "py-input", "slugcats.json"));
	}

	public static string PathOfMetadata(string slugcat, string region)
	{
		return Path.Combine(PathOfRegion(slugcat, region), "metadata.json");
	}

	public static string PathOfScreenshot(string slugcat, string region, string room, int num)
	{
		return $"{Path.Combine(PathOfRegion(slugcat, region), room.ToLower())}_{num}.png";
	}

	public static int ScugPriority(string slugcat)
	{
		return slugcat switch
		{
			"white" => 10, 
			"artificer" => 9, 
			"saint" => 8, 
			"rivulet" => 7, 
			_ => 0, 
		};
	}

	public static IEnumerator CaptureTask(RainWorldGame game)
	{
		Custom.Log("capture task start");
		UnityEngine.Random.InitState(0);
		while (game.cameras[0].room == null || !game.cameras[0].room.ReadyForPlayer)
		{
			yield return null;
		}
		for (int j = 0; j < 40; j++)
		{
			yield return null;
		}
		game.cameras[0].room.abstractRoom.Abstractize();
		SlugcatFile slugcatsJson = new SlugcatFile();
		string[] array = captureSpecific;
		if (array != null && array.Length != 0)
		{
			string[] array2 = captureSpecific;
			foreach (string text in array2)
			{
				SlugcatStats.Name saveStateNumber = new SlugcatStats.Name(text.Split(';')[0]);
				game.GetStorySession.saveStateNumber = saveStateNumber;
				game.GetStorySession.saveState.saveStateNumber = saveStateNumber;
				slugcatsJson.AddCurrentSlugcat(game);
				foreach (object item in CaptureRegion(game, text.Split(';')[1]))
				{
					yield return item;
				}
			}
		}
		else if (ModManager.Watcher)
		{
			SlugcatStats.Name watcher = WatcherEnums.SlugcatStatsName.Watcher;
			game.GetStorySession.saveStateNumber = watcher;
			game.GetStorySession.saveState.saveStateNumber = watcher;
			Region[] regions = game.overWorld.regions;
			foreach (Region region in regions)
			{
				if (!(ReusedRooms.SlugcatFor("watcher", region.name) == "watcher"))
				{
					continue;
				}
				foreach (object item2 in CaptureRegion(game, region.name))
				{
					yield return item2;
				}
			}
			slugcatsJson.AddCurrentSlugcat(game);
		}
		else
		{
			foreach (string item3 in ExtEnum<SlugcatStats.Name>.values.entries.OrderByDescending(ScugPriority))
			{
				SlugcatStats.Name name = new SlugcatStats.Name(item3);
				if (SlugcatStats.HiddenOrUnplayableSlugcat(name))
				{
					continue;
				}
				game.GetStorySession.saveStateNumber = name;
				game.GetStorySession.saveState.saveStateNumber = name;
				foreach (string item4 in SlugcatStats.getSlugcatStoryRegions(name).Concat(SlugcatStats.getSlugcatOptionalRegions(name)))
				{
					foreach (object item5 in CaptureRegion(game, item4))
					{
						yield return item5;
					}
				}
				slugcatsJson.AddCurrentSlugcat(game);
			}
		}
		File.WriteAllText(PathOfSlugcatData(), Json.Serialize(slugcatsJson));
		Custom.Log("capture task done!");
		Application.Quit();
	}

	public static IEnumerable CaptureRegion(RainWorldGame game, string region)
	{
		SlugcatStats.Name slugcat = game.StoryCharacter;
		UnityEngine.Random.InitState(0);
		game.overWorld.LoadWorld(region, slugcat, singleRoomWorld: false);
		Custom.Log($"Loaded {slugcat}/{region}");
		Directory.CreateDirectory(PathOfRegion(slugcat.value, region));
		RegionInfo mapContent = new RegionInfo(game.world);
		List<AbstractRoom> list = game.world.abstractRooms.ToList();
		list.RemoveAll(HiddenRoom);
		list.RemoveAll((AbstractRoom r) => r.offScreenDen);
		string text = ReusedRooms.SlugcatRoomsToUse(slugcat.value, game.world, list);
		if (text != null)
		{
			mapContent.copyRooms = text;
		}
		else
		{
			foreach (AbstractRoom item in list)
			{
				foreach (object item2 in CaptureRoom(item, mapContent))
				{
					yield return item2;
				}
			}
		}
		File.WriteAllText(PathOfMetadata(slugcat.value, region), Json.Serialize(mapContent));
		Custom.Log("capture task done with " + region);
	}

	public static IEnumerable CaptureRoom(AbstractRoom room, RegionInfo regionContent)
	{
		RainWorldGame game = room.world.game;
		game.overWorld.activeWorld.loadingRooms.Clear();
		UnityEngine.Random.InitState(0);
		game.overWorld.activeWorld.ActivateRoom(room);
		if (game.overWorld.activeWorld.loadingRooms.Count > 0 && game.overWorld.activeWorld.loadingRooms[0].room == room.realizedRoom)
		{
			RoomPreparer roomPreparer = game.overWorld.activeWorld.loadingRooms[0];
			while (!roomPreparer.done)
			{
				roomPreparer.Update();
			}
		}
		while (room.realizedRoom.loadingProgress < 3 || room.realizedRoom.waitToEnterAfterFullyLoaded >= 1)
		{
			room.realizedRoom.Update();
		}
		Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>();
		dictionary.Add("SU_B13", new int[1] { 2 });
		dictionary.Add("GW_S08", new int[1] { 2 });
		dictionary.Add("SL_C01", new int[2] { 4, 5 });
		if (dictionary.TryGetValue(room.name, out var value))
		{
			List<Vector2> list = room.realizedRoom.cameraPositions.ToList();
			for (int num = value.Length - 1; num >= 0; num--)
			{
				list.RemoveAt(value[num] - 1);
			}
			room.realizedRoom.cameraPositions = list.ToArray();
		}
		yield return null;
		UnityEngine.Random.InitState(0);
		game.cameras[0].MoveCamera(room.realizedRoom, 0);
		game.cameras[0].virtualMicrophone.AllQuiet();
		while (game.cameras[0].AboutToSwitchRoom)
		{
			yield return null;
		}
		UnityEngine.Random.InitState(0);
		regionContent.UpdateRoom(room.realizedRoom);
		for (int i = 0; i < room.realizedRoom.cameraPositions.Length; i++)
		{
			UnityEngine.Random.InitState(room.name.GetHashCode());
			game.cameras[0].MoveCamera(i);
			game.cameras[0].virtualMicrophone.AllQuiet();
			while (game.cameras[0].AboutToSwitchCameraPos)
			{
				yield return null;
			}
			if (screenshots)
			{
				for (int j = 0; j < 20; j++)
				{
					yield return null;
				}
				string text = PathOfScreenshot(game.StoryCharacter.value, room.world.name, room.name, i);
				if (!File.Exists(text))
				{
					ScreenCapture.CaptureScreenshot(text);
				}
			}
			regionContent.LogPalette(game.cameras[0].currentPalette);
			yield return null;
		}
		UnityEngine.Random.InitState(0);
		room.Abstractize();
		yield return null;
	}
}
