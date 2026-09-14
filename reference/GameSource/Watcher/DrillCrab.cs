using System;
using System.Collections.Generic;
using RWCustom;
using Smoke;
using UnityEngine;

namespace Watcher;

public class DrillCrab : AirBreatherCreature, PhysicalObject.IHaveAppendages
{
	public class SmallLeg : Leg
	{
		public static float[] segmentLengths = new float[5] { 45f, 45f, 50f, 58f, 35f };

		public SmallLeg(DrillCrab crab, float side, float lengthFac, int index)
			: base(crab, side, lengthFac, index, 185f, segmentLengths)
		{
		}

		public override void DoInverseKinematics(float flip, float extended)
		{
			segments[2].pos = Leg.InverseKinematic(anchor, base.Tip.pos, lengthFac * 97f * (extended * 0.8f + 0.2f), lengthFac * 92f * (extended * 0.5f + 0.5f), 0f - flip);
			segments[1].pos = Leg.InverseKinematic(anchor, segments[2].pos, lengthFac * 52f * (extended * 0.5f + 0.5f), segments[2].prevDist, 0f - flip);
			segments[0].pos = Leg.InverseKinematic(anchor, segments[1].pos, segments[0].prevDist, segments[1].prevDist, 0f - flip);
			segments[3].pos = Leg.InverseKinematic(segments[2].pos, base.Tip.pos, segments[3].prevDist, segments[4].prevDist, flip);
		}

		public override void LandOnGround()
		{
			base.LandOnGround();
			crab.room.PlaySound(WatcherEnums.WatcherSoundID.Drill_Crab_Step, base.Tip.pos, crab.abstractCreature);
		}
	}

	public class TowerLeg : Leg
	{
		public static float[] segmentLengths = new float[6] { 55f, 120f, 120f, 120f, 70f, 35f };

		public TowerLeg(DrillCrab crab, float side, float lengthFac, int index)
			: base(crab, side, lengthFac, index, 450f, segmentLengths)
		{
			accel = 0.05f;
			maxSpeed = 50f;
			separation = 9f;
		}

		public override void DoInverseKinematics(float flip, float extended)
		{
			segments[2].pos = Leg.InverseKinematic(anchor, base.Tip.pos, (100f + 150f * extended) * lengthFac, (125f + 70f * Mathf.Pow(extended, 0.2f) + 25f * extended) * lengthFac, 0f - flip);
			segments[1].pos = Leg.InverseKinematic(anchor, segments[2].pos, (100f + 50f * extended) * lengthFac, segments[2].prevDist, 0f - flip);
			segments[0].pos = Leg.InverseKinematic(anchor, segments[1].pos, segments[0].prevDist, segments[1].prevDist, 0f - flip);
			segments[3].pos = Leg.InverseKinematic(segments[2].pos, base.Tip.pos, segments[3].prevDist, (85f + 15f * extended) * lengthFac, 0f - flip);
			segments[4].pos = Leg.InverseKinematic(segments[3].pos, base.Tip.pos, segments[4].prevDist, segments[5].prevDist, flip);
		}

		public override void LandOnGround()
		{
			base.LandOnGround();
			float num = float.PositiveInfinity;
			foreach (AbstractCreature player2 in crab.room.game.Players)
			{
				if (player2.realizedCreature is Player player && player.room == crab.room)
				{
					num = Mathf.Min(num, Vector2.Distance(base.Tip.pos, player.mainBodyChunk.pos));
				}
			}
			float num2 = Mathf.InverseLerp(300f, 150f, num);
			if (num2 > 0f)
			{
				crab.room.PlaySound(WatcherEnums.WatcherSoundID.Tower_Crab_Step, base.Tip.pos, num2, 1f, crab.abstractCreature);
			}
		}
	}

	public abstract class Leg
	{
		public class Segment
		{
			public Vector2 pos;

			public Vector2 lastPos;

			public Vector2 vel;

			public float prevDist;

			public Segment(float prevDist)
			{
				this.prevDist = prevDist;
			}
		}

		public class Mode : ExtEnum<Mode>
		{
			public static Mode Retracting = new Mode("Retracting", register: true);

			public static Mode Seeking = new Mode("Seeking", register: true);

			public static Mode Scanning = new Mode("Scanning", register: true);

			public Mode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public DrillCrab crab;

		public Segment[] segments;

		public Vector2 targetPos;

		public Mode mode;

		public Vector2 anchor;

		public Vector2 lastAnchor;

		public float side;

		public Vector2 scanFrom;

		public int scanTime;

		public int segmentsRetracted;

		public int timeSpentRetracting;

		public float flatten;

		public int index;

		public float lengthFac;

		public float maxLength;

		public float drag = 0.95f;

		public float maxSpeed = 25f;

		public float accel = 0.2f;

		public float separation = 18f;

		public SharedPhysics.TerrainCollisionData cd = new SharedPhysics.TerrainCollisionData(default(Vector2), default(Vector2), default(Vector2), 1f, default(IntVector2), goThroughFloors: true);

		public Segment Tip => segments[segments.Length - 1];

		public bool Supporting { get; set; }

		public bool Limp
		{
			get
			{
				if (!(mode == Mode.Retracting))
				{
					return Dead;
				}
				return true;
			}
		}

		public bool WantsToStep
		{
			get
			{
				if (!(Mathf.Abs(Mathf.DeltaAngle(IdealAngle, CurrentAngle)) > 15f))
				{
					return !Supporting;
				}
				return true;
			}
		}

		public float IdealAngle => (0f - separation) * side - 15f * crab.targetFlip;

		public float CurrentAngle => 0f - Vector2.SignedAngle(Vector2.down, Tip.pos - crab.bodyChunks[0].pos);

		public float Health
		{
			get
			{
				return crab.State.legHealths[index];
			}
			set
			{
				crab.State.legHealths[index] = value;
			}
		}

		public bool Dead => Health <= 0f;

		public Leg(DrillCrab crab, float side, float lengthFac, int index, float baseLength, float[] segmentLengths)
		{
			this.crab = crab;
			this.side = side;
			this.index = index;
			this.lengthFac = lengthFac;
			maxLength = baseLength * lengthFac;
			segments = new Segment[segmentLengths.Length];
			for (int i = 0; i < segmentLengths.Length; i++)
			{
				segments[i] = new Segment(segmentLengths[i] * lengthFac);
			}
			ResetPhysics();
		}

		public void ResetPhysics()
		{
			Segment[] array = segments;
			foreach (Segment obj in array)
			{
				obj.pos = crab.bodyChunks[0].pos;
				obj.lastPos = crab.bodyChunks[0].pos;
				obj.vel = default(Vector2);
			}
			anchor = crab.bodyChunks[0].pos;
			lastAnchor = crab.bodyChunks[0].pos;
			Supporting = false;
			StartStep();
		}

