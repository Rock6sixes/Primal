using System.Collections.Generic;
using System.Linq;
using RWCustom;
using Smoke;
using UnityEngine;

namespace Watcher;

public class LightningMaker : UpdatableAndDeletable, IDrawable
{
	public class StrikeData
	{
		public Vector2 _pos;

		public int buildupTime;

		public bool isBG;

		public float branchingChance;

		public float effectRadius;

		public float killRadius;

		public bool ready;

		public StaticBuildup staticBuildup;

		public Vector2 pos
		{
			get
			{
				StaticBuildup obj = staticBuildup;
				if (obj == null || !obj.snapToWater)
				{
					return _pos;
				}
				return staticBuildup.pos;
			}
			set
			{
				_pos = value;
			}
		}
	}

	public class StrikeAOE : UpdatableAndDeletable
	{
		public Vector2 pos;

		public float effectRadius;

		public float killRadius;

		public Counter time = new Counter(10);

		public PhysicalObject[] targets;

		public Color explodeColor;

		public bool smokeSpawned;

		public bool hitsWater;

		public Color col;

		public bool hiddenByRipple => room.game.ActiveRippleLayer != 0;

		public StrikeAOE(Vector2 pos, float effectRadius, float killRadius, PhysicalObject[] targets, Room room, Color col)
		{
			base.room = room;
			this.pos = pos;
			this.effectRadius = effectRadius;
			this.killRadius = killRadius;
			explodeColor = new Color(1f, 0.4f, 0.3f);
			hitsWater = pos.y - room.FloatWaterLevel(pos) < 20f;
			this.targets = targets;
			this.col = col;
		}

