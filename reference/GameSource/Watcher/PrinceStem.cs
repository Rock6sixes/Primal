using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class PrinceStem : UpdatableAndDeletable, IDrawable
{
	public class Joint
	{
		public PrinceStem arm;

		public Joint previous;

		public Joint next;

		public int index;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float totalLength;

		public float alongArmFrac;

		public float nextAlongArmFrac;

		public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public float currentInvKinFlip;

		public float CurrentLength
		{
			get
			{
				if (next != null)
				{
					return Vector2.Distance(pos, next.pos);
				}
				return Vector2.Distance(pos, arm.prince.bodyChunks[0].pos);
			}
		}

		public Vector2 ElbowPos(float timeStacker, Vector2 Tip)
		{
			Vector2 vc = Vector2.Lerp(lastPos, pos, timeStacker);
			if (next != null)
			{
				return Custom.InverseKinematic(Tip, vc, totalLength * (1f / 3f), totalLength * (2f / 3f), (index % 2 == 0) ? 1f : (-1f));
			}
			return Custom.InverseKinematic(Tip, vc, totalLength / 3f, totalLength / 3f, (index % 2 == 0) ? 1f : (-1f));
		}

		public Joint(PrinceStem arm, int index, float totalLength)
		{
			this.arm = arm;
			this.index = index;
			this.totalLength = totalLength;
			currentInvKinFlip = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			pos = arm.BasePos(1f);
			lastPos = pos;
		}

		public void Update()
		{
			Room room = arm.prince.room;
			lastPos = pos;
			pos += vel;
			vel *= 0.7f;
			bool flag = true;
			if ((float)index == 0f)
			{
				pos = arm.BasePos(1f);
			}
			else if (index < arm.joints.Length - 1)
			{
				if (index == 1 && (flag || room.GetTile(previous.ElbowPos(1f, pos)).Solid))
				{
					Vector2 vector = Custom.InverseKinematic(previous.pos, next.pos, previous.totalLength, totalLength, currentInvKinFlip);
					Vector2 a = Custom.InverseKinematic(previous.pos, next.pos, previous.totalLength, totalLength, 0f - currentInvKinFlip);
					float num = (room.GetTile(vector).Solid ? 10f : 0f);
					num += (room.GetTile(previous.ElbowPos(1f, Vector2.Lerp(vector, previous.pos, 0.2f))).Solid ? 1f : 0f);
					float num2 = (room.GetTile(a).Solid ? 10f : 0f);
					num2 += (room.GetTile(previous.ElbowPos(1f, Vector2.Lerp(a, previous.pos, 0.2f))).Solid ? 1f : 0f);
					if (num > num2)
					{
						currentInvKinFlip *= -1f;
						vector = Custom.InverseKinematic(previous.pos, next.pos, previous.totalLength, totalLength, currentInvKinFlip);
					}
					else if (num == 0f)
					{
						vel += Vector2.ClampMagnitude(vector - pos, 100f) / 100f * 1.8f;
					}
				}
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, 1f, new IntVector2(0, 0), goThroughFloors: true);
				cd = SharedPhysics.VerticalCollision(room, cd);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				cd = SharedPhysics.SlopesVertically(room, cd);
				pos = cd.pos;
				vel = cd.vel;
			}
			if (next != null)
			{
				Vector2 vector2 = Custom.DirVec(pos, next.pos);
				float currentLength = CurrentLength;
				float num3 = 0.5f;
				if (index == 0)
				{
					num3 = 0f;
				}
				else if (index == arm.joints.Length - 2)
				{
					num3 = 1f;
				}
				float num4 = -1f;
				float num5 = 0.5f;
				if (previous != null)
				{
					num5 = Custom.LerpMap(Vector2.Dot(Custom.DirVec(previous.pos, pos), vector2), -1f, 1f, 1f, 0.2f);
				}
				if (currentLength > totalLength)
				{
					num4 = totalLength;
				}
				else if (currentLength < totalLength * num5)
				{
					num4 = totalLength * num5;
				}
				if (num4 > 0f)
				{
					Vector2 vector3 = vector2 * ((currentLength - num4) * num3);
					Vector2 vector4 = vector2 * ((currentLength - num4) * (1f - num3));
					pos += vector3;
					vel += vector3;
					next.vel -= vector4;
				}
			}
			else
			{
				Vector2 vector5 = arm.prince.bodyChunks[1].pos;
				vector5 -= Custom.PerpendicularVector(arm.prince.behavior.GetToDir) * totalLength / 2f;
				vector5 += Custom.DirVec(arm.prince.bodyChunks[1].pos, pos) * totalLength / 2f;
				vel += Vector2.ClampMagnitude(vector5 - pos, 50f) / 50f * 1.2f;
				pos += Vector2.ClampMagnitude(vector5 - pos, 50f) / 50f * 1.2f;
				Vector2 vector6 = Custom.DirVec(pos, arm.prince.bodyChunks[0].pos);
				float currentLength2 = CurrentLength;
				pos += vector6 * (currentLength2 - totalLength);
				vel += vector6 * (currentLength2 - totalLength);
			}
		}
	}

	public class Tether : SegmentRope
	{
		public PrinceStem stem;

		public int fromJoint;

		public int toJoint;

		public float radFac;

		public float FromRad => stem.OverallStemRad(stem.joints[fromJoint].alongArmFrac) * stem.StemSegmentRad(0f);

		public float ToRad => stem.OverallStemRad(stem.joints[fromJoint].nextAlongArmFrac) * stem.StemSegmentRad(1f);

		public float GetRad(float t)
		{
			return Mathf.Lerp(FromRad, ToRad, t) * (0.7f + 0.3f * Mathf.Cos(t * (float)Math.PI * 2f)) * radFac;
		}

		public Tether(PrinceStem stem, int fromJoint, int toJoint, float idealLength, float radFac)
			: base(Mathf.CeilToInt(idealLength / 30f), idealLength)
		{
			this.stem = stem;
			this.fromJoint = fromJoint;
			this.toJoint = toJoint;
			this.radFac = radFac;
			drag = 0.95f;
			Reset();
		}

		public void Update()
		{
			Vector2 bezierPos = stem.GetBezierPos(fromJoint, 1f);
			Vector2 bezierPos2 = stem.GetBezierPos(toJoint, 1f);
			Vector2 vector = stem.GetBezierTangent(fromJoint, 1f) * 0.05f;
			Vector2 vector2 = stem.GetBezierTangent(toJoint, 1f) * 0.05f;
			base.UpdateRope(stem.room, bezierPos, bezierPos2);
			for (int i = 0; i < segments.Length; i++)
			{
				float num = (float)i / (float)(segments.Length - 1);
				segments[i].vel += vector * Mathf.Max(0f, 1f - 2f * num);
				segments[i].vel -= vector2 * Mathf.Max(0f, 2f * num - 1f);
			}
		}

		public void Reset()
		{
			Reset(stem.GetBezierPos(fromJoint, 1f), stem.GetBezierPos(toJoint, 1f));
		}
	}

	public Joint[] joints;

	public PrinceBulb bulb;

	public Prince prince;

	public StaticSoundLoop baseMoveSoundLoop;

	public Rect area;

	public Color effectColor;

	public Tether[] tethers;

	public static float[] tangentLengths = new float[5] { 100f, 100f, 40f, 40f, 80f };

	public int firstBoneSprite;

	public int firstTetherSprite;

	public int totalSprites;

	public int jointSoundCooldown;

	public const float headForce = 4.2f;

	public const float bodyForce = 0.5f;

	public float Extension
	{
		get
		{
			float num = 0f;
			float num2 = 0f;
			Joint[] array = joints;
			foreach (Joint joint in array)
			{
				num2 += joint.totalLength;
				num += joint.CurrentLength;
			}
			return num / num2;
		}
	}

	public PrinceStem(PrinceBulb bulb, Prince prince)
	{
		this.bulb = bulb;
		this.prince = prince;
		joints = new Joint[4]
		{
			new Joint(this, 0, 300f),
			new Joint(this, 1, 150f),
			new Joint(this, 2, 90f),
			new Joint(this, 3, 30f)
		};
		for (int i = 1; i < joints.Length; i++)
		{
			joints[i].previous = joints[i - 1];
			joints[i - 1].next = joints[i];
		}
		float num = joints.Sum((Joint joint) => joint.totalLength);
		float num2 = 0f;
		for (int j = 0; j < joints.Length; j++)
		{
			joints[j].alongArmFrac = num2 / num;
			joints[j].nextAlongArmFrac = (num2 + joints[j].totalLength) / num;
			num2 += joints[j].totalLength;
		}
		tethers = new Tether[4]
		{
			new Tether(this, 0, 1, joints[0].totalLength * 1.2f, 0.5f),
			new Tether(this, 0, 1, joints[0].totalLength * 1.5f, 0.45f),
			new Tether(this, 1, 4, joints[1].totalLength + joints[2].totalLength + joints[3].totalLength, 0.25f),
			new Tether(this, 0, 3, (joints[0].totalLength + joints[1].totalLength + joints[2].totalLength) * 0.7f, 0.3f)
		};
		Vector2 vector = bulb.room.MiddleOfTile(14, 24);
		Vector2 vector2 = bulb.room.MiddleOfTile(30, 40);
		if (Region.IsDaemonRegion(bulb.room.world.name))
		{
			vector = bulb.room.MiddleOfTile(20, 21);
			vector2 = bulb.room.MiddleOfTile(24, 25);
		}
		area = new Rect(vector, vector2 - vector);
		firstTetherSprite = totalSprites;
		totalSprites += tethers.Length;
		firstBoneSprite = totalSprites;
		totalSprites += joints.Length;
		effectColor = bulb.abstractPhysicalObject.world.region?.regionParams.corruptionEffectColor ?? Color.blue;
	}

	public override void Update(bool eu)
	{
		if (baseMoveSoundLoop == null)
		{
			baseMoveSoundLoop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Prince_Arm_Move_LOOP, prince.firstChunk.pos, prince.room, 1f, 1f);
		}
		baseMoveSoundLoop.pos = prince.firstChunk.pos;
		if (prince.bodyChunks[1].vel.magnitude >= 0.8f)
		{
			float num = Mathf.InverseLerp(0.8f, 3f, prince.bodyChunks[1].vel.magnitude);
			float b = 0.5f + num * 0.5f;
			baseMoveSoundLoop.volume = Mathf.Lerp(baseMoveSoundLoop.volume, num, 0.08f);
			baseMoveSoundLoop.pitch = Mathf.Lerp(baseMoveSoundLoop.pitch, b, 0.08f);
		}
		else
		{
			baseMoveSoundLoop.volume = Mathf.Lerp(baseMoveSoundLoop.volume, 0f, 0.08f);
			baseMoveSoundLoop.pitch = Mathf.Lerp(baseMoveSoundLoop.pitch, 0.5f, 0.08f);
		}
		baseMoveSoundLoop.Update();
		if (prince.Consious)
		{
			prince.bodyChunks[0].vel *= 0.5f;
			Vector2 getToPos = prince.behavior.GetToPos;
			Vector2 getToDir = prince.behavior.GetToDir;
			prince.bodyChunks[0].vel += Vector2.ClampMagnitude(getToPos - prince.bodyChunks[0].pos, 40f) / 40f * 4.2f;
			for (int i = 1; i < prince.bodyChunks.Length; i++)
			{
				prince.bodyChunks[i].vel += Vector2.ClampMagnitude(getToPos - prince.bodyChunks[0].pos, 40f) / 40f * 0.5f;
			}
			prince.WeightedPush(1, 0, -getToDir, 5f);
			prince.WeightedPush(2, 0, -getToDir, 4f);
			prince.WeightedPush(4, 0, Vector2.down, 0.25f);
		}
		if (jointSoundCooldown > 0)
		{
			jointSoundCooldown--;
		}
		List<int> list = null;
		for (int j = 0; j < joints.Length; j++)
		{
			joints[j].Update();
			if (jointSoundCooldown <= 0 && joints[j].vel.magnitude >= 2f)
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(j);
			}
		}
		if (list != null)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Prince_Arm_Joint, joints[list[UnityEngine.Random.Range(0, list.Count)]].pos);
			jointSoundCooldown = UnityEngine.Random.Range(20, 80);
		}
		for (int k = 0; k < tethers.Length; k++)
		{
			tethers[k].Update();
		}
	}

	public Vector2 BasePos(float timeStacker)
	{
		return Vector2.Lerp(bulb.firstChunk.lastPos, bulb.firstChunk.pos, timeStacker);
	}

	public static string GetStemTexture()
	{
		if (!Futile.atlasManager.DoesContainAtlas("veins"))
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "veins.png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: false, crispPixels: false);
			texture2D.wrapMode = TextureWrapMode.Mirror;
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture("veins", texture2D, textureFromAsset: false);
		}
		return "veins";
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		string stemTexture = GetStemTexture();
		for (int i = 0; i < joints.Length; i++)
		{
			sLeaser.sprites[firstBoneSprite + i] = TriangleMesh.MakeLongMesh(10, pointyTip: false, customColor: true, stemTexture);
			sLeaser.sprites[firstBoneSprite + i].shader = rCam.game.rainWorld.Shaders["PrinceStem"];
		}
		for (int j = 0; j < tethers.Length; j++)
		{
			sLeaser.sprites[firstTetherSprite + j] = TriangleMesh.MakeLongMesh(tethers[j].segments.Length, pointyTip: false, customColor: true, stemTexture);
			sLeaser.sprites[firstTetherSprite + j].shader = rCam.game.rainWorld.Shaders["PrinceStem"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < joints.Length; i++)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[firstBoneSprite + i] as TriangleMesh;
			for (int j = 0; j < triangleMesh.vertices.Length; j += 2)
			{
				float num = (float)j / (float)(triangleMesh.vertices.Length - 2);
				float t = Mathf.Lerp(joints[i].alongArmFrac, joints[i].nextAlongArmFrac, num);
				Vector2 bezierPos = GetBezierPos(i, timeStacker);
				Vector2 cA = bezierPos + GetBezierTangent(i, timeStacker);
				Vector2 bezierPos2 = GetBezierPos(i + 1, timeStacker);
				Vector2 cB = bezierPos2 - GetBezierTangent(i + 1, timeStacker);
				float num2 = OverallStemRad(t) * StemSegmentRad(num);
				Vector2 vector = Custom.Bezier(bezierPos, cA, bezierPos2, cB, num);
				Vector2 vector2 = Custom.PerpendicularVector(Custom.BezierDerivative(bezierPos, cA, bezierPos2, cB, num));
				triangleMesh.MoveVertice(j, vector + vector2 * num2 - camPos);
				triangleMesh.MoveVertice(j + 1, vector - vector2 * num2 - camPos);
			}
		}
		for (int k = 0; k < tethers.Length; k++)
		{
			TriangleMesh triangleMesh2 = sLeaser.sprites[firstTetherSprite + k] as TriangleMesh;
			SimpleSegment[] segments = tethers[k].segments;
			Vector2 vector3 = segments[0].DrawPos(timeStacker);
			vector3 += (vector3 - segments[1].DrawPos(timeStacker)).normalized;
			float num3 = tethers[k].GetRad(0f);
			for (int l = 0; l < segments.Length; l++)
			{
				Vector2 vector4 = segments[l].DrawPos(timeStacker);
				Vector2 vector5 = Custom.PerpendicularVector(Custom.DirVec(vector3, vector4));
				float t2 = (float)l / (float)(segments.Length - 1);
				float rad = tethers[k].GetRad(t2);
				triangleMesh2.MoveVertice(l * 4, (vector3 + vector4) / 2f - vector5 * (num3 + rad) / 2f - camPos);
				triangleMesh2.MoveVertice(l * 4 + 1, (vector3 + vector4) / 2f + vector5 * (num3 + rad) / 2f - camPos);
				triangleMesh2.MoveVertice(l * 4 + 2, vector4 - vector5 * rad - camPos);
				triangleMesh2.MoveVertice(l * 4 + 3, vector4 + vector5 * rad - camPos);
				vector3 = vector4;
				num3 = rad;
			}
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public float StemSegmentRad(float t)
	{
		return 1f + Mathf.Cos(t * (float)Math.PI * 2f) * 0.1f;
	}

	public float OverallStemRad(float t)
	{
		return 25f - 15f * t;
	}

	public Vector2 GetBezierTangent(int joint, float timeStacker)
	{
		if (joint == 0)
		{
			return bulb.GetDirection(timeStacker) * tangentLengths[joint];
		}
		if (joint == joints.Length)
		{
			Vector2 vector = Vector2.Lerp(prince.bodyChunks[0].lastPos, prince.bodyChunks[0].pos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(prince.bodyChunks[1].lastPos, prince.bodyChunks[1].pos, timeStacker);
			return (vector - vector2).normalized * tangentLengths[joint];
		}
		Vector2 bezierPos = GetBezierPos(joint, timeStacker);
		Vector2 bezierPos2 = GetBezierPos(joint - 1, timeStacker);
		Vector2 vector3 = (GetBezierPos(joint + 1, timeStacker) - bezierPos).normalized - (bezierPos2 - bezierPos).normalized;
		if (vector3.magnitude > 1E-05f)
		{
			vector3 /= Mathf.Sqrt(vector3.magnitude);
		}
		return vector3 * tangentLengths[joint];
	}

	public Vector2 GetBezierPos(int joint, float timeStacker)
	{
		if (joint < joints.Length)
		{
			return Vector2.Lerp(joints[joint].lastPos, joints[joint].pos, timeStacker);
		}
		return Vector2.Lerp(prince.bodyChunks[0].lastPos, prince.bodyChunks[0].pos, timeStacker);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		float num = 0f;
		float num2 = 0f;
		Joint[] array = joints;
		foreach (Joint joint in array)
		{
			num2 += joint.totalLength;
		}
		for (int j = 0; j < joints.Length; j++)
		{
			TriangleMesh triangleMesh = sLeaser.sprites[firstBoneSprite + j] as TriangleMesh;
			for (int k = 0; k < triangleMesh.verticeColors.Length; k += 2)
			{
				float num3 = (float)k / (float)(triangleMesh.verticeColors.Length - 2);
				float a = (num + joints[j].totalLength * num3) / num2;
				Color color = effectColor;
				color.a = a;
				triangleMesh.verticeColors[k] = color;
				triangleMesh.verticeColors[k + 1] = color;
				triangleMesh.UVvertices[k].y = 0f;
				triangleMesh.UVvertices[k + 1].y = 0f;
			}
			num += joints[j].totalLength;
		}
		for (int l = 0; l < tethers.Length; l++)
		{
			TriangleMesh triangleMesh2 = sLeaser.sprites[firstTetherSprite + l] as TriangleMesh;
			float alongArmFrac = joints[tethers[l].fromJoint].alongArmFrac;
			float nextAlongArmFrac = joints[tethers[l].toJoint - 1].nextAlongArmFrac;
			for (int m = 0; m < triangleMesh2.verticeColors.Length; m += 2)
			{
				Color color2 = Color.Lerp(effectColor, palette.blackColor, 0.5f);
				color2.a = Mathf.Lerp(alongArmFrac, nextAlongArmFrac, (float)m / (float)(triangleMesh2.verticeColors.Length - 2));
				triangleMesh2.verticeColors[m] = color2;
				triangleMesh2.verticeColors[m + 1] = color2;
				triangleMesh2.UVvertices[m].y = (float)(l + 1) / (float)tethers.Length;
				triangleMesh2.UVvertices[m + 1].y = (float)(l + 1) / (float)tethers.Length;
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite fSprite in sprites)
		{
			fSprite.RemoveFromContainer();
			newContatiner.AddChild(fSprite);
		}
	}
}
