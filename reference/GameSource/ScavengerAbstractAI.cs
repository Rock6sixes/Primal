using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class ScavengerAbstractAI : AbstractCreatureAI, IOwnAnAbstractSpacePathFinder
{
	public struct CoordinateAndFloat
	{
		public WorldCoordinate coord;

		public float flt;

		public CoordinateAndFloat(WorldCoordinate coord, float flt)
		{
			this.coord = coord;
			this.flt = flt;
		}
	}

	public class ScavengerSquad
	{
		public class MissionID : ExtEnum<MissionID>
		{
			public static MissionID None = new MissionID("None", register: true);

			public static MissionID GuardOutpost = new MissionID("GuardOutpost", register: true);

			public static MissionID HuntCreature = new MissionID("HuntCreature", register: true);

			public static MissionID ProtectCreature = new MissionID("ProtectCreature", register: true);

			public static MissionID Trade = new MissionID("Trade", register: true);

			public MissionID(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public List<AbstractCreature> members;

		public AbstractCreature leader;

		public Color color;

		public ScavengersWorldAI.Outpost guardOutpost;

		public ScavengersWorldAI.Trader tradeSpot;

		public AbstractCreature targetCreature;

		public MissionID mType;

		public bool Active
		{
			get
			{
				if (members.Count > 0)
				{
					return (leader.abstractAI as ScavengerAbstractAI).squad == this;
				}
				return false;
			}
		}

		public bool StayIn
		{
			get
			{
				if (members.Count >= 2 || missionType != MissionID.None)
				{
					if (leader != null)
					{
						return leader.state.alive;
					}
					return false;
				}
				return false;
			}
		}

		public bool HasAMission
		{
			get
			{
				if (!MissionRoom.HasValue)
				{
					return false;
				}
				if (guardOutpost == null && targetCreature == null && tradeSpot == null)
				{
					return false;
				}
				if (leader != null && (leader.abstractAI as ScavengerAbstractAI).RoomGhostScary(MissionRoom.Value) > 0f)
				{
					return false;
				}
				if (ModManager.MSC && targetCreature != null && (targetCreature.world.GetAbstractRoom(targetCreature.pos.room).shelter || targetCreature.world.GetAbstractRoom(targetCreature.pos.room).gate))
				{
					return false;
				}
				return true;
			}
		}

		public int? MissionRoom
		{
			get
			{
				if (guardOutpost != null)
				{
					return guardOutpost.room;
				}
				if (tradeSpot != null)
				{
					return tradeSpot.room;
				}
				if (targetCreature != null)
				{
					return targetCreature.pos.room;
				}
				return null;
			}
		}

		public MissionID missionType
		{
			get
			{
				if (mType == MissionID.None)
				{
					return MissionID.None;
				}
				if ((mType == MissionID.GuardOutpost && guardOutpost == null) || (mType == MissionID.HuntCreature && targetCreature == null) || (mType == MissionID.ProtectCreature && targetCreature == null) || (mType == MissionID.Trade && (tradeSpot == null || tradeSpot.transgressedByPlayer)))
				{
					mType = MissionID.None;
				}
				return mType;
			}
			set
			{
				mType = value;
			}
		}

		public bool StationaryMission
		{
			get
			{
				if (!(mType == MissionID.GuardOutpost))
				{
					return mType == MissionID.Trade;
				}
				return true;
			}
		}

		public ScavengerSquad(AbstractCreature leader)
		{
			this.leader = leader;
			members = new List<AbstractCreature> { leader };
			color = Custom.HSL2RGB(UnityEngine.Random.value, 1f, 0.5f);
		}

		public void AddMember(AbstractCreature newMember)
		{
			for (int i = 0; i < members.Count; i++)
			{
				if (members[i] == newMember)
				{
					Custom.Log("scav already member of this squad.", ((members[i].abstractAI as ScavengerAbstractAI).squad == this).ToString(), ((members[i].abstractAI as ScavengerAbstractAI).squad == null).ToString());
					return;
				}
			}
			(newMember.abstractAI as ScavengerAbstractAI).squad = this;
			members.Add(newMember);
			UpdateLeader();
		}

		public void RemoveMember(AbstractCreature noLongerMember)
		{
			for (int num = members.Count - 1; num >= 0; num--)
			{
				if (members[num] == noLongerMember)
				{
					(members[num].abstractAI as ScavengerAbstractAI).squad = null;
					members.RemoveAt(num);
				}
			}
			UpdateLeader();
		}

		public void Dissolve()
		{
			for (int num = members.Count - 1; num >= 0; num--)
			{
				(members[num].abstractAI as ScavengerAbstractAI).squad = null;
			}
			members.Clear();
			leader = null;
		}

		public void UpdateLeader()
		{
			float num = 0f;
			for (int i = 0; i < members.Count; i++)
			{
				if (LeaderScore(members[i]) > num)
				{
					num = LeaderScore(members[i]);
					leader = members[i];
				}
			}
		}

		public static float LeaderScore(AbstractCreature member)
		{
			if (member.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar)
			{
				return -1f;
			}
			return member.personality.dominance * (member.state as HealthState).health;
		}

		public void CommonMovement(int dstRoom, AbstractCreature notThisOne, bool onlyInRoom)
		{
			Custom.Log($"Squad {leader.ID} is moving to {leader.world.GetAbstractRoom(dstRoom).name}");
			for (int i = 0; i < members.Count; i++)
			{
				if (members[i] != notThisOne && (!onlyInRoom || members[i].pos.room == leader.pos.room))
				{
					(members[i].abstractAI as ScavengerAbstractAI).GoToRoom(dstRoom);
				}
			}
		}

		public bool DoesScavengerWantToBeInSquad(ScavengerAbstractAI testScav)
		{
			if (targetCreature != null)
			{
				if (missionType == MissionID.HuntCreature && testScav.parent.state.socialMemory.GetLike(targetCreature.ID) > 0.35f)
				{
					return false;
				}
				if (missionType == MissionID.ProtectCreature && testScav.parent.state.socialMemory.GetLike(targetCreature.ID) < -0.35f)
				{
					return false;
				}
			}
			return true;
		}
	}

	public int timeInRoom;

	public int freeze;

	public int dontMigrate;

	public bool lastInOffscreenDen;

	public ScavengerSquad squad;

	public int carryRocks;

	public WorldCoordinate unreachableSquadLeaderPos;

	public WorldCoordinate longTermMigration;

	public ScavengersWorldAI worldAI;

	public int controlledMigrateTime;

	public bool bringPearlHome;

	public bool missionAppropriateGear;

	public override float offscreenSpeedFac => Mathf.Max(0.5f, base.offscreenSpeedFac);

	public bool UnderSquadLeaderControl
	{
		get
		{
			if (squad != null)
			{
				return squad.leader != parent;
			}
			return false;
		}
	}

	public bool GhostOutOfCurrentRoom => RoomGhostScary(parent.pos.room) > 0f;

	public float Shyness => world.game.session.creatureCommunities.scavengerShyness;

	public ScavengerAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		if (ModManager.ChallengeModule)
		{
			if (world.game.IsArenaSession && world.game.GetArenaGameSession.chMeta != null && world.game.GetArenaGameSession.chMeta.seed >= 0)
			{
				if (world.game.GetArenaGameSession.chMeta.seed == 123)
				{
					int[] array = new int[12]
					{
						5845, 7576, 6566, 7033, 2710, 5930, 8274, 3007, 9509, 4917,
						1255, 7801
					};
					if (Scavenger.ArenaScavID < array.Length)
					{
						parent.ID.setAltSeed(array[Scavenger.ArenaScavID]);
					}
					else
					{
						parent.ID.setAltSeed(world.game.GetArenaGameSession.chMeta.seed + Scavenger.ArenaScavID);
					}
				}
				else
				{
					parent.ID.setAltSeed(world.game.GetArenaGameSession.chMeta.seed + Scavenger.ArenaScavID);
				}
			}
			Scavenger.ArenaScavID++;
		}
		carryRocks = Custom.IntClamp((int)(Mathf.Pow(world.game.SeededRandom(parent.ID.RandomSeed), Mathf.Lerp(2.5f, 0.5f, parent.personality.aggression * (1f - parent.personality.dominance))) * 4.5f), 0, 4);
		if (ModManager.DLCShared && parent.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ScavengerElite)
		{
			carryRocks = Mathf.Min(carryRocks, 1);
		}
		lastInOffscreenDen = true;
		if (world.singleRoomWorld || world.offScreenDen.index == parent.pos.room || parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar || parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
		{
			InitGearUp();
		}
		missionAppropriateGear = true;
		dontMigrate = UnityEngine.Random.Range(400, 4800);
		longTermMigration = parent.pos;
		if (world.scavengersWorldAI == null)
		{
			world.AddWorldProcess(new ScavengersWorldAI(world));
		}
		world.scavengersWorldAI.AddScavenger(this);
		worldAI = world.scavengersWorldAI;
		if (world.game.IsArenaSession && parent.pos.room == world.offScreenDen.index)
		{
			freeze = UnityEngine.Random.Range(40, 400);
		}
	}

	public override void NewWorld(World newWorld)
	{
		base.NewWorld(newWorld);
		if (squad != null)
		{
			squad.RemoveMember(parent);
		}
		if (newWorld.scavengersWorldAI == null)
		{
			newWorld.AddWorldProcess(new ScavengersWorldAI(newWorld));
		}
		newWorld.scavengersWorldAI.AddScavenger(this);
		worldAI = newWorld.scavengersWorldAI;
		longTermMigration = parent.pos;
	}

	public override void AbstractBehavior(int time)
	{
		if (ModManager.MSC && parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
		{
			freeze = 40;
		}
		if (freeze > 0)
		{
			freeze -= time;
			return;
		}
		controlledMigrateTime--;
		if (parent.pos.room == base.MigrationDestination.room && dontMigrate > 0)
		{
			dontMigrate -= time;
		}
		if (squad != null && !squad.DoesScavengerWantToBeInSquad(this))
		{
			if (parent.world.game.devToolsActive)
			{
				Custom.Log("scav leave squad");
			}
			squad.RemoveMember(parent);
		}
		bool flag = ModManager.MSC && parent.controlled;
		if (parent.realizedCreature == null)
		{
			if (path.Count > 0)
			{
				FollowPath(time);
				return;
			}
			if (base.MigrationDestination != longTermMigration && (!flag || controlledMigrateTime > 0) && (squad == null || !squad.StationaryMission))
			{
				SetDestination(longTermMigration);
				if (!DoIHaveAPathToCoordinate(longTermMigration))
				{
					longTermMigration = base.destination;
				}
			}
			if (!flag && ((world.rainCycle.TimeUntilRain < 800 && !parent.nightCreature && !parent.ignoreCycle) || (parent.nightCreature && world.rainCycle.dayNightCounter < 600) || GoHome()))
			{
				if (!base.denPosition.HasValue || !parent.pos.CompareDisregardingTile(base.denPosition.Value))
				{
					GoToDen();
				}
				return;
			}
		}
		else if (!flag && base.denPosition.HasValue && !base.destination.CompareDisregardingTile(base.denPosition.Value) && GoHome())
		{
			SetDestination(base.denPosition.Value);
			return;
		}
		if (squad != null && !squad.StayIn)
		{
			squad.RemoveMember(parent);
		}
		if (parent.pos.room == world.offScreenDen.index)
		{
			InOffscreenDen();
		}
		else
		{
			if (flag)
			{
				return;
			}
			lastInOffscreenDen = false;
			if (GhostOutOfCurrentRoom && RoomGhostScary(base.MigrationDestination.room) > 0f)
			{
				Migrate(1f);
			}
			else if (squad != null && squad.HasAMission && squad.MissionRoom.HasValue && base.destination.room != squad.MissionRoom && worldAI.floodFiller.IsRoomAccessible(squad.MissionRoom.Value))
			{
				GoToRoom(squad.MissionRoom.Value);
			}
			else if (UnderSquadLeaderControl && (squad.leader.pos.room != unreachableSquadLeaderPos.room || squad.leader.pos.abstractNode != unreachableSquadLeaderPos.abstractNode) && squad.leader.pos.room != world.offScreenDen.index && squad.leader.pos.room != base.MigrationDestination.room && squad.leader.abstractAI.MigrationDestination.room != base.MigrationDestination.room && TimeInfluencedRandomRoll(1f - parent.personality.bravery, time))
			{
				unreachableSquadLeaderPos = squad.leader.pos.WashTileData();
				GoToRoom(squad.leader.pos.room);
			}
			else if ((squad == null || !squad.HasAMission) && (path.Count == 0 || parent.realizedCreature != null) && base.MigrationDestination.room == parent.pos.room)
			{
				float num = parent.personality.bravery * parent.personality.energy;
				if (UnderSquadLeaderControl)
				{
					num /= 3f;
				}
				if (squad != null && squad.leader == parent)
				{
					num = 0.8f;
				}
				if (dontMigrate < 1 && (squad == null || squad.leader.abstractAI.MigrationDestination.room == parent.pos.room) && TimeInfluencedRandomRoll(0.1f / Mathf.Lerp(100f, 2f, num), time))
				{
					Migrate(num);
				}
				else if (TimeInfluencedRandomRoll(parent.personality.nervous * 0.1f, time) && parent.realizedCreature == null)
				{
					RandomMoveWithinRoom();
				}
			}
		}
	}

	public void Migrate(float roaming)
	{
		if (squad != null && squad.HasAMission)
		{
			return;
		}
		bool flag = (UnityEngine.Random.value < 0.2f || squad == null) && (squad == null || squad.leader == parent);
		if (flag)
		{
			SetDestination(base.denPosition.Value);
		}
		else
		{
			RandomMoveToOtherRoom((int)Mathf.Lerp(30f, 600f, roaming));
		}
		if (base.MigrationDestination.room != parent.pos.room && squad != null && squad.leader == parent)
		{
			squad.CommonMovement(base.MigrationDestination.room, parent, !flag);
		}
		if (flag && squad != null)
		{
			if (squad.leader == parent)
			{
				squad.Dissolve();
			}
			else
			{
				squad.RemoveMember(parent);
			}
		}
		dontMigrate = UnityEngine.Random.Range(400, 4800);
	}

	public void InOffscreenDen()
	{
		if (!lastInOffscreenDen)
		{
			ReGearInDen();
			freeze = UnityEngine.Random.Range(300, 500);
			lastInOffscreenDen = true;
			return;
		}
		if (!missionAppropriateGear)
		{
			ReGearInDen();
		}
		if ((parent.state as HealthState).health < 0.5f || (world.rainCycle.TimeUntilRain < 800 && !parent.nightCreature && !parent.ignoreCycle) || (parent.nightCreature && world.rainCycle.dayNightCounter < 600))
		{
			freeze = 100;
		}
		else if (world.singleRoomWorld)
		{
			GoToRoom(0);
		}
		else if (UnderSquadLeaderControl)
		{
			if (squad.leader.abstractAI.MigrationDestination.room != world.offScreenDen.index)
			{
				GoToRoom(squad.leader.abstractAI.MigrationDestination.room);
			}
			else if (squad.leader.abstractAI.destination.room != world.offScreenDen.index)
			{
				GoToRoom(squad.leader.abstractAI.destination.room);
			}
		}
		else if (squad != null && squad.MissionRoom.HasValue && squad.missionType != ScavengerSquad.MissionID.None)
		{
			GoToRoom(squad.MissionRoom.Value);
		}
		else
		{
			TryAssembleSquad();
		}
	}

	public bool ReadyToJoinSquad()
	{
		if (freeze < 1)
		{
			return squad == null;
		}
		return false;
	}

	public void TryAssembleSquad()
	{
		if ((parent.nightCreature && world.rainCycle.dayNightCounter < 600) || (world.rainCycle.TimeUntilRain < 800 && !parent.nightCreature && !parent.ignoreCycle) || parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < parent.Room.creatures.Count; i++)
		{
			if (parent.Room.creatures[i].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && parent.Room.creatures[i] != parent && (parent.Room.creatures[i].abstractAI as ScavengerAbstractAI).ReadyToJoinSquad() && parent.CanTravelTogether(parent.Room.creatures[i]))
			{
				if (ScavengerSquad.LeaderScore(parent.Room.creatures[i]) > ScavengerSquad.LeaderScore(parent))
				{
					return;
				}
				num++;
			}
		}
		if (num < 2)
		{
			if (!world.singleRoomWorld)
			{
				return;
			}
			int num2 = 0;
			for (int j = 0; j < world.NumberOfRooms; j++)
			{
				for (int k = 0; k < world.GetAbstractRoom(world.firstRoomIndex + j).creatures.Count; k++)
				{
					if (world.GetAbstractRoom(world.firstRoomIndex + j).creatures[k].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && world.GetAbstractRoom(world.firstRoomIndex + j).creatures[k].state.alive && parent.CanTravelTogether(parent.Room.creatures[j]))
					{
						num2++;
					}
				}
			}
			if (num < num2)
			{
				return;
			}
		}
		WorldCoordinate coord = RandomDestinationRoom();
		if (!CanRoamThroughRoom(coord.room) || !worldAI.floodFiller.IsRoomAccessible(coord.room))
		{
			return;
		}
		for (int l = 0; l < world.GetAbstractRoom(coord).creatures.Count; l++)
		{
			if (world.GetAbstractRoom(coord).creatures[l].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && (world.GetAbstractRoom(coord).creatures[l].abstractAI as ScavengerAbstractAI).squad != null)
			{
				return;
			}
		}
		if (squad != null)
		{
			squad.RemoveMember(parent);
		}
		squad = new ScavengerSquad(parent);
		int maxExclusive = 7;
		if (ModManager.MMF && world.region != null)
		{
			if (world.region.name == "SH" || world.region.name == "CC" || (ModManager.MSC && world.region.name == "LC"))
			{
				maxExclusive = 5;
			}
			else if (world.region.name == "SU" || world.region.name == "UW" || world.region.name == "SI")
			{
				maxExclusive = 3;
			}
		}
		int num3 = Math.Min(num, UnityEngine.Random.Range(2, maxExclusive));
		for (int m = 0; m < parent.Room.creatures.Count; m++)
		{
			if (num3 <= 0)
			{
				break;
			}
			if (parent.Room.creatures[m].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && parent.Room.creatures[m] != parent && (parent.Room.creatures[m].abstractAI as ScavengerAbstractAI).ReadyToJoinSquad() && parent.CanTravelTogether(parent.Room.creatures[m]))
			{
				squad.AddMember(parent.Room.creatures[m]);
				num3--;
			}
		}
		if (!squad.StayIn)
		{
			squad = null;
		}
		if (squad != null)
		{
			ScavengersWorldAI.Outpost outpost = worldAI.OutPostInNeedOfGuard();
			if (outpost != null && RoomGhostScary(outpost.room) == 0f)
			{
				Custom.Log("Squad going to guard outpost!");
				if (ModManager.Watcher)
				{
					bool flag = false;
					bool flag2 = false;
					foreach (AbstractCreature member in squad.members)
					{
						if (member.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar)
						{
							flag = true;
						}
						if (member.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
						{
							flag2 = true;
						}
					}
					for (int n = 0; n < parent.Room.creatures.Count; n++)
					{
						if (parent.Room.creatures[n].creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && parent.Room.creatures[n] != parent && (parent.Room.creatures[n].abstractAI as ScavengerAbstractAI).ReadyToJoinSquad() && parent.CanTravelTogether(parent.Room.creatures[n]))
						{
							if (!flag && parent.Room.creatures[n].creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar)
							{
								squad.AddMember(parent.Room.creatures[n]);
								flag = true;
							}
							if (!flag2 && parent.Room.creatures[n].creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
							{
								squad.AddMember(parent.Room.creatures[n]);
								flag2 = true;
							}
							if (flag && flag2)
							{
								break;
							}
						}
					}
				}
				outpost.guardSquad = squad;
				squad.guardOutpost = outpost;
				squad.missionType = ScavengerSquad.MissionID.GuardOutpost;
				for (int num4 = 0; num4 < world.GetAbstractRoom(outpost.room).nodes.Length; num4++)
				{
					if (world.GetAbstractRoom(outpost.room).nodes[num4].type == AbstractRoomNode.Type.RegionTransportation)
					{
						coord = new WorldCoordinate(outpost.room, -1, -1, num4);
						break;
					}
				}
			}
		}
		SetDestination(coord);
		longTermMigration = coord;
		dontMigrate = UnityEngine.Random.Range(400, 4800);
		if (squad == null || squad.leader != parent)
		{
			return;
		}
		squad.CommonMovement(coord.room, parent, onlyInRoom: false);
		for (int num5 = 0; num5 < squad.members.Count; num5++)
		{
			if (squad.members[num5] != parent)
			{
				(squad.members[num5].abstractAI as ScavengerAbstractAI).freeze = num5 * 10 + UnityEngine.Random.Range(0, 10);
				(squad.members[num5].abstractAI as ScavengerAbstractAI).dontMigrate = UnityEngine.Random.Range(400, 4800);
			}
		}
	}

	public void GoToRoom(int dstRoom)
	{
		if (base.MigrationDestination.room == dstRoom || parent.pos.room == dstRoom || RoomGhostScary(dstRoom) > 0f || (ModManager.MSC && (parent.world.GetAbstractRoom(dstRoom).shelter || parent.world.GetAbstractRoom(dstRoom).gate)))
		{
			return;
		}
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		List<WorldCoordinate> list2 = new List<WorldCoordinate>();
		for (int i = 0; i < parent.world.GetAbstractRoom(dstRoom).nodes.Length; i++)
		{
			if (parent.world.GetAbstractRoom(dstRoom).nodes[i].type.Index == -1 || !parent.creatureTemplate.mappedNodeTypes[parent.world.GetAbstractRoom(dstRoom).nodes[i].type.Index])
			{
				continue;
			}
			bool flag = true;
			if (parent.world.GetAbstractRoom(dstRoom).nodes[i].type != AbstractRoomNode.Type.RegionTransportation)
			{
				flag = false;
			}
			for (int j = 0; j < parent.world.GetAbstractRoom(dstRoom).nodes.Length; j++)
			{
				if (flag)
				{
					break;
				}
				if (parent.world.GetAbstractRoom(dstRoom).nodes[j].type == AbstractRoomNode.Type.RegionTransportation && parent.world.GetAbstractRoom(dstRoom).ConnectionAndBackPossible(j, i, parent.creatureTemplate))
				{
					flag = true;
				}
			}
			if (flag)
			{
				list.Add(new WorldCoordinate(dstRoom, -1, -1, i));
			}
			if (worldAI.floodFiller.IsNodeAccessible(dstRoom, i))
			{
				list2.Add(new WorldCoordinate(dstRoom, -1, -1, i));
			}
		}
		if (list.Count > 0)
		{
			SetDestination(list[UnityEngine.Random.Range(0, list.Count)]);
			longTermMigration = base.destination;
		}
		else if (list2.Count > 0)
		{
			SetDestination(list2[UnityEngine.Random.Range(0, list2.Count)]);
			longTermMigration = base.destination;
		}
		else
		{
			Custom.Log("scav can't reach room", world.IsRoomInRegion(dstRoom) ? world.GetAbstractRoom(dstRoom).name : dstRoom.ToString());
		}
		if (dstRoom == world.offScreenDen.index && base.MigrationDestination.room == world.offScreenDen.index && longTermMigration.room == world.offScreenDen.index)
		{
			Custom.Log("Go home freeze");
			freeze = UnityEngine.Random.Range(300, 500);
		}
	}

	public override bool CanRoamThroughRoom(int room)
	{
		return RoomGhostScary(room) <= RoomGhostScary(parent.pos.room);
	}

	public float RoomGhostScary(int testRoom)
	{
		if (ModManager.MSC && world.game.IsStorySession && world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			if (world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
			{
				for (int i = 0; i < world.game.Players.Count; i++)
				{
					if (world.game.Players[i].pos.room == testRoom)
					{
						return 1f;
					}
				}
			}
			return 0f;
		}
		if (Shyness == 0f || testRoom == world.offScreenDen.index)
		{
			return 0f;
		}
		float num = 0f;
		for (int j = 0; j < world.game.Players.Count; j++)
		{
			if (world.game.Players[j].pos.room == testRoom)
			{
				num += 0.5f * Shyness;
			}
		}
		if (Shyness > 0.33f)
		{
			for (int k = 0; k < world.GetAbstractRoom(testRoom).connections.Length; k++)
			{
				if (world.GetAbstractRoom(testRoom).connections[k] <= -1)
				{
					continue;
				}
				for (int l = 0; l < world.game.Players.Count; l++)
				{
					if (world.game.Players[l].pos.room == world.GetAbstractRoom(testRoom).connections[k])
					{
						num += 0.25f * Mathf.InverseLerp(0.33f, 1f, Shyness);
					}
				}
				if (!(Shyness > 0.66f))
				{
					continue;
				}
				AbstractRoom abstractRoom = world.GetAbstractRoom(world.GetAbstractRoom(testRoom).connections[k]);
				for (int m = 0; m < abstractRoom.connections.Length; m++)
				{
					if (abstractRoom.connections[m] <= -1)
					{
						continue;
					}
					for (int n = 0; n < world.game.Players.Count; n++)
					{
						if (world.game.Players[n].pos.room == abstractRoom.connections[m])
						{
							num += 0.125f * Mathf.InverseLerp(0.66f, 1f, Shyness);
						}
					}
				}
			}
		}
		return num;
	}

	public float CostAddOfNode(WorldCoordinate coordinate)
	{
		return RoomGhostScary(coordinate.room) * 10000f;
	}

	public override void Die()
	{
		if (squad != null)
		{
			squad.RemoveMember(parent);
		}
		base.Die();
	}

	public override bool DoIwantToDropThisItemInDen(AbstractPhysicalObject item)
	{
		return false;
	}

	public bool GoHome()
	{
		if (parent.pos.room == world.offScreenDen.index)
		{
			return false;
		}
		if (bringPearlHome)
		{
			return true;
		}
		if (!missionAppropriateGear)
		{
			return true;
		}
		return false;
	}

	public WorldCoordinate RandomDestinationRoom()
	{
		List<CoordinateAndFloat> list = new List<CoordinateAndFloat>();
		float num = 0f;
		for (int i = 0; i < world.NumberOfRooms; i++)
		{
			if ((ModManager.MSC && (world.GetAbstractRoom(i + world.firstRoomIndex).shelter || world.GetAbstractRoom(i + world.firstRoomIndex).gate)) || !(world.GetAbstractRoom(i + world.firstRoomIndex).AttractionForCreature(parent) != AbstractRoom.CreatureRoomAttraction.Forbidden) || !worldAI.floodFiller.IsRoomAccessible(i + world.firstRoomIndex) || (ModManager.MSC && world.game.globalRain.DrainWorldPositionFlooded(new WorldCoordinate(i + world.firstRoomIndex, 0, 11, -1))))
			{
				continue;
			}
			float num2 = world.GetAbstractRoom(i + world.firstRoomIndex).SizeDependentAttractionValueForCreature(parent);
			int num3 = 0;
			for (int j = 0; j < world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length; j++)
			{
				if (world.GetAbstractRoom(i + world.firstRoomIndex).nodes[j].type.Index != -1 && parent.creatureTemplate.mappedNodeTypes[world.GetAbstractRoom(i + world.firstRoomIndex).nodes[j].type.Index])
				{
					num3++;
				}
			}
			for (int k = 0; k < world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length; k++)
			{
				if (world.GetAbstractRoom(i + world.firstRoomIndex).nodes[k].type.Index != -1 && parent.creatureTemplate.mappedNodeTypes[world.GetAbstractRoom(i + world.firstRoomIndex).nodes[k].type.Index])
				{
					list.Add(new CoordinateAndFloat(new WorldCoordinate(i + world.firstRoomIndex, -1, -1, k), num2 / (float)num3));
					num += num2 / (float)num3;
				}
			}
		}
		float num4 = UnityEngine.Random.value * num;
		for (int l = 0; l < list.Count; l++)
		{
			if (num4 < list[l].flt)
			{
				return list[l].coord;
			}
			num4 -= list[l].flt;
		}
		Custom.Log("scav weighted random failure");
		return new WorldCoordinate(world.offScreenDen.index, -1, -1, 0);
	}

	public void ControlledLongTermDestination()
	{
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		WorldCoordinate[] regionAccessNodes = world.regionAccessNodes;
		for (int i = 0; i < regionAccessNodes.Length; i++)
		{
			WorldCoordinate item = regionAccessNodes[i];
			if (item.room != parent.Room.index)
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			WorldCoordinate worldCoordinate = list[Mathf.Min(list.Count - 1, UnityEngine.Random.Range(0, list.Count))];
			controlledMigrateTime = 80;
			SetDestination(worldCoordinate);
			longTermMigration = worldCoordinate;
			dontMigrate = 400;
		}
	}

	public void InitGearUp()
	{
		int num = 3;
		int num2 = 40;
		float num3 = 1f;
		if (world.game.IsStorySession)
		{
			num2 = (world.game.session as StoryGameSession).saveState.cycleNumber;
			if (world.game.StoryCharacter == SlugcatStats.Name.Yellow)
			{
				num2 = Mathf.FloorToInt((float)num2 * 0.75f);
				num3 = 0.5f;
			}
			else if (world.game.StoryCharacter == SlugcatStats.Name.Red)
			{
				num2 += 60;
				num3 = 1.5f;
			}
		}
		bool flag = false;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		if (ModManager.ChallengeModule && world.game.IsArenaSession && world.game.GetArenaGameSession.chMeta != null && world.game.GetArenaGameSession.chMeta.seed >= 0)
		{
			UnityEngine.Random.InitState(parent.ID.RandomSeed);
			flag = true;
		}
		bool flag2 = false;
		if (!ModManager.DLCShared || (world.game.IsStorySession && SlugcatStats.AtOrAfterTimeline(world.game.TimelinePoint, SlugcatStats.Timeline.Yellow)))
		{
			flag2 = true;
		}
		if (ModManager.Watcher && (parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar || parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerDisciple) && num > 0)
		{
			AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(world, WatcherEnums.AbstractObjectType.Boomerang, null, parent.pos, world.game.GetNewID());
			world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject, num, carry: true);
			num--;
		}
		int num4 = Custom.IntClamp((int)(Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, Mathf.Pow(parent.personality.dominance, 3f - num3))) * (3.5f + num3)), 0, 4);
		if (ModManager.DLCShared && parent.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ScavengerElite && num4 < 1)
		{
			num4 = 1;
		}
		if ((ModManager.Watcher && parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar) || parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
		{
			num4 = 1;
		}
		if (num4 > 0)
		{
			for (int i = 0; i < num4; i++)
			{
				AbstractPhysicalObject abstractPhysicalObject2 = ((!ModManager.DLCShared || !(parent.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ScavengerElite)) ? new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), IsSpearExplosive(num2)) : ((!(UnityEngine.Random.value < 0.5f || flag2)) ? new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), explosive: false, electric: true) : new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), explosive: true)));
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject2);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject2, num, carry: true);
				num--;
			}
		}
		string text = "";
		if (world.game.IsStorySession && world.region != null && !world.singleRoomWorld)
		{
			text = world.region.name;
		}
		if (num >= 0)
		{
			Region region = world.region;
			if ((region != null && region.regionParams.locustRegion && UnityEngine.Random.value < 0.6f) || (ModManager.Watcher && world.game.IsStorySession && text == "WARC" && UnityEngine.Random.value < 0.2f))
			{
				AbstractPhysicalObject abstractPhysicalObject3 = new GraffitiBomb.AbstractGraffitiBomb(world, null, parent.pos, world.game.GetNewID(), -1, -1, null);
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject3);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject3, num, carry: true);
				num--;
			}
			if (ModManager.Watcher && world.game.IsStorySession && text == "WARA" && UnityEngine.Random.value < 0.2f)
			{
				AbstractPhysicalObject abstractPhysicalObject4 = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, parent.pos, world.game.GetNewID(), -1, -1, null);
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject4);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject4, num, carry: true);
				num--;
			}
		}
		if (num >= 0 && UnityEngine.Random.value < 0.6f)
		{
			if (world.singleRoomWorld)
			{
				goto IL_05e9;
			}
			if (!world.game.IsStorySession || !SlugcatStats.AtOrAfterTimeline(world.game.TimelinePoint, SlugcatStats.Timeline.Saint))
			{
				switch (text)
				{
				case "SB":
					if (UnityEngine.Random.value < 0.7f)
					{
						break;
					}
					goto default;
				default:
					if (text == "WARC" && UnityEngine.Random.value < 0.2f)
					{
						break;
					}
					goto IL_05e9;
				case "SH":
				case "WARG":
					break;
				}
			}
			goto IL_060c;
		}
		goto IL_066b;
		IL_066b:
		if (ModManager.DLCShared)
		{
			if (num >= 0)
			{
				switch (text)
				{
				case "SB":
				case "SL":
				case "MS":
					if (UnityEngine.Random.value < 0.27f)
					{
						AbstractConsumable abstractConsumable = new AbstractConsumable(world, DLCSharedEnums.AbstractObjectType.GlowWeed, null, parent.pos, world.game.GetNewID(), -1, -1, null);
						abstractConsumable.isConsumed = true;
						world.GetAbstractRoom(parent.pos).AddEntity(abstractConsumable);
						new AbstractPhysicalObject.CreatureGripStick(parent, abstractConsumable, num, carry: true);
						num--;
					}
					break;
				}
			}
			if (num >= 0 && (text == "LF" || text == "OE") && UnityEngine.Random.value < 0.27f)
			{
				AbstractConsumable abstractConsumable2 = new AbstractConsumable(world, DLCSharedEnums.AbstractObjectType.GooieDuck, null, parent.pos, world.game.GetNewID(), -1, -1, null);
				abstractConsumable2.isConsumed = true;
				world.GetAbstractRoom(parent.pos).AddEntity(abstractConsumable2);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractConsumable2, num, carry: true);
				num--;
			}
		}
		if (num >= 0 && Mathf.Pow(UnityEngine.Random.value, num3) < Mathf.InverseLerp(10f, 60f, num2))
		{
			int num5 = Custom.IntClamp((int)(Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, Mathf.Pow(parent.personality.dominance, 2f) * Mathf.InverseLerp(10f, 60f, num2))) * 2.5f), 0, 2);
			for (int j = 0; j < num5; j++)
			{
				AbstractPhysicalObject abstractPhysicalObject5 = ((!ModManager.DLCShared || !(parent.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ScavengerElite) || flag2 || ((!world.game.IsArenaSession || world.game.GetArenaGameSession.chMeta != null) && (!(UnityEngine.Random.value <= parent.personality.aggression / 5f) || (!(parent.personality.dominance > 0.6f) && !parent.nightCreature)))) ? new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, parent.pos, world.game.GetNewID()) : new AbstractPhysicalObject(world, DLCSharedEnums.AbstractObjectType.SingularityBomb, null, parent.pos, world.game.GetNewID()));
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject5);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject5, num, carry: true);
				num--;
				if (num < 0)
				{
					break;
				}
			}
		}
		if (num >= 0 && UnityEngine.Random.value < 0.08f)
		{
			AbstractPhysicalObject abstractPhysicalObject6 = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject6);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject6, num, carry: true);
			num--;
		}
		if (num >= 0 && Mathf.Pow(UnityEngine.Random.value, num3) < (world.game.IsStorySession ? Mathf.InverseLerp(40f, 110f, num2) : 0.8f) && UnityEngine.Random.value < 1f / Mathf.Lerp(12f, 3f, parent.personality.dominance))
		{
			SporePlant.AbstractSporePlant abstractSporePlant = new SporePlant.AbstractSporePlant(world, null, parent.pos, world.game.GetNewID(), -1, -1, null, used: false, pacified: true);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractSporePlant, num, carry: true);
			world.GetAbstractRoom(parent.pos).AddEntity(abstractSporePlant);
			num--;
		}
		if (num >= 0)
		{
			int num6 = UnityEngine.Random.Range(0, carryRocks + 1);
			for (int k = 0; k < num6; k++)
			{
				AbstractPhysicalObject abstractPhysicalObject7 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Rock, null, parent.pos, world.game.GetNewID());
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject7);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject7, num, carry: true);
				num--;
				if (num < 0)
				{
					break;
				}
			}
		}
		if (ModManager.DLCShared && parent.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ScavengerElite)
		{
			while (num >= 0)
			{
				AbstractPhysicalObject abstractPhysicalObject8 = new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), IsSpearExplosive(num2));
				world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject8);
				new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject8, num, carry: true);
				num--;
			}
		}
		if (flag)
		{
			UnityEngine.Random.state = state;
		}
		return;
		IL_05e9:
		if (ModManager.DLCShared && parent.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ScavengerElite)
		{
			goto IL_060c;
		}
		goto IL_066b;
		IL_060c:
		AbstractPhysicalObject abstractPhysicalObject9 = new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, parent.pos, world.game.GetNewID());
		world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject9);
		new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject9, num, carry: true);
		num--;
		goto IL_066b;
	}

	public bool IsSpearExplosive(int cycleNum)
	{
		float num = parent.personality.dominance * ((parent.personality.aggression + parent.personality.bravery) / 2f) * 0.75f;
		if (world.game.IsStorySession && world.region != null && !world.singleRoomWorld)
		{
			if (world.region.name == "SU")
			{
				num = Mathf.Pow(num, 2f) * 0.5f;
			}
			else if (world.region.name == "HI" || world.region.name == "DS")
			{
				num *= 0.5f;
			}
			num = Mathf.Lerp(num, 1f, Mathf.InverseLerp(-0.5f, -1f, world.game.session.creatureCommunities.LikeOfPlayer(CreatureCommunities.CommunityID.Scavengers, world.RegionNumber, 0)) * 0.15f);
			num *= Mathf.InverseLerp(10f, 30f, cycleNum);
			if (SlugcatStats.AtOrBeforeTimeline(world.game.TimelinePoint, SlugcatStats.Timeline.Red))
			{
				num = Mathf.Lerp(num, 1f, 0.25f * UnityEngine.Random.value);
			}
		}
		else
		{
			num = Mathf.Lerp(num, 1f, 0.15f);
		}
		return UnityEngine.Random.value < num;
	}

	public void ReGearInDen()
	{
		Custom.Log("regear in den");
		bringPearlHome = false;
		int num = 0;
		bool[] array = new bool[4];
		for (int num2 = parent.stuckObjects.Count - 1; num2 >= 0; num2--)
		{
			if (parent.stuckObjects[num2] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[num2].A == parent)
			{
				if (parent.stuckObjects[num2].B.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
				{
					Custom.Log($"scav dropping {parent.stuckObjects[num2].B.type} in den");
					DropAndDestroy(parent.stuckObjects[num2]);
				}
				else
				{
					int grasp = (parent.stuckObjects[num2] as AbstractPhysicalObject.CreatureGripStick).grasp;
					if (grasp >= 0 && grasp < 4)
					{
						array[grasp] = true;
						num++;
					}
					else
					{
						DropAndDestroy(parent.stuckObjects[num2]);
					}
				}
			}
		}
		if (squad == null)
		{
			return;
		}
		if (squad.missionType == ScavengerSquad.MissionID.Trade)
		{
			for (int num3 = parent.stuckObjects.Count - 1; num3 >= 0; num3--)
			{
				if (parent.stuckObjects[num3] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[num3].A == parent && parent.stuckObjects[num3].B.type == AbstractPhysicalObject.AbstractObjectType.Rock)
				{
					DropAndDestroy(parent.stuckObjects[num3]);
				}
			}
		}
		if (squad.missionType != ScavengerSquad.MissionID.Trade)
		{
			UpdateMissionAppropriateGear();
			if (missionAppropriateGear)
			{
				return;
			}
		}
		if (num == 4)
		{
			for (int num4 = parent.stuckObjects.Count - 1; num4 >= 0; num4--)
			{
				if (parent.stuckObjects[num4] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[num4].A == parent)
				{
					int grasp2 = (parent.stuckObjects[num4] as AbstractPhysicalObject.CreatureGripStick).grasp;
					DropAndDestroy(parent.stuckObjects[num4]);
					num--;
					array[grasp2] = false;
					Custom.Log("make grasp free:", grasp2.ToString());
					break;
				}
			}
		}
		int num5 = -1;
		for (int i = 0; i < 4; i++)
		{
			if (!array[i])
			{
				num5 = i;
				break;
			}
		}
		if (num5 == -1)
		{
			return;
		}
		if (squad.missionType == ScavengerSquad.MissionID.GuardOutpost || squad.missionType == ScavengerSquad.MissionID.HuntCreature || squad.missionType == ScavengerSquad.MissionID.ProtectCreature)
		{
			AbstractPhysicalObject abstractPhysicalObject = new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), IsSpearExplosive(world.game.IsStorySession ? (world.game.GetStorySession.saveState.cycleNumber + ((world.game.StoryCharacter == SlugcatStats.Name.Red) ? 60 : 0)) : 0));
			world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject, num5, carry: true);
			array[num5] = true;
		}
		else if (squad.missionType == ScavengerSquad.MissionID.Trade)
		{
			AbstractPhysicalObject abstractPhysicalObject2 = TradeItem(main: true);
			world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject2);
			new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject2, num5, carry: true);
			array[num5] = true;
			for (int j = 0; j < array.Length; j++)
			{
				if (!array[j])
				{
					abstractPhysicalObject2 = TradeItem(main: false);
					world.GetAbstractRoom(parent.pos).AddEntity(abstractPhysicalObject2);
					new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject2, j, carry: true);
					array[j] = true;
				}
			}
			for (int k = 0; k < parent.stuckObjects.Count; k++)
			{
				if (parent.stuckObjects[k] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[k].A == parent && parent.stuckObjects[k].B.type == AbstractPhysicalObject.AbstractObjectType.Spear)
				{
					(parent.stuckObjects[k].B as AbstractSpear).explosive = true;
				}
			}
		}
		missionAppropriateGear = true;
	}

	public void DropAndDestroy(AbstractPhysicalObject.AbstractObjectStick stick)
	{
		stick.Deactivate();
		stick.B.Destroy();
	}

	public AbstractPhysicalObject TradeItem(bool main)
	{
		string text = "";
		if (world.game.IsStorySession && world.region != null && !world.singleRoomWorld)
		{
			text = world.region.name;
		}
		bool flag = text == "SH" || text == "SB";
		bool flag2 = ModManager.Watcher && text == "WBLA";
		bool flag3 = ModManager.Watcher && text == "WSKD";
		if (ModManager.Watcher && text == "WARC")
		{
			if (UnityEngine.Random.value > 0.3f)
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
		}
		if (main)
		{
			if (flag2)
			{
				return new GraffitiBomb.AbstractGraffitiBomb(world, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (flag)
			{
				return new AbstractPhysicalObject(world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, parent.pos, world.game.GetNewID());
			}
			if (flag3)
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
		}
		else
		{
			if (UnityEngine.Random.value < (flag ? 0.5f : 0.05f))
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (UnityEngine.Random.value < ((text == "LF" || flag2) ? 0f : 0.05f))
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.PuffBall, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (UnityEngine.Random.value < 0.2f)
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (UnityEngine.Random.value < 0.1f)
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.Mushroom, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
			if (UnityEngine.Random.value < 0.02f && world.game.StoryCharacter != SlugcatStats.Name.Red)
			{
				return new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, parent.pos, world.game.GetNewID(), -1, -1, null);
			}
		}
		return new AbstractSpear(world, null, parent.pos, world.game.GetNewID(), explosive: true);
	}

	public void UpdateMissionAppropriateGear()
	{
		missionAppropriateGear = false;
		if (squad == null || squad.missionType == ScavengerSquad.MissionID.None)
		{
			missionAppropriateGear = true;
			return;
		}
		if (parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar || parent.creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
		{
			for (int i = 0; i < parent.stuckObjects.Count; i++)
			{
				if (parent.stuckObjects[i] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[i].A == parent && parent.stuckObjects[i].B.type == WatcherEnums.AbstractObjectType.Boomerang)
				{
					missionAppropriateGear = true;
					return;
				}
			}
		}
		if (squad.missionType == ScavengerSquad.MissionID.GuardOutpost || squad.missionType == ScavengerSquad.MissionID.HuntCreature || squad.missionType == ScavengerSquad.MissionID.ProtectCreature)
		{
			for (int j = 0; j < parent.stuckObjects.Count; j++)
			{
				if (parent.stuckObjects[j] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[j].A == parent && parent.stuckObjects[j].B.type == AbstractPhysicalObject.AbstractObjectType.Spear)
				{
					missionAppropriateGear = true;
					break;
				}
			}
		}
		else
		{
			if (!(squad.missionType == ScavengerSquad.MissionID.Trade))
			{
				return;
			}
			string text = "";
			if (world.game.IsStorySession && world.region != null && !world.singleRoomWorld)
			{
				text = world.region.name;
			}
			switch (text)
			{
			case "SH":
			case "SB":
			{
				for (int k = 0; k < parent.stuckObjects.Count; k++)
				{
					if (parent.stuckObjects[k] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[k].A == parent && parent.stuckObjects[k].B.type == AbstractPhysicalObject.AbstractObjectType.Lantern)
					{
						missionAppropriateGear = true;
						break;
					}
				}
				return;
			}
			case "WBLA":
			{
				for (int m = 0; m < parent.stuckObjects.Count; m++)
				{
					if (parent.stuckObjects[m] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[m].A == parent && parent.stuckObjects[m].B.type == AbstractPhysicalObject.AbstractObjectType.GraffitiBomb)
					{
						missionAppropriateGear = true;
						break;
					}
				}
				return;
			}
			case "WSKD":
			{
				for (int n = 0; n < parent.stuckObjects.Count; n++)
				{
					if (parent.stuckObjects[n] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[n].A == parent && parent.stuckObjects[n].B.type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower)
					{
						missionAppropriateGear = true;
						break;
					}
				}
				return;
			}
			case "WARC":
			{
				for (int l = 0; l < parent.stuckObjects.Count; l++)
				{
					if (parent.stuckObjects[l] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[l].A == parent && (parent.stuckObjects[l].B.type == AbstractPhysicalObject.AbstractObjectType.Lantern || parent.stuckObjects[l].B.type == AbstractPhysicalObject.AbstractObjectType.GraffitiBomb))
					{
						missionAppropriateGear = true;
						break;
					}
				}
				return;
			}
			}
			for (int num = 0; num < parent.stuckObjects.Count; num++)
			{
				if (parent.stuckObjects[num] is AbstractPhysicalObject.CreatureGripStick && parent.stuckObjects[num].A == parent && parent.stuckObjects[num].B.type == AbstractPhysicalObject.AbstractObjectType.Spear && (parent.stuckObjects[num].B as AbstractSpear).explosive)
				{
					missionAppropriateGear = true;
					break;
				}
			}
		}
	}
}
