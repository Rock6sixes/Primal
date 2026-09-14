using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleJellyGraphics : GraphicsModule
{
	public class JellyLeg
	{
		public const int LengthPerSegment = 15;

		public bool Big;

		public int SegmentCount;

		public Vector2 attachmentOffset;

		public Vector2 legDirection;

		public Vector2[,] segments;

		public float IdleMinThreshold;

		public float IdleMaxThreshold;

		public float IdleAmount;

		public float PrevIdleAmount;

		public float wiggleOffset;

		public Vector2 AttachmentPoint(Vector2 bodyPos, float bodyRotation, float tilt)
		{
			return bodyPos + Custom.rotateVectorDeg(attachmentOffset * new Vector2(Mathf.Cos(tilt * ((float)Math.PI / 180f)), 1f), bodyRotation);
		}

		public JellyLeg(Vector2 bodyPos, Vector2 attachmentOffset, float length, bool big)
		{
			this.attachmentOffset = attachmentOffset;
			legDirection = attachmentOffset.normalized;
			Big = big;
			SegmentCount = Mathf.CeilToInt(length / 15f);
			segments = new Vector2[SegmentCount, 2];
			for (int i = 0; i < SegmentCount; i++)
			{
				segments[i, 0] = bodyPos + attachmentOffset + legDirection * length;
				segments[i, 1] = segments[i, 0];
			}
			wiggleOffset = UnityEngine.Random.value * (float)Math.PI;
			IdleMinThreshold = UnityEngine.Random.Range(-100, 150);
			IdleMaxThreshold = IdleMinThreshold + (float)UnityEngine.Random.Range(10, 200);
		}

		public void Reset(Vector2 bodyPos, float bodyRotation, float tilt)
		{
			Vector2 vector = AttachmentPoint(bodyPos, bodyRotation, tilt);
			segments[0, 1] = vector;
			segments[0, 0] = vector;
			Vector2 vector2 = Custom.rotateVectorDeg(legDirection, bodyRotation);
			for (int i = 1; i < SegmentCount; i++)
			{
				_ = vector2 * 15f;
				Vector2 vector3 = vector + vector2 * 15f * i;
				segments[i, 0] = vector3;
				segments[i, 1] = vector3;
			}
		}

		public void Update(Vector2 bodyPos, float bodyRotation, float tilt, float idleTime, float time)
		{
			PrevIdleAmount = IdleAmount;
			float b = Custom.MapRangeClamped(idleTime, IdleMinThreshold, IdleMaxThreshold, 0f, 1f);
			IdleAmount = Mathf.Lerp(IdleAmount, b, 0.03f);
			Vector2 vector = AttachmentPoint(bodyPos, bodyRotation, tilt);
			segments[0, 1] = segments[0, 0];
			segments[0, 0] = vector;
			Vector2 vector2 = vector;
			Vector2 vector3 = Custom.rotateVectorDeg(legDirection, bodyRotation);
			for (int i = 1; i < SegmentCount; i++)
			{
				Vector2 vector4 = segments[i, 0];
				Vector2 vector5 = segments[i, 1];
				Vector2 vector6 = vector4 - vector5;
				segments[i, 1] = vector4;
				Vector2 vector7 = vector4 + vector6 * 0.85f;
				vector7 = (vector7 - vector2).normalized * 15f;
				Vector2 target = vector3 * 15f;
				float num = 1f - (float)i / (float)SegmentCount;
				vector7 = Custom.ClampAngle(target, vector7, -5f, 5f, 0.01f * num * num);
				float num2 = Mathf.Sin(time / 80f + (float)i * 0.1f + wiggleOffset) * (1f - num) * 0.05f;
				vector7 += num2 * Custom.PerpendicularVector(vector7).normalized;
				vector7 += vector2;
				segments[i, 0] = vector7;
				vector2 = vector7;
			}
		}
	}

	public int bodySpriteIndex;

	public int legsStartSpriteIndex;

	public int legsEndSpriteIndex;

	public int effectSpriteIndex;

	public int glowSpriteIndex;

	public float desiredRotation;

	public float rotation;

	public float lastRotation;

	public float tilt;

	public float lastTilt;

	public float darkness;

	public JellyLeg[] legs;

	public float legScale;

	public float time;

	public RippleJelly jelly => base.owner as RippleJelly;

	public FShader BodyShader => Custom.rainWorld.Shaders[(jelly.abstractPhysicalObject.rippleLayer == 0) ? "RippleJellyBody" : "RippleJellyBodyRippleSide"];

	public FShader ArmsShader => Custom.rainWorld.Shaders[(jelly.abstractPhysicalObject.rippleLayer == 0) ? "RippleSpawnBody" : "RippleSpawnBodyRippleSide"];

	public FShader EffectShader => Custom.rainWorld.Shaders[(jelly.abstractPhysicalObject.rippleLayer == 0) ? "RippleGlow" : "RippleGlowRippleSide"];

	public FShader GlowShader => Custom.rainWorld.Shaders[(jelly.abstractPhysicalObject.rippleLayer == 0) ? "FlatWaterLightRippleSpawn" : "FlatWaterLightRippleSpawnRippleSide"];

	public RippleJellyGraphics(RippleJelly jelly)
		: base(jelly, internalContainers: false)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(this.jelly.abstractPhysicalObject.ID.RandomSeed);
		float rad = jelly.FirstChunk().rad;
		legScale = Custom.MapRangeClamped(rad, 0f, 60f, 0.3f, 1f);
		int num = 8;
		int num2 = UnityEngine.Random.Range(16, 32);
		int num3 = num + num2;
		legs = new JellyLeg[num3];
		int num4 = 0;
		for (int i = 0; i < num; i++)
		{
			float f = ((float)i / (float)num * 360f + (float)UnityEngine.Random.Range(-30, 30)) * ((float)Math.PI / 180f);
			Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
			float length = (float)(130 + UnityEngine.Random.Range(-20, 20)) * legScale;
			legs[num4++] = new JellyLeg(jelly.FirstChunk().pos, vector * (rad - 5f), length, big: true);
		}
		for (int j = 0; j < num2; j++)
		{
			Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
			float length2 = (float)(52 + UnityEngine.Random.Range(-10, 10)) * legScale;
			legs[num4++] = new JellyLeg(jelly.FirstChunk().pos, normalized * rad, length2, big: false);
		}
		UnityEngine.Random.state = state;
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].Reset(jelly.FirstChunk().pos, rotation, tilt);
		}
	}

	public override void Update()
	{
		base.Update();
		time += 1f;
		lastRotation = rotation;
		if (jelly.FirstChunk().vel.magnitude > 0.2f)
		{
			desiredRotation = jelly.FirstChunk().vel.GetAngle();
		}
		rotation = Mathf.LerpAngle(rotation, desiredRotation, 0.05f);
		lastTilt = tilt;
		float num = Mathf.Clamp(jelly.FirstChunk().vel.magnitude * 20f, 0f, 80f);
		if (tilt < num)
		{
			tilt = Mathf.Lerp(tilt, num, 0.1f);
		}
		else
		{
			tilt = Mathf.Lerp(tilt, num, 0.01f);
		}
		for (int i = 0; i < legs.Length; i++)
		{
			legs[i].Update(jelly.FirstChunk().pos, rotation, tilt, jelly.IdleTime, time);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3 + legs.Length];
		int num = (effectSpriteIndex = 0) + 1;
		sLeaser.sprites[effectSpriteIndex] = new FSprite("Futile_White");
		sLeaser.sprites[effectSpriteIndex].shader = EffectShader;
		glowSpriteIndex = num;
		num++;
		sLeaser.sprites[glowSpriteIndex] = new FSprite("Futile_White");
		sLeaser.sprites[glowSpriteIndex].shader = GlowShader;
		legsStartSpriteIndex = num;
		num += legs.Length;
		for (int i = 0; i < legs.Length; i++)
		{
			JellyLeg jellyLeg = legs[i];
			string text = (jellyLeg.Big ? "OralArm" : ("AquapedeWing" + (i % 4 + 1)));
			int num2 = legsStartSpriteIndex + i;
			TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(jellyLeg.SegmentCount, pointyTip: false, customColor: true, text);
			FAtlasElement elementWithName = Futile.atlasManager.GetElementWithName(text);
			float num3 = 1f / (float)((jellyLeg.SegmentCount - 1) * 2 + 1);
			for (int j = 0; j < triangleMesh.UVvertices.Length; j++)
			{
				triangleMesh.UVvertices[j].x = ((j % 2 == 0) ? 0f : 1f) * elementWithName.uvRect.width + elementWithName.uvRect.x;
				float num4 = (float)(j / 2) * num3;
				triangleMesh.UVvertices[j].y = num4 * elementWithName.uvRect.height + elementWithName.uvRect.y;
				triangleMesh.verticeColors[j].a = Mathf.Lerp(0.5f, 1f, num4 * num4);
			}
			triangleMesh.shader = ArmsShader;
			sLeaser.sprites[num2] = triangleMesh;
		}
		legsEndSpriteIndex = num;
		bodySpriteIndex = num;
		num++;
		sLeaser.sprites[bodySpriteIndex] = new FSprite("Futile_White");
		sLeaser.sprites[bodySpriteIndex].scale = (jelly.FirstChunk().rad * 2f + 4f) / sLeaser.sprites[bodySpriteIndex].width;
		sLeaser.sprites[bodySpriteIndex].shader = BodyShader;
		float r = Mathf.Clamp01(jelly.FirstChunk().rad / 30f);
		sLeaser.sprites[bodySpriteIndex].color = new Color(r, 1f, 1f, 1f);
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(jelly.firstChunk.lastPos, jelly.firstChunk.pos, timeStacker);
		Vector2 vector2 = vector - camPos;
		float num = Mathf.LerpAngle(lastRotation, rotation, timeStacker);
		float num2 = Mathf.LerpAngle(lastTilt, tilt, timeStacker);
		float num3 = Mathf.Lerp(jelly.lastFade, jelly.fade, timeStacker);
		float num4 = Mathf.Lerp(jelly.lastEatTime, jelly.eatTime, timeStacker) / (float)RippleJelly.MaxEatTime;
		num3 *= Mathf.Clamp01(Custom.MapRange(num4, 0.8f, 1f, 1f, 0f));
		float num5 = 1f - num4;
		num5 *= num5;
		float num6 = jelly.FirstChunk().rad * 2f;
		sLeaser.sprites[bodySpriteIndex].shader = BodyShader;
		sLeaser.sprites[bodySpriteIndex].x = vector2.x;
		sLeaser.sprites[bodySpriteIndex].y = vector2.y;
		sLeaser.sprites[bodySpriteIndex].rotation = num;
		float num7 = (num6 + 4f) / 20f;
		sLeaser.sprites[bodySpriteIndex].scaleX = num7 * Mathf.Abs(Mathf.Cos(num2 * ((float)Math.PI / 180f))) * num5;
		sLeaser.sprites[bodySpriteIndex].scaleY = num7 * num5;
		sLeaser.sprites[bodySpriteIndex].alpha = num3;
		sLeaser.sprites[effectSpriteIndex].shader = EffectShader;
		sLeaser.sprites[effectSpriteIndex].x = vector2.x;
		sLeaser.sprites[effectSpriteIndex].y = vector2.y;
		sLeaser.sprites[effectSpriteIndex].scale = 2f * num6 / 10f;
		sLeaser.sprites[effectSpriteIndex].alpha = Mathf.Pow(0.1f, 0.6f) * Custom.LerpMap(darkness, 0.1f, 0.9f, 0.3f, 1f, 1.5f) * num3;
		sLeaser.sprites[glowSpriteIndex].shader = GlowShader;
		sLeaser.sprites[glowSpriteIndex].x = vector2.x;
		sLeaser.sprites[glowSpriteIndex].y = vector2.y;
		sLeaser.sprites[glowSpriteIndex].scale = 2.8f * num6 * Mathf.Lerp(0.6f, 1f, darkness) / 10f;
		sLeaser.sprites[glowSpriteIndex].alpha = 0.5f * num3;
		float a = Mathf.Sin(num2 * ((float)Math.PI / 180f));
		float b = Mathf.Cos(num2 * ((float)Math.PI / 180f));
		for (int i = 0; i < legs.Length; i++)
		{
			JellyLeg jellyLeg = legs[i];
			float perc = Mathf.Lerp(jellyLeg.PrevIdleAmount, jellyLeg.IdleAmount, timeStacker);
			float num8 = Mathf.Max(Mathf.Lerp(a, b, Custom.LerpQuadEaseInOut(1f, 0f, perc)), 0.1f);
			float num9 = (float)(jellyLeg.Big ? 6 : 4) * legScale;
			int num10 = i + legsStartSpriteIndex;
			TriangleMesh triangleMesh = sLeaser.sprites[num10] as TriangleMesh;
			triangleMesh.shader = ArmsShader;
			Vector2 vector3 = Vector2.Lerp(jellyLeg.segments[0, 1], jellyLeg.segments[0, 0], timeStacker);
			for (int j = 0; j < jellyLeg.SegmentCount; j++)
			{
				Vector2 vector4 = Vector2.Lerp(jellyLeg.segments[j, 1], jellyLeg.segments[j, 0], timeStacker);
				vector4 = (vector4 - vector3) * num8 + vector3;
				vector4 = (vector4 - vector) * num5 + vector;
				Vector2 normalized = (vector4 - vector3).normalized;
				Vector2 vector5 = Custom.PerpendicularVector(normalized) * num9;
				float num11 = Vector2.Distance(vector4, vector3) / 5f;
				triangleMesh.MoveVertice(j * 4, vector3 - vector5 + normalized * num11 - camPos);
				triangleMesh.MoveVertice(j * 4 + 1, vector3 + vector5 + normalized * num11 - camPos);
				triangleMesh.MoveVertice(j * 4 + 2, vector4 - vector5 - normalized * num11 - camPos);
				triangleMesh.MoveVertice(j * 4 + 3, vector4 + vector5 - normalized * num11 - camPos);
				vector3 = vector4;
			}
			float num12 = 1f / (float)((jellyLeg.SegmentCount - 1) * 2 + 1);
			for (int k = 0; k < triangleMesh.verticeColors.Length; k += 2)
			{
				float num13 = (float)(k / 2) * num12;
				float num14 = Mathf.Pow(num3, 1f - num13 * 0.9f);
				triangleMesh.verticeColors[k].a = Mathf.Lerp(0.5f, 1f, num13 * num13) * num14;
				triangleMesh.verticeColors[k + 1].a = Mathf.Lerp(0.5f, 1f, num13 * num13) * num14;
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		darkness = palette.darkness;
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("GrabShaders");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			if (i == glowSpriteIndex)
			{
				rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[i]);
			}
			else if (i == effectSpriteIndex)
			{
				rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
	}
}
