using System;
using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class SandGrub : Creature, IPlayerEdible, PhysicalObject.IHaveAppendages
{
	public class IndividualVariations
	{
		public float tailLengthFactor;

		public Color effectColor;

		public float bodySizeFactor;

		public float tailThicknessFactor;

		public IndividualVariations(AbstractCreature abstractCreature)
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(abstractCreature.ID.RandomSeed);
			effectColor = new HSLColor(Custom.WrappedRandomVariation(0.184f, 0.1f, 0.5f), Custom.ClampedRandomVariation(0.6f, 0.35f, 1f), Custom.ClampedRandomVariation(0.45f, 0.25f, 0.3f)).rgb;
			tailLengthFactor = 1f + Mathf.Pow(UnityEngine.Random.value, 2f) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * 0.25f;
			bodySizeFactor = 1f + Mathf.Pow(UnityEngine.Random.value, 2f) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * 0.25f;
			tailThicknessFactor = 1f + Mathf.Pow(UnityEngine.Random.value, 2f) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * 0.25f;
			UnityEngine.Random.state = state;
		}
	}

	public SandGrubAI ai;

	public SandGrubBurrow burrow;

	public float tailLength;

	public float tetherLength;

	public IndividualVariations iVars;

	public float stretchDir;

	public int timeBiting;

	public BigSandGrubNeck tentacle;

	public float mouthOpen;

	public float lastMouthOpen;

	public bool safariRetract;

	public int returnToBurrowCounter;

	public int buryCounter;

	public int tentacleHuntCounter;

	public float lastTetherLength;

	public ChunkDynamicSoundLoop soundLoop;

	public ChunkDynamicSoundLoop warningLoop;

	public float timeToNextWarningModeSwitch;

	public int warningModeSwitchTicker;

	public bool warningAudible;

	public float warningWarble;

	public new SandGrubState State => base.State as SandGrubState;

	public bool Big => base.Template.type == WatcherEnums.CreatureTemplateType.BigSandGrub;

	public bool BodyInBurrow
	{
		get
		{
			if (burrow != null)
			{
				return ai.behavior != SandGrubAI.Behavior.OutOfBurrow;
			}
			return false;
		}
	}

	public bool TetheredToBurrow => burrow != null;

	public bool Grabbable
	{
		get
		{
			if (!FullyInsideBurrow && BitesLeft > 0)
			{
				return !Big;
			}
			return false;
		}
	}

	public bool FullyInsideBurrow
	{
		get
		{
			if (burrow != null && BodyInBurrow && ai.targetBurrowDepth > 20f)
			{
				return Vector2.Dot(HeadChunk.pos - burrow.pos, burrow.dir) < 0f;
			}
			return false;
		}
	}

	public float Burying => Mathf.Clamp01((float)(buryCounter - 40) / 40f);

	public BodyChunk HeadChunk => base.bodyChunks[0];

	public BodyChunk TailChunk => base.bodyChunks[3];

	public override float VisibilityBonus
	{
		get
		{
			if (!BodyInBurrow)
			{
				return 0f;
			}
			return -1f;
		}
	}

	public override Vector2 VisionPoint
	{
		get
		{
			if (!BodyInBurrow)
			{
				return base.firstChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.firstChunk.pos) * base.firstChunk.rad;
			}
			return burrow.pos + burrow.dir * 15f;
		}
	}

	public int BitesLeft { get; set; }

	public int FoodPoints => 2;

	public bool Edible
	{
		get
		{
			if (base.dead && BitesLeft > 0)
			{
				return !Big;
			}
			return false;
		}
	}

	public bool AutomaticPickUp => false;

	public SandGrub(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		float num = 0.25f;
		tailLength = (Big ? 275f : 90f);
		iVars = new IndividualVariations(abstractCreature);
		if (Big)
		{
			iVars.bodySizeFactor = 1f + Mathf.Abs(iVars.bodySizeFactor - 1f);
		}
		tailLength *= iVars.tailLengthFactor;
		base.bodyChunks = new BodyChunk[4];
		base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), iVars.bodySizeFactor * 4f, num * 0.25f);
		base.bodyChunks[1] = new BodyChunk(this, 1, default(Vector2), iVars.bodySizeFactor * 5f, num * 0.25f);
		base.bodyChunks[2] = new BodyChunk(this, 2, default(Vector2), iVars.bodySizeFactor * (Big ? 5f : 4.5f), num * 0.25f);
		base.bodyChunks[3] = new BodyChunk(this, 3, default(Vector2), iVars.bodySizeFactor * (Big ? 5f : 3f), num * 0.25f);
		bodyChunkConnections = new BodyChunkConnection[4];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 6f, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 6f, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[3], 5f, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[3] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[3], 15f, BodyChunkConnection.Type.Push, 1f, -1f);
		base.airFriction = 0.995f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.1f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		base.GoThroughFloors = true;
		base.CollideWithTerrain = false;
		BitesLeft = 3;
		if (Big)
		{
			tentacle = new BigSandGrubNeck(this, 8, tailLength, iVars.bodySizeFactor * 4.5f);
			appendages = new List<Appendage>();
			appendages.Add(new Appendage(this, 0, tentacle.tChunks.Length + 1));
			soundLoop = new ChunkDynamicSoundLoop(HeadChunk);
			warningLoop = new ChunkDynamicSoundLoop(HeadChunk);
			warningLoop.Volume = 0f;
		}
		if (!State.alive)
		{
			return;
		}
		foreach (UpdatableAndDeletable update in abstractCreature.Room.realizedRoom.updateList)
		{
			if (update is SandGrubNetwork sandGrubNetwork && sandGrubNetwork.OwnsGrub(this))
			{
				if (sandGrubNetwork.FreeBurrowCount > 0)
				{
					SwitchBurrow(sandGrubNetwork.GetRandomFreeBurrow());
				}
				break;
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		base.CollideWithTerrain = !BodyInBurrow;
		base.CollideWithObjects = ai.behavior != SandGrubAI.Behavior.Hide;
		lastTetherLength = tetherLength;
		lastMouthOpen = mouthOpen;
		if (base.safariControlled)
		{
			Player.InputPackage? inputPackage = inputWithDiagonals;
			if (inputPackage.HasValue)
			{
				Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
				inputPackage = lastInputWithDiagonals;
				if (inputPackage.HasValue)
				{
					Player.InputPackage valueOrDefault2 = inputPackage.GetValueOrDefault();
					if (valueOrDefault.thrw && !valueOrDefault2.thrw && !warningAudible)
					{
						SwapWarningMode();
						if (Big && !BodyInBurrow)
						{
							room.PlaySound(WatcherEnums.WatcherSoundID.Big_Sand_Grub_Threaten, base.mainBodyChunk, loop: false, 2f, 1f);
						}
					}
				}
			}
		}
		float to;
		if (!base.Consious)
		{
			to = 0f;
		}
		else
		{
			if (Big)
			{
				if (ai.behavior == SandGrubAI.Behavior.OutOfBurrow)
				{
					Vector2? huntPos = ai.huntPos;
					if (huntPos.HasValue)
					{
						Vector2 valueOrDefault3 = huntPos.GetValueOrDefault();
						if (ai.HuntGracePeriod == 0f && base.grasps[0] == null)
						{
							if (!base.safariControlled)
							{
								to = ((!Custom.DistLess(valueOrDefault3, base.bodyChunks[0].pos, 100f)) ? 0f : ((ai.huntTarget == null) ? 0.6f : 1f));
							}
							else
							{
								ref Player.InputPackage? reference = ref inputWithDiagonals;
								to = ((!reference.HasValue || !reference.GetValueOrDefault().pckp) ? 0.4f : 1f);
							}
							goto IL_01c5;
						}
					}
				}
				to = 0f;
				goto IL_01c5;
			}
			if (ai.behavior == SandGrubAI.Behavior.Hunt && base.grasps[0] == null)
			{
				if (base.safariControlled)
				{
					ref Player.InputPackage? reference2 = ref inputWithDiagonals;
					to = ((reference2.HasValue && reference2.GetValueOrDefault().pckp) ? 1f : 0f);
				}
				else
				{
					to = 1f;
				}
			}
			else
			{
				to = 0f;
			}
		}
		goto IL_0259;
		IL_0259:
		mouthOpen = Custom.LerpAndTick(mouthOpen, to, 0.08f, 0.05f);
		if (Big)
		{
			base.gravity = ((base.Consious && !BodyInBurrow) ? 0f : 0.9f);
			UpdateTentacle();
		}
		else
		{
			base.gravity = ((grabbedBy.Count > 0) ? 0.25f : 0.9f);
		}
		if (grabbedBy.Count > 0)
		{
			ai.Scare(320);
			LoseAllGrasps();
		}
		if ((grabbedBy.Count > 0 || burrow == null) && ai.behavior != SandGrubAI.Behavior.OutOfBurrow && !Big)
		{
			if (grabbedBy.Count > 0)
			{
				room.PlaySound(Big ? WatcherEnums.WatcherSoundID.Big_Sand_Grub_Squeal : WatcherEnums.WatcherSoundID.Sand_Grub_Squeal, base.mainBodyChunk);
			}
			ai.LeaveBurrow();
		}
		if (base.Consious || (base.stun < 20 && !base.dead && grabbedBy.Count > 0))
		{
			Act(eu);
		}
		else
		{
			LoseAllGrasps();
			returnToBurrowCounter = 0;
		}
		if (BodyInBurrow)
		{
			tentacleHuntCounter = 0;
		}
		if (base.grasps[0] == null)
		{
			timeBiting = 0;
		}
		else
		{
			timeBiting++;
		}
		if (BodyInBurrow)
		{
			Vector2 dir = burrow.dir;
			Vector2 vector = burrow.pos - dir * ai.targetBurrowDepth;
			TailChunk.vel *= 0.6f;
			TailChunk.vel += (vector - TailChunk.pos) * 0.5f;
			TailChunk.pos += (vector - TailChunk.pos) * 0.5f;
			Vector2 vector2 = Custom.PerpendicularVector(dir);
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				BodyChunk bodyChunk = base.bodyChunks[i];
				bodyChunk.vel *= 0.75f;
				bodyChunk.vel += vector2 * (Vector2.Dot(burrow.pos - bodyChunk.pos, vector2) * 0.25f);
				bodyChunk.vel += dir * 1.25f;
			}
			bool flag = Vector2.Dot(HeadChunk.pos, dir) < Vector2.Dot(TailChunk.pos, dir) + 5f;
			for (int j = 0; j < 3; j++)
			{
				bodyChunkConnections[0].type = (flag ? BodyChunkConnection.Type.Pull : BodyChunkConnection.Type.Normal);
			}
			bodyChunkConnections[3].active = !flag;
			tetherLength = 0f;
		}
		else if (TetheredToBurrow && !Big)
		{
			tetherLength = Mathf.Max(tetherLength, Vector2.Distance(TailChunk.pos, burrow.pos) - 10f);
			if (tetherLength > tailLength + 20f)
			{
				PullFromBurrow();
			}
			else
			{
				tetherLength = Mathf.Min(tetherLength, tailLength);
				BodyChunk tailChunk = TailChunk;
				Vector2 vector3 = (burrow.pos - tailChunk.pos).normalized * Mathf.Max(Vector2.Distance(burrow.pos, tailChunk.pos) - tetherLength, 0f);
				tailChunk.vel += vector3 * 0.15f;
				foreach (Grasp item in grabbedBy)
				{
					item.grabber.mainBodyChunk.vel += vector3 / item.grabber.mainBodyChunk.mass * 0.04f;
				}
			}
		}
		if (BodyInBurrow && HeadChunk.pos.y < burrow.pos.y != HeadChunk.lastPos.y < burrow.pos.y)
		{
			if (room.terrain != null)
			{
				room.AddObject(new SandPuffSpawner.SandPuff(burrow.pos, UnityEngine.Random.value * 0.1f + 0.1f));
			}
			room.PlaySound(WatcherEnums.WatcherSoundID.Sand_Grub_Peek, HeadChunk);
		}
		ChangeCollisionLayer((!FullyInsideBurrow) ? 1 : 0);
		canBeHitByWeapons = !FullyInsideBurrow;
		if (soundLoop != null)
		{
			soundLoop.sound = WatcherEnums.WatcherSoundID.Big_Sand_Grub_Slide_LOOP;
			float num = Custom.LerpMap(Mathf.Abs(tetherLength - lastTetherLength) + HeadChunk.vel.magnitude * 0.5f, 1f, 5f, 0f, 1f);
			if (soundLoop.Volume < num)
			{
				soundLoop.Volume = Mathf.Lerp(soundLoop.Volume, num, 0.2f);
			}
			else
			{
				soundLoop.Volume = Mathf.MoveTowards(soundLoop.Volume, num, 0.05f);
			}
			soundLoop.Update();
		}
		if (warningLoop != null)
		{
			warningLoop.sound = WatcherEnums.WatcherSoundID.Big_Sand_Grub_Rattle_LOOP;
			warningModeSwitchTicker++;
			if ((float)warningModeSwitchTicker >= timeToNextWarningModeSwitch && (!base.safariControlled || warningAudible))
			{
				SwapWarningMode();
			}
			float num2 = ((warningAudible && BodyInBurrow && base.Consious) ? (0.25f + Mathf.Sin((float)warningModeSwitchTicker * 1f / (float)Math.PI * warningWarble) * 0.15f) : 0f);
			if (warningLoop.Volume < num2)
			{
				warningLoop.Volume = Mathf.Lerp(warningLoop.Volume, num2, 0.2f);
			}
			else
			{
				warningLoop.Volume = Mathf.MoveTowards(warningLoop.Volume, num2, 0.05f);
			}
			warningLoop.Update();
		}
		if (!FullyInsideBurrow)
		{
			return;
		}
		for (int num3 = base.abstractCreature.stuckObjects.Count - 1; num3 >= 0; num3--)
		{
			if (base.abstractCreature.stuckObjects[num3] is AbstractPhysicalObject.AbstractSpearStick abstractSpearStick && abstractSpearStick.A.realizedObject is Weapon weapon)
			{
				weapon.Destroy();
				base.abstractCreature.stuckObjects[num3].Deactivate();
			}
		}
		return;
		IL_01c5:
		if (!BodyInBurrow && UnityEngine.Random.value < 0.005f)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Big_Sand_Grub_Threaten, base.mainBodyChunk);
		}
		goto IL_0259;
	}

	public void SwapWarningMode()
	{
		warningAudible = !warningAudible;
		if (warningAudible)
		{
			timeToNextWarningModeSwitch = UnityEngine.Random.Range(10, 80);
		}
		else
		{
			timeToNextWarningModeSwitch = UnityEngine.Random.Range(40, 200);
		}
		warningWarble = UnityEngine.Random.Range(0.25f, 2f);
		warningModeSwitchTicker = 0;
	}

	public void UpdateTentacle()
	{
		tentacle.Update();
		tentacle.limp = !base.Consious;
		if (burrow == null)
		{
			tetherLength = 0f;
		}
		tentacle.retractFac = 1f - tetherLength / tailLength;
		if (BodyInBurrow)
		{
			tentacle.ClearGrabDest();
		}
		if (tetherLength > 0f)
		{
			float num = (tentacle.limp ? 0.5f : 0f);
			if (tentacleHuntCounter > 0 && tentacleHuntCounter < 5)
			{
				num = 0.5f;
			}
			Vector2 vector = Custom.DirVec(TailChunk.pos, tentacle.Tip.pos);
			float num2 = Vector2.Distance(TailChunk.pos, tentacle.Tip.pos);
			Vector2 vector2 = vector * (num2 * (1f - num));
			Vector2 vector3 = vector * (num2 * num);
			TailChunk.pos += vector2;
			TailChunk.vel += vector2;
			tentacle.Tip.pos -= vector3;
			tentacle.Tip.vel -= vector3;
			for (int i = 0; i < tentacle.tChunks.Length; i++)
			{
				float value = (float)i / (float)(tentacle.tChunks.Length - 1);
				if (base.Consious && burrow != null)
				{
					tentacle.tChunks[i].vel *= 0.9f;
					tentacle.tChunks[i].vel += burrow.dir * (Mathf.InverseLerp(0.5f, 0f, value) * 0.3f);
				}
				else
				{
					tentacle.tChunks[i].vel *= 0.95f;
				}
			}
			if (base.Consious)
			{
				for (int j = 1; j < tentacle.tChunks.Length - 1; j++)
				{
					BigSandGrubNeck.Segment obj = tentacle.tChunks[j - 1];
					BigSandGrubNeck.Segment segment = tentacle.tChunks[j - 1];
					Vector2 vector4 = (obj.pos - segment.pos).normalized * 0.05f;
					obj.vel += vector4;
					segment.vel -= vector4;
				}
			}
			return;
		}
		for (int k = 0; k < tentacle.tChunks.Length; k++)
		{
			if (burrow != null)
			{
				tentacle.tChunks[k].pos = burrow.pos - burrow.dir * 10f;
			}
			else
			{
				tentacle.tChunks[k].pos = base.bodyChunks[0].pos;
			}
			tentacle.tChunks[k].vel = Vector2.zero;
		}
	}

	public void PullFromBurrow()
	{
		burrow.GrubPulled();
		if (base.graphicsModule is SandGrubGraphics sandGrubGraphics)
		{
			sandGrubGraphics.PulledFromBurrow();
		}
		if (room.terrain != null)
		{
			room.AddObject(new SandPuffSpawner.SandPuff(burrow.pos, UnityEngine.Random.value * 0.2f + 0.2f));
		}
		room.PlaySound(WatcherEnums.WatcherSoundID.Sand_Grub_Pull_From_Burrow, burrow.pos);
		if (grabbedBy.Count > 0)
		{
			SetKillTag(grabbedBy[0].grabber.abstractCreature);
		}
		Die();
		SwitchBurrow(null);
	}

	public void SwitchBurrow(SandGrubBurrow newBurrow)
	{
		if (burrow != null && burrow.grub == this)
		{
			burrow.grub = null;
		}
		burrow = newBurrow;
		if (newBurrow != null)
		{
			base.abstractCreature.pos = newBurrow.room.GetWorldCoordinate(newBurrow.pos);
			newBurrow.grub = this;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].HardSetPosition(burrow.pos - burrow.dir * (15f - (float)i));
				base.bodyChunks[i].vel = default(Vector2);
			}
			tentacle?.Reset(burrow.pos - burrow.dir * 10f);
			base.graphicsModule?.Reset();
		}
		ai?.SwitchedBurrow();
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (burrow != null)
		{
			SwitchBurrow(burrow);
		}
	}

	public override void Die()
	{
		base.Die();
		burrow?.GrubKilled();
	}

	public void Act(bool eu)
	{
		ai.Update();
		if (ai.behavior == SandGrubAI.Behavior.Peek)
		{
			base.bodyChunks[0].vel.x += stretchDir;
			base.bodyChunks[1].vel.x += stretchDir * 0.5f;
		}
		else if (ai.behavior == SandGrubAI.Behavior.Hunt)
		{
			if (base.grasps[0] == null)
			{
				base.bodyChunks[0].vel += new Vector2(stretchDir * 1.5f, 0f) + Custom.RNV() * 0.5f;
				base.bodyChunks[1].vel += new Vector2(stretchDir * 0.5f, 0f) + Custom.RNV() * 0.5f;
				if (base.safariControlled)
				{
					if (mouthOpen > 0.8f && ai.huntTarget?.realizedObject != null && Vector2.Distance(HeadChunk.pos, ai.huntTarget.realizedObject.firstChunk.pos) < 8f)
					{
						Bite(ai.huntTarget.realizedObject.firstChunk);
					}
				}
				else if (ai.huntTarget?.realizedObject != null && Vector2.Distance(HeadChunk.pos, ai.huntTarget.realizedObject.firstChunk.pos) < 8f)
				{
					Bite(ai.huntTarget.realizedObject.firstChunk);
				}
			}
			else
			{
				Carry(eu);
			}
		}
		else if (ai.behavior == SandGrubAI.Behavior.OutOfBurrow)
		{
			if (Big)
			{
				TentacleHunt(eu);
			}
			else
			{
				ReelIn();
			}
		}
	}

	public void TentacleHunt(bool eu)
	{
		tentacleHuntCounter++;
		if (tentacleHuntCounter < 6)
		{
			base.CollideWithTerrain = false;
		}
		if (base.grasps[0] != null)
		{
			Carry(eu);
		}
		bool flag = !ai.huntPos.HasValue || base.grasps[0] != null || safariRetract;
		if (base.safariControlled)
		{
			flag = safariRetract;
		}
		Vector2 vector = Custom.DirVec(base.bodyChunks[3].pos, base.bodyChunks[0].pos);
		Vector2 vector2 = vector;
		if (burrow != null)
		{
			if (!flag)
			{
				if (tetherLength == 0f)
				{
					room.PlaySound(WatcherEnums.WatcherSoundID.Big_Sand_Grub_Unfurl, burrow.pos);
				}
				if (tetherLength < 20f)
				{
					tetherLength = 20f;
				}
				tetherLength = Custom.LerpAndTick(tetherLength, tailLength, 0.01f, 2f);
				Vector2? vector3 = null;
				if (base.safariControlled)
				{
					Player.InputPackage? inputPackage = inputWithDiagonals;
					if (inputPackage.HasValue)
					{
						Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
						if (valueOrDefault.AnyDirectionalInput)
						{
							vector3 = tentacle.Tip.pos + new Vector2(valueOrDefault.x, valueOrDefault.y);
						}
					}
				}
				else
				{
					vector3 = ai.huntPos;
				}
				if (vector3.HasValue)
				{
					Vector2 valueOrDefault2 = vector3.GetValueOrDefault();
					vector2 = Custom.DirVec(tentacle.Tip.pos, valueOrDefault2);
					if (ai.HuntGracePeriod > 0f && burrow != null)
					{
						tentacle.MoveGrabDest(Vector2.Lerp(valueOrDefault2, burrow.pos, 0.5f) + burrow.dir * (ai.HuntGracePeriod * 60f));
					}
					else
					{
						tentacle.MoveGrabDest(valueOrDefault2);
					}
				}
				vector2 = Vector3.Slerp(vector2, burrow.dir, Mathf.Max(1f - (float)tentacleHuntCounter / 30f, 0f));
				tentacle.Tip.vel.y += 0.2f;
			}
			else
			{
				vector2 = Custom.DirVec(burrow.pos - burrow.dir * 10f, tentacle.Tip.pos);
				RetractTentacle();
			}
		}
		BodyChunk[] array = base.bodyChunks;
		foreach (BodyChunk obj in array)
		{
			obj.vel = Vector2.Lerp(obj.vel, tentacle.Tip.pos - tentacle.Tip.lastPos, 0.2f);
		}
		base.bodyChunks[0].vel += vector2;
		base.bodyChunks[3].vel -= vector2;
		Vector2 vector4 = Custom.PerpendicularVector(vector);
		for (int j = 0; j < tentacle.tChunks.Length; j++)
		{
			float value = 1f - (float)j / (float)(tentacle.tChunks.Length - 1);
			BigSandGrubNeck.Segment segment = tentacle.tChunks[j];
			segment.vel -= vector4 * (Vector2.Dot(segment.pos - tentacle.Tip.pos, vector4) * Mathf.InverseLerp(0.3f, 0f, value) * 0.5f);
		}
		tentacle.tChunks[tentacle.tChunks.Length - 1].vel += vector * 0.1f;
		tentacle.tChunks[tentacle.tChunks.Length - 2].vel += vector * 0.05f;
		tentacle.tChunks[tentacle.tChunks.Length - 3].vel -= vector * 0.05f;
		tentacle.tChunks[tentacle.tChunks.Length - 4].vel -= vector * 0.1f;
	}

	public void RetractTentacle()
	{
		tetherLength = Custom.LerpAndTick(tetherLength, 0f, 0.01f, (base.grasps[0] == null) ? 1.5f : 2f);
		if (base.grasps[0] != null)
		{
			tetherLength = Mathf.Max(Vector2.Distance(tentacle.Tip.pos, burrow.pos) - 10f - (float)timeBiting / 4f, tetherLength);
		}
		tentacle.ClearGrabDest();
		float num = Mathf.InverseLerp(tailLength * 0.02f, 0f, tetherLength);
		if (num > 0f)
		{
			BigSandGrubNeck.Segment[] tChunks = tentacle.tChunks;
			foreach (BigSandGrubNeck.Segment segment in tChunks)
			{
				segment.vel = Vector2.Lerp(segment.vel, burrow.pos - segment.pos, num * 0.05f);
			}
			BodyChunk[] array = base.bodyChunks;
			foreach (BodyChunk bodyChunk in array)
			{
				bodyChunk.vel = Vector2.Lerp(bodyChunk.vel, burrow.pos - bodyChunk.pos, num * 0.05f);
			}
		}
		if (tetherLength == 0f)
		{
			ai.ReturnToBurrow();
		}
	}

	public void ReelIn()
	{
		Vector2 vector = Custom.DirVec(TailChunk.pos, HeadChunk.pos);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player)
		{
			grabbedBy[0].grabbedChunk.vel -= vector * 2f;
			HeadChunk.vel += vector * 2f;
		}
		Vector2 vector3 = vector2 * stretchDir;
		base.bodyChunks[0].vel += vector3;
		base.bodyChunks[1].vel -= vector3;
		base.bodyChunks[2].vel -= vector3;
		base.bodyChunks[3].vel += vector3;
		if (burrow == null)
		{
			return;
		}
		tetherLength -= 1f;
		if (tetherLength < 0f)
		{
			returnToBurrowCounter++;
			if (returnToBurrowCounter > 20)
			{
				returnToBurrowCounter = 0;
				ai.ReturnToBurrow();
			}
		}
		else
		{
			returnToBurrowCounter = 0;
		}
	}

	public void Bite(BodyChunk chunk)
	{
		timeBiting = 0;
		if (Big)
		{
			Grab(chunk.owner, 0, chunk.index, Grasp.Shareability.CanNotShare, 0.75f, overrideEquallyDominant: false, pacifying: true);
			room.PlaySound(WatcherEnums.WatcherSoundID.Big_Sand_Grub_Bite, HeadChunk);
		}
		else
		{
			bool flag = chunk.owner is Creature creature && creature.Template.smallCreature;
			Grab(chunk.owner, 0, chunk.index, flag ? Grasp.Shareability.CanNotShare : Grasp.Shareability.NonExclusive, 0.5f, overrideEquallyDominant: false, flag);
			room.PlaySound(WatcherEnums.WatcherSoundID.Sand_Grub_Bite, HeadChunk);
		}
	}

	public void Carry(bool eu)
	{
		BodyChunk grabbedChunk = base.grasps[0].grabbedChunk;
		BodyChunk headChunk = HeadChunk;
		float num = Vector2.Distance(headChunk.pos, grabbedChunk.pos);
		Vector2 vector = Custom.DirVec(TailChunk.pos, HeadChunk.pos);
		if ((num > headChunk.rad + grabbedChunk.rad + 20f || (burrow != null && Vector2.Distance(grabbedChunk.pos, burrow.pos) > grabbedChunk.rad + (Big ? tailLength : 30f))) && buryCounter < 40)
		{
			ReleaseGrasp(0);
			return;
		}
		if (!Big)
		{
			float num2 = headChunk.mass / (headChunk.mass + grabbedChunk.mass);
			Vector2 vector2 = (headChunk.pos - grabbedChunk.pos).normalized * (num - grabbedChunk.rad);
			headChunk.vel -= vector2 * (0.5f * (1f - num2));
			headChunk.pos -= vector2 * (1f - num2);
			grabbedChunk.vel += vector2 * (0.5f * num2);
		}
		else
		{
			grabbedChunk.MoveFromOutsideMyUpdate(eu, headChunk.pos + vector * 5f);
			grabbedChunk.owner.PushOutOf(headChunk.pos, headChunk.rad, grabbedChunk.index);
			grabbedChunk.vel = headChunk.vel;
		}
		bool flag = Big || !(base.grasps[0].grabbed is Creature creature) || creature.Template.bodySize < 1.25f;
		if (!Big && timeBiting > (flag ? 320 : 120) && UnityEngine.Random.value < 0.01f && buryCounter < 40)
		{
			ai.GiveUpOnEating();
			ReleaseGrasp(0);
		}
		else if (Big && base.grasps[0].grabbed is Player player && UnityEngine.Random.value < player.Wiggle * 0.03f && buryCounter < 40)
		{
			ReleaseGrasp(0);
			Stun(10);
		}
		else if (flag)
		{
			Bury(grabbedChunk, eu);
		}
	}

	public override void Stun(int st)
	{
		base.Stun(st);
		if (st >= 10 && ai.behavior != SandGrubAI.Behavior.OutOfBurrow && !Big)
		{
			ai.LeaveBurrow();
		}
	}

	public void Bury(BodyChunk chunk, bool eu)
	{
		if (burrow == null)
		{
			return;
		}
		if (Custom.DistLess(chunk.pos, burrow.pos, Mathf.Max(chunk.rad, 4f) + 2f) || buryCounter >= 40 || room.GetTile(chunk.pos).Solid || (room.terrain != null && room.terrain.Contains(chunk.pos)))
		{
			buryCounter++;
		}
		else
		{
			buryCounter = Mathf.Max(buryCounter - 2, 0);
		}
		if (buryCounter > 40 && chunk.owner.grabbedBy.Count == 1)
		{
			if (chunk.owner is Creature creature)
			{
				creature.Stun(50);
			}
			chunk.owner.CollideWithTerrain = false;
			Vector2 vector = burrow.pos - burrow.dir * (chunk.rad * 2f + 10f);
			BodyChunk[] array = chunk.owner.bodyChunks;
			foreach (BodyChunk obj in array)
			{
				obj.vel *= 0.15f;
				obj.RelativeMoveFromOutsideMyUpdate(eu, Vector2.ClampMagnitude(vector - chunk.pos, 1f + (float)(buryCounter - 40) * 0.1f));
			}
			if (buryCounter > 80 && Custom.DistLess(chunk.pos, vector, 5f))
			{
				ReleaseGrasp(0);
				(chunk.owner as Creature)?.SetKillTag(base.abstractCreature);
				(chunk.owner as Creature)?.Die();
				chunk.owner.abstractPhysicalObject.Destroy();
				chunk.owner.Destroy();
				buryCounter = 0;
			}
		}
	}

	public override void ReleaseGrasp(int grasp)
	{
		if (grasp == 0 && base.grasps[0] != null && !base.grasps[grasp].grabbed.CollideWithTerrain)
		{
			base.grasps[grasp].grabbed.CollideWithTerrain = true;
		}
		base.ReleaseGrasp(grasp);
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		Creature creature = otherObject as Creature;
		if (myChunk == 0 && ai.WantsToEat(otherObject.abstractPhysicalObject) && base.grasps[0] == null && (ai.behavior == SandGrubAI.Behavior.Hunt || (Big && ai.behavior == SandGrubAI.Behavior.OutOfBurrow)) && (!Big || ai.HuntGracePeriod == 0f) && mouthOpen > 0.5f)
		{
			Bite(otherObject.bodyChunks[otherChunk]);
		}
		else if (creature != null && ai.DynamicRelationship(creature.abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid)
		{
			ai.Scare(200);
		}
		if (creature != null)
		{
			ai.TouchedCreature(creature);
		}
	}

	public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
	{
		buryCounter = 0;
		return base.Grab(obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
	}

	public override void InitiateGraphicsModule()
	{
		if (Big)
		{
			if (base.graphicsModule == null)
			{
				GraphicsModule graphicsModule2 = (base.graphicsModule = new BigSandGrubGraphics(this));
			}
		}
		else if (base.graphicsModule == null)
		{
			GraphicsModule graphicsModule2 = (base.graphicsModule = new SandGrubGraphics(this));
		}
	}

	public override void Grabbed(Grasp grasp)
	{
		base.Grabbed(grasp);
		if (room != null)
		{
			room.InGameNoise(new InGameNoise(grasp.grabber.mainBodyChunk.pos, 350f, grasp.grabber, 1f));
		}
	}

	public void ThrowByPlayer()
	{
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (hitAppendage != null)
		{
			damage *= 0.25f;
			stunBonus *= 0.5f;
		}
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
		if (Big && hitChunk != null && directionAndMomentum.HasValue)
		{
			tentacle.Tip.vel += directionAndMomentum.Value / 0.2f;
			if (damage >= 0.25f || UnityEngine.Random.value > State.health)
			{
				ai.Scare(UnityEngine.Random.Range(120, 200));
			}
		}
	}

	public void BitByPlayer(Grasp grasp, bool eu)
	{
		base.bodyChunks[3 - BitesLeft].rad *= 0.5f;
		bodyChunkConnections[3].active = false;
		bodyChunkConnections[3 - BitesLeft].distance *= 0.25f;
		BitesLeft--;
		room.PlaySound(SoundID.Slugcat_Eat_Centipede, base.firstChunk);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].MoveFromOutsideMyUpdate(eu, Vector2.Lerp(base.bodyChunks[0].pos, grasp.grabber.mainBodyChunk.pos, 0.8f - 0.15f * (float)i));
		}
		if (BitesLeft < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			base.abstractCreature.destroyOnAbstraction = true;
			base.abstractCreature.saveCreature = false;
		}
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		if (segment == 0)
		{
			if (burrow != null)
			{
				return burrow.pos;
			}
			return tentacle.tChunks[segment].pos;
		}
		return tentacle.tChunks[segment - 1].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		ApplyForceOnSegment(pos.prevSegment, momentum * (1f - pos.distanceToNext));
		ApplyForceOnSegment(pos.prevSegment + 1, momentum * pos.distanceToNext);
	}

	public void ApplyForceOnSegment(int segment, Vector2 momentum)
	{
		if (segment > 0 && segment <= tentacle.tChunks.Length)
		{
			tentacle.tChunks[segment - 1].pos += momentum / 0.2f;
			tentacle.tChunks[segment - 1].vel += momentum / 0.1f;
		}
	}
}
