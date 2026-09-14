using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Rewired.Utils.Libraries.TinyJson;
using UnityEngine;

namespace Rewired.Data;

public class RewiredUserDataStore : UserDataStore
{
	public class ControllerAssignmentSaveInfo
	{
		public class PlayerInfo
		{
			public int id;

			public bool hasKeyboard;

			public bool hasMouse;

			public JoystickInfo[] joysticks;

			public int joystickCount
			{
				get
				{
					if (joysticks == null)
					{
						return 0;
					}
					return joysticks.Length;
				}
			}

			public int IndexOfJoystick(int joystickId)
			{
				for (int i = 0; i < joystickCount; i++)
				{
					if (joysticks[i] != null && joysticks[i].id == joystickId)
					{
						return i;
					}
				}
				return -1;
			}

			public bool ContainsJoystick(int joystickId)
			{
				return IndexOfJoystick(joystickId) >= 0;
			}
		}

		public class JoystickInfo
		{
			public Guid instanceGuid;

			public string hardwareIdentifier;

			public int id;
		}

		public PlayerInfo[] players;

		public int playerCount
		{
			get
			{
				if (players == null)
				{
					return 0;
				}
				return players.Length;
			}
		}

		public ControllerAssignmentSaveInfo()
		{
		}

		public ControllerAssignmentSaveInfo(int playerCount)
		{
			players = new PlayerInfo[playerCount];
			for (int i = 0; i < playerCount; i++)
			{
				players[i] = new PlayerInfo();
			}
		}

		public int IndexOfPlayer(int playerId)
		{
			for (int i = 0; i < playerCount; i++)
			{
				if (players[i] != null && players[i].id == playerId)
				{
					return i;
				}
			}
			return -1;
		}

		public bool ContainsPlayer(int playerId)
		{
			return IndexOfPlayer(playerId) >= 0;
		}
	}

	public class JoystickAssignmentHistoryInfo
	{
		public Joystick joystick;

		public int oldJoystickId;

		public JoystickAssignmentHistoryInfo(Joystick joystick, int oldJoystickId)
		{
			if (joystick == null)
			{
				throw new ArgumentNullException("joystick");
			}
			this.joystick = joystick;
			this.oldJoystickId = oldJoystickId;
		}
	}

	public const string thisScriptName = "RewiredUserDataStore";

	public const string logPrefix = "Rewired: ";

	public const string editorLoadedMessage = "\n***IMPORTANT:*** Changes made to the Rewired Input Manager configuration after the last time XML data was saved WILL NOT be used because the loaded old saved data has overwritten these values. If you change something in the Rewired Input Manager such as a Joystick Map or Input Behavior settings, you will not see these changes reflected in the current configuration. Clear PlayerPrefs using the inspector option on the RewiredUserDataStore component.";

	public const string playerPrefsKeySuffix_controllerAssignments = "ControllerAssignments";

	public const int controllerMapPPKeyVersion_original = 0;

	public const int controllerMapPPKeyVersion_includeDuplicateJoystickIndex = 1;

	public const int controllerMapPPKeyVersion_supportDisconnectedControllers = 2;

	public const int controllerMapPPKeyVersion_includeFormatVersion = 2;

	public const int controllerMapPPKeyVersion = 2;

	[Tooltip("Should this script be used? If disabled, nothing will be saved or loaded.")]
	[SerializeField]
	public bool isEnabled = true;

	[Tooltip("Should saved data be loaded on start?")]
	[SerializeField]
	public bool loadDataOnStart = true;

	[Tooltip("Should Player Joystick assignments be saved and loaded? This is not totally reliable for all Joysticks on all platforms. Some platforms/input sources do not provide enough information to reliably save assignments from session to session and reboot to reboot.")]
	[SerializeField]
	public bool loadJoystickAssignments = true;

	[Tooltip("Should Player Keyboard assignments be saved and loaded?")]
	[SerializeField]
	public bool loadKeyboardAssignments = true;

	[Tooltip("Should Player Mouse assignments be saved and loaded?")]
	[SerializeField]
	public bool loadMouseAssignments = true;

	[Tooltip("The PlayerPrefs key prefix. Change this to change how keys are stored in PlayerPrefs. Changing this will make saved data already stored with the old key no longer accessible.")]
	[SerializeField]
	public string playerPrefsKeyPrefix = "RewiredSaveData";

	[NonSerialized]
	public bool allowImpreciseJoystickAssignmentMatching = true;

