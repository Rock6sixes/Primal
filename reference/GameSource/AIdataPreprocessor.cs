using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

public class AIdataPreprocessor
{
	public class SubRoutine
	{
		public AImap aiMap;

		public Visualizer visualizer;

		public bool done;

		public SubRoutine(AImap aiMap, Visualizer visualizer)
		{
			this.aiMap = aiMap;
			this.visualizer = visualizer;
		}

		public virtual void Update()
		{
		}

		public void Finish()
		{
			done = true;
		}
	}

	public class DijkstraMapper : SubRoutine
	{
		public class Cell
		{
			public IntVector2 pos;

			public Cell parent;

			public int generation;

			public PathCost cost;

			public int x => pos.x;

			public int y => pos.y;

			public Cell(IntVector2 pos, Cell parent, int generation, PathCost cost)
			{
				this.pos = pos;
				this.parent = parent;
				this.generation = generation;
				this.cost = cost;
			}
		}

		public MinHeap<PathCost, (IntVector2 parent, IntVector2 tile)> checkNext;

		public CreatureTemplate crit;

		public Cell[,] cellGrid;

		public DijkstraMapper(AImap aiMap, CreatureTemplate crit, Visualizer visualizer)
			: base(aiMap, visualizer)
		{
			this.crit = crit;
			checkNext = new MinHeap<PathCost, (IntVector2, IntVector2)>();
			visualizer?.InitWithCrit(crit);
		}

		public virtual void AddCell(IntVector2 pos, Cell parent, PathCost cost, int generation)
		{
			cellGrid[pos.x, pos.y] = new Cell(pos, parent, generation, cost);
			if (visualizer != null)
			{
				visualizer.ColorCell(pos, 1, cellGrid);
			}
		}
	}

	public class NodeMapper : DijkstraMapper
	{
		public int currentNodeForCreature;

		public ConnectivityMapper connectivityMapper;

		public int currentNodeCommon => CreatureSpecificToCommonNodeIndex(aiMap.room, currentNodeForCreature, crit);

		public NodeMapper(AImap aiMap, CreatureTemplate crit, int currentNodeForCreature, ConnectivityMapper connectivityMapper, Visualizer visualizer, bool dontBake)
			: base(aiMap, crit, visualizer)
		{
			this.currentNodeForCreature = currentNodeForCreature;
			this.connectivityMapper = connectivityMapper;
			IntVector2[] array;
			if (currentNodeCommon >= aiMap.room.exitAndDenIndex.Length)
			{
				array = ((currentNodeCommon >= aiMap.room.exitAndDenIndex.Length + aiMap.room.borderExits.Length) ? aiMap.room.hives[currentNodeCommon - aiMap.room.exitAndDenIndex.Length - aiMap.room.borderExits.Length] : aiMap.room.borderExits[currentNodeCommon - aiMap.room.exitAndDenIndex.Length].borderTiles);
			}
			else
			{
				array = new IntVector2[1] { aiMap.room.ShortcutLeadingToNode(currentNodeCommon).StartTile };
				if (visualizer == null)
				{
				}
			}
			cellGrid = new Cell[aiMap.width, aiMap.height];
			for (int i = 0; i < array.Length; i++)
			{
				if (aiMap.TileAccessibleToCreature(array[i], crit))
				{
					AddCell(array[i], null, new PathCost(0f, PathCost.Legality.Allowed), 1);
				}
			}
			if (dontBake)
			{
				checkNext.Clear();
				Finish();
			}
		}

		public override void Update()
		{
			if (checkNext.Count > 0)
			{
				KeyValuePair<PathCost, (IntVector2, IntVector2)> keyValuePair = checkNext.TakeMin();
				PathCost key = keyValuePair.Key;
				var (intVector, pos) = keyValuePair.Value;
				if (cellGrid[pos.x, pos.y] == null)
				{
					Cell cell = cellGrid[intVector.x, intVector.y];
					int generation = ((cell == null) ? 1 : (cell.generation + 1));
					AddCell(pos, cell, key, generation);
				}
			}
			else
			{
				Finish();
			}
		}

