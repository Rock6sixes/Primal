using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class Sandstorm : UpdatableAndDeletable, IDrawable
{
	public bool shelter;

	public Vector2 globalWindDir;

	public Vector2 lastWindScroll;

	public Vector2 windScroll;

	public Vector2 windVel;

	public Vector2 targetWindVel;

	public int windSwitchCounter;

	public float pushIntensity;

	public float targetPushIntensity;

	public float lethality;

	public Texture2D surfaceMask;

	public float[] surfaceMaskValues;

	public static int buildupStartTime = -400;

	public static int buildupEndTime = 2400;

	public static int lethalStartTime = 2200;

	public static int lethalMaxTime = 2800;

	public static float minWindSpeed = 50f;

	public static float maxWindSpeed = 100f;

	public GenericDynamicSoundLoop windLoop;

	public GenericDynamicSoundLoop sandLoop;

	public DisembodiedDynamicSoundLoop rumbleLoop;

	public DisembodiedDynamicSoundLoop screenShakeLoop;

	public float DrownOutSounds => GlobalIntensity;

	public float ScreenShake { get; set; }

	public float MicroScreenShake => Mathf.Min(ScreenShake / 0.6f, 1f);

	public float GlobalIntensity { get; set; }

	public float SurfaceIntensity { get; set; }

	public string SurfaceMaskElement => room.abstractRoom.name + "_SandstormSurfaceMask";

	public Sandstorm(Room room)
	{
		base.room = room;
		shelter = room.abstractRoom.shelter && !room.world.brokenShelters[room.abstractRoom.shelterIndex];
		globalWindDir = Vector2.right;
		windLoop = new GenericDynamicSoundLoop(this, MixWind);
		windLoop.VolumeGroup = 3;
		windLoop.Volume = 0f;
		windLoop.sound = WatcherEnums.WatcherSoundID.Sandstorm_Wind;
		sandLoop = new GenericDynamicSoundLoop(this, MixSand);
		sandLoop.VolumeGroup = 3;
		sandLoop.Volume = 0f;
		sandLoop.sound = WatcherEnums.WatcherSoundID.Sandstorm_Sand;
		rumbleLoop = new DisembodiedDynamicSoundLoop(this);
		rumbleLoop.Volume = 0f;
		rumbleLoop.VolumeGroup = 3;
		rumbleLoop.sound = SoundID.Death_Rain_Rumble_LOOP;
		screenShakeLoop = new DisembodiedDynamicSoundLoop(this);
		screenShakeLoop.Volume = 0f;
		screenShakeLoop.sound = SoundID.Screen_Shake_LOOP;
	}

	public SoundModifiers MixWind(VirtualMicrophone mic, Vector2 listenerPos)
	{
		SoundModifiers neutral = SoundModifiers.Neutral;
		IntVector2 tilePosition = room.GetTilePosition(listenerPos);
		float num = Mathf.Max(Mathf.Sqrt(GlobalIntensity), Mathf.Sqrt(SurfaceIntensity) / 2f);
		if (surfaceMaskValues != null && SurfaceIntensity != GlobalIntensity)
		{
			float num2 = 0f;
			float num3 = 0f;
			int num4 = 0;
			for (int i = -20; i <= 20; i++)
			{
				int num5 = Mathf.Clamp(tilePosition.y + i, 0, room.TileHeight - 1);
				for (int j = -20; j <= 20; j++)
				{
					if (j * j + i * i <= 400)
					{
						int num6 = Mathf.Clamp(tilePosition.x + j, 0, room.TileWidth - 1);
						float num7 = surfaceMaskValues[num6 + num5 * room.TileWidth];
						num2 += num7;
						num3 += num7 * (float)j;
						num4++;
					}
				}
			}
			num3 /= (float)num4;
			num2 /= (float)num4;
			neutral.pan = Mathf.Clamp(num3 / 20f * 2f, -1f, 1f) * 0.75f;
			neutral.volume = Mathf.Lerp(num, Mathf.Sqrt(SurfaceIntensity), num2 * 2f);
		}
		else
		{
			neutral.volume = num;
		}
		neutral.pan += windVel.x / maxWindSpeed * 0.1f;
		return neutral;
	}

	public SoundModifiers MixSand(VirtualMicrophone mic, Vector2 listenerPos)
	{
		return MixWind(mic, listenerPos);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		SurfaceIntensity = room.roomSettings.GetEffectAmount(WatcherEnums.RoomEffectType.SurfaceSandstorm);
		if (shelter)
		{
			GlobalIntensity = 0f;
			ScreenShake = 0f;
			lethality = 0f;
		}
		else
		{
			float num = room.world.rainCycle.timer - room.world.rainCycle.cycleLength;
			if (room.game.IsArenaSession)
			{
				num *= 3.2f;
			}
			ScreenShake = Mathf.InverseLerp(buildupStartTime, (float)buildupEndTime * 0.5f, num);
			if (room.game.IsArenaSession)
			{
				num += 1280f;
			}
			GlobalIntensity = Mathf.Pow(Mathf.InverseLerp(0f, buildupEndTime, num), 2.2f) * room.roomSettings.RainIntensity;
			lethality = Mathf.InverseLerp(lethalStartTime, lethalMaxTime, num) * room.roomSettings.RainIntensity;
			if (room.roomSettings.RainIntensity <= 0.4f)
			{
				lethality = 0f;
			}
			SurfaceIntensity = Mathf.Max(SurfaceIntensity - Mathf.InverseLerp(buildupStartTime, 0f, num), 0f);
		}
		if (SurfaceIntensity > 0f && !surfaceMask)
		{
			GenerateSurfaceMask();
		}
		SurfaceIntensity = Mathf.Max(SurfaceIntensity, GlobalIntensity);
		if (SurfaceIntensity > 0f || GlobalIntensity > 0f)
		{
			DoSandstorm();
		}
		else
		{
			windScroll = default(Vector2);
			windVel = minWindSpeed * globalWindDir;
			pushIntensity = 0f;
			targetPushIntensity = 0f;
		}
		windLoop.Volume = 1f * room.game.rippleFade;
		windLoop.Pitch = windVel.magnitude / maxWindSpeed;
		windLoop.Update();
		sandLoop.Volume = Mathf.Lerp(SurfaceIntensity, pushIntensity, 0.5f) * room.game.rippleFade;
		sandLoop.Pitch = Mathf.Abs(windVel.normalized.x) + GlobalIntensity * 0.2f;
		sandLoop.Update();
		rumbleLoop.Volume = ScreenShake * room.roomSettings.RumbleIntensity * DrownOutSounds * room.game.rippleFade;
		rumbleLoop.Update();
		if (room.game.cameras[0].room == room)
		{
			screenShakeLoop.Volume = room.game.cameras[0].ScreenShake;
		}
		else
		{
			screenShakeLoop.Volume = 0f;
		}
		screenShakeLoop.Update();
	}

	public void DoSandstorm()
	{
		if (windSwitchCounter-- < 0)
		{
			windSwitchCounter = Random.Range(20, 60);
			targetWindVel = Custom.RotateAroundOrigo(globalWindDir, Random.Range(-10f, 10f)) * (Random.Range(minWindSpeed, maxWindSpeed) * Mathf.Lerp(0.65f, 1f, SurfaceIntensity));
		}
		lastWindScroll = windScroll;
		windScroll += windVel;
		windVel = Vector2.Lerp(windVel, targetWindVel, 0.05f);
		if (Random.value < 0.05f)
		{
			targetPushIntensity = Random.value * Mathf.InverseLerp(0.25f, 1f, SurfaceIntensity);
		}
		pushIntensity = Mathf.Lerp(pushIntensity, targetPushIntensity, 0.1f);
		AffectObjects(pushIntensity);
	}

	public void AffectObjects(float amount)
	{
		List<PhysicalObject>[] physicalObjects = room.physicalObjects;
		for (int i = 0; i < physicalObjects.Length; i++)
		{
			foreach (PhysicalObject item in physicalObjects[i])
			{
				if (item.abstractPhysicalObject.rippleLayer != 0 || item.SandstormImmune)
				{
					continue;
				}
				float num = Mathf.Pow(item.windAffectiveness, 0.5f);
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					float num2 = amount * 3f * num;
					if (room.GetTile(bodyChunk.pos).wallbehind)
					{
						num2 *= 0.75f;
					}
					if (bodyChunk.ContactPoint.y < 0)
					{
						num2 *= 0.75f;
					}
					if (surfaceMask != null)
					{
						IntVector2 tilePosition = room.GetTilePosition(bodyChunk.pos);
						float a = surfaceMask.GetPixel(Mathf.Clamp(tilePosition.x, 0, room.TileWidth - 1), Mathf.Clamp(tilePosition.y, 0, room.TileHeight - 1)).a;
						num2 *= Mathf.Lerp(a, 1f, GlobalIntensity);
					}
					if (bodyChunk.ContactPoint.y < 0 && item.bodyChunks.Length == 1 && bodyChunk.rad < 7f && amount < 0.5f)
					{
						num2 = 0f;
					}
					bodyChunk.vel += num2 * Custom.RotateAroundOrigo(globalWindDir, Random.Range(-10f, 10f));
				}
				if (item is Creature creature && lethality > 0f)
				{
					if (Mathf.Pow(Random.value, 1.2f) * 2f < lethality)
					{
						creature.Stun(Random.Range(1, 1 + (int)(9f * lethality)));
					}
					creature.rainDeath += lethality / 20f * Random.Range(0.5f, 1f);
					if (creature.rainDeath > 1f && Random.value < 0.025f)
					{
						creature.Die();
					}
				}
			}
		}
	}

	public void GenerateSurfaceMask()
	{
		Room room = base.room;
		int tileWidth = room.TileWidth;
		int tileHeight = room.TileHeight;
		float[] array = new float[tileWidth * tileHeight];
		float[] array2 = new float[tileWidth * tileHeight];
		new List<IntVector2>();
		int num = ((globalWindDir.x > 0f) ? 1 : (-1));
		int num2 = ((num != 1) ? (tileWidth - 1) : 0);
		int num3 = ((num == 1) ? tileWidth : (-1));
		for (int i = 0; i < tileHeight; i++)
		{
			array[num2 + i * tileWidth] = (room.HasAnySolid(num2, i) ? 0f : 1f);
		}
		float[] array3 = new float[3] { 0.15f, 1f, 0.15f };
		for (int j = num2 + num; j != num3; j += num)
		{
			for (int k = 0; k < tileHeight; k++)
			{
				float num4 = 0f;
				if (!room.HasAnySolid(j, k))
				{
					for (int l = 0; l < array3.Length; l++)
					{
						int num5 = Mathf.Clamp(k + l - array3.Length / 2, 0, tileHeight - 1);
						num4 += array3[l] * array[j - num + num5 * tileWidth];
					}
				}
				array[j + k * tileWidth] = Mathf.Min(num4, 1f);
			}
		}
		for (int m = 0; m < 2; m++)
		{
			for (int n = 0; n < tileHeight; n++)
			{
				for (int num6 = 0; num6 < tileWidth; num6++)
				{
					float num7 = 0f;
					IntVector2[] eightDirectionsAndZero = Custom.eightDirectionsAndZero;
					for (int num8 = 0; num8 < eightDirectionsAndZero.Length; num8++)
					{
						IntVector2 intVector = eightDirectionsAndZero[num8];
						num7 += array[Mathf.Clamp(num6 + intVector.x, 0, tileWidth - 1) + Mathf.Clamp(n + intVector.y, 0, tileHeight - 1) * tileWidth];
					}
					array2[num6 + n * tileWidth] = num7 / 9f;
				}
			}
			float[] array4 = array2;
			float[] array5 = array;
			array = array4;
			array2 = array5;
		}
		if (!Futile.atlasManager.DoesContainElementWithName(SurfaceMaskElement))
		{
			surfaceMask = new Texture2D(tileWidth, tileHeight, TextureFormat.Alpha8, mipChain: false)
			{
				wrapMode = TextureWrapMode.Clamp
			};
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture(SurfaceMaskElement, surfaceMask, textureFromAsset: false);
		}
		else
		{
			surfaceMask = (Texture2D)Futile.atlasManager.GetAtlasWithName(SurfaceMaskElement).texture;
		}
		if (surfaceMask.width != tileWidth || surfaceMask.height != tileHeight)
		{
			surfaceMask.Resize(tileWidth, tileHeight);
		}
		surfaceMask.SetPixelData(array.Select((float x) => (byte)Mathf.Clamp(x * 255f, 0f, 255f)).ToArray(), 0);
		surfaceMask.Apply();
		surfaceMaskValues = array;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White")
		{
			scaleX = 85.4375f,
			scaleY = 48.0625f,
			anchorX = 0f,
			anchorY = 0f,
			shader = rCam.game.rainWorld.Shaders["Sandstorm"]
		};
		AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Shader.SetGlobalColor("_sandstormColor", rCam.terrainPalette?.SandstormColor ?? palette.skyColor);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector4 value = default(Vector4);
		value.x = Mathf.Lerp(lastWindScroll.x, windScroll.x, timeStacker);
		value.y = Mathf.Lerp(lastWindScroll.y, windScroll.y, timeStacker);
		value.z = SurfaceIntensity;
		value.w = 0f;
		Shader.SetGlobalVector("_sandstormParams", value);
		if ((bool)surfaceMask)
		{
			Shader.SetGlobalTexture("_SandstormSurfaceMask", surfaceMask);
		}
		sLeaser.sprites[0].alpha = GlobalIntensity;
		sLeaser.sprites[0].isVisible = GlobalIntensity > 0f || SurfaceIntensity > 0f;
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}
}
