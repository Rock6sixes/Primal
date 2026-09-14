using System;
using System.Linq;
using SlugBase.DataTypes;
using UnityEngine;

namespace SlugBase;

/// <summary>
/// Converts <see cref="T:SlugBase.JsonAny" /> to other types.
/// </summary>
public static class JsonUtils
{
	private static float ToColorElement(JsonAny json)
	{
		JsonAny.Element type = json.Type;
		JsonAny.Element element = type;
		if (element == JsonAny.Element.Number)
		{
			float num = json.AsFloat();
			if (num < 0f || num > 1f)
			{
				throw new JsonException("Color element was out of range!", json);
			}
			return num;
		}
		throw new JsonException("Color element wasn't a number!", json);
	}

	/// <summary>Convert to <see cref="T:System.Int64" />.</summary>
	public static long ToLong(JsonAny json)
	{
		return json.AsLong();
	}

	/// <summary>Convert to <see cref="T:System.Int32" />.</summary>
	public static int ToInt(JsonAny json)
	{
		return json.AsInt();
	}

	/// <summary>Convert to <see cref="T:System.Double" />.</summary>
	public static double ToDouble(JsonAny json)
	{
		return json.AsDouble();
	}

	/// <summary>Convert to <see cref="T:System.Single" />.</summary>
	public static float ToFloat(JsonAny json)
	{
		return json.AsFloat();
	}

	/// <summary>Convert to <see cref="T:System.String" />.</summary>
	public static string ToString(JsonAny json)
	{
		return json.AsString();
	}

	/// <summary>Convert to <see cref="T:System.Boolean" />.</summary>
	public static bool ToBool(JsonAny json)
	{
		return json.AsBool();
	}

	/// <summary>Convert to <see cref="T:SlugcatStats.Name" />.</summary>
	public static Name ToSlugcatName(JsonAny json)
	{
		return Utils.GetName(json.AsString());
	}

	/// <summary>
	/// Convert to <see cref="T:UnityEngine.Color" />.
	/// </summary>
	/// <remarks>
	/// This may be a hex string or equivalent integer; list of components; or object with "r", "g", "b", and possibly "a" properties.
	/// </remarks>
	public static Color ToColor(JsonAny json)
	{
		switch (json.Type)
		{
		case JsonAny.Element.String:
			try
			{
				return RXUtils.GetColorFromHex(json.AsString());
			}
			catch (Exception inner)
			{
				throw new JsonException("Could not convert hex string to color!", inner, json);
			}
		case JsonAny.Element.Number:
		{
			long num = json.AsLong();
			if (num < 0 || num > uint.MaxValue)
			{
				throw new JsonException("Integer color was out of range!", json);
			}
			return RXUtils.GetColorFromHex((uint)num);
		}
		case JsonAny.Element.List:
		{
			JsonList jsonList = json.AsList();
			if (jsonList.Count < 3 || jsonList.Count > 4)
			{
				throw new JsonException("Color list must have 3 or 4 elements!", json);
			}
			Color result = new Color(0f, 0f, 0f, 1f);
			for (int i = 0; i < jsonList.Count; i++)
			{
				result[i] = ToColorElement(jsonList[i]);
			}
			return result;
		}
		case JsonAny.Element.Object:
		{
			JsonObject jsonObject = json.AsObject();
			float r = ToColorElement(jsonObject.Get("r"));
			float g = ToColorElement(jsonObject.Get("g"));
			float b = ToColorElement(jsonObject.Get("b"));
			JsonAny? jsonAny = jsonObject.TryGet("a");
			float a;
			if (jsonAny.HasValue)
			{
				JsonAny valueOrDefault = jsonAny.GetValueOrDefault();
				a = ToColorElement(valueOrDefault);
			}
			else
			{
				a = 1f;
			}
			return new Color(r, g, b, a);
		}
		default:
			throw new JsonException("Invalid color!", json);
		}
	}

	/// <summary>Convert to <see cref="T:SlugBase.DataTypes.PlayerColor" /> via <see cref="M:SlugBase.DataTypes.PlayerColor.#ctor(SlugBase.JsonAny)" />.</summary>
	public static PlayerColor ToPlayerColor(JsonAny json)
	{
		return new PlayerColor(json);
	}

