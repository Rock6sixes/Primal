using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArenaBehaviors;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class SandboxGameSession : ArenaGameSession
{
	public SandboxEditor editor;

	public bool PlayMode;

	public SandboxOverlay overlay;

	public bool overlaySpawned;

	public List<int> playerSpawnDens;

	public bool sandboxInitiated;

	public bool winLoseGameOver;

	public bool checkWinLose;

	public List<SandboxEditor.PlacedIconData> symbolsDataForAfterRoomLoad;

	public List<SandboxEditor.PlacedIconData> symbolsData;

	public List<AbstractCreature> ChallengeKillList;

	public override bool SpawnDefaultRoomItems
	{
		get
		{
			if (PlayMode)
			{
				return base.GameTypeSetup.levelItems;
			}
			return false;
		}
	}

	public SandboxGameSession(RainWorldGame game)
		: base(game)
	{
		PlayMode = arenaSitting.sandboxPlayMode;
		if (!PlayMode)
		{
			arenaSitting.currentLevel = 0;
			if (noRain == null)
			{
				AddBehavior(new NoRain(this));
			}
		}
		else
		{
			arenaSitting.gameTypeSetup.UpdateCustomWinCondition();
			checkWinLose = arenaSitting.players.Count > 0 && arenaSitting.gameTypeSetup.customWinCondition;
		}
	}

	public override void ProcessShutDown()
	{
		base.ProcessShutDown();
		overlay.ShutDownProcess();
	}

	public override void SpawnCreatures()
	{
		base.SpawnCreatures();
		if (PlayMode)
		{
			int currConfNumber = -1;
			symbolsData = null;
			if (ModManager.ChallengeModule && arenaSitting.gameTypeSetup.gameType == DLCSharedEnums.GameTypeID.Challenge)
			{
				symbolsData = ChallengeInformation.LoadChallengeConfiguration(arenaSitting.gameTypeSetup.challengeID);
				ChallengeKillList = new List<AbstractCreature>();
			}
			else
			{
				symbolsData = SandboxEditor.LoadConfiguration(ref currConfNumber, arenaSitting.GetCurrentLevel, game.manager.rainWorld);
			}
			symbolsDataForAfterRoomLoad = new List<SandboxEditor.PlacedIconData>();
			List<CreatureTemplate.Type> list = new List<CreatureTemplate.Type>();
			for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
			{
				if (StaticWorld.creatureTemplates[i].IsLizard)
				{
					MultiplayerUnlocks.SandboxUnlockID unlockID = new MultiplayerUnlocks.SandboxUnlockID(StaticWorld.creatureTemplates[i].type.ToString());
					if (arenaSitting.multiplayerUnlocks.SandboxItemUnlocked(unlockID))
					{
						list.Add(StaticWorld.creatureTemplates[i].type);
					}
				}
			}
			List<int> list2 = new List<int>();
			List<Vector3> list3 = new List<Vector3>();
			for (int j = 0; j < symbolsData.Count; j++)
			{
				if (!(symbolsData[j].data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature) || !(symbolsData[j].data.critType == CreatureTemplate.Type.GreenLizard) || (symbolsData[j].data.intData != 1 && symbolsData[j].data.intData != 2))
				{
					continue;
				}
				for (int k = 0; k < symbolsData.Count; k++)
				{
					if (list.Contains(symbolsData[k].data.critType) && symbolsData[k].data.intData == 0)
					{
						float z = Vector2.Distance(symbolsData[j].pos, symbolsData[k].pos);
						list3.Add(new Vector3(j, k, z));
					}
				}
				EntityID obj = (base.GameTypeSetup.saveCreatures ? symbolsData[j].ID : game.GetNewID());
				Random.State state = Random.state;
				Random.InitState(obj.number);
				symbolsData[j].data.critType = list[Random.Range(0, list.Count)];
				Random.state = state;
			}
			list3 = list3.OrderBy((Vector3 o) => o.z).ToList();
			for (int l = 0; l < list3.Count && !(list3[l].z > 150f); l++)
			{
				int num = (int)list3[l].x;
				int num2 = (int)list3[l].y;
				if (!list2.Contains(num2) && !list2.Contains(num))
				{
					list2.Add(num2);
					list2.Add(num);
					symbolsData[num2].data.intData = symbolsData[num].data.intData;
					symbolsData[num].data.intData = -1;
				}
			}
			for (int m = 0; m < symbolsData.Count; m++)
			{
				SpawnEntity(symbolsData[m]);
			}
			if (ModManager.MSC)
			{
				ChallengeKillList = new List<AbstractCreature>(game.world.GetAbstractRoom(0).creatures);
			}
		}
		else
		{
			bool flag = false;
			if (arenaSitting.players.Count < 1)
			{
				flag = true;
				arenaSitting.players.Add(new ArenaSitting.ArenaPlayer(0));
			}
			for (int n = 0; n < arenaSitting.players.Count; n++)
			{
				AbstractCreature abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, new WorldCoordinate(0, 0, -50, 0), game.GetNewID());
				game.world.GetAbstractRoom(0).AddEntity(abstractCreature);
				abstractCreature.abstractAI.SetDestinationNoPathing(new WorldCoordinate(0, -1, -1, Random.Range(0, game.world.GetAbstractRoom(0).nodes.Length)), migrate: true);
				(abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator = 10 + arenaSitting.players[n].playerNumber;
			}
			if (flag)
			{
				arenaSitting.players.Clear();
			}
		}
	}

	public override void Initiate()
	{
		base.Initiate();
		overlay.Initiate(PlayMode);
		if (PlayMode)
		{
			for (int i = 0; i < 30; i++)
			{
				game.Update();
				counter = 0;
			}
			if (ModManager.MSC && chMeta != null)
			{
				List<int> list = new List<int>();
				list.Add(chMeta.spawnDen);
				if (chMeta.spawnDen2 >= 0)
				{
					list.Add(chMeta.spawnDen2);
				}
				if (chMeta.spawnDen3 >= 0)
				{
					list.Add(chMeta.spawnDen3);
				}
				if (chMeta.spawnDen4 >= 0)
				{
					list.Add(chMeta.spawnDen4);
				}
				SpawnPlayers(base.room, list);
			}
			else
			{
				SpawnPlayers(base.room, playerSpawnDens);
			}
			AddHUD();
		}
		else
		{
			AddHUD();
			editor = new SandboxEditor(this);
			overlay.sandboxEditorSelector.ConnectToEditor(editor);
		}
		sandboxInitiated = true;
		overlay.fadingOut = true;
	}

	public override void Update()
	{
		if (!overlaySpawned && base.room != null)
		{
			overlaySpawned = true;
			base.room.AddObject(new SandboxOverlayOwner(base.room, this, !PlayMode));
		}
		base.Update();
		if (checkWinLose && playersSpawned)
		{
			if (Players.Count == 1)
			{
				if (arenaSitting.players[0].sandboxWin != 0)
				{
					CustomGameOver();
				}
			}
			else
			{
				if (PlayersStillActive(addToAliveTime: false, dontCountSandboxLosers: true) < 2)
				{
					CustomGameOver();
				}
				for (int i = 0; i < arenaSitting.players.Count; i++)
				{
					if (arenaSitting.players[i].sandboxWin > 0)
					{
						CustomGameOver();
						break;
					}
				}
			}
		}
		if (editor != null)
		{
			editor.Update();
		}
		if (PlayMode && base.room != null && symbolsDataForAfterRoomLoad.Count > 0)
		{
			for (int j = 0; j < symbolsDataForAfterRoomLoad.Count; j++)
			{
				SpawnEntityAfterRoomLoad(symbolsDataForAfterRoomLoad[j]);
			}
			symbolsDataForAfterRoomLoad.Clear();
		}
		if (Input.GetKeyDown(KeyCode.Q) && !PlayMode)
		{
			if (game.cameras[0].currentCameraPosition < base.room.cameraPositions.Length - 1)
			{
				game.cameras[0].MoveCamera(game.cameras[0].currentCameraPosition + 1);
			}
			else
			{
				game.cameras[0].MoveCamera(0);
			}
		}
	}

	public void CustomGameOver()
	{
		winLoseGameOver = true;
		outsidePlayersCountAsDead = false;
		checkWinLose = false;
	}

	public override bool ShouldSessionEnd()
	{
		if (arenaSitting.players.Count == 0)
		{
			return game.world.rainCycle.TimeUntilRain < -200;
		}
		if (winLoseGameOver)
		{
			return true;
		}
		if (PlayMode && initiated && sandboxInitiated)
		{
			return thisFrameActivePlayers == 0;
		}
		return false;
	}

	public void SpawnEntity(SandboxEditor.PlacedIconData placedIconData)
	{
		IconSymbol.IconSymbolData data = placedIconData.data;
		if (data.critType == null || data.critType.Index >= StaticWorld.creatureTemplates.Length)
		{
			return;
		}
		WorldCoordinate pos = new WorldCoordinate(0, -1, -1, -1);
		pos.x = Mathf.RoundToInt(placedIconData.pos.x / 20f);
		pos.y = Mathf.RoundToInt(placedIconData.pos.y / 20f);
		EntityID entityID = (base.GameTypeSetup.saveCreatures ? placedIconData.ID : game.GetNewID());
		if (ModManager.Watcher && data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature && (data.critType == WatcherEnums.CreatureTemplateType.SandGrub || data.critType == WatcherEnums.CreatureTemplateType.BigSandGrub))
		{
			game.world.GetAbstractRoom(0).AddArenaSandGrubBurrow(placedIconData.pos, data.critType == WatcherEnums.CreatureTemplateType.BigSandGrub);
		}
		else if (data.itemType == AbstractPhysicalObject.AbstractObjectType.SeedCob)
		{
			symbolsDataForAfterRoomLoad.Add(placedIconData);
		}
		else if (data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature)
		{
			if (data.intData < 0)
			{
				return;
			}
			AbstractCreature abstractCreature = null;
			if (base.GameTypeSetup.saveCreatures)
			{
				for (int i = 0; i < arenaSitting.creatures.Count; i++)
				{
					if (arenaSitting.creatures[i].creatureTemplate.type == data.critType && arenaSitting.creatures[i].ID == entityID)
					{
						abstractCreature = arenaSitting.creatures[i];
						arenaSitting.creatures.RemoveAt(i);
						for (int j = 0; j < 2; j++)
						{
							abstractCreature.state.CycleTick();
						}
						string creatureString = SaveState.AbstractCreatureToStringSingleRoomWorld(abstractCreature);
						abstractCreature = SaveState.AbstractCreatureFromString(game.world, creatureString, onlyInCurrentRegion: false);
						if (abstractCreature != null)
						{
							abstractCreature.pos = pos;
							break;
						}
					}
				}
			}
			if (abstractCreature == null)
			{
				abstractCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(data.critType), null, pos, entityID);
			}
			if (data.intData > 0 && abstractCreature.state != null && abstractCreature.state is LizardState)
			{
				if (data.intData == 1)
				{
					(abstractCreature.state as LizardState).SetRotType(LizardState.RotType.Opossum);
				}
				else if (data.intData == 2)
				{
					(abstractCreature.state as LizardState).SetRotType(LizardState.RotType.Full);
				}
			}
			if (data.critType == WatcherEnums.CreatureTemplateType.SkyWhale && data.intData == 1)
			{
				abstractCreature.superSizeMe = true;
			}
			if (data.critType == CreatureTemplate.Type.Slugcat)
			{
				if (playerSpawnDens == null)
				{
					playerSpawnDens = new List<int>();
				}
				playerSpawnDens.Add(data.intData);
			}
			else if (data.critType == CreatureTemplate.Type.TentaclePlant || data.critType == CreatureTemplate.Type.PoleMimic || (ModManager.DLCShared && data.critType == DLCSharedEnums.CreatureTemplateType.StowawayBug))
			{
				abstractCreature.pos.x = -1;
				abstractCreature.pos.y = -1;
				abstractCreature.pos.abstractNode = data.intData;
				game.world.GetAbstractRoom(0).entitiesInDens.Add(abstractCreature);
			}
			else if (data.critType == CreatureTemplate.Type.Centipede)
			{
				float num = 0f;
				if (data.intData == 2)
				{
					num = Mathf.Lerp(0.265f, 0.55f, Mathf.Pow(Custom.ClampedRandomVariation(0.5f, 0.5f, 0.7f), 1.2f));
				}
				else if (data.intData == 3)
				{
					num = Mathf.Lerp(0.7f, 1f, Mathf.Pow(Random.value, 0.6f));
				}
				abstractCreature.spawnData = string.Format(CultureInfo.InvariantCulture, "{{{0}}}", num);
				abstractCreature.setCustomFlags();
				game.world.GetAbstractRoom(0).AddEntity(abstractCreature);
			}
			else if (data.critType == CreatureTemplate.Type.Fly || data.critType == CreatureTemplate.Type.Spider || data.critType == CreatureTemplate.Type.Leech || data.critType == CreatureTemplate.Type.SeaLeech || (ModManager.DLCShared && data.critType == DLCSharedEnums.CreatureTemplateType.JungleLeech))
			{
				for (int k = 0; k < 5; k++)
				{
					game.world.GetAbstractRoom(0).AddEntity(new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(data.critType), null, pos, entityID));
				}
			}
			else
			{
				game.world.GetAbstractRoom(0).AddEntity(abstractCreature);
			}
		}
		else
		{
			SpawnItems(data, pos, entityID);
		}
	}

	public void SpawnEntityAfterRoomLoad(SandboxEditor.PlacedIconData placedIconData)
	{
		IconSymbol.IconSymbolData data = placedIconData.data;
		WorldCoordinate worldCoordinate = new WorldCoordinate(0, -1, -1, -1);
		worldCoordinate.x = Mathf.RoundToInt(placedIconData.pos.x / 20f);
		worldCoordinate.y = Mathf.RoundToInt(placedIconData.pos.y / 20f);
		EntityID iD = (base.GameTypeSetup.saveCreatures ? placedIconData.ID : game.GetNewID());
		if (data.itemType == AbstractPhysicalObject.AbstractObjectType.SeedCob)
		{
			SeedCob.AbstractSeedCob abstractSeedCob = new SeedCob.AbstractSeedCob(game.world, null, worldCoordinate, iD, -1, -1, dead: false, null);
			abstractSeedCob.placedObjectIndex = -1;
			abstractSeedCob.rotted = data.intData == 1;
			game.world.GetAbstractRoom(0).AddEntity(abstractSeedCob);
			abstractSeedCob.Realize();
			(abstractSeedCob.realizedObject as SeedCob).placedPos = base.room.MiddleOfTile(worldCoordinate);
			abstractSeedCob.realizedObject.PlaceInRoom(base.room);
			if ((abstractSeedCob.realizedObject as SeedCob).stalkLength > 250f)
			{
				base.room.RemoveObject(abstractSeedCob.realizedObject);
				abstractSeedCob.realizedObject.Destroy();
				abstractSeedCob.Destroy();
			}
		}
		else
		{
			AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(game.world, data.itemType, null, worldCoordinate, iD);
			game.world.GetAbstractRoom(0).AddEntity(abstractPhysicalObject);
			abstractPhysicalObject.Realize();
			abstractPhysicalObject.realizedObject.PlaceInRoom(base.room);
		}
	}

	public void SpawnItems(IconSymbol.IconSymbolData data, WorldCoordinate pos, EntityID entityID)
	{
		AbstractPhysicalObject.AbstractObjectType itemType = data.itemType;
		if (itemType == AbstractPhysicalObject.AbstractObjectType.Spear)
		{
			if (data.intData == 3)
			{
				game.world.GetAbstractRoom(0).AddEntity(new AbstractSpear(game.world, null, pos, entityID, explosive: false, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f))));
			}
			else
			{
				game.world.GetAbstractRoom(0).AddEntity(new AbstractSpear(game.world, null, pos, entityID, data.intData == 1, ModManager.MSC && data.intData == 2));
			}
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.WaterNut)
		{
			game.world.GetAbstractRoom(0).AddEntity(new WaterNut.AbstractWaterNut(game.world, null, pos, entityID, -1, -1, null, swollen: false));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
		{
			game.world.GetAbstractRoom(0).AddEntity(new DangleFruit.AbstractDangleFruit(game.world, null, pos, entityID, -1, -1, data.intData == 1, null));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.SporePlant)
		{
			game.world.GetAbstractRoom(0).AddEntity(new SporePlant.AbstractSporePlant(game.world, null, pos, entityID, -1, -1, null, used: false, pacified: true));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.BubbleGrass)
		{
			game.world.GetAbstractRoom(0).AddEntity(new BubbleGrass.AbstractBubbleGrass(game.world, null, pos, entityID, 1f, -1, -1, null));
		}
		else if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
		{
			game.world.GetAbstractRoom(0).AddEntity(new FireEgg.AbstractBugEgg(game.world, null, pos, entityID, Mathf.Lerp(0.35f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f))));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.DataPearl)
		{
			DataPearl.AbstractDataPearl ent = new DataPearl.AbstractDataPearl(game.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, pos, entityID, -1, -1, null, new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries[Random.Range(0, ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.entries.Count)]));
			game.world.GetAbstractRoom(0).AddEntity(ent);
		}
		else if (ModManager.MSC && itemType == MoreSlugcatsEnums.AbstractObjectType.JokeRifle)
		{
			game.world.GetAbstractRoom(0).AddEntity(new JokeRifle.AbstractRifle(game.world, null, pos, entityID, JokeRifle.AbstractRifle.AmmoType.Rock));
		}
		else if (ModManager.DLCShared && itemType == DLCSharedEnums.AbstractObjectType.LillyPuck)
		{
			game.world.GetAbstractRoom(0).AddEntity(new LillyPuck.AbstractLillyPuck(game.world, null, pos, entityID, 3, -1, -1, null));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.Pomegranate)
		{
			game.world.GetAbstractRoom(0).AddEntity(new Pomegranate.AbstractPomegranate(game.world, null, pos, entityID, -1, -1, null, smashed: false, disconnected: false, spearmasterStabbed: false));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.GraffitiBomb)
		{
			game.world.GetAbstractRoom(0).AddEntity(new GraffitiBomb.AbstractGraffitiBomb(game.world, null, pos, entityID, -1, -1, null));
		}
		else if (itemType == AbstractPhysicalObject.AbstractObjectType.VultureMask)
		{
			if (ModManager.MSC && Random.value < 0.25f && (global::MoreSlugcats.MoreSlugcats.chtUnlockCreatures.Value || game.rainWorld.progression.miscProgressionData.GetTokenCollected(MoreSlugcatsEnums.SandboxUnlockID.ScavengerElite)))
			{
				string spriteOverride = "KrakenMask";
				switch (Random.Range(0, 4))
				{
				case 1:
					spriteOverride = "SpikeMask";
					break;
				case 2:
					spriteOverride = "HornedMask";
					break;
				case 3:
					spriteOverride = "SadMask";
					break;
				}
				game.world.GetAbstractRoom(0).AddEntity(new VultureMask.AbstractVultureMask(game.world, null, pos, entityID, entityID.RandomSeed, king: false, scavKing: false, spriteOverride));
			}
			else
			{
				game.world.GetAbstractRoom(0).AddEntity(new VultureMask.AbstractVultureMask(game.world, null, pos, entityID, entityID.RandomSeed, Random.value < 0.25f && (MultiplayerUnlocks.CheckUnlockAll() || game.rainWorld.progression.miscProgressionData.GetTokenCollected(MultiplayerUnlocks.SandboxUnlockID.KingVulture))));
			}
		}
		else if (AbstractConsumable.IsTypeConsumable(data.itemType))
		{
			game.world.GetAbstractRoom(0).AddEntity(new AbstractConsumable(game.world, data.itemType, null, pos, entityID, -1, -1, null));
		}
		else
		{
			game.world.GetAbstractRoom(0).AddEntity(new AbstractPhysicalObject(game.world, data.itemType, null, pos, entityID));
		}
	}
}
