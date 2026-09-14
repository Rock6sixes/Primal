using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlugBase;

/// <summary>
/// Represents a collection of values with unique IDs that can be loaded from JSON.
/// </summary>
/// <typeparam name="TKey">The type of the <see cref="T:ExtEnum`1" /> keys.</typeparam>
/// <typeparam name="TValue">The type of the values.</typeparam>
public class JsonRegistry<TKey, TValue> where TKey : ExtEnum<TKey>
{
	private readonly struct Entry
	{
		public readonly string Path;

		public readonly TValue Value;

		public Entry(string path, TValue value)
		{
			Path = path;
			Value = value;
		}
	}

	/// <summary>
	/// Provides data for the <see cref="E:SlugBase.JsonRegistry`2.EntryReloaded" /> event.
	/// </summary>
	public class ReloadedEventArgs : EventArgs
	{
		/// <summary>
		/// The unique key of the reloaded entry.
		/// </summary>
		public TKey Key { get; }

		/// <summary>
		/// The reloaded entry.
		/// </summary>
		public TValue Value { get; }

		internal ReloadedEventArgs(TKey key, TValue value)
		{
			Key = key;
			Value = value;
		}
	}

	/// <summary>
	/// Provides data for the <see cref="E:SlugBase.JsonRegistry`2.LoadFailed" /> event.
	/// </summary>
	public class LoadErrorEventArgs : EventArgs
	{
		/// <summary>
		/// The exception that caused this event, or <c>null</c> if it was not from an exception.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// An error message that may be shown to the user.
		/// </summary>
		public string ErrorMessage { get; }

		/// <summary>
		/// The path to the file that errored, or <c>null</c> if it was not loaded from a file.
		/// </summary>
		public string Path { get; }

		internal LoadErrorEventArgs(Exception exception, string path)
			: this(exception.Message, exception, path)
		{
		}

		internal LoadErrorEventArgs(string message, Exception exception, string path)
		{
			ErrorMessage = message;
			Exception = exception;
			Path = path;
		}
	}

	private readonly Dictionary<TKey, Entry> _entries = new Dictionary<TKey, Entry>();

	private readonly Func<TKey, JsonObject, TValue> _fromJson;

	private readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

	private readonly ConcurrentQueue<string> _reloadQueue = new ConcurrentQueue<string>();

	private bool _reloadOnChange;

	/// <summary>
	/// Whether this registry should track files to reload.
	/// Call <see cref="M:SlugBase.JsonRegistry`2.ReloadChangedFiles" /> to apply changes.
	/// </summary>
	public bool WatchForChanges
	{
		get
		{
			return _reloadOnChange;
		}
		set
		{
			_reloadOnChange = value;
			if (value)
			{
				return;
			}
			string result;
			while (_reloadQueue.TryDequeue(out result))
			{
			}
			foreach (FileSystemWatcher value2 in _watchers.Values)
			{
				value2.Dispose();
			}
			_watchers.Clear();
		}
	}

	/// <summary>
	/// A collection of all keys used by this registry.
	/// </summary>
	public IEnumerable<TKey> Keys => _entries.Keys;

	/// <summary>
	/// A collection of all values registered.
	/// </summary>
	public IEnumerable<TValue> Values => _entries.Values.Select((Entry entry) => entry.Value);

	/// <summary>
	/// Occurs when a loaded JSON file is modified while <see cref="P:SlugBase.JsonRegistry`2.WatchForChanges" /> is set.
	/// </summary>
	public event EventHandler<ReloadedEventArgs> EntryReloaded;

	/// <summary>
	/// Occurs when a JSON file fails to load or reload.
	/// </summary>
	public event EventHandler<LoadErrorEventArgs> LoadFailed;

	/// <summary>
	/// Create a new registry.
	/// </summary>
	/// <param name="fromJson">The factory that creates values from JSON.</param>
	public JsonRegistry(Func<TKey, JsonObject, TValue> fromJson)
	{
		_fromJson = fromJson;
	}

