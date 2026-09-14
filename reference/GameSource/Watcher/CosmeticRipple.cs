using System;
using System.Globalization;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class CosmeticRipple : UpdatableAndDeletable, IDrawable, INotifyWhenRoomUnloaded
{
	public float lastScale = 80f;

	public float scale = 80f;

	public float displayScale;

	public Vector2 lastPos = Vector2.zero;

	public Vector2 pos = Vector2.zero;

	public string shader;

	public MaskSource ripple;

	public CosmeticRippleData data;

	public PlacedObject placedObject;

	public int flags;

	public float squish;

	public float intensity;

	public float displayIntensity;

	public float fallOff;

	public float _lastFallOff;

	public float _lastDisplayIntensity;

	public float _lastDepthMix;

	public float depthMix;

	public bool isGameplay;

	public bool isTransition;

	public float rotation;

	public int interpolateDelay = 9;

	public string maskShader = "ShiftMask";

	public Player player;

	public float transitionOpenness;

	public float transitionShape;

	public float lastTransitionOpenness;

	public float lastTransitionShape;

	public CosmeticRippleData Data => placedObject.data as CosmeticRippleData;

	public bool isInverted
	{
		get
		{
			return (flags & 1) > 0;
		}
		set
		{
			if (value)
			{
				flags |= 1;
			}
			else
			{
				flags &= -2;
			}
			Refresh();
		}
	}

	public bool square
	{
		get
		{
			return (flags & 2) > 0;
		}
		set
		{
			if (value)
			{
				flags |= 2;
			}
			else
			{
				flags &= -3;
			}
		}
	}

	public bool leavesTrail
	{
		get
		{
			return (flags & 4) > 0;
		}
		set
		{
			if (value)
			{
				flags |= 4;
			}
			else
			{
				flags &= -5;
			}
		}
	}

	public bool persistentTear { get; set; }

	public CosmeticRipple(PlacedObject pObj, string maskShaderOverride = null)
	{
		data = pObj.data as CosmeticRippleData;
		placedObject = pObj;
		pos = data.pos;
		scale = data.scale;
		intensity = data.intensity;
		fallOff = data.fallOff;
		isInverted = data.invert;
		depthMix = data.depthMix;
		square = data.square;
		squish = data.squish;
		leavesTrail = data.leavesTrail;
		isGameplay = data.isGameplay;
		isTransition = data.isTransition;
		maskShader = maskShaderOverride ?? maskShader;
	}

	public static void ReplaceBasicShader(FSprite[] sprites)
	{
		if (sprites == null || sprites.Length == 0)
		{
			return;
		}
		FShader fShader = Custom.rainWorld.Shaders["RippleBasic"];
		int index = Custom.rainWorld.Shaders["Basic"].index;
		foreach (FSprite fSprite in sprites)
		{
			if (fSprite != null && fSprite.shader.index == index)
			{
				fSprite.shader = fShader;
			}
		}
	}

	public void Refresh()
	{
		if (ripple != null)
		{
			ripple.DeleteMe();
			ripple = null;
		}
	}

	public void RemoveObject()
	{
		Refresh();
	}

	public void RoomUnloaded()
	{
		RemoveObject();
	}

	public void setProps(float timeStacker)
	{
		ripple.SetProperty(0, Mathf.Lerp(_lastDisplayIntensity, displayIntensity, timeStacker));
		ripple.SetProperty(1, (float)flags / 255f);
		ripple.SetProperty(2, Mathf.Lerp(_lastFallOff, fallOff, timeStacker));
		ripple.SetProperty(3, Mathf.Lerp(_lastDepthMix, depthMix, timeStacker));
	}

	public static string IdentifyingString(CosmeticRippleData data, AbstractRoom sourceRoom)
	{
		return sourceRoom.name + ":" + data.pos.x.ToString(CultureInfo.InvariantCulture) + ":" + data.pos.y.ToString(CultureInfo.InvariantCulture);
	}

	public static string RoomFromIdentifyingString(string identifyingString)
	{
		return identifyingString.Split(':')[0];
	}

	public string MyIdentifyingString()
	{
		return IdentifyingString(Data, room.abstractRoom);
	}

	public void UpdateRotation()
	{
		if (ripple != null)
		{
			rotation = Custom.AimFromOneVectorToAnother(Vector2.zero, data.handlePos);
		}
	}

	public void RippleUpdate()
	{
		_lastFallOff = fallOff;
		_lastDepthMix = depthMix;
		_lastDisplayIntensity = displayIntensity;
		if (room.BeingViewed)
		{
			MaskSource maskSource = ripple;
			if (maskSource == null || maskSource.beingDeleted)
			{
				RippleCameraData rippleData = MaskSource.Camera.rippleData;
				if (rippleData != null)
				{
					if (!isGameplay)
					{
						ripple = MaskMaker.MakeSource("RippleGrab", maskShader, isInverted);
					}
					else if (!rippleData.gameplayRippleActive && isTransition)
					{
						ripple = MaskMaker.MakeSource("GameplayRippleGrab", maskShader, isSecondLayer: false, null, pos, null, Vector3.one * 10000f);
						ripple.interpolate = false;
						rippleData.transitionRippleSpawned = true;
					}
					else if (rippleData.transitionRippleSpawned || rippleData.gameplayRippleAnimation < 0.1f)
					{
						persistentTear = maskShader == "ShiftMask";
						ripple = MaskMaker.MakeSource("GameplayRippleGrab", persistentTear ? (maskShader = "RippleTearMask") : maskShader);
					}
					UpdateRotation();
				}
			}
			if (ripple != null)
			{
				float num = scale;
				if (room.game.IsStorySession && data.cycleExpiry > 0)
				{
					float num2 = (room.game.GetStorySession.saveState.cycleNumber - data.cycleSpawnedOn) / data.cycleExpiry;
					num *= 1f - num2 * 0.6f;
				}
				if (data.animateOnSpawn)
				{
					if (persistentTear)
					{
						displayScale = num;
						displayIntensity = Mathf.Min(displayIntensity + data.animationSpeed, 1f);
						if (displayIntensity == 1f)
						{
							data.animateOnSpawn = false;
						}
					}
					else
					{
						displayScale = Mathf.Lerp(displayScale, num, data.animationSpeed);
						displayIntensity = intensity * (displayScale / num);
						if (Math.Abs(displayScale - num) <= num * 0.05f)
						{
							data.animateOnSpawn = false;
						}
					}
				}
				else
				{
					displayScale = num;
					displayIntensity = intensity;
				}
				ripple.SetPos(pos);
				ripple.SetRotation(rotation * Vector3.back);
				ripple.SetScale(new Vector3(displayScale / 8f / (1f - squish * 0.99f), displayScale / 8f * (1f - squish), 1f));
				if (!ripple.interpolate)
				{
					interpolateDelay--;
					if (interpolateDelay <= 0)
					{
						ripple.interpolate = true;
					}
				}
				if (data.isGameplay && data.cycleExpiry > 0 && !data.animateOnSpawn && UnityEngine.Random.value < 0.0033333334f)
				{
					if (persistentTear)
					{
						room.MaterializeRippleSpawn(PointToTear(pos + new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * UnityEngine.Random.Range(0f, data.scale)), Room.RippleSpawnSource.Ripple);
					}
					else
					{
						room.MaterializeRippleSpawn(pos + Custom.RNV() * UnityEngine.Random.Range(0f, data.scale * 0.8f), Room.RippleSpawnSource.Ripple);
					}
				}
			}
			if (room.game.IsStorySession && data.cycleExpiry > 0 && room.game.GetStorySession.saveState.cycleNumber - data.cycleSpawnedOn > data.cycleExpiry && !base.slatedForDeletetion)
			{
				base.slatedForDeletetion = true;
				RemoveObject();
				room.RemoveObject(this);
			}
		}
		else if (ripple != null)
		{
			ripple = null;
		}
	}

	public float PointInRipple(Vector2 point)
	{
		if (ripple?.obj == null)
		{
			return 0f;
		}
		if (data.invert)
		{
			return 0f;
		}
		if (displayIntensity == 0f)
		{
			return 0f;
		}
		if ((double)depthMix > 0.75 || (double)depthMix < 0.25)
		{
			return 0f;
		}
		float num = 0f;
		Vector2 vector = point - pos;
		vector = Quaternion.Euler(-ripple.obj.transform.localRotation.eulerAngles) * vector;
		vector /= (Vector2)ripple.obj.transform.localScale;
		if (persistentTear)
		{
			float num2 = Mathf.Clamp01(displayIntensity * 3f - 1.5f);
			return Mathf.InverseLerp(0.3f * num2, 0.15f * num2, (vector * new Vector2(1.5f, 1.1f)).magnitude);
		}
		if (!data.square)
		{
			num = Mathf.InverseLerp(0.4999f - fallOff * 0.5f, 0.5f, vector.magnitude);
			return Mathf.InverseLerp(0.1f, 0.8f, (1f - num) * displayIntensity);
		}
		vector.x = Mathf.Abs(vector.x);
		vector.y = Mathf.Abs(vector.y);
		vector.x = Mathf.InverseLerp(0.5f - fallOff, 0.5001f, vector.x);
		vector.y = Mathf.InverseLerp(0.5f - fallOff, 0.5001f, vector.y);
		num = 1f - Mathf.Lerp(Mathf.Max(vector.x, vector.y), vector.magnitude, (vector.x * vector.y > 0f) ? 1f : 0f);
		return Mathf.InverseLerp(0.1f, 0.2f, num * displayIntensity);
	}

	public Vector2 PointToTear(Vector2 point)
	{
		point -= pos;
		for (int num = 4; num >= 0; num--)
		{
			float num2 = (float)num / 4f;
			Vector2 vector = point * num2;
			vector = Quaternion.Euler(-ripple.obj.transform.localRotation.eulerAngles) * vector;
			vector /= (Vector2)ripple.obj.transform.localScale;
			if (Mathf.InverseLerp(0.3f, 0.15f, (vector * new Vector2(1.5f, 1.1f)).magnitude) > 0.2f)
			{
				return point * num2 + pos;
			}
		}
		return Vector2.zero;
	}

	public override void Update(bool eu)
	{
		lastPos = pos;
		lastScale = displayScale;
		if (isTransition)
		{
			lastTransitionOpenness = transitionOpenness;
			lastTransitionShape = transitionShape;
		}
		RippleUpdate();
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (ripple != null)
		{
			setProps(timeStacker);
			if (isTransition)
			{
				ripple.SetProperty("_Openness", Mathf.Lerp(lastTransitionOpenness, transitionOpenness, timeStacker));
				ripple.SetProperty("_RingsToSpirals", Mathf.Lerp(lastTransitionShape, transitionShape, timeStacker));
			}
			ripple.DrawUpdate(timeStacker, rCam, camPos);
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
	}
}
