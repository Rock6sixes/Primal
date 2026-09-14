namespace Watcher;

public class BadWarpTutorial : UpdatableAndDeletable
{
	public int message;

	public BadWarpTutorial(Room room)
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
		else if (room.game.GetStorySession.saveState.miscWorldSaveData.badWarpTutorialCounter > ((room.game.GetStorySession.saveState.miscWorldSaveData.numberOfBadWarpsGenerated > 0) ? 1 : 3))
		{
			Destroy();
		}
		else if (room.game.session.Players[0].realizedCreature != null && room.game.cameras[0].hud != null && room.game.cameras[0].hud.textPrompt != null && room.game.cameras[0].hud.textPrompt.messages.Count < 1)
		{
			switch (message)
			{
			case 0:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Holding UP and SPECIAL without a karma flower is unstable and dangerous"), 120, 160, darken: false, hideHud: true);
				message++;
				break;
			case 1:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("However, this can also form new random paths in a pinch"), 120, 160, darken: false, hideHud: true);
				message++;
				break;
			case 2:
				room.game.GetStorySession.saveState.miscWorldSaveData.badWarpTutorialCounter = room.game.GetStorySession.saveState.miscWorldSaveData.badWarpTutorialCounter + 1;
				Destroy();
				break;
			}
		}
	}
}
