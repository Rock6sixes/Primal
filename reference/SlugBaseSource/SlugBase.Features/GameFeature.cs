using System;

namespace SlugBase.Features;

/// <summary>
/// A constant setting of a <see cref="T:SlugBase.SlugBaseCharacter" />'s save slot.
/// </summary>
/// <typeparam name="T">The type that stores this setting's information.</typeparam>
public class GameFeature<T> : Feature<T>
{
	/// <summary>
	/// Creates a new <see cref="T:SlugBase.Features.GameFeature`1" /> with the given <paramref name="id" />.
	/// </summary>
	/// <param name="id">The JSON key.</param>
	/// <param name="factory">A delegate that parses <see cref="T:SlugBase.JsonAny" /> into <typeparamref name="T" />. An exception should be thrown on failure.</param>
	public GameFeature(string id, Func<JsonAny, T> factory)
		: base(id, factory)
	{
	}

	/// <summary>
	/// Gets the <typeparamref name="T" /> instance assocated with <paramref name="game" />.
	/// </summary>
	/// <param name="game">A <see cref="T:RainWorldGame" /> instance that may belong to a <see cref="T:SlugBase.SlugBaseCharacter" /> with this <see cref="T:SlugBase.Features.Feature" />.</param>
	/// <param name="value">The stored setting, or <typeparamref name="T" />'s default value if the feature wasn't found.</param>
	/// <returns><c>true</c> if the <paramref name="game" />'s <see cref="T:SlugBase.SlugBaseCharacter" /> had this feature, <c>false</c> otherwise.</returns>
	public bool TryGet(RainWorldGame game, out T value)
	{
		return base.TryGet(SlugBaseCharacter.Get(game.StoryCharacter), out value);
	}
}
