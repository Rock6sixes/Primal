using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

namespace DevInterface;

public class ObjectsPage : Page
{
	public class DevObjectCategories : ExtEnum<DevObjectCategories>
	{
		public static DevObjectCategories Gameplay = new DevObjectCategories("Gameplay", register: true);

		public static DevObjectCategories Creatures = new DevObjectCategories("Creatures", register: true);

		public static DevObjectCategories Consumable = new DevObjectCategories("Consumable", register: true);

		public static DevObjectCategories CoralBrain = new DevObjectCategories("CoralBrain", register: true);

		public static DevObjectCategories Ripple = new DevObjectCategories("Ripple", register: true);

		public static DevObjectCategories Decoration = new DevObjectCategories("Decoration", register: true);

		public static DevObjectCategories Tutorial = new DevObjectCategories("Tutorial", register: true);

		public static DevObjectCategories Terrain = new DevObjectCategories("Terrain", register: true);

		public static DevObjectCategories Water = new DevObjectCategories("Water", register: true);

		public static DevObjectCategories Unsorted = new DevObjectCategories("Unsorted", register: true);

		public DevObjectCategories(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public PlacedObject.LightFixtureData.Type lastPlacedLightFixture = PlacedObject.LightFixtureData.Type.RedLight;

	public PlacedObjectRepresentation draggedObject;

	public PlacedObjectRepresentation removeIfReleaseObject;

	public TrashBin trashBin;

	public Panel objectsPanel;

	public bool collapseAll;

	public PlacedObject.Type[] placedObjectTypes;

	public int maxObjectsPerPage = 20;

	public int currObjectsPage;

	public int totalObjectsPages;

	public ObjectsPage(DevUI owner, string IDstring, DevUINode parentNode, string name)
		: base(owner, IDstring, parentNode, name)
	{
		objectsPanel = new Panel(owner, "Objects_Panel", this, new Vector2(550f, 80f), new Vector2(400f, 260f), "OBJECTS: ");
		subNodes.Add(objectsPanel);
		for (int i = 0; i < 2; i++)
		{
			objectsPanel.subNodes.Add(new Button(owner, (i != 0) ? "Next_Button" : "Prev_Button", objectsPanel, new Vector2(5f + 100f * (float)i, objectsPanel.size.y - 16f - 5f), 95f, (i != 0) ? "Next Page" : "Previous Page"));
		}
		subNodes.Add(new Button(owner, "Collapse", this, new Vector2(20f, 20f), 95f, "Collapse All"));
		trashBin = new TrashBin(owner, "Trash_Bin", this, new Vector2(40f, 40f));
		subNodes.Add(trashBin);
		AssembleObjectPages();
		RefreshObjectsPage();
	}

	public void RefreshObjectsPage()
	{
		if (totalObjectsPages == 0)
		{
			currObjectsPage = 0;
		}
		for (int num = objectsPanel.subNodes.Count - 1; num >= 2; num--)
		{
			objectsPanel.subNodes[num].ClearSprites();
			objectsPanel.subNodes.RemoveAt(num);
		}
		int num2 = currObjectsPage * maxObjectsPerPage;
		int i = 0;
		objectsPanel.Title = "OBJECTS: ...";
		for (; i < maxObjectsPerPage && i + num2 < placedObjectTypes.Length; i++)
		{
			float num3 = (float)maxObjectsPerPage / 2f;
			float num4 = (float)Mathf.FloorToInt((float)i / num3) * 195f;
			float num5 = 5f;
			num5 += num4;
			float num6 = 20f * num3 * (float)Mathf.FloorToInt((float)i / num3);
			float num7 = objectsPanel.size.y - 16f - 35f - 20f * (float)i;
			num7 += num6;
			if (i == 0)
			{
				objectsPanel.Title = "OBJECTS: " + DevObjectGetCategoryFromPlacedType(placedObjectTypes[num2 + i]);
			}
			if (placedObjectTypes[num2 + i] != PlacedObject.Type.None)
			{
				objectsPanel.subNodes.Add(new AddObjectButton(owner, objectsPanel, new Vector2(num5, num7), 190f, placedObjectTypes[num2 + i]));
			}
		}
	}

	public override void Update()
	{
		draggedObject = null;
		base.Update();
		if (draggedObject != null && trashBin.MouseOver)
		{
			trashBin.LineColor = ((Random.value < 0.5f) ? new Color(1f, 0f, 0f) : new Color(1f, 1f, 1f));
			removeIfReleaseObject = draggedObject;
			return;
		}
		trashBin.LineColor = new Color(1f, 1f, 1f);
		if (!owner.mouseDown && removeIfReleaseObject != null)
		{
			RemoveObject(removeIfReleaseObject);
		}
		removeIfReleaseObject = null;
	}

	public override void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		if (type == DevUISignalType.ButtonClick)
		{
			switch (sender.IDstring)
			{
			case "Save_Settings":
				base.RoomSettings.Save();
				break;
			case "Export_Sandbox":
				(owner.game.GetArenaGameSession as SandboxGameSession).editor.DevToolsExportConfig();
				break;
			case "Prev_Button":
				currObjectsPage--;
				if (currObjectsPage < 0)
				{
					currObjectsPage = totalObjectsPages - 1;
				}
				RefreshObjectsPage();
				break;
			case "Next_Button":
				currObjectsPage++;
				if (currObjectsPage >= totalObjectsPages)
				{
					currObjectsPage = 0;
				}
				RefreshObjectsPage();
				break;
			case "Save_Specific":
				base.RoomSettings.Save(owner.game.TimelinePoint);
				break;
			case "Collapse":
			{
				collapseAll = !collapseAll;
				for (int i = 0; i < subNodes.Count; i++)
				{
					for (int j = 0; j < subNodes[i].subNodes.Count; j++)
					{
						if (subNodes[i].subNodes[j] is Panel panel)
						{
							panel.collapsed = collapseAll;
						}
					}
				}
				if (objectsPanel.collapsed != collapseAll)
				{
					objectsPanel.ToggleCollapse();
				}
				break;
			}
			}
		}
		else if (type == DevUISignalType.Create)
		{
			CreateObjRep(new PlacedObject.Type(message), null);
		}
	}

