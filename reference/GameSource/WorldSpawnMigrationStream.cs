using System.Collections.Generic;
using System.Globalization;
using RWCustom;
using UnityEngine;

public class WorldSpawnMigrationStream
{
	public BezierSpline spline;

	public Dictionary<AbstractRoom, int> rooms = new Dictionary<AbstractRoom, int>();

	public Vector2 lowerLeft = Vector2.zero;

	public float width = 70f;

	public int rate = 30;

	public WorldSpawnMigrationStream nextStream;

	public string destRoom;

	public string nextStreamName;

	public bool[] layers = new bool[3] { true, true, true };

	public string name;

	public bool foundIntersectingRooms;

	public bool DestRoomSealed(RainWorldGame game)
	{
		if (destRoom != null && game.IsStorySession)
		{
			return game.GetStorySession.saveState.miscWorldSaveData.roomsSealedByWeaverAbility.Contains(destRoom.ToLowerInvariant());
		}
		return false;
	}

	public WorldSpawnMigrationStream(BezierSpline spline, string name)
	{
		this.spline = spline;
		this.name = name;
	}

	public void FindIntersectingRooms(World world)
	{
		for (int i = 0; i < spline.Segments; i++)
		{
			BezierCurve bezier = spline.GetBezier(i);
			float segmentLength = spline.GetSegmentLength(i);
			for (int j = 0; (float)j < segmentLength; j += 20)
			{
				float f = (float)j / segmentLength;
				Vector2 point = bezier.GetPoint(f);
				AbstractRoom[] abstractRooms = world.abstractRooms;
				foreach (AbstractRoom abstractRoom in abstractRooms)
				{
					if (!abstractRoom.shelter && !abstractRoom.gate && !rooms.ContainsKey(abstractRoom) && layers[abstractRoom.layer] && GetRoomMapRect(abstractRoom).Contains(point))
					{
						rooms.Add(abstractRoom, i);
					}
				}
			}
		}
	}

	public Rect GetRoomMapRect(AbstractRoom room)
	{
		Vector2 vector = room.mapPos + lowerLeft;
		Vector2 vector2 = Custom.IntVector2ToVector2(room.size) * 1.5f;
		Vector2 vector3 = vector - vector2;
		Vector2 vector4 = vector + vector2;
		return Rect.MinMaxRect(vector3.x, vector3.y, vector4.x, vector4.y);
	}

	public void ApplyToRoom(Room room)
	{
		if (!foundIntersectingRooms)
		{
			FindIntersectingRooms(room.world);
			foundIntersectingRooms = true;
		}
		if (!rooms.ContainsKey(room.abstractRoom))
		{
			return;
		}
		Rect roomMapRect = GetRoomMapRect(room.abstractRoom);
		int num = rooms[room.abstractRoom];
		float point = 1f;
		int segment = num;
		float point2 = 1f;
		bool flag = false;
		bool flag2 = false;
		for (int i = num; i < spline.Segments; i++)
		{
			float segmentLength = spline.GetSegmentLength(i);
			for (int j = 0; (float)j < segmentLength; j += 3)
			{
				float num2 = (float)j / segmentLength;
				Vector2 point3 = spline.GetBezier(i).GetPoint(num2);
				if (roomMapRect.Contains(point3) && !flag)
				{
					point = num2;
					flag = true;
				}
				else if (!roomMapRect.Contains(point3) && flag)
				{
					segment = i;
					point2 = num2;
					flag = false;
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				break;
			}
		}
		if (!flag2)
		{
			segment = spline.Segments - 1;
		}
		BezierSpline section = spline.GetSection(num, point, segment, point2);
		Vector2 vector = new Vector2(roomMapRect.xMin, roomMapRect.yMin) * 6.6666665f;
		BezierCurve[] allBeziers = section.GetAllBeziers();
		for (int k = 0; k < section.Segments; k++)
		{
			section.SetBezier(k, allBeziers[k] * 6.6666665f + -vector);
		}
		room.AddObject(new VoidSpawnMigrationStream(room, section, this));
	}

	public static WorldSpawnMigrationStream Parse(string s, World world)
	{
		string[] array = s.Split(',');
		if (array[0].Contains("^"))
		{
			return OldParse(s, world);
		}
		WorldSpawnMigrationStream worldSpawnMigrationStream = new WorldSpawnMigrationStream(new BezierSpline(ParseVector(array[1]), ParseVector(array[3]), ParseVector(array[2]), ParseVector(array[4])), array[0]);
		if (!string.IsNullOrWhiteSpace(array[5]) && array[5] != "None")
		{
			worldSpawnMigrationStream.nextStreamName = array[5];
		}
		worldSpawnMigrationStream.layers = new bool[3];
		string text = array[6];
		foreach (char c in text)
		{
			worldSpawnMigrationStream.layers[int.Parse(c.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture)] = true;
		}
		worldSpawnMigrationStream.width = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
		worldSpawnMigrationStream.rate = int.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
		if (array.Length > 9 && !string.IsNullOrWhiteSpace(array[9]) && array[9] != "None")
		{
			worldSpawnMigrationStream.destRoom = array[9];
		}
		return worldSpawnMigrationStream;
	}

	public static WorldSpawnMigrationStream OldParse(string s, World world)
	{
		string[] array = s.Split(',');
		List<BezierSpline.Midpoint> list = new List<BezierSpline.Midpoint>();
		string[] array2 = array[4].Split('|');
		foreach (string text in array2)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				list.Add(BezierSpline.Midpoint.FromString(text));
			}
		}
		return new WorldSpawnMigrationStream(new BezierSpline(ParseVector(array[0]), ParseVector(array[2]), ParseVector(array[1]), ParseVector(array[3]), list.ToArray()), NewName(world));
	}

	public static string NewName(World world)
	{
		if (world.voidSpawnWorldAI != null)
		{
			int num = world.voidSpawnWorldAI.worldMigrationStreams.Count + 1;
			while (world.voidSpawnWorldAI.GetWorldMigrationStream("stream" + num) != null)
			{
				num++;
			}
			return "stream" + num;
		}
		return "stream1";
	}

	public string Serialize()
	{
		string text = "";
		for (int i = 0; i < layers.Length; i++)
		{
			if (layers[i])
			{
				text += i;
			}
		}
		return string.Join(",", name, SerializeVector(spline.posA), SerializeVector(spline.posB), SerializeVector(spline.handleA), SerializeVector(spline.handleB), nextStreamName ?? "None", text, width, rate, destRoom ?? "None");
	}

	public List<string> SerializeMidpoints()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < spline.midpoints.Count; i++)
		{
			list.Add(string.Join(",", name, i, spline.midpoints[i].Floor().ToString()));
		}
		return list;
	}

	public static Vector2 ParseVector(string s)
	{
		string[] array = s.Split('^');
		return new Vector2(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	public static string SerializeVector(Vector2 v)
	{
		return Mathf.Floor(v.x) + "^" + Mathf.Floor(v.y);
	}
}
