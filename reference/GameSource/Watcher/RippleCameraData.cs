using UnityEngine;
using UnityEngine.Rendering;

namespace Watcher;

public class RippleCameraData
{
	public static Material combinerMaterial = new Material(Shader.Find("Futile/LevelBlend"));

	public string levelBlendBufferName = "levelblend";

	public CommandBuffer saveRippleMaskBuffer;

	public RenderTexture rippleMaskRT;

	public Material rippleMaskSaver = new Material(Shader.Find("Futile/SaveRippleMask"));

	public CommandBuffer savePlayerCamoMaskBuffer;

	public RenderTexture playerCamoMaskRT;

	public Material playerCamoMaskSaver = new Material(Shader.Find("Futile/SavePlayerCamoMask"));

	public Texture2D rippleTargetScreen;

	public Texture2D gameplayTargetScreen;

	public byte[] preLoadTexture;

	public byte[] preLoadGameplayTexture;

	public bool isPassAdded;

	public bool hasScreen;

	public bool hasGameplayScreen;

	public bool needClear;

	public int? paletteToLoad;

	public float paletteAmount;

	public float _trailPaletteAmount = 1f;

	public Texture2D paletteTexture;

	public Texture2D gameplayPaletteTexture;

	public Texture2D fadeTex;

	public Texture2D gameplayFadeTex;

	public RoomPalette currentPalette;

	public RoomPalette gameplayPalette;

	public float trailAmount;

	public bool isTrailAmountChanged;

	public bool gameplayRippleActive;

	public float gameplayRippleAnimation;

	public bool transitionRippleSpawned;

	public float trailPaletteAmountStored;

	public float trailAmountStored;

	public RoomCamera camera;

	public Vector2 transitionDirection;

	public int screenTransition;

	public float trailPaletteAmount
	{
		get
		{
			return _trailPaletteAmount;
		}
		set
		{
			_trailPaletteAmount = value;
			Shader.SetGlobalFloat("_RippleTrailPaletteAmount", _trailPaletteAmount);
		}
	}

	public float TrailAmount
	{
		get
		{
			return trailAmount;
		}
		set
		{
			trailAmount = value;
			isTrailAmountChanged = true;
		}
	}

	public RippleCameraData(RoomCamera owner)
	{
		camera = owner;
		rippleTargetScreen = new Texture2D(1400, 800, TextureFormat.ARGB32, mipChain: false);
		rippleTargetScreen.filterMode = FilterMode.Point;
		gameplayTargetScreen = new Texture2D(1400, 800, TextureFormat.ARGB32, mipChain: false);
		gameplayTargetScreen.filterMode = FilterMode.Point;
		rippleMaskRT = new RenderTexture(Futile.screen.pixelWidth / 2, Futile.screen.pixelHeight / 2, 0, RenderTextureFormat.ARGB32);
		rippleMaskRT.useMipMap = true;
		rippleMaskRT.autoGenerateMips = true;
		rippleMaskRT.filterMode = FilterMode.Bilinear;
		playerCamoMaskRT = new RenderTexture(Futile.screen.pixelWidth, Futile.screen.pixelHeight, 0, RenderTextureFormat.ARGB32);
		playerCamoMaskRT.useMipMap = true;
		playerCamoMaskRT.autoGenerateMips = true;
		playerCamoMaskRT.filterMode = FilterMode.Bilinear;
		paletteTexture = new Texture2D(32, 8, TextureFormat.ARGB32, mipChain: false);
		paletteTexture.anisoLevel = 0;
		paletteTexture.filterMode = FilterMode.Point;
		paletteTexture.wrapMode = TextureWrapMode.Clamp;
		gameplayPaletteTexture = new Texture2D(32, 8, TextureFormat.ARGB32, mipChain: false);
		gameplayPaletteTexture.anisoLevel = 0;
		gameplayPaletteTexture.filterMode = FilterMode.Point;
		gameplayPaletteTexture.wrapMode = TextureWrapMode.Clamp;
		Shader.SetGlobalTexture("_RippleMaskSaved", rippleMaskRT);
		Shader.SetGlobalTexture("_PlayerCamoMaskSaved", playerCamoMaskRT);
		Shader.DisableKeyword("GAMEPLAYRIPPLETEXTURE");
	}

	public void LoadGameplayPalette()
	{
		camera.LoadPalette(332, ref gameplayFadeTex);
		ApplyGameplayPalette();
	}

