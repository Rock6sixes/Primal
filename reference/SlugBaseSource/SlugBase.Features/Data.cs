using System.Runtime.CompilerServices;

namespace SlugBase.Features;

/// <summary>
/// Stores <typeparamref name="TValue" />s associated with <typeparamref name="THolder" />s.
/// </summary>
/// <remarks>
/// <see cref="T:SlugBase.Features.PlayerData`1" /> and <see cref="T:SlugBase.Features.GameData`1" /> should be used when possible.
/// Otherwise, consider making a child class with <c>Get</c> and <c>TryGet</c> methods that find the most
/// appropriate <see cref="T:SlugBase.SlugBaseCharacter" /> to pass to <see cref="M:SlugBase.Features.Data`2.Get(SlugBase.SlugBaseCharacter,`0)" />.
/// </remarks>
/// <typeparam name="THolder">The key type that values are associated with.</typeparam>
/// <typeparam name="TValue">The type of data stored.</typeparam>
public class Data<THolder, TValue> : Data where THolder : class where TValue : new()
{
	private readonly ConditionalWeakTable<THolder, StrongBox<TValue>> _values = new ConditionalWeakTable<THolder, StrongBox<TValue>>();

	/// <summary>
	/// Creates a new instance of <see cref="T:SlugBase.Features.Data`2" /> that depends on <paramref name="requiredFeature" />.
	/// </summary>
	/// <param name="requiredFeature">The required <see cref="T:SlugBase.Features.Feature" />, or <c>null</c> if data access should not be locked behind a feature.</param>
	public Data(Feature requiredFeature)
		: base(requiredFeature)
	{
	}

	/// <summary>
	/// Gets the <typeparamref name="TValue" /> instance assocated with <paramref name="key" />, constructing it if it does not exist.
	/// <para>If <paramref name="character" /> does not have <see cref="P:SlugBase.Features.Data.RequiredFeature" />, then <c>null</c> is returned.</para>
	/// </summary>
	/// <param name="character">The <see cref="T:SlugBase.SlugBaseCharacter" /> that may own <see cref="P:SlugBase.Features.Data.RequiredFeature" />.</param>
	/// <param name="key">The key the data is attached to.</param>
	public StrongBox<TValue> Get(SlugBaseCharacter character, THolder key)
	{
		if (base.RequiredFeature != null && (character == null || !character.Features.Contains(base.RequiredFeature)))
		{
			return null;
		}
		if (!_values.TryGetValue(key, out var value))
		{
			_values.Add(key, value = new StrongBox<TValue>(new TValue()));
		}
		return value;
	}
}
/// <summary>
/// Represents variable information of a <see cref="T:SlugBase.SlugBaseCharacter" /> that depends on a <see cref="T:SlugBase.Features.Feature" />.
/// </summary>
public abstract class Data
{
	/// <summary>
	/// The feature this data depends upon.
	/// </summary>
	public Feature RequiredFeature { get; }

	/// <summary>
	/// Create a new <see cref="T:SlugBase.Features.Data" /> instance that requires a given feature.
	/// </summary>
	/// <param name="requiredFeature">The feature that this requires, or null to not require a feature.</param>
	public Data(Feature requiredFeature)
	{
		RequiredFeature = requiredFeature;
	}

	/// <summary>
	/// Gets the value of a <see cref="T:System.Runtime.CompilerServices.StrongBox`1" />.
	/// </summary>
	/// <typeparam name="T">The stored value's type.</typeparam>
	/// <param name="box">The <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> holding the value or <c>null</c>.</param>
	/// <param name="value">The stored value, or <c>default</c> if <paramref name="box" /> is <c>null</c>.</param>
	/// <returns><c>false</c> if <paramref name="box" /> was <c>null</c>, <c>true</c> otherwise.</returns>
	public static bool TryUnbox<T>(StrongBox<T> box, out T value)
	{
		if (box != null)
		{
			value = box.Value;
			return true;
		}
		value = default(T);
		return false;
	}
}
