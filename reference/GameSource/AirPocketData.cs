using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class AirPocketData : PlacedObject.ResizableObjectData
{
	public Vector2 panelPos = new Vector2(30f, 30f);

	public bool flood = true;

	public float waterLevel = 80f;

	public AirPocketData(PlacedObject owner)
		: base(owner)
	{
		handlePos = new Vector2(100f, 200f);
	}

	public override void FromString(string s)
	{
		base.FromString(s);
		string[] array = Regex.Split(s, "~");
		panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
		flood = array[4] == "Y";
		waterLevel = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
	}

	public override string ToString()
	{
		string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}", panelPos.x, panelPos.y, flood ? "Y" : "N", waterLevel);
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
	}
}
