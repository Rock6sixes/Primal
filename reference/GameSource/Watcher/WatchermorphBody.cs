using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class WatchermorphBody : CosmeticSprite
{
	public class FakeArm : CosmeticSprite
	{
		public Color color;

		public float transform;

		public float lastTransform;

		public float grow;

		public float animationTime;

		public float lastAnimationTime;

		public bool left;

		public float scale = 1f;

		public Vector2 finDir;

		public Vector2 direction;

		public Vector2 bodyDir;

		public Vector2 tailDir;

		public Vector2[] armMesh;

		public Vector2[] lastArmMesh;

		public int nQuads = 15;

		public Player owner;

		public FakeArm(Room room, Player owner)
		{
			base.room = room;
			this.owner = owner;
			int num = (nQuads * 2 + 1) * 2;
			armMesh = new Vector2[num];
			lastArmMesh = new Vector2[num];
		}

		public override void Update(bool eu)
		{
			UpdateFinPos();
			UpdateArmMesh();
		}

		public void UpdateFinPos()
		{
			finDir = Vector2.Lerp(finDir, tailDir, 0.05f);
		}

		public void UpdateArmMesh()
		{
			for (int i = 0; i < armMesh.Length; i++)
			{
				lastArmMesh[i] = armMesh[i];
			}
			int num = armMesh.Length / 2;
			float num2 = Mathf.Sin(animationTime * (float)Math.PI);
			float num3 = Mathf.Cos(animationTime * (float)Math.PI);
			float num4 = (4f + transform * 4f) * grow;
			float num5 = (20f + transform * 100f) * grow * scale;
			float num6 = 10f + num2 * 90f;
			Vector2 vector = Custom.rotateVectorDeg(direction * (0.4f + Mathf.Max(0f, 0f - num3) * 0.6f), num6);
			Vector2 vector2 = Custom.rotateVectorDeg(direction * (1f - Mathf.Abs(0f - num3) * 0.4f), num6 * 0.7f + (20f + num2 * 40f));
			for (int j = 0; j < num; j++)
			{
				float num7 = (float)j / (float)(num - 1);
				float t = Mathf.InverseLerp(0.4f, 0.5f, num7);
				Vector2 vector3 = Custom.PerpendicularVector(Vector2.Lerp(vector, vector2, t)) * 0.5f;
				Vector2 a = vector * (num5 * num7);
				Vector2 b = vector * (num5 * 0.5f) + vector2 * (num5 * Mathf.Clamp01(num7 - 0.5f));
				Vector2 vector4 = Vector2.Lerp(a, b, t);
				a = vector4 - vector3 * num4;
				b = vector4 + vector3 * num4;
				if (left)
				{
					a = Vector2.Reflect(a, -direction);
					b = Vector2.Reflect(b, -direction);
				}
				armMesh[j * 2] = a;
				armMesh[j * 2 + 1] = b;
			}
			Vector2 vector5 = Vector2.up * 10f;
			float num8 = ((!left) ? 1 : (-1));
			if (left)
			{
				vector = Vector2.Reflect(vector, -direction);
			}
			Vector2 vector6 = direction * num8;
			vector6 = (vector6 + vector).normalized;
			for (int k = 0; k < num; k++)
			{
				float num9 = (float)k / (float)(num - 1);
				float num10 = Mathf.InverseLerp(0.5f - num9 * 0.2f, 0.7f, transform);
				Vector2 vector7 = Vector2.Lerp(bodyDir, finDir.normalized, Mathf.Clamp01(num9 * num10 - 0.2f));
				Vector2 vector8 = Vector2.Lerp(vector6 * (1f + num10 * 3f), (1f + num10 * 4f) * vector7, num9 * num10);
				Vector2 vector9 = vector8 * (num9 * num5);
				Vector2 vector10 = Custom.PerpendicularVector(vector8.normalized) * num8;
				Vector2 vector11 = vector10 * (Mathf.Sin((0f - animationTime) * 10f + num9 * 20f) * num4 * Mathf.InverseLerp(0.1f, 1f, num9));
				vector11 *= num8;
				vector5 += vector8 * (num5 / (float)num + 50f * num9 * num10);
				float num11 = (num4 * 0.5f + num9 * num10 * 12f) * Mathf.InverseLerp(1f, 0.8f, num9);
				Vector2 b2 = vector9 - vector10 * num11 + vector11;
				Vector2 b3 = vector9 + vector10 * num11 + vector11;
				armMesh[k * 2] = Vector2.Lerp(armMesh[k * 2], b2, num10);
				armMesh[k * 2 + 1] = Vector2.Lerp(armMesh[k * 2 + 1], b3, num10);
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites = new FSprite[1];
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(nQuads * 2, 1), customColor: true);
			sLeaser.sprites[0] = triangleMesh;
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["WatcherMorphFin"];
			UpdateUV(sLeaser.sprites[0] as TriangleMesh);
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			sLeaser.sprites[0].SetPosition(pos - camPos);
			if (owner.graphicsModule != null && owner.graphicsModule is PlayerGraphics playerGraphics)
			{
				color = playerGraphics.lastBodyColor;
			}
			UpdateMesh(sLeaser.sprites[0] as TriangleMesh, timeStacker);
			sLeaser.sprites[0].MoveToBack();
		}

		public void UpdateMesh(TriangleMesh mesh, float timeStacker)
		{
			for (int i = 0; i < mesh.vertices.Length; i++)
			{
				mesh.vertices[i] = Vector2.Lerp(lastArmMesh[i], armMesh[i], timeStacker);
			}
			int num = mesh.vertices.Length / 2;
			float value = Mathf.Lerp(lastTransform, transform, timeStacker);
			for (int j = 0; j < num; j++)
			{
				float num2 = (float)j / (float)(num - 1);
				float a = Mathf.InverseLerp(0.5f - num2 * 0.2f, 0.7f, value);
				Color color = new Color(this.color.r, this.color.g, this.color.b, a);
				mesh.verticeColors[j * 2] = color;
				mesh.verticeColors[j * 2 + 1] = color;
			}
		}

		public void UpdateUV(TriangleMesh mesh)
		{
			int num = mesh.vertices.Length / 2;
			for (int i = 0; i < num; i++)
			{
				float num2 = (float)i / (float)(num - 1);
				mesh.UVvertices[i * 2] = Vector2.up * num2;
				mesh.UVvertices[i * 2 + 1] = Vector2.up * num2 + Vector2.right;
			}
		}
	}

	public Color color;

	public bool debugColors;

	public Player owner;

	public int quadMult = 3;

	public Vector2 bodyDir;

	public Vector2 tailDir;

	public float width = 10f;

	public float segLength = 3f;

	public int nArms = 3;

	public float animationTime;

	public float watcherMorph;

	public float lastWatcherMorph;

	public FakeArm[] arms;

	public Vector2[] origTailVerts;

	public StaticSoundLoop glowLoop;

	public Vector2[] bodyMovement;

	public Vector2[] lastBodyMovement;

	public WatchermorphBody(Player owner)
	{
		this.owner = owner;
		bodyMovement = new Vector2[8 * quadMult + 1];
		lastBodyMovement = new Vector2[bodyMovement.Length];
		animationTime = owner.swimCycle;
		room = owner.room;
		arms = new FakeArm[nArms * 2];
		for (int i = 0; i < nArms; i++)
		{
			arms[i * 2] = new FakeArm(room, owner);
			arms[i * 2 + 1] = new FakeArm(room, owner)
			{
				left = true
			};
		}
		for (int j = 0; j < arms.Length; j++)
		{
			room.AddObject(arms[j]);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		pos = owner.mainBodyChunk.pos;
		lastWatcherMorph = watcherMorph;
		if (watcherMorph == 0f)
		{
			animationTime = owner.swimCycle;
		}
		else
		{
			animationTime += Mathf.Lerp(0.05f, 0.015f, watcherMorph) * Mathf.Clamp01(0.2f + owner.mainBodyChunk.vel.magnitude);
		}
		UpdateBodyMovement();
		UpdateArms();
		width = Mathf.Lerp(10f, 40f, Custom.LerpQuadEaseInOut(0f, 1f, Mathf.InverseLerp(0.05f, 1f, watcherMorph)));
		segLength = Mathf.Lerp(3f, 40f, Custom.LerpQuadEaseInOut(0f, 1f, watcherMorph));
		bool retract = watcherMorph > 0.1f;
		if (owner.graphicsModule != null)
		{
			(owner.graphicsModule as PlayerGraphics).hands[0].retract = retract;
			(owner.graphicsModule as PlayerGraphics).hands[1].retract = retract;
		}
		if (glowLoop == null)
		{
			glowLoop = new StaticSoundLoop(SoundID.Void_Sea_Individual_Worm_Glow_LOOP, default(Vector2), room, 0f, 1.6f);
		}
		if (glowLoop != null)
		{
			glowLoop.Update();
			glowLoop.pos = pos;
			if (room.game.creditsRoll != null)
			{
				glowLoop.volume *= 0.995f;
				return;
			}
			glowLoop.volume = Mathf.InverseLerp(0.4f, 0.7f, watcherMorph) * 0.3f;
			glowLoop.pitch = 1.6f - Mathf.InverseLerp(0.4f, 0.5f, watcherMorph) * 0.3f;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[4];
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", TriangleMesh.GridTriangles(8 * quadMult, 1), debugColors);
		sLeaser.sprites[0] = triangleMesh;
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["WatcherMorphBody"];
		origTailVerts = new Vector2[18];
		UpdateUV(sLeaser.sprites[0] as TriangleMesh);
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].shader = Custom.rainWorld.Shaders["UnderWaterLightStrong"];
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = Custom.rainWorld.Shaders["FlatLightNoisy"];
		sLeaser.sprites[3] = new FSprite("Futile_White");
		sLeaser.sprites[3].shader = Custom.rainWorld.Shaders["UnderWaterLightStrong"];
		rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[2]);
		rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[3]);
		rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[1]);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (!sLeaser.deleteMeNextFrame)
		{
			Vector2 position = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
			PlayerGraphics playerGraphics = owner.graphicsModule as PlayerGraphics;
			UpdateOrigTail(timeStacker, camPos);
			MoveMesh(sLeaser.sprites[0] as TriangleMesh, camPos, timeStacker);
			int num = playerGraphics?.faceSpriteIndex ?? (-1);
			if (playerGraphics != null)
			{
				color = playerGraphics.lastBodyColor;
			}
			if (num != -1)
			{
				sLeaser.sprites[0].container.AddChildAtIndex(sLeaser.sprites[0], num + 1);
			}
			if (!debugColors)
			{
				float num2 = Mathf.Lerp(lastWatcherMorph, watcherMorph, timeStacker);
				float num3 = Mathf.InverseLerp(0f, 0.5f, num2);
				float num4 = (rCam.voidSeaMode ? Mathf.InverseLerp(0f, -3000f, pos.y) : 0f);
				float num5 = Mathf.InverseLerp(0.5f, 0.7f, num2);
				sLeaser.sprites[0].color = new Color(color.r, color.g, color.b, num3);
				sLeaser.sprites[1].SetPosition(position);
				sLeaser.sprites[1].scale = Mathf.Lerp(num3 * 15f, 25f, num2);
				Color saturatedGold = RainWorld.SaturatedGold;
				saturatedGold.a = num5 * 0.3f;
				sLeaser.sprites[1].color = saturatedGold;
				sLeaser.sprites[3].SetPosition(position);
				sLeaser.sprites[3].scale = Mathf.Lerp(num3 * 5f, 20f, num2);
				sLeaser.sprites[3].color = new Color(color.r, color.g, color.b, num3);
				sLeaser.sprites[2].SetPosition(position);
				sLeaser.sprites[2].scale = Mathf.Lerp(num4 * 90f, 150f, num2);
				saturatedGold.a = Mathf.Lerp(num4 * 0.15f, 0.25f, num2);
				sLeaser.sprites[2].color = saturatedGold;
			}
		}
	}

	public void UpdateUV(TriangleMesh mesh)
	{
		int num = mesh.vertices.Length / 2;
		for (int i = 0; i < num; i++)
		{
			float num2 = (float)i / (float)(num - 1);
			mesh.UVvertices[i * 2] = Vector2.up * num2;
			mesh.UVvertices[i * 2 + 1] = Vector2.up * num2 + Vector2.right;
			if (debugColors)
			{
				mesh.verticeColors[i * 2] = ((i % 2 == 0) ? Color.blue : Color.red);
				mesh.verticeColors[i * 2 + 1] = ((i % 2 == 0) ? Color.blue : Color.red);
			}
		}
	}

	public void UpdateOrigTail(float timeStacker, Vector2 camPos)
	{
		if (owner.graphicsModule == null)
		{
			return;
		}
		Vector2[,] drawPositions = (owner.graphicsModule as PlayerGraphics).drawPositions;
		TailSegment[] tail = (owner.graphicsModule as PlayerGraphics).tail;
		Vector2 vector = Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker);
		Vector2 vector2 = Vector2.Lerp(drawPositions[1, 1], drawPositions[1, 0], timeStacker);
		Vector2 vector3 = (vector2 * 3f + vector) / 4f - camPos;
		float num = 6f;
		int num2 = 2;
		Vector2 normalized = (vector2 - vector).normalized;
		Vector2 vector4 = Custom.PerpendicularVector(normalized) * num;
		Vector2 vector5 = vector - normalized * num - camPos;
		origTailVerts[0] = vector5 - vector4;
		origTailVerts[1] = vector5 + vector4;
		for (int i = 0; i < tail.Length; i++)
		{
			Vector2 vector6 = Vector2.Lerp(tail[i].lastPos, tail[i].pos, timeStacker) - camPos;
			Vector2 normalized2 = (vector6 - vector3).normalized;
			Vector2 vector7 = Custom.PerpendicularVector(normalized2);
			int num3 = i * 4 + num2;
			float num4 = Vector2.Distance(vector6, vector3) / 5f;
			if (i == 0)
			{
				num4 = 0f;
			}
			Vector2 vector8 = vector7 * num + normalized2 * num4;
			origTailVerts[num3] = vector3 - vector8;
			origTailVerts[num3 + 1] = vector3 + vector8;
			origTailVerts[num3 + 2] = vector6;
			origTailVerts[num3 + 3] = vector6;
			Vector2 vector9 = vector7 * tail[i].StretchedRad - normalized2 * num4;
			if (i < 3)
			{
				origTailVerts[num3 + 2] -= vector9;
				origTailVerts[num3 + 3] += vector9;
			}
			num = tail[i].StretchedRad;
			vector3 = vector6;
		}
		for (int j = 1; j < tail.Length; j++)
		{
			int num5 = j * 4;
			Vector2 vector10 = origTailVerts[num5];
			origTailVerts[num5] = origTailVerts[num5 + 2];
			origTailVerts[num5 + 2] = vector10;
		}
	}

	public void UpdateArms()
	{
		for (int i = 0; i < arms.Length; i++)
		{
			arms[i].lastTransform = arms[i].transform;
			arms[i].lastAnimationTime = arms[i].animationTime;
			arms[i].transform = watcherMorph;
			arms[i].animationTime = animationTime - 0.25f * (float)(i / 2);
			arms[i].grow = Mathf.InverseLerp(0f, 0.05f, watcherMorph - 0.04f * (float)(i / 2));
			arms[i].bodyDir = bodyDir;
			arms[i].tailDir = tailDir;
			arms[i].scale = 1f - (float)(i / 2) * 0.2f;
		}
	}

	public void UpdateBodyMovement()
	{
		for (int i = 0; i < bodyMovement.Length; i++)
		{
			lastBodyMovement[i] = bodyMovement[i];
		}
		if (owner.graphicsModule != null)
		{
			Vector2[,] drawPositions = (owner.graphicsModule as PlayerGraphics).drawPositions;
			TailSegment[] tail = (owner.graphicsModule as PlayerGraphics).tail;
			Vector2 vector = drawPositions[0, 0];
			Vector2 vector2 = drawPositions[1, 0];
			Vector2 vector3 = tail[0].pos;
			float num = Mathf.Clamp01(0.5f + owner.mainBodyChunk.vel.magnitude * 0.5f);
			float num2 = Mathf.InverseLerp(0.3f, 0f, watcherMorph);
			float t = num2 * 0.98f + 0.02f;
			num2 = num2 * 0.95f + 0.05f;
			float b = Custom.VecToDeg(Vector2.Lerp((vector2 - vector).normalized, -owner.mainBodyChunk.vel.normalized, Mathf.Clamp01(watcherMorph * 2f) * Mathf.Min(1f, owner.mainBodyChunk.vel.magnitude)));
			bodyDir = Custom.DegToVec(Mathf.LerpAngle(Custom.VecToDeg(bodyDir), b, num2));
			tailDir = Custom.DegToVec(Mathf.LerpAngle(Custom.VecToDeg(tailDir), Custom.VecToDeg((vector3 - vector2).normalized), t));
			for (int j = 0; j < bodyMovement.Length; j++)
			{
				float num3 = (float)j / (float)bodyMovement.Length;
				Vector2 normalized = Vector2.Lerp(bodyDir, tailDir, num3).normalized;
				Vector2 vector4 = Custom.PerpendicularVector(normalized) * Mathf.InverseLerp(0.25f, 1f, watcherMorph) * (Mathf.Sin((0f - animationTime) * 10f + num3 * 8f) * width * 2f * num3 * num);
				bodyMovement[j] = owner.mainBodyChunk.pos + normalized * (((float)j - 1.25f) * segLength) + vector4;
			}
		}
	}

	public void MoveMesh(TriangleMesh mesh, Vector2 camPos, float timeStacker)
	{
		int num = origTailVerts.Length / 2;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 2;
			mesh.vertices[num2 * quadMult] = origTailVerts[num2];
			mesh.vertices[num2 * quadMult + 1] = origTailVerts[num2 + 1];
			if (debugColors)
			{
				mesh.verticeColors[num2 * quadMult] = Color.green;
				mesh.verticeColors[num2 * quadMult + 1] = Color.green;
			}
			if (i < num - 1)
			{
				for (int j = 1; j < quadMult; j++)
				{
					float t = (float)j / (float)quadMult;
					mesh.vertices[num2 * quadMult + j * 2] = Vector2.Lerp(origTailVerts[num2], origTailVerts[num2 + 2], t);
					mesh.vertices[num2 * quadMult + 1 + j * 2] = Vector2.Lerp(origTailVerts[num2 + 1], origTailVerts[num2 + 3], t);
				}
			}
		}
		float value = Mathf.Lerp(lastWatcherMorph, watcherMorph, timeStacker);
		float t2 = Mathf.InverseLerp(0f, 0.5f, value);
		for (int k = 0; k < bodyMovement.Length; k++)
		{
			float num3 = (float)k / (float)bodyMovement.Length;
			float num4 = width * (1f - num3);
			Vector2 zero = Vector2.zero;
			Vector2 vector = Vector2.Lerp(lastBodyMovement[k], bodyMovement[k], timeStacker);
			zero = ((k >= bodyMovement.Length - 1) ? (Vector2.Lerp(lastBodyMovement[k - 1], bodyMovement[k - 1], timeStacker) - vector).normalized : (vector - Vector2.Lerp(lastBodyMovement[k + 1], bodyMovement[k + 1], timeStacker)).normalized);
			Vector2 vector2 = Custom.PerpendicularVector(zero);
			int num5 = k * 2;
			mesh.vertices[num5] = Vector2.Lerp(mesh.vertices[num5], vector + vector2 * num4 - camPos, t2);
			mesh.vertices[num5 + 1] = Vector2.Lerp(mesh.vertices[num5 + 1], vector - vector2 * num4 - camPos, t2);
			if (k > 1 && k <= arms.Length && k % 2 == 0)
			{
				int num6 = k - 2;
				arms[num6].pos = mesh.vertices[num5] + camPos - vector2 * (num4 * 0.1f);
				arms[num6 + 1].pos = mesh.vertices[num5 + 1] + camPos + vector2 * (num4 * 0.1f);
				arms[num6].direction = vector2;
				arms[num6 + 1].direction = vector2;
			}
		}
	}
}
