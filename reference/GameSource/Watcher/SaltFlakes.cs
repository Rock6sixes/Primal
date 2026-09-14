using RWCustom;
using UnityEngine;

namespace Watcher;

public class SaltFlakes : UpdatableAndDeletable
{
	public class SaltFlake : CosmeticSprite
	{
		public Vector2 driftDir;

		public float lastHealth;

		public float health;

		public float depth;

		public bool stuck;

		public float lastRotation;

		public float rotation;

		public Vector3 individual;

		public Vector2 windDir;

		public bool InPlayLayer => depth == 0f;

		public SaltFlake(Vector2 pos, bool playLayerOnly)
		{
			base.pos = pos;
			lastPos = pos;
			driftDir = Custom.DegToVec(Random.value * 360f);
			lastHealth = 1f;
			health = 1f;
			individual = new Vector3(Random.value, Random.value, Random.value);
			if (playLayerOnly || Random.value < 0.2f)
			{
				depth = 0f;
			}
			else if (Random.value < 0.5f)
			{
				depth = -0.5f * Random.value;
			}
			else
			{
				depth = Mathf.Pow(Random.value, 1.5f) * 5f;
			}
		}

		public override void Update(bool eu)
		{
			evenUpdate = eu;
			lastRotation = rotation;
			vel *= 0.9f;
			driftDir = (driftDir + Custom.DegToVec(Random.value * 360f) * 0.2f).normalized;
			if (!stuck)
			{
				vel += driftDir * 0.1f;
				rotation -= vel.x * 4f;
			}
			vel.y += 0.2f;
			lastPos = pos;
			windDir = room.windDirection;
			Vector2 vector = ((!stuck) ? (windDir * 0.05f) : Vector2.zero);
			pos += (vel + vector) / (depth + 1f);
			lastHealth = health;
			if (InPlayLayer)
			{
				if (room.GetTile(pos).Solid)
				{
					if (!room.GetTile(lastPos).Solid)
					{
						pos = lastPos;
					}
					health -= 1f / 30f;
					if (lastHealth < 0f)
					{
						Destroy();
						return;
					}
				}
				else
				{
					health = Mathf.Min(1f, health + 1f / 70f);
				}
				if (room.PointSubmerged(pos))
				{
					pos.y = room.FloatWaterLevel(pos);
				}
			}
			CheckForDestruction();
		}

