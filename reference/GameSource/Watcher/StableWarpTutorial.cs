namespace Watcher;

public class StableWarpTutorial : UpdatableAndDeletable
{
	public int message;

	public StableWarpTutorial(Room room)
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
		else if (room.game.GetStorySession.saveState.miscWorldSaveData.stableWarpTutorialCounter > ((room.game.GetStorySession.saveState.miscWorldSaveData.numberOfWarpPointsGenerated > 0) ? 1 : 3))
		{
			Destroy();
		}
		else if (room.game.session.Players[0].realizedCreature != null && room.game.cameras[0].hud != null && room.game.cameras[0].hud.textPrompt != null && room.game.cameras[0].hud.textPrompt.messages.Count < 1)
		{
			switch (message)
			{
			case 0:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Hold UP and SPECIAL together to form new random paths"), 120, 160, darken: false, hideHud: true);
				message++;
				break;
			case 1:
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("This will consume the effect of a karma flower in the process"), 120, 160, darken: false, hideHud: true);
				message++;
				break;
			case 2:
				room.game.GetStorySession.saveState.miscWorldSaveData.stableWarpTutorialCounter = room.game.GetStorySession.saveState.miscWorldSaveData.stableWarpTutorialCounter + 1;
				room.game.GetStorySession.saveState.miscWorldSaveData.cycleLastShownStableWarpTutorial = room.game.GetStorySession.saveState.cycleNumber;
				Destroy();
				break;
			}
		}
	}
}
