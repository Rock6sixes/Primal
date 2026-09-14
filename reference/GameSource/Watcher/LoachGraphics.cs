using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class LoachGraphics : GraphicsModule, ILookingAtCreatures, HasDanglers, DaddyGraphics.IHaveRotGraphics
{
	public struct DanglerAttachment
	{
		public int firstChunk;

		public float lerp;

		public Vector2 offset;

		public DanglerAttachment(int firstChunk, float lerp, Vector2 offset)
		{
			this.firstChunk = firstChunk;
			this.lerp = lerp;
			this.offset = offset;
		}
	}

	public class EyeData
	{
		public LoachGraphics owner;

		public DaddyGraphics.Eye eye;

		public float alongBody;

		public float perpOffset;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public Vector2 IdealPos
		{
			get
			{
				Vector2 vector = owner.realizedCreature.bodyChunks[0].pos;
				Vector2 vector2 = owner.realizedCreature.bodyChunks[1].pos;
				vector += owner.realizedCreature.bodyChunks[0].rad * 0.9f * (vector - vector2).normalized;
				Vector2 b = owner.realizedCreature.bodyChunks[2].pos;
				Vector2 vector3 = Custom.Bezier(vector, vector2, b, vector2, alongBody);
				Vector2 vector4 = Custom.PerpendicularVector(Custom.BezierDerivative(vector, vector2, b, vector2, alongBody));
				return vector3 + vector4 * perpOffset;
			}
		}

		public EyeData(LoachGraphics owner, DaddyGraphics.Eye eye, float alongBody, float perpOffset)
		{
			this.owner = owner;
			this.eye = eye;
			this.alongBody = alongBody;
			this.perpOffset = perpOffset;
		}

		public void Update()
		{
			lastPos = pos;
			pos += vel;
			Vector2 idealPos = IdealPos;
			vel += (idealPos - pos) * 0.5f;
			pos += (idealPos - pos) * 0.5f;
			vel = Vector2.Lerp(vel, owner.realizedCreature.mainBodyChunk.vel, 0.2f);
			pos = Vector2.ClampMagnitude(pos - idealPos, eye.rad) + idealPos;
		}

		public void Reset()
		{
			pos = IdealPos;
			lastPos = pos;
			vel = default(Vector2);
		}

		public Vector2 DrawPos(float timeStacker)
		{
			return Vector2.Lerp(lastPos, pos, timeStacker);
		}

		public Vector2 PushOutFromPos(float timeStacker)
		{
			Vector2 a = Vector2.Lerp(owner.realizedCreature.bodyChunks[0].lastPos, owner.realizedCreature.bodyChunks[0].pos, timeStacker);
			Vector2 vector = Vector2.Lerp(owner.realizedCreature.bodyChunks[1].lastPos, owner.realizedCreature.bodyChunks[1].pos, timeStacker);
			Vector2 b = Vector2.Lerp(owner.realizedCreature.bodyChunks[2].lastPos, owner.realizedCreature.bodyChunks[2].pos, timeStacker);
			if (alongBody < 0.5f)
			{
				return Vector2.Lerp(a, vector, alongBody * 2f);
			}
			return Vector2.Lerp(vector, b, alongBody * 2f - 1f);
		}
	}

	public class SpotData
	{
		public Vector2 attachment;

		public float lightness;

		public float scale;

		public float angleFactor;

		public SpotData(Vector2 attachment)
		{
			this.attachment = attachment;
			lightness = Mathf.Pow(UnityEngine.Random.value, 2f) * 0.5f + 0.25f;
			scale = UnityEngine.Random.value * 0.4f + 0.6f;
			angleFactor = UnityEngine.Random.value * 0.5f + 0.25f;
		}
	}

	public class LoachTentacleGraphic : RopeGraphic
	{
		public struct CircleData
		{
			public Vector2 offset;

			public float radius;

			public bool eye;

			public CircleData(Vector2 offset, float radius, bool eye)
			{
				this.offset = offset;
				this.radius = radius;
				this.eye = eye;
			}
		}

		public LoachGraphics graphicsModule;

		public LoachTentacle tentacle;

		public int firstSprite;

		public CircleData[] circles;

		public int TotalSprites => 1 + circles.Length;

		public LoachTentacleGraphic(LoachGraphics graphicsModule, LoachTentacle tentacle, int firstSprite)
			: base(15)
		{
			this.graphicsModule = graphicsModule;
			this.tentacle = tentacle;
			this.firstSprite = firstSprite;
			List<CircleData> list = new List<CircleData>();
			int num = (graphicsModule.Rotted ? UnityEngine.Random.Range(8, 11) : 0);
			for (int i = 0; i < num; i++)
			{
				float num2 = 1f - (float)i / (float)(num - 1);
				Vector2 offset = new Vector2(UnityEngine.Random.Range(-1f, 1f) * 5f * num2, (float)i * 4f);
				float num3 = Mathf.Lerp(UnityEngine.Random.value, num2, UnityEngine.Random.value) * 4f + 1.5f;
				if (i == 0)
				{
					num3 = Mathf.Lerp(num3, 5f, 0.5f);
				}
				list.Add(new CircleData(offset, num3, eye: false));
			}
			for (int j = 0; j < num; j++)
			{
				float num4 = 1f - (float)j / (float)(num - 1);
				if (UnityEngine.Random.value < 0.6f * num4)
				{
					float radius = list[j].radius * Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, num4)));
					list.Add(new CircleData(list[j].offset, radius, eye: true));
				}
			}
			circles = list.ToArray();
		}

		public override void Update()
		{
			int listCount = 0;
			AddToPositionsList(listCount++, tentacle.FloatBase);
			for (int i = 0; i < tentacle.tChunks.Length; i++)
			{
				for (int j = 1; j < tentacle.tChunks[i].rope.TotalPositions; j++)
				{
					AddToPositionsList(listCount++, tentacle.tChunks[i].rope.GetPosition(j));
				}
			}
			AlignAndConnect(listCount);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMeshAtlased(segments.Length, pointyTip: false, customColor: true);
			for (int i = 0; i < circles.Length; i++)
			{
				sLeaser.sprites[firstSprite + 1 + i] = new FSprite("Circle20", quadType: false);
			}
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
			if (triangleMesh.vertices.Length != segments.Length * 4)
			{
				InitiateSprites(sLeaser, rCam);
			}
			Vector2 vector = Vector2.Lerp(tentacle.connectedChunk.lastPos, tentacle.connectedChunk.pos, timeStacker);
			vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector);
			float a = 2f;
			for (int i = 0; i < segments.Length; i++)
			{
				float num = (float)i / (float)(segments.Length - 1);
				Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num2 = Vector2.Distance(vector, vector2) / 3f;
				float num3 = ((!graphicsModule.Rotted) ? ((1.5f + 2f * Mathf.Pow(1f - num, 2f)) * Mathf.Pow(1f - num, 0.25f) + 3f * Mathf.Sin((float)Math.PI * Mathf.Pow(num, 10f))) : (2.5f + 1f * Mathf.Pow(1f - num, 2f)));
				triangleMesh.MoveVertice(i * 4, vector - normalized * num2 - vector3 * Mathf.Lerp(a, num3, 0.5f) - camPos);
				triangleMesh.MoveVertice(i * 4 + 1, vector - normalized * num2 + vector3 * Mathf.Lerp(a, num3, 0.5f) - camPos);
				triangleMesh.MoveVertice(i * 4 + 2, vector2 + normalized * num2 - vector3 * num3 - camPos);
				triangleMesh.MoveVertice(i * 4 + 3, vector2 + normalized * num2 + vector3 * num3 - camPos);
				vector = vector2;
				a = num3;
			}
			for (int j = 0; j < circles.Length; j++)
			{
				Vector2 offset = circles[j].offset;
				offset.y = 1f - Mathf.Clamp01(offset.y / Mathf.Max(tentacle.CurrentLength(), 1f));
				sLeaser.sprites[firstSprite + 1 + j].scale = circles[j].radius / 10f;
				sLeaser.sprites[firstSprite + 1 + j].SetPosition(OnTubePos(offset, timeStacker) - camPos);
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			base.ApplyPalette(sLeaser, rCam, palette);
			Color bodyColor = graphicsModule.bodyColor;
			Color color = graphicsModule.EffectColor;
			if (!graphicsModule.Rotted)
			{
				color = Color.Lerp(color, bodyColor, 0.75f);
			}
			TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
			for (int i = 0; i < triangleMesh.verticeColors.Length; i += 2)
			{
				float num = (float)i / (float)(triangleMesh.verticeColors.Length - 2);
				triangleMesh.verticeColors[i] = (triangleMesh.verticeColors[i + 1] = Color.Lerp(bodyColor, color, Mathf.Pow(Mathf.Clamp01(num * 1.5f - 0.5f), 2f) * 0.4f));
			}
			for (int j = 0; j < circles.Length; j++)
			{
				float num2 = (float)j / (float)(circles.Length - 1);
				sLeaser.sprites[firstSprite + 1 + j].color = Color.Lerp(bodyColor, color, circles[j].eye ? 1f : (0.4f - 0.1f * num2));
			}
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel = Vector2.zero;
			if (tentacle.room.GetTile(smoothedGoalPos).Solid && !tentacle.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, tentacle.room.TileRect(tentacle.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
				segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
			}
			else
			{
				segments[segment].pos = smoothedGoalPos;
			}
		}

		public Vector2 OnTubePos(Vector2 pos, float timeStacker)
		{
			Vector2 p = OneDimensionalTubePos(pos.y - 1f / (float)segments.Length, timeStacker);
			Vector2 p2 = OneDimensionalTubePos(pos.y + 1f / (float)segments.Length, timeStacker);
			return OneDimensionalTubePos(pos.y, timeStacker) + Custom.PerpendicularVector(Custom.DirVec(p, p2)) * pos.x;
		}

		public Vector2 OneDimensionalTubePos(float floatPos, float timeStacker)
		{
			int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (float)(segments.Length - 1)), 0, segments.Length - 1);
			int num2 = Custom.IntClamp(num + 1, 0, segments.Length - 1);
			float t = Mathf.InverseLerp(num, num2, floatPos * (float)(segments.Length - 1));
			return Vector2.Lerp(Vector2.Lerp(segments[num].lastPos, segments[num2].lastPos, t), Vector2.Lerp(segments[num].pos, segments[num2].pos, t), timeStacker);
		}
	}

	public Color bodyColor;

	public Dangler.DanglerProps danglerProps;

	public Dangler[] danglers;

	public DanglerAttachment[] danglerAttachments;

	public LoachTentacleGraphic[] tentacleGraphics;

	public SpotData[] spots;

	public float bodyLength;

	public EyeData[] eyeData;

	public int blink;

	public CreatureLooker creatureLooker;

	public int reactionTimer;

	public float flip;

	public float lastFlip;

	public float resting;

	public float lastResting;

	public static float breathsPerSecond = 1f / 3f;

	public int breathTimer;

	public float lastBreathFactor;

	public float breathFactor;

	public static float swayRate = 5f / 9f;

	public float walkTimer;

	public float walkSway;

	public float lastWalkSway;

	public int firstDanglerSprite;

	public int firstShoulderSprite;

	public int firstBodySprite;

	public int firstSpotSprite;

	public int firstLegSprite;

	public int firstPeeperSprite;

	public int totalSprites;

	public static Vector2[,,] legGraphicAnchors = new Vector2[2, 2, 2]
	{
		{
			{
				new Vector2(19f, 44f),
				new Vector2(13f, 62f)
			},
			{
				new Vector2(32f, 69f),
				new Vector2(13f, 56f)
			}
		},
		{
			{
				new Vector2(8f, 5f),
				new Vector2(6f, 4f)
			},
			{
				new Vector2(3.5f, 3f),
				new Vector2(6f, 3f)
			}
		}
	};

	public Loach realizedCreature => base.owner as Loach;

	public bool Rotted => realizedCreature.Rotted;

	public Color EffectColor => realizedCreature.effectColor;

	public Color EyeColor => realizedCreature.eyeColor;

	public float BumpSize => 1f;

	public DaddyLongLegs.IHaveRotParts RotPartsOwner => realizedCreature;

	public bool ApplyPaletteInDraw => false;

	public float Digesting => realizedCreature.MostDigestedEatObject;

	public DaddyGraphics.Eye[] Eyes { get; set; }

	public DaddyTentacle[] Tentacles => Array.Empty<DaddyTentacle>();

	public int LegSprite(int leg, int part)
	{
		return firstLegSprite + leg * 3 + part;
	}

	public float LegPartLength(int pos, int part, bool includeExtension)
	{
		if (pos == 0)
		{
			return ((part == 0) ? 35f : 55f) * (includeExtension ? (realizedCreature.preferredHeight / 15f) : 1f);
		}
		return ((part == 0) ? 60f : 45f) * (includeExtension ? (realizedCreature.preferredHeight / 15f) : 1f);
	}

	public LoachGraphics(PhysicalObject po)
		: base(po, internalContainers: false)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(realizedCreature.abstractCreature.ID.RandomSeed);
		bodyLength = realizedCreature.bodyChunks[0].rad + realizedCreature.bodyChunks[realizedCreature.bodyChunks.Length - 1].rad;
		for (int i = 0; i < realizedCreature.bodyChunks.Length - 1; i++)
		{
			bodyLength += realizedCreature.bodyChunkConnections[i].distance;
		}
		internalContainerObjects = new List<ObjectHeldInInternalContainer>();
		creatureLooker = new CreatureLooker(this, realizedCreature.AI.tracker, realizedCreature, 0.4f, 30);
		danglerProps = new Dangler.DanglerProps();
		danglers = new Dangler[20];
		danglerAttachments = new DanglerAttachment[danglers.Length];
		for (int j = 0; j < danglers.Length; j++)
		{
			float num = Mathf.Lerp(6f, 12f, Mathf.Pow(UnityEngine.Random.value, 3f));
			if (UnityEngine.Random.value < 0.25f)
			{
				int num2 = 4;
				float lerp = 1f;
				float rad = realizedCreature.bodyChunks[num2 + 1].rad;
				danglerAttachments[j] = new DanglerAttachment(num2, lerp, Custom.RotateAroundOrigo(new Vector2(0f, (0f - rad) * 0.7f), UnityEngine.Random.Range(-70f, 70f)));
			}
			else
			{
				int num3 = UnityEngine.Random.Range(0, realizedCreature.bodyChunks.Length - 2);
				float value = UnityEngine.Random.value;
				float num4 = Mathf.Lerp(realizedCreature.bodyChunks[num3].rad, realizedCreature.bodyChunks[num3 + 1].rad, value);
				danglerAttachments[j] = new DanglerAttachment(num3, value, new Vector2(0f, (0f - num4) * 0.7f));
			}
			float num5 = ((float)danglerAttachments[j].firstChunk + danglerAttachments[j].lerp) / (float)realizedCreature.bodyChunks.Length;
			num *= 0.3f + 0.7f * num5 * (1f - num5) * 4f;
			num = Mathf.Max(num, 3f);
			danglers[j] = new Dangler(this, j, (int)num, 3f, 4f, collideWithTerrain: false);
			for (int k = 0; k < danglers[j].segments.Length; k++)
			{
				float num6 = (float)k / (float)(danglers[j].segments.Length - 1);
				danglers[j].segments[k].rad *= Mathf.Pow(1f - num6, 0.6f) + Mathf.Sin((num6 * 0.9f + 0.1f) * (float)Math.PI);
			}
		}
		GenerateSpots();
		tentacleGraphics = new LoachTentacleGraphic[realizedCreature.tentacle.Length];
		for (int l = 0; l < tentacleGraphics.Length; l++)
		{
			tentacleGraphics[l] = new LoachTentacleGraphic(this, realizedCreature.tentacle[l], totalSprites);
			totalSprites += tentacleGraphics[l].TotalSprites;
		}
		firstDanglerSprite = totalSprites;
		totalSprites += danglers.Length;
		firstShoulderSprite = totalSprites;
		totalSprites += realizedCreature.legs.Length;
		firstBodySprite = totalSprites;
		totalSprites += realizedCreature.bodyChunks.Length * 2 - 1;
		firstSpotSprite = totalSprites;
		totalSprites += spots.Length * 2;
		firstLegSprite = totalSprites;
		totalSprites += realizedCreature.legs.Length * 3;
		firstPeeperSprite = totalSprites;
		totalSprites += ((!Rotted) ? 4 : 0);
		if (Rotted)
		{
			Eyes = new DaddyGraphics.Eye[15];
			eyeData = new EyeData[Eyes.Length];
			for (int m = 0; m < Eyes.Length; m++)
			{
				float num7 = (float)m / (float)(Eyes.Length - 1);
				Eyes[m] = new DaddyGraphics.Eye(this, this, m, Mathf.Lerp(5f, 13f, Mathf.Lerp(1f - num7, UnityEngine.Random.value, UnityEngine.Random.value)), HDmode: false, totalSprites);
				float alongBody = Mathf.Pow(num7, 2.5f);
				float perpOffset = realizedCreature.mainBodyChunk.rad * UnityEngine.Random.Range(-1f, 1f) * Mathf.Lerp(0.5f, 1f, Mathf.Sqrt(num7));
				eyeData[m] = new EyeData(this, Eyes[m], alongBody, perpOffset);
				totalSprites += Eyes[m].sprites;
			}
		}
		else
		{
			Eyes = new DaddyGraphics.Eye[0];
			eyeData = new EyeData[0];
		}
		UnityEngine.Random.state = state;
	}

	public void GenerateSpots()
	{
		Vector2[] array = new Vector2[35];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = UnityEngine.Random.insideUnitCircle;
			array[i] *= (array[i].sqrMagnitude + 1f) / 2f;
		}
		float num = 0.2f;
		for (int j = 0; j < 200; j++)
		{
			for (int k = 0; k < array.Length; k++)
			{
				for (int l = k + 1; l < array.Length; l++)
				{
					Vector2 vector = array[l] - array[k];
					float sqrMagnitude = vector.sqrMagnitude;
					if (!(sqrMagnitude >= num * num))
					{
						float num2 = Mathf.Sqrt(sqrMagnitude);
						Vector2 vector2 = (num - num2) / num2 * 0.2f * vector;
						array[k] -= vector2;
						array[k].x = Mathf.Max(array[k].x, -1f);
						array[l] += vector2;
						array[l].x = Mathf.Max(array[l].x, -1f);
					}
				}
			}
		}
		spots = new SpotData[array.Length];
		for (int m = 0; m < array.Length; m++)
		{
			spots[m] = new SpotData(array[m]);
		}
	}

	public override void Update()
	{
		base.Update();
		creatureLooker.Update();
		breathTimer++;
		lastBreathFactor = breathFactor;
		if (realizedCreature.Consious)
		{
			breathFactor = Mathf.Lerp(breathFactor, Mathf.Sin((float)breathTimer / 40f * (float)Math.PI * 2f * breathsPerSecond), 0.1f);
		}
		else
		{
			breathTimer = 0;
			breathFactor = Mathf.Lerp(breathFactor, 0f, 0.01f);
		}
		lastWalkSway = walkSway;
		if (realizedCreature.Consious)
		{
			float num = Mathf.Min(1f, Mathf.Abs(realizedCreature.bodyChunks[2].vel.x) / 2f);
			walkTimer += num * swayRate / 40f * (float)Math.PI * 2f;
			walkTimer %= (float)Math.PI * 2f;
			walkSway = Mathf.Lerp(walkSway, Mathf.Sin(walkTimer) * num, 0.1f);
		}
		else
		{
			walkSway = Mathf.Lerp(walkSway, 0f, 0.01f);
		}
		LoachTentacleGraphic[] array = tentacleGraphics;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Update();
		}
		Dangler[] array2 = danglers;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Update();
		}
		EyeData[] array3 = eyeData;
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].Update();
		}
		DaddyGraphics.Eye[] eyes = Eyes;
		for (int i = 0; i < eyes.Length; i++)
		{
			eyes[i].Update();
		}
		if (reactionTimer > 0)
		{
			reactionTimer--;
		}
		if (!realizedCreature.Consious)
		{
			blink = 10;
		}
		blink--;
		if (blink < -80 && UnityEngine.Random.value < 0.05f)
		{
			blink = UnityEngine.Random.Range(25, 35);
		}
		if (realizedCreature.AI.behavior == LoachAI.Behavior.Hunt && realizedCreature.AI.focusCreature != null && reactionTimer == 0)
		{
			reactionTimer = UnityEngine.Random.Range(10, 40);
			ReactToCreature(realizedCreature.AI.focusCreature);
		}
		lastFlip = flip;
		flip = Custom.LerpAndTick(flip, realizedCreature.bodDir.x, 0.05f, 0.005f);
		lastResting = resting;
		resting = realizedCreature.resting;
	}

	public override void Reset()
	{
		base.Reset();
		LoachTentacleGraphic[] array = tentacleGraphics;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset(realizedCreature.mainBodyChunk.pos);
		}
		Dangler[] array2 = danglers;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Reset();
		}
		for (int j = 0; j < eyeData.Length; j++)
		{
			eyeData[j].Reset();
		}
	}

	public void ReactToCreature(Tracker.CreatureRepresentation rep)
	{
		if (!Rotted)
		{
			return;
		}
		Vector2 pos = realizedCreature.mainBodyChunk.pos;
		Vector2 p = realizedCreature.room.MiddleOfTile(rep.BestGuessForPosition());
		float num = float.MinValue;
		int num2 = -1;
		for (int i = 0; i < Eyes.Length; i++)
		{
			float num3 = Vector2.Dot(Custom.DirVec(pos, eyeData[i].pos), Custom.DirVec(pos, p));
			if (Eyes[i].soundSource != null)
			{
				num3 -= 1f;
			}
			if (Eyes[i].creatureRep != null)
			{
				num3 -= 2f;
			}
			if (num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		Eyes[num2].ReactToCreature(rep);
		base.owner.room.PlaySound(SoundID.Daddy_React_To_Tentacle_Touch, pos, base.owner.abstractPhysicalObject);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		for (int i = 0; i < realizedCreature.bodyChunks.Length * 2 - 1; i++)
		{
			sLeaser.sprites[firstBodySprite + i] = new FSprite("Futile_White")
			{
				shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
				alpha = 0.6f
			};
		}
		DaddyGraphics.Eye[] eyes = Eyes;
		for (int j = 0; j < eyes.Length; j++)
		{
			eyes[j].InitiateSprites(sLeaser, rCam);
		}
		if (!Rotted)
		{
			sLeaser.sprites[firstPeeperSprite] = new FSprite("LoachEye");
			sLeaser.sprites[firstPeeperSprite + 1] = new FSprite("Circle4");
			sLeaser.sprites[firstPeeperSprite + 2] = new FSprite("LoachEye");
			sLeaser.sprites[firstPeeperSprite + 3] = new FSprite("Circle4");
		}
		LoachTentacleGraphic[] array = tentacleGraphics;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].InitiateSprites(sLeaser, rCam);
		}
		for (int k = 0; k < danglers.Length; k++)
		{
			danglers[k].InitSprite(sLeaser, firstDanglerSprite + k);
		}
		for (int l = 0; l < realizedCreature.legs.Length; l++)
		{
			sLeaser.sprites[LegSprite(l, 0)] = new FSprite("deerLeg" + ((l % 2 == 0) ? "A" : "B") + "1");
			sLeaser.sprites[LegSprite(l, 0)].anchorY = legGraphicAnchors[1, l % 2, 0].y / legGraphicAnchors[0, l % 2, 0].y;
			sLeaser.sprites[LegSprite(l, 0)].anchorX = legGraphicAnchors[1, l % 2, 0].x / legGraphicAnchors[0, l % 2, 0].x;
			sLeaser.sprites[LegSprite(l, 1)] = new FSprite("deerLeg" + ((l % 2 == 0) ? "A" : "B") + "2");
			sLeaser.sprites[LegSprite(l, 1)].anchorY = legGraphicAnchors[1, l % 2, 1].y / legGraphicAnchors[0, l % 2, 1].y;
			sLeaser.sprites[LegSprite(l, 1)].anchorX = legGraphicAnchors[1, l % 2, 1].x / legGraphicAnchors[0, l % 2, 1].x;
			sLeaser.sprites[LegSprite(l, 2)] = TriangleMesh.MakeLongMesh(7, pointyTip: false, customColor: true);
		}
		for (int m = 0; m < realizedCreature.legs.Length; m++)
		{
			sLeaser.sprites[firstShoulderSprite + m] = TriangleMesh.MakeLongMeshAtlased(5, pointyTip: false, customColor: false);
		}
		for (int n = 0; n < spots.Length; n++)
		{
			sLeaser.sprites[firstSpotSprite + n] = new FSprite(Rotted ? "GrowthA0" : "GrowthA1")
			{
				anchorY = 0f
			};
			sLeaser.sprites[firstSpotSprite + n + spots.Length] = new FSprite(Rotted ? "GrowthB0" : "GrowthB1")
			{
				anchorY = 0f
			};
		}
		sLeaser.containers = new FContainer[2]
		{
			new FContainer(),
			new FContainer()
		};
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (sLeaser.deleteMeNextFrame)
		{
			return;
		}
		float num = CurrentFaceDir(timeStacker);
		Vector2 vector = Custom.DirVec(Vector2.Lerp(realizedCreature.bodyChunks[3].lastPos, realizedCreature.bodyChunks[3].pos, timeStacker), Vector2.Lerp(realizedCreature.bodyChunks[1].lastPos, realizedCreature.bodyChunks[1].pos, timeStacker));
		LoachTentacleGraphic[] array = tentacleGraphics;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		for (int j = 0; j < danglers.Length; j++)
		{
			danglers[j].DrawSprite(firstDanglerSprite + j, sLeaser, rCam, timeStacker, camPos);
		}
		for (int k = 0; k < realizedCreature.bodyChunks.Length; k++)
		{
			BodyChunk bodyChunk = realizedCreature.bodyChunks[k];
			Vector2 a = Vector2.Lerp(bodyChunk.lastPos, bodyChunk.pos, timeStacker);
			sLeaser.sprites[firstBodySprite + k * 2].SetPosition(Vector2.Lerp(bodyChunk.lastPos, bodyChunk.pos, timeStacker) - camPos);
			sLeaser.sprites[firstBodySprite + k * 2].rotation = Custom.VecToDeg(bodyChunk.Rotation) + (float)k * 40f;
			sLeaser.sprites[firstBodySprite + k * 2].scale = GetChunkVisualRad(k, timeStacker) / 8f * 1.05f;
			if (k < realizedCreature.bodyChunks.Length - 1)
			{
				BodyChunk bodyChunk2 = realizedCreature.bodyChunks[k + 1];
				Vector2 b = Vector2.Lerp(bodyChunk2.lastPos, bodyChunk2.pos, timeStacker);
				sLeaser.sprites[firstBodySprite + k * 2 + 1].SetPosition(Vector2.Lerp(a, b, 0.5f) - camPos);
				sLeaser.sprites[firstBodySprite + k * 2 + 1].rotation = Custom.VecToDeg(Vector2.Lerp(bodyChunk.Rotation, bodyChunk2.Rotation, 0.5f)) + 120f + (float)k * 40f;
				sLeaser.sprites[firstBodySprite + k * 2 + 1].scale = Mathf.Lerp(GetChunkVisualRad(k, timeStacker), GetChunkVisualRad(k + 1, timeStacker), 0.5f) / 8f * 1.05f;
			}
		}
		for (int l = 0; l < realizedCreature.legs.Length; l++)
		{
			LoachLeg loachLeg = realizedCreature.legs[l];
			TriangleMesh triangleMesh = sLeaser.sprites[firstShoulderSprite + l] as TriangleMesh;
			Vector2 vector2 = Vector2.Lerp(loachLeg.connectedChunk.lastPos, loachLeg.connectedChunk.pos, timeStacker);
			float rad = loachLeg.connectedChunk.rad;
			Vector2 normalized = (Vector2.Lerp(loachLeg.tChunks[1].lastPos, loachLeg.tChunks[1].pos, timeStacker) - vector2).normalized;
			float num2 = loachLeg.CurrentLength() / loachLeg.maxLength;
			for (int m = 0; m < triangleMesh.vertices.Length; m += 2)
			{
				float num3 = (float)m / (float)(triangleMesh.vertices.Length - 2) * 2f - 1f;
				Vector2 vector3 = Custom.RotateAroundOrigo(-normalized, num3 * 50f);
				float num4 = (1f - num2 * 0.75f) * (Mathf.Cos(num3 * (float)Math.PI) * 0.5f + 0.5f) * 25f;
				triangleMesh.MoveVertice(m, vector2 + vector3 * (rad * 0.85f) - camPos);
				triangleMesh.MoveVertice(m + 1, vector2 + vector3 * (rad * 0.85f + num4) - camPos);
			}
		}
		DaddyGraphics.Eye[] eyes = Eyes;
		for (int i = 0; i < eyes.Length; i++)
		{
			eyes[i].DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		if (!Rotted)
		{
			Vector2 vector4 = Vector2.Lerp(realizedCreature.bodyChunks[0].lastPos, realizedCreature.bodyChunks[0].pos, timeStacker);
			Vector2 vector5 = Vector3.Slerp(realizedCreature.bodyChunks[1].lastPos, realizedCreature.bodyChunks[1].pos, timeStacker);
			Vector2 normalized2 = (vector4 - vector5).normalized;
			Vector2 vector6 = Custom.PerpendicularVector(normalized2);
			float rad2 = realizedCreature.bodyChunks[0].rad;
			Vector2 vector7 = vector4 + new Vector2(100f, -50f);
			if (creatureLooker.lookCreature != null && realizedCreature.room != null)
			{
				vector7 = ((!creatureLooker.lookCreature.VisualContact || creatureLooker.lookCreature.representedCreature.realizedCreature == null) ? realizedCreature.room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition()) : creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos);
			}
			vector7 = Vector2.ClampMagnitude(vector7 - vector4, 200f) / 200f;
			for (int n = 0; n < 2; n++)
			{
				float f = num + (float)Math.PI * 2f / 5f * ((n == 0) ? 1f : (-1f)) + Mathf.Lerp(lastWalkSway, walkSway, timeStacker) * (float)Math.PI * 0.06f;
				Vector2 vector8 = vector4 + normalized2 * 3f + vector6 * (Mathf.Sin(f) * (rad2 - 3f));
				float num5 = Mathf.Cos(f);
				FSprite obj = sLeaser.sprites[firstPeeperSprite + n * 2];
				FSprite fSprite = sLeaser.sprites[firstPeeperSprite + n * 2 + 1];
				obj.SetPosition(vector8 - camPos);
				obj.scaleY = num5;
				obj.rotation = Custom.VecToDeg(normalized2) - (float)Math.PI / 2f;
				obj.isVisible = num5 > 0f;
				fSprite.SetPosition(vector8 + vector7 * 2f - camPos);
				if (blink <= 0)
				{
					fSprite.SetElementByName("Circle4");
					fSprite.scaleX = 1f;
					fSprite.scaleY = Mathf.Min(1f, num5 * 3f);
					fSprite.rotation = 0f;
				}
				else
				{
					fSprite.SetElementByName("pixel");
					fSprite.scaleX = 1f;
					fSprite.scaleY = 4f;
					fSprite.rotation = Custom.VecToDeg(normalized2);
				}
				fSprite.isVisible = num5 > 0f;
			}
		}
		float num6 = num * (float)Math.PI * 0.5f + Mathf.Lerp(lastWalkSway, walkSway, timeStacker) * (float)Math.PI * 0.06f;
		float num7 = bodyLength - 100f;
		float num8 = bodyLength - 3f;
		float num9 = 2f;
		for (int num10 = 0; num10 < spots.Length; num10++)
		{
			float num11 = Mathf.Lerp(num7, num8, spots[num10].attachment.x * 0.5f + 0.5f);
			float num12 = spots[num10].attachment.y * (num8 - num7) / 2f * num9;
			Vector2 vector9 = AlongBodyPos(num11, timeStacker);
			float num13 = AlongBodyRad(num11, timeStacker);
			Vector2 vector10 = Custom.DirVec(AlongBodyPos(num11 - 20f, timeStacker), AlongBodyPos(num11 + 20f, timeStacker));
			Vector2 vector11 = -Custom.PerpendicularVector(vector10);
			vector10 = Vector3.Slerp(vector10, vector9 - Vector2.Lerp(realizedCreature.mainBodyChunk.lastPos, realizedCreature.mainBodyChunk.pos, timeStacker), spots[num10].angleFactor);
			float f2 = num12 / AlongBodyRad(num11, timeStacker, useBreath: false) + (float)Math.PI - num6;
			bool flag = Mathf.Cos(f2) > 0f;
			vector9 += Mathf.Sin(f2) * num13 * vector11;
			int num14 = firstSpotSprite + num10;
			int num15 = firstSpotSprite + num10 + spots.Length;
			sLeaser.sprites[num14].SetPosition(vector9 - camPos);
			sLeaser.sprites[num14].scaleX = spots[num10].scale * (0f - Mathf.Sign(num));
			sLeaser.sprites[num14].scaleY = spots[num10].scale;
			sLeaser.sprites[num14].rotation = Custom.VecToDeg(vector10);
			sLeaser.sprites[num15].SetPosition(sLeaser.sprites[num14].GetPosition());
			sLeaser.sprites[num15].scaleX = sLeaser.sprites[num14].scaleX;
			sLeaser.sprites[num15].scaleY = sLeaser.sprites[num14].scaleY;
			sLeaser.sprites[num15].rotation = sLeaser.sprites[num14].rotation;
			FContainer fContainer = sLeaser.containers[flag ? 1 : 0];
			if (sLeaser.sprites[num14].container != fContainer)
			{
				fContainer.AddChildAtIndex(sLeaser.sprites[num14], 0);
				fContainer.AddChild(sLeaser.sprites[num15]);
			}
		}
		for (int num16 = 0; num16 < realizedCreature.legs.Length; num16++)
		{
			LoachLeg loachLeg2 = realizedCreature.legs[num16];
			Vector2 vector12 = Vector2.Lerp(loachLeg2.connectedChunk.lastPos, loachLeg2.connectedChunk.pos, timeStacker);
			Vector2 vector13 = Vector2.Lerp(loachLeg2.tChunks[1].lastPos, loachLeg2.tChunks[1].pos, timeStacker);
			if (num16 % 2 == 0)
			{
				vector13 = Vector2.Lerp(vector13, vector12, 0.2f) + vector * 20f;
				vector12 = Vector2.Lerp(vector12, Vector2.Lerp(realizedCreature.bodyChunks[0].lastPos, realizedCreature.bodyChunks[0].pos, timeStacker), 0.4f) + new Vector2(0f, -20f) + vector * 5f;
			}
			else
			{
				vector12 = Vector2.Lerp(vector12, Vector2.Lerp(realizedCreature.bodyChunks[4].lastPos, realizedCreature.bodyChunks[4].pos, timeStacker), 0.5f) + new Vector2(0f, -5f);
				vector12 -= vector * 17f;
				vector13 -= vector * 10f;
			}
			Vector2 a2 = Custom.InverseKinematic(vector12, vector13, LegPartLength(num16 % 2, 0, includeExtension: true), LegPartLength(num16 % 2, 1, includeExtension: true), ((num16 % 2 == 0) ? 1f : (-1f)) * Mathf.Sign(num));
			a2 = Vector2.Lerp(a2, vector12 + Custom.DirVec(vector12, Vector2.Lerp(loachLeg2.tChunks[0].lastPos, loachLeg2.tChunks[0].pos, timeStacker)) * LegPartLength(num16 % 2, 0, includeExtension: true), 1f - Mathf.Abs(num));
			Vector2 a3 = Vector2.Lerp(loachLeg2.tChunks[4].lastPos, loachLeg2.tChunks[4].pos, timeStacker);
			Vector2 vector14 = Vector2.Lerp(loachLeg2.tChunks[5].lastPos, loachLeg2.tChunks[5].pos, timeStacker);
			a3 = Vector2.Lerp(a3, vector14 + Custom.DirVec(vector14, Vector2.Lerp(loachLeg2.tChunks[4].lastPos, loachLeg2.tChunks[4].pos, timeStacker)) * 40f, 0.5f);
			float num17 = Vector2.Distance(vector13, a3);
			Vector2 a4 = Custom.InverseKinematic(vector13, a3, Mathf.Lerp(num17 / 2f, 80f, 0.5f), Mathf.Lerp(num17 / 2f, 80f, 0.5f), 0f - Mathf.Sign(num));
			a4 = Vector2.Lerp(a4, Vector2.Lerp(Vector2.Lerp(loachLeg2.tChunks[2].lastPos, loachLeg2.tChunks[2].pos, timeStacker), Vector2.Lerp(loachLeg2.tChunks[3].lastPos, loachLeg2.tChunks[3].pos, timeStacker), 0.5f), Mathf.Lerp(1f, 0.5f, Mathf.Abs(num)));
			float num18 = ((num < ((num16 % 2 == 0) ? 0.5f : (-0.5f))) ? (-1f) : 1f);
			if (num16 % 2 == 0 && Mathf.Abs(num) < 0.7f && Mathf.Sign(num) != num18)
			{
				vector12 += Custom.PerpendicularVector((vector12 - a2).normalized) * (50f * num18 * Mathf.InverseLerp(0f, 0.7f, Mathf.Abs(num)));
				vector12 = a2 + Custom.DirVec(a2, vector12) * (LegPartLength(1, 0, includeExtension: true) * (1f - Mathf.Lerp(lastResting, resting, timeStacker)));
			}
			sLeaser.sprites[LegSprite(num16, 0)].x = a2.x - camPos.x;
			sLeaser.sprites[LegSprite(num16, 0)].y = a2.y - camPos.y;
			sLeaser.sprites[LegSprite(num16, 0)].rotation = Custom.AimFromOneVectorToAnother(a2, vector12);
			sLeaser.sprites[LegSprite(num16, 0)].scaleY = Vector2.Distance(vector12, a2) / LegPartLength(num16 % 2, 0, includeExtension: false);
			sLeaser.sprites[LegSprite(num16, 0)].scaleX = num18 * Mathf.Min(1f, Vector2.Distance(vector12, a2) / LegPartLength(num16 % 2, 0, includeExtension: false));
			sLeaser.sprites[LegSprite(num16, 1)].x = vector13.x - camPos.x;
			sLeaser.sprites[LegSprite(num16, 1)].y = vector13.y - camPos.y;
			sLeaser.sprites[LegSprite(num16, 1)].rotation = Custom.AimFromOneVectorToAnother(vector13, a2);
			sLeaser.sprites[LegSprite(num16, 1)].scaleY = Vector2.Distance(a2, vector13) / LegPartLength(num16 % 2, 1, includeExtension: false);
			sLeaser.sprites[LegSprite(num16, 1)].scaleX = num18 * Mathf.Min(1f, Vector2.Distance(a2, vector13) / LegPartLength(num16 % 2, 1, includeExtension: false));
			Vector2 vector15 = a2;
			for (int num19 = 0; num19 < 7; num19++)
			{
				Vector2 vector16 = a2;
				float num20 = Mathf.Lerp(5f, 1f, (float)num19 / 6f);
				float num21 = Mathf.Lerp(5f, 1f, ((float)num19 + 0.5f) / 6f);
				switch (num19)
				{
				case 0:
					vector16 = vector13 - Custom.DirVec(a2, a4) * 2.5f;
					num20 = 0f;
					num21 = 4f;
					break;
				case 1:
					vector16 = vector13 + Custom.DirVec(a2, a4) * 7.5f;
					num20 = 4f;
					num21 = 6f;
					break;
				case 2:
					vector16 = a4 - Custom.DirVec(vector13, a3) * 7.5f;
					break;
				case 3:
					vector16 = a4 + Custom.DirVec(vector13, a3) * 7.5f;
					num21 = 4.5f;
					break;
				case 4:
					vector16 = a3 - Custom.DirVec(a4, vector14) * 7.5f;
					break;
				case 5:
					vector16 = a3 + Custom.DirVec(a4, vector14) * 7.5f;
					num20 = 3f;
					num21 = 2f;
					break;
				case 6:
					vector16 = vector14;
					break;
				}
				Vector2 normalized3 = (vector16 - vector15).normalized;
				Vector2 vector17 = Custom.PerpendicularVector(normalized3);
				float num22 = Vector2.Distance(vector16, vector15) / 5f;
				TriangleMesh obj2 = sLeaser.sprites[LegSprite(num16, 2)] as TriangleMesh;
				obj2.MoveVertice(num19 * 4 + 2, vector16 - vector17 * num21 - normalized3 * num22 - camPos);
				obj2.MoveVertice(num19 * 4 + 3, vector16 + vector17 * num21 - normalized3 * num22 - camPos);
				if (num19 == 6)
				{
					num22 = Vector2.Distance(vector16, vector15);
				}
				obj2.MoveVertice(num19 * 4, vector15 - vector17 * num20 + normalized3 * num22 - camPos);
				obj2.MoveVertice(num19 * 4 + 1, vector15 + vector17 * num20 + normalized3 * num22 - camPos);
				vector15 = vector16;
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public float CurrentFaceDir(float timeStacker)
	{
		return Mathf.Lerp(lastFlip, flip, timeStacker);
	}

	public Vector2 AlongBodyPos(float x, float timeStacker)
	{
		BodyChunk[] bodyChunks = realizedCreature.bodyChunks;
		int num = bodyChunks.Length - 1;
		if (x <= bodyChunks[0].rad)
		{
			x -= bodyChunks[0].rad;
			Vector2 vector = Vector2.Lerp(bodyChunks[0].lastPos, bodyChunks[0].pos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(bodyChunks[1].lastPos, bodyChunks[1].pos, timeStacker);
			return vector + (vector2 - vector).normalized * x;
		}
		if (x >= bodyLength - bodyChunks[num].rad)
		{
			x -= bodyLength - bodyChunks[num].rad;
			Vector2 vector3 = Vector2.Lerp(bodyChunks[num].lastPos, bodyChunks[num].pos, timeStacker);
			Vector2 vector4 = Vector2.Lerp(bodyChunks[num - 1].lastPos, bodyChunks[num - 1].pos, timeStacker);
			return vector3 + (vector3 - vector4).normalized * x;
		}
		PhysicalObject.BodyChunkConnection[] bodyChunkConnections = realizedCreature.bodyChunkConnections;
		x -= bodyChunks[0].rad;
		int i;
		for (i = 0; i < bodyChunks.Length - 2 && x > bodyChunkConnections[i].distance; i++)
		{
			x -= bodyChunkConnections[i].distance;
		}
		Vector2 a = Vector2.Lerp(bodyChunks[i].lastPos, bodyChunks[i].pos, timeStacker);
		Vector2 b = Vector2.Lerp(bodyChunks[i + 1].lastPos, bodyChunks[i + 1].pos, timeStacker);
		return Vector2.Lerp(a, b, x / bodyChunkConnections[i].distance);
	}

	public float AlongBodyRad(float x, float timeStacker, bool useBreath = true)
	{
		BodyChunk[] bodyChunks = realizedCreature.bodyChunks;
		int num = bodyChunks.Length - 1;
		float num2 = (useBreath ? GetChunkVisualRad(0, timeStacker) : bodyChunks[0].rad);
		float num3 = (useBreath ? GetChunkVisualRad(num, timeStacker) : bodyChunks[num].rad);
		if (x < bodyChunks[0].rad)
		{
			x -= bodyChunks[0].rad;
			return Mathf.Sqrt(1f - Mathf.Pow(Mathf.Max(x / bodyChunks[0].rad, -1f), 2f)) * num2;
		}
		if (x > bodyLength - bodyChunks[num].rad)
		{
			x -= bodyLength - bodyChunks[num].rad;
			return Mathf.Sqrt(1f - Mathf.Pow(Mathf.Min(x / num3, 1f), 2f)) * num3;
		}
		PhysicalObject.BodyChunkConnection[] bodyChunkConnections = realizedCreature.bodyChunkConnections;
		x -= bodyChunks[0].rad;
		int i;
		for (i = 0; i < bodyChunks.Length - 2 && x > bodyChunkConnections[i].distance; i++)
		{
			x -= bodyChunkConnections[i].distance;
		}
		float a = (useBreath ? GetChunkVisualRad(i, timeStacker) : bodyChunks[i].rad);
		float b = (useBreath ? GetChunkVisualRad(i + 1, timeStacker) : bodyChunks[i + 1].rad);
		return Mathf.Lerp(a, b, x / bodyChunkConnections[i].distance);
	}

	public float GetChunkVisualRad(int index, float timeStacker)
	{
		float num = (float)index / (float)(realizedCreature.bodyChunks.Length - 1);
		float num2 = Mathf.Lerp(lastBreathFactor, breathFactor, timeStacker);
		return realizedCreature.bodyChunks[index].rad * (1f + 0.1f * Mathf.Sin(num * (float)Math.PI) * num2);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		bodyColor = (Rotted ? palette.blackColor : Color.Lerp(palette.blackColor, Color.white, 0.1f));
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = bodyColor;
		}
		DaddyGraphics.Eye[] eyes = Eyes;
		for (int j = 0; j < eyes.Length; j++)
		{
			eyes[j].ApplyPalette(sLeaser, rCam, palette);
		}
		if (!Rotted)
		{
			Color color = Color.Lerp(bodyColor, palette.blackColor, 0.5f);
			Color color2 = Color.Lerp(new Color(0.003921569f, 0.003921569f, 0.003921569f), Color.white, 0.1f);
			for (int k = 0; k < 2; k++)
			{
				sLeaser.sprites[firstPeeperSprite + k * 2].color = color;
				sLeaser.sprites[firstPeeperSprite + k * 2 + 1].color = color2;
			}
		}
		LoachTentacleGraphic[] array = tentacleGraphics;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].ApplyPalette(sLeaser, rCam, palette);
		}
		for (int l = 0; l < spots.Length; l++)
		{
			sLeaser.sprites[firstSpotSprite + l].color = bodyColor;
			sLeaser.sprites[firstSpotSprite + l + spots.Length].color = Color.Lerp(bodyColor, EffectColor, spots[l].lightness);
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FContainer[] containers = sLeaser.containers;
		foreach (FContainer node in containers)
		{
			newContatiner.AddChild(node);
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node2 in sprites)
		{
			newContatiner.AddChild(node2);
		}
		sLeaser.containers[1].MoveInFrontOfOtherNode(sLeaser.sprites[firstSpotSprite - 1]);
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score * (1f + realizedCreature.AI.CreatureUnease(crit.representedCreature));
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}

	public Vector2 DanglerConnection(int index, float timeStacker)
	{
		DanglerAttachment danglerAttachment = danglerAttachments[index];
		Vector2 a = Vector2.Lerp(realizedCreature.bodyChunks[danglerAttachment.firstChunk].lastPos, realizedCreature.bodyChunks[danglerAttachment.firstChunk].pos, timeStacker);
		Vector2 b = Vector2.Lerp(realizedCreature.bodyChunks[danglerAttachment.firstChunk + 1].lastPos, realizedCreature.bodyChunks[danglerAttachment.firstChunk + 1].pos, timeStacker);
		return Vector2.Lerp(a, b, danglerAttachment.lerp) + danglerAttachment.offset;
	}

	public Dangler.DanglerProps Props(int index)
	{
		return danglerProps;
	}

	void DaddyGraphics.IHaveRotGraphics.TentOwnerFeelSomethingWithTentacle(Tracker.CreatureRepresentation creatureRep, Vector2 feelPos)
	{
	}

	float DaddyGraphics.IHaveRotGraphics.TubeRadius(DaddyGraphics.DaddyTubeGraphic tube)
	{
		return 5f;
	}

	Color DaddyGraphics.IHaveRotGraphics.TubeEffectColor(DaddyGraphics.DaddyTubeGraphic tube)
	{
		return EffectColor;
	}

	Color DaddyGraphics.IHaveRotGraphics.TubeBumpColor(DaddyGraphics.DaddyTubeGraphic tube)
	{
		return EffectColor;
	}

	Vector2 DaddyGraphics.IHaveRotGraphics.TubeDeadLegPushOutFromPos(DaddyGraphics.DaddyTubeGraphic tube)
	{
		return default(Vector2);
	}

	Color DaddyGraphics.IHaveRotGraphics.RotBodyColor()
	{
		return bodyColor;
	}

	Color DaddyGraphics.IHaveRotGraphics.EyeBulbColor(int index)
	{
		return bodyColor;
	}

	Vector2 DaddyGraphics.IHaveRotGraphics.EyePosition(int index, float timeStacker)
	{
		return eyeData[index].DrawPos(timeStacker);
	}

	Vector2 DaddyGraphics.IHaveRotGraphics.EyePushOutFromPos(int index, float timeStacker)
	{
		return eyeData[index].PushOutFromPos(timeStacker);
	}
}
