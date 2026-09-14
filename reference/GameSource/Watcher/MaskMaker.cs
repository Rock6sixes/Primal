using System.Collections.Generic;
using UnityEngine;

namespace Watcher;

public class MaskMaker
{
	public List<MaskLayer> masks;

	public static MaskMaker instance;

	public bool needSort;

	public static bool isInstanced;

	public static MaskMaker maskMaker
	{
		get
		{
			if (instance == null)
			{
				instance = new MaskMaker();
				isInstanced = true;
			}
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	public void Dispose()
	{
		maskMaker.ClearAllMasks();
		maskMaker = null;
		isInstanced = false;
	}

	public MaskMaker()
	{
		masks = new List<MaskLayer>();
	}

	public MaskLayer MakeMask(string shader)
	{
		MaskLayer maskLayer = FindMask(shader);
		if (maskLayer != null)
		{
			return maskLayer;
		}
		maskLayer = new MaskLayer(shader);
		masks.Add(maskLayer);
		return maskLayer;
	}

	public static MaskSource MakeSource(string maskLayerShader, string maskSourceShader, bool isSecondLayer = false, Mesh mesh = null, Vector2? pos = null, Vector3? rotation = null, Vector3? scale = null)
	{
		MaskSource result = maskMaker.MakeMask(maskLayerShader).MakeSource(pos ?? Vector2.zero, rotation ?? Vector3.zero, scale ?? Vector3.one, maskSourceShader, isSecondLayer, mesh);
		maskMaker.needSort = true;
		maskMaker.Sort();
		return result;
	}

	public void Update()
	{
		if (needSort)
		{
			Sort();
		}
		for (int i = 0; i < masks.Count; i++)
		{
			masks[i].Update();
		}
	}

	public void ClearAllMasks()
	{
		for (int i = 0; i < masks.Count; i++)
		{
			masks[i].DeleteMe();
		}
		masks.Clear();
		masks = null;
	}

	public MaskLayer FindMask(string maskLayerName)
	{
		for (int i = 0; i < masks.Count; i++)
		{
			if (masks[i].name == maskLayerName)
			{
				return masks[i];
			}
		}
		return null;
	}

	public void Sort()
	{
		for (int i = 0; i < masks.Count; i++)
		{
			masks[i].Queue = i * 3;
		}
		needSort = false;
	}

	public void DrawUpdate(float timeStacker, RoomCamera rCam, Vector2 camPos)
	{
		for (int i = 0; i < masks.Count; i++)
		{
			for (int j = 0; j < masks[i].maskSources.Count; j++)
			{
				if (masks[i].maskSources[j].isBaseNode)
				{
					masks[i].maskSources[j].DrawUpdate(timeStacker, rCam, camPos);
				}
			}
		}
	}

	public static void InitTextures()
	{
		Shader.SetGlobalTexture("_DynamicLevelElements", Texture2D.blackTexture);
		Shader.SetGlobalTexture("_SlopedTerrainMask", Texture2D.blackTexture);
		Shader.SetGlobalTexture("_RippleMask", Texture2D.blackTexture);
		Shader.SetGlobalTexture("_UrbanShadowsGrab", Texture2D.blackTexture);
		Shader.SetGlobalTexture("_UrbanShadowsBlurGrab", Texture2D.blackTexture);
		Shader.SetGlobalTexture("_PearlReaderGrab", Texture2D.blackTexture);
	}
}
