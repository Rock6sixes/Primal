using RWCustom;

public class FloodFillPather
{
	public enum Status : byte
	{
		InHeap,
		Visited,
		Closed
	}

	public IntVector2 dest;

	public AImap map;

	public CreatureTemplate creatureType;

	public Status[] nodeStatus;

	public QuickPathFinder.MinHeap nodeHeap;

	public QuickPathFinder.PathNode[] nodes;

	public bool Done { get; set; }

	public FloodFillPather(IntVector2 dest, AImap map, CreatureTemplate creatureType)
	{
		dest = new IntVector2(Custom.IntClamp(dest.x, 0, map.width - 1), Custom.IntClamp(dest.y, 0, map.height - 1));
		this.dest = dest;
		this.map = map;
		this.creatureType = creatureType;
		nodeHeap = new QuickPathFinder.MinHeap(new QuickPathFinder.PathNode(dest.x, dest.y, null, new PathCost(0f, PathCost.Legality.Allowed), new PathCost(0f, PathCost.Legality.Allowed)));
		nodes = new QuickPathFinder.PathNode[map.width * map.height];
		nodeStatus = new Status[nodes.Length];
		QuickPathFinder.PathNode pathNode = new QuickPathFinder.PathNode(dest.x, dest.y, null, new PathCost(0f, PathCost.Legality.Allowed), new PathCost(0f, PathCost.Legality.Allowed));
		nodes[dest.x + dest.y * map.width] = pathNode;
		SetStatus(pathNode.Pos, Status.Closed);
		nodeHeap.Add(pathNode);
	}

	public IntVector2? GetNextPos(IntVector2 tile)
	{
		if (tile.x < 0 || tile.y < 0 || tile.x >= map.width || tile.y >= map.height)
		{
			return null;
		}
		return nodes[tile.x + tile.y * map.width]?.oneStepCloserToGoal?.Pos;
	}

	public PathCost? GetCostToDestination(IntVector2 tile)
	{
		if (tile.x < 0 || tile.y < 0 || tile.x >= map.width || tile.y >= map.height)
		{
			return null;
		}
		return nodes[tile.x + tile.y * map.width]?.stepsToGoal;
	}

	public void Update()
	{
		if (Done)
		{
			return;
		}
		if (nodeHeap.Empty())
		{
			Done = true;
			nodeHeap = null;
			nodeStatus = null;
			return;
		}
		QuickPathFinder.PathNode pathNode = nodeHeap.ExtractFirst();
		SetStatus(pathNode.Pos, Status.Closed);
		foreach (MovementConnection incomingPath in map.map[pathNode.x, pathNode.y].incomingPaths)
		{
			IntVector2 startTile = incomingPath.StartTile;
			if (startTile.x >= 0 && startTile.x < map.width && startTile.y >= 0 && startTile.y < map.height && map.IsConnectionAllowedForCreature(incomingPath, creatureType) && GetStatus(incomingPath.StartTile) != Status.Closed)
			{
				PathCost stepsToGoal = pathNode.stepsToGoal;
				stepsToGoal += creatureType.ConnectionResistance(incomingPath.type) * incomingPath.distance;
				stepsToGoal += map.TileCostForCreature(incomingPath.startCoord, creatureType);
				QuickPathFinder.PathNode pathNode2 = new QuickPathFinder.PathNode(startTile.x, startTile.y, pathNode, stepsToGoal, stepsToGoal);
				nodes[startTile.x + startTile.y * map.width] = pathNode2;
				if (GetStatus(startTile) == Status.InHeap)
				{
					nodeHeap.Add(pathNode2);
				}
				else
				{
					nodeHeap.FindAndReplace(pathNode2);
				}
				SetStatus(startTile, Status.Visited);
			}
		}
	}

	public Status GetStatus(IntVector2 tile)
	{
		if (tile.x < 0 || tile.y < 0 || tile.x >= map.width || tile.y >= map.height)
		{
			return Status.Closed;
		}
		return nodeStatus[tile.x + tile.y * map.width];
	}

	public void SetStatus(IntVector2 tile, Status status)
	{
		if (tile.x >= 0 && tile.y >= 0 && tile.x < map.width && tile.y < map.height)
		{
			nodeStatus[tile.x + tile.y * map.width] = status;
		}
	}

	public bool IsReachable(WorldCoordinate coord)
	{
		if (coord.TileDefined)
		{
			return IsReachable(coord.Tile);
		}
		return false;
	}

	public bool IsReachable(IntVector2 tile)
	{
		PathCost? costToDestination = GetCostToDestination(tile);
		if (!costToDestination.HasValue)
		{
			return false;
		}
		return costToDestination.GetValueOrDefault().legality <= PathCost.Legality.Unwanted;
	}
}
