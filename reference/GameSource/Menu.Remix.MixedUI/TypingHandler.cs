using System.Collections.Generic;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class TypingHandler : MonoBehaviour
{
	public string _lastInput = "";

	public HashSet<ICanBeTyped> _assigned = new HashSet<ICanBeTyped>();

	public ICanBeTyped _focused;

	public void Update()
	{
		if (_assigned.Count < 1)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (_focused != null && !(_focused as UIfocusable).held)
		{
			_focused = null;
		}
		if (_focused == null)
		{
			foreach (ICanBeTyped item in _assigned)
			{
				if (item is UIfocusable && (item as UIfocusable).Focused)
				{
					_focused = item;
					break;
				}
			}
			if (_focused == null)
			{
				_lastInput = "";
				return;
			}
		}
		string inputString = Input.inputString;
		HashSet<char> hashSet = new HashSet<char>();
		for (int i = 0; i < _lastInput.Length; i++)
		{
			hashSet.Add(_lastInput[i]);
		}
		Queue<char> queue = new Queue<char>();
		for (int j = 0; j < inputString.Length; j++)
		{
			if (!hashSet.Contains(inputString[j]))
			{
				queue.Enqueue(inputString[j]);
			}
		}
		while (queue.Count > 0)
		{
			_focused.OnKeyDown(queue.Dequeue());
		}
		_lastInput = inputString;
	}

	public void OnDestroy()
	{
		_assigned.Clear();
		CanBeTypedExt._HandlerOnDestroy();
	}

	public void Assign(ICanBeTyped typable)
	{
		_assigned.Add(typable);
	}

	public bool IsAssigned(ICanBeTyped typable)
	{
		return _assigned.Contains(typable);
	}

	public void Unassign(ICanBeTyped typable)
	{
		_assigned.Remove(typable);
	}
}
