using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BigSkyWhaleSpawner : UpdatableAndDeletable
{
	public class Data : PlacedObject.Data
	{
		public int minDelay = 600;

		public int maxDelay = 1200;

		public int direction;

		public Vector2 panelPos;

		public Data(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			minDelay = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			maxDelay = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			direction = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
		}

		public string BaseSaveString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", panelPos.x, panelPos.y, minDelay, maxDelay, direction);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString();
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public PlacedObject pObj;

	public int spawnDelay;

	public int prevDirection;

	public static float ascentAngle = 30f;

	public Data SpawnerData => pObj.data as Data;

	public bool AnyWhalesInRoom
	{
		get
		{
			foreach (UpdatableAndDeletable update in room.updateList)
			{
				if (update is BigSkyWhale)
				{
					return true;
				}
			}
			return false;
		}
	}

	public BigSkyWhaleSpawner(Room room, PlacedObject pObj)
	{
		base.room = room;
		this.pObj = pObj;
		spawnDelay = Random.Range(SpawnerData.minDelay, SpawnerData.maxDelay);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!AnyWhalesInRoom)
		{
			if (spawnDelay > 0)
			{
				spawnDelay--;
			}
			else
			{
				SpawnSkyWhale();
			}
		}
	}

	public static List<Vector2> GetPath(Room room, int direction)
	{
		List<Vector2> list = new List<Vector2>();
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if (placedObject.type == PlacedObject.Type.SkyWhalePathfinding)
			{
				list.Add(placedObject.pos + (placedObject.data as PlacedObject.SkyWhalePathfindingData).handlePos * Random.value);
			}
		}
		if (list.Count == 0)
		{
			return list;
		}
		list.Sort((Vector2 a, Vector2 b) => a.x.CompareTo(b.x));
		bool flag = direction < 0 || (direction == 0 && Random.value < 0.5f);
		list.Insert(0, GetEndPoint(room, list[0], left: true, flag ? BigSkyWhale.bodyLength : 0f));
		list.Add(GetEndPoint(room, list[list.Count - 1], left: false, flag ? 0f : BigSkyWhale.bodyLength));
		if (flag)
		{
			list.Reverse();
		}
		return list;
	}

	public void SpawnSkyWhale()
	{
		List<Vector2> path = GetPath(room, (prevDirection == 0) ? SpawnerData.direction : (-prevDirection));
		if (path.Count != 0)
		{
			prevDirection = (int)Mathf.Sign(path[path.Count - 1].x - path[0].x);
			room.AddObject(new BigSkyWhale(path));
			spawnDelay = Random.Range(SpawnerData.minDelay, SpawnerData.maxDelay);
		}
	}

	public static Vector2 GetEndPoint(Room room, Vector2 prevPos, bool left, float bodyLength)
	{
		float num = bodyLength + 300f;
		Vector2 vector = Custom.RotateAroundOrigo(Vector2.right, 0f - ascentAngle);
		if (left)
		{
			vector.x = 0f - vector.x;
		}
		Vector2 result = prevPos + ((room.PixelHeight - prevPos.y + 1500f) / vector.y + num) * vector;
		Vector2 result2 = new Vector2(left ? (0f - num) : (room.PixelWidth + num), prevPos.y);
		if (!(prevPos.y < room.PixelHeight) || (!(prevPos.x < 0f) && !(prevPos.x > room.PixelWidth)))
		{
			return result;
		}
		return result2;
	}
}
