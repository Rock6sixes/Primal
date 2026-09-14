using System;

namespace SlugBase;

/// <summary>
/// Represents errors that occur when accessing JSON data.
/// </summary>
public class JsonException : Exception
{
	/// <summary>
	/// The path to the element that failed to parse, starting from the root object.
	/// </summary>
	public string JsonPath { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:SlugBase.JsonException" /> class with a path to the invalid element.</summary>
	public JsonException(JsonAny json)
	{
		JsonPath = json._path?.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:SlugBase.JsonException" /> class with a specified error message
	/// and a path to the invalid element.</summary>
	public JsonException(string message, JsonAny json)
		: base(message)
	{
		JsonPath = json._path?.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:SlugBase.JsonException" /> class with a specified error message,
	/// a reference to the inner exception that is the cause of this exception, and a path to the invalid element.</summary>
	public JsonException(string message, Exception inner, JsonAny json)
		: base(message, inner)
	{
		JsonPath = json._path?.ToString();
	}

	internal JsonException(JsonPathNode path)
	{
		JsonPath = path?.ToString();
	}

	internal JsonException(string message, JsonPathNode path)
		: base(message)
	{
		JsonPath = path?.ToString();
	}

	internal JsonException(string message, Exception inner, JsonPathNode path)
		: base(message, inner)
	{
		JsonPath = path?.ToString();
	}
}
