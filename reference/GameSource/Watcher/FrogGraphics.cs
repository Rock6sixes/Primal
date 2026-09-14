using RWCustom;
using UnityEngine;

namespace Watcher;

public class FrogGraphics : GraphicsModule, ILookingAtCreatures
{
	public class Umbilical : CosmeticSprite
	{
		public struct Segment
		{
			public Vector2 pos;

			public Vector2 lastPos;

			public Vector2 vel;

			public float life;

			public float lifeTime;
		}

		public int bodypartToUse = 7;

		public float attachRad;

		public float attachAng;

		public int feedSuckCounter = 100;

		public int feedSuck;

		public Segment[] points;

		public Creature frog;

		public Creature attachedCreature;

		public Color fogColor;

		public Color threadCol;

		public Color blackColor;

		public Vector2 AttachPos
		{
			get
			{
				BodyChunk bodyChunk = attachedCreature.bodyChunks[bodypartToUse];
				return bodyChunk.pos + Custom.DegToVec(attachAng) * (bodyChunk.rad * attachRad);
			}
		}

		public Vector2 LastAttachPos
		{
			get
			{
				BodyChunk bodyChunk = attachedCreature.bodyChunks[bodypartToUse];
				return bodyChunk.lastPos + Custom.DegToVec(attachAng) * (bodyChunk.rad * attachRad);
			}
		}

		public bool killUmbilical
		{
			get
			{
				if (frog.grasps[0] != null)
				{
					return frog.dead;
				}
				return true;
			}
		}

		public Umbilical(Room room, Frog frog, Creature attachedCreature, Vector2 shootVel)
			: this(room, frog, attachedCreature, Random.Range(0, attachedCreature.bodyChunks.Length - 1), shootVel)
		{
		}

		public Umbilical(Room room, Frog frog, Creature attachedCreature, int bodyPartToUse, Vector2 shootVel)
		{
			base.room = room;
			this.frog = frog;
			this.attachedCreature = attachedCreature;
			attachRad = Random.Range(0f, 0.25f);
			attachAng = Random.Range(0f, 360f);
			points = new Segment[Random.Range(5, 8)];
			bodypartToUse = bodyPartToUse;
			for (int i = 0; i < points.Length; i++)
			{
				points[i].pos = AttachPos + Custom.RNV();
				points[i].lastPos = AttachPos + Custom.RNV();
				points[i].vel = shootVel * (0.3f * Random.value) + Custom.RNV() * (Random.value * 1.5f);
				points[i].life = 2f;
				points[i].lifeTime = Mathf.Lerp(150f, 200f, Mathf.Pow(Random.value, 0.3f));
			}
		}

