using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HUD;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class WarpMap : HudPart
{
	public abstract class WarpMapObject
	{
		public WarpMap map;

		public WarpMapObject(WarpMap map)
		{
			this.map = map;
		}

		public virtual void Update()
		{
		}

		public virtual void Draw(float timeStacker)
		{
		}

		public virtual void ClearSprites()
		{
		}
	}

	public class WarpRegionIcon : WarpMapObject
	{
		public List<WarpRegionConnection> connections;

		public HUDCircle circle;

		public HUDCircle selectionCircle;

		public FSprite slugcatIcon;

		public FSprite regionIcon;

		public List<FSprite> ringIcons;

		public FSprite[] voidFeathers;

		public float[] voidFeatherProgs;

		public float[] voidFeatherOffsets;

		public float[] voidFeatherScales;

		public string region;

		public bool newRegion;

		public bool infected;

		public bool hasVoidWeaver;

		public bool spinningTopEncounter;

		public bool visited;

		public bool active;

		public bool activeTerraceAnimation;

		public float ringFade;

		public float fadeCounter;

		public float maxFade = 1f;

		public float fadeIn = -0.25f;

		public int index;

		public Vector3 _missingPos;

		public Vector3 Pos
		{
			get
			{
				if (region.ToLowerInvariant() == "wrsa")
				{
					return Vector3.zero;
				}
				if (0 <= index && index <= map.mapData.regionPositions.Length)
				{
					return map.mapData.regionPositions[index];
				}
				return _missingPos;
			}
			set
			{
				if (0 <= index && index <= map.mapData.regionPositions.Length)
				{
					map.mapData.regionPositions[index] = value;
				}
				else
				{
					_missingPos = value;
				}
			}
		}

		public int CircleColor
		{
			get
			{
				if (Selected)
				{
					return 4;
				}
				if (hasVoidWeaver)
				{
					return 6;
				}
				if (infected)
				{
					return 7;
				}
				if (spinningTopEncounter)
				{
					return 6;
				}
				return 0;
			}
		}

		public bool Selected
		{
			get
			{
				return map.selectedRegion == this;
			}
			set
			{
				if (value)
				{
					map.selectedRegion = this;
				}
				else if (Selected)
				{
					map.selectedRegion = null;
				}
			}
		}

		public WarpRegionIcon(WarpMap map, int index, string region, bool infected, bool hasVoidWeaver, bool hasSpinningTopEncounter, bool visited)
			: base(map)
		{
			this.index = index;
			this.region = region;
			this.infected = infected;
			this.hasVoidWeaver = hasVoidWeaver;
			this.visited = visited;
			spinningTopEncounter = hasSpinningTopEncounter;
			ringIcons = new List<FSprite>();
			voidFeathers = new FSprite[0];
			connections = new List<WarpRegionConnection>();
			if (map.mapData.newWarpRegions.Contains(region))
			{
				newRegion = true;
			}
			AddGraphics();
		}

		public void AddGraphics()
		{
			ClearSprites();
			circle = new HUDCircle(map.hud, HUDCircle.SnapToGraphic.None, map.container, CircleColor);
			selectionCircle = new HUDCircle(map.hud, HUDCircle.SnapToGraphic.None, map.container, CircleColor);
			string text = "warp-" + region;
			if (!visited)
			{
				text = "warp-unknown";
			}
			if (!Futile.atlasManager.DoesContainElementWithName(text))
			{
				Texture2D texture2D = new Texture2D(0, 0);
				string path = AssetManager.ResolveFilePath("illustrations/" + text + ".png");
				if (File.Exists(path))
				{
					texture2D.LoadImage(File.ReadAllBytes(path));
				}
				texture2D.filterMode = FilterMode.Point;
				Futile.atlasManager.LoadAtlasFromTexture(text, texture2D, textureFromAsset: false);
			}
			regionIcon = new FSprite(text);
			regionIcon.SetAnchor(0.5f, 0.5f);
			map.container.AddChild(regionIcon);
			if (!(map.hud.owner is WarpSafariMenu) && region.ToLowerInvariant() == map.mapData.currentPlayerRegion.ToLowerInvariant())
			{
				slugcatIcon = new FSprite("Kill_Slugcat");
				slugcatIcon.SetAnchor(0.5f, 0.5f);
				map.container.AddChild(slugcatIcon);
			}
			if (hasVoidWeaver)
			{
				voidFeathers = new FSprite[7];
				voidFeatherProgs = new float[voidFeathers.Length];
				voidFeatherOffsets = new float[voidFeathers.Length];
				voidFeatherScales = new float[voidFeathers.Length];
				for (int i = 0; i < voidFeathers.Length; i++)
				{
					voidFeathers[i] = new FSprite("voidMapFeather" + UnityEngine.Random.Range(0, 5));
					voidFeathers[i].SetAnchor(0.5f, 0.5f);
					voidFeathers[i].color = RainWorld.SaturatedGold;
					voidFeatherProgs[i] = (float)i / (float)voidFeathers.Length;
					voidFeatherScales[i] = UnityEngine.Random.Range(0.2f, 0.7f);
					voidFeatherOffsets[i] = UnityEngine.Random.Range(-1f, 1f);
					map.container.AddChild(voidFeathers[i]);
				}
			}
			else
			{
				voidFeathers = new FSprite[0];
			}
			bool flag = spinningTopEncounter && !map.ActiveTerraceAnimation;
			if (flag)
			{
				FSprite fSprite = new FSprite(infected ? "echoRingHalf" : "echoRing");
				fSprite.SetAnchor(0.5f, 0.5f);
				fSprite.color = RainWorld.SaturatedGold;
				map.container.AddChild(fSprite);
				ringIcons.Add(fSprite);
				ringFade = 1f;
			}
			if (infected)
			{
				FSprite fSprite2 = new FSprite(flag ? "rotRingHalf" : "rotRing");
				fSprite2.SetAnchor(0.5f, 0.5f);
				fSprite2.color = RainWorld.RippleColor;
				map.container.AddChild(fSprite2);
				ringIcons.Add(fSprite2);
				ringFade = 1f;
			}
			UpdateGraphics();
			circle.Update();
			selectionCircle.Update();
		}

		public override void Update()
		{
			base.Update();
			if (map.visible)
			{
				Vector3 pos = Pos;
				Vector2 vector = RotateAroundCircle(new Vector2(pos.x, pos.z), (0f - map.panVel.x) / 60f);
				pos.x = vector.x;
				pos.z = vector.y;
				Vector2 vector2 = RotateAroundCircle(new Vector2(pos.y, pos.z), (0f - map.panVel.y) / 60f);
				pos.y = vector2.x;
				pos.z = vector2.y;
				Pos = pos;
			}
			for (int i = 0; i < voidFeathers.Length; i++)
			{
				voidFeatherProgs[i] += 0.00083333335f;
			}
			if (activeTerraceAnimation)
			{
				UpdateTerraceAnimation();
			}
		}

		public void UpdateTerraceAnimation()
		{
			bool flag = true;
			foreach (FSprite ringIcon in ringIcons)
			{
				if (ringIcon.element.name == "rotRing")
				{
					if (ringFade > 0f)
					{
						ringFade = Custom.LerpAndTick(ringFade, 0f, 0.025f, 0.01f);
						flag = false;
					}
					else
					{
						flag = true;
						ringIcon.element = Futile.atlasManager.GetElementWithName("rotRingHalf");
					}
				}
				if (ringIcon.element.name == "echoRing" || ringIcon.element.name == "echoRingHalf")
				{
					flag = false;
					if (ringFade < 1f)
					{
						ringFade = Custom.LerpAndTick(ringFade, 1f, 0.025f, 0.01f);
					}
					else
					{
						activeTerraceAnimation = false;
					}
				}
			}
			if (flag)
			{
				FSprite fSprite = new FSprite(infected ? "echoRingHalf" : "echoRing");
				fSprite.SetAnchor(0.5f, 0.5f);
				fSprite.color = RainWorld.SaturatedGold;
				map.container.AddChild(fSprite);
				ringIcons.Add(fSprite);
				ringFade = 0f;
			}
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			circle.Draw(timeStacker);
			selectionCircle.Draw(timeStacker);
		}

		public void UpdateGraphics()
		{
			circle.Update();
			Vector3 vector = Pos * map.warpMapDiameter / 2f;
			float num = Mathf.Lerp(0.35f, 1f, (Pos.z + 1f) / 2f);
			circle.pos = new Vector2(vector.x, vector.y) + map.hud.rainWorld.screenSize / 2f;
			circle.rad = 60f * num;
			circle.thickness = 4f * num;
			float num2 = 1f;
			if (newRegion && map.hud.owner.GetOwnerType() != global::HUD.HUD.OwnerType.FastTravelScreen && map.hud.owner.GetOwnerType() != global::HUD.HUD.OwnerType.WarpSafariMenu)
			{
				if (map.visible)
				{
					if (map.mapData.dontReplayAnimations)
					{
						fadeIn = 1f;
					}
					if (fadeIn < 1f)
					{
						fadeIn += 0.01f;
					}
					else
					{
						fadeCounter += 0.05f;
					}
				}
				else
				{
					fadeIn = 0f;
				}
				num2 = ((!(fadeIn < 1f)) ? (Mathf.Abs(Mathf.Sin(fadeCounter + (float)Math.PI / 2f)) * 0.5f + 0.5f) : Mathf.Clamp(fadeIn, 0f, 1f));
			}
			maxFade = Mathf.Lerp(maxFade, active ? 1f : 0.25f, 0.08f);
			num2 *= maxFade;
			float num3 = map.fade * num2;
			circle.fade = ((infected || spinningTopEncounter) ? 0f : num3);
			circle.color = CircleColor;
			selectionCircle.Update();
			selectionCircle.pos = circle.pos;
			selectionCircle.rad = ((infected || spinningTopEncounter) ? 80f : 70f) * num;
			selectionCircle.thickness = circle.thickness;
			selectionCircle.fade = (Selected ? num3 : 0f);
			selectionCircle.color = circle.color;
			if (map.ActiveTerraceAnimation || activeTerraceAnimation)
			{
				circle.fade = map.fade * num2 * (1f - ringFade);
				selectionCircle.rad = Mathf.Lerp(70f, 80f, ringFade * 4f) * num;
			}
			if (regionIcon != null)
			{
				regionIcon.x = Mathf.Lerp(selectionCircle.pos.x, selectionCircle.lastPos.x, 0.5f);
				regionIcon.y = Mathf.Lerp(selectionCircle.pos.y, selectionCircle.lastPos.y, 0.5f);
				regionIcon.scale = num;
				regionIcon.alpha = num3;
			}
			if (slugcatIcon != null)
			{
				slugcatIcon.x = regionIcon.x;
				slugcatIcon.y = regionIcon.y;
				slugcatIcon.scale = num;
				slugcatIcon.alpha = num3;
			}
			for (int i = 0; i < ringIcons.Count; i++)
			{
				ringIcons[i].x = regionIcon.x;
				ringIcons[i].y = regionIcon.y;
				ringIcons[i].scale = num;
				ringIcons[i].alpha = num3 * ringFade;
			}
			float num4 = 0.35f;
			for (int j = 0; j < voidFeathers.Length; j++)
			{
				if (voidFeatherProgs[j] >= 1f)
				{
					voidFeatherProgs[j] -= 1f;
					voidFeathers[j].SetElementByName("voidMapFeather" + UnityEngine.Random.Range(0, 5));
					voidFeatherOffsets[j] = UnityEngine.Random.Range(-1f, 1f);
					voidFeatherScales[j] = UnityEngine.Random.Range(0.2f, 0.7f);
				}
				if (voidFeatherProgs[j] <= num4)
				{
					voidFeathers[j].alpha = num3 * Mathf.Lerp(0f, 1f, voidFeatherProgs[j] / num4);
				}
				else if (voidFeatherProgs[j] >= 1f - num4)
				{
					voidFeathers[j].alpha = num3 * Mathf.Lerp(1f, 0f, (voidFeatherProgs[j] - (1f - num4)) / num4);
				}
				else
				{
					voidFeathers[j].alpha = num3;
				}
				voidFeathers[j].scale = num * voidFeatherScales[j];
				float num5 = circle.pos.x + circle.rad * 0.75f * voidFeatherOffsets[j];
				float num6 = circle.pos.y + circle.rad - circle.rad * 2f * voidFeatherProgs[j];
				float num7 = Mathf.Lerp(circle.rad * 0.7f, circle.rad * 0.25f, Mathf.Abs(voidFeatherOffsets[j]));
				float f = Mathf.Sin(voidFeatherProgs[j] * (float)Math.PI * 3f + voidFeatherOffsets[j] * (float)Math.PI * 2f);
				float num8 = Mathf.Lerp(230f, 310f, Mathf.Abs(f));
				voidFeathers[j].x = num5 + Custom.rotateVectorDeg(new Vector2(num7, 0f), num8).x;
				voidFeathers[j].y = num6 + Custom.rotateVectorDeg(new Vector2(num7 / 4f, 0f), num8).y;
				voidFeathers[j].rotation = 0f - (num8 - 270f);
			}
		}

		public void RefreshGraphics()
		{
			circle.ClearSprite();
			map.container.AddChild(circle.sprite);
			selectionCircle.ClearSprite();
			map.container.AddChild(selectionCircle.sprite);
			if (regionIcon != null)
			{
				regionIcon.RemoveFromContainer();
				map.container.AddChild(regionIcon);
			}
			if (slugcatIcon != null)
			{
				slugcatIcon.RemoveFromContainer();
				map.container.AddChild(slugcatIcon);
			}
			for (int i = 0; i < ringIcons.Count; i++)
			{
				ringIcons[i].RemoveFromContainer();
				map.container.AddChild(ringIcons[i]);
			}
			for (int j = 0; j < voidFeathers.Length; j++)
			{
				voidFeathers[j].RemoveFromContainer();
				map.container.AddChild(voidFeathers[j]);
			}
		}

		public override void ClearSprites()
		{
			if (circle != null)
			{
				circle.ClearSprite();
			}
			if (selectionCircle != null)
			{
				selectionCircle.ClearSprite();
			}
			if (regionIcon != null)
			{
				regionIcon.RemoveFromContainer();
				regionIcon = null;
			}
			if (slugcatIcon != null)
			{
				slugcatIcon.RemoveFromContainer();
				slugcatIcon = null;
			}
			for (int i = 0; i < ringIcons.Count; i++)
			{
				ringIcons[i].RemoveFromContainer();
			}
			for (int j = 0; j < voidFeathers.Length; j++)
			{
				voidFeathers[j].RemoveFromContainer();
			}
			voidFeathers = new FSprite[0];
			ringIcons.Clear();
		}
	}

	public class WarpRegionConnection : WarpMapObject
	{
		public WarpRegionIcon regionA;

		public WarpRegionIcon regionB;

		public bool playerGenerated;

		public bool oneWay;

		public bool sealedConnection;

		public FSprite lineSprite;

		public int segments;

		public bool newConnection;

		public float fadeCounter;

		public float fadeIn = -0.25f;

		public float maxFade = 1f;

		public bool active;

		public WarpRegionConnection(WarpMap map, WarpRegionIcon regionA, WarpRegionIcon regionB, bool playerGenerated, bool oneWayConnection, bool newConnection, bool sealedConnection)
			: base(map)
		{
			this.regionA = regionA;
			this.regionB = regionB;
			this.playerGenerated = playerGenerated;
			oneWay = oneWayConnection;
			this.newConnection = newConnection;
			this.sealedConnection = sealedConnection;
			regionA?.connections.Add(this);
			regionB?.connections.Add(this);
			InitiateSprites();
		}

		public override void Update()
		{
			if (newConnection && map.visible && map.hud.owner.GetOwnerType() != global::HUD.HUD.OwnerType.FastTravelScreen)
			{
				if (map.mapData.dontReplayAnimations)
				{
					fadeIn = 1f;
				}
				if (fadeIn < 1f)
				{
					fadeIn += 0.01f;
				}
				else
				{
					fadeCounter += 0.05f;
				}
			}
			maxFade = Mathf.Lerp(maxFade, active ? 1f : 0.5f, 0.08f);
		}

		public void InitiateSprites()
		{
			_ = regionA.circle.pos;
			_ = regionB.circle.pos;
			segments = 1;
			lineSprite = TriangleMesh.MakeLongMesh(segments, pointyTip: false, customColor: true);
			if (sealedConnection)
			{
				lineSprite.shader = map.hud.rainWorld.Shaders["MapSealedShortcut"];
			}
			else if (playerGenerated || oneWay)
			{
				lineSprite.shader = map.hud.rainWorld.Shaders["MapShortcut"];
			}
			else
			{
				lineSprite.shader = map.hud.rainWorld.Shaders["Basic"];
			}
			map.container.AddChild(lineSprite);
		}

		public override void Draw(float timeStacker)
		{
			lineSprite.isVisible = map.visible;
			Vector2 pos = regionA.circle.pos;
			Vector2 pos2 = regionB.circle.pos;
			Vector2 vector = pos;
			float num = 1f;
			if (newConnection && map.visible)
			{
				num = ((!(fadeIn < 1f)) ? (Mathf.Abs(Mathf.Sin(fadeCounter + (float)Math.PI / 2f)) * 0.5f + 0.5f) : Mathf.Clamp(fadeIn, 0f, 1f));
			}
			num *= maxFade;
			Color color = new Color(1f, oneWay ? 0.8f : 0.1f, 0f, map.fade * num);
			if (sealedConnection)
			{
				color = new Color(RainWorld.SaturatedGold.r, RainWorld.SaturatedGold.g, RainWorld.SaturatedGold.b, map.fade * num);
			}
			else if (!playerGenerated && !oneWay)
			{
				color = new Color(RainWorld.MapColor.r, RainWorld.MapColor.g, RainWorld.MapColor.b, map.fade * num);
			}
			if (lineSprite != null)
			{
				(lineSprite as TriangleMesh).verticeColors[0] = color;
				(lineSprite as TriangleMesh).verticeColors[1] = color;
				float num2 = Mathf.Lerp(1f, 4f, Mathf.InverseLerp(0.5f, 1f, maxFade));
				for (int i = 0; i < segments; i++)
				{
					float t = (float)(i + 1) / (float)segments;
					Vector2 vector2 = Vector2.Lerp(pos, pos2, t);
					Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
					(lineSprite as TriangleMesh).verticeColors[i * 4] = color;
					(lineSprite as TriangleMesh).verticeColors[i * 4 + 1] = color;
					(lineSprite as TriangleMesh).verticeColors[i * 4 + 2] = color;
					(lineSprite as TriangleMesh).verticeColors[i * 4 + 3] = color;
					(lineSprite as TriangleMesh).MoveVertice(i * 4, vector - vector3 * num2);
					(lineSprite as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * num2);
					(lineSprite as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num2);
					(lineSprite as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num2);
					vector = vector2;
				}
			}
		}

		public override void ClearSprites()
		{
			if (lineSprite != null)
			{
				lineSprite.RemoveFromContainer();
			}
		}
	}

	public class WarpFastTravelCursor : WarpMapObject
	{
		public class FakeHoldButton
		{
			public FSprite circleSprite;

			public float filled;

			public float lastFilled;

			public float fillTime;

			public bool hasSignalled;

			public int buttonReleasedCounter;

			public float pulse;

			public float lastPulse;

			public float rad;

			public float lastRad;

			public Vector2 pos;

			public Vector2 lastPos;

			public Color col;

			public Color lastCol;

			public string signalText;

			public bool controlledFromOutside;

			public bool held;

			public bool lastHeld;

			public MenuMicrophone.MenuSoundLoop soundLoop;

			public bool warningMode;

			public global::Menu.Menu menu;

			public bool FillingUp => filled > 0f;

			public FakeHoldButton(global::Menu.Menu menu, string singalText, float fillTime)
			{
				this.menu = menu;
				this.fillTime = fillTime;
				signalText = singalText;
				circleSprite = new FSprite("Futile_White");
				circleSprite.shader = menu.manager.rainWorld.Shaders["HoldButtonCircle"];
				col = Color.white;
			}

			public void Update()
			{
				if (held)
				{
					if (soundLoop == null)
					{
						soundLoop = menu.PlayLoop(SoundID.MENU_Security_Button_LOOP, 0f, 0f, 1f, isBkgLoop: false);
					}
					soundLoop.loopVolume = Mathf.Lerp(soundLoop.loopVolume, 1f, 0.85f);
					soundLoop.loopPitch = Mathf.Lerp(0.3f, 1.5f, filled) - 0.15f * Mathf.Sin(pulse * (float)Math.PI * 2f);
				}
				else if (!held && soundLoop != null)
				{
					soundLoop.loopVolume = Mathf.Max(0f, soundLoop.loopVolume - 0.125f);
					if (soundLoop.loopVolume <= 0f)
					{
						soundLoop.Destroy();
						soundLoop = null;
					}
				}
				if (held)
				{
					lastPulse = pulse;
					pulse += filled / 20f;
				}
				else
				{
					pulse = 0f;
					lastPulse = 0f;
				}
				lastFilled = filled;
				lastPos = pos;
				lastCol = col;
				lastRad = rad;
				if (held)
				{
					filled = Custom.LerpAndTick(filled, 1f, 0.007f, 1f / fillTime);
					if (filled >= 1f && !hasSignalled)
					{
						menu.Singal(null, signalText);
						hasSignalled = true;
						menu.ResetSelection();
					}
					buttonReleasedCounter = 0;
					if (!lastHeld)
					{
						menu.PlaySound(SoundID.MENU_Security_Button_Init);
					}
				}
				else
				{
					if (lastHeld && !hasSignalled)
					{
						menu.PlaySound(SoundID.MENU_Security_Button_Release);
					}
					if (hasSignalled)
					{
						buttonReleasedCounter++;
						if (buttonReleasedCounter > 30)
						{
							filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f);
							if (filled < 0.5f)
							{
								hasSignalled = false;
							}
						}
						else
						{
							filled = 1f;
						}
					}
					else
					{
						filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f);
					}
				}
				lastHeld = held;
			}

			public void GrafUpdate(float timeStacker)
			{
				float num = Mathf.Lerp(lastRad, rad, timeStacker);
				Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
				num += 0.5f;
				circleSprite.x = vector.x;
				circleSprite.y = vector.y;
				circleSprite.scale = num;
				circleSprite.color = Color.Lerp(col, lastCol, timeStacker);
				circleSprite.scale = (num + 15f) / 8f;
				circleSprite.alpha = Mathf.Lerp(lastFilled, filled, timeStacker);
			}

			public void RemoveSprites()
			{
				circleSprite.RemoveFromContainer();
				if (soundLoop != null)
				{
					soundLoop.Destroy();
				}
			}
		}

		public HUDCircle circle;

		public FSprite bkgFade;

		public FSprite[] lines;

		public FakeHoldButton holdButton;

		public FastTravelScreen fastTravelScreen;

		public WarpRegionIcon selectedRegion;

		public float snapToMarker;

		public float pulse;

		public float lastPulse;

		public int changeLayerCounter;

		public bool lastPlaceButton;

		public float fadeOpacity;

		public float lastFadeOpacity;

		public float buttonPrompt;

		public float lastButtonPrompt;

		public WarpFastTravelCursor(WarpMap map, FastTravelScreen fastTravelScreen)
			: base(map)
		{
			this.fastTravelScreen = fastTravelScreen;
			holdButton = new FakeHoldButton(fastTravelScreen, "RIPPLE WARP", 120f);
			map.inFrontContainer.AddChild(holdButton.circleSprite);
			bkgFade = new FSprite("Futile_White");
			bkgFade.shader = map.hud.rainWorld.Shaders["FlatLight"];
			bkgFade.color = new Color(0f, 0f, 0f);
			map.inFrontContainer.AddChild(bkgFade);
			bkgFade.isVisible = false;
			lines = new FSprite[4];
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = new FSprite("pixel");
				lines[i].anchorY = 0f;
				lines[i].scaleX = 2f;
				map.inFrontContainer.AddChild(lines[i]);
				lines[i].isVisible = false;
			}
			circle = new HUDCircle(map.hud, HUDCircle.SnapToGraphic.None, map.inFrontContainer, 0);
			circle.thickness = 2f;
			circle.pos = map.hud.rainWorld.screenSize / 2f;
			circle.lastPos = circle.pos;
			circle.fade = 0f;
			circle.lastFade = 0f;
			circle.rad = 0f;
		}

		public bool UnselectableRegion(WarpRegionIcon regionIcon)
		{
			if (regionIcon.visited && !Region.IsVanillaSentientRotRegion(regionIcon.region) && !Region.IsAncientUrbanRegion(regionIcon.region) && !Region.IsDaemonRegion(regionIcon.region))
			{
				return Region.IsSentientRotRegion(regionIcon.region);
			}
			return true;
		}

		public override void Update()
		{
			if (!map.visible)
			{
				snapToMarker = 0f;
				selectedRegion = null;
				circle.pos = map.hud.rainWorld.screenSize / 2f;
				circle.lastPos = circle.pos;
				circle.fade = 0f;
				circle.lastFade = 0f;
				circle.rad = 0f;
				return;
			}
			base.Update();
			lastPulse = pulse;
			pulse += 0.02f;
			lastFadeOpacity = fadeOpacity;
			WarpRegionIcon warpRegionIcon = map.selectedRegion;
			bool flag = true;
			if (selectedRegion != warpRegionIcon)
			{
				snapToMarker = Custom.LerpAndTick(snapToMarker, 0f, 0.01f, 0.05f);
				if (snapToMarker == 0f)
				{
					selectedRegion = warpRegionIcon;
					if (warpRegionIcon != null)
					{
						flag = UnselectableRegion(warpRegionIcon);
						if (flag)
						{
							fastTravelScreen.PlaySound(SoundID.MENU_Button_Select_Mouse);
						}
						else
						{
							fastTravelScreen.PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Mouse);
						}
					}
				}
			}
			else if (selectedRegion != null)
			{
				flag = UnselectableRegion(selectedRegion);
				snapToMarker = Custom.LerpAndTick(snapToMarker, 1f, 0.01f, 0.05f);
			}
			circle.Update();
			holdButton.Update();
			if (selectedRegion != null && snapToMarker > 0f)
			{
				circle.pos = Vector2.Lerp(map.hud.rainWorld.screenSize / 2f, selectedRegion.circle.pos, Custom.SCurve(snapToMarker, 0.3f));
			}
			else
			{
				circle.pos = map.hud.rainWorld.screenSize / 2f;
			}
			circle.rad = Mathf.Lerp(100f, ((selectedRegion == null) ? 100f : (selectedRegion.circle.rad + (flag ? 35f : 25f))) + 3f * Mathf.Sin(pulse * (float)Math.PI * 2f), Mathf.Pow(snapToMarker, 2.5f));
			if (selectedRegion == null)
			{
				circle.fade = Custom.LerpAndTick(circle.fade, 0f, 0.1f, 0.05f);
			}
			else if (flag)
			{
				circle.fade = Custom.LerpAndTick(circle.fade, 0.4f, 0.01f, 0.05f);
			}
			else
			{
				circle.fade = Custom.LerpAndTick(circle.fade, 1f, 0.05f, 0.05f);
			}
			circle.fade = Mathf.Min(circle.fade, map.fade);
			holdButton.held = map.hud.owner.MapInput.spec && selectedRegion != null && !flag && circle.visible && (holdButton.held || map.panVel.magnitude < 0.1f);
			if (holdButton.held)
			{
				map.panVel = Vector2.zero;
				map.speedUp = 0f;
			}
			holdButton.pos = circle.pos;
			holdButton.rad = circle.rad;
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			float num = Mathf.Lerp(circle.lastRad, circle.rad, timeStacker);
			float num2 = Mathf.Lerp(circle.lastFade, circle.fade, timeStacker);
			Vector2 vector = Vector2.Lerp(circle.lastPos, circle.pos, timeStacker);
			bkgFade.x = vector.x;
			bkgFade.y = vector.y;
			bkgFade.alpha = Mathf.Lerp(lastFadeOpacity, fadeOpacity, timeStacker) * 0.4f * num2;
			for (int i = 0; i < lines.Length; i++)
			{
				float num3 = -45f + 90f * (float)i;
				Vector2 vector2 = vector + Custom.DegToVec(num3) * num;
				lines[i].x = vector2.x;
				lines[i].y = vector2.y;
				lines[i].rotation = num3;
				lines[i].scaleY = Mathf.Lerp(45f - num, 10f, 0.5f);
				lines[i].alpha = num2;
				lines[i].isVisible = map.visible;
			}
			bkgFade.isVisible = map.visible;
			circle.Draw(timeStacker);
			holdButton.GrafUpdate(timeStacker);
			bkgFade.scale = 12.5f;
		}

		public override void ClearSprites()
		{
			base.ClearSprites();
			circle.ClearSprite();
			holdButton.RemoveSprites();
			bkgFade.RemoveFromContainer();
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i].RemoveFromContainer();
			}
		}
	}

	public Map.MapData mapData;

	public float warpMapDiameter = 600f;

	public FContainer inFrontContainer;

	public int invisibleCounter;

	public Vector2 panVel;

	public float speedUp;

	public int fadeCounter;

	public bool visible;

	public float fade;

	public float lastFade;

	public int terraceAnimationTime;

	public WarpRegionIcon selectedRegion;

	public List<WarpRegionIcon> warpRegionIcons;

	public List<WarpMapObject> objects;

	public FContainer container => hud.fContainers[0];

	public int TerraceAnimationFreq => Mathf.Max(20 - mapData.unencounteredSpinningTopRegions.Count, 10);

	public bool ActiveTerraceAnimation
	{
		get
		{
			if (hud.owner is SleepAndDeathScreen sleepAndDeathScreen)
			{
				return sleepAndDeathScreen.terraceAnimation;
			}
			return false;
		}
		set
		{
			if (hud.owner is SleepAndDeathScreen sleepAndDeathScreen)
			{
				sleepAndDeathScreen.terraceAnimation = value;
			}
		}
	}

	public SaveState GetSaveState()
	{
		SaveState result = null;
		if (hud.owner.GetOwnerType() == global::HUD.HUD.OwnerType.Player || hud.owner.GetOwnerType() == global::HUD.HUD.OwnerType.FastTravelScreen || hud.owner.GetOwnerType() == global::HUD.HUD.OwnerType.WarpFastTravelScreen || hud.owner.GetOwnerType() == global::HUD.HUD.OwnerType.RegionOverview || (ModManager.MSC && hud.owner.GetOwnerType() == MoreSlugcatsEnums.OwnerType.SafariOverseer))
		{
			result = hud.rainWorld.progression.currentSaveState;
		}
		else if (hud.owner.GetOwnerType() == global::HUD.HUD.OwnerType.SleepScreen || hud.owner.GetOwnerType() == global::HUD.HUD.OwnerType.DeathScreen)
		{
			result = (hud.owner as SleepAndDeathScreen).saveState;
		}
		return result;
	}

	public Region FindRegion(string name)
	{
		if (name == null)
		{
			return null;
		}
		Region[] array = null;
		if (hud.rainWorld.processManager.currentMainLoop is RainWorldGame rainWorldGame)
		{
			array = rainWorldGame.overWorld.regions;
		}
		else if (hud.rainWorld.processManager.currentMainLoop is FastTravelScreen fastTravelScreen)
		{
			array = fastTravelScreen.allRegions;
		}
		if (array == null)
		{
			array = Region.LoadAllRegions(SlugcatStats.SlugcatToTimeline(hud.rainWorld.progression.PlayingAsSlugcat), null);
		}
		Region[] array2 = array;
		foreach (Region region in array2)
		{
			if (region.name.ToLower() == name.ToLower())
			{
				return region;
			}
		}
		return null;
	}

	public WarpMap(global::HUD.HUD hud, Map.MapData mapData)
		: base(hud)
	{
		if (hud.owner is WarpSafariMenu)
		{
			warpMapDiameter = 450f;
		}
		this.mapData = mapData;
		objects = new List<WarpMapObject>();
		inFrontContainer = new FContainer();
		Futile.stage.AddChild(inFrontContainer);
		if (hud.owner.GetOwnerType() == global::HUD.HUD.OwnerType.WarpFastTravelScreen && hud.owner is FastTravelScreen { IsWarpFastTravelScreen: not false } fastTravelScreen)
		{
			objects.Add(new WarpFastTravelCursor(this, fastTravelScreen));
		}
		LoadWarpConnections();
	}

	public void LoadWarpConnections()
	{
		SaveState saveState = GetSaveState();
		List<string> list = new List<string>();
		if (saveState != null)
		{
			for (int i = 0; i < saveState.regionStates.Length; i++)
			{
				if (saveState.regionStates[i] != null)
				{
					list.Add(saveState.regionStates[i].regionName.ToLowerInvariant());
				}
				else if (saveState.regionLoadStrings[i] != null)
				{
					string[] array = Regex.Split(Regex.Split(saveState.regionLoadStrings[i], "<rgA>")[0], "<rgB>");
					list.Add(array[1].ToLowerInvariant());
				}
			}
		}
		warpRegionIcons = new List<WarpRegionIcon>();
		bool flag = !ActiveTerraceAnimation && (saveState == null || !saveState.miscWorldSaveData.seenTerraceDream);
		for (int j = 0; j < mapData.discoveredWarpRegions.Count; j++)
		{
			bool visited = list.Contains(mapData.discoveredWarpRegions[j].ToLowerInvariant()) || saveState == null;
			warpRegionIcons.Add(new WarpRegionIcon(this, j, mapData.discoveredWarpRegions[j], mapData.infectedWarpRegions.Contains(mapData.discoveredWarpRegions[j]), mapData.voidWeaverPresenceRegions.Contains(mapData.discoveredWarpRegions[j]), mapData.unencounteredSpinningTopRegions.Contains(mapData.discoveredWarpRegions[j]) && !flag, visited));
		}
		List<WarpRegionConnection> list2 = new List<WarpRegionConnection>();
		for (int k = 0; k < mapData.allWarpConnections.Count; k++)
		{
			if ((saveState != null && mapData.allWarpConnectionData[k].ExpiredOnCycleAndTriggeredWeaver(saveState.cycleNumber, saveState) && !mapData.allWarpConnectionData[k].wasNonDynamicWarpBeforeWeaverTriggered) || (saveState != null && mapData.allWarpConnectionData[k].ExpiredByLimitedUses()))
			{
				continue;
			}
			string[] array2 = mapData.allWarpConnections[k].Split('-');
			WarpRegionIcon regionA = null;
			WarpRegionIcon regionB = null;
			for (int l = 0; l < warpRegionIcons.Count; l++)
			{
				if (warpRegionIcons[l].region == array2[0])
				{
					regionA = warpRegionIcons[l];
					break;
				}
			}
			for (int m = 0; m < warpRegionIcons.Count; m++)
			{
				if (warpRegionIcons[m].region == array2[1])
				{
					regionB = warpRegionIcons[m];
					break;
				}
			}
			bool sealedConnection = mapData.allWarpConnectionData[k].weaverTriggeredCycle > 0;
			if (saveState != null)
			{
				if (mapData.allWarpConnectionData[k].destRoom != null && saveState.miscWorldSaveData.roomsSealedByWeaverAbility.Contains(mapData.allWarpConnectionData[k].destRoom.ToLowerInvariant()))
				{
					sealedConnection = true;
				}
				if (mapData.allWarpConnectionData[k].destRoom != null && saveState.miscWorldSaveData.roomsSealedByVoidWeaver.Contains(mapData.allWarpConnectionData[k].destRoom.ToLowerInvariant()))
				{
					sealedConnection = true;
				}
				if (mapData.allWarpConnectionSourceRooms[k] != null && saveState.miscWorldSaveData.roomsSealedByWeaverAbility.Contains(mapData.allWarpConnectionSourceRooms[k].ToLowerInvariant()))
				{
					sealedConnection = true;
				}
				if (mapData.allWarpConnectionSourceRooms[k] != null && saveState.miscWorldSaveData.roomsSealedByVoidWeaver.Contains(mapData.allWarpConnectionSourceRooms[k].ToLowerInvariant()))
				{
					sealedConnection = true;
				}
			}
			list2.Add(new WarpRegionConnection(this, regionA, regionB, mapData.allWarpConnectionData[k].dynamicWarpPoint, mapData.allWarpConnectionData[k].oneWay, mapData.newWarpConnections.Contains(mapData.allWarpConnections[k]), sealedConnection));
		}
		objects.AddRange(warpRegionIcons);
		objects.AddRange(list2);
	}

	public override void Update()
	{
		if (visible)
		{
			invisibleCounter = 0;
		}
		else
		{
			invisibleCounter++;
		}
		lastFade = fade;
		panVel *= Custom.LerpMap(panVel.magnitude, 0f, 2.5f + 4f * Custom.SCurve(Mathf.InverseLerp(0.5f, 1f, speedUp), 0.65f), 1f, 0.85f);
		base.Update();
		WarpRegionIcon warpRegionIcon = null;
		float num = 5f;
		if (invisibleCounter == 5)
		{
			mapData.ResetWarpMapToCenter();
		}
		for (int i = 0; i < objects.Count; i++)
		{
			objects[i].Update();
		}
		for (int num2 = warpRegionIcons.Count - 1; num2 >= 0; num2--)
		{
			float num3 = Vector3.Distance(new Vector3(0f, 0f, 1f), warpRegionIcons[num2].Pos);
			if (warpRegionIcons[num2].region.ToLowerInvariant() == "wrsa")
			{
				num3 = 0.35f;
			}
			if (num3 < num)
			{
				warpRegionIcon = warpRegionIcons[num2];
				num = num3;
			}
		}
		List<WarpRegionIcon> list = warpRegionIcons.OrderBy((WarpRegionIcon o) => o.circle.rad).ToList();
		List<WarpRegionIcon> list2 = new List<WarpRegionIcon>();
		bool flag = warpRegionIcon != null && (num < 0.25f || warpRegionIcon.region.ToLowerInvariant() == "wrsa");
		if (flag)
		{
			if (!warpRegionIcon.Selected)
			{
				warpRegionIcon.Selected = true;
				for (int j = 0; j < warpRegionIcons.Count; j++)
				{
					if (warpRegionIcons[j].active)
					{
						warpRegionIcons[j].active = false;
						for (int k = 0; k < warpRegionIcons[j].connections.Count; k++)
						{
							warpRegionIcons[j].connections[k].active = false;
						}
					}
				}
				if (!list2.Contains(warpRegionIcon))
				{
					list2.Add(warpRegionIcon);
				}
				for (int l = 0; l < warpRegionIcon.connections.Count; l++)
				{
					warpRegionIcon.connections[l].active = true;
					if (!list2.Contains(warpRegionIcon.connections[l].regionA))
					{
						list2.Add(warpRegionIcon.connections[l].regionA);
					}
					if (!list2.Contains(warpRegionIcon.connections[l].regionB))
					{
						list2.Add(warpRegionIcon.connections[l].regionB);
					}
				}
			}
			if (visible && hud.owner.MapInput.jmp && warpRegionIcon.visited && TryGoToRegionMap(warpRegionIcon.region))
			{
				return;
			}
		}
		if (selectedRegion != null && !flag)
		{
			selectedRegion.Selected = false;
		}
		for (int m = 0; m < list.Count; m++)
		{
			if (!list2.Contains(list[m]))
			{
				list[m].UpdateGraphics();
				list[m].RefreshGraphics();
			}
		}
		if (list2.Count > 0)
		{
			List<WarpRegionIcon> list3 = list2.OrderBy((WarpRegionIcon o) => o.circle.rad).ToList();
			for (int n = 0; n < list3.Count; n++)
			{
				list3[n].active = true;
				list3[n].UpdateGraphics();
				list3[n].RefreshGraphics();
			}
		}
		if (hud.owner.RevealMap)
		{
			fadeCounter += ((!ModManager.MMF || !MMF.cfgFastMapReveal.Value) ? 1 : 2);
			if (fadeCounter > 30 || hud.owner.GetOwnerType() != global::HUD.HUD.OwnerType.Player)
			{
				fade = Mathf.Min(fade + 1f / 30f, 1f);
				fade = Mathf.Lerp(fade, 1f, 0.1f);
			}
		}
		else
		{
			fadeCounter -= 5;
			if (fadeCounter < 0)
			{
				fadeCounter = 0;
			}
			fade = Mathf.Max(fade - 0.05f, 0f);
			fade = Mathf.Lerp(fade, 0f, 0.1f);
		}
		if (hud.HideGeneralHud)
		{
			fade = 0f;
		}
		visible = fade > 0f && lastFade > 0f;
		if (visible && fadeCounter > 30 && ActiveTerraceAnimation)
		{
			int num4 = ((!RWInput.PlayerInput(0).mp) ? 1 : 3);
			for (int num5 = 0; num5 < num4; num5++)
			{
				UpdateTerraceAnimation();
			}
		}
		Vector2 vector = hud.owner.MapInput.analogueDir;
		if (vector.x == 0f && vector.y == 0f && (hud.owner.MapInput.x != 0 || hud.owner.MapInput.y != 0))
		{
			vector = Custom.DirVec(new Vector2(0f, 0f), new Vector2(hud.owner.MapInput.x, hud.owner.MapInput.y));
		}
		if (vector.magnitude > 0.1f)
		{
			speedUp = Mathf.Clamp01(speedUp + Custom.LerpMap(Vector2.Dot(panVel.normalized, vector.normalized) * vector.magnitude, 0.6f, 0.8f, -1f / Custom.LerpMap(speedUp, 0.5f, 1f, 7f, 30f), 1f / 140f));
			panVel += vector * (0.45f + 0.1f * Mathf.InverseLerp(0.5f, 1f, speedUp));
		}
		else
		{
			speedUp = Mathf.Max(0f, speedUp - 1f / 30f);
			panVel *= 0.8f;
		}
	}

	public void UpdateTerraceAnimation()
	{
		bool flag = mapData.unencounteredSpinningTopRegions.Count - 1 < terraceAnimationTime / TerraceAnimationFreq;
		if (terraceAnimationTime % TerraceAnimationFreq == 0 && !flag)
		{
			string text = mapData.unencounteredSpinningTopRegions[mapData.unencounteredSpinningTopRegions.Count - 1 - terraceAnimationTime / TerraceAnimationFreq];
			foreach (WarpRegionIcon warpRegionIcon in warpRegionIcons)
			{
				if (warpRegionIcon.region.ToLowerInvariant() == text.ToLowerInvariant())
				{
					warpRegionIcon.activeTerraceAnimation = true;
				}
			}
		}
		if (flag)
		{
			bool flag2 = false;
			foreach (WarpRegionIcon warpRegionIcon2 in warpRegionIcons)
			{
				if (warpRegionIcon2.activeTerraceAnimation)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				ActiveTerraceAnimation = false;
			}
		}
		terraceAnimationTime++;
	}

	public bool TryGoToRegionMap(string region)
	{
		SaveState saveState = GetSaveState();
		if (saveState == null)
		{
			return false;
		}
		Region region2 = FindRegion(region);
		if (region2 != null)
		{
			WorldLoader worldLoader = new WorldLoader(null, hud.rainWorld.progression.PlayingAsSlugcat, SlugcatStats.SlugcatToTimeline(hud.rainWorld.progression.PlayingAsSlugcat), singleRoomWorld: false, region2.name, region2, hud.rainWorld.setup, WorldLoader.LoadingContext.FASTTRAVEL);
			worldLoader.NextActivity();
			while (!worldLoader.Finished)
			{
				worldLoader.Update();
				Thread.Sleep(1);
			}
			if (saveState.regionStates[region2.regionNumber] == null)
			{
				saveState.regionStates[region2.regionNumber] = new RegionState(saveState, worldLoader.ReturnWorld());
			}
			Map.MapData mapData = new Map.MapData(worldLoader.ReturnWorld(), hud.rainWorld);
			mapData.currentPlayerRegion = this.mapData.currentPlayerRegion;
			hud.InitFastTravelHud(mapData);
			return true;
		}
		return false;
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		for (int i = 0; i < objects.Count; i++)
		{
			objects[i].Draw(timeStacker);
		}
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		for (int i = 0; i < objects.Count; i++)
		{
			objects[i].ClearSprites();
		}
		inFrontContainer.RemoveFromContainer();
	}

	public static Vector2 RotateAroundCircle(Vector2 pos, float angleRotate)
	{
		float f = Mathf.Atan2(pos.y, pos.x) + angleRotate;
		return new Vector2(Mathf.Cos(f) * pos.magnitude, Mathf.Sin(f) * pos.magnitude);
	}
}
