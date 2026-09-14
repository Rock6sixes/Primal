using System;
using RWCustom;
using UnityEngine;

public class ThreatDetermination
{
	public int threatDeclineCounter;

	public float currentThreat;

	public float threat;

	public int musicAgnosticThreatDeclineCounter;

	public float currentMusicAgnosticThreat;

	public float musicAgnosticThreat;

	public int room = -1;

	public int lastRoom = -1;

	public int lastLastRoom = -1;

	public int playerNumber;

	public ThreatDetermination(int playerNumber)
	{
		this.playerNumber = playerNumber;
	}

	public void Update(RainWorldGame game)
	{
		if (game.manager.currentMainLoop == null || game.manager.currentMainLoop.ID != ProcessManager.ProcessID.Game)
		{
			currentThreat = 0f;
			currentMusicAgnosticThreat = 0f;
		}
		else
		{
			if (playerNumber >= game.Players.Count || !(game.Players[playerNumber].realizedCreature is Player { room: not null } player))
			{
				return;
			}
			if (player.room.game.GameOverModeActive || player.redsIllness != null)
			{
				currentThreat = 0f;
				currentMusicAgnosticThreat = 0f;
				return;
			}
			float num = player.room.roomSettings.BkgDroneVolume;
			if (!player.room.world.rainCycle.MusicAllowed && player.room.roomSettings.DangerType != RoomRain.DangerType.None)
			{
				num = 0f;
			}
			float num2 = 0f;
			GhostWorldPresence ghostWorldPresence = null;
			if (game.cameras[0].ghostMode > (ModManager.MMF ? 0.1f : 0f))
			{
				float num3 = -1f;
				if (player.room.world.worldGhost != null)
				{
					num3 = player.room.world.worldGhost.GhostMode(player.room.abstractRoom, player.abstractCreature.world.RoomToWorldPos(player.mainBodyChunk.pos, player.room.abstractRoom.index));
					ghostWorldPresence = player.room.world.worldGhost;
				}
				if (ModManager.Watcher)
				{
					for (int i = 0; i < player.room.world.spinningTopPresences.Count; i++)
					{
						float num4 = player.room.world.spinningTopPresences[i].GhostMode(player.room.abstractRoom, player.abstractCreature.world.RoomToWorldPos(player.mainBodyChunk.pos, player.room.abstractRoom.index));
						if (num4 > num3)
						{
							num3 = num4;
							ghostWorldPresence = player.room.world.spinningTopPresences[i];
						}
					}
				}
				num2 = ((ghostWorldPresence != null) ? num3 : 1f);
			}
			else
			{
				num2 = 0f;
			}
			if (num2 > 0f)
			{
				num = 0f;
			}
			if (!player.room.world.singleRoomWorld && player.room.abstractRoom.index != room)
			{
				lastLastRoom = lastRoom;
				lastRoom = room;
				room = player.room.abstractRoom.index;
				if (room != lastLastRoom)
				{
					threatDeclineCounter = Math.Max(threatDeclineCounter, 120);
					musicAgnosticThreatDeclineCounter = Math.Max(musicAgnosticThreatDeclineCounter, 120);
				}
			}
			threat = 0f;
			musicAgnosticThreat = 0f;
			for (int j = 0; j < player.room.abstractRoom.creatures.Count; j++)
			{
				if (player.room.abstractRoom.creatures[j].realizedCreature != null && player.room.abstractRoom.creatures[j].realizedCreature != player)
				{
					float num5 = ThreatOfCreature(player.room.abstractRoom.creatures[j].realizedCreature, player);
					if (num > 0f)
					{
						threat = Mathf.Lerp(threat + num5, Mathf.Max(threat, num5), 0.75f);
					}
					musicAgnosticThreat = Mathf.Lerp(musicAgnosticThreat + num5, Mathf.Max(musicAgnosticThreat, num5), 0.75f);
				}
			}
			int num6 = 0;
			Player.InputPackage inputPackage = player.input[0];
			for (int k = 1; k < player.input.Length; k++)
			{
				if (player.input[k].x != inputPackage.x)
				{
					num6++;
				}
				if (player.input[k].y != inputPackage.y)
				{
					num6++;
				}
				if (player.input[k].jmp != inputPackage.jmp)
				{
					num6 += 2;
				}
				if (player.input[k].thrw != inputPackage.thrw)
				{
					num6 += 2;
				}
				if (player.input[k].pckp != inputPackage.pckp)
				{
					num6++;
				}
			}
			if (num > 0f)
			{
				threat = Mathf.Pow(Mathf.Clamp(threat, 0f, 1f), Custom.LerpMap(num6, 5f, 30f, 1.2f, 0.8f));
			}
			musicAgnosticThreat = Mathf.Pow(Mathf.Clamp(musicAgnosticThreat, 0f, 1f), Custom.LerpMap(num6, 5f, 30f, 1.2f, 0.8f));
			if (num2 > 0f)
			{
				musicAgnosticThreat = 0f;
			}
			if (threat > 0.25f && threat < 0.75f)
			{
				threat = Mathf.Lerp(threat, 0.5f, Mathf.InverseLerp(0.25f, 0f, Mathf.Abs(threat - 0.5f)));
			}
			if (musicAgnosticThreat > 0.25f && musicAgnosticThreat < 0.75f)
			{
				musicAgnosticThreat = Mathf.Lerp(musicAgnosticThreat, 0.5f, Mathf.InverseLerp(0.25f, 0f, Mathf.Abs(musicAgnosticThreat - 0.5f)));
			}
			if (threatDeclineCounter > 0)
			{
				threatDeclineCounter--;
			}
			if (musicAgnosticThreatDeclineCounter > 0)
			{
				musicAgnosticThreatDeclineCounter--;
			}
			if (threat < currentThreat - 0.35f)
			{
				threatDeclineCounter = Math.Max(threatDeclineCounter, 10);
			}
			if (musicAgnosticThreat < currentMusicAgnosticThreat - 0.35f)
			{
				musicAgnosticThreatDeclineCounter = Math.Max(musicAgnosticThreatDeclineCounter, 10);
			}
			if (currentThreat < threat)
			{
				currentThreat = Mathf.Min(1f, currentThreat + 1f / Mathf.Lerp(280f, 80f, threat));
			}
			else if (threatDeclineCounter > 0)
			{
				currentThreat = Mathf.Max(0f, currentThreat - 1f / Mathf.Lerp(800f, 4200f, threat));
			}
			else
			{
				currentThreat = Mathf.Max(0f, currentThreat - 1f / Mathf.Lerp(1600f, 22000f, Mathf.Pow(threat, 0.25f)));
			}
			if (currentMusicAgnosticThreat < musicAgnosticThreat)
			{
				currentMusicAgnosticThreat = Mathf.Min(1f, currentMusicAgnosticThreat + 1f / Mathf.Lerp(280f, 80f, musicAgnosticThreat));
			}
			else if (musicAgnosticThreatDeclineCounter > 0)
			{
				currentMusicAgnosticThreat = Mathf.Max(0f, currentMusicAgnosticThreat - 1f / Mathf.Lerp(800f, 4200f, musicAgnosticThreat));
			}
			else
			{
				currentMusicAgnosticThreat = Mathf.Max(0f, currentMusicAgnosticThreat - 1f / Mathf.Lerp(1600f, 22000f, Mathf.Pow(musicAgnosticThreat, 0.25f)));
			}
		}
	}

