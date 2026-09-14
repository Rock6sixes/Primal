using UnityEngine;

namespace Music;

public class PlayerThreatTracker
{
	public MusicPlayer musicPlayer;

	public ThreatDetermination threatDetermine;

	public int playerNumber;

	public float recommendedDroneVolume = 0.3f;

	public float currentThreat;

	public float currentMusicAgnosticThreat;

	public int room = -1;

	public int lastRoom = -1;

	public int lastLastRoom = -1;

	public int roomSwitches;

	public int roomSwitchDelay = 200;

	public string region;

	public float ghostMode;

	public float spinningTopLaughMode;

	public PlayerThreatTracker(MusicPlayer musicPlayer, int playerNumber)
	{
		this.musicPlayer = musicPlayer;
		this.playerNumber = playerNumber;
		threatDetermine = new ThreatDetermination(playerNumber);
	}

	public void Update()
	{
		if (musicPlayer.manager.currentMainLoop == null || musicPlayer.manager.currentMainLoop.ID != ProcessManager.ProcessID.Game)
		{
			recommendedDroneVolume = 0f;
			currentThreat = 0f;
			currentMusicAgnosticThreat = 0f;
			region = null;
		}
		else
		{
			if (playerNumber >= (musicPlayer.manager.currentMainLoop as RainWorldGame).Players.Count || !((musicPlayer.manager.currentMainLoop as RainWorldGame).Players[playerNumber].realizedCreature is Player { room: not null } player))
			{
				return;
			}
			if (player.room.game.GameOverModeActive || player.redsIllness != null)
			{
				recommendedDroneVolume = 0f;
				currentThreat = 0f;
				currentMusicAgnosticThreat = 0f;
				return;
			}
			recommendedDroneVolume = player.room.roomSettings.BkgDroneVolume;
			if (!player.room.world.rainCycle.MusicAllowed && player.room.roomSettings.DangerType != RoomRain.DangerType.None)
			{
				recommendedDroneVolume = 0f;
			}
			string text = null;
			int num = -1;
			if ((musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].ghostMode > (ModManager.MMF ? 0.1f : 0f))
			{
				float num2 = -1f;
				if (player.room.world.worldGhost != null)
				{
					num2 = player.room.world.worldGhost.GhostMode(player.room.abstractRoom, player.abstractCreature.world.RoomToWorldPos(player.mainBodyChunk.pos, player.room.abstractRoom.index));
					text = player.room.world.worldGhost.songName;
				}
				if (ModManager.Watcher)
				{
					foreach (GhostWorldPresence spinningTopPresence in player.room.world.spinningTopPresences)
					{
						float num3 = spinningTopPresence.GhostMode(player.room.abstractRoom, player.abstractCreature.world.RoomToWorldPos(player.mainBodyChunk.pos, player.room.abstractRoom.index));
						if (num3 > num2)
						{
							num2 = num3;
							text = spinningTopPresence.songName;
							num = spinningTopPresence.spinningTopSpawnId;
						}
					}
					if (player.room.VoidWeaverActive)
					{
						num2 = 1f;
						text = player.room.world.weaverPresence?.songName;
						num = -1;
					}
				}
				if (text == null)
				{
					ghostMode = 1f;
				}
				else if (ModManager.Watcher && num != -1)
				{
					ghostMode = (musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].ghostMode;
				}
				else
				{
					ghostMode = num2;
				}
			}
			else
			{
				ghostMode = 0f;
			}
			spinningTopLaughMode = 0f;
			if (ghostMode > 0f)
			{
				recommendedDroneVolume = 0f;
				musicPlayer.FadeOutAllNonGhostSongs(120f);
				if (text != null && (musicPlayer.song == null || !(musicPlayer.song is GhostSong)))
				{
					if (ModManager.Watcher && num != -1)
					{
						musicPlayer.RequestGhostSong(text, num);
					}
					else
					{
						musicPlayer.RequestGhostSong(text);
					}
				}
			}
			else
			{
				GhostWorldPresence ghostWorldPresence = null;
				for (int i = 0; i < player.room.world.spinningTopPresences.Count; i++)
				{
					if (player.room.world.spinningTopPresences[i].LaughMode(player.room.abstractRoom) > 0f)
					{
						ghostWorldPresence = player.room.world.spinningTopPresences[i];
						break;
					}
				}
				if ((musicPlayer.manager.currentMainLoop as RainWorldGame).cameras[0].spinningTopLaughMode > 0f)
				{
					spinningTopLaughMode = 1f;
					recommendedDroneVolume = 0f;
					musicPlayer.FadeOutAllNonGhostSongs(120f);
					if (ghostWorldPresence != null && (musicPlayer.song == null || !(musicPlayer.song is GhostSong)))
					{
						musicPlayer.RequestGhostSong(ghostWorldPresence.songName, ghostWorldPresence.spinningTopSpawnId);
					}
				}
			}
			if (!player.room.world.singleRoomWorld)
			{
				if (player.room.abstractRoom.index != room)
				{
					lastLastRoom = lastRoom;
					lastRoom = room;
					room = player.room.abstractRoom.index;
					if (room != lastLastRoom)
					{
						roomSwitches++;
						string text2 = ((player.room.world.region.regionParams.proceduralMusicBank == "") ? player.room.world.region.name : player.room.world.region.regionParams.proceduralMusicBank);
						if (text2 != region)
						{
							region = text2;
							musicPlayer.NewRegion(region);
						}
					}
				}
				if (roomSwitches > 0 && roomSwitchDelay > 0)
				{
					roomSwitchDelay--;
					if (roomSwitchDelay < 1)
					{
						if (musicPlayer.song != null)
						{
							musicPlayer.song.PlayerToNewRoom();
						}
						if (musicPlayer.nextSong != null)
						{
							musicPlayer.nextSong.PlayerToNewRoom();
						}
						roomSwitchDelay = Random.Range(80, 400);
						roomSwitches--;
					}
				}
			}
			else if ((musicPlayer.manager.currentMainLoop as RainWorldGame).IsArenaSession && (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge && (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.chMeta != null && !string.IsNullOrEmpty((musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.chMeta.threatMusic))
			{
				string threatMusic = (musicPlayer.manager.currentMainLoop as RainWorldGame).GetArenaGameSession.chMeta.threatMusic;
				if (region != threatMusic)
				{
					region = threatMusic;
					musicPlayer.NewRegion(region);
				}
			}
			threatDetermine.Update(musicPlayer.manager.currentMainLoop as RainWorldGame);
			if (musicPlayer.song != null)
			{
				threatDetermine.currentThreat = 0f;
			}
			currentThreat = threatDetermine.currentThreat;
			currentMusicAgnosticThreat = threatDetermine.currentMusicAgnosticThreat;
		}
	}
}
