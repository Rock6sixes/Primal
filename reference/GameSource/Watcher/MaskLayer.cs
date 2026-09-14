using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class MaskLayer
{
	public bool needSort;

	public bool deleteMe;

	public Dictionary<string, Material> materials;

	public string name { get; set; }

	public List<MaskSource> maskSources { get; set; }

	public int Queue
	{
		set
		{
			foreach (KeyValuePair<string, Material> material in materials)
			{
				int renderQueue = material.Value.renderQueue;
				material.Value.renderQueue = 2001 + value + renderQueue % 3;
			}
			foreach (MaskSource maskSource in maskSources)
			{
				maskSource.UpdateQueue();
			}
		}
	}

	public MaskLayer(string shader)
	{
		materials = new Dictionary<string, Material>();
		maskSources = new List<MaskSource>();
		name = shader;
		MaskSource item = new MaskSource(GetMaterial(shader, 2));
		needSort = true;
		maskSources.Add(item);
	}

	public MaskSource MakeSource(Vector2 pos, Vector3 rotation, Vector3 scale, string shader, bool isSecondLayer = false, Mesh mesh = null)
	{
		if (deleteMe)
		{
			return null;
		}
		MaskSource maskSource = new MaskSource(pos, rotation, scale, GetMaterial(shader, isSecondLayer ? 1 : 0), mesh);
		maskSources.Add(maskSource);
		needSort = true;
		return maskSource;
	}

	public void Update()
	{
		for (int num = maskSources.Count - 1; num >= 0; num--)
		{
			if (maskSources[num].beingDeleted)
			{
				maskSources.RemoveAt(num);
			}
		}
	}

	public void DeleteMe()
	{
		for (int i = 0; i < maskSources.Count; i++)
		{
			maskSources[i].DeleteMe();
		}
		maskSources.Clear();
		maskSources = null;
		materials = null;
		deleteMe = true;
	}

	public Material GetMaterial(string shader, int queueOffset = 0)
	{
		if (materials.TryGetValue(shader + queueOffset, out var value))
		{
			return value;
		}
		FShader fShader = Custom.rainWorld.Shaders[shader];
		value = new Material(fShader.shader);
		string[] keywords = fShader.keywords;
		if (keywords != null)
		{
			for (int i = 0; i < keywords.Length; i++)
			{
				value.EnableKeyword(keywords[i]);
			}
		}
		value.name = $"{name}-{shader}-{queueOffset}";
		value.renderQueue = 2001 + queueOffset;
		materials[shader + queueOffset] = value;
		return value;
	}
}
