using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Watcher;

public class RippleTree : UpdatableAndDeletable, IDrawable
{
	public struct Cosmetics
	{
		public bool droopLeft;

		public float droopy;

		public float spiralAmount;

		public float spiralCoils;

		public float sinOffset;

		public float sinWidth;

		public float sinDistance;

		public float depth;

		public static Cosmetics GenerateTreeCosmetics(RippleTree tree, float scale)
		{
			Cosmetics result = default(Cosmetics);
			UnityEngine.Random.State state = UnityEngine.Random.state;
			try
			{
				UnityEngine.Random.InitState(tree.seed);
				result.droopLeft = tree.Data.faceLeft;
				float valueOrDefault = tree.Data.droopy.GetValueOrDefault();
				float toA = tree.Data.spiralAmount ?? 1f;
				result.droopy = Mathf.Lerp(UnityEngine.Random.Range(0.7f, 1f), valueOrDefault, Mathf.Pow(scale, UnityEngine.Random.Range(0.15f, 0.25f)));
				result.spiralAmount = Custom.LerpMap(scale, 0.2f, toA, 0f, 1f, UnityEngine.Random.Range(0.2f, 0.4f));
				float toB = UnityEngine.Random.Range(3f, 5f);
				float? num = tree.Data.spiralCoils;
				if (num.HasValue)
				{
					float valueOrDefault2 = num.GetValueOrDefault();
					toB = valueOrDefault2;
				}
				result.spiralCoils = Mathf.Lerp(1f, Custom.LerpMap(tree.SpiralRad, 20f, 100f, 2f, toB), Mathf.Pow(scale, UnityEngine.Random.Range(0.7f, 0.9f)));
				float num2 = UnityEngine.Random.Range(30f, 80f);
				num = tree.Data.sinDist;
				if (num.HasValue)
				{
					float valueOrDefault3 = num.GetValueOrDefault();
					num2 = valueOrDefault3;
				}
				float b = UnityEngine.Random.Range(7f, 10f);
				num = tree.Data.sinWidth;
				if (num.HasValue)
				{
					float valueOrDefault4 = num.GetValueOrDefault();
					b = valueOrDefault4;
				}
				result.sinOffset = UnityEngine.Random.Range(0f, num2);
				num = tree.Data.sinOffset;
				if (num.HasValue)
				{
					float valueOrDefault5 = num.GetValueOrDefault();
					result.sinOffset = valueOrDefault5;
				}
				result.sinDistance = Mathf.Lerp(UnityEngine.Random.Range(12f, 20f), num2, Mathf.Pow(scale, UnityEngine.Random.Range(0.7f, 0.9f)));
				result.sinWidth = Mathf.Lerp(UnityEngine.Random.Range(4f, 6f), b, Mathf.Pow(scale, 1f));
				result.depth = tree.Data.depth ?? 6f;
				return result;
			}
			finally
			{
				UnityEngine.Random.state = state;
			}
		}

		public static Cosmetics GenerateStalkCosmetics(RippleTree tree, float scale)
		{
			Cosmetics result = default(Cosmetics);
			UnityEngine.Random.State state = UnityEngine.Random.state;
			try
			{
				UnityEngine.Random.InitState(tree.seed);
				result.droopLeft = tree.Data.faceLeft;
				float b = UnityEngine.Random.Range(0f, 0.5f);
				float? num = tree.Data.droopy;
				if (num.HasValue)
				{
					float valueOrDefault = num.GetValueOrDefault();
					b = valueOrDefault;
				}
				float b2 = UnityEngine.Random.Range(0.5f, 0.8f);
				num = tree.Data.spiralAmount;
				if (num.HasValue)
				{
					float valueOrDefault2 = num.GetValueOrDefault();
					b2 = valueOrDefault2;
				}
				result.droopy = Mathf.Lerp(UnityEngine.Random.Range(0.5f, 1f), b, Mathf.Pow(scale, UnityEngine.Random.Range(0.15f, 0.25f)));
				result.spiralAmount = Mathf.Lerp(0f, b2, Mathf.Pow(scale, UnityEngine.Random.Range(0.2f, 0.4f)));
				float b3 = UnityEngine.Random.Range(1f, 2.5f);
				num = tree.Data.spiralCoils;
				if (num.HasValue)
				{
					float valueOrDefault3 = num.GetValueOrDefault();
					b3 = valueOrDefault3;
				}
				result.spiralCoils = Mathf.Lerp(1f, b3, Mathf.Pow(scale, UnityEngine.Random.Range(0.7f, 0.9f)));
				float num2 = UnityEngine.Random.Range(16f, 40f);
				num = tree.Data.sinDist;
				if (num.HasValue)
				{
					float valueOrDefault4 = num.GetValueOrDefault();
					num2 = valueOrDefault4;
				}
				float b4 = UnityEngine.Random.Range(4f, 7f);
				num = tree.Data.sinWidth;
				if (num.HasValue)
				{
					float valueOrDefault5 = num.GetValueOrDefault();
					b4 = valueOrDefault5;
				}
				result.sinOffset = UnityEngine.Random.Range(0f, num2);
				num = tree.Data.sinOffset;
				if (num.HasValue)
				{
					float valueOrDefault6 = num.GetValueOrDefault();
					result.sinOffset = valueOrDefault6;
				}
				result.sinDistance = Mathf.Lerp(UnityEngine.Random.Range(10f, 16f), num2, Mathf.Pow(scale, UnityEngine.Random.Range(0.7f, 0.9f)));
				result.sinWidth = Mathf.Lerp(UnityEngine.Random.Range(2f, 4f), b4, Mathf.Pow(scale, 1f));
				result.depth = UnityEngine.Random.Range(0, 9);
				num = tree.Data.depth;
				if (num.HasValue)
				{
					float valueOrDefault7 = num.GetValueOrDefault();
					result.depth = valueOrDefault7;
				}
			}
			finally
			{
				UnityEngine.Random.state = state;
			}
			return result;
		}

