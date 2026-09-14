using RWCustom;
using UnityEngine;

public class TailSegment : BodyPart
{
	public float connectionRad;

	public Vector2? connectedPoint;

	public TailSegment connectedSegment;

	public float stretched;

	public float affectPrevious;

	public bool pullInPreviousPosition;

	public float StretchedRad => rad * stretched;

	public TailSegment(GraphicsModule ow, float rd, float cnRd, TailSegment cnSeg, float sfFric, float aFric, float affectPrevious, bool pullInPreviousPosition)
		: base(ow)
	{
		rad = rd;
		connectionRad = cnRd;
		surfaceFric = sfFric;
		airFriction = aFric;
		connectedSegment = cnSeg;
		this.affectPrevious = affectPrevious;
		this.pullInPreviousPosition = pullInPreviousPosition;
		connectedPoint = null;
		Reset(owner.owner.bodyChunks[1].pos);
	}

	public override void Update()
	{
		lastPos = pos;
		pos += vel;
		vel *= airFriction;
		stretched = 1f;
		if (connectedSegment != null)
		{
			if (!Custom.DistLess(pos, connectedSegment.pos, connectionRad))
			{
				float num = Vector2.Distance(pos, connectedSegment.pos);
				Vector2 vector = Custom.DirVec(pos, connectedSegment.pos);
				Vector2 vector2 = vector * ((connectionRad - num) * (1f - affectPrevious));
				Vector2 vector3 = vector * ((connectionRad - num) * affectPrevious);
				pos -= vector2;
				vel -= vector2;
				if (pullInPreviousPosition)
				{
					connectedSegment.pos += vector3;
				}
				connectedSegment.vel += vector3;
				stretched = Mathf.Clamp((connectionRad / (num * 0.5f) + 2f) / 3f, 0.2f, 1f);
			}
		}
		else if (connectedPoint.HasValue && !Custom.DistLess(pos, connectedPoint.Value, connectionRad))
		{
			float num2 = Vector2.Distance(pos, connectedPoint.Value);
			Vector2 vector4 = Custom.DirVec(pos, connectedPoint.Value) * (connectionRad - num2);
			pos -= vector4;
			vel -= vector4;
			stretched = Mathf.Clamp((connectionRad / (num2 * 0.5f) + 2f) / 3f, 0.2f, 1f);
		}
		if (connectedSegment != null)
		{
			PushOutOfTerrain(owner.owner.room, connectedSegment.pos);
		}
		else if (connectedPoint.HasValue)
		{
			PushOutOfTerrain(owner.owner.room, connectedPoint.Value);
		}
	}
}
