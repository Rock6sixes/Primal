using System.Collections.Generic;
using System.IO;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class MultiplayerUnlocks
{
	public class LevelUnlockID : ExtEnum<LevelUnlockID>
	{
		public static LevelUnlockID Default = new LevelUnlockID("Default", register: true);

		public static LevelUnlockID Hidden = new LevelUnlockID("Hidden", register: true);

		public static LevelUnlockID SU = new LevelUnlockID("SU", register: true);

		public static LevelUnlockID HI = new LevelUnlockID("HI", register: true);

		public static LevelUnlockID CC = new LevelUnlockID("CC", register: true);

		public static LevelUnlockID GW = new LevelUnlockID("GW", register: true);

		public static LevelUnlockID SL = new LevelUnlockID("SL", register: true);

		public static LevelUnlockID SH = new LevelUnlockID("SH", register: true);

		public static LevelUnlockID DS = new LevelUnlockID("DS", register: true);

		public static LevelUnlockID SI = new LevelUnlockID("SI", register: true);

		public static LevelUnlockID LF = new LevelUnlockID("LF", register: true);

		public static LevelUnlockID UW = new LevelUnlockID("UW", register: true);

		public static LevelUnlockID SB = new LevelUnlockID("SB", register: true);

		public static LevelUnlockID SS = new LevelUnlockID("SS", register: true);

		public LevelUnlockID(string value, bool register = false)
			: base(value, register)
		{
		}

		public static void Init()
		{
			ExtEnum<LevelUnlockID>.values = new ExtEnumType();
			ExtEnum<LevelUnlockID>.values.AddEntry(Default.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(Hidden.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(SU.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(HI.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(CC.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(GW.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(SL.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(SH.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(DS.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(SI.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(LF.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(UW.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(SB.value);
			ExtEnum<LevelUnlockID>.values.AddEntry(SS.value);
		}
	}

	public class SandboxUnlockID : ExtEnum<SandboxUnlockID>
	{
		public static SandboxUnlockID Slugcat = new SandboxUnlockID("Slugcat", register: true);

		public static SandboxUnlockID GreenLizard = new SandboxUnlockID("GreenLizard", register: true);

		public static SandboxUnlockID PinkLizard = new SandboxUnlockID("PinkLizard", register: true);

		public static SandboxUnlockID BlueLizard = new SandboxUnlockID("BlueLizard", register: true);

		public static SandboxUnlockID WhiteLizard = new SandboxUnlockID("WhiteLizard", register: true);

		public static SandboxUnlockID BlackLizard = new SandboxUnlockID("BlackLizard", register: true);

		public static SandboxUnlockID YellowLizard = new SandboxUnlockID("YellowLizard", register: true);

		public static SandboxUnlockID CyanLizard = new SandboxUnlockID("CyanLizard", register: true);

		public static SandboxUnlockID RedLizard = new SandboxUnlockID("RedLizard", register: true);

		public static SandboxUnlockID Salamander = new SandboxUnlockID("Salamander", register: true);

		public static SandboxUnlockID Fly = new SandboxUnlockID("Fly", register: true);

		public static SandboxUnlockID CicadaA = new SandboxUnlockID("CicadaA", register: true);

		public static SandboxUnlockID CicadaB = new SandboxUnlockID("CicadaB", register: true);

		public static SandboxUnlockID Snail = new SandboxUnlockID("Snail", register: true);

		public static SandboxUnlockID Leech = new SandboxUnlockID("Leech", register: true);

		public static SandboxUnlockID SeaLeech = new SandboxUnlockID("SeaLeech", register: true);

		public static SandboxUnlockID PoleMimic = new SandboxUnlockID("PoleMimic", register: true);

		public static SandboxUnlockID TentaclePlant = new SandboxUnlockID("TentaclePlant", register: true);

		public static SandboxUnlockID Scavenger = new SandboxUnlockID("Scavenger", register: true);

		public static SandboxUnlockID VultureGrub = new SandboxUnlockID("VultureGrub", register: true);

		public static SandboxUnlockID Vulture = new SandboxUnlockID("Vulture", register: true);

		public static SandboxUnlockID KingVulture = new SandboxUnlockID("KingVulture", register: true);

		public static SandboxUnlockID SmallCentipede = new SandboxUnlockID("SmallCentipede", register: true);

		public static SandboxUnlockID MediumCentipede = new SandboxUnlockID("MediumCentipede", register: true);

		public static SandboxUnlockID BigCentipede = new SandboxUnlockID("BigCentipede", register: true);

		public static SandboxUnlockID RedCentipede = new SandboxUnlockID("RedCentipede", register: true);

		public static SandboxUnlockID Centiwing = new SandboxUnlockID("Centiwing", register: true);

		public static SandboxUnlockID TubeWorm = new SandboxUnlockID("TubeWorm", register: true);

		public static SandboxUnlockID Hazer = new SandboxUnlockID("Hazer", register: true);

		public static SandboxUnlockID LanternMouse = new SandboxUnlockID("LanternMouse", register: true);

		public static SandboxUnlockID Spider = new SandboxUnlockID("Spider", register: true);

		public static SandboxUnlockID BigSpider = new SandboxUnlockID("BigSpider", register: true);

		public static SandboxUnlockID SpitterSpider = new SandboxUnlockID("SpitterSpider", register: true);

		public static SandboxUnlockID MirosBird = new SandboxUnlockID("MirosBird", register: true);

		public static SandboxUnlockID BrotherLongLegs = new SandboxUnlockID("BrotherLongLegs", register: true);

		public static SandboxUnlockID DaddyLongLegs = new SandboxUnlockID("DaddyLongLegs", register: true);

		public static SandboxUnlockID Deer = new SandboxUnlockID("Deer", register: true);

		public static SandboxUnlockID EggBug = new SandboxUnlockID("EggBug", register: true);

		public static SandboxUnlockID DropBug = new SandboxUnlockID("DropBug", register: true);

		public static SandboxUnlockID BigNeedleWorm = new SandboxUnlockID("BigNeedleWorm", register: true);

		public static SandboxUnlockID SmallNeedleWorm = new SandboxUnlockID("SmallNeedleWorm", register: true);

		public static SandboxUnlockID JetFish = new SandboxUnlockID("JetFish", register: true);

		public static SandboxUnlockID BigEel = new SandboxUnlockID("BigEel", register: true);

		public static SandboxUnlockID Rock = new SandboxUnlockID("Rock", register: true);

		public static SandboxUnlockID Spear = new SandboxUnlockID("Spear", register: true);

		public static SandboxUnlockID FireSpear = new SandboxUnlockID("FireSpear", register: true);

		public static SandboxUnlockID ScavengerBomb = new SandboxUnlockID("ScavengerBomb", register: true);

		public static SandboxUnlockID SporePlant = new SandboxUnlockID("SporePlant", register: true);

		public static SandboxUnlockID Lantern = new SandboxUnlockID("Lantern", register: true);

		public static SandboxUnlockID FlyLure = new SandboxUnlockID("FlyLure", register: true);

		public static SandboxUnlockID Mushroom = new SandboxUnlockID("Mushroom", register: true);

		public static SandboxUnlockID FlareBomb = new SandboxUnlockID("FlareBomb", register: true);

		public static SandboxUnlockID PuffBall = new SandboxUnlockID("PuffBall", register: true);

		public static SandboxUnlockID WaterNut = new SandboxUnlockID("WaterNut", register: true);

		public static SandboxUnlockID FirecrackerPlant = new SandboxUnlockID("FirecrackerPlant", register: true);

		public static SandboxUnlockID DangleFruit = new SandboxUnlockID("DangleFruit", register: true);

		public static SandboxUnlockID JellyFish = new SandboxUnlockID("JellyFish", register: true);

		public static SandboxUnlockID BubbleGrass = new SandboxUnlockID("BubbleGrass", register: true);

		public static SandboxUnlockID SlimeMold = new SandboxUnlockID("SlimeMold", register: true);

		public static SandboxUnlockID SeedCob = new SandboxUnlockID("SeedCob", register: true);

		public SandboxUnlockID(string value, bool register = false)
			: base(value, register)
		{
		}

		public static void Init()
		{
			ExtEnum<SandboxUnlockID>.values = new ExtEnumType();
			ExtEnum<SandboxUnlockID>.values.AddEntry(Slugcat.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(GreenLizard.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(PinkLizard.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(BlueLizard.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(WhiteLizard.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(BlackLizard.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(YellowLizard.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(CyanLizard.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(RedLizard.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Salamander.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Fly.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(CicadaA.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(CicadaB.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Snail.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Leech.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(SeaLeech.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(PoleMimic.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(TentaclePlant.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Scavenger.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(VultureGrub.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Vulture.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(KingVulture.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(SmallCentipede.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(MediumCentipede.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(BigCentipede.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(RedCentipede.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Centiwing.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(TubeWorm.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Hazer.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(LanternMouse.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Spider.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(BigSpider.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(SpitterSpider.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(MirosBird.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(BrotherLongLegs.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(DaddyLongLegs.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Deer.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(EggBug.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(DropBug.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(BigNeedleWorm.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(SmallNeedleWorm.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(JetFish.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(BigEel.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Rock.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Spear.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(FireSpear.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(ScavengerBomb.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(SporePlant.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Lantern.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(FlyLure.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(Mushroom.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(FlareBomb.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(PuffBall.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(WaterNut.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(FirecrackerPlant.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(DangleFruit.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(JellyFish.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(BubbleGrass.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(SlimeMold.value);
			ExtEnum<SandboxUnlockID>.values.AddEntry(SeedCob.value);
		}
	}

	public class Unlock
	{
		public LevelUnlockID ID;

		public List<string> levels;

		public List<CreatureTemplate.Type> creatures;

		public Unlock(LevelUnlockID ID)
		{
			this.ID = ID;
			levels = new List<string>();
			creatures = UnlockedCritters(ID);
		}
	}

	public class SlugcatUnlockID : ExtEnum<SlugcatUnlockID>
	{
		public static SlugcatUnlockID Hunter = new SlugcatUnlockID("Hunter", register: true);

		public static SlugcatUnlockID Gourmand = new SlugcatUnlockID("Gourmand", register: true);

		public static SlugcatUnlockID Rivulet = new SlugcatUnlockID("Rivulet", register: true);

		public static SlugcatUnlockID Saint = new SlugcatUnlockID("Saint", register: true);

		public static SlugcatUnlockID Artificer = new SlugcatUnlockID("Artificer", register: true);

		public static SlugcatUnlockID Spearmaster = new SlugcatUnlockID("Spearmaster", register: true);

		public SlugcatUnlockID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SafariUnlockID : ExtEnum<SafariUnlockID>
	{
		public static SafariUnlockID SU = new SafariUnlockID("SU", register: true);

		public static SafariUnlockID HI = new SafariUnlockID("HI", register: true);

		public static SafariUnlockID GW = new SafariUnlockID("GW", register: true);

		public static SafariUnlockID DS = new SafariUnlockID("DS", register: true);

		public static SafariUnlockID SH = new SafariUnlockID("SH", register: true);

		public static SafariUnlockID VS = new SafariUnlockID("VS", register: true);

		public static SafariUnlockID SL = new SafariUnlockID("SL", register: true);

		public static SafariUnlockID CC = new SafariUnlockID("CC", register: true);

		public static SafariUnlockID UW = new SafariUnlockID("UW", register: true);

		public static SafariUnlockID SS = new SafariUnlockID("SS", register: true);

		public static SafariUnlockID SI = new SafariUnlockID("SI", register: true);

		public static SafariUnlockID LF = new SafariUnlockID("LF", register: true);

		public static SafariUnlockID SB = new SafariUnlockID("SB", register: true);

		public static SafariUnlockID OE = new SafariUnlockID("OE", register: true);

		public static SafariUnlockID LC = new SafariUnlockID("LC", register: true);

		public static SafariUnlockID LM = new SafariUnlockID("LM", register: true);

		public static SafariUnlockID DM = new SafariUnlockID("DM", register: true);

		public static SafariUnlockID MS = new SafariUnlockID("MS", register: true);

		public static SafariUnlockID RM = new SafariUnlockID("RM", register: true);

		public static SafariUnlockID UG = new SafariUnlockID("UG", register: true);

		public static SafariUnlockID CL = new SafariUnlockID("CL", register: true);

		public SafariUnlockID(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public static AGLog<MultiplayerUnlocks> Log = new AGLog<MultiplayerUnlocks>();

	public PlayerProgression progression;

	public List<Unlock> unlockedBatches;

	public bool unlockAll;

	public bool unlockNoSpoilers;

	public bool[] creaturesUnlockedForLevelSpawn;

	public static List<SandboxUnlockID> CreatureUnlockList;

	public static List<SandboxUnlockID> ItemUnlockList;

	public bool[] itemsUnlockedForLevelSpawn;

	public static int TOTAL_CHALLENGES = 70;

	public float ExoticItems => Mathf.Pow(Mathf.InverseLerp(0f, 6f, unlockedBatches.Count), 1.25f);

	public static bool CheckUnlockAll()
	{
		if (CheckUnlockDevMode())
		{
			return true;
		}
		if (!File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Levels" + Path.DirectorySeparatorChar + "unlockall_Arena.txt").ToLowerInvariant()))
		{
			return false;
		}
		return File.ReadAllText(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Levels" + Path.DirectorySeparatorChar + "unlockall_Arena.txt").ToLowerInvariant()) == "27623Ez6WV33O36543o9412DpOoPZ11";
	}

	public static bool CheckUnlockNoSpoilers()
	{
		if (!File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Levels" + Path.DirectorySeparatorChar + "unlocknospoilers_Arena.txt").ToLowerInvariant()))
		{
			return false;
		}
		return File.ReadAllText(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Levels" + Path.DirectorySeparatorChar + "unlocknospoilers_Arena.txt").ToLowerInvariant()) == "1123A452ACAPwA62AgD8AAIA";
	}

	public MultiplayerUnlocks(PlayerProgression progression, List<string> allLevels)
	{
		this.progression = progression;
		unlockAll = CheckUnlockAll();
		unlockNoSpoilers = CheckUnlockNoSpoilers();
		creaturesUnlockedForLevelSpawn = new bool[ExtEnum<CreatureTemplate.Type>.values.Count];
		itemsUnlockedForLevelSpawn = new bool[ExtEnum<AbstractPhysicalObject.AbstractObjectType>.values.Count];
		unlockedBatches = new List<Unlock>();
		List<Unlock> list = new List<Unlock>();
		if (unlockNoSpoilers || unlockAll || (ModManager.MMF && MMF.chtUnlockArenas.Value))
		{
			list.Add(new Unlock(LevelUnlockID.Default));
			list.Add(new Unlock(LevelUnlockID.SU));
			list.Add(new Unlock(LevelUnlockID.HI));
			list.Add(new Unlock(LevelUnlockID.CC));
			list.Add(new Unlock(LevelUnlockID.DS));
		}
		if (unlockAll || (ModManager.MMF && MMF.chtUnlockArenas.Value))
		{
			list.Add(new Unlock(LevelUnlockID.GW));
			list.Add(new Unlock(LevelUnlockID.SL));
			list.Add(new Unlock(LevelUnlockID.SH));
			list.Add(new Unlock(LevelUnlockID.SI));
			list.Add(new Unlock(LevelUnlockID.LF));
			list.Add(new Unlock(LevelUnlockID.UW));
			list.Add(new Unlock(LevelUnlockID.SB));
			list.Add(new Unlock(LevelUnlockID.SS));
		}
		if (ModManager.MSC && global::MoreSlugcats.MoreSlugcats.chtUnlockArenas.Value)
		{
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.LC));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.MS));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.HR));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.DM));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.OE));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.RM));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.CL));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.VS));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.LM));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.GWold));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.UG));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.gutter));
			list.Add(new Unlock(MoreSlugcatsEnums.LevelUnlockID.filter));
		}
		if (ModManager.Watcher && global::Watcher.Watcher.chtUnlockArenas.Value)
		{
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.HP));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WSKA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WSKB));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WRFA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WRRA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WPGA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WARF));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WSKD));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WMPA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WTDA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WTDB));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WARG));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WARD));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WBLA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WARE));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WRFB));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WSKC));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WVWA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WVWB));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WARB));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WARC));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WPTA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WARA));
			list.Add(new Unlock(WatcherEnums.LevelUnlockID.WAUA));
		}
		int count = ExtEnum<LevelUnlockID>.values.Count;
		for (int i = LevelUnlockID.Hidden.Index + 1; i < count; i++)
		{
			string entry = ExtEnum<LevelUnlockID>.values.GetEntry(i);
			LevelUnlockID levelUnlockID = ((entry == null) ? null : new LevelUnlockID(entry));
			if (levelUnlockID != null && progression.miscProgressionData.GetTokenCollected(levelUnlockID))
			{
				unlockedBatches.Add(new Unlock(levelUnlockID));
				SafariUnlockID safariUnlockID = TiedSafariID(levelUnlockID);
				if (safariUnlockID != null && !progression.miscProgressionData.GetTokenCollected(safariUnlockID))
				{
					progression.miscProgressionData.SetTokenCollected(safariUnlockID);
				}
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (levelUnlockID == list[j].ID)
				{
					unlockedBatches.Add(list[j]);
					break;
				}
			}
		}
		for (int k = 0; k < unlockedBatches.Count; k++)
		{
			for (int l = 0; l < unlockedBatches[k].creatures.Count; l++)
			{
				if (unlockedBatches[k].creatures[l].Index != -1)
				{
					creaturesUnlockedForLevelSpawn[unlockedBatches[k].creatures[l].Index] = true;
				}
			}
		}
		foreach (SandboxUnlockID creatureUnlock in CreatureUnlockList)
		{
			if (SandboxItemUnlocked(creatureUnlock))
			{
				creaturesUnlockedForLevelSpawn[(int)SymbolDataForSandboxUnlock(creatureUnlock).critType] = true;
			}
		}
		foreach (SandboxUnlockID itemUnlock in ItemUnlockList)
		{
			if (SandboxItemUnlocked(itemUnlock))
			{
				itemsUnlockedForLevelSpawn[(int)SymbolDataForSandboxUnlock(itemUnlock).itemType] = true;
			}
		}
		for (int m = 0; m < allLevels.Count; m++)
		{
			LevelUnlockID levelUnlockID2 = LevelLockID(allLevels[m]);
			for (int n = 0; n < unlockedBatches.Count; n++)
			{
				if (unlockedBatches[n].ID == levelUnlockID2)
				{
					unlockedBatches[n].levels.Add(allLevels[m]);
					break;
				}
			}
		}
		if (progression.miscProgressionData.beaten_Watcher_SentientRot && !progression.miscProgressionData.GetTokenCollected(WatcherEnums.SafariUnlockID.WORA))
		{
			progression.miscProgressionData.SetTokenCollected(WatcherEnums.SafariUnlockID.WSUR);
			progression.miscProgressionData.SetTokenCollected(WatcherEnums.SafariUnlockID.WDSR);
			progression.miscProgressionData.SetTokenCollected(WatcherEnums.SafariUnlockID.WHIR);
			progression.miscProgressionData.SetTokenCollected(WatcherEnums.SafariUnlockID.WGWR);
			progression.miscProgressionData.SetTokenCollected(WatcherEnums.SafariUnlockID.WORA);
		}
		SetLevelSpawnAvailFromUnlockList();
	}

	public bool IsLevelUnlocked(string levelName)
	{
		if (CheckUnlockDevMode())
		{
			return true;
		}
		LevelUnlockID levelUnlockID = LevelLockID(levelName);
		if (levelUnlockID == LevelUnlockID.Default)
		{
			return true;
		}
		if (ModManager.MSC)
		{
			if (levelUnlockID == MoreSlugcatsEnums.LevelUnlockID.Challenge)
			{
				if (progression.miscProgressionData.challengeArenaUnlocks.Contains(levelName))
				{
					return true;
				}
				return false;
			}
			if (levelUnlockID == MoreSlugcatsEnums.LevelUnlockID.ChallengeOnly)
			{
				return false;
			}
		}
		for (int i = 0; i < unlockedBatches.Count; i++)
		{
			if (unlockedBatches[i].ID == levelUnlockID)
			{
				return LevelExtraConditions(levelName);
			}
		}
		return false;
	}

	public static bool IsPlacedCreatureAllowedToSpawn(CreatureTemplate.Type tp, RainWorldGame game)
	{
		if (game.session is SandboxGameSession)
		{
			return false;
		}
		if (game.session is CompetitiveGameSession competitiveGameSession)
		{
			if (competitiveGameSession.GameTypeSetup.wildLifeSetting == ArenaSetup.GameTypeSetup.WildLifeSetting.Off)
			{
				return false;
			}
			return competitiveGameSession.arenaSitting.multiplayerUnlocks.IsCreatureUnlockedForLevelSpawn(tp);
		}
		return true;
	}

	public bool IsCreatureUnlockedForLevelSpawn(CreatureTemplate.Type tp)
	{
		if (tp == null || tp.Index == -1)
		{
			return false;
		}
		return creaturesUnlockedForLevelSpawn[tp.Index];
	}

	public static string LevelDisplayName(string s)
	{
		switch (s.ToLowerInvariant())
		{
		case "cc_intersection":
			return "Scaffolding";
		case "deathpit":
			return "Pit";
		case "ds_drainage":
			return "Cistern";
		case "ds_filters":
			return "Filters";
		case "fourfingers":
			return "Shafts";
		case "hi_stovepipes":
			return "Stove Pipes";
		case "largehall":
			return "Platforms";
		case "pipelands":
			return "Islands";
		case "sh_bigroom":
			return "Hive";
		case "sh_planters":
			return "Planters";
		case "si_array":
			return "Array";
		case "smallroom":
			return "Chamber";
		case "su_lolipops":
			return "Blocks";
		case "su_stoneheads":
			return "Stoneheads";
		case "swingroom2":
			return "Waste Deposit";
		case "thepit":
			return "Amphitheater";
		case "waterreactor":
			return "Water Reactor";
		case "waterworks":
			return "Waterworks";
		case "platforms":
			return "Hole";
		default:
			if (ModManager.MSC)
			{
				switch (s.ToLowerInvariant())
				{
				case "chasteps":
					return "Steps";
				case "skypillar":
					return "Sky Pillar";
				case "chal_ai":
					return "Processing Unit";
				case "powerflux":
					return "Power Flux";
				case "rottenheap":
					return "Rotten Heap";
				case "crop plot":
					return "Crop Plot";
				case "sidepaths":
					return "Side Paths";
				}
			}
			return Custom.ToTitleCase(s.ToLowerInvariant());
		}
	}

	public int LevelListSortNumber(string levelName)
	{
		LevelUnlockID levelUnlockID = LevelLockID(levelName);
		if (levelUnlockID == LevelUnlockID.Default)
		{
			return 0;
		}
		for (int i = 0; i < unlockedBatches.Count; i++)
		{
			if (unlockedBatches[i].ID == levelUnlockID)
			{
				return 1 + i;
			}
		}
		return 0;
	}

	public string LevelListSortString(string levelName)
	{
		return LevelListSortNumber(levelName).ToString("000") + LevelDisplayName(levelName);
	}

	public static LevelUnlockID LevelLockID(string levelName)
	{
		switch (levelName.ToLowerInvariant())
		{
		case "su_lolipops":
		case "su_stoneheads":
		case "smallroom":
			return LevelUnlockID.SU;
		case "hi_stovepipes":
		case "fourfingers":
		case "platforms":
			return LevelUnlockID.HI;
		case "cc_intersection":
		case "thrones":
			return LevelUnlockID.CC;
		case "grid":
		case "deathpit":
		case "pylons":
			return LevelUnlockID.GW;
		case "waterworks":
		case "refinery":
			return LevelUnlockID.SL;
		case "cabinets":
		case "sh_bigroom":
		case "sh_planters":
			return LevelUnlockID.SH;
		case "ds_drainage":
		case "ds_filters":
		case "waterreactor":
			return LevelUnlockID.DS;
		case "antenna":
		case "summit":
		case "si_array":
			return LevelUnlockID.SI;
		case "accelerator":
		case "pipelands":
			return LevelUnlockID.LF;
		case "swingroom2":
		case "joint":
			return LevelUnlockID.UW;
		case "cave":
		case "shortcuts":
		case "nest":
			return LevelUnlockID.SB;
		case "hub":
			return LevelUnlockID.Hidden;
		default:
			if (ModManager.MSC)
			{
				switch (levelName)
				{
				case "caustic vats":
				case "acid field":
					return MoreSlugcatsEnums.LevelUnlockID.GWold;
				case "barrens":
				case "twisted ruin":
					return MoreSlugcatsEnums.LevelUnlockID.CL;
				case "conflux":
				case "dark tower":
					return MoreSlugcatsEnums.LevelUnlockID.DM;
				case "sidepaths":
				case "void shrine":
					return MoreSlugcatsEnums.LevelUnlockID.HR;
				case "roof":
				case "sky lobby":
					return MoreSlugcatsEnums.LevelUnlockID.LC;
				case "sea spires":
				case "substructure":
					return MoreSlugcatsEnums.LevelUnlockID.LM;
				case "rust core":
				case "memory bypass":
					return MoreSlugcatsEnums.LevelUnlockID.MS;
				case "railway":
				case "sun baked":
					return MoreSlugcatsEnums.LevelUnlockID.OE;
				case "decaying construct":
				case "lightning machine":
					return MoreSlugcatsEnums.LevelUnlockID.RM;
				case "man eater":
				case "crawl space":
					return MoreSlugcatsEnums.LevelUnlockID.UG;
				case "towering":
				case "gantry":
					return MoreSlugcatsEnums.LevelUnlockID.VS;
				case "muck machine":
				case "rock bottom":
					return MoreSlugcatsEnums.LevelUnlockID.gutter;
				case "filtration node":
				case "submerged":
					return MoreSlugcatsEnums.LevelUnlockID.filter;
				default:
					if (!levelName.StartsWith("multi_"))
					{
						break;
					}
					goto case "basin";
				case "basin":
				case "powerflux":
				case "dark water":
				case "chal_ai":
				case "passages":
				case "girders":
				case "chutes":
				case "chasteps":
				case "aorta":
				case "duct":
				case "substation":
				case "baths":
				case "grill":
				case "chapopcorn":
				case "charger":
				case "barrels":
				case "water house":
				case "mines":
					return MoreSlugcatsEnums.LevelUnlockID.ChallengeOnly;
				}
			}
			if (ModManager.Watcher)
			{
				switch (levelName)
				{
				case "canal":
					return WatcherEnums.LevelUnlockID.HP;
				case "vortex":
				case "cloister":
					return WatcherEnums.LevelUnlockID.WARA;
				case "complex":
				case "hull":
					return WatcherEnums.LevelUnlockID.WARB;
				case "chasm":
				case "stilts":
					return WatcherEnums.LevelUnlockID.WARC;
				case "research":
				case "decay":
					return WatcherEnums.LevelUnlockID.WARD;
				case "exhaust":
				case "array":
					return WatcherEnums.LevelUnlockID.WARE;
				case "station":
				case "aeolian":
					return WatcherEnums.LevelUnlockID.WARF;
				case "floor":
				case "balcony":
					return WatcherEnums.LevelUnlockID.WARG;
				case "hadal":
				case "terminus":
					return WatcherEnums.LevelUnlockID.WMPA;
				case "sinkhole":
				case "sand fort":
				case "mud pit":
					return WatcherEnums.LevelUnlockID.WBLA;
				case "receiver":
				case "satellites":
				case "transmission nodes":
					return WatcherEnums.LevelUnlockID.WPTA;
				case "pots":
				case "urban":
				case "hills":
					return WatcherEnums.LevelUnlockID.WPGA;
				case "lagoon":
				case "bridge":
					return WatcherEnums.LevelUnlockID.WRFA;
				case "circulation":
				case "intake":
					return WatcherEnums.LevelUnlockID.WRFB;
				case "fountain":
				case "shallows":
					return WatcherEnums.LevelUnlockID.WSKA;
				case "neon":
				case "sunbath":
					return WatcherEnums.LevelUnlockID.WSKB;
				case "suspended":
				case "waterhalls":
					return WatcherEnums.LevelUnlockID.WSKC;
				case "aviary":
				case "containers":
					return WatcherEnums.LevelUnlockID.WSKD;
				case "squaremaze":
				case "tomb":
				case "drill site":
					return WatcherEnums.LevelUnlockID.WTDA;
				case "tunnel":
				case "edge":
				case "flats":
					return WatcherEnums.LevelUnlockID.WTDB;
				case "rig":
				case "beams":
				case "manufactory":
					return WatcherEnums.LevelUnlockID.WRRA;
				case "drain":
				case "floodgates":
				case "geysers":
					return WatcherEnums.LevelUnlockID.WVWA;
				case "mire":
				case "rupture":
					return WatcherEnums.LevelUnlockID.WVWB;
				case "sewer":
				case "shelves":
				case "cityscape":
					return WatcherEnums.LevelUnlockID.WAUA;
				}
			}
			return LevelUnlockID.Default;
		}
	}

	public bool LevelExtraConditions(string levelName)
	{
		return levelName.ToLowerInvariant() switch
		{
			"fourfingers" => IsCreatureUnlockedForLevelSpawn(CreatureTemplate.Type.Snail), 
			"ds_drainage" => IsCreatureUnlockedForLevelSpawn(CreatureTemplate.Type.Centipede), 
			"cave" => IsCreatureUnlockedForLevelSpawn(CreatureTemplate.Type.BigEel), 
			_ => true, 
		};
	}

	public static List<CreatureTemplate.Type> UnlockedCritters(LevelUnlockID ID)
	{
		List<CreatureTemplate.Type> list = new List<CreatureTemplate.Type>();
		if (ID == LevelUnlockID.Default)
		{
			list.Add(CreatureTemplate.Type.PinkLizard);
			list.Add(CreatureTemplate.Type.GreenLizard);
			list.Add(CreatureTemplate.Type.Fly);
			list.Add(CreatureTemplate.Type.GarbageWorm);
			list.Add(CreatureTemplate.Type.TentaclePlant);
			list.Add(CreatureTemplate.Type.PoleMimic);
			list.Add(CreatureTemplate.Type.MirosBird);
			list.Add(CreatureTemplate.Type.EggBug);
		}
		else if (ID == LevelUnlockID.Hidden)
		{
			list.Add(CreatureTemplate.Type.RedLizard);
			list.Add(CreatureTemplate.Type.CyanLizard);
			list.Add(CreatureTemplate.Type.KingVulture);
			list.Add(CreatureTemplate.Type.SmallNeedleWorm);
			list.Add(CreatureTemplate.Type.BigNeedleWorm);
			list.Add(CreatureTemplate.Type.DropBug);
		}
		else if (ID == LevelUnlockID.SU)
		{
			list.Add(CreatureTemplate.Type.CicadaA);
			list.Add(CreatureTemplate.Type.CicadaB);
			list.Add(CreatureTemplate.Type.SmallCentipede);
		}
		else if (ID == LevelUnlockID.HI)
		{
			list.Add(CreatureTemplate.Type.BlueLizard);
			list.Add(CreatureTemplate.Type.WhiteLizard);
			list.Add(CreatureTemplate.Type.Vulture);
			list.Add(CreatureTemplate.Type.SmallCentipede);
		}
		else if (ID == LevelUnlockID.CC)
		{
			list.Add(CreatureTemplate.Type.Centipede);
			list.Add(CreatureTemplate.Type.SmallCentipede);
			list.Add(CreatureTemplate.Type.BlueLizard);
			list.Add(CreatureTemplate.Type.WhiteLizard);
			list.Add(CreatureTemplate.Type.Vulture);
			list.Add(CreatureTemplate.Type.Scavenger);
		}
		else if (ID == LevelUnlockID.GW)
		{
			list.Add(CreatureTemplate.Type.BrotherLongLegs);
			list.Add(CreatureTemplate.Type.Scavenger);
			list.Add(CreatureTemplate.Type.BlueLizard);
			list.Add(CreatureTemplate.Type.WhiteLizard);
			list.Add(CreatureTemplate.Type.Vulture);
			list.Add(CreatureTemplate.Type.CicadaA);
			list.Add(CreatureTemplate.Type.CicadaB);
			list.Add(CreatureTemplate.Type.Leech);
		}
		else if (ID == LevelUnlockID.SL)
		{
			list.Add(CreatureTemplate.Type.Leech);
			list.Add(CreatureTemplate.Type.SeaLeech);
			list.Add(CreatureTemplate.Type.JetFish);
			list.Add(CreatureTemplate.Type.BigEel);
			list.Add(CreatureTemplate.Type.Salamander);
		}
		else if (ID == LevelUnlockID.SH)
		{
			list.Add(CreatureTemplate.Type.Spider);
			list.Add(CreatureTemplate.Type.BigSpider);
			list.Add(CreatureTemplate.Type.SpitterSpider);
			list.Add(CreatureTemplate.Type.LanternMouse);
		}
		else if (ID == LevelUnlockID.DS)
		{
			list.Add(CreatureTemplate.Type.Salamander);
			list.Add(CreatureTemplate.Type.Leech);
			list.Add(CreatureTemplate.Type.Snail);
		}
		else if (ID == LevelUnlockID.SI)
		{
			list.Add(CreatureTemplate.Type.Centiwing);
			list.Add(CreatureTemplate.Type.CicadaA);
			list.Add(CreatureTemplate.Type.CicadaB);
			list.Add(CreatureTemplate.Type.Scavenger);
		}
		else if (ID == LevelUnlockID.LF)
		{
			list.Add(CreatureTemplate.Type.Centipede);
			list.Add(CreatureTemplate.Type.SmallCentipede);
			list.Add(CreatureTemplate.Type.BlueLizard);
			list.Add(CreatureTemplate.Type.Vulture);
			list.Add(CreatureTemplate.Type.Scavenger);
		}
		else if (ID == LevelUnlockID.UW)
		{
			list.Add(CreatureTemplate.Type.WhiteLizard);
			list.Add(CreatureTemplate.Type.TubeWorm);
			list.Add(CreatureTemplate.Type.YellowLizard);
		}
		else if (ID == LevelUnlockID.SB)
		{
			list.Add(CreatureTemplate.Type.BlackLizard);
			list.Add(CreatureTemplate.Type.BigSpider);
			list.Add(CreatureTemplate.Type.SpitterSpider);
			list.Add(CreatureTemplate.Type.Salamander);
			list.Add(CreatureTemplate.Type.Scavenger);
			list.Add(CreatureTemplate.Type.Centipede);
		}
		else if (ID == LevelUnlockID.SS)
		{
			list.Add(CreatureTemplate.Type.DaddyLongLegs);
		}
		if (ModManager.MSC)
		{
			if (ID == MoreSlugcatsEnums.LevelUnlockID.LC)
			{
				list.Add(DLCSharedEnums.CreatureTemplateType.ScavengerElite);
				list.Add(CreatureTemplate.Type.Vulture);
				list.Add(CreatureTemplate.Type.Scavenger);
				list.Add(CreatureTemplate.Type.BlueLizard);
				list.Add(CreatureTemplate.Type.WhiteLizard);
				list.Add(CreatureTemplate.Type.TubeWorm);
				list.Add(CreatureTemplate.Type.CicadaA);
				list.Add(CreatureTemplate.Type.CicadaB);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.MS)
			{
				list.Add(DLCSharedEnums.CreatureTemplateType.EelLizard);
				list.Add(DLCSharedEnums.CreatureTemplateType.AquaCenti);
				list.Add(DLCSharedEnums.CreatureTemplateType.BigJelly);
				list.Add(CreatureTemplate.Type.BigEel);
				list.Add(CreatureTemplate.Type.BlueLizard);
				list.Add(CreatureTemplate.Type.WhiteLizard);
				list.Add(CreatureTemplate.Type.Salamander);
				list.Add(CreatureTemplate.Type.JetFish);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.HR)
			{
				list.Add(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
				list.Add(DLCSharedEnums.CreatureTemplateType.MirosVulture);
				list.Add(MoreSlugcatsEnums.CreatureTemplateType.FireBug);
				list.Add(CreatureTemplate.Type.RedLizard);
				list.Add(CreatureTemplate.Type.CyanLizard);
				list.Add(CreatureTemplate.Type.BigSpider);
				list.Add(CreatureTemplate.Type.SpitterSpider);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.DM)
			{
				list.Add(DLCSharedEnums.CreatureTemplateType.MirosVulture);
				list.Add(DLCSharedEnums.CreatureTemplateType.Inspector);
				list.Add(DLCSharedEnums.CreatureTemplateType.MotherSpider);
				list.Add(CreatureTemplate.Type.DaddyLongLegs);
				list.Add(CreatureTemplate.Type.WhiteLizard);
				list.Add(CreatureTemplate.Type.BigSpider);
				list.Add(CreatureTemplate.Type.SpitterSpider);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.OE)
			{
				list.Add(DLCSharedEnums.CreatureTemplateType.SpitLizard);
				list.Add(DLCSharedEnums.CreatureTemplateType.Yeek);
				list.Add(CreatureTemplate.Type.Vulture);
				list.Add(CreatureTemplate.Type.Scavenger);
				list.Add(CreatureTemplate.Type.BlueLizard);
				list.Add(CreatureTemplate.Type.WhiteLizard);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.RM)
			{
				list.Add(DLCSharedEnums.CreatureTemplateType.Inspector);
				list.Add(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs);
				list.Add(CreatureTemplate.Type.DaddyLongLegs);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.CL)
			{
				list.Add(CreatureTemplate.Type.BlueLizard);
				list.Add(CreatureTemplate.Type.YellowLizard);
				list.Add(CreatureTemplate.Type.Scavenger);
				list.Add(CreatureTemplate.Type.Centipede);
				list.Add(CreatureTemplate.Type.BigSpider);
				list.Add(CreatureTemplate.Type.SpitterSpider);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.VS)
			{
				list.Add(CreatureTemplate.Type.Centiwing);
				list.Add(CreatureTemplate.Type.Centipede);
				list.Add(CreatureTemplate.Type.BigSpider);
				list.Add(CreatureTemplate.Type.BlueLizard);
				list.Add(CreatureTemplate.Type.WhiteLizard);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.LM)
			{
				list.Add(CreatureTemplate.Type.BlueLizard);
				list.Add(CreatureTemplate.Type.WhiteLizard);
				list.Add(CreatureTemplate.Type.Salamander);
				list.Add(CreatureTemplate.Type.JetFish);
				list.Add(CreatureTemplate.Type.CicadaA);
				list.Add(CreatureTemplate.Type.CicadaB);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.GWold)
			{
				list.Add(CreatureTemplate.Type.Scavenger);
				list.Add(CreatureTemplate.Type.BlueLizard);
				list.Add(CreatureTemplate.Type.CicadaA);
				list.Add(CreatureTemplate.Type.CicadaB);
				list.Add(CreatureTemplate.Type.BigSpider);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.UG)
			{
				list.Add(CreatureTemplate.Type.BlackLizard);
				list.Add(CreatureTemplate.Type.BigSpider);
				list.Add(CreatureTemplate.Type.Scavenger);
				list.Add(CreatureTemplate.Type.BlueLizard);
				list.Add(DLCSharedEnums.CreatureTemplateType.EelLizard);
				list.Add(DLCSharedEnums.CreatureTemplateType.JungleLeech);
				list.Add(DLCSharedEnums.CreatureTemplateType.StowawayBug);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.gutter)
			{
				list.Add(DLCSharedEnums.CreatureTemplateType.EelLizard);
				list.Add(CreatureTemplate.Type.WhiteLizard);
				list.Add(CreatureTemplate.Type.Centipede);
			}
			if (ID == MoreSlugcatsEnums.LevelUnlockID.filter)
			{
				list.Add(CreatureTemplate.Type.BlackLizard);
				list.Add(CreatureTemplate.Type.BigSpider);
				list.Add(CreatureTemplate.Type.LanternMouse);
			}
		}
		if (ModManager.Watcher)
		{
			if (ID == WatcherEnums.LevelUnlockID.HP)
			{
				list.Add(CreatureTemplate.Type.BlueLizard);
				list.Add(CreatureTemplate.Type.WhiteLizard);
				list.Add(CreatureTemplate.Type.Snail);
				list.Add(CreatureTemplate.Type.Leech);
				list.Add(CreatureTemplate.Type.Scavenger);
				list.Add(CreatureTemplate.Type.CicadaA);
				list.Add(CreatureTemplate.Type.CicadaB);
				list.Add(CreatureTemplate.Type.Vulture);
				list.Add(CreatureTemplate.Type.DropBug);
				list.Add(CreatureTemplate.Type.CyanLizard);
			}
			if (ID == WatcherEnums.LevelUnlockID.WSKA)
			{
				list.Add(CreatureTemplate.Type.Salamander);
				list.Add(WatcherEnums.CreatureTemplateType.Tardigrade);
			}
			if (ID == WatcherEnums.LevelUnlockID.WSKB)
			{
				list.Add(CreatureTemplate.Type.Centipede);
				list.Add(CreatureTemplate.Type.BigNeedleWorm);
				list.Add(CreatureTemplate.Type.SmallNeedleWorm);
			}
			if (ID == WatcherEnums.LevelUnlockID.WRFA)
			{
				list.Add(CreatureTemplate.Type.Salamander);
				list.Add(WatcherEnums.CreatureTemplateType.Tardigrade);
				list.Add(WatcherEnums.CreatureTemplateType.Barnacle);
				list.Add(CreatureTemplate.Type.JetFish);
			}
			if (ID == WatcherEnums.LevelUnlockID.WRRA)
			{
				list.Add(CreatureTemplate.Type.Salamander);
				list.Add(WatcherEnums.CreatureTemplateType.PeachLizard);
				list.Add(WatcherEnums.CreatureTemplateType.Tardigrade);
				list.Add(WatcherEnums.CreatureTemplateType.Frog);
				list.Add(CreatureTemplate.Type.Centipede);
				list.Add(CreatureTemplate.Type.SeaLeech);
			}
			if (ID == WatcherEnums.LevelUnlockID.WPGA)
			{
				list.Add(CreatureTemplate.Type.Deer);
				list.Add(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
				list.Add(WatcherEnums.CreatureTemplateType.Frog);
			}
			if (ID == WatcherEnums.LevelUnlockID.WARF)
			{
				list.Add(WatcherEnums.CreatureTemplateType.BigMoth);
			}
			if (ID == WatcherEnums.LevelUnlockID.WSKD)
			{
				list.Add(WatcherEnums.CreatureTemplateType.BigMoth);
				list.Add(WatcherEnums.CreatureTemplateType.MothGrub);
				list.Add(CreatureTemplate.Type.LanternMouse);
			}
			if (ID == WatcherEnums.LevelUnlockID.WMPA)
			{
				list.Add(WatcherEnums.CreatureTemplateType.PeachLizard);
				list.Add(WatcherEnums.CreatureTemplateType.Barnacle);
				list.Add(WatcherEnums.CreatureTemplateType.DrillCrab);
				list.Add(WatcherEnums.CreatureTemplateType.TowerCrab);
				list.Add(CreatureTemplate.Type.YellowLizard);
			}
			if (ID == WatcherEnums.LevelUnlockID.WTDA)
			{
				list.Add(WatcherEnums.CreatureTemplateType.FireSprite);
				list.Add(WatcherEnums.CreatureTemplateType.BoxWorm);
				list.Add(WatcherEnums.CreatureTemplateType.DrillCrab);
				list.Add(WatcherEnums.CreatureTemplateType.SandGrub);
				list.Add(CreatureTemplate.Type.Centipede);
				list.Add(CreatureTemplate.Type.SmallCentipede);
			}
			if (ID == WatcherEnums.LevelUnlockID.WTDB)
			{
				list.Add(WatcherEnums.CreatureTemplateType.Loach);
				list.Add(CreatureTemplate.Type.SmallCentipede);
			}
			if (ID == WatcherEnums.LevelUnlockID.WARG)
			{
				list.Add(WatcherEnums.CreatureTemplateType.BlizzardLizard);
			}
			if (ID == WatcherEnums.LevelUnlockID.WARD)
			{
				list.Add(WatcherEnums.CreatureTemplateType.BigMoth);
			}
			if (ID == WatcherEnums.LevelUnlockID.WBLA)
			{
				list.Add(WatcherEnums.CreatureTemplateType.DrillCrab);
				list.Add(WatcherEnums.CreatureTemplateType.IndigoLizard);
				list.Add(WatcherEnums.CreatureTemplateType.SandGrub);
				list.Add(WatcherEnums.CreatureTemplateType.BigSandGrub);
				list.Add(DLCSharedEnums.CreatureTemplateType.SpitLizard);
				list.Add(CreatureTemplate.Type.BlackLizard);
			}
			if (ID == WatcherEnums.LevelUnlockID.WARE)
			{
				list.Add(WatcherEnums.CreatureTemplateType.FireSprite);
				list.Add(WatcherEnums.CreatureTemplateType.BoxWorm);
				list.Add(CreatureTemplate.Type.Centipede);
				list.Add(CreatureTemplate.Type.YellowLizard);
			}
			if (ID == WatcherEnums.LevelUnlockID.WRFB)
			{
				list.Add(WatcherEnums.CreatureTemplateType.Barnacle);
				list.Add(WatcherEnums.CreatureTemplateType.PeachLizard);
				list.Add(CreatureTemplate.Type.Salamander);
			}
			if (ID == WatcherEnums.LevelUnlockID.WSKC)
			{
				list.Add(CreatureTemplate.Type.Salamander);
			}
			if (ID == WatcherEnums.LevelUnlockID.WVWA)
			{
				list.Add(DLCSharedEnums.CreatureTemplateType.EelLizard);
				list.Add(WatcherEnums.CreatureTemplateType.Frog);
				list.Add(CreatureTemplate.Type.Centiwing);
			}
			if (ID == WatcherEnums.LevelUnlockID.WVWB)
			{
				list.Add(WatcherEnums.CreatureTemplateType.BigSandGrub);
				list.Add(WatcherEnums.CreatureTemplateType.SkyWhale);
				list.Add(WatcherEnums.CreatureTemplateType.Frog);
			}
			if (ID == WatcherEnums.LevelUnlockID.WARB)
			{
				list.Add(WatcherEnums.CreatureTemplateType.Angler);
				list.Add(WatcherEnums.CreatureTemplateType.Barnacle);
				list.Add(CreatureTemplate.Type.Salamander);
			}
			if (ID == WatcherEnums.LevelUnlockID.WARC)
			{
				list.Add(WatcherEnums.CreatureTemplateType.Angler);
				list.Add(WatcherEnums.CreatureTemplateType.BasiliskLizard);
				list.Add(WatcherEnums.CreatureTemplateType.Frog);
				list.Add(CreatureTemplate.Type.LanternMouse);
				list.Add(CreatureTemplate.Type.Centiwing);
			}
			if (ID == WatcherEnums.LevelUnlockID.WPTA)
			{
				list.Add(WatcherEnums.CreatureTemplateType.SkyWhale);
				list.Add(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
				list.Add(CreatureTemplate.Type.YellowLizard);
				list.Add(CreatureTemplate.Type.TubeWorm);
			}
			if (ID == WatcherEnums.LevelUnlockID.WARA)
			{
				list.Add(WatcherEnums.CreatureTemplateType.ScavengerTemplar);
				list.Add(WatcherEnums.CreatureTemplateType.ScavengerDisciple);
				list.Add(CreatureTemplate.Type.BigSpider);
				list.Add(CreatureTemplate.Type.SpitterSpider);
			}
			if (ID == WatcherEnums.LevelUnlockID.WAUA)
			{
				list.Add(WatcherEnums.CreatureTemplateType.SmallMoth);
				list.Add(WatcherEnums.CreatureTemplateType.Rat);
			}
		}
		return list;
	}

	public CreatureTemplate.Type RecursiveFallBackCritter(CreatureTemplate.Type crit)
	{
		if (crit == null)
		{
			return null;
		}
		if (IsCreatureUnlockedForLevelSpawn(crit))
		{
			return crit;
		}
		return RecursiveFallBackCritter(FallBackCrit(crit));
	}

	public static CreatureTemplate.Type FallBackCrit(CreatureTemplate.Type crit)
	{
		if (crit == CreatureTemplate.Type.CyanLizard)
		{
			if (!(Random.value < 0.5f))
			{
				return CreatureTemplate.Type.BlueLizard;
			}
			return CreatureTemplate.Type.WhiteLizard;
		}
		if (crit == CreatureTemplate.Type.WhiteLizard)
		{
			return CreatureTemplate.Type.BlueLizard;
		}
		if (crit == CreatureTemplate.Type.BlueLizard || crit == CreatureTemplate.Type.RedLizard || crit == CreatureTemplate.Type.BlackLizard || crit == CreatureTemplate.Type.Salamander || crit == CreatureTemplate.Type.YellowLizard)
		{
			if (!(Random.value < 0.75f))
			{
				return CreatureTemplate.Type.GreenLizard;
			}
			return CreatureTemplate.Type.PinkLizard;
		}
		if (crit == CreatureTemplate.Type.SeaLeech)
		{
			return CreatureTemplate.Type.Leech;
		}
		if (crit == CreatureTemplate.Type.DaddyLongLegs)
		{
			return CreatureTemplate.Type.BrotherLongLegs;
		}
		if (crit == CreatureTemplate.Type.Centiwing)
		{
			return CreatureTemplate.Type.Centipede;
		}
		if (crit == CreatureTemplate.Type.DropBug || crit == CreatureTemplate.Type.SpitterSpider)
		{
			return CreatureTemplate.Type.BigSpider;
		}
		if (crit == CreatureTemplate.Type.KingVulture)
		{
			return CreatureTemplate.Type.Vulture;
		}
		if (crit == CreatureTemplate.Type.BigNeedleWorm)
		{
			if (!(Random.value < 0.5f))
			{
				return CreatureTemplate.Type.CicadaB;
			}
			return CreatureTemplate.Type.CicadaA;
		}
		if (ModManager.DLCShared)
		{
			if (crit == DLCSharedEnums.CreatureTemplateType.JungleLeech)
			{
				return CreatureTemplate.Type.Leech;
			}
			if (crit == DLCSharedEnums.CreatureTemplateType.MirosVulture)
			{
				return CreatureTemplate.Type.KingVulture;
			}
			if (crit == DLCSharedEnums.CreatureTemplateType.MotherSpider)
			{
				return CreatureTemplate.Type.SpitterSpider;
			}
			if (crit == DLCSharedEnums.CreatureTemplateType.Yeek)
			{
				if (!(Random.value < 0.5f))
				{
					return CreatureTemplate.Type.CicadaB;
				}
				return CreatureTemplate.Type.CicadaA;
			}
			if (crit == DLCSharedEnums.CreatureTemplateType.SpitLizard)
			{
				return CreatureTemplate.Type.GreenLizard;
			}
			if (crit == DLCSharedEnums.CreatureTemplateType.EelLizard)
			{
				return CreatureTemplate.Type.Salamander;
			}
			if (crit == DLCSharedEnums.CreatureTemplateType.ZoopLizard)
			{
				if (!(Random.value < 0.5f))
				{
					return CreatureTemplate.Type.BlueLizard;
				}
				return CreatureTemplate.Type.WhiteLizard;
			}
			if (crit == DLCSharedEnums.CreatureTemplateType.ScavengerElite)
			{
				return CreatureTemplate.Type.Scavenger;
			}
			if (crit == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return CreatureTemplate.Type.DaddyLongLegs;
			}
		}
		if (ModManager.MSC && crit == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
		{
			return CreatureTemplate.Type.EggBug;
		}
		if (ModManager.Watcher)
		{
			if (crit == WatcherEnums.CreatureTemplateType.Barnacle || crit == WatcherEnums.CreatureTemplateType.Frog)
			{
				return CreatureTemplate.Type.Snail;
			}
			if (crit == WatcherEnums.CreatureTemplateType.BasiliskLizard)
			{
				return CreatureTemplate.Type.BlackLizard;
			}
			if (crit == WatcherEnums.CreatureTemplateType.SmallMoth)
			{
				return WatcherEnums.CreatureTemplateType.BigMoth;
			}
			if (crit == WatcherEnums.CreatureTemplateType.ScavengerDisciple || crit == WatcherEnums.CreatureTemplateType.ScavengerTemplar)
			{
				return CreatureTemplate.Type.Scavenger;
			}
			if (crit == WatcherEnums.CreatureTemplateType.IndigoLizard)
			{
				return CreatureTemplate.Type.GreenLizard;
			}
			if (crit == WatcherEnums.CreatureTemplateType.PeachLizard)
			{
				return CreatureTemplate.Type.YellowLizard;
			}
			if (crit == WatcherEnums.CreatureTemplateType.BlizzardLizard)
			{
				return CreatureTemplate.Type.CyanLizard;
			}
			if (crit == WatcherEnums.CreatureTemplateType.Rat)
			{
				return CreatureTemplate.Type.SmallCentipede;
			}
		}
		return null;
	}

	public bool SandboxItemUnlocked(SandboxUnlockID unlockID)
	{
		if (unlockID == null)
		{
			return false;
		}
		if (unlockID == SandboxUnlockID.Slugcat || unlockID == SandboxUnlockID.GreenLizard || unlockID == SandboxUnlockID.PinkLizard || unlockID == SandboxUnlockID.Fly || unlockID == SandboxUnlockID.Rock || unlockID == SandboxUnlockID.Spear)
		{
			return true;
		}
		if ((unlockNoSpoilers || unlockAll || (ModManager.MMF && MMF.chtUnlockCreatures.Value)) && (unlockID == SandboxUnlockID.BlueLizard || unlockID == SandboxUnlockID.WhiteLizard || unlockID == SandboxUnlockID.YellowLizard || unlockID == SandboxUnlockID.Salamander || unlockID == SandboxUnlockID.CicadaA || unlockID == SandboxUnlockID.CicadaB || unlockID == SandboxUnlockID.Snail || unlockID == SandboxUnlockID.Leech || unlockID == SandboxUnlockID.PoleMimic || unlockID == SandboxUnlockID.Vulture || unlockID == SandboxUnlockID.SmallCentipede || unlockID == SandboxUnlockID.MediumCentipede || unlockID == SandboxUnlockID.BigCentipede || unlockID == SandboxUnlockID.LanternMouse || unlockID == SandboxUnlockID.Scavenger))
		{
			return true;
		}
		if ((unlockNoSpoilers || unlockAll || (ModManager.MMF && MMF.chtUnlockItems.Value)) && (unlockID == SandboxUnlockID.FireSpear || unlockID == SandboxUnlockID.FlareBomb || unlockID == SandboxUnlockID.PuffBall || unlockID == SandboxUnlockID.FirecrackerPlant || unlockID == SandboxUnlockID.DangleFruit || unlockID == SandboxUnlockID.SeedCob))
		{
			return true;
		}
		if ((unlockAll || (ModManager.MMF && MMF.chtUnlockCreatures.Value)) && (unlockID == SandboxUnlockID.CyanLizard || unlockID == SandboxUnlockID.RedLizard || unlockID == SandboxUnlockID.KingVulture || unlockID == SandboxUnlockID.RedCentipede || unlockID == SandboxUnlockID.Centiwing || unlockID == SandboxUnlockID.TubeWorm || unlockID == SandboxUnlockID.Hazer || unlockID == SandboxUnlockID.BigSpider || unlockID == SandboxUnlockID.SpitterSpider || unlockID == SandboxUnlockID.BrotherLongLegs || unlockID == SandboxUnlockID.DaddyLongLegs || unlockID == SandboxUnlockID.Deer || unlockID == SandboxUnlockID.EggBug || unlockID == SandboxUnlockID.DropBug || unlockID == SandboxUnlockID.BigNeedleWorm || unlockID == SandboxUnlockID.SmallNeedleWorm || unlockID == SandboxUnlockID.BigEel || unlockID == SandboxUnlockID.JetFish || unlockID == SandboxUnlockID.Spider || unlockID == SandboxUnlockID.SeaLeech || unlockID == SandboxUnlockID.JellyFish || unlockID == SandboxUnlockID.TentaclePlant || unlockID == SandboxUnlockID.MirosBird || unlockID == SandboxUnlockID.VultureGrub || unlockID == SandboxUnlockID.BlackLizard))
		{
			return true;
		}
		if ((unlockAll || (ModManager.MMF && MMF.chtUnlockItems.Value)) && (unlockID == SandboxUnlockID.ScavengerBomb || unlockID == SandboxUnlockID.BubbleGrass || unlockID == SandboxUnlockID.SlimeMold || unlockID == SandboxUnlockID.Mushroom || unlockID == SandboxUnlockID.FlyLure || unlockID == SandboxUnlockID.SporePlant || unlockID == SandboxUnlockID.Lantern || unlockID == SandboxUnlockID.WaterNut))
		{
			return true;
		}
		if (ModManager.MSC && global::MoreSlugcats.MoreSlugcats.chtUnlockCreatures.Value)
		{
			if (unlockID == MoreSlugcatsEnums.SandboxUnlockID.MirosVulture || unlockID == MoreSlugcatsEnums.SandboxUnlockID.SpitLizard || unlockID == MoreSlugcatsEnums.SandboxUnlockID.EelLizard || unlockID == MoreSlugcatsEnums.SandboxUnlockID.MotherSpider || unlockID == MoreSlugcatsEnums.SandboxUnlockID.TerrorLongLegs || unlockID == MoreSlugcatsEnums.SandboxUnlockID.AquaCenti || unlockID == MoreSlugcatsEnums.SandboxUnlockID.FireBug || unlockID == MoreSlugcatsEnums.SandboxUnlockID.ScavengerElite || unlockID == MoreSlugcatsEnums.SandboxUnlockID.Inspector || unlockID == MoreSlugcatsEnums.SandboxUnlockID.Yeek || unlockID == MoreSlugcatsEnums.SandboxUnlockID.BigJelly || unlockID == MoreSlugcatsEnums.SandboxUnlockID.JungleLeech || unlockID == MoreSlugcatsEnums.SandboxUnlockID.ZoopLizard)
			{
				return true;
			}
			if (unlockID == MoreSlugcatsEnums.SandboxUnlockID.SlugNPC && global::MoreSlugcats.MoreSlugcats.chtUnlockSlugpups.Value)
			{
				return true;
			}
		}
		if (ModManager.MSC && global::MoreSlugcats.MoreSlugcats.chtUnlockItems.Value && (unlockID == MoreSlugcatsEnums.SandboxUnlockID.ElectricSpear || unlockID == MoreSlugcatsEnums.SandboxUnlockID.LillyPuck || unlockID == MoreSlugcatsEnums.SandboxUnlockID.Pearl || unlockID == MoreSlugcatsEnums.SandboxUnlockID.SingularityBomb || unlockID == MoreSlugcatsEnums.SandboxUnlockID.FireEgg || unlockID == MoreSlugcatsEnums.SandboxUnlockID.HellSpear || unlockID == MoreSlugcatsEnums.SandboxUnlockID.VultureMask || unlockID == MoreSlugcatsEnums.SandboxUnlockID.GooieDuck || unlockID == MoreSlugcatsEnums.SandboxUnlockID.DandelionPeach || unlockID == MoreSlugcatsEnums.SandboxUnlockID.GlowWeed || unlockID == MoreSlugcatsEnums.SandboxUnlockID.EnergyCell || unlockID == MoreSlugcatsEnums.SandboxUnlockID.JokeRifle))
		{
			return true;
		}
		if (ModManager.Watcher && global::Watcher.Watcher.chtUnlockCreatures.Value && (unlockID == WatcherEnums.SandboxUnlockID.Barnacle || unlockID == WatcherEnums.SandboxUnlockID.Tardigrade || unlockID == WatcherEnums.SandboxUnlockID.Frog || unlockID == WatcherEnums.SandboxUnlockID.DrillCrab || unlockID == WatcherEnums.SandboxUnlockID.SandGrub || unlockID == WatcherEnums.SandboxUnlockID.BigSandGrub || unlockID == WatcherEnums.SandboxUnlockID.SmallMoth || unlockID == WatcherEnums.SandboxUnlockID.BigMoth || unlockID == WatcherEnums.SandboxUnlockID.BoxWorm || unlockID == WatcherEnums.SandboxUnlockID.FireSprite || unlockID == WatcherEnums.SandboxUnlockID.ScavengerTemplar || unlockID == WatcherEnums.SandboxUnlockID.ScavengerDisciple || unlockID == WatcherEnums.SandboxUnlockID.Loach || unlockID == WatcherEnums.SandboxUnlockID.RotLoach || unlockID == WatcherEnums.SandboxUnlockID.Rattler || unlockID == WatcherEnums.SandboxUnlockID.IndigoLizard || unlockID == WatcherEnums.SandboxUnlockID.BlizzardLizard || unlockID == WatcherEnums.SandboxUnlockID.BasiliskLizard || unlockID == WatcherEnums.SandboxUnlockID.ProtoLizard || unlockID == WatcherEnums.SandboxUnlockID.RotLizard || unlockID == WatcherEnums.SandboxUnlockID.Rat || unlockID == WatcherEnums.SandboxUnlockID.Angler || unlockID == WatcherEnums.SandboxUnlockID.SkyWhale || unlockID == WatcherEnums.SandboxUnlockID.AltSkyWhale || unlockID == WatcherEnums.SandboxUnlockID.MothGrub || unlockID == WatcherEnums.SandboxUnlockID.PeachLizard))
		{
			return true;
		}
		if (ModManager.Watcher && global::Watcher.Watcher.chtUnlockItems.Value && (unlockID == WatcherEnums.SandboxUnlockID.RotDangleFruit || unlockID == WatcherEnums.SandboxUnlockID.RotSeedCob || unlockID == WatcherEnums.SandboxUnlockID.Boomerang || unlockID == WatcherEnums.SandboxUnlockID.GraffitiBomb || unlockID == WatcherEnums.SandboxUnlockID.SoftToy || unlockID == WatcherEnums.SandboxUnlockID.BallToy || unlockID == WatcherEnums.SandboxUnlockID.SpinToy || unlockID == WatcherEnums.SandboxUnlockID.WeirdToy || unlockID == WatcherEnums.SandboxUnlockID.Pomegranate))
		{
			return true;
		}
		if (progression.miscProgressionData.GetTokenCollected(unlockID))
		{
			return true;
		}
		if (ParentSandboxID(unlockID) != null)
		{
			return progression.miscProgressionData.GetTokenCollected(ParentSandboxID(unlockID));
		}
		return false;
	}

	public static IconSymbol.IconSymbolData SymbolDataForSandboxUnlock(SandboxUnlockID unlockID)
	{
		if (unlockID == SandboxUnlockID.MediumCentipede)
		{
			return new IconSymbol.IconSymbolData(CreatureTemplate.Type.Centipede, AbstractPhysicalObject.AbstractObjectType.Creature, 2);
		}
		if (unlockID == SandboxUnlockID.BigCentipede)
		{
			return new IconSymbol.IconSymbolData(CreatureTemplate.Type.Centipede, AbstractPhysicalObject.AbstractObjectType.Creature, 3);
		}
		if (unlockID == SandboxUnlockID.FireSpear)
		{
			return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, AbstractPhysicalObject.AbstractObjectType.Spear, 1);
		}
		if (ModManager.MSC)
		{
			if (unlockID == MoreSlugcatsEnums.SandboxUnlockID.SlugNPC)
			{
				return new IconSymbol.IconSymbolData(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
			}
			if (unlockID == MoreSlugcatsEnums.SandboxUnlockID.ElectricSpear)
			{
				return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, AbstractPhysicalObject.AbstractObjectType.Spear, 2);
			}
			if (unlockID == MoreSlugcatsEnums.SandboxUnlockID.HellSpear)
			{
				return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, AbstractPhysicalObject.AbstractObjectType.Spear, 3);
			}
			if (unlockID == MoreSlugcatsEnums.SandboxUnlockID.Pearl)
			{
				return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, AbstractPhysicalObject.AbstractObjectType.DataPearl, 0);
			}
		}
		if (ModManager.Watcher)
		{
			if (unlockID == WatcherEnums.SandboxUnlockID.RotDangleFruit)
			{
				return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, AbstractPhysicalObject.AbstractObjectType.DangleFruit, 1);
			}
			if (unlockID == WatcherEnums.SandboxUnlockID.RotSeedCob)
			{
				return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, AbstractPhysicalObject.AbstractObjectType.SeedCob, 1);
			}
			if (unlockID == WatcherEnums.SandboxUnlockID.RotLizard)
			{
				return new IconSymbol.IconSymbolData(CreatureTemplate.Type.GreenLizard, AbstractPhysicalObject.AbstractObjectType.Creature, 1);
			}
			if (unlockID == WatcherEnums.SandboxUnlockID.ProtoLizard)
			{
				return new IconSymbol.IconSymbolData(CreatureTemplate.Type.GreenLizard, AbstractPhysicalObject.AbstractObjectType.Creature, 2);
			}
			if (unlockID == WatcherEnums.SandboxUnlockID.AltSkyWhale)
			{
				return new IconSymbol.IconSymbolData(WatcherEnums.CreatureTemplateType.SkyWhale, AbstractPhysicalObject.AbstractObjectType.Creature, 1);
			}
		}
		foreach (SandboxUnlockID creatureUnlock in CreatureUnlockList)
		{
			if (unlockID == creatureUnlock)
			{
				return new IconSymbol.IconSymbolData(new CreatureTemplate.Type(unlockID.ToString()), AbstractPhysicalObject.AbstractObjectType.Creature, 0);
			}
		}
		return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, new AbstractPhysicalObject.AbstractObjectType(unlockID.ToString()), 0);
	}

	public static SandboxUnlockID SandboxUnlockForSymbolData(IconSymbol.IconSymbolData data)
	{
		try
		{
			if (data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature)
			{
				if (data.critType == CreatureTemplate.Type.Centipede)
				{
					return (data.intData == 2) ? SandboxUnlockID.MediumCentipede : SandboxUnlockID.BigCentipede;
				}
				if (ModManager.Watcher)
				{
					for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
					{
						if (StaticWorld.creatureTemplates[i].type == data.critType)
						{
							if (!StaticWorld.creatureTemplates[i].IsLizard)
							{
								break;
							}
							if (data.intData == 1)
							{
								return WatcherEnums.SandboxUnlockID.RotLizard;
							}
							if (data.intData != 2)
							{
								break;
							}
							return WatcherEnums.SandboxUnlockID.ProtoLizard;
						}
					}
				}
				return new SandboxUnlockID(data.critType.ToString());
			}
			if (data.itemType == AbstractPhysicalObject.AbstractObjectType.Spear && data.intData > 0)
			{
				if (ModManager.MSC && data.intData == 3)
				{
					return MoreSlugcatsEnums.SandboxUnlockID.HellSpear;
				}
				if (ModManager.MSC && data.intData == 2)
				{
					return MoreSlugcatsEnums.SandboxUnlockID.ElectricSpear;
				}
				return SandboxUnlockID.FireSpear;
			}
			if (ModManager.Watcher && data.itemType == AbstractPhysicalObject.AbstractObjectType.DangleFruit && data.intData == 1)
			{
				return WatcherEnums.SandboxUnlockID.RotDangleFruit;
			}
			if (ModManager.Watcher && data.itemType == AbstractPhysicalObject.AbstractObjectType.SeedCob && data.intData == 1)
			{
				return WatcherEnums.SandboxUnlockID.RotSeedCob;
			}
			return new SandboxUnlockID(data.itemType.ToString());
		}
		catch
		{
			return SandboxUnlockID.Slugcat;
		}
	}

	public static List<SandboxUnlockID> TiedSandboxIDs(SandboxUnlockID ID, bool includeParent)
	{
		List<SandboxUnlockID> list = ((ID == SandboxUnlockID.Slugcat) ? new List<SandboxUnlockID>
		{
			SandboxUnlockID.GreenLizard,
			SandboxUnlockID.PinkLizard,
			SandboxUnlockID.Fly,
			SandboxUnlockID.Rock,
			SandboxUnlockID.Spear
		} : ((ID == SandboxUnlockID.CicadaA) ? new List<SandboxUnlockID> { SandboxUnlockID.CicadaB } : ((ID == SandboxUnlockID.SmallCentipede) ? new List<SandboxUnlockID> { SandboxUnlockID.MediumCentipede } : ((ID == SandboxUnlockID.BigNeedleWorm) ? new List<SandboxUnlockID> { SandboxUnlockID.SmallNeedleWorm } : ((ModManager.Watcher && ID == WatcherEnums.SandboxUnlockID.BigSandGrub) ? new List<SandboxUnlockID> { WatcherEnums.SandboxUnlockID.SandGrub } : ((ModManager.Watcher && ID == WatcherEnums.SandboxUnlockID.BoxWorm) ? new List<SandboxUnlockID> { WatcherEnums.SandboxUnlockID.FireSprite } : ((ModManager.Watcher && ID == WatcherEnums.SandboxUnlockID.ScavengerTemplar) ? new List<SandboxUnlockID> { WatcherEnums.SandboxUnlockID.ScavengerDisciple } : ((!ModManager.Watcher || !(ID == WatcherEnums.SandboxUnlockID.ProtoLizard)) ? new List<SandboxUnlockID>() : new List<SandboxUnlockID> { WatcherEnums.SandboxUnlockID.RotLizard }))))))));
		if (includeParent)
		{
			list.Insert(0, ID);
		}
		return list;
	}

	public static SafariUnlockID TiedSafariID(LevelUnlockID ID)
	{
		SafariUnlockID safariUnlockID = new SafariUnlockID(ID.value);
		if (safariUnlockID.Index != -1 && WarpSafariMenu.WarpExclusiveSafari(safariUnlockID))
		{
			return safariUnlockID;
		}
		return null;
	}

	public static SandboxUnlockID ParentSandboxID(SandboxUnlockID childUnlock)
	{
		if (childUnlock == SandboxUnlockID.GreenLizard || childUnlock == SandboxUnlockID.PinkLizard || childUnlock == SandboxUnlockID.Fly || childUnlock == SandboxUnlockID.Rock || childUnlock == SandboxUnlockID.Spear)
		{
			return SandboxUnlockID.Slugcat;
		}
		if (childUnlock == SandboxUnlockID.CicadaB)
		{
			return SandboxUnlockID.CicadaA;
		}
		if (childUnlock == SandboxUnlockID.MediumCentipede)
		{
			return SandboxUnlockID.SmallCentipede;
		}
		if (childUnlock == SandboxUnlockID.SmallNeedleWorm)
		{
			return SandboxUnlockID.BigNeedleWorm;
		}
		if (ModManager.Watcher && childUnlock == WatcherEnums.SandboxUnlockID.SandGrub)
		{
			return WatcherEnums.SandboxUnlockID.BigSandGrub;
		}
		if (ModManager.Watcher && childUnlock == WatcherEnums.SandboxUnlockID.FireSprite)
		{
			return WatcherEnums.SandboxUnlockID.BoxWorm;
		}
		if (ModManager.Watcher && childUnlock == WatcherEnums.SandboxUnlockID.ScavengerDisciple)
		{
			return WatcherEnums.SandboxUnlockID.ScavengerTemplar;
		}
		if (ModManager.Watcher && childUnlock == WatcherEnums.SandboxUnlockID.RotLizard)
		{
			return WatcherEnums.SandboxUnlockID.ProtoLizard;
		}
		return null;
	}

	public static bool CheckUnlockDevMode()
	{
		if (ModManager.MSC && File.Exists((Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "Levels" + Path.DirectorySeparatorChar + "unlockall_DevMode.txt").ToLowerInvariant()))
		{
			return File.ReadAllText((Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "Levels" + Path.DirectorySeparatorChar + "unlockall_DevMode.txt").ToLowerInvariant()) == "8B0fa98455af49ecab0AA409c838c1AC";
		}
		return false;
	}

	public static bool CheckUnlockSafari()
	{
		if (CheckUnlockDevMode())
		{
			return true;
		}
		if (global::MoreSlugcats.MoreSlugcats.chtUnlockSafari.Value)
		{
			return true;
		}
		return false;
	}

	public static bool CheckUnlockWatcherSafari()
	{
		if (CheckUnlockDevMode())
		{
			return true;
		}
		if (global::Watcher.Watcher.chtUnlockSafari.Value)
		{
			return true;
		}
		return false;
	}

	public static bool CheckUnlockChallenge()
	{
		if (CheckUnlockDevMode())
		{
			return true;
		}
		if (global::MoreSlugcats.MoreSlugcats.chtUnlockChallenges.Value)
		{
			return true;
		}
		return false;
	}

	public bool IsItemUnlockedForLevelSpawn(AbstractPhysicalObject.AbstractObjectType tp)
	{
		if (tp.Index == -1)
		{
			return false;
		}
		return itemsUnlockedForLevelSpawn[tp.Index];
	}

	public bool ClassUnlocked(SlugcatStats.Name classID)
	{
		if (SlugcatStats.HiddenOrUnplayableSlugcat(classID))
		{
			return false;
		}
		if (classID == SlugcatStats.Name.Red)
		{
			return progression.miscProgressionData.redUnlocked;
		}
		if (ModManager.MSC)
		{
			if (classID == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
			{
				if (!progression.miscProgressionData.GetTokenCollected(SlugcatUnlockID.Gourmand))
				{
					return global::MoreSlugcats.MoreSlugcats.chtUnlockClasses.Value;
				}
				return true;
			}
			if (classID == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				if (!progression.miscProgressionData.GetTokenCollected(SlugcatUnlockID.Rivulet))
				{
					return global::MoreSlugcats.MoreSlugcats.chtUnlockClasses.Value;
				}
				return true;
			}
			if (classID == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				if (!progression.miscProgressionData.GetTokenCollected(SlugcatUnlockID.Artificer))
				{
					return global::MoreSlugcats.MoreSlugcats.chtUnlockClasses.Value;
				}
				return true;
			}
			if (classID == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				if (!progression.miscProgressionData.GetTokenCollected(SlugcatUnlockID.Saint))
				{
					return global::MoreSlugcats.MoreSlugcats.chtUnlockClasses.Value;
				}
				return true;
			}
			if (classID == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				if (!progression.miscProgressionData.GetTokenCollected(SlugcatUnlockID.Spearmaster))
				{
					return global::MoreSlugcats.MoreSlugcats.chtUnlockClasses.Value;
				}
				return true;
			}
		}
		if (ModManager.Watcher && classID == WatcherEnums.SlugcatStatsName.Watcher)
		{
			if (!progression.miscProgressionData.GetTokenCollected(WatcherEnums.SlugcatUnlockID.Watcher))
			{
				return global::Watcher.Watcher.chtUnlockArenaClass.Value;
			}
			return true;
		}
		return true;
	}

	public void SetLevelSpawnAvailFromUnlockList()
	{
		creaturesUnlockedForLevelSpawn = new bool[ExtEnum<CreatureTemplate.Type>.values.Count];
		itemsUnlockedForLevelSpawn = new bool[ExtEnum<AbstractPhysicalObject.AbstractObjectType>.values.Count];
		for (int i = 0; i < unlockedBatches.Count; i++)
		{
			for (int j = 0; j < unlockedBatches[i].creatures.Count; j++)
			{
				if (unlockedBatches[i].creatures[j].Index != -1)
				{
					creaturesUnlockedForLevelSpawn[unlockedBatches[i].creatures[j].Index] = true;
				}
			}
		}
		foreach (SandboxUnlockID creatureUnlock in CreatureUnlockList)
		{
			if (SandboxItemUnlocked(creatureUnlock) && SymbolDataForSandboxUnlock(creatureUnlock).critType.Index != -1)
			{
				creaturesUnlockedForLevelSpawn[SymbolDataForSandboxUnlock(creatureUnlock).critType.Index] = true;
			}
		}
		foreach (SandboxUnlockID itemUnlock in ItemUnlockList)
		{
			if (SandboxItemUnlocked(itemUnlock) && SymbolDataForSandboxUnlock(itemUnlock).itemType.Index != -1)
			{
				itemsUnlockedForLevelSpawn[SymbolDataForSandboxUnlock(itemUnlock).itemType.Index] = true;
			}
		}
	}
}
