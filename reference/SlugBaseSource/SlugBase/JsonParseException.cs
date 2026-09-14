using System;

namespace SlugBase;

/// <summary>
/// Represents errors that occur when parsing JSON data.
/// </summary>
public class JsonParseException : Exception
{
	/// <summary>
	/// The offset in the input string that the error occurred at.
	/// </summary>
	public int CharIndex { get; private set; }

	/// <summary>
	/// The line in the input string that the error occurred at.
	/// </summary>
	public int Line { get; private set; }

	internal JsonParseException(string text, int position)
	{
		FindLine(text, position);
	}

	internal JsonParseException(string text, int position, string message)
		: base(message)
	{
		FindLine(text, position);
	}

	internal JsonParseException(string text, int position, string message, Exception inner)
		: base(message, inner)
	{
		FindLine(text, position);
	}

	private void FindLine(string text, int position)
	{
		int num = 0;
		for (int i = 0; i < text.Length && i < position; i++)
		{
			if (text[i] == '\r' || text[i] == '\n')
			{
				num++;
				if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
				{
					i++;
				}
			}
		}
		CharIndex = position;
		Line = num;
	}
}
