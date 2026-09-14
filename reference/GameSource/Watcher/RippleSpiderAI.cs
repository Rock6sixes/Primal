using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleSpiderAI : ArtificialIntelligence
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior Travelling = new Behavior("Travelling", register: true);

		public static Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class RippleSpiderPather : StandardPather
	{
		public static HashSet<(string, string)> bannedConnections = new HashSet<(string, string)>
		{
			("WORA_CITY7", "WORA_CITY14X"),
			("WORA_INTERSTICE", "WORA_CITY14X"),
			("WBLA_D01", "WBLA_C01"),
			("WBLA_D01", "WBLA_B01"),
			("WBLA_D01", "WBLA_B03"),
			("WPTA_F01", "WPTA_B07"),
			("WVWA_F01", "WVWA_B07")
		};

		public RippleSpiderPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
			: base(AI, world, creature)
		{
		}

		public override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
		{
			PathCost result = base.CheckConnectionCost(start, goal, connection, followingPath);
			if (connection.destinationCoord.room != connection.startCoord.room)
			{
				string item = creature.world.GetAbstractRoom(connection.startCoord.room)?.name;
				string item2 = creature.world.GetAbstractRoom(connection.destinationCoord.room)?.name;
				if (bannedConnections.Contains((item, item2)))
				{
					result += new PathCost(0f, PathCost.Legality.Unallowed);
				}
			}
			return result;
		}
	}

	public RippleSpider realizedCreature;

	public float runSpeed;

	public bool reachedTarget;

	public Behavior behavior;

	public DebugDestinationVisualizer debugDestinationVisualizer;

	public WorldCoordinate idleDestination;

	public float TravelUtility
	{
		get
		{
			if (!creature.abstractAI.WantToMigrate)
			{
				return 0f;
			}
			if (!base.pathFinder.CoordinateReachable(creature.abstractAI.MigrationDestination))
			{
				return 0f;
			}
			float num = realizedCreature.room.abstractRoom.AttractionValueForCreature(realizedCreature.Template.type);
			float num2 = realizedCreature.room.world.GetAbstractRoom(creature.abstractAI.MigrationDestination).AttractionValueForCreature(realizedCreature.Template.type);
			float num3 = Mathf.Lerp(0.99f, 0.1f, Mathf.Pow(num, 0.25f));
			if (num > num2)
			{
				return num3 *= 0.25f;
			}
			if (num == num2)
			{
				return num3 *= 0.5f;
			}
			return num3;
		}
	}

	public bool WaitForPlayer
	{
		get
		{
			return (creature.abstractAI as RippleSpiderAbstractAI).waitForPlayer;
		}
		set
		{
			(creature.abstractAI as RippleSpiderAbstractAI).waitForPlayer = value;
		}
	}

	public RippleSpiderAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		realizedCreature = creature.realizedCreature as RippleSpider;
		realizedCreature.AI = this;
		AddModule(new RippleSpiderPather(this, world, creature)
		{
			heuristicCostFac = 1f,
			timeUntilPathCertain = 10,
			accessibilityStepsPerFrame = 200,
			stepsPerFrame = 40
		});
		AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10));
		AddModule(new StuckTracker(this, trackPastPositions: true, trackNotFollowingCurrentGoal: true));
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		behavior = Behavior.Idle;
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
	}

	public Behavior DetermineBehavior()
	{
		float num = base.stuckTracker.Utility();
		float travelUtility = TravelUtility;
		float b = 0.05f;
		float num2 = Mathf.Max(Mathf.Max(num, travelUtility), b);
		if (num == num2)
		{
			return Behavior.GetUnstuck;
		}
		if (travelUtility == num2)
		{
			return Behavior.Travelling;
		}
		return Behavior.Idle;
	}

	public override void Update()
	{
		base.Update();
		if (realizedCreature.room != null)
		{
			if (debugDestinationVisualizer != null)
			{
				debugDestinationVisualizer.Update();
			}
			base.pathFinder.walkPastPointOfNoReturn = stranded;
			behavior = DetermineBehavior();
			Act();
		}
	}

	public void Act()
	{
		creature.abstractAI.AbstractBehavior(1);
		if (WaitForPlayer)
		{
			foreach (AbstractCreature player in creature.world.game.Players)
			{
				if (creature.realizedCreature != null && player.realizedCreature != null && player.realizedCreature.room == creature.realizedCreature.room && player.realizedCreature.room.VisualContact(player.realizedCreature.VisionPoint, creature.realizedCreature.VisionPoint))
				{
					Vector2 visionPoint = creature.realizedCreature.VisionPoint;
					Vector2 visionPoint2 = player.realizedCreature.VisionPoint;
					if (Custom.DistLess(visionPoint, visionPoint2, 300f))
					{
						WaitForPlayer = false;
					}
				}
			}
		}
		if ((creature.abstractAI.MigrationDestination == default(WorldCoordinate) || creature.pos.CompareDisregardingTile(creature.abstractAI.MigrationDestination)) && !WaitForPlayer)
		{
			if (base.pathFinder.DoneMappingAccessibility && (idleDestination == default(WorldCoordinate) || idleDestination.room != creature.pos.room || !base.pathFinder.CoordinateReachable(idleDestination)))
			{
				GetNewIdleDestination();
			}
			SetDestination(idleDestination);
			if (Custom.DistLess(realizedCreature.mainBodyChunk.pos, realizedCreature.room.MiddleOfTile(idleDestination), 10f))
			{
				reachedTarget = true;
			}
		}
	}

	public void GetNewIdleDestination()
	{
		float num = float.NegativeInfinity;
		IntVector2 intVector = default(IntVector2);
		for (int i = 0; i < 100; i++)
		{
			IntVector2 intVector2 = realizedCreature.room.RandomTile();
			float num2 = IdleScore(intVector2);
			if (num2 > num)
			{
				num = num2;
				intVector = intVector2;
			}
		}
		if (intVector != default(IntVector2))
		{
			idleDestination = new WorldCoordinate(creature.pos.room, intVector.x, intVector.y, -1);
		}
	}

	public float IdleScore(IntVector2 tile)
	{
		WorldCoordinate coord = new WorldCoordinate(creature.pos.room, tile.x, tile.y, -1);
		if (!base.pathFinder.CoordinateReachable(coord))
		{
			return float.NegativeInfinity;
		}
		float num = 0f;
		num += (float)coord.y / (float)realizedCreature.room.TileHeight;
		if (!realizedCreature.room.HasAnySolid(coord.x, coord.y - 1))
		{
			num -= 1f;
		}
		return num - (float)realizedCreature.room.aimap.getTerrainProximity(tile) / 2f;
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		return cost;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return false;
	}
}
