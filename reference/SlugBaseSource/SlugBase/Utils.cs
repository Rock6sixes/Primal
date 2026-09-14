using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SlugBase;

internal static class Utils
{
	/// <summary>
	/// Gets the first non-null, registered enum in <paramref name="values" />.
	/// </summary>
	/// <param name="values">The collection to search through.</param>
	/// <returns>A registered enum, or <c>null</c> if no enums in the list are registered.</returns>
	public static T FirstValidEnum<T>(IEnumerable<T> values) where T : ExtEnum<T>
	{
		return values.FirstOrDefault((T val) => (ExtEnum<T>)val != (ExtEnum<T>)null && ((ExtEnumBase)(object)val).Index != -1);
	}

	/// <summary>
	/// Gets all non-null, registered enums in <paramref name="values" />.
	/// </summary>
	/// <param name="values">The collection to search through.</param>
	public static List<T> AllValidEnums<T>(IEnumerable<T> values) where T : ExtEnum<T>
	{
		return values.Where((T val) => (ExtEnum<T>)val != (ExtEnum<T>)null && ((ExtEnumBase)(object)val).Index != -1).ToList();
	}

	/// <summary>
	/// Gets the <see cref="T:SlugcatStats.Name" /> corresponding to <paramref name="text" />, converting canon character names to code names.
	/// </summary>
	/// <param name="text">The name of the character to convert to an ID.</param>
	public static Name GetName(string text)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		ExtEnumBase val = default(ExtEnumBase);
		if (ExtEnumBase.TryParse(typeof(Name), text, true, ref val))
		{
			return (Name)val;
		}
		string text2 = text.ToLowerInvariant();
		if (1 == 0)
		{
		}
		Name result = (Name)(text2 switch
		{
			"survivor" => Name.White, 
			"hunter" => Name.Red, 
			"monk" => Name.Yellow, 
			"spearmaster" => (object)new Name("Spear", false), 
			"inv" => (object)new Name("Inv", false), 
			_ => (object)new Name(text, false), 
		});
		if (1 == 0)
		{
		}
		return result;
	}

	/// <summary>
	/// Changes the case of <paramref name="name" /> to match the corresponding enum in <typeparamref name="T" />.
	/// </summary>
	/// <param name="name">The string to capitalize.</param>
	/// <returns>The value of an enum in <typeparamref name="T" />, or <paramref name="name" /> if no matching entry exists.</returns>
	public static string MatchCaseInsensitiveEnum<T>(string name) where T : ExtEnum<T>
	{
		return ExtEnum<T>.values.entries.FirstOrDefault((string value) => value.Equals(name, StringComparison.InvariantCultureIgnoreCase)) ?? name;
	}

	/// <summary>
	/// Converts a list of hexadecimal strings to colors.
	/// </summary>
	public static List<Color> StringsToColors(IEnumerable<string> stringList)
	{
		if (stringList == null)
		{
			return null;
		}
		List<Color> list = new List<Color>();
		foreach (string @string in stringList)
		{
			ColorUtility.TryParseHtmlString("#" + @string, out var color);
			list.Add(color);
		}
		return list;
	}

	/// <summary>
	/// Converts a list of colors to hexadecimal strings.
	/// </summary>
	public static List<string> ColorsToStrings(IEnumerable<Color> colorList)
	{
		if (colorList == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (Color color in colorList)
		{
			list.Add(ColorUtility.ToHtmlStringRGB(color));
		}
		return list;
	}
}
