using System.Collections.Generic;
using System.Linq;
using Menu;
using Menu.Remix;

public static class MachineConnector
{
	public static bool _testing = false;

	public static Dictionary<string, OptionInterface> _registeredOIs = new Dictionary<string, OptionInterface>();

	public static void _Initialize()
	{
		MenuColorEffect._Initialize();
		ValueConverter._Initialize();
		OptionalText._Initialize();
	}

	public static void _RefreshOIs()
	{
		Dictionary<string, OptionInterface> dictionary = new Dictionary<string, OptionInterface>();
		ModManager.Mod[] array = ModManager.InstalledMods.ToArray();
		foreach (ModManager.Mod mod in array)
		{
			if (char.IsLetterOrDigit(mod.id.ToCharArray()[0]))
			{
				if (!_registeredOIs.TryGetValue(mod.id, out var value))
				{
					value = new InternalOI_Auto(mod);
				}
				dictionary.Add(mod.id, value);
			}
		}
		_registeredOIs = dictionary;
	}

	public static void _LoadAllConfigs()
	{
		OptionInterface[] array = _registeredOIs.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]._LoadConfigFile();
		}
	}

	public static void LogMessage(object msg)
	{
	}

	public static void LogInfo(object msg)
	{
	}

	public static void LogWarning(object msg)
	{
	}

	public static void LogError(object msg)
	{
	}

	public static void _ClearRegisteredOIs()
	{
		_registeredOIs.Clear();
	}

	public static bool SetRegisteredOI(string modID, OptionInterface oi)
	{
		if (_registeredOIs.TryGetValue(modID, out var value))
		{
			oi.mod = value.mod;
			_registeredOIs.Remove(modID);
			_registeredOIs.Add(modID, oi);
			return true;
		}
		return false;
	}

	public static OptionInterface GetRegisteredOI(string modID)
	{
		if (_registeredOIs.TryGetValue(modID, out var value))
		{
			return value;
		}
		return null;
	}

	public static bool IsThisModActive(string modID)
	{
		OptionInterface value;
		if (!char.IsLetterOrDigit(modID.ToCharArray()[0]))
		{
			foreach (ModManager.Mod activeMod in ModManager.ActiveMods)
			{
				if (activeMod.id == modID)
				{
					return true;
				}
			}
		}
		else if (_registeredOIs.TryGetValue(modID, out value))
		{
			return value.mod.enabled;
		}
		return false;
	}

	public static void ResetConfig(OptionInterface oi)
	{
		if (!_registeredOIs.TryGetValue(oi.mod.id, out oi))
		{
			return;
		}
		foreach (KeyValuePair<string, ConfigurableBase> configurable in oi.config.configurables)
		{
			if (!configurable.Value.IsCosmetic)
			{
				configurable.Value.BoxedValue = ValueConverter.ConvertToValue(configurable.Value.defaultValue, configurable.Value.settingType);
			}
		}
	}

	public static void SaveConfig(OptionInterface oi)
	{
		if (_registeredOIs.TryGetValue(oi.mod.id, out oi))
		{
			oi._SaveConfigFile();
		}
	}

	public static void ReloadConfig(OptionInterface oi)
	{
		if (_registeredOIs.TryGetValue(oi.mod.id, out oi))
		{
			oi.config.Reload();
		}
	}
}
