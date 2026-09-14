using RWCustom;
using Smoke;
using UnityEngine;

namespace Watcher;

public class LizardBlizzardModule
{
	public Lizard lizard;

	public BlizzardBeamSmoke breathSmoke;

	public BlizzardShieldSmoke shieldSmoke;

	public float shieldSpinSpeed;

	public bool openShield;

	public int beamTimer;

	public int beamUpBobTime;

	public int targetInViewTime;

	public StaticSoundLoop beamSound;

	public StaticSoundLoop beamHitSound;

	public BlizzardHaloShine haloShine;

	public bool HasBeam
	{
		get
		{
			if (beamTimer > 85)
			{
				return beamTimer < 170;
			}
			return false;
		}
	}

	public LizardBlizzardModule(Lizard lizard)
	{
		this.lizard = lizard;
	}

	public void NewRoom(Room newRoom)
	{
		beamSound = new StaticSoundLoop(WatcherEnums.WatcherSoundID.BlizLiz_Beam_LOOP, lizard.bodyChunks[0].pos, lizard.room, 0f, 1f);
		beamHitSound = new StaticSoundLoop(WatcherEnums.WatcherSoundID.BlizLiz_BeamHit_LOOP, lizard.bodyChunks[0].pos, lizard.room, 0f, 1f);
	}

