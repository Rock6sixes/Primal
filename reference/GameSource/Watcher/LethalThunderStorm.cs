using RWCustom;
using UnityEngine;

namespace Watcher;

public class LethalThunderStorm : UpdatableAndDeletable
{
	public float amount;

	public float lethalAmount;

	public float cosmeticAmount;

	public Counter lethal;

	public Counter cosmetic;

	public RoomSettings.RoomEffect effect;

	public float cosmeticAmountFinal => 1f - amount * cosmeticAmount;

	public float lethalAmountFinal => 1f - amount * lethalAmount;

	public int GetCosmeticDelay(float amount)
	{
		return Random.Range(20, 120 + (int)(4800f * amount));
	}

	public int GetLethalDelay(float amount)
	{
		return Random.Range(120, 280 + (int)(1200f * amount));
	}

	public LethalThunderStorm(RoomSettings.RoomEffect effect, Room room)
	{
		base.room = room;
		this.effect = effect;
		lethal = new Counter(160);
		lethal.SetToMin();
		cosmetic = new Counter(40);
		cosmetic.SetToMin();
		UpdateEffectParameters();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		UpdateEffectParameters();
		lethal.Tick();
		cosmetic.Tick();
		TrySpawnLethalStrike();
		TrySpawnCosmeticStrike();
	}

	public void UpdateEffectParameters()
	{
		float num = lethalAmount;
		float num2 = cosmeticAmount;
		amount = effect.GetAmount(0);
		lethalAmount = effect.GetAmount(1);
		cosmeticAmount = effect.GetAmount(2);
		if (num != lethalAmount)
		{
			lethal.SetMax(GetLethalDelay(lethalAmountFinal));
			lethal.SetToMax();
		}
		if (num2 != cosmeticAmount)
		{
			cosmetic.SetMax(GetCosmeticDelay(cosmeticAmountFinal));
			cosmetic.SetToMax();
		}
	}

	public void TrySpawnCosmeticStrike()
	{
		if (cosmetic.isFinished && cosmeticAmount != 0f && amount != 0f)
		{
			if (room.lightningMaker == null)
			{
				room.AddObject(room.lightningMaker = new LightningMaker(room));
			}
			Vector2 scrPos = new Vector2((float)Futile.screen.pixelWidth * 0.5f + (float)Random.Range(-300, 300), (float)Futile.screen.pixelHeight * 0.5f + (float)Random.Range(-200, 0));
			room.lightningMaker.StrikeBG(scrPos, 0.3f, 1f);
			cosmetic.SetMax(GetCosmeticDelay(cosmeticAmountFinal));
			cosmetic.SetToMax();
		}
	}

	public void TrySpawnLethalStrike()
	{
		if (lethal.isFinished && lethalAmount != 0f && amount != 0f)
		{
			if (room.lightningMaker == null)
			{
				room.AddObject(room.lightningMaker = new LightningMaker(room));
			}
			Vector2? vector = FindStrikePosition(20);
			if (vector.HasValue)
			{
				Vector2 valueOrDefault = vector.GetValueOrDefault();
				room.lightningMaker.Strike(valueOrDefault, 0f, 1f, 800f, 200f, 400);
				lethal.SetMax(GetLethalDelay(lethalAmountFinal));
				lethal.SetToMax();
			}
		}
	}

	public Vector2? FindStrikePosition(int numberOfTries)
	{
		for (int i = 0; i < numberOfTries; i++)
		{
			IntVector2 pos = new IntVector2(Random.Range(0, room.TileWidth), room.TileHeight - 1);
			pos.y = room.aimap.getAItile(pos).fallRiskTile.y;
			if (pos.y <= 0)
			{
				if (room.PointSubmerged(room.MiddleOfTile(pos)))
				{
					return room.MiddleOfTile(pos);
				}
				continue;
			}
			bool flag = false;
			for (int num = room.TileHeight; num > pos.y; num--)
			{
				if (room.GetTile(pos.x, num).verticalBeam)
				{
					return room.MiddleOfTile(pos);
				}
				if (room.GetTile(pos.x, num).horizontalBeam && (double)Random.value > 0.8)
				{
					return room.MiddleOfTile(pos);
				}
				for (int j = 1; j < 8; j++)
				{
					IntVector2 pos2 = new IntVector2(pos.x - j, num);
					if (room.GetTile(pos2).verticalBeam)
					{
						return room.MiddleOfTile(pos2);
					}
					Room obj = room;
					pos2 = new IntVector2(pos.x + j, num);
					if (obj.GetTile(pos2).verticalBeam)
					{
						return room.MiddleOfTile(pos2);
					}
				}
				if (room.GetTile(pos.x, num).IsSolid())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return room.MiddleOfTile(pos);
			}
		}
		return null;
	}
}