		public override void Destroy()
		{
			base.Destroy();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if ((int)time == time.max)
			{
				for (int i = 0; i < 25; i++)
				{
					Vector2 vector = Custom.RNV();
					if (room.GetTile(pos + vector * 20f).Solid)
					{
						if (!room.GetTile(pos - vector * 20f).Solid)
						{
							vector *= -1f;
						}
						else
						{
							vector = Custom.RNV();
						}
					}
					for (int j = 0; j < 3; j++)
					{
						room.AddObject(new Spark(pos + vector * Mathf.Lerp(30f, 60f, Random.value), vector * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * (20f * Random.value), Color.Lerp(explodeColor, Color.white, Random.value), null, 11, 28));
					}
					for (int k = 0; k < 20; k++)
					{
						room.AddObject(new Spark(pos, Vector2.up + Custom.RNV() * (40f * (0.1f + Random.value)), Color.Lerp(col, Color.white, Random.value), null, 11, 28));
					}
					room.AddObject(new Explosion.FlashingSmoke(pos + vector * (40f * Random.value), vector * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, Color.white, explodeColor, Random.Range(3, 11)));
					Explosion.ExplosionSmoke explosionSmoke = new Explosion.ExplosionSmoke(pos, Custom.RNV() * (5f * Random.value), 1f);
					explosionSmoke.lifeTime *= 3f;
					room.AddObject(explosionSmoke);
				}
				if (room.waterObject != null)
				{
					room.waterObject.Explosion(new Explosion(room, null, pos, 1, killRadius, 10f, 0f, 0f, 0f, null, 0f, 0f, 0f));
				}
				if (!hiddenByRipple)
				{
					room.PlaySound(WatcherEnums.WatcherSoundID.Lightning_Strike_Instant, pos, 0.9f, Random.Range(0.8f, 1.2f));
				}
			}
			time.Tick();
			if (!hiddenByRipple)
			{
				room.ScreenMovement(pos, default(Vector2), time.normalized * 2f);
			}
			for (int l = 0; l < targets.Length; l++)
			{
				if (targets[l] == null)
				{
					continue;
				}
				Vector2 vector2 = targets[l].firstChunk.pos - pos + Custom.RNV();
				float magnitude = vector2.magnitude;
				float num = 1f - Mathf.Clamp01(magnitude / effectRadius);
				float num2 = 1f - Mathf.Clamp01(magnitude / killRadius);
				if (killRadius != 0f)
				{
					float num3 = num2 * 30f + num * 10f;
					num3 *= time.normalized;
					targets[l].firstChunk.vel += vector2.normalized * num3;
				}
				if (magnitude < killRadius)
				{
					Explosion.ExplosionSmoke explosionSmoke2 = new Explosion.ExplosionSmoke(targets[l].firstChunk.pos, Custom.RNV() * (5f * Random.value), 1f);
					explosionSmoke2.lifeTime *= 3f * num2;
					room.AddObject(explosionSmoke2);
				}
				if (smokeSpawned)
				{
					continue;
				}
				if (killRadius != 0f && targets[l] is Creature creature)
				{
					IntVector2 tilePosition = room.GetTilePosition(pos);
					IntVector2 tilePosition2 = room.GetTilePosition(creature.mainBodyChunk.pos);
					bool flag = false;
					if (hitsWater)
					{
						flag = creature.Submersion != 0f;
					}
					if (room.RayTraceTilesForTerrain(tilePosition.x, tilePosition.y, tilePosition2.x, tilePosition2.y) || flag)
					{
						int num4 = (int)(200f * num);
						float num5 = killRadius;
						if (flag)
						{
							num5 = killRadius * 10f;
							num2 = 1f - Mathf.Clamp01(magnitude / num5);
						}
						if (magnitude < num5)
						{
							if (Random.value < 0.7f)
							{
								creature.Violence(null, null, creature.mainBodyChunk, null, Creature.DamageType.Explosion, 10f * num2, 0f);
							}
							else
							{
								num4 = (int)((float)num4 * (1f + num2));
							}
						}
						if (creature is Player { dead: false } player && magnitude < killRadius * 1.2f)
						{
							player.Deafen((int)(180f * num));
						}
						creature.Stun(num4);
						room.AddObject(new CreatureSpasmer(creature, allowDead: false, num4));
					}
				}
				int num6 = (int)((float)(30 + targets[l].bodyChunks.Length) * Custom.LerpQuadEaseIn(0f, 1f, num)) + 1;
				for (int m = 0; m < num6; m++)
				{
					int num7 = Random.Range(0, targets[l].bodyChunks.Length - 1);
					room.AddObject(new Spark(targets[l].bodyChunks[num7].pos, Vector2.up + Custom.RNV() * (20f * Random.value), Color.Lerp(col, Color.white, Random.value), null, 11, 28));
				}
				if (killRadius != 0f && magnitude < killRadius)
				{
					targets[l].HitByExplosion(10f, null, 0);
				}
				Smolder smolder = new Smolder(room, targets[l].firstChunk.pos, targets[l].firstChunk, null);
				smolder.life = (int)((float)smolder.life * num);
				room.AddObject(smolder);
			}
			smokeSpawned = true;
			if (time.isFinished)
			{
				Destroy();
			}
		}
	}

	public class StaticBuildup : UpdatableAndDeletable
	{
		public class SizzleSound : UpdatableAndDeletable
		{
			public PositionedSoundEmitter sound;

			public StaticBuildup owner;

			public bool hiddenByRipple => room.game.ActiveRippleLayer != 0;

			public SizzleSound(Room room, StaticBuildup owner, BodyChunk chunk)
			{
				this.owner = owner;
				sound = room.PlaySound(WatcherEnums.WatcherSoundID.Electric_Sizzle, chunk);
			}

			public SizzleSound(Room room, StaticBuildup owner, Vector2 pos)
			{
				this.owner = owner;
				sound = new PositionedSoundEmitter(pos, 1f, 1f);
				room.PlaySound(WatcherEnums.WatcherSoundID.Electric_Sizzle, sound, loop: false, 1f, 1f, randomStartPosition: false);
			}

			public override void Update(bool eu)
			{
				base.Update(eu);
				StaticBuildup staticBuildup = owner;
				if (staticBuildup == null || staticBuildup.ready)
				{
					sound.volume = 0f;
					Destroy();
				}
				else if (hiddenByRipple)
				{
					sound.volume = Mathf.Lerp(sound.volume, 0f, 0.1f);
				}
				else
				{
					float num = (owner.timeInfluence + Custom.LerpQuadEaseIn(0f, 0.1f, owner.timeInfluence) + Custom.LerpQuadEaseIn(0f, 0.5f, Mathf.Clamp01(owner.timeInfluence - 0.5f) * 2f)) * 0.5f;
					float num2 = Vector2.Distance(owner.pos, sound.pos) / owner.radius;
					sound.volume = num * (1f - num2);
				}
			}
		}

