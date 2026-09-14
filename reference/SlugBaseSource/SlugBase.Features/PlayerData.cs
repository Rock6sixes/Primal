using System.Runtime.CompilerServices;

namespace SlugBase.Features;

/// <summary>
/// Stores per-player variables.
/// </summary>
/// <typeparam name="TValue">The type of data stored.</typeparam>
public class PlayerData<TValue> : Data<PlayerState, TValue> where TValue : new()
{
	/// <summary>
	/// Creates a new per-player variable that depends on <paramref name="requiredFeature" />.
	/// </summary>
	/// <param name="requiredFeature">The required <see cref="T:SlugBase.Features.Feature" />, or <c>null</c> if data access should not be locked behind a feature.</param>
	public PlayerData(Feature requiredFeature)
		: base(requiredFeature)
	{
	}

	/// <summary>
	/// Gets the <typeparamref name="TValue" /> instance assocated with <paramref name="state" />, constructing it if it does not exist.
	/// <para>If the game's <see cref="T:SlugBase.SlugBaseCharacter" /> does not have <see cref="P:SlugBase.Features.Data.RequiredFeature" />, then <c>null</c> is returned.</para>
	/// </summary>
	/// <param name="state">The player state the variable is associated with.</param>
	public StrongBox<TValue> Get(PlayerState state)
	{
		PhysicalObject realizedObject = ((AbstractPhysicalObject)((CreatureState)state).creature).realizedObject;
		return Get((Player)(object)((realizedObject is Player) ? realizedObject : null));
	}

	/// <summary>
	/// Gets the <typeparamref name="TValue" /> instance assocated with <paramref name="player" />, constructing it if it does not exist.
	/// <para>If the game's <see cref="T:SlugBase.SlugBaseCharacter" /> does not have <see cref="P:SlugBase.Features.Data.RequiredFeature" />, then <c>null</c> is returned.</para>
	/// </summary>
	/// <param name="player">The player the variable is associated with.</param>
	public StrongBox<TValue> Get(Player player)
	{
		return Get(SlugBaseCharacter.Get(player.SlugCatClass), player.playerState);
	}

	/// <summary>
	/// Gets the <typeparamref name="TValue" /> instance assocated with <paramref name="state" />, constructing it if it does not exist.
	/// </summary>
	/// <param name="state">The player state the variable is associated with.</param>
	/// <param name="value">The stored value, or <typeparamref name="TValue" />'s default value if the required feature wasn't found.</param>
	/// <returns><c>true</c> if the player's <see cref="T:SlugBase.SlugBaseCharacter" /> had <see cref="P:SlugBase.Features.Data.RequiredFeature" />, <c>false</c> otherwise.</returns>
	public bool TryGet(PlayerState state, out TValue value)
	{
		return Data.TryUnbox(Get(state), out value);
	}

	/// <summary>
	/// Gets the <typeparamref name="TValue" /> instance assocated with <paramref name="player" />, constructing it if it does not exist.
	/// </summary>
	/// <param name="player">The player the variable is associated with.</param>
	/// <param name="value">The stored value, or <typeparamref name="TValue" />'s default value if the required feature wasn't found.</param>
	/// <returns><c>true</c> if the player's <see cref="T:SlugBase.SlugBaseCharacter" /> had <see cref="P:SlugBase.Features.Data.RequiredFeature" />, <c>false</c> otherwise.</returns>
	public bool TryGet(Player player, out TValue value)
	{
		return Data.TryUnbox(Get(player), out value);
	}
}
