using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HUD;
using Music;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class MonolithScene : UpdatableAndDeletable, IDrawable
{
	public class Element
	{
		public Vector3 basePos;

		public Vector3 pos;

		public Vector3 posView;

		public Vector2 drawPos;

		public Vector2 lastDrawPos;

		public FSprite sprite;

		public float animOffset;

		public float depth;

		public float lastDepth;

		public Vector2 baseScale;

		public Vector2 scale;

		public Vector2 drawScale;

		public Vector2 lastDrawScale;

		public Movement movement;

		public float time;

		public float lastTime;

		public float timeIncrement = 0.1f;

		public float rotation;

		public float lastRotation;

		public View viewPoint;

		public Vector3 randomVector;

		public Vector3 individualVariation;

		public int depthChannel = -1;

		public Element(Vector3 pos, Vector2 scale, FSprite sprite, View viewPoint, float animOffset = 0f, Movement movement = null)
		{
			basePos = pos;
			baseScale = scale;
			this.animOffset = animOffset;
			this.sprite = sprite;
			this.movement = movement;
			this.viewPoint = viewPoint;
			randomVector = Custom.RNV3();
			individualVariation = new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
		}

		public void Update()
		{
			lastDrawPos = drawPos;
			lastDrawScale = drawScale;
			lastRotation = rotation;
			lastDepth = depth;
			lastTime = time;
			time += timeIncrement;
			scale = baseScale;
			pos = basePos;
			movement?.Move(this);
			viewPoint.ApplyViewPoint(this);
			movement?.MoveAfter(this);
		}

		public static void SortByDepth(List<Element> elements)
		{
			elements.Sort((Element s1, Element s2) => s2.pos.z.CompareTo(s1.pos.z));
		}

		public void SetDepthToColorChannel(float depth)
		{
			depth = Mathf.InverseLerp(0.1f, 0f, depth);
			Color color = sprite.color;
			if (depthChannel == 0)
			{
				color.r = depth;
			}
			else if (depthChannel == 1)
			{
				color.g = depth;
			}
			else if (depthChannel == 2)
			{
				color.b = depth;
			}
			else if (depthChannel == 3)
			{
				color.a = depth;
			}
			sprite.color = color;
		}

		public static Element CreateLink(Element a, Element b, float size, Color color, View viewPoint)
		{
			return new Element(Vector3.zero, Vector2.one * size, new FSprite("Futile_White")
			{
				color = color,
				shader = Custom.rainWorld.Shaders["MonolithLine"]
			}, viewPoint, 0f, new Connection(a, b));
		}

		public static Element CreateSphere(Vector3 point, Movement orbit, float animOffset, float size, View viewPoint)
		{
			Color.Lerp(Color.black, Color.white, animOffset);
			Element element = new Element(point, Vector2.one * size, new FSprite("Futile_White")
			{
				shader = Custom.rainWorld.Shaders["MonolithSphere"]
			}, viewPoint, animOffset, orbit);
			element.sprite.color = new Color(element.individualVariation.x, 0f, 0f, 0f);
			element.depthChannel = 1;
			return element;
		}

		public static Element[] RandomLinks(List<Element> points, int nConnections, float size, Color color, View viewPoint)
		{
			Element[] array = new Element[nConnections];
			int num = -1;
			points.Count();
			for (int i = 0; i < nConnections; i++)
			{
				int num2 = ((num == -1) ? UnityEngine.Random.Range(0, points.Count - 1) : num);
				int num3 = (num2 + UnityEngine.Random.Range(1, points.Count - 1)) % points.Count;
				num = num3;
				array[i] = CreateLink(points[num2], points[num3], size, color, viewPoint);
			}
			return array;
		}

		public static Element[] SphereRing(Vector3 point, int nSpheres, float sphereSize, Movement orbit, View viewPoint)
		{
			Element[] array = new Element[nSpheres];
			for (int i = 0; i < nSpheres; i++)
			{
				float t = (float)i / (float)nSpheres;
				Color.Lerp(Color.black, Color.white, t);
				Element element = CreateSphere(point, orbit, t, sphereSize, viewPoint);
				array[i] = element;
			}
			return array;
		}

		public static Element CreateMonolith(Vector3 point, Vector2 size, Color color, View viewPoint = null)
		{
			return new Element(point, size, new FSprite("Futile_White")
			{
				shader = Custom.rainWorld.Shaders["Monolith"],
				anchorY = 0.2f,
				color = color
			}, viewPoint)
			{
				depthChannel = 1
			};
		}

		public static Element CreateDetailedMonolith(string name, Vector3 point, Vector2 size, Color color, View viewPoint = null)
		{
			FAtlasElement orLoadFAtlasElement = GetOrLoadFAtlasElement(name);
			if (orLoadFAtlasElement == null)
			{
				return CreateMonolith(point, size, color, viewPoint);
			}
			return new Element(point, size, new FSprite(orLoadFAtlasElement)
			{
				shader = Custom.rainWorld.Shaders["MonolithDetailed"],
				anchorY = 0.2f,
				color = color
			}, viewPoint)
			{
				depthChannel = 1
			};
		}
	}

	public abstract class Movement
	{
		public abstract void Move(Element element);

		public abstract void MoveAfter(Element element);

		public Movement()
		{
		}
	}

	public class Orbit : Movement
	{
		public float radius;

		public float speed;

		public float axisAmount;

		public float disorder;

		public Vector3 axis;

		public Orbit(float radius, float speed, Vector3? axis = null, float disorder = 0f)
		{
			this.radius = radius;
			this.speed = speed;
			this.axis = axis ?? Vector3.up;
			axisAmount = 1f;
		}

		public override void Move(Element element)
		{
			AdjustSpeed(element);
			float f = element.time * 0.01f + element.animOffset * (float)Math.PI * 2f;
			float x = Mathf.Sin(f);
			float z = Mathf.Cos(f);
			Vector3 vector = new Vector3(x, 0f, z) * radius;
			if (axis != Vector3.up || disorder > 0f)
			{
				Quaternion b = Quaternion.FromToRotation(Vector3.up, Vector3.Slerp(axis, element.randomVector, disorder));
				vector = Quaternion.Slerp(Quaternion.identity, b, axisAmount) * vector;
			}
			element.pos += vector;
		}

		public override void MoveAfter(Element element)
		{
		}

		public void AdjustSpeed(Element element)
		{
			element.timeIncrement = speed * 0.1f;
		}
	}

	public class Connection : Movement
	{
		public Element a;

		public Element b;

		public Connection(Element firstElement, Element secondElement)
		{
			a = firstElement;
			b = secondElement;
		}

		public override void Move(Element element)
		{
			element.pos = Vector3.Lerp(a.pos, b.pos, 0.5f);
		}

		public override void MoveAfter(Element element)
		{
			element.rotation = Custom.AimFromOneVectorToAnother(a.drawPos, b.drawPos);
			element.drawScale.y = Vector2.Distance(a.drawPos, b.drawPos) * ARZapper.magicScale;
			element.drawScale.x = element.baseScale.x * Mathf.Min(a.depth, b.depth);
			element.drawPos = Vector2.Lerp(a.drawPos, b.drawPos, 0.5f);
		}
	}

	public class View
	{
		public float nearPlane;

		public float nearPlaneInitial;

		public Vector3 pos;

		public Vector2 horizonPoint;

		public View(Vector3 pos, float nearPlane, Vector2? horizonPoint = null)
		{
			this.pos = pos;
			this.nearPlane = nearPlane;
			nearPlaneInitial = nearPlane;
			this.horizonPoint = horizonPoint ?? (Vector2.one * 0.5f);
		}

		public void ApplyViewPoint(Element element)
		{
			element.posView = element.pos - pos + new Vector3(sSize.x * 0.5f, sSize.y * 0.5f, 0f);
			element.sprite.isVisible = element.posView.z > 0f;
			element.depth = DepthScale(element.posView, nearPlane);
			element.drawScale = element.scale * element.depth;
			element.drawPos = DrawPos(element.posView, element.depth, horizonPoint);
		}

		public static Vector2 DrawPos(Vector3 pos, float depth, Vector2 horizon)
		{
			return Vector2.LerpUnclamped(Custom.rainWorld.options.ScreenSize * horizon, new Vector2(pos.x, pos.y), depth);
		}

		public static float DepthScale(Vector3 pos, float nearPlane)
		{
			return nearPlane / Mathf.Max(pos.z, 1f);
		}
	}

	public FContainer container;

	public List<Element> elements;

	public List<Element> bgElements;

	public View viewPoint;

	public float time;

	public float lastTime;

	public Vector2 monolithSize = new Vector2(40f, 80f);

	public float sphereSize = 20f;

	public int nBgSprites;

	public Color bgColor = new Color(0.106f, 0.129f, 0.176f);

	public Color monolithColor = new Color(0.106f, 0.129f, 0.176f, 0f);

	public Color linkColor;

	public Color linkColor2;

	public Orbit mainOrbit;

	public Orbit secondaryOrbit;

	public List<Element> innerCircle;

	public List<Element> monolithSpheres;

	public List<Element> links;

	public float sceneProgression;

	public float lastSceneProgression;

	public Element zoomSphere;

	public Element neighbour1;

	public Element neighbour2;

	public Element monolith;

	public Vector2 horizonPoint = new Vector2(0.5f, 0.85f);

	public Song daemonSong;

	public bool useSong = true;

	public bool isInit;

	public float sphereOpen;

	public float lastSphereOpen;

	public float sphereTransition;

	public float lastSphereTransition;

	public float monolithGlow;

	public float lastMonolithGlow;

	public int seed = 1000;

	public static Vector2 sSize => Custom.rainWorld.screenSize;

	public MonolithScene(Room room)
	{
		base.room = room;
		linkColor = RainWorld.RippleGold;
		linkColor2 = RainWorld.SaturatedGold;
		elements = new List<Element>();
		bgElements = new List<Element>();
		innerCircle = new List<Element>();
		monolithSpheres = new List<Element>();
		links = new List<Element>();
		if (room.game.manager.musicPlayer == null)
		{
			useSong = false;
		}
		CreateElements();
	}

	public void CreateElements()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		viewPoint = new View(Point(0f, 1500f, -3000f), 1000f, horizonPoint);
		mainOrbit = new Orbit(1500f, 2f);
		secondaryOrbit = new Orbit(3500f, -2f);
		Vector3 vector = Point(0f, 0f, 2300f);
		innerCircle.AddRange(Element.SphereRing(vector, 20, sphereSize, mainOrbit, viewPoint));
		elements.AddRange(innerCircle);
		monolith = Element.CreateDetailedMonolith("daemon_resized_alt", vector + Vector3.down * 2500f, Vector2.one * 5f, monolithColor, viewPoint);
		elements.Add(monolith);
		monolithSpheres.AddRange(Element.SphereRing(vector, 30, sphereSize, secondaryOrbit, viewPoint));
		elements.AddRange(monolithSpheres);
		monolithSpheres.AddRange(innerCircle);
		elements.AddRange(Element.SphereRing(Point(-2000f, 0f, -4000f), 20, sphereSize, new Orbit(2500f, 1f), viewPoint));
		elements.AddRange(Element.SphereRing(Point(3000f, 1000f, -6000f), 20, sphereSize, new Orbit(3500f, 1f), viewPoint));
		Vector3 vector2 = Point(6000f, -1000f, 13000f);
		elements.AddRange(Element.SphereRing(vector2, 20, sphereSize, new Orbit(2500f, 1f), viewPoint));
		elements.Add(Element.CreateDetailedMonolith("daemon_resized_small", vector2 + Vector3.down * 1000f, Vector2.one * 7f, monolithColor, viewPoint));
		elements.AddRange(Element.SphereRing(vector2, 20, sphereSize, new Orbit(3500f, 1f, Custom.RNV3()), viewPoint));
		vector2 = Point(-10000f, -2000f, 20000f);
		elements.AddRange(Element.SphereRing(vector2, 15, sphereSize, new Orbit(2500f, 1f), viewPoint));
		elements.Add(Element.CreateDetailedMonolith("daemon_resized_small", vector2 + Vector3.down * 1000f, Vector2.one * 7f, monolithColor, viewPoint));
		elements.AddRange(Element.SphereRing(vector2, 15, sphereSize, new Orbit(4000f, 1f, Custom.RNV3()), viewPoint));
		vector2 = Point(-7000f, 0f, 45000f);
		elements.AddRange(Element.SphereRing(vector2, 15, sphereSize, new Orbit(2500f, 1f), viewPoint));
		elements.Add(Element.CreateMonolith(vector2, monolithSize, monolithColor, viewPoint));
		elements.AddRange(Element.SphereRing(vector2, 15, sphereSize, new Orbit(4000f, 1f, Custom.RNV3()), viewPoint));
		vector2 = Point(8000f, 0f, 85000f);
		bgElements.AddRange(Element.SphereRing(vector2, 10, sphereSize, new Orbit(2500f, 1f), viewPoint));
		bgElements.Add(Element.CreateMonolith(vector2, monolithSize, monolithColor, viewPoint));
		sphereSize = 40f;
		bgElements.AddRange(Element.SphereRing(Point(30000f, -10000f, 185000f), 40, sphereSize, new Orbit(80000f, 2f, Custom.RNV3()), viewPoint));
		bgElements.AddRange(Element.SphereRing(Point(40000f, -40000f, 145000f), 40, sphereSize, new Orbit(70000f, 2f, Custom.RNV3()), viewPoint));
		bgElements.AddRange(Element.SphereRing(Point(-40000f, -40000f, 175000f), 40, sphereSize, new Orbit(75000f, 2f, Custom.RNV3()), viewPoint));
		bgElements.AddRange(Element.SphereRing(Point(-10000f, 40000f, 150000f), 40, sphereSize, new Orbit(75000f, 2f, Custom.RNV3()), viewPoint));
		links.AddRange(Element.RandomLinks(monolithSpheres, 80, 5f, linkColor, viewPoint));
		elements.AddRange(links);
		UnityEngine.Random.state = state;
	}

	public static Vector3 Point(float x, float y, float z)
	{
		return new Vector3(x, y, z);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastTime = time;
		if (room.BeingViewed || !isInit)
		{
			if (useSong)
			{
				time = ((float?)daemonSong?.subTracks[0]?.source?.timeSamples / 44100f * 0.24f).GetValueOrDefault();
			}
			else
			{
				time += 0.008f;
			}
			UpdateScene();
			for (int i = 0; i < bgElements.Count; i++)
			{
				bgElements[i].Update();
			}
			for (int j = 0; j < elements.Count; j++)
			{
				elements[j].Update();
			}
			Element.SortByDepth(elements);
			isInit = true;
		}
	}

	public override void PausedUpdate()
	{
		if (room.BeingViewed)
		{
			base.PausedUpdate();
			if (useSong && daemonSong != null)
			{
				daemonSong.subTracks[0].source.pitch = ((!room.game.GamePaused) ? 1 : 0);
			}
		}
	}

	public void UpdateScene()
	{
		lastSceneProgression = sceneProgression;
		sceneProgression = time - 12.33f;
		if (sceneProgression > 10f)
		{
			Destroy();
			return;
		}
		lastSphereOpen = sphereOpen;
		lastSphereTransition = sphereTransition;
		lastMonolithGlow = monolithGlow;
		if (sceneProgression < 8f)
		{
			HideHUD();
		}
		if (sceneProgression < 6.5f)
		{
			PreventPlayerMovement();
		}
		if (sceneProgression < 9f)
		{
			PreventRoomExit();
		}
		float t = Custom.LerpExpEaseOut(0f, 1f, Mathf.InverseLerp(-8.76f, -6.36f, sceneProgression));
		float t2 = Custom.LerpExpEaseOut(0f, 1f, Mathf.InverseLerp(-10f, 3f, sceneProgression));
		float t3 = Custom.LerpQuadEaseInOut(0f, 1f, Mathf.InverseLerp(-1f, 2.7f, sceneProgression));
		float num = Custom.LerpQuadEaseInOut(1f, 0f, Mathf.InverseLerp(3f, 4.2f, sceneProgression) * Mathf.InverseLerp(6f, 4.8f, sceneProgression));
		float disorder = Custom.LerpExpEaseInOut(1f, 0f, Mathf.InverseLerp(4f, 5f, sceneProgression));
		float num2 = Custom.LerpExpEaseInOut(0f, 1f, Mathf.InverseLerp(3.68f, 4.68f, sceneProgression));
		sphereOpen = Custom.LerpQuadEaseInOut(0f, 1f, Mathf.InverseLerp(5f, 6f, sceneProgression));
		monolithGlow = (1f - num) * 0.4f + num2 * (1f - sphereOpen) * 0.7f;
		sphereTransition = Custom.LerpQuadEaseInOut(0f, 1f, Mathf.InverseLerp(7.8f, 9f, sceneProgression));
		float t4 = Custom.LerpQuadEaseInOut(0f, 1f, Mathf.InverseLerp(5.7f, 9f, sceneProgression));
		float t5 = Custom.LerpQuadEaseInOut(0f, 1f, Mathf.InverseLerp(5.7f, 8f, sceneProgression));
		if (sceneProgression > 5f && zoomSphere == null)
		{
			Element.SortByDepth(innerCircle);
			zoomSphere = innerCircle[innerCircle.Count - 1];
			neighbour1 = innerCircle[innerCircle.Count - 2];
			neighbour2 = innerCircle[innerCircle.Count - 3];
		}
		mainOrbit.disorder = disorder;
		secondaryOrbit.disorder = disorder;
		mainOrbit.speed = num;
		secondaryOrbit.speed = num;
		viewPoint.pos = Vector3.Lerp(Point(0f, 1500f, -900000f), Point(0f, 1500f, -3000f), t2);
		viewPoint.horizonPoint = Vector2.Lerp(horizonPoint + Vector2.down * 0.25f, horizonPoint, t3);
		viewPoint.nearPlane = Mathf.Lerp(10f, 600f, t);
		viewPoint.nearPlane = Mathf.Lerp(viewPoint.nearPlane, 1000f, t2);
		viewPoint.nearPlane = Mathf.Lerp(viewPoint.nearPlane, 400f, t4);
		if (zoomSphere != null)
		{
			viewPoint.pos = new Vector3(Mathf.Lerp(viewPoint.pos.x, zoomSphere.pos.x, t5), Mathf.Lerp(viewPoint.pos.y, zoomSphere.pos.y, t5), Mathf.Lerp(viewPoint.pos.z, zoomSphere.pos.z, t4));
			viewPoint.horizonPoint = Vector2.Lerp(horizonPoint, Vector2.one * 0.5f, t5);
		}
		room.game.cameras[0].virtualMicrophone.globalSoundMuffle = 1f - sphereTransition;
		room.game.cameras[0].virtualMicrophone.underWater = 1f - sphereTransition;
	}

	public void HideHUD()
	{
		global::HUD.HUD hud = room.game.cameras[0].hud;
		if (hud != null)
		{
			hud.textPrompt.subregionTracker.counter = 0;
			hud.foodMeter.visibleCounter = 0;
			hud.karmaMeter.forceVisibleCounter = 0;
			hud.rainMeter.remainVisibleCounter = 0;
		}
	}

	public void PreventPlayerMovement()
	{
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature != null)
			{
				room.game.Players[i].realizedCreature.stun = Mathf.Max(room.game.Players[i].realizedCreature.stun, 2);
			}
		}
	}

	public void PreventRoomExit()
	{
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature != null)
			{
				room.game.Players[i].realizedCreature.shortcutDelay = Mathf.Max(room.game.Players[i].realizedCreature.shortcutDelay, 2);
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (useSong && daemonSong == null)
		{
			daemonSong = room.game.manager.musicPlayer.RequestDaemonSequenceSong();
		}
		Vector2 vector = sSize / 16f + Vector2.one * 0.5f;
		nBgSprites = 4;
		sLeaser.sprites = new FSprite[nBgSprites + 1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["MonolithSphere"];
		sLeaser.sprites[0].scale = 1f;
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].scaleX = vector.x;
		sLeaser.sprites[1].scaleY = vector.y;
		sLeaser.sprites[1].anchorX = 0f;
		sLeaser.sprites[1].anchorY = 0f;
		sLeaser.sprites[1].shader = Custom.rainWorld.Shaders["MonolithGlimmer"];
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = Custom.rainWorld.Shaders["Gradient"];
		sLeaser.sprites[2].scaleX = vector.x;
		sLeaser.sprites[2].scaleY = vector.y * 3f;
		sLeaser.sprites[2].anchorX = 0f;
		sLeaser.sprites[2].anchorY = -0.1f;
		sLeaser.sprites[2].y = sSize.y * viewPoint.horizonPoint.y;
		sLeaser.sprites[3] = new FSprite("Futile_White");
		sLeaser.sprites[3].shader = Custom.rainWorld.Shaders["Gradient"];
		sLeaser.sprites[3].scaleX = vector.x;
		sLeaser.sprites[3].scaleY = vector.y * -3f;
		sLeaser.sprites[3].anchorX = 0f;
		sLeaser.sprites[3].anchorY = -0.1f;
		sLeaser.sprites[3].y = sSize.y * viewPoint.horizonPoint.y;
		sLeaser.sprites[4] = new FSprite("Futile_White");
		sLeaser.sprites[4].scaleX = vector.x;
		sLeaser.sprites[4].scaleY = vector.y;
		sLeaser.sprites[4].anchorX = 0f;
		sLeaser.sprites[4].anchorY = 0f;
		sLeaser.sprites[4].shader = Custom.rainWorld.Shaders["MonolithFly"];
		container = rCam.ReturnFContainer("WarpPoint");
		Element.SortByDepth(bgElements);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = Mathf.Lerp(lastSceneProgression, sceneProgression, timeStacker);
		Shader.SetGlobalVector("_MonolithFogColor", bgColor);
		Shader.SetGlobalFloat("_MonolithSceneTime", num);
		Shader.SetGlobalVector("_MonolithHorizon", viewPoint.horizonPoint);
		Shader.SetGlobalFloat("_MonolithGlimmerScale", 1f / (viewPoint.nearPlane / viewPoint.nearPlaneInitial));
		if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room || (rCam.AboutToSwitchCameraPos && rCam.AboutToSwitchRoom)))
		{
			sLeaser.CleanSpritesAndRemove();
			for (int i = 0; i < elements.Count; i++)
			{
				elements[i].sprite.RemoveFromContainer();
			}
			for (int j = 0; j < bgElements.Count; j++)
			{
				bgElements[j].sprite.RemoveFromContainer();
			}
			return;
		}
		int num2 = 1;
		num2 = ((bgElements.Count <= 0) ? (num2 + container.GetChildIndex(sLeaser.sprites[nBgSprites - 1])) : (num2 + container.GetChildIndex(bgElements[bgElements.Count - 1].sprite)));
		sLeaser.sprites[2].y = sSize.y * viewPoint.horizonPoint.y;
		sLeaser.sprites[3].y = sSize.y * viewPoint.horizonPoint.y;
		float a = Mathf.Lerp(lastMonolithGlow, monolithGlow, timeStacker);
		if (monolith != null)
		{
			Color color = monolith.sprite.color;
			color.a = a;
			monolith.sprite.color = color;
		}
		for (int k = 0; k < elements.Count; k++)
		{
			Vector3 vector = Vector3.Lerp(elements[k].lastDrawPos, elements[k].drawPos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(elements[k].lastDrawScale, elements[k].drawScale, timeStacker);
			float rotation = Mathf.LerpAngle(elements[k].lastRotation, elements[k].rotation, timeStacker);
			elements[k].sprite.SetPosition(vector);
			elements[k].sprite.scaleX = vector2.x;
			elements[k].sprite.scaleY = vector2.y;
			elements[k].sprite.rotation = rotation;
			if (elements[k].depthChannel > 0 && elements[k].depthChannel < 4)
			{
				elements[k].SetDepthToColorChannel(Mathf.Lerp(elements[k].lastDepth, elements[k].depth, timeStacker));
			}
			container.AddChildAtIndex(elements[k].sprite, num2 + k);
		}
		container.AddChildAtIndex(sLeaser.sprites[nBgSprites], num2 + elements.Count);
		sLeaser.sprites[nBgSprites].isVisible = num < -5f;
		for (int l = 0; l < bgElements.Count; l++)
		{
			Vector3 vector3 = Vector3.Lerp(bgElements[l].lastDrawPos, bgElements[l].drawPos, timeStacker);
			Vector2 vector4 = Vector2.Lerp(bgElements[l].lastDrawScale, bgElements[l].drawScale, timeStacker);
			float rotation2 = Mathf.LerpAngle(bgElements[l].lastRotation, bgElements[l].rotation, timeStacker);
			bgElements[l].sprite.SetPosition(vector3);
			bgElements[l].sprite.scaleX = vector4.x;
			bgElements[l].sprite.scaleY = vector4.y;
			bgElements[l].sprite.rotation = rotation2;
			if (bgElements[l].depthChannel > 0 && bgElements[l].depthChannel < 4)
			{
				bgElements[l].SetDepthToColorChannel(Mathf.Lerp(bgElements[l].lastDepth, bgElements[l].depth, timeStacker));
			}
		}
		if (zoomSphere != null)
		{
			float num3 = Mathf.Lerp(lastSphereOpen, sphereOpen, timeStacker);
			float b = Mathf.Lerp(lastSphereTransition, sphereTransition, timeStacker);
			Color color2 = zoomSphere.sprite.color;
			color2.b = b;
			color2.a = num3;
			zoomSphere.sprite.color = color2;
			color2 = neighbour1.sprite.color;
			color2.a = num3 * 0.5f;
			neighbour1.sprite.color = color2;
			color2 = neighbour2.sprite.color;
			color2.a = num3 * 0.5f;
			neighbour2.sprite.color = color2;
		}
		for (int m = 0; m < links.Count; m++)
		{
			float num4 = Mathf.InverseLerp(3.1f, 3.3f, num + links[m].individualVariation.x * 2f);
			if (num4 >= 1f)
			{
				links[m].sprite.isVisible = false;
			}
			Color color3 = Color.Lerp(linkColor, linkColor2, num4);
			color3.a = num4;
			links[m].sprite.color = color3;
		}
	}

	public static FAtlasElement GetOrLoadFAtlasElement(string name)
	{
		if (Futile.atlasManager.DoesContainElementWithName(name))
		{
			return Futile.atlasManager.GetElementWithName(name);
		}
		string text = AssetManager.ResolveFilePath("illustrations" + Path.DirectorySeparatorChar + name + ".png");
		if (!File.Exists(text))
		{
			return null;
		}
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: true, crispPixels: true);
		Futile.atlasManager.LoadAtlasFromTexture(name, texture2D, textureFromAsset: false);
		return Futile.atlasManager.GetElementWithName(name);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		bgColor.a = 1f;
		Color saturatedGold = RainWorld.SaturatedGold;
		saturatedGold.a = 1f;
		Color rippleGold = RainWorld.RippleGold;
		rippleGold.a = 1f;
		sLeaser.sprites[1].color = bgColor;
		sLeaser.sprites[2].color = saturatedGold;
		sLeaser.sprites[3].color = rippleGold;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < nBgSprites; i++)
		{
			container.AddChild(sLeaser.sprites[i]);
		}
		for (int j = 0; j < bgElements.Count; j++)
		{
			container.AddChild(bgElements[j].sprite);
		}
		for (int k = 0; k < elements.Count; k++)
		{
			container.AddChild(elements[k].sprite);
		}
		container.AddChild(sLeaser.sprites[nBgSprites]);
	}

	public static void SpawnMonolithScene(Room room)
	{
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (room.updateList[i] is MonolithScene)
			{
				return;
			}
		}
		room.AddObject(new MonolithScene(room));
	}
}
