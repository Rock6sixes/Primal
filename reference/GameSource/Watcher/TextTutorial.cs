namespace Watcher;

public class TextTutorial : UpdatableAndDeletable
{
	public struct Message
	{
		public string text;

		public int time;

		public int wait;

		public Message(string text, int time, int wait)
		{
			this.text = text;
			this.time = time;
			this.wait = wait;
		}
	}

	public int message;

	public Message[] messages;

	public DeathPersistentSaveData.Tutorial tutorial;

	public bool watcherTutorial;

	public DeathPersistentSaveData DeathPersistentSaveData => room.game.GetStorySession.saveState.deathPersistentSaveData;

	public TextTutorial(Room room, DeathPersistentSaveData.Tutorial tutorial, Message[] messages)
	{
		base.room = room;
		this.messages = messages;
		this.tutorial = tutorial;
		if (room.game.session.Players[0].pos.room != room.abstractRoom.index)
		{
			Destroy();
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!Active())
		{
			Destroy();
		}
		else if (room.game.session.Players[0].realizedCreature != null && room.game.cameras[0].hud != null && room.game.cameras[0].hud.textPrompt != null && room.game.cameras[0].hud.textPrompt.messages.Count < 1)
		{
			string text = room.game.manager.rainWorld.inGameTranslator.Translate(messages[message].text);
			room.game.cameras[0].hud.textPrompt.AddMessage(text, messages[message].wait, messages[message].time * 4, darken: false, hideHud: true);
			message++;
			if (message >= messages.Length)
			{
				FinishedTutorial();
				Destroy();
			}
		}
	}

	public virtual void FinishedTutorial()
	{
		DeathPersistentSaveData.SetTutorialValue(tutorial, flag: true);
	}

	public virtual bool Active()
	{
		if (watcherTutorial && !Watcher.cfgWatcherTutorials.Value)
		{
			return false;
		}
		if (room.game.IsStorySession)
		{
			if (!(tutorial == null))
			{
				return !DeathPersistentSaveData.tutorialMessages.Contains(tutorial);
			}
			return true;
		}
		return false;
	}
}
