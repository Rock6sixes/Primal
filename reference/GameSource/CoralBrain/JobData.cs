using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace CoralBrain;

public struct JobData
{
	[ReadOnly]
	public int2[] naFourDirections;

	public float2[] nativePoints;

	public Vector2[] outPoints;
}
