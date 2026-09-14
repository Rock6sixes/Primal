using System;
using RWCustom;
using UnityEngine;
using Watcher;

public class GraffitiCloud : CosmeticSprite
{
	public float life;

	public float lastLife;

	public float lifeTime;

	public float rotation;

	public float lastRotation;

	public float rotVel;

	public Vector2 initialPos;

	public Vector2 getToPos;

	public float rad;

	public Color color;

	public int checkInsectsDelay;

	public int rippleLayer;

	public GraffitiSplat splat;

	public AbstractCreature thrownBy;

	public GraffitiCloud(Vector2 pos, Vector2 vel, Color color, float size, int checkInsectsDelay, GraffitiSplat splat, AbstractCreature thrownBy, int rippleLayer)
	{
		this.checkInsectsDelay = checkInsectsDelay;
		this.rippleLayer = rippleLayer;
		life = size;
		lastLife = size;
		initialPos = pos;
		lastPos = pos;
		base.vel = vel;
		this.color = color;
		this.splat = splat;
		this.thrownBy = thrownBy;
		getToPos = pos + new Vector2(Mathf.Lerp(-80f, 80f, UnityEngine.Random.value), Mathf.Lerp(-100f, 400f, UnityEngine.Random.value));
		base.pos = pos + vel.normalized * (60f * UnityEngine.Random.value);
		rad = Mathf.Lerp(0.6f, 1.5f, UnityEngine.Random.value) * size;
		rotation = UnityEngine.Random.value * 360f;
		lastRotation = rotation;
		rotVel = Mathf.Lerp(-6f, 6f, UnityEngine.Random.value);
		lifeTime = Mathf.Lerp(150f, 500f, UnityEngine.Random.value);
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
		if (splat != null)
		{
			if (pos.x < splat.rect.xMin)
			{
				splat.rect.xMin = pos.x;
			}
			if (pos.x > splat.rect.xMax)
			{
				splat.rect.xMax = pos.x;
			}
			if (pos.y < splat.rect.yMin)
			{
				splat.rect.yMin = pos.y;
			}
			if (pos.y > splat.rect.yMax)
			{
				splat.rect.yMax = pos.y;
			}
		}
		if (checkInsectsDelay > -1)
		{
			checkInsectsDelay--;
			if (checkInsectsDelay < 1)
			{
				checkInsectsDelay = 20;
				float num = (float)GraffitiBomb.repelLocustsDuration / 20f;
				float num2 = Mathf.InverseLerp(lifeTime, lifeTime - 10f, life * lifeTime);
				num = Custom.LerpMap(Vector2.Distance(pos, initialPos), 25f, 100f, num, num * Mathf.Lerp(0.9f, 0.1f, num2), 3f);
				num *= Mathf.Lerp(num2, 4f, 1f);
				num *= Custom.LerpMap(life * lifeTime, lifeTime / 3f * 2f, lifeTime / 3f, 1f, 0f, 0.5f);
				for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
				{
					Creature realizedCreature = room.abstractRoom.creatures[i].realizedCreature;
					if (realizedCreature == null || (room.abstractRoom.creatures[i].rippleLayer != rippleLayer && !room.abstractRoom.creatures[i].rippleBothSides))
					{
						continue;
					}
					for (int j = 0; j < realizedCreature.bodyChunks.Length; j++)
					{
						if (!Custom.DistLess(pos, realizedCreature.bodyChunks[j].pos, rad + realizedCreature.bodyChunks[j].rad + 20f))
						{
							continue;
						}
						realizedCreature.AddMud((int)num, GraffitiBomb.repelLocustsDuration, color);
						if (realizedCreature is Lizard)
						{
							if (realizedCreature.Template.type == CreatureTemplate.Type.WhiteLizard)
							{
								IrritateCreature(realizedCreature, 0f - num);
							}
							else if (realizedCreature.Template.type == WatcherEnums.CreatureTemplateType.BasiliskLizard)
							{
								IrritateCreature(realizedCreature, (0f - num) * 0.5f);
							}
							else if (realizedCreature.Template.type == WatcherEnums.CreatureTemplateType.IndigoLizard)
							{
								float value = Vector3.Distance(Custom.RGB2Vec3((realizedCreature as Lizard).effectColor), Custom.RGB2Vec3(color));
								value = Mathf.Pow(Mathf.InverseLerp(0.15f, 0.65f, value), 0.5f);
								IrritateCreature(realizedCreature, Mathf.Lerp(num * 0.5f, 0f - num, value));
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
			}
		}
		base.Update(eu);
	}

	public void IrritateCreature(Creature crit, float amount)
	{
		if (crit.State.socialMemory != null && thrownBy != null)
		{
			SocialMemory.Relationship orInitiateRelationship = crit.State.socialMemory.GetOrInitiateRelationship(thrownBy.ID);
			float num = Mathf.Clamp01(Vector3.Distance(Custom.RGB2Vec3(crit.mudColor), Custom.RGB2Vec3(color))) * 0.3f;
			amount = Mathf.Lerp(t: Mathf.Clamp01((float)(crit.muddy / GraffitiBomb.repelLocustsDuration) - num), a: amount / 1000f, b: 0f);
			if (crit.abstractCreature.abstractAI.RealAI.friendTracker?.friend == thrownBy.realizedCreature && amount < 0f)
			{
				amount /= 10f;
			}
			orInitiateRelationship.InfluenceTempLike(amount);
			orInitiateRelationship.InfluenceLike(amount * 0.25f);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["FireSmoke"];
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
		sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Max(0f, num), 1.5f);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = Color.Lerp(color, palette.texture.GetPixel(11, 4), palette.darkness * 0.8f);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
