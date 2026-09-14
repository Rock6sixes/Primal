using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Watcher.TardigradeCosmetics;

namespace Watcher;

public class TardigradeGraphics : GraphicsModule, IMuddableGraphics
{
	public class Spine
	{
		public Vector2[] chunkPositions;

		public Vector2[] handles;

		public Spine(TardigradeGraphics tGraphics, float timeStacker)
		{
			chunkPositions = new Vector2[tGraphics.bunny.bodyChunks.Length];
			for (int i = 0; i < chunkPositions.Length; i++)
			{
				chunkPositions[i] = Vector2.Lerp(tGraphics.bunny.bodyChunks[i].lastPos, tGraphics.bunny.bodyChunks[i].pos, timeStacker);
			}
			Vector2 vector = (chunkPositions[0] + chunkPositions[2]) * 0.5f;
			Vector2 vector2 = (chunkPositions[1] + chunkPositions[3]) * 0.5f;
			handles = new Vector2[2];
			handles[0] = vector + (chunkPositions[1] - vector) * 2f;
			handles[1] = vector2 + (chunkPositions[2] - vector2) * 2f;
		}

		public SpineData GetSpineData(float f)
		{
			Vector2 vector = Custom.Bezier(chunkPositions[0], handles[0], chunkPositions[3], handles[1], f);
			Vector2 vector2 = ((!(f - 0.05f <= 0f)) ? Custom.DirVec(vector, Custom.Bezier(chunkPositions[0], handles[0], chunkPositions[3], handles[1], f - 0.05f)) : Custom.DirVec(chunkPositions[1], chunkPositions[0]));
			return new SpineData(vector, vector2, Custom.PerpendicularVector(vector2));
		}
	}

	public struct SpineData
	{
		public Vector2 pos;

		public Vector2 dir;

		public Vector2 perp;

		public SpineData(Vector2 pos, Vector2 dir, Vector2 perp)
		{
			this.pos = pos;
			this.dir = dir;
			this.perp = perp;
		}
	}

	public Tardigrade bunny;

	public TardigradeLimb[,] legs;

	public int totalExtraSprites;

	public List<Template> cosmetics;

	public Color bodyColor;

	public Color blackColor;

	public Color secondaryColor;

	public float visualDryness;

	public int AfterLegs => legs.GetLength(0);

	public int BodyMesh => AfterLegs + bunny.bodyChunks.Length;

	public int TotalBetween => bunny.bodyChunks.Length + 2;

	public int StartOfExtraSprites => legs.GetLength(0) + TotalBetween + legs.GetLength(0);

	public int TotalSprites => legs.GetLength(0) + TotalBetween + legs.GetLength(0) + totalExtraSprites;

	public int LegMesh(int pair, int side)
	{
		return side * (legs.GetLength(0) + TotalBetween) + pair;
	}