	public void ApplyGameplayPalette()
	{
		gameplayPalette = new RoomPalette(gameplayPaletteTexture, 1f - gameplayPaletteTexture.GetPixel(9, 7).r, 1f - gameplayPaletteTexture.GetPixel(30, 7).r, gameplayPaletteTexture.GetPixel(2, 7), gameplayPaletteTexture.GetPixel(4, 7), gameplayPaletteTexture.GetPixel(5, 7), gameplayPaletteTexture.GetPixel(6, 7), gameplayPaletteTexture.GetPixel(7, 7), gameplayPaletteTexture.GetPixel(8, 7), gameplayPaletteTexture.GetPixel(1, 7), gameplayPaletteTexture.GetPixel(0, 7), gameplayPaletteTexture.GetPixel(10, 7), gameplayPaletteTexture.GetPixel(11, 7), gameplayPaletteTexture.GetPixel(12, 7), gameplayPaletteTexture.GetPixel(13, 7));
	}

	public void LoadPalette()
	{
		if (paletteToLoad.HasValue)
		{
			camera.LoadPalette(paletteToLoad.Value, ref fadeTex);
			paletteToLoad = null;
			ApplyPalette();
		}
	}

	public void ApplyPalette()
	{
		currentPalette = new RoomPalette(paletteTexture, 1f - paletteTexture.GetPixel(9, 7).r, 1f - paletteTexture.GetPixel(30, 7).r, paletteTexture.GetPixel(2, 7), paletteTexture.GetPixel(4, 7), paletteTexture.GetPixel(5, 7), paletteTexture.GetPixel(6, 7), paletteTexture.GetPixel(7, 7), paletteTexture.GetPixel(8, 7), paletteTexture.GetPixel(1, 7), paletteTexture.GetPixel(0, 7), paletteTexture.GetPixel(10, 7), paletteTexture.GetPixel(11, 7), paletteTexture.GetPixel(12, 7), paletteTexture.GetPixel(13, 7));
		float value = currentPalette.fogAmount;
		if (currentPalette.fogColor.r == 0f && currentPalette.fogColor.g == 0f && currentPalette.fogColor.b > 0f)
		{
			value = 1f + currentPalette.fogColor.b;
		}
		Shader.SetGlobalFloat("_rippleFogAmount", value);
	}

