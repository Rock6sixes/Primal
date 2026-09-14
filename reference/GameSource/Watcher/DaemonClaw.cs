using RWCustom;
using UnityEngine;

namespace Watcher;

public class DaemonClaw : UpdatableAndDeletable, IDrawable
{
	public Vector2 basePos;

	public Vector2 baseDirection;

	public Vector2 ropePos;

	public (float current, float last)[] armAngles;

	public float[] armAngularVelocities;

	public Vector2[] desiredArmPositions;

	public static float[] armLengths = new float[3] { 180f, 15f, 140f };

	public static float totalArmLength = armLengths[0] + armLengths[1] + armLengths[2];

	public bool mirrored;

	public Vector2 targetPos;

	public int totalSprites;

	public int baseGoopSpriteStartIndex;

	public int arm1SpriteStartIndex;

	public int arm2SpriteStartIndex;

	public int arm3SpriteStartIndex;

	public int armGooSpriteStartIndex;

	public int clawSpriteStartIndex;

	public int armRopeSpriteStartIndex;

	public Vector2[] armGooOffsets;

	public float armGooRadius = 8f;

	public Vector2[] baseGooOffsets;

	public float baseGooRadius = 30f;

	public Vector2 clawPos;

	public int threadIndex;

	public Counter cycleCounter;

	public int threadCycle;

	public int nThreads;

	public WeaverThread[] threads;

	public float handScale = 1.25f;

	public float ropePosOffset = 30f;

	public static Color daemonColor = new Color(0.03137255f, 0.03137255f, 4f / 85f);

	public bool canSpawnThread
	{
		get
		{
			if (Custom.rainWorld.progression.miscProgressionData.beaten_Watcher_SpinningTop && Custom.rainWorld.progression.miscProgressionData.beaten_Watcher_VoidWeaver)
			{
				return room.game.GetStorySession.saveState.miscWorldSaveData.SentientRotAscensionUnlock;
			}
			return false;
		}
	}

	public DaemonClaw(Room room, Vector2 basePos, Vector2 direction)
	{
		Random.State state = Random.state;
		Random.InitState(0);
		base.room = room;
		this.basePos = basePos;
		targetPos = basePos + direction;
		armAngles = new(float, float)[armLengths.Length];
		armAngularVelocities = new float[armLengths.Length];
		desiredArmPositions = new Vector2[armLengths.Length + 1];
		baseDirection = direction.normalized;
		mirrored = baseDirection.x < 0f;
		ropePos = basePos + Vector2.Perpendicular(baseDirection) * ropePosOffset;
		float num = 0f;
		desiredArmPositions[0] = basePos;
		for (int i = 1; i < desiredArmPositions.Length; i++)
		{
			num += armLengths[i - 1];
			desiredArmPositions[i] = basePos + baseDirection * num;
		}
		armAngles[0].current = Custom.VecToDeg(direction);
		armAngles[0].last = armAngles[0].current;
		int num2 = 24;
		int num3 = 24;
		armGooOffsets = new Vector2[num2 + num3];
		for (int j = 0; j < armGooOffsets.Length; j++)
		{
			if (j < num2)
			{
				float ang = Random.value * 360f;
				float num4 = Mathf.Sqrt(Random.value) * armGooRadius;
				armGooOffsets[j] = Custom.DegToVec(ang) * num4;
			}
			else
			{
				float value = Random.value;
				float y = Mathf.Lerp(0f, armLengths[2] / -3f, value);
				float num5 = Mathf.Lerp(0f - armGooRadius, armGooRadius, Random.value) * (1f - value) * (1f - value);
				armGooOffsets[j] = new Vector2(num5 + value * -20f * (float)((!mirrored) ? 1 : (-1)), y);
			}
		}
		baseGooOffsets = new Vector2[32];
		for (int k = 0; k < baseGooOffsets.Length; k++)
		{
			float ang2 = Random.value * 360f;
			float num6 = Mathf.Sqrt(Random.value) * baseGooRadius;
			baseGooOffsets[k] = Custom.DegToVec(ang2) * num6;
		}
		totalSprites = 0;
		baseGoopSpriteStartIndex = totalSprites;
		totalSprites += baseGooOffsets.Length;
		arm1SpriteStartIndex = totalSprites;
		totalSprites += 2;
		arm2SpriteStartIndex = totalSprites;
		totalSprites += 3;
		arm3SpriteStartIndex = totalSprites;
		totalSprites++;
		armGooSpriteStartIndex = totalSprites;
		totalSprites += armGooOffsets.Length;
		clawSpriteStartIndex = totalSprites;
		totalSprites += 3;
		targetPos = basePos + baseDirection * (totalArmLength * 0.7f) + Random.insideUnitCircle * (totalArmLength * 0.3f);
		if (canSpawnThread)
		{
			cycleCounter = new Counter(Random.Range(20, 40));
			nThreads = Random.Range(3, 7);
			threads = new WeaverThread[nThreads];
			for (int l = 0; l < nThreads; l++)
			{
				Vector2 vector = RandomClawPos(biggerArea: true);
				Vector2 vector2 = RandomClawPos(biggerArea: true);
				threads[l] = new WeaverThread(room, 600f, vector, vector2, stuckAtA: true, stuckAtB: true)
				{
					bothSides = true,
					conRad = 10f,
					stuckPosA = vector,
					stuckPosB = vector2
				};
				threads[l].graphic.thickness = Random.Range(0.3f, 0.7f);
				room.AddObject(threads[l]);
			}
		}
		Random.state = state;
	}

