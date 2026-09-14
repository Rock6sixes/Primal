using RWCustom;
using UnityEngine;

public abstract class CosmeticInsect : CosmeticSprite
{
	public class Type : ExtEnum<Type>
	{
		public static Type StandardFly = new Type("StandardFly", register: true);

		public static Type FireFly = new Type("FireFly", register: true);

		public static Type TinyDragonFly = new Type("TinyDragonFly", register: true);

		public static Type RockFlea = new Type("RockFlea", register: true);

		public static Type GrassHopper = new Type("GrassHopper", register: true);

		public static Type RedSwarmer = new Type("RedSwarmer", register: true);

		public static Type Ant = new Type("Ant", register: true);

		public static Type Beetle = new Type("Beetle", register: true);

		public static Type WaterGlowworm = new Type("WaterGlowworm", register: true);

		public static Type Wasp = new Type("Wasp", register: true);

		public static Type Moth = new Type("Moth", register: true);

		public Type(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class CreatureAvoider
	{
		public CosmeticInsect owner;

		public Creature currentWorstCrit;

		public int updateRate;

		public int timer;

		public float visualRange;

		public float minSize;

		public Room room => owner.room;

		public float FleeSpeed
		{
			get
			{
				if (currentWorstCrit == null)
				{
					return 0f;
				}
				return Mathf.Pow(Mathf.InverseLerp(visualRange, visualRange * 0.1f, Vector2.Distance(owner.pos, currentWorstCrit.DangerPos)), Custom.LerpMap(currentWorstCrit.TotalMass, 0f, 20f, 3f, 0.2f)) * Custom.LerpMap(currentWorstCrit.TotalMass, 0f, 20f, 0.5f, 1f);
			}
		}

		public CreatureAvoider(CosmeticInsect owner, int updateRate, float visualRange, float minSize)
		{
			this.updateRate = updateRate;
			this.owner = owner;
			this.visualRange = visualRange;
			this.minSize = minSize;
			timer = Random.Range(0, updateRate);
		}

		public void Reset()
		{
			currentWorstCrit = null;
		}

		public void Update()
		{
			timer--;
			if (timer < 1)
			{
				timer = updateRate;
				Creature creature = null;
				if (room.abstractRoom.creatures.Count > 0)
				{
					creature = room.abstractRoom.creatures[Random.Range(0, room.abstractRoom.creatures.Count)].realizedCreature;
				}
				if (creature != null && creature.abstractPhysicalObject.rippleLayer != owner.rippleLayer && !creature.abstractPhysicalObject.rippleBothSides)
				{
					creature = null;
				}
				if (creature != null)
				{
					float num = ScaryScore(currentWorstCrit);
					float num2 = ScaryScore(creature);
					if (num2 > num)
					{
						currentWorstCrit = creature;
						num = num2;
					}
					if (num == 0f)
					{
						currentWorstCrit = null;
					}
				}
			}
			else if (currentWorstCrit != null && !Custom.DistLess(owner.pos, currentWorstCrit.DangerPos, visualRange))
			{
				currentWorstCrit = null;
			}
		}

		public float ScaryScore(Creature testCrit)
		{
			if (testCrit == null || testCrit.dead || testCrit.room != room || !Custom.DistLess(owner.pos, testCrit.DangerPos, visualRange) || testCrit.TotalMass < minSize || !room.VisualContact(owner.pos, testCrit.DangerPos))
			{
				return 0f;
			}
			return Mathf.Min(testCrit.TotalMass, 20f) * Mathf.Lerp(50f, Vector2.Distance(testCrit.mainBodyChunk.lastPos, testCrit.mainBodyChunk.pos), 0.5f) * Mathf.InverseLerp(visualRange, visualRange * 0.1f, Vector2.Distance(owner.pos, testCrit.DangerPos));
		}
	}

	public bool terrainCollision = true;

	public bool alive = true;

	public float inGround;

	public float lastInGround;

	public bool wantToBurrow;

	public Vector2? burrowPos;

	public bool lastWallCollide;

	public CreatureAvoider creatureAvoider;

	public int getAwayFromRainTime;

	public float emergeAfterRainTime;

	public Vector2 lastNonSolidPos;

	public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public bool submerged;

	public int rippleLayer;

	public InsectCoordinator.Swarm mySwarm;

	public Type type;

	public bool OutOfBounds
	{
		get
		{
			if (mySwarm != null)
			{
				return !Custom.DistLess(pos, mySwarm.placedObject.pos, mySwarm.insectGroupData.Rad);
			}
			return false;
		}
	}

	public CosmeticInsect(Room room, Vector2 pos, Type type)
	{
		this.type = type;
		base.pos = pos;
		lastPos = pos;
		getAwayFromRainTime = Random.Range(400, 1600);
		emergeAfterRainTime = Mathf.Lerp(0.1f, 0.6f, Custom.ClampedRandomVariation(0.5f, 0.5f, 0.1f));
		if (room.world.rainCycle.CycleStartUp < emergeAfterRainTime)
		{
			IntVector2 tilePosition = room.GetTilePosition(pos);
			while (tilePosition.y > 0 && !room.HasAnySolid(tilePosition))
			{
				tilePosition.y--;
			}
			lastInGround = 1f;
			inGround = 1f;
			burrowPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, 9f);
			if (room.terrain != null && room.terrain.SnapToTerrain(burrowPos.Value, 0f, out var normal, pos).y > burrowPos.Value.y && normal.y > 0f)
			{
				burrowPos = room.terrain.SnapToTerrain(burrowPos.Value, pos) - new Vector2(0f, 1f);
			}
			wantToBurrow = true;
		}
		lastNonSolidPos = pos;
	}

