using Menu;
using Smoke;

namespace Watcher;

public class WatcherEnums
{
	public class SlugcatStatsName
	{
		public static SlugcatStats.Name Watcher;

		public static void RegisterValues()
		{
			Watcher = new SlugcatStats.Name("Watcher", register: true);
		}

		public static void UnregisterValues()
		{
			Watcher?.Unregister();
			Watcher = null;
		}
	}

	public class SlugcatUnlockID
	{
		public static MultiplayerUnlocks.SlugcatUnlockID Watcher;

		public static void RegisterValues()
		{
			Watcher = new MultiplayerUnlocks.SlugcatUnlockID("Watcher", register: true);
		}

		public static void UnregisterValues()
		{
			Watcher?.Unregister();
			Watcher = null;
		}
	}

	public class SandboxUnlockID
	{
		public static MultiplayerUnlocks.SandboxUnlockID Barnacle;

		public static MultiplayerUnlocks.SandboxUnlockID Tardigrade;

		public static MultiplayerUnlocks.SandboxUnlockID Frog;

		public static MultiplayerUnlocks.SandboxUnlockID DrillCrab;

		public static MultiplayerUnlocks.SandboxUnlockID SandGrub;

		public static MultiplayerUnlocks.SandboxUnlockID BigSandGrub;

		public static MultiplayerUnlocks.SandboxUnlockID SmallMoth;

		public static MultiplayerUnlocks.SandboxUnlockID BigMoth;

		public static MultiplayerUnlocks.SandboxUnlockID BoxWorm;

		public static MultiplayerUnlocks.SandboxUnlockID FireSprite;

		public static MultiplayerUnlocks.SandboxUnlockID ScavengerTemplar;

		public static MultiplayerUnlocks.SandboxUnlockID ScavengerDisciple;

		public static MultiplayerUnlocks.SandboxUnlockID Loach;

		public static MultiplayerUnlocks.SandboxUnlockID RotLoach;

		public static MultiplayerUnlocks.SandboxUnlockID Rattler;

		public static MultiplayerUnlocks.SandboxUnlockID IndigoLizard;

		public static MultiplayerUnlocks.SandboxUnlockID PeachLizard;

		public static MultiplayerUnlocks.SandboxUnlockID BlizzardLizard;

		public static MultiplayerUnlocks.SandboxUnlockID BasiliskLizard;

		public static MultiplayerUnlocks.SandboxUnlockID ProtoLizard;

		public static MultiplayerUnlocks.SandboxUnlockID RotLizard;

		public static MultiplayerUnlocks.SandboxUnlockID Rat;

		public static MultiplayerUnlocks.SandboxUnlockID GrappleSnake;

		public static MultiplayerUnlocks.SandboxUnlockID Millipede;

		public static MultiplayerUnlocks.SandboxUnlockID Angler;

		public static MultiplayerUnlocks.SandboxUnlockID SkyWhale;

		public static MultiplayerUnlocks.SandboxUnlockID AltSkyWhale;

		public static MultiplayerUnlocks.SandboxUnlockID RotDangleFruit;

		public static MultiplayerUnlocks.SandboxUnlockID RotSeedCob;

		public static MultiplayerUnlocks.SandboxUnlockID Pomegranate;

		public static MultiplayerUnlocks.SandboxUnlockID Boomerang;

		public static MultiplayerUnlocks.SandboxUnlockID GraffitiBomb;

		public static MultiplayerUnlocks.SandboxUnlockID BallToy;

		public static MultiplayerUnlocks.SandboxUnlockID SoftToy;

		public static MultiplayerUnlocks.SandboxUnlockID SpinToy;

		public static MultiplayerUnlocks.SandboxUnlockID WeirdToy;

		public static MultiplayerUnlocks.SandboxUnlockID MothGrub;

		public static void RegisterValues()
		{
			Barnacle = new MultiplayerUnlocks.SandboxUnlockID("Barnacle", register: true);
			Tardigrade = new MultiplayerUnlocks.SandboxUnlockID("Tardigrade", register: true);
			Frog = new MultiplayerUnlocks.SandboxUnlockID("Frog", register: true);
			DrillCrab = new MultiplayerUnlocks.SandboxUnlockID("DrillCrab", register: true);
			SandGrub = new MultiplayerUnlocks.SandboxUnlockID("SandGrub", register: true);
			BigSandGrub = new MultiplayerUnlocks.SandboxUnlockID("BigSandGrub", register: true);
			SmallMoth = new MultiplayerUnlocks.SandboxUnlockID("SmallMoth", register: true);
			BigMoth = new MultiplayerUnlocks.SandboxUnlockID("BigMoth", register: true);
			BoxWorm = new MultiplayerUnlocks.SandboxUnlockID("BoxWorm", register: true);
			FireSprite = new MultiplayerUnlocks.SandboxUnlockID("FireSprite", register: true);
			ScavengerTemplar = new MultiplayerUnlocks.SandboxUnlockID("ScavengerTemplar", register: true);
			ScavengerDisciple = new MultiplayerUnlocks.SandboxUnlockID("ScavengerDisciple", register: true);
			Loach = new MultiplayerUnlocks.SandboxUnlockID("Loach", register: true);
			RotLoach = new MultiplayerUnlocks.SandboxUnlockID("RotLoach", register: true);
			Rattler = new MultiplayerUnlocks.SandboxUnlockID("Rattler", register: true);
			IndigoLizard = new MultiplayerUnlocks.SandboxUnlockID("IndigoLizard", register: true);
			BlizzardLizard = new MultiplayerUnlocks.SandboxUnlockID("BlizzardLizard", register: true);
			BasiliskLizard = new MultiplayerUnlocks.SandboxUnlockID("BasiliskLizard", register: true);
			PeachLizard = new MultiplayerUnlocks.SandboxUnlockID("PeachLizard", register: true);
			ProtoLizard = new MultiplayerUnlocks.SandboxUnlockID("ProtoLizard", register: true);
			RotLizard = new MultiplayerUnlocks.SandboxUnlockID("RotLizard", register: true);
			Rat = new MultiplayerUnlocks.SandboxUnlockID("Rat", register: true);
			Angler = new MultiplayerUnlocks.SandboxUnlockID("Angler", register: true);
			SkyWhale = new MultiplayerUnlocks.SandboxUnlockID("SkyWhale", register: true);
			AltSkyWhale = new MultiplayerUnlocks.SandboxUnlockID("AltSkyWhale", register: true);
			RotDangleFruit = new MultiplayerUnlocks.SandboxUnlockID("RotDangleFruit", register: true);
			RotSeedCob = new MultiplayerUnlocks.SandboxUnlockID("RotSeedCob", register: true);
			Pomegranate = new MultiplayerUnlocks.SandboxUnlockID("Pomegranate", register: true);
			Boomerang = new MultiplayerUnlocks.SandboxUnlockID("Boomerang", register: true);
			GraffitiBomb = new MultiplayerUnlocks.SandboxUnlockID("GraffitiBomb", register: true);
			BallToy = new MultiplayerUnlocks.SandboxUnlockID("BallToy", register: true);
			SoftToy = new MultiplayerUnlocks.SandboxUnlockID("SoftToy", register: true);
			SpinToy = new MultiplayerUnlocks.SandboxUnlockID("SpinToy", register: true);
			WeirdToy = new MultiplayerUnlocks.SandboxUnlockID("WeirdToy", register: true);
			MothGrub = new MultiplayerUnlocks.SandboxUnlockID("MothGrub", register: true);
		}

		public static void UnregisterValues()
		{
			Barnacle?.Unregister();
			Barnacle = null;
			Tardigrade?.Unregister();
			Tardigrade = null;
			Frog?.Unregister();
			Frog = null;
			DrillCrab?.Unregister();
			DrillCrab = null;
			SandGrub?.Unregister();
			SandGrub = null;
			BigSandGrub?.Unregister();
			BigSandGrub = null;
			SmallMoth?.Unregister();
			BigMoth = null;
			BigMoth?.Unregister();
			BigMoth = null;
			BoxWorm?.Unregister();
			BoxWorm = null;
			FireSprite?.Unregister();
			FireSprite = null;
			ScavengerTemplar?.Unregister();
			ScavengerTemplar = null;
			ScavengerDisciple?.Unregister();
			ScavengerDisciple = null;
			Loach?.Unregister();
			Loach = null;
			RotLoach?.Unregister();
			RotLoach = null;
			Rattler?.Unregister();
			Rattler = null;
			IndigoLizard?.Unregister();
			IndigoLizard = null;
			BlizzardLizard?.Unregister();
			BlizzardLizard = null;
			BasiliskLizard?.Unregister();
			BasiliskLizard = null;
			ProtoLizard?.Unregister();
			ProtoLizard = null;
			PeachLizard?.Unregister();
			PeachLizard = null;
			RotLizard?.Unregister();
			RotLizard = null;
			Rat?.Unregister();
			Rat = null;
			Angler?.Unregister();
			Angler = null;
			SkyWhale?.Unregister();
			SkyWhale = null;
			AltSkyWhale?.Unregister();
			AltSkyWhale = null;
			RotDangleFruit?.Unregister();
			RotDangleFruit = null;
			RotSeedCob?.Unregister();
			RotSeedCob = null;
			Pomegranate?.Unregister();
			Pomegranate = null;
			Boomerang?.Unregister();
			Boomerang = null;
			GraffitiBomb?.Unregister();
			GraffitiBomb = null;
			BallToy?.Unregister();
			BallToy = null;
			SoftToy?.Unregister();
			SoftToy = null;
			SpinToy?.Unregister();
			SpinToy = null;
			WeirdToy?.Unregister();
			WeirdToy = null;
			MothGrub?.Unregister();
			MothGrub = null;
		}
	}

	public class LevelUnlockID
	{
		public static MultiplayerUnlocks.LevelUnlockID HP;

		public static MultiplayerUnlocks.LevelUnlockID WARA;

		public static MultiplayerUnlocks.LevelUnlockID WARB;

		public static MultiplayerUnlocks.LevelUnlockID WARC;

		public static MultiplayerUnlocks.LevelUnlockID WARD;

		public static MultiplayerUnlocks.LevelUnlockID WARE;

		public static MultiplayerUnlocks.LevelUnlockID WARF;

		public static MultiplayerUnlocks.LevelUnlockID WARG;

		public static MultiplayerUnlocks.LevelUnlockID WMPA;

		public static MultiplayerUnlocks.LevelUnlockID WBLA;

		public static MultiplayerUnlocks.LevelUnlockID WPTA;

		public static MultiplayerUnlocks.LevelUnlockID WPGA;

		public static MultiplayerUnlocks.LevelUnlockID WRFA;

		public static MultiplayerUnlocks.LevelUnlockID WRFB;

		public static MultiplayerUnlocks.LevelUnlockID WSKA;

		public static MultiplayerUnlocks.LevelUnlockID WSKB;

		public static MultiplayerUnlocks.LevelUnlockID WSKC;

		public static MultiplayerUnlocks.LevelUnlockID WSKD;

		public static MultiplayerUnlocks.LevelUnlockID WTDA;

		public static MultiplayerUnlocks.LevelUnlockID WTDB;

		public static MultiplayerUnlocks.LevelUnlockID WRRA;

		public static MultiplayerUnlocks.LevelUnlockID WVWA;

		public static MultiplayerUnlocks.LevelUnlockID WVWB;

		public static MultiplayerUnlocks.LevelUnlockID WAUA;

		public static void RegisterValues()
		{
			HP = new MultiplayerUnlocks.LevelUnlockID("HP", register: true);
			WSKA = new MultiplayerUnlocks.LevelUnlockID("WSKA", register: true);
			WSKB = new MultiplayerUnlocks.LevelUnlockID("WSKB", register: true);
			WRFA = new MultiplayerUnlocks.LevelUnlockID("WRFA", register: true);
			WRRA = new MultiplayerUnlocks.LevelUnlockID("WRRA", register: true);
			WPGA = new MultiplayerUnlocks.LevelUnlockID("WPGA", register: true);
			WARF = new MultiplayerUnlocks.LevelUnlockID("WARF", register: true);
			WSKD = new MultiplayerUnlocks.LevelUnlockID("WSKD", register: true);
			WMPA = new MultiplayerUnlocks.LevelUnlockID("WMPA", register: true);
			WTDA = new MultiplayerUnlocks.LevelUnlockID("WTDA", register: true);
			WTDB = new MultiplayerUnlocks.LevelUnlockID("WTDB", register: true);
			WARG = new MultiplayerUnlocks.LevelUnlockID("WARG", register: true);
			WARD = new MultiplayerUnlocks.LevelUnlockID("WARD", register: true);
			WBLA = new MultiplayerUnlocks.LevelUnlockID("WBLA", register: true);
			WARE = new MultiplayerUnlocks.LevelUnlockID("WARE", register: true);
			WRFB = new MultiplayerUnlocks.LevelUnlockID("WRFB", register: true);
			WSKC = new MultiplayerUnlocks.LevelUnlockID("WSKC", register: true);
			WVWA = new MultiplayerUnlocks.LevelUnlockID("WVWA", register: true);
			WVWB = new MultiplayerUnlocks.LevelUnlockID("WVWB", register: true);
			WARB = new MultiplayerUnlocks.LevelUnlockID("WARB", register: true);
			WARC = new MultiplayerUnlocks.LevelUnlockID("WARC", register: true);
			WPTA = new MultiplayerUnlocks.LevelUnlockID("WPTA", register: true);
			WARA = new MultiplayerUnlocks.LevelUnlockID("WARA", register: true);
			WAUA = new MultiplayerUnlocks.LevelUnlockID("WAUA", register: true);
		}

		public static void UnregisterValues()
		{
			HP?.Unregister();
			HP = null;
			WARA?.Unregister();
			WARA = null;
			WARB?.Unregister();
			WARB = null;
			WARC?.Unregister();
			WARC = null;
			WARD?.Unregister();
			WARD = null;
			WARE?.Unregister();
			WARE = null;
			WARF?.Unregister();
			WARF = null;
			WARG?.Unregister();
			WARG = null;
			WMPA?.Unregister();
			WMPA = null;
			WBLA?.Unregister();
			WBLA = null;
			WPTA?.Unregister();
			WPTA = null;
			WPGA?.Unregister();
			WPGA = null;
			WRFA?.Unregister();
			WRFA = null;
			WRFB?.Unregister();
			WRFB = null;
			WSKA?.Unregister();
			WSKA = null;
			WSKB?.Unregister();
			WSKB = null;
			WSKC?.Unregister();
			WSKC = null;
			WSKD?.Unregister();
			WSKD = null;
			WTDA?.Unregister();
			WTDA = null;
			WTDB?.Unregister();
			WTDB = null;
			WRRA?.Unregister();
			WRRA = null;
			WVWA?.Unregister();
			WVWA = null;
			WVWB?.Unregister();
			WVWB = null;
			WAUA?.Unregister();
			WAUA = null;
		}
	}

	public class SafariUnlockID
	{
		public static MultiplayerUnlocks.SafariUnlockID WARA;

		public static MultiplayerUnlocks.SafariUnlockID WARB;

		public static MultiplayerUnlocks.SafariUnlockID WARC;

		public static MultiplayerUnlocks.SafariUnlockID WARD;

		public static MultiplayerUnlocks.SafariUnlockID WARE;

		public static MultiplayerUnlocks.SafariUnlockID WARF;

		public static MultiplayerUnlocks.SafariUnlockID WARG;

		public static MultiplayerUnlocks.SafariUnlockID WSKA;

		public static MultiplayerUnlocks.SafariUnlockID WSKB;

		public static MultiplayerUnlocks.SafariUnlockID WSKC;

		public static MultiplayerUnlocks.SafariUnlockID WSKD;

		public static MultiplayerUnlocks.SafariUnlockID WRFA;

		public static MultiplayerUnlocks.SafariUnlockID WRFB;

		public static MultiplayerUnlocks.SafariUnlockID WTDA;

		public static MultiplayerUnlocks.SafariUnlockID WTDB;

		public static MultiplayerUnlocks.SafariUnlockID WVWA;

		public static MultiplayerUnlocks.SafariUnlockID WVWB;

		public static MultiplayerUnlocks.SafariUnlockID WPGA;

		public static MultiplayerUnlocks.SafariUnlockID WRRA;

		public static MultiplayerUnlocks.SafariUnlockID WMPA;

		public static MultiplayerUnlocks.SafariUnlockID WBLA;

		public static MultiplayerUnlocks.SafariUnlockID WPTA;

		public static MultiplayerUnlocks.SafariUnlockID WAUA;

		public static MultiplayerUnlocks.SafariUnlockID WSUR;

