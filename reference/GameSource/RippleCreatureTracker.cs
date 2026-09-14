using System.Collections.Generic;
using UnityEngine;

public class RippleCreatureTracker : UpdatableAndDeletable
{
	public class RippleCreatureSprite : CosmeticSprite
	{
		public AbstractCreature creature;

		public Vector2 targetPos;

		public RippleCreatureSprite(AbstractCreature creature, Room room)
		{
			base.room = room;
			this.creature = creature;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (room != null && creature != null && creature.Room == room.abstractRoom)
			{
				if (creature.realizedCreature != null)
				{
					pos = creature.realizedCreature.mainBodyChunk.pos;
					targetPos = pos;
				}
				else
				{
					targetPos = new Vector2((float)creature.pos.Tile.x * 20f, (float)creature.pos.Tile.y * 20f);
					pos = new Vector2(Mathf.Lerp(pos.x, targetPos.x, 0.05f), Mathf.Lerp(pos.y, targetPos.y, 0.05f));
				}
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLightBothSides"];
			sLeaser.sprites[0].alpha = 0f;
			sLeaser.sprites[0].anchorX = 0.5f;
			sLeaser.sprites[0].anchorY = 0.5f;
			sLeaser.sprites[0].scale = 10f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			newContatiner = rCam.ReturnFContainer("HUD");
			base.AddToContainer(sLeaser, rCam, newContatiner);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (room == null || creature == null)
			{
				return;
			}
			sLeaser.sprites[0].x = pos.x - camPos.x;
			sLeaser.sprites[0].y = pos.y - camPos.y;
			sLeaser.sprites[0].color = ((creature.realizedCreature != null) ? creature.realizedCreature.ShortCutColor() : creature.creatureTemplate.shortcutColor);
			if (room.game.ActiveRippleLayer == creature.rippleLayer || creature.rippleBothSides || (creature.realizedCreature != null && creature.realizedCreature.inShortcut))
			{
				sLeaser.sprites[0].alpha = Mathf.Lerp(sLeaser.sprites[0].alpha, 0f, 0.05f);
				return;
			}
			if (room.game.ActiveRippleLayer != 0)
			{
				sLeaser.sprites[0].alpha = Mathf.Lerp(sLeaser.sprites[0].alpha, 0.1f, 0.05f);
				return;
			}
			float num = 0f;
			for (int i = 0; i < room.cosmeticRipples.Count; i++)
			{
				float magnitude = (pos - room.cosmeticRipples[i].pos).magnitude;
				if (magnitude <= room.cosmeticRipples[i].displayScale)
				{
					num = 1f;
					break;
				}
				if (magnitude <= room.cosmeticRipples[i].displayScale + 200f)
				{
					float b = 1f - (magnitude - room.cosmeticRipples[i].displayScale) / 200f;
					num = Mathf.Max(num, b);
				}
			}
			sLeaser.sprites[0].alpha = Mathf.Lerp(sLeaser.sprites[0].alpha, 0.4f * num, 0.05f);
		}
	}

	public Dictionary<int, RippleCreatureSprite> creatureTrackers;

	public List<int> idsToRemove;

	public RippleCreatureTracker(Room room)
	{
		base.room = room;
		creatureTrackers = new Dictionary<int, RippleCreatureSprite>();
		idsToRemove = new List<int>();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (!creatureTrackers.ContainsKey(room.abstractRoom.creatures[i].ID.number))
			{
				RippleCreatureSprite rippleCreatureSprite = new RippleCreatureSprite(room.abstractRoom.creatures[i], room);
				room.AddObject(rippleCreatureSprite);
				creatureTrackers[room.abstractRoom.creatures[i].ID.number] = rippleCreatureSprite;
			}
		}
		idsToRemove.Clear();
		foreach (KeyValuePair<int, RippleCreatureSprite> creatureTracker in creatureTrackers)
		{
			RippleCreatureSprite value = creatureTracker.Value;
			if (value.creature == null || value.creature.slatedForDeletion || value.creature.Room != room.abstractRoom)
			{
				idsToRemove.Add(creatureTracker.Key);
			}
		}
		for (int j = 0; j < idsToRemove.Count; j++)
		{
			creatureTrackers[idsToRemove[j]].Destroy();
			room.RemoveObject(creatureTrackers[idsToRemove[j]]);
			creatureTrackers.Remove(idsToRemove[j]);
		}
		if (creatureTrackers.Count <= 0 || (!base.slatedForDeletetion && room != null))
		{
			return;
		}
		foreach (KeyValuePair<int, RippleCreatureSprite> creatureTracker2 in creatureTrackers)
		{
			creatureTracker2.Value.Destroy();
			room.RemoveObject(creatureTracker2.Value);
		}
		creatureTrackers.Clear();
	}
}
