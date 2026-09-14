using System.Globalization;
using System.Text.RegularExpressions;

namespace DevInterface;

public class GeyserData : PlacedObject.ResizableObjectData
{
	public float timing;

	public GeyserData(PlacedObject owner)
		: base(owner)
	{
		timing = 0.5f;
	}

	public override void FromString(string s)
	{
		base.FromString(s);
		string[] array = Regex.Split(s, "~");
		timing = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 3);
	}

	public override string ToString()
	{
		string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}", timing);
		baseString = SaveState.SetCustomData(this, baseString);
		return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
	}
}
