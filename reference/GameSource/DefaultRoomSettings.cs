public static class DefaultRoomSettings
{
	public static RoomSettings ancestor;

	static DefaultRoomSettings()
	{
		ancestor = new RoomSettings("roottemplate", null, template: true, firstTemplate: false, null, null);
		ancestor.isAncestor = true;
		ancestor.DangerType = RoomRain.DangerType.Rain;
		ancestor.CeilingDrips = 0.5f;
		ancestor.RainIntensity = 1f;
		ancestor.RumbleIntensity = 1f;
		ancestor.Palette = 0;
		ancestor.EffectColorA = 0;
		ancestor.EffectColorB = 0;
		ancestor.Clouds = 0f;
		ancestor.Grime = 0.5f;
		ancestor.BkgDroneVolume = 0.3f;
		ancestor.BkgDroneNoThreatVolume = 1f;
		ancestor.WaveAmplitude = 0f;
		ancestor.WaveLength = 0.5f;
		ancestor.WaveSpeed = 0.75f;
		ancestor.SecondWaveAmplitude = 0f;
		ancestor.SecondWaveLength = 1f / 6f;
		ancestor.RandomItemDensity = 0.5f;
		ancestor.RandomItemSpearChance = 0.2f;
		ancestor.WaterReflectionAlpha = 1f;
		ancestor.TerrainLight = 0.5f;
		ancestor.TerrainStainAmount = 0f;
		ancestor.TerrainStainBrightness = 0.5f;
		ancestor.TerrainStainHeight = 0f;
		ancestor.TerrainWaves = 0f;
		ancestor.TerrainEdgeRadius = 60f;
		ancestor.TerrainGooHeight = 1f;
		ancestor.TerrainGrain = 0.35f;
		ancestor.TerrainDepth = -5f;
		ancestor.TerrainSkyFade = 0f;
		ancestor.TerrainPalette = "NO PALETTE";
	}
}