		public static MultiplayerUnlocks.SafariUnlockID WDSR;

		public static MultiplayerUnlocks.SafariUnlockID WHIR;

		public static MultiplayerUnlocks.SafariUnlockID WGWR;

		public static MultiplayerUnlocks.SafariUnlockID WORA;

		public static void RegisterValues()
		{
			WSKA = new MultiplayerUnlocks.SafariUnlockID("WSKA", register: true);
			WSKB = new MultiplayerUnlocks.SafariUnlockID("WSKB", register: true);
			WRFA = new MultiplayerUnlocks.SafariUnlockID("WRFA", register: true);
			WRRA = new MultiplayerUnlocks.SafariUnlockID("WRRA", register: true);
			WPGA = new MultiplayerUnlocks.SafariUnlockID("WPGA", register: true);
			WARF = new MultiplayerUnlocks.SafariUnlockID("WARF", register: true);
			WSKD = new MultiplayerUnlocks.SafariUnlockID("WSKD", register: true);
			WMPA = new MultiplayerUnlocks.SafariUnlockID("WMPA", register: true);
			WTDA = new MultiplayerUnlocks.SafariUnlockID("WTDA", register: true);
			WTDB = new MultiplayerUnlocks.SafariUnlockID("WTDB", register: true);
			WARG = new MultiplayerUnlocks.SafariUnlockID("WARG", register: true);
			WARD = new MultiplayerUnlocks.SafariUnlockID("WARD", register: true);
			WBLA = new MultiplayerUnlocks.SafariUnlockID("WBLA", register: true);
			WARE = new MultiplayerUnlocks.SafariUnlockID("WARE", register: true);
			WRFB = new MultiplayerUnlocks.SafariUnlockID("WRFB", register: true);
			WSKC = new MultiplayerUnlocks.SafariUnlockID("WSKC", register: true);
			WVWA = new MultiplayerUnlocks.SafariUnlockID("WVWA", register: true);
			WVWB = new MultiplayerUnlocks.SafariUnlockID("WVWB", register: true);
			WARB = new MultiplayerUnlocks.SafariUnlockID("WARB", register: true);
			WARC = new MultiplayerUnlocks.SafariUnlockID("WARC", register: true);
			WPTA = new MultiplayerUnlocks.SafariUnlockID("WPTA", register: true);
			WARA = new MultiplayerUnlocks.SafariUnlockID("WARA", register: true);
			WAUA = new MultiplayerUnlocks.SafariUnlockID("WAUA", register: true);
			WSUR = new MultiplayerUnlocks.SafariUnlockID("WSUR", register: true);
			WDSR = new MultiplayerUnlocks.SafariUnlockID("WDSR", register: true);
			WHIR = new MultiplayerUnlocks.SafariUnlockID("WHIR", register: true);
			WGWR = new MultiplayerUnlocks.SafariUnlockID("WGWR", register: true);
			WORA = new MultiplayerUnlocks.SafariUnlockID("WORA", register: true);
		}

		public static void UnregisterValues()
		{
			WARA?.Unregister();
			WARA = null;
			WARB?.Unregister();
			WARB = null;
			WARC?.Unregister();
			WARC = null;
			WARD?.Unregister();
			WARD = null;
			WARE?.Unregister();
			WARE = null;
			WARF?.Unregister();
			WARF = null;
			WARG?.Unregister();
			WARG = null;
			WMPA?.Unregister();
			WMPA = null;
			WBLA?.Unregister();
			WBLA = null;
			WPTA?.Unregister();
			WPTA = null;
			WPGA?.Unregister();
			WPGA = null;
			WRFA?.Unregister();
			WRFA = null;
			WRFB?.Unregister();
			WRFB = null;
			WSKA?.Unregister();
			WSKA = null;
			WSKB?.Unregister();
			WSKB = null;
			WSKC?.Unregister();
			WSKC = null;
			WSKD?.Unregister();
			WSKD = null;
			WTDA?.Unregister();
			WTDA = null;
			WTDB?.Unregister();
			WTDB = null;
			WRRA?.Unregister();
			WRRA = null;
			WVWA?.Unregister();
			WVWA = null;
			WVWB?.Unregister();
			WVWB = null;
			WAUA?.Unregister();
			WAUA = null;
			WSUR?.Unregister();
			WSUR = null;
			WDSR?.Unregister();
			WDSR = null;
			WHIR?.Unregister();
			WHIR = null;
			WGWR?.Unregister();
			WGWR = null;
			WORA?.Unregister();
			WORA = null;
		}
	}

	public class MenuSceneID
	{
		public static MenuScene.SceneID Slugcat_Watcher;

		public static MenuScene.SceneID End_Watcher_A;

		public static MenuScene.SceneID End_Watcher_B;

		public static MenuScene.SceneID End_Watcher_C;

		public static MenuScene.SceneID End_Watcher_D;

		public static MenuScene.SceneID Ripple_Sleep;

		public static MenuScene.SceneID Ripple_Sleep_Death;

		public static MenuScene.SceneID Dream_Watcher1;

		public static MenuScene.SceneID Dream_Watcher2;

		public static MenuScene.SceneID Dream_Watcher3;

		public static MenuScene.SceneID Dream_Watcher4;

		public static MenuScene.SceneID Dream_Watcher5;

		public static MenuScene.SceneID Dream_Watcher6;

		public static MenuScene.SceneID Dream_SpinningTop1;

		public static MenuScene.SceneID Dream_SpinningTop2;

		public static MenuScene.SceneID Dream_SpinningTop3;

		public static MenuScene.SceneID Dream_SpinningTop4;

		public static MenuScene.SceneID Dream_SpinningTop5;

		public static MenuScene.SceneID Dream_SpinningTop6;

		public static MenuScene.SceneID Dream_SpinningTop7;

		public static MenuScene.SceneID Dream_Rot1;

		public static MenuScene.SceneID Dream_Rot2;

		public static MenuScene.SceneID Dream_Rot3;

		public static MenuScene.SceneID Dream_Rot4;

		public static MenuScene.SceneID Dream_Rot5;

		public static MenuScene.SceneID Dream_Rot6;

		public static MenuScene.SceneID Dream_Rot7;

		public static MenuScene.SceneID Dream_Rot8;

		public static MenuScene.SceneID Dream_Rot9;

		public static MenuScene.SceneID Dream_Rot10;

		public static MenuScene.SceneID Ending_VoidBath1;

		public static MenuScene.SceneID Ending_VoidBath2;

		public static MenuScene.SceneID Ending_VoidBath3;

		public static MenuScene.SceneID Ending_SpinningTop1;

		public static MenuScene.SceneID Ending_SpinningTop2;

		public static MenuScene.SceneID Ending_SpinningTop3;

		public static MenuScene.SceneID Ending_Prince1;

		public static MenuScene.SceneID Ending_Prince2;

		public static MenuScene.SceneID Ending_Prince3;

		public static MenuScene.SceneID Ending_Prince4;

		public static MenuScene.SceneID Ending_Prince5;

		public static MenuScene.SceneID Ending_Prince6;

		public static MenuScene.SceneID Dream_VoidWeaver1;

		public static MenuScene.SceneID Dream_VoidWeaver2;

		public static MenuScene.SceneID Dream_VoidWeaver3;

		public static MenuScene.SceneID Dream_VoidWeaver4;

		public static MenuScene.SceneID Dream_VoidWeaver5;

		public static MenuScene.SceneID Dream_VoidWeaver6;

		public static MenuScene.SceneID Dream_VoidWeaver7;

		public static MenuScene.SceneID Ending_Weaver1;

		public static MenuScene.SceneID Ending_Weaver2;

		public static MenuScene.SceneID Ending_Weaver3;

		public static MenuScene.SceneID Ending_Weaver4;

		public static MenuScene.SceneID Ending_Weaver5;

		public static MenuScene.SceneID Ending_Weaver6;

		public static MenuScene.SceneID Dream_Terrace;

		public static MenuScene.SceneID MainMenu_Watcher;

		public static void RegisterValues()
		{
			Slugcat_Watcher = new MenuScene.SceneID("Slugcat_Watcher", register: true);
			End_Watcher_A = new MenuScene.SceneID("End_Watcher_A", register: true);
			End_Watcher_B = new MenuScene.SceneID("End_Watcher_B", register: true);
			End_Watcher_C = new MenuScene.SceneID("End_Watcher_C", register: true);
			End_Watcher_D = new MenuScene.SceneID("End_Watcher_D", register: true);
			Ripple_Sleep = new MenuScene.SceneID("Ripple_Sleep", register: true);
			Ripple_Sleep_Death = new MenuScene.SceneID("Ripple_Sleep_Death", register: true);
			Dream_Watcher1 = new MenuScene.SceneID("Dream_Watcher1", register: true);
			Dream_Watcher2 = new MenuScene.SceneID("Dream_Watcher2", register: true);
			Dream_Watcher3 = new MenuScene.SceneID("Dream_Watcher3", register: true);
			Dream_Watcher4 = new MenuScene.SceneID("Dream_Watcher4", register: true);
			Dream_Watcher5 = new MenuScene.SceneID("Dream_Watcher5", register: true);
			Dream_Watcher6 = new MenuScene.SceneID("Dream_Watcher6", register: true);
			Dream_SpinningTop1 = new MenuScene.SceneID("Dream_SpinningTop1", register: true);
			Dream_SpinningTop2 = new MenuScene.SceneID("Dream_SpinningTop2", register: true);
			Dream_SpinningTop3 = new MenuScene.SceneID("Dream_SpinningTop3", register: true);
			Dream_SpinningTop4 = new MenuScene.SceneID("Dream_SpinningTop4", register: true);
			Dream_SpinningTop5 = new MenuScene.SceneID("Dream_SpinningTop5", register: true);
			Dream_SpinningTop6 = new MenuScene.SceneID("Dream_SpinningTop6", register: true);
			Dream_SpinningTop7 = new MenuScene.SceneID("Dream_SpinningTop7", register: true);
			Dream_Rot1 = new MenuScene.SceneID("Dream_Rot1", register: true);
			Dream_Rot2 = new MenuScene.SceneID("Dream_Rot2", register: true);
			Dream_Rot3 = new MenuScene.SceneID("Dream_Rot3", register: true);
			Dream_Rot4 = new MenuScene.SceneID("Dream_Rot4", register: true);
			Dream_Rot5 = new MenuScene.SceneID("Dream_Rot5", register: true);
			Dream_Rot6 = new MenuScene.SceneID("Dream_Rot6", register: true);
			Dream_Rot7 = new MenuScene.SceneID("Dream_Rot7", register: true);
			Dream_Rot8 = new MenuScene.SceneID("Dream_Rot8", register: true);
			Dream_Rot9 = new MenuScene.SceneID("Dream_Rot9", register: true);
			Dream_Rot10 = new MenuScene.SceneID("Dream_Rot10", register: true);
			Ending_VoidBath1 = new MenuScene.SceneID("Ending_VoidBath1", register: true);
			Ending_VoidBath2 = new MenuScene.SceneID("Ending_VoidBath2", register: true);
			Ending_VoidBath3 = new MenuScene.SceneID("Ending_VoidBath3", register: true);
			Ending_SpinningTop1 = new MenuScene.SceneID("Ending_SpinningTop1", register: true);
			Ending_SpinningTop2 = new MenuScene.SceneID("Ending_SpinningTop2", register: true);
			Ending_SpinningTop3 = new MenuScene.SceneID("Ending_SpinningTop3", register: true);
			Ending_Prince1 = new MenuScene.SceneID("Ending_Prince1", register: true);
			Ending_Prince2 = new MenuScene.SceneID("Ending_Prince2", register: true);
			Ending_Prince3 = new MenuScene.SceneID("Ending_Prince3", register: true);
			Ending_Prince4 = new MenuScene.SceneID("Ending_Prince4", register: true);
			Ending_Prince5 = new MenuScene.SceneID("Ending_Prince5", register: true);
			Ending_Prince6 = new MenuScene.SceneID("Ending_Prince6", register: true);
			Dream_VoidWeaver1 = new MenuScene.SceneID("Dream_VoidWeaver1", register: true);
			Dream_VoidWeaver2 = new MenuScene.SceneID("Dream_VoidWeaver2", register: true);
			Dream_VoidWeaver3 = new MenuScene.SceneID("Dream_VoidWeaver3", register: true);
			Dream_VoidWeaver4 = new MenuScene.SceneID("Dream_VoidWeaver4", register: true);
			Dream_VoidWeaver5 = new MenuScene.SceneID("Dream_VoidWeaver5", register: true);
			Dream_VoidWeaver6 = new MenuScene.SceneID("Dream_VoidWeaver6", register: true);
			Dream_VoidWeaver7 = new MenuScene.SceneID("Dream_VoidWeaver7", register: true);
			Ending_Weaver1 = new MenuScene.SceneID("Ending_Weaver1", register: true);
			Ending_Weaver2 = new MenuScene.SceneID("Ending_Weaver2", register: true);
			Ending_Weaver3 = new MenuScene.SceneID("Ending_Weaver3", register: true);
			Ending_Weaver4 = new MenuScene.SceneID("Ending_Weaver4", register: true);
			Ending_Weaver5 = new MenuScene.SceneID("Ending_Weaver5", register: true);
			Ending_Weaver6 = new MenuScene.SceneID("Ending_Weaver6", register: true);
			Dream_Terrace = new MenuScene.SceneID("Dream_Terrace", register: true);
			MainMenu_Watcher = new MenuScene.SceneID("MainMenu_Watcher", register: true);
		}

		public static void UnregisterValues()
		{
			Slugcat_Watcher?.Unregister();
			Slugcat_Watcher = null;
			End_Watcher_A?.Unregister();
			End_Watcher_A = null;
			End_Watcher_B?.Unregister();
			End_Watcher_B = null;
			End_Watcher_C?.Unregister();
			End_Watcher_C = null;
			End_Watcher_D?.Unregister();
			End_Watcher_D = null;
			Ripple_Sleep?.Unregister();
			Ripple_Sleep = null;
			Ripple_Sleep_Death?.Unregister();
			Ripple_Sleep_Death = null;
			Dream_Watcher1?.Unregister();
			Dream_Watcher1 = null;
			Dream_Watcher2?.Unregister();
			Dream_Watcher2 = null;
			Dream_Watcher3?.Unregister();
			Dream_Watcher3 = null;
			Dream_Watcher4?.Unregister();
			Dream_Watcher4 = null;
			Dream_Watcher5?.Unregister();
			Dream_Watcher5 = null;
			Dream_Watcher6?.Unregister();
			Dream_Watcher6 = null;
			Dream_SpinningTop1?.Unregister();
			Dream_SpinningTop1 = null;
			Dream_SpinningTop2?.Unregister();
			Dream_SpinningTop2 = null;
			Dream_SpinningTop3?.Unregister();
			Dream_SpinningTop3 = null;
			Dream_SpinningTop4?.Unregister();
			Dream_SpinningTop4 = null;
			Dream_SpinningTop5?.Unregister();
			Dream_SpinningTop5 = null;
			Dream_SpinningTop6?.Unregister();
			Dream_SpinningTop6 = null;
			Dream_SpinningTop7?.Unregister();
			Dream_SpinningTop7 = null;
			Dream_Rot1?.Unregister();
			Dream_Rot1 = null;
			Dream_Rot2?.Unregister();
			Dream_Rot2 = null;
			Dream_Rot3?.Unregister();
			Dream_Rot3 = null;
			Dream_Rot4?.Unregister();
			Dream_Rot4 = null;
			Dream_Rot5?.Unregister();
			Dream_Rot5 = null;
			Dream_Rot6?.Unregister();
			Dream_Rot6 = null;
			Dream_Rot7?.Unregister();
			Dream_Rot7 = null;
			Dream_Rot8?.Unregister();
			Dream_Rot8 = null;
			Dream_Rot9?.Unregister();
			Dream_Rot9 = null;
			Dream_Rot10?.Unregister();
			Dream_Rot10 = null;
			Ending_VoidBath1?.Unregister();
			Ending_VoidBath1 = null;
			Ending_VoidBath2?.Unregister();
			Ending_VoidBath2 = null;
			Ending_VoidBath3?.Unregister();
			Ending_VoidBath3 = null;
			Ending_SpinningTop1?.Unregister();
			Ending_SpinningTop1 = null;
			Ending_SpinningTop2?.Unregister();
			Ending_SpinningTop2 = null;
			Ending_SpinningTop3?.Unregister();
			Ending_SpinningTop3 = null;
			Ending_Prince1?.Unregister();
			Ending_Prince1 = null;
			Ending_Prince2?.Unregister();
			Ending_Prince2 = null;
			Ending_Prince3?.Unregister();
			Ending_Prince3 = null;
			Ending_Prince4?.Unregister();
			Ending_Prince4 = null;
			Ending_Prince5?.Unregister();
			Ending_Prince5 = null;
			Ending_Prince6?.Unregister();
			Ending_Prince6 = null;
			Dream_VoidWeaver1?.Unregister();
			Dream_VoidWeaver1 = null;
			Dream_VoidWeaver2?.Unregister();
			Dream_VoidWeaver2 = null;
			Dream_VoidWeaver3?.Unregister();
			Dream_VoidWeaver3 = null;
			Dream_VoidWeaver4?.Unregister();
			Dream_VoidWeaver4 = null;
			Dream_VoidWeaver5?.Unregister();
			Dream_VoidWeaver5 = null;
			Dream_VoidWeaver6?.Unregister();
			Dream_VoidWeaver6 = null;
			Dream_VoidWeaver7?.Unregister();
			Dream_VoidWeaver7 = null;
			Ending_Weaver1?.Unregister();
			Ending_Weaver1 = null;
			Ending_Weaver2?.Unregister();
			Ending_Weaver2 = null;
			Ending_Weaver3?.Unregister();
			Ending_Weaver3 = null;
			Ending_Weaver4?.Unregister();
			Ending_Weaver4 = null;
			Ending_Weaver5?.Unregister();
			Ending_Weaver5 = null;
			Ending_Weaver6?.Unregister();
			Ending_Weaver6 = null;
			Dream_Terrace?.Unregister();
			Dream_Terrace = null;
			MainMenu_Watcher?.Unregister();
			MainMenu_Watcher = null;
		}
	}

