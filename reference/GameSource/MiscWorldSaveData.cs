using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class MiscWorldSaveData
{
	public int SSaiConversationsHad;

	public int SSaiThrowOuts;

	public SLOrcacleState privSlOracleState;

	public PlayerGuideState playerGuideState;

	public List<string> unrecognizedSaveStrings;

	public bool moonRevived;

	public bool pebblesSeenGreenNeuron;

	public bool memoryArraysFrolicked;

	public SlugcatStats.Name saveStateNumber;

	public int cyclesSinceSSai;

	public bool pebblesEnergyTaken;

	public int energySeenState;

	public bool moonHeartRestored;

	public bool moonGivenRobe;

	public bool smPearlTagged;

	public bool discussedHalcyon;

	public bool halcyonStolen;

	public bool pebblesRivuletPostgame;

	public bool hrMelted;

	public bool artificerLeftGarbageWastes;

	public int cyclesSinceLastSlugpup;

	public int[] integersWatcher;

	public int[] unrecognizedIntegersWatcher;

	public bool lockedOuterRimProgressionFlag;

	public bool visitedShopRoom;

	public List<string> regionsInfectedBySentientRot;

	public List<string> regionsInfectedBySentientRotSpread;

	public List<string> roomsSealedByWeaverAbility;

	public List<string> roomsSealedByVoidWeaver;

	public List<string> oldRoomsSealedByWeaverAbility;

	public List<string> oldRoomsSealedByVoidWeaver;

	public Dictionary<string, string> discoveredWarpPoints;

	public Dictionary<string, int> extraSentientRotTicks;

	public List<string> pendingSentientRotInfections;

	public int rippleEggsCollected;

	public List<(string, int)> rippleEggsToRespawn;

	public static int rippleEggRespawnThreshold = 50;

	public bool hasDeferredDream;

	public bool seenRotDream;

	public bool seenSpinningTopDream;

	public bool seenTerraceDream;

	public DreamsState.DreamID deferredDream;

	public bool hasDeferredSpinningTopConversation;

	public Conversation.ID deferredSpinningTopConversation;

	public SLOrcacleState SLOracleState
	{
		get
		{
			if (privSlOracleState == null)
			{
				privSlOracleState = new SLOrcacleState(isDebugState: false, saveStateNumber);
			}
			return privSlOracleState;
		}
	}

	public bool EverMetMoon
	{
		get
		{
			if (privSlOracleState == null)
			{
				return false;
			}
			return privSlOracleState.playerEncounters > 0;
		}
	}

	public int camoTutorialCounter
	{
		get
		{
			return integersWatcher[0];
		}
		set
		{
			integersWatcher[0] = value;
		}
	}

	public int usedCamoAbility
	{
		get
		{
			return integersWatcher[1];
		}
		set
		{
			integersWatcher[1] = value;
		}
	}

	public int cycleFirstStartedWarpJourney
	{
		get
		{
			return integersWatcher[2];
		}
		set
		{
			integersWatcher[2] = value;
		}
	}

	public int numberOfWarpPointsGenerated
	{
		get
		{
			return integersWatcher[3];
		}
		set
		{
			integersWatcher[3] = value;
		}
	}

	public int numberOfVoidWeaverEncounters
	{
		get
		{
			return integersWatcher[4];
		}
		set
		{
			integersWatcher[4] = value;
		}
	}

	public int numberOfUndefinedWarpsTransversed
	{
		get
		{
			return integersWatcher[5];
		}
		set
		{
			integersWatcher[5] = value;
		}
	}

	public int numberOfPrinceEncounters
	{
		get
		{
			return integersWatcher[6];
		}
		set
		{
			integersWatcher[6] = value;
		}
	}

	public int numberOfWarpPointsSealed
	{
		get
		{
			return integersWatcher[8];
		}
		set
		{
			integersWatcher[8] = value;
		}
	}

	public int numberOfSentientRotSpreads
	{
		get
		{
			return integersWatcher[9];
		}
		set
		{
			integersWatcher[9] = value;
		}
	}

	public int numberOfBadWarpsGenerated
	{
		get
		{
			return integersWatcher[10];
		}
		set
		{
			integersWatcher[10] = value;
		}
	}

	public int cycleLastEncounteredPrince
	{
		get
		{
			return integersWatcher[11];
		}
		set
		{
			integersWatcher[11] = value;
		}
	}

	public int highestPrinceConversationSeen
	{
		get
		{
			return integersWatcher[12];
		}
		set
		{
			integersWatcher[12] = value;
		}
	}

	public int princeWeaverDialogProgression
	{
		get
		{
			return integersWatcher[13];
		}
		set
		{
			integersWatcher[13] = value;
		}
	}

	public int princeWeaverGrowthAcknowledgement
	{
		get
		{
			return integersWatcher[14];
		}
		set
		{
			integersWatcher[14] = value;
		}
	}

	public bool hasVoidWeaverAbility => numberOfVoidWeaverEncounters >= 4;

	public int stableWarpTutorialCounter
	{
		get
		{
			return integersWatcher[16];
		}
		set
		{
			integersWatcher[16] = value;
		}
	}

	public int badWarpTutorialCounter
	{
		get
		{
			return integersWatcher[17];
		}
		set
		{
			integersWatcher[17] = value;
		}
	}

	public int warpFatigueTutorialCounter
	{
		get
		{
			return integersWatcher[18];
		}
		set
		{
			integersWatcher[18] = value;
		}
	}

	public int warpExhaustionTutorialCounter
	{
		get
		{
			return integersWatcher[19];
		}
		set
		{
			integersWatcher[19] = value;
		}
	}

	public int cycleLastShownStableWarpTutorial
	{
		get
		{
			return integersWatcher[20];
		}
		set
		{
			integersWatcher[20] = value;
		}
	}

	public int numberOfRottedVanillaVisits
	{
		get
		{
			return integersWatcher[21];
		}
		set
		{
			integersWatcher[21] = value;
		}
	}

	public int hasSkippedFirstWarpFatigueTransfer
	{
		get
		{
			return integersWatcher[22];
		}
		set
		{
			integersWatcher[22] = value;
		}
	}

	public bool hasRippleEggWarpAbility
	{
		get
		{
			return integersWatcher[23] >= 1;
		}
		set
		{
			integersWatcher[23] = (value ? 1 : 0);
		}
	}

	public int numberOfRippleEggWarpsGenerated
	{
		get
		{
			return integersWatcher[23] - 1;
		}
		set
		{
			integersWatcher[23] = value + 1;
		}
	}

	public int remainingRegionsForSentientRotEnding => Mathf.Max(0, 21 - regionsInfectedBySentientRotSpread.Count);

	public bool SentientRotEndingConditionsMet
	{
		get
		{
			if (remainingRegionsForSentientRotEnding != 0 || lockedOuterRimProgressionFlag || highestPrinceConversationSeen <= PrinceBehavior.PrinceConversation.PrinceConversationToId(WatcherEnums.ConversationID.Prince_1))
			{
				return Custom.rainWorld.progression.miscProgressionData.beaten_Watcher_SentientRot;
			}
			return true;
		}
	}

	public bool SentientRotAscensionUnlock => highestPrinceConversationSeen > PrinceBehavior.PrinceConversation.PrinceConversationToId(WatcherEnums.ConversationID.Prince_1);

	public bool RoomSealedByWeaverOrWeaverAbility(string room)
	{
		if (!roomsSealedByVoidWeaver.Contains(room))
		{
			return roomsSealedByWeaverAbility.Contains(room);
		}
		return true;
	}

	public MiscWorldSaveData(SlugcatStats.Name saveStateNumber)
	{
		this.saveStateNumber = saveStateNumber;
		playerGuideState = new PlayerGuideState();
		unrecognizedSaveStrings = new List<string>();
		integersWatcher = new int[24];
		discoveredWarpPoints = new Dictionary<string, string>();
		extraSentientRotTicks = new Dictionary<string, int>();
		regionsInfectedBySentientRot = new List<string>();
		regionsInfectedBySentientRotSpread = new List<string>();
		pendingSentientRotInfections = new List<string>();
		roomsSealedByWeaverAbility = new List<string>();
		roomsSealedByVoidWeaver = new List<string>();
		oldRoomsSealedByWeaverAbility = new List<string>();
		oldRoomsSealedByVoidWeaver = new List<string>();
		rippleEggsToRespawn = new List<(string, int)>();
	}

	public override string ToString()
	{
		string text = "";
		oldRoomsSealedByVoidWeaver = new List<string>(roomsSealedByVoidWeaver);
		oldRoomsSealedByWeaverAbility = new List<string>(roomsSealedByWeaverAbility);
		if (SSaiConversationsHad > 0)
		{
			text += string.Format(CultureInfo.InvariantCulture, "SSaiConversationsHad<mwB>{0}<mwA>", SSaiConversationsHad);
		}
		if (SSaiThrowOuts > 0)
		{
			text += string.Format(CultureInfo.InvariantCulture, "SSaiThrowOuts<mwB>{0}<mwA>", SSaiThrowOuts);
		}
		if (privSlOracleState != null && (privSlOracleState.playerEncounters > 0 || (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)))
		{
			text = text + "SLaiState<mwB>" + privSlOracleState.ToString() + "<mwA>";
		}
		text = text + "playerGuideState<mwB>" + playerGuideState?.ToString() + "<mwA>";
		if (moonRevived)
		{
			text += "MOONREVIVED<mwA>";
		}
		if (pebblesSeenGreenNeuron)
		{
			text += "PEBBLESHELPED<mwA>";
		}
		if (memoryArraysFrolicked)
		{
			text += "MEMORYFROLICK<mwA>";
		}
		if (ModManager.MSC)
		{
			if (cyclesSinceSSai > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "CyclesSinceSSai<mwB>{0}<mwA>", cyclesSinceSSai);
			}
			if (pebblesEnergyTaken)
			{
				text += "ENERGYRAILOFF<mwA>";
			}
			if (energySeenState > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "EnergySeenState<mwB>{0}<mwA>", energySeenState);
			}
			if (moonHeartRestored)
			{
				text += "MOONHEART<mwA>";
			}
			if (moonGivenRobe)
			{
				text += "MOONROBE<mwA>";
			}
			if (smPearlTagged)
			{
				text += "SMPEARLTAGGED<mwA>";
			}
			if (discussedHalcyon)
			{
				text += "HALCYONTALK<mwA>";
			}
			if (halcyonStolen)
			{
				text += "HALCYONSTOLE<mwA>";
			}
			if (pebblesRivuletPostgame)
			{
				text += "PEBRIVPOST<mwA>";
			}
			if (hrMelted)
			{
				text += "HRMELT<mwA>";
			}
			if (artificerLeftGarbageWastes)
			{
				text += "ARTYLEFTGW<mwA>";
			}
			if (cyclesSinceLastSlugpup > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "CyclesSinceSlugpup<mwB>{0}<mwA>", cyclesSinceLastSlugpup);
			}
		}
		if (ModManager.Watcher)
		{
			if (saveStateNumber == WatcherEnums.SlugcatStatsName.Watcher)
			{
				text += "IntegersWatcher<mwB>";
				text += SaveUtils.SaveIntegerArray(',', integersWatcher, unrecognizedIntegersWatcher);
				text += "<mwA>";
			}
			if (lockedOuterRimProgressionFlag)
			{
				text += "OUTERRIMLOCK<mwA>";
			}
			if (visitedShopRoom)
			{
				text += "VISITSHOP<mwA>";
			}
			if (hasDeferredDream)
			{
				text = text + "DEFERREDDREAM<mwB>" + deferredDream.value + "<mwA>";
			}
			if (hasDeferredSpinningTopConversation)
			{
				text = text + "DEFERREDSTC<mwB>" + deferredSpinningTopConversation.value + "<mwA>";
			}
			if (seenRotDream)
			{
				text += "SEENROTDREAM<mwA>";
			}
			if (seenSpinningTopDream)
			{
				text += "SEENSTDREAM<mwA>";
			}
			if (seenTerraceDream)
			{
				text += "SEENTERRACEDREAM<mwA>";
			}
		}
		if (discoveredWarpPoints.Count > 0)
		{
			text += "WARPPOINTS<mwB>";
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, string> discoveredWarpPoint in discoveredWarpPoints)
			{
				list.Add(discoveredWarpPoint.Key + "<wP>" + discoveredWarpPoint.Value);
			}
			text += string.Join("<mwC>", list.ToArray());
			text += "<mwA>";
		}
		if (roomsSealedByWeaverAbility.Count > 0)
		{
			text += "WEAVERSEALS<mwB>";
			text += string.Join("<mwC>", roomsSealedByWeaverAbility.ToArray());
			text += "<mwA>";
		}
		if (roomsSealedByVoidWeaver.Count > 0)
		{
			text += "VOIDWEAVERSEALS<mwB>";
			List<string> list2 = new List<string>();
			foreach (string item in roomsSealedByVoidWeaver)
			{
				if (!roomsSealedByWeaverAbility.Contains(item))
				{
					list2.Add(item);
				}
			}
			text += string.Join("<mwC>", list2.ToArray());
			text += "<mwA>";
		}
		if (regionsInfectedBySentientRot.Count > 0)
		{
			text += "ROTINFECTEDREGIONS<mwB>";
			text += string.Join("<mwC>", regionsInfectedBySentientRot.ToArray());
			text += "<mwA>";
		}
		if (regionsInfectedBySentientRotSpread.Count > 0)
		{
			text += "ROTSPREADREGIONS<mwB>";
			text += string.Join("<mwC>", regionsInfectedBySentientRotSpread.ToArray());
			text += "<mwA>";
		}
		if (pendingSentientRotInfections.Count > 0)
		{
			text += "PENDINGROTSPREADS<mwB>";
			text += string.Join("<mwC>", pendingSentientRotInfections.ToArray());
			text += "<mwA>";
		}
		if (extraSentientRotTicks.Count > 0)
		{
			text += "EXTRAROTSPREADS<mwB>";
			List<string> list3 = new List<string>();
			foreach (KeyValuePair<string, int> extraSentientRotTick in extraSentientRotTicks)
			{
				list3.Add(extraSentientRotTick.Key + "<wP>" + extraSentientRotTick.Value);
			}
			text += string.Join("<mwC>", list3.ToArray());
			text += "<mwA>";
		}
		if (rippleEggsCollected > 0)
		{
			text = text + "RIPPLEEGGSCOLLECTED<mwB>" + rippleEggsCollected + "<mwA>";
		}
		if (rippleEggsToRespawn.Count > 0)
		{
			text += "RIPPLEEGGSTORESPAWN<mwB>";
			List<string> list4 = new List<string>();
			foreach (var (text2, num) in rippleEggsToRespawn)
			{
				list4.Add(text2 + "~" + num);
			}
			text += string.Join("<mwC>", list4.ToArray());
			text += "<mwA>";
		}
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString + "<mwA>";
		}
		return text;
	}

	public void FromString(string s)
	{
		moonRevived = false;
		pebblesSeenGreenNeuron = false;
		memoryArraysFrolicked = false;
		SSaiConversationsHad = 0;
		SSaiThrowOuts = 0;
		if (ModManager.MSC)
		{
			cyclesSinceSSai = 0;
			hrMelted = false;
			artificerLeftGarbageWastes = false;
			cyclesSinceLastSlugpup = 0;
			if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				moonHeartRestored = true;
			}
			if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				moonGivenRobe = true;
			}
		}
		if (ModManager.Watcher)
		{
			lockedOuterRimProgressionFlag = false;
			visitedShopRoom = false;
			hasDeferredDream = false;
			hasDeferredSpinningTopConversation = false;
			seenRotDream = false;
			seenSpinningTopDream = false;
			seenTerraceDream = false;
		}
		Array.Clear(integersWatcher, 0, integersWatcher.Length);
		unrecognizedIntegersWatcher = null;
		unrecognizedSaveStrings.Clear();
		string[] array = Regex.Split(s, "<mwA>");
		for (int i = 0; i < array.Length; i++)
		{
			bool flag = false;
			string[] array2 = Regex.Split(array[i], "<mwB>");
			switch (array2[0])
			{
			case "SSaiConversationsHad":
				SSaiConversationsHad = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "SSaiThrowOuts":
				SSaiThrowOuts = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "SLaiState":
				privSlOracleState = new SLOrcacleState(isDebugState: false, saveStateNumber);
				privSlOracleState.FromString(array2[1]);
				break;
			case "playerGuideState":
				playerGuideState.FromString(array2[1]);
				break;
			case "MOONREVIVED":
				moonRevived = true;
				break;
			case "PEBBLESHELPED":
				pebblesSeenGreenNeuron = true;
				break;
			case "MEMORYFROLICK":
				memoryArraysFrolicked = true;
				break;
			case "CyclesSinceSSai":
				if (ModManager.MSC)
				{
					cyclesSinceSSai = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag = true;
				}
				break;
			case "ENERGYRAILOFF":
				if (ModManager.MSC)
				{
					pebblesEnergyTaken = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "EnergySeenState":
				if (ModManager.MSC)
				{
					energySeenState = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag = true;
				}
				break;
			case "MOONHEART":
				if (ModManager.MSC)
				{
					moonHeartRestored = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "MOONROBE":
				if (ModManager.MSC)
				{
					moonGivenRobe = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "SMPEARLTAGGED":
				if (ModManager.MSC)
				{
					smPearlTagged = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "HALCYONTALK":
				if (ModManager.MSC)
				{
					discussedHalcyon = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "HALCYONSTOLE":
				if (ModManager.MSC)
				{
					halcyonStolen = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "PEBRIVPOST":
				if (ModManager.MSC)
				{
					pebblesRivuletPostgame = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "HRMELT":
				if (ModManager.MSC)
				{
					hrMelted = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "ARTYLEFTGW":
				if (ModManager.MSC)
				{
					artificerLeftGarbageWastes = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "CyclesSinceSlugpup":
				if (ModManager.MSC)
				{
					cyclesSinceLastSlugpup = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag = true;
				}
				break;
			case "IntegersWatcher":
				if (ModManager.Watcher)
				{
					unrecognizedIntegersWatcher = SaveUtils.LoadIntegersArray(array2[1], ',', integersWatcher);
				}
				else
				{
					flag = true;
				}
				break;
			case "WARPPOINTS":
			{
				string[] array3 = Regex.Split(array2[1], "<mwC>");
				discoveredWarpPoints = new Dictionary<string, string>();
				for (int j = 0; j < array3.Length; j++)
				{
					if (array3[j] != string.Empty)
					{
						string[] array4 = Regex.Split(array3[j], "<wP>");
						discoveredWarpPoints[array4[0]] = array4[1];
					}
				}
				break;
			}
			case "WEAVERSEALS":
			{
				string[] collection = Regex.Split(array2[1], "<mwC>");
				roomsSealedByWeaverAbility = new List<string>(collection);
				oldRoomsSealedByWeaverAbility = new List<string>(collection);
				break;
			}
			case "VOIDWEAVERSEALS":
			{
				string[] collection2 = Regex.Split(array2[1], "<mwC>");
				roomsSealedByVoidWeaver = new List<string>(collection2);
				oldRoomsSealedByVoidWeaver = new List<string>(collection2);
				break;
			}
			case "ROTINFECTEDREGIONS":
			{
				string[] source3 = Regex.Split(array2[1], "<mwC>");
				regionsInfectedBySentientRot = new List<string>(source3.Where((string s) => !string.IsNullOrWhiteSpace(s)));
				break;
			}
			case "ROTSPREADREGIONS":
			{
				string[] source2 = Regex.Split(array2[1], "<mwC>");
				regionsInfectedBySentientRotSpread = new List<string>(source2.Where((string s) => !string.IsNullOrWhiteSpace(s)));
				break;
			}
			case "PENDINGROTSPREADS":
			{
				string[] source = Regex.Split(array2[1], "<mwC>");
				pendingSentientRotInfections = new List<string>(source.Where((string s) => !string.IsNullOrWhiteSpace(s)));
				break;
			}
			case "EXTRAROTSPREADS":
			{
				string[] array7 = Regex.Split(array2[1], "<mwC>");
				extraSentientRotTicks = new Dictionary<string, int>();
				for (int l = 0; l < array7.Length; l++)
				{
					if (array7[l] != string.Empty)
					{
						string[] array8 = Regex.Split(array7[l], "<wP>");
						extraSentientRotTicks[array8[0]] = int.Parse(array8[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
				}
				break;
			}
			case "RIPPLEEGGSCOLLECTED":
				rippleEggsCollected = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "RIPPLEEGGSTORESPAWN":
			{
				Regex.Split(array2[1], "<mwC>");
				rippleEggsToRespawn = new List<(string, int)>();
				string[] array5 = Regex.Split(array2[1], "<mwC>");
				foreach (string text in array5)
				{
					if (!string.IsNullOrWhiteSpace(text))
					{
						string[] array6 = Regex.Split(text, "~");
						if (array6.Length >= 2 && int.TryParse(array6[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
						{
							rippleEggsToRespawn.Add((array6[0], result));
						}
					}
				}
				break;
			}
			case "OUTERRIMLOCK":
				if (ModManager.Watcher)
				{
					lockedOuterRimProgressionFlag = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "VISITSHOP":
				if (ModManager.Watcher)
				{
					visitedShopRoom = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "SEENROTDREAM":
				if (ModManager.Watcher)
				{
					seenRotDream = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "SEENSTDREAM":
				if (ModManager.Watcher)
				{
					seenSpinningTopDream = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "SEENTERRACEDREAM":
				if (ModManager.Watcher)
				{
					seenTerraceDream = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "DEFERREDDREAM":
				if (ModManager.Watcher)
				{
					hasDeferredDream = true;
					deferredDream = new DreamsState.DreamID(array2[1]);
				}
				else
				{
					flag = true;
				}
				break;
			case "DEFERREDSTC":
				if (ModManager.Watcher)
				{
					hasDeferredSpinningTopConversation = true;
					deferredSpinningTopConversation = new Conversation.ID(array2[1]);
				}
				else
				{
					flag = true;
				}
				break;
			default:
				flag = true;
				break;
			}
			if (flag && array[i].Trim().Length > 0 && array2.Length >= 1)
			{
				unrecognizedSaveStrings.Add(array[i]);
			}
		}
	}
}
