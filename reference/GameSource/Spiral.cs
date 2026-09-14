using System;
using UnityEngine;

public struct Spiral
{
	public float innerRadius;

	public float ringSpacing;

	public Spiral(float innerRadius, float ringSpacing)
	{
		this.innerRadius = innerRadius;
		this.ringSpacing = ringSpacing;
	}

	public readonly float LengthToAngle(float x)
	{
		return (float)Math.PI * 2f / ringSpacing * (Mathf.Sqrt(innerRadius * innerRadius + x * ringSpacing / (float)Math.PI) - innerRadius);
	}

	public readonly Vector2 LengthToPos(float x)
	{
		return AngleToPos(LengthToAngle(x));
	}

	public readonly Vector2 LengthToDir(float x)
	{
		return AngleToDir(LengthToAngle(x));
	}

	public readonly float AngleToLength(float radians)
	{
		return ringSpacing * radians * radians / ((float)Math.PI * 4f) + innerRadius * radians;
	}

	public readonly Vector2 AngleToPos(float radians)
	{
		return (radians * ringSpacing / ((float)Math.PI * 2f) + innerRadius) * new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
	}

	public readonly Vector2 AngleToDir(float radians)
	{
		float num = Mathf.Cos(radians);
		float num2 = Mathf.Sin(radians);
		float num3 = ringSpacing / ((float)Math.PI * 2f);
		float num4 = ringSpacing * radians / ((float)Math.PI * 2f) + innerRadius;
		return new Vector2(num * num3 - num2 * num4, num2 * num3 + num * num4).normalized;
	}
}
