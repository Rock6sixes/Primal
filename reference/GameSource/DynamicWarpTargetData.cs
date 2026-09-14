using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class DynamicWarpTargetData : PlacedObject.Data
{
	public Vector2 panelPos;

	public float rippleLevelRequirement;

	public bool deadEnd;

	public bool badWarp;

	public const float specialWarpValue = 999f;

	public bool specialWarp
	{
		get
		{
			return rippleLevelRequirement == 999f;
		}
		set
		{
			rippleLevelRequirement = 999f;
		}
	}

	public DynamicWarpTargetData(PlacedObject owner)
		: base(owner)
	{
	}

	public override void FromString(string s)
	{
		string[] array = Regex.Split(s, "~");
		int num = array.Length;
		panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
		panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		rippleLevelRequirement = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		deadEnd = ((num <= 3) ? deadEnd : (array[3] == "true"));
		badWarp = ((num <= 4) ? badWarp : (array[4] == "true"));
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
	}

	public override string ToString()
	{
		string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", panelPos.x, panelPos.y, rippleLevelRequirement, deadEnd ? "true" : "false", badWarp ? "true" : "false");
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
	}
}