	public float ThreatOfCreature(Creature creature, Player player)
	{
		float dangerousToPlayer = creature.Template.dangerousToPlayer;
		if (creature.abstractCreature.rippleLayer != player.abstractCreature.rippleLayer && !creature.abstractCreature.rippleBothSides && !player.abstractCreature.rippleBothSides)
		{
			return 0f;
		}
		if (dangerousToPlayer == 0f)
		{
			return 0f;
		}
		if (creature.dead)
		{
			return dangerousToPlayer / 3f;
		}
		bool flag = false;
		float f = 0f;
		if (creature.abstractCreature.abstractAI != null && creature.abstractCreature.abstractAI.RealAI != null && creature.abstractCreature.abstractAI.RealAI.tracker != null)
		{
			for (int i = 0; i < creature.abstractCreature.abstractAI.RealAI.tracker.CreaturesCount; i++)
			{
				if (creature.abstractCreature.abstractAI.RealAI.tracker.GetRep(i).representedCreature == player.abstractCreature)
				{
					flag = creature.abstractCreature.abstractAI.RealAI.tracker.GetRep(i).VisualContact;
					f = creature.abstractCreature.abstractAI.RealAI.tracker.GetRep(i).EstimatedChanceOfFinding;
					break;
				}
			}
		}
		dangerousToPlayer *= Custom.LerpMap(Vector2.Distance(creature.DangerPos, player.mainBodyChunk.pos), 300f, 2400f, 1f, flag ? 0.2f : 0f);
		dangerousToPlayer *= 1f + Mathf.InverseLerp(300f, 20f, Vector2.Distance(creature.DangerPos, player.mainBodyChunk.pos)) * Mathf.InverseLerp(2f, 7f, creature.firstChunk.vel.magnitude);
		dangerousToPlayer *= Mathf.Lerp(1f / 3f, 1f, Mathf.Pow(f, 0.75f));
		if (creature.abstractCreature.abstractAI != null && creature.abstractCreature.abstractAI.RealAI != null)
		{
			dangerousToPlayer *= creature.abstractCreature.abstractAI.RealAI.CurrentPlayerAggression(player.abstractCreature);
		}
		return dangerousToPlayer;
	}
}
