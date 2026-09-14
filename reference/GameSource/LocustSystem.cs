using System;
using System.Collections.Generic;
using System.IO;
using DevInterface;
using RWCustom;
using UnityEngine;
using Watcher;

public class LocustSystem : UpdatableAndDeletable, IDrawable
{
	public class GroundLocust
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float seed;

		public float depth;

		public bool alive;

		public Vector2? groundNormal;

		public Vector2 facing;

		public float lastDepth;

		public int poisonCounter;

		public bool flyAway;

		public bool scared;

		public Swarm swarm;

		public BodyChunk attachedToChunk;

		public int burrowCounter;

		public int rejoinCloudCounter;

		public int idleTime;

		public bool lastContact;

		public Vector2 swarmMotion;

		public Vector2 attachmentPos;

		public int lungeCooldown;

		public const int burrowDuration = 30;

		public const int poisonDuration = 240;

		public const int maxLungeCooldown = 120;

		public const float drag = 0.98f;

		public const float swarmDrag = 0.93f;

		public float TargetHeight => Custom.PushFromHalf(seed * 100f % 1f, 0.5f);

		public bool Poisoned
		{
			get
			{
				return poisonCounter > 0;
			}
			set
			{
				if (poisonCounter == 0)
				{
					poisonCounter = 1;
				}
			}
		}

		public GroundLocust(Vector2 pos, float seed, float depth)
		{
			this.pos = pos;
			lastPos = pos;
			vel = Vector2.zero;
			this.seed = seed;
			this.depth = depth;
			alive = true;
			facing = Vector2.right;
			lastDepth = depth;
			lungeCooldown = UnityEngine.Random.Range(0, 120);
		}

		public GroundLocust(CloudLocust cloudLocust)
			: this(cloudLocust.pos, cloudLocust.seed, cloudLocust.depth)
		{
			lastPos = cloudLocust.lastPos;
			vel = cloudLocust.vel;
		}

		public void Idle(int time)
		{
			idleTime = time;
		}

		public void Update(LocustSystem owner)
		{
			lastPos = pos;
			lastDepth = depth;
			depth = Custom.LerpAndTick(depth, 0f, 0.05f, 0.01f);
			if (burrowCounter > 0)
			{
				burrowCounter++;
				pos -= (groundNormal ?? Vector2.up) * 0.3f;
				if (burrowCounter > 30)
				{
					alive = false;
				}
			}
			if (!scared)
			{
				scared = owner.IsWhaleNear(pos);
			}
			if (scared)
			{
				foreach (BigSkyWhale bigWhale in owner.bigWhales)
				{
					if (bigWhale.MouthContains(pos))
					{
						alive = false;
					}
				}
			}
			if (attachedToChunk == null && burrowCounter == 0)
			{
				DoPhysics(owner);
			}
			if (swarm != null)
			{
				if (swarm != null && attachedToChunk != null)
				{
					DoAttached(owner);
				}
				else
				{
					DoSwarming(owner);
				}
			}
			else
			{
				attachedToChunk = null;
			}
			if ((swarm != null && (Custom.DistLess(pos, swarm.center, 100f) || owner.room.VisualContact(pos, swarm.center))) || Poisoned)
			{
				rejoinCloudCounter = 0;
			}
			else
			{
				rejoinCloudCounter++;
			}
			if (idleTime > 0)
			{
				rejoinCloudCounter = 0;
				idleTime--;
			}
			if (Poisoned)
			{
				poisonCounter++;
			}
			if (swarm != null && (rejoinCloudCounter > 80 || Poisoned || scared))
			{
				swarm.RemoveLocust(this);
			}
			IntVector2 tilePosition = owner.room.GetTilePosition(pos + new Vector2(0f, 10f));
			flyAway = swarm == null && (rejoinCloudCounter > 40 || scared) && !groundNormal.HasValue && owner.room.RayTraceTilesForTerrain(tilePosition.x, tilePosition.y, tilePosition.x, owner.room.TileHeight - 1) && !Poisoned;
			if (flyAway && owner.CloudLine(pos.x, 0.1f, isCluster: false) < pos.y)
			{
				owner.AddCloudLocust(this);
				alive = false;
			}
			if (pos.x < owner.minX - 100f || pos.x > owner.maxX + 100f || pos.y > owner.room.PixelHeight + 100f || poisonCounter > 240 || rejoinCloudCounter > 320)
			{
				alive = false;
			}
		}

		public void DoPhysics(LocustSystem owner)
		{
			vel *= ((swarm != null) ? 0.93f : 0.98f);
			if (Poisoned)
			{
				vel.y -= 1f;
			}
			else if (swarm == null)
			{
				vel.y -= 0.35f + 0.25f * (seed * 100f % 1f);
			}
			if (!Poisoned)
			{
				foreach (AvoidPoint avoidPoint in owner.avoidPoints)
				{
					float num = Vector2.SqrMagnitude(pos - avoidPoint.pos);
					if (!(num > avoidPoint.radius * avoidPoint.radius))
					{
						Vector2 vector = (pos - avoidPoint.pos) / num;
						num = Mathf.Sqrt(num);
						vel += vector * (Mathf.Max(avoidPoint.radius - num, 30f) * 0.12f);
					}
				}
			}
			bool flag = false;
			ref Vector2? reference = ref groundNormal;
			if ((reference.HasValue && reference.GetValueOrDefault().y > 0f) || groundNormal.HasValue)
			{
				if (!Poisoned)
				{
					vel = Vector2.zero;
				}
				flag = true;
			}
			pos += vel;
			SharedPhysics.TerrainCollisionData cd = new SharedPhysics.TerrainCollisionData(pos, lastPos, vel, 2f, default(IntVector2), goThroughFloors: true);
			cd = SharedPhysics.VerticalCollision(owner.room, cd);
			cd = SharedPhysics.SlopesVertically(owner.room, cd);
			cd = SharedPhysics.HorizontalCollision(owner.room, cd);
			pos = cd.pos;
			vel = cd.vel;
			if (cd.contactPoint != default(IntVector2) && swarm == null)
			{
				if (cd.contactPoint == new IntVector2(0, -1) && owner.room.terrain != null && Vector2.Distance(owner.room.terrain.SnapToTerrain(pos, cd.rad, out var normal, lastPos), pos) < 2f)
				{
					groundNormal = normal;
				}
				else
				{
					groundNormal = -cd.contactPoint.ToVector2().normalized;
				}
				Vector2 vector2 = Custom.PerpendicularVector(groundNormal.Value);
				if (Vector2.Dot(facing, vector2) < 0f)
				{
					facing = -vector2;
				}
				else
				{
					facing = vector2;
				}
				flag = true;
			}
			else
			{
				Vector2 vector3 = Vector3.Slerp(vel.normalized, new Vector2(Mathf.Sign(vel.x), 0f), 0.5f);
				if (vel.sqrMagnitude > 0.0001f)
				{
					facing = Vector3.Slerp(facing, vector3.normalized, 0.2f);
				}
			}
			if (flyAway)
			{
				vel.y += (scared ? 0.7f : 1.25f);
			}
			float num2 = (scared ? 0.06f : 0.02f);
			if (groundNormal.HasValue && burrowCounter == 0 && UnityEngine.Random.value < num2 && !Poisoned)
			{
				if (rejoinCloudCounter > 120)
				{
					burrowCounter = 1;
				}
				else
				{
					vel += Custom.RotateAroundOrigo(groundNormal.Value, UnityEngine.Random.Range(-45f, 45f)) * 8f;
					facing = ((vel.x > 0f) ? Vector2.right : Vector2.left);
					groundNormal = null;
				}
			}
			if (scared && !groundNormal.HasValue)
			{
				vel += new Vector2(Mathf.PerlinNoise(seed * 10000f + 10f, 10f + (float)owner.timer / 50f) * 2f - 1f, Mathf.PerlinNoise(seed * 10000f + 20f, 20f + (float)owner.timer / 50f) * 2f - 1f);
			}
			if (flag && !lastContact)
			{
				owner.room.PlaySound(WatcherEnums.WatcherSoundID.Locust_Land, pos);
			}
			lastContact = flag;
		}

