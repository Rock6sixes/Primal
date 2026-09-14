using System;
using System.Collections.Generic;
using System.IO;
using HUD;
using RWCustom;
using UnityEngine;

namespace Menu;

public class RippleLadder : PositionedMenuObject
{
	public global::HUD.HUD hud;

	public float scroll;

	public float lastScroll;

	public float scrollVel;

	public bool movementShown;

	public int moveToKarma;

	public int minKarma;

	public int maxKarma;

	public bool cappedMovement;

	public IntVector2 displayKarma;

	public bool karmaIncreasing;

	public bool reinforced;

	public bool playerDeath;

	public bool startedAnimating;

	public KarmaLadder.Phase phase = KarmaLadder.Phase.Resting;

	public int ticksInPhase;

	public float ladderAlpha;

	public float glowAlpha;

	public MenuMicrophone.MenuSoundLoop movementSoundLoop;

	public List<MenuIllustration> rippleLevels;

	public MenuIllustration rippleGlow;

	public MenuIllustration reinforcedSymbol;

	public float reinforcementWhite;

	public float lastReinforcementWhite;

	public float reinforcementDeath;

	public float lastReinforcementDeath;

	public float reinforcementCycle;

	public float lastReinforcementCycle;

	public float reinforcementClamp;

	public float lastReinforcementClamp;

	public float reinforcementLines;

	public float lastReinforcementLines;

	public FContainer[] containers => (menu as KarmaLadderScreen).ladderContainers;

	public bool AllAnimationDone
	{
		get
		{
			if (startedAnimating)
			{
				return displayKarma.x == moveToKarma;
			}
			return false;
		}
	}

	public int displayKarmaCap => maxKarma;

	public RippleLadder(Menu menu, MenuObject owner, Vector2 pos, global::HUD.HUD hud, IntVector2 displayKarma, float minRipple, float maxRipple, bool reinforced, bool playerDeath)
		: base(menu, owner, pos)
	{
		this.hud = hud;
		this.displayKarma = displayKarma;
		this.reinforced = reinforced;
		this.playerDeath = playerDeath;
		minKarma = (int)((minRipple - 1f) / 0.5f);
		maxKarma = (int)((maxRipple - 1f) / 0.5f);
		displayKarma.x = Custom.IntClamp(displayKarma.x, minKarma, maxKarma);
		if (playerDeath && !reinforced)
		{
			displayKarma.y--;
		}
		moveToKarma = Custom.IntClamp(displayKarma.y - 100, minKarma, maxKarma);
		scroll = displayKarma.x;
		lastScroll = scroll;
		ladderAlpha = 1f;
		rippleLevels = new List<MenuIllustration>();
		string folderName = "Scenes" + Path.DirectorySeparatorChar + "ripple screen - watcher";
		string text = KarmaMeter.RippleSymbolSprite(small: false, (float)moveToKarma / 2f + 1f);
		text = text.Replace("ripple", "ripple - 0 - ");
		rippleGlow = new MenuIllustration(menu, this, folderName, text, Vector2.zero, crispPixels: false, anchorCenter: true);
		rippleGlow.alpha = 0f;
		rippleGlow.lastAlpha = 0f;
		subObjects.Add(rippleGlow);
		for (float num = 1f; num <= 5f; num += 0.5f)
		{
			string fileName = "ripplescreen - " + KarmaMeter.RippleSymbolSprite(small: false, num);
			MenuIllustration item = new MenuIllustration(menu, this, folderName, fileName, Vector2.zero, crispPixels: false, anchorCenter: true);
			rippleLevels.Add(item);
			subObjects.Add(item);
		}
		if (reinforced)
		{
			reinforcedSymbol = new MenuIllustration(menu, this, folderName, "reinforced", Vector2.zero, crispPixels: false, anchorCenter: true);
			reinforcedSymbol.alpha = 0f;
			reinforcedSymbol.lastAlpha = 0f;
			subObjects.Add(reinforcedSymbol);
		}
	}

