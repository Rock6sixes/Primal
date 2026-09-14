using Smoke;
using UnityEngine;

public class HarmfulSteam : UpdatableAndDeletable
{
	public PlacedObject placedObject;

	public PlacedObject.HarmfulSteamData data;

	public float durationTarget;

	public float frequencyTarget;

	public float duration;

	public float frequency;

	public float lifetime;

	public float dangerRange;

	public Vector2 fromPos;

	public Vector2 toPos;

	public Vector2[] steamZone;

	public SteamSmoke steam;

	public RectangularDynamicSoundLoop soundLoop;

	public HarmfulSteam(PlacedObject pObj, Room room)
	{
		placedObject = pObj;
		data = placedObject.data as PlacedObject.HarmfulSteamData;
		base.room = room;
		durationTarget = data.duration;
		frequencyTarget = data.frequency;
		lifetime = data.lifetime;
		fromPos = placedObject.pos;
		toPos = data.handlePos;
		steam = new SteamSmoke(room);
		soundLoop = new RectangularDynamicSoundLoop(this, new FloatRect(fromPos.x - 20f, fromPos.y - 20f, fromPos.x + 20f, fromPos.y + 20f), room);
		soundLoop.sound = SoundID.Gate_Water_Steam_LOOP;
		soundLoop.Volume = 0f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		durationTarget = data.duration;
		frequencyTarget = data.frequency;
		lifetime = data.lifetime;
		dangerRange = 0.9f;
		fromPos = placedObject.pos;
		toPos = data.handlePos;
		if (room.VoidWeaverActive)
		{
			duration = 0f;
			frequency = 0f;
		}
		else if (soundLoop != null)
		{
			if (soundLoop.Volume > 0f)
			{
				soundLoop.Update();
			}
			frequency += 0.0025f;
			if (frequency >= frequencyTarget)
			{
				duration += 0.0025f;
				if (room.game.ActiveRippleLayer == 0)
				{
					soundLoop.Volume += 0.024999999f;
					soundLoop.Volume = Mathf.Clamp(soundLoop.Volume, 0f, 0.8f);
				}
				steam.EmitSmoke(fromPos, toPos * 0.25f, room.RoomRect, lifetime);
				if (duration >= durationTarget)
				{
					duration = 0f;
					frequency = 0f;
				}
			}
			else
			{
				soundLoop.Volume -= 0.012499999f;
				if (soundLoop.Volume <= 0f)
				{
					soundLoop.Volume = 0f;
					if (soundLoop.emitter != null)
					{
						soundLoop.emitter.alive = false;
						soundLoop.emitter = null;
					}
				}
			}
		}
		for (int i = 0; i < steam.particles.Count; i++)
		{
			if (!(steam.particles[i].life > dangerRange))
			{
				continue;
			}
			for (int j = 0; j < room.physicalObjects.Length; j++)
			{
				for (int k = 0; k < room.physicalObjects[j].Count; k++)
				{
					for (int l = 0; l < room.physicalObjects[j][k].bodyChunks.Length; l++)
					{
						Vector2 vector = room.physicalObjects[j][k].bodyChunks[l].ContactPoint.ToVector2();
						Vector2 b = room.physicalObjects[j][k].bodyChunks[l].pos + vector * (room.physicalObjects[j][k].bodyChunks[l].rad + 30f);
						if (Vector2.Distance(steam.particles[i].pos, b) < 10f && room.physicalObjects[j][k] is Creature && (room.physicalObjects[j][k] as Creature).abstractCreature.rippleLayer == 0 && (room.physicalObjects[j][k] as Creature).stun == 0)
						{
							(room.physicalObjects[j][k] as Creature).stun = 100;
							room.AddObject(new CreatureSpasmer(room.physicalObjects[j][k] as Creature, allowDead: false, (room.physicalObjects[j][k] as Creature).stun));
							room.PlaySound(SoundID.Gate_Water_Steam_Puff, (room.physicalObjects[j][k] as Creature).mainBodyChunk, loop: false, 1.1f, 1f);
							room.PlaySound(SoundID.Big_Spider_Spit_Warning_Rustle, (room.physicalObjects[j][k] as Creature).mainBodyChunk, loop: false, 1f, 1f);
							return;
						}
					}
				}
			}
		}
	}
}
