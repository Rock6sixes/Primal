using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SimplePool<T> : MonoBehaviour
{
	[NonSerialized]
	public static SimplePool<T> Instance;

	public Stack<T> pooledObjects;

	public int totalCapacity;

	public void Awake()
	{
		SetupPool();
		Instance = this;
	}

	public void SetupPool(int initialPoolSize = 50)
	{
		totalCapacity = initialPoolSize;
		pooledObjects = new Stack<T>(initialPoolSize);
		for (int i = 0; i < initialPoolSize; i++)
		{
			AddInstanceToPool(i);
		}
	}

	public abstract T CreateInstance(int i);

	public void AddInstanceToPool(int instanceNumber)
	{
		T val = CreateInstance(instanceNumber);
		BeforePush(val);
		pooledObjects.Push(val);
	}

	public T Pop()
	{
		if (pooledObjects.Count == 0)
		{
			AddInstanceToPool(totalCapacity);
			totalCapacity++;
		}
		T val = pooledObjects.Pop();
		BeforePop(val);
		return val;
	}

	public void Push(T obj)
	{
		BeforePush(obj);
		pooledObjects.Push(obj);
	}

	public abstract void BeforePop(T obj);

	public abstract void BeforePush(T obj);

	public SimplePool()
	{
	}
}
