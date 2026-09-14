using System.Collections.Generic;
using Noise;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BarnacleAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction
{
	public class AIParams
	{
		public bool respondsToNoise;
	}

	public class Behavior : ExtEnum<Behavior>
	{
		public static Behavior Idle = new Behavior("Idle", register: true);

		public static Behavior Travelling = new Behavior("Travelling", register: true);

		public static Behavior Flee = new Behavior("Flee", register: true);

		public static Behavior InvestigateSound = new Behavior("InvestigateSound", register: true);

		public static Behavior EscapeRain = new Behavior("EscapeRain", register: true);

		public static Behavior GetUnstuck = new Behavior("GetUnstuck", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SubBehavior : ExtEnum<SubBehavior>
	{
		public static SubBehavior None = new SubBehavior("None", register: true);

		public static SubBehavior TravelToWater = new SubBehavior("TravelToWater", register: true);

		public static SubBehavior TravelToIdle = new SubBehavior("TravelToIdle", register: true);

		public static SubBehavior TemporaryPanic = new SubBehavior("TemporaryPanic", register: true);

		public SubBehavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Barnacle realizedCreature;

	public Barnacle leader;

	public List<Barnacle> leading = new List<Barnacle>();

	public int invalidLeaderTime;

	public Vector2 leaderIdleSpot;

	public bool leaderIdleSpotWasDistant;

	public int leaderIdleSpotExpiry;

	public float groupDiscomfort;

	public float nextGroupDiscomfortTick;

	public Vector2? bestNarrowPos;

	public Vector2? blacklistIdlePos;

	public int bestNarrowScore;

	public int narrowAttempts;

	public bool nearNarrowPos;

	public Vector2? fgp;

	public float excitement;

	public int panic;

	public bool scattering;

	public bool idleSplash;

	public int temporaryPanic;

	public int lastDistressLength;

	public float hunger;

	public float fear;

	public float rainFear;

	public int safariCallCooldown;

	public int runAwayTime;

	public float runSpeed;

	public Tracker.CreatureRepresentation focusCreature;

	public WorldCoordinate? exploreCoordinate;

	public float currentUtility;

	public AIParams aiparams;

	public Behavior behavior;

	public SubBehavior subBehavior;

	public int inSubBehaviorTime;

	public int nextSubBehaviorTime;

	public Vector2? waterIdleSpot;

	public Vector2? panicSpot;

	public int nextPanicSpotShuffle;

	public int inWaterTime;

	public Vector2? floatGoalPos
	{
		get
		{
			return fgp;
		}
		set
		{
			fgp = value;
			if (value.HasValue)
			{
				creature.abstractAI.SetDestination(realizedCreature.room.GetWorldCoordinate(value.Value));
			}
		}
	}

	public float CombinedFear => Mathf.Max(fear, Mathf.Pow(rainFear, 3f));

	public float TravelUtility
	{
		get
		{
			if (!creature.abstractAI.WantToMigrate)
			{
				return 0f;
			}
			if (!base.pathFinder.CoordinateReachable(creature.abstractAI.MigrationDestination))
			{
				return 0f;
			}
			float num = realizedCreature.room.abstractRoom.AttractionValueForCreature(creature);
			float num2 = realizedCreature.room.world.GetAbstractRoom(creature.abstractAI.MigrationDestination).AttractionValueForCreature(creature);
			float num3 = Mathf.Lerp(0.99f, 0.1f, Mathf.Pow(num, 0.25f));
			if (num > num2)
			{
				return num3 *= 0.25f;
			}
			if (num == num2)
			{
				return num3 *= 0.5f;
			}
			return num3;
		}
	}

	public BarnacleAI(AbstractCreature creature, World world)
		: base(creature, world)
	{
		realizedCreature = creature.realizedCreature as Barnacle;
		realizedCreature.AI = this;
		aiparams = new AIParams();
		AddModule(new StandardPather(this, world, creature));
		AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10));
		if (aiparams.respondsToNoise)
		{
			AddModule(new NoiseTracker(this, base.tracker));
		}
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new StuckTracker(this, trackPastPositions: false, trackNotFollowingCurrentGoal: true));
		base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
		AddModule(new RelationshipTracker(this, base.tracker));
		AddModule(new UtilityComparer(this));
		base.pathFinder.stepsPerFrame = 20;
		base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
		if (base.noiseTracker != null)
		{
			base.utilityComparer.AddComparedModule(base.noiseTracker, null, 0.2f, 1.2f);
		}
		base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
		behavior = Behavior.Idle;
		subBehavior = SubBehavior.None;
		if (!realizedCreature.hasShell)
		{
			OnLoseShell();
		}
	}

	public override void NewRoom(Room room)
	{
		if (base.denFinder.GetDenPosition().HasValue && base.denFinder.GetDenPosition().Value.CompareDisregardingTile(base.pathFinder.GetDestination))
		{
			creature.abstractAI.SetDestination(new WorldCoordinate(room.abstractRoom.index, Random.Range(0, room.TileWidth), Random.Range(0, room.TileHeight), -1));
		}
		base.NewRoom(room);
	}

	public Behavior DetermineBehavior()
	{
		Behavior behavior = Behavior.Idle;
		AIModule aIModule = base.utilityComparer.HighestUtilityModule();
		currentUtility = base.utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			behavior = ((aIModule is ThreatTracker) ? Behavior.Flee : ((aIModule is RainTracker) ? Behavior.EscapeRain : ((aIModule is NoiseTracker) ? Behavior.InvestigateSound : ((!(aIModule is StuckTracker)) ? Behavior.Idle : Behavior.GetUnstuck))));
		}
		if (currentUtility < 0.05f)
		{
			currentUtility = 0.05f;
			behavior = Behavior.Idle;
		}
		if (temporaryPanic > 0)
		{
			behavior = Behavior.Flee;
		}
		if (behavior != Behavior.EscapeRain && behavior != Behavior.Flee && currentUtility < TravelUtility)
		{
			currentUtility = TravelUtility;
			behavior = Behavior.Travelling;
		}
		return behavior;
	}

	public void UpdateSubBehavior()
	{
		if (leader != null)
		{
			SwitchSubBehavior(SubBehavior.None);
			inSubBehaviorTime = 0;
			inWaterTime = 0;
			return;
		}
		if (subBehavior == SubBehavior.None)
		{
			if (Random.value < 0.5f)
			{
				SwitchSubBehavior(SubBehavior.TravelToWater);
			}
			else
			{
				SwitchSubBehavior(SubBehavior.TravelToIdle);
			}
		}
		else
		{
			inSubBehaviorTime++;
		}
		if (subBehavior == SubBehavior.TravelToWater)
		{
			if (realizedCreature.Submersion > 0f)
			{
				inWaterTime++;
			}
			if (!waterIdleSpot.HasValue)
			{
				waterIdleSpot = WaterIdleSpot();
			}
			if (!waterIdleSpot.HasValue && inSubBehaviorTime >= 40)
			{
				SwitchSubBehavior(SubBehavior.TravelToIdle);
			}
			else if (waterIdleSpot.HasValue && (inWaterTime >= 200 || inSubBehaviorTime > nextSubBehaviorTime))
			{
				SwitchSubBehavior(SubBehavior.TravelToIdle);
			}
		}
		else if (subBehavior == SubBehavior.TravelToIdle)
		{
			if (!bestNarrowPos.HasValue || (inSubBehaviorTime < 80 && bestNarrowScore > 5))
			{
				UpdateNarrowIdleSpot();
			}
			if (inSubBehaviorTime > nextSubBehaviorTime)
			{
				SwitchSubBehavior(SubBehavior.TravelToWater);
			}
		}
	}

	public void SwitchSubBehavior(SubBehavior newBehavior)
	{
		subBehavior = newBehavior;
		bestNarrowPos = null;
		waterIdleSpot = null;
		blacklistIdlePos = null;
		inSubBehaviorTime = 0;
		inWaterTime = 0;
		nextSubBehaviorTime = 0;
		if (realizedCreature.safariControlled)
		{
			return;
		}
		if (newBehavior == SubBehavior.TravelToIdle)
		{
			nextSubBehaviorTime = Random.Range(3600, 7200);
			if (leading.Count > 0)
			{
				realizedCreature.room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Assemble_Call, realizedCreature.mainBodyChunk);
			}
		}
		else if (newBehavior == SubBehavior.TravelToWater)
		{
			nextSubBehaviorTime = Random.Range(2800, 4000);
			if (leading.Count > 0)
			{
				realizedCreature.room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Travel_Call, realizedCreature.mainBodyChunk);
			}
		}
		else if (newBehavior == SubBehavior.TemporaryPanic)
		{
			nextSubBehaviorTime = temporaryPanic;
			ShufflePanicSpot(scattering);
		}
	}

	public override void Update()
	{
		base.Update();
		if (realizedCreature.room == null)
		{
			return;
		}
		LeadingUpdate();
		if (ModManager.MSC && realizedCreature.LickedByPlayer != null)
		{
			base.tracker.SeeCreature(realizedCreature.LickedByPlayer.abstractCreature);
		}
		if (panic > 0)
		{
			panic--;
		}
		if (runAwayTime > 0)
		{
			runAwayTime--;
		}
		if (temporaryPanic > 0)
		{
			if (subBehavior != SubBehavior.TemporaryPanic)
			{
				SwitchSubBehavior(SubBehavior.TemporaryPanic);
			}
			temporaryPanic--;
			nextPanicSpotShuffle--;
			nextSubBehaviorTime = temporaryPanic;
			if (!panicSpot.HasValue || nextPanicSpotShuffle <= 0)
			{
				ShufflePanicSpot(scattering);
			}
			if (temporaryPanic <= 0)
			{
				SwitchSubBehavior(SubBehavior.TravelToIdle);
			}
		}
		base.pathFinder.walkPastPointOfNoReturn = stranded || !base.denFinder.GetDenPosition().HasValue || !base.pathFinder.CoordinatePossibleToGetBackFrom(base.denFinder.GetDenPosition().Value);
		fear = ((base.threatTracker != null) ? base.utilityComparer.GetSmoothedNonWeightedUtility(base.threatTracker) : 0f);
		hunger = ((base.preyTracker != null) ? base.utilityComparer.GetSmoothedNonWeightedUtility(base.preyTracker) : 0f);
		rainFear = base.utilityComparer.GetSmoothedNonWeightedUtility(base.rainTracker);
		excitement = Mathf.Lerp(excitement, Mathf.Max(hunger, CombinedFear), 0.1f);
		if (fear > 0.8f)
		{
			lastDistressLength = Custom.IntClamp(lastDistressLength + 1, 0, 500);
		}
		else if (fear < 0.2f)
		{
			lastDistressLength = 0;
		}
		if (base.threatTracker != null)
		{
			((base.utilityComparer.GetUtilityTracker(base.threatTracker).smoother as FloatTweener.FloatTweenUpAndDown).down as FloatTweener.FloatTweenBasic).speed = 1f / ((float)(lastDistressLength + 20) * 3f);
		}
		currentUtility = base.utilityComparer.HighestUtility();
		behavior = DetermineBehavior();
		Act();
		if (runAwayTime > 0)
		{
			runSpeed = 1f;
		}
	}

	public void Act()
	{
		idleSplash = false;
		if (leader != null && behavior != Behavior.EscapeRain && behavior != Behavior.Flee && behavior != Behavior.GetUnstuck && !realizedCreature.safariControlled)
		{
			FollowLeader();
			return;
		}
		leaderIdleSpotExpiry = 0;
		if (behavior == Behavior.Idle || behavior == Behavior.Travelling)
		{
			UpdateSubBehavior();
		}
		nearNarrowPos = leading.Count > 0 && subBehavior == SubBehavior.TravelToIdle && bestNarrowPos.HasValue && Vector2.Distance(realizedCreature.mainBodyChunk.pos, bestNarrowPos.Value) < 150f;
		if (realizedCreature.Swimming && (behavior == Behavior.Idle || behavior == Behavior.Travelling))
		{
			if (subBehavior == SubBehavior.TravelToIdle && bestNarrowPos.HasValue && inSubBehaviorTime > 80)
			{
				creature.abstractAI.SetDestination(realizedCreature.room.GetWorldCoordinate(bestNarrowPos.Value));
				exploreCoordinate = null;
			}
			else if (exploreCoordinate.HasValue)
			{
				creature.abstractAI.SetDestination(exploreCoordinate.Value);
				if (Custom.ManhattanDistance(creature.pos, exploreCoordinate.Value) < 5 || (Random.value < 0.0125f && base.pathFinder.DoneMappingAccessibility && realizedCreature.room.aimap.TileAccessibleToCreature(creature.pos.x, creature.pos.y, creature.creatureTemplate) && !base.pathFinder.CoordinateReachableAndGetbackable(exploreCoordinate.Value)))
				{
					exploreCoordinate = null;
				}
			}
			else if (Custom.ManhattanDistance(creature.pos, base.pathFinder.GetDestination) < 5 || !base.pathFinder.CoordinateReachableAndGetbackable(base.pathFinder.GetDestination))
			{
				WorldCoordinate worldCoordinate = realizedCreature.room.GetWorldCoordinate(Custom.RestrictInRect(realizedCreature.mainBodyChunk.pos, new FloatRect(0f, 0f, realizedCreature.room.PixelWidth, realizedCreature.room.PixelHeight)) + Custom.RNV() * 200f);
				if (realizedCreature.room.IsPositionInsideBoundries(worldCoordinate.Tile) && base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate) && realizedCreature.room.GetTile(worldCoordinate).DeepWater && realizedCreature.room.VisualContact(creature.pos.Tile, worldCoordinate.Tile))
				{
					creature.abstractAI.SetDestination(worldCoordinate);
				}
			}
			if (Random.value < 1f / (exploreCoordinate.HasValue ? 1600f : 80f))
			{
				WorldCoordinate worldCoordinate2 = new WorldCoordinate(realizedCreature.room.abstractRoom.index, Random.Range(0, realizedCreature.room.TileWidth), Random.Range(0, realizedCreature.room.TileHeight), -1);
				if (realizedCreature.room.aimap.TileAccessibleToCreature(worldCoordinate2.Tile, creature.creatureTemplate) && base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate2) && realizedCreature.room.GetTile(worldCoordinate2).DeepWater)
				{
					exploreCoordinate = worldCoordinate2;
				}
			}
			if (Custom.DistLess(creature.pos, base.pathFinder.GetDestination, 20f) && realizedCreature.room.VisualContact(creature.pos, base.pathFinder.GetDestination))
			{
				floatGoalPos = realizedCreature.room.MiddleOfTile(base.pathFinder.GetDestination);
			}
			else
			{
				floatGoalPos = null;
			}
			return;
		}
		floatGoalPos = null;
		if (realizedCreature.safariControlled)
		{
			if (realizedCreature.inputWithDiagonals.HasValue)
			{
				Player.InputPackage value = realizedCreature.inputWithDiagonals.Value;
				if (!value.AnyDirectionalInput)
				{
					runSpeed = Mathf.Lerp(runSpeed, 0f, 0.8f);
				}
				else if (behavior == Behavior.Idle || behavior == Behavior.Travelling)
				{
					runSpeed = Mathf.Lerp(runSpeed, 0.75f, 0.5f);
				}
				else
				{
					runSpeed = Mathf.Lerp(runSpeed, 1f, 0.5f);
				}
				if (value.jmp)
				{
					temporaryPanic = Mathf.Max(10, temporaryPanic);
					runAwayTime = Mathf.Max(10, runAwayTime);
					ref Player.InputPackage? lastInputWithDiagonals = ref realizedCreature.lastInputWithDiagonals;
					if (lastInputWithDiagonals.HasValue && !lastInputWithDiagonals.GetValueOrDefault().jmp)
					{
						SetGroupDiscomfortTick(1.1f);
					}
				}
				if (safariCallCooldown > 0)
				{
					safariCallCooldown--;
				}
				else if (!value.jmp && value.thrw)
				{
					ref Player.InputPackage? lastInputWithDiagonals2 = ref realizedCreature.lastInputWithDiagonals;
					if (lastInputWithDiagonals2.HasValue && !lastInputWithDiagonals2.GetValueOrDefault().thrw)
					{
						SafariCall(value.AnyDirectionalInput);
						safariCallCooldown = 400;
					}
				}
				nearNarrowPos = value.AnyDirectionalInput;
			}
		}
		else if (behavior == Behavior.Idle || behavior == Behavior.Travelling)
		{
			bool flag = true;
			if (subBehavior == SubBehavior.TravelToIdle && bestNarrowPos.HasValue && inSubBehaviorTime > 80)
			{
				creature.abstractAI.SetDestination(realizedCreature.room.GetWorldCoordinate(bestNarrowPos.Value));
			}
			else if (subBehavior == SubBehavior.TravelToWater && waterIdleSpot.HasValue)
			{
				creature.abstractAI.SetDestination(realizedCreature.room.GetWorldCoordinate(waterIdleSpot.Value));
			}
			if (creature.pos.room == base.pathFinder.GetDestination.room)
			{
				if (!base.pathFinder.GetDestination.TileDefined || (!bestNarrowPos.HasValue && !waterIdleSpot.HasValue))
				{
					creature.abstractAI.SetDestination(creature.pos);
					bestNarrowPos = null;
					waterIdleSpot = null;
					flag = false;
				}
				else if (Custom.ManhattanDistance(creature.pos.Tile, base.pathFinder.GetEffectualDestination.Tile) < 3)
				{
					idleSplash = true;
					flag = false;
				}
			}
			if (flag)
			{
				runSpeed = Mathf.Lerp(runSpeed, 0.75f, 0.5f);
			}
			else
			{
				runSpeed = Mathf.Lerp(runSpeed, 0f, 0.8f);
			}
			focusCreature = null;
		}
		else if (behavior == Behavior.Flee)
		{
			if (temporaryPanic == 0 && base.threatTracker != null)
			{
				WorldCoordinate destination = base.threatTracker.FleeTo(creature.pos, 5, 20, fear > 1f / 3f);
				creature.abstractAI.SetDestination(destination);
				if (base.threatTracker.ThreatOfTile(creature.pos, accountThreatCreatureAccessibility: true) > 1f)
				{
					if (panic < 40)
					{
						panic = 40;
					}
					else
					{
						panic += 2;
					}
				}
				runSpeed = Mathf.Lerp(runSpeed, Mathf.Pow(CombinedFear, 0.1f), 0.5f);
				focusCreature = base.threatTracker.mostThreateningCreature;
			}
			else
			{
				if (panicSpot.HasValue)
				{
					creature.abstractAI.SetDestination(realizedCreature.room.GetWorldCoordinate(panicSpot.Value));
				}
				runSpeed = Mathf.Lerp(runSpeed, 1f, 0.5f);
			}
		}
		else if (behavior == Behavior.EscapeRain)
		{
			if (base.denFinder.GetDenPosition().HasValue)
			{
				creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
			}
			runSpeed = Mathf.Lerp(runSpeed, 1f, 0.1f);
		}
		else if (behavior == Behavior.InvestigateSound)
		{
			creature.abstractAI.SetDestination(base.noiseTracker.ExaminePos);
			runSpeed = Mathf.Lerp(runSpeed, 0.6f, 0.2f);
		}
		else if (behavior == Behavior.GetUnstuck)
		{
			creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
			runSpeed = Mathf.Max(runSpeed, base.stuckTracker.Utility());
		}
		if (base.noiseTracker != null)
		{
			base.noiseTracker.hearingSkill = Custom.LerpMap(runSpeed, 0f, 0.6f, 1.3f, 0.6f);
		}
	}

	public void SetGroupDiscomfortTick(float targetValue)
	{
		if (leader != null)
		{
			leader.AI.nextGroupDiscomfortTick = Mathf.Max(leader.AI.nextGroupDiscomfortTick, targetValue);
		}
		else
		{
			nextGroupDiscomfortTick = Mathf.Max(nextGroupDiscomfortTick, targetValue);
		}
	}

	public void SafariCall(bool idle)
	{
		List<Barnacle> list = new List<Barnacle>();
		if (leader == null)
		{
			Barnacle barnacle = null;
			float num = 0f;
			new List<Barnacle>();
			foreach (AbstractCreature creature in creature.Room.creatures)
			{
				if (!(creature.realizedCreature is Barnacle barnacle2) || !base.creature.CanTravelTogether(barnacle2.abstractCreature) || barnacle2 == realizedCreature || barnacle2.AI.leader == realizedCreature)
				{
					continue;
				}
				float num2 = Vector2.Distance(barnacle2.mainBodyChunk.pos, realizedCreature.mainBodyChunk.pos);
				if (num2 <= 800f)
				{
					if (barnacle2.AI.leading.Count > 0 && (barnacle == null || num2 < num))
					{
						barnacle = barnacle2;
					}
					else if (barnacle2.AI.leading.Count == 0 && barnacle2.AI.leader != null && (barnacle == null || num2 < num))
					{
						barnacle = barnacle2.AI.leader;
						num = num2;
					}
					else
					{
						list.Add(barnacle2);
					}
				}
			}
			if (barnacle != null)
			{
				leader = barnacle;
				barnacle.AI.leading.Add(realizedCreature);
			}
		}
		if (leader != null)
		{
			list.Clear();
			list.AddRange(leader.AI.leading);
			list.Add(leader);
			AbandonLeader();
		}
		foreach (Barnacle item in list)
		{
			if (item != realizedCreature)
			{
				item.AI.AbandonLeader();
				item.AI.leader = realizedCreature;
				leading.Add(item);
			}
		}
		if (idle)
		{
			realizedCreature.room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Assemble_Call, realizedCreature.mainBodyChunk);
		}
		else
		{
			realizedCreature.room.PlaySound(WatcherEnums.WatcherSoundID.Barnacle_Travel_Call, realizedCreature.mainBodyChunk);
		}
	}

	public void LeadingUpdate()
	{
		if (leader != null)
		{
			groupDiscomfort = leader.AI.groupDiscomfort;
			if (!CanBeALeader(leader) || (leader.room != null && leader.room.abstractRoom.index != realizedCreature.room.abstractRoom.index) || Vector2.Distance(leader.mainBodyChunk.pos, realizedCreature.mainBodyChunk.pos) > 800f)
			{
				invalidLeaderTime++;
			}
			else
			{
				invalidLeaderTime = 0;
			}
			if (!CanBeALeader(leader) || invalidLeaderTime >= 400)
			{
				AbandonLeader();
			}
		}
		else
		{
			if (nextGroupDiscomfortTick > 0f)
			{
				groupDiscomfort += nextGroupDiscomfortTick;
				nextGroupDiscomfortTick = 0f;
			}
			else
			{
				groupDiscomfort = Mathf.Max(0f, groupDiscomfort - 0.0125f);
			}
			if (groupDiscomfort >= 1f)
			{
				for (int i = 0; i < leading.Count; i++)
				{
					leading[i].AI.temporaryPanic = Mathf.Max(leading[i].AI.temporaryPanic, 400);
					leading[i].AI.runAwayTime = 400;
				}
				if (subBehavior != SubBehavior.TravelToIdle)
				{
					SwitchSubBehavior(SubBehavior.TravelToIdle);
					runAwayTime = 400;
					blacklistIdlePos = realizedCreature.mainBodyChunk.pos;
					UpdateNarrowIdleSpot();
					for (int j = 0; j < 50; j++)
					{
						if (bestNarrowPos.HasValue && bestNarrowScore < 10)
						{
							break;
						}
						UpdateNarrowIdleSpot();
					}
					if (bestNarrowPos.HasValue)
					{
						realizedCreature.TryJump(bestNarrowPos.Value);
					}
					else
					{
						realizedCreature.TryJump(realizedCreature.mainBodyChunk.pos + Custom.RNV() * 50f);
					}
				}
				for (int k = 0; k < leading.Count; k++)
				{
					leading[k].delayBeforeJumping = Random.Range(1, 120);
					if (bestNarrowPos.HasValue)
					{
						leading[k].jumpTarget = bestNarrowPos.Value;
					}
					else
					{
						leading[k].jumpTarget = leading[k].mainBodyChunk.pos + Custom.RNV() * 50f;
					}
				}
				groupDiscomfort = 0f;
			}
		}
		if (leading.Count != 0 || leader != null)
		{
			return;
		}
		Barnacle barnacle = null;
		float num = 0f;
		List<Barnacle> list = new List<Barnacle>();
		for (int l = 0; l < realizedCreature.room.physicalObjects.Length; l++)
		{
			for (int m = 0; m < realizedCreature.room.physicalObjects[l].Count; m++)
			{
				PhysicalObject physicalObject = realizedCreature.room.physicalObjects[l][m];
				if (!(physicalObject is Barnacle) || physicalObject == realizedCreature || !creature.CanTravelTogether((physicalObject as Barnacle).abstractCreature))
				{
					continue;
				}
				Barnacle barnacle2 = physicalObject as Barnacle;
				float num2 = Vector2.Distance(barnacle2.mainBodyChunk.pos, realizedCreature.mainBodyChunk.pos);
				if (num2 <= 800f)
				{
					if (barnacle2.AI.leading.Count > 0 && CanBeALeader(barnacle2) && (barnacle == null || num2 < num))
					{
						barnacle = barnacle2;
						num = num2;
					}
					else if (barnacle2.AI.leading.Count == 0 && barnacle2.AI.leader == null)
					{
						list.Add(barnacle2);
					}
				}
			}
		}
		if (barnacle != null)
		{
			leader = barnacle;
			barnacle.AI.leading.Add(realizedCreature);
		}
		else if (list.Count > 0 && CanBeALeader(realizedCreature))
		{
			for (int n = 0; n < list.Count; n++)
			{
				list[n].AI.leader = realizedCreature;
				leading.Add(list[n]);
			}
		}
	}

	public bool CanBeALeader(Barnacle b)
	{
		if (b.dead || !b.hasShell || b.slatedForDeletetion || b.AI.temporaryPanic > 0)
		{
			return false;
		}
		return true;
	}

	public void AbandonLeader()
	{
		if (leader == null)
		{
			return;
		}
		for (int num = leader.AI.leading.Count - 1; num >= 0; num--)
		{
			if (leader.AI.leading[num] == realizedCreature)
			{
				leader.AI.leading.RemoveAt(num);
			}
		}
		leader = null;
		invalidLeaderTime = 0;
	}

	public void OnLoseShell()
	{
		if (base.threatTracker == null)
		{
			AddModule(new ThreatTracker(this, 3));
			FloatTweener.FloatTween smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.0025f));
			base.utilityComparer.AddComparedModule(base.threatTracker, smoother, 0.4f, 1.1f);
			if (realizedCreature.room != null)
			{
				base.threatTracker.NewRoom(realizedCreature.room);
			}
		}
	}

	public void FollowLeader()
	{
		float num = Vector2.Distance(leader.mainBodyChunk.pos, realizedCreature.mainBodyChunk.pos);
		float num2 = Vector2.Distance(realizedCreature.mainBodyChunk.pos, realizedCreature.room.MiddleOfTile(base.pathFinder.GetEffectualDestination));
		WorldCoordinate worldCoordinate = leader.abstractCreature.pos;
		if (num > 100f && (leader.safariControlled || num2 > 40f))
		{
			runSpeed = Mathf.Lerp(runSpeed, Mathf.InverseLerp(40f, 180f, num), 0.8f);
			leaderIdleSpotExpiry = 0;
		}
		else
		{
			idleSplash = true;
			runSpeed = Mathf.Lerp(runSpeed, 0f, 0.8f);
			leaderIdleSpotExpiry--;
			if (leaderIdleSpotExpiry <= 0 || (leaderIdleSpotWasDistant && leader.AI.nearNarrowPos))
			{
				Vector2? vector = null;
				vector = ((!leader.AI.nearNarrowPos) ? IdleSpotNearLeader(300f, 600f) : IdleSpotNearLeader(50f, 100f));
				if (vector.HasValue)
				{
					leaderIdleSpot = vector.Value;
					if (leader.AI.nearNarrowPos)
					{
						leaderIdleSpotExpiry = Random.Range(200, 2000);
					}
					else
					{
						leaderIdleSpotExpiry = Random.Range(400, 2000);
					}
					leaderIdleSpotWasDistant = !leader.AI.nearNarrowPos;
				}
				else if (leader.AI.nearNarrowPos)
				{
					leaderIdleSpot = leader.mainBodyChunk.pos;
				}
				else
				{
					leaderIdleSpot = realizedCreature.mainBodyChunk.pos;
				}
			}
			worldCoordinate = realizedCreature.room.GetWorldCoordinate(leaderIdleSpot);
		}
		exploreCoordinate = worldCoordinate;
		floatGoalPos = realizedCreature.room.MiddleOfTile(worldCoordinate);
		creature.abstractAI.SetDestination(worldCoordinate);
	}

	public Vector2? IdleSpotNearLeader(float minRadius, float maxRadius)
	{
		if (leader != null)
		{
			for (int i = 0; i < 20; i++)
			{
				Vector2 vector = leader.mainBodyChunk.pos + Custom.RNV() * Random.Range(minRadius, maxRadius);
				if (realizedCreature.room.GetTile(vector) != null && base.pathFinder.CoordinateReachableAndGetbackable(realizedCreature.room.GetWorldCoordinate(vector)))
				{
					return vector;
				}
			}
		}
		return null;
	}

	public void ShufflePanicSpot(bool scattering)
	{
		this.scattering = scattering;
		if (scattering)
		{
			nextPanicSpotShuffle = Random.Range(40, 200);
		}
		else
		{
			nextPanicSpotShuffle = Random.Range(200, 1200);
		}
		IntVector2 pos = realizedCreature.room.RandomTile();
		for (int i = 0; i < 20; i++)
		{
			if (!base.pathFinder.CoordinateReachableAndGetbackable(realizedCreature.room.GetWorldCoordinate(pos)))
			{
				panicSpot = null;
				pos = realizedCreature.room.RandomTile();
				continue;
			}
			panicSpot = realizedCreature.room.MiddleOfTile(pos);
			break;
		}
	}

	public void UpdateNarrowIdleSpot()
	{
		IntVector2 pos = realizedCreature.room.RandomTile();
		for (int i = 0; i < 20; i++)
		{
			if (!base.pathFinder.CoordinateReachableAndGetbackable(realizedCreature.room.GetWorldCoordinate(pos)))
			{
				pos = realizedCreature.room.RandomTile();
			}
			else if (!blacklistIdlePos.HasValue || !(Vector2.Distance(realizedCreature.room.MiddleOfTile(pos), blacklistIdlePos.Value) < 400f))
			{
				break;
			}
		}
		if (!base.pathFinder.CoordinateReachableAndGetbackable(realizedCreature.room.GetWorldCoordinate(pos)))
		{
			return;
		}
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = true;
		int num = 0;
		int num2 = 0;
		if (realizedCreature.room.aimap.getAItile(pos).narrowSpace)
		{
			if (realizedCreature.room.GetTile(pos.x - 1, pos.y).Solid && realizedCreature.room.GetTile(pos.x + 1, pos.y).Solid)
			{
				num2 = 99;
				num = 99;
			}
		}
		else
		{
			for (int j = 0; j < 5; j++)
			{
				if (flag)
				{
					if (realizedCreature.room.GetTile(pos.x - j, pos.y).Solid)
					{
						flag = false;
					}
					else
					{
						num2++;
					}
				}
				if (flag2)
				{
					if (realizedCreature.room.GetTile(pos.x + j, pos.y).Solid)
					{
						flag2 = false;
					}
					else
					{
						num2++;
					}
				}
				if (flag3)
				{
					if (realizedCreature.room.GetTile(pos.x, pos.y + 1).Solid)
					{
						flag3 = false;
					}
					else
					{
						num++;
					}
				}
				if (flag4)
				{
					if (realizedCreature.room.GetTile(pos.x, pos.y - 1).Solid)
					{
						flag4 = false;
					}
					else
					{
						num++;
					}
				}
			}
		}
		int num3 = Mathf.Min(num2, num);
		if (num2 < num)
		{
			num3++;
			num3 *= 3;
		}
		if (blacklistIdlePos.HasValue && Vector2.Distance(realizedCreature.room.MiddleOfTile(pos), blacklistIdlePos.Value) < 400f)
		{
			num3++;
			num3 *= 25;
		}
		for (int k = 0; k < realizedCreature.room.abstractRoom.nodes.Length; k++)
		{
			if (realizedCreature.room.abstractRoom.nodes[k].type == AbstractRoomNode.Type.Exit && realizedCreature.room.aimap.ExitDistanceForCreature(pos, k, realizedCreature.abstractCreature.creatureTemplate) <= 5)
			{
				num3++;
				num3 *= 10;
				break;
			}
		}
		if (!bestNarrowPos.HasValue || num3 < bestNarrowScore)
		{
			bestNarrowScore = num3;
			bestNarrowPos = realizedCreature.room.MiddleOfTile(pos);
		}
	}

	public Vector2? WaterIdleSpot()
	{
		if (!realizedCreature.room.water)
		{
			return null;
		}
		float x = Random.Range(0f, realizedCreature.room.PixelWidth);
		float y = realizedCreature.room.FloatWaterLevel(new Vector2(x, realizedCreature.room.PixelHeight));
		IntVector2 tilePosition = realizedCreature.room.GetTilePosition(new Vector2(x, y));
		List<IntVector2> list = new List<IntVector2>();
		if (ModManager.DLCShared && realizedCreature.room.waterInverted)
		{
			for (int i = tilePosition.y; i < realizedCreature.room.TileHeight; i++)
			{
				IntVector2 intVector = new IntVector2(tilePosition.x, i);
				if (base.pathFinder.CoordinateReachableAndGetbackable(realizedCreature.room.GetWorldCoordinate(intVector)))
				{
					list.Add(intVector);
				}
			}
		}
		else
		{
			for (int j = 0; j <= tilePosition.y; j++)
			{
				IntVector2 intVector2 = new IntVector2(tilePosition.x, j);
				if (base.pathFinder.CoordinateReachableAndGetbackable(realizedCreature.room.GetWorldCoordinate(intVector2)))
				{
					list.Add(intVector2);
				}
			}
		}
		if (list.Count > 0)
		{
			IntVector2 pos = list[Random.Range(0, list.Count)];
			return realizedCreature.room.MiddleOfTile(pos);
		}
		return null;
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		return base.VisualScore(lookAtPoint, targetSpeed);
	}

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
	{
		for (int i = 0; i < base.tracker.CreaturesCount; i++)
		{
			Tracker.CreatureRepresentation rep = base.tracker.GetRep(i);
			if (Custom.ManhattanDistance(connection.startCoord, rep.BestGuessForPosition()) < 3)
			{
				if (Custom.ManhattanDistance(creature.pos, rep.BestGuessForPosition()) < 3)
				{
					cost.resistance += 100f;
				}
				else
				{
					cost.resistance += 10f + 90f * CreatureUnease(rep.representedCreature);
				}
			}
		}
		if (!realizedCreature.Swimming)
		{
			cost.resistance += Random.value;
		}
		return cost;
	}

	public override bool WantToStayInDenUntilEndOfCycle()
	{
		return behavior == Behavior.EscapeRain;
	}

	public float CreatureUnease(AbstractCreature crit)
	{
		if (crit.state.dead || realizedCreature.Template.CreatureRelationship(crit.creatureTemplate).type == CreatureTemplate.Relationship.Type.Ignores)
		{
			return 0f;
		}
		float num = 1f;
		num *= crit.creatureTemplate.bodySize;
		num *= Mathf.Lerp(realizedCreature.abstractCreature.creatureTemplate.CreatureRelationship(crit.creatureTemplate).intensity, 1f, 0.5f);
		if (crit.realizedCreature != null && !crit.realizedCreature.Consious)
		{
			num /= 20f;
		}
		if (realizedCreature.room.GetTile(realizedCreature.mainBodyChunk.pos).DeepWater && crit.creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.AirOnly)
		{
			num /= 3f;
		}
		else if (!realizedCreature.room.GetTile(realizedCreature.mainBodyChunk.pos).AnyWater && crit.creatureTemplate.waterRelationship == CreatureTemplate.WaterRelationship.WaterOnly)
		{
			num /= 3f;
		}
		return num;
	}

	public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (base.threatTracker != null && relationship.type == CreatureTemplate.Relationship.Type.Afraid)
		{
			return base.threatTracker;
		}
		if (base.preyTracker != null && (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Attacks))
		{
			return base.preyTracker;
		}
		return null;
	}

	public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
	{
		return creature.creatureTemplate.CreatureRelationship(dRelation.trackerRep.representedCreature.creatureTemplate);
	}

	public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
	{
		return null;
	}

	public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
	{
	}
}
