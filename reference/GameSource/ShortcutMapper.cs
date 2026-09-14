using System;
using System.Collections.Generic;
using RWCustom;

public class ShortcutMapper
{
	public Room room;

	public int x;

	public int y;

	public List<ShortcutData> shortcuts;

	public List<IntVector2> shortcutsIndex;

	public List<IntVector2> nodesIndex;

	public bool done;

	public ShortcutMapper(Room rm)
	{
		room = rm;
		x = 0;
		y = room.TileHeight - 1;
		shortcuts = new List<ShortcutData>();
		shortcutsIndex = new List<IntVector2>();
		nodesIndex = new List<IntVector2>();
		for (int i = 0; i < 3; i++)
		{
			int num = -1;
			switch (i)
			{
			case 0:
				num = 2;
				break;
			case 1:
				num = 3;
				break;
			case 2:
				num = 5;
				break;
			}
			for (int num2 = room.TileHeight - 1; num2 >= 0; num2--)
			{
				for (int j = 0; j < room.TileWidth; j++)
				{
					if (room.GetTile(j, num2).shortCut == num)
					{
						nodesIndex.Add(new IntVector2(j, num2));
					}
				}
			}
		}
		room.borderExits = MapRoomBorders();
	}

	public void Update()
	{
		if (room.GetTile(x, y).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
		{
			ShortcutData item = MapShortcut(room, x, y, nodesIndex);
			shortcuts.Add(item);
			shortcutsIndex.Add(new IntVector2(item.StartTile.x, item.StartTile.y));
		}
		x++;
		if (x >= room.TileWidth)
		{
			x = 0;
			y--;
			if (y < 0)
			{
				room.shortcuts = shortcuts.ToArray();
				room.shortcutsIndex = shortcutsIndex.ToArray();
				room.exitAndDenIndex = nodesIndex.ToArray();
				CheckShortcutIndexes();
				done = true;
			}
		}
	}

	public static ShortcutData MapShortcut(Room room, int x, int y, List<IntVector2> nodesIndex)
	{
		IntVector2 intVector = new IntVector2(x, y);
		IntVector2 lastPos = intVector;
		IntVector2 intVector2 = intVector;
		List<IntVector2> list = new List<IntVector2> { intVector };
		int num = 0;
		do
		{
			num++;
			intVector2 = intVector;
			intVector = ShortcutHandler.NextShortcutPosition(intVector, lastPos, room);
			list.Add(intVector);
			if (intVector.x == intVector2.x && intVector.y == intVector2.y)
			{
				return new ShortcutData(room, ShortcutData.Type.DeadEnd, num, new IntVector2(x, y), intVector, -1, list.ToArray());
			}
			lastPos = intVector2;
			if (room.GetTile(intVector).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				return new ShortcutData(room, ShortcutData.Type.Normal, num, new IntVector2(x, y), intVector, -1, list.ToArray());
			}
			if (room.GetTile(intVector).shortCut == 2)
			{
				return new ShortcutData(room, ShortcutData.Type.RoomExit, num, new IntVector2(x, y), intVector, nodesIndex.IndexOf(intVector), list.ToArray());
			}
			if (room.GetTile(intVector).shortCut == 3)
			{
				return new ShortcutData(room, ShortcutData.Type.CreatureHole, num, new IntVector2(x, y), intVector, nodesIndex.IndexOf(intVector), list.ToArray());
			}
			if (room.GetTile(intVector).shortCut == 4)
			{
				return new ShortcutData(room, ShortcutData.Type.NPCTransportation, num, new IntVector2(x, y), intVector, -1, list.ToArray());
			}
			if (room.GetTile(intVector).shortCut == 5)
			{
				return new ShortcutData(room, ShortcutData.Type.RegionTransportation, num, new IntVector2(x, y), intVector, nodesIndex.IndexOf(intVector), list.ToArray());
			}
		}
		while (num <= 1000);
		return new ShortcutData(room, ShortcutData.Type.DeadEnd, num, new IntVector2(x, y), intVector, -1, list.ToArray());
	}

	public void CheckShortcutIndexes()
	{
		for (int i = 0; i < room.TileWidth; i++)
		{
			for (int j = 0; j < room.TileHeight; j++)
			{
				if (room.GetTile(i, j).Terrain == Room.Tile.TerrainType.ShortcutEntrance && Array.IndexOf(room.shortcutsIndex, new IntVector2(i, j)) == -1)
				{
					Custom.LogWarning(room.abstractRoom.name, "SHORTCUT NOT INDEXED! x:", i.ToString(), "y:", j.ToString());
				}
			}
		}
	}

	public RoomBorderExit[] MapRoomBorders()
	{
		if (room.abstractRoom.shelter || room.abstractRoom.gate)
		{
			return new RoomBorderExit[0];
		}
		IntVector2 intVector = new IntVector2(0, 0);
		IntVector2 intVector2 = new IntVector2(0, 1);
		List<RoomBorderExit> list = new List<RoomBorderExit>();
		List<IntVector2> list2 = new List<IntVector2>();
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			AbstractRoomNode.Type type = null;
			switch (i)
			{
			case 0:
				type = AbstractRoomNode.Type.SideExit;
				break;
			case 1:
				type = AbstractRoomNode.Type.SkyExit;
				break;
			case 2:
				type = AbstractRoomNode.Type.SeaExit;
				break;
			}
			if (i == 2 && !room.water)
			{
				break;
			}
			intVector = new IntVector2(0, 0);
			intVector2 = new IntVector2(0, 1);
			list2.Clear();
			bool flag = false;
			IntVector2? intVector3 = null;
			while (num < 100000)
			{
				num++;
				intVector += intVector2;
				bool flag2 = false;
				if (ValidBorderTile(intVector, type))
				{
					if (flag)
					{
						if (!intVector3.HasValue)
						{
							intVector3 = intVector;
							flag2 = true;
						}
						else
						{
							IntVector2 value = intVector;
							IntVector2? intVector4 = intVector3;
							if (value == intVector4)
							{
								break;
							}
						}
						list2.Clear();
					}
					if (intVector3.HasValue)
					{
						list2.Add(intVector);
					}
				}
				else if (!flag && intVector3.HasValue && list2.Count > 0)
				{
					list.Add(new RoomBorderExit(list2.ToArray(), type));
				}
				if (intVector2.y == 1 && intVector.y == room.TileHeight - 1)
				{
					intVector2 = new IntVector2(1, 0);
				}
				else if (intVector2.x == 1 && intVector.x == room.TileWidth - 1)
				{
					intVector2 = new IntVector2(0, -1);
				}
				else if (intVector2.y == -1 && intVector.y == 0)
				{
					intVector2 = new IntVector2(-1, 0);
				}
				else if (intVector2.x == -1 && intVector.x == 0)
				{
					intVector2 = new IntVector2(0, 1);
					if (!intVector3.HasValue)
					{
						intVector3 = intVector;
					}
					else if (intVector == intVector3.Value && !flag2)
					{
						if (list2.Count > 0)
						{
							list.Add(new RoomBorderExit(list2.ToArray(), type));
						}
						break;
					}
				}
				flag = !ValidBorderTile(intVector, type);
			}
		}
		return list.ToArray();
	}

	public bool ValidBorderTile(IntVector2 bPos, AbstractRoomNode.Type type)
	{
		if (room.HasAnySolid(bPos))
		{
			return false;
		}
		if (type == AbstractRoomNode.Type.SideExit)
		{
			if (bPos.x > 0 && bPos.x < room.TileWidth - 1)
			{
				return false;
			}
			if (room.GetTile(bPos).AnyWater)
			{
				return false;
			}
		}
		else if (type == AbstractRoomNode.Type.SkyExit)
		{
			if (bPos.y < room.TileHeight - 1)
			{
				return false;
			}
		}
		else if (type == AbstractRoomNode.Type.SeaExit && !room.GetTile(bPos).AnyWater)
		{
			return false;
		}
		return true;
	}
}
