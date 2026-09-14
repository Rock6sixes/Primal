using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using IL.JollyCoop.JollyMenu;
using JollyCoop.JollyMenu;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using On;
using On.JollyCoop.JollyMenu;
using RWCustom;
using SlugBase.DataTypes;
using SlugBase.Features;
using UnityEngine;

namespace SlugBase;

internal static class JollyCoopHooks
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static hook_ValueOfSlider _003C0_003E__ColorChangeDialog_ValueOfSlider;

		public static hook_SliderSetValue _003C1_003E__ColorChangeDialog_SliderSetValue;

		public static hook_LoadJollyColorsFromOptions _003C2_003E__PlayerGraphics_LoadJollyColorsFromOptions;

		public static hook_ApplyOption _003C3_003E__Options_ApplyOption;

		public static hook_ToString _003C4_003E__Options_ToString;

		public static hook_ctor _003C5_003E__On_ColorChangeDialog_ctor;

		public static hook_ActualSavingColor _003C6_003E__ColorChangeDialog_ActualSavingColor;

		public static hook_Singal _003C7_003E__ColorChangeDialog_Singal;

		public static hook_AddSlider _003C8_003E__ColorChangeDialog_AddSlider;

		public static Manipulator _003C9_003E__IL_ColorChangeDialog_ctor;
	}

	private static readonly ConditionalWeakTable<ColorChangeDialog, List<ColorSlider>> _extraSliders = new ConditionalWeakTable<ColorChangeDialog, List<ColorSlider>>();

	private static readonly ConditionalWeakTable<JollyPlayerOptions, List<Color>> _extraColors = new ConditionalWeakTable<JollyPlayerOptions, List<Color>>();

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
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		object obj = _003C_003EO._003C0_003E__ColorChangeDialog_ValueOfSlider;
		if (obj == null)
		{
			hook_ValueOfSlider val = ColorChangeDialog_ValueOfSlider;
			_003C_003EO._003C0_003E__ColorChangeDialog_ValueOfSlider = val;
			obj = (object)val;
		}
		ColorChangeDialog.ValueOfSlider += (hook_ValueOfSlider)obj;
		object obj2 = _003C_003EO._003C1_003E__ColorChangeDialog_SliderSetValue;
		if (obj2 == null)
		{
			hook_SliderSetValue val2 = ColorChangeDialog_SliderSetValue;
			_003C_003EO._003C1_003E__ColorChangeDialog_SliderSetValue = val2;
			obj2 = (object)val2;
		}
		ColorChangeDialog.SliderSetValue += (hook_SliderSetValue)obj2;
		object obj3 = _003C_003EO._003C2_003E__PlayerGraphics_LoadJollyColorsFromOptions;
		if (obj3 == null)
		{
			hook_LoadJollyColorsFromOptions val3 = PlayerGraphics_LoadJollyColorsFromOptions;
			_003C_003EO._003C2_003E__PlayerGraphics_LoadJollyColorsFromOptions = val3;
			obj3 = (object)val3;
		}
		PlayerGraphics.LoadJollyColorsFromOptions += (hook_LoadJollyColorsFromOptions)obj3;
		object obj4 = _003C_003EO._003C3_003E__Options_ApplyOption;
		if (obj4 == null)
		{
			hook_ApplyOption val4 = Options_ApplyOption;
			_003C_003EO._003C3_003E__Options_ApplyOption = val4;
			obj4 = (object)val4;
		}
		Options.ApplyOption += (hook_ApplyOption)obj4;
		object obj5 = _003C_003EO._003C4_003E__Options_ToString;
		if (obj5 == null)
		{
			hook_ToString val5 = Options_ToString;
			_003C_003EO._003C4_003E__Options_ToString = val5;
			obj5 = (object)val5;
		}
		Options.ToString += (hook_ToString)obj5;
		object obj6 = _003C_003EO._003C5_003E__On_ColorChangeDialog_ctor;
		if (obj6 == null)
		{
			hook_ctor val6 = On_ColorChangeDialog_ctor;
			_003C_003EO._003C5_003E__On_ColorChangeDialog_ctor = val6;
			obj6 = (object)val6;
		}
		ColorChangeDialog.ctor += (hook_ctor)obj6;
		object obj7 = _003C_003EO._003C6_003E__ColorChangeDialog_ActualSavingColor;
		if (obj7 == null)
		{
			hook_ActualSavingColor val7 = ColorChangeDialog_ActualSavingColor;
			_003C_003EO._003C6_003E__ColorChangeDialog_ActualSavingColor = val7;
			obj7 = (object)val7;
		}
		ColorChangeDialog.ActualSavingColor += (hook_ActualSavingColor)obj7;
		object obj8 = _003C_003EO._003C7_003E__ColorChangeDialog_Singal;
		if (obj8 == null)
		{
			hook_Singal val8 = ColorChangeDialog_Singal;
			_003C_003EO._003C7_003E__ColorChangeDialog_Singal = val8;
			obj8 = (object)val8;
		}
		ColorChangeDialog.Singal += (hook_Singal)obj8;
		object obj9 = _003C_003EO._003C8_003E__ColorChangeDialog_AddSlider;
		if (obj9 == null)
		{
			hook_AddSlider val9 = ColorChangeDialog_AddSlider;
			_003C_003EO._003C8_003E__ColorChangeDialog_AddSlider = val9;
			obj9 = (object)val9;
		}
		ColorChangeDialog.AddSlider += (hook_AddSlider)obj9;
		object obj10 = _003C_003EO._003C9_003E__IL_ColorChangeDialog_ctor;
		if (obj10 == null)
		{
			Manipulator val10 = IL_ColorChangeDialog_ctor;
			_003C_003EO._003C9_003E__IL_ColorChangeDialog_ctor = val10;
			obj10 = (object)val10;
		}
		ColorChangeDialog.ctor += (Manipulator)obj10;
	}

	private static void ColorChangeDialog_SliderSetValue(orig_SliderSetValue orig, ColorChangeDialog self, Slider slider, float f)
	{
		if (((ExtEnumBase)slider.ID).value.Contains("JOLLY") && _extraSliders.TryGetValue(self, out var value))
		{
			string[] array = ((ExtEnumBase)slider.ID).value.Split('_');
			if (int.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result) && result - 3 >= 0 && result - 3 < value.Count)
			{
				ColorSlider val = value[result - 3];
				switch (array[2])
				{
				case "HUE":
					val.hslColor.hue = f;
					break;
				case "SAT":
					val.hslColor.saturation = f;
					break;
				case "LIT":
					val.hslColor.lightness = f;
					break;
				}
				val.HSL2RGB();
				((Menu)self).selectedObject = (MenuObject)(object)slider;
				return;
			}
		}
		orig.Invoke(self, slider, f);
	}

	private static float ColorChangeDialog_ValueOfSlider(orig_ValueOfSlider orig, ColorChangeDialog self, Slider slider)
	{
		if (((ExtEnumBase)slider.ID).value.Contains("JOLLY") && _extraSliders.TryGetValue(self, out var value))
		{
			string[] array = ((ExtEnumBase)slider.ID).value.Split('_');
			if (int.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var result) && result - 3 >= 0 && result - 3 < value.Count)
			{
				ColorSlider val = value[result - 3];
				switch (array[2])
				{
				case "HUE":
					return val.hslColor.hue;
				case "SAT":
					return val.hslColor.saturation;
				case "LIT":
					return val.hslColor.lightness;
				}
			}
		}
		return orig.Invoke(self, slider);
	}

	private static void PlayerGraphics_LoadJollyColorsFromOptions(orig_LoadJollyColorsFromOptions orig, int playerNumber)
	{
		orig.Invoke(playerNumber);
		JollyPlayerOptions playerOptions = Custom.rainWorld.options.jollyPlayerOptionsArray[playerNumber];
		List<Color> extraColors = GetExtraColors(playerOptions);
		if (extraColors.Count > 0 && PlayerGraphics.jollyColors[playerNumber].Length < extraColors.Count + 3)
		{
			Array.Resize(ref PlayerGraphics.jollyColors[playerNumber], extraColors.Count + 3);
		}
		for (int i = 0; i < extraColors.Count; i++)
		{
			PlayerGraphics.jollyColors[playerNumber][i + 3] = extraColors[i];
		}
	}

	private static bool Options_ApplyOption(orig_ApplyOption orig, Options self, string[] splt2)
	{
		try
		{
			if (splt2[0] == "SlugBaseCustomJollyColors" && ModManager.JollyCoop)
			{
				string[] array = Regex.Split(splt2[1], "<optC>");
				for (int i = 0; i < self.jollyPlayerOptionsArray.Length && i < array.Length; i++)
				{
					if (!(array[i] == ""))
					{
						List<Color> value = Utils.StringsToColors(array[i].Split(','));
						_extraColors.Remove(self.jollyPlayerOptionsArray[i]);
						_extraColors.Add(self.jollyPlayerOptionsArray[i], value);
					}
				}
				return false;
			}
		}
		catch (Exception ex)
		{
			SlugBasePlugin.Logger.LogError("Failed to load SlugBase's Jolly Coop colors!\n" + ex);
		}
		return orig.Invoke(self, splt2);
	}

	private static string Options_ToString(orig_ToString orig, Options self)
	{
		string text = "";
		if (ModManager.JollyCoop)
		{
			text = "SlugBaseCustomJollyColors<optB>";
			for (int i = 0; i < self.jollyPlayerOptionsArray.Length; i++)
			{
				List<Color> extraColors = GetExtraColors(self.jollyPlayerOptionsArray[i]);
				text = text + string.Join(",", Utils.ColorsToStrings(extraColors)) + "<optC>";
			}
			text += "<optA>";
		}
		return orig.Invoke(self) + text;
	}

	private static void On_ColorChangeDialog_ctor(orig_ctor orig, ColorChangeDialog self, JollySetupDialog jollyDialog, Name playerName, int playerNumber, ProcessManager manager, List<string> names)
	{
		orig.Invoke(self, jollyDialog, playerName, playerNumber, manager, names);
		if (SlugBaseCharacter.TryGet(playerName, out var character) && ((Feature<ColorSlot[]>)PlayerFeatures.CustomColors).TryGet(character, out ColorSlot[] value) && value.Length > 3)
		{
			ColorSlot[] array = value.Skip(3).ToArray();
			List<Color> extraColors = GetExtraColors(self.JollyOptions);
			List<ColorSlider> list = new List<ColorSlider>();
			_extraSliders.Add(self, list);
			while (extraColors.Count < array.Length)
			{
				extraColors.Add(array[extraColors.Count].Default);
			}
			for (int i = 0; i < array.Length; i++)
			{
				ColorSlider val = null;
				self.AddSlider(ref val, ((Menu)jollyDialog).Translate(array[i].Name), new Vector2(135f + 140f * (float)(i % 3), 90f - 100f * Mathf.Max((i - 2) % 3, 0f)), self.playerNumber, i + 3);
				list.Add(val);
				((MenuObject)val.litSlider).nextSelectable[3] = (MenuObject)(object)((DialogNotify)self).okButton;
				((MenuObject)((DialogNotify)self).okButton).nextSelectable[i % 3] = (MenuObject)(object)val.litSlider;
				ColorSlider val2 = (ColorSlider)(i switch
				{
					2 => self.unique, 
					1 => self.face, 
					0 => self.body, 
					_ => list[i - 3], 
				});
				((MenuObject)val.hueSlider).nextSelectable[1] = (MenuObject)(object)val2.litSlider;
				((MenuObject)val2.litSlider).nextSelectable[3] = (MenuObject)(object)val.hueSlider;
				list[i].color = extraColors[i];
				list[i].RGB2HSL();
			}
			SlugBasePlugin.Logger.LogDebug($"{list.Count} sliders, {extraColors.Count} colors, {array.Length} color slots, {value.Length} all colors");
		}
	}

	private static void ColorChangeDialog_ActualSavingColor(orig_ActualSavingColor orig, ColorChangeDialog self)
	{
		orig.Invoke(self);
		if (_extraSliders.TryGetValue(self, out var value))
		{
			List<Color> extraColors = GetExtraColors(self.JollyOptions);
			for (int i = 0; i < value.Count && i < extraColors.Count; i++)
			{
				extraColors[i] = value[i].color;
			}
		}
	}

	private static void ColorChangeDialog_Singal(orig_Singal orig, ColorChangeDialog self, MenuObject sender, string message)
	{
		orig.Invoke(self, sender, message);
		if (!message.StartsWith("RESET_COLOR_DIALOG_") || !SlugBaseCharacter.TryGet(self.playerClass, out var character) || !((Feature<ColorSlot[]>)PlayerFeatures.CustomColors).TryGet(character, out ColorSlot[] value) || value.Length <= 3 || !_extraSliders.TryGetValue(self, out var value2))
		{
			return;
		}
		ColorSlot[] array = value.Skip(3).ToArray();
		List<Color> extraColors = GetExtraColors(self.JollyOptions);
		for (int i = 0; i < value2.Count; i++)
		{
			if (i >= extraColors.Count)
			{
				extraColors.Add(Color.white);
			}
			extraColors[i] = ((i < array.Length) ? array[i].Default : Color.white);
			value2[i].color = extraColors[i];
			value2[i].RGB2HSL();
		}
	}

	private static void ColorChangeDialog_AddSlider(orig_AddSlider orig, ColorChangeDialog self, ref ColorSlider slider, string labelString, Vector2 position, int playerNumber, int bodyPart)
	{
		if (SlugBaseCharacter.TryGet(self.playerClass, out var character) && ((Feature<ColorSlot[]>)PlayerFeatures.CustomColors).TryGet(character, out ColorSlot[] value) && value.Length > 3)
		{
			position.y += 50f;
			if (value.Length > 6)
			{
				position.y += 50f;
			}
		}
		orig.Invoke(self, ref slider, labelString, position, playerNumber, bodyPart);
	}

	private static void IL_ColorChangeDialog_ctor(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		if (val.TryGotoNext((MoveType)0, new Func<Instruction, bool>[1]
		{
			(Instruction i) => ILPatternMatchingExt.MatchNewobj<Vector2>(i)
		}))
		{
			val.Emit(OpCodes.Ldarg_2);
			val.EmitDelegate<Func<float, Name, float>>((Func<float, Name, float>)delegate(float height, Name name)
			{
				if (SlugBaseCharacter.TryGet(name, out var character2) && ((Feature<ColorSlot[]>)PlayerFeatures.CustomColors).TryGet(character2, out ColorSlot[] value2) && value2.Length > 3)
				{
					height += 200f;
					if (value2.Length > 6)
					{
						height += 200f;
					}
				}
				return height;
			});
			int index = val.Index;
			val.Index = index + 1;
			if (val.TryGotoNext((MoveType)0, new Func<Instruction, bool>[1]
			{
				(Instruction i) => ILPatternMatchingExt.MatchNewobj<Vector2>(i)
			}))
			{
				val.Emit(OpCodes.Ldarg_2);
				val.EmitDelegate<Func<float, Name, float>>((Func<float, Name, float>)delegate(float resetY, Name playerClass)
				{
					if (SlugBaseCharacter.TryGet(playerClass, out var character) && ((Feature<ColorSlot[]>)PlayerFeatures.CustomColors).TryGet(character, out ColorSlot[] value) && value.Length > 3)
					{
						resetY += 100f;
						if (value.Length > 6)
						{
							resetY += 100f;
						}
					}
					return resetY;
				});
			}
			else
			{
				SlugBasePlugin.Logger.LogError("IL hook IL_ColorChangeDialog_ctor, move reset button, failed!");
			}
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook IL_ColorChangeDialog_ctor, resize box, failed!");
		}
	}

	private static List<Color> GetExtraColors(JollyPlayerOptions playerOptions)
	{
		if (!_extraColors.TryGetValue(playerOptions, out var value))
		{
			_extraColors.Add(playerOptions, value = new List<Color>());
		}
		return value;
	}
}
