using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

public class Boomerang : Weapon
{
	public bool spinning;

	public float angleAdded;

	public int ticks;

	public bool startStepping;

	public Vector2 lastVelocity;

	public bool captureVelocity;

	public bool readyForReturn;

	public Creature lastThrownBy;

	public Color gradColor;

	public int catchTicks;

	public bool hitSomething;

	public bool skipForAFrame;

	public Vector2 ThrowPos;

	public Boomerang(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		collisionLayer = 2;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.4f;
		base.firstChunk.loudness = 9f;
		doNotTumbleAtLowSpeed = true;
		tailPos = base.firstChunk.pos;
		soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
		soundLoop.Volume = 0f;
		soundLoop.sound = WatcherEnums.WatcherSoundID.Boomerang_Flying_LOOP;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (soundLoop != null)
		{
			if (base.mode == Mode.Thrown)
			{
				soundLoop.Volume = Mathf.InverseLerp(0f, 5f, base.firstChunk.vel.magnitude);
			}
			else
			{
				soundLoop.Volume = 0f;
			}
			soundLoop.Update();
		}
		if (base.mode == Mode.Thrown)
		{
			if (captureVelocity)
			{
				lastVelocity = base.firstChunk.vel;
				captureVelocity = false;
			}
			ticks++;
			if (startStepping)
			{
				if (hitSomething)
				{
					base.firstChunk.vel *= 0.95f;
					base.firstChunk.vel += Vector2.up * 0.1f;
				}
				if (!CheckDegree())
				{
					base.firstChunk.vel = Vector3.Slerp(base.firstChunk.vel.normalized, Custom.DirVec(base.firstChunk.pos, thrownPos), (thrownBy is Scavenger && hitSomething) ? 0.075f : 0.15f).normalized * base.firstChunk.vel.magnitude;
				}
				else
				{
					SetValuesBack();
				}
			}
			else if (ticks == 12)
			{
				BeginReturnArc();
			}
			if (base.firstChunk.vel.magnitude < 1f || (startStepping && base.firstChunk.ContactPoint.y < 0) || ticks == 20)
			{
				SetValuesBack();
			}
			if (thrownBy is Scavenger && startStepping && Custom.DistLess(thrownBy.firstChunk.pos, base.firstChunk.pos, 150f))
			{
				SetValuesBack();
			}
		}
		else if (base.firstChunk.ContactPoint.y != 0)
		{
			rotationSpeed = (rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
		}
		if (base.mode == Mode.Free && lastThrownBy is Scavenger && catchTicks <= 15)
		{
			catchTicks++;
			if (Custom.DistLess(lastThrownBy.firstChunk.pos, base.firstChunk.pos, 95f))
			{
				catchTicks = 0;
				(lastThrownBy as Scavenger).PickUpAndPlaceInInventory(this, lethalityBypass: true);
			}
		}
	}

	public void SetValuesBack()
	{
		hitSomething = false;
		catchTicks = 0;
		startStepping = false;
		skipForAFrame = false;
		base.mode = Mode.Free;
		ticks = 0;
		base.forbiddenToPlayer = 0;
		base.gravity = 0.9f;
		if (spinning)
		{
			spinning = false;
		}
	}

	public bool CheckDegree()
	{
		float num = 15f;
		float num2 = Custom.VecToDeg(Custom.DirVec(base.firstChunk.pos, thrownPos));
		float num3 = Custom.VecToDeg(base.firstChunk.vel);
		if (num3 > num2 - num)
		{
			return num3 < num2 + num;
		}
		return false;
	}

	public void BeginReturnArc(bool hitSomething = false)
	{
		if (ticks < 4)
		{
			SetValuesBack();
		}
		else if (!startStepping)
		{
			base.gravity = 0.9f;
			base.firstChunk.vel = Custom.rotateVectorDeg(lastVelocity, 125f * (float)((!(lastVelocity.x > 0f)) ? 1 : (-1))) / (1.6f + 0.1f * (12f - (float)ticks));
			if (thrownBy is Scavenger && hitSomething)
			{
				this.hitSomething = hitSomething;
				base.gravity = 0.8f;
				base.firstChunk.vel += Vector2.up * 3f;
			}
			throwDir *= -1;
			readyForReturn = false;
			startStepping = true;
			skipForAFrame = true;
		}
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}
		if (thrownBy is Scavenger && thrownBy == result.obj)
		{
			SetValuesBack();
			return false;
		}
		if (result.obj.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !result.obj.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides)
		{
			return false;
		}
		if (thrownBy is Scavenger && (thrownBy as Scavenger).AI != null)
		{
			(thrownBy as Scavenger).AI.HitAnObjectWithWeapon(this, result.obj);
		}
		vibrate = 20;
		if (result.obj is Creature)
		{
			float stunBonus = 45f;
			if (ModManager.MMF && MMF.cfgIncreaseStuns.Value && (result.obj is Cicada || result.obj is LanternMouse || (ModManager.MSC && result.obj is Yeek)))
			{
				stunBonus = 90f;
			}
			if (ModManager.ChallengeModule && room.game.IsArenaSession && room.game.GetArenaGameSession.chMeta != null)
			{
				stunBonus = 90f;
			}
			(result.obj as Creature).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass, result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, 0.01f, stunBonus);
		}
		else if (result.chunk != null)
		{
			result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
		}
		else if (result.onAppendagePos != null)
		{
			(result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
		}
		room.PlaySound(WatcherEnums.WatcherSoundID.Boomerang_Collide_Creature, base.firstChunk);
		if (result.chunk != null)
		{
			BeginReturnArc(hitSomething: true);
			room.AddObject(new ExplosionSpikes(room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
		}
		SetRandomSpin();
		return true;
	}

	public override void HitWall()
	{
		if (skipForAFrame)
		{
			skipForAFrame = false;
		}
		if (readyForReturn)
		{
			BeginReturnArc();
		}
		else
		{
			SetValuesBack();
		}
		if (room.BeingViewed)
		{
			for (int i = 0; i < 7; i++)
			{
				room.AddObject(new Spark(base.firstChunk.pos + throwDir.ToVector2() * (base.firstChunk.rad - 1f), Custom.DegToVec(Random.value * 360f) * (10f * Random.value) + -throwDir.ToVector2() * 10f, new Color(1f, 1f, 1f), null, 2, 4));
			}
		}
		room.ScreenMovement(base.firstChunk.pos, throwDir.ToVector2() * 1.5f, 0f);
		room.PlaySound(WatcherEnums.WatcherSoundID.Boomerang_Collide_Terrain, base.firstChunk);
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		readyForReturn = true;
		captureVelocity = true;
		base.gravity = ((thrownBy is Scavenger) ? (-0.25f) : (-0.9f));
		ThrowPos = thrownPos;
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc / ((thrownBy is Scavenger) ? 0.9f : 1.1f), eu);
		lastThrownBy = thrownBy;
		base.firstChunk.vel = new Vector2(base.firstChunk.vel.x, base.firstChunk.vel.y - ((thrownBy is Scavenger) ? 20f : 7f));
		room?.PlaySound(WatcherEnums.WatcherSoundID.Boomerang_Thrown, base.firstChunk);
		SetRandomSpin();
	}

	public override void SetRandomSpin()
	{
		if (room != null)
		{
			rotationSpeed = ((Random.value < 0.5f) ? (-1f) : 1f) * Mathf.Lerp(50f, 150f, Random.value) * Mathf.Lerp(0.05f, 1f, room.gravity);
		}
		spinning = true;
	}

	public override void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Slugcat_Pick_Up_Rock, base.firstChunk);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3];
		sLeaser.sprites[0] = new FSprite("TemplarBoomerangGrad");
		sLeaser.sprites[1] = new FSprite("TemplarBoomerang");
		TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
		{
			new TriangleMesh.Triangle(0, 1, 2)
		};
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
		sLeaser.sprites[2] = triangleMesh;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		if (vibrate > 0)
		{
			vector += Custom.DegToVec(Random.value * 360f) * (2f * Random.value);
		}
		sLeaser.sprites[0].SetPosition(vector - camPos);
		sLeaser.sprites[1].SetPosition(vector - camPos);
		Vector3 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		sLeaser.sprites[0].rotation = Custom.VecToDeg(vector2);
		sLeaser.sprites[1].rotation = sLeaser.sprites[0].rotation;
		if (base.mode == Mode.Thrown)
		{
			sLeaser.sprites[2].isVisible = true;
			Vector2 vector3 = Vector2.Lerp(tailPos, base.firstChunk.lastPos, timeStacker);
			Vector2 vector4 = Custom.PerpendicularVector((vector - vector3).normalized);
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice(0, vector + vector4 * 3f - camPos);
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice(1, vector - vector4 * 3f - camPos);
			(sLeaser.sprites[2] as TriangleMesh).MoveVertice(2, vector3 - camPos);
		}
		else
		{
			sLeaser.sprites[2].isVisible = false;
		}
		Vector2 anchor = ((grabbedBy.Count > 0) ? new Vector2(0.3f, 0.3f) : new Vector2(0.5f, 0.5f));
		sLeaser.sprites[0].SetAnchor(anchor);
		sLeaser.sprites[1].SetAnchor(anchor);
		if (blink > 1 && Random.value < 0.5f)
		{
			sLeaser.sprites[0].color = base.blinkColor;
			sLeaser.sprites[1].color = base.blinkColor;
		}
		else
		{
			sLeaser.sprites[0].color = gradColor;
			sLeaser.sprites[1].color = color;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = palette.blackColor;
		gradColor = RainWorld.GoldRGB;
		sLeaser.sprites[0].color = gradColor;
		sLeaser.sprites[1].color = color;
		sLeaser.sprites[2].color = gradColor;
	}
}