		public Vector2 pos;

		public int buildupTime;

		public int time;

		public int lastTime;

		public float radius;

		public float killRadius;

		public bool ready;

		public int maxTargets = 10;

		public PhysicalObject[] targets;

		public Counter[] sizzleCounters;

		public Counter sizzleCounter;

		public float[] distance;

		public bool snapToCloseTarget = true;

		public bool snapToWater;

		public IntRect bounds;

		public Color col;

		public float timeInfluence => 1f - (float)time / (float)buildupTime;

		public float lastTimeInfluence => 1f - (float)lastTime / (float)buildupTime;

		public float Influence(int index)
		{
			return 1f - distance[index] / radius;
		}

		public StaticBuildup(Vector2 pos, int buildupTime, float radius, float killRadius, Room room, Color col)
		{
			base.room = room;
			this.pos = pos;
			this.buildupTime = buildupTime;
			time = buildupTime;
			this.radius = radius;
			this.killRadius = killRadius;
			targets = new PhysicalObject[maxTargets];
			sizzleCounters = new Counter[maxTargets];
			for (int i = 0; i < maxTargets; i++)
			{
				sizzleCounters[i] = new Counter(0, 80);
			}
			sizzleCounter = new Counter(0, 80);
			distance = new float[maxTargets];
			IntVector2 tilePosition = room.GetTilePosition(pos);
			int num = (int)(radius / 20f);
			bounds = new IntRect(tilePosition.x - num, tilePosition.y - num, tilePosition.x + num, tilePosition.y + num);
			this.col = col;
			this.pos -= Vector2.up * 10f;
			if (room.GetTile(pos).verticalBeam)
			{
				snapToCloseTarget = false;
				this.pos += Vector2.up * 20f;
			}
		}

		public override void Destroy()
		{
			base.Destroy();
		}

		public bool IsTargetForbidden(PhysicalObject target)
		{
			if (target.abstractPhysicalObject.rippleLayer == 1)
			{
				return true;
			}
			if (target is VoidSpawn)
			{
				return true;
			}
			return false;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (snapToWater)
			{
				pos.y = room.FloatWaterLevel(pos);
			}
			TrackTargets();
			SpawnSparks();
			UpdateMainSizzle();
			lastTime = time;
			if (time > 0)
			{
				time--;
			}
			else if (ready)
			{
				Destroy();
			}
		}

		public Vector2 GetBestTarget()
		{
			Vector2 result = pos;
			if (!snapToCloseTarget)
			{
				return result;
			}
			IntVector2 tilePosition = room.GetTilePosition(pos);
			float num = float.MaxValue;
			for (int i = 0; i < maxTargets; i++)
			{
				if (targets[i] != null && !(distance[i] > killRadius) && !(distance[i] > num))
				{
					IntVector2 tilePosition2 = room.GetTilePosition(targets[i].firstChunk.pos);
					if (room.RayTraceTilesForTerrain(tilePosition.x, tilePosition.y, tilePosition2.x, tilePosition2.y))
					{
						num = distance[i];
						result = targets[i].firstChunk.pos;
					}
				}
			}
			return result;
		}

		public void UpdateMainSizzle()
		{
			sizzleCounter.Tick();
			if (sizzleCounter.isFinished)
			{
				room.AddObject(new SizzleSound(room, this, pos));
				sizzleCounter.Reset();
			}
		}