	/// <summary>
	/// Register all JSON files in a directory using <see cref="M:SlugBase.JsonRegistry`2.AddFromFile(System.String)" />.
	/// </summary>
	/// <param name="directory">The directory to search.</param>
	public void ScanDirectory(string directory)
	{
		string[] source = AssetManager.ListDirectory(directory, false, true, false);
		foreach (string item in source.Where((string file) => file.EndsWith(".json")))
		{
			if (IsMostRecent(item))
			{
				TryAddFromFile(item);
			}
		}
	}

	/// <summary>
	/// Parse a file as JSON and link it to a new <see cref="T:ExtEnum`1" /> ID.
	/// </summary>
	/// <param name="path">The file path to the json.</param>
	/// <returns>The registered key and value.</returns>
	public KeyValuePair<TKey, TValue> AddFromFile(string path)
	{
		path = path.Replace('/', Path.DirectorySeparatorChar);
		foreach (KeyValuePair<TKey, Entry> entry in _entries)
		{
			if (entry.Value.Path != null && entry.Value.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase))
			{
				Remove(entry.Key);
				break;
			}
		}
		KeyValuePair<TKey, TValue> result = Add(path, JsonAny.Parse(File.ReadAllText(path)).AsObject());
		if (WatchForChanges)
		{
			string directoryName = Path.GetDirectoryName(path);
			if (!_watchers.ContainsKey(directoryName))
			{
				FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(directoryName, "*.json");
				fileSystemWatcher.Changed += FileChanged;
				fileSystemWatcher.EnableRaisingEvents = true;
				_watchers.Add(directoryName, fileSystemWatcher);
			}
		}
		SlugBasePlugin.Logger.LogDebug("Added SlugBase object from " + path);
		return result;
	}

	/// <summary>
	/// Parse a file as JSON and link it to a new <see cref="T:ExtEnum`1" /> ID.
	/// Load errors can be monitored with <see cref="E:SlugBase.JsonRegistry`2.LoadFailed" />.
	/// </summary>
	/// <param name="path">The file path to the json.</param>
	/// <returns>The registered key and value, or <c>null</c> if loading failed.</returns>
	public KeyValuePair<TKey, TValue>? TryAddFromFile(string path)
	{
		try
		{
			return AddFromFile(path);
		}
		catch (Exception ex)
		{
			string message = ((!(ex is JsonException ex2)) ? ((!(ex is JsonParseException ex3)) ? ex.Message : $"{ex3.Message}\nLine: {ex3.Line + 1}") : (ex2.Message + "\nField: " + (ex2.JsonPath ?? "unknown")));
			this.LoadFailed?.Invoke(this, new LoadErrorEventArgs(message, ex, path));
			return null;
		}
	}

	/// <summary>
	/// Load a <typeparamref name="TValue" /> from JSON and link it to a new <see cref="T:ExtEnum`1" /> ID.
	/// </summary>
	/// <param name="json">The json data for the new object.</param>
	/// <returns>The registered key and value.</returns>
	public KeyValuePair<TKey, TValue> Add(JsonObject json)
	{
		return Add(null, json);
	}

	private KeyValuePair<TKey, TValue> Add(string path, JsonObject json)
	{
		TKey val = ClaimID(json.GetString("id"));
		try
		{
			Entry value = new Entry(path, _fromJson(val, json));
			_entries[val] = value;
			return new KeyValuePair<TKey, TValue>(val, value.Value);
		}
		catch
		{
			((ExtEnum<TKey>)val).Unregister();
			throw;
		}
	}

	/// <summary>
	/// Unregister a value.
	/// </summary>
	/// <param name="id"></param>
	/// <exception cref="T:System.ArgumentNullException"></exception>
	/// <exception cref="T:System.ArgumentException"></exception>
	public void Remove(TKey id)
	{
		if ((ExtEnum<TKey>)id == (ExtEnum<TKey>)null)
		{
			throw new ArgumentNullException("id");
		}
		if (!_entries.ContainsKey(id))
		{
			throw new ArgumentException($"ID \"{id}\" is not a value of {typeof(TKey).FullName}");
		}
		if (!_entries.Remove(id))
		{
			throw new ArgumentException($"ID \"{id}\" from {typeof(TKey).FullName} was not in the registry!");
		}
		((ExtEnum<TKey>)id).Unregister();
	}

	/// <summary>
	/// Get a registered value by key.
	/// </summary>
	/// <param name="id">The unique ID of the value.</param>
	/// <param name="value">The registered value with the unique ID.</param>
	/// <returns><c>true</c> if the value was found, <c>false</c> otherwise.</returns>
	public bool TryGet(TKey id, out TValue value)
	{
		if ((ExtEnum<TKey>)id != (ExtEnum<TKey>)null && _entries.TryGetValue(id, out var value2))
		{
			value = value2.Value;
			return true;
		}
		value = default(TValue);
		return false;
	}

	/// <summary>
	/// Get a registered value by key.
	/// </summary>
	/// <param name="id">The unique ID of the value.</param>
	/// <returns>The registered value with the unique ID.</returns>
	public TValue GetOrDefault(TKey id)
	{
		TValue value;
		return ((ExtEnum<TKey>)id != (ExtEnum<TKey>)null && TryGet(id, out value)) ? value : default(TValue);
	}

	/// <summary>
	/// Gets the path to a registered value by key.
	/// </summary>
	/// <param name="id">The unique ID of the value.</param>
	/// <param name="path">The path to the value's JSON file.</param>
	/// <returns><c>true</c> if the value was found and has a path, <c>false</c> otherwise.</returns>
	public bool TryGetPath(TKey id, out string path)
	{
		if ((ExtEnum<TKey>)id != (ExtEnum<TKey>)null && _entries.TryGetValue(id, out var value))
		{
			path = value.Path;
			return true;
		}
		path = null;
		return false;
	}

	/// <summary>
	/// Reload all JSON files that have been modified.
	/// </summary>
	public void ReloadChangedFiles()
	{
		string result;
		while (_reloadQueue.TryDequeue(out result))
		{
			KeyValuePair<TKey, TValue>? keyValuePair = TryAddFromFile(result);
			if (keyValuePair.HasValue)
			{
				try
				{
					this.EntryReloaded?.Invoke(this, new ReloadedEventArgs(keyValuePair.Value.Key, keyValuePair.Value.Value));
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private bool IsMostRecent(string path)
	{
		string fullPath = Path.GetFullPath(path);
		foreach (Mod activeMod in ModManager.ActiveMods)
		{
			string value = Path.GetFullPath(activeMod.TargetedPath) + Path.DirectorySeparatorChar;
			string text = Path.GetFullPath(activeMod.NewestPath) + Path.DirectorySeparatorChar;
			string text2 = Path.GetFullPath(activeMod.path) + Path.DirectorySeparatorChar;
			if (activeMod.hasTargetedVersionFolder && fullPath.StartsWith(value, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			if (activeMod.hasNewestFolder && fullPath.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
			{
				string path2 = fullPath.Substring(text.Length);
				return !activeMod.hasTargetedVersionFolder || !File.Exists(Path.Combine(activeMod.TargetedPath, path2));
			}
			if (fullPath.StartsWith(text2, StringComparison.InvariantCultureIgnoreCase))
			{
				string path3 = fullPath.Substring(text2.Length);
				return (!activeMod.hasTargetedVersionFolder || !File.Exists(Path.Combine(activeMod.TargetedPath, path3))) && (!activeMod.hasNewestFolder || !File.Exists(Path.Combine(activeMod.NewestPath, path3)));
			}
		}
		return true;
	}

	private void FileChanged(object sender, FileSystemEventArgs e)
	{
		if (IsMostRecent(e.FullPath))
		{
			_reloadQueue.Enqueue(e.FullPath);
		}
	}

	private TKey ClaimID(string id)
	{
		if (ExtEnum<TKey>.values.entries.Contains(id))
		{
			throw new ArgumentException("ID \"" + id + "\" from " + typeof(TKey).FullName + " is already in use!");
		}
		TKey result = (TKey)Activator.CreateInstance(typeof(TKey), id, true);
		ExtEnum<TKey>.valuesVersion += 1;
		return result;
	}
}
