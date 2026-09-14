using System.Collections.Generic;
using System.Globalization;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class AbstractCreature : AbstractPhysicalObject
{
	public struct Personality
	{
		public float energy;

		public float bravery;

		public float sympathy;

		public float dominance;

		public float nervous;

		public float aggression;

		public Personality(EntityID ID)
		{
			Random.State state = Random.state;
			Random.InitState(ID.RandomSeed);
			sympathy = Random.value;
			energy = Random.value;
			bravery = Random.value;
			sympathy = Custom.PushFromHalf(sympathy, 1.5f);
			energy = Custom.PushFromHalf(energy, 1.5f);
			bravery = Custom.PushFromHalf(bravery, 1.5f);
			nervous = Mathf.Lerp(Random.value, Mathf.Lerp(energy, 1f - bravery, 0.5f), Mathf.Pow(Random.value, 0.25f));
			aggression = Mathf.Lerp(Random.value, (energy + bravery) / 2f * (1f - sympathy), Mathf.Pow(Random.value, 0.25f));
			dominance = Mathf.Lerp(Random.value, (energy + bravery + aggression) / 3f, Mathf.Pow(Random.value, 0.25f));
			nervous = Custom.PushFromHalf(nervous, 2.5f);
			aggression = Custom.PushFromHalf(aggression, 2.5f);
			Random.state = state;
		}
	}

	public CreatureTemplate creatureTemplate;

	public CreatureState state;

	public AbstractCreatureAI abstractAI;

	public int distanceToMyNode;

	public int remainInDenCounter;

	public WorldCoordinate spawnDen = new WorldCoordinate(-1, -1, -1, -1);

	public string spawnData;

	public bool hadInvalidSpawn;

	public List<string> unrecognizedFlags = new List<string>();

	public bool superSizeMe;

	public bool preCycle;

	public int RemovedKarma;

	public bool controlled;

	public bool voidCreature;

	public bool saveCreature;

	public float Hypothermia;

	public bool HypothermiaImmune;

	public bool Winterized;

	public bool ignoreCycle;

	public bool tentacleImmune;

	public bool nightCreature;

	public bool lavaImmune;

	public bool rippleCreature;

	public string namedAttraction;

	public Personality personality;

	public Creature realizedCreature
	{
		get
		{
			return realizedObject as Creature;
		}
		set
		{
			realizedObject = value;
		}
	}

	public bool PacifiedBecauseCarried
	{
		get
		{
			for (int i = 0; i < stuckObjects.Count; i++)
			{
				if (stuckObjects[i] is CreatureGripStick && stuckObjects[i].B == this == (stuckObjects[i] as CreatureGripStick).carry)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool Quantify
	{
		get
		{
			if (!creatureTemplate.quantified)
			{
				return false;
			}
			if (!state.alive)
			{
				return false;
			}
			if (stuckObjects.Count == 0)
			{
				return false;
			}
			return true;
		}
	}

	public int karmicPotential
	{
		get
		{
			if (realizedCreature != null && realizedCreature is Scavenger && (realizedCreature as Scavenger).King)
			{
				return Mathf.Clamp(5 - RemovedKarma, 0, 5);
			}
			if (ModManager.MSC && global::MoreSlugcats.MoreSlugcats.cfgArtificerCorpseMaxKarma.Value)
			{
				return Mathf.Clamp(5 - RemovedKarma, 0, 5);
			}
			if (realizedCreature != null && realizedCreature is Scavenger && (realizedCreature as Scavenger).Elite)
			{
				return Mathf.Clamp(1 + Mathf.Min(4, (int)Mathf.Floor(personality.bravery * 2f + personality.energy * 2f + personality.sympathy * 2f + personality.dominance * 2f)) - RemovedKarma, 0, 5);
			}
			return Mathf.Clamp(1 + (int)Mathf.Floor(personality.bravery + personality.energy + personality.sympathy + personality.dominance) - RemovedKarma, 0, 5);
		}
	}

	public AbstractRoomNode.Type GetNodeType => base.Room.nodes[pos.abstractNode].type;

	public AbstractCreature(World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
		: base(world, AbstractObjectType.Creature, realizedCreature, pos, ID)
	{
		this.creatureTemplate = creatureTemplate;
		personality = new Personality(ID);
		namedAttraction = null;
		remainInDenCounter = -1;
		if (world == null)
		{
			return;
		}
		if (!(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Slugcat))
		{
			if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate)
			{
				state = new LizardState(this);
			}
			else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Fly || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Leech || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Spider)
			{
				state = new NoHealthState(this);
			}
			else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.GarbageWorm)
			{
				remainInDenCounter = 0;
				GarbageWormAI.MoveAbstractCreatureToGarbage(this, base.Room);
				state = new GarbageWormState(this, Mathf.Lerp(0.35f, 1f, Mathf.Pow(world.game.SeededRandom(ID.RandomSeed), 0.5f)));
			}
			else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LanternMouse)
			{
				state = new MouseState(this);
			}
			else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Vulture)
			{
				state = new Vulture.VultureState(this);
			}
			else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.DaddyLongLegs)
			{
				state = new DaddyLongLegs.DaddyState(this);
			}
			else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.VultureGrub)
			{
				state = new VultureGrub.VultureGrubState(this);
			}
			else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigNeedleWorm || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.SmallNeedleWorm)
			{
				state = new NeedleWormAbstractAI.NeedleWormState(this);
			}
			else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centipede)
			{
				if (creatureTemplate.type == CreatureTemplate.Type.SmallCentipede)
				{
					state = new HealthState(this);
				}
				else
				{
					state = new Centipede.CentipedeState(this);
				}
			}
			else
			{
				state = new HealthState(this);
			}
		}
		if (creatureTemplate.AI)
		{
			if (creatureTemplate.type == CreatureTemplate.Type.Vulture || creatureTemplate.type == CreatureTemplate.Type.KingVulture)
			{
				abstractAI = new VultureAbstractAI(world, this);
			}
			else if (creatureTemplate.type == CreatureTemplate.Type.CicadaA || creatureTemplate.type == CreatureTemplate.Type.CicadaB)
			{
				abstractAI = new CicadaAbstractAI(world, this);
			}
			else if (creatureTemplate.type == CreatureTemplate.Type.BigEel)
			{
				abstractAI = new BigEelAbstractAI(world, this);
			}
			else if (creatureTemplate.type == CreatureTemplate.Type.Deer)
			{
				abstractAI = new DeerAbstractAI(world, this);
			}
			else if (creatureTemplate.type == CreatureTemplate.Type.MirosBird)
			{
				abstractAI = new MirosBirdAbstractAI(world, this);
			}
			else if (creatureTemplate.type == CreatureTemplate.Type.Scavenger)
			{
				abstractAI = new ScavengerAbstractAI(world, this);
			}
			else if (creatureTemplate.type == CreatureTemplate.Type.Overseer)
			{
				abstractAI = new OverseerAbstractAI(world, this);
			}
			else if (creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm || creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm)
			{
				abstractAI = new NeedleWormAbstractAI(world, this);
			}
			else if (creatureTemplate.type == CreatureTemplate.Type.DropBug)
			{
				abstractAI = new DropBugAbstractAI(world, this);
			}
			else
			{
				abstractAI = new AbstractCreatureAI(world, this);
			}
		}
		saveCreature = true;
		if (ModManager.DLCShared)
		{
			MSCStateAI();
		}
		if (ModManager.Watcher)
		{
			WatcherStateAI();
		}
		if (base.Room == null)
		{
			Custom.LogWarning($"{creatureTemplate.type} HAD INVALID ROOM!");
			hadInvalidSpawn = true;
			pos.abstractNode = -1;
		}
		else
		{
			if (pos.abstractNode >= base.Room.nodes.Length)
			{
				Custom.LogWarning($"{creatureTemplate.type} HAD INVALID NODE! SETTING TO -1 -- {pos} -- room length: {base.Room.nodes.Length}");
				hadInvalidSpawn = true;
				pos.abstractNode = -1;
			}
			if (pos.abstractNode > -1 && pos.abstractNode < base.Room.nodes.Length && base.Room.nodes[pos.abstractNode].type == AbstractRoomNode.Type.Den && !pos.TileDefined)
			{
				if (base.Room.offScreenDen)
				{
					remainInDenCounter = 1;
				}
				else
				{
					remainInDenCounter = Random.Range(100, 1000);
				}
				if (abstractAI != null)
				{
					abstractAI.denPosition = pos;
				}
				spawnDen = pos;
			}
		}
		if (creatureTemplate.type == CreatureTemplate.Type.TentaclePlant || creatureTemplate.type == CreatureTemplate.Type.PoleMimic)
		{
			remainInDenCounter = 0;
		}
	}

	public override void Update(int time)
	{
		base.Update(time);
		if (state.alive && base.Room == null)
		{
			destroyOnAbstraction = true;
			Die();
			return;
		}
		if (ModManager.MMF && !InDen && state.alive && (creatureTemplate.type == CreatureTemplate.Type.TentaclePlant || creatureTemplate.type == CreatureTemplate.Type.PoleMimic) && pos.abstractNode != -1 && pos.abstractNode < base.Room.nodes.Length && GetNodeType != AbstractRoomNode.Type.Den)
		{
			destroyOnAbstraction = true;
			Die();
		}
		if (!InDen && abstractAI != null && realizedCreature == null && state.alive && !PacifiedBecauseCarried)
		{
			abstractAI.Update(time);
		}
		if (ModManager.HypothermiaModule && realizedCreature == null && Hypothermia < 1f && world.game.IsStorySession && world.rainCycle.BlizzardWorldActive && !(world.name.ToLowerInvariant() == "ug") && !Region.IsRubiconRegion(world.name) && !(world.name.ToLowerInvariant() == "cl") && !(world.name.ToLowerInvariant() == "sb"))
		{
			if (InDen || HypothermiaImmune || (ModManager.Watcher && rippleLayer != 0))
			{
				Hypothermia = Mathf.Lerp(Hypothermia, 0f, 0.04f);
			}
			else
			{
				Hypothermia = Mathf.Lerp(Hypothermia, 3f, Mathf.InverseLerp(0f, -600f, world.rainCycle.AmountLeft));
			}
		}
	}

	public void InDenUpdate(int time)
	{
		bool flag = !preCycle || (preCycle && (!ModManager.PrecycleModule || world.rainCycle.maxPreTimer > 0));
		if (!(remainInDenCounter > -1 && (!nightCreature || (nightCreature && (float)world.rainCycle.dayNightCounter > 600f) || ignoreCycle) && flag))
		{
			return;
		}
		if (WantToStayInDenUntilEndOfCycle())
		{
			remainInDenCounter = -1;
		}
		else
		{
			if (base.Room.world.game.IsArenaSession && !base.Room.world.game.GetArenaGameSession.IsCreatureAllowedToEmergeFromDen(this))
			{
				return;
			}
			if (ModManager.MSC)
			{
				if (!base.Room.isBattleArena)
				{
					remainInDenCounter -= time;
				}
				if (DrainWorldDenFlooded() && remainInDenCounter < 0)
				{
					remainInDenCounter = Random.Range(100, 400);
				}
				if (base.Room.battleArenaTriggeredTime > 0)
				{
					remainInDenCounter = -1;
				}
			}
			else
			{
				remainInDenCounter -= time;
			}
			if (remainInDenCounter < 0)
			{
				remainInDenCounter = -1;
				base.Room.MoveEntityOutOfDen(this);
			}
		}
	}

	public override void Move(WorldCoordinate newCoord)
	{
		if (Quantify && world.GetAbstractRoom(newCoord).realizedRoom == null)
		{
			Custom.Log("quantified creature delete itself!");
			world.GetAbstractRoom(pos).RemoveEntity(this);
			Destroy();
			world.GetAbstractRoom(newCoord).AddQuantifiedCreature(newCoord.abstractNode, creatureTemplate.type);
			return;
		}
		if (creatureTemplate.type == CreatureTemplate.Type.GarbageWorm && world.GetAbstractRoom(newCoord).garbageHoles > 0)
		{
			GarbageWormAI.MoveAbstractCreatureToGarbage(this, world.GetAbstractRoom(newCoord));
		}
		if (abstractAI != null && newCoord.room != pos.room)
		{
			abstractAI.lastRoom = pos.room;
		}
		base.Move(newCoord);
		if (abstractAI != null)
		{
			abstractAI.Moved();
		}
	}

	public override void ChangeRooms(WorldCoordinate newCoord)
	{
		if (realizedCreature == null && world.GetAbstractRoom(newCoord).realizedRoom != null && world.GetAbstractRoom(newCoord).realizedRoom.shortCutsReady)
		{
			Realize();
			realizedCreature.inShortcut = true;
			world.game.shortcuts.CreatureEnterFromAbstractRoom(realizedCreature, world.GetAbstractRoom(newCoord), newCoord.abstractNode);
		}
		base.ChangeRooms(newCoord);
	}

	public override void Realize()
	{
		if (base.Room == null)
		{
			return;
		}
		if (ModManager.MSC)
		{
			CheckVoidseaArena();
		}
		if (realizedCreature != null)
		{
			return;
		}
		if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Slugcat)
		{
			realizedCreature = new Player(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate)
		{
			realizedCreature = new Lizard(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Fly)
		{
			realizedCreature = new Fly(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Leech)
		{
			realizedCreature = new Leech(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Snail)
		{
			realizedCreature = new Snail(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Vulture)
		{
			realizedCreature = new Vulture(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.GarbageWorm)
		{
			GarbageWormAI.MoveAbstractCreatureToGarbage(this, base.Room);
			realizedCreature = new GarbageWorm(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.CicadaA)
		{
			realizedCreature = new Cicada(this, world, creatureTemplate.type == CreatureTemplate.Type.CicadaA);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LanternMouse)
		{
			realizedCreature = new LanternMouse(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Spider)
		{
			realizedCreature = new Spider(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.JetFish)
		{
			realizedCreature = new JetFish(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigEel)
		{
			realizedCreature = new BigEel(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Deer)
		{
			realizedCreature = new Deer(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TubeWorm)
		{
			realizedCreature = new TubeWorm(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.DaddyLongLegs)
		{
			if (ModManager.DLCShared && creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.Inspector)
			{
				realizedCreature = new Inspector(this, world);
				state = new Inspector.InspectorState(this);
			}
			else
			{
				realizedCreature = new DaddyLongLegs(this, world);
			}
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TentaclePlant)
		{
			if (creatureTemplate.type == CreatureTemplate.Type.TentaclePlant)
			{
				realizedCreature = new TentaclePlant(this, world);
			}
			else
			{
				realizedCreature = new PoleMimic(this, world);
			}
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.MirosBird)
		{
			realizedCreature = new MirosBird(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TempleGuard)
		{
			realizedCreature = new TempleGuard(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centipede || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.RedCentipede || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centiwing || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.SmallCentipede)
		{
			realizedCreature = new Centipede(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger)
		{
			realizedCreature = new Scavenger(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Overseer)
		{
			realizedCreature = new Overseer(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.VultureGrub)
		{
			if (creatureTemplate.type == CreatureTemplate.Type.VultureGrub)
			{
				realizedCreature = new VultureGrub(this, world);
			}
			else if (creatureTemplate.type == CreatureTemplate.Type.Hazer)
			{
				realizedCreature = new Hazer(this, world);
			}
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.EggBug)
		{
			realizedCreature = new EggBug(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigSpider || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.SpitterSpider)
		{
			realizedCreature = new BigSpider(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigNeedleWorm)
		{
			if (creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm)
			{
				realizedCreature = new SmallNeedleWorm(this, world);
			}
			else
			{
				realizedCreature = new BigNeedleWorm(this, world);
			}
		}
		else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.DropBug)
		{
			realizedCreature = new DropBug(this, world);
		}
		if (ModManager.DLCShared)
		{
			MSCRealizeCustom();
		}
		if (ModManager.Watcher)
		{
			WatcherRealizeCustom();
		}
		InitiateAI();
		for (int i = 0; i < stuckObjects.Count; i++)
		{
			if (stuckObjects[i].A.realizedObject == null)
			{
				stuckObjects[i].A.Realize();
			}
			if (stuckObjects[i].B.realizedObject == null)
			{
				stuckObjects[i].B.Realize();
			}
		}
	}

	public void InitiateAI()
	{
		if (!(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Slugcat))
		{
			if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate)
			{
				abstractAI.RealAI = new LizardAI(this, world);
			}
			else if (!(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Fly) && !(creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Leech))
			{
				if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Snail)
				{
					abstractAI.RealAI = new SnailAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Vulture)
				{
					abstractAI.RealAI = new VultureAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.GarbageWorm)
				{
					abstractAI.RealAI = new GarbageWormAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.CicadaA)
				{
					abstractAI.RealAI = new CicadaAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LanternMouse)
				{
					abstractAI.RealAI = new MouseAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.JetFish)
				{
					abstractAI.RealAI = new JetFishAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigEel)
				{
					abstractAI.RealAI = new BigEelAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Deer)
				{
					abstractAI.RealAI = new DeerAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TubeWorm)
				{
					abstractAI.RealAI = new TubeWormAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.DaddyLongLegs)
				{
					if (ModManager.DLCShared && creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.Inspector)
					{
						abstractAI.RealAI = new InspectorAI(this, world);
					}
					else
					{
						abstractAI.RealAI = new DaddyAI(this, world);
					}
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TentaclePlant)
				{
					if (creatureTemplate.type == CreatureTemplate.Type.TentaclePlant)
					{
						abstractAI.RealAI = new TentaclePlantAI(this, world);
					}
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.MirosBird)
				{
					abstractAI.RealAI = new MirosBirdAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.TempleGuard)
				{
					abstractAI.RealAI = new TempleGuardAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centipede || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.RedCentipede || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centiwing || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.SmallCentipede)
				{
					abstractAI.RealAI = new CentipedeAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger)
				{
					abstractAI.RealAI = new ScavengerAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Overseer)
				{
					abstractAI.RealAI = new OverseerAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.EggBug)
				{
					abstractAI.RealAI = new EggBugAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigSpider || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.SpitterSpider)
				{
					abstractAI.RealAI = new BigSpiderAI(this, world);
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigNeedleWorm || creatureTemplate.TopAncestor().type == CreatureTemplate.Type.SmallNeedleWorm)
				{
					if (creatureTemplate.type == CreatureTemplate.Type.SmallNeedleWorm)
					{
						abstractAI.RealAI = new SmallNeedleWormAI(this, world);
					}
					else
					{
						abstractAI.RealAI = new BigNeedleWormAI(this, world);
					}
				}
				else if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.DropBug)
				{
					abstractAI.RealAI = new DropBugAI(this, world);
				}
			}
		}
		if (ModManager.Watcher)
		{
			WatcherInitiateAI();
		}
		if (ModManager.DLCShared)
		{
			MSCInitiateAI();
		}
	}

	public override void RealizeInRoom()
	{
		if (base.Room.shelter && base.Room.realizedRoom != null)
		{
			Custom.Log("realize critter in shelter", creatureTemplate.name);
			for (int i = 0; i < base.Room.realizedRoom.updateList.Count; i++)
			{
				if (base.Room.realizedRoom.updateList[i] is ShelterDoor)
				{
					pos.Tile = (base.Room.realizedRoom.updateList[i] as ShelterDoor).playerSpawnPos;
					if (ModManager.CoopAvailable && state is PlayerState)
					{
						pos.Tile = new IntVector2(pos.Tile.x, pos.Tile.y + (state as PlayerState).playerNumber);
					}
					break;
				}
			}
		}
		if (!pos.NodeDefined)
		{
			pos.abstractNode = base.Room.CreatureSpecificToCommonNodeIndex(Random.Range(0, base.Room.NodesRelevantToCreature(creatureTemplate)), creatureTemplate);
		}
		IntVector2[] alreadyAccessible = null;
		if (abstractAI != null)
		{
			alreadyAccessible = abstractAI.PlaceInRealizedRoom();
		}
		base.RealizeInRoom();
		if (abstractAI != null && abstractAI.RealAI != null && abstractAI.RealAI.pathFinder != null)
		{
			abstractAI.RealAI.pathFinder.InitiAccessibilityMapping(pos, alreadyAccessible);
		}
	}

	public override void Abstractize(WorldCoordinate coord)
	{
		if (destroyOnAbstraction)
		{
			realizedCreature = null;
			if (abstractAI != null)
			{
				abstractAI.RealAI = null;
			}
			base.Room.RemoveEntity(this);
			Destroy();
			return;
		}
		if (realizedCreature == null || base.Room.realizedRoom == null || !base.Room.realizedRoom.readyForAI)
		{
			realizedCreature = null;
			return;
		}
		realizedCreature.Abstractize();
		timeSpentHere = 0;
		distanceToMyNode = 0;
		if (coord == pos)
		{
			pos = QuickConnectivity.DefineNodeOfLocalCoordinate(pos, world, creatureTemplate);
			if (creatureTemplate.PreBakedPathingIndex > -1)
			{
				int creatureSpecificExitIndex = base.Room.CommonToCreatureSpecificNodeIndex(pos.abstractNode, creatureTemplate);
				distanceToMyNode = base.Room.realizedRoom.aimap.ExitDistanceForCreatureAndCheckNeighbours(pos.Tile, creatureSpecificExitIndex, creatureTemplate);
				if (distanceToMyNode == -1)
				{
					if (pos.abstractNode < base.Room.realizedRoom.exitAndDenIndex.Length && base.Room.realizedRoom.aimap.TileOrNeighborsAccessibleToCreature(pos.Tile, creatureTemplate))
					{
						distanceToMyNode = Custom.ManhattanDistance(pos.Tile, world.GetAbstractRoom(pos).realizedRoom.ShortcutLeadingToNode(QuickConnectivity.DefineNodeOfLocalCoordinate(pos, world, creatureTemplate).abstractNode).StartTile);
					}
					else if (base.Room.nodes[pos.abstractNode].borderExit)
					{
						distanceToMyNode = 0;
					}
				}
			}
			else
			{
				distanceToMyNode = -1;
			}
		}
		Move(coord);
		if (creatureTemplate.grasps > 0)
		{
			for (int i = 0; i < creatureTemplate.grasps; i++)
			{
				if (realizedCreature.grasps[i] == null)
				{
					continue;
				}
				bool flag = false;
				if (ModManager.MMF)
				{
					AbstractPhysicalObject abstractPhysicalObject = realizedCreature.grasps[i].grabbed.abstractPhysicalObject;
					if (abstractPhysicalObject is AbstractCreature && (abstractPhysicalObject as AbstractCreature).realizedCreature != null && (abstractPhysicalObject as AbstractCreature).realizedCreature.grasps != null)
					{
						Creature.Grasp[] grasps = (abstractPhysicalObject as AbstractCreature).realizedCreature.grasps;
						foreach (Creature.Grasp grasp in grasps)
						{
							if (grasp != null && grasp.grabbed != null && grasp.grabbed == realizedCreature)
							{
								Custom.LogWarning("ABSTRACT MOVEMENT DANGER > Canceling grasp abstractize, Grasp is circular and thus endless! Was : " + this?.ToString() + " holding onto " + grasp.grabbed);
								flag = true;
								grasp.Release();
							}
						}
					}
					if (realizedCreature.grasps[i] != null && realizedCreature.grasps[i].grabbed == realizedCreature)
					{
						Custom.LogWarning("ABSTRACT MOVEMENT DANGER > Canceling grasp abstractize, Grasp is holding itself, will be endless! Was : " + this);
						flag = true;
						realizedCreature.grasps[i].Release();
					}
				}
				if (!flag)
				{
					realizedCreature.grasps[i].grabbed.abstractPhysicalObject.Abstractize(coord);
				}
			}
		}
		if (creatureTemplate.AI && abstractAI.RealAI != null && abstractAI.RealAI.pathFinder != null)
		{
			abstractAI.SetDestination(QuickConnectivity.DefineNodeOfLocalCoordinate(abstractAI.destination, world, creatureTemplate));
			abstractAI.timeBuffer = 0;
			if (creatureTemplate.abstractImmobile)
			{
				abstractAI.path.Clear();
			}
			else if (abstractAI.destination.room == pos.room && abstractAI.destination.abstractNode == pos.abstractNode)
			{
				abstractAI.path.Clear();
			}
			else
			{
				List<WorldCoordinate> list = abstractAI.RealAI.pathFinder.CreatePathForAbstractreature(abstractAI.destination);
				if (list != null)
				{
					abstractAI.path = list;
				}
				else
				{
					abstractAI.FindPath(abstractAI.destination);
				}
			}
			abstractAI.RealAI = null;
		}
		realizedCreature = null;
	}

	public bool AllowedToExistInRoom(Room room)
	{
		if (room.game == null)
		{
			return false;
		}
		if (room.readyForAI)
		{
			return true;
		}
		if (room.shortCutsReady && !creatureTemplate.AI)
		{
			return true;
		}
		return false;
	}

	public void OpportunityToEnterDen(WorldCoordinate den)
	{
		if (creatureTemplate.doesNotUseDens)
		{
			return;
		}
		if (WantToStayInDenUntilEndOfCycle())
		{
			Custom.Log(creatureTemplate.name, "stay in den until cycle end");
			remainInDenCounter = -1;
			base.Room.MoveEntityToDen(this);
		}
		else if (remainInDenCounter > 0)
		{
			base.Room.MoveEntityToDen(this);
		}
		else if (creatureTemplate.stowFoodInDen)
		{
			bool flag = false;
			for (int i = 0; i < stuckObjects.Count; i++)
			{
				if (stuckObjects[i] is CreatureGripStick && stuckObjects[i].A == this && stuckObjects[i].B is AbstractCreature)
				{
					flag = true;
				}
			}
			if (flag)
			{
				remainInDenCounter = 200;
				base.Room.MoveEntityToDen(this);
			}
		}
		else if (ModManager.PrecycleModule && base.Room.world.rainCycle.preTimer > 0)
		{
			remainInDenCounter = 300;
			base.Room.MoveEntityToDen(this);
		}
	}

	public override void IsEnteringDen(WorldCoordinate den)
	{
		if (creatureTemplate.quantified && creatureTemplate.type == CreatureTemplate.Type.Fly)
		{
			world.fliesWorldAI.RespawnOneFly();
			base.Room.RemoveEntity(this);
			return;
		}
		base.IsEnteringDen(den);
		for (int num = stuckObjects.Count - 1; num >= 0; num--)
		{
			if (num < stuckObjects.Count && stuckObjects[num] is CreatureGripStick && stuckObjects[num].A == this)
			{
				if (stuckObjects[num].B is AbstractCreature)
				{
					if (abstractAI != null && abstractAI.RealAI != null && abstractAI.RealAI.preyTracker != null)
					{
						abstractAI.RealAI.preyTracker.ForgetPrey(stuckObjects[num].B as AbstractCreature);
					}
					if (ModManager.ChallengeModule && base.Room.realizedRoom != null && base.Room.realizedRoom.world.game.IsArenaSession && base.Room.realizedRoom.world.game.GetArenaGameSession.chMeta != null && base.Room.realizedRoom.world.game.GetArenaGameSession.chMeta.secondaryWinMethod == ChallengeInformation.ChallengeMeta.WinCondition.PROTECT)
					{
						bool flag = false;
						if (base.Room.realizedRoom.world.game.GetArenaGameSession.chMeta.protectCreature == null || base.Room.realizedRoom.world.game.GetArenaGameSession.chMeta.protectCreature == "")
						{
							if ((stuckObjects[num].B as AbstractCreature).creatureTemplate.type != CreatureTemplate.Type.Slugcat)
							{
								flag = true;
							}
						}
						else if ((stuckObjects[num].B as AbstractCreature).creatureTemplate.name.ToLower() == base.Room.realizedRoom.world.game.GetArenaGameSession.chMeta.protectCreature.ToLower() || (stuckObjects[num].B as AbstractCreature).creatureTemplate.type.value.ToLower() == base.Room.realizedRoom.world.game.GetArenaGameSession.chMeta.protectCreature.ToLower())
						{
							flag = true;
						}
						if (flag)
						{
							for (int i = 0; i < base.Room.realizedRoom.world.game.Players.Count; i++)
							{
								if (base.Room.realizedRoom.world.game.Players[i].realizedCreature != null)
								{
									base.Room.realizedRoom.world.game.Players[i].realizedCreature.Die();
								}
							}
						}
					}
					if (ModManager.CoopAvailable && (stuckObjects[num].B as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Slugcat)
					{
						if ((stuckObjects[num].B as AbstractCreature).realizedCreature != null)
						{
							((stuckObjects[num].B as AbstractCreature).realizedCreature as Player).PermaDie();
						}
						else
						{
							JollyCustom.Log("[Jolly] Abs player got dragged into a den, permadying...");
							((stuckObjects[num].B as AbstractCreature).state as PlayerState).permaDead = true;
						}
					}
					(stuckObjects[num].B as AbstractCreature).Die();
					if ((stuckObjects[num].B as AbstractCreature).realizedCreature != null)
					{
						(stuckObjects[num].B as AbstractCreature).realizedCreature.Die();
					}
					if (remainInDenCounter > -1 && remainInDenCounter < 200 && !WantToStayInDenUntilEndOfCycle())
					{
						remainInDenCounter = 200;
					}
				}
				if (abstractAI == null || abstractAI.DoIwantToDropThisItemInDen(stuckObjects[num].B))
				{
					DropCarriedObject((stuckObjects[num] as CreatureGripStick).grasp);
				}
			}
		}
		if ((nightCreature || ignoreCycle) && (remainInDenCounter > 2400 || remainInDenCounter > world.rainCycle.TimeUntilRain))
		{
			Abstractize(den);
		}
	}

	public override void IsExitingDen()
	{
		base.IsExitingDen();
		timeSpentHere = 0;
		if (base.Room.realizedRoom != null && base.Room.realizedRoom.shortCutsReady)
		{
			Realize();
			realizedCreature.inShortcut = true;
			world.game.shortcuts.CreatureEnterFromAbstractRoom(realizedCreature, world.GetAbstractRoom(pos), pos.abstractNode);
		}
	}

	public void DropCarriedObject(int graspIndex)
	{
		for (int num = stuckObjects.Count - 1; num >= 0; num--)
		{
			if (stuckObjects[num] is CreatureGripStick && stuckObjects[num].A == this && (stuckObjects[num] as CreatureGripStick).grasp == graspIndex)
			{
				stuckObjects[num].Deactivate();
			}
		}
		if (realizedCreature != null && realizedCreature.grasps != null && realizedCreature.grasps[graspIndex] != null)
		{
			realizedCreature.ReleaseGrasp(graspIndex);
		}
	}

	public bool WantToStayInDenUntilEndOfCycle()
	{
		if (ModManager.MMF && creatureTemplate.type == CreatureTemplate.Type.Fly)
		{
			return false;
		}
		if (creatureTemplate.doesNotUseDens)
		{
			return false;
		}
		if (ModManager.ChallengeModule && world.game.IsArenaSession && world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge)
		{
			return false;
		}
		if (state.dead)
		{
			return true;
		}
		if (state is HealthState && (state as HealthState).health < 0.6f)
		{
			return true;
		}
		if (world.rainCycle.TimeUntilRain < ((!world.game.IsStorySession) ? 15 : 60) * 40 && !nightCreature && !ignoreCycle)
		{
			return true;
		}
		if (preCycle && (!ModManager.PrecycleModule || world.rainCycle.maxPreTimer <= 0))
		{
			return true;
		}
		if (ModManager.MMF && (creatureTemplate.type == CreatureTemplate.Type.PoleMimic || creatureTemplate.type == CreatureTemplate.Type.TentaclePlant))
		{
			return false;
		}
		if (abstractAI != null)
		{
			bool flag = abstractAI.RealAI != null && abstractAI.RealAI.injuryTracker != null && abstractAI.RealAI.injuryTracker.Utility() > 0.5f;
			if (!ModManager.PrecycleModule || world.rainCycle.maxPreTimer <= 0)
			{
				return abstractAI.WantToStayInDenUntilEndOfCycle() || flag;
			}
			return flag;
		}
		if (creatureTemplate.type == CreatureTemplate.Type.Slugcat)
		{
			return false;
		}
		if (creatureTemplate.type == CreatureTemplate.Type.Leech && realizedCreature != null && (realizedCreature as Leech).fleeFromRain)
		{
			return true;
		}
		if (creatureTemplate.type == CreatureTemplate.Type.Spider && realizedCreature != null && (realizedCreature as Spider).denMovement == 1)
		{
			return true;
		}
		return false;
	}

	public bool RequiresAIMapToEnterRoom()
	{
		if (creatureTemplate.requireAImap)
		{
			return true;
		}
		List<AbstractPhysicalObject> allConnectedObjects = GetAllConnectedObjects();
		for (int i = 0; i < allConnectedObjects.Count; i++)
		{
			if (allConnectedObjects[i] is AbstractCreature && (allConnectedObjects[i] as AbstractCreature).creatureTemplate.requireAImap)
			{
				return true;
			}
		}
		return false;
	}

	public bool FollowedByCamera(int cameraNumber)
	{
		if (world.game.cameras[cameraNumber].followAbstractCreature == this)
		{
			return true;
		}
		List<AbstractPhysicalObject> allConnectedObjects = GetAllConnectedObjects();
		for (int i = 0; i < allConnectedObjects.Count; i++)
		{
			if (world.game.cameras[cameraNumber].followAbstractCreature == allConnectedObjects[i])
			{
				return true;
			}
		}
		return false;
	}

	public void Die()
	{
		if (state.alive && ID.spawner >= 0 && world.game.session is StoryGameSession)
		{
			(world.game.session as StoryGameSession).saveState.AddCreatureToRespawn(this);
		}
		state.Die();
		if (abstractAI != null)
		{
			abstractAI.Die();
		}
	}

	public override string ToString()
	{
		return creatureTemplate.name + " " + ID.ToString();
	}

	public bool DrainWorldDenFlooded()
	{
		if (world == null || base.Room == null || (world != null && world.game.globalRain.drainWorldFlood == 0f))
		{
			return false;
		}
		if (creatureTemplate.doesNotUseDens)
		{
			return false;
		}
		if (base.Room == world.offScreenDen)
		{
			return false;
		}
		if (pos.abstractNode > -1 && creatureTemplate.waterRelationship != CreatureTemplate.WaterRelationship.Amphibious && creatureTemplate.waterRelationship != CreatureTemplate.WaterRelationship.WaterOnly && world.game.globalRain.drainWorldFlood > 0f)
		{
			if (base.Room.realizedRoom == null && world.game.globalRain.DrainWorldPositionFlooded(new WorldCoordinate(pos.room, 0, world.GetAbstractRoom(pos.room).size.y / 4, -1)))
			{
				return true;
			}
			if (base.Room.realizedRoom != null && (base.Room.realizedRoom.roomSettings.DangerType == RoomRain.DangerType.Flood || base.Room.realizedRoom.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain) && base.Room.realizedRoom.shortCutsReady && world.game.globalRain.DrainWorldPositionFlooded(base.Room.realizedRoom.ShortcutLeadingToNode(pos.abstractNode).startCoord))
			{
				return true;
			}
		}
		return false;
	}

	public void setCustomFlags()
	{
		if (base.Room == null)
		{
			return;
		}
		unrecognizedFlags = new List<string>();
		nightCreature = false;
		ignoreCycle = false;
		superSizeMe = false;
		tentacleImmune = false;
		voidCreature = false;
		if (base.Room.world.game.IsStorySession && base.Room.world.rainCycle.BlizzardWorldActive)
		{
			if (ModManager.HypothermiaModule && creatureTemplate.BlizzardAdapted)
			{
				HypothermiaImmune = true;
			}
			if (creatureTemplate.BlizzardWanderer)
			{
				ignoreCycle = true;
			}
		}
		if (ModManager.ChallengeModule && base.Room.world.game.IsArenaSession && base.Room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge)
		{
			ChallengeInformation.ChallengeMeta challengeMeta = base.Room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta;
			if (challengeMeta.globalTag != null && challengeMeta.globalTag == "Lavasafe")
			{
				lavaImmune = true;
			}
			else if (challengeMeta.globalTag != null && challengeMeta.globalTag == "Voidsea")
			{
				voidCreature = true;
				lavaImmune = true;
			}
			else if (challengeMeta.globalTag != null && challengeMeta.globalTag == "TentacleImmune")
			{
				tentacleImmune = true;
			}
		}
		List<string> list = new List<string>();
		if (base.Room.world.region != null)
		{
			list.AddRange(base.Room.world.region.regionParams.globalCreatureFlags_All);
			if (base.Room.world.region.regionParams.globalCreatureFlags_Specific.TryGetValue(creatureTemplate.type, out var value))
			{
				list.AddRange(value);
			}
		}
		if (spawnData != null && spawnData[0] == '{')
		{
			list.AddRange(spawnData.Substring(1, spawnData.Length - 2).Split(',', '|'));
		}
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Length <= 0)
			{
				continue;
			}
			switch (list[i].Split(':')[0])
			{
			case "Night":
				nightCreature = true;
				ignoreCycle = false;
				break;
			case "Lavasafe":
				lavaImmune = true;
				break;
			case "Ignorecycle":
				ignoreCycle = true;
				break;
			case "AlternateForm":
				superSizeMe = true;
				break;
			case "TentacleImmune":
				tentacleImmune = true;
				break;
			case "Winter":
				Winterized = true;
				break;
			case "PreCycle":
				preCycle = true;
				break;
			case "Voidsea":
				lavaImmune = true;
				voidCreature = true;
				break;
			case "Ripple":
				rippleCreature = true;
				rippleLayer = 1;
				break;
			case "Seed":
				ID.setAltSeed(int.Parse(list[i].Split(':')[1], NumberStyles.Any, CultureInfo.InvariantCulture));
				personality = new Personality(ID);
				break;
			case "RotType":
				if (state != null && state is LizardState lizardState)
				{
					int a = int.Parse(list[i].Split(':')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					lizardState.SetRotType(new LizardState.RotType(ExtEnum<LizardState.RotType>.values.entries[Mathf.Min(a, ExtEnum<LizardState.RotType>.values.entries.Count - 1)]));
					flag = true;
				}
				break;
			case "Slayer":
				if (state != null && state is Tardigrade.TardigradeState tardigradeState)
				{
					tardigradeState.slayer = true;
				}
				break;
			case "NamedAttr":
				namedAttraction = list[i].Split(':')[1];
				abstractAI.NewWorld(world);
				break;
			default:
				if (ModManager.DevTools && world.game.IsArenaSession)
				{
					Custom.LogWarning($"creature [{creatureTemplate.type}] has unrecognized flag: [{list[i]}]");
				}
				unrecognizedFlags.Add(list[i]);
				break;
			}
		}
		if (ModManager.PrecycleModule && preCycle && base.Room.shelter)
		{
			preCycle = false;
		}
		if (!ModManager.Watcher || state == null || !(state is LizardState lizardState2))
		{
			return;
		}
		if (base.Room.world.game.IsStorySession && base.Room.world.game.GetStorySession.finalWarpSequenceStarted)
		{
			lizardState2.SetRotType(LizardState.RotType.None);
		}
		else if (base.Room.world.regionState != null && base.Room.world.regionState.sentientRotProgression.ContainsKey(base.Room.name))
		{
			RegionState.SentientRotState sentientRotState = base.Room.world.regionState.sentientRotProgression[base.Room.name];
			LizardState.RotType rotType = LizardState.RotType.None;
			if (sentientRotState.rotIntensity > 0.75f)
			{
				rotType = ((ID.number % 10 < 4) ? LizardState.RotType.Full : ((ID.number % 10 >= 7) ? LizardState.RotType.Slight : LizardState.RotType.Opossum));
			}
			else if (sentientRotState.rotIntensity > 0.5f)
			{
				if (ID.number % 10 < 1)
				{
					rotType = LizardState.RotType.Full;
				}
				else if (ID.number % 10 < 4)
				{
					rotType = LizardState.RotType.Opossum;
				}
				else if (ID.number % 10 < 8)
				{
					rotType = LizardState.RotType.Slight;
				}
			}
			else if (sentientRotState.rotIntensity > 0.25f)
			{
				if (ID.number % 10 < 2)
				{
					rotType = LizardState.RotType.Opossum;
				}
				else if (ID.number % 10 < 6)
				{
					rotType = LizardState.RotType.Slight;
				}
			}
			else if (sentientRotState.rotIntensity > 0f)
			{
				if (ID.number % 10 < 1)
				{
					rotType = LizardState.RotType.Opossum;
				}
				else if (ID.number % 10 < 3)
				{
					rotType = LizardState.RotType.Slight;
				}
			}
			if (Region.IsWatcherVanillaRegion(base.Room.world.name) && base.Room.world.game.IsStorySession && base.Room.world.game.GetStorySession.saveState.miscWorldSaveData.discoveredWarpPoints.Count == 0 && base.Room.world.game.GetStorySession.saveState.deathPersistentSaveData.spawnedWarpPoints.Count == 0)
			{
				if (base.Room.world.game.GetStorySession.saveState.deathPersistentSaveData.maximumRippleLevel >= 0.5f)
				{
					if (rotType == LizardState.RotType.Full)
					{
						rotType = LizardState.RotType.Opossum;
					}
				}
				else
				{
					rotType = LizardState.RotType.None;
				}
			}
			if (rotType.Index > lizardState2.rotType.Index)
			{
				lizardState2.SetRotType(rotType);
			}
		}
		else if (!flag)
		{
			lizardState2.SetRotType(LizardState.RotType.None);
		}
	}

	public void MSCStateAI()
	{
		if (creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Overseer)
		{
			lavaImmune = true;
		}
		if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.MirosVulture)
		{
			state = new Vulture.VultureState(this);
		}
		if (creatureTemplate.AI && creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.MirosVulture)
		{
			abstractAI = new VultureAbstractAI(world, this);
		}
		if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
		{
			state = new DaddyLongLegs.DaddyState(this);
		}
		if (creatureTemplate.AI && creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ScavengerElite)
		{
			abstractAI = new ScavengerAbstractAI(world, this);
		}
		if (creatureTemplate.AI && creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.Yeek)
		{
			state = new YeekState(this);
		}
		if (creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.BigJelly)
		{
			state = new BigJellyState(this);
		}
		if (creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.StowawayBug)
		{
			state = new StowawayBugState(this);
		}
		if (creatureTemplate.type == CreatureTemplate.Type.Overseer)
		{
			lavaImmune = true;
		}
		if (creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.StowawayBug)
		{
			remainInDenCounter = 0;
		}
		if (ModManager.MSC)
		{
			if (creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				state = new DaddyLongLegs.DaddyState(this);
			}
			if (creatureTemplate.AI && creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
			{
				abstractAI = new ScavengerAbstractAI(world, this);
			}
			if (creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				state = new PlayerNPCState(this, 0);
				abstractAI = new SlugNPCAbstractAI(world, this);
			}
		}
	}

	public void WatcherStateAI()
	{
		if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.DrillCrab)
		{
			abstractAI = new DrillCrabAbstractAI(world, this);
			state = new DrillCrabState(this);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.SandGrub)
		{
			state = new SandGrubState(this);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			state = new BigMoth.BigMothState(this);
			abstractAI = new BigMothAbstractAI(world, this);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.SkyWhale)
		{
			abstractAI = new SkyWhaleAbstractAI(world, this);
		}
		else if (creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerTemplar)
		{
			abstractAI = new ScavengerAbstractAI(world, this);
		}
		else if (creatureTemplate.type == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
		{
			abstractAI = new ScavengerAbstractAI(world, this);
		}
		else if (creatureTemplate.type == WatcherEnums.CreatureTemplateType.Rat)
		{
			abstractAI = new AbstractCreatureAI(world, this);
		}
		else if (creatureTemplate.type == WatcherEnums.CreatureTemplateType.Frog)
		{
			state = new FrogState(this);
			abstractAI = new AbstractCreatureAI(world, this);
		}
		else if (creatureTemplate.type == WatcherEnums.CreatureTemplateType.Loach || creatureTemplate.type == WatcherEnums.CreatureTemplateType.RotLoach)
		{
			state = new LoachState(this);
			abstractAI = new LoachAbstractAI(world, this);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Tardigrade)
		{
			state = new Tardigrade.TardigradeState(this);
		}
		else if (creatureTemplate.type == WatcherEnums.CreatureTemplateType.RippleSpider)
		{
			abstractAI = new RippleSpiderAbstractAI(world, this);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			state = new MothGrub.MothGrubState(this);
		}
	}

	public void CheckVoidseaArena()
	{
		if (base.Room.world.game.IsArenaSession && base.Room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType != DLCSharedEnums.GameTypeID.Challenge && creatureTemplate.type != CreatureTemplate.Type.Slugcat)
		{
			voidCreature = MultiplayerUnlocks.LevelLockID(base.Room.name) == MoreSlugcatsEnums.LevelUnlockID.HR;
			lavaImmune = voidCreature;
		}
	}

	public bool CanTravelTogether(AbstractCreature otherCreature)
	{
		return otherCreature.namedAttraction == namedAttraction;
	}

	public void MSCRealizeCustom()
	{
		if (ModManager.ChallengeModule && base.Room.world.game.IsArenaSession && base.Room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge && creatureTemplate.type != CreatureTemplate.Type.Slugcat)
		{
			setCustomFlags();
		}
		if (realizedCreature != null)
		{
			return;
		}
		if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.AquaCenti)
		{
			realizedCreature = new Centipede(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
		{
			superSizeMe = true;
			realizedCreature = new DaddyLongLegs(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.MotherSpider)
		{
			realizedCreature = new BigSpider(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.MirosVulture)
		{
			realizedCreature = new Vulture(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.StowawayBug)
		{
			realizedCreature = new StowawayBug(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.Yeek)
		{
			realizedCreature = new Yeek(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.BigJelly)
		{
			realizedCreature = new BigJellyFish(this, world);
		}
		if (ModManager.MSC)
		{
			if (creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				realizedCreature = new DaddyLongLegs(this, world);
			}
			else if (creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				realizedCreature = new EggBug(this, world);
			}
			else if (creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				realizedCreature = new Player(this, world);
			}
		}
	}

	public void MSCInitiateAI()
	{
		if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.AquaCenti)
		{
			abstractAI.RealAI = new CentipedeAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.MotherSpider)
		{
			abstractAI.RealAI = new BigSpiderAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.MirosVulture)
		{
			abstractAI.RealAI = new VultureAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.StowawayBug)
		{
			abstractAI.RealAI = new StowawayBugAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == DLCSharedEnums.CreatureTemplateType.Yeek)
		{
			abstractAI.RealAI = new YeekAI(this, world);
		}
		if (ModManager.MSC)
		{
			if (creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				abstractAI.RealAI = new EggBugAI(this, world);
			}
			else if (creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				abstractAI.RealAI = new SlugNPCAI(this, world);
			}
		}
	}

	public void WatcherRealizeCustom()
	{
		if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.DrillCrab)
		{
			realizedCreature = new DrillCrab(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Barnacle)
		{
			realizedCreature = new Barnacle(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.SandGrub)
		{
			realizedCreature = new SandGrub(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			realizedCreature = new BigMoth(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.BoxWorm)
		{
			realizedCreature = new BoxWorm(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.FireSprite)
		{
			realizedCreature = new FireSprite(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Rattler)
		{
			realizedCreature = new Rattler(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.SkyWhale)
		{
			realizedCreature = new SkyWhale(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Loach)
		{
			realizedCreature = new Loach(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Rat)
		{
			realizedCreature = new Rat(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Frog)
		{
			realizedCreature = new Frog(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Tardigrade)
		{
			realizedCreature = new Tardigrade(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.GrappleSnake)
		{
			realizedCreature = new GrappleSnake(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Millipede)
		{
			realizedCreature = new Millipede(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Angler)
		{
			realizedCreature = new Angler(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.RippleSpider)
		{
			realizedCreature = new RippleSpider(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			realizedCreature = new MothGrub(this, world);
		}
	}

	public void WatcherInitiateAI()
	{
		if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.DrillCrab)
		{
			abstractAI.RealAI = new DrillCrabAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Barnacle)
		{
			abstractAI.RealAI = new BarnacleAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.SandGrub)
		{
			abstractAI.RealAI = new SandGrubAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			abstractAI.RealAI = new BigMothAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.BoxWorm)
		{
			abstractAI.RealAI = new BoxWormAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.FireSprite)
		{
			abstractAI.RealAI = new FireSpriteAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Rattler)
		{
			abstractAI.RealAI = new RattlerAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.SkyWhale)
		{
			abstractAI.RealAI = new SkyWhaleAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Loach)
		{
			abstractAI.RealAI = new LoachAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Rat)
		{
			abstractAI.RealAI = new RatAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Frog)
		{
			abstractAI.RealAI = new FrogAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Tardigrade)
		{
			abstractAI.RealAI = new TardigradeAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.GrappleSnake)
		{
			abstractAI.RealAI = new GrappleSnakeAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Millipede)
		{
			abstractAI.RealAI = new MillipedeAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.Angler)
		{
			abstractAI.RealAI = new AnglerAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.RippleSpider)
		{
			abstractAI.RealAI = new RippleSpiderAI(this, world);
		}
		else if (creatureTemplate.TopAncestor().type == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			abstractAI.RealAI = new MothGrubAI(this, world);
		}
	}

	public void extractKarma()
	{
		if (!global::MoreSlugcats.MoreSlugcats.cfgArtificerCorpseNoKarmaLoss.Value)
		{
			RemovedKarma++;
		}
	}

	public bool IsVoided()
	{
		if (ModManager.MSC && voidCreature)
		{
			if (!(creatureTemplate.type == CreatureTemplate.Type.RedLizard) && !(creatureTemplate.type == CreatureTemplate.Type.RedCentipede) && !(creatureTemplate.type == CreatureTemplate.Type.BigSpider) && !(creatureTemplate.type == CreatureTemplate.Type.DaddyLongLegs) && !(creatureTemplate.type == CreatureTemplate.Type.BrotherLongLegs) && !(creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs) && !(creatureTemplate.type == CreatureTemplate.Type.BigEel))
			{
				return creatureTemplate.type == CreatureTemplate.Type.CyanLizard;
			}
			return true;
		}
		return false;
	}
}
