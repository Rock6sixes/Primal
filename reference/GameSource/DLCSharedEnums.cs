public class DLCSharedEnums
{
	public class RoomEffectType
	{
		public static RoomSettings.RoomEffect.Type InvertedWater;

		public static RoomSettings.RoomEffect.Type RoomWrap;

		public static RoomSettings.RoomEffect.Type DustWave;

		public static void RegisterValues()
		{
			InvertedWater = new RoomSettings.RoomEffect.Type("InvertedWater", register: true);
			RoomWrap = new RoomSettings.RoomEffect.Type("RoomWrap", register: true);
			DustWave = new RoomSettings.RoomEffect.Type("DustWave", register: true);
		}

		public static void UnregisterValues()
		{
			InvertedWater?.Unregister();
			InvertedWater = null;
			RoomWrap?.Unregister();
			RoomWrap = null;
			DustWave?.Unregister();
			DustWave = null;
		}
	}

	public class PlacedObjectType
	{
		public static PlacedObject.Type OEsphere;

		public static PlacedObject.Type MSArteryPush;

		public static PlacedObject.Type GooieDuck;

		public static PlacedObject.Type LillyPuck;

		public static PlacedObject.Type GlowWeed;

		public static PlacedObject.Type BigJellyFish;

		public static PlacedObject.Type RotFlyPaper;

		public static PlacedObject.Type DandelionPeach;

		public static PlacedObject.Type Stowaway;

		public static void RegisterValues()
		{
			OEsphere = new PlacedObject.Type("OEsphere", register: true);
			MSArteryPush = new PlacedObject.Type("MSArteryPush", register: true);
			GooieDuck = new PlacedObject.Type("GooieDuck", register: true);
			LillyPuck = new PlacedObject.Type("LillyPuck", register: true);
			GlowWeed = new PlacedObject.Type("GlowWeed", register: true);
			BigJellyFish = new PlacedObject.Type("BigJellyFish", register: true);
			RotFlyPaper = new PlacedObject.Type("RotFlyPaper", register: true);
			DandelionPeach = new PlacedObject.Type("DandelionPeach", register: true);
			Stowaway = new PlacedObject.Type("Stowaway", register: true);
		}

		public static void UnregisterValues()
		{
			OEsphere?.Unregister();
			OEsphere = null;
			MSArteryPush?.Unregister();
			MSArteryPush = null;
			GooieDuck?.Unregister();
			GooieDuck = null;
			LillyPuck?.Unregister();
			LillyPuck = null;
			GlowWeed?.Unregister();
			GlowWeed = null;
			BigJellyFish?.Unregister();
			BigJellyFish = null;
			RotFlyPaper?.Unregister();
			RotFlyPaper = null;
			DandelionPeach?.Unregister();
			DandelionPeach = null;
			Stowaway?.Unregister();
			Stowaway = null;
		}
	}

	public class CreatureTemplateType
	{
		public static CreatureTemplate.Type MirosVulture;

		public static CreatureTemplate.Type SpitLizard;

		public static CreatureTemplate.Type EelLizard;

		public static CreatureTemplate.Type MotherSpider;

		public static CreatureTemplate.Type TerrorLongLegs;

		public static CreatureTemplate.Type AquaCenti;

		public static CreatureTemplate.Type StowawayBug;

		public static CreatureTemplate.Type ScavengerElite;

		public static CreatureTemplate.Type Inspector;

		public static CreatureTemplate.Type Yeek;

		public static CreatureTemplate.Type BigJelly;

		public static CreatureTemplate.Type JungleLeech;

		public static CreatureTemplate.Type ZoopLizard;

		public static void RegisterValues()
		{
			MirosVulture = new CreatureTemplate.Type("MirosVulture", register: true);
			SpitLizard = new CreatureTemplate.Type("SpitLizard", register: true);
			EelLizard = new CreatureTemplate.Type("EelLizard", register: true);
			MotherSpider = new CreatureTemplate.Type("MotherSpider", register: true);
			TerrorLongLegs = new CreatureTemplate.Type("TerrorLongLegs", register: true);
			AquaCenti = new CreatureTemplate.Type("AquaCenti", register: true);
			StowawayBug = new CreatureTemplate.Type("StowawayBug", register: true);
			ScavengerElite = new CreatureTemplate.Type("ScavengerElite", register: true);
			Inspector = new CreatureTemplate.Type("Inspector", register: true);
			Yeek = new CreatureTemplate.Type("Yeek", register: true);
			BigJelly = new CreatureTemplate.Type("BigJelly", register: true);
			JungleLeech = new CreatureTemplate.Type("JungleLeech", register: true);
			ZoopLizard = new CreatureTemplate.Type("ZoopLizard", register: true);
		}

		public static void UnregisterValues()
		{
			MirosVulture?.Unregister();
			MirosVulture = null;
			SpitLizard?.Unregister();
			SpitLizard = null;
			EelLizard?.Unregister();
			EelLizard = null;
			MotherSpider?.Unregister();
			MotherSpider = null;
			TerrorLongLegs?.Unregister();
			TerrorLongLegs = null;
			AquaCenti?.Unregister();
			AquaCenti = null;
			StowawayBug?.Unregister();
			StowawayBug = null;
			ScavengerElite?.Unregister();
			ScavengerElite = null;
			Inspector?.Unregister();
			Inspector = null;
			Yeek?.Unregister();
			Yeek = null;
			BigJelly?.Unregister();
			BigJelly = null;
			JungleLeech?.Unregister();
			JungleLeech = null;
			ZoopLizard?.Unregister();
			ZoopLizard = null;
		}
	}

	public class AbstractObjectType
	{
		public static AbstractPhysicalObject.AbstractObjectType SingularityBomb;

		public static AbstractPhysicalObject.AbstractObjectType Seed;

		public static AbstractPhysicalObject.AbstractObjectType GooieDuck;

		public static AbstractPhysicalObject.AbstractObjectType LillyPuck;

		public static AbstractPhysicalObject.AbstractObjectType GlowWeed;

		public static AbstractPhysicalObject.AbstractObjectType DandelionPeach;

		public static void RegisterValues()
		{
			SingularityBomb = new AbstractPhysicalObject.AbstractObjectType("SingularityBomb", register: true);
			Seed = new AbstractPhysicalObject.AbstractObjectType("Seed", register: true);
			GooieDuck = new AbstractPhysicalObject.AbstractObjectType("GooieDuck", register: true);
			LillyPuck = new AbstractPhysicalObject.AbstractObjectType("LillyPuck", register: true);
			GlowWeed = new AbstractPhysicalObject.AbstractObjectType("GlowWeed", register: true);
			DandelionPeach = new AbstractPhysicalObject.AbstractObjectType("DandelionPeach", register: true);
		}

		public static void UnregisterValues()
		{
			SingularityBomb?.Unregister();
			SingularityBomb = null;
			Seed?.Unregister();
			Seed = null;
			GooieDuck?.Unregister();
			GooieDuck = null;
			LillyPuck?.Unregister();
			LillyPuck = null;
			GlowWeed?.Unregister();
			GlowWeed = null;
			DandelionPeach?.Unregister();
			DandelionPeach = null;
		}
	}

	public class RoomRainDangerType
	{
		public static RoomRain.DangerType Blizzard;

		public static void RegisterValues()
		{
			Blizzard = new RoomRain.DangerType("Blizzard", register: true);
		}

		public static void UnregisterValues()
		{
			if (Blizzard != null)
			{
				Blizzard.Unregister();
				Blizzard = null;
			}
		}
	}

	public class ScavengerAnimationID
	{
		public static Scavenger.ScavengerAnimation.ID PrepareToJump;

		public static Scavenger.ScavengerAnimation.ID Jumping;

		public static void RegisterValues()
		{
			PrepareToJump = new Scavenger.ScavengerAnimation.ID("PrepareToJump", register: true);
			Jumping = new Scavenger.ScavengerAnimation.ID("Jumping", register: true);
		}

		public static void UnregisterValues()
		{
			PrepareToJump?.Unregister();
			PrepareToJump = null;
			Jumping?.Unregister();
			Jumping = null;
		}
	}

	public class SharedSoundID
	{
		public static SoundID Singularity;

		public static SoundID Duck_Pop;

		public static SoundID Terror_Moo;

		public static SoundID Volt_Shock;

		public static void RegisterValues()
		{
			Duck_Pop = new SoundID("Duck_Pop", register: true);
			Singularity = new SoundID("Singularity", register: true);
			Terror_Moo = new SoundID("Terror_Moo", register: true);
			Volt_Shock = new SoundID("Volt_Shock", register: true);
		}

		public static void UnregisterValues()
		{
			Duck_Pop?.Unregister();
			Duck_Pop = null;
			Singularity?.Unregister();
			Singularity = null;
			Terror_Moo?.Unregister();
			Terror_Moo = null;
			Volt_Shock?.Unregister();
			Volt_Shock = null;
		}
	}

	public class ProcessID
	{
		public static ProcessManager.ProcessID SpecialUnlock;

		public static void RegisterValues()
		{
			SpecialUnlock = new ProcessManager.ProcessID("SpecialUnlock", register: true);
		}

		public static void UnregisterValues()
		{
			SpecialUnlock?.Unregister();
			SpecialUnlock = null;
		}
	}

	public class GameTypeID
	{
		public static ArenaSetup.GameTypeID Challenge;

		public static void RegisterValues()
		{
			if (ModManager.ChallengeModule)
			{
				Challenge = new ArenaSetup.GameTypeID("Challenge", register: true);
			}
		}

		public static void UnregisterValues()
		{
			Challenge?.Unregister();
			Challenge = null;
		}
	}

	public static void InitExtEnumTypes()
	{
		_ = RoomEffectType.RoomWrap;
		_ = PlacedObjectType.OEsphere;
		_ = CreatureTemplateType.BigJelly;
		_ = AbstractObjectType.Seed;
		_ = RoomRainDangerType.Blizzard;
		_ = ScavengerAnimationID.Jumping;
		_ = SharedSoundID.Terror_Moo;
		_ = ProcessID.SpecialUnlock;
		_ = GameTypeID.Challenge;
	}

	public static void RegisterAllEnumExtensions()
	{
		PlacedObjectType.RegisterValues();
		RoomEffectType.RegisterValues();
		CreatureTemplateType.RegisterValues();
		AbstractObjectType.RegisterValues();
		RoomRainDangerType.RegisterValues();
		ScavengerAnimationID.RegisterValues();
		SharedSoundID.RegisterValues();
		ProcessID.RegisterValues();
		GameTypeID.RegisterValues();
	}

	public static void UnregisterAllEnumExtensions()
	{
		PlacedObjectType.UnregisterValues();
		RoomEffectType.UnregisterValues();
		CreatureTemplateType.UnregisterValues();
		AbstractObjectType.UnregisterValues();
		RoomRainDangerType.UnregisterValues();
		ScavengerAnimationID.UnregisterValues();
		SharedSoundID.UnregisterValues();
		ProcessID.UnregisterValues();
		GameTypeID.UnregisterValues();
	}
}
