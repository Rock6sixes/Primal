using RWCustom;
using UnityEngine;

namespace Watcher;

public class MothGrubGraphics : GraphicsModule
{
	public MothGrub bug;

	public Color bodyColor;

	public Vector2[,,] wrapping;

	public Vector2[,,,] limbs;

	public float flapness;

	public int flapAnim;

	public int flapWarmUp;

	public int BodyMesh => base.owner.bodyChunks.Length;

	public int BodyStripeMesh => base.owner.bodyChunks.Length + 1;

	public int MouthSpriteV => base.owner.bodyChunks.Length + 2;

	public int MouthSpriteH => base.owner.bodyChunks.Length + 3;

	public MothGrubGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bug = ow as MothGrub;
		wrapping = new Vector2[2, 10, 2];
		limbs = new Vector2[2, 2, 6, 3];
	}

	public int LimbMesh(int s, int i)
	{
		return base.owner.bodyChunks.Length + 4 + s * 2 + i;
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < wrapping.GetLength(1); i++)
		{
			float t = Mathf.InverseLerp(0f, wrapping.GetLength(1) - 1, i);
			wrapping[0, i, 0] = Vector2.Lerp(bug.bodyChunks[2].pos, bug.bodyChunks[0].pos, t) - new Vector2(20f, 0f);
			wrapping[1, i, 0] = Vector2.Lerp(bug.bodyChunks[2].pos, bug.bodyChunks[0].pos, t) + new Vector2(20f, 0f);
			wrapping[0, i, 1] = wrapping[0, i, 0];
			wrapping[1, i, 1] = wrapping[1, i, 0];
		}
		for (int j = 0; j < limbs.GetLength(0); j++)
		{
			int num = ((j == 0) ? 1 : (-1));
			for (int k = 0; k < limbs.GetLength(1); k++)
			{
				int num2 = ((k == 0) ? 1 : (-1));
				for (int l = 0; l < limbs.GetLength(2); l++)
				{
					limbs[j, k, l, 0] = bug.bodyChunks[1].pos + new Vector2((float)num * 3f + (float)(num * l), (float)num2 * 3f);
					limbs[j, k, l, 1] = limbs[j, k, l, 0];
					limbs[j, k, l, 2] = Vector2.zero;
				}
			}
		}
	}

	public override void Update()
	{
		base.Update();
		float to = 0.5f + Mathf.Cos((float)flapAnim * (0.05f + bug.abstractCreature.personality.energy * 0.05f)) * 0.3f;
		float num = 0.03f;
		if (bug.Consious)
		{
			flapAnim++;
			bool flag = false;
			if (bug.AI.creatureLooker.lookCreature != null && bug.AI.creatureLooker.lookCreature.VisualContact && bug.AI.creatureLooker.lookCreature.representedCreature.realizedCreature != null && bug.AI.creatureLooker.lookCreature.representedCreature.realizedCreature is BigMoth { wantToFly: not false })
			{
				flag = true;
			}
			if (flag)
			{
				if (flapWarmUp > 0)
				{
					if (flapWarmUp < 230)
					{
						flapWarmUp++;
					}
				}
				else if (flapWarmUp == 0 && Random.value < 0.1f)
				{
					flapWarmUp = 1;
				}
			}
			else if (flapWarmUp > 0)
			{
				flapWarmUp = Mathf.Max(flapWarmUp - 2, 0);
			}
			if (flapWarmUp > 0)
			{
				float num2 = Mathf.Min(flapWarmUp, 230f) / 230f;
				to = Mathf.Cos((float)flapAnim * (0.15f + bug.abstractCreature.personality.energy * 0.2f));
				num = 0.05f + num2 * 0.95f;
			}
		}
		flapness = Custom.LerpAndTick(flapness, to, num, num * 0.1f);
		Vector2 vector = bug.bodyChunks[2].pos;
		Vector2 vector2 = Custom.ClosestPointOnLine(bug.bodyChunks[3].pos, bug.bodyChunks[4].pos, bug.bodyChunks[2].pos);
		if (Vector2.Dot(Custom.PerpendicularVector(Custom.DirVec(bug.bodyChunks[3].pos, bug.bodyChunks[4].pos)), Custom.DirVec(bug.bodyChunks[2].pos, vector2)) < 0f)
		{
			vector = vector2;
		}
		Vector2 vector3 = Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
		Vector2 vector4 = Custom.DirVec(vector, bug.bodyChunks[1].pos);
		Vector2 vector5 = Custom.PerpendicularVector(vector3);
		Vector2 vector6 = Custom.PerpendicularVector(vector4);
		Vector2 b = bug.bodyChunks[0].pos + (vector3 - vector4 * 0.4f) * 10f;
		float num3 = Mathf.Abs(Vector2.Dot(vector3, vector6));
		Vector2 cB = Vector2.Lerp(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos + vector3 * 4f, 0.5f) + (vector5 * 0.5f + Custom.DirVec(bug.bodyChunks[4].pos, bug.bodyChunks[0].pos)).normalized * (8f + num3 * 2f);
		Vector2 cB2 = Vector2.Lerp(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos + vector3 * 4f, 0.5f) + (-vector5 * 0.5f + Custom.DirVec(bug.bodyChunks[3].pos, bug.bodyChunks[0].pos)).normalized * (8f + num3 * 2f);
		Vector2 cA = bug.bodyChunks[3].pos + (vector6 * 2f + Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[3].pos)).normalized * (8f + num3 * 2f);
		Vector2 cA2 = bug.bodyChunks[4].pos + (-vector6 * 2f + Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[4].pos)).normalized * (8f + num3 * 2f);
		Vector2 a = vector - vector4 * (12f + num3 * 2f) - vector6 * (Vector2.Dot(vector3, vector6) * 5f);
		Custom.BezierThroughHandles(a, ref cA, b, ref cB);
		Custom.BezierThroughHandles(a, ref cA2, b, ref cB2);
		for (int i = 0; i < wrapping.GetLength(1); i++)
		{
			wrapping[0, i, 1] = wrapping[0, i, 0];
			wrapping[1, i, 1] = wrapping[1, i, 0];
			float f = Mathf.InverseLerp(0f, wrapping.GetLength(1) - 1, i);
			wrapping[0, i, 0] = Custom.Bezier(a, cA, b, cB, f);
			wrapping[1, i, 0] = Custom.Bezier(a, cA2, b, cB2, f);
		}
		float num4 = 17f;
		float num5 = num4 / (float)limbs.GetLength(2);
		for (int j = 0; j < limbs.GetLength(0); j++)
		{
			for (int k = 0; k < limbs.GetLength(1); k++)
			{
				Vector2 vector7 = Custom.DirVec(wrapping[j, wrapping.GetLength(1) - 5, 0], wrapping[j, wrapping.GetLength(1) - 3, 0]);
				Vector2 vector8 = Custom.PerpendicularVector(vector7) * ((j == 0) ? 1f : (-1f));
				Vector2 normalized = (vector8 + vector7 * (((k == 0) ? 1f : (-1f)) * 0.7f * flapness)).normalized;
				Vector2 vector9 = wrapping[j, wrapping.GetLength(1) - 4, 0] - vector8 * 2f + vector7 * ((k == 0) ? 0f : (-2f));
				for (int l = 0; l < limbs.GetLength(2); l++)
				{
					limbs[j, k, l, 1] = limbs[j, k, l, 0];
					limbs[j, k, l, 0] += limbs[j, k, l, 2];
					limbs[j, k, l, 2] *= 0.9f;
					float num6 = Mathf.InverseLerp(0f, limbs.GetLength(2), l + 1);
					Vector2 vector10 = (vector9 + normalized * (num4 * num6) - limbs[j, k, l, 0]) * (0.25f * (1f - num6 * 0.8f));
					limbs[j, k, l, 0] += vector10;
					limbs[j, k, l, 2] += vector10;
				}
				for (int m = 1; m < limbs.GetLength(2); m++)
				{
					Vector2 vector11 = Custom.DirVec(limbs[j, k, m - 1, 0], limbs[j, k, m, 0]);
					if (Vector2.Distance(limbs[j, k, m - 1, 0], limbs[j, k, m, 0]) > num5)
					{
						limbs[j, k, m, 0] = limbs[j, k, m - 1, 0] + vector11 * num5;
					}
				}
				limbs[j, k, 0, 0] = vector9;
				limbs[j, k, 1, 0] = Vector2.Lerp(limbs[j, k, 1, 0], vector9, 0.5f);
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[base.owner.bodyChunks.Length + 4 + 4];
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("Circle20");
			sLeaser.sprites[i].scale = base.owner.bodyChunks[i].rad / 10f;
		}
		sLeaser.sprites[BodyMesh] = TriangleMesh.MakeLongMesh(wrapping.GetLength(1) / 2, pointyTip: false, customColor: false);
		sLeaser.sprites[BodyStripeMesh] = TriangleMesh.MakeLongMesh(wrapping.GetLength(1) / 2, pointyTip: false, customColor: false);
		sLeaser.sprites[BodyStripeMesh].shader = rCam.room.game.rainWorld.Shaders["MothGrubStripes"];
		sLeaser.sprites[BodyStripeMesh].alpha = 0.8f;
		sLeaser.sprites[MouthSpriteV] = new FSprite("pixel");
		sLeaser.sprites[MouthSpriteH] = new FSprite("pixel");
		sLeaser.sprites[MouthSpriteV].scaleX = 1.7f;
		sLeaser.sprites[MouthSpriteH].scaleY = 1.7f;
		for (int j = 0; j < limbs.GetLength(0); j++)
		{
			for (int k = 0; k < limbs.GetLength(1); k++)
			{
				sLeaser.sprites[LimbMesh(j, k)] = TriangleMesh.MakeLongMesh(limbs.GetLength(2) / 2, pointyTip: false, customColor: false);
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2[] array = new Vector2[bug.bodyChunks.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Vector2.Lerp(bug.bodyChunks[i].lastPos, bug.bodyChunks[i].pos, timeStacker);
		}
		Vector2 v = Custom.DirVec(array[1], array[0]);
		Vector2 v2 = Custom.DirVec(array[2], array[1]);
		Custom.PerpendicularVector(v);
		Custom.PerpendicularVector(v2);
		for (int j = 0; j < base.owner.bodyChunks.Length; j++)
		{
			sLeaser.sprites[j].x = array[j].x - camPos.x;
			sLeaser.sprites[j].y = array[j].y - camPos.y;
		}
		for (int k = 0; k < wrapping.GetLength(1); k++)
		{
			Vector2 pos = Vector2.Lerp(wrapping[0, k, 1], wrapping[0, k, 0], timeStacker) - camPos;
			Vector2 pos2 = Vector2.Lerp(wrapping[1, k, 1], wrapping[1, k, 0], timeStacker) - camPos;
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(k * 2, pos);
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(k * 2 + 1, pos2);
			(sLeaser.sprites[BodyStripeMesh] as TriangleMesh).MoveVertice(k * 2, pos);
			(sLeaser.sprites[BodyStripeMesh] as TriangleMesh).MoveVertice(k * 2 + 1, pos2);
		}
		Vector2 p = Vector2.Lerp(wrapping[0, wrapping.GetLength(1) - 1, 1], wrapping[0, wrapping.GetLength(1) - 1, 0], timeStacker);
		Vector2 normalized = (Custom.DirVec(Vector2.Lerp(wrapping[0, wrapping.GetLength(1) - 2, 1], wrapping[0, wrapping.GetLength(1) - 2, 0], timeStacker), p) + Custom.DirVec(Vector2.Lerp(wrapping[1, wrapping.GetLength(1) - 2, 1], wrapping[1, wrapping.GetLength(1) - 2, 0], timeStacker), p)).normalized;
		p -= normalized * 2.5f;
		sLeaser.sprites[MouthSpriteV].x = p.x - camPos.x;
		sLeaser.sprites[MouthSpriteV].y = p.y - camPos.y;
		sLeaser.sprites[MouthSpriteH].x = p.x - camPos.x;
		sLeaser.sprites[MouthSpriteH].y = p.y - camPos.y;
		sLeaser.sprites[MouthSpriteH].scaleX = 7f;
		sLeaser.sprites[MouthSpriteV].scaleY = 7f;
		sLeaser.sprites[MouthSpriteH].rotation = Custom.VecToDeg(normalized);
		sLeaser.sprites[MouthSpriteV].rotation = Custom.VecToDeg(normalized);
		for (int l = 0; l < limbs.GetLength(0); l++)
		{
			for (int m = 0; m < limbs.GetLength(1); m++)
			{
				int num = LimbMesh(l, m);
				Vector2 vector = Vector2.Lerp(limbs[l, m, 0, 1], limbs[l, m, 0, 0], timeStacker);
				Vector2 p2 = Vector2.Lerp(limbs[l, m, 1, 1], limbs[l, m, 1, 0], timeStacker);
				Vector2 v3 = vector + Custom.DirVec(p2, vector) * 4f;
				for (int n = 0; n < limbs.GetLength(2); n++)
				{
					float num2 = Mathf.InverseLerp(0f, limbs.GetLength(2) - 1, n);
					Vector2 vector2 = Vector2.Lerp(limbs[l, m, n, 1], limbs[l, m, n, 0], timeStacker);
					Vector2 vector3 = Custom.PerpendicularVector(vector2, v3);
					float num3 = (2.5f - 0.4f * num2) * ((num2 > 0.7f) ? (1f - Mathf.Pow(num2 - 0.7f, 2f) * 11f) : 1f);
					(sLeaser.sprites[num] as TriangleMesh).MoveVertice(n * 2, vector2 - vector3 * num3 - camPos);
					(sLeaser.sprites[num] as TriangleMesh).MoveVertice(n * 2 + 1, vector2 + vector3 * num3 - camPos);
					v3 = vector2;
				}
			}
		}
		sLeaser.sprites[0].isVisible = false;
		sLeaser.sprites[1].isVisible = false;
		sLeaser.sprites[2].isVisible = false;
		sLeaser.sprites[3].isVisible = false;
		sLeaser.sprites[4].isVisible = false;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = Color.white;
		sLeaser.sprites[1].color = Color.red;
		sLeaser.sprites[2].color = Color.green;
		sLeaser.sprites[3].color = Color.blue;
		sLeaser.sprites[4].color = Color.yellow;
		bodyColor = Color.Lerp(new Color(1f, 0.72f, 0.5f), palette.blackColor, 0.2f);
		sLeaser.sprites[BodyMesh].color = bodyColor;
		sLeaser.sprites[BodyStripeMesh].color = Color.Lerp(bodyColor, palette.blackColor, 0.15f);
		Color color = Color.Lerp(bodyColor, palette.blackColor, 0.9f);
		sLeaser.sprites[MouthSpriteV].color = color;
		sLeaser.sprites[MouthSpriteH].color = color;
		for (int i = 0; i < limbs.GetLength(0); i++)
		{
			for (int j = 0; j < limbs.GetLength(1); j++)
			{
				sLeaser.sprites[LimbMesh(i, j)].color = bodyColor;
			}
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}
}
