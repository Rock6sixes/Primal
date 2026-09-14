using System;
using RWCustom;
using Unity.Burst.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CoralBrain;

public class Mycelium
{
	public struct MyceliaConnection : IEquatable<MyceliaConnection>
	{
		public Mycelium A;

		public Mycelium B;

		public bool Equals(MyceliaConnection other)
		{
			if (object.Equals(A, other.A))
			{
				return object.Equals(B, other.B);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is MyceliaConnection other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((A != null) ? A.GetHashCode() : 0) * 397) ^ ((B != null) ? B.GetHashCode() : 0);
		}

		public static bool operator ==(MyceliaConnection left, MyceliaConnection right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MyceliaConnection left, MyceliaConnection right)
		{
			return !left.Equals(right);
		}

		public MyceliaConnection(Mycelium A, Mycelium B)
		{
			this.A = A;
			this.B = B;
		}

		public Mycelium Other(Mycelium me)
		{
			if (me == A)
			{
				return B;
			}
			return A;
		}
	}

	public Vector2[,] points;

	public float conRad;

	public Color color;

	public CoralNeuronSystem system;

	public IOwnMycelia owner;

	public int index;

	public float length;

	public MyceliaConnection connection;

	public int lastCameraCullTick;

	public bool viewedByCamera;

	public bool useStaticCulling;

	public bool culled;

	public bool lastCulled;

	public bool moveAwayFromWalls = true;

	public int rest;

	public JobData _jobDataStep1;

	public JobHandle handleJob01;

	public Vector2 Tip => points[points.GetLength(0) - 1, 0];

	public Vector2 Base => points[0, 0];