	public override void Update(bool eu)
	{
		cycleCounter?.Tick();
		if ((threadCycle == 0 || threadCycle == 6) && Random.Range(0, (threads == null) ? 200 : 50) == 1)
		{
			targetPos = RandomClawPos();
			if (threads != null)
			{
				cycleCounter.Reset();
				threadCycle++;
			}
		}
		CycleThread();
		MoveThread();
		if (!room.BeingViewed)
		{
			clawPos = targetPos;
		}
		Vector2 vector = Custom.rotateVectorDeg((targetPos - basePos).normalized, 90 * ((!mirrored) ? 1 : (-1)));
		Vector2 vector2 = Custom.rotateVectorDeg((targetPos - basePos).normalized, 90 * (mirrored ? 1 : (-1)));
		float num = 0f;
		for (int i = 1; i < desiredArmPositions.Length; i++)
		{
			num += armLengths[i - 1];
			desiredArmPositions[i] = basePos + (((float)i > (float)desiredArmPositions.Length / 2f) ? vector : vector2) * num;
		}
		for (int j = 0; j < 32; j++)
		{
			desiredArmPositions[desiredArmPositions.Length - 1] = targetPos;
			for (int num2 = desiredArmPositions.Length - 2; num2 >= 0; num2--)
			{
				Vector2 normalized = (desiredArmPositions[num2] - desiredArmPositions[num2 + 1]).normalized;
				desiredArmPositions[num2] = desiredArmPositions[num2 + 1] + normalized * armLengths[num2];
			}
			desiredArmPositions[0] = basePos;
			for (int k = 1; k < desiredArmPositions.Length; k++)
			{
				Vector2 normalized2 = (desiredArmPositions[k] - desiredArmPositions[k - 1]).normalized;
				desiredArmPositions[k] = desiredArmPositions[k - 1] + normalized2 * armLengths[k - 1];
			}
		}
		for (int l = 0; l < armAngles.Length; l++)
		{
			armAngles[l].last = armAngles[l].current;
		}
		Vector2 to = Vector2.up;
		for (int m = 0; m < armAngles.Length; m++)
		{
			armAngles[m].current += armAngularVelocities[m];
			Vector2 normalized3 = (desiredArmPositions[m + 1] - desiredArmPositions[m]).normalized;
			float target = ((m == 0) ? Custom.VecToDeg(normalized3) : Vector2.SignedAngle(normalized3, to));
			float num3 = Mathf.DeltaAngle(armAngles[m].current, target) * 0.01f;
			armAngularVelocities[m] *= 0.9f;
			armAngularVelocities[m] += num3;
			to = normalized3;
		}
	}