		public void Update()
		{
			Vector2 vector = Custom.DirVec(crab.bodyChunks[1].pos, crab.bodyChunks[0].pos);
			lastAnchor = anchor;
			anchor = crab.bodyChunks[0].pos + vector * 10f + Custom.PerpendicularVector(crab.bodyChunks[1].pos, crab.bodyChunks[0].pos) * ((Mathf.Pow(Mathf.Abs(side), 0.5f) * 8f + 4f) * Mathf.Sign(side) + crab.flip * 5f);
			Segment[] array = segments;
			foreach (Segment obj in array)
			{
				obj.lastPos = obj.pos;
			}
			Vector2? vector2 = null;
			if (Dead)
			{
				mode = Mode.Retracting;
			}
			if (Dead && (!crab.Tower || segmentsRetracted > 1))
			{
				if (!crab.Tower)
				{
					segmentsRetracted = 0;
				}
				timeSpentRetracting = 0;
			}
			else if (mode == Mode.Seeking)
			{
				float num = Vector2.Distance(anchor, targetPos);
				if (num <= maxLength)
				{
					vector2 = targetPos;
				}
				else
				{
					float t = Mathf.InverseLerp(0f, 80f, num - maxLength);
					vector2 = (targetPos - anchor) / num * Mathf.SmoothStep(maxLength, 80f, t) + anchor;
				}
				if (Supporting)
				{
					flatten = Mathf.Max(flatten - 1f / 30f, 0f);
				}
			}
			else if (mode == Mode.Retracting)
			{
				if (segmentsRetracted < segments.Length && crab.Consious)
				{
					Segment segment = segments[segmentsRetracted];
					segment.vel += (anchor - segment.pos).normalized * 5f;
					segment.vel = Vector2.Lerp(segment.vel, crab.bodyChunks[0].vel, 0.1f);
					segment.pos = Custom.RotateAroundVector(segment.pos, anchor, Vector2.SignedAngle(vector, segment.pos - anchor) * 0.25f);
					if (Custom.DistLess(segment.pos, anchor, crab.bodyChunks[0].rad))
					{
						segmentsRetracted++;
					}
				}
				timeSpentRetracting++;
			}
			else if (mode == Mode.Scanning)
			{
				if (scanTime++ > 40)
				{
					StartStep();
				}
				vector2 = anchor + Custom.RotateAroundOrigo(new Vector2(0f, -100f), IdealAngle);
				float degAng = IdealAngle + UnityEngine.Random.Range(-10f, 10f);
				Vector2? vector3 = SharedPhysics.ExactTerrainRayTracePos(crab.room, scanFrom, scanFrom + Custom.RotateAroundOrigo(Vector2.down * maxLength, degAng));
				if (vector3.HasValue)
				{
					SetTarget(vector3.Value);
				}
			}
			if (Limp)
			{
				AnimatePhysics();
			}
			else if (vector2.HasValue)
			{
				AnimateInverseKinematics(vector2.Value);
			}
		}

		public void AnimateInverseKinematics(Vector2 aimFor)
		{
			if (Vector2.Distance(Tip.pos, anchor) > maxLength)
			{
				Tip.pos = (Tip.pos - anchor).normalized * maxLength + anchor;
				Supporting = false;
			}
			if (Supporting)
			{
				Tip.vel = Vector2.zero;
			}
			else
			{
				Vector2 normalized = (Tip.pos - crab.bodyChunks[0].pos).normalized;
				Vector2 vector = Custom.PerpendicularVector(normalized);
				float num = Vector2.Distance(Tip.pos, crab.bodyChunks[0].pos);
				float num2 = Vector2.Distance(aimFor, crab.bodyChunks[0].pos);
				float num3 = Vector2.SignedAngle(normalized, aimFor - crab.bodyChunks[0].pos);
				float num4 = Custom.LerpMap(Mathf.Abs(num3), 45f, 10f, 100f, num2);
				Vector2 b = normalized * (Mathf.Clamp((num4 - num) / 20f, -1f, 1f) * maxSpeed);
				b += vector * (Mathf.Clamp(num3 / 45f, -1f, 1f) * maxSpeed);
				Tip.vel = Vector2.Lerp(Tip.vel, b, accel);
				Tip.pos += Tip.vel;
				Tip.vel *= drag;
				if ((crab.room.GetTile(Tip.pos).Solid || (crab.room.terrain != null && crab.room.terrain.Contains(Tip.pos))) && num4 == num2)
				{
					LandOnGround();
				}
			}
			float num5 = Custom.SCurve(flatten, 2f);
			float flip = Mathf.Sin((side * 0.4f + 0.75f * crab.flip) * (float)Math.PI / 2f) * (1f - num5 * 0.8f);
			float extended = Vector2.Distance(Tip.pos, anchor) / maxLength;
			if (crab.graphicsModule != null && crab.graphicsModule.culled)
			{
				Vector2 a = anchor;
				Vector2 pos = Tip.pos;
				for (int i = 0; i < segments.Length - 1; i++)
				{
					float t = (float)i / (float)(segments.Length - 1);
					segments[i].pos = Vector2.Lerp(a, pos, t);
				}
			}
			else
			{
				DoInverseKinematics(flip, extended);
			}
			for (int j = 0; j < segments.Length - 1; j++)
			{
				segments[j].vel = segments[j].pos - segments[j].lastPos;
			}
		}

		public virtual void LandOnGround()
		{
			Tip.vel = Vector2.zero;
			Supporting = true;
		}

		public abstract void DoInverseKinematics(float flip, float extended);

		public void AnimatePhysics()
		{
			Supporting = false;
			Vector2 pos = anchor;
			for (int i = segmentsRetracted; i < segments.Length; i++)
			{
				Vector2 vector = segments[i].pos - pos;
				float magnitude = vector.magnitude;
				vector /= magnitude;
				float prevDist = segments[i].prevDist;
				float num = ((magnitude > prevDist) ? (magnitude - prevDist) : ((!(magnitude < prevDist / 2f) || (!(mode != Mode.Retracting) && i == segmentsRetracted)) ? 0f : (magnitude - prevDist / 2f)));
				Vector2 vector2 = vector * (num * 0.25f);
				segments[i].pos -= vector2;
				segments[i].vel -= vector2;
				if (i > segmentsRetracted)
				{
					segments[i - 1].pos += vector2;
					segments[i - 1].vel += vector2;
				}
				pos = segments[i].pos;
			}
			pos = anchor;
			for (int j = 0; j < segments.Length; j++)
			{
				Segment segment = segments[j];
				if (j < segmentsRetracted)
				{
					segment.pos = anchor;
					segment.vel = crab.bodyChunks[0].vel;
				}
				else
				{
					segment.vel.y -= crab.room.gravity;
					segment.pos += segment.vel;
					if (timeSpentRetracting < 20 || !crab.Consious)
					{
						cd.lastPos = segment.lastPos;
						cd.pos = segment.pos;
						cd.vel = segment.vel;
						cd.contactPoint = default(IntVector2);
						cd = SharedPhysics.VerticalCollision(crab.room, cd);
						cd = SharedPhysics.SlopesVertically(crab.room, cd);
						cd = SharedPhysics.HorizontalCollision(crab.room, cd);
						segment.pos = cd.pos;
						segment.vel = cd.vel;
					}
					segment.pos = Vector2.ClampMagnitude(segment.pos - pos, segment.prevDist * 1.5f) + pos;
				}
				segment.vel *= 0.975f;
				pos = segment.pos;
			}
		}

