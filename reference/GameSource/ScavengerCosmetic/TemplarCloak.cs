using RWCustom;
using UnityEngine;

namespace ScavengerCosmetic;

public class TemplarCloak : Template
{
	public SimpleSegment[,] segments;

	public int size = 5;

	public float targetLength = 50f;

	public Color blackColor;

	public float seed;

	public TemplarCloak(ScavengerGraphics owner, int firstSprite)
		: base(owner, firstSprite)
	{
		segments = new SimpleSegment[size + 1, size + 1];
		totalSprites = 2;
		Random.State state = Random.state;
		Random.InitState(owner.scavenger.abstractCreature.ID.RandomSeed + 1);
		seed = Random.value;
		Random.state = state;
	}

	public override void Reset()
	{
		for (int i = 0; i <= size; i++)
		{
			for (int j = 0; j <= size; j++)
			{
				segments[j, i].Reset(owner.owner.bodyChunks[0].pos);
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[firstSprite] = TriangleMesh.MakeGridMesh("Futile_White", size);
		sLeaser.sprites[firstSprite].shader = rCam.game.rainWorld.Shaders["TemplarCloak"];
		sLeaser.sprites[firstSprite + 1] = new FSprite("Futile_White")
		{
			shader = rCam.game.rainWorld.Shaders["GoldenGlow"]
		};
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		TriangleMesh triangleMesh = sLeaser.sprites[firstSprite] as TriangleMesh;
		int num = 0;
		for (int i = 0; i <= size; i++)
		{
			for (int j = 0; j <= size; j++)
			{
				triangleMesh.MoveVertice(num++, segments[j, i].DrawPos(timeStacker) - camPos);
			}
		}
		num = 0;
		for (int k = 0; k <= size; k++)
		{
			for (int l = 0; l <= size; l++)
			{
				Color goldRGB = RainWorld.GoldRGB;
				Vector2 p = triangleMesh.vertices[l + Mathf.Max(k - 1, 0) * (size + 1)];
				Vector2 p2 = triangleMesh.vertices[l + Mathf.Min(k + 1, size) * (size + 1)];
				Vector2 vector = Custom.DirVec(p, p2) * 5f;
				goldRGB = Color.Lerp(goldRGB, blackColor, Mathf.PerlinNoise(vector.x, vector.y) * 0.5f);
				goldRGB.a = seed;
				triangleMesh.verticeColors[num++] = goldRGB;
			}
		}
		Vector2 vector2 = Vector2.Lerp(base.scavGrphs.scavenger.mainBodyChunk.lastPos, base.scavGrphs.scavenger.mainBodyChunk.pos, timeStacker);
		sLeaser.sprites[firstSprite + 1].SetPosition(vector2 - camPos);
		sLeaser.sprites[firstSprite + 1].alpha = ((base.scavGrphs.scavenger.karmicArmor != null) ? 0.85f : 0.55f);
		sLeaser.sprites[firstSprite + 1].scale = ((base.scavGrphs.scavenger.karmicArmor != null) ? 15f : 12.5f);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer container)
	{
		container = rCam.ReturnFContainer("Bloom");
		container.AddChild(sLeaser.sprites[firstSprite + 1]);
	}

	public override void Update()
	{
		float num = targetLength / (float)size;
		BodyChunk mainBodyChunk = base.scavGrphs.scavenger.mainBodyChunk;
		Vector2 normalized = (base.scavGrphs.scavenger.bodyChunks[2].pos - mainBodyChunk.pos).normalized;
		Room room = base.scavGrphs.scavenger.room;
		for (int i = 0; i <= size; i++)
		{
			float targetDist = targetLength / (float)size * 0.5f;
			for (int j = 0; j <= size; j++)
			{
				segments[j, i].lastPos = segments[j, i].pos;
				segments[j, i].vel = Vector2.ClampMagnitude(segments[j, i].vel * 0.95f, 10f);
				if (i > 0)
				{
					ConnectSegments(j, i, j, i - 1, num, 0.7f);
				}
				if (j > 0)
				{
					ConnectSegments(j, i, j - 1, i, targetDist, 0.5f);
				}
			}
		}
		ConnectEnd();
		for (int k = 2; k <= size; k++)
		{
			float num2 = (float)k / (float)size;
			for (int l = 0; l <= size; l++)
			{
				float num3 = (float)l / (float)size * 2f - 1f;
				ref SimpleSegment reference = ref segments[l, k];
				reference.vel.y -= 0.4f;
				reference.vel.x -= normalized.x * (1f - num2) * 3f;
				float num4 = 1f - 2f * num2;
				if (num4 > 0f)
				{
					reference.vel += -normalized * num4;
					reference.vel.x += num4 * num3 * 3f * (1f - 0.5f * Mathf.Abs(normalized.x));
				}
			}
		}
		for (int m = 0; m <= size; m++)
		{
			float t = (float)m / (float)size;
			for (int n = 0; n <= size; n++)
			{
				ref SimpleSegment reference2 = ref segments[n, m];
				reference2.pos += reference2.vel;
				if (m > 2 && room.GetTile(reference2.lastPos).Solid && Custom.DistLess(reference2.lastPos, reference2.pos, num * 4f))
				{
					float rad = Mathf.Lerp(3f, 1f, t);
					SharedPhysics.TerrainCollisionData cd = new SharedPhysics.TerrainCollisionData(reference2.pos, reference2.lastPos, reference2.vel, rad, default(IntVector2), goThroughFloors: true);
					cd = SharedPhysics.HorizontalCollision(room, cd);
					cd = SharedPhysics.VerticalCollision(room, cd);
					reference2.pos = cd.pos;
					reference2.vel = cd.vel;
				}
			}
		}
	}

	public void ConnectEnd()
	{
		BodyChunk mainBodyChunk = base.scavGrphs.scavenger.mainBodyChunk;
		BodyChunk bodyChunk = base.scavGrphs.scavenger.bodyChunks[2];
		Vector2 normalized = (bodyChunk.pos - mainBodyChunk.pos).normalized;
		Vector2 vector = Custom.PerpendicularVector(normalized);
		for (int i = 0; i <= size; i++)
		{
			float num = (float)i / (float)size * 2f - 1f;
			ref SimpleSegment reference = ref segments[i, 0];
			reference.pos = bodyChunk.pos + vector * (num * 5f);
			reference.vel = bodyChunk.vel;
		}
		for (int j = 0; j <= size; j++)
		{
			float num2 = (float)j / (float)size * 2f - 1f;
			ref SimpleSegment reference2 = ref segments[j, 1];
			reference2.pos = mainBodyChunk.pos + normalized * (mainBodyChunk.rad * 0.3f) + vector * (num2 * 9f);
			reference2.vel = mainBodyChunk.vel;
		}
	}

	public void ConnectSegments(int x, int y, int otherX, int otherY, float targetDist, float massRatio)
	{
		ref SimpleSegment reference = ref segments[x, y];
		ref SimpleSegment reference2 = ref segments[otherX, otherY];
		Vector2 vector = reference2.pos - reference.pos;
		float magnitude = vector.magnitude;
		if (magnitude > targetDist)
		{
			Vector2 vector2 = vector / magnitude;
			float num = targetDist - magnitude;
			reference.pos -= vector2 * (0.45f * num * massRatio);
			reference.vel -= vector2 * (0.35f * num * massRatio);
			reference2.pos += vector2 * (0.45f * num * (1f - massRatio));
			reference2.vel += vector2 * (0.35f * num * (1f - massRatio));
		}
	}
}
