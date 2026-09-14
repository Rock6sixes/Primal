using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class WaterSurfaceLightSpawner : UpdatableAndDeletable
{
	public IntVector2 tile;

	public IntVector2? startTile;

	public IntVector2? endTile;

	public List<DebugSprite> debugSprites;

	public int minObstacleTileSize = 3;

	public List<WallLight.WallLightData> lights;

	public bool lightsSpawned;

	public bool lightsFound;

	public bool colorUpdated;

	public bool useBrightestColor;

	public bool addFlatLights;

	public float openWaterLightHeight = 600f;

	public float wholeRoomLightHeight = 150f;

	public float brightness = 1f;

	public RoomPalette.ColorName paletteColor = RoomPalette.ColorName.WaterColor1;

	public Vector3 col;

	public IntVector2? roomWaterStart;

	public IntVector2? roomWaterEnd;

	public bool debug;

	public DebugSprite Sprite(bool isStart, Vector2 position)
	{
		return new DebugSprite(Vector2.zero, new FSprite("Futile_White")
		{
			scale = 1f,
			color = (isStart ? Color.green : Color.red)
		}, room)
		{
			pos = position
		};
	}

	public WaterSurfaceLightSpawner(Room room, float brightness = 1f, bool useBrightestColor = false)
	{
		this.useBrightestColor = useBrightestColor;
		this.brightness = brightness;
		tile = new IntVector2(0, room.defaultWaterLevel);
		lights = new List<WallLight.WallLightData>();
		if (debug)
		{
			debugSprites = new List<DebugSprite>();
		}
		Spawn();
	}

	public WaterSurfaceLightSpawner(Room room, RoomPalette.ColorName paletteColor, float brightness = 1f)
		: this(room, brightness)
	{
		this.paletteColor = paletteColor;
	}

	public override void Update(bool eu)
	{
		Spawn();
	}

	public void Spawn()
	{
		if (room == null)
		{
			return;
		}
		if (!lightsFound)
		{
			while (tile.x <= room.TileWidth)
			{
				FindWaterLightPositions();
				tile.x++;
				IntVector2? intVector = startTile;
				if (!intVector.HasValue)
				{
					continue;
				}
				IntVector2 valueOrDefault = intVector.GetValueOrDefault();
				intVector = endTile;
				if (intVector.HasValue)
				{
					IntVector2 valueOrDefault2 = intVector.GetValueOrDefault();
					IntVector2 valueOrDefault3 = roomWaterStart.GetValueOrDefault();
					if (!roomWaterStart.HasValue)
					{
						valueOrDefault3 = valueOrDefault;
						roomWaterStart = valueOrDefault3;
					}
					roomWaterEnd = valueOrDefault2;
					startTile = null;
					endTile = null;
					Vector2 vector = room.MiddleOfTile(valueOrDefault + Custom.fourDirections[1] * 4);
					Vector2 vector2 = room.MiddleOfTile(valueOrDefault2 + Custom.fourDirections[1] * 4);
					WallLight.WallLightData item = new WallLight.WallLightData(null)
					{
						pos = vector,
						right = vector2 - vector,
						up = new Vector2(-100f, openWaterLightHeight),
						one = vector2 - vector + new Vector2(100f, openWaterLightHeight),
						usePalette = true,
						colorName = paletteColor,
						useBrightestWaterColor = useBrightestColor,
						value = 0.25f * brightness
					};
					lights.Add(item);
					item = new WallLight.WallLightData(null)
					{
						pos = vector + Vector2.left * 80f + Vector2.down * (openWaterLightHeight * 0.16f),
						right = vector2 - vector + Vector2.right * 160f,
						up = new Vector2(-100f, openWaterLightHeight * 0.8f),
						one = vector2 - vector + new Vector2(100f, openWaterLightHeight * 0.8f),
						usePalette = true,
						colorName = paletteColor,
						useBrightestWaterColor = useBrightestColor,
						value = 0.25f * brightness,
						flat = true
					};
					lights.Add(item);
					if (debug)
					{
						debugSprites.Add(Sprite(isStart: true, room.MiddleOfTile(valueOrDefault)));
						debugSprites.Add(Sprite(isStart: false, room.MiddleOfTile(valueOrDefault2)));
					}
				}
			}
			lightsFound = true;
		}
		SpawnWallLights();
	}

	public void SpawnWallLights()
	{
		if (lightsSpawned)
		{
			return;
		}
		foreach (WallLight.WallLightData light in lights)
		{
			room.AddObject(new WallLight(room, light));
		}
		if (lights.Count != 0)
		{
			Vector2 vector = room.MiddleOfTile(tile);
			Vector2 vector2 = Vector2.zero;
			Vector2 vector3 = new Vector2(room.PixelWidth, 0f);
			IntVector2? intVector = roomWaterStart;
			if (intVector.HasValue)
			{
				IntVector2 valueOrDefault = intVector.GetValueOrDefault();
				vector2 = room.MiddleOfTile(valueOrDefault);
			}
			intVector = roomWaterEnd;
			if (intVector.HasValue)
			{
				IntVector2 valueOrDefault2 = intVector.GetValueOrDefault();
				vector3 = room.MiddleOfTile(valueOrDefault2) - vector2;
			}
			float num = 200f + Mathf.Clamp(vector3.x / 3f, 0f, 300f);
			WallLight.WallLightData data = new WallLight.WallLightData(null)
			{
				pos = new Vector2(-200f + vector2.x, vector.y - 40f),
				right = Vector2.right * (vector3.x + 400f),
				up = Vector2.up * wholeRoomLightHeight,
				one = new Vector2(vector3.x + 400f, wholeRoomLightHeight),
				usePalette = true,
				colorName = paletteColor,
				useBrightestWaterColor = useBrightestColor,
				value = 0.5f * brightness
			};
			room.AddObject(new WallLight(room, data));
			if (addFlatLights)
			{
				data = new WallLight.WallLightData(null)
				{
					pos = new Vector2(0f - num + vector2.x, vector.y - wholeRoomLightHeight),
					right = Vector2.right * (vector3.x + num * 2f),
					up = Vector2.up * (wholeRoomLightHeight * 3f) + Vector2.right * (num * 0.5f),
					one = new Vector2(vector3.x + num * 2f - num * 0.5f, wholeRoomLightHeight * 3f),
					usePalette = true,
					colorName = paletteColor,
					useBrightestWaterColor = useBrightestColor,
					value = 0.75f * brightness,
					flat = true
				};
				room.AddObject(new WallLight(room, data));
			}
		}
		lightsSpawned = true;
		if (!debug)
		{
			return;
		}
		foreach (DebugSprite debugSprite in debugSprites)
		{
			room.AddObject(debugSprite);
		}
	}

	public void UpdateColors()
	{
	}

	public void FindWaterLightPositions()
	{
		bool flag = CheckTile(tile);
		if (tile.x == 0 && !room.GetTile(tile).IsSolid())
		{
			startTile = tile + Custom.fourDirections[0] * 20;
			endTile = tile;
		}
		else if (tile.x == room.TileWidth && !room.GetTile(tile).IsSolid())
		{
			startTile = tile;
			endTile = tile + Custom.fourDirections[2] * 20;
		}
		else if (!startTile.HasValue)
		{
			if (flag)
			{
				startTile = tile + Custom.fourDirections[2] * (minObstacleTileSize - 1);
			}
		}
		else if (!flag)
		{
			endTile = tile + Custom.fourDirections[0];
		}
	}

	public bool CheckTile(IntVector2 tile)
	{
		int num = 0;
		for (int i = 0; i < minObstacleTileSize; i++)
		{
			IntVector2 pos = tile + Custom.fourDirections[2] * i;
			num += (room.GetTile(pos).IsSolid() ? 1 : 0);
		}
		return num < minObstacleTileSize;
	}
}