		public static Cosmetics GenerateSproutCosmetics(RippleTree tree, float scale)
		{
			Cosmetics result = default(Cosmetics);
			UnityEngine.Random.State state = UnityEngine.Random.state;
			try
			{
				UnityEngine.Random.InitState(tree.seed);
				result.droopLeft = UnityEngine.Random.value > 0.5f;
				result.droopy = Mathf.Lerp(UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(0f, 0.5f), Mathf.Pow(scale, UnityEngine.Random.Range(0.15f, 0.25f)));
				result.spiralAmount = Mathf.Lerp(0f, UnityEngine.Random.Range(0.1f, 0.5f), Mathf.Pow(scale, UnityEngine.Random.Range(0.2f, 0.4f)));
				float b = UnityEngine.Random.Range(1f, 2.5f);
				result.spiralCoils = Mathf.Lerp(1f, b, Mathf.Pow(scale, UnityEngine.Random.Range(0.7f, 0.9f)));
				float num = UnityEngine.Random.Range(16f, 40f);
				float b2 = UnityEngine.Random.Range(4f, 7f);
				result.sinOffset = UnityEngine.Random.Range(0f, num);
				result.sinDistance = Mathf.Lerp(UnityEngine.Random.Range(10f, 16f), num, Mathf.Pow(scale, UnityEngine.Random.Range(0.7f, 0.9f)));
				result.sinWidth = Mathf.Lerp(UnityEngine.Random.Range(2f, 4f), b2, Mathf.Pow(scale, 1f));
				if (tree.waterRoot)
				{
					result.depth = 0f;
				}
				else
				{
					result.depth = UnityEngine.Random.Range(0, 9);
				}
			}
			finally
			{
				UnityEngine.Random.state = state;
			}
			return result;
		}
	}

	public PlacedObject pObj;

	public RippleTree parent;

	public List<RippleTree> sprouts;

	public int wantsToTutorialTimer = -1;

	public bool lastPressed;

	public float glowTimer;

	public float spiralRadVariation;

	public int seed;

	public float scale;

	public float lastScale;

	public float lastUseScale;

	public float stunt;

	public bool waterRoot;

	public Vector2 rootPos;

	public Vector2 goalPos;

	public Vector2 spiralCenter;

	public Spiral spiral;

	public Vector2[] stalk;

	public Vector2[] leftStalk;

	public Vector2[] rightStalk;

	public float stalkRad = 3f;

	public float stalkSegmentDist = 4f;

	public float spiralSegmentDist = 4f;

	public int taperDistance = 8;

	public int baseThickDistance = 20;

	public int glowTaperDistance = 50;

	public bool threeStems;

	public Cosmetics? _cosmetics;

	public Color color;

	public Color stalkColor;

	public Color depthColor;

	public PlacedObject.RippleStalkData Data
	{
		get
		{
			if (pObj != null)
			{
				return pObj.data as PlacedObject.RippleStalkData;
			}
			return null;
		}
	}

	public bool Tree
	{
		get
		{
			if (pObj != null)
			{
				return pObj.type == PlacedObject.Type.RippleTree;
			}
			return false;
		}
	}

	public bool Stalk
	{
		get
		{
			if (pObj != null)
			{
				return pObj.type == PlacedObject.Type.RippleStalk;
			}
			return false;
		}
	}

	public bool Sprout => pObj == null;

