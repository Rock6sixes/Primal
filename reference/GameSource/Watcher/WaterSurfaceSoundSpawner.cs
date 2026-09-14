using System.Collections.Generic;
using UnityEngine;

namespace Watcher;

public class WaterSurfaceSoundSpawner : UpdatableAndDeletable
{
	public class SpawnParameters
	{
		public string[] sounds;

		public float volume = 1f;

		public float pitch = 1f;

		public float radius = 10f;

		public float taper = 0.1f;

		public float doppler;

		public float pitchVariation;

		public float _spaceBetweenSounds = 100f;

		public bool startAtRandomTime = true;

		public float spaceBetweenSounds
		{
			get
			{
				return _spaceBetweenSounds;
			}
			set
			{
				_spaceBetweenSounds = Mathf.Max(value, 100f);
			}
		}
	}

	public SpawnParameters sp;

	public bool soundsSpawned;

	public bool positionsRandomized;

	public bool debug;

	public List<AmbientSoundPlayer> soundPlayers;

	public List<DebugSprite> sprites;

	public bool debugInit;

	public DebugSprite DebugSprite => new DebugSprite(Vector2.zero, new FSprite("Futile_White")
	{
		scale = 1f,
		color = Color.red
	}, room);

	public WaterSurfaceSoundSpawner(SpawnParameters spawnParameters, Room room)
	{
		sp = spawnParameters;
		base.room = room;
		soundPlayers = new List<AmbientSoundPlayer>();
		if (debug)
		{
			sprites = new List<DebugSprite>();
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room != null)
		{
			AddSounds();
			RandomizePositions();
			ResetOnLeavingRoom();
		}
	}

	public void RandomizePositions()
	{
		if (!room.BeingViewed || !soundsSpawned || positionsRandomized || !sp.startAtRandomTime)
		{
			return;
		}
		for (int i = 0; i < soundPlayers.Count; i++)
		{
			if (soundPlayers[i].clip == null)
			{
				return;
			}
			soundPlayers[i].audioSource.time = soundPlayers[i].clip.length * Random.value;
		}
		positionsRandomized = true;
	}

	public void ResetOnLeavingRoom()
	{
		if (!room.BeingViewed)
		{
			if (soundsSpawned)
			{
				soundsSpawned = false;
			}
			if (positionsRandomized)
			{
				positionsRandomized = false;
			}
			soundPlayers.Clear();
		}
	}

	public void AddSounds()
	{
		if (!room.BeingViewed || soundsSpawned)
		{
			return;
		}
		VirtualMicrophone virtualMicrophone = room.game.cameras[0].virtualMicrophone;
		int num = (int)(room.PixelWidth / 20f);
		bool flag = true;
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			Vector2 pos = new Vector2(10f + (float)i * 20f, (float)room.defaultWaterLevel * 20f);
			bool flag2 = flag;
			flag = room.GetTile(pos).IsSolid();
			bool flag3 = room.GetTile(new Vector2(pos.x + 20f, pos.y)).IsSolid();
			if (!flag && (flag2 != flag || flag3 != flag || !(pos.x - num2 < sp.spaceBetweenSounds)))
			{
				AmbientSoundPlayer item = new AmbientSoundPlayer(virtualMicrophone, CreateSound(pos));
				virtualMicrophone.ambientSoundPlayers.Add(item);
				num2 = pos.x;
				AddDebugSprites(pos);
				soundPlayers.Add(item);
			}
		}
		if (debug)
		{
			debugInit = true;
		}
		soundsSpawned = true;
	}

	public void AddDebugSprites(Vector2 pos)
	{
		if (debug && !debugInit)
		{
			DebugSprite debugSprite = DebugSprite;
			debugSprite.pos = pos;
			room.AddObject(debugSprite);
		}
	}

	public SpotSound CreateSound(Vector2 pos)
	{
		int num = Random.Range(0, sp.sounds.Length);
		return new SpotSound(sp.sounds[num], inherited: true)
		{
			type = AmbientSound.Type.Spot,
			rad = sp.radius,
			taper = sp.taper,
			pitch = sp.pitch + (Random.value * 2f - 1f) * sp.pitchVariation,
			volume = sp.volume,
			pos = pos
		};
	}
}
