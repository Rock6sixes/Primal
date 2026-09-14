using SlugBase.DataTypes;
using UnityEngine;

namespace SlugBase.Features;

/// <summary>
/// Helper methods to construct <see cref="T:SlugBase.Features.Feature`1" />s with simple parsing rules.
/// </summary>
public static class FeatureTypes
{
	private static JsonAny AssertLength(JsonAny json, int minLength, int maxLength)
	{
		JsonList? jsonList = json.TryList();
		if (jsonList.HasValue)
		{
			JsonList valueOrDefault = jsonList.GetValueOrDefault();
			if (true)
			{
				if (valueOrDefault.Count < minLength)
				{
					throw new JsonException($"List must contain at least {minLength} elements!", valueOrDefault);
				}
				if (valueOrDefault.Count > maxLength)
				{
					throw new JsonException($"List may not contain more than {maxLength} elements!", valueOrDefault);
				}
			}
		}
		return json;
	}

	/// <summary>Create a player feature that takes one integer.</summary>
	public static PlayerFeature<int> PlayerInt(string id)
	{
		return new PlayerFeature<int>(id, JsonUtils.ToInt);
	}

	/// <summary>Create a player feature that takes one integer.</summary>
	public static PlayerFeature<long> PlayerLong(string id)
	{
		return new PlayerFeature<long>(id, JsonUtils.ToLong);
	}

	/// <summary>Create a player feature that takes one number.</summary>
	public static PlayerFeature<double> PlayerDouble(string id)
	{
		return new PlayerFeature<double>(id, JsonUtils.ToDouble);
	}

	/// <summary>Create a player feature that takes one number.</summary>
	public static PlayerFeature<float> PlayerFloat(string id)
	{
		return new PlayerFeature<float>(id, JsonUtils.ToFloat);
	}

	/// <summary>Create a player feature that takes one string.</summary>
	public static PlayerFeature<string> PlayerString(string id)
	{
		return new PlayerFeature<string>(id, JsonUtils.ToString);
	}

	/// <summary>Create a player feature that takes a slugcat name.</summary>
	public static PlayerFeature<Name> PlayerSlugcatName(string id)
	{
		return new PlayerFeature<Name>(id, JsonUtils.ToSlugcatName);
	}

