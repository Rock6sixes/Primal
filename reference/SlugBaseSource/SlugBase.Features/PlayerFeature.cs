using System;

namespace SlugBase.Features;

/// <summary>
/// A constant setting of a <see cref="T:SlugBase.SlugBaseCharacter" />'s player.
/// </summary>
/// <typeparam name="T">The type that stores this setting's information</typeparam>
public class PlayerFeature<T> : Feature<T>
{
	/// <summary>
	/// Creates a new <see cref="T:SlugBase.Features.PlayerFeature`1" /> with the given <paramref name="id" />.
	/// </summary>
	/// <param name="id">The JSON key.</param>
	/// <param name="factory">A delegate that parses <see cref="T:SlugBase.JsonAny" /> into <typeparamref name="T" />. An exception should be thrown on failure.</param>
	public PlayerFeature(string id, Func<JsonAny, T> factory)
		: base(id, factory)
	{
	}

	/// <summary>
	/// Gets the <typeparamref name="T" /> instance assocated with <paramref name="player" />.
	/// </summary>
	/// <param name="player">A <see cref="T:Player" /> instance that may be a <see cref="T:SlugBase.SlugBaseCharacter" /> with this <see cref="T:SlugBase.Features.Feature" />.</param>
	/// <param name="value">The stored setting, or <typeparamref name="T" />'s default value if the feature wasn't found.</param>
	/// <returns><c>true</c> if the <paramref name="player" />'s <see cref="T:SlugBase.SlugBaseCharacter" /> had this feature, <c>false</c> otherwise.</returns>
	public bool TryGet(Player player, out T value)
	{
		return base.TryGet(SlugBaseCharacter.Get(player.SlugCatClass), out value);
	}
}
