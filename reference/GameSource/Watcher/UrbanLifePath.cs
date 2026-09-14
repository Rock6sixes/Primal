using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanLifePath : UpdatableAndDeletable
{
	public class UrbanLifePathData : PlacedObject.Data
	{
		public UrbanLifePath obj;

		public Vector2 panelPos;

		public Vector2 pos;

		public Vector2 pointA;

		public Vector2 pointB;

		public float density;

		public float scale;

		public UrbanLifePathData(PlacedObject owner)
			: base(owner)
		{
			panelPos = Vector2.one * 100f;
			pos = owner.pos;
			pointA = Vector2.left * 100f;
			pointB = Vector2.right * 100f;
			density = 0.5f;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			int num = -1;
			panelPos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : panelPos.x);
			panelPos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : panelPos.y);
			pos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.x);
			pos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.y);
			pointA.x = ((array.Length > ++num) ? ParseFloat(array[num]) : pointA.x);
			pointA.y = ((array.Length > ++num) ? ParseFloat(array[num]) : pointA.y);
			pointB.x = ((array.Length > ++num) ? ParseFloat(array[num]) : pointB.x);
			pointB.y = ((array.Length > ++num) ? ParseFloat(array[num]) : pointB.y);
			density = ((array.Length > ++num) ? ParseFloat(array[num]) : density);
			scale = ((array.Length > ++num) ? ParseFloat(array[num]) : scale);
		}

		public float ParseFloat(string s)
		{
			return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}", panelPos.x, panelPos.y, pos.x, pos.y, pointA.x, pointA.y, pointB.x, pointB.y, density, scale);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public UrbanLifePathData data;

	public Counter spawnCounter = new Counter(160);

	public Counter vehicleSpawnCounter = new Counter(80);

	public UrbanLifePath(Room room, UrbanLifePathData data)
	{
		base.room = room;
		this.data = data;
		InitializePath();
	}

	public static UrbanLifePath FromPlacedObject(Room room, PlacedObject pObj)
	{
		return new UrbanLifePath(room, pObj.data as UrbanLifePathData);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		spawnCounter.Tick();
		vehicleSpawnCounter.Tick();
		if (spawnCounter.isFinished && Random.value > 0.98f)
		{
			spawnCounter.Reset();
			room.AddObject(new UrbanShadow(room, data.pos, data.pointA, data.pointB, isVehicle: false));
		}
		if (vehicleSpawnCounter.isFinished && Random.value > 0.98f)
		{
			vehicleSpawnCounter.Reset();
			room.AddObject(new UrbanShadow(room, data.pos, data.pointA, data.pointB, isVehicle: true));
		}
	}

	public void InitializePath()
	{
		for (int i = 0; i < 5; i++)
		{
			room.AddObject(new UrbanShadow(room, data.pos, data.pointA, data.pointB, isVehicle: false, randomStart: true));
		}
	}
}
