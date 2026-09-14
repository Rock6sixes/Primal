using System.Collections.Generic;
using System.Globalization;
using RWCustom;
using Smoke;
using UnityEngine;

namespace Watcher;

public class BoxWorm : Creature, ICallForHelp
{
	public class IndividualVariations
	{
		public int numberOfLarvaHolders = 10;

		public int numberOfLarva = 2;

		public float sizeVariation = 1f;

		public float[] horizontalOffsets;

		public bool[] hasLarva;

		public IndividualVariations()
		{
		}

		public IndividualVariations(AbstractCreature aCreature)
			: this()
		{
			int randomSeed = aCreature.ID.RandomSeed;
			Random.State state = Random.state;
			Random.InitState(randomSeed);
			numberOfLarva = Mathf.Max(1, (int)(Random.value * (float)Mathf.Min(5, numberOfLarvaHolders)));
			sizeVariation = 1f + Random.value * 0.2f;
			horizontalOffsets = new float[numberOfLarvaHolders];
			hasLarva = new bool[numberOfLarvaHolders];
			for (int i = 0; i < horizontalOffsets.Length; i++)
			{
				float num = Random.value * 2f - 1f;
				if (i > 0 && Mathf.Abs(num - horizontalOffsets[i - 1]) < 0.3f)
				{
					i--;
				}
				else
				{
					horizontalOffsets[i] = num;
				}
			}
			int num2 = 0;
			do
			{
				int num3 = (int)(Random.value * (float)Mathf.Min(numberOfLarva, hasLarva.Length));
				if (!hasLarva[num3])
				{
					hasLarva[num3] = true;
					num2++;
				}
			}
			while (num2 < numberOfLarva);
			Random.state = state;
		}
	}

	public class GroundGrabber
	{
		public BodyChunk chunk;

		public bool isGrabbing;

		public bool reachedDestination;

		public float speed = 1f;

		public Vector2? grabDestination { get; set; }

		public bool isBodyChunkOnGround { get; set; }

		public GroundGrabber(BodyChunk chunk)
		{
			this.chunk = chunk;
		}

		public void Update()
		{
			if (chunk.owner.room != null)
			{
				UpdatePosition();
				if (isGrabbing && grabDestination.HasValue)
				{
					CheckIfDestinationReached();
					GrabGround();
				}
			}
		}

		public void GrabGround()
		{
			Vector2 value = grabDestination.Value;
			chunk.vel = MoveToPoint(chunk.pos, value, 5f * speed);
		}

		public void CheckIfDestinationReached()
		{
			Vector2 value = grabDestination.Value;
			if (!reachedDestination && Vector2.Distance(value, chunk.pos) < 5f)
			{
				reachedDestination = true;
			}
			if (reachedDestination && Vector2.Distance(value, chunk.pos) > 30f)
			{
				reachedDestination = true;
			}
		}

		public bool IsBodyChunkOnGround(BodyChunk chunk)
		{
			Room room = chunk.owner.room;
			IntVector2 pos = room.GetTilePosition(chunk.pos) + Custom.fourDirections[1];
			Room.Tile.TerrainType terrain = room.GetTile(pos).Terrain;
			isBodyChunkOnGround = terrain != 0 || room.aimap.getAItile(chunk.pos).floorAltitude <= 1;
			return isBodyChunkOnGround;
		}

		public void UpdatePosition()
		{
			if (!reachedDestination && !isGrabbing)
			{
				grabDestination = null;
			}
			if (IsBodyChunkOnGround(chunk))
			{
				grabDestination = chunk.owner.room.MiddleOfTile(chunk.pos);
			}
		}

		public void Grab()
		{
			isGrabbing = true;
		}

		public void Release()
		{
			isGrabbing = false;
		}
	}

	public class CreatureSteamDamage
	{
		public EntityID ID { get; set; }

		public int damage { get; set; }

		public int temporaryDamage { get; set; }

		public int health { get; set; }

		public bool IsLethal => damage >= health;

		public CreatureSteamDamage(AbstractCreature abstractCreature, int health)
		{
			ID = abstractCreature.ID;
			this.health = health;
		}

		public void AddDamage(int damage)
		{
			temporaryDamage += damage;
		}

		public void FinalizeDamage(int threshold)
		{
			if (temporaryDamage > threshold)
			{
				damage++;
			}
			temporaryDamage = 0;
		}
	}

	public class Injury
	{
		public BoxWorm owner;

		public Spear spear;

		public int chunk;

		public Counter steamCounter = new Counter(200);

		public Vector2? direction;

		public ChunkSoundEmitter steamSE;

		public bool dripPlayed;

		public Injury(BoxWorm owner, Spear spear, int chunk)
		{
			this.owner = owner;
			this.spear = spear;
			this.chunk = chunk;
			if (spear.thrownBy is Player)
			{
				owner.ai.ModifyPlayerRelationship(spear.thrownBy as Player, -0.05f);
			}
		}

		public void Update()
		{
			if (spear?.stuckInObject != null || steamCounter.isFinished)
			{
				return;
			}
			if (spear != null)
			{
				if (spear.grabbedBy.Count > 0)
				{
					direction = (owner.bodyChunks[chunk].pos - spear.grabbedBy[0].grabber.firstChunk.pos).normalized;
					direction = Custom.rotateVectorDeg(direction.Value, Random.Range(-45, 45));
				}
				steamSE = owner.room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Idle_Steam, owner.bodyChunks[chunk], loop: false, 0.001f, 0.9f);
				owner.Violence(spear.firstChunk, null, owner.mainBodyChunk, null, DamageType.None, 10f * owner.SteamPower, 0f);
				if (spear.thrownBy is Player)
				{
					owner.ai.ModifyPlayerRelationship(spear.thrownBy as Player, -0.1f);
				}
				spear = null;
			}
			steamCounter.Tick();
			if ((float)(int)owner.steamAvailable > (float)owner.minSteamRequired * 2f)
			{
				owner.steamAvailable.Add(-1);
			}
			EmitSteam();
		}

