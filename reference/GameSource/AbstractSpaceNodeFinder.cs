using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class AbstractSpaceNodeFinder
{
	public class Status : ExtEnum<Status>
	{
		public static Status Flooding = new Status("Flooding", register: true);

		public static Status Stepping = new Status("Stepping", register: true);

		public static Status Finished = new Status("Finished", register: true);

		public static Status Failed = new Status("Failed", register: true);

		public Status(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SearchingFor : ExtEnum<SearchingFor>
	{
		public static SearchingFor Den = new SearchingFor("Den", register: true);

		public static SearchingFor SwarmRoom = new SearchingFor("SwarmRoom", register: true);

		public static SearchingFor AttractiveRoom = new SearchingFor("AttractiveRoom", register: true);

		public SearchingFor(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class FloodMethod : ExtEnum<FloodMethod>
	{
		public static FloodMethod Cost = new FloodMethod("Cost", register: true);

		public static FloodMethod Random = new FloodMethod("Random", register: true);

		public FloodMethod(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Node
	{
		public WorldCoordinate pos;

		public Node parent;

		public float costToGoal;

		public Node(WorldCoordinate pos, Node parent, float costToGoal)
		{
			this.pos = pos;
			this.parent = parent;
			this.costToGoal = costToGoal;
		}
	}

	public Status status;

	public int maxGenerations;

	public int generation;

	public SearchingFor searchingFor;

	public FloodMethod floodMethod;

	public List<Node> checkNextList;

	public bool[,] alreadyChecked;

	public World world;

	public CreatureTemplate creatureType;

	public Node walker;

	public List<WorldCoordinate> path;

	public WorldCoordinate startPos;

	public AbstractCreature abstractCreature;

	public bool finished
	{
		get
		{
			if (!(status == Status.Finished))
			{
				return status == Status.Failed;
			}
			return true;
		}
	}

	public AbstractSpaceNodeFinder(SearchingFor searchingFor, FloodMethod floodMethod, int maxGenerations, WorldCoordinate pos, AbstractCreature abstractCreature, World world, float randomFac)
		: this(searchingFor, floodMethod, maxGenerations, pos, abstractCreature.creatureTemplate, world, randomFac)
	{
		this.abstractCreature = abstractCreature;
	}

	public AbstractSpaceNodeFinder(SearchingFor searchingFor, FloodMethod floodMethod, int maxGenerations, WorldCoordinate pos, CreatureTemplate creatureType, World world, float randomFac)
	{
		this.floodMethod = floodMethod;
		this.searchingFor = searchingFor;
		generation = 0;
		this.maxGenerations = maxGenerations;
		WorldCoordinate pos2 = QuickConnectivity.DefineNodeOfLocalCoordinate(pos, world, creatureType);
		status = Status.Flooding;
		checkNextList = new List<Node>
		{
			new Node(pos2, null, 0f)
		};
		alreadyChecked = new bool[world.NumberOfRooms, world.mostNodesInARoom];
		path = new List<WorldCoordinate>();
		this.world = world;
		this.creatureType = creatureType;
		if (IsNodeGoal(pos2))
		{
			status = Status.Finished;
			path.Add(pos2.WashTileData());
		}
		walker = null;
		abstractCreature = null;
	}

	public void Update()
	{
		if (checkNextList.Count == 0 && status == Status.Flooding)
		{
			status = Status.Failed;
		}
		if (status != Status.Flooding && status != Status.Stepping)
		{
			return;
		}
		if (status == Status.Flooding)
		{
			Node node = checkNextList[0];
			if (floodMethod == FloodMethod.Cost)
			{
				foreach (Node checkNext in checkNextList)
				{
					if (checkNext.costToGoal < node.costToGoal)
					{
						node = checkNext;
					}
				}
			}
			else if (floodMethod == FloodMethod.Random)
			{
				node = checkNextList[Random.Range(0, checkNextList.Count)];
			}
			int num = node.pos.room - world.firstRoomIndex;
			int abstractNode = node.pos.abstractNode;
			if (num < 0 || abstractNode < 0 || num >= alreadyChecked.GetLength(0) || abstractNode >= alreadyChecked.GetLength(1))
			{
				checkNextList.Remove(node);
				return;
			}
			alreadyChecked[num, abstractNode] = true;
			checkNextList.Remove(node);
			AbstractRoom abstractRoom = world.GetAbstractRoom(node.pos);
			if (abstractNode >= abstractRoom.nodes.Length)
			{
				return;
			}
			for (int i = 0; i < abstractRoom.nodes.Length; i++)
			{
				if (i != abstractNode)
				{
					WorldCoordinate pos = new WorldCoordinate(abstractRoom.index, -1, -1, i);
					float num2 = (float)abstractRoom.ConnectivityCost(pos.abstractNode, abstractNode, creatureType) * creatureType.ConnectionResistance(MovementConnection.MovementType.OffScreenMovement).resistance;
					if (num2 >= 0f && !alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
					{
						AddNode(pos, node, num2);
					}
				}
			}
			if (abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit && !creatureType.mappedNodeTypes[0])
			{
				int num3 = abstractNode;
				if (abstractRoom.connections[num3] > -1)
				{
					WorldCoordinate pos = new WorldCoordinate(abstractRoom.connections[num3], -1, -1, world.GetAbstractRoom(abstractRoom.connections[num3]).ExitIndex(abstractRoom.index));
					float num2 = (float)world.TotalShortCutLengthBetweenTwoConnectedRooms(abstractRoom.index, abstractRoom.connections[num3]) * creatureType.ConnectionResistance(MovementConnection.MovementType.ShortCut).resistance;
					if (num2 > 0f && GetCoordinateAttraction(pos) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num2 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num2);
						}
					}
				}
			}
			else if (abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.SideExit && !creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.SideExit])
			{
				for (int j = 0; j < world.sideAccessNodes.Length; j++)
				{
					if (world.sideAccessNodes[j].CompareDisregardingTile(node.pos))
					{
						continue;
					}
					WorldCoordinate pos = world.sideAccessNodes[j];
					float num2 = (float)world.SideHighwayDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SideHighway).resistance;
					if (num2 > 0f && GetCoordinateAttraction(pos) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num2 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num2);
						}
					}
				}
			}
			else if (abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.SkyExit && !creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.SkyExit])
			{
				for (int k = 0; k < world.skyAccessNodes.Length; k++)
				{
					if (world.skyAccessNodes[k].CompareDisregardingTile(node.pos))
					{
						continue;
					}
					WorldCoordinate pos = world.skyAccessNodes[k];
					float num2 = (float)world.SkyHighwayDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SkyHighway).resistance;
					if (num2 > 0f && GetCoordinateAttraction(pos) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num2 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num2);
						}
					}
				}
			}
			else if (abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.SeaExit && !creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.SeaExit])
			{
				for (int l = 0; l < world.seaAccessNodes.Length; l++)
				{
					if (world.seaAccessNodes[l].CompareDisregardingTile(node.pos))
					{
						continue;
					}
					WorldCoordinate pos = world.seaAccessNodes[l];
					float num2 = (float)world.SeaHighwayDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SeaHighway).resistance;
					if (num2 > 0f && GetCoordinateAttraction(pos) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num2 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num2);
						}
					}
				}
			}
			else if (abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.RegionTransportation && !creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.RegionTransportation])
			{
				for (int m = 0; m < world.regionAccessNodes.Length; m++)
				{
					if (world.regionAccessNodes[m].CompareDisregardingTile(node.pos))
					{
						continue;
					}
					WorldCoordinate pos = world.regionAccessNodes[m];
					float num2 = (float)world.RegionTransportationDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.RegionTransportation).resistance;
					if (num2 > 0f && GetCoordinateAttraction(pos) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num2 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num2);
						}
					}
				}
			}
			if (IsNodeGoal(node.pos))
			{
				walker = node;
				status = Status.Stepping;
			}
		}
		else if (status == Status.Stepping)
		{
			path.Add(walker.pos);
			if (walker.parent != null)
			{
				walker = walker.parent;
			}
			else
			{
				status = Status.Finished;
			}
		}
	}

	public AbstractRoom.CreatureRoomAttraction GetCoordinateAttraction(WorldCoordinate checkPos)
	{
		if (abstractCreature != null)
		{
			return world.GetAbstractRoom(checkPos).AttractionForCreature(abstractCreature);
		}
		return world.GetAbstractRoom(checkPos).AttractionForCreature(creatureType.type);
	}

	public bool IsNodeGoal(WorldCoordinate pos)
	{
		if (searchingFor == SearchingFor.Den)
		{
			if (pos.NodeDefined)
			{
				return world.GetNode(pos).type == AbstractRoomNode.Type.Den;
			}
			return false;
		}
		if (searchingFor == SearchingFor.SwarmRoom)
		{
			if (world.GetAbstractRoom(pos).swarmRoom && world.regionState != null)
			{
				return world.regionState.SwarmRoomActive(world.GetAbstractRoom(pos).swarmRoomIndex);
			}
			return false;
		}
		if (searchingFor == SearchingFor.AttractiveRoom)
		{
			if (abstractCreature != null)
			{
				return Custom.LerpMap(generation, 0f, maxGenerations, Mathf.Lerp(0.5f, 1.5f, Random.value), 1f) * world.GetAbstractRoom(pos).SizeDependentAttractionValueForCreature(abstractCreature) > world.GetAbstractRoom(startPos).SizeDependentAttractionValueForCreature(abstractCreature);
			}
			return Custom.LerpMap(generation, 0f, maxGenerations, Mathf.Lerp(0.5f, 1.5f, Random.value), 1f) * world.GetAbstractRoom(pos).SizeDependentAttractionValueForCreature(creatureType.type) > world.GetAbstractRoom(startPos).SizeDependentAttractionValueForCreature(creatureType.type);
		}
		return true;
	}

	public void AddNode(WorldCoordinate pos, Node parent, float cost)
	{
		Node item = new Node(pos, parent, parent.costToGoal + (float)(int)cost);
		checkNextList.Add(item);
	}

	public List<WorldCoordinate> ReturnPathToClosest()
	{
		if (status != Status.Finished)
		{
			return null;
		}
		return path;
	}
}
