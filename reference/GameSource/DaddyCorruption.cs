using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Noise;
using RWCustom;
using UnityEngine;
using Watcher;

public class DaddyCorruption : UpdatableAndDeletable, INotifyWhenRoomIsReady, IDrawable, IReactToNoises
{
	public class EatenCreature
	{
		public int disableCounter;

		public Creature creature;

		public Vector2 goalPos;

		public float progression;

		public float goalPosBulbRad;

		public float wait;

		public EatenCreature(Creature creature, Vector2 goalPos, float goalPosBulbRad)
		{
			this.creature = creature;
			this.goalPos = goalPos;
			progression = 0f;
			wait = 0f;
			this.goalPosBulbRad = goalPosBulbRad;
		}

		public void BulbInteraction(Vector2 newGoalPos, float newGoalPosBulbRad)
		{
			wait += newGoalPosBulbRad;
			if (newGoalPosBulbRad > goalPosBulbRad)
			{
				goalPos = newGoalPos;
				goalPosBulbRad = newGoalPosBulbRad;
			}
			disableCounter = 0;
		}

		public void Update()
		{
			if (wait > 12000f)
			{
				for (int i = 0; i < creature.bodyChunkConnections.Length; i++)
				{
					creature.bodyChunkConnections[i].type = PhysicalObject.BodyChunkConnection.Type.Pull;
				}
				progression += 0.05f;
				for (int j = 0; j < creature.bodyChunks.Length; j++)
				{
					creature.bodyChunks[j].collideWithTerrain = false;
					creature.bodyChunks[j].vel *= 1f - progression;
					creature.bodyChunks[j].pos = Vector2.Lerp(creature.bodyChunks[j].pos, goalPos, progression);
				}
				if (creature.graphicsModule != null && creature.graphicsModule.bodyParts != null)
				{
					for (int k = 0; k < creature.graphicsModule.bodyParts.Length; k++)
					{
						creature.graphicsModule.bodyParts[k].vel *= 1f - progression;
						creature.graphicsModule.bodyParts[k].pos = Vector2.Lerp(creature.graphicsModule.bodyParts[k].pos, goalPos, progression);
					}
				}
				if (progression >= 1f)
				{
					if (ModManager.CoopAvailable && creature is Player)
					{
						(creature as Player).PermaDie();
					}
					creature.Die();
					if (!ModManager.DLCShared || creature.abstractCreature.creatureTemplate.type != DLCSharedEnums.CreatureTemplateType.Inspector)
					{
						creature.Destroy();
					}
				}
			}
			else
			{
				disableCounter++;
			}
		}
	}

	public class Bulb : DaddyGraphics.DaddyBubbleOwner
	{
		public int firstSprite;

		public int totalSprites;

		public IntVector2 tile;

		public Vector2 lastPos;

		public Vector2 pos;

		public Vector2 vel;

		public Vector2 stuckPos;

		public Vector2 eyeStalkPos;

		public DaddyCorruption owner;

		public float corruptionLevelAtMySpot;

		public float rad;

		public float rotation;

		public float eyeRad;

		public bool hasEye = true;

		public bool hasBlackGoo;

		public bool lastVisible;

		public CustomRotData customData;

		public float focus;

		public float lastFocus;

		public float getToFocus;

		public float closed;

		public float lastClosed;

		public int eyesClosedDelay;

		public int feltSomethingDelay;

		public float light;

		public float feltSomethingIntensity;

		public int reactionDelay;

		public int bubblesWait;

		public Vector2 lookDir;

		public Vector2 lastLookDir;

		public Vector2 nextLookDir;

		public Vector2 feltSomethingAt;

		public bool hasHeardSound;

		public Vector2? legReachPos;

		public BodyChunk eatChunk;

		public Color renderCol;

		public Vector2 renderCenterPos;

		public bool bodyInMidground;

		public int dotSprite;

		public bool hasDot;

		public LittleLeg leg;

		public Tendril tendril;

		public int BlackGooSprite
		{
			get
			{
				if (hasEye)
				{
					return firstSprite + 3;
				}
				return firstSprite + 1;
			}
		}

		public Color EyeColor
		{
			get
			{
				if (customData != null)
				{
					if (customData.color == CustomRotData.RotColor.EffectColorA)
					{
						return owner.effectColorA;
					}
					if (customData.color == CustomRotData.RotColor.EffectColorB)
					{
						return owner.effectColorB;
					}
				}
				return owner.eyeColor;
			}
		}

		public Color EffectColor
		{
			get
			{
				if (customData != null)
				{
					if (customData.color == CustomRotData.RotColor.EffectColorA)
					{
						return owner.effectColorA;
					}
					if (customData.color == CustomRotData.RotColor.EffectColorB)
					{
						return owner.effectColorB;
					}
				}
				return owner.effectColor;
			}
		}

		public int EyeSprite(int part)
		{
			return firstSprite + 1 + part;
		}

		public Bulb(DaddyCorruption owner, int spr, bool hasBlackGoo, IntVector2 tile)
		{
			this.owner = owner;
			this.tile = tile;
			this.hasBlackGoo = hasBlackGoo && (!ModManager.MMF || !owner.room.abstractRoom.singleRealizedRoom);
			firstSprite = spr;
			totalSprites = 1;
			lastVisible = true;
			stuckPos = owner.room.MiddleOfTile(tile);
			pos = stuckPos;
			lastPos = pos;
			renderCenterPos = pos;
			for (int i = 0; i < 8; i++)
			{
				if (owner.Occupied(tile + Custom.eightDirections[i]))
				{
					Vector2 b = owner.room.MiddleOfTile(tile + Custom.eightDirections[i]);
					stuckPos = Vector2.Lerp(stuckPos, b, UnityEngine.Random.value * 0.5f);
				}
			}
			nextLookDir = Custom.RNV();
			customData = owner.GetCustomRotData(tile);
			corruptionLevelAtMySpot = owner.CorruptionLevel(stuckPos);
			if (customData != null)
			{
				rad = Mathf.Lerp(customData.minSize, Mathf.Lerp(customData.minSize, customData.maxSize, UnityEngine.Random.Range(0.5f, 1f)), corruptionLevelAtMySpot);
			}
			else
			{
				rad = Mathf.Lerp(4f, 10f + 10f * UnityEngine.Random.value, corruptionLevelAtMySpot);
			}
			if (customData != null)
			{
				if (customData.eyeType == CustomRotData.EyeType.Cross)
				{
					hasEye = UnityEngine.Random.value < corruptionLevelAtMySpot * Mathf.Min(customData.eyeChance * 2f, 1f) + Mathf.Max(customData.eyeChance * 2f - 1f, 0f);
				}
				else
				{
					hasEye = false;
				}
			}
			else
			{
				hasEye = UnityEngine.Random.value < corruptionLevelAtMySpot;
			}
			for (int j = 0; j < 5; j++)
			{
				if (!hasEye)
				{
					break;
				}
				IntVector2 intVector = tile + Custom.fourDirectionsAndZero[j];
				if (intVector.x < owner.bottomLeft.x || intVector.y < owner.bottomLeft.y || intVector.x > owner.topRight.x || intVector.y > owner.topRight.y)
				{
					continue;
				}
				List<Bulb> list = owner.bulbs[intVector.x - owner.bottomLeft.x, intVector.y - owner.bottomLeft.y];
				if (list == null)
				{
					continue;
				}
				foreach (Bulb item in list)
				{
					if (item.hasEye && Custom.DistLess(stuckPos, item.stuckPos, rad + item.rad))
					{
						hasEye = false;
						break;
					}
				}
			}
			stuckPos -= owner.Dir(owner.room.GetTilePosition(stuckPos)) * (20f - rad);
			eyeStalkPos = stuckPos - owner.Dir(tile) * Mathf.Lerp(10f, 50f, UnityEngine.Random.value);
			rotation = UnityEngine.Random.value * 360f;
			if (hasEye)
			{
				totalSprites = 3;
				if (customData != null)
				{
					eyeRad = UnityEngine.Random.Range(customData.minEyeSize, customData.maxEyeSize);
				}
				else
				{
					eyeRad = UnityEngine.Random.value;
				}
			}
			if (this.hasBlackGoo)
			{
				totalSprites++;
			}
			hasDot = owner.bulbs[tile.x - owner.bottomLeft.x, tile.y - owner.bottomLeft.y].Count == 0 && customData?.eyeType == CustomRotData.EyeType.Dot && UnityEngine.Random.value < customData.eyeChance;
			bodyInMidground = !hasEye && !hasDot && UnityEngine.Random.value < 0.5f;
			if (customData?.eyeType == CustomRotData.EyeType.Dot && owner.bulbs[tile.x - owner.bottomLeft.x, tile.y - owner.bottomLeft.y].Count > 0 && owner.bulbs[tile.x - owner.bottomLeft.x, tile.y - owner.bottomLeft.y][0].hasDot)
			{
				bodyInMidground = true;
			}
			if (hasDot)
			{
				dotSprite = firstSprite + totalSprites;
				totalSprites++;
			}
			if ((!ModManager.MMF || !owner.room.abstractRoom.singleRealizedRoom) && UnityEngine.Random.value < 0.2f && UnityEngine.Random.value < owner.CorruptionLevel(stuckPos) && (customData == null || customData.behavior == CustomRotData.Behavior.Hostile))
			{
				leg = new LittleLeg(this, owner, firstSprite + totalSprites, stuckPos, stuckPos + owner.Dir(tile) * Mathf.Lerp(20f, 150f, Mathf.Pow(UnityEngine.Random.value, 0.5f) * owner.CorruptionLevel(stuckPos)));
				totalSprites += leg.graphic.sprites;
				owner.room.AddObject(leg);
			}
			if (customData?.behavior == CustomRotData.Behavior.Passive && UnityEngine.Random.value < customData.legChance && UnityEngine.Random.value < Mathf.Pow(corruptionLevelAtMySpot, 0.5f) && owner.Dir(owner.room.GetTilePosition(stuckPos)).y < 0f && owner.room.GetTile(owner.room.GetTilePosition(stuckPos) - new IntVector2(0, 1)).Terrain == Room.Tile.TerrainType.Air)
			{
				Vector2 spawnPos = stuckPos;
				Vector2 tipPos = stuckPos + Vector2.down * Mathf.Lerp(35f, 65f, Mathf.Pow(UnityEngine.Random.value, 0.75f) * corruptionLevelAtMySpot);
				tendril = new Tendril(this, owner, firstSprite + totalSprites, spawnPos, tipPos);
				totalSprites += tendril.graphic.sprites;
				owner.room.AddObject(tendril);
			}
		}

