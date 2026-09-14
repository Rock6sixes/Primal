using System;
using System.Collections.Generic;
using RWCustom;
using Smoke;
using UnityEngine;

public class GraffitiBomb : Weapon
{
	public class AbstractGraffitiBomb : AbstractConsumable
	{
		public Color color;

		public AbstractGraffitiBomb(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData)
			: base(world, AbstractObjectType.GraffitiBomb, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData)
		{
			color = GenerateColorFromSeed();
		}

		public AbstractGraffitiBomb(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData, Color color)
			: base(world, AbstractObjectType.GraffitiBomb, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData)
		{
			this.color = color;
		}

		public Color GenerateColorFromSeed()
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(ID.RandomSeed);
			float[,] array = new float[5, 2]
			{
				{ 5f, 260f },
				{ 4f, 80f },
				{ 3f, 200f },
				{ 2f, 320f },
				{ 1f, 140f }
			};
			float num = 250f;
			float num2 = UnityEngine.Random.Range(0f, 15f);
			float num3 = 0.5f;
			for (int i = 0; i < 5; i++)
			{
				float num4 = array[i, 0];
				if (num2 < num4)
				{
					num3 = (num4 - num2) / num4;
					num = array[i, 1];
					break;
				}
				num2 -= num4;
			}
			num = ((!(num3 < 0.5f)) ? (num + Mathf.Lerp(0f, 30f, 1f - Mathf.Pow(Mathf.Sin(num3 * (float)Math.PI), 2f))) : (num - Mathf.Lerp(30f, 0f, Mathf.Pow(Mathf.Sin(num3 * (float)Math.PI), 2f))));
			float a = Mathf.Lerp(0.6f, 0.7f, Mathf.DeltaAngle(num, 260f) / 180f);
			HSLColor hSLColor = new HSLColor(num / 360f, 1f, Mathf.Lerp(a, 0.8f, UnityEngine.Random.value));
			UnityEngine.Random.state = state;
			return hSLColor.rgb;
		}
	}

	public GraffitiSmoke smoke;

	public float beingEaten;

	public bool lastModeThrown;

	public float swallowed;

	public int dotsStart = 3;

	public Vector2[] dots;

	public float[] segments = new float[4] { -10f, -9f, 9f, 10f };

	public static int repelLocustsDuration = 800;

	public Color sporeColor => AbstrConsumable.color;

	public AbstractGraffitiBomb AbstrConsumable => abstractPhysicalObject as AbstractGraffitiBomb;

	public GraffitiBomb(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 7f, 0.11f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.95f;
		base.gravity = 0.86f;
		bounce = 0.2f;
		surfaceFriction = 0.3f;
		collisionLayer = 2;
		base.waterFriction = 0.95f;
		base.buoyancy = 1.8f;
		exitThrownModeSpeed = 8f;
		tailPos = base.firstChunk.pos;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		dots = new Vector2[UnityEngine.Random.Range(5, 8)];
		List<Vector2> list = new List<Vector2>();
		int num = UnityEngine.Random.Range(8, 12);
		float num2 = 18f / ((float)num / 2f);
		float num3 = UnityEngine.Random.Range(0f, num2);
		float num4 = UnityEngine.Random.Range(0f, 360f);
		for (int i = 0; i < num; i++)
		{
			if (num3 > 18f)
			{
				break;
			}
			if (num4 >= 350f)
			{
				num4 -= 360f;
			}
			if (num4 <= 190f)
			{
				num4 = Custom.LerpMap(num4, -10f, 190f, 0f, 180f);
				list.Add(new Vector2(Mathf.Cos(num4 * (float)Math.PI / 180f), (num3 - 9f) / 18f));
			}
			if (i % 2 == 0)
			{
				num4 = (num4 + UnityEngine.Random.Range(170f, 180f)) % 360f;
				num3 += UnityEngine.Random.Range(-1f, 1f);
			}
			else
			{
				num4 = (num4 + UnityEngine.Random.Range(70f, 90f)) % 360f;
				num3 += num2 + UnityEngine.Random.Range(-1f, 1f);
			}
		}
		dots = list.ToArray();
		UnityEngine.Random.state = state;
		float a = 1f;
		float num5 = -1f;
		float a2 = 1f;
		float num6 = -1f;
		for (int j = 0; j < dots.Length; j++)
		{
			a = Mathf.Min(a, dots[j].x);
			num5 = Mathf.Max(num5, dots[j].x);
			a2 = Mathf.Min(a2, dots[j].y);
			num6 = Mathf.Max(num6, dots[j].y);
		}
		for (int k = 0; k < dots.Length; k++)
		{
			dots[k].x = -1f + 2f * Mathf.InverseLerp(a, num5, dots[k].x);
			dots[k].y = -1f + 2f * Mathf.InverseLerp(a2, num6, dots[k].y);
		}
		float num7 = 0f;
		for (int l = 0; l < dots.Length; l++)
		{
			num7 = Mathf.Max(num7, dots[l].magnitude);
		}
		for (int m = 0; m < dots.Length; m++)
		{
			dots[m] /= num7;
		}
	}

	public override void Update(bool eu)
	{
		if (beingEaten > 0f)
		{
			beingEaten += 0.1f;
			if (beingEaten > 1f)
			{
				Destroy();
			}
		}
		if (lastModeThrown && (base.firstChunk.ContactPoint.x != 0 || base.firstChunk.ContactPoint.y != 0))
		{
			Explode();
		}
		lastModeThrown = base.mode == Mode.Thrown;
		if (base.firstChunk.ContactPoint.y != 0)
		{
			rotationSpeed = (rotationSpeed * 2f + base.firstChunk.vel.x * 5f) / 3f;
		}
		if (smoke != null)
		{
			if (room.ViewedByAnyCamera(base.firstChunk.pos, 300f) && !base.slatedForDeletetion)
			{
				if (base.mode == Mode.Carried || base.mode == Mode.Free)
				{
					smoke.EmitSmoke(base.firstChunk.pos + rotation.normalized * 8f + Custom.PerpendicularVector(rotation).normalized * UnityEngine.Random.Range(-2f, 2f), Custom.DegToVec(Custom.VecToDeg(rotation) + UnityEngine.Random.Range(-20f, 20f)) * UnityEngine.Random.Range(0.4f, 0.8f), sporeColor, UnityEngine.Random.Range(120, 160), UnityEngine.Random.value > 0.5f);
				}
				else if (base.mode == Mode.Thrown)
				{
					smoke.EmitSmoke(base.firstChunk.pos + rotation.normalized * 8f, Custom.DegToVec(Custom.VecToDeg(rotation) + UnityEngine.Random.Range(-10f, 10f)) * 0.1f, sporeColor, UnityEngine.Random.Range(20, 50), connect: true);
				}
			}
			if (smoke.slatedForDeletetion || smoke.room != room)
			{
				smoke = null;
			}
		}
		else
		{
			smoke = new GraffitiSmoke(room);
			room.AddObject(smoke);
		}
		if (!AbstrConsumable.isConsumed && grabbedBy.Count > 0)
		{
			AbstrConsumable.Consume();
		}
		bool flag = false;
		if (base.mode == Mode.Carried && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).swallowAndRegurgitateCounter > 50 && (grabbedBy[0].grabber as Player).objectInStomach == null && (grabbedBy[0].grabber as Player).input[0].pckp)
		{
			int num = -1;
			for (int i = 0; i < 2; i++)
			{
				if ((grabbedBy[0].grabber as Player).grasps[i] != null && (grabbedBy[0].grabber as Player).CanBeSwallowed((grabbedBy[0].grabber as Player).grasps[i].grabbed))
				{
					num = i;
					break;
				}
			}
			if (num > -1 && (grabbedBy[0].grabber as Player).grasps[num] != null && (grabbedBy[0].grabber as Player).grasps[num].grabbed == this)
			{
				flag = true;
			}
		}
		swallowed = Custom.LerpAndTick(swallowed, flag ? 1f : 0f, 0.05f, 0.05f);
		base.Update(eu);
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		while (abstractPhysicalObject.pos.y >= 0 && !placeRoom.GetTile(abstractPhysicalObject.pos.Tile + new IntVector2(0, -1)).Solid)
		{
			abstractPhysicalObject.pos.y--;
		}
		base.PlaceInRoom(placeRoom);
		rotation = Custom.DegToVec(Mathf.Lerp(45f, 135f, abstractPhysicalObject.world.game.SeededRandom(abstractPhysicalObject.ID.RandomSeed)) - 90f);
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.chunk == null)
		{
			return false;
		}
		if (result.chunk.owner.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !result.chunk.owner.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides)
		{
			return false;
		}
		result.chunk.vel += base.firstChunk.vel * 0.1f / result.chunk.mass;
		base.HitSomething(result, eu);
		Explode();
		return true;
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		room?.PlaySound(SoundID.Slugcat_Throw_Puffball, base.firstChunk);
	}

	public override void PickedUp(Creature upPicker)
	{
		room.PlaySound(SoundID.Slugcat_Pick_Up_Puffball, base.firstChunk);
	}

	public override void HitWall()
	{
		Explode();
		SetRandomSpin();
		ChangeMode(Mode.Free);
		base.forbiddenToPlayer = 10;
	}

	public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
	{
		base.HitByExplosion(hitFac, explosion, hitChunk);
		Explode();
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		Explode();
	}

	public void Explode()
	{
		if (!base.slatedForDeletetion)
		{
			Vector2 vector = new Vector2(50f, 50f);
			GraffitiSplat graffitiSplat = new GraffitiSplat(sporeColor, base.firstChunk.pos, new Rect(base.firstChunk.pos - vector, vector * 2f));
			room.AddObject(graffitiSplat);
			for (int i = 0; i < 80; i++)
			{
				room.AddObject(new GraffitiCloud(base.firstChunk.pos, Custom.RNV() * (UnityEngine.Random.value * 10f), sporeColor, 1f, i % 20, graffitiSplat, thrownBy?.abstractCreature, abstractPhysicalObject.rippleLayer));
			}
			room.PlaySound(SoundID.Puffball_Eplode, base.firstChunk);
			Destroy();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[dotsStart + dots.Length * 2];
		sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.Length - 1, pointyTip: false, customColor: true);
		sLeaser.sprites[1] = TriangleMesh.MakeLongMesh(segments.Length - 1, pointyTip: false, customColor: true);
		sLeaser.sprites[1].color = Color.white;
		sLeaser.sprites[1].alpha = 0.2f;
		sLeaser.sprites[2] = TriangleMesh.MakeLongMesh(segments.Length - 1, pointyTip: false, customColor: true);
		sLeaser.sprites[2].color = Color.black;
		sLeaser.sprites[2].alpha = 0.2f;
		for (int i = 0; i < dots.Length; i++)
		{
			sLeaser.sprites[dotsStart + i] = new FSprite("JetFishEyeB");
			sLeaser.sprites[dotsStart + dots.Length + i] = new FSprite("pixel");
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		float degAng = Custom.VecToDeg(vector2);
		if (vibrate > 0)
		{
			vector += Custom.DegToVec(UnityEngine.Random.value * 360f) * (2f * UnityEngine.Random.value);
		}
		float num = 1f;
		if (beingEaten > 0f || swallowed > 0f)
		{
			num = 1f - Mathf.Max(beingEaten, swallowed * 0.5f);
		}
		TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
		TriangleMesh triangleMesh2 = sLeaser.sprites[1] as TriangleMesh;
		TriangleMesh triangleMesh3 = sLeaser.sprites[2] as TriangleMesh;
		for (int i = 0; i < segments.Length - 1; i++)
		{
			Vector2 vector3 = vector - camPos + segments[i] * num * vector2;
			float num2 = i switch
			{
				0 => 4f, 
				1 => 5f, 
				2 => 4f, 
				_ => 5f, 
			} * num;
			triangleMesh.MoveVertice(i * 4, vector3 + Custom.PerpendicularVector(vector2).normalized * num2);
			triangleMesh.MoveVertice(i * 4 + 1, vector3 - Custom.PerpendicularVector(vector2).normalized * num2);
			triangleMesh2.MoveVertice(i * 4, vector3 + Custom.PerpendicularVector(vector2).normalized * (num2 - 1f));
			triangleMesh2.MoveVertice(i * 4 + 1, vector3 + Custom.PerpendicularVector(vector2).normalized * (num2 - 3f));
			triangleMesh3.MoveVertice(i * 4, vector3 - Custom.PerpendicularVector(vector2).normalized * (num2 - 1f));
			triangleMesh3.MoveVertice(i * 4 + 1, vector3 - Custom.PerpendicularVector(vector2).normalized * num2);
			vector3 = vector - camPos + vector2 * segments[i + 1];
			triangleMesh.MoveVertice(i * 4 + 2, vector3 + Custom.PerpendicularVector(vector2).normalized * num2);
			triangleMesh.MoveVertice(i * 4 + 3, vector3 - Custom.PerpendicularVector(vector2).normalized * num2);
			triangleMesh2.MoveVertice(i * 4 + 2, vector3 + Custom.PerpendicularVector(vector2).normalized * (num2 - 1f));
			triangleMesh2.MoveVertice(i * 4 + 3, vector3 + Custom.PerpendicularVector(vector2).normalized * (num2 - 3f));
			triangleMesh3.MoveVertice(i * 4 + 2, vector3 - Custom.PerpendicularVector(vector2).normalized * (num2 - 1f));
			triangleMesh3.MoveVertice(i * 4 + 3, vector3 - Custom.PerpendicularVector(vector2).normalized * num2);
		}
		if (blink > 0)
		{
			bool flag = blink > 1 && UnityEngine.Random.value < 0.5f;
			sLeaser.sprites[0].color = (flag ? base.blinkColor : color);
		}
		else if (sLeaser.sprites[0].color != color)
		{
			sLeaser.sprites[0].color = color;
		}
		for (int j = 0; j < dots.Length; j++)
		{
			Vector2 vector4 = vector + Custom.RotateAroundOrigo(new Vector2(dots[j].x * 4.5f, dots[j].y * 9.5f) * num, degAng);
			sLeaser.sprites[dotsStart + j].x = vector4.x - camPos.x;
			sLeaser.sprites[dotsStart + j].y = vector4.y - camPos.y;
			sLeaser.sprites[dotsStart + j].rotation = Custom.VecToDeg(Custom.RotateAroundOrigo(dots[j], degAng).normalized);
			sLeaser.sprites[dotsStart + j].scaleX = num;
			sLeaser.sprites[dotsStart + j].scaleY = Custom.LerpMap(Mathf.Abs(dots[j].x), 0f, 1f, 1f, 0.25f, 4f);
			sLeaser.sprites[dotsStart + dots.Length + j].x = vector4.x - camPos.x;
			sLeaser.sprites[dotsStart + dots.Length + j].y = vector4.y - camPos.y;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.color = Color.Lerp(new Color(0.5f, 0.45f, 0.25f), palette.texture.GetPixel(11, 4), 0.4f);
		sLeaser.sprites[0].color = base.color;
		Color color = Color.Lerp(base.color, sporeColor, 0.5f);
		for (int i = 0; i < dots.Length; i++)
		{
			sLeaser.sprites[dotsStart + i].color = color;
			sLeaser.sprites[dotsStart + dots.Length + i].color = sporeColor;
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			if (i != 1 && i != 2)
			{
				newContatiner.AddChild(sLeaser.sprites[i]);
			}
		}
		newContatiner.AddChild(sLeaser.sprites[1]);
		newContatiner.AddChild(sLeaser.sprites[2]);
	}
}
