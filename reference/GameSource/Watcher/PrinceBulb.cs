using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class PrinceBulb : PhysicalObject, IDrawable, INotifyWhenRoomIsReady
{
	public Color eyeColor;

	public Color blackColor;

	public Vector2 idealPos;

	public const int petalMeshQuadsX = 8;

	public const int petalMeshQuadsY = 16;

	public const int slitMeshQuads = 24;

	public float openFac;

	public float lastOpenFac;

	public float openVel;

	public float targetOpenFac;

	public Prince prince;

	public int firstBgPetalSprite;

	public int firstFgPetalSprite;

	public int totalSprites;

	public int[] petalLayerOrder = new int[4] { 0, 1, 3, 2 };

	public int spritesPerPetal = 3;

	public AbstractPrinceBulb AbstractBulb => (AbstractPrinceBulb)abstractPhysicalObject;

	public Vector2 AnchorPos => AbstractBulb.pos.Tile.ToVector2() * 20f + new Vector2(10f, 10f);

	public int PetalSprite(int index, int part)
	{
		int num = petalLayerOrder[index];
		if (num == 0)
		{
			return firstBgPetalSprite + part;
		}
		return firstFgPetalSprite + (num - 1) * spritesPerPetal + part;
	}

	public Vector2 GetDirection(float timeStacker)
	{
		return ((Vector2)BulbForwardDir(timeStacker)).normalized;
	}

	public PrinceBulb(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		idealPos = AnchorPos;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, idealPos, AbstractBulb.radius, 8f + 4f * (float)AbstractBulb.growthStage);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.95f;
		base.gravity = 0.5f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.4f;
		base.CollideWithTerrain = false;
		collisionRange = AbstractBulb.radius * 2f;
		eyeColor = abstractPhysicalObject.world.region?.regionParams.corruptionEyeColor ?? Color.blue;
		firstBgPetalSprite = totalSprites;
		totalSprites += spritesPerPetal;
		firstFgPetalSprite = totalSprites;
		totalSprites += 3 * spritesPerPetal;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		base.firstChunk.pos = Vector2.ClampMagnitude(base.firstChunk.pos - idealPos, 20f) + idealPos;
		base.firstChunk.vel += (idealPos - base.firstChunk.pos) * 0.1f;
		if (prince != null)
		{
			base.firstChunk.vel += prince.bodyChunks[0].vel * 0.04f;
		}
		Prince obj = prince;
		targetOpenFac = ((obj != null) ? (obj.stem.Extension * 0.8f) : 0f);
		lastOpenFac = openFac;
		openFac = Mathf.Clamp(openFac + openVel, 0f, 2f);
		openVel *= 0.8f;
		openVel += (targetOpenFac - openFac) * 0.03f;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		TriangleMesh.Triangle[] tris = MakeGridMeshTris(8, 16);
		TriangleMesh.Triangle[] tris2 = MakeGridMeshTris(1, 24);
		FShader shader = Custom.rainWorld.Shaders["RippleBasicBothSides"];
		for (int i = 0; i < 4; i++)
		{
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
			AssignGridMeshUVs(triangleMesh, 8, 16);
			sLeaser.sprites[PetalSprite(i, 0)] = triangleMesh;
			sLeaser.sprites[PetalSprite(i, 0)].shader = shader;
			for (int j = 0; j < 2; j++)
			{
				TriangleMesh triangleMesh2 = new TriangleMesh("Futile_White", tris2, customColor: true);
				AssignGridMeshUVs(triangleMesh2, 1, 24);
				sLeaser.sprites[PetalSprite(i, 1 + j)] = triangleMesh2;
				sLeaser.sprites[PetalSprite(i, 1 + j)].shader = shader;
			}
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public static TriangleMesh.Triangle[] MakeGridMeshTris(int quadsX, int quadsY)
	{
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[2 * quadsX * quadsY];
		int num = 0;
		int num2 = quadsX + 1;
		for (int i = 0; i < quadsY; i++)
		{
			for (int j = 0; j < quadsX; j++)
			{
				int num3 = i * num2 + j;
				array[num++] = new TriangleMesh.Triangle(num3, num3 + 1, num3 + num2 + 1);
				array[num++] = new TriangleMesh.Triangle(num3, num3 + num2 + 1, num3 + num2);
			}
		}
		return array;
	}

	public static void AssignGridMeshUVs(TriangleMesh mesh, int quadsX, int quadsY)
	{
		for (int i = 0; i <= quadsY; i++)
		{
			for (int j = 0; j <= quadsX; j++)
			{
				mesh.UVvertices[i * (quadsX + 1) + j] = new Vector2((float)j / (float)quadsX, (float)i / (float)quadsY);
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		FContainer fContainer = newContatiner ?? rCam.ReturnFContainer("Background");
		FContainer fContainer2 = newContatiner ?? rCam.ReturnFContainer("Items");
		FSprite[] sprites = sLeaser.sprites;
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].RemoveFromContainer();
		}
		for (int j = 0; j < firstFgPetalSprite; j++)
		{
			fContainer.AddChild(sLeaser.sprites[j]);
		}
		for (int k = firstFgPetalSprite; k < totalSprites; k++)
		{
			fContainer2.AddChild(sLeaser.sprites[k]);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		for (int i = 0; i < 4; i++)
		{
			sLeaser.sprites[PetalSprite(i, 0)].color = palette.blackColor;
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < 4; i++)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[PetalSprite(i, 0)] as TriangleMesh;
			for (int j = 0; j <= 16; j++)
			{
				for (int k = 0; k <= 8; k++)
				{
					float aroundPoleDegrees = Mathf.Lerp(-45f, 45f, (float)k / 8f);
					float alongBulbDegrees = (float)j / 16f * 180f;
					Vector2 vector = OnPetalPos(i, aroundPoleDegrees, alongBulbDegrees, timeStacker);
					triangleMesh.MoveVertice(j * 9 + k, vector - camPos);
				}
			}
			for (int l = 0; l < 2; l++)
			{
				TriangleMesh triangleMesh2 = sLeaser.sprites[PetalSprite(i, 1 + l)] as TriangleMesh;
				Vector2 v = default(Vector2);
				for (int m = 0; m < triangleMesh2.vertices.Length; m += 2)
				{
					float num = (float)m / (float)(triangleMesh2.vertices.Length - 2);
					float num2 = num * 60f;
					float num3 = (1f - num) * 3f + 1f;
					num3 /= Mathf.Sin(num2 * ((float)Math.PI / 180f)) * base.firstChunk.rad;
					num3 *= 57.29578f;
					float aroundPoleDegrees2 = ((l == 0) ? (-45f) : Mathf.Max(-45f, 45f - num3));
					float aroundPoleDegrees3 = ((l == 0) ? Mathf.Min(45f, -45f + num3) : 45f);
					Vector2 vector2 = OnPetalPos(i, aroundPoleDegrees2, num2, timeStacker);
					Vector2 vector3 = OnPetalPos(i, aroundPoleDegrees3, num2, timeStacker);
					if (m > 0)
					{
						Vector2 vector4 = Custom.PerpendicularVector(vector2, v);
						vector2 -= vector4 * 0.5f;
						vector3 += vector4 * 0.5f;
					}
					triangleMesh2.MoveVertice(m, vector2 - camPos);
					triangleMesh2.MoveVertice(m + 1, vector3 - camPos);
					triangleMesh2.verticeColors[m] = Color.Lerp(blackColor, eyeColor, Mathf.Sqrt((1f - num) * 2f));
					triangleMesh2.verticeColors[m + 1] = Color.Lerp(blackColor, eyeColor, Mathf.Sqrt((1f - num) * 2f));
					v = vector2;
				}
			}
		}
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public Vector3 OnPetalPos(int petal, float aroundPoleDegrees, float alongBulbDegrees, float timeStacker)
	{
		Vector3 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector3 vector2 = OnBulbDir((float)petal * 90f + aroundPoleDegrees, alongBulbDegrees, timeStacker);
		Vector3 vector3 = OnBulbDir((float)petal * 90f, 90f, timeStacker);
		Vector3 vector4 = BulbForwardDir(timeStacker);
		float num = Mathf.Lerp(lastOpenFac, openFac, timeStacker);
		float num2 = 1f - Mathf.Sin(Mathf.Min(90f, alongBulbDegrees) * ((float)Math.PI / 180f));
		return vector + base.firstChunk.rad * vector2 - 0.5f * base.firstChunk.rad * num2 * Mathf.Pow(num, 3f) * vector4 + 1.8f * base.firstChunk.rad * num2 * num * vector3;
	}

	public Vector3 OnBulbDir(float aroundPoleDegrees, float alongBulbDegrees, float timeStacker)
	{
		Vector3 vector = BulbForwardDir(timeStacker);
		Vector3 vector2 = new Vector3(0f, 0.35f, -1f);
		vector2 = (vector2 - vector * Vector3.Dot(vector2, vector)).normalized;
		Vector3 vector3 = Quaternion.AngleAxis(aroundPoleDegrees, vector) * vector2;
		float f = alongBulbDegrees * ((float)Math.PI / 180f);
		return vector * Mathf.Cos(f) + vector3 * Mathf.Sin(f);
	}

	public Vector3 BulbForwardDir(float timeStacker)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) + AbstractBulb.direction * base.firstChunk.rad - idealPos;
		float z = Mathf.Lerp(1f, 0.3f, Mathf.Lerp(lastOpenFac, openFac, timeStacker));
		return new Vector3(vector.x / base.firstChunk.rad, vector.y / base.firstChunk.rad, z).normalized;
	}

	public void ShortcutsReady()
	{
	}

	public void AIMapReady()
	{
		if (room.game.IsStorySession && (room.game.GetStorySession.saveState.miscWorldSaveData.numberOfPrinceEncounters >= 3 || Region.IsDaemonRegion(room.world.name)))
		{
			room.AddObject(new CosmeticPrinceStem(this, new Vector2(50f, 80f), new Vector2(70f, 60f), new Vector2(20f, -10f)));
			room.AddObject(new CosmeticPrinceStem(this, new Vector2(50f, -50f), new Vector2(90f, -50f), new Vector2(20f, 0f)));
			prince = new Prince(new AbstractPhysicalObject(room.world, WatcherEnums.AbstractObjectType.Prince, null, abstractPhysicalObject.pos, room.game.GetNewID()));
			PrinceStem princeStem = new PrinceStem(this, prince);
			prince.stem = princeStem;
			room.AddObject(princeStem);
			room.AddObject(prince);
			openFac = 0.7f;
			targetOpenFac = 0.7f;
		}
	}
}