	public void Update()
	{
		if (lizard.graphicsModule != null && haloShine == null)
		{
			LizardGraphics lizardGraphics = lizard.graphicsModule as LizardGraphics;
			for (int i = 0; i < lizardGraphics.cosmetics.Count; i++)
			{
				if (lizardGraphics.cosmetics[i] is BlizzardHaloShine blizzardHaloShine)
				{
					haloShine = blizzardHaloShine;
					break;
				}
			}
		}
		if (!lizard.Consious && shieldSpinSpeed != 0f && Random.value < 0.04f)
		{
			beamTimer = 900;
		}
		beamUpBobTime++;
		openShield = beamTimer == 0 && !lizard.dead && !lizard.room.abstractRoom.shelter;
		if (beamTimer > 0)
		{
			beamTimer++;
			if (!lizard.Consious || (lizard.AI != null && lizard.AI.preyTracker.MostAttractivePrey == null && !lizard.safariControlled) || lizard.grasps[0] != null)
			{
				beamTimer += 5;
			}
			if (beamTimer >= 1000)
			{
				beamTimer = 0;
			}
		}
		if (openShield)
		{
			if (shieldSpinSpeed == 0f)
			{
				shieldSpinSpeed = (((double)Random.value > 0.5) ? 0.01f : (-0.01f));
			}
			shieldSpinSpeed = Mathf.MoveTowards(shieldSpinSpeed, (shieldSpinSpeed > 0f) ? 1f : (-1f), 0.01f);
			for (int j = 0; j < lizard.room.physicalObjects.Length; j++)
			{
				for (int k = 0; k < lizard.room.physicalObjects[j].Count; k++)
				{
					if (lizard.room.physicalObjects[j][k] == lizard || !lizard.abstractPhysicalObject.IsSameRippleLayer(lizard.room.physicalObjects[j][k].abstractPhysicalObject.rippleLayer))
					{
						continue;
					}
					for (int l = 0; l < lizard.room.physicalObjects[j][k].bodyChunks.Length; l++)
					{
						BodyChunk bodyChunk = lizard.room.physicalObjects[j][k].bodyChunks[l];
						if (Vector2.Distance(lizard.firstChunk.pos, bodyChunk.pos) < 170f)
						{
							Vector2 vector = Custom.DirVec(lizard.firstChunk.pos, bodyChunk.pos) * ((1f - Vector2.Distance(lizard.firstChunk.pos, bodyChunk.pos) / 170f) * (lizard.room.VisualContact(lizard.firstChunk.pos, bodyChunk.pos) ? 2f : 0.5f));
							vector += Custom.PerpendicularVector(vector) * (0.3f * shieldSpinSpeed);
							bodyChunk.vel += Vector2.ClampMagnitude(vector / bodyChunk.mass, 10f);
						}
					}
				}
			}
		}
		else
		{
			shieldSpinSpeed = Mathf.MoveTowards(shieldSpinSpeed, 0f, 0.01f);
		}
		float f = Mathf.Min((float)beamTimer / 85f, 1f);
		if (beamTimer > 85 || beamTimer == 0)
		{
			f = 0f;
		}
		else
		{
			f = Mathf.Pow(f, 3f) * 2f;
			if (Random.value < f * 0.4f)
			{
				Vector2 pos = lizard.firstChunk.pos + Custom.DirVec(lizard.bodyChunks[1].pos, lizard.firstChunk.pos) * 40f;
				lizard.room.AddObject(new ShockWave(pos, 50f + 50f * f, 0.005f + 0.04f * f, 5));
			}
		}
		beamSound.Update();
		beamSound.pitch = 1f;
		beamSound.volume = (HasBeam ? 1f : 0f);
		if (shieldSpinSpeed != 0f)
		{
			beamSound.pos = lizard.firstChunk.pos;
			beamSound.pitch = (HasBeam ? 1f : 0.5f);
			beamSound.volume = (HasBeam ? 1f : (Mathf.Abs(shieldSpinSpeed) * 0.25f));
			for (int m = 0; m < lizard.room.updateList.Count; m++)
			{
				if (!(lizard.room.updateList[m] is CosmeticSprite) || lizard.room.updateList[m] is SmokeSystem || IsForbiddenToPull(lizard.room.updateList[m]))
				{
					continue;
				}
				CosmeticSprite cosmeticSprite = lizard.room.updateList[m] as CosmeticSprite;
				float num = 0.5f;
				Vector2 vector2 = Custom.DirVec(lizard.bodyChunks[1].pos + Random.insideUnitCircle * 20f, cosmeticSprite.pos);
				if (Vector2.Distance(lizard.bodyChunks[1].pos, cosmeticSprite.pos) > 20f)
				{
					float num2 = Vector2.Distance(lizard.bodyChunks[1].pos, cosmeticSprite.pos);
					if (num2 < 300f)
					{
						num *= 1f - num2 / 300f;
						cosmeticSprite.vel += Custom.PerpendicularVector(vector2) * (shieldSpinSpeed * num * 3f);
						cosmeticSprite.vel += vector2 * (-2.5f * num);
					}
				}
			}
		}
		else
		{
			beamSound.pos = lizard.bodyChunks[0].pos;
		}
		if (lizard.graphicsModule != null)
		{
			if (breathSmoke == null)
			{
				breathSmoke = new BlizzardBeamSmoke(lizard.room);
				lizard.room.AddObject(breathSmoke);
			}
			if (shieldSmoke == null)
			{
				shieldSmoke = new BlizzardShieldSmoke(lizard.room);
				lizard.room.AddObject(shieldSmoke);
			}
			Vector2 a = lizard.bodyChunks[0].pos;
			if (HasBeam)
			{
				Vector2 vector3 = RayTraceBeamHitPos();
				a = vector3;
				Vector2 vector4 = Custom.DirVec(lizard.bodyChunks[0].pos, vector3);
				float num3 = Vector2.Distance(lizard.bodyChunks[0].pos, vector3);
				for (int n = 0; n < 5; n++)
				{
					breathSmoke.EmitSmoke(lizard.firstChunk.pos, Custom.DirVec(lizard.bodyChunks[1].pos, lizard.bodyChunks[0].pos) * (30f + 20f * Random.value) + Random.insideUnitCircle * 10f, Random.value < 0.5f, 50f);
				}
				for (int num4 = 0; num4 < 4; num4++)
				{
					breathSmoke.EmitSmoke(vector3, Custom.DirVec(vector3, lizard.bodyChunks[1].pos) * (5f + 20f * Random.value) + Random.insideUnitCircle * 5f, Random.value < 0.5f, 30f);
				}
				for (int num5 = 0; num5 < lizard.room.abstractRoom.creatures.Count; num5++)
				{
					if (lizard.room.abstractRoom.creatures[num5].realizedCreature == null || lizard.room.abstractRoom.creatures[num5].realizedCreature == lizard)
					{
						continue;
					}
					Creature realizedCreature = lizard.room.abstractRoom.creatures[num5].realizedCreature;
					float num6 = 1000f;
					int num7 = -1;
					for (int num8 = 0; num8 < realizedCreature.bodyChunks.Length; num8++)
					{
						float num9 = Vector2.Distance(Custom.ClosestPointOnLineSegment(lizard.bodyChunks[0].pos, vector3, realizedCreature.bodyChunks[num8].pos), realizedCreature.bodyChunks[num8].pos) - realizedCreature.bodyChunks[num8].rad;
						if (num9 < num6)
						{
							num6 = num9;
							num7 = num8;
						}
					}
					if (num7 != -1 && num6 < 10f && Vector2.Distance(lizard.bodyChunks[0].pos, realizedCreature.bodyChunks[num7].pos) < num3)
					{
						if (realizedCreature is Player && Random.value < 0.2f)
						{
							realizedCreature.Violence(lizard.mainBodyChunk, vector4 * 4f, realizedCreature.bodyChunks[num7], null, Creature.DamageType.Explosion, 1.5f, 0f);
						}
						else
						{
							realizedCreature.Violence(lizard.mainBodyChunk, vector4 * 4f, realizedCreature.bodyChunks[num7], null, Creature.DamageType.Electric, 0.1f, 0f);
						}
						for (int num10 = 0; num10 < 2; num10++)
						{
							breathSmoke.EmitSmoke(realizedCreature.bodyChunks[num7].pos, Random.insideUnitCircle * 15f, Random.value < 0.5f, 30f);
						}
						a = realizedCreature.bodyChunks[num7].pos;
					}
				}
				for (int num11 = 0; num11 < lizard.room.updateList.Count; num11++)
				{
					if (lizard.room.updateList[num11] is CosmeticSprite && !(lizard.room.updateList[num11] is SmokeSystem) && !IsForbiddenToPull(lizard.room.updateList[num11]))
					{
						CosmeticSprite cosmeticSprite2 = lizard.room.updateList[num11] as CosmeticSprite;
						float num12 = Vector2.Distance(Custom.ClosestPointOnLineSegment(lizard.bodyChunks[0].pos, vector3, cosmeticSprite2.pos), cosmeticSprite2.pos);
						if (num12 < 100f)
						{
							cosmeticSprite2.vel += vector4 * (5f * (1f - num12 / 100f));
						}
					}
				}
			}
			beamHitSound.Update();
			beamHitSound.pos = Vector2.Lerp(a, lizard.firstChunk.pos, 0.5f);
			beamHitSound.pitch = 1f;
			beamHitSound.volume = (HasBeam ? 1f : 0f);
			if (shieldSpinSpeed != 0f)
			{
				for (int num13 = 0; num13 < 1; num13++)
				{
					shieldSmoke.EmitSmoke(lizard.bodyChunks[Random.Range(0, 3)].pos + Random.insideUnitCircle * 80f, Random.insideUnitCircle.normalized * (3f + Random.value * 2f), this, Random.value < 0.3f, 100f);
				}
			}
		}
		else
		{
			beamHitSound.Update();
			beamHitSound.pos = lizard.bodyChunks[0].pos;
			beamHitSound.volume = 0f;
		}
		if (breathSmoke != null && (breathSmoke.slatedForDeletetion || breathSmoke.room != lizard.room))
		{
			breathSmoke = null;
		}
		if (shieldSmoke != null && (shieldSmoke.slatedForDeletetion || shieldSmoke.room != lizard.room))
		{
			shieldSmoke = null;
		}
		if (lizard.dead)
		{
			openShield = false;
		}
	}

