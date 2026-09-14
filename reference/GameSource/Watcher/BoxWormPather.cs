using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BoxWormPather : StandardPather
{
	public List<WorldCoordinate> badTiles;

	public WorldCoordinate overTheGapCoordinate;

	public static IntVector2[] columnOffsets = new IntVector2[9]
	{
		new IntVector2(0, 0),
		new IntVector2(0, 1),
		new IntVector2(0, -1),
		new IntVector2(0, 2),
		new IntVector2(0, -2),
		new IntVector2(0, 3),
		new IntVector2(0, -3),
		new IntVector2(0, 4),
		new IntVector2(0, -4)
	};

	public BoxWormPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
		walkPastPointOfNoReturn = true;
		badTiles = new List<WorldCoordinate>();
	}

	public new MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
	{
		MovementConnection movementConnection = base.FollowPath((overTheGapCoordinate == default(WorldCoordinate)) ? originPos : overTheGapCoordinate, actuallyFollowingThisPath);
		MoveToLongGap(originPos, movementConnection);
		FindLongGap(movementConnection.startCoord, movementConnection);
		return movementConnection;
	}

	public void FindLongGap(WorldCoordinate originPos, MovementConnection connection)
	{
		if (!base.lookingForImpossiblePath || overTheGapCoordinate != default(WorldCoordinate) || AITileAtWorldCoordinate(connection.destinationCoord).outgoingPaths.Any((MovementConnection x) => x.type == MovementConnection.MovementType.ReachOverGap))
		{
			return;
		}
		PathingCellAtWorldCoordinate(originPos);
		if (PathingCellAtWorldCoordinate(originPos).costToGoal.legality < PathCost.Legality.IllegalConnection || connection.type == MovementConnection.MovementType.ReachOverGap || realizedRoom.GetTile(connection.DestTile + Custom.fourDirections[1]).Terrain != 0 || realizedRoom.GetTile(connection.DestTile + Custom.fourDirections[1] * 2).IsSolid())
		{
			return;
		}
		realizedRoom.GetTile(connection.StartTile + Custom.fourDirections[1]);
		if (realizedRoom.GetTile(connection.StartTile + Custom.fourDirections[1]).IsSolid() && !badTiles.Any((WorldCoordinate x) => x == originPos))
		{
			WorldCoordinate worldCoordinate = ScanColumn(originPos, 4, destination.x - originPos.x);
			if (worldCoordinate != default(WorldCoordinate))
			{
				overTheGapCoordinate = worldCoordinate;
			}
			else
			{
				badTiles.Add(originPos);
			}
		}
	}

	public void MoveToLongGap(WorldCoordinate originPos, MovementConnection connection)
	{
		if (!(overTheGapCoordinate == default(WorldCoordinate)) && (Custom.ManhattanDistance(originPos, overTheGapCoordinate) > 12 || Custom.ManhattanDistance(originPos.Tile, overTheGapCoordinate.Tile) < 2))
		{
			overTheGapCoordinate = default(WorldCoordinate);
		}
	}

	public WorldCoordinate ScanColumn(WorldCoordinate pos, int distance, int direction = 1)
	{
		if (direction == 0)
		{
			return default(WorldCoordinate);
		}
		direction = Mathf.Clamp(direction, -1, 1);
		PathingCell pathingCell = PathingCellAtWorldCoordinate(pos);
		for (int i = 0; i < 4; i++)
		{
			IntVector2 intVector = Custom.fourDirections[direction + 1] * (distance + i);
			WorldCoordinate worldCoordinate = default(WorldCoordinate);
			for (int j = 0; j < columnOffsets.Length - i * 2; j++)
			{
				WorldCoordinate worldCoordinate2 = pos + columnOffsets[j] + intVector;
				PathingCell pathingCell2 = PathingCellAtWorldCoordinate(worldCoordinate2);
				if (isAirOnSolid(worldCoordinate2) && !(pathingCell2.costToGoal.resistance > pathingCell.costToGoal.resistance) && (pathingCell2.costToGoal.resistance < PathingCellAtWorldCoordinate(worldCoordinate).costToGoal.resistance || worldCoordinate == default(WorldCoordinate)))
				{
					worldCoordinate = worldCoordinate2;
				}
			}
			if (worldCoordinate != default(WorldCoordinate))
			{
				return worldCoordinate;
			}
		}
		return default(WorldCoordinate);
	}

	public bool isAirOnSolid(WorldCoordinate pos)
	{
		if (realizedRoom.GetTile(pos).Terrain == Room.Tile.TerrainType.Air)
		{
			if (realizedRoom.GetTile(pos + Custom.fourDirections[1]).Terrain != Room.Tile.TerrainType.Solid)
			{
				return realizedRoom.GetTile(pos + Custom.fourDirections[1]).Terrain == Room.Tile.TerrainType.Floor;
			}
			return true;
		}
		return false;
	}
}