	public TardigradeGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		bunny = ow as Tardigrade;
		int[] array = new int[4] { 1, 2, 2, 3 };
		legs = new TardigradeLimb[bunny.bodyChunks.Length, 2];
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			for (int j = 0; j < legs.GetLength(1); j++)
			{
				legs[i, j] = new TardigradeLimb(this, bunny.bodyChunks[array[i]], i, 1f, 0.5f, 0.98f, 10f, 0.8f, j);
				if (i > 0)
				{
					legs[i - 1, (j == 0) ? 1u : 0u].legToTrigger = legs[i, j];
				}
			}
		}
		cosmetics = new List<Template>();
		int startOfExtraSprites = StartOfExtraSprites;
		Template.FloatCurve floatCurve = null;
		startOfExtraSprites += AddCosmetic(new Spikes(spikeLengthsCurve: (!bunny.IsSlayer) ? ((Template.FloatCurve)new Template.FlatOneCurve()) : ((Template.FloatCurve)new Template.SlayerSpineCurve()), tGraphics: this, startSprite: startOfExtraSprites, startPoint: bunny.IsSlayer ? 0.3f : 0.35f, endPoint: 1f, wrap: 0.55f, length: 10f + bunny.iVars.spikeLength * 35f, width: 1.5f + bunny.iVars.spikeWidth * 3.7f, layBack: bunny.iVars.spikeLayBack, puffOut: bunny.iVars.spikePuffOut, rows: 2, numberInRow: bunny.iVars.spikesInRow, spikeCurve: new Template.SharpCurve()));
		startOfExtraSprites += AddCosmetic(new Spikes(this, startOfExtraSprites, 0f, 0.125f, 0.7f, 10f + bunny.iVars.earLength * 17f, 1.5f + bunny.iVars.earWidth * 2.5f, 0f, 0f, bunny.IsSlayer ? 4 : 2, 1, new Template.FlatOneCurve(), new Template.RoundedCurve()));
	}

	public int AddCosmetic(Template cosmetic)
	{
		cosmetics.Add(cosmetic);
		totalExtraSprites += cosmetic.numberOfSprites;
		return cosmetic.numberOfSprites;
	}

	public override void Update()
	{
		base.Update();
		Vector2 a = Custom.ClosestPointOnLine(legs[0, 0].absoluteHuntPos, legs[0, 1].absoluteHuntPos, bunny.bodyTrail[1]);
		if (legs[0, 0].reachedSnapPosition && legs[0, 1].reachedSnapPosition && Vector2.Distance(a, Vector2.Lerp(legs[0, 0].absoluteHuntPos, legs[0, 1].absoluteHuntPos, 0.5f)) > Vector2.Distance(legs[0, 0].absoluteHuntPos, legs[0, 1].absoluteHuntPos) * 0.5f)
		{
			Vector2 vector = Custom.DirVec(bunny.bodyTrail[2], bunny.bodyTrail[1]);
			Vector2 b = bunny.bodyTrail[1] + vector * 100f;
			int num = 1;
			if (Vector2.Distance(legs[0, 0].absoluteHuntPos, b) > Vector2.Distance(legs[0, 1].absoluteHuntPos, b))
			{
				num = 0;
			}
			FindGrip(0, num, (Vector2.Dot(vector, Custom.DirVec(legs[0, num].absoluteHuntPos, bunny.bodyTrail[1])) > 0f) ? 1f : (-1f));
		}
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			for (int j = 0; j < legs.GetLength(1); j++)
			{
				if (bunny.Consious)
				{
					if (legs[i, j].mode == Limb.Mode.HuntRelativePosition || Vector2.Distance(legs[i, j].absoluteHuntPos, bunny.bodyTrail[1 + i / 2]) > 40f * bunny.scale)
					{
						FindGrip(i, j, Random.value);
					}
				}
				else
				{
					legs[i, j].mode = Limb.Mode.Dangle;
					legs[i, j].pos = Vector2.Lerp(legs[i, j].pos, LimbConnectionPoint(i, j), bunny.dryness * (bunny.Dried ? 0.99f : 0.2f));
				}
				legs[i, j].Update();
				legs[i, j].ConnectToPoint(LimbConnectionPoint(i, j), 40f * bunny.scale * (bunny.Consious ? 1f : 0.5f), push: false, 0f, legs[i, j].connection.vel, 1f, 0.5f);
			}
		}
		Spine spine = new Spine(this, 1f);
		for (int k = 0; k < cosmetics.Count; k++)
		{
			cosmetics[k].Update(spine);
		}
	}

	public Vector2 LimbConnectionPoint(int pair, int side)
	{
		float num = (1f - Mathf.Abs(bunny.flip)) * ((bunny.flip > 0f) ? (-1f) : 1f);
		Vector2 vector = Custom.PerpendicularVector(Custom.DirVec(bunny.bodyChunks[legs[pair, side].connection.index].pos, bunny.bodyChunks[legs[pair, side].connection.index - 1].pos)) * (num * ((side == 0) ? (-1f) : 1f) * 8f * bunny.scale);
		return pair switch
		{
			1 => Vector2.Lerp(bunny.bodyChunks[1].pos, bunny.bodyChunks[2].pos, 0.7f) + vector, 
			2 => Vector2.Lerp(bunny.bodyChunks[2].pos, bunny.bodyChunks[3].pos, 0.5f) + vector, 
			_ => legs[pair, side].connection.pos + vector, 
		};
	}

	public override void Reset()
	{
		base.Reset();
		for (int i = 0; i < cosmetics.Count; i++)
		{
			cosmetics[i].Reset();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		for (int i = 0; i < bunny.bodyChunks.Length; i++)
		{
			sLeaser.sprites[AfterLegs + i] = new FSprite("Circle20");
			sLeaser.sprites[AfterLegs + i].scale = bunny.bodyChunks[i].rad * 0.1f;
		}
		sLeaser.sprites[BodyMesh] = TriangleMesh.MakeLongMesh(6, pointyTip: false, customColor: false);
		sLeaser.sprites[BodyMesh + 1] = new FSprite("pixel");
		sLeaser.sprites[BodyMesh + 1].scale = 2f;
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				int num = LegMesh(j, k);
				sLeaser.sprites[num] = TriangleMesh.MakeLongMesh(4, pointyTip: false, customColor: true);
			}
		}
		for (int l = 0; l < cosmetics.Count; l++)
		{
			cosmetics[l].InitiateSprites(sLeaser, rCam);
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		if (visualDryness != bunny.dryness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			visualDryness = bunny.dryness;
		}
		float num = (1f - Mathf.Abs(bunny.flip)) * ((bunny.flip > 0f) ? (-1f) : 1f);
		Spine spine = new Spine(this, timeStacker);
		for (int i = 0; i < spine.chunkPositions.Length; i++)
		{
			sLeaser.sprites[AfterLegs + i].x = spine.chunkPositions[i].x - camPos.x;
			sLeaser.sprites[AfterLegs + i].y = spine.chunkPositions[i].y - camPos.y;
		}
		sLeaser.sprites[AfterLegs].isVisible = bunny.BitesLeft > 2;
		sLeaser.sprites[AfterLegs + 1].isVisible = bunny.BitesLeft > 1;
		sLeaser.sprites[AfterLegs + 1].isVisible = bunny.BitesLeft > 1;
		float num2 = 8f * bunny.scale;
		Vector2 vector = spine.chunkPositions[3] + Custom.DirVec(spine.chunkPositions[2], spine.chunkPositions[3]);
		float num3 = (float)bunny.BitesLeft / 3f;
		for (int j = 0; j < 6; j++)
		{
			float f = 1f - Mathf.InverseLerp(0f, 5f, j) * num3;
			Vector2 vector2 = Custom.Bezier(spine.chunkPositions[0], spine.handles[0], spine.chunkPositions[3], spine.handles[1], f);
			Vector2 vector3 = Custom.PerpendicularVector(vector2, vector);
			float num4 = 8f * bunny.scale;
			Vector2 vector4 = vector3 * ((num4 + num2) * 0.5f);
			Vector2 vector5 = vector3 * num4;
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4, (vector + vector2) / 2f - vector4 - camPos);
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 1, (vector + vector2) / 2f + vector4 - camPos);
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - vector5 - camPos);
			(sLeaser.sprites[BodyMesh] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + vector5 - camPos);
			vector = vector2;
			num2 = num4;
		}
		Vector2 vector6 = spine.chunkPositions[0] + Custom.PerpendicularVector(Custom.DirVec(spine.chunkPositions[1], spine.chunkPositions[0])) * (8f * num * bunny.scale);
		sLeaser.sprites[BodyMesh + 1].x = vector6.x - camPos.x;
		sLeaser.sprites[BodyMesh + 1].y = vector6.y - camPos.y;
		sLeaser.sprites[BodyMesh + 1].color = (rCam.room.PointSubmerged(vector6 + new Vector2(0f, -5f)) ? new Color(0f, 0.003921569f, 0f) : blackColor);
		sLeaser.sprites[BodyMesh + 1].isVisible = bunny.BitesLeft > 2;
		for (int k = 0; k < legs.GetLength(0); k++)
		{
			Vector2 vector7 = Custom.DirVec(spine.chunkPositions[legs[k, 0].connection.index], spine.chunkPositions[legs[k, 0].connection.index - 1]);
			for (int l = 0; l < legs.GetLength(1); l++)
			{
				int num5 = LegMesh(k, l);
				Vector2 vector8 = LimbConnectionPoint(k, l);
				Vector2 vector9 = Vector2.Lerp(legs[k, l].lastPos, legs[k, l].pos, timeStacker);
				float num6 = Mathf.Pow(Mathf.Min(Vector2.Distance(vector8, vector9) / 70f * bunny.scale, 1f), 2f);
				Vector2 v = Custom.DirVec(vector8, vector9);
				float num7 = Vector2.Distance(vector9, vector8 - vector7 * (10f * bunny.scale));
				float num8 = Vector2.Distance(vector9, vector8 + vector7 * (10f * bunny.scale));
				float num9 = num7 / Mathf.Max(1f, num7 + num8) * 2f - 1f;
				Vector2 vector10 = Vector2.Lerp(vector9, vector8, 0.5f) + Custom.PerpendicularVector(v) * (10f * num9 * bunny.flip * bunny.scale);
				num2 = 0.5f;
				vector = vector9 + Custom.DirVec(vector10, vector9);
				for (int m = 0; m < 4; m++)
				{
					float num10 = Mathf.InverseLerp(0f, 3f, m);
					Vector2 vector11 = Vector2.Lerp(Vector2.Lerp(vector9, vector10, num10), Vector2.Lerp(vector10, vector8, num10), num10);
					Vector2 vector12 = Custom.PerpendicularVector(vector11, vector);
					float num11 = (0.5f - 2.5f * num10 + (1f + Mathf.Pow(num10 - 1f, 3f)) * 3.5f * (1f - num6)) * bunny.scale;
					Vector2 vector13 = vector12 * ((num11 + num2) * 0.5f);
					Vector2 vector14 = vector12 * num11;
					(sLeaser.sprites[num5] as TriangleMesh).MoveVertice(m * 4, (vector + vector11) / 2f - vector13 - camPos);
					(sLeaser.sprites[num5] as TriangleMesh).MoveVertice(m * 4 + 1, (vector + vector11) / 2f + vector13 - camPos);
					(sLeaser.sprites[num5] as TriangleMesh).MoveVertice(m * 4 + 2, vector11 - vector14 - camPos);
					(sLeaser.sprites[num5] as TriangleMesh).MoveVertice(m * 4 + 3, vector11 + vector14 - camPos);
					vector = vector11;
					num2 = num11;
				}
			}
		}
		sLeaser.sprites[LegMesh(0, 0)].isVisible = bunny.BitesLeft > 1;
		sLeaser.sprites[LegMesh(0, 1)].isVisible = bunny.BitesLeft > 1;
		for (int n = 0; n < cosmetics.Count; n++)
		{
			cosmetics[n].DrawSprites(sLeaser, rCam, timeStacker, camPos, spine);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		bodyColor = Color.Lerp(bunny.iVars.bodyColor, palette.fogColor, 0.15f);
		if (bunny.IsSlayer)
		{
			bodyColor = bunny.iVars.bodyColor;
		}
		bodyColor = Color.Lerp(bodyColor, palette.fogColor, 0.55f * bunny.dryness);
		secondaryColor = bunny.iVars.secondaryColor;
		secondaryColor = Color.Lerp(secondaryColor, palette.fogColor, 0.55f * bunny.dryness);
		blackColor = palette.blackColor;
		blackColor = Color.Lerp(blackColor, palette.fogColor, 0.55f * bunny.dryness);
		for (int i = 0; i < bunny.bodyChunks.Length; i++)
		{
			sLeaser.sprites[AfterLegs + i].color = bodyColor;
		}
		sLeaser.sprites[BodyMesh].color = bodyColor;
		sLeaser.sprites[BodyMesh + 1].color = palette.blackColor;
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				int num = LegMesh(j, k);
				for (int l = 0; l < (sLeaser.sprites[num] as TriangleMesh).verticeColors.Length; l++)
				{
					(sLeaser.sprites[num] as TriangleMesh).verticeColors[l] = Color.Lerp(bodyColor, secondaryColor, 1f - (float)l / ((float)(sLeaser.sprites[num] as TriangleMesh).verticeColors.Length - 1f));
				}
			}
		}
		for (int m = 0; m < cosmetics.Count; m++)
		{
			cosmetics[m].ApplyPalette(sLeaser, rCam, palette);
		}
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		for (int i = 0; i < cosmetics.Count; i++)
		{
			cosmetics[i].AddToContainer(sLeaser, rCam, newContatiner, Template.SpritesOverlap.Behind);
		}
		for (int j = 0; j < StartOfExtraSprites; j++)
		{
			newContatiner.AddChild(sLeaser.sprites[j]);
		}
		for (int k = 0; k < cosmetics.Count; k++)
		{
			cosmetics[k].AddToContainer(sLeaser, rCam, newContatiner, Template.SpritesOverlap.InFront);
		}
		if (sLeaser.containers != null)
		{
			FContainer[] containers = sLeaser.containers;
			foreach (FContainer node in containers)
			{
				newContatiner.AddChild(node);
			}
		}
	}

	public void FindGrip(int p, int s, float multiplier = 1f)
	{
		Vector2 vector = LimbConnectionPoint(p, s);
		int index = legs[p, s].connection.index;
		Vector2 vector2 = ((index == 0) ? Custom.DirVec(bunny.bodyTrail[1], bunny.bodyTrail[0]) : Custom.DirVec(bunny.bodyTrail[index], bunny.bodyTrail[index - 1]));
		legs[p, s].FindGrip(base.owner.room, vector, vector, 30f * bunny.scale, vector + vector2 * (multiplier * 20f * bunny.scale), 2, 2, behindWalls: false);
		legs[p, s].reachedSnapPosition = false;
	}

	public bool MuddableSprite(RoomCamera.SpriteLeaser sLeaser, int sprite)
	{
		return sprite != BodyMesh + 1;
	}

	public void SetUpSpecialMudSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera.SpriteLeaser mudSleaser, MudOverlay mudOverlay)
	{
		float num = bunny.bodyChunks[0].rad + bunny.bodyChunks[bunny.bodyChunks.Length - 1].rad;
		PhysicalObject.BodyChunkConnection[] bodyChunkConnections = bunny.bodyChunkConnections;
		foreach (PhysicalObject.BodyChunkConnection bodyChunkConnection in bodyChunkConnections)
		{
			num += bodyChunkConnection.distance;
		}
		MudUtils.SetUpTube(mudOverlay.GetMudSprite(BodyMesh), num, bunny.bodyChunks[0].rad);
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				MudUtils.SetUpTube(mudOverlay.GetMudSprite(LegMesh(j, k)), legs[j, k].rad, 3f);
			}
		}
		foreach (Template cosmetic in cosmetics)
		{
			for (int l = cosmetic.startSprite; l < cosmetic.numberOfSprites; l++)
			{
				FSprite fSprite = sLeaser.sprites[l];
				if (fSprite is TriangleMesh)
				{
					MudUtils.SetUpTube(mudOverlay.GetMudSprite(fSprite), 15f, 3f);
				}
			}
		}
	}
}