		public void TrackTargets()
		{
			int num = 0;
			float num2 = timeInfluence;
			for (int i = 0; i < room.physicalObjects.Length; i++)
			{
				for (int j = 0; j < room.physicalObjects[i].Count; j++)
				{
					if (num >= maxTargets)
					{
						break;
					}
					AbstractPhysicalObject abstractPhysicalObject = room.physicalObjects[i][j].abstractPhysicalObject;
					if (abstractPhysicalObject == null || abstractPhysicalObject.realizedObject == null || IsTargetForbidden(abstractPhysicalObject.realizedObject) || !bounds.Includes(abstractPhysicalObject.pos.Tile))
					{
						continue;
					}
					float num3 = Vector2.Distance(pos, abstractPhysicalObject.realizedObject.firstChunk.pos);
					if (!(num3 >= radius))
					{
						targets[num] = abstractPhysicalObject.realizedObject;
						distance[num] = num3;
						float num4 = num2 * Influence(num);
						if (targets[num] is Creature { dead: not false } creature)
						{
							creature.mainBodyChunk.vel += Vector2.up * (num4 * targets[num].TotalMass);
						}
						num++;
					}
				}
			}
			for (int k = num; k < maxTargets; k++)
			{
				targets[k] = null;
			}
		}

		public void SpawnSparks()
		{
			float num = timeInfluence;
			num = Custom.LerpQuadEaseIn(0f, 0.1f, num) + Custom.LerpQuadEaseIn(0f, 0.5f, Mathf.Clamp01(num - 0.5f) * 2f);
			for (int i = 0; i < targets.Length; i++)
			{
				sizzleCounters[i].Tick();
				if (targets[i] == null)
				{
					continue;
				}
				float num2 = Vector2.Distance(pos, targets[i].firstChunk.pos) / radius;
				if (!(Random.value < 1f - Custom.LerpQuadEaseIn(0f, 0.01f, num2) - num * Mathf.Clamp01(1f - num2)) && targets[i].bodyChunks.Length != 0)
				{
					int num3 = Random.Range(0, targets[i].bodyChunks.Length - 1);
					Vector2 vector = Custom.RNV();
					float num4 = (1f - num2) * num;
					room.AddObject(new Spark(targets[i].bodyChunks[num3].pos + targets[i].bodyChunks[num3].rad * vector, Vector2.up + vector * (40f * num4 * Random.value), Color.Lerp(col, Color.white, Random.value), null, (int)(11f * num4), (int)(28f * num4)));
					if (sizzleCounters[i].isFinished)
					{
						room.AddObject(new SizzleSound(room, this, targets[i].firstChunk));
						sizzleCounters[i].Reset();
					}
				}
			}
		}
	}

	public int lightningSpritePool;

	public int staticSpritePool;

	public List<StrikeData> strikeQueue = new List<StrikeData>();

	public List<Vector3> safePlaces = new List<Vector3>();

	public float[] animations;

	public float[] lastAnimations;

	public float[] spriteRotation;

	public Vector2[] spritePos;

	public Vector2[] scale;

	public Vector2[] coord;

	public float blinded;

	public float lastBlinded;

	public float speed = 0.05f;

	public float overlap = 0.25f;

	public float hue;

	public bool beamVisible;

	public Rect screenRect;

	public int[] type;

	public static Vector2[] boltVerts = new Vector2[4]
	{
		new Vector2(-8f, -2f),
		new Vector2(-8f, 18f),
		new Vector2(8f, -2f),
		new Vector2(8f, 18f)
	};

	public static Vector2[] quadVerts = new Vector2[4]
	{
		new Vector2(-8f, -8f),
		new Vector2(-8f, 8f),
		new Vector2(8f, -8f),
		new Vector2(8f, 8f)
	};

	public bool hiddenByRipple => room.game.ActiveRippleLayer != 0;

	public Color mainColor { get; set; }

	public LightningMaker(Room room, int lightningSpritePool = 400, int staticSpritePool = 100)
	{
		base.room = room;
		this.lightningSpritePool = lightningSpritePool;
		this.staticSpritePool = staticSpritePool;
		animations = Enumerable.Repeat(6f, lightningSpritePool).ToArray();
		lastAnimations = new float[lightningSpritePool];
		spriteRotation = new float[lightningSpritePool];
		type = new int[lightningSpritePool];
		spritePos = new Vector2[lightningSpritePool];
		scale = new Vector2[lightningSpritePool];
		coord = new Vector2[lightningSpritePool];
		screenRect = new Rect(0f, 0f, Custom.rainWorld.options.ScreenSize.x, Custom.rainWorld.options.ScreenSize.y);
		SetHue(room.roomSettings.GetEffect(WatcherEnums.RoomEffectType.SKLightningHue)?.amount ?? 0.535f);
	}

