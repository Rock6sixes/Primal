using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class ScavengersWorldAI : World.WorldProcess
{
	public class Outpost
	{
		public ScavengersWorldAI worldAI;

		public int room;

		public ScavengerAbstractAI.ScavengerSquad guardSquad;

		public int feePayed;

		public int[] killSquads;

		public int IdealGuardForce => Math.Min(7, worldAI.scavengers.Count / worldAI.outPosts.Count);

		public Outpost(ScavengersWorldAI worldAI, int room)
		{
			this.worldAI = worldAI;
			this.room = room;
			killSquads = new int[4];
		}
	}

	public class Trader
	{
		public ScavengersWorldAI worldAI;

		public int room;

		public ScavengerAbstractAI.ScavengerSquad squad;

		public bool transgressedByPlayer;

		public Trader(ScavengersWorldAI worldAI, int room)
		{
			this.worldAI = worldAI;
			this.room = room;
		}

		public void Update()
		{
			if (!transgressedByPlayer)
			{
				if (squad != null && !squad.Active)
				{
					Custom.Log("trade squad inactive.");
					squad = null;
				}
				if (squad == null)
				{
					FindTrader();
				}
			}
		}

		public void FindTrader()
		{
			ScavengerAbstractAI scavengerAbstractAI = null;
			float num = 0f;
			for (int i = 0; i < worldAI.scavengers.Count; i++)
			{
				if (ScavScore(worldAI.scavengers[i]) > num)
				{
					num = ScavScore(worldAI.scavengers[i]);
					scavengerAbstractAI = worldAI.scavengers[i];
				}
			}
			if (scavengerAbstractAI != null)
			{
				if (scavengerAbstractAI.squad != null)
				{
					scavengerAbstractAI.squad.RemoveMember(scavengerAbstractAI.parent);
				}
				scavengerAbstractAI.squad = new ScavengerAbstractAI.ScavengerSquad(scavengerAbstractAI.parent);
				scavengerAbstractAI.squad.missionType = ScavengerAbstractAI.ScavengerSquad.MissionID.Trade;
				scavengerAbstractAI.squad.tradeSpot = this;
				(scavengerAbstractAI.squad.leader.abstractAI as ScavengerAbstractAI).UpdateMissionAppropriateGear();
				squad = scavengerAbstractAI.squad;
				Custom.Log($"{scavengerAbstractAI.parent.ID} selected as trader in {room}");
			}
		}

		public float ScavScore(ScavengerAbstractAI testScav)
		{
			if (testScav.parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar || testScav.parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
			{
				return 0f;
			}
			if (worldAI.world.GetAbstractRoom(room).AttractionForCreature(testScav.parent) == AbstractRoom.CreatureRoomAttraction.Forbidden)
			{
				return 0f;
			}
			return Mathf.Lerp(worldAI.world.game.SeededRandom(room + testScav.parent.ID.RandomSeed), 1f, Mathf.InverseLerp(0.5f, 1f, Mathf.Abs(testScav.parent.personality.dominance - 0.5f)));
		}
	}

	public class WorldFloodFiller
	{
		public World world;

		public List<WorldCoordinate> checkNext;

		public bool[][] nodesMatrix;

		public bool[] roomsMatrix;

		public bool finished;

		public bool IsRoomAccessible(WorldCoordinate node)
		{
			return IsRoomAccessible(node.room);
		}

		public bool IsRoomAccessible(int room)
		{
			int num = room - world.firstRoomIndex;
			if (num < 0 || num >= roomsMatrix.Length)
			{
				return false;
			}
			return roomsMatrix[num];
		}

		public bool IsNodeAccessible(WorldCoordinate node)
		{
			return IsNodeAccessible(node.room, node.abstractNode);
		}

		public bool IsNodeAccessible(int room, int node)
		{
			int num = room - world.firstRoomIndex;
			if (num < 0 || num >= nodesMatrix.Length || node < 0)
			{
				return false;
			}
			return nodesMatrix[num][node];
		}

		public WorldFloodFiller(World world, WorldCoordinate startPosition)
		{
			this.world = world;
			nodesMatrix = new bool[world.NumberOfRooms][];
			for (int i = 0; i < world.NumberOfRooms; i++)
			{
				nodesMatrix[i] = new bool[world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length];
			}
			roomsMatrix = new bool[world.NumberOfRooms];
			checkNext = new List<WorldCoordinate>();
			for (int j = 0; j < world.NumberOfRooms; j++)
			{
				for (int k = 0; k < world.GetAbstractRoom(j + world.firstRoomIndex).nodes.Length; k++)
				{
					if (world.GetAbstractRoom(j + world.firstRoomIndex).nodes[k].type == AbstractRoomNode.Type.RegionTransportation)
					{
						checkNext.Add(new WorldCoordinate(j + world.firstRoomIndex, -1, -1, k));
						roomsMatrix[j] = true;
						nodesMatrix[j][k] = true;
					}
				}
			}
		}

		public void Update()
		{
			if (checkNext.Count < 1)
			{
				finished = true;
				return;
			}
			WorldCoordinate worldCoordinate = checkNext[0];
			checkNext.RemoveAt(0);
			if (!worldCoordinate.NodeDefined)
			{
				return;
			}
			AbstractRoom abstractRoom = world.GetAbstractRoom(worldCoordinate);
			if (abstractRoom == null || worldCoordinate.abstractNode >= abstractRoom.nodes.Length)
			{
				return;
			}
			if (world.GetNode(worldCoordinate).type == AbstractRoomNode.Type.Exit && abstractRoom.connections[worldCoordinate.abstractNode] > -1)
			{
				WorldCoordinate item = new WorldCoordinate(abstractRoom.connections[worldCoordinate.abstractNode], -1, -1, world.GetAbstractRoom(abstractRoom.connections[worldCoordinate.abstractNode]).ExitIndex(worldCoordinate.room));
				if (item.abstractNode > -1 && !nodesMatrix[item.room - world.firstRoomIndex][item.abstractNode])
				{
					checkNext.Add(item);
					roomsMatrix[item.room - world.firstRoomIndex] = true;
					nodesMatrix[item.room - world.firstRoomIndex][item.abstractNode] = true;
				}
			}
			for (int i = 0; i < abstractRoom.nodes.Length; i++)
			{
				if (!nodesMatrix[worldCoordinate.room - world.firstRoomIndex][i] && i != worldCoordinate.abstractNode && abstractRoom.ConnectionAndBackPossible(worldCoordinate.abstractNode, i, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger)))
				{
					checkNext.Add(new WorldCoordinate(worldCoordinate.room, -1, -1, i));
					roomsMatrix[worldCoordinate.room - world.firstRoomIndex] = true;
					nodesMatrix[worldCoordinate.room - world.firstRoomIndex][i] = true;
				}
			}
		}
	}

	public List<ScavengerAbstractAI> scavengers;

	public List<Outpost> outPosts;

	public List<Trader> traders;

	public WorldFloodFiller floodFiller;

	public List<ScavengerAbstractAI.ScavengerSquad> playerAssignedSquads;

	public int playerSquadCooldown;

	public ScavengersWorldAI(World world)
		: base(world)
	{
		base.world = world;
		scavengers = new List<ScavengerAbstractAI>();
		outPosts = new List<Outpost>();
		traders = new List<Trader>();
		playerAssignedSquads = new List<ScavengerAbstractAI.ScavengerSquad>();
		for (int i = 0; i < world.NumberOfRooms; i++)
		{
			if (world.GetAbstractRoom(i + world.firstRoomIndex).scavengerOutpost)
			{
				outPosts.Add(new Outpost(this, i + world.firstRoomIndex));
			}
			if (world.GetAbstractRoom(i + world.firstRoomIndex).scavengerTrader)
			{
				traders.Add(new Trader(this, i + world.firstRoomIndex));
			}
		}
		floodFiller = new WorldFloodFiller(world, new WorldCoordinate(world.offScreenDen.index, -1, -1, 0));
	}

	public void AddScavenger(ScavengerAbstractAI newScav)
	{
		for (int i = 0; i < scavengers.Count; i++)
		{
			if (scavengers[i] == newScav)
			{
				return;
			}
		}
		scavengers.Add(newScav);
	}

	public Outpost OutPostInNeedOfGuard()
	{
		for (int i = 0; i < outPosts.Count; i++)
		{
			if (outPosts[i].guardSquad == null && (!ModManager.MSC || !world.game.globalRain.DrainWorldPositionFlooded(new WorldCoordinate(outPosts[i].room, 0, 7, -1))))
			{
				return outPosts[i];
			}
		}
		return null;
	}

	public override void Update()
	{
		if (!floodFiller.finished)
		{
			floodFiller.Update();
		}
		if (scavengers.Count == 0)
		{
			return;
		}
		ScavengerAbstractAI scavengerAbstractAI;
		for (int i = 0; i < outPosts.Count; i++)
		{
			if (outPosts[i].guardSquad != null && (!outPosts[i].guardSquad.Active || outPosts[i].guardSquad.guardOutpost != outPosts[i]))
			{
				if (outPosts[i].guardSquad.guardOutpost == outPosts[i])
				{
					outPosts[i].guardSquad.guardOutpost = null;
				}
				outPosts[i].guardSquad = null;
			}
			else
			{
				if (outPosts[i].guardSquad == null || outPosts[i].guardSquad.members.Count >= outPosts[i].IdealGuardForce)
				{
					continue;
				}
				List<ScavengerAbstractAI> list = new List<ScavengerAbstractAI>();
				foreach (ScavengerAbstractAI scavenger in scavengers)
				{
					if (world.GetAbstractRoom(outPosts[i].room).AttractionForCreature(scavenger.parent) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						list.Add(scavenger);
					}
				}
				if (list.Count != 0)
				{
					scavengerAbstractAI = list[UnityEngine.Random.Range(0, list.Count)];
					if (scavengerAbstractAI.squad == null)
					{
						outPosts[i].guardSquad.AddMember(scavengerAbstractAI.parent);
						scavengerAbstractAI.GoToRoom(outPosts[i].room);
					}
					else if (!scavengerAbstractAI.squad.HasAMission)
					{
						scavengerAbstractAI.squad.RemoveMember(scavengerAbstractAI.parent);
						outPosts[i].guardSquad.AddMember(scavengerAbstractAI.parent);
						scavengerAbstractAI.GoToRoom(outPosts[i].room);
					}
				}
			}
		}
		for (int j = 0; j < traders.Count; j++)
		{
			traders[j].Update();
		}
		for (int num = playerAssignedSquads.Count - 1; num >= 0; num--)
		{
			if (!playerAssignedSquads[num].Active || playerAssignedSquads[num].missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.None)
			{
				playerAssignedSquads.RemoveAt(num);
			}
		}
		if (!ModManager.MSC || playerSquadCooldown <= 0)
		{
			if (world.game.session.creatureCommunities.scavengerShyness == 0f)
			{
				for (int k = 0; k < world.game.Players.Count; k++)
				{
					float num2 = world.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, world.RegionNumber, k);
					int num3 = 0;
					if (num2 < 0f)
					{
						for (int l = 0; l < outPosts.Count; l++)
						{
							num3 += outPosts[l].killSquads[k];
						}
						for (int m = 0; m < traders.Count; m++)
						{
							if (traders[m].transgressedByPlayer)
							{
								num3++;
							}
						}
					}
					if ((ModManager.MMF && (MMF.cfgScavengerKillSquadDelay.Value || (ModManager.MSC && world.game.IsStorySession && world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer)) && world.game.Players[k].Room != null && (world.game.Players[k].Room.shelter || world.game.Players[k].Room.gate || world.GetAbstractRoom(world.game.Players[k].pos.room)?.AttractionForCreature(CreatureTemplate.Type.Scavenger) == AbstractRoom.CreatureRoomAttraction.Forbidden)) || (ModManager.MMF && (MMF.cfgScavengerKillSquadDelay.Value || (ModManager.MSC && world.game.IsStorySession && world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer)) && world.game.timeInRegionThisCycle < 4800))
					{
						ResetSquadCooldown(num2);
					}
					else if (playerAssignedSquads.Count < num3 + (int)(Mathf.InverseLerp(0.8f, 1f, Mathf.Abs(num2)) * 2f))
					{
						scavengerAbstractAI = scavengers[UnityEngine.Random.Range(0, scavengers.Count)];
						if (scavengerAbstractAI.squad != null && !scavengerAbstractAI.squad.HasAMission)
						{
							playerAssignedSquads.Add(scavengerAbstractAI.squad);
							scavengerAbstractAI.squad.targetCreature = world.game.Players[k];
							scavengerAbstractAI.squad.missionType = ((num2 < 0f) ? ScavengerAbstractAI.ScavengerSquad.MissionID.HuntCreature : ScavengerAbstractAI.ScavengerSquad.MissionID.ProtectCreature);
							ResetSquadCooldown(num2);
							Custom.Log($"-------A SCAV SQUAD IS AFTER THE PLAYER {scavengerAbstractAI.squad.missionType}");
						}
					}
				}
			}
		}
		else
		{
			playerSquadCooldown--;
		}
		scavengerAbstractAI = scavengers[UnityEngine.Random.Range(0, scavengers.Count)];
		if (scavengerAbstractAI.parent.state.dead)
		{
			scavengers.Remove(scavengerAbstractAI);
		}
	}

	public void ResetSquadCooldown(float like)
	{
		if (ModManager.MMF && (MMF.cfgScavengerKillSquadDelay.Value || (ModManager.MSC && world.game.IsStorySession && world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer)))
		{
			if (world.region == null)
			{
				playerSquadCooldown = ((like >= 0f) ? UnityEngine.Random.Range(1100, 1700) : UnityEngine.Random.Range(4200, 8200));
			}
			else
			{
				playerSquadCooldown = ((like >= 0f) ? UnityEngine.Random.Range(world.region.regionParams.scavengerDelayRepeatMin, world.region.regionParams.scavengerDelayRepeatMax) : UnityEngine.Random.Range(world.region.regionParams.scavengerDelayInitialMin, world.region.regionParams.scavengerDelayInitialMax));
			}
		}
	}
}
