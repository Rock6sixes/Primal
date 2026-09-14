using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class Rat : Creature, IPlayerEdible
{
	public class CreatureParams
	{
		public float totalMass;

		public float sizeMultiplier;

		public float huntSpeedFactor;

		public float travelSpeedFactor;

		public float fleeSpeedFactor;

		public float idleSpeedFactor;

		public bool separateSwimBehavior;

		public float stunOnCollideVelocity;

		public float stunOnCollideDamage;

		public float stunOnCollideDuration;

		public float dominance;

		public CreatureParams()
		{
			totalMass = 0.3f;
			sizeMultiplier = 1f;
			huntSpeedFactor = 1f;
			travelSpeedFactor = 1f;
			fleeSpeedFactor = 1.4f;
			idleSpeedFactor = 1.2f;
			separateSwimBehavior = true;
			stunOnCollideVelocity = 0f;
			stunOnCollideDuration = 10f;
			stunOnCollideDamage = 0.1f;
			dominance = UnityEngine.Random.Range(0f, 1f);
		}
	}

	public RatAI AI;

	public CreatureParams creatureParams;

	public int footingCounter;

	public int specialMoveCounter;

	public IntVector2 specialMoveDestination;

	public MovementConnection lastFollowedConnection;

	public List<AbstractCreature> deadBoredCreatures = new List<AbstractCreature>();

	public Rat packLeader;

	public int packLeaderCD = 25;

	public int noJumps;

	public int bites;

	public int eaten;

	public bool Footing => footingCounter > 20;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		creatureParams = new CreatureParams();
		UnityEngine.Random.state = state;
	}

	public Rat(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5.5f * creatureParams.sizeMultiplier, creatureParams.totalMass / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 6f * creatureParams.sizeMultiplier, creatureParams.totalMass / 2f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 12f * creatureParams.sizeMultiplier, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		bites = 3;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new RatGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (bites == 1)
		{
			base.bodyChunks[0].rad = 1f;
			base.bodyChunks[0].mass = 0.1f;
		}
		if (base.Consious)
		{
			if (room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template))
			{
				footingCounter++;
				Act();
			}
			else
			{
				footingCounter = 0;
			}
			if (UnityEngine.Random.value < 0.005f && grabbedBy.Count == 0)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Rat_Chatter, base.mainBodyChunk);
			}
		}
		else
		{
			footingCounter = 0;
		}
		if (Footing)
		{
			for (int i = 0; i < 2; i++)
			{
				base.bodyChunks[i].vel *= 0.8f;
				base.bodyChunks[i].vel.y += base.gravity;
			}
		}
		if (base.Consious && !Footing && AI.behavior == RatAI.Behavior.Flee && !base.safariControlled)
		{
			for (int j = 0; j < 2; j++)
			{
				if (room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[j].pos), base.Template))
				{
					base.bodyChunks[j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * (UnityEngine.Random.value * 5f);
				}
			}
		}
		if (eaten > 0)
		{
			eaten--;
			if (eaten == 0)
			{
				Destroy();
			}
		}
	}

	public void GetPackLeader()
	{
		if (packLeaderCD <= 0 && AI.behavior != RatAI.Behavior.Flee)
		{
			packLeaderCD = ((packLeader == null) ? 100 : 1000);
			bool flag = false;
			for (int i = 0; i < AI.tracker.CreaturesCount; i++)
			{
				if (!(AI.tracker.GetRep(i).representedCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Rat) || !AI.tracker.GetRep(i).VisualContact || AI.tracker.GetRep(i).representedCreature.realizedCreature == null || !AI.tracker.GetRep(i).representedCreature.realizedCreature.Consious)
				{
					continue;
				}
				if (packLeader != null && packLeader.creatureParams.dominance <= (AI.tracker.GetRep(i).representedCreature.realizedCreature as Rat).creatureParams.dominance)
				{
					if ((AI.tracker.GetRep(i).representedCreature.realizedCreature as Rat).packLeader != null)
					{
						packLeader = (AI.tracker.GetRep(i).representedCreature.realizedCreature as Rat).packLeader;
						break;
					}
					packLeader = AI.tracker.GetRep(i).representedCreature.realizedCreature as Rat;
					flag = true;
				}
				else if (packLeader == null && creatureParams.dominance < (AI.tracker.GetRep(i).representedCreature.realizedCreature as Rat).creatureParams.dominance)
				{
					packLeader = AI.tracker.GetRep(i).representedCreature.realizedCreature as Rat;
					flag = true;
				}
			}
			if (!flag)
			{
				packLeader = null;
			}
		}
		else
		{
			packLeaderCD--;
		}
	}

	public void TryJump(Vector2 awayFromPoint)
	{
		if (base.Consious && noJumps <= 0 && (room.aimap.TileAccessibleToCreature(base.bodyChunks[0].pos, base.Template) || room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template)) && !room.aimap.getAItile(base.bodyChunks[1].pos).narrowSpace)
		{
			Vector2 vector = Custom.DirVec(awayFromPoint, (base.bodyChunks[0].pos + base.bodyChunks[1].pos) / 2f);
			vector += Custom.RNV() * 0.3f;
			vector.Normalize();
			vector = Vector3.Slerp(vector, new Vector2(0f, 1f), Custom.LerpMap(vector.y, -0.5f, 0.5f, 0.7f, 0.3f));
			base.bodyChunks[0].vel *= 0.5f;
			base.bodyChunks[1].vel *= 0.5f;
			base.bodyChunks[0].vel += vector * 17f + Custom.RNV() * 5f * UnityEngine.Random.value;
			base.bodyChunks[1].vel += vector * 17f + Custom.RNV() * 5f * UnityEngine.Random.value;
			footingCounter = 0;
			Vector2 vector2 = Custom.PerpendicularVector(vector) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			base.bodyChunks[0].vel += vector2 * 11f;
			base.bodyChunks[1].vel -= vector2 * 11f;
			if (!base.safariControlled)
			{
				noJumps = 90;
			}
		}
	}

	public void Swim()
	{
		base.GoThroughFloors = true;
		MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[0].pos), actuallyFollowingThisPath: true);
		if (movementConnection == default(MovementConnection))
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
		}
		if (movementConnection == default(MovementConnection) && Math.Abs(base.abstractCreature.pos.y - room.defaultWaterLevel) < 4)
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(new WorldCoordinate(base.abstractCreature.pos.room, base.abstractCreature.pos.x, room.defaultWaterLevel, base.abstractCreature.pos.abstractNode), actuallyFollowingThisPath: true);
		}
		if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
		{
			movementConnection = default(MovementConnection);
			if (inputWithDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithDiagonals.Value.AnyDirectionalInput)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
				}
			}
		}
		if (movementConnection != default(MovementConnection))
		{
			if (movementConnection.StartTile.y == movementConnection.DestTile.y && room.GetTile(movementConnection.DestTile).WaterSurface)
			{
				Vector2 moveTo = room.MiddleOfTile(movementConnection.DestTile);
				MoveTowards(moveTo);
				footingCounter = 0;
				return;
			}
			Vector2 moveTo2 = room.MiddleOfTile(movementConnection.DestTile);
			base.bodyChunks[0].vel *= 0.9f;
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)) * 1.4f;
			if (!base.safariControlled || base.Submersion < 0.9f)
			{
				footingCounter = Math.Max(footingCounter, 25);
				MoveTowards(moveTo2);
				footingCounter = 20;
			}
			else
			{
				base.mainBodyChunk.vel *= 0.75f;
				footingCounter = 0;
				MoveTowards(moveTo2);
				footingCounter = 0;
			}
		}
		else
		{
			base.mainBodyChunk.vel.y += 0.5f;
		}
	}

	public void Act()
	{
		GetPackLeader();
		if (base.Submersion > 0.3f && creatureParams.separateSwimBehavior)
		{
			Swim();
			AI.Update();
			return;
		}
		AI.Update();
		if (specialMoveCounter > 0)
		{
			specialMoveCounter--;
			MoveTowards(room.MiddleOfTile(specialMoveDestination));
			if (Custom.DistLess(base.mainBodyChunk.pos, room.MiddleOfTile(specialMoveDestination), 5f))
			{
				specialMoveCounter = 0;
			}
			return;
		}
		if (!room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template) && !room.aimap.TileAccessibleToCreature(base.bodyChunks[1].pos, base.Template))
		{
			footingCounter = 0;
		}
		MovementConnection followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
		if (followingConnection == default(MovementConnection))
		{
			followingConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.bodyChunks[1].pos), actuallyFollowingThisPath: true);
		}
		if (base.abstractCreature.controlled && (followingConnection == default(MovementConnection) || !AllowableControlledAIOverride(followingConnection.type)))
		{
			SafariControls(ref followingConnection);
		}
		if (followingConnection != default(MovementConnection))
		{
			if (followingConnection.type == MovementConnection.MovementType.ReachUp)
			{
				(AI.pathFinder as StandardPather).pastConnections.Clear();
			}
			if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				enteringShortCut = followingConnection.StartTile;
				if (base.safariControlled)
				{
					bool flag = false;
					List<IntVector2> list = new List<IntVector2>();
					ShortcutData[] shortcuts = room.shortcuts;
					for (int i = 0; i < shortcuts.Length; i++)
					{
						ShortcutData shortcutData = shortcuts[i];
						if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
						{
							list.Add(shortcutData.StartTile);
						}
						if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
						{
							flag = true;
						}
					}
					if (flag)
					{
						if (list.Count > 0)
						{
							list.Shuffle();
							NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
						}
						else
						{
							NPCTransportationDestination = followingConnection.destinationCoord;
						}
					}
				}
				else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
				{
					NPCTransportationDestination = followingConnection.destinationCoord;
				}
			}
			else if (followingConnection.type == MovementConnection.MovementType.OpenDiagonal || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
			{
				specialMoveCounter = 30;
				specialMoveDestination = followingConnection.DestTile;
			}
			else
			{
				Vector2 vector = room.MiddleOfTile(followingConnection.DestTile);
				if (lastFollowedConnection != default(MovementConnection) && lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
				{
					base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector) * 4f;
				}
				if (Footing)
				{
					for (int j = 0; j < 2; j++)
					{
						if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
						{
							base.bodyChunks[j].vel.x += Mathf.Min((vector.x - base.bodyChunks[j].pos.x) / 8f, 1.2f);
						}
						else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
						{
							base.bodyChunks[j].vel.y += Mathf.Min((vector.y - base.bodyChunks[j].pos.y) / 8f, 1.2f);
						}
					}
				}
				if (lastFollowedConnection != default(MovementConnection) && (Footing || room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template)) && ((followingConnection.startCoord.x != followingConnection.destinationCoord.x && lastFollowedConnection.startCoord.x == lastFollowedConnection.destinationCoord.x) || (followingConnection.startCoord.y != followingConnection.destinationCoord.y && lastFollowedConnection.startCoord.y == lastFollowedConnection.destinationCoord.y)))
				{
					base.mainBodyChunk.vel *= 0.7f;
					base.bodyChunks[1].vel *= 0.5f;
				}
				if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
				{
					footingCounter = 0;
				}
				MoveTowards(vector);
			}
			lastFollowedConnection = followingConnection;
		}
		else
		{
			base.GoThroughFloors = false;
		}
	}

	public void MoveTowards(Vector2 moveTo)
	{
		Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, moveTo);
		if (room.aimap.getAItile(base.bodyChunks[1].pos).acc >= AItile.Accessibility.Climb)
		{
			vector *= 0.5f;
		}
		if (!Footing)
		{
			vector *= 0.3f;
		}
		if (IsTileSolid(1, 0, -1) && (((double)vector.x < -0.5 && base.mainBodyChunk.pos.x > base.bodyChunks[1].pos.x + 5f) || ((double)vector.x > 0.5 && base.mainBodyChunk.pos.x < base.bodyChunks[1].pos.x - 5f)))
		{
			base.mainBodyChunk.vel.x -= ((vector.x < 0f) ? (-1f) : 1f) * 1.3f;
			base.bodyChunks[1].vel.x += ((vector.x < 0f) ? (-1f) : 1f) * 0.5f;
			if (!IsTileSolid(0, 0, 1))
			{
				base.mainBodyChunk.vel.y += 3.2f;
			}
		}
		float num = creatureParams.idleSpeedFactor;
		if (AI.behavior == RatAI.Behavior.Flee || AI.behavior == RatAI.Behavior.EscapeRain)
		{
			num = creatureParams.fleeSpeedFactor;
		}
		else if (AI.behavior == RatAI.Behavior.ReturnPrey || AI.behavior == RatAI.Behavior.Travelling || AI.behavior == RatAI.Behavior.InvestigateSound)
		{
			num = creatureParams.travelSpeedFactor;
		}
		else if (AI.behavior == RatAI.Behavior.Hunt)
		{
			num = creatureParams.huntSpeedFactor;
		}
		base.mainBodyChunk.vel += vector * (4.2f * AI.runSpeed * num);
		base.bodyChunks[1].vel -= vector * (1f * AI.runSpeed * num);
		base.GoThroughFloors = moveTo.y < base.mainBodyChunk.pos.y - 5f;
	}

	public void SafariControls(ref MovementConnection followingConnection)
	{
		followingConnection = default(MovementConnection);
		if (inputWithDiagonals.HasValue)
		{
			MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
			if (followingConnection != default(MovementConnection))
			{
				type = followingConnection.type;
			}
			if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				type = MovementConnection.MovementType.ShortCut;
			}
			if (inputWithDiagonals.Value.AnyDirectionalInput)
			{
				followingConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
			}
			else if (inputWithDiagonals.Value.y < 0)
			{
				base.GoThroughFloors = true;
			}
			else
			{
				base.GoThroughFloors = false;
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		if (otherObject is Rat && creatureParams.dominance > (otherObject as Rat).creatureParams.dominance)
		{
			(otherObject as Rat).AI.pathFinder.SetDestination(AI.pathFinder.GetDestination);
			return;
		}
		base.Collide(otherObject, myChunk, otherChunk);
		if (base.Consious && creatureParams.stunOnCollideVelocity > 0f && Vector2.Distance(base.bodyChunks[myChunk].vel, otherObject.bodyChunks[otherChunk].vel) >= creatureParams.stunOnCollideVelocity && otherObject is Creature)
		{
			(otherObject as Creature).Violence(base.bodyChunks[myChunk], base.bodyChunks[myChunk].vel * base.bodyChunks[myChunk].mass, otherObject.bodyChunks[otherChunk], null, DamageType.Blunt, creatureParams.stunOnCollideDamage, creatureParams.stunOnCollideDuration);
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		if (speed > 1.5f && firstContact)
		{
			Mathf.InverseLerp(6f, 14f, speed);
		}
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void Die()
	{
		base.Die();
	}

	public override Color ShortCutColor()
	{
		return base.ShortCutColor();
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (RippleViolenceCheck(source))
		{
			if (!base.dead)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Rat_Shriek, base.mainBodyChunk);
			}
			base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
		}
	}

	public override void Grabbed(Grasp grasp)
	{
		base.Grabbed(grasp);
		if (!base.dead && room != null)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Rat_Shriek, base.mainBodyChunk);
		}
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
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * ((-1.5f + (float)i) * 15f);
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 2f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public void BitByPlayer(Grasp grasp, bool eu)
	{
		bites--;
		if (!base.dead)
		{
			Die();
		}
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Final_Bite_Fly : SoundID.Slugcat_Bite_Fly, base.mainBodyChunk);
		base.mainBodyChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites < 1 && eaten == 0)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			eaten = 3;
		}
	}

	public void ThrowByPlayer()
	{
	}
}
