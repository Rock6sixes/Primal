using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class LoachTentacle : Tentacle
{
	public class Task : ExtEnum<Task>
	{
		public static Task SearchGround = new Task("SearchGround", register: true);

		public static Task Recede = new Task("Recede", register: true);

		public static Task Hunt = new Task("Hunt", register: true);

		public static Task HuntBackup = new Task("HuntBackup", register: true);

		public static Task LurchSearch = new Task("LurchSearch", register: true);

		public static Task Digest = new Task("Digest", register: true);

		public Task(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public int canBindTime;

	public Vector2 searchVelocity;

	public int tentacleNumber;

	public Task task;

	public int graspAge;

	public BodyChunk grabChunk;

	public Tracker.CreatureRepresentation huntedCreature;

	public float sticky;

	public new List<IntVector2> scratchPath;

	public float stun;

	public int lurchSearchUpdate = 3;

	public bool huntBackupCheck1;

	public bool huntBackupCheck2;

	public bool Up;

	public float bezierT;

	public Vector2 preyPos = Vector2.zero;

	public Loach Loach => owner as Loach;

	public LoachTentacle(Loach loach, BodyChunk chunk, float length, int tentacleNumber)
		: base(loach, chunk, length)
	{
		tProps = new TentacleProps(stiff: false, rope: true, shorten: false, 0.5f, 0f, 0.1f, 0f, 0.2f, 1.2f, 10f, 0.25f, 3f, 15, 20, 20, 0);
		tChunks = new TentacleChunk[6];
		for (int i = 0; i < tChunks.Length; i++)
		{
			tChunks[i] = new TentacleChunk(this, i, (float)(i + 1) / (float)tChunks.Length, Mathf.Lerp(loach.rootRad, loach.tipRad, (float)i / (float)tChunks.Length + 1f));
		}
		stretchAndSqueeze = 0.1f;
		this.tentacleNumber = tentacleNumber;
		SwitchTask(Task.SearchGround);
	}

	public void LookForHuntedCreature()
	{
		if (Loach.AI.preyTracker.TotalTrackedPrey == 0 || task == Task.Recede)
		{
			return;
		}
		Tracker.CreatureRepresentation trackedPrey = Loach.AI.preyTracker.GetTrackedPrey(UnityEngine.Random.Range(0, Loach.AI.preyTracker.TotalTrackedPrey));
		if (trackedPrey.BestGuessForPosition().room == Loach.abstractCreature.pos.room && !(Vector2.Distance(room.MiddleOfTile(trackedPrey.BestGuessForPosition()), base.FloatBase) > idealLength) && !IsCreatureCaughtEnough(trackedPrey.representedCreature))
		{
			huntedCreature = trackedPrey;
			if (!Loach.safariControlled)
			{
				SwitchTask(Task.Hunt);
			}
		}
	}

	public void SwitchTask(Task newTask)
	{
		if (newTask == Task.LurchSearch)
		{
			tProps.goalAttractionSpeedTip = 250f;
		}
		else
		{
			tProps.goalAttractionSpeedTip = 0.1f;
		}
		if (newTask != Task.Hunt)
		{
			huntedCreature = null;
		}
		if (newTask != Task.Recede)
		{
			graspAge = 0;
			if (grabChunk != null)
			{
				grabChunk = null;
			}
		}
		else
		{
			graspAge = UnityEngine.Random.Range(0, 35);
		}
		_ = newTask == Task.SearchGround;
		if (newTask == Task.HuntBackup)
		{
			huntBackupCheck1 = true;
			huntBackupCheck2 = false;
		}
		task = newTask;
	}

	public void Touch()
	{
		bool flag = false;
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (room.abstractRoom.creatures[i].realizedCreature == null || room.abstractRoom.creatures[i].realizedCreature.inShortcut || room.abstractRoom.creatures[i].realizedCreature == Loach || room.abstractRoom.creatures[i].tentacleImmune)
			{
				continue;
			}
			Creature realizedCreature = room.abstractRoom.creatures[i].realizedCreature;
			for (int j = 0; j < tChunks.Length; j++)
			{
				for (int k = 0; k < realizedCreature.bodyChunks.Length; k++)
				{
					if (!Custom.DistLess(tChunks[j].pos, realizedCreature.bodyChunks[k].pos, tChunks[j].rad + realizedCreature.bodyChunks[k].rad))
					{
						continue;
					}
					if (Loach.Rotted && realizedCreature is Player && room.game.IsStorySession && room.game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility)
					{
						(realizedCreature as Player).SetHaloDisplayTime(20);
						continue;
					}
					Loach.AI.tracker.SeeCreature(realizedCreature.abstractCreature);
					if (realizedCreature.abstractCreature.creatureTemplate.AI && realizedCreature.abstractCreature.abstractAI.RealAI != null && realizedCreature.abstractCreature.abstractAI.RealAI.tracker != null)
					{
						realizedCreature.abstractCreature.abstractAI.RealAI.tracker.SeeCreature(Loach.abstractCreature);
					}
					CollideWithCreature(j, realizedCreature.bodyChunks[k]);
					if (realizedCreature.newToRoomInvinsibility >= 1 || grabChunk != null || j != tChunks.Length - 1 || (Loach.safariControlled && Loach.controlRegion == null && !(task == Task.LurchSearch)) || (!(task == Task.Hunt) && IsCreatureCaughtEnough(realizedCreature.abstractCreature)))
					{
						break;
					}
					flag = true;
					if (!(Vector2.Distance(tChunks[j].vel, realizedCreature.bodyChunks[k].vel) < Mathf.Lerp(1f, 8f, sticky)))
					{
						break;
					}
					bool flag2 = Loach.AI.tracker.RepresentationForObject(realizedCreature, AddIfMissing: false) != null && Loach.CanEatCreature(realizedCreature.abstractCreature);
					for (int l = 0; l < tChunks.Length && flag2; l++)
					{
						if (tChunks[l].phase > -1f || room.GetTile(tChunks[l].pos).Solid)
						{
							flag2 = false;
						}
					}
					if (!flag2)
					{
						break;
					}
					if (Loach.State.SetNewTarget(0.25f))
					{
						Loach.AI.preyTracker.ForgetAllPrey();
						Loach.AI.preyTracker.AddPrey(Loach.AI.tracker.RepresentationForObject(realizedCreature, AddIfMissing: false));
					}
					grabChunk = realizedCreature.bodyChunks[k];
					room.PlaySound(WatcherEnums.WatcherSoundID.Loach_Tentacle_Grab, tChunks[j].pos, 1f, 1f, owner.abstractPhysicalObject);
					SwitchTask(Task.Recede);
					return;
				}
			}
		}
		if (flag)
		{
			sticky = Mathf.Min(1f, sticky + 1f / 30f);
		}
		else
		{
			sticky = Mathf.Max(0f, sticky - 1f / 60f);
		}
	}

	public void CollideWithCreature(int tChunk, BodyChunk creatureChunk)
	{
		if (backtrackFrom <= -1 || backtrackFrom > tChunk)
		{
			float num = Vector2.Distance(tChunks[tChunk].pos, creatureChunk.pos);
			float num2 = (tChunks[tChunk].rad + creatureChunk.rad) / 4f;
			Vector2 vector = Custom.DirVec(tChunks[tChunk].pos, creatureChunk.pos);
			float num3 = creatureChunk.mass / (creatureChunk.mass + 0.01f);
			float num4 = 0.8f;
			Vector2 vector2 = vector * ((num - num2) * num3 * num4);
			tChunks[tChunk].pos += vector2;
			tChunks[tChunk].vel += vector2;
			Vector2 vector3 = vector * ((num - num2) * (1f - num3) * num4);
			creatureChunk.pos -= vector3;
			creatureChunk.vel -= vector3;
		}
	}

	public override void Update()
	{
		base.Update();
		if (task != Task.HuntBackup)
		{
			LookForHuntedCreature();
		}
		if (grabChunk != null)
		{
			float num = Vector2.Distance(base.Tip.pos, grabChunk.pos);
			float num2 = (base.Tip.rad + grabChunk.rad) / 4f;
			Vector2 vector = Custom.DirVec(base.Tip.pos, grabChunk.pos);
			float num3 = grabChunk.mass / (grabChunk.mass + 0.01f);
			float num4 = 1f;
			Vector2 vector2 = vector * ((num - num2) * num3 * num4);
			base.Tip.pos += vector2;
			base.Tip.vel += vector2;
			Vector2 vector3 = vector * ((num - num2) * (1f - num3) * num4);
			grabChunk.pos -= vector3;
			grabChunk.vel -= vector3;
			if (grabChunk.owner is Player && UnityEngine.Random.value < Mathf.Lerp(0f, 0.1f, (grabChunk.owner as Player).GraspWiggle))
			{
				stun = Math.Max(stun, UnityEngine.Random.Range(1, 17));
				grabChunk = null;
			}
			else if (grabChunk.owner is Creature creature && !Loach.CanEatCreature(creature.abstractCreature))
			{
				grabChunk = null;
			}
		}
		if (Loach.digestingCounter > 0 && task != Task.Digest)
		{
			SwitchTask(Task.Digest);
		}
		Vector2 pos = Loach.firstChunk.pos;
		if (Loach.controlRegion != null && task != Task.Recede && task != Task.LurchSearch)
		{
			SafariHunt(ref scratchPath);
		}
		else if (task == Task.Hunt)
		{
			Hunt(ref scratchPath);
		}
		else if (task == Task.Recede)
		{
			if (GetTentacleGrabTotal() < 4)
			{
				graspAge++;
			}
			if (grabChunk == null)
			{
				SwitchTask(Task.SearchGround);
			}
			else
			{
				MoveGrabDest(pos + Custom.DirVec(pos, grabChunk.pos) * 20f, ref scratchPath);
			}
			Vector2 p = pos;
			bool flag = room.VisualContact(grabChunk.pos, pos);
			for (int num5 = tChunks.Length - 1; num5 >= 0; num5--)
			{
				Vector2 p2 = base.FloatBase;
				if (num5 > 0)
				{
					p2 = tChunks[num5 - 1].pos;
					if (!flag && !room.VisualContact(grabChunk.pos, tChunks[num5 - 1].pos))
					{
						p = tChunks[num5].pos;
						flag = true;
					}
				}
				tChunks[num5].vel += Custom.DirVec(tChunks[num5].pos, p2) * 1.2f;
				if (tChunks[num5].phase > -1f || room.GetTile(tChunks[num5].pos).Solid)
				{
					room.PlaySound(WatcherEnums.WatcherSoundID.Loach_Tentacle_Release, grabChunk);
					grabChunk = null;
					SwitchTask(Task.SearchGround);
					break;
				}
			}
			if (graspAge > 120 && UnityEngine.Random.value + (float)graspAge / 600f > 1.2f)
			{
				SwitchTask(Task.HuntBackup);
			}
			if (task == Task.Recede)
			{
				grabChunk.vel += (Vector2)Vector3.Slerp(Custom.DirVec(grabChunk.pos, p), Custom.DirVec(base.Tip.pos, tChunks[tChunks.Length - 2].pos), 0.5f) * (Custom.LerpMap(grabPath.Count, 3f, 18f, 0.65f, 0.25f) * 0.3f) / (grabChunk.mass * 1.4f / (float)GetTentacleGrabTotal());
			}
		}
		if (task == Task.Digest)
		{
			floatGrabDest = Loach.firstChunk.pos;
			if (Loach.digestingCounter <= 0)
			{
				SwitchTask(Task.SearchGround);
			}
		}
		if (task == Task.HuntBackup)
		{
			if (huntBackupCheck1)
			{
				floatGrabDest = Loach.firstChunk.pos;
				if (Custom.DistLess(base.Tip.pos, Loach.firstChunk.pos, base.Tip.rad + Loach.firstChunk.rad))
				{
					LoachTentacle[] tentacle = Loach.tentacle;
					foreach (LoachTentacle loachTentacle in tentacle)
					{
						if (loachTentacle.huntedCreature != null)
						{
							Vector2 v = Custom.DirVec(Loach.firstChunk.pos, room.MiddleOfTile(loachTentacle.huntedCreature.BestGuessForPosition()));
							v = Custom.PerpendicularVector(v) + new Vector2(UnityEngine.Random.Range(-3, 3), UnityEngine.Random.Range(-3, 3));
							preyPos = room.MiddleOfTile(loachTentacle.huntedCreature.BestGuessForPosition());
							base.Tip.vel += v * 2f;
							huntBackupCheck1 = false;
							huntBackupCheck2 = true;
							bezierT = 0f;
							Up = UnityEngine.Random.Range(0, 1) == 0;
							break;
						}
					}
					if (!huntBackupCheck2)
					{
						SwitchTask(Task.SearchGround);
					}
				}
			}
			if (huntBackupCheck2)
			{
				Vector2 normalized = Custom.rotateVectorDeg(Custom.DirVec(Loach.firstChunk.pos, preyPos), UnityEngine.Random.Range(45f, 90f)).normalized;
				floatGrabDest = Custom.Bezier(Loach.firstChunk.pos, Loach.firstChunk.pos + (Up ? normalized : (-normalized)) * 60f, preyPos, preyPos + (Up ? normalized : (-normalized)) * 60f, bezierT);
				bezierT += 1f / 120f;
				if (bezierT > 0.6f)
				{
					huntBackupCheck1 = true;
					huntBackupCheck2 = false;
					SwitchTask(Task.SearchGround);
					LookForHuntedCreature();
				}
			}
		}
		if (task == Task.SearchGround)
		{
			if (Loach.isLurching)
			{
				SwitchTask(Task.LurchSearch);
			}
			if (Loach.slimeMoldGrabPos.HasValue)
			{
				floatGrabDest = Loach.slimeMoldGrabPos;
				for (int j = 0; j < tChunks.Length; j++)
				{
					if (Custom.DistLess(tChunks[j].pos, Loach.slimeMoldToGrab.firstChunk.pos, tChunks[j].rad + Loach.slimeMoldToGrab.firstChunk.rad + 5f))
					{
						grabChunk = Loach.slimeMoldToGrab.firstChunk;
						SwitchTask(Task.Recede);
					}
				}
			}
			else
			{
				floatGrabDest = Loach.firstChunk.pos + Loach.firstChunk.vel * UnityEngine.Random.Range(8f, 16f) - new Vector2(0f, UnityEngine.Random.Range(140, 200));
			}
		}
		if (task == Task.LurchSearch)
		{
			if (lurchSearchUpdate == 0)
			{
				lurchSearchUpdate = 3;
				if (!Loach.isLurching)
				{
					SwitchTask(Task.SearchGround);
				}
				Vector2 vector4 = Custom.DirVec(Loach.bodyChunks[1].pos, Loach.mainBodyChunk.pos) * UnityEngine.Random.Range(50f, 100f);
				vector4.y = UnityEngine.Random.Range(-60f, 120f);
				floatGrabDest = Loach.firstChunk.pos + vector4;
			}
			else
			{
				lurchSearchUpdate--;
			}
		}
		Touch();
	}

	public int GetTentacleGrabTotal()
	{
		int num = 0;
		LoachTentacle[] tentacle = Loach.tentacle;
		for (int i = 0; i < tentacle.Length; i++)
		{
			if (tentacle[i].grabChunk != null)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsCreatureCaughtEnough(AbstractCreature crit)
	{
		int num = 0;
		for (int i = 0; i < Loach.tentacle.Length; i++)
		{
			if (Loach.tentacle[i].grabChunk != null && Loach.tentacle[i].grabChunk.owner is Creature && (Loach.tentacle[i].grabChunk.owner as Creature).abstractCreature == crit && Loach.tentacle[i].graspAge < 20)
			{
				num++;
			}
		}
		if (num >= 7)
		{
			return true;
		}
		return false;
	}

	public void SafariHunt(ref List<IntVector2> path)
	{
		float num = (float)tentacleNumber / (float)(Loach.tentacle.Length - 1) * 2f - 1f;
		Vector2 vec = Vector3.Slerp(Vector3.down, connectedChunk.Rotation, 0.75f);
		vec = Custom.RotateAroundOrigo(vec, num * 20f);
		for (int i = 0; i < tChunks.Length; i++)
		{
			float num2 = 1f - (float)i / (float)(tChunks.Length - 1) * 2f;
			if (num2 < 0f)
			{
				break;
			}
			tChunks[i].vel += vec * num2;
		}
		Vector2 vector = base.Tip.pos - Loach.mainBodyChunk.pos;
		float num3 = Custom.Dist(vector, Loach.controlRegion.pos);
		if (!Loach.controlLimit.HasValue || Custom.Dist(Loach.controlLimit.Value, Loach.controlRegion.pos) > num3)
		{
			Loach.controlLimit = vector;
		}
		MoveGrabDest(Loach.mainBodyChunk.pos + Loach.controlRegion.pos, ref path);
	}

	public void Hunt(ref List<IntVector2> path)
	{
		if (huntedCreature.BestGuessForPosition().room != Loach.abstractCreature.pos.room || huntedCreature.deleteMeNextFrame)
		{
			SwitchTask(Task.SearchGround);
			return;
		}
		float num = (float)tentacleNumber / (float)(Loach.tentacle.Length - 1) * 2f - 1f;
		Vector2 vec = Vector3.Slerp(Vector3.down, connectedChunk.Rotation, 0.75f);
		vec = Custom.RotateAroundOrigo(vec, num * 20f);
		for (int i = 0; i < tChunks.Length; i++)
		{
			float num2 = 1f - (float)i / (float)(tChunks.Length - 1) * 2f;
			if (num2 < 0f)
			{
				break;
			}
			tChunks[i].vel += vec * num2;
		}
		if (huntedCreature.VisualContact)
		{
			MoveGrabDest(huntedCreature.representedCreature.realizedCreature.mainBodyChunk.pos, ref path);
			return;
		}
		if (huntedCreature.BestGuessForPosition().TileDefined)
		{
			MoveGrabDest(room.MiddleOfTile(huntedCreature.BestGuessForPosition()), ref path);
		}
		for (int j = 0; j < tChunks.Length; j++)
		{
			if (!(huntedCreature is Tracker.ElaborateCreatureRepresentation))
			{
				continue;
			}
			for (int k = 0; k < (huntedCreature as Tracker.ElaborateCreatureRepresentation).ghosts.Count; k++)
			{
				if (room.GetTilePosition(tChunks[j].pos) == (huntedCreature as Tracker.ElaborateCreatureRepresentation).ghosts[k].coord.Tile)
				{
					(huntedCreature as Tracker.ElaborateCreatureRepresentation).ghosts[k].Push();
				}
			}
		}
	}
}
