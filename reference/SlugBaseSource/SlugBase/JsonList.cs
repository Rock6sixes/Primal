using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SlugBase;

/// <summary>
/// A JSON list.
/// </summary>
public readonly struct JsonList : IReadOnlyList<JsonAny>, IReadOnlyCollection<JsonAny>, IEnumerable<JsonAny>, IEnumerable
{
	[CompilerGenerated]
	private sealed class _003CGetEnumerator_003Ed__18 : IEnumerator<JsonAny>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private JsonAny _003C_003E2__current;

		public JsonList _003C_003E4__this;

		private int _003Ci_003E5__1;

		JsonAny IEnumerator<JsonAny>.Current
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
		public _003CGetEnumerator_003Ed__18(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__1 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__1++;
				break;
			}
			if (_003Ci_003E5__1 < _003C_003E4__this._list.Count)
			{
				_003C_003E2__current = _003C_003E4__this.Get(_003Ci_003E5__1);
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	internal readonly List<object> _list;

	internal readonly JsonPathNode _path;

	/// <summary>
	/// Get the number of elements in this list.
	/// </summary>
	public int Count => _list.Count;

	/// <summary>
	/// Get an element from this list.
	/// </summary>
	/// <param name="key">The index.</param>
	/// <returns>The found value.</returns>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range.</exception>
	public JsonAny this[int key] => Get(key);

	internal JsonList(List<object> list, JsonPathNode path)
	{
		_list = list;
		_path = path;
	}

	/// <summary>
	/// Get an element from this list.
	/// </summary>
	/// <param name="key">The index.</param>
	/// <returns>The found value.</returns>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range.</exception>
	public JsonAny Get(int key)
	{
		if (key < 0)
		{
			throw new JsonException($"Index {key} may not be negative!", this);
		}
		if (key >= _list.Count)
		{
			throw new JsonException($"Index {key} is out of range for a list of size {_list.Count}!", this);
		}
		return new JsonAny(_list[key], new JsonPathNode($"[{key}]", _path));
	}

	/// <summary>
	/// Get an element from this list.
	/// </summary>
	/// <param name="key">The index.</param>
	/// <returns>The found value, or <c>null</c> if <paramref name="key" /> is out of range.</returns>
	public JsonAny? TryGet(int key)
	{
		if (key >= 0 && key < _list.Count)
		{
			return new JsonAny(_list[key], new JsonPathNode($"[{key}]", _path));
		}
		return null;
	}

	/// <summary>Get a <see cref="T:SlugBase.JsonObject" /> from this list.</summary>
	/// <param name="key">The index.</param>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range or is not the right type.</exception>
	public JsonObject GetObject(int key)
	{
		return Get(key).AsObject();
	}

	/// <summary>Get a <see cref="T:SlugBase.JsonList" /> from this list.</summary>
	/// <param name="key">The index.</param>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range or is not the right type.</exception>
	public JsonList GetList(int key)
	{
		return Get(key).AsList();
	}

	/// <summary>Get a <see cref="T:System.Double" /> from this list.</summary>
	/// <param name="key">The index.</param>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range or is not the right type.</exception>
	public double GetDouble(int key)
	{
		return Get(key).AsDouble();
	}

	/// <summary>Get a <see cref="T:System.Single" /> from this list.</summary>
	/// <param name="key">The index.</param>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range or is not the right type.</exception>
	public float GetFloat(int key)
	{
		return Get(key).AsFloat();
	}

	/// <summary>Get a <see cref="T:System.Int64" /> from this list.</summary>
	/// <param name="key">The index.</param>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range or is not the right type.</exception>
	public long GetLong(int key)
	{
		return Get(key).AsLong();
	}

	/// <summary>Get an <see cref="T:System.Int32" /> from this list.</summary>
	/// <param name="key">The index.</param>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range or is not the right type.</exception>
	public int GetInt(int key)
	{
		return Get(key).AsInt();
	}

	/// <summary>Get a <see cref="T:System.String" /> from this list.</summary>
	/// <param name="key">The index.</param>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range or is not the right type.</exception>
	public string GetString(int key)
	{
		return Get(key).AsString();
	}

	/// <summary>Get a <see cref="T:System.Boolean" /> from this list.</summary>
	/// <param name="key">The index.</param>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="key" /> is out of range or is not the right type.</exception>
	public bool GetBool(int key)
	{
		return Get(key).AsBool();
	}

	/// <summary>
	/// Get <paramref name="list" /> as a <see cref="T:SlugBase.JsonAny" />.
	/// </summary>
	public static implicit operator JsonAny(JsonList list)
	{
		return new JsonAny(list._list, list._path);
	}

	/// <summary>
	/// Gets an enumerator for all elements of this list.
	/// </summary>
	[IteratorStateMachine(typeof(_003CGetEnumerator_003Ed__18))]
	public IEnumerator<JsonAny> GetEnumerator()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetEnumerator_003Ed__18(0)
		{
			_003C_003E4__this = this
		};
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
