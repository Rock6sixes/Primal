using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BigSandGrubGraphics : GraphicsModule
{
	public class BodyNode
	{
		public Vector2 pos;

		public float twist;

		public float rad;

		public Vector2 lastPos;

		public float lastTwist;

		public Vector2 GetPos(float timeStacker)
		{
			return Vector2.Lerp(lastPos, pos, timeStacker);
		}

		public float GetTwist(float timeStacker)
		{
			return Mathf.Lerp(lastTwist, twist, timeStacker);
		}

		public void Update(BigSandGrubGraphics owner, BodyNode prev, BodyNode next)
		{
			lastPos = pos;
			lastTwist = twist;
			Vector2 vector = Custom.DirVec((prev ?? this).pos, (next ?? this).pos);
			twist = twist * 0.9f + (prev ?? this).lastTwist * 0.05f + (next ?? this).twist * 0.05f;
			twist = Mathf.Lerp(twist, vector.x, 0.05f);
			if (owner.Grub.BodyInBurrow)
			{
				twist = 0f;
			}
		}
	}

	public int firstToothSprite;

	public int tailSprite;

	public int firstPlateSprite;

	public int totalSprites;

	public int plates;

	public float gray;

	public float lastGray;

	public int totalSegments;

	public BodyNode[] bodyNodes;

	public int tailSegmentCount;

	public SandGrub Grub => base.owner as SandGrub;

	public BigSandGrubGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		tailSegmentCount = (int)((float)Grub.tentacle.tChunks.Length * 1.5f);
		totalSegments = base.owner.bodyChunks.Length + tailSegmentCount;
		plates = Mathf.Min(totalSegments, 8);
		bodyNodes = new BodyNode[totalSegments];
		for (int i = 0; i < bodyNodes.Length; i++)
		{
			bodyNodes[i] = new BodyNode();
		}
		firstToothSprite = totalSprites;
		totalSprites += 4;
		tailSprite = totalSprites;
		totalSprites++;
		firstPlateSprite = totalSprites;
		totalSprites += plates;
		gray = (Grub.State.dead ? 1f : 0f);
		lastGray = gray;
	}

	public override void Update()
	{
		base.Update();
		lastGray = gray;
		gray = Mathf.MoveTowards(gray, Grub.dead ? 1f : 0f, 0.0020833334f);
		UpdateSegments();
	}

	public void UpdateSegments()
	{
		for (int i = 0; i < bodyNodes.Length; i++)
		{
			bodyNodes[i].Update(this, (i == 0) ? null : bodyNodes[i - 1], (i == bodyNodes.Length - 1) ? null : bodyNodes[i + 1]);
		}
		for (int j = 0; j < base.owner.bodyChunks.Length; j++)
		{
			bodyNodes[j].pos = base.owner.bodyChunks[j].pos;
			bodyNodes[j].rad = base.owner.bodyChunks[j].rad;
		}
		BigSandGrubNeck.Segment[] tChunks = Grub.tentacle.tChunks;
		Vector2 vector = ((Grub.burrow == null) ? (tChunks[0].pos - Custom.DirVec(tChunks[0].pos, tChunks[1].pos) * 10f) : (Grub.burrow.pos - Grub.burrow.dir * 10f));
		int num = tChunks.Length - 1;
		float num2 = 0f;
		float num3 = 0f;
		for (int k = 0; k < tChunks.Length; k++)
		{
			num3 += Vector2.Distance((k == 0) ? vector : tChunks[k - 1].pos, tChunks[k].pos);
		}
		num3 = Mathf.Max(num3, Grub.tailLength);
		for (int l = 4; l < totalSegments; l++)
		{
			Vector2 pos = tChunks[num].pos;
			Vector2 b = ((num == 0) ? vector : tChunks[num - 1].pos);
			float num4 = Vector2.Distance(pos, b);
			num2 += num3 / (float)tailSegmentCount;
			while (num2 > num4 && num > 0)
			{
				num--;
				num2 -= num4;
				pos = tChunks[num].pos;
				b = ((num == 0) ? vector : tChunks[num - 1].pos);
				num4 = Vector2.Distance(pos, b);
			}
			float t = num2 / Mathf.Max(num4, 1f);
			bodyNodes[l].pos = Vector2.Lerp(pos, b, t);
			bodyNodes[l].rad = Mathf.Lerp(tChunks[num].rad, tChunks[Mathf.Max(num - 1, 0)].rad, t);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(Grub.abstractCreature.ID.RandomSeed);
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[firstToothSprite] = new FSprite("grubTooth");
		sLeaser.sprites[firstToothSprite + 1] = new FSprite("grubTooth")
		{
			scaleX = -1f
		};
		int num = UnityEngine.Random.Range(0, 3);
		sLeaser.sprites[firstToothSprite + 2] = new VertexColorSprite($"SpiderLeg{num}B")
		{
			anchorY = 0f,
			scaleY = 0.75f,
			scaleX = -0.75f
		};
		sLeaser.sprites[firstToothSprite + 3] = new VertexColorSprite($"SpiderLeg{num}B")
		{
			anchorY = 0f,
			scaleY = 0.75f
		};
		sLeaser.sprites[tailSprite] = TriangleMesh.MakeLongMeshAtlased(totalSegments, pointyTip: false, customColor: true);
		for (int i = 0; i < plates; i++)
		{
			sLeaser.sprites[firstPlateSprite + i] = new FSprite("Futile_White")
			{
				shader = rCam.game.rainWorld.Shaders["JaggedCircle"],
				alpha = 0.2f
			};
		}
		UnityEngine.Random.state = state;
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
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[firstToothSprite + i].color = color;
			Color[] verticeColors = (sLeaser.sprites[firstToothSprite + i + 2] as VertexColorSprite).verticeColors;
			verticeColors[0] = color;
			verticeColors[1] = color;
			verticeColors[2] = Color.Lerp(palette.blackColor, color, 0.4f);
			verticeColors[3] = Color.Lerp(palette.blackColor, color, 0.4f);
		}
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
		Vector2 pos = bodyNodes[0].GetPos(timeStacker);
		Vector2 vector = Custom.DirVec(bodyNodes[1].GetPos(timeStacker), pos);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		bool flag = Grub.grasps[0] != null;
		float num3 = Mathf.Lerp(Grub.lastMouthOpen, Grub.mouthOpen, timeStacker);
		for (int j = 0; j < plates; j++)
		{
			Vector2 pos2 = bodyNodes[Mathf.Max(j - 1, 0)].GetPos(timeStacker);
			Vector2 vector3 = bodyNodes[j].GetPos(timeStacker);
			Vector2 vector4 = bodyNodes[Mathf.Min(j + 1, totalSegments - 1)].GetPos(timeStacker);
			if (j >= 4)
			{
				vector4 = vector3;
				vector3 = (vector3 + pos2) / 2f;
			}
			float rotation = Custom.AimFromOneVectorToAnother(vector4, pos2);
			float f = bodyNodes[j].GetTwist(timeStacker) * (float)Math.PI * 0.4f;
			sLeaser.sprites[firstPlateSprite + j].SetPosition(vector3 - camPos + Custom.PerpendicularVector(vector4, pos2) * (Mathf.Sin(f) * bodyNodes[j].rad));
			sLeaser.sprites[firstPlateSprite + j].scaleX = ((j == 0) ? 3f : 4f) * Grub.iVars.bodySizeFactor / 8f * Mathf.Cos(f);
			sLeaser.sprites[firstPlateSprite + j].scaleY = 0.25f;
			sLeaser.sprites[firstPlateSprite + j].rotation = rotation;
			sLeaser.sprites[firstPlateSprite + j].color = color;
			sLeaser.sprites[firstPlateSprite + j].isVisible = j >= num;
		}
		TriangleMesh triangleMesh = sLeaser.sprites[tailSprite] as TriangleMesh;
		Vector2 vector5 = pos + (5f - num3 * 2f) * vector;
		float num4 = bodyNodes[0].rad * num3 * 1.5f + num3 * 5f;
		if (flag)
		{
			num4 = bodyNodes[0].rad * (1f + num3 * 0.5f) + num3 * 5f;
		}
		Vector2 vector6 = Vector2.zero;
		for (int k = 0; k < totalSegments; k++)
		{
			Vector2 pos3 = bodyNodes[k].GetPos(timeStacker);
			Vector2 vector7 = (pos3 - vector5).normalized;
			float num5 = Vector2.Distance(pos3, vector5) / 5f;
			float num6 = bodyNodes[k].rad + Custom.LerpMap(k, 0f, 4f, 5f, 0f) * num3;
			int num7 = (totalSegments - k - 1) * 4;
			if (k == totalSegments - 1)
			{
				vector7 = ((Grub.burrow == null) ? vector6 : (-Grub.burrow.dir));
				num5 = 0f;
			}
			vector6 = vector7;
			Vector2 vector8 = Custom.PerpendicularVector(vector7);
			if (k < num)
			{
				Vector2 pos4 = bodyNodes[num].GetPos(timeStacker);
				triangleMesh.MoveVertice(num7 + 3, pos4 - camPos);
				triangleMesh.MoveVertice(num7 + 2, pos4 - camPos);
				triangleMesh.MoveVertice(num7 + 1, pos4 - camPos);
				triangleMesh.MoveVertice(num7, pos4 - camPos);
			}
			else
			{
				Vector2 vector9 = vector8 * ((num4 + num6) * 0.5f);
				Vector2 vector10 = vector8 * num6;
				triangleMesh.MoveVertice(num7 + 3, vector5 - vector9 + vector7 * num5 - camPos);
				triangleMesh.MoveVertice(num7 + 2, vector5 + vector9 + vector7 * num5 - camPos);
				triangleMesh.MoveVertice(num7 + 1, pos3 - vector10 - vector7 * num5 - camPos);
				triangleMesh.MoveVertice(num7, pos3 + vector10 - vector7 * num5 - camPos);
			}
			num4 = num6;
			vector5 = pos3;
		}
		for (int l = 0; l < triangleMesh.vertices.Length; l += 2)
		{
			float num8 = 1f - (float)l / (float)(triangleMesh.vertices.Length - 2);
			Color color2 = Color.Lerp(a, color, Mathf.Pow(num8, 2f) * Mathf.Pow(Mathf.Sin(num8 * (float)Math.PI * 6f), 2f));
			triangleMesh.verticeColors[l] = color2;
			triangleMesh.verticeColors[l + 1] = color2;
		}
		for (int m = 0; m < 2; m++)
		{
			float num9 = (float)m * 2f - 1f;
			sLeaser.sprites[firstToothSprite + m].SetPosition(pos + vector * (4f - num3 * 1f) + vector2 * (num9 * (0.5f + num3 * 3f)) - camPos);
			sLeaser.sprites[firstToothSprite + m].rotation = Custom.VecToDeg(vector);
			sLeaser.sprites[firstToothSprite + m].isVisible = num == 0;
			float num10 = (flag ? 0.75f : num3);
			float num11 = (flag ? 2.5f : (Mathf.Max(num3 * 2f - 1f, 0f) * 30f));
			sLeaser.sprites[firstToothSprite + m + 2].SetPosition(pos + vector * (-15f + num10 * 12f) + vector2 * (num9 * (1.5f + num3 * 1.5f)) - camPos);
			sLeaser.sprites[firstToothSprite + m + 2].scaleX = num9 * 0.75f;
			sLeaser.sprites[firstToothSprite + m + 2].rotation = Custom.VecToDeg(vector) - num9 * num11;
			sLeaser.sprites[firstToothSprite + m + 2].isVisible = num == 0 && num10 > 0f;
		}
	}
}
