using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class GooDripSource : UpdatableAndDeletable
{
	public class GooDripsData : PlacedObject.ResizableObjectData
	{
		public Vector2 panelPos;

		public float frequency = 0.5f;

		public GooDripsData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			frequency = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}", panelPos.x, panelPos.y, frequency);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public PlacedObject placedObject;

	public List<IntVector2> dripTiles;

	public GooDripsData Data => placedObject.data as GooDripsData;

	public GooDripSource(PlacedObject placedObject)
	{
		this.placedObject = placedObject;
	}

	public float GetFrequency()
	{
		return Data.frequency;
	}

	public void SetFrequency(float amount)
	{
		Data.frequency = amount;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (dripTiles == null)
		{
			RefreshCeilingTiles();
		}
		for (int i = 0; i < dripTiles.Count; i++)
		{
			if (Random.value < Data.frequency * 0.02f)
			{
				IntVector2 pos = dripTiles[Random.Range(0, dripTiles.Count)];
				Vector2 ceilingPoint = GetCeilingPoint(room, pos, Random.value);
				room.AddObject(new GooDrip(ceilingPoint, Random.Range(20f, 80f)));
			}
		}
	}

	public void RefreshCeilingTiles()
	{
		if (room != null)
		{
			dripTiles = FindCeilingTiles(room, placedObject.pos, Data.Rad);
		}
		else
		{
			dripTiles = null;
		}
	}

	public static Vector2 GetCeilingPoint(Room room, IntVector2 pos, float frac)
	{
		if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Slope)
		{
			Room.SlopeDirection slopeDirection = room.IdentifySlope(pos);
			if (slopeDirection == Room.SlopeDirection.UpRight || slopeDirection == Room.SlopeDirection.DownLeft)
			{
				return pos.ToVector2() * 20f + Vector2.Lerp(new Vector2(0f, 20f), new Vector2(20f, 0f), frac);
			}
			return pos.ToVector2() * 20f + Vector2.Lerp(new Vector2(0f, 0f), new Vector2(20f, 20f), frac);
		}
		return pos.ToVector2() * 20f + new Vector2(Mathf.Clamp01(frac) * 20f, 0f);
	}

	public static List<IntVector2> FindCeilingTiles(Room room, Vector2 pos, float radius)
	{
		List<IntVector2> list = new List<IntVector2>();
		IntVector2 tilePosition = room.GetTilePosition(pos);
		int num = tilePosition.x - (int)radius;
		int num2 = tilePosition.x + (int)radius;
		int num3 = tilePosition.y - (int)radius;
		int num4 = tilePosition.y + (int)radius;
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				Room.Tile tile = room.GetTile(j, i);
				if (Custom.DistLess(pos, room.MiddleOfTile(j, i), radius) && (tile.Terrain == Room.Tile.TerrainType.Solid || tile.Terrain == Room.Tile.TerrainType.Slope) && !room.HasAnySolid(j, i - 1))
				{
					list.Add(new IntVector2(j, i));
				}
			}
		}
		return list;
	}
}
