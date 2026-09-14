using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public static class ArenaCreatureSpawner
{
	public abstract class Spawner
	{
		public float spawnChance = 1f;

		public bool spawn = true;

		public string groupString;

		public string symbolString;

		public string invSymbolString;

		public CreatureGroup group;

		public SpawnSymbol symbol;

		public SpawnSymbol invSymbol;

		public int ID = UnityEngine.Random.Range(0, 10000);

		public Spawner()
		{
		}

		public void Disable()
		{
			spawn = false;
			spawnChance = 0f;
		}
	}

	public class CritterSpawnData : Spawner
	{
		public CreatureTemplate.Type type;

		public List<int> dens;

		public string spawnDataString;

		public bool rare;

		public CritterSpawnData(CreatureTemplate.Type type)
		{
			this.type = type;
			dens = new List<int>();
		}
	}

	public class DenGroup
	{
		public string name;

		public List<int> dens;

		public DenGroup(string name)
		{
			this.name = name;
			dens = new List<int>();
		}
	}

	public class CreatureGroup : Spawner
	{
		public string name;

		public List<Spawner> connectedSpawners;

		public CreatureGroup(string name)
		{
			this.name = name;
			connectedSpawners = new List<Spawner>();
		}

		public bool AnyConnectedSpawnersActive()
		{
			for (int i = 0; i < connectedSpawners.Count; i++)
			{
				if (connectedSpawners[i].spawn)
				{
					return true;
				}
			}
			return false;
		}
	}

	public class FilterSpawner : CritterSpawnData
	{
		public string name;

		public bool rolled;

		public FilterSpawner(string name)
			: base(null)
		{
			this.name = name;
		}
	}

	public class SpawnSymbol
	{
		public string name;

		public List<string> possibleOutcomes;

		public bool inverse;

		public string decidedOutcome;

		public List<Spawner> connectedSpawners;

		public SpawnSymbol(string name, string firstPossoutcome, bool inv)
		{
			this.name = name;
			possibleOutcomes = new List<string> { firstPossoutcome };
			inverse = inv;
			connectedSpawners = new List<Spawner>();
		}

		public bool AnyConnectedSpawnersActiveUnderCurrentRoll()
		{
			for (int i = 0; i < connectedSpawners.Count; i++)
			{
				if (connectedSpawners[i].spawn)
				{
					if (!inverse && connectedSpawners[i].symbolString.Substring(1, 1) == decidedOutcome)
					{
						return true;
					}
					if (inverse && connectedSpawners[i].invSymbolString.Substring(1, 1) != decidedOutcome)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public static AGLog<FromStaticClass> Log = new AGLog<FromStaticClass>();

	public static bool allowLockedCreatures;

	public static void SpawnArenaCreatures(RainWorldGame game, ArenaSetup.GameTypeSetup.WildLifeSetting wildLifeSetting, ref List<AbstractCreature> availableCreatures, ref MultiplayerUnlocks unlocks)
	{
		string path = AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + game.world.GetAbstractRoom(0).name + "_Arena.txt");
		if (!File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		float num = 1f;
		if (wildLifeSetting == ArenaSetup.GameTypeSetup.WildLifeSetting.Off)
		{
			return;
		}
		if (wildLifeSetting == ArenaSetup.GameTypeSetup.WildLifeSetting.Low)
		{
			num = 0.5f;
		}
		else if (wildLifeSetting == ArenaSetup.GameTypeSetup.WildLifeSetting.Medium)
		{
			num = 1f;
		}
		else if (wildLifeSetting == ArenaSetup.GameTypeSetup.WildLifeSetting.High)
		{
			num = 1.5f;
		}
		AbstractRoom abstractRoom = game.world.GetAbstractRoom(0);
		bool flag = game.devToolsActive || ModManager.DevTools;
		if (flag)
		{
			Custom.LogWarning("load arena [" + abstractRoom.name + "]");
		}
		List<Spawner> list = new List<Spawner>();
		List<CritterSpawnData> spawnList = new List<CritterSpawnData>();
		List<CreatureGroup> list2 = new List<CreatureGroup>();
		List<SpawnSymbol> symbols = new List<SpawnSymbol>();
		List<DenGroup> list3 = new List<DenGroup>();
		float num2 = -1f;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length <= 2 || !(array[i].Substring(0, 2) != "//"))
			{
				continue;
			}
			string[] array2 = Regex.Split(array[i], " - ");
			int num3 = 1;
			Spawner spawner = null;
			switch (array2[0])
			{
			case "Creature":
			{
				CreatureTemplate.Type type = WorldLoader.CreatureTypeFromString(array2[1]);
				if (type != null)
				{
					spawner = new CritterSpawnData(type);
				}
				else if (flag)
				{
					Custom.LogWarning("creature not rec. " + array2[1]);
				}
				break;
			}
			case "CreatureGroup":
				spawner = new CreatureGroup(array2[1]);
				break;
			case "Filter":
				if (game.session is CompetitiveGameSession competitiveGameSession)
				{
					spawner = new FilterSpawner(array2[1]);
					competitiveGameSession.spawnFilters.Add(spawner as FilterSpawner);
				}
				break;
			case "DenGroup":
			{
				list3.Add(new DenGroup(array2[1]));
				string[] array3 = array2[2].Split(',');
				for (int j = 0; j < array3.Length; j++)
				{
					if (int.TryParse(array3[j], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
					{
						list3[list3.Count - 1].dens.Add(result);
						if (flag)
						{
							if (result < 0 || result >= abstractRoom.nodes.Length)
							{
								Custom.LogWarning($"DenGroup line [{i}] den is not in room. DenGroup: [{array2[1]}], den: [{result}], room nodes: [{abstractRoom.nodes.Length}]");
							}
							else if (abstractRoom.nodes[result].type != AbstractRoomNode.Type.Den)
							{
								Custom.LogWarning($"DenGroup line [{i}] den is wrong node type. DenGroup: [{array2[1]}], den: [{result}], node type: [{abstractRoom.nodes[result].type}]");
							}
						}
					}
					else if (flag)
					{
						Custom.LogWarning($"DenGroup line [{i}] is not an int. DenGroup: [{array2[1]}], den: [{array3[j]}]");
					}
				}
				break;
			}
			case "PlayersGlow":
				game.GetArenaGameSession.playersGlowing = true;
				break;
			case "GoalAmount":
				num2 = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			default:
				if (flag && !string.IsNullOrWhiteSpace(array2[0]))
				{
					Custom.LogWarning($"line [{i}] is unrecognized type: [{array[i]}]");
				}
				break;
			}
			if (spawner == null)
			{
				continue;
			}
			for (int k = 2; k < array2.Length; k++)
			{
				string[] array4 = Regex.Split(array2[k], ":");
				switch (array4[0])
				{
				case "chance":
				{
					if (float.TryParse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result6))
					{
						spawner.spawnChance = result6;
					}
					else if (flag)
					{
						Custom.LogWarning($"creature line [{i}] chance is not a number: [{array2[k]}], full line: [{array[i]}]");
					}
					break;
				}
				case "RARE":
					if (spawner is CritterSpawnData)
					{
						(spawner as CritterSpawnData).rare = true;
					}
					else if (flag)
					{
						Custom.LogWarning($"creature group [{i}] can't be RARE! full line: [{array[i]}]");
					}
					break;
				case "group":
					spawner.groupString = array4[1];
					break;
				case "dens":
					if (spawner is CritterSpawnData critterSpawnData && !(spawner is FilterSpawner))
					{
						bool flag2 = true;
						for (int l = 0; l < list3.Count; l++)
						{
							if (list3[l].name == array4[1])
							{
								(spawner as CritterSpawnData).dens = list3[l].dens;
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							string[] array5 = array4[1].Split(',');
							for (int m = 0; m < array5.Length; m++)
							{
								if (int.TryParse(array5[m], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
								{
									(spawner as CritterSpawnData).dens.Add(result2);
									if (!flag)
									{
										continue;
									}
									if (result2 == -1)
									{
										if (critterSpawnData.type != null && !StaticWorld.GetCreatureTemplate(critterSpawnData.type).hibernateOffScreen)
										{
											Custom.LogWarning($"non offscreen creature spawning in offscreen den: [{result2}], full line: [{array[i]}]");
										}
									}
									else if (result2 < 0 || result2 >= abstractRoom.nodes.Length)
									{
										Custom.LogWarning($"den is not in room. den: [{result2}], room nodes: [{abstractRoom.nodes.Length}], full line: [{array[i]}]");
									}
									else if (result2 >= 0 && abstractRoom.nodes[result2].type != AbstractRoomNode.Type.Den)
									{
										Custom.LogWarning($"den is wrong node type. den: [{result2}], node type: [{abstractRoom.nodes[result2].type}], full line: [{array[i]}]");
									}
								}
								else if (flag)
								{
									Custom.LogWarning($"creature line [{i}] den is not an integer. [{array5[m]}], full line: [{array[i]}]");
								}
							}
						}
						if (flag && critterSpawnData.type != null && StaticWorld.GetCreatureTemplate(critterSpawnData.type).hibernateOffScreen && critterSpawnData.dens[0] != -1)
						{
							Custom.LogWarning("offscreen creature spawning in den: [" + array4[1] + "], full line: [" + array[i] + "]");
						}
					}
					else if (flag)
					{
						Custom.LogWarning($"creature group [{i}] can't have dens! full line: [{array[i]}]");
					}
					break;
				case "spawnDataString":
					if (spawner is CritterSpawnData && !(spawner is FilterSpawner))
					{
						(spawner as CritterSpawnData).spawnDataString = string.Join(":", array4.Skip(1));
					}
					else if (flag)
					{
						Custom.LogWarning($"creature group [{i}] can't have spawnDataString! full line: [{array[i]}]");
					}
					break;
				case "amount":
					if (spawner is CritterSpawnData && !(spawner is FilterSpawner))
					{
						if (array4[1] == "players")
						{
							num3 = game.GetArenaGameSession.arenaSitting.players.Count;
							break;
						}
						string[] array5 = array4[1].Split('-');
						int result4;
						int result5;
						if (array5.Length == 1)
						{
							if (int.TryParse(array5[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result3))
							{
								num3 = result3;
							}
							else if (flag)
							{
								Custom.LogWarning($"creature line [{i}] amount is not a number: [{array2[k]}], full line: [{array[i]}]");
							}
						}
						else if (int.TryParse(array5[0], NumberStyles.Any, CultureInfo.InvariantCulture, out result4) && int.TryParse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture, out result5))
						{
							num3 = UnityEngine.Random.Range(result4, result5);
						}
						else if (flag)
						{
							Custom.LogWarning($"creature line [{i}] amount is not a number: [{array2[k]}], full line: [{array[i]}]");
						}
					}
					else if (flag)
					{
						Custom.LogWarning($"creature group [{i}] can't have amount! full line: [{array[i]}]");
					}
					break;
				case "symbol":
					AddToSymbol(array4[1], inv: false, ref symbols);
					spawner.symbolString = array4[1];
					break;
				case "invSymbol":
					AddToSymbol(array4[1], inv: true, ref symbols);
					spawner.invSymbolString = array4[1];
					break;
				case "creature":
					if (spawner is FilterSpawner filterSpawner)
					{
						CreatureTemplate.Type type2 = WorldLoader.CreatureTypeFromString(array4[1]);
						if (type2 != null)
						{
							filterSpawner.type = type2;
						}
						else if (flag)
						{
							Custom.LogWarning("creature not rec. " + array4[1]);
						}
					}
					else if (flag)
					{
						Custom.LogWarning($"line [{i}] can only use creature param if it's a Filter! full line: [{array[i]}]");
					}
					break;
				default:
					if (flag)
					{
						Custom.LogWarning($"creature line [{i}] has unrecognized field: [{array4[0]}]");
					}
					break;
				}
			}
			if (flag && spawner is CritterSpawnData critterSpawnData2 && !(spawner is FilterSpawner) && critterSpawnData2.dens.Count < 1 && critterSpawnData2.type != null && !StaticWorld.GetCreatureTemplate(critterSpawnData2.type).hibernateOffScreen)
			{
				Custom.LogWarning($"creature line [{i}] has no dens assigned (and is not an offscreen creature), full line: [{array[i]}]");
			}
			if (num3 > 0)
			{
				if (spawner is CreatureGroup)
				{
					list2.Add(spawner as CreatureGroup);
				}
				else if (spawner is FilterSpawner)
				{
					spawnList.Add(spawner as CritterSpawnData);
				}
				else if (spawner is CritterSpawnData)
				{
					spawnList.Add(spawner as CritterSpawnData);
				}
				list.Add(spawner);
				spawner.spawn = true;
				for (int n = 1; n < num3; n++)
				{
					CritterSpawnData critterSpawnData3 = new CritterSpawnData((spawner as CritterSpawnData).type);
					critterSpawnData3.dens = (spawner as CritterSpawnData).dens;
					critterSpawnData3.groupString = (spawner as CritterSpawnData).groupString;
					critterSpawnData3.symbolString = (spawner as CritterSpawnData).symbolString;
					critterSpawnData3.invSymbolString = (spawner as CritterSpawnData).invSymbolString;
					critterSpawnData3.spawnChance = (spawner as CritterSpawnData).spawnChance;
					critterSpawnData3.spawnDataString = (spawner as CritterSpawnData).spawnDataString;
					critterSpawnData3.rare = (spawner as CritterSpawnData).rare;
					spawnList.Add(critterSpawnData3);
					list.Add(critterSpawnData3);
					critterSpawnData3.spawn = true;
				}
			}
		}
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			if (list[num4].symbolString != null)
			{
				for (int num5 = 0; num5 < symbols.Count; num5++)
				{
					if (list[num4].symbolString.Substring(0, 1) == symbols[num5].name)
					{
						list[num4].symbol = symbols[num5];
						symbols[num5].connectedSpawners.Add(list[num4]);
						break;
					}
				}
			}
			if (list[num4].invSymbolString != null)
			{
				for (int num6 = 0; num6 < symbols.Count; num6++)
				{
					if (list[num4].invSymbolString.Substring(0, 1) == symbols[num6].name)
					{
						list[num4].invSymbol = symbols[num6];
						symbols[num6].connectedSpawners.Add(list[num4]);
						break;
					}
				}
			}
			if (list[num4].groupString == null)
			{
				continue;
			}
			bool flag3 = false;
			for (int num7 = 0; num7 < list2.Count; num7++)
			{
				if (list[num4].groupString == list2[num7].name)
				{
					list[num4].group = list2[num7];
					list2[num7].connectedSpawners.Add(list[num4]);
					flag3 = true;
					break;
				}
			}
			if (flag && !flag3)
			{
				if (list[num4] is CritterSpawnData critterSpawnData4)
				{
					Custom.LogWarning($"creature spawner [{critterSpawnData4.type}] has group name [{critterSpawnData4.groupString}] that doesn't match any group");
				}
				else if (list[num4] is CreatureGroup creatureGroup)
				{
					Custom.LogWarning("creature spawner [" + creatureGroup.name + "] has group name [" + creatureGroup.groupString + "] that doesn't match any group");
				}
			}
		}
		float num8 = 0f;
		List<CreatureTemplate.Type> list4 = new List<CreatureTemplate.Type>();
		List<CreatureTemplate.Type> list5 = new List<CreatureTemplate.Type>();
		int num9 = 0;
		if (ModManager.MSC)
		{
			for (int num10 = 0; num10 < abstractRoom.creatures.Count; num10++)
			{
				if (IsMajorCreature(abstractRoom.creatures[num10].creatureTemplate.type))
				{
					num9++;
				}
			}
		}
		for (int num11 = 0; num11 < spawnList.Count; num11++)
		{
			float num12 = Mathf.Clamp01(spawnList[num11].spawnChance);
			if (spawnList[num11].group != null)
			{
				num12 *= Mathf.Clamp01(spawnList[num11].group.spawnChance);
			}
			if (spawnList[num11].symbol != null)
			{
				num12 *= 1f / (float)spawnList[num11].symbol.possibleOutcomes.Count;
			}
			if (spawnList[num11].invSymbol != null)
			{
				num12 *= 1f - 1f / (float)spawnList[num11].invSymbol.possibleOutcomes.Count;
			}
			bool flag4 = ModManager.ChallengeModule && allowLockedCreatures && (game.GetArenaGameSession.chMeta == null || game.GetArenaGameSession.chMeta.unlimitedDanger || !IsMajorCreature(spawnList[num11].type) || num9 == 0);
			if (unlocks.IsCreatureUnlockedForLevelSpawn(spawnList[num11].type) || flag4)
			{
				if (!list4.Contains(spawnList[num11].type))
				{
					list4.Add(spawnList[num11].type);
				}
			}
			else if (spawnList[num11] is FilterSpawner filterSpawner2)
			{
				if (filterSpawner2.type != null)
				{
					filterSpawner2.Disable();
					num12 *= 0f;
				}
			}
			else
			{
				if (!list5.Contains(spawnList[num11].type))
				{
					list5.Add(spawnList[num11].type);
				}
				CreatureTemplate.Type type3 = unlocks.RecursiveFallBackCritter(spawnList[num11].type);
				if (type3 != null)
				{
					spawnList[num11].type = type3;
					spawnList[num11].spawnChance = Mathf.Clamp01(spawnList[num11].spawnChance) * 0.01f;
					num12 *= 0.5f;
				}
				else
				{
					spawnList[num11].Disable();
					num12 *= 0f;
				}
			}
			num8 += num12;
		}
		float num13 = (float)list4.Count / (float)(list4.Count + list5.Count);
		float num14 = Mathf.InverseLerp(0.7f, 0.3f, num13);
		if (num2 > 0f)
		{
			num2 *= Mathf.Lerp(Mathf.InverseLerp(0.15f, 0.75f, num13), 1f, 0.5f) * num;
		}
		for (int num15 = 0; num15 < list2.Count; num15++)
		{
			if (UnityEngine.Random.value > list2[num15].spawnChance || !list2[num15].AnyConnectedSpawnersActive())
			{
				list2[num15].Disable();
			}
		}
		for (int num16 = 0; num16 < spawnList.Count; num16++)
		{
			if (spawnList[num16].rare && UnityEngine.Random.value > Mathf.Pow(spawnList[num16].spawnChance, Custom.LerpMap(num13, 0.35f, 0.85f, 0.5f, 0.05f)))
			{
				spawnList[num16].Disable();
			}
		}
		for (int num17 = 0; num17 < symbols.Count; num17++)
		{
			if (flag && symbols[num17].possibleOutcomes.Count < 2)
			{
				Custom.LogWarning("less than 2 possible outcomes for symbol [" + symbols[num17].name + "]");
			}
			if (symbols[num17].possibleOutcomes.Count == 0)
			{
				continue;
			}
			symbols[num17].decidedOutcome = symbols[num17].possibleOutcomes[UnityEngine.Random.Range(0, symbols[num17].possibleOutcomes.Count)];
			for (int num18 = 0; num18 < 10; num18++)
			{
				if (symbols[num17].AnyConnectedSpawnersActiveUnderCurrentRoll())
				{
					break;
				}
				if (symbols[num17].possibleOutcomes.Count == 0)
				{
					break;
				}
				symbols[num17].decidedOutcome = symbols[num17].possibleOutcomes[UnityEngine.Random.Range(0, symbols[num17].possibleOutcomes.Count)];
			}
		}
		for (int num19 = 0; num19 < list.Count; num19++)
		{
			if (list[num19].group != null && !list[num19].group.spawn)
			{
				list[num19].Disable();
			}
			else if (list[num19].symbol != null && list[num19].symbol.decidedOutcome != list[num19].symbolString.Substring(1, 1))
			{
				list[num19].Disable();
			}
			else if (list[num19].invSymbol != null && list[num19].invSymbol.decidedOutcome == list[num19].invSymbolString.Substring(1, 1))
			{
				list[num19].Disable();
			}
		}
		if (num2 > -1f)
		{
			num8 = Mathf.Lerp(num8, num2, 0.5f);
		}
		num8 *= num * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value);
		num8 = ((!(num2 > -1f)) ? (num8 + Mathf.Lerp(-1.2f, 1.2f, Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(num8, 2f, 10f, 0.25f, 3f)))) : (num8 + Mathf.Lerp(-1.2f, 1.2f, Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(num8, num2 - 2f, num2 + 2f, 0.25f, 2f)))));
		int num20 = 0;
		if (wildLifeSetting.Index != -1)
		{
			num20 = Custom.IntClamp(Mathf.RoundToInt(num8), wildLifeSetting.Index, 25);
		}
		List<CritterSpawnData> list6 = new List<CritterSpawnData>();
		for (int num21 = 0; num21 < num20; num21++)
		{
			CritterSpawnData critterSpawnData5 = WeightedRandom(spawnList);
			if (critterSpawnData5 is FilterSpawner filterSpawner3)
			{
				filterSpawner3.rolled = true;
				filterSpawner3.Disable();
			}
			else if (critterSpawnData5 != null)
			{
				critterSpawnData5.Disable();
				list6.Add(critterSpawnData5);
				if (num14 > 0f)
				{
					Diversify(ref spawnList, critterSpawnData5.type, num14);
				}
			}
		}
		int[] array6 = new int[abstractRoom.nodes.Length];
		for (int num22 = 0; num22 < list6.Count; num22++)
		{
			CritterSpawnData critterSpawnData6 = list6[num22];
			if (ModManager.ChallengeModule && game.GetArenaGameSession.chMeta != null && IsMajorCreature(critterSpawnData6.type))
			{
				num9++;
				if (num9 > 1 && !game.GetArenaGameSession.chMeta.unlimitedDanger)
				{
					continue;
				}
			}
			if (critterSpawnData6.dens.Count < 1 || critterSpawnData6.dens[0] == -1)
			{
				AbstractCreature abstractCreature = CreateAbstractCreature(game.world, critterSpawnData6.type, new WorldCoordinate(game.world.offScreenDen.index, -1, -1, 0), ref availableCreatures);
				if (abstractCreature != null)
				{
					abstractCreature.spawnData = critterSpawnData6.spawnDataString;
					abstractCreature.setCustomFlags();
					game.world.offScreenDen.AddEntity(abstractCreature);
				}
				continue;
			}
			int num23 = int.MaxValue;
			for (int num24 = 0; num24 < critterSpawnData6.dens.Count; num24++)
			{
				num23 = Math.Min(num23, array6[critterSpawnData6.dens[num24]]);
			}
			List<int> list7 = new List<int>();
			for (int num25 = 0; num25 < critterSpawnData6.dens.Count; num25++)
			{
				if (array6[critterSpawnData6.dens[num25]] <= num23)
				{
					list7.Add(critterSpawnData6.dens[num25]);
				}
			}
			if (list7.Count == 0)
			{
				continue;
			}
			int num26 = list7[UnityEngine.Random.Range(0, list7.Count)];
			array6[num26]++;
			if (StaticWorld.GetCreatureTemplate(critterSpawnData6.type).quantified)
			{
				abstractRoom.AddQuantifiedCreature(num26, critterSpawnData6.type, UnityEngine.Random.Range(7, 11));
				continue;
			}
			AbstractCreature abstractCreature2 = CreateAbstractCreature(game.world, critterSpawnData6.type, new WorldCoordinate(abstractRoom.index, -1, -1, num26), ref availableCreatures);
			if (abstractCreature2 == null)
			{
				continue;
			}
			abstractCreature2.spawnData = critterSpawnData6.spawnDataString;
			abstractCreature2.setCustomFlags();
			abstractRoom.MoveEntityToDen(abstractCreature2);
			if (abstractCreature2.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm)
			{
				for (int num27 = UnityEngine.Random.Range(0, 4); num27 >= 0; num27--)
				{
					AbstractCreature ent = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SmallNeedleWorm), null, new WorldCoordinate(abstractRoom.index, -1, -1, num26), game.GetNewID());
					abstractRoom.MoveEntityToDen(ent);
				}
			}
		}
	}

	public static CritterSpawnData WeightedRandom(List<CritterSpawnData> inputList)
	{
		float num = 0f;
		for (int i = 0; i < inputList.Count; i++)
		{
			num += inputList[i].spawnChance;
		}
		float num2 = UnityEngine.Random.value * num;
		for (int j = 0; j < inputList.Count; j++)
		{
			num2 -= inputList[j].spawnChance;
			if (num2 < 0f)
			{
				return inputList[j];
			}
		}
		return null;
	}

	public static void Diversify(ref List<CritterSpawnData> spawnList, CreatureTemplate.Type type, float diversify)
	{
		for (int i = 0; i < spawnList.Count; i++)
		{
			if (spawnList[i].type != type)
			{
				spawnList[i].spawnChance *= 1f + diversify * 10f;
			}
		}
	}

	public static AbstractCreature CreateAbstractCreature(World world, CreatureTemplate.Type critType, WorldCoordinate pos, ref List<AbstractCreature> availableCreatures)
	{
		for (int num = availableCreatures.Count - 1; num >= 0; num--)
		{
			if (availableCreatures[num].creatureTemplate.type == critType)
			{
				AbstractCreature abstractCreature = availableCreatures[num];
				availableCreatures.RemoveAt(num);
				for (int i = 0; i < 2; i++)
				{
					abstractCreature.state.CycleTick();
				}
				abstractCreature.pos = pos;
				abstractCreature.world = world;
				string creatureString = SaveState.AbstractCreatureToStringSingleRoomWorld(abstractCreature);
				return SaveState.AbstractCreatureFromString(world, creatureString, onlyInCurrentRegion: false);
			}
		}
		return new AbstractCreature(world, StaticWorld.GetCreatureTemplate(critType), null, pos, world.game.GetNewID());
	}

	public static void AddToSymbol(string symbolString, bool inv, ref List<SpawnSymbol> symbols)
	{
		for (int i = 0; i < symbols.Count; i++)
		{
			if (!(symbols[i].name == symbolString.Substring(0, 1)))
			{
				continue;
			}
			for (int j = 0; j < symbols[i].possibleOutcomes.Count; j++)
			{
				if (symbols[i].possibleOutcomes[j] == symbolString.Substring(1, 1))
				{
					return;
				}
			}
			symbols[i].possibleOutcomes.Add(symbolString.Substring(1, 1));
			return;
		}
		symbols.Add(new SpawnSymbol(symbolString.Substring(0, 1), symbolString.Substring(1, 1), inv));
	}

	public static bool IsMajorCreature(CreatureTemplate.Type type)
	{
		if (!(type == CreatureTemplate.Type.BigEel) && !(type == DLCSharedEnums.CreatureTemplateType.BigJelly) && !(type == CreatureTemplate.Type.BigEel) && !(type == CreatureTemplate.Type.BrotherLongLegs) && !(type == CreatureTemplate.Type.DaddyLongLegs) && !(type == CreatureTemplate.Type.Deer) && !(type == DLCSharedEnums.CreatureTemplateType.Inspector) && !(type == CreatureTemplate.Type.Vulture) && !(type == CreatureTemplate.Type.KingVulture) && !(type == CreatureTemplate.Type.MirosBird) && !(type == DLCSharedEnums.CreatureTemplateType.MirosVulture) && !(type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs) && !(type == CreatureTemplate.Type.RedCentipede) && !(type == DLCSharedEnums.CreatureTemplateType.AquaCenti))
		{
			return type == CreatureTemplate.Type.RedLizard;
		}
		return true;
	}
}
