using System;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class WallLight : UpdatableAndDeletable, IDrawable
{
	public class WallLightData : PlacedObject.Data
	{
		public WallLight obj;

		public Vector2 pos;

		public Vector2 panelPos = new Vector2(150f, 50f);

		public Vector2 up = Vector2.up * 100f;

		public Vector2 right = Vector2.right * 100f;

		public Vector2 one = Vector2.one * 100f;

		public RoomPalette.ColorName colorName = RoomPalette.ColorName.SkyColor;

		public bool usePalette;

		public float hue;

		public float saturation = 0.5f;

		public float value = 1f;

		public bool useBrightestWaterColor;

		public bool flat;

		public ActiveDuring activeDuring = ActiveDuring.Always;

		public ShadowType shadowType = ShadowType.Soft;

		public WallLightData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			int num = -1;
			if (array.Length > ++num)
			{
				panelPos.x = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				panelPos.y = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				pos.x = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				pos.y = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				up.x = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				up.y = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				right.x = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				right.y = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				one.x = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				one.y = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				hue = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				saturation = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				value = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				usePalette = bool.Parse(array[num]);
			}
			if (array.Length > ++num)
			{
				colorName = new RoomPalette.ColorName(array[num]);
			}
			if (array.Length > ++num)
			{
				useBrightestWaterColor = bool.Parse(array[num]);
			}
			if (array.Length > ++num)
			{
				flat = bool.Parse(array[num]);
			}
			if (array.Length > ++num)
			{
				activeDuring = new ActiveDuring(array[num]);
			}
			if (array.Length > ++num)
			{
				shadowType = new ShadowType(array[num]);
			}
		}

		public float ParseFloat(string s)
		{
			return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}~{17}~{18}", panelPos.x, panelPos.y, pos.x, pos.y, up.x, up.y, right.x, right.y, one.x, one.y, hue, saturation, value, usePalette, colorName.value, useBrightestWaterColor, flat, activeDuring.value, shadowType.value);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class ActiveDuring : ExtEnum<ActiveDuring>
	{
		public static ActiveDuring Always = new ActiveDuring("Always", register: true);

		public static ActiveDuring DuringCycle = new ActiveDuring("With Sun", register: true);

		public static ActiveDuring EndOfCycle = new ActiveDuring("During Night", register: true);

		public ActiveDuring(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class ShadowType : ExtEnum<ShadowType>
	{
		public static ShadowType Hard = new ShadowType("Hard", register: true);

		public static ShadowType Soft = new ShadowType("Soft", register: true);

		public static ShadowType SoftStatic = new ShadowType("Soft static", register: true);

		public ShadowType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public int nLen = 8;

	public int nWidth = 8;

	public Vector2 _pos;

	public Vector2 _up;

	public Vector2 _right;

	public Vector2 _one;

	public Vector2 _lastPos;

	public float _hue;

	public float _saturation;

	public float _value;

	public float fade;

	public bool meshDirty;

	public bool colDirty;

	public bool uvDirty;

	public bool shaderDirty;

	public bool _usePalette;

	public bool _useBrightestWaterColor;

	public bool _flat;

	public bool _softEdge;

	public RoomPalette.ColorName _colorName;

	public TriangleMesh mesh;

	public ActiveDuring _activeDuring;

	public ShadowType _shadowType;

	public Color paletteColor;

	public float finalValue => value * fade;

	public ActiveDuring activeDuring
	{
		get
		{
			return _activeDuring;
		}
		set
		{
			_activeDuring = value;
			fade = 0f;
			colDirty = true;
		}
	}

	public Vector2 pos
	{
		get
		{
			return _pos;
		}
		set
		{
			_pos = value;
		}
	}

	public Vector2 up
	{
		get
		{
			return _up;
		}
		set
		{
			_up = value;
			meshDirty = true;
		}
	}

	public Vector2 right
	{
		get
		{
			return _right;
		}
		set
		{
			_right = value;
			meshDirty = true;
			colDirty = true;
		}
	}

	public Vector2 one
	{
		get
		{
			return _one;
		}
		set
		{
			_one = value;
			meshDirty = true;
		}
	}

	public float hue
	{
		get
		{
			return _hue;
		}
		set
		{
			_hue = value;
			colDirty = true;
		}
	}

	public float saturation
	{
		get
		{
			return _saturation;
		}
		set
		{
			_saturation = value;
			colDirty = true;
		}
	}

	public float value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			colDirty = true;
		}
	}

	public bool usePalette
	{
		get
		{
			return _usePalette;
		}
		set
		{
			_usePalette = value;
			colDirty = true;
		}
	}

	public bool useBrightestWaterColor
	{
		get
		{
			return _useBrightestWaterColor;
		}
		set
		{
			_useBrightestWaterColor = value;
			colDirty = true;
		}
	}

	public bool flat
	{
		get
		{
			return _flat;
		}
		set
		{
			_flat = value;
			shaderDirty = true;
		}
	}

	public ShadowType shadowType
	{
		get
		{
			return _shadowType;
		}
		set
		{
			_shadowType = value;
			shaderDirty = true;
		}
	}

	public RoomPalette.ColorName colorName
	{
		get
		{
			return _colorName;
		}
		set
		{
			_colorName = value;
			colDirty = true;
		}
	}

	public WallLight(Room room, WallLightData data)
	{
		_pos = data.pos;
		_up = data.up;
		_right = data.right;
		_one = data.one;
		_hue = data.hue;
		_saturation = data.saturation;
		_value = data.value;
		_usePalette = data.usePalette;
		_colorName = data.colorName;
		flat = data.flat;
		_useBrightestWaterColor = data.useBrightestWaterColor;
		activeDuring = data.activeDuring;
		_shadowType = data.shadowType;
		mesh = CreateMesh();
		SetDirty();
	}

	public static WallLight FromPlacedObject(Room room, PlacedObject pObj)
	{
		return new WallLight(room, pObj.data as WallLightData);
	}

	public void SetDirty()
	{
		meshDirty = true;
		colDirty = true;
		uvDirty = true;
		shaderDirty = true;
	}

	public override void Destroy()
	{
		base.Destroy();
	}

	public override void Update(bool eu)
	{
		_lastPos = _pos;
		UpdateFade();
		base.Update(eu);
	}

	public void UpdateFade()
	{
		if (activeDuring == ActiveDuring.EndOfCycle)
		{
			float num = fade;
			if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DayNight) > 0f && (float)room.world.rainCycle.dayNightCounter >= 6000f * room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DayNight) * 1.75f)
			{
				fade = Mathf.Lerp(fade, 1f, 0.005f);
			}
			if (num != fade)
			{
				colDirty = true;
			}
		}
		else if (activeDuring == ActiveDuring.DuringCycle)
		{
			float num2 = fade;
			fade = Mathf.Pow(Mathf.InverseLerp(-0.5f, 0f, room.world.rainCycle.ShaderLight), 1.2f);
			if (num2 != fade)
			{
				colDirty = true;
			}
		}
		else
		{
			fade = 1f;
		}
	}

	public TriangleMesh CreateMesh()
	{
		TriangleMesh.Triangle[] tris = TriangleMesh.GridTriangles(nLen, nWidth);
		return new TriangleMesh("Futile_White", tris, customColor: true);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		if (mesh == null)
		{
			mesh = CreateMesh();
		}
		sLeaser.sprites[0] = mesh;
		RefreshShader(sLeaser.sprites[0]);
		UpdateMesh(mesh);
		UpdateUV(mesh);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].SetPosition(Vector2.Lerp(_lastPos, _pos, timeStacker) - camPos);
		RefreshShader(sLeaser.sprites[0]);
		UpdateMesh(sLeaser.sprites[0] as TriangleMesh);
		UpdateUV(sLeaser.sprites[0] as TriangleMesh);
		UpdateColor(sLeaser.sprites[0] as TriangleMesh, rCam);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void RefreshShader(FSprite sprite)
	{
		if (shaderDirty)
		{
			shaderDirty = false;
			if (_softEdge)
			{
				sprite.shader = Custom.rainWorld.Shaders["WallLightSoftEdge"];
			}
			else if (_flat)
			{
				sprite.shader = Custom.rainWorld.Shaders["WallLightFlat"];
			}
			else if (shadowType == ShadowType.Soft)
			{
				sprite.shader = Custom.rainWorld.Shaders["WallLight"];
			}
			else if (shadowType == ShadowType.SoftStatic)
			{
				sprite.shader = Custom.rainWorld.Shaders["WallLightStaticNoise"];
			}
			else if (shadowType == ShadowType.Hard)
			{
				sprite.shader = Custom.rainWorld.Shaders["WallLightHardShadow"];
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer(flat ? "Bloom" : "ForegroundLights").AddChild(sLeaser.sprites[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		UpdatePaletteColor(palette);
	}

	public void UpdatePaletteColor(RoomPalette palette)
	{
		if (_usePalette)
		{
			Color color = palette.GetColor(colorName);
			if (useBrightestWaterColor)
			{
				color = BrightestColor(palette.GetColor(RoomPalette.ColorName.WaterColor1), palette.GetColor(RoomPalette.ColorName.WaterColor2), palette.GetColor(RoomPalette.ColorName.WaterSurfaceColor1), palette.GetColor(RoomPalette.ColorName.WaterSurfaceColor2));
			}
			colDirty = colDirty || color != paletteColor;
			paletteColor = color;
		}
	}

	public Color BrightestColor(Color c1, Color c2, Color c3, Color c4)
	{
		Color color = c1;
		if (Lum(c2) > Lum(color))
		{
			color = c2;
		}
		if (Lum(c3) > Lum(color))
		{
			color = c3;
		}
		if (Lum(c4) > Lum(color))
		{
			color = c4;
		}
		return color;
	}

	public float Lum(Color c)
	{
		return c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
	}

	public void UpdateUV(TriangleMesh mesh)
	{
		if (!uvDirty)
		{
			return;
		}
		_ = (Mathf.Atan2(right.x, right.y) + (float)Math.PI) / ((float)Math.PI * 2f);
		for (int i = 0; i <= nLen; i++)
		{
			int num = i * (nWidth + 1);
			Vector2 vector = Vector2.right * i / nLen;
			Vector2 vector2 = vector + Vector2.up;
			mesh.UVvertices[num] = vector;
			mesh.UVvertices[num + nWidth] = vector2;
			for (int j = 1; j < nWidth; j++)
			{
				mesh.UVvertices[num + j] = Vector2.Lerp(vector, vector2, (float)j / (float)nWidth);
			}
		}
		uvDirty = false;
	}

	public void UpdateMesh(TriangleMesh mesh)
	{
		if (!meshDirty)
		{
			return;
		}
		for (int i = 0; i <= nLen; i++)
		{
			int num = i * (nWidth + 1);
			float t = (float)i / (float)nLen;
			Vector2 a = Vector2.Lerp(Vector2.LerpUnclamped(Vector2.zero, up, -0.05f), Vector2.LerpUnclamped(_right, _one, -0.05f), t);
			Vector2 b = Vector2.LerpUnclamped(up, one, Mathf.Lerp(-0.3f, 1.3f, t));
			mesh.MoveVertice(num, a);
			mesh.MoveVertice(num + nWidth, b);
			for (int j = 1; j < nWidth; j++)
			{
				mesh.MoveVertice(num + j, Vector2.Lerp(a, b, (float)j / (float)nWidth));
			}
		}
		meshDirty = false;
	}

	public void UpdateColor(TriangleMesh mesh, RoomCamera rCam)
	{
		if (colDirty)
		{
			Color color = (_usePalette ? (paletteColor * (flat ? 1f : finalValue)) : Custom.HSL2RGB(_hue, _saturation, finalValue * 0.5f));
			color.a = (flat ? finalValue : ((Mathf.Atan2(right.x, right.y) + (float)Math.PI) / ((float)Math.PI * 2f)));
			for (int i = 0; i < mesh.verticeColors.Length; i++)
			{
				mesh.verticeColors[i] = color;
			}
			colDirty = false;
		}
	}
}