	public void AddCommandBuffer()
	{
		if (!hasScreen && camera.levelTexCombiner.combinedLevelTex == null)
		{
			camera.levelTexCombiner.Initialize();
		}
		camera.levelTexCombiner.AddPass(hasScreen ? ((Texture)rippleTargetScreen) : ((Texture)camera.levelTexCombiner.combinedLevelTex), combinerMaterial, levelBlendBufferName, CameraEvent.BeforeForwardAlpha);
		Shader.EnableKeyword("RIPPLE");
		isPassAdded = true;
		if (saveRippleMaskBuffer == null)
		{
			saveRippleMaskBuffer = new CommandBuffer();
			saveRippleMaskBuffer.name = "SaveRippleMask";
			saveRippleMaskBuffer.Blit(null, rippleMaskRT, rippleMaskSaver);
		}
		Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque, saveRippleMaskBuffer);
		if (savePlayerCamoMaskBuffer == null)
		{
			savePlayerCamoMaskBuffer = new CommandBuffer();
			savePlayerCamoMaskBuffer.name = "SavePlayerCamoMask";
			savePlayerCamoMaskBuffer.Blit(null, playerCamoMaskRT, playerCamoMaskSaver);
		}
		Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, savePlayerCamoMaskBuffer);
		LoadPalette();
	}

	public void RemoveCommandBuffer()
	{
		camera.levelTexCombiner.RemovePass(levelBlendBufferName);
		Shader.DisableKeyword("RIPPLE");
		isPassAdded = false;
		transitionRippleSpawned = false;
		if (saveRippleMaskBuffer != null)
		{
			Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, saveRippleMaskBuffer);
			RoomCamera roomCamera = camera;
			if (roomCamera != null && roomCamera.AboutToSwitchRoom)
			{
				Graphics.Blit(Texture2D.blackTexture, rippleMaskRT);
			}
		}
		if (savePlayerCamoMaskBuffer != null)
		{
			Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, savePlayerCamoMaskBuffer);
			RoomCamera roomCamera2 = camera;
			if (roomCamera2 != null && roomCamera2.AboutToSwitchRoom)
			{
				Graphics.Blit(Texture2D.blackTexture, playerCamoMaskSaver);
			}
		}
	}

	public void PreLoadTexture(string path)
	{
		if (path != null)
		{
			preLoadTexture = AssetManager.PreLoadTexture(path);
			hasScreen = true;
		}
		else
		{
			hasScreen = false;
		}
	}

	public void PreLoadGameplayTexture(string path)
	{
		if (path != null)
		{
			preLoadGameplayTexture = AssetManager.PreLoadTexture(path);
			hasGameplayScreen = true;
		}
		else
		{
			hasGameplayScreen = false;
		}
	}

	public void SetGlobals()
	{
		if (hasScreen)
		{
			Shader.SetGlobalTexture("_RippleLevelTex", rippleTargetScreen);
		}
		else
		{
			Shader.SetGlobalTexture("_RippleLevelTex", camera.levelTexCombiner.combinedLevelTex);
		}
		if (hasGameplayScreen)
		{
			Shader.EnableKeyword("GAMEPLAYRIPPLETEXTURE");
			Shader.SetGlobalTexture("_GameplayRippleTexture", gameplayTargetScreen);
		}
		else
		{
			Shader.DisableKeyword("GAMEPLAYRIPPLETEXTURE");
			Shader.SetGlobalTexture("_GameplayRippleTexture", Texture2D.whiteTexture);
		}
		Shader.SetGlobalFloat("_ForceRippleTrailSwirl", 0f);
	}

	public void SetScreenTransitionDirection(int nextCamPos)
	{
		if (camera.room == null)
		{
			return;
		}
		if (nextCamPos == -1)
		{
			transitionDirection = Vector2.zero;
			return;
		}
		Vector2 vector = Vector2.one;
		if (nextCamPos != -1)
		{
			vector = camera.room.cameraPositions[camera.currentCameraPosition] - camera.room.cameraPositions[nextCamPos];
			vector /= camera.sSize;
			vector = ((Mathf.Abs(vector.x) > Mathf.Abs(vector.y)) ? (Vector2.right * Mathf.Sign(vector.x)) : (Vector2.up * (0f - Mathf.Sign(vector.y))));
		}
		transitionDirection = -vector;
	}

	public void Clear()
	{
		Shader.SetGlobalTexture("_RippleMask", Texture2D.blackTexture);
		Graphics.Blit(Texture2D.blackTexture, playerCamoMaskSaver);
		Graphics.Blit(Texture2D.blackTexture, rippleMaskRT);
		needClear = false;
	}

	public void SetParams()
	{
		MaskLayer maskLayer = MaskMaker.maskMaker.FindMask("RippleGrab");
		if (maskLayer != null)
		{
			float num = ((float)camera.room.game.framesPerSecond + 20f) * Time.deltaTime;
			maskLayer.maskSources[0]?.SetProperty(0, trailAmount / num);
		}
	}

	public void SetTarget()
	{
		Shader.SetGlobalFloat("_RipplePaletteAmount", paletteAmount);
		trailPaletteAmount = _trailPaletteAmount;
		if (hasScreen)
		{
			rippleTargetScreen.LoadImage(preLoadTexture, markNonReadable: false);
		}
		if (hasGameplayScreen)
		{
			gameplayTargetScreen.LoadImage(preLoadGameplayTexture, markNonReadable: false);
		}
		SetGlobals();
	}

	public void Update()
	{
		if (isTrailAmountChanged)
		{
			SetParams();
		}
		if (paletteTexture != null)
		{
			Shader.SetGlobalTexture("_RipplePalTex", paletteTexture);
		}
		if (gameplayPaletteTexture != null)
		{
			Shader.SetGlobalTexture("_GameplayRipplePalTex", gameplayPaletteTexture);
		}
	}

	public void DrawUpdate()
	{
		if (camera.levelTextureChangeFrame == Time.renderedFrameCount)
		{
			rippleMaskSaver.SetVector("_Offset", transitionDirection);
		}
		else if (camera.levelTextureChangeFrame + 1 <= Time.renderedFrameCount)
		{
			rippleMaskSaver.SetVector("_Offset", Vector2.zero);
		}
		Shader.SetGlobalTexture("_PlayerCamoMask", Texture2D.blackTexture);
		Shader.SetGlobalTexture("_PlayerCamoMaskBeforePlayer", Texture2D.blackTexture);
	}

	public float PointInRipple(Vector2 point)
	{
		if (camera.room.cosmeticRipples.Count == 0)
		{
			return 0f;
		}
		int count = camera.room.cosmeticRipples.Count;
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			num = Mathf.Max(num, camera.room.cosmeticRipples[i].PointInRipple(point));
		}
		return num;
	}

	public void StoreTrailParameters()
	{
		trailAmountStored = trailAmount;
		trailPaletteAmountStored = _trailPaletteAmount;
	}

	public void Dispose()
	{
		RemoveCommandBuffer();
		savePlayerCamoMaskBuffer?.Dispose();
		saveRippleMaskBuffer?.Dispose();
		Object.Destroy(rippleMaskRT);
		Object.Destroy(rippleMaskSaver);
		Object.Destroy(playerCamoMaskRT);
		Object.Destroy(rippleTargetScreen);
		Object.Destroy(gameplayTargetScreen);
		Object.Destroy(paletteTexture);
		Object.Destroy(gameplayPaletteTexture);
		Object.Destroy(fadeTex);
		Object.Destroy(gameplayFadeTex);
	}
}