		public float LifeOfSegment(int i)
		{
			if (i > 0)
			{
				return Mathf.Min(points[i].life, points[i - 1].life);
			}
			return points[i].life;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			bool flag = true;
			for (int i = 0; i < points.Length; i++)
			{
				points[i].lastPos = points[i].pos;
				points[i].pos += points[i].vel;
				points[i].vel *= Custom.LerpMap(points[i].vel.magnitude, 1f, 30f, 0.99f, 0.8f);
				points[i].vel.y -= Mathf.Lerp(0.1f, 0.6f, LifeOfSegment(i));
				if (!(LifeOfSegment(i) > 0f))
				{
					continue;
				}
				if (room.GetTile(points[i].pos).Solid)
				{
					SharedPhysics.TerrainCollisionData cd = new SharedPhysics.TerrainCollisionData(points[i].pos, points[i].lastPos, points[i].vel, 1f, default(IntVector2), goThroughFloors: true);
					cd = SharedPhysics.HorizontalCollision(room, cd);
					cd = SharedPhysics.VerticalCollision(room, cd);
					points[i].pos = cd.pos;
					points[i].vel = cd.vel;
				}
				if (i > 0)
				{
					if (!Custom.DistLess(points[i].pos, points[i - 1].pos, 6f))
					{
						Vector2 vector = Custom.DirVec(points[i].pos, points[i - 1].pos) * (Vector2.Distance(points[i].pos, points[i - 1].pos) - 6f);
						points[i].pos += vector * 0.15f;
						points[i].vel += vector * 0.25f;
						points[i - 1].pos -= vector * 0.15f;
						points[i - 1].vel -= vector * 0.25f;
					}
					if (!room.VisualContact(points[i].pos, points[i - 1].pos) && killUmbilical)
					{
						points[i].life -= 0.2f;
					}
				}
				if (i > 1 && LifeOfSegment(i - 1) > 0f)
				{
					points[i].vel += Custom.DirVec(points[i - 2].pos, points[i].pos) * 0.6f;
					points[i - 2].vel -= Custom.DirVec(points[i - 2].pos, points[i].pos) * 0.6f;
				}
				if (killUmbilical)
				{
					points[i].life -= 1f / points[i].lifeTime;
				}
				if (points[i].life > 0f)
				{
					flag = false;
				}
			}
			if (LifeOfSegment(0) > 0f)
			{
				points[0].pos = AttachPos;
				points[0].vel *= 0f;
				points[1].vel += Custom.DirVec(AttachPos, AttachPos * 3f);
				points[3].vel += Custom.DirVec(AttachPos, AttachPos * 1.5f);
				if (LifeOfSegment(1) <= 0f || attachedCreature.enteringShortCut.HasValue || attachedCreature.room != room)
				{
					points[0].life -= 1f;
				}
			}
			if (LifeOfSegment(points.Length - 1) > 0f)
			{
				Vector2 rotation = frog.mainBodyChunk.Rotation;
				points[points.Length - 1].pos = frog.bodyChunks[0].pos;
				points[points.Length - 1].vel = frog.bodyChunks[0].vel;
				points[points.Length - 2].vel += rotation * -2f + new Vector2(0f, -1.5f);
				points[points.Length - 3].vel += rotation * -1f + new Vector2(0f, -1f);
				if (frog.slatedForDeletetion || frog.room != room)
				{
					points[points.Length - 1].life -= 1f;
				}
			}
			if (flag)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(points.Length, pointyTip: false, customColor: true);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = ((!(LifeOfSegment(0) > 0f) || attachedCreature == null) ? Vector2.Lerp(points[0].lastPos, points[0].pos, timeStacker) : Vector2.Lerp(LastAttachPos, AttachPos, timeStacker));
			float num = 0f;
			float b = 1f;
			bool flag = rCam.room.Darkness(pos) > 0.2f;
			TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
			for (int i = 0; i < points.Length; i++)
			{
				float f = Mathf.InverseLerp(0f, points.Length - 1, i);
				float num2 = LifeOfSegment(i);
				if (i < points.Length - 1)
				{
					num2 = Mathf.Min(num2, LifeOfSegment(i + 1));
				}
				float num3 = 0.5f * Mathf.InverseLerp(0f, 0.3f, num2);
				Vector2 vector2 = Vector2.Lerp(points[i].lastPos, points[i].pos, timeStacker);
				if (i == 0 && LifeOfSegment(0) > 0f)
				{
					vector2 = vector;
				}
				else if (i == points.Length - 1 && LifeOfSegment(i) > 0f)
				{
					Vector2 rotation = frog.mainBodyChunk.Rotation;
					vector2 = Vector2.Lerp(frog.bodyChunks[0].lastPos - rotation * 5f, frog.bodyChunks[0].pos - rotation * 5f, timeStacker);
				}
				Vector2 vector3 = Custom.PerpendicularVector(vector2, vector);
				Vector2 vector4 = vector3 * ((num3 + num) * 0.5f);
				triangleMesh.MoveVertice(i * 4, (vector + vector2) / 2f - vector4 - camPos);
				triangleMesh.MoveVertice(i * 4 + 1, (vector + vector2) / 2f + vector4 - camPos);
				triangleMesh.MoveVertice(i * 4 + 2, vector2 - vector3 * num3 - camPos);
				triangleMesh.MoveVertice(i * 4 + 3, vector2 + vector3 * num3 - camPos);
				Color color = Color.Lerp(fogColor, Color.Lerp(new Color(1f, 0f, 0f), Color.Lerp(threadCol, new Color(1f, 1f, 1f), rCam.room.WaterShinyness(vector2, timeStacker)), 0.1f + 0.9f * Mathf.Pow(f, 0.25f + num2)), Mathf.Min(num2, b));
				if (flag && num2 > 0f)
				{
					color = Color.Lerp(color, blackColor, rCam.room.DarknessOfPoint(rCam, vector2));
				}
				for (int j = 0; j < 4; j++)
				{
					(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + j] = color;
				}
				vector = vector2;
				num = num3;
				b = num2;
			}
			if (feedSuckCounter > 0)
			{
				feedSuckCounter--;
			}
			else
			{
				feedSuckCounter = Random.Range(100, 150);
				if (feedSuck < points.Length)
				{
					feedSuck++;
				}
				else
				{
					feedSuck = 0;
				}
			}
			for (int k = 0; k < 4; k++)
			{
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[k] = Color.Lerp(fogColor, blackColor, LifeOfSegment(0));
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			blackColor = palette.blackColor;
			fogColor = palette.fogColor;
			threadCol = Color.Lerp(new Color(0.95f, 0.8f, 0.55f), palette.fogColor, 0.2f);
		}
	}