	public virtual void Reset(Vector2 resetPos)
	{
		burrowPos = null;
		pos = resetPos;
		lastPos = resetPos;
		lastNonSolidPos = resetPos;
		vel = Custom.RNV() * Random.value;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastInGround = inGround;
		submerged = room.PointSubmerged(pos);
		if (burrowPos.HasValue)
		{
			pos = burrowPos.Value;
			vel *= 0f;
			if (wantToBurrow)
			{
				inGround = Mathf.Min(1f, inGround + 1f / 30f);
				if (inGround == 1f)
				{
					Destroy();
				}
			}
			else
			{
				inGround = Mathf.Max(0f, inGround - 1f / 30f);
				if (inGround == 0f)
				{
					EmergeFromGround(burrowPos.Value);
					burrowPos = null;
				}
			}
		}
		else
		{
			if (terrainCollision)
			{
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, 1f, new IntVector2(0, 0), goThroughFloors: true);
				cd = SharedPhysics.VerticalCollision(room, cd);
				cd = SharedPhysics.SlopesVertically(room, cd);
				cd = SharedPhysics.HorizontalCollision(room, cd);
				pos = cd.pos;
				vel = cd.vel;
				if (cd.contactPoint.x != 0 || cd.contactPoint.y != 0)
				{
					WallCollision(cd.contactPoint, !lastWallCollide);
					lastWallCollide = true;
				}
				else
				{
					lastWallCollide = false;
				}
				if (!alive && cd.contactPoint.y < 0)
				{
					Destroy();
				}
				if (wantToBurrow && cd.contactPoint.y < 0)
				{
					burrowPos = cd.pos + new Vector2(0f, 0f - cd.rad - 2f);
				}
			}
			else if (wantToBurrow && room.GetTile(pos).Solid)
			{
				burrowPos = pos + new Vector2(0f, -2f);
			}
			if (wantToBurrow && pos.y < -100f)
			{
				Destroy();
			}
			if (room.GetTile(pos).Solid)
			{
				Reset(lastNonSolidPos);
			}
			else
			{
				lastNonSolidPos = pos;
			}
		}
		if (Random.value < 0.025f)
		{
			if (!wantToBurrow && room.world.rainCycle.TimeUntilRain < getAwayFromRainTime)
			{
				wantToBurrow = true;
			}
			else if (wantToBurrow && room.world.rainCycle.CycleStartUp > emergeAfterRainTime)
			{
				wantToBurrow = false;
			}
		}
		if (alive)
		{
			Act();
		}
		if (!room.BeingViewed)
		{
			Destroy();
		}
	}

	public virtual void Act()
	{
		if (creatureAvoider != null)
		{
			creatureAvoider.Update();
		}
	}

	public virtual void WallCollision(IntVector2 dir, bool first)
	{
	}

	public virtual void EmergeFromGround(Vector2 emergePos)
	{
	}
}