	public class SlideShowID
	{
		public static SlideShow.SlideShowID DreamWatcher;

		public static SlideShow.SlideShowID DreamSpinningTop;

		public static SlideShow.SlideShowID DreamRot;

		public static SlideShow.SlideShowID DreamTerrace;

		public static SlideShow.SlideShowID EndingVoidBath;

		public static SlideShow.SlideShowID EndingSpinningTop;

		public static SlideShow.SlideShowID EndingRot;

		public static SlideShow.SlideShowID DreamVoidWeaver;

		public static SlideShow.SlideShowID EndingVoidWeaver;

		public static void RegisterValues()
		{
			DreamWatcher = new SlideShow.SlideShowID("DreamWatcher", register: true);
			DreamSpinningTop = new SlideShow.SlideShowID("DreamSpinningTop", register: true);
			DreamRot = new SlideShow.SlideShowID("DreamRot", register: true);
			DreamTerrace = new SlideShow.SlideShowID("DreamTerrace", register: true);
			EndingVoidBath = new SlideShow.SlideShowID("EndingVoidBath", register: true);
			EndingSpinningTop = new SlideShow.SlideShowID("EndingSpinningTop", register: true);
			EndingRot = new SlideShow.SlideShowID("EndingRot", register: true);
			DreamVoidWeaver = new SlideShow.SlideShowID("DreamVoidWeaver", register: true);
			EndingVoidWeaver = new SlideShow.SlideShowID("EndingVoidWeaver", register: true);
		}

		public static void UnregisterValues()
		{
			DreamWatcher?.Unregister();
			DreamWatcher = null;
			DreamSpinningTop?.Unregister();
			DreamSpinningTop = null;
			DreamRot?.Unregister();
			DreamRot = null;
			DreamTerrace?.Unregister();
			DreamTerrace = null;
			EndingVoidBath?.Unregister();
			EndingVoidBath = null;
			EndingSpinningTop?.Unregister();
			EndingSpinningTop = null;
			EndingRot?.Unregister();
			EndingRot = null;
			DreamVoidWeaver?.Unregister();
			DreamVoidWeaver = null;
			EndingVoidWeaver?.Unregister();
			EndingVoidWeaver = null;
		}
	}

	public class DreamID
	{
		public static DreamsState.DreamID DreamSpinningTop;

		public static DreamsState.DreamID DreamRot;

		public static DreamsState.DreamID DreamVoidWeaver;

		public static DreamsState.DreamID DreamTerrace;

		public static void RegisterValues()
		{
			DreamSpinningTop = new DreamsState.DreamID("DreamSpinningTop", register: true);
			DreamRot = new DreamsState.DreamID("DreamRot", register: true);
			DreamVoidWeaver = new DreamsState.DreamID("DreamVoidWeaver", register: true);
			DreamTerrace = new DreamsState.DreamID("DreamTerrace", register: true);
		}

		public static void UnregisterValues()
		{
			DreamSpinningTop?.Unregister();
			DreamSpinningTop = null;
			DreamRot?.Unregister();
			DreamRot = null;
			DreamVoidWeaver?.Unregister();
			DreamVoidWeaver = null;
			DreamTerrace?.Unregister();
			DreamTerrace = null;
		}
	}

	public class AbstractObjectType
	{
		public static AbstractPhysicalObject.AbstractObjectType RippleSpawn;

		public static AbstractPhysicalObject.AbstractObjectType FireSpriteLarva;

		public static AbstractPhysicalObject.AbstractObjectType Boomerang;

		public static AbstractPhysicalObject.AbstractObjectType PrinceBulb;

		public static AbstractPhysicalObject.AbstractObjectType Prince;

		public static AbstractPhysicalObject.AbstractObjectType SpinToy;

		public static AbstractPhysicalObject.AbstractObjectType BallToy;

		public static AbstractPhysicalObject.AbstractObjectType SoftToy;

		public static AbstractPhysicalObject.AbstractObjectType WeirdToy;

		public static AbstractPhysicalObject.AbstractObjectType KnotSpawn;

		public static AbstractPhysicalObject.AbstractObjectType RippleJelly;

		public static void RegisterValues()
		{
			RippleSpawn = new AbstractPhysicalObject.AbstractObjectType("RippleSpawn", register: true);
			FireSpriteLarva = new AbstractPhysicalObject.AbstractObjectType("FireSpriteLarva", register: true);
			Boomerang = new AbstractPhysicalObject.AbstractObjectType("Boomerang", register: true);
			PrinceBulb = new AbstractPhysicalObject.AbstractObjectType("PrinceBulb", register: true);
			Prince = new AbstractPhysicalObject.AbstractObjectType("Prince", register: true);
			SpinToy = new AbstractPhysicalObject.AbstractObjectType("SpinToy", register: true);
			BallToy = new AbstractPhysicalObject.AbstractObjectType("BallToy", register: true);
			SoftToy = new AbstractPhysicalObject.AbstractObjectType("SoftToy", register: true);
			WeirdToy = new AbstractPhysicalObject.AbstractObjectType("WeirdToy", register: true);
			KnotSpawn = new AbstractPhysicalObject.AbstractObjectType("KnotSpawn", register: true);
			RippleJelly = new AbstractPhysicalObject.AbstractObjectType("RippleJelly", register: true);
		}

		public static void UnregisterValues()
		{
			RippleSpawn?.Unregister();
			RippleSpawn = null;
			FireSpriteLarva?.Unregister();
			FireSpriteLarva = null;
			Boomerang?.Unregister();
			Boomerang = null;
			PrinceBulb?.Unregister();
			PrinceBulb = null;
			Prince?.Unregister();
			Prince = null;
			SpinToy?.Unregister();
			SpinToy = null;
			BallToy?.Unregister();
			BallToy = null;
			SoftToy?.Unregister();
			SoftToy = null;
			WeirdToy?.Unregister();
			WeirdToy = null;
			KnotSpawn?.Unregister();
			KnotSpawn = null;
			RippleJelly?.Unregister();
			RippleJelly = null;
		}
	}

	public class RoomEffectType
	{
		public static RoomSettings.RoomEffect.Type RippleSettings;

		public static RoomSettings.RoomEffect.Type OuterRimView;

		public static RoomSettings.RoomEffect.Type SentientRotParticles;

		public static RoomSettings.RoomEffect.Type SentientRotInfection;

		public static RoomSettings.RoomEffect.Type AncientUrbanView;

		public static RoomSettings.RoomEffect.Type SaltFlakes;

		public static RoomSettings.RoomEffect.Type InnerOuterRimView;

		public static RoomSettings.RoomEffect.Type ToxicBrineWater;

		public static RoomSettings.RoomEffect.Type SurfaceSandstorm;

		public static RoomSettings.RoomEffect.Type SKLightning;

		public static RoomSettings.RoomEffect.Type SKLightningHue;

		public static RoomSettings.RoomEffect.Type FlowerWind;

		public static void RegisterValues()
		{
			RippleSettings = new RoomSettings.RoomEffect.Type("Ripple Settings", register: true);
			OuterRimView = new RoomSettings.RoomEffect.Type("Outer Rim View", register: true);
			SentientRotParticles = new RoomSettings.RoomEffect.Type("Sentient Rot Spores", register: true);
			SentientRotInfection = new RoomSettings.RoomEffect.Type("Sentient Rot Infection", register: true);
			AncientUrbanView = new RoomSettings.RoomEffect.Type("Ancient Urban View", register: true);
			SaltFlakes = new RoomSettings.RoomEffect.Type("Salt Flakes", register: true);
			InnerOuterRimView = new RoomSettings.RoomEffect.Type("Inner Outer Rim View", register: true);
			ToxicBrineWater = new RoomSettings.RoomEffect.Type("Toxic Brine Water", register: true);
			SurfaceSandstorm = new RoomSettings.RoomEffect.Type("SurfaceSandstorm", register: true);
			SKLightning = new RoomSettings.RoomEffect.Type("SKLightning", register: true);
			SKLightningHue = new RoomSettings.RoomEffect.Type("SKLightningHue", register: true);
			FlowerWind = new RoomSettings.RoomEffect.Type("FlowerWind", register: true);
		}

		public static void UnregisterValues()
		{
			RippleSettings?.Unregister();
			RippleSettings = null;
			OuterRimView?.Unregister();
			OuterRimView = null;
			SentientRotParticles?.Unregister();
			SentientRotParticles = null;
			SentientRotInfection?.Unregister();
			SentientRotInfection = null;
			AncientUrbanView?.Unregister();
			AncientUrbanView = null;
			SaltFlakes?.Unregister();
			SaltFlakes = null;
			InnerOuterRimView?.Unregister();
			InnerOuterRimView = null;
			ToxicBrineWater?.Unregister();
			ToxicBrineWater = null;
			SurfaceSandstorm?.Unregister();
			SurfaceSandstorm = null;
			SKLightning?.Unregister();
			SKLightning = null;
			SKLightningHue?.Unregister();
			SKLightningHue = null;
			FlowerWind?.Unregister();
			FlowerWind = null;
		}
	}

	public class DataPearlType
	{
		public static DataPearl.AbstractDataPearl.DataPearlType WORA;

		public static DataPearl.AbstractDataPearl.DataPearlType WAUA;

		public static DataPearl.AbstractDataPearl.DataPearlType DRONE;

		public static DataPearl.AbstractDataPearl.DataPearlType ABSTRACT;

		public static DataPearl.AbstractDataPearl.DataPearlType AUDIO_VOICEWIND1;

		public static DataPearl.AbstractDataPearl.DataPearlType AUDIO_VOICEWIND2;

		public static DataPearl.AbstractDataPearl.DataPearlType AUDIO_JAM1;

		public static DataPearl.AbstractDataPearl.DataPearlType AUDIO_JAM2;

		public static DataPearl.AbstractDataPearl.DataPearlType AUDIO_JAM3;

		public static DataPearl.AbstractDataPearl.DataPearlType AUDIO_JAM4;

		public static DataPearl.AbstractDataPearl.DataPearlType AUDIO_GROOVE;

		public static DataPearl.AbstractDataPearl.DataPearlType TEXT_AUDIO_TALKSHOW;

		public static DataPearl.AbstractDataPearl.DataPearlType TEXT_NOTIONOFSELF;

		public static DataPearl.AbstractDataPearl.DataPearlType TEXT_SECRET;

		public static DataPearl.AbstractDataPearl.DataPearlType TEXT_CONTEMPT;

		public static DataPearl.AbstractDataPearl.DataPearlType TEXT_STARDUST;

		public static DataPearl.AbstractDataPearl.DataPearlType TEXT_KITESDAY;

		public static void RegisterValues()
		{
			WORA = new DataPearl.AbstractDataPearl.DataPearlType("WORA", register: true);
			WAUA = new DataPearl.AbstractDataPearl.DataPearlType("WAUA", register: true);
			DRONE = new DataPearl.AbstractDataPearl.DataPearlType("DRONE", register: true);
			ABSTRACT = new DataPearl.AbstractDataPearl.DataPearlType("ABSTRACT", register: true);
			AUDIO_VOICEWIND1 = new DataPearl.AbstractDataPearl.DataPearlType("AUDIO_VOICEWIND1", register: true);
			AUDIO_VOICEWIND2 = new DataPearl.AbstractDataPearl.DataPearlType("AUDIO_VOICEWIND2", register: true);
			AUDIO_JAM1 = new DataPearl.AbstractDataPearl.DataPearlType("AUDIO_JAM1", register: true);
			AUDIO_JAM2 = new DataPearl.AbstractDataPearl.DataPearlType("AUDIO_JAM2", register: true);
			AUDIO_JAM3 = new DataPearl.AbstractDataPearl.DataPearlType("AUDIO_JAM3", register: true);
			AUDIO_JAM4 = new DataPearl.AbstractDataPearl.DataPearlType("AUDIO_JAM4", register: true);
			AUDIO_GROOVE = new DataPearl.AbstractDataPearl.DataPearlType("AUDIO_GROOVE", register: true);
			TEXT_AUDIO_TALKSHOW = new DataPearl.AbstractDataPearl.DataPearlType("TEXT_AUDIO_TALKSHOW", register: true);
			TEXT_NOTIONOFSELF = new DataPearl.AbstractDataPearl.DataPearlType("TEXT_NOTIONOFSELF", register: true);
			TEXT_SECRET = new DataPearl.AbstractDataPearl.DataPearlType("TEXT_SECRET", register: true);
			TEXT_CONTEMPT = new DataPearl.AbstractDataPearl.DataPearlType("TEXT_CONTEMPT", register: true);
			TEXT_STARDUST = new DataPearl.AbstractDataPearl.DataPearlType("TEXT_STARDUST", register: true);
			TEXT_KITESDAY = new DataPearl.AbstractDataPearl.DataPearlType("TEXT_KITESDAY", register: true);
		}

		public static void UnregisterValues()
		{
			WORA?.Unregister();
			WORA = null;
			WAUA?.Unregister();
			WAUA = null;
			DRONE?.Unregister();
			DRONE = null;
			ABSTRACT?.Unregister();
			ABSTRACT = null;
			AUDIO_VOICEWIND1?.Unregister();
			AUDIO_VOICEWIND1 = null;
			AUDIO_VOICEWIND2?.Unregister();
			AUDIO_VOICEWIND2 = null;
			AUDIO_JAM1?.Unregister();
			AUDIO_JAM1 = null;
			AUDIO_JAM2?.Unregister();
			AUDIO_JAM2 = null;
			AUDIO_JAM3?.Unregister();
			AUDIO_JAM3 = null;
			AUDIO_JAM4?.Unregister();
			AUDIO_JAM4 = null;
			AUDIO_GROOVE?.Unregister();
			AUDIO_GROOVE = null;
			TEXT_AUDIO_TALKSHOW?.Unregister();
			TEXT_AUDIO_TALKSHOW = null;
			TEXT_NOTIONOFSELF?.Unregister();
			TEXT_NOTIONOFSELF = null;
			TEXT_SECRET?.Unregister();
			TEXT_SECRET = null;
			TEXT_CONTEMPT?.Unregister();
			TEXT_CONTEMPT = null;
			TEXT_STARDUST?.Unregister();
			TEXT_STARDUST = null;
			TEXT_KITESDAY?.Unregister();
			TEXT_KITESDAY = null;
		}
	}

	public class PlacedObjectType
	{
		public static PlacedObject.Type CosmeticRipple;

		public static PlacedObject.Type SpinningTopSpot;

		public static PlacedObject.Type WeaverSpot;

		public static PlacedObject.Type FlameJet;

		public static PlacedObject.Type AetherRainbow;

		public static PlacedObject.Type WallLight;

		public static PlacedObject.Type PlacedBoxWorm;

		public static PlacedObject.Type PlacedFireSprite;

		public static PlacedObject.Type PlacedDrillCrab;

		public static PlacedObject.Type PlacedRats;

		public static PlacedObject.Type PlacedFrog;

