using System;
using UnityEngine;

public class GooDrip : CosmeticSprite
{
	public float length;

	public Vector2 tailPos;

	public Vector2 lastTailPos;

	public float glitter;

	public float lastGlitter;

	public int attached;

	public int swayCounter;

	public Color blackColor;

	public Color glitterColor;

	public BodyChunk attachedChunk;

	public Oracle.OracleArm.Joint attachedJoint;

	public Vector2 attachmentOffset;

	public Color? color;

	public static TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[2]
	{
		new TriangleMesh.Triangle(0, 1, 2),
		new TriangleMesh.Triangle(0, 2, 3)
	};

	public bool IsAttached => attached >= 0;

	public GooDrip(Vector2 pos, float length)
	{
		this.length = length;
		base.pos = pos;
		lastPos = pos;
		tailPos = pos;
		lastTailPos = pos;
		attached = UnityEngine.Random.Range(40, 140);
		swayCounter = UnityEngine.Random.Range(0, 10000);
	}

	public GooDrip(BodyChunk attachedChunk, Vector2 pos, float length)
		: this(pos, length)
	{
		this.attachedChunk = attachedChunk;
		attachmentOffset = pos - attachedChunk.pos;
	}

	public GooDrip(Oracle.OracleArm.Joint attachedJoint, Vector2 pos, float length)
		: this(pos, length)
	{
		this.attachedJoint = attachedJoint;
		attachmentOffset = pos - attachedJoint.pos;
	}

	public override void Update(bool eu)
	{
		lastTailPos = tailPos;
		base.Update(eu);
		swayCounter++;
		attached--;
		if (attached >= 0)
		{
			Vector2 zero = Vector2.zero;
			if (attachedChunk != null)
			{
				tailPos = attachedChunk.pos + attachmentOffset;
				zero = attachedChunk.vel;
			}
			else if (attachedJoint != null)
			{
				tailPos = attachedJoint.pos + attachmentOffset;
				zero = attachedJoint.vel;
			}
			vel = (tailPos + Vector2.down * length - pos) * new Vector2(0.2f, 0.04f) + zero * 0.5f;
			if (attached == 0)
			{
				length = Vector2.Distance(pos, tailPos);
			}
		}
		else
		{
			vel.y -= 1f;
			tailPos = pos + new Vector2(0f, length);
			length = Mathf.Lerp(length, 2f + vel.magnitude * 3f, 0.25f);
			if ((room.HasAnySolid(room.GetTilePosition(tailPos)) && attached < -5) || tailPos.y < -100f)
			{
				Destroy();
			}
		}
		glitter = Mathf.MoveTowards(glitter, 0f, 0.15f);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new TriangleMesh("Futile_White", tris, customColor: true);
		AddToContainer(sLeaser, rCam, null);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		glitterColor = Color.white;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		TriangleMesh obj = sLeaser.sprites[0] as TriangleMesh;
		float num = Mathf.Lerp(lastGlitter, glitter, timeStacker);
		if (num > 0f)
		{
			num *= UnityEngine.Random.Range(0.5f, 1f);
		}
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker) + new Vector2(Mathf.Cos(((float)swayCounter + timeStacker) / 40f * (float)Math.PI) * 0.5f, 0f);
		Vector2 vector2 = Vector2.Lerp(lastTailPos, tailPos, timeStacker);
		float x = 1f + Mathf.InverseLerp(40f, 10f, Vector2.Distance(vector, vector2));
		obj.MoveVertice(0, vector + new Vector2(x, 0f) - camPos);
		obj.MoveVertice(1, vector - new Vector2(x, 0f) - camPos);
		obj.MoveVertice(2, vector2 - new Vector2(x, 0f) * 0.5f - camPos);
		obj.MoveVertice(3, vector2 + new Vector2(x, 0f) * 0.5f - camPos);
		Color color = this.color ?? blackColor;
		obj.verticeColors[0] = Color.Lerp(color, glitterColor, num);
		obj.verticeColors[1] = Color.Lerp(color, glitterColor, num);
		obj.verticeColors[2] = color;
		obj.verticeColors[3] = color;
	}
}
