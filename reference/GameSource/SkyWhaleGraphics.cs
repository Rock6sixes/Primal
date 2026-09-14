using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Watcher;

public class SkyWhaleGraphics : GraphicsModule, ILookingAtCreatures
{
	public class Fin : BodyPart
	{
		public int segment;

		public float length;

		public float attachmentAngle;

		public float lastAngle;

		public float angle;

		public bool InBackground => Mathf.Cos((owner as SkyWhaleGraphics).SkyWhale.roll * (float)Math.PI / 2f + attachmentAngle) < 0f;

		public Fin(SkyWhaleGraphics ow, int segment, float side, float length)
			: base(ow)
		{
			this.segment = segment;
			this.length = length;
			attachmentAngle = (float)Math.PI * (ow.SkyWhale.AltVariant ? 0.87f : 0.6f) * (0f - side);
		}

		public override void Update()
		{
			lastAngle = angle;
			lastPos = pos;
			pos += vel;
			vel *= 0.9f;
		}
	}

	public class Marking
	{
		public class ColorMode : ExtEnum<ColorMode>
		{
			public static ColorMode Scar = new ColorMode("Scar", register: true);

			public static ColorMode LightSkin = new ColorMode("LightSkin", register: true);

			public static ColorMode DarkSkin = new ColorMode("DarkSkin", register: true);

			public static ColorMode Black = new ColorMode("Black", register: true);

			public ColorMode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public float segment;

		public float angle;

		public string sprite;

		public float spriteRotation;

		public float width;

		public float height;

		public ColorMode color;

		public Marking(float smoothSegment, float radiansFromUp, float spriteRotation, string sprite, ColorMode color)
		{
			segment = smoothSegment;
			angle = radiansFromUp;
			this.sprite = sprite;
			this.spriteRotation = spriteRotation;
			this.color = color;
			FAtlasElement elementWithName = Futile.atlasManager.GetElementWithName(sprite);
			width = elementWithName.sourceRect.width;
			height = elementWithName.sourceRect.height;
		}
	}

	public class ArmorPlate
	{
		public float segment;

		public float angle;

		public float length;

		public ArmorPlate(float segment, float angle, float length)
		{
			this.segment = segment;
			this.angle = angle;
			this.length = length;
		}
	}

	public struct BodySegment
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public float rad;

		public float distFromSnout;

		public BodySegment(Vector2 pos, Vector2 lastPos, float rad)
		{
			this.pos = pos;
			this.lastPos = lastPos;
			this.rad = rad;
			distFromSnout = 0f;
		}

		public Vector2 DrawPos(float timeStacker)
		{
			return Vector2.Lerp(lastPos, pos, timeStacker);
		}
	}

	public CreatureLooker creatureLooker;

	public int snoutSegments = 6;

	public TailSegment[] tail;

	public float tailSegSeparation;

	public Fin[,] fins;

	public BodySegment[] smoothSegments;

	public Marking[] markings;

	public ArmorPlate[] armor;

	public float eyeOnBodySegment;

	public float eyeOnBodyAngle;

	public Tracker.CreatureRepresentation lastLookCreature;

	public int blink;

	public int swimTimer;

	public Color bgColor;

	public Color skinColor;

	public Color undersideColor;

	public Color blackColor;

	public int bodySprite;

	public int firstTailSprite;

	public int undersideSprite;

	public int firstMarkingSprite;

	public int eyeShadowSprite;

	public int firstEyeSprite;

	public int firstArmorSprite;

	public int firstWhiskerSprite;

	public int firstFinSprite;

	public int totalSprites;

	public SkyWhale SkyWhale => base.owner as SkyWhale;

	public int TotalUnsmoothedSegments => SkyWhale.bodyChunks.Length + tail.Length;

	public float BodyLength => smoothSegments[smoothSegments.Length - 1].distFromSnout;

	public Vector2? LookPos
	{
		get
		{
			Tracker.CreatureRepresentation lookCreature = creatureLooker.lookCreature;
			if (lookCreature == null)
			{
				return null;
			}
			if (lookCreature.VisualContact && lookCreature.representedCreature.realizedCreature != null)
			{
				return lookCreature.representedCreature.realizedCreature.DangerPos;
			}
			return SkyWhale.room.MiddleOfTile(lookCreature.BestGuessForPosition());
		}
	}

