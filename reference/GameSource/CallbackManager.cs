using System;
using System.Collections.Generic;

public class CallbackManager
{
	public static CallbackManager instance;

	public List<IDisposable> callbackList = new List<IDisposable>();

	public static CallbackManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new CallbackManager();
			}
			return instance;
		}
	}

	public void Add(IDisposable callback)
	{
		callbackList.Add(callback);
	}

	public void Clear()
	{
		for (int i = 0; i < callbackList.Count; i++)
		{
			callbackList[i].Dispose();
			callbackList[i] = null;
		}
		callbackList.Clear();
	}
}
