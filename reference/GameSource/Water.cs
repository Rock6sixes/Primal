using System;
using System.Collections.Generic;
using System.Linq;
using Noise;
using RWCustom;
using SplashWater;
using UnityEngine;
using Watcher;

public class Water : IDrawable, IAccessibilityModifier
{
	public class WaterSoundObject : UpdatableAndDeletable
	{
	}

	public class SurfacePoint
	{
		public Vector2 defaultPos;

		public Vector2 pos;

		public Vector2 lastDefaultPos;

		public Vector2 lastPos;

		public float height;

		public float lastHeight;

		public float nextHeight;

		public Vector2 RoomPos => defaultPos + pos;

		public Vector2 LastRoomPos => defaultPos + lastPos;

		public SurfacePoint(Vector2 defaultPos)
		{
			this.defaultPos = defaultPos;
			lastDefaultPos = defaultPos;
			lastPos = new Vector2(0f, 0f);
			pos = new Vector2(0f, 0f);
			height = 0f;
			lastHeight = 0f;
			nextHeight = 0f;
		}

		public Vector2 DrawPos(float timeStacker)
		{
			return Vector2.Lerp(lastDefaultPos + lastPos, defaultPos + pos, timeStacker);
		}
	}

	public class BubbleEmitter
	{
		public Water water;

		public BodyChunk chunk;

		public float amount;

		public float decrease;

		public BubbleEmitter(Water water, BodyChunk chunk, float amount, float decrease)
		{
			this.water = water;
			this.chunk = chunk;
			this.amount = amount;
			this.decrease = decrease;
		}