	public SkyWhaleGraphics(PhysicalObject po)
		: base(po, internalContainers: false)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(SkyWhale.abstractCreature.ID.RandomSeed);
		creatureLooker = new CreatureLooker(this, SkyWhale.AI.tracker, SkyWhale, 0.5f, 70);
		cullRange = 200f * SkyWhale.creatureParams.sizeMultiplier;
		swimTimer = UnityEngine.Random.Range(0, 1000);
		tail = new TailSegment[5];
		tailSegSeparation = SkyWhale.bodyChunkConnections[0].distance * 0.7f;
		for (int i = 0; i < tail.Length; i++)
		{
			float num = (float)(i + 1) / (float)tail.Length;
			float rd = (1f - num) * SkyWhale.bodyChunks[SkyWhale.bodyChunks.Length - 1].rad + Mathf.Pow(num, 3f) * 7f;
			float cnRd = tailSegSeparation;
			tail[i] = new TailSegment(this, rd, cnRd, (i > 0) ? tail[i - 1] : null, 0.85f, 0.95f, 0.4f, pullInPreviousPosition: false);
		}
		fins = new Fin[5, 2];
		for (int j = 0; j < fins.GetLength(0); j++)
		{
			for (int k = 0; k < fins.GetLength(1); k++)
			{
				float num2 = (float)j / (float)(fins.GetLength(0) - 1);
				float num3 = 150f * Mathf.Lerp(0.5f, 1f, 4f * num2 * (1f - num2));
				int segment = j * 2 + 2 + snoutSegments;
				if (SkyWhale.AltVariant)
				{
					num3 *= 0.4f;
				}
				fins[j, k] = new Fin(this, segment, (float)k * 2f - 1f, num3);
			}
		}
		smoothSegments = new BodySegment[TotalUnsmoothedSegments * 2 - 1 + snoutSegments];
		for (int l = 0; l < snoutSegments; l++)
		{
			float num4 = (SkyWhale.bodyChunks[0].rad - SkyWhale.bodyChunks[1].rad) / SkyWhale.bodyChunkConnections[0].distance;
			float f = (1f - (float)l / (float)snoutSegments) * (float)Math.PI / 2f;
			float rad = SkyWhale.bodyChunks[0].rad;
			float num5 = Mathf.Sin(f) * rad;
			smoothSegments[l].rad = (rad + num4 * num5) * Mathf.Cos(f);
			smoothSegments[l].distFromSnout = rad - num5;
		}
		float num6 = SkyWhale.bodyChunks[0].rad;
		for (int m = snoutSegments; m < smoothSegments.Length; m += 2)
		{
			int num7 = (m - snoutSegments) / 2;
			BodySegment unsmoothedSegment = GetUnsmoothedSegment(num7);
			float num8 = ((num7 < SkyWhale.bodyChunks.Length - 1) ? SkyWhale.bodyChunkConnections[num7].distance : tailSegSeparation);
			smoothSegments[m].rad = unsmoothedSegment.rad;
			smoothSegments[m].distFromSnout = num6;
			if (m + 1 < smoothSegments.Length)
			{
				BodySegment unsmoothedSegment2 = GetUnsmoothedSegment(num7 + 1);
				smoothSegments[m + 1].rad = (unsmoothedSegment.rad + unsmoothedSegment2.rad) / 2f;
				smoothSegments[m + 1].distFromSnout = num6 + num8 / 2f;
			}
			num6 += num8;
		}
		if (SkyWhale.AltVariant)
		{
			for (int n = 0; n < smoothSegments.Length; n++)
			{
				float num9 = Mathf.Min(smoothSegments[n].distFromSnout / BodyLength * 6f, 1f);
				smoothSegments[n].rad *= 1f - 0.5f * Mathf.Pow(1f - num9, 2f);
			}
		}
		List<Marking> list = new List<Marking>();
		if (SkyWhale.AltVariant)
		{
			int num10 = UnityEngine.Random.Range(80, 120);
			for (int num11 = 0; num11 < num10; num11++)
			{
				float num12 = SegmentFromDist(UnityEngine.Random.Range(40f, BodyLength));
				float num13 = (float)Math.PI - GetUndersideAngle(num12);
				float num14 = Mathf.Pow(UnityEngine.Random.value, 2f);
				num13 -= num14 * 1.5f;
				if (UnityEngine.Random.value < 0.5f)
				{
					num13 = 0f - num13;
				}
				string sprite = ((UnityEngine.Random.value < 1f - num14 * 1.5f) ? $"Pebble{UnityEngine.Random.Range(5, 9)}" : "Circle4");
				list.Add(new Marking(num12, num13, UnityEngine.Random.Range(-180f, 180f), sprite, Marking.ColorMode.Scar));
			}
		}
		else
		{
			if (UnityEngine.Random.value < Mathf.Pow(1f - SkyWhale.creatureParams.ruggedness, 2f))
			{
				AddFreckles(list);
			}
			if (UnityEngine.Random.value < SkyWhale.creatureParams.ruggedness * 3f)
			{
				AddScars(list);
			}
		}
		markings = list.ToArray();
		if (!SkyWhale.AltVariant && UnityEngine.Random.value < 0.4f * SkyWhale.creatureParams.ruggedness + 0.2f)
		{
			armor = GenerateArmor();
		}
		else
		{
			armor = new ArmorPlate[0];
		}
		if (SkyWhale.AltVariant)
		{
			eyeOnBodyAngle = 2.042035f;
		}
		else
		{
			eyeOnBodyAngle = (110f + UnityEngine.Random.Range(-15f, 15f)) * ((float)Math.PI / 180f);
		}
		eyeOnBodySegment = (float)snoutSegments + 1.2f + UnityEngine.Random.Range(-0.5f, 0.5f);
		bodyParts = new BodyPart[tail.Length];
		for (int num15 = 0; num15 < bodyParts.Length; num15++)
		{
			bodyParts[num15] = tail[num15];
			bodyParts[num15].bodyPartArrayIndex = num15;
		}
		bodySprite = totalSprites;
		totalSprites++;
		undersideSprite = totalSprites;
		totalSprites++;
		firstTailSprite = totalSprites;
		totalSprites += 2;
		firstMarkingSprite = totalSprites;
		totalSprites += markings.Length;
		eyeShadowSprite = totalSprites;
		if (SkyWhale.AltVariant)
		{
			totalSprites++;
		}
		firstEyeSprite = totalSprites;
		totalSprites += 2;
		firstArmorSprite = totalSprites;
		totalSprites += armor.Length * 2;
		firstFinSprite = totalSprites;
		totalSprites += fins.Length;
		firstWhiskerSprite = totalSprites;
		totalSprites += SkyWhale.whiskers.Count;
		UnityEngine.Random.state = state;
	}

	public ArmorPlate[] GenerateArmor()
	{
		float num = UnityEngine.Random.Range(50f, 100f);
		ArmorPlate[] array = new ArmorPlate[UnityEngine.Random.Range(4, 9)];
		for (int i = 0; i < array.Length; i++)
		{
			float num2 = (float)i / (float)(array.Length - 1);
			float num3 = Mathf.Lerp(35f, 25f, Mathf.Pow(Mathf.Abs(1f - 2f * num2), 3f));
			float segment = SegmentFromDist(num + 32f * (float)i - num3 / 2f);
			float angle = (40f + 202.5f * num2 * Mathf.Pow(1f - num2, 2f)) * ((float)Math.PI / 180f);
			array[i] = new ArmorPlate(segment, angle, num3);
		}
		Array.Reverse(array);
		return array;
	}

	public void AddFreckles(List<Marking> markings)
	{
		float t = ((UnityEngine.Random.value < 0.5f) ? 0f : UnityEngine.Random.value);
		int num = UnityEngine.Random.Range(20, 60);
		for (int i = 0; i < num; i++)
		{
			float num2 = SegmentFromDist(UnityEngine.Random.value * BodyLength);
			num2 = Mathf.Lerp(num2, Mathf.Round(num2), t);
			float radiansFromUp = UnityEngine.Random.Range(-120f, 120f) * ((float)Math.PI / 180f);
			markings.Add(new Marking(num2, radiansFromUp, UnityEngine.Random.Range(-180f, 180f), "tinyStar", Marking.ColorMode.LightSkin));
		}
	}

	public void AddScars(List<Marking> markings)
	{
		int num = (int)(UnityEngine.Random.Range(30f, 80f) * SkyWhale.creatureParams.ruggedness);
		for (int i = 0; i < num; i++)
		{
			float num2 = Mathf.Pow(UnityEngine.Random.value, 1.7f) * BodyLength * 0.6f;
			float smoothSegment = SegmentFromDist(num2);
			float radiansFromUp = UnityEngine.Random.Range(-150f, 150f) * ((float)Math.PI / 180f);
			bool flag = UnityEngine.Random.value < Mathf.InverseLerp(15f, 120f, num2) * 0.8f;
			int num3 = UnityEngine.Random.Range(0, 5);
			Marking marking = new Marking(smoothSegment, radiansFromUp, UnityEngine.Random.Range(-180f, 180f), $"scar{(flag ? 'A' : 'B')}{num3}", Marking.ColorMode.Scar);
			marking.width *= UnityEngine.Random.Range(0.6f, 1f);
			marking.height *= UnityEngine.Random.Range(0.6f, 1f);
			markings.Add(marking);
		}
	}

	public Vector2 GetPointOnBody(int smoothSegment, float radiansFromUp, float timeStacker, float offset = 0f, bool absolute = false)
	{
		Vector2 vector = Custom.PerpendicularVector(GetBodyDir(smoothSegment, timeStacker));
		float num = (absolute ? 0f : (Mathf.Lerp(SkyWhale.lastRoll, SkyWhale.roll, timeStacker) * (float)Math.PI / 2f));
		return smoothSegments[smoothSegment].DrawPos(timeStacker) + vector * ((smoothSegments[smoothSegment].rad + offset) * Mathf.Sin(num + radiansFromUp));
	}

	public Vector2 GetPointOnBody(float smoothSegment, float radiansFromUp, float timeStacker, float offset = 0f, bool absoluteRoll = false)
	{
		int num = (int)smoothSegment;
		return Vector2.Lerp(GetPointOnBody(num, radiansFromUp, timeStacker, offset, absoluteRoll), GetPointOnBody(Mathf.Min(num + 1, smoothSegments.Length - 1), radiansFromUp, timeStacker, offset, absoluteRoll), smoothSegment - (float)num);
	}

	public Vector2 GetBodyDir(int smoothSegment, float timeStacker)
	{
		Vector2 vector = smoothSegments[Mathf.Max(smoothSegment - 1, 0)].DrawPos(timeStacker);
		Vector2 vector2 = smoothSegments[Mathf.Min(smoothSegment + 1, smoothSegments.Length - 1)].DrawPos(timeStacker);
		return (vector - vector2).normalized;
	}

	public Vector2 GetBodyDir(float smoothSegment, float timeStacker)
	{
		int num = (int)smoothSegment;
		return Vector3.Slerp(GetBodyDir(num, timeStacker), GetBodyDir(Mathf.Min(num, smoothSegments.Length - 1), timeStacker), smoothSegment - (float)num);
	}

	public BodySegment GetUnsmoothedSegment(int index)
	{
		if (index < SkyWhale.bodyChunks.Length)
		{
			BodyChunk bodyChunk = SkyWhale.bodyChunks[index];
			return new BodySegment(bodyChunk.pos, bodyChunk.lastPos, bodyChunk.rad);
		}
		TailSegment tailSegment = tail[index - SkyWhale.bodyChunks.Length];
		return new BodySegment(tailSegment.pos, tailSegment.lastPos, tailSegment.rad);
	}

	public float SegmentFromDist(float distFromSnout)
	{
		for (int i = 1; i < smoothSegments.Length; i++)
		{
			if (smoothSegments[i].distFromSnout > distFromSnout)
			{
				float num = (distFromSnout - smoothSegments[i - 1].distFromSnout) / (smoothSegments[i].distFromSnout - smoothSegments[i - 1].distFromSnout);
				return (float)(i - 1) + num;
			}
		}
		return smoothSegments.Length;
	}

	public override void Reset()
	{
		base.Reset();
		Vector2 pos = SkyWhale.bodyChunks[SkyWhale.bodyChunks.Length - 1].pos;
		Vector2 pos2 = SkyWhale.bodyChunks[SkyWhale.bodyChunks.Length - 2].pos;
		Vector2 normalized = (pos - pos2).normalized;
		for (int i = 0; i < tail.Length; i++)
		{
			tail[i].Reset(pos + (float)(i + 1) * tailSegSeparation * normalized);
		}
	}

	public Vector2 GetFinAttachmentPos(int i, float angle, float timeStacker)
	{
		return GetPointOnBody(i, angle, timeStacker, -10f);
	}

	public int FinSprite(int i, int side)
	{
		return firstFinSprite + i + side * fins.GetLength(0);
	}

	public override void Update()
	{
		base.Update();
		creatureLooker.Update();
		if (blink > 0)
		{
			blink--;
		}
		blink = Mathf.Max(blink, SkyWhale.stun);
		if (!SkyWhale.Consious)
		{
			blink = Mathf.Max(blink, 1);
		}
		if (SkyWhale.Consious)
		{
			swimTimer++;
			if (swimTimer % 60 == 12)
			{
				float vol = (SkyWhale.AltVariant ? 0.4f : 1f);
				float pitch = Custom.LerpMap(SkyWhale.creatureParams.sizeMultiplier, 3f, 5f, 1.2f, 0.8f);
				SkyWhale.room.PlaySound(WatcherEnums.WatcherSoundID.Sky_Whale_Fin_Flap, SkyWhale.mainBodyChunk, loop: false, vol, pitch);
			}
		}
		if (creatureLooker.lookCreature != lastLookCreature)
		{
			lastLookCreature = creatureLooker.lookCreature;
			blink = Mathf.Max(blink, UnityEngine.Random.Range(5, 20));
		}
		for (int i = 0; i < smoothSegments.Length; i++)
		{
			smoothSegments[i].lastPos = smoothSegments[i].pos;
		}
		for (int j = snoutSegments; j < smoothSegments.Length; j += 2)
		{
			int num = (j - snoutSegments) / 2;
			BodySegment unsmoothedSegment = GetUnsmoothedSegment(num);
			smoothSegments[j].pos = unsmoothedSegment.pos;
			if (j + 1 < smoothSegments.Length)
			{
				BodySegment unsmoothedSegment2 = GetUnsmoothedSegment(num + 1);
				smoothSegments[j + 1].pos = (unsmoothedSegment.pos + unsmoothedSegment2.pos) / 2f;
			}
		}
		Vector2 normalized = (SkyWhale.bodyChunks[0].pos - SkyWhale.bodyChunks[1].pos).normalized;
		for (int k = 0; k < snoutSegments; k++)
		{
			float num2 = 1f - (float)k / (float)snoutSegments;
			smoothSegments[k].pos = SkyWhale.bodyChunks[0].pos + normalized * (SkyWhale.bodyChunks[0].rad * Mathf.Sin(num2 * (float)Math.PI / 2f));
		}
		Vector2 vector = smoothSegments[0].pos;
		for (int l = 1; l < smoothSegments.Length - 1; l++)
		{
			Vector2 pos = smoothSegments[l].pos;
			Vector2 vector2 = (vector + smoothSegments[l + 1].pos) / 2f;
			smoothSegments[l].pos = (pos + vector2) / 2f;
			vector = pos;
		}
		if (SkyWhale.AltVariant)
		{
			Vector2 vector3 = smoothSegments[0].pos;
			for (int m = 0; m < smoothSegments.Length - 1; m++)
			{
				Vector2 pos2 = smoothSegments[m].pos;
				Vector2 vector4 = Custom.PerpendicularVector((vector3 - smoothSegments[m + 1].pos).normalized);
				float num3 = Mathf.Min(smoothSegments[m].distFromSnout / BodyLength * 6f, 1f);
				float num4 = Mathf.Pow(1f - num3, 1.5f);
				smoothSegments[m].pos -= vector4 * (num4 * Mathf.Sin(SkyWhale.roll * (float)Math.PI / 2f) * SkyWhale.creatureParams.sizeMultiplier * 3f);
				vector3 = pos2;
			}
		}
		for (int n = 0; n < fins.GetLength(0); n++)
		{
			for (int num5 = 0; num5 < fins.GetLength(1); num5++)
			{
				Fin fin = fins[n, num5];
				Vector2 finAttachmentPos = GetFinAttachmentPos(fin.segment, fin.attachmentAngle, 1f);
				Vector2 normalized2 = (smoothSegments[fin.segment - 1].pos - smoothSegments[fin.segment + 1].pos).normalized;
				Vector2 vector5 = Custom.PerpendicularVector(normalized2);
				fin.Update();
				fin.ConnectToPoint(finAttachmentPos, fin.length, push: true, 0f, base.owner.bodyChunks[0].vel, 0.1f, 0f);
				fin.angle = fin.attachmentAngle + Mathf.Sin((float)swimTimer / 30f * (float)Math.PI - (float)n * (float)Math.PI / 4f) * Mathf.Sign(fin.attachmentAngle) * 0.5f;
				fin.pos = finAttachmentPos - normalized2 * (fin.length * 0.7f) + vector5 * (Mathf.Sin(SkyWhale.roll * (float)Math.PI / 2f + fin.angle) * fin.length * 0.7f);
			}
		}
		tail[0].connectedPoint = SkyWhale.bodyChunks[SkyWhale.bodyChunks.Length - 1].pos;
		TailSegment[] array = tail;
		for (int num6 = 0; num6 < array.Length; num6++)
		{
			array[num6].Update();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(SkyWhale.abstractCreature.ID.RandomSeed + 2);
		sLeaser.containers = new FContainer[2];
		sLeaser.containers[0] = new FContainer
		{
			shouldSortByZ = true
		};
		sLeaser.containers[1] = new FContainer
		{
			shouldSortByZ = true
		};
		sLeaser.sprites = new FSprite[totalSprites];
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[smoothSegments.Length * 2 - 2];
		for (int i = 0; i < array.Length; i += 2)
		{
			array[i] = new TriangleMesh.Triangle(i, i + 1, i + 2);
			array[i + 1] = new TriangleMesh.Triangle(i + 2, i + 1, i + 3);
		}
		sLeaser.sprites[bodySprite] = new TriangleMesh("Futile_White", array, customColor: false)
		{
			shader = rCam.game.rainWorld.Shaders["SkyWhaleBody"]
		};
		sLeaser.sprites[undersideSprite] = new TriangleMesh("Futile_White", array, customColor: false);
		TriangleMesh triangleMesh = sLeaser.sprites[bodySprite] as TriangleMesh;
		for (int j = 0; j < triangleMesh.UVvertices.Length; j += 2)
		{
			int num = j / 2;
			float x = smoothSegments[num].distFromSnout / BodyLength;
			triangleMesh.UVvertices[j] = new Vector2(x, 0f);
			triangleMesh.UVvertices[j + 1] = new Vector2(x, 1f);
		}
		sLeaser.sprites[firstTailSprite] = TriangleMesh.MakeLongMesh(5, pointyTip: false, customColor: true);
		sLeaser.sprites[firstTailSprite + 1] = TriangleMesh.MakeLongMesh(5, pointyTip: false, customColor: true);
		for (int k = 0; k < markings.Length; k++)
		{
			sLeaser.sprites[firstMarkingSprite + k] = new CustomFSprite(markings[k].sprite);
		}
		sLeaser.sprites[firstEyeSprite] = new FSprite("deerEyeA");
		sLeaser.sprites[firstEyeSprite + 1] = new FSprite("deerEyeB");
		if (SkyWhale.AltVariant)
		{
			sLeaser.sprites[eyeShadowSprite] = new FSprite("deerEyeA");
		}
		for (int l = 0; l < armor.Length; l++)
		{
			TriangleMesh triangleMesh2 = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: true);
			TriangleMesh triangleMesh3 = new TriangleMesh(triangleMesh2.element.name, triangleMesh2.triangles, customColor: false);
			triangleMesh3.shader = rCam.game.rainWorld.Shaders["SkyWhaleCuticle"];
			triangleMesh3.alpha = UnityEngine.Random.value;
			triangleMesh3.vertices = triangleMesh2.vertices;
			for (int m = 0; m < triangleMesh3.UVvertices.Length; m += 2)
			{
				float x2 = (float)m / (float)(triangleMesh3.UVvertices.Length - 2);
				triangleMesh3.UVvertices[m] = new Vector2(x2, 0f);
				triangleMesh3.UVvertices[m + 1] = new Vector2(x2, 1f);
			}
			sLeaser.sprites[firstArmorSprite + l * 2] = triangleMesh2;
			sLeaser.sprites[firstArmorSprite + l * 2 + 1] = triangleMesh3;
		}
		for (int n = 0; n < fins.GetLength(0); n++)
		{
			for (int num2 = 0; num2 < fins.GetLength(1); num2++)
			{
				sLeaser.sprites[FinSprite(n, num2)] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: true);
				sLeaser.sprites[FinSprite(n, num2)].sortZ = num2 * fins.GetLength(0) - n;
			}
		}
		for (int num3 = 0; num3 < SkyWhale.whiskers.Count; num3++)
		{
			sLeaser.sprites[firstWhiskerSprite + num3] = TriangleMesh.MakeLongMesh(SkyWhale.whiskers[num3].segments.Length, pointyTip: true, customColor: false);
		}
		UnityEngine.Random.state = state;
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		newContatiner.AddChild(sLeaser.containers[0]);
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node in sprites)
		{
			newContatiner.AddChild(node);
		}
		newContatiner.AddChild(sLeaser.containers[1]);
	}

	public float GetUndersideAngle(float seg)
	{
		float num = seg / (float)smoothSegments.Length;
		float num2 = (float)Math.PI / 4f * Mathf.Sqrt(Mathf.Min(num * num * 10f, 1f));
		if (SkyWhale.AltVariant)
		{
			num2 *= 1.5f * Mathf.Sqrt(Mathf.InverseLerp(0.85f, 0.65f, num));
		}
		return num2;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (SkyWhale.room == null || culled)
		{
			return;
		}
		float num = Mathf.Lerp(SkyWhale.lastRoll, SkyWhale.roll, timeStacker);
		Mathf.Cos(num * (float)Math.PI / 2f);
		float num2 = Mathf.Sin(num * (float)Math.PI / 2f);
		TriangleMesh triangleMesh = sLeaser.sprites[undersideSprite] as TriangleMesh;
		for (int i = 0; i < smoothSegments.Length; i++)
		{
			_ = (float)i / (float)(smoothSegments.Length - 1);
			Vector2 vector = smoothSegments[i].DrawPos(timeStacker);
			Vector2 vector2 = Custom.PerpendicularVector((smoothSegments[(i != 0) ? (i - 1) : 0].DrawPos(timeStacker) - smoothSegments[(i == smoothSegments.Length - 1) ? i : (i + 1)].DrawPos(timeStacker)).normalized);
			float rad = smoothSegments[i].rad;
			float num3 = GetUndersideAngle(i);
			if (num2 < 0f)
			{
				num3 = 0f - num3;
			}
			float num4 = ((!(Mathf.Cos(num * (float)Math.PI / 2f + num3) > 0f)) ? Mathf.Sin(num * (float)Math.PI / 2f + num3) : Mathf.Sign(num3));
			triangleMesh.MoveVertice(i * 2, vector - vector2 * (Mathf.Sign(num3) * rad) - camPos);
			triangleMesh.MoveVertice(i * 2 + 1, vector - vector2 * (num4 * rad) - camPos);
		}
		TriangleMesh triangleMesh2 = sLeaser.sprites[bodySprite] as TriangleMesh;
		for (int j = 0; j < smoothSegments.Length; j++)
		{
			Vector2 vector3 = smoothSegments[j].DrawPos(timeStacker);
			Vector2 vector4 = Custom.PerpendicularVector((smoothSegments[(j != 0) ? (j - 1) : 0].DrawPos(timeStacker) - smoothSegments[(j == smoothSegments.Length - 1) ? j : (j + 1)].DrawPos(timeStacker)).normalized);
			float rad2 = smoothSegments[j].rad;
			triangleMesh2.MoveVertice(j * 2, vector3 + vector4 * rad2 - camPos);
			triangleMesh2.MoveVertice(j * 2 + 1, vector3 - vector4 * rad2 - camPos);
		}
		triangleMesh2.alpha = num * 0.5f + 0.5f;
		for (int k = 0; k < markings.Length; k++)
		{
			Marking marking = markings[k];
			CustomFSprite customFSprite = sLeaser.sprites[firstMarkingSprite + k] as CustomFSprite;
			if (Mathf.Cos(num * (float)Math.PI / 2f + marking.angle) < 0f)
			{
				customFSprite.isVisible = false;
				continue;
			}
			customFSprite.isVisible = true;
			Vector2 vector5 = GetPointOnBody(marking.segment, marking.angle, timeStacker) - camPos;
			Vector2 bodyDir = GetBodyDir(marking.segment, timeStacker);
			Vector2 vector6 = Mathf.Cos(num * (float)Math.PI / 2f + marking.angle) * Custom.PerpendicularVector(bodyDir);
			float num5 = Mathf.Cos(marking.spriteRotation * ((float)Math.PI / 180f));
			float num6 = Mathf.Sin(marking.spriteRotation * ((float)Math.PI / 180f));
			Vector2 vector7 = marking.width / 2f * new Vector2(num5, 0f - num6);
			Vector2 vector8 = marking.height / 2f * new Vector2(num6, num5);
			customFSprite.vertices[0] = vector5 + (0f - vector7.x - vector8.x) * bodyDir + (0f - vector7.y - vector8.y) * vector6;
			customFSprite.vertices[1] = vector5 + (vector7.x - vector8.x) * bodyDir + (vector7.y - vector8.y) * vector6;
			customFSprite.vertices[2] = vector5 + (vector7.x + vector8.x) * bodyDir + (vector7.y + vector8.y) * vector6;
			customFSprite.vertices[3] = vector5 + (0f - vector7.x + vector8.x) * bodyDir + (0f - vector7.y + vector8.y) * vector6;
			customFSprite.Refresh();
		}
		float num7 = eyeOnBodyAngle * (0f - Mathf.Sign(num));
		if (Mathf.Cos(num + num7) > 0f)
		{
			sLeaser.sprites[firstEyeSprite].isVisible = true;
			sLeaser.sprites[firstEyeSprite + 1].isVisible = blink == 0;
			Vector2 pointOnBody = GetPointOnBody(eyeOnBodySegment, num7, timeStacker);
			Vector2 bodyDir2 = GetBodyDir(eyeOnBodySegment, timeStacker);
			sLeaser.sprites[firstEyeSprite].SetPosition(pointOnBody - camPos);
			sLeaser.sprites[firstEyeSprite].rotation = Custom.VecToDeg(bodyDir2) - 90f;
			sLeaser.sprites[firstEyeSprite].scaleY = Mathf.Clamp01(Mathf.Cos(num + num7) * (SkyWhale.AltVariant ? 6f : 2f));
			sLeaser.sprites[firstEyeSprite].SetElementByName((blink == 0) ? "deerEyeA" : "deerEyeA2");
			Vector2 vector9 = LookPos ?? (pointOnBody + bodyDir2 * 120f);
			sLeaser.sprites[firstEyeSprite + 1].SetPosition(pointOnBody + Vector2.ClampMagnitude((vector9 - pointOnBody) / 70f, 1.5f) - camPos);
			sLeaser.sprites[firstEyeSprite + 1].rotation = sLeaser.sprites[firstEyeSprite].rotation;
			sLeaser.sprites[firstEyeSprite + 1].scaleY = sLeaser.sprites[firstEyeSprite].scaleY;
			if (SkyWhale.AltVariant)
			{
				sLeaser.sprites[eyeShadowSprite].isVisible = true;
				sLeaser.sprites[eyeShadowSprite].SetPosition(pointOnBody - camPos);
				sLeaser.sprites[eyeShadowSprite].rotation = sLeaser.sprites[firstEyeSprite].rotation;
				sLeaser.sprites[eyeShadowSprite].scaleX = sLeaser.sprites[firstEyeSprite].scaleX * 2f;
				sLeaser.sprites[eyeShadowSprite].scaleY = Mathf.Clamp01(Mathf.Cos(num + num7) * 4f) * 1.25f;
			}
		}
		else
		{
			sLeaser.sprites[firstEyeSprite].isVisible = false;
			sLeaser.sprites[firstEyeSprite + 1].isVisible = false;
			if (SkyWhale.AltVariant)
			{
				sLeaser.sprites[eyeShadowSprite].isVisible = false;
			}
		}
		for (int l = 0; l < armor.Length; l++)
		{
			ArmorPlate armorPlate = armor[l];
			TriangleMesh triangleMesh3 = sLeaser.sprites[firstArmorSprite + l * 2] as TriangleMesh;
			Vector2 bodyDir3 = GetBodyDir(armorPlate.segment, timeStacker);
			for (int m = 0; m < triangleMesh3.vertices.Length; m += 2)
			{
				float num8 = (float)m / (float)(triangleMesh3.vertices.Length - 2) * 2f - 1f;
				float value = num * (float)Math.PI / 2f + armorPlate.angle * num8;
				float value2 = num * (float)Math.PI / 2f + (armorPlate.angle + 0.17453292f) * num8;
				value = Mathf.Clamp(value, -(float)Math.PI / 2f, (float)Math.PI / 2f);
				value2 = Mathf.Clamp(value2, -(float)Math.PI / 2f, (float)Math.PI / 2f);
				Vector2 pointOnBody2 = GetPointOnBody(armorPlate.segment, value, timeStacker, 0f, absoluteRoll: true);
				Vector2 pointOnBody3 = GetPointOnBody(armorPlate.segment, value2, timeStacker, 0f, absoluteRoll: true);
				Vector2 vector10 = Custom.PerpendicularVector(bodyDir3) * Mathf.Sin(value2);
				float num9 = 1f - 0.5f * Mathf.Abs(num8);
				triangleMesh3.MoveVertice(m, pointOnBody2 + bodyDir3 * (armorPlate.length * (1f - num9) * 0.2f) - camPos);
				triangleMesh3.MoveVertice(m + 1, pointOnBody3 - bodyDir3 * (armorPlate.length * num9) + vector10 * 5f - camPos);
				Color color = Color.Lerp(Color.Lerp(skinColor, Color.white, 0.15f), bgColor, Mathf.Pow(Vector2.Dot(vector10, Vector2.up), 2f) * 0.5f);
				triangleMesh3.verticeColors[m] = Color.Lerp(color, skinColor, 0.6f);
				triangleMesh3.verticeColors[m + 1] = color;
			}
			(sLeaser.sprites[firstArmorSprite + l * 2 + 1] as TriangleMesh).Refresh();
		}
		for (int n = 0; n < fins.GetLength(0); n++)
		{
			for (int num10 = 0; num10 < fins.GetLength(1); num10++)
			{
				Fin fin = fins[n, num10];
				TriangleMesh triangleMesh4 = sLeaser.sprites[FinSprite(n, num10)] as TriangleMesh;
				Vector2 finAttachmentPos = GetFinAttachmentPos(fin.segment, fin.attachmentAngle, timeStacker);
				Vector2 vector11 = Vector2.Lerp(fin.lastPos, fin.pos, timeStacker);
				Vector2 vector12 = Custom.PerpendicularVector((vector11 - finAttachmentPos).normalized);
				Vector2 normalized = (smoothSegments[fin.segment - 1].DrawPos(timeStacker) - smoothSegments[fin.segment + 1].DrawPos(timeStacker)).normalized;
				Vector2 vector13 = Custom.PerpendicularVector(normalized) * Mathf.Sin(num * (float)Math.PI / 2f + fin.attachmentAngle);
				if (Vector2.Dot(vector12, vector13) > 0f)
				{
					vector12 = -vector12;
				}
				for (int num11 = 0; num11 < triangleMesh4.vertices.Length; num11 += 2)
				{
					float num12 = (float)num11 / (float)(triangleMesh4.vertices.Length - 2);
					Vector2 vector14 = Custom.Bezier(finAttachmentPos, finAttachmentPos + vector13 * (fin.length * 0.7f) - normalized * (fin.length * 0.2f * (SkyWhale.AltVariant ? 0.5f : 1f)), vector11, vector11 + vector13 * (fin.length * 0.2f * (1f - SkyWhale.creatureParams.finSharpness)), num12);
					float num13 = (1f - num12) * 4f;
					float num14 = fin.length * Mathf.Lerp(2f * num12 * (1f - num12), 4f * num12 * Mathf.Pow(1f - num12, 2f), SkyWhale.creatureParams.finSharpness * 0.8f);
					if (SkyWhale.AltVariant)
					{
						num14 *= 0.1f + Mathf.Pow(num12, 0.7f) * 0.9f;
					}
					triangleMesh4.MoveVertice(num11, vector14 - vector12 * num13 - camPos);
					triangleMesh4.MoveVertice(num11 + 1, vector14 + vector12 * num13 - vector13 * num14 - camPos);
				}
				FContainer fContainer = sLeaser.containers[(!fin.InBackground) ? 1u : 0u];
				if (fContainer != triangleMesh4.container)
				{
					fContainer.AddChild(triangleMesh4);
				}
			}
		}
		float num15 = SkyWhale.creatureParams.sizeMultiplier * 20f;
		float num16 = SkyWhale.creatureParams.sizeMultiplier * 7f;
		for (int num17 = 0; num17 < 2; num17++)
		{
			float num18 = num * 0.9f;
			TriangleMesh triangleMesh5 = sLeaser.sprites[firstTailSprite + num17] as TriangleMesh;
			int num19 = smoothSegments.Length - 1;
			Vector2 vector15 = smoothSegments[num19].DrawPos(timeStacker);
			Vector2 vector16 = -GetBodyDir(num19, timeStacker);
			Vector2 vector17 = Custom.PerpendicularVector(vector16) * Mathf.Cos(num18 * (float)Math.PI / 2f);
			Vector2 vector18 = (vector16 * Mathf.Sin(0.6981317f) + vector17 * (Mathf.Sin(0.6981317f) * ((float)num17 * 2f - 1f))) * num15;
			Vector2 vector19 = Custom.PerpendicularVector(vector18);
			for (int num20 = 0; num20 < triangleMesh5.vertices.Length; num20 += 2)
			{
				float num21 = (float)num20 / (float)(triangleMesh5.vertices.Length - 2);
				Vector2 vector20 = vector15 + vector18 * num21;
				float a = Mathf.Lerp(smoothSegments[num19].rad, Mathf.Cos(num21 * (float)Math.PI / 2f) * num16, num21);
				float b = Mathf.Lerp(smoothSegments[num19].rad, Mathf.Cos(num21 * (float)Math.PI / 2f) * num16 * 0.5f, num21);
				float num22 = Mathf.Lerp(a, b, Mathf.Abs(Mathf.Sin(num18 * (float)Math.PI / 2f)));
				triangleMesh5.MoveVertice(num20, vector20 - vector19 * num22 - camPos);
				triangleMesh5.MoveVertice(num20 + 1, vector20 + vector19 * num22 - camPos);
			}
		}
		for (int num23 = 0; num23 < SkyWhale.whiskers.Count; num23++)
		{
			SkyWhale.SkyWhaleWhisker skyWhaleWhisker = SkyWhale.whiskers[num23];
			TriangleMesh triangleMesh6 = sLeaser.sprites[firstWhiskerSprite + num23] as TriangleMesh;
			Vector2 vector21 = skyWhaleWhisker.segments[0].DrawPos(timeStacker);
			vector21 += (vector21 - skyWhaleWhisker.segments[1].DrawPos(timeStacker)).normalized;
			float num24 = 1.5f;
			for (int num25 = 0; num25 < skyWhaleWhisker.segments.Length; num25++)
			{
				float t = (float)num25 / (float)(skyWhaleWhisker.segments.Length - 1);
				Vector2 vector22 = skyWhaleWhisker.segments[num25].DrawPos(timeStacker);
				Vector2 vector23 = Custom.PerpendicularVector((vector22 - vector21).normalized);
				float num26 = Mathf.Lerp(skyWhaleWhisker.startRadius, skyWhaleWhisker.endRadius, t);
				Vector2 vector24 = vector23 * ((num24 + num26) * 0.5f);
				triangleMesh6.MoveVertice(num25 * 4, vector22 - vector24 - camPos);
				triangleMesh6.MoveVertice(num25 * 4 + 1, vector22 + vector24 - camPos);
				if (num25 < skyWhaleWhisker.segments.Length - 1)
				{
					triangleMesh6.MoveVertice(num25 * 4 + 2, vector22 - vector23 * num26 - camPos);
					triangleMesh6.MoveVertice(num25 * 4 + 3, vector22 + vector23 * num26 - camPos);
				}
				else
				{
					triangleMesh6.MoveVertice(num25 * 4 + 2, vector22 - camPos);
				}
				num24 = num26;
				vector21 = vector22;
			}
			triangleMesh6.color = Color.Lerp(skinColor, bgColor, skyWhaleWhisker.BackgroundFactor * 0.5f);
			FContainer fContainer2 = sLeaser.containers[(!skyWhaleWhisker.InBackground) ? 1u : 0u];
			if (fContainer2 != triangleMesh6.container)
			{
				fContainer2.AddChild(triangleMesh6);
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		bgColor = Color.Lerp(palette.skyColor, palette.fogColor, palette.fogAmount);
		if (SkyWhale.AltVariant)
		{
			skinColor = new Color(0.17f, 0.17f, 0.17f);
			undersideColor = new Color(0.46f, 0.4f, 0.29f);
		}
		else
		{
			skinColor = Color.Lerp(blackColor, bgColor, 0.1f);
			undersideColor = Color.Lerp(Color.Lerp(blackColor, Color.white, 0.5f), bgColor, 0.5f);
		}
		sLeaser.sprites[bodySprite].color = skinColor;
		sLeaser.sprites[undersideSprite].color = undersideColor;
		sLeaser.sprites[firstEyeSprite].color = undersideColor;
		sLeaser.sprites[firstEyeSprite + 1].color = blackColor;
		if (SkyWhale.AltVariant)
		{
			sLeaser.sprites[eyeShadowSprite].color = skinColor;
		}
		for (int i = 0; i < markings.Length; i++)
		{
			CustomFSprite customFSprite = sLeaser.sprites[firstMarkingSprite + i] as CustomFSprite;
			Color color = blackColor;
			if (markings[i].color == Marking.ColorMode.Scar)
			{
				color = Color.Lerp(undersideColor, skinColor, 0.6f);
			}
			else if (markings[i].color == Marking.ColorMode.LightSkin)
			{
				color = Color.Lerp(skinColor, Color.white, 0.3f);
			}
			else if (markings[i].color == Marking.ColorMode.DarkSkin)
			{
				color = skinColor;
			}
			else if (markings[i].color == Marking.ColorMode.Black)
			{
				color = blackColor;
			}
			for (int j = 0; j < 4; j++)
			{
				customFSprite.verticeColors[j] = color;
			}
		}
		for (int k = 0; k < fins.GetLength(0); k++)
		{
			Color a = (SkyWhale.AltVariant ? undersideColor : skinColor);
			Color b = (SkyWhale.AltVariant ? skinColor : undersideColor);
			for (int l = 0; l < fins.GetLength(1); l++)
			{
				TriangleMesh triangleMesh = sLeaser.sprites[FinSprite(k, l)] as TriangleMesh;
				for (int m = 0; m < triangleMesh.verticeColors.Length; m += 2)
				{
					float f = (float)m / (float)(triangleMesh.verticeColors.Length - 2);
					triangleMesh.verticeColors[m] = (triangleMesh.verticeColors[m + 1] = Color.Lerp(a, b, Mathf.Pow(f, 5f) * 0.5f));
				}
			}
		}
		for (int n = 0; n < 2; n++)
		{
			TriangleMesh triangleMesh2 = sLeaser.sprites[firstTailSprite + n] as TriangleMesh;
			for (int num = 0; num < triangleMesh2.verticeColors.Length; num += 2)
			{
				float f2 = (float)num / (float)(triangleMesh2.verticeColors.Length - 2);
				triangleMesh2.verticeColors[num] = (triangleMesh2.verticeColors[num + 1] = Color.Lerp(skinColor, undersideColor, Mathf.Pow(f2, 5f) * 0.5f));
			}
		}
		for (int num2 = 0; num2 < armor.Length; num2++)
		{
			sLeaser.sprites[firstArmorSprite + num2 * 2 + 1].color = skinColor;
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		return score * (1f + SkyWhale.AI.CreatureUnease(crit.representedCreature));
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return null;
	}

	public void LookAtNothing()
	{
	}
}
