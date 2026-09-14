using RWCustom;
using UnityEngine;

namespace Watcher;

public class WeaverThread : CosmeticSprite
{
	public class ThreadGraphic : RopeGraphic
	{
		public WeaverThread owner;

		public int firstSprite;

		public int totalSprites;

		public int sprites;

		public Rect segmentBounds;

		public bool lastVisible;

		public float thickness;

		public bool colDirty = true;

		public float randomOffset;

		public float _startAnimation = 1f;

		public float startAnimation
		{
			get
			{
				return _startAnimation;
			}
			set
			{
				_startAnimation = value;
				colDirty = true;
			}
		}

		public ThreadGraphic(WeaverThread owner, int segments, int firstSprite)
			: base(segments)
		{
			this.owner = owner;
			this.firstSprite = firstSprite;
			lastVisible = true;
			segmentBounds = default(Rect);
			switch (Random.Range(0, 3))
			{
			case 0:
				thickness = 1f;
				break;
			case 1:
				thickness = 1.5f;
				break;
			default:
				thickness = 2f;
				break;
			}
			randomOffset = Random.value;
		}

		public void UpdateSegmentBounds(bool init, Vector2 pos)
		{
			if (init)
			{
				segmentBounds.xMin = pos.x;
				segmentBounds.xMax = pos.x;
				segmentBounds.yMin = pos.y;
				segmentBounds.yMax = pos.y;
				return;
			}
			if (pos.x > segmentBounds.xMax)
			{
				segmentBounds.xMax = pos.x;
			}
			if (pos.y > segmentBounds.yMax)
			{
				segmentBounds.yMax = pos.y;
			}
			if (pos.x < segmentBounds.xMin)
			{
				segmentBounds.xMin = pos.x;
			}
			if (pos.y < segmentBounds.yMin)
			{
				segmentBounds.yMin = pos.y;
			}
		}

