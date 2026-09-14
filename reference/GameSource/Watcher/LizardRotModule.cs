using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class LizardRotModule : DaddyLongLegs.IHaveRotParts
{
	public static List<CreatureTemplate> rotTemplates = new List<CreatureTemplate>();

	public Lizard lizard;

	public List<IntVector2> scratchPath;

	public bool hangingInTentacle;

	public DaddyTentacle[] tentacles;

	public Vector2 moveDirection = new Vector2(0f, -1f);

	public bool tentaclesHoldOn;

	public bool moving;

	public List<IntVector2> pastPositions;

	public int notFollowingPathToCurrentGoalCounter;

	public float unconditionalSupport;

	public int legsGrabbing;

	public int stuckCounter;

	public PlacedObject stuckPos;

	public bool daddyMode;

	public bool nearNormalClimbable;

	public bool transforming;

	public int transfromTimer;

	public int sizzleSoundCooldown;

	public int vocalizeSoundCooldown;

	public ChunkDynamicSoundLoop hissLoop;

	public int hissTime;

	public float currentHissVolume;

	public Color rotColor;

	public Room room => lizard.room;

	public DaddyGraphics.IHaveRotGraphics RotGraphicsOwner => (lizard.graphicsModule as LizardGraphics).rotGraphics;

	public DaddyTentacle[] RotTentacles => tentacles;

	public ArtificialIntelligence RotAI => lizard.AI;

	public Vector2 RotMoveDirection => moveDirection;

	public bool RotSizeClass => true;

	public Color RotEyeColor => rotColor;

	public int RotEyesClose => 0;

	public bool RotDigesting => transforming;

	public int RotStuckCounter => stuckCounter;

	public LizardRotModule(Lizard lizard)
	{
		this.lizard = lizard;
		hissLoop = new ChunkDynamicSoundLoop(lizard.firstChunk);
		hissLoop.Volume = 0f;
		if (lizard.LizardState.rotType == LizardState.RotType.Slight)
		{
			tentacles = new DaddyTentacle[0];
		}
		else
		{
			tentacles = new DaddyTentacle[Random.Range(5, (!RotSizeClass) ? 8 : 10)];
			float num = Mathf.Lerp((!RotSizeClass) ? 816f : 1440f, (float)tentacles.Length * ((!RotSizeClass) ? 144f : 192f), 0.5f);
			List<float> list = new List<float>();
			for (int i = 0; i < tentacles.Length; i++)
			{
				list.Add(num / (float)tentacles.Length);
			}
			for (int j = 0; j < 5 * tentacles.Length; j++)
			{
				int index = Random.Range(0, tentacles.Length);
				float num2 = list[index] * Random.value * 0.3f;
				if (list[index] - num2 > 100f)
				{
					list[Random.Range(0, tentacles.Length)] += num2;
					list[index] -= num2;
				}
			}
			for (int k = 0; k < tentacles.Length; k++)
			{
				tentacles[k] = new DaddyTentacle(lizard, this, lizard.bodyChunks[k % lizard.bodyChunks.Length], list[k], k, Custom.DegToVec(Mathf.Lerp(0f, 360f, (float)k / (float)tentacles.Length)));
				lizard.appendages.Add(new PhysicalObject.Appendage(lizard, k, tentacles[k].tChunks.Length + 1));
			}
		}
		if (lizard.abstractCreature.world.region != null)
		{
			rotColor = lizard.abstractCreature.world.region.regionParams.corruptionEffectColor;
		}
		else
		{
			rotColor = new Color(0f, 0f, 1f);
		}
	}

	public void Hiss(int duration)
	{
		if (hissTime < 0)
		{
			hissTime = 0;
		}
		hissTime += duration;
	}

	public void SwitchToDaddyMode()
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].Reset(lizard.bodyChunks[1].pos);
			lizard.appendages[i].canBeHit = true;
		}
		if (lizard.graphicsModule != null)
		{
			for (int j = 0; j < tentacles.Length; j++)
			{
				(RotGraphicsOwner as LizardRotGraphics).legs[j].Reset(lizard.bodyChunks[1].pos);
			}
		}
		daddyMode = true;
		if (rotTemplates.Contains(lizard.abstractCreature.creatureTemplate))
		{
			return;
		}
		CreatureTemplate creatureTemplate = null;
		for (int k = 0; k < rotTemplates.Count; k++)
		{
			if (rotTemplates[k].type == lizard.Template.type)
			{
				creatureTemplate = rotTemplates[k];
				break;
			}
		}
		if (creatureTemplate == null)
		{
			creatureTemplate = new CreatureTemplate(lizard.abstractCreature.creatureTemplate);
			creatureTemplate.daddyCorruptionImmune = true;
			CreatureTemplate creatureTemplate2 = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DaddyLongLegs);
			creatureTemplate.pathingPreferencesTiles = creatureTemplate2.pathingPreferencesTiles;
			creatureTemplate.pathingPreferencesConnections = creatureTemplate2.pathingPreferencesConnections;
			rotTemplates.Add(creatureTemplate);
		}
		lizard.abstractCreature.creatureTemplate = creatureTemplate;
		lizard.AI.pathFinder.Reset(room);
	}

	public void SwitchToLizardMode()
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			lizard.appendages[i].canBeHit = false;
		}
		daddyMode = false;
		if (rotTemplates.Contains(lizard.abstractCreature.creatureTemplate))
		{
			lizard.abstractCreature.creatureTemplate = StaticWorld.GetCreatureTemplate(lizard.Template.type);
			lizard.AI.pathFinder.Reset(room);
		}
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		segment--;
		if (segment < 0)
		{
			return tentacles[appendage].connectedChunk.pos;
		}
		return tentacles[appendage].tChunks[segment].pos;
	}

	public void ApplyForceOnAppendage(PhysicalObject.Appendage.Pos pos, Vector2 momentum)
	{
		if (pos.prevSegment > 0)
		{
			tentacles[pos.appendage.appIndex].tChunks[pos.prevSegment - 1].pos += momentum / 0.04f * (1f - pos.distanceToNext);
			tentacles[pos.appendage.appIndex].tChunks[pos.prevSegment - 1].vel += momentum / 0.04f * (1f - pos.distanceToNext);
		}
		else
		{
			tentacles[pos.appendage.appIndex].connectedChunk.pos += momentum / tentacles[pos.appendage.appIndex].connectedChunk.mass * (1f - pos.distanceToNext);
			tentacles[pos.appendage.appIndex].connectedChunk.vel += momentum / tentacles[pos.appendage.appIndex].connectedChunk.mass * (1f - pos.distanceToNext);
		}
		tentacles[pos.appendage.appIndex].tChunks[pos.prevSegment].pos += momentum / 0.04f * pos.distanceToNext;
		tentacles[pos.appendage.appIndex].tChunks[pos.prevSegment].vel += momentum / 0.04f * pos.distanceToNext;
	}

	public void NewRoom(Room newRoom)
	{
		pastPositions = new List<IntVector2>();
		if (lizard.LizardState.rotType == LizardState.RotType.Full && !rotTemplates.Contains(lizard.abstractCreature.creatureTemplate))
		{
			SwitchToDaddyMode();
		}
		daddyMode = rotTemplates.Contains(lizard.abstractCreature.creatureTemplate);
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].NewRoom(newRoom);
			lizard.appendages[i].canBeHit = daddyMode;
		}
	}

	public void PlaceInRoom(Room placeRoom)
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			IntVector2 tilePosition = room.GetTilePosition(tentacles[i].connectedChunk.pos);
			Vector2 vector = Custom.DirVec(lizard.firstChunk.pos, tentacles[i].connectedChunk.pos);
			IntVector2 tilePosition2 = room.GetTilePosition(tentacles[i].connectedChunk.pos + vector * tentacles[i].idealLength);
			List<IntVector2> path = new List<IntVector2>();
			room.RayTraceTilesList(tilePosition.x, tilePosition.y, tilePosition2.x, tilePosition2.y, ref path);
			for (int j = 1; j < path.Count; j++)
			{
				if (room.GetTile(path[j]).Solid)
				{
					path.RemoveRange(j, path.Count - j);
					break;
				}
			}
			tentacles[i].segments = path;
			for (int k = 0; k < tentacles[i].tChunks.Length; k++)
			{
				tentacles[i].tChunks[k].Reset();
			}
			tentacles[i].MoveGrabDest(room.MiddleOfTile(path[path.Count - 1]), ref scratchPath);
		}
		unconditionalSupport = 1f;
	}

	public void Update()
	{
		if (!lizard.dead)
		{
			if (transforming)
			{
				transfromTimer--;
				lizard.stun = Mathf.Max(lizard.stun, 12);
				Vector2 vector = Custom.RNV();
				for (int i = 0; i < lizard.bodyChunks.Length; i++)
				{
					vector = Vector3.Slerp(-vector.normalized, Custom.RNV(), Random.value);
					vector *= Mathf.Min(3f, Random.value * 3f / Mathf.Lerp(lizard.bodyChunks[i].mass, 1f, 0.5f));
					lizard.bodyChunks[i].pos += vector;
					lizard.bodyChunks[i].vel += vector * 0.5f;
				}
				if (lizard.graphicsModule != null && lizard.graphicsModule.bodyParts != null)
				{
					for (int j = 0; j < lizard.graphicsModule.bodyParts.Length; j++)
					{
						vector = Vector3.Slerp(-vector.normalized, Custom.RNV(), Random.value);
						vector *= Random.value * 2f;
						lizard.graphicsModule.bodyParts[j].pos += vector;
						lizard.graphicsModule.bodyParts[j].vel += vector;
						if (lizard.graphicsModule.bodyParts[j] is Limb)
						{
							(lizard.graphicsModule.bodyParts[j] as Limb).mode = Limb.Mode.Dangle;
						}
					}
				}
				if (transfromTimer <= 0)
				{
					lizard.LizardState.health = 1f;
					transforming = false;
					return;
				}
			}
		}
		else
		{
			hissTime = 0;
		}
		hissTime--;
		sizzleSoundCooldown--;
		vocalizeSoundCooldown--;
		if (!lizard.dead && (lizard.LizardState.rotType == LizardState.RotType.Full || lizard.LizardState.rotType == LizardState.RotType.Opossum) && vocalizeSoundCooldown < 0 && Random.value < 0.00055555557f)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.RotLiz_Vocalize, lizard.firstChunk);
			vocalizeSoundCooldown = Random.Range(500, 900);
		}
		if (lizard.LizardState.health <= 0.5f && sizzleSoundCooldown < 0 && Random.value < 0.0025f)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.RotLiz_Injured, lizard.firstChunk);
			for (int k = 0; k < lizard.bodyChunks.Length; k++)
			{
				int num = ((!(Random.value < 0.5f)) ? Random.Range(0, 6) : 0);
				for (int l = 0; l < num; l++)
				{
					room.AddObject(new Spark(lizard.bodyChunks[k].pos, lizard.bodyChunks[k].vel * Random.value + Custom.RNV() * (Random.value * 6f), new Color(1f, 1f, 1f), null, 6, 18));
				}
			}
			sizzleSoundCooldown = Random.Range(450, 800);
		}
		if (hissTime > 0)
		{
			currentHissVolume = Mathf.Lerp(currentHissVolume, 1f, 0.2f);
		}
		else
		{
			currentHissVolume = Mathf.Lerp(currentHissVolume, 0f, 0.05f);
		}
		hissLoop.sound = WatcherEnums.WatcherSoundID.RotLiz_Hiss;
		hissLoop.Volume = currentHissVolume;
		hissLoop.Update();
		if (!daddyMode)
		{
			return;
		}
		nearNormalClimbable = false;
		for (int m = 0; m < 8; m++)
		{
			if (lizard.room.GetTile(room.GetTilePosition(lizard.firstChunk.pos) + Custom.eightDirectionsDiagonalsLast[m]).IsSolid())
			{
				nearNormalClimbable = true;
				break;
			}
		}
		unconditionalSupport = Mathf.Max(0f, unconditionalSupport - 0.025f);
		hangingInTentacle = false;
		legsGrabbing = 0;
		for (int n = 0; n < tentacles.Length; n++)
		{
			tentacles[n].Update();
			if (tentacles[n].atGrabDest)
			{
				legsGrabbing++;
			}
		}
	}

	public void Act()
	{
		if (!daddyMode)
		{
			return;
		}
		float num = 0.6f;
		int num2 = 3;
		float num3 = 1.2f;
		float num4 = 0.2f;
		Vector2? vector = null;
		MovementConnection followingConnection = default(MovementConnection);
		if (lizard.safariControlled)
		{
			stuckPos = null;
			num = 0.25f;
			MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
			if (room.GetTile(lizard.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				type = MovementConnection.MovementType.ShortCut;
			}
			else
			{
				for (int i = 0; i < Custom.fourDirections.Length; i++)
				{
					if (room.GetTile(lizard.mainBodyChunk.pos + Custom.fourDirections[i].ToVector2() * 20f).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					{
						type = MovementConnection.MovementType.BigCreatureShortCutSqueeze;
						break;
					}
				}
			}
			if (lizard.inputWithDiagonals.HasValue)
			{
				if (lizard.inputWithDiagonals.Value.x != 0 || lizard.inputWithDiagonals.Value.y != 0)
				{
					bool flag = false;
					for (int j = 0; j < tentacles.Length; j++)
					{
						if (tentacles[j].grabChunk != null && tentacles[j].grabChunk.owner is Creature)
						{
							flag = true;
							break;
						}
					}
					if (!lizard.inputWithDiagonals.Value.pckp || flag)
					{
						vector = lizard.mainBodyChunk.pos + new Vector2(lizard.inputWithDiagonals.Value.x, lizard.inputWithDiagonals.Value.y) * 40f;
						followingConnection = new MovementConnection(type, room.GetWorldCoordinate(lizard.mainBodyChunk.pos), room.GetWorldCoordinate(vector.Value), 2);
					}
					else
					{
						moving = false;
					}
				}
				else
				{
					moving = false;
				}
				if ((lizard.inputWithDiagonals.Value.thrw && !lizard.lastInputWithDiagonals.Value.thrw) || lizard.inputWithDiagonals.Value.jmp)
				{
					for (int k = 0; k < tentacles.Length; k++)
					{
						tentacles[k].neededForLocomotion = true;
						tentacles[k].SwitchTask(DaddyTentacle.Task.Locomotion);
					}
				}
			}
			else
			{
				moving = false;
			}
			if (!moving)
			{
				unconditionalSupport = 1f;
				num3 = ((legsGrabbing <= tentacles.Length / 2) ? (0.5f + Mathf.Lerp(0f, 0.5f, legsGrabbing / (tentacles.Length / 2))) : 1f);
			}
			else if (legsGrabbing < tentacles.Length / 2)
			{
				num3 *= Mathf.Lerp(0.6f, 1f, legsGrabbing / (tentacles.Length / 2));
			}
			if (lizard.inputWithDiagonals.HasValue && lizard.inputWithDiagonals.Value.jmp)
			{
				unconditionalSupport = 0f;
				for (int l = 0; l < tentacles.Length; l++)
				{
					tentacles[l].neededForLocomotion = false;
					tentacles[l].SwitchTask(DaddyTentacle.Task.Hunt);
				}
				num3 = -1f;
			}
		}
		if (stuckPos == null)
		{
			if (notFollowingPathToCurrentGoalCounter < 200 && lizard.AI.pathFinder.GetEffectualDestination != lizard.AI.pathFinder.GetDestination)
			{
				notFollowingPathToCurrentGoalCounter++;
			}
			else if (notFollowingPathToCurrentGoalCounter > 0)
			{
				notFollowingPathToCurrentGoalCounter--;
			}
			if (notFollowingPathToCurrentGoalCounter > 100)
			{
				for (int m = 0; m < lizard.bodyChunks.Length; m++)
				{
					if (legsGrabbing != 0)
					{
						break;
					}
					if (lizard.bodyChunks[m].ContactPoint.x != 0 || lizard.bodyChunks[m].ContactPoint.y != 0)
					{
						legsGrabbing = 1;
					}
				}
			}
			int num5 = 0;
			if ((Custom.ManhattanDistance(lizard.abstractCreature.pos, lizard.AI.pathFinder.GetEffectualDestination) > num2 || notFollowingPathToCurrentGoalCounter > 100) && legsGrabbing > 0)
			{
				pastPositions.Insert(0, lizard.abstractCreature.pos.Tile);
				if (pastPositions.Count > 80)
				{
					pastPositions.RemoveAt(pastPositions.Count - 1);
				}
				for (int n = 40; n < pastPositions.Count; n++)
				{
					if (Custom.DistLess(lizard.abstractCreature.pos.Tile, pastPositions[n], 4f))
					{
						num5++;
					}
				}
			}
			if (num5 > 30)
			{
				stuckCounter++;
			}
			else
			{
				stuckCounter -= 2;
			}
			stuckCounter = Custom.IntClamp(stuckCounter, 0, 200);
			if (stuckCounter > 100)
			{
				for (int num6 = 0; num6 < lizard.bodyChunks.Length; num6++)
				{
					lizard.bodyChunks[num6].vel += Custom.RNV() * (3f * Random.value * Mathf.InverseLerp(100f, 200f, stuckCounter));
				}
			}
			if (lizard.safariControlled)
			{
				stuckCounter = 0;
			}
		}
		else
		{
			stuckCounter = 0;
		}
		if ((legsGrabbing > tentacles.Length / 2 && moving) || stuckCounter > 100)
		{
			float num7 = float.MinValue;
			int num8 = -1;
			for (int num9 = 0; num9 < tentacles.Length; num9++)
			{
				if (tentacles[num9].atGrabDest && tentacles[num9].huntCreature == null && tentacles[num9].ReleaseScore() > num7)
				{
					num7 = tentacles[num9].ReleaseScore();
					num8 = num9;
				}
			}
			if (num8 > -1)
			{
				List<IntVector2> path = null;
				tentacles[num8].UpdateClimbGrabPos(ref path);
			}
		}
		float num10 = 0f;
		float num11 = 0f;
		for (int num12 = 0; num12 < tentacles.Length; num12++)
		{
			float num13 = Mathf.Pow(tentacles[num12].chunksGripping, 0.5f);
			if (tentacles[num12].atGrabDest && tentacles[num12].grabDest.HasValue)
			{
				num11 += Mathf.Pow(Mathf.InverseLerp(Custom.LerpMap(stuckCounter, 0f, 100f, -0.1f, -1f), 0.85f, Vector2.Dot((tentacles[num12].floatGrabDest.Value - lizard.mainBodyChunk.pos).normalized, moveDirection)), 0.8f) / (float)tentacles.Length;
				num13 = Mathf.Lerp(num13, 1f, 0.75f);
			}
			num10 += num13 / (float)tentacles.Length;
		}
		num11 = Mathf.Pow(num11 * num10, Custom.LerpMap(stuckCounter, 100f, 200f, 0.8f, 0.1f));
		num10 = Mathf.Pow(num10, 0.3f);
		num10 = Mathf.Max(num10, unconditionalSupport);
		num11 = Mathf.Max(num11, unconditionalSupport);
		float num14 = 0f;
		for (int num15 = 0; num15 < tentacles.Length; num15++)
		{
			if (tentacles[num15].neededForLocomotion)
			{
				num14 += 1f / (float)tentacles.Length;
			}
		}
		if (num10 < 1f - num14)
		{
			float num16 = float.MinValue;
			int num17 = Random.Range(0, tentacles.Length);
			for (int num18 = 0; num18 < tentacles.Length; num18++)
			{
				if (!tentacles[num18].neededForLocomotion)
				{
					float num19 = 1000f / Mathf.Lerp(tentacles[num18].idealLength * (float)room.aimap.getTerrainProximity(tentacles[num18].Tip.pos), 200f, 0.8f);
					if (tentacles[num18].task == DaddyTentacle.Task.Grabbing)
					{
						num19 *= 0.01f;
					}
					if (tentacles[num18].task == DaddyTentacle.Task.Hunt)
					{
						num19 *= 0.1f;
					}
					if (tentacles[num18].task == DaddyTentacle.Task.ExamineSound)
					{
						num19 *= 0.6f;
					}
					if (num19 > num16)
					{
						num16 = num19;
						num17 = num18;
					}
				}
			}
			tentacles[num17].neededForLocomotion = true;
		}
		else if ((double)num10 > 0.85)
		{
			tentacles[Random.Range(0, tentacles.Length)].neededForLocomotion = false;
		}
		for (int num20 = 0; num20 < lizard.bodyChunks.Length; num20++)
		{
			lizard.bodyChunks[num20].vel *= Mathf.Lerp(1f, 0.95f, num10);
			lizard.bodyChunks[num20].vel.y += (lizard.gravity - lizard.buoyancy * lizard.bodyChunks[num20].submersion) * num10 * num3;
		}
		if (!lizard.safariControlled && Custom.ManhattanDistance(lizard.abstractCreature.pos, lizard.AI.pathFinder.GetEffectualDestination) < num2)
		{
			for (int num21 = 0; num21 < lizard.bodyChunks.Length; num21++)
			{
				lizard.bodyChunks[num21].vel += Vector2.ClampMagnitude(room.MiddleOfTile(lizard.AI.pathFinder.GetEffectualDestination) - lizard.bodyChunks[0].pos, 30f) / 30f * (num * num11);
			}
		}
		else if (lizard.safariControlled && vector.HasValue && Custom.ManhattanDistance(lizard.abstractCreature.pos, Custom.MakeWorldCoordinate(new IntVector2((int)vector.Value.x / 20, (int)vector.Value.y / 20), lizard.abstractCreature.Room.index)) < num2)
		{
			for (int num22 = 0; num22 < lizard.bodyChunks.Length; num22++)
			{
				lizard.bodyChunks[num22].vel += Vector2.ClampMagnitude(room.MiddleOfTile((int)vector.Value.x / 20, (int)vector.Value.y / 20) - lizard.bodyChunks[0].pos, 30f) / 30f * (num * num11);
			}
		}
		else if (lizard.followingConnection == default(MovementConnection))
		{
			lizard.followingConnection = CheckTentaclesForAccessibleTerrain();
		}
		if (lizard.safariControlled && (lizard.followingConnection == default(MovementConnection) || !lizard.AllowableControlledAIOverride(lizard.followingConnection.type)))
		{
			lizard.followingConnection = followingConnection;
		}
		moving = lizard.followingConnection != default(MovementConnection);
		if (ModManager.MMF && lizard.followingConnection == default(MovementConnection))
		{
			if (!lizard.safariControlled)
			{
				moveDirection = (moveDirection + new Vector2(0f, 0f - num / 10f)).normalized;
			}
		}
		else if (lizard.followingConnection != default(MovementConnection))
		{
			if (lizard.shortcutDelay < 1 && lizard.followingConnection.type == MovementConnection.MovementType.ShortCut)
			{
				lizard.enteringShortCut = lizard.followingConnection.StartTile;
				return;
			}
			lizard.GoThroughFloors = lizard.followingConnection.DestTile.y < lizard.followingConnection.StartTile.y;
			for (int num23 = 0; num23 < lizard.bodyChunks.Length; num23++)
			{
				lizard.bodyChunks[num23].vel += Custom.DirVec(lizard.bodyChunks[0].pos, room.MiddleOfTile(lizard.followingConnection.DestTile)) * (num * num11 * ((num23 == 0) ? 1.25f : 1f));
			}
			MovementConnection movementConnection = lizard.followingConnection;
			Vector2 vector2 = Custom.DirVec(movementConnection.StartTile.ToVector2(), movementConnection.DestTile.ToVector2());
			for (int num24 = 0; num24 < 10; num24++)
			{
				movementConnection = (lizard.AI.pathFinder as LizardPather).FollowPath(movementConnection.destinationCoord, null, actuallyFollowingThisPath: false);
				if (movementConnection == default(MovementConnection))
				{
					break;
				}
				vector2 += Custom.DirVec(movementConnection.StartTile.ToVector2(), movementConnection.DestTile.ToVector2());
			}
			moveDirection = (moveDirection + vector2.normalized * num4).normalized;
		}
		else
		{
			moveDirection = (moveDirection + new Vector2(0f, 0f - num / 10f)).normalized;
		}
	}

	public void Stun()
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			if (Random.value < 0.5f)
			{
				tentacles[i].neededForLocomotion = true;
				tentacles[i].SwitchTask(DaddyTentacle.Task.Locomotion);
			}
		}
	}

	public void Violence(float damage)
	{
		if ((((double)damage > 0.05 && lizard.LizardState.health < Random.value) || (double)lizard.LizardState.health <= 0.3) && !daddyMode && !transforming && lizard.LizardState.rotType == LizardState.RotType.Opossum)
		{
			transforming = true;
			transfromTimer = Random.Range(100, 200);
			SwitchToDaddyMode();
			room.PlaySound(WatcherEnums.WatcherSoundID.RotLiz_Erupt_Tentacles, lizard.firstChunk);
			lizard.LizardState.health = 1f;
		}
		else if (!lizard.dead)
		{
			if (vocalizeSoundCooldown <= 0)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.RotLiz_Pain, lizard.firstChunk);
				vocalizeSoundCooldown = Random.Range(500, 900);
			}
			else
			{
				lizard.voice.MakeSound(LizardVoice.Emotion.PainImpact, damage * 2f);
			}
		}
	}

	public void SpitOutOfShortCut()
	{
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].Reset(tentacles[i].connectedChunk.pos);
		}
	}

	public MovementConnection CheckTentaclesForAccessibleTerrain()
	{
		Vector2 pos = lizard.mainBodyChunk.pos;
		float num = float.MaxValue;
		Vector2 vector = lizard.mainBodyChunk.pos;
		if (lizard.AI.pathFinder.GetDestination.room == lizard.abstractCreature.pos.room && lizard.AI.pathFinder.GetDestination.NodeDefined)
		{
			vector = room.MiddleOfTile(lizard.AI.pathFinder.GetDestination);
		}
		for (int i = 0; i < tentacles.Length; i++)
		{
			for (int j = 0; j < tentacles[i].tChunks.Length; j++)
			{
				if (room.aimap.TileAccessibleToCreature(tentacles[i].tChunks[j].pos, lizard.Template) && Custom.DistLess(tentacles[i].tChunks[j].pos, vector, num))
				{
					pos = tentacles[i].tChunks[j].pos;
					num = Vector2.Distance(tentacles[i].tChunks[j].pos, vector);
				}
			}
		}
		if (num < float.MaxValue)
		{
			return new MovementConnection(MovementConnection.MovementType.Standard, room.GetWorldCoordinate(lizard.mainBodyChunk.pos), room.GetWorldCoordinate(pos), (int)(num / 20f));
		}
		return default(MovementConnection);
	}
}
