using System.Collections.Generic;

namespace SlugBase.DataTypes;

/// <summary>
/// Represents the nourishment and edibility of foods for a <see cref="T:SlugBase.SlugBaseCharacter" />.
/// </summary>
public class Diet
{
	/// <summary>
	/// The food value multiplier of dead creatures.
	/// <para>If the creature is consumed in its entirety, such as <see cref="F:CreatureTemplate.Type.Fly" />, <see cref="P:SlugBase.DataTypes.Diet.Meat" /> is used instead.</para>
	/// </summary>
	public float Corpses { get; set; }

	/// <summary>
	/// The food value multiplier of small creatures or living objects like <see cref="F:AbstractPhysicalObject.AbstractObjectType.JellyFish" />.
	/// </summary>
	public float Meat { get; set; }

	/// <summary>
	/// The food value multiplier of non-living foods.
	/// </summary>
	public float Plants { get; set; }

	/// <summary>
	/// The food value multipliers of individual object types.
	/// </summary>
	public Dictionary<AbstractObjectType, float> ObjectOverrides { get; } = new Dictionary<AbstractObjectType, float>();


	/// <summary>
	/// The food value multipliers of individual creature types.
	/// </summary>
	public Dictionary<Type, float> CreatureOverrides { get; } = new Dictionary<Type, float>();


	/// <summary>
	/// Creates a new <see cref="T:SlugBase.DataTypes.Diet" /> from JSON.
	/// </summary>
	/// <param name="json">The JSON to load.</param>
	public Diet(JsonAny json)
	{
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Expected O, but got Unknown
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Expected O, but got Unknown
		Name val = null;
		float? num = null;
		float? num2 = null;
		float? num3 = null;
		string text = json.TryString();
		if (text != null)
		{
			val = Utils.GetName(text);
			if ((ExtEnum<Name>)(object)val != (ExtEnum<Name>)null && !SetBase(val))
			{
				throw new JsonException("Couldn't find character \"" + ((ExtEnumBase)val).value + "\" to inherit diet from!", json);
			}
		}
		else
		{
			JsonObject jsonObject = json.AsObject();
			string text2 = jsonObject.TryGet("base")?.AsString();
			if (text2 != null)
			{
				val = Utils.GetName(text2);
			}
			if ((ExtEnum<Name>)(object)val != (ExtEnum<Name>)null && !SetBase(val))
			{
				throw new JsonException("Couldn't find character \"" + ((ExtEnumBase)val).value + "\" to inherit diet from!", json);
			}
			float? num4 = num;
			if (!num4.HasValue)
			{
				num = jsonObject.TryGet("corpses")?.AsFloat();
			}
			num4 = num2;
			if (!num4.HasValue)
			{
				num2 = jsonObject.TryGet("meat")?.AsFloat();
			}
			num4 = num3;
			if (!num4.HasValue)
			{
				num3 = jsonObject.TryGet("plants")?.AsFloat();
			}
			JsonObject? jsonObject2 = jsonObject.TryGet("overrides")?.AsObject();
			if (jsonObject2.HasValue)
			{
				JsonObject valueOrDefault = jsonObject2.GetValueOrDefault();
				if (true)
				{
					foreach (KeyValuePair<string, JsonAny> item in valueOrDefault)
					{
						ObjectOverrides[new AbstractObjectType(item.Key, false)] = item.Value.AsFloat();
						CreatureOverrides[new Type(item.Key, false)] = item.Value.AsFloat();
					}
				}
			}
		}
		if ((ExtEnum<Name>)(object)val == (ExtEnum<Name>)null)
		{
			if (!num.HasValue)
			{
				throw new JsonException("Missing \"corpses\" or \"base\" diet property!", json);
			}
			if (!num2.HasValue)
			{
				throw new JsonException("Missing \"meat\" or \"base\" diet property!", json);
			}
			if (!num3.HasValue)
			{
				throw new JsonException("Missing \"plants\" or \"base\" diet property!", json);
			}
		}
		if (num.HasValue)
		{
			Corpses = num.Value;
		}
		if (num2.HasValue)
		{
			Meat = num2.Value;
		}
		if (num3.HasValue)
		{
			Plants = num3.Value;
		}
	}

	/// <summary>
	/// Gets the food value multiplier for an object that was consumed in its entirety.
	/// </summary>
	/// <param name="obj">The object that was eaten.</param>
	/// <returns>A multiplier for the nourishment of this object.</returns>
	public float GetFoodMultiplier(PhysicalObject obj)
	{
		Creature val = (Creature)(object)((obj is Creature) ? obj : null);
		if (val != null && CreatureOverrides.TryGetValue(val.Template.type, out var value))
		{
			return value;
		}
		if (ObjectOverrides.TryGetValue(obj.abstractPhysicalObject.type, out value))
		{
			return value;
		}
		return IsObjectMeat(obj.abstractPhysicalObject.type) ? Meat : Plants;
	}

	/// <summary>
	/// Gets the food value multiplier for a creature
	/// </summary>
	/// <param name="player">The player to pass to <see cref="M:Player.EatMeatOmnivoreGreenList(Creature)" />.</param>
	/// <param name="crit">The creature that is being eaten.</param>
	/// <returns>A multiplier for the nourishment of this creature.</returns>
	public float GetMeatMultiplier(Player player, Creature crit)
	{
		if (CreatureOverrides.TryGetValue(crit.Template.type, out var value))
		{
			return value;
		}
		return (player != null && player.EatMeatOmnivoreGreenList(crit)) ? Meat : Corpses;
	}

	private static bool IsObjectMeat(AbstractObjectType type)
	{
		return (ExtEnum<AbstractObjectType>)(object)type == (ExtEnum<AbstractObjectType>)(object)AbstractObjectType.Creature || (ExtEnum<AbstractObjectType>)(object)type == (ExtEnum<AbstractObjectType>)(object)AbstractObjectType.EggBugEgg || (ExtEnum<AbstractObjectType>)(object)type == (ExtEnum<AbstractObjectType>)(object)AbstractObjectType.JellyFish;
	}

	private bool SetBase(Name name)
	{
		if ((ExtEnum<Name>)(object)name == (ExtEnum<Name>)null)
		{
			return false;
		}
		switch (((ExtEnumBase)name).value)
		{
		case "White":
		case "Yellow":
		case "Rivulet":
			Corpses = -1f;
			Meat = 1f;
			Plants = 1f;
			return true;
		case "Red":
		case "Artificer":
			Corpses = 1f;
			Meat = 1f;
			Plants = 0.25f;
			CreatureOverrides[Type.Fly] = 0.25f;
			return true;
		case "Gourmand":
		case "Inv":
			Corpses = 0.5f;
			Meat = 1f;
			Plants = 1f;
			return true;
		case "Spear":
			Corpses = -1f;
			Meat = -1f;
			Plants = -1f;
			return true;
		case "Saint":
			Corpses = -1f;
			Meat = -1f;
			Plants = 1f;
			return true;
		default:
			return false;
		}
	}
}