		public static PlacedObject.Type PlacedLoach;

		public static PlacedObject.Type PlacedRotLoach;

		public static PlacedObject.Type PlacedBarnacles;

		public static PlacedObject.Type SKLightningRod;

		public static PlacedObject.Type PrinceBulb;

		public static PlacedObject.Type RottenDangleFruit;

		public static PlacedObject.Type RottenSeedCob;

		public static PlacedObject.Type UrbanLife;

		public static PlacedObject.Type UrbanLifePath;

		public static PlacedObject.Type SpinToy;

		public static PlacedObject.Type BallToy;

		public static PlacedObject.Type SoftToy;

		public static PlacedObject.Type WeirdToy;

		public static PlacedObject.Type UrbanCandles;

		public static PlacedObject.Type UrbanCandleHolder;

		public static PlacedObject.Type KarmaFlowerPatch;

		public static PlacedObject.Type ARZapperOmni;

		public static PlacedObject.Type ARZapperVertical;

		public static PlacedObject.Type ARZapperHorizontal;

		public static PlacedObject.Type DaemonClaw;

		public static PlacedObject.Type DaemonEye;

		public static PlacedObject.Type DaemonCrown;

		public static PlacedObject.Type PearlReader;

		public static PlacedObject.Type BigSkyWhaleSpawner;

		public static PlacedObject.Type BigSkyWhaleTrigger;

		public static PlacedObject.Type TowerCrabSpawner;

		public static void RegisterValues()
		{
			CosmeticRipple = new PlacedObject.Type("CosmeticRipple", register: true);
			SpinningTopSpot = new PlacedObject.Type("SpinningTopSpot", register: true);
			WeaverSpot = new PlacedObject.Type("WeaverSpot", register: true);
			FlameJet = new PlacedObject.Type("FlameJet", register: true);
			AetherRainbow = new PlacedObject.Type("AetherRainbow", register: true);
			WallLight = new PlacedObject.Type("WallLight", register: true);
			PlacedBoxWorm = new PlacedObject.Type("PlacedBoxWorm", register: true);
			PlacedFireSprite = new PlacedObject.Type("PlacedFireSprite", register: true);
			PlacedDrillCrab = new PlacedObject.Type("PlacedDrillCrab", register: true);
			PlacedRats = new PlacedObject.Type("PlacedRats", register: true);
			PlacedFrog = new PlacedObject.Type("PlacedFrog", register: true);
			PlacedLoach = new PlacedObject.Type("PlacedLoach", register: true);
			PlacedRotLoach = new PlacedObject.Type("PlacedRotLoach", register: true);
			PlacedBarnacles = new PlacedObject.Type("PlacedBarnacles", register: true);
			SKLightningRod = new PlacedObject.Type("SKLightningRod", register: true);
			PrinceBulb = new PlacedObject.Type("PrinceBulb", register: true);
			RottenDangleFruit = new PlacedObject.Type("RottenDangleFruit", register: true);
			RottenSeedCob = new PlacedObject.Type("RottenSeedCob", register: true);
			UrbanLife = new PlacedObject.Type("UrbanLife", register: true);
			UrbanLifePath = new PlacedObject.Type("UrbanLifePath", register: true);
			SpinToy = new PlacedObject.Type("SpinToy", register: true);
			BallToy = new PlacedObject.Type("BallToy", register: true);
			SoftToy = new PlacedObject.Type("SoftToy", register: true);
			WeirdToy = new PlacedObject.Type("WeirdToy", register: true);
			UrbanCandles = new PlacedObject.Type("UrbanCandles", register: true);
			UrbanCandleHolder = new PlacedObject.Type("UrbanCandleHolder", register: true);
			KarmaFlowerPatch = new PlacedObject.Type("KarmaFlowerPatch", register: true);
			ARZapperOmni = new PlacedObject.Type("ARZapperOmni", register: true);
			ARZapperVertical = new PlacedObject.Type("ARZapperVertical", register: true);
			ARZapperHorizontal = new PlacedObject.Type("ARZapperHorizontal", register: true);
			DaemonClaw = new PlacedObject.Type("DaemonClaw", register: true);
			DaemonEye = new PlacedObject.Type("DaemonEye", register: true);
			DaemonCrown = new PlacedObject.Type("DaemonCrown", register: true);
			PearlReader = new PlacedObject.Type("PearlReader", register: true);
			BigSkyWhaleSpawner = new PlacedObject.Type("BigSkyWhaleSpawner", register: true);
			BigSkyWhaleTrigger = new PlacedObject.Type("BigSkyWhaleTrigger", register: true);
			TowerCrabSpawner = new PlacedObject.Type("TowerCrabSpawner", register: true);
		}

		public static void UnregisterValues()
		{
			CosmeticRipple?.Unregister();
			CosmeticRipple = null;
			SpinningTopSpot?.Unregister();
			SpinningTopSpot = null;
			WeaverSpot?.Unregister();
			WeaverSpot = null;
			FlameJet?.Unregister();
			FlameJet = null;
			AetherRainbow?.Unregister();
			AetherRainbow = null;
			WallLight?.Unregister();
			WallLight = null;
			PlacedBoxWorm?.Unregister();
			PlacedBoxWorm = null;
			PlacedFireSprite?.Unregister();
			PlacedFireSprite = null;
			PlacedDrillCrab?.Unregister();
			PlacedDrillCrab = null;
			PlacedRats?.Unregister();
			PlacedRats = null;
			PlacedFrog?.Unregister();
			PlacedFrog = null;
			PlacedLoach?.Unregister();
			PlacedLoach = null;
			PlacedRotLoach?.Unregister();
			PlacedRotLoach = null;
			PlacedBarnacles?.Unregister();
			PlacedBarnacles = null;
			SKLightningRod?.Unregister();
			SKLightningRod = null;
			PrinceBulb?.Unregister();
			PrinceBulb = null;
			RottenDangleFruit?.Unregister();
			RottenDangleFruit = null;
			RottenSeedCob?.Unregister();
			RottenSeedCob = null;
			UrbanLife?.Unregister();
			UrbanLife = null;
			UrbanLifePath?.Unregister();
			UrbanLifePath = null;
			SpinToy?.Unregister();
			SpinToy = null;
			BallToy?.Unregister();
			BallToy = null;
			SoftToy?.Unregister();
			SoftToy = null;
			WeirdToy?.Unregister();
			WeirdToy = null;
			UrbanCandles?.Unregister();
			UrbanCandles = null;
			UrbanCandleHolder?.Unregister();
			UrbanCandleHolder = null;
			KarmaFlowerPatch?.Unregister();
			KarmaFlowerPatch = null;
			ARZapperOmni?.Unregister();
			ARZapperOmni = null;
			ARZapperVertical?.Unregister();
			ARZapperVertical = null;
			ARZapperHorizontal?.Unregister();
			ARZapperHorizontal = null;
			DaemonClaw?.Unregister();
			DaemonClaw = null;
			DaemonEye?.Unregister();
			DaemonEye = null;
			DaemonCrown?.Unregister();
			DaemonCrown = null;
			PearlReader?.Unregister();
			PearlReader = null;
			BigSkyWhaleSpawner?.Unregister();
			BigSkyWhaleSpawner = null;
			BigSkyWhaleTrigger?.Unregister();
			BigSkyWhaleTrigger = null;
			TowerCrabSpawner?.Unregister();
			TowerCrabSpawner = null;
		}
	}

	public class CreatureTemplateType
	{
		public static CreatureTemplate.Type DrillCrab;

		public static CreatureTemplate.Type TowerCrab;

		public static CreatureTemplate.Type Barnacle;

		public static CreatureTemplate.Type SandGrub;

		public static CreatureTemplate.Type BigSandGrub;

		public static CreatureTemplate.Type BigMoth;

		public static CreatureTemplate.Type SmallMoth;

		public static CreatureTemplate.Type BoxWorm;

		public static CreatureTemplate.Type FireSprite;

		public static CreatureTemplate.Type Rattler;

		public static CreatureTemplate.Type SkyWhale;

		public static CreatureTemplate.Type ScavengerTemplar;

		public static CreatureTemplate.Type ScavengerDisciple;

		public static CreatureTemplate.Type Loach;

		public static CreatureTemplate.Type RotLoach;

		public static CreatureTemplate.Type BlizzardLizard;

		public static CreatureTemplate.Type BasiliskLizard;

		public static CreatureTemplate.Type IndigoLizard;

		public static CreatureTemplate.Type PeachLizard;

		public static CreatureTemplate.Type Rat;

		public static CreatureTemplate.Type Frog;

		public static CreatureTemplate.Type Tardigrade;

		public static CreatureTemplate.Type GrappleSnake;

		public static CreatureTemplate.Type Millipede;

		public static CreatureTemplate.Type Angler;

		public static CreatureTemplate.Type RippleSpider;

		public static CreatureTemplate.Type MothGrub;

		public static void RegisterValues()
		{
			DrillCrab = new CreatureTemplate.Type("DrillCrab", register: true);
			TowerCrab = new CreatureTemplate.Type("TowerCrab", register: true);
			Barnacle = new CreatureTemplate.Type("Barnacle", register: true);
			SandGrub = new CreatureTemplate.Type("SandGrub", register: true);
			BigSandGrub = new CreatureTemplate.Type("BigSandGrub", register: true);
			BigMoth = new CreatureTemplate.Type("BigMoth", register: true);
			SmallMoth = new CreatureTemplate.Type("SmallMoth", register: true);
			BoxWorm = new CreatureTemplate.Type("BoxWorm", register: true);
			FireSprite = new CreatureTemplate.Type("FireSprite", register: true);
			Rattler = new CreatureTemplate.Type("Rattler", register: true);
			SkyWhale = new CreatureTemplate.Type("SkyWhale", register: true);
			ScavengerTemplar = new CreatureTemplate.Type("ScavengerTemplar", register: true);
			ScavengerDisciple = new CreatureTemplate.Type("ScavengerDisciple", register: true);
			Loach = new CreatureTemplate.Type("Loach", register: true);
			RotLoach = new CreatureTemplate.Type("RotLoach", register: true);
			BlizzardLizard = new CreatureTemplate.Type("BlizzardLizard", register: true);
			BasiliskLizard = new CreatureTemplate.Type("BasiliskLizard", register: true);
			IndigoLizard = new CreatureTemplate.Type("IndigoLizard", register: true);
			PeachLizard = new CreatureTemplate.Type("PeachLizard", register: true);
			Rat = new CreatureTemplate.Type("Rat", register: true);
			Frog = new CreatureTemplate.Type("Frog", register: true);
			Tardigrade = new CreatureTemplate.Type("Tardigrade", register: true);
			GrappleSnake = new CreatureTemplate.Type("GrappleSnake", register: true);
			Millipede = new CreatureTemplate.Type("Millipede", register: true);
			Angler = new CreatureTemplate.Type("Angler", register: true);
			RippleSpider = new CreatureTemplate.Type("RippleSpider", register: true);
			MothGrub = new CreatureTemplate.Type("MothGrub", register: true);
		}

		public static void UnregisterValues()
		{
			DrillCrab?.Unregister();
			DrillCrab = null;
			TowerCrab?.Unregister();
			TowerCrab = null;
			Barnacle?.Unregister();
			Barnacle = null;
			SandGrub?.Unregister();
			SandGrub = null;
			BigSandGrub?.Unregister();
			BigSandGrub = null;
			BigMoth?.Unregister();
			BigMoth = null;
			SmallMoth?.Unregister();
			SmallMoth = null;
			BoxWorm?.Unregister();
			BoxWorm = null;
			FireSprite?.Unregister();
			FireSprite = null;
			Rattler?.Unregister();
			Rattler = null;
			SkyWhale?.Unregister();
			SkyWhale = null;
			ScavengerTemplar?.Unregister();
			ScavengerTemplar = null;
			ScavengerDisciple?.Unregister();
			ScavengerDisciple = null;
			Loach?.Unregister();
			Loach = null;
			RotLoach?.Unregister();
			RotLoach = null;
			BlizzardLizard?.Unregister();
			BlizzardLizard = null;
			BasiliskLizard?.Unregister();
			BasiliskLizard = null;
			IndigoLizard?.Unregister();
			IndigoLizard = null;
			PeachLizard?.Unregister();
			PeachLizard = null;
			Rat?.Unregister();
			Rat = null;
			Frog?.Unregister();
			Frog = null;
			Tardigrade?.Unregister();
			Tardigrade = null;
			GrappleSnake?.Unregister();
			GrappleSnake = null;
			Millipede?.Unregister();
			Millipede = null;
			Angler?.Unregister();
			Angler = null;
			RippleSpider?.Unregister();
			RippleSpider = null;
			MothGrub?.Unregister();
			MothGrub = null;
		}
	}

	public class CommunityID
	{
		public static CreatureCommunities.CommunityID BoxWorms;

		public static void RegisterValues()
		{
			BoxWorms = new CreatureCommunities.CommunityID("BoxWorms", register: true);
		}

		public static void UnregisterValues()
		{
			BoxWorms?.Unregister();
			BoxWorms = null;
		}
	}

	public class Tutorial
	{
		public static DeathPersistentSaveData.Tutorial stableWarpTutorial;

		public static DeathPersistentSaveData.Tutorial badWarpTutorial;

		public static DeathPersistentSaveData.Tutorial warpFatigueTutorial;

		public static DeathPersistentSaveData.Tutorial warpExhaustionTutorial;

		public static DeathPersistentSaveData.Tutorial rippleEggTutorial;

		public static void RegisterValues()
		{
			stableWarpTutorial = new DeathPersistentSaveData.Tutorial("stableWarpTutorial", register: true);
			badWarpTutorial = new DeathPersistentSaveData.Tutorial("badWarpTutorial", register: true);
			warpFatigueTutorial = new DeathPersistentSaveData.Tutorial("warpFatigueTutorial", register: true);
			warpExhaustionTutorial = new DeathPersistentSaveData.Tutorial("warpExhaustionTutorial", register: true);
			rippleEggTutorial = new DeathPersistentSaveData.Tutorial("rippleEggTutorial", register: true);
		}

		public static void UnregisterValues()
		{
			stableWarpTutorial?.Unregister();
			stableWarpTutorial = null;
			badWarpTutorial?.Unregister();
			badWarpTutorial = null;
			warpFatigueTutorial?.Unregister();
			warpFatigueTutorial = null;
			warpExhaustionTutorial?.Unregister();
			warpExhaustionTutorial = null;
			rippleEggTutorial?.Unregister();
			rippleEggTutorial = null;
		}
	}

	public class GhostID
	{
		public static GhostWorldPresence.GhostID SpinningTop;

		public static void RegisterValues()
		{
			SpinningTop = new GhostWorldPresence.GhostID("SpinningTop", register: true);
		}

		public static void UnregisterValues()
		{
			SpinningTop?.Unregister();
			SpinningTop = null;
		}
	}

	public class ConversationID
	{
		public static Conversation.ID Ghost_ST_V1;

		public static Conversation.ID Ghost_ST_V2;

		public static Conversation.ID Ghost_ST_V3;

		public static Conversation.ID Ghost_ST_RIP1;

		public static Conversation.ID Ghost_ST_ROT1;

		public static Conversation.ID Ghost_ST_AU1;

		public static Conversation.ID Ghost_ST_AU2;

		public static Conversation.ID Ghost_ST_N1;

		public static Conversation.ID Ghost_ST_N2;

		public static Conversation.ID Ghost_ST_N3;

		public static Conversation.ID Ghost_ST_N4;

		public static Conversation.ID Ghost_ST_N5;

		public static Conversation.ID Ghost_ST_N6;

		public static Conversation.ID Ghost_ST_N7;

		public static Conversation.ID Prince_KS_1_1;

		public static Conversation.ID Prince_KS_1_2;

		public static Conversation.ID Prince_KS_1_3;

		public static Conversation.ID Prince_KS_1_4;

		public static Conversation.ID Prince_KS_2_1;

		public static Conversation.ID Prince_KS_2_2;

		public static Conversation.ID Prince_KS_2_3;

		public static Conversation.ID Prince_KS_2_4;

		public static Conversation.ID Prince_1;

		public static Conversation.ID Prince_2;

		public static Conversation.ID Prince_3;

		public static Conversation.ID Prince_4;

		public static Conversation.ID Prince_5;

		public static Conversation.ID Prince_6;

		public static Conversation.ID Prince_7;

		public static Conversation.ID Prince_FINAL;

		public static Conversation.ID Weaver_1;

		public static Conversation.ID Weaver_2;

		public static Conversation.ID Weaver_3;

		public static Conversation.ID Weaver_4;