	public override void Update()
	{
		base.Update();
		float num = (karmaIncreasing ? 0.15f : 0.3f);
		if (movementSoundLoop == null && menu.manager.menuMic != null)
		{
			movementSoundLoop = menu.PlayLoop(SoundID.MENU_Karma_Ladder_Movement_LOOP, 0f, 1f, 1f, isBkgLoop: false);
		}
		if (movementSoundLoop != null)
		{
			movementSoundLoop.loopVolume = Mathf.InverseLerp(0f, 0.01f, Mathf.Abs(lastScroll - scroll) * (1f / num));
			movementSoundLoop.loopPitch = Custom.LerpMap(Mathf.Abs(lastScroll - scroll) * (1f / num), 0f, 0.03f, 0.5f, 1f, 0.8f);
			if (movementSoundLoop.slatedForDeletion)
			{
				movementSoundLoop = null;
			}
		}
		lastScroll = scroll;
		lastReinforcementWhite = reinforcementWhite;
		lastReinforcementDeath = reinforcementDeath;
		lastReinforcementCycle = reinforcementCycle;
		lastReinforcementClamp = reinforcementClamp;
		lastReinforcementLines = reinforcementLines;
		scroll += scrollVel * num;
		if (AllAnimationDone)
		{
			glowAlpha = Mathf.Lerp(glowAlpha, 1f, 0.01f);
		}
		if (reinforced)
		{
			if (phase == KarmaLadder.Phase.MovingA || phase == KarmaLadder.Phase.MovingB || phase == KarmaLadder.Phase.Deselecting || phase == KarmaLadder.Phase.CappedMovement)
			{
				reinforcementWhite = Custom.LerpAndTick(reinforcementWhite, 0f, 0.07f, 1f / 52f);
			}
			else
			{
				reinforcementWhite = Custom.LerpAndTick(reinforcementWhite, 1f, 0.11f, 1f / 52f);
			}
			if (phase == KarmaLadder.Phase.MovingA || phase == KarmaLadder.Phase.MovingB)
			{
				reinforcementLines = Custom.LerpAndTick(reinforcementLines, 0f, 0.1f, 0.05f);
			}
			else
			{
				reinforcementLines = Mathf.Min(Custom.LerpAndTick(reinforcementLines, 1f, 0.1f, 0.05f), 1f - reinforcementDeath);
			}
		}
		else
		{
			reinforcementWhite = 0f;
			reinforcementLines = 0f;
		}
		if (phase != KarmaLadder.Phase.ReinforceSave)
		{
			reinforcementClamp = 0f;
		}
		ticksInPhase++;
		if (phase == KarmaLadder.Phase.Resting)
		{
			scrollVel *= 0.3f;
			scroll = Mathf.Lerp(scroll, displayKarma.x, 0.2f);
			if (displayKarma.x != moveToKarma || (playerDeath && reinforced))
			{
				NewPhase(KarmaLadder.Phase.Deselecting);
			}
		}
		else if (phase == KarmaLadder.Phase.Deselecting)
		{
			float num2 = Mathf.InverseLerp(0f, 30f, ticksInPhase);
			if (num2 >= 1f)
			{
				if (playerDeath && reinforced)
				{
					NewPhase(KarmaLadder.Phase.ReinforceSave);
					return;
				}
				if (cappedMovement)
				{
					NewPhase(KarmaLadder.Phase.CappedMovement);
					return;
				}
				NewPhase(KarmaLadder.Phase.MovingA);
				scrollVel += Mathf.Sign(displayKarma.x - moveToKarma) * 0.01f;
			}
		}
		else if (phase == KarmaLadder.Phase.MovingA || phase == KarmaLadder.Phase.MovingB)
		{
			bool flag = phase == KarmaLadder.Phase.MovingA;
			float num3 = (float)moveToKarma + Mathf.Sign(moveToKarma - displayKarma.x) * -0.049f * (flag ? 1f : 0f);
			scrollVel += (num3 - scroll) / (flag ? 400f : 3f);
			scrollVel += Mathf.Sign(num3 - scroll) * 0.001f;
			if (flag)
			{
				scrollVel *= 0.95f;
			}
			else
			{
				scrollVel *= 0.9f;
			}
			if (scroll < num3 == moveToKarma < displayKarma.x == flag)
			{
				if (flag)
				{
					NewPhase(KarmaLadder.Phase.MovingB);
				}
				else
				{
					NewPhase(KarmaLadder.Phase.Settling);
				}
			}
		}
		else if (phase == KarmaLadder.Phase.Settling)
		{
			float num2 = Mathf.InverseLerp(0f, 40f, ticksInPhase);
			scrollVel *= 0.6f;
			scroll = Mathf.Lerp(scroll, displayKarma.x, 0.2f);
			if (reinforced && reinforcementDeath > 0f)
			{
				reinforcementDeath = Mathf.Max(reinforcementDeath, Mathf.InverseLerp(0.2f, 0.8f, num2));
				if (reinforcementDeath >= 1f)
				{
					reinforced = false;
					menu.PlaySound(SoundID.MENU_Karma_Ladder_Reinforcement_Dissipate_B);
				}
			}
			if (num2 >= 1f)
			{
				if (karmaIncreasing && cappedMovement)
				{
					NewPhase(KarmaLadder.Phase.CapBump);
				}
				else if (karmaIncreasing)
				{
					NewPhase(KarmaLadder.Phase.Bump);
				}
				else
				{
					NewPhase(KarmaLadder.Phase.Resting);
				}
			}
		}
		else if (phase == KarmaLadder.Phase.Bump)
		{
			scrollVel *= 0.4f;
			scroll = Mathf.Lerp(scroll, displayKarma.x, 0.2f);
			float num2 = Mathf.InverseLerp(0f, 45f, ticksInPhase);
			if (ticksInPhase == 4)
			{
				menu.PlaySound(SoundID.MENU_Karma_Ladder_Increase_Bump);
			}
			if (num2 >= 1f)
			{
				NewPhase(KarmaLadder.Phase.Resting);
			}
		}
		else if (phase == KarmaLadder.Phase.CappedMovement)
		{
			float num2 = Mathf.InverseLerp(0f, 15f, ticksInPhase);
			scrollVel *= Mathf.Lerp(1f, 0.92f, Mathf.Pow(num2, 6f));
			if (ticksInPhase == 9)
			{
				menu.PlaySound((moveToKarma > displayKarma.x) ? SoundID.MENU_Karma_Ladder_Hit_Upper_Cap : SoundID.MENU_Karma_Ladder_Hit_Lower_Cap);
			}
			if (num2 < 0.5f)
			{
				scrollVel -= Mathf.Sign(displayKarma.x - moveToKarma) * 0.001f;
			}
			else
			{
				scrollVel += Mathf.Sign(displayKarma.x - moveToKarma) * 0.001f;
			}
			if (num2 >= 1f)
			{
				moveToKarma = displayKarma.x;
				NewPhase(KarmaLadder.Phase.Settling);
			}
		}
		else if (phase == KarmaLadder.Phase.CapBump)
		{
			scrollVel *= 0.4f;
			scroll = Mathf.Lerp(scroll, displayKarma.x, 0.2f);
			float num2 = Mathf.InverseLerp(0f, 45f, ticksInPhase);
			if (ticksInPhase == 4)
			{
				menu.PlaySound(SoundID.MENU_Karma_Ladder_Upper_Cap_Bump);
			}
			if (num2 >= 1f)
			{
				NewPhase(KarmaLadder.Phase.Resting);
			}
		}
		else
		{
			if (!(phase == KarmaLadder.Phase.ReinforceSave))
			{
				return;
			}
			reinforcementClamp = Mathf.Max(0f, reinforcementClamp - 0.05f);
			if (reinforcementClamp <= 0f)
			{
				moveToKarma = displayKarma.x;
				reinforcementDeath = 0.01f;
				NewPhase(KarmaLadder.Phase.Settling);
			}
			float num2 = Mathf.InverseLerp(0f, 35f, ticksInPhase);
			scrollVel *= 0.95f;
			if (num2 < 0.25f)
			{
				scrollVel += ((float)moveToKarma - scroll) / 400f;
				scrollVel += Mathf.Sign((float)moveToKarma - scroll) * 0.001f;
			}
			else
			{
				scrollVel += Mathf.Sign(displayKarma.x - moveToKarma) * 0.001f;
			}
			scroll = Custom.LerpMap(num2, 0.6f, 1f, scroll, displayKarma.x, 8f);
			if (ticksInPhase == 17)
			{
				menu.PlaySound(SoundID.MENU_Karma_Ladder_Reinforce_Save_Grab);
			}
			else if (ticksInPhase == 35)
			{
				menu.PlaySound(SoundID.MENU_Karma_Ladder_Reinforce_Save_Pull);
			}
			else if (ticksInPhase == 50)
			{
				menu.PlaySound(SoundID.MENU_Karma_Ladder_Reinforce_Save_Secure);
			}
			if (!(num2 > 0.2f))
			{
				return;
			}
			reinforcementCycle = Mathf.Floor(reinforcementCycle);
			if (Mathf.Abs(scroll - (float)displayKarma.x) < 0.1f)
			{
				reinforcementClamp = Mathf.Max(0f, reinforcementClamp - 0.05f);
				if (reinforcementClamp <= 0f)
				{
					moveToKarma = displayKarma.x;
					reinforcementDeath = 0.01f;
					NewPhase(KarmaLadder.Phase.Settling);
				}
			}
			else
			{
				reinforcementClamp = Mathf.InverseLerp(0.2f, 0.5f, num2);
			}
		}
	}

