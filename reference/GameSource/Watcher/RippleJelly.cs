using System;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class RippleJelly : PhysicalObject, IRippleSpawn
{
	public abstract class Behavior
	{
		public RippleJelly owner;

		public Behavior(RippleJelly owner)
		{
			this.owner = owner;
		}

		public abstract void Reset();

		public abstract void Update();
	}

	public class Wander : Behavior
	{
		public Vector2 desiredDirection;

		public Wander(RippleJelly owner)
			: base(owner)
		{
		}

		public override void Reset()
		{
		}

		public override void Update()
		{
			desiredDirection = (desiredDirection + UnityEngine.Random.insideUnitCircle * 0.04f).normalized;
			if (owner.TimeUntilNextMovement <= 0)
			{
				Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
				float value = UnityEngine.Random.value;
				value *= value;
				value = Mathf.Lerp(1f, 20f, value);
				owner.Squirt(normalized * value);
				owner.TimeUntilNextMovement = UnityEngine.Random.Range(20, 300);
			}
		}
	}

	public class FleeFromTarget : Behavior
	{
		public const float SafeDistance = 240f;

		public PhysicalObject target;

		public Behavior previousBehavior;

		public FleeFromTarget(RippleJelly owner, PhysicalObject target)
			: base(owner)
		{
			this.target = target;
			previousBehavior = owner.CurrentBehavior;
		}

		public override void Reset()
		{
		}

		public override void Update()
		{
			Vector2 pos = owner.FirstChunk().pos;
			Vector2 pos2 = target.FirstChunk().pos;
			if (Custom.Dist(pos, pos2) >= 240f)
			{
				owner.SetBehavior(previousBehavior);
			}
			else if (owner.TimeUntilNextMovement <= 0)
			{
				Vector2 normalized = (pos - pos2).normalized;
				normalized = (normalized + Vector2.down * 0.25f).normalized;
				float num = Mathf.Lerp(10f, 15f, UnityEngine.Random.value);
				owner.Squirt(normalized * num);
				owner.TimeUntilNextMovement = UnityEngine.Random.Range(50, 60);
			}
		}
	}

	public Behavior CurrentBehavior;

	public Vector2 AmountToSquirt;

	public int IdleTime;

	public int TimeUntilNextMovement;

	public float fade;

	public float lastFade;

	public bool startFadeOut;

	public int timeUntilFadeout = 3000;

	public static int MaxEatTime = 100;

	public bool Eaten;

	public int eatTime;

	public int lastEatTime;

	public AbstractPhysicalObject Consumer;

	public RippleJelly(AbstractPhysicalObject physicalObject)
		: base(physicalObject)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(physicalObject.ID.RandomSeed);
		float value = UnityEngine.Random.value;
		value = Mathf.Lerp(10f, 30f, Mathf.Pow(value, 2f));
		TimeUntilNextMovement = UnityEngine.Random.Range(0, 100);
		UnityEngine.Random.state = state;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), value, value * 0.01f);
		bodyChunkConnections = Array.Empty<BodyChunkConnection>();
		base.airFriction = 0.95f;
		base.gravity = 0f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = base.airFriction;
		waterRetardationImmunity = 1f;
		base.buoyancy = 0f;
		base.CollideWithTerrain = false;
		base.CollideWithObjects = false;
		canBeHitByWeapons = false;
		CurrentBehavior = new Wander(this);
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new RippleJellyGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		Vector2 newPos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].HardSetPosition(newPos);
		}
		base.graphicsModule?.Reset();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (UpdateFade() || UpdateConsumed())
		{
			return;
		}
		if (!(CurrentBehavior is FleeFromTarget))
		{
			foreach (Player item in room.PlayersInRoom)
			{
				Vector2 pos = FirstChunk().pos;
				Vector2 pos2 = item.FirstChunk().pos;
				if (Custom.Dist(pos, pos2) < 240f)
				{
					SetBehavior(new FleeFromTarget(this, item));
				}
			}
			foreach (VoidSpawn voidSpawn in room.voidSpawns)
			{
				if (!(voidSpawn.variant != VoidSpawn.SpawnType.RippleAmoeba))
				{
					Vector2 pos3 = FirstChunk().pos;
					Vector2 pos4 = voidSpawn.FirstChunk().pos;
					if (Custom.Dist(pos3, pos4) < 240f)
					{
						SetBehavior(new FleeFromTarget(this, voidSpawn));
					}
				}
			}
		}
		IdleTime++;
		TimeUntilNextMovement--;
		CurrentBehavior.Update();
		Vector2 vector = AmountToSquirt * 0.9f;
		Vector2 vector2 = AmountToSquirt - vector;
		base.bodyChunks[0].vel += vector2;
		AmountToSquirt = vector;
	}

	public bool UpdateFade()
	{
		lastFade = fade;
		if (room.PointSubmerged(FirstChunk().pos) || startFadeOut || (abstractPhysicalObject.rippleLayer == 1 && room.game.ActiveRippleLayer != 1))
		{
			float num = 0.05f;
			fade -= num;
			if (fade <= 0f)
			{
				Destroy();
				return true;
			}
		}
		else
		{
			fade = Mathf.Min(1f, fade + 0.025f);
		}
		if (timeUntilFadeout > 0)
		{
			timeUntilFadeout--;
			if (timeUntilFadeout == 0)
			{
				startFadeOut = true;
			}
		}
		return false;
	}

	public bool UpdateConsumed()
	{
		lastEatTime = eatTime;
		if (!Eaten)
		{
			return false;
		}
		if (Consumer == null || room.abstractRoom != Consumer.Room || Consumer.realizedObject == null)
		{
			return true;
		}
		PhysicalObject realizedObject = Consumer.realizedObject;
		eatTime++;
		float num = (float)eatTime / (float)MaxEatTime;
		base.firstChunk.pos = Vector2.Lerp(base.firstChunk.pos, realizedObject.FirstChunk().pos, num * num);
		if (eatTime >= MaxEatTime)
		{
			room.AddObject(new ShockWave(base.firstChunk.pos, 100f, UnityEngine.Random.value * 0.075f + 0.1f, UnityEngine.Random.Range(10, 20)));
			room.PlaySound(WatcherEnums.WatcherSoundID.Ripple_Creature_Swap_Dimensions, base.firstChunk.pos);
			Destroy();
		}
		return true;
	}

	public void SetTimeUntilFadeOut(int time)
	{
		timeUntilFadeout = time;
	}

	public void ChangeRippleLayer(int rippleLayer, bool showEffect)
	{
		ChangeRippleLayer(rippleLayer);
		if (showEffect)
		{
			room.AddObject(new ShockWave(base.firstChunk.pos, 50f, UnityEngine.Random.value * 0.075f + 0.05f, UnityEngine.Random.Range(10, 40)));
			room.PlaySound(WatcherEnums.WatcherSoundID.Ripple_Creature_Swap_Dimensions, base.firstChunk.pos);
		}
		else
		{
			fade = 0.01f;
			lastFade = 0.01f;
		}
	}

	public void Squirt(Vector2 velocity)
	{
		if (!(velocity.magnitude < 0.01f))
		{
			AmountToSquirt += velocity;
			IdleTime = UnityEngine.Random.Range(-50, 0);
		}
	}

	public void SetBehavior(Behavior behavior)
	{
		behavior.Reset();
		CurrentBehavior = behavior;
	}
}
