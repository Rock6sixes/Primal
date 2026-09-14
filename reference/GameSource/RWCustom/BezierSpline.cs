using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace RWCustom;

public class BezierSpline
{
	public struct Midpoint
	{
		public Vector2 pos;

		public float PerpendicularAngle;

		public float length1;

		public float length2;

		public Vector2 dir1
		{
			get
			{
				return Custom.DegToVec(PerpendicularAngle + 90f) * length1;
			}
			set
			{
				length1 = value.magnitude;
				PerpendicularAngle = Custom.VecToDeg(value) - 90f;
			}
		}

		public Vector2 dir2
		{
			get
			{
				return Custom.DegToVec(PerpendicularAngle - 90f) * length2;
			}
			set
			{
				length2 = value.magnitude;
				PerpendicularAngle = Custom.VecToDeg(value) + 90f;
			}
		}

		public Midpoint(Vector2 pos, float PerpendicularAngle, float length1, float length2)
		{
			this = default(Midpoint);
			this.pos = pos;
			this.PerpendicularAngle = PerpendicularAngle;
			this.length1 = length1;
			this.length2 = length2;
		}

		public Midpoint Floor()
		{
			return new Midpoint(new Vector2(Mathf.Floor(pos.x), Mathf.Floor(pos.y)), PerpendicularAngle, Mathf.Floor(length1), Mathf.Floor(length2));
		}

		public static Midpoint FromString(string s)
		{
			string[] array = s.Split('^');
			Midpoint result = default(Midpoint);
			result.pos = new Vector2(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture));
			result.PerpendicularAngle = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			result.length1 = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			result.length2 = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			return result;
		}

		public override string ToString()
		{
			return string.Join("^", pos.x.ToString(CultureInfo.InvariantCulture), pos.y.ToString(CultureInfo.InvariantCulture), PerpendicularAngle.ToString(CultureInfo.InvariantCulture), length1.ToString(CultureInfo.InvariantCulture), length2.ToString(CultureInfo.InvariantCulture));
		}
	}

	public Vector2 posA;

	public Vector2 posB;

	public Vector2 handleA;

	public Vector2 handleB;

	public List<Midpoint> midpoints = new List<Midpoint>();

	public float?[] _cachedLengths = new float?[0];

	public int Segments => midpoints.Count + 1;

	public float?[] cachedLengths
	{
		get
		{
			if (_cachedLengths.Length != Segments)
			{
				_cachedLengths = new float?[Segments];
			}
			return _cachedLengths;
		}
	}

	public float GetFullLength
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < Segments; i++)
			{
				num += GetSegmentLength(i);
			}
			return num;
		}
	}

	public BezierSpline()
	{
	}

	public BezierSpline(Vector2 posA, Vector2 handleA, Vector2 posB, Vector2 handleB, params Midpoint[] midSegments)
	{
		this.posA = posA;
		this.posB = posB;
		this.handleA = handleA;
		this.handleB = handleB;
		midpoints = midSegments.ToList();
	}

	public float GetSegmentLength(int segment)
	{
		float? num = cachedLengths[segment];
		if (num.HasValue)
		{
			return num.GetValueOrDefault();
		}
		float length = GetBezier(segment).Length;
		cachedLengths[segment] = length;
		return length;
	}

	public BezierCurve[] GetAllBeziers()
	{
		BezierCurve[] array = new BezierCurve[Segments];
		for (int i = 0; i < Segments; i++)
		{
			array[i] = GetBezier(i);
		}
		return array;
	}

	public BezierCurve GetBezier(int segment)
	{
		BezierCurve result = default(BezierCurve);
		if (segment == 0)
		{
			result.posA = posA;
			result.handleA = handleA;
		}
		else
		{
			result.posA = midpoints[segment - 1].pos;
			result.handleA = midpoints[segment - 1].dir2 + midpoints[segment - 1].pos;
		}
		if (segment == midpoints.Count)
		{
			result.posB = posB;
			result.handleB = handleB;
		}
		else
		{
			result.posB = midpoints[segment].pos;
			result.handleB = midpoints[segment].dir1 + midpoints[segment].pos;
		}
		return result;
	}

	public void SetBezier(int segment, BezierCurve bezier)
	{
		if (segment == 0)
		{
			posA = bezier.posA;
			handleA = bezier.handleA;
		}
		else
		{
			Midpoint value = midpoints[segment - 1];
			value.pos = bezier.posA;
			value.dir2 = bezier.handleA - bezier.posA;
			midpoints[segment - 1] = value;
		}
		if (segment == midpoints.Count)
		{
			posB = bezier.posB;
			handleB = bezier.handleB;
		}
		else
		{
			Midpoint value2 = midpoints[segment];
			value2.pos = bezier.posB;
			value2.dir1 = bezier.handleB - bezier.posB;
			midpoints[segment] = value2;
		}
		cachedLengths[segment] = null;
	}

	public void AddMidpoint(Midpoint midpoint, int afterSegment)
	{
		midpoints.Insert(afterSegment, midpoint);
		_cachedLengths = new float?[Segments];
	}

	public void RemoveMidpoint(int midpoint)
	{
		midpoints.RemoveAt(midpoint);
		_cachedLengths = new float?[Segments];
	}

	public void Split(int segment, float point, out BezierSpline spline1, out BezierSpline spline2)
	{
		GetBezier(segment).Split(point, out var bezier, out var bezier2);
		List<Midpoint> list = new List<Midpoint>();
		for (int i = 0; i < segment; i++)
		{
			list.Add(midpoints[i]);
		}
		spline1 = new BezierSpline(posA, handleA, posB, handleB, list.ToArray());
		spline1.SetBezier(segment, bezier);
		list = new List<Midpoint>();
		for (int j = segment; j < Segments - 1; j++)
		{
			list.Add(midpoints[j]);
		}
		spline2 = new BezierSpline(posA, handleA, posB, handleB, list.ToArray());
		spline2.SetBezier(0, bezier2);
	}

	public BezierSpline GetSection(int segment1, float point1, int segment2, float point2)
	{
		Split(segment2, point2, out var spline, out var spline2);
		spline.Split(segment1, point1, out spline2, out var spline3);
		return spline3;
	}
}