		public void Update()
		{
			owner.slowedCreatures.Clear();
			lastPos = pos;
			pos += vel;
			vel *= ((customData != null) ? 0.8f : 0.9f);
			vel += lookDir * 0.1f;
			vel -= (pos - stuckPos) / 10f;
			lastClosed = closed;
			if (bubblesWait > 0)
			{
				bubblesWait--;
			}
			if (!Custom.DistLess(pos, stuckPos, rad / 2f))
			{
				vel -= (pos - stuckPos).normalized * (Vector2.Distance(pos, stuckPos) - rad / 2f);
				pos -= (pos - stuckPos).normalized * (Vector2.Distance(pos, stuckPos) - rad / 2f);
			}
			if (eatChunk != null)
			{
				vel += Custom.DirVec(pos, eatChunk.pos) * ((customData?.behavior == CustomRotData.Behavior.Passive) ? 0.2f : 1f);
				if (customData?.behavior == CustomRotData.Behavior.Passive)
				{
					if (eatChunk.owner is Creature item && owner.slowedCreatures.Add(item))
					{
						eatChunk.vel *= new Vector2(1f - 0.4f * customData.slowdown, 1f - 0.2f * customData.slowdown);
					}
				}
				else
				{
					float num = owner.CorruptionLevel(tile);
					Vector2 vector = Custom.DirVec(eatChunk.pos, pos);
					for (int i = 0; i < 8; i++)
					{
						float num2 = owner.CorruptionLevel(tile + Custom.eightDirections[i]);
						if (num2 > num)
						{
							vector = Custom.eightDirections[i].ToVector2().normalized;
							num = num2;
						}
					}
					if ((eatChunk.owner is Player && owner.room.game.IsStorySession && owner.room.game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility) || (eatChunk.owner is Creature && (eatChunk.owner as Creature).abstractCreature.tentacleImmune))
					{
						eatChunk.vel *= new Vector2(0.92f, 0.96f);
					}
					else
					{
						eatChunk.vel *= 0.9f;
						eatChunk.vel += vector * (rad * 0.02f) / eatChunk.mass;
					}
					owner.BulbNibbleAtChunk(this, eatChunk);
					pos += Custom.RNV();
					vel += Custom.RNV() * 0.2f;
					closed = Mathf.Min(1f, closed + 0.1f);
					Vector3 vector2 = eatChunk.pos;
					if (feltSomethingDelay < 1 && UnityEngine.Random.value < 0.025f)
					{
						FeltSomething(UnityEngine.Random.value, vector2);
					}
				}
				if (!Custom.DistLess(eatChunk.pos, pos, rad + eatChunk.rad + ((customData?.behavior == CustomRotData.Behavior.Passive) ? 2f : 10f)) || eatChunk.owner.slatedForDeletetion || eatChunk.owner.room != owner.room)
				{
					eatChunk = null;
				}
				return;
			}
			float num3 = 1f;
			if (ModManager.MMF && owner.room.abstractRoom.singleRealizedRoom)
			{
				num3 = 0.15f;
			}
			if (UnityEngine.Random.value <= num3)
			{
				for (int j = 0; j < owner.room.abstractRoom.creatures.Count; j++)
				{
					if (eatChunk != null)
					{
						break;
					}
					if (owner.room.abstractRoom.creatures[j].realizedCreature == null || owner.room.abstractRoom.creatures[j].realizedCreature is DaddyLongLegs || (ModManager.MMF && owner.room.abstractRoom.creatures[j].realizedCreature is Overseer))
					{
						continue;
					}
					for (int k = 0; k < owner.room.abstractRoom.creatures[j].realizedCreature.bodyChunks.Length; k++)
					{
						if (eatChunk != null)
						{
							break;
						}
						if (Custom.DistLess(owner.room.abstractRoom.creatures[j].realizedCreature.bodyChunks[k].pos, pos, owner.room.abstractRoom.creatures[j].realizedCreature.bodyChunks[k].rad + rad))
						{
							eatChunk = owner.room.abstractRoom.creatures[j].realizedCreature.bodyChunks[k];
							FeltSomething(1f, eatChunk.pos);
						}
					}
				}
			}
			if (leg != null)
			{
				vel += Custom.DirVec(pos, leg.segments[Custom.IntClamp(leg.segments.GetLength(0) / 2, 0, leg.segments.GetLength(0) - 1), 0]);
			}
			closed = Mathf.Max(0f, closed - 0.05f);
			float num4 = 0f;
			num4 = light * Mathf.InverseLerp(0f, 1f, Vector2.Distance(lastLookDir, lookDir));
			light = Mathf.Max(0f, light - 0.05f);
			if (UnityEngine.Random.value < num4)
			{
				getToFocus = Mathf.Max(getToFocus, UnityEngine.Random.value);
			}
			else if (UnityEngine.Random.value < 1f / 70f)
			{
				getToFocus = 0f;
			}
			lastFocus = focus;
			if (focus < getToFocus)
			{
				focus = Mathf.Min(focus + 0.05f, getToFocus);
			}
			else
			{
				focus = Mathf.Max(focus - 0.05f, getToFocus);
			}
			if (UnityEngine.Random.value < 0.01f)
			{
				legReachPos = null;
			}
			lastLookDir = lookDir;
			if (reactionDelay < 1)
			{
				lookDir = nextLookDir;
				if (hasHeardSound)
				{
					light = Mathf.Max(light, UnityEngine.Random.value);
					legReachPos = pos + lookDir * Mathf.Lerp(100f, 200f, UnityEngine.Random.value);
				}
				hasHeardSound = false;
				reactionDelay = UnityEngine.Random.Range(10, 20);
			}
			else
			{
				reactionDelay--;
			}
			if (UnityEngine.Random.value < 0.00125f)
			{
				nextLookDir = Custom.RNV();
			}
			if (feltSomethingDelay > 0)
			{
				feltSomethingDelay--;
			}
			if (feltSomethingIntensity > 0f)
			{
				TransmitFeel();
				feltSomethingIntensity = 0f;
			}
		}

		public void HeardNoise(Vector2 noisePos)
		{
			if (owner.GWmode || (ModManager.MMF && owner.room.abstractRoom.singleRealizedRoom) || customData?.behavior == CustomRotData.Behavior.Passive)
			{
				return;
			}
			nextLookDir = Custom.DirVec(stuckPos, noisePos);
			hasHeardSound = true;
			if (bubblesWait < 1 && UnityEngine.Random.value < Custom.LerpMap(rad, 3f, 16f, 0.0125f, 0.125f) && owner.room.VisualContact(pos + Custom.DirVec(pos, noisePos) * 20f, pos + Custom.DirVec(pos, noisePos) * 120f))
			{
				float num = (Mathf.Pow(UnityEngine.Random.value, 2f) + Mathf.InverseLerp(3f, 15f, rad)) / 2f;
				for (int i = 0; i < (int)Mathf.Lerp(2f, 9f, num); i++)
				{
					owner.room.AddObject(new DaddyBubble(this, Custom.DirVec(pos, noisePos) * 6f / (1f + (float)i * 0.2f), num, UnityEngine.Random.value, 0f));
				}
				if (UnityEngine.Random.value < 1f / 3f)
				{
					owner.room.AddObject(new DaddyRipple(this, noisePos, default(Vector2), num, EyeColor));
				}
				bubblesWait = UnityEngine.Random.Range(40, 140);
			}
		}

		public void FeltSomething(float intensity, Vector2 feltAtPos)
		{
			if (feltSomethingDelay <= 0 && !(intensity <= 0f) && !(customData?.behavior == CustomRotData.Behavior.Passive))
			{
				feltSomethingIntensity = intensity;
				feltSomethingAt = feltAtPos;
				lookDir = Custom.DirVec(stuckPos, feltAtPos);
				feltSomethingDelay = UnityEngine.Random.Range(20, 40);
				if (!legReachPos.HasValue)
				{
					legReachPos = feltAtPos;
				}
				if (UnityEngine.Random.value < intensity)
				{
					hasHeardSound = true;
				}
			}
		}

