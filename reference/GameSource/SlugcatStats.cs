using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Expedition;
using JollyCoop;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class SlugcatStats
{
	public class Timeline : ExtEnum<Timeline>
	{
		public static Timeline Spear = new Timeline("Spear", register: true);

		public static Timeline Artificer = new Timeline("Artificer", register: true);

		public static Timeline Sofanthiel = new Timeline("Inv", register: true);

		public static Timeline Red = new Timeline("Red", register: true);

		public static Timeline Gourmand = new Timeline("Gourmand", register: true);

		public static Timeline White = new Timeline("White", register: true);

		public static Timeline Yellow = new Timeline("Yellow", register: true);

		public static Timeline Rivulet = new Timeline("Rivulet", register: true);

		public static Timeline Saint = new Timeline("Saint", register: true);

		public static Timeline Watcher = new Timeline("Watcher", register: true);

		public Timeline(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class Name : ExtEnum<Name>
	{
		public static Name White = new Name("White", register: true);

		public static Name Yellow = new Name("Yellow", register: true);

		public static Name Red = new Name("Red", register: true);

		public static Name Night = new Name("Night", register: true);

		public Name(string value, bool register = false)
			: base(value, register)
		{
		}

		[Obsolete]
		public static void Init()
		{
		}

		public static Name ArenaColor(int playerIndex)
		{
			return playerIndex switch
			{
				0 => White, 
				1 => Yellow, 
				2 => Red, 
				3 => Night, 
				_ => null, 
			};
		}
	}

	public Name name = Name.White;

	public int maxFood;

	public int foodToHibernate;

	public float runspeedFac = 1f;

	public float bodyWeightFac = 1f;

	public float generalVisibilityBonus;

	public float visualStealthInSneakMode = 0.5f;

	public float loudnessFac = 1f;

	public float lungsFac;

	public bool malnourished;

	public bool malnourishedByCreature;

	public int throwingSkill;

	public float poleClimbSpeedFac;

	public float corridorClimbSpeedFac;

	public float drownThreshold = 1f / 3f;

	public float swimForceFac = 1f;

	public float swimBoostCost = (ModManager.MMF ? 0.18f : 0.2f);

	public float swimBoostForce = 3f;

	public int swimBoostCooldown = 20;

	public float swimBoostMinAir = 0.5f;

	public static IntVector2 SlugcatFoodMeter(Name slugcat)
	{
		if (slugcat == Name.White)
		{
			return new IntVector2(7, 4);
		}
		if (slugcat == Name.Yellow)
		{
			return new IntVector2(5, 3);
		}
		if (slugcat == Name.Red)
		{
			return new IntVector2(9, 6);
		}
		if (ModManager.MSC)
		{
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
			{
				return new IntVector2(12, 12);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				return new IntVector2(6, 5);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				return new IntVector2(9, 6);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return new IntVector2(5, 4);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				return new IntVector2(10, 5);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				return new IntVector2(11, 7);
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
			{
				return new IntVector2(3, 2);
			}
		}
		if (ModManager.Watcher)
		{
			_ = slugcat == WatcherEnums.SlugcatStatsName.Watcher;
			return new IntVector2(7, 4);
		}
		return new IntVector2(7, 4);
	}

	public SlugcatStats(Name slugcat, bool malnourished)
	{
		this.malnourished = malnourished;
		name = slugcat;
		IntVector2 intVector = SlugcatFoodMeter(slugcat);
		maxFood = intVector.x;
		foodToHibernate = intVector.y;
		if (malnourished)
		{
			foodToHibernate = maxFood;
		}
		lungsFac = 1f;
		poleClimbSpeedFac = 1f;
		corridorClimbSpeedFac = 1f;
		if (slugcat == Name.White)
		{
			throwingSkill = 1;
		}
		else if (slugcat == Name.Yellow)
		{
			bodyWeightFac = 0.95f;
			generalVisibilityBonus = -0.1f;
			visualStealthInSneakMode = 0.6f;
			loudnessFac = 0.75f;
			lungsFac = ((ModManager.MMF && MMF.cfgMonkBreathTime.Value) ? 0.8f : 1.2f);
			throwingSkill = 0;
		}
		else if (slugcat == Name.Red)
		{
			runspeedFac = 1.2f;
			bodyWeightFac = 1.12f;
			generalVisibilityBonus = 0.1f;
			visualStealthInSneakMode = 0.3f;
			loudnessFac = 1.35f;
			throwingSkill = 2;
			poleClimbSpeedFac = 1.25f;
			corridorClimbSpeedFac = 1.2f;
		}
		if (ModManager.MSC)
		{
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				bodyWeightFac = 1.35f;
				poleClimbSpeedFac = 0.8f;
				corridorClimbSpeedFac = 0.86f;
				throwingSkill = 2;
				loudnessFac = 1.5f;
				generalVisibilityBonus = 0.3f;
				visualStealthInSneakMode = 0.2f;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				bodyWeightFac = 0.95f;
				throwingSkill = 1;
				lungsFac = 0.15f;
				runspeedFac = 1.75f;
				poleClimbSpeedFac = 1.8f;
				corridorClimbSpeedFac = 1.6f;
				swimBoostCost = 0.025f;
				swimBoostCooldown = 10;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				runspeedFac = 1.2f;
				bodyWeightFac = 1.12f;
				generalVisibilityBonus = 0.1f;
				visualStealthInSneakMode = 0.3f;
				loudnessFac = 1.35f;
				throwingSkill = 2;
				poleClimbSpeedFac = 1.25f;
				corridorClimbSpeedFac = 1.2f;
				swimBoostCost = 0.015f;
				swimBoostForce = 6f;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				throwingSkill = 0;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				runspeedFac = 1.2f;
				bodyWeightFac = 0.85f;
				generalVisibilityBonus = 0.1f;
				visualStealthInSneakMode = 0.3f;
				loudnessFac = 1.35f;
				throwingSkill = 2;
				poleClimbSpeedFac = 1.25f;
				corridorClimbSpeedFac = 1.2f;
			}
			else if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
			{
				bodyWeightFac = 0.65f;
				generalVisibilityBonus = -0.2f;
				visualStealthInSneakMode = 0.6f;
				loudnessFac = 0.5f;
				lungsFac = 0.8f;
				throwingSkill = 0;
				poleClimbSpeedFac = 0.8f;
				corridorClimbSpeedFac = 0.8f;
				runspeedFac = 0.8f;
			}
			else if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				throwingSkill = 0;
			}
		}
		if (ModManager.Watcher && slugcat == WatcherEnums.SlugcatStatsName.Watcher)
		{
			throwingSkill = 1;
			visualStealthInSneakMode = 0.5f;
			lungsFac = 0.8f;
		}
		if (ModManager.Expedition && ModManager.MSC && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
		{
			lungsFac = 0.15f;
			runspeedFac = 1.75f;
			poleClimbSpeedFac = 1.8f;
			corridorClimbSpeedFac = 1.6f;
		}
		if (malnourished)
		{
			throwingSkill = 0;
			if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				bodyWeightFac = 0.91f;
				runspeedFac = 1.27f;
				poleClimbSpeedFac = 1.1f;
				corridorClimbSpeedFac = 1.2f;
			}
			else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				bodyWeightFac = 1.15f;
				runspeedFac = 0.875f;
				poleClimbSpeedFac = 0.75f;
				corridorClimbSpeedFac = 0.81f;
				throwingSkill = 2;
			}
			else if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
			{
				poleClimbSpeedFac = 0.75f;
				corridorClimbSpeedFac = 0.7f;
				runspeedFac = 0.7f;
			}
			else if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
			{
				runspeedFac = 1.27f;
				poleClimbSpeedFac = 1.1f;
				corridorClimbSpeedFac = 1.2f;
			}
			else
			{
				bodyWeightFac = Mathf.Min(bodyWeightFac, 0.9f);
				runspeedFac = 0.875f;
				poleClimbSpeedFac = 0.8f;
				corridorClimbSpeedFac = 0.86f;
			}
		}
	}

	public static int NourishmentOfObjectEaten(Name slugcatIndex, IPlayerEdible eatenobject)
	{
		if (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			if (eatenobject is JellyFish || eatenobject is Centipede || eatenobject is Fly || eatenobject is VultureGrub || eatenobject is SmallNeedleWorm || eatenobject is Hazer)
			{
				return -1;
			}
			if (ModManager.Watcher && (eatenobject is Rat || eatenobject is Frog || eatenobject is Tardigrade || eatenobject is Barnacle || eatenobject is BoxWorm.Larva || eatenobject is SandGrub))
			{
				return -1;
			}
		}
		int num = 0;
		if (slugcatIndex == Name.Red || (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Artificer))
		{
			bool flag = true;
			if (eatenobject is Centipede || eatenobject is VultureGrub || eatenobject is Hazer || eatenobject is EggBugEgg || eatenobject is SmallNeedleWorm || eatenobject is JellyFish)
			{
				flag = false;
			}
			if (ModManager.Watcher && (eatenobject is Frog || eatenobject is Tardigrade || eatenobject is Barnacle || eatenobject is BoxWorm.Larva || eatenobject is SandGrub))
			{
				flag = false;
			}
			num = ((!flag) ? (num + 4 * eatenobject.FoodPoints) : (num + eatenobject.FoodPoints));
		}
		else if (!ModManager.MSC || slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			num = ((eatenobject is DangleFruit && (eatenobject as DangleFruit).AbstrConsumable.rotted) ? (num + 2) : ((!(eatenobject is GlowWeed)) ? (num + 4 * eatenobject.FoodPoints) : (num + 2)));
		}
		return num;
	}

	public static bool PearlsGivePassageProgress(StoryGameSession session)
	{
		if (session.saveState.deathPersistentSaveData.theMark)
		{
			if (ModManager.MSC && session.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return false;
			}
			if (ModManager.Expedition && session.game.rainWorld.ExpeditionMode)
			{
				return true;
			}
			if (ModManager.Watcher && session.saveStateNumber == WatcherEnums.SlugcatStatsName.Watcher)
			{
				return true;
			}
			if (session.saveStateNumber == Name.Red || (ModManager.MSC && (session.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer || session.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Spear || session.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)))
			{
				return true;
			}
			if (session.saveState.miscWorldSaveData.EverMetMoon && session.saveState.miscWorldSaveData.SLOracleState.SpeakingTerms)
			{
				return true;
			}
		}
		return false;
	}

	[Obsolete("Use timeline parameter function instead.")]
	public static float SpearSpawnExplosiveRandomChance(Name index)
	{
		return SpearSpawnExplosiveRandomChance(SlugcatToTimeline(index));
	}

	public static float SpearSpawnExplosiveRandomChance(Timeline index)
	{
		if (AtOrBeforeTimeline(index, Timeline.Artificer))
		{
			return 0.012f;
		}
		if (AtOrBeforeTimeline(index, Timeline.Red))
		{
			return 0.008f;
		}
		if (index == Timeline.Gourmand)
		{
			return 0.001f;
		}
		return 0f;
	}

	[Obsolete("Use timeline parameter function instead.")]
	public static float SpearSpawnElectricRandomChance(Name index)
	{
		return SpearSpawnElectricRandomChance(SlugcatToTimeline(index));
	}

	public static float SpearSpawnElectricRandomChance(Timeline index)
	{
		if (!ModManager.MSC)
		{
			return 0f;
		}
		if (index == Timeline.Spear)
		{
			return 0.082f;
		}
		if (index == Timeline.Artificer)
		{
			return 0.065f;
		}
		if (AtOrBeforeTimeline(index, Timeline.Red))
		{
			return 0.011f;
		}
		if (index == Timeline.Gourmand)
		{
			return 0.008f;
		}
		return 0f;
	}

	[Obsolete("Use timeline parameter function instead.")]
	public static float SpearSpawnModifier(Name index, float originalSpearChance)
	{
		return SpearSpawnModifier(SlugcatToTimeline(index), originalSpearChance);
	}

	public static float SpearSpawnModifier(Timeline index, float originalSpearChance)
	{
		if (index == Timeline.Red)
		{
			return Mathf.Pow(originalSpearChance, 0.85f);
		}
		if (index == Timeline.Spear)
		{
			return Mathf.Pow(originalSpearChance, 0.9f);
		}
		if (index == Timeline.Artificer)
		{
			return Mathf.Pow(originalSpearChance, 0.8f);
		}
		if (index == Timeline.Gourmand)
		{
			return Mathf.Pow(originalSpearChance, 0.95f);
		}
		if (index == Timeline.Saint)
		{
			return Mathf.Pow(originalSpearChance, 1.4f);
		}
		return originalSpearChance;
	}

	public static bool AutoGrabBatflys(Name slugcatNum)
	{
		if (!ModManager.MMF)
		{
			return true;
		}
		if (slugcatNum == Name.Red && !MMF.cfgHunterBatflyAutograb.Value)
		{
			return true;
		}
		if (slugcatNum == Name.Red || (ModManager.MSC && (slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Artificer || slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Saint || slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Spear)))
		{
			return false;
		}
		return true;
	}

	public static int SlugcatStartingKarma(Name slugcatNum)
	{
		if (ModManager.MSC && slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			return 0;
		}
		if (ModManager.MSC && slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			return 1;
		}
		return 4;
	}

	public static bool SlugcatCanMaul(Name slugcatNum)
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		if (!(slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Artificer))
		{
			return slugcatNum == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel;
		}
		return true;
	}

	public static string getSlugcatName(Name i)
	{
		if (i == null)
		{
			return "Random";
		}
		if (i == Name.White)
		{
			return "Survivor";
		}
		if (i == Name.Yellow)
		{
			return "Monk";
		}
		if (i == Name.Red)
		{
			return "Hunter";
		}
		if (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Spear)
		{
			return "Spearmaster";
		}
		return i.value;
	}

	public static List<string> SlugcatStoryRegions(Name i)
	{
		string[] source = ((ModManager.Watcher && i == WatcherEnums.SlugcatStatsName.Watcher) ? new string[24]
		{
			"WSKA", "WSKB", "WRFA", "WRRA", "WPGA", "WARF", "WSKD", "WMPA", "WTDA", "WTDB",
			"WARG", "WARD", "WBLA", "WARE", "WRFB", "WSKC", "WVWA", "WVWB", "WARB", "WARC",
			"WPTA", "WARA", "WAUA", "WORA"
		} : ((!ModManager.MSC) ? new string[12]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "SL", "SI", "LF", "UW",
			"SS", "SB"
		} : ((i == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) ? new string[14]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "SL", "SI", "LF",
			"UW", "RM", "SB", "MS"
		} : ((i == MoreSlugcatsEnums.SlugcatStatsName.Artificer) ? new string[14]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "LM", "SI", "LF",
			"UW", "SS", "SB", "LC"
		} : ((i == MoreSlugcatsEnums.SlugcatStatsName.Saint) ? new string[12]
		{
			"SU", "HI", "UG", "CC", "GW", "VS", "CL", "SL", "SI", "LF",
			"SB", "HR"
		} : ((i == MoreSlugcatsEnums.SlugcatStatsName.Spear) ? new string[14]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "LM", "SI", "LF",
			"UW", "SS", "SB", "DM"
		} : ((!(i == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)) ? new string[13]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "SL", "SI", "LF",
			"UW", "SS", "SB"
		} : new string[14]
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "VS", "SL", "SI", "LF",
			"UW", "SS", "SB", "OE"
		})))))));
		return source.ToList();
	}

	[Obsolete("Renamed to SlugcatStoryRegions")]
	public static string[] getSlugcatStoryRegions(Name i)
	{
		return SlugcatStoryRegions(i).ToArray();
	}

	public static List<string> SlugcatOptionalRegions(Name i)
	{
		string[] source = ((ModManager.Watcher && i == WatcherEnums.SlugcatStatsName.Watcher) ? new string[9] { "HI", "SU", "CC", "SH", "WHIR", "WSUR", "WDSR", "WGWR", "WSSR" } : ((!ModManager.MSC) ? new string[0] : ((!(i == Name.White) && !(i == Name.Yellow)) ? ((!(i == Name.Red) && !(i == MoreSlugcatsEnums.SlugcatStatsName.Saint) && !(i == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)) ? new string[0] : new string[1] { "MS" }) : new string[2] { "OE", "MS" })));
		return source.ToList();
	}

	[Obsolete("Renamed to SlugcatOptionalRegions")]
	public static string[] getSlugcatOptionalRegions(Name i)
	{
		return SlugcatOptionalRegions(i).ToArray();
	}

	public static LinkedList<Timeline> SlugcatTimelineOrder()
	{
		return new LinkedList<Timeline>(new Timeline[9]
		{
			Timeline.Spear,
			Timeline.Artificer,
			Timeline.Sofanthiel,
			Timeline.Red,
			Timeline.Gourmand,
			Timeline.White,
			Timeline.Yellow,
			Timeline.Rivulet,
			Timeline.Saint
		});
	}

	public static Timeline SlugcatToTimeline(Name slugcat)
	{
		if (slugcat == Name.Red)
		{
			return Timeline.Red;
		}
		if (slugcat == Name.White)
		{
			return Timeline.White;
		}
		if (slugcat == Name.Yellow)
		{
			return Timeline.Yellow;
		}
		if (ModManager.MSC)
		{
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				return Timeline.Spear;
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				return Timeline.Artificer;
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
			{
				return Timeline.Sofanthiel;
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				return Timeline.Gourmand;
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				return Timeline.Rivulet;
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return Timeline.Saint;
			}
		}
		if (ModManager.Watcher && slugcat == WatcherEnums.SlugcatStatsName.Watcher)
		{
			return Timeline.Watcher;
		}
		return Timeline.White;
	}

	public static bool AtOrBeforeTimeline(Timeline currentTime, Timeline compareTime)
	{
		if (currentTime == null || compareTime == null)
		{
			return false;
		}
		LinkedList<Timeline> linkedList = SlugcatTimelineOrder();
		int num = -1;
		int num2 = -1;
		int num3 = 0;
		for (LinkedListNode<Timeline> linkedListNode = linkedList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value == currentTime)
			{
				num = num3;
			}
			if (linkedListNode.Value == compareTime)
			{
				num2 = num3;
			}
			if (num >= 0 && num2 >= 0)
			{
				break;
			}
			num3++;
		}
		if (num == -1 || num2 == -1)
		{
			return false;
		}
		return num <= num2;
	}

	public static bool AtOrAfterTimeline(Timeline currentTime, Timeline compareTime)
	{
		if (currentTime == null || compareTime == null)
		{
			return false;
		}
		LinkedList<Timeline> linkedList = SlugcatTimelineOrder();
		int num = -1;
		int num2 = -1;
		int num3 = 0;
		for (LinkedListNode<Timeline> linkedListNode = linkedList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value == currentTime)
			{
				num = num3;
			}
			if (linkedListNode.Value == compareTime)
			{
				num2 = num3;
			}
			if (num >= 0 && num2 >= 0)
			{
				break;
			}
			num3++;
		}
		if (num == -1 || num2 == -1)
		{
			return false;
		}
		return num >= num2;
	}

	[Obsolete("Renamed to SlugcatTimelineOrder")]
	public static Timeline[] getSlugcatTimelineOrder()
	{
		return SlugcatTimelineOrder().ToArray();
	}

	public static bool HiddenOrUnplayableSlugcat(Name i)
	{
		if (i == Name.Night)
		{
			return true;
		}
		if (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
		{
			return true;
		}
		if (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
		{
			return true;
		}
		if (ModManager.JollyCoop && i == JollyEnums.Name.JollyPlayer1)
		{
			return true;
		}
		if (ModManager.JollyCoop && i == JollyEnums.Name.JollyPlayer2)
		{
			return true;
		}
		if (ModManager.JollyCoop && i == JollyEnums.Name.JollyPlayer3)
		{
			return true;
		}
		if (ModManager.JollyCoop && i == JollyEnums.Name.JollyPlayer4)
		{
			return true;
		}
		return false;
	}

	public static bool SlugcatUnlocked(Name i, RainWorld rainWorld)
	{
		if (ModManager.MSC && global::MoreSlugcats.MoreSlugcats.chtUnlockCampaigns.Value)
		{
			return true;
		}
		if (ModManager.Watcher && i == WatcherEnums.SlugcatStatsName.Watcher && global::Watcher.Watcher.chtUnlockCampaign.Value)
		{
			return true;
		}
		if (ModManager.MMF && i == Name.Red && MMF.chtUnlockHunter.Value)
		{
			return true;
		}
		if (ModManager.Expedition && rainWorld.ExpeditionMode && ExpeditionGame.unlockedExpeditionSlugcats.Contains(i))
		{
			return true;
		}
		bool num = ModManager.MSC && (File.Exists((Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "unlockmsc.txt").ToLowerInvariant()) || rainWorld.setup.unlockMSCCharacters);
		bool redUnlocked = rainWorld.progression.miscProgressionData.redUnlocked;
		bool flag = ModManager.MSC && (rainWorld.progression.miscProgressionData.beaten_Hunter || rainWorld.progression.miscProgressionData.beaten_Gourmand || rainWorld.progression.miscProgressionData.beaten_Artificer);
		bool flag2 = ModManager.MSC && rainWorld.progression.miscProgressionData.beaten_Rivulet && rainWorld.progression.miscProgressionData.beaten_SpearMaster;
		if (num)
		{
			return true;
		}
		if (ModManager.MSC && rainWorld.setup.betaTestSlugcat == i)
		{
			return true;
		}
		if (ModManager.Watcher && i == WatcherEnums.SlugcatStatsName.Watcher && redUnlocked)
		{
			return true;
		}
		if ((i == Name.Red || (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Gourmand) || (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Artificer)) && redUnlocked)
		{
			return true;
		}
		if (ModManager.MSC && (i == MoreSlugcatsEnums.SlugcatStatsName.Spear || i == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) && flag)
		{
			return true;
		}
		if (ModManager.MSC && i == MoreSlugcatsEnums.SlugcatStatsName.Saint && flag2)
		{
			return true;
		}
		if (i != Name.Red && (!ModManager.MSC || (i != MoreSlugcatsEnums.SlugcatStatsName.Gourmand && i != MoreSlugcatsEnums.SlugcatStatsName.Artificer && i != MoreSlugcatsEnums.SlugcatStatsName.Rivulet && i != MoreSlugcatsEnums.SlugcatStatsName.Spear && i != MoreSlugcatsEnums.SlugcatStatsName.Saint)) && (!ModManager.Watcher || i != WatcherEnums.SlugcatStatsName.Watcher))
		{
			return true;
		}
		return false;
	}

	public static bool IsSlugcatFromMSC(Name i)
	{
		if (!(i.value == "Rivulet") && !(i.value == "Artificer") && !(i.value == "Saint") && !(i.value == "Spear") && !(i.value == "Gourmand"))
		{
			return i.value == "Inv";
		}
		return true;
	}
}