	[NonSerialized]
	public bool deferredJoystickAssignmentLoadPending;

	[NonSerialized]
	public bool wasJoystickEverDetected;

	[NonSerialized]
	public List<int> __allActionIds;

	[NonSerialized]
	public string __allActionIdsString;

	public bool IsEnabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			isEnabled = value;
		}
	}

	public bool LoadDataOnStart
	{
		get
		{
			return loadDataOnStart;
		}
		set
		{
			loadDataOnStart = value;
		}
	}

	public bool LoadJoystickAssignments
	{
		get
		{
			return loadJoystickAssignments;
		}
		set
		{
			loadJoystickAssignments = value;
		}
	}

	public bool LoadKeyboardAssignments
	{
		get
		{
			return loadKeyboardAssignments;
		}
		set
		{
			loadKeyboardAssignments = value;
		}
	}

	public bool LoadMouseAssignments
	{
		get
		{
			return loadMouseAssignments;
		}
		set
		{
			loadMouseAssignments = value;
		}
	}

	public string PlayerPrefsKeyPrefix
	{
		get
		{
			return playerPrefsKeyPrefix;
		}
		set
		{
			playerPrefsKeyPrefix = value;
		}
	}

	public string playerPrefsKey_controllerAssignments => string.Format("{0}_{1}", playerPrefsKeyPrefix, "ControllerAssignments");

	public bool loadControllerAssignments
	{
		get
		{
			if (!loadKeyboardAssignments && !loadMouseAssignments)
			{
				return loadJoystickAssignments;
			}
			return true;
		}
	}

	public List<int> allActionIds
	{
		get
		{
			if (__allActionIds != null)
			{
				return __allActionIds;
			}
			List<int> list = new List<int>();
			IList<InputAction> actions = ReInput.mapping.Actions;
			for (int i = 0; i < actions.Count; i++)
			{
				list.Add(actions[i].id);
			}
			__allActionIds = list;
			return list;
		}
	}

	public string allActionIdsString
	{
		get
		{
			if (!string.IsNullOrEmpty(__allActionIdsString))
			{
				return __allActionIdsString;
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<int> list = allActionIds;
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(list[i]);
			}
			__allActionIdsString = stringBuilder.ToString();
			return __allActionIdsString;
		}
	}

	public override void Save()
	{
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not save any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			SaveAll();
		}
	}

	public override void SaveControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not save any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			SaveControllerDataNow(playerId, controllerType, controllerId);
		}
	}

	public override void SaveControllerData(ControllerType controllerType, int controllerId)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not save any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			SaveControllerDataNow(controllerType, controllerId);
		}
	}

	public override void SavePlayerData(int playerId)
	{
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not save any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			SavePlayerDataNow(playerId);
		}
	}

	public override void SaveInputBehavior(int playerId, int behaviorId)
	{
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not save any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			SaveInputBehaviorNow(playerId, behaviorId);
		}
	}

	public override void Load()
	{
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not load any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			LoadAll();
		}
	}

	public override void LoadControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not load any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			LoadControllerDataNow(playerId, controllerType, controllerId);
		}
	}

	public override void LoadControllerData(ControllerType controllerType, int controllerId)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not load any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			LoadControllerDataNow(controllerType, controllerId);
		}
	}

	public override void LoadPlayerData(int playerId)
	{
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not load any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			LoadPlayerDataNow(playerId);
		}
	}

	public override void LoadInputBehavior(int playerId, int behaviorId)
	{
		if (!IsEnabled)
		{
			Debug.LogWarning("Rewired: RewiredUserDataStore is disabled and will not load any data.", (UnityEngine.Object)(object)this);
		}
		else
		{
			LoadInputBehaviorNow(playerId, behaviorId);
		}
	}

	public override void OnInitialize()
	{
		if (loadDataOnStart)
		{
			((UserDataStore)this).Load();
			if (loadControllerAssignments && ReInput.controllers.joystickCount > 0)
			{
				wasJoystickEverDetected = true;
				SaveControllerAssignments();
			}
		}
	}

	public override void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if (IsEnabled && (int)args.controllerType == 2)
		{
			LoadJoystickData(args.controllerId);
			if (loadDataOnStart && loadJoystickAssignments && !wasJoystickEverDetected)
			{
				((MonoBehaviour)this).StartCoroutine(LoadJoystickAssignmentsDeferred());
			}
			if (loadJoystickAssignments && !deferredJoystickAssignmentLoadPending)
			{
				SaveControllerAssignments();
			}
			wasJoystickEverDetected = true;
		}
	}

	public override void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if (IsEnabled && (int)args.controllerType == 2)
		{
			SaveJoystickData(args.controllerId);
		}
	}

	public override void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (IsEnabled && loadControllerAssignments)
		{
			SaveControllerAssignments();
		}
	}

	public override void SaveControllerMap(int playerId, ControllerMap controllerMap)
	{
		if (controllerMap != null)
		{
			Player player = ReInput.players.GetPlayer(playerId);
			if (player != null)
			{
				SaveControllerMap(player, controllerMap);
			}
		}
	}

	public override ControllerMap LoadControllerMap(int playerId, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return null;
		}
		return LoadControllerMap(player, controllerIdentifier, categoryId, layoutId);
	}

	public int LoadAll()
	{
		int num = 0;
		if (loadControllerAssignments && LoadControllerAssignmentsNow())
		{
			num++;
		}
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			num += LoadPlayerDataNow(allPlayers[i]);
		}
		return num + LoadAllJoystickCalibrationData();
	}

	public int LoadPlayerDataNow(int playerId)
	{
		return LoadPlayerDataNow(ReInput.players.GetPlayer(playerId));
	}

	public int LoadPlayerDataNow(Player player)
	{
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		num += LoadInputBehaviors(player.id);
		num += LoadControllerMaps(player.id, (ControllerType)0, 0);
		num += LoadControllerMaps(player.id, (ControllerType)1, 0);
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			num += LoadControllerMaps(player.id, (ControllerType)2, ((Controller)joystick).id);
		}
		RefreshLayoutManager(player.id);
		return num;
	}

	public int LoadAllJoystickCalibrationData()
	{
		int num = 0;
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			num += LoadJoystickCalibrationData(joysticks[i]);
		}
		return num;
	}

	public int LoadJoystickCalibrationData(Joystick joystick)
	{
		if (joystick == null)
		{
			return 0;
		}
		if (!((ControllerWithAxes)joystick).ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick)))
		{
			return 0;
		}
		return 1;
	}

	public int LoadJoystickCalibrationData(int joystickId)
	{
		return LoadJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	public int LoadJoystickData(int joystickId)
	{
		int num = 0;
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player val = allPlayers[i];
			if (val.controllers.ContainsController((ControllerType)2, joystickId))
			{
				num += LoadControllerMaps(val.id, (ControllerType)2, joystickId);
				RefreshLayoutManager(val.id);
			}
		}
		return num + LoadJoystickCalibrationData(joystickId);
	}

	public int LoadControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		int num = 0 + LoadControllerMaps(playerId, controllerType, controllerId);
		RefreshLayoutManager(playerId);
		return num + LoadControllerDataNow(controllerType, controllerId);
	}

	public int LoadControllerDataNow(ControllerType controllerType, int controllerId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		int num = 0;
		if ((int)controllerType == 2)
		{
			num += LoadJoystickCalibrationData(controllerId);
		}
		return num;
	}

	public int LoadControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return num;
		}
		Controller controller = ReInput.controllers.GetController(controllerType, controllerId);
		if (controller == null)
		{
			return num;
		}
		IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
		for (int i = 0; i < mapCategories.Count; i++)
		{
			InputMapCategory val = mapCategories[i];
			if (!((InputCategory)val).userAssignable)
			{
				continue;
			}
			IList<InputLayout> list = ReInput.mapping.MapLayouts(controller.type);
			for (int j = 0; j < list.Count; j++)
			{
				InputLayout val2 = list[j];
				ControllerMap val3 = LoadControllerMap(player, controller.identifier, ((InputCategory)val).id, val2.id);
				if (val3 != null)
				{
					player.controllers.maps.AddMap(controller, val3);
					num++;
				}
			}
		}
		return num;
	}

	public ControllerMap LoadControllerMap(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (player == null)
		{
			return null;
		}
		string controllerMapXml = GetControllerMapXml(player, controllerIdentifier, categoryId, layoutId);
		if (string.IsNullOrEmpty(controllerMapXml))
		{
			return null;
		}
		ControllerMap val = ControllerMap.CreateFromXml(((ControllerIdentifier)(ref controllerIdentifier)).controllerType, controllerMapXml);
		if (val == null)
		{
			return null;
		}
		List<int> controllerMapKnownActionIds = GetControllerMapKnownActionIds(player, controllerIdentifier, categoryId, layoutId);
		AddDefaultMappingsForNewActions(controllerIdentifier, val, controllerMapKnownActionIds);
		return val;
	}

	public int LoadInputBehaviors(int playerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
		for (int i = 0; i < inputBehaviors.Count; i++)
		{
			num += LoadInputBehaviorNow(player, inputBehaviors[i]);
		}
		return num;
	}

	public int LoadInputBehaviorNow(int playerId, int behaviorId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return 0;
		}
		InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
		if (inputBehavior == null)
		{
			return 0;
		}
		return LoadInputBehaviorNow(player, inputBehavior);
	}

	public int LoadInputBehaviorNow(Player player, InputBehavior inputBehavior)
	{
		if (player == null || inputBehavior == null)
		{
			return 0;
		}
		string inputBehaviorXml = GetInputBehaviorXml(player, inputBehavior.id);
		if (inputBehaviorXml == null || inputBehaviorXml == string.Empty)
		{
			return 0;
		}
		if (!inputBehavior.ImportXmlString(inputBehaviorXml))
		{
			return 0;
		}
		return 1;
	}

	public bool LoadControllerAssignmentsNow()
	{
		try
		{
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = LoadControllerAssignmentData();
			if (controllerAssignmentSaveInfo == null)
			{
				return false;
			}
			if (loadKeyboardAssignments || loadMouseAssignments)
			{
				LoadKeyboardAndMouseAssignmentsNow(controllerAssignmentSaveInfo);
			}
			if (loadJoystickAssignments)
			{
				LoadJoystickAssignmentsNow(controllerAssignmentSaveInfo);
			}
		}
		catch
		{
		}
		return true;
	}

	public bool LoadKeyboardAndMouseAssignmentsNow(ControllerAssignmentSaveInfo data)
	{
		try
		{
			if (data == null && (data = LoadControllerAssignmentData()) == null)
			{
				return false;
			}
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				if (data.ContainsPlayer(allPlayer.id))
				{
					ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.players[data.IndexOfPlayer(allPlayer.id)];
					if (loadKeyboardAssignments)
					{
						allPlayer.controllers.hasKeyboard = playerInfo.hasKeyboard;
					}
					if (loadMouseAssignments)
					{
						allPlayer.controllers.hasMouse = playerInfo.hasMouse;
					}
				}
			}
		}
		catch
		{
		}
		return true;
	}

	public bool LoadJoystickAssignmentsNow(ControllerAssignmentSaveInfo data)
	{
		try
		{
			if (ReInput.controllers.joystickCount == 0)
			{
				return false;
			}
			if (data == null && (data = LoadControllerAssignmentData()) == null)
			{
				return false;
			}
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				allPlayer.controllers.ClearControllersOfType((ControllerType)2);
			}
			List<JoystickAssignmentHistoryInfo> list = (loadJoystickAssignments ? new List<JoystickAssignmentHistoryInfo>() : null);
			foreach (Player allPlayer2 in ReInput.players.AllPlayers)
			{
				if (!data.ContainsPlayer(allPlayer2.id))
				{
					continue;
				}
				ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.players[data.IndexOfPlayer(allPlayer2.id)];
				for (int i = 0; i < playerInfo.joystickCount; i++)
				{
					ControllerAssignmentSaveInfo.JoystickInfo joystickInfo2 = playerInfo.joysticks[i];
					if (joystickInfo2 == null)
					{
						continue;
					}
					Joystick joystick = FindJoystickPrecise(joystickInfo2);
					if (joystick != null)
					{
						if (list.Find((JoystickAssignmentHistoryInfo x) => x.joystick == joystick) == null)
						{
							list.Add(new JoystickAssignmentHistoryInfo(joystick, joystickInfo2.id));
						}
						allPlayer2.controllers.AddController((Controller)(object)joystick, false);
					}
				}
			}
			if (allowImpreciseJoystickAssignmentMatching)
			{
				foreach (Player allPlayer3 in ReInput.players.AllPlayers)
				{
					if (!data.ContainsPlayer(allPlayer3.id))
					{
						continue;
					}
					ControllerAssignmentSaveInfo.PlayerInfo playerInfo2 = data.players[data.IndexOfPlayer(allPlayer3.id)];
					for (int j = 0; j < playerInfo2.joystickCount; j++)
					{
						ControllerAssignmentSaveInfo.JoystickInfo joystickInfo = playerInfo2.joysticks[j];
						if (joystickInfo == null)
						{
							continue;
						}
						Joystick val = null;
						int num = list.FindIndex((JoystickAssignmentHistoryInfo x) => x.oldJoystickId == joystickInfo.id);
						if (num >= 0)
						{
							val = list[num].joystick;
						}
						else
						{
							if (!TryFindJoysticksImprecise(joystickInfo, out var matches))
							{
								continue;
							}
							foreach (Joystick match in matches)
							{
								if (list.Find((JoystickAssignmentHistoryInfo x) => x.joystick == match) == null)
								{
									val = match;
									break;
								}
							}
							if (val == null)
							{
								continue;
							}
							list.Add(new JoystickAssignmentHistoryInfo(val, joystickInfo.id));
						}
						allPlayer3.controllers.AddController((Controller)(object)val, false);
					}
				}
			}
		}
		catch
		{
		}
		if (ReInput.configuration.autoAssignJoysticks)
		{
			ReInput.controllers.AutoAssignJoysticks();
		}
		return true;
	}

	public ControllerAssignmentSaveInfo LoadControllerAssignmentData()
	{
		try
		{
			if (!IsEnabled)
			{
				return null;
			}
			if (!PlayerPrefs.HasKey(playerPrefsKey_controllerAssignments))
			{
				return null;
			}
			string @string = PlayerPrefs.GetString(playerPrefsKey_controllerAssignments);
			if (string.IsNullOrEmpty(@string))
			{
				return null;
			}
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = JsonParser.FromJson<ControllerAssignmentSaveInfo>(@string);
			if (controllerAssignmentSaveInfo == null || controllerAssignmentSaveInfo.playerCount == 0)
			{
				return null;
			}
			return controllerAssignmentSaveInfo;
		}
		catch
		{
			return null;
		}
	}

	public IEnumerator LoadJoystickAssignmentsDeferred()
	{
		deferredJoystickAssignmentLoadPending = true;
		yield return new WaitForEndOfFrame();
		if (ReInput.isReady)
		{
			LoadJoystickAssignmentsNow(null);
			SaveControllerAssignments();
			deferredJoystickAssignmentLoadPending = false;
		}
	}

	public void SaveAll()
	{
		if (IsEnabled)
		{
			IList<Player> allPlayers = ReInput.players.AllPlayers;
			for (int i = 0; i < allPlayers.Count; i++)
			{
				SavePlayerDataNow(allPlayers[i]);
			}
			SaveAllJoystickCalibrationData();
			if (loadControllerAssignments)
			{
				SaveControllerAssignments();
			}
			PlayerPrefs.Save();
		}
	}

	public void SavePlayerDataNow(int playerId)
	{
		if (IsEnabled)
		{
			SavePlayerDataNow(ReInput.players.GetPlayer(playerId));
			PlayerPrefs.Save();
		}
	}

	public void SavePlayerDataNow(Player player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (player != null)
		{
			PlayerSaveData saveData = player.GetSaveData(true);
			SaveInputBehaviors(player, saveData);
			SaveControllerMaps(player, saveData);
		}
	}

	public void SaveAllJoystickCalibrationData()
	{
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			SaveJoystickCalibrationData(joysticks[i]);
		}
	}

	public void SaveJoystickCalibrationData(int joystickId)
	{
		SaveJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	public void SaveJoystickCalibrationData(Joystick joystick)
	{
		if (IsEnabled && joystick != null)
		{
			JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
			PlayerPrefs.SetString(GetJoystickCalibrationMapPlayerPrefsKey(joystick), ((CalibrationMapSaveData)calibrationMapSaveData).map.ToXmlString());
		}
	}

	public void SaveJoystickData(int joystickId)
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player val = allPlayers[i];
			if (val.controllers.ContainsController((ControllerType)2, joystickId))
			{
				SaveControllerMaps(val.id, (ControllerType)2, joystickId);
			}
		}
		SaveJoystickCalibrationData(joystickId);
	}

	public void SaveControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled)
		{
			SaveControllerMaps(playerId, controllerType, controllerId);
			SaveControllerDataNow(controllerType, controllerId);
			PlayerPrefs.Save();
		}
	}

	public void SaveControllerDataNow(ControllerType controllerType, int controllerId)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if (IsEnabled)
		{
			if ((int)controllerType == 2)
			{
				SaveJoystickCalibrationData(controllerId);
			}
			PlayerPrefs.Save();
		}
	}

	public void SaveControllerMaps(Player player, PlayerSaveData playerSaveData)
	{
		foreach (ControllerMapSaveData allControllerMapSaveDatum in ((PlayerSaveData)(ref playerSaveData)).AllControllerMapSaveData)
		{
			SaveControllerMap(player, allControllerMapSaveDatum.map);
		}
	}

	public void SaveControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null || !player.controllers.ContainsController(controllerType, controllerId))
		{
			return;
		}
		ControllerMapSaveData[] mapSaveData = player.controllers.maps.GetMapSaveData(controllerType, controllerId, true);
		if (mapSaveData != null)
		{
			for (int i = 0; i < mapSaveData.Length; i++)
			{
				SaveControllerMap(player, mapSaveData[i].map);
			}
		}
	}

	public void SaveControllerMap(Player player, ControllerMap controllerMap)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled)
		{
			PlayerPrefs.SetString(GetControllerMapPlayerPrefsKey(player, controllerMap.controller.identifier, controllerMap.categoryId, controllerMap.layoutId, 2), controllerMap.ToXmlString());
			PlayerPrefs.SetString(GetControllerMapKnownActionIdsPlayerPrefsKey(player, controllerMap.controller.identifier, controllerMap.categoryId, controllerMap.layoutId, 2), allActionIdsString);
		}
	}

	public void SaveInputBehaviors(Player player, PlayerSaveData playerSaveData)
	{
		if (player != null)
		{
			InputBehavior[] inputBehaviors = ((PlayerSaveData)(ref playerSaveData)).inputBehaviors;
			for (int i = 0; i < inputBehaviors.Length; i++)
			{
				SaveInputBehaviorNow(player, inputBehaviors[i]);
			}
		}
	}

	public void SaveInputBehaviorNow(int playerId, int behaviorId)
	{
		if (!IsEnabled)
		{
			return;
		}
		Player player = ReInput.players.GetPlayer(playerId);
		if (player != null)
		{
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior != null)
			{
				SaveInputBehaviorNow(player, inputBehavior);
				PlayerPrefs.Save();
			}
		}
	}

	public void SaveInputBehaviorNow(Player player, InputBehavior inputBehavior)
	{
		if (IsEnabled && player != null && inputBehavior != null)
		{
			PlayerPrefs.SetString(GetInputBehaviorPlayerPrefsKey(player, inputBehavior.id), inputBehavior.ToXmlString());
		}
	}

	public bool SaveControllerAssignments()
	{
		try
		{
			if (!IsEnabled)
			{
				return false;
			}
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = new ControllerAssignmentSaveInfo(ReInput.players.allPlayerCount);
			for (int i = 0; i < ReInput.players.allPlayerCount; i++)
			{
				Player val = ReInput.players.AllPlayers[i];
				ControllerAssignmentSaveInfo.PlayerInfo playerInfo = new ControllerAssignmentSaveInfo.PlayerInfo();
				controllerAssignmentSaveInfo.players[i] = playerInfo;
				playerInfo.id = val.id;
				playerInfo.hasKeyboard = val.controllers.hasKeyboard;
				playerInfo.hasMouse = val.controllers.hasMouse;
				ControllerAssignmentSaveInfo.JoystickInfo[] array = (playerInfo.joysticks = new ControllerAssignmentSaveInfo.JoystickInfo[val.controllers.joystickCount]);
				for (int j = 0; j < val.controllers.joystickCount; j++)
				{
					Joystick val2 = val.controllers.Joysticks[j];
					ControllerAssignmentSaveInfo.JoystickInfo joystickInfo = new ControllerAssignmentSaveInfo.JoystickInfo();
					joystickInfo.instanceGuid = ((Controller)val2).deviceInstanceGuid;
					joystickInfo.id = ((Controller)val2).id;
					joystickInfo.hardwareIdentifier = ((Controller)val2).hardwareIdentifier;
					array[j] = joystickInfo;
				}
			}
			PlayerPrefs.SetString(playerPrefsKey_controllerAssignments, JsonWriter.ToJson((object)controllerAssignmentSaveInfo));
			PlayerPrefs.Save();
		}
		catch
		{
		}
		return true;
	}

	public bool ControllerAssignmentSaveDataExists()
	{
		if (!IsEnabled)
		{
			return false;
		}
		if (!PlayerPrefs.HasKey(playerPrefsKey_controllerAssignments))
		{
			return false;
		}
		if (string.IsNullOrEmpty(PlayerPrefs.GetString(playerPrefsKey_controllerAssignments)))
		{
			return false;
		}
		return true;
	}

	public string GetBasePlayerPrefsKey(Player player)
	{
		return playerPrefsKeyPrefix + "|playerName=" + player.name;
	}

	public string GetControllerMapPlayerPrefsKey(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return string.Concat(GetBasePlayerPrefsKey(player) + "|dataType=ControllerMap", GetControllerMapPlayerPrefsKeyCommonSuffix(player, controllerIdentifier, categoryId, layoutId, ppKeyVersion));
	}

	public string GetControllerMapKnownActionIdsPlayerPrefsKey(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return string.Concat(GetBasePlayerPrefsKey(player) + "|dataType=ControllerMap_KnownActionIds", GetControllerMapPlayerPrefsKeyCommonSuffix(player, controllerIdentifier, categoryId, layoutId, ppKeyVersion));
	}

	public static string GetControllerMapPlayerPrefsKeyCommonSuffix(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Invalid comparison between Unknown and I4
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Invalid comparison between Unknown and I4
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		if (ppKeyVersion >= 2)
		{
			text = text + "|kv=" + ppKeyVersion;
		}
		text = text + "|controllerMapType=" + GetControllerMapType(((ControllerIdentifier)(ref controllerIdentifier)).controllerType).Name;
		text = text + "|categoryId=" + categoryId + "|layoutId=" + layoutId;
		if (ppKeyVersion >= 2)
		{
			text = text + "|hardwareGuid=" + ((ControllerIdentifier)(ref controllerIdentifier)).hardwareTypeGuid.ToString();
			if (((ControllerIdentifier)(ref controllerIdentifier)).hardwareTypeGuid == Guid.Empty)
			{
				text = text + "|hardwareIdentifier=" + ((ControllerIdentifier)(ref controllerIdentifier)).hardwareIdentifier;
			}
			if ((int)((ControllerIdentifier)(ref controllerIdentifier)).controllerType == 2)
			{
				text = text + "|duplicate=" + GetDuplicateIndex(player, controllerIdentifier);
			}
		}
		else
		{
			text = text + "|hardwareIdentifier=" + ((ControllerIdentifier)(ref controllerIdentifier)).hardwareIdentifier;
			if ((int)((ControllerIdentifier)(ref controllerIdentifier)).controllerType == 2)
			{
				text = text + "|hardwareGuid=" + ((ControllerIdentifier)(ref controllerIdentifier)).hardwareTypeGuid.ToString();
				if (ppKeyVersion >= 1)
				{
					text = text + "|duplicate=" + GetDuplicateIndex(player, controllerIdentifier);
				}
			}
		}
		return text;
	}

	public string GetJoystickCalibrationMapPlayerPrefsKey(Joystick joystick)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		string text = playerPrefsKeyPrefix + "|dataType=CalibrationMap";
		ControllerType type = ((Controller)joystick).type;
		return string.Concat(string.Concat(text + "|controllerType=" + ((object)(ControllerType)(ref type)).ToString(), "|hardwareIdentifier=", ((Controller)joystick).hardwareIdentifier), "|hardwareGuid=", ((Controller)joystick).hardwareTypeGuid.ToString());
	}

	public string GetInputBehaviorPlayerPrefsKey(Player player, int inputBehaviorId)
	{
		return string.Concat(GetBasePlayerPrefsKey(player) + "|dataType=InputBehavior", "|id=", inputBehaviorId.ToString());
	}

	public string GetControllerMapXml(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			return null;
		}
		for (int num = 2; num >= 0; num--)
		{
			string controllerMapPlayerPrefsKey = GetControllerMapPlayerPrefsKey(player, controllerIdentifier, categoryId, layoutId, num);
			if (PlayerPrefs.HasKey(controllerMapPlayerPrefsKey))
			{
				return PlayerPrefs.GetString(controllerMapPlayerPrefsKey);
			}
		}
		return null;
	}

	public List<int> GetControllerMapKnownActionIds(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!IsEnabled)
		{
			return new List<int>();
		}
		List<int> list = new List<int>();
		string key = null;
		bool flag = false;
		for (int num = 2; num >= 0; num--)
		{
			key = GetControllerMapKnownActionIdsPlayerPrefsKey(player, controllerIdentifier, categoryId, layoutId, num);
			if (PlayerPrefs.HasKey(key))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return list;
		}
		string @string = PlayerPrefs.GetString(key);
		if (string.IsNullOrEmpty(@string))
		{
			return list;
		}
		string[] array = @string.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]) && int.TryParse(array[i], out var result))
			{
				list.Add(result);
			}
		}
		return list;
	}

	public string GetJoystickCalibrationMapXml(Joystick joystick)
	{
		if (!IsEnabled)
		{
			return string.Empty;
		}
		string joystickCalibrationMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(joystick);
		if (!PlayerPrefs.HasKey(joystickCalibrationMapPlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(joystickCalibrationMapPlayerPrefsKey);
	}

	public string GetInputBehaviorXml(Player player, int id)
	{
		if (!IsEnabled)
		{
			return string.Empty;
		}
		string inputBehaviorPlayerPrefsKey = GetInputBehaviorPlayerPrefsKey(player, id);
		if (!PlayerPrefs.HasKey(inputBehaviorPlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(inputBehaviorPlayerPrefsKey);
	}

	public void AddDefaultMappingsForNewActions(ControllerIdentifier controllerIdentifier, ControllerMap controllerMap, List<int> knownActionIds)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		if (controllerMap == null || knownActionIds == null || knownActionIds == null || knownActionIds.Count == 0)
		{
			return;
		}
		ControllerMap controllerMapInstance = ReInput.mapping.GetControllerMapInstance(controllerIdentifier, controllerMap.categoryId, controllerMap.layoutId);
		if (controllerMapInstance == null)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (int allActionId in allActionIds)
		{
			if (!knownActionIds.Contains(allActionId))
			{
				list.Add(allActionId);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		ElementAssignment val = default(ElementAssignment);
		foreach (ActionElementMap allMap in controllerMapInstance.AllMaps)
		{
			if (list.Contains(allMap.actionId) && !controllerMap.DoesElementAssignmentConflict(allMap))
			{
				((ElementAssignment)(ref val))._002Ector(controllerMap.controllerType, allMap.elementType, allMap.elementIdentifierId, allMap.axisRange, allMap.keyCode, allMap.modifierKeyFlags, allMap.actionId, allMap.axisContribution, allMap.invert);
				controllerMap.CreateElementMap(val);
			}
		}
	}

	public Joystick FindJoystickPrecise(ControllerAssignmentSaveInfo.JoystickInfo joystickInfo)
	{
		if (joystickInfo == null)
		{
			return null;
		}
		if (joystickInfo.instanceGuid == Guid.Empty)
		{
			return null;
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			if (((Controller)joysticks[i]).deviceInstanceGuid == joystickInfo.instanceGuid)
			{
				return joysticks[i];
			}
		}
		return null;
	}

	public bool TryFindJoysticksImprecise(ControllerAssignmentSaveInfo.JoystickInfo joystickInfo, out List<Joystick> matches)
	{
		matches = null;
		if (joystickInfo == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(joystickInfo.hardwareIdentifier))
		{
			return false;
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			if (string.Equals(((Controller)joysticks[i]).hardwareIdentifier, joystickInfo.hardwareIdentifier, StringComparison.OrdinalIgnoreCase))
			{
				if (matches == null)
				{
					matches = new List<Joystick>();
				}
				matches.Add(joysticks[i]);
			}
		}
		return matches != null;
	}

	public static int GetDuplicateIndex(Player player, ControllerIdentifier controllerIdentifier)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		Controller controller = ReInput.controllers.GetController(controllerIdentifier);
		if (controller == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Controller controller2 in player.controllers.Controllers)
		{
			if (controller2.type != controller.type)
			{
				continue;
			}
			bool flag = false;
			if ((int)controller.type == 2)
			{
				if (((controller2 is Joystick) ? controller2 : null).hardwareTypeGuid != controller.hardwareTypeGuid)
				{
					continue;
				}
				if (controller.hardwareTypeGuid != Guid.Empty)
				{
					flag = true;
				}
			}
			if (flag || !(controller2.hardwareIdentifier != controller.hardwareIdentifier))
			{
				if (controller2 == controller)
				{
					return num;
				}
				num++;
			}
		}
		return num;
	}

	public void RefreshLayoutManager(int playerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player != null)
		{
			player.controllers.maps.layoutManager.Apply();
		}
	}

	public static Type GetControllerMapType(ControllerType controllerType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		switch ((int)controllerType)
		{
		default:
			if ((int)controllerType == 20)
			{
				return typeof(CustomControllerMap);
			}
			Debug.LogWarning("Rewired: Unknown ControllerType " + ((object)(ControllerType)(ref controllerType)).ToString());
			return null;
		case 2:
			return typeof(JoystickMap);
		case 0:
			return typeof(KeyboardMap);
		case 1:
			return typeof(MouseMap);
		}
	}
}
