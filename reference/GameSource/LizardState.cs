using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class LizardState : HealthState, DaddyLongLegs.IHaveTentacleHealthState
{
	public class RotType : ExtEnum<RotType>
	{
		public static RotType None = new RotType("None", register: true);

		public static RotType Slight = new RotType("Slight", register: true);

		public static RotType Opossum = new RotType("Opossum", register: true);

		public static RotType Full = new RotType("Full", register: true);

		public RotType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public float throatHealth;

	public float[] limbHealth;

	public RotType rotType;

	public float[] tentacleHealth;

	public float[] TentacleHealth => tentacleHealth;

	public LizardState(AbstractCreature creature)
		: base(creature)
	{
		throatHealth = 1f;
		if (ModManager.DLCShared && creature.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.SpitLizard)
		{
			limbHealth = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };
		}
		else
		{
			limbHealth = new float[4] { 1f, 1f, 1f, 1f };
		}
		rotType = RotType.None;
	}

	public void SetRotType(RotType type)
	{
		rotType = type;
		if (tentacleHealth == null && rotType != RotType.Slight)
		{
			tentacleHealth = new float[10];
			for (int i = 0; i < tentacleHealth.Length; i++)
			{
				tentacleHealth[i] = 1f;
			}
		}
		if (type != RotType.None)
		{
			creature.tentacleImmune = true;
		}
	}

	public override string ToString()
	{
		string text = HealthBaseSaveString();
		if (throatHealth < 1f)
		{
			text += string.Format(CultureInfo.InvariantCulture, "<cB>ThroatHealth<cC>{0}", throatHealth);
		}
		bool flag = false;
		for (int i = 0; i < limbHealth.Length; i++)
		{
			if (limbHealth[i] < 1f)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			text += string.Format(CultureInfo.InvariantCulture, "<cB>LimbHealth<cC>{0}", string.Join("<cC>", limbHealth));
		}
		if (rotType != RotType.None)
		{
			text += string.Format(CultureInfo.InvariantCulture, "<cB>RotType<cC>{0}", rotType.Index);
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
		bool flag = false;
		for (int i = 0; i < s.Length; i++)
		{
			string[] array = Regex.Split(s[i], "<cC>");
			switch (array[0])
			{
			case "ThroatHealth":
				throatHealth = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "LimbHealth":
			{
				for (int j = 0; j < array.Length - 1; j++)
				{
					limbHealth[j] = float.Parse(array[j + 1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				break;
			}
			case "RotType":
				flag = true;
				SetRotType(new RotType(ExtEnum<RotType>.values.entries[Mathf.Min(int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), ExtEnum<RotType>.values.entries.Count)]));
				break;
			}
		}
		unrecognizedSaveStrings.Remove("ThroatHealth");
		unrecognizedSaveStrings.Remove("LimbHealth");
		if (flag)
		{
			unrecognizedSaveStrings.Remove("RotType");
		}
	}

	public override void CycleTick()
	{
		throatHealth = Mathf.Min(throatHealth + 0.25f, 1f);
		for (int i = 0; i < limbHealth.Length; i++)
		{
			limbHealth[i] = Mathf.Min(limbHealth[i] + 0.5f, 1f);
		}
		base.CycleTick();
	}
}
