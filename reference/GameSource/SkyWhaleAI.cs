using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class SkyWhaleAI : ArtificialIntelligence
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior FollowPath = new Behavior("FollowPath", register: true);

		public static Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Vector2 currentNodePosition;

	public Vector2 previousNodePosition;

	public int testNodeIndex;

	public SkyWhale skyWhale;

	public float currentUtility;

	public int timeToTire;

	public WorldCoordinate tempIdlePos;

	public int idlePosCounter;

	public bool tryingToLeaveRoom;

	public Behavior behavior;

	public PlacedObject.SkyWhalePathfindingData CurrentNode => (creature.abstractAI as SkyWhaleAbstractAI).NodePath?.CurrentNode;

	public List<PlacedObject.SkyWhalePathfindingData> PathfindingNodes => (creature.abstractAI as SkyWhaleAbstractAI).NodePath?.PathfindingNodes;

	public bool AllowMovementBetweenRooms
	{
		get
		{
			if (!skyWhale.safariControlled)
			{
				return tryingToLeaveRoom;
			}
			return true;
		}
	}

	public SkyWhaleAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		skyWhale = creature.realizedCreature as SkyWhale;
		skyWhale.AI = this;
		AddModule(new SkyWhalePather(this, world, creature)
		{
			accessibilityStepsPerFrame = 350,
			stepsPerFrame = 340,
			walkPastPointOfNoReturn = true,
			timeUntilPathCertain = 20
		});
		AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: true));
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		AddModule(new UtilityComparer(this));
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
	}

	public override void NewRoom(Room room)
	{
		WorldCoordinate worldCoordinate = room.GetWorldCoordinate(new IntVector2(Random.Range(0, room.TileWidth), Random.Range(0, room.TileHeight)));
		if (base.pathFinder.GetDestination.room == room.abstractRoom.index && !base.pathFinder.GetDestination.TileDefined && base.pathFinder.GetDestination.CompareDisregardingTile(creature.pos))
		{
			creature.abstractAI.SetDestination(worldCoordinate);
		}
		else if (base.denFinder.GetDenPosition().HasValue && base.denFinder.GetDenPosition().Value.CompareDisregardingTile(base.pathFinder.GetDestination))
		{
			creature.abstractAI.SetDestination(new WorldCoordinate(room.abstractRoom.index, Random.Range(0, room.TileWidth), Random.Range(0, room.TileHeight), -1));
		}
		base.NewRoom(room);
		if (PathfindingNodes != null && PathfindingNodes.Count == 0)
		{
			(creature.abstractAI as SkyWhaleAbstractAI).NodePath.BuildPath(creature.pos, fromEntrance: true);
			PlacedObject.SkyWhalePathfindingData node = FindClosestNode();
			(creature.abstractAI as SkyWhaleAbstractAI).NodePath.MoveToNodeOnPath(node);
			previousNodePosition = room.MiddleOfTile(creature.pos.Tile);
		}
		timeToTire = Random.Range(100, 1400);
		tryingToLeaveRoom = false;
	}

	public Behavior DetermineBehavior()
	{
		Behavior result = Behavior.Idle;
		base.utilityComparer.GetUtilityTracker(base.rainTracker).weight = ((skyWhale.room.roomRain == null || skyWhale.room.roomRain.dangerType == RoomRain.DangerType.None) ? 0f : 1f);
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			result = ((aIModule is RainTracker) ? Behavior.EscapeRain : ((!(aIModule is StuckTracker)) ? Behavior.Idle : Behavior.GetUnstuck));
		}
		if (currentUtility < 0.05f)
		{
			currentUtility = 0.05f;
			List<PlacedObject.SkyWhalePathfindingData> pathfindingNodes = PathfindingNodes;
			result = ((pathfindingNodes == null || pathfindingNodes.Count <= 0) ? Behavior.Idle : Behavior.FollowPath);
		}
		return result;
	}

	public override void Update()
	{
		base.Update();
		if (skyWhale.room != null)
		{
			if (ModManager.MSC && skyWhale.LickedByPlayer != null)
			{
				base.tracker.SeeCreature(skyWhale.LickedByPlayer.abstractCreature);
			}
			currentUtility = base.utilityComparer.HighestUtility();
			behavior = DetermineBehavior();
			base.stuckTracker.satisfiedWithThisPosition = false;
			Act();
		}
	}

	public void Act()
	{
		if (behavior == Behavior.Idle)
		{
			SkyWhaleNodePath nodePath = (creature.abstractAI as SkyWhaleAbstractAI).NodePath;
			if (nodePath == null || nodePath.usedPathfindingNodes.Count <= 0)
			{
				List<PlacedObject.SkyWhalePathfindingData> pathfindingNodes = PathfindingNodes;
				if (pathfindingNodes == null || pathfindingNodes.Count <= 0)
				{
					goto IL_005e;
				}
			}
			tryingToLeaveRoom = true;
			goto IL_005e;
		}
		if (behavior == Behavior.EscapeRain)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
		}
		else if (behavior == Behavior.GetUnstuck)
		{
			creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
		}
		else
		{
			if (!(behavior == Behavior.FollowPath))
			{
				return;
			}
			if (CurrentNode == null || (PathfindingNodes != null && !PathfindingNodes.Contains(CurrentNode)))
			{
				(creature.abstractAI as SkyWhaleAbstractAI).NodePath.BuildPath(creature.pos, fromEntrance: true);
				PlacedObject.SkyWhalePathfindingData node = FindClosestNode();
				(creature.abstractAI as SkyWhaleAbstractAI).NodePath.MoveToNodeOnPath(node);
				previousNodePosition = currentNodePosition;
			}
			if (CurrentNode != null)
			{
				if (testNodeIndex >= PathfindingNodes.Count)
				{
					testNodeIndex = 0;
				}
				Vector2 vector = CurrentNode.ClosestPoint(skyWhale.firstChunk.pos);
				Vector2 vector2 = PathfindingNodes[testNodeIndex].ClosestPoint(skyWhale.firstChunk.pos);
				float num = Vector2.Distance(skyWhale.firstChunk.pos, vector);
				float num2 = Vector2.Distance(skyWhale.firstChunk.pos, vector2);
				if (CurrentNode != PathfindingNodes[testNodeIndex] && num2 < num)
				{
					(creature.abstractAI as SkyWhaleAbstractAI).NodePath.MoveToNodeOnPath(PathfindingNodes[testNodeIndex]);
					previousNodePosition = currentNodePosition;
					num = num2;
					vector = vector2;
				}
				testNodeIndex++;
				currentNodePosition = vector;
				if (Custom.ManhattanDistance(skyWhale.room.GetWorldCoordinate(vector), base.pathFinder.GetDestination) > 1)
				{
					creature.abstractAI.SetDestination(skyWhale.room.GetWorldCoordinate(vector));
				}
				Vector2 lhs = Custom.DirVec(previousNodePosition, currentNodePosition);
				if (num < 100f || Vector2.Dot(lhs, skyWhale.firstChunk.pos - currentNodePosition) > -50f)
				{
					(creature.abstractAI as SkyWhaleAbstractAI).NodePath.MoveToNextNode();
					previousNodePosition = currentNodePosition;
				}
			}
			if (PathfindingNodes.Count == 0)
			{
				(creature.abstractAI as SkyWhaleAbstractAI).NodePath.EndOfPath(skyWhale.WhiskerCheck());
				previousNodePosition = currentNodePosition;
				if (PathfindingNodes.Count == 0)
				{
					tryingToLeaveRoom = true;
				}
			}
		}
		return;
		IL_005e:
		WorldCoordinate coord = new WorldCoordinate(skyWhale.room.abstractRoom.index, Random.Range(0, skyWhale.room.TileWidth), Random.Range(0, skyWhale.room.TileHeight), -1);
		if (IdleScore(coord) < IdleScore(tempIdlePos))
		{
			tempIdlePos = coord;
		}
		if (tryingToLeaveRoom && base.denFinder.GetDenPosition().HasValue)
		{
			creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
		}
		else if (timeInRoom > timeToTire)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				if (idlePosCounter <= 0)
				{
					timeInRoom = 0;
					tryingToLeaveRoom = false;
				}
				else if (!tryingToLeaveRoom)
				{
					creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
					idlePosCounter = Random.Range(500, 1000);
					timeToTire = Random.Range(400, 1000);
					timeInRoom = Mathf.Max(timeToTire + 1, timeInRoom);
					tryingToLeaveRoom = true;
				}
			}
			else
			{
				timeInRoom = 0;
				tryingToLeaveRoom = false;
			}
		}
		else if (IdleScore(tempIdlePos) < IdleScore(base.pathFinder.GetDestination) + Custom.LerpMap(idlePosCounter, 0f, 300f, 100f, -300f))
		{
			SetDestination(tempIdlePos);
			idlePosCounter = Random.Range(200, 500);
			tempIdlePos = new WorldCoordinate(skyWhale.room.abstractRoom.index, Random.Range(0, skyWhale.room.TileWidth), Random.Range(0, skyWhale.room.TileHeight), -1);
		}
		idlePosCounter--;
	}

	public PlacedObject.SkyWhalePathfindingData FindClosestNode()
	{
		return (creature.abstractAI as SkyWhaleAbstractAI).NodePath?.FindClosestNodeToPoint(skyWhale.firstChunk.pos, PathfindingNodes);
	}

	public float IdleScore(WorldCoordinate coord)
	{
		if (coord.room != creature.pos.room || !base.pathFinder.CoordinateReachableAndGetbackable(coord))
		{
			return float.MaxValue;
		}
		float num = 1f;
		num = ((!skyWhale.AltVariant) ? (num + (float)(Mathf.Min(skyWhale.room.aimap.getTerrainProximity(coord), 10) - 10) * 40f) : (num + (float)(Mathf.Max(10 - skyWhale.room.aimap.getTerrainProximity(coord), 0) * 50)));
		if (skyWhale.AltVariant && !skyWhale.room.RayTraceTilesForTerrain(coord.x, coord.y, coord.x, skyWhale.room.TileHeight - 1))
		{
			num += 5000f;
		}
		if (base.pathFinder.CoordinateReachableAndGetbackable(coord + new IntVector2(0, -1)))
		{
			num += 10f;
		}
		return num;
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed);
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return base.rainTracker.Utility() > 0.01f;
	}

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
	{
		if (skyWhale.graphicsModule != null)
		{
			(skyWhale.graphicsModule as SkyWhaleGraphics).creatureLooker.ReevaluateLookObject(creatureRep, 2f);
		}
	}

	public float CreatureUnease(AbstractCreature crit)
	{
		if (crit.state.dead || skyWhale.Template.CreatureRelationship(crit.creatureTemplate).type == CreatureTemplate.Relationship.Type.Ignores)
		{
			return 0f;
		}
		float num = 1f;
		num *= crit.creatureTemplate.bodySize;
		num *= Mathf.Lerp(skyWhale.abstractCreature.creatureTemplate.CreatureRelationship(crit.creatureTemplate).intensity, 1f, 0.5f);
		if (crit.realizedCreature != null && !crit.realizedCreature.Consious)
		{
			num /= 20f;
		}
		if (skyWhale.room.GetTile(skyWhale.mainBodyChunk.pos).DeepWater && crit.creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.AirOnly)
		{
			num /= 3f;
		}
		else if (!skyWhale.room.GetTile(skyWhale.mainBodyChunk.pos).AnyWater && crit.creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.WaterOnly)
		{
			num /= 3f;
		}
		return num;
	}
}
