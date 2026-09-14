using RWCustom;
using UnityEngine;

namespace Watcher;

public class GrappleSnakeGraphics : GraphicsModule
{
	public struct HairInfo
	{
		public int bodyIndex;

		public float angleOffset;

		public float scaleFactor;

		public HairInfo(int bodyIndex, float angleOffset, float scaleFactor)
		{
			this.bodyIndex = bodyIndex;
			this.angleOffset = angleOffset;
			this.scaleFactor = scaleFactor;
		}
	}

	public int hairCount = 60;

	public HairInfo[] hairInfo;

	public TailSegment[] wigglyTail;

	public GrappleSnake snake => base.owner as GrappleSnake;

	public int headIndex => bodyIndex + 1;

	public int bodyIndex => 0;

	public int hookStartIndex => headIndex + 1;

	public int hooksEndIndex => hookStartIndex + snake.hooks.Count * 2;

	public int hairsStartIndex => hooksEndIndex + 1;

	public int hairsEndIndex => hairsStartIndex + hairCount;

	public int spriteCount => hairsEndIndex;

	public GrappleSnakeGraphics(GrappleSnake snake)
		: base(snake, internalContainers: false)
	{
		wigglyTail = new TailSegment[20];
		bodyParts = new BodyPart[wigglyTail.Length];
		float num = 150f;
		for (int i = 0; i < wigglyTail.Length; i++)
		{
			float num2 = (float)i / (float)wigglyTail.Length;
			float num3 = 1f - num2;
			float rd = Mathf.Lerp(snake.bodyChunks[snake.bodyChunks.Length - 1].rad, 0.8f, 1f - num3 * num3 * num3);
			float num4 = num / (float)wigglyTail.Length;
			if (i == 0)
			{
				num4 = (num4 + snake.bodyChunkConnections[snake.bodyChunkConnections.Length - 1].distance) * 0.8f;
			}
			wigglyTail[i] = new TailSegment(this, rd, num4, (i > 0) ? wigglyTail[i - 1] : null, 0.85f, 0.75f, 0f, pullInPreviousPosition: false);
			bodyParts[i] = wigglyTail[i];
		}
		Random.State state = Random.state;
		Random.InitState(snake.abstractCreature.ID.RandomSeed);
		hairInfo = new HairInfo[hairCount];
		for (int j = 0; j < hairInfo.Length; j++)
		{
			int num5 = (int)(Random.value * Random.value * (float)snake.bodyChunks.Length * 0.5f);
			float angleOffset = Random.Range(45f, 135f) * (float)(((double)Random.value > 0.5) ? 1 : (-1));
			hairInfo[j] = new HairInfo(num5, angleOffset, Random.value);
		}
		Random.state = state;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[spriteCount];
		sLeaser.sprites[headIndex] = new FSprite("Futile_White");
		sLeaser.sprites[headIndex].scale = snake.mainBodyChunk.rad / 10f * 1.33f;
		sLeaser.sprites[headIndex].color = new Color(1f, 1f, 1f);
		sLeaser.sprites[headIndex].shader = rCam.room.game.rainWorld.Shaders["JaggedCircle"];
		sLeaser.sprites[headIndex].alpha = 0.25f;
		sLeaser.sprites[bodyIndex] = TriangleMesh.MakeLongMesh(snake.bodyChunks.Length + wigglyTail.Length, pointyTip: true, customColor: true);
		sLeaser.sprites[bodyIndex].shader = rCam.room.game.rainWorld.Shaders["GrappleSnake"];
		sLeaser.sprites[bodyIndex].alpha = 0.5f;
		int num;
		for (num = hookStartIndex; num < hooksEndIndex; num++)
		{
			sLeaser.sprites[num] = new FSprite("Circle20");
			sLeaser.sprites[num].scale = 0.5f;
			sLeaser.sprites[num].color = new Color(0f, 0f, 1f);
			num++;
			sLeaser.sprites[num] = TriangleMesh.MakeLongMesh(1, pointyTip: false, customColor: false);
		}
		for (int i = hairsStartIndex; i < hairsEndIndex; i++)
		{
			HairInfo hairInfo = this.hairInfo[i - hairsStartIndex];
			sLeaser.sprites[i] = new FSprite("LizardScaleA0");
			sLeaser.sprites[i].scale = Mathf.Lerp(15f, 25f, hairInfo.scaleFactor) / 10f;
			if (hairInfo.angleOffset > 0f)
			{
				sLeaser.sprites[i].scaleX = -1f;
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void Update()
	{
		base.Update();
		BodyChunk bodyChunk = snake.bodyChunks[snake.bodyChunks.Length - 1];
		wigglyTail[0].connectedPoint = bodyChunk.pos;
		wigglyTail[0].vel.y -= base.owner.gravity;
		wigglyTail[0].Update();
		Vector2 rotation = bodyChunk.Rotation;
		Vector2 vector = Custom.PerpendicularVector(rotation);
		_ = bodyChunk.pos;
		for (int i = 1; i < wigglyTail.Length; i++)
		{
			TailSegment tailSegment = wigglyTail[i];
			float num = (float)i / (float)wigglyTail.Length;
			float num2 = Mathf.Sin(num * 24f) * 13f * num;
			_ = bodyChunk.pos + rotation * (i + 1) * tailSegment.connectionRad + vector * num2;
			tailSegment.vel.y -= base.owner.gravity;
			tailSegment.Update();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		sLeaser.sprites[headIndex].color = palette.blackColor;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = SmoothedChunkPosition(0, timeStacker);
		float num = SmoothedChunkRadius(0);
		for (int i = 0; i < snake.bodyChunks.Length + wigglyTail.Length; i++)
		{
			Vector2 vector2 = SmoothedChunkPosition(i, timeStacker);
			Vector2 normalized = (vector2 - vector).normalized;
			Vector2 vector3 = Custom.PerpendicularVector(normalized);
			float num2 = SmoothedChunkRadius(i);
			float num3 = Vector2.Distance(vector2, vector) / 5f;
			(sLeaser.sprites[bodyIndex] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * (num + num2) * 0.5f + normalized * num3 - camPos);
			(sLeaser.sprites[bodyIndex] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * (num + num2) * 0.5f + normalized * num3 - camPos);
			if (i < snake.bodyChunks.Length + wigglyTail.Length - 1)
			{
				(sLeaser.sprites[bodyIndex] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num2 - normalized * num3 - camPos);
				(sLeaser.sprites[bodyIndex] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num2 - normalized * num3 - camPos);
			}
			else
			{
				(sLeaser.sprites[bodyIndex] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - camPos);
			}
			num = num2;
			vector = vector2;
		}
		sLeaser.sprites[headIndex].x = Mathf.Lerp(snake.mainBodyChunk.lastPos.x, snake.mainBodyChunk.pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[headIndex].y = Mathf.Lerp(snake.mainBodyChunk.lastPos.y, snake.mainBodyChunk.pos.y, timeStacker) - camPos.y;
		for (int j = hairsStartIndex; j < hairsEndIndex; j++)
		{
			HairInfo hairInfo = this.hairInfo[j - hairsStartIndex];
			FSprite obj = sLeaser.sprites[j];
			BodyChunk bodyChunk = snake.bodyChunks[hairInfo.bodyIndex];
			BodyChunk rotationChunk = bodyChunk.rotationChunk;
			Vector2 v = new Vector2(Mathf.Lerp(bodyChunk.lastPos.x, bodyChunk.pos.x, timeStacker) - Mathf.Lerp(rotationChunk.lastPos.x, rotationChunk.pos.x, timeStacker), Mathf.Lerp(bodyChunk.lastPos.y, bodyChunk.pos.y, timeStacker) - Mathf.Lerp(rotationChunk.lastPos.y, rotationChunk.pos.y, timeStacker));
			v.Normalize();
			float num4 = Custom.VecToDeg(v) + hairInfo.angleOffset;
			Vector2 vector4 = Custom.DegToVec(num4) * bodyChunk.rad;
			obj.x = Mathf.Lerp(bodyChunk.lastPos.x, bodyChunk.pos.x, timeStacker) + vector4.x - camPos.x;
			obj.y = Mathf.Lerp(bodyChunk.lastPos.y, bodyChunk.pos.y, timeStacker) + vector4.y - camPos.y;
			obj.rotation = num4;
		}
	}

	public Vector2 SmoothedChunkPosition(int index, float partialTick)
	{
		if (index >= base.owner.bodyChunks.Length)
		{
			int num = index - base.owner.bodyChunks.Length;
			return new Vector2(Mathf.Lerp(wigglyTail[num].lastPos.x, wigglyTail[num].pos.x, partialTick), Mathf.Lerp(wigglyTail[num].lastPos.y, wigglyTail[num].pos.y, partialTick));
		}
		return new Vector2(Mathf.Lerp(base.owner.bodyChunks[index].lastPos.x, base.owner.bodyChunks[index].pos.x, partialTick), Mathf.Lerp(base.owner.bodyChunks[index].lastPos.y, base.owner.bodyChunks[index].pos.y, partialTick));
	}

	public float SmoothedChunkRadius(int index)
	{
		if (index >= base.owner.bodyChunks.Length)
		{
			int num = index - base.owner.bodyChunks.Length;
			return wigglyTail[num].rad;
		}
		return base.owner.bodyChunks[index].rad;
	}
}
