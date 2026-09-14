using RWCustom;
using UnityEngine;

public class SegmentRope
{
	public SimpleSegment[] segments;

	public float idealLength;

	public bool terrainCollision;

	public float drag = 0.999f;

	public float gravity = 0.9f;

	public bool pushSegments;

	public SegmentRope(int segments, float idealLength)
	{
		this.segments = new SimpleSegment[segments];
		this.idealLength = idealLength;
	}

	public virtual void UpdateRope(Room room, Vector2? from, Vector2? to)
	{
		for (int i = 0; i < segments.Length; i++)
		{
			segments[i].vel.y -= gravity * room.gravity;
			segments[i].lastPos = segments[i].pos;
			segments[i].pos += segments[i].vel;
			segments[i].vel *= drag;
			if (terrainCollision && (i != 0 || !from.HasValue) && (i != segments.Length - 1 || !to.HasValue) && (room.aimap == null || room.aimap.getTerrainProximity(segments[i].pos) < 3) && terrainCollision)
			{
				SharedPhysics.TerrainCollisionData cd = new SharedPhysics.TerrainCollisionData(segments[i].pos, segments[i].lastPos, segments[i].vel, 2f, new IntVector2(0, 0), goThroughFloors: true);
				cd = SharedPhysics.VerticalCollision(room, cd);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				segments[i].pos = cd.pos;
				segments[i].vel = cd.vel;
				if (cd.contactPoint.x != 0)
				{
					segments[i].vel.y *= 0.6f;
				}
				if (cd.contactPoint.y != 0)
				{
					segments[i].vel.x *= 0.6f;
				}
			}
		}
		ConnectEnd(0, from);
		ConnectEnd(segments.Length - 1, to);
		for (int num = segments.Length - 1; num > 0; num--)
		{
			ConnectSegments(num, num - 1);
		}
		ConnectEnd(0, from);
		ConnectEnd(segments.Length - 1, to);
		for (int j = 1; j < segments.Length; j++)
		{
			ConnectSegments(j, j - 1);
		}
		ConnectEnd(0, from);
		ConnectEnd(segments.Length - 1, to);
	}

	public void ConnectSegments(int a, int b)
	{
		float num = idealLength / (float)(segments.Length - 1) * 0.5f;
		Vector2 normalized = (segments[a].pos - segments[b].pos).normalized;
		float num2 = Vector2.Distance(segments[a].pos, segments[b].pos);
		float num3 = Mathf.InverseLerp(0f, num, num2);
		if (num2 > num || pushSegments)
		{
			Vector2 vector = normalized * ((num - num2) * 0.5f * num3);
			segments[a].pos += vector;
			segments[a].vel += vector;
			segments[b].pos -= vector;
			segments[b].vel -= vector;
		}
	}

	public void ConnectEnd(int segment, Vector2? pos)
	{
		if (pos.HasValue)
		{
			segments[segment].pos = pos.Value;
			segments[segment].vel = segments[segment].pos - segments[segment].lastPos;
		}
	}

	public void Reset(Vector2 from, Vector2 to)
	{
		for (int i = 0; i < segments.Length; i++)
		{
			segments[i].Reset(Vector2.Lerp(from, to, (float)i / (float)(segments.Length - 1)));
		}
	}
}