		public void CheckForDestruction()
		{
			if (windDir == Vector2.zero)
			{
				if (pos.y > room.PixelHeight + 300f)
				{
					Destroy();
				}
			}
			else if (!new FloatRect(-300f, -150f, room.PixelWidth + 300f, room.PixelHeight + 150f).Vector2Inside(pos))
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[(!(depth < 0f)) ? 1 : 2];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["SaltFlake"];
			sLeaser.sprites[0].scaleX = 1f + individual.z * 0.3f;
			if (depth < 0f)
			{
				sLeaser.sprites[1] = new FSprite("Futile_White");
				sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["SaltFlakeShadow"];
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastHealth, health, timeStacker) * 20f), 0.2f) / (1f + depth * 0.4f) * sizeMultiplier;
			sLeaser.sprites[0].scaleX = num * (1f + individual.z * 0.3f);
			sLeaser.sprites[0].scaleY = num;
			Color color = new Color(individual.x, individual.y, 0f, (depth > 0f) ? 0f : 1f);
			sLeaser.sprites[0].color = color;
			float num2 = Mathf.Lerp(lastRotation, rotation, timeStacker);
			sLeaser.sprites[0].rotation = num2;
			if (sLeaser.sprites.Length == 2)
			{
				sLeaser.sprites[1].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x + Mathf.InverseLerp(0f, -0.5f, depth) * rCam.room.lightAngle.x * 15f;
				sLeaser.sprites[1].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y + Mathf.InverseLerp(0f, -0.5f, depth) * (0f - rCam.room.lightAngle.y) * 15f;
				sLeaser.sprites[1].color = color;
				sLeaser.sprites[1].rotation = num2;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			newContatiner = rCam.ReturnFContainer(InPlayLayer ? "Items" : "Foreground");
			sLeaser.sprites[0].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[0]);
			if (sLeaser.sprites.Length == 2)
			{
				sLeaser.sprites[1].RemoveFromContainer();
				rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[1]);
			}
		}
	}

	public class SaltFlakeStalk : CosmeticSprite
	{
		public SaltFlake saltFlake;

		public float stalkLength;

		public int segs;

		public Vector2 stalkDir;

		public Vector2 tip;

		public Vector2 lastTip;

		public float straightStalk;

		public float lastStraightStalk;

		public SaltFlakeStalk(SaltFlake saltFlake, Room room)
		{
			this.saltFlake = saltFlake;
			base.room = room;
			pos = new Vector2(saltFlake.pos.x, room.MiddleOfTile(saltFlake.pos).y - 11f);
			if (!room.GetTile(pos).Solid)
			{
				pos.y -= 20f;
			}
			lastPos = pos;
			stalkLength = Vector2.Distance(pos, saltFlake.pos);
			segs = Custom.IntClamp((int)(stalkLength / 2f), 2, 15);
			stalkDir = Custom.DegToVec(Mathf.Lerp(-45f, 45f, room.game.SeededRandom((int)pos.x + (int)pos.y + room.abstractRoom.index)));
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (saltFlake != null)
			{
				Vector2 vector = pos + new Vector2(0f, stalkLength);
				saltFlake.vel += (vector - saltFlake.pos) * 0.05f;
				if (!Custom.DistLess(saltFlake.pos, pos, stalkLength))
				{
					Vector2 vector2 = Custom.DirVec(saltFlake.pos, pos);
					float num = Vector2.Distance(saltFlake.pos, pos);
					saltFlake.pos += (num - stalkLength) * vector2;
					saltFlake.vel += (num - stalkLength) * vector2;
				}
				if (!room.ViewedByAnyCamera(saltFlake.pos, 100f))
				{
					return;
				}
				for (int i = 0; i < room.physicalObjects.Length; i++)
				{
					if (saltFlake == null)
					{
						break;
					}
					for (int j = 0; j < room.physicalObjects[i].Count; j++)
					{
						if (saltFlake == null)
						{
							break;
						}
						for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
						{
							if (saltFlake == null)
							{
								break;
							}
							if (Custom.DistLess(room.physicalObjects[i][j].bodyChunks[k].pos, saltFlake.pos, room.physicalObjects[i][j].bodyChunks[k].rad + 3f))
							{
								Vector2 vector3 = room.physicalObjects[i][j].bodyChunks[k].pos - room.physicalObjects[i][j].bodyChunks[k].lastPos;
								saltFlake.vel = Vector2.Lerp(saltFlake.vel, vector3, Random.value);
								if (Random.value < Mathf.InverseLerp(1f, 8f, vector3.magnitude))
								{
									saltFlake.vel += Custom.DirVec(room.physicalObjects[i][j].bodyChunks[k].pos, saltFlake.pos) * (vector3.magnitude * 0.5f);
									saltFlake.vel += vector3 * 0.5f;
									saltFlake.stuck = false;
									tip = saltFlake.pos;
									lastTip = saltFlake.pos;
									saltFlake = null;
								}
							}
						}
					}
				}
			}
			else
			{
				lastTip = tip;
				tip = Vector2.Lerp(tip, pos, 0.1f);
				lastStraightStalk = straightStalk;
				straightStalk = Mathf.Min(1f, straightStalk + Random.value / 15f);
				if (Custom.DistLess(tip, pos, 1f))
				{
					Destroy();
				}
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segs, pointyTip: false, customColor: true);
			base.InitiateSprites(sLeaser, rCam);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = ((saltFlake == null) ? Vector2.Lerp(lastTip, tip, timeStacker) : Vector2.Lerp(saltFlake.lastPos, saltFlake.pos, timeStacker));
			float num = 0.5f;
			Vector2 vector2 = pos;
			float num2 = 1f - Mathf.Lerp(lastStraightStalk, straightStalk, timeStacker);
			for (int i = 0; i < segs; i++)
			{
				float f = (float)i / (float)(segs - 1);
				Vector2 vector3 = stalkDir * (stalkLength * 0.5f * num2);
				Vector2 vector4 = Custom.Bezier(pos, pos + vector3, vector, vector - vector3, f);
				Vector2 normalized = (vector2 - vector4).normalized;
				Vector2 vector5 = Custom.PerpendicularVector(normalized);
				float num3 = Vector2.Distance(vector2, vector4) / 5f;
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector2 - normalized * num3 - vector5 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector2 - normalized * num3 + vector5 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector4 + normalized * num3 - vector5 * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector4 + normalized * num3 + vector5 * num - camPos);
				vector2 = vector4;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			Color pixel = palette.texture.GetPixel(5, 2);
			for (int i = 0; i < segs * 4; i++)
			{
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = Color.Lerp(pixel, new Color(1f, 1f, 1f), (float)i / (float)(segs * 4 - 1));
			}
			base.ApplyPalette(sLeaser, rCam, palette);
		}
	}

	public static float amountMultiplier = 2f;

	public static float sizeMultiplier = 0.5f;

	public RoomSettings.RoomEffect effect;

	public float wait;

	public float SpawnChance => effect.amount * (ModManager.Watcher ? (1f - Watcher.cfgReduceFallingParticles.Value) : 1f) * Mathf.Pow(1f - room.world.rainCycle.ProximityToMiddleOfCycle, 0.15f) * (1f - Mathf.Pow(room.game.globalRain.Intensity, 0.1f));

	public SaltFlakes(RoomSettings.RoomEffect effect, Room rm)
	{
		this.effect = effect;
		room = rm;
		for (int i = 0; (float)i < (float)(rm.TileWidth * rm.TileHeight * 20) * SpawnChance * amountMultiplier / 1000f; i++)
		{
			IntVector2 pos = new IntVector2(Random.Range(0, rm.TileWidth), Random.Range(0, rm.TileHeight));
			if (!rm.GetTile(pos).Solid)
			{
				rm.AddObject(new SaltFlake(rm.MiddleOfTile(pos) + new Vector2(-10f + 20f * Random.value, -10f + 20f * Random.value), playLayerOnly: false));
			}
		}
		int num = rm.abstractRoom.index;
		for (int j = 0; j < rm.roomSettings.placedObjects.Count; j++)
		{
			if (!(rm.roomSettings.placedObjects[j].type == PlacedObject.Type.DandelionPatch))
			{
				continue;
			}
			PlacedObject.ResizableObjectData resizableObjectData = rm.roomSettings.placedObjects[j].data as PlacedObject.ResizableObjectData;
			IntVector2 tilePosition = rm.GetTilePosition(rm.roomSettings.placedObjects[j].pos);
			for (int k = tilePosition.x - (int)(resizableObjectData.Rad / 20f); k <= tilePosition.x + (int)(resizableObjectData.Rad / 20f); k++)
			{
				for (int l = tilePosition.y - (int)(resizableObjectData.Rad / 20f); l <= tilePosition.y + (int)(resizableObjectData.Rad / 20f); l++)
				{
					if (k <= 0 || k >= rm.TileWidth || l <= 0 || l >= rm.TileHeight || rm.GetTile(k, l).Solid)
					{
						continue;
					}
					IntVector2 intVector = new IntVector2(k, l);
					if (rm.GetTile(intVector).Solid || rm.GetTile(intVector + new IntVector2(0, 1)).Solid || !rm.GetTile(intVector + new IntVector2(0, -1)).Solid || (rm.GetTile(intVector + new IntVector2(-1, 0)).Solid && rm.GetTile(intVector + new IntVector2(1, 0)).Solid))
					{
						continue;
					}
					float num2 = Mathf.InverseLerp(resizableObjectData.Rad, resizableObjectData.Rad / 3f, Vector2.Distance(rm.roomSettings.placedObjects[j].pos, rm.MiddleOfTile(intVector)));
					num2 *= Custom.LerpMap(resizableObjectData.Rad, 40f, 200f, 0.5f, 1f);
					for (int m = 0; (float)m < 3f * num2; m++)
					{
						if (rm.game.SeededRandom(num) < Mathf.Pow(num2, 0.5f))
						{
							float num3 = 0.1f + 0.9f * Mathf.Pow(rm.game.SeededRandom(num + 1000), Mathf.Lerp(0.5f, 2.5f, num2));
							SaltFlake saltFlake = new SaltFlake(rm.MiddleOfTile(intVector) + new Vector2(-10f + rm.game.SeededRandom(num - 1000) * 20f, -10f + num3 * 30f), playLayerOnly: true)
							{
								stuck = true
							};
							SaltFlakeStalk obj = new SaltFlakeStalk(saltFlake, rm);
							rm.AddObject(obj);
							rm.AddObject(saltFlake);
						}
						num++;
					}
				}
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (wait > 0f)
		{
			wait -= 1f;
			return;
		}
		wait += Mathf.Lerp(30f, 0.4f, effect.amount) / amountMultiplier;
		if (Random.value < SpawnChance)
		{
			SpawnFlakes();
		}
	}

	public void SpawnFlakes()
	{
		Vector2 windDirection = room.windDirection;
		if (windDirection == Vector2.zero)
		{
			room.AddObject(new SaltFlake(new Vector2(Mathf.Lerp(-150f, room.PixelWidth + 150f, Random.value), -100f), playLayerOnly: false));
			return;
		}
		if (windDirection.x > 10f)
		{
			room.AddObject(new SaltFlake(new Vector2(-250f, Mathf.Lerp(-100f, room.PixelHeight + 100f, Random.value)), playLayerOnly: false));
		}
		else if (windDirection.x < 10f)
		{
			room.AddObject(new SaltFlake(new Vector2(room.PixelWidth + 250f, Mathf.Lerp(-100f, room.PixelHeight + 100f, Random.value)), playLayerOnly: false));
		}
		if (windDirection.y > -30f)
		{
			room.AddObject(new SaltFlake(new Vector2(Mathf.Lerp(-150f, room.PixelWidth + 150f, Random.value), -100f), playLayerOnly: false));
		}
		else if (windDirection.y < -50f)
		{
			room.AddObject(new SaltFlake(new Vector2(Mathf.Lerp(-150f, room.PixelWidth + 150f, Random.value), room.PixelHeight + 100f), playLayerOnly: false));
		}
	}
}
