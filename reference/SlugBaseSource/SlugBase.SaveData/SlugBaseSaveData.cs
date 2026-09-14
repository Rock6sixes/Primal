using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace SlugBase.SaveData;

/// <summary>
/// A helper for interacting with the game's save data.
/// </summary>
public class SlugBaseSaveData
{
	internal const string KEY_SUFFIX = "_SlugBaseSaveData_";

	internal const string KEY_SUFFIX_INTERNAL = "_SlugBaseInternal_";

	internal static readonly ConditionalWeakTable<MiscWorldSaveData, SlugBaseSaveData> WorldData = new ConditionalWeakTable<MiscWorldSaveData, SlugBaseSaveData>();

	internal static readonly ConditionalWeakTable<MiscProgressionData, SlugBaseSaveData> ProgressionData = new ConditionalWeakTable<MiscProgressionData, SlugBaseSaveData>();

	internal static readonly ConditionalWeakTable<DeathPersistentSaveData, SlugBaseSaveData> DeathPersistentData = new ConditionalWeakTable<DeathPersistentSaveData, SlugBaseSaveData>();

	private readonly Dictionary<string, object> _data;

	private readonly List<string> _unrecognizedSaveStrings;

	internal SlugBaseSaveData(List<string> unrecognizedSaveStrings)
	{
		_data = new Dictionary<string, object>();
		_unrecognizedSaveStrings = unrecognizedSaveStrings;
	}

	/// <summary>
	/// Gets a value from the save data.
	/// </summary>
	/// <param name="key">The key for retrieving the value.</param>
	/// <param name="value">The stored value.</param>
	/// <typeparam name="T">The value's type.</typeparam>
	/// <returns><c>true</c> if a stored value was found, <c>false</c> otherwise.</returns>
	public bool TryGet<T>(string key, out T value)
	{
		return TryGetRaw<T>(key + "_SlugBaseSaveData_", out value);
	}

	internal bool TryGetInternal<T>(string key, out T value)
	{
		return TryGetRaw<T>(key + "_SlugBaseInternal_", out value);
	}

	private bool TryGetRaw<T>(string key, out T value)
	{
		if (_data.TryGetValue(key, out var value2) && value2 is T val)
		{
			value = val;
			return true;
		}
		if (LoadStringFromUnrecognizedStrings(key, out var value3))
		{
			try
			{
				value = JsonConvert.DeserializeObject<T>(value3);
			}
			catch (Exception exception)
			{
				SlugBasePlugin.Logger.LogError("Failed to convert key \"" + key + "\" to " + typeof(T).Name + ": \"" + value3 + "\"");
				Debug.LogException(exception);
				value = default(T);
				return false;
			}
			_data[key] = value;
			return true;
		}
		value = default(T);
		return false;
	}

	/// <summary>
	/// Sets or adds a value to the save data.
	/// </summary>
	/// <param name="key">The key for storing the value.</param>
	/// <param name="value">The value to be stored.</param>
	/// <typeparam name="T">The value's type.</typeparam>
	public void Set<T>(string key, T value)
	{
		_data[key + "_SlugBaseSaveData_"] = value;
	}

	internal void SetInternal<T>(string key, T value)
	{
		_data[key + "_SlugBaseInternal_"] = value;
	}

	/// <summary>
	/// Removes a value from the save data.
	/// </summary>
	/// <param name="key">The key for removing the value.</param>
	/// <returns><see langword="true" /> if the key was found and removed, <see langword="false" /> otherwise.</returns>
	public bool Remove(string key)
	{
		return _data.Remove(key + "_SlugBaseSaveData_");
	}

	internal bool RemoveInternal(string key)
	{
		return _data.Remove(key + "_SlugBaseInternal_");
	}

	internal void SaveToStrings(List<string> strings)
	{
		foreach (KeyValuePair<string, object> datum in _data)
		{
			try
			{
				SavePairToStrings(strings, datum.Key, JsonConvert.SerializeObject(datum.Value));
			}
			catch (Exception exception)
			{
				SlugBasePlugin.Logger.LogError("Failed to serialize key \"" + datum.Key + "\" of type " + (datum.Value?.GetType().Name ?? "null") + "!");
				Debug.LogException(exception);
			}
		}
	}

	internal bool LoadStringFromUnrecognizedStrings(string key, out string value)
	{
		for (int i = 0; i < _unrecognizedSaveStrings.Count; i++)
		{
			string text = _unrecognizedSaveStrings[i];
			if (text.StartsWith(key))
			{
				try
				{
					value = Encoding.UTF8.GetString(Convert.FromBase64String(text.Substring(key.Length)));
				}
				catch (Exception exception)
				{
					SlugBasePlugin.Logger.LogError("Badly formatted save data for key \"" + key + "\": \"" + text.Substring(key.Length) + "\"");
					Debug.LogException(exception);
					_unrecognizedSaveStrings.RemoveAt(i);
					break;
				}
				_unrecognizedSaveStrings.RemoveAt(i);
				return true;
			}
		}
		value = null;
		return false;
	}

	private static void SavePairToStrings(List<string> strings, string key, string value)
	{
		string text = key + Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
		for (int i = 0; i < strings.Count; i++)
		{
			if (strings[i].StartsWith(key))
			{
				strings[i] = text;
				return;
			}
		}
		strings.Add(text);
	}
}
