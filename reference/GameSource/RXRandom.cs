using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RXRandom
{
	public static System.Random _randomSource = new System.Random();

	public static float Float()
	{
		return (float)_randomSource.NextDouble();
	}

	public static float Float(int seed)
	{
		return (float)new System.Random(seed).NextDouble();
	}

	public static double Double()
	{
		return _randomSource.NextDouble();
	}

	public static float Float(float max)
	{
		return (float)_randomSource.NextDouble() * max;
	}

	public static int Int()
	{
		return _randomSource.Next();
	}

	public static int Int(int max)
	{
		if (max == 0)
		{
			return 0;
		}
		return _randomSource.Next() % max;
	}

	public static float Range(float low, float high)
	{
		return low + (high - low) * (float)_randomSource.NextDouble();
	}

	public static int Range(int low, int high)
	{
		int num = high - low;
		if (num == 0)
		{
			return low;
		}
		return low + _randomSource.Next() % num;
	}

	public static bool Bool()
	{
		return _randomSource.NextDouble() < 0.5;
	}

	public static object Select(params object[] objects)
	{
		return objects[_randomSource.Next() % objects.Length];
	}

	public static T AnyItem<T>(T[] items)
	{
		if (items.Length == 0)
		{
			return default(T);
		}
		return items[_randomSource.Next() % items.Length];
	}

	public static T AnyItem<T>(List<T> items)
	{
		if (items.Count == 0)
		{
			return default(T);
		}
		return items[_randomSource.Next() % items.Count];
	}

	public static Vector2 Vector2Normalized()
	{
		return new Vector2(Range(-1f, 1f), Range(-1f, 1f)).normalized;
	}

	public static Vector3 Vector3Normalized()
	{
		return new Vector3(Range(-1f, 1f), Range(-1f, 1f), Range(-1f, 1f)).normalized;
	}

	public static void ShuffleList<T>(List<T> list)
	{
		List<T> collection = list.OrderBy((T _) => _randomSource.Next()).ToList();
		list.Clear();
		list.AddRange(collection);
	}

	public static void Shuffle<T>(this List<T> list)
	{
		List<T> collection = list.OrderBy((T _) => _randomSource.Next()).ToList();
		list.Clear();
		list.AddRange(collection);
	}
}
