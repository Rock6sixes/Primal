using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;
using Watcher;

public class LobeTree : PhysicalObject, IDrawable
{
	public class LobeTreeData : PlacedObject.ResizableObjectData
	{
		public Vector2 rootOffset;

		public LobeTreeData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			rootOffset.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			rootOffset.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}", rootOffset.x, rootOffset.y);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class AbstractLobeTree : AbstractPhysicalObject
	{
		public int placedObjectIndex;

		public PlacedObject useData;

		public AbstractLobeTree(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int placedObjectIndex)
			: base(world, type, realizedObject, pos, ID)
		{
			this.placedObjectIndex = placedObjectIndex;
		}

		public AbstractLobeTree(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, PlacedObject data)
			: base(world, type, realizedObject, pos, ID)
		{
			useData = data;
			placedObjectIndex = -1;
		}
	}

	public class LobeTreeRope : CosmeticSprite
	{
		public SimpleSegment[] segments;

		public BodyChunk chunk;

		public Vector2 relativeStart;

		public Vector2? end;

		public float idealLength;

		public int dampingTimer;

		public Color? overrideColor;

		public float thickness = 1f;

		public LobeTreeRope(BodyChunk chunk, Vector2 relativeStart, float length, Vector2 end)
			: this(chunk, relativeStart, length)
		{
			this.end = end;
			dampingTimer = 80;
		}

