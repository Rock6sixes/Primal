using System;
using System.Collections.Generic;
using MoreSlugcats;

public class GhostCreatureSedater : UpdatableAndDeletable
{
	public WorldCoordinate? den;

	public List<AbstractCreature> creaturesMovedToDens = new List<AbstractCreature>();

	public GhostCreatureSedater(Room spawnRoom)
	{
		for (int i = 0; i < spawnRoom.abstractRoom.nodes.Length; i++)
		{
			if (spawnRoom.abstractRoom.nodes[i].type == AbstractRoomNode.Type.Den)
			{
				den = new WorldCoordinate(spawnRoom.abstractRoom.index, -1, -1, i);
				break;
			}
		}
		for (int num = spawnRoom.abstractRoom.creatures.Count - 1; num >= 0; num--)
		{
			if (spawnRoom.abstractRoom.creatures[num].creatureTemplate.ghostSedationImmune)
			{
				continue;
			}
			AbstractCreature abstractCreature = spawnRoom.abstractRoom.creatures[num];
			WorldCoordinate? worldCoordinate = den;
			if (abstractCreature.spawnDen.room == spawnRoom.abstractRoom.index)
			{
				worldCoordinate = abstractCreature.spawnDen;
			}
			else if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Fly || abstractCreature.creatureTemplate.type == CreatureTemplate.Type.GarbageWorm || abstractCreature.creatureTemplate.UseAnyRoomBorderExit)
			{
				continue;
			}
			bool flag = !abstractCreature.state.dead;
			foreach (AbstractPhysicalObject.AbstractObjectStick stuckObject in abstractCreature.stuckObjects)
			{
				if (stuckObject is AbstractPhysicalObject.CreatureGripStick && stuckObject.B == abstractCreature)
				{
					flag = false;
					break;
				}
			}
			if (worldCoordinate.HasValue && flag)
			{
				if (abstractCreature.realizedCreature != null)
				{
					abstractCreature.realizedCreature.Destroy();
				}
				abstractCreature.Move(den.Value);
				spawnRoom.abstractRoom.MoveEntityToDen(abstractCreature);
			}
		}
		foreach (AbstractWorldEntity entitiesInDen in spawnRoom.abstractRoom.entitiesInDens)
		{
			if (entitiesInDen is AbstractCreature abstractCreature2)
			{
				creaturesMovedToDens.Add(abstractCreature2);
				abstractCreature2.remainInDenCounter = -1;
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (ModManager.MSC && room.game.rainWorld.safariMode)
		{
			return;
		}
		bool flag = false;
		if (room.world.worldGhost != null && room.world.worldGhost.CreaturesSleepInRoom(room.abstractRoom))
		{
			flag = true;
		}
		if (ModManager.Watcher && !flag)
		{
			for (int i = 0; i < room.world.spinningTopPresences.Count; i++)
			{
				if (room.world.spinningTopPresences[i].CreaturesSleepInRoom(room.abstractRoom) && !room.world.game.GetStorySession.saveState.deathPersistentSaveData.spinningTopEncounters.Contains(room.world.spinningTopPresences[i].spinningTopSpawnId))
				{
					flag = true;
					break;
				}
			}
			if (room.game.IsStorySession && !room.game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility && room.world.weaverPresence != null && room.abstractRoom == room.world.weaverPresence.weaverRoom)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			Destroy();
			return;
		}
		for (int num = room.abstractRoom.creatures.Count - 1; num >= 0; num--)
		{
			AbstractCreature abstractCreature = room.abstractRoom.creatures[num];
			if (!abstractCreature.creatureTemplate.ghostSedationImmune)
			{
				if ((ModManager.MMF && MMF.cfgVanillaExploits.Value) || !abstractCreature.creatureTemplate.UseAnyRoomBorderExit)
				{
					if (abstractCreature.realizedCreature != null)
					{
						abstractCreature.realizedCreature.stun = Math.Max(abstractCreature.realizedCreature.stun, 20);
					}
				}
				else
				{
					if (abstractCreature.realizedCreature != null)
					{
						abstractCreature.realizedCreature.Destroy();
					}
					WorldCoordinate worldCoordinate = new WorldCoordinate(room.world.offScreenDen.index, -1, -1, 0);
					abstractCreature.abstractAI.SetDestination(worldCoordinate);
					abstractCreature.Move(worldCoordinate);
				}
			}
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		for (int i = 0; i < creaturesMovedToDens.Count; i++)
		{
			creaturesMovedToDens[i].remainInDenCounter = 0;
		}
		creaturesMovedToDens.Clear();
	}
}