		public static Conversation.ID Prince_Weaver_1;

		public static Conversation.ID Prince_Weaver_2;

		public static Conversation.ID Prince_Ascension;

		public static Conversation.ID Prince_Ascension_Marked;

		public static void RegisterValues()
		{
			Ghost_ST_V1 = new Conversation.ID("Ghost_ST_V1", register: true);
			Ghost_ST_V2 = new Conversation.ID("Ghost_ST_V2", register: true);
			Ghost_ST_V3 = new Conversation.ID("Ghost_ST_V3", register: true);
			Ghost_ST_RIP1 = new Conversation.ID("Ghost_ST_RIP1", register: true);
			Ghost_ST_ROT1 = new Conversation.ID("Ghost_ST_ROT1", register: true);
			Ghost_ST_AU1 = new Conversation.ID("Ghost_ST_AU1", register: true);
			Ghost_ST_AU2 = new Conversation.ID("Ghost_ST_AU2", register: true);
			Ghost_ST_N1 = new Conversation.ID("Ghost_ST_N1", register: true);
			Ghost_ST_N2 = new Conversation.ID("Ghost_ST_N2", register: true);
			Ghost_ST_N3 = new Conversation.ID("Ghost_ST_N3", register: true);
			Ghost_ST_N4 = new Conversation.ID("Ghost_ST_N4", register: true);
			Ghost_ST_N5 = new Conversation.ID("Ghost_ST_N5", register: true);
			Ghost_ST_N6 = new Conversation.ID("Ghost_ST_N6", register: true);
			Ghost_ST_N7 = new Conversation.ID("Ghost_ST_N7", register: true);
			Prince_KS_1_1 = new Conversation.ID("Prince_KS_1_1", register: true);
			Prince_KS_1_2 = new Conversation.ID("Prince_KS_1_2", register: true);
			Prince_KS_1_3 = new Conversation.ID("Prince_KS_1_3", register: true);
			Prince_KS_1_4 = new Conversation.ID("Prince_KS_1_4", register: true);
			Prince_KS_2_1 = new Conversation.ID("Prince_KS_2_1", register: true);
			Prince_KS_2_2 = new Conversation.ID("Prince_KS_2_2", register: true);
			Prince_KS_2_3 = new Conversation.ID("Prince_KS_2_3", register: true);
			Prince_KS_2_4 = new Conversation.ID("Prince_KS_2_4", register: true);
			Prince_1 = new Conversation.ID("Prince_1", register: true);
			Prince_2 = new Conversation.ID("Prince_2", register: true);
			Prince_3 = new Conversation.ID("Prince_3", register: true);
			Prince_4 = new Conversation.ID("Prince_4", register: true);
			Prince_5 = new Conversation.ID("Prince_5", register: true);
			Prince_6 = new Conversation.ID("Prince_6", register: true);
			Prince_7 = new Conversation.ID("Prince_7", register: true);
			Prince_FINAL = new Conversation.ID("Prince_FINAL", register: true);
			Weaver_1 = new Conversation.ID("Weaver_1", register: true);
			Weaver_2 = new Conversation.ID("Weaver_2", register: true);
			Weaver_3 = new Conversation.ID("Weaver_3", register: true);
			Weaver_4 = new Conversation.ID("Weaver_4", register: true);
			Prince_Weaver_1 = new Conversation.ID("Prince_Weaver_1", register: true);
			Prince_Weaver_2 = new Conversation.ID("Prince_Weaver_2", register: true);
			Prince_Ascension = new Conversation.ID("Prince_Ascension", register: true);
			Prince_Ascension_Marked = new Conversation.ID("Prince_Ascension_Marked", register: true);
		}

		public static void UnregisterValues()
		{
			Ghost_ST_V1?.Unregister();
			Ghost_ST_V1 = null;
			Ghost_ST_V2?.Unregister();
			Ghost_ST_V2 = null;
			Ghost_ST_V3?.Unregister();
			Ghost_ST_V3 = null;
			Ghost_ST_RIP1?.Unregister();
			Ghost_ST_RIP1 = null;
			Ghost_ST_ROT1?.Unregister();
			Ghost_ST_ROT1 = null;
			Ghost_ST_AU1?.Unregister();
			Ghost_ST_AU1 = null;
			Ghost_ST_AU2?.Unregister();
			Ghost_ST_AU2 = null;
			Ghost_ST_N1?.Unregister();
			Ghost_ST_N1 = null;
			Ghost_ST_N2?.Unregister();
			Ghost_ST_N2 = null;
			Ghost_ST_N3?.Unregister();
			Ghost_ST_N3 = null;
			Ghost_ST_N4?.Unregister();
			Ghost_ST_N4 = null;
			Ghost_ST_N5?.Unregister();
			Ghost_ST_N5 = null;
			Ghost_ST_N6?.Unregister();
			Ghost_ST_N6 = null;
			Ghost_ST_N7?.Unregister();
			Ghost_ST_N7 = null;
			Prince_KS_1_1?.Unregister();
			Prince_KS_1_1 = null;
			Prince_KS_1_2?.Unregister();
			Prince_KS_1_2 = null;
			Prince_KS_1_3?.Unregister();
			Prince_KS_1_3 = null;
			Prince_KS_1_4?.Unregister();
			Prince_KS_1_4 = null;
			Prince_KS_2_1?.Unregister();
			Prince_KS_2_1 = null;
			Prince_KS_2_2?.Unregister();
			Prince_KS_2_2 = null;
			Prince_KS_2_3?.Unregister();
			Prince_KS_2_3 = null;
			Prince_KS_2_4?.Unregister();
			Prince_KS_2_4 = null;
			Prince_1?.Unregister();
			Prince_1 = null;
			Prince_2?.Unregister();
			Prince_2 = null;
			Prince_3?.Unregister();
			Prince_3 = null;
			Prince_4?.Unregister();
			Prince_4 = null;
			Prince_5?.Unregister();
			Prince_5 = null;
			Prince_6?.Unregister();
			Prince_6 = null;
			Prince_7?.Unregister();
			Prince_7 = null;
			Weaver_1?.Unregister();
			Weaver_1 = null;
			Weaver_2?.Unregister();
			Weaver_2 = null;
			Weaver_3?.Unregister();
			Weaver_3 = null;
			Weaver_4?.Unregister();
			Weaver_4 = null;
			Prince_Weaver_1?.Unregister();
			Prince_Weaver_1 = null;
			Prince_Weaver_2?.Unregister();
			Prince_Weaver_2 = null;
			Prince_Ascension?.Unregister();
			Prince_Ascension = null;
			Prince_Ascension_Marked?.Unregister();
			Prince_Ascension_Marked = null;
		}
	}

	public class WatcherDangerType
	{
		public static RoomRain.DangerType Sandstorm;

		public static void RegisterValues()
		{
			Sandstorm = new RoomRain.DangerType("Sandstorm", register: true);
		}

		public static void UnregisterValues()
		{
			Sandstorm?.Unregister();
			Sandstorm = null;
		}
	}

	public class WatcherSmokeType
	{
		public static SmokeSystem.SmokeType BlizzardBeamSmoke;

		public static SmokeSystem.SmokeType BlizzardShieldSmoke;

		public static void RegisterValues()
		{
			BlizzardBeamSmoke = new SmokeSystem.SmokeType("BlizzardBeamSmoke", register: true);
			BlizzardShieldSmoke = new SmokeSystem.SmokeType("BlizzardShieldSmoke", register: true);
		}

		public static void UnregisterValues()
		{
			BlizzardBeamSmoke?.Unregister();
			BlizzardBeamSmoke = null;
			BlizzardShieldSmoke?.Unregister();
			BlizzardShieldSmoke = null;
		}
	}

	public class WatcherSoundID
	{
		public static SoundID Sandstorm_Wind;

		public static SoundID Sandstorm_Sand;

		public static SoundID Lightning_Strike_Instant;

		public static SoundID Natural_Thunder;

		public static SoundID Electric_Sizzle;

		public static SoundID Flame_Jet_Smoke_LOOP;

		public static SoundID Flame_Jet_Fire_LOOP;

		public static SoundID Warp_Point_Normal_Idle_LOOP;

		public static SoundID Warp_Point_Bad_Idle_LOOP;

		public static SoundID Warp_Point_Ripple_Idle_LOOP;

		public static SoundID Warp_Point_One_Way_Exit_Idle_LOOP;

		public static SoundID Warp_Point_Ripple_Hint;

		public static SoundID Warp_Point_Ripple_Hint_LOOP;

		public static SoundID Warp_Point_Normal_Spawn;

		public static SoundID Warp_Point_Bad_Spawn;

		public static SoundID Warp_Point_Normal_Suck_In_Objects_LOOP;

		public static SoundID Warp_Point_Bad_Suck_In_Objects_LOOP;

		public static SoundID Warp_Point_Ripple_Suck_In_Objects_LOOP;

		public static SoundID Warp_Point_Perform_Warp;

		public static SoundID Warp_Point_Spit_Out_Object;

		public static SoundID Warp_Point_Open;

		public static SoundID Warp_Point_Close;

		public static SoundID Warp_Point_Distortion_Wave_Hit_Player;

		public static SoundID Player_Generating_Warp_Point_LOOP;

		public static SoundID Player_Generating_Bad_Warp_Point_LOOP;

		public static SoundID Player_Generated_Success_Normal;

		public static SoundID Player_Generated_Success_Bad;

		public static SoundID Player_Activating_Camo_LOOP;

		public static SoundID Player_Deactivating_Camo_LOOP;

		public static SoundID Player_Activated_Camo;

		public static SoundID Player_Deactivated_Camo;

		public static SoundID Player_Deactivated_Camo_From_Stun;

		public static SoundID Player_Enter_Ripple_Dimension;

		public static SoundID Player_Exit_Ripple_Dimension;

		public static SoundID Player_Exit_Ripple_Dimension_From_Stun;

		public static SoundID Player_Fail_To_Switch_Ripple_Layer;

		public static SoundID Player_Spawn_Ripple_Tear_Small;

		public static SoundID Player_Spawn_Ripple_Tear_Large;

		public static SoundID Prince_Talk;

		public static SoundID Prince_Arm_Move_LOOP;

		public static SoundID Prince_Arm_Joint;

		public static SoundID Void_Weaver_Ability_Spawn_Thread;

		public static SoundID Void_Weaver_Ability_Close_Portal;

		public static SoundID Void_Weaver_Vanish;

		public static SoundID Void_Weaver_Voice_1;

		public static SoundID Void_Weaver_Voice_2;

		public static SoundID Void_Weaver_Voice_3;

		public static SoundID Void_Weaver_Voice_4;

		public static SoundID Void_Weaver_Aura;

		public static SoundID Void_Weaver_Rustle;

		public static SoundID Void_Weaver_Jangle;

		public static SoundID Void_Weaver_Pluck_Thread;

		public static SoundID Void_Weaver_Halo_LOOP;

		public static SoundID Void_Weaver_Flare;

		public static SoundID Spinning_Top_Vanish;

		public static SoundID Spinning_Top_Laugh_S;

		public static SoundID Spinning_Top_Laugh_L;

		public static SoundID Spinning_Top_Voice;

		public static SoundID Rot_Worm_Aura_LOOP;

		public static SoundID Rot_Worm_Woosh_LOOP;

		public static SoundID Ripple_Creature_Swap_Dimensions;

		public static SoundID Ripple_Creature_Death_Effect_LOOP;

		public static SoundID Elder_Ripple_Aura;

		public static SoundID Rattler_Shake_LOOP;

		public static SoundID Rattler_Tentacle_Grab;

		public static SoundID Rattler_Squeal;

		public static SoundID Sky_Whale_Fin_Flap;

		public static SoundID Sky_Whale_Cry;

		public static SoundID Sky_Whale_Click;

		public static SoundID Sky_Whale_Groan;

		public static SoundID Big_Sky_Whale_Flyby;

		public static SoundID Big_Sky_Whale_Call;

		public static SoundID Barnacle_Step;

		public static SoundID Barnacle_Shell_Clink;

		public static SoundID Barnacle_Assemble_Call;

		public static SoundID Barnacle_Travel_Call;

		public static SoundID Barnacle_Spit_Water;

		public static SoundID Barnacle_Suck_Water;

		public static SoundID Barnacle_Push_Away_Nearby_Creatures;

		public static SoundID Barnacle_Shell_Explode;

		public static SoundID Barnacle_Shell_Crack;

		public static SoundID Barnacle_Shell_Fragment_Hit_Terrain;

		public static SoundID Barnacle_Shell_Deflect_Projectile;

		public static SoundID Barnacle_Legs_Skitter;

		public static SoundID Drill_Crab_Drill_LOOP;

		public static SoundID Drill_Crab_Drill_Impact_LOOP;

		public static SoundID Drill_Crab_Step;

		public static SoundID Drill_Crab_Squeal;

		public static SoundID Drill_Crab_Enter_Shell;

		public static SoundID Drill_Crab_Exit_Shell;

		public static SoundID Drill_Crab_Start_Up;

		public static SoundID Drill_Crab_Turn_Off;

		public static SoundID Drill_Crab_Collide_Terrain;

		public static SoundID Drill_Crab_Collide_Creature;

		public static SoundID Drill_Crab_CrabChatter_LOOP;

		public static SoundID Drill_Crab_CrabChatterLow_LOOP;

		public static SoundID Tower_Crab_Step;

		public static SoundID Sand_Grub_Bite;

		public static SoundID Sand_Grub_Peek;

		public static SoundID Sand_Grub_Pull_From_Burrow;

		public static SoundID Sand_Grub_Squeal;

		public static SoundID Big_Sand_Grub_Bite;

		public static SoundID Big_Sand_Grub_Unfurl;

		public static SoundID Big_Sand_Grub_Slide_LOOP;

		public static SoundID Big_Sand_Grub_Squeal;

		public static SoundID Big_Sand_Grub_Rattle_LOOP;

		public static SoundID Big_Sand_Grub_Threaten;

		public static SoundID Locust_Cloud_Near_LOOP;

		public static SoundID Locust_Cloud_Far_LOOP;

		public static SoundID Locust_Call_LOOP;

		public static SoundID Locust_Swarm_LOOP;

		public static SoundID Locust_Land;

		public static SoundID Locust_Poisoned;

		public static SoundID Locust_Attach;

		public static SoundID Box_Worm_Rip_Out_Larva;

		public static SoundID Box_Worm_Pulling_Larva_LOOP;

		public static SoundID Box_Worm_Spitting_Steam_LOOP;

		public static SoundID Box_Worm_Idle_Steam;

		public static SoundID Box_Worm_Fire_Sprite_Enter;

		public static SoundID Box_Worm_Activate_Spin;

		public static SoundID Box_Worm_Activate_Ping;

		public static SoundID Box_Worm_Spin_Head_LOOP;

		public static SoundID Box_Worm_Crawl_Retract;

		public static SoundID Box_Worm_Crawl_Extend;

		public static SoundID Box_Worm_Hit;

		public static SoundID Slugcat_Bite_Box_Worm_Larva;

		public static SoundID Slugcat_Eat_Box_Worm_Larva;

		public static SoundID Fire_Sprite_Wing_Spread_Idle_LOOP;

		public static SoundID Fire_Sprite_Flying_Boost;

		public static SoundID Fire_Sprite_Flying_LOOP;

		public static SoundID Fire_Sprite_Chirp;

		public static SoundID Fire_Sprite_Hit;

		public static SoundID Templar_Shield_Deflect;

		public static SoundID Templar_Shield_Tick_1;

		public static SoundID Templar_Shield_Tick_2;

		public static SoundID Templar_Shield_Tick_3;

		public static SoundID Templar_Shield_Tick_4;

		public static SoundID Templar_Shield_Tick_5;

		public static SoundID Templar_Shield_Tick_6;

		public static SoundID Templar_Shield_Tick_7;

		public static SoundID Templar_Shield_Tick_8;

		public static SoundID Templar_Shield_Tick_9;

		public static SoundID Templar_Shield_Explode;

		public static SoundID Boomerang_Flying_LOOP;

		public static SoundID Boomerang_Collide_Terrain;

		public static SoundID Boomerang_Collide_Creature;

		public static SoundID Boomerang_Thrown;

		public static SoundID BlizLiz_Beam_LOOP;

		public static SoundID BlizLiz_BeamHit_LOOP;

		public static SoundID BlizLiz_Charge;

		public static SoundID BlizLiz_Pop;

		public static SoundID RotLiz_Erupt_Tentacles;

		public static SoundID RotLiz_Pain;

		public static SoundID RotLiz_Vocalize;

		public static SoundID RotLiz_Hiss;

