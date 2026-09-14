using System;
using System.Collections.Generic;
using UnityEngine;

public class HeavyTexturesCache
{
	public static List<string> futileAtlasListings = new List<string>();

	public static Dictionary<string, Texture2D> miscTextures = new Dictionary<string, Texture2D>(StringComparer.InvariantCultureIgnoreCase);

	public static void LoadAndCacheAtlasFromTexture(string atlas, Texture texture, bool textureFromAsset)
	{
		if (Futile.atlasManager.GetAtlasWithName(atlas) == null)
		{
			Futile.atlasManager.LoadAtlasFromTexture(atlas, texture, textureFromAsset);
			futileAtlasListings.Add(atlas);
		}
	}

	public static Texture2D LoadMiscTexture(string path, TextureWrapMode wrapMode = TextureWrapMode.Repeat, FilterMode filterMode = FilterMode.Point)
	{
		Texture2D miscTexture = GetMiscTexture(path);
		if (miscTexture != null)
		{
			return miscTexture;
		}
		miscTexture = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
		AssetManager.SafeWWWLoadTexture(ref miscTexture, path, clampWrapMode: false, crispPixels: false);
		miscTexture.wrapMode = wrapMode;
		miscTexture.anisoLevel = 0;
		miscTexture.filterMode = filterMode;
		miscTextures[path] = miscTexture;
		return miscTexture;
	}

	public static Texture2D GetMiscTexture(string path)
	{
		if (!miscTextures.TryGetValue(path, out var value))
		{
			return null;
		}
		return value;
	}

	public static void ClearMiscTextures()
	{
		foreach (Texture2D value in miscTextures.Values)
		{
			UnityEngine.Object.Destroy(value);
		}
		miscTextures.Clear();
	}

	public static void ClearRegisteredFutileAtlases()
	{
		for (int i = 0; i < futileAtlasListings.Count; i++)
		{
			Futile.atlasManager.UnloadAtlas(futileAtlasListings[i]);
		}
		futileAtlasListings.Clear();
	}

	public static void UnloadAtlasTexture(string fileName, bool textureFromAsset)
	{
		futileAtlasListings.Remove(fileName);
		Futile.atlasManager.UnloadAtlas(fileName);
	}
}