	public void MoveThread()
	{
		if (threads != null)
		{
			if (threadCycle == 5)
			{
				threads[threadIndex].stuckPosA = clawPos;
			}
			else if (threadCycle == 11)
			{
				threads[threadIndex].stuckPosB = clawPos;
			}
		}
	}

	public void CycleThread()
	{
		if (threads != null && threadCycle != 0 && threadCycle != 6 && cycleCounter.isFinished)
		{
			Vector2 b = targetPos;
			if (threadCycle == 1 || threadCycle == 3 || threadCycle == 7 || threadCycle == 9)
			{
				threadCycle++;
			}
			else if (threadCycle == 5 || threadCycle == 11)
			{
				targetPos = RandomClawPos();
				threadCycle++;
			}
			else if (threadCycle == 2)
			{
				threadIndex = (threadIndex + 1) % nThreads;
				targetPos = threads[threadIndex].stuckPosA.Value;
				threadCycle++;
			}
			else if (threadCycle == 8)
			{
				threadIndex = (threadIndex + 1) % nThreads;
				targetPos = threads[threadIndex].stuckPosB.Value;
				threadCycle++;
			}
			else if (threadCycle == 4 || threadCycle == 10)
			{
				targetPos = RandomClawPos(biggerArea: true);
				threadCycle++;
			}
			else if (threadCycle > 7)
			{
				threadCycle = 0;
			}
			cycleCounter.SetMax(Random.Range(20, 60) + (int)(Vector2.Distance(targetPos, b) * 0.5f));
			cycleCounter.Reset();
		}
	}

