using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class LoachLeg : Tentacle
{
	public DebugSprite[] grabGoalSprites;

	public int tentacleNumber;

	public int side;

	public int pair;

	public Vector2 tentacleDir;

	public float maxLength;

	public Vector2 desiredGrabPos;

	public bool attachedAtTip;

	public int framesWithoutReaching;

	public int grabDelay;

	public int setGrabDelay = 5;

	public float fatigue;

	public int sureOfFootingCounter;

	public float smoothedFootingSecurity;

	public bool playStepSound;

	public float stepSoundVol;

	public BodyChunk headChunk;

	public Loach Loach => owner as Loach;

	public LoachLeg OtherTentacleInPair => Loach.legs[(tentacleNumber < Loach.legs.Length / 2) ? (tentacleNumber + Loach.legs.Length / 2) : (tentacleNumber - Loach.legs.Length / 2)];

	public LoachLeg(Loach loach, BodyChunk chunk, BodyChunk headChunk, float length, int tentacleNumber)
		: base(loach, chunk, length)
	{
		this.tentacleNumber = tentacleNumber;
		tProps = new TentacleProps(stiff: false, rope: false, shorten: true, 0.5f, 0f, 0.75f, 0.125f, 1.04f, 1.2f, 10f, 1f, 5f, 15, 60, 12, 0);
		tChunks = new TentacleChunk[6];
		for (int i = 0; i < tChunks.Length; i++)
		{
			tChunks[i] = new TentacleChunk(this, i, (float)(i + 1) / (float)tChunks.Length, 4f);
		}
		side = ((tentacleNumber % 2 != 0) ? 1 : 0);
		pair = ((tentacleNumber >= 2) ? 1 : 0);
		this.headChunk = headChunk;
		tentacleDir = Custom.DegToVec((float)tentacleNumber * 60f + 30f);
		stretchAndSqueeze = 0.1f;
		debugViz = false;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		if (debugViz)
		{
			if (grabGoalSprites != null)
			{
				grabGoalSprites[0].RemoveFromRoom();
				grabGoalSprites[1].RemoveFromRoom();
			}
			grabGoalSprites = new DebugSprite[2];
			grabGoalSprites[0] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
			grabGoalSprites[0].sprite.scale = 10f;
			grabGoalSprites[0].sprite.color = new Color(1f, 0f, 0f);
			room.AddObject(grabGoalSprites[0]);
			grabGoalSprites[1] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
			grabGoalSprites[1].sprite.scale = 10f;
			grabGoalSprites[1].sprite.color = new Color(0f, 5f, 0f);
			room.AddObject(grabGoalSprites[1]);
		}
		IntVector2 tilePosition = room.GetTilePosition(base.FloatBase + new Vector2(0f, -400f) + tentacleDir * 100f);
		IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, base.BasePos, tilePosition);
		if (intVector.HasValue)
		{
			Vector2 newGrabDest = Custom.RestrictInRect(base.FloatBase, room.TileRect(intVector.Value).Grow(2f));
			SharedPhysics.RayTracedTilesArray(base.FloatBase, room.MiddleOfTile(intVector.Value), segments);
			if (segments.Count > 2)
			{
				segments.RemoveAt(segments.Count - 1);
			}
			List<IntVector2> path = null;
			MoveGrabDest(newGrabDest, ref path);
			for (int i = 0; i < tChunks.Length; i++)
			{
				tChunks[i].Reset();
			}
			attachedAtTip = true;
			smoothedFootingSecurity = 1f;
		}
	}

	public override void Update()
	{
		base.Update();
		fatigue = Mathf.Lerp(fatigue, 0f, 0.01f);
		smoothedFootingSecurity = Mathf.Lerp(smoothedFootingSecurity, attachedAtTip ? 0f : Mathf.InverseLerp(3f, 30f, sureOfFootingCounter), 0.2f);
		attachedAtTip = false;
		maxLength = 20f * (float)Loach.maxPreferredHeight * 1.3333334f;
		idealLength = Mathf.Min(Mathf.Lerp(idealLength, base.grabDest.HasValue ? (Vector2.Distance(base.FloatBase, floatGrabDest.Value) * 1.5f * Mathf.InverseLerp(setGrabDelay, 0f, grabDelay)) : maxLength, 0.03f), maxLength);
		base.retractFac = ((Loach.resting == 1f) ? 1f : 0f);
		limp = !Loach.Consious;
		if (limp)
		{
			floatGrabDest = null;
			for (int i = 0; i < tChunks.Length; i++)
			{
				tChunks[i].vel *= 0.9f;
				tChunks[i].vel.y -= 0.5f;
			}
		}
		for (int j = 0; j < tChunks.Length; j++)
		{
			tChunks[j].rad = 5f;
			float num = (float)j / (float)(tChunks.Length - 1);
			tChunks[j].vel *= 0.5f;
			if (backtrackFrom == -1 || j < backtrackFrom)
			{
				if (j < tChunks.Length - 1)
				{
					tChunks[j].vel += Loach.HeadDir * (Mathf.Lerp(-1f, 1f, num) * Mathf.Pow(1f - num, 1.8f));
				}
				tChunks[j].vel += Loach.moveDirection * (Mathf.Sin(smoothedFootingSecurity * (float)Math.PI) * 1.5f * Mathf.Lerp(1f, 2.5f, Mathf.Sin(num * (float)Math.PI)));
			}
			tChunks[j].pos = Vector2.Lerp(tChunks[j].pos, room.MiddleOfTile(segments[tChunks[j].currentSegment]), 0.03f);
			if (room.PointSubmerged(tChunks[j].pos))
			{
				tChunks[j].vel *= 0.5f;
			}
		}
		if (backtrackFrom == -1)
		{
			tChunks[1].vel += Loach.moveDirection;
		}
		if (backtrackFrom == -1 && base.grabDest.HasValue)
		{
			base.Tip.vel += Custom.DirVec(base.Tip.pos, floatGrabDest.Value) * Mathf.Lerp(0.2f, 38f, smoothedFootingSecurity);
		}
		if (floatGrabDest.HasValue && Custom.DistLess(base.Tip.pos, floatGrabDest.Value, 40f) && backtrackFrom == -1 && Loach.resting < 1f)
		{
			base.Tip.pos = floatGrabDest.Value;
			base.Tip.vel *= 0f;
			attachedAtTip = true;
		}
		base.Tip.collideWithTerrain = !attachedAtTip;
		UpdateDesiredGrabPos();
		for (int k = 0; k < tChunks.Length; k++)
		{
			tChunks[k].vel.y -= 0.1f;
			tChunks[k].vel += connectedChunk.vel * 0.1f;
			if (!attachedAtTip)
			{
				if (floatGrabDest.HasValue)
				{
					tChunks[k].vel += Custom.DirVec(tChunks[k].pos, floatGrabDest.Value) * 0.3f;
				}
				else
				{
					tChunks[k].vel += Custom.DirVec(tChunks[k].pos, desiredGrabPos + Custom.DirVec(base.FloatBase, desiredGrabPos) * 70f) * 0.6f;
				}
			}
		}
		if (attachedAtTip)
		{
			framesWithoutReaching = 0;
			if (SharedPhysics.RayTraceTilesForTerrain(room, base.BasePos, base.grabDest.Value))
			{
				if (!Custom.DistLess(base.Tip.pos, headChunk.pos, maxLength))
				{
					ReleaseGrip();
				}
				if (!Custom.DistLess(base.Tip.pos, headChunk.pos, maxLength * 0.9f))
				{
					Loach.heldBackByLeg = true;
				}
			}
			else
			{
				ReleaseGrip();
			}
			if (playStepSound)
			{
				if (stepSoundVol > 0.5f)
				{
					room.PlaySound(WatcherEnums.WatcherSoundID.Loach_Step, base.Tip.pos, Mathf.InverseLerp(0.5f, 1f, stepSoundVol) * Mathf.InverseLerp(7f, 45f, Vector2.Distance(base.Tip.pos, base.Tip.lastPos)), 1f, Loach.abstractCreature);
				}
				playStepSound = false;
			}
			stepSoundVol = 0f;
		}
		else
		{
			stepSoundVol = Mathf.Min(1f, stepSoundVol + 0.025f);
			playStepSound = true;
			FindGrabPos();
			framesWithoutReaching++;
			if ((float)framesWithoutReaching > 60f && !floatGrabDest.HasValue)
			{
				framesWithoutReaching = 0;
			}
		}
		if (debugViz)
		{
			grabGoalSprites[1].pos = desiredGrabPos;
		}
	}

	public void ReleaseGrip()
	{
		if (OtherTentacleInPair.grabDelay < 1 && grabDelay < 1)
		{
			grabDelay = setGrabDelay;
		}
		floatGrabDest = null;
		sureOfFootingCounter = 0;
		fatigue = Mathf.Min(fatigue + 0.1f, 1f);
	}

	public void UpdateDesiredGrabPos()
	{
		if (Loach.stayStill)
		{
			desiredGrabPos = connectedChunk.pos + (new Vector2(0f, -1f) + tentacleDir * 0.5f).normalized * Mathf.Min(Loach.nextFloorHeight, maxLength * 0.9f);
		}
		else
		{
			desiredGrabPos = connectedChunk.pos + (new Vector2(0f, -1f) + Loach.moveDirection * ((pair == 0) ? 0.8f : 0.6f) + tentacleDir * 0.5f).normalized * Mathf.Min(Loach.nextFloorHeight, maxLength * 0.9f);
		}
	}

	public void FindGrabPos()
	{
		if (grabDelay > 0)
		{
			grabDelay--;
			return;
		}
		IntVector2? intVector = null;
		int num = UnityEngine.Random.Range(2, 5) * ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
		for (int i = 0; i < 9; i++)
		{
			if (intVector.HasValue)
			{
				break;
			}
			intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, base.BasePos, room.GetTilePosition(desiredGrabPos) + Custom.eightDirectionsAndZero[i] * num);
			if (intVector.HasValue && room.GetTile(intVector.Value + new IntVector2(0, 1)).Solid)
			{
				intVector = null;
			}
		}
		if (intVector.HasValue)
		{
			if (!base.grabDest.HasValue || GripPointAttractiveness(intVector.Value) > GripPointAttractiveness(base.grabDest.Value) * (Loach.stayStill ? 1f : 3f))
			{
				List<IntVector2> path = null;
				MoveGrabDest(Custom.RestrictInRect(base.FloatBase + tentacleDir * (Vector2.Distance(base.FloatBase, room.MiddleOfTile(intVector.Value)) * 0.5f), FloatRect.MakeFromVector2(room.MiddleOfTile(intVector.Value) - new Vector2(18f, 11f), room.MiddleOfTile(intVector.Value) + new Vector2(18f, 11f))), ref path);
				sureOfFootingCounter = 0;
			}
			else
			{
				sureOfFootingCounter++;
			}
		}
	}

	public float Support()
	{
		if (!attachedAtTip)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < Loach.legs.Length; i++)
		{
			if (i != tentacleNumber && Loach.legs[i].attachedAtTip && Loach.legs[i].Tip.pos.x < Loach.bodyChunks[2].pos.x != base.Tip.pos.x < Loach.bodyChunks[2].pos.x)
			{
				num = Mathf.Max(num, Mathf.Pow(Mathf.Sin(Mathf.InverseLerp(400f, 0f, Mathf.Abs(Loach.bodyChunks[2].pos.x - Loach.legs[i].Tip.pos.x)) * (float)Math.PI), 0.3f));
			}
		}
		return Mathf.InverseLerp(Mathf.Lerp(0.5f, -1f, num), 1f, Vector2.Dot(new Vector2(0f, -1f), Custom.DirVec(connectedChunk.pos, base.Tip.pos)));
	}

	public float ReleaseScore()
	{
		float num = (base.grabDest.HasValue ? (Vector2.Distance(floatGrabDest.Value, desiredGrabPos) * 1f) : Vector2.Distance(base.Tip.pos, desiredGrabPos));
		if (attachedAtTip)
		{
			num *= 2f;
		}
		if (!OtherTentacleInPair.attachedAtTip)
		{
			num /= 100f;
		}
		num *= 1f + (base.FloatBase.x - base.Tip.pos.x) * Mathf.Sign(Loach.moveDirection.x) * 1.4f;
		num /= 1f + Support() * 10f;
		return num * (1f - fatigue);
	}

	public float GripPointAttractiveness(IntVector2 pos)
	{
		if (!room.GetTile(pos + new IntVector2(0, 1)).Solid)
		{
			return 100f / room.GetTilePosition(desiredGrabPos).FloatDist(pos);
		}
		return 65f / room.GetTilePosition(desiredGrabPos).FloatDist(pos);
	}

	public void PushChunksApartInternal(int a, int b)
	{
		Vector2 vector = Custom.DirVec(tChunks[a].pos, tChunks[b].pos);
		float num = Vector2.Distance(tChunks[a].pos, tChunks[b].pos);
		float num2 = 10f;
		if (num < num2)
		{
			tChunks[a].pos -= vector * (num2 - num) * 0.5f;
			tChunks[a].vel -= vector * (num2 - num) * 0.5f;
			tChunks[b].pos += vector * (num2 - num) * 0.5f;
			tChunks[b].vel += vector * (num2 - num) * 0.5f;
		}
	}

	public override IntVector2 GravityDirection()
	{
		if (!(UnityEngine.Random.value < 0.5f))
		{
			return new IntVector2(0, -1);
		}
		return new IntVector2((int)Mathf.Sign(tentacleDir.x), -1);
	}
}
