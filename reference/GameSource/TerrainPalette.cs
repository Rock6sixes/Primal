using System;
using System.IO;
using RWCustom;
using UnityEngine;

public class TerrainPalette
{
	public class PaletteInfo
	{
		public Color[] main;

		public Color[] rain;

		public Color[] echo;

		public IntVector2 rainSize;

		public IntVector2 echoSize;

		public string name;

		public IntVector2 PaletteSize { get; set; }

		public PaletteInfo(string name)
		{
			this.name = name;
			Texture2D texture2D = LoadTex(name);
			if (texture2D == null)
			{
				throw new ArgumentException("Could not find terrain palette: " + name);
			}
			Texture2D texture2D2 = LoadTex(name + "_rain");
			Texture2D texture2D3 = LoadTex(name + "_echo");
			if (!texture2D3)
			{
				texture2D3 = LoadTex("default_echo");
			}
			main = texture2D.GetPixels();
			rain = texture2D2?.GetPixels();
			echo = texture2D3?.GetPixels();
			PaletteSize = new IntVector2(texture2D.width, texture2D.height);
			if ((bool)texture2D2)
			{
				rainSize = new IntVector2(texture2D2.width, texture2D2.height);
				PaletteSize = new IntVector2(Mathf.Max(PaletteSize.x, texture2D2.width), Mathf.Max(PaletteSize.y, texture2D2.height));
			}
			if ((bool)texture2D3)
			{
				echoSize = new IntVector2(texture2D3.width, texture2D3.height);
				PaletteSize = new IntVector2(Mathf.Max(PaletteSize.x, texture2D3.width), Mathf.Max(PaletteSize.y, texture2D3.height));
			}
		}

		public void Reload()
		{
			ReloadTex(name);
			ReloadTex(name + "_rain");
			ReloadTex(name + "_echo");
			ReloadTex("default_echo");
		}

		public void Resize(IntVector2 newSize)
		{
			Resize(ref main, PaletteSize, newSize);
			PaletteSize = newSize;
			if (rain != null)
			{
				Resize(ref rain, rainSize, newSize);
				rainSize = newSize;
			}
			if (echo != null)
			{
				Resize(ref echo, echoSize, newSize);
				echoSize = newSize;
			}
		}

		public void GetColors(Color[] colors, float rain, float echo)
		{
			if (colors.Length != main.Length)
			{
				throw new ArgumentException("Color array must match palette size!");
			}
			Array.Copy(main, colors, main.Length);
			if (this.rain != null && rain > 0f)
			{
				LerpColors(colors, this.rain, rain);
			}
			if (this.echo != null && echo > 0f)
			{
				LerpColors(colors, this.echo, echo);
			}
		}

