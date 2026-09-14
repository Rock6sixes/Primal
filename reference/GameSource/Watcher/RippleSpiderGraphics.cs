using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleSpiderGraphics : GraphicsModule
{
	public class Tentacle : Limb
	{
		public SimpleSegment[] segments;

		public float length;

		public int detachedTimer;

		public Tentacle(GraphicsModule owner, BodyChunk connection, int num, float length)
			: base(owner, connection, num, 0.1f, 0.7f, 0.99f, 17f, 0.8f)
		{
			segments = new SimpleSegment[7];
			this.length = length;
		}

		public override void Reset(Vector2 resetPoint)
		{
			base.Reset(resetPoint);
			base.mode = Mode.Dangle;
			if (segments != null)
			{
				for (int i = 0; i < segments.Length; i++)
				{
					segments[i].Reset(resetPoint);
				}
			}
		}

		public void MoveEnd()
		{
			RippleSpider rippleSpider = owner.owner as RippleSpider;
			RippleSpiderGraphics rippleSpiderGraphics = owner as RippleSpiderGraphics;
			Vector2 normalized = (rippleSpider.bodyChunks[0].pos - rippleSpider.bodyChunks[1].pos).normalized;
			float num = 0.5f + 0.5f * Mathf.Sin(rippleSpider.runCycle * (float)Math.PI + (float)limbNumber / (float)rippleSpiderGraphics.tentacles.Length * (float)Math.PI * 2f);
			if (base.mode == Mode.HuntRelativePosition)
			{
				base.mode = Mode.Dangle;
			}
			Vector2 vector = Custom.RotateAroundOrigo(normalized, ((float)limbNumber - (float)(rippleSpiderGraphics.tentacles.Length - 1) / 2f) * 35f);
			if (rippleSpider.Consious)
			{
				vector += rippleSpider.walkSide * -Custom.PerpendicularVector(normalized) * 0.5f;
				vector += rippleSpider.travelDir * Mathf.Pow(num, 0.5f);
				vector.Normalize();
			}
			Vector2 vector2 = connection.pos + vector * 3f;
			ConnectToPoint(vector2, length, push: false, 0f, connection.vel, 0.1f, 0f);
			ConnectToPoint(connection.pos, length, push: false, 0f, connection.vel, 0.1f, 0f);
			if (Custom.DistLess(pos, connection.pos, 6f))
			{
				pos = connection.pos + (pos - connection.pos).normalized * 6f;
			}
			Vector2 vector3 = vector2 + vector * length;
			Tentacle[] tentacles = rippleSpiderGraphics.tentacles;
			foreach (Tentacle tentacle in tentacles)
			{
				if (this != tentacle && Custom.DistLess(vector3, tentacle.absoluteHuntPos, length * 0.3f))
				{
					vector3 = tentacle.absoluteHuntPos + (vector3 - tentacle.absoluteHuntPos).normalized * (length * 0.3f);
				}
			}
			if (!reachedSnapPosition)
			{
				FindGrip(rippleSpider.room, vector2, connection.pos + vector * connection.rad, length * 1.5f, vector3, -2, -2, behindWalls: false);
				Vector2 vector4 = Vector2.Lerp(vector3, connection.pos, 0.3f);
				if (base.mode != Mode.HuntAbsolutePosition && !rippleSpider.room.GetTile(vector4).Solid && (rippleSpider.room.terrain == null || !rippleSpider.room.terrain.Contains(vector4)))
				{
					base.mode = Mode.HuntAbsolutePosition;
					absoluteHuntPos = vector4;
					GrabbedTerrain();
				}
				detachedTimer++;
			}
			else
			{
				detachedTimer = 0;
			}
			if (!Custom.DistLess(pos, absoluteHuntPos, length * Mathf.Pow(1f - num, 0.2f)))
			{
				base.mode = Mode.Dangle;
				vel += vector * 3f;
				vel = Vector2.Lerp(vel, vector3 - pos, 0.5f);
			}
			else
			{
				vel += vector * Custom.LerpMap(detachedTimer, 10f, 40f, 1f, 0.1f);
			}
		}

		public void MoveSegments()
		{
			RippleSpider rippleSpider = owner.owner as RippleSpider;
			Vector2 vector = Custom.InverseKinematic(connection.pos, pos, length * 0.6f, length * 0.7f, 0f - rippleSpider.walkSide);
			Vector2 vector2 = connection.pos;
			Vector2 cA = vector;
			Vector2 vector3 = pos;
			Vector2 cB = vector3 + Custom.RotateAroundOrigo(vector2 - vector3, rippleSpider.walkSide * 70f).normalized;
			for (int i = 0; i < segments.Length; i++)
			{
				float f = (float)i / (float)(segments.Length - 1);
				segments[i].lastPos = segments[i].pos;
				segments[i].pos = Custom.Bezier(vector2, cA, vector3, cB, f);
			}
		}

		public override void Update()
		{
			base.Update();
			MoveEnd();
			MoveSegments();
		}
	}

	public Tentacle[] tentacles;

	public float opacity = 1f;

	public float lastOpacity = 1f;

	public int headSprite;

	public int tailSprite;

	public int firstTentacleSprite;

	public int firstEffectSprite;

	public int totalSprites;

	public RippleSpider Spider => base.owner as RippleSpider;

	public RippleSpiderGraphics(PhysicalObject po)
		: base(po, internalContainers: false)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(Spider.abstractCreature.ID.RandomSeed);
		tentacles = new Tentacle[4];
		bodyParts = new BodyPart[tentacles.Length];
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i] = new Tentacle(this, Spider.bodyChunks[0], i, 40f);
			bodyParts[i] = tentacles[i];
		}
		UnityEngine.Random.state = state;
		for (int j = 0; j < bodyParts.Length; j++)
		{
			bodyParts[j].bodyPartArrayIndex = j;
		}
		firstEffectSprite = totalSprites;
		totalSprites += 2;
		headSprite = totalSprites;
		totalSprites++;
		tailSprite = totalSprites;
		totalSprites++;
		firstTentacleSprite = totalSprites;
		totalSprites += tentacles.Length;
	}

	public override void Update()
	{
		base.Update();
		Tentacle[] array = tentacles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Update();
		}
		lastOpacity = opacity;
		if (Spider.enteringShortCut.HasValue)
		{
			opacity = Mathf.Max(0f, opacity - 0.1f);
		}
		else
		{
			opacity = Mathf.MoveTowards(opacity, Mathf.PerlinNoise(0f, (float)Spider.room.game.clock / 40f) * 0.5f + 0.5f, 0.1f);
		}
		opacity = Mathf.Min(opacity, 1f - Spider.DespawnFade);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[headSprite] = new FSprite("Circle20")
		{
			shader = rCam.game.rainWorld.Shaders["RippleSpawnBody"],
			scale = Spider.bodyChunks[0].rad / 10f
		};
		sLeaser.sprites[tailSprite] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: false);
		sLeaser.sprites[tailSprite].shader = rCam.game.rainWorld.Shaders["RippleSpawnBody"];
		for (int i = 0; i < tentacles.Length; i++)
		{
			sLeaser.sprites[firstTentacleSprite + i] = TriangleMesh.MakeLongMesh(tentacles[i].segments.Length, pointyTip: false, customColor: false);
			sLeaser.sprites[firstTentacleSprite + i].shader = rCam.game.rainWorld.Shaders["RippleSpawnBody"];
		}
		for (int j = 0; j < 2; j++)
		{
			sLeaser.sprites[firstEffectSprite + j] = new FSprite("Futile_White")
			{
				shader = rCam.game.rainWorld.Shaders["RippleHybridBoth"]
			};
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(Spider.bodyChunks[0].lastPos, Spider.bodyChunks[0].pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(Spider.bodyChunks[1].lastPos, Spider.bodyChunks[1].pos, timeStacker);
		Vector2 normalized = (vector - vector2).normalized;
		sLeaser.sprites[headSprite].SetPosition(vector - camPos);
		TriangleMesh triangleMesh = sLeaser.sprites[tailSprite] as TriangleMesh;
		Vector2 a = vector;
		Vector2 cA = vector - normalized * 10f;
		Vector2 b = vector2 - Custom.RotateAroundOrigo(normalized, Spider.walkSide * 45f) * (Spider.bodyChunks[1].rad * 4f);
		Vector2 cB = vector2;
		for (int i = 0; i < triangleMesh.vertices.Length; i += 2)
		{
			float num = (float)i / (float)(triangleMesh.vertices.Length - 2);
			float bodyRad = GetBodyRad(num);
			Vector2 vector3 = Custom.Bezier(a, cA, b, cB, num);
			Vector2 vector4 = Custom.PerpendicularVector(Custom.BezierDerivative(a, cA, b, cB, num).normalized);
			triangleMesh.MoveVertice(i, vector3 + vector4 * bodyRad - camPos);
			triangleMesh.MoveVertice(i + 1, vector3 - vector4 * bodyRad - camPos);
		}
		for (int j = 0; j < tentacles.Length; j++)
		{
			Tentacle tentacle = tentacles[j];
			TriangleMesh triangleMesh2 = sLeaser.sprites[firstTentacleSprite + j] as TriangleMesh;
			Vector2 vector5 = vector;
			float num2 = 3f;
			for (int k = 0; k < tentacle.segments.Length; k++)
			{
				Vector2 vector6 = tentacle.segments[k].DrawPos(timeStacker);
				Vector2 normalized2 = (vector6 - vector5).normalized;
				Vector2 vector7 = Custom.PerpendicularVector(normalized2);
				float num3 = (float)k / (float)(tentacle.segments.Length - 1);
				float num4 = Vector2.Distance(vector6, vector5) / 5f;
				float num5 = 3f * (1f - num3);
				Vector2 vector8 = vector7 * ((num5 + num2) * 0.5f);
				Vector2 vector9 = vector7 * num5;
				triangleMesh2.MoveVertice(k * 4, vector5 - vector8 + normalized2 * num4 - camPos);
				triangleMesh2.MoveVertice(k * 4 + 1, vector5 + vector8 + normalized2 * num4 - camPos);
				triangleMesh2.MoveVertice(k * 4 + 2, vector6 - vector9 - normalized2 * num4 - camPos);
				triangleMesh2.MoveVertice(k * 4 + 3, vector6 + vector9 - normalized2 * num4 - camPos);
				num2 = num5;
				vector5 = vector6;
			}
		}
		float num6 = Mathf.Lerp(lastOpacity, opacity, timeStacker);
		FSprite[] sprites = sLeaser.sprites;
		for (int l = 0; l < sprites.Length; l++)
		{
			sprites[l].alpha = num6 * 0.7f;
		}
		sLeaser.sprites[firstEffectSprite].SetPosition(vector - camPos);
		sLeaser.sprites[firstEffectSprite].color = new Color(0f, num6, 0f, 1f);
		sLeaser.sprites[firstEffectSprite].scale = 5f;
		sLeaser.sprites[firstEffectSprite + 1].SetPosition(vector2 - camPos);
		sLeaser.sprites[firstEffectSprite + 1].color = new Color(0.5f, num6, 0f, 1f);
		sLeaser.sprites[firstEffectSprite + 1].scale = 6.25f;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public float GetBodyRad(float t)
	{
		float rad = Spider.bodyChunks[0].rad;
		return Mathf.Lerp(12f * Mathf.Sin(t * (float)Math.PI), rad, (1f - t) * (1f - t));
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		FSprite[] sprites = sLeaser.sprites;
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].color = RainWorld.RippleColor;
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("GrabShaders");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
