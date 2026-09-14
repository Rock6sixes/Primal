using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Menu;
using On;
using On.Menu;
using RWCustom;
using SlugBase.Interface;
using UnityEngine;

namespace SlugBase;

internal static class CoreHooks
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static hook_getSlugcatName _003C0_003E__SlugcatStats_getSlugcatName;

		public static hook_LoadFonts _003C1_003E__InGameTranslator_LoadFonts;

		public static hook_ctor _003C2_003E__SlugcatPageNewGame_ctor;

		public static hook_SetSlugcatColorOrder _003C3_003E__SlugcatSelectMenu_SetSlugcatColorOrder;
	}

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
		object obj = _003C_003EO._003C0_003E__SlugcatStats_getSlugcatName;
		if (obj == null)
		{
			hook_getSlugcatName val = SlugcatStats_getSlugcatName;
			_003C_003EO._003C0_003E__SlugcatStats_getSlugcatName = val;
			obj = (object)val;
		}
		SlugcatStats.getSlugcatName += (hook_getSlugcatName)obj;
		object obj2 = _003C_003EO._003C1_003E__InGameTranslator_LoadFonts;
		if (obj2 == null)
		{
			hook_LoadFonts val2 = InGameTranslator_LoadFonts;
			_003C_003EO._003C1_003E__InGameTranslator_LoadFonts = val2;
			obj2 = (object)val2;
		}
		InGameTranslator.LoadFonts += (hook_LoadFonts)obj2;
		object obj3 = _003C_003EO._003C2_003E__SlugcatPageNewGame_ctor;
		if (obj3 == null)
		{
			hook_ctor val3 = SlugcatPageNewGame_ctor;
			_003C_003EO._003C2_003E__SlugcatPageNewGame_ctor = val3;
			obj3 = (object)val3;
		}
		SlugcatPageNewGame.ctor += (hook_ctor)obj3;
		object obj4 = _003C_003EO._003C3_003E__SlugcatSelectMenu_SetSlugcatColorOrder;
		if (obj4 == null)
		{
			hook_SetSlugcatColorOrder val4 = SlugcatSelectMenu_SetSlugcatColorOrder;
			_003C_003EO._003C3_003E__SlugcatSelectMenu_SetSlugcatColorOrder = val4;
			obj4 = (object)val4;
		}
		SlugcatSelectMenu.SetSlugcatColorOrder += (hook_SetSlugcatColorOrder)obj4;
	}

	private static string SlugcatStats_getSlugcatName(orig_getSlugcatName orig, Name i)
	{
		if (SlugBaseCharacter.TryGet(i, out var character))
		{
			string displayName = character.DisplayName;
			if (displayName != null)
			{
				return displayName.StartsWith("The ", StringComparison.InvariantCultureIgnoreCase) ? displayName.Substring(4) : displayName;
			}
		}
		return orig.Invoke(i);
	}

	private static void InGameTranslator_LoadFonts(orig_LoadFonts orig, LanguageID lang, Menu menu)
	{
		orig.Invoke(lang, menu);
		ErrorList.Instance.MarkDirty();
	}

	private static void SlugcatPageNewGame_ctor(orig_ctor orig, SlugcatPageNewGame self, Menu menu, MenuObject owner, int pageIndex, Name slugcatNumber)
	{
		orig.Invoke(self, menu, owner, pageIndex, slugcatNumber);
		if (SlugBaseCharacter.TryGet(slugcatNumber, out var character))
		{
			string text = character.DisplayName ?? "Missing Name";
			string text2 = character.Description ?? "Missing Description";
			text = ((MenuObject)self).menu.Translate(text.ToUpper());
			text2 = Custom.ReplaceLineDelimeters(((MenuObject)self).menu.Translate(text2));
			int num = text2.Count((char f) => f == '\n');
			float num2 = ((num > 1) ? 30f : 0f);
			self.difficultyLabel.text = text;
			((PositionedMenuObject)self.difficultyLabel).pos = new Vector2(-1000f, ((SlugcatPage)self).imagePos.y - 249f + num2);
			self.infoLabel.text = text2;
			((PositionedMenuObject)self.infoLabel).pos = new Vector2(-1000f, ((SlugcatPage)self).imagePos.y - 249f - 60f + num2 / 2f);
		}
	}

	private static void SlugcatSelectMenu_SetSlugcatColorOrder(orig_SetSlugcatColorOrder orig, SlugcatSelectMenu self)
	{
		orig.Invoke(self);
		foreach (Name key in SlugBaseCharacter.Registry.Keys)
		{
			if (!SlugcatStats.HiddenOrUnplayableSlugcat(key))
			{
				self.slugcatColorOrder.Add(key);
			}
		}
		for (int i = 0; i < self.slugcatColorOrder.Count; i++)
		{
			if ((ExtEnum<Name>)(object)self.slugcatColorOrder[i] == (ExtEnum<Name>)(object)((MainLoopProcess)self).manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat)
			{
				self.slugcatPageIndex = i;
				break;
			}
			if ((ExtEnum<Name>)(object)self.slugcatColorOrder[i] == (ExtEnum<Name>)(object)Name.White)
			{
				self.slugcatPageIndex = i;
			}
		}
	}
}
