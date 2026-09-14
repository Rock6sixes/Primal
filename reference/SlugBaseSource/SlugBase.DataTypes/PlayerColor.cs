using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SlugBase.Features;
using UnityEngine;

namespace SlugBase.DataTypes;

/// <summary>
/// Represents a color that may copy from <see cref="F:SlugBase.Features.PlayerFeatures.CustomColors" />.
/// </summary>
public readonly struct PlayerColor
{
	/// <summary>
	/// The body color of <see cref="T:SlugBase.SlugBaseCharacter" />s.
	/// </summary>
	public static readonly PlayerColor Body = new PlayerColor("Body");

	/// <summary>
	/// The eye color of <see cref="T:SlugBase.SlugBaseCharacter" />s.
	/// </summary>
	public static readonly PlayerColor Eyes = new PlayerColor("Eyes");

	private static readonly Regex _hex = new Regex("^[0-9a-fA-F]+$");

	private readonly string _name;

	private readonly Color _color;

	/// <summary>
	/// Creates a new <see cref="T:SlugBase.DataTypes.PlayerColor" /> from JSON.
	/// </summary>
	/// <param name="json">The JSON to load.</param>
	public PlayerColor(JsonAny json)
	{
		string text = json.AsString();
		if (text != null && !_hex.IsMatch(text))
		{
			_name = text;
			_color = default(Color);
		}
		else
		{
			_name = null;
			_color = JsonUtils.ToColor(json);
		}
	}

	/// <summary>
	/// Creates a new <see cref="T:SlugBase.DataTypes.PlayerColor" />.
	/// </summary>
	/// <param name="name">The name of the custom color to copy.</param>
	public PlayerColor(string name)
	{
		_name = name;
		_color = default(Color);
	}

	/// <summary>
	/// Creates a new <see cref="T:SlugBase.DataTypes.PlayerColor" />.
	/// </summary>
	/// <param name="color">The color to return.</param>
	public PlayerColor(Color color)
	{
		_name = null;
		_color = color;
	}

	/// <summary>
	/// Gets the value of this color for a player.
	/// </summary>
	/// <param name="playerGraphics">The player to get the color from.</param>
	/// <returns>The custom color, or <c>null</c> if it was not overridden via <see cref="F:SlugBase.Features.PlayerFeatures.CustomColors" />.</returns>
	public Color? GetColor(PlayerGraphics playerGraphics)
	{
		if (_name == null)
		{
			return _color;
		}
		if (PlayerFeatures.CustomColors.TryGet(playerGraphics.player, out var value))
		{
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i].Name == _name)
				{
					return value[i].GetColor(playerGraphics);
				}
			}
			return null;
		}
		return null;
	}

	/// <summary>
	/// Gets a color from the "custom_colors" feature by name for the given SlugBase character graphics.
	/// </summary>
	/// <param name="playerGraphics">The player graphics to get the color of.</param>
	/// <param name="name">The name of the custom color.</param>
	/// <exception cref="T:System.ArgumentException"><paramref name="playerGraphics" /> is not a <see cref="T:SlugBase.SlugBaseCharacter" /> instance, or does not have the "custom_colors" feature.</exception>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">No custom color was found matching <paramref name="name" />.</exception>
	public static Color GetCustomColor(PlayerGraphics playerGraphics, string name)
	{
		if (!SlugBaseCharacter.TryGet(playerGraphics.player.SlugCatClass, out var character))
		{
			throw new ArgumentException("Player was not added by SlugBase!", "playerGraphics");
		}
		if (!((Feature<ColorSlot[]>)PlayerFeatures.CustomColors).TryGet(character, out ColorSlot[] _))
		{
			throw new ArgumentException("Player does not have the \"custom_colors\" feature!");
		}
		Color? color = new PlayerColor(name).GetColor(playerGraphics);
		if (color.HasValue)
		{
			Color valueOrDefault = color.GetValueOrDefault();
			if (true)
			{
				return valueOrDefault;
			}
		}
		throw new KeyNotFoundException("Could not find custom color: \"" + name + "\"!");
	}

	/// <summary>
	/// Gets a color from the "custom_colors" feature by index for the given SlugBase character graphics.
	/// </summary>
	/// <param name="playerGraphics">The player graphics to get the color of.</param>
	/// <param name="index">The index of the custom color.</param>
	/// <exception cref="T:System.ArgumentException"><paramref name="playerGraphics" /> is not a <see cref="T:SlugBase.SlugBaseCharacter" /> instance, or does not have the "custom_colors" feature.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException"><paramref name="index" /> is out of range of the "custom_colors" list.</exception>
	public static Color GetCustomColor(PlayerGraphics playerGraphics, int index)
	{
		if (!SlugBaseCharacter.TryGet(playerGraphics.player.SlugCatClass, out var character))
		{
			throw new ArgumentException("Player was not added by SlugBase!", "playerGraphics");
		}
		if (!((Feature<ColorSlot[]>)PlayerFeatures.CustomColors).TryGet(character, out ColorSlot[] value))
		{
			throw new ArgumentException("Player does not have the \"custom_colors\" feature!");
		}
		if (index < 0 || index >= value.Length)
		{
			throw new IndexOutOfRangeException($"Could not find custom color with index {index}!");
		}
		return value[index].GetColor(playerGraphics);
	}
}
