using System.Globalization;
using UnityEngine;

namespace RWCustom;

public struct BezierCurve
{
	public Vector2 posA;

	public Vector2 posB;

	public Vector2 handleA;

	public Vector2 handleB;

	public float cachedLength;

	public int cachedHash;

	public float Length
	{
		get
		{
			if (cachedHash == GetHashCode())
			{
				return cachedLength;
			}
			cachedLength = Custom.BezierLengthRecursive(posA, handleA, posB, handleB);
			cachedHash = GetHashCode();
			return cachedLength;
		}
	}

	public BezierCurve(Vector2 posA, Vector2 handleA, Vector2 posB, Vector2 handleB)
	{
		this = default(BezierCurve);
		this.posA = posA;
		this.posB = posB;
		this.handleA = handleA;
		this.handleB = handleB;
	}

	public Vector2 GetPoint(float f)
	{
		return Custom.Bezier(posA, handleA, posB, handleB, f);
	}

	public void Split(float f, out BezierCurve bezier1, out BezierCurve bezier2)
	{
		Custom.BezierSplit(this, f, out bezier1, out bezier2);
	}

	public override int GetHashCode()
	{
		return posA.GetHashCode() ^ posB.GetHashCode() ^ handleA.GetHashCode() ^ handleB.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is BezierCurve bezierCurve)
		{
			if (posA == bezierCurve.posA && posB == bezierCurve.posB && handleA == bezierCurve.handleA)
			{
				return handleB == bezierCurve.handleB;
			}
			return false;
		}
		return base.Equals(obj);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "[{0} - {1} - {2} - {3}]", posA, handleA, posB, handleB);
	}

	public static BezierCurve operator +(BezierCurve bezier, Vector2 pos)
	{
		return new BezierCurve(bezier.posA + pos, bezier.handleA + pos, bezier.posB + pos, bezier.handleB + pos);
	}

	public static BezierCurve operator *(BezierCurve bezier, float mult)
	{
		return new BezierCurve(bezier.posA * mult, bezier.handleA * mult, bezier.posB * mult, bezier.handleB * mult);
	}
}
