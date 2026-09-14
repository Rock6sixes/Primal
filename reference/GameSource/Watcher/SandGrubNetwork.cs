using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Watcher;

public class SandGrubNetwork : UpdatableAndDeletable
{
	public class NetworkData : PlacedObject.ConsumableObjectData
	{
		public float density;

		public float chance;

		public float adultChance;

		public NetworkData(PlacedObject owner)
			: base(owner)
		{
			density = 0.5f;
			chance = 0.5f;
			adultChance = 0.5f;
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			if (array.Length >= 7)
			{
				if (float.TryParse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
				{
					density = result;
				}
				if (float.TryParse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var result2))
				{
					chance = result2;
				}
				if (float.TryParse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture, out var result3))
				{
					adultChance = result3;
				}
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
			}
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}", density, chance, adultChance);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public PlacedObject placedObject;

	public int placedObjectIndex;

	public List<SandGrubBurrow> burrows = new List<SandGrubBurrow>();

	public bool scanForBurrows;

	public bool spawnGrubs;

	public Vector2 pos;

	public float chance;

	public float density;

	public float adultChance;

	public int exactAdultAmount = -1;

	public NetworkData Data => placedObject?.data as NetworkData;

	public bool Consumed => room.game.GetStorySession?.saveState.ItemConsumed(room.world, karmaFlower: false, room.abstractRoom.index, placedObjectIndex) ?? false;

	public int FreeBurrowCount => burrows.Count((SandGrubBurrow burrow) => burrow.grub == null);

	public SandGrubNetwork(PlacedObject placedObject, int placedObjectIndex, bool spawnGrubs)
	{
		this.placedObject = placedObject;
		this.placedObjectIndex = placedObjectIndex;
		this.spawnGrubs = spawnGrubs;
		scanForBurrows = true;
	}

	public SandGrubNetwork(Vector2 pos, float spawnChance, float density, float adultChance, bool spawnGrubs)
	{
		placedObject = null;
		placedObjectIndex = -1;
		scanForBurrows = true;
		this.pos = pos;
		chance = spawnChance;
		this.density = density;
		this.adultChance = adultChance;
		this.spawnGrubs = spawnGrubs;
	}

	public SandGrubNetwork(Vector2 pos, int exactAdultAmount)
	{
		placedObject = null;
		placedObjectIndex = -1;
		scanForBurrows = true;
		this.pos = pos;
		this.exactAdultAmount = exactAdultAmount;
		spawnGrubs = true;
	}

	public override void Update(bool eu)
	{
		if (Data != null)
		{
			pos = placedObject.pos;
			chance = Data.chance;
			density = Data.density;
			adultChance = Data.adultChance;
			if (!MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.BigSandGrub, room.game))
			{
				density *= adultChance;
				adultChance = 0f;
			}
			if (!MultiplayerUnlocks.IsPlacedCreatureAllowedToSpawn(WatcherEnums.CreatureTemplateType.SandGrub, room.game))
			{
				if (adultChance == 0f)
				{
					Destroy();
					return;
				}
				density *= 1f - adultChance;
				adultChance = 1f;
			}
		}
		base.Update(eu);
		if (scanForBurrows)
		{
			scanForBurrows = false;
			{
				foreach (UpdatableAndDeletable update in room.updateList)
				{
					if (update is SandGrubBurrow sandGrubBurrow && (sandGrubBurrow.network == null || Vector2.Distance(sandGrubBurrow.network.placedObject.pos, sandGrubBurrow.pos) > Vector2.Distance(pos, sandGrubBurrow.pos)))
					{
						sandGrubBurrow.network?.burrows.Remove(sandGrubBurrow);
						sandGrubBurrow.SetNetwork(this);
						burrows.Add(sandGrubBurrow);
					}
				}
				return;
			}
		}
		if (burrows.Count == 0)
		{
			Destroy();
		}
		else
		{
			if (!spawnGrubs || !room.readyForAI)
			{
				return;
			}
			spawnGrubs = false;
			if (exactAdultAmount >= 0)
			{
				int num = exactAdultAmount;
				int freeBurrowCount = FreeBurrowCount;
				for (int i = 0; i < freeBurrowCount; i++)
				{
					SpawnGrub(num > 0);
					num--;
				}
			}
			else if (CheckSpawnChance())
			{
				int num2 = Mathf.Min(Mathf.Max(1, Mathf.CeilToInt(density * (float)burrows.Count)), FreeBurrowCount);
				for (int j = 0; j < num2; j++)
				{
					SpawnGrub(j == 0 && UnityEngine.Random.value < adultChance);
				}
			}
		}
	}

	public SandGrubBurrow GetRandomFreeBurrow()
	{
		int num = UnityEngine.Random.Range(0, FreeBurrowCount);
		foreach (SandGrubBurrow burrow in burrows)
		{
			if (burrow.grub == null)
			{
				if (num == 0)
				{
					return burrow;
				}
				num--;
			}
		}
		throw new Exception("Failed to find a free burrow!");
	}

	public List<SandGrubBurrow> GetAllBurrows()
	{
		return burrows;
	}

	public bool OwnsGrub(SandGrub grub)
	{
		if (placedObject == null)
		{
			return true;
		}
		if (grub.State.origRoom == room.abstractRoom.index)
		{
			return grub.State.placedObjectIndex == placedObjectIndex;
		}
		return false;
	}

	public void SpawnGrub(bool big)
	{
		CreatureTemplate.Type type = (big ? WatcherEnums.CreatureTemplateType.BigSandGrub : WatcherEnums.CreatureTemplateType.SandGrub);
		AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(type), null, new WorldCoordinate(room.abstractRoom.index, -1, -1, -1), room.game.GetNewID());
		SandGrubState obj = (SandGrubState)abstractCreature.state;
		obj.origRoom = room.abstractRoom.index;
		obj.placedObjectIndex = placedObjectIndex;
		room.abstractRoom.AddEntity(abstractCreature);
		abstractCreature.RealizeInRoom();
	}

	public bool CheckSpawnChance()
	{
		if (room.game.session is StoryGameSession storyGameSession)
		{
			return room.game.SeededRandom(storyGameSession.saveState.cycleNumber + storyGameSession.saveState.seed + room.abstractRoom.index * 10000) < chance;
		}
		return UnityEngine.Random.value < chance;
	}

	public void MarkConsumed()
	{
		if (room.game.session is StoryGameSession storyGameSession && !Consumed)
		{
			int waitCycles = UnityEngine.Random.Range(Data.minRegen, Data.maxRegen);
			storyGameSession.saveState.ReportConsumedItem(room.world, karmaFlower: false, room.abstractRoom.index, room.roomSettings.placedObjects.IndexOf(placedObject), waitCycles);
		}
	}
}
