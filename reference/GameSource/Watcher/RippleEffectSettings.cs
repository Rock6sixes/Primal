using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Watcher;

public class RippleEffectSettings : RoomSettings.RoomEffect
{
	public SlugcatStats.Timeline destTimeline;

	public string destRegion;

	public string _destRoom;

	public float trailAmount;

	public int[] screenMap;

	public int screens;

	public int rippleScreens;

	public int palette;

	public float paletteAmount;

	public string destRoom
	{
		get
		{
			return _destRoom;
		}
		set
		{
			_destRoom = value;
			rippleScreens = ((value != null) ? CountScreens(_destRoom) : 0);
			ResetScreenMap();
		}
	}

	public bool TryGetMappedScreen(int camPos, out int screen)
	{
		if (screenMap.Length == 0 || camPos < 0 || camPos >= screenMap.Length)
		{
			screen = 1;
		}
		else
		{
			screen = screenMap[Mathf.Min(camPos, screenMap.Length - 1)];
		}
		return screen != 0;
	}

	public RippleEffectSettings(Type type, float amount, string name)
		: base(type, amount, inherited: false)
	{
		base.type = type;
		base.amount = amount;
		screens = CountScreens(name);
		screenMap = new int[screens];
		palette = 0;
		paletteAmount = 0f;
	}

	public void ResetScreenMap()
	{
		for (int i = 0; i < screenMap.Length; i++)
		{
			screenMap[i] = ((i < rippleScreens) ? (i + 1) : 0);
		}
	}

	public int CountScreens(string name)
	{
		string text = WorldLoader.FindRoomFile(name, includeRootDirectory: false, ".txt");
		if (text != null)
		{
			return File.ReadAllLines(text).ElementAt(3).Split('|')
				.Count();
		}
		return 0;
	}

	public override string ToString()
	{
		return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}", type.ToString(), amount, panelPosition.x, panelPosition.y, (destTimeline != null) ? destTimeline.ToString() : "NULL", (destRegion != null) ? destRegion : "NULL", (destRoom != null) ? destRoom : "NULL", trailAmount, string.Join(".", screenMap), palette, paletteAmount), "-", unrecognizedAttributes);
	}

	public new void FromString(string[] s)
	{
		try
		{
			type = new Type(s[0]);
			amount = float.Parse(s[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPosition.x = float.Parse(s[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPosition.y = float.Parse(s[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			destTimeline = ((s[4] == "NULL") ? null : new SlugcatStats.Timeline(s[4]));
			destRegion = ((s[5] == "NULL") ? null : s[5]);
			destRoom = ((s[6] == "NULL") ? null : s[6]);
			trailAmount = ((s.Length > 7) ? float.Parse(s[7], NumberStyles.Any, CultureInfo.InvariantCulture) : 0f);
			screenMap = ((s.Length > 8) ? (from e in s[8].Split('.')
				select int.Parse(e, NumberStyles.Any, CultureInfo.InvariantCulture)).ToArray() : screenMap);
			palette = ((s.Length > 9) ? int.Parse(s[9], NumberStyles.Any, CultureInfo.InvariantCulture) : palette);
			paletteAmount = ((s.Length > 10) ? float.Parse(s[10], NumberStyles.Any, CultureInfo.InvariantCulture) : paletteAmount);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(s, 11);
		}
		catch
		{
		}
	}
}
