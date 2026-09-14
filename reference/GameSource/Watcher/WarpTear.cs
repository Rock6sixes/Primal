using System.Collections.Generic;
using System.Linq;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class WarpTear : UpdatableAndDeletable, IDrawable
{
	public struct TreadAnim
	{
		public Vector2 start;

		public Vector2 end;

		public Vector2 pos;

		public float order;
	}

	public class Tear : UpdatableAndDeletable, IDrawable, INotifyWhenRoomUnloaded
	{
		public WarpTear root;

		public Vector2 pos;

		public float movement;

		public Vector3 scale;

		public Mesh mesh;

		public MaskSource warpTear;

		public Tear parent;

		public float baseSize = 30f;

		public float sizeStep = 10f;

		public float sizeVariation;

		public float randomOffset;

		public float branchRandomValue;

		public int nChildren;

		public int totalLength;

		public float branchingChance;

		public bool branchingPoint;

		public DebugSprite startSprite;

		public DebugSprite branchPointSprite;

		public DebugSprite endSprite;

		public bool debug;

		public Vector2 target => Custom.DegToVec(rotation + movement);

		public float rotation { get; set; }

		public float angleVariation => 60f * ((float)Mathf.Max(totalLength - nChildren / 2, 0) / (float)totalLength);

		public float size => (baseSize + sizeStep * (float)(1 + nChildren)) * (1f + sizeVariation);

		public Tear(Room room, WarpTear root, int nChildren, float branchingChance, Tear parent = null, float? rotation = null)
		{
			this.root = root;
			this.nChildren = nChildren;
			this.branchingChance = branchingChance;
			this.parent = parent;
			base.room = room;
			randomOffset = Random.value;
			if (parent != null)
			{
				branchRandomValue = parent.branchRandomValue;
				sizeVariation = parent.sizeVariation;
				totalLength = parent.totalLength;
				this.rotation = parent.rotation + (Random.value * 2f - 1f) * angleVariation;
				pos = parent.pos + parent.target * parent.size;
			}
			else
			{
				branchRandomValue = Random.value;
				sizeVariation = Random.value;
				totalLength = nChildren;
				this.rotation = ((!rotation.HasValue) ? (Random.value * 360f) : (rotation.Value + (Random.value * 2f - 1f) * angleVariation));
				pos = root.pos;
			}
			scale = new Vector3(1.5f, 2f, 1f) * (size / 20f);
			mesh = MaskSource.CreateQuadMesh();
			mesh.vertices = mesh.vertices.Select((Vector3 v) => v + Vector3.up * 0.3f).ToArray();
			mesh.RecalculateBounds();
			SpawnChildren();
			SpawnWeaverThread();
			if (debug)
			{
				startSprite = new DebugSprite(default(Vector2), new FSprite("Futile_White")
				{
					color = Color.green,
					scale = 0.8f
				}, room);
				endSprite = new DebugSprite(default(Vector2), new FSprite("Futile_White")
				{
					color = Color.red,
					scale = 0.7f
				}, room);
				branchPointSprite = new DebugSprite(default(Vector2), new FSprite("Futile_White")
				{
					color = Color.blue,
					scale = 0.6f
				}, room);
				room.AddObject(startSprite);
				room.AddObject(endSprite);
				room.AddObject(branchPointSprite);
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			if (warpTear != null)
			{
				warpTear.DeleteMe();
				warpTear = null;
			}
			if (mesh != null)
			{
				Object.Destroy(mesh);
				mesh = null;
			}
		}

		public void RoomUnloaded()
		{
			Destroy();
		}

		public void SpawnChildren()
		{
			if (nChildren > 0)
			{
				room.AddObject(new Tear(room, root, nChildren - 1, branchingChance, this));
				if (totalLength - nChildren > 1 && !(Random.value > branchingChance))
				{
					branchingPoint = true;
					float num = Random.value - 0.5f;
					num += Mathf.Sign(num) * 0.5f;
					num *= 70f;
					room.AddObject(new Tear(room, root, nChildren + 1, branchingChance * 0.5f, this, rotation + num));
				}
			}
		}

		public void SpawnWeaverThread()
		{
			if ((root.pos - pos).magnitude > 1000f || (room.CameraViewingPoint(pos) == -1 && !root.VoidWeaverEndingMode))
			{
				return;
			}
			float num = totalLength - nChildren;
			Vector2 vector = target * size;
			Vector2 vector2 = Custom.PerpendicularVector(target);
			int num2 = (int)(size / (15f + num * 12f));
			for (int i = 0; i < num2; i++)
			{
				float num3 = (float)i / (float)num2;
				Vector2 vector3 = pos + vector * num3;
				if (!((vector3 - root.pos).magnitude < 110f))
				{
					float num4 = Mathf.Sign(Random.value - 0.5f);
					float num5 = (Random.value * 2f - 1f) * 0.5f;
					Vector2 vector4 = vector * num5 + vector2 * (num4 * size * (0.25f + Random.value * 0.3f));
					Vector2 vector5 = vector * (0f - num5) - vector2 * (num4 * size * (0.25f + Random.value * 0.3f));
					if (IsThreadVisible(vector3 + vector4, vector3 + vector5))
					{
						root.threadAnim.Add(new TreadAnim
						{
							start = vector4,
							end = vector5,
							pos = vector3,
							order = num + num3
						});
					}
				}
			}
		}

		public bool IsThreadVisible(Vector2 p1, Vector2 p2)
		{
			if (root.VoidWeaverEndingMode)
			{
				return true;
			}
			if (!room.RoomRect.Vector2Inside(p1) && !room.RoomRect.Vector2Inside(p2))
			{
				return false;
			}
			if (room.PointSubmerged(p1) && room.PointSubmerged(p2))
			{
				return false;
			}
			return true;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (root.slatedForDeletetion)
			{
				Destroy();
			}
			if (parent != null)
			{
				pos = parent.pos + parent.target * parent.size;
			}
			UpdateBranchMovement();
			if (!room.BeingViewed || base.slatedForDeletetion || warpTear == null || warpTear.beingDeleted)
			{
				return;
			}
			warpTear.SetPos(pos);
			warpTear.SetScale(scale);
			warpTear.SetRotation(Vector3.back * (rotation + movement));
			if (debug)
			{
				startSprite.pos = pos;
				endSprite.pos = pos + target * size;
				if (branchingPoint)
				{
					branchPointSprite.pos = pos + target * size;
				}
			}
		}

		public void UpdateBranchMovement()
		{
			float num = (float)(totalLength - nChildren) / (float)totalLength;
			movement = Mathf.Sin((float)root.animation * (0.002f + 0.002f * num) * (0.25f + randomOffset * 0.75f) + randomOffset * 100f) * angleVariation * 0.5f;
			movement *= Custom.LerpQuadEaseInOut(1f, 0f, Mathf.InverseLerp(num * 0.8f, num * 0.8f + 0.2f, root.moveBranchToDefaultPos * 0.7f - 0.1f));
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.maskSources = new MaskSource[1];
			UpdateBranchMovement();
			warpTear = MaskMaker.MakeSource("WarpTearGrab", "WarpTearMask", isSecondLayer: false, mesh, pos, Vector3.back * (rotation + movement), scale);
			sLeaser.maskSources[0] = warpTear;
			warpTear.SetProperty(0, branchRandomValue);
			warpTear.SetProperty(2, randomOffset);
			warpTear.SetPos(pos, initialize: true);
			warpTear.SetScale(scale, initialize: true);
			warpTear.SetRotation(Vector3.back * (rotation + movement), initialize: true);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			if (warpTear != null && !warpTear.beingDeleted)
			{
				warpTear.SetProperty(1, Mathf.Lerp(root.lastOpenAnimation, root.openAnimation, timeStacker));
				warpTear.DrawUpdate(timeStacker, rCam, camPos);
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
		}
	}

	public class TearBlocker : UpdatableAndDeletable, IDrawable
	{
		public MaskSource tearBlocker;

		public Vector2 pos;

		public Vector3 _scale;

		public float animation;

		public float fadeInTime = 40f;

		public bool blocker = true;

		public float scale
		{
			set
			{
				_scale = Vector3.one * value;
			}
		}

		public Vector3 scale3
		{
			set
			{
				_scale = value;
			}
		}

		public TearBlocker(Vector2 pos, float scale, bool blocker = true)
		{
			this.pos = pos;
			this.scale = scale;
			this.blocker = blocker;
		}

		public TearBlocker(Vector2 pos, float scale, float startingAnimation, bool blocker = true)
		{
			this.pos = pos;
			this.scale = scale;
			animation = startingAnimation;
			this.blocker = blocker;
		}

		public TearBlocker(ZapCoil coil)
		{
			pos = coil.GetFloatRect.Center;
			_scale = new Vector3((float)coil.rect.Width * 8f, (float)coil.rect.Height * 8f, 1f);
			animation = 1f;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (!room.BeingViewed || base.slatedForDeletetion)
			{
				if (blocker)
				{
					animation = 1f;
				}
			}
			else if (tearBlocker != null && !tearBlocker.beingDeleted)
			{
				tearBlocker.SetPos(pos);
				tearBlocker.SetScale(_scale);
				if (blocker && animation < 1f)
				{
					tearBlocker.SetProperty(0, animation += 1f / fadeInTime);
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			if (tearBlocker != null)
			{
				tearBlocker.DeleteMe();
				tearBlocker = null;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.maskSources = new MaskSource[1];
			sLeaser.maskSources[0] = MaskMaker.MakeSource("WarpTearGrab", "WarpTearBlocker", blocker, null, pos, null, _scale);
			tearBlocker = sLeaser.maskSources[0];
			tearBlocker.SetProperty(1, (!blocker) ? 1 : 0);
			tearBlocker.SetProperty(0, animation);
			tearBlocker.SetPos(pos, initialize: true);
			tearBlocker.SetScale(_scale, initialize: true);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			tearBlocker?.DrawUpdate(timeStacker, rCam, camPos);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
		}
	}

	public List<Tear> tears;

	public Vector2 pos;

	public Vector2 lastPos;

	public int seed;

	public TearBlocker entrance;

	public TearBlocker bigRift;

	public LightSource light;

	public List<TearBlocker> importantBlockers;

	public List<TreadAnim> threadAnim = new List<TreadAnim>();

	public Color lightColor = new Color(0f, 0.2f, 0.7f);

	public float lightRad = 1000f;

	public float weaverBlockAnimation;

	public float moveBranchToDefaultPos;

	public float openAnimation;

	public float fadeAnim = 1f;

	public float lastWeaverBlockAnimation;

	public float lastOpenAnimation;

	public float lastFadeAnim;

	public int minBranches = 3;

	public int maxBranches = 10;

	public int minLength = 5;

	public int maxLength = 10;

	public float fadeDuration = 240f;

	public bool threadsSorted;

	public bool isInit;

	public bool _fadeIn;

	public bool _fadeOut;

	public bool isRippleSide;

	public bool shortcutBlockersSpawned;

	public bool importantBlockersSpawned;

	public bool partialWeaverBlock;

	public bool debug;

	public int animation;

	public string shaderOverride;

	public Counter bigRiftFadeIn;

	public bool forceNoVisuals;

	public bool deleteWhenClosed;

	public bool moveToBack;

	public float branchingChance;

	public bool VoidWeaverEndingMode => room.abstractRoom.name == "WRSA_WEAVER";

	public WarpTear(Room room, Vector2 pos, float branchingChance, bool rippleSide, int? seed = null, string shaderOverride = null, bool spawnBigRift = false)
	{
		this.pos = pos;
		base.room = room;
		isRippleSide = rippleSide;
		this.branchingChance = branchingChance;
		int valueOrDefault = seed.GetValueOrDefault();
		if (!seed.HasValue)
		{
			valueOrDefault = (int)(Random.value * 1000000f);
			seed = valueOrDefault;
		}
		this.seed = seed.Value;
		tears = new List<Tear>();
		importantBlockers = new List<TearBlocker>();
		animation = Random.Range(0, 10000);
		if (shaderOverride != null)
		{
			this.shaderOverride = shaderOverride;
		}
		if (spawnBigRift)
		{
			room.AddObject(bigRift = new TearBlocker(pos, 0f, 0.6f, blocker: false));
			bigRiftFadeIn = new Counter(240, 0, countsUp: true);
		}
		InitTears();
		room.AddObject(light = new LightSource(pos, environmentalLight: false, lightColor, this)
		{
			rad = lightRad,
			affectedByPaletteDarkness = 0.5f
		});
		UpdateLight();
		room.AddObject(entrance = new TearBlocker(pos, 20f)
		{
			blocker = false
		});
		WarpPoint.WarpPointTimer warpPointTimer = room.game.cameras[0].warpPointTimer;
		if (warpPointTimer != null)
		{
			forceNoVisuals = warpPointTimer.forceNoExitVisuals;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastPos = pos;
		lastWeaverBlockAnimation = weaverBlockAnimation;
		lastOpenAnimation = openAnimation;
		animation++;
		if (weaverBlockAnimation != 0f)
		{
			openAnimation = Mathf.Clamp01(openAnimation + 0.0125f);
		}
		if (bigRift != null)
		{
			bigRiftFadeIn.Tick();
			bigRift.scale = 300f * bigRiftFadeIn.normalized;
		}
		UpdateLight();
		UpdateFade();
		if (entrance != null)
		{
			entrance.pos = pos;
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		entrance?.Destroy();
		bigRift?.Destroy();
		foreach (TearBlocker importantBlocker in importantBlockers)
		{
			importantBlocker.Destroy();
		}
	}

	public void UpdateLight()
	{
		if (light == null)
		{
			return;
		}
		light.setPos = pos;
		light.setAlpha = Mathf.InverseLerp(0.5f, 1f, openAnimation * fadeAnim) * 0.5f;
		if (weaverBlockAnimation != 0f)
		{
			if (weaverBlockAnimation == 1f)
			{
				light.color = RainWorld.SaturatedGold * 0.5f;
				light.setRad = lightRad * 0.5f;
			}
			else
			{
				light.color = Color.Lerp(lightColor, RainWorld.SaturatedGold * 0.5f, weaverBlockAnimation);
				light.setRad = Mathf.Lerp(lightRad, lightRad * 0.5f, weaverBlockAnimation);
			}
		}
	}

	public void UpdateFade()
	{
		lastFadeAnim = fadeAnim;
		if (_fadeIn && fadeAnim < 1f)
		{
			fadeAnim = Mathf.Clamp01(fadeAnim + 1f / fadeDuration);
			if (partialWeaverBlock)
			{
				weaverBlockAnimation = Mathf.Lerp(0.3f, 0f, fadeAnim);
			}
		}
		else
		{
			_fadeIn = false;
		}
		if (_fadeOut && fadeAnim > 0f)
		{
			fadeAnim = Mathf.Clamp01(fadeAnim - 1f / fadeDuration);
			if (partialWeaverBlock)
			{
				weaverBlockAnimation = Mathf.Lerp(0.3f, 0f, fadeAnim);
			}
		}
		else
		{
			_fadeOut = false;
		}
		if (!_fadeOut && fadeAnim == 0f)
		{
			Destroy();
		}
	}

	public void FadeIn(float duration)
	{
		fadeDuration = Mathf.Max(duration, 1f);
		_fadeIn = true;
		_fadeOut = false;
	}

	public void FadeOut(float duration)
	{
		if (!_fadeOut && fadeAnim > 0f)
		{
			room.PlaySound(WatcherEnums.WatcherSoundID.Warp_Point_Close, pos);
		}
		fadeDuration = Mathf.Max(duration, 1f);
		_fadeOut = true;
		_fadeIn = false;
	}

	public void InitTears()
	{
		if (!isInit)
		{
			Random.State state = Random.state;
			Random.InitState(seed);
			SpawnDefault();
			SpawnTowardsNearbyScreens();
			Random.state = state;
			isInit = true;
			SpawnDebugSprites();
		}
	}

	public void SpawnDebugSprites()
	{
		if (debug)
		{
			for (int i = 0; i < tears.Count; i++)
			{
				room.AddObject(new DebugSprite(pos, new FSprite("Futile_White")
				{
					rotation = tears[i].rotation,
					scaleY = 100f,
					scaleX = 1f,
					anchorY = 0f,
					color = Color.red
				}, room));
			}
		}
	}

	public void SpawnImportantBlockers()
	{
		if (!shortcutBlockersSpawned && room.shortCutsReady)
		{
			for (int i = 0; i < room.shortcuts.Length; i++)
			{
				if (room.shortcuts[i].shortCutType != ShortcutData.Type.NPCTransportation && room.shortcuts[i].shortCutType != ShortcutData.Type.DeadEnd)
				{
					TearBlocker tearBlocker = new TearBlocker(room.MiddleOfTile(room.shortcuts[i].StartTile), 16f, 1f);
					importantBlockers.Add(tearBlocker);
					room.AddObject(tearBlocker);
				}
			}
			shortcutBlockersSpawned = true;
		}
		if (importantBlockersSpawned)
		{
			return;
		}
		for (int j = 0; j < room.updateList.Count; j++)
		{
			if (room.updateList[j] is ZapCoil coil)
			{
				TearBlocker tearBlocker2 = new TearBlocker(coil);
				importantBlockers.Add(tearBlocker2);
				room.AddObject(tearBlocker2);
			}
		}
		importantBlockersSpawned = true;
	}

	public void GetThreadAnimation(List<Vector2> start, List<Vector2> end, List<Vector2> pos)
	{
		SortThreadOrder();
		for (int i = 0; i < threadAnim.Count; i++)
		{
			start.Add(threadAnim[i].start);
			end.Add(threadAnim[i].end);
			pos.Add(threadAnim[i].pos);
		}
	}

	public void SortThreadOrder()
	{
		if (!threadsSorted)
		{
			threadAnim.Sort((TreadAnim s1, TreadAnim s2) => s1.order.CompareTo(s2.order));
			threadsSorted = true;
		}
	}

	public void SpawnTowardsNearbyScreens()
	{
		if (room.cameraPositions.Length <= 1)
		{
			return;
		}
		int num = room.CameraViewingPoint(pos);
		if (num == -1)
		{
			return;
		}
		for (int i = 0; i < room.cameraPositions.Length; i++)
		{
			if (i != num)
			{
				Vector2 v = room.cameraPositions[i] - room.cameraPositions[num];
				if (!(v.magnitude > 1100f))
				{
					Tear tear = new Tear(room, this, Random.Range(minLength, maxLength), branchingChance * 0.5f, null, Custom.VecToDeg(v))
					{
						pos = pos
					};
					tears.Add(tear);
					room.AddObject(tear);
				}
			}
		}
	}

	public void SpawnDefault()
	{
		int num = Random.Range(minBranches, maxBranches);
		for (int i = 0; i < num; i++)
		{
			Tear tear = new Tear(room, this, Random.Range(minLength, maxLength), branchingChance)
			{
				pos = pos
			};
			tears.Add(tear);
			room.AddObject(tear);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		SpawnImportantBlockers();
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders[(shaderOverride != null) ? shaderOverride : (isRippleSide ? "WarpTearRippleSide" : "WarpTear")];
		sLeaser.sprites[0].scale = rCam.sSize.x * (0.125f + ((bigRift != null) ? 0.04f : 0f)) + 8f;
		WarpPoint.WarpPointTimer warpPointTimer = room.game.cameras[0].warpPointTimer;
		if (warpPointTimer != null && warpPointTimer.forceNoExitVisuals)
		{
			forceNoVisuals = true;
			sLeaser.SetAllSpritesVisibility(isVisible: false);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		SpawnImportantBlockers();
		sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
		rCam.ReturnFContainer("WarpPoint");
		if (moveToBack)
		{
			sLeaser.sprites[0].MoveToBack();
		}
		Color black = Color.black;
		black.r = Mathf.Lerp(lastOpenAnimation, openAnimation, timeStacker);
		black.g = Mathf.Lerp(lastWeaverBlockAnimation, weaverBlockAnimation, timeStacker);
		black.b = Mathf.Lerp(lastFadeAnim, fadeAnim, timeStacker);
		sLeaser.sprites[0].color = black;
		if (forceNoVisuals)
		{
			sLeaser.SetAllSpritesVisibility(isVisible: false);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		FContainer fContainer = rCam.ReturnFContainer(VoidWeaverEndingMode ? "Background" : "WarpPoint");
		fContainer.AddChildAtIndex(sLeaser.sprites[0], Mathf.Max(fContainer.GetChildCount() - 1, 0));
		Shader.SetGlobalInt("_warpTearIgnoreCreatures", VoidWeaverEndingMode ? 1 : 0);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}
}
