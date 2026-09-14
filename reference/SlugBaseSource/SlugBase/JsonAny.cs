using System;
using System.Collections.Generic;

namespace SlugBase;

/// <summary>
/// A JSON object, list, number, or string.
/// </summary>
public readonly struct JsonAny
{
	/// <summary>
	/// Represents the type of a JSON element.
	/// </summary>
	public enum Element
	{
		/// <summary>
		/// Values associated with string keys.
		/// </summary>
		Object,
		/// <summary>
		/// Values associated with integer keys.
		/// </summary>
		List,
		/// <summary>
		/// A number.
		/// </summary>
		Number,
		/// <summary>
		/// A string.
		/// </summary>
		String,
		/// <summary>
		/// A boolean.
		/// </summary>
		Bool,
		/// <summary>
		/// A <c>null</c> value.
		/// </summary>
		Null
	}

	internal readonly object _object;

	internal readonly JsonPathNode _path;

	/// <summary>
	/// The type of this element.
	/// </summary>
	public Element Type
	{
		get
		{
			object @object = _object;
			if (1 == 0)
			{
			}
			Element result;
			if (!(@object is Dictionary<string, object>))
			{
				if (!(@object is List<object>))
				{
					if (!(@object is double))
					{
						if (!(@object is string))
						{
							if (!(@object is bool))
							{
								if (@object != null)
								{
									throw new InvalidOperationException("Invalid value in JsonAny!");
								}
								result = Element.Null;
							}
							else
							{
								result = Element.Bool;
							}
						}
						else
						{
							result = Element.String;
						}
					}
					else
					{
						result = Element.Number;
					}
				}
				else
				{
					result = Element.List;
				}
			}
			else
			{
				result = Element.Object;
			}
			if (1 == 0)
			{
			}
			return result;
		}
	}

	internal JsonAny(object obj, JsonPathNode path)
	{
		_object = obj;
		_path = path;
	}

	/// <summary>Cast to <see cref="T:SlugBase.JsonObject" />.</summary>
	/// <exception cref="T:SlugBase.JsonException">This isn't a JSON object.</exception>
	public JsonObject AsObject()
	{
		if (_object is Dictionary<string, object> dict)
		{
			return new JsonObject(dict, _path);
		}
		throw new JsonException($"Json {Type} cannot be converted to Object!", this);
	}

	/// <summary>Cast to <see cref="T:SlugBase.JsonList" />.</summary>
	/// <exception cref="T:SlugBase.JsonException">This isn't a JSON list.</exception>
	public JsonList AsList()
	{
		if (_object is List<object> list)
		{
			return new JsonList(list, _path);
		}
		throw new JsonException($"Json {Type} cannot be converted to List!", this);
	}

	/// <summary>Cast to <see cref="T:System.Double" />.</summary>
	/// <exception cref="T:SlugBase.JsonException">This isn't a number.</exception>
	public double AsDouble()
	{
		if (_object is double result)
		{
			return result;
		}
		throw new JsonException($"Json {Type} cannot be converted to Double!", this);
	}

	/// <summary>Cast to <see cref="T:System.Int64" />.</summary>
	/// <exception cref="T:SlugBase.JsonException">This isn't a number.</exception>
	public long AsLong()
	{
		return (long)AsDouble();
	}

	/// <summary>Cast to <see cref="T:System.String" />.</summary>
	/// <exception cref="T:SlugBase.JsonException">This isn't a string.</exception>
	public string AsString()
	{
		if (_object is string result)
		{
			return result;
		}
		throw new JsonException($"Json {Type} cannot be converted to String!", this);
	}

	/// <summary>Cast to <see cref="T:System.Int32" />.</summary>
	/// <exception cref="T:SlugBase.JsonException">This isn't a number.</exception>
	public int AsInt()
	{
		return (int)AsDouble();
	}

	/// <summary>Cast to <see cref="T:System.Single" />.</summary>
	/// <exception cref="T:SlugBase.JsonException">This isn't a number.</exception>
	public float AsFloat()
	{
		return (float)AsDouble();
	}

	/// <summary>Cast to <see cref="T:System.Boolean" />.</summary>
	/// <exception cref="T:SlugBase.JsonException">This isn't a boolean.</exception>
	public bool AsBool()
	{
		if (_object is bool result)
		{
			return result;
		}
		throw new JsonException($"Json {Type} cannot be converted to Bool!", this);
	}

	/// <summary>Try casting to <see cref="T:SlugBase.JsonObject" />, returning <c>null</c> on failure.</summary>
	public JsonObject? TryObject()
	{
		return (Type == Element.Object) ? new JsonObject?(AsObject()) : null;
	}

	/// <summary>Try casting to <see cref="T:SlugBase.JsonList" />, returning <c>null</c> on failure.</summary>
	public JsonList? TryList()
	{
		return (Type == Element.List) ? new JsonList?(AsList()) : null;
	}

	/// <summary>Try casting to <see cref="T:System.Int64" />, returning <c>null</c> on failure.</summary>
	public long? TryLong()
	{
		return (Type == Element.Number) ? new long?(AsLong()) : null;
	}

	/// <summary>Try casting to <see cref="T:System.Int32" />, returning <c>null</c> on failure.</summary>
	public int? TryInt()
	{
		return (Type == Element.Number) ? new int?(AsInt()) : null;
	}

	/// <summary>Try casting to <see cref="T:System.Double" />, returning <c>null</c> on failure.</summary>
	public double? TryDouble()
	{
		return (Type == Element.Number) ? new double?(AsDouble()) : null;
	}

	/// <summary>Try casting to <see cref="T:System.Single" />, returning <c>null</c> on failure.</summary>
	public float? TryFloat()
	{
		return (Type == Element.Number) ? new float?(AsFloat()) : null;
	}

	/// <summary>Try casting to <see cref="T:System.String" />, returning <c>null</c> on failure.</summary>
	public string TryString()
	{
		return (Type == Element.String) ? AsString() : null;
	}

	/// <summary>Try casting to <see cref="T:System.Boolean" />, returning <c>null</c> on failure.</summary>
	public bool? TryBool()
	{
		return (Type == Element.Bool) ? new bool?(AsBool()) : null;
	}

	/// <summary>Test if this value is <c>null</c>.</summary>
	public bool IsNull()
	{
		return Type == Element.Null;
	}

	/// <summary>Cast to <see cref="T:SlugBase.JsonObject" />.</summary>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="json" /> isn't a JSON object.</exception>
	public static JsonObject AsObject(JsonAny json)
	{
		return json.AsObject();
	}

	/// <summary>Cast to <see cref="T:SlugBase.JsonList" />.</summary>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="json" /> isn't a JSON list.</exception>
	public static JsonList AsList(JsonAny json)
	{
		return json.AsList();
	}

	/// <summary>Cast to <see cref="T:System.Double" />.</summary>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="json" /> isn't a number.</exception>
	public static double AsDouble(JsonAny json)
	{
		return json.AsDouble();
	}

	/// <summary>Cast to <see cref="T:System.Single" />.</summary>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="json" /> isn't a number.</exception>
	public static float AsFloat(JsonAny json)
	{
		return json.AsFloat();
	}

	/// <summary>Cast to <see cref="T:System.Int64" />.</summary>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="json" /> isn't a number.</exception>
	public static long AsLong(JsonAny json)
	{
		return json.AsLong();
	}

	/// <summary>Cast to <see cref="T:System.Int32" />.</summary>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="json" /> isn't a number.</exception>
	public static int AsInt(JsonAny json)
	{
		return json.AsInt();
	}

	/// <summary>Cast to <see cref="T:System.String" />.</summary>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="json" /> isn't a string.</exception>
	public static string AsString(JsonAny json)
	{
		return json.AsString();
	}

	/// <summary>Cast to <see cref="T:System.Boolean" />.</summary>
	/// <exception cref="T:SlugBase.JsonException"><paramref name="json" /> isn't a boolean.</exception>
	public static bool AsBool(JsonAny json)
	{
		return json.AsBool();
	}

	/// <summary>
	/// Parse JSON text as a <see cref="T:SlugBase.JsonAny" />.
	/// </summary>
	/// <param name="data">The JSON text.</param>
	/// <returns>A <see cref="T:SlugBase.JsonAny" /> representing the root element.</returns>
	/// <exception cref="T:SlugBase.JsonParseException">The root element could not be parsed.</exception>
	public static JsonAny Parse(string data)
	{
		return new JsonAny(JsonParser.Parse(data), new JsonPathNode("root", null));
	}
}