	public Vector2 RandomClawPos(bool biggerArea = false)
	{
		return basePos + baseDirection * (totalArmLength * 0.7f) + Random.insideUnitCircle * (totalArmLength * (0.3f + (biggerArea ? 0.4f : 0f)));
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		for (int i = 0; i < baseGooOffsets.Length; i++)
		{
			int num = i % 6;
			sLeaser.sprites[baseGoopSpriteStartIndex + i] = new FSprite("RootBall" + num);
			sLeaser.sprites[baseGoopSpriteStartIndex + i].scale = Mathf.Lerp(1f, 1.8f, Random.value);
			sLeaser.sprites[baseGoopSpriteStartIndex + i].anchorY = 0.7f;
		}
		float num2 = armLengths[0];
		TriangleMesh triangleMesh = TriangleMesh.MakeLongMeshAtlased(32, pointyTip: false, customColor: false);
		for (int j = 0; j < triangleMesh.vertices.Length; j += 2)
		{
			float num3 = (float)j / 2f / ((float)triangleMesh.vertices.Length / 2f - 1f);
			float num4 = (4f * num3 * num3 - 4f * num3) * -10f;
			float num5 = Mathf.Lerp(15f, 4f, num3);
			float num6 = Mathf.PingPong(num3, 0.5f) * 2f;
			num6 = Mathf.Clamp01(num6 * 5f);
			num6 = Mathf.Sqrt((0f - num6) * num6 + 2f * num6);
			num5 *= num6;
			float num7 = Mathf.PingPong(num3 * 15f, 1f) * Mathf.Lerp(1f, 0.2f, num3) * 10f;
			num7 += Mathf.Lerp(-0.5f, 0.5f, Mathf.PingPong(num3 * 8f, 1f)) * 10f;
			num7 *= Mathf.Lerp(1f, 0.2f, num3);
			float num8 = num3 * 20f;
			num8 = (num8 - Mathf.Floor(num8)) * Mathf.Lerp(1f, 0.5f, num3) * 3f;
			float num9 = Mathf.Abs(Mathf.PerlinNoise(num3 * 5f, 0f)) * Mathf.Lerp(1f, 0.25f, num3) * 15f;
			float num10 = num3 * 30f;
			num10 = (num10 - Mathf.Floor(num10)) * num3 * 6f;
			float y = Mathf.Lerp(0f - num5, num2 + num5, num3);
			Vector2 vec = new Vector2(num4 - num5 - num8 - num9, y);
			Vector2 vec2 = new Vector2(num4 + num5 + num7 + num10, y);
			float num11 = Mathf.Pow(1f - num3, 3.2f);
			vec = Custom.RotateAroundOrigo(vec, num11 * 90f);
			vec2 = Custom.RotateAroundOrigo(vec2, num11 * 90f);
			if (mirrored)
			{
				vec *= new Vector2(-1f, 1f);
				vec2 *= new Vector2(-1f, 1f);
			}
			triangleMesh.vertices[j] = vec;
			triangleMesh.vertices[j + 1] = vec2;
		}
		sLeaser.sprites[arm1SpriteStartIndex] = triangleMesh;
		TriangleMesh triangleMesh2 = TriangleMesh.MakeLongMeshAtlased(24, pointyTip: false, customColor: false);
		sLeaser.sprites[arm1SpriteStartIndex + 1] = triangleMesh2;
		float num12 = armLengths[1];
		Vector2 vector = Vector2.down;
		TriangleMesh triangleMesh3 = TriangleMesh.MakeLongMeshAtlased(16, pointyTip: false, customColor: false);
		for (int k = 0; k < triangleMesh3.vertices.Length; k += 2)
		{
			float num13 = (float)k / 2f / ((float)triangleMesh3.vertices.Length / 2f - 1f);
			float x = (-4f * num13 * num13 + 4f * num13) * -6.5f * (mirrored ? (-1f) : 1f);
			float num14 = 3f;
			num14 += Mathf.Abs(Mathf.Sin(num13 * 10f) * 2f);
			Vector2 vector2 = new Vector2(x, num13 * num12);
			Vector2 vector3 = Vector2.Perpendicular((vector2 - vector).normalized);
			triangleMesh3.vertices[k] = vector2 - vector3 * num14;
			triangleMesh3.vertices[k + 1] = vector2 + vector3 * num14;
			vector = vector2;
		}
		sLeaser.sprites[arm2SpriteStartIndex] = triangleMesh3;
		sLeaser.sprites[arm2SpriteStartIndex + 1] = new FSprite("Circle20");
		sLeaser.sprites[arm2SpriteStartIndex + 1].scale = 0.4f;
		sLeaser.sprites[arm2SpriteStartIndex + 2] = new FSprite("Circle20");
		sLeaser.sprites[arm2SpriteStartIndex + 2].scale = 0.4f;
		float num15 = armLengths[2];
		TriangleMesh triangleMesh4 = TriangleMesh.MakeLongMeshAtlased(32, pointyTip: false, customColor: false);
		for (int l = 0; l < triangleMesh4.vertices.Length; l += 2)
		{
			float num16 = (float)l / 2f / ((float)triangleMesh4.vertices.Length / 2f - 1f);
			float num17 = (4f * num16 * num16 - 4f * num16 + 1f) * 80f;
			num17 *= Mathf.PingPong(num16 * 0.8f, 0.5f) * 2f;
			float b = Mathf.Lerp(4f, 8f, num16);
			b = Mathf.Max(Mathf.Lerp(7f, 0f, num16), b);
			float num18 = Mathf.PingPong(num16, 0.5f) * 2f;
			num18 = Mathf.Clamp01(num18 * 5f);
			num18 = Mathf.Sqrt((0f - num18) * num18 + 2f * num18);
			b *= num18;
			float num19 = num16 * 13.5f;
			num19 -= Mathf.Floor(num19);
			num19 = Mathf.Pow(num19, 3f) * Mathf.Lerp(0.2f, 1f, num16) * 10f;
			float num20 = Mathf.PerlinNoise(num16 * 5f, 0f) * Mathf.Clamp01(Mathf.PingPong(num16, 0.5f) * 2f * 5f) * 10f;
			num17 += num20;
			float num21 = Mathf.Abs(Mathf.PerlinNoise(num16 * 30f, 30f)) * 3.5f;
			float y2 = Mathf.Lerp(0f - b, num15 + b, num16);
			Vector2 vec3 = new Vector2(num17 - b - num21, y2);
			Vector2 vec4 = new Vector2(num17 + b + num19, y2);
			vec3 = Custom.RotateAroundOrigo(vec3, -18f);
			vec4 = Custom.RotateAroundOrigo(vec4, -18f);
			if (mirrored)
			{
				vec3 *= new Vector2(-1f, 1f);
				vec4 *= new Vector2(-1f, 1f);
			}
			triangleMesh4.vertices[l] = vec3;
			triangleMesh4.vertices[l + 1] = vec4;
		}
		sLeaser.sprites[arm3SpriteStartIndex] = triangleMesh4;
		for (int m = 0; m < armGooOffsets.Length; m++)
		{
			int num22 = m % 6;
			sLeaser.sprites[armGooSpriteStartIndex + m] = new FSprite("RootBall" + num22);
			sLeaser.sprites[armGooSpriteStartIndex + m].scale = Mathf.Lerp(1f, 1.5f, Random.value);
			sLeaser.sprites[armGooSpriteStartIndex + m].anchorY = 0.7f;
		}
		sLeaser.sprites[clawSpriteStartIndex] = new FSprite("Cicada5body");
		sLeaser.sprites[clawSpriteStartIndex].scaleX = handScale;
		sLeaser.sprites[clawSpriteStartIndex].scaleY = (float)((!mirrored) ? 1 : (-1)) * handScale;
		sLeaser.sprites[clawSpriteStartIndex].anchorX = 0.1f;
		sLeaser.sprites[clawSpriteStartIndex + 1] = new FSprite("SpiderLeg1B");
		sLeaser.sprites[clawSpriteStartIndex + 1].anchorY = 0.05f;
		sLeaser.sprites[clawSpriteStartIndex + 1].scaleX = (float)((!mirrored) ? 1 : (-1)) * handScale;
		sLeaser.sprites[clawSpriteStartIndex + 1].scaleY = handScale;
		sLeaser.sprites[clawSpriteStartIndex + 2] = new FSprite("SpiderLeg2B");
		sLeaser.sprites[clawSpriteStartIndex + 2].anchorY = 0.05f;
		sLeaser.sprites[clawSpriteStartIndex + 2].scaleX = (float)((!mirrored) ? 1 : (-1)) * handScale;
		sLeaser.sprites[clawSpriteStartIndex + 2].scaleY = handScale;
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = basePos - camPos;
		float num = Mathf.LerpAngle(armAngles[0].last, armAngles[0].current, timeStacker);
		TriangleMesh triangleMesh = sLeaser.sprites[arm1SpriteStartIndex] as TriangleMesh;
		triangleMesh.SetPosition(vector);
		triangleMesh.rotation = num;
		TriangleMesh triangleMesh2 = sLeaser.sprites[arm1SpriteStartIndex + 1] as TriangleMesh;
		Vector2 vector2 = (mirrored ? Vector2.left : Vector2.right);
		for (int i = 0; i < triangleMesh2.vertices.Length; i += 2)
		{
			float num2 = (float)i / 2f / ((float)triangleMesh2.vertices.Length / 2f) * ((float)triangleMesh.vertices.Length / 2f);
			int num3 = Mathf.FloorToInt(num2) * 2;
			int num4 = Mathf.CeilToInt(num2) * 2;
			float t = num2 - (float)Mathf.FloorToInt(num2);
			Vector2 a = Vector2.Lerp(triangleMesh.vertices[num3], triangleMesh.vertices[num4], t);
			Vector2 b = Vector2.Lerp(triangleMesh.vertices[num3 + 1], triangleMesh.vertices[num4 + 1], t);
			float num5 = (float)i / 2f / ((float)triangleMesh2.vertices.Length / 2f - 1f);
			float num6 = Mathf.Sin(num5 * 40f - ((float)room.game.clock + timeStacker) * 0.03f) * (1f - num5);
			num6 = num6 * 0.5f + 0.5f;
			num6 = Mathf.Lerp(0.25f, 0.75f, num6);
			Vector2 vector3 = Vector2.Lerp(a, b, num6);
			Vector2 vector4 = Vector2.Perpendicular((vector3 - vector2).normalized);
			float num7 = 3f * (1f - num5);
			triangleMesh2.vertices[i] = vector3 - vector4 * num7;
			triangleMesh2.vertices[i + 1] = vector3 + vector4 * num7;
			vector2 = vector3;
		}
		triangleMesh2.SetPosition(vector);
		triangleMesh2.rotation = num;
		for (int j = 0; j < baseGooOffsets.Length; j++)
		{
			sLeaser.sprites[baseGoopSpriteStartIndex + j].SetPosition(baseGooOffsets[j] + basePos + baseDirection * -20f - camPos);
		}
		vector += Custom.DegToVec(num) * armLengths[0];
		num += Mathf.LerpAngle(armAngles[1].last, armAngles[1].current, timeStacker);
		sLeaser.sprites[arm2SpriteStartIndex].SetPosition(vector);
		sLeaser.sprites[arm2SpriteStartIndex].rotation = num;
		sLeaser.sprites[arm2SpriteStartIndex + 1].SetPosition(vector);
		vector += Custom.DegToVec(num) * armLengths[1];
		num += Mathf.LerpAngle(armAngles[2].last, armAngles[2].current, timeStacker);
		sLeaser.sprites[arm2SpriteStartIndex + 2].SetPosition(vector);
		sLeaser.sprites[arm3SpriteStartIndex].SetPosition(vector);
		sLeaser.sprites[arm3SpriteStartIndex].rotation = num;
		vector += Custom.DegToVec(num) * armLengths[2];
		num += 0f;
		for (int k = 0; k < armGooOffsets.Length; k++)
		{
			Vector2 vec = armGooOffsets[k];
			vec = Custom.RotateAroundOrigo(vec, num);
			sLeaser.sprites[armGooSpriteStartIndex + k].rotation = Custom.MapRangeClamped(vec.x, 0f - armGooRadius, armGooRadius, -5f, 5f);
			sLeaser.sprites[armGooSpriteStartIndex + k].SetPosition(vec + vector);
		}
		Vector2 vector5 = Custom.DegToVec(num);
		Vector2 vector6 = vector - vector5 * 7f;
		sLeaser.sprites[clawSpriteStartIndex].SetPosition(vector6);
		sLeaser.sprites[clawSpriteStartIndex].rotation = Custom.VecToDeg(vector5) - 90f;
		clawPos = vector6 + vector5 * 60f + camPos;
		Vector2 position = vector6 + vector5 * (41f * handScale) + Custom.PerpendicularVector(vector5) * ((float)(-((!mirrored) ? 1 : (-1))) * 6f * handScale);
		for (int l = 0; l < 2; l++)
		{
			sLeaser.sprites[clawSpriteStartIndex + 1 + l].SetPosition(position);
			sLeaser.sprites[clawSpriteStartIndex + 1 + l].rotation = Custom.VecToDeg(vector5);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < baseGooOffsets.Length; i++)
		{
			sLeaser.sprites[baseGoopSpriteStartIndex + i].color = daemonColor;
		}
		sLeaser.sprites[armRopeSpriteStartIndex].color = daemonColor;
		sLeaser.sprites[arm1SpriteStartIndex].color = daemonColor;
		sLeaser.sprites[arm1SpriteStartIndex + 1].color = daemonColor * 1.5f;
		sLeaser.sprites[arm1SpriteStartIndex + 2].color = daemonColor;
		sLeaser.sprites[arm2SpriteStartIndex].color = daemonColor;
		sLeaser.sprites[arm2SpriteStartIndex + 1].color = daemonColor;
		sLeaser.sprites[arm2SpriteStartIndex + 2].color = daemonColor;
		sLeaser.sprites[arm3SpriteStartIndex].color = daemonColor;
		for (int j = 0; j < armGooOffsets.Length; j++)
		{
			sLeaser.sprites[armGooSpriteStartIndex + j].color = daemonColor;
		}
		sLeaser.sprites[clawSpriteStartIndex].color = daemonColor;
		sLeaser.sprites[clawSpriteStartIndex + 1].color = daemonColor;
		sLeaser.sprites[clawSpriteStartIndex + 2].color = daemonColor;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
