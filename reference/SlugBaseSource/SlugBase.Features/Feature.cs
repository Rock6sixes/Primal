using System;

namespace SlugBase.Features;

/// <summary>
/// A strongly-typed constant setting of a <see cref="T:SlugBase.SlugBaseCharacter" />.
/// </summary>
/// <remarks>
/// <see cref="T:SlugBase.Features.PlayerFeature`1" /> and <see cref="T:SlugBase.Features.GameFeature`1" /> should be used when possible.
/// Otherwise, consider making a child class with a <c>TryGet</c> method that finds the most
/// appropriate <see cref="T:SlugBase.SlugBaseCharacter" /> to pass to <see cref="M:SlugBase.Features.Feature`1.TryGet(SlugBase.SlugBaseCharacter,`0@)" />.
/// </remarks>
/// <typeparam name="T">The type that stores this setting's information.</typeparam>
public class Feature<T> : Feature
{
	private readonly Func<JsonAny, T> _factory;

	/// <summary>
	/// Creates a new <see cref="T:SlugBase.Features.Feature`1" /> with the given <paramref name="id" />.
	/// </summary>
	/// <param name="id">The JSON key.</param>
	/// <param name="factory">A delegate that parses <see cref="T:SlugBase.JsonAny" /> into <typeparamref name="T" />. An exception should be thrown on failure.</param>
	/// <exception cref="T:System.ArgumentNullException"><paramref name="factory" /> is <c>null</c>.</exception>
	/// <exception cref="T:System.ArgumentNullException"><paramref name="id" /> is <c>null</c>.</exception>
	/// <exception cref="T:System.ArgumentException">A <see cref="T:SlugBase.Features.Feature" /> with the given <paramref name="id" /> already exists.</exception>
	public Feature(string id, Func<JsonAny, T> factory)
		: base(id)
	{
		_factory = factory;
	}

	/// <summary>
	/// Gets the <typeparamref name="T" /> instance assocated with <paramref name="character" />.
	/// </summary>
	/// <param name="character">The <see cref="T:SlugBase.SlugBaseCharacter" /> that may have this <see cref="T:SlugBase.Features.Feature" />.</param>
	/// <param name="value">The stored setting, or <typeparamref name="T" />'s default value if the feature wasn't found.</param>
	/// <returns><c>true</c> if <paramref name="character" /> had this feature, <c>false</c> otherwise.</returns>
	public bool TryGet(SlugBaseCharacter character, out T value)
	{
		value = default(T);
		return character?.Features.TryGet(this, out value) ?? false;
	}

	internal override object Create(JsonAny json)
	{
		return _factory(json);
	}
}
/// <summary>
/// Represents a constant setting of a <see cref="T:SlugBase.SlugBaseCharacter" />.
/// </summary>
public abstract class Feature
{
	/// <summary>
	/// This <see cref="T:SlugBase.Features.Feature" />'s JSON key.
	/// </summary>
	public string ID { get; }

	/// <summary>
	/// Creates a new <see cref="T:SlugBase.Features.Feature" /> with the given <paramref name="id" />.
	/// </summary>
	/// <param name="id">The JSON key.</param>
	/// <exception cref="T:System.ArgumentNullException"><paramref name="id" /> is <c>null</c>.</exception>
	/// <exception cref="T:System.ArgumentException">A <see cref="T:SlugBase.Features.Feature" /> with the given <paramref name="id" /> already exists.</exception>
	public Feature(string id)
	{
		ID = id;
		if (id == null)
		{
			throw new ArgumentNullException();
		}
		FeatureManager.Register(this);
	}

	internal abstract object Create(JsonAny json);
}