	public void SetHue(float hue)
	{
		this.hue = hue;
		mainColor = Custom.HSL2RGB(hue, 1f, 0.5f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastBlinded = blinded;
		RoomSettings.RoomEffect effect = room.roomSettings.GetEffect(WatcherEnums.RoomEffectType.SKLightningHue);
		if (effect != null && hue != effect.amount)
		{
			SetHue(effect.amount);
			Shader.SetGlobalColor("_SKLightningColor", mainColor);
		}
		for (int i = 0; i < strikeQueue.Count; i++)
		{
			StrikeData strikeData = strikeQueue[i];
			if (strikeData.buildupTime > 0)
			{
				strikeData.buildupTime--;
			}
		}
		if (beamVisible && blinded < 2f)
		{
			blinded += 0.1f;
		}
		else if (blinded > 0f)
		{
			blinded -= 0.02f;
		}
		if (ModManager.Watcher && Watcher.cfgDisableLightningFlashes.Value)
		{
			blinded = 0f;
		}
		for (int j = 0; j < lightningSpritePool; j++)
		{
			if (!(animations[j] > 5f))
			{
				lastAnimations[j] = animations[j];
				animations[j] += speed;
				if (type[j] == 1 || type[j] == 4)
				{
					animations[j] += speed;
				}
			}
		}
		for (int num = strikeQueue.Count - 1; num >= 0; num--)
		{
			StrikeData strikeData2 = strikeQueue[num];
			if (strikeData2.buildupTime <= 0)
			{
				if (strikeData2.isBG)
				{
					strikeQueue.RemoveAt(num);
					ArrangeSprites(strikeData2);
					if (!hiddenByRipple)
					{
						room.AddObject(new DelayedThunder(40, 200));
					}
				}
				else if (!strikeData2.ready)
				{
					strikeData2.pos = strikeData2.staticBuildup.GetBestTarget();
					ArrangeSprites(strikeData2);
					strikeData2.buildupTime = 4;
					strikeData2.ready = true;
				}
				else
				{
					strikeData2.staticBuildup.ready = true;
					strikeQueue.RemoveAt(num);
					room.AddObject(new StrikeAOE(strikeData2.pos, strikeData2.effectRadius, strikeData2.killRadius, strikeData2.staticBuildup.targets, room, mainColor));
				}
			}
		}
	}

	public bool IsPosProtected(Vector2 pos)
	{
		for (int i = 0; i < safePlaces.Count; i++)
		{
			if (Vector2.Distance(pos, safePlaces[i]) < safePlaces[i].z)
			{
				return true;
			}
		}
		return false;
	}

	public void Strike(Vector2 pos, float branchingChance, float size, float effectRadius, float killRadius, int delayFrames)
	{
		if (!IsPosProtected(pos) || killRadius == 0f)
		{
			StrikeData strikeData = new StrikeData
			{
				pos = pos,
				branchingChance = branchingChance,
				effectRadius = effectRadius,
				killRadius = killRadius,
				buildupTime = delayFrames,
				staticBuildup = new StaticBuildup(pos, delayFrames, effectRadius, killRadius, room, mainColor)
				{
					snapToWater = room.PointSubmerged(pos)
				}
			};
			strikeData.pos = strikeData.staticBuildup.pos;
			strikeQueue.Add(strikeData);
			room.AddObject(strikeData.staticBuildup);
		}
	}

	public void StrikeBG(Vector2 scrPos, float branchingChance, float size)
	{
		StrikeData item = new StrikeData
		{
			pos = scrPos,
			branchingChance = branchingChance,
			isBG = true
		};
		strikeQueue.Add(item);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		TriangleMesh.Triangle[] tris = TriangleMesh.GridTriangles(1, 1);
		_ = new Vector2[4]
		{
			new Vector2(-8f, -2f),
			new Vector2(-8f, 18f),
			new Vector2(8f, -2f),
			new Vector2(8f, 18f)
		};
		Vector2[] uVvertices = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f)
		};
		sLeaser.sprites = new FSprite[lightningSpritePool + staticSpritePool + 1];
		for (int i = 0; i < lightningSpritePool; i++)
		{
			sLeaser.sprites[i] = new TriangleMesh("Futile_White", tris, customColor: true);
			(sLeaser.sprites[i] as TriangleMesh).vertices = boltVerts;
			(sLeaser.sprites[i] as TriangleMesh).UVvertices = uVvertices;
			sLeaser.sprites[i].isVisible = false;
			sLeaser.sprites[i].shader = Custom.rainWorld.Shaders["SKLightning"];
		}
		for (int j = lightningSpritePool; j < lightningSpritePool + staticSpritePool; j++)
		{
			sLeaser.sprites[j] = new FSprite("Futile_White");
			sLeaser.sprites[j].isVisible = false;
			sLeaser.sprites[j].shader = Custom.rainWorld.Shaders["LightningBolt"];
			sLeaser.sprites[j].anchorY = 0.1f;
			sLeaser.sprites[j].color = mainColor;
		}
		int num = sLeaser.sprites.Length - 1;
		sLeaser.sprites[num] = new FSprite("Futile_White");
		sLeaser.sprites[num].scale = rCam.sSize.x * 0.125f + 8f;
		sLeaser.sprites[num].shader = Custom.rainWorld.Shaders["Darken"];
		sLeaser.sprites[num].SetPosition(rCam.sSize * 0.0625f);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		int num = lightningSpritePool;
		beamVisible = false;
		for (int i = 0; i < strikeQueue.Count; i++)
		{
			if (strikeQueue[i].isBG)
			{
				continue;
			}
			float num2 = Mathf.Lerp(strikeQueue[i].staticBuildup.lastTimeInfluence, strikeQueue[i].staticBuildup.timeInfluence, timeStacker);
			if (strikeQueue[i].staticBuildup.timeInfluence == 0f)
			{
				num2 = 0f;
			}
			for (int j = -1; j < strikeQueue[i].staticBuildup.targets.Length; j++)
			{
				if (num >= sLeaser.sprites.Length - 1)
				{
					break;
				}
				StaticBuildup staticBuildup = strikeQueue[i].staticBuildup;
				if (j < 0)
				{
					sLeaser.sprites[num].isVisible = true;
					sLeaser.sprites[num].SetPosition(strikeQueue[i].pos - camPos);
					sLeaser.sprites[num].scaleY = 6f;
					sLeaser.sprites[num].scaleX = 8f;
					sLeaser.sprites[num].color = new Color((num2 + Custom.LerpExpEaseIn(0f, 1f, num2)) * 0.5f * 1f, 0f, hue);
					num++;
					continue;
				}
				PhysicalObject physicalObject = staticBuildup.targets[j];
				if (physicalObject == null)
				{
					continue;
				}
				float num3 = room.FloatWaterLevel(physicalObject.firstChunk.pos);
				float num4 = physicalObject.firstChunk.pos.y - num3;
				if (!(num4 < -40f))
				{
					Vector2 vector = Vector2.Lerp(physicalObject.firstChunk.lastPos, physicalObject.firstChunk.pos, timeStacker);
					if (num4 < 0f)
					{
						vector.y = num3;
					}
					float num5 = Mathf.InverseLerp(-40f, 0f, num4);
					float num6 = staticBuildup.Influence(j) * num5;
					sLeaser.sprites[num].isVisible = true;
					sLeaser.sprites[num].SetPosition(vector - camPos);
					if (!staticBuildup.ready)
					{
						sLeaser.sprites[num].scaleY = 5f;
						sLeaser.sprites[num].scaleX = 3f;
						sLeaser.sprites[num].color = new Color((num2 + Custom.LerpExpEaseIn(0f, 1f, num2)) * 0.5f * num6, 0f, hue);
					}
					num++;
				}
			}
		}
		for (int k = num; k < sLeaser.sprites.Length - 1; k++)
		{
			sLeaser.sprites[k].isVisible = false;
		}
		for (int l = 0; l < lightningSpritePool; l++)
		{
			FSprite fSprite = sLeaser.sprites[l];
			if (animations[l] > 5f)
			{
				fSprite.isVisible = false;
				continue;
			}
			fSprite.shader = GetShader(type[l]);
			fSprite.SetPosition(spritePos[l] - ((type[l] == 1) ? Vector2.zero : camPos));
			fSprite.rotation = spriteRotation[l];
			fSprite.scaleX = scale[l].x;
			fSprite.scaleY = scale[l].y;
			fSprite.isVisible = true;
			(sLeaser.sprites[l] as TriangleMesh).vertices = boltVerts;
			Color color = fSprite.color;
			float num7 = Mathf.Lerp(lastAnimations[l], animations[l], timeStacker);
			if (type[l] == 2)
			{
				(sLeaser.sprites[l] as TriangleMesh).vertices = quadVerts;
				color.r = 1f;
				color.g = 1f;
				color.b = 1f;
				color.a = ((num7 > 0f) ? Mathf.Clamp01(1f - num7) : 0f);
				fSprite.color = color * room.game.rippleFade;
			}
			else if (type[l] == 3)
			{
				(sLeaser.sprites[l] as TriangleMesh).vertices = quadVerts;
				color.r = mainColor.r;
				color.g = mainColor.g;
				color.b = mainColor.b;
				color = Color.Lerp(Color.white, color, Mathf.Clamp01(num7));
				color.a = ((num7 > 0f) ? (Mathf.Clamp01(1f - num7) * 0.5f + Mathf.Clamp01(1f - num7 * 3f) * 2f) : 0f);
				fSprite.color = color;
			}
			else if (type[l] == 4)
			{
				(sLeaser.sprites[l] as TriangleMesh).vertices = quadVerts;
				fSprite.SetPosition(spritePos[l]);
				color.a = num7 * 0.25f;
				fSprite.color = color;
			}
			else
			{
				color.a = 1f - Mathf.Abs(Mathf.Clamp01(num7 * 0.5f) - 0.5f) * 2f;
				color.r = Mathf.Clamp01(num7);
				color.b = Mathf.InverseLerp(1f, 5f, num7);
				fSprite.color = color;
				(fSprite as TriangleMesh).verticeColors[1].g = coord[l].x;
				(fSprite as TriangleMesh).verticeColors[3].g = coord[l].x;
				(fSprite as TriangleMesh).verticeColors[0].g = coord[l].y;
				(fSprite as TriangleMesh).verticeColors[2].g = coord[l].y;
			}
			Rect localRect = fSprite.localRect;
			localRect.center = spritePos[l];
			if (rCam.RectCurrentlyVisible(localRect, 100f, widescreen: true) && type[l] == 0 && animations[l] < 1f)
			{
				beamVisible = true;
			}
		}
		Shader.SetGlobalVector("_SKLightningBlindness", new Vector2(Mathf.Lerp(lastBlinded, blinded, timeStacker), blinded - lastBlinded));
		sLeaser.sprites[sLeaser.sprites.Length - 1].MoveToBack();
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public FShader GetShader(int type)
	{
		return type switch
		{
			1 => Custom.rainWorld.Shaders["SKLightning"], 
			2 => Custom.rainWorld.Shaders["SKLightningFlash"], 
			3 => Custom.rainWorld.Shaders["FlatLight"], 
			4 => Custom.rainWorld.Shaders["SKLightningBGFlash"], 
			_ => Custom.rainWorld.Shaders["SKLightningForeground"], 
		};
	}

