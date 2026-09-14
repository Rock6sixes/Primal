using System;
using Unity.Mathematics;
using UnityEngine;
using Watcher;

public class SentientRotSporesSwitch : CosmeticSprite
{
	public float initialEffectAmount;

	public float lastEffectAmount;

	public Vector2[] positions;

	public Vector2[] previousPositions;

	public float[] xRotations;

	public float[] yRotations;

	public int[] lifetimes;

	public int hideAmount;

	public int updateOffset;

	public SentientRotSporesSwitch(Room room)
	{
		base.room = room;
		initialEffectAmount = room.roomSettings.GetEffect(WatcherEnums.RoomEffectType.SentientRotParticles)?.amount ?? 0f;
		initialEffectAmount *= 1f - global::Watcher.Watcher.cfgReduceFallingParticles.Value;
		lastEffectAmount = initialEffectAmount;
		float num = initialEffectAmount / 2500f;
		int num2 = Mathf.Clamp((int)(base.room.PixelWidth * base.room.PixelHeight * num), 1, 824);
		positions = new Vector2[num2];
		previousPositions = new Vector2[num2];
		xRotations = new float[num2];
		yRotations = new float[num2];
		lifetimes = new int[num2];
		for (int i = 0; i < num2; i++)
		{
			positions[i] = room.RandomPos();
			previousPositions[i] = positions[i];
			xRotations[i] = UnityEngine.Random.value * 360f;
			yRotations[i] = UnityEngine.Random.value * (float)Math.PI * 2f;
			lifetimes[i] = UnityEngine.Random.Range(0, 200);
		}
	}

	public float Mod(float x, float a)
	{
		return x - Mathf.Floor(x / a) * a;
	}

