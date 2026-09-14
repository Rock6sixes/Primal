using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Watcher;

public class VoidSpawnGraphics : ComplexGraphicsModule
{
	public class Antenna : GraphicsSubModule
	{
		public Vector2[,] segments;

		public float conRad;

		public float thickness;

		public float rigid;

		public float forceDirection;

		public float ang;

		public int rigidSegments;

		public VoidSpawnGraphics vsGraphics => owner as VoidSpawnGraphics;

		public virtual Vector2 ResetPos => vsGraphics.spawn.mainBody[0].pos;

		public virtual Vector2 ResetDir => Custom.RNV();

		public Antenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection)
			: base(owner, firstSprite)
		{
			this.conRad = conRad;
			this.thickness = thickness;
			this.rigid = rigid;
			this.rigidSegments = rigidSegments;
			this.forceDirection = forceDirection;
			this.ang = ang;
			totalSprites = 1;
			segments = new Vector2[segs, 3];
		}

		public override void Update()
		{
			base.Update();
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 1] = segments[i, 0];
				segments[i, 0] += segments[i, 2];
				segments[i, 2] *= Custom.LerpMap(segments[i, 2].magnitude, 0.2f * vsGraphics.spawn.sizeFac, 6f * vsGraphics.spawn.sizeFac, 1f, 0.7f);
			}
			for (int j = 1; j < segments.GetLength(0); j++)
			{
				Vector2 vector = Custom.DirVec(segments[j, 0], segments[j - 1, 0]);
				float num = Vector2.Distance(segments[j, 0], segments[j - 1, 0]);
				Vector2 vector2 = vector * ((conRad - num) * 0.5f);
				segments[j, 0] -= vector2;
				segments[j, 2] -= vector2;
				segments[j - 1, 0] += vector2;
				segments[j - 1, 2] += vector2;
			}
			for (int k = 2; k < segments.GetLength(0); k++)
			{
				Vector2 vector3 = Custom.DirVec(segments[k, 0], segments[k - 2, 0]);
				segments[k, 2] -= vector3 * rigid;
				segments[k - 2, 2] += vector3 * rigid;
			}
		}

		public override void Reset()
		{
			base.Reset();
			Vector2 resetPos = ResetPos;
			Vector2 resetDir = ResetDir;
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 0] = resetPos + resetDir * conRad;
				segments[i, 1] = segments[i, 0];
				segments[i, 2] *= 0f;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: true);
			sLeaser.sprites[firstSprite].shader = vsGraphics.BodyShader;
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
			Vector2 vector2 = vector;
			if (this is TailAntenna)
			{
				vector2 = Vector2.Lerp(vsGraphics.spawn.bodyChunks[vsGraphics.spawn.bodyChunks.Length - 2].lastPos, vsGraphics.spawn.bodyChunks[vsGraphics.spawn.bodyChunks.Length - 2].pos, timeStacker);
				vector += Custom.DirVec(vector, vector2) * (Vector2.Distance(vector, vector2) / 5f * 2f + 1f);
			}
			else
			{
				vector += Custom.DirVec(Vector2.Lerp(segments[1, 1], segments[1, 0], timeStacker), vector) * (conRad * 0.3f);
			}
			float num = thickness;
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				float f = (float)i / (float)(segments.GetLength(0) - 1);
				Vector2 vector3 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
				Vector2 normalized = (vector3 - vector2).normalized;
				Vector2 vector4 = Custom.PerpendicularVector(normalized);
				float num2 = Vector2.Distance(vector3, vector2) / 5f;
				float num3 = Mathf.Lerp(thickness, 0.5f, Mathf.Pow(f, 0.2f));
				Vector2 vector5 = vector4 * ((num + num3) * 0.5f);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4, vector - vector5 + normalized * num2 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector5 + normalized * num2 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector3 - vector4 * num3 - normalized * num2 - camPos);
				(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector3 + vector4 * num3 - normalized * num2 - camPos);
				vector = vector3;
				vector2 = vector;
				num = num3;
			}
			for (int j = 0; j < (sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors.Length; j++)
			{
				(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[j] = new Color(vsGraphics.meshColor.r, vsGraphics.meshColor.g, vsGraphics.meshColor.b, vsGraphics.AlphaFromGlowDist((sLeaser.sprites[firstSprite] as TriangleMesh).vertices[j] + camPos, vsGraphics.glowPos));
			}
			sLeaser.sprites[firstSprite].shader = vsGraphics.BodyShader;
		}
	}

	public class FrontAntenna : Antenna
	{
		public float startLength;

		public override Vector2 ResetPos => base.vsGraphics.spawn.mainBody[0].pos;

		public override Vector2 ResetDir => Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.vsGraphics.spawn.mainBody[1].pos, base.vsGraphics.spawn.mainBody[0].pos) + ang);

		public FrontAntenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection)
			: base(owner, firstSprite, segs, conRad, thickness, ang, rigid, rigidSegments, forceDirection)
		{
		}

		public override void Update()
		{
			base.Update();
			segments[0, 2] *= 0f;
			Vector2 vector = Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.vsGraphics.spawn.mainBody[1].pos, base.vsGraphics.spawn.mainBody[0].pos) + ang);
			segments[0, 0] = base.vsGraphics.spawn.mainBody[0].pos + vector * startLength;
			for (int i = 1; i < segments.GetLength(0) && i < rigidSegments; i++)
			{
				segments[i, 2] += vector * (forceDirection * Mathf.InverseLerp(rigidSegments, 1f, i));
			}
		}
	}

	public class TailAntenna : Antenna
	{
		public override Vector2 ResetPos => base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 1].pos;

		public override Vector2 ResetDir => Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 2].pos, base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 1].pos) + ang);

		public TailAntenna(ComplexGraphicsModule owner, int firstSprite, int segs, float conRad, float thickness, float ang, float rigid, int rigidSegments, float forceDirection)
			: base(owner, firstSprite, segs, conRad, thickness, ang, rigid, rigidSegments, forceDirection)
		{
		}

		public override void Update()
		{
			base.Update();
			segments[0, 0] = base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 1].pos;
			segments[0, 2] *= 0f;
			Vector2 vector = Custom.DegToVec(Custom.AimFromOneVectorToAnother(base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 2].pos, base.vsGraphics.spawn.mainBody[base.vsGraphics.spawn.mainBody.Length - 1].pos) + ang);
			for (int i = 1; i < segments.GetLength(0) && i < rigidSegments; i++)
			{
				segments[i, 2] += vector * (forceDirection * Mathf.InverseLerp(rigidSegments, 1f, i));
			}
		}
	}

	public class VoidSpawnVis : CosmeticSprite
	{
		public VoidSpawn spawn;

		public VoidSpawnVis(VoidSpawn spawn)
		{
			this.spawn = spawn;
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			List<FSprite> list = new List<FSprite>();
			for (int i = 0; i < spawn.mainBody.Length; i++)
			{
				list.Add(new FSprite("Circle20")
				{
					color = Color.red,
					alpha = 0.5f,
					scale = spawn.mainBody[i].rad / 20f
				});
			}
			foreach (Antenna item in (spawn.graphicsModule as VoidSpawnGraphics).antennae)
			{
				for (int j = 0; j < item.segments.GetLength(0); j++)
				{
					list.Add(new FSprite("pixel")
					{
						color = Color.yellow
					});
				}
			}
			sLeaser.sprites = list.ToArray();
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("WarpPoint"));
			base.InitiateSprites(sLeaser, rCam);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			int num = 0;
			BodyChunk[] mainBody = spawn.mainBody;
			for (int i = 0; i < mainBody.Length; i++)
			{
				_ = mainBody[i];
				Vector2 vector = Vector2.Lerp(spawn.mainBody[num].lastPos, spawn.mainBody[num].pos, timeStacker);
				sLeaser.sprites[num].x = vector.x - camPos.x;
				sLeaser.sprites[num].y = vector.y - camPos.y;
				sLeaser.sprites[num].scale = spawn.mainBody[num].rad / 20f;
				num++;
			}
			foreach (Antenna item in (spawn.graphicsModule as VoidSpawnGraphics).antennae)
			{
				for (int j = 0; j < item.segments.GetLength(0); j++)
				{
					Vector2 vector2 = Vector2.Lerp(item.segments[j, 1], item.segments[j, 0], timeStacker);
					sLeaser.sprites[num].x = vector2.x - camPos.x;
					sLeaser.sprites[num].y = vector2.y - camPos.y;
					num++;
				}
			}
		}
	}

	public Vector2 glowPos;

	public float playerGlowVision;

	public float darkness;

	public List<Antenna> antennae;

	public float[,] playersGlowVision;

	public int chunkResolution = 1;

	public Color meshColor;

	public VoidSpawn spawn => base.owner as VoidSpawn;

	public bool dayLightMode => spawn.dayLightMode;

	public int BodyMeshSprite => 0;

	public int GlowSprite => 1;

	public int EffectSprite => 2;

	public bool RippleMigration => spawn.RippleMigration;

	public bool hasOwnGoldEffect => spawn.voidMeltInRoom == 0f;

	public bool rippleMode
	{
		get
		{
			if (spawn.room == null)
			{
				return spawn.rippleSpawn;
			}
			if (spawn.rippleSpawn)
			{
				return spawn.abstractPhysicalObject.rippleLayer == spawn.room.game.ActiveRippleLayer;
			}
			return false;
		}
	}

	public FShader BodyShader => Custom.rainWorld.Shaders[(!spawn.rippleSpawn) ? "VoidSpawnBody" : ((spawn.abstractPhysicalObject.rippleLayer == 0) ? "RippleSpawnBody" : "RippleSpawnBodyRippleSide")];

	public FShader EffectShader => Custom.rainWorld.Shaders[(!spawn.rippleSpawn) ? "GoldenGlow" : ((spawn.abstractPhysicalObject.rippleLayer == 0) ? "RippleGlow" : "RippleGlowRippleSide")];

	public FShader GlowShader => Custom.rainWorld.Shaders[(!spawn.rippleSpawn || RippleMigration) ? "FlatWaterLightBothSides" : ((spawn.abstractPhysicalObject.rippleLayer == 0) ? "FlatWaterLightRippleSpawn" : "FlatWaterLightRippleSpawnRippleSide")];

	public VoidSpawnGraphics(PhysicalObject owner)
		: base(owner, internalContainers: false)
	{
		playersGlowVision = new float[owner.abstractPhysicalObject.world.game.Players.Count, 2];
		totalSprites = (hasOwnGoldEffect ? 3 : 2);
		antennae = new List<Antenna>();
		float num = Mathf.Lerp(spawn.sizeFac, 0.5f + 0.5f * UnityEngine.Random.value, UnityEngine.Random.value);
		int num2;
		int segs;
		float num7;
		float num3;
		float forceDirection;
		float num5;
		if (spawn.variant == VoidSpawn.SpawnType.ElderRipple)
		{
			num = spawn.sizeFac;
			antennae.Add(new TailAntenna(this, totalSprites, 20, 5f * num, spawn.mainBody[spawn.mainBody.Length - 1].rad, 0f, 0.1f * num, 8, 3f));
			AddSubModule(antennae[antennae.Count - 1]);
		}
		else if (spawn.variant == VoidSpawn.SpawnType.RippleAmoeba)
		{
			antennae.Add(new TailAntenna(this, totalSprites, UnityEngine.Random.Range(4, 8), 12f * num, spawn.mainBody[spawn.mainBody.Length - 1].rad, 0f, 0.1f * num, 2, 2.2f));
			AddSubModule(antennae[antennae.Count - 1]);
		}
		else if (spawn.variant == VoidSpawn.SpawnType.RippleJelly)
		{
			num2 = UnityEngine.Random.Range(6, 8);
			num3 = Mathf.Lerp(Mathf.Lerp(8f, 24f, UnityEngine.Random.value) * (float)num2, Mathf.Lerp(16f, 45f, UnityEngine.Random.value), UnityEngine.Random.value);
			segs = UnityEngine.Random.Range(4, 14);
			float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
			for (int i = 0; i < num2; i++)
			{
				float num4 = (float)i / (float)(num2 - 1);
				antennae.Add(new TailAntenna(this, totalSprites, segs, 12f * num * Mathf.Lerp(a, 1f, Mathf.Sin(num4 * (float)Math.PI)), spawn.mainBody[spawn.mainBody.Length - 1].rad, Mathf.Lerp(0f - num3, num3, num4), 0.1f * num, 2, 2.2f));
				AddSubModule(antennae[antennae.Count - 1]);
			}
		}
		else
		{
			switch (UnityEngine.Random.Range(0, 3))
			{
			case 0:
				antennae.Add(new TailAntenna(this, totalSprites, UnityEngine.Random.Range(3, 18), 12f * num, spawn.mainBody[spawn.mainBody.Length - 1].rad, 0f, 0.1f * num, 2, 2.2f));
				AddSubModule(antennae[antennae.Count - 1]);
				break;
			case 1:
			{
				num2 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(2, 5));
				num3 = Mathf.Lerp(Mathf.Lerp(2f, 15f, UnityEngine.Random.value) * (float)num2, Mathf.Lerp(8f, 70f, UnityEngine.Random.value), UnityEngine.Random.value);
				segs = UnityEngine.Random.Range(3, 18);
				float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
				for (int k = 0; k < num2; k++)
				{
					float num9 = (float)k / (float)(num2 - 1);
					antennae.Add(new TailAntenna(this, totalSprites, segs, 12f * num * Mathf.Lerp(a, 1f, Mathf.Sin(num9 * (float)Math.PI)), spawn.mainBody[spawn.mainBody.Length - 1].rad, Mathf.Lerp(0f - num3, num3, num9), 0.1f * num, 2, 2.2f));
					AddSubModule(antennae[antennae.Count - 1]);
				}
				break;
			}
			case 2:
			{
				num2 = UnityEngine.Random.Range(2, 6);
				num5 = Mathf.Lerp(0.1f, 1.8f, UnityEngine.Random.value);
				num3 = Mathf.Lerp(Mathf.Lerp(2f, 15f, UnityEngine.Random.value) * (float)num2, Mathf.Lerp(8f, 70f, UnityEngine.Random.value), UnityEngine.Random.value);
				segs = UnityEngine.Random.Range(3, UnityEngine.Random.Range(5, 8));
				int num6 = UnityEngine.Random.Range(1, segs + 1);
				forceDirection = Mathf.Lerp(1.5f, 7f, UnityEngine.Random.value) * num5 / Mathf.Lerp(1f, num6, 0.5f);
				num7 = Mathf.Lerp(4f, 12f, UnityEngine.Random.value) * num;
				float a = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value);
				for (int j = 0; j < num2; j++)
				{
					float num8 = (float)j / (float)(num2 - 1);
					antennae.Add(new TailAntenna(this, totalSprites, segs, num7 * Mathf.Lerp(a, 1f, Mathf.Sin(num8 * (float)Math.PI)), 2f, Mathf.Lerp(0f - num3, num3, num8), num5, num6, forceDirection));
					AddSubModule(antennae[antennae.Count - 1]);
				}
				break;
			}
			}
		}
		num2 = UnityEngine.Random.Range(2, UnityEngine.Random.Range(2, 7));
		segs = UnityEngine.Random.Range(2, UnityEngine.Random.Range(4, (int)Custom.LerpMap(spawn.mainBody.Length, 3f, 16f, 6f, 12f, 0.5f)));
		int num10 = segs;
		if (UnityEngine.Random.value < 0.5f)
		{
			num10 = UnityEngine.Random.Range(1, segs + 1);
		}
		num7 = Mathf.Lerp(3f, 8f, UnityEngine.Random.value);
		num3 = Mathf.Lerp(12f, 50f, Mathf.Pow(UnityEngine.Random.value, 1.5f));
		forceDirection = Mathf.Lerp(2f, 7f, UnityEngine.Random.value) / Mathf.Lerp(1f, num10, 0.5f);
		num5 = Mathf.Lerp(0.4f, 2.2f, UnityEngine.Random.value);
		if (spawn.variant == VoidSpawn.SpawnType.ElderRipple)
		{
			num2 = 4;
			segs = 40;
			num10 = 10;
			num3 = 35f;
			num5 = 1.2f;
			forceDirection = 5f;
			chunkResolution = 5;
		}
		if (spawn.variant != VoidSpawn.SpawnType.RippleJelly)
		{
			for (int l = 0; l < num2; l++)
			{
				float t = (float)l / (float)(num2 - 1);
				antennae.Add(new FrontAntenna(this, totalSprites, segs, num7, 2f * num, Mathf.Lerp(0f - num3, num3, t), num5, num10, forceDirection));
				AddSubModule(antennae[antennae.Count - 1]);
				if (spawn.variant == VoidSpawn.SpawnType.ElderRipple)
				{
					(antennae[antennae.Count - 1] as FrontAntenna).startLength = 20f;
				}
			}
		}
		Reset();
	}

	public override void Update()
	{
		if (!spawn.culled)
		{
			base.Update();
		}
		if (base.owner.room == null)
		{
			return;
		}
		for (int i = 0; i < base.owner.room.game.Players.Count; i++)
		{
			playersGlowVision[i, 1] = playersGlowVision[i, 0];
			float num = 0f;
			bool flag = false;
			AbstractCreature abstractCreature = base.owner.room.game.Players[i];
			if (abstractCreature.realizedCreature != null && abstractCreature.Room == base.owner.abstractPhysicalObject.Room && (base.owner.room.game.setupValues.playerGlowing || (base.owner.room.game.IsStorySession && base.owner.room.game.GetStorySession.saveState.CanSeeVoidSpawn) || (abstractCreature.realizedCreature as Player).devMaxLevelRipple))
			{
				if (spawn.egg != null && spawn.egg.holdingSpawn && spawn.egg.RippleEggHidden)
				{
					flag = true;
					if (spawn.egg.rippleRevealedFromFlower)
					{
						num = 0.75f;
					}
					if ((num != 1f && (abstractCreature.realizedCreature as Player).KarmaIsReinforced) || (abstractCreature.realizedCreature as Player).rippleSpawnEggReveal > 0f)
					{
						num = Mathf.Clamp01((abstractCreature.realizedCreature as Player).rippleSpawnEggReveal / 400f);
					}
				}
				else
				{
					num = 1f;
				}
			}
			if (playersGlowVision[i, 0] < num)
			{
				playersGlowVision[i, 0] = Custom.LerpAndTick(playersGlowVision[i, 0], num, 0.025f, 1f / 60f);
			}
			else if (flag)
			{
				playersGlowVision[i, 0] = Custom.LerpAndTick(playersGlowVision[i, 0], num, 0.025f, 1f / 60f);
			}
			else
			{
				playersGlowVision[i, 0] = Custom.LerpAndTick(playersGlowVision[i, 0], num, 0.1f, 1f / 3f);
			}
		}
		if (!(spawn.behavior is WatcherRoomSpecificScript.WORA_ElderSpawn.ElderBehavior { closeIn: not false } elderBehavior) || !(elderBehavior.orbitAngle < elderBehavior.closeInEnd))
		{
			return;
		}
		foreach (Antenna item in antennae)
		{
			if (item is FrontAntenna frontAntenna)
			{
				frontAntenna.rigid = Custom.LerpAndTick(frontAntenna.rigid, 0f, 0.02f, 0.01f);
				if (frontAntenna.rigidSegments > 0)
				{
					frontAntenna.rigidSegments = Mathf.Min(frontAntenna.rigidSegments, (int)Custom.LerpMap(elderBehavior.orbitAngle, elderBehavior.closeInEnd, elderBehavior.closeInEnd - 720f, 20f, 0f));
				}
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites[BodyMeshSprite] = TriangleMesh.MakeLongMesh(spawn.mainBody.Length * chunkResolution, pointyTip: false, customColor: true);
		sLeaser.sprites[BodyMeshSprite].shader = BodyShader;
		sLeaser.sprites[GlowSprite] = new FSprite("Futile_White");
		sLeaser.sprites[GlowSprite].shader = GlowShader;
		if (hasOwnGoldEffect)
		{
			sLeaser.sprites[EffectSprite] = new FSprite("Futile_White");
			sLeaser.sprites[EffectSprite].shader = EffectShader;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (rCam.followAbstractCreature != null && rCam.followAbstractCreature.realizedCreature != null && rCam.followAbstractCreature.realizedCreature is Player player)
		{
			if (player.abstractCreature.Room == base.owner.abstractPhysicalObject.Room)
			{
				if (player.inShortcut)
				{
					try
					{
						glowPos = player.inShortcutVessel.SmoothPos(timeStacker);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				else
				{
					glowPos = Vector2.Lerp(rCam.followAbstractCreature.realizedCreature.mainBodyChunk.lastPos, rCam.followAbstractCreature.realizedCreature.mainBodyChunk.pos, timeStacker);
				}
			}
			playerGlowVision = Mathf.Lerp(playersGlowVision[(rCam.followAbstractCreature.realizedCreature as Player).playerState.playerNumber, 1], playersGlowVision[(rCam.followAbstractCreature.realizedCreature as Player).playerState.playerNumber, 0], timeStacker) * Mathf.Lerp(spawn.lastFade, spawn.fade, timeStacker);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = Vector2.Lerp(spawn.mainBody[0].lastPos, spawn.mainBody[0].pos, timeStacker);
		if (!spawn.culled)
		{
			sLeaser.sprites[BodyMeshSprite].shader = BodyShader;
			if (hasOwnGoldEffect)
			{
				sLeaser.sprites[EffectSprite].shader = EffectShader;
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = true;
			}
			Vector2 a = vector;
			if (spawn.inEggMode > 0f && spawn.egg != null)
			{
				a = Vector2.Lerp(a, Vector2.Lerp(spawn.egg.lastPos, spawn.egg.pos, timeStacker), spawn.inEggMode);
			}
			if (dayLightMode)
			{
				meshColor = new Color(1f - AlphaFromGlowDist(a, glowPos) * 0.5f, 0.5f, 0f);
			}
			else if (RippleMigration)
			{
				meshColor = new Color(0.7f + 0.3f * Mathf.InverseLerp(0.9f, 0.1f, darkness), Mathf.Lerp(Mathf.Lerp(Custom.LerpMap(darkness, 0.9f, 0.1f, 0.1f, 0.075f, 0.5f), 0.24f, spawn.inEggMode), 0.8f, spawn.voidMeltInRoom), 0f);
			}
			else
			{
				meshColor = new Color(0.7f + 0.3f * Mathf.InverseLerp(0.1f, 0.9f, darkness), Mathf.Lerp(Mathf.Lerp(Custom.LerpMap(darkness, 0.1f, 0.9f, 0.1f, 0.075f, 0.5f), 0.24f, spawn.inEggMode), 0.8f, spawn.voidMeltInRoom), 0f);
			}
			UpdateGlowSpriteColor(sLeaser);
			sLeaser.sprites[GlowSprite].x = a.x - camPos.x;
			sLeaser.sprites[GlowSprite].y = a.y - camPos.y;
			if (spawn.inEggMode > 0.9f && spawn.egg != null)
			{
				sLeaser.sprites[GlowSprite].scale = Mathf.Lerp(2.8f * spawn.TotalMass * Mathf.Lerp(0.6f, 1f, darkness), spawn.egg.rad / 4f, Mathf.Lerp(0.9f, 1f, spawn.inEggMode));
			}
			else
			{
				sLeaser.sprites[GlowSprite].scale = 2.8f * spawn.TotalMass * Mathf.Lerp(0.6f, 1f, darkness);
			}
			if (dayLightMode)
			{
				sLeaser.sprites[GlowSprite].alpha = Mathf.Pow(AlphaFromGlowDist(a, glowPos), 0.5f) * Mathf.Lerp(0.5f, 1f, spawn.inEggMode);
			}
			else if (RippleMigration)
			{
				sLeaser.sprites[GlowSprite].alpha = AlphaFromGlowDist(a, glowPos) * Mathf.Lerp(Mathf.Lerp(0.4f, 0.1f, Mathf.Pow(darkness, 2f)), 1f, spawn.voidMeltInRoom * 0.5f) * 0.4f;
			}
			else
			{
				sLeaser.sprites[GlowSprite].alpha = AlphaFromGlowDist(a, glowPos) * Mathf.Lerp(Mathf.Lerp(0.1f, 0.4f, Mathf.Pow(darkness, 2f)), 1f, spawn.voidMeltInRoom * 0.5f);
			}
			sLeaser.sprites[GlowSprite].shader = GlowShader;
			if (hasOwnGoldEffect)
			{
				sLeaser.sprites[EffectSprite].x = a.x - camPos.x;
				sLeaser.sprites[EffectSprite].y = a.y - camPos.y;
				sLeaser.sprites[EffectSprite].scale = 6f * spawn.TotalMass;
				if (spawn.variant == VoidSpawn.SpawnType.ElderRipple)
				{
					sLeaser.sprites[EffectSprite].scale = 2f * spawn.TotalMass;
				}
				if (dayLightMode)
				{
					sLeaser.sprites[EffectSprite].alpha = Mathf.Pow(AlphaFromGlowDist(a, glowPos), 0.5f) * 0.5f;
				}
				else if (RippleMigration)
				{
					sLeaser.sprites[EffectSprite].alpha = Mathf.Pow(AlphaFromGlowDist(a, glowPos), 0.6f) * Custom.LerpMap(darkness, 0.9f, 0.1f, 0.3f, 1f, 1.5f) * 0.4f;
				}
				else
				{
					sLeaser.sprites[EffectSprite].alpha = Mathf.Pow(AlphaFromGlowDist(a, glowPos), 0.6f) * Custom.LerpMap(darkness, 0.1f, 0.9f, 0.3f, 1f, 1.5f);
				}
			}
			vector += Custom.DirVec(Vector2.Lerp(spawn.mainBody[1].lastPos, spawn.mainBody[1].pos, timeStacker), vector) * spawn.mainBody[0].rad;
			float num = spawn.mainBody[0].rad / 2f;
			if (spawn.variant == VoidSpawn.SpawnType.ElderRipple)
			{
				num = 0f;
			}
			if (chunkResolution == 1)
			{
				for (int j = 0; j < spawn.mainBody.Length; j++)
				{
					Vector2 vector2 = Vector2.Lerp(spawn.mainBody[j].lastPos, spawn.mainBody[j].pos, timeStacker);
					Vector2 normalized = (vector2 - vector).normalized;
					Vector2 vector3 = Custom.PerpendicularVector(normalized);
					float num2 = Vector2.Distance(vector2, vector) / 5f;
					float rad = spawn.mainBody[j].rad;
					Vector2 vector4 = vector3 * ((num + rad) * 0.5f);
					(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4, vector - vector4 + normalized * num2 - camPos);
					(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector + vector4 + normalized * num2 - camPos);
					(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - vector3 * rad - normalized * num2 - camPos);
					(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + vector3 * rad - normalized * num2 - camPos);
					vector = vector2;
					num = rad;
				}
			}
			else
			{
				Vector2[,] array = new Vector2[spawn.mainBody.Length + 1, 2];
				Vector2[,] array2 = new Vector2[spawn.mainBody.Length + 1, 2];
				Vector2 b = Vector2.zero;
				Vector2 b2 = Vector2.zero;
				for (int k = 0; k < spawn.mainBody.Length; k++)
				{
					Vector2 vector5 = Vector2.Lerp(spawn.mainBody[k].lastPos, spawn.mainBody[k].pos, timeStacker);
					Vector2 normalized2 = (vector5 - vector).normalized;
					Vector2 vector6 = Custom.PerpendicularVector(normalized2);
					float num3 = Vector2.Distance(vector5, vector) / 5f;
					float rad2 = spawn.mainBody[k].rad;
					if (k == 0)
					{
						Vector2 vector7 = vector6 * ((num + rad2) * 0.5f);
						array[k, 0] = vector - vector7 + normalized2 * num3 - camPos;
						array[k, 1] = vector + vector7 + normalized2 * num3 - camPos;
					}
					array[k + 1, 0] = vector5 - vector6 * rad2 - normalized2 * num3 - camPos;
					array[k + 1, 1] = vector5 + vector6 * rad2 - normalized2 * num3 - camPos;
					Vector2 normalized3 = (array[k + 1, 0] - array[k, 0]).normalized;
					Vector2 normalized4 = (array[k + 1, 1] - array[k, 1]).normalized;
					float num4 = 2f;
					array2[k, 0] = Vector2.Lerp(normalized3, b, 0.5f) * (num3 * num4);
					array2[k, 1] = Vector2.Lerp(normalized4, b2, 0.5f) * (num3 * num4);
					if (k == spawn.mainBody.Length - 1)
					{
						array2[k + 1, 0] = Vector2.Lerp(normalized3, b, 0.5f);
						array2[k + 1, 1] = Vector2.Lerp(normalized4, b2, 0.5f);
					}
					vector = vector5;
					num = rad2;
					b = normalized3;
					b2 = normalized4;
				}
				for (int l = 0; l < spawn.mainBody.Length; l++)
				{
					BezierCurve bezierCurve = new BezierCurve(array[l, 0], array[l, 0] + array2[l, 0], array[l + 1, 0], array[l + 1, 0] - array2[l + 1, 0]);
					BezierCurve bezierCurve2 = new BezierCurve(array[l, 1], array[l, 1] + array2[l, 1], array[l + 1, 1], array[l + 1, 1] - array2[l + 1, 1]);
					for (int m = 0; m < chunkResolution * 2; m++)
					{
						float f = (float)m / (float)(chunkResolution * 2);
						(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(l * 4 * chunkResolution + m * 2, bezierCurve.GetPoint(f));
						(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).MoveVertice(l * 4 * chunkResolution + m * 2 + 1, bezierCurve2.GetPoint(f));
					}
				}
			}
			foreach (Antenna item in antennae)
			{
				if (item is TailAntenna tailAntenna && tailAntenna.segments[0, 1] != Vector2.zero)
				{
					TriangleMesh triangleMesh = sLeaser.sprites[tailAntenna.firstSprite] as TriangleMesh;
					if (!(triangleMesh.vertices[0] == Vector2.zero) || !(triangleMesh.vertices[1] == Vector2.zero))
					{
						TriangleMesh triangleMesh2 = sLeaser.sprites[BodyMeshSprite] as TriangleMesh;
						triangleMesh.MoveVertice(0, triangleMesh2.vertices[triangleMesh2.vertices.Length - 2]);
						triangleMesh.MoveVertice(1, triangleMesh2.vertices[triangleMesh2.vertices.Length - 1]);
					}
				}
			}
			for (int n = 0; n < (sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors.Length; n++)
			{
				(sLeaser.sprites[BodyMeshSprite] as TriangleMesh).verticeColors[n] = new Color(meshColor.r, meshColor.g, meshColor.b, AlphaFromGlowDist((sLeaser.sprites[BodyMeshSprite] as TriangleMesh).vertices[n] + camPos, glowPos));
			}
			if (spawn.behavior is WatcherRoomSpecificScript.WORA_ElderSpawn.ElderBehavior { closeIn: not false } elderBehavior && elderBehavior.orbitAngle < elderBehavior.collapseAnimStart)
			{
				FSprite[] sprites = sLeaser.sprites;
				for (int num5 = 0; num5 < sprites.Length; num5++)
				{
					if (sprites[num5] is TriangleMesh mesh)
					{
						CollapseAnimation(mesh, WatcherRoomSpecificScript.WORA_ElderSpawn.orbitCenter - camPos);
					}
				}
				sLeaser.sprites[GlowSprite].scale *= Mathf.InverseLerp(elderBehavior.collapseAnimFinish, elderBehavior.collapseAnimStart, elderBehavior.orbitAngle);
				if (hasOwnGoldEffect)
				{
					sLeaser.sprites[EffectSprite].scale *= Mathf.InverseLerp(elderBehavior.collapseAnimFinish, elderBehavior.collapseAnimStart, elderBehavior.orbitAngle);
				}
			}
		}
		else
		{
			for (int num6 = 0; num6 < sLeaser.sprites.Length; num6++)
			{
				sLeaser.sprites[num6].isVisible = false;
			}
		}
		MapExporter.VoidSpawnGraphics_DrawSprites(this, sLeaser);
	}

	public void CollapseAnimation(TriangleMesh mesh, Vector2 center)
	{
		if (spawn.behavior is WatcherRoomSpecificScript.WORA_ElderSpawn.ElderBehavior elderBehavior)
		{
			int num = (int)Mathf.Abs(elderBehavior.orbitAngle - elderBehavior.collapseAnimStart);
			for (int i = 0; i < mesh.vertices.Length; i++)
			{
				float num2 = Vector2.Distance(mesh.vertices[i], center);
				mesh.MoveVertice(i, Vector2.Lerp(mesh.vertices[i], center, Mathf.Pow(Mathf.InverseLerp(num2, num2 * 3f, num), 2f)));
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		darkness = palette.darkness;
		UpdateGlowSpriteColor(sLeaser);
	}

	public void UpdateGlowSpriteColor(RoomCamera.SpriteLeaser sLeaser)
	{
		if (rippleMode)
		{
			sLeaser.sprites[GlowSprite].color = RainWorld.RippleColor;
		}
		else if (dayLightMode)
		{
			sLeaser.sprites[GlowSprite].color = RainWorld.SaturatedGold;
		}
		else
		{
			sLeaser.sprites[GlowSprite].color = Color.Lerp(RainWorld.SaturatedGold, RainWorld.GoldRGB, spawn.rippleSpawn ? 1f : Mathf.InverseLerp(0.3f, 0.9f, darkness));
		}
	}

	public bool VisibleAtGlowDist(Vector2 A, Vector2 B, float margin)
	{
		if (playerGlowVision == 0f)
		{
			return false;
		}
		float num = (spawn.rippleSpawn ? 1.5f : 1f);
		if (spawn.rippleSpawn && Vector2.Distance(A, B) < (100f + 400f * num + margin) / 2f)
		{
			return true;
		}
		return Custom.DistLess(A, B, Mathf.Lerp(100f, 400f * num, playerGlowVision) + margin);
	}

	public float AlphaFromGlowDist(Vector2 A, Vector2 B)
	{
		float num = 1f;
		foreach (PlacedObject.HideVoidSpawnData hideZone in spawn.hideZones)
		{
			float num2 = Vector2.Distance(A, hideZone.owner.pos);
			if (num2 < hideZone.Rad)
			{
				num = Mathf.Min(1f - Mathf.InverseLerp(hideZone.Rad, hideZone.Rad - 20f, num2), num);
			}
		}
		if (RippleMigration)
		{
			num *= 0.4f;
		}
		float num3 = Vector2.Distance(A, B);
		float num4 = (spawn.rippleSpawn ? 1.5f : 1f);
		bool flag = spawn.egg != null && spawn.egg.holdingSpawn && spawn.egg.rippleRevealedFromFlower;
		if (flag)
		{
			num4 = 0.75f;
		}
		if (spawn.rippleSpawn && !flag && num3 < (100f + 400f * num4) / 2f)
		{
			return playerGlowVision * num;
		}
		if (spawn.inEggMode > 0f && !flag)
		{
			return Mathf.Lerp(Mathf.Sin(Mathf.InverseLerp(Mathf.Lerp(100f, 400f * num4, playerGlowVision), 50f, num3) * (float)Math.PI), Mathf.InverseLerp(Mathf.Lerp(100f, 400f * num4, playerGlowVision), 50f, num3), spawn.inEggMode) * playerGlowVision * num;
		}
		return Mathf.Sin(Mathf.InverseLerp(Mathf.Lerp(100f, 400f * num4, playerGlowVision), 50f, num3) * (float)Math.PI) * playerGlowVision * num;
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
			if (i == GlowSprite)
			{
				rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[i]);
			}
			else if (i == EffectSprite && hasOwnGoldEffect)
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
