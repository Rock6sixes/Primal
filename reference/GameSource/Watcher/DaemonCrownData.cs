using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class DaemonCrownData : PlacedObject.Data
{
	public Vector2 handlePos;

	public Vector2 panelPos;

	public int depth;

	public float Rad => handlePos.magnitude;

	public DaemonCrownData(PlacedObject owner)
		: base(owner)
	{
		handlePos = new Vector2(0f, 100f);
		panelPos = Custom.DegToVec(30f) * 100f;
		depth = 0;
	}

	public string BaseSaveString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", handlePos.x, handlePos.y, panelPos.x, panelPos.y, depth);
	}

	public override string ToString()
	{
		string baseString = BaseSaveString();
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
	}

	public override void FromString(string s)
	{
		string[] array = Regex.Split(s, "~");
		handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
		handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
		depth = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
	}
}