	public float ArrangeSprites(StrikeData strike)
	{
		int num = (strike.isBG ? 20 : ((int)(room.PixelHeight * 1.5f) / 150));
		int num2 = num;
		float num3 = Custom.VecToDeg(Vector2.up);
		Vector2 vector = strike.pos;
		bool flag = false;
		bool flag2 = false;
		int num4 = (strike.isBG ? 25 : 15);
		int number = 0;
		for (int i = 0; i < lightningSpritePool; i++)
		{
			if (animations[i] < 3f)
			{
				continue;
			}
			if (num2 < 0)
			{
				if (strike.isBG)
				{
					type[i] = 4;
					spritePos[i] = vector;
					scale[i] = Vector2.one * 200f;
					animations[i] = 0f;
					lastAnimations[i] = animations[i];
				}
				break;
			}
			if (!flag && !strike.isBG)
			{
				spritePos[i] = strike.pos;
				type[i] = 2;
				scale[i] = Vector2.one * ((float)Futile.screen.pixelWidth * 0.25f);
				animations[i] = 0f;
				lastAnimations[i] = animations[i];
				flag = true;
				continue;
			}
			if (!flag2 && !strike.isBG)
			{
				spritePos[i] = strike.pos;
				type[i] = 3;
				scale[i] = Vector2.one * 50f;
				animations[i] = 0f;
				lastAnimations[i] = animations[i];
				flag2 = true;
				continue;
			}
			float num5;
			if (num2 == num)
			{
				num5 = num3;
				vector += Vector2.down * 20f;
			}
			else
			{
				num5 = num3 + (float)Random.Range(-num4, num4);
			}
			Vector2 vector2 = vector + Custom.DegToVec(num5) * (strike.isBG ? 60 : 150);
			if (IsSegmentVisible(vector, vector2, strike.isBG))
			{
				spriteRotation[i] = num5;
				spritePos[i] = vector;
				type[i] = (strike.isBG ? 1 : 0);
				coord[i] = new Vector2((float)num2 / (float)(num + 1), (float)(num2 + 1) / (float)(num + 1));
				scale[i] = new Vector2(strike.isBG ? 3f : 16f, (vector2 - vector).magnitude * 0.0625f);
				animations[i] = 0f;
				lastAnimations[i] = animations[i];
				number++;
			}
			vector = vector2;
			num3 = num5;
			if (Random.value < strike.branchingChance)
			{
				Branch(strike, ref i, ref number, vector, num3, num2, num);
			}
			num2--;
		}
		return 1f / speed;
	}

