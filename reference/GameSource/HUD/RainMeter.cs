using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace HUD;

public class RainMeter : HudPart
{
	public Vector2 pos;

	public Vector2 lastPos;

	public int remainVisibleCounter;

	public float fade;

	public float lastFade;

	public float plop;

	public HUDCircle[] circles;

	public float fRain;

	public int halfTimeBlink;

	public bool halfTimeShown;

	public int tickCounter;

	public float tickPulse;

	public int timePerCircle;

	public bool Show
	{
		get
		{
			if (halfTimeBlink <= 0 && !hud.showKarmaFoodRain)
			{
				return hud.owner.RevealMap;
			}
			return true;
		}
	}

	public RainMeter(HUD hud, FContainer fContainer)
		: base(hud)
	{
		lastPos = pos;
		World world = (hud.owner as Player).abstractCreature.world;
		timePerCircle = 1200;
		int num = world.rainCycle.cycleLength / timePerCircle;
		if (num > 30)
		{
			num = 30;
			timePerCircle = world.rainCycle.cycleLength / num;
		}
		circles = new HUDCircle[num];
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i] = new HUDCircle(hud, HUDCircle.SnapToGraphic.smallEmptyCircle, fContainer, 0);
		}
		if (ModManager.MSC && world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint && !Region.IsRubiconRegion(hud.map.RegionName))
		{
			halfTimeShown = true;
		}
	}

	public override void Update()
	{
		World world = (hud.owner as Player).abstractCreature.world;
		bool disableRain = world.game.setupValues.disableRain;
		if (ModManager.MSC && world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint && !Region.IsRubiconRegion(hud.map.RegionName))
		{
			halfTimeShown = true;
		}
		if (ModManager.MSC && (hud.owner as Player).inVoidSea)
		{
			halfTimeShown = true;
		}
		lastPos = pos;
		pos = hud.karmaMeter.pos;
		if (!halfTimeShown && !disableRain && world.rainCycle.AmountLeft < 0.5f && (hud.owner as Player).room != null && (hud.owner as Player).room.roomSettings.DangerType != RoomRain.DangerType.None && (!ModManager.MMF || !world.rainCycle.RegionHidesTimer))
		{
			halfTimeBlink = 220;
			halfTimeShown = true;
		}
		lastFade = fade;
		if (remainVisibleCounter > 0)
		{
			remainVisibleCounter--;
		}
		if (halfTimeBlink > 0)
		{
			halfTimeBlink--;
			hud.karmaMeter.forceVisibleCounter = Math.Max(hud.karmaMeter.forceVisibleCounter, 10);
		}
		if (ModManager.MMF && MMF.cfgTickTock.Value)
		{
			tickPulse = Mathf.Lerp(tickPulse, 0f, 0.1f);
		}
		else
		{
			tickPulse = 0f;
		}
		if ((hud.karmaMeter.fade > 0f && Show) || remainVisibleCounter > 0)
		{
			fade = Mathf.Min(1f, fade + 1f / 30f);
			if (ModManager.MMF && MMF.cfgTickTock.Value && (hud.owner as Player).room != null && hud.owner.RevealMap && world.rainCycle.AmountLeft > 0f && !world.rainCycle.RegionHidesTimer)
			{
				tickCounter++;
				if (tickCounter % 240 == 0)
				{
					(hud.owner as Player).room.PlaySound(MMFEnums.MMFSoundID.Tick, 0f, 0.85f, 1f);
					tickPulse = 1f;
				}
				if (tickCounter % 240 == 120)
				{
					(hud.owner as Player).room.PlaySound(MMFEnums.MMFSoundID.Tock, 0f, 0.85f, 1f);
					tickPulse = 1f;
				}
			}
		}
		else
		{
			fade = Mathf.Max(0f, fade - 0.1f);
		}
		if (hud.HideGeneralHud)
		{
			fade = 0f;
		}
		if (fade >= 0.7f)
		{
			plop = Mathf.Min(1f, plop + 0.05f);
		}
		else
		{
			plop = 0f;
		}
		if (disableRain)
		{
			fRain = 1f;
		}
		else
		{
			fRain = world.rainCycle.AmountLeft;
		}
		bool flag = ModManager.MMF && MMF.cfgHideRainMeterNoThreat.Value && world.rainCycle.RegionHidesTimer && ((hud.owner as Player).room == null || (hud.owner as Player).room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.DayNight) == 0f);
		for (int i = 0; i < circles.Length; i++)
		{
			circles[i].Update();
			if (fade > 0f || lastFade > 0f)
			{
				float num = (float)i / (float)(circles.Length - 1);
				float value = Mathf.InverseLerp((float)i / (float)circles.Length, (float)(i + 1) / (float)circles.Length, fRain);
				float num2 = Mathf.InverseLerp(0.5f, 0.475f, Mathf.Abs(0.5f - Mathf.InverseLerp(1f / 30f, 1f, value)));
				if (flag)
				{
					circles[i].rad = (3f * Mathf.Pow(fade, 2f) + Mathf.InverseLerp(0.075f, 0f, Mathf.Abs(1f - num - Mathf.Lerp((1f - fRain) * fade - 0.075f, 1.075f, Mathf.Pow(plop, 0.85f)))) * 2f * fade) * Mathf.InverseLerp(0f, 1f / 30f, 1f);
					circles[i].thickness = 1f;
					circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
					circles[i].snapRad = 3f;
					circles[i].snapThickness = 1f;
				}
				else
				{
					if (halfTimeBlink > 0)
					{
						num2 = Mathf.Max(num2, (halfTimeBlink % 15 < 7) ? 0f : 1f);
					}
					circles[i].rad = ((2f + num2) * Mathf.Pow(fade, 2f) + Mathf.InverseLerp(0.075f, 0f, Mathf.Abs(1f - num - Mathf.Lerp((1f - fRain) * fade - 0.075f, 1.075f, Mathf.Pow(plop, 0.85f)))) * 2f * fade) * Mathf.InverseLerp(0f, 1f / 30f, value);
					if (num2 == 0f)
					{
						circles[i].thickness = -1f;
						circles[i].snapGraphic = HUDCircle.SnapToGraphic.Circle4;
						circles[i].snapRad = 2f;
						circles[i].snapThickness = -1f;
					}
					else
					{
						circles[i].thickness = Mathf.Lerp(3.5f, 1f, num2);
						circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
						circles[i].snapRad = 3f;
						circles[i].snapThickness = 1f;
					}
				}
				circles[i].pos = pos + Custom.DegToVec((1f - (float)i / (float)circles.Length) * 360f * Custom.SCurve(Mathf.Pow(fade, 1.5f - num), 0.6f)) * (hud.karmaMeter.Radius + 8.5f + num2 + 4f * tickPulse);
			}
			else
			{
				circles[i].rad = 0f;
			}
		}
	}

	public Vector2 DrawPos(float timeStacker)
	{
		return Vector2.Lerp(lastPos, pos, timeStacker);
	}

	public override void Draw(float timeStacker)
	{
		if (!ModManager.MSC || !(hud.owner.GetOwnerType() == HUD.OwnerType.Player) || !((hud.owner as Player).abstractCreature.world.game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint) || Region.IsRubiconRegion(hud.map.RegionName))
		{
			for (int i = 0; i < circles.Length; i++)
			{
				circles[i].Draw(timeStacker);
			}
		}
	}

	public void ResetHalfTime()
	{
		halfTimeShown = false;
		halfTimeBlink = 0;
	}

	public void SuppressHalfTime()
	{
		halfTimeShown = true;
	}

	public override void ClearSprites()
	{
		HUDCircle[] array = circles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ClearSprite();
		}
	}
}
