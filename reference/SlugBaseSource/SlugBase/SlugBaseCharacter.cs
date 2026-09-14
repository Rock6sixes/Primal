using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SlugBase.Features;
using SlugBase.Interface;
using UnityEngine;

namespace SlugBase;

/// <summary>
/// A character added by SlugBase.
/// </summary>
public class SlugBaseCharacter
{
	/// <summary>
	/// Stores the <see cref="T:SlugBase.Features.Feature" />s of a <see cref="T:SlugBase.SlugBaseCharacter" />.
	/// </summary>
	public class FeatureList : IEnumerable<Feature>, IEnumerable
	{
		private readonly Dictionary<Feature, object> _features = new Dictionary<Feature, object>();

		/// <summary>
		/// Get the value of a <see cref="T:SlugBase.Features.Feature`1" />.
		/// </summary>
		/// <typeparam name="T">The <see cref="T:SlugBase.Features.Feature`1" />'s data type.</typeparam>
		/// <param name="feature">The feature to get data from.</param>
		/// <param name="value">The feature's data, or <typeparamref name="T" />'s default value if it was not found.</param>
		/// <returns><c>true</c> if the feature was found, <c>false</c> otherwise.</returns>
		public bool TryGet<T>(Feature<T> feature, out T value)
		{
			if (_features.TryGetValue(feature, out var value2))
			{
				value = (T)value2;
				return true;
			}
			value = default(T);
			return false;
		}

		/// <summary>
		/// Add a <see cref="T:SlugBase.Features.Feature" /> or replace an existing <see cref="T:SlugBase.Features.Feature" />'s value.
		/// </summary>
		/// <remarks>
		/// Some features are only read occasionally, such as when starting a game or entering a room.
		/// Instead of modifying features during gameplay, consider defining a custom <see cref="T:SlugBase.Features.Feature`1" /> and corresponding <see cref="T:SlugBase.Features.Data`2" />.
		/// </remarks>
		/// <param name="feature">The feature to add or replace.</param>
		/// <param name="value">The feature's new value.</param>
		/// <exception cref="T:SlugBase.JsonException"><paramref name="value" /> was not a valid value for <paramref name="feature" />.</exception>
		public void Set(Feature feature, JsonAny value)
		{
			_features[feature] = feature.Create(value);
		}

		/// <summary>
		/// Remove a <see cref="T:SlugBase.Features.Feature" />.
		/// </summary>
		/// <remarks>
		/// Some features are only read occasionally, such as when starting a game or entering a room.
		/// Instead of modifying features during gameplay, consider defining a custom <see cref="T:SlugBase.Features.Feature`1" /> and corresponding <see cref="T:SlugBase.Features.Data`2" />.
		/// </remarks>
		/// <param name="feature">The feature to remove.</param>
		/// <returns><c>true</c> if the feature was present, <c>false</c> otherwise.</returns>
		public bool Remove(Feature feature)
		{
			return _features.Remove(feature);
		}

		/// <summary>
		/// Check this list for a <see cref="T:SlugBase.Features.Feature" />.
		/// </summary>
		/// <param name="feature">The <see cref="T:SlugBase.Features.Feature" /> to check for.</param>
		/// <returns><c>true</c> if <paramref name="feature" /> was found, <c>false</c> otherwise.</returns>
		public bool Contains(Feature feature)
		{
			return _features.ContainsKey(feature);
		}

		internal void AddMany(JsonObject json)
		{
			foreach (KeyValuePair<string, JsonAny> item in json)
			{
				if (FeatureManager.TryGetFeature(item.Key, out var feature))
				{
					_features.Add(feature, feature.Create(item.Value));
					continue;
				}
				throw new JsonException("Couldn't find feature: " + item.Key + "!", json);
			}
		}

		internal void Clear()
		{
			_features.Clear();
		}

