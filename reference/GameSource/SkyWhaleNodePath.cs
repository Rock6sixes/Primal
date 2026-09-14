using System.Collections.Generic;
using System.Linq;
using DevInterface;
using RWCustom;
using UnityEngine;

public class SkyWhaleNodePath
{
	public class SkyWhalePathingVis : MapPage.CreatureVis.CreatureSpecificVis
	{
		public List<FSprite> nodeSprites = new List<FSprite>();

		public int offset;

		public FSprite dirSprite;

		public FSprite destDirSprite;

		public SkyWhaleAI AI => (crit.realizedCreature as SkyWhale)?.AI;

		public SkyWhaleNodePath NodePath => (crit.abstractAI as SkyWhaleAbstractAI).NodePath;

		public List<PlacedObject.SkyWhalePathfindingData> AINodes => NodePath?.usedPathfindingNodes.Concat(NodePath.PathfindingNodes).ToList();

		public SkyWhalePathingVis(MapPage myMapPage, AbstractCreature crit)
			: base(myMapPage, crit)
		{
			base.myMapPage = myMapPage;
			base.crit = crit;
			List<int> list = new List<int>();
			foreach (MapPage.CreatureVis creatureVisualization in myMapPage.creatureVisualizations)
			{
				if (creatureVisualization.crit.Room == crit.Room && creatureVisualization.crit != crit && creatureVisualization.creatureSpecificVis is SkyWhalePathingVis skyWhalePathingVis)
				{
					list.Add(skyWhalePathingVis.offset);
				}
			}
			offset = 0;
			int num = 1;
			while (list.Contains(offset))
			{
				offset = num / 2;
				if (num % 2 == 0)
				{
					offset = -offset;
				}
				num++;
			}
		}

		public override void Update()
		{
			if (AINodes == null)
			{
				RemoveNodeSprites();
				return;
			}
			if (NodePath != null && AINodes.Count != nodeSprites.Count)
			{
				RemoveNodeSprites();
				nodeSprites = new List<FSprite>();
				for (int i = 0; i < AINodes.Count; i++)
				{
					nodeSprites.Add(new FSprite("pixel")
					{
						anchorY = 0f
					});
					Futile.stage.AddChild(nodeSprites[nodeSprites.Count - 1]);
				}
			}
			for (int j = 0; j < nodeSprites.Count; j++)
			{
				nodeSprites[j].color = ((j < NodePath.usedPathfindingNodes.Count) ? Color.blue : Color.red);
				IntVector2 intVector = new IntVector2((int)(AINodes[j].owner.pos.x / 20f), (int)(AINodes[j].owner.pos.y / 20f));
				Vector2 vector = myMapPage.CreatureVisPos(new WorldCoordinate(crit.pos.room, intVector.x, intVector.y, crit.pos.abstractNode), inDen: false, stack: true);
				nodeSprites[j].rotation = Custom.VecToDeg(AINodes[j].handlePos);
				nodeSprites[j].scaleY = AINodes[j].handlePos.magnitude / 10f;
				Vector2 v = Custom.PerpendicularVector(AINodes[j].handlePos);
				v = Custom.DegToVec(Mathf.Round(Custom.VecToDeg(v) / 45f) * 45f);
				v = new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
				v *= (float)offset;
				nodeSprites[j].SetPosition(vector + v);
			}
			if (crit.realizedCreature is SkyWhale skyWhale && crit.pos.TileDefined)
			{
				if (dirSprite == null || destDirSprite == null)
				{
					RemoveDirSprites();
					dirSprite = new FSprite("pixel")
					{
						anchorY = 0f,
						color = Color.yellow,
						scaleY = 20f
					};
					destDirSprite = new FSprite("pixel")
					{
						anchorY = 0f,
						color = Color.green,
						scaleY = 20f
					};
					Futile.stage.AddChild(dirSprite);
					Futile.stage.AddChild(destDirSprite);
				}
				Vector2 position = myMapPage.CreatureVisPos(crit.pos, inDen: false, stack: true);
				dirSprite.SetPosition(position);
				dirSprite.rotation = Custom.VecToDeg(skyWhale.flyDir);
				destDirSprite.SetPosition(position);
				destDirSprite.rotation = Custom.VecToDeg(skyWhale.destFlyDir);
				bool flag = AI != null && AI.pathFinder.GetDestination != AI.pathFinder.GetEffectualDestination && AI.behavior == SkyWhaleAI.Behavior.FollowPath && AI.CurrentNode != null;
				destDirSprite.color = (flag ? Color.cyan : Color.green);
			}
			else if (dirSprite != null || destDirSprite != null)
			{
				RemoveDirSprites();
			}
		}

		public override void Destroy()
		{
			RemoveNodeSprites();
			RemoveDirSprites();
		}

