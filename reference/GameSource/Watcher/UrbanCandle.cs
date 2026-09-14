using System.Globalization;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class UrbanCandle : CosmeticSprite, INotifyWhenRoomUnloaded
{
	public struct UrbanCandleData
	{
		public Vector2 pos;

		public float depth;

		public float height;

		public float width;

		public float rotation;

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", pos.x, pos.y, depth, height, width, rotation);
		}
	}

	public DynamicLevelElement candle;

	public float height;

	public float width;

	public float rotation;

	public float lastHeight;

	public float depth;

	public static bool debug;

	public float agitation;

	public float lastAgitation;

	public Vector4 variation;

	public Vector2 flamePos;

	public Vector2 lastFlamePos;

	public float time;

	public float burnRate = 0.01f;

	public float targetBurnRate;

	public Vector2 lightWobble;

	public Vector2 lastLightWobble;

	public Counter smokeTimer = new Counter(Random.Range(200, 400), 0, countsUp: true);

	public Counter deathTimer = new Counter(1000);

	public float lastFlameIntensity;

	public float flameIntensity;

	public Mesh urbanCandleMesh;

	public bool isLit => flameIntensity > 0.1f;

	public float clampedHeight => Mathf.Max(deathTimer.normalized * 0.2f, height);

	public Vector2 getFlamePos => pos + Custom.rotateVectorDeg(Vector2.up, rotation) * clampedHeight * 16f;

	public float getFlameIntensity => Mathf.InverseLerp(0f, 1f, height);

	public UrbanCandle(Room room, UrbanCandleData data)
	{
		base.room = room;
		pos = data.pos;
		lastPos = pos;
		depth = data.depth;
		height = data.height;
		if (room.game.IsStorySession && room.game.GetStorySession.saveState.deathPersistentSaveData.sawVoidBathSlideshow)
		{
			height = 0f;
			burnRate = 10f;
		}
		lastHeight = height;
		width = data.width;
		rotation = data.rotation;
		flamePos = getFlamePos;
		lastFlamePos = getFlamePos;
		agitation = 1f;
		targetBurnRate = burnRate;
		lastAgitation = agitation;
		flameIntensity = getFlameIntensity;
		variation = new Vector4(Random.value, Random.value, Random.value, Random.value);
		urbanCandleMesh = new Mesh
		{
			name = "UrbanCandle Mesh"
		};
		Vector3[] vertices = MaskSource.quadVerts.Select((Vector3 v) => v - Vector3.down * 0.5f).ToArray();
		urbanCandleMesh.vertices = vertices;
		urbanCandleMesh.triangles = MaskSource.quadTris;
		urbanCandleMesh.uv = MaskSource.quadUV;
		urbanCandleMesh.RecalculateBounds();
		candle = new DynamicLevelElement(pos, new Vector2(width, height), null, 0, urbanCandleMesh, "DynamicLevelUrbanCandle")
		{
			rotation = rotation,
			dontSetProps = true,
			dontDrawUpdate = true
		};
		room.AddObject(candle);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (deathTimer.isFinished)
		{
			Destroy();
		}
		lastFlamePos = flamePos;
		flamePos = getFlamePos;
		lastHeight = clampedHeight;
		lastAgitation = agitation;
		lastFlameIntensity = flameIntensity;
		lastLightWobble = lightWobble;
		burnRate = Mathf.Lerp(burnRate, targetBurnRate, 0.05f);
		height -= (0.01f + variation.x * 0.01f) * (0.1f + lastFlameIntensity * 0.9f) * burnRate;
		flameIntensity = getFlameIntensity;
		float num = 0.1f + agitation * 0.9f;
		time += 0.02f * num;
		lightWobble = new Vector2(FlameJet.AnimatePerlin(time + variation.x * 11f) * 2f - 1f, FlameJet.AnimatePerlin(time + variation.y * 11f) * 2f - 1f) * 0.1f;
		lightWobble *= num;
		if (!isLit)
		{
			smokeTimer.Tick();
		}
		if (smokeTimer.isFinished)
		{
			deathTimer.Tick();
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		candle.Destroy();
		if (urbanCandleMesh != null)
		{
			Object.Destroy(urbanCandleMesh);
			urbanCandleMesh = null;
		}
	}

	public static void SetRoomTargetBurnRate(Room room, float burnRate)
	{
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (room.updateList[i] is UrbanCandle urbanCandle)
			{
				urbanCandle.targetBurnRate = burnRate;
			}
		}
	}

	public static float? AdaptDepth(int depth, bool removePlayerPlane = true)
	{
		depth = Mathf.Max(30 - depth, 0);
		if (removePlayerPlane && depth >= 24 && depth <= 26)
		{
			return null;
		}
		return (float)depth / 30f;
	}

	public static Vector2? FindPos(Room r, Vector2 p, float candleDepth, float height, float rotation, int stepsPastPoint = 20)
	{
		DebugPoint(r, p, 0.9f, 45f, Color.yellow);
		int num = (int)(height * 8f) + 20;
		bool flag = false;
		Vector2 vector = Custom.rotateVectorDeg(Vector2.down * 2f, rotation);
		Counter counter = new Counter(num);
		p -= vector * num;
		num += stepsPastPoint;
		Counter counter2 = new Counter(5);
		Vector2 p2 = ApplyDepth(r, p, candleDepth);
		DebugPoint(r, p2, 1f, 0f, Color.magenta);
		for (int i = 0; i < num; i++)
		{
			bool flag2 = false;
			(flag, flag2) = IsPosSolid(r, p2, candleDepth);
			if (flag && counter.isFinished)
			{
				DebugPoint(r, p2, 1f, 0f, Color.green);
				break;
			}
			if (flag2)
			{
				DebugPoint(r, p2, 1f, 0f, Color.red);
				counter2.Tick();
				if (counter2.isFinished)
				{
					counter.Reset();
					counter2.Reset();
				}
			}
			DebugPoint(r, p2, 0.5f, 0f, counter.isFinished ? Color.white : Color.blue);
			p2 += vector;
			p += vector;
			counter.Tick();
		}
		if (flag && (int)counter <= 0)
		{
			return p + vector * 5f;
		}
		return null;
	}

	public static void DebugPoint(Room r, Vector2 p, float scale, float rotation, Color color)
	{
		if (debug)
		{
			color.a = 0.3f;
			scale *= 0.3f;
			r.AddObject(new DebugSprite(p, new FSprite("Futile_White")
			{
				scale = scale,
				rotation = rotation,
				color = color
			}, r));
		}
	}

	public static (bool, bool) IsPosSolid(Room r, Vector2 p, float candleDepth)
	{
		float num = 1f - r.game.cameras[0].DepthAtCoordinate(p);
		if (num == 0f)
		{
			num = -1f;
		}
		num = Mathf.Abs(num - candleDepth);
		return (num < 0.0333f, (double)num < 0.0999000035226345);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3];
		float num = 1f - (1f - depth) * 0.1f;
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["UrbanCandleSSS"];
		sLeaser.sprites[0].scaleX = width * num;
		sLeaser.sprites[0].scaleY = height * num;
		sLeaser.sprites[0].anchorY = 0f;
		sLeaser.sprites[0].rotation = rotation;
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].shader = Custom.rainWorld.Shaders["UrbanCandleFlame"];
		sLeaser.sprites[1].scaleX = 0.75f * num;
		sLeaser.sprites[1].scaleY = 1.75f * num;
		sLeaser.sprites[1].anchorY = 0.1f;
		sLeaser.sprites[1].color = new Color(depth, variation.z, variation.w, 0f);
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = Custom.rainWorld.Shaders["DeepLightSource"];
		AddToContainer(sLeaser, rCam, null);
	}

	public static Vector2 ApplyDepth(Room room, Vector2 pos, float depth)
	{
		return room.game.cameras[0].ApplyDepth(pos, (1f - depth) * 30f - 5f);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (sLeaser.deleteMeNextFrame)
		{
			return;
		}
		float num = 1f - (1f - depth) * 0.1f;
		Vector2 vector = ApplyDepth(room, Vector2.Lerp(lastPos, pos, timeStacker), depth);
		Vector2 vector2 = ApplyDepth(room, Vector2.Lerp(lastFlamePos, flamePos, timeStacker), depth);
		float num2 = Mathf.Lerp(lastHeight, clampedHeight, timeStacker);
		if (candle.element != null)
		{
			candle.element.SetProperty(0, depth);
			candle.element.SetProperty(1, variation.x);
			candle.element.SetProperty(2, variation.y);
			candle.element.SetProperty(3, width / num2);
			candle.element.ApplyVertexColor();
			candle.element.setGameObjectPos = vector - camPos;
			candle.element.setGameObjectRotation = Vector3.back * rotation;
			candle.element.setGameObjectScale = new Vector3(width, num2, 1f) * num;
		}
		if (smokeTimer.isFinished || !rCam.PositionCurrentlyVisible(vector, 200f, widescreen: true))
		{
			sLeaser.sprites[0].isVisible = false;
			sLeaser.sprites[1].isVisible = false;
			sLeaser.sprites[2].isVisible = false;
			return;
		}
		sLeaser.sprites[1].isVisible = true;
		sLeaser.sprites[0].isVisible = true;
		sLeaser.sprites[0].SetPosition(vector - camPos);
		sLeaser.sprites[0].scaleX = width * num;
		sLeaser.sprites[0].scaleY = num2 * num;
		Color color = sLeaser.sprites[0].color;
		color.r = depth;
		color.g = variation.x;
		color.b = variation.y;
		color.a = width / num2;
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[1].SetPosition(vector2 - camPos);
		float num3 = Mathf.Lerp(lastFlameIntensity, flameIntensity, timeStacker);
		float num4 = Custom.LerpQuadEaseIn(1f, 0f, 1f - num3);
		Vector2 vector3 = Vector2.Lerp(lastLightWobble, lightWobble, timeStacker);
		float num5 = Mathf.Clamp(width * 2f, 0.5f, 1f);
		if (isLit)
		{
			sLeaser.sprites[1].scaleX = 0.75f * num * num4 * num5;
			sLeaser.sprites[1].scaleY = 1.75f * num * num4 * num5;
			Color color2 = sLeaser.sprites[1].color;
			color2.a = agitation;
			color2.r = depth;
			sLeaser.sprites[1].rotation = vector3.x * 10f;
			sLeaser.sprites[1].color = color2;
		}
		else
		{
			float perc = 1f - Mathf.Abs(smokeTimer.normalized - 0.5f) * 2f;
			perc = Custom.LerpQuadEaseInOut(0f, 1f, perc);
			sLeaser.sprites[1].scaleY = 0.5f * perc;
			sLeaser.sprites[1].scaleX = (6f + variation.y * 3f) * perc;
			sLeaser.sprites[1].color = new Color(0f, 0f, variation.x, perc * 0.4f);
			sLeaser.sprites[1].anchorY = 0.5f;
			sLeaser.sprites[1].anchorX = 0f;
			sLeaser.sprites[1].rotation = -90f;
		}
		sLeaser.sprites[1].shader = Custom.rainWorld.Shaders[isLit ? "UrbanCandleFlame" : "FlameJet"];
		sLeaser.sprites[1].isVisible = !smokeTimer.isFinished;
		sLeaser.sprites[2].SetPosition(vector2 - camPos + Vector2.up * (sLeaser.sprites[1].scaleY * 8f) + vector3 * 15f);
		sLeaser.sprites[2].color = new Color(0.05f, 0.5f, num4 * 0.56f + vector3.y, 1f - depth);
		sLeaser.sprites[2].scale = (5f + 7f * num4) * num5;
		sLeaser.sprites[2].isVisible = isLit;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[1]);
		rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[2]);
	}

	public void RoomUnloaded()
	{
		Destroy();
	}
}