		public override void Update()
		{
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i].lastPos = segments[i].pos;
				segments[i].pos = owner.segments[i, 0];
				UpdateSegmentBounds(i == 0, segments[i].pos);
			}
		}

		public override void ConnectPhase(float totalRopeLength)
		{
		}

		public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
		{
			segments[segment].vel *= 0f;
			if (owner.room.GetTile(smoothedGoalPos).Solid && !owner.room.GetTile(goalPos).Solid)
			{
				FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.room.TileRect(owner.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
				segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
			}
			else
			{
				segments[segment].pos = smoothedGoalPos;
			}
			UpdateSegmentBounds(init: false, segments[segment].pos);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			TriangleMesh triangleMesh = TriangleMesh.MakeLongMeshAtlased(segments.Length, pointyTip: false, customColor: true);
			triangleMesh.shader = rCam.room.game.rainWorld.Shaders[owner.bothSides ? "WeaverThreadBoth" : "WeaverThreadRipple"];
			totalSprites = sLeaser.sprites.Length;
			SetUV(triangleMesh);
			SetCol(triangleMesh);
			sLeaser.sprites[firstSprite] = triangleMesh;
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			bool flag = rCam.room.ViewedByAnyCamera(segmentBounds, thickness);
			if (flag != lastVisible)
			{
				sLeaser.sprites[0].isVisible = flag;
				lastVisible = flag;
			}
			if (flag)
			{
				Vector2 vector = Vector2.Lerp(segments[0].lastPos, segments[0].pos, timeStacker);
				vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector) * 1f;
				SetCol(sLeaser.sprites[0] as TriangleMesh);
				for (int i = 0; i < segments.Length; i++)
				{
					Vector2 vector2 = Vector2.Lerp(segments[i].lastPos, segments[i].pos, timeStacker);
					Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
					(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * thickness - camPos);
					(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * thickness - camPos);
					(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * thickness - camPos);
					(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * thickness - camPos);
					vector = vector2;
				}
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void SetCol(TriangleMesh mesh)
		{
			if (colDirty)
			{
				for (int i = 0; i < mesh.verticeColors.Length; i++)
				{
					mesh.verticeColors[i] = new Color(startAnimation, randomOffset, 0f, 0f);
				}
				colDirty = false;
			}
		}

		public void SetUV(TriangleMesh mesh)
		{
			int num = segments.Length;
			for (int i = 0; i < num; i++)
			{
				float num2 = (float)i / (float)(num + 1);
				int num3 = i * 4;
				mesh.UVvertices[num3] = Vector2.right * num2;
				mesh.UVvertices[num3 + 1] = Vector2.right * num2 + Vector2.up;
				if (num3 + 2 < mesh.UVvertices.Length)
				{
					num2 = (float)(i + 1) / (float)(num + 1);
					mesh.UVvertices[num3 + 2] = Vector2.right * num2;
					mesh.UVvertices[num3 + 3] = Vector2.right * num2 + Vector2.up;
				}
			}
		}
	}

	public ThreadGraphic graphic;

	public float conRad = 25f;

	public float pushApart = 0.15f;

	public Vector2[,] segments;

	public Vector2? stuckPosA;

	public Vector2? stuckPosB;

	public bool bothSides;

	public bool isWarpPointThread;

	public WarpTear.TearBlocker blockerA;

	public WarpTear.TearBlocker blockerB;

	public int soundCooldown;

	public const int maxSoundDelay = 800;

	public const int minSoundDelay = 200;

	public bool instantSettle;

	public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public WeaverThread(Room room, float length, Vector2 spawnPosA, Vector2 spawnPosB, bool stuckAtA, bool stuckAtB, bool warpPointThread = false)
	{
		base.room = room;
		if (stuckAtA)
		{
			stuckPosA = spawnPosA;
		}
		if (stuckAtB)
		{
			stuckPosB = spawnPosB;
		}
		int num = Mathf.Max(2, (int)Mathf.Clamp(length / conRad, 1f, 200f)) + 1;
		segments = new Vector2[num, 3];
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			float t = (float)i / (float)(segments.GetLength(0) - 1);
			segments[i, 0] = Vector2.Lerp(spawnPosA, spawnPosB, t) + Custom.RNV() * Random.value;
			segments[i, 1] = segments[i, 0];
			segments[i, 2] = Custom.RNV() * Random.value;
		}
		graphic = new ThreadGraphic(this, num, 0);
		isWarpPointThread = warpPointThread;
		SpawnBlockers();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!isWarpPointThread)
		{
			if (soundCooldown > 0)
			{
				soundCooldown--;
			}
			else
			{
				soundCooldown = Random.Range(200, 800);
				float rippleDimension = room.game.cameras[0].virtualMicrophone.rippleDimension;
				if (rippleDimension > 0f)
				{
					Vector2 vector = segments[Random.Range(0, segments.GetLength(0)), 0];
					room.PlaySound(WatcherEnums.WatcherSoundID.Void_Weaver_Pluck_Thread, vector, rippleDimension, 1f);
				}
			}
		}
		UpdateBlockers();
		int num = 1;
		if (instantSettle && room.aimap != null)
		{
			instantSettle = false;
			num = 100;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 2; j < segments.GetLength(0); j++)
			{
				Vector2 vector2 = Custom.DirVec(segments[j - 2, 0], segments[j, 0]);
				segments[j - 2, 2] -= vector2 * pushApart;
				segments[j, 2] += vector2 * pushApart;
			}
			if (room.aimap != null)
			{
				for (int k = 0; k < segments.GetLength(0); k++)
				{
					segments[k, 2].y -= 0.9f * room.gravity * GravityAffected(k);
					segments[k, 1] = segments[k, 0];
					segments[k, 0] += segments[k, 2];
					segments[k, 2] *= 0.999f;
					if (room.gravity < 1f && room.readyForAI && room.aimap.getTerrainProximity(segments[k, 0]) < 4)
					{
						IntVector2 tilePosition = room.GetTilePosition(segments[k, 0]);
						Vector2 vector3 = new Vector2(0f, 0f);
						for (int l = 0; l < 4; l++)
						{
							if (!room.GetTile(tilePosition + Custom.fourDirections[l]).Solid && !room.aimap.getAItile(tilePosition + Custom.fourDirections[l]).narrowSpace)
							{
								float num2 = 0f;
								for (int m = 0; m < 4; m++)
								{
									num2 += (float)room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[l] + Custom.fourDirections[m]);
								}
								vector3 += Custom.fourDirections[l].ToVector2() * num2;
							}
						}
						segments[k, 2] += vector3.normalized * ((room.GetTile(segments[k, 0]).Solid ? 1f : Custom.LerpMap(room.aimap.getTerrainProximity(segments[k, 0]), 0f, 3f, 2f, 0.2f)) * (1f - room.gravity));
					}
					if (!isWarpPointThread && k > 2 && room.aimap.getTerrainProximity(segments[k, 0]) < 3)
					{
						SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(segments[k, 0], segments[k, 1], segments[k, 2], 2f, new IntVector2(0, 0), goThroughFloors: true);
						cd = SharedPhysics.VerticalCollision(room, cd);
						cd = SharedPhysics.HorizontalCollision(room, cd);
						segments[k, 0] = cd.pos;
						segments[k, 2] = cd.vel;
						if (cd.contactPoint.x != 0)
						{
							segments[k, 2].y *= 0.6f;
						}
						if (cd.contactPoint.y != 0)
						{
							segments[k, 2].x *= 0.6f;
						}
					}
				}
			}
			ConnectToWalls();
			for (int num3 = segments.GetLength(0) - 1; num3 > 0; num3--)
			{
				Connect(num3, num3 - 1);
			}
			ConnectToWalls();
			for (int n = 1; n < segments.GetLength(0); n++)
			{
				Connect(n, n - 1);
			}
			ConnectToWalls();
		}
		graphic.Update();
	}

	public void SpawnBlockers()
	{
		if (isWarpPointThread)
		{
			Vector2? vector = stuckPosA;
			if (vector.HasValue)
			{
				Vector2 valueOrDefault = vector.GetValueOrDefault();
				room.AddObject(blockerA = new WarpTear.TearBlocker(valueOrDefault, 4f, 0f));
			}
			vector = stuckPosB;
			if (vector.HasValue)
			{
				Vector2 valueOrDefault2 = vector.GetValueOrDefault();
				room.AddObject(blockerB = new WarpTear.TearBlocker(valueOrDefault2, 4f, 0f));
			}
		}
	}

	public void UpdateBlockers()
	{
		if (isWarpPointThread)
		{
			Vector2? vector = stuckPosA;
			if (vector.HasValue)
			{
				Vector2 valueOrDefault = vector.GetValueOrDefault();
				blockerA.pos = valueOrDefault;
			}
			vector = stuckPosB;
			if (vector.HasValue)
			{
				Vector2 valueOrDefault2 = vector.GetValueOrDefault();
				blockerB.pos = valueOrDefault2;
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		graphic.InitiateSprites(sLeaser, rCam);
		base.InitiateSprites(sLeaser, rCam);
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(isWarpPointThread ? "WarpPoint" : "GrabShaders"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		graphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		graphic.ApplyPalette(sLeaser, rCam, palette);
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public void ConnectToWalls()
	{
		if (stuckPosA.HasValue)
		{
			segments[0, 0] = stuckPosA.Value;
			segments[0, 2] *= 0f;
		}
		if (stuckPosB.HasValue)
		{
			segments[segments.GetLength(0) - 1, 0] = stuckPosB.Value;
			segments[segments.GetLength(0) - 1, 2] *= 0f;
		}
	}

	public void Connect(int A, int B)
	{
		Vector2 normalized = (segments[A, 0] - segments[B, 0]).normalized;
		float num = Vector2.Distance(segments[A, 0], segments[B, 0]);
		float num2 = Mathf.InverseLerp(0f, conRad, num);
		Vector2 vector = normalized * ((conRad - num) * 0.5f * num2);
		segments[A, 0] += vector;
		segments[A, 2] += vector;
		segments[B, 0] -= vector;
		segments[B, 2] -= vector;
	}

	public virtual float GravityAffected(int seg)
	{
		return 1f;
	}
}