		public LobeTreeRope(BodyChunk chunk, Vector2 relativeStart, float length)
		{
			this.chunk = chunk;
			this.relativeStart = relativeStart;
			idealLength = length;
			end = null;
			segments = new SimpleSegment[Mathf.Max(2, (int)(idealLength / 10f) + 1)];
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i] = default(SimpleSegment);
			}
			Reset();
		}

		public void Reset()
		{
			for (int i = 0; i < segments.Length; i++)
			{
				float t = (float)i / (float)(segments.Length - 1);
				segments[i].vel = Vector2.zero;
				segments[i].pos = Vector2.Lerp(((chunk == null) ? Vector2.zero : chunk.pos) + relativeStart, end ?? ((chunk == null) ? Vector2.zero : chunk.pos), t);
				segments[i].lastPos = segments[i].pos;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (dampingTimer > 0)
			{
				dampingTimer--;
				for (int i = 0; i < segments.Length; i++)
				{
					segments[i].vel *= 0.7f;
				}
			}
			for (int j = 0; j < segments.Length; j++)
			{
				segments[j].vel.y -= 0.9f * room.gravity;
				segments[j].lastPos = segments[j].pos;
				segments[j].pos += segments[j].vel;
				segments[j].vel *= 0.999f;
				if (j > 2 && (room.aimap == null || room.aimap.getTerrainProximity(segments[j].pos) < 3))
				{
					SharedPhysics.TerrainCollisionData cd2 = SharedPhysics.VerticalCollision(cd: new SharedPhysics.TerrainCollisionData(segments[j].pos, segments[j].lastPos, segments[j].vel, 2f, new IntVector2(0, 0), goThroughFloors: true), room: room);
					cd2 = SharedPhysics.HorizontalCollision(room, cd2);
					segments[j].pos = cd2.pos;
					segments[j].vel = cd2.vel;
					if (cd2.contactPoint.x != 0)
					{
						segments[j].vel.y *= 0.6f;
					}
					if (cd2.contactPoint.y != 0)
					{
						segments[j].vel.x *= 0.6f;
					}
				}
			}
			ConnectEnds();
			for (int num = segments.Length - 1; num > 0; num--)
			{
				ConnectSegments(num, num - 1);
			}
			ConnectEnds();
			for (int k = 1; k < segments.Length; k++)
			{
				ConnectSegments(k, k - 1);
			}
			ConnectEnds();
		}

		public void ConnectEnds()
		{
			segments[0].pos = ((chunk == null) ? Vector2.zero : chunk.pos) + relativeStart;
			if (chunk != null)
			{
				segments[0].vel = chunk.vel;
			}
			if (end.HasValue)
			{
				segments[segments.Length - 1].pos = end.Value;
				segments[segments.Length - 1].vel = Vector2.zero;
			}
		}

		public void ConnectSegments(int A, int B)
		{
			float num = idealLength / (float)(segments.Length - 1) * 0.5f;
			Vector2 normalized = (segments[A].pos - segments[B].pos).normalized;
			float num2 = Vector2.Distance(segments[A].pos, segments[B].pos);
			float num3 = Mathf.InverseLerp(0f, num, num2);
			Vector2 vector = normalized * ((num - num2) * 0.5f * num3);
			segments[A].pos += vector;
			segments[A].vel += vector;
			segments[B].pos -= vector;
			segments[B].vel -= vector;
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			if (overrideColor.HasValue)
			{
				sLeaser.sprites[0].color = overrideColor.Value;
			}
			else
			{
				sLeaser.sprites[0].color = palette.texture.GetPixel(11, 1);
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: false);
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
			float num = thickness;
			Vector2 vector = segments[0].DrawPos(timeStacker);
			for (int i = 0; i < segments.Length; i++)
			{
				SimpleSegment simpleSegment = segments[i];
				Vector2 vector2 = simpleSegment.DrawPos(i);
				Vector2 vector3 = Custom.PerpendicularVector(Custom.DirVec(vector, vector2));
				triangleMesh.MoveVertice(i * 4, vector - vector3 * num - camPos);
				triangleMesh.MoveVertice(i * 4 + 1, vector + vector3 * num - camPos);
				triangleMesh.MoveVertice(i * 4 + 2, vector2 - vector3 * num - camPos);
				triangleMesh.MoveVertice(i * 4 + 3, vector2 + vector3 * num - camPos);
				vector = vector2;
			}
		}
	}

	public Vector2 center;

	public Vector2 trunkStart;

	public Vector2 trunkStartControl;

	public Vector2 trunkEnd;

	public Vector2 trunkEndControl;

	public Vector2[] chunkTargets;

	public Color[] chunkColors;

	public Vector2 centerOffset;

	public Vector2 lastCenterOffset;

	public Vector2 recoil;

	public int totalSprites;

	public int firstTrunkSprite;

	public int firstChunkSprite;

	public Color dripColor;

	public AbstractLobeTree AbstractTree => abstractPhysicalObject as AbstractLobeTree;

	public float MaxRecoil => Mathf.Min(30f, Vector2.Distance(trunkStart, trunkEnd) * 0.25f);

	public Vector2 CenterOfMass
	{
		get
		{
			Vector2 zero = Vector2.zero;
			BodyChunk[] array = base.bodyChunks;
			foreach (BodyChunk bodyChunk in array)
			{
				zero += bodyChunk.pos * bodyChunk.mass;
			}
			return zero / base.TotalMass;
		}
	}

	public LobeTree(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[15];
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i] = new BodyChunk(this, i, default(Vector2), 30f, 0.4f);
		}
		bodyChunkConnections = new BodyChunkConnection[0];
		chunkTargets = new Vector2[base.bodyChunks.Length];
		chunkColors = new Color[base.bodyChunks.Length];
		base.airFriction = 0.9f;
		base.gravity = 0f;
		bounce = 0.2f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.1f;
		windAffectiveness = 0.25f;
		totalSprites = 0;
		firstTrunkSprite = totalSprites;
		totalSprites++;
		firstChunkSprite = totalSprites;
		totalSprites += base.bodyChunks.Length;
		collisionRange = 150f;
		if (AbstractTree.placedObjectIndex == -1)
		{
			for (int j = 0; j < base.bodyChunks.Length; j++)
			{
				base.bodyChunks[j].collideWithObjects = false;
				base.bodyChunks[j].collideWithTerrain = false;
				base.bodyChunks[j].collideWithSlopes = false;
			}
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (AbstractTree.placedObjectIndex >= 0 && AbstractTree.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			Generate(placeRoom.roomSettings.placedObjects[AbstractTree.placedObjectIndex]);
			return;
		}
		if (AbstractTree.useData != null)
		{
			Generate(AbstractTree.useData);
			return;
		}
		center = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
		trunkStart = SharedPhysics.ExactTerrainRayTracePos(placeRoom, center, center + Vector2.down * 700f) ?? (center + Vector2.down * 250f);
		trunkStartControl = trunkStart;
		trunkEnd = center;
		trunkEndControl = center;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			chunkTargets[i] = center;
			base.bodyChunks[i].pos = center;
		}
	}

	public void Generate(PlacedObject pObj)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((int)(pObj.pos.x + pObj.pos.y));
		LobeTreeData lobeTreeData = pObj.data as LobeTreeData;
		center = pObj.pos;
		GetTrunkBezier(room, pObj, out trunkStart, out trunkStartControl, out trunkEnd, out trunkEndControl);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = center + UnityEngine.Random.insideUnitCircle * lobeTreeData.Rad;
			base.bodyChunks[i].rad = UnityEngine.Random.Range(0.3f, 0.5f) * lobeTreeData.Rad;
			base.bodyChunks[i].mass = base.bodyChunks[i].rad / 10f;
		}
		Vector2 vector = Custom.PerpendicularVector(lobeTreeData.handlePos);
		for (int j = 0; j < 100; j++)
		{
			BodyChunk[] array = base.bodyChunks;
			foreach (BodyChunk bodyChunk in array)
			{
				bodyChunk.pos = Vector2.Lerp(bodyChunk.pos, center, 0.15f);
				bodyChunk.pos -= vector * (Vector2.Dot(bodyChunk.pos - center, vector) * 0.4f);
				BodyChunk[] array2 = base.bodyChunks;
				foreach (BodyChunk bodyChunk2 in array2)
				{
					if (bodyChunk != bodyChunk2)
					{
						Vector2 vector2 = bodyChunk2.pos - bodyChunk.pos;
						float magnitude = vector2.magnitude;
						Vector2 vector3 = vector2 / magnitude;
						float num = (bodyChunk.rad + bodyChunk2.rad) / 2f;
						Vector2 vector4 = vector3 * ((magnitude - num) * ((magnitude < num) ? 0.15f : 0.01f));
						bodyChunk.pos += vector4;
						bodyChunk2.pos -= vector4;
					}
				}
			}
		}
		Array.Sort(base.bodyChunks, (BodyChunk a, BodyChunk b) => Vector2.Distance(b.pos, center).CompareTo(Vector2.Distance(a.pos, center)));
		for (int m = 0; m < base.bodyChunks.Length; m++)
		{
			chunkTargets[m] = base.bodyChunks[m].pos;
		}
		Vector2 normalized = (center - trunkEnd).normalized;
		Vector2 rhs = Custom.PerpendicularVector(normalized);
		float num2 = Vector2.Distance(center, trunkEnd);
		for (int n = 0; n < base.bodyChunks.Length; n++)
		{
			Vector2 lhs = chunkTargets[n] - center;
			float num3 = Mathf.Sqrt(Mathf.Pow(Vector2.Dot(lhs, normalized) * 0.3f, 2f) + Mathf.Pow(Vector2.Dot(lhs, rhs), 2f));
			chunkColors[n] = new Color(Mathf.Clamp01((num3 + base.bodyChunks[n].rad) / num2), base.bodyChunks[n].rad / 50f, 0f, 1f);
		}
		collisionRange = lobeTreeData.Rad * 1.25f + 20f;
		for (int num4 = 0; num4 < 10; num4++)
		{
			BodyChunk bodyChunk3 = base.bodyChunks[UnityEngine.Random.Range(0, base.bodyChunks.Length)];
			Vector2 vector5 = bodyChunk3.pos + UnityEngine.Random.insideUnitCircle * (base.bodyChunks[num4].rad * 0.8f);
			Vector2 vector6 = Custom.RotateAroundOrigo((trunkStart - vector5).normalized, UnityEngine.Random.Range(-45f, 45f));
			Vector2? vector7 = SharedPhysics.ExactTerrainRayTracePos(room, vector5, vector6 * 500f + vector5);
			if (vector7.HasValue)
			{
				LobeTreeRope lobeTreeRope = ((!(UnityEngine.Random.value < 0.2f)) ? new LobeTreeRope(bodyChunk3, vector5 - bodyChunk3.pos, Vector2.Distance(vector5, vector7.Value) * UnityEngine.Random.Range(0.75f, 1.25f), vector7.Value) : new LobeTreeRope(bodyChunk3, vector5 - bodyChunk3.pos, Vector2.Distance(vector5, vector7.Value) * UnityEngine.Random.Range(0.25f, 0.75f)));
				lobeTreeRope.thickness = Custom.ClampedRandomVariation(0.5f, 0.25f, 2f) * 2f;
				room.AddObject(lobeTreeRope);
			}
		}
		UnityEngine.Random.state = state;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastCenterOffset = centerOffset;
		centerOffset = Vector2.Lerp(centerOffset, recoil, 0.05f);
		recoil = Vector2.MoveTowards(recoil, Vector2.zero, 0.05f);
		float totalMass = base.TotalMass;
		float num = base.firstChunk.pos.x;
		float num2 = base.firstChunk.pos.x;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			BodyChunk bodyChunk = base.bodyChunks[i];
			Vector2 vector = chunkTargets[i] + lastCenterOffset - bodyChunk.pos;
			bodyChunk.pos += vector * 0.15f;
			bodyChunk.vel += vector * 0.15f;
			bodyChunk.vel *= 0.9f;
			centerOffset -= vector * (0.35f * bodyChunk.mass) / totalMass;
			num = Mathf.Min(num, bodyChunk.pos.x - bodyChunk.rad + 2f);
			num2 = Mathf.Max(num2, bodyChunk.pos.x + bodyChunk.rad - 2f);
		}
		if (!(UnityEngine.Random.value < 1f - Mathf.Pow(0.9995f, Mathf.Abs(num - num2))))
		{
			return;
		}
		BodyChunk bodyChunk2 = null;
		float num3 = UnityEngine.Random.Range(num, num2);
		float num4 = float.PositiveInfinity;
		BodyChunk[] array = base.bodyChunks;
		foreach (BodyChunk bodyChunk3 in array)
		{
			float num5 = bodyChunk3.pos.x - num3;
			float num6 = bodyChunk3.pos.y - Mathf.Sqrt(bodyChunk3.rad * bodyChunk3.rad - num5 * num5);
			if (num6 < num4)
			{
				bodyChunk2 = bodyChunk3;
				num4 = num6;
			}
		}
		if (!float.IsInfinity(num4) && !float.IsNaN(num4) && bodyChunk2 != null)
		{
			room.AddObject(new GooDrip(bodyChunk2, new Vector2(num3, num4 + 2f), UnityEngine.Random.Range(20f, 80f))
			{
				color = dripColor
			});
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (otherObject is Creature creature)
		{
			recoil = Vector2.Lerp(recoil, Custom.DirVec(creature.DangerPos, CenterOfMass) * MaxRecoil, 0.2f);
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		recoil = Vector2.Lerp(recoil, Custom.DirVec(weapon.firstChunk.pos, CenterOfMass) * MaxRecoil, 0.6f);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (!Futile.atlasManager.DoesContainAtlas("brainmatter"))
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "brainmatter.png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: false, crispPixels: false);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture("brainmatter", texture2D, textureFromAsset: false);
		}
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[firstTrunkSprite] = TriangleMesh.MakeLongMesh(20, pointyTip: false, customColor: false);
		sLeaser.sprites[firstTrunkSprite].shader = rCam.game.rainWorld.Shaders["BrainStem"];
		sLeaser.sprites[firstTrunkSprite].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, 0f, 1f);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			sLeaser.sprites[firstChunkSprite + i] = new FSprite("brainmatter")
			{
				shader = rCam.game.rainWorld.Shaders["BrainBall"],
				rotation = UnityEngine.Random.value * 360f
			};
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		dripColor = palette.texture.GetPixel(7, 1);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[firstChunkSprite + i]);
		}
		rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[firstTrunkSprite]);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			FSprite fSprite = sLeaser.sprites[firstChunkSprite + i];
			fSprite.SetPosition(Vector2.Lerp(base.bodyChunks[i].lastPos, base.bodyChunks[i].pos, timeStacker) - camPos);
			fSprite.scale = base.bodyChunks[i].rad / fSprite.element.sourceSize.x * 2.25f;
			fSprite.color = chunkColors[i];
		}
		TriangleMesh triangleMesh = sLeaser.sprites[firstTrunkSprite] as TriangleMesh;
		float a = 20f;
		float num = 12f;
		Vector2 vector = Vector2.Lerp(lastCenterOffset, centerOffset, timeStacker);
		for (int j = 0; j < triangleMesh.vertices.Length; j += 2)
		{
			float num2 = (float)j / (float)(triangleMesh.vertices.Length - 2) * 1.5f;
			Vector2 vector2;
			Vector2 vector3;
			Vector2 vector4;
			float num3;
			if (num2 < 1f)
			{
				vector2 = Custom.Bezier(trunkStart, trunkStartControl, trunkEnd + vector, trunkEndControl + vector, num2);
				vector3 = Custom.BezierDerivative(trunkStart, trunkStartControl, trunkEnd + vector, trunkEndControl + vector, num2).normalized;
				vector4 = Custom.PerpendicularVector(vector3);
				num3 = Mathf.Lerp(a, num, Mathf.Pow(num2, 0.5f));
			}
			else
			{
				num2 = (num2 - 1f) / 0.5f;
				vector2 = Vector2.Lerp(trunkEnd + vector, center + vector, num2);
				vector3 = Custom.DirVec(trunkEnd + vector, center + vector);
				vector4 = Custom.PerpendicularVector(vector3);
				num3 = num + Mathf.Pow(num2, 0.3f) * 20f * Mathf.Pow(1f - num2, 0.3f);
			}
			if (j == 0)
			{
				vector2 -= vector3 * 20f;
			}
			triangleMesh.MoveVertice(j, vector2 + vector4 * num3 - camPos);
			triangleMesh.MoveVertice(j + 1, vector2 - vector4 * num3 - camPos);
		}
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public static void GetTrunkBezier(Room room, PlacedObject pObj, out Vector2 a, out Vector2 cA, out Vector2 b, out Vector2 cB)
	{
		LobeTreeData lobeTreeData = pObj.data as LobeTreeData;
		a = pObj.pos + lobeTreeData.rootOffset;
		b = pObj.pos + lobeTreeData.handlePos;
		cB = b + lobeTreeData.handlePos.normalized * (Vector2.Distance(a, b) * 0.6f);
		Vector2 vector = new Vector2(0f, 0f);
		for (int i = -2; i <= 2; i++)
		{
			for (int j = -2; j <= 2; j++)
			{
				if (room.GetTile(room.GetTilePosition(a) + new IntVector2(j, i)).Solid)
				{
					vector -= new Vector2(j, i) / (j * j + i * i);
				}
			}
		}
		if (vector == Vector2.zero)
		{
			vector = Vector2.up;
		}
		else
		{
			vector.Normalize();
		}
		cA = a + vector * (Vector2.Distance(a, b) * 0.6f);
	}

	public static bool ValidSpawnPos(Room room, IntVector2 pos, List<Vector2> lobeTreeSpawnLocsSoFar)
	{
		Vector2 b = room.MiddleOfTile(pos);
		for (int i = 0; i < lobeTreeSpawnLocsSoFar.Count; i++)
		{
			if (Vector2.Distance(lobeTreeSpawnLocsSoFar[i], b) < 140f)
			{
				return false;
			}
		}
		foreach (WarpPoint warpPoint in room.warpPoints)
		{
			if (Vector2.Distance(warpPoint.pos, b) < warpPoint.PullRadius * 1.5f)
			{
				return false;
			}
		}
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if ((placedObject.type == PlacedObject.Type.DataPearl || placedObject.type == PlacedObject.Type.UniqueDataPearl || placedObject.type == PlacedObject.Type.RippleSpawnEgg || placedObject.type == PlacedObject.Type.KarmaFlower) && Vector2.Distance(placedObject.pos, b) < 100f)
			{
				return false;
			}
			if (placedObject.type == PlacedObject.Type.RippleTree && Mathf.Abs(b.x - placedObject.pos.x) < 150f)
			{
				return false;
			}
		}
		if (room.aimap.getTerrainProximity(pos) < 3)
		{
			return false;
		}
		AItile aItile = room.aimap.getAItile(pos.x - 1, pos.y);
		AItile aItile2 = room.aimap.getAItile(pos.x + 1, pos.y);
		if (aItile == null || aItile.acc == AItile.Accessibility.Solid || aItile.acc == AItile.Accessibility.Floor || aItile.acc == AItile.Accessibility.Ceiling)
		{
			return false;
		}
		if (aItile2 == null || aItile2.acc == AItile.Accessibility.Solid || aItile2.acc == AItile.Accessibility.Floor || aItile2.acc == AItile.Accessibility.Ceiling)
		{
			return false;
		}
		return true;
	}

	public static bool ValidRootPos(Room room, Vector2 testPos)
	{
		IntVector2 tilePosition = room.GetTilePosition(testPos);
		IntVector2[] eightDirectionsAndZero = Custom.eightDirectionsAndZero;
		foreach (IntVector2 intVector in eightDirectionsAndZero)
		{
			if (!room.GetTile(tilePosition + intVector + new IntVector2(0, -2)).Solid)
			{
				return false;
			}
		}
		return true;
	}
}
