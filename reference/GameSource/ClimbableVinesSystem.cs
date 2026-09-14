using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class ClimbableVinesSystem : UpdatableAndDeletable
{
	public class VinePosition
	{
		public IClimbableVine vine;

		public float floatPos;

		public VinePosition(IClimbableVine vine, float floatPos)
		{
			this.vine = vine;
			this.floatPos = floatPos;
		}
	}

	public List<IClimbableVine> vines;

	public VineVisualizer viz;

	public ClimbableVinesSystem()
	{
		vines = new List<IClimbableVine>();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}

	public VinePosition VineOverlap(Vector2 pos, float rad)
	{
		float num = float.MaxValue;
		VinePosition result = null;
		for (int i = 0; i < vines.Count; i++)
		{
			if (!vines[i].CurrentlyClimbable())
			{
				continue;
			}
			for (int j = 0; j < vines[i].TotalPositions() - 1; j++)
			{
				if (OverlappingSegment(vines[i].Pos(j), vines[i].Rad(j), vines[i].Pos(j + 1), vines[i].Rad(j + 1), pos, rad))
				{
					Vector2 b = ClosestPointOnSegment(vines[i].Pos(j), vines[i].Pos(j + 1), pos);
					float num2 = Vector2.Distance(pos, b);
					if (num2 < num)
					{
						num = num2;
						float t = Mathf.InverseLerp(0f, Vector2.Distance(vines[i].Pos(j), vines[i].Pos(j + 1)), Vector2.Distance(vines[i].Pos(j), b));
						result = new VinePosition(vines[i], Mathf.Lerp(FloatAtSegment(vines[i], j), FloatAtSegment(vines[i], j + 1), t));
					}
				}
			}
		}
		return result;
	}

	public Vector2 OnVinePos(VinePosition vPos)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vPos.vine.TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		return Vector2.Lerp(vPos.vine.Pos(num), vPos.vine.Pos(num2), t);
	}

	public float VineRad(VinePosition vPos)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vPos.vine.TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		return Mathf.Lerp(vPos.vine.Rad(num), vPos.vine.Rad(num2), t);
	}

	public Vector2 VineDir(VinePosition vPos)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vPos.vine.TotalPositions() - 1);
		if (num == num2)
		{
			return Custom.DirVec(vPos.vine.Pos(vPos.vine.TotalPositions() - 2), vPos.vine.Pos(vPos.vine.TotalPositions() - 1));
		}
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		return Vector2.Lerp(DirOfSegment(vPos.vine, num), DirOfSegment(vPos.vine, num2), t).normalized;
	}

	public Vector2 DirOfSegment(IClimbableVine vine, int seg)
	{
		if (seg == vine.TotalPositions() - 1)
		{
			return Custom.DirVec(vine.Pos(vine.TotalPositions() - 2), vine.Pos(vine.TotalPositions() - 1));
		}
		return Custom.DirVec(vine.Pos(seg), vine.Pos(seg + 1));
	}

	public float ClimbOnVineSpeed(VinePosition vPos, Vector2 goalPos)
	{
		if (vPos.floatPos == 0f)
		{
			return 1f;
		}
		if (vPos.floatPos == 1f)
		{
			return -1f;
		}
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vPos.vine.TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		Vector2 vector = Vector2.Lerp(vPos.vine.Pos(num), vPos.vine.Pos(num2), t);
		float f = Vector2.Dot((vPos.vine.Pos(num) - vPos.vine.Pos(num2)).normalized, (vector - goalPos).normalized);
		return Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(f)) * Mathf.Sign(f);
	}

	public VinePosition VineSwitch(VinePosition vPos, Vector2 goalPos, float rad)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vPos.vine.TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		Vector2 vector = Vector2.Lerp(vPos.vine.Pos(num), vPos.vine.Pos(num2), t);
		if (Mathf.Abs(Vector2.Dot((vPos.vine.Pos(num) - vPos.vine.Pos(num2)).normalized, (vector - goalPos).normalized)) > 0.5f)
		{
			return null;
		}
		float num3 = float.MaxValue;
		VinePosition result = null;
		for (int i = 0; i < vines.Count; i++)
		{
			if (!vines[i].CurrentlyClimbable())
			{
				continue;
			}
			for (int j = 0; j < vines[i].TotalPositions() - 1; j++)
			{
				if (!OverlappingSegment(vines[i].Pos(j), vines[i].Rad(j), vines[i].Pos(j + 1), vines[i].Rad(j + 1), vector, rad))
				{
					continue;
				}
				Vector2 vector2 = ClosestPointOnSegment(vines[i].Pos(j), vines[i].Pos(j + 1), vector);
				float num4 = Vector2.Distance(vector2, goalPos);
				num4 *= 1f - 0.25f * Mathf.Abs(Vector2.Dot((vines[i].Pos(j) - vines[i].Pos(j + 1)).normalized, (vector - goalPos).normalized));
				if (vines[i] == vPos.vine)
				{
					float num5 = Mathf.Lerp(FloatAtSegment(vines[i], j), FloatAtSegment(vines[i], j + 1), Mathf.InverseLerp(0f, Vector2.Distance(vines[i].Pos(j), vines[i].Pos(j + 1)), Vector2.Distance(vines[i].Pos(j), vector2))) * TotalLength(vines[i]);
					if (Mathf.Abs(vPos.floatPos * TotalLength(vPos.vine) - num5) < 100f)
					{
						num4 = float.MaxValue;
					}
				}
				if (num4 < num3)
				{
					num3 = num4;
					float t2 = Mathf.InverseLerp(0f, Vector2.Distance(vines[i].Pos(j), vines[i].Pos(j + 1)), Vector2.Distance(vines[i].Pos(j), vector2));
					result = new VinePosition(vines[i], Mathf.Lerp(FloatAtSegment(vines[i], j), FloatAtSegment(vines[i], j + 1), t2));
				}
			}
		}
		return result;
	}

	public float TotalLength(IClimbableVine vine)
	{
		float num = 0f;
		for (int i = 0; i < vine.TotalPositions() - 1; i++)
		{
			num += Vector2.Distance(vine.Pos(i), vine.Pos(i + 1));
		}
		return num;
	}

	public float FloatAtSegment(IClimbableVine vine, int segment)
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < vine.TotalPositions() - 1; i++)
		{
			if (i < segment)
			{
				num2 += Vector2.Distance(vine.Pos(i), vine.Pos(i + 1));
			}
			num += Vector2.Distance(vine.Pos(i), vine.Pos(i + 1));
		}
		return num2 / num;
	}

	public int PrevSegAtFloat(IClimbableVine vine, float fPos)
	{
		fPos *= TotalLength(vine);
		float num = 0f;
		for (int i = 0; i < vine.TotalPositions() - 1; i++)
		{
			num += Vector2.Distance(vine.Pos(i), vine.Pos(i + 1));
			if (num > fPos)
			{
				return i;
			}
		}
		return vine.TotalPositions() - 1;
	}

	public bool OverlappingSegment(Vector2 A, float aRad, Vector2 B, float bRad, Vector2 testPos, float tRad)
	{
		Vector2 vector = Custom.PerpendicularVector((A - B).normalized);
		if (Custom.DistanceToLine(testPos, A + vector, A - vector) >= 0f)
		{
			return Vector2.Distance(testPos, A) < aRad + tRad;
		}
		if (Custom.DistanceToLine(testPos, B + vector, B - vector) <= 0f)
		{
			return Vector2.Distance(testPos, B) < bRad + tRad;
		}
		Vector2 vector2 = Custom.ClosestPointOnLine(A, B, testPos);
		return Vector2.Distance(vector2, testPos) < Custom.LerpMap(Vector2.Distance(A, vector2), 0f, Vector2.Distance(A, B), aRad, bRad) + tRad;
	}

	public Vector2 ClosestPointOnSegment(Vector2 A, Vector2 B, Vector2 testPos)
	{
		Vector2 vector = Custom.PerpendicularVector((A - B).normalized);
		if (Custom.DistanceToLine(testPos, A + vector, A - vector) >= 0f)
		{
			return A;
		}
		if (Custom.DistanceToLine(testPos, B + vector, B - vector) <= 0f)
		{
			return B;
		}
		return Custom.ClosestPointOnLine(A, B, testPos);
	}

	public void ConnectChunkToVine(BodyChunk chunk, VinePosition vPos, float conRad)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vPos.vine.TotalPositions() - 1);
		float t = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		Vector2 vector = Vector2.Lerp(vPos.vine.Pos(num), vPos.vine.Pos(num2), t);
		float num3 = chunk.mass / (chunk.mass + Mathf.Lerp(vPos.vine.Mass(num), vPos.vine.Mass(num2), t));
		float num4 = Vector2.Distance(chunk.pos, vector);
		Vector2 vector2 = Custom.DirVec(chunk.pos, vector);
		if (num4 > conRad)
		{
			Vector2 vector3 = vector2 * ((num4 - conRad) * (1f - num3));
			Vector2 movement = -vector2 * ((num4 - conRad) * num3);
			chunk.pos += vector3;
			chunk.vel += vector3;
			vPos.vine.Push(num, movement);
			vPos.vine.Push(num2, movement);
		}
	}

	public void PushAtVine(VinePosition vPos, Vector2 push)
	{
		int num = PrevSegAtFloat(vPos.vine, vPos.floatPos);
		int num2 = Custom.IntClamp(num + 1, 0, vPos.vine.TotalPositions() - 1);
		float num3 = Mathf.InverseLerp(FloatAtSegment(vPos.vine, num), FloatAtSegment(vPos.vine, num2), vPos.floatPos);
		float num4 = Mathf.Max(1f, Mathf.Lerp(vPos.vine.Mass(num), vPos.vine.Mass(num2), num3));
		vPos.vine.Push(num, push / num4 * (1f - num3));
		vPos.vine.Push(num2, push / num4 * num3);
	}

	public void VineBeingClimbedOn(VinePosition vPos, Creature crit)
	{
		vPos.vine.BeingClimbedOn(crit);
	}

	public bool VineCurrentlyClimbable(VinePosition vPos)
	{
		return vPos.vine.CurrentlyClimbable();
	}

	public IClimbableVine GetVineObject(VinePosition vPos)
	{
		return vPos.vine;
	}
}
