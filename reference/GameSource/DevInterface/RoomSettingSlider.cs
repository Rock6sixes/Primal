using UnityEngine;

namespace DevInterface;

public class RoomSettingSlider : Slider
{
	public class Type : ExtEnum<Type>
	{
		public static Type RainIntensity = new Type("RainIntensity", register: true);

		public static Type RumbleIntensity = new Type("RumbleIntensity", register: true);

		public static Type CeilingDrips = new Type("CeilingDrips", register: true);

		public static Type WaveSpeed = new Type("WaveSpeed", register: true);

		public static Type WaveAmplitude = new Type("WaveAmplitude", register: true);

		public static Type WaveLength = new Type("WaveLength", register: true);

		public static Type SecondWaveAmplitude = new Type("SecondWaveAmplitude", register: true);

		public static Type SecondWaveLength = new Type("SecondWaveLength", register: true);

		public static Type Clouds = new Type("Clouds", register: true);

		public static Type Grime = new Type("Grime", register: true);

		public static Type BkgDroneVolume = new Type("BkgDroneVolume", register: true);

		public static Type BkgDroneNoThreatVol = new Type("BkgDroneNoThreatVol", register: true);

		public static Type RandomObjsDens = new Type("RandomObjsDens", register: true);

		public static Type RandomObjsSpearChance = new Type("RandomObjsSpearChance", register: true);

		public static Type WaterReflectionAplha = new Type("WaterReflectionAplha", register: true);

		public static Type TerrainLight = new Type("TerrainLight", register: true);

		public static Type TerrainStainAmount = new Type("TerrainStainAmount", register: true);

		public static Type TerrainStainHeight = new Type("TerrainStainHeight", register: true);

		public static Type TerrainStainBrightness = new Type("TerrainStainBrightness", register: true);

		public static Type TerrainGooHeight = new Type("TerrainGooHeight", register: true);

		public static Type TerrainWaves = new Type("TerrainWaves", register: true);

		public static Type TerrainEdgeRadius = new Type("TerrainEdgeRadius", register: true);

		public static Type TerrainGrain = new Type("TerrainGrain", register: true);

		public static Type TerrainDepth = new Type("TerrainDepth", register: true);

		public static Type TerrainSkyFade = new Type("TerrainSkyFade", register: true);

		public Type(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Type type;

	public RoomSettingSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, Type type)
		: base(owner, IDstring, parentNode, pos, title, inheritButton: true, 110f)
	{
		this.type = type;
	}

