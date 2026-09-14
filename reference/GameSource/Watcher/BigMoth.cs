using System;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class BigMoth : InsectoidCreature
{
	public struct IndividualVariations
	{
		public Color bodyColor;

		public Color secondaryColor;

		public float kneeColorness;

		public float legPuff;

		public float chub;

		public int antennaLength;

		public float antennaWidth;

		public int taggedLeg;

		public Color legTagColor;

		public IndividualVariations(Color bodyColor, Color secondaryColor, float kneeColorness, float chub, float legPuff, int antennaLength, float antennaWidth, int taggedLeg, Color legTagColor)
		{
			this.bodyColor = bodyColor;
			this.secondaryColor = secondaryColor;
			this.kneeColorness = kneeColorness;
			this.legPuff = legPuff;
			this.chub = chub;
			this.antennaLength = antennaLength;
			this.antennaWidth = antennaWidth;
			this.taggedLeg = taggedLeg;
			this.legTagColor = legTagColor;
		}
	}

	public class MothWingPair
	{
		public class Mode : ExtEnum<Mode>
		{
			public static Mode Glide = new Mode("Glide", register: true);

			public static Mode ReadyFlap = new Mode("ReadyFlap", register: true);

			public static Mode Flap = new Mode("Flap", register: true);

			public Mode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public BigMoth bug;

		public int pairIndex;

		public Mode mode;

		public float flapness;

		public float flapVel;

		public bool madeFlapSound;

		public MothWingPair(BigMoth bug, int pairIndex)
		{
			this.bug = bug;
			this.pairIndex = pairIndex;
		}

		public void Update()
		{
			_ = flapness;
			float num = bug.flyUpness * ((pairIndex == 0) ? 0.7f : 1f) * (0.5f + bug.runSpeed) * (bug.Small ? 3f : 1f);
			if (!bug.wantToFly)
			{
				mode = Mode.ReadyFlap;
			}
			for (int i = 1; i < 3; i++)
			{
				bug.bodyChunks[i].vel.y *= 0.97f + 0.03f * (1f - Mathf.Sin(flapness * (float)Math.PI));
				if (bug.narrowUpcoming)
				{
					bug.bodyChunks[i].vel *= 0.8f;
				}
			}
			if (mode == Mode.Glide)
			{
				flapness = Mathf.Lerp(flapness, 0.5f, 0.05f);
				flapVel = 0f;
			}
			else if (mode == Mode.ReadyFlap)
			{
				flapVel += (bug.Small ? 0.012f : 0.007f) * num;
				if (flapness >= 1f)
				{
					flapness = 1f;
					flapVel = 0f;
					if (pairIndex == 0)
					{
						mode = Mode.Flap;
					}
				}
				else if (flapness < 0f)
				{
					flapness = 0f;
				}
			}
			else if (mode == Mode.Flap)
			{
				flapVel -= 0.01f * num;
				Vector2 vector = new Vector2(bug.moveDirection.x * Mathf.Abs(flapVel) * 10f, Mathf.Abs(flapVel) * 40f * ((pairIndex == 0) ? 0.7f : 1f)) * ((1f - Mathf.Max(bug.runSpeed - 0.5f, 0f) * 0.3f) * (bug.narrowUpcoming ? 3f : 1f));
				for (int j = 0; j < 3; j++)
				{
					bug.bodyChunks[j].vel += vector * (((j == 1) ? 1.5f : 0.25f) * (bug.Small ? 0.5f : 1f));
				}
				if ((double)flapness < 0.5)
				{
					madeFlapSound = true;
					float num2 = Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(flapVel));
					bug.room.PlaySound(WatcherEnums.WatcherSoundID.Moth_Wingflaps, bug.bodyChunks[1].pos, ((0.2f + num2 * 0.8f) * ((pairIndex == 0) ? 0.77f : 1f) - UnityEngine.Random.value * 0.1f) * Mathf.Min(bug.scale, 1.2f), 0.8f + num2 * 0.4f - ((pairIndex == 0) ? 0f : 0.1f) + UnityEngine.Random.value * 0.2f);
				}
				else
				{
					madeFlapSound = false;
				}
				if (flapness <= 0f)
				{
					flapness = 0f;
					flapVel = 0f;
					mode = Mode.Glide;
					if (pairIndex == 0 && bug.wingPairs[1].mode == Mode.ReadyFlap)
					{
						bug.wingPairs[1].mode = Mode.Flap;
					}
				}
			}
			if (!bug.wantToFly)
			{
				flapness = Mathf.Lerp(flapness, 0f, 0.1f);
				flapVel = 0f;
			}
			flapness += flapVel;
			if (bug.graphicsModule != null)
			{
				BigMothGraphics bigMothGraphics = bug.graphicsModule as BigMothGraphics;
				float num3 = 0f;
				float num4 = 0f;
				for (int k = 0; k < bigMothGraphics.wings.Length; k++)
				{
					num3 += Vector2.Distance(bigMothGraphics.wings[k].segments[bigMothGraphics.wings[k].segments.GetLength(0) - 1, 0] - bigMothGraphics.wings[k].segments[0, 0], bigMothGraphics.wings[k].segments[bigMothGraphics.wings[k].segments.GetLength(0) - 1, 1] - bigMothGraphics.wings[k].segments[0, 1]);
					num4 += bigMothGraphics.wings[k].segments[bigMothGraphics.wings[k].segments.GetLength(0) - 1, 1].y - bigMothGraphics.wings[k].segments[bigMothGraphics.wings[k].segments.GetLength(0) - 1, 0].y - (bigMothGraphics.wings[k].segments[0, 1].y - bigMothGraphics.wings[k].segments[0, 0].y);
				}
				num3 /= (float)bigMothGraphics.wings.Length;
				num4 /= (float)bigMothGraphics.wings.Length;
				bug.wooshSound.volume = Custom.SCurve(Mathf.InverseLerp(0.4f, 18f, num3), 0.6f) * (1f - bigMothGraphics.wingStashness) * 0.9f * Mathf.Min(bug.scale, 1.2f);
				bug.wooshSound.pitch = Mathf.Lerp(0.2f, 1.25f, Mathf.InverseLerp(-20f, 20f, num4));
			}
			else
			{
				bug.wooshSound.volume = 0f;
				bug.wooshSound.pitch = 1f;
			}
			bug.wooshSound.pos = bug.bodyChunks[1].pos;
			bug.wooshSound.Update();
		}
	}

	public class MothLeg
	{
		public DebugSprite backLineSpr;

		public DebugSprite lineSpr;

		public DebugSprite conSpr;

		public DebugSprite footSpr;

		public DebugSprite idealSpr;

		public BigMoth bug;

		public int pairIndex;

		public int index;

		public Vector2 connectionPos;

		public Vector2 lastConnectionPos;

		public Vector2 footPos;

		public Vector2 lastFootPos;

		public Vector2? stepPos;

		public Vector2? idealStepPos;

		public bool securedFoot;

		public bool backLegs;

		public int connectedChunk;

		public float maxReach;

		public float stepRadius;

		public int stretchCounter;

		public float legTension;

		public float lastAwareLegExtension;

		public bool allowStepToTriggerOther;

		public float lastStepDistToNewStep;

		public float stepProgressCounter;

		public BodyChunk grabbedChunk;

		public float scale => bug.scale;

		public Room room => bug.room;

		public MothLeg(BigMoth bug, int pairIndex, int index)
		{
			this.bug = bug;
			this.pairIndex = pairIndex;
			this.index = index;
			backLegs = pairIndex == 0;
			if (pairIndex == 0)
			{
				maxReach = 144f * scale;
				stepRadius = 100f * scale;
				connectedChunk = 2;
			}
			else
			{
				maxReach = 190f * scale;
				stepRadius = 200f * scale;
				connectedChunk = 1;
			}
		}

		public void Update()
		{
			lastConnectionPos = connectionPos;
			lastFootPos = footPos;
			Vector2 vector = Custom.DirVec(bug.bodyChunks[2].pos, bug.bodyChunks[1].pos);
			Vector2 vector2 = Custom.PerpendicularVector(vector);
			float num = 1f - Mathf.Abs(bug.flip);
			connectionPos = bug.bodyChunks[connectedChunk].pos + (vector2 * (5f * ((index % 2 == 0) ? 1f : (-1f)) * num) + (backLegs ? (-vector * 5f) : (vector * 5f))) * scale;
			if (grabbedChunk != null)
			{
				if (bug.grasps[0] == null || (bug.grasps[0] != null && grabbedChunk.owner != bug.grasps[0].grabbed))
				{
					grabbedChunk = null;
				}
				else
				{
					idealStepPos = null;
					stepPos = null;
					footPos = grabbedChunk.pos;
				}
			}
			if (idealStepPos.HasValue)
			{
				Vector2 vector3 = Custom.DirVec(connectionPos, idealStepPos.Value);
				Vector2? vector4 = stepPos;
				if (TryPlaceFoot(connectionPos + vector3 * (maxReach * (bug.wantToFly ? 0.7f : 1f))))
				{
					if (vector4.HasValue)
					{
						stepProgressCounter = 0f;
						lastStepDistToNewStep = Vector2.Distance(vector4.Value, stepPos.Value);
					}
					idealStepPos = null;
				}
				else
				{
					int num2 = ((!(idealStepPos.Value.x > bug.bodyChunks[2].pos.x)) ? 1 : (-1));
					idealStepPos = idealStepPos.Value + new Vector2(num2 * 2, 0f);
				}
			}
			if (stepPos.HasValue && Vector2.Distance(connectionPos, stepPos.Value) > maxReach)
			{
				stepPos = null;
			}
			if (stepPos.HasValue)
			{
				StepMode();
			}
			else
			{
				FreeMode();
			}
			footPos += bug.bodyChunks[connectedChunk].vel * 0.3f;
			Vector2? vector5 = SharedPhysics.ExactTerrainRayTracePos(room, connectionPos, footPos);
			if (vector5.HasValue)
			{
				footPos = vector5.Value;
			}
			float num3 = Vector2.Distance(connectionPos, footPos);
			Vector2 vector6 = Custom.DirVec(connectionPos, footPos);
			if (num3 > maxReach)
			{
				footPos = connectionPos + vector6 * maxReach;
				stepPos = null;
			}
		}

		public bool TryGrab(PhysicalObject obj)
		{
			if (grabbedChunk != null && grabbedChunk.owner == obj)
			{
				return true;
			}
			for (int i = 0; i < obj.bodyChunks.Length; i++)
			{
				if (Vector2.Distance(Custom.ClosestPointOnLineSegment(connectionPos, footPos, obj.bodyChunks[i].pos), obj.bodyChunks[i].pos) < obj.bodyChunks[i].rad + 10f)
				{
					grabbedChunk = obj.bodyChunks[i];
					return true;
				}
			}
			return false;
		}

		public void StepMode()
		{
			float num = 1f;
			if (lastStepDistToNewStep > 10f * (0.5f + 0.5f * scale))
			{
				stepProgressCounter += 20f * bug.runSpeed * (0.5f + 0.5f * scale);
				float num2 = Mathf.Clamp01(stepProgressCounter / lastStepDistToNewStep);
				footPos.y += Mathf.Sin(num2 * (float)Math.PI) * 7f * ((pairIndex == 0) ? 0.7f : 1f) * (bug.runSpeed + Mathf.Pow(bug.runSpeed, 2f)) * (0.5f + 0.5f * scale);
				num = 0.25f + 0.75f * num2;
			}
			footPos = Custom.MoveTowards(footPos, stepPos.Value, 24f * num * bug.runSpeed * (0.5f + 0.5f * scale));
			if (Vector2.Distance(footPos, stepPos.Value) < 7f * scale || securedFoot)
			{
				footPos = stepPos.Value;
				if (!securedFoot)
				{
					securedFoot = true;
					bug.LegSecuredFoot(pairIndex, index);
					allowStepToTriggerOther = false;
				}
			}
			Vector2 vector = Custom.DirVec(connectionPos, stepPos.Value);
			float num3 = Vector2.Distance(connectionPos, stepPos.Value);
			if ((double)num3 > (double)maxReach * 0.85)
			{
				stretchCounter++;
				if (stretchCounter > 70)
				{
					stretchCounter = 0;
					TryPlaceFoot(connectionPos + new Vector2(vector.x * 0.5f, vector.y).normalized * num3);
					return;
				}
			}
			else
			{
				stretchCounter = 0;
			}
			if (bug.Consious)
			{
				lastAwareLegExtension = Vector2.Distance(connectionPos, footPos);
				legTension = 1f;
			}
			else if (securedFoot && stepPos.HasValue)
			{
				legTension -= 0.03f * UnityEngine.Random.value * (0.5f + legTension * 0.5f);
				if ((double)(legTension * scale) <= 0.01)
				{
					legTension = 0f;
					stepPos = null;
				}
				else if (num3 < lastAwareLegExtension)
				{
					float num4 = 0.5f * legTension * scale;
					bug.bodyChunks[connectedChunk].vel *= 0.9f;
					bug.bodyChunks[connectedChunk].vel -= vector * ((lastAwareLegExtension - num3) * num4);
				}
			}
			else
			{
				stepPos = null;
			}
		}

		public void FreeMode()
		{
			stretchCounter = 0;
			float num = Vector2.Distance(connectionPos, footPos);
			Vector2 vector = Custom.DirVec(connectionPos, footPos);
			footPos += vector * ((maxReach * 0.5f - num) * 0.3f) / scale;
			footPos.y -= bug.gravity * bug.room.gravity * 10f;
		}

		public void SetIdealStepPos(Vector2 pos)
		{
			idealStepPos = pos;
		}

		public bool TryPlaceFoot(Vector2 idealPos)
		{
			allowStepToTriggerOther = true;
			Vector2? vector = SharedPhysics.ExactTerrainRayTracePos(room, connectionPos, idealPos);
			if (vector.HasValue)
			{
				Vector2 vector2 = vector.Value - Custom.DirVec(connectionPos, idealPos) * 5f;
				if (room.GetTile(vector2 + new Vector2(0f, -10f)).Solid || (room.terrain != null && room.terrain.Contains(vector.Value + new Vector2(0f, -20f))))
				{
					stretchCounter = 0;
					securedFoot = false;
					stepPos = vector.Value;
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
			connectionPos = bug.bodyChunks[connectedChunk].pos;
			lastConnectionPos = connectionPos;
			stepPos = null;
		}
	}

	public class BigMothState : HealthState
	{
		public class TagOwner : ExtEnum<TagOwner>
		{
			public static TagOwner None = new TagOwner("None", register: true);

			public static TagOwner GA = new TagOwner("GA", register: true);

			public static TagOwner DG = new TagOwner("DG", register: true);

			public static TagOwner Slugcat = new TagOwner("Slugcat", register: true);

			public TagOwner(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public int foodStored;

		public float babyness;

		public int hatchedCycle;

		public bool tagged;

		public TagOwner tagOwner;

		public BigMothState(AbstractCreature creature)
			: base(creature)
		{
			tagOwner = TagOwner.None;
			hatchedCycle = -1;
		}

		public override string ToString()
		{
			string text = "";
			if (tagOwner != TagOwner.None)
			{
				text += string.Format(CultureInfo.InvariantCulture, "<cB>TagOwner<cC>{0}", tagOwner.Index);
			}
			if (hatchedCycle >= 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "<cB>HatchedCycle<cC>{0}", hatchedCycle);
			}
			return base.ToString() + text;
		}

		public override void LoadFromString(string[] s)
		{
			base.LoadFromString(s);
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < s.Length; i++)
			{
				string[] array = Regex.Split(s[i], "<cC>");
				switch (array[0])
				{
				case "TagOwner":
					flag = true;
					tagOwner = new TagOwner(ExtEnum<TagOwner>.values.entries[Mathf.Min(int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), ExtEnum<TagOwner>.values.entries.Count)]);
					break;
				case "HatchedCycle":
					flag2 = true;
					hatchedCycle = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					babyness = (creature.world.game.IsStorySession ? (1f - Mathf.Clamp01((float)(creature.world.game.GetStorySession.saveState.cycleNumber - hatchedCycle) / 7f)) : 0f);
					break;
				}
			}
			if (flag)
			{
				unrecognizedSaveStrings.Remove("TagOwner");
			}
			if (flag2)
			{
				unrecognizedSaveStrings.Remove("HatchedCycle");
			}
		}
	}

	public BigMothAI AI;

	public float scale;

	public float flip;

	public float flipness;

	public int flipSide = 1;

	public Vector2 moveDirection;

	public float flyUpness;

	public bool safariWalkMode;

	public float shake;

	public float runSpeed;

	public float groundAngle;

	public float stance = 0.6f;

	public int legsOnGround;

	public Vector2 feetAvgPos;

	public int holdThroughWallCounter;

	public Vector2 lookPos;

	public Vector2 randomLookPos;

	public bool randomLookAtCreature;

	public int randomLookCounter;

	public float headStayUpCounter;

	public MothLeg[,] legs;

	public MothWingPair[] wingPairs;

	public BodyChunk drinkChunk;

	public bool drinkingChunk;

	public float tongueHold;

	public int eatCounter;

	public Rope rope;

	public bool wantToFly;

	public int wantToFlyCounter;

	public int stuckCounter;

	public bool narrowUpcoming;

	public PathfindingVisualizer pathfindingVisualizer;

	public StaticSoundLoop wooshSound;

	public IndividualVariations iVars;

	public bool Small => base.Template.type == WatcherEnums.CreatureTemplateType.SmallMoth;

	public void GenerateIVars()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(base.abstractCreature.ID.RandomSeed);
		scale *= 0.9f + ((base.abstractCreature.personality.dominance - Mathf.Pow(UnityEngine.Random.value, 2f)) * 0.6f + (1f - Mathf.Pow(UnityEngine.Random.value, 2f)) * 0.4f) * 0.2f;
		int antennaLength = Mathf.Max(Mathf.RoundToInt((float)UnityEngine.Random.Range(4, 7) * scale), 3);
		float antennaWidth = UnityEngine.Random.Range(2f, 7f) * scale;
		Color white = Color.white;
		Color b = Color.Lerp(new Color(0.5f, 0.25f, 0.1f), new Color(0.6f, 0.5f, 0.4f), UnityEngine.Random.value);
		white = ((!Small) ? Color.Lerp(white, b, Mathf.Pow(UnityEngine.Random.value, 2f) * 0.17f) : Color.Lerp(Color.Lerp(new Color(0.35f, 0.35f, 0.35f), new Color(0.433f, 0.433f, 0.433f), Mathf.Pow(UnityEngine.Random.value, 2f)), b, Mathf.Pow(UnityEngine.Random.value, 2f) * 0.1f));
		white = Color.Lerp(white, new Color(1f, 0.7f, 0f), (base.abstractCreature.personality.dominance - Mathf.Pow(UnityEngine.Random.value, 2f)) * 0.1f);
		Color secondaryColor = Color.Lerp(Color.Lerp(white, new HSLColor(UnityEngine.Random.value * 0.17f, 1f, 0.5f).rgb, UnityEngine.Random.value * (Small ? 0.2f : 0.4f)), Small ? Color.white : Color.black, Small ? 0.7f : 0.3f);
		bool num = (double)UnityEngine.Random.value < 0.1;
		iVars = new IndividualVariations(white, secondaryColor, UnityEngine.Random.value, Mathf.Lerp(UnityEngine.Random.value, 0.5f, UnityEngine.Random.value), Mathf.Pow(UnityEngine.Random.value, 2f), antennaLength, antennaWidth, UnityEngine.Random.Range(0, 4), Color.black);
		if (num && (base.abstractCreature.state as BigMothState).tagOwner == BigMothState.TagOwner.None && Region.IsAncientUrbanRegion(base.abstractCreature.world.name))
		{
			if ((double)UnityEngine.Random.value > 0.4)
			{
				(base.abstractCreature.state as BigMothState).tagOwner = BigMothState.TagOwner.GA;
			}
			else
			{
				(base.abstractCreature.state as BigMothState).tagOwner = BigMothState.TagOwner.DG;
			}
		}
		if ((base.abstractCreature.state as BigMothState).tagOwner == BigMothState.TagOwner.None)
		{
			iVars.taggedLeg = -1;
		}
		else if ((base.abstractCreature.state as BigMothState).tagOwner == BigMothState.TagOwner.GA)
		{
			iVars.taggedLeg = 2;
		}
		else if ((base.abstractCreature.state as BigMothState).tagOwner == BigMothState.TagOwner.Slugcat)
		{
			iVars.legTagColor = new HSLColor(UnityEngine.Random.value, 0.45f + UnityEngine.Random.value * 0.6f, 0.3f + UnityEngine.Random.value * 0.2f).rgb;
		}
		if (iVars.legTagColor == Color.black)
		{
			iVars.legTagColor = TagColor((base.abstractCreature.state as BigMothState).tagOwner);
		}
		UnityEngine.Random.state = state;
	}

	public static Color TagColor(BigMothState.TagOwner owner)
	{
		if (owner == BigMothState.TagOwner.DG)
		{
			return new Color(0f, 0.2745f, 0.6549f);
		}
		if (owner == BigMothState.TagOwner.GA)
		{
			return new Color(0.2f, 0.8196f, 0.1843f);
		}
		return Color.black;
	}

	public BigMoth(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		scale = (Small ? 0.3f : 1f);
		scale *= 1f - (base.State as BigMothState).babyness * 0.6f;
		GenerateIVars();
		float num = 5.5f;
		base.bodyChunks = new BodyChunk[5];
		float num2 = Mathf.Pow(scale, 2f);
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 9f * scale, num * 0.05f * num2);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 13f * scale, num * 0.3f * num2);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 13f * scale, num * 0.3f * num2);
		base.bodyChunks[3] = new BodyChunk(this, 3, new Vector2(0f, 0f), 13f * scale, num * 0.2f * num2);
		base.bodyChunks[4] = new BodyChunk(this, 4, new Vector2(0f, 0f), 11f * scale, num * 0.15f * num2);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].restrictInRoomRange = 2400f;
			base.bodyChunks[i].defaultRestrictInRoomRange = 2400f;
		}
		bodyChunkConnections = new BodyChunkConnection[6];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 20f * scale, BodyChunkConnection.Type.Pull, 1f, -1f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 20f * scale, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[3], 30f * scale, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[3] = new BodyChunkConnection(base.bodyChunks[3], base.bodyChunks[4], 30f * scale, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[4] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[3], 50f * scale, BodyChunkConnection.Type.Push, 1f, -1f);
		bodyChunkConnections[5] = new BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[4], 60f * scale, BodyChunkConnection.Type.Push, 1f, -1f);
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.35f;
		collisionLayer = 1;
		base.waterFriction = 0.9f;
		base.buoyancy = 1.2f;
		windAffectiveness = 0.25f;
		base.GoThroughFloors = true;
		legs = new MothLeg[2, 2];
		for (int j = 0; j < legs.GetLength(0); j++)
		{
			for (int k = 0; k < legs.GetLength(1); k++)
			{
				legs[j, k] = new MothLeg(this, j, k);
			}
		}
		wingPairs = new MothWingPair[2];
		for (int l = 0; l < wingPairs.Length; l++)
		{
			wingPairs[l] = new MothWingPair(this, l);
		}
	}

	public override void RecreateSticksFromAbstract()
	{
		AbstractCreature abstractCreature = null;
		for (int i = 0; i < base.abstractCreature.stuckObjects.Count; i++)
		{
			if (base.abstractCreature.stuckObjects[i] is AbstractPhysicalObject.CreatureGripStick && base.abstractCreature.stuckObjects[i].A == base.abstractCreature && base.abstractCreature.stuckObjects[i].B is AbstractCreature abstractCreature2 && abstractCreature2.creatureTemplate.type == WatcherEnums.CreatureTemplateType.MothGrub)
			{
				abstractCreature = (base.abstractCreature.stuckObjects[i] as AbstractPhysicalObject.CreatureGripStick).B as AbstractCreature;
			}
		}
		base.RecreateSticksFromAbstract();
		if (abstractCreature != null)
		{
			abstractCreature.LoseAllStuckObjects();
			(base.abstractCreature.abstractAI as BigMothAbstractAI).holdBaby = abstractCreature;
			(base.abstractCreature.abstractAI as BigMothAbstractAI).forceRealHoldTime = 40;
		}
	}

	public void AccessSkyGate(WorldCoordinate start, WorldCoordinate dest)
	{
		room.game.shortcuts.CreatureTakeFlight(this, AbstractRoomNode.Type.SkyExit, start, dest);
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new BigMothGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, new Vector2(Input.mousePosition.x, Input.mousePosition.y) + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (drinkChunk != null && room != drinkChunk.owner.room)
		{
			ReleaseTongue();
		}
		if (drinkChunk != null && drinkingChunk)
		{
			for (int i = 0; i < drinkChunk.owner.grabbedBy.Count; i++)
			{
				if (!AI.knownThiefs.Contains(drinkChunk.owner.grabbedBy[i].grabber.abstractCreature))
				{
					room.socialEventRecognizer.SocialEvent(SocialEventRecognizer.EventID.Theft, drinkChunk.owner.grabbedBy[i].grabber, this, drinkChunk.owner);
					AI.knownThiefs.Add(drinkChunk.owner.grabbedBy[i].grabber.abstractCreature);
				}
			}
			Vector2 pos = drinkChunk.pos;
			tongueHold = Mathf.MoveTowards(tongueHold, 0f, 0.02f);
			float num = Vector2.Distance(base.firstChunk.pos, pos);
			if (num > 120f * scale)
			{
				Vector2 vector = Custom.DirVec(base.firstChunk.pos, pos);
				float num2 = (num - 120f * scale) * 0.2f;
				tongueHold += num2 * 0.01f;
				float num3 = drinkChunk.mass / (base.firstChunk.mass + drinkChunk.mass);
				Vector2 vector2 = vector * (num2 * num3);
				Vector2 vector3 = vector * (num2 * (1f - num3));
				base.firstChunk.pos += vector2;
				base.firstChunk.vel += vector2;
				drinkChunk.pos -= vector3;
				drinkChunk.vel -= vector3;
			}
			if (base.Consious && drinkChunk.owner is Creature { State: { } state } creature)
			{
				if (creature is MothGrub mothGrub)
				{
					eatCounter++;
					if ((float)eatCounter > 200f / scale)
					{
						(base.State as BigMothState).foodStored = Mathf.Max((base.State as BigMothState).foodStored - 1, 0);
						eatCounter = 0;
						mothGrub.lookOffTime = 20;
						mothGrub.lookOffWay = Vector2.Lerp(mothGrub.lookOffWay, Custom.DirVec(mothGrub.firstChunk.pos, base.firstChunk.pos), 0.25f);
					}
				}
				else
				{
					HealthState healthState = null;
					if (state is HealthState)
					{
						healthState = state as HealthState;
					}
					if (!creature.dead && healthState != null)
					{
						healthState.health -= 0.075f * scale / (16f * Mathf.Max(1f, base.TotalMass));
					}
					else if (state.meatLeft > 0)
					{
						eatCounter++;
						if ((float)eatCounter > 200f / scale)
						{
							(base.State as BigMothState).foodStored++;
							state.meatLeft--;
							eatCounter = 0;
							if (state.meatLeft == 0 && !creature.dead)
							{
								creature.Die();
							}
						}
					}
				}
			}
			if (tongueHold > 1f || room != drinkChunk.owner.room || !room.VisualContact(base.firstChunk.pos, drinkChunk.pos))
			{
				ReleaseTongue();
			}
		}
		if (base.grasps[0] != null)
		{
			GrabUpdate();
		}
		else if (rope != null)
		{
			rope.Reset();
		}
		BigMothAbstractAI bigMothAbstractAI = base.abstractCreature.abstractAI as BigMothAbstractAI;
		if (bigMothAbstractAI.holdBaby != null)
		{
			if (bigMothAbstractAI.holdBaby.realizedCreature == null || bigMothAbstractAI.holdBaby.realizedCreature.slatedForDeletetion || bigMothAbstractAI.holdBaby.realizedCreature.room != room)
			{
				if (bigMothAbstractAI.holdBaby.realizedCreature != null)
				{
					bigMothAbstractAI.holdBaby.realizedCreature.RemoveFromRoom();
				}
				bigMothAbstractAI.holdBaby.LoseAllStuckObjects();
				bigMothAbstractAI.holdBaby.pos = base.abstractCreature.pos;
				bigMothAbstractAI.holdBaby.RealizeInRoom();
				Grab(bigMothAbstractAI.holdBaby.realizedObject, 0, UnityEngine.Random.Range(0, bigMothAbstractAI.holdBaby.realizedObject.bodyChunks.Length), Grasp.Shareability.NonExclusive, 1f, overrideEquallyDominant: true, pacifying: false);
			}
			if (bigMothAbstractAI.holdBaby.realizedObject != null && (base.grasps[0] == null || base.grasps[0].grabbed == null || base.grasps[0].discontinued || base.grasps[0].grabbed.slatedForDeletetion || base.grasps[0].grabbed != bigMothAbstractAI.holdBaby.realizedObject))
			{
				for (int j = 0; j < bigMothAbstractAI.holdBaby.realizedObject.bodyChunks.Length; j++)
				{
					bigMothAbstractAI.holdBaby.realizedObject.bodyChunks[j].pos = legs[0, 0].footPos;
					bigMothAbstractAI.holdBaby.realizedObject.bodyChunks[j].vel = base.bodyChunks[3].vel;
				}
				AI.TryGrab(bigMothAbstractAI.holdBaby.realizedObject, frontLegsOnly: false, forceOthersToLetGo: true, forceAllGrabOnGrip: true);
			}
			bigMothAbstractAI.forceRealHoldTime--;
			if (bigMothAbstractAI.forceRealHoldTime <= 0)
			{
				bigMothAbstractAI.holdBaby = null;
			}
		}
		if (wantToFlyCounter > 0)
		{
			wantToFlyCounter--;
		}
		wantToFly = wantToFlyCounter > 0;
		for (int k = 0; k < legs.GetLength(0); k++)
		{
			for (int l = 0; l < legs.GetLength(1); l++)
			{
				legs[k, l].Update();
			}
		}
		for (int m = 0; m < wingPairs.Length; m++)
		{
			wingPairs[m].Update();
		}
		if (base.Consious)
		{
			Act();
		}
		else
		{
			WeightedPush(0, 1, Custom.DirVec(base.bodyChunks[2].pos, base.bodyChunks[1].pos), 3f);
		}
	}

	public void GrabUpdate()
	{
		if (base.grasps[0].grabbed is Creature creature)
		{
			if (creature.enteringShortCut.HasValue || CritGrabbedToMe(creature))
			{
				ReleaseGrasp(0);
				return;
			}
			if (creature is Player player && (double)UnityEngine.Random.value < 0.03)
			{
				player.standing = false;
			}
		}
		if (!room.VisualContact(base.grasps[0].grabbedChunk.pos, base.bodyChunks[2].pos) && !room.VisualContact(base.grasps[0].grabbedChunk.pos, base.bodyChunks[1].pos))
		{
			holdThroughWallCounter++;
			if (holdThroughWallCounter > 70)
			{
				ReleaseGrasp(0);
				return;
			}
		}
		else
		{
			holdThroughWallCounter = Mathf.Max(holdThroughWallCounter - 1, 0);
		}
		float num = 0f;
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			for (int j = 0; j < legs.GetLength(1); j++)
			{
				if (legs[i, j].grabbedChunk != null)
				{
					num += 1f;
				}
			}
		}
		num /= (float)(legs.GetLength(0) * legs.GetLength(1));
		if (base.grasps[0].grabbed is MothGrub)
		{
			num = Mathf.Min(num * 2f, 1f);
		}
		BodyChunk bodyChunk = base.bodyChunks[2];
		BodyChunk grabbedChunk = base.grasps[0].grabbedChunk;
		float num2 = grabbedChunk.mass * ((base.grasps[0].grabbed is MothGrub) ? 0.35f : 1f);
		float num3 = num2 / (bodyChunk.mass * ((!Small) ? 1f : ((room.PixelHeight < base.firstChunk.pos.y) ? 2f : 1.4f)) + num2);
		if (!Small && (rope == null || rope.room != room))
		{
			rope = new Rope(room, bodyChunk.pos, grabbedChunk.pos, 1f);
		}
		Vector2 vector = (grabbedChunk.vel - bodyChunk.vel) * (0.2f * num);
		if (!(base.grasps[0].grabbed is Fly))
		{
			bodyChunk.vel += vector * num3;
			grabbedChunk.vel -= vector * (1f - num3);
		}
		float num4 = Vector2.Distance(bodyChunk.pos, grabbedChunk.pos);
		Vector2 vector2 = Custom.DirVec(bodyChunk.pos, grabbedChunk.pos);
		Vector2 vector3 = vector2;
		if (rope != null)
		{
			rope.Update(bodyChunk.pos, grabbedChunk.pos);
			if (rope.bends.Count > 0)
			{
				num4 = Vector2.Distance(bodyChunk.pos, rope.AConnect) + Vector2.Distance(rope.BConnect, grabbedChunk.pos);
				for (int k = 1; k < rope.bends.Count; k++)
				{
					num4 += Vector2.Distance(rope.bends[k - 1].pos, rope.bends[k].pos);
				}
				vector2 = Custom.DirVec(bodyChunk.pos, rope.bends[0].pos);
				vector3 = Custom.DirVec(rope.bends[rope.bends.Count - 1].pos, grabbedChunk.pos);
			}
		}
		float num5 = 100f * scale * (Small ? 0f : 1f) * ((base.grasps[0].grabbed is MothGrub) ? 0.8f : 1f);
		if (num4 > 170f * scale * (Small ? 3f : 1f))
		{
			ReleaseGrasp(0);
		}
		else if (!(num4 < num5) || !AI.SmallWantsToEat(base.grasps[0].grabbed))
		{
			float num6 = (num4 - num5) * 0.25f * num;
			Vector2 vector4 = vector2 * (num6 * num3);
			Vector2 vector5 = vector3 * (num6 * (1f - num3));
			bodyChunk.pos += vector4;
			bodyChunk.vel += vector4;
			grabbedChunk.pos -= vector5;
			grabbedChunk.vel -= vector5;
		}
	}

	public bool CritGrabbedToMe(Creature crit)
	{
		for (int i = 0; i < grabbedBy.Count; i++)
		{
			if (grabbedBy[i].grabber == crit)
			{
				return true;
			}
		}
		return false;
	}

	public void DrinkChunk()
	{
		drinkingChunk = true;
		tongueHold = 1f;
		room.PlaySound((drinkChunk.owner is Player) ? SoundID.Leech_Attatch_Player : SoundID.Leech_Attatch_NPC, base.firstChunk.pos, 2f, 1f);
	}

	public void ReleaseTongue()
	{
		drinkChunk = null;
		drinkingChunk = false;
		eatCounter = 0;
	}

	public void Act()
	{
		AI.Update();
		if (drinkChunk != null && base.graphicsModule == null)
		{
			drinkingChunk = true;
		}
		float num = 1f;
		Vector2 pos = base.bodyChunks[2].pos;
		if (Small)
		{
			pos -= new Vector2(0f, 7f);
		}
		if (narrowUpcoming && Small)
		{
			pos = base.bodyChunks[1].pos;
		}
		narrowUpcoming = false;
		MovementConnection followingConnection = (AI.pathFinder as BigMothPather).FollowPath(room.GetWorldCoordinate(pos), actuallyFollowingThisPath: true);
		if (room == null)
		{
			return;
		}
		if (followingConnection == default(MovementConnection))
		{
			for (int i = 0; i < 4; i++)
			{
				followingConnection = (AI.pathFinder as BigMothPather).FollowPath(room.GetWorldCoordinate(pos + Custom.fourDirections[i].ToVector2() * 20f), actuallyFollowingThisPath: true);
				if (room == null)
				{
					return;
				}
				if (followingConnection != default(MovementConnection))
				{
					break;
				}
			}
		}
		if (base.safariControlled && (followingConnection == default(MovementConnection) || !AllowableControlledAIOverride(followingConnection.type)))
		{
			SafariControls(ref followingConnection, pos);
		}
		if (followingConnection != default(MovementConnection) && (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation))
		{
			enteringShortCut = followingConnection.StartTile;
			if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				NPCTransportationDestination = followingConnection.destinationCoord;
			}
			return;
		}
		if (followingConnection != default(MovementConnection))
		{
			Vector2 vector = room.MiddleOfTile(followingConnection.destinationCoord);
			int num2 = 1;
			MovementConnection movementConnection = followingConnection;
			if (!base.safariControlled)
			{
				for (int j = 0; j < 3; j++)
				{
					if (!narrowUpcoming && ((room.GetTile(movementConnection.destinationCoord + new IntVector2(0, 1)).Solid && room.GetTile(movementConnection.destinationCoord + new IntVector2(0, -1)).Solid) || (room.GetTile(movementConnection.destinationCoord + new IntVector2(1, 0)).Solid && room.GetTile(movementConnection.destinationCoord + new IntVector2(-1, 0)).Solid)))
					{
						narrowUpcoming = true;
					}
					movementConnection = (AI.pathFinder as BigMothPather).FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
					if (movementConnection == default(MovementConnection) || !room.IsPositionInsideBoundries(movementConnection.DestTile))
					{
						break;
					}
					vector += room.MiddleOfTile(movementConnection.destinationCoord);
					num2++;
				}
			}
			vector /= (float)num2;
			if (!Small)
			{
				int num3 = 0;
				for (int k = 0; k < 5 && !room.GetTile(vector + Vector2.up * (20f * (float)(1 + k))).Solid; k++)
				{
					num3++;
				}
				num = (float)(num3 - 1) / 4f;
			}
			else if (narrowUpcoming)
			{
				num = 0f;
			}
			Vector2 vector2 = vector - pos;
			if (!base.safariControlled && wantToFly && room.VisualContact(pos, room.MiddleOfTile(AI.pathFinder.GetDestination)))
			{
				vector2 = vector2 * 0.3f + Vector2.ClampMagnitude(room.MiddleOfTile(AI.pathFinder.GetDestination) - pos, 20f) * 0.7f;
			}
			vector2 = Vector2.ClampMagnitude(vector2, 10f) / 10f;
			if (!wantToFly)
			{
				vector2.y = Mathf.Clamp(vector.y - pos.y, -10f, 10f) / 10f;
			}
			moveDirection = Vector2.MoveTowards(moveDirection, vector2, 0.1f);
			if (!room.IsPositionInsideBoundries(room.GetTilePosition(vector)))
			{
				moveDirection = vector2;
			}
			if (flyUpness < vector2.y)
			{
				flyUpness = Mathf.Lerp(flyUpness, vector2.y, 0.5f);
			}
			else
			{
				flyUpness = Mathf.Lerp(flyUpness, 0.15f, 0.015f);
			}
			if (AI.behavior == BigMothAI.Behavior.GetUnstuck)
			{
				float num4 = Mathf.Min(1f - (float)stuckCounter / 70f, 0f) + 1f;
				for (int l = 1; l < 3; l++)
				{
					base.bodyChunks[l].pos += vector2 * num4;
					base.bodyChunks[l].vel += vector2 * num4;
				}
				stuckCounter++;
				if (base.graphicsModule != null)
				{
					(base.graphicsModule as BigMothGraphics).blinkCounter = 5;
				}
			}
			else
			{
				stuckCounter = 0;
			}
		}
		Walk();
		Fly();
		if (randomLookCounter == 0)
		{
			if ((double)UnityEngine.Random.value < 0.02)
			{
				randomLookAtCreature = (double)UnityEngine.Random.value < 0.7;
				randomLookCounter = UnityEngine.Random.Range(20, 400);
				randomLookPos = new Vector2((float)(room.TileWidth * 20) * UnityEngine.Random.value, (float)(room.TileHeight * 20) * UnityEngine.Random.value);
			}
		}
		else
		{
			randomLookCounter--;
		}
		bool flag = AI.behavior != BigMothAI.Behavior.Idle;
		Vector2 a = base.bodyChunks[1].pos + moveDirection * 300f;
		if (randomLookCounter > 0)
		{
			a = randomLookPos;
			if (randomLookAtCreature)
			{
				flag = true;
			}
		}
		if (flag && AI.creatureLooker.lookCreature != null)
		{
			a = ((!AI.creatureLooker.lookCreature.VisualContact) ? room.MiddleOfTile(AI.creatureLooker.lookCreature.BestGuessForPosition()) : AI.creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos);
		}
		if (drinkChunk != null)
		{
			a = base.bodyChunks[1].pos + (drinkChunk.pos - base.bodyChunks[1].pos) * 10f;
		}
		a = Vector2.Lerp(a, base.bodyChunks[1].pos, ((base.bodyChunks[1].pos.x - a.x) * flip > 0f) ? 0.5f : 0f);
		lookPos = Vector2.MoveTowards(lookPos, a, Mathf.Max(Mathf.Min(Vector2.Distance(lookPos, a) * 0.1f, 200f), 5f));
		num *= 1f - Mathf.Max(runSpeed - 0.5f, 0f) * 0.8f;
		if (num > stance)
		{
			stance = Mathf.Lerp(stance, num, 0.02f);
		}
		else
		{
			stance = Mathf.Lerp(stance, num, 0.08f);
		}
		stance = Mathf.Clamp01(stance);
		flipness = Mathf.Lerp(flipness, moveDirection.x, 0.02f * Mathf.Abs(moveDirection.x));
		flipness = Mathf.Clamp(flipness, -1f, 1f);
		if ((double)flipness > 0.01)
		{
			flipSide = 1;
		}
		else if ((double)flipness < -0.01)
		{
			flipSide = -1;
		}
		flip = Mathf.Clamp(flipness, (flipSide == 1) ? 0.01f : (-1f), (flipSide == -1) ? (-0.01f) : 1f);
		Vector2 v = Vector2.up;
		if (legsOnGround > 0)
		{
			float num5 = base.bodyChunks[2].pos.x + flip * 57f * (0.5f + 0.5f * scale);
			v = Custom.DirVec(feetAvgPos, new Vector2(num5, base.bodyChunks[2].pos.y));
			float num6 = Mathf.Clamp01(Mathf.Abs(feetAvgPos.x - num5) / (70f * (0.5f + 0.5f * scale)));
			v = (Vector2.up * (1f - num6) + v * num6).normalized;
		}
		float num7 = 0.6f * stance;
		Vector2 normalized = new Vector2((1f - num7) * flip, num7 + (1f - Mathf.Abs(flip)) * 0.1f).normalized;
		groundAngle = Mathf.LerpAngle(groundAngle, Custom.VecToDeg(v), 0.1f);
		Vector2 vector3 = Custom.DegToVec(groundAngle + Custom.VecToDeg(normalized));
		Vector2 normalized2 = (vector3 + Vector2.ClampMagnitude((lookPos - base.bodyChunks[1].pos) / 700f, 1f) * (0.7f + Mathf.Clamp01(Vector2.Dot(vector3, Custom.DirVec(base.bodyChunks[1].pos, lookPos))) * 2f)).normalized;
		WeightedPush(0, 1, normalized2, 3f);
		WeightedPush(1, 2, vector3, 3f);
		WeightedPush(2, 3, vector3, 2f);
		WeightedPush(3, 4, vector3, 2f);
		shake = Custom.LerpAndTick(shake, 0f, 0.008f, 0.005f);
		if (!((double)shake > 0.01))
		{
			return;
		}
		for (int m = 0; m < base.bodyChunks.Length; m++)
		{
			base.bodyChunks[m].vel += UnityEngine.Random.insideUnitCircle * (shake * 3f * scale);
		}
		if (base.graphicsModule == null)
		{
			return;
		}
		BigMothGraphics bigMothGraphics = base.graphicsModule as BigMothGraphics;
		for (int n = 0; n < bigMothGraphics.antennas.Length; n++)
		{
			Vector2 vector4 = UnityEngine.Random.insideUnitCircle * (shake * 5f * scale);
			for (int num8 = 0; num8 < bigMothGraphics.antennas[n].segments.GetLength(0); num8++)
			{
				float num9 = Mathf.Pow(Mathf.InverseLerp(0f, bigMothGraphics.antennas[n].segments.GetLength(0) - 1, num8), 2f);
				bigMothGraphics.antennas[n].segments[num8, 2] += vector4 * (num9 + 0.1f);
			}
		}
	}

	public void Fly()
	{
		if (wantToFly && wingPairs[0].mode != MothWingPair.Mode.Flap)
		{
			wingPairs[0].mode = MothWingPair.Mode.ReadyFlap;
		}
		if (wingPairs[0].mode != MothWingPair.Mode.Glide && wingPairs[1].mode != MothWingPair.Mode.Flap)
		{
			wingPairs[1].mode = MothWingPair.Mode.ReadyFlap;
		}
	}

	public void Walk()
	{
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			for (int j = 0; j < legs.GetLength(1); j++)
			{
				if (!legs[i, j].stepPos.HasValue)
				{
					legs[i, j].SetIdealStepPos(legs[i, j].connectionPos + new Vector2(UnityEngine.Random.value * 2f - 1f, -1f).normalized * legs[i, j].maxReach);
				}
				else if (legs[i, j].stepPos.Value.y > base.bodyChunks[2].pos.y && (double)UnityEngine.Random.value < 0.1)
				{
					legs[i, j].stepPos = null;
				}
			}
		}
		int num = 0;
		feetAvgPos = Vector2.zero;
		legsOnGround = 0;
		for (int k = 0; k < legs.GetLength(0); k++)
		{
			for (int l = 0; l < legs.GetLength(1); l++)
			{
				if (legs[k, l].stepPos.HasValue)
				{
					if (Vector2.Distance(legs[k, l].connectionPos, legs[k, l].stepPos.Value) > legs[k, l].maxReach * 0.8f)
					{
						num++;
					}
					feetAvgPos += legs[k, l].stepPos.Value;
					legsOnGround++;
				}
			}
		}
		feetAvgPos /= (float)legsOnGround;
		if ((!base.safariControlled || !safariWalkMode) && num + (legs.GetLength(0) * legs.GetLength(1) - legsOnGround) > 2 && !drinkingChunk)
		{
			wantToFlyCounter = 7;
		}
		if (legs[0, 0].stepPos.HasValue && legs[0, 1].stepPos.HasValue)
		{
			float x = base.bodyChunks[legs[0, 0].connectedChunk].pos.x;
			int num2 = 0;
			if (legs[0, 0].stepPos.Value.x > legs[0, 1].stepPos.Value.x)
			{
				num2 = 1;
			}
			int num3 = ((num2 == 0) ? 1 : 0);
			int num4 = 0;
			int num5 = -1;
			if (legs[0, num2].stepPos.Value.x > x)
			{
				num4 = -1;
				num5 = num3;
			}
			else if (legs[0, num3].stepPos.Value.x < x)
			{
				num4 = 1;
				num5 = num2;
			}
			if (num5 >= 0)
			{
				legs[0, num5].SetIdealStepPos(new Vector2(base.bodyChunks[legs[0, 0].connectedChunk].pos.x + (float)num4 * legs[0, 0].stepRadius * (0.5f + Mathf.Max(runSpeed - 0.5f, 0f) * 0.5f), feetAvgPos.y));
			}
		}
		if (legsOnGround >= 2)
		{
			for (int m = 0; m < base.bodyChunks.Length; m++)
			{
				float num6 = 1.5f;
				switch (m)
				{
				case 3:
					num6 *= 0.7f;
					break;
				case 4:
					num6 *= 0.4f;
					break;
				}
				base.bodyChunks[m].vel.y += (base.gravity - base.buoyancy * base.bodyChunks[m].submersion) * num6;
			}
			Vector2 vector = moveDirection * (0.8f * runSpeed * (0.5f + 0.5f * scale));
			for (int n = 1; n < 3; n++)
			{
				base.bodyChunks[n].vel *= 0.8f;
				base.bodyChunks[n].pos += vector;
				base.bodyChunks[n].vel += moveDirection * (0.8f * runSpeed * (0.5f + 0.5f * scale) * ((narrowUpcoming && n == 1) ? 5f : 1f));
			}
		}
		else if (!base.safariControlled || !safariWalkMode)
		{
			wantToFlyCounter = 14;
		}
	}

	public void LegSecuredFoot(int pair, int leg)
	{
		room.PlaySound(WatcherEnums.WatcherSoundID.Moth_Step, base.mainBodyChunk);
		if (pair == 0 && legs[pair, leg].allowStepToTriggerOther)
		{
			int num = ((legs[pair, leg].footPos.x >= legs[pair, leg].lastFootPos.x) ? 1 : (-1));
			legs[1, leg].SetIdealStepPos(new Vector2(legs[pair, leg].footPos.x + legs[pair, leg].stepRadius * (float)num * 2f * (0.5f + Mathf.Max(runSpeed - 0.5f, 0f) * 0.5f), legs[pair, leg].footPos.y));
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		for (int i = 0; i < legs.GetLength(0); i++)
		{
			for (int j = 0; j < legs.GetLength(1); j++)
			{
				legs[i, j].Reset();
			}
		}
		if (rope != null)
		{
			rope.Reset();
		}
		wooshSound = new StaticSoundLoop(SoundID.Vulture_Wing_Woosh_LOOP, base.bodyChunks[1].pos, room, 1f, 1f);
	}

	public void SafariControls(ref MovementConnection followingConnection, Vector2 pathFromPos)
	{
		MovementConnection.MovementType movementType = MovementConnection.MovementType.Standard;
		followingConnection = default(MovementConnection);
		if (Small && room.GetTile(pathFromPos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
		{
			movementType = MovementConnection.MovementType.ShortCut;
		}
		Player.InputPackage? inputPackage = inputWithDiagonals;
		if (!inputPackage.HasValue)
		{
			return;
		}
		Player.InputPackage valueOrDefault = inputPackage.GetValueOrDefault();
		safariWalkMode = !valueOrDefault.jmp && movementType == MovementConnection.MovementType.Standard;
		_ = Vector2.zero;
		if (valueOrDefault.analogueDir.magnitude > 0f)
		{
			_ = valueOrDefault.analogueDir.normalized;
		}
		else if (valueOrDefault.x != 0 || valueOrDefault.y != 0)
		{
			_ = new Vector2(valueOrDefault.x, valueOrDefault.y).normalized;
		}
		IntVector2 intVector = new IntVector2(valueOrDefault.x, valueOrDefault.y);
		IntVector2 pos = room.GetTilePosition(pathFromPos);
		int num = 1;
		if (valueOrDefault.AnyDirectionalInput)
		{
			IntVector2 tilePosition = room.GetTilePosition(pathFromPos);
			for (int i = 0; i < 3; i++)
			{
				if (!narrowUpcoming && ((room.GetTile(tilePosition + new IntVector2(0, 1)).Solid && room.GetTile(tilePosition + new IntVector2(0, -1)).Solid) || (room.GetTile(tilePosition + new IntVector2(1, 0)).Solid && room.GetTile(tilePosition + new IntVector2(-1, 0)).Solid)))
				{
					narrowUpcoming = true;
				}
				tilePosition += intVector;
				if (room.GetTile(tilePosition).Solid)
				{
					break;
				}
				pos = tilePosition;
				num += Mathf.Abs(intVector.x) + Mathf.Abs(intVector.y);
			}
		}
		if (safariWalkMode)
		{
			if (intVector.y == 0 && room.aimap.getAItile(pos).floorAltitude < 100000)
			{
				int num2 = Mathf.RoundToInt(5f * scale);
				if (Small)
				{
					num2 = 1;
				}
				if (!narrowUpcoming)
				{
					pos.y += num2 - room.aimap.getAItile(pos).floorAltitude;
				}
			}
		}
		else
		{
			wantToFlyCounter = 7;
		}
		followingConnection = new MovementConnection(movementType, room.GetWorldCoordinate(pathFromPos), room.GetWorldCoordinate(pos), num);
		if (inputWithDiagonals.Value.thrw && inputWithDiagonals.Value.x == 0)
		{
			_ = inputWithDiagonals.Value.y;
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		float num = otherObject.bodyChunks[otherChunk].mass / (base.bodyChunks[myChunk].mass + otherObject.bodyChunks[otherChunk].mass);
		Vector2 vector = (otherObject.bodyChunks[otherChunk].vel - base.bodyChunks[myChunk].vel) * 0.1f;
		base.bodyChunks[myChunk].vel += vector * num;
		otherObject.bodyChunks[otherChunk].vel -= vector * (1f - num);
		if (myChunk == 0 && base.graphicsModule != null)
		{
			(base.graphicsModule as BigMothGraphics).blinkCounter = 7;
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (chunk == 0 && base.graphicsModule != null)
		{
			(base.graphicsModule as BigMothGraphics).blinkCounter = 7;
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * ((-1.5f + (float)i * 0.25f) * 15f);
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 2f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		float num = 1f;
		switch (hitChunk.index)
		{
		case 0:
			num = 2f;
			break;
		case 3:
			num = 0.7f;
			break;
		case 4:
			num = 0.3f;
			break;
		}
		damage *= num;
		stunBonus *= num;
		base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
	}

	public override void Stun(int st)
	{
		base.Stun(st);
		if (base.Stunned)
		{
			LoseAllGrasps();
		}
	}
}
