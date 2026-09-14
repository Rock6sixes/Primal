using RWCustom;
using UnityEngine;

namespace DevInterface;

public class RoomPanel : Panel
{
	public Vector2 devPos;

	public int layer = 1;

	public MiniMap miniMap;

	public World world;

	public MapPage mapPage;

	public bool disableDragging;

	public bool lastMouseOver;

	public MapObject.RoomRepresentation roomRep;

	public bool CanonView
	{
		get
		{
			if (mapPage == null)
			{
				return true;
			}
			return mapPage.canonView;
		}
	}

	public Vector2 PanPos
	{
		get
		{
			if (mapPage == null)
			{
				return Vector2.zero;
			}
			return mapPage.panPos;
		}
	}

	public override Vector2 absPos
	{
		get
		{
			if (CanonView)
			{
				return base.absPos + PanPos;
			}
			return devPos + PanPos;
		}
		set
		{
			if (CanonView)
			{
				base.absPos = value - PanPos;
			}
			else
			{
				devPos = value - PanPos;
			}
		}
	}

	public bool Visible
	{
		get
		{
			if (mapPage != null && mapPage.canonView)
			{
				return mapPage.visibleLayers[layer];
			}
			return true;
		}
	}

	public bool Clickable
	{
		get
		{
			if (mapPage != null)
			{
				if (Visible)
				{
					return !mapPage.rippleStreamEdit;
				}
				return false;
			}
			return true;
		}
	}

	public override Vector2 nonCollapsedAbsPos
	{
		get
		{
			if (CanonView)
			{
				return base.nonCollapsedAbsPos + PanPos;
			}
			return devPos + PanPos;
		}
	}

	public override void Move(Vector2 newPos)
	{
		if (CanonView)
		{
			base.Move(newPos - PanPos);
			return;
		}
		devPos = newPos - PanPos;
		Refresh();
	}

	public RoomPanel(DevUI owner, World world, DevUINode parentNode, Vector2 pos, MapObject.RoomRepresentation roomRep)
		: base(owner, roomRep.room.name + "_Panel", parentNode, pos, new Vector2(100f, 10f), roomRep.room.name)
	{
		this.world = world;
		this.roomRep = roomRep;
		mapPage = null;
		if (parentNode is MapPage)
		{
			mapPage = parentNode as MapPage;
		}
		miniMap = new MiniMap(owner, world, mapPage, this, new Vector2(5f, 5f), new Vector2(1f, 1f), roomRep);
		subNodes.Add(miniMap);
		fLabels.Add(new FLabel(Custom.GetFont(), "F:0"));
		if (owner != null)
		{
			Futile.stage.AddChild(fLabels[fLabels.Count - 1]);
		}
	}

	public override void Update()
	{
		base.Update();
		if (base.MouseOver != lastMouseOver)
		{
			if (base.MouseOver)
			{
				string text = "n:" + roomRep.room.index;
				if (roomRep.room.swarmRoom)
				{
					text += ", Swarm";
					if (!world.singleRoomWorld && !world.regionState.SwarmRoomActive(roomRep.room.swarmRoomIndex))
					{
						text += "-DEPLT!";
					}
				}
				base.Title = text;
			}
			else
			{
				base.Title = roomRep.room.name;
			}
		}
		if (!dragged && !disableDragging && miniMap.MouseOver && owner != null && owner.mouseClick && Clickable)
		{
			dragged = true;
			moveOffset = (CanonView ? nonCollapsedAbsPos : (devPos + PanPos)) - owner.mousePos;
		}
		lastMouseOver = base.MouseOver;
	}

	public override void Refresh()
	{
		roomRep.CreateMapTexture(roomRep.room.realizedRoom);
		size = new Vector2(100f, 10f);
		if (roomRep.mapTex != null)
		{
			size = new Vector2(roomRep.mapTex.sourcePixelSize.x, roomRep.mapTex.sourcePixelSize.y) * 2f + new Vector2(10f, 15f);
		}
		if (!Visible)
		{
			foreach (FSprite fSprite in fSprites)
			{
				fSprite.isVisible = false;
			}
			foreach (FLabel fLabel in fLabels)
			{
				fLabel.isVisible = false;
			}
			miniMap.Refresh();
			return;
		}
		foreach (FSprite fSprite2 in fSprites)
		{
			fSprite2.isVisible = true;
		}
		foreach (FLabel fLabel2 in fLabels)
		{
			fLabel2.isVisible = true;
		}
		if (CanonView)
		{
			Vector2 vector = pos;
			pos = new Vector2(-10000f, -10000f);
			MoveLabel(1, absPos);
			base.Refresh();
			pos = vector;
			(subNodes[0] as MiniMap).pos = -(subNodes[0] as MiniMap).size * 0.5f;
			miniMap.Refresh();
			return;
		}
		float num = (float)roomRep.room.nodes.Length * 18f / size.x;
		num = Mathf.Floor(num + 1f);
		size.y += num * 18f;
		(subNodes[0] as MiniMap).pos.x = 5f;
		(subNodes[0] as MiniMap).pos.y = 10f + num * 18f;
		if (fSprites.Count > 0)
		{
			if (mapPage != null && mapPage.subRegionsMode)
			{
				fSprites[0].color = MapPage.SubregionColor(roomRep.room);
			}
			else if (mapPage != null && roomRep.room.realizedRoom != null && mapPage.attractivenessPanel == null)
			{
				fSprites[0].color = ((owner != null && owner.game.cameras[0].room.abstractRoom.index == roomRep.room.index) ? new Color(1f, 0f, 0f) : new Color(0f, 0.5f, 0f));
				if (Random.value < 0.5f)
				{
					for (int i = 0; i < world.loadingRooms.Count; i++)
					{
						if (world.loadingRooms[i].room.abstractRoom.index == roomRep.room.index)
						{
							fSprites[0].color = new Color(0f, 0f, 0.5f);
							break;
						}
					}
				}
			}
			else
			{
				fSprites[0].color = new Color(0f, 0f, 0f);
			}
		}
		if (fLabels.Count > 1)
		{
			MoveLabel(1, absPos + size + new Vector2(20f, -10f));
			fLabels[1].text = "F:" + miniMap.roomRep.room.NumberOfQuantifiedCreatureInRoom(CreatureTemplate.Type.Fly);
		}
		base.Refresh();
	}
}
