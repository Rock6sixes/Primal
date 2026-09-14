using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public static class MoreSlugcatsCreatures
{
	public static string CreatureName(CreatureTemplate.Type creature)
	{
		if (ModManager.DLCShared)
		{
			if (creature == DLCSharedEnums.CreatureTemplateType.MirosVulture)
			{
				return "Miros Vulture";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.SpitLizard)
			{
				return "Caramel Lizard";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.EelLizard)
			{
				return "Eel";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.ZoopLizard)
			{
				return "Strawberry";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.MotherSpider)
			{
				return "Mother Spider";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return "Terror Long Legs";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.AquaCenti)
			{
				return "Aquapede";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.StowawayBug)
			{
				return "Stowaway Bug";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.ScavengerElite)
			{
				return "Elite Scavenger";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.Inspector)
			{
				return "Inspector";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.Yeek)
			{
				return "Yeek";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.BigJelly)
			{
				return "Big Jellyfish";
			}
			if (creature == DLCSharedEnums.CreatureTemplateType.JungleLeech)
			{
				return "Jungle Leech";
			}
		}
		if (ModManager.MSC)
		{
			if (creature == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				return "Hunter";
			}
			if (creature == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				return "Hell Bug";
			}
			if (creature == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
			{
				return "King Scavenger";
			}
			if (creature == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				return "Slugcat NPC";
			}
			if (creature == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
			{
				return "Train Lizard";
			}
		}
		return null;
	}

	public static void CreaturePluralNames(ref string[] creatureNames)
	{
		if (ModManager.DLCShared)
		{
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.MirosVulture] = Custom.rainWorld.inGameTranslator.Translate("Miros Vultures");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.MotherSpider] = Custom.rainWorld.inGameTranslator.Translate("Mother Spiders");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.EelLizard] = Custom.rainWorld.inGameTranslator.Translate("Eel Lizards");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.SpitLizard] = Custom.rainWorld.inGameTranslator.Translate("Caramel Lizards");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.TerrorLongLegs] = Custom.rainWorld.inGameTranslator.Translate("Terror Long Legs");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.AquaCenti] = Custom.rainWorld.inGameTranslator.Translate("Aquapedes");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.Inspector] = Custom.rainWorld.inGameTranslator.Translate("Inspectors");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.Yeek] = Custom.rainWorld.inGameTranslator.Translate("Yeek");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.BigJelly] = Custom.rainWorld.inGameTranslator.Translate("Large Jellyfish");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.StowawayBug] = Custom.rainWorld.inGameTranslator.Translate("Stowaway Bugs");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.ZoopLizard] = Custom.rainWorld.inGameTranslator.Translate("Strawberry Lizards");
			creatureNames[(int)DLCSharedEnums.CreatureTemplateType.ScavengerElite] = Custom.rainWorld.inGameTranslator.Translate("Elite Scavengers");
		}
		if (ModManager.MSC)
		{
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.FireBug] = Custom.rainWorld.inGameTranslator.Translate("Firebugs");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.SlugNPC] = Custom.rainWorld.inGameTranslator.Translate("Slugcats");
		}
	}

	public static CreatureTemplate.Type CreatureTypeFromString(string s)
	{
		if (ModManager.DLCShared)
		{
			CreatureTemplate.Type type = null;
			switch (s.ToLower())
			{
			case "aquacentipede":
			case "aqua centipede":
			case "aquacenti":
			case "aqua centi":
			case "aquapede":
				type = DLCSharedEnums.CreatureTemplateType.AquaCenti;
				break;
			case "caramel":
				type = DLCSharedEnums.CreatureTemplateType.SpitLizard;
				break;
			case "strawberry":
				type = DLCSharedEnums.CreatureTemplateType.ZoopLizard;
				break;
			case "eel":
				type = DLCSharedEnums.CreatureTemplateType.EelLizard;
				break;
			case "terror":
			case "terror long legs":
			case "terrorlonglegs":
			case "mother":
			case "mother long legs":
			case "motherlonglegs":
				type = DLCSharedEnums.CreatureTemplateType.TerrorLongLegs;
				break;
			case "motherspider":
			case "mother spider":
				type = DLCSharedEnums.CreatureTemplateType.MotherSpider;
				break;
			case "mirosvulture":
			case "miros vulture":
				type = DLCSharedEnums.CreatureTemplateType.MirosVulture;
				break;
			case "stowaway":
			case "stowaway bug":
				type = DLCSharedEnums.CreatureTemplateType.StowawayBug;
				break;
			case "scavengerelite":
			case "elitescavenger":
			case "scavenger elite":
			case "elite scavenger":
			case "elite":
				type = DLCSharedEnums.CreatureTemplateType.ScavengerElite;
				break;
			case "inspector":
				type = DLCSharedEnums.CreatureTemplateType.Inspector;
				break;
			case "yeek":
				type = DLCSharedEnums.CreatureTemplateType.Yeek;
				break;
			case "bigjelly":
			case "big jelly":
				type = DLCSharedEnums.CreatureTemplateType.BigJelly;
				break;
			}
			if (type != null)
			{
				return type;
			}
		}
		if (ModManager.MSC)
		{
			CreatureTemplate.Type type2 = null;
			switch (s.ToLower())
			{
			case "train":
				type2 = MoreSlugcatsEnums.CreatureTemplateType.TrainLizard;
				break;
			case "hunterdaddy":
			case "hunter daddy":
			case "hunter":
				type2 = MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy;
				break;
			case "hellbug":
			case "hell bug":
			case "firebug":
			case "fire bug":
				type2 = MoreSlugcatsEnums.CreatureTemplateType.FireBug;
				break;
			case "slugnpc":
			case "slug npc":
			case "slugcatnpc":
			case "slugcat npc":
				type2 = MoreSlugcatsEnums.CreatureTemplateType.SlugNPC;
				break;
			}
			if (type2 != null)
			{
				return type2;
			}
		}
		return null;
	}

	public static string SpriteNameOfCreature(IconSymbol.IconSymbolData iconData)
	{
		if (ModManager.DLCShared)
		{
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.ZoopLizard)
			{
				return CreatureSymbol.LizardSpriteName("Kill_White_Lizard", iconData.intData);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.EelLizard)
			{
				return CreatureSymbol.LizardSpriteName("Kill_Salamander", iconData.intData);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.JungleLeech)
			{
				return "Kill_Leech";
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return "Kill_Daddy";
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.MotherSpider)
			{
				return "Kill_BigSpider";
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.StowawayBug)
			{
				return "Kill_Stowaway";
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.AquaCenti)
			{
				return "Kill_Centiwing";
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.MirosVulture)
			{
				return "Kill_MirosBird";
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.ScavengerElite)
			{
				return "Kill_ScavengerElite";
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.SpitLizard)
			{
				return CreatureSymbol.LizardSpriteName("Kill_Spit_Lizard", iconData.intData);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.Inspector)
			{
				return "Kill_Inspector";
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.Yeek)
			{
				return "Kill_Yeek";
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.BigJelly)
			{
				return "Kill_BigJellyFish";
			}
		}
		if (ModManager.MSC)
		{
			if (iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
			{
				return CreatureSymbol.LizardSpriteName("Kill_Standard_Lizard", iconData.intData);
			}
			if (iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				return "Kill_Slugcat";
			}
			if (iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				return "Kill_FireBug";
			}
			if (iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing)
			{
				return "Kill_ScavengerKing";
			}
			if (iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				return "Kill_Slugcat";
			}
		}
		return null;
	}

	public static Color? ColorOfCreature(IconSymbol.IconSymbolData iconData)
	{
		if (ModManager.DLCShared)
		{
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.ZoopLizard)
			{
				return new Color(0.95f, 0.73f, 0.73f);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.StowawayBug)
			{
				return new Color(0.36862746f, 0.36862746f, 37f / 85f);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.AquaCenti)
			{
				return new Color(0f, 0f, 1f);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.MotherSpider || iconData.critType == DLCSharedEnums.CreatureTemplateType.JungleLeech)
			{
				return new Color(0.1f, 0.7f, 0.1f);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.MirosVulture)
			{
				return new Color(46f / 51f, 0.05490196f, 0.05490196f);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.SpitLizard)
			{
				return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.EelLizard)
			{
				return new Color(0.02f, 0.78039217f, 0.2f);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.Inspector)
			{
				return new Color(38f / 85f, 46f / 51f, 0.76862746f);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.Yeek)
			{
				return new Color(0.9f, 0.9f, 0.9f);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.BigJelly)
			{
				return new Color(1f, 0.85f, 0.7f);
			}
			if (iconData.critType == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return new Color(0.3f, 0f, 1f);
			}
		}
		if (ModManager.MSC)
		{
			if (iconData.critType == CreatureTemplate.Type.Vulture || iconData.critType == CreatureTemplate.Type.KingVulture)
			{
				return new Color(0.83137256f, 0.7921569f, 37f / 85f);
			}
			if (iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
			{
				return new Color(0.3f, 0f, 1f);
			}
			if (iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				return Color.Lerp(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red), Color.gray, 0.4f);
			}
			if (iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				return new Color(1f, 0.47058824f, 0.47058824f);
			}
		}
		return null;
	}

	public static bool DoesCreatureEarnATrophy(CreatureTemplate.Type creature)
	{
		return true;
	}

	public static void DefaultKillScores(ref int[] killScores)
	{
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.MirosVulture] = 25;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.MotherSpider] = 4;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.EelLizard] = 6;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.SpitLizard] = 11;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.TerrorLongLegs] = 25;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.AquaCenti] = 10;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.FireBug] = 5;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.Inspector] = 12;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.Yeek] = 2;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.BigJelly] = 20;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.StowawayBug] = 8;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.ZoopLizard] = 6;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.ScavengerElite] = 12;
		killScores[(int)MoreSlugcatsEnums.SandboxUnlockID.SlugNPC] = 2;
	}

	public static bool CreaturePerfEstimate(CreatureTemplate.Type critType, ref float linear, ref float exponential)
	{
		if (ModManager.DLCShared)
		{
			if (critType == DLCSharedEnums.CreatureTemplateType.SpitLizard || critType == DLCSharedEnums.CreatureTemplateType.EelLizard || critType == DLCSharedEnums.CreatureTemplateType.ZoopLizard)
			{
				linear += 0.5f;
				exponential += 0.5f;
				return true;
			}
			if (critType == DLCSharedEnums.CreatureTemplateType.AquaCenti)
			{
				linear += 0.8f;
				exponential += 0.5f;
				return true;
			}
			if (critType == DLCSharedEnums.CreatureTemplateType.ScavengerElite)
			{
				linear += 0.5f;
				exponential += 0.925f;
				return true;
			}
			if (critType == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
			{
				linear += 4f;
				exponential += 1.75f;
				return true;
			}
			if (critType == DLCSharedEnums.CreatureTemplateType.MotherSpider)
			{
				linear += 3f;
				exponential += 1.5f;
				return true;
			}
			if (critType == DLCSharedEnums.CreatureTemplateType.StowawayBug)
			{
				linear += 3f;
				exponential += 1.5f;
				return true;
			}
			if (critType == DLCSharedEnums.CreatureTemplateType.Inspector)
			{
				linear += 2.4f;
				exponential += 1.2f;
				return true;
			}
			if (critType == DLCSharedEnums.CreatureTemplateType.Yeek)
			{
				linear += 0.3f;
				exponential += 0.4f;
				return true;
			}
			if (critType == DLCSharedEnums.CreatureTemplateType.BigJelly)
			{
				linear += 3f;
				exponential += 1.9f;
				return true;
			}
		}
		if (ModManager.MSC)
		{
			if (critType == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				linear += 0.3f;
				exponential += 0.4f;
				return true;
			}
			if (critType == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				linear += 0.5f;
				exponential += 0.925f;
				return true;
			}
		}
		return false;
	}

	public static string CritString(AbstractCreature crit)
	{
		if (ModManager.DLCShared)
		{
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return "Tll";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.AquaCenti)
			{
				return "aC";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.MirosVulture)
			{
				return "mV";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.MotherSpider)
			{
				return "SM";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.StowawayBug)
			{
				return "Sb";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.SpitLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.EelLizard)
			{
				return "E";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ZoopLizard)
			{
				return "L";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.Inspector)
			{
				return "Is";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.Yeek)
			{
				return "Ye";
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.BigJelly)
			{
				return "Jy";
			}
		}
		if (ModManager.MSC)
		{
			if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				return "hll";
			}
			if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				return "Fb";
			}
			if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				return "NPC";
			}
		}
		return null;
	}

	public static Color? CritCol(AbstractCreature crit)
	{
		if (ModManager.DLCShared)
		{
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs || crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.AquaCenti)
			{
				return new Color(0f, 0f, 1f);
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.MirosVulture)
			{
				new Color(0.75f, 0f, 0f);
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.MotherSpider)
			{
				new Color(0f, 1f, 0f);
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.StowawayBug)
			{
				return new Color(0f, 1f, 0.47058824f);
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.SpitLizard)
			{
				return new Color(0.5f, 0.5f, 0.5f);
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.EelLizard)
			{
				return new Color(0f, 0f, 0f, 1f);
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.ZoopLizard)
			{
				return new Color(0.95f, 0.73f, 0.73f);
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.Inspector)
			{
				return Color.white;
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.Yeek)
			{
				return new Color(1f, 1f, 0.7f);
			}
			if (crit.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.BigJelly)
			{
				return new Color(1f, 1f, 1f);
			}
		}
		if (ModManager.MSC)
		{
			if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				new Color(0.57f, 0.11f, 0.23f);
			}
			if (crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				return new Color(1f, 0.5f, 0.5f);
			}
		}
		return null;
	}

	public static bool IsThisHostileCreatureForShelter(AbstractCreature creature)
	{
		CreatureTemplate.Type type = creature.creatureTemplate.type;
		if (ModManager.DLCShared)
		{
			if (type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return true;
			}
			if (type == DLCSharedEnums.CreatureTemplateType.MirosVulture)
			{
				return true;
			}
		}
		if (ModManager.MSC && type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
		{
			return true;
		}
		return false;
	}

	public static bool IsThisBigCreatureForShelter(AbstractCreature creature)
	{
		CreatureTemplate.Type type = creature.creatureTemplate.type;
		if (ModManager.DLCShared)
		{
			if (type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return true;
			}
			if (type == DLCSharedEnums.CreatureTemplateType.MirosVulture)
			{
				return true;
			}
		}
		return false;
	}
}