	public bool IsSegmentVisible(Vector2 a, Vector2 b, bool isBG)
	{
		_ = room.game.cameras[0];
		if (isBG)
		{
			if (!screenRect.Contains(a))
			{
				return screenRect.Contains(b);
			}
			return true;
		}
		if (!room.RoomRect.Vector2Inside(a))
		{
			return room.RoomRect.Vector2Inside(b);
		}
		return true;
	}

	public void Branch(StrikeData strike, ref int i, ref int number, Vector2 start, float rotation, int segNum, int total)
	{
		rotation += (float)Random.Range(25, 45) * Mathf.Sign(Random.value - 0.5f);
		int num = total - 2;
		if (segNum > num)
		{
			return;
		}
		float num2 = rotation;
		Vector2 vector = start;
		while (i < lightningSpritePool)
		{
			if (animations[i] < 3f)
			{
				i++;
				continue;
			}
			float num3 = num2 + (float)Random.Range(-25, 25);
			Vector2 vector2 = Custom.DegToVec(num3) * (strike.isBG ? 40 : 140);
			Vector2 vector3 = vector - vector2;
			if (IsSegmentVisible(vector, vector3, strike.isBG))
			{
				spriteRotation[i] = num3;
				spritePos[i] = vector3;
				type[i] = (strike.isBG ? 1 : 0);
				animations[i] = 0f;
				lastAnimations[i] = animations[i];
				coord[i] = new Vector2((float)segNum / (float)(total + 1), (float)(segNum + 1) / (float)(total + 1));
				scale[i] = new Vector2(strike.isBG ? 2f : 12f, (vector3 - vector).magnitude * 0.0625f);
				number++;
			}
			vector = vector3;
			num2 = num3;
			segNum++;
			if (Random.value < strike.branchingChance)
			{
				Branch(strike, ref i, ref number, vector, num2, segNum, num);
			}
			if (segNum <= total - 1)
			{
				continue;
			}
			break;
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		FContainer fContainer = rCam.ReturnFContainer("GrabShaders");
		int num = sLeaser.sprites.Length;
		for (int i = 0; i < num - 1; i++)
		{
			fContainer.AddChildAtIndex(sLeaser.sprites[i], 0);
		}
		fContainer.AddChild(sLeaser.sprites[num - 1]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Shader.SetGlobalColor("_SKLightningColor", mainColor);
	}
}
