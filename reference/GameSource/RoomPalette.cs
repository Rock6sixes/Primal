using UnityEngine;

public struct RoomPalette
{
	public class ColorName : ExtEnum<ColorName>
	{
		public static ColorName BlackColor = new ColorName("BlackColor", register: true);

		public static ColorName WaterColor1 = new ColorName("WaterColor1", register: true);

		public static ColorName WaterColor2 = new ColorName("WaterColor2", register: true);

		public static ColorName WaterSurfaceColor1 = new ColorName("WaterSurfaceColor1", register: true);

		public static ColorName WaterSurfaceColor2 = new ColorName("WaterSurfaceColor2", register: true);

		public static ColorName WaterShineColor = new ColorName("WaterShineColor", register: true);

		public static ColorName FogColor = new ColorName("FogColor", register: true);

		public static ColorName ShortCutSymbol = new ColorName("ShortCutSymbol", register: true);

		public static ColorName SkyColor = new ColorName("SkyColor", register: true);

		public ColorName(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Color blackColor;

	public Color waterColor1;

	public Color waterColor2;

	public Color waterSurfaceColor1;

	public Color waterSurfaceColor2;

	public Color waterShineColor;

	public Color fogColor;

	public Color shortCutSymbol;

	public Color skyColor;

	public float fogAmount;

	public float darkness;

	public Color[] shortcutColors;

	public Texture2D texture;

	public RoomPalette(Texture2D texture, float fogAmount, float darkness, Color blackColor, Color waterColor1, Color waterColor2, Color waterSurfaceColor1, Color waterSurfaceColor2, Color waterShineColor, Color fogColor, Color skyColor, Color shortcut1, Color shortcut2, Color shortcut3, Color shortCutSymbol)
	{
		this.texture = texture;
		this.fogAmount = fogAmount;
		this.blackColor = blackColor;
		this.waterColor1 = waterColor1;
		this.waterColor2 = waterColor2;
		this.waterSurfaceColor1 = waterSurfaceColor1;
		this.waterSurfaceColor2 = waterSurfaceColor2;
		this.waterShineColor = waterShineColor;
		this.fogColor = fogColor;
		this.skyColor = skyColor;
		this.shortCutSymbol = shortCutSymbol;
		this.darkness = darkness;
		shortcutColors = new Color[3] { shortcut1, shortcut2, shortcut3 };
	}

	public Color GetColor(ColorName colorName)
	{
		if (colorName == ColorName.BlackColor)
		{
			return blackColor;
		}
		if (colorName == ColorName.WaterColor1)
		{
			return waterColor1;
		}
		if (colorName == ColorName.WaterColor2)
		{
			return waterColor2;
		}
		if (colorName == ColorName.WaterSurfaceColor1)
		{
			return waterSurfaceColor1;
		}
		if (colorName == ColorName.WaterSurfaceColor2)
		{
			return waterSurfaceColor2;
		}
		if (colorName == ColorName.WaterShineColor)
		{
			return waterShineColor;
		}
		if (colorName == ColorName.FogColor)
		{
			return fogColor;
		}
		if (colorName == ColorName.ShortCutSymbol)
		{
			return shortCutSymbol;
		}
		if (colorName == ColorName.SkyColor)
		{
			return skyColor;
		}
		return blackColor;
	}
}
