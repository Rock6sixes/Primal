using System;
using System.Collections.Generic;

namespace RWCustom;

public class MinHeap<TKey, TValue> where TKey : IComparable<TKey>
{
	public List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>();

	public int Count => list.Count;

	public void Add(TKey key, TValue value)
	{
		list.Add(new KeyValuePair<TKey, TValue>(key, value));
		PercolateDown(list.Count - 1);
	}

	public void Clear()
	{
		list.Clear();
	}

	public KeyValuePair<TKey, TValue> TakeMin()
	{
		KeyValuePair<TKey, TValue> result = list[0];
		list[0] = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		PercolateUp(0);
		return result;
	}

	public void PercolateUp(int i)
	{
		while (i * 2 + 1 < this.list.Count)
		{
			int num = i * 2 + 1;
			int num2 = num;
			if (num + 1 < this.list.Count && this.list[num + 1].Key.CompareTo(this.list[num].Key) < 0)
			{
				num2 = num + 1;
			}
			if (this.list[num2].Key.CompareTo(this.list[i].Key) < 0)
			{
				List<KeyValuePair<TKey, TValue>> obj = this.list;
				int index = num2;
				List<KeyValuePair<TKey, TValue>> list = this.list;
				int index2 = i;
				KeyValuePair<TKey, TValue> keyValuePair = this.list[i];
				KeyValuePair<TKey, TValue> keyValuePair2 = this.list[num2];
				KeyValuePair<TKey, TValue> keyValuePair4 = (obj[index] = keyValuePair);
				keyValuePair4 = (list[index2] = keyValuePair2);
				i = num2;
				continue;
			}
			break;
		}
	}

	public void PercolateDown(int i)
	{
		while (i > 0)
		{
			int num = (i - 1) / 2;
			if (this.list[i].Key.CompareTo(this.list[num].Key) < 0)
			{
				List<KeyValuePair<TKey, TValue>> obj = this.list;
				int index = num;
				List<KeyValuePair<TKey, TValue>> list = this.list;
				int index2 = i;
				KeyValuePair<TKey, TValue> keyValuePair = this.list[i];
				KeyValuePair<TKey, TValue> keyValuePair2 = this.list[num];
				KeyValuePair<TKey, TValue> keyValuePair4 = (obj[index] = keyValuePair);
				keyValuePair4 = (list[index2] = keyValuePair2);
				i = num;
				continue;
			}
			break;
		}
	}
}
