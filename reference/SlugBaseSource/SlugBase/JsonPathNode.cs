using System.Collections.Generic;

namespace SlugBase;

internal class JsonPathNode
{
	private readonly string _text;

	private readonly JsonPathNode _prev;

	public JsonPathNode(string text, JsonPathNode prev)
	{
		_text = text;
		_prev = prev;
	}

	public override string ToString()
	{
		List<string> list = new List<string>();
		JsonPathNode jsonPathNode = this;
		while (jsonPathNode != null && list.Count < 100)
		{
			list.Add(jsonPathNode._text);
			jsonPathNode = jsonPathNode._prev;
		}
		list.Reverse();
		return string.Concat(list);
	}
}