		public void TransmitFeel()
		{
			for (int i = 0; i < 5; i++)
			{
				if (tile.x + Custom.fourDirectionsAndZero[i].x < owner.bottomLeft.x || tile.y + Custom.fourDirectionsAndZero[i].y < owner.bottomLeft.y || tile.x + Custom.fourDirectionsAndZero[i].x > owner.topRight.x || tile.y + Custom.fourDirectionsAndZero[i].y > owner.topRight.y || owner.bulbs[tile.x - owner.bottomLeft.x + Custom.fourDirectionsAndZero[i].x, tile.y - owner.bottomLeft.y + Custom.fourDirectionsAndZero[i].y] == null)
				{
					continue;
				}
				for (int j = 0; j < owner.bulbs[tile.x - owner.bottomLeft.x + Custom.fourDirectionsAndZero[i].x, tile.y - owner.bottomLeft.y + Custom.fourDirectionsAndZero[i].y].Count; j++)
				{
					if (Custom.DistLess(stuckPos, owner.bulbs[tile.x - owner.bottomLeft.x + Custom.fourDirectionsAndZero[i].x, tile.y - owner.bottomLeft.y + Custom.fourDirectionsAndZero[i].y][j].stuckPos, 20f + rad + owner.bulbs[tile.x - owner.bottomLeft.x + Custom.fourDirectionsAndZero[i].x, tile.y - owner.bottomLeft.y + Custom.fourDirectionsAndZero[i].y][j].rad))
					{
						owner.bulbs[tile.x - owner.bottomLeft.x + Custom.fourDirectionsAndZero[i].x, tile.y - owner.bottomLeft.y + Custom.fourDirectionsAndZero[i].y][j].FeltSomething(feltSomethingIntensity - 0.075f, pos);
					}
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[firstSprite] = new FSprite("Futile_White");
			sLeaser.sprites[firstSprite].scale = rad / 8f;
			sLeaser.sprites[firstSprite].shader = rCam.room.game.rainWorld.Shaders["JaggedCircleBothSides"];
			sLeaser.sprites[firstSprite].alpha = 0.6f;
			if (hasEye)
			{
				for (int i = 0; i < 2; i++)
				{
					sLeaser.sprites[firstSprite + 1 + i] = MakeSlitMesh();
					sLeaser.sprites[firstSprite + 1 + i].shader = rCam.room.game.rainWorld.Shaders["RippleBasicBothSides"];
				}
			}
			if (hasBlackGoo)
			{
				sLeaser.sprites[BlackGooSprite] = new FSprite("corruption");
				sLeaser.sprites[BlackGooSprite].scale = rad * Mathf.Clamp(rad * 0.35f, 3f, 7f) / 100f * (customData?.darknessScale ?? 1f);
				sLeaser.sprites[BlackGooSprite].shader = rCam.room.game.rainWorld.Shaders["BlackGooBothSides"];
				sLeaser.sprites[BlackGooSprite].rotation = rotation;
			}
			if (hasDot)
			{
				sLeaser.sprites[dotSprite] = new FSprite("Futile_White");
				sLeaser.sprites[dotSprite].scale = rad / 8f * UnityEngine.Random.Range(customData?.minEyeSize ?? 0.45f, customData?.maxEyeSize ?? 0.65f);
				sLeaser.sprites[dotSprite].shader = rCam.room.game.rainWorld.Shaders["JaggedCircleBothSides"];
				sLeaser.sprites[dotSprite].alpha = 0.2f;
			}
			if (leg != null)
			{
				leg.graphic.InitiateSprites(sLeaser, rCam);
			}
			tendril?.graphic.InitiateSprites(sLeaser, rCam);
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			bool flag = rCam.room.ViewedByAnyCamera(vector, rad * 2f);
			float num = 0f;
			if (flag != lastVisible)
			{
				sLeaser.sprites[firstSprite].isVisible = flag;
				if (hasBlackGoo)
				{
					sLeaser.sprites[BlackGooSprite].isVisible = flag;
				}
				if (hasEye)
				{
					sLeaser.sprites[EyeSprite(0)].isVisible = flag;
					sLeaser.sprites[EyeSprite(1)].isVisible = flag;
				}
				if (hasDot)
				{
					sLeaser.sprites[dotSprite].isVisible = flag;
				}
				lastVisible = flag;
			}
			if (flag)
			{
				num = Custom.AimFromOneVectorToAnother(eyeStalkPos, pos) + rotation;
				sLeaser.sprites[firstSprite].x = vector.x - camPos.x;
				sLeaser.sprites[firstSprite].y = vector.y - camPos.y;
				sLeaser.sprites[firstSprite].rotation = num;
				if (hasDot)
				{
					sLeaser.sprites[dotSprite].SetPosition(vector + Custom.DirVec(eyeStalkPos, vector) * (rad * (1f - customData.maxEyeSize)) - camPos);
					sLeaser.sprites[dotSprite].rotation = num;
				}
				if (hasBlackGoo)
				{
					sLeaser.sprites[BlackGooSprite].x = stuckPos.x - camPos.x;
					sLeaser.sprites[BlackGooSprite].y = stuckPos.y - camPos.y;
				}
			}
			if (leg != null)
			{
				leg.graphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
			}
			tendril?.graphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
			if (hasEye && flag)
			{
				RenderSlits(vector, eyeStalkPos, num, sLeaser, rCam, timeStacker, camPos);
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color effectColor = EffectColor;
			if (customData != null)
			{
				float t = 0f;
				if (customData.eyeType == CustomRotData.EyeType.Dot)
				{
					t = 0f;
				}
				else if (customData.eyeType == CustomRotData.EyeType.Cross)
				{
					t = Mathf.Lerp(customData.minColor, customData.maxColor, Mathf.Pow(UnityEngine.Random.value, 0.5f));
				}
				sLeaser.sprites[firstSprite].color = Color.Lerp(palette.blackColor, effectColor, t);
			}
			else
			{
				sLeaser.sprites[firstSprite].color = palette.blackColor;
			}
			if (hasDot)
			{
				sLeaser.sprites[dotSprite].color = Color.Lerp(palette.blackColor, effectColor, Mathf.Lerp(customData.minColor, customData.maxColor, corruptionLevelAtMySpot));
			}
			if (leg != null)
			{
				leg.graphic.ApplyPalette(sLeaser, rCam, palette);
			}
			tendril?.graphic.ApplyPalette(sLeaser, rCam, palette);
		}

		public void RenderSlits(Vector2 pos, Vector2 middleOfBody, float rotation, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float num = Mathf.Lerp(rad * 0.5f, rad, eyeRad);
			float num2 = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastClosed, closed, timeStacker)), 0.6f);
			float num3 = 1f * (1f - num2);
			Vector2 b = Vector2.Lerp(lastLookDir, lookDir, timeStacker);
			float a = Mathf.Lerp(lastFocus, focus, timeStacker) * Mathf.Pow(Mathf.InverseLerp(-1f, 1f, Vector2.Dot(Custom.DirVec(middleOfBody, pos), lookDir.normalized)), 0.7f);
			a = Mathf.Max(a, num2);
			float a2 = Mathf.InverseLerp(0f, Mathf.Lerp(30f, 50f, 1f), Vector2.Distance(middleOfBody, pos + Custom.DirVec(middleOfBody, pos) * rad)) * 0.9f;
			a2 = Mathf.Lerp(a2, 1f, 0.5f * a);
			Vector2 vector = Vector2.Lerp(Custom.DirVec(middleOfBody, pos) * a2, b, b.magnitude * 0.5f);
			renderCenterPos = pos + vector * rad;
			renderCol = Color.Lerp(EyeColor, new Color(1f, 1f, 1f), Mathf.Lerp(UnityEngine.Random.value * light, 1f, num2));
			sLeaser.sprites[EyeSprite(0)].color = renderCol;
			sLeaser.sprites[EyeSprite(1)].color = renderCol;
			for (int i = 0; i < 2; i++)
			{
				Vector2 vector2 = Custom.DegToVec(rotation + 90f * (float)i);
				Vector2 vector3 = Custom.PerpendicularVector(vector2);
				(sLeaser.sprites[EyeSprite(i)] as TriangleMesh).MoveVertice(0, pos + BulgeVertex(vector2 * (num * 0.9f * Mathf.Lerp(1f, 0.6f, a)), vector, rad) - camPos);
				(sLeaser.sprites[EyeSprite(i)] as TriangleMesh).MoveVertice(9, pos + BulgeVertex(vector2 * ((0f - num) * 0.9f * Mathf.Lerp(1f, 0.6f, a)), vector, rad) - camPos);
				for (int j = 1; j < 5; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						float num4 = num * ((j < 3) ? 0.7f : 0.25f) * ((k == 0) ? 1f : (-1f)) * Mathf.Lerp(1f, 0.6f, a);
						int num5 = ((k == 0) ? j : (9 - j));
						float num6 = num3 * ((j < 3) ? 0.5f : 1f) * ((num5 % 2 == 0) ? 1f : (-1f)) * Mathf.Lerp(1f, 2.5f, a);
						(sLeaser.sprites[EyeSprite(i)] as TriangleMesh).MoveVertice(num5, pos + BulgeVertex(vector2 * num4 + vector3 * num6, vector, rad) - camPos);
					}
				}
			}
		}

		public Vector2 BulgeVertex(Vector2 v, Vector2 dir, float rad)
		{
			return Vector2.Lerp(v, Vector2.ClampMagnitude(v + dir * rad, rad), dir.magnitude);
		}

		public TriangleMesh MakeSlitMesh()
		{
			TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[8];
			for (int i = 0; i < 8; i++)
			{
				array[i] = new TriangleMesh.Triangle(i, i + 1, i + 2);
			}
			return new TriangleMesh("Futile_White", array, customColor: false);
		}

		public void AddBodyToContainerP1(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			rCam.ReturnFContainer(bodyInMidground ? "Midground" : "Water").AddChild(sLeaser.sprites[firstSprite]);
		}

