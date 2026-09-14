using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanCandlePlacer
{
	public class UrbanCandlePlacerData : PlacedObject.Data
	{
		public Vector2 panelPos = Vector2.one * 200f;

		public Vector2 handlePos = Vector2.up * 100f;

		public Vector2 brushHandlePos = Vector2.one * 100f;

		public Vector2 pos;

		public List<UrbanCandle> placedCandles;

		public List<UrbanCandle.UrbanCandleData> candles;

		public float rotationVariation;

		public float rotation;

		public float widthMax = 0.8f;

		public float widthMin = 0.4f;

		public float heightMax = 6f;

		public float heightMin = 1f;

		public int depthMax = 30;

		public int depthMin;

		public int maxCandles = 10;

		public float radius = 100f;

		public UrbanCandlePlacerData(PlacedObject owner)
			: base(owner)
		{
			candles = new List<UrbanCandle.UrbanCandleData>();
			placedCandles = new List<UrbanCandle>();
			pos = owner.pos;
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			int num = -1;
			panelPos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : panelPos.x);
			panelPos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : panelPos.y);
			brushHandlePos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : brushHandlePos.x);
			brushHandlePos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : brushHandlePos.y);
			handlePos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : handlePos.x);
			handlePos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : handlePos.y);
			int b = ((array.Length > ++num) ? ParseInt(array[num]) : 0);
			b = Mathf.Min((array.Length - num) / 6, b);
			for (int i = 0; i < b; i++)
			{
				int num2 = 1 + num + i * 6;
				candles.Add(new UrbanCandle.UrbanCandleData
				{
					pos = new Vector2(ParseFloat(array[num2]), ParseFloat(array[num2 + 1])),
					depth = ParseFloat(array[num2 + 2]),
					height = ParseFloat(array[num2 + 3]),
					width = ParseFloat(array[num2 + 4]),
					rotation = ParseFloat(array[num2 + 5])
				});
			}
		}

		public float ParseFloat(string s)
		{
			return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public int ParseInt(string s)
		{
			return int.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public string SaveCandles()
		{
			return string.Join("~", candles.Select((UrbanCandle.UrbanCandleData v) => v.ToString()));
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}", panelPos.x, panelPos.y, brushHandlePos.x, brushHandlePos.y, handlePos.x, handlePos.y, candles.Count, SaveCandles());
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public static void AddCandles(List<UrbanCandle.UrbanCandleData> candles, Room room, Vector2 pos, float radius, int depthStart, int depthEnd, float minHeight, float maxHeight, float minWidth, float maxWidth, float rotation, float rotationVaritation, int maxCandlesSpawned = 10, int maxSpawnAttempts = 90)
	{
		CorrectOrder(ref depthStart, ref depthEnd);
		CorrectOrder(ref minHeight, ref maxHeight);
		CorrectOrder(ref minWidth, ref maxWidth);
		UrbanCandle.UrbanCandleData urbanCandleData = default(UrbanCandle.UrbanCandleData);
		urbanCandleData.pos = Vector2.zero;
		urbanCandleData.height = 1f;
		urbanCandleData.width = 1f;
		urbanCandleData.depth = 0f;
		urbanCandleData.rotation = 0f;
		UrbanCandle.UrbanCandleData item = urbanCandleData;
		for (int i = 0; i < maxSpawnAttempts; i++)
		{
			float? num = UrbanCandle.AdaptDepth(UnityEngine.Random.Range(depthStart, depthEnd));
			if (num.HasValue)
			{
				float valueOrDefault = num.GetValueOrDefault();
				Vector2 p = pos + Custom.RNV() * (UnityEngine.Random.value * radius);
				item.height = UnityEngine.Random.Range(minHeight, maxHeight);
				item.width = UnityEngine.Random.Range(minWidth, maxWidth);
				item.rotation = rotation + UnityEngine.Random.Range(0f - rotationVaritation, rotationVaritation);
				item.depth = valueOrDefault;
				Vector2? vector = UrbanCandle.FindPos(room, p, valueOrDefault, item.height, item.rotation);
				if (vector.HasValue)
				{
					Vector2 valueOrDefault2 = vector.GetValueOrDefault();
					item.pos = valueOrDefault2;
					candles.Add(item);
					maxCandlesSpawned--;
				}
				if (maxCandlesSpawned <= 0)
				{
					break;
				}
			}
		}
	}

	public static void RemoveCandles(List<UrbanCandle.UrbanCandleData> candles, Vector2 pos, float radius, int depthStart, int depthEnd)
	{
		CorrectOrder(ref depthStart, ref depthEnd);
		float depthEnd2 = UrbanCandle.AdaptDepth(depthStart, removePlayerPlane: false).Value;
		float depthStart2 = UrbanCandle.AdaptDepth(depthEnd, removePlayerPlane: false).Value;
		for (int num = candles.Count - 1; num >= 0; num--)
		{
			if (CandleMatch(candles[num], pos, radius, depthStart2, depthEnd2))
			{
				candles.RemoveAt(num);
			}
		}
	}

	public static void CorrectOrder<T>(ref T a, ref T b) where T : IComparable<T>
	{
		if (a.CompareTo(b) >= 0)
		{
			T val = a;
			a = b;
			b = val;
		}
	}

	public static bool CandleMatch(UrbanCandle.UrbanCandleData candle, Vector2 pos, float radius, float depthStart, float depthEnd)
	{
		if (candle.depth >= depthStart && candle.depth <= depthEnd)
		{
			return Vector2.Distance(candle.pos, pos) < radius;
		}
		return false;
	}

	public static void RespawnCandles(List<UrbanCandle> spawnedCandles, List<UrbanCandle.UrbanCandleData> data, Room room)
	{
		for (int i = 0; i < spawnedCandles.Count; i++)
		{
			spawnedCandles[i].Destroy();
		}
		spawnedCandles.Clear();
		for (int j = 0; j < data.Count; j++)
		{
			spawnedCandles.Add(new UrbanCandle(room, data[j]));
			room.AddObject(spawnedCandles[j]);
		}
	}
}
