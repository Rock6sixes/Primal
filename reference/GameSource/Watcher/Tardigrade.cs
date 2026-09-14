using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class Tardigrade : Creature, IPlayerEdible
{
	public struct IndividualVariations
	{
		public Color bodyColor;

		public Color secondaryColor;

		public int spikesInRow;

		public float spikeWidth;

		public float spikeLength;

		public float spikeLayBack;

		public float spikePuffOut;

		public float earWidth;

		public float earLength;

		public IndividualVariations(Color bodyColor, Color secondaryColor, int spikesInRow, float spikeWidth, float spikeLength, float spikeLayBack, float spikePuffOut, float earWidth, float earLength)
		{
			this.bodyColor = bodyColor;
			this.secondaryColor = secondaryColor;
			this.spikesInRow = spikesInRow;
			this.spikeWidth = spikeWidth;
			this.spikeLength = spikeLength;
			this.spikeLayBack = spikeLayBack;
			this.spikePuffOut = spikePuffOut;
			this.earWidth = earWidth;
			this.earLength = earLength;
		}
	}

	public class TardigradeState : HealthState
	{
		public int bites;

		public float juice;

		public bool slayer;

		public bool dried;

		public TardigradeState(AbstractCreature creature)
			: base(creature)
		{
			bites = 3;
			juice = 1.7f;
		}

		public override string ToString()
		{
			return HealthBaseSaveString() + (slayer ? "<cB>SLAYER" : "") + (dried ? "<cB>DRIED" : "");
		}

		public override void LoadFromString(string[] s)
		{
			base.LoadFromString(s);
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < s.Length; i++)
			{
				switch (Regex.Split(s[i], "<cC>")[0])
				{
				case "SLAYER":
					flag = true;
					slayer = true;
					break;
				case "DRIED":
					flag2 = true;
					dried = true;
					break;
				}
			}
			if (flag)
			{
				unrecognizedSaveStrings.Remove("SLAYER");
			}
			if (flag2)
			{
				unrecognizedSaveStrings.Remove("DRIED");
			}
		}
	}

	public TardigradeAI AI;

	public float runSpeed;

	public Vector2? posOnTrail;

	public int timeOffground;

	public float flip;

	public float walkBob;

	public Vector2[] bodyTrail;

	public MovementConnection lastFollowedConnection;

	public float scale = 1f;

	public float dryness;

	public float lastDryness;

	public IndividualVariations iVars;

	public bool Dried
	{
		get
		{
			return (base.State as TardigradeState).dried;
		}
		set
		{
			(base.State as TardigradeState).dried = value;
		}
	}

	public bool IsSlayer => (base.State as TardigradeState).slayer;

	public int BitesLeft => (base.State as TardigradeState).bites;

	public int FoodPoints
	{
		get
		{
			if (!IsSlayer)
			{
				return 2;
			}
			return 4;
		}
	}

	public bool Edible => !Dried;

	public bool AutomaticPickUp => true;

	public Tardigrade(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		GenerateIVars();
		float num = 0.4f * scale;
		base.bodyChunks = new BodyChunk[4];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 8f * scale, num / 4f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 8f * scale, num / 4f);
		base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 8f * scale, num / 4f);
		base.bodyChunks[3] = new BodyChunk(this, 3, new Vector2(0f, 0f), 8f * scale, num / 4f);
		bodyChunkConnections = new BodyChunkConnection[5];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 12f * scale, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[2], 12f * scale, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[2] = new BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[3], 12f * scale, BodyChunkConnection.Type.Normal, 1f, -1f);
		bodyChunkConnections[3] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[2], 24f * scale, BodyChunkConnection.Type.Push, 0.7f, -1f);
		bodyChunkConnections[4] = new BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[3], 24f * scale, BodyChunkConnection.Type.Push, 0.7f, -1f);
		bodyTrail = new Vector2[base.bodyChunks.Length];
		base.airFriction = 0.99f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.35f;
		collisionLayer = 1;
		base.waterFriction = 0.9f;
		base.buoyancy = 0.5f;
		currentAffectiveness = 0.4f;
		if (Dried)
		{
			dryness = 1f;
		}
		flip = Random.value * 2f - 1f;
	}

	public void GenerateIVars()
	{
		Random.State state = Random.state;
		Random.InitState(base.abstractCreature.ID.RandomSeed);
		scale *= 0.55f + ((base.abstractCreature.personality.dominance - Mathf.Pow(Random.value, 2f)) * 0.6f + (1f - Mathf.Pow(Random.value, 2f)) * 0.4f) * 0.15f;
		Color rgb = new HSLColor(0.3f + Random.value * 0.6f, 0.3f + Random.value * 0.17f, 0.5f + Random.value * 0.3f).rgb;
		Color rgb2 = new HSLColor(Random.value, 0.75f + Random.value * 0.25f, 0.5f + Random.value * 0.2f).rgb;
		iVars = new IndividualVariations(rgb, rgb2, Random.Range(2, 7), Random.value, Random.value, Random.value * 0.7f + 0.3f, Random.value * 0.8f, Random.value, Random.value);
		if (IsSlayer)
		{
			scale = 0.87f;
			iVars.bodyColor = new Color(0f, 0.549f, 0.929f);
			iVars.secondaryColor = new Color(0.91f, 0.95f, 0.64f);
			iVars.spikesInRow = 5;
			iVars.spikeWidth = 0.3f;
			iVars.spikeLength = 0.7f;
			iVars.spikeLayBack = 0.7f;
			iVars.spikePuffOut = 0.9f;
			iVars.earWidth = 0.5f;
			iVars.earLength = 0.3f;
		}
		Random.state = state;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new TardigradeGraphics(this);
		}
		base.graphicsModule.Reset();
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		for (int i = 0; i < bodyTrail.Length; i++)
		{
			bodyTrail[i] = base.bodyChunks[i].pos;
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = newRoom.MiddleOfTile(pos);
		Vector2 vector2 = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = vector + vector2 * ((1f - (float)i / (float)base.bodyChunks.Length) * 30f * scale);
			bodyTrail[i] = base.bodyChunks[i].pos;
		}
		posOnTrail = base.bodyChunks[0].pos;
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
		bool flag = room.roomSettings != null && room.roomSettings.DangerType == WatcherEnums.WatcherDangerType.Sandstorm;
		bool flag2 = base.Submersion > 0.01f;
		if (Dried)
		{
			if (flag2)
			{
				if (Random.value < 0.1f + dryness * 0.4f)
				{
					room.AddObject(new Bubble(base.bodyChunks[Random.Range(0, base.bodyChunks.Length)].pos, Custom.RNV() * (Mathf.Pow(Random.value, 2.1f) * 2.5f), bottomBubble: false, fakeWaterBubble: false, Random.value > 0.5f));
				}
				dryness = Custom.LerpAndTick(dryness, 0f, 0.01f, 0.002f);
				if (dryness <= 0f)
				{
					Dried = false;
				}
			}
			else
			{
				dryness = Custom.LerpAndTick(dryness, 1f, 0.025f, 0.005f);
			}
		}
		else if (flag && !flag2)
		{
			dryness = Custom.LerpAndTick(dryness, 1f, 0.008f, 0.0015f);
			if (dryness >= 1f)
			{
				Dried = true;
			}
		}
		else
		{
			dryness = Custom.LerpAndTick(dryness, 0f, 0.025f, 0.005f);
		}
		if (dryness > 0.01f)
		{
			if (dryness > 0.8f)
			{
				Stun(20);
			}
			float num = dryness;
			Vector2 p = (base.bodyChunks[0].pos + base.bodyChunks[1].pos) * 0.5f;
			Vector2 vector = Custom.DirVec((base.bodyChunks[3].pos + base.bodyChunks[2].pos) * 0.5f, p);
			Vector2 vector2 = (base.bodyChunks[0].pos + base.bodyChunks[3].pos) * 0.5f;
			Vector2 vector3 = (vector2 + vector * 2f - base.bodyChunks[0].pos) * (0.25f * num);
			base.bodyChunks[0].pos += vector3;
			base.bodyChunks[0].vel += vector3;
			base.bodyChunks[3].pos -= vector3;
			base.bodyChunks[3].vel -= vector3;
			Vector2 vector4 = (base.bodyChunks[1].pos + base.bodyChunks[2].pos) * 0.5f;
			vector3 = (vector4 + vector * 1f - base.bodyChunks[1].pos) * (0.25f * num);
			base.bodyChunks[1].pos += vector3;
			base.bodyChunks[1].vel += vector3;
			base.bodyChunks[2].pos -= vector3;
			base.bodyChunks[2].vel -= vector3;
			vector3 = (vector4 - vector2) * (0.25f * num);
			base.bodyChunks[0].pos += vector3;
			base.bodyChunks[0].vel += vector3;
			base.bodyChunks[3].pos += vector3;
			base.bodyChunks[3].vel += vector3;
			base.bodyChunks[1].pos -= vector3;
			base.bodyChunks[1].vel -= vector3;
			base.bodyChunks[2].pos -= vector3;
			base.bodyChunks[2].vel -= vector3;
		}
		if (lastDryness != dryness)
		{
			lastDryness = dryness;
			for (int i = 0; i < base.bodyChunks.Length; i++)
			{
				base.bodyChunks[i].mass = 0.4f * scale / 4f * (1f - 0.7f * dryness);
			}
			for (int j = 0; j < bodyChunkConnections.Length; j++)
			{
				bodyChunkConnections[j].active = dryness <= 0.95f;
			}
		}
		timeOffground++;
		if (grabbedBy.Count > 0)
		{
			Stun(12);
		}
		Vector2? vector5 = null;
		for (int k = 0; k < bodyTrail.Length; k++)
		{
			if (room.aimap.TileAccessibleToCreature(bodyTrail[k], base.Template))
			{
				timeOffground = 0;
				if (!vector5.HasValue)
				{
					vector5 = bodyTrail[k];
				}
			}
		}
		if (timeOffground <= 5)
		{
			if (!posOnTrail.HasValue && vector5.HasValue && base.Consious)
			{
				posOnTrail = vector5.Value;
			}
		}
		else
		{
			posOnTrail = null;
		}
		if (posOnTrail.HasValue && Vector2.Distance(base.bodyChunks[0].pos, posOnTrail.Value) > 27f * scale * (room.PointSubmerged(base.firstChunk.pos) ? 1.5f : 1f))
		{
			posOnTrail = null;
		}
		if (base.Consious)
		{
			Act();
		}
	}

	public override void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
	{
		if (dryness > 0.9f)
		{
			float num = dryness;
			Vector2 p = (base.bodyChunks[0].pos + base.bodyChunks[1].pos) * 0.5f;
			Vector2 vector = Custom.DirVec((base.bodyChunks[3].pos + base.bodyChunks[2].pos) * 0.5f, p);
			Vector2 vector2 = (base.bodyChunks[0].pos + base.bodyChunks[3].pos) * 0.5f;
			Vector2 vector3 = (vector2 + vector * 2f - base.bodyChunks[0].pos) * (0.5f * num);
			base.bodyChunks[0].pos += vector3;
			base.bodyChunks[3].pos -= vector3;
			Vector2 vector4 = (base.bodyChunks[1].pos + base.bodyChunks[2].pos) * 0.5f;
			vector3 = (vector4 + vector - base.bodyChunks[1].pos) * (0.5f * num);
			base.bodyChunks[1].pos += vector3;
			base.bodyChunks[2].pos -= vector3;
			vector3 = (vector4 - vector2) * (0.5f * num);
			base.bodyChunks[0].pos += vector3;
			base.bodyChunks[3].pos += vector3;
			base.bodyChunks[1].pos -= vector3;
			base.bodyChunks[2].pos -= vector3;
		}
		base.GraphicsModuleUpdated(actuallyViewed, eu);
	}

	public void Act()
	{
		AI.Update();
		bodyTrail[0] = (posOnTrail.HasValue ? posOnTrail.Value : base.bodyChunks[0].pos);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			float num = (float)i / ((float)base.bodyChunks.Length - 1f);
			Vector2 vector;
			if (i > 0)
			{
				vector = Custom.DirVec(bodyTrail[i], bodyTrail[i - 1]);
				float num2 = Vector2.Distance(bodyTrail[i], bodyTrail[i - 1]);
				bodyTrail[i] += vector * (num2 - bodyChunkConnections[i - 1].distance);
			}
			else
			{
				vector = Custom.DirVec(bodyTrail[i + 1], bodyTrail[i]);
			}
			if (posOnTrail.HasValue)
			{
				float submersion = base.firstChunk.submersion;
				Vector2 vector2 = Custom.PerpendicularVector(vector);
				Vector2 vector3 = (bodyTrail[i] + vector2 * ((flip * (room.GetTile(bodyTrail[i]).AnyBeam ? 12f : (-8f + 10f * scale)) + Mathf.Sin(walkBob * 0.3f + num * 7f) * 1f) * scale) - base.bodyChunks[i].pos) * (runSpeed * (1f - dryness * 0.5f));
				base.bodyChunks[i].vel *= 0.9f - 0.2f * submersion;
				base.bodyChunks[i].pos += vector3 * (0.1f + submersion * 0.3f);
				base.bodyChunks[i].vel += vector3 * (0.1f + submersion * 0.3f);
				base.bodyChunks[i].vel.y = base.bodyChunks[i].vel.y + Mathf.Lerp(base.gravity, base.gravity - base.buoyancy, base.bodyChunks[i].submersion);
			}
		}
		Vector2 pos = base.bodyChunks[0].pos;
		if (posOnTrail.HasValue)
		{
			pos = posOnTrail.Value;
		}
		if (room.GetWorldCoordinate(pos) == AI.pathFinder.GetDestination && AI.threatTracker.Utility() < 0.5f)
		{
			base.GoThroughFloors = false;
			return;
		}
		MovementConnection movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(pos), actuallyFollowingThisPath: true);
		if (movementConnection == default(MovementConnection))
		{
			movementConnection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(pos), actuallyFollowingThisPath: true);
		}
		if (base.safariControlled)
		{
			movementConnection = default(MovementConnection);
			if (inputWithDiagonals.HasValue)
			{
				MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					type = MovementConnection.MovementType.ShortCut;
				}
				if (inputWithDiagonals.Value.AnyDirectionalInput)
				{
					movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
				}
			}
		}
		if (movementConnection != default(MovementConnection))
		{
			Run(movementConnection);
			return;
		}
		if (lastFollowedConnection != default(MovementConnection))
		{
			MoveTowards(room.MiddleOfTile(lastFollowedConnection.DestTile));
		}
		base.GoThroughFloors = false;
	}

	public void MoveTowards(Vector2 moveTo)
	{
		if (posOnTrail.HasValue)
		{
			bool num = !AI.pathFinder.DestInRoom || !(room.GetTilePosition(posOnTrail.Value) == AI.pathFinder.GetDestination.Tile);
			Vector2 vector = Vector2.ClampMagnitude(moveTo - posOnTrail.Value, 10f) / 10f;
			if (num)
			{
				posOnTrail += vector * (runSpeed * 1.75f * scale * (1f - dryness * 0.5f));
			}
			walkBob += vector.magnitude * runSpeed * (1f - dryness * 0.5f);
			float num2 = 0f;
			if (IsTileSolid(0, 0, -1) || room.GetTile(base.firstChunk.pos).AnyBeam || IsTileSolid(0, 0, 1))
			{
				num2 = 1f;
			}
			if (IsTileSolid(0, 0, 1) && !IsTileSolid(0, 0, -1))
			{
				num2 *= -1f;
			}
			flip = Mathf.Clamp(flip + vector.x * 0.03f * runSpeed * num2, -1f, 1f);
		}
		base.GoThroughFloors = moveTo.y < base.bodyChunks[0].pos.y - 5f;
	}

	public void Run(MovementConnection followingConnection)
	{
		if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
		{
			enteringShortCut = followingConnection.StartTile;
			if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
			{
				NPCTransportationDestination = followingConnection.destinationCoord;
			}
		}
		else
		{
			Vector2 moveTo = room.MiddleOfTile(followingConnection.DestTile);
			MoveTowards(moveTo);
		}
		lastFollowedConnection = followingConnection;
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
	{
		if (!Dried)
		{
			damage *= 1f / (scale + 0.5f);
			base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
		}
	}

	public override void Die()
	{
		if (!Dried)
		{
			base.Die();
		}
	}

	public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
	{
		if (Dried)
		{
			return false;
		}
		if (source is Spear spear)
		{
			float num = 1f - spear.abstractSpear.poison;
			float juice = (base.State as TardigradeState).juice;
			(base.State as TardigradeState).juice = Mathf.Max((base.State as TardigradeState).juice - num, 0f);
			float num2 = juice - (base.State as TardigradeState).juice;
			if (num2 <= 0f)
			{
				return base.SpearStick(source, dmg, chunk, appPos, direction);
			}
			Vector3 vector = Custom.RGB2HSL(iVars.bodyColor);
			if (spear.abstractSpear.poison <= 0f)
			{
				spear.reinitiateSpritesOnDraw = true;
				spear.abstractSpear.poisonHue = vector.x;
			}
			else
			{
				spear.abstractSpear.poisonHue = HSLColor.Lerp(new HSLColor(vector.x, vector.y, vector.z), new HSLColor(spear.abstractSpear.poisonHue, 1f, 0.5f), spear.abstractSpear.poison * 0.5f * num2).hue;
			}
			spear.abstractSpear.poison += num2;
		}
		return base.SpearStick(source, dmg, chunk, appPos, direction);
	}

	public void BitByPlayer(Grasp grasp, bool eu)
	{
		Creature grabber = grasp.grabber;
		Vector3 vector = Custom.RGB2HSL((BitesLeft == 3) ? iVars.secondaryColor : iVars.bodyColor);
		room.AddObject(new PoisonInjecter(grabber, 0.22f, (10f + Random.value * 8f) * ((BitesLeft == 3) ? 1f : 4.4f), new HSLColor(vector.x, Mathf.Lerp(vector.y, 1f, 0.5f), 0.5f).rgb));
		(base.State as TardigradeState).bites--;
		room.PlaySound((BitesLeft == 0) ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, base.firstChunk);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grabber.mainBodyChunk.pos);
		if (BitesLeft <= 1 && !base.dead)
		{
			Die();
		}
		if (BitesLeft < 1)
		{
			(grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
	}

	public void ThrowByPlayer()
	{
	}
}