		public static SoundID Lizard_Voice_Rot_A;

		public static SoundID Lizard_Voice_Rot_B;

		public static SoundID RotLiz_Injured;

		public static SoundID Moth_Step;

		public static SoundID Moth_Grab;

		public static SoundID Moth_Chatter;

		public static SoundID Moth_Chatter_Loud;

		public static SoundID Moth_Clicking;

		public static SoundID Moth_Wingflaps;

		public static SoundID Rat_Scramble;

		public static SoundID Rat_Chatter;

		public static SoundID Rat_Shriek;

		public static SoundID Loach_Step;

		public static SoundID Loach_Tentacle_Grab;

		public static SoundID Loach_Tentacle_Release;

		public static SoundID Loach_Passive_Groan;

		public static SoundID Loach_Aggressive_Groan;

		public static SoundID Loach_Curious;

		public static SoundID Loach_Lunge;

		public static SoundID Loach_Eat;

		public static SoundID Frog_Passive_Croak;

		public static SoundID Frog_Aggressive_Croak;

		public static SoundID Frog_Jump;

		public static SoundID Frog_Attach;

		public static SoundID Frog_Drain;

		public static SoundID Frog_Pain;

		public static SoundID Angler_Pulse;

		public static SoundID Angler_Swim;

		public static SoundID Angler_Shriek;

		public static SoundID Angler_Bite;

		public static SoundID SoftToy_Squeak;

		public static SoundID SoftToy_Hit;

		public static SoundID SpinToy_Impact;

		public static SoundID SpinToy_Spin;

		public static SoundID BallToy_Roll_LOOP;

		public static SoundID BallToy_BounceS;

		public static SoundID BallToy_BounceM;

		public static SoundID BallToy_BounceH;

		public static SoundID ARZapper_Random_Spark;

		public static SoundID ARZapper_Powered_LOOP;

		public static SoundID ARZapper_Hit;

		public static SoundID ARZapper_Disintegrate;

		public static SoundID Water_Machinery_Deep;

		public static SoundID Water_Machinery_Hit;

		public static SoundID Skink_Dig_LOOP;

		public static SoundID Skink_Dig_Deep_LOOP;

		public static SoundID Skink_Scrape;

		public static SoundID HeartbeatFast_LOOP;

		public static SoundID HeartbeatHard_LOOP;

		public static SoundID HeartbeatMed_LOOP;

		public static SoundID Ripplesick_LOOP;

		public static SoundID RipplesickPop;

		public static SoundID RipplesickSpark;

		public static SoundID Pearl_ATalkShow;

		public static SoundID Pearl_AGrumbGroove;

		public static SoundID Pearl_AJam1;

		public static SoundID Pearl_AJam2;

		public static SoundID Pearl_AJam3;

		public static SoundID Pearl_AJam4;

		public static SoundID Pearl_AVoiceWind1;

		public static SoundID Pearl_AVoiceWind2;

		public static SoundID PearlReaderPearl;

		public static SoundID PearlReaderClick;

		public static SoundID PearlReaderArm_LOOP;

		public static SoundID PearlReaderText_LOOP;

		public static SoundID PearlReaderNoise;

		public static SoundID PearlReaderNoise_LOOP;

		public static SoundID DaemonHintST;

		public static SoundID DaemonHintPrince;

		public static SoundID DaemonHintWeaver;

		public static void RegisterValues()
		{
			Sandstorm_Wind = new SoundID("Sandstorm_Wind", register: true);
			Sandstorm_Sand = new SoundID("Sandstorm_Sand", register: true);
			Lightning_Strike_Instant = new SoundID("Lightning_Strike_Instant", register: true);
			Natural_Thunder = new SoundID("Natural_Thunder", register: true);
			Electric_Sizzle = new SoundID("Electric_Sizzle", register: true);
			Flame_Jet_Smoke_LOOP = new SoundID("Flame_Jet_Smoke_LOOP", register: true);
			Flame_Jet_Fire_LOOP = new SoundID("Flame_Jet_Fire_LOOP", register: true);
			Warp_Point_Normal_Idle_LOOP = new SoundID("Warp_Point_Normal_Idle_LOOP", register: true);
			Warp_Point_Bad_Idle_LOOP = new SoundID("Warp_Point_Bad_Idle_LOOP", register: true);
			Warp_Point_Ripple_Idle_LOOP = new SoundID("Warp_Point_Ripple_Idle_LOOP", register: true);
			Warp_Point_One_Way_Exit_Idle_LOOP = new SoundID("Warp_Point_One_Way_Exit_Idle_LOOP", register: true);
			Warp_Point_Ripple_Hint = new SoundID("Warp_Point_Ripple_Hint", register: true);
			Warp_Point_Ripple_Hint_LOOP = new SoundID("Warp_Point_Ripple_Hint_LOOP", register: true);
			Warp_Point_Normal_Spawn = new SoundID("Warp_Point_Normal_Spawn", register: true);
			Warp_Point_Bad_Spawn = new SoundID("Warp_Point_Bad_Spawn", register: true);
			Warp_Point_Normal_Suck_In_Objects_LOOP = new SoundID("Warp_Point_Normal_Suck_In_Objects_LOOP", register: true);
			Warp_Point_Bad_Suck_In_Objects_LOOP = new SoundID("Warp_Point_Bad_Suck_In_Objects_LOOP", register: true);
			Warp_Point_Ripple_Suck_In_Objects_LOOP = new SoundID("Warp_Point_Ripple_Suck_In_Objects_LOOP", register: true);
			Warp_Point_Perform_Warp = new SoundID("Warp_Point_Perform_Warp", register: true);
			Warp_Point_Spit_Out_Object = new SoundID("Warp_Point_Spit_Out_Object", register: true);
			Warp_Point_Open = new SoundID("Warp_Point_Open", register: true);
			Warp_Point_Close = new SoundID("Warp_Point_Close", register: true);
			Warp_Point_Distortion_Wave_Hit_Player = new SoundID("Warp_Point_Distortion_Wave_Hit_Player", register: true);
			Player_Generating_Warp_Point_LOOP = new SoundID("Player_Generating_Warp_Point_LOOP", register: true);
			Player_Generating_Bad_Warp_Point_LOOP = new SoundID("Player_Generating_Bad_Warp_Point_LOOP", register: true);
			Player_Generated_Success_Normal = new SoundID("Player_Generated_Success_Normal", register: true);
			Player_Generated_Success_Bad = new SoundID("Player_Generated_Success_Bad", register: true);
			Player_Activating_Camo_LOOP = new SoundID("Player_Activating_Camo_LOOP", register: true);
			Player_Deactivating_Camo_LOOP = new SoundID("Player_Deactivating_Camo_LOOP", register: true);
			Player_Activated_Camo = new SoundID("Player_Activated_Camo", register: true);
			Player_Deactivated_Camo = new SoundID("Player_Deactivated_Camo", register: true);
			Player_Deactivated_Camo_From_Stun = new SoundID("Player_Deactivated_Camo_From_Stun", register: true);
			Player_Enter_Ripple_Dimension = new SoundID("Player_Enter_Ripple_Dimension", register: true);
			Player_Exit_Ripple_Dimension = new SoundID("Player_Exit_Ripple_Dimension", register: true);
			Player_Exit_Ripple_Dimension_From_Stun = new SoundID("Player_Exit_Ripple_Dimension_From_Stun", register: true);
			Player_Fail_To_Switch_Ripple_Layer = new SoundID("Player_Fail_To_Switch_Ripple_Layer", register: true);
			Player_Spawn_Ripple_Tear_Small = new SoundID("Player_Spawn_Ripple_Tear_Small", register: true);
			Player_Spawn_Ripple_Tear_Large = new SoundID("Player_Spawn_Ripple_Tear_Large", register: true);
			Prince_Talk = new SoundID("Prince_Talk", register: true);
			Prince_Arm_Move_LOOP = new SoundID("Prince_Arm_Move_LOOP", register: true);
			Prince_Arm_Joint = new SoundID("Prince_Arm_Joint", register: true);
			Void_Weaver_Ability_Spawn_Thread = new SoundID("Void_Weaver_Ability_Spawn_Thread", register: true);
			Void_Weaver_Ability_Close_Portal = new SoundID("Void_Weaver_Ability_Close_Portal", register: true);
			Void_Weaver_Vanish = new SoundID("Void_Weaver_Vanish", register: true);
			Void_Weaver_Voice_1 = new SoundID("Void_Weaver_Voice_1", register: true);
			Void_Weaver_Voice_2 = new SoundID("Void_Weaver_Voice_2", register: true);
			Void_Weaver_Voice_3 = new SoundID("Void_Weaver_Voice_3", register: true);
			Void_Weaver_Voice_4 = new SoundID("Void_Weaver_Voice_4", register: true);
			Void_Weaver_Aura = new SoundID("Void_Weaver_Aura", register: true);
			Void_Weaver_Rustle = new SoundID("Void_Weaver_Rustle", register: true);
			Void_Weaver_Jangle = new SoundID("Void_Weaver_Jangle", register: true);
			Void_Weaver_Pluck_Thread = new SoundID("Void_Weaver_Pluck_Thread", register: true);
			Void_Weaver_Halo_LOOP = new SoundID("Void_Weaver_Halo_LOOP", register: true);
			Void_Weaver_Flare = new SoundID("Void_Weaver_Flare", register: true);
			Spinning_Top_Vanish = new SoundID("Spinning_Top_Vanish", register: true);
			Spinning_Top_Laugh_S = new SoundID("Spinning_Top_Laugh_S", register: true);
			Spinning_Top_Laugh_L = new SoundID("Spinning_Top_Laugh_L", register: true);
			Spinning_Top_Voice = new SoundID("Spinning_Top_Voice", register: true);
			Rot_Worm_Aura_LOOP = new SoundID("Rot_Worm_Aura_LOOP", register: true);
			Rot_Worm_Woosh_LOOP = new SoundID("Rot_Worm_Woosh_LOOP", register: true);
			Ripple_Creature_Swap_Dimensions = new SoundID("Ripple_Creature_Swap_Dimensions", register: true);
			Ripple_Creature_Death_Effect_LOOP = new SoundID("Ripple_Creature_Death_Effect_LOOP", register: true);
			Elder_Ripple_Aura = new SoundID("Elder_Ripple_Aura", register: true);
			Rattler_Shake_LOOP = new SoundID("Rattler_Shake_LOOP", register: true);
			Rattler_Tentacle_Grab = new SoundID("Rattler_Tentacle_Grab", register: true);
			Rattler_Squeal = new SoundID("Rattler_Squeal", register: true);
			Sky_Whale_Fin_Flap = new SoundID("Sky_Whale_Fin_Flap", register: true);
			Sky_Whale_Cry = new SoundID("Sky_Whale_Cry", register: true);
			Sky_Whale_Click = new SoundID("Sky_Whale_Click", register: true);
			Sky_Whale_Groan = new SoundID("Sky_Whale_Groan", register: true);
			Big_Sky_Whale_Flyby = new SoundID("Big_Sky_Whale_Flyby", register: true);
			Big_Sky_Whale_Call = new SoundID("Big_Sky_Whale_Call", register: true);
			Barnacle_Step = new SoundID("Barnacle_Step", register: true);
			Barnacle_Shell_Clink = new SoundID("Barnacle_Shell_Clink", register: true);
			Barnacle_Assemble_Call = new SoundID("Barnacle_Assemble_Call", register: true);
			Barnacle_Travel_Call = new SoundID("Barnacle_Travel_Call", register: true);
			Barnacle_Spit_Water = new SoundID("Barnacle_Spit_Water", register: true);
			Barnacle_Suck_Water = new SoundID("Barnacle_Suck_Water", register: true);
			Barnacle_Push_Away_Nearby_Creatures = new SoundID("Barnacle_Push_Away_Nearby_Creatures", register: true);
			Barnacle_Shell_Explode = new SoundID("Barnacle_Shell_Explode", register: true);
			Barnacle_Shell_Crack = new SoundID("Barnacle_Shell_Crack", register: true);
			Barnacle_Shell_Fragment_Hit_Terrain = new SoundID("Barnacle_Shell_Fragment_Hit_Terrain", register: true);
			Barnacle_Shell_Deflect_Projectile = new SoundID("Barnacle_Shell_Deflect_Projectile", register: true);
			Barnacle_Legs_Skitter = new SoundID("Barnacle_Legs_Skitter", register: true);
			Drill_Crab_Drill_LOOP = new SoundID("Drill_Crab_Drill_LOOP", register: true);
			Drill_Crab_Drill_Impact_LOOP = new SoundID("Drill_Crab_Drill_Impact_LOOP", register: true);
			Drill_Crab_Step = new SoundID("Drill_Crab_Step", register: true);
			Drill_Crab_Squeal = new SoundID("Drill_Crab_Squeal", register: true);
			Drill_Crab_Enter_Shell = new SoundID("Drill_Crab_Enter_Shell", register: true);
			Drill_Crab_Exit_Shell = new SoundID("Drill_Crab_Exit_Shell", register: true);
			Drill_Crab_Start_Up = new SoundID("Drill_Crab_Start_Up", register: true);
			Drill_Crab_Turn_Off = new SoundID("Drill_Crab_Turn_Off", register: true);
			Drill_Crab_Collide_Terrain = new SoundID("Drill_Crab_Collide_Terrain", register: true);
			Drill_Crab_Collide_Creature = new SoundID("Drill_Crab_Collide_Creature", register: true);
			Drill_Crab_CrabChatter_LOOP = new SoundID("Drill_Crab_CrabChatter_LOOP", register: true);
			Drill_Crab_CrabChatterLow_LOOP = new SoundID("Drill_Crab_CrabChatterLow_LOOP", register: true);
			Tower_Crab_Step = new SoundID("Tower_Crab_Step", register: true);
			Sand_Grub_Bite = new SoundID("Sand_Grub_Bite", register: true);
			Sand_Grub_Peek = new SoundID("Sand_Grub_Peek", register: true);
			Sand_Grub_Pull_From_Burrow = new SoundID("Sand_Grub_Pull_From_Burrow", register: true);
			Sand_Grub_Squeal = new SoundID("Sand_Grub_Squeal", register: true);
			Big_Sand_Grub_Bite = new SoundID("Big_Sand_Grub_Bite", register: true);
			Big_Sand_Grub_Unfurl = new SoundID("Big_Sand_Grub_Unfurl", register: true);
			Big_Sand_Grub_Slide_LOOP = new SoundID("Big_Sand_Grub_Slide_LOOP", register: true);
			Big_Sand_Grub_Squeal = new SoundID("Big_Sand_Grub_Squeal", register: true);
			Big_Sand_Grub_Rattle_LOOP = new SoundID("Big_Sand_Grub_Rattle_LOOP", register: true);
			Big_Sand_Grub_Threaten = new SoundID("Big_Sand_Grub_Threaten", register: true);
			Locust_Cloud_Near_LOOP = new SoundID("Locust_Cloud_Near_LOOP", register: true);
			Locust_Cloud_Far_LOOP = new SoundID("Locust_Cloud_Far_LOOP", register: true);
			Locust_Swarm_LOOP = new SoundID("Locust_Swarm_LOOP", register: true);
			Locust_Call_LOOP = new SoundID("Locust_Call_LOOP", register: true);
			Locust_Land = new SoundID("Locust_Land", register: true);
			Locust_Poisoned = new SoundID("Locust_Poisoned", register: true);
			Locust_Attach = new SoundID("Locust_Attach", register: true);
			Box_Worm_Rip_Out_Larva = new SoundID("Box_Worm_Rip_Out_Larva", register: true);
			Box_Worm_Pulling_Larva_LOOP = new SoundID("Box_Worm_Pulling_Larva_LOOP", register: true);
			Box_Worm_Spitting_Steam_LOOP = new SoundID("Box_Worm_Spitting_Steam_LOOP", register: true);
			Box_Worm_Idle_Steam = new SoundID("Box_Worm_Idle_Steam", register: true);
			Box_Worm_Fire_Sprite_Enter = new SoundID("Box_Worm_Fire_Sprite_Enter", register: true);
			Box_Worm_Activate_Spin = new SoundID("Box_Worm_Activate_Spin", register: true);
			Box_Worm_Activate_Ping = new SoundID("Box_Worm_Activate_Ping", register: true);
			Box_Worm_Spin_Head_LOOP = new SoundID("Box_Worm_Spin_Head_LOOP", register: true);
			Box_Worm_Crawl_Retract = new SoundID("Box_Worm_Crawl_Retract", register: true);
			Box_Worm_Crawl_Extend = new SoundID("Box_Worm_Crawl_Extend", register: true);
			Box_Worm_Hit = new SoundID("Box_Worm_Hit", register: true);
			Slugcat_Bite_Box_Worm_Larva = new SoundID("Slugcat_Bite_Box_Worm_Larva", register: true);
			Slugcat_Eat_Box_Worm_Larva = new SoundID("Slugcat_Eat_Box_Worm_Larva", register: true);
			Fire_Sprite_Wing_Spread_Idle_LOOP = new SoundID("Fire_Sprite_Wing_Spread_Idle_LOOP", register: true);
			Fire_Sprite_Flying_Boost = new SoundID("Fire_Sprite_Flying_Boost", register: true);
			Fire_Sprite_Flying_LOOP = new SoundID("Fire_Sprite_Flying_LOOP", register: true);
			Fire_Sprite_Chirp = new SoundID("Fire_Sprite_Chirp", register: true);
			Fire_Sprite_Hit = new SoundID("Fire_Sprite_Hit", register: true);
			Templar_Shield_Deflect = new SoundID("Templar_Shield_Deflect", register: true);
			Templar_Shield_Tick_1 = new SoundID("Templar_Shield_Tick_1", register: true);
			Templar_Shield_Tick_2 = new SoundID("Templar_Shield_Tick_2", register: true);
			Templar_Shield_Tick_3 = new SoundID("Templar_Shield_Tick_3", register: true);
			Templar_Shield_Tick_4 = new SoundID("Templar_Shield_Tick_4", register: true);
			Templar_Shield_Tick_5 = new SoundID("Templar_Shield_Tick_5", register: true);
			Templar_Shield_Tick_6 = new SoundID("Templar_Shield_Tick_6", register: true);
			Templar_Shield_Tick_7 = new SoundID("Templar_Shield_Tick_7", register: true);
			Templar_Shield_Tick_8 = new SoundID("Templar_Shield_Tick_8", register: true);
			Templar_Shield_Tick_9 = new SoundID("Templar_Shield_Tick_9", register: true);
			Templar_Shield_Explode = new SoundID("Templar_Shield_Explode", register: true);
			Boomerang_Flying_LOOP = new SoundID("Boomerang_Flying_LOOP", register: true);
			Boomerang_Collide_Terrain = new SoundID("Boomerang_Collide_Terrain", register: true);
			Boomerang_Collide_Creature = new SoundID("Boomerang_Collide_Creature", register: true);
			Boomerang_Thrown = new SoundID("Boomerang_Thrown", register: true);
			RotLiz_Erupt_Tentacles = new SoundID("RotLiz_Erupt_Tentacles", register: true);
			RotLiz_Pain = new SoundID("RotLiz_Pain", register: true);
			RotLiz_Vocalize = new SoundID("RotLiz_Vocalize", register: true);
			RotLiz_Hiss = new SoundID("RotLiz_Hiss", register: true);
			Lizard_Voice_Rot_A = new SoundID("Lizard_Voice_Rot_A", register: true);
			Lizard_Voice_Rot_B = new SoundID("Lizard_Voice_Rot_B", register: true);
			RotLiz_Injured = new SoundID("RotLiz_Injured", register: true);
			BlizLiz_Beam_LOOP = new SoundID("BlizLiz_Beam_LOOP", register: true);
			BlizLiz_BeamHit_LOOP = new SoundID("BlizLiz_BeamHit_LOOP", register: true);
			BlizLiz_Charge = new SoundID("BlizLiz_Charge", register: true);
			BlizLiz_Pop = new SoundID("BlizLiz_Pop", register: true);
			Moth_Step = new SoundID("Moth_Step", register: true);
			Moth_Grab = new SoundID("Moth_Grab", register: true);
			Moth_Chatter = new SoundID("Moth_Chatter", register: true);
			Moth_Chatter_Loud = new SoundID("Moth_Chatter_Loud", register: true);
			Moth_Clicking = new SoundID("Moth_Clicking", register: true);
			Moth_Wingflaps = new SoundID("Moth_Wingflaps", register: true);
			Rat_Scramble = new SoundID("Rat_Scramble", register: true);
			Rat_Chatter = new SoundID("Rat_Chatter", register: true);
			Rat_Shriek = new SoundID("Rat_Shriek", register: true);
			Loach_Step = new SoundID("Loach_Step", register: true);
			Loach_Tentacle_Grab = new SoundID("Loach_Tentacle_Grab", register: true);
			Loach_Tentacle_Release = new SoundID("Loach_Tentacle_Release", register: true);
			Loach_Passive_Groan = new SoundID("Loach_Passive_Groan", register: true);
			Loach_Aggressive_Groan = new SoundID("Loach_Aggressive_Groan", register: true);
			Loach_Curious = new SoundID("Loach_Curious", register: true);
			Loach_Lunge = new SoundID("Loach_Lunge", register: true);
			Loach_Eat = new SoundID("Loach_Eat", register: true);
			Frog_Passive_Croak = new SoundID("Frog_Passive_Croak", register: true);
			Frog_Aggressive_Croak = new SoundID("Frog_Aggressive_Croak", register: true);
			Frog_Jump = new SoundID("Frog_Jump", register: true);
			Frog_Attach = new SoundID("Frog_Attach", register: true);
			Frog_Drain = new SoundID("Frog_Drain", register: true);
			Frog_Pain = new SoundID("Frog_Pain", register: true);
			Angler_Pulse = new SoundID("Angler_Pulse", register: true);
			Angler_Swim = new SoundID("Angler_Swim", register: true);
			Angler_Shriek = new SoundID("Angler_Shriek", register: true);
			Angler_Bite = new SoundID("Angler_Bite", register: true);
			SoftToy_Squeak = new SoundID("SoftToy_Squeak", register: true);
			SoftToy_Hit = new SoundID("SoftToy_Hit", register: true);
			SpinToy_Impact = new SoundID("SpinToy_Impact", register: true);
			SpinToy_Spin = new SoundID("SpinToy_Spin", register: true);
			BallToy_Roll_LOOP = new SoundID("BallToy_Roll_LOOP", register: true);
			BallToy_BounceS = new SoundID("BallToy_BounceS", register: true);
			BallToy_BounceM = new SoundID("BallToy_BounceM", register: true);
			BallToy_BounceH = new SoundID("BallToy_BounceH", register: true);
			ARZapper_Random_Spark = new SoundID("ARZapper_Random_Spark", register: true);
			ARZapper_Powered_LOOP = new SoundID("ARZapper_Powered_LOOP", register: true);
			ARZapper_Hit = new SoundID("ARZapper_Hit", register: true);
			ARZapper_Disintegrate = new SoundID("ARZapper_Disintegrate", register: true);
			Water_Machinery_Deep = new SoundID("Water_Machinery_Deep", register: true);
			Water_Machinery_Hit = new SoundID("Water_Machinery_Hit", register: true);
			Skink_Dig_LOOP = new SoundID("Skink_Dig_LOOP", register: true);
			Skink_Dig_Deep_LOOP = new SoundID("Skink_Dig_Deep_LOOP", register: true);
			Skink_Scrape = new SoundID("Skink_Scrape", register: true);
			HeartbeatFast_LOOP = new SoundID("HeartbeatFast_LOOP", register: true);
			HeartbeatHard_LOOP = new SoundID("HeartbeatHard_LOOP", register: true);
			HeartbeatMed_LOOP = new SoundID("HeartbeatMed_LOOP", register: true);
			Ripplesick_LOOP = new SoundID("Ripplesick_LOOP", register: true);
			RipplesickPop = new SoundID("RipplesickPop", register: true);
			RipplesickSpark = new SoundID("RipplesickSpark", register: true);
			Pearl_ATalkShow = new SoundID("Pearl_ATalkShow", register: true);
			Pearl_AGrumbGroove = new SoundID("Pearl_AGrumbGroove", register: true);
			Pearl_AJam1 = new SoundID("Pearl_AJam1", register: true);
			Pearl_AJam2 = new SoundID("Pearl_AJam2", register: true);
			Pearl_AJam3 = new SoundID("Pearl_AJam3", register: true);
			Pearl_AJam4 = new SoundID("Pearl_AJam4", register: true);
			Pearl_AVoiceWind1 = new SoundID("Pearl_AVoiceWind1", register: true);
			Pearl_AVoiceWind2 = new SoundID("Pearl_AVoiceWind2", register: true);
			PearlReaderPearl = new SoundID("PearlReaderPearl", register: true);
			PearlReaderClick = new SoundID("PearlReaderClick", register: true);
			PearlReaderArm_LOOP = new SoundID("PearlReaderArm_LOOP", register: true);
			PearlReaderText_LOOP = new SoundID("PearlReaderText_LOOP", register: true);
			PearlReaderNoise = new SoundID("PearlReaderNoise", register: true);
			PearlReaderNoise_LOOP = new SoundID("PearlReaderNoise_LOOP", register: true);
			DaemonHintST = new SoundID("DaemonHintST", register: true);
			DaemonHintPrince = new SoundID("DaemonHintPrince", register: true);
			DaemonHintWeaver = new SoundID("DaemonHintWeaver", register: true);
		}

