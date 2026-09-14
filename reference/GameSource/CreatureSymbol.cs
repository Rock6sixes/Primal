using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class CreatureSymbol : IconSymbol
{
	public CreatureTemplate.Type critType => iconData.critType;

	public CreatureSymbol(IconSymbolData iconData, FContainer container)
		: base(iconData, container)
	{
		myColor = ColorOfCreature(iconData);
		spriteName = SpriteNameOfCreature(iconData);
		graphWidth = Futile.atlasManager.GetElementWithName(spriteName).sourcePixelSize.x;
	}

	public static IconSymbolData SymbolDataFromCreature(AbstractCreature creature)
	{
		if (creature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
		{
			return new IconSymbolData(creature.creatureTemplate.type, AbstractPhysicalObject.AbstractObjectType.Creature, (creature.state as PlayerState).playerNumber);
		}
		if (creature.creatureTemplate.type == CreatureTemplate.Type.Centipede)
		{
			float num = Centipede.GenerateSize(creature);
			if (num < 0.255f)
			{
				return new IconSymbolData(creature.creatureTemplate.type, AbstractPhysicalObject.AbstractObjectType.Creature, 1);
			}
			if (num < 0.6f)
			{
				return new IconSymbolData(creature.creatureTemplate.type, AbstractPhysicalObject.AbstractObjectType.Creature, 2);
			}
			return new IconSymbolData(creature.creatureTemplate.type, AbstractPhysicalObject.AbstractObjectType.Creature, 3);
		}
		if (creature.creatureTemplate.IsLizard && ModManager.Watcher && creature.state is LizardState && (creature.state as LizardState).rotType != LizardState.RotType.None)
		{
			return new IconSymbolData(creature.creatureTemplate.type, AbstractPhysicalObject.AbstractObjectType.Creature, (!((creature.state as LizardState).rotType == LizardState.RotType.Full)) ? 1 : 2);
		}
		if (creature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.SkyWhale && creature.superSizeMe)
		{
			return new IconSymbolData(creature.creatureTemplate.type, AbstractPhysicalObject.AbstractObjectType.Creature, 1);
		}
		return new IconSymbolData(creature.creatureTemplate.type, AbstractPhysicalObject.AbstractObjectType.Creature, 0);
	}

	public static bool DoesCreatureEarnATrophy(CreatureTemplate.Type creature)
	{
		if (creature.Index == -1)
		{
			return false;
		}
		if (StaticWorld.GetCreatureTemplate(creature).quantified)
		{
			return false;
		}
		if (creature == CreatureTemplate.Type.VultureGrub || creature == CreatureTemplate.Type.Hazer || creature == CreatureTemplate.Type.TubeWorm || creature == CreatureTemplate.Type.SmallNeedleWorm || creature == CreatureTemplate.Type.SmallCentipede || creature == CreatureTemplate.Type.StandardGroundCreature)
		{
			return false;
		}
		if (ModManager.MMF && (creature == CreatureTemplate.Type.Deer || creature == CreatureTemplate.Type.Overseer || creature == CreatureTemplate.Type.TempleGuard || creature == CreatureTemplate.Type.GarbageWorm))
		{
			return false;
		}
		if (ModManager.MSC && !MoreSlugcatsCreatures.DoesCreatureEarnATrophy(creature))
		{
			return false;
		}
		if (ModManager.Watcher && !WatcherCreatures.DoesCreatureEarnATrophy(creature))
		{
			return false;
		}
		return true;
	}

	public static string LizardSpriteName(string defaultSpriteName, int intData)
	{
		return intData switch
		{
			2 => "Kill_ProtoLizard", 
			1 => "Kill_RotLizard", 
			_ => defaultSpriteName, 
		};
	}

	public static string SpriteNameOfCreature(IconSymbolData iconData)
	{
		if (iconData.critType.Index == -1)
		{
			return "Futile_White";
		}
		if (iconData.critType == CreatureTemplate.Type.Slugcat)
		{
			return "Kill_Slugcat";
		}
		if (iconData.critType == CreatureTemplate.Type.GreenLizard)
		{
			return LizardSpriteName("Kill_Green_Lizard", iconData.intData);
		}
		if (iconData.critType == CreatureTemplate.Type.PinkLizard || iconData.critType == CreatureTemplate.Type.BlueLizard || iconData.critType == CreatureTemplate.Type.CyanLizard || iconData.critType == CreatureTemplate.Type.RedLizard)
		{
			return LizardSpriteName("Kill_Standard_Lizard", iconData.intData);
		}
		if (iconData.critType == CreatureTemplate.Type.WhiteLizard)
		{
			return LizardSpriteName("Kill_White_Lizard", iconData.intData);
		}
		if (iconData.critType == CreatureTemplate.Type.BlackLizard)
		{
			return LizardSpriteName("Kill_Black_Lizard", iconData.intData);
		}
		if (iconData.critType == CreatureTemplate.Type.YellowLizard)
		{
			return LizardSpriteName("Kill_Yellow_Lizard", iconData.intData);
		}
		if (iconData.critType == CreatureTemplate.Type.Salamander)
		{
			return LizardSpriteName("Kill_Salamander", iconData.intData);
		}
		if (iconData.critType == CreatureTemplate.Type.Scavenger)
		{
			return "Kill_Scavenger";
		}
		if (iconData.critType == CreatureTemplate.Type.Vulture)
		{
			return "Kill_Vulture";
		}
		if (iconData.critType == CreatureTemplate.Type.KingVulture)
		{
			return "Kill_KingVulture";
		}
		if (iconData.critType == CreatureTemplate.Type.CicadaA || iconData.critType == CreatureTemplate.Type.CicadaB)
		{
			return "Kill_Cicada";
		}
		if (iconData.critType == CreatureTemplate.Type.Snail)
		{
			return "Kill_Snail";
		}
		if (iconData.critType == CreatureTemplate.Type.Centiwing)
		{
			return "Kill_Centiwing";
		}
		if (iconData.critType == CreatureTemplate.Type.SmallCentipede)
		{
			return "Kill_Centipede1";
		}
		if (iconData.critType == CreatureTemplate.Type.Centipede)
		{
			return "Kill_Centipede" + Custom.IntClamp(iconData.intData, 1, 3);
		}
		if (iconData.critType == CreatureTemplate.Type.RedCentipede)
		{
			return "Kill_Centipede3";
		}
		if (iconData.critType == CreatureTemplate.Type.BrotherLongLegs || iconData.critType == CreatureTemplate.Type.DaddyLongLegs)
		{
			return "Kill_Daddy";
		}
		if (iconData.critType == CreatureTemplate.Type.LanternMouse)
		{
			return "Kill_Mouse";
		}
		if (iconData.critType == CreatureTemplate.Type.GarbageWorm)
		{
			return "Kill_Garbageworm";
		}
		if (iconData.critType == CreatureTemplate.Type.Fly)
		{
			return "Kill_Bat";
		}
		if (iconData.critType == CreatureTemplate.Type.Leech || iconData.critType == CreatureTemplate.Type.SeaLeech)
		{
			return "Kill_Leech";
		}
		if (iconData.critType == CreatureTemplate.Type.Spider)
		{
			return "Kill_SmallSpider";
		}
		if (iconData.critType == CreatureTemplate.Type.JetFish)
		{
			return "Kill_Jetfish";
		}
		if (iconData.critType == CreatureTemplate.Type.BigEel)
		{
			return "Kill_BigEel";
		}
		if (iconData.critType == CreatureTemplate.Type.Deer)
		{
			return "Kill_RainDeer";
		}
		if (iconData.critType == CreatureTemplate.Type.TubeWorm)
		{
			return "Kill_Tubeworm";
		}
		if (iconData.critType == CreatureTemplate.Type.TentaclePlant)
		{
			return "Kill_TentaclePlant";
		}
		if (iconData.critType == CreatureTemplate.Type.PoleMimic)
		{
			return "Kill_PoleMimic";
		}
		if (iconData.critType == CreatureTemplate.Type.MirosBird)
		{
			return "Kill_MirosBird";
		}
		if (iconData.critType == CreatureTemplate.Type.Overseer)
		{
			return "Kill_Overseer";
		}
		if (iconData.critType == CreatureTemplate.Type.VultureGrub)
		{
			return "Kill_VultureGrub";
		}
		if (iconData.critType == CreatureTemplate.Type.EggBug)
		{
			return "Kill_EggBug";
		}
		if (iconData.critType == CreatureTemplate.Type.BigSpider || iconData.critType == CreatureTemplate.Type.SpitterSpider)
		{
			return "Kill_BigSpider";
		}
		if (iconData.critType == CreatureTemplate.Type.BigNeedleWorm)
		{
			return "Kill_NeedleWorm";
		}
		if (iconData.critType == CreatureTemplate.Type.SmallNeedleWorm)
		{
			return "Kill_SmallNeedleWorm";
		}
		if (iconData.critType == CreatureTemplate.Type.DropBug)
		{
			return "Kill_DropBug";
		}
		if (iconData.critType == CreatureTemplate.Type.Hazer)
		{
			return "Kill_Hazer";
		}
		string text = MoreSlugcatsCreatures.SpriteNameOfCreature(iconData);
		if (text != null)
		{
			return text;
		}
		if (ModManager.Watcher)
		{
			string text2 = WatcherCreatures.SpriteNameOfCreature(iconData);
			if (text2 != null)
			{
				return text2;
			}
		}
		return "Futile_White";
	}

	public static Color ColorOfCreature(IconSymbolData iconData)
	{
		if (iconData.critType.Index == -1)
		{
			return global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey);
		}
		if (iconData.critType == CreatureTemplate.Type.Slugcat)
		{
			return PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(iconData.intData));
		}
		if (iconData.critType == CreatureTemplate.Type.GreenLizard)
		{
			return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
		}
		if (iconData.critType == CreatureTemplate.Type.PinkLizard)
		{
			return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
		}
		if (iconData.critType == CreatureTemplate.Type.BlueLizard)
		{
			return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
		}
		if (iconData.critType == CreatureTemplate.Type.WhiteLizard)
		{
			return (StaticWorld.GetCreatureTemplate(iconData.critType).breedParameters as LizardBreedParams).standardColor;
		}
		if (iconData.critType == CreatureTemplate.Type.RedLizard)
		{
			return new Color(46f / 51f, 0.05490196f, 0.05490196f);
		}
		if (iconData.critType == CreatureTemplate.Type.BlackLizard)
		{
			return new Color(0.36862746f, 0.36862746f, 37f / 85f);
		}
		if (iconData.critType == CreatureTemplate.Type.YellowLizard || iconData.critType == CreatureTemplate.Type.SmallCentipede || iconData.critType == CreatureTemplate.Type.Centipede)
		{
			return new Color(1f, 0.6f, 0f);
		}
		if (iconData.critType == CreatureTemplate.Type.RedCentipede)
		{
			return new Color(46f / 51f, 0.05490196f, 0.05490196f);
		}
		if (iconData.critType == CreatureTemplate.Type.CyanLizard || iconData.critType == CreatureTemplate.Type.Overseer)
		{
			return new Color(0f, 0.9098039f, 46f / 51f);
		}
		if (iconData.critType == CreatureTemplate.Type.Salamander)
		{
			return new Color(14f / 15f, 0.78039217f, 76f / 85f);
		}
		if (iconData.critType == CreatureTemplate.Type.CicadaB)
		{
			return new Color(0.36862746f, 0.36862746f, 37f / 85f);
		}
		if (iconData.critType == CreatureTemplate.Type.CicadaA)
		{
			return new Color(1f, 1f, 1f);
		}
		if (iconData.critType == CreatureTemplate.Type.SpitterSpider || iconData.critType == CreatureTemplate.Type.Leech)
		{
			return new Color(58f / 85f, 8f / 51f, 0.11764706f);
		}
		if (iconData.critType == CreatureTemplate.Type.SeaLeech || iconData.critType == CreatureTemplate.Type.TubeWorm)
		{
			return new Color(0.05f, 0.3f, 0.7f);
		}
		if (iconData.critType == CreatureTemplate.Type.Centiwing)
		{
			return new Color(0.05490196f, 0.69803923f, 0.23529412f);
		}
		if (iconData.critType == CreatureTemplate.Type.BrotherLongLegs)
		{
			return new Color(0.45490196f, 0.5254902f, 26f / 85f);
		}
		if (iconData.critType == CreatureTemplate.Type.DaddyLongLegs)
		{
			return new Color(0f, 0f, 1f);
		}
		if (iconData.critType == CreatureTemplate.Type.VultureGrub)
		{
			return new Color(0.83137256f, 0.7921569f, 37f / 85f);
		}
		if (iconData.critType == CreatureTemplate.Type.EggBug)
		{
			return new Color(0f, 1f, 0.47058824f);
		}
		if (iconData.critType == CreatureTemplate.Type.BigNeedleWorm || iconData.critType == CreatureTemplate.Type.SmallNeedleWorm)
		{
			return new Color(1f, 0.59607846f, 0.59607846f);
		}
		if (iconData.critType == CreatureTemplate.Type.Hazer)
		{
			return new Color(18f / 85f, 0.7921569f, 33f / 85f);
		}
		Color? color = MoreSlugcatsCreatures.ColorOfCreature(iconData);
		if (color.HasValue)
		{
			return color.Value;
		}
		if (ModManager.Watcher)
		{
			Color? color2 = WatcherCreatures.ColorOfCreature(iconData);
			if (color2.HasValue)
			{
				return color2.Value;
			}
		}
		return global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey);
	}
}
