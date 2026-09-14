using RWCustom;
using UnityEngine;

namespace Watcher;

public class WarpSpawningRipple : CosmeticSprite
{
	public class WarpSpawningBubble : CosmeticSprite
	{
		public MaskSource bubble;

		public Vector2 offset;

		public WarpSpawningRipple followTarget;

		public Counter life;

		public Counter burstCounter;

		public Counter spherifyCounter;

		public float time;

		public bool soundPlayed;

		public bool playSound;

		public bool followsWarpCreation;

		public float scale;

		public float distanceFromCenter;

		public float closenessToCenter;

		public float burst;

		public float lastBurst;

		public float lastProgress;

		public float progress;

		public float lastSpherify;

		public float spherify;

		public float rippleState;

		public float secondPhase;

		public float lastSecondPhase;

		public float rotationAcceleration;

		public Vector2 actualPos;

		public Vector2 lastActualPos;

		public Vector2 shake;

		public bool unstable;

		public bool broke;

		public WarpSpawningBubble(Vector2 pos, Vector2 offset, float scale, float rippleState, WarpSpawningRipple followTarget, bool unstable, int life = 50)
		{
			base.pos = pos;
			this.scale = scale;
			this.offset = offset;
			this.followTarget = followTarget;
			this.rippleState = rippleState;
			this.unstable = unstable;
			this.life = new Counter(life, 0, countsUp: true);
			shake = Vector2.zero;
			rotationAcceleration = 0f;
			burstCounter = new Counter(Random.Range(15, 40), 0, countsUp: true);
			spherifyCounter = new Counter(Random.Range(40, 80));
			distanceFromCenter = offset.magnitude / (unstable ? 100f : 200f);
			closenessToCenter = 1f - distanceFromCenter;
			lastSpherify = 1f;
			spherify = 1f;
			time = Random.Range(0f, 2000f);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastProgress = progress;
			lastBurst = burst;
			lastSpherify = spherify;
			lastActualPos = actualPos;
			lastSecondPhase = secondPhase;
			time += 0.01f + progress * 0.03f;
			if (playSound && !soundPlayed)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.RipplesickPop, pos);
				soundPlayed = true;
			}
			if (unstable)
			{
				shake = new Vector2(FlameJet.AnimatePerlin(time) * 4f - 2f, FlameJet.AnimatePerlin(time + 1000f) * 4f - 2f) * (5f * (life.normalized + progress) * 0.5f);
			}
			if (followTarget != null && unstable && !broke)
			{
				WarpSpawningRipple warpSpawningRipple = followTarget;
				if ((warpSpawningRipple == null || !warpSpawningRipple.secondPhase) && life.normalized >= 0.5f && followTarget.bubblesLost < followTarget.maxBubbles / 4 && Random.value > 0.995f)
				{
					broke = true;
					followTarget.bubblesLost++;
				}
			}
			if (burstCounter.isFinished)
			{
				Destroy();
				return;
			}
			life.Tick();
			if ((followTarget == null && progress >= 1f) || broke)
			{
				burstCounter.Tick();
			}
			else
			{
				if (followTarget?.isFinished ?? life.isFinished)
				{
					burstCounter.Tick();
				}
				WarpSpawningRipple warpSpawningRipple2 = followTarget;
				if (warpSpawningRipple2 == null || warpSpawningRipple2.secondPhase)
				{
					spherifyCounter.Tick();
				}
			}
			if (progress >= 1f)
			{
				rotationAcceleration += (0.1f - distanceFromCenter * 0.05f) * (unstable ? (-0.5f) : 1f);
			}
			secondPhase = followTarget?.secondPhaseProgress ?? 0f;
			float num = Mathf.Clamp01(secondPhase * 2f);
			progress = life.normalized;
			burst = burstCounter.normalized;
			spherify = 0.2f + Mathf.Clamp01(spherifyCounter.normalized * 3f - 1f - distanceFromCenter) * 0.8f;
			offset = Custom.rotateVectorDeg(offset, closenessToCenter * 0.1f + rotationAcceleration);
			if (followTarget != null)
			{
				pos = Vector2.Lerp(pos, followTarget.pos, closenessToCenter * 0.1f);
			}
			actualPos = pos + shake + offset * (Custom.LerpQuadEaseInOut(1f, 2f + closenessToCenter, num) * Custom.LerpQuadEaseInOut(0f, 1f, 1f - Mathf.Clamp01(secondPhase * 2.5f - distanceFromCenter) * 0.8f));
			float magnitude = (lastActualPos - pos).magnitude;
			bubble?.SetPos(actualPos);
			bubble?.SetRotation(Mathf.LerpAngle(Custom.VecToDeg(offset), Custom.AimFromOneVectorToAnother(lastActualPos, actualPos), Mathf.Clamp01(secondPhase * 8f)) * Vector3.back);
			bubble?.SetScale(new Vector3(1f - magnitude * 0.001f * num, 1f + magnitude * 0.0025f * num, 1f) * scale);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.maskSources = new MaskSource[1];
			if (bubble == null)
			{
				bubble = MaskMaker.MakeSource("GameplayRippleGrab", "RippleBubbleMask", isSecondLayer: true, null, pos, null, Vector3.zero);
			}
			sLeaser.maskSources[0] = bubble;
			bubble.SetProperty(2, Random.value);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			float value = Mathf.Lerp(lastProgress, progress, timeStacker);
			float max = Mathf.Lerp(lastSpherify, spherify, timeStacker);
			float value2 = Mathf.Lerp(lastBurst, burst, timeStacker);
			float num = Mathf.Lerp(lastSecondPhase, secondPhase, timeStacker);
			bubble.SetProperty(0, Mathf.Clamp(value, 0f, max));
			bubble.SetProperty(1, value2);
			bubble.SetProperty(3, Mathf.Clamp01(rippleState - num));
			bubble.DrawUpdate(timeStacker, rCam, camPos);
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
	}

	public class RippleSpark : Spark
	{
		public float randomOffset;

		public float alpha;

		public float lastAlpha;

		public bool soundPlayed;

		public RippleSpark(Vector2 pos, Vector2 vel, int standardLifeTime, int exceptionalLifeTime)
			: base(pos, vel, Color.white, null, standardLifeTime, exceptionalLifeTime)
		{
			randomOffset = Random.Range(0f, 100f);
		}

		public override void Update(bool eu)
		{
			if (!soundPlayed)
			{
				room.PlaySound(WatcherEnums.WatcherSoundID.RipplesickSpark, pos);
				soundPlayed = true;
			}
			lastAlpha = alpha;
			alpha = Mathf.InverseLerp(0f, Mathf.Min(40, lifeTime), life * (float)lifeTime);
			vel.y += gravity;
			base.Update(eu);
			vel = Custom.rotateVectorDeg(vel, (FlameJet.Noise(life * 0.5f + randomOffset) * 2f - 1f) * 60f);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[2];
			TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[1]
			{
				new TriangleMesh.Triangle(0, 1, 2)
			};
			TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, customColor: false);
			sLeaser.sprites[0] = triangleMesh;
			sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["RippleSpawnBody"];
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = Custom.rainWorld.Shaders["RippleGlow"];
			sLeaser.sprites[1].scale = 3f;
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[1]);
			rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[0]);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			sLeaser.sprites[1].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
			Color color = new Color(1f, 1f, 1f, Mathf.Lerp(lastAlpha, alpha, timeStacker));
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[1].color = color;
		}
	}

	public float progress;

	public float secondPhaseProgress;

	public float secondPhaseSpeed;

	public int bubbles;

	public int maxBubbles = 150;

	public float rippleState;

	public int bubblesLost;

	public float maxRingScale;

	public float ringScale;

	public float lastRingScale;

	public float ringIntensity;

	public float lastRingIntensity;

	public bool unstable;

	public Color color;

	public MaskSource ring;

	public Counter deathCounter = new Counter(40);

	public bool isFinished { get; set; }

	public bool secondPhase { get; set; }

	public WarpSpawningRipple(Room room, Vector2 pos, float rippleState, bool unstable)
	{
		this.unstable = unstable;
		base.room = room;
		base.pos = pos;
		this.rippleState = rippleState;
		maxRingScale = (unstable ? 20 : 40);
		ringScale = maxRingScale;
		lastRingScale = maxRingScale;
		color = (unstable ? Color.magenta : Color.blue);
	}

	public override void Update(bool eu)
	{
		if (!room.BeingViewed || deathCounter.isFinished)
		{
			Destroy();
		}
		if (isFinished)
		{
			deathCounter.Tick();
		}
		SpawnBubbles();
		UpdateSecondPhase();
		UpdateRing();
		base.Update(eu);
	}

	public void SpawnBubbles()
	{
		if (!secondPhase && !isFinished && bubbles < maxBubbles)
		{
			for (int i = 0; i < 3; i++)
			{
				bubbles++;
			}
			float num = (float)bubbles / (float)maxBubbles;
			float value = Random.value;
			Vector2 offset = Custom.RNV() * (10f + (unstable ? 80f : 190f) * (num + value * 0.2f));
			room.AddObject(new WarpSpawningBubble(pos, offset, (3f + value * 10f) * (unstable ? 0.5f : 1f), rippleState, this, unstable));
		}
	}

	public void UpdateSecondPhase()
	{
		if (secondPhase && !isFinished)
		{
			if (secondPhaseProgress >= 1f)
			{
				isFinished = true;
				return;
			}
			secondPhaseProgress = Mathf.Clamp01(secondPhaseProgress + secondPhaseSpeed);
			_ = secondPhaseSpeed;
		}
	}

	public void Success(float duration, bool bad, bool weird, bool strong)
	{
		room.PlaySound(bad ? WatcherEnums.WatcherSoundID.Player_Generated_Success_Bad : WatcherEnums.WatcherSoundID.Player_Generated_Success_Normal, pos, 4f, 1f);
		room.AddObject(new ShockWave(pos, 120f, 0.05f, 10));
		secondPhase = true;
		secondPhaseSpeed = 1f / duration;
	}

	public void Failure()
	{
		isFinished = true;
	}

	public void UpdateRing()
	{
		lastRingScale = ringScale;
		lastRingIntensity = ringIntensity;
		if (ring != null)
		{
			if (!isFinished)
			{
				ringIntensity = Mathf.Clamp01(progress * 2f - 1f);
			}
			else
			{
				ringIntensity -= 0.05f;
			}
			if (secondPhase)
			{
				ringIntensity = 0f;
				ringScale = 0f;
			}
			ringScale = Custom.LerpExpEaseIn(1f, 0f, ringIntensity);
			ring.SetPos(pos);
			ring.SetScale(Vector3.one * (ringScale * maxRingScale));
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["FlatLight"];
		sLeaser.sprites[0].color = color;
		sLeaser.maskSources = new MaskSource[1];
		if (ring == null)
		{
			ring = MaskMaker.MakeSource("GameplayRippleGrab", "RippleRingMask", isSecondLayer: true, null, pos, null, Vector3.one * (ringScale * maxRingScale));
		}
		sLeaser.maskSources[0] = ring;
		ring.SetProperty(0, 0f);
		ring.ApplyVertexColor();
		UpdateRing();
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Bloom"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		ring.SetProperty(0, Mathf.Lerp(lastRingIntensity, ringIntensity, timeStacker));
		ring.SetProperty(2, Mathf.Lerp(lastRingScale, ringScale, timeStacker));
		ring.DrawUpdate(timeStacker, rCam, camPos);
		sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
		sLeaser.sprites[0].scale = maxRingScale * 2f;
		Color color = sLeaser.sprites[0].color;
		color.a = ringIntensity * 0.1f;
		sLeaser.sprites[0].color = color;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
