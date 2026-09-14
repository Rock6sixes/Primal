using System;
using RWCustom;
using UnityEngine;

public class SporeCloud : CosmeticSprite
{
	public float life;

	public float lastLife;

	public float lifeTime;

	public float rotation;

	public float lastRotation;

	public float rotVel;

	public Vector2 getToPos;

	public float rad;

	public Color color;

	public int checkInsectsDelay;

	public AbstractCreature killTag;

	public InsectCoordinator smallInsects;

	public int rippleLayer;

	public bool nonToxic;

	public SporeCloud(Vector2 pos, Vector2 vel, Color color, float size, AbstractCreature killTag, int checkInsectsDelay, InsectCoordinator smallInsects)
		: this(pos, vel, color, size, killTag, checkInsectsDelay, smallInsects, 0)
	{
	}

	public SporeCloud(Vector2 pos, Vector2 vel, Color color, float size, AbstractCreature killTag, int checkInsectsDelay, InsectCoordinator smallInsects, int rippleLayer)
	{
		this.checkInsectsDelay = checkInsectsDelay;
		this.smallInsects = smallInsects;
		this.rippleLayer = rippleLayer;
		life = size;
		lastLife = size;
		lastPos = pos;
		base.vel = vel;
		this.color = color;
		this.killTag = killTag;
		getToPos = pos + new Vector2(Mathf.Lerp(-50f, 50f, UnityEngine.Random.value), Mathf.Lerp(-100f, 400f, UnityEngine.Random.value));
		base.pos = pos + vel.normalized * (60f * UnityEngine.Random.value);
		rad = Mathf.Lerp(0.6f, 1.5f, UnityEngine.Random.value) * size;
		rotation = UnityEngine.Random.value * 360f;
		lastRotation = rotation;
		rotVel = Mathf.Lerp(-6f, 6f, UnityEngine.Random.value);
		lifeTime = Mathf.Lerp(170f, 400f, UnityEngine.Random.value);
	}

	public override void Update(bool eu)
	{
		vel *= 0.9f;
		vel += Custom.DirVec(pos, getToPos) * (UnityEngine.Random.value * 0.04f);
		lastRotation = rotation;
		rotation += rotVel * vel.magnitude;
		lastLife = life;
		life -= 1f / lifeTime;
		if (room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
		{
			IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
			FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
			pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
			if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
			{
				vel.x = Mathf.Abs(vel.x);
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
			{
				vel.x = 0f - Mathf.Abs(vel.x);
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
			{
				vel.y = Mathf.Abs(vel.y);
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
			{
				vel.y = 0f - Mathf.Abs(vel.y);
			}
		}
		if (lastLife <= 0f)
		{
			Destroy();
		}
		if (!nonToxic && checkInsectsDelay > -1)
		{
			checkInsectsDelay--;
			if (checkInsectsDelay < 1)
			{
				checkInsectsDelay = 20;
				for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
				{
					Creature realizedCreature = room.abstractRoom.creatures[i].realizedCreature;
					if (realizedCreature == null || (room.abstractRoom.creatures[i].rippleLayer != rippleLayer && !room.abstractRoom.creatures[i].rippleBothSides))
					{
						continue;
					}
					for (int j = 0; j < realizedCreature.bodyChunks.Length; j++)
					{
						if (Custom.DistLess(pos, realizedCreature.bodyChunks[j].pos, rad + realizedCreature.bodyChunks[j].rad + 20f))
						{
							realizedCreature.repelLocusts = Mathf.Max(realizedCreature.repelLocusts, PuffBall.repelLocustsDuration);
							if (realizedCreature is Player player)
							{
								player.sporeParticleTicks = Mathf.Max(player.sporeParticleTicks, PuffBall.repelLocustsDuration - 80);
							}
						}
					}
					if (realizedCreature.Template.type == CreatureTemplate.Type.Fly || realizedCreature.Template.type == CreatureTemplate.Type.Spider)
					{
						if (Custom.DistLess(pos, realizedCreature.mainBodyChunk.pos, rad + realizedCreature.mainBodyChunk.rad + 20f))
						{
							if (UnityEngine.Random.value < life)
							{
								realizedCreature.Die();
							}
							else
							{
								realizedCreature.Stun(UnityEngine.Random.Range(10, 120));
							}
						}
					}
					else
					{
						if (!(realizedCreature is InsectoidCreature insectoidCreature))
						{
							continue;
						}
						for (int k = 0; k < realizedCreature.bodyChunks.Length; k++)
						{
							if (!Custom.DistLess(pos, realizedCreature.bodyChunks[k].pos, rad + realizedCreature.bodyChunks[k].rad + 20f))
							{
								continue;
							}
							insectoidCreature.poison += 0.025f * Mathf.Pow(life, 4f) / Mathf.Lerp(realizedCreature.TotalMass, 1f, 0.15f);
							if (insectoidCreature.poison >= 1f)
							{
								if (insectoidCreature.State is HealthState healthState)
								{
									healthState.health -= 0.0125f * Mathf.Pow(life, 4f) / Mathf.Lerp(realizedCreature.TotalMass, 1f, 0.15f);
								}
								else
								{
									realizedCreature.Die();
								}
							}
							realizedCreature.SetKillTag(killTag);
							realizedCreature.Stun(Mathf.RoundToInt(20f * UnityEngine.Random.value * life / Mathf.Lerp(realizedCreature.TotalMass, 1f, 0.15f)));
						}
					}
				}
				for (int l = 0; l < room.physicalObjects.Length; l++)
				{
					for (int m = 0; m < room.physicalObjects[l].Count; m++)
					{
						if (room.physicalObjects[l][m].abstractPhysicalObject.rippleLayer == rippleLayer || room.physicalObjects[l][m].abstractPhysicalObject.rippleBothSides)
						{
							if (room.physicalObjects[l][m] is SporePlant)
							{
								(room.physicalObjects[l][m] as SporePlant).PuffBallSpores(pos, rad);
							}
							else if (room.physicalObjects[l][m] is SporePlant.AttachedBee && Custom.DistLess(pos, room.physicalObjects[l][m].firstChunk.pos, rad + 20f))
							{
								(room.physicalObjects[l][m] as SporePlant.AttachedBee).life -= 0.5f;
							}
						}
					}
				}
				foreach (UpdatableAndDeletable update in room.updateList)
				{
					if (update is LocustSystem locustSystem)
					{
						locustSystem.KillInRadius(pos, rad + 70f);
						locustSystem.AddAvoidancePoint(pos, rad + 200f, 20);
					}
				}
				if (smallInsects != null)
				{
					for (int n = 0; n < smallInsects.allInsects.Count; n++)
					{
						if (Custom.DistLess(smallInsects.allInsects[n].pos, pos, rad + 70f))
						{
							smallInsects.allInsects[n].alive = false;
						}
					}
				}
			}
		}
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Spores"];
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[0].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
		float num = Mathf.Lerp(lastLife, life, timeStacker);
		sLeaser.sprites[0].scale = 7f * rad * ((num > 0.5f) ? Custom.LerpMap(num, 1f, 0.5f, 0.5f, 1f) : Mathf.Sin(num * (float)Math.PI));
		sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastLife, life, timeStacker)), 1.2f);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = color;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
