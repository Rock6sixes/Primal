using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class GrassBlade
{
	public class TerrainGrassPatchData : PlacedObject.Data
	{
		public Vector2 pos;

		public int amount = 50;

		public float depthOffset = 0.566f;

		public float depthRange = 1f;

		public float maxHeight = 30f;

		public float minHeight = 5f;

		public float fallOff = 1f;

		public float range = 100f;

		public float width = 1f;

		public List<GrassBlade> spawnedGrass;

		public TerrainGrassPatchData(PlacedObject owner)
			: base(owner)
		{
			if (ModManager.DevTools || Custom.rainWorld.setup.devToolsActive)
			{
				spawnedGrass = new List<GrassBlade>();
			}
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			int num = -1;
			pos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.x);
			pos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.y);
			amount = ((array.Length > ++num) ? ParseInt(array[num]) : amount);
			depthOffset = ((array.Length > ++num) ? ParseFloat(array[num]) : depthOffset);
			depthRange = ((array.Length > ++num) ? ParseFloat(array[num]) : depthRange);
			maxHeight = ((array.Length > ++num) ? ParseFloat(array[num]) : maxHeight);
			minHeight = ((array.Length > ++num) ? ParseFloat(array[num]) : minHeight);
			fallOff = ((array.Length > ++num) ? ParseFloat(array[num]) : fallOff);
			range = ((array.Length > ++num) ? ParseFloat(array[num]) : range);
			width = ((array.Length > ++num) ? ParseFloat(array[num]) : width);
		}

		public float ParseFloat(string s)
		{
			return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public int ParseInt(string s)
		{
			return int.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}", pos.x, pos.y, amount, depthOffset, depthRange, maxHeight, minHeight, fallOff, range, width);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public float depth;

	public float bend;

	public float time;

	public float depthInfluence;

	public Room room;

	public Vector2 pos;

	public Vector3 variation;

	public bool active = true;

	public float maxHeight = 30f;

	public float initialBend;

	public float finalHeight;

	public float height;

	public float width;

	public float individual;

	public float segmentHeight;

	public Vector2 perpendicular;

	public Vector2 tip;

	public Color color;

	public static int grassBladeQuads = 4;

	public Vector2[] bladeMesh;

	public Vector2[] lastBladeMesh;

	public static int grassBladeVerts => (grassBladeQuads + 1) * 2;

	public static int[] grassBladeTris => MaskSource.GridTriangles(grassBladeQuads, 1);

	public GrassBlade(Room room, Vector2 pos, float maxHeight, float depth, float widthMultiplier)
	{
		this.room = room;
		this.maxHeight = maxHeight;
		Grass.InitGrassInRoom(room);
		room.grass.grassBlades.Add(this);
		TerrainManager terrain = room.terrain;
		if (terrain != null)
		{
			this.depth = Mathf.Lerp(0.0333f, 1f, depth);
			pos = terrain.roomTerrain.SnapToTerrain(pos.x, this.depth);
		}
		depthInfluence = Mathf.InverseLerp(0.16f, 0f, Mathf.Abs(this.depth - 0.16f));
		initialBend = Random.Range(-7, 7);
		bend = initialBend;
		variation = new Vector3(Random.value, Random.value, Random.value);
		height = 0.5f + Mathf.InverseLerp(0.1f, 0.3f, this.depth) * 0.5f + Mathf.InverseLerp(0.5f, 1f, this.depth) * 0.3f;
		if (depth < 0.04f)
		{
			height += 0.4f;
			pos += Vector2.down * (30f + Mathf.Pow(room.roomSettings.TerrainGooHeight, 2f) * 1500f);
		}
		finalHeight = height * maxHeight;
		finalHeight = Mathf.Lerp(finalHeight * 0.7f, finalHeight, variation.x);
		width = 2f + 6f * height;
		width = Mathf.Lerp(width * 0.7f, width, variation.y);
		width *= widthMultiplier;
		individual = 0.004f + (1f - this.depth) * 0.5f + ((variation.z > 0.5f) ? 0.5f : 0f);
		segmentHeight = finalHeight / (float)(grassBladeQuads - 1);
		color = new Color(1f - this.depth, individual, 0f, 0f);
		bladeMesh = new Vector2[grassBladeVerts];
		lastBladeMesh = new Vector2[grassBladeVerts];
		this.pos = pos;
		UpdateBladeMesh(init: true);
	}

	public static void SpawnGrassPatch(Room room, TerrainGrassPatchData data)
	{
		float a = ((data.minHeight < data.maxHeight) ? data.minHeight : data.maxHeight);
		float b = ((data.minHeight < data.maxHeight) ? data.maxHeight : data.minHeight);
		if (ModManager.DevTools || Custom.rainWorld.setup.devToolsActive)
		{
			for (int i = 0; i < data.spawnedGrass.Count; i++)
			{
				if (room.grass == null)
				{
					break;
				}
				room.grass.grassBlades.Remove(data.spawnedGrass[i]);
			}
			data.spawnedGrass.Clear();
		}
		for (int j = 0; j < data.amount; j++)
		{
			float num = Random.Range(Mathf.Max(0f, data.depthOffset - data.depthRange * 0.5f), Mathf.Min(1f, data.depthOffset + data.depthRange * 0.5f));
			float num2 = Random.Range(0f - data.range, data.range);
			num2 *= 1f - Mathf.Abs(data.depthOffset - num) * 0.2f * data.depthRange;
			float t = Mathf.InverseLerp(data.range, data.range * data.fallOff * 0.999f, Mathf.Abs(num2));
			float num3 = Mathf.Lerp(a, b, t);
			GrassBlade item = new GrassBlade(room, data.pos + Vector2.right * num2, num3, num, data.width);
			if (ModManager.DevTools || Custom.rainWorld.setup.devToolsActive)
			{
				data.spawnedGrass.Add(item);
			}
		}
		room.grass.needsShuffle = true;
	}

	public void UpdateBladeMesh(ref Vector3[] verts, ref TriangleMesh spriteMesh, ref Color[] col, int index, float timeStacker)
	{
		active = true;
		_ = pos;
		index *= grassBladeVerts;
		for (int i = 0; i <= grassBladeQuads; i++)
		{
			int num = index + i * 2;
			int num2 = num + 1;
			col[num] = color;
			col[num2] = color;
			if (i == 0)
			{
				col[num].b = 1f;
				col[num2].b = 1f;
			}
			verts[num] = Vector2.Lerp(lastBladeMesh[i * 2], bladeMesh[i * 2], timeStacker);
			verts[num2] = Vector2.Lerp(lastBladeMesh[i * 2 + 1], bladeMesh[i * 2 + 1], timeStacker);
			spriteMesh.vertices[num] = verts[num];
			spriteMesh.vertices[num2] = verts[num2];
		}
		spriteMesh.verticeColors = col;
	}

	public void UpdateBladeMesh(bool init = false)
	{
		tip = Custom.DegToVec(bend * (0f - (3f + height * 5f)));
		perpendicular = Vector2.Perpendicular(tip);
		bladeMesh.CopyTo(lastBladeMesh, 0);
		for (int i = 0; i <= grassBladeQuads; i++)
		{
			int num = i * 2;
			int num2 = num + 1;
			float num3 = (float)i / (float)grassBladeQuads;
			Vector2 vector = Vector2.Lerp(Vector2.up, tip, num3);
			Vector2 vector2 = Vector2.Lerp(Vector2.left, perpendicular, num3).normalized * width * (1f - num3 * num3);
			bladeMesh[num] = pos + vector * ((float)i * segmentHeight) + vector2;
			if (i == 0)
			{
				bladeMesh[num].y -= 10f;
			}
			bladeMesh[num2] = bladeMesh[num] - vector2 * 2f;
		}
		if (init)
		{
			bladeMesh.CopyTo(lastBladeMesh, 0);
		}
	}

	public static void ZeroOut(ref Vector3[] verts, ref Vector2[] spriteMesh, int index)
	{
		index *= grassBladeVerts;
		for (int i = 0; i < grassBladeVerts; i++)
		{
			verts[index + i] = Vector3.zero;
			spriteMesh[index + i] = Vector2.zero;
		}
	}

	public void Update()
	{
		time += 0.01f;
		if (active)
		{
			GetPushedByCreatures();
			bend += ((Mathf.Sin(pos.x * 0.005f + time) * 0.5f + 0.5f) * 0.4f + (Mathf.Sin(pos.x * 0.073f + time * 3.5f) * 0.5f + 0.5f) * 0.2f) * (10f - Mathf.Abs(bend)) * 0.01f;
			bend = Mathf.Lerp(initialBend, bend, 0.95f);
			UpdateBladeMesh();
			active = false;
		}
	}

	public void GetPushedByCreatures()
	{
		if (depthInfluence <= 0f)
		{
			return;
		}
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				PhysicalObject physicalObject = room.physicalObjects[i][j];
				if (!(physicalObject is Creature))
				{
					continue;
				}
				Creature creature = physicalObject as Creature;
				for (int k = 0; k < creature.bodyChunks.Length; k++)
				{
					float num = Vector2.Distance(creature.bodyChunks[k].pos, pos);
					if (!(num - creature.bodyChunks[k].rad > 20f))
					{
						bend = Mathf.Clamp(Mathf.Lerp(bend, bend - creature.bodyChunks[k].vel.x, 0.1f * Mathf.Clamp01(1f - num / 20f)), -40f, 40f);
					}
				}
			}
		}
	}
}
