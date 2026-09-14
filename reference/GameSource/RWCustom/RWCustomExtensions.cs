using System.Runtime.CompilerServices;
using UnityEngine;

namespace RWCustom;

public static class RWCustomExtensions
{
	public static int IndexfOf(this IntVector2[] arr, IntVector2 item)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			if (item == arr[i])
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexfOf(this int[] arr, int item)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			if (item == arr[i])
			{
				return i;
			}
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Vec2(this WorldCoordinate obj)
	{
		return new Vector2(obj.x, obj.y) * 20f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntVector2 IntVec2(this WorldCoordinate obj)
	{
		return new IntVector2(obj.x, obj.y);
	}
}