		public void RemoveNodeSprites()
		{
			foreach (FSprite nodeSprite in nodeSprites)
			{
				nodeSprite.RemoveFromContainer();
			}
			nodeSprites.Clear();
		}

		public void RemoveDirSprites()
		{
			dirSprite?.RemoveFromContainer();
			destDirSprite?.RemoveFromContainer();
			dirSprite = null;
			destDirSprite = null;
		}
	}

	public SkyWhaleAbstractAI ai;

	public int room = -1;

	public bool destroy;

	public List<PlacedObject.SkyWhalePathfindingData> PathfindingNodes = new List<PlacedObject.SkyWhalePathfindingData>();

	public List<PlacedObject.SkyWhalePathfindingData> usedPathfindingNodes = new List<PlacedObject.SkyWhalePathfindingData>();

	public AbstractCreature Parent => ai.parent;

	public PlacedObject.SkyWhalePathfindingData CurrentNode
	{
		get
		{
			if (PathfindingNodes.Count <= 0)
			{
				return null;
			}
			return PathfindingNodes[0];
		}
	}

	public SkyWhaleNodePath(SkyWhaleAbstractAI ai, int room)
	{
		this.ai = ai;
		this.room = room;
	}

	public void MoveToNextNode()
	{
		if (PathfindingNodes.Count > 0)
		{
			usedPathfindingNodes.Add(PathfindingNodes[0]);
			PathfindingNodes.RemoveAt(0);
		}
	}

	public void MoveToNodeOnPath(PlacedObject.SkyWhalePathfindingData node)
	{
		if (node == null)
		{
			destroy = true;
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			while (PathfindingNodes.Count > 0 && PathfindingNodes[0] != node)
			{
				MoveToNextNode();
			}
			if (i == 0)
			{
				EndOfPath(forceReset: true);
			}
		}
	}

	public void EndOfPath(bool forceReset)
	{
		if (forceReset || (double)Random.value < 0.4)
		{
			WorldCoordinate pos = Parent.pos;
			if (usedPathfindingNodes.Count > 0)
			{
				pos.Tile = Room.StaticGetTilePosition(usedPathfindingNodes[usedPathfindingNodes.Count - 1].owner.pos);
			}
			BuildPath(pos, fromEntrance: false);
		}
	}

	public void BuildPath(WorldCoordinate startPos, bool fromEntrance)
	{
		Room realizedRoom = Parent.world.GetAbstractRoom(Parent.pos).realizedRoom;
		if (realizedRoom == null)
		{
			return;
		}
		Vector2 vector;
		if (fromEntrance && startPos.NodeDefined)
		{
			vector = realizedRoom.MiddleOfTile(realizedRoom.LocalCoordinateOfNode(startPos.abstractNode).Tile);
		}
		else
		{
			if (!startPos.TileDefined)
			{
				return;
			}
			vector = realizedRoom.MiddleOfTile(startPos.Tile);
		}
		List<PlacedObject.SkyWhalePathfindingData> list = new List<PlacedObject.SkyWhalePathfindingData>();
		if (room == Parent.pos.room && usedPathfindingNodes.Count > 0)
		{
			list = usedPathfindingNodes;
			if (PathfindingNodes.Count > 0)
			{
				list.AddRange(PathfindingNodes);
			}
		}
		else
		{
			for (int i = 0; i < realizedRoom.roomSettings.placedObjects.Count; i++)
			{
				if (realizedRoom.roomSettings.placedObjects[i].data is PlacedObject.SkyWhalePathfindingData item)
				{
					list.Add(item);
				}
			}
		}
		PathfindingNodes = new List<PlacedObject.SkyWhalePathfindingData>();
		usedPathfindingNodes = new List<PlacedObject.SkyWhalePathfindingData>();
		while (list.Count > 0)
		{
			PlacedObject.SkyWhalePathfindingData skyWhalePathfindingData = FindClosestNodeToPoint(vector, list);
			list.Remove(skyWhalePathfindingData);
			PathfindingNodes.Add(skyWhalePathfindingData);
			vector = Custom.ClosestPointOnLineSegment(skyWhalePathfindingData.owner.pos, skyWhalePathfindingData.owner.pos + (skyWhalePathfindingData.owner.data as PlacedObject.SkyWhalePathfindingData).handlePos, vector);
		}
	}

	public PlacedObject.SkyWhalePathfindingData FindClosestNodeToPoint(Vector2 point, List<PlacedObject.SkyWhalePathfindingData> pathfindingNodes)
	{
		if (pathfindingNodes.Count == 0)
		{
			return null;
		}
		int index = 0;
		float num = Vector2.Distance(point, pathfindingNodes[0].owner.pos);
		for (int i = 1; i < pathfindingNodes.Count; i++)
		{
			float num2 = Vector2.Distance(point, pathfindingNodes[i].owner.pos);
			if (num2 < num)
			{
				index = i;
				num = num2;
			}
		}
		return pathfindingNodes[index];
	}
}
