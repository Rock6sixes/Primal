using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Menu;
using Newtonsoft.Json;
using UnityEngine;

namespace SlugBase.SaveData;

/// <summary>
/// Holds all the slugcat associated data that is needed outside of loaded savestate
/// </summary>
internal class MinedSaveData
{
	/// <summary>
	/// Registry of all additional data for each savegamedata
	/// </summary>
	internal static readonly ConditionalWeakTable<SaveGameData, MinedSaveData> Data = new ConditionalWeakTable<SaveGameData, MinedSaveData>();

	public SceneID SelectMenuScene;

	public MinedSaveData(RainWorld rainWorld, Name slugcat)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		string[] progLinesFromMemory = rainWorld.progression.GetProgLinesFromMemory();
		for (int i = 0; i < progLinesFromMemory.Length; i++)
		{
			string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
			if (array.Length == 2 && array[0] == "SAVE STATE" && (ExtEnum<Name>)(object)BackwardsCompatibilityRemix.ParseSaveNumber(array[1]) == (ExtEnum<Name>)(object)slugcat)
			{
				string text = Mine<string>(array[1], ">SELECTSCENE_SlugBaseInternal_", "<");
				if (text != null)
				{
					SelectMenuScene = new SceneID(text, false);
				}
				break;
			}
		}
	}

	/// <summary>
	/// Finds a base64 encoded json string inside saveline and tries to unpack it into an object
	/// </summary>
	/// <typeparam name="T">Type of object that is being looked for</typeparam>
	/// <param name="saveLine">Line to search object in</param>
	/// <param name="startMarker">Left delimiter of saved value</param>
	/// <param name="endMarker">Right delimiter of saved value</param>
	/// <param name="defaultValue">What to return if the process failed</param>
	/// <returns>instance of T saved inside string</returns>
	private static T Mine<T>(string saveLine, string startMarker, string endMarker, T defaultValue = default(T))
	{
		int num = saveLine.IndexOf(startMarker);
		if (num == -1)
		{
			return defaultValue;
		}
		num += startMarker.Length;
		int num2 = saveLine.IndexOf(endMarker, num);
		try
		{
			string @string = Encoding.UTF8.GetString(Convert.FromBase64String(saveLine.Substring(num, num2 - num)));
			return JsonConvert.DeserializeObject<T>(@string);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			SlugBasePlugin.Logger.LogError("Failed to mine key \"" + startMarker + "\"");
		}
		return defaultValue;
	}
}