		public void Retract()
		{
			if (!Dead)
			{
				SwitchMode(Mode.Retracting);
				Tip.vel = GetNormal(Tip.pos) * 3f;
				targetPos = default(Vector2);
			}
		}

		public void StartStep()
		{
			if (!Dead)
			{
				SwitchMode(Mode.Scanning);
				scanFrom = crab.nextStepPos;
			}
		}

		public void SetTarget(Vector2 pos)
		{
			if (!Dead)
			{
				SwitchMode(Mode.Seeking);
				targetPos = pos;
			}
		}

		public void ForceAttach(Vector2 pos)
		{
			SetTarget(pos);
			Tip.pos = pos;
			Supporting = true;
		}

		public void SwitchMode(Mode newMode)
		{
			if (newMode != mode && mode == Mode.Retracting)
			{
				flatten = 1f;
			}
			mode = newMode;
			Supporting = false;
			segmentsRetracted = 0;
			scanTime = 0;
			scanFrom = default(Vector2);
		}

		public void ApplyForce(Appendage.Pos pos, Vector2 momentum)
		{
			int num = pos.prevSegment - 1;
			ApplyForce(num, momentum * (1f - pos.distanceToNext));
			ApplyForce(num + 1, momentum * pos.distanceToNext);
		}

		public void ApplyForce(int segment, Vector2 momentum)
		{
			if (segment == -1)
			{
				crab.bodyChunks[0].vel += momentum / crab.bodyChunks[0].mass;
			}
			else
			{
				segments[segment].vel += momentum * 2f;
			}
		}

		public static Vector2 InverseKinematic(Vector2 shoulderPos, Vector2 handPos, float upperArmLength, float lowerArmLength, float flip)
		{
			float num = Vector2.Distance(shoulderPos, handPos);
			float f = Mathf.Acos(Mathf.Clamp((num * num + upperArmLength * upperArmLength - lowerArmLength * lowerArmLength) / (2f * num * upperArmLength), -1f, 1f));
			Vector2 normalized = (handPos - shoulderPos).normalized;
			float num2 = Mathf.Cos(f);
			float num3 = Mathf.Sin(f) * flip;
			return new Vector2(normalized.x * num2 + normalized.y * num3, normalized.y * num2 - normalized.x * num3) * upperArmLength + shoulderPos;
		}

		public Vector2 GetNormal(Vector2 pos)
		{
			for (int i = 0; i < 4; i++)
			{
				if (!crab.room.GetTile(pos + Custom.fourDirections[i].ToVector2() * 5f).Solid && crab.room.GetTile(pos + Custom.fourDirections[i].ToVector2() * 5f).Solid)
				{
					return Custom.fourDirections[i].ToVector2();
				}
			}
			return Vector2.zero;
		}
	}

	public class Eye
	{
		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float side;

		public Eye(float side)
		{
			this.side = side;
		}
	}

	public class IndividualVariations
	{
		public int extraTeeth;

		public float legThickness = 0.5f;

		public float legUniformity = 0.5f;

		public float legLength = 0.5f;

		public float bodySize = 0.5f;

		public float drillSize = 0.5f;

		public float eyestalkLength = 0.5f;

		public float eyestalkAsymmetry = 0.5f;

		public float eyeLightness = 0.5f;

		public IndividualVariations()
		{
		}

		public IndividualVariations(AbstractCreature crab)
			: this()
		{
			int randomSeed = crab.ID.RandomSeed;
			AbstractCreature.Personality personality = crab.personality;
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(randomSeed);
			float value = UnityEngine.Random.value;
			if (value < 0.08f)
			{
				extraTeeth = 2;
			}
			else if (value < 0.3f)
			{
				extraTeeth = 1;
			}
			if (UnityEngine.Random.value > personality.aggression)
			{
				extraTeeth = -extraTeeth;
			}
			legThickness = Mathf.Lerp(personality.dominance, UnityEngine.Random.value, UnityEngine.Random.value);
			legUniformity = Mathf.Lerp(1f - personality.energy, UnityEngine.Random.value, UnityEngine.Random.value);
			legLength = Mathf.Lerp(personality.dominance, UnityEngine.Random.value, UnityEngine.Random.value);
			bodySize = Mathf.Lerp(Mathf.Max(1f - personality.energy, personality.dominance), UnityEngine.Random.value, UnityEngine.Random.value);
			drillSize = Mathf.Lerp(Mathf.Max(personality.aggression, personality.dominance), UnityEngine.Random.value, UnityEngine.Random.value);
			eyestalkLength = UnityEngine.Random.value;
			if (UnityEngine.Random.value < 0.25f)
			{
				eyestalkAsymmetry = 0.5f + 0.5f * Mathf.Lerp(personality.nervous, UnityEngine.Random.value, UnityEngine.Random.value) * ((UnityEngine.Random.value < 0.5f) ? 1f : (-1f));
				eyestalkAsymmetry = Custom.PushFromHalf(eyestalkAsymmetry, 0.5f);
			}
			eyeLightness = UnityEngine.Random.value;
			if (crab.creatureTemplate.type == WatcherEnums.CreatureTemplateType.TowerCrab)
			{
				bodySize += 1f;
				drillSize += 1f;
				legThickness += 1f;
				eyestalkLength *= 2f;
			}
			UnityEngine.Random.state = state;
		}
	}

	public DrillCrabAI ai;

	public Leg[] legs;

	public Eye[] eyes;

	public IndividualVariations iVars;

	public int hideTimer;

	public int spinOutTimer;

	public float eyesHidden;

	public float lastEyesHidden;

	public float spinUp;

	public int sparkDelay;

	public int terrainSoundDelay;

	public int objectHitDelay;

	public ChunkDynamicSoundLoop soundLoop;

	public ChunkDynamicSoundLoop impactSoundLoop;

	public float pitchDown;

	public float timeToNextChatterModeSwitch;

	public bool chatterAudible;

	public float chatterFade;

	public bool readyForStartupSound;

	public ExhaustSmoke smoke;

	public Color fireColor = new Color(1f, 0.6f, 0.2f);

	public Color smokeColor = new Color(0.2f, 0.2f, 0.2f);

	public const int spinDuration = 200;

	public const int spinStartupTime = 60;

	public const int shortHideDuration = 120;

	public const int hideDuration = 400;

	public const int peekDuration = 80;

	public int stepCounter;

	public int legToMove;

	public Vector2 nextStepPos;

	public float walkAnim;

	public float targetFlip;

	public bool squeeze;

	public float squeezeFactor;

	public float flip;

	public float lastFlip;

	public bool stunLimp;

	public float speed = 1f;

	public Vector2 moveTarget;

	public int moveTargetAge;

	public int maxMoveTargetAge;

	public int noPathCounter;

	public float idealHeight;

	public static Dictionary<int, List<IntVector2>> nearbyTilesCache = new Dictionary<int, List<IntVector2>>();

