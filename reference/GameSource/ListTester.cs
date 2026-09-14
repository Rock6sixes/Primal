using System.Collections.Generic;
using UnityEngine;

public class ListTester
{
	public List<int> list { get; set; }

	public ListTester()
	{
		list = new List<int>();
	}

	public void Update()
	{
		list.Add(Random.Range(1, 100));
	}
}
