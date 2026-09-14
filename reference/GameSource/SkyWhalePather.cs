using RWCustom;
using UnityEngine;

public class SkyWhalePather : BorderExitPather
{
	public int highestGeneration = -2;

	public SkyWhalePather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
	}

	public override void Update()
	{
		base.Update();
		if (creatureFollowingGeneration > highestGeneration)
		{
			highestGeneration = creatureFollowingGeneration;
		}
		if (checkNextList.Count == 0 && creatureFollowingGeneration < highestGeneration && timeInGeneration > 80)
		{
			AssignNewDestination(destination);
		}
		reAssignDestinationOnceAccessibilityMappingIsDone = false;
	}

	public override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		PathCost result = base.CheckConnectionCost(start, goal, connection, followingPath);
		if (connection.destinationCoord.TileDefined && destination.TileDefined && Custom.ManhattanDistance(connection.destinationCoord, destination) > 6)
		{
			result.resistance += Mathf.Clamp(10f - (float)realizedRoom.aimap.getTerrainProximity(connection.destinationCoord), 0f, 10f) * 10f;
		}
		if (connection.destinationCoord.TileDefined && realizedRoom.aimap.getTerrainProximity(connection.destinationCoord) < 4 && result.legality > PathCost.Legality.IllegalConnection)
		{
			result += new PathCost(0f, PathCost.Legality.IllegalConnection);
		}
		if (connection.type == MovementConnection.MovementType.SideHighway && (!(creature.abstractAI as SkyWhaleAbstractAI).CanUseSideExit(connection.destinationCoord) || !(creature.abstractAI as SkyWhaleAbstractAI).CanUseSideExit(connection.startCoord)))
		{
			result += new PathCost(0f, PathCost.Legality.IllegalConnection);
		}
		return result;
	}

	public override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
			if (base.lookingForImpossiblePath && !cell.reachable)
			{
				return new PathCost(costToGoal.resistance * 2f, costToGoal.legality);
			}
			return new PathCost(costToGoal.resistance + Vector2.Distance(cell.worldCoordinate.Tile.ToVector2(), base.creaturePos.Tile.ToVector2()), costToGoal.legality);
		}
		return costToGoal;
	}

	public MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
	{
		int num = int.MinValue;
		PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
		WorldCoordinate dest = originPos;
		if (!originPos.TileDefined && !originPos.NodeDefined)
		{
			return default(MovementConnection);
		}
		WorldCoordinate worldCoordinate = new WorldCoordinate(originPos.room, originPos.x, originPos.y, originPos.abstractNode);
		if (originPos.TileDefined)
		{
			worldCoordinate.Tile = Custom.RestrictInRect(worldCoordinate.Tile, coveredArea);
			if (worldCoordinate.TileDefined && (worldCoordinate.x == 0 || worldCoordinate.x == realizedRoom.TileWidth - 1))
			{
				int num2 = -1;
				int num3 = int.MaxValue;
				for (int i = 0; i < realizedRoom.borderExits.Length; i++)
				{
					if (!(realizedRoom.borderExits[i].type == AbstractRoomNode.Type.SideExit))
					{
						continue;
					}
					for (int j = 0; j < realizedRoom.borderExits[i].borderTiles.Length; j++)
					{
						if (Custom.ManhattanDistance(realizedRoom.borderExits[i].borderTiles[j], worldCoordinate.Tile) < num3)
						{
							num2 = i;
							num3 = Custom.ManhattanDistance(realizedRoom.borderExits[i].borderTiles[j], worldCoordinate.Tile);
							if (num3 < 1)
							{
								break;
							}
						}
					}
				}
				if (num2 > -1)
				{
					int num4 = -1;
					for (int k = 0; k < realizedRoom.borderExits[num2].borderTiles.Length; k++)
					{
						if (realizedRoom.aimap.TileAccessibleToCreature(realizedRoom.borderExits[num2].borderTiles[k], base.creatureType) && PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k])).generation >= num4 && !PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k])).inCheckNextList)
						{
							worldCoordinate = realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k]);
							num4 = PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k])).generation;
						}
					}
				}
			}
		}
		if (actuallyFollowingThisPath && debugDrawer != null)
		{
			debugDrawer.Blink(worldCoordinate);
		}
		PathingCell pathingCell = PathingCellAtWorldCoordinate(worldCoordinate);
		if (pathingCell != null)
		{
			if (!pathingCell.reachable || !pathingCell.possibleToGetBackFrom)
			{
				OutOfElement(worldCoordinate);
			}
			MovementConnection movementConnection = default(MovementConnection);
			PathCost pathCost2 = new PathCost(0f, PathCost.Legality.Unallowed);
			int num5 = -acceptablePathAge;
			PathCost.Legality legality = PathCost.Legality.Unallowed;
			int num6 = -acceptablePathAge;
			float num7 = float.MaxValue;
			int num8 = 0;
			while (true)
			{
				MovementConnection movementConnection2 = ConnectionAtCoordinate(outGoing: true, worldCoordinate, num8);
				num8++;
				if (movementConnection2 == default(MovementConnection))
				{
					break;
				}
				if (movementConnection2.destinationCoord.TileDefined && !Custom.InsideRect(movementConnection2.DestTile, coveredArea))
				{
					continue;
				}
				PathingCell pathingCell2 = PathingCellAtWorldCoordinate(movementConnection2.destinationCoord);
				PathCost pathCost3 = CheckConnectionCost(pathingCell, pathingCell2, movementConnection2, followingPath: true);
				if (!pathingCell2.possibleToGetBackFrom && !walkPastPointOfNoReturn)
				{
					pathCost3.legality = PathCost.Legality.Unallowed;
				}
				PathCost pathCost4 = pathingCell2.costToGoal + pathCost3;
				if (movementConnection2.destinationCoord.TileDefined && destination.TileDefined && movementConnection2.destinationCoord.Tile == destination.Tile)
				{
					pathCost4.resistance = 0f;
				}
				else if (realizedRoom.IsPositionInsideBoundries(creature.pos.Tile) && !actuallyFollowingThisPath)
				{
					pathCost3 += new PathCost(100f, PathCost.Legality.Unwanted);
				}
				if (movementConnection2.type == MovementConnection.MovementType.OutsideRoom && !(AI as SkyWhaleAI).AllowMovementBetweenRooms)
				{
					pathCost3 += new PathCost(0f, PathCost.Legality.Unallowed);
				}
				if (pathingCell2.generation > num6)
				{
					num6 = pathingCell2.generation;
					num7 = pathCost4.resistance;
				}
				else if (pathingCell2.generation == num6 && pathCost4.resistance < num7)
				{
					num7 = pathCost4.resistance;
				}
				if (pathCost3.legality < legality)
				{
					movementConnection = movementConnection2;
					legality = pathCost3.legality;
					num5 = pathingCell2.generation;
					pathCost2 = pathCost4;
				}
				else if (pathCost3.legality == legality)
				{
					if (pathingCell2.generation > num5)
					{
						movementConnection = movementConnection2;
						legality = pathCost3.legality;
						num5 = pathingCell2.generation;
						pathCost2 = pathCost4;
					}
					else if (pathingCell2.generation == num5 && pathCost4 <= pathCost2)
					{
						movementConnection = movementConnection2;
						legality = pathCost3.legality;
						num5 = pathingCell2.generation;
						pathCost2 = pathCost4;
					}
				}
			}
			if (creature.realizedCreature.abstractCreature.world.game.devToolsActive && Input.GetKey("u") && actuallyFollowingThisPath)
			{
				Custom.Log($"{worldCoordinate}, chosen move:{movementConnection}");
			}
			if (legality <= PathCost.Legality.Unwanted)
			{
				if (actuallyFollowingThisPath)
				{
					creatureFollowingGeneration = num5;
					if (movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.ShortCut && realizedRoom.shortcutData(movementConnection.StartTile).shortCutType == ShortcutData.Type.RoomExit)
					{
						LeavingRoom();
					}
				}
				if (actuallyFollowingThisPath && movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.OutsideRoom && !movementConnection.destinationCoord.TileDefined && creature.realizedCreature.shortcutDelay < 1)
				{
					int num9 = 60;
					if (!Custom.InsideRect(originPos.Tile, new IntRect(-num9, -num9, realizedRoom.TileWidth + num9, realizedRoom.TileHeight + num9)))
					{
						WorldCoordinate[] sideAccessNodes = world.sideAccessNodes;
						for (int l = 0; l < sideAccessNodes.Length; l++)
						{
							WorldCoordinate worldCoordinate2 = sideAccessNodes[l];
							PathingCell pathingCell3 = PathingCellAtWorldCoordinate(worldCoordinate2);
							if (pathingCell3.generation > num)
							{
								num = pathingCell3.generation;
								pathCost = pathingCell3.costToGoal;
								dest = worldCoordinate2;
							}
							else if (pathingCell3.generation == num && pathingCell3.costToGoal < pathCost)
							{
								pathCost = pathingCell3.costToGoal;
								dest = worldCoordinate2;
							}
							if (worldCoordinate2.CompareDisregardingTile(destination))
							{
								dest = worldCoordinate2;
								break;
							}
						}
						if (!dest.CompareDisregardingTile(movementConnection.destinationCoord))
						{
							(creature.realizedCreature as SkyWhale).AccessSideGate(movementConnection.destinationCoord, dest);
							if (dest.room != base.creaturePos.room)
							{
								LeavingRoom();
							}
						}
						return default(MovementConnection);
					}
					IntVector2 intVector = new IntVector2(0, 1);
					if (movementConnection.startCoord.x == 0)
					{
						intVector = new IntVector2(-1, 0);
					}
					else if (movementConnection.startCoord.x == realizedRoom.TileWidth - 1)
					{
						intVector = new IntVector2(1, 0);
					}
					else if (movementConnection.startCoord.y == 0)
					{
						intVector = new IntVector2(0, -1);
					}
					return new MovementConnection(MovementConnection.MovementType.Standard, originPos, new WorldCoordinate(originPos.room, originPos.x + intVector.x * 10, originPos.y + intVector.y * 10, originPos.abstractNode), 1);
				}
				return movementConnection;
			}
		}
		return default(MovementConnection);
	}
}
