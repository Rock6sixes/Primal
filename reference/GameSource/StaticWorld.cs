using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public static class StaticWorld
{
	public static CreatureTemplate[] creatureTemplates;

	public static CreatureTemplate[] preBakedPathingCreatures;

	public static CreatureTemplate[] quantifiedCreatures;

	public static int numberOfRoomAttractions;

	public static void InitYeek(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate scavengerTemplate)
	{
		if (ModManager.DLCShared)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 2f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 50f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 120f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 150f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 15f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 4f, PathCost.Legality.Unwanted));
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.Yeek, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 0.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 0.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.75f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = scavengerTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 2100f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].wormgrassTilesIgnored = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Grab/Drop";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Jump";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Run";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
		}
	}

	public static void InitJungleLeech(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate leechTemplate)
	{
		if (ModManager.DLCShared)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 50f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 50f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.JungleLeech, leechTemplate, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardAdapted = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 0.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 0.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = leechTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 0.05f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Attach";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Release";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
		}
	}

	public static void InitMotherSpider(List<CreatureTemplate> tempCreatureTemplates)
	{
		if (ModManager.DLCShared)
		{
			List<TileTypeResistance> tileResistances = new List<TileTypeResistance>();
			List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>();
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.MotherSpider, tempCreatureTemplates[tempCreatureTemplates.Count - 2], tileResistances, connectionResistances, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 1.1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 0.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 300f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 6;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Pounce";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Grab";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Birth";
		}
	}

	public static void InitMirosVulture(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate vultureTemplate)
	{
		if (ModManager.DLCShared)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SkyHighway, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 10f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.MirosVulture, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardAdapted = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 20f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 10;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canFly = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = vultureTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 1.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 8f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 5;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 7000f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.4f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterVision = 0.8f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throughSurfaceVision = 0.8f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].hibernateOffScreen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0.7f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 15;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 0.25f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Fly";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Snip";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Laser";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => (aimap.getTerrainProximity(pos) < 2) ? 1 : 0;
		}
	}

	public static void InitAquapede(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate jetfishTemplate, CreatureTemplate centipedeTemplate, CreatureTemplate blueLizardTemplate)
	{
		if (ModManager.DLCShared)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Unwanted));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Unwanted));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 10f, PathCost.Legality.Unwanted));
			list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 100f, PathCost.Legality.Unwanted));
			list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 100f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DoubleReachUp, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 1.1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 1.1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 2f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.AquaCenti, centipedeTemplate, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 0.9f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 0.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].quickDeath = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 150;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = jetfishTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doubleReachUpConnectionParams = blueLizardTemplate.doubleReachUpConnectionParams;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 1900f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterVision = 2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throughSurfaceVision = 0.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.95f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 0.35f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].lungCapacity = 9900f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].damageRestistances[5, 0] = 102f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].damageRestistances[5, 1] = 102f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardAdapted = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Swap Heads";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Grab/Shock";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Release";
		}
	}

	public static void InitLizardsMSC(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate lizardTemplate, CreatureTemplate greenLizardTemplate, CreatureTemplate pinkLizardTemplate, CreatureTemplate blueLizardTemplate, CreatureTemplate creatureTemplateSalamander)
	{
		if (ModManager.DLCShared)
		{
			tempCreatureTemplates.Add(LizardBreeds.BreedTemplate(DLCSharedEnums.CreatureTemplateType.SpitLizard, lizardTemplate, null, null, greenLizardTemplate));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardAdapted = true;
			tempCreatureTemplates.Add(LizardBreeds.BreedTemplate(DLCSharedEnums.CreatureTemplateType.EelLizard, lizardTemplate, creatureTemplateSalamander));
			tempCreatureTemplates.Add(LizardBreeds.BreedTemplate(DLCSharedEnums.CreatureTemplateType.ZoopLizard, lizardTemplate, pinkLizardTemplate, blueLizardTemplate, greenLizardTemplate));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
		}
		if (ModManager.MSC)
		{
			tempCreatureTemplates.Add(LizardBreeds.BreedTemplate(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, lizardTemplate, pinkLizardTemplate, null, null));
		}
	}

	public static void InitLongLegsMSC(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate daddyTemplate)
	{
		List<TileTypeResistance> list = new List<TileTypeResistance>();
		List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
		if (ModManager.DLCShared)
		{
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, daddyTemplate, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardAdapted = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 400f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 4f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = daddyTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 10;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutColor = new Color(0.3f, 0f, 1f);
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Cannonball";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Consume";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Release";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].isTooCloseToTerrain = delegate(AImap aimap, IntVector2 pos)
			{
				if (aimap.room.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					return 2;
				}
				int terrainProximity = aimap.getTerrainProximity(pos);
				return (terrainProximity < 2 || terrainProximity > 11) ? 1 : 0;
			};
			list.Clear();
			list2.Clear();
		}
		if (ModManager.MSC)
		{
			tempCreatureTemplates.Add(new CreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, daddyTemplate, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 50f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = daddyTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 5;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].saveCreature = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutColor = new Color(0.57255f, 0.11373f, 0.22745f);
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Cannonball";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Consume";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Release";
			list.Clear();
			list2.Clear();
		}
		if (ModManager.DLCShared)
		{
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 100f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 100f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 100f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.Inspector, daddyTemplate, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = daddyTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 3.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 130f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Emotional State";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Grab";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Throw";
		}
	}

	public static void InitScavengersMSC(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate scavengerTemplate)
	{
		List<TileTypeResistance> list = new List<TileTypeResistance>();
		List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
		if (ModManager.DLCShared)
		{
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.ScavengerElite, scavengerTemplate, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 2.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 1.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.75f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 1.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = scavengerTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 1200f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].hibernateOffScreen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamBetweenRoomsChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamInRoomChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityID = CreatureCommunities.CommunityID.Scavengers;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0.9f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesRegionTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesCreatureHoles = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Jump";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Pick Up";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Throw";
			list.Clear();
			list2.Clear();
		}
		if (ModManager.MSC)
		{
			tempCreatureTemplates.Add(new CreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, tempCreatureTemplates[tempCreatureTemplates.Count - 1], list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 4f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 1.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 4f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.75f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 1.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = scavengerTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 1200f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].hibernateOffScreen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamBetweenRoomsChance = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamInRoomChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityID = CreatureCommunities.CommunityID.Scavengers;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesRegionTransportation = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesCreatureHoles = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doesNotUseDens = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].saveCreature = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Point";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Pick Up";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Throw";
		}
	}

	public static void InitBigJellyfish(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate jetfishTemplate, CreatureTemplate blueLizardTemplate)
	{
		if (ModManager.DLCShared)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Unallowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Unallowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 10f, PathCost.Legality.Unallowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 100f, PathCost.Legality.Unallowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 100f, PathCost.Legality.Unallowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Unallowed));
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.BigJelly, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 0.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].quickDeath = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 0;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 150000;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 3;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = jetfishTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doubleReachUpConnectionParams = blueLizardTemplate.doubleReachUpConnectionParams;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 1900f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterVision = 20f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throughSurfaceVision = 0.1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.25f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0.7f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 0.35f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 3;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].lungCapacity = 9900f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].damageRestistances[5, 0] = 102f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].damageRestistances[5, 1] = 102f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].wormGrassImmune = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractImmobile = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].forbidStandardShortcutEntry = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardAdapted = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Ascend";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Descend";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Moo!";
		}
	}

	public static void InitStowaway(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate flyTemplate)
	{
		if (ModManager.DLCShared)
		{
			List<TileTypeResistance> tileResistances = new List<TileTypeResistance>();
			List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>();
			tempCreatureTemplates.Add(new CreatureTemplate(DLCSharedEnums.CreatureTemplateType.StowawayBug, null, tileResistances, connectionResistances, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 0.8f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 160;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canFly = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 0.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 3;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = flyTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].forbidStandardShortcutEntry = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityID = CreatureCommunities.CommunityID.None;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].lungCapacity = 200f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 0;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].saveCreature = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doesNotUseDens = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractImmobile = true;
		}
	}

	public static void InitFireBug(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate whiteLizardTemplate)
	{
		if (ModManager.MSC)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 1.2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 2f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.FireBug, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 1.4f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 1.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 2.8f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 0.4f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = whiteLizardTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 1200f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 0.1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterPathingResistance = 40f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 2;
		}
	}

	public static void InitSlugpup(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate scavengerTemplate)
	{
		if (ModManager.MSC)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 20f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 2.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 4f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 20f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.5f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = scavengerTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterPathingResistance = 2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].ghostSedationImmune = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].saveCreature = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].interestInOtherAncestorsCatches = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].interestInOtherCreaturesCatches = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].name = "Slugpup";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 3;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Jump";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Pick up / Eat";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Throw";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 800f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].saveCreature = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
		}
	}

	public static void InitStaticWorldRelationshipsMSC()
	{
		if (ModManager.MSC)
		{
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.1f));
			EstablishRelationship(CreatureTemplate.Type.Scavenger, MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
			EstablishRelationship(CreatureTemplate.Type.Leech, MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, DLCSharedEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.CicadaB, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.SeaLeech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, DLCSharedEnums.CreatureTemplateType.JungleLeech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.Centiwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.Overseer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.TempleGuard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.BrotherLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, DLCSharedEnums.CreatureTemplateType.Inspector, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, DLCSharedEnums.CreatureTemplateType.AquaCenti, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, DLCSharedEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.5f));
			EstablishRelationship(CreatureTemplate.Type.EggBug, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.5f));
			EstablishRelationship(CreatureTemplate.Type.Scavenger, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ScavengerElite, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));
			EstablishRelationship(CreatureTemplate.Type.MirosBird, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(CreatureTemplate.Type.Snail, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.Centiwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.6f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.SpitterSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
			EstablishRelationship(CreatureTemplate.Type.LizardTemplate, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(CreatureTemplate.Type.Fly, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(CreatureTemplate.Type.LanternMouse, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
			EstablishRelationship(CreatureTemplate.Type.Vulture, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
			EstablishRelationship(CreatureTemplate.Type.CicadaA, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
			EstablishRelationship(CreatureTemplate.Type.CicadaB, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
			EstablishRelationship(CreatureTemplate.Type.JetFish, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
			EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(CreatureTemplate.Type.MirosBird, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.6f));
			EstablishRelationship(CreatureTemplate.Type.Scavenger, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
			EstablishRelationship(CreatureTemplate.Type.BigSpider, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.BigSpider, MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
			EstablishRelationship(CreatureTemplate.Type.DropBug, MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		}
		if (ModManager.DLCShared)
		{
			EstablishRelationship(CreatureTemplate.Type.Slugcat, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(CreatureTemplate.Type.LanternMouse, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(CreatureTemplate.Type.LizardTemplate, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, DLCSharedEnums.CreatureTemplateType.ScavengerElite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.01f));
			EstablishRelationship(CreatureTemplate.Type.Scavenger, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ScavengerElite, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
			EstablishRelationship(CreatureTemplate.Type.CicadaA, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(CreatureTemplate.Type.JetFish, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
			EstablishRelationship(CreatureTemplate.Type.BigSpider, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(CreatureTemplate.Type.DropBug, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f));
			EstablishRelationship(CreatureTemplate.Type.MirosBird, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.3f));
			EstablishRelationship(CreatureTemplate.Type.KingVulture, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(CreatureTemplate.Type.Vulture, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0f));
			EstablishRelationship(CreatureTemplate.Type.SmallCentipede, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
			EstablishRelationship(CreatureTemplate.Type.Centipede, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
			EstablishRelationship(CreatureTemplate.Type.RedCentipede, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
			EstablishRelationship(CreatureTemplate.Type.TentaclePlant, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, DLCSharedEnums.CreatureTemplateType.Inspector, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(CreatureTemplate.Type.Scavenger, DLCSharedEnums.CreatureTemplateType.Inspector, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, CreatureTemplate.Type.TempleGuard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.Leech, DLCSharedEnums.CreatureTemplateType.Inspector, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.Leech, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.Leech, DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(CreatureTemplate.Type.Leech, DLCSharedEnums.CreatureTemplateType.EelLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
			EstablishRelationship(CreatureTemplate.Type.BlueLizard, DLCSharedEnums.CreatureTemplateType.ZoopLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(CreatureTemplate.Type.BlueLizard, DLCSharedEnums.CreatureTemplateType.EelLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.25f));
			EstablishRelationship(CreatureTemplate.Type.JetFish, DLCSharedEnums.CreatureTemplateType.EelLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.CicadaB, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.LanternMouse, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.Scavenger, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ScavengerElite, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.1f));
			EstablishRelationship(CreatureTemplate.Type.LizardTemplate, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(CreatureTemplate.Type.DropBug, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(CreatureTemplate.Type.Vulture, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(CreatureTemplate.Type.BigSpider, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.AquaCenti, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(CreatureTemplate.Type.JetFish, DLCSharedEnums.CreatureTemplateType.AquaCenti, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			for (int i = 0; i < creatureTemplates.Length; i++)
			{
				EstablishRelationship(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(i)), DLCSharedEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
			}
			EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, DLCSharedEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.BrotherLongLegs, DLCSharedEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, DLCSharedEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.BigEel, DLCSharedEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.BigJelly, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.BigJelly, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.BrotherLongLegs, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.Deer, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.Vulture, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.KingVulture, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.TentaclePlant, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.PoleMimic, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.BrotherLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.LizardTemplate, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.Centipede, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.BigSpider, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.BigEel, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.LanternMouse, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.CicadaA, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.JetFish, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.Scavenger, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.EggBug, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.MirosBird, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.Snail, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(CreatureTemplate.Type.Spider, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.TempleGuard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, DLCSharedEnums.CreatureTemplateType.EelLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
			EstablishRelationship(CreatureTemplate.Type.RedLizard, DLCSharedEnums.CreatureTemplateType.ZoopLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
			EstablishRelationship(CreatureTemplate.Type.GreenLizard, DLCSharedEnums.CreatureTemplateType.ZoopLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
			EstablishRelationship(CreatureTemplate.Type.PinkLizard, DLCSharedEnums.CreatureTemplateType.ZoopLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
			EstablishRelationship(CreatureTemplate.Type.YellowLizard, DLCSharedEnums.CreatureTemplateType.ZoopLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, CreatureTemplate.Type.PinkLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.35f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, CreatureTemplate.Type.BlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.65f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, CreatureTemplate.Type.RedLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
			EstablishRelationship(CreatureTemplate.Type.BigSpider, DLCSharedEnums.CreatureTemplateType.ZoopLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.6f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, DLCSharedEnums.CreatureTemplateType.SpitLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, DLCSharedEnums.CreatureTemplateType.ZoopLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, CreatureTemplate.Type.PinkLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.2f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.8f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, CreatureTemplate.Type.WhiteLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.05f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, CreatureTemplate.Type.BlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.CicadaA, DLCSharedEnums.CreatureTemplateType.SpitLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.1f));
			EstablishRelationship(CreatureTemplate.Type.Vulture, DLCSharedEnums.CreatureTemplateType.SpitLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(CreatureTemplate.Type.PinkLizard, DLCSharedEnums.CreatureTemplateType.SpitLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.15f));
			EstablishRelationship(CreatureTemplate.Type.BlueLizard, DLCSharedEnums.CreatureTemplateType.SpitLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.25f));
			EstablishRelationship(CreatureTemplate.Type.WhiteLizard, DLCSharedEnums.CreatureTemplateType.SpitLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.05f));
			EstablishRelationship(CreatureTemplate.Type.KingVulture, DLCSharedEnums.CreatureTemplateType.SpitLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.55f));
			EstablishRelationship(CreatureTemplate.Type.BigSpider, DLCSharedEnums.CreatureTemplateType.SpitLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.6f));
		}
	}

	public static void InitBarnacle(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate blueLizardTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 3f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1.5f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 3f, PathCost.Legality.Unwanted));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 1.5f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 20f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 1.5f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.Barnacle, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 0.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 0.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.05f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 4f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = blueLizardTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 0;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 300f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterVision = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throughSurfaceVision = 0.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 0.25f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterPathingResistance = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Flee";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Call";
		}
	}

	public static void InitDrillCrab(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate deerTemplate)
	{
		if (!ModManager.Watcher)
		{
			return;
		}
		List<TileTypeResistance> list = new List<TileTypeResistance>();
		List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
		list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 1000f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 1000f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 2f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.DrillCrab, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0f));
		tempCreatureTemplates.Add(creatureTemplate);
		creatureTemplate.baseDamageResistance = 2.5f;
		creatureTemplate.baseStunResistance = 1.5f;
		creatureTemplate.abstractedLaziness = 40;
		creatureTemplate.roamBetweenRoomsChance = 0f;
		creatureTemplate.AI = true;
		creatureTemplate.requireAImap = true;
		creatureTemplate.canFly = false;
		creatureTemplate.preBakedPathingAncestor = deerTemplate;
		creatureTemplate.offScreenSpeed = 1f;
		creatureTemplate.bodySize = 5f;
		creatureTemplate.visualRadius = 600f;
		creatureTemplate.movementBasedVision = 0.3f;
		creatureTemplate.dangerousToPlayer = 0.35f;
		creatureTemplate.communityID = CreatureCommunities.CommunityID.None;
		creatureTemplate.hibernateOffScreen = true;
		creatureTemplate.quickDeath = true;
		creatureTemplate.wormGrassImmune = true;
		creatureTemplate.jumpAction = "Move";
		creatureTemplate.pickupAction = "Bow";
		creatureTemplate.throwAction = "Cannonball";
		creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		creatureTemplate.meatPoints = 6;
		creatureTemplate.isTooCloseToTerrain = delegate(AImap aimap, IntVector2 pos)
		{
			int num = 8;
			int num2 = 2;
			int num3 = 1;
			if (aimap.getTerrainProximity(pos) < num2)
			{
				return 1;
			}
			bool flag = false;
			for (int i = -num3; i <= num3; i++)
			{
				if (pos.x + i >= 0 && pos.x + i < aimap.room.TileWidth && aimap.getAItile(pos.x + i, pos.y).floorAltitude <= num)
				{
					flag = true;
					break;
				}
			}
			return (!flag) ? 1 : 0;
		};
		InitTowerCrab(tempCreatureTemplates, creatureTemplate);
	}

	public static void InitTowerCrab(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate drillCrabTemplate)
	{
		if (!ModManager.Watcher)
		{
			return;
		}
		List<TileTypeResistance> tileResistances = new List<TileTypeResistance>();
		List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>();
		CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.TowerCrab, drillCrabTemplate, tileResistances, connectionResistances, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		tempCreatureTemplates.Add(creatureTemplate);
		creatureTemplate.dangerousToPlayer = 0f;
		creatureTemplate.baseDamageResistance = 3.5f;
		creatureTemplate.offScreenSpeed = 3f;
		creatureTemplate.isTooCloseToTerrain = delegate(AImap aimap, IntVector2 pos)
		{
			int num = 18;
			int num2 = 3;
			int num3 = 3;
			if (aimap.getTerrainProximity(pos) < num2)
			{
				return 1;
			}
			bool flag = false;
			for (int i = -num3; i <= num3; i++)
			{
				if (pos.x + i >= 0 && pos.x + i < aimap.room.TileWidth && aimap.getAItile(pos.x + i, pos.y).floorAltitude <= num)
				{
					flag = true;
					break;
				}
			}
			return (!flag) ? 1 : 0;
		};
	}

	public static void InitSandGrub(List<CreatureTemplate> tempCreatureTemplates)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> tileResistances = new List<TileTypeResistance>();
			List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>();
			CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.SandGrub, null, tileResistances, connectionResistances, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			tempCreatureTemplates.Add(creatureTemplate);
			creatureTemplate.baseDamageResistance = 0.5f;
			creatureTemplate.baseStunResistance = 0.5f;
			creatureTemplate.instantDeathDamageLimit = 1f;
			creatureTemplate.AI = true;
			creatureTemplate.requireAImap = false;
			creatureTemplate.doPreBakedPathing = false;
			creatureTemplate.offScreenSpeed = 0f;
			creatureTemplate.bodySize = 0.4f;
			creatureTemplate.visualRadius = 300f;
			creatureTemplate.movementBasedVision = 0f;
			creatureTemplate.dangerousToPlayer = 0f;
			creatureTemplate.communityID = CreatureCommunities.CommunityID.None;
			creatureTemplate.countsAsAKill = 0;
			creatureTemplate.quickDeath = true;
			creatureTemplate.grasps = 1;
			creatureTemplate.smallCreature = true;
			creatureTemplate.jumpAction = "Burrow";
			creatureTemplate.pickupAction = "Grab";
			creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
			InitBigSandGrub(tempCreatureTemplates, creatureTemplate);
		}
	}

	public static void InitBigSandGrub(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate sandWormTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> tileResistances = new List<TileTypeResistance>();
			List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>();
			CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.BigSandGrub, sandWormTemplate, tileResistances, connectionResistances, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			tempCreatureTemplates.Add(creatureTemplate);
			creatureTemplate.baseDamageResistance = 1.5f;
			creatureTemplate.baseStunResistance = 1.5f;
			creatureTemplate.instantDeathDamageLimit = 1f;
			creatureTemplate.AI = true;
			creatureTemplate.requireAImap = true;
			creatureTemplate.doPreBakedPathing = false;
			creatureTemplate.offScreenSpeed = 0f;
			creatureTemplate.bodySize = 1.2f;
			creatureTemplate.visualRadius = 300f;
			creatureTemplate.movementBasedVision = 0f;
			creatureTemplate.dangerousToPlayer = 0.8f;
			creatureTemplate.communityID = CreatureCommunities.CommunityID.None;
			creatureTemplate.countsAsAKill = 1;
			creatureTemplate.quickDeath = true;
			creatureTemplate.grasps = 1;
			creatureTemplate.jumpAction = "Burrow";
			creatureTemplate.pickupAction = "Grab";
			creatureTemplate.throwAction = "Hiss";
			creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		}
	}

	public static void InitRattler(List<CreatureTemplate> tempCreatureTemplates)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> tileResistances = new List<TileTypeResistance>();
			List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>();
			CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.Rattler, null, tileResistances, connectionResistances, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			tempCreatureTemplates.Add(creatureTemplate);
			creatureTemplate.baseDamageResistance = 3.5f;
			creatureTemplate.baseStunResistance = 0.5f;
			creatureTemplate.instantDeathDamageLimit = 1f;
			creatureTemplate.AI = true;
			creatureTemplate.requireAImap = true;
			creatureTemplate.doPreBakedPathing = false;
			creatureTemplate.offScreenSpeed = 0f;
			creatureTemplate.bodySize = 1f;
			creatureTemplate.visualRadius = 600f;
			creatureTemplate.movementBasedVision = 0f;
			creatureTemplate.dangerousToPlayer = 0f;
			creatureTemplate.communityID = CreatureCommunities.CommunityID.None;
			creatureTemplate.countsAsAKill = 1;
			creatureTemplate.grasps = 4;
			creatureTemplate.jumpAction = "Cycle";
			creatureTemplate.pickupAction = "Grab";
			creatureTemplate.throwAction = "Hiss";
			creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		}
	}

	public static void InitBigMoth(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate vultureTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SkyHighway, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
			CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.BigMoth, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			tempCreatureTemplates.Add(creatureTemplate);
			creatureTemplate.abstractedLaziness = 100;
			creatureTemplate.baseDamageResistance = 4.4f;
			creatureTemplate.baseStunResistance = 1f;
			creatureTemplate.instantDeathDamageLimit = 0.9f;
			creatureTemplate.AI = true;
			creatureTemplate.canFly = true;
			creatureTemplate.requireAImap = true;
			creatureTemplate.doPreBakedPathing = false;
			creatureTemplate.preBakedPathingAncestor = vultureTemplate;
			creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			creatureTemplate.communityInfluence = 0.5f;
			creatureTemplate.socialMemory = true;
			creatureTemplate.dangerousToPlayer = 0.5f;
			creatureTemplate.offScreenSpeed = 0.8f;
			creatureTemplate.bodySize = 5f;
			creatureTemplate.grasps = 0;
			creatureTemplate.visualRadius = 1100f;
			creatureTemplate.movementBasedVision = 0.6f;
			creatureTemplate.hibernateOffScreen = true;
			creatureTemplate.forbidStandardShortcutEntry = true;
			creatureTemplate.grasps = 1;
			creatureTemplate.jumpAction = "Fly";
			creatureTemplate.pickupAction = "Pick Up";
			creatureTemplate.throwAction = "Consume";
			creatureTemplate.isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => (aimap.getTerrainProximity(pos) < 2) ? 1 : 0;
		}
	}

	public static void InitSmallMoth(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate bigMothTemplate, CreatureTemplate batTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SkyHighway, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
			CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.SmallMoth, bigMothTemplate, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			tempCreatureTemplates.Add(creatureTemplate);
			creatureTemplate.requireAImap = true;
			creatureTemplate.doPreBakedPathing = false;
			creatureTemplate.preBakedPathingAncestor = batTemplate;
			creatureTemplate.baseDamageResistance = 0.3f;
			creatureTemplate.baseStunResistance = 0.3f;
			creatureTemplate.instantDeathDamageLimit = 1f;
			creatureTemplate.forbidStandardShortcutEntry = false;
			creatureTemplate.hibernateOffScreen = false;
			creatureTemplate.stowFoodInDen = true;
			creatureTemplate.dangerousToPlayer = 0f;
			creatureTemplate.jumpAction = "Fly";
			creatureTemplate.pickupAction = "Pick Up";
			creatureTemplate.throwAction = "Consume";
		}
	}

	public static void InitBoxWorm(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate pathingAncestor)
	{
		if (!ModManager.Watcher)
		{
			return;
		}
		List<TileTypeResistance> list = new List<TileTypeResistance>();
		List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
		list.Add(new TileTypeResistance(AItile.Accessibility.Air, 2f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 0.5f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.CurvedFloor, 0.35f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 0.5f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 0.5f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Unallowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenUnallowed, 10f, PathCost.Legality.Unallowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Unallowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BigCreatureShortCutSqueeze, 1f, PathCost.Legality.IllegalConnection));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DoubleReachUp, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 1f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.BoxWorm, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.DoesntTrack, 0f));
		tempCreatureTemplates.Add(creatureTemplate);
		creatureTemplate.doubleReachUpConnectionParams = new int[3] { 8, 7, 8 };
		creatureTemplate.baseDamageResistance = 40f;
		creatureTemplate.AI = true;
		creatureTemplate.requireAImap = true;
		creatureTemplate.canFly = false;
		creatureTemplate.doPreBakedPathing = false;
		creatureTemplate.preBakedPathingAncestor = pathingAncestor;
		creatureTemplate.offScreenSpeed = 0.02f;
		creatureTemplate.visualRadius = 1000f;
		creatureTemplate.dangerousToPlayer = 0.5f;
		creatureTemplate.roamBetweenRoomsChance = 0f;
		creatureTemplate.communityID = WatcherEnums.CommunityID.BoxWorms;
		creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		creatureTemplate.jumpAction = "Discard";
		creatureTemplate.pickupAction = "Move";
		creatureTemplate.throwAction = "Spit";
		creatureTemplate.isTooCloseToTerrain = delegate(AImap aimap, IntVector2 pos)
		{
			if (aimap.room.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				return 2;
			}
			bool flag = false;
			for (int i = -3; i <= 3; i++)
			{
				if (pos.x + i >= 0 && pos.x + i < aimap.room.TileWidth && aimap.getAItile(pos.x + i, pos.y).floorAltitude <= 5)
				{
					flag = true;
					break;
				}
			}
			return (!flag || aimap.getTerrainProximity(pos) > 6) ? 1 : 0;
		};
		list.Clear();
		list2.Clear();
	}

	public static void InitFireSprite(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate flyTemplate)
	{
		if (!ModManager.Watcher)
		{
			return;
		}
		List<TileTypeResistance> list = new List<TileTypeResistance>();
		List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
		list.Add(new TileTypeResistance(AItile.Accessibility.Air, 0.1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 1f, PathCost.Legality.Unallowed));
		CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.FireSprite, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.DoesntTrack, 0f));
		tempCreatureTemplates.Add(creatureTemplate);
		creatureTemplate.AI = true;
		creatureTemplate.requireAImap = true;
		creatureTemplate.canFly = true;
		creatureTemplate.doPreBakedPathing = false;
		creatureTemplate.offScreenSpeed = 0.1f;
		creatureTemplate.bodySize = 0.1f;
		creatureTemplate.grasps = 1;
		creatureTemplate.smallCreature = true;
		creatureTemplate.preBakedPathingAncestor = flyTemplate;
		creatureTemplate.visualRadius = 800f;
		creatureTemplate.socialMemory = true;
		creatureTemplate.usesNPCTransportation = true;
		creatureTemplate.usesRegionTransportation = true;
		creatureTemplate.baseDamageResistance = 0f;
		creatureTemplate.baseStunResistance = 0.2f;
		creatureTemplate.communityID = WatcherEnums.CommunityID.BoxWorms;
		creatureTemplate.instantDeathDamageLimit = 1f;
		creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		creatureTemplate.jumpAction = "Eat";
		creatureTemplate.pickupAction = "Burrow";
		creatureTemplate.isTooCloseToTerrain = delegate(AImap aimap, IntVector2 pos)
		{
			if (aimap.room.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				return 2;
			}
			return (aimap.getTerrainProximity(pos) > 20) ? 1 : 0;
		};
		list.Clear();
		list2.Clear();
	}

	public static void InitSkyWhale(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate skyWhaleTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 100f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 100f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10000f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 10000f, PathCost.Legality.Allowed));
			CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.SkyWhale, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			tempCreatureTemplates.Add(creatureTemplate);
			creatureTemplate.baseDamageResistance = 50f;
			creatureTemplate.baseStunResistance = 8f;
			creatureTemplate.AI = true;
			creatureTemplate.canFly = true;
			creatureTemplate.requireAImap = true;
			creatureTemplate.doPreBakedPathing = false;
			creatureTemplate.preBakedPathingAncestor = skyWhaleTemplate;
			creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
			creatureTemplate.offScreenSpeed = 0.8f;
			creatureTemplate.bodySize = 11f;
			creatureTemplate.grasps = 0;
			creatureTemplate.visualRadius = 1100f;
			creatureTemplate.movementBasedVision = 0f;
			creatureTemplate.hibernateOffScreen = true;
			creatureTemplate.forbidStandardShortcutEntry = true;
			creatureTemplate.jumpAction = "Call";
			creatureTemplate.isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => (aimap.getTerrainProximity(pos) < 4) ? 1 : 0;
		}
	}

	public static void InitScavengerTemplar(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate scavengerTemplarTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.ScavengerTemplar, scavengerTemplarTemplate, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 2.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 1.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.75f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 1.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = scavengerTemplarTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 1200f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].hibernateOffScreen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamBetweenRoomsChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamInRoomChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityID = CreatureCommunities.CommunityID.Scavengers;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0.9f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesRegionTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesCreatureHoles = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Jump";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Pick Up";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Throw";
			list.Clear();
			list2.Clear();
		}
	}

	public static void InitScavengerDisciple(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate scavengerTemplarTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.ScavengerDisciple, scavengerTemplarTemplate, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 2.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 1.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1.6f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.75f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 1.2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = scavengerTemplarTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 2500f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterVision = 2500f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throughSurfaceVision = 0.8f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].hibernateOffScreen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamBetweenRoomsChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamInRoomChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityID = CreatureCommunities.CommunityID.Scavengers;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0.9f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesRegionTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesCreatureHoles = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Jump";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Pick Up";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Throw";
			list.Clear();
			list2.Clear();
		}
	}

	public static void InitLoach(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate ancestorTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 100f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
			CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.Loach, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			creatureTemplate.baseDamageResistance = 400f;
			creatureTemplate.baseStunResistance = 8f;
			creatureTemplate.AI = true;
			creatureTemplate.canFly = false;
			creatureTemplate.requireAImap = true;
			creatureTemplate.doPreBakedPathing = false;
			creatureTemplate.preBakedPathingAncestor = ancestorTemplate;
			creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
			creatureTemplate.dangerousToPlayer = 0.5f;
			creatureTemplate.offScreenSpeed = 0.8f;
			creatureTemplate.bodySize = 11f;
			creatureTemplate.grasps = 0;
			creatureTemplate.visualRadius = 1100f;
			creatureTemplate.movementBasedVision = 0f;
			creatureTemplate.hibernateOffScreen = true;
			creatureTemplate.forbidStandardShortcutEntry = true;
			creatureTemplate.canAutoAbstractPath = false;
			creatureTemplate.jumpAction = "Lunge";
			creatureTemplate.pickupAction = "Grab";
			creatureTemplate.throwAction = "Howl";
			creatureTemplate.isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => (aimap.getTerrainProximity(pos) < 3 || aimap.getTerrainProximity(pos) > 16 || aimap.getAItile(pos).smoothedFloorAltitude > 16) ? 1 : 0;
			tempCreatureTemplates.Add(creatureTemplate);
		}
	}

	public static void InitRotLoach(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate loach)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> tileResistances = new List<TileTypeResistance>();
			List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>();
			CreatureTemplate creatureTemplate = new CreatureTemplate(WatcherEnums.CreatureTemplateType.RotLoach, loach, tileResistances, connectionResistances, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
			tempCreatureTemplates.Add(creatureTemplate);
			creatureTemplate.baseDamageResistance = 800f;
			creatureTemplate.dangerousToPlayer = 0.8f;
		}
	}

	public static void InitLizardsWatcher(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate lizardTemplate, CreatureTemplate greenLizardTemplate, CreatureTemplate pinkLizardTemplate, CreatureTemplate blueLizardTemplate, CreatureTemplate creatureTemplateSalamander)
	{
		if (ModManager.Watcher)
		{
			tempCreatureTemplates.Add(LizardBreeds.BreedTemplate(WatcherEnums.CreatureTemplateType.BlizzardLizard, lizardTemplate, null, blueLizardTemplate, null));
			tempCreatureTemplates.Add(LizardBreeds.BreedTemplate(WatcherEnums.CreatureTemplateType.BasiliskLizard, lizardTemplate, pinkLizardTemplate, null, null));
			tempCreatureTemplates.Add(LizardBreeds.BreedTemplate(WatcherEnums.CreatureTemplateType.IndigoLizard, lizardTemplate, pinkLizardTemplate, null, greenLizardTemplate));
			tempCreatureTemplates.Add(LizardBreeds.BreedTemplate(WatcherEnums.CreatureTemplateType.PeachLizard, lizardTemplate, pinkLizardTemplate, null, null));
		}
	}

	public static void InitRat(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate ratTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 1f, PathCost.Legality.Unallowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Unallowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 50f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.Rat, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.75f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 0;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 0.7f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = ratTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 750f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterVision = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throughSurfaceVision = 0.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].hibernateOffScreen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamBetweenRoomsChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamInRoomChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesRegionTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesCreatureHoles = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Flee";
			list.Clear();
			list2.Clear();
		}
	}

	public static void InitFrog(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate frogTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.CurvedFloor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 1f, PathCost.Legality.Unallowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Unallowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 50f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.Frog, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.75f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 0.7f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = frogTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 2500f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterVision = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throughSurfaceVision = 0.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].hibernateOffScreen = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamBetweenRoomsChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamInRoomChance = -1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesRegionTransportation = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesCreatureHoles = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Jump";
			list.Clear();
			list2.Clear();
		}
	}

	public static void InitRippleSpider(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate Template)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.CurvedFloor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 5f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 5f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 20f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 20f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 4f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.RippleSpider, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.5f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamBetweenRoomsChance = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].roamInRoomChance = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = Template;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterPathingResistance = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canFly = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].socialMemory = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].interestInOtherAncestorsCatches = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].interestInOtherCreaturesCatches = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].name = "RippleSpider";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 3;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "Jump";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Pick Up";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throwAction = "Throw";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 800f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].saveCreature = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].ghostSedationImmune = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].wormGrassImmune = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].daddyCorruptionImmune = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutColor = RainWorld.RippleColor;
		}
	}

	public static void InitTardigrade(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate whiteLizardTemplate)
	{
		if (!ModManager.Watcher)
		{
			return;
		}
		List<TileTypeResistance> list = new List<TileTypeResistance>();
		List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
		list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed));
		list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 50f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Unwanted));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 10f, PathCost.Legality.Unwanted));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 3f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 5f, PathCost.Legality.Allowed));
		list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.Tardigrade, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f)));
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 1.3f;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 2f;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1.5f;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.3f;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 0.4f;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = true;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = whiteLizardTemplate;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 0;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 300f;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 0.01f;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterPathingResistance = 40f;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = false;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 1;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
		tempCreatureTemplates[tempCreatureTemplates.Count - 1].isTooCloseToTerrain = delegate(AImap aimap, IntVector2 pos)
		{
			if (aimap.room.GetTile(pos).AnyBeam || aimap.room.GetTile(pos - new IntVector2(0, 1)).Terrain == Room.Tile.TerrainType.Floor)
			{
				return 0;
			}
			for (int i = 0; i < 8; i++)
			{
				if (aimap.room.HasAnySolid(pos + Custom.eightDirections[i]))
				{
					return 0;
				}
			}
			return 1;
		};
		list.Clear();
		list2.Clear();
	}

	public static void InitGrappleSnake(List<CreatureTemplate> tempCreatureTemplates)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BigCreatureShortCutSqueeze, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.GrappleSnake, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 200f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 200f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].damageRestistances[(int)Creature.DamageType.Explosion, 0] = 0.03f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canFly = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.02f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 5.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 300f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0.85f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityID = CreatureCommunities.CommunityID.None;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 5;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardWanderer = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].BlizzardAdapted = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 5;
		}
	}

	public static void InitMillipede(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate whiteLizardTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed));
			list.Add(new TileTypeResistance(AItile.Accessibility.Wall, 50f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 10f, PathCost.Legality.Unwanted));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 5f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.Millipede, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 3.4f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 50;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 0.4f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 2;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = whiteLizardTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 0;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 300f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].communityInfluence = 0.1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterPathingResistance = 40f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 1;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = true;
			list.Clear();
			list2.Clear();
		}
	}

	public static void InitAngler(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate jetFishTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BigCreatureShortCutSqueeze, 10f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.Angler, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].abstractedLaziness = 100;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 2f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 2.8f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = jetFishTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 0;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].stowFoodInDen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doesNotUseDens = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 450f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterVision = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].throughSurfaceVision = 0.65f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].movementBasedVision = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutSegments = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].shortcutColor = new Color(1f, 0.3f, 0f);
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].dangerousToPlayer = 0.5f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 4;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].jumpAction = "objecttype-Lantern";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].pickupAction = "Chomp";
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => (aimap.getTerrainProximity(pos) < 2 && aimap.room.GetTile(pos).Terrain != Room.Tile.TerrainType.ShortcutEntrance) ? 1 : 0;
			list.Clear();
			list2.Clear();
		}
	}

	public static void InitMothGrub(List<CreatureTemplate> tempCreatureTemplates, CreatureTemplate greenLizardTemplate)
	{
		if (ModManager.Watcher)
		{
			List<TileTypeResistance> list = new List<TileTypeResistance>();
			List<TileConnectionResistance> list2 = new List<TileConnectionResistance>();
			list.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 1f, PathCost.Legality.Allowed));
			list2.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 1f, PathCost.Legality.Allowed));
			tempCreatureTemplates.Add(new CreatureTemplate(WatcherEnums.CreatureTemplateType.MothGrub, null, list, list2, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseDamageResistance = 2.3f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].baseStunResistance = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].instantDeathDamageLimit = 1f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].AI = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].requireAImap = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].canSwim = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].doPreBakedPathing = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].offScreenSpeed = 0f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].bodySize = 0.8f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].preBakedPathingAncestor = greenLizardTemplate;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].grasps = 0;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].visualRadius = 450f;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].hibernateOffScreen = true;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].meatPoints = 5;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].usesNPCTransportation = false;
			tempCreatureTemplates[tempCreatureTemplates.Count - 1].isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => aimap.getAItile(pos).narrowSpace ? 1 : 0;
			list.Clear();
			list2.Clear();
		}
	}

	public static void InitStaticWorldRelationshipsWatcher()
	{
		if (!ModManager.Watcher)
		{
			return;
		}
		if (ModManager.DLCShared)
		{
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.2f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, WatcherEnums.CreatureTemplateType.RotLoach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.RotLoach, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, WatcherEnums.CreatureTemplateType.Tardigrade, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, WatcherEnums.CreatureTemplateType.BoxWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, WatcherEnums.CreatureTemplateType.FireSprite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.2f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, DLCSharedEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, DLCSharedEnums.CreatureTemplateType.AquaCenti, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.4f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.AquaCenti, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.StowawayBug, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, DLCSharedEnums.CreatureTemplateType.SpitLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
			EstablishRelationship(DLCSharedEnums.CreatureTemplateType.SpitLizard, WatcherEnums.CreatureTemplateType.PeachLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		}
		if (ModManager.MSC)
		{
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, WatcherEnums.CreatureTemplateType.Tardigrade, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.FireBug, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
			EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, WatcherEnums.CreatureTemplateType.DrillCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.2f));
			EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		}
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.BrotherLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.DrillCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.DrillCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.RedLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.DrillCrab, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, WatcherEnums.CreatureTemplateType.DrillCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.RedCentipede, WatcherEnums.CreatureTemplateType.DrillCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.DrillCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.DrillCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, WatcherEnums.CreatureTemplateType.DrillCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		CreatureTemplate creatureTemplate = GetCreatureTemplate(WatcherEnums.CreatureTemplateType.TowerCrab);
		for (int i = 0; i < creatureTemplates.Length; i++)
		{
			CreatureTemplate.Relationship relationship = creatureTemplate.CreatureRelationship(creatureTemplates[i]);
			if (relationship.type == CreatureTemplate.Relationship.Type.Uncomfortable || relationship.type == CreatureTemplate.Relationship.Type.Afraid)
			{
				EstablishRelationship(WatcherEnums.CreatureTemplateType.TowerCrab, new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(i)), new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			}
			CreatureTemplate.Relationship relationship2 = creatureTemplates[i].CreatureRelationship(creatureTemplate);
			if (relationship2.type == CreatureTemplate.Relationship.Type.Attacks || relationship2.type == CreatureTemplate.Relationship.Type.Eats)
			{
				EstablishRelationship(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(i)), WatcherEnums.CreatureTemplateType.TowerCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			}
		}
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SandGrub, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SandGrub, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SandGrub, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SandGrub, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SandGrub, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SandGrub, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SandGrub, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SandGrub, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Centiwing, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.RedCentipede, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Leech, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.SeaLeech, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Fly, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.SmallCentipede, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.CicadaB, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.SpitterSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, WatcherEnums.CreatureTemplateType.MothGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.75f));
		EstablishRelationship(CreatureTemplate.Type.Centiwing, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.75f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaB, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.EggBug, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Leech, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.75f));
		EstablishRelationship(CreatureTemplate.Type.SeaLeech, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.SmallCentipede, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.SpitterSpider, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.TubeWorm, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		for (int j = 0; j < creatureTemplates.Length; j++)
		{
			if (creatureTemplates[j].CreatureRelationship(WatcherEnums.CreatureTemplateType.BigSandGrub).type == CreatureTemplate.Relationship.Type.Eats)
			{
				EstablishRelationship(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(j)), WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			}
		}
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.SeaLeech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rattler, WatcherEnums.CreatureTemplateType.RotLoach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.SpitterSpider, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Spider, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Leech, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, WatcherEnums.CreatureTemplateType.Rattler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.9f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.RedLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.RedLizard, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.CyanLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.CyanLizard, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.Centiwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Centiwing, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.SpitterSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.SpitterSpider, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, WatcherEnums.CreatureTemplateType.MothGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.3f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.RedLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.RedLizard, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.CyanLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.CyanLizard, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.Centiwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Centiwing, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.SpitterSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.SpitterSpider, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, WatcherEnums.CreatureTemplateType.DrillCrab, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Fly, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, WatcherEnums.CreatureTemplateType.BigSandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.SmallCentipede, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Barnacle, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.75f));
		EstablishRelationship(CreatureTemplate.Type.EggBug, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.75f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Barnacle, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BoxWorm, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BoxWorm, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.BoxWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BoxWorm, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BoxWorm, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.DoesntTrack, 0f));
		EstablishRelationship(CreatureTemplate.Type.SmallCentipede, WatcherEnums.CreatureTemplateType.BoxWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BoxWorm, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BoxWorm, WatcherEnums.CreatureTemplateType.FireSprite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BoxWorm, WatcherEnums.CreatureTemplateType.BoxWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.BoxWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.FireSprite, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.FireSprite, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.DoesntTrack, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.FireSprite, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.FireSprite, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.FireSprite, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.FireSprite, WatcherEnums.CreatureTemplateType.BoxWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.FireSprite, WatcherEnums.CreatureTemplateType.FireSprite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.FireSprite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Barnacle, WatcherEnums.CreatureTemplateType.FireSprite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Snail, WatcherEnums.CreatureTemplateType.FireSprite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		for (int k = 0; k < creatureTemplates.Length; k++)
		{
			CreatureTemplate.Relationship relationship3 = creatureTemplates[k].CreatureRelationship(WatcherEnums.CreatureTemplateType.FireSprite);
			if ((relationship3.type == CreatureTemplate.Relationship.Type.Eats || relationship3.type == CreatureTemplate.Relationship.Type.Attacks) && GetCreatureTemplate(WatcherEnums.CreatureTemplateType.FireSprite).CreatureRelationship(creatureTemplates[k]).type == CreatureTemplate.Relationship.Type.DoesntTrack)
			{
				EstablishRelationship(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(k)), WatcherEnums.CreatureTemplateType.FireSprite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			}
			CreatureTemplate.Relationship relationship4 = creatureTemplates[k].CreatureRelationship(WatcherEnums.CreatureTemplateType.BoxWorm);
			if ((relationship4.type == CreatureTemplate.Relationship.Type.Eats || relationship4.type == CreatureTemplate.Relationship.Type.Attacks) && GetCreatureTemplate(WatcherEnums.CreatureTemplateType.BoxWorm).CreatureRelationship(creatureTemplates[k]).type == CreatureTemplate.Relationship.Type.DoesntTrack)
			{
				EstablishRelationship(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(k)), WatcherEnums.CreatureTemplateType.BoxWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			}
		}
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SkyWhale, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		for (int l = 0; l < creatureTemplates.Length; l++)
		{
			CreatureTemplate.Relationship relationship5 = creatureTemplates[l].CreatureRelationship(WatcherEnums.CreatureTemplateType.SkyWhale);
			if (relationship5.type == CreatureTemplate.Relationship.Type.Eats || relationship5.type == CreatureTemplate.Relationship.Type.Attacks)
			{
				EstablishRelationship(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(l)), WatcherEnums.CreatureTemplateType.SkyWhale, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			}
		}
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.StandardGroundCreature, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(CreatureTemplate.Type.Leech, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.KingVulture, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.EggBug, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.Centiwing, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.Fly, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.Snail, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.Spider, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.SpitterSpider, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.RedCentipede, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Loach, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.RotLoach, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.RotLoach, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.SpitterSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, WatcherEnums.CreatureTemplateType.SmallMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, WatcherEnums.CreatureTemplateType.SandGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Rat, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.RedCentipede, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.01f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.Rat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, WatcherEnums.CreatureTemplateType.SkyWhale, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, WatcherEnums.CreatureTemplateType.BigMoth, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BigMoth, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.SmallMoth, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BasiliskLizard, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(DLCSharedEnums.CreatureTemplateType.EelLizard, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.Salamander, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, WatcherEnums.CreatureTemplateType.Frog, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Leech, WatcherEnums.CreatureTemplateType.Tardigrade, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, WatcherEnums.CreatureTemplateType.Loach, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, WatcherEnums.CreatureTemplateType.BasiliskLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.Tardigrade, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.Tardigrade, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.Tardigrade, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BasiliskLizard, WatcherEnums.CreatureTemplateType.Tardigrade, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, WatcherEnums.CreatureTemplateType.Tardigrade, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.01f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Tardigrade, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.8f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.3f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Angler, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.RedCentipede, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Hazer, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.Frog, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.SmallCentipede, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, WatcherEnums.CreatureTemplateType.Angler, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, WatcherEnums.CreatureTemplateType.PeachLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, WatcherEnums.CreatureTemplateType.Barnacle, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.08f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, WatcherEnums.CreatureTemplateType.Tardigrade, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.12f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Snail, WatcherEnums.CreatureTemplateType.PeachLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, CreatureTemplate.Type.YellowLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.45f));
		EstablishRelationship(CreatureTemplate.Type.YellowLizard, WatcherEnums.CreatureTemplateType.PeachLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.45f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, WatcherEnums.CreatureTemplateType.PeachLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, WatcherEnums.CreatureTemplateType.IndigoLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.55f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.IndigoLizard, WatcherEnums.CreatureTemplateType.PeachLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, WatcherEnums.CreatureTemplateType.PeachLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.PeachLizard, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(WatcherEnums.CreatureTemplateType.BlizzardLizard, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.BlizzardLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.BasiliskLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, WatcherEnums.CreatureTemplateType.IndigoLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		for (int m = 0; m < creatureTemplates.Length; m++)
		{
			EstablishRelationship(creatureTemplates[m].type, WatcherEnums.CreatureTemplateType.RippleSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		}
	}

	static StaticWorld()
	{
	}

	public static void InitCustomTemplates()
	{
		Custom.LogImportant("Init Custom Templates");
	}

	public static void InitStaticWorld()
	{
		numberOfRoomAttractions = ExtEnum<AbstractRoom.CreatureRoomAttraction>.values.Count;
		List<CreatureTemplate> list = new List<CreatureTemplate>();
		List<TileTypeResistance> list2 = new List<TileTypeResistance>();
		List<TileConnectionResistance> list3 = new List<TileConnectionResistance>();
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 20f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 20f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate = new CreatureTemplate(CreatureTemplate.Type.StandardGroundCreature, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		list.Add(creatureTemplate);
		creatureTemplate.doPreBakedPathing = true;
		creatureTemplate.virtualCreature = true;
		creatureTemplate.canSwim = true;
		creatureTemplate.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1.2f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 3000f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 6000f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 4000f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 0.9f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.Slugcat, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].baseDamageResistance = 1f;
		list[list.Count - 1].baseStunResistance = 1f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].grasps = 2;
		list[list.Count - 1].offScreenSpeed = 0.1f;
		list[list.Count - 1].bodySize = 1f;
		list[list.Count - 1].shortcutSegments = 2;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate;
		list[list.Count - 1].meatPoints = 3;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
		list[list.Count - 1].waterPathingResistance = 2f;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].ghostSedationImmune = true;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.CurvedFloor, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DoubleReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 20f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 20f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.LizardTurn, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.LizardTemplate, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		CreatureTemplate creatureTemplate2 = list[list.Count - 1];
		creatureTemplate2.offScreenSpeed = 0.3f;
		creatureTemplate2.grasps = 1;
		creatureTemplate2.virtualCreature = true;
		creatureTemplate2.abstractedLaziness = 50;
		creatureTemplate2.AI = true;
		creatureTemplate2.requireAImap = true;
		creatureTemplate2.bodySize = 2f;
		creatureTemplate2.stowFoodInDen = true;
		creatureTemplate2.shortcutSegments = 3;
		creatureTemplate2.communityID = CreatureCommunities.CommunityID.Lizards;
		creatureTemplate2.communityInfluence = 1f;
		creatureTemplate2.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		creatureTemplate2.canSwim = true;
		creatureTemplate2.socialMemory = true;
		creatureTemplate2.usesNPCTransportation = true;
		creatureTemplate2.interestInOtherAncestorsCatches = 1.25f;
		creatureTemplate2.interestInOtherCreaturesCatches = 1.1f;
		list2.Clear();
		list3.Clear();
		CreatureTemplate creatureTemplate3 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.PinkLizard, creatureTemplate2, null, null, null);
		list.Add(creatureTemplate3);
		CreatureTemplate creatureTemplate4 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.GreenLizard, creatureTemplate2, null, null, null);
		list.Add(creatureTemplate4);
		CreatureTemplate creatureTemplate5 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.BlueLizard, creatureTemplate2, null, null, null);
		list.Add(creatureTemplate5);
		list.Add(LizardBreeds.BreedTemplate(CreatureTemplate.Type.YellowLizard, creatureTemplate2, creatureTemplate3, null, null));
		CreatureTemplate creatureTemplate6 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.WhiteLizard, creatureTemplate2, creatureTemplate3, null, null);
		list.Add(creatureTemplate6);
		list.Add(LizardBreeds.BreedTemplate(CreatureTemplate.Type.RedLizard, creatureTemplate2, creatureTemplate3, null, null));
		list.Add(LizardBreeds.BreedTemplate(CreatureTemplate.Type.BlackLizard, creatureTemplate2, creatureTemplate3, null, null));
		CreatureTemplate creatureTemplate7 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.Salamander, creatureTemplate2, creatureTemplate3, null, null);
		list.Add(creatureTemplate7);
		CreatureTemplate creatureTemplate8 = LizardBreeds.BreedTemplate(CreatureTemplate.Type.CyanLizard, creatureTemplate2, creatureTemplate3, creatureTemplate5, null);
		list.Add(creatureTemplate8);
		InitLizardsMSC(list, creatureTemplate2, creatureTemplate4, creatureTemplate3, creatureTemplate5, creatureTemplate7);
		InitLizardsWatcher(list, creatureTemplate2, creatureTemplate4, creatureTemplate3, creatureTemplate5, creatureTemplate7);
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate9 = new CreatureTemplate(CreatureTemplate.Type.Fly, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0f));
		list.Add(creatureTemplate9);
		list[list.Count - 1].baseDamageResistance = 0.15f;
		list[list.Count - 1].baseStunResistance = 0.2f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].abstractedLaziness = 100;
		list[list.Count - 1].AI = false;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canFly = true;
		list[list.Count - 1].doPreBakedPathing = true;
		list[list.Count - 1].offScreenSpeed = 0.1f;
		list[list.Count - 1].bodySize = 0.1f;
		list[list.Count - 1].quantified = true;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].smallCreature = true;
		list[list.Count - 1].visualRadius = 450f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.None;
		list[list.Count - 1].countsAsAKill = 0;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].pickupAction = "Burrow/Attach";
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate10 = new CreatureTemplate(CreatureTemplate.Type.Leech, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		list.Add(creatureTemplate10);
		list[list.Count - 1].baseDamageResistance = 0.1f;
		list[list.Count - 1].baseStunResistance = 0.1f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].abstractedLaziness = 100;
		list[list.Count - 1].AI = false;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].doPreBakedPathing = true;
		list[list.Count - 1].offScreenSpeed = 0.1f;
		list[list.Count - 1].bodySize = 0.1f;
		list[list.Count - 1].quantified = true;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].smallCreature = true;
		list[list.Count - 1].visualRadius = 400f;
		list[list.Count - 1].dangerousToPlayer = 0.05f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.None;
		list[list.Count - 1].countsAsAKill = 0;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].pickupAction = "Attach";
		list[list.Count - 1].throwAction = "Release";
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.SeaLeech, creatureTemplate10, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].baseDamageResistance = 0.2f;
		list[list.Count - 1].baseStunResistance = 0.2f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate10;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].bodySize = 0.15f;
		list[list.Count - 1].dangerousToPlayer = 0.07f;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].pickupAction = "Attach";
		list[list.Count - 1].throwAction = "Release";
		list2.Clear();
		list3.Clear();
		InitJungleLeech(list, creatureTemplate10);
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Wall, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 0.2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 20f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.6f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 1.6f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate11 = new CreatureTemplate(CreatureTemplate.Type.Snail, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
		list.Add(creatureTemplate11);
		list[list.Count - 1].baseDamageResistance = 0.4f;
		list[list.Count - 1].baseStunResistance = 0.8f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].doPreBakedPathing = true;
		list[list.Count - 1].grasps = 0;
		list[list.Count - 1].offScreenSpeed = 0.05f;
		list[list.Count - 1].bodySize = 0.7f;
		list[list.Count - 1].shortcutSegments = 2;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].visualRadius = 300f;
		list[list.Count - 1].waterVision = 1f;
		list[list.Count - 1].throughSurfaceVision = 0.5f;
		list[list.Count - 1].countsAsAKill = 1;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
		list[list.Count - 1].waterPathingResistance = 1f;
		list[list.Count - 1].communityInfluence = 0.25f;
		list[list.Count - 1].meatPoints = 1;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].throwAction = "Click";
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SkyHighway, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.Vulture, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].baseDamageResistance = 8.5f;
		list[list.Count - 1].baseStunResistance = 3.5f;
		list[list.Count - 1].abstractedLaziness = 10;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canSwim = false;
		list[list.Count - 1].canFly = true;
		list[list.Count - 1].doPreBakedPathing = true;
		list[list.Count - 1].canAutoAbstractPath = false;
		list[list.Count - 1].offScreenSpeed = 0.8f;
		list[list.Count - 1].bodySize = 6f;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].shortcutSegments = 5;
		list[list.Count - 1].visualRadius = 7000f;
		list[list.Count - 1].movementBasedVision = 0.4f;
		list[list.Count - 1].waterVision = 0.8f;
		list[list.Count - 1].throughSurfaceVision = 0.8f;
		list[list.Count - 1].hibernateOffScreen = true;
		list[list.Count - 1].dangerousToPlayer = 0.7f;
		list[list.Count - 1].communityInfluence = 0.25f;
		list[list.Count - 1].socialMemory = true;
		list[list.Count - 1].meatPoints = 14;
		list[list.Count - 1].lungCapacity = 750f;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		CreatureTemplate creatureTemplate12 = list[list.Count - 1];
		list[list.Count - 1].interestInOtherAncestorsCatches = 0.25f;
		list[list.Count - 1].interestInOtherCreaturesCatches = 0.25f;
		list[list.Count - 1].forbidStandardShortcutEntry = true;
		list[list.Count - 1].jumpAction = "Fly";
		list[list.Count - 1].pickupAction = "Ensnare";
		list[list.Count - 1].throwAction = "Release";
		list[list.Count - 1].isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => (aimap.getTerrainProximity(pos) < 2) ? 1 : 0;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SkyHighway, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.KingVulture, creatureTemplate12, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].baseDamageResistance = 12.5f;
		list[list.Count - 1].baseStunResistance = 6.5f;
		list[list.Count - 1].abstractedLaziness = 10;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate12;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].offScreenSpeed = 0.8f;
		list[list.Count - 1].bodySize = 8.5f;
		list[list.Count - 1].visualRadius = 7000f;
		list[list.Count - 1].movementBasedVision = 0.4f;
		list[list.Count - 1].waterVision = 0.8f;
		list[list.Count - 1].throughSurfaceVision = 0.8f;
		list[list.Count - 1].dangerousToPlayer = 0.9f;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list[list.Count - 1].meatPoints = 16;
		list[list.Count - 1].lungCapacity = 750f;
		list[list.Count - 1].canFly = true;
		list[list.Count - 1].jumpAction = "Fly";
		list[list.Count - 1].pickupAction = "Ensnare";
		list[list.Count - 1].throwAction = "Harpoon";
		list[list.Count - 1].isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => (aimap.getTerrainProximity(pos) < 2) ? 1 : 0;
		list2.Clear();
		list3.Clear();
		InitMirosVulture(list, creatureTemplate12);
		list.Add(new CreatureTemplate(CreatureTemplate.Type.GarbageWorm, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].offScreenSpeed = 0f;
		list[list.Count - 1].bodySize = 1f;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].visualRadius = 800f;
		list[list.Count - 1].saveCreature = false;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		list[list.Count - 1].baseDamageResistance = 1f;
		list[list.Count - 1].baseStunResistance = 1f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.GarbageWorms;
		list[list.Count - 1].communityInfluence = 1f;
		list[list.Count - 1].forbidStandardShortcutEntry = true;
		list[list.Count - 1].jumpAction = "Emotional State";
		list[list.Count - 1].pickupAction = "Grab";
		list[list.Count - 1].throwAction = "Release";
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 20f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 20f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.LanternMouse, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f)));
		list[list.Count - 1].baseDamageResistance = 0.5f;
		list[list.Count - 1].baseStunResistance = 0.4f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].offScreenSpeed = 0.3f;
		list[list.Count - 1].abstractedLaziness = 50;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].bodySize = 0.7f;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].shortcutSegments = 2;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].visualRadius = 800f;
		list[list.Count - 1].communityInfluence = 1f;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list[list.Count - 1].meatPoints = 2;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].jumpAction = "Sleep";
		list[list.Count - 1].pickupAction = "Hang";
		list[list.Count - 1].throwAction = "Squeak";
		list[list.Count - 1].usesNPCTransportation = true;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.CicadaA, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0f)));
		list[list.Count - 1].baseDamageResistance = 0.8f;
		list[list.Count - 1].baseStunResistance = 1f;
		list[list.Count - 1].abstractedLaziness = 100;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canFly = true;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].offScreenSpeed = 0.5f;
		list[list.Count - 1].bodySize = 0.6f;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate9;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].visualRadius = 1100f;
		list[list.Count - 1].movementBasedVision = 0.6f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.Cicadas;
		list[list.Count - 1].communityInfluence = 1f;
		list[list.Count - 1].socialMemory = true;
		list[list.Count - 1].lungCapacity = 180f;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		list[list.Count - 1].meatPoints = 3;
		list[list.Count - 1].usesNPCTransportation = true;
		list[list.Count - 1].jumpAction = "Bash";
		list[list.Count - 1].pickupAction = "Pick up Food";
		list[list.Count - 1].throwAction = "Discard";
		list2.Clear();
		list3.Clear();
		list.Add(new CreatureTemplate(CreatureTemplate.Type.CicadaB, list[list.Count - 1], list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0f)));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Wall, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 0.2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 20f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.6f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 1.6f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.Spider, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].baseDamageResistance = 0.1f;
		list[list.Count - 1].baseStunResistance = 0.1f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].abstractedLaziness = 100;
		list[list.Count - 1].AI = false;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate11;
		list[list.Count - 1].offScreenSpeed = 0.1f;
		list[list.Count - 1].bodySize = 0.1f;
		list[list.Count - 1].quantified = true;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].smallCreature = true;
		list[list.Count - 1].visualRadius = 600f;
		list[list.Count - 1].dangerousToPlayer = 0.1f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.None;
		list[list.Count - 1].countsAsAKill = 0;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
		list[list.Count - 1].usesNPCTransportation = true;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate13 = new CreatureTemplate(CreatureTemplate.Type.JetFish, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		list.Add(creatureTemplate13);
		list[list.Count - 1].baseDamageResistance = 0.95f;
		list[list.Count - 1].baseStunResistance = 0.9f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].abstractedLaziness = 100;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].offScreenSpeed = 2f;
		list[list.Count - 1].bodySize = 0.8f;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate10;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].visualRadius = 450f;
		list[list.Count - 1].waterVision = 1f;
		list[list.Count - 1].throughSurfaceVision = 0.65f;
		list[list.Count - 1].movementBasedVision = 0f;
		list[list.Count - 1].socialMemory = true;
		list[list.Count - 1].communityInfluence = 0.75f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.JetFish;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
		list[list.Count - 1].meatPoints = 4;
		list[list.Count - 1].jumpAction = "Water Jet";
		list[list.Count - 1].pickupAction = "Pick up Food";
		list[list.Count - 1].throwAction = "Discard";
		list[list.Count - 1].usesNPCTransportation = true;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SeaHighway, 100000f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.BigEel, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].baseDamageResistance = 1000f;
		list[list.Count - 1].baseStunResistance = 1000f;
		list[list.Count - 1].abstractedLaziness = 100;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].doPreBakedPathing = true;
		list[list.Count - 1].offScreenSpeed = 1f;
		list[list.Count - 1].bodySize = 100f;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].visualRadius = 1450f;
		list[list.Count - 1].waterVision = 1f;
		list[list.Count - 1].throughSurfaceVision = 0.65f;
		list[list.Count - 1].movementBasedVision = 0f;
		list[list.Count - 1].hibernateOffScreen = true;
		list[list.Count - 1].dangerousToPlayer = 0.8f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.None;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
		list[list.Count - 1].forbidStandardShortcutEntry = true;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].pickupAction = "Chomp";
		list[list.Count - 1].isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => (aimap.getTerrainProximity(pos) < 4) ? 1 : 0;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, (ModManager.MMF && MMF.cfgDeerBehavior.Value) ? 100f : 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 100000f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, (ModManager.MMF && MMF.cfgDeerBehavior.Value) ? 100f : 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, (ModManager.MMF && MMF.cfgDeerBehavior.Value) ? 1000f : 10f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate14 = new CreatureTemplate(CreatureTemplate.Type.Deer, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		list.Add(creatureTemplate14);
		list[list.Count - 1].baseDamageResistance = 200f;
		list[list.Count - 1].baseStunResistance = 8f;
		list[list.Count - 1].abstractedLaziness = 100;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].doPreBakedPathing = true;
		list[list.Count - 1].canAutoAbstractPath = false;
		list[list.Count - 1].offScreenSpeed = 1f;
		list[list.Count - 1].bodySize = 10f;
		list[list.Count - 1].grasps = 0;
		list[list.Count - 1].stowFoodInDen = false;
		list[list.Count - 1].visualRadius = 1450f;
		list[list.Count - 1].waterVision = 0.5f;
		list[list.Count - 1].throughSurfaceVision = 0.65f;
		list[list.Count - 1].movementBasedVision = 0f;
		list[list.Count - 1].hibernateOffScreen = true;
		list[list.Count - 1].communityInfluence = 0.85f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.Deer;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].forbidStandardShortcutEntry = true;
		list[list.Count - 1].jumpAction = "Bow";
		list[list.Count - 1].pickupAction = "Eat";
		list[list.Count - 1].throwAction = "Howl";
		list[list.Count - 1].isTooCloseToTerrain = (AImap aimap, IntVector2 pos) => (aimap.getTerrainProximity(pos) < 3 || aimap.getAItile(pos).smoothedFloorAltitude > 17) ? 1 : 0;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DoubleReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.TubeWorm, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].baseDamageResistance = 0.3f;
		list[list.Count - 1].baseStunResistance = 0.3f;
		list[list.Count - 1].instantDeathDamageLimit = 1.2f;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].offScreenSpeed = 0f;
		list[list.Count - 1].bodySize = 0.25f;
		list[list.Count - 1].grasps = 0;
		list[list.Count - 1].visualRadius = 100f;
		list[list.Count - 1].communityInfluence = 0.15f;
		list[list.Count - 1].countsAsAKill = 1;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].type == CreatureTemplate.Type.GreenLizard)
			{
				list[list.Count - 1].preBakedPathingAncestor = list[i];
				break;
			}
		}
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		list[list.Count - 1].meatPoints = 1;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].usesNPCTransportation = true;
		list[list.Count - 1].jumpAction = "Use Both Tongues";
		list[list.Count - 1].pickupAction = "Use Rear Tongue";
		list[list.Count - 1].throwAction = "Use Front Tongue";
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BigCreatureShortCutSqueeze, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate15 = new CreatureTemplate(CreatureTemplate.Type.DaddyLongLegs, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		list.Add(creatureTemplate15);
		list[list.Count - 1].baseDamageResistance = 200f;
		list[list.Count - 1].baseStunResistance = 200f;
		list[list.Count - 1].damageRestistances[(int)Creature.DamageType.Explosion, 0] = 0.03f;
		list[list.Count - 1].abstractedLaziness = 400;
		list[list.Count - 1].roamBetweenRoomsChance = 0.05f;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canFly = false;
		list[list.Count - 1].doPreBakedPathing = true;
		list[list.Count - 1].offScreenSpeed = 0.02f;
		list[list.Count - 1].bodySize = 5.5f;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].visualRadius = 0f;
		list[list.Count - 1].movementBasedVision = 0f;
		list[list.Count - 1].dangerousToPlayer = 0.85f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.None;
		list[list.Count - 1].daddyCorruptionImmune = true;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
		list[list.Count - 1].meatPoints = 10;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].shortcutColor = Color.blue;
		list[list.Count - 1].shortcutSegments = 3;
		list[list.Count - 1].jumpAction = "Cannonball";
		list[list.Count - 1].pickupAction = "Consume";
		list[list.Count - 1].throwAction = "Release";
		list[list.Count - 1].isTooCloseToTerrain = delegate(AImap aimap, IntVector2 pos)
		{
			if (aimap.room.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				return 2;
			}
			int terrainProximity3 = aimap.getTerrainProximity(pos);
			return (terrainProximity3 < 2 || terrainProximity3 > 11) ? 1 : 0;
		};
		list2.Clear();
		list3.Clear();
		list.Add(new CreatureTemplate(CreatureTemplate.Type.BrotherLongLegs, creatureTemplate15, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].baseDamageResistance = 100f;
		list[list.Count - 1].baseStunResistance = 2f;
		list[list.Count - 1].damageRestistances[(int)Creature.DamageType.Explosion, 0] = 0.03f;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate15;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].bodySize = 2.5f;
		list[list.Count - 1].offScreenSpeed = 0.1f;
		list[list.Count - 1].dangerousToPlayer = 0.75f;
		list[list.Count - 1].meatPoints = 5;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].shortcutColor = new Color(0.7f, 0.7f, 0.4f);
		list[list.Count - 1].shortcutSegments = 2;
		list[list.Count - 1].jumpAction = "Cannonball";
		list[list.Count - 1].pickupAction = "Consume";
		list[list.Count - 1].throwAction = "Release";
		list[list.Count - 1].isTooCloseToTerrain = delegate(AImap aimap, IntVector2 pos)
		{
			if (aimap.room.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				return 2;
			}
			int terrainProximity2 = aimap.getTerrainProximity(pos);
			return (terrainProximity2 < 2 || terrainProximity2 > 11) ? 1 : 0;
		};
		list2.Clear();
		list3.Clear();
		InitLongLegsMSC(list, creatureTemplate15);
		list.Add(new CreatureTemplate(CreatureTemplate.Type.TentaclePlant, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].baseDamageResistance = 2.5f;
		list[list.Count - 1].baseStunResistance = 0.8f;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = false;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].offScreenSpeed = 0f;
		list[list.Count - 1].bodySize = 2f;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].visualRadius = 400f;
		list[list.Count - 1].movementBasedVision = 1f;
		list[list.Count - 1].waterVision = 1f;
		list[list.Count - 1].throughSurfaceVision = 0f;
		list[list.Count - 1].dangerousToPlayer = 0.5f;
		list[list.Count - 1].communityInfluence = 0.1f;
		list[list.Count - 1].wormGrassImmune = true;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].pickupAction = "Snatch";
		list2.Clear();
		list3.Clear();
		list.Add(new CreatureTemplate(CreatureTemplate.Type.PoleMimic, list[list.Count - 1], list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].baseDamageResistance = 0.9f;
		list[list.Count - 1].baseStunResistance = 0.5f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].dangerousToPlayer = 0.5f;
		list[list.Count - 1].communityInfluence = 0.05f;
		list[list.Count - 1].countsAsAKill = 1;
		list[list.Count - 1].AI = false;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].wormGrassImmune = true;
		list[list.Count - 1].bodySize = 1.4f;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].jumpAction = "Stealth";
		list[list.Count - 1].pickupAction = "Grab";
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OutsideRoom, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 100f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.MirosBird, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].baseDamageResistance = 7f;
		list[list.Count - 1].baseStunResistance = 2.75f;
		list[list.Count - 1].abstractedLaziness = 10;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canFly = false;
		list[list.Count - 1].doPreBakedPathing = true;
		list[list.Count - 1].canAutoAbstractPath = false;
		list[list.Count - 1].offScreenSpeed = 1.5f;
		list[list.Count - 1].bodySize = 7f;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].visualRadius = 1800f;
		list[list.Count - 1].movementBasedVision = 0.1f;
		list[list.Count - 1].hibernateOffScreen = true;
		list[list.Count - 1].dangerousToPlayer = 0.6f;
		list[list.Count - 1].communityInfluence = 0.1f;
		list[list.Count - 1].meatPoints = 12;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list[list.Count - 1].forbidStandardShortcutEntry = true;
		list[list.Count - 1].interestInOtherAncestorsCatches = 2f;
		list[list.Count - 1].interestInOtherCreaturesCatches = 0.5f;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].canFly = true;
		list[list.Count - 1].jumpAction = "Sprint";
		list[list.Count - 1].pickupAction = "Snip";
		list[list.Count - 1].throwAction = "Release";
		list[list.Count - 1].isTooCloseToTerrain = delegate(AImap aimap, IntVector2 pos)
		{
			int terrainProximity = aimap.getTerrainProximity(pos);
			if (terrainProximity < 2)
			{
				return 1;
			}
			AItile aItile = aimap.getAItile(pos);
			return (aItile.smoothedFloorAltitude > 2 && (float)(aItile.smoothedFloorAltitude + aItile.floorAltitude) > Custom.LerpMap(terrainProximity, 2f, 6f, 6f, 4f) * 2f) ? 1 : 0;
		};
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Unallowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.TempleGuard, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f)));
		list[list.Count - 1].baseDamageResistance = 10000000f;
		list[list.Count - 1].baseStunResistance = 10000000f;
		list[list.Count - 1].abstractedLaziness = 20000;
		list[list.Count - 1].roamInRoomChance = -1f;
		list[list.Count - 1].roamBetweenRoomsChance = -1f;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canSwim = false;
		list[list.Count - 1].canFly = true;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate12;
		list[list.Count - 1].offScreenSpeed = 0.8f;
		list[list.Count - 1].bodySize = 7f;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].shortcutSegments = 5;
		list[list.Count - 1].visualRadius = 7000f;
		list[list.Count - 1].movementBasedVision = 0.4f;
		list[list.Count - 1].waterVision = 0.8f;
		list[list.Count - 1].throughSurfaceVision = 0.8f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.None;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list[list.Count - 1].forbidStandardShortcutEntry = true;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].pickupAction = "Telekinesis";
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Wall, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DoubleReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 1.1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 1.1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 2f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate16 = new CreatureTemplate(CreatureTemplate.Type.Centipede, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		list.Add(creatureTemplate16);
		list[list.Count - 1].baseDamageResistance = 0.9f;
		list[list.Count - 1].baseStunResistance = 0.6f;
		list[list.Count - 1].quickDeath = false;
		list[list.Count - 1].offScreenSpeed = 0.3f;
		list[list.Count - 1].grasps = 2;
		list[list.Count - 1].abstractedLaziness = 150;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].bodySize = 1f;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].shortcutSegments = 3;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate5;
		list[list.Count - 1].doubleReachUpConnectionParams = creatureTemplate5.doubleReachUpConnectionParams;
		list[list.Count - 1].visualRadius = 900f;
		list[list.Count - 1].waterVision = 0.4f;
		list[list.Count - 1].throughSurfaceVision = 0.85f;
		list[list.Count - 1].movementBasedVision = 0.95f;
		list[list.Count - 1].dangerousToPlayer = 0.3f;
		list[list.Count - 1].communityInfluence = 0.25f;
		list[list.Count - 1].meatPoints = 1;
		list[list.Count - 1].lungCapacity = 900f;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].damageRestistances[(int)Creature.DamageType.Electric, 0] = 102f;
		list[list.Count - 1].damageRestistances[(int)Creature.DamageType.Electric, 1] = 102f;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].stowFoodInDen = false;
		list[list.Count - 1].usesNPCTransportation = true;
		list[list.Count - 1].jumpAction = "Swap Heads";
		list[list.Count - 1].pickupAction = "Grab/Shock";
		list[list.Count - 1].throwAction = "Release";
		list2.Clear();
		list3.Clear();
		list.Add(new CreatureTemplate(CreatureTemplate.Type.RedCentipede, creatureTemplate16, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].baseDamageResistance = 1f;
		list[list.Count - 1].visualRadius = 1100f;
		list[list.Count - 1].dangerousToPlayer = 0.6f;
		list[list.Count - 1].communityInfluence = 0.25f;
		list[list.Count - 1].meatPoints = 1;
		list[list.Count - 1].shortcutColor = Color.red;
		list[list.Count - 1].shortcutSegments = 5;
		list[list.Count - 1].bodySize = 8.5f;
		list[list.Count - 1].dangerousToPlayer = 0.9f;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].jumpAction = "Swap Heads";
		list[list.Count - 1].pickupAction = "Grab/Shock";
		list[list.Count - 1].throwAction = "Release";
		list2.Clear();
		list3.Clear();
		list.Add(new CreatureTemplate(CreatureTemplate.Type.SmallCentipede, creatureTemplate16, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].roamBetweenRoomsChance = -1f;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.None;
		list[list.Count - 1].dangerousToPlayer = 0f;
		list[list.Count - 1].baseDamageResistance = 1f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].countsAsAKill = 0;
		list[list.Count - 1].abstractedLaziness = 1000;
		list[list.Count - 1].meatPoints = 0;
		list[list.Count - 1].bodySize = 0.2f;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].jumpAction = "Swap Heads";
		list[list.Count - 1].pickupAction = "Grab/Shock";
		list[list.Count - 1].throwAction = "Release";
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.Centiwing, creatureTemplate16, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].baseDamageResistance = 1f;
		list[list.Count - 1].canFly = true;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate9;
		list[list.Count - 1].visualRadius = 300f;
		list[list.Count - 1].waterVision = 0.4f;
		list[list.Count - 1].throughSurfaceVision = 0.85f;
		list[list.Count - 1].movementBasedVision = 0.25f;
		list[list.Count - 1].dangerousToPlayer = 0.4f;
		list[list.Count - 1].communityInfluence = 0.25f;
		list[list.Count - 1].meatPoints = 1;
		list[list.Count - 1].bodySize = 1.2f;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list[list.Count - 1].jumpAction = "Swap Heads";
		list[list.Count - 1].pickupAction = "Grab/Shock";
		list[list.Count - 1].throwAction = "Release";
		list2.Clear();
		list3.Clear();
		InitAquapede(list, creatureTemplate13, creatureTemplate16, creatureTemplate5);
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DoubleReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 20f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 2.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 225f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1.2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 15f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.RegionTransportation, 400f, PathCost.Legality.Allowed));
		CreatureTemplate creatureTemplate17 = new CreatureTemplate(CreatureTemplate.Type.Scavenger, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f));
		list.Add(creatureTemplate17);
		list[list.Count - 1].baseDamageResistance = 1.1f;
		list[list.Count - 1].baseStunResistance = 1.3f;
		list[list.Count - 1].instantDeathDamageLimit = 1.5f;
		list[list.Count - 1].offScreenSpeed = 0.75f;
		list[list.Count - 1].grasps = 4;
		list[list.Count - 1].abstractedLaziness = 50;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].bodySize = 1.2f;
		list[list.Count - 1].stowFoodInDen = false;
		list[list.Count - 1].shortcutSegments = 2;
		list[list.Count - 1].doPreBakedPathing = true;
		list[list.Count - 1].visualRadius = 1200f;
		list[list.Count - 1].movementBasedVision = 0.3f;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list[list.Count - 1].hibernateOffScreen = true;
		list[list.Count - 1].roamBetweenRoomsChance = -1f;
		list[list.Count - 1].roamInRoomChance = -1f;
		list[list.Count - 1].socialMemory = true;
		list[list.Count - 1].communityID = CreatureCommunities.CommunityID.Scavengers;
		list[list.Count - 1].communityInfluence = 1f;
		list[list.Count - 1].dangerousToPlayer = 0.65f;
		list[list.Count - 1].meatPoints = 4;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].usesNPCTransportation = true;
		list[list.Count - 1].usesRegionTransportation = true;
		list[list.Count - 1].usesCreatureHoles = false;
		list[list.Count - 1].jumpAction = "Point";
		list[list.Count - 1].pickupAction = "Pick Up";
		list[list.Count - 1].throwAction = "Throw";
		list2.Clear();
		list3.Clear();
		InitScavengersMSC(list, creatureTemplate17);
		list.Add(new CreatureTemplate(CreatureTemplate.Type.Overseer, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].baseDamageResistance = 0.9f;
		list[list.Count - 1].baseStunResistance = 0.5f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].offScreenSpeed = 0f;
		list[list.Count - 1].bodySize = 0.3f;
		list[list.Count - 1].wormGrassImmune = true;
		list[list.Count - 1].saveCreature = false;
		list[list.Count - 1].countsAsAKill = 1;
		list[list.Count - 1].BlizzardWanderer = true;
		list[list.Count - 1].BlizzardAdapted = true;
		list2.Clear();
		list3.Clear();
		list.Add(new CreatureTemplate(CreatureTemplate.Type.VultureGrub, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].baseDamageResistance = 0.3f;
		list[list.Count - 1].baseStunResistance = 0.3f;
		list[list.Count - 1].instantDeathDamageLimit = 0.6f;
		list[list.Count - 1].AI = false;
		list[list.Count - 1].requireAImap = false;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].offScreenSpeed = 0f;
		list[list.Count - 1].bodySize = 0.2f;
		list[list.Count - 1].grasps = 0;
		list[list.Count - 1].visualRadius = 0f;
		list[list.Count - 1].communityInfluence = 0f;
		list[list.Count - 1].countsAsAKill = 0;
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].type == CreatureTemplate.Type.GreenLizard)
			{
				list[list.Count - 1].preBakedPathingAncestor = list[j];
				break;
			}
		}
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		list[list.Count - 1].jumpAction = "Signal";
		list.Add(new CreatureTemplate(CreatureTemplate.Type.Hazer, list[list.Count - 1], list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
		list[list.Count - 1].jumpAction = "Camouflage";
		list[list.Count - 1].throwAction = "Spray Ink";
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Wall, 50f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Unwanted));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 10f, PathCost.Legality.Unwanted));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.EggBug, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f)));
		list[list.Count - 1].baseDamageResistance = 0.15f;
		list[list.Count - 1].baseStunResistance = 0.3f;
		list[list.Count - 1].instantDeathDamageLimit = 0.7f;
		list[list.Count - 1].offScreenSpeed = 0.3f;
		list[list.Count - 1].abstractedLaziness = 50;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].bodySize = 0.4f;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].shortcutSegments = 2;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate6;
		list[list.Count - 1].grasps = 0;
		list[list.Count - 1].visualRadius = 1200f;
		list[list.Count - 1].communityInfluence = 0.1f;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list[list.Count - 1].waterPathingResistance = 40f;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].meatPoints = 1;
		list[list.Count - 1].jumpAction = "Flee";
		list[list.Count - 1].usesNPCTransportation = true;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1.5f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Wall, 3f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Unwanted));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 10f, PathCost.Legality.Unwanted));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 1.5f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.BigSpider, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].baseDamageResistance = 0.6f;
		list[list.Count - 1].baseStunResistance = 0.5f;
		list[list.Count - 1].instantDeathDamageLimit = 1f;
		list[list.Count - 1].offScreenSpeed = 0.3f;
		list[list.Count - 1].abstractedLaziness = 50;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].bodySize = 1.1f;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].shortcutSegments = 2;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate5;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].visualRadius = 500f;
		list[list.Count - 1].communityInfluence = 0.1f;
		list[list.Count - 1].dangerousToPlayer = 0.35f;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list[list.Count - 1].waterPathingResistance = 5f;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].meatPoints = 3;
		list[list.Count - 1].interestInOtherAncestorsCatches = 0f;
		list[list.Count - 1].interestInOtherCreaturesCatches = 2f;
		list[list.Count - 1].jumpAction = "Pounce";
		list[list.Count - 1].pickupAction = "Grab";
		list[list.Count - 1].throwAction = "Release";
		list[list.Count - 1].usesNPCTransportation = true;
		list.Add(new CreatureTemplate(CreatureTemplate.Type.SpitterSpider, list[list.Count - 1], list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f)));
		list[list.Count - 1].bodySize = 1.2f;
		list[list.Count - 1].baseDamageResistance = 1.1f;
		list[list.Count - 1].baseStunResistance = 0.6f;
		list[list.Count - 1].visualRadius = 1100f;
		list[list.Count - 1].dangerousToPlayer = 0.5f;
		list[list.Count - 1].meatPoints = 4;
		list[list.Count - 1].jumpAction = "Spit";
		list[list.Count - 1].pickupAction = "Grab";
		list[list.Count - 1].throwAction = "Release";
		list2.Clear();
		list3.Clear();
		InitMotherSpider(list);
		list2.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 1f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.BigNeedleWorm, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f)));
		list[list.Count - 1].baseDamageResistance = 0.4f;
		list[list.Count - 1].baseStunResistance = 1f;
		list[list.Count - 1].instantDeathDamageLimit = 1.2f;
		list[list.Count - 1].abstractedLaziness = 10;
		list[list.Count - 1].roamBetweenRoomsChance = 0.07f;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].canFly = true;
		list[list.Count - 1].doPreBakedPathing = false;
		list[list.Count - 1].offScreenSpeed = 1.5f;
		list[list.Count - 1].bodySize = 0.7f;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate9;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].visualRadius = 900f;
		list[list.Count - 1].movementBasedVision = 0.3f;
		list[list.Count - 1].communityInfluence = 0.5f;
		list[list.Count - 1].socialMemory = true;
		list[list.Count - 1].shortcutSegments = 3;
		list[list.Count - 1].shortcutColor = new Color(1f, 0.5f, 0.5f);
		list[list.Count - 1].dangerousToPlayer = 0.5f;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
		list[list.Count - 1].meatPoints = 3;
		list[list.Count - 1].jumpAction = "Noot";
		list[list.Count - 1].pickupAction = "Skewer";
		list[list.Count - 1].usesNPCTransportation = true;
		list.Add(new CreatureTemplate(CreatureTemplate.Type.SmallNeedleWorm, list[list.Count - 1], list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 1f)));
		list[list.Count - 1].smallCreature = true;
		list[list.Count - 1].bodySize = 0.15f;
		list[list.Count - 1].baseDamageResistance = 0.2f;
		list[list.Count - 1].baseStunResistance = 0.4f;
		list[list.Count - 1].socialMemory = false;
		list[list.Count - 1].roamBetweenRoomsChance = 0.05f;
		list[list.Count - 1].offScreenSpeed = 3.2f;
		list[list.Count - 1].shortcutSegments = 1;
		list[list.Count - 1].dangerousToPlayer = 0f;
		list[list.Count - 1].meatPoints = 0;
		list[list.Count - 1].jumpAction = "Noot";
		list[list.Count - 1].pickupAction = "Hang On Mom";
		list[list.Count - 1].throwAction = "Detach";
		list[list.Count - 1].usesNPCTransportation = true;
		list2.Clear();
		list3.Clear();
		list2.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1.5f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Wall, 3f, PathCost.Legality.Allowed));
		list2.Add(new TileTypeResistance(AItile.Accessibility.Ceiling, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 10f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 3f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
		list3.Add(new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 1.5f, PathCost.Legality.Allowed));
		list.Add(new CreatureTemplate(CreatureTemplate.Type.DropBug, null, list2, list3, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f)));
		list[list.Count - 1].abstractedLaziness = 200;
		list[list.Count - 1].roamBetweenRoomsChance = 0.025f;
		list[list.Count - 1].baseDamageResistance = 0.95f;
		list[list.Count - 1].baseStunResistance = 0.6f;
		list[list.Count - 1].offScreenSpeed = 0.1f;
		list[list.Count - 1].AI = true;
		list[list.Count - 1].requireAImap = true;
		list[list.Count - 1].bodySize = 1.2f;
		list[list.Count - 1].stowFoodInDen = true;
		list[list.Count - 1].shortcutSegments = 2;
		list[list.Count - 1].preBakedPathingAncestor = creatureTemplate5;
		list[list.Count - 1].grasps = 1;
		list[list.Count - 1].visualRadius = 800f;
		list[list.Count - 1].movementBasedVision = 0.65f;
		list[list.Count - 1].communityInfluence = 0.1f;
		list[list.Count - 1].waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
		list[list.Count - 1].waterPathingResistance = 2f;
		list[list.Count - 1].canSwim = true;
		list[list.Count - 1].meatPoints = 3;
		list[list.Count - 1].dangerousToPlayer = 0.4f;
		list[list.Count - 1].jumpAction = "Pounce";
		list[list.Count - 1].pickupAction = "Hang/Grab";
		list[list.Count - 1].throwAction = "Release";
		list[list.Count - 1].usesNPCTransportation = true;
		list2.Clear();
		list3.Clear();
		InitYeek(list, creatureTemplate17);
		InitBigJellyfish(list, creatureTemplate13, creatureTemplate5);
		InitStowaway(list, creatureTemplate9);
		InitFireBug(list, creatureTemplate6);
		InitSlugpup(list, creatureTemplate17);
		InitBarnacle(list, creatureTemplate5);
		InitDrillCrab(list, creatureTemplate14);
		InitSandGrub(list);
		InitBigMoth(list, creatureTemplate12);
		InitSmallMoth(list, list[list.Count - 1], creatureTemplate9);
		InitBoxWorm(list, creatureTemplate9);
		InitFireSprite(list, creatureTemplate9);
		InitRattler(list);
		InitSkyWhale(list, creatureTemplate12);
		InitScavengerTemplar(list, creatureTemplate17);
		InitScavengerDisciple(list, creatureTemplate17);
		InitLoach(list, creatureTemplate4);
		InitRotLoach(list, list[list.Count - 1]);
		InitRat(list, creatureTemplate6);
		InitFrog(list, creatureTemplate7);
		InitTardigrade(list, creatureTemplate6);
		InitGrappleSnake(list);
		InitMillipede(list, creatureTemplate6);
		InitAngler(list, creatureTemplate13);
		InitRippleSpider(list, creatureTemplate9);
		InitMothGrub(list, creatureTemplate4);
		creatureTemplates = new CreatureTemplate[Math.Max(list.Count, ExtEnum<CreatureTemplate.Type>.values.Count)];
		foreach (CreatureTemplate item in list)
		{
			if (item.type.Index != -1)
			{
				creatureTemplates[item.type.Index] = item;
			}
		}
		InitCustomTemplates();
		int num = 0;
		int num2 = 0;
		for (int num3 = creatureTemplates.Length - 1; num3 >= 0; num3--)
		{
			if (creatureTemplates[num3] == null)
			{
				creatureTemplates[num3] = new CreatureTemplate(CreatureTemplate.Type.StandardGroundCreature, null, new List<TileTypeResistance>(), new List<TileConnectionResistance>(), new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
			}
		}
		for (int k = 0; k < creatureTemplates.Length; k++)
		{
			if (creatureTemplates[k].doPreBakedPathing)
			{
				num++;
			}
			if (creatureTemplates[k].quantified)
			{
				num2++;
			}
		}
		preBakedPathingCreatures = new CreatureTemplate[num];
		quantifiedCreatures = new CreatureTemplate[num2];
		num = 0;
		num2 = 0;
		for (int l = 0; l < creatureTemplates.Length; l++)
		{
			creatureTemplates[l].index = l;
			if (creatureTemplates[l].doPreBakedPathing)
			{
				preBakedPathingCreatures[num] = creatureTemplates[l];
				preBakedPathingCreatures[num].PreBakedPathingIndex = num;
				num++;
			}
			if (creatureTemplates[l].quantified)
			{
				quantifiedCreatures[num2] = creatureTemplates[l];
				quantifiedCreatures[num2].quantifiedIndex = num2;
				num2++;
			}
		}
		InitStaticWorldRelationships();
		InitStaticWorldRelationshipsMSC();
		InitStaticWorldRelationshipsWatcher();
		for (int m = 0; m < creatureTemplate8.relationships.Length; m++)
		{
			if (creatureTemplate8.relationships[m].type == CreatureTemplate.Relationship.Type.Eats)
			{
				creatureTemplate8.relationships[m].intensity *= 0.3f;
			}
		}
	}

	public static void InitStaticWorldRelationships()
	{
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.025f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.35f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.45f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.LizardTemplate, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.PinkLizard, CreatureTemplate.Type.PinkLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.PinkLizard, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.PinkLizard, CreatureTemplate.Type.BlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.PinkLizard, CreatureTemplate.Type.WhiteLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, CreatureTemplate.Type.PinkLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, CreatureTemplate.Type.WhiteLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, CreatureTemplate.Type.BlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.GreenLizard, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BlueLizard, CreatureTemplate.Type.PinkLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.BlueLizard, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.BlueLizard, CreatureTemplate.Type.BlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.45f));
		EstablishRelationship(CreatureTemplate.Type.BlueLizard, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.BlueLizard, CreatureTemplate.Type.WhiteLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.BlueLizard, CreatureTemplate.Type.CyanLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.BlueLizard, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.WhiteLizard, CreatureTemplate.Type.PinkLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.WhiteLizard, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.WhiteLizard, CreatureTemplate.Type.BlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.35f));
		EstablishRelationship(CreatureTemplate.Type.WhiteLizard, CreatureTemplate.Type.WhiteLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.WhiteLizard, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.WhiteLizard, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.RedLizard, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.RedLizard, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.RedLizard, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.RedLizard, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.RedLizard, CreatureTemplate.Type.RedLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));
		EstablishRelationship(CreatureTemplate.Type.RedLizard, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.CyanLizard, CreatureTemplate.Type.CyanLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.075f));
		EstablishRelationship(CreatureTemplate.Type.CyanLizard, CreatureTemplate.Type.BlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.BlackLizard, CreatureTemplate.Type.BlackLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.YellowLizard, CreatureTemplate.Type.YellowLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.Salamander, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Fly, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.Fly, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.Fly, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.Fly, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Fly, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Fly, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.Fly, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.01f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.Snail, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Snail, CreatureTemplate.Type.BrotherLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.Snail, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Snail, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.Snail, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.01f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Vulture, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.KingVulture, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.55f));
		EstablishRelationship(CreatureTemplate.Type.KingVulture, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.55f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.CicadaB, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.Centiwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.35f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.WhiteLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.BlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaB, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.CicadaB, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
		EstablishRelationship(CreatureTemplate.Type.CicadaB, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.Salamander, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.BrotherLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Leech, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Spider, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Spider, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Spider, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Spider, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Spider, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.Salamander, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.JetFish, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.01f));
		EstablishRelationship(CreatureTemplate.Type.BigEel, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.CicadaB, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.BrotherLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BrotherLongLegs, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BrotherLongLegs, CreatureTemplate.Type.TempleGuard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.TempleGuard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.Centiwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.SpitterSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.65f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Slugcat, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.TentaclePlant, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.TempleGuard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f));
		EstablishRelationship(CreatureTemplate.Type.MirosBird, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.TempleGuard, CreatureTemplate.Type.TempleGuard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 0f));
		EstablishRelationship(CreatureTemplate.Type.GarbageWorm, CreatureTemplate.Type.BrotherLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.GarbageWorm, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.GarbageWorm, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.GarbageWorm, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.GarbageWorm, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.SmallCentipede, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.RedCentipede, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.75f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.95f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 1f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Pack, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.BrotherLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.95f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.65f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.7f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.45f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.55f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.TempleGuard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.Centipede, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.RedCentipede, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.RedCentipede, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(CreatureTemplate.Type.RedCentipede, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
		EstablishRelationship(CreatureTemplate.Type.SmallCentipede, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.SmallCentipede, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.BlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.GreenLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.RedLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.11f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.45f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.Centiwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.CicadaB, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
		EstablishRelationship(CreatureTemplate.Type.BigSpider, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.RedLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.25f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.GarbageWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.Deer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
		EstablishRelationship(CreatureTemplate.Type.DropBug, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
		for (int i = 0; i < creatureTemplates.Length; i++)
		{
			EstablishRelationship(CreatureTemplate.Type.EggBug, new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(i)), new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(0f, 8f, creatureTemplates[i].bodySize)));
		}
		EstablishRelationship(CreatureTemplate.Type.EggBug, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		for (int j = 0; j < creatureTemplates.Length; j++)
		{
			EstablishRelationship(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(j)), CreatureTemplate.Type.Overseer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
		}
		EstablishRelationship(CreatureTemplate.Type.Scavenger, CreatureTemplate.Type.Overseer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));
	}

	public static CreatureTemplate GetCreatureTemplate(string nm)
	{
		for (int i = 0; i < creatureTemplates.Length; i++)
		{
			if (creatureTemplates[i].name == nm)
			{
				return creatureTemplates[i];
			}
		}
		return null;
	}

	public static CreatureTemplate GetCreatureTemplate(CreatureTemplate.Type type)
	{
		if (type == null || type.Index == -1)
		{
			return null;
		}
		return creatureTemplates[type.Index];
	}

	public static void EstablishRelationship(CreatureTemplate.Type a, CreatureTemplate.Type b, CreatureTemplate.Relationship relationship)
	{
		CreatureTemplate creatureTemplate = GetCreatureTemplate(a);
		CreatureTemplate creatureTemplate2 = GetCreatureTemplate(b);
		if (creatureTemplate == null || creatureTemplate2 == null || creatureTemplate2.type.Index == -1)
		{
			return;
		}
		creatureTemplate.relationships[creatureTemplate2.type.Index] = relationship;
		CreatureTemplate[] array = creatureTemplates;
		foreach (CreatureTemplate creatureTemplate3 in array)
		{
			if (creatureTemplate3 != null)
			{
				if (creatureTemplate3.ancestor == creatureTemplate)
				{
					EstablishRelationship(creatureTemplate3.type, creatureTemplate2.type, relationship);
				}
				if (creatureTemplate3.ancestor == creatureTemplate2)
				{
					EstablishRelationship(creatureTemplate.type, creatureTemplate3.type, relationship);
				}
			}
		}
	}
}
