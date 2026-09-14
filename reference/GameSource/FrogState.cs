using System;
using System.Globalization;
using System.Text.RegularExpressions;

public class FrogState : CreatureState
{
	public EntityID? creatureAttachedTo;

	public int feedSize;

	public bool bloodBank => feedSize >= 3;

	public FrogState(AbstractCreature creature)
		: base(creature)
	{
	}

	public override string ToString()
	{
		string text = base.ToString();
		if (!creatureAttachedTo.HasValue)
		{
			return text;
		}
		text += string.Format(CultureInfo.InvariantCulture, "CreatureAttachedTo<cC>{0}<cB>", creatureAttachedTo.Value.ToString());
		return text + string.Format(CultureInfo.InvariantCulture, "FeedSize<cC>{0}<cB>", feedSize);
	}

	public override void LoadFromString(string[] s)
	{
		base.LoadFromString(s);
		for (int i = 0; i < s.Length - 1; i++)
		{
			try
			{
				string[] array = Regex.Split(s[i], "<cC>");
				switch (array[0])
				{
				case "CreatureAttachedTo":
					creatureAttachedTo = EntityID.FromString(array[1]);
					break;
				case "FeedSize":
					feedSize = int.Parse(array[1], NumberStyles.Any, CultureInfo.CurrentCulture);
					break;
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
