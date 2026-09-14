using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class ARZapper : CosmeticSprite
{
	public class ZapFlash : CosmeticSprite
	{
		public Counter lifeCounter;

		public Color color;

		public float lastLife;

		public float size;

		public float blink;

		public float lastBlink;

		public float sizeVariation;

		public float lastSizeVariation;

		public ZapFlash(Vector2 initPos, float size, Color color)
		{
			this.size = size;
			this.color = color;
			lifeCounter = new Counter((int)(Mathf.Lerp(4f, 12f, Random.value) + 2f * size));
			lastLife = (int)lifeCounter;
			pos = initPos;
			lastPos = initPos;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastLife = lifeCounter.normalized;
			lifeCounter.Tick();
			lastBlink = blink;
			blink = Random.value;
			lastSizeVariation = sizeVariation;
			sizeVariation = Random.value;
			if (lifeCounter.isFinished)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			float num = Mathf.Lerp(lastLife, lifeCounter.normalized, timeStacker);
			sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
			sLeaser.sprites[0].scale = Mathf.Lerp(0.5f, 1f, 0.5f + Mathf.Lerp(lastBlink, blink, timeStacker) * 0.5f * num * size) * 400f * magicScale;
			sLeaser.sprites[0].alpha = num * Mathf.Lerp(lastBlink, blink, timeStacker);
		}
	}

	public class DesintegrationTracker
	{
		public class DesintegrationCandidate
		{
			public int timeToRegen;

			public int time;

			public int strikes;

			public PhysicalObject candidate;

			public DesintegrationCandidate(PhysicalObject candidate, int timeToRegen = 1200)
			{
				this.timeToRegen = timeToRegen;
				time = timeToRegen;
				strikes = 0;
				this.candidate = candidate;
			}
		}

		public List<DesintegrationCandidate> desintegrationCandidates;

		public DesintegrationTracker()
		{
			desintegrationCandidates = new List<DesintegrationCandidate>();
		}

		public void Update()
		{
			if (desintegrationCandidates.Count != 0)
			{
				Regenerate();
				CheckForRemoval();
			}
		}

		public void Regenerate()
		{
			for (int i = 0; i < desintegrationCandidates.Count; i++)
			{
				desintegrationCandidates[i].time--;
				if (desintegrationCandidates[i].time <= 0)
				{
					desintegrationCandidates[i].strikes--;
					desintegrationCandidates[i].time = desintegrationCandidates[i].timeToRegen;
				}
			}
		}

		public void CheckForRemoval()
		{
			for (int num = desintegrationCandidates.Count - 1; num >= 0; num--)
			{
				if (desintegrationCandidates[num].strikes <= 0 || desintegrationCandidates[num].strikes >= 3)
				{
					desintegrationCandidates.RemoveAt(num);
				}
			}
		}

		public int Strike(PhysicalObject obj)
		{
			DesintegrationCandidate desintegrationCandidate = null;
			for (int i = 0; i < desintegrationCandidates.Count; i++)
			{
				if (desintegrationCandidates[i].candidate == obj)
				{
					desintegrationCandidate = desintegrationCandidates[i];
				}
			}
			if (desintegrationCandidate == null)
			{
				desintegrationCandidate = new DesintegrationCandidate(obj);
				desintegrationCandidates.Add(desintegrationCandidate);
			}
			desintegrationCandidate.strikes++;
			return desintegrationCandidate.strikes;
		}
	}

	public IntRect rect;

	public FloatRect floatRect;

	public WallLight[] wallLights;

	public Counter shorted;

	public static float magicScale = 0.0625f;

	public int powerOn;

	public float temp;

	public float coolingDown;

	public float lastTemp;

	public float lastCoolingDown;

	public int direction;

	public bool gravityDependent;

	public bool up;

	public bool down;

	public bool left;

	public bool right;

	public bool oneSide;

	public int nSidesExposed = 4;

	public float lightIntensity = 0.7f;

	public bool searchedForLightRod;

	public bool noDamage;

	public float noise;

	public SSLightRod lightRod;

	public Color rodColor = new Color(0.0186f, 0.808f, 0.93f);

	public Color deathColor = new Color(0.993f, 0.56f, 1f);

	public float disturbanceTime;

	public bool debug;

	public DebugSprite lastContactPointDbg;

	public DebugSprite noDamageIndicatorDbg;

	public RectangularDynamicSoundLoop soundLoop;

	public bool hiddenByRipple => room.game.ActiveRippleLayer != 0;

	public ARZapper(IntRect rect, int direction, Room room)
	{
		this.rect = rect;
		floatRect = new FloatRect((float)rect.left * 20f - 5f, (float)rect.bottom * 20f - 5f, (float)rect.right * 20f + 25f, (float)rect.top * 20f + 25f);
		base.room = room;
		this.direction = direction;
		soundLoop = new RectangularDynamicSoundLoop(this, floatRect, room);
		soundLoop.sound = WatcherEnums.WatcherSoundID.ARZapper_Powered_LOOP;
		soundLoop.Volume = 0f;
		shorted = new Counter(Random.Range(120, 240), 0, countsUp: true);
		shorted.SetToMax();
		disturbanceTime = Random.Range(0f, 50000f);
		CheckSidesConnected();
		IsOneSide();
		SpawnWallLights();
		if (room.game.desintegrationTracker == null)
		{
			room.game.desintegrationTracker = new DesintegrationTracker();
		}
		if (debug)
		{
			lastContactPointDbg = new DebugSprite(floatRect.Center, new FSprite("Futile_White")
			{
				scale = 1f,
				color = Color.yellow,
				rotation = 45f
			}, room);
			room.AddObject(lastContactPointDbg);
			noDamageIndicatorDbg = new DebugSprite(floatRect.Center, new FSprite("Futile_White")
			{
				scale = 0.5f,
				color = (noDamage ? Color.green : Color.red)
			}, room);
			room.AddObject(noDamageIndicatorDbg);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		UpdatePower();
		HandleDamage();
		soundLoop.Volume = Mathf.Lerp(soundLoop.Volume, (!hiddenByRipple) ? powerOn : 0, (powerOn > 0) ? 0.01f : 0.1f);
		soundLoop.Pitch = Mathf.Lerp(soundLoop.Pitch, 0.7f + (float)powerOn * temp * noise * 0.9f, 0.5f);
		soundLoop.Update();
	}

	public void UpdatePower()
	{
		lastTemp = temp;
		lastCoolingDown = coolingDown;
		if (!searchedForLightRod)
		{
			FindLightRod();
		}
		shorted.Tick();
		powerOn = ((room.gravity > 0.5f) ? 1 : 0);
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f)
		{
			powerOn = ((room.world.rainCycle.brokenAntiGrav.to == 1f && room.world.rainCycle.brokenAntiGrav.progress == 1f) ? 1 : 0);
		}
		if (!shorted.isFinished)
		{
			powerOn = 0;
		}
		temp = Mathf.Lerp(temp, powerOn, Mathf.Lerp(0.004f + (1f - temp) * 0.01f, 0.006f, powerOn));
		coolingDown = Mathf.Lerp(coolingDown, 1 - powerOn, Mathf.Lerp(0.1f, 0.002f, temp));
		if (powerOn > 0 && !hiddenByRipple && Random.value > 0.99f)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.ARZapper_Random_Spark, floatRect.RandomPos());
		}
		float normalized = shorted.normalized;
		disturbanceTime += Mathf.InverseLerp(0.11f, 0.1f, normalized);
		float num = FlameJet.AnimatePerlin(disturbanceTime);
		if (lightRod != null)
		{
			Color a = Color.Lerp(rodColor * num, Color.black, normalized);
			lightRod.color = Color.Lerp(a, rodColor, Mathf.InverseLerp(0.8f, 1f, normalized));
		}
	}

	public void HandleDamage()
	{
		if (noDamage || powerOn == 0 || temp < 0.2f)
		{
			return;
		}
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				if (room.physicalObjects[i][j].abstractPhysicalObject.rippleLayer != 0 || IsObjectContactImmune(room.physicalObjects[i][j]) || (room.physicalObjects[i][j] is Creature creature && IsCreatureContactImmune(creature)))
				{
					continue;
				}
				for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
				{
					Vector2 vector = room.physicalObjects[i][j].bodyChunks[k].ContactPoint.ToVector2();
					Vector2 vector2 = room.MiddleOfTile(room.physicalObjects[i][j].bodyChunks[k].pos + vector * (room.physicalObjects[i][j].bodyChunks[k].rad + 15f));
					if (room.GetTile(vector2).IsSolid() && floatRect.Vector2Inside(vector2) && (!(room.physicalObjects[i][j] is Player) || room.cosmeticRipples.Count == 0 || !PositionHiddenByRippleTears(vector2)))
					{
						ZapperContact(room.physicalObjects[i][j].bodyChunks[k], vector2, vector);
						if (debug && room.physicalObjects[i][j] is Player)
						{
							lastContactPointDbg.pos = vector2;
						}
						Short();
						return;
					}
				}
			}
		}
	}

	public bool PositionHiddenByRippleTears(Vector2 testPos)
	{
		for (int i = 0; i < room.cosmeticRipples.Count; i++)
		{
			if (room.cosmeticRipples[i].persistentTear && room.cosmeticRipples[i].PointInRipple(testPos) > 0f)
			{
				return true;
			}
		}
		return false;
	}

	public void ZapperContact(BodyChunk bc, Vector2 zapPos, Vector2 contactPoint)
	{
		if (ModManager.DLCShared && bc.owner is ElectricSpear)
		{
			(bc.owner as ElectricSpear).Recharge();
			return;
		}
		Vector2 vector = -(contactPoint * 6f + Custom.RNV() * Random.value);
		bc.vel += vector / bc.mass;
		room.AddObject(new ZapFlash(zapPos, Mathf.InverseLerp(-0.05f, 15f, bc.rad * 2f), deathColor));
		SparksFromPoint(zapPos - contactPoint * 20f, vector, 8);
		int num = room.game.desintegrationTracker.Strike(bc.owner);
		if (!hiddenByRipple)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.ARZapper_Hit, zapPos);
		}
		if (num >= 3 && !IsObjectIndestructible(bc.owner) && !(bc.owner is Creature { dead: false }))
		{
			if (!hiddenByRipple)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.ARZapper_Disintegrate, zapPos);
			}
			bc.owner.Destroy();
			for (int i = 0; i < bc.owner.bodyChunks.Length; i++)
			{
				BodyChunk bodyChunk = bc.owner.bodyChunks[i];
				room.AddObject(new ZapFlash(bodyChunk.pos, Mathf.InverseLerp(-0.05f, 15f, bc.rad), deathColor));
				SparksFromBodyChunk(bodyChunk, 10);
			}
			return;
		}
		switch (num)
		{
		case 1:
			if (bc.owner is Creature)
			{
				(bc.owner as Creature).Stun(100);
				room.AddObject(new CreatureSpasmer(bc.owner as Creature, allowDead: true, 80));
			}
			SparksFromBodyChunk(bc, 5);
			break;
		case 2:
			if (bc.owner is Creature)
			{
				(bc.owner as Creature).Die();
			}
			SparksFromBodyChunk(bc, 10);
			break;
		}
	}

	public void SparksFromPoint(Vector2 point, Vector2 direction, int number)
	{
		for (int i = 0; i < number; i++)
		{
			room.AddObject(new Spark(point, direction + Custom.RNV() * Random.Range(1f, 3f), deathColor, null, 120, 240));
		}
	}

	public void SparksFromBodyChunk(BodyChunk bc, int number)
	{
		for (int i = 0; i < number; i++)
		{
			Vector2 p = bc.pos + Custom.RNV() * (Random.value * bc.rad);
			room.AddObject(new Spark(p, Custom.DirVec(bc.pos, p) * Random.Range(5, 20), deathColor, null, 120, 240));
		}
	}

	public void Short()
	{
		shorted.SetMax(Random.Range(120, 240));
		shorted.Reset();
	}

	public void FindLightRod()
	{
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (!(room.updateList[i] is SSLightRod sSLightRod) || !floatRect.Vector2Inside(sSLightRod.placedObject.pos + Custom.DegToVec(sSLightRod.rodData.rotation) * (sSLightRod.rodData.length * 0.5f)))
			{
				continue;
			}
			lightRod = sSLightRod;
			lightRod.color = rodColor;
			foreach (SSLightRod.LightVessel light in lightRod.lights)
			{
				light.light.color = rodColor;
			}
			break;
		}
		searchedForLightRod = true;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		Vector2 scale = GetScale(0f);
		sLeaser.sprites[0].scaleX = scale.x;
		sLeaser.sprites[0].scaleY = scale.y;
		sLeaser.sprites[0].rotation = GetRotation();
		sLeaser.sprites[0].shader = GetShader();
		scale = GetScale(80f);
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].scaleX = scale.x;
		sLeaser.sprites[1].scaleY = scale.y;
		sLeaser.sprites[1].rotation = GetRotation();
		sLeaser.sprites[1].shader = Custom.rainWorld.Shaders["ARZapperGlow"];
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 position = rCam.ApplyDepth(floatRect.Center, -3f) - camPos;
		float num = Mathf.Lerp(lastTemp, temp, timeStacker);
		float num2 = Mathf.Lerp(lastCoolingDown, coolingDown, timeStacker);
		Color color = new Color(num, num2, (direction == 1) ? (floatRect.Height / floatRect.Width) : (floatRect.Width / floatRect.Height), 1f);
		sLeaser.sprites[0].SetPosition(position);
		sLeaser.sprites[0].color = color;
		sLeaser.sprites[1].SetPosition(position);
		sLeaser.sprites[1].color = color;
		if (nSidesExposed > 0)
		{
			float num3 = Mathf.Lerp(Mathf.Clamp01(num), Mathf.Clamp01(num * 2f - 1f), num2);
			for (int i = 0; i < nSidesExposed; i++)
			{
				wallLights[i].value = num3 * lightIntensity;
				wallLights[i].hue = 0.99f - num3 * 0.04f;
			}
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		FContainer fContainer = rCam.ReturnFContainer("Foreground");
		FContainer fContainer2 = rCam.ReturnFContainer("ForegroundLights");
		fContainer.AddChild(sLeaser.sprites[0]);
		fContainer2.AddChild(sLeaser.sprites[1]);
	}

	public void SpawnWallLights()
	{
		if (nSidesExposed == 0)
		{
			return;
		}
		WallLight.WallLightData wallLightData = new WallLight.WallLightData(null)
		{
			hue = 0.95f,
			saturation = 0.7f
		};
		wallLights = new WallLight[nSidesExposed];
		int num = 0;
		float num2 = 140f;
		float num3 = num2 * 0.75f;
		if (rect.Width > 0)
		{
			if (!up)
			{
				wallLightData.pos = floatRect.GetCorner(0) + Vector2.down * 20f;
				wallLightData.right = floatRect.GetCorner(1) - wallLightData.pos + Vector2.down * 20f;
				wallLightData.up = Vector2.up * num2 + Vector2.left * num3;
				wallLightData.one = new Vector2(wallLightData.right.x, wallLightData.up.y) + Vector2.right * num3;
				wallLights[num] = new WallLight(room, wallLightData)
				{
					_softEdge = true
				};
				room.AddObject(wallLights[num]);
				num++;
			}
			if (!down)
			{
				wallLightData.pos = floatRect.GetCorner(2) + Vector2.up * 20f;
				wallLightData.right = floatRect.GetCorner(3) - wallLightData.pos + Vector2.up * 20f;
				wallLightData.up = Vector2.down * num2 + Vector2.right * num3;
				wallLightData.one = new Vector2(wallLightData.right.x, wallLightData.up.y) + Vector2.left * num3;
				wallLights[num] = new WallLight(room, wallLightData)
				{
					_softEdge = true
				};
				room.AddObject(wallLights[num]);
				num++;
			}
		}
		if (rect.Height > 0)
		{
			if (!right)
			{
				wallLightData.pos = floatRect.GetCorner(1) + Vector2.left * 20f;
				wallLightData.right = floatRect.GetCorner(2) - wallLightData.pos + Vector2.left * 20f;
				wallLightData.up = Vector2.right * num2 + Vector2.up * num3;
				wallLightData.one = new Vector2(wallLightData.up.x, wallLightData.right.y) + Vector2.down * num3;
				wallLights[num] = new WallLight(room, wallLightData)
				{
					_softEdge = true
				};
				room.AddObject(wallLights[num]);
				num++;
			}
			if (!left)
			{
				wallLightData.pos = floatRect.GetCorner(3) + Vector2.right * 20f;
				wallLightData.right = floatRect.GetCorner(0) - wallLightData.pos + Vector2.right * 20f;
				wallLightData.up = Vector2.left * num2 + Vector2.down * num3;
				wallLightData.one = new Vector2(wallLightData.up.x, wallLightData.right.y) + Vector2.up * num3;
				wallLights[num] = new WallLight(room, wallLightData)
				{
					_softEdge = true
				};
				room.AddObject(wallLights[num]);
				num++;
			}
		}
	}

	public bool IsOneSide()
	{
		if (direction == 2)
		{
			oneSide = false;
			return oneSide;
		}
		oneSide = ((direction != 1) ? (!(up & down)) : (!left || !right));
		return oneSide;
	}

	public FShader GetShader()
	{
		if (direction == 2)
		{
			return Custom.rainWorld.Shaders["ARZapperOmni"];
		}
		if (oneSide)
		{
			return Custom.rainWorld.Shaders["ARZapperOneSide"];
		}
		return Custom.rainWorld.Shaders["ARZapper"];
	}

	public float GetRotation()
	{
		if (direction == 2)
		{
			return 0f;
		}
		return (direction != 1) ? (down ? 180 : 0) : (right ? 90 : 270);
	}

	public Vector2 GetScale(float add)
	{
		Vector2 result = new Vector2(floatRect.Width + add, floatRect.Height + add) * magicScale;
		if (direction == 1)
		{
			result = new Vector2(result.y, result.x);
		}
		return result;
	}

	public void CheckSidesConnected()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 1 + rect.Width;
		int num6 = 1 + rect.Height;
		for (int i = 0; i < num5; i++)
		{
			AddDebugPoint(rect.left + i, rect.top + 1);
			if (room.GetTile(rect.left + i, rect.top + 1).Terrain != 0)
			{
				num++;
			}
		}
		for (int j = 0; j < num5; j++)
		{
			AddDebugPoint(rect.left + j, rect.bottom - 1);
			if (room.GetTile(rect.left + j, rect.bottom - 1).Terrain != 0)
			{
				num2++;
			}
		}
		for (int k = 0; k < num6; k++)
		{
			AddDebugPoint(rect.left - 1, rect.bottom + k);
			if (room.GetTile(rect.left - 1, rect.bottom + k).Terrain != 0)
			{
				num3++;
			}
		}
		for (int l = 0; l < num6; l++)
		{
			AddDebugPoint(rect.right + 1, rect.bottom + l);
			if (room.GetTile(rect.right + 1, rect.bottom + l).Terrain != 0)
			{
				num4++;
			}
		}
		if (num == num5 && num2 == num5 && num3 == num6 && num4 == num6)
		{
			nSidesExposed = 0;
			noDamage = true;
			return;
		}
		up = (float)num > (float)(num5 - 1) * 0.6f;
		down = (float)num2 > (float)(num5 - 1) * 0.6f;
		left = (float)num3 > (float)(num6 - 1) * 0.6f;
		right = (float)num4 > (float)(num6 - 1) * 0.6f;
		nSidesExposed -= ((up || num5 <= 1) ? 1 : 0);
		nSidesExposed -= ((down || num5 <= 1) ? 1 : 0);
		nSidesExposed -= ((left || num6 <= 1) ? 1 : 0);
		nSidesExposed -= ((right || num6 <= 1) ? 1 : 0);
	}

	public static bool IsCreatureContactImmune(Creature creature)
	{
		if (ModManager.Watcher)
		{
			if (creature.abstractCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BoxWorm)
			{
				return true;
			}
			if (creature.abstractCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.FireSprite)
			{
				return true;
			}
			if (creature.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsObjectContactImmune(PhysicalObject obj)
	{
		_ = obj is VoidSpawn;
		return false;
	}

	public static bool IsObjectIndestructible(PhysicalObject po)
	{
		AbstractPhysicalObject.AbstractObjectType type = po.abstractPhysicalObject.type;
		if (type == AbstractPhysicalObject.AbstractObjectType.KarmaFlower || type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl || type == AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer || type == AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer || type == AbstractPhysicalObject.AbstractObjectType.NSHSwarmer)
		{
			return true;
		}
		if (ModManager.MSC && (type == MoreSlugcatsEnums.AbstractObjectType.JokeRifle || type == MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl || type == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl || type == MoreSlugcatsEnums.AbstractObjectType.EnergyCell || type == MoreSlugcatsEnums.AbstractObjectType.MoonCloak))
		{
			return true;
		}
		return false;
	}

	public void AddDebugPoint(int x, int y)
	{
		if (debug)
		{
			bool flag = room.GetTile(x, y).IsSolid();
			room.AddObject(new DebugSprite(room.MiddleOfTile(x, y), new FSprite("Futile_White")
			{
				scale = 0.25f,
				color = (flag ? Color.red : Color.blue)
			}, room));
		}
	}
}
