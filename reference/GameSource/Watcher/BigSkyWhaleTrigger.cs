using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BigSkyWhaleTrigger : UpdatableAndDeletable
{
	public class Data : PlacedObject.ResizableObjectData
	{
		public Vector2 panelPos;

		public int direction;

		public int waitCycles;

		public Data(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			direction = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
			waitCycles = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
			unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}", panelPos.x, panelPos.y, direction, waitCycles);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public PlacedObject pObj;

	public Data TriggerData => pObj.data as Data;

	public bool AnyPlayersInRange
	{
		get
		{
			foreach (AbstractCreature player2 in room.game.Players)
			{
				if (player2?.realizedCreature is Player player && player.room == room && Custom.DistLess(player.mainBodyChunk.pos, pObj.pos, TriggerData.Rad))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool AnyWhalesInRoom
	{
		get
		{
			foreach (UpdatableAndDeletable update in room.updateList)
			{
				if (update is BigSkyWhale)
				{
					return true;
				}
			}
			return false;
		}
	}

	public BigSkyWhaleTrigger(Room room, PlacedObject pObj)
	{
		base.room = room;
		this.pObj = pObj;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (AnyPlayersInRange && !AnyWhalesInRoom)
		{
			SpawnSkyWhale();
			MarkConsumed();
			Destroy();
		}
	}

	public void MarkConsumed()
	{
		if (room.world.game.session is StoryGameSession storyGameSession)
		{
			int num = room.roomSettings.placedObjects.IndexOf(pObj);
			if (num >= 0)
			{
				storyGameSession.saveState.ReportConsumedItem(room.world, karmaFlower: false, room.abstractRoom.index, num, TriggerData.waitCycles);
			}
		}
	}

	public void SpawnSkyWhale()
	{
		List<Vector2> path = BigSkyWhaleSpawner.GetPath(room, TriggerData.direction);
		if (path.Count != 0)
		{
			room.AddObject(new BigSkyWhale(path));
			Destroy();
		}
	}
}
