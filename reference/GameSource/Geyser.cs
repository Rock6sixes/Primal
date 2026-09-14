using DevInterface;
using RWCustom;
using Smoke;
using SplashWater;
using UnityEngine;

public class Geyser : UpdatableAndDeletable
{
	public SteamSmoke steam;

	public PlacedObject pObj;

	public int ventDelay;

	public float minPeriod = 2f;

	public float buildupTime = 2f;

	public RectangularDynamicSoundLoop rumble;

	public GeyserData Data => pObj.data as GeyserData;

	public Geyser(PlacedObject placedObject)
	{
		pObj = placedObject;
	}

	public float TimeUntilEruption(int clock)
	{
		float num = room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.GeyserFrequency)?.amount ?? 0.2f;
		float num2 = minPeriod / num;
		float num3 = (float)clock / 40f - Data.timing * num2;
		return Mathf.Ceil(num3 / num2) * num2 - num3;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		float top = (room.PointSubmerged(pObj.pos) ? room.FloatWaterLevel(pObj.pos) : pObj.pos.y);
		FloatRect rect = new FloatRect(pObj.pos.x - 10f, pObj.pos.y, pObj.pos.x + 10f, top);
		if (rumble == null)
		{
			rumble = new RectangularDynamicSoundLoop(this, rect, room);
		}
		rumble.sound = SoundID.Geyser_Rumble_LOOP;
		rumble.rect = rect;
		rumble.Volume = 1f;
		rumble.Pitch = 1f;
		rumble.Update();
		if (steam == null)
		{
			steam = new SteamSmoke(room);
			room.AddObject(steam);
		}
		float num = TimeUntilEruption(room.game.clock);
		float num2 = TimeUntilEruption(room.game.clock - 1);
		if (num < 2f && num2 >= 2f)
		{
			room.PlayRectSound(SoundID.Geyser_Whistle, rect, loop: false, 1f, 1f);
		}
		if (num < buildupTime && ventDelay-- <= 0)
		{
			ventDelay = Random.Range(5, 35);
			if (room.PointSubmerged(pObj.pos))
			{
				for (int i = 0; i < 4; i++)
				{
					Vector2 pos = pObj.pos;
					if (room.GetTile(pos).Solid)
					{
						pos.y = room.MiddleOfTile(pos).y + 11f;
					}
					Vector2 vel = Custom.RotateAroundOrigo(Data.handlePos.normalized, Random.Range(-45f, 45f)) * Random.Range(5f, 13f);
					room.AddObject(new Bubble(pos, vel, bottomBubble: false, fakeWaterBubble: false));
				}
			}
			if (!room.PointSubmerged(pObj.pos, 35f))
			{
				Vector2 vel2 = Custom.RotateAroundOrigo(Data.handlePos.normalized, Random.Range(-45f, 45f)) * Random.Range(12f, 15f);
				steam.EmitSmoke(pObj.pos, vel2, new FloatRect(pObj.pos.x - 200f, pObj.pos.y - 200f, pObj.pos.x + 200f, pObj.pos.y + 200f), 0.2f);
			}
		}
		if (num > num2)
		{
			Erupt();
		}
	}

	public void Erupt()
	{
		Vector2 pos = pObj.pos;
		Vector2 rhs = pos + Data.handlePos;
		for (int i = 0; i < 3; i++)
		{
			steam.EmitSmoke(pObj.pos, Data.handlePos * Random.Range(0.05f, 0.15f), FloatRect.MakeFromVector2(Vector2.Min(pos, rhs), Vector2.Max(pos, rhs)).Grow(200f), 0.6f);
		}
		for (int j = 0; j < 10; j++)
		{
			Vector2 pos2 = pObj.pos;
			Vector2 vel = Custom.RotateAroundOrigo(Data.handlePos.normalized, Random.Range(-20f, 20f)) * Random.Range(5f, 24f);
			pos2.x += Random.Range(-5f, 5f);
			if (room.FloatWaterLevel(pos2) > pos2.y)
			{
				pos2.y = room.FloatWaterLevel(pos2) + 1f;
			}
			room.AddObject(new WaterDrip(pos2, vel, waterColor: true));
		}
		if (room.PointSubmerged(pObj.pos))
		{
			int num = Random.Range(4, 7);
			for (int k = 0; k < num; k++)
			{
				Splash splash = new Splash();
				room.AddObject(splash);
				Vector2 pos3 = pObj.pos;
				pos3.x += Random.Range(-20f, 20f);
				pos3.y = Mathf.Max(pos3.y, room.FloatWaterLevel(pos3));
				Vector2 vel2 = (pos3 + new Vector2(0f, 10f) - pObj.pos).normalized * Random.Range(5f, 20f);
				vel2.x += Random.Range(-2f, 2f);
				splash.Reset(pos3, vel2, 10f + 10f * Random.value, 3f + 5f * Random.value);
			}
		}
		room.AddObject(new GeyserEruption(pObj.pos, Data.handlePos, 60f));
		if (room.PointSubmerged(pObj.pos, -5f))
		{
			room.waterObject.Explosion(pObj.pos + new Vector2(0f, 40f), 200f, 4f);
		}
		room.PlaySound(SoundID.Geyser_Erupt, pObj.pos);
	}
}
