using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class CutsceneArtificerRobo : UpdatableAndDeletable
{
	public class Phase : ExtEnum<Phase>
	{
		public static Phase Init = new Phase("Init", register: true);

		public static Phase PlayerRun = new Phase("PlayerRun", register: true);

		public static Phase ActivateRobo = new Phase("ActivateRobo", register: true);

		public static Phase JollyPlayersEnter = new Phase("JollyPlayersEnter", register: true);

		public static Phase End = new Phase("End", register: true);

		public Phase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class StartController : Player.PlayerController
	{
		public Player player;

		public int playerIndex;

		public CutsceneArtificerRobo owner;

		public StartController(CutsceneArtificerRobo owner, Player player, int playerIndex)
		{
			this.owner = owner;
			this.player = player;
			this.playerIndex = playerIndex;
		}

		public override Player.InputPackage GetInput()
		{
			return owner.GetInput(player, playerIndex);
		}
	}

	public AncientBot bot;

	public bool initController;

	public Phase phase;

	public StartController startController;

	public int cutsceneTimer;

	public int timeInJollyPhase;

	public int timeBetweenJollyActivations = 80;

	public Player player
	{
		get
		{
			AbstractCreature firstAlivePlayer = room.game.FirstAlivePlayer;
			if (room.game.Players.Count > 0 && firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				return firstAlivePlayer.realizedCreature as Player;
			}
			return null;
		}
	}

	public CutsceneArtificerRobo(Room room)
	{
		Custom.Log("ARTIFICER CUTSCENE 2 START!");
		base.room = room;
		phase = Phase.Init;
		bot = new AncientBot(new Vector2(470f, 1790f), new Color(1f, 0f, 0f), null, online: false);
		room.AddObject(bot);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (phase != Phase.End)
		{
			int num = 0;
			for (int i = 0; i < room.game.Players.Count; i++)
			{
				if (room.game.Players[i] == room.game.FirstAlivePlayer || room.game.Players[i].realizedCreature == null)
				{
					continue;
				}
				num++;
				Player player = room.game.Players[i].realizedCreature as Player;
				if (!JollyPlayerActivated(num))
				{
					player.allowOutOfBounds = true;
					if (player.controller == null)
					{
						player.controller = new Player.NullController();
					}
					for (int j = 0; j < player.bodyChunks.Length; j++)
					{
						player.bodyChunks[j].vel = Vector2.zero;
						player.bodyChunks[j].pos = new Vector2(948f - (float)j * 20f, 2000f);
						player.bodyChunks[j].lastPos = new Vector2(948f - (float)j * 20f, 2000f);
					}
				}
				else
				{
					player.allowOutOfBounds = false;
					if (player.controller == null || !(player.controller is StartController))
					{
						player.controller = new StartController(this, player, i);
					}
				}
			}
		}
		if (phase == Phase.Init)
		{
			if (this.player != null && !initController && this.player.controller == null)
			{
				startController = new StartController(this, this.player, room.game.FirstAlivePlayerIndex);
				this.player.controller = startController;
				bot.tiedToObject = this.player;
				initController = true;
			}
			if (initController)
			{
				phase = Phase.PlayerRun;
			}
		}
		else if (phase == Phase.PlayerRun || phase == Phase.ActivateRobo)
		{
			cutsceneTimer++;
		}
		else if (phase == Phase.JollyPlayersEnter)
		{
			cutsceneTimer++;
			timeInJollyPhase++;
			int num2 = 0;
			for (int k = 0; k < room.game.Players.Count; k++)
			{
				if (room.game.Players[k] == room.game.FirstAlivePlayer || room.game.Players[k].realizedCreature == null)
				{
					continue;
				}
				num2++;
				Player player2 = room.game.Players[k].realizedCreature as Player;
				if (!JollyPlayerActivated(num2) || player2.room == room)
				{
					continue;
				}
				int num3 = timeInJollyPhase - ((num2 - 1) * timeBetweenJollyActivations + 1);
				if (num3 == 1 || num3 % 120 == 0)
				{
					for (int l = 0; l < player2.bodyChunks.Length; l++)
					{
						player2.bodyChunks[l].vel = Custom.DegToVec(Random.value * 360f) * 12f;
						player2.bodyChunks[l].pos = new Vector2(988f, 452f);
						player2.bodyChunks[l].lastPos = new Vector2(988f, 452f);
					}
				}
			}
			if (room.game.Players.Count == 1 || timeInJollyPhase >= room.game.Players.Count * timeBetweenJollyActivations)
			{
				phase = Phase.End;
			}
		}
		else
		{
			if (!(phase == Phase.End))
			{
				return;
			}
			Custom.Log("ARTIFICER CUTSCENE 2 END!");
			for (int m = 0; m < room.game.Players.Count; m++)
			{
				if (room.game.Players[m].realizedCreature != null)
				{
					(room.game.Players[m].realizedCreature as Player).controller = null;
				}
			}
			RainWorldGame.ForceSaveNewDenLocation(room.game, "GW_A24", saveWorldStates: true);
			room.game.cameras[0].ExitCutsceneMode();
			room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Hold pick-up to transform rocks or spears into explosives, at the cost of food."), 20, 500, darken: true, hideHud: true);
			Destroy();
		}
	}

	public bool JollyPlayerActivated(int index)
	{
		return timeInJollyPhase >= (index - 1) * timeBetweenJollyActivations + 1;
	}

	public Player.InputPackage GetInput(Player p, int playerIndex)
	{
		if (p == null)
		{
			return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, 0, 0, jmp: false, thrw: false, pckp: false, mp: false, crouchToggle: false);
		}
		int x = 0;
		int y = 0;
		bool jmp = false;
		bool pckp = false;
		bool thrw = false;
		if (p.abstractCreature == room.game.FirstAlivePlayer)
		{
			if (phase == Phase.PlayerRun)
			{
				x = 1;
				if (!p.standing && cutsceneTimer % 2 == 0)
				{
					y = 1;
				}
				if (p.mainBodyChunk.pos.x >= 420f)
				{
					phase = Phase.ActivateRobo;
					cutsceneTimer = 0;
				}
			}
			else if (phase == Phase.ActivateRobo)
			{
				if (bot.myAnimation == AncientBot.Animation.IdleOffline && cutsceneTimer >= 45)
				{
					bot.myAnimation = AncientBot.Animation.TurnOn;
				}
				if (bot.myMovement != AncientBot.FollowMode.Offline)
				{
					phase = Phase.JollyPlayersEnter;
					if (room.game.IsStorySession)
					{
						(room.game.session as StoryGameSession).saveState.hasRobo = true;
					}
					p.myRobot = bot;
				}
			}
		}
		else if (phase == Phase.JollyPlayersEnter)
		{
			if (p.room != room)
			{
				x = 1;
			}
			if (p.room == room && p.mainBodyChunk.pos.x < (float)(370 - 30 * playerIndex))
			{
				x = 1;
				y = ((cutsceneTimer % 5 == 0) ? 1 : 0);
			}
		}
		return new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.None, x, y, jmp, thrw, pckp, mp: false, crouchToggle: false);
	}
}