		/// <summary>
		/// Returns an enumerator that iterates through all features in this collection.
		/// </summary>
		public IEnumerator<Feature> GetEnumerator()
		{
			return _features.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>
	/// Provides data for the <see cref="E:SlugBase.SlugBaseCharacter.Refreshed" /> event.
	/// </summary>
	public class RefreshEventArgs : EventArgs
	{
		/// <summary>
		/// The current <see cref="T:RainWorldGame" />.
		/// </summary>
		public RainWorldGame Game { get; }

		/// <summary>
		/// The ID of the reloaded <see cref="T:SlugBase.SlugBaseCharacter" />.
		/// </summary>
		public Name ID { get; }

		/// <summary>
		/// The reloaded <see cref="T:SlugBase.SlugBaseCharacter" />.
		/// </summary>
		public SlugBaseCharacter Character { get; }

		internal RefreshEventArgs(RainWorldGame game, Name id, SlugBaseCharacter character)
		{
			Game = game;
			ID = id;
			Character = character;
		}
	}

	/// <summary>
	/// Stores all registered <see cref="T:SlugBase.SlugBaseCharacter" />s.
	/// </summary>
	public static JsonRegistry<Name, SlugBaseCharacter> Registry { get; }

	/// <summary>
	/// This character's unique name.
	/// </summary>
	public Name Name { get; }

	/// <summary>
	/// The displayed name of this character, such as "The Survivor", "The Monk", or "The Hunter".
	/// </summary>
	public string DisplayName { get; set; }

	/// <summary>
	/// A description of this character that appears on the select menu.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Settings, abilities, or other <see cref="T:SlugBase.Features.Feature" />s of this character.
	/// </summary>
	public FeatureList Features { get; } = new FeatureList();


	/// <summary>
	/// Occurs when any <see cref="T:SlugBase.SlugBaseCharacter" />'s JSON file is modified, after all features have been loaded.
	/// </summary>
	/// <remarks>
	/// This event is only raised when in-game.
	/// </remarks>
	public static event EventHandler<RefreshEventArgs> Refreshed;

	public static bool TryGet(Name name, out SlugBaseCharacter character)
	{
		return Registry.TryGet(name, out character);
	}

	public static SlugBaseCharacter Get(Name name)
	{
		return Registry.GetOrDefault(name);
	}

	/// <summary>
	/// Creates a new, blank <see cref="T:SlugBase.SlugBaseCharacter" />.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:SlugBase.SlugBaseCharacter.DisplayName" />, <see cref="P:SlugBase.SlugBaseCharacter.Description" />, and <see cref="P:SlugBase.SlugBaseCharacter.Features" /> to customize this character.
	/// </remarks>
	/// <param name="id">The new character's unique ID.</param>
	/// <returns>A new <see cref="T:SlugBase.SlugBaseCharacter" /> with a default name, default description, and no features.</returns>
	public static SlugBaseCharacter Create(string id)
	{
		Dictionary<string, object> obj = new Dictionary<string, object>
		{
			{ "id", id },
			{ "name", "No Name" },
			{ "description", "No description." }
		};
		return Registry.Add(JsonConverter.ToJson(obj)).Value;
	}

	static SlugBaseCharacter()
	{
		Registry = new JsonRegistry<Name, SlugBaseCharacter>((Name key, JsonObject json) => new SlugBaseCharacter(key, json));
		Registry.EntryReloaded += delegate(object _, JsonRegistry<Name, SlugBaseCharacter>.ReloadedEventArgs args)
		{
			MainLoopProcess obj = UnityEngine.Object.FindObjectOfType<RainWorld>()?.processManager.currentMainLoop;
			RainWorldGame val = (RainWorldGame)(object)((obj is RainWorldGame) ? obj : null);
			if (val != null)
			{
				SlugBaseCharacter.Refreshed?.Invoke(_, new RefreshEventArgs(val, args.Key, args.Value));
			}
			args.Value.UpdateLegacyTimeline();
		};
	}

	private SlugBaseCharacter(Name name, JsonObject json)
	{
		Name = name;
		DisplayName = json.GetString("name");
		Description = json.GetString("description");
		Features.Clear();
		JsonObject? jsonObject = json.TryGet("features")?.AsObject();
		if (jsonObject.HasValue)
		{
			JsonObject valueOrDefault = jsonObject.GetValueOrDefault();
			if (true)
			{
				Features.AddMany(valueOrDefault);
			}
		}
	}

	/// <summary>
	/// If necessary, registers or updates a character-specific timeline that uses the pre-Watcher world state system.
	/// </summary>
	internal void UpdateLegacyTimeline()
	{
		if (!((Feature<Timeline[]>)GameFeatures.WorldState).TryGet(this, out Timeline[] value) || value.Length == 0 || !(((ExtEnumBase)value[0]).value == ((ExtEnumBase)Name).value) || (((ExtEnumBase)value[0]).Index != -1 && (!CustomTimeline.Registry.TryGet(value[0], out var value2) || CustomTimeline.Registry.TryGetPath(value[0], out var _))))
		{
			return;
		}
		try
		{
			Timeline val = value[0];
			Name[] value3;
			List<object> list = (((Feature<Name[]>)GameFeatures.TimelineBefore).TryGet(this, out value3) ? new List<object>(value3.Select((Name x) => ((ExtEnumBase)x).value)) : new List<object>());
			Name[] value4;
			List<object> list2 = (((Feature<Name[]>)GameFeatures.TimelineBefore).TryGet(this, out value4) ? new List<object>(value3.Select((Name x) => ((ExtEnumBase)x).value)) : new List<object>());
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"id",
					((ExtEnumBase)val).value
				},
				{
					"base",
					new List<object>(from x in value.Skip(1)
						select ((ExtEnumBase)x).value)
				}
			};
			if (list.Count > 0)
			{
				dictionary.Add("before", list);
			}
			if (list2.Count > 0)
			{
				dictionary.Add("after", list2);
			}
			JsonObject json = JsonConverter.ToJson(dictionary);
			if (CustomTimeline.Registry.TryGet(val, out value2))
			{
				CustomTimeline.Registry.Remove(val);
			}
			CustomTimeline.Registry.Add(json);
		}
		catch (Exception ex)
		{
			Registry.TryGetPath(Name, out var path2);
			ErrorList.Instance.AddError(ErrorList.ErrorIcon.Timeline, "Failed to create legacy timeline: " + ex.Message, path2, null);
			Debug.LogException(ex);
		}
	}
}
