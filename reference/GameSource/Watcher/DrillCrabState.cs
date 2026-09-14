using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Watcher;

public class DrillCrabState : HealthState
{
	public float[] legHealths;

	public float layer;

	public bool BehindPlayLayer => layer >= 1f / 3f;

	public DrillCrabState(AbstractCreature creature)
		: base(creature)
	{
		legHealths = new float[6];
		for (int i = 0; i < legHealths.Length; i++)
		{
			legHealths[i] = 1f;
		}
	}

	public override string ToString()
	{
		string text = HealthBaseSaveString();
		text += "<cB>Legs<cC>";
		for (int i = 0; i < legHealths.Length; i++)
		{
			if (i > 0)
			{
				text += ",";
			}
			text += base.health.ToString("G4", CultureInfo.InvariantCulture);
		}
		if (layer > 0f)
		{
			text = text + "<cB>Layer<cC>" + layer.ToString("G4", CultureInfo.InvariantCulture);
		}
		foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + "<cB>" + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value;
		}
		return text;
	}

	public override void LoadFromString(string[] s)
	{
		base.LoadFromString(s);
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "<cC>");
			if (array.Length < 2)
			{
				continue;
			}
			switch (array[0])
			{
			case "Legs":
			{
				string[] array2 = array[1].Split(',');
				for (int j = 0; j < legHealths.Length; j++)
				{
					if (j < array2.Length)
					{
						legHealths[j] = float.Parse(array2[j], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					else
					{
						legHealths[j] = 1f;
					}
				}
				break;
			}
			case "Layer":
				layer = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			}
		}
		unrecognizedSaveStrings.Remove("Legs");
	}

	public override void CycleTick()
	{
		base.CycleTick();
		for (int i = 0; i < legHealths.Length; i++)
		{
			if (legHealths[i] > 0f)
			{
				legHealths[i] = Mathf.Min(legHealths[i] + 0.25f, 1f);
			}
		}
	}
}