		public void Update()
		{
			amount -= decrease;
			if (UnityEngine.Random.value * 40f < amount)
			{
				water.room.AddObject(new Bubble(chunk.pos + Custom.DegToVec(UnityEngine.Random.value * 360f) * (UnityEngine.Random.value * chunk.rad), chunk.vel, bottomBubble: false, fakeWaterBubble: false));
			}
			if (ModManager.MMF && water.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidSea) > 0f)
			{
				amount = 0f;
			}
		}
	}

	public class RippleWave
	{
		public Water water;

		public Vector2 pos;

		public float rad;

		public float speed;

		public float width;

		public float lifeTime;

		public float intensity;

		public float life;

		public RippleWave(Water water, Vector2 pos, float initRad, float speed, float width, float lifeTime, float intensity)
		{
			rad = initRad;
			this.water = water;
			this.pos = pos;
			this.speed = speed;
			this.width = width;
			this.lifeTime = lifeTime;
			this.intensity = intensity;
			life = 1f;
		}

		public void Update()
		{
			rad += speed * Mathf.Pow(1f - water.viscosity, 5f);
			life -= 1f / lifeTime;
			water.Ripple(this);
		}
	}

	public abstract class Surface
	{
		public int totalPoints;

		public Rect bounds;

		public SurfacePoint[,] points;

		public Water water;

		public float sinCounter;

		public float waveAmplitude;

		public float waveSpeed;

		public float waveLength;

		public float rollBackLength;

		public float rollBackAmp;

		public Surface(Water water, Rect bounds)
		{
			this.water = water;
			totalPoints = (int)(bounds.width / water.triangleWidth) + 1;
			points = new SurfacePoint[totalPoints, 2];
			for (int i = 0; i < totalPoints; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					points[i, j] = new SurfacePoint(new Vector2(bounds.xMin + ((float)i + ((j == 0) ? 0f : 0.5f)) * water.triangleWidth, water.originalWaterLevel));
				}
			}
			waveAmplitude = water.waveAmplitude;
			waveSpeed = water.waveSpeed;
			waveLength = water.waveLength;
			rollBackLength = water.rollBackLength;
			rollBackAmp = water.rollBackAmp;
		}

		public int PreviousPoint(float horizontalPosition)
		{
			int num = Mathf.Clamp(Mathf.FloorToInt((horizontalPosition + 250f) / water.triangleWidth) + 2, 0, totalPoints - 1);
			while (num > 0 && horizontalPosition < points[num, 0].defaultPos.x + points[num, 0].pos.x)
			{
				num--;
			}
			return num;
		}

		public int ClosestPoint(float horizontalPosition)
		{
			int num = Custom.IntClamp(PreviousPoint(horizontalPosition), 0, totalPoints - 2);
			float num2 = Mathf.Abs(points[num, 0].defaultPos.x + points[num, 0].pos.x - horizontalPosition);
			float num3 = Mathf.Abs(points[num + 1, 0].defaultPos.x + points[num + 1, 0].pos.x - horizontalPosition);
			if (num2 < num3)
			{
				return num;
			}
			return num + 1;
		}

		public float CurrentWaterLevel(float horizontalPosition)
		{
			int num = PreviousPoint(horizontalPosition);
			int num2 = Custom.IntClamp(num + 1, 0, totalPoints - 1);
			float t = Mathf.InverseLerp(points[num, 0].defaultPos.x + points[num, 0].pos.x, points[num2, 0].defaultPos.x + points[num2, 0].pos.x, horizontalPosition);
			return Mathf.Lerp(points[num, 0].defaultPos.y + points[num, 0].pos.y, points[num2, 0].defaultPos.y + points[num2, 0].pos.y, t);
		}

		public abstract float TargetWaterLevel(float horizontalPosition);

		public void GeneralUpsetSurface(float intensity)
		{
			int num = UnityEngine.Random.Range(0, totalPoints);
			points[num, 0].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * (2f * intensity * Mathf.Pow(1f - water.viscosity, 5f));
			points[num, 0].height += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 4f * intensity * (1f - water.viscosity);
		}

		public void WaterfallHitSurface(float left, float right, float flow)
		{
			int num = PreviousPoint(left);
			int num2 = Custom.IntClamp(PreviousPoint(right) + 1, 0, totalPoints - 1);
			for (int i = num; i <= num2; i++)
			{
				if (UnityEngine.Random.value < flow)
				{
					points[i, 0].height += (1f - water.viscosity) * 1.9f * Mathf.Lerp(-1f, 0.5f, UnityEngine.Random.value) * ((i == num || i == num2) ? (-1f) : 1f) * Mathf.Lerp(0.6f, 1f, flow);
					points[i, 0].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * (5f * Mathf.Pow(1f - water.viscosity, 5f));
				}
			}
		}

		public void DrainAffectSurface(float left, float right, float flow)
		{
			int num = PreviousPoint(left);
			int num2 = Custom.IntClamp(PreviousPoint(right) + 1, 0, totalPoints - 1);
			for (int i = num; i <= num2; i++)
			{
				if (UnityEngine.Random.value < flow)
				{
					points[i, 0].height -= (1f - water.viscosity) * 1.6f * Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value) * ((i == num || i == num2) ? 0.5f : 1f) * flow;
					points[i, 0].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * (5f * Mathf.Pow(1f - water.viscosity, 5f) * flow);
				}
			}
		}

		public virtual void Update()
		{
			float r = water.R;
			float num = 0f;
			Room room = water.room;
			float viscosity = water.viscosity;
			float triangleWidth = water.triangleWidth;
			float num2 = ((room.world.rainCycle == null) ? 0f : room.world.rainCycle.ScreenShake);
			float num3 = ((room.world.rainCycle == null) ? 0f : room.world.rainCycle.MicroScreenShake);
			float rumbleIntensity = room.roomSettings.RumbleIntensity;
			bool flag = room.roomSettings.DangerType != RoomRain.DangerType.None && (!ModManager.DLCShared || room.roomSettings.DangerType != DLCSharedEnums.RoomRainDangerType.Blizzard);
			for (int i = 0; i < totalPoints; i++)
			{
				if (i == 0)
				{
					points[i, 0].nextHeight = (2f * points[i, 0].height + (r - 1f) * points[i, 0].lastHeight + 2f * Mathf.Pow(r, 2f) * (points[i + 1, 0].height - points[i, 0].height)) / (1f + r);
				}
				else if (i == totalPoints - 1)
				{
					points[i, 0].nextHeight = (2f * points[i, 0].height + (r - 1f) * points[i, 0].lastHeight + 2f * Mathf.Pow(r, 2f) * (points[i - 1, 0].height - points[i, 0].height)) / (1f + r);
				}
				else
				{
					points[i, 0].nextHeight = Mathf.Pow(r, 2f) * (points[i - 1, 0].height + points[i + 1, 0].height) + 2f * (1f - Mathf.Pow(r, 2f)) * points[i, 0].height - points[i, 0].lastHeight;
					if (room.GetTile(points[i, 0].defaultPos + new Vector2(0f, points[i, 0].height)).Terrain == Room.Tile.TerrainType.Solid)
					{
						points[i, 0].nextHeight *= (room.waterInFrontOfTerrain ? 0.95f : 0.75f);
					}
				}
				points[i, 0].nextHeight += Mathf.Lerp(0f - waveAmplitude, waveAmplitude, UnityEngine.Random.value) * 0.005f * (1f - viscosity);
				points[i, 0].nextHeight *= 0.99f * (0.1f * (1f - viscosity) + 0.9f);
				if (flag)
				{
					points[i, 0].nextHeight += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * num2 * rumbleIntensity * (1f - viscosity);
				}
				num += points[i, 0].height;
				for (int j = 0; j < 2; j++)
				{
					float num4 = (0f - (float)i) * triangleWidth / waveLength;
					points[i, j].lastPos = points[i, j].pos;
					points[i, j].lastDefaultPos = points[i, j].defaultPos;
					float num5 = points[i, j].height * 3f;
					float num6 = 3f;
					for (int k = -1; k < 2; k += 2)
					{
						if (i + k * 2 > 0 && i + k * 2 < totalPoints && Mathf.Abs(points[i, j].height - points[i + k, j].height) > Mathf.Abs(points[i, j].height - points[i + k * 2, j].height))
						{
							num5 += points[i + k, j].height;
							num6 += 1f;
						}
					}
					points[i, j].pos = new Vector2(0f, num5 / num6);
					points[i, j].pos += Custom.DegToVec((num4 + sinCounter * ((j == 1) ? 1f : (-1f))) * 360f) * waveAmplitude;
					if (flag)
					{
						points[i, j].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * (num3 * 4f * rumbleIntensity * Mathf.Pow(1f - viscosity, 3f));
					}
					points[i, j].pos += Custom.DegToVec((num4 + sinCounter * ((j == 1) ? (-1f) : 1f)) * 360f * rollBackLength) * (waveAmplitude * rollBackAmp * Mathf.Pow(1f - viscosity, 3f));
					points[i, j].defaultPos.y = TargetWaterLevel(points[i, j].defaultPos.x + points[i, j].pos.x);
				}
			}
			num /= (float)totalPoints * 1.5f;
			for (int l = 0; l < totalPoints; l++)
			{
				points[l, 0].lastHeight = points[l, 0].height;
				float num7 = points[l, 0].nextHeight - num;
				if (l > 0 && l < totalPoints - 1)
				{
					num7 = Mathf.Lerp(num7, Mathf.Lerp(points[l - 1, 0].nextHeight, points[l + 1, 0].nextHeight, 0.5f), 0.01f);
				}
				points[l, 0].height = Mathf.Clamp(num7, -40f, 40f);
			}
			sinCounter -= waveSpeed * Mathf.Pow(1f - viscosity, 2f);
		}

		public void Explosion(Vector2 pos, float rad, float frc)
		{
			for (int i = 0; i < totalPoints; i++)
			{
				if (Custom.DistLess(points[i, 0].RoomPos, pos, rad) && water.room.VisualContact(points[i, 0].RoomPos, pos))
				{
					float num = Mathf.InverseLerp(rad, 0f, Vector2.Distance(points[i, 0].RoomPos, pos));
					if (num > 0.8f)
					{
						num *= -2f;
					}
					num *= frc;
					points[i, 0].height += num * ((pos.y > CurrentWaterLevel(pos.x)) ? (-1f) : 1f) * UnityEngine.Random.value * (1f - water.viscosity);
					points[i, 0].pos += Custom.DegToVec(UnityEngine.Random.value) * num;
				}
			}
		}

		public void Explosion(Explosion explosion)
		{
			for (int i = 0; i < totalPoints; i++)
			{
				if (Custom.DistLess(points[i, 0].RoomPos, explosion.pos, explosion.rad) && water.room.VisualContact(explosion.pos, points[i, 0].RoomPos))
				{
					float num = Mathf.Pow(Mathf.InverseLerp(explosion.rad, 0f, Vector2.Distance(points[i, 0].RoomPos, explosion.pos)), 2f);
					Vector2 vector = Custom.DirVec(explosion.pos, points[i, 0].RoomPos);
					points[i, 0].pos += vector * (num * explosion.force * 5f);
					points[i, 0].height += Mathf.Cos(num * (float)Math.PI * 2f) * num * explosion.force * 8f;
					num *= Mathf.InverseLerp(0f, 8f, explosion.force);
					if (UnityEngine.Random.value < Mathf.Pow(num, 0.4f))
					{
						Splash splash = new Splash();
						water.room.AddObject(splash);
						vector.y = Mathf.Abs(vector.y) * 3f;
						vector.x *= 0.15f;
						splash.Reset(Vector2.Lerp(points[Math.Max(0, i - 1), 0].RoomPos, points[Math.Min(points.GetLength(0) - 1, i + 1), 0].RoomPos, UnityEngine.Random.value) - vector * 3f, vector * ((1f + num) * 12f), 6f + 15f * num, 3f + num * 4f);
					}
				}
			}
		}
	}

	public class DefaultSurface : Surface
	{
		public List<PlacedObject> waterCutoffs = new List<PlacedObject>();

		public DefaultSurface(Water water)
			: base(water, new Rect(0f - water.leftMargin, 0f, water.leftMargin + water.room.PixelWidth + water.rightMargin, water.room.PixelHeight))
		{
			foreach (PlacedObject placedObject in water.room.roomSettings.placedObjects)
			{
				if (placedObject.type == PlacedObject.Type.WaterCutoff)
				{
					waterCutoffs.Add(placedObject);
				}
			}
		}

		public override void Update()
		{
			base.Update();
			waveAmplitude = water.waveAmplitude;
			waveSpeed = water.waveSpeed;
			waveLength = water.waveLength;
			rollBackLength = water.rollBackLength;
			rollBackAmp = water.rollBackAmp;
		}

		public override float TargetWaterLevel(float horizontalPosition)
		{
			float num = water.fWaterLevel;
			for (int i = 0; i < waterCutoffs.Count; i++)
			{
				WaterCutoffData waterCutoffData = waterCutoffs[i].data as WaterCutoffData;
				Vector2 pos = waterCutoffs[i].pos;
				Vector2 vector = pos + waterCutoffData.handlePos;
				float num2 = (horizontalPosition - pos.x) / (vector.x - pos.x);
				if (num2 >= 0f && num2 <= 1f)
				{
					float level = pos.y + (vector.y - pos.y) * num2;
					level = water.AlternateSurfaceFloodLevel(level, 1.5f);
					num = Mathf.Min(num, level);
				}
			}
			return num;
		}
	}

	public class AirPocketSurface : Surface
	{
		public PlacedObject pObj;

		public float surfaceLevel;

		public AirPocketData Data => pObj.data as AirPocketData;

		public float DefaultSurfaceLevel => pObj.pos.y + Mathf.Min(0f, Data.handlePos.y) + Data.waterLevel;

		public AirPocketSurface(Water water, PlacedObject pObj)
			: base(water, GetBounds(pObj))
		{
			this.pObj = pObj;
			surfaceLevel = DefaultSurfaceLevel;
			waveAmplitude *= 0.2f;
		}

		public static Rect GetBounds(PlacedObject pObj)
		{
			AirPocketData airPocketData = pObj.data as AirPocketData;
			Vector2 vector = Vector2.Min(pObj.pos, pObj.pos + airPocketData.handlePos);
			Vector2 vector2 = Vector2.Max(pObj.pos, pObj.pos + airPocketData.handlePos);
			return Rect.MinMaxRect(vector.x, vector.y, vector2.x, vector2.y);
		}

		public override void Update()
		{
			base.Update();
			waveAmplitude = water.waveAmplitude * 0.2f;
			waveSpeed = water.waveSpeed;
			waveLength = water.waveLength;
			rollBackLength = water.rollBackLength;
			rollBackAmp = water.rollBackAmp;
			bounds = GetBounds(pObj);
			surfaceLevel = DefaultSurfaceLevel;
			if (Data.flood)
			{
				surfaceLevel = water.AlternateSurfaceFloodLevel(surfaceLevel, 0.5f);
			}
		}

		public override float TargetWaterLevel(float horizontalPosition)
		{
			return surfaceLevel;
		}
	}

	public Room room;

	public float triangleWidth = 20f;

	public bool slatedForDeletetion;

	public float leftMargin;

	public float rightMargin;

	public Surface[] surfaces;

	public Vector4[] airPockets = new Vector4[8];

	public int pointsToRender;

	public float originalWaterLevel;

	public float fWaterLevel;

	public float lastFWaterLevel;

	public List<BubbleEmitter> bubbleEmitters;

	public List<RippleWave> rippleWaves;

	public float dx;

	public float dt;

	public float C;

	public float R;

	public float cosmeticSurfaceDisplace;

	public RoomPalette palette;

	public float cosmeticLowerBorder = -1f;

	public RectangularDynamicSoundLoop waterSounds;

	public RectangularDynamicSoundLoop upsetWaterSounds;

	public WaterSoundObject waterSoundObject;

	public float viscosity;

	public FSprite firstWaterSprite;

	public bool WaterIsLethal;

	public bool WaterIsBrine;

	public DefaultSurface MainSurface => surfaces[0] as DefaultSurface;

	public float waveAmplitude => Mathf.Lerp(1f, 40f, room.roomSettings.WaveAmplitude);

	public float waveSpeed => Mathf.Lerp(-1f / 30f, 1f / 30f, room.roomSettings.WaveSpeed);

	public float waveLength => Mathf.Lerp(50f, 750f, room.roomSettings.WaveLength);

	public float rollBackLength => Mathf.Lerp(2f, 0f, room.roomSettings.SecondWaveLength);

	public float rollBackAmp => room.roomSettings.SecondWaveAmplitude;

	public Water(Room room, int waterLevel)
	{
		this.room = room;
		originalWaterLevel = room.MiddleOfTile(new IntVector2(0, waterLevel)).y;
		if (ModManager.MSC && room.roomRain != null && room.roomRain.globalRain != null && (room.roomSettings.DangerType == RoomRain.DangerType.Flood || room.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain))
		{
			fWaterLevel = originalWaterLevel + room.roomRain.globalRain.flood;
		}
		fWaterLevel = originalWaterLevel;
		dx = 0.0005f * triangleWidth;
		dt = 0.0045f;
		C = 1f;
		R = C * dt / dx;
		leftMargin = 220f;
		rightMargin = 220f;
		_ = leftMargin;
		_ = room.PixelWidth;
		_ = rightMargin;
		surfaces = new Surface[1 + room.roomSettings.placedObjects.Count((PlacedObject x) => x.type == PlacedObject.Type.AirPocket)];
		surfaces[0] = new DefaultSurface(this);
		int num = 1;
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if (placedObject.type == PlacedObject.Type.AirPocket)
			{
				surfaces[num++] = new AirPocketSurface(this, placedObject);
			}
		}
		pointsToRender = Custom.IntClamp((int)((room.game.rainWorld.options.ScreenSize.x + 60f) / triangleWidth) + 2, 0, MainSurface.totalPoints);
		bubbleEmitters = new List<BubbleEmitter>();
		rippleWaves = new List<RippleWave>();
		waterSoundObject = new WaterSoundObject();
		room.AddObject(waterSoundObject);
		if (ModManager.DLCShared && room.waterInverted)
		{
			waterSounds = new RectangularDynamicSoundLoop(waterSoundObject, new FloatRect(0f, room.PixelHeight - (float)(room.defaultWaterLevel - 1) * 20f, room.PixelWidth, room.PixelHeight - (float)room.defaultWaterLevel * 20f), room);
		}
		else
		{
			waterSounds = new RectangularDynamicSoundLoop(waterSoundObject, new FloatRect(0f, (float)(room.defaultWaterLevel - 1) * 20f, room.PixelWidth, (float)room.defaultWaterLevel * 20f), room);
		}
		waterSounds.sound = SoundID.Water_Surface_Calm_LOOP;
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.LethalWater) > 0f)
		{
			WaterIsLethal = true;
		}
		float effectAmount = room.roomSettings.GetEffectAmount(WatcherEnums.RoomEffectType.ToxicBrineWater);
		if (effectAmount > 0f)
		{
			WaterIsBrine = true;
			AddBrineAmbientSounds();
			room.AddObject(new WaterSurfaceLightSpawner(room, RoomPalette.ColorName.WaterColor1, effectAmount)
			{
				addFlatLights = true
			});
		}
	}

	public void Update()
	{
		waterSounds.Update();
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SilenceWater) > 0f)
		{
			waterSounds.Volume = 1f - room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SilenceWater);
		}
		viscosity = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterViscosity);
		bool num = ModManager.DLCShared && room.waterInverted;
		IntVector2 intVector = new IntVector2(UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.defaultWaterLevel));
		if (num)
		{
			intVector = new IntVector2(UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(room.defaultWaterLevel, room.TileHeight * 20));
		}
		if (room.GetTile(intVector).Terrain == Room.Tile.TerrainType.Air && room.GetTile(intVector + new IntVector2(0, -1)).Terrain == Room.Tile.TerrainType.Solid)
		{
			room.AddObject(new Bubble(room.MiddleOfTile(intVector) + new Vector2(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value), -10f), new Vector2(0f, 0f), bottomBubble: true, fakeWaterBubble: false));
		}
		int num2 = 0;
		if (WaterIsBrine)
		{
			SpawnSurfaceFizz(4, RoomPalette.ColorName.WaterColor1);
		}
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			foreach (PhysicalObject item in room.physicalObjects[i])
			{
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					if (viscosity > 0f && bodyChunk.submersion > 0f)
					{
						if (bodyChunk.submersion < 1f)
						{
							bodyChunk.vel.x *= 1f - 0.75f * viscosity;
							if (bodyChunk.vel.y > 0f)
							{
								bodyChunk.vel.y *= 1f - 0.075f * viscosity;
							}
							else
							{
								bodyChunk.vel.y *= 1f - 0.15f * viscosity;
							}
						}
						else
						{
							bodyChunk.vel.x *= 1f - 0.225f * viscosity;
							if (bodyChunk.vel.y > 0f)
							{
								bodyChunk.vel.y *= 1f - 0.1f * viscosity;
							}
							else
							{
								bodyChunk.vel.y *= 1f - 0.15f * viscosity;
							}
						}
					}
					Surface surface = GetSurface(bodyChunk.pos);
					float num3 = DetailedWaterLevel(bodyChunk.pos);
					if ((bodyChunk.vel.y < -3f && bodyChunk.lastPos.y > num3 && bodyChunk.pos.y <= num3) || (bodyChunk.vel.y > 3f && bodyChunk.lastPos.y < num3 && bodyChunk.pos.y >= num3))
					{
						float num4 = Mathf.Lerp(bodyChunk.vel.y * bodyChunk.rad * Mathf.Lerp(bodyChunk.mass, 1f, 0.3f) / 3f, 10f, 0.5f);
						if (Mathf.Abs(num4) > Mathf.Abs(bodyChunk.vel.y))
						{
							num4 = bodyChunk.vel.y;
						}
						if (bodyChunk.splashStop == 0)
						{
							bodyChunk.splashStop = 10;
							int num5 = surface.ClosestPoint(bodyChunk.pos.x);
							surface.points[num5, 0].height += num4 / 2f * (1f - viscosity);
							if (num5 > 0)
							{
								surface.points[num5 - 1, 0].height += num4 / 6f * (1f - viscosity);
							}
							if (num5 < surface.totalPoints - 1)
							{
								surface.points[num5 + 1, 0].height += num4 / 6f * (1f - viscosity);
							}
							ThroughSurfaceSound(bodyChunk);
						}
						if (Mathf.Abs(num4) > 5f)
						{
							Splash splash = new Splash();
							room.AddObject(splash);
							splash.Reset(new Vector2(bodyChunk.pos.x, num3 + ((num4 > 0f) ? 20f : (-20f))), new Vector2(bodyChunk.vel.x * 0.5f, Custom.LerpMap(Mathf.Abs(num4), 10f, 40f, 4f, 14f)), Custom.LerpMap(Mathf.Abs(bodyChunk.vel.y) * bodyChunk.rad * bodyChunk.mass, 4f, 300f, 7f, 90f), bodyChunk.rad);
							num2++;
						}
						if (num2 >= 10)
						{
							continue;
						}
						for (int k = 0; k < (int)Mathf.Abs(num4 * ((num4 < 0f) ? 0.25f : 0.55f)); k++)
						{
							Splash splash = new Splash();
							room.AddObject(splash);
							splash.Reset(new Vector2(bodyChunk.pos.x + Mathf.Lerp(0f - bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value), num3 + 5f), new Vector2(bodyChunk.vel.x, 0f) + Custom.DegToVec(-45f + 90f * UnityEngine.Random.value) * (UnityEngine.Random.value * (Mathf.Abs(bodyChunk.vel.y) * ((num4 > 0f) ? 0.4f : 0.3f) + 10f)), Custom.LerpMap(Mathf.Abs(bodyChunk.vel.y) * bodyChunk.rad * bodyChunk.mass, 7f, 200f, 2f, 10f), bodyChunk.rad);
							num2++;
						}
						if (bodyChunk.vel.y < 0f && bubbleEmitters.Count < 10)
						{
							bubbleEmitters.Add(new BubbleEmitter(this, bodyChunk, Mathf.Clamp((1f + bodyChunk.vel.magnitude) * Mathf.Lerp(Mathf.Abs(num4), 5f, 0.75f) * 0.2f, 3f, 25f), 0.4f));
							for (int l = 0; l < (int)Mathf.Abs(num4 * 0.75f); l++)
							{
								room.AddObject(new Bubble(new Vector2(bodyChunk.pos.x + Mathf.Lerp(0f - bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value), num3 - 5f), Custom.DegToVec(135f + 90f * UnityEngine.Random.value) * (UnityEngine.Random.value * (Mathf.Abs(num4 * 1.5f) + 10f)), bottomBubble: false, fakeWaterBubble: false));
								num2++;
							}
						}
					}
					else if (bodyChunk.pos.y - bodyChunk.rad < num3 && bodyChunk.pos.y + bodyChunk.rad > num3)
					{
						int num6 = surface.PreviousPoint(bodyChunk.pos.x);
						int num7 = Custom.IntClamp(num6 + 1, 0, surface.totalPoints - 1);
						surface.points[num6, 0].height -= bodyChunk.vel.x * bodyChunk.rad * 0.03f * (1f - viscosity);
						surface.points[num7, 0].height += bodyChunk.vel.x * bodyChunk.rad * 0.03f * (1f - viscosity);
						if (Mathf.Abs(bodyChunk.vel.y) > 2f)
						{
							surface.points[num6, 0].height += bodyChunk.vel.y * bodyChunk.rad * 0.01f * (1f - viscosity);
							surface.points[num7, 0].height += bodyChunk.vel.y * bodyChunk.rad * 0.01f * (1f - viscosity);
						}
					}
				}
			}
		}
		for (int num8 = bubbleEmitters.Count - 1; num8 >= 0; num8--)
		{
			if (bubbleEmitters[num8].amount <= 0f)
			{
				bubbleEmitters.RemoveAt(num8);
			}
			else
			{
				bubbleEmitters[num8].Update();
			}
		}
		for (int num9 = rippleWaves.Count - 1; num9 >= 0; num9--)
		{
			if (rippleWaves[num9].life < 0f)
			{
				rippleWaves.RemoveAt(num9);
			}
			else
			{
				rippleWaves[num9].Update();
			}
		}
		Surface[] array = surfaces;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Update();
		}
		lastFWaterLevel = fWaterLevel;
	}

	public Surface GetSurface(Vector2 pos)
	{
		for (int i = 1; i < surfaces.Length; i++)
		{
			if (surfaces[i].bounds.Contains(pos))
			{
				return surfaces[i];
			}
		}
		return MainSurface;
	}

	public float MultiSurfaceFWaterLevel(Vector2 pos)
	{
		return GetSurface(pos).TargetWaterLevel(pos.x);
	}

	public void ThroughSurfaceSound(BodyChunk chunk)
	{
		float num = Mathf.Sin(Mathf.InverseLerp(-1f, -50f, chunk.vel.y));
		float num2 = Mathf.InverseLerp(-30f, -60f, chunk.vel.y);
		if (!(chunk.vel.y < -1f))
		{
			return;
		}
		if (chunk.mass < 0.1f)
		{
			room.PlaySound(SoundID.Small_Object_Into_Water_Fast, chunk, loop: false, num2, 1f);
			room.PlaySound(SoundID.Small_Object_Into_Water_Slow, chunk, loop: false, num, 1f);
			if (ModManager.MMF)
			{
				room.InGameNoise(new InGameNoise(chunk.pos, 250f, chunk.owner, num2));
			}
		}
		else if (chunk.mass < 0.8f)
		{
			room.PlaySound(SoundID.Medium_Object_Into_Water_Fast, chunk, loop: false, num2, 1f);
			room.PlaySound(SoundID.Medium_Object_Into_Water_Slow, chunk, loop: false, num, 1f);
			if (ModManager.MMF)
			{
				room.InGameNoise(new InGameNoise(chunk.pos, 450f, chunk.owner, num2 * 1.2f + num));
			}
		}
		else
		{
			room.PlaySound(SoundID.Large_Object_Into_Water_Fast, chunk, loop: false, num2, 1f);
			room.PlaySound(SoundID.Large_Object_Into_Water_Slow, chunk, loop: false, num, 1f);
			if (ModManager.MMF)
			{
				room.InGameNoise(new InGameNoise(chunk.pos, 850f, chunk.owner, num2 * 2f + num));
			}
		}
	}

	public Vector2 SurfaceLeftAndRightBoundries()
	{
		return new Vector2(MainSurface.points[0, 0].defaultPos.x, MainSurface.points[MainSurface.totalPoints - 1, 0].defaultPos.x);
	}

	public float DetailedWaterLevel(Vector2 pos)
	{
		return GetSurface(pos).CurrentWaterLevel(pos.x);
	}

	public bool IsTileAccessible(IntVector2 tile, CreatureTemplate crit)
	{
		if (!WaterIsLethal && !WaterIsBrine)
		{
			return true;
		}
		return !room.GetTile(tile).AnyWater && !room.GetTile(tile + new IntVector2(0, -1)).AnyWater;
	}

	public Vector2 shiftWithInversion(Vector2 shift)
	{
		if (ModManager.DLCShared && room.waterInverted)
		{
			return new Vector2(shift.x, 0f - shift.y);
		}
		return shift;
	}

	public int FindFirstVisibleTriangle(RoomCamera rCam)
	{
		for (int i = 0; i < MainSurface.totalPoints; i++)
		{
			if (rCam.IsVisibleAtCameraPosition(rCam.currentCameraPosition, MainSurface.points[i, 0].defaultPos) && i > 0)
			{
				return i - 1;
			}
		}
		return 0;
	}

	public void WaterfallHitSurface(Vector2 sourcePos, float width, float flow)
	{
		GetSurface(sourcePos).WaterfallHitSurface(sourcePos.x - width / 2f, sourcePos.x + width / 2f, flow);
	}

	public void DrainAffectSurface(Vector2 sourcePos, float width, float flow)
	{
		GetSurface(sourcePos).DrainAffectSurface(sourcePos.x - width / 2f, sourcePos.x + width / 2f, flow);
	}

	public void GeneralUpsetSurface(float intensity)
	{
		Surface[] array = surfaces;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GeneralUpsetSurface(intensity);
		}
	}

	public void Explosion(Vector2 pos, float rad, float frc)
	{
		Surface[] array = surfaces;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Explosion(pos, rad, frc);
		}
		rippleWaves.Add(new RippleWave(this, pos, rad / 4f, 17f, 70f, 20f, frc * 3f));
	}

	public void Explosion(Explosion explosion)
	{
		Surface[] array = surfaces;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Explosion(explosion);
		}
		rippleWaves.Add(new RippleWave(this, explosion.pos, explosion.rad / 4f, 17f, 70f, 20f, explosion.force * 3f));
	}

	public void Ripple(RippleWave wave)
	{
		Surface[] array = surfaces;
		foreach (Surface surface in array)
		{
			for (int j = 0; j < surface.totalPoints; j++)
			{
				float num = Mathf.InverseLerp(wave.width, wave.width / 3f, Mathf.Abs(Vector2.Distance(surface.points[j, 0].RoomPos, wave.pos) - wave.rad));
				num *= Mathf.InverseLerp(0f, 0.5f, wave.life);
				surface.points[j, 0].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * (num * wave.intensity * Mathf.Pow(1f - viscosity, 5f));
				surface.points[j, 0].height += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.1f * num * wave.intensity * (1f - viscosity);
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[surfaces.Length * 2];
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[pointsToRender * 2];
		for (int i = 0; i < pointsToRender; i++)
		{
			int num = i * 2;
			array[num] = new TriangleMesh.Triangle(num, num + 1, num + 2);
			array[num + 1] = new TriangleMesh.Triangle(num + 1, num + 2, num + 3);
		}
		bool flag = (ModManager.DLCShared && room.roomSettings.DangerType == DLCSharedEnums.RoomRainDangerType.Blizzard) || (ModManager.MSC && Region.IsRubiconRegion(room.world.name)) || room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.LavaSurface) != null;
		sLeaser.sprites[0] = new WaterTriangleMesh("Futile_White", array, customColor: true);
		sLeaser.sprites[0].shader = room.game.rainWorld.Shaders[flag ? "WaterSlush" : "WaterSurface"];
		TriangleMesh.Triangle[] array2 = new TriangleMesh.Triangle[pointsToRender * 2];
		for (int j = 0; j < pointsToRender; j++)
		{
			int num2 = j * 2;
			array2[num2] = new TriangleMesh.Triangle(num2, num2 + 1, num2 + 2);
			array2[num2 + 1] = new TriangleMesh.Triangle(num2 + 1, num2 + 2, num2 + 3);
		}
		sLeaser.sprites[1] = new WaterTriangleMesh("Futile_White", array2, customColor: false);
		sLeaser.sprites[1].color = new Color(0f, 0f, 0f);
		sLeaser.sprites[1].shader = room.game.rainWorld.Shaders["DeepWater"];
		firstWaterSprite = sLeaser.sprites[0];
		for (int k = 1; k < surfaces.Length; k++)
		{
			TriangleMesh.Triangle[] array3 = new TriangleMesh.Triangle[(Mathf.Min(surfaces[k].points.Length, pointsToRender) - 1) * 2];
			for (int l = 0; l < array3.Length; l += 2)
			{
				array3[l] = new TriangleMesh.Triangle(l, l + 1, l + 3);
				array3[l + 1] = new TriangleMesh.Triangle(l, l + 3, l + 2);
			}
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", array3, customColor: true)
			{
				shader = room.game.rainWorld.Shaders[flag ? "WaterSlush" : "WaterSurface"]
			};
			sLeaser.sprites[k * 2] = triangleMesh;
			for (int m = 0; m < triangleMesh.UVvertices.Length; m += 2)
			{
				triangleMesh.UVvertices[m] = new Vector2(0f, 0f);
				triangleMesh.UVvertices[m + 1] = new Vector2(1f, 1f);
			}
			triangleMesh.alpha = 1f - (float)k / (float)(surfaces.Length - 1);
			triangleMesh = new TriangleMesh("Futile_White", array3, customColor: false)
			{
				shader = room.game.rainWorld.Shaders["DeepWater"]
			};
			sLeaser.sprites[k * 2 + 1] = triangleMesh;
			for (int n = 0; n < triangleMesh.UVvertices.Length; n += 2)
			{
				triangleMesh.UVvertices[n] = new Vector2(0f, 0f);
				triangleMesh.UVvertices[n + 1] = new Vector2(1f, 1f);
			}
			triangleMesh.alpha = 1f - (float)k / (float)(surfaces.Length - 1);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[1].isVisible = !rCam.voidSeaMode;
		if (rCam.voidSeaMode)
		{
			return;
		}
		float num = Mathf.Lerp(lastFWaterLevel, fWaterLevel, timeStacker);
		UpdateAirPockets(rCam, timeStacker, camPos);
		Shader.SetGlobalVectorArray("_airPockets", airPockets);
		float num2 = -10f;
		if (cosmeticLowerBorder > -1f)
		{
			num2 = cosmeticLowerBorder - camPos.y;
		}
		if (ModManager.DLCShared && room.waterInverted)
		{
			num2 = (float)room.TileHeight * 22f;
		}
		int num3 = Custom.IntClamp(MainSurface.PreviousPoint(camPos.x - 30f), 0, MainSurface.totalPoints - 1);
		int num4 = Custom.IntClamp(num3 + pointsToRender, 0, MainSurface.totalPoints - 1);
		for (int i = num3; i < num4; i++)
		{
			int num5 = (i - num3) * 2;
			Vector2 v = MainSurface.points[i, 0].DrawPos(timeStacker) - camPos + new Vector2(0f, cosmeticSurfaceDisplace);
			Vector2 v2 = MainSurface.points[i, 1].DrawPos(timeStacker) - camPos + new Vector2(0f, cosmeticSurfaceDisplace);
			Vector2 v3 = MainSurface.points[i + 1, 0].DrawPos(timeStacker) - camPos + new Vector2(0f, cosmeticSurfaceDisplace);
			Vector2 v4 = MainSurface.points[i + 1, 1].DrawPos(timeStacker) - camPos + new Vector2(0f, cosmeticSurfaceDisplace);
			v = Custom.ApplyDepthOnVector(v, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), -10f);
			v2 = Custom.ApplyDepthOnVector(v2, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), 30f);
			v3 = Custom.ApplyDepthOnVector(v3, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), -10f);
			v4 = Custom.ApplyDepthOnVector(v4, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), 30f);
			if (i == num3)
			{
				v2.x -= 100f;
			}
			else if (i == num4 - 1)
			{
				v2.x += 100f;
			}
			Vector2 vector = Vector2.zero;
			if (ModManager.DLCShared && room.waterInverted)
			{
				vector = new Vector2(0f, -40f);
			}
			(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(num5, v);
			(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(num5 + 1, v2 + vector);
			(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(num5 + 2, v3);
			(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(num5 + 3, ModManager.DLCShared ? (v4 + vector) : v3);
			Vector2 vector2 = MainSurface.points[Mathf.Max(i - 1, 0), 0].DrawPos(timeStacker);
			Vector2 normalized = (MainSurface.points[i + 1, 0].DrawPos(timeStacker) - vector2).normalized;
			Color surfaceColor = GetSurfaceColor(rCam, MainSurface.points[i, 0].DrawPos(timeStacker), normalized, timeStacker);
			float num6 = rCam.room.WaterShinyness(MainSurface.points[i, 0].DrawPos(timeStacker), timeStacker);
			(sLeaser.sprites[0] as WaterTriangleMesh).verticeColors[num5] = surfaceColor;
			(sLeaser.sprites[0] as WaterTriangleMesh).verticeColors[num5 + 1] = Color.Lerp(palette.waterSurfaceColor2, palette.waterShineColor, num6 * (1f - palette.fogAmount));
			float num7 = ((surfaces.Length == 1 && MainSurface.waterCutoffs.Count == 0) ? 0.3f : 0f);
			(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num5, new Vector2(v.x + v.y * num7, num2));
			(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num5 + 1, v);
			(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num5 + 2, new Vector2(v3.x + v3.y * num7, num2));
			(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num5 + 3, v3);
		}
		if (ModManager.DLCShared)
		{
			for (int j = (num4 - num3) * 2; j < (sLeaser.sprites[0] as WaterTriangleMesh).vertices.Length; j++)
			{
				(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(j, new Vector2(3400f, num - camPos.y + cosmeticSurfaceDisplace));
				(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(j, new Vector2(3400f, num - camPos.y + cosmeticSurfaceDisplace));
			}
		}
		(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(0, new Vector2(-10f, num2));
		(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(1, new Vector2(-10f, num - camPos.y + cosmeticSurfaceDisplace));
		(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice((sLeaser.sprites[1] as WaterTriangleMesh).vertices.Length - 2, new Vector2(ModManager.DLCShared ? 3400f : 1400f, num - camPos.y + cosmeticSurfaceDisplace));
		(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice((sLeaser.sprites[1] as WaterTriangleMesh).vertices.Length - 1, new Vector2(ModManager.DLCShared ? 3400f : 1400f, num2));
		if (rCam.followAbstractCreature != null && rCam.followAbstractCreature.realizedCreature != null && rCam.followAbstractCreature.realizedCreature is Player && rCam.followAbstractCreature.realizedCreature.room == room && !rCam.followAbstractCreature.realizedCreature.dead)
		{
			Player player = rCam.followAbstractCreature.realizedCreature as Player;
			float t = Mathf.Lerp(player.outOfBreathFac[1], player.outOfBreathFac[0], timeStacker);
			float b = Mathf.Lerp(1f, 0.75f + 0.25f * Mathf.Sin(Mathf.Lerp(player.outOfBreathFac[3], player.outOfBreathFac[2], timeStacker) * 2f), t);
			Vector2 vector3 = Vector2.Lerp(rCam.followAbstractCreature.realizedCreature.mainBodyChunk.lastPos, rCam.followAbstractCreature.realizedCreature.mainBodyChunk.pos, timeStacker) - camPos;
			if (ModManager.DLCShared && room.roomSettings.GetEffect(DLCSharedEnums.RoomEffectType.RoomWrap) != null)
			{
				(sLeaser.sprites[1] as WaterTriangleMesh).color = new Color(vector3.x / rCam.sSize.x, vector3.y / rCam.sSize.y, b);
			}
			else
			{
				(sLeaser.sprites[1] as WaterTriangleMesh).color = new Color(Mathf.InverseLerp(0f, rCam.sSize.x, vector3.x), Mathf.InverseLerp(0f, rCam.sSize.y, vector3.y), b);
			}
		}
		else
		{
			(sLeaser.sprites[1] as WaterTriangleMesh).color = new Color(0f, 0f, 0f);
		}
		(sLeaser.sprites[0] as WaterTriangleMesh).verticeColors[(sLeaser.sprites[0] as WaterTriangleMesh).verticeColors.Length - 2] = palette.waterSurfaceColor1;
		for (int k = 1; k < surfaces.Length; k++)
		{
			Surface surface = surfaces[k];
			TriangleMesh triangleMesh = sLeaser.sprites[k * 2] as TriangleMesh;
			TriangleMesh triangleMesh2 = sLeaser.sprites[k * 2 + 1] as TriangleMesh;
			int num8 = Mathf.Clamp(surface.PreviousPoint(camPos.x - 30f), 0, surface.totalPoints - 1);
			for (int l = 0; l < triangleMesh.vertices.Length; l += 2)
			{
				int num9 = Mathf.Clamp(l / 2 + num8, 0, surface.totalPoints - 1);
				Vector2 targetPoint = surface.points[num9, 0].DrawPos(timeStacker);
				Vector2 targetPoint2 = surface.points[num9, 1].DrawPos(timeStacker);
				targetPoint = surface.bounds.GetClosestInteriorPoint(targetPoint);
				targetPoint2 = surface.bounds.GetClosestInteriorPoint(targetPoint2);
				Vector2 vector4 = new Vector2(targetPoint.x, MainSurface.CurrentWaterLevel(targetPoint.x));
				if (num9 == 0)
				{
					targetPoint.x = (targetPoint2.x = surface.bounds.xMin);
				}
				else if (num9 == surface.totalPoints - 1)
				{
					targetPoint.x = (targetPoint2.x = surface.bounds.xMax);
				}
				Vector2 depthPoint = new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f));
				Vector2 pos = Custom.ApplyDepthOnVector(targetPoint - camPos, depthPoint, -10f);
				Vector2 vector5 = Custom.ApplyDepthOnVector(targetPoint2 - camPos, depthPoint, 30f);
				Vector2 vector6 = Custom.ApplyDepthOnVector(vector4 - camPos, depthPoint, -10f);
				float num10 = Mathf.Max(Custom.ApplyDepthOnVector(new Vector2(0f, surface.bounds.yMin) - camPos, depthPoint, -10f).y, num2);
				Vector2 vector7 = Vector2.zero;
				if (ModManager.DLCShared && room.waterInverted)
				{
					vector7 = new Vector2(0f, -40f);
				}
				Vector2 vector8 = surface.points[Mathf.Max(num9 - 1, 0), 0].DrawPos(timeStacker);
				Vector2 normalized2 = (surface.points[Mathf.Min(num9 + 1, surface.totalPoints - 1), 0].DrawPos(timeStacker) - vector8).normalized;
				float num11 = rCam.room.WaterShinyness(targetPoint, timeStacker);
				Color surfaceColor2 = GetSurfaceColor(rCam, targetPoint, normalized2, timeStacker);
				surfaceColor2.a = triangleMesh.verticeColors[l].a;
				Color color = Color.Lerp(palette.waterSurfaceColor2, palette.waterShineColor, num11 * (1f - palette.fogAmount));
				color.a = triangleMesh.verticeColors[l + 1].a;
				triangleMesh.MoveVertice(l, pos);
				triangleMesh.MoveVertice(l + 1, vector5 + vector7);
				triangleMesh.verticeColors[l] = surfaceColor2;
				triangleMesh.verticeColors[l + 1] = color;
				triangleMesh2.MoveVertice(l, new Vector2(pos.x, num10));
				triangleMesh2.MoveVertice(l + 1, pos);
				if (num2 < vector6.y)
				{
					triangleMesh2.UVvertices[l].y = Mathf.InverseLerp(num2, vector6.y, num10);
					triangleMesh2.UVvertices[l + 1].y = Mathf.InverseLerp(num2, vector6.y, pos.y);
				}
				else
				{
					triangleMesh2.UVvertices[l].y = 1f;
					triangleMesh2.UVvertices[l + 1].y = 1f;
				}
			}
			triangleMesh2.color = sLeaser.sprites[1].color;
		}
		if (slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public Color GetSurfaceColor(RoomCamera rCam, Vector2 point, Vector2 tangent, float timeStacker)
	{
		float num = rCam.room.WaterShinyness(point, timeStacker);
		float value = Vector2.Dot(tangent, Custom.DegToVec(60f));
		num = Mathf.Pow(num, 0.1f) * Mathf.InverseLerp(0.9f - num * 0.1f, 0.98f - num * 0.05f, value);
		Color color = Color.Lerp(palette.waterSurfaceColor1, palette.waterShineColor, num);
		float num2 = room.Darkness(point);
		if (num2 > 0f)
		{
			for (int i = 0; i < 2; i++)
			{
				foreach (LightSource item in (i == 0) ? room.lightSources : room.cosmeticLightSources)
				{
					float num3 = Mathf.InverseLerp(point.y + 500f, point.y, item.Pos.y);
					if (item.Pos.y < point.y)
					{
						num3 *= Mathf.InverseLerp(point.y - item.Rad * 0.7f, point.y, item.Pos.y);
					}
					Color b = item.color * item.Alpha * num3 * Mathf.InverseLerp(10f + 160f * num3, 10f, Mathf.Abs(Custom.DistanceToLine(item.Pos, point, point + Custom.PerpendicularVector(tangent) + new Vector2(0f, 1f - num3)))) * num2 * Mathf.InverseLerp(0.1f, 0.3f, tangent.x);
					color = Custom.Screen(color, b);
				}
			}
		}
		return color;
	}

	public void UpdateAirPockets(RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Rect other = new Rect(Vector2.zero, rCam.sSize);
		int i = 0;
		for (int j = 1; j < surfaces.Length; j++)
		{
			if (i >= airPockets.Length)
			{
				break;
			}
			Rect rect = surfaces[j].bounds.CloneWithExpansion(-0.5f);
			rect.position -= camPos;
			rect = Custom.ApplyDepthOnRect(rect, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), -10f);
			if (rect.Overlaps(other))
			{
				airPockets[i++] = new Vector4(rect.xMin / rCam.sSize.x, rect.yMin / rCam.sSize.y, rect.xMax / rCam.sSize.x, rect.yMax / rCam.sSize.y);
			}
		}
		for (; i < airPockets.Length; i++)
		{
			airPockets[i] = default(Vector4);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		this.palette = palette;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[i]);
		}
	}

	public void Destroy()
	{
		slatedForDeletetion = true;
	}

	public void AddBrineAmbientSounds()
	{
		WaterSurfaceSoundSpawner.SpawnParameters spawnParameters = new WaterSurfaceSoundSpawner.SpawnParameters();
		spawnParameters.sounds = new string[1] { "SO_WAT-LightFlow.ogg" };
		spawnParameters.radius = 1f;
		spawnParameters.taper = 0.3f;
		spawnParameters.pitch = 1.99f;
		spawnParameters.volume = (1f - room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SilenceWater)) * 0.9f;
		spawnParameters.pitchVariation = 0.05f;
		spawnParameters.doppler = 0.6f;
		WaterSurfaceSoundSpawner.SpawnParameters spawnParameters2 = spawnParameters;
		room.AddObject(new WaterSurfaceSoundSpawner(spawnParameters2, room));
	}

	public void SpawnSurfaceFizz(int amount, RoomPalette.ColorName color)
	{
		Vector2 pos = room.game.cameras[0].pos;
		if ((float)(room.defaultWaterLevel + 1) * 20f < pos.y || (float)room.defaultWaterLevel * 20f > pos.y + 768f)
		{
			return;
		}
		for (int i = 0; i < amount; i++)
		{
			float num = pos.x + (float)UnityEngine.Random.Range(0, Futile.screen.pixelWidth);
			Vector2 pos2 = new Vector2(num, MainSurface.CurrentWaterLevel(num));
			if (pos2.x > 0f && pos2.x < room.PixelWidth && room.GetTile(pos2).IsSolid())
			{
				break;
			}
			room.AddObject(new Spark(pos2, (Vector2.up * 2f + Custom.RNV() * 1f) * (0.5f + 3f * UnityEngine.Random.value), room.game.cameras[0].currentPalette.GetColor(color), null, 10, 15));
		}
	}

	public float AlternateSurfaceFloodLevel(float level, float floodRate)
	{
		if ((room.roomSettings.DangerType == RoomRain.DangerType.Flood || room.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain) && (room.world.rainCycle.TimeUntilRain <= 0 || room.roomRain.globalRain.drainWorldFlood != 0f))
		{
			if (ModManager.PrecycleModule)
			{
				float num = room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index).y + level;
				float flood = room.roomRain.globalRain.flood;
				level += Mathf.Max(flood - num, 0f) * floodRate;
			}
			else
			{
				level += room.roomRain.globalRain.flood * floodRate;
			}
		}
		return level;
	}
}