	/// <summary>Create a player feature that takes an array of integers.</summary>
	public static PlayerFeature<int[]> PlayerInts(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new PlayerFeature<int[]>(id, (JsonAny json) => JsonUtils.ToInts(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a player feature that takes an array of integers.</summary>
	public static PlayerFeature<long[]> PlayerLongs(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new PlayerFeature<long[]>(id, (JsonAny json) => JsonUtils.ToLongs(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a player feature that takes an array of numbers.</summary>
	public static PlayerFeature<double[]> PlayerDoubles(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new PlayerFeature<double[]>(id, (JsonAny json) => JsonUtils.ToDoubles(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a player feature that takes an array of numbers.</summary>
	public static PlayerFeature<float[]> PlayerFloats(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new PlayerFeature<float[]>(id, (JsonAny json) => JsonUtils.ToFloats(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a player feature that takes an array of strings.</summary>
	public static PlayerFeature<string[]> PlayerStrings(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new PlayerFeature<string[]>(id, (JsonAny json) => JsonUtils.ToStrings(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a player feature that takes an array of slugcat names.</summary>
	public static PlayerFeature<Name[]> PlayerSlugcatNames(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new PlayerFeature<Name[]>(id, (JsonAny json) => JsonUtils.ToSlugcatNames(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a player feature that takes a color.</summary>
	public static PlayerFeature<Color> PlayerColor(string id)
	{
		return new PlayerFeature<Color>(id, JsonUtils.ToColor);
	}

	/// <summary>Create a player feature that takes a palette-modified color.</summary>
	public static PlayerFeature<PlayerColor> PlayerCustomColor(string id)
	{
		return new PlayerFeature<PlayerColor>(id, JsonUtils.ToPlayerColor);
	}

	/// <summary>Create a player feature that takes one boolean.</summary>
	public static PlayerFeature<bool> PlayerBool(string id)
	{
		return new PlayerFeature<bool>(id, JsonUtils.ToBool);
	}

	/// <summary>Create a player feature that takes one enum value.</summary>
	public static PlayerFeature<T> PlayerEnum<T>(string id) where T : struct
	{
		return new PlayerFeature<T>(id, JsonUtils.ToEnum<T>);
	}

	/// <summary>Create a player feature that takes one enum value.</summary>
	public static PlayerFeature<T> PlayerExtEnum<T>(string id) where T : ExtEnum<T>
	{
		return new PlayerFeature<T>(id, JsonUtils.ToExtEnum<T>);
	}

	/// <summary>Create a player feature that takes an array of enum values.</summary>
	public static PlayerFeature<T[]> PlayerExtEnums<T>(string id, int minLength = 0, int maxLength = int.MaxValue) where T : ExtEnum<T>
	{
		return new PlayerFeature<T[]>(id, (JsonAny json) => JsonUtils.ToExtEnums<T>(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a game feature that takes one integer.</summary>
	public static GameFeature<int> GameInt(string id)
	{
		return new GameFeature<int>(id, JsonUtils.ToInt);
	}

	/// <summary>Create a game feature that takes one integer.</summary>
	public static GameFeature<long> GameLong(string id)
	{
		return new GameFeature<long>(id, JsonUtils.ToLong);
	}

	/// <summary>Create a game feature that takes one number.</summary>
	public static GameFeature<double> GameDouble(string id)
	{
		return new GameFeature<double>(id, JsonUtils.ToDouble);
	}

	/// <summary>Create a game feature that takes one number.</summary>
	public static GameFeature<float> GameFloat(string id)
	{
		return new GameFeature<float>(id, JsonUtils.ToFloat);
	}

	/// <summary>Create a game feature that takes one string.</summary>
	public static GameFeature<string> GameString(string id)
	{
		return new GameFeature<string>(id, JsonUtils.ToString);
	}

	/// <summary>Create a game feature that takes a slugcat name.</summary>
	public static GameFeature<Name> GameSlugcatName(string id)
	{
		return new GameFeature<Name>(id, JsonUtils.ToSlugcatName);
	}

	/// <summary>Create a game feature that takes an array of integers.</summary>
	public static GameFeature<int[]> GameInts(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new GameFeature<int[]>(id, (JsonAny json) => JsonUtils.ToInts(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a game feature that takes an array of integers.</summary>
	public static GameFeature<long[]> GameLongs(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new GameFeature<long[]>(id, (JsonAny json) => JsonUtils.ToLongs(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a game feature that takes an array of numbers.</summary>
	public static GameFeature<double[]> GameDoubles(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new GameFeature<double[]>(id, (JsonAny json) => JsonUtils.ToDoubles(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a game feature that takes an array of numbers.</summary>
	public static GameFeature<float[]> GameFloats(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new GameFeature<float[]>(id, (JsonAny json) => JsonUtils.ToFloats(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a game feature that takes an array of strings.</summary>
	public static GameFeature<string[]> GameStrings(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new GameFeature<string[]>(id, (JsonAny json) => JsonUtils.ToStrings(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a game feature that takes an array of slugcat names.</summary>
	public static GameFeature<Name[]> GameSlugcatNames(string id, int minLength = 0, int maxLength = int.MaxValue)
	{
		return new GameFeature<Name[]>(id, (JsonAny json) => JsonUtils.ToSlugcatNames(AssertLength(json, minLength, maxLength)));
	}

	/// <summary>Create a game feature that takes a color.</summary>
	public static GameFeature<Color> GameColor(string id)
	{
		return new GameFeature<Color>(id, JsonUtils.ToColor);
	}

	/// <summary>Create a game feature that takes a palette-modified color.</summary>
	public static GameFeature<PlayerColor> GameCustomColor(string id)
	{
		return new GameFeature<PlayerColor>(id, JsonUtils.ToPlayerColor);
	}

	/// <summary>Create a game feature that takes one boolean.</summary>
	public static GameFeature<bool> GameBool(string id)
	{
		return new GameFeature<bool>(id, JsonUtils.ToBool);
	}

	/// <summary>Create a game feature that takes one enum value.</summary>
	public static GameFeature<T> GameEnum<T>(string id) where T : struct
	{
		return new GameFeature<T>(id, JsonUtils.ToEnum<T>);
	}

	/// <summary>Create a game feature that takes one enum value.</summary>
	public static GameFeature<T> GameExtEnum<T>(string id) where T : ExtEnum<T>
	{
		return new GameFeature<T>(id, JsonUtils.ToExtEnum<T>);
	}

	/// <summary>Create a game feature that takes an array of enum values.</summary>
	public static GameFeature<T[]> GameExtEnums<T>(string id, int minLength = 0, int maxLength = int.MaxValue) where T : ExtEnum<T>
	{
		return new GameFeature<T[]>(id, (JsonAny json) => JsonUtils.ToExtEnums<T>(AssertLength(json, minLength, maxLength)));
	}
}
