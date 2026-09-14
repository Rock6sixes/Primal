using System;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class FlameJet : UpdatableAndDeletable, IDrawable
{
	public class Animation : ExtEnum<Animation>
	{
		public static Animation None = new Animation("None", register: true);

		public static Animation Sine = new Animation("Sine", register: true);

		public static Animation Perlin = new Animation("Perlin", register: true);

		public Animation(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class ActiveDuring : ExtEnum<ActiveDuring>
	{
		public static ActiveDuring Always = new ActiveDuring("Always", register: true);

		public static ActiveDuring DuringCycle = new ActiveDuring("DuringCycle", register: true);

		public static ActiveDuring EndOfCycle = new ActiveDuring("EndOfCycle", register: true);

		public ActiveDuring(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class FlameJetData : PlacedObject.Data
	{
		public FlameJet obj;

		public Vector2 panelPos = Vector2.one * 100f;

		public Vector2 pos;

		public Vector2 target = Vector2.up * 100f;

		public float intensity = 1f;

		public float temperature;

		public float intensityAnimSpeed = 0.5f;

		public float intensityAnimOffset;

		public float temperatureAnimSpeed = 0.5f;

		public float temperatureAnimOffset;

		public Animation intensityAnim = Animation.None;

		public Animation temperatureAnim = Animation.None;

		public ActiveDuring activeDuring = ActiveDuring.Always;

		public float intensityMin;

		public float intensityMax = 1f;

		public float temperatureMin;

		public float temperatureMax = 1f;

		public bool linkTempToIntens;

		public int lethality = 2;

		public float width = 0.25f;

		public float fireVolumeMax = 0.75f;

		public float smokeVolumeMax = 0.75f;

		public FlameJetData(PlacedObject owner)
			: base(owner)
		{
		}

		public override void FromString(string s)
		{
			string[] array = Regex.Split(s, "~");
			int num = -1;
			panelPos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : panelPos.x);
			panelPos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : panelPos.y);
			pos.x = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.x);
			pos.y = ((array.Length > ++num) ? ParseFloat(array[num]) : pos.y);
			target.x = ((array.Length > ++num) ? ParseFloat(array[num]) : target.x);
			target.y = ((array.Length > ++num) ? ParseFloat(array[num]) : target.y);
			intensity = ((array.Length > ++num) ? ParseFloat(array[num]) : intensity);
			temperature = ((array.Length > ++num) ? ParseFloat(array[num]) : temperature);
			intensityAnimSpeed = ((array.Length > ++num) ? ParseFloat(array[num]) : intensityAnimSpeed);
			intensityAnimOffset = ((array.Length > ++num) ? ParseFloat(array[num]) : intensityAnimOffset);
			temperatureAnimSpeed = ((array.Length > ++num) ? ParseFloat(array[num]) : temperatureAnimSpeed);
			temperatureAnimOffset = ((array.Length > ++num) ? ParseFloat(array[num]) : temperatureAnimOffset);
			intensityAnim = ((array.Length > ++num) ? new Animation(array[num]) : intensityAnim);
			temperatureAnim = ((array.Length > ++num) ? new Animation(array[num]) : temperatureAnim);
			activeDuring = ((array.Length > ++num) ? new ActiveDuring(array[num]) : activeDuring);
			intensityMin = ((array.Length > ++num) ? ParseFloat(array[num]) : intensityMin);
			intensityMax = ((array.Length > ++num) ? ParseFloat(array[num]) : intensityMax);
			temperatureMin = ((array.Length > ++num) ? ParseFloat(array[num]) : temperatureMin);
			temperatureMax = ((array.Length > ++num) ? ParseFloat(array[num]) : temperatureMax);
			linkTempToIntens = ((array.Length > ++num) ? bool.Parse(array[num]) : linkTempToIntens);
			width = ((array.Length > ++num) ? ParseFloat(array[num]) : width);
			lethality = ((array.Length > ++num) ? ParseInt(array[num]) : lethality);
			fireVolumeMax = ((array.Length > ++num) ? ParseFloat(array[num]) : fireVolumeMax);
			smokeVolumeMax = ((array.Length > ++num) ? ParseFloat(array[num]) : smokeVolumeMax);
		}

		public float ParseFloat(string s)
		{
			return float.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public int ParseInt(string s)
		{
			return int.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}~{16}~{17}~{18}~{19}~{20}~{21}~{22}~{23}", panelPos.x, panelPos.y, pos.x, pos.y, target.x, target.y, intensity, temperature, intensityAnimSpeed, intensityAnimOffset, temperatureAnimSpeed, temperatureAnimOffset, intensityAnim.value, temperatureAnim.value, activeDuring.value, intensityMin, intensityMax, temperatureMin, temperatureMax, linkTempToIntens, width, lethality, fireVolumeMax, smokeVolumeMax);
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "~", unrecognizedAttributes);
		}
	}

	public Vector2 pos;

	public Vector2 lastPos;

	public Vector2 setPos;

	public Vector2 target;

	public Vector2 lastTarget;

	public Vector2 setTarget;

	public float intensity;

	public float temperature;

	public float[] finalIntensity;

	public float[] finalTemperature;

	public float intensityAnimSpeed;

	public float intensityAnimOffset;

	public float temperatureAnimSpeed;

	public float temperatureAnimOffset;

	public Animation intensityAnim;

	public Animation temperatureAnim;

	public ActiveDuring activeDuring;

	public float cycleFade;

	public float _intensityMin;

	public float _intensityMax;

	public float _temperatureMin;

	public float _temperatureMax;

	public float time;

	public bool linkTempToIntens;

	public int nJetPos = 12;

	public Vector2[] jetPos;

	public Vector2[] lastJetPos;

	public Vector2[] jetPerp;

	public Vector2[] lastJetPerp;

	public float[] jetScale;

	public float[] lastJetScale;

	public float _width;

	public float lowPitch;

	public float highPitch;

	public float fireVolumeMax;

	public float smokeVolumeMax;

	public Vector2[] lightPos;

	public float individualOffset;

	public int lethality;

	public float downwards;

	public FloatRect bounds;

	public StaticSoundLoop soundLoop;

	public SoundID lastSoundID = WatcherEnums.WatcherSoundID.Flame_Jet_Fire_LOOP;

	public bool debug;

	public bool debugInit;

	public float rippleMult;

	public float lastRippleMult;

	public DebugSprite[] jetPosDebug;

	public DebugSprite[] jetNormalDebug;

	public DebugSprite upMixDebug;

	public DebugSprite breakPoingDebug;

	public DebugSprite towardsDebug;

	public DebugSprite boundsDebugA;

	public DebugSprite boundsDebugB;

	public DebugSprite boundsDebugC;

	public DebugSprite boundsDebugD;

	public float intensityMin
	{
		get
		{
			return _intensityMin;
		}
		set
		{
			_intensityMin = Mathf.Clamp(value, 0f, _intensityMax);
		}
	}

	public float intensityMax
	{
		get
		{
			return _intensityMax;
		}
		set
		{
			_intensityMax = Mathf.Clamp(value, _intensityMin, 1f);
		}
	}

	public float temperatureMin
	{
		get
		{
			return _temperatureMin;
		}
		set
		{
			_temperatureMin = Mathf.Clamp(value, 0f, _temperatureMax);
		}
	}

	public float temperatureMax
	{
		get
		{
			return _temperatureMax;
		}
		set
		{
			_temperatureMax = Mathf.Clamp(value, _temperatureMin, 1f);
		}
	}

	public float width
	{
		get
		{
			return _width * 4f;
		}
		set
		{
			_width = value;
		}
	}

	public bool hiddenByRipple => room.game.ActiveRippleLayer != 0;

	public FlameJet(Room room, FlameJetData data)
	{
		base.room = room;
		lastPos = (setPos = (pos = data.pos));
		lastTarget = (setTarget = (target = data.target));
		intensity = data.intensity;
		temperature = data.temperature;
		intensityAnimSpeed = data.intensityAnimSpeed;
		intensityAnimOffset = data.intensityAnimOffset;
		temperatureAnimSpeed = data.temperatureAnimSpeed;
		temperatureAnimOffset = data.temperatureAnimOffset;
		intensityAnim = data.intensityAnim;
		temperatureAnim = data.temperatureAnim;
		activeDuring = data.activeDuring;
		intensityMin = data.intensityMin;
		intensityMax = data.intensityMax;
		temperatureMin = data.temperatureMin;
		temperatureMax = data.temperatureMax;
		linkTempToIntens = data.linkTempToIntens;
		lethality = data.lethality;
		width = data.width;
		fireVolumeMax = data.fireVolumeMax;
		smokeVolumeMax = data.smokeVolumeMax;
		individualOffset = UnityEngine.Random.value;
		jetPos = new Vector2[nJetPos];
		lastJetPos = new Vector2[nJetPos];
		jetPerp = new Vector2[nJetPos];
		lastJetPerp = new Vector2[nJetPos];
		jetScale = new float[nJetPos];
		lastJetScale = new float[nJetPos];
		finalIntensity = new float[nJetPos];
		finalTemperature = new float[nJetPos];
		lightPos = new Vector2[2];
		lowPitch = UnityEngine.Random.Range(0.4f, 0.6f);
		highPitch = UnityEngine.Random.Range(0.8f, 1f);
		rippleMult = 1f;
		lastRippleMult = 1f;
		bounds = default(FloatRect);
		if (debug)
		{
			jetPosDebug = new DebugSprite[nJetPos];
			jetNormalDebug = new DebugSprite[nJetPos];
		}
	}

	public static FlameJet FromPlacedObject(Room room, PlacedObject pObj)
	{
		return new FlameJet(room, pObj.data as FlameJetData);
	}

	public override void Update(bool eu)
	{
		UpdateCycleFade();
		UpdateJetPos();
		UpdateJetPerpendiculars();
		UpdateJetScales();
		UpdateIntensity();
		UpdateTemperature();
		UpdateDamage();
		lastRippleMult = rippleMult;
		rippleMult = Mathf.Lerp(rippleMult, (!hiddenByRipple) ? 1 : 0, 0.01f);
		downwards = Mathf.InverseLerp(0.5f, 0.8f, Vector2.Dot(target.normalized, Vector2.down)) * intensity;
		int num = nJetPos / 2;
		SoundID soundID = WatcherEnums.WatcherSoundID.Flame_Jet_Fire_LOOP;
		if (finalTemperature[num] < 0.5f)
		{
			soundID = WatcherEnums.WatcherSoundID.Flame_Jet_Smoke_LOOP;
		}
		if (soundLoop == null || soundID != lastSoundID)
		{
			if (soundLoop != null)
			{
				soundLoop.volume = 0f;
				if (soundLoop.emitter != null)
				{
					soundLoop.emitter.Destroy();
				}
			}
			lastSoundID = soundID;
			soundLoop = new StaticSoundLoop(soundID, setPos, room, 0f, 1f)
			{
				fadeOutOnDestroyFrames = 30
			};
		}
		if (soundID == WatcherEnums.WatcherSoundID.Flame_Jet_Smoke_LOOP)
		{
			soundLoop.volume = Mathf.Lerp(soundLoop.volume, Mathf.Lerp(0.01f, 1f * smokeVolumeMax, finalIntensity[num]), 0.1f) * rippleMult;
			soundLoop.pitch = Mathf.Lerp(soundLoop.pitch, Mathf.Lerp(lowPitch, lowPitch + highPitch * 0.1f, finalIntensity[num]), 0.1f);
		}
		else
		{
			soundLoop.volume = Mathf.Lerp(soundLoop.volume, Mathf.Lerp(0.01f, 1f * fireVolumeMax, finalIntensity[num]), 0.1f) * rippleMult;
			soundLoop.pitch = Mathf.Lerp(lowPitch, highPitch, finalIntensity[num]);
		}
		soundLoop.pos = setPos;
		soundLoop.Update();
		InitDebug();
		time += Time.fixedDeltaTime * 6f;
		lastPos = pos;
		lastTarget = target;
		pos = setPos;
		target = setTarget * cycleFade;
		base.Update(eu);
	}

	public void UpdateDamage()
	{
		if (lethality < 1)
		{
			return;
		}
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			foreach (PhysicalObject item in room.physicalObjects[i])
			{
				if (!(item is Creature creature) || creature.enteringShortCut.HasValue || creature.abstractPhysicalObject.rippleLayer != 0)
				{
					continue;
				}
				Vector2 vector = creature.mainBodyChunk.pos - pos;
				if (!bounds.Vector2Inside(vector))
				{
					continue;
				}
				for (int j = 0; j < jetPos.Length - 1; j++)
				{
					float t = (float)(Time.frameCount % 10) * 0.1f;
					Vector2 b = Vector2.Lerp(jetPos[j], jetPos[j + 1], t);
					float num = Mathf.Lerp(jetScale[j], jetScale[j + 1], t);
					float num2 = Mathf.Clamp((float)j / 3f, 0.5f, 1f);
					num2 *= Mathf.Clamp((float)(jetPos.Length - 1 - j) / 5f, 0.5f, 1f);
					float num3 = Vector2.Distance(vector, b);
					if (ModManager.HypothermiaModule)
					{
						creature.Hypothermia = Mathf.Lerp(creature.Hypothermia, 0f, 0.001f * finalTemperature[j]);
					}
					if (creature is Player player && num3 < num * 3f)
					{
						if (num3 < num * 2f)
						{
							player.lungsExhausted = true;
						}
						if (num3 < num * 1.5f)
						{
							player.eyesClosedTime++;
						}
					}
					float num4 = num * 0.5f * num2;
					if (!(num3 > num4))
					{
						float num5 = 1f - num3 / num4;
						creature.mainBodyChunk.vel += target.normalized * (finalIntensity[j] * num5);
						if (lethality >= 2 && !(finalTemperature[j + 1] < 0.5f) && !creature.dead)
						{
							creature.Violence(null, null, creature.mainBodyChunk, null, Creature.DamageType.Explosion, Custom.LerpMap(finalTemperature[j + 1], 0.5f, 1f, 0f, (1f + finalIntensity[j] * 10f) * num5), 10f);
							break;
						}
					}
				}
			}
		}
	}

	public void UpdateCycleFade()
	{
		if (activeDuring == ActiveDuring.Always)
		{
			cycleFade = 1f;
			return;
		}
		cycleFade = Mathf.Clamp(room.world.rainCycle.RainApproaching * 2f, 0.001f, 0.999f);
		if (!(activeDuring == ActiveDuring.DuringCycle) && activeDuring == ActiveDuring.EndOfCycle)
		{
			cycleFade = 1f - cycleFade;
		}
	}

	public void UpdateJetPos()
	{
		Vector2 vector = target * 1.5f;
		lightPos[1] = lightPos[0];
		lightPos[0] = new Vector2(AnimatePerlin((time + 11f) * 0.2f) * 2f - 1f, AnimatePerlin((time + 27f) * 0.2f) * 2f - 1f) * 10f;
		bounds.left = 0f;
		bounds.right = 0f;
		bounds.top = 0f;
		bounds.bottom = 0f;
		for (int i = 0; i < nJetPos; i++)
		{
			Vector2 vector2 = Vector2.up * (vector.magnitude * (1f - finalIntensity[i] * 0.9f) * (0.3f + finalIntensity[i] * 0.7f));
			Vector2 vector3 = vector * (finalIntensity[i] * finalIntensity[i]);
			Vector2 vector4 = vector3 * (2.1f - finalIntensity[i]);
			vector3 *= 1f - finalIntensity[i];
			Vector2 vector5 = vector4 * 0.8f + vector2;
			Mathf.Clamp((finalIntensity[i] - finalIntensity[Mathf.Min(i + 1, nJetPos - 11)]) * vector.magnitude * 0.5f, 0.5f, 1f);
			lastJetPos[i] = jetPos[i];
			float perc = (float)i / (float)nJetPos;
			perc = Custom.LerpQuadEaseOut(0f, 1f, perc);
			float perc2 = Mathf.Clamp01(Mathf.InverseLerp(vector3.magnitude / vector.magnitude + 0.001f, 1.001f, perc));
			Vector2 vector6 = Vector2.Lerp(vector4, vector5, Custom.LerpQuadEaseIn(0f, 1f, perc2) * (1f - downwards));
			jetPos[i] = vector6 * perc;
			jetPos[i] = Vector2.Lerp(jetPos[i], lastJetPos[i], Custom.LerpQuadEaseOut(0f, 1f, perc2) * (0.9f + (0.5f - Mathf.Abs(finalIntensity[i] - 0.5f)) * 0.05f) * (1f - downwards));
			AddPointToBounds(jetPos[i], jetScale[i] * 2f);
			if (debug && debugInit && i == 0)
			{
				upMixDebug.pos = pos + vector5;
				towardsDebug.pos = pos + vector4;
				breakPoingDebug.pos = pos + vector3;
			}
		}
		if (debug && debugInit)
		{
			boundsDebugA.pos = pos + bounds.GetCorner(0);
			boundsDebugB.pos = pos + bounds.GetCorner(1);
			boundsDebugC.pos = pos + bounds.GetCorner(2);
			boundsDebugD.pos = pos + bounds.GetCorner(3);
		}
	}

	public void AddPointToBounds(Vector2 p, float size)
	{
		if (p.x - size < bounds.left)
		{
			bounds.left = p.x - size;
		}
		if (p.x + size > bounds.right)
		{
			bounds.right = p.x + size;
		}
		if (p.y - size < bounds.bottom)
		{
			bounds.bottom = p.y - size;
		}
		if (p.y + size > bounds.top)
		{
			bounds.top = p.y + size;
		}
	}

	public void UpdateJetPerpendiculars()
	{
		for (int i = 0; i < nJetPos; i++)
		{
			lastJetPerp[i] = jetPerp[i];
			jetPerp[i] = GetPerpendicular(jetPos, i);
		}
	}

	public void UpdateJetScales()
	{
		for (int i = 0; i < nJetPos; i++)
		{
			lastJetScale[i] = jetScale[i];
			jetScale[i] = Mathf.Min(GetIntersectionRadius(jetPos, jetPerp, i) * 0.9f, 10f + 20f * ((float)i / (float)(nJetPos - 1)) * finalTemperature[i] + (10f + 10f * finalTemperature[i]) * finalIntensity[i]) * width;
			jetScale[i] *= cycleFade;
		}
		for (int j = 1; j < nJetPos - 1; j++)
		{
			lastJetScale[j] = jetScale[j];
			jetScale[j] += jetScale[j - 1] + jetScale[j + 1];
			jetScale[j] *= 0.33333f;
		}
		for (int k = 2; k < nJetPos - 2; k++)
		{
			lastJetScale[k] = jetScale[k];
			jetScale[k] += jetScale[k - 1] + jetScale[k + 1] + jetScale[k - 2] + jetScale[k + 2];
			jetScale[k] *= 0.2f;
		}
	}

	public void UpdateIntensity()
	{
		if (intensityAnim == Animation.None)
		{
			for (int num = nJetPos - 2; num > -1; num--)
			{
				finalIntensity[num + 1] = finalIntensity[num];
			}
			finalIntensity[0] = intensity * cycleFade;
			return;
		}
		for (int num2 = nJetPos - 2; num2 > -1; num2--)
		{
			finalIntensity[num2 + 1] = Mathf.Lerp(finalIntensity[num2 + 1], finalIntensity[num2], 0.5f);
		}
		float num3 = time * intensityAnimSpeed + intensityAnimOffset;
		num3 = Animate(intensityAnim, num3);
		finalIntensity[0] = Mathf.Lerp(intensityMin, intensityMax, num3) * intensity * cycleFade;
	}

	public void UpdateTemperature()
	{
		if (linkTempToIntens)
		{
			for (int i = 0; i < nJetPos; i++)
			{
				finalTemperature[i] = finalIntensity[i];
			}
			return;
		}
		if (temperatureAnim == Animation.None)
		{
			for (int num = nJetPos - 2; num > -1; num--)
			{
				finalTemperature[num + 1] = finalTemperature[num];
			}
			finalTemperature[0] = temperature * cycleFade;
			return;
		}
		for (int num2 = nJetPos - 2; num2 > -1; num2--)
		{
			finalTemperature[num2 + 1] = Mathf.Lerp(finalTemperature[num2 + 1], finalTemperature[num2], 0.5f);
		}
		float num3 = time * temperatureAnimSpeed + temperatureAnimOffset;
		num3 = Animate(temperatureAnim, num3);
		finalTemperature[0] = Mathf.Lerp(temperatureMin, temperatureMax, num3) * temperature * cycleFade;
	}

	public float Animate(Animation type, float time)
	{
		if (type == Animation.Sine)
		{
			return AnimateSine(time);
		}
		if (type == Animation.Perlin)
		{
			return AnimatePerlin(time);
		}
		return time;
	}

	public float AnimateSine(float time)
	{
		return Mathf.Sin(time) * 0.5f + 0.5f;
	}

	public static float AnimatePerlin(float time)
	{
		float num = time * 2.3f;
		float num2 = Custom.Mod(num, 1f);
		float num3 = num - num2;
		num2 = SinSmooth(num2);
		float num4 = Mathf.Lerp(Noise(num3), Noise(num3 + 1f), num2);
		float num5 = time * 5.3f;
		num2 = Custom.Mod(num5, 1f);
		num3 = num5 - num2;
		return (num4 + Mathf.Lerp(t: SinSmooth(num2), a: Noise(num3), b: Noise(num3 + 1f))) * 0.5f;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[6];
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].shader = Custom.rainWorld.Shaders["LightSource"];
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = Custom.rainWorld.Shaders["LightSource"];
		sLeaser.sprites[3] = new FSprite("Futile_White");
		sLeaser.sprites[3].shader = Custom.rainWorld.Shaders["LightSource"];
		sLeaser.sprites[5] = new FSprite("Futile_White");
		sLeaser.sprites[5].shader = Custom.rainWorld.Shaders["HeatDistortion"];
		sLeaser.sprites[5].anchorY = 0.2f;
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[nJetPos * 4];
		for (int i = 0; i < nJetPos - 1; i++)
		{
			int num = i * 3;
			int num2 = i * 4;
			array[num2] = new TriangleMesh.Triangle(num, num + 1, num + 3);
			array[num2 + 1] = new TriangleMesh.Triangle(num + 3, num + 1, num + 4);
			array[num2 + 2] = new TriangleMesh.Triangle(num + 1, num + 2, num + 4);
			array[num2 + 3] = new TriangleMesh.Triangle(num + 4, num + 2, num + 5);
		}
		TriangleMesh triangleMesh = new TriangleMesh("Futile_White", array, customColor: true);
		sLeaser.sprites[0] = triangleMesh;
		sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["FlameJet"];
		TriangleMesh triangleMesh2 = new TriangleMesh("Futile_White", array, customColor: true);
		sLeaser.sprites[4] = triangleMesh2;
		sLeaser.sprites[4].shader = Custom.rainWorld.Shaders["FlameJetGlow"];
		for (int j = 0; j < nJetPos; j++)
		{
			int num3 = j * 3;
			triangleMesh.UVvertices[num3] = Vector2.Lerp(Vector2.zero, Vector2.right, (float)j / (float)(nJetPos - 1));
			triangleMesh.UVvertices[num3 + 1] = new Vector2(triangleMesh.UVvertices[num3].x, 0.5f);
			triangleMesh.UVvertices[num3 + 2] = new Vector2(triangleMesh.UVvertices[num3].x, 1f);
			triangleMesh2.UVvertices[num3] = triangleMesh.UVvertices[num3];
			triangleMesh2.UVvertices[num3 + 1] = triangleMesh.UVvertices[num3 + 1];
			triangleMesh2.UVvertices[num3 + 2] = triangleMesh.UVvertices[num3 + 2];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
		int num = nJetPos / 2;
		sLeaser.sprites[0].SetPosition(vector);
		sLeaser.sprites[4].SetPosition(vector);
		Vector2 vector2 = Vector2.Lerp(lightPos[1], lightPos[0], timeStacker) * target.magnitude / 500f;
		Vector2 position = vector + Vector2.Lerp(lastJetPos[num], jetPos[num], timeStacker) + vector2 * (Mathf.Lerp(lastJetScale[num], jetScale[num], timeStacker) * 0.1f);
		Color color = new Color(0.82f, 0.365f, 0.106f, Mathf.Min(finalTemperature[num], Mathf.Max(0f, finalTemperature[num] - 0.3f) * 5f));
		color *= Mathf.Lerp(lastRippleMult, rippleMult, timeStacker);
		float num2 = target.magnitude * 0.12f * (0.5f + finalIntensity[num] * 0.5f);
		sLeaser.sprites[1].SetPosition(position);
		sLeaser.sprites[1].color = color;
		sLeaser.sprites[1].scale = num2;
		sLeaser.sprites[2].SetPosition(position);
		sLeaser.sprites[2].color = color;
		sLeaser.sprites[2].scale = num2;
		sLeaser.sprites[3].SetPosition(position);
		sLeaser.sprites[3].color = new Color(1f, 0.906f, 0.631f, color.a);
		sLeaser.sprites[3].scale = num2 * 0.5f;
		sLeaser.sprites[5].SetPosition(vector + Vector2.Lerp(lastJetPos[num], jetPos[num], timeStacker));
		sLeaser.sprites[5].color = new Color(0f, 0f, 0f, color.a * 0.5f);
		sLeaser.sprites[5].scale = num2 * 0.5f;
		TriangleMesh triangleMesh = (TriangleMesh)sLeaser.sprites[0];
		TriangleMesh triangleMesh2 = (TriangleMesh)sLeaser.sprites[4];
		for (int i = 0; i < nJetPos; i++)
		{
			int num3 = i * 3;
			Vector2 vector3 = Vector2.Lerp(lastJetPerp[i] * lastJetScale[i], jetPerp[i] * jetScale[i], timeStacker);
			Vector2 vector4 = Vector2.Lerp(lastJetPos[i], jetPos[i], timeStacker);
			triangleMesh.MoveVertice(num3, vector4 + vector3);
			triangleMesh.MoveVertice(num3 + 1, vector4);
			triangleMesh.MoveVertice(num3 + 2, vector4 - vector3);
			triangleMesh2.MoveVertice(num3, vector4 + vector3 * 1.4f);
			triangleMesh2.MoveVertice(num3 + 1, vector4);
			triangleMesh2.MoveVertice(num3 + 2, vector4 - vector3 * 1.4f);
			Color color2 = new Color(finalIntensity[i], finalTemperature[i], individualOffset, 1f);
			triangleMesh.verticeColors[num3] = color2;
			triangleMesh.verticeColors[num3 + 1] = color2;
			triangleMesh.verticeColors[num3 + 2] = color2;
			triangleMesh2.verticeColors[num3] = triangleMesh.verticeColors[num3];
			triangleMesh2.verticeColors[num3 + 1] = triangleMesh.verticeColors[num3];
			triangleMesh2.verticeColors[num3 + 2] = triangleMesh.verticeColors[num3];
		}
		for (int j = 0; j < nJetPos; j++)
		{
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
		if (debug && debugInit)
		{
			for (int k = 0; k < nJetPos; k++)
			{
				Vector2 position2 = Vector2.Lerp(lastPos + lastJetPos[k], pos + jetPos[k], timeStacker) - camPos;
				jetPosDebug[k].sprite.SetPosition(position2);
				jetNormalDebug[k].sprite.SetPosition(position2);
				jetNormalDebug[k].sprite.rotation = Custom.VecToDeg(jetPerp[k]);
				jetNormalDebug[k].sprite.scaleY = jetScale[k];
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[0]);
		FContainer fContainer = rCam.ReturnFContainer("ForegroundLights");
		fContainer.AddChild(sLeaser.sprites[1]);
		fContainer.AddChild(sLeaser.sprites[2]);
		fContainer.AddChild(sLeaser.sprites[3]);
		rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[4]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[5]);
	}

	public Vector2 GetPerpendicular(Vector2[] points, int index)
	{
		int num = Mathf.Clamp(index - 1, 0, points.Length - 1);
		index = Mathf.Clamp(index, 0, points.Length - 1);
		int num2 = Mathf.Clamp(index + 1, 0, points.Length - 1);
		return GetPerpendicular(points[num], points[index], points[num2]);
	}

	public float GetIntersectionRadius(Vector2[] points, Vector2[] perp, int index)
	{
		int num = Mathf.Clamp(index - 1, 0, points.Length - 1);
		index = Mathf.Clamp(index, 0, points.Length - 1);
		int num2 = Mathf.Clamp(index + 1, 0, points.Length - 1);
		Vector2 vector = points[num];
		Vector2 vector2 = points[index];
		Vector2 vector3 = points[num2];
		Vector2 b = vector + perp[num] * 20f;
		Vector2 vector4 = vector2 + perp[index] * 20f;
		Vector2 b2 = vector3 + perp[num2] * 20f;
		return Mathf.Min((Custom.LineIntersection(vector, b, vector2, vector4) - vector).magnitude, (Custom.LineIntersection(vector2, vector4, vector3, b2) - vector).magnitude);
	}

	public Vector2 GetPerpendicular(Vector2 a, Vector2 o, Vector2 b)
	{
		if (a == o && o == b)
		{
			return default(Vector2);
		}
		if (a == o)
		{
			a -= b - o;
		}
		if (b == o)
		{
			b -= a - o;
		}
		Vector2 normalized = (o - a).normalized;
		Vector2 normalized2 = (b - o).normalized;
		return Custom.PerpendicularVector(Vector2.Lerp(normalized, normalized2, LeanTowardLarger(a - o, b - o)).normalized);
	}

	public float LeanTowardLarger(Vector2 oa, Vector2 ob)
	{
		float magnitude = oa.magnitude;
		float magnitude2 = ob.magnitude;
		float num = magnitude - magnitude2;
		float num2 = Mathf.Sign(num);
		return 0.5f - num / (Mathf.Max(0f, (0f - num2) * magnitude2) + Mathf.Max(0f, num2 * magnitude)) * 0.5f;
	}

	public static float SinSmooth(float value)
	{
		value = Mathf.Clamp01(value);
		return Mathf.Sin((value - 0.5f) * (float)Math.PI) * 0.5f + 0.5f;
	}

	public static float Noise(float time)
	{
		return (Mathf.Sin(time * 199934.12f) + Mathf.Sin(time * 179994.12f)) * 0.25f + 0.5f;
	}

	public void InitDebug()
	{
		if (debug && !debugInit)
		{
			for (int i = 0; i < nJetPos; i++)
			{
				jetPosDebug[i] = new DebugSprite(default(Vector2), new FSprite("Futile_White")
				{
					color = Color.Lerp(Color.blue, Color.magenta, (float)i / (float)(nJetPos - 1)),
					scale = 0.5f
				}, room);
				room.AddObject(jetPosDebug[i]);
				jetNormalDebug[i] = new DebugSprite(default(Vector2), new FSprite("pixel")
				{
					color = Color.white,
					scaleY = 50f,
					scaleX = 2f
				}, room);
				room.AddObject(jetNormalDebug[i]);
			}
			upMixDebug = new DebugSprite(default(Vector2), new FSprite("Futile_White")
			{
				scale = 0.5f,
				color = Color.green,
				alpha = 0.5f
			}, room);
			room.AddObject(upMixDebug);
			breakPoingDebug = new DebugSprite(default(Vector2), new FSprite("Futile_White")
			{
				scale = 0.5f,
				color = Color.red,
				alpha = 0.5f
			}, room);
			room.AddObject(breakPoingDebug);
			towardsDebug = new DebugSprite(default(Vector2), new FSprite("Futile_White")
			{
				scale = 0.5f,
				color = Color.green,
				alpha = 0.5f
			}, room);
			room.AddObject(towardsDebug);
			boundsDebugA = new DebugSprite(pos, new FSprite("Futile_White")
			{
				scale = 1f,
				color = Color.magenta,
				rotation = 45f
			}, room);
			boundsDebugB = new DebugSprite(pos, new FSprite("Futile_White")
			{
				scale = 1f,
				color = Color.magenta,
				rotation = 45f
			}, room);
			boundsDebugC = new DebugSprite(pos, new FSprite("Futile_White")
			{
				scale = 1f,
				color = Color.magenta,
				rotation = 45f
			}, room);
			boundsDebugD = new DebugSprite(pos, new FSprite("Futile_White")
			{
				scale = 1f,
				color = Color.magenta,
				rotation = 45f
			}, room);
			room.AddObject(boundsDebugA);
			room.AddObject(boundsDebugB);
			room.AddObject(boundsDebugC);
			room.AddObject(boundsDebugD);
			debugInit = true;
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}
}