		public static void UnregisterValues()
		{
			Sandstorm_Wind?.Unregister();
			Sandstorm_Wind = null;
			Sandstorm_Sand?.Unregister();
			Sandstorm_Sand = null;
			Lightning_Strike_Instant?.Unregister();
			Lightning_Strike_Instant = null;
			Natural_Thunder?.Unregister();
			Natural_Thunder = null;
			Electric_Sizzle?.Unregister();
			Electric_Sizzle = null;
			Flame_Jet_Smoke_LOOP?.Unregister();
			Flame_Jet_Smoke_LOOP = null;
			Flame_Jet_Fire_LOOP?.Unregister();
			Flame_Jet_Fire_LOOP = null;
			Warp_Point_Normal_Idle_LOOP?.Unregister();
			Warp_Point_Normal_Idle_LOOP = null;
			Warp_Point_Bad_Idle_LOOP?.Unregister();
			Warp_Point_Bad_Idle_LOOP = null;
			Warp_Point_Ripple_Idle_LOOP?.Unregister();
			Warp_Point_Ripple_Idle_LOOP = null;
			Warp_Point_One_Way_Exit_Idle_LOOP?.Unregister();
			Warp_Point_One_Way_Exit_Idle_LOOP = null;
			Warp_Point_Ripple_Hint?.Unregister();
			Warp_Point_Ripple_Hint = null;
			Warp_Point_Ripple_Hint_LOOP?.Unregister();
			Warp_Point_Ripple_Hint_LOOP = null;
			Warp_Point_Normal_Spawn?.Unregister();
			Warp_Point_Normal_Spawn = null;
			Warp_Point_Bad_Spawn?.Unregister();
			Warp_Point_Bad_Spawn = null;
			Warp_Point_Normal_Suck_In_Objects_LOOP?.Unregister();
			Warp_Point_Normal_Suck_In_Objects_LOOP = null;
			Warp_Point_Bad_Suck_In_Objects_LOOP?.Unregister();
			Warp_Point_Bad_Suck_In_Objects_LOOP = null;
			Warp_Point_Ripple_Suck_In_Objects_LOOP?.Unregister();
			Warp_Point_Ripple_Suck_In_Objects_LOOP = null;
			Warp_Point_Perform_Warp?.Unregister();
			Warp_Point_Perform_Warp = null;
			Warp_Point_Spit_Out_Object?.Unregister();
			Warp_Point_Spit_Out_Object = null;
			Warp_Point_Open?.Unregister();
			Warp_Point_Open = null;
			Warp_Point_Close?.Unregister();
			Warp_Point_Close = null;
			Warp_Point_Distortion_Wave_Hit_Player?.Unregister();
			Warp_Point_Distortion_Wave_Hit_Player = null;
			Player_Generating_Warp_Point_LOOP?.Unregister();
			Player_Generating_Warp_Point_LOOP = null;
			Player_Generating_Bad_Warp_Point_LOOP?.Unregister();
			Player_Generating_Bad_Warp_Point_LOOP = null;
			Player_Generated_Success_Normal?.Unregister();
			Player_Generated_Success_Normal = null;
			Player_Generated_Success_Bad?.Unregister();
			Player_Generated_Success_Bad = null;
			Player_Activating_Camo_LOOP?.Unregister();
			Player_Activating_Camo_LOOP = null;
			Player_Deactivating_Camo_LOOP?.Unregister();
			Player_Deactivating_Camo_LOOP = null;
			Player_Activated_Camo?.Unregister();
			Player_Activated_Camo = null;
			Player_Deactivated_Camo?.Unregister();
			Player_Deactivated_Camo = null;
			Player_Deactivated_Camo_From_Stun?.Unregister();
			Player_Deactivated_Camo_From_Stun = null;
			Player_Enter_Ripple_Dimension?.Unregister();
			Player_Enter_Ripple_Dimension = null;
			Player_Exit_Ripple_Dimension?.Unregister();
			Player_Exit_Ripple_Dimension = null;
			Player_Exit_Ripple_Dimension_From_Stun?.Unregister();
			Player_Exit_Ripple_Dimension_From_Stun = null;
			Player_Fail_To_Switch_Ripple_Layer?.Unregister();
			Player_Fail_To_Switch_Ripple_Layer = null;
			Player_Spawn_Ripple_Tear_Small?.Unregister();
			Player_Spawn_Ripple_Tear_Small = null;
			Player_Spawn_Ripple_Tear_Large?.Unregister();
			Player_Spawn_Ripple_Tear_Large = null;
			Prince_Talk?.Unregister();
			Prince_Talk = null;
			Prince_Arm_Move_LOOP?.Unregister();
			Prince_Arm_Move_LOOP = null;
			Prince_Arm_Joint?.Unregister();
			Prince_Arm_Joint = null;
			Void_Weaver_Ability_Spawn_Thread?.Unregister();
			Void_Weaver_Ability_Spawn_Thread = null;
			Void_Weaver_Ability_Close_Portal?.Unregister();
			Void_Weaver_Ability_Close_Portal = null;
			Void_Weaver_Vanish?.Unregister();
			Void_Weaver_Vanish = null;
			Void_Weaver_Voice_1?.Unregister();
			Void_Weaver_Voice_1 = null;
			Void_Weaver_Voice_2?.Unregister();
			Void_Weaver_Voice_2 = null;
			Void_Weaver_Voice_3?.Unregister();
			Void_Weaver_Voice_3 = null;
			Void_Weaver_Voice_4?.Unregister();
			Void_Weaver_Voice_4 = null;
			Void_Weaver_Aura?.Unregister();
			Void_Weaver_Aura = null;
			Void_Weaver_Rustle?.Unregister();
			Void_Weaver_Rustle = null;
			Void_Weaver_Jangle?.Unregister();
			Void_Weaver_Jangle = null;
			Void_Weaver_Pluck_Thread?.Unregister();
			Void_Weaver_Pluck_Thread = null;
			Void_Weaver_Halo_LOOP?.Unregister();
			Void_Weaver_Halo_LOOP = null;
			Void_Weaver_Flare?.Unregister();
			Void_Weaver_Flare = null;
			Spinning_Top_Vanish?.Unregister();
			Spinning_Top_Vanish = null;
			Spinning_Top_Laugh_S?.Unregister();
			Spinning_Top_Laugh_S = null;
			Spinning_Top_Laugh_L?.Unregister();
			Spinning_Top_Laugh_L = null;
			Spinning_Top_Voice?.Unregister();
			Spinning_Top_Voice = null;
			Rot_Worm_Aura_LOOP?.Unregister();
			Rot_Worm_Aura_LOOP = null;
			Rot_Worm_Woosh_LOOP?.Unregister();
			Rot_Worm_Woosh_LOOP = null;
			Ripple_Creature_Swap_Dimensions?.Unregister();
			Ripple_Creature_Swap_Dimensions = null;
			Ripple_Creature_Death_Effect_LOOP?.Unregister();
			Ripple_Creature_Death_Effect_LOOP = null;
			Rattler_Shake_LOOP?.Unregister();
			Rattler_Shake_LOOP = null;
			Rattler_Tentacle_Grab?.Unregister();
			Rattler_Tentacle_Grab = null;
			Rattler_Squeal?.Unregister();
			Rattler_Squeal = null;
			Sky_Whale_Fin_Flap?.Unregister();
			Sky_Whale_Fin_Flap = null;
			Sky_Whale_Cry?.Unregister();
			Sky_Whale_Cry = null;
			Sky_Whale_Click?.Unregister();
			Sky_Whale_Click = null;
			Sky_Whale_Groan?.Unregister();
			Sky_Whale_Groan = null;
			Big_Sky_Whale_Flyby?.Unregister();
			Big_Sky_Whale_Flyby = null;
			Big_Sky_Whale_Call?.Unregister();
			Big_Sky_Whale_Call = null;
			Barnacle_Step?.Unregister();
			Barnacle_Step = null;
			Barnacle_Shell_Clink?.Unregister();
			Barnacle_Shell_Clink = null;
			Barnacle_Assemble_Call?.Unregister();
			Barnacle_Assemble_Call = null;
			Barnacle_Travel_Call?.Unregister();
			Barnacle_Travel_Call = null;
			Barnacle_Spit_Water?.Unregister();
			Barnacle_Spit_Water = null;
			Barnacle_Suck_Water?.Unregister();
			Barnacle_Suck_Water = null;
			Barnacle_Push_Away_Nearby_Creatures?.Unregister();
			Barnacle_Push_Away_Nearby_Creatures = null;
			Barnacle_Shell_Explode?.Unregister();
			Barnacle_Shell_Explode = null;
			Barnacle_Shell_Crack?.Unregister();
			Barnacle_Shell_Crack = null;
			Barnacle_Shell_Fragment_Hit_Terrain?.Unregister();
			Barnacle_Shell_Fragment_Hit_Terrain = null;
			Barnacle_Shell_Deflect_Projectile?.Unregister();
			Barnacle_Shell_Deflect_Projectile = null;
			Barnacle_Legs_Skitter?.Unregister();
			Barnacle_Legs_Skitter = null;
			Drill_Crab_Drill_LOOP?.Unregister();
			Drill_Crab_Drill_LOOP = null;
			Drill_Crab_Drill_Impact_LOOP?.Unregister();
			Drill_Crab_Drill_Impact_LOOP = null;
			Drill_Crab_Step?.Unregister();
			Drill_Crab_Step = null;
			Drill_Crab_Squeal?.Unregister();
			Drill_Crab_Squeal = null;
			Drill_Crab_Enter_Shell?.Unregister();
			Drill_Crab_Enter_Shell = null;
			Drill_Crab_Exit_Shell?.Unregister();
			Drill_Crab_Exit_Shell = null;
			Drill_Crab_Start_Up?.Unregister();
			Drill_Crab_Start_Up = null;
			Drill_Crab_Turn_Off?.Unregister();
			Drill_Crab_Turn_Off = null;
			Drill_Crab_Collide_Terrain?.Unregister();
			Drill_Crab_Collide_Terrain = null;
			Drill_Crab_Collide_Creature?.Unregister();
			Drill_Crab_Collide_Creature = null;
			Drill_Crab_CrabChatter_LOOP?.Unregister();
			Drill_Crab_CrabChatter_LOOP = null;
			Drill_Crab_CrabChatterLow_LOOP?.Unregister();
			Drill_Crab_CrabChatterLow_LOOP = null;
			Tower_Crab_Step?.Unregister();
			Tower_Crab_Step = null;
			Sand_Grub_Bite?.Unregister();
			Sand_Grub_Bite = null;
			Sand_Grub_Peek?.Unregister();
			Sand_Grub_Peek = null;
			Sand_Grub_Pull_From_Burrow?.Unregister();
			Sand_Grub_Pull_From_Burrow = null;
			Sand_Grub_Squeal?.Unregister();
			Sand_Grub_Squeal = null;
			Big_Sand_Grub_Bite?.Unregister();
			Big_Sand_Grub_Bite = null;
			Big_Sand_Grub_Unfurl?.Unregister();
			Big_Sand_Grub_Unfurl = null;
			Big_Sand_Grub_Slide_LOOP?.Unregister();
			Big_Sand_Grub_Slide_LOOP = null;
			Big_Sand_Grub_Squeal?.Unregister();
			Big_Sand_Grub_Squeal = null;
			Big_Sand_Grub_Rattle_LOOP?.Unregister();
			Big_Sand_Grub_Rattle_LOOP = null;
			Big_Sand_Grub_Threaten?.Unregister();
			Big_Sand_Grub_Threaten = null;
			Locust_Cloud_Near_LOOP?.Unregister();
			Locust_Cloud_Near_LOOP = null;
			Locust_Cloud_Far_LOOP?.Unregister();
			Locust_Cloud_Far_LOOP = null;
			Locust_Call_LOOP?.Unregister();
			Locust_Call_LOOP = null;
			Locust_Swarm_LOOP?.Unregister();
			Locust_Swarm_LOOP = null;
			Locust_Land?.Unregister();
			Locust_Land = null;
			Locust_Poisoned?.Unregister();
			Locust_Poisoned = null;
			Locust_Attach?.Unregister();
			Locust_Attach = null;
			Box_Worm_Rip_Out_Larva?.Unregister();
			Box_Worm_Rip_Out_Larva = null;
			Box_Worm_Pulling_Larva_LOOP?.Unregister();
			Box_Worm_Pulling_Larva_LOOP = null;
			Box_Worm_Spitting_Steam_LOOP?.Unregister();
			Box_Worm_Spitting_Steam_LOOP = null;
			Box_Worm_Idle_Steam?.Unregister();
			Box_Worm_Idle_Steam = null;
			Box_Worm_Fire_Sprite_Enter?.Unregister();
			Box_Worm_Fire_Sprite_Enter = null;
			Box_Worm_Activate_Spin?.Unregister();
			Box_Worm_Activate_Spin = null;
			Box_Worm_Activate_Ping?.Unregister();
			Box_Worm_Activate_Ping = null;
			Box_Worm_Spin_Head_LOOP?.Unregister();
			Box_Worm_Spin_Head_LOOP = null;
			Box_Worm_Crawl_Retract?.Unregister();
			Box_Worm_Crawl_Retract = null;
			Box_Worm_Crawl_Extend?.Unregister();
			Box_Worm_Crawl_Extend = null;
			Box_Worm_Hit?.Unregister();
			Box_Worm_Hit = null;
			Slugcat_Bite_Box_Worm_Larva?.Unregister();
			Slugcat_Bite_Box_Worm_Larva = null;
			Slugcat_Eat_Box_Worm_Larva?.Unregister();
			Slugcat_Eat_Box_Worm_Larva = null;
			Fire_Sprite_Wing_Spread_Idle_LOOP?.Unregister();
			Fire_Sprite_Wing_Spread_Idle_LOOP = null;
			Fire_Sprite_Flying_Boost?.Unregister();
			Fire_Sprite_Flying_Boost = null;
			Fire_Sprite_Flying_LOOP?.Unregister();
			Fire_Sprite_Flying_LOOP = null;
			Fire_Sprite_Chirp?.Unregister();
			Fire_Sprite_Chirp = null;
			Fire_Sprite_Hit?.Unregister();
			Fire_Sprite_Hit = null;
			Templar_Shield_Deflect?.Unregister();
			Templar_Shield_Deflect = null;
			Templar_Shield_Tick_1?.Unregister();
			Templar_Shield_Tick_1 = null;
			Templar_Shield_Tick_2?.Unregister();
			Templar_Shield_Tick_2 = null;
			Templar_Shield_Tick_3?.Unregister();
			Templar_Shield_Tick_3 = null;
			Templar_Shield_Tick_4?.Unregister();
			Templar_Shield_Tick_4 = null;
			Templar_Shield_Tick_5?.Unregister();
			Templar_Shield_Tick_5 = null;
			Templar_Shield_Tick_6?.Unregister();
			Templar_Shield_Tick_6 = null;
			Templar_Shield_Tick_7?.Unregister();
			Templar_Shield_Tick_7 = null;
			Templar_Shield_Tick_8?.Unregister();
			Templar_Shield_Tick_8 = null;
			Templar_Shield_Tick_9?.Unregister();
			Templar_Shield_Tick_9 = null;
			Templar_Shield_Explode?.Unregister();
			Templar_Shield_Explode = null;
			Boomerang_Flying_LOOP?.Unregister();
			Boomerang_Flying_LOOP = null;
			Boomerang_Collide_Terrain?.Unregister();
			Boomerang_Collide_Terrain = null;
			Boomerang_Collide_Creature?.Unregister();
			Boomerang_Collide_Creature = null;
			Boomerang_Thrown?.Unregister();
			Boomerang_Thrown = null;
			BlizLiz_Beam_LOOP?.Unregister();
			BlizLiz_Beam_LOOP = null;
			BlizLiz_BeamHit_LOOP?.Unregister();
			BlizLiz_BeamHit_LOOP = null;
			BlizLiz_Charge?.Unregister();
			BlizLiz_Charge = null;
			BlizLiz_Pop?.Unregister();
			BlizLiz_Pop = null;
			RotLiz_Erupt_Tentacles?.Unregister();
			RotLiz_Erupt_Tentacles = null;
			RotLiz_Pain?.Unregister();
			RotLiz_Pain = null;
			RotLiz_Vocalize?.Unregister();
			RotLiz_Vocalize = null;
			RotLiz_Hiss?.Unregister();
			RotLiz_Hiss = null;
			Lizard_Voice_Rot_A?.Unregister();
			Lizard_Voice_Rot_A = null;
			Lizard_Voice_Rot_B?.Unregister();
			Lizard_Voice_Rot_B = null;
			RotLiz_Injured?.Unregister();
			RotLiz_Injured = null;
			Moth_Step?.Unregister();
			Moth_Step = null;
			Moth_Grab?.Unregister();
			Moth_Grab = null;
			Moth_Chatter?.Unregister();
			Moth_Chatter = null;
			Moth_Chatter_Loud?.Unregister();
			Moth_Chatter_Loud = null;
			Moth_Clicking?.Unregister();
			Moth_Clicking = null;
			Moth_Wingflaps?.Unregister();
			Moth_Wingflaps = null;
			Rat_Scramble?.Unregister();
			Rat_Scramble = null;
			Rat_Chatter?.Unregister();
			Rat_Chatter = null;
			Rat_Shriek?.Unregister();
			Rat_Shriek = null;
			Loach_Step?.Unregister();
			Loach_Step = null;
			Loach_Tentacle_Grab?.Unregister();
			Loach_Tentacle_Grab = null;
			Loach_Tentacle_Release?.Unregister();
			Loach_Tentacle_Release = null;
			Loach_Passive_Groan?.Unregister();
			Loach_Passive_Groan = null;
			Loach_Aggressive_Groan?.Unregister();
			Loach_Aggressive_Groan = null;
			Loach_Curious?.Unregister();
			Loach_Curious = null;
			Loach_Lunge?.Unregister();
			Loach_Lunge = null;
			Loach_Eat?.Unregister();
			Loach_Eat = null;
			Frog_Passive_Croak?.Unregister();
			Frog_Passive_Croak = null;
			Frog_Aggressive_Croak?.Unregister();
			Frog_Aggressive_Croak = null;
			Frog_Jump?.Unregister();
			Frog_Jump = null;
			Frog_Attach?.Unregister();
			Frog_Attach = null;
			Frog_Drain?.Unregister();
			Frog_Drain = null;
			Frog_Pain?.Unregister();
			Frog_Pain = null;
			Angler_Pulse?.Unregister();
			Angler_Pulse = null;
			Angler_Swim?.Unregister();
			Angler_Swim = null;
			Angler_Shriek?.Unregister();
			Angler_Shriek = null;
			Angler_Bite?.Unregister();
			Angler_Bite = null;
			SoftToy_Squeak?.Unregister();
			SoftToy_Squeak = null;
			SoftToy_Hit?.Unregister();
			SoftToy_Hit = null;
			SpinToy_Impact?.Unregister();
			SpinToy_Impact = null;
			SpinToy_Spin?.Unregister();
			SpinToy_Spin = null;
			BallToy_Roll_LOOP?.Unregister();
			BallToy_Roll_LOOP = null;
			BallToy_BounceS?.Unregister();
			BallToy_BounceS = null;
			BallToy_BounceM?.Unregister();
			BallToy_BounceM = null;
			BallToy_BounceH?.Unregister();
			BallToy_BounceH = null;
			ARZapper_Random_Spark?.Unregister();
			ARZapper_Random_Spark = null;
			ARZapper_Powered_LOOP?.Unregister();
			ARZapper_Powered_LOOP = null;
			ARZapper_Hit?.Unregister();
			ARZapper_Hit = null;
			ARZapper_Disintegrate?.Unregister();
			ARZapper_Disintegrate = null;
			Water_Machinery_Deep?.Unregister();
			Water_Machinery_Deep = null;
			Water_Machinery_Hit?.Unregister();
			Water_Machinery_Hit = null;
			Skink_Dig_LOOP?.Unregister();
			Skink_Dig_LOOP = null;
			Skink_Dig_Deep_LOOP?.Unregister();
			Skink_Dig_Deep_LOOP = null;
			Skink_Scrape?.Unregister();
			Skink_Scrape = null;
			HeartbeatFast_LOOP?.Unregister();
			HeartbeatFast_LOOP = null;
			HeartbeatHard_LOOP?.Unregister();
			HeartbeatHard_LOOP = null;
			HeartbeatMed_LOOP?.Unregister();
			HeartbeatMed_LOOP = null;
			Ripplesick_LOOP?.Unregister();
			Ripplesick_LOOP = null;
			RipplesickPop?.Unregister();
			RipplesickPop = null;
			RipplesickSpark?.Unregister();
			RipplesickSpark = null;
			Pearl_ATalkShow?.Unregister();
			Pearl_ATalkShow = null;
			Pearl_AGrumbGroove?.Unregister();
			Pearl_AGrumbGroove = null;
			Pearl_AJam1?.Unregister();
			Pearl_AJam1 = null;
			Pearl_AJam2?.Unregister();
			Pearl_AJam2 = null;
			Pearl_AJam3?.Unregister();
			Pearl_AJam3 = null;
			Pearl_AJam4?.Unregister();
			Pearl_AJam4 = null;
			Pearl_AVoiceWind1?.Unregister();
			Pearl_AVoiceWind1 = null;
			Pearl_AVoiceWind2?.Unregister();
			Pearl_AVoiceWind2 = null;
			PearlReaderPearl?.Unregister();
			PearlReaderPearl = null;
			PearlReaderClick?.Unregister();
			PearlReaderClick = null;
			PearlReaderArm_LOOP?.Unregister();
			PearlReaderArm_LOOP = null;
			PearlReaderText_LOOP?.Unregister();
			PearlReaderText_LOOP = null;
			PearlReaderNoise?.Unregister();
			PearlReaderNoise = null;
			PearlReaderNoise_LOOP?.Unregister();
			PearlReaderNoise_LOOP = null;
			DaemonHintST?.Unregister();
			DaemonHintST = null;
			DaemonHintPrince?.Unregister();
			DaemonHintPrince = null;
			DaemonHintWeaver?.Unregister();
			DaemonHintWeaver = null;
		}
	}

