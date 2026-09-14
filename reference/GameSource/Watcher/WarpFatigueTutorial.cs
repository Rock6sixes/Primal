namespace Watcher;

public class WarpFatigueTutorial : UpdatableAndDeletable
{
	public int message;

	public WarpFatigueTutorial(Room room)
	{
		base.room = room;
		if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
		{
			Destroy();
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!room.game.IsStorySession || !Watcher.cfgWatcherTutorials.Value)
		{
			Destroy();
		}
		else if (room.game.GetStorySession.saveState.miscWorldSaveData.warpFatigueTutorialCounter > 1)
		{
			Destroy();
		}
		else if (room.game.session.Players[0].realizedCreature != null && room.game.cameras[0].hud != null && room.game.cameras[0].hud.textPrompt != null && room.game.cameras[0].hud.textPrompt.messages.Count < 1)
		{
			switch (message)
			{
			case 0:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Crossing to new frontiers takes a toll on the body, but sleep can help you recover"), 120, 160, darken: false, hideHud: true);
				message++;
				break;
			case 1:
				room.game.GetStorySession.saveState.miscWorldSaveData.warpFatigueTutorialCounter = room.game.GetStorySession.saveState.miscWorldSaveData.warpFatigueTutorialCounter + 1;
				Destroy();
				break;
			}
		}
	}
}
