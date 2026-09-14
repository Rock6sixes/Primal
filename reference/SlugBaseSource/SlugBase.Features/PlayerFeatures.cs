using System.Collections.Generic;
using System.Linq;
using SlugBase.DataTypes;
using UnityEngine;

namespace SlugBase.Features;

/// <summary>
/// Built-in <see cref="T:SlugBase.Features.Feature" />s describing the player.
/// </summary>
public static class PlayerFeatures
{
	/// <summary>"color": Player body and UI color.</summary>
	public static readonly PlayerFeature<Color> SlugcatColor = FeatureTypes.PlayerColor("color");

	/// <summary>"auto_grab_batflies": Grab batflies on collision.</summary>
	public static readonly PlayerFeature<bool> AutoGrabFlies = FeatureTypes.PlayerBool("auto_grab_batflies");

	/// <summary>"weight": Weight multiplier.</summary>
	public static readonly PlayerFeature<float[]> WeightMul = FeatureTypes.PlayerFloats("weight", 1, 2);

	/// <summary>"tunnel_speed": Move speed in tunnels.</summary>
	public static readonly PlayerFeature<float[]> TunnelSpeedMul = FeatureTypes.PlayerFloats("tunnel_speed", 1, 2);

	/// <summary>"climb_speed": Move speed on poles.</summary>
	public static readonly PlayerFeature<float[]> ClimbSpeedMul = FeatureTypes.PlayerFloats("climb_speed", 1, 2);

	/// <summary>"walk_speed": Standing move speed.</summary>
	public static readonly PlayerFeature<float[]> WalkSpeedMul = FeatureTypes.PlayerFloats("walk_speed", 1, 2);

	/// <summary>"crouch_stealth": Visual stealth while crouched.</summary>
	public static readonly PlayerFeature<float[]> CrouchStealth = FeatureTypes.PlayerFloats("crouch_stealth", 1, 2);

	/// <summary>"throw_skill": Spear damage and speed.</summary>
	public static readonly PlayerFeature<int[]> ThrowSkill = FeatureTypes.PlayerInts("throw_skill", 1, 2);

	/// <summary>"lung_capacity": Time underwater before drowning.</summary>
	public static readonly PlayerFeature<float[]> LungsCapacityMul = FeatureTypes.PlayerFloats("lung_capacity", 1, 2);

	/// <summary>"loudness": Sound alert multiplier.</summary>
	public static readonly PlayerFeature<float[]> LoudnessMul = FeatureTypes.PlayerFloats("loudness", 1, 2);

	/// <summary>"back_spear": Store a spear on back.</summary>
	public static readonly PlayerFeature<bool> BackSpear = FeatureTypes.PlayerBool("back_spear");

	/// <summary>"alignments": Initial community reputation.</summary>
	public static readonly PlayerFeature<Dictionary<CommunityID, RepOverride>> CommunityAlignments = new PlayerFeature<Dictionary<CommunityID, RepOverride>>("alignments", delegate(JsonAny json)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		JsonObject jsonObject = json.AsObject();
		Dictionary<CommunityID, RepOverride> dictionary = new Dictionary<CommunityID, RepOverride>();
		foreach (KeyValuePair<string, JsonAny> item in jsonObject)
		{
			CommunityID key = new CommunityID(Utils.MatchCaseInsensitiveEnum<CommunityID>(item.Key), false);
			dictionary[key] = new RepOverride(item.Value);
		}
		return dictionary;
	});

	/// <summary>"diet": Edibility and nourishment of foods.</summary>
	public static readonly PlayerFeature<Diet> Diet = new PlayerFeature<Diet>("diet", (JsonAny json) => new Diet(json));

	/// <summary>"custom_colors": Configurable player colors.</summary>
	public static readonly PlayerFeature<ColorSlot[]> CustomColors = new PlayerFeature<ColorSlot[]>("custom_colors", delegate(JsonAny json)
	{
		JsonList jsonList2 = json.AsList();
		ColorSlot[] array = new ColorSlot[jsonList2.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new ColorSlot(i, jsonList2[i]);
		}
		if (array.Length < 1 || array[0].Name != "Body")
		{
			throw new JsonException("Expected \"Body\" as first custom color!", jsonList2);
		}
		if (array.Length < 2 || array[1].Name != "Eyes")
		{
			throw new JsonException("Expected \"Eyes\" as second custom color!", jsonList2);
		}
		return array;
	});

	/// <summary>"can_maul": Ability to maul creatures.</summary>
	public static readonly PlayerFeature<bool> CanMaul = FeatureTypes.PlayerBool("can_maul");

	/// <summary>"maul_damage": Damage of maul attack.</summary>
	public static readonly PlayerFeature<float> MaulDamage = FeatureTypes.PlayerFloat("maul_damage");

	/// <summary>"maul_blacklist": Creatures that cannot be mauled.</summary>
	public static readonly PlayerFeature<Type[]> MaulBlacklist = new PlayerFeature<Type[]>("maul_blacklist", delegate(JsonAny json)
	{
		JsonList jsonList = json.AsList();
		return jsonList.Select(JsonUtils.ToExtEnum<Type>).ToArray();
	});
}
