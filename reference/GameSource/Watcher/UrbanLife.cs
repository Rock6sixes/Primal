using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanLife : CosmeticSprite
{
	public class UrbanLifeData : PlacedObject.Data
	{
		public Vector2 panelPos = Vector2.one * 100f;

		public Vector2 pos;

		public Vector2 upLeft;

		public Vector2 downRight;

		public Vector2 direction;

		public float scale;

		public float intensity;

		public float nLayers;

		public UrbanLife obj;

		public bool isShadow;

		public int index;

		public Vector2[] perScreenOffset;

		public UrbanLifeData(PlacedObject owner)
			: base(owner)
		{
			upLeft = (Vector2.up + Vector2.left) * 100f;
			downRight = (Vector2.down + Vector2.right) * 100f;
			direction = Vector2.down * 150f;
			scale = 10f;
			intensity = 0.1f;
			nLayers = 0.5f;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			int num = -1;
			panelPos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : panelPos.x);
			panelPos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : panelPos.y);
			pos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.x);
			pos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.y);
			upLeft.x = ((array.Length > ++num) ? ParseFloat(array[num]) : upLeft.x);
			upLeft.y = ((array.Length > ++num) ? ParseFloat(array[num]) : upLeft.y);
			downRight.x = ((array.Length > ++num) ? ParseFloat(array[num]) : downRight.x);
			downRight.y = ((array.Length > ++num) ? ParseFloat(array[num]) : downRight.y);
			direction.x = ((array.Length > ++num) ? ParseFloat(array[num]) : direction.x);
			direction.y = ((array.Length > ++num) ? ParseFloat(array[num]) : direction.y);
			scale = ((array.Length > ++num) ? ParseFloat(array[num]) : scale);
			intensity = ((array.Length > ++num) ? ParseFloat(array[num]) : intensity);
			nLayers = ((array.Length > ++num) ? ParseFloat(array[num]) : nLayers);
			isShadow = ((array.Length > ++num) ? bool.Parse(array[num]) : isShadow);
			if (array.Length > ++num)
			{
				ParseVectorArray(ref perScreenOffset, array[num]);
			}
		}

		public void ParseVectorArray(ref Vector2[] target, string s)
		{
			List<string> list = s.Split('|').ToList();
			if (list.Count != 0)
			{
				if (target == null)
				{
					target = new Vector2[list.Count];
				}
				for (int i = 0; i < list.Count; i++)
				{
					string[] array = list[i].Split('v');
					target[i] = new Vector2(ParseFloat(array[0]), ParseFloat(array[1]));
				}
			}
		}

		public string SaveVectorArray(Vector2[] a)
		{
			return string.Join("|", a.Select((Vector2 v) => $"{v.x}v{v.y}").ToList());
		}

		public float ParseFloat(string s)
		{
			return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}", panelPos.x, panelPos.y, pos.x, pos.y, upLeft.x, upLeft.y, downRight.x, downRight.y, direction.x, direction.y, scale, intensity, nLayers, isShadow, SaveVectorArray(perScreenOffset));
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class UrbanLifeBlur : CosmeticSprite
	{
		public UrbanLifeBlur(Room room)
		{
			base.room = room;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.maskSources = new MaskSource[2];
			sLeaser.maskSources[0] = MaskMaker.MakeSource("UrbanShadowsGrab", "Basic");
			sLeaser.maskSources[0].setGameObjectScale = Vector3.one * 0.1f;
			sLeaser.maskSources[1] = MaskMaker.MakeSource("UrbanShadowsBlurGrab", "UrbanShadowsBlur");
			sLeaser.maskSources[1].setGameObjectScale = Vector3.one * 300f;
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			Shader.SetGlobalVector("_UrbanLifeCamPos", camPos * renderScale);
			Vector2.Lerp(lastPos, pos, timeStacker);
		}
	}

	public UrbanLifeData data;

	public static float renderScale = 0.22f;

	public static int maxLayers = 100;

	public bool meshDirty;

	public int nLen = 1;

	public int nWidth = 1;

	public int nLayers;

	public float variation;

	public Vector2 lastDirection;

	public Vector2 direction;

	public Vector2 lastDownRight;

	public Vector2 lastDownLeft;

	public Vector2 lastUpRight;

	public Vector2 lastUpLeft;

	public Vector2 sSize => Custom.rainWorld.screenSize;

	public UrbanLife(Room room, UrbanLifeData data)
	{
		this.data = data;
		base.room = room;
		variation = Random.value;
		if (data.perScreenOffset == null)
		{
			data.perScreenOffset = new Vector2[room.cameraPositions.Length];
		}
		if (!room.urbanLifeBlurSpawned)
		{
			room.AddObject(new UrbanLifeBlur(room));
			room.urbanLifeBlurSpawned = true;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastDirection = data.direction;
		direction = data.direction;
		pos = data.pos;
		CheckForCornerChange();
	}

	public void CheckForCornerChange()
	{
		if (lastDownRight != data.downRight || lastUpLeft != data.upLeft)
		{
			meshDirty = true;
		}
		lastDownRight = data.downRight;
		lastUpLeft = data.upLeft;
	}

	public static UrbanLife FromPlacedObject(Room room, PlacedObject pObj)
	{
		return new UrbanLife(room, pObj.data as UrbanLifeData);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		TriangleMesh.Triangle[] tris = TriangleMesh.GridTriangles(nLen, nWidth);
		if (rCam.AUrbanLifeCoord == null)
		{
			rCam.AUrbanLifeCoord = new Vector4[10];
		}
		nLayers = (int)Mathf.Max(1f, data.nLayers * (float)maxLayers);
		sLeaser.sprites = new FSprite[nLayers];
		for (int i = 0; i < nLayers; i++)
		{
			sLeaser.sprites[i] = new TriangleMesh("Futile_White", tris, customColor: true);
			if (data.isShadow)
			{
				sLeaser.sprites[i].shader = Custom.rainWorld.Shaders["UrbanLifeShadow"];
			}
			else
			{
				sLeaser.sprites[i].shader = Custom.rainWorld.Shaders[(i != 0) ? "UrbanLife" : "UrbanLifeFirstLayer"];
			}
		}
		UpdateUV(sLeaser.sprites);
		meshDirty = true;
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Bloom"));
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Shader.SetGlobalColor("_UrbanLifeLightColor", palette.texture.GetPixel(8, 5));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (RoomCamera.doubleZoomMode)
		{
			camPos = camPos * 2f - new Vector2(RoomCamera.halfTileOffsetX ? 10 : 0, RoomCamera.halfTileOffsetY ? 10 : 0);
		}
		if (data.isShadow && !base.slatedForDeletetion && room == rCam.room)
		{
			Shader.EnableKeyword("URBANLIFE");
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		UpdateMesh(sLeaser.sprites);
		float num = nLayers;
		float num2 = (num + num * 0.5f + 1f) / num;
		Vector4 vector = new Vector4((data.upLeft.y + pos.y) / sSize.y, (data.downRight.y + pos.y) / sSize.y, (data.upLeft.x + pos.x) / sSize.x, (data.downRight.x + pos.x) / sSize.x);
		vector *= renderScale;
		rCam.AUrbanLifeCoord[data.index] = vector;
		Vector2 vector2 = Vector2.Lerp(lastDirection, direction, timeStacker);
		vector2.x = rCam.ApplyDepth(vector2, -30f).x + 270f;
		Vector2 vector3 = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
		for (int i = 0; (float)i < num; i++)
		{
			float perc = (float)i / num;
			perc = Custom.LerpCircEaseIn(0f, 1f, perc);
			Vector2 position = vector3 + vector2 * (Mathf.Lerp(perc, perc * perc, 0.125f) * num2);
			position += data.perScreenOffset[rCam.currentCameraPosition];
			sLeaser.sprites[i].SetPosition(position);
			sLeaser.sprites[i].scale = 1f + perc * data.scale * 0.01f;
			Color black = Color.black;
			if (data.isShadow)
			{
				black.r = perc;
				black.g = 1f - data.intensity;
			}
			else
			{
				black.r = 1f - perc;
				black.g = data.intensity;
			}
			black.b = (float)data.index / 255f;
			black.a = variation;
			sLeaser.sprites[i].color = black;
		}
	}

	public void UpdateMesh(FSprite[] meshes)
	{
		if (!meshDirty)
		{
			return;
		}
		for (int i = 0; i < meshes.Length; i++)
		{
			for (int j = 0; j <= nLen; j++)
			{
				TriangleMesh triangleMesh = meshes[i] as TriangleMesh;
				int num = j * (nWidth + 1);
				float t = (float)j / (float)nLen;
				Vector2 a = Vector2.Lerp(new Vector2(data.upLeft.x, data.downRight.y), data.downRight, t);
				Vector2 b = Vector2.Lerp(data.upLeft, new Vector2(data.downRight.x, data.upLeft.y), t);
				triangleMesh.MoveVertice(num, a);
				triangleMesh.MoveVertice(num + nWidth, b);
				for (int k = 1; k < nWidth; k++)
				{
					triangleMesh.MoveVertice(num + k, Vector2.Lerp(a, b, (float)k / (float)nWidth));
				}
			}
		}
		meshDirty = false;
	}

	public void UpdateUV(FSprite[] meshes)
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			for (int j = 0; j <= nLen; j++)
			{
				TriangleMesh triangleMesh = meshes[i] as TriangleMesh;
				int num = j * (nWidth + 1);
				Vector2 vector = Vector2.right * j / nLen;
				Vector2 vector2 = vector + Vector2.up;
				triangleMesh.UVvertices[num] = vector;
				triangleMesh.UVvertices[num + nWidth] = vector2;
				for (int k = 1; k < nWidth; k++)
				{
					triangleMesh.UVvertices[num + k] = Vector2.Lerp(vector, vector2, (float)k / (float)nWidth);
				}
			}
		}
	}
}
