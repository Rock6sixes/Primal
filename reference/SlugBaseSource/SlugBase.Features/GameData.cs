using System.Runtime.CompilerServices;

namespace SlugBase.Features;

/// <summary>
/// Stores per-game variables.
/// </summary>
/// <typeparam name="TValue">The type of data stored.</typeparam>
public class GameData<TValue> : Data<RainWorldGame, TValue> where TValue : new()
{
	/// <summary>
	/// Create a new per-game variable that depends on <paramref name="requiredFeature" />.
	/// </summary>
	/// <param name="requiredFeature">The required <see cref="T:SlugBase.Features.Feature" />, or <c>null</c> if data access should not be locked behind a feature.</param>
	public GameData(Feature requiredFeature)
		: base(requiredFeature)
	{
	}

	/// <summary>
	/// Gets the <typeparamref name="TValue" /> instance assocated with <paramref name="game" />, constructing it if it does not exist.
	/// <para>If the game's <see cref="T:SlugBase.SlugBaseCharacter" /> does not have <see cref="P:SlugBase.Features.Data.RequiredFeature" />, then <c>null</c> is returned.</para>
	/// </summary>
	/// <param name="game">The current game.</param>
	public StrongBox<TValue> Get(RainWorldGame game)
	{
		return Get(SlugBaseCharacter.Get(game.StoryCharacter), game);
	}

	/// <summary>
	/// Gets the <typeparamref name="TValue" /> instance assocated with <paramref name="game" />, constructing it if it does not exist.
	/// </summary>
	/// <param name="game">The current game.</param>
	/// <param name="value">The stored value, or <typeparamref name="TValue" />'s default value if the required feature wasn't found.</param>
	/// <returns><c>true</c> if <paramref name="game" /> had <see cref="P:SlugBase.Features.Data.RequiredFeature" />, <c>false</c> otherwise.</returns>
	public bool TryGet(RainWorldGame game, out TValue value)
	{
		return Data.TryUnbox(Get(game), out value);
	}
}