	public CreatureLooker creatureLooker;

	public Limb[] limbs;

	public Vector2[] kick;

	public float roll;

	public float lastRoll;

	public float lastSmoothedSitting;

	public float smoothedSitting;

	public int walkCounter;

	public int movingLeg;

	public Color bodyColor;

	public Color bellyColor;

	public Color bloodColor;

	public Color eyeColor;

	public Color blackColor;

	public Color legBloodColor = new Color(2f / 85f, 4f / 51f, 1f / 85f);

	public float darkness;

	public float lastDarkness;

	public float hornScale;

	public int horns;

	public Vector2[] relEyePos = new Vector2[2]
	{
		new Vector2(2f, 4f),
		new Vector2(4.5f, 0f)
	};

	public Vector2 relHipPos = new Vector2(1.5f, -1f);

	public Vector2 relShoulderPos = new Vector2(1.5f, 1.5f);

	public float legLength = 12f;

	public float armLength = 8f;

	public int minStepDuration = 5;

	public int firstBodySprite;

	public int firstEyeSprite;

	public int firstLegSprite;

	public int firstArmSprite;

	public int firstHornSprite;

	public int totalSprites;

	public int kickCounter;

	public Frog realizedCreature => base.owner as Frog;

	public int EyeSprite(int side, int index, int part)
	{
		return firstEyeSprite + part * 4 + index * 2 + side;
	}

	public FrogGraphics(PhysicalObject po)
		: base(po, internalContainers: false)
	{
		Random.State state = Random.state;
		Random.InitState(realizedCreature.abstractCreature.ID.RandomSeed);
		GenerateColors();
		horns = Mathf.RoundToInt(3f * Random.value);
		hornScale = Random.Range(0.8f, 1.2f);
		Random.state = state;
		limbs = new Limb[4];
		for (int i = 0; i < limbs.Length; i++)
		{
			limbs[i] = new Limb(this, realizedCreature.bodyChunks[(i >= 2) ? 1u : 0u], i, 0.5f, 0.5f, 0.98f, 4f, 0.5f);
		}
		kick = new Vector2[2];
		bodyParts = new BodyPart[4];
		for (int j = 0; j < 4; j++)
		{
			bodyParts[j] = limbs[j];
		}
		creatureLooker = new CreatureLooker(this, realizedCreature.AI.tracker, realizedCreature, 0f, 30);
		firstHornSprite = totalSprites;
		totalSprites += horns * 2;
		firstLegSprite = totalSprites;
		totalSprites += 2;
		firstArmSprite = totalSprites;
		totalSprites += 2;
		firstBodySprite = totalSprites;
		totalSprites += 4;
		firstEyeSprite = totalSprites;
		totalSprites += 8;
	}

	public void GenerateColors()
	{
		HSLColor hSLColor;
		HSLColor hSLColor2;
		HSLColor hSLColor3;
		if (Random.value < 0.05f)
		{
			hSLColor = new HSLColor(0.2f, 0.17f, 0.15f);
			hSLColor2 = new HSLColor(0.12f, 0.09f, 0.41f);
			hSLColor3 = hSLColor;
			hSLColor3.lightness += 0.5f;
		}
		else
		{
			hSLColor = new HSLColor(0.06f, 0.47f, 0.38f);
			hSLColor2 = new HSLColor(0.09f, 0.45f, 0.74f);
			hSLColor3 = hSLColor;
		}
		new HSLColor(0.01f, 0.94f, 0.31f);
		float num = Custom.ClampedRandomVariation(0.5f, 0.1f, 0.2f) - 0.5f;
		hSLColor.hue = Mathf.Repeat(hSLColor.hue + num, 1f);
		hSLColor.saturation = Custom.ClampedRandomVariation(hSLColor.saturation, 0.1f, 0.1f);
		hSLColor.lightness = Custom.ClampedRandomVariation(hSLColor.lightness, 0.2f, 0.1f);
		hSLColor2.hue = Mathf.Repeat(hSLColor2.hue + num, 1f);
		hSLColor2.saturation = Custom.ClampedRandomVariation(hSLColor2.saturation, 0.1f, 0.1f);
		hSLColor2.lightness = Custom.ClampedRandomVariation(hSLColor2.lightness, 0.2f, 0.1f);
		bodyColor = hSLColor.rgb;
		bellyColor = hSLColor2.rgb;
		eyeColor = hSLColor3.rgb;
		bloodColor = new Color(0.09f, 0.36f, 0.03f);
	}

