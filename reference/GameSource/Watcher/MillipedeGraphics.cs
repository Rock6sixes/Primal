using RWCustom;
using UnityEngine;

namespace Watcher;

public class MillipedeGraphics : GraphicsModule
{
	public Millipede bug;

	public MillipedeLimb[,] legs;

	public int TotalSprites => legs.GetLength(0) + bug.bodyChunks.Length + legs.GetLength(0) + 1;

	public int LegSprite(int pair, int side)
	{
		return ((side == 0) ? (legs.GetLength(0) + bug.bodyChunks.Length) : 0) + pair;
	}

	public MillipedeGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bug = ow as Millipede;
		legs = new MillipedeLimb[bug.bodyChunks.Length * 2, 2];
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			for (int j = 0; j < legs.GetLength(1); j++)
			{
				legs[i, j] = new MillipedeLimb(this, bug.bodyChunks[i / 2], i, 1f, 0.5f, 0.98f, 15f, 0.95f, j);
				if (i > 0)
				{
					legs[i - 1, j].legToTrigger = legs[i, j];
				}
			}
		}
	}

	public override void Update()
	{
		base.Update();
		Vector2 a = Custom.ClosestPointOnLine(legs[0, 0].absoluteHuntPos, legs[0, 1].absoluteHuntPos, bug.bodyTrail[0]);
		if (legs[0, 0].reachedSnapPosition && legs[0, 1].reachedSnapPosition && Vector2.Distance(a, Vector2.Lerp(legs[0, 0].absoluteHuntPos, legs[0, 1].absoluteHuntPos, 0.5f)) > Vector2.Distance(legs[0, 0].absoluteHuntPos, legs[0, 1].absoluteHuntPos) * 0.5f)
		{
			Vector2 vector = Custom.DirVec(bug.bodyTrail[1], bug.bodyTrail[0]);
			Vector2 b = bug.bodyTrail[0] + vector * 100f;
			int num = 1;
			if (Vector2.Distance(legs[0, 0].absoluteHuntPos, b) > Vector2.Distance(legs[0, 1].absoluteHuntPos, b))
			{
				num = 0;
			}
			FindGrip(0, num, (Vector2.Dot(vector, Custom.DirVec(legs[0, num].absoluteHuntPos, bug.bodyTrail[0])) > 0f) ? 1f : (-1f));
		}
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			for (int j = 0; j < legs.GetLength(1); j++)
			{
				if (legs[i, j].mode == Limb.Mode.HuntRelativePosition || Vector2.Distance(legs[i, j].absoluteHuntPos, bug.bodyTrail[i / 2]) > 40f)
				{
					FindGrip(i, j, Random.value);
				}
				legs[i, j].Update();
				legs[i, j].ConnectToPoint(LimbConnectionPoint(i, j), 40f, push: false, 0f, legs[i, j].connection.vel, 1f, 0.5f);
			}
		}
	}

	public virtual Vector2 LimbConnectionPoint(int pair, int side)
	{
		return legs[pair, side].connection.pos + new Vector2((pair % 2 == 0) ? (-2f) : 2f, 0f);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		for (int i = 0; i < bug.bodyChunks.Length; i++)
		{
			sLeaser.sprites[i + legs.GetLength(0)] = new FSprite("Circle20");
			sLeaser.sprites[i + legs.GetLength(0)].scale = bug.bodyChunks[i].rad * 0.1f;
		}
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				int num = LegSprite(j, k);
				sLeaser.sprites[num] = new FSprite("pixel");
				sLeaser.sprites[num].scaleX = 2f;
				sLeaser.sprites[num].anchorY = 0f;
			}
		}
		sLeaser.sprites[sLeaser.sprites.Length - 1] = new FSprite("FaceA0");
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		for (int i = 0; i < bug.bodyChunks.Length; i++)
		{
			Vector2 vector = Vector2.Lerp(bug.bodyChunks[i].lastPos, bug.bodyChunks[i].pos, timeStacker) - camPos;
			sLeaser.sprites[i + legs.GetLength(0)].x = vector.x;
			sLeaser.sprites[i + legs.GetLength(0)].y = vector.y;
		}
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				int num = LegSprite(j, k);
				Vector2 vector2 = LimbConnectionPoint(j, k);
				Vector2 vector3 = Vector2.Lerp(legs[j, k].lastPos, legs[j, k].pos, timeStacker);
				sLeaser.sprites[num].x = vector2.x - camPos.x;
				sLeaser.sprites[num].y = vector2.y - camPos.y;
				sLeaser.sprites[num].scaleY = Vector2.Distance(vector2, vector3);
				sLeaser.sprites[num].rotation = Custom.VecToDeg(Custom.DirVec(vector2, vector3));
			}
		}
		Vector2 vector4 = Vector2.Lerp(bug.bodyChunks[0].lastPos, bug.bodyChunks[0].pos, timeStacker);
		sLeaser.sprites[sLeaser.sprites.Length - 1].x = vector4.x - camPos.x;
		sLeaser.sprites[sLeaser.sprites.Length - 1].y = vector4.y - camPos.y;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < bug.bodyChunks.Length; i++)
		{
			sLeaser.sprites[i + legs.GetLength(0)].color = Color.white;
		}
		sLeaser.sprites[legs.GetLength(0)].color = Color.white;
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				int num = LegSprite(j, k);
				sLeaser.sprites[num].color = ((k == 0) ? Color.white : new Color(0.7f, 0.7f, 0.7f));
			}
		}
		sLeaser.sprites[sLeaser.sprites.Length - 1].color = palette.blackColor;
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public void FindGrip(int p, int s, float multiplier = 1f)
	{
		Vector2 vector = LimbConnectionPoint(p, s);
		int index = legs[p, s].connection.index;
		Vector2 vector2 = ((index == 0) ? Custom.DirVec(bug.bodyTrail[1], bug.bodyTrail[0]) : Custom.DirVec(bug.bodyTrail[index], bug.bodyTrail[index - 1]));
		legs[p, s].FindGrip(base.owner.room, vector, vector, 30f, vector + vector2 * multiplier * 15f, 2, 2, behindWalls: false);
		legs[p, s].reachedSnapPosition = false;
	}
}