	public float3 Mod(float3 x, float a)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return x - Floor(x / a) * a;
	}

	public float4 Mod(float4 x, float a)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return x - Floor(x / a) * a;
	}

	public float Permute(float x)
	{
		return Mathf.Floor(Mod((x * 34f + 1f) * x, 289f));
	}

	public float3 Permute(float3 x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return Mod((x * 34f + 1f) * x, 289f);
	}

	public float4 Permute(float4 x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return Mod((x * 34f + 1f) * x, 289f);
	}

	public float TaylorInvSqrt(float r)
	{
		return 1.7928429f - 0.85373473f * r;
	}

	public float4 TaylorInvSqrt(float4 r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		return new float4(TaylorInvSqrt(r.x), TaylorInvSqrt(r.y), TaylorInvSqrt(r.z), TaylorInvSqrt(r.w));
	}

	public float Step(float y, float x)
	{
		if (!(x >= y))
		{
			return 0f;
		}
		return 1f;
	}

	public float3 Step(float3 y, float3 x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		return new float3(Step(y.x, x.x), Step(y.y, x.y), Step(y.z, x.z));
	}

	public float4 Step(float4 y, float4 x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		return new float4(Step(y.x, x.x), Step(y.y, x.y), Step(y.z, x.z), Step(y.w, x.w));
	}

	public float Dot(float3 A, float3 B)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return A.x * B.x + A.y * B.y + A.z * B.z;
	}

	public float Dot(float4 A, float4 B)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		return A.x * B.x + A.y * B.y + A.z * B.z + A.w * B.w;
	}

	public float3 Floor(float3 v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return new float3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
	}

	public float4 Floor(float4 v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return new float4(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z), Mathf.Floor(v.w));
	}

	public float4 Abs(float4 v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return new float4(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z), Mathf.Abs(v.w));
	}

	public float3 Min(float3 A, float3 B)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		return new float3(Mathf.Min(A.x, B.x), Mathf.Min(A.y, B.y), Mathf.Min(A.z, B.z));
	}

	public float3 Max(float3 A, float3 B)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		return new float3(Mathf.Max(A.x, B.x), Mathf.Max(A.y, B.y), Mathf.Max(A.z, B.z));
	}

	public float4 Max(float4 A, float4 B)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		return new float4(Mathf.Max(A.x, B.x), Mathf.Max(A.y, B.y), Mathf.Max(A.z, B.z), Mathf.Max(A.w, B.w));
	}

	public float SimplexNoise(float3 v)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		float3 val = default(float3);
		((float3)(ref val))._002Ector(1f / 6f, 1f / 6f, 1f / 6f);
		float4 val2 = default(float4);
		((float4)(ref val2))._002Ector(0f, 0.5f, 1f, 2f);
		float3 val3 = Floor(v + Dot(v, new float3(1f / 3f, 1f / 3f, 1f / 3f)));
		float3 val4 = v - val3 + Dot(val3, val);
		float3 a = Step(((float3)(ref val4)).yzx, val4);
		float3 b = 1f - ((float3)(ref a)).zxy;
		float3 val5 = Min(a, b);
		float3 val6 = Max(a, b);
		float3 val7 = val4 - val5 + 1f * val;
		float3 val8 = val4 - val6 + 2f * val;
		float3 val9 = val4 - 1f + 3f * val;
		val3 = Mod(val3, 289f);
		float4 val10 = Permute(Permute(Permute(val3.z + new float4(0f, val5.z, val6.z, 1f)) + val3.y + new float4(0f, val5.y, val6.y, 1f)) + val3.x + new float4(0f, val5.x, val6.x, 1f));
		float3 val11 = 1f / 7f * ((float4)(ref val2)).wyz - ((float4)(ref val2)).xzx;
		float4 val12 = val10 - 49f * Floor(val10 * val11.z * val11.z);
		float4 val13 = Floor(val12 * val11.z);
		float4 val14 = Floor(val12 - 7f * val13);
		float4 v2 = val13 * val11.x + ((float3)(ref val11)).yyyy;
		float4 v3 = val14 * val11.x + ((float3)(ref val11)).yyyy;
		float4 val15 = 1f - Abs(v2) - Abs(v3);
		float4 v4 = default(float4);
		((float4)(ref v4))._002Ector(((float4)(ref v2)).xy, ((float4)(ref v3)).xy);
		float4 v5 = default(float4);
		((float4)(ref v5))._002Ector(((float4)(ref v2)).zw, ((float4)(ref v3)).zw);
		float4 val16 = Floor(v4) * 2f + 1f;
		float4 val17 = Floor(v5) * 2f + 1f;
		float4 val18 = -Step(val15, new float4(0f, 0f, 0f, 0f));
		float4 val19 = ((float4)(ref v4)).xzyw + ((float4)(ref val16)).xzyw * ((float4)(ref val18)).xxyy;
		float4 val20 = ((float4)(ref v5)).xzyw + ((float4)(ref val17)).xzyw * ((float4)(ref val18)).zzww;
		float3 val21 = default(float3);
		((float3)(ref val21))._002Ector(((float4)(ref val19)).xy, val15.x);
		float3 val22 = default(float3);
		((float3)(ref val22))._002Ector(((float4)(ref val19)).zw, val15.y);
		float3 val23 = default(float3);
		((float3)(ref val23))._002Ector(((float4)(ref val20)).xy, val15.z);
		float3 val24 = default(float3);
		((float3)(ref val24))._002Ector(((float4)(ref val20)).zw, val15.w);
		float4 val25 = TaylorInvSqrt(new float4(Dot(val21, val21), Dot(val22, val22), Dot(val23, val23), Dot(val24, val24)));
		val21 *= val25.x;
		val22 *= val25.y;
		val23 *= val25.z;
		val24 *= val25.w;
		float4 val26 = Max(0.6f - new float4(Dot(val4, val4), Dot(val7, val7), Dot(val8, val8), Dot(val9, val9)), float4.zero);
		val26 *= val26;
		return 42f * Dot(val26 * val26, new float4(Dot(val21, val4), Dot(val22, val7), Dot(val23, val8), Dot(val24, val9)));
	}

	public override void Update(bool eu)
	{
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		base.Update(eu);
		if (!room.BeingViewed || base.slatedForDeletetion)
		{
			return;
		}
		RoomSettings.RoomEffect effect = room.roomSettings.GetEffect(WatcherEnums.RoomEffectType.SentientRotParticles);
		if (effect != null)
		{
			float num = effect.amount * (1f - global::Watcher.Watcher.cfgReduceFallingParticles.Value);
			if (num != lastEffectAmount)
			{
				hideAmount = (int)(Mathf.InverseLerp(initialEffectAmount, 0f, num) * (float)positions.Length);
			}
			lastEffectAmount = effect.amount;
		}
		int num2 = 10;
		for (int i = 0; i < positions.Length; i++)
		{
			yRotations[i] += 0.175f;
			if (yRotations[i] > (float)Math.PI * 2f)
			{
				yRotations[i] -= (float)Math.PI * 2f;
			}
		}
		for (int j = updateOffset; j < positions.Length - hideAmount; j += num2)
		{
			lifetimes[j]++;
			if (lifetimes[j] >= 200)
			{
				positions[j] = room.RandomPos();
				previousPositions[j] = positions[j];
				lifetimes[j] = 0;
			}
			Vector2 vector = positions[j] - previousPositions[j];
			previousPositions[j] = positions[j];
			Vector2 vector2 = positions[j] * 0.01f;
			Vector2 vector3 = new Vector2(SimplexNoise(new float3(vector2.x, vector2.y, (float)room.game.clock * 0.002f)), SimplexNoise(new float3(vector2.x, vector2.y, -3000f + (float)room.game.clock * 0.002f))).normalized * 0.2f;
			vector = vector * 0.95f + vector3;
			positions[j] += vector;
		}
		updateOffset = (updateOffset + 1) % num2;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (!base.slatedForDeletetion)
		{
			sLeaser.sprites = new FSprite[positions.Length];
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i] = new FSprite("Pebble" + UnityEngine.Random.Range(1, 15));
			}
			AddToContainer(sLeaser, rCam, null);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (!base.slatedForDeletetion)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].x = positions[i].x - camPos.x;
				sLeaser.sprites[i].y = positions[i].y - camPos.y;
				sLeaser.sprites[i].rotation = xRotations[i];
				sLeaser.sprites[i].scaleY = yRotations[i] / ((float)Math.PI * 2f);
				sLeaser.sprites[i].scaleX = 1f - sLeaser.sprites[i].scaleY;
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (!base.slatedForDeletetion)
		{
			base.ApplyPalette(sLeaser, rCam, palette);
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].color = palette.blackColor;
			}
		}
	}
}