		public void Resize(ref Color[] colors, IntVector2 oldSize, IntVector2 size)
		{
			if (oldSize == size)
			{
				return;
			}
			Color[] array = new Color[size.x * size.y];
			for (int i = 0; i < size.x; i++)
			{
				array[i] = colors[Mathf.Min(i, oldSize.x - 1)];
			}
			int num = (size.y - 1) / 2;
			int x = size.x;
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < num; k++)
				{
					for (int l = 0; l < size.x; l++)
					{
						array[x++] = Sample(colors, oldSize, (float)l / (float)(size.x - 1), (float)k / (float)Mathf.Max(num - 1, 1), j == 1);
					}
				}
			}
			colors = array;
		}

		public Color Sample(Color[] palette, IntVector2 size, float lightness, float depth, bool lit)
		{
			int num = (size.y - 1) / 2;
			float x = (float)(size.x - 1) * lightness;
			float y = (float)(num - 1) * depth + (float)((!lit) ? 1 : (num + 1));
			return Sample(palette, size, x, y);
		}

		public Color Sample(Color[] palette, IntVector2 size, float x, float y)
		{
			int num = Mathf.Clamp((int)x, 0, size.x - 2);
			float t = x - (float)num;
			int num2 = Mathf.Clamp((int)y, 0, size.y - 2);
			float t2 = y - (float)num2;
			int num3 = num + num2 * size.x;
			return Color.Lerp(Color.Lerp(palette[num3], palette[num3 + 1], t), Color.Lerp(palette[num3 + size.x], palette[num3 + size.x + 1], t), t2);
		}

		public Texture2D LoadTex(string imageName)
		{
			string path = AssetManager.ResolveFilePath("terrainpalettes/" + imageName + ".png");
			if (!File.Exists(path))
			{
				return null;
			}
			return HeavyTexturesCache.LoadMiscTexture(path);
		}

		public void ReloadTex(string imageName)
		{
			string path = AssetManager.ResolveFilePath("terrainpalettes/" + imageName + ".png");
			Texture2D miscTexture = HeavyTexturesCache.GetMiscTexture(AssetManager.ResolveFilePath("terrainpalettes/" + imageName + ".png"));
			if ((bool)miscTexture)
			{
				try
				{
					miscTexture.LoadImage(File.ReadAllBytes(path));
				}
				catch
				{
				}
			}
		}
	}

	public Texture2D texture;

	public Color[] texturePixels;

	public Color[] fadePixels;

	public Color[] rotPixels;

	public PaletteInfo mainPal;

	public PaletteInfo fadePal;

	public PaletteInfo rotPal;

	public Color GlitterColor { get; set; }

	public Color LightTint { get; set; }

	public Color DarkDustColor { get; set; }

	public Color LightDustColor { get; set; }

	public Color? SandstormColor { get; set; }

	public string MainPaletteName { get; set; }

	public string FadePaletteName { get; set; }

	public TerrainPalette(string name, string fadeName)
	{
		mainPal = new PaletteInfo(name);
		if (fadeName != null)
		{
			fadePal = new PaletteInfo(fadeName);
		}
		rotPal = new PaletteInfo("rot");
		IntVector2 newSize = mainPal.PaletteSize;
		if (fadePal != null)
		{
			newSize = new IntVector2(Mathf.Max(newSize.x, fadePal.PaletteSize.x), Mathf.Max(newSize.y, fadePal.PaletteSize.y));
		}
		mainPal.Resize(newSize);
		fadePal?.Resize(newSize);
		rotPal.Resize(newSize);
		texture = new Texture2D(newSize.x, newSize.y, TextureFormat.RGBA32, mipChain: false)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		texturePixels = new Color[newSize.x * newSize.y];
		fadePixels = new Color[newSize.x * newSize.y];
		MainPaletteName = name;
		FadePaletteName = fadeName;
		UpdateFade(0f, 0f, 0f, 0f, 0f);
	}

	public void UpdateFade(float fade, float mushroom, float rain, float echo, float rot)
	{
		mainPal.GetColors(texturePixels, rain, echo);
		if (fadePal != null && fade > 0f)
		{
			fadePal.GetColors(fadePixels, rain, echo);
			LerpColors(texturePixels, fadePixels, fade);
		}
		if (rot > 0f)
		{
			if (rotPixels == null)
			{
				rotPixels = new Color[texturePixels.Length];
			}
			rotPal.GetColors(rotPixels, rain, echo);
			LerpColors(texturePixels, rotPixels, rot);
		}
		texture.SetPixels(texturePixels);
		texture.Apply();
		GlitterColor = texturePixels[0];
		LightTint = texturePixels[1];
		DarkDustColor = texturePixels[2];
		LightDustColor = texturePixels[3];
		SandstormColor = texturePixels[4];
		if (SandstormColor == Color.black)
		{
			SandstormColor = null;
		}
	}

	public void Reload()
	{
		mainPal.Reload();
		fadePal?.Reload();
		rotPal?.Reload();
	}

	public static void LerpColors(Color[] from, Color[] to, float t)
	{
		for (int i = 0; i < from.Length && i < to.Length; i++)
		{
			from[i] = Color.Lerp(from[i], to[i], t);
		}
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(texture);
	}
}
