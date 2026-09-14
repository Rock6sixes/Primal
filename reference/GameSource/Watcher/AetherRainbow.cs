using System;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class AetherRainbow : CosmeticSprite
{
	public class AetherRainbowData : PlacedObject.Data
	{
		public AetherRainbow obj;

		public Vector2 panelPos = Vector2.one * 100f;

		public Vector2 pos;

		public Vector2 start = Vector2.left * 100f;

		public Vector2 end = Vector2.right * 100f;

		public Vector2 arch = Vector2.up * 100f;

		public float width = 0.1f;

		public float specterScale = 1f;

		public float specterOffset = 0.5f;

		public float fade;

		public bool fadeToArch = true;

		public float opacity = 1f;

		public float depth = 0.1428f;

		public float grain;

		public float startDepth;

		public float archDepth;

		public float endDepth;

		public float startWidth;

		public float archWidth;

		public float endWidth;

		public float chance = 0.5f;

		public bool alwaysShow = true;

		public AetherRainbowData(PlacedObject owner)
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
				start.x = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				start.y = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				end.x = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				end.y = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				arch.x = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				arch.y = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				width = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				specterScale = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				specterOffset = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				fade = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				opacity = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				depth = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				grain = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				fadeToArch = bool.Parse(array[num]);
			}
			if (array.Length > ++num)
			{
				startDepth = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				archDepth = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				endDepth = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				startWidth = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				archWidth = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				endWidth = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				chance = ParseFloat(array[num]);
			}
			if (array.Length > ++num)
			{
				alwaysShow = bool.Parse(array[num]);
			}
			if (array.Length > ++num)
			{
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, num);
			}
		}

		public float ParseFloat(string s)
		{
			return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}~{17}~{18}~{19}~{20}~{21}~{22}~{23}~{24}~{25}", panelPos.x, panelPos.y, pos.x, pos.y, start.x, start.y, end.x, end.y, arch.x, arch.y, width, specterScale, specterOffset, fade, opacity, depth, grain, fadeToArch, startDepth, archDepth, endDepth, startWidth, archWidth, endWidth, chance, alwaysShow);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public float _archWidth;

	public float _startWidth;

	public float _endDepth;

	public float _archDepth;

	public float _startDepth;

	public float _grain;

	public float _depth;

	public float _opacity;

	public float _fade;

	public float _specterOffset;

	public float _specterScale;

	public float _width;

	public float _endWidth;

	public bool _alwaysShow;

	public bool _fadeToArch;

	public Vector2 _arch;

	public Vector2 _end;

	public Vector2 _start;

	public bool edgeDirty;

	public bool meshDirty;

	public bool uvDirty;

	public bool colDirty;

	public int nLen = 64;

	public int nWidth = 12;

	public Vector2[] edgePos;

	public Vector2 center;

	public float archPointPos;

	public float chance;

	public float cycleFade;

	public bool concave;

	public bool refreshOnViewed;

	public bool debug;

	public bool debugInit;

	public DebugSprite[] debugSprites;

	public FLabel[] debugLabels;

	public Vector2 start
	{
		get
		{
			return _start;
		}
		set
		{
			Set(ref _start, value, ref edgeDirty);
		}
	}

	public Vector2 end
	{
		get
		{
			return _end;
		}
		set
		{
			Set(ref _end, value, ref edgeDirty);
		}
	}

	public Vector2 arch
	{
		get
		{
			return _arch;
		}
		set
		{
			Set(ref _arch, value, ref edgeDirty);
		}
	}

	public float width
	{
		get
		{
			return _width;
		}
		set
		{
			Set(ref _width, value, ref meshDirty);
		}
	}

	public float specterScale
	{
		get
		{
			return _specterScale;
		}
		set
		{
			Set(ref _specterScale, value, ref uvDirty);
		}
	}

	public float specterOffset
	{
		get
		{
			return _specterOffset;
		}
		set
		{
			Set(ref _specterOffset, value, ref uvDirty);
		}
	}

	public bool fadeToArch
	{
		get
		{
			return _fadeToArch;
		}
		set
		{
			Set(ref _fadeToArch, value, ref colDirty);
		}
	}

	public float fade
	{
		get
		{
			return _fade;
		}
		set
		{
			Set(ref _fade, value, ref colDirty);
		}
	}

	public float opacity
	{
		get
		{
			return _opacity;
		}
		set
		{
			Set(ref _opacity, value, ref colDirty);
		}
	}

	public float depth
	{
		get
		{
			return _depth;
		}
		set
		{
			Set(ref _depth, value, ref colDirty);
		}
	}

	public float grain
	{
		get
		{
			return _grain;
		}
		set
		{
			Set(ref _grain, value, ref colDirty);
		}
	}

	public bool alwaysShow
	{
		get
		{
			return _alwaysShow;
		}
		set
		{
			Set(ref _alwaysShow, value, ref colDirty);
		}
	}

	public float startDepth
	{
		get
		{
			return _startDepth;
		}
		set
		{
			Set(ref _startDepth, value, ref colDirty);
		}
	}

	public float archDepth
	{
		get
		{
			return _archDepth;
		}
		set
		{
			Set(ref _archDepth, value, ref colDirty);
		}
	}

	public float endDepth
	{
		get
		{
			return _endDepth;
		}
		set
		{
			Set(ref _endDepth, value, ref colDirty);
		}
	}

	public float startWidth
	{
		get
		{
			return _startWidth;
		}
		set
		{
			Set(ref _startWidth, value, ref meshDirty);
		}
	}

	public float archWidth
	{
		get
		{
			return _archWidth;
		}
		set
		{
			Set(ref _archWidth, value, ref meshDirty);
		}
	}

	public float endWidth
	{
		get
		{
			return _endWidth;
		}
		set
		{
			Set(ref _endWidth, value, ref meshDirty);
		}
	}

	public void Set<T>(ref T toSet, T value, ref bool dirtyFlag)
	{
		dirtyFlag = dirtyFlag || !toSet.Equals(value);
		toSet = value;
	}

	public AetherRainbow(Room room, AetherRainbowData data)
	{
		pos = data.pos;
		_start = data.start;
		_end = data.end;
		_arch = data.arch;
		_width = data.width;
		_specterScale = data.specterScale;
		_specterOffset = data.specterOffset;
		_fade = data.fade;
		_fadeToArch = data.fadeToArch;
		_opacity = data.opacity;
		_depth = data.depth;
		_grain = data.grain;
		_startDepth = data.startDepth;
		_archDepth = data.archDepth;
		_endDepth = data.endDepth;
		_startWidth = data.startWidth;
		_archWidth = data.archWidth;
		_endWidth = data.endWidth;
		_alwaysShow = data.alwaysShow;
		chance = data.chance;
		edgePos = new Vector2[nLen + 1];
		SetDirty();
		Futile.atlasManager.LoadAtlasFromTexture("rainbow", Resources.Load("Atlases/Rainbow") as Texture2D, textureFromAsset: true);
		if (debug)
		{
			debugSprites = new DebugSprite[nLen + 1];
			debugLabels = new FLabel[3];
		}
	}

	public static AetherRainbow FromPlacedObject(Room room, PlacedObject pObj)
	{
		return new AetherRainbow(room, pObj.data as AetherRainbowData);
	}

	public override void Update(bool eu)
	{
		InitDebug();
		RefreshOnReentry();
		UpdateEdgePos();
		UpdateCycleFade();
		UpdateDebug();
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		TriangleMesh.Triangle[] tris = TriangleMesh.GridTriangles(nLen, nWidth);
		TriangleMesh triangleMesh = new TriangleMesh("rainbow", tris, customColor: true);
		uvDirty = true;
		UpdateUV(triangleMesh);
		colDirty = true;
		UpdateColor(triangleMesh, rCam);
		sLeaser.sprites[0] = triangleMesh;
		if (ModManager.Watcher && Watcher.cfgDisableRainbowGrain.Value)
		{
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["AetherRainbowMinimalGrain"];
		}
		else
		{
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["AetherRainbow"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].SetPosition(pos - camPos);
		UpdateMesh(sLeaser.sprites[0] as TriangleMesh);
		UpdateUV(sLeaser.sprites[0] as TriangleMesh);
		UpdateColor(sLeaser.sprites[0] as TriangleMesh, rCam);
		if (debug && debugInit)
		{
			debugLabels[0].SetPosition(pos + start + Vector2.up * 20f - camPos);
			debugLabels[1].SetPosition(pos + arch + Vector2.up * 20f - camPos);
			debugLabels[2].SetPosition(pos + end + Vector2.up * 20f - camPos);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
	}

	public void RefreshOnReentry()
	{
		if (!room.BeingViewed)
		{
			refreshOnViewed = true;
		}
		else if (refreshOnViewed)
		{
			SetDirty();
			refreshOnViewed = false;
		}
	}

	public void SetDirty()
	{
		edgeDirty = true;
		meshDirty = true;
		uvDirty = true;
		colDirty = true;
	}

	public void UpdateCycleFade()
	{
		if (alwaysShow || room.game.IsArenaSession)
		{
			cycleFade = 1f;
			return;
		}
		float num = cycleFade;
		cycleFade = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(room.world.rainCycle.CycleStartUp, 0.75f) * (float)Math.PI)), 0.6f);
		if (cycleFade != num)
		{
			colDirty = true;
		}
		if (room.world.rainCycle.CycleStartUp >= 1f)
		{
			Destroy();
		}
	}

	public void UpdateEdgePos()
	{
		if (edgeDirty)
		{
			bool flag = concave;
			float num = archPointPos;
			center = FindCenter(start, arch, end);
			Vector2 vector = start - center;
			Vector2 from = end - center;
			Vector2 vector2 = arch - center;
			float num2 = Vector2.SignedAngle(vector2, vector);
			float num3 = Vector2.SignedAngle(from, vector2);
			num2 = ((num2 < 0f) ? (360f + num2) : num2);
			num3 = ((num3 < 0f) ? (360f + num3) : num3);
			float num4 = num2 + num3;
			float num5 = num4 / (float)nLen;
			if (concave = num4 > 360f)
			{
				num2 = 360f - num2;
				num3 = 360f - num3;
				num4 = num2 + num3;
				num5 = (0f - num4) / (float)nLen;
			}
			archPointPos = num2 / num4;
			edgePos[0] = start;
			edgePos[nLen] = end;
			for (int i = 1; i < nLen; i++)
			{
				edgePos[i] = center + Custom.rotateVectorDeg(vector, num5 * (float)i);
			}
			edgeDirty = false;
			uvDirty = ((!uvDirty) ? (concave != flag) : uvDirty);
			colDirty = ((!colDirty) ? (archPointPos != num) : colDirty);
			meshDirty = true;
			if (debug && debugInit)
			{
				debugLabels[0].text = num2.ToString();
				debugLabels[2].text = num3.ToString();
				debugLabels[1].text = num4.ToString();
			}
		}
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
			Vector2 vector = edgePos[i] - center;
			float num2 = (float)i / (float)nLen;
			float num3 = Ease(Triangle(num2, archPointPos));
			vector = vector.normalized * Mathf.Clamp((width + Mathf.Lerp(startWidth, endWidth, num2) + num3 * archWidth) * 600f, 0f, vector.magnitude);
			Vector2 a = edgePos[i] - vector;
			Vector2 b = edgePos[i] + vector;
			mesh.MoveVertice(num, a);
			mesh.MoveVertice(num + nWidth, b);
			for (int j = 1; j < nWidth; j++)
			{
				mesh.MoveVertice(num + j, Vector2.Lerp(a, b, (float)j / (float)nWidth));
			}
		}
		meshDirty = false;
	}

	public void UpdateUV(TriangleMesh mesh)
	{
		if (!uvDirty)
		{
			return;
		}
		float num = specterScale;
		float num2 = specterOffset - 0.5f;
		for (int i = 0; i <= nLen; i++)
		{
			int num3 = i * (nWidth + 1);
			_ = (edgePos[i] - center).normalized * (width * 600f);
			Vector2 vector = -Vector2.up * 0.5f + Vector2.right * i / nLen;
			Vector2 vector2 = vector + Vector2.up;
			vector.y *= num;
			vector2.y *= num;
			vector.y += num2;
			vector2.y += num2;
			if (concave)
			{
				Vector2 vector3 = vector;
				vector = vector2;
				vector2 = vector3;
			}
			mesh.UVvertices[num3] = vector;
			mesh.UVvertices[num3 + nWidth] = vector2;
			for (int j = 1; j < nWidth; j++)
			{
				mesh.UVvertices[num3 + j] = Vector2.Lerp(vector, vector2, (float)j / (float)nWidth);
			}
		}
		uvDirty = false;
	}

	public void UpdateColor(TriangleMesh mesh, RoomCamera rCam)
	{
		if (!colDirty)
		{
			return;
		}
		Color black = Color.black;
		bool flag = ModManager.Watcher && Watcher.cfgDisableRainbowGrain.Value;
		for (int i = 0; i <= nLen; i++)
		{
			int num = i * (nWidth + 1);
			float num2 = (float)i / (float)nLen;
			float num3 = Triangle(num2, archPointPos);
			float num4 = 0f;
			num4 = Mathf.Clamp01(Mathf.InverseLerp(value: fadeToArch ? num3 : (1f - Mathf.Abs((num2 - 0.5f) * 2f)), a: 0f, b: fade + 0.01f));
			for (int j = 0; j <= nWidth; j++)
			{
				float num5 = (float)j / (float)nWidth;
				black.a = Mathf.Clamp01(Mathf.Lerp(0f, num4, Mathf.InverseLerp(0.5f, 0.3f, Mathf.Abs(num5 - 0.5f)))) * opacity * cycleFade * Mathf.InverseLerp(0.2f, 0f, rCam.ghostMode);
				black.r = (flag ? 0f : grain);
				black.g = depth + Mathf.Lerp(startDepth, endDepth, num2) + Ease(num3) * archDepth;
				mesh.verticeColors[num + j] = black;
			}
		}
		colDirty = false;
	}

	public float Ease(float v)
	{
		float num = Mathf.Sign(v);
		v = Mathf.Abs(v);
		return v * (2f - v) * num;
	}

	public float Triangle(float x, float a)
	{
		a = Mathf.Clamp(a, 0.0001f, 0.9999f);
		if (x < a)
		{
			return x / a;
		}
		return (0f - x + 1f) / (1f - a);
	}

	public Vector2 FindCenter(Vector2 a, Vector2 b, Vector2 c)
	{
		Vector2 vector = (a + b) * 0.5f;
		Vector2 b2 = vector + Vector2.Perpendicular(a - b);
		Vector2 vector2 = (b + c) * 0.5f;
		Vector2 b3 = vector2 + Vector2.Perpendicular(c - b);
		return Custom.LineIntersection(vector, b2, vector2, b3);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void UpdateDebug()
	{
		if (debugInit && debug)
		{
			for (int i = 0; i <= nLen; i++)
			{
				debugSprites[i].pos = pos + edgePos[i];
			}
		}
	}

	public void InitDebug()
	{
		if (!debugInit && debug)
		{
			for (int i = 0; i < debugSprites.Length; i++)
			{
				debugSprites[i] = new DebugSprite(default(Vector2), new FSprite("Futile_White")
				{
					color = Color.Lerp(Color.red, Color.green, (float)i / (float)nLen),
					scale = 1f
				}, room);
				room.AddObject(debugSprites[i]);
			}
			for (int j = 0; j < debugLabels.Length; j++)
			{
				debugLabels[j] = new FLabel(Custom.GetFont(), "")
				{
					color = Color.red
				};
				room.game.cameras[0].ReturnFContainer("HUD").AddChild(debugLabels[j]);
			}
			debugInit = true;
		}
	}
}
