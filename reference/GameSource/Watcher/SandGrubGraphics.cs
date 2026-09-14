using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class SandGrubGraphics : GraphicsModule
{
	public struct TailSpine
	{
		public int segment;

		public float pos;

		public float rotation;

		public float length;
	}

	public int firstToothSprite;

	public int firstSpineSprite;

	public int tailSprite;

	public int firstPlateSprite;

	public int totalSprites;

	public int plates = 5;

	public TailSegment[] tail;

	public TailSpine[] spines;

	public float mouthOpen;

	public float lastMouthOpen;

	public int gnashCounter = 20;

	public float gray;

	public float lastGray;

	public int TotalSegments => base.owner.bodyChunks.Length + tail.Length;

	public SandGrub Grub => base.owner as SandGrub;

	public SandGrubGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.owner.abstractPhysicalObject.ID.RandomSeed);
		tail = new TailSegment[15];
		bodyParts = new BodyPart[tail.Length];
		TailSegment cnSeg = null;
		for (int i = 0; i < tail.Length; i++)
		{
			float t = (float)i / (float)(tail.Length - 1);
			TailSegment tailSegment = new TailSegment(this, GenerateTailContour(t), (i == 0) ? 2f : (Grub.tailLength / (float)tail.Length), cnSeg, 0.85f, 0.95f, 0.4f, pullInPreviousPosition: true);
			bodyParts[i] = tailSegment;
			tail[i] = tailSegment;
			cnSeg = tailSegment;
		}
		spines = new TailSpine[UnityEngine.Random.Range(10, 20)];
		for (int j = 0; j < spines.Length; j++)
		{
			float num = (float)j / (float)(spines.Length - 1);
			spines[j] = new TailSpine
			{
				segment = tail.Length - (int)(num * 2f) - 2,
				pos = num * 2f % 1f,
				length = Mathf.Lerp(5f, 15f, 4f * num * (1f - num)),
				rotation = (UnityEngine.Random.value * 2f - 1f) * num * 60f
			};
		}
		UnityEngine.Random.state = state;
		firstToothSprite = totalSprites;
		totalSprites += 2;
		tailSprite = totalSprites;
		totalSprites++;
		firstSpineSprite = totalSprites;
		totalSprites += spines.Length;
		firstPlateSprite = totalSprites;
		totalSprites += plates;
	}

	public override void Update()
	{
		base.Update();
		lastMouthOpen = mouthOpen;
		bool flag;
		if (Grub.grabbedBy.Count <= 0 || Grub.stun >= 20 || Grub.dead)
		{
			flag = !Grub.Consious || (Grub.ai.behavior == SandGrubAI.Behavior.Hunt && Grub.grasps[0] == null);
		}
		else
		{
			flag = gnashCounter > 0;
			gnashCounter = ((gnashCounter > 0) ? (gnashCounter - 1) : (gnashCounter + 1));
			if (gnashCounter == 0)
			{
				gnashCounter = UnityEngine.Random.Range(10, 30) * ((!flag) ? 1 : (-1));
			}
		}
		mouthOpen = Custom.LerpAndTick(mouthOpen, flag ? 1f : 0f, 0.15f, 0.1f);
		lastGray = gray;
		gray = Mathf.MoveTowards(gray, Grub.dead ? 1f : 0f, 1f / 120f);
		Vector2 pos = base.owner.bodyChunks[base.owner.bodyChunks.Length - 1].pos;
		Vector2 vector = Custom.DirVec(base.owner.bodyChunks[base.owner.bodyChunks.Length - 2].pos, pos);
		tail[0].connectedPoint = pos;
		float num = 0f;
		for (int i = 0; i < tail.Length; i++)
		{
			TailSegment tailSegment = tail[i];
			tailSegment.Update();
			tailSegment.vel.y -= 0.9f;
			if (i < 6)
			{
				float num2 = 1f - (float)i / 6f;
				tailSegment.vel += vector * (num2 * 3f);
				tailSegment.vel = Vector2.Lerp(tailSegment.vel, base.owner.bodyChunks[base.owner.bodyChunks.Length - 1].vel, num2 * 0.2f);
			}
			if (Grub.TetheredToBurrow && (num >= Grub.tetherLength * 0.5f || i == tail.Length - 1))
			{
				tailSegment.pos = Grub.burrow.pos - Grub.burrow.dir * 2f;
				tailSegment.vel = Vector2.zero;
			}
			num += tail[i].connectionRad;
		}
	}

	public override void Reset()
	{
		base.Reset();
		Vector2 pos = Grub.mainBodyChunk.pos;
		TailSegment[] array = tail;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset(pos);
		}
	}

	public void PulledFromBurrow()
	{
		Vector2 pos = Grub.bodyChunks[Grub.bodyChunks.Length - 1].pos;
		for (int i = 0; i < tail.Length; i++)
		{
			TailSegment tailSegment = tail[i];
			float num = 1f - (float)i / (float)(tail.Length - 1);
			tailSegment.vel *= 0.5f;
			tailSegment.vel += Grub.burrow.dir * (30f * UnityEngine.Random.value * Mathf.Pow(num, 2f)) + (pos - tailSegment.pos).normalized * (12f * num);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[firstToothSprite] = new FSprite("grubTooth");
		sLeaser.sprites[firstToothSprite + 1] = new FSprite("grubTooth")
		{
			scaleX = -1f
		};
		sLeaser.sprites[tailSprite] = TriangleMesh.MakeLongMeshAtlased(TotalSegments, pointyTip: true, customColor: true);
		for (int i = 0; i < plates; i++)
		{
			sLeaser.sprites[firstPlateSprite + i] = new FSprite("Futile_White")
			{
				shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
				alpha = 0.2f
			};
		}
		for (int j = 0; j < spines.Length; j++)
		{
			sLeaser.sprites[firstSpineSprite + j] = new FSprite("pixel")
			{
				anchorY = 0f,
				scaleY = spines[j].length
			};
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite node in sprites)
		{
			newContatiner.AddChild(node);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color color = Color.Lerp(palette.blackColor, Color.white, 0.5f);
		sLeaser.sprites[firstToothSprite].color = color;
		sLeaser.sprites[firstToothSprite + 1].color = color;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		FSprite[] sprites;
		if (culled)
		{
			sprites = sLeaser.sprites;
			for (int i = 0; i < sprites.Length; i++)
			{
				sprites[i].isVisible = false;
			}
			return;
		}
		sprites = sLeaser.sprites;
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].isVisible = true;
		}
		int num = 3 - Grub.BitesLeft;
		float num2 = Mathf.Lerp(lastGray, gray, timeStacker);
		Color a = Color.Lerp(rCam.currentPalette.blackColor, rCam.currentPalette.fogColor, num2 * 0.1f);
		Color color = Color.Lerp(a, Grub.iVars.effectColor, 0.35f - 0.25f * num2);
		Vector2 vector = SegmentPos(0, timeStacker);
		Vector2 vector2 = Custom.DirVec(SegmentPos(1, timeStacker), vector);
		Vector2 vector3 = Custom.PerpendicularVector(vector2);
		float num3 = Mathf.Lerp(lastMouthOpen, mouthOpen, timeStacker);
		for (int j = 0; j < plates; j++)
		{
			Vector2 p = SegmentPos(Mathf.Max(j - 1, 0), timeStacker);
			Vector2 vector4 = SegmentPos(j, timeStacker);
			float rotation = Custom.AimFromOneVectorToAnother(SegmentPos(Mathf.Min(j + 1, TotalSegments - 1), timeStacker), p);
			sLeaser.sprites[firstPlateSprite + j].SetPosition(vector4 - camPos);
			sLeaser.sprites[firstPlateSprite + j].scaleX = (SegmentRadius(j) - 1f) / 8f;
			sLeaser.sprites[firstPlateSprite + j].scaleY = 0.25f;
			sLeaser.sprites[firstPlateSprite + j].rotation = rotation;
			sLeaser.sprites[firstPlateSprite + j].color = color;
			sLeaser.sprites[firstPlateSprite + j].isVisible = j >= num;
		}
		TriangleMesh triangleMesh = sLeaser.sprites[tailSprite] as TriangleMesh;
		Vector2 vector5 = vector + (5f - num3 * 2f) * vector2;
		float num4 = SegmentRadius(0) * num3 * 1.5f;
		for (int k = 0; k < TotalSegments; k++)
		{
			Vector2 vector6 = SegmentPos(k, timeStacker);
			Vector2 normalized = (vector6 - vector5).normalized;
			Vector2 vector7 = Custom.PerpendicularVector(normalized);
			float num5 = Vector2.Distance(vector6, vector5) / 5f;
			float num6 = SegmentRadius(k);
			int num7 = k * 4;
			if (k < num)
			{
				Vector2 vector8 = SegmentPos(num, timeStacker);
				triangleMesh.MoveVertice(num7, vector8 - camPos);
				triangleMesh.MoveVertice(num7 + 1, vector8 - camPos);
				triangleMesh.MoveVertice(num7 + 2, vector8 - camPos);
				triangleMesh.MoveVertice(num7 + 3, vector8 - camPos);
			}
			else
			{
				Vector2 vector9 = vector7 * ((num4 + num6) * 0.5f);
				triangleMesh.MoveVertice(num7, vector5 - vector9 + normalized * num5 - camPos);
				triangleMesh.MoveVertice(num7 + 1, vector5 + vector9 + normalized * num5 - camPos);
				if (k < TotalSegments - 1)
				{
					Vector2 vector10 = vector7 * num6;
					triangleMesh.MoveVertice(num7 + 2, vector6 - vector10 - normalized * num5 - camPos);
					triangleMesh.MoveVertice(num7 + 3, vector6 + vector10 - normalized * num5 - camPos);
				}
				else
				{
					triangleMesh.MoveVertice(num7 + 2, vector6 - camPos);
				}
			}
			num4 = num6;
			vector5 = vector6;
		}
		for (int l = 0; l < triangleMesh.vertices.Length; l += 2)
		{
			float f = (float)l / (float)(triangleMesh.vertices.Length - 2);
			Color color2 = Color.Lerp(a, color, Mathf.Pow(f, 2f));
			triangleMesh.verticeColors[l] = color2;
			if (l + 1 < triangleMesh.verticeColors.Length)
			{
				triangleMesh.verticeColors[l + 1] = color2;
			}
		}
		for (int m = 0; m < 2; m++)
		{
			float num8 = (float)m * 2f - 1f;
			sLeaser.sprites[firstToothSprite + m].SetPosition(vector + vector2 * (4f - num3 * 1f) + vector3 * (num8 * (0.5f + num3 * 3f)) - camPos);
			sLeaser.sprites[firstToothSprite + m].rotation = Custom.VecToDeg(vector2);
			sLeaser.sprites[firstToothSprite + m].isVisible = num == 0;
		}
		if (!Grub.TetheredToBurrow)
		{
			for (int n = 0; n < spines.Length; n++)
			{
				TailSpine tailSpine = spines[n];
				Vector2 a2 = Vector2.Lerp(tail[tailSpine.segment].lastPos, tail[tailSpine.segment].pos, timeStacker);
				Vector2 vector11 = Vector2.Lerp(tail[tailSpine.segment + 1].lastPos, tail[tailSpine.segment + 1].pos, timeStacker);
				Vector2 vector12 = Vector2.Lerp(a2, vector11, tailSpine.pos);
				sLeaser.sprites[firstSpineSprite + n].SetPosition(vector12 - camPos);
				sLeaser.sprites[firstSpineSprite + n].color = color;
				sLeaser.sprites[firstSpineSprite + n].rotation = Custom.AimFromOneVectorToAnother(tail[tailSpine.segment - 1].pos, vector11) + tailSpine.rotation;
				sLeaser.sprites[firstSpineSprite + n].isVisible = true;
			}
		}
		else
		{
			for (int num9 = 0; num9 < spines.Length; num9++)
			{
				sLeaser.sprites[firstSpineSprite + num9].isVisible = false;
			}
		}
	}

	public Vector2 SegmentPos(int i, float timeStacker)
	{
		if (i < base.owner.bodyChunks.Length)
		{
			return Vector2.Lerp(base.owner.bodyChunks[i].lastPos, base.owner.bodyChunks[i].pos, timeStacker);
		}
		return Vector2.Lerp(tail[i - base.owner.bodyChunks.Length].lastPos, tail[i - base.owner.bodyChunks.Length].pos, timeStacker);
	}

	public float SegmentRadius(int i)
	{
		if (i < base.owner.bodyChunks.Length)
		{
			return base.owner.bodyChunks[i].rad;
		}
		return tail[i - base.owner.bodyChunks.Length].rad;
	}

	public float GenerateTailContour(float t)
	{
		float num = 2f * Grub.iVars.tailThicknessFactor;
		float num2 = Mathf.Min((t - 0.1f) / 0.25f, 1f);
		return (base.owner.bodyChunks[base.owner.bodyChunks.Length - 1].rad - num) * (Mathf.Cos((float)Math.PI * num2) * 0.5f + 0.5f) + num * Mathf.Pow(1f - t, 0.75f) + 0.75f * UnityEngine.Random.value;
	}
}