	public void RemoveObject(PlacedObjectRepresentation objRep)
	{
		base.RoomSettings.placedObjects.Remove(objRep.pObj);
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		for (int i = 0; i < base.RoomSettings.placedObjects.Count; i++)
		{
			CreateObjRep(base.RoomSettings.placedObjects[i].type, base.RoomSettings.placedObjects[i]);
		}
	}

	public void CreateObjRep(PlacedObject.Type tp, PlacedObject pObj)
	{
		if (pObj == null)
		{
			pObj = new PlacedObject(tp, null);
			pObj.pos = owner.room.game.cameras[0].pos + Vector2.Lerp(owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f;
			base.RoomSettings.placedObjects.Add(pObj);
			if (tp == PlacedObject.Type.LightFixture)
			{
				(pObj.data as PlacedObject.LightFixtureData).type = lastPlacedLightFixture;
			}
			else if (tp == PlacedObject.Type.TerrainHandle && owner.room.terrain == null)
			{
				owner.room.AddObject(new TerrainCurve(owner.room));
			}
		}
		PlacedObjectRepresentation placedObjectRepresentation = null;
		if (tp == PlacedObject.Type.LightSource)
		{
			placedObjectRepresentation = new LightSourceRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.LightFixture)
		{
			placedObjectRepresentation = new LightFixtureRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.CoralCircuit || tp == PlacedObject.Type.CoralStem || tp == PlacedObject.Type.CoralStemWithNeurons || tp == PlacedObject.Type.Corruption || tp == PlacedObject.Type.CorruptionDarkness || tp == PlacedObject.Type.StuckDaddy || tp == PlacedObject.Type.WallMycelia || tp == PlacedObject.Type.ProjectedStars || tp == PlacedObject.Type.CentipedeAttractor || tp == PlacedObject.Type.DandelionPatch || tp == PlacedObject.Type.NoSpearStickZone || tp == PlacedObject.Type.TradeOutpost || tp == PlacedObject.Type.ScavengerTreasury || tp == PlacedObject.Type.CosmeticSlimeMold || tp == PlacedObject.Type.CosmeticSlimeMold2 || tp == PlacedObject.Type.PlayerPushback || tp == PlacedObject.Type.NoLeviathanStrandingZone || tp == PlacedObject.Type.NeuronSpawner || tp == PlacedObject.Type.NoCurrentZone || tp == PlacedObject.Type.BrainMold || tp == PlacedObject.Type.LocustCloud || tp == PlacedObject.Type.WarpFilter || (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.PrinceBulb) || (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.SKLightningRod))
		{
			placedObjectRepresentation = new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: true);
		}
		else if (tp == PlacedObject.Type.CoralNeuron || tp == PlacedObject.Type.CorruptionTube || tp == PlacedObject.Type.LanternOnStick || tp == PlacedObject.Type.ScavTradeInstruction || tp == PlacedObject.Type.DeadTokenStalk || tp == PlacedObject.Type.Vine || tp == PlacedObject.Type.TerrainSunOffset)
		{
			placedObjectRepresentation = new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: false);
		}
		else if (tp == PlacedObject.Type.ZapCoil || tp == PlacedObject.Type.SuperStructureFuses || tp == PlacedObject.Type.ARKillRect)
		{
			placedObjectRepresentation = new GridRectObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.SpotLight || tp == PlacedObject.Type.SuperJumpInstruction || tp == PlacedObject.Type.NoRippleSproutZone)
		{
			placedObjectRepresentation = new QuadObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.DeepProcessing)
		{
			placedObjectRepresentation = new DeepProcessingRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.SSLightRod)
		{
			placedObjectRepresentation = new SSLightRodRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.ScavengerOutpost)
		{
			placedObjectRepresentation = new ScavOutpostRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.DangleFruit || tp == PlacedObject.Type.DataPearl || tp == PlacedObject.Type.UniqueDataPearl || tp == PlacedObject.Type.SeedCob || tp == PlacedObject.Type.FlareBomb || tp == PlacedObject.Type.PuffBall || tp == PlacedObject.Type.GraffitiBomb || tp == PlacedObject.Type.WaterNut || tp == PlacedObject.Type.JellyFish || tp == PlacedObject.Type.KarmaFlower || tp == PlacedObject.Type.Mushroom || tp == PlacedObject.Type.VoidSpawnEgg || tp == PlacedObject.Type.FirecrackerPlant || tp == PlacedObject.Type.VultureGrub || tp == PlacedObject.Type.DeadVultureGrub || tp == PlacedObject.Type.SlimeMold || tp == PlacedObject.Type.FlyLure || tp == PlacedObject.Type.SporePlant || tp == PlacedObject.Type.NeedleEgg || tp == PlacedObject.Type.BubbleGrass || tp == PlacedObject.Type.Hazer || tp == PlacedObject.Type.DeadHazer || tp == PlacedObject.Type.Lantern || tp == PlacedObject.Type.SandGrubNetwork)
		{
			placedObjectRepresentation = new ConsumableRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.ExitSymbolHidden || tp == PlacedObject.Type.ExitSymbolShelter)
		{
			placedObjectRepresentation = new TileObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.CustomDecal)
		{
			placedObjectRepresentation = new CustomDecalRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.InsectGroup)
		{
			placedObjectRepresentation = new InsectGroupRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.MultiplayerItem)
		{
			placedObjectRepresentation = new MultiplayerItemRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.BlueToken || tp == PlacedObject.Type.GoldToken || tp == PlacedObject.Type.GreenToken)
		{
			placedObjectRepresentation = new TokenRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.Filter)
		{
			placedObjectRepresentation = new FilterRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.CompetitiveFilter)
		{
			placedObjectRepresentation = new CompetitiveFilterRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.RippleLevelFilter)
		{
			placedObjectRepresentation = new RippleLevelFilterRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.PrinceFilter)
		{
			placedObjectRepresentation = new PrinceFilterRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.RippleEggFilter)
		{
			placedObjectRepresentation = new RippleEggFilterRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.RippleEggDestination)
		{
			placedObjectRepresentation = new RippleEggDestinationRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.ReliableIggyDirection)
		{
			placedObjectRepresentation = new ReliableDirectionRep(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.Rainbow)
		{
			placedObjectRepresentation = new RainbowRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.RainbowNoFade)
		{
			placedObjectRepresentation = new RainbowNoFadeRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.LightBeam)
		{
			placedObjectRepresentation = new LightBeamRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.ExitSymbolAncientShelter)
		{
			placedObjectRepresentation = new TileObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.FairyParticleSettings)
		{
			placedObjectRepresentation = new FairyParticleRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.DayNightSettings)
		{
			placedObjectRepresentation = new DayNightRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.LightningMachine)
		{
			placedObjectRepresentation = new LightningMachineRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.EnergySwirl)
		{
			placedObjectRepresentation = new EnergySwirlRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.SteamPipe || tp == PlacedObject.Type.WallSteamer)
		{
			placedObjectRepresentation = new SteamPipeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.SnowSource)
		{
			placedObjectRepresentation = new SnowSourceRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.CellDistortion)
		{
			placedObjectRepresentation = new CellDistortionRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.LocalBlizzard)
		{
			placedObjectRepresentation = new LocalBlizzardRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.AdjustableFan)
		{
			placedObjectRepresentation = new AdjustableFanRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.HarmfulSteam)
		{
			placedObjectRepresentation = new HarmfulSteamRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.WarpPoint)
		{
			placedObjectRepresentation = new WarpPointToRoomRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.SkyWhalePathfinding)
		{
			placedObjectRepresentation = new SkyWhalePathfindingNodeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.WindRect)
		{
			placedObjectRepresentation = new WindRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.TerrainHandle)
		{
			placedObjectRepresentation = new TerrainHandleRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.LocalTerrain)
		{
			placedObjectRepresentation = new LocalTerrainRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.CurvedSlope)
		{
			placedObjectRepresentation = new CurvedSlopeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.SuperSlope)
		{
			placedObjectRepresentation = new SuperSlopeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.FluxWaterfall || tp == PlacedObject.Type.FluxDrain)
		{
			placedObjectRepresentation = new WaterFlowRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.TerrainRubble)
		{
			placedObjectRepresentation = new TerrainRubbleRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: true);
		}
		else if (tp == PlacedObject.Type.CosmeticRipple)
		{
			placedObjectRepresentation = new CosmeticRippleRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.Pomegranate)
		{
			placedObjectRepresentation = new PomegranateRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.PomegranateVine)
		{
			placedObjectRepresentation = new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: false);
		}
		else if (tp == PlacedObject.Type.HugeTurbine)
		{
			placedObjectRepresentation = new HugeTurbineRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.Geyser)
		{
			placedObjectRepresentation = new GeyserRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.PlateTree)
		{
			placedObjectRepresentation = new PlateTreeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), PlateTree.Variant.Plate);
		}
		else if (tp == PlacedObject.Type.RotPlateTree)
		{
			placedObjectRepresentation = new PlateTreeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), PlateTree.Variant.Rot);
		}
		else if (tp == PlacedObject.Type.WaterCurrent)
		{
			placedObjectRepresentation = new WaterCurrentRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.BlackSpot)
		{
			placedObjectRepresentation = new BlackSpotRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.DynamicWarpTarget)
		{
			placedObjectRepresentation = new DynamicWarpTargetRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.SpinningTopSpot)
		{
			placedObjectRepresentation = new SpinningTopSpotRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.WeaverSpot)
		{
			placedObjectRepresentation = new WeaverSpotRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.FloatingDebris)
		{
			placedObjectRepresentation = new FloatingDebrisRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.GooDrips)
		{
			placedObjectRepresentation = new GooDripsRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.LobeTree)
		{
			placedObjectRepresentation = new LobeTreeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.PassiveCorruption)
		{
			placedObjectRepresentation = new CustomRotRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.FlameJet)
		{
			placedObjectRepresentation = new FlameJetRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.WindDirection)
		{
			placedObjectRepresentation = new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: false);
		}
		else if (tp == PlacedObject.Type.WaterCutoff)
		{
			placedObjectRepresentation = new WaterCutoffRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.AirPocket)
		{
			placedObjectRepresentation = new AirPocketRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.MudPit)
		{
			placedObjectRepresentation = new MudPitRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.TerrainGrassPatch)
		{
			placedObjectRepresentation = new TerrainGrassPatchRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.AetherRainbow)
		{
			placedObjectRepresentation = new AetherRainbowRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.WallLight)
		{
			placedObjectRepresentation = new WallLightRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.UrbanLife)
		{
			placedObjectRepresentation = new UrbanLifeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.UrbanLifePath)
		{
			placedObjectRepresentation = new UrbanLifePathRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.UrbanCandles)
		{
			placedObjectRepresentation = new UrbanCandlePlacerRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.UrbanCandleHolder)
		{
			placedObjectRepresentation = new UrbanCandleHolderRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.KarmaFlowerPatch)
		{
			placedObjectRepresentation = new KarmaFlowerPatchRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.BigSkyWhaleSpawner)
		{
			placedObjectRepresentation = new BigSkyWhaleSpawnerRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.BigSkyWhaleTrigger)
		{
			placedObjectRepresentation = new BigSkyWhaleTriggerRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (ModManager.Watcher && tp == WatcherEnums.PlacedObjectType.TowerCrabSpawner)
		{
			placedObjectRepresentation = new TowerCrabSpawnerRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.SpawnMigrationStream)
		{
			placedObjectRepresentation = new SpawnMigrationStreamRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
		}
		else if (tp == PlacedObject.Type.HideVoidSpawn)
		{
			placedObjectRepresentation = new HideVoidSpawnRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
		}
		else if (tp == PlacedObject.Type.RippleTree)
		{
			placedObjectRepresentation = new RippleTreeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, "Ripple Tree");
		}
		else if (tp == PlacedObject.Type.RippleStalk)
		{
			placedObjectRepresentation = new RippleTreeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, "Ripple Stalk");
		}
		else
		{
			if (ModManager.DLCShared)
			{
				if (tp == DLCSharedEnums.PlacedObjectType.GooieDuck || tp == DLCSharedEnums.PlacedObjectType.LillyPuck || tp == DLCSharedEnums.PlacedObjectType.GlowWeed || tp == DLCSharedEnums.PlacedObjectType.DandelionPeach)
				{
					placedObjectRepresentation = new ConsumableRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
				}
				else if (tp == DLCSharedEnums.PlacedObjectType.MSArteryPush || tp == DLCSharedEnums.PlacedObjectType.BigJellyFish || tp == DLCSharedEnums.PlacedObjectType.RotFlyPaper || tp == DLCSharedEnums.PlacedObjectType.Stowaway)
				{
					placedObjectRepresentation = new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: true);
				}
				else if (tp == DLCSharedEnums.PlacedObjectType.OEsphere)
				{
					placedObjectRepresentation = new OEsphereRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
				}
			}
			if (ModManager.MSC)
			{
				if (tp == MoreSlugcatsEnums.PlacedObjectType.Germinator || tp == MoreSlugcatsEnums.PlacedObjectType.MoonCloak || tp == MoreSlugcatsEnums.PlacedObjectType.HRGuard)
				{
					placedObjectRepresentation = new ConsumableRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
				}
				else if (tp == MoreSlugcatsEnums.PlacedObjectType.KarmaShrine)
				{
					placedObjectRepresentation = new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: true);
				}
				else if (tp == MoreSlugcatsEnums.PlacedObjectType.WhiteToken || tp == MoreSlugcatsEnums.PlacedObjectType.RedToken || tp == MoreSlugcatsEnums.PlacedObjectType.DevToken)
				{
					placedObjectRepresentation = new TokenRepresentation(owner, tp.ToString() + "_Rep", this, pObj);
				}
			}
			if (ModManager.Watcher)
			{
				if (tp == WatcherEnums.PlacedObjectType.RottenDangleFruit || tp == WatcherEnums.PlacedObjectType.RottenSeedCob)
				{
					placedObjectRepresentation = new ConsumableRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
				}
				else if (tp == WatcherEnums.PlacedObjectType.ARZapperOmni || tp == WatcherEnums.PlacedObjectType.ARZapperVertical || tp == WatcherEnums.PlacedObjectType.ARZapperHorizontal)
				{
					placedObjectRepresentation = new GridRectObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
				}
				else if (tp == WatcherEnums.PlacedObjectType.DaemonClaw)
				{
					placedObjectRepresentation = new SteamPipeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
				}
				else if (tp == WatcherEnums.PlacedObjectType.DaemonEye)
				{
					placedObjectRepresentation = new DaemonEyeRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
				}
				else if (tp == WatcherEnums.PlacedObjectType.DaemonCrown)
				{
					placedObjectRepresentation = new DaemonCrownRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
				}
				else if (tp == WatcherEnums.PlacedObjectType.PearlReader)
				{
					placedObjectRepresentation = new ResizeableObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString(), showRing: false);
				}
			}
			if (placedObjectRepresentation == null)
			{
				placedObjectRepresentation = new PlacedObjectRepresentation(owner, tp.ToString() + "_Rep", this, pObj, tp.ToString());
			}
		}
		if (placedObjectRepresentation != null)
		{
			tempNodes.Add(placedObjectRepresentation);
			subNodes.Add(placedObjectRepresentation);
		}
	}

	public DevObjectCategories DevObjectGetCategoryFromPlacedType(PlacedObject.Type type)
	{
		if (type == PlacedObject.Type.LightSource || type == PlacedObject.Type.LightFixture || type == PlacedObject.Type.SpotLight || type == PlacedObject.Type.LightBeam || type == PlacedObject.Type.CustomDecal || type == PlacedObject.Type.Rainbow || type == PlacedObject.Type.FairyParticleSettings || type == PlacedObject.Type.DandelionPatch || type == PlacedObject.Type.LanternOnStick || type == PlacedObject.Type.InsectGroup || type == PlacedObject.Type.BrokenShelterWaterLevel || type == PlacedObject.Type.SnowSource || type == PlacedObject.Type.LocalBlizzard || type == PlacedObject.Type.DayNightSettings || type == PlacedObject.Type.LightningMachine || type == PlacedObject.Type.SteamPipe || type == PlacedObject.Type.WallSteamer || type == PlacedObject.Type.BlinkingFlower || type == PlacedObject.Type.CellDistortion || type == PlacedObject.Type.EnergySwirl || type == PlacedObject.Type.AdjustableFan || type == PlacedObject.Type.PomegranateVine || type == PlacedObject.Type.HugeTurbine || type == PlacedObject.Type.PlateTree || type == PlacedObject.Type.FloatingDebris || type == PlacedObject.Type.GooDrips || type == PlacedObject.Type.LobeTree || type == PlacedObject.Type.RotPlateTree || type == PlacedObject.Type.BrainMold || type == PlacedObject.Type.FlameJet || type == PlacedObject.Type.WindDirection || type == PlacedObject.Type.RainbowNoFade)
		{
			return DevObjectCategories.Decoration;
		}
		if (type == PlacedObject.Type.TempleGuard || type == PlacedObject.Type.Hazer || type == PlacedObject.Type.DeadHazer || type == PlacedObject.Type.JellyFish || type == PlacedObject.Type.VultureGrub || type == PlacedObject.Type.DeadVultureGrub || type == PlacedObject.Type.SandGrubHole || type == PlacedObject.Type.SandGrubNetwork || type == PlacedObject.Type.Rattler || type == PlacedObject.Type.LocustCloud)
		{
			return DevObjectCategories.Creatures;
		}
		if (type == PlacedObject.Type.Filter || type == PlacedObject.Type.CompetitiveFilter || type == PlacedObject.Type.PrinceFilter || type == PlacedObject.Type.NoSpearStickZone || type == PlacedObject.Type.PlayerPushback || type == PlacedObject.Type.CentipedeAttractor || type == PlacedObject.Type.VoidSpawnEgg || type == PlacedObject.Type.ScavengerOutpost || type == PlacedObject.Type.TradeOutpost || type == PlacedObject.Type.ScavengerTreasury || type == PlacedObject.Type.ExitSymbolShelter || type == PlacedObject.Type.ExitSymbolAncientShelter || type == PlacedObject.Type.ExitSymbolHidden || type == PlacedObject.Type.NoLeviathanStrandingZone || type == PlacedObject.Type.NeuronSpawner || type == PlacedObject.Type.DeathFallFocus || type == PlacedObject.Type.Vine || type == PlacedObject.Type.HarmfulSteam || type == PlacedObject.Type.WindRect || type == PlacedObject.Type.ARKillRect || type == PlacedObject.Type.ShelterSpawnPoint || type == PlacedObject.Type.GhostSpot || type == PlacedObject.Type.SkyWhalePathfinding || type == PlacedObject.Type.MudPit)
		{
			return DevObjectCategories.Gameplay;
		}
		if (type == PlacedObject.Type.ReliableSpear || type == PlacedObject.Type.DataPearl || type == PlacedObject.Type.UniqueDataPearl || type == PlacedObject.Type.DangleFruit || type == PlacedObject.Type.WaterNut || type == PlacedObject.Type.FlareBomb || type == PlacedObject.Type.PuffBall || type == PlacedObject.Type.GraffitiBomb || type == PlacedObject.Type.KarmaFlower || type == PlacedObject.Type.Mushroom || type == PlacedObject.Type.SlimeMold || type == PlacedObject.Type.CosmeticSlimeMold || type == PlacedObject.Type.CosmeticSlimeMold2 || type == PlacedObject.Type.FlyLure || type == PlacedObject.Type.FirecrackerPlant || type == PlacedObject.Type.SporePlant || type == PlacedObject.Type.NeedleEgg || type == PlacedObject.Type.BubbleGrass || type == PlacedObject.Type.HangingPearls || type == PlacedObject.Type.Lantern || type == PlacedObject.Type.VultureMask || type == PlacedObject.Type.SeedCob || type == PlacedObject.Type.DeadSeedCob || type == PlacedObject.Type.Pomegranate || type == PlacedObject.Type.MultiplayerItem)
		{
			return DevObjectCategories.Consumable;
		}
		if (type == PlacedObject.Type.CoralStem || type == PlacedObject.Type.CoralStemWithNeurons || type == PlacedObject.Type.CoralNeuron || type == PlacedObject.Type.CoralCircuit || type == PlacedObject.Type.WallMycelia || type == PlacedObject.Type.ProjectedStars || type == PlacedObject.Type.SuperStructureFuses || type == PlacedObject.Type.DeepProcessing || type == PlacedObject.Type.GravityDisruptor || type == PlacedObject.Type.ZapCoil || type == PlacedObject.Type.SSLightRod || type == PlacedObject.Type.Corruption || type == PlacedObject.Type.CorruptionTube || type == PlacedObject.Type.CorruptionDarkness || type == PlacedObject.Type.StuckDaddy || type == PlacedObject.Type.PassiveCorruption)
		{
			return DevObjectCategories.CoralBrain;
		}
		if (type == PlacedObject.Type.WarpPoint || type == PlacedObject.Type.DynamicWarpTarget || type == PlacedObject.Type.CosmeticRipple || type == PlacedObject.Type.RippleLevelFilter || type == PlacedObject.Type.RippleEggFilter || type == PlacedObject.Type.WarpFilter || type == PlacedObject.Type.RippleSpawnEgg || type == PlacedObject.Type.RippleTree || type == PlacedObject.Type.RippleStalk || type == PlacedObject.Type.RippleEggDestination || type == PlacedObject.Type.NoRippleSproutZone)
		{
			return DevObjectCategories.Ripple;
		}
		if (type == PlacedObject.Type.SuperJumpInstruction || type == PlacedObject.Type.ProjectedImagePosition || type == PlacedObject.Type.ScavTradeInstruction || type == PlacedObject.Type.ReliableIggyDirection || type == PlacedObject.Type.DeadTokenStalk || type == PlacedObject.Type.BlueToken || type == PlacedObject.Type.GoldToken || type == PlacedObject.Type.GreenToken)
		{
			return DevObjectCategories.Tutorial;
		}
		if (type == PlacedObject.Type.TerrainHandle || type == PlacedObject.Type.TerrainRubble || type == PlacedObject.Type.TerrainSunOffset || type == PlacedObject.Type.BlackSpot || type == PlacedObject.Type.LocalTerrain || type == PlacedObject.Type.SuperSlope || type == PlacedObject.Type.CurvedSlope || type == PlacedObject.Type.TerrainGrassPatch)
		{
			return DevObjectCategories.Terrain;
		}
		if (type == PlacedObject.Type.FluxWaterfall || type == PlacedObject.Type.WaterCycleBottom || type == PlacedObject.Type.WaterCycleTop || type == PlacedObject.Type.FluxDrain || type == PlacedObject.Type.Geyser || type == PlacedObject.Type.WaterCurrent || type == PlacedObject.Type.NoCurrentZone || type == PlacedObject.Type.WaterCutoff || type == PlacedObject.Type.AirPocket)
		{
			return DevObjectCategories.Water;
		}
		if (ModManager.DLCShared)
		{
			if (type == DLCSharedEnums.PlacedObjectType.OEsphere)
			{
				return DevObjectCategories.Decoration;
			}
			if (type == DLCSharedEnums.PlacedObjectType.MSArteryPush)
			{
				return DevObjectCategories.Gameplay;
			}
			if (type == DLCSharedEnums.PlacedObjectType.DandelionPeach || type == DLCSharedEnums.PlacedObjectType.RotFlyPaper || type == DLCSharedEnums.PlacedObjectType.GooieDuck || type == DLCSharedEnums.PlacedObjectType.LillyPuck || type == DLCSharedEnums.PlacedObjectType.GlowWeed)
			{
				return DevObjectCategories.Consumable;
			}
			if (type == DLCSharedEnums.PlacedObjectType.BigJellyFish || type == DLCSharedEnums.PlacedObjectType.Stowaway)
			{
				return DevObjectCategories.Creatures;
			}
		}
		if (ModManager.MSC)
		{
			if (type == MoreSlugcatsEnums.PlacedObjectType.KarmaShrine)
			{
				return DevObjectCategories.Gameplay;
			}
			if (type == MoreSlugcatsEnums.PlacedObjectType.WhiteToken || type == MoreSlugcatsEnums.PlacedObjectType.RedToken || type == MoreSlugcatsEnums.PlacedObjectType.DevToken)
			{
				return DevObjectCategories.Tutorial;
			}
		}
		if (ModManager.Watcher)
		{
			if (type == WatcherEnums.PlacedObjectType.AetherRainbow || type == WatcherEnums.PlacedObjectType.WallLight || type == WatcherEnums.PlacedObjectType.SKLightningRod || type == WatcherEnums.PlacedObjectType.UrbanLife || type == WatcherEnums.PlacedObjectType.UrbanLifePath || type == WatcherEnums.PlacedObjectType.KarmaFlowerPatch || type == WatcherEnums.PlacedObjectType.UrbanCandles || type == WatcherEnums.PlacedObjectType.UrbanCandleHolder)
			{
				return DevObjectCategories.Decoration;
			}
			if (type == WatcherEnums.PlacedObjectType.WeaverSpot || type == WatcherEnums.PlacedObjectType.PrinceBulb || type == WatcherEnums.PlacedObjectType.SpinToy || type == WatcherEnums.PlacedObjectType.BallToy || type == WatcherEnums.PlacedObjectType.SoftToy || type == WatcherEnums.PlacedObjectType.WeirdToy || type == WatcherEnums.PlacedObjectType.ARZapperOmni || type == WatcherEnums.PlacedObjectType.ARZapperVertical || type == WatcherEnums.PlacedObjectType.ARZapperHorizontal || type == WatcherEnums.PlacedObjectType.DaemonClaw || type == WatcherEnums.PlacedObjectType.DaemonEye || type == WatcherEnums.PlacedObjectType.DaemonCrown || type == WatcherEnums.PlacedObjectType.PearlReader)
			{
				return DevObjectCategories.Gameplay;
			}
			if (type == WatcherEnums.PlacedObjectType.SpinningTopSpot || type == PlacedObject.Type.HideVoidSpawn || type == PlacedObject.Type.SpawnMigrationStream)
			{
				return DevObjectCategories.Ripple;
			}
			if (type == WatcherEnums.PlacedObjectType.PlacedBoxWorm || type == WatcherEnums.PlacedObjectType.PlacedFireSprite || type == WatcherEnums.PlacedObjectType.PlacedDrillCrab || type == WatcherEnums.PlacedObjectType.PlacedRats || type == WatcherEnums.PlacedObjectType.PlacedFrog || type == WatcherEnums.PlacedObjectType.PlacedLoach || type == WatcherEnums.PlacedObjectType.PlacedRotLoach || type == WatcherEnums.PlacedObjectType.PlacedBarnacles || type == WatcherEnums.PlacedObjectType.BigSkyWhaleSpawner || type == WatcherEnums.PlacedObjectType.BigSkyWhaleTrigger || type == WatcherEnums.PlacedObjectType.TowerCrabSpawner)
			{
				return DevObjectCategories.Creatures;
			}
			if (type == WatcherEnums.PlacedObjectType.RottenDangleFruit || type == WatcherEnums.PlacedObjectType.RottenSeedCob)
			{
				return DevObjectCategories.Consumable;
			}
		}
		return DevObjectCategories.Unsorted;
	}

	public void AssembleObjectPages()
	{
		maxObjectsPerPage = 22;
		Dictionary<DevObjectCategories, List<PlacedObject.Type>> dictionary = new Dictionary<DevObjectCategories, List<PlacedObject.Type>>();
		foreach (string entry in ExtEnum<DevObjectCategories>.values.entries)
		{
			dictionary[new DevObjectCategories(entry)] = new List<PlacedObject.Type>();
		}
		foreach (string entry2 in ExtEnum<PlacedObject.Type>.values.entries)
		{
			PlacedObject.Type type = new PlacedObject.Type(entry2);
			if (!(type == PlacedObject.Type.None))
			{
				DevObjectCategories key = DevObjectGetCategoryFromPlacedType(type);
				dictionary[key].Add(type);
			}
		}
		int num = 0;
		foreach (string entry3 in ExtEnum<DevObjectCategories>.values.entries)
		{
			DevObjectCategories key2 = new DevObjectCategories(entry3);
			int num2 = (dictionary[key2].Count + maxObjectsPerPage - 1) / maxObjectsPerPage * maxObjectsPerPage;
			while (dictionary[key2].Count < num2)
			{
				dictionary[key2].Add(PlacedObject.Type.None);
			}
			num += num2;
		}
		int num3 = 0;
		placedObjectTypes = new PlacedObject.Type[num];
		foreach (string entry4 in ExtEnum<DevObjectCategories>.values.entries)
		{
			DevObjectCategories key3 = new DevObjectCategories(entry4);
			for (int i = 0; i < dictionary[key3].Count; i++)
			{
				placedObjectTypes[num3] = dictionary[key3][i];
				num3++;
			}
		}
		totalObjectsPages = placedObjectTypes.Length / maxObjectsPerPage;
	}
}
