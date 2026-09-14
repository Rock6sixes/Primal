using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Expedition;
using IL.Menu;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using On.Expedition;
using On.Menu;
using RWCustom;
using SlugBase.Features;
using UnityEngine;

namespace SlugBase;

internal class ExpeditionHooks
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static hook_GetPlayableCharacters _003C0_003E__ExpeditionData_GetPlayableCharacters;

		public static hook_CheckUnlocked _003C1_003E__ExpeditionProgression_CheckUnlocked;

		public static Manipulator _003C2_003E__CharacterSelectPage_ctorIL;

		public static hook_ctor _003C3_003E__CharacterSelectPage_ctor;

		public static hook_Singal _003C4_003E__CharacterSelectPage_Singal;

		public static hook_UpdateSelectedSlugcat _003C5_003E__CharacterSelectPage_UpdateSelectedSlugcat;

		public static hook_GetSlugcatPortrait _003C6_003E__CharacterSelectPage_GetSlugcatPortrait;
	}

	public static int maxSlugPages = 0;

	public static int currentSlugPage = 0;

	private static readonly SceneID[] randomScenes = (SceneID[])(object)new SceneID[30]
	{
		SceneID.MainMenu_Downpour,
		SceneID.Intro_1_Tree,
		SceneID.Intro_2_Branch,
		SceneID.Intro_3_In_Tree,
		SceneID.Intro_4_Walking,
		SceneID.Intro_5_Hunting,
		SceneID.Intro_8_Climbing,
		SceneID.Intro_9_Rainy_Climb,
		SceneID.Intro_10_Fall,
		SceneID.Intro_10_5_Separation,
		SceneID.Intro_11_Drowning,
		SceneID.Landscape_CC,
		SceneID.Landscape_DS,
		SceneID.Landscape_GW,
		SceneID.Landscape_HI,
		SceneID.Landscape_LF,
		SceneID.Landscape_SB,
		SceneID.Landscape_SH,
		SceneID.Landscape_SI,
		SceneID.Landscape_SL,
		SceneID.Landscape_SU,
		SceneID.Dream_Moon_Friend,
		SceneID.Dream_Pebbles,
		new SceneID("Landscape_MS", false),
		new SceneID("Landscape_LC", false),
		new SceneID("Landscape_OE", false),
		new SceneID("Landscape_HR", false),
		new SceneID("Landscape_UG", false),
		new SceneID("Landscape_VS", false),
		new SceneID("Landscape_CL", false)
	};

	public static void Apply()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		object obj = _003C_003EO._003C0_003E__ExpeditionData_GetPlayableCharacters;
		if (obj == null)
		{
			hook_GetPlayableCharacters val = ExpeditionData_GetPlayableCharacters;
			_003C_003EO._003C0_003E__ExpeditionData_GetPlayableCharacters = val;
			obj = (object)val;
		}
		ExpeditionData.GetPlayableCharacters += (hook_GetPlayableCharacters)obj;
		object obj2 = _003C_003EO._003C1_003E__ExpeditionProgression_CheckUnlocked;
		if (obj2 == null)
		{
			hook_CheckUnlocked val2 = ExpeditionProgression_CheckUnlocked;
			_003C_003EO._003C1_003E__ExpeditionProgression_CheckUnlocked = val2;
			obj2 = (object)val2;
		}
		ExpeditionProgression.CheckUnlocked += (hook_CheckUnlocked)obj2;
		object obj3 = _003C_003EO._003C2_003E__CharacterSelectPage_ctorIL;
		if (obj3 == null)
		{
			Manipulator val3 = CharacterSelectPage_ctorIL;
			_003C_003EO._003C2_003E__CharacterSelectPage_ctorIL = val3;
			obj3 = (object)val3;
		}
		CharacterSelectPage.ctor += (Manipulator)obj3;
		object obj4 = _003C_003EO._003C3_003E__CharacterSelectPage_ctor;
		if (obj4 == null)
		{
			hook_ctor val4 = CharacterSelectPage_ctor;
			_003C_003EO._003C3_003E__CharacterSelectPage_ctor = val4;
			obj4 = (object)val4;
		}
		CharacterSelectPage.ctor += (hook_ctor)obj4;
		object obj5 = _003C_003EO._003C4_003E__CharacterSelectPage_Singal;
		if (obj5 == null)
		{
			hook_Singal val5 = CharacterSelectPage_Singal;
			_003C_003EO._003C4_003E__CharacterSelectPage_Singal = val5;
			obj5 = (object)val5;
		}
		CharacterSelectPage.Singal += (hook_Singal)obj5;
		object obj6 = _003C_003EO._003C5_003E__CharacterSelectPage_UpdateSelectedSlugcat;
		if (obj6 == null)
		{
			hook_UpdateSelectedSlugcat val6 = CharacterSelectPage_UpdateSelectedSlugcat;
			_003C_003EO._003C5_003E__CharacterSelectPage_UpdateSelectedSlugcat = val6;
			obj6 = (object)val6;
		}
		CharacterSelectPage.UpdateSelectedSlugcat += (hook_UpdateSelectedSlugcat)obj6;
		object obj7 = _003C_003EO._003C6_003E__CharacterSelectPage_GetSlugcatPortrait;
		if (obj7 == null)
		{
			hook_GetSlugcatPortrait val7 = CharacterSelectPage_GetSlugcatPortrait;
			_003C_003EO._003C6_003E__CharacterSelectPage_GetSlugcatPortrait = val7;
			obj7 = (object)val7;
		}
		CharacterSelectPage.GetSlugcatPortrait += (hook_GetSlugcatPortrait)obj7;
	}

	private static List<Name> ExpeditionData_GetPlayableCharacters(orig_GetPlayableCharacters orig)
	{
		List<Name> list = orig.Invoke();
		foreach (SlugBaseCharacter item in SlugBaseCharacter.Registry.Values.OrderBy((SlugBaseCharacter chara) => ((ExtEnumBase)chara.Name).value))
		{
			if (!((Feature<bool>)GameFeatures.ExpeditionEnabled).TryGet(item, out bool value) || value)
			{
				list.Add(item.Name);
			}
		}
		if (File.Exists(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "pandemonium.txt").ToLowerInvariant()))
		{
			if (ModManager.MSC)
			{
				list.Add(SlugcatStatsName.Sofanthiel);
				list.Add(SlugcatStatsName.Slugpup);
			}
			list.Add(Name.Night);
		}
		return list;
	}

	private static bool ExpeditionProgression_CheckUnlocked(orig_CheckUnlocked orig, ProcessManager manager, Name slugcat)
	{
		if (SlugBaseCharacter.TryGet(slugcat, out var _))
		{
			return true;
		}
		return orig.Invoke(manager, slugcat);
	}

	private static void CharacterSelectPage_ctorIL(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		if (val.TryGotoNext((MoveType)0, new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchStloc(x, 4)
		}))
		{
			val.Index += 1;
			val.Emit(OpCodes.Ldarg_0);
			val.Emit(OpCodes.Ldloc_3);
			val.Emit(OpCodes.Ldarg_1);
			val.EmitDelegate<Action<CharacterSelectPage, int, Menu>>((Action<CharacterSelectPage, int, Menu>)delegate(CharacterSelectPage self, int i, Menu menu)
			{
				//IL_0059: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Expected O, but got Unknown
				if (i > 7)
				{
					self.slugcatButtons[i] = new SelectOneButton(menu, (MenuObject)(object)self, "", "SLUG-" + i, new Vector2(415f + 110f * (float)(i - 3), 410f), new Vector2(94f, 94f), self.slugcatButtons, i);
					((MenuObject)self).subObjects.Add((MenuObject)(object)self.slugcatButtons[i]);
				}
			});
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook CharacterSelectPage_ctorIL failed!");
		}
	}

	private static void CharacterSelectPage_ctor(orig_ctor orig, CharacterSelectPage self, Menu menu, MenuObject owner, Vector2 pos)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected O, but got Unknown
		orig.Invoke(self, menu, owner, pos);
		if (ExpeditionGame.playableCharacters.Count > 8)
		{
			maxSlugPages = (ExpeditionGame.playableCharacters.Count + 7) / 8;
			currentSlugPage = 0;
			ConstructPage(self, menu);
			SymbolButton val = new SymbolButton(menu, (MenuObject)(object)self, "Big_Menu_Arrow", "CUSTOMSLUGPAGE_LEFT", new Vector2(330f, 432.5f));
			((FNode)val.symbolSprite).rotation = 270f;
			((RectangularMenuObject)val).size = new Vector2(45f, 45f);
			((RectangularMenuObject)val.roundedRect).size = ((RectangularMenuObject)val).size;
			((MenuObject)self).subObjects.Add((MenuObject)(object)val);
			SymbolButton val2 = new SymbolButton(menu, (MenuObject)(object)self, "Big_Menu_Arrow", "CUSTOMSLUGPAGE_RIGHT", new Vector2(990f, 432.5f));
			((FNode)val2.symbolSprite).rotation = 90f;
			((RectangularMenuObject)val2).size = new Vector2(45f, 45f);
			((RectangularMenuObject)val2.roundedRect).size = ((RectangularMenuObject)val2).size;
			((MenuObject)self).subObjects.Add((MenuObject)(object)val2);
			menu.selectedObject = (MenuObject)(object)self.slugcatButtons[1];
			ExpeditionMenu val3 = (ExpeditionMenu)menu;
			if (val3.currentSelection >= ExpeditionGame.playableCharacters.Count || val3.currentSelection < 0)
			{
				val3.currentSelection = 1;
			}
		}
	}

	private static void ConstructPage(CharacterSelectPage self, Menu menu)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected O, but got Unknown
		for (int i = 0; i < ExpeditionGame.playableCharacters.Count; i++)
		{
			((MenuObject)self.slugcatButtons[i]).RemoveSprites();
			((MenuObject)self).RemoveSubObject((MenuObject)(object)self.slugcatButtons[i]);
			((PositionedMenuObject)self.slugcatButtons[i]).pos = new Vector2(10000f, 10000f);
		}
		for (int j = currentSlugPage * 8; j < Mathf.Min((currentSlugPage + 1) * 8, ExpeditionGame.playableCharacters.Count); j++)
		{
			bool greyedOut = !ExpeditionGame.unlockedExpeditionSlugcats.Contains(ExpeditionGame.playableCharacters[j]);
			int num = j - 8 * currentSlugPage;
			if (j < 3 + 8 * currentSlugPage)
			{
				self.slugcatButtons[j] = new SelectOneButton(menu, (MenuObject)(object)self, "", "SLUG-" + j, new Vector2(525f + 110f * (float)num, 525f), new Vector2(94f, 94f), self.slugcatButtons, j);
				((MenuObject)self).subObjects.Add((MenuObject)(object)self.slugcatButtons[j]);
			}
			else if (j >= 3 + 8 * currentSlugPage && j <= 7 + 8 * currentSlugPage)
			{
				self.slugcatButtons[j] = new SelectOneButton(menu, (MenuObject)(object)self, "", "SLUG-" + j, new Vector2(415f + 110f * (float)(num - 3), 410f), new Vector2(94f, 94f), self.slugcatButtons, j);
				((MenuObject)self).subObjects.Add((MenuObject)(object)self.slugcatButtons[j]);
			}
			((ButtonTemplate)self.slugcatButtons[j]).buttonBehav.greyedOut = greyedOut;
		}
		self.ReloadSlugcatPortraits();
	}

	private static void CharacterSelectPage_Singal(orig_Singal orig, CharacterSelectPage self, MenuObject sender, string message)
	{
		orig.Invoke(self, sender, message);
		if (message == "CUSTOMSLUGPAGE_LEFT")
		{
			currentSlugPage--;
			if (currentSlugPage < 0)
			{
				currentSlugPage = maxSlugPages - 1;
			}
			ConstructPage(self, ((MenuObject)self).menu);
			((MenuObject)self).menu.PlaySound(SoundID.MENU_Checkbox_Check);
		}
		if (message == "CUSTOMSLUGPAGE_RIGHT")
		{
			currentSlugPage++;
			if (currentSlugPage > maxSlugPages - 1)
			{
				currentSlugPage = 0;
			}
			ConstructPage(self, ((MenuObject)self).menu);
			((MenuObject)self).menu.PlaySound(SoundID.MENU_Checkbox_Check);
		}
	}

	private static void CharacterSelectPage_UpdateSelectedSlugcat(orig_UpdateSelectedSlugcat orig, CharacterSelectPage self, int num)
	{
		orig.Invoke(self, num);
		if (string.IsNullOrEmpty(self.slugcatName.text) && string.IsNullOrEmpty(self.slugcatDescription.text) && num >= 0 && num < ExpeditionGame.playableCharacters.Count)
		{
			Name val = ExpeditionGame.playableCharacters[num];
			string text;
			string text2;
			if (SlugBaseCharacter.TryGet(val, out var character))
			{
				text = character.DisplayName;
				text2 = ((!((Feature<string>)GameFeatures.ExpeditionDescription).TryGet(character, out string value)) ? character.Description : value);
			}
			else if ((ExtEnum<Name>)(object)val == (ExtEnum<Name>)(object)Name.Night)
			{
				text = "NIGHTCAT";
				text2 = "A mystery to everyone, with no unique skills, The Nightcat embarks <LINE>on an expedition to find purpose";
			}
			else if ((ExtEnum<Name>)(object)val == (ExtEnum<Name>)(object)SlugcatStatsName.Sofanthiel)
			{
				text = ((UnityEngine.Random.value < 0.33f) ? "SOFANTHIEL" : ((UnityEngine.Random.value < 0.33f) ? "INV" : ((UnityEngine.Random.value < 0.33f) ? "ENOT" : "???")));
				text2 = "A freak of nature, this creature not meant to exist, or prosper in any meaningful way<LINE>travels alone attempting to prove itself to the world";
			}
			else if ((ExtEnum<Name>)(object)val == (ExtEnum<Name>)(object)SlugcatStatsName.Slugpup)
			{
				text = "SLUGPUP";
				text2 = "A small lost and curious child, it's unrelenting bravery drives it to<LINE>complete harder and harder tasks, will you help?";
			}
			else
			{
				text = null;
				text2 = null;
			}
			if (text != null)
			{
				self.slugcatName.text = ((MenuObject)self).menu.Translate(text).ToUpper();
				self.slugcatDescription.text = ((MenuObject)self).menu.Translate(text2).Replace("<LINE>", Environment.NewLine);
				self.slugcatScene = randomScenes[UnityEngine.Random.Range(0, randomScenes.Length - ((!ModManager.MSC) ? 7 : 0))];
			}
		}
	}

	private static MenuIllustration CharacterSelectPage_GetSlugcatPortrait(orig_GetSlugcatPortrait orig, CharacterSelectPage self, Name slugcat, Vector2 pos)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected O, but got Unknown
		MenuIllustration val = orig.Invoke(self, slugcat, pos);
		if (SlugBaseCharacter.TryGet(slugcat, out var character))
		{
			string text = "illustrations";
			string text2 = "multiplayerportrait30";
			for (int i = -1; i < 4; i++)
			{
				string text3 = "multiplayerportrait" + ((i == -1) ? 4 : i).ToString() + "1-" + character.Name;
				if (File.Exists(AssetManager.ResolveFilePath(text + Path.DirectorySeparatorChar + text3 + ".png")))
				{
					text2 = text3;
					break;
				}
			}
			val = new MenuIllustration(((MenuObject)self).menu, (MenuObject)(object)self, text, text2, pos, true, true);
		}
		else if ((ExtEnum<Name>)(object)slugcat == (ExtEnum<Name>)(object)SlugcatStatsName.Sofanthiel || (ExtEnum<Name>)(object)slugcat == (ExtEnum<Name>)(object)SlugcatStatsName.Slugpup || (ExtEnum<Name>)(object)slugcat == (ExtEnum<Name>)(object)Name.Night)
		{
			string text4 = "illustrations";
			string text5 = "multiplayerportrait31";
			bool flag = false;
			if ((ExtEnum<Name>)(object)slugcat == (ExtEnum<Name>)(object)SlugcatStatsName.Sofanthiel)
			{
				int num = UnityEngine.Random.Range(1, 13);
				text4 = "content";
				text5 = ((UnityEngine.Random.value < 0.05f) ? "blush_001" : ("sm" + ((num == 11) ? 15 : num)));
				flag = true;
			}
			else if ((ExtEnum<Name>)(object)slugcat == (ExtEnum<Name>)(object)SlugcatStatsName.Slugpup)
			{
				text5 = "slugcatportraitalive1";
			}
			val = new MenuIllustration(((MenuObject)self).menu, (MenuObject)(object)self, text4, text5, pos, true, true);
			((FNode)val.sprite).scale = (flag ? 0.2f : 1f);
		}
		return val;
	}
}
