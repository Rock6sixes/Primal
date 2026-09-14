using System.Threading;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RoomPositionPanel : Panel
{
	public string roomName;

	public WorldLoader worldLoader;

	public RoomPanel roomPanel;

	public bool tileClicked;

	public int tileClickedX;

	public int tileClickedY;

	public RoomPositionPanel(DevUI owner, DevUINode parentNode, Vector2 pos, SlugcatStats.Timeline timeline, string roomName)
		: base(owner, "Room_Position_Panel", parentNode, pos, new Vector2(100f, 20f), "Select position")
	{
		SlugcatStats.Name playerCharacter = SlugcatStats.Name.White;
		if (owner.game.IsStorySession)
		{
			playerCharacter = owner.game.GetStorySession.saveStateNumber;
		}
		this.roomName = roomName;
		worldLoader = new WorldLoader(owner.game, playerCharacter, timeline, singleRoomWorld: true, roomName, null, Custom.rainWorld.setup);
	}

	public override void Update()
	{
		base.Update();
		if (worldLoader != null)
		{
			if (worldLoader.Finished)
			{
				World world = worldLoader.ReturnWorld();
				MapObject.RoomRepresentation roomRep = new MapObject.RoomRepresentation(world, world.GetAbstractRoom(roomName), forceRenderMode: true);
				roomPanel = new RoomPanel(owner, world, this, pos, roomRep);
				roomPanel.disableDragging = true;
				subNodes.Add(roomPanel);
				roomPanel.Refresh();
				roomPanel.Move(new Vector2(roomPanel.size.x * 0.75f, roomPanel.size.y * 0.75f));
				roomPanel.Refresh();
				size = new Vector2(roomPanel.size.x * 1.5f, roomPanel.size.y * 1.5f);
				Refresh();
				worldLoader = null;
			}
			else
			{
				worldLoader.NextActivity();
				while (!worldLoader.Finished)
				{
					worldLoader.Update();
					Thread.Sleep(1);
				}
			}
		}
		else if (roomPanel.miniMap.MouseOver && roomPanel.miniMap.owner.mouseClick)
		{
			float num = roomPanel.miniMap.owner.mousePos.x - roomPanel.miniMap.absPos.x;
			float num2 = roomPanel.miniMap.owner.mousePos.y - roomPanel.miniMap.absPos.y;
			float num3 = num / roomPanel.miniMap.size.x;
			float num4 = num2 / roomPanel.miniMap.size.y;
			tileClickedX = (int)(num3 * (float)roomPanel.miniMap.roomRep.room.size.x);
			tileClickedY = (int)(num4 * (float)roomPanel.miniMap.roomRep.room.size.y);
			tileClicked = true;
		}
	}
}