	public float GoalScale
	{
		get
		{
			if (Sprout && parent != null)
			{
				return Mathf.Max(0f, parent.GoalScale - stunt) * Mathf.InverseLerp(1000f, 0f, Vector2.Distance(parent.pObj.pos, goalPos));
			}
			if (parent != null)
			{
				return Custom.LerpMap(Vector2.Distance(parent.rootPos, rootPos), 0f, 1000f, parent.GoalScale, 0.1f);
			}
			if (Data == null)
			{
				return 0f;
			}
			if (Data is PlacedObject.RippleTreeData rippleTreeData)
			{
				float val = (room?.world.regionState?.rippleSpawnEggState?.percentEggsCollected).GetValueOrDefault();
				float? testRippleAmount = Data.testRippleAmount;
				if (testRippleAmount.HasValue)
				{
					float valueOrDefault = testRippleAmount.GetValueOrDefault();
					val = valueOrDefault;
				}
				float num = Mathf.InverseLerp(0.5f, 1f, rippleTreeData.sproutThreshold);
				float toA = Custom.LerpMap(rippleTreeData.sproutEnd, 0f, 0.5f, num, 1f);
				float num2 = Mathf.Max(0f, 1f - rippleTreeData.sproutThreshold * 2f);
				float toB = Mathf.Max(num2, Mathf.InverseLerp(1f, 0.5f, rippleTreeData.sproutEnd));
				return Custom.LerpMap(val, num, toA, num2, toB);
			}
			return 1f;
		}
	}

	public float GlowAmount
	{
		get
		{
			if (Sprout)
			{
				return 0f;
			}
			if (parent != null)
			{
				return parent.GlowAmount * Mathf.InverseLerp(5000f, 20f, Vector2.Distance(parent.rootPos, rootPos));
			}
			return Mathf.Pow(Mathf.InverseLerp(0f, (float)TicksToReveal * Mathf.Pow(scale, 0.25f), glowTimer), 0.5f);
		}
	}

	public int TicksToReveal => (int)Custom.LerpMap(scale, 0.25f, 1f, 10f, 60f) * 40;

	public float SpiralRad => Custom.LerpMap(scale, 0.2f, 1f, Tree ? 20f : 10f, goalRad, spiralRadVariation);

	public float StalkLength => Mathf.Lerp(Tree ? 40f : (Stalk ? 20f : 0f), Mathf.Max(1f, goalPos.y - rootPos.y - goalRad * 2f), Mathf.Pow(scale, 0.2f));

	public float goalRad
	{
		get
		{
			if (Data == null)
			{
				return 20f;
			}
			return Data.Rad;
		}
	}

	public Cosmetics cosmetics
	{
		get
		{
			if (!_cosmetics.HasValue)
			{
				if (Sprout)
				{
					_cosmetics = Cosmetics.GenerateSproutCosmetics(this, lastUseScale);
				}
				else if (threeStems)
				{
					_cosmetics = Cosmetics.GenerateTreeCosmetics(this, lastUseScale);
				}
				else
				{
					_cosmetics = Cosmetics.GenerateStalkCosmetics(this, lastUseScale);
				}
			}
			return _cosmetics.Value;
		}
	}

	public RippleTree(Room room, PlacedObject pObj)
	{
		this.pObj = pObj;
		if (pObj.type == PlacedObject.Type.RippleTree)
		{
			threeStems = true;
		}
		scale = GoalScale;
		lastScale = GoalScale;
		lastUseScale = GoalScale;
		base.room = room;
		UpdatePlacedObjectInfo();
	}