	public override void Refresh()
	{
		base.Refresh();
		string text = "";
		if (type == Type.RainIntensity)
		{
			text = (base.RoomSettings.rInts.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.rInts.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.RainIntensity * 100f) + "%";
			RefreshNubPos(base.RoomSettings.RainIntensity);
		}
		else if (type == Type.RumbleIntensity)
		{
			text = (base.RoomSettings.rumInts.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.rumInts.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.RumbleIntensity * 100f) + "%";
			RefreshNubPos(base.RoomSettings.RumbleIntensity);
		}
		else if (type == Type.CeilingDrips)
		{
			text = (base.RoomSettings.cDrips.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.cDrips.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.CeilingDrips * 100f) + "%";
			RefreshNubPos(base.RoomSettings.CeilingDrips);
		}
		else if (type == Type.WaveSpeed)
		{
			text = (base.RoomSettings.wSpeed.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.wSpeed.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(Mathf.Lerp(-1f / 30f, 1f / 30f, base.RoomSettings.WaveSpeed) * 1000f) + "kPX/f";
			RefreshNubPos(base.RoomSettings.WaveSpeed);
		}
		else if (type == Type.WaveLength)
		{
			text = (base.RoomSettings.wLength.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.wLength.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)Mathf.Lerp(50f, 750f, base.RoomSettings.WaveLength) + "px";
			RefreshNubPos(base.RoomSettings.WaveLength);
		}
		else if (type == Type.WaveAmplitude)
		{
			text = (base.RoomSettings.wAmp.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.wAmp.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)Mathf.Lerp(1f, 40f, base.RoomSettings.WaveAmplitude) + "px";
			RefreshNubPos(base.RoomSettings.WaveAmplitude);
		}
		else if (type == Type.SecondWaveLength)
		{
			text = (base.RoomSettings.swLength.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.swLength.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)Mathf.Lerp(0f, 200f, base.RoomSettings.SecondWaveLength) + "%";
			RefreshNubPos(base.RoomSettings.SecondWaveLength);
		}
		else if (type == Type.SecondWaveAmplitude)
		{
			text = (base.RoomSettings.swAmp.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.swAmp.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)Mathf.Lerp(0f, 100f, base.RoomSettings.SecondWaveAmplitude) + "%";
			RefreshNubPos(base.RoomSettings.SecondWaveAmplitude);
		}
		else if (type == Type.Clouds)
		{
			text = (base.RoomSettings.clds.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.clds.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.Clouds * 100f) + "%";
			RefreshNubPos(base.RoomSettings.Clouds);
		}
		else if (type == Type.Grime)
		{
			text = (base.RoomSettings.grm.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.grm.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.Grime * 100f) + "%";
			RefreshNubPos(base.RoomSettings.Grime);
		}
		else if (type == Type.BkgDroneVolume)
		{
			text = (base.RoomSettings.bkgDrnVl.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.bkgDrnVl.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.BkgDroneVolume * 100f) + "%";
			RefreshNubPos(base.RoomSettings.BkgDroneVolume);
		}
		else if (type == Type.BkgDroneNoThreatVol)
		{
			text = (base.RoomSettings.bkgDrnNoThreatVol.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.bkgDrnNoThreatVol.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.BkgDroneNoThreatVolume * 100f) + "%";
			RefreshNubPos(base.RoomSettings.BkgDroneNoThreatVolume);
		}
		else if (type == Type.RandomObjsDens)
		{
			text = (base.RoomSettings.rndItmDns.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.rndItmDns.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.RandomItemDensity * 100f) + "%";
			RefreshNubPos(base.RoomSettings.RandomItemDensity);
		}
		else if (type == Type.RandomObjsSpearChance)
		{
			text = (base.RoomSettings.rndItmSprChnc.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.rndItmSprChnc.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.RandomItemSpearChance * 100f) + "%";
			RefreshNubPos(base.RoomSettings.RandomItemSpearChance);
		}
		else if (type == Type.WaterReflectionAplha)
		{
			text = (base.RoomSettings.wtrRflctAlpha.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.wtrRflctAlpha.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.WaterReflectionAlpha * 100f) + "%";
			RefreshNubPos(base.RoomSettings.WaterReflectionAlpha);
		}
		else if (type == Type.TerrainLight)
		{
			text = (base.RoomSettings.terrainLight.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainLight.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.TerrainLight * 100f) + "%";
			RefreshNubPos(base.RoomSettings.TerrainLight);
		}
		else if (type == Type.TerrainStainAmount)
		{
			text = (base.RoomSettings.terrainStainAmount.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainStainAmount.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.TerrainStainAmount * 100f) + "%";
			RefreshNubPos(base.RoomSettings.TerrainStainAmount);
		}
		else if (type == Type.TerrainStainBrightness)
		{
			text = (base.RoomSettings.terrainStainBrightness.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainStainBrightness.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.TerrainStainBrightness * 100f) + "%";
			RefreshNubPos(base.RoomSettings.TerrainStainBrightness);
		}
		else if (type == Type.TerrainWaves)
		{
			text = (base.RoomSettings.terrainWaves.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainWaves.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.TerrainWaves * 100f) + "%";
			RefreshNubPos(base.RoomSettings.TerrainWaves);
		}
		else if (type == Type.TerrainEdgeRadius)
		{
			text = (base.RoomSettings.terrainEdgeRadius.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainEdgeRadius.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)base.RoomSettings.TerrainEdgeRadius + "%";
			RefreshNubPos(base.RoomSettings.TerrainEdgeRadius / TerrainCurve.edgeRadiusSliderMax);
		}
		else if (type == Type.TerrainGooHeight)
		{
			text = (base.RoomSettings.terrainGooHeight.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainGooHeight.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.TerrainGooHeight * 100f) + "%";
			RefreshNubPos(base.RoomSettings.TerrainGooHeight);
		}
		else if (type == Type.TerrainGrain)
		{
			text = (base.RoomSettings.terrainGrain.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainGrain.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.TerrainGrain * 100f) + "%";
			RefreshNubPos(base.RoomSettings.TerrainGrain);
		}
		else if (type == Type.TerrainDepth)
		{
			text = (base.RoomSettings.terrainDepth.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainDepth.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + base.RoomSettings.TerrainDepth.ToString(".0");
			RefreshNubPos(Mathf.InverseLerp(TerrainCurve.depthSliderMin, TerrainCurve.depthSliderMax, base.RoomSettings.TerrainDepth));
		}
		else if (type == Type.TerrainStainHeight)
		{
			text = (base.RoomSettings.terrainStainHeight.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainStainHeight.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.TerrainStainHeight * 100f) + "%";
			RefreshNubPos(base.RoomSettings.TerrainStainHeight);
		}
		else if (type == Type.TerrainSkyFade)
		{
			text = (base.RoomSettings.terrainSkyFade.HasValue ? "" : ((base.RoomSettings.parent.isAncestor || !base.RoomSettings.parent.terrainSkyFade.HasValue) ? "<A>" : "<T>"));
			base.NumberText = text + " " + (int)(base.RoomSettings.TerrainSkyFade * 100f) + "%";
			RefreshNubPos(base.RoomSettings.TerrainSkyFade);
		}
	}

	public override void NubDragged(float nubPos)
	{
		if (type == Type.RainIntensity)
		{
			base.RoomSettings.RainIntensity = nubPos;
			Refresh();
		}
		else if (type == Type.RumbleIntensity)
		{
			base.RoomSettings.RumbleIntensity = nubPos;
			Refresh();
		}
		else if (type == Type.CeilingDrips)
		{
			base.RoomSettings.CeilingDrips = nubPos;
			Refresh();
		}
		else if (type == Type.WaveSpeed)
		{
			base.RoomSettings.WaveSpeed = nubPos;
			Refresh();
		}
		else if (type == Type.WaveLength)
		{
			base.RoomSettings.WaveLength = nubPos;
			Refresh();
		}
		else if (type == Type.WaveAmplitude)
		{
			base.RoomSettings.WaveAmplitude = nubPos;
			Refresh();
		}
		else if (type == Type.SecondWaveLength)
		{
			base.RoomSettings.SecondWaveLength = nubPos;
			Refresh();
		}
		else if (type == Type.SecondWaveAmplitude)
		{
			base.RoomSettings.SecondWaveAmplitude = nubPos;
			Refresh();
		}
		else if (type == Type.Clouds)
		{
			base.RoomSettings.Clouds = nubPos;
			Refresh();
		}
		else if (type == Type.Grime)
		{
			base.RoomSettings.Grime = nubPos;
			Shader.SetGlobalFloat(RainWorld.ShadPropGrime, nubPos);
			Refresh();
		}
		else if (type == Type.BkgDroneVolume)
		{
			base.RoomSettings.BkgDroneVolume = nubPos;
			Refresh();
		}
		else if (type == Type.BkgDroneNoThreatVol)
		{
			base.RoomSettings.BkgDroneNoThreatVolume = nubPos;
			Refresh();
		}
		else if (type == Type.RandomObjsDens)
		{
			base.RoomSettings.RandomItemDensity = nubPos;
			Refresh();
		}
		else if (type == Type.RandomObjsSpearChance)
		{
			base.RoomSettings.RandomItemSpearChance = nubPos;
			Refresh();
		}
		else if (type == Type.WaterReflectionAplha)
		{
			base.RoomSettings.WaterReflectionAlpha = nubPos;
			Refresh();
		}
		else if (type == Type.TerrainLight)
		{
			base.RoomSettings.TerrainLight = nubPos;
			Refresh();
		}
		else if (type == Type.TerrainStainAmount)
		{
			base.RoomSettings.TerrainStainAmount = nubPos;
			Refresh();
		}
		else if (type == Type.TerrainStainHeight)
		{
			base.RoomSettings.TerrainStainHeight = nubPos;
			Refresh();
		}
		else if (type == Type.TerrainStainBrightness)
		{
			base.RoomSettings.TerrainStainBrightness = nubPos;
			Refresh();
		}
		else if (type == Type.TerrainWaves)
		{
			base.RoomSettings.TerrainWaves = nubPos;
			Refresh();
		}
		else if (type == Type.TerrainEdgeRadius)
		{
			base.RoomSettings.TerrainEdgeRadius = nubPos * TerrainCurve.edgeRadiusSliderMax;
			Refresh();
		}
		else if (type == Type.TerrainGooHeight)
		{
			base.RoomSettings.TerrainGooHeight = nubPos;
			Refresh();
		}
		else if (type == Type.TerrainGrain)
		{
			base.RoomSettings.TerrainGrain = nubPos;
			Refresh();
		}
		else if (type == Type.TerrainDepth)
		{
			base.RoomSettings.TerrainDepth = Mathf.Lerp(TerrainCurve.depthSliderMin, TerrainCurve.depthSliderMax, nubPos);
			Refresh();
		}
		else if (type == Type.TerrainSkyFade)
		{
			base.RoomSettings.TerrainSkyFade = nubPos;
			Refresh();
		}
	}

	public override void ClickedResetToInherent()
	{
		if (type == Type.RainIntensity)
		{
			base.RoomSettings.rInts = null;
			Refresh();
		}
		else if (type == Type.RumbleIntensity)
		{
			base.RoomSettings.rumInts = null;
			Refresh();
		}
		else if (type == Type.CeilingDrips)
		{
			base.RoomSettings.cDrips = null;
			Refresh();
		}
		else if (type == Type.WaveSpeed)
		{
			base.RoomSettings.wSpeed = null;
			Refresh();
		}
		else if (type == Type.WaveLength)
		{
			base.RoomSettings.wLength = null;
			Refresh();
		}
		else if (type == Type.WaveAmplitude)
		{
			base.RoomSettings.wAmp = null;
			Refresh();
		}
		else if (type == Type.SecondWaveLength)
		{
			base.RoomSettings.swLength = null;
			Refresh();
		}
		else if (type == Type.SecondWaveAmplitude)
		{
			base.RoomSettings.swAmp = null;
			Refresh();
		}
		else if (type == Type.Clouds)
		{
			base.RoomSettings.clds = null;
			Refresh();
		}
		else if (type == Type.Grime)
		{
			base.RoomSettings.grm = null;
			Refresh();
		}
		else if (type == Type.BkgDroneVolume)
		{
			base.RoomSettings.bkgDrnVl = null;
			Refresh();
		}
		else if (type == Type.BkgDroneNoThreatVol)
		{
			base.RoomSettings.bkgDrnNoThreatVol = null;
			Refresh();
		}
		else if (type == Type.RandomObjsDens)
		{
			base.RoomSettings.rndItmDns = null;
			Refresh();
		}
		else if (type == Type.RandomObjsSpearChance)
		{
			base.RoomSettings.rndItmSprChnc = null;
			Refresh();
		}
		else if (type == Type.WaterReflectionAplha)
		{
			base.RoomSettings.wtrRflctAlpha = null;
			Refresh();
		}
		else if (type == Type.TerrainLight)
		{
			base.RoomSettings.terrainLight = null;
			Refresh();
		}
		else if (type == Type.TerrainStainAmount)
		{
			base.RoomSettings.terrainStainAmount = null;
			Refresh();
		}
		else if (type == Type.TerrainStainHeight)
		{
			base.RoomSettings.terrainStainHeight = null;
			Refresh();
		}
		else if (type == Type.TerrainStainBrightness)
		{
			base.RoomSettings.terrainStainBrightness = null;
			Refresh();
		}
		else if (type == Type.TerrainWaves)
		{
			base.RoomSettings.terrainWaves = null;
			Refresh();
		}
		else if (type == Type.TerrainEdgeRadius)
		{
			base.RoomSettings.terrainEdgeRadius = null;
			Refresh();
		}
		else if (type == Type.TerrainGooHeight)
		{
			base.RoomSettings.terrainGooHeight = null;
			Refresh();
		}
		else if (type == Type.TerrainGrain)
		{
			base.RoomSettings.terrainGrain = null;
			Refresh();
		}
		else if (type == Type.TerrainDepth)
		{
			base.RoomSettings.terrainDepth = null;
			Refresh();
		}
		else if (type == Type.TerrainSkyFade)
		{
			base.RoomSettings.terrainSkyFade = null;
			Refresh();
		}
	}
}