		public void DoAttached(LocustSystem owner)
		{
			Vector2 rotation = attachedToChunk.Rotation;
			pos = attachedToChunk.pos + attachmentPos.y * rotation + attachmentPos.x * Custom.PerpendicularVector(rotation);
			vel = attachedToChunk.vel;
			facing = rotation;
		}

		public void DoSwarming(LocustSystem owner)
		{
			Vector2 vector = swarm.center - pos;
			float magnitude = vector.magnitude;
			if (magnitude > 0f)
			{
				vel += vector / magnitude * Custom.LerpMap(magnitude, 0f, swarm.CurrentRadius + 40f, 0f, 2f, 2f);
			}
			if (UnityEngine.Random.value < 0.05f)
			{
				swarmMotion = UnityEngine.Random.insideUnitCircle * 0.5f;
			}
			vel += swarmMotion;
			vel = Vector2.ClampMagnitude(vel, 10f);
			foreach (GroundLocust locust in swarm.locusts)
			{
				if (locust != this)
				{
					Vector2 vector2 = locust.pos - pos;
					float magnitude2 = vector2.magnitude;
					if (magnitude2 > 0f)
					{
						vel -= vector2 / magnitude2 * Custom.LerpMap(magnitude, 4f, 8f, 2f, 0f);
					}
				}
			}
			if (!swarm.Lethal)
			{
				return;
			}
			Creature target = swarm.target;
			if (target == null)
			{
				return;
			}
			if (lungeCooldown > 0)
			{
				lungeCooldown--;
			}
			if (lungeCooldown == 0 && UnityEngine.Random.value < 0.03f)
			{
				lungeCooldown = 120;
				Lunge(target.bodyChunks[UnityEngine.Random.Range(0, target.bodyChunks.Length)]);
			}
			BodyChunk[] bodyChunks = target.bodyChunks;
			foreach (BodyChunk bodyChunk in bodyChunks)
			{
				if (Custom.DistLess(bodyChunk.pos, pos, bodyChunk.rad) && !owner.InAvoidanceZone(bodyChunk.pos) && swarm.TryAttach())
				{
					Attach(bodyChunk);
				}
			}
		}

		public void Lunge(BodyChunk chunk)
		{
			vel += (chunk.pos - pos).normalized * 10f;
		}