	public RippleTree(Room room, Vector2 rootPos, int seed)
	{
		this.rootPos = rootPos;
		this.seed = seed;
		spiralRadVariation = 0.5f + room.game.SeededRandom(seed) * 0.2f;
		stunt = room.game.SeededRandom(seed + 1) / 2f + 0.25f;
		scale = GoalScale;
		lastScale = GoalScale;
		lastUseScale = GoalScale;
		goalPos = rootPos + new Vector2(0f, Mathf.Lerp(20f, 120f, Mathf.Pow(UnityEngine.Random.value, 3f)));
		if (!room.GetTile(rootPos).AnyWater || !(UnityEngine.Random.value < 0.5f))
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			if (!room.GetTile(rootPos + new Vector2(0f, (float)i * 20f)).AnyWater)
			{
				waterRoot = true;
				goalPos = rootPos + new Vector2(0f, (float)i * 20f + UnityEngine.Random.Range(10f, 30f));
				break;
			}
		}
	}

	public void UpdatePlacedObjectInfo()
	{
		goalPos = pObj.pos;
		int num = (int)pObj.pos.x + (int)pObj.pos.y;
		if (num != seed)
		{
			seed = num;
			spiralRadVariation = 0.5f + room.game.SeededRandom(seed) * 0.2f;
			rootPos = pObj.pos;
			TryFindRoot(room);
			if (Tree)
			{
				PlantSprouts(room);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!Tree && parent == null && room != null)
		{
			FindParent();
		}
		if (wantsToTutorialTimer > 0)
		{
			wantsToTutorialTimer--;
			TutorialUpdate();
		}
		if (Tree && GoalScale > 0.25f)
		{
			RevealRippleSpawn();
		}
		lastScale = scale;
		if (scale != GoalScale)
		{
			scale = Custom.LerpSinEaseInOut(scale, GoalScale, 0.2f);
		}
		if (glowTimer > 0f)
		{
			glowTimer -= 1f;
		}
		if (!room.game.devToolsActive || !Tree)
		{
			return;
		}
		if (Input.GetKey(KeyCode.Q) && !lastPressed)
		{
			lastPressed = true;
			RegionState.RippleSpawnEggState rippleSpawnEggState = room?.world.regionState?.rippleSpawnEggState;
			if (rippleSpawnEggState != null)
			{
				float num = 1f / (float)rippleSpawnEggState.totalEggs;
				Data.testRippleAmount = (Data.testRippleAmount ?? rippleSpawnEggState.percentEggsCollected) + num;
				string[] array = new string[1];
				float? testRippleAmount = Data.testRippleAmount;
				array[0] = "eggsCollected percent: " + testRippleAmount;
				Custom.LogImportant(array);
			}
		}
		if (!Input.GetKey(KeyCode.Q) && lastPressed)
		{
			lastPressed = false;
		}
	}

	public void FindParent()
	{
		float num = float.MaxValue;
		foreach (UpdatableAndDeletable update in room.updateList)
		{
			if (update != this && update is RippleTree { pObj: not null } rippleTree && rippleTree.pObj.type == PlacedObject.Type.RippleTree)
			{
				float num2 = Vector2.Distance(rippleTree.pObj.pos, goalPos);
				if (num2 < num)
				{
					num = num2;
					parent = rippleTree;
					scale = GoalScale;
				}
			}
		}
	}

	public void RevealRippleSpawn()
	{
		int ticksToReveal = TicksToReveal;
		int num = (int)Custom.LerpMap(scale, 0.25f, 1f, 5f, 20f);
		bool flag = false;
		foreach (Player item in room.PlayersInRoom)
		{
			if (item != null && item.firstChunk != null && Mathf.Abs(item.firstChunk.pos.x - pObj.pos.x) < Data.Rad && item.firstChunk.pos.y < rootPos.y + StalkLength + SpiralRad * 3f && item.firstChunk.pos.y > rootPos.y)
			{
				item.SetRippleSpawnEggReveal(Mathf.Min(item.rippleSpawnEggReveal + (float)num, ticksToReveal), this);
				flag = true;
				if (wantsToTutorialTimer == -1)
				{
					wantsToTutorialTimer = 400;
				}
			}
		}
		if (flag && glowTimer < (float)ticksToReveal)
		{
			glowTimer = Mathf.Min(glowTimer + (float)num, ticksToReveal);
		}
	}

	public void TutorialUpdate()
	{
		if (room.game.IsStorySession && room.game.GetStorySession.saveState.deathPersistentSaveData.rippleEggTutorial)
		{
			wantsToTutorialTimer = 0;
			return;
		}
		int num = 0;
		foreach (UpdatableAndDeletable update in room.updateList)
		{
			if (!(update is VoidSpawnEgg { RippleSpawn: not false, holdingSpawn: not false, RippleEggHidden: not false } voidSpawnEgg) || !(voidSpawnEgg.spawn.graphicsModule is VoidSpawnGraphics voidSpawnGraphics) || !(voidSpawnGraphics.AlphaFromGlowDist(voidSpawnGraphics.glowPos, voidSpawnEgg.pos) > 0.5f))
			{
				continue;
			}
			AbstractRoom abstractRoom = voidSpawnEgg.FindNearestWarpRoom();
			if (abstractRoom != null)
			{
				bool flag = false;
				foreach (string key in room.game.GetStorySession.saveState.miscWorldSaveData.discoveredWarpPoints.Keys)
				{
					if (key.Split(':')[0].ToLowerInvariant() == abstractRoom.name.ToLowerInvariant())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					num++;
				}
			}
			if (num >= 2)
			{
				TextTutorial textTutorial = new TextTutorial(room, WatcherEnums.Tutorial.rippleEggTutorial, new TextTutorial.Message[1]
				{
					new TextTutorial.Message("Free them. They will show you the way.", 40, 40)
				});
				textTutorial.watcherTutorial = true;
				room.AddObject(textTutorial);
				break;
			}
		}
	}

	public void TryFindRoot(Room placeRoom)
	{
		int x = placeRoom.GetTilePosition(pObj.pos).x;
		int num = placeRoom.GetTilePosition(pObj.pos).y;
		while (num >= 0 && num >= placeRoom.GetTilePosition(pObj.pos).y - 25)
		{
			if (!placeRoom.GetTile(x, num).Solid && placeRoom.GetTile(x, num - 1).Solid)
			{
				rootPos = new Vector2(pObj.pos.x, placeRoom.MiddleOfTile(x, num).y - 20f);
				GenerateStalk();
				break;
			}
			if (!placeRoom.GetTile(x, num).Solid && placeRoom.GetTile(x, num - 1).Solid)
			{
				rootPos = new Vector2(pObj.pos.x, placeRoom.MiddleOfTile(x, num).y - 20f);
				GenerateStalk();
				break;
			}
			num--;
		}
	}

	public void PlantSprouts(Room placeRoom)
	{
		if (sprouts != null)
		{
			foreach (RippleTree sprout in sprouts)
			{
				sprout.Destroy();
			}
		}
		List<PlacedObject.QuadObjectData> list = new List<PlacedObject.QuadObjectData>();
		foreach (PlacedObject placedObject in room.roomSettings.placedObjects)
		{
			if (placedObject.type == PlacedObject.Type.NoRippleSproutZone)
			{
				list.Add(placedObject.data as PlacedObject.QuadObjectData);
			}
		}
		sprouts = new List<RippleTree>();
		UnityEngine.Random.State state = UnityEngine.Random.state;
		try
		{
			List<IntVector2> list2 = new List<IntVector2>();
			UnityEngine.Random.InitState(seed);
			for (int i = 0; i < 200; i++)
			{
				int num = (int)(Mathf.Pow(UnityEngine.Random.value, 2f) * 40f) * ((i % 2 == 0) ? 1 : (-1));
				int num2 = (int)(Mathf.Pow(UnityEngine.Random.value, 2f) * 40f) * ((i / 2 % 2 == 0) ? 1 : (-1));
				IntVector2 intVector = placeRoom.GetTilePosition(pObj.pos) + new IntVector2(num, num2);
				int num3 = intVector.y + num2;
				while (num3 >= 0 && num3 >= intVector.y + num2 - 20)
				{
					if (!placeRoom.GetTile(intVector.x, num3).Solid && placeRoom.GetTile(intVector.x, num3 - 1).Solid)
					{
						int num4 = 0;
						IntVector2[] eightDirections = Custom.eightDirections;
						for (int j = 0; j < eightDirections.Length; j++)
						{
							IntVector2 intVector2 = eightDirections[j];
							if (placeRoom.GetTile(intVector.x + intVector2.x, num3 + intVector2.y).Solid)
							{
								num4++;
							}
							if (placeRoom.GetTile(intVector.x + intVector2.x, num3 + intVector2.y).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
							{
								num4 = 6;
								break;
							}
						}
						if (num4 < 6 && placeRoom.GetTile(intVector.x, num3 - 2).Solid && (placeRoom.GetTile(intVector.x - 1, num3 - 1).Solid || placeRoom.GetTile(intVector.x - 1, num3 - 2).Solid) && (placeRoom.GetTile(intVector.x + 1, num3 - 1).Solid || placeRoom.GetTile(intVector.x + 1, num3 - 2).Solid))
						{
							float value = UnityEngine.Random.value;
							bool flag = false;
							foreach (PlacedObject.QuadObjectData item in list)
							{
								if (Custom.QuadPolyContainsPoint(placeRoom.MiddleOfTile(intVector.x, num3) - item.owner.pos, Vector2.zero, item.handles[0], item.handles[1], item.handles[2]))
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								int num5 = 0;
								foreach (IntVector2 item2 in list2)
								{
									if (item2 == new IntVector2(intVector.x, num3))
									{
										num5++;
									}
								}
								if (value < 1f / (float)(num5 * 2 + 1))
								{
									int num6 = num + num2 + i;
									RippleTree rippleTree = new RippleTree(placeRoom, placeRoom.MiddleOfTile(intVector.x, num3) + new Vector2(Mathf.Lerp(-9f, 9f, placeRoom.game.SeededRandom(num6 + i)), -35f), num6);
									placeRoom.AddObject(rippleTree);
									sprouts.Add(rippleTree);
									list2.Add(new IntVector2(intVector.x, num3));
								}
							}
						}
					}
					num3--;
				}
			}
		}
		finally
		{
			UnityEngine.Random.state = state;
		}
	}

	public void ResetCosmetics()
	{
		_cosmetics = null;
	}

	public void GenerateStalk()
	{
		ResetCosmetics();
		spiral = new Spiral(0f, SpiralRad / cosmetics.spiralCoils);
		float num = (cosmetics.spiralCoils - Mathf.Ceil(cosmetics.spiralCoils)) * 360f;
		float num2 = spiral.AngleToLength((Mathf.Lerp(270f, 90f, cosmetics.droopy) + 360f * Mathf.Ceil(cosmetics.spiralCoils) + num) * ((float)Math.PI / 180f));
		Vector2 vector = (cosmetics.droopLeft ? new Vector2(-1f, 1f) : new Vector2(1f, 1f));
		float num3 = num2 * cosmetics.spiralAmount;
		float num4 = Mathf.Max(0f, num3 / spiralSegmentDist);
		float num5 = StalkLength / stalkSegmentDist;
		stalk = new Vector2[Mathf.Max(0, Mathf.CeilToInt(num5 + num4) + 1)];
		float num6 = Mathf.Max(0f, spiral.AngleToLength(spiral.LengthToAngle(num2) - Mathf.Lerp(360f, 270f, cosmetics.droopy) * ((float)Math.PI / 180f)));
		float num7 = cosmetics.spiralAmount * 90f;
		float num8 = num7 / spiralSegmentDist;
		float f = num5 + num8;
		if (threeStems)
		{
			leftStalk = new Vector2[Mathf.CeilToInt(f) + 1];
			rightStalk = new Vector2[Mathf.CeilToInt(f) + 1];
		}
		spiralCenter = rootPos + new Vector2(0f, StalkLength) + 2f * SpiralRad * Custom.DegToVec(Mathf.Lerp(0f, 90f, cosmetics.droopy)) * vector;
		for (int i = 0; i < stalk.Length; i++)
		{
			float num9 = Mathf.LerpUnclamped(0f, StalkLength, Custom.InverseLerpUnclamped(0f, num5, i));
			Vector2 vector2 = rootPos + new Vector2(0f, num9);
			float num10 = Mathf.InverseLerp(0f, 40f, num9);
			float p = Custom.LerpMap(num9, StalkLength - 120f, StalkLength + 40f, 10f, 1f);
			float num11 = Mathf.Pow(Mathf.InverseLerp(StalkLength - 120f, StalkLength + 40f, num9), p);
			num10 *= 1f - num11;
			float num12 = Mathf.Sin((num9 + cosmetics.sinOffset) / cosmetics.sinDistance) * cosmetics.sinWidth * num10;
			vector2.x += num12;
			if (num11 > 0f && i != 0)
			{
				float x = Mathf.LerpUnclamped(num2, num2 - num3, Mathf.InverseLerp(num5, num5 + num4, i));
				Vector2 vector3 = Custom.rotateVectorDeg(spiral.LengthToPos(x), num) * vector;
				vector2 = Vector2.Lerp(vector2, spiralCenter + vector3, num11);
			}
			stalk[i] = vector2;
			if (!threeStems)
			{
				continue;
			}
			if (i < leftStalk.Length)
			{
				float num13 = Mathf.Pow(Mathf.InverseLerp(StalkLength + 120f, StalkLength - 40f * num11, num9), 1f);
				float num14 = Mathf.Lerp(Mathf.Pow(Mathf.InverseLerp(StalkLength - 80f, StalkLength + 80f, num9), 2f), num11, num13);
				vector2 = rootPos + new Vector2(num12, num9) + new Vector2(-20f, 0f) * vector;
				if (num14 > 0f && i != 0)
				{
					float x2 = Mathf.LerpUnclamped(num2, num2 - num7, Mathf.InverseLerp(num5, num5 + num8, i));
					if (i == leftStalk.Length - 1)
					{
						x2 = num2 - num7;
					}
					Vector2 vector4 = Custom.rotateVectorDeg(spiral.LengthToPos(x2), num) * vector;
					vector2 = Vector2.Lerp(vector2, spiralCenter + vector4 + new Vector2(-20f * num13, 0f) * vector, num14);
				}
				leftStalk[i] = vector2;
			}
			if (i >= rightStalk.Length)
			{
				continue;
			}
			float num15 = Mathf.Pow(Mathf.InverseLerp(StalkLength + 120f, StalkLength - 40f * num11, num9), 1f);
			float num16 = Mathf.Lerp(Mathf.Pow(Mathf.InverseLerp(StalkLength - 40f, StalkLength + 80f, num9), 2f), num11, num15);
			vector2 = rootPos + new Vector2(num12, num9) + new Vector2(20f * num15, 0f) * vector;
			float num17 = i - Mathf.FloorToInt(num5);
			if (num16 > 0f && num17 > 0f && i != 0)
			{
				float x3 = Mathf.LerpUnclamped(num6, num6 + num7, Mathf.InverseLerp(num5, num5 + num8, i));
				if (i == rightStalk.Length - 1)
				{
					x3 = num6 + num7;
				}
				vector2 = Vector2.Lerp(vector2, spiralCenter + Custom.rotateVectorDeg(spiral.LengthToPos(x3), num) * vector, num16);
			}
			rightStalk[i] = vector2;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		GenerateStalk();
		if (stalk.Length < 2)
		{
			sLeaser.sprites = new FSprite[0];
			return;
		}
		sLeaser.sprites = new FSprite[threeStems ? 7 : 2];
		for (int i = 0; i < ((!threeStems) ? 1 : 3); i++)
		{
			Vector2[] array = i switch
			{
				1 => leftStalk, 
				2 => rightStalk, 
				_ => stalk, 
			};
			sLeaser.sprites[i * 2] = TriangleMesh.MakeOneToOneLongMesh(array.Length, customColor: true);
			sLeaser.sprites[i * 2].shader = rCam.room.game.rainWorld.Shaders["RippleDialLevel"];
			sLeaser.sprites[i * 2 + 1] = TriangleMesh.MakeOneToOneLongMesh(array.Length, customColor: true);
			sLeaser.sprites[i * 2 + 1].shader = rCam.room.game.rainWorld.Shaders["RippleTreeGlow"];
		}
		if (threeStems)
		{
			sLeaser.sprites[6] = new CircularSprite("Futile_White");
			sLeaser.sprites[6].shader = rCam.room.game.rainWorld.Shaders["LightSource"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		bool flag = !room.world.region.regionParams.softLightRegion && ((!room.GetTile(rootPos).AnyWater && cosmetics.depth < 5f) || cosmetics.depth == 0f);
		if (newContatiner == null)
		{
			newContatiner = (flag ? rCam.ReturnFContainer("Water") : ((!(cosmetics.depth < 6f)) ? rCam.ReturnFContainer("Background") : rCam.ReturnFContainer("Items")));
		}
		for (int num = sLeaser.sprites.Length - 1; num >= 0; num--)
		{
			if (num % 2 != 0 || num == 6)
			{
				rCam.ReturnFContainer(flag ? "GrabShaders" : "ForegroundLights").AddChild(sLeaser.sprites[num]);
			}
			else
			{
				newContatiner.AddChild(sLeaser.sprites[num]);
			}
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = RainWorld.RippleColor;
		stalkColor = Color.Lerp(palette.blackColor, palette.fogColor, cosmetics.depth / 31f);
		Vector2 vector = rootPos - rCam.pos;
		int x = (int)cosmetics.depth;
		int y = 4;
		if (rCam.PositionCurrentlyVisible(rootPos, 0f, widescreen: true))
		{
			x = (int)(rCam.levelTexture.GetPixel((int)vector.x, (int)vector.y).r * 255f);
			y = Mathf.FloorToInt((float)x / 30f);
			x %= 30;
			x = (int)Mathf.Lerp(x, cosmetics.depth, cosmetics.depth / 31f);
		}
		depthColor = palette.texture.GetPixel(x, y);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
			return;
		}
		float num = Mathf.Lerp(lastScale, scale, timeStacker);
		bool flag = Data != null && Data.update;
		if (num != lastUseScale || flag)
		{
			if (flag)
			{
				UpdatePlacedObjectInfo();
			}
			lastUseScale = num;
			List<(FContainer, int)> list = new List<(FContainer, int)>();
			FSprite[] sprites = sLeaser.sprites;
			foreach (FSprite fSprite in sprites)
			{
				list.Add((fSprite.container, fSprite.container.GetChildIndex(fSprite)));
				fSprite.RemoveFromContainer();
			}
			InitiateSprites(sLeaser, rCam);
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			if (sLeaser.sprites.Length == list.Count)
			{
				int num2 = 0;
				foreach (var (fContainer, newIndex) in list)
				{
					if (sLeaser.sprites[num2].container == fContainer)
					{
						fContainer.AddChildAtIndex(sLeaser.sprites[num2], newIndex);
					}
					num2++;
				}
			}
			if (flag)
			{
				Data.update = false;
			}
		}
		if (stalk.Length < 2)
		{
			return;
		}
		for (int j = 0; j < (threeStems ? 6 : 2); j++)
		{
			Vector2[] array = j switch
			{
				2 => leftStalk, 
				3 => leftStalk, 
				4 => rightStalk, 
				5 => rightStalk, 
				_ => stalk, 
			};
			Vector2 vector = array[0] + new Vector2(0f, -1f);
			Vector2 vector2 = array[0];
			Vector2 vector3 = array[0];
			for (int k = 0; k < array.Length; k++)
			{
				Vector2 vector4 = Vector2.Lerp(array[k], array[k], timeStacker);
				Vector2 vector5 = Custom.PerpendicularVector((vector4 - vector).normalized);
				float num3 = stalkRad;
				if (j % 2 != 0)
				{
					num3 = Custom.LerpMap(k, array.Length - taperDistance, array.Length, Mathf.Min(40f, Vector2.Distance(spiralCenter, vector4)), num3);
				}
				if (!Sprout)
				{
					num3 *= 1f + Mathf.InverseLerp(baseThickDistance, 0f, k) * Mathf.InverseLerp(20f, 120f, SpiralRad);
				}
				Vector2 vector6 = Custom.PerpendicularVector(vector2 - vector3);
				float num4 = Custom.LerpMap(k, array.Length - taperDistance, array.Length, 1f, -1f, 2f);
				Vector2 vector7 = vector4 - vector5 * num3;
				Vector2 vector8 = vector4 + vector5 * num3 * num4;
				if (k != 0)
				{
					vector7 -= vector6 * Mathf.Min(0f, Vector2.Dot(vector7 - vector2, vector6) - 1f);
					vector8 -= vector6 * Mathf.Min(0f, Vector2.Dot(vector8 - vector3, vector6) - 1f);
				}
				(sLeaser.sprites[j] as TriangleMesh).MoveVertice(k * 2, vector7 - camPos);
				(sLeaser.sprites[j] as TriangleMesh).MoveVertice(k * 2 + 1, vector8 - camPos);
				vector2 = vector7;
				vector3 = vector8;
				vector = vector4;
			}
		}
		TriangleMesh triangleMesh = sLeaser.sprites[0] as TriangleMesh;
		TriangleMesh triangleMesh2 = sLeaser.sprites[1] as TriangleMesh;
		TriangleMesh triangleMesh3 = (threeStems ? (sLeaser.sprites[2] as TriangleMesh) : null);
		TriangleMesh triangleMesh4 = (threeStems ? (sLeaser.sprites[4] as TriangleMesh) : null);
		TriangleMesh triangleMesh5 = (threeStems ? (sLeaser.sprites[3] as TriangleMesh) : null);
		TriangleMesh triangleMesh6 = (threeStems ? (sLeaser.sprites[5] as TriangleMesh) : null);
		float glowAmount = GlowAmount;
		for (int l = 0; l < triangleMesh.verticeColors.Length; l += 2)
		{
			float num5 = (float)l / (float)(triangleMesh.verticeColors.Length - 2);
			Color b = Color.Lerp(stalkColor, this.color, Mathf.Pow(num5, 0.2f));
			float a = Mathf.Pow(Mathf.Lerp(Mathf.Lerp(0.1f, 0.25f, cosmetics.depth / 10f), 0.75f, num5), Mathf.Lerp(1f, 0.4f, cosmetics.depth / 10f));
			a = Mathf.Min(a, Mathf.InverseLerp(0f, taperDistance, l));
			b = Color.Lerp(depthColor, b, a);
			b = Color.Lerp(b, rCam.currentPalette.fogColor, Mathf.Lerp(rCam.currentPalette.fogAmount, 1f, 0.5f) * Mathf.Clamp01((cosmetics.depth - 5f) / 30f));
			b.a = (30f - cosmetics.depth) / 30f;
			triangleMesh.verticeColors[l] = (triangleMesh.verticeColors[l + 1] = b);
			float a2 = Mathf.InverseLerp(triangleMesh2.verticeColors.Length, triangleMesh2.verticeColors.Length - glowTaperDistance * 4, l);
			a2 = Mathf.Min(a2, Mathf.InverseLerp(0f, glowTaperDistance, l));
			Color color = new Color(a2, Mathf.Lerp(0.5f, 0.8f, num5), Mathf.Lerp(0.4f, 0.7f, Mathf.Pow(num5, 3f)) * Mathf.Max(0f, glowAmount - Mathf.Pow(num5 / 6f, 2f)));
			color.a = b.a + 1f / 30f;
			triangleMesh2.verticeColors[l] = (triangleMesh2.verticeColors[l + 1] = color);
			if (threeStems)
			{
				if (triangleMesh3.verticeColors.Length > l)
				{
					a2 = (color.r = Mathf.Min(a2, Mathf.InverseLerp(triangleMesh5.verticeColors.Length, triangleMesh5.verticeColors.Length - glowTaperDistance * 2, l)));
					triangleMesh3.verticeColors[l] = (triangleMesh3.verticeColors[l + 1] = b);
					triangleMesh5.verticeColors[l] = (triangleMesh5.verticeColors[l + 1] = color);
				}
				if (triangleMesh4.verticeColors.Length > l)
				{
					a2 = Mathf.Min(a2, Mathf.InverseLerp(triangleMesh6.verticeColors.Length, triangleMesh6.verticeColors.Length - glowTaperDistance * 2, l));
					color.r = a2;
					triangleMesh4.verticeColors[l] = (triangleMesh4.verticeColors[l + 1] = b);
					triangleMesh6.verticeColors[l] = (triangleMesh6.verticeColors[l + 1] = color);
				}
			}
		}
		if (threeStems)
		{
			sLeaser.sprites[6].SetPosition(spiralCenter - camPos);
			sLeaser.sprites[6].color = Color.Lerp(RainWorld.RippleColor, RainWorld.SaturatedGold, 0.5f);
			sLeaser.sprites[6].scale = SpiralRad / 4f;
			sLeaser.sprites[6].alpha = 0.6f * Mathf.Max(0f, glowAmount - 0.3f);
		}
	}
}