	public void FireBeam()
	{
		beamTimer = 1;
		lizard.room.AddObject(new ShockWave(lizard.firstChunk.pos, 530f, 0.045f, 9));
		lizard.room.PlaySound(WatcherEnums.WatcherSoundID.BlizLiz_Pop, lizard.firstChunk.pos, 0.5f, 1f);
		lizard.room.PlaySound(WatcherEnums.WatcherSoundID.BlizLiz_Charge, lizard.firstChunk.pos, 1f, 1f);
	}

	public bool IsForbiddenToPull(UpdatableAndDeletable uad)
	{
		if (uad is RippleRing || uad is RippleFullScreen || uad is DebugSprite || uad is WarpSpawningRipple || uad is RippleDeathEffect || uad is WarpSpawningRipple.WarpSpawningBubble)
		{
			return true;
		}
		return false;
	}

	public Vector2 RayTraceBeamHitPos()
	{
		Vector2 vector = Custom.DirVec(lizard.bodyChunks[1].pos, lizard.bodyChunks[0].pos);
		Vector2 corner = Custom.RectCollision(lizard.bodyChunks[0].pos, lizard.bodyChunks[0].pos + vector * 100000f, lizard.room.RoomRect).GetCorner(FloatRect.CornerLabel.D);
		IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(lizard.room, lizard.bodyChunks[0].pos, corner);
		if (intVector.HasValue)
		{
			return lizard.room.MiddleOfTile(intVector.Value);
		}
		return vector * 10000f;
	}
}
