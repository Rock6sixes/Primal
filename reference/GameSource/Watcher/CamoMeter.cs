using HUD;
using RWCustom;
using UnityEngine;

namespace Watcher;

public class CamoMeter : HudPart
{
	public const float spriteWidth = 340f;

	public const float spriteHeight = 80f;

	public const float spriteAnchorX = 0.11764706f;

	public PlayerSpecificMultiplayerHud multiHud;

	public Vector2 pos;

	public Vector2 lastPos;

	public FSprite meterSprite;

	public float fade;

	public float lastFade;

	public float full;

	public float lastFull;

	public float percentLimited;

	public float lastAnimTime;

	public float animTime;

	public float animSpeed;

	public RainWorldGame jollyCoopGameRef;

	public Player Player
	{
		get
		{
			if (multiHud != null)
			{
				return multiHud.RealizedPlayer;
			}
			return hud.owner as Player;
		}
	}

	public bool Unlocked
	{
		get
		{
			if (!(Player.maxRippleLevel >= 0.5f))
			{
				return Player.OutsideWatcherCampaign;
			}
			return true;
		}
	}

	public bool ForceShow
	{
		get
		{
			if (!Unlocked)
			{
				return false;
			}
			if (jollyCoopGameRef != null)
			{
				for (int i = 0; i < jollyCoopGameRef.Players.Count; i++)
				{
					if (jollyCoopGameRef.Players[i].realizedCreature != null)
					{
						Player player = jollyCoopGameRef.Players[i].realizedCreature as Player;
						if (player.SlugCatClass == WatcherEnums.SlugcatStatsName.Watcher && ((player.isCamo && player.camoCharge >= player.usableCamoLimit * 0.7f) || player.activateCamoTimer > 0 || (player.isCamo && player.inCamoTime < 400)))
						{
							return true;
						}
					}
				}
				return false;
			}
			if ((!Player.isCamo || !(Player.camoCharge >= Player.usableCamoLimit * 0.7f)) && Player.activateCamoTimer <= 0)
			{
				if (Player.isCamo)
				{
					return Player.inCamoTime < 400;
				}
				return false;
			}
			return true;
		}
	}

	public CamoMeter(global::HUD.HUD hud, PlayerSpecificMultiplayerHud multiHud, FContainer fContainer)
		: base(hud)
	{
		this.multiHud = multiHud;
		if (multiHud == null)
		{
			pos = new Vector2(80f, 70f);
		}
		else
		{
			Vector2 cornerPos = multiHud.cornerPos;
			if ((float)multiHud.flip < 0f)
			{
				cornerPos.x -= 300f;
			}
			else
			{
				cornerPos.x += 20f;
			}
			if (cornerPos.y < 100f)
			{
				cornerPos.y += 40f;
			}
			else
			{
				cornerPos.y -= 40f;
			}
			pos = cornerPos;
		}
		lastPos = pos;
		fade = 0f;
		lastFade = 0f;
		meterSprite = new FSprite(GetBaseTexture())
		{
			anchorX = 0.11764706f,
			scaleX = 21.25f,
			scaleY = 5f,
			shader = hud.rainWorld.Shaders["CamoMeter"]
		};
		fContainer.AddChild(meterSprite);
	}

