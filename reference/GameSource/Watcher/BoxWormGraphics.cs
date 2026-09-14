using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BoxWormGraphics : GraphicsModule
{
	public class Data
	{
		public BoxWorm ThisCreature;

		public int numberOfBodyBlobs;

		public int numberOfLarvaHolders;

		public int maxNumberOfLarva;

		public int numberOfHeadSprites;

		public int totalSprites;

		public Vector2[] lastBlobPositions;

		public float[] lastBlobScales;

		public bool initDebug = true;

		public DebugSprite[] debugSprites;

		public bool debug;

		public bool colorDirty;

		public float attackSwell;

		public int blobsPerChunk = 5;

		public int bandPosition;

		public bool initialized;

		public float steamPower;

		public float headRot = 170f;

		public float headRotOffset;

		public float headRotCorrection;

		public float headRotCorrecitonStep;

		public float lastHeadRotOffset;

		public float lastHeadRotation;

		public float headClosing = 0.7f;

		public float lastHeadClosing;

		public bool startSpin;

		public Counter activationTimer = new Counter(80);

		public int[] larvaHolderSpritePositions;

		public (Vector2, Vector2, Vector2, Vector2)[] splineSegments;

		public Vector2 Pos => ThisCreature.mainBodyChunk.pos;

		public Vector2 LastPos => ThisCreature.mainBodyChunk.lastPos;

		public Room room => ThisCreature.room;

		public Vector2[] blobPositions { get; set; }

		public float[] blobScales { get; set; }

		public Data(PhysicalObject ow)
		{
			ThisCreature = ow as BoxWorm;
			splineSegments = new(Vector2, Vector2, Vector2, Vector2)[Mathf.Max(ThisCreature.bodyChunkCount - 1, 3)];
			numberOfBodyBlobs = splineSegments.Length * blobsPerChunk;
			numberOfLarvaHolders = ThisCreature.individual.numberOfLarvaHolders;
			numberOfHeadSprites = 2;
			maxNumberOfLarva = 5;
			bandPosition = (blobsPerChunk - 1) / 2;
			totalSprites = numberOfBodyBlobs + numberOfLarvaHolders * 2 + maxNumberOfLarva + numberOfHeadSprites;
			blobPositions = new Vector2[numberOfBodyBlobs];
			lastBlobPositions = new Vector2[numberOfBodyBlobs];
			blobScales = new float[numberOfBodyBlobs];
			lastBlobScales = new float[numberOfBodyBlobs];
			debugSprites = new DebugSprite[ThisCreature.bodyChunkCount];
			larvaHolderSpritePositions = new int[ThisCreature.larvaHolders.Length];
			for (int i = 0; i < larvaHolderSpritePositions.Length; i++)
			{
				larvaHolderSpritePositions[i] = GetBlobIndex(ThisCreature.larvaHolders[i].posAlongBody);
			}
			activationTimer.SetToMin();
		}

		public void Update()
		{
			if (debug)
			{
				UpdateDebugSprites();
			}
			UpdateAnimationTimers();
			UpdateSplineSegments();
			UpdateBodyBlobPositions();
			UpdateBodyScales();
			UpdateHeadRotation();
		}

		public int GetBlobIndex(float p)
		{
			return Mathf.Min((int)(p * (float)blobsPerChunk), blobPositions.Length);
		}

		public void UpdateHeadRotation()
		{
			lastHeadRotOffset = headRotOffset;
			if (!ThisCreature.CanMove)
			{
				startSpin = false;
				headRotOffset = 0f;
			}
			if (ThisCreature.CanMove && !startSpin)
			{
				activationTimer.Reset();
				startSpin = true;
			}
			if (!activationTimer.isFinished)
			{
				if ((int)activationTimer > activationTimer.max - 2)
				{
					Vector2 vector = ThisCreature.bodyChunks[0].pos - ThisCreature.bodyChunks[1].pos;
					room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, ThisCreature.mainBodyChunk, loop: false, UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.1f, 0.3f));
					room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, ThisCreature.mainBodyChunk, loop: false, UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.1f, 0.3f));
					room.PlaySound(SoundID.SS_AI_Marble_Hit_Floor, ThisCreature.mainBodyChunk, loop: false, UnityEngine.Random.Range(0.7f, 1f), UnityEngine.Random.Range(0.1f, 0.3f));
					room.AddObject(new Spark(ThisCreature.bodyChunks[0].pos + vector * 0.8f + Custom.RNV() * 4f, vector.normalized * 14f + Custom.RNV() * 4f, BaseColor(room), null, 8, 36));
					room.AddObject(new Spark(ThisCreature.bodyChunks[0].pos + vector * 0.8f + Custom.RNV() * 4f, vector.normalized * 10f + Custom.RNV() * 4f, BaseColor(room), null, 12, 26));
					room.AddObject(new Spark(ThisCreature.bodyChunks[0].pos + vector * 0.8f + Custom.RNV() * 4f, vector.normalized * 8f + Custom.RNV() * 4f, BaseColor(room), null, 22, 46));
				}
				headRotOffset += Mathf.Pow(activationTimer.normalized, 2f) * 20f;
				activationTimer.Tick();
			}
			HeadRotationCorrection();
			if (activationTimer.isFinished)
			{
				float b = Custom.AimFromOneVectorToAnother(blobPositions[0], blobPositions[1]);
				lastHeadRotation = headRot;
				headRot = Mathf.LerpAngle(headRot, b, 0.07f);
			}
		}

		public void HeadRotationCorrection()
		{
			if ((int)ThisCreature.steamAvailable < ThisCreature.minSteamRequired * 3 || ThisCreature.ai.behavior == BoxWormAI.Behavior.Dormant)
			{
				return;
			}
			if (headRotCorrection < 1f)
			{
				if (!((double)UnityEngine.Random.value > 0.02))
				{
					float num = Custom.Mod(headRot + headRotOffset, 180f);
					if (!(Mathf.Abs(num - 90f) < 20f))
					{
						headRotCorrection = Custom.Mod(90f - num, 180f);
						headRotCorrecitonStep = 0f;
					}
				}
			}
			else
			{
				headRotCorrecitonStep = Mathf.Min(headRotCorrecitonStep + 0.2f, Mathf.Min(headRotCorrection / 30f), 10f);
				headRotOffset += headRotCorrecitonStep;
				headRotCorrection = Mathf.Max(headRotCorrection - headRotCorrecitonStep, headRotCorrecitonStep);
			}
		}

		public void UpdateDebugSprites()
		{
			if (initDebug)
			{
				for (int i = 0; i < debugSprites.Length; i++)
				{
					FSprite fSprite = new FSprite("Circle20");
					fSprite.scaleX = 0.5f;
					fSprite.scaleY = 0.5f;
					fSprite.color = Color.red;
					debugSprites[i] = new DebugSprite(Vector2.zero, fSprite, room);
					room.AddObject(debugSprites[i]);
				}
				initDebug = false;
			}
			for (int j = 0; j < debugSprites.Length; j++)
			{
				debugSprites[j].pos = ThisCreature.bodyChunks[j].pos;
			}
		}

		public Vector2 PointAlongBody(float pos)
		{
			float num = pos * (float)splineSegments.Length;
			int num2 = (int)num;
			num -= (float)num2;
			num = 1f / 3f + num / 3f;
			return Spline(splineSegments[num2], num);
		}

		public void UpdateBodyBlobPositions()
		{
			for (int i = 0; i < blobPositions.Length; i++)
			{
				float pos = (float)i / (float)blobPositions.Length;
				lastBlobPositions[i] = blobPositions[i];
				blobPositions[i] = PointAlongBody(pos);
			}
		}

		public void UpdateBodyScales()
		{
			for (int i = 0; i < blobPositions.Length; i++)
			{
				float num = (float)((i + blobsPerChunk - bandPosition) % blobsPerChunk) / (float)blobsPerChunk;
				float num2 = (float)i / (float)numberOfBodyBlobs;
				lastBlobScales[i] = blobScales[i];
				blobScales[i] = 2f + Mathf.Sin(num * (float)Math.PI) * (0.5f * steamPower + 1.2f * attackSwell * (1f - num2));
			}
		}

		public void UpdateSplineSegments()
		{
			int bodyChunkCount = ThisCreature.bodyChunkCount;
			splineSegments[0] = Spline(ThisCreature.bodyChunks[0].pos * 2f - ThisCreature.bodyChunks[1].pos, ThisCreature.bodyChunks[0].pos, ThisCreature.bodyChunks[1].pos, ThisCreature.bodyChunks[2].pos);
			splineSegments[splineSegments.Length - 1] = Spline(ThisCreature.bodyChunks[bodyChunkCount - 3].pos, ThisCreature.bodyChunks[bodyChunkCount - 2].pos, ThisCreature.bodyChunks[bodyChunkCount - 1].pos, ThisCreature.bodyChunks[bodyChunkCount - 1].pos * 2f - ThisCreature.bodyChunks[bodyChunkCount - 2].pos);
			for (int i = 0; i < splineSegments.Length - 2; i++)
			{
				splineSegments[i + 1] = Spline(ThisCreature.bodyChunks[i].pos, ThisCreature.bodyChunks[i + 1].pos, ThisCreature.bodyChunks[i + 2].pos, ThisCreature.bodyChunks[i + 3].pos);
			}
		}

		public void UpdateAnimationTimers()
		{
			float num = Mathf.Clamp01((float)(int)ThisCreature.steamAvailable / 100f);
			num = Mathf.Lerp((ThisCreature.individual.horizontalOffsets[0] * 0.25f + 1f) * 0.6f, num, num);
			lastHeadClosing = headClosing;
			headClosing = Mathf.Lerp(headClosing, num, num);
			float normalized = ThisCreature.attackTimer.normalized;
			steamPower = ThisCreature.SteamPower;
			bool flag = normalized < 0.9f && !ThisCreature.DoSteam && normalized > 0.2f && !ThisCreature.steamCoolDown && ThisCreature.ai.behavior == BoxWormAI.Behavior.Attack;
			attackSwell = Mathf.Lerp(attackSwell, flag ? 1 : 0, ThisCreature.DoSteam ? 0.04f : 0.02f);
		}

		public Vector2 Spline((Vector2, Vector2, Vector2, Vector2) abcd, float t)
		{
			(Vector2, Vector2, Vector2, Vector2) tuple = abcd;
			Vector2 item = tuple.Item1;
			Vector2 item2 = tuple.Item2;
			Vector2 item3 = tuple.Item3;
			Vector2 item4 = tuple.Item4;
			float num = t * t;
			float num2 = num * t;
			return item + item2 * t + item3 * num + item4 * num2;
		}

		public (Vector2, Vector2, Vector2, Vector2) Spline(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			return (a, 3f * (b - a), 3f * (c - 2f * b + a), d - 3f * c + 3f * b - a);
		}
	}

	public bool rearrange;

	public Data data => (base.owner as BoxWorm).graphicData;

	public BoxWormGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		if ((base.owner as BoxWorm).graphicData == null)
		{
			(base.owner as BoxWorm).graphicData = new Data(ow);
		}
	}

	public override void Update()
	{
		base.Update();
	}

	public static Color BaseColor(Room room)
	{
		return BaseColor(room.abstractRoom);
	}

	public static Color BaseColor(AbstractRoom room)
	{
		if (room.world.region != null)
		{
			return room.world.region.regionParams.boxWormColor;
		}
		if (room.world.name.ToLowerInvariant() == "wtda")
		{
			return RainWorld.SaturatedGold;
		}
		return Color.cyan;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[data.totalSprites];
		int num = 0;
		for (int i = num; i < data.numberOfBodyBlobs; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].shader = Custom.rainWorld.Shaders["BoxWormBody"];
			sLeaser.sprites[i].alpha = 1f;
		}
		num += data.numberOfBodyBlobs;
		for (int j = num; j < num + data.numberOfLarvaHolders; j++)
		{
			sLeaser.sprites[j] = new FSprite("Futile_White");
			sLeaser.sprites[j].shader = Custom.rainWorld.Shaders["BoxWormLarvaHolder"];
			sLeaser.sprites[j].alpha = 1f;
			sLeaser.sprites[j].color = Color.red;
		}
		num += data.numberOfLarvaHolders;
		for (int k = num; k < num + data.numberOfLarvaHolders; k++)
		{
			sLeaser.sprites[k] = new FSprite("Futile_White");
			sLeaser.sprites[k].shader = Custom.rainWorld.Shaders["JaggedCircle"];
		}
		num += data.numberOfLarvaHolders;
		for (int l = num; l < num + data.maxNumberOfLarva; l++)
		{
			sLeaser.sprites[l] = new FSprite("Futile_White");
			sLeaser.sprites[l].shader = Custom.rainWorld.Shaders["BoxWormFakeLarva"];
		}
		num += data.maxNumberOfLarva;
		sLeaser.sprites[num] = new FSprite("Futile_White");
		sLeaser.sprites[num].shader = Custom.rainWorld.Shaders["BoxWormBox"];
		num++;
		sLeaser.sprites[num] = new FSprite("Futile_White");
		sLeaser.sprites[num].shader = Custom.rainWorld.Shaders["BoxWormOpenBox"];
		num++;
		base.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Background");
		}
		int num = 0;
		for (int num2 = data.numberOfBodyBlobs - 1; num2 >= 0; num2--)
		{
			newContatiner.AddChild(sLeaser.sprites[num2]);
		}
		num += data.numberOfBodyBlobs;
		for (int i = 0; i < data.numberOfLarvaHolders; i++)
		{
			int num3 = data.numberOfBodyBlobs + i;
			int num4 = data.larvaHolderSpritePositions[i];
			if (i < data.maxNumberOfLarva)
			{
				int num5 = data.numberOfBodyBlobs + data.numberOfLarvaHolders * 2 + i;
				newContatiner.AddChild(sLeaser.sprites[num5]);
				sLeaser.sprites[num5].MoveInFrontOfOtherNode(sLeaser.sprites[num4 - 1]);
			}
			newContatiner.AddChild(sLeaser.sprites[num3]);
			sLeaser.sprites[num3].MoveInFrontOfOtherNode(sLeaser.sprites[num4 - 1]);
			num3 += data.numberOfLarvaHolders;
			newContatiner.AddChild(sLeaser.sprites[num3]);
			sLeaser.sprites[num3].MoveInFrontOfOtherNode(sLeaser.sprites[num4 + 1]);
		}
		num += data.numberOfLarvaHolders * 2 + data.maxNumberOfLarva;
		newContatiner.AddChild(sLeaser.sprites[num + 1]);
		newContatiner.AddChild(sLeaser.sprites[num]);
		num += 2;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		int num = 0;
		if (rearrange)
		{
			sLeaser.RemoveAllSpritesFromContainer();
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
			rearrange = false;
		}
		for (int i = 0; i < data.numberOfBodyBlobs; i++)
		{
			float scale = Mathf.Lerp(data.lastBlobScales[i], data.blobScales[i], timeStacker);
			sLeaser.sprites[i].SetPosition(Vector2.Lerp(data.lastBlobPositions[i], data.blobPositions[i], timeStacker) - camPos);
			sLeaser.sprites[i].scale = scale;
		}
		num += data.numberOfBodyBlobs;
		for (int j = 0; j < data.numberOfLarvaHolders; j++)
		{
			int num2 = data.numberOfBodyBlobs + j;
			int num3 = data.larvaHolderSpritePositions[j];
			float num4 = Mathf.Lerp(data.lastBlobScales[num3], data.lastBlobScales[num3], timeStacker);
			Vector2 vector = Vector2.Lerp(data.lastBlobPositions[num3], data.blobPositions[num3], timeStacker);
			Vector2 p = Vector2.Lerp(data.lastBlobPositions[num3 + 1], data.blobPositions[num3 + 1], timeStacker);
			float num5 = data.ThisCreature.individual.horizontalOffsets[j];
			float num6 = Custom.AimFromOneVectorToAnother(p, vector);
			Vector2 vector2 = -Custom.DegToVec(num6);
			float num7 = Mathf.Clamp01(data.ThisCreature.SteamPowerWithOffset((int)((num5 - 1f) * 1111f) % 40 - 20) * 2f);
			sLeaser.sprites[num2].SetPosition(vector - camPos);
			sLeaser.sprites[num2].anchorY = (num4 - 2f) * 0.5f + 1.5f;
			sLeaser.sprites[num2].rotation = num6 + num5 * (70f - 20f * Mathf.Clamp01(num7));
			sLeaser.sprites[num2].scale = 1f;
			Color color = sLeaser.sprites[num2].color;
			color.a = num7;
			sLeaser.sprites[num2].color = color;
			if (j < data.maxNumberOfLarva)
			{
				int num8 = data.numberOfBodyBlobs + data.numberOfLarvaHolders * 2 + j;
				if (data.ThisCreature.larvaHolders[j].hasLarva)
				{
					sLeaser.sprites[num8].SetPosition(vector - camPos);
					sLeaser.sprites[num8].anchorY = (num4 - 2f) * 0.5f + 1.5f;
					sLeaser.sprites[num8].rotation = sLeaser.sprites[num2].rotation;
					sLeaser.sprites[num8].scaleX = 1f;
					sLeaser.sprites[num8].scaleY = 1.5f;
					sLeaser.sprites[num8].color = new Color(Mathf.Clamp01(data.ThisCreature.larvaHolders[j].dst / 6f), sLeaser.sprites[num8].color[1], num5 * 0.5f + 0.5f);
					Color color2 = sLeaser.sprites[num8].color;
					color2.a = num7;
					sLeaser.sprites[num8].color = color2;
				}
				sLeaser.sprites[num8].isVisible = data.ThisCreature.larvaHolders[j].hasLarva;
			}
			num2 += data.numberOfLarvaHolders;
			float num9 = Mathf.Clamp01(data.steamPower * 3f);
			sLeaser.sprites[num2].SetPosition(vector + vector2 * (num9 * 3f) - camPos);
			sLeaser.sprites[num2].scale = num4 - 0.08f + (1f - num9) * 0.5f;
			sLeaser.sprites[num2].rotation = (1f - num9) * (float)j * 111f;
			Color color3 = sLeaser.sprites[num2].color;
			color3.a = 0.2f + num9 * 0.8f;
			sLeaser.sprites[num2].color = color3;
		}
		Vector2 vector3 = Vector2.Lerp(data.lastBlobPositions[0], data.blobPositions[0], timeStacker);
		num += data.numberOfLarvaHolders * 2 + data.maxNumberOfLarva;
		sLeaser.sprites[num].SetPosition(vector3 - camPos);
		sLeaser.sprites[num].rotation = data.headRot;
		Color color4 = sLeaser.sprites[num].color;
		color4.a = Custom.Mod(data.headRot + 180f + Mathf.Lerp(data.lastHeadRotOffset, data.headRotOffset, timeStacker), 180f) / 180f;
		sLeaser.sprites[num].color = color4;
		sLeaser.sprites[num].scaleX = 2f + 1f * Mathf.Clamp01(1f - (float)((int)data.ThisCreature.steamAvailable - 100) / 4f);
		sLeaser.sprites[num].scaleY = 3f;
		sLeaser.sprites[num].anchorY = 0.7f;
		sLeaser.sprites[num].isVisible = data.headClosing == 1f;
		num++;
		sLeaser.sprites[num].SetPosition(vector3 - camPos);
		sLeaser.sprites[num].rotation = data.headRot;
		sLeaser.sprites[num].scaleX = 6f;
		sLeaser.sprites[num].scaleY = 4.8f;
		sLeaser.sprites[num].isVisible = data.headClosing != 1f;
		color4 = sLeaser.sprites[num].color;
		color4.r = (base.owner as BoxWorm).steamAvailable.normalized * 3f * (float)((base.owner as BoxWorm).NeedsHelp() ? 1 : 0);
		color4.a = Mathf.Lerp(data.lastHeadClosing, data.headClosing, timeStacker);
		sLeaser.sprites[num].color = color4;
		num++;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color blackColor = palette.blackColor;
		Color color = palette.blackColor + palette.skyColor * 0.3f;
		Color color2 = BaseColor(rCam.room) * 0.1f;
		for (int i = 0; i < data.numberOfBodyBlobs; i++)
		{
			float num = ((i % data.blobsPerChunk == data.bandPosition) ? 0.3f : 0f);
			sLeaser.sprites[i].color = Color.Lerp(color, blackColor, (float)i / (float)data.numberOfBodyBlobs + num);
		}
		for (int j = 0; j < data.numberOfLarvaHolders; j++)
		{
			Color color3 = Color.Lerp(color, blackColor, (float)data.larvaHolderSpritePositions[j] / (float)data.numberOfBodyBlobs);
			if (j < data.maxNumberOfLarva)
			{
				int num2 = j + data.numberOfBodyBlobs + data.numberOfLarvaHolders * 2;
				sLeaser.sprites[num2].color = new Color(0f, color3.maxColorComponent, 0f);
			}
			sLeaser.sprites[j + data.numberOfBodyBlobs].color = color3;
			sLeaser.sprites[j + data.numberOfBodyBlobs + data.numberOfLarvaHolders].color = color3 + color2;
		}
		int num3 = data.numberOfBodyBlobs + data.numberOfLarvaHolders * 2 + data.maxNumberOfLarva;
		sLeaser.sprites[num3].color = color;
		sLeaser.sprites[num3 + 1].color = color;
		num3 += 2;
	}

	public override void PushOutOf(Vector2 pos, float rad)
	{
		base.PushOutOf(pos, rad);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void SuckedIntoShortCut(Vector2 shortCutPosition)
	{
		base.SuckedIntoShortCut(shortCutPosition);
	}
}