		public void EmitSteam()
		{
			float num = owner.SteamPower * steamCounter.normalized;
			steamSE.volume = Mathf.Lerp(steamSE.volume, num, 0.1f);
			steamSE.pitch = Mathf.Lerp(steamSE.pitch, 0.9f + num * 0.1f, 0.1f);
			if (num <= 0f)
			{
				return;
			}
			if (!direction.HasValue)
			{
				direction = (owner.bodyChunks[chunk - 1].pos - owner.bodyChunks[chunk].pos).normalized;
				direction = Custom.PerpendicularVector(direction.Value);
			}
			if (Random.value * 0.5f > num)
			{
				owner.room.AddObject(new WaterDrip(owner.bodyChunks[chunk].pos, (direction.Value + Custom.RNV() * 0.1f) * (70f * num), waterColor: true));
				if (!dripPlayed)
				{
					owner.room.PlaySound(SoundID.Splashing_Water_Into_Terrain, owner.bodyChunks[chunk], loop: false, 0.9f, 1f);
					dripPlayed = true;
				}
			}
			if (!owner.DoSteam)
			{
				if (owner.dead)
				{
					num = 1f;
				}
				FloatRect confines = owner.room.TileRect(owner.room.GetTilePosition(owner.bodyChunks[chunk].pos)).Grow(200f);
				owner.steam.EmitSmoke(owner.bodyChunks[chunk].pos, direction.Value * (20f * num), confines, 0.5f * num);
			}
		}
	}

	public class LarvaHolder
	{
		public BodyChunk bodyChunk;

		public Vector2 position;

		public float horizontalOffset;

		public float verticalOffset;

		public Vector2 perpendicular;

		public Vector2 pos;

		public Vector2 lastPos;

		public bool larvaDetached;

		public int blobIndex;

		public float posAlongBody;

		public bool closed;

		public Counter timeToDislodge = new Counter(100, 0, countsUp: true);

		public Larva.AbstractLarva abstractLarva;

		public Vector2 larvaPos;

		public Vector2 larvaDir;

		public Counter dripCounter = new Counter(20);

		public int index;

		public bool forceRelease;

		public bool retracted;

		public bool playedDislodgeSound;

		public StaticSoundLoop pullingSoundLoop;

		public bool hasLarva { get; set; }

		public BoxWormGraphics.Data gm => (bodyChunk.owner as BoxWorm).graphicData;

		public Room room => bodyChunk.owner.room;

		public Larva larva => abstractLarva.realizedObject as Larva;

		public float dst { get; set; }

		public LarvaHolder(int index, BodyChunk parentBodyChunk, float horizontalOffset, float verticalOffset, bool hasLarva)
		{
			this.index = index;
			bodyChunk = parentBodyChunk;
			this.horizontalOffset = horizontalOffset;
			this.verticalOffset = verticalOffset;
			this.hasLarva = hasLarva;
			blobIndex = -1;
			posAlongBody = (float)bodyChunk.index - 0.25f + verticalOffset * 0.35f;
			dripCounter.SetToMin();
		}

		public void Update()
		{
			if (gm == null)
			{
				return;
			}
			if (blobIndex == -1)
			{
				blobIndex = gm.larvaHolderSpritePositions[index];
			}
			lastPos = pos;
			pos = gm.blobPositions[blobIndex];
			if (room == null)
			{
				return;
			}
			if (pullingSoundLoop == null)
			{
				pullingSoundLoop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Box_Worm_Pulling_Larva_LOOP, pos, room, 0f, 1f);
			}
			pullingSoundLoop.pos = pos;
			pullingSoundLoop.Update();
			if (!dripCounter.isFinished)
			{
				Vector2 vector = pos + larvaDir * (gm.blobScales[blobIndex] * 10f);
				for (int num = Mathf.Min((int)dripCounter / 3, 5); num >= 0; num--)
				{
					room.AddObject(new WaterDrip(vector - larvaDir * 0.2f + Custom.RNV() * 3.5f, (larvaDir.normalized * 10f + Custom.RNV() * 1.5f) * dripCounter.normalized, waterColor: true));
				}
				dripCounter.Tick();
			}
			if (hasLarva)
			{
				larvaDir = Custom.DegToVec(Custom.AimFromOneVectorToAnother(gm.blobPositions[blobIndex], gm.blobPositions[blobIndex + 1]) + horizontalOffset * 90f);
				larvaPos = pos + larvaDir * (gm.blobScales[blobIndex] * 10f);
				if (abstractLarva == null)
				{
					abstractLarva = new Larva.AbstractLarva(room.world, null, room.GetWorldCoordinate(position), room.game.GetNewID());
					room.abstractRoom.AddEntity(abstractLarva);
					abstractLarva.RealizeInRoom();
				}
				larva.retracted = retracted && !timeToDislodge.isFinished;
				ManageLarvaDetachment();
			}
		}

		public void ManageLarvaDetachment()
		{
			if (forceRelease)
			{
				timeToDislodge.SetToMax();
			}
			if (!timeToDislodge.isFinished)
			{
				larva.bodyChunks[0].pos = larvaPos;
				larva.bodyChunks[0].collideWithTerrain = false;
				larva.bodyChunks[0].mass = 0.5f;
				larva.detachDirection = larvaDir;
				if (timeToDislodge.normalized > 0f && !playedDislodgeSound)
				{
					pullingSoundLoop.volume = Mathf.Lerp(pullingSoundLoop.volume, Mathf.Clamp01(dst / 6f), 0.2f);
				}
				else
				{
					pullingSoundLoop.volume = Mathf.Lerp(pullingSoundLoop.volume, 0f, 0.02f);
				}
			}
			else
			{
				pullingSoundLoop.volume = 0f;
				if (!playedDislodgeSound)
				{
					room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Rip_Out_Larva, larva.firstChunk);
					playedDislodgeSound = true;
				}
				larva.bodyChunks[0].collideWithTerrain = true;
				larva.bodyChunks[0].mass = 0.2f;
				dripCounter.Reset();
				if (larva.grabbedBy.Count > 0)
				{
					(bodyChunk.owner as BoxWorm).ai.CreatureGrabsLarva(-0.13f, larva.grabbedBy[0].grabber.abstractCreature);
				}
				if (!larva.edible)
				{
					larva.lifeCounter.Reset();
				}
			}
			if (larva.grabbedBy.Count == 0 && !timeToDislodge.isFinished)
			{
				larva.bodyChunks[0].vel = Vector2.zero;
				timeToDislodge.Reset();
			}
			else if (!timeToDislodge.isFinished)
			{
				bodyChunk.vel += larva.bodyChunks[0].vel * 0.2f;
			}
			larva.edible = timeToDislodge.isFinished;
			hasLarva = !timeToDislodge.isFinished;
			dst = Mathf.Abs(larva.bodyChunks[0].vel.x);
			if (dst > 3f && (bodyChunk.owner as BoxWorm).steamAvailable.isFinished)
			{
				timeToDislodge.Tick();
			}
			larva.bodyChunks[0].vel = Vector2.zero;
		}
	}

	public class Larva : PlayerCarryableItem, IDrawable, IPlayerEdible
	{
		public class AbstractLarva : AbstractPhysicalObject
		{
			public AbstractLarva(World world, PhysicalObject obj, WorldCoordinate pos, EntityID ID)
				: base(world, WatcherEnums.AbstractObjectType.FireSpriteLarva, obj, pos, ID)
			{
			}

			public override string ToString()
			{
				string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}", base.IDAndRippleLayerString, type.ToString(), pos.SaveToString());
				baseString = SaveState.SetCustomData(this, baseString);
				return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
			}
		}

		public bool edible;

		public bool retracted;

		public Vector2 detachDirection;

		public float vel;

		public Counter lifeCounter = new Counter(500);

		public float randomOffset;

		public float rotation;

		public float lastRotation;

		public int bites = 3;

		public int FoodPoints => 1;

		public bool Edible => edible;

		public bool AutomaticPickUp => false;

		public int BitesLeft => bites;

		public Larva(AbstractPhysicalObject abstractPhysicalObject)
			: base(abstractPhysicalObject)
		{
			base.bodyChunks = new BodyChunk[1];
			base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 4f, 0.2f);
			bodyChunkConnections = new BodyChunkConnection[0];
			base.airFriction = 0.95f;
			base.gravity = 0.9f;
			bounce = 0.2f;
			surfaceFriction = 0.25f;
			collisionLayer = 0;
			base.waterFriction = 0.95f;
			base.buoyancy = 1.1f;
			randomOffset = Random.value;
			edible = true;
			lifeCounter.SetToMin();
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (retracted && grabbedBy.Count > 0)
			{
				while (grabbedBy.Count > 0)
				{
					grabbedBy[0].Release();
				}
			}
			if (edible)
			{
				if ((double)detachDirection.magnitude > 0.0001)
				{
					base.bodyChunks[0].vel += detachDirection * 5f;
					foreach (Grasp item in grabbedBy)
					{
						item.grabber.mainBodyChunk.vel += detachDirection * 5f;
					}
					detachDirection *= 0.9f;
				}
				lifeCounter.Tick();
				lastRotation = rotation;
				if (base.bodyChunks[0].ContactPoint == Custom.fourDirections[1])
				{
					rotation = Mathf.LerpAngle(rotation, Custom.VecToDeg(Custom.DegToVec(rotation) * Vector2.right), 0.3f);
				}
				else
				{
					rotation += Mathf.DeltaAngle(rotation, Custom.VecToDeg(new Vector2(base.bodyChunks[0].vel.x, 0f - base.bodyChunks[0].vel.y) - Vector2.down * 3f)) * 0.5f;
				}
			}
			else
			{
				rotation = Custom.VecToDeg(detachDirection);
			}
		}

		public override void PlaceInRoom(Room placeRoom)
		{
			base.PlaceInRoom(placeRoom);
			base.bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile));
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[0]);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void BitByPlayer(Grasp grasp, bool eu)
		{
			bites--;
			room.PlaySound((bites == 0) ? WatcherEnums.WatcherSoundID.Slugcat_Eat_Box_Worm_Larva : WatcherEnums.WatcherSoundID.Slugcat_Bite_Box_Worm_Larva, base.firstChunk);
			base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
			if (bites < 1)
			{
				(grasp.grabber as Player).ObjectEaten(this);
				grasp.Release();
				Destroy();
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(base.bodyChunks[0].lastPos, base.bodyChunks[0].pos, timeStacker) - camPos;
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].isVisible = edible;
			sLeaser.sprites[0].color = new Color(1f - (float)bites / 3f, lifeCounter.normalized, randomOffset, 10f);
			sLeaser.sprites[0].rotation = Mathf.LerpAngle(lastRotation, rotation, timeStacker);
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["BoxWormLarvaFood"];
			sLeaser.sprites[0].scale = 1f;
			sLeaser.sprites[0].scaleY = 3f;
			sLeaser.sprites[0].anchorY = 0.75f;
			sLeaser.sprites[0].isVisible = true;
			AddToContainer(sLeaser, rCam, null);
		}

		public void ThrowByPlayer()
		{
		}
	}

	public BoxWormAI ai;

	public IndividualVariations individual;

	public Counter steamAvailable = new Counter(800);

	public List<IntVector2> headTraveledTiles;

	public int minSteamRequired = 100;

	public float attackRange = 300f;

	public int bodyChunkCount = 7;

	public float bodyChunkSize;

	public float bodyChunkMass;

	public float totalMass = 1.4f;

	public Vector2 rootPoint;

	public Vector2 headPoint;

	public Vector2 movementTarget;

	public bool needsReroot = true;

	public float uprightAmount;

	public bool headMoving = true;

	public Counter switchTimer = new Counter(10);

	public Counter movementTimer = new Counter(100);

	public Counter attackTimer = new Counter(200);

	public Counter helpRecievedTimer = new Counter(40);

	public Counter transitionTimer = new Counter(80, 0, countsUp: true);

	public Counter stuckTimer = new Counter(90);

	public Counter trickyTerrainTimer = new Counter(400, 0, countsUp: true);

	public Counter helpCallTimer = new Counter(20, 0, countsUp: true);

	public Counter releaseSteamTimer = new Counter(20);

	public SteamSmoke steam;

	public FloatRect steamConfines;

	public int bodySupport;

	public Vector2 destPos;

	public bool debug;

	public DebugSprite moveTargetSprite;

	public DebugSprite moveStartSprite;

	public IntVector2 lastDestTile;

	public int tailMovementIndex;

	public float MovementSpeed = 12f;

	public List<CreatureSteamDamage> steamDamageCreatureList;

	public List<EntityID> spasmingCreatures;

	public MovementConnection connection;

	public GroundGrabber[] groundGrabbers;

	public BoxWormGraphics.Data graphicData;

	public bool needToGetCloserToGround;

	public Counter helpSoundCounter = new Counter(80, 0, countsUp: true);

	public ChunkSoundEmitter helpPingSE;

	public ChunkSoundEmitter helpPing2SE;

	public ChunkSoundEmitter shortSteamSE;

	public ChunkSoundEmitter shortSpitSteamSE;

	public ChunkSoundEmitter spasmSE;

	public Counter spasmSoundCounter = new Counter(40);

	public ChunkSoundEmitter extendSE;

	public ChunkSoundEmitter retractSE;

	public List<Injury> injuries = new List<Injury>();

	public Counter helpPromiseCounter = new Counter(160);

	public StaticSoundLoop steamSoundLoop;

	public StaticSoundLoop headMotorLoop;

	public StaticSoundLoop steamRumbleLoop;

	public bool CanMove => (int)steamAvailable >= minSteamRequired;

	public float SteamPower => Mathf.Clamp01(Mathf.InverseLerp(minSteamRequired, minSteamRequired + 400, (int)steamAvailable));

	public float archAmount { get; set; }

	public bool steamCoolDown { get; set; }

	public LarvaHolder[] larvaHolders { get; set; }

	public float totalWormLength { get; set; }

	public bool DoSteam
	{
		get
		{
			if (base.safariControlled)
			{
				return inputWithDiagonals?.thrw ?? false;
			}
			if (attackTimer.normalized > 0.2f)
			{
				return attackTimer.normalized < 0.5f;
			}
			return false;
		}
	}

	public float SteamPowerWithOffset(int offset)
	{
		return Mathf.Clamp01(Mathf.InverseLerp(minSteamRequired + offset, minSteamRequired + 400 + offset, (int)steamAvailable));
	}

	public BoxWorm(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		individual = new IndividualVariations(abstractCreature);
		larvaHolders = new LarvaHolder[individual.numberOfLarvaHolders];
		bodyChunkSize = 12f * individual.sizeVariation;
		bodyChunkMass = totalMass / (float)bodyChunkCount;
		totalWormLength = (float)bodyChunkCount * bodyChunkSize;
		base.bodyChunks = new BodyChunk[bodyChunkCount];
		for (int i = 0; i < bodyChunkCount; i++)
		{
			base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(bodyChunkSize * (float)i * 10f, 0f), bodyChunkSize, bodyChunkMass);
		}
		bodyChunkConnections = new BodyChunkConnection[bodyChunkCount - 1];
		for (int j = 0; j < bodyChunkConnections.Length; j++)
		{
			bodyChunkConnections[j] = new BodyChunkConnection(base.bodyChunks[j], base.bodyChunks[j + 1], bodyChunkSize * 2f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		}
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.01f;
		surfaceFriction = 0.1f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		windAffectiveness = 0.1f;
		steamAvailable.SetToMin();
		headTraveledTiles = new List<IntVector2>();
		steamDamageCreatureList = new List<CreatureSteamDamage>();
		spasmingCreatures = new List<EntityID>();
		abstractCreature.HypothermiaImmune = true;
		groundGrabbers = new GroundGrabber[base.bodyChunks.Length];
		for (int k = 0; k < base.bodyChunks.Length; k++)
		{
			groundGrabbers[k] = new GroundGrabber(base.bodyChunks[k]);
		}
		for (int l = 0; l < larvaHolders.Length; l++)
		{
			int num = (l + 2) / 2;
			larvaHolders[l] = new LarvaHolder(l, base.bodyChunks[num], individual.horizontalOffsets[l], l % 2, individual.hasLarva[l]);
		}
		releaseSteamTimer.SetToMin();
		helpSoundCounter.SetToMin();
		helpPromiseCounter.SetToMin();
		helpRecievedTimer.SetToMin();
	}

	public override Color ShortCutColor()
	{
		return BoxWormGraphics.BaseColor(ai.creature.Room.realizedRoom);
	}

	public void UpdateTimers()
	{
		attackTimer.Tick();
		switchTimer.Tick();
		movementTimer.Tick();
		steamAvailable.Tick();
		if ((base.dead && !steamAvailable.isFinished) || (base.safariControlled && ai.behavior == BoxWormAI.Behavior.Dormant))
		{
			steamAvailable.Add(-5);
		}
		helpRecievedTimer.Tick();
		transitionTimer.Tick();
		trickyTerrainTimer.Tick();
		helpSoundCounter.Tick();
		spasmSoundCounter.Tick();
		helpPromiseCounter.Tick();
	}

	public override void InitiateGraphicsModule()
	{
		if (debug)
		{
			if (base.graphicsModule == null)
			{
				GraphicsModule graphicsModule2 = (base.graphicsModule = new DebugGraphicsModule(this));
			}
			return;
		}
		if (base.graphicsModule == null)
		{
			GraphicsModule graphicsModule2 = (base.graphicsModule = new BoxWormGraphics(this));
		}
		base.graphicsModule.Reset();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		if (steam != null)
		{
			steam.RemoveFromRoom();
			newRoom.AddObject(steam);
		}
		else
		{
			steam = new SteamSmoke(newRoom);
			newRoom.AddObject(steam);
		}
	}

	public override void Update(bool eu)
	{
		if (moveTargetSprite == null && debug)
		{
			FSprite fSprite = new FSprite("pixel");
			fSprite.alpha = 0.5f;
			moveTargetSprite = new DebugSprite(Vector2.zero, fSprite, room);
			room.AddObject(moveTargetSprite);
			fSprite = new FSprite("pixel");
			fSprite.alpha = 1f;
			moveStartSprite = new DebugSprite(Vector2.zero, fSprite, room);
			room.AddObject(moveStartSprite);
		}
		base.Update(eu);
		UpdateTimers();
		if (room == null)
		{
			return;
		}
		base.mainBodyChunk.collideWithTerrain = true;
		base.bodyChunks[6].collideWithTerrain = true;
		LarvaHolder[] array = larvaHolders;
		foreach (LarvaHolder larvaHolder in array)
		{
			larvaHolder.retracted = CanMove;
			larvaHolder.Update();
			if (larvaHolder.hasLarva && base.dead && steamAvailable.isFinished)
			{
				larvaHolder.forceRelease = true;
			}
		}
		if (steam == null)
		{
			steam = new SteamSmoke(room);
			room.AddObject(steam);
		}
		UpdateSound();
		UpdateInjuries();
		if (!CanLetGo())
		{
			helpPromiseCounter.Reset();
		}
		if (base.graphicsModule != null && graphicData != null)
		{
			graphicData.Update();
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (base.stun > 0 || base.dead)
		{
			Unroot();
			return;
		}
		GroundGrabber[] array2 = groundGrabbers;
		foreach (GroundGrabber obj in array2)
		{
			obj.Update();
			obj.speed = transitionTimer.normalized;
		}
		if (!steam.Dead)
		{
			CheckForSteamDamage();
		}
		BodySupport();
		ai.Update();
		base.CollideWithObjects = (int)helpRecievedTimer > 0;
	}

	public void UpdateInjuries()
	{
		foreach (Injury injury in injuries)
		{
			injury.Update();
		}
	}

	public void UpdateSound()
	{
		if (steamSoundLoop != null)
		{
			steamSoundLoop.pos = base.mainBodyChunk.pos;
			if (!DoSteam || steamCoolDown)
			{
				steamSoundLoop.volume = Mathf.Lerp(steamSoundLoop.volume, 0f, 0.075f);
				steamSoundLoop.pitch = 0.5f + steamAvailable.normalized * 0.5f;
				if (steamSoundLoop.volume < 0.1f)
				{
					steamSoundLoop.volume = 0f;
				}
			}
			steamSoundLoop.Update();
		}
		float normalized = helpSoundCounter.normalized;
		float val = Mathf.Abs(normalized * 2f - 1f);
		if (helpPingSE != null)
		{
			helpPingSE.volume = Mathf.Clamp01(Custom.LerpMap(normalized, 0f, 0.2f, 0f, 0.4f));
			helpPingSE.pitch = Mathf.Clamp01(Custom.LerpMap(normalized, 0f, 0.5f, 0f, 1f));
			if (helpPing2SE != null)
			{
				helpPing2SE.volume = helpPingSE.volume;
				helpPing2SE.pitch = helpPingSE.pitch;
			}
		}
		if (shortSpitSteamSE != null)
		{
			shortSpitSteamSE.volume = Mathf.Clamp01(Custom.LerpMap(val, 0.2f, 0.7f, 0.3f, 0f));
			shortSpitSteamSE.pitch = Mathf.Clamp01(Custom.LerpMap(val, 2f, 0.4f, 1f, 0.8f));
		}
		if (shortSteamSE != null)
		{
			shortSteamSE.volume = Mathf.Clamp01(Custom.LerpMap(val, 0.5f, 1f, 0.6f, 0f));
			shortSteamSE.pitch = Mathf.Clamp01(Custom.LerpMap(val, 0.5f, 1f, 1f, 0.8f));
		}
		if (extendSE != null && !extendSE.soundStillPlaying)
		{
			extendSE = null;
		}
		if (retractSE != null && !retractSE.soundStillPlaying)
		{
			retractSE = null;
		}
		if ((int)steamAvailable > 100 || !helpRecievedTimer.isFinished)
		{
			if (headMotorLoop == null)
			{
				headMotorLoop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Box_Worm_Spin_Head_LOOP, base.mainBodyChunk.pos, room, 1f, 0f);
			}
			if (headMotorLoop != null && graphicData != null)
			{
				float num = 0f;
				num = Mathf.Abs(graphicData.lastHeadRotOffset - graphicData.headRotOffset) / 20f;
				num = Mathf.Lerp(num, Mathf.Max(num, Mathf.Abs(graphicData.lastHeadRotation - graphicData.headRot) / 10f), 0.1f);
				num = Mathf.Max(num, Mathf.Abs(graphicData.lastHeadClosing - graphicData.headClosing) * 30f);
				num = Mathf.Clamp01(num);
				num *= (base.dead ? 0f : 1f);
				headMotorLoop.pitch = Mathf.Lerp(headMotorLoop.pitch, 0.2f + Mathf.Clamp01(num * 4f) * 0.2f, 0.9f);
				headMotorLoop.volume = Mathf.Lerp(headMotorLoop.volume, Mathf.Clamp01(num * 10f) * 0.4f, 0.9f);
				headMotorLoop.pos = base.mainBodyChunk.pos;
				headMotorLoop.Update();
			}
		}
		else if (headMotorLoop != null)
		{
			headMotorLoop.volume = 0f;
		}
		if ((int)steamAvailable > 0)
		{
			if (steamRumbleLoop == null)
			{
				steamRumbleLoop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Sandstorm_Sand, base.bodyChunks[base.bodyChunks.Length / 2].pos, room, 0f, 0.1f);
			}
			if (steamRumbleLoop != null)
			{
				steamRumbleLoop.pitch = 0.001f + SteamPower * 0.06f - (1f - Mathf.Abs(attackTimer.normalized * 2f - 1f)) * 0.03f;
				steamRumbleLoop.volume = Mathf.Clamp01(SteamPower * 4f);
				steamRumbleLoop.pos = base.bodyChunks[base.bodyChunks.Length / 2].pos;
				steamRumbleLoop.Update();
			}
		}
		if (spasmSE != null)
		{
			spasmSE.volume = Mathf.Lerp(spasmSE.volume, stuckTimer.isFinished ? 0f : ((1f - Mathf.Abs(Custom.LerpMap(stuckTimer.normalized, 0.5f, 0f, 0f, 2f) - 1f)) * 0.6f), 0.05f + spasmSoundCounter.normalized);
			if (spasmSE.volume < 0.01f && spasmSoundCounter.isFinished)
			{
				spasmSE = null;
			}
		}
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos onAppendagePos, Vector2 direction)
	{
		if (chunk == base.firstChunk)
		{
			if (room != null)
			{
				room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, base.mainBodyChunk);
			}
			return false;
		}
		if (source is Spear spear)
		{
			injuries.Add(new Injury(this, spear, chunk.index));
		}
		if (base.graphicsModule != null)
		{
			(base.graphicsModule as BoxWormGraphics).rearrange = true;
		}
		return true;
	}

	public void CrawlDirectionSwitch()
	{
		bool flag = archAmount > 0.8f;
		bool flag2 = archAmount < 0.015f;
		bool isFinished = movementTimer.isFinished;
		if (headMoving)
		{
			groundGrabbers[6].Grab();
			if ((flag2 || isFinished) && IsBodyChunkOnGround(0))
			{
				switchTimer.ResetIfFinished();
				if ((int)switchTimer <= 1)
				{
					groundGrabbers[6].Release();
					headMoving = false;
					movementTimer.Reset();
				}
			}
			return;
		}
		groundGrabbers[0].Grab();
		if ((flag || isFinished) && IsBodyChunkOnGround(6))
		{
			switchTimer.ResetIfFinished();
			if ((int)switchTimer <= 1)
			{
				groundGrabbers[0].Release();
				headMoving = true;
				headTraveledTiles.Clear();
				tailMovementIndex = 0;
				movementTimer.Reset();
			}
		}
	}

	public void ReleaseAllGrabbers()
	{
		GroundGrabber[] array = groundGrabbers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Release();
		}
	}

	public void CheckForSteamDamage()
	{
		int count = room.abstractRoom.creatures.Count;
		for (int i = 0; i < count; i++)
		{
			CheckCreature(room.abstractRoom.creatures[i]);
		}
	}

	public void ApplySteamDamage(AbstractCreature aCreature, BodyChunk chunk, int damage)
	{
		if (damage > 0)
		{
			if (aCreature.realizedCreature is Player)
			{
				CreatureSteamDamage creatureSteamDamage = TrackDamage(aCreature, damage);
				int num = (int)(Random.value * 1.8f) * Mathf.Max(0, creatureSteamDamage.damage - creatureSteamDamage.health);
				damage *= (creatureSteamDamage.IsLethal ? (2 + num) : 2);
			}
			aCreature.realizedCreature.Violence(base.mainBodyChunk, null, chunk, null, DamageType.Water, (float)damage / 200f, 0f);
			aCreature.realizedCreature.stun++;
			if (ModManager.HypothermiaModule)
			{
				aCreature.Hypothermia = Mathf.Lerp(aCreature.Hypothermia, 0f, 0.05f);
			}
		}
	}

	public void CheckCreature(AbstractCreature abstractCreature)
	{
		if (abstractCreature.realizedCreature != null && !abstractCreature.realizedCreature.dead && !(abstractCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BoxWorm) && !(abstractCreature.creatureTemplate.type == WatcherEnums.CreatureTemplateType.FireSprite) && steamConfines.Vector2Inside(room.MiddleOfTile(abstractCreature.pos)))
		{
			int num = abstractCreature.realizedCreature.bodyChunks.Length;
			int totalDamage = 0;
			for (int i = 0; i < num; i++)
			{
				CheckBodyChunk(ref totalDamage, abstractCreature, abstractCreature.realizedCreature.bodyChunks[i]);
			}
			ApplySpasm(abstractCreature, totalDamage * 200);
		}
	}

	public void CheckBodyChunk(ref int totalDamage, AbstractCreature aCreature, BodyChunk chunk)
	{
		int steamHits = 0;
		if (steamConfines.Vector2Inside(chunk.pos))
		{
			for (int i = 0; i < steam.particles.Count; i++)
			{
				CheckParticleCollision(ref steamHits, i, chunk);
			}
			totalDamage += steamHits;
			ApplySteamDamage(aCreature, chunk, steamHits);
		}
	}

	public void CheckParticleCollision(ref int steamHits, int particleIndex, BodyChunk chunk)
	{
		SmokeSystem.SmokeSystemParticle smokeSystemParticle = steam.particles[particleIndex];
		if (!(smokeSystemParticle.life < 0.1f) && !(Vector2.Distance(smokeSystemParticle.pos, chunk.pos) > chunk.rad + 20f))
		{
			steamHits += (int)(smokeSystemParticle.life * 7f);
		}
	}

	public CreatureSteamDamage TrackDamage(AbstractCreature abstractCreature, int damage)
	{
		foreach (CreatureSteamDamage steamDamageCreature in steamDamageCreatureList)
		{
			if (steamDamageCreature.ID == abstractCreature.ID)
			{
				steamDamageCreature.AddDamage(damage);
				return steamDamageCreature;
			}
		}
		CreatureSteamDamage creatureSteamDamage = new CreatureSteamDamage(abstractCreature, 2);
		creatureSteamDamage.AddDamage(damage);
		steamDamageCreatureList.Add(creatureSteamDamage);
		return creatureSteamDamage;
	}

	public void ApplySpasm(AbstractCreature abstractCreature, int duration)
	{
		if (duration == 0)
		{
			return;
		}
		foreach (EntityID spasmingCreature in spasmingCreatures)
		{
			if (spasmingCreature == abstractCreature.ID)
			{
				return;
			}
		}
		spasmingCreatures.Add(abstractCreature.ID);
		room.AddObject(new CreatureSpasmer(abstractCreature.realizedCreature, allowDead: false, duration));
	}

	public void FinalizeSteamDamage()
	{
		foreach (CreatureSteamDamage steamDamageCreature in steamDamageCreatureList)
		{
			steamDamageCreature.FinalizeDamage(1000);
		}
	}

	public void BodySupport()
	{
		int num = 0;
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			if (IsBodyChunkOnGround(i))
			{
				num++;
			}
		}
		bodySupport = num;
		if (bodySupport == 0)
		{
			ReleaseAllGrabbers();
		}
	}

	public void DormantStance()
	{
		if (needsReroot || Vector2.Distance(rootPoint, base.bodyChunks[3].pos) > bodyChunkSize * 4f)
		{
			Reroot();
			return;
		}
		if (uprightAmount < 4f)
		{
			uprightAmount += 0.1f;
		}
		base.bodyChunks[0].vel = MoveToPoint(base.bodyChunks[0].pos, headPoint, uprightAmount);
		base.bodyChunks[3].vel = MoveToPoint(base.bodyChunks[3].pos, rootPoint, 1f + uprightAmount * 2.5f);
		stuckTimer.Reset();
	}

	public void Reroot()
	{
		if (IsBodyChunkOnGround(3))
		{
			rootPoint = room.MiddleOfTile(base.bodyChunks[3].pos);
			headPoint = rootPoint + Vector2.up * (bodyChunkSize * 6f);
			needsReroot = false;
		}
	}

	public void Unroot()
	{
		needsReroot = true;
		uprightAmount = 0f;
	}

	public void FollowPath()
	{
		if (room == null || bodySupport <= 0 || !CanMove)
		{
			return;
		}
		Arch();
		CrawlDirectionSwitch();
		if (switchTimer.isFinished)
		{
			stuckTimer.Tick();
			float movementStrength = MovementSpeed * (trickyTerrainTimer.normalized * 0.5f + 0.5f) * SteamPower * transitionTimer.normalized;
			if (headMoving)
			{
				MoveHead(movementStrength);
			}
			else
			{
				MoveTail(movementStrength);
			}
			base.GoThroughFloors = connection.type == MovementConnection.MovementType.DropToFloor;
		}
	}

	public void MoveHead(float movementStrength)
	{
		base.mainBodyChunk.collideWithTerrain = false;
		base.bodyChunks[6].collideWithTerrain = true;
		StandardPather standardPather = ai.pathFinder as StandardPather;
		if ((int)switchTimer > 1)
		{
			IntVector2 pos = lastDestTile + new IntVector2(0, Mathf.Clamp(room.aimap.getAItile(lastDestTile).fallRiskTile.y - lastDestTile.y, -1, 1));
			connection = standardPather.FollowPath(room.GetWorldCoordinate(pos), actuallyFollowingThisPath: true);
		}
		else
		{
			connection = standardPather.FollowPath(room.GetWorldCoordinate(room.MiddleOfTile(base.mainBodyChunk.pos)), actuallyFollowingThisPath: true);
		}
		if (base.safariControlled)
		{
			Player.InputPackage? inputPackage = inputWithDiagonals;
			if (inputPackage.HasValue)
			{
				Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
				Vector2 vector = Vector2.zero;
				if (valueOrDefault.analogueDir.magnitude > 0f)
				{
					vector = valueOrDefault.analogueDir.normalized;
				}
				else if (valueOrDefault.x != 0 || valueOrDefault.y != 0)
				{
					vector = new Vector2(valueOrDefault.x, valueOrDefault.y).normalized;
				}
				if (vector != Vector2.zero)
				{
					connection = new MovementConnection(MovementConnection.MovementType.Standard, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + vector * 40f), 2);
				}
			}
		}
		if (destPos == default(Vector2))
		{
			destPos = groundGrabbers[0].grabDestination ?? room.MiddleOfTile(base.mainBodyChunk.pos);
			if (debug)
			{
				moveStartSprite.sprite.color = Color.yellow;
				moveStartSprite.pos = room.MiddleOfTile(destPos);
			}
			return;
		}
		if (debug)
		{
			moveTargetSprite.sprite.alpha = 0.5f;
		}
		if (debug)
		{
			moveTargetSprite.pos = room.MiddleOfTile(connection.DestTile);
		}
		needToGetCloserToGround = movementTimer.isFinished || archAmount < 0.001f || needToGetCloserToGround;
		if (needToGetCloserToGround)
		{
			if (groundGrabbers[0].isBodyChunkOnGround)
			{
				needToGetCloserToGround = false;
				trickyTerrainTimer.Reset();
				movementTimer.Set(movementTimer.min);
			}
			if (headTraveledTiles.Count != 0)
			{
				IntVector2 intVector = headTraveledTiles[headTraveledTiles.Count - 1];
				if (needToGetCloserToGround)
				{
					int floorAltitude = room.aimap.getAItile(intVector).floorAltitude;
					if (floorAltitude <= 5)
					{
						intVector.y -= floorAltitude;
					}
					if (debug)
					{
						moveStartSprite.sprite.color = Color.red;
					}
				}
				MoveChunkToPoint(0, room.MiddleOfTile(intVector), movementStrength * 0.5f);
				if (Custom.ManhattanDistance(intVector, room.GetTilePosition(base.mainBodyChunk.pos)) <= 1)
				{
					headTraveledTiles.Pop();
				}
				if (debug)
				{
					moveStartSprite.pos = room.MiddleOfTile(intVector);
				}
			}
			else
			{
				needToGetCloserToGround = false;
				MoveChunkToPoint(0, base.bodyChunks[6].pos, movementStrength * 0.5f);
				if (debug)
				{
					moveStartSprite.sprite.color = Color.red;
					moveStartSprite.pos = room.MiddleOfTile(base.bodyChunks[6].pos);
				}
			}
			return;
		}
		bool flag = false;
		for (int i = 0; i < headTraveledTiles.Count; i++)
		{
			if (connection.DestTile == headTraveledTiles[i])
			{
				flag = true;
				break;
			}
		}
		if (stuckTimer.isFinished)
		{
			connection = standardPather.FollowPath(connection.destinationCoord + room.GetTilePosition(StuckSpasm()), actuallyFollowingThisPath: true);
		}
		if (connection.DestTile != Custom.fourDirectionsAndZero[0] && lastDestTile != connection.DestTile && !flag)
		{
			stuckTimer.Set(stuckTimer.max + Random.Range(0, 40));
			headTraveledTiles.Add(lastDestTile);
			lastDestTile = connection.DestTile;
			destPos = room.MiddleOfTile(lastDestTile);
		}
		IntVector2 intVector2 = Custom.fourDirections[(Custom.DirVec(base.mainBodyChunk.pos, destPos).x <= 0f) ? 2 : 0];
		IntVector2 pos2 = connection.DestTile + intVector2;
		IntVector2 pos3 = connection.DestTile + intVector2 * 2;
		if (room.aimap.getAItile(pos3).fallRiskTile.y - room.aimap.getAItile(pos2).fallRiskTile.y > 2 && archAmount < 0.8f && IsBodyChunkOnGround(0))
		{
			trickyTerrainTimer.Reset();
			movementTimer.Set(movementTimer.min);
			if (debug)
			{
				moveStartSprite.sprite.color = Color.magenta;
			}
			return;
		}
		if (debug)
		{
			moveTargetSprite.sprite.alpha = 0.5f;
			moveTargetSprite.pos = room.MiddleOfTile(connection.DestTile);
			moveStartSprite.sprite.color = Color.green;
			moveStartSprite.pos = room.MiddleOfTile(destPos);
		}
		MoveChunkToPoint(0, destPos, movementStrength);
		if (extendSE == null)
		{
			extendSE = room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Crawl_Extend, base.mainBodyChunk, loop: false, Mathf.Clamp01(base.mainBodyChunk.vel.magnitude * 0.04f), Random.Range(0.4f, 6f));
		}
	}

	public void MoveTail(float movementStrength)
	{
		base.mainBodyChunk.collideWithTerrain = true;
		base.bodyChunks[6].collideWithTerrain = false;
		if (headTraveledTiles.Count == 0 || tailMovementIndex >= headTraveledTiles.Count - 1)
		{
			MoveChunkToPoint(6, base.mainBodyChunk.pos, movementStrength);
			return;
		}
		if (Vector2.Distance(base.bodyChunks[6].pos, base.mainBodyChunk.pos) < Vector2.Distance(room.MiddleOfTile(headTraveledTiles[tailMovementIndex]), base.mainBodyChunk.pos))
		{
			tailMovementIndex++;
			return;
		}
		Vector2 destination = room.MiddleOfTile(headTraveledTiles[tailMovementIndex]);
		if (room.GetTilePosition(base.bodyChunks[6].pos) == headTraveledTiles[tailMovementIndex])
		{
			tailMovementIndex++;
		}
		MoveChunkToPoint(6, destination, movementStrength);
		if (retractSE == null)
		{
			retractSE = room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Crawl_Retract, base.bodyChunks[6], loop: false, Mathf.Clamp01(base.bodyChunks[6].vel.magnitude * 0.04f), Random.Range(0.5f, 1f));
		}
	}

	public void MoveChunkToPoint(int index, Vector2 destination, float movementStrength)
	{
		base.bodyChunks[index].vel = MoveToPoint(base.bodyChunks[index].pos, destination + Vector2.up * 10f + StuckSpasm(), movementStrength);
		base.bodyChunks[3].vel += StuckSpasm();
	}

	public void AttackStance()
	{
		groundGrabbers[6].Grab();
		groundGrabbers[5].Grab();
		if (bodySupport > 0)
		{
			Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, ai.TargetPosition);
			Vector2 vector2 = Vector2.right * ((0f - vector.x) * 2f);
			Vector2 vector3 = MoveToPoint(base.bodyChunks[2].pos, base.bodyChunks[4].pos + Vector2.up * (bodyChunkSize * 6f), 4f);
			Vector2 up = Vector2.up;
			Vector2 vector4 = MoveToPoint(base.bodyChunks[0].pos, base.bodyChunks[1].pos + vector * (bodyChunkSize * (4f + 4f * (1f - vector.x) * Mathf.Clamp01(Mathf.Sign(vector.y)))), 2f);
			float num = 0.75f + SteamPower * 0.25f;
			float normalized = transitionTimer.normalized;
			normalized = (0f - normalized) * normalized * (2f * normalized - 3f);
			normalized *= num;
			base.bodyChunks[0].vel += vector4 * normalized;
			base.bodyChunks[1].vel += up * normalized;
			base.bodyChunks[2].vel += vector3 * normalized;
			base.bodyChunks[3].vel += vector2 * normalized;
			headMoving = true;
			movementTimer.Reset();
		}
	}

	public void PerformAttack()
	{
		AttackStance();
		if (attackTimer.isFinished && transitionTimer.isFinished)
		{
			attackTimer.Reset();
			FinalizeSteamDamage();
			spasmingCreatures.Clear();
		}
		if (!transitionTimer.isFinished)
		{
			attackTimer.Reset();
		}
		if (steamAvailable.normalized > (base.safariControlled ? 0.4f : 0.8f))
		{
			steamCoolDown = false;
		}
		if (steamAvailable.normalized < (base.safariControlled ? 0.2f : 0.4f) || steamCoolDown)
		{
			steamCoolDown = true;
		}
		else if (DoSteam)
		{
			if (steamSoundLoop == null)
			{
				steamSoundLoop = new StaticSoundLoop(WatcherEnums.WatcherSoundID.Box_Worm_Spitting_Steam_LOOP, base.mainBodyChunk.pos, room, 0.6f, 1f);
			}
			steamSoundLoop.volume = 0.6f;
			Vector2 normalized = (base.mainBodyChunk.pos - base.bodyChunks[1].pos).normalized;
			Vector2 vector = normalized * (ai.TargetPosition - base.mainBodyChunk.pos).magnitude;
			vector = Vector2.ClampMagnitude(vector, attackRange);
			vector *= 0.175f;
			IntVector2 tilePosition = room.GetTilePosition(Vector2.Lerp(base.bodyChunks[0].pos, ai.TargetPosition, 0.5f));
			steamConfines = room.TileRect(tilePosition).Grow(600f);
			steam.EmitSmoke(base.bodyChunks[0].pos + normalized * 8.5f, vector, steamConfines, 0.9f);
			DepleteSteam((int)(vector.magnitude * 0.15f));
			Vector2 vector2 = vector * 0.01f;
			base.bodyChunks[0].vel -= vector2;
			base.bodyChunks[1].vel -= vector2;
			base.bodyChunks[2].vel -= vector2;
			base.bodyChunks[3].vel -= vector2;
		}
	}

	public void HelpCallEffect()
	{
		if (CanMove || helpCallTimer.isFinished)
		{
			return;
		}
		helpCallTimer.Tick();
		if (helpCallTimer.normalized < 0.8f)
		{
			EmitSteam();
			if (helpSoundCounter.isFinished)
			{
				helpPingSE = room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Activate_Spin, base.mainBodyChunk, loop: false, 0f, 0f);
				helpPing2SE = room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Activate_Ping, base.mainBodyChunk, loop: false, 0f, 0f);
				shortSpitSteamSE = room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Spitting_Steam_LOOP, base.mainBodyChunk, loop: false, 0f, 1f);
				shortSteamSE = room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Idle_Steam, base.mainBodyChunk, loop: false, 0f, 1f);
				helpSoundCounter.Reset();
			}
		}
	}

	public void ReleaseSteam()
	{
		if (!releaseSteamTimer.isFinished && !CanMove)
		{
			releaseSteamTimer.Tick();
			EmitSteam();
			if (helpSoundCounter.isFinished)
			{
				shortSpitSteamSE = room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Spitting_Steam_LOOP, base.mainBodyChunk, loop: false, 0f, 1f);
				shortSteamSE = room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Idle_Steam, base.mainBodyChunk, loop: false, 0f, 1f);
				helpSoundCounter.Reset();
			}
		}
	}

	public void EmitSteam()
	{
		Vector2 vector = base.bodyChunks[0].pos - base.bodyChunks[1].pos;
		vector *= 0.125f;
		base.bodyChunks[0].vel -= vector * helpCallTimer.normalized;
		steamConfines = room.TileRect(room.GetTilePosition(base.bodyChunks[0].pos)).Grow(200f);
		steam.EmitSmoke(base.bodyChunks[0].pos, vector, steamConfines, 0.25f);
	}

	public Vector2 StuckSpasm()
	{
		if (stuckTimer.normalized <= 0.5f)
		{
			if (spasmSE == null)
			{
				spasmSE = room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Activate_Ping, base.mainBodyChunk, loop: false, 0f, Random.Range(0.1f, 0.15f));
				spasmSoundCounter.Reset();
			}
			return Custom.RNV() * ((0f - (stuckTimer.normalized * 2f - 1f)) * 10f);
		}
		return Vector2.zero;
	}

	public void Arch()
	{
		float num = 1f - Mathf.Clamp01(Mathf.Abs(base.bodyChunks[6].pos.y - base.bodyChunks[0].pos.y) / 60f);
		archAmount = Vector2.Distance(base.bodyChunks[0].pos, base.bodyChunks[6].pos);
		archAmount = Mathf.Lerp(1f, 0f, archAmount / (bodyChunkSize * 11f));
		Vector2 to = Vector2.Lerp(base.bodyChunks[0].pos, base.bodyChunks[6].pos, 0.5f);
		float num2 = (archAmount + 0.1f) * bodyChunkSize * 6f;
		to += Vector2.up * num2;
		float normalized = transitionTimer.normalized;
		if (bodySupport > 0)
		{
			base.bodyChunks[3].vel = MoveToPoint(base.bodyChunks[3].pos, to, 10f * normalized * num);
			base.bodyChunks[4].vel += (Custom.DirVec(base.bodyChunks[4].pos, base.bodyChunks[6].pos) * Vector2.right * 4f + Vector2.up) * (archAmount * normalized * num);
			base.bodyChunks[2].vel += (Custom.DirVec(base.bodyChunks[2].pos, base.bodyChunks[0].pos) * Vector2.right * 4f + Vector2.up) * (archAmount * normalized * num);
		}
	}

	public override void Die()
	{
		base.Die();
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (RippleViolenceCheck(source) && !(type == DamageType.Electric) && !(type == DamageType.Stab))
		{
			if (!base.dead)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Box_Worm_Hit, base.mainBodyChunk);
			}
			base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
		}
	}

	public void RecieveHelp()
	{
		if (!base.dead)
		{
			steamAvailable.Add(2);
		}
		helpRecievedTimer.Reset();
		helpPromiseCounter.Reset();
	}

	public bool NeedsHelp()
	{
		return ai.wantToCallForHelp;
	}

	public float HelpPriority()
	{
		return ai.helpPriority;
	}

	public bool CanLetGo()
	{
		if ((int)steamAvailable <= 100)
		{
			if (!helpRecievedTimer.isFinished)
			{
				return base.dead;
			}
			return true;
		}
		return false;
	}

	public bool CanAcceptHelp()
	{
		return helpRecievedTimer.isFinished;
	}

	public void AggreToHelp()
	{
		helpPromiseCounter.Reset();
	}

	public bool IsHelpComing()
	{
		return !helpPromiseCounter.isFinished;
	}

	public Vector2 HelpPoint()
	{
		return base.mainBodyChunk.pos + Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 3f;
	}

	public Vector2 HelpEntranceVec()
	{
		return Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * 50f;
	}

	public void DepleteSteam(int amount)
	{
		steamAvailable.Add(-amount);
	}

	public static Vector2 MoveToPoint(Vector2 from, Vector2 to, float maxMagnitude)
	{
		return Vector2.ClampMagnitude(to - from, maxMagnitude);
	}

	public bool IsBodyChunkOnGround(int index)
	{
		return groundGrabbers[index].isBodyChunkOnGround;
	}
}
