using Menu;
using RWCustom;
using UnityEngine;

namespace Watcher;

public static class WatcherCreatures
{
	public static string CreatureName(CreatureTemplate.Type creature)
	{
		if (creature == WatcherEnums.CreatureTemplateType.Barnacle)
		{
			return "Barnacle";
		}
		if (creature == WatcherEnums.CreatureTemplateType.DrillCrab)
		{
			return "DrillCrab";
		}
		if (creature == WatcherEnums.CreatureTemplateType.TowerCrab)
		{
			return "TowerCrab";
		}
		if (creature == WatcherEnums.CreatureTemplateType.SandGrub)
		{
			return "SandGrub";
		}
		if (creature == WatcherEnums.CreatureTemplateType.BigSandGrub)
		{
			return "BigSandGrub";
		}
		if (creature == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			return "BigMoth";
		}
		if (creature == WatcherEnums.CreatureTemplateType.SmallMoth)
		{
			return "SmallMoth";
		}
		if (creature == WatcherEnums.CreatureTemplateType.BoxWorm)
		{
			return "Box Worm";
		}
		if (creature == WatcherEnums.CreatureTemplateType.FireSprite)
		{
			return "Fire Sprite";
		}
		if (creature == WatcherEnums.CreatureTemplateType.Rattler)
		{
			return "Rattler";
		}
		if (creature == WatcherEnums.CreatureTemplateType.SkyWhale)
		{
			return "SkyWhale";
		}
		if (creature == WatcherEnums.CreatureTemplateType.ScavengerTemplar)
		{
			return "ScavengerTemplar";
		}
		if (creature == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
		{
			return "ScavengerDisciple";
		}
		if (creature == WatcherEnums.CreatureTemplateType.Loach)
		{
			return "Loach";
		}
		if (creature == WatcherEnums.CreatureTemplateType.RotLoach)
		{
			return "Rot Behemoth";
		}
		if (creature == WatcherEnums.CreatureTemplateType.BlizzardLizard)
		{
			return "Blizzard Lizard";
		}
		if (creature == WatcherEnums.CreatureTemplateType.BasiliskLizard)
		{
			return "Basilisk Lizard";
		}
		if (creature == WatcherEnums.CreatureTemplateType.IndigoLizard)
		{
			return "Indigo Lizard";
		}
		if (creature == WatcherEnums.CreatureTemplateType.PeachLizard)
		{
			return "Peach Lizard";
		}
		if (creature == WatcherEnums.CreatureTemplateType.Rat)
		{
			return "Rat";
		}
		if (creature == WatcherEnums.CreatureTemplateType.Frog)
		{
			return "Frog";
		}
		if (creature == WatcherEnums.CreatureTemplateType.Tardigrade)
		{
			return "Tardigrade";
		}
		if (creature == WatcherEnums.CreatureTemplateType.GrappleSnake)
		{
			return "GrappleSnake";
		}
		if (creature == WatcherEnums.CreatureTemplateType.Millipede)
		{
			return "Millipede";
		}
		if (creature == WatcherEnums.CreatureTemplateType.Angler)
		{
			return "Angler";
		}
		if (creature == WatcherEnums.CreatureTemplateType.RippleSpider)
		{
			return "Ripple Spider";
		}
		if (creature == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			return "MothGrub";
		}
		return null;
	}

	public static void CreaturePluralNames(ref string[] creatureNames)
	{
		creatureNames[(int)WatcherEnums.CreatureTemplateType.Barnacle] = Custom.rainWorld.inGameTranslator.Translate("Barnacles");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.DrillCrab] = Custom.rainWorld.inGameTranslator.Translate("Drill Crabs");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.TowerCrab] = Custom.rainWorld.inGameTranslator.Translate("Tower Crabs");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.SandGrub] = Custom.rainWorld.inGameTranslator.Translate("Sand Grubs");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.BigSandGrub] = Custom.rainWorld.inGameTranslator.Translate("Sand Worms");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.BigMoth] = Custom.rainWorld.inGameTranslator.Translate("Big Moths");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.SmallMoth] = Custom.rainWorld.inGameTranslator.Translate("Small Moths");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.BoxWorm] = Custom.rainWorld.inGameTranslator.Translate("Box Worms");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.FireSprite] = Custom.rainWorld.inGameTranslator.Translate("Fire Sprites");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.Rattler] = Custom.rainWorld.inGameTranslator.Translate("Bone Shakers");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.SkyWhale] = Custom.rainWorld.inGameTranslator.Translate("Sky Whales");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.ScavengerTemplar] = Custom.rainWorld.inGameTranslator.Translate("Scavenger Templars");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.ScavengerDisciple] = Custom.rainWorld.inGameTranslator.Translate("Scavenger Disciples");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.Loach] = Custom.rainWorld.inGameTranslator.Translate("Loaches");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.RotLoach] = Custom.rainWorld.inGameTranslator.Translate("Rot Behemoths");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.BlizzardLizard] = Custom.rainWorld.inGameTranslator.Translate("Blizzard Lizards");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.BasiliskLizard] = Custom.rainWorld.inGameTranslator.Translate("Basilisk Lizards");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.IndigoLizard] = Custom.rainWorld.inGameTranslator.Translate("Indigo Lizards");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.PeachLizard] = Custom.rainWorld.inGameTranslator.Translate("Peach Lizards");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.Rat] = Custom.rainWorld.inGameTranslator.Translate("Rats");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.Frog] = Custom.rainWorld.inGameTranslator.Translate("Frogs");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.Tardigrade] = Custom.rainWorld.inGameTranslator.Translate("Tardigrades");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.GrappleSnake] = Custom.rainWorld.inGameTranslator.Translate("Grapple Snakes");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.Millipede] = Custom.rainWorld.inGameTranslator.Translate("Millipedes");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.Angler] = Custom.rainWorld.inGameTranslator.Translate("Anglers");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.RippleSpider] = Custom.rainWorld.inGameTranslator.Translate("Ripple Spiders");
		creatureNames[(int)WatcherEnums.CreatureTemplateType.MothGrub] = Custom.rainWorld.inGameTranslator.Translate("MothGrubs");
	}

	public static CreatureTemplate.Type CreatureTypeFromString(string s)
	{
		switch (s.ToLower())
		{
		case "barnacle":
			return WatcherEnums.CreatureTemplateType.Barnacle;
		case "drillcrab":
			return WatcherEnums.CreatureTemplateType.DrillCrab;
		case "towercrab":
			return WatcherEnums.CreatureTemplateType.TowerCrab;
		case "sandgrub":
			return WatcherEnums.CreatureTemplateType.SandGrub;
		case "bigsandgrub":
			return WatcherEnums.CreatureTemplateType.BigSandGrub;
		case "bigmoth":
		case "big moth":
			return WatcherEnums.CreatureTemplateType.BigMoth;
		case "smallmoth":
		case "small moth":
			return WatcherEnums.CreatureTemplateType.SmallMoth;
		case "boxworm":
			return WatcherEnums.CreatureTemplateType.BoxWorm;
		case "firesprite":
			return WatcherEnums.CreatureTemplateType.FireSprite;
		case "rattler":
			return WatcherEnums.CreatureTemplateType.Rattler;
		case "sky whale":
		case "skywhale":
			return WatcherEnums.CreatureTemplateType.SkyWhale;
		case "scavengertemplar":
		case "templarscavenger":
		case "scavenger templar":
		case "templar scavenger":
		case "templar":
			return WatcherEnums.CreatureTemplateType.ScavengerTemplar;
		case "scavengerdisciple":
		case "scavenger disciple":
		case "disciplescavenger":
		case "disciple scavenger":
		case "disciple":
			return WatcherEnums.CreatureTemplateType.ScavengerDisciple;
		case "loach":
			return WatcherEnums.CreatureTemplateType.Loach;
		case "rotbehemoth":
		case "rot behemoth":
		case "rbehemoth":
		case "behemoth":
		case "bigrot":
			return WatcherEnums.CreatureTemplateType.RotLoach;
		case "blizzard lizard":
		case "blizard":
		case "blizzard":
			return WatcherEnums.CreatureTemplateType.BlizzardLizard;
		case "basilisk lizard":
		case "basilisk":
			return WatcherEnums.CreatureTemplateType.BasiliskLizard;
		case "indigo lizard":
		case "indigo":
		case "skink":
			return WatcherEnums.CreatureTemplateType.IndigoLizard;
		case "peach lizard":
		case "peach":
			return WatcherEnums.CreatureTemplateType.PeachLizard;
		case "rat":
			return WatcherEnums.CreatureTemplateType.Rat;
		case "frog":
			return WatcherEnums.CreatureTemplateType.Frog;
		case "tardigrade":
		case "seapig":
			return WatcherEnums.CreatureTemplateType.Tardigrade;
		case "grapplesnake":
		case "grapple snake":
			return WatcherEnums.CreatureTemplateType.GrappleSnake;
		case "millipede":
		case "milli":
			return WatcherEnums.CreatureTemplateType.Millipede;
		case "angler":
		case "anglerfish":
		case "angler fish":
			return WatcherEnums.CreatureTemplateType.Angler;
		case "ripple spider":
		case "ripplespider":
			return WatcherEnums.CreatureTemplateType.RippleSpider;
		case "mothgrub":
		case "moth grub":
			return WatcherEnums.CreatureTemplateType.MothGrub;
		default:
			return null;
		}
	}

	public static string SpriteNameOfCreature(IconSymbol.IconSymbolData iconData)
	{
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Barnacle)
		{
			return "Kill_Barnacle";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.DrillCrab || iconData.critType == WatcherEnums.CreatureTemplateType.TowerCrab)
		{
			return "Kill_DrillCrab";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.SandGrub)
		{
			return "Kill_SandGrub";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.BigSandGrub)
		{
			return "Kill_BigSandGrub";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			return "Kill_BigMoth";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.SmallMoth)
		{
			return "Kill_SmallMoth";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.BoxWorm)
		{
			return "Kill_BoxWorm";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.FireSprite)
		{
			return "Kill_FireSprite";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.SkyWhale)
		{
			return "Kill_SkyWhale";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.ScavengerTemplar)
		{
			return "Kill_ScavengerTemplar";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
		{
			return "Kill_ScavengerDisciple";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.BlizzardLizard)
		{
			return CreatureSymbol.LizardSpriteName("Kill_BlizzardLizard", iconData.intData);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.BasiliskLizard)
		{
			return CreatureSymbol.LizardSpriteName("Kill_Basilisk", iconData.intData);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.IndigoLizard)
		{
			return CreatureSymbol.LizardSpriteName("Kill_IndigoLizard", iconData.intData);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.PeachLizard)
		{
			return CreatureSymbol.LizardSpriteName("Kill_PeachLizard", iconData.intData);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Rat)
		{
			return "Kill_Rat";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Frog)
		{
			return "Kill_Frog";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Loach)
		{
			return "Kill_Loach";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.RotLoach)
		{
			return "Kill_RotLoach";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Rattler)
		{
			return "Kill_Rattler";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Tardigrade)
		{
			return "Kill_Tardigrade";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.GrappleSnake)
		{
			return "Futile_White";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Millipede)
		{
			return "Futile_White";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Angler)
		{
			return "Kill_Angler";
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			return "Kill_MothGrub";
		}
		return null;
	}

	public static Color? ColorOfCreature(IconSymbol.IconSymbolData iconData)
	{
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Barnacle)
		{
			return null;
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.DrillCrab)
		{
			return global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.TowerCrab)
		{
			return new Color(0.32f, 0.22f, 0.18f);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			return Color.white;
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.SmallMoth)
		{
			return null;
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.BoxWorm)
		{
			return new Color(0f, 0.9098039f, 46f / 51f);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.FireSprite)
		{
			return new Color(0f, 0.9098039f, 46f / 51f);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.SkyWhale)
		{
			if (iconData.intData == 1)
			{
				return new Color(0.5f, 0.45f, 0.35f);
			}
			return global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.White);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Frog)
		{
			return new Color(0.6784314f, 4f / 15f, 18f / 85f);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Rat)
		{
			return new Color(0.6784314f, 4f / 15f, 18f / 85f);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Rattler)
		{
			return new Color(0.3f, 0f, 1f);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.RotLoach)
		{
			return new Color(0.3f, 0f, 1f);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.ScavengerTemplar || iconData.critType == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
		{
			return new Color(1f, 0.8f, 0.3f);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.BlizzardLizard)
		{
			return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.BasiliskLizard)
		{
			return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.IndigoLizard)
		{
			return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.PeachLizard)
		{
			return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Tardigrade)
		{
			return new Color(0f, 0.9098039f, 46f / 51f);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.GrappleSnake)
		{
			return global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey);
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Millipede)
		{
			return Color.yellow;
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.Angler)
		{
			return null;
		}
		if (iconData.critType == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			return new Color(1f, 0.7f, 0.55f);
		}
		return null;
	}

	public static bool DoesCreatureEarnATrophy(CreatureTemplate.Type creature)
	{
		if (creature == WatcherEnums.CreatureTemplateType.Rat || creature == WatcherEnums.CreatureTemplateType.SkyWhale || creature == WatcherEnums.CreatureTemplateType.Loach || creature == WatcherEnums.CreatureTemplateType.RotLoach || creature == WatcherEnums.CreatureTemplateType.GrappleSnake)
		{
			return false;
		}
		return true;
	}

	public static void DefaultKillScores(ref int[] killScores)
	{
		killScores[(int)WatcherEnums.SandboxUnlockID.Barnacle] = 4;
		killScores[(int)WatcherEnums.SandboxUnlockID.Tardigrade] = 2;
		killScores[(int)WatcherEnums.SandboxUnlockID.Frog] = 1;
		killScores[(int)WatcherEnums.SandboxUnlockID.DrillCrab] = 16;
		killScores[(int)WatcherEnums.SandboxUnlockID.SandGrub] = 2;
		killScores[(int)WatcherEnums.SandboxUnlockID.BigSandGrub] = 4;
		killScores[(int)WatcherEnums.SandboxUnlockID.MothGrub] = 1;
		killScores[(int)WatcherEnums.SandboxUnlockID.SmallMoth] = 2;
		killScores[(int)WatcherEnums.SandboxUnlockID.BigMoth] = 8;
		killScores[(int)WatcherEnums.SandboxUnlockID.BoxWorm] = 12;
		killScores[(int)WatcherEnums.SandboxUnlockID.FireSprite] = 4;
		killScores[(int)WatcherEnums.SandboxUnlockID.ScavengerTemplar] = 12;
		killScores[(int)WatcherEnums.SandboxUnlockID.ScavengerDisciple] = 12;
		killScores[(int)WatcherEnums.SandboxUnlockID.Loach] = 25;
		killScores[(int)WatcherEnums.SandboxUnlockID.RotLoach] = 25;
		killScores[(int)WatcherEnums.SandboxUnlockID.Rattler] = 4;
		killScores[(int)WatcherEnums.SandboxUnlockID.IndigoLizard] = 7;
		killScores[(int)WatcherEnums.SandboxUnlockID.BlizzardLizard] = 25;
		killScores[(int)WatcherEnums.SandboxUnlockID.BasiliskLizard] = 9;
		killScores[(int)WatcherEnums.SandboxUnlockID.ProtoLizard] = 7;
		killScores[(int)WatcherEnums.SandboxUnlockID.RotLizard] = 14;
		killScores[(int)WatcherEnums.SandboxUnlockID.PeachLizard] = 5;
		killScores[(int)WatcherEnums.SandboxUnlockID.Rat] = 1;
		killScores[(int)WatcherEnums.SandboxUnlockID.Angler] = 8;
		killScores[(int)WatcherEnums.SandboxUnlockID.SkyWhale] = 25;
		killScores[(int)WatcherEnums.SandboxUnlockID.AltSkyWhale] = 25;
	}

	public static bool CreaturePerfEstimate(CreatureTemplate.Type critType, ref float linear, ref float exponential)
	{
		if (critType == WatcherEnums.CreatureTemplateType.Barnacle)
		{
			linear += 0.2f;
			exponential += 0.3f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.DrillCrab || critType == WatcherEnums.CreatureTemplateType.TowerCrab)
		{
			linear += 1.5f;
			exponential += 0.3f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.SandGrub)
		{
			linear += 0.2f;
			exponential += 0.3f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.BigSandGrub)
		{
			linear += 0.2f;
			exponential += 0.3f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.Rattler)
		{
			linear += 0.5f;
			exponential += 0.3f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.SkyWhale)
		{
			linear += 1.5f;
			exponential += 1.5f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.ScavengerTemplar)
		{
			linear += 0.5f;
			exponential += 0.925f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.GrappleSnake)
		{
			linear += 3f;
			exponential += 1.5f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.ScavengerDisciple)
		{
			linear += 0.5f;
			exponential += 0.925f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.Loach)
		{
			linear += 3f;
			exponential += 1.5f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.RotLoach)
		{
			linear += 3f;
			exponential += 1.5f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.Rat)
		{
			linear += 0.5f;
			exponential += 0.1f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.Frog)
		{
			linear += 0.4f;
			exponential += 0.4f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.Tardigrade)
		{
			linear += 0.2f;
			exponential += 0.3f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.BoxWorm)
		{
			linear += 1.1f;
			exponential += 0.65f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.FireSprite)
		{
			linear += 0.4f;
			exponential += 0.5f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			linear += 1.1f;
			exponential += 0.65f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.SmallMoth)
		{
			linear += 0.5f;
			exponential += 0.5f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.BlizzardLizard)
		{
			linear += 0.75f;
			exponential += 1.15f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.BasiliskLizard)
		{
			linear += 0.6f;
			exponential += 0.5f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.IndigoLizard)
		{
			linear += 0.6f;
			exponential += 0.5f;
			return true;
		}
		if (critType == WatcherEnums.CreatureTemplateType.PeachLizard)
		{
			linear += 0.6f;
			exponential += 0.5f;
			return true;
		}
		return false;
	}

	public static string CritString(AbstractCreature crit)
	{
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Barnacle)
		{
			return "ba";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.DrillCrab)
		{
			return "Cr";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.TowerCrab)
		{
			return "TC";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			return "BMo";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SmallMoth)
		{
			return "sMo";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BoxWorm)
		{
			return "BW";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.FireSprite)
		{
			return "FS";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SkyWhale)
		{
			return "SW";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BlizzardLizard)
		{
			return "blzL";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BasiliskLizard)
		{
			return "bskL";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.IndigoLizard)
		{
			return "iL";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.PeachLizard)
		{
			return "coL";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Tardigrade)
		{
			return "p";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.GrappleSnake)
		{
			return "GS";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Millipede)
		{
			return "Mi";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Angler)
		{
			return "Ar";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Loach)
		{
			return "Lo";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.RotLoach)
		{
			return "RB";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Rat)
		{
			return "R";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Frog)
		{
			return "F";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Rattler)
		{
			return "Ra";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SandGrub)
		{
			return "sg";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigSandGrub)
		{
			return "SG";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			return "gMo";
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.RippleSpider)
		{
			return "RS";
		}
		return null;
	}

	public static Color? CritCol(AbstractCreature crit)
	{
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Barnacle)
		{
			return new Color(1f, 1f, 1f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.DrillCrab)
		{
			return new Color(0.25f, 0.25f, 0.3f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.TowerCrab)
		{
			return new Color(0.15f, 0.15f, 0.15f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigMoth)
		{
			return Color.white;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BoxWorm)
		{
			return Color.cyan * Color.grey;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.FireSprite)
		{
			return Color.cyan;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SmallMoth)
		{
			return Color.gray;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SkyWhale)
		{
			return Color.white;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BlizzardLizard)
		{
			return Color.white;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BasiliskLizard)
		{
			return Color.yellow;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.IndigoLizard)
		{
			return new Color(0.2f, 0f, 1f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.PeachLizard)
		{
			return new Color(0.5f, 0f, 0.9f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Tardigrade)
		{
			return Color.cyan;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.GrappleSnake)
		{
			return new Color(1f, 1f, 1f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Millipede)
		{
			return Color.yellow;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Angler)
		{
			return Color.yellow;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Loach)
		{
			return new Color(0.1f, 0.4f, 0.25f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.RotLoach)
		{
			return RainWorld.RippleColor;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Rat)
		{
			return new Color(0.55f, 0.5f, 0.3f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Frog)
		{
			return new Color(0.4f, 0.25f, 0.1f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.Rattler)
		{
			return RainWorld.RippleColor;
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SandGrub)
		{
			return new Color(0.55f, 0.5f, 0.3f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigSandGrub)
		{
			return new Color(0.55f, 0.5f, 0.3f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.MothGrub)
		{
			return new Color(1f, 0.7f, 0.55f);
		}
		if (crit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.RippleSpider)
		{
			return new Color(0.5f, 0.2f, 1f);
		}
		return null;
	}

	public static bool IsThisHostileCreatureForShelter(AbstractCreature creature)
	{
		return false;
	}

	public static bool IsThisBigCreatureForShelter(AbstractCreature creature)
	{
		return false;
	}
}
