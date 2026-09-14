using RWCustom;
using UnityEngine;

namespace Music;

public class GhostSong : Song
{
	public int outOfGhostRangeCounter;

	public int spinningTopSpawnId;

	public float timeToNextLaughModeSwitch;

	public int laughModeSwitchTicker;

	public bool laughingAudible;

	public float laughFade;

	public float laughTargetVolume;

	public float musicTargetVolume;

	public GhostSong(MusicPlayer musicPlayer, string ghostSongName)
		: base(musicPlayer, ghostSongName, MusicPlayer.MusicContext.StoryMode)
	{
		priority = 1.1f;
		stopAtGate = true;
		stopAtDeath = true;
		fadeInTime = 200f;
		spinningTopSpawnId = -1;
	}

	public GhostSong(MusicPlayer musicPlayer, string ghostSongName, int spinningTopSpawnId)
		: base(musicPlayer, ghostSongName, MusicPlayer.MusicContext.StoryMode)
	{
		priority = 1.1f;
		stopAtGate = true;
		stopAtDeath = true;
		fadeInTime = 200f;
		this.spinningTopSpawnId = spinningTopSpawnId;
	}

	public override void Update()
	{
		base.Update();
		if (spinningTopSpawnId >= 0)
		{
			if (subTracks.Count == 1)
			{
				SubTrack subTrack = new SubTrack(this, 1, (spinningTopSpawnId % 6 == 0) ? "RWTW_ST_Amb_01" : ((spinningTopSpawnId % 6 == 1) ? "RWTW_ST_Amb_02" : ((spinningTopSpawnId % 6 == 2) ? "RWTW_ST_Amb_03" : ((spinningTopSpawnId % 6 == 3) ? "RWTW_ST_Amb_04" : ((spinningTopSpawnId % 6 == 4) ? "RWTW_ST_Amb_05" : "RWTW_ST_Amb_06")))));
				subTrack.volume = 0f;
				subTracks.Add(subTrack);
				SwapLaughingMode();
			}
			if (musicPlayer.threatTracker != null && musicPlayer.threatTracker.ghostMode > 0.15f)
			{
				musicTargetVolume = Custom.LerpAndTick(musicTargetVolume, Mathf.Pow(musicPlayer.threatTracker.ghostMode, 0.5f) * 0.15f, 0.0005f, 0.0025f);
			}
			else
			{
				musicTargetVolume = 0f;
			}
			if (laughingAudible && musicTargetVolume == 0f)
			{
				laughFade = Mathf.Lerp(laughFade, 1f, 0.05f);
			}
			else
			{
				laughFade = Mathf.Lerp(laughFade, 0f, 0.025f);
			}
			laughModeSwitchTicker++;
			if ((float)laughModeSwitchTicker >= timeToNextLaughModeSwitch)
			{
				SwapLaughingMode();
			}
			laughTargetVolume = Custom.LerpAndTick(laughTargetVolume, 0.4f, 0.0005f, 0.0025f);
			baseVolume = 1f;
			subTracks[0].volume = Mathf.Lerp(subTracks[0].volume, musicTargetVolume, 0.05f);
			subTracks[1].volume = Mathf.Max(0.01f, laughTargetVolume * laughFade);
		}
		else
		{
			baseVolume = Custom.LerpAndTick(baseVolume, Mathf.Pow((musicPlayer.threatTracker != null) ? musicPlayer.threatTracker.ghostMode : 0f, 0.5f) * 0.4f, 0.0005f, 0.0025f);
		}
		if (musicPlayer.threatTracker == null || (musicPlayer.threatTracker.ghostMode == 0f && musicPlayer.threatTracker.spinningTopLaughMode == 0f))
		{
			outOfGhostRangeCounter++;
			if (outOfGhostRangeCounter > 200)
			{
				FadeOut(10f);
				musicPlayer.ResumeProcedural();
			}
		}
		else
		{
			outOfGhostRangeCounter = 0;
		}
	}

	public void SwapLaughingMode()
	{
		laughingAudible = !laughingAudible;
		if (laughingAudible)
		{
			timeToNextLaughModeSwitch = Random.Range(80, 300);
		}
		else
		{
			timeToNextLaughModeSwitch = Random.Range(40, 400);
		}
		laughModeSwitchTicker = 0;
	}
}
