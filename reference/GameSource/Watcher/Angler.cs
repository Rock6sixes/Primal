using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class Angler : Creature
{
	public AnglerAI AI;

	public float flip;

	public float rudderTurnWay;

	public int consiousCounter;

	public float mouthOpenness;

	public int jawState;

	public int jawProgress;

	public float swimSpeed;

	public float swimSpeedGoal;

	public Vector2 swimDir;

	public LightSource lightSource;

	public bool safariLightInput;

	public bool squeeze;

	public float squeezeFac;

	public Vector2 GlowPos
	{
		get
		{
			if (base.graphicsModule != null)
			{
				return (base.graphicsModule as AnglerGraphics).lightPos;
			}
			return base.firstChunk.pos;
		}
	}

	public Angler(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[3];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 25f, 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 20f, 1.8f);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 15f, 1.2f);
		bodyChunkConnections = new BodyChunkConnection[3];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 20f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 24f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[2], 44f, BodyChunkConnection.Type.Push, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.buoyancy = 0.9f;
		base.waterFriction = 0.97f;
		base.GoThroughFloors = true;
		currentAffectiveness = 0.1f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new AnglerGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
	}

	public void JawsSlamShut()
	{
		Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
		bool flag = false;
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int num = room.physicalObjects[i].Count - 1; num >= 0; num--)
			{
				if (!(room.physicalObjects[i][num] is EnergyCell) && (room.physicalObjects[i][num].abstractPhysicalObject.rippleLayer == abstractPhysicalObject.rippleLayer || room.physicalObjects[i][num].abstractPhysicalObject.rippleBothSides || abstractPhysicalObject.rippleBothSides))
				{
					for (int j = 0; j < room.physicalObjects[i][num].bodyChunks.Length; j++)
					{
						BodyChunk bodyChunk = room.physicalObjects[i][num].bodyChunks[j];
						Vector2 vector2 = Custom.ClosestPointOnLineSegment(base.firstChunk.pos, base.firstChunk.pos + vector * 60f, bodyChunk.pos);
						float num2 = Vector2.Distance(base.firstChunk.pos, vector2) / 60f;
						if (!(Vector2.Distance(vector2, bodyChunk.pos) < bodyChunk.rad + num2 * 25f))
						{
							continue;
						}
						float num3 = 0f;
						for (int k = 0; k < room.physicalObjects[i][num].bodyChunks.Length; k++)
						{
							num3 += room.physicalObjects[i][num].bodyChunks[k].rad;
						}
						if (num3 < 30f)
						{
							flag = true;
							if (room.physicalObjects[i][num] is Creature creature)
							{
								AI.tracker.ForgetCreature(creature.abstractCreature);
								creature.Die();
							}
							room.physicalObjects[i][num].Destroy();
						}
						break;
					}
				}
			}
		}
		int num4 = Random.Range(5, 11) * ((!flag) ? 1 : 3);
		for (int l = 0; l < num4; l++)
		{
			Vector2 pos = base.firstChunk.pos + vector * (80f * Random.value) + Random.insideUnitCircle * 20f;
			Vector2 vel = Random.insideUnitCircle * 10f;
			if (room.PointSubmerged(pos))
			{
				room.AddObject(new Bubble(pos, vel, bottomBubble: false, fakeWaterBubble: false));
			}
			else
			{
				room.AddObject(new WaterDrip(pos, vel, waterColor: false));
			}
		}
		if (flag)
		{
			room.AddObject(new ShockWave(base.firstChunk.pos + vector * 40f, 170f, 0.015f, 7));
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (lightSource == null)
		{
			lightSource = new LightSource(base.firstChunk.pos, environmentalLight: false, Color.green, this);
			lightSource.affectedByPaletteDarkness = 0.5f;
			room.AddObject(lightSource);
		}
		else if (base.graphicsModule == null)
		{
			lightSource.setPos = base.firstChunk.pos;
			lightSource.setRad = 300f;
			lightSource.setAlpha = 0.5f;
			if (lightSource.slatedForDeletetion || lightSource.room != room)
			{
				lightSource = null;
			}
		}
		Vector2 lhs = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
		if (squeeze || enteringShortCut.HasValue)
		{
			squeezeFac = Mathf.Min(0.95f, squeezeFac + 0.02f);
		}
		else
		{
			squeezeFac = Mathf.Max(0f, squeezeFac - 1f / 30f);
		}
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].terrainSqueeze = 1f - squeezeFac;
		}
		squeeze = false;
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (base.Consious)
		{
			Act();
		}
		else
		{
			jawState = 0;
			jawProgress = 0;
			mouthOpenness = Custom.LerpAndTick(mouthOpenness, Mathf.Clamp01((1f - Vector2.Dot(lhs, Custom.DirVec(base.bodyChunks[2].pos, base.bodyChunks[0].pos))) * 5f + 0.2f), 0.08f, 0.02f);
			swimSpeed = Custom.LerpAndTick(swimSpeed, 0f, 0.0032f, 0.00032f);
		}
		float num = (0.5f + base.bodyChunks[0].submersion * 0.5f) * base.bodyChunks[1].submersion * base.bodyChunks[2].submersion;
		base.firstChunk.vel += (Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) + swimDir * 0.4f).normalized * (Mathf.Pow(swimSpeed * 2f, 2f) * 2f * num);
		Vector2 b = (base.bodyChunks[0].vel + base.bodyChunks[2].vel) * 0.5f;
		base.bodyChunks[0].vel = Vector2.Lerp(base.bodyChunks[0].vel, b, Mathf.Pow(num, 0.2f) * (base.bodyChunks[2].mass / (base.bodyChunks[0].mass + base.bodyChunks[2].mass)) * 0.25f);
		base.bodyChunks[2].vel = Vector2.Lerp(base.bodyChunks[2].vel, b, Mathf.Pow(num, 0.2f) * (1f - base.bodyChunks[2].mass / (base.bodyChunks[0].mass + base.bodyChunks[2].mass)) * 0.25f) * (1f - 0.1f * Mathf.Pow(num, 0.2f));
	}

	public void Act()
	{
		AI.Update();
		Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
		Vector2 lhs = Custom.PerpendicularVector(vector);
		consiousCounter++;
		if (jawState == 0)
		{
			float num = ((AI.lockedCreature != null) ? 0.3f : 0.15f);
			float to = 0.1f + (Mathf.Sin((float)consiousCounter * 0.07f) + 1f) / 2f * (0.25f + (Mathf.Max(Mathf.Abs(Mathf.Sin((float)consiousCounter * 0.005f)), 0.5f) - 0.5f) / 0.5f * 0.75f * num);
			mouthOpenness = Custom.LerpAndTick(mouthOpenness, to, 0.2f, 0.1f);
			jawProgress = 0;
		}
		else if (jawState == 1)
		{
			int num2 = 44;
			jawProgress++;
			mouthOpenness = Custom.LerpAndTick(mouthOpenness, Mathf.Pow((float)jawProgress / (float)num2 - 1f, 5f) + 1f, 0.1f, 0.02f);
			if (jawProgress >= num2)
			{
				jawState++;
				jawProgress = 0;
			}
		}
		else if (jawState == 2)
		{
			mouthOpenness = Mathf.Clamp01(1f - Mathf.Abs(Mathf.Sin((float)consiousCounter * 0.8f) * 0.05f + Mathf.Sin((float)consiousCounter * 0.175f) * 0.14f));
			jawProgress = 0;
		}
		else if (jawState == 3)
		{
			int num3 = 4;
			jawProgress++;
			mouthOpenness = 1f - Mathf.Pow((float)jawProgress / (float)num3, 2f);
			if (jawProgress >= num3)
			{
				jawState++;
				jawProgress = 0;
				JawsSlamShut();
				room.PlaySound(WatcherEnums.WatcherSoundID.Angler_Bite, base.firstChunk);
				if ((base.graphicsModule as AnglerGraphics)?.shriekSound != null)
				{
					(base.graphicsModule as AnglerGraphics).shriekSound.alive = false;
					(base.graphicsModule as AnglerGraphics).shriekSound = null;
				}
			}
		}
		else if (jawState == 4)
		{
			int num4 = 12;
			jawProgress++;
			mouthOpenness = (1f - (Mathf.Pow((float)(-jawProgress) / (float)num4, 3f) + 1f)) * 0.25f;
			if (jawProgress >= num4)
			{
				jawState = 0;
				jawProgress = 0;
			}
		}
		if (AI.behavior == AnglerAI.Behavior.GetUnstuck && base.Submersion > 0f)
		{
			float num5 = Mathf.InverseLerp(0.5f, 1f, AI.stuckTracker.Utility());
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].vel += Custom.RNV() * (Random.value * 10f * num5);
				base.bodyChunks[i].terrainSqueeze = Mathf.Lerp(base.bodyChunks[i].terrainSqueeze, 0.1f, num5);
			}
		}
		Vector2 pos = base.firstChunk.pos;
		MovementConnection movementConnection = (AI.pathFinder as FishPather).FollowPath(room.GetWorldCoordinate(pos), actuallyFollowingThisPath: true);
		if (movementConnection == default(MovementConnection))
		{
			for (int j = 0; j < 8; j++)
			{
				movementConnection = (AI.pathFinder as FishPather).FollowPath(room.GetWorldCoordinate(pos + Custom.eightDirections[j].ToVector2() * 20f), actuallyFollowingThisPath: true);
				if (movementConnection != default(MovementConnection))
				{
					break;
				}
			}
		}
		if (!base.safariControlled && AI.stuckTracker.satisfiedWithThisPosition)
		{
			AI.stuckTracker.satisfiedWithThisPosition = false;
		}
		MovementConnection movementConnection2 = default(MovementConnection);
		if (base.safariControlled)
		{
			Player.InputPackage? inputPackage = inputWithDiagonals;
			if (inputPackage.HasValue)
			{
				Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
				AI.stuckTracker.satisfiedWithThisPosition = base.Submersion > 0.3f;
				if (valueOrDefault.analogueDir.magnitude > 0f)
				{
					swimDir += valueOrDefault.analogueDir.normalized;
				}
				else if (valueOrDefault.x != 0 || valueOrDefault.y != 0)
				{
					swimDir += new Vector2(valueOrDefault.x, valueOrDefault.y).normalized;
				}
				swimDir = swimDir.normalized;
				if (valueOrDefault.AnyDirectionalInput)
				{
					MovementConnection.MovementType movementType = MovementConnection.MovementType.Standard;
					if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					{
						movementType = MovementConnection.MovementType.ShortCut;
					}
					else
					{
						for (int k = 0; k < 3; k++)
						{
							if (movementType == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
							{
								break;
							}
							Vector2 vector2 = base.mainBodyChunk.pos + swimDir * 20f * k;
							IntVector2[] fourDirections = Custom.fourDirections;
							foreach (IntVector2 intVector in fourDirections)
							{
								if (room.GetTile(vector2 + intVector.ToVector2() * 20f).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
								{
									movementType = MovementConnection.MovementType.BigCreatureShortCutSqueeze;
									break;
								}
							}
						}
					}
					movementConnection2 = new MovementConnection(movementType, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + swimDir * 40f), 2);
				}
				safariLightInput = valueOrDefault.jmp;
				bool flag = lastInputWithDiagonals?.pckp ?? false;
				if (valueOrDefault.pckp && !flag && jawState == 0)
				{
					jawState = 1;
				}
				if (jawState == 1 && !valueOrDefault.pckp)
				{
					jawState = 0;
				}
				if (jawState == 2 && !valueOrDefault.pckp)
				{
					if (AI.angerTimer > 0)
					{
						jawState++;
					}
					else
					{
						jawState = 0;
					}
				}
			}
		}
		if (base.safariControlled && (movementConnection == default(MovementConnection) || !base.AllowableControlledAIOverride(movementConnection.type) || (base.AllowableControlledAIOverride(movementConnection2.type) && movementConnection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)))
		{
			movementConnection = movementConnection2;
		}
		if (movementConnection != default(MovementConnection) && (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation))
		{
			enteringShortCut = movementConnection.StartTile;
			if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				NPCTransportationDestination = movementConnection.destinationCoord;
			}
			return;
		}
		if (!base.safariControlled)
		{
			if (AI.floatGoalPos.HasValue && AI.pathFinder.GetDestination.TileDefined && AI.pathFinder.GetDestination.room == room.abstractRoom.index && room.VisualContact(base.mainBodyChunk.pos, AI.floatGoalPos.Value))
			{
				swimDir = Custom.DirVec(base.mainBodyChunk.pos, AI.floatGoalPos.Value);
			}
			else if (movementConnection != default(MovementConnection))
			{
				swimDir = Custom.DirVec(pos, room.MiddleOfTile(movementConnection.DestTile));
				WorldCoordinate destinationCoord = movementConnection.destinationCoord;
				for (int m = 0; m < 4; m++)
				{
					MovementConnection movementConnection3 = (AI.pathFinder as FishPather).FollowPath(destinationCoord, actuallyFollowingThisPath: false);
					if (movementConnection3 != default(MovementConnection) && movementConnection3.destinationCoord.TileDefined && movementConnection3.destinationCoord.room == room.abstractRoom.index && room.VisualContact(movementConnection.destinationCoord.Tile, movementConnection3.DestTile))
					{
						swimDir += Custom.DirVec(pos, room.MiddleOfTile(movementConnection3.DestTile));
						destinationCoord = movementConnection3.destinationCoord;
					}
				}
				swimDir = swimDir.normalized;
			}
		}
		if (movementConnection != default(MovementConnection))
		{
			if (shortcutDelay < 1)
			{
				squeeze = movementConnection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze;
			}
			WorldCoordinate destinationCoord2 = movementConnection.destinationCoord;
			for (int n = 0; n < 2; n++)
			{
				MovementConnection movementConnection4 = (AI.pathFinder as FishPather).FollowPath(destinationCoord2, actuallyFollowingThisPath: false);
				if (movementConnection4 != default(MovementConnection))
				{
					if (shortcutDelay < 1 && (movementConnection4.type == MovementConnection.MovementType.ShortCut || movementConnection4.type == MovementConnection.MovementType.NPCTransportation || movementConnection4.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze || room.aimap.getAItile(movementConnection4.DestTile).narrowSpace))
					{
						squeeze = true;
					}
					destinationCoord2 = movementConnection4.destinationCoord;
				}
			}
			if (base.safariControlled && shortcutDelay < 1 && movementConnection2 != default(MovementConnection) && movementConnection2.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
			{
				squeeze = true;
			}
		}
		float to2 = flip;
		if (base.Submersion < 0.4f)
		{
			if (base.bodyChunks[0].ContactPoint.y < 0 && Random.value < 0.05f)
			{
				base.mainBodyChunk.vel += Custom.DegToVec(Mathf.Lerp(-45f, 45f, Random.value)) * Mathf.Lerp(6f, 26f, Random.value);
				to2 = ((lhs.y > 0f) ? 1f : (-1f));
				flip = Custom.LerpAndTick(flip, to2, Random.value * 0.5f, 0.075f);
			}
		}
		else
		{
			to2 = ((Mathf.Abs(swimDir.x) > 0.2f) ? (((swimDir.x > 0f) ? 0.9f : (-0.9f)) + Mathf.Sin((float)consiousCounter * 0.055f) * 0.1f) : flip);
			to2 *= Mathf.Clamp01(Vector2.Dot(vector, swimDir)) * 0.8f + 0.2f;
		}
		flip = Custom.LerpAndTick(flip, to2, 0.018f, 0.005f);
		rudderTurnWay = Custom.LerpAndTick(rudderTurnWay, Vector2.Dot(lhs, swimDir), 0.2f, 0.002f);
		if (Vector2.Dot(vector, swimDir) < 0f)
		{
			rudderTurnWay = ((rudderTurnWay > 0f) ? 1f : (-1f));
		}
		WeightedPush(0, 1, swimDir, 4f * ((1f - Mathf.Abs(flip)) * 0.5f + 0.5f) * base.bodyChunks[0].submersion * (0.7f + 0.3f * swimSpeedGoal) + ((AI.behavior == AnglerAI.Behavior.Stalk) ? 0.5f : 0f));
		if (squeeze)
		{
			base.firstChunk.vel *= 0.85f;
			base.firstChunk.pos += swimDir * 2.5f;
			base.firstChunk.vel += swimDir * 2.5f;
		}
		float num6 = 1f;
		WorldCoordinate getDestination = AI.pathFinder.GetDestination;
		if (!base.safariControlled && getDestination != default(WorldCoordinate) && getDestination.room == room.abstractRoom.index)
		{
			num6 *= Mathf.Min(Vector2.Distance(base.firstChunk.pos, room.MiddleOfTile(getDestination)), 70f) / 70f;
		}
		swimSpeed = Custom.LerpAndTick(swimSpeed, swimSpeedGoal * num6, 0.01f, 0.05f);
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (otherObject is Creature creature && AI.lockedCreature != null && creature.abstractCreature == AI.lockedCreature.representedCreature && (!base.safariControlled || safariLightInput))
		{
			AI.angerTimer = Random.Range(200, 700);
			(base.graphicsModule as AnglerGraphics)?.Shriek();
		}
		if (!base.safariControlled && jawState == 2 && Mathf.Abs(flip) > 0.55f && Vector2.Dot(Custom.DirVec(base.bodyChunks[1].pos, base.firstChunk.pos), Custom.DirVec(base.firstChunk.pos, otherObject.bodyChunks[otherChunk].pos)) > 0.1f)
		{
			jawState++;
		}
	}

	public override void Die()
	{
		base.Die();
		base.buoyancy = 0.92f;
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (RippleViolenceCheck(source))
		{
			if (damage > 0.05f)
			{
				AI.angerTimer = Random.Range(200, 500);
				(base.graphicsModule as AnglerGraphics)?.Shriek();
			}
			Vector2 lhs = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
			if (hitChunk.index == 0 && type == DamageType.Stab && Vector2.Dot(lhs, Custom.DirVec(base.bodyChunks[0].pos, source.pos)) > 1f - 0.5f * mouthOpenness * (0.3f + 0.7f * Mathf.Abs(flip)))
			{
				damage *= 2f;
				stunBonus *= 2f;
			}
			base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
		}
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
	{
		return chunk.index != 2;
	}

	public override void Stun(int st)
	{
		base.Stun(st);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) + vector * ((float)(2 - i) * 20f);
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 3f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}
}