	/// <summary>Convert list to <see cref="T:System.Int64" />[].</summary>
	public static long[] ToLongs(JsonAny json)
	{
		return (!json.TryLong().HasValue) ? json.AsList().Select(ToLong).ToArray() : new long[1] { json.AsLong() };
	}

	/// <summary>Convert list to <see cref="T:System.Int32" />[].</summary>
	public static int[] ToInts(JsonAny json)
	{
		return (!json.TryInt().HasValue) ? json.AsList().Select(ToInt).ToArray() : new int[1] { json.AsInt() };
	}

	/// <summary>Convert list to <see cref="T:System.Double" />[].</summary>
	public static double[] ToDoubles(JsonAny json)
	{
		return (!json.TryDouble().HasValue) ? json.AsList().Select(ToDouble).ToArray() : new double[1] { json.AsDouble() };
	}

	/// <summary>Convert list to <see cref="T:System.Single" />[].</summary>
	public static float[] ToFloats(JsonAny json)
	{
		return (!json.TryFloat().HasValue) ? json.AsList().Select(ToFloat).ToArray() : new float[1] { json.AsFloat() };
	}

	/// <summary>Convert list to <see cref="T:System.String" />[].</summary>
	public static string[] ToStrings(JsonAny json)
	{
		return (json.TryString() == null) ? json.AsList().Select(ToString).ToArray() : new string[1] { json.AsString() };
	}

	/// <summary>Convert list to <see cref="T:SlugcatStats.Name" />[].</summary>
	public static Name[] ToSlugcatNames(JsonAny json)
	{
		return (Name[])((json.TryString() == null) ? ((Array)json.AsList().Select(ToSlugcatName).ToArray()) : ((Array)new Name[1] { ToSlugcatName(json) }));
	}

	/// <summary>Convert to <see cref="T:System.Enum" /> value.</summary>
	public static T ToEnum<T>(JsonAny json) where T : struct
	{
		if (Enum.TryParse<T>(json.AsString(), ignoreCase: true, out var result))
		{
			return result;
		}
		throw new JsonException("\"" + json.AsString() + "\" was not a value of \"" + typeof(T).Name + "\"!", json);
	}

	/// <summary>
	/// Convert to <see cref="T:ExtEnum`1" /> value.
	/// </summary>
	/// <remarks>
	/// <see cref="P:ExtEnumBase.Index" /> will be -1 for values that could not be parsed.
	/// </remarks>
	public static T ToExtEnum<T>(JsonAny json) where T : ExtEnum<T>
	{
		return (T)Activator.CreateInstance(typeof(T), Utils.MatchCaseInsensitiveEnum<T>(json.AsString()), false);
	}

	/// <summary>
	/// Convert list to <see cref="T:ExtEnum`1" />[].
	/// </summary>
	/// <remarks>
	/// <see cref="P:ExtEnumBase.Index" /> will be -1 for values that could not be parsed.
	/// </remarks>
	public static T[] ToExtEnums<T>(JsonAny json) where T : ExtEnum<T>
	{
		if (json.TryString() == null)
		{
			return json.AsList().Select(ToExtEnum<T>).ToArray();
		}
		return new T[1] { ToExtEnum<T>(json) };
	}

	/// <summary>
	/// Convert to <see cref="T:UnityEngine.Vector2" />
	/// </summary>
	/// <remarks>
	/// This may be a list of components or an object with "x" and "y" properties.
	/// </remarks>
	public static Vector2 ToVector2(JsonAny json)
	{
		switch (json.Type)
		{
		case JsonAny.Element.List:
		{
			JsonList jsonList = json.AsList();
			if (jsonList.Count != 2)
			{
				throw new JsonException("2D vector must contain 2 values!", json);
			}
			return new Vector2(jsonList.GetFloat(0), jsonList.GetFloat(1));
		}
		case JsonAny.Element.Object:
		{
			JsonObject jsonObject = json.AsObject();
			return new Vector2(jsonObject.GetFloat("x"), jsonObject.GetFloat("y"));
		}
		default:
			throw new JsonException("Invalid 2D vector!", json);
		}
	}
}