		public void WriteDijkstraMap(CreatureSpecificAImap m, int n)
		{
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					if (cellGrid[i, j] != null)
					{
						m.SetDistanceToExit(i, j, n, cellGrid[i, j].generation);
					}
				}
			}
		}

		public override void AddCell(IntVector2 pos, Cell parent, PathCost cost, int generation)
		{
			base.AddCell(pos, parent, cost, generation);
			foreach (MovementConnection incomingPath in aiMap.getAItile(pos).incomingPaths)
			{
				if (aiMap.IsConnectionAllowedForCreature(incomingPath, crit) && cellGrid[incomingPath.startCoord.x, incomingPath.startCoord.y] == null)
				{
					checkNext.Add(cost + aiMap.ConnectionCostForCreature(incomingPath, crit), (pos, incomingPath.StartTile));
				}
			}
			connectivityMapper.TileDiscovered(currentNodeCommon, pos, crit, cost, generation);
		}
	}

	public class AccessibilityDijkstraMapper : DijkstraMapper
	{
		public int gen;

		public AccessibilityDijkstraMapper(AImap aiMap, CreatureTemplate crit, Visualizer visualizer, bool dontBake)
			: base(aiMap, crit, visualizer)
		{
			cellGrid = new Cell[aiMap.width, aiMap.height];
			IntVector2 otherVector = new IntVector2(aiMap.room.TileWidth / 2, aiMap.room.TileHeight / 2);
			float num = new IntVector2(0, 0).FloatDist(otherVector);
			for (int i = 0; i < aiMap.room.TileWidth; i++)
			{
				for (int j = 0; j < aiMap.room.TileHeight; j++)
				{
					if (aiMap.AnyExitReachableFromTile(new IntVector2(i, j), crit))
					{
						float resistance = new IntVector2(i, j).FloatDist(otherVector) / num;
						AddCell(cost: new PathCost(resistance, PathCost.Legality.Allowed), pos: new IntVector2(i, j), parent: null, generation: 0);
					}
				}
			}
			if (dontBake)
			{
				checkNext.Clear();
				Finish();
			}
		}

		public override void Update()
		{
			if (checkNext.Count > 0)
			{
				KeyValuePair<PathCost, (IntVector2, IntVector2)> keyValuePair = checkNext.TakeMin();
				PathCost key = keyValuePair.Key;
				IntVector2 item = keyValuePair.Value.Item1;
				IntVector2 item2 = keyValuePair.Value.Item2;
				if (cellGrid[item2.x, item2.y] == null)
				{
					gen++;
					AddCell(item2, cellGrid[item.x, item.y], key, gen);
				}
			}
			else
			{
				Finish();
			}
		}

		public void WriteAccessibilityMap(CreatureSpecificAImap m)
		{
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					if (cellGrid[i, j] != null)
					{
						m.SetAccessibility(i, j, 1f - (float)cellGrid[i, j].generation / (float)gen);
					}
				}
			}
		}

		public override void AddCell(IntVector2 pos, Cell parent, PathCost cost, int generation)
		{
			base.AddCell(pos, parent, cost, generation);
			foreach (MovementConnection outgoingPath in aiMap.getAItile(pos).outgoingPaths)
			{
				if (cellGrid[outgoingPath.destinationCoord.x, outgoingPath.destinationCoord.y] == null)
				{
					checkNext.Add(cost + aiMap.ConnectionCostForCreature(outgoingPath, crit), (pos, outgoingPath.DestTile));
				}
			}
			if (visualizer != null)
			{
				visualizer.ColorCell(pos, 1, cellGrid);
			}
		}
	}

	public class VisibilityMapper : SubRoutine
	{
		public int x;

		public int y;

		public int w;

		public int h;

		public int[,] grid;

		public bool[] solidGrid;

		public int horizontalMargin;

		public int topMargin;

		public int bottomMargin;

		public VisibilityMapper(AImap aiMap, Visualizer visualizer, bool dontBake)
			: base(aiMap, visualizer)
		{
			Room room = aiMap.room;
			w = room.TileWidth;
			h = room.TileHeight;
			grid = new int[w, h];
			solidGrid = new bool[w * h];
			for (int i = 0; i < room.TileWidth; i++)
			{
				for (int j = 0; j < room.TileHeight; j++)
				{
					solidGrid[i + j * w] = room.HasAnySolid(i, j);
				}
			}
			if (room.borderExits.Length != 0)
			{
				horizontalMargin = 10;
				topMargin = 20;
				bottomMargin = 5;
			}
			visualizer?.InitNoCrit();
			if (dontBake)
			{
				Finish();
			}
		}

		public override void Update()
		{
			if (done)
			{
				return;
			}
			if (!solidGrid[x + y * w])
			{
				VisitTile();
			}
			x++;
			if (x >= aiMap.room.TileWidth)
			{
				x = 0;
				y++;
				if (y >= aiMap.room.TileHeight)
				{
					Finish();
				}
			}
		}

		public void VisitTile()
		{
			int num = 0;
			for (int i = Math.Max(x - 50, -horizontalMargin); i < Math.Min(x + 50, aiMap.room.TileWidth + horizontalMargin); i++)
			{
				for (int j = Math.Max(y - 50, -bottomMargin); j < Math.Min(y + 50, aiMap.room.TileHeight + topMargin); j++)
				{
					if (RayTraceTilesForTerrain(x, y, i, j))
					{
						num++;
					}
				}
			}
			grid[x, y] = num;
			if (visualizer != null)
			{
				visualizer.ColorCell(new IntVector2(x, y), grid);
			}
		}

		public bool RayTraceTilesForTerrain(int x0, int y0, int x1, int y1)
		{
			int num = Math.Abs(x1 - x0);
			int num2 = Math.Abs(y1 - y0);
			int num3 = x0;
			int num4 = y0;
			int num5 = w;
			int num6 = h;
			int num7 = Math.Min(Math.Max(num3, 0), num5 - 1);
			int num8 = Math.Min(Math.Max(num4, 0), num6 - 1);
			int num9 = 1 + num + num2;
			int num10 = ((x1 > x0) ? 1 : (-1));
			int num11 = ((y1 > y0) ? 1 : (-1));
			int num12 = num - num2;
			num *= 2;
			num2 *= 2;
			while (num9 > 0)
			{
				if (solidGrid[num7 + num8 * num5])
				{
					return false;
				}
				if (num12 > 0)
				{
					num3 += num10;
					num7 = Math.Min(Math.Max(num3, 0), num5 - 1);
					num12 -= num2;
				}
				else
				{
					num4 += num11;
					num8 = Math.Min(Math.Max(num4, 0), num6 - 1);
					num12 += num;
				}
				num9--;
			}
			return true;
		}

		public void WriteVisibilityMap()
		{
			for (int i = 0; i < aiMap.room.TileWidth; i++)
			{
				for (int j = 0; j < aiMap.room.TileHeight; j++)
				{
					aiMap.getAItile(i, j).visibility = grid[i, j];
				}
			}
		}
	}

	public class ConnectivityMapper
	{
		public int[,,,] connectivityMap;

		public List<ShortcutData> shortCutsInNodeOrder;

		public int roomNodes;

		public Room room;

		public int exitNodes;

		public int denNodes;

		public ConnectivityMapper(int roomNodes, Room room)
		{
			this.roomNodes = roomNodes;
			this.room = room;
			connectivityMap = new int[StaticWorld.preBakedPathingCreatures.Length, roomNodes, roomNodes, 2];
			for (int i = 0; i < StaticWorld.preBakedPathingCreatures.Length; i++)
			{
				for (int j = 0; j < roomNodes; j++)
				{
					for (int k = 0; k < roomNodes; k++)
					{
						for (int l = 0; l < 2; l++)
						{
							connectivityMap[i, j, k, l] = -1;
						}
					}
				}
			}
			List<ShortcutData> list = new List<ShortcutData>();
			for (int m = 0; m < room.shortcuts.Length; m++)
			{
				if (room.shortcuts[m].shortCutType == ShortcutData.Type.RoomExit)
				{
					list.Add(room.shortcuts[m]);
					exitNodes++;
				}
				else if (room.shortcuts[m].shortCutType == ShortcutData.Type.CreatureHole)
				{
					list.Add(room.shortcuts[m]);
					denNodes++;
				}
				else if (room.shortcuts[m].shortCutType == ShortcutData.Type.RegionTransportation)
				{
					list.Add(room.shortcuts[m]);
				}
			}
			IEnumerable<ShortcutData> source = list.OrderBy((ShortcutData pet) => pet.destNode);
			shortCutsInNodeOrder = source.ToList();
			_ = shortCutsInNodeOrder.Count;
			_ = room.exitAndDenIndex.Length;
			for (int n = 0; n < shortCutsInNodeOrder.Count; n++)
			{
				_ = shortCutsInNodeOrder[n].destNode;
			}
		}

		public void TileDiscovered(int b, IntVector2 tile, CreatureTemplate crit, PathCost cost, int generation)
		{
			for (int i = 0; i < shortCutsInNodeOrder.Count; i++)
			{
				if (shortCutsInNodeOrder[i].startCoord.x == tile.x && shortCutsInNodeOrder[i].startCoord.y == tile.y)
				{
					if (i != b && cost.Allowed && ((crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Exit] && shortCutsInNodeOrder[i].shortCutType == ShortcutData.Type.RoomExit) || (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Den] && shortCutsInNodeOrder[i].shortCutType == ShortcutData.Type.CreatureHole) || (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.RegionTransportation] && shortCutsInNodeOrder[i].shortCutType == ShortcutData.Type.RegionTransportation)))
					{
						connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), i, b, 0] = (int)cost.resistance;
						connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), i, b, 1] = generation;
					}
					break;
				}
			}
			if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.SeaExit] || crit.mappedNodeTypes[(int)AbstractRoomNode.Type.SkyExit] || crit.mappedNodeTypes[(int)AbstractRoomNode.Type.SideExit])
			{
				for (int j = 0; j < room.borderExits.Length; j++)
				{
					if (room.borderExits[j].type.Index == -1 || !crit.mappedNodeTypes[room.borderExits[j].type.Index])
					{
						continue;
					}
					int num = shortCutsInNodeOrder.Count + j;
					if (connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num, b, 1] != -1)
					{
						continue;
					}
					for (int k = 0; k < room.borderExits[j].borderTiles.Length; k++)
					{
						if (tile == room.borderExits[j].borderTiles[k] && num != b && cost.Allowed)
						{
							connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num, b, 0] = (int)cost.resistance;
							connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num, b, 1] = generation;
							break;
						}
					}
				}
			}
			if (!crit.mappedNodeTypes[(int)AbstractRoomNode.Type.BatHive])
			{
				return;
			}
			for (int l = 0; l < room.hives.Length; l++)
			{
				int num2 = shortCutsInNodeOrder.Count + room.borderExits.Length + l;
				if (connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num2, b, 1] != -1)
				{
					continue;
				}
				for (int m = 0; m < room.hives[l].Length; m++)
				{
					if (tile == room.hives[l][m] && num2 != b && cost.Allowed)
					{
						connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num2, b, 0] = (int)cost.resistance;
						connectivityMap[StaticWorld.preBakedPathingCreatures.IndexOf(crit), num2, b, 1] = generation;
						break;
					}
				}
			}
		}

		public AbstractRoomNode[] ReturnNodes()
		{
			AbstractRoomNode[] array = new AbstractRoomNode[roomNodes];
			for (int i = 0; i < roomNodes; i++)
			{
				AbstractRoomNode.Type exit = AbstractRoomNode.Type.Exit;
				exit = ((i < exitNodes) ? AbstractRoomNode.Type.Exit : ((i < exitNodes + denNodes) ? AbstractRoomNode.Type.Den : ((i < shortCutsInNodeOrder.Count) ? AbstractRoomNode.Type.RegionTransportation : ((i < shortCutsInNodeOrder.Count + room.borderExits.Length) ? room.borderExits[i - shortCutsInNodeOrder.Count].type : ((i >= shortCutsInNodeOrder.Count + room.borderExits.Length + room.hives.GetLength(0)) ? AbstractRoomNode.Type.GarbageHoles : AbstractRoomNode.Type.BatHive)))));
				int shortCutLength = -1;
				int viewedByCamera = -1;
				bool submerged = false;
				int entranceWidth = 1;
				if (exit == AbstractRoomNode.Type.Exit || exit == AbstractRoomNode.Type.Den)
				{
					shortCutLength = shortCutsInNodeOrder[i].length;
					viewedByCamera = room.CameraViewingNode(i);
					submerged = room.GetTile(shortCutsInNodeOrder[i].StartTile).AnyWater;
				}
				else if (exit == AbstractRoomNode.Type.SideExit || exit == AbstractRoomNode.Type.SkyExit || exit == AbstractRoomNode.Type.SeaExit)
				{
					submerged = room.borderExits[i - room.exitAndDenIndex.Length].type == AbstractRoomNode.Type.SeaExit;
					entranceWidth = room.borderExits[i - room.exitAndDenIndex.Length].borderTiles.Length;
				}
				array[i] = new AbstractRoomNode(exit, shortCutLength, roomNodes, submerged, viewedByCamera, entranceWidth);
				for (int j = 0; j < roomNodes; j++)
				{
					for (int k = 0; k < StaticWorld.preBakedPathingCreatures.Length; k++)
					{
						for (int l = 0; l < 2; l++)
						{
							array[i].connectivity[k, j, l] = connectivityMap[k, i, j, l];
						}
					}
				}
			}
			return array;
		}
	}

	public class Visualizer
	{
		public AImap aiMap;

		public DebugSprite[,] sprites;

		public int highest;

		public Visualizer(AImap aiMap)
		{
			this.aiMap = aiMap;
			sprites = new DebugSprite[aiMap.width, aiMap.height];
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					FSprite sp = new FSprite("pixel")
					{
						scale = 22f,
						color = new Color(0.5f, 0.5f, 0.5f),
						alpha = 0f
					};
					sprites[i, j] = new DebugSprite(aiMap.room.MiddleOfTile(i, j), sp, aiMap.room);
					aiMap.room.AddObject(sprites[i, j]);
				}
			}
			highest = 0;
		}

		public void ColorCell(IntVector2 pos, int col, DijkstraMapper.Cell[,] cellGrid)
		{
			switch (col)
			{
			case 0:
				sprites[pos.x, pos.y].sprite.color = new Color(1f, 1f, 0f);
				break;
			case 1:
				sprites[pos.x, pos.y].sprite.color = GenerationColor(cellGrid[pos.x, pos.y].generation);
				if (cellGrid[pos.x, pos.y].generation >= highest)
				{
					UpdateAllWithNewHighest(cellGrid, cellGrid[pos.x, pos.y].generation);
				}
				break;
			case 2:
				sprites[pos.x, pos.y].sprite.color = new Color(1f, 0f, 1f);
				break;
			}
		}

		public void UpdateAllWithNewHighest(DijkstraMapper.Cell[,] cellGrid, int newHighest)
		{
			highest = newHighest;
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					if (cellGrid[i, j] != null)
					{
						sprites[i, j].sprite.color = GenerationColor(cellGrid[i, j].generation);
					}
				}
			}
		}

		public void ColorCell(IntVector2 pos, int[,] intGrid)
		{
			sprites[pos.x, pos.y].sprite.color = GenerationColor(intGrid[pos.x, pos.y]);
			if (intGrid[pos.x, pos.y] >= highest)
			{
				UpdateAllWithNewHighest(intGrid, intGrid[pos.x, pos.y]);
			}
		}

		public void ColorCell(IntVector2 pos, Color col)
		{
			sprites[pos.x, pos.y].sprite.color = col;
		}

		public void UpdateAllWithNewHighest(int[,] intGrid, int newHighest)
		{
			highest = newHighest;
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					sprites[i, j].sprite.color = GenerationColor(intGrid[i, j]);
				}
			}
		}

		public Color GenerationColor(int i)
		{
			return Custom.HSL2RGB((float)i / (float)highest * 0.7f, 1f, (i == 0) ? 0.2f : 0.5f);
		}

		public void InitWithCrit(CreatureTemplate crit)
		{
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					sprites[i, j].sprite.alpha = 0.75f;
					if (aiMap.TileAccessibleToCreature(new IntVector2(i, j), crit))
					{
						sprites[i, j].sprite.color = new Color(0.5f, 0.5f, 0.5f);
					}
					else
					{
						sprites[i, j].sprite.color = new Color(0f, 0f, 0f);
					}
				}
			}
			highest = 0;
		}

		public void InitNoCrit()
		{
			for (int i = 0; i < aiMap.width; i++)
			{
				for (int j = 0; j < aiMap.height; j++)
				{
					sprites[i, j].sprite.alpha = 0.75f;
				}
			}
			highest = 0;
		}
	}

	public static AGLog<AIdataPreprocessor> Log = new AGLog<AIdataPreprocessor>();

	public AImap aiMap;

	public SubRoutine currentSubRoutine;

	public int currentCreatureIndex;

	public int currentNode;

	public int roomNodes;

	public ConnectivityMapper connectivityMapper;

	public bool done;

	public bool viz;

	public Visualizer visualizer;

	public bool falseBake;

	public AIdataPreprocessor(AImap aiMap, bool falseBake)
	{
		this.aiMap = aiMap;
		this.falseBake = falseBake;
		if (viz)
		{
			visualizer = new Visualizer(aiMap);
		}
		aiMap.creatureSpecificAImaps = new CreatureSpecificAImap[StaticWorld.preBakedPathingCreatures.Length];
		roomNodes = TotalNodes(aiMap.room);
		for (int i = 0; i < StaticWorld.preBakedPathingCreatures.Length; i++)
		{
			aiMap.creatureSpecificAImaps[i] = new CreatureSpecificAImap(aiMap, StaticWorld.preBakedPathingCreatures[i]);
		}
		connectivityMapper = new ConnectivityMapper(roomNodes, aiMap.room);
		currentNode = -1;
		currentCreatureIndex = -1;
		currentSubRoutine = new VisibilityMapper(aiMap, visualizer, falseBake);
	}

	public void Update()
	{
		if (done)
		{
			return;
		}
		currentSubRoutine.Update();
		if (!currentSubRoutine.done)
		{
			return;
		}
		if (currentCreatureIndex == -1)
		{
			NodeDone();
			NextCreature();
			return;
		}
		NodeDone();
		currentNode++;
		if (currentNode < NodesRelevantToCreature(aiMap.room, StaticWorld.preBakedPathingCreatures[currentCreatureIndex]))
		{
			NextNode();
			return;
		}
		if (currentNode == NodesRelevantToCreature(aiMap.room, StaticWorld.preBakedPathingCreatures[currentCreatureIndex]))
		{
			MapAccessibility();
			return;
		}
		CreatureDone();
		NextCreature();
	}

	public void CreatureDone()
	{
		_ = viz;
		if (currentCreatureIndex >= StaticWorld.preBakedPathingCreatures.Length)
		{
			done = true;
		}
	}

	public void NextCreature()
	{
		currentCreatureIndex++;
		while (currentCreatureIndex < StaticWorld.preBakedPathingCreatures.Length && NodesRelevantToCreature(aiMap.room, StaticWorld.preBakedPathingCreatures[currentCreatureIndex]) == 0)
		{
			_ = viz;
			currentCreatureIndex++;
		}
		if (currentCreatureIndex >= StaticWorld.preBakedPathingCreatures.Length)
		{
			Finish();
			return;
		}
		_ = viz;
		if (viz)
		{
			visualizer.InitWithCrit(StaticWorld.preBakedPathingCreatures[currentCreatureIndex]);
		}
		currentNode = 0;
		NextNode();
	}

	public void NodeDone()
	{
		_ = viz;
		if (currentSubRoutine is NodeMapper)
		{
			(currentSubRoutine as NodeMapper).WriteDijkstraMap(aiMap.creatureSpecificAImaps[currentCreatureIndex], currentNode);
		}
		else if (currentSubRoutine is AccessibilityDijkstraMapper)
		{
			(currentSubRoutine as AccessibilityDijkstraMapper).WriteAccessibilityMap(aiMap.creatureSpecificAImaps[currentCreatureIndex]);
		}
		else if (currentSubRoutine is VisibilityMapper)
		{
			(currentSubRoutine as VisibilityMapper).WriteVisibilityMap();
		}
	}

	public void NextNode()
	{
		_ = viz;
		currentSubRoutine = new NodeMapper(aiMap, StaticWorld.preBakedPathingCreatures[currentCreatureIndex], currentNode, connectivityMapper, visualizer, falseBake);
		if (viz)
		{
			visualizer.InitWithCrit(StaticWorld.preBakedPathingCreatures[currentCreatureIndex]);
		}
	}

	public void MapAccessibility()
	{
		_ = viz;
		currentSubRoutine = new AccessibilityDijkstraMapper(aiMap, StaticWorld.preBakedPathingCreatures[currentCreatureIndex], visualizer, falseBake);
		if (viz)
		{
			visualizer.InitWithCrit(StaticWorld.preBakedPathingCreatures[currentCreatureIndex]);
		}
	}

	public void Finish()
	{
		done = true;
	}

	public AbstractRoomNode[] Connectivity()
	{
		return connectivityMapper.ReturnNodes();
	}

	public static int TotalNodes(Room room)
	{
		return room.exitAndDenIndex.Length + room.hives.Length + room.borderExits.Length + ((room.garbageHoles != null) ? 1 : 0);
	}

	public static int NodesRelevantToCreature(Room room, CreatureTemplate crit)
	{
		int num = 0;
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Exit])
		{
			for (int i = 0; i < room.exitAndDenIndex.Length && room.ShortcutLeadingToNode(i).shortCutType == ShortcutData.Type.RoomExit; i++)
			{
				num++;
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Den])
		{
			for (int j = 0; j < room.exitAndDenIndex.Length; j++)
			{
				if (room.ShortcutLeadingToNode(j).shortCutType == ShortcutData.Type.CreatureHole)
				{
					num++;
				}
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.RegionTransportation])
		{
			for (int k = 0; k < room.exitAndDenIndex.Length; k++)
			{
				if (room.ShortcutLeadingToNode(k).shortCutType == ShortcutData.Type.RegionTransportation)
				{
					num++;
				}
			}
		}
		for (int l = 0; l < room.borderExits.Length; l++)
		{
			if (room.borderExits[l].type.Index != -1 && crit.mappedNodeTypes[room.borderExits[l].type.Index])
			{
				num++;
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.BatHive])
		{
			num += room.hives.Length;
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.GarbageHoles] && room.garbageHoles != null)
		{
			num++;
		}
		return num;
	}

	public static int CreatureSpecificToCommonNodeIndex(Room room, int n, CreatureTemplate crit)
	{
		int num = 0;
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Exit])
		{
			for (int i = 0; i < room.exitAndDenIndex.Length; i++)
			{
				if (room.ShortcutLeadingToNode(i).shortCutType == ShortcutData.Type.RoomExit)
				{
					if (n == num)
					{
						return i;
					}
					num++;
				}
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.Den])
		{
			for (int j = 0; j < room.exitAndDenIndex.Length; j++)
			{
				if (room.ShortcutLeadingToNode(j).shortCutType == ShortcutData.Type.CreatureHole)
				{
					if (n == num)
					{
						return j;
					}
					num++;
				}
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.RegionTransportation])
		{
			for (int k = 0; k < room.exitAndDenIndex.Length; k++)
			{
				if (room.ShortcutLeadingToNode(k).shortCutType == ShortcutData.Type.RegionTransportation)
				{
					if (n == num)
					{
						return k;
					}
					num++;
				}
			}
		}
		for (int l = 0; l < room.borderExits.Length; l++)
		{
			if (room.borderExits[l].type.Index != -1 && crit.mappedNodeTypes[room.borderExits[l].type.Index])
			{
				if (n == num)
				{
					return room.exitAndDenIndex.Length + l;
				}
				num++;
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.BatHive])
		{
			for (int m = 0; m < room.hives.Length; m++)
			{
				if (n == num)
				{
					return room.exitAndDenIndex.Length + room.borderExits.Length + m;
				}
				num++;
			}
		}
		if (crit.mappedNodeTypes[(int)AbstractRoomNode.Type.GarbageHoles] && room.garbageHoles != null && n == num)
		{
			return room.exitAndDenIndex.Length + room.borderExits.Length + room.hives.Length;
		}
		return -1;
	}
}