	public void ChangeOverlap(bool attached)
	{
		RoomCamera[] cameras = realizedCreature.room.game.cameras;
		foreach (RoomCamera roomCamera in cameras)
		{
			roomCamera.MoveObjectToContainer(this, roomCamera.ReturnFContainer(attached ? "Background" : "Midground"));
		}
	}

	public Vector2 GetLegsUp(float timeStacker)
	{
		Vector2 vector = Vector2.Lerp(realizedCreature.bodyChunks[0].lastPos, realizedCreature.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(realizedCreature.bodyChunks[1].lastPos, realizedCreature.bodyChunks[1].pos, timeStacker);
		return Vector3.Slerp((vector - vector2).normalized, Vector2.up, Mathf.Lerp(lastSmoothedSitting, smoothedSitting, timeStacker));
	}

	public Vector2 GetArmsDir(float timeStacker)
	{
		Vector2 vector = Vector2.Lerp(realizedCreature.bodyChunks[0].lastPos, realizedCreature.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(realizedCreature.bodyChunks[1].lastPos, realizedCreature.bodyChunks[1].pos, timeStacker);
		return Vector3.Slerp((vector - vector2).normalized, Vector2.down, Mathf.Lerp(lastSmoothedSitting, smoothedSitting, timeStacker) * 0.5f + 0.5f);
	}

	public Vector2 GetHipPos(int leg, float timeStacker)
	{
		Vector2 vector = Vector2.Lerp(realizedCreature.bodyChunks[1].lastPos, realizedCreature.bodyChunks[1].pos, timeStacker);
		Vector2 legsUp = GetLegsUp(timeStacker);
		Vector2 vector2 = -Custom.PerpendicularVector(legsUp);
		float num = (float)leg * 2f - 1f;
		return vector + vector2 * (relHipPos.x * num) + legsUp * relHipPos.y;
	}

	public Vector2 GetShoulderPos(int leg, float timeStacker)
	{
		Vector2 vector = Vector2.Lerp(realizedCreature.bodyChunks[0].lastPos, realizedCreature.bodyChunks[0].pos, timeStacker);
		Vector2 legsUp = GetLegsUp(timeStacker);
		Vector2 vector2 = -Custom.PerpendicularVector(legsUp);
		float num = (float)leg * 2f - 1f;
		return vector + vector2 * (relShoulderPos.x * num) + legsUp * relShoulderPos.y;
	}

	public void StartStep(int leg)
	{
		walkCounter = 0;
		movingLeg = leg;
		float num = (float)leg * 2f - 1f;
		Vector2 pos = realizedCreature.bodyChunks[0].pos;
		Vector2 pos2 = realizedCreature.bodyChunks[1].pos;
		Vector2 normalized = (pos - pos2).normalized;
		Vector2 vector = -Custom.PerpendicularVector(normalized);
		int forbiddenXDirs = -(int)Mathf.Sign(realizedCreature.mainBodyChunk.vel.x);
		int forbiddenYDirs = -(int)Mathf.Sign(realizedCreature.mainBodyChunk.vel.y);
		Vector2 hipPos = GetHipPos(leg, 1f);
		Vector2 goalPos = pos2 + vector * (5f * num) + realizedCreature.walkDir * 10f;
		limbs[leg].FindGrip(realizedCreature.room, hipPos, pos2, legLength * 3f, goalPos, forbiddenXDirs, forbiddenYDirs, behindWalls: true);
		hipPos = GetShoulderPos(leg, 1f);
		goalPos = pos + normalized * 10f + vector * (5f * num) + realizedCreature.walkDir * 10f;
		limbs[3 - leg].FindGrip(realizedCreature.room, hipPos, pos, armLength * (3f + Mathf.Lerp(0f, 2f, realizedCreature.FrogState.feedSize / realizedCreature.maxFeeds)), goalPos, forbiddenXDirs, forbiddenYDirs, behindWalls: true);
	}

	public override void Update()
	{
		base.Update();
		creatureLooker.Update();
		lastSmoothedSitting = smoothedSitting;
		smoothedSitting = Mathf.MoveTowards(smoothedSitting, realizedCreature.sitting, 0.4f);
		Vector2 legsUp = GetLegsUp(1f);
		Vector2 vector = -Custom.PerpendicularVector(legsUp);
		Vector2 pos = realizedCreature.bodyChunks[0].pos;
		Vector2 pos2 = realizedCreature.bodyChunks[1].pos;
		Vector2 normalized = (pos - pos2).normalized;
		lastRoll = roll;
		float to = ((realizedCreature.attachedObject != null || !realizedCreature.Consious) ? 0f : Mathf.Clamp(normalized.x * 2f, -1f, 1f));
		roll = Custom.LerpAndTick(roll, to, 0.1f, 0.05f);
		if (realizedCreature.walking)
		{
			walkCounter++;
		}
		float num = Mathf.Min((float)realizedCreature.timeAttached / 120f, 1f);
		for (int i = 0; i < 2; i++)
		{
			if (num == 1f)
			{
				if (realizedCreature.KickUntilDetached)
				{
					kickCounter++;
					if (Random.value < 0.075f)
					{
						StartKick(i);
						realizedCreature.room.AddObject(new WaterDrip(Vector2.Lerp(realizedCreature.bodyChunks[1].pos, realizedCreature.bodyChunks[0].pos, Random.value), new Vector2(Random.Range(-3f, 3f), 5f), waterColor: false));
					}
				}
				else if (Random.value < 0.002f)
				{
					StartKick(i);
				}
			}
			else if (num > 0f && kick[i].magnitude < 0.5f && Random.value < (1f - num) * 0.1f)
			{
				StartKick(i);
			}
			kick[i] = Vector2.MoveTowards(kick[i], Vector2.zero, 0.2f);
		}
		for (int j = 0; j < 2; j++)
		{
			float num2 = (float)j * 2f - 1f;
			Limb limb = limbs[j];
			Vector2 hipPos = GetHipPos(j, 1f);
			if (realizedCreature.attachedObject != null)
			{
				limb.mode = Limb.Mode.Dangle;
				limb.vel += legsUp * -1f + vector * (num2 * 0.3f) + kick[j] * 3f;
			}
			else if (realizedCreature.swimming)
			{
				limb.mode = Limb.Mode.HuntRelativePosition;
				float num3 = Mathf.Pow(1f - Mathf.InverseLerp(30f, 23f, realizedCreature.noJumps), 1.5f);
				if (realizedCreature.noJumps < 20)
				{
					num3 = 1f;
				}
				limb.relativeHuntPos = Custom.DegToVec((130f + num3 * 40f) * ((j == 0) ? (-1f) : 1f)) * 20f;
			}
			else if (realizedCreature.walking)
			{
				if (movingLeg != j && !Custom.DistLess(limb.absoluteHuntPos, hipPos, legLength * 0.9f) && walkCounter > minStepDuration)
				{
					StartStep(j);
				}
			}
			else if (!realizedCreature.Consious)
			{
				limb.mode = Limb.Mode.Dangle;
			}
			else if (smoothedSitting == 0f)
			{
				limb.mode = Limb.Mode.Dangle;
				limb.vel += vector * 0.6f + legsUp * -3f;
			}
			else
			{
				limb.mode = Limb.Mode.HuntAbsolutePosition;
				limb.absoluteHuntPos = hipPos - legsUp * 8f;
			}
			limb.Update();
			limb.ConnectToPoint(hipPos, legLength, push: false, 0.1f, realizedCreature.bodyChunks[1].vel, 0.2f, 0f);
		}
		Vector2 armsDir = GetArmsDir(1f);
		for (int k = 0; k < 2; k++)
		{
			float num4 = (float)k * 2f - 1f;
			Limb limb2 = limbs[k + 2];
			Vector2 shoulderPos = GetShoulderPos(k, 1f);
			limb2.retract = false;
			if (realizedCreature.swimming)
			{
				limb2.mode = Limb.Mode.HuntRelativePosition;
				float num5 = Mathf.Pow(1f - Mathf.InverseLerp(20f, 30f, realizedCreature.noJumps), 1.5f);
				limb2.relativeHuntPos = Custom.DegToVec((5f + num5 * 170f) * ((k == 1) ? (-1f) : 1f)) * 20f;
			}
			else if (!realizedCreature.walking)
			{
				if (realizedCreature.attachedObject != null)
				{
					limb2.retract = true;
				}
				else if (!realizedCreature.Consious)
				{
					limb2.mode = Limb.Mode.Dangle;
				}
				else if (smoothedSitting == 0f)
				{
					limb2.mode = Limb.Mode.Dangle;
					limb2.vel += vector * (num4 * 0.6f) + armsDir * 3f;
				}
				else
				{
					limb2.mode = Limb.Mode.HuntAbsolutePosition;
					limb2.absoluteHuntPos = shoulderPos + armsDir * 8f;
				}
			}
			limb2.Update();
			limb2.ConnectToPoint(shoulderPos, armLength, push: false, 0.1f, realizedCreature.bodyChunks[0].vel, 0.2f, 0f);
		}
		for (int l = 0; l < limbs.Length; l++)
		{
			if (limbs[l].mode == Limb.Mode.Dangle)
			{
				limbs[l].vel.y -= 0.5f * (1f - realizedCreature.Submersion);
			}
		}
	}

	public void DoneWithJump()
	{
		Vector2 legsUp = GetLegsUp(1f);
		Vector2 armsDir = GetArmsDir(1f);
		for (int i = 0; i < 2; i++)
		{
			Vector2 shoulderPos = GetShoulderPos(i, 1f);
			Limb obj = limbs[i + 2];
			Limb obj2 = limbs[i];
			Vector2 hipPos = GetHipPos(i, 1f);
			obj2.mode = Limb.Mode.HuntAbsolutePosition;
			obj2.absoluteHuntPos = hipPos - legsUp * 8f;
			obj2.Update();
			obj2.ConnectToPoint(hipPos, legLength, push: false, 0.1f, realizedCreature.bodyChunks[1].vel, 0.2f, 0f);
			StartStep(i);
			obj.mode = Limb.Mode.HuntAbsolutePosition;
			obj.absoluteHuntPos = shoulderPos + armsDir * 8f;
			obj.Update();
			obj.ConnectToPoint(shoulderPos, armLength, push: false, 0.1f, realizedCreature.bodyChunks[0].vel, 0.2f, 0f);
		}
	}

	public void StartKick(int leg)
	{
		Vector2 normalized = (realizedCreature.bodyChunks[0].pos - realizedCreature.bodyChunks[1].pos).normalized;
		kick[leg] = Custom.RotateAroundOrigo(normalized, Random.Range(10f, 30f) * ((float)leg * 2f - 1f));
	}

	public override void Reset()
	{
		base.Reset();
		BodyPart[] array = bodyParts;
		foreach (BodyPart obj in array)
		{
			obj.vel *= 0f;
			obj.pos = realizedCreature.mainBodyChunk.pos;
			obj.lastPos = obj.pos;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[firstBodySprite] = new FSprite("Circle20")
		{
			scaleX = 0.4f,
			scaleY = 0.55f
		};
		sLeaser.sprites[firstBodySprite + 1] = new FSprite("Circle20")
		{
			scaleX = 0.45f,
			scaleY = 0.55f
		};
		sLeaser.sprites[firstBodySprite + 2] = new FSprite("Circle20");
		sLeaser.sprites[firstBodySprite + 3] = new FSprite("Circle20");
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[EyeSprite(i, 0, 0)] = new FSprite("tinyStar");
			sLeaser.sprites[EyeSprite(i, 0, 1)] = new FSprite("pixel");
			sLeaser.sprites[EyeSprite(i, 1, 0)] = new FSprite("Circle4");
			sLeaser.sprites[EyeSprite(i, 1, 1)] = new FSprite("pixel")
			{
				scale = 2f
			};
		}
		sLeaser.sprites[firstLegSprite] = new FSprite("FrogLeg00");
		sLeaser.sprites[firstLegSprite + 1] = new FSprite("FrogLeg00");
		sLeaser.sprites[firstArmSprite] = new FSprite("FrogLeg00");
		sLeaser.sprites[firstArmSprite + 1] = new FSprite("FrogLeg00");
		for (int j = 0; j < horns; j++)
		{
			sLeaser.sprites[firstHornSprite + j * 2] = new FSprite("LizardScaleA0")
			{
				anchorY = 0f
			};
			sLeaser.sprites[firstHornSprite + j * 2 + 1] = new FSprite("LizardScaleB0")
			{
				anchorY = 0f
			};
		}
		string layerName = ((realizedCreature.attachedObject != null) ? "Background" : "Midground");
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(layerName));
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		Vector2 vector = Vector2.Lerp(realizedCreature.bodyChunks[0].lastPos, realizedCreature.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(realizedCreature.bodyChunks[1].lastPos, realizedCreature.bodyChunks[1].pos, timeStacker);
		vector2 += Custom.PerpendicularVector(vector, vector2) * 2f;
		Vector2 normalized = (vector - vector2).normalized;
		Vector2 vector3 = -Custom.PerpendicularVector(normalized);
		float rotation = Custom.VecToDeg(normalized);
		float num = Mathf.Lerp(lastRoll, roll, timeStacker);
		float bloodBankBloat = realizedCreature.bloodBankBloat;
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector2);
		if (darkness > 0.5f)
		{
			darkness = Mathf.Lerp(darkness, 0.5f, rCam.room.LightSourceExposure(vector2));
		}
		if (lastDarkness != darkness || realizedCreature.injectedPoison > 0f || realizedCreature.FrogState.feedSize > 0)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		sLeaser.sprites[firstBodySprite].SetPosition(vector + ScalableFeatures(normalized, bloodBankBloat) + normalized * 1f - camPos);
		sLeaser.sprites[firstBodySprite].rotation = rotation;
		sLeaser.sprites[firstBodySprite].isVisible = realizedCreature.bites > 2;
		sLeaser.sprites[firstBodySprite + 1].SetPosition(vector2 + normalized * 2f - camPos);
		sLeaser.sprites[firstBodySprite + 1].rotation = rotation;
		sLeaser.sprites[firstBodySprite + 1].isVisible = realizedCreature.bites > 2;
		float croaking = realizedCreature.Croaking;
		float num2 = croaking * (1f - Mathf.Pow(croaking, 4f));
		sLeaser.sprites[firstBodySprite + 2].SetPosition((vector + vector2) / 2f + vector3 * (1.5f * num) - ScalableFeatures(normalized, bloodBankBloat) - camPos);
		sLeaser.sprites[firstBodySprite + 2].scale = (1f + bloodBankBloat + num2 * 0.5f) * 4f / 20f;
		sLeaser.sprites[firstBodySprite + 3].SetPosition((vector + vector2) / 2f + vector3 * ((2.5f + num2 * 5f) * num) - ScalableFeatures(normalized, bloodBankBloat) + normalized * 3f - camPos);
		sLeaser.sprites[firstBodySprite + 3].scale = (1f + bloodBankBloat + num2 * 2f) * 5f / 20f;
		sLeaser.sprites[firstBodySprite + 2].isVisible = realizedCreature.bites > 2;
		sLeaser.sprites[firstBodySprite + 3].isVisible = realizedCreature.bites > 2;
		Vector2 vector4 = Vector2.zero;
		if (realizedCreature.Consious && realizedCreature.room != null)
		{
			Tracker.CreatureRepresentation lookCreature = creatureLooker.lookCreature;
			if (lookCreature != null)
			{
				vector4 = ((!lookCreature.VisualContact) ? (realizedCreature.room.MiddleOfTile(lookCreature.BestGuessForPosition()) - vector).normalized : (lookCreature.representedCreature.realizedCreature.DangerPos - vector).normalized);
			}
		}
		Color color = Color.Lerp(bodyColor, blackColor, darkness * 0.6f + 0.4f);
		bool flag = realizedCreature.stun > 0 || !realizedCreature.Consious || realizedCreature.attachedObject != null;
		for (int i = 0; i < 2; i++)
		{
			float num3 = (float)i * 2f - 1f + num * 0.5f;
			float num4 = Mathf.InverseLerp(1.2f, 1f, Mathf.Abs(num3));
			float rotation2 = ((num4 < 1f) ? Custom.VecToDeg(normalized) : 0f);
			for (int j = 0; j < 2; j++)
			{
				FSprite fSprite = sLeaser.sprites[EyeSprite(i, j, 0)];
				FSprite obj = sLeaser.sprites[EyeSprite(i, j, 1)];
				fSprite.isVisible = realizedCreature.bites >= 3;
				obj.isVisible = realizedCreature.bites >= 3;
				Vector2 vector5 = vector + ScalableFeatures(vector3 * (relEyePos[j].x * num3), bloodBankBloat / 3f) + vector3 * (relEyePos[j].x * num3) + ScalableFeatures(normalized * relEyePos[j].y, bloodBankBloat) + normalized * relEyePos[j].y;
				fSprite.SetPosition(vector5 - camPos);
				fSprite.scaleX = num4;
				fSprite.rotation = rotation2;
				fSprite.color = (flag ? color : blackColor);
				obj.SetPosition(vector5 + ((j == 1) ? vector4 : Vector2.zero) - camPos);
				obj.scaleX = ((j == 1) ? (2f * num4) : num4);
				obj.rotation = rotation2;
				obj.color = (flag ? color : eyeColor);
			}
		}
		for (int k = 0; k < 2; k++)
		{
			float side = (float)k * 2f - 1f;
			Vector2 hipPos = GetHipPos(k, timeStacker);
			Vector2 to = Vector2.Lerp(limbs[k].lastPos, limbs[k].pos, timeStacker);
			ConnectLimbSprite(sLeaser.sprites[firstLegSprite + k], hipPos, to, 0.9f, side, camPos);
			sLeaser.sprites[firstLegSprite + k].scale = 1f * Mathf.Lerp(1f, 1.15f, Mathf.InverseLerp(0f, 4f, bloodBankBloat));
		}
		for (int l = 0; l < 2; l++)
		{
			float num5 = (float)l * 2f - 1f;
			Vector2 shoulderPos = GetShoulderPos(l, timeStacker);
			Vector2 to2 = Vector2.Lerp(limbs[l + 2].lastPos, limbs[l + 2].pos, timeStacker);
			ConnectLimbSprite(sLeaser.sprites[firstArmSprite + l], shoulderPos, to2, 0.6f, 0f - num5, camPos);
		}
		for (int m = 0; m < horns; m++)
		{
			FSprite fSprite2 = sLeaser.sprites[firstHornSprite + m * 2];
			FSprite obj2 = sLeaser.sprites[firstHornSprite + m * 2 + 1];
			fSprite2.isVisible = realizedCreature.bites >= 3;
			obj2.isVisible = realizedCreature.bites >= 3;
			Vector2 vector6 = vector + ScalableFeatures(normalized * (4f - (float)m * 2f), bloodBankBloat) + normalized * (4f - (float)m * 2f) - vector3 * (num * 3f);
			fSprite2.SetPosition(vector6 - camPos);
			fSprite2.rotation = Custom.VecToDeg(vector3);
			fSprite2.scaleY = (0f - num) * hornScale * 0.35f;
			fSprite2.scaleX = hornScale * 0.4f;
			obj2.SetPosition(vector6 - camPos);
			obj2.rotation = fSprite2.rotation;
			obj2.scaleX = fSprite2.scaleX;
			obj2.scaleY = fSprite2.scaleY;
		}
	}

	public Vector2 ScalableFeatures(Vector2 positionToScale, float scaler)
	{
		return positionToScale.normalized * scaler;
	}

	public static void ConnectLimbSprite(FSprite sprite, Vector2 from, Vector2 to, float scale, float side, Vector2 camPos)
	{
		int num = Mathf.Clamp(Mathf.Abs(Mathf.RoundToInt(side * 2f)), 0, 1);
		int num2 = Mathf.Clamp(Mathf.RoundToInt(Vector2.Distance(from, to) / 2f / scale - 2f), 0, 4);
		sprite.SetElementByName($"FrogLeg{num}{num2}");
		sprite.SetPosition(from - camPos);
		sprite.rotation = Custom.VecToDeg(to - from) - 90f;
		sprite.scaleX = scale;
		sprite.scaleY = ((side < 0f) ? scale : (0f - scale));
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color a = Color.Lerp(bodyColor, palette.blackColor, darkness);
		Color a2 = Color.Lerp(Color.Lerp(bellyColor, bloodColor, (float)realizedCreature.FrogState.feedSize / (float)realizedCreature.maxFeeds), palette.blackColor, darkness);
		blackColor = palette.blackColor;
		a = Color.Lerp(a, realizedCreature.injectedPoisonColor, Mathf.Clamp01(realizedCreature.injectedPoison) * 0.4f);
		a2 = Color.Lerp(a2, realizedCreature.injectedPoisonColor, Mathf.Clamp01(realizedCreature.injectedPoison) * 0.4f);
		sLeaser.sprites[firstBodySprite].color = a;
		sLeaser.sprites[firstBodySprite + 1].color = a;
		sLeaser.sprites[firstBodySprite + 2].color = a2;
		sLeaser.sprites[firstBodySprite + 3].color = a2;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[EyeSprite(i, j, 0)].color = palette.blackColor;
				sLeaser.sprites[EyeSprite(i, j, 1)].color = eyeColor;
			}
			sLeaser.sprites[firstLegSprite + i].color = Color.Lerp(a, legBloodColor, (float)realizedCreature.FrogState.feedSize / ((float)realizedCreature.maxFeeds * 1.5f));
			sLeaser.sprites[firstArmSprite + i].color = Color.Lerp(a, legBloodColor, (float)realizedCreature.FrogState.feedSize / ((float)realizedCreature.maxFeeds * 1.5f));
		}
		for (int k = 0; k < horns; k++)
		{
			sLeaser.sprites[firstHornSprite + k * 2].color = a;
			sLeaser.sprites[firstHornSprite + k * 2 + 1].color = a2;
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
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
}