	public void GoToKarma(int newGoalKarma)
	{
		moveToKarma = Custom.IntClamp(newGoalKarma, minKarma, maxKarma);
		karmaIncreasing = moveToKarma > displayKarma.x;
		cappedMovement = moveToKarma < 0 || moveToKarma > displayKarmaCap;
		if (displayKarma.x == moveToKarma)
		{
			movementShown = true;
		}
		startedAnimating = true;
	}

	public void NewPhase(KarmaLadder.Phase nextPhase)
	{
		ticksInPhase = -1;
		if (nextPhase == KarmaLadder.Phase.Deselecting)
		{
			menu.PlaySound(SoundID.MENU_Karma_Ladder_Deselect);
		}
		else if (nextPhase == KarmaLadder.Phase.MovingA || nextPhase == KarmaLadder.Phase.CappedMovement || nextPhase == KarmaLadder.Phase.ReinforceSave)
		{
			menu.PlaySound(SoundID.MENU_Karma_Ladder_Start_Moving);
		}
		else if (nextPhase == KarmaLadder.Phase.Resting)
		{
			movementShown = true;
		}
		else if (nextPhase == KarmaLadder.Phase.Settling)
		{
			if (karmaIncreasing && !cappedMovement)
			{
				menu.PlaySound(SoundID.MENU_Karma_Ladder_Increase_Bump_Charge);
			}
			else if (reinforcementDeath > 0f)
			{
				menu.PlaySound(SoundID.MENU_Karma_Ladder_Reinforcement_Dissipate_A);
			}
			else
			{
				menu.PlaySound(SoundID.MENU_Karma_Ladder_Reselect);
			}
			displayKarma.x = moveToKarma;
		}
		phase = nextPhase;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		DrawPos(timeStacker);
		float num = 255f;
		float num2 = 160f;
		float a = 1f;
		float num3 = 0.5f;
		for (int i = 0; i < rippleLevels.Count; i++)
		{
			float num4 = (float)i - scroll;
			float scale = num3;
			float num5 = 0f;
			if (Mathf.Abs(num4) < 1f)
			{
				float num6 = 0f;
				num6 = ((!(num4 < 0f)) ? Custom.LerpBackEaseIn(0f, 1f, Mathf.Abs(num4)) : (0f - Custom.LerpExpEaseOut(0f, 1f, Mathf.Abs(num4))));
				scale = Mathf.Lerp(a, num3, Mathf.Abs(num6));
				num5 = num6 * num;
			}
			else if (!(Mathf.Abs(num4) < 2f))
			{
				num5 = ((!(num4 < 0f)) ? (num + num2 * (num4 - 1f)) : (0f - num - num2 * (Mathf.Abs(num4) - 1f)));
			}
			else
			{
				float num7 = 0f;
				num7 = ((!(num4 < 0f)) ? Custom.LerpCircEaseIn(0f, 1f, Mathf.Abs(num4) - 1f) : (0f - Custom.LerpCircEaseOut(0f, 1f, Mathf.Abs(num4) - 1f)));
				num5 = Mathf.Sign(num4) * num + num7 * num2;
			}
			rippleLevels[i].pos = new Vector2(0f, num5);
			rippleLevels[i].sprite.scale = scale;
			rippleLevels[i].sprite.color = RainWorld.SaturatedGold;
			rippleLevels[i].setAlpha = ladderAlpha;
			if (i < minKarma || i > maxKarma)
			{
				rippleLevels[i].setAlpha = 0f;
			}
			if (i == moveToKarma && reinforcedSymbol != null)
			{
				float b = Mathf.Lerp(lastReinforcementClamp, reinforcementClamp, timeStacker);
				float num8 = Mathf.Lerp(lastReinforcementDeath, reinforcementDeath, timeStacker);
				float num9 = Mathf.Lerp(lastReinforcementWhite, reinforcementWhite, timeStacker) * (1f - num8) * Mathf.Max(0.5f + 0.5f * Mathf.Sin((float)Math.PI * 2f * Mathf.Lerp(lastReinforcementCycle, reinforcementCycle, timeStacker)), b);
				reinforcedSymbol.pos = rippleLevels[i].pos;
				reinforcedSymbol.sprite.scale = scale;
				reinforcedSymbol.sprite.color = RainWorld.SaturatedGold;
				reinforcedSymbol.setAlpha = ladderAlpha * num9;
			}
		}
		rippleGlow.pos = Vector2.zero;
		rippleGlow.sprite.color = RainWorld.SaturatedGold;
		rippleGlow.setAlpha = ladderAlpha * glowAlpha;
	}

	public override void RemoveSprites()
	{
		base.RemoveSprites();
		for (int i = 0; i < rippleLevels.Count; i++)
		{
			rippleLevels[i].RemoveSprites();
			rippleLevels[i].UnloadFile();
		}
		rippleGlow.RemoveSprites();
		rippleGlow.UnloadFile();
		if (reinforcedSymbol != null)
		{
			reinforcedSymbol.RemoveSprites();
			reinforcedSymbol.UnloadFile();
		}
	}
}