		public void Attach(BodyChunk chunk)
		{
			chunk.owner.room.PlaySound(WatcherEnums.WatcherSoundID.Locust_Attach, chunk, loop: false, 0.8f, UnityEngine.Random.Range(0.75f, 1.5f));
			attachedToChunk = chunk;
			attachmentPos = UnityEngine.Random.insideUnitCircle * (chunk.rad + 3f);
		}
	}

	public struct CloudLocust
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float seed;

		public float depth;

		public bool alive;

		public const float maxSpeed = 15.999996f;

		public const float accel = 0.8f;

		public const float drag = 0.95f;

		public bool Background => depth > 0.2f;

		public float TargetHeight => Custom.PushFromHalf(seed * 100f % 1f, 0.5f);

		public CloudLocust(Vector2 pos, float seed, float depth)
		{
			this.pos = pos;
			lastPos = pos;
			vel = Vector2.zero;
			this.seed = seed;
			this.depth = depth;
			alive = true;
			vel.x = 15.999996f;
		}

		public void Update(LocustSystem owner, bool isCluster)
		{
			lastPos = pos;
			bool flag = owner.IsWhaleNear(pos);
			if (flag)
			{
				foreach (BigSkyWhale bigWhale in owner.bigWhales)
				{
					if (bigWhale.MouthContains(pos))
					{
						alive = false;
					}
				}
			}
			float num = owner.CloudLine(pos.x, TargetHeight, isCluster);
			vel *= 0.95f;
			vel.x += 0.8f * (0.9f + 0.2f * seed) * (flag ? 0.5f : 1f);
			vel.y -= 0.1f;
			vel.y = Mathf.Lerp(vel.y, (num - pos.y) / 5f, 0.1f);
			Vector2 vector = new Vector2(Mathf.PerlinNoise(seed * 10000f + 10f, 10f + (float)owner.timer / 20f) * 2f - 1f, Mathf.PerlinNoise(seed * 10000f + 20f, 20f + (float)owner.timer / 20f) * 2f - 1f);
			if (flag)
			{
				vector *= 3f;
			}
			vel += vector;
			pos += vel;
			if (pos.x < owner.minX - 100f || pos.x > owner.maxX + 100f)
			{
				alive = false;
			}
		}
	}

	public class Swarm
	{
		public LocustSystem owner;

		public List<GroundLocust> locusts = new List<GroundLocust>();

		public Creature target;

		public Vector2 center;

		public bool disbanded;

		public float radius = 120f;

		public float collisionRadius = 8f;

		public int maxLocusts;

		public int disbandCounter;

		public Vector2 vel;

		public Vector2 targetPos;

		public int totalLocustsAttached;

		public int attachCooldown;

		public int age;

		public int hitByWeapons;

		public int killCounter;

		public StaticSoundLoop loop;

		public float SwarmProgress => Mathf.Min(1f, (float)Attached / 10f);

		public bool Lethal => owner.CanKill(target);

		public int Attached
		{
			get
			{
				int num = 0;
				foreach (GroundLocust locust in locusts)
				{
					if (locust.attachedToChunk != null)
					{
						num++;
					}
				}
				return num;
			}
		}

		public float CamoFactor
		{
			get
			{
				if (!(target is Player player))
				{
					return 0f;
				}
				return Mathf.Clamp01(player.camoProgress - Mathf.InverseLerp(80f, 480f, age));
			}
		}

		public int CurrentMaxLocusts => (int)Mathf.Lerp(maxLocusts, 1f, Mathf.Sqrt(CamoFactor));

		public float CurrentRadius => radius * (1f + CamoFactor * 3f);

		public Swarm(LocustSystem owner, Creature target, Vector2 pos)
		{
			maxLocusts = 20 + (int)(30f * owner.density);
			this.owner = owner;
			this.target = target;
			center = pos;
			targetPos = target.DangerPos;
			radius *= Custom.LerpMap(target.Template.bodySize, 1f, 5f, 1f, 2f);
		}

		public void AddLocust(GroundLocust locust)
		{
			locusts.Add(locust);
			locust.swarm = this;
		}

		public void RemoveLocust(GroundLocust locust)
		{
			locusts.Remove(locust);
			locust.swarm = null;
		}

		public void HitByWeapon(Weapon weapon, Vector2 pos, Vector2 dir)
		{
			float num = ((hitByWeapons > 0) ? 0.5f : 1f);
			foreach (GroundLocust locust in locusts)
			{
				if (locust.attachedToChunk != null && UnityEngine.Random.value < num)
				{
					locust.attachedToChunk = null;
					locust.vel += dir * UnityEngine.Random.Range(2f, 5f) + UnityEngine.Random.insideUnitCircle * 5f;
					if (UnityEngine.Random.value < 0.5f)
					{
						locust.Poisoned = true;
					}
				}
			}
			hitByWeapons++;
		}

		public void Update()
		{
			age++;
			if (loop == null)
			{
				loop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Locust_Swarm_LOOP, center, owner.room, 0f, 1f);
			}
			loop.Update();
			loop.volume = Mathf.MoveTowards(loop.volume, Mathf.Min(1f, (float)locusts.Count / 20f), 0.025f);
			if (locusts.Count > 0)
			{
				Vector2 pos = new Vector2(0f, 0f);
				foreach (GroundLocust locust in locusts)
				{
					pos += locust.pos;
				}
				pos /= (float)locusts.Count;
				loop.pos = pos;
			}
			if (disbanded)
			{
				if (loop.volume == 0f)
				{
					owner.disbandingSwarms.Remove(this);
				}
				return;
			}
			if (!IsTargetValid() || disbandCounter > 200 || (!target.leechedOut && UnityEngine.Random.value < 0.1f && owner.SwarmScore(target) <= 0f))
			{
				Disband();
				return;
			}
			if (target.leechedOut)
			{
				for (int num = locusts.Count - 1; num >= 0; num--)
				{
					if (locusts[num].attachedToChunk != null && UnityEngine.Random.value < 0.05f)
					{
						locusts[num].attachedToChunk = null;
						locusts[num].vel = Custom.RNV() * (UnityEngine.Random.value * 3f);
					}
					else if (UnityEngine.Random.value < 0.05f)
					{
						RemoveLocust(locusts[num]);
					}
				}
				disbandCounter++;
				return;
			}
			Recruit();
			if (attachCooldown > 0)
			{
				attachCooldown--;
			}
			bool flag = owner.ExposedToSky(target) >= 0.5f;
			if (owner.room.VisualContact(center, target.mainBodyChunk.pos) && flag)
			{
				targetPos = target.mainBodyChunk.pos;
				disbandCounter = 0;
			}
			else
			{
				targetPos += UnityEngine.Random.insideUnitCircle * 5f;
				if (disbandCounter % 20 == 0 && flag)
				{
					Vector2? newCenter = GetNewCenter(target.mainBodyChunk.pos);
					if (newCenter.HasValue)
					{
						Vector2 valueOrDefault = newCenter.GetValueOrDefault();
						center = valueOrDefault;
					}
				}
				disbandCounter++;
			}
			if (locusts.Count > maxLocusts - 5 && UnityEngine.Random.value < 0.04f)
			{
				RemoveLocust(locusts[UnityEngine.Random.Range(0, locusts.Count)]);
			}
			vel *= 0.85f;
			vel += Custom.DirVec(center, targetPos + new Vector2(0f, 15f)) * Custom.LerpMap(age, 80f, 240f, 1f, 2f);
			center += vel;
			Vector2 lastPos = center;
			SharedPhysics.TerrainCollisionData cd = new SharedPhysics.TerrainCollisionData(center, lastPos, vel, collisionRadius, default(IntVector2), goThroughFloors: true);
			cd = SharedPhysics.VerticalCollision(owner.room, cd);
			cd = SharedPhysics.HorizontalCollision(owner.room, cd);
			center = cd.pos;
			vel = cd.vel;
			int attached = Attached;
			if (totalLocustsAttached != attached)
			{
				target.Stun(Mathf.Min(attached * 2, 30));
			}
			float num2 = (float)attached / (float)maxLocusts;
			if (num2 > 0.1f)
			{
				if (target is Player)
				{
					if (UnityEngine.Random.value < num2 * 0.1f || num2 > 0.7f)
					{
						target.Stun(20);
					}
				}
				else if (num2 > 0.7f)
				{
					target.Stun(20);
				}
			}
			if (num2 > 0.1f && target.Stunned)
			{
				killCounter++;
				if (killCounter > 80)
				{
					target.SetKillTag(null);
					target.Die();
					target.leechedOut = true;
				}
			}
			else
			{
				killCounter = Mathf.Max(0, killCounter - 4);
			}
			totalLocustsAttached = attached;
		}

		public bool TryAttach()
		{
			if (attachCooldown == 0)
			{
				attachCooldown = UnityEngine.Random.Range(13, 17) + (int)(CamoFactor * 30f);
				if (target is Scavenger)
				{
					attachCooldown += 20;
				}
				return true;
			}
			return false;
		}

		public bool IsTargetValid()
		{
			if (target != null && !target.slatedForDeletetion && target.room == owner.room && target.abstractCreature.rippleLayer == 0)
			{
				return Mathf.Abs(target.mainBodyChunk.pos.x - center.x) < 300f;
			}
			return false;
		}

		public Vector2? GetNewCenter(Vector2 targetPos)
		{
			GroundLocust groundLocust = null;
			float num = float.PositiveInfinity;
			foreach (GroundLocust locust in locusts)
			{
				float num2 = Vector2.Distance(locust.pos, center);
				if (num2 < num && owner.room.VisualContact(locust.pos, targetPos))
				{
					groundLocust = locust;
					num = num2;
				}
			}
			return groundLocust?.pos;
		}

		public void Disband()
		{
			disbanded = true;
			for (int i = 0; i < locusts.Count; i++)
			{
				locusts[i].swarm = null;
			}
			locusts.Clear();
			owner.swarms.Remove(this);
			if (!owner.disbandingSwarms.Contains(this))
			{
				owner.disbandingSwarms.Add(this);
			}
		}

		public void Recruit()
		{
			int num = (owner.cloudLocusts.Count + 50) / 100;
			for (int i = 0; i < num; i++)
			{
				if (locusts.Count >= CurrentMaxLocusts)
				{
					break;
				}
				int index = UnityEngine.Random.Range(0, owner.cloudLocusts.Count);
				CloudLocust cloudLocust = owner.cloudLocusts[index];
				if (owner.CanLocustJoinSwarm(cloudLocust, this))
				{
					GroundLocust groundLocust = new GroundLocust(cloudLocust);
					cloudLocust.alive = false;
					owner.cloudLocusts[index] = cloudLocust;
					owner.groundLocusts.Add(groundLocust);
					AddLocust(groundLocust);
				}
			}
		}
	}

	public struct AvoidPoint
	{
		public Vector2 pos;

		public float radius;

		public int life;

		public AvoidPoint(Vector2 pos, float radius, int life)
		{
			this.pos = pos;
			this.radius = radius;
			this.life = life;
		}
	}

	public class SoundPlayer : UpdatableAndDeletable
	{
		public DisembodiedDynamicSoundLoop nearLoop;

		public DisembodiedDynamicSoundLoop farLoop;

		public DisembodiedDynamicSoundLoop callLoop;

		public float timeToNextCallModeSwitch;

		public int callModeSwitchTicker;

		public bool callAudible;

		public float callFade;

		public SoundPlayer()
		{
			callAudible = true;
			SwapCallMode();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			float num = 0f;
			float num2 = 1f;
			RoomSettings.RoomEffect effect = room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.LocustSwarmConfig);
			if (effect != null)
			{
				num = effect.GetAmount(1);
				num2 = effect.GetAmount(2);
			}
			if (nearLoop == null)
			{
				nearLoop = new DisembodiedDynamicSoundLoop(this);
			}
			if (farLoop == null)
			{
				farLoop = new DisembodiedDynamicSoundLoop(this);
			}
			if (callLoop == null)
			{
				callLoop = new DisembodiedDynamicSoundLoop(this);
			}
			float num3 = num * num2;
			if (callAudible)
			{
				callFade = Mathf.Lerp(callFade, 1f, 0.05f);
			}
			else
			{
				callFade = Mathf.Lerp(callFade, 0f, 0.025f);
			}
			callModeSwitchTicker++;
			if ((float)callModeSwitchTicker >= timeToNextCallModeSwitch)
			{
				SwapCallMode();
			}
			nearLoop.sound = WatcherEnums.WatcherSoundID.Locust_Cloud_Near_LOOP;
			nearLoop.Volume = Mathf.Lerp(num3 * 0.5f, num3, 1f - callFade);
			nearLoop.Update();
			farLoop.sound = WatcherEnums.WatcherSoundID.Locust_Cloud_Far_LOOP;
			farLoop.Volume = num * (1f - num2);
			farLoop.Update();
			callLoop.sound = WatcherEnums.WatcherSoundID.Locust_Call_LOOP;
			callLoop.Volume = Mathf.Lerp(0f, num3, callFade);
			callLoop.Update();
		}

		public void SwapCallMode()
		{
			callAudible = !callAudible;
			if (callAudible)
			{
				timeToNextCallModeSwitch = UnityEngine.Random.Range(120, 400);
			}
			else
			{
				timeToNextCallModeSwitch = UnityEngine.Random.Range(400, 2400);
			}
			callModeSwitchTicker = 0;
		}
	}

	public List<PlacedObject> cloudLineObjects = new List<PlacedObject>();

	public List<CloudLocust> cloudLocusts = new List<CloudLocust>();

	public List<CloudLocust> cloudClusters = new List<CloudLocust>();

	public List<GroundLocust> groundLocusts = new List<GroundLocust>();

	public List<Swarm> swarms = new List<Swarm>();

	public List<Swarm> disbandingSwarms = new List<Swarm>();

	public float spawnCounter;

	public List<AvoidPoint> avoidPoints = new List<AvoidPoint>();

	public const int spritesPerCloudLocust = 2;

	public const int spritesPerGroundLocust = 2;

	public const int spritesPerCloudCluster = 1;

	public float minX;

	public float maxX;

	public float maxY;

	public float density;

	public int[] threatHeights;

	public List<BigSkyWhale> bigWhales = new List<BigSkyWhale>();

	public List<SkyWhale> smallWhales = new List<SkyWhale>();

	public int timer;

	public float spawnMultiplier;

	public const float belowCloudThreatDistance = 600f;

	public HashSet<PlacedObject> tempObjects = new HashSet<PlacedObject>();

	public float LocustsPerTick => Custom.LerpMap(density, 0f, 0.5f, 0.1f, 4f) * spawnMultiplier;

	public float ClusterChance => Custom.LerpMap(density, 0.5f, 1f, 0f, 0.5f, 2f);

	public LocustSystem(Room room)
	{
		minX = room.cameraPositions[0].x;
		maxX = room.cameraPositions[0].x;
		maxY = room.cameraPositions[0].y;
		Vector2[] cameraPositions = room.cameraPositions;
		for (int i = 0; i < cameraPositions.Length; i++)
		{
			Vector2 vector = cameraPositions[i];
			minX = Mathf.Min(minX, vector.x);
			maxX = Mathf.Max(maxX, vector.x + 1366f);
			maxY = Mathf.Max(maxY, vector.y + 768f);
		}
		minX -= 100f;
		maxX += 100f;
		maxY += 100f;
		threatHeights = new int[Mathf.RoundToInt((maxX - minX) / 20f)];
		spawnMultiplier = (room.VoidWeaverActive ? 0f : 1f);
	}

	public bool HasLocustThreat(IntVector2 tile)
	{
		int num = Mathf.Clamp(Mathf.FloorToInt((float)tile.x - minX / 20f), 0, threatHeights.Length - 1);
		return tile.y >= threatHeights[num];
	}

	public float GetSwarmProgress(Creature target)
	{
		foreach (Swarm swarm in swarms)
		{
			if (swarm.target == target)
			{
				return swarm.SwarmProgress;
			}
		}
		return 0f;
	}

	public float CloudLine(float x, float heightFac, bool isCluster)
	{
		int i;
		for (i = 1; i < cloudLineObjects.Count - 1 && cloudLineObjects[i].pos.x < x; i++)
		{
		}
		if (i >= cloudLineObjects.Count)
		{
			return room.PixelHeight - 80f;
		}
		PlacedObject placedObject = cloudLineObjects[i - 1];
		PlacedObject placedObject2 = cloudLineObjects[i];
		float num = (x - placedObject.pos.x) / (placedObject2.pos.x - placedObject.pos.x);
		float num2 = Mathf.Lerp(placedObject.pos.y, placedObject2.pos.y, num);
		float num3 = Mathf.Lerp((placedObject.data as PlacedObject.ResizableObjectData).Rad, (placedObject2.data as PlacedObject.ResizableObjectData).Rad, num);
		if (isCluster)
		{
			num3 = Mathf.Max(num3 - 128f, 0f);
		}
		num2 += (heightFac * 2f - 1f) * num3;
		if (num < 0f || num > 1f)
		{
			num2 += Mathf.Max(num - 1f, 0f - num) * (placedObject2.pos.x - placedObject.pos.x) * 0.5f;
		}
		return num2;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		float num = ((!room.VoidWeaverActive) ? 1f : 0f);
		if (room.roomSettings.DangerType != RoomRain.DangerType.None && room.roomSettings.RainIntensity >= 0.2f)
		{
			num *= Mathf.InverseLerp(0f, 0.5f, room.world.rainCycle.RainApproaching);
		}
		foreach (BigSkyWhale bigWhale in bigWhales)
		{
			SimpleSegment[] segments = bigWhale.segments;
			for (int i = 0; i < segments.Length; i++)
			{
				if (Mathf.Abs(segments[i].pos.x - minX) < bigWhale.scareRadius)
				{
					num = 0f;
					break;
				}
			}
		}
		if (spawnMultiplier > num)
		{
			spawnMultiplier = num;
		}
		else
		{
			spawnMultiplier = Mathf.Min(spawnMultiplier + 0.0025f, num);
		}
		timer++;
		bigWhales.Clear();
		smallWhales.Clear();
		if (!room.BeingViewed)
		{
			cloudLocusts.Clear();
			return;
		}
		foreach (UpdatableAndDeletable update in room.updateList)
		{
			if (update is BigSkyWhale item)
			{
				bigWhales.Add(item);
			}
			else if (update is SkyWhale { AltVariant: not false } skyWhale)
			{
				smallWhales.Add(skyWhale);
			}
		}
		if ((room.game.devToolsActive && room.game.devUI?.activePage is ObjectsPage) || cloudLineObjects.Count == 0)
		{
			RefreshCloudLineObjects();
		}
		for (int j = 0; j < avoidPoints.Count; j++)
		{
			AvoidPoint value = avoidPoints[j];
			value.life--;
			avoidPoints[j] = value;
		}
		avoidPoints.RemoveAll((AvoidPoint point) => point.life <= 0);
		foreach (AbstractWorldEntity entity in room.abstractRoom.entities)
		{
			if (entity is AbstractPhysicalObject { realizedObject: not null } abstractPhysicalObject)
			{
				float avoidanceRadius = GetAvoidanceRadius(abstractPhysicalObject);
				if (avoidanceRadius > 0f)
				{
					AddAvoidancePoint(abstractPhysicalObject.realizedObject.firstChunk.pos, avoidanceRadius, 1);
				}
			}
		}
		density = room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.LocustSwarmConfig)?.GetAmount(0) ?? 0.5f;
		if (cloudLocusts.Count == 0)
		{
			WarmUp();
		}
		for (int k = 0; k < cloudLocusts.Count; k++)
		{
			CloudLocust value2 = cloudLocusts[k];
			value2.Update(this, isCluster: false);
			cloudLocusts[k] = value2;
		}
		cloudLocusts.RemoveAll((CloudLocust locust) => !locust.alive);
		for (int l = 0; l < cloudClusters.Count; l++)
		{
			CloudLocust value3 = cloudClusters[l];
			value3.Update(this, isCluster: true);
			cloudClusters[l] = value3;
		}
		cloudClusters.RemoveAll((CloudLocust locust) => !locust.alive);
		foreach (GroundLocust groundLocust in groundLocusts)
		{
			groundLocust.Update(this);
		}
		groundLocusts.RemoveAll((GroundLocust locust) => !locust.alive);
		float num2 = 1f / LocustsPerTick;
		spawnCounter += 1f;
		while (spawnCounter > num2)
		{
			spawnCounter -= num2;
			AddCloudLocust(minX + spawnCounter * 15.999996f);
		}
		DoGrounding();
		SpawnSwarms();
		for (int num3 = swarms.Count - 1; num3 >= 0; num3--)
		{
			swarms[num3].Update();
		}
		for (int num4 = disbandingSwarms.Count - 1; num4 >= 0; num4--)
		{
			disbandingSwarms[num4].Update();
		}
	}

	public float GetAvoidanceRadius(AbstractPhysicalObject obj)
	{
		if (obj.realizedObject is Creature { repelLocusts: >0 } creature)
		{
			return Custom.LerpMap(creature.repelLocusts, 40f, 0f, 250f, 0f, 2f);
		}
		return 0f;
	}

	public void SpawnSwarms()
	{
		if (room.abstractRoom.creatures.Count == 0)
		{
			return;
		}
		Creature realizedCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)].realizedCreature;
		if (realizedCreature != null && SwarmScore(realizedCreature) > 0f && !IsCreatureBeingSwarmed(realizedCreature))
		{
			Vector2 pos = realizedCreature.mainBodyChunk.pos;
			Swarm closestSwarm = GetClosestSwarm(pos);
			if (closestSwarm == null || !Custom.DistLess(pos, closestSwarm.center, 400f))
			{
				Swarm item = new Swarm(this, realizedCreature, new Vector2(pos.x, CloudLine(pos.x, 0f, isCluster: false)));
				swarms.Add(item);
			}
		}
	}

	public void DoGrounding()
	{
		float num = room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.LocustSwarmConfig)?.GetAmount(4) ?? 0f;
		int num2 = (cloudLocusts.Count + 50) / 100;
		for (int i = 0; i < num2; i++)
		{
			CloudLocust cloudLocust = cloudLocusts[UnityEngine.Random.Range(0, cloudLocusts.Count)];
			if (!room.HasAnySolid(room.GetTilePosition(cloudLocust.pos)) && UnityEngine.Random.value < 0.2f * num && !IsWhaleNear(cloudLocust.pos))
			{
				GroundLocust groundLocust = new GroundLocust(cloudLocust);
				groundLocust.Idle(UnityEngine.Random.Range(80, 240));
				cloudLocust.alive = false;
				groundLocusts.Add(groundLocust);
			}
		}
	}

	public Swarm GetClosestSwarm(Vector2 point)
	{
		Swarm result = null;
		float num = float.PositiveInfinity;
		foreach (Swarm swarm in swarms)
		{
			float num2 = Vector2.Distance(swarm.center, point);
			if (num2 < num)
			{
				result = swarm;
				num = num2;
			}
		}
		return result;
	}

	public bool IsCreatureBeingSwarmed(Creature crit)
	{
		foreach (Swarm swarm in swarms)
		{
			if (swarm.target == crit)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanLocustJoinSwarm(CloudLocust locust, Swarm swarm)
	{
		if (locust.alive && !room.HasAnySolid(room.GetTilePosition(locust.pos)) && Mathf.Abs(locust.pos.x - swarm.center.x) < 300f)
		{
			return room.VisualContact(locust.pos, swarm.center);
		}
		return false;
	}

	public float SwarmScore(Creature crit)
	{
		float num = SwarmScore(crit.Template, crit.State.dead);
		if (crit.abstractCreature.rippleLayer != 0 || num == 0f || crit.leechedOut || ExposedToSky(crit) < 0.5f || (crit.Template.type == CreatureTemplate.Type.Slugcat && room.game.devToolsActive && room.game.devUI?.activePage is ObjectsPage) || crit.repelLocusts > 0 || crit.grabbedBy.Count > 0)
		{
			return 0f;
		}
		Vector2 pos = crit.mainBodyChunk.pos;
		float value = Mathf.Max(0f, CloudLine(pos.x, 0f, isCluster: false) - pos.y);
		return Mathf.InverseLerp(600f, 300f, value) * num;
	}

	public float SwarmScore(CreatureTemplate template, bool dead)
	{
		CreatureTemplate.Type type = template.type;
		_ = template.TopAncestor().type;
		if (type == WatcherEnums.CreatureTemplateType.SandGrub)
		{
			if (!dead)
			{
				return 0f;
			}
			return 1f;
		}
		if (type == WatcherEnums.CreatureTemplateType.BigSandGrub || type == WatcherEnums.CreatureTemplateType.IndigoLizard || type == CreatureTemplate.Type.Overseer)
		{
			return 0f;
		}
		return Mathf.InverseLerp(6f, 1f, template.bodySize);
	}

	public bool CanKill(Creature creature)
	{
		if (creature.Template.bodySize < 1.5f)
		{
			return creature.TotalMass < 1f;
		}
		return false;
	}

	public void WarmUp()
	{
		float num = 15.999996f / LocustsPerTick;
		if (!float.IsInfinity(num))
		{
			for (float num2 = minX; num2 < maxX; num2 += num)
			{
				AddCloudLocust(num2);
			}
		}
	}

	public CloudLocust AddCloudLocust(float x)
	{
		bool flag = UnityEngine.Random.value < ClusterChance;
		CloudLocust cloudLocust = new CloudLocust(new Vector2(x, 0f), UnityEngine.Random.value, UnityEngine.Random.Range(-1f, flag ? 0.75f : 1f));
		cloudLocust.pos.y = CloudLine(cloudLocust.pos.x, cloudLocust.TargetHeight, flag);
		cloudLocust.lastPos = cloudLocust.pos;
		Vector2 vector = Custom.DirVec(cloudLocust.pos, new Vector2(cloudLocust.pos.x + 1f, CloudLine(cloudLocust.pos.x + 1f, cloudLocust.TargetHeight, flag)));
		cloudLocust.vel = vector * 15.999996f;
		if (flag)
		{
			cloudClusters.Add(cloudLocust);
		}
		else
		{
			cloudLocusts.Add(cloudLocust);
		}
		return cloudLocust;
	}

	public CloudLocust AddCloudLocust(GroundLocust groundLocust)
	{
		CloudLocust cloudLocust = new CloudLocust(groundLocust.pos, groundLocust.seed, groundLocust.depth);
		cloudLocust.lastPos = groundLocust.lastPos;
		cloudLocust.vel = groundLocust.vel;
		cloudLocusts.Add(cloudLocust);
		return cloudLocust;
	}

	public void RefreshCloudLineObjects()
	{
		foreach (PlacedObject cloudLineObject in cloudLineObjects)
		{
			tempObjects.Add(cloudLineObject);
		}
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if (placedObject.type == PlacedObject.Type.LocustCloud)
			{
				if (!tempObjects.Contains(placedObject))
				{
					cloudLineObjects.Add(placedObject);
				}
				else
				{
					tempObjects.Remove(placedObject);
				}
			}
		}
		foreach (PlacedObject tempObject in tempObjects)
		{
			cloudLineObjects.Remove(tempObject);
		}
		tempObjects.Clear();
		for (int i = 1; i < cloudLineObjects.Count; i++)
		{
			if (cloudLineObjects[i - 1].pos.x > cloudLineObjects[i].pos.x)
			{
				cloudLineObjects.Sort((PlacedObject a, PlacedObject b) => a.pos.x.CompareTo(b.pos.x));
				break;
			}
		}
		for (int j = 0; j < threatHeights.Length; j++)
		{
			int num = (int)(minX / 20f) + j;
			int num2 = Mathf.FloorToInt((CloudLine((float)num * 20f + 10f, 0f, isCluster: false) - 600f) / 20f);
			int num3 = room.TileHeight - 1;
			while (num3 > 0 && num3 > num2 && !room.HasAnySolid(num, num3))
			{
				num3--;
			}
			threatHeights[j] = num3;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		LoadLocustClusterGraphic();
		sLeaser.sprites = new FSprite[0];
		sLeaser.containers = new FContainer[4];
		for (int i = 0; i < sLeaser.containers.Length; i++)
		{
			sLeaser.containers[i] = new FContainer();
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public static void LoadLocustClusterGraphic()
	{
		if (!Futile.atlasManager.DoesContainAtlas("locust_cluster"))
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false, linear: true)
			{
				filterMode = FilterMode.Point,
				anisoLevel = 0
			};
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + "locust_cluster.png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: false, crispPixels: false);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture("locust_cluster", texture2D, textureFromAsset: false);
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.containers[0]);
		rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.containers[1]);
		rCam.ReturnFContainer("Items").AddChild(sLeaser.containers[2]);
		rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.containers[3]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		DrawCloudLocusts(sLeaser, rCam, timeStacker, camPos);
		DrawGroundLocusts(sLeaser, rCam, timeStacker, camPos);
		DrawCloudClusters(sLeaser, rCam, timeStacker, camPos);
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void DrawCloudLocusts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		FContainer fContainer = sLeaser.containers[0];
		FContainer fContainer2 = sLeaser.containers[1];
		int num = 0;
		int num2 = 0;
		RoomPalette currentPalette = rCam.currentPalette;
		Color b = Color.Lerp(currentPalette.skyColor, currentPalette.fogColor, currentPalette.fogAmount);
		for (int i = 0; i < cloudLocusts.Count; i++)
		{
			CloudLocust cloudLocust = cloudLocusts[i];
			float num3 = cloudLocust.depth * 5f * 30f;
			Vector2 ps = Vector2.Lerp(cloudLocust.lastPos, cloudLocust.pos, timeStacker);
			ps = rCam.ApplyDepth(ps, cloudLocust.depth * 30f);
			ps -= camPos;
			if (!(ps.x < -20f) && !(ps.y < -20f) && !(ps.x > rCam.sSize.x + 20f) && !(ps.y > rCam.sSize.y + 20f) && cloudLocust.alive)
			{
				float num4 = 10f / (10f - num3 * -0.025f);
				FContainer fContainer3 = (cloudLocust.Background ? fContainer : fContainer2);
				ref int reference = ref cloudLocust.Background ? ref num : ref num2;
				if (fContainer3.GetChildCount() < reference + 2)
				{
					AddNewCloudSprites(fContainer3, rCam);
				}
				FSprite obj = (FSprite)fContainer3.GetChildAt(reference);
				FSprite fSprite = (FSprite)fContainer3.GetChildAt(reference + 1);
				reference += 2;
				float num5 = 0f - Vector2.SignedAngle(Vector2.right, cloudLocust.vel);
				obj.isVisible = true;
				obj.rotation = num5;
				obj.SetPosition(ps);
				obj.scale = num4 * 0.75f;
				obj.color = Color.Lerp(currentPalette.blackColor, b, cloudLocust.depth);
				obj.alpha = (cloudLocust.Background ? 0f : 1f);
				float num6 = Mathf.PingPong(cloudLocust.seed + ((float)timer + timeStacker) * Mathf.Lerp(0.7f, 1.2f, cloudLocust.seed), 1f);
				fSprite.isVisible = true;
				fSprite.SetPosition(ps);
				fSprite.rotation = Mathf.Lerp(-45f, 45f, num6) + num5;
				fSprite.color = Color.Lerp(currentPalette.blackColor, b, cloudLocust.depth * 0.2f + 0.8f);
				fSprite.scale = num4 * 0.75f;
				fSprite.scaleY *= 1f - Mathf.Abs(num6 - 0.5f) * 2f + 1f;
				fSprite.alpha = (cloudLocust.Background ? 0f : 1f);
			}
		}
		for (int j = num2; j < fContainer2.GetChildCount(); j++)
		{
			fContainer2.GetChildAt(j).isVisible = false;
		}
		for (int k = num; k < fContainer.GetChildCount(); k++)
		{
			fContainer.GetChildAt(k).isVisible = false;
		}
	}

	public void DrawGroundLocusts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		FContainer fContainer = sLeaser.containers[2];
		int num = 0;
		RoomPalette currentPalette = rCam.currentPalette;
		Color b = Color.Lerp(currentPalette.skyColor, currentPalette.fogColor, currentPalette.fogAmount);
		for (int i = 0; i < groundLocusts.Count; i++)
		{
			GroundLocust groundLocust = groundLocusts[i];
			float num2 = Mathf.Lerp(groundLocust.lastDepth, groundLocust.depth, timeStacker);
			float num3 = num2 * 5f * 30f;
			Vector2 ps = Vector2.Lerp(groundLocust.lastPos, groundLocust.pos, timeStacker);
			ps = rCam.ApplyDepth(ps, num2 * 30f);
			ps -= camPos;
			if (!(ps.x < -20f) && !(ps.y < -20f) && !(ps.x > rCam.sSize.x + 20f) && !(ps.y > rCam.sSize.y + 20f) && groundLocust.alive)
			{
				float scale = Mathf.Lerp(10f / (10f - num3 * -0.025f), 1f, 0.75f);
				float num4 = Mathf.Clamp01(((float)groundLocust.burrowCounter - timeStacker) / 30f);
				float num5 = Custom.LerpMap((float)groundLocust.poisonCounter - timeStacker, 160f, 240f, 1f, 0f);
				if (fContainer.GetChildCount() < num + 2)
				{
					AddNewGroundSprites(fContainer, rCam);
				}
				Vector2 vector = groundLocust.facing;
				if (num4 > 0f)
				{
					vector = Vector3.Slerp(vector, Custom.RotateAroundOrigo((-groundLocust.groundNormal) ?? Vector2.down, Mathf.Sin(num4 * 30f * 0.9f) * 15f), Mathf.Pow(num4, 0.5f));
				}
				float num6 = Custom.VecToDeg(vector) - 90f;
				FSprite fSprite = (FSprite)fContainer.GetChildAt(num);
				FSprite fSprite2 = (FSprite)fContainer.GetChildAt(num + 1);
				num += 2;
				fSprite.isVisible = true;
				fSprite.SetPosition(ps);
				fSprite.rotation = num6;
				fSprite.scale = scale;
				fSprite.scaleY *= num5;
				fSprite.color = Color.Lerp(currentPalette.blackColor, b, num2);
				float num7 = Mathf.PingPong(groundLocust.seed + ((float)timer + timeStacker) * Mathf.Lerp(0.7f, 1.2f, groundLocust.seed), 1f);
				if (groundLocust.groundNormal.HasValue || groundLocust.Poisoned)
				{
					num7 = 0.5f;
				}
				fSprite2.isVisible = true;
				fSprite2.SetPosition(ps);
				fSprite2.rotation = Mathf.Lerp(-45f, 45f, num7) + num6;
				fSprite2.color = Color.Lerp(currentPalette.blackColor, b, num2 * 0.2f + 0.8f);
				fSprite2.scale = scale;
				fSprite2.scaleY *= (1f - Mathf.Abs(num7 - 0.5f) * 2f + 1f) * (1f - Mathf.Pow(num4, 2f)) * num5;
				if (groundLocust.facing.x < 0f)
				{
					fSprite.scaleY *= -1f;
					fSprite2.scaleY *= -1f;
				}
			}
		}
		for (int j = num; j < fContainer.GetChildCount(); j++)
		{
			fContainer.GetChildAt(j).isVisible = false;
		}
	}

	public void DrawCloudClusters(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		FContainer fContainer = sLeaser.containers[3];
		int num = 0;
		for (int i = 0; i < cloudClusters.Count; i++)
		{
			CloudLocust cloudLocust = cloudClusters[i];
			float num2 = cloudLocust.depth * 5f * 30f;
			Vector2 ps = Vector2.Lerp(cloudLocust.lastPos, cloudLocust.pos, timeStacker);
			ps = rCam.ApplyDepth(ps, cloudLocust.depth * 30f);
			ps -= camPos;
			if (!(ps.x < -150f) && !(ps.y < -150f) && !(ps.x > rCam.sSize.x + 150f) && !(ps.y > rCam.sSize.y + 150f) && cloudLocust.alive)
			{
				float scale = Mathf.Lerp(10f / (10f - num2 * -0.025f), 1f, 0.75f);
				if (fContainer.GetChildCount() < num + 1)
				{
					AddNewClusterSprites(fContainer, rCam);
				}
				FSprite obj = (FSprite)fContainer.GetChildAt(num);
				num++;
				float t = Mathf.PingPong(cloudLocust.seed + ((float)timer + timeStacker) * Mathf.Lerp(0.7f, 1.2f, cloudLocust.seed), 1f);
				t = (float)Math.PI / 180f * Mathf.Lerp(-45f, 45f, t);
				obj.isVisible = true;
				obj.SetPosition(ps);
				obj.scale = scale;
				obj.color = new Color(Mathf.Cos(t), Mathf.Sin(t), cloudLocust.depth, 1f);
			}
		}
		for (int j = num; j < fContainer.GetChildCount(); j++)
		{
			fContainer.GetChildAt(j).isVisible = false;
		}
	}

	public void AddNewCloudSprites(FContainer container, RoomCamera rCam)
	{
		container.AddChild(new FSprite("LocustBody")
		{
			anchorX = 0.7f,
			shader = rCam.game.rainWorld.Shaders["CustomDepth"]
		});
		container.AddChild(new FSprite("LocustWing")
		{
			anchorX = 1f,
			shader = rCam.game.rainWorld.Shaders["CustomDepth"]
		});
	}

	public void AddNewGroundSprites(FContainer container, RoomCamera rCam)
	{
		container.AddChild(new FSprite("LocustBody")
		{
			anchorX = 0.7f,
			shader = rCam.game.rainWorld.Shaders["RippleBasic"]
		});
		container.AddChild(new FSprite("LocustWing")
		{
			anchorX = 1f,
			shader = rCam.game.rainWorld.Shaders["RippleBasic"]
		});
	}

	public void AddNewClusterSprites(FContainer container, RoomCamera rCam)
	{
		container.AddChild(new FSprite("locust_cluster")
		{
			shader = rCam.game.rainWorld.Shaders["LocustCluster"]
		});
	}

	public float ExposedToSky(Creature creature)
	{
		IntVector2 tilePosition = room.GetTilePosition(creature.mainBodyChunk.pos + new Vector2(0f, creature.mainBodyChunk.rad));
		if (tilePosition.y >= room.TileHeight)
		{
			return 1f;
		}
		int num = 0;
		int num2 = 0;
		while (num2 >= -3 && !room.HasAnySolid(tilePosition.x + num2, tilePosition.y))
		{
			if (room.RayTraceTilesForTerrain(tilePosition.x + num2, tilePosition.y, tilePosition.x + num2, room.TileHeight - 1))
			{
				num++;
			}
			num2--;
		}
		for (int i = 0; i <= 3 && !room.HasAnySolid(tilePosition.x + i, tilePosition.y); i++)
		{
			if (i != 0 && room.RayTraceTilesForTerrain(tilePosition.x + i, tilePosition.y, tilePosition.x + i, room.TileHeight - 1))
			{
				num++;
			}
		}
		return (float)num / 7f;
	}

	public bool InAvoidanceZone(Vector2 pos)
	{
		foreach (AvoidPoint avoidPoint in avoidPoints)
		{
			if (Vector2.SqrMagnitude(avoidPoint.pos - pos) < avoidPoint.radius * avoidPoint.radius)
			{
				return true;
			}
		}
		return false;
	}

	public void KillInRadius(Vector2 pos, float radius)
	{
		bool flag = false;
		foreach (GroundLocust groundLocust in groundLocusts)
		{
			if (Custom.DistLess(pos, groundLocust.pos, radius))
			{
				flag = true;
				groundLocust.Poisoned = true;
			}
		}
		if (flag)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Locust_Poisoned, pos);
		}
	}

	public void AddAvoidancePoint(Vector2 pos, float radius, int life)
	{
		avoidPoints.Add(new AvoidPoint(pos, radius, life));
	}

	public void WeaponThrown(Weapon weapon, Creature thrownBy, Vector2 thrownPos, IntVector2 throwDir)
	{
		if (!weapon.HeavyWeapon || thrownBy == null)
		{
			return;
		}
		foreach (Swarm swarm in swarms)
		{
			if (swarm.target == thrownBy && swarm.SwarmProgress > 0f)
			{
				swarm.HitByWeapon(weapon, thrownPos, throwDir.ToVector2());
			}
		}
	}

	public bool IsWhaleNear(Vector2 pos)
	{
		foreach (BigSkyWhale bigWhale in bigWhales)
		{
			SimpleSegment[] segments = bigWhale.segments;
			for (int i = 0; i < segments.Length; i++)
			{
				SimpleSegment simpleSegment = segments[i];
				if (Custom.DistLess(pos, simpleSegment.pos, bigWhale.scareRadius))
				{
					return true;
				}
			}
		}
		foreach (SkyWhale smallWhale in smallWhales)
		{
			BodyChunk[] bodyChunks = smallWhale.bodyChunks;
			foreach (BodyChunk obj in bodyChunks)
			{
				Vector2 pos2 = obj.pos;
				float num = obj.rad + 200f;
				if (pos.x > pos2.x - num && pos.x < pos2.x + num && pos.y > pos2.y - num - 400f && pos.y < pos2.y + num)
				{
					return true;
				}
			}
		}
		return false;
	}
}