	public bool Tower => base.Template.type == WatcherEnums.CreatureTemplateType.TowerCrab;

	public float StuckAmount => ai.stuckTracker?.Utility() ?? 0f;

	public float DistanceToPlayer
	{
		get
		{
			float num = float.PositiveInfinity;
			if (room != null)
			{
				foreach (AbstractCreature player2 in room.game.Players)
				{
					if (player2?.realizedCreature is Player player && player.room == room)
					{
						num = Mathf.Min(num, Vector2.Distance(player.mainBodyChunk.pos, base.mainBodyChunk.pos));
					}
				}
			}
			return num;
		}
	}

	public int LegsSupporting
	{
		get
		{
			int num = 0;
			Leg[] array = legs;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Supporting)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool SpinningOut
	{
		get
		{
			if (spinOutTimer > 0)
			{
				return spinOutTimer < 200;
			}
			return false;
		}
	}

	public bool Hiding => hideTimer > 0;

	public bool Kneeling => ai.kneelTimer > 0;

	public new DrillCrabState State => base.State as DrillCrabState;

	public float LegLengthFactor => Mathf.Lerp(Tower ? 0.6f : 0.8f, 1.2f, iVars.legLength);

	public DrillCrab(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		iVars = new IndividualVariations(abstractCreature);
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, Vector2.zero, 20f * Mathf.LerpUnclamped(0.9f, 1.1f, iVars.bodySize), 1f);
		base.bodyChunks[1] = new BodyChunk(this, 1, Vector2.zero, 16f * Mathf.LerpUnclamped(0.9f, 1.1f, iVars.drillSize), 0.5f);
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 27f, BodyChunkConnection.Type.Normal, 1f, -1f);
		base.bodyChunks[0].rotationChunk = base.bodyChunks[1];
		base.bodyChunks[1].rotationChunk = base.bodyChunks[0];
		appendages = new List<Appendage>();
		legs = new Leg[6];
		eyes = new Eye[2];
		for (int i = 0; i < legs.Length; i++)
		{
			float num = (float)i / (float)(legs.Length - 1) * 2f - 1f;
			float num2 = 1.2f - Mathf.Abs(num) * 0.2f;
			num2 *= LegLengthFactor;
			if (Tower)
			{
				legs[i] = new TowerLeg(this, num, num2, i);
			}
			else
			{
				legs[i] = new SmallLeg(this, num, num2, i);
			}
			appendages.Add(new Appendage(this, i, legs[i].segments.Length + 1));
		}
		if (Tower)
		{
			idealHeight = 400f;
		}
		else
		{
			idealHeight = 150f;
		}
		idealHeight *= LegLengthFactor;
		maxMoveTargetAge = (Tower ? 20 : 5);
		moveTargetAge = UnityEngine.Random.Range(0, maxMoveTargetAge);
		eyes[0] = new Eye(1f);
		eyes[1] = new Eye(-1f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.1f;
		collisionLayer = ((!State.BehindPlayLayer) ? 1 : 2);
		base.waterFriction = 0.96f;
		base.buoyancy = 0f;
		base.GoThroughFloors = true;
		base.CollideWithSlopes = false;
		canBeHitByWeapons = !State.BehindPlayLayer;
		soundLoop = new ChunkDynamicSoundLoop(base.bodyChunks[0])
		{
			Volume = 0f
		};
		impactSoundLoop = new ChunkDynamicSoundLoop(base.bodyChunks[0])
		{
			Volume = 0f
		};
		SwapChatterMode();
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			GraphicsModule graphicsModule2 = (base.graphicsModule = new DrillCrabGraphics(this));
		}
		base.graphicsModule.Reset();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		squeeze = false;
		if (LegsSupporting < 2)
		{
			nextStepPos = base.bodyChunks[0].pos;
		}
		else
		{
			FollowPath();
		}
		timeToNextChatterModeSwitch -= 1f;
		if (timeToNextChatterModeSwitch <= 0f)
		{
			SwapChatterMode();
		}
		if (chatterAudible && spinUp <= 0f && ai.behavior != DrillCrabAI.Behavior.Flee && base.Consious)
		{
			chatterFade = Mathf.Lerp(chatterFade, 1f, 0.05f);
		}
		else
		{
			chatterFade = Mathf.Lerp(chatterFade, 0f, 0.025f);
		}
		if (base.dead)
		{
			chatterFade = Mathf.Max(chatterFade - 0.1f, 0f);
		}
		soundLoop.sound = SoundID.None;
		if (spinUp > 0f)
		{
			soundLoop.sound = WatcherEnums.WatcherSoundID.Drill_Crab_Drill_LOOP;
			soundLoop.Volume = Mathf.Pow(spinUp, 0.5f);
			soundLoop.Pitch = Mathf.Lerp(0.25f, 1f, spinUp * (1f - pitchDown * 0.5f)) * Mathf.Lerp(0.9f, 1.1f, iVars.drillSize);
			if (readyForStartupSound && spinUp > 0.25f)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Drill_Crab_Start_Up, base.mainBodyChunk);
				readyForStartupSound = false;
			}
		}
		else
		{
			float num = chatterFade;
			if (Tower)
			{
				num *= Mathf.InverseLerp(1000f, 750f, DistanceToPlayer) * 0.7f;
			}
			readyForStartupSound = true;
			soundLoop.Volume = num;
			soundLoop.Pitch = (Tower ? Mathf.Lerp(0.7f, 1f, base.abstractCreature.personality.energy) : Mathf.Lerp(1f, 0.8f, iVars.bodySize));
			if (soundLoop.Volume > 0f)
			{
				soundLoop.sound = (Hiding ? WatcherEnums.WatcherSoundID.Drill_Crab_CrabChatterLow_LOOP : WatcherEnums.WatcherSoundID.Drill_Crab_CrabChatter_LOOP);
			}
		}
		if (room == null)
		{
			return;
		}
		soundLoop.Update();
		lastFlip = flip;
		lastEyesHidden = eyesHidden;
		if (base.Consious)
		{
			float target = 0f;
			if (Hiding && hideTimer < 80)
			{
				target = 0.2f;
			}
			else if (Hiding)
			{
				target = 1f;
			}
			eyesHidden = Mathf.MoveTowards(eyesHidden, target, 1f / 30f);
		}
		spinUp = Mathf.MoveTowards(spinUp, SpinningOut ? 1f : 0f, 1f / 60f);
		if (spinOutTimer > 0)
		{
			spinOutTimer--;
			if (spinOutTimer == 0)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Drill_Crab_Turn_Off, base.mainBodyChunk);
			}
		}
		if (sparkDelay > 0)
		{
			sparkDelay--;
		}
		if (terrainSoundDelay > 0)
		{
			terrainSoundDelay--;
		}
		if (objectHitDelay > 0)
		{
			objectHitDelay--;
		}
		pitchDown = Custom.LerpAndTick(pitchDown, 0f, 0.1f, 0.02f);
		if (hideTimer > 0)
		{
			hideTimer--;
			if (hideTimer == 0)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Drill_Crab_Exit_Shell, base.mainBodyChunk);
			}
		}
		bool flag = false;
		if (spinUp > 0f)
		{
			flag = BounceOffTerrain();
			DoSmoke();
			Vector2 vector = Custom.RNV() * (UnityEngine.Random.value * 0.5f * spinUp);
			base.bodyChunks[0].vel += vector / base.bodyChunks[0].mass;
			base.bodyChunks[1].vel -= vector / base.bodyChunks[0].mass;
		}
		impactSoundLoop.sound = WatcherEnums.WatcherSoundID.Drill_Crab_Drill_Impact_LOOP;
		if (flag)
		{
			impactSoundLoop.Volume = Mathf.Max(impactSoundLoop.Volume, UnityEngine.Random.Range(0.5f, 1f) * Mathf.Sqrt(spinUp));
		}
		else
		{
			impactSoundLoop.Volume = Custom.LerpAndTick(impactSoundLoop.Volume, 0f, 0.08f, 0.02f);
		}
		impactSoundLoop.Update();
		squeezeFactor = Mathf.MoveTowards(squeezeFactor, squeeze ? 1f : 0f, 1f / 30f);
		BodyChunk[] array = base.bodyChunks;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].terrainSqueeze = 1f - 0.4f * squeezeFactor;
		}
		Leg[] array2 = legs;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Update();
		}
		if (base.Consious)
		{
			ai.Update();
		}
		if (!Hiding && base.Consious)
		{
			Act();
		}
		else
		{
			if (!base.Consious)
			{
				hideTimer = Mathf.Max(hideTimer, 90);
			}
			array2 = legs;
			foreach (Leg leg in array2)
			{
				if (leg.mode != Leg.Mode.Retracting)
				{
					leg.Retract();
				}
			}
		}
		float b = Mathf.Lerp(0.9f, 0f, (float)LegsSupporting / (Tower ? 1f : 2f)) + Custom.LerpMap(noPathCounter, 2f, 5f, 0f, 0.1f);
		base.gravity = Mathf.Max(base.gravity - 1f / 30f, b);
		flip = Custom.LerpAndTick(flip, targetFlip, 0.035f, 0.015f);
		base.airFriction = Mathf.Lerp(1f, 0.925f, (float)LegsSupporting / 2f);
		Eye[] array3 = eyes;
		foreach (Eye eye in array3)
		{
			eye.lastPos = eye.pos;
			Vector2 vector2 = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
			if (base.Consious)
			{
				Vector2 vector3 = Vector3.Slerp(vector2, Vector2.up, 0.65f);
				Vector2 vector4 = Custom.PerpendicularVector(vector3);
				float num2 = Mathf.Lerp(50f, 90f, iVars.eyestalkLength) + 5f * (iVars.eyestalkAsymmetry * 2f - 1f) * eye.side;
				Vector2 vector5 = base.bodyChunks[0].pos + vector3 * (num2 * ((Tower && !Hiding) ? (-0.5f) : 1f)) - vector4 * (3f * eye.side + flip * 6f);
				if (Tower && !Hiding)
				{
					vector5.x += 50f * flip;
				}
				if (ai.creatureLooker.lookCreature != null)
				{
					Tracker.CreatureRepresentation lookCreature = ai.creatureLooker.lookCreature;
					Vector2 vector6 = ((!lookCreature.VisualContact) ? room.MiddleOfTile(lookCreature.BestGuessForPosition()) : lookCreature.representedCreature.realizedCreature.DangerPos);
					vector5 += Vector2.ClampMagnitude((vector6 - vector5) / 20f, 20f) * new Vector2(0.65f, 1f);
				}
				vector5 = SharedPhysics.ExactTerrainRayTracePos(room, base.bodyChunks[0].pos, vector5) ?? vector5;
				eye.vel += (vector5 - eye.pos) * 0.075f;
				eye.pos += (vector5 - eye.pos) * 0.1f;
				eye.vel *= 0.935f;
				eye.pos += eye.vel;
				continue;
			}
			Vector2 vector7 = base.bodyChunks[0].pos + vector2 * 25f - Custom.PerpendicularVector(vector2) * (eye.side * 5f);
			float num3 = Vector2.Distance(vector7, eye.pos);
			float num4 = 50f;
			if (num3 > num4)
			{
				Vector2 vector8 = Custom.DirVec(vector7, eye.pos) * ((num4 - num3) * 0.5f);
				eye.vel += vector8;
				eye.pos += vector8;
			}
			eye.vel *= 0.975f;
			eye.vel.y -= room.gravity;
			eye.pos += eye.vel;
			SharedPhysics.TerrainCollisionData cd = new SharedPhysics.TerrainCollisionData(eye.pos, eye.lastPos, eye.vel, 2f, default(IntVector2), goThroughFloors: true);
			cd = SharedPhysics.HorizontalCollision(room, cd);
			cd = SharedPhysics.VerticalCollision(room, cd);
			cd = SharedPhysics.SlopesVertically(room, cd);
			eye.pos = cd.pos;
			eye.vel = cd.vel;
			eye.pos = Vector2.ClampMagnitude(eye.pos - vector7, num4 * 1.5f) + vector7;
		}
	}

	public void SwapChatterMode()
	{
		chatterAudible = !chatterAudible;
		if (chatterAudible)
		{
			timeToNextChatterModeSwitch = UnityEngine.Random.Range(80, 300) + (Tower ? 320 : 0);
		}
		else
		{
			timeToNextChatterModeSwitch = UnityEngine.Random.Range(40, 400);
		}
	}

	public void Act()
	{
		if (!base.safariControlled && ai.fleeFromCreature != null)
		{
			targetFlip = Mathf.Sign(room.MiddleOfTile(ai.fleeFromCreature.BestGuessForPosition()).x - base.bodyChunks[0].pos.x);
		}
		walkAnim += speed / 80f;
		walkAnim %= 1f;
		stepCounter++;
		if ((float)stepCounter * speed > 5f)
		{
			stepCounter = 0;
			legToMove = (legToMove + 1) % (legs.Length / 2);
			int num = legToMove;
			int num2 = legs.Length - legToMove - 1;
			int num3 = LegsSupporting;
			if (legs[num].Supporting)
			{
				num3--;
			}
			if (legs[num2].Supporting)
			{
				num3--;
			}
			if ((legs[num].WantsToStep || legs[num2].WantsToStep) && (!legs[num].Supporting || !legs[num2].Supporting || num3 >= 2))
			{
				legs[num].StartStep();
				legs[num2].StartStep();
			}
		}
		base.bodyChunks[0].vel.y += 0.5f / base.bodyChunks[0].mass;
		base.bodyChunks[1].vel.y -= 0.5f / base.bodyChunks[1].mass;
		float num4 = Mathf.InverseLerp(1f, 5f, Vector2.Distance(base.bodyChunks[0].pos, base.bodyChunks[0].lastPos));
		float num5 = 0.35f + Mathf.Sin(walkAnim * (float)Math.PI * 2f) * 0.15f * speed * num4;
		if (base.bodyChunks[1].pos.y < base.bodyChunks[0].pos.y - 3f)
		{
			base.bodyChunks[0].vel.x += num5 * flip / base.bodyChunks[0].mass;
			base.bodyChunks[1].vel.x -= num5 * flip / base.bodyChunks[1].mass;
		}
		if (Mathf.InverseLerp(Tower ? 0.25f : 0.75f, 0f, State.health) > 0f && UnityEngine.Random.value < (1f - State.health) * (Tower ? 0.002f : 0.01f))
		{
			Stun(UnityEngine.Random.Range(40, 120));
		}
	}

	public void Hide(bool spin)
	{
		if (hideTimer < 80)
		{
			if (hideTimer == 0)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.Drill_Crab_Enter_Shell, base.mainBodyChunk);
			}
			if (spin)
			{
				hideTimer = 400;
				spinOutTimer = 260;
			}
			else
			{
				hideTimer = Mathf.Min(UnityEngine.Random.Range(120, 400), UnityEngine.Random.Range(120, 400));
			}
		}
	}

	public void Panic()
	{
		if (spinOutTimer <= 0)
		{
			hideTimer = 400;
			spinOutTimer = 200;
		}
	}

	public void DoSmoke()
	{
		Vector2 vector = Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos);
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		Vector2 emissionPoint = base.bodyChunks[0].pos + vector * 21f - vector2 * 28f;
		if (smoke == null || smoke.room != room)
		{
			smoke = new ExhaustSmoke(room, this, fireColor, smokeColor);
			room.AddObject(smoke);
		}
		if (spinUp > 0.2f)
		{
			smoke.EmitParticle(emissionPoint, vector * (spinUp * UnityEngine.Random.Range(10f, 15f)) + Custom.RNV() * (UnityEngine.Random.value * 6f) + base.bodyChunks[0].vel * 0.75f);
		}
	}

	public bool BounceOffTerrain()
	{
		Vector2 vector = -base.bodyChunks[1].ContactPoint.ToVector2().normalized;
		if (vector == Vector2.zero)
		{
			return false;
		}
		Vector2 vector2 = Custom.PerpendicularVector(vector);
		if (Vector2.Dot(vector2, base.bodyChunks[1].pos - base.bodyChunks[0].pos) < 0f)
		{
			vector2 = -vector2;
		}
		Vector2 vector3 = Custom.RotateAroundOrigo(vector, UnityEngine.Random.Range(-45f, 45f));
		float num = 25f * spinUp * UnityEngine.Random.Range(0.4f, 1f);
		base.bodyChunks[0].vel += num * vector3;
		base.bodyChunks[1].vel += UnityEngine.Random.value * num * vector3;
		BodyChunk[] array = base.bodyChunks;
		foreach (BodyChunk obj in array)
		{
			obj.vel = Vector2.ClampMagnitude(obj.vel, 60f);
		}
		targetFlip = UnityEngine.Random.Range(-1f, 1f);
		Vector2? vector4 = SharedPhysics.ExactTerrainRayTracePos(room, base.bodyChunks[1].pos, base.bodyChunks[1].pos - vector * (base.bodyChunks[1].rad * 1.25f));
		Vector2 vector5 = vector4 ?? (base.bodyChunks[1].pos - vector * base.bodyChunks[1].rad);
		if (spinUp > 0.5f && sparkDelay == 0)
		{
			sparkDelay = 4;
			int num2 = (int)((float)UnityEngine.Random.Range(4, 9) * (spinUp * 2f - 1f));
			for (int j = 0; j < num2; j++)
			{
				room.AddObject(new Spark(vector5 + vector2 * (UnityEngine.Random.value * 5f) + vector, Custom.RNV() * 6f + vector2 * (UnityEngine.Random.value * 10f), new Color(1f, 1f, 0.8f), null, 5, 18));
			}
		}
		if (spinUp > 0.1f && terrainSoundDelay == 0)
		{
			terrainSoundDelay = 4;
			room.PlaySound(WatcherEnums.WatcherSoundID.Drill_Crab_Collide_Terrain, base.bodyChunks[0].pos, Mathf.Sqrt(spinUp), 1f, base.abstractCreature);
			room.ScreenMovement(base.firstChunk.pos, vector3 * (4f * spinUp), 0.4f * spinUp);
		}
		pitchDown = Mathf.Max(pitchDown, UnityEngine.Random.value * 0.5f);
		return vector4.HasValue;
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (myChunk == 1 && spinUp >= 0.25f)
		{
			DrillHitChunk(base.bodyChunks[myChunk], otherObject.bodyChunks[otherChunk]);
		}
	}

	public void DrillHitChunk(BodyChunk myChunk, BodyChunk otherChunk)
	{
		float magnitude = (myChunk.vel - otherChunk.vel).magnitude;
		float num = spinUp * Custom.LerpMap(magnitude, 0f, 20f, 1f, 2f);
		Vector2 vector = (otherChunk.pos - myChunk.pos).normalized * (num * 10f);
		if (otherChunk.owner is Creature creature)
		{
			if (creature is DrillCrab)
			{
				num *= 0.25f;
			}
			if (spinUp > 0.1f && objectHitDelay == 0)
			{
				objectHitDelay = 4;
				room.PlaySound(WatcherEnums.WatcherSoundID.Drill_Crab_Collide_Creature, base.bodyChunks[0].pos, spinUp, UnityEngine.Random.Range(0.9f, 1.1f), base.abstractCreature);
				creature.Violence(myChunk, vector, otherChunk, null, DamageType.Blunt, num, 20f);
				Vector2 vector2 = Custom.RotateAroundOrigo((myChunk.pos - otherChunk.pos).normalized, UnityEngine.Random.Range(-45f, 45f));
				float num2 = 15f * spinUp * UnityEngine.Random.Range(0.4f, 1f);
				base.bodyChunks[0].vel += num2 * vector2;
				base.bodyChunks[1].vel += UnityEngine.Random.value * num2 * vector2;
				(creature as Barnacle)?.LoseShell();
			}
		}
		else
		{
			otherChunk.vel += vector / otherChunk.mass;
		}
		pitchDown = Mathf.Max(pitchDown, UnityEngine.Random.value * 0.5f + 0.5f);
	}

	public void ResetLegs()
	{
		nextStepPos = base.bodyChunks[0].pos;
		Leg[] array = legs;
		foreach (Leg obj in array)
		{
			obj.ResetPhysics();
			obj.ForceAttach(base.bodyChunks[0].pos - new Vector2(0f, idealHeight));
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		if (base.coord.NodeDefined && !base.coord.TileDefined && placeRoom.abstractRoom.nodes[base.coord.abstractNode].type == AbstractRoomNode.Type.SideExit)
		{
			int num = base.coord.abstractNode - placeRoom.exitAndDenIndex.Length;
			RoomBorderExit roomBorderExit = placeRoom.borderExits[num];
			int num2 = placeRoom.TileHeight;
			IntVector2[] borderTiles = roomBorderExit.borderTiles;
			for (int i = 0; i < borderTiles.Length; i++)
			{
				IntVector2 intVector = borderTiles[i];
				num2 = Mathf.Min(num2, intVector.y);
			}
			float num3 = float.PositiveInfinity;
			borderTiles = roomBorderExit.borderTiles;
			for (int i = 0; i < borderTiles.Length; i++)
			{
				IntVector2 intVector2 = borderTiles[i];
				float num4 = Mathf.Abs((float)(intVector2.y - num2) - idealHeight / 20f);
				if (num4 < num3)
				{
					base.abstractCreature.pos.y = intVector2.y;
					num3 = num4;
				}
			}
		}
		base.PlaceInRoom(placeRoom);
		ResetLegs();
		Eye[] array = eyes;
		foreach (Eye obj in array)
		{
			obj.lastPos = base.bodyChunks[0].pos;
			obj.pos = base.bodyChunks[0].pos;
			obj.vel = Vector2.zero;
		}
	}

	public void FollowPath()
	{
		moveTargetAge++;
		if (moveTargetAge > maxMoveTargetAge)
		{
			RefreshMoveTarget();
		}
		Vector2 vector = Vector2.zero;
		if (base.safariControlled)
		{
			Player.InputPackage? inputPackage = inputWithDiagonals;
			if (inputPackage.HasValue)
			{
				Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
				if (valueOrDefault.analogueDir.magnitude > 0f)
				{
					vector = valueOrDefault.analogueDir.normalized;
				}
				else if (valueOrDefault.x != 0 || valueOrDefault.y != 0)
				{
					vector = new Vector2(valueOrDefault.x, valueOrDefault.y).normalized;
				}
				Vector2 vector2 = room.MiddleOfTile(base.bodyChunks[0].pos) + vector * 20f;
				if (!room.GetTile(vector2).IsSolid())
				{
					float num = (float)room.aimap.getClampedAItile(vector2).floorAltitude * 20f;
					float num2 = (Kneeling ? 0.5f : 1f) * Custom.LerpMap(StuckAmount, 0.2f, 1f, 1f, 0.7f);
					float num3 = idealHeight * num2;
					GetMoveTarget(room.GetTilePosition(vector2));
					float num4 = (num - num3) / 20f;
					if (num4 < 0f)
					{
						num4 = Mathf.Abs(num4) / 2f;
					}
					num4 /= 4f;
					if (num4 > 0.25f)
					{
						vector2.y += (num3 - num) * num4;
						vector = Custom.DirVec(base.bodyChunks[0].pos, vector2).normalized;
					}
				}
				if (valueOrDefault.jmp)
				{
					ref Player.InputPackage? reference = ref lastInputWithDiagonals;
					if (reference.HasValue && !reference.GetValueOrDefault().jmp)
					{
						if (vector != Vector2.zero)
						{
							targetFlip = vector.x;
						}
						else
						{
							targetFlip = ((!(targetFlip > 0f)) ? 1 : (-1));
						}
					}
				}
			}
		}
		else if (moveTarget != default(Vector2) && room != null)
		{
			if (ai.pathFinder == null || room.GetTilePosition(base.bodyChunks[0].pos) != ai.pathFinder.GetEffectualDestination.Tile || !Custom.DistLess(base.bodyChunks[0].pos, room.MiddleOfTile(moveTarget), 10f))
			{
				vector = Custom.DirVec(base.bodyChunks[0].pos, moveTarget);
				if (ai.stuckTracker != null)
				{
					ai.stuckTracker.satisfiedWithThisPosition = false;
				}
			}
			else if (ai.stuckTracker != null)
			{
				ai.stuckTracker.satisfiedWithThisPosition = true;
			}
			if (Mathf.Abs(moveTarget.x - base.bodyChunks[0].pos.x) > 35f)
			{
				targetFlip = ((moveTarget.x > base.bodyChunks[0].pos.x) ? 1f : (-1f));
			}
		}
		if (vector != Vector2.zero)
		{
			vector *= 0.4f * speed * (1f + StuckAmount);
			base.bodyChunks[0].vel += vector;
			if (moveTarget.y > base.bodyChunks[0].pos.y && base.bodyChunks[0].vel.y < -1f)
			{
				base.bodyChunks[0].vel.y *= 0.5f;
			}
		}
	}

	public MovementConnection GetMovementConnection(FloodFillPather pather, IntVector2 pos)
	{
		IntVector2 tile = new IntVector2(Mathf.Clamp(pos.x, 0, room.TileWidth - 1), Mathf.Clamp(pos.y, 0, room.TileHeight - 1));
		pos.y = tile.y;
		if ((pos.x <= 0 || pos.x >= room.TileWidth - 1) && pather.IsReachable(tile))
		{
			bool flag = pather.dest.x > room.TileWidth / 2;
			WorldCoordinate worldCoordinate = room.GetWorldCoordinate(pos);
			WorldCoordinate worldCoordinate2 = room.GetWorldCoordinate(pos + new IntVector2(flag ? 1 : (-1), 0));
			return new MovementConnection(MovementConnection.MovementType.Standard, worldCoordinate, worldCoordinate2, Mathf.Abs(worldCoordinate2.x - worldCoordinate.x) + Mathf.Abs(worldCoordinate2.y - worldCoordinate.y));
		}
		if (pather.IsReachable(pos))
		{
			IntVector2? nextPos = pather.GetNextPos(pos);
			if (nextPos.HasValue)
			{
				IntVector2 valueOrDefault = nextPos.GetValueOrDefault();
				WorldCoordinate worldCoordinate3 = room.GetWorldCoordinate(pos);
				WorldCoordinate worldCoordinate4 = room.GetWorldCoordinate(valueOrDefault);
				return new MovementConnection(MovementConnection.MovementType.Standard, worldCoordinate3, worldCoordinate4, Mathf.Abs(worldCoordinate4.x - worldCoordinate3.x) + Mathf.Abs(worldCoordinate4.y - worldCoordinate3.y));
			}
		}
		return default(MovementConnection);
	}

	public void RefreshMoveTarget()
	{
		moveTargetAge = 0;
		FloodFillPather pathFinderOverride = ai.pathFinderOverride;
		DrillCrabPather drillCrabPather = ai.pathFinder as DrillCrabPather;
		MovementConnection movementConnection = default(MovementConnection);
		IntVector2 tilePosition = room.GetTilePosition(base.bodyChunks[0].pos);
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
				if (valueOrDefault.AnyDirectionalInput)
				{
					movementConnection = new MovementConnection(MovementConnection.MovementType.Standard, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + vector * 40f), 2);
				}
			}
		}
		else
		{
			int maxRadius = (Tower ? 4 : 2);
			if (pathFinderOverride != null)
			{
				foreach (IntVector2 nearbyTile in GetNearbyTiles(maxRadius))
				{
					movementConnection = GetMovementConnection(pathFinderOverride, tilePosition + nearbyTile);
					if (movementConnection != default(MovementConnection))
					{
						break;
					}
				}
			}
			else
			{
				int num = -1;
				foreach (IntVector2 nearbyTile2 in GetNearbyTiles(maxRadius))
				{
					num = Mathf.Max(num, drillCrabPather.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(tilePosition + nearbyTile2)).generation);
				}
				foreach (IntVector2 nearbyTile3 in GetNearbyTiles(maxRadius))
				{
					if (drillCrabPather.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(tilePosition + nearbyTile3)).generation >= num)
					{
						movementConnection = drillCrabPather.FollowPath(room.GetWorldCoordinate(tilePosition + nearbyTile3), actuallyFollowingThisPath: true);
						if (movementConnection != default(MovementConnection) || room == null)
						{
							break;
						}
					}
				}
			}
		}
		noPathCounter = 0;
		if (room == null)
		{
			return;
		}
		IntVector2 destTile = movementConnection.DestTile;
		nextStepPos = room.MiddleOfTile(destTile);
		if (room.aimap.getTerrainProximity(tilePosition) <= 2)
		{
			squeeze = true;
		}
		for (int i = 0; i < 16; i++)
		{
			if (i < 4 && room.aimap.getTerrainProximity(destTile) <= 2)
			{
				squeeze = true;
			}
			MovementConnection movementConnection2;
			if (pathFinderOverride != null)
			{
				movementConnection2 = GetMovementConnection(pathFinderOverride, destTile);
				bool num2 = movementConnection2.DestTile.x < movementConnection2.StartTile.x == movementConnection2.StartTile.x < room.TileWidth / 2;
				bool flag = base.mainBodyChunk.pos.x < -600f || base.mainBodyChunk.pos.x > 600f + room.PixelWidth;
				if (num2 && flag)
				{
					Destroy();
					base.abstractCreature.Destroy();
					room.abstractRoom.RemoveEntity(base.abstractCreature);
				}
			}
			else
			{
				movementConnection2 = drillCrabPather.FollowPath(room.GetWorldCoordinate(destTile), actuallyFollowingThisPath: false);
			}
			if (movementConnection2 == default(MovementConnection) || movementConnection2.type != 0 || (pathFinderOverride == null && movementConnection2.destinationCoord == drillCrabPather.GetEffectualDestination))
			{
				break;
			}
			destTile = movementConnection2.DestTile;
			if (Mathf.Abs(base.bodyChunks[0].pos.x - (float)destTile.x * 20f) > 120f || !room.RayTraceTilesForTerrain(tilePosition.x, tilePosition.y, destTile.x, destTile.y))
			{
				break;
			}
			if (Mathf.Abs(base.bodyChunks[0].pos.x - (float)destTile.x) < (Tower ? 80f : 30f))
			{
				nextStepPos = GetMoveTarget(destTile);
			}
		}
		nextStepPos = base.bodyChunks[0].pos + Vector2.ClampMagnitude(nextStepPos - base.bodyChunks[0].pos, base.bodyChunks[0].vel.magnitude * 20f);
		moveTarget = ((destTile.x == movementConnection.StartTile.x) ? room.MiddleOfTile(destTile) : GetMoveTarget(destTile));
	}

	public void AccessSideSpace(WorldCoordinate start, WorldCoordinate dest)
	{
		room.game.shortcuts.CreatureTakeFlight(this, AbstractRoomNode.Type.SideExit, start, dest);
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos onAppendagePos, Vector2 direction)
	{
		return !IsDamageBlocked(DamageType.Stab, direction, chunk);
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (IsDamageBlocked(type, directionAndMomentum, hitChunk))
		{
			room?.PlaySound(SoundID.Lizard_Head_Shield_Deflect, base.mainBodyChunk.pos, base.abstractCreature);
			return;
		}
		float damage2 = damage;
		if (type == DamageType.Stab && damage >= 0.25f && (hitAppendage == null || !legs[hitAppendage.appendage.appIndex].Dead))
		{
			Panic();
		}
		if (hitAppendage != null)
		{
			Leg obj = legs[hitAppendage.appendage.appIndex];
			if (type != DamageType.Electric)
			{
				stunBonus = 0f;
			}
			float num = (Tower ? 2f : (2f / 3f));
			damage2 = Mathf.Min(obj.Health, damage / num) * 0.5f;
			obj.Health = Mathf.Max(obj.Health - damage / num, 0f);
		}
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage2, stunBonus);
	}

	public override void Stun(int st)
	{
		if (st > 30)
		{
			base.Stun(st);
		}
	}

	public bool IsDamageBlocked(DamageType type, Vector2? dir, BodyChunk chunk)
	{
		if (chunk == null || type == DamageType.Electric || type == DamageType.Explosion || type == DamageType.Water)
		{
			return false;
		}
		if (!dir.HasValue)
		{
			return chunk.index != 0;
		}
		float num = Vector2.Angle(dir.Value, base.bodyChunks[1].pos - base.bodyChunks[0].pos);
		if (chunk.index == 0)
		{
			return num > 70f;
		}
		return true;
	}

	public Vector2 GetMoveTarget(IntVector2 tile)
	{
		int num = ((!Tower) ? 1 : 3);
		float num2 = 0f;
		for (int i = -num; i <= num; i++)
		{
			num2 += (float)room.aimap.getClampedAItile(tile.x + i, tile.y).floorAltitude * 20f - 10f;
		}
		num2 /= (float)(2 * num + 1);
		float num3 = (Kneeling ? 0.5f : 1f) * Custom.LerpMap(StuckAmount, 0.2f, 1f, 1f, 0.7f);
		IntVector2 tilePosition = room.GetTilePosition(room.MiddleOfTile(tile) + new Vector2(0f, idealHeight * num3 - num2));
		IntVector2 intVector = new IntVector2(0, Math.Sign(tilePosition.y - tile.y));
		for (int j = 0; j < 18; j++)
		{
			if (tile == tilePosition)
			{
				break;
			}
			if (!room.aimap.TileAccessibleToCreature(IntVector2.RectClamp(tile + intVector, 0, 0, room.TileWidth - 1, room.TileHeight - 1), base.Template))
			{
				break;
			}
			tile += intVector;
		}
		return room.MiddleOfTile(tile);
	}

	public static List<IntVector2> GetNearbyTiles(int maxRadius)
	{
		if (nearbyTilesCache.TryGetValue(maxRadius, out var value))
		{
			return value;
		}
		value = new List<IntVector2>();
		for (int i = -maxRadius; i <= maxRadius; i++)
		{
			for (int j = -maxRadius; j <= maxRadius; j++)
			{
				if (new Vector2(j, i).magnitude <= (float)maxRadius)
				{
					value.Add(new IntVector2(j, i));
				}
			}
		}
		value.Sort((IntVector2 a, IntVector2 b) => a.ToVector2().magnitude.CompareTo(b.ToVector2().magnitude));
		nearbyTilesCache[maxRadius] = value;
		return value;
	}

	public Vector2 AppendagePosition(int appendage, int segment)
	{
		if (segment == 0)
		{
			return base.bodyChunks[0].pos;
		}
		return legs[appendage].segments[segment - 1].pos;
	}

	public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
	{
		legs[pos.appendage.appIndex].ApplyForce(pos, momentum);
	}
}