	public string GetBaseTexture()
	{
		if (multiHud == null || (multiHud.abstractPlayer.state as PlayerState).playerNumber == 0)
		{
			return "Futile_White";
		}
		string text = "CamoMeter_" + (multiHud.abstractPlayer.state as PlayerState).playerNumber;
		if (!Futile.atlasManager.DoesContainElementWithName(text))
		{
			Texture2D texture2D = new Texture2D(16, 16);
			texture2D.filterMode = FilterMode.Bilinear;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			Color color = PlayerColor();
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					texture2D.SetPixel(j, i, color);
				}
			}
			texture2D.Apply();
			Futile.atlasManager.LoadAtlasFromTexture(text, texture2D, textureFromAsset: false);
		}
		return text;
	}

	public Color PlayerColor()
	{
		if (multiHud != null)
		{
			switch ((multiHud.abstractPlayer.state as PlayerState).playerNumber)
			{
			case 0:
				return PlayerGraphics.SlugcatColor(SlugcatStats.Name.White);
			case 1:
				return PlayerGraphics.SlugcatColor(SlugcatStats.Name.Yellow);
			case 2:
				return PlayerGraphics.SlugcatColor(SlugcatStats.Name.Red);
			case 3:
				return Color.Lerp(Color.white, PlayerGraphics.SlugcatColor(SlugcatStats.Name.Night), 0.7f);
			}
		}
		return Color.white;
	}

	public override void Update()
	{
		lastPos = pos;
		lastFade = fade;
		lastFull = full;
		lastAnimTime = animTime;
		if (ModManager.CoopAvailable && Player.room != null && Player.room.game.IsStorySession)
		{
			jollyCoopGameRef = Player.room.game;
		}
		if (ForceShow && multiHud == null)
		{
			hud.karmaMeter.forceVisibleCounter = Mathf.Max(hud.karmaMeter.forceVisibleCounter, 60);
		}
		bool flag = false;
		float num = -1f;
		if (jollyCoopGameRef != null && jollyCoopGameRef.Players.Count > 1)
		{
			for (int i = 0; i < jollyCoopGameRef.Players.Count; i++)
			{
				if (jollyCoopGameRef.Players[i].realizedCreature == null)
				{
					continue;
				}
				Player player = jollyCoopGameRef.Players[i].realizedCreature as Player;
				if (player.SlugCatClass == WatcherEnums.SlugcatStatsName.Watcher)
				{
					if (player.isCamo)
					{
						flag = true;
					}
					if (num < 0f || player.camoCharge < num)
					{
						num = player.camoCharge;
					}
				}
			}
		}
		else
		{
			flag = Player.isCamo;
			num = Player.camoCharge;
		}
		float to = ((num == 0f) ? 0f : ((!flag) ? (-0.5f) : 1f));
		animSpeed = Custom.LerpAndTick(animSpeed, to, 0.02f, 0.01f);
		animTime += animSpeed / 40f;
		full = 1f - num / Player.camoLimit;
		if (multiHud == null)
		{
			pos = hud.karmaMeter.pos;
			fade = (Unlocked ? hud.karmaMeter.fade : 0f);
		}
		else if (full < 1f || hud.showKarmaFoodRain)
		{
			fade = Mathf.Min(1f, fade + 0.05f);
		}
		else
		{
			fade = Mathf.Max(0f, fade - 0.05f);
		}
		if (hud.HideGeneralHud)
		{
			fade = 0f;
		}
		if (Player.room == null || !Player.room.game.IsStorySession)
		{
			return;
		}
		StoryGameSession getStorySession = Player.room.game.GetStorySession;
		if (getStorySession != null)
		{
			percentLimited = 0f;
			if (getStorySession.warpTraversalsLeftUntilFullWarpFatigue < getStorySession.warpFatigueDecayLength)
			{
				percentLimited = 1f - (float)getStorySession.warpTraversalsLeftUntilFullWarpFatigue / (float)getStorySession.warpFatigueDecayLength;
			}
		}
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public override void Draw(float timeStacker)
	{
		float a = Mathf.Lerp(lastFade, fade, timeStacker);
		float r = Mathf.Lerp(lastFull, full, timeStacker);
		float b = Mathf.Lerp(lastAnimTime, animTime, timeStacker);
		float num = 1f;
		if (multiHud == null)
		{
			num = 1f + 0.2f * Mathf.Lerp(hud.foodMeter.lastFade, hud.foodMeter.fade, timeStacker);
		}
		meterSprite.SetPosition(DrawPos(timeStacker));
		meterSprite.color = new Color(r, percentLimited, b, a);
		meterSprite.scaleY = 5f * num;
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		meterSprite.RemoveFromContainer();
		multiHud?.ClearSprites();
	}
}
