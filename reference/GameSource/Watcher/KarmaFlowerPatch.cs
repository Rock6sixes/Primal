using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class KarmaFlowerPatch : CosmeticSprite, INotifyWhenRoomIsReady
{
	public struct Flower
	{
		public Vector2 idealPos;

		public Vector2 root;

		public Vector2 direction;

		public float radius;

		public float depth;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public Flower(Vector2 pos, Vector2 root, Vector2 direction, float radius, float depth)
		{
			idealPos = pos;
			this.root = root;
			this.direction = direction;
			this.radius = radius;
			this.depth = depth;
			this.pos = pos;
			lastPos = pos;
			vel = Vector2.zero;
		}

		public Vector2 DrawPos(float timeStacker)
		{
			return Vector2.Lerp(lastPos, pos, timeStacker);
		}
	}

	public class KarmaFlowerPatchData : PlacedObject.ResizableObjectData
	{
		public float tilt = 0.5f;

		public Vector2 panelPos;

		public float glowStrength = 0.25f;

		public float glowRadius = 0.5f;

		public Vector2 Direction => handlePos.normalized * tilt;

		public KarmaFlowerPatchData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			tilt = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			if (array.Length > 6)
			{
				panelPos.x = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
				panelPos.y = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
				glowStrength = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
				glowRadius = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
			}
			else
			{
				unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 3);
			}
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", handlePos.x, handlePos.y, tilt, panelPos.x, panelPos.y, glowStrength, glowRadius);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public Flower[] flowers = new Flower[0];

	public LightSource[] lights = new LightSource[0];

	public float[] lightAlphas = new float[0];

	public const int spritesPerFlower = 6;

	public const int lightsPerFlower = 3;

	public const int stemSprite = 0;

	public const int firstPetalSprite = 1;

	public const int ringSprite = 5;

	public Color stalkColor;

	public Color petalColor;

	public float windScroll;

	public float shrinkFactor;

	public float lastShrinkFactor;

	public int EffectSprite(int flower, int index)
	{
		return flowers.Length * (6 + index) + flower;
	}

	public override void Update(bool eu)
	{
		lastShrinkFactor = shrinkFactor;
		base.Update(eu);
		RoomSettings.RoomEffect effect = room.roomSettings.GetEffect(WatcherEnums.RoomEffectType.FlowerWind);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (effect != null)
		{
			num = (effect.GetAmount(0) * 2f - 1f) * 2f;
			num2 = (effect.GetAmount(1) * 2f - 1f) * 2f;
			num3 = effect.GetAmount(2) * 5f * ((num + num2 > 0f) ? (-1f) : 1f);
		}
		windScroll += num3 / 40f;
		for (int i = 0; i < flowers.Length; i++)
		{
			ref Flower reference = ref flowers[i];
			reference.lastPos = reference.pos;
			reference.pos += reference.vel;
			reference.vel *= 0.8f;
			reference.vel.x += Mathf.Lerp(num, num2, Mathf.PerlinNoise(reference.root.x / 100f + windScroll, windScroll / 10f));
			reference.vel += (reference.idealPos - reference.pos) * 0.1f;
			float num4 = Vector2.Distance(reference.pos, reference.root);
			float num5 = Vector2.Distance(reference.idealPos, reference.root);
			if (num4 > num5)
			{
				Vector2 vector = (num4 - num5) * (reference.root - reference.pos).normalized;
				reference.pos += vector;
				reference.vel += vector;
			}
		}
		for (int j = 0; j < lights.Length; j++)
		{
			lights[j].setAlpha = lightAlphas[j] * (1f - shrinkFactor);
		}
		List<PhysicalObject>[] physicalObjects = room.physicalObjects;
		for (int k = 0; k < physicalObjects.Length; k++)
		{
			foreach (PhysicalObject item in physicalObjects[k])
			{
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					if (bodyChunk.vel.magnitude < 0.1f)
					{
						continue;
					}
					for (int m = 0; m < flowers.Length; m++)
					{
						if (Custom.DistLess(flowers[m].pos, bodyChunk.pos, bodyChunk.rad + 5f))
						{
							flowers[m].vel += bodyChunk.vel * 0.02f;
						}
					}
				}
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[flowers.Length * 9];
		for (int i = 0; i < flowers.Length; i++)
		{
			int num = i * 6;
			sLeaser.sprites[num] = TriangleMesh.MakeLongMeshAtlased(3, pointyTip: false, customColor: true);
			for (int j = 0; j < 4; j++)
			{
				sLeaser.sprites[num + 1 + j] = new FSprite("KarmaPetal", quadType: false)
				{
					anchorY = 0f
				};
			}
			sLeaser.sprites[num + 5] = new FSprite("FoodCircleA", quadType: false);
			for (int k = 0; k < 3; k++)
			{
				sLeaser.sprites[EffectSprite(i, k)] = new FSprite("Futile_White");
			}
			sLeaser.sprites[EffectSprite(i, 0)].shader = rCam.room.game.rainWorld.Shaders["FlatLightBehindTerrain"];
			sLeaser.sprites[EffectSprite(i, 1)].shader = rCam.room.game.rainWorld.Shaders["FlatLightBehindTerrain"];
			sLeaser.sprites[EffectSprite(i, 2)].shader = rCam.room.game.rainWorld.Shaders["GoldenGlow"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < flowers.Length * 6; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
		FContainer fContainer = rCam.ReturnFContainer("Foreground");
		FContainer fContainer2 = rCam.ReturnFContainer("GrabShaders");
		for (int j = 0; j < flowers.Length; j++)
		{
			fContainer.AddChild(sLeaser.sprites[EffectSprite(j, 0)]);
			fContainer.AddChild(sLeaser.sprites[EffectSprite(j, 1)]);
			fContainer2.AddChild(sLeaser.sprites[EffectSprite(j, 2)]);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (sLeaser.sprites.Length != flowers.Length * 9)
		{
			sLeaser.RemoveAllSpritesFromContainer();
			InitiateSprites(sLeaser, rCam);
		}
		for (int i = 0; i < flowers.Length; i++)
		{
			Flower flower = flowers[i];
			int num = i * 6;
			bool flag = !rCam.PositionCurrentlyVisible(flower.pos, 20f, widescreen: true);
			for (int j = 0; j < 6; j++)
			{
				sLeaser.sprites[num + j].isVisible = !flag;
			}
			for (int k = 0; k < 3; k++)
			{
				sLeaser.sprites[EffectSprite(i, k)].isVisible = !flag;
			}
			if (flag)
			{
				continue;
			}
			float num2 = Mathf.Lerp(lastShrinkFactor, shrinkFactor, timeStacker);
			float num3 = flower.radius * (1f - num2);
			Vector2 root = flower.root;
			Vector2 vector = Vector2.Lerp(flower.DrawPos(timeStacker), flower.root, num2);
			Vector2 ps = vector + (vector - root).normalized * (num3 * 0.75f);
			root = rCam.ApplyDepth(root, flower.depth);
			vector = rCam.ApplyDepth(vector, flower.depth);
			ps = rCam.ApplyDepth(ps, flower.depth);
			TriangleMesh triangleMesh = sLeaser.sprites[num] as TriangleMesh;
			for (int l = 0; l < triangleMesh.vertices.Length; l += 2)
			{
				float t = (float)l / (float)(triangleMesh.vertices.Length - 2);
				Vector2 vector2 = Vector2.Lerp(root, vector, t);
				if (l == 0)
				{
					vector2.y -= 5f;
				}
				Vector2 vector3 = Custom.PerpendicularVector(root, vector);
				float num4 = 0.75f;
				triangleMesh.verticeColors[l] = (triangleMesh.verticeColors[l + 1] = Color.Lerp(stalkColor, petalColor, t));
				triangleMesh.MoveVertice(l, vector2 + vector3 * num4 - camPos);
				triangleMesh.MoveVertice(l + 1, vector2 - vector3 * num4 - camPos);
			}
			float x = flower.direction.x;
			float y = flower.direction.y;
			Vector2 direction = flower.direction;
			Vector3 vector4 = new Vector3(x, y, Mathf.Sqrt(1f - direction.sqrMagnitude));
			Vector2 vector5 = Vector3.Cross(new Vector3(0f, 0f, 1f), vector4).normalized;
			Vector2 vector6 = Vector3.Cross(vector4, vector5);
			for (int m = 0; m < 4; m++)
			{
				FSprite obj = sLeaser.sprites[num + m + 1];
				float f = (float)m * (float)Math.PI / 2f + (float)i;
				Vector2 vector7 = ps + (vector6 * Mathf.Sin(f) + vector5 * Mathf.Cos(f)) * num3;
				obj.SetPosition(vector - camPos);
				obj.rotation = Custom.AimFromOneVectorToAnother(vector, vector7);
				obj.scaleX = 0.375f;
				obj.scaleY = (vector7 - vector).magnitude / 20f;
				obj.color = petalColor;
			}
			FSprite obj2 = sLeaser.sprites[num + 5];
			obj2.rotation = Custom.VecToDeg(vector6);
			obj2.width = num3 * 2f;
			obj2.height = num3 * vector6.magnitude * 2f;
			obj2.SetPosition(ps - camPos);
			obj2.color = petalColor;
			float num5 = 1f - num2;
			sLeaser.sprites[EffectSprite(i, 0)].scale = 75f * num5 / 16f;
			sLeaser.sprites[EffectSprite(i, 0)].alpha = 0.4f * num5;
			sLeaser.sprites[EffectSprite(i, 0)].color = Custom.HSL2RGB(RainWorld.AntiGold.hue, 0.6f, 0.2f);
			sLeaser.sprites[EffectSprite(i, 1)].scale = 40f * num5 / 16f;
			sLeaser.sprites[EffectSprite(i, 1)].alpha = 0.5f * num5;
			sLeaser.sprites[EffectSprite(i, 1)].color = petalColor;
			sLeaser.sprites[EffectSprite(i, 2)].scale = 40f * num5 / 16f;
			sLeaser.sprites[EffectSprite(i, 2)].alpha = 0.8f * num5;
			for (int n = 0; n < 3; n++)
			{
				sLeaser.sprites[EffectSprite(i, n)].SetPosition(ps - camPos);
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		stalkColor = Color.Lerp(palette.blackColor, palette.fogColor, 0.3f);
		petalColor = RainWorld.GoldRGB;
	}

	public void AIMapReady()
	{
	}

	public void ShortcutsReady()
	{
		PlaceFlowers();
	}

	public void PlaceFlowers()
	{
		LightSource[] array = lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Destroy();
		}
		List<PlacedObject> list = new List<PlacedObject>();
		List<Flower> list2 = new List<Flower>();
		List<LightSource> list3 = new List<LightSource>();
		int num = 0;
		IntRect intRect = new IntRect(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if (placedObject.type == WatcherEnums.PlacedObjectType.KarmaFlowerPatch && placedObject.active)
			{
				list.Add(placedObject);
				float rad = (placedObject.data as KarmaFlowerPatchData).Rad;
				intRect.left = Mathf.Min(intRect.left, Mathf.FloorToInt((placedObject.pos.x - rad) / 20f));
				intRect.bottom = Mathf.Min(intRect.bottom, Mathf.FloorToInt((placedObject.pos.y - rad) / 20f));
				intRect.right = Mathf.Max(intRect.right, Mathf.FloorToInt((placedObject.pos.x + rad) / 20f));
				intRect.top = Mathf.Max(intRect.top, Mathf.FloorToInt((placedObject.pos.y + rad) / 20f));
				num ^= (int)placedObject.pos.x + room.TileWidth * 20 * (int)placedObject.pos.y;
			}
		}
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(num);
		for (int j = intRect.bottom; j <= intRect.top; j++)
		{
			int num2 = 0;
			for (int k = intRect.left; k <= intRect.right; k++)
			{
				if (!CanSpawnFlowers(k, j))
				{
					continue;
				}
				Vector2 floorPos = GetFloorPos(k, j, 0.5f);
				float num3 = 0f;
				float num4 = 0f;
				Vector2 zero = Vector2.zero;
				float num5 = 0f;
				float num6 = 0f;
				foreach (PlacedObject item in list)
				{
					KarmaFlowerPatchData karmaFlowerPatchData = item.data as KarmaFlowerPatchData;
					float num7 = Mathf.InverseLerp(karmaFlowerPatchData.Rad, 0f, Vector2.Distance(floorPos, item.pos));
					num3 += num7 * (1f - num3);
					num4 += num7;
					zero += num7 * karmaFlowerPatchData.Direction;
					num5 += num7 * karmaFlowerPatchData.glowStrength;
					num6 += num7 * karmaFlowerPatchData.glowRadius;
				}
				zero /= num4;
				num5 /= num4;
				num6 /= num4;
				bool flag = false;
				for (int l = 0; l < 3; l++)
				{
					if (UnityEngine.Random.value < num3)
					{
						flag = true;
						float num8 = Mathf.Lerp(num3, Mathf.Pow(UnityEngine.Random.value, 2f), UnityEngine.Random.value);
						Vector2 floorPos2 = GetFloorPos(k, j, UnityEngine.Random.value);
						Vector2 vector = floorPos2 + Custom.RotateAroundOrigo(new Vector2(0f, 30f * num8 * Custom.ClampedRandomVariation(0.75f, 0.25f, 2f)), UnityEngine.Random.Range(-10f, 10f));
						list2.Add(new Flower(vector, floorPos2, Custom.RotateAroundOrigo(zero, UnityEngine.Random.Range(-25f, 25f)), Mathf.Lerp(4f, 8f, num8), UnityEngine.Random.value));
					}
				}
				if (num2 == 0 && flag && num5 > 0f && num6 > 0f)
				{
					LightSource lightSource = new LightSource(floorPos + new Vector2(0f, 15f), environmentalLight: true, Color.Lerp(RainWorld.SaturatedGold, Color.white, 0.5f), this);
					lightSource.HardSetAlpha(0.4f * num5);
					lightSource.HardSetRad(Mathf.Lerp(80f, 150f, num3) * num6 * 2f);
					room.AddObject(lightSource);
					list3.Add(lightSource);
					num2 = 1;
				}
				if (num2 > 0)
				{
					num2--;
				}
			}
		}
		UnityEngine.Random.state = state;
		list2.Sort(delegate(Flower a, Flower b)
		{
			float depth = a.depth;
			return depth.CompareTo(b.depth);
		});
		flowers = list2.ToArray();
		lights = list3.ToArray();
		lightAlphas = list3.Select((LightSource light) => light.alpha).ToArray();
		for (int m = 0; m < list3.Count; m++)
		{
			list3[m].HardSetAlpha(lightAlphas[m] * (1f - shrinkFactor));
		}
		lastShrinkFactor = shrinkFactor;
	}

	public bool CanSpawnFlowers(int x, int y)
	{
		if (room.GetTile(x, y).Terrain == Room.Tile.TerrainType.Slope)
		{
			Room.SlopeDirection slopeDirection = room.IdentifySlope(x, y);
			if (!(slopeDirection == Room.SlopeDirection.UpLeft))
			{
				return slopeDirection == Room.SlopeDirection.UpRight;
			}
			return true;
		}
		if (!room.HasAnySolid(x, y))
		{
			return room.HasAnySolid(x, y - 1);
		}
		return false;
	}

	public Vector2 GetFloorPos(int x, int y, float t)
	{
		t *= 20f;
		Vector2 vector = new Vector2((float)x * 20f, (float)y * 20f);
		if (room.GetTile(x, y).Terrain == Room.Tile.TerrainType.Slope)
		{
			if (room.IdentifySlope(x, y) == Room.SlopeDirection.UpLeft)
			{
				return vector + new Vector2(t, t);
			}
			return vector + new Vector2(t, 1f - t);
		}
		return vector + new Vector2(t, 0f);
	}
}