	public Mycelium(CoralNeuronSystem system, IOwnMycelia owner, int index, float length, Vector2 initPoint)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		lastCameraCullTick = -1;
		useStaticCulling = true;
		this.system = system;
		system?.mycelia.Add(this);
		this.owner = owner;
		this.index = index;
		this.length = length;
		float num = Mathf.Max(length, Mathf.Lerp(length, 40f, 0.5f));
		points = new Vector2[Custom.IntClamp((int)(num / 15f), 2, 20), 3];
		conRad = length / (float)points.GetLength(0);
		Reset(initPoint);
		_jobDataStep1 = default(JobData);
		_jobDataStep1.naFourDirections = (int2[])(object)new int2[4];
		_jobDataStep1.naFourDirections[0] = new int2(-1, 0);
		_jobDataStep1.naFourDirections[1] = new int2(0, -1);
		_jobDataStep1.naFourDirections[2] = new int2(1, 0);
		_jobDataStep1.naFourDirections[3] = new int2(0, 1);
		_jobDataStep1.nativePoints = (float2[])(object)new float2[points.GetLength(0) * 3];
		_jobDataStep1.outPoints = new Vector2[points.GetLength(0) * 3];
	}

	public void ConnectSystem(CoralNeuronSystem newSystem)
	{
		system = newSystem;
		newSystem?.mycelia.Add(this);
	}

	public void Reset(Vector2 resetPos)
	{
		Vector2 vector = resetPos + length * 0.6f * owner.ResetDir(index);
		Vector2 cA = resetPos + (Custom.DirVec(resetPos, vector) + Custom.RNV()).normalized * (Vector2.Distance(resetPos, vector) * 0.5f);
		Vector2 cB = vector + (Custom.DirVec(vector, resetPos) + Custom.RNV()).normalized * (Vector2.Distance(resetPos, vector) * 0.5f);
		for (int i = 0; i < points.GetLength(0); i++)
		{
			points[i, 0] = Custom.Bezier(resetPos, cA, vector, cB, (float)i / (float)(points.GetLength(0) - 1)) + Custom.RNV();
			points[i, 1] = points[i, 0];
			points[i, 2] = Custom.RNV();
		}
	}

	public void Update()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		if (useStaticCulling)
		{
			culled = (system != null && system.Frozen) || !viewedByCamera;
		}
		else
		{
			culled = system != null && system.Frozen;
		}
		if (lastCameraCullTick != owner.OwnerRoom.camerasChangedTick)
		{
			viewedByCamera = owner.OwnerRoom.ViewedByAnyCamera(Base, length + 50f);
			lastCameraCullTick = owner.OwnerRoom.camerasChangedTick;
		}
		if (lastCulled && !culled)
		{
			Reset(owner.ConnectionPos(index, 1f));
		}
		lastCulled = culled;
		if (culled || owner.OwnerRoom.aimap == null)
		{
			return;
		}
		int num = points.GetLength(0);
		Indexer val = default(Indexer);
		val.width = 3;
		Indexer val2 = val;
		float2 val3 = default(float2);
		float2 val4 = default(float2);
		for (int i = 0; i < num && ((Indexer)(ref val2)).ind(i, 2) < points.Length; i++)
		{
			((float2)(ref val3))._002Ector(points[i, 0].x, points[i, 0].y);
			((float2)(ref val4))._002Ector(points[i, 2].x, points[i, 2].y);
			_jobDataStep1.nativePoints[((Indexer)(ref val2)).ind(i, 0)] = val3 + val4;
			_jobDataStep1.nativePoints[((Indexer)(ref val2)).ind(i, 1)] = val3;
			_jobDataStep1.nativePoints[((Indexer)(ref val2)).ind(i, 2)] = val4 * 0.999f;
		}
		Phase1(num);
		Phase4();
		if (owner != null)
		{
			points[0, 0] = owner.ConnectionPos(index, 1f);
			points[0, 2] *= 0f;
		}
		if (rest > 0)
		{
			rest--;
		}
		if (connection != default(MyceliaConnection))
		{
			if (connection.Other(this).connection != connection || !Custom.DistLess(connection.A.Base, connection.B.Base, connection.A.length + connection.B.length) || UnityEngine.Random.value < 0.005f)
			{
				connection = default(MyceliaConnection);
				rest = UnityEngine.Random.Range(20, 200);
				return;
			}
			Mycelium mycelium = connection.Other(this);
			if (Custom.DistLess(mycelium.Tip, Tip, 10f))
			{
				Vector2 vector = Custom.DirVec(Tip, mycelium.Tip);
				float num2 = Vector2.Distance(Tip, mycelium.Tip);
				Vector2 vector2 = vector * ((num2 - 1f) * 0.5f);
				points[points.GetLength(0) - 1, 0] += vector2;
				points[points.GetLength(0) - 1, 2] += vector2;
				mycelium.points[mycelium.points.GetLength(0) - 1, 0] -= vector2;
				mycelium.points[mycelium.points.GetLength(0) - 1, 2] -= vector2;
				if (UnityEngine.Random.value < 0.05f)
				{
					owner.OwnerRoom.AddObject(new NeuronSpark((mycelium.Tip + Tip) / 2f));
				}
			}
			else
			{
				points[points.GetLength(0) - 1, 2] = Vector2.Lerp(points[points.GetLength(0) - 1, 2], Vector2.ClampMagnitude(mycelium.Tip - Tip, 5f), 0.5f);
			}
		}
		else if (system != null && rest < 1 && system.mycelia.Count > 0)
		{
			Mycelium mycelium2 = system.mycelia[UnityEngine.Random.Range(0, system.mycelia.Count)];
			if (mycelium2 != this && mycelium2.owner != owner && mycelium2.connection == default(MyceliaConnection) && Custom.DistLess(Base, mycelium2.Base, (length + mycelium2.length) * 0.75f))
			{
				connection = new MyceliaConnection(this, mycelium2);
				mycelium2.connection = connection;
			}
		}
	}

	public void Phase4()
	{
		ExtraExtentions.CopyToAlt(_jobDataStep1.outPoints, points);
	}

	public void Phase1(int pointsDim0Length)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		int width = 3;
		Indexer val = default(Indexer);
		val.width = width;
		Indexer val2 = val;
		float2[] nativePoints = _jobDataStep1.nativePoints;
		float2 val5 = default(float2);
		float2 val7 = default(float2);
		for (int i = 0; i < pointsDim0Length; i++)
		{
			if (Hint.Likely(i > 0))
			{
				float2 val3 = new float2(nativePoints[((Indexer)(ref val2)).ind(i, 0)].x - nativePoints[((Indexer)(ref val2)).ind(i - 1, 0)].x, nativePoints[((Indexer)(ref val2)).ind(i, 0)].y - nativePoints[((Indexer)(ref val2)).ind(i - 1, 0)].y);
				float num = ExtraExtentions.magnitude(val3);
				float2 val4 = ExtraExtentions.normalized(val3);
				((float2)(ref val5))._002Ector(val4.x * (conRad - num) * 0.5f, val4.y * (conRad - num) * 0.5f);
				nativePoints[((Indexer)(ref val2)).ind(i, 0)] = new float2(nativePoints[((Indexer)(ref val2)).ind(i, 0)].x + val5.x, nativePoints[((Indexer)(ref val2)).ind(i, 0)].y + val5.y);
				nativePoints[((Indexer)(ref val2)).ind(i, 2)] = new float2(nativePoints[((Indexer)(ref val2)).ind(i, 2)].x + val5.x, nativePoints[((Indexer)(ref val2)).ind(i, 2)].y + val5.y);
				nativePoints[((Indexer)(ref val2)).ind(i - 1, 0)] = new float2(nativePoints[((Indexer)(ref val2)).ind(i - 1, 0)].x - val5.x, nativePoints[((Indexer)(ref val2)).ind(i - 1, 0)].y - val5.y);
				nativePoints[((Indexer)(ref val2)).ind(i - 1, 2)] = new float2(nativePoints[((Indexer)(ref val2)).ind(i - 1, 2)].x - val5.x, nativePoints[((Indexer)(ref val2)).ind(i - 1, 2)].y - val5.y);
				if (Hint.Likely(i > 1))
				{
					val4 = ExtraExtentions.normalized(new float2(nativePoints[((Indexer)(ref val2)).ind(i, 0)].x - nativePoints[((Indexer)(ref val2)).ind(i - 2, 0)].x, nativePoints[((Indexer)(ref val2)).ind(i, 0)].y - nativePoints[((Indexer)(ref val2)).ind(i - 2, 0)].y));
					nativePoints[((Indexer)(ref val2)).ind(i, 2)] = new float2(nativePoints[((Indexer)(ref val2)).ind(i, 2)].x + val4.x * 0.2f, nativePoints[((Indexer)(ref val2)).ind(i, 2)].y + val4.y * 0.2f);
					nativePoints[((Indexer)(ref val2)).ind(i - 2, 2)] = new float2(nativePoints[((Indexer)(ref val2)).ind(i - 2, 2)].x - val4.x * 0.2f, nativePoints[((Indexer)(ref val2)).ind(i - 2, 2)].y - val4.y * 0.2f);
				}
			}
			int2 val6 = Room.StaticGetTilePosition(nativePoints[((Indexer)(ref val2)).ind(i, 0)]);
			int num2 = int.MaxValue;
			if (owner.OwnerRoom.aimap.height != -1 && val6.x >= 0 && val6.y >= 0 && val6.x < owner.OwnerRoom.aimap.width && val6.y < owner.OwnerRoom.aimap.height)
			{
				num2 = owner.OwnerRoom.aimap.terrainProximity[ExtraExtentions.ind(val6.x, val6.y, owner.OwnerRoom.aimap.height)];
			}
			if (moveAwayFromWalls && num2 < 4)
			{
				((float2)(ref val7))._002Ector(0f, 0f);
				for (int j = 0; j < 4; j++)
				{
					int num3 = 0;
					for (int k = 0; k < 4; k++)
					{
						int2 val8 = val6 + _jobDataStep1.naFourDirections[j] + _jobDataStep1.naFourDirections[k];
						if (val8.x >= 0 && val8.y >= 0 && val8.x < owner.OwnerRoom.aimap.width && val8.y < owner.OwnerRoom.aimap.height)
						{
							num3 += owner.OwnerRoom.aimap.terrainProximity[ExtraExtentions.ind(val8.x, val8.y, owner.OwnerRoom.aimap.height)];
						}
					}
					((float2)(ref val7))._002Ector(val7.x + (float)(_jobDataStep1.naFourDirections[j].x * num3), val7.y + (float)(_jobDataStep1.naFourDirections[j].y * num3));
				}
				float num4 = Custom.LerpMap(num2, 0f, 3f, 2f, 0.2f);
				float2 val9 = ExtraExtentions.normalized(val7);
				nativePoints[((Indexer)(ref val2)).ind(i, 2)] = new float2(nativePoints[((Indexer)(ref val2)).ind(i, 2)].x + val9.x * num4, nativePoints[((Indexer)(ref val2)).ind(i, 2)].y + val9.y * num4);
			}
			_jobDataStep1.outPoints[((Indexer)(ref val2)).ind(i, 1)] = new Vector2(nativePoints[((Indexer)(ref val2)).ind(i, 1)].x, nativePoints[((Indexer)(ref val2)).ind(i, 1)].y);
			if (Hint.Likely(i > 1))
			{
				_jobDataStep1.outPoints[((Indexer)(ref val2)).ind(i - 2, 0)] = new Vector2(nativePoints[((Indexer)(ref val2)).ind(i - 2, 0)].x, nativePoints[((Indexer)(ref val2)).ind(i - 2, 0)].y);
				_jobDataStep1.outPoints[((Indexer)(ref val2)).ind(i - 2, 2)] = new Vector2(nativePoints[((Indexer)(ref val2)).ind(i - 2, 2)].x, nativePoints[((Indexer)(ref val2)).ind(i - 2, 2)].y);
			}
		}
		for (int l = Math.Max(0, pointsDim0Length - 3); l < pointsDim0Length; l++)
		{
			_jobDataStep1.outPoints[((Indexer)(ref val2)).ind(l, 0)] = new Vector2(nativePoints[((Indexer)(ref val2)).ind(l, 0)].x, nativePoints[((Indexer)(ref val2)).ind(l, 0)].y);
			_jobDataStep1.outPoints[((Indexer)(ref val2)).ind(l, 2)] = new Vector2(nativePoints[((Indexer)(ref val2)).ind(l, 2)].x, nativePoints[((Indexer)(ref val2)).ind(l, 2)].y);
		}
	}

	public void InitiateSprites(int spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[spr] = TriangleMesh.MakeLongMesh(points.GetLength(0), pointyTip: false, customColor: true);
		UpdateColor(color, 0f, spr, sLeaser);
	}

	public void DrawSprites(int spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[spr].isVisible = !culled;
		if (!culled)
		{
			Vector2 vector = Vector2.Lerp(points[0, 1], points[0, 0], timeStacker);
			if (owner != null)
			{
				vector = owner.ConnectionPos(index, timeStacker);
			}
			float num = 0.5f;
			for (int i = 0; i < points.GetLength(0); i++)
			{
				Vector2 vector2 = Vector2.Lerp(points[i, 1], points[i, 0], timeStacker);
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num2 = Vector2.Distance(vector, vector2) / 5f;
				(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num2 - vector3 * num - camPos);
				(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num2 + vector3 * num - camPos);
				(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num2 - vector3 * num - camPos);
				(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num2 + vector3 * num - camPos);
				vector = vector2;
			}
		}
	}

	public void UpdateColor(Color newColor, float gradientStart, int spr, RoomCamera.SpriteLeaser sLeaser)
	{
		color = newColor;
		for (int i = 0; i < (sLeaser.sprites[spr] as TriangleMesh).verticeColors.Length; i++)
		{
			float value = (float)i / (float)((sLeaser.sprites[spr] as TriangleMesh).verticeColors.Length - 1);
			(sLeaser.sprites[spr] as TriangleMesh).verticeColors[i] = Color.Lerp(color, Custom.HSL2RGB(22f / 45f, 0.5f, 0.2f), Mathf.InverseLerp(gradientStart, 1f, value));
		}
		for (int j = 1; j < 3; j++)
		{
			(sLeaser.sprites[spr] as TriangleMesh).verticeColors[(sLeaser.sprites[spr] as TriangleMesh).verticeColors.Length - j] = new Color(0f, 0f, 1f);
		}
	}
}