	public static void InitExtEnumTypes()
	{
	}

	public static void RegisterAllEnumExtensions()
	{
		SlugcatStatsName.RegisterValues();
		MenuSceneID.RegisterValues();
		SlideShowID.RegisterValues();
		DreamID.RegisterValues();
		PlacedObjectType.RegisterValues();
		RoomEffectType.RegisterValues();
		CreatureTemplateType.RegisterValues();
		CommunityID.RegisterValues();
		GhostID.RegisterValues();
		AbstractObjectType.RegisterValues();
		WatcherSoundID.RegisterValues();
		WatcherDangerType.RegisterValues();
		WatcherSmokeType.RegisterValues();
		ConversationID.RegisterValues();
		Tutorial.RegisterValues();
		SlugcatUnlockID.RegisterValues();
		SandboxUnlockID.RegisterValues();
		LevelUnlockID.RegisterValues();
		SafariUnlockID.RegisterValues();
		DataPearlType.RegisterValues();
	}

	public static void UnregisterAllEnumExtensions()
	{
		SlugcatStatsName.UnregisterValues();
		MenuSceneID.UnregisterValues();
		SlideShowID.UnregisterValues();
		DreamID.UnregisterValues();
		PlacedObjectType.UnregisterValues();
		RoomEffectType.UnregisterValues();
		CreatureTemplateType.UnregisterValues();
		CommunityID.UnregisterValues();
		GhostID.UnregisterValues();
		AbstractObjectType.UnregisterValues();
		WatcherSoundID.UnregisterValues();
		WatcherDangerType.UnregisterValues();
		WatcherSmokeType.UnregisterValues();
		ConversationID.UnregisterValues();
		Tutorial.UnregisterValues();
		SlugcatUnlockID.UnregisterValues();
		SandboxUnlockID.UnregisterValues();
		LevelUnlockID.UnregisterValues();
		SafariUnlockID.UnregisterValues();
		DataPearlType.UnregisterValues();
	}
}
