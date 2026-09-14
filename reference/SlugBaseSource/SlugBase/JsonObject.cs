using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SlugBase;

/// <summary>
/// A JSON object.
/// </summary>
public readonly struct JsonObject : IEnumerable<KeyValuePair<string, JsonAny>>, IEnumerable
{
	[CompilerGenerated]
	private sealed class _003CGetEnumerator_003Ed__16 : IEnumerator<KeyValuePair<string, JsonAny>>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private KeyValuePair<string, JsonAny> _003C_003E2__current;

		public JsonObject _003C_003E4__this;

		private Dictionary<string, object>.KeyCollection.Enumerator _003C_003Es__1;

		private string _003Ckey_003E5__2;

		KeyValuePair<string, JsonAny> IEnumerator<KeyValuePair<string, JsonAny>>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetEnumerator_003Ed__16(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003Es__1 = default(Dictionary<string, object>.KeyCollection.Enumerator);
			_003Ckey_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003Es__1 = _003C_003E4__this._dict.Keys.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					_003Ckey_003E5__2 = null;
					break;
				}
				if (_003C_003Es__1.MoveNext())
				{
					_003Ckey_003E5__2 = _003C_003Es__1.Current;
					_003C_003E2__current = new KeyValuePair<string, JsonAny>(_003Ckey_003E5__2, _003C_003E4__this.Get(_003Ckey_003E5__2));
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003Es__1 = default(Dictionary<string, object>.KeyCollection.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003Es__1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	internal readonly Dictionary<string, object> _dict;

	internal readonly JsonPathNode _path;

	/// <summary>
	/// Get an element from this object.
	/// </summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <returns>The found value.</returns>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist.</exception>
	public JsonAny this[string key] => Get(key);

	internal JsonObject(Dictionary<string, object> dict, JsonPathNode path)
	{
		_dict = dict;
		_path = path;
	}

	/// <summary>
	/// Get an element from this object.
	/// </summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <returns>The found value.</returns>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist.</exception>
	public JsonAny Get(string key)
	{
		if (!_dict.TryGetValue(key, out var value))
		{
			throw new JsonException("Missing \"" + key + "\" property!", this);
		}
		return new JsonAny(value, new JsonPathNode("." + key, _path));
	}

	/// <summary>
	/// Get an element from this object.
	/// </summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <returns>The found value, or <c>null</c> if the property doesn't exist.</returns>
	public JsonAny? TryGet(string key)
	{
		if (_dict.TryGetValue(key, out var value))
		{
			return new JsonAny(value, new JsonPathNode("." + key, _path));
		}
		return null;
	}

	/// <summary>Get a <see cref="T:SlugBase.JsonObject" /> from this object.</summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist.</exception>
	public JsonObject GetObject(string key)
	{
		return Get(key).AsObject();
	}

	/// <summary>Get a <see cref="T:SlugBase.JsonList" /> from this object.</summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist or was the wrong type.</exception>
	public JsonList GetList(string key)
	{
		return Get(key).AsList();
	}

	/// <summary>Get a <see cref="T:System.Double" /> from this object.</summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist or was the wrong type.</exception>
	public double GetDouble(string key)
	{
		return Get(key).AsDouble();
	}

	/// <summary>Get a <see cref="T:System.Single" /> from this object.</summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist or was the wrong type.</exception>
	public float GetFloat(string key)
	{
		return Get(key).AsFloat();
	}

	/// <summary>Get a <see cref="T:System.Int64" /> from this object.</summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist or was the wrong type.</exception>
	public long GetLong(string key)
	{
		return Get(key).AsLong();
	}

	/// <summary>Get an <see cref="T:System.Int32" /> from this object.</summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist or was the wrong type.</exception>
	public int GetInt(string key)
	{
		return Get(key).AsInt();
	}

	/// <summary>Get a <see cref="T:System.String" /> from this object.</summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist or was the wrong type.</exception>
	public string GetString(string key)
	{
		return Get(key).AsString();
	}

	/// <summary>Get a <see cref="T:System.Boolean" /> from this object.</summary>
	/// <param name="key">The JSON property to search for.</param>
	/// <exception cref="T:SlugBase.JsonException">The specified property doesn't exist or was the wrong type.</exception>
	public bool GetBool(string key)
	{
		return Get(key).AsBool();
	}

	/// <summary>
	/// Get <paramref name="obj" /> as a <see cref="T:SlugBase.JsonAny" />.
	/// </summary>
	public static implicit operator JsonAny(JsonObject obj)
	{
		return new JsonAny(obj._dict, obj._path);
	}

	/// <summary>
	/// Get an enumerator for all properties and values of this object.
	/// </summary>
	[IteratorStateMachine(typeof(_003CGetEnumerator_003Ed__16))]
	public IEnumerator<KeyValuePair<string, JsonAny>> GetEnumerator()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetEnumerator_003Ed__16(0)
		{
			_003C_003E4__this = this
		};
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
