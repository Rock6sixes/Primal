using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BigMothGraphics : GraphicsModule, IMuddableGraphics
{
	public class TongueGraphics
	{
		public DebugSprite[] segSprs;

		public BigMothGraphics owner;

		public int index;

		public Vector2[,] segments;

		public float width;

		public float outness;

		public float foldness = 1f;

		public int suckCounter;

		public float glugSize;

		public int tongueOverCounter;

		public TongueGraphics(BigMothGraphics owner)
		{
			this.owner = owner;
			segments = new Vector2[owner.bug.Small ? 6 : 12, 3];
			segSprs = new DebugSprite[segments.GetLength(0)];
		}

		public void Update()
		{
			glugSize = Custom.LerpAndTick(glugSize, (!owner.bug.drinkingChunk) ? 0f : ((owner.bug.drinkChunk.owner is Creature { State: { meatLeft: >0 } }) ? 2.8f : 1f), 0.03f, 0.004f);
			outness = Custom.LerpAndTick(outness, (owner.bug.AI.behavior == BigMothAI.Behavior.Eat || owner.bug.drinkChunk != null) ? 1f : 0f, 0.008f, 0.001f);
			foldness = Custom.LerpAndTick(foldness, (owner.bug.drinkChunk != null) ? 0f : 1f, 0.008f, 0.001f);
			float num = 10f;
			segments[0, 1] = segments[0, 0];
			Vector2 vector = owner.headPos + owner.headDir * (23f * owner.scale);
			segments[0, 0] = vector;
			num *= outness * owner.scale;
			Vector2 vector2 = Custom.PerpendicularVector(owner.headDir);
			Vector2 normalized = (owner.headDir + vector2 * (owner.faceFlip * foldness * 0.7f * outness)).normalized;
			vector2 = Custom.PerpendicularVector(normalized);
			Vector2 zero = Vector2.zero;
			Vector2 vector3 = Vector2.right;
			for (int i = 1; i < segments.GetLength(0); i++)
			{
				float num2 = (float)i / (float)(segments.GetLength(0) - 1) * outness;
				float num3 = 1f - num2;
				segments[i, 1] = segments[i, 0];
				segments[i, 0] += segments[i, 2];
				segments[i, 2].y -= owner.bug.gravity * (num2 * 0.5f + 0.5f);
				segments[i, 2] *= 0.95f;
				float num4 = Vector2.Distance(segments[i - 1, 0], segments[i, 0]);
				Vector2 vector4 = Custom.DirVec(segments[i - 1, 0], segments[i, 0]);
				if (num4 > num)
				{
					Vector2 vector5 = vector4 * (num - num4);
					segments[i, 0] = segments[i - 1, 0] + vector4 * num;
					segments[i, 2] += vector5 * 0.3f;
				}
				Vector2 vector6 = vector3;
				Vector2 vector7 = Custom.PerpendicularVector(vector6);
				vector6 = (vector6 * (1f - (num2 + 0.4f) * foldness * outness) + vector7 * ((num2 + 0.4f) * foldness * outness)).normalized * (num3 * 0.8f + 0.2f);
				Vector2 vector8 = (vector + (normalized * (vector6.x + zero.x) - vector2 * ((vector6.y + zero.y) * owner.faceFlip)) * num - segments[i, 0]) * (0.25f * foldness);
				segments[i, 0] += vector8;
				segments[i, 2] += vector8;
				segments[i, 2] += owner.bug.bodyChunks[0].vel * (num3 * 0.1f);
				segments[i, 2] += owner.headDir * ((num3 * 0.7f + 0.2f) * (1f - foldness));
				zero += vector6;
				vector3 = vector6;
			}
			segments[0, 0] = vector;
			if (owner.bug.drinkChunk != null && owner.bug.drinkChunk.owner.room == owner.bug.room)
			{
				if (!owner.bug.drinkingChunk && outness > 0.5f && foldness < 0.9f)
				{
					float num5 = Vector2.Distance(segments[segments.GetLength(0) - 1, 0], owner.bug.drinkChunk.pos);
					Vector2 vector9 = Custom.DirVec(segments[segments.GetLength(0) - 1, 0], owner.bug.drinkChunk.pos);
					for (int j = 1; j < segments.GetLength(0); j++)
					{
						float num6 = Mathf.InverseLerp(0f, segments.GetLength(0) - 1, j);
						segments[j, 2] += vector9 * (num5 * 0.15f * Mathf.Pow(num6 * (1f - foldness), 2f));
					}
					bool flag = false;
					if (num5 < owner.bug.drinkChunk.rad + 10f)
					{
						flag = true;
						if (tongueOverCounter >= 30)
						{
							owner.bug.DrinkChunk();
						}
					}
					tongueOverCounter = Mathf.Clamp(tongueOverCounter + (flag ? 2 : (-1)), 0, 30);
				}
				if (owner.bug.drinkingChunk)
				{
					tongueOverCounter = 0;
					suckCounter += (owner.bug.Consious ? ((!(owner.bug.drinkChunk.owner is MothGrub)) ? 1 : (-1)) : 0);
					Vector2 pos = owner.bug.drinkChunk.pos;
					float num7 = Vector2.Distance(vector, pos);
					segments[segments.GetLength(0) - 1, 0] = pos;
					for (int k = 1; k < segments.GetLength(0) - 1; k++)
					{
						float num8 = Mathf.InverseLerp(0f, segments.GetLength(0) - 1, k);
						Vector2 vector10 = Vector2.Lerp(vector, pos, num8);
						Vector2 vector11 = (vector10 - segments[k, 0]) * num8;
						segments[k, 0] = Vector2.Lerp(segments[k, 0], vector10, num8 * 0.7f + num7 / (400f * owner.scale));
						segments[k, 2] += vector11 * 0.2f;
					}
				}
			}
			else
			{
				tongueOverCounter = 0;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			int tongueSprite = owner.TongueSprite;
			sLeaser.sprites[tongueSprite] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: true);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			int tongueSprite = owner.TongueSprite;
			float b = 0.25f;
			float num = Mathf.Max(1.2f * outness, b);
			Vector2 vector = Vector2.Lerp(segments[segments.GetLength(0) - 1, 1], segments[segments.GetLength(0) - 1, 0], timeStacker);
			vector += Custom.DirVec(Vector2.Lerp(segments[segments.GetLength(0) - 2, 1], segments[segments.GetLength(0) - 2, 0], timeStacker), vector) * 1.5f;
			float num2 = num;
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				int num3 = segments.GetLength(0) - 1 - i;
				float num4 = 1f - Mathf.InverseLerp(0f, segments.GetLength(0) - 1, i) * outness;
				float num5 = Mathf.Lerp(num, b, num4) * owner.scale;
				num5 += Mathf.Max(Mathf.Sin((float)suckCounter * 0.15f + num4 * 12f), 0f) * 0.5f * owner.scale * glugSize;
				Vector2 vector2 = Vector2.Lerp(segments[num3, 1], segments[num3, 0], timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num6 = Vector2.Distance(vector2, vector) / 5f;
				Vector2 vector4 = vector3 * num5;
				if (i == 0)
				{
					(sLeaser.sprites[tongueSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector4 - camPos);
					(sLeaser.sprites[tongueSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector4 - camPos);
				}
				else
				{
					Vector2 vector5 = vector3 * ((num5 + num2) * 0.5f);
					(sLeaser.sprites[tongueSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector5 + normalized * num6 - camPos);
					(sLeaser.sprites[tongueSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector5 + normalized * num6 - camPos);
				}
				(sLeaser.sprites[tongueSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector4 - normalized * num6 - camPos);
				(sLeaser.sprites[tongueSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector4 - normalized * num6 - camPos);
				if (!owner.shadowMode)
				{
					Color color = Color.Lerp(owner.tongueBaseColor, owner.tongueTipColor, num4);
					(sLeaser.sprites[tongueSprite] as TriangleMesh).verticeColors[i * 4] = color;
					(sLeaser.sprites[tongueSprite] as TriangleMesh).verticeColors[i * 4 + 1] = color;
					(sLeaser.sprites[tongueSprite] as TriangleMesh).verticeColors[i * 4 + 2] = color;
					(sLeaser.sprites[tongueSprite] as TriangleMesh).verticeColors[i * 4 + 3] = color;
				}
				vector = vector2;
				num2 = num5;
			}
		}
	}

	public class AntennaGraphics
	{
		public BigMothGraphics owner;

		public int index;

		public Vector2[,] segments;

		public AntennaGraphics(BigMothGraphics owner, int index)
		{
			this.owner = owner;
			this.index = index;
			segments = new Vector2[owner.bug.iVars.antennaLength, 3];
		}

		public void Update()
		{
			float num = 10f;
			segments[0, 1] = segments[0, 0];
			Vector2 vector = owner.headPos + (owner.headDir * 4.5f + Custom.PerpendicularVector(owner.headDir) * ((float)((index == 0) ? (-4) : 4) * (0.3f + (1f - Mathf.Abs(owner.faceFlip) * 0.7f)))) * owner.scale;
			segments[0, 0] = vector;
			float num2 = ((index == 0 == (double)owner.faceFlip > 0.5) ? (0.5f - Mathf.Abs(owner.faceFlip) * 0.1f) : (0.5f + Mathf.Abs(owner.faceFlip) * 0.1f));
			Vector2 vector2 = Custom.PerpendicularVector(owner.headDir) * (((index == 0) ? 1f : (-1f)) * (0.05f + owner.bug.abstractCreature.personality.sympathy * 0.4f));
			for (int i = 1; i < segments.GetLength(0); i++)
			{
				float num3 = 1f - Mathf.InverseLerp(0f, segments.GetLength(0) - 1, i);
				segments[i, 1] = segments[i, 0];
				segments[i, 0] += segments[i, 2];
				segments[i, 2].y -= owner.bug.gravity * 0.07f * (1f - num3);
				segments[i, 2] += owner.bug.room.windDirection * ((0.3f + UnityEngine.Random.value * 0.7f) * 0.005f * (1f - num3));
				segments[i, 2] *= 0.9f;
				float num4 = Vector2.Distance(segments[i - 1, 0], segments[i, 0]);
				Vector2 vector3 = Custom.DirVec(segments[i - 1, 0], segments[i, 0]) * ((num - num4) * num2);
				segments[i, 0] += vector3;
				segments[i, 2] += vector3;
				segments[i - 1, 0] -= vector3;
				segments[i - 1, 2] -= vector3;
				segments[i, 2] += owner.bug.bodyChunks[0].vel * (num3 * 0.1f);
				segments[i, 2] -= owner.headDir * (num3 * 0.5f + 0.2f) + vector2;
			}
			segments[0, 0] = vector;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			int num = owner.AntennaSprites(index);
			sLeaser.sprites[num] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: false);
			sLeaser.sprites[num + 1] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: false);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			int num = owner.AntennaSprites((owner.faceFlip < 0f) ? index : (1 - index));
			Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
			float num2 = Mathf.Lerp(owner.lastFaceFlip, owner.faceFlip, timeStacker);
			float num3 = 1f - 0.24f * num2;
			float num4 = 1f + 0.24f * num2;
			float num5 = 0f;
			float num6 = 0f;
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				float num7 = Mathf.InverseLerp(0f, segments.GetLength(0) - 1, i);
				float num8 = Mathf.Lerp(1.5f, 0.5f, num7) * owner.scale;
				float num9 = Mathf.Sin(num7 * (float)Math.PI) * owner.bug.iVars.antennaWidth + num8;
				Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num10 = Vector2.Distance(vector2, vector) / 5f;
				if (i == 0)
				{
					(sLeaser.sprites[num] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * ((num9 + num6) * 0.5f * num3) - camPos);
					(sLeaser.sprites[num] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * ((num9 + num6) * 0.5f * num4) - camPos);
					(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * ((num8 + num5) * 0.5f * num3) - camPos);
					(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * ((num8 + num5) * 0.5f * num4) - camPos);
				}
				else
				{
					(sLeaser.sprites[num] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * ((num9 + num6) * 0.5f * num3) + normalized * num10 - camPos);
					(sLeaser.sprites[num] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * ((num9 + num6) * 0.5f * num4) + normalized * num10 - camPos);
					(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * ((num8 + num5) * 0.5f * num3) + normalized * num10 - camPos);
					(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * ((num8 + num5) * 0.5f * num4) + normalized * num10 - camPos);
				}
				(sLeaser.sprites[num] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * (num9 * num3) - normalized * num10 - camPos);
				(sLeaser.sprites[num] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * (num9 * num4) - normalized * num10 - camPos);
				(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * (num8 * num3) - normalized * num10 - camPos);
				(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * (num8 * num4) - normalized * num10 - camPos);
				vector = vector2;
				num5 = num8;
				num6 = num9;
			}
		}
	}

	public class WingGraphics
	{
		public int firstSpriteOnLayer;

		public bool inFront;

		public bool initiallyInFront;

		public BigMothGraphics owner;

		public BigMoth.MothWingPair pair;

		public int index;

		public Vector2 tipPos;

		public Vector2[,] segments;

		public float spreadOutness;

		public float scale => owner.bug.scale;

		public WingGraphics(BigMothGraphics owner, BigMoth.MothWingPair pair, int index, int firstSpriteOnLayer, bool inFront)
		{
			this.owner = owner;
			this.pair = pair;
			this.index = index;
			this.firstSpriteOnLayer = firstSpriteOnLayer;
			this.inFront = inFront;
			initiallyInFront = inFront;
			segments = new Vector2[owner.bug.Small ? 6 : 8, 3];
		}

		public void Update()
		{
			inFront = ((owner.bug.flip > 0f) ? initiallyInFront : (!initiallyInFront));
			Vector2 vector = Custom.DirVec(owner.bug.bodyChunks[2].pos, owner.bug.bodyChunks[1].pos);
			Vector2 vector2 = Custom.PerpendicularVector(vector);
			float num = 1f;
			if (!inFront)
			{
				num = 1f - Mathf.Abs(owner.bug.flip) * 1.5f;
			}
			Vector2 vector3 = owner.bug.bodyChunks[1].pos + (vector * ((pair.pairIndex == 0) ? (-5f) : 5f) + vector2 * (((index == 0) ? 1f : (-1f)) * ((pair.pairIndex == 0) ? 12f : 7f) * num)) * scale;
			Vector2 vector4 = Custom.DegToVec((pair.flapness * 2f - 1f) * 70f * ((index == 0) ? 1f : (-1f)) + ((index == 0) ? (-90f) : 90f) + owner.bug.moveDirection.x * 40f * ((pair.pairIndex == 1) ? 1f : 0.7f) * (Mathf.Abs(owner.bug.flip) * 0.5f + 0.5f));
			spreadOutness = ((index == 0) ? (-0.7f) : 0.7f);
			if (!inFront)
			{
				spreadOutness *= Mathf.Lerp(1f, 1f - Mathf.Abs(owner.bug.flip) * 2f, owner.wingStashness);
			}
			float num2 = ((pair.pairIndex == 0) ? 110f : 140f) * scale;
			tipPos = vector3 + vector4 * num2;
			Vector2 a = vector3 + (-vector * ((pair.pairIndex == 0) ? 1.4f : 1.2f) + vector2 * ((index == 0) ? 1f : (-1f))).normalized * (num2 * 0.8f);
			Vector2 a2 = Vector2.Lerp(vector3, tipPos, 0.3f) + vector2 * (((index == 0) ? 30f : (-30f)) * scale);
			Vector2 a3 = Vector2.Lerp(vector3, tipPos, 0.7f) + vector2 * (((index == 0) ? 10f : (-10f)) * scale);
			Vector2 vector5 = Custom.DirVec(vector3, owner.bug.bodyChunks[4].pos);
			Vector2 vector6 = Custom.PerpendicularVector(vector5);
			Vector2 b = vector3 + ((vector5 - vector * 0.4f).normalized - vector6 * (owner.bug.flip * 0.5f)).normalized * num2;
			Vector2 b2 = Vector2.Lerp(vector3, owner.bug.bodyChunks[2].pos, 0.8f) + vector2 * (((index == 0) ? 40f : (-40f)) * (inFront ? 1f : (-0.8f)) * scale);
			Vector2 b3 = Vector2.Lerp(vector3, owner.bug.bodyChunks[4].pos, 0.7f) + vector5 * (((index == 0) ? 20f : (-20f)) * (inFront ? 1f : (-0.8f)) * scale);
			tipPos = Vector2.Lerp(tipPos, Vector2.Lerp(a, b, Mathf.Abs(owner.bug.flip)), owner.wingStashness);
			Vector2 cA = Vector2.Lerp(a2, b2, Mathf.Abs(owner.bug.flip));
			Vector2 cB = Vector2.Lerp(a3, b3, Mathf.Abs(owner.bug.flip));
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 1] = segments[i, 0];
				segments[i, 0] += segments[i, 2];
				segments[i, 2] += owner.bug.room.windDirection * ((0.3f + UnityEngine.Random.value * UnityEngine.Random.value * 0.7f) * 0.027f);
				segments[i, 2] *= 0.9f;
				float num3 = Mathf.InverseLerp(0f, segments.GetLength(0), i + 1);
				Vector2 b4 = Custom.Bezier(vector3, cA, tipPos, cB, num3);
				Vector2 vector7 = (Vector2.Lerp(Vector2.Lerp(vector3, tipPos, num3), b4, owner.wingStashness) - segments[i, 0]) * (0.25f * (1f - num3 * 0.8f));
				segments[i, 0] += vector7;
				segments[i, 2] += vector7;
			}
			float num4 = num2 / (float)segments.GetLength(0);
			for (int j = 1; j < segments.GetLength(0); j++)
			{
				Vector2 vector8 = Custom.DirVec(segments[j - 1, 0], segments[j, 0]);
				if (Vector2.Distance(segments[j - 1, 0], segments[j, 0]) > num4)
				{
					segments[j, 0] = segments[j - 1, 0] + vector8 * num4;
				}
			}
			segments[0, 0] = vector3;
			segments[1, 0] = Vector2.Lerp(segments[1, 0], vector3, 0.5f);
		}

		public float WingRadiusAlong(float f)
		{
			return Mathf.Lerp(0.25f, 15f, Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow((1f - f) * 0.95f, 2f) * (float)Math.PI)), 0.5f)) * scale;
		}

		public Vector2 WingPositionAlong(float f, float x, float timeStacker)
		{
			float num = f * (float)(segments.GetLength(0) - 1);
			int num2 = Mathf.Min(Mathf.FloorToInt(num), segments.GetLength(0) - 2);
			Vector2 vector = Vector2.Lerp(segments[num2, 1], segments[num2, 0], timeStacker);
			Vector2 vector2 = Vector2.Lerp(segments[num2 + 1, 1], segments[num2 + 1, 0], timeStacker);
			return Vector2.Lerp(vector, vector2, num - (float)num2) + Custom.PerpendicularVector(vector - vector2) * WingRadiusAlong(f) * x * spreadOutness;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			int num = (initiallyInFront ? owner.FirstInFrontWingSprite : owner.FirstBehindWingSprite) + firstSpriteOnLayer;
			sLeaser.sprites[num] = new FSprite("Circle20");
			sLeaser.sprites[num].scaleY = 0.5f * scale;
			sLeaser.sprites[num].scaleX = scale;
			sLeaser.sprites[num + 1] = TriangleMesh.MakeLongMesh(segments.GetLength(0) / 2, pointyTip: false, customColor: true);
			sLeaser.sprites[num + 1].shader = rCam.room.game.rainWorld.Shaders["MothWing"];
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			int num = (inFront ? owner.FirstInFrontWingSprite : owner.FirstBehindWingSprite) + firstSpriteOnLayer;
			Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
			Vector2 vector2 = Custom.DirVec(Vector2.Lerp(segments[1, 1], segments[1, 0], timeStacker), vector);
			Vector2 v = vector + vector2 * (10f * scale);
			float num2 = ((index != 1) ? 1 : (-1));
			Vector2 vector3 = vector + Custom.PerpendicularVector(vector2) * (WingRadiusAlong(0f) * spreadOutness);
			sLeaser.sprites[num].x = vector3.x - camPos.x;
			sLeaser.sprites[num].y = vector3.y - camPos.y;
			sLeaser.sprites[num].rotation = Custom.VecToDeg(vector2);
			float a = 0.5f + spreadOutness * num2 * -1f * 0.5f;
			float t = Mathf.Abs(owner.bug.flip) * (0.2f + owner.wingStashness * 0.8f);
			float num3 = 0f;
			float num4 = 0f;
			if (pair.pairIndex == 0)
			{
				if (inFront)
				{
					Mathf.Lerp(0.2f, 0.3f, t);
					num3 = Mathf.Lerp(0.2f, 0.15f, t);
					num4 = Mathf.Lerp(0.5f, 0.2f, t);
				}
				else
				{
					Mathf.Lerp(0.2f, 0.95f, t);
					num3 = Mathf.Lerp(0.2f, 0.4f, t);
					num4 = Mathf.Lerp(0.5f, 0.8f, t);
				}
			}
			else if (inFront)
			{
				Mathf.Lerp(0.05f, 0.1f, t);
				num3 = Mathf.Lerp(0.1f, 0.1f, t);
				num4 = Mathf.Lerp(0.3f, 0f, t);
			}
			else
			{
				Mathf.Lerp(0.05f, 0.8f, t);
				num3 = Mathf.Lerp(0.1f, 0.7f, t);
				num4 = Mathf.Lerp(0.3f, 0.95f, t);
			}
			sLeaser.sprites[num].color = owner.BodyColorWithDepth(num3);
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				float num5 = Mathf.InverseLerp(0f, segments.GetLength(0) - 1, i);
				float depth = Mathf.Lerp(num3, num4, num5);
				Vector2 vector4 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
				Vector2 vector5 = Custom.PerpendicularVector(vector4, v) * num2;
				float num6 = WingRadiusAlong(num5);
				float num7 = spreadOutness * num2;
				(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(i * 2, vector4 - vector5 * (num6 * (1f + num7) * 1.5f) - camPos);
				(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(i * 2 + 1, vector4 + vector5 * (num6 * (1f - num7) * 1.5f) - camPos);
				float num8 = Mathf.Sin(num5 * (float)Math.PI * 0.5f);
				(sLeaser.sprites[num + 1] as TriangleMesh).verticeColors[i * 2] = SetColorAlpha(owner.BodyColorWithDepth(depth, num8 * spreadOutness * num2), a);
				(sLeaser.sprites[num + 1] as TriangleMesh).verticeColors[i * 2 + 1] = SetColorAlpha(owner.BodyColorWithDepth(depth, num8 * (0f - spreadOutness) * num2), a);
				v = vector4;
			}
		}

		public Color SetColorAlpha(Color c, float a)
		{
			return new Color(c.r, c.g, c.b, a);
		}
	}

	public class LegGraphics
	{
		public int firstSpriteOnLayer;

		public bool inFront;

		public bool initiallyInFront;

		public BigMothGraphics owner;

		public BigMoth.MothLeg leg;

		public Vector2[,] segments;

		public float[] legSegmentLengths = new float[3] { 30f, 34f, 30f };

		public float scale => owner.bug.scale;

		public LegGraphics(BigMothGraphics owner, BigMoth.MothLeg leg, int firstSpriteOnLayer, bool inFront)
		{
			this.owner = owner;
			this.leg = leg;
			this.firstSpriteOnLayer = firstSpriteOnLayer;
			this.inFront = inFront;
			initiallyInFront = inFront;
			segments = new Vector2[4, 2];
			if (leg.pairIndex == 0)
			{
				legSegmentLengths = new float[3] { 24f, 24f, 22f };
			}
			for (int i = 0; i < legSegmentLengths.Length; i++)
			{
				legSegmentLengths[i] *= scale;
			}
		}

		public void Update()
		{
			inFront = ((owner.bug.flip > 0f) ? initiallyInFront : (!initiallyInFront));
			bool flag = leg.pairIndex == 0;
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 1] = segments[i, 0];
			}
			segments[0, 0] = leg.connectionPos;
			segments[segments.GetLength(0) - 1, 1] = segments[segments.GetLength(0) - 1, 0];
			segments[segments.GetLength(0) - 1, 0] = leg.footPos;
			Vector2 vector = Custom.PerpendicularVector(Custom.DirVec(segments[0, 0], segments[3, 0])) * (owner.bug.flip * 8f * (flag ? (-1f) : 1f) * scale);
			segments[1, 0] -= vector;
			segments[2, 0] += vector;
			for (int num = segments.GetLength(0) - 1; num >= 1; num--)
			{
				segments[num, 0] += owner.bug.bodyChunks[leg.connectedChunk].vel * (3 - num) / 3f;
				Vector2 vector2 = Custom.DirVec(segments[num - 1, 0], segments[num, 0]);
				float num2 = Vector2.Distance(segments[num - 1, 0], segments[num, 0]);
				Vector2 vector3 = vector2 * ((legSegmentLengths[num - 1] - num2) * 0.5f);
				segments[num, 0] += vector3;
				segments[num - 1, 0] -= vector3;
			}
			segments[0, 0] = leg.connectionPos;
			for (int j = 1; j < segments.GetLength(0) - 1; j++)
			{
				segments[j, 0] = segments[j - 1, 0] + Custom.DirVec(segments[j - 1, 0], segments[j, 0]) * legSegmentLengths[j - 1];
			}
			segments[segments.GetLength(0) - 1, 0] = leg.footPos;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			int num = (initiallyInFront ? owner.FirstInFrontLegSprite : owner.FirstBehindLegSprite) + firstSpriteOnLayer;
			sLeaser.sprites[num] = new FSprite("Circle20");
			sLeaser.sprites[num].scale = 0.25f * scale;
			int num2 = (owner.bug.Small ? 3 : 7);
			sLeaser.sprites[num + 1] = TriangleMesh.MakeLongMesh(num2, pointyTip: false, customColor: true);
			sLeaser.sprites[num + 2] = TriangleMesh.MakeLongMesh(num2, pointyTip: false, customColor: true);
			sLeaser.sprites[num + 3] = new FSprite("pixel");
			sLeaser.sprites[num + 3].scaleX = 9.5f * scale;
			sLeaser.sprites[num + 3].scaleY = 6f * scale;
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			int num = (inFront ? owner.FirstInFrontLegSprite : owner.FirstBehindLegSprite) + firstSpriteOnLayer;
			Vector2[] array = new Vector2[segments.GetLength(0)];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
			}
			sLeaser.sprites[num].x = array[2].x - camPos.x;
			sLeaser.sprites[num].y = array[2].y - camPos.y;
			float t = Mathf.Abs(owner.bug.flip);
			float num2 = 0f;
			num2 = ((leg.pairIndex == 0) ? ((!inFront) ? Mathf.Lerp(0.25f, 0.6f, t) : Mathf.Lerp(0.15f, 0.2f, t)) : ((!inFront) ? Mathf.Lerp(0.2f, 0.5f, t) : Mathf.Lerp(0.1f, 0.15f, t)));
			Color color = owner.BodyColorWithDepth(num2);
			Color color2 = owner.BodyColorWithDepth(num2, owner.bug.iVars.kneeColorness * 0.25f);
			sLeaser.sprites[num].color = color2;
			float num3 = Vector2.Distance(array[2], array[3]);
			Vector2 vector = Vector2.Lerp(array[2], array[3], 0.7f) + Custom.PerpendicularVector(Custom.DirVec(array[2], array[3])) * (owner.bug.flip * Mathf.Min(num3 - legSegmentLengths[2] * 3.2f, 0f) * 0.44f * scale);
			float num4 = 0f;
			Vector2 vector2 = array[3] + Custom.DirVec(array[3], array[2]) * scale;
			int num5 = (owner.bug.Small ? 3 : 7);
			for (int j = 0; j < num5; j++)
			{
				float num6 = 1f - Mathf.InverseLerp(0f, num5 - 1, j);
				Vector2 vector3 = Vector2.Lerp(Vector2.Lerp(array[2], vector, num6), Vector2.Lerp(vector, array[3], num6), num6);
				Vector2 vector4 = Custom.PerpendicularVector(vector3, vector2);
				float num7 = (Mathf.Lerp(2.5f, 0.5f, num6) + Mathf.Max(1f - Mathf.Pow(num6 * 4f - 1f, 2f), 0f) * 0.7f * (1f + owner.bug.iVars.legPuff * 1.8f)) * scale;
				Vector2 vector5 = vector4 * ((num7 + num4) * 0.5f);
				Vector2 vector6 = vector4 * num7;
				(sLeaser.sprites[num + 2] as TriangleMesh).MoveVertice(j * 4, (vector2 + vector3) / 2f - vector5 - camPos);
				(sLeaser.sprites[num + 2] as TriangleMesh).MoveVertice(j * 4 + 1, (vector2 + vector3) / 2f + vector5 - camPos);
				(sLeaser.sprites[num + 2] as TriangleMesh).MoveVertice(j * 4 + 2, vector3 - vector6 - camPos);
				(sLeaser.sprites[num + 2] as TriangleMesh).MoveVertice(j * 4 + 3, vector3 + vector6 - camPos);
				Color color3 = Color.Lerp(color2, color, num6);
				(sLeaser.sprites[num + 2] as TriangleMesh).verticeColors[j * 4] = color3;
				(sLeaser.sprites[num + 2] as TriangleMesh).verticeColors[j * 4 + 1] = color3;
				(sLeaser.sprites[num + 2] as TriangleMesh).verticeColors[j * 4 + 2] = color3;
				(sLeaser.sprites[num + 2] as TriangleMesh).verticeColors[j * 4 + 3] = color3;
				vector2 = vector3;
				num4 = num7;
			}
			num4 = 0f;
			vector2 = array[2] + Custom.DirVec(array[1], array[2]) * scale;
			for (int k = 0; k < num5; k++)
			{
				float num8 = 1f - Mathf.InverseLerp(0f, num5 - 1, k);
				Vector2 vector7 = Custom.Bezier(array[0], array[1], array[2], array[1], num8);
				Vector2 vector8 = Custom.PerpendicularVector(vector7, vector2);
				float num9 = (Mathf.Max(1f - Mathf.Pow(num8 * 4f - 1f, 2f), 0f) * 0.7f + Mathf.Max(1f - Mathf.Pow(num8 * 4f - 3f, 2f), 0f) * 0.95f * (1f + owner.bug.iVars.legPuff * 2.5f) + Mathf.Lerp(5f, 2.5f, num8)) * scale;
				Vector2 vector9 = vector8 * ((num9 + num4) * 0.5f);
				Vector2 vector10 = vector8 * num9;
				(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(k * 4, (vector2 + vector7) / 2f - vector9 - camPos);
				(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(k * 4 + 1, (vector2 + vector7) / 2f + vector9 - camPos);
				(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(k * 4 + 2, vector7 - vector10 - camPos);
				(sLeaser.sprites[num + 1] as TriangleMesh).MoveVertice(k * 4 + 3, vector7 + vector10 - camPos);
				Color color4 = Color.Lerp(color, color2, num8);
				(sLeaser.sprites[num + 1] as TriangleMesh).verticeColors[k * 4] = color4;
				(sLeaser.sprites[num + 1] as TriangleMesh).verticeColors[k * 4 + 1] = color4;
				(sLeaser.sprites[num + 1] as TriangleMesh).verticeColors[k * 4 + 2] = color4;
				(sLeaser.sprites[num + 1] as TriangleMesh).verticeColors[k * 4 + 3] = color4;
				vector2 = vector7;
				num4 = num9;
			}
			if (leg.pairIndex * 2 + leg.index == owner.bug.iVars.taggedLeg)
			{
				sLeaser.sprites[num + 3].isVisible = true;
				Vector2 vector11 = Vector2.Lerp(array[1], array[2], 0.7f);
				sLeaser.sprites[num + 3].x = vector11.x - camPos.x;
				sLeaser.sprites[num + 3].y = vector11.y - camPos.y;
				sLeaser.sprites[num + 3].rotation = Custom.VecToDeg(Custom.DirVec(array[1], array[2]));
				Color legTagColor = owner.bug.iVars.legTagColor;
				Color b = Color.Lerp(Color.Lerp(legTagColor, owner.depthColor, 0.2f), owner.palette.fogColor, 0.7f);
				sLeaser.sprites[num + 3].color = (owner.shadowMode ? new Color(0.003921569f, 0f, 0f) : Color.Lerp(legTagColor, b, num2));
				if ((owner.bug.abstractCreature.state as BigMoth.BigMothState).tagOwner == BigMoth.BigMothState.TagOwner.Slugcat)
				{
					sLeaser.sprites[num + 3].scaleY = (2.25f + (owner.bug.abstractCreature.state as BigMoth.BigMothState).babyness * 3f) * scale;
				}
			}
			else
			{
				sLeaser.sprites[num + 3].isVisible = false;
			}
		}
	}

	public BigMoth bug;

	public Vector2 headPos;

	public Vector2 lastHeadPos;

	public Vector2 headDir;

	public Vector2 lastHeadDir;

	public float faceFlip;

	public float lastFaceFlip;

	public Color bodyColor;

	public Color depthColor;

	public Color tongueBaseColor;

	public Color tongueTipColor;

	public float eyeOpenness;

	public float lastEyeOpenness;

	public int blinkCounter;

	public float wingStashness = 0.5f;

	public TongueGraphics tongue;

	public AntennaGraphics[] antennas;

	public LegGraphics[,] legs;

	public WingGraphics[] wings;

	public bool shadowMode;

	public bool spritesInShadowMode;

	public RoomPalette palette;

	public float scale => bug.scale;

	public int FirstBehindWingSprite => 0;

	public int FirstBehindLegSprite => 4;

	public int AbdomenMesh => 12;

	public int BodyMesh => AbdomenMesh + 1;

	public int FirstInFrontLegSprite => AbdomenMesh + 2;

	public int FirstInFrontWingSprite => FirstInFrontLegSprite + 8;

	public int TongueSprite => FirstInFrontWingSprite + 4;

	public int HeadMesh => TongueSprite + 1 + antennas.Length * 2 + 1;

	public int TotalSprites => 30 + antennas.Length * 2;

	public override bool ShouldBeCulled
	{
		get
		{
			if (base.ShouldBeCulled)
			{
				if (base.owner.room != null)
				{
					return base.owner.firstChunk.pos.y < base.owner.room.PixelHeight;
				}
				return true;
			}
			return false;
		}
	}

	public int AntennaSprites(int antennae)
	{
		return HeadMesh - (1 + antennas.Length * 2) + antennae * 2;
	}

	public int EyeSprite(int eye)
	{
		return HeadMesh - 1 + eye * 2;
	}

	public BigMothGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bug = ow as BigMoth;
		tongue = new TongueGraphics(this);
		antennas = new AntennaGraphics[2];
		for (int i = 0; i < antennas.Length; i++)
		{
			antennas[i] = new AntennaGraphics(this, i);
		}
		legs = new LegGraphics[2, 2];
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				legs[j, k] = new LegGraphics(this, bug.legs[j, k], (j != 0) ? 4 : 0, k == 0);
			}
		}
		wings = new WingGraphics[bug.wingPairs.Length * 2];
		for (int l = 0; l < wings.Length; l++)
		{
			wings[l] = new WingGraphics(this, bug.wingPairs[Mathf.FloorToInt(l / 2)], l % 2, Mathf.FloorToInt(l / 2) * 2, l % 2 == 0);
		}
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < antennas.Length; i++)
		{
			for (int j = 0; j < antennas[i].segments.GetLength(0); j++)
			{
				antennas[i].segments[j, 0] = bug.bodyChunks[0].pos + new Vector2(0f, j * 2);
				antennas[i].segments[j, 1] = antennas[i].segments[j, 0];
				antennas[i].segments[j, 2] = Vector2.zero;
			}
		}
		for (int k = 0; k < legs.GetLength(0); k++)
		{
			for (int l = 0; l < legs.GetLength(1); l++)
			{
				for (int m = 0; m < legs[k, l].segments.GetLength(0); m++)
				{
					legs[k, l].segments[m, 0] = bug.bodyChunks[legs[k, l].leg.connectedChunk].pos + new Vector2(-5 + k * 5, -5 + l * 5 - m * 2);
					legs[k, l].segments[m, 1] = legs[k, l].segments[m, 0];
				}
			}
		}
		for (int n = 0; n < wings.Length; n++)
		{
			for (int num = 0; num < wings[n].segments.GetLength(0); num++)
			{
				wings[n].segments[num, 0] = bug.bodyChunks[1].pos + new Vector2(0f, -num * 2);
				wings[n].segments[num, 1] = wings[n].segments[num, 0];
				wings[n].segments[num, 2] = Vector2.zero;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (culled)
		{
			return;
		}
		lastHeadPos = headPos;
		lastHeadDir = headDir;
		lastEyeOpenness = eyeOpenness;
		lastFaceFlip = faceFlip;
		if (blinkCounter > 0)
		{
			blinkCounter--;
		}
		else if (UnityEngine.Random.value < ((bug.AI.behavior == BigMothAI.Behavior.Attack) ? 0.02f : 0.01f))
		{
			if (blinkCounter <= 0)
			{
				float pitch = (bug.Small ? 1.3f : 1f);
				if (bug.AI.behavior == BigMothAI.Behavior.Attack)
				{
					if (UnityEngine.Random.value < 0.8f)
					{
						bug.room.PlaySound(WatcherEnums.WatcherSoundID.Moth_Chatter_Loud, bug.firstChunk.pos, 0.33f, pitch);
						bug.shake = 1f;
					}
				}
				else if (bug.AI.behavior == BigMothAI.Behavior.Eat)
				{
					if (UnityEngine.Random.value < 0.3f)
					{
						bug.room.PlaySound(WatcherEnums.WatcherSoundID.Moth_Clicking, bug.firstChunk.pos, 0.22f, pitch);
						bug.shake = 0.7f;
					}
				}
				else if (UnityEngine.Random.value < 0.4f)
				{
					bug.room.PlaySound(WatcherEnums.WatcherSoundID.Moth_Chatter, bug.firstChunk.pos, 0.3f, pitch);
					bug.shake = 0.3f;
				}
			}
			blinkCounter = ((bug.AI.behavior == BigMothAI.Behavior.Attack) ? 20 : 10);
		}
		if (!bug.Consious)
		{
			blinkCounter = 10;
		}
		eyeOpenness = Mathf.MoveTowards(eyeOpenness, (blinkCounter > 0) ? 0.4f : 1f, 0.3f);
		wingStashness = Mathf.MoveTowards(wingStashness, bug.wantToFly ? 0f : 1f, bug.wantToFly ? 0.1f : 0.025f);
		Vector2 lookPos = bug.lookPos;
		Vector2 vector = Custom.DirVec(bug.bodyChunks[1].pos, bug.bodyChunks[0].pos);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		Vector2 normalized = (Vector2.down * 1.9f + new Vector2(bug.flip, 0f) - vector2 * bug.flip).normalized;
		normalized = Vector2.Lerp(normalized, Custom.DirVec(bug.bodyChunks[0].pos, lookPos), Mathf.Min(Vector2.Distance(bug.bodyChunks[0].pos, lookPos) / 700f, 0.8f)).normalized;
		if ((double)Vector2.Dot(normalized, headDir) > 0.4)
		{
			headDir = normalized;
		}
		else
		{
			headDir = Vector3.Slerp(headDir, normalized, 0.05f);
		}
		headPos = bug.bodyChunks[0].pos - headDir * (5f * scale) + vector * (2f * scale);
		faceFlip = Vector2.Dot(-vector2, headDir);
		tongue.Update();
		for (int i = 0; i < antennas.Length; i++)
		{
			antennas[i].Update();
		}
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				legs[j, k].Update();
			}
		}
		for (int l = 0; l < wings.Length; l++)
		{
			wings[l].Update();
		}
		if (!shadowMode && bug.mainBodyChunk.pos.y > bug.room.PixelHeight + 400f)
		{
			shadowMode = true;
		}
		else if (shadowMode && bug.mainBodyChunk.pos.y < bug.room.PixelHeight + 200f)
		{
			shadowMode = false;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		int segments = (bug.Small ? 3 : 8);
		sLeaser.sprites[AbdomenMesh] = TriangleMesh.MakeLongMesh(8, pointyTip: false, customColor: true);
		sLeaser.sprites[BodyMesh] = TriangleMesh.MakeLongMesh(segments, pointyTip: false, customColor: true);
		segments = (bug.Small ? 2 : 5);
		sLeaser.sprites[HeadMesh] = TriangleMesh.MakeLongMesh(segments, pointyTip: false, customColor: false);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[EyeSprite(i)] = new FSprite("Circle20");
			sLeaser.sprites[EyeSprite(i)].scaleY = 0.3f * (0.5f + 0.5f * scale);
			sLeaser.sprites[EyeSprite(i)].scaleX = 0.21f * (0.5f + 0.5f * scale);
		}
		tongue.InitiateSprites(sLeaser, rCam);
		for (int j = 0; j < antennas.Length; j++)
		{
			antennas[j].InitiateSprites(sLeaser, rCam);
		}
		for (int k = 0; k < wings.Length; k++)
		{
			wings[k].InitiateSprites(sLeaser, rCam);
		}
		for (int l = 0; l < legs.GetLength(0); l++)
		{
			for (int m = 0; m < legs.GetLength(1); m++)
			{
				legs[l, m].InitiateSprites(sLeaser, rCam);
			}
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (shadowMode)
		{
			newContatiner = rCam.ReturnFContainer("Shadows");
		}
		else if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		spritesInShadowMode = sLeaser.sprites[EyeSprite(0)].color == new Color(0.003921569f, 0f, 0f);
		if (!shadowMode && spritesInShadowMode)
		{
			ExitShadowMode(sLeaser, rCam, changeContainer: true);
			spritesInShadowMode = false;
		}
		else if (shadowMode && !spritesInShadowMode)
		{
			EnterShadowMode(sLeaser, rCam, changeContainer: true);
			spritesInShadowMode = true;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (shadowMode)
		{
			camPos.y += rCam.room.PixelHeight + 300f;
		}
		else if (culled)
		{
			return;
		}
		Vector2[] array = new Vector2[bug.bodyChunks.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Vector2.Lerp(bug.bodyChunks[i].lastPos, bug.bodyChunks[i].pos, timeStacker);
		}
		Vector2 a = array[0] + Custom.DirVec(array[1], array[0]) * (7f * scale);
		Vector2 vector = array[2] + Custom.DirVec(array[2], array[3]) * (20f * scale);
		Vector2 cA = array[1] + Custom.PerpendicularVector(Custom.DirVec(array[2], array[0])) * (bug.flip * 7f * scale);
		Color b = BodyColorWithDepth(0f);
		Color a2 = BodyColorWithDepth(Mathf.Lerp(0.2f, 0.5f, Mathf.Abs(bug.flip)));
		float num = 0f;
		Vector2 vector2 = vector + Custom.DirVec(array[2], array[3]);
		int num2 = (bug.Small ? 3 : 8);
		for (int j = 0; j < num2; j++)
		{
			float num3 = 1f - Mathf.InverseLerp(0f, num2 - 1, j);
			num3 = Mathf.Lerp((float)Math.Tanh((num3 - 0.5f) * (float)Math.PI * 2f) * 0.5f + 0.5f, num3, 0.5f);
			Vector2 vector3 = Custom.Bezier(a, cA, vector, array[1], num3);
			Vector2 vector4 = Custom.PerpendicularVector(vector3, vector2);
			float num4 = Mathf.Lerp(1f, 9f + bug.iVars.chub * 4f, Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num3, 1.4f) * (float)Math.PI)), 0.2f)) * Mathf.Lerp(1f, 1.25f, num3) * scale;
			Vector2 vector5 = vector4 * ((num4 + num) * 0.5f);
			Vector2 vector6 = vector4 * num4;
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4, (vector2 + vector3) / 2f - vector5 - camPos);
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 1, (vector2 + vector3) / 2f + vector5 - camPos);
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 2, vector3 - vector6 - camPos);
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 3, vector3 + vector6 - camPos);
			Color color = Color.Lerp(a2, b, 0.3f + Mathf.InverseLerp(0f, 7.5f, j) * 0.7f);
			Color color2 = Color.Lerp(a2, b, 0.3f + Mathf.InverseLerp(0f, 7.5f, (float)j + 0.5f) * 0.7f);
			(sLeaser.sprites[BodyMesh] as TriangleMesh).verticeColors[j * 4] = color;
			(sLeaser.sprites[BodyMesh] as TriangleMesh).verticeColors[j * 4 + 1] = color;
			(sLeaser.sprites[BodyMesh] as TriangleMesh).verticeColors[j * 4 + 2] = color2;
			(sLeaser.sprites[BodyMesh] as TriangleMesh).verticeColors[j * 4 + 3] = color2;
			vector2 = vector3;
			num = num4;
		}
		a = array[2] + Custom.DirVec(array[2], array[1]) * (10f * scale);
		vector = array[4] + Custom.DirVec(array[3], array[4]) * (10f * scale);
		cA = array[3];
		num = 0f;
		vector2 = vector + Custom.DirVec(array[3], array[4]) * (1.5f * scale);
		for (int k = 0; k < 8; k++)
		{
			float num5 = 1f - Mathf.InverseLerp(0f, 7f, k);
			num5 = Mathf.Lerp((float)Math.Tanh((num5 - 0.5f) * (float)Math.PI * 2f) * 0.5f + 0.5f, num5, 0.5f);
			Vector2 vector7 = Custom.Bezier(a, cA, vector, cA, num5);
			Vector2 vector8 = Custom.PerpendicularVector(vector7, vector2);
			float num6 = Mathf.Lerp(1f, 13f + 6f * bug.iVars.chub, Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(num5, 1.4f) * (float)Math.PI)), 0.5f)) * scale;
			Vector2 vector9 = vector8 * ((num6 + num) * 0.5f);
			Vector2 vector10 = vector8 * num6;
			(sLeaser.sprites[AbdomenMesh] as TriangleMesh).MoveVertice(k * 4, (vector2 + vector7) / 2f - vector9 - camPos);
			(sLeaser.sprites[AbdomenMesh] as TriangleMesh).MoveVertice(k * 4 + 1, (vector2 + vector7) / 2f + vector9 - camPos);
			(sLeaser.sprites[AbdomenMesh] as TriangleMesh).MoveVertice(k * 4 + 2, vector7 - vector10 - camPos);
			(sLeaser.sprites[AbdomenMesh] as TriangleMesh).MoveVertice(k * 4 + 3, vector7 + vector10 - camPos);
			Color color3 = Color.Lerp(a2, b, Mathf.InverseLerp(0f, 7.5f, k) * 0.5f);
			Color color4 = Color.Lerp(a2, b, Mathf.InverseLerp(0f, 7.5f, (float)k + 0.5f) * 0.5f);
			(sLeaser.sprites[AbdomenMesh] as TriangleMesh).verticeColors[k * 4] = color3;
			(sLeaser.sprites[AbdomenMesh] as TriangleMesh).verticeColors[k * 4 + 1] = color3;
			(sLeaser.sprites[AbdomenMesh] as TriangleMesh).verticeColors[k * 4 + 2] = color4;
			(sLeaser.sprites[AbdomenMesh] as TriangleMesh).verticeColors[k * 4 + 3] = color4;
			vector2 = vector7;
			num = num6;
		}
		Vector2 vector11 = Vector2.Lerp(lastHeadPos, headPos, timeStacker);
		Vector2 vector12 = Vector3.Slerp(lastHeadDir, headDir, timeStacker);
		Custom.PerpendicularVector(Custom.DirVec(array[1], array[0]));
		Vector2 vector13 = Custom.PerpendicularVector(vector12);
		float num7 = 27f * scale;
		float num8 = 8f * (1f - Mathf.Abs(faceFlip) * 0.25f) * scale;
		num2 = (bug.Small ? 4 : 10);
		for (int l = 0; l < num2; l++)
		{
			float f = Mathf.InverseLerp(0f, num2 - 1, l);
			f = Mathf.Pow(f, 2f);
			float num9 = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.Pow(f, 0.5f) * (float)Math.PI)), 0.7f) * num8;
			Vector2 vector14 = vector12 * (num7 * f);
			(sLeaser.sprites[HeadMesh] as TriangleMesh).MoveVertice(l * 2, vector11 + vector14 - vector13 * num9 - camPos);
			(sLeaser.sprites[HeadMesh] as TriangleMesh).MoveVertice(l * 2 + 1, vector11 + vector14 + vector13 * num9 - camPos);
		}
		for (int m = 0; m < 2; m++)
		{
			Vector2 vector15 = vector11 + (vector12 * (18f * ((m == 1) ? (0.8f + (1f - Mathf.Abs(faceFlip)) * 0.2f) : 1f)) + vector13 * (6.5f * ((m == 0) ? 1f : (-1f)) * (1f - Mathf.Abs(faceFlip) * 0.25f) * ((faceFlip > 0f) ? 1f : (-1f)))) * scale;
			sLeaser.sprites[EyeSprite(m)].rotation = Custom.VecToDeg((vector12 + vector13 * (((m == 0) ? (-1f) : 1f) * ((faceFlip >= 0f) ? 1f : (-1f)) * 0.2f)).normalized);
			sLeaser.sprites[EyeSprite(m)].x = vector15.x - camPos.x;
			sLeaser.sprites[EyeSprite(m)].y = vector15.y - camPos.y;
			sLeaser.sprites[EyeSprite(m)].scaleX = 0.21f * Mathf.Lerp(lastEyeOpenness, eyeOpenness, timeStacker) * (0.5f + 0.5f * scale);
		}
		tongue.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		for (int n = 0; n < antennas.Length; n++)
		{
			antennas[n].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		for (int num10 = 0; num10 < wings.Length; num10++)
		{
			wings[num10].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		for (int num11 = 0; num11 < legs.GetLength(0); num11++)
		{
			for (int num12 = 0; num12 < legs.GetLength(1); num12++)
			{
				legs[num11, num12].DrawSprites(sLeaser, rCam, timeStacker, camPos);
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		this.palette = palette;
		ExitShadowMode(sLeaser, rCam, changeContainer: false);
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public void EnterShadowMode(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, bool changeContainer)
	{
		Color color = new Color(0.003921569f, 0f, 0f);
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (sLeaser.sprites[i] is TriangleMesh { customColor: not false } triangleMesh)
			{
				for (int j = 0; j < triangleMesh.verticeColors.Length; j++)
				{
					triangleMesh.verticeColors[j] = color;
				}
			}
			else
			{
				sLeaser.sprites[i].color = color;
			}
		}
		if (changeContainer)
		{
			AddToContainer(sLeaser, rCam, null);
		}
	}

	public void ExitShadowMode(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, bool changeContainer)
	{
		bodyColor = Color.Lerp(bug.iVars.bodyColor, palette.fogColor, 0.03f);
		bodyColor = Color.Lerp(bodyColor, palette.blackColor, palette.darkness * 0.3f);
		depthColor = Color.Lerp(Color.Lerp(bodyColor, Color.black, 0.2f), palette.fogColor, 0.7f);
		tongueBaseColor = Color.Lerp(Color.Lerp(Color.red, bodyColor, 0.7f), palette.blackColor, 0.15f);
		tongueTipColor = Color.Lerp(tongueBaseColor, palette.blackColor, 0.44f);
		sLeaser.sprites[HeadMesh].color = bodyColor;
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[EyeSprite(i)].color = palette.blackColor;
		}
		for (int j = 0; j < 2; j++)
		{
			sLeaser.sprites[AntennaSprites(j)].color = Color.Lerp(bodyColor, palette.blackColor, 0.1f);
			sLeaser.sprites[AntennaSprites(j) + 1].color = bodyColor;
		}
		if (changeContainer)
		{
			AddToContainer(sLeaser, rCam, null);
		}
	}

	public Color BodyColorWithDepth(float depth, float secondary = 0f)
	{
		if (shadowMode)
		{
			return new Color(0.003921569f, 0f, 0f);
		}
		return Color.Lerp(Color.Lerp(bodyColor, bug.iVars.secondaryColor, secondary), depthColor, depth);
	}

	public bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite)
	{
		if (!bug.Small)
		{
			return false;
		}
		if (sprite == EyeSprite(0) || sprite == EyeSprite(0))
		{
			return false;
		}
		return true;
	}

	public void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay)
	{
	}
}