		public void AddBodyToContainerP2(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			if (leg != null)
			{
				for (int i = 0; i < leg.graphic.sprites; i++)
				{
					rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[leg.graphic.firstSprite + i]);
				}
			}
			if (tendril != null)
			{
				for (int j = 0; j < tendril.graphic.sprites; j++)
				{
					rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[tendril.graphic.firstSprite + j]);
				}
			}
		}

		public void AddBodyToContainerP3(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			if (hasBlackGoo)
			{
				rCam.ReturnFContainer("Shortcuts").AddChild(sLeaser.sprites[BlackGooSprite]);
			}
		}

		public void AddEyeToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			if (hasEye)
			{
				rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[EyeSprite(0)]);
				rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[EyeSprite(1)]);
			}
			if (hasDot)
			{
				rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[dotSprite]);
			}
		}

		public Color GetColor()
		{
			return renderCol;
		}

		public Color GetEyeColor()
		{
			if (SentientRotMode(owner.room))
			{
				return RainWorld.RippleColor;
			}
			return Color.blue;
		}

		public Vector2 GetPosition()
		{
			return renderCenterPos;
		}
	}

	public abstract class CorruptionTube : UpdatableAndDeletable
	{
		public abstract class TubeGraphic : RopeGraphic
		{
			public struct Bump
			{
				public Vector2 pos;

				public float size;

				public float eyeSize;

				public Bump(Vector2 pos, float size, float eyeSize)
				{
					this.pos = pos;
					this.size = size;
					this.eyeSize = eyeSize;
				}
			}

			public CorruptionTube owner;

			public int firstSprite;

			public int totalSprites;

			public int sprites;

			public Rect segmentBounds;

			public bool lastVisible;

			public Bump[] bumps;

			public TubeGraphic(CorruptionTube owner, int segments, int firstSprite)
				: base(segments)
			{
				this.owner = owner;
				this.firstSprite = firstSprite;
				lastVisible = true;
				segmentBounds = default(Rect);
			}

			public void UpdateSegmentBounds(bool init, Vector2 pos)
			{
				if (init)
				{
					segmentBounds.xMin = pos.x;
					segmentBounds.xMax = pos.x;
					segmentBounds.yMin = pos.y;
					segmentBounds.yMax = pos.y;
					return;
				}
				if (pos.x > segmentBounds.xMax)
				{
					segmentBounds.xMax = pos.x;
				}
				if (pos.y > segmentBounds.yMax)
				{
					segmentBounds.yMax = pos.y;
				}
				if (pos.x < segmentBounds.xMin)
				{
					segmentBounds.xMin = pos.x;
				}
				if (pos.y < segmentBounds.yMin)
				{
					segmentBounds.yMin = pos.y;
				}
			}

			public override void Update()
			{
				for (int i = 0; i < segments.Length; i++)
				{
					segments[i].lastPos = segments[i].pos;
					segments[i].pos = owner.segments[i, 0];
					UpdateSegmentBounds(i == 0, segments[i].pos);
				}
			}

			public override void ConnectPhase(float totalRopeLength)
			{
			}

			public override void MoveSegment(int segment, Vector2 goalPos, Vector2 smoothedGoalPos)
			{
				segments[segment].vel *= 0f;
				if (owner.room.GetTile(smoothedGoalPos).Solid && !owner.room.GetTile(goalPos).Solid)
				{
					FloatRect floatRect = Custom.RectCollision(smoothedGoalPos, goalPos, owner.room.TileRect(owner.room.GetTilePosition(smoothedGoalPos)).Grow(3f));
					segments[segment].pos = new Vector2(floatRect.left, floatRect.bottom);
				}
				else
				{
					segments[segment].pos = smoothedGoalPos;
				}
				UpdateSegmentBounds(init: false, segments[segment].pos);
			}

			public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
			{
				sLeaser.sprites[firstSprite] = TriangleMesh.MakeLongMeshAtlased(segments.Length, pointyTip: false, customColor: true);
				sLeaser.sprites[firstSprite].shader = rCam.room.game.rainWorld.Shaders["RippleBasicBothSides"];
				int num = 0;
				for (int i = 0; i < bumps.Length; i++)
				{
					sLeaser.sprites[firstSprite + 1 + i] = new FSprite("Circle20", quadType: false);
					sLeaser.sprites[firstSprite + 1 + i].scale = Mathf.Lerp(2f, 6f, bumps[i].size) / 10f;
					sLeaser.sprites[firstSprite + 1 + i].shader = rCam.room.game.rainWorld.Shaders["RippleBasicBothSides"];
					if (bumps[i].eyeSize > 0f)
					{
						sLeaser.sprites[firstSprite + 1 + bumps.Length + num] = new FSprite("Circle20", quadType: false);
						sLeaser.sprites[firstSprite + 1 + bumps.Length + num].scale = Mathf.Lerp(2f, 6f, bumps[i].size) * bumps[i].eyeSize / 10f;
						sLeaser.sprites[firstSprite + 1 + bumps.Length + num].shader = rCam.room.game.rainWorld.Shaders["RippleBasicBothSides"];
						num++;
					}
				}
				totalSprites = 1 + bumps.Length + num;
				ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}

			public override void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
			{
				float num = 2f;
				bool flag = rCam.room.ViewedByAnyCamera(segmentBounds, num);
				if (flag != lastVisible)
				{
					for (int i = firstSprite; i < firstSprite + totalSprites; i++)
					{
						sLeaser.sprites[i].isVisible = flag;
					}
					lastVisible = flag;
				}
				if (!flag)
				{
					return;
				}
				Vector2 vector = Vector2.Lerp(segments[0].lastPos, segments[0].pos, timeStacker);
				vector += Custom.DirVec(Vector2.Lerp(segments[1].lastPos, segments[1].pos, timeStacker), vector) * 1f;
				for (int j = 0; j < segments.Length; j++)
				{
					Vector2 vector2 = Vector2.Lerp(segments[j].lastPos, segments[j].pos, timeStacker);
					Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(j * 4, vector - vector3 * num - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector + vector3 * num - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector2 - vector3 * num - camPos);
					(sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector2 + vector3 * num - camPos);
					vector = vector2;
				}
				int num2 = 0;
				for (int k = 0; k < bumps.Length; k++)
				{
					Vector2 vector4 = OnTubePos(bumps[k].pos, timeStacker);
					sLeaser.sprites[firstSprite + 1 + k].x = vector4.x - camPos.x;
					sLeaser.sprites[firstSprite + 1 + k].y = vector4.y - camPos.y;
					if (bumps[k].eyeSize > 0f)
					{
						sLeaser.sprites[firstSprite + 1 + bumps.Length + num2].x = vector4.x - camPos.x;
						sLeaser.sprites[firstSprite + 1 + bumps.Length + num2].y = vector4.y - camPos.y;
						num2++;
					}
				}
			}

			public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
			{
				Color color = owner.EffectColor;
				if (ModManager.MSC && this is NeuronFilledLeg.LegGraphic)
				{
					color = new Color(0.13f, 0f, 0.19f);
				}
				for (int i = 0; i < (sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length; i++)
				{
					float floatPos = (float)i / (float)((sLeaser.sprites[firstSprite] as TriangleMesh).vertices.Length - 1);
					(sLeaser.sprites[firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(palette.blackColor, color, OnTubeEffectColorFac(floatPos));
				}
				int num = 0;
				for (int j = 0; j < bumps.Length; j++)
				{
					sLeaser.sprites[firstSprite + 1 + j].color = Color.Lerp(palette.blackColor, color, OnTubeEffectColorFac(bumps[j].pos.y));
					if (bumps[j].eyeSize > 0f)
					{
						sLeaser.sprites[firstSprite + 1 + bumps.Length + num].color = (owner.owner.GWmode ? palette.blackColor : color);
						num++;
					}
				}
			}

			public virtual float OnTubeEffectColorFac(float floatPos)
			{
				return Mathf.Pow(floatPos, 1.5f) * 0.4f;
			}

			public Vector2 OnTubePos(Vector2 pos, float timeStacker)
			{
				Vector2 p = OneDimensionalTubePos(pos.y - 1f / (float)(segments.Length - 1), timeStacker);
				Vector2 p2 = OneDimensionalTubePos(pos.y + 1f / (float)(segments.Length - 1), timeStacker);
				return OneDimensionalTubePos(pos.y, timeStacker) + Custom.PerpendicularVector(Custom.DirVec(p, p2)) * pos.x;
			}

			public Vector2 OneDimensionalTubePos(float floatPos, float timeStacker)
			{
				int num = Custom.IntClamp(Mathf.FloorToInt(floatPos * (float)(segments.Length - 1)), 0, segments.Length - 1);
				int num2 = Custom.IntClamp(num + 1, 0, segments.Length - 1);
				float t = Mathf.InverseLerp(num, num2, floatPos * (float)(segments.Length - 1));
				return Vector2.Lerp(Vector2.Lerp(segments[num].lastPos, segments[num2].lastPos, t), Vector2.Lerp(segments[num].pos, segments[num2].pos, t), timeStacker);
			}
		}

		public DaddyCorruption owner;

		public TubeGraphic graphic;

		public float conRad = 10f;

		public float pushApart = 0.15f;

		public Vector2[,] segments;

		public Vector2? stuckPosA;

		public Vector2? stuckPosB;

		public SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public virtual Color EffectColor => owner.effectColor;

		public CorruptionTube(DaddyCorruption owner, float length, Vector2 spawnPosA, Vector2 spawnPosB, bool stuckAtA, bool stuckAtB)
		{
			room = owner.room;
			this.owner = owner;
			if (stuckAtA)
			{
				stuckPosA = spawnPosA;
			}
			if (stuckAtB)
			{
				stuckPosB = spawnPosB;
			}
			if (ModManager.MMF)
			{
				segments = new Vector2[Mathf.Max(2, (int)Mathf.Clamp(length / conRad, 1f, 200f)), 3];
			}
			else
			{
				segments = new Vector2[(int)Mathf.Clamp(length / conRad, 1f, 200f), 3];
			}
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				float t = (float)i / (float)(segments.GetLength(0) - 1);
				segments[i, 0] = Vector2.Lerp(spawnPosA, spawnPosB, t) + Custom.RNV() * UnityEngine.Random.value;
				segments[i, 1] = segments[i, 0];
				segments[i, 2] = Custom.RNV() * UnityEngine.Random.value;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 2; i < segments.GetLength(0); i++)
			{
				Vector2 vector = Custom.DirVec(segments[i - 2, 0], segments[i, 0]);
				segments[i - 2, 2] -= vector * pushApart;
				segments[i, 2] += vector * pushApart;
			}
			for (int j = 0; j < segments.GetLength(0); j++)
			{
				segments[j, 2].y -= 0.9f * room.gravity * GravityAffected(j);
				segments[j, 1] = segments[j, 0];
				segments[j, 0] += segments[j, 2];
				segments[j, 2] *= 0.999f;
				if (room.gravity < 1f && room.readyForAI && room.aimap.getTerrainProximity(segments[j, 0]) < 4)
				{
					IntVector2 tilePosition = room.GetTilePosition(segments[j, 0]);
					Vector2 vector2 = new Vector2(0f, 0f);
					for (int k = 0; k < 4; k++)
					{
						if (!room.GetTile(tilePosition + Custom.fourDirections[k]).Solid && !room.aimap.getAItile(tilePosition + Custom.fourDirections[k]).narrowSpace)
						{
							float num = 0f;
							for (int l = 0; l < 4; l++)
							{
								num += (float)room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[k] + Custom.fourDirections[l]);
							}
							vector2 += Custom.fourDirections[k].ToVector2() * num;
						}
					}
					segments[j, 2] += vector2.normalized * ((room.GetTile(segments[j, 0]).Solid ? 1f : Custom.LerpMap(room.aimap.getTerrainProximity(segments[j, 0]), 0f, 3f, 2f, 0.2f)) * (1f - room.gravity));
				}
				if (j > 2 && room.aimap.getTerrainProximity(segments[j, 0]) < 3)
				{
					SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(segments[j, 0], segments[j, 1], segments[j, 2], 2f, new IntVector2(0, 0), goThroughFloors: true);
					cd = SharedPhysics.VerticalCollision(room, cd);
					cd = SharedPhysics.HorizontalCollision(room, cd);
					segments[j, 0] = cd.pos;
					segments[j, 2] = cd.vel;
					if (cd.contactPoint.x != 0)
					{
						segments[j, 2].y *= 0.6f;
					}
					if (cd.contactPoint.y != 0)
					{
						segments[j, 2].x *= 0.6f;
					}
				}
			}
			ConnectToWalls();
			for (int num2 = segments.GetLength(0) - 1; num2 > 0; num2--)
			{
				Connect(num2, num2 - 1);
			}
			ConnectToWalls();
			for (int m = 1; m < segments.GetLength(0); m++)
			{
				Connect(m, m - 1);
			}
			ConnectToWalls();
			graphic.Update();
		}

		public void ConnectToWalls()
		{
			if (stuckPosA.HasValue)
			{
				segments[0, 0] = stuckPosA.Value;
				segments[0, 2] *= 0f;
			}
			if (stuckPosB.HasValue)
			{
				segments[segments.GetLength(0) - 1, 0] = stuckPosB.Value;
				segments[segments.GetLength(0) - 1, 2] *= 0f;
			}
		}

		public void Connect(int A, int B)
		{
			Vector2 normalized = (segments[A, 0] - segments[B, 0]).normalized;
			float num = Vector2.Distance(segments[A, 0], segments[B, 0]);
			float num2 = Mathf.InverseLerp(0f, conRad, num);
			Vector2 vector = normalized * ((conRad - num) * 0.5f * num2);
			segments[A, 0] += vector;
			segments[A, 2] += vector;
			segments[B, 0] -= vector;
			segments[B, 2] -= vector;
		}

		public virtual float GravityAffected(int seg)
		{
			return 1f;
		}
	}

	public class LittleLeg : CorruptionTube
	{
		public class LegGraphic : TubeGraphic
		{
			public float deadness;

			public LegGraphic(CorruptionTube owner, int parts, int firstSprite)
				: base(owner, parts, firstSprite)
			{
				deadness = UnityEngine.Random.value;
				sprites = 1;
				bumps = new Bump[parts / 2 + UnityEngine.Random.Range(5, 8)];
				for (int i = 0; i < bumps.Length; i++)
				{
					float num = Mathf.Pow(UnityEngine.Random.value, 0.5f);
					if (i == 0)
					{
						num = 1f;
					}
					bumps[i] = new Bump(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3f * num, Mathf.Lerp(Mathf.InverseLerp(0f, parts, parts - 20), 1f, num)), Mathf.Lerp(UnityEngine.Random.value, num, UnityEngine.Random.value), (UnityEngine.Random.value * (1f + deadness) < Mathf.Lerp(0f, 0.6f, num)) ? Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.5f, 0.5f, num))) : 0f);
					sprites++;
					if (bumps[i].eyeSize > 0f)
					{
						sprites++;
					}
				}
			}

			public override float OnTubeEffectColorFac(float floatPos)
			{
				return base.OnTubeEffectColorFac(floatPos) * (1f - deadness);
			}
		}

		public Vector2 mountedDir;

		public int moveCounter;

		public Bulb myBulb;

		public BodyChunk grabChunk;

		public float pullInPrey;

		public override Color EffectColor
		{
			get
			{
				if (myBulb.customData == null)
				{
					return base.EffectColor;
				}
				return myBulb.EffectColor;
			}
		}

		public LittleLeg(Bulb myBulb, DaddyCorruption owner, int firstSprite, Vector2 spawnPos, Vector2 tipPos)
			: base(owner, Vector2.Distance(spawnPos, tipPos), spawnPos, tipPos, stuckAtA: true, stuckAtB: false)
		{
			this.myBulb = myBulb;
			if (ModManager.MMF && room.abstractRoom.singleRealizedRoom)
			{
				graphic = new LegGraphic(this, segments.GetLength(0), firstSprite);
			}
			else
			{
				graphic = new LegGraphic(this, Custom.IntClamp((int)(Vector2.Distance(spawnPos, tipPos) / 10f), 1, 200), firstSprite);
			}
			mountedDir = (tipPos - spawnPos).normalized;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				float num = (float)i / (float)(segments.GetLength(0) - 1);
				segments[i, 2] += mountedDir * Mathf.InverseLerp(5f, 1f, i);
				if (grabChunk != null)
				{
					segments[i, 2] += Custom.DirVec(grabChunk.pos, segments[i, 0]) * (0.2f * UnityEngine.Random.value);
				}
				else if (myBulb.legReachPos.HasValue)
				{
					segments[i, 2] += Custom.DirVec(segments[i, 0], myBulb.legReachPos.Value) * (0.2f * UnityEngine.Random.value);
				}
				else if (moveCounter < 0)
				{
					segments[i, 2] += Custom.RNV() * (2f * UnityEngine.Random.value * num);
				}
			}
			if (grabChunk != null)
			{
				if (!myBulb.hasHeardSound)
				{
					myBulb.light = Mathf.Max(UnityEngine.Random.value, myBulb.light);
					myBulb.nextLookDir = Custom.DirVec(myBulb.pos, grabChunk.pos);
				}
				Vector2 vector = Custom.DirVec(grabChunk.pos, segments[segments.GetLength(0) - 1, 0]);
				float num2 = Vector2.Distance(grabChunk.pos, segments[segments.GetLength(0) - 1, 0]);
				float num3 = grabChunk.mass / (0.1f + grabChunk.mass);
				Vector2 vector2 = vector * ((grabChunk.rad - num2) * (1f - num3));
				Vector2 vector3 = vector * ((grabChunk.rad - num2) * num3);
				grabChunk.pos -= vector2;
				grabChunk.vel -= vector2;
				segments[segments.GetLength(0) - 1, 0] += vector3;
				segments[segments.GetLength(0) - 1, 2] += vector3;
				float num4 = 10f;
				if (ModManager.MMF && grabChunk.owner is Player)
				{
					num4 = Mathf.Lerp(10f, 0f, Mathf.Clamp((grabChunk.owner as Player).GraspWiggle * 10f, 0f, 1f));
				}
				grabChunk.vel += Custom.DirVec(grabChunk.pos, myBulb.stuckPos) * (Mathf.InverseLerp(0.5f, 1f, pullInPrey) * 3f) / grabChunk.mass;
				if (!Custom.DistLess(grabChunk.pos, stuckPosA.Value, (float)segments.GetLength(0) * num4 + grabChunk.rad + (ModManager.MMF ? 40f : 50f)) || grabChunk.owner.slatedForDeletetion || grabChunk.owner.room != room)
				{
					(grabChunk.owner as Creature).GrabbedByDaddyCorruption = false;
					grabChunk = null;
				}
				if (ModManager.MMF && owner.CorruptionLevel(stuckPosA.Value) < 0.4f && UnityEngine.Random.Range(0f, 100f) < 1f)
				{
					(grabChunk.owner as Creature).GrabbedByDaddyCorruption = false;
					grabChunk = null;
				}
				pullInPrey = Mathf.Min(1f, pullInPrey + 1f / 90f);
				if (ModManager.MMF)
				{
					pullInPrey = Mathf.Max(pullInPrey, 20f);
				}
				return;
			}
			pullInPrey = Mathf.Max(0f, pullInPrey - 0.025f);
			if (!ModManager.MMF || owner.CorruptionLevel(stuckPosA.Value) > 0.2f)
			{
				for (int j = 0; j < room.abstractRoom.creatures.Count; j++)
				{
					if (grabChunk != null)
					{
						break;
					}
					if (room.abstractRoom.creatures[j].realizedCreature == null || room.abstractRoom.creatures[j].creatureTemplate.daddyCorruptionImmune || (ModManager.MMF && room.abstractRoom.creatures[j].realizedCreature is Overseer) || room.abstractRoom.creatures[j].tentacleImmune)
					{
						continue;
					}
					for (int k = 0; k < room.abstractRoom.creatures[j].realizedCreature.bodyChunks.Length; k++)
					{
						if (grabChunk != null)
						{
							break;
						}
						if (Custom.DistLess(room.abstractRoom.creatures[j].realizedCreature.bodyChunks[k].pos, segments[segments.GetLength(0) - 1, 0], room.abstractRoom.creatures[j].realizedCreature.bodyChunks[k].rad))
						{
							if (room.abstractRoom.creatures[j].realizedCreature is Player && room.game.IsStorySession && room.game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility)
							{
								(room.abstractRoom.creatures[j].realizedCreature as Player).SetHaloDisplayTime(20);
								continue;
							}
							grabChunk = room.abstractRoom.creatures[j].realizedCreature.bodyChunks[k];
							(grabChunk.owner as Creature).GrabbedByDaddyCorruption = true;
							myBulb.FeltSomething(1f, grabChunk.pos);
						}
					}
				}
			}
			moveCounter--;
			if (moveCounter < 0 && UnityEngine.Random.value < 0.025f)
			{
				moveCounter = UnityEngine.Random.Range(80, 300);
			}
		}

		public override float GravityAffected(int seg)
		{
			return Mathf.InverseLerp(2f, 5f, seg);
		}
	}

	public class Tendril : CorruptionTube
	{
		public class TendrilGraphic : TubeGraphic
		{
			public TendrilGraphic(CorruptionTube owner, int parts, int firstSprite)
				: base(owner, parts, firstSprite)
			{
				sprites = 1;
				bumps = new Bump[parts / 2 + UnityEngine.Random.Range(5, 8)];
				for (int i = 0; i < bumps.Length; i++)
				{
					float num = Mathf.Pow(UnityEngine.Random.value, 0.5f);
					if (i == 0)
					{
						num = 1f;
					}
					bumps[i] = new Bump(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3f * num, Mathf.Lerp(Mathf.InverseLerp(0f, parts, parts - 20), 1f, num)), Mathf.Lerp(UnityEngine.Random.value, num, UnityEngine.Random.value), 0f);
					sprites++;
					if (bumps[i].eyeSize > 0f)
					{
						sprites++;
					}
				}
			}

			public override float OnTubeEffectColorFac(float floatPos)
			{
				return 0f;
			}
		}

		public Bulb myBulb;

		public Tendril(Bulb myBulb, DaddyCorruption owner, int firstSprite, Vector2 spawnPos, Vector2 tipPos)
			: base(owner, Vector2.Distance(spawnPos, tipPos), spawnPos, tipPos, stuckAtA: true, stuckAtB: false)
		{
			this.myBulb = myBulb;
			graphic = new TendrilGraphic(this, Custom.IntClamp((int)(Vector2.Distance(spawnPos, tipPos) / 10f), 1, 200), firstSprite);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (UnityEngine.Random.value < 0.0025f)
			{
				segments[UnityEngine.Random.Range(0, segments.GetLength(0)), 2] += Custom.RNV() * UnityEngine.Random.value;
			}
		}

		public override float GravityAffected(int seg)
		{
			return 1f;
		}
	}

	public class CorruptionDarkness : UpdatableAndDeletable, IDrawable
	{
		public PlacedObject placedObject;

		public CorruptionDarkness(PlacedObject placedObject)
		{
			this.placedObject = placedObject;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("corruption");
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["BlackGoo"];
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = placedObject.pos.x - camPos.x;
			sLeaser.sprites[0].y = placedObject.pos.y - camPos.y;
			sLeaser.sprites[0].rotation = Custom.VecToDeg((placedObject.data as PlacedObject.ResizableObjectData).handlePos.normalized);
			sLeaser.sprites[0].scale = (placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude / 100f;
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			sLeaser.sprites[0].RemoveFromContainer();
			rCam.ReturnFContainer("Shortcuts").AddChild(sLeaser.sprites[0]);
		}
	}

	public class ClimbableCorruptionTube : CorruptionTube, IClimbableVine
	{
		public class ClimbTubeGraphic : TubeGraphic
		{
			public ClimbTubeGraphic(CorruptionTube owner, int parts, int firstSprite)
				: base(owner, parts, firstSprite)
			{
				sprites = 1;
				if (ModManager.MMF && owner.room.abstractRoom.singleRealizedRoom)
				{
					bumps = new Bump[Math.Max(0, parts / 2 + UnityEngine.Random.Range(-1, 6))];
				}
				else
				{
					bumps = new Bump[Math.Max(0, parts + UnityEngine.Random.Range(-5, 6))];
				}
				for (int i = 0; i < bumps.Length; i++)
				{
					bumps[i] = new Bump(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 2f, UnityEngine.Random.value), Mathf.Pow(UnityEngine.Random.value, 1.4f), (UnityEngine.Random.value < 0.2f) ? Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, 0.5f)) : 0f);
					sprites++;
					if (bumps[i].eyeSize > 0f)
					{
						sprites++;
					}
				}
			}

			public override void Update()
			{
				for (int i = 0; i < segments.Length; i++)
				{
					segments[i].lastPos = segments[i].pos;
					segments[i].pos = (owner as ClimbableCorruptionTube).OnTubePos((float)i / (float)(segments.Length - 1));
					UpdateSegmentBounds(i == 0, segments[i].pos);
				}
			}

			public override float OnTubeEffectColorFac(float floatPos)
			{
				return Mathf.Sin(floatPos * (float)Math.PI) * 0.4f;
			}
		}

		public Rope[] ropes;

		public List<Vector2> possList;

		public Vector2 aMountDir;

		public Vector2 bMountDir;

		public ClimbableCorruptionTube(Room room, DaddyCorruption owner, int firstSprite, PlacedObject placedObject)
			: base(owner, ((placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude * 1.1f + 50f) / 3f, placedObject.pos, placedObject.pos + (placedObject.data as PlacedObject.ResizableObjectData).handlePos, stuckAtA: true, stuckAtB: true)
		{
			graphic = new ClimbTubeGraphic(this, Custom.IntClamp((int)(((placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude * 1.1f + 50f) / 10f), 1, 200), firstSprite);
			if (room.climbableVines == null)
			{
				room.climbableVines = new ClimbableVinesSystem();
				room.AddObject(room.climbableVines);
			}
			room.climbableVines.vines.Add(this);
			aMountDir = owner.Dir(room.GetTilePosition(placedObject.pos));
			bMountDir = owner.Dir(room.GetTilePosition(placedObject.pos + (placedObject.data as PlacedObject.ResizableObjectData).handlePos));
			ropes = new Rope[segments.GetLength(0) - 1];
			for (int i = 0; i < ropes.Length; i++)
			{
				ropes[i] = new Rope(room, segments[i, 0], segments[i + 1, 0], 2f);
			}
			conRad *= 3f;
			pushApart /= 3f;
			possList = new List<Vector2>();
			for (int j = 0; j < segments.GetLength(0); j++)
			{
				possList.Add(segments[j, 0]);
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				possList[i] = segments[i, 0];
				segments[i, 2] += aMountDir * (Mathf.InverseLerp(3f, 1f, i) * Mathf.Lerp(0.5f, 0.1f, room.gravity));
				segments[i, 2] += bMountDir * (Mathf.InverseLerp(segments.GetLength(0) - 4, segments.GetLength(0) - 2, i) * Mathf.Lerp(0.5f, 0.1f, room.gravity));
				segments[i, 2] += Custom.RNV() * (0.15f * UnityEngine.Random.value * Mathf.Sin((float)i / (float)(segments.GetLength(0) - 1) * (float)Math.PI) * (1f - room.gravity));
			}
			for (int j = 0; j < ropes.Length; j++)
			{
				if (ropes[j].bends.Count > 3)
				{
					ropes[j].Reset();
				}
				ropes[j].Update(segments[j, 0], segments[j + 1, 0]);
				if (ropes[j].totalLength > conRad)
				{
					Vector2 vector = Custom.DirVec(segments[j, 0], ropes[j].AConnect) * ((ropes[j].totalLength - conRad) * 0.5f);
					segments[j, 0] += vector;
					segments[j, 2] += vector;
					vector = Custom.DirVec(segments[j + 1, 0], ropes[j].BConnect) * ((ropes[j].totalLength - conRad) * 0.5f);
					segments[j + 1, 0] += vector;
					segments[j + 1, 2] += vector;
				}
			}
			if (ModManager.MMF)
			{
				graphic.Update();
			}
		}

		public override float GravityAffected(int seg)
		{
			return Mathf.Min(Mathf.InverseLerp(2f, 5f, seg), Mathf.InverseLerp(segments.GetLength(0) - 3, segments.GetLength(0) - 6, seg));
		}

		public Vector2 OnTubePos(float ps)
		{
			int num = Custom.IntClamp(Mathf.FloorToInt(ps * (float)(segments.GetLength(0) - 1)), 0, segments.GetLength(0) - 1);
			int num2 = Custom.IntClamp(num + 1, 0, segments.GetLength(0) - 1);
			float f = Mathf.InverseLerp(num, num2, ps * (float)(segments.GetLength(0) - 1));
			Vector2 cA = segments[num, 0] - (segments[Custom.IntClamp(num - 1, 0, segments.GetLength(0) - 1), 0] - segments[num, 0]).normalized * (Vector2.Distance(segments[num, 0], segments[num2, 0]) * 0.25f);
			Vector2 cB = segments[num2, 0] - (segments[Custom.IntClamp(num2 + 1, 0, segments.GetLength(0) - 1), 0] - segments[num2, 0]).normalized * (Vector2.Distance(segments[num, 0], segments[num2, 0]) * 0.25f);
			return Custom.Bezier(segments[num, 0], cA, segments[num2, 0], cB, f);
		}

		public Vector2 Pos(int index)
		{
			return segments[index, 0];
		}

		public int TotalPositions()
		{
			return segments.GetLength(0);
		}

		public float Rad(int index)
		{
			return 2f;
		}

		public float Mass(int index)
		{
			return 0.25f;
		}

		public void Push(int index, Vector2 movement)
		{
			segments[index, 0] += movement;
			segments[index, 2] += movement;
		}

		public void BeingClimbedOn(Creature crit)
		{
		}

		public bool CurrentlyClimbable()
		{
			return true;
		}
	}

	public class DaddyRestraint : CorruptionTube
	{
		public class RestraintTubeGraphic : TubeGraphic
		{
			public RestraintTubeGraphic(CorruptionTube owner, int parts, int firstSprite)
				: base(owner, parts, firstSprite)
			{
				sprites = 1;
				bumps = new Bump[Math.Max(0, parts + UnityEngine.Random.Range(-5, 6))];
				for (int i = 0; i < bumps.Length; i++)
				{
					bumps[i] = new Bump(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 2f, UnityEngine.Random.value), Mathf.Pow(UnityEngine.Random.value, 1.4f), (UnityEngine.Random.value < 0.2f) ? Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, 0.5f)) : 0f);
					sprites++;
					if (bumps[i].eyeSize > 0f)
					{
						sprites++;
					}
				}
			}

			public override float OnTubeEffectColorFac(float floatPos)
			{
				return Mathf.Sin(floatPos * (float)Math.PI) * 0.4f;
			}
		}

		public PlacedObject placedObject;

		public DaddyLongLegs daddy;

		public bool fixate;

		public int fixCounter;

		public List<Vector2> completeFixPositions;

		public DaddyRestraint(DaddyLongLegs daddy, DaddyCorruption owner, int firstSprite, PlacedObject placedObject)
			: base(owner, (placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude * 0.5f, placedObject.pos, placedObject.pos + (placedObject.data as PlacedObject.ResizableObjectData).handlePos, stuckAtA: true, stuckAtB: false)
		{
			this.daddy = daddy;
			this.placedObject = placedObject;
			if (ModManager.MMF && room.abstractRoom.singleRealizedRoom)
			{
				graphic = new RestraintTubeGraphic(this, segments.GetLength(0), firstSprite);
			}
			else
			{
				graphic = new RestraintTubeGraphic(this, Custom.IntClamp((int)((placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude * 0.5f / 10f), 1, 200), firstSprite);
			}
			if ((placedObject.data as PlacedObject.ResizableObjectData).Rad < 50f)
			{
				fixate = true;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			segments[segments.GetLength(0) - 1, 0] = daddy.MiddleOfBody;
			segments[segments.GetLength(0) - 1, 2] *= 0f;
			if (fixate)
			{
				if (completeFixPositions == null)
				{
					fixCounter++;
					if (fixCounter > 10)
					{
						completeFixPositions = new List<Vector2>();
						for (int i = 0; i < daddy.bodyChunks.Length; i++)
						{
							completeFixPositions.Add(daddy.bodyChunks[i].pos + (placedObject.pos - daddy.MiddleOfBody));
							daddy.bodyChunks[i].collideWithTerrain = false;
						}
						daddy.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
					}
				}
				else
				{
					for (int j = 0; j < daddy.bodyChunks.Length; j++)
					{
						daddy.bodyChunks[j].pos = completeFixPositions[j];
						daddy.bodyChunks[j].vel *= 0f;
					}
				}
			}
			else if (!Custom.DistLess(daddy.mainBodyChunk.pos, placedObject.pos, (placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude))
			{
				Vector2 vector = Custom.DirVec(daddy.mainBodyChunk.pos, placedObject.pos);
				float num = Vector2.Distance(daddy.mainBodyChunk.pos, placedObject.pos);
				daddy.mainBodyChunk.pos += vector * (num - (placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude);
				daddy.mainBodyChunk.vel += vector * (num - (placedObject.data as PlacedObject.ResizableObjectData).handlePos.magnitude);
			}
			if (ModManager.MMF)
			{
				graphic.Update();
			}
		}
	}

	public class CustomRotData : PlacedObject.ResizableObjectData
	{
		public class RotColor : ExtEnum<RotColor>
		{
			public static RotColor EffectColorA = new RotColor("EffectColorA");

			public static RotColor EffectColorB = new RotColor("EffectColorB");

			public static RotColor Default = new RotColor("Default");

			public RotColor(string value, bool register = true)
				: base(value, register)
			{
			}
		}

		public class EyeType : ExtEnum<EyeType>
		{
			public static EyeType Cross = new EyeType("Cross");

			public static EyeType Dot = new EyeType("Dot");

			public EyeType(string value, bool register = true)
				: base(value, register)
			{
			}
		}

		public class Behavior : ExtEnum<Behavior>
		{
			public static Behavior Passive = new Behavior("Passive");

			public static Behavior Hostile = new Behavior("Hostile");

			public Behavior(string value, bool register = true)
				: base(value, register)
			{
			}
		}

		public const float sizeLimit = 30f;

		public const float densityLimit = 3f;

		public const float darknessScaleLimit = 2f;

		public Vector2 panelPos;

		public float minSize = 7f;

		public float maxSize = 15f;

		public float eyeChance = 0.8f;

		public float minEyeSize = 0.3f;

		public float maxEyeSize = 0.55f;

		public float minColor = 0.05f;

		public float maxColor = 0.15f;

		public float slowdown = 0.2f;

		public float legChance = 0.2f;

		public RotColor color = RotColor.Default;

		public EyeType eyeType = EyeType.Dot;

		public Behavior behavior = Behavior.Passive;

		public float density = 2f;

		public float darknessChance;

		public float darknessScale = 1f;

		public CustomRotData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			try
			{
				if (array.Length > 2)
				{
					panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
					panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
					minSize = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
					maxSize = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
					eyeChance = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
					minEyeSize = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
					maxEyeSize = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
					minColor = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);
					maxColor = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
					slowdown = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
					legChance = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
					color = new RotColor(array[13]);
					eyeType = new EyeType(array[14]);
					behavior = new Behavior(array[15]);
					density = float.Parse(array[16], NumberStyles.Any, CultureInfo.InvariantCulture);
					darknessChance = float.Parse(array[17], NumberStyles.Any, CultureInfo.InvariantCulture);
					darknessScale = float.Parse(array[18], NumberStyles.Any, CultureInfo.InvariantCulture);
					unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 19);
				}
			}
			catch (Exception)
			{
			}
		}

		public override string ToString()
		{
			string baseString = BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}", panelPos.x, panelPos.y, minSize, maxSize, eyeChance, minEyeSize, maxEyeSize, minColor, maxColor, slowdown, legChance, color.value, eyeType.value, behavior.value, density, darknessChance, darknessScale);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public class NeuronFilledLeg : CorruptionTube
	{
		public class LegGraphic : TubeGraphic
		{
			public float deadness;

			public LegGraphic(CorruptionTube owner, int parts, int firstSprite)
				: base(owner, parts, firstSprite)
			{
				deadness = UnityEngine.Random.value / 2f;
				sprites = 1;
				bumps = new Bump[parts / 2 + UnityEngine.Random.Range(5, 8)];
				for (int i = 0; i < bumps.Length; i++)
				{
					float num = Mathf.Pow(UnityEngine.Random.value, 0.5f);
					if (i == 0)
					{
						num = 1f;
					}
					bumps[i] = new Bump(new Vector2(Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 3f * num, Mathf.Lerp(Mathf.InverseLerp(0f, parts, parts - 20), 1f, num)), Mathf.Lerp(UnityEngine.Random.value, num, UnityEngine.Random.value), (UnityEngine.Random.value * (1f + deadness) >= Mathf.Lerp(0f, 0.6f, num)) ? 0f : Mathf.Lerp(0.2f, 0.8f, Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(1.1f, 0.4f, num))));
					sprites++;
					if (bumps[i].eyeSize > 0f)
					{
						sprites++;
					}
				}
			}

			public override float OnTubeEffectColorFac(float floatPos)
			{
				return base.OnTubeEffectColorFac(floatPos) * (1f - deadness);
			}
		}

		public Vector2 mountedDir;

		public int moveCounter;

		public List<AbstractPhysicalObject> mySwarmers;

		public PlacedObject.ResizableObjectData resizableObjectData;

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				float num = (float)i / (float)(segments.GetLength(0) - 1);
				segments[i, 2] += mountedDir * Mathf.InverseLerp(5f, 1f, i);
				if (moveCounter < 0)
				{
					segments[i, 2] += Custom.RNV() * (2f * UnityEngine.Random.value * num);
				}
			}
			moveCounter--;
			if (moveCounter < 0 && UnityEngine.Random.value < 0.025f)
			{
				moveCounter = UnityEngine.Random.Range(80, 300);
			}
			int min = Mathf.Min(4, segments.GetLength(0));
			int max = segments.GetLength(0) - 1;
			for (int j = 0; j < mySwarmers.Count; j++)
			{
				if (mySwarmers[j] != null)
				{
					if (mySwarmers[j].realizedObject == null)
					{
						mySwarmers[j].RealizeInRoom();
					}
					int num2 = Mathf.Clamp(segments.GetLength(0) - j * 2, min, max);
					int num3 = Mathf.Clamp(num2 - 1, min, max);
					mySwarmers[j].realizedObject.firstChunk.pos = segments[num2, 0] + Custom.PerpendicularVector(Custom.DirVec(segments[num2, 0], segments[num3, 0])) * (Mathf.Sin(num2) * 7f);
					(mySwarmers[j].realizedObject as SSOracleSwarmer).direction = Custom.DirVec(segments[num2, 0], segments[num3, 0]) * ((j % 5 == 0) ? 1f : (-1f)) + Custom.DegToVec(Mathf.Cos(j) * 10f);
					(mySwarmers[j].realizedObject as SSOracleSwarmer).dark = segments[num2, 2].magnitude < 2f;
					(mySwarmers[j].realizedObject as SSOracleSwarmer).firstChunk.vel *= 0.2f;
					if (mySwarmers[j].realizedObject.grabbedBy.Count > 0)
					{
						mySwarmers[j].destroyOnAbstraction = false;
						(mySwarmers[j].realizedObject as SSOracleSwarmer).dark = false;
						mySwarmers[j] = null;
						break;
					}
				}
			}
		}

		public override float GravityAffected(int seg)
		{
			return Mathf.InverseLerp(2f, 5f, seg);
		}

		public NeuronFilledLeg(DaddyCorruption owner, int firstSprite, Vector2 spawnPos, Vector2 tipPos, PlacedObject.ResizableObjectData resizableObjectData)
			: base(owner, Vector2.Distance(spawnPos, tipPos), spawnPos, tipPos, stuckAtA: true, stuckAtB: false)
		{
			if (room.abstractRoom.singleRealizedRoom)
			{
				graphic = new LegGraphic(this, segments.GetLength(0), firstSprite);
			}
			else
			{
				graphic = new LegGraphic(this, Custom.IntClamp((int)(Vector2.Distance(spawnPos, tipPos) / 10f), 1, 200), firstSprite);
			}
			float num = Vector2.Distance(spawnPos, tipPos);
			mySwarmers = new List<AbstractPhysicalObject>();
			UnityEngine.Random.State state = UnityEngine.Random.state;
			int num2 = 0;
			if (room.game.IsStorySession)
			{
				num2 = room.game.GetStorySession.saveState.cycleNumber;
			}
			UnityEngine.Random.InitState((int)spawnPos.magnitude + (int)tipPos.magnitude + (int)num + room.abstractRoom.index + num2);
			for (int i = 0; (float)i < Mathf.Lerp(2f, 6f, Mathf.InverseLerp(50f, 200f, num)); i++)
			{
				if (UnityEngine.Random.value < 0.5f)
				{
					AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, null, room.ToWorldCoordinate(tipPos), room.world.game.GetNewID());
					mySwarmers.Add(abstractPhysicalObject);
					abstractPhysicalObject.destroyOnAbstraction = true;
				}
				else
				{
					mySwarmers.Add(null);
				}
			}
			UnityEngine.Random.state = state;
			this.resizableObjectData = resizableObjectData;
			mountedDir = (tipPos - spawnPos).normalized;
		}
	}

	public IntVector2 bottomLeft;

	public IntVector2 topRight;

	public List<PlacedObject> places;

	public List<IntVector2> tiles;

	public Vector2[,] directions;

	public List<Bulb>[,] bulbs;

	public int totalSprites;

	public List<ClimbableCorruptionTube> climbTubes;

	public List<DaddyRestraint> restrainedDaddies;

	public List<EatenCreature> eatCreatures;

	public bool GWmode;

	public Color effectColor;

	public Color eyeColor;

	public Color effectColorA;

	public Color effectColorB;

	public List<NeuronFilledLeg> neuronLegs;

	public List<Bulb> allBulbs = new List<Bulb>();

	public HashSet<Creature> slowedCreatures = new HashSet<Creature>();

	public DaddyCorruption(Room room)
	{
		places = new List<PlacedObject>();
		climbTubes = new List<ClimbableCorruptionTube>();
		restrainedDaddies = new List<DaddyRestraint>();
		eatCreatures = new List<EatenCreature>();
		if (ModManager.MSC)
		{
			neuronLegs = new List<NeuronFilledLeg>();
		}
		effectColor = new Color(0f, 0f, 1f);
		eyeColor = effectColor;
		if (ModManager.MSC && Region.IsRubiconRegion(room.world.name))
		{
			GWmode = false;
		}
		else
		{
			GWmode = room.world.name.ToLowerInvariant() == "gw" || (ModManager.MSC && room.world.name.ToLowerInvariant() == "cl");
		}
	}

	public void ShortcutsReady()
	{
	}

	public static bool SentientRotMode(Room rm)
	{
		if (ModManager.Watcher && rm != null && rm.game.IsStorySession)
		{
			if (rm.game.GetStorySession.saveStateNumber == WatcherEnums.SlugcatStatsName.Watcher)
			{
				return true;
			}
			if (Region.IsSentientRotRegion(rm.world.name))
			{
				return true;
			}
			if (rm.world.regionState != null && rm.world.regionState.sentientRotProgression.ContainsKey(rm.abstractRoom.name))
			{
				return true;
			}
		}
		return false;
	}

	public void AIMapReady()
	{
		bottomLeft = new IntVector2(int.MaxValue, int.MaxValue);
		topRight = new IntVector2(int.MinValue, int.MinValue);
		tiles = new List<IntVector2>();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((int)places[0].pos.x + (int)places[0].pos.y);
		for (int i = 0; i < places.Count; i++)
		{
			for (int j = room.GetTilePosition(places[i].pos).x - (int)((places[i].data as PlacedObject.ResizableObjectData).Rad / 20f) - 1; j <= room.GetTilePosition(places[i].pos).x + (int)((places[i].data as PlacedObject.ResizableObjectData).Rad / 20f) + 1; j++)
			{
				for (int k = room.GetTilePosition(places[i].pos).y - (int)((places[i].data as PlacedObject.ResizableObjectData).Rad / 20f) - 1; k <= room.GetTilePosition(places[i].pos).y + (int)((places[i].data as PlacedObject.ResizableObjectData).Rad / 20f) + 1; k++)
				{
					if (room.GetTile(j, k).Solid || !Custom.DistLess(room.MiddleOfTile(j, k), places[i].pos, (places[i].data as PlacedObject.ResizableObjectData).Rad) || tiles.Contains(new IntVector2(j, k)))
					{
						continue;
					}
					bool flag = false;
					for (int l = 0; l < 8; l++)
					{
						if (flag)
						{
							break;
						}
						if (room.GetTile(j + Custom.eightDirections[l].x, k + Custom.eightDirections[l].y).Solid)
						{
							flag = true;
						}
					}
					if (flag)
					{
						tiles.Add(new IntVector2(j, k));
						if (j < bottomLeft.x)
						{
							bottomLeft.x = j;
						}
						if (k < bottomLeft.y)
						{
							bottomLeft.y = k;
						}
						if (j > topRight.x)
						{
							topRight.x = j;
						}
						if (k > topRight.y)
						{
							topRight.y = k;
						}
					}
				}
			}
		}
		directions = new Vector2[topRight.x - bottomLeft.x + 1, topRight.y - bottomLeft.y + 1];
		bulbs = new List<Bulb>[topRight.x - bottomLeft.x + 1, topRight.y - bottomLeft.y + 1];
		for (int m = 0; m < tiles.Count; m++)
		{
			Vector2 vector = new Vector2(0f, 0f);
			for (int n = 1; n < 3; n++)
			{
				for (int num = 0; num < 8; num++)
				{
					if (room.GetTile(tiles[m] + Custom.eightDirections[num] * n).Solid)
					{
						vector -= Custom.eightDirections[num].ToVector2().normalized / n;
					}
					else
					{
						vector += Custom.eightDirections[num].ToVector2().normalized / n;
					}
				}
			}
			vector.Normalize();
			directions[tiles[m].x - bottomLeft.x, tiles[m].y - bottomLeft.y] = vector;
		}
		for (int num2 = 0; num2 < tiles.Count; num2++)
		{
			Vector2 vector2 = new Vector2(0f, 0f);
			for (int num3 = 1; num3 < 4; num3++)
			{
				for (int num4 = 0; num4 < 8; num4++)
				{
					if (!room.GetTile(tiles[num2] + Custom.eightDirections[num4] * num3).Solid)
					{
						if (Occupied(tiles[num2] + Custom.eightDirections[num4] * num3))
						{
							vector2 -= Custom.eightDirections[num4].ToVector2().normalized / num3;
						}
						else
						{
							vector2 += Custom.eightDirections[num4].ToVector2().normalized / num3;
						}
					}
				}
			}
			directions[tiles[num2].x - bottomLeft.x, tiles[num2].y - bottomLeft.y] = (directions[tiles[num2].x - bottomLeft.x, tiles[num2].y - bottomLeft.y] + vector2 * 0.15f).normalized;
		}
		totalSprites = 0;
		for (int num5 = 0; num5 < tiles.Count; num5++)
		{
			bulbs[tiles[num5].x - bottomLeft.x, tiles[num5].y - bottomLeft.y] = new List<Bulb>();
			CustomRotData customRotData = GetCustomRotData(tiles[num5]);
			float num6 = customRotData?.density ?? 1f;
			for (int num7 = UnityEngine.Random.Range(1, 1 + (int)(num6 * Mathf.Lerp(1f, 3f, CorruptionLevel(tiles[num5])))); num7 >= 0; num7--)
			{
				bool flag2 = num7 == 0;
				if (flag2 && customRotData != null && UnityEngine.Random.value > customRotData.darknessChance)
				{
					flag2 = false;
				}
				Bulb item = new Bulb(this, totalSprites, flag2, tiles[num5]);
				allBulbs.Add(item);
				bulbs[tiles[num5].x - bottomLeft.x, tiles[num5].y - bottomLeft.y].Add(item);
				totalSprites += bulbs[tiles[num5].x - bottomLeft.x, tiles[num5].y - bottomLeft.y][bulbs[tiles[num5].x - bottomLeft.x, tiles[num5].y - bottomLeft.y].Count - 1].totalSprites;
			}
		}
		for (int num8 = 0; num8 < room.roomSettings.placedObjects.Count; num8++)
		{
			if (!room.roomSettings.placedObjects[num8].active)
			{
				continue;
			}
			if (room.roomSettings.placedObjects[num8].type == PlacedObject.Type.CorruptionTube)
			{
				ClimbableCorruptionTube climbableCorruptionTube = new ClimbableCorruptionTube(room, this, totalSprites, room.roomSettings.placedObjects[num8]);
				room.AddObject(climbableCorruptionTube);
				climbTubes.Add(climbableCorruptionTube);
				totalSprites += climbableCorruptionTube.graphic.sprites;
			}
			else if (room.roomSettings.placedObjects[num8].type == PlacedObject.Type.StuckDaddy)
			{
				Vector2 pos = room.roomSettings.placedObjects[num8].pos;
				AbstractCreature abstractCreature = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DaddyLongLegs), null, room.GetWorldCoordinate(pos), new EntityID(-1, room.abstractRoom.index * 1000 + num8));
				abstractCreature.destroyOnAbstraction = true;
				abstractCreature.ignoreCycle = true;
				if (ModManager.MSC)
				{
					abstractCreature.saveCreature = false;
				}
				room.abstractRoom.AddEntity(abstractCreature);
				abstractCreature.RealizeInRoom();
				DaddyLongLegs obj = abstractCreature.realizedCreature as DaddyLongLegs;
				obj.stuckPos = room.roomSettings.placedObjects[num8];
				DaddyRestraint daddyRestraint = new DaddyRestraint(obj, this, totalSprites, room.roomSettings.placedObjects[num8]);
				room.AddObject(daddyRestraint);
				restrainedDaddies.Add(daddyRestraint);
				totalSprites += daddyRestraint.graphic.sprites;
			}
			else if (ModManager.DLCShared && room.roomSettings.placedObjects[num8].type == DLCSharedEnums.PlacedObjectType.RotFlyPaper)
			{
				NeuronFilledLeg neuronFilledLeg = new NeuronFilledLeg(this, totalSprites, room.roomSettings.placedObjects[num8].pos, room.roomSettings.placedObjects[num8].pos + (room.roomSettings.placedObjects[num8].data as PlacedObject.ResizableObjectData).handlePos, room.roomSettings.placedObjects[num8].data as PlacedObject.ResizableObjectData);
				room.AddObject(neuronFilledLeg);
				neuronLegs.Add(neuronFilledLeg);
				totalSprites += neuronFilledLeg.graphic.sprites;
			}
		}
		UnityEngine.Random.state = state;
	}

	public Vector2 Dir(IntVector2 iv)
	{
		return Dir(iv.x, iv.y);
	}

	public Vector2 Dir(int x, int y)
	{
		if (x < bottomLeft.x || x > topRight.x || y < bottomLeft.y || y > topRight.y)
		{
			return new Vector2(0f, 0f);
		}
		return directions[x - bottomLeft.x, y - bottomLeft.y];
	}

	public bool Occupied(IntVector2 iv)
	{
		return Occupied(iv.x, iv.y);
	}

	public bool Occupied(int x, int y)
	{
		if (x < bottomLeft.x || x > topRight.x || y < bottomLeft.y || y > topRight.y)
		{
			return false;
		}
		return directions[x - bottomLeft.x, y - bottomLeft.y].magnitude > 0f;
	}

	public float CorruptionLevel(IntVector2 iv)
	{
		if (ModManager.MMF && room.abstractRoom.singleRealizedRoom)
		{
			return 0f;
		}
		return CorruptionLevel(room.MiddleOfTile(iv));
	}

	public float CorruptionLevel(Vector2 testPos)
	{
		float num = 0f;
		for (int i = 0; i < places.Count; i++)
		{
			if (Custom.DistLess(testPos, places[i].pos, (places[i].data as PlacedObject.ResizableObjectData).Rad))
			{
				float a = Mathf.InverseLerp((places[i].data as PlacedObject.ResizableObjectData).Rad, 0f, Vector2.Distance(testPos, places[i].pos));
				a = Mathf.Lerp(a, Mathf.InverseLerp(0f, 200f, (places[i].data as PlacedObject.ResizableObjectData).Rad - Vector2.Distance(testPos, places[i].pos)), 0.5f);
				if (a > num)
				{
					num = a;
				}
			}
		}
		return num;
	}

	public CustomRotData GetCustomRotData(IntVector2 tile)
	{
		Vector2 vector = room.MiddleOfTile(tile);
		PlacedObject placedObject = null;
		float num = 0f;
		for (int i = 0; i < places.Count; i++)
		{
			PlacedObject.ResizableObjectData resizableObjectData = places[i].data as PlacedObject.ResizableObjectData;
			if (Custom.DistLess(vector, places[i].pos, resizableObjectData.Rad))
			{
				float a = Mathf.InverseLerp(resizableObjectData.Rad, 0f, Vector2.Distance(vector, places[i].pos));
				a = Mathf.Lerp(a, Mathf.InverseLerp(0f, 200f, resizableObjectData.Rad - Vector2.Distance(vector, places[i].pos)), 0.5f);
				if (a > num)
				{
					placedObject = places[i];
					num = a;
				}
			}
		}
		return placedObject?.data as CustomRotData;
	}

	public override void Update(bool eu)
	{
		if (ModManager.MSC && room != null && places.Count > 0 && room.PointDeferred(places[0].pos))
		{
			return;
		}
		base.Update(eu);
		foreach (Bulb allBulb in allBulbs)
		{
			allBulb.Update();
		}
		if (room != null && SlugcatStats.AtOrBeforeTimeline(room.game.TimelinePoint, SlugcatStats.Timeline.Artificer) && GWmode)
		{
			effectColor = new Color(0f, 0f, 1f);
			eyeColor = effectColor;
			GWmode = false;
		}
		for (int num = eatCreatures.Count - 1; num >= 0; num--)
		{
			eatCreatures[num].Update();
			if (eatCreatures[num].progression >= 1f || eatCreatures[num].disableCounter > 20 || eatCreatures[num].creature.room != room)
			{
				eatCreatures.RemoveAt(num);
			}
		}
	}

	public void BulbNibbleAtChunk(Bulb bulb, BodyChunk chunk)
	{
		bool flag = false;
		for (int i = 0; i < eatCreatures.Count; i++)
		{
			if (flag)
			{
				break;
			}
			if (eatCreatures[i].creature == chunk.owner)
			{
				eatCreatures[i].BulbInteraction(bulb.eyeStalkPos, bulb.rad);
				flag = true;
			}
		}
		if (!flag && chunk.owner is Creature && !(chunk.owner as Creature).abstractCreature.tentacleImmune && !chunk.owner.slatedForDeletetion && chunk.owner.room == room)
		{
			if (chunk.owner is Player && room.game.IsStorySession && room.game.GetStorySession.saveState.miscWorldSaveData.hasVoidWeaverAbility)
			{
				(chunk.owner as Player).SetHaloDisplayTime(20);
			}
			else
			{
				eatCreatures.Add(new EatenCreature(chunk.owner as Creature, bulb.eyeStalkPos, bulb.rad));
			}
		}
	}

	public void NoiseInRoom(InGameNoise noise)
	{
		foreach (Bulb allBulb in allBulbs)
		{
			if (Custom.DistLess(noise.pos, allBulb.pos, noise.strength))
			{
				allBulb.HeardNoise(noise.pos);
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		if (ModManager.MSC && rCam.room.world.region != null && Region.IsRubiconRegion(rCam.room.world.name))
		{
			effectColor = RainWorld.SaturatedGold;
			eyeColor = effectColor;
		}
		else if (rCam.room.world.region != null)
		{
			effectColor = rCam.room.world.region.regionParams.corruptionEffectColor;
			eyeColor = rCam.room.world.region.regionParams.corruptionEyeColor;
		}
		else
		{
			effectColor = new Color(0f, 0f, 1f);
			eyeColor = effectColor;
		}
		foreach (Bulb allBulb in allBulbs)
		{
			allBulb.InitiateSprites(sLeaser, rCam);
		}
		for (int i = 0; i < climbTubes.Count; i++)
		{
			climbTubes[i].graphic.InitiateSprites(sLeaser, rCam);
		}
		for (int j = 0; j < restrainedDaddies.Count; j++)
		{
			restrainedDaddies[j].graphic.InitiateSprites(sLeaser, rCam);
		}
		if (ModManager.MSC)
		{
			for (int k = 0; k < neuronLegs.Count; k++)
			{
				neuronLegs[k].graphic.InitiateSprites(sLeaser, rCam);
			}
		}
		AddToContainer(sLeaser, rCam, null);
		ApplyPalette(sLeaser, rCam, rCam.currentPalette);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (ModManager.MSC && room != null && places.Count > 0 && room.PointDeferred(places[0].pos))
		{
			return;
		}
		if (sLeaser.sprites.Length == 0 && bulbs != null)
		{
			InitiateSprites(sLeaser, rCam);
		}
		foreach (Bulb allBulb in allBulbs)
		{
			allBulb.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		for (int i = 0; i < climbTubes.Count; i++)
		{
			climbTubes[i].graphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		for (int j = 0; j < restrainedDaddies.Count; j++)
		{
			restrainedDaddies[j].graphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
		}
		if (ModManager.MSC)
		{
			for (int k = 0; k < neuronLegs.Count; k++)
			{
				neuronLegs[k].graphic.DrawSprite(sLeaser, rCam, timeStacker, camPos);
			}
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (sLeaser.sprites.Length == 0)
		{
			return;
		}
		effectColorA = palette.texture.GetPixel(31, 5);
		effectColorB = palette.texture.GetPixel(31, 3);
		foreach (Bulb allBulb in allBulbs)
		{
			allBulb.ApplyPalette(sLeaser, rCam, palette);
		}
		for (int i = 0; i < climbTubes.Count; i++)
		{
			climbTubes[i].graphic.ApplyPalette(sLeaser, rCam, palette);
		}
		for (int j = 0; j < restrainedDaddies.Count; j++)
		{
			restrainedDaddies[j].graphic.ApplyPalette(sLeaser, rCam, palette);
		}
		if (ModManager.MSC)
		{
			for (int k = 0; k < neuronLegs.Count; k++)
			{
				neuronLegs[k].graphic.ApplyPalette(sLeaser, rCam, palette);
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
		}
		foreach (Bulb allBulb in allBulbs)
		{
			allBulb.AddBodyToContainerP1(sLeaser, rCam);
			allBulb.AddBodyToContainerP2(sLeaser, rCam);
			allBulb.AddBodyToContainerP3(sLeaser, rCam);
			allBulb.AddEyeToContainer(sLeaser, rCam);
		}
		for (int j = 0; j < climbTubes.Count; j++)
		{
			for (int k = 0; k < climbTubes[j].graphic.sprites; k++)
			{
				sLeaser.sprites[climbTubes[j].graphic.firstSprite + k].RemoveFromContainer();
				rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[climbTubes[j].graphic.firstSprite + k]);
			}
		}
		for (int l = 0; l < restrainedDaddies.Count; l++)
		{
			for (int m = 0; m < restrainedDaddies[l].graphic.sprites; m++)
			{
				sLeaser.sprites[restrainedDaddies[l].graphic.firstSprite + m].RemoveFromContainer();
				rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[restrainedDaddies[l].graphic.firstSprite + m]);
			}
		}
		if (!ModManager.MSC)
		{
			return;
		}
		for (int n = 0; n < neuronLegs.Count; n++)
		{
			for (int num = 0; num < neuronLegs[n].graphic.sprites; num++)
			{
				sLeaser.sprites[neuronLegs[n].graphic.firstSprite + num].RemoveFromContainer();
				rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[neuronLegs[n].graphic.firstSprite + num]);
			}
		}
	}
}
