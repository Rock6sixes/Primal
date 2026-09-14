using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using IL;
using IL.Menu;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using On;
using On.Menu;
using RWCustom;
using SlugBase.Assets;
using SlugBase.DataTypes;
using SlugBase.SaveData;
using UnityEngine;

namespace SlugBase.Features;

internal static class FeatureHooks
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static hook_PreSwitchMainProcess _003C0_003E__ProcessManager_PreSwitchMainProcess;

		public static Manipulator _003C1_003E__DreamsState_StaticEndOfCycleProgress;

		public static hook_SlugcatTimelineOrder _003C2_003E__SlugcatStats_SlugcatTimelineOrder;

		public static hook_SlugcatCanMaul _003C3_003E__SlugcatStats_SlugcatCanMaul;

		public static hook_CanMaulCreature _003C4_003E__Player_CanMaulCreature;

		public static Manipulator _003C5_003E__Player_GrabUpdate;

		public static hook_ctor _003C6_003E__Player_ctor;

		public static Manipulator _003C7_003E__DreamScreen_Update;

		public static hook_ctor _003C8_003E__MenuScene_ctor;

		public static hook_DefaultBodyPartColorHex _003C9_003E__PlayerGraphics_DefaultBodyPartColorHex;

		public static hook_ColoredBodyPartList _003C10_003E__PlayerGraphics_ColoredBodyPartList;

		public static hook_DrawSprites _003C11_003E__PlayerGraphics_DrawSprites;

		public static Manipulator _003C12_003E__PlayerGraphics_ApplyPalette;

		public static hook_CanEatMeat _003C13_003E__Player_CanEatMeat;

		public static Manipulator _003C14_003E__Player_EatMeatUpdate;

		public static hook_ObjectEaten _003C15_003E__Player_ObjectEaten;

		public static hook_NourishmentOfObjectEaten _003C16_003E__SlugcatStats_NourishmentOfObjectEaten;

		public static Manipulator _003C17_003E__SlugcatPage_AddImage;

		public static hook_LoadDefaultCommunityAlignments _003C18_003E__CreatureCommunities_LoadDefaultCommunityAlignments;

		public static hook_LikeOfPlayer _003C19_003E__CreatureCommunities_LikeOfPlayer;

		public static hook_SlugcatFoodMeter _003C20_003E__SlugcatStats_SlugcatFoodMeter;

		public static hook_CanUseUnlockedGates _003C21_003E__DeathPersistentSaveData_CanUseUnlockedGates;

		public static hook_ctor _003C22_003E__RainCycle_ctor;

		public static hook_AutoGrabBatflys _003C23_003E__SlugcatStats_AutoGrabBatflys;

		public static hook_ctor _003C24_003E__SlugcatStats_ctor;

		public static hook_SlugcatStartingKarma _003C25_003E__SlugcatStats_SlugcatStartingKarma;

		public static hook_ctor _003C26_003E__SaveState_ctor;

		public static hook_GeneratePopulation _003C27_003E__WorldLoader_GeneratePopulation;

		public static hook_SetAsPlayerGuide _003C28_003E__OverseerAbstractAI_SetAsPlayerGuide;

		public static hook_OverseerSpawnConditions _003C29_003E__WorldLoader_OverseerSpawnConditions;

		public static hook_DefaultSlugcatColor _003C30_003E__PlayerGraphics_DefaultSlugcatColor;

		public static hook_GetStoryDenPosition _003C31_003E__SaveState_GetStoryDenPosition;

		public static Manipulator _003C32_003E__IntroRoll_ctor;

		public static EventHandler<SlugBaseCharacter.RefreshEventArgs> _003C33_003E__Refreshed;
	}

	private static bool _generatingPopulation;

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
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Expected O, but got Unknown
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Expected O, but got Unknown
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Expected O, but got Unknown
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Expected O, but got Unknown
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Expected O, but got Unknown
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Expected O, but got Unknown
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Expected O, but got Unknown
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Expected O, but got Unknown
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Expected O, but got Unknown
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Expected O, but got Unknown
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Expected O, but got Unknown
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Expected O, but got Unknown
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Expected O, but got Unknown
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Expected O, but got Unknown
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Expected O, but got Unknown
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Expected O, but got Unknown
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Expected O, but got Unknown
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Expected O, but got Unknown
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Expected O, but got Unknown
		object obj = _003C_003EO._003C0_003E__ProcessManager_PreSwitchMainProcess;
		if (obj == null)
		{
			hook_PreSwitchMainProcess val = ProcessManager_PreSwitchMainProcess;
			_003C_003EO._003C0_003E__ProcessManager_PreSwitchMainProcess = val;
			obj = (object)val;
		}
		ProcessManager.PreSwitchMainProcess += (hook_PreSwitchMainProcess)obj;
		object obj2 = _003C_003EO._003C1_003E__DreamsState_StaticEndOfCycleProgress;
		if (obj2 == null)
		{
			Manipulator val2 = DreamsState_StaticEndOfCycleProgress;
			_003C_003EO._003C1_003E__DreamsState_StaticEndOfCycleProgress = val2;
			obj2 = (object)val2;
		}
		DreamsState.StaticEndOfCycleProgress += (Manipulator)obj2;
		object obj3 = _003C_003EO._003C2_003E__SlugcatStats_SlugcatTimelineOrder;
		if (obj3 == null)
		{
			hook_SlugcatTimelineOrder val3 = SlugcatStats_SlugcatTimelineOrder;
			_003C_003EO._003C2_003E__SlugcatStats_SlugcatTimelineOrder = val3;
			obj3 = (object)val3;
		}
		SlugcatStats.SlugcatTimelineOrder += (hook_SlugcatTimelineOrder)obj3;
		object obj4 = _003C_003EO._003C3_003E__SlugcatStats_SlugcatCanMaul;
		if (obj4 == null)
		{
			hook_SlugcatCanMaul val4 = SlugcatStats_SlugcatCanMaul;
			_003C_003EO._003C3_003E__SlugcatStats_SlugcatCanMaul = val4;
			obj4 = (object)val4;
		}
		SlugcatStats.SlugcatCanMaul += (hook_SlugcatCanMaul)obj4;
		object obj5 = _003C_003EO._003C4_003E__Player_CanMaulCreature;
		if (obj5 == null)
		{
			hook_CanMaulCreature val5 = Player_CanMaulCreature;
			_003C_003EO._003C4_003E__Player_CanMaulCreature = val5;
			obj5 = (object)val5;
		}
		Player.CanMaulCreature += (hook_CanMaulCreature)obj5;
		object obj6 = _003C_003EO._003C5_003E__Player_GrabUpdate;
		if (obj6 == null)
		{
			Manipulator val6 = Player_GrabUpdate;
			_003C_003EO._003C5_003E__Player_GrabUpdate = val6;
			obj6 = (object)val6;
		}
		Player.GrabUpdate += (Manipulator)obj6;
		object obj7 = _003C_003EO._003C6_003E__Player_ctor;
		if (obj7 == null)
		{
			hook_ctor val7 = Player_ctor;
			_003C_003EO._003C6_003E__Player_ctor = val7;
			obj7 = (object)val7;
		}
		Player.ctor += (hook_ctor)obj7;
		object obj8 = _003C_003EO._003C7_003E__DreamScreen_Update;
		if (obj8 == null)
		{
			Manipulator val8 = DreamScreen_Update;
			_003C_003EO._003C7_003E__DreamScreen_Update = val8;
			obj8 = (object)val8;
		}
		DreamScreen.Update += (Manipulator)obj8;
		object obj9 = _003C_003EO._003C8_003E__MenuScene_ctor;
		if (obj9 == null)
		{
			hook_ctor val9 = MenuScene_ctor;
			_003C_003EO._003C8_003E__MenuScene_ctor = val9;
			obj9 = (object)val9;
		}
		MenuScene.ctor += (hook_ctor)obj9;
		object obj10 = _003C_003EO._003C9_003E__PlayerGraphics_DefaultBodyPartColorHex;
		if (obj10 == null)
		{
			hook_DefaultBodyPartColorHex val10 = PlayerGraphics_DefaultBodyPartColorHex;
			_003C_003EO._003C9_003E__PlayerGraphics_DefaultBodyPartColorHex = val10;
			obj10 = (object)val10;
		}
		PlayerGraphics.DefaultBodyPartColorHex += (hook_DefaultBodyPartColorHex)obj10;
		object obj11 = _003C_003EO._003C10_003E__PlayerGraphics_ColoredBodyPartList;
		if (obj11 == null)
		{
			hook_ColoredBodyPartList val11 = PlayerGraphics_ColoredBodyPartList;
			_003C_003EO._003C10_003E__PlayerGraphics_ColoredBodyPartList = val11;
			obj11 = (object)val11;
		}
		PlayerGraphics.ColoredBodyPartList += (hook_ColoredBodyPartList)obj11;
		object obj12 = _003C_003EO._003C11_003E__PlayerGraphics_DrawSprites;
		if (obj12 == null)
		{
			hook_DrawSprites val12 = PlayerGraphics_DrawSprites;
			_003C_003EO._003C11_003E__PlayerGraphics_DrawSprites = val12;
			obj12 = (object)val12;
		}
		PlayerGraphics.DrawSprites += (hook_DrawSprites)obj12;
		object obj13 = _003C_003EO._003C12_003E__PlayerGraphics_ApplyPalette;
		if (obj13 == null)
		{
			Manipulator val13 = PlayerGraphics_ApplyPalette;
			_003C_003EO._003C12_003E__PlayerGraphics_ApplyPalette = val13;
			obj13 = (object)val13;
		}
		PlayerGraphics.ApplyPalette += (Manipulator)obj13;
		object obj14 = _003C_003EO._003C13_003E__Player_CanEatMeat;
		if (obj14 == null)
		{
			hook_CanEatMeat val14 = Player_CanEatMeat;
			_003C_003EO._003C13_003E__Player_CanEatMeat = val14;
			obj14 = (object)val14;
		}
		Player.CanEatMeat += (hook_CanEatMeat)obj14;
		object obj15 = _003C_003EO._003C14_003E__Player_EatMeatUpdate;
		if (obj15 == null)
		{
			Manipulator val15 = Player_EatMeatUpdate;
			_003C_003EO._003C14_003E__Player_EatMeatUpdate = val15;
			obj15 = (object)val15;
		}
		Player.EatMeatUpdate += (Manipulator)obj15;
		object obj16 = _003C_003EO._003C15_003E__Player_ObjectEaten;
		if (obj16 == null)
		{
			hook_ObjectEaten val16 = Player_ObjectEaten;
			_003C_003EO._003C15_003E__Player_ObjectEaten = val16;
			obj16 = (object)val16;
		}
		Player.ObjectEaten += (hook_ObjectEaten)obj16;
		object obj17 = _003C_003EO._003C16_003E__SlugcatStats_NourishmentOfObjectEaten;
		if (obj17 == null)
		{
			hook_NourishmentOfObjectEaten val17 = SlugcatStats_NourishmentOfObjectEaten;
			_003C_003EO._003C16_003E__SlugcatStats_NourishmentOfObjectEaten = val17;
			obj17 = (object)val17;
		}
		SlugcatStats.NourishmentOfObjectEaten += (hook_NourishmentOfObjectEaten)obj17;
		object obj18 = _003C_003EO._003C17_003E__SlugcatPage_AddImage;
		if (obj18 == null)
		{
			Manipulator val18 = SlugcatPage_AddImage;
			_003C_003EO._003C17_003E__SlugcatPage_AddImage = val18;
			obj18 = (object)val18;
		}
		SlugcatPage.AddImage += (Manipulator)obj18;
		object obj19 = _003C_003EO._003C18_003E__CreatureCommunities_LoadDefaultCommunityAlignments;
		if (obj19 == null)
		{
			hook_LoadDefaultCommunityAlignments val19 = CreatureCommunities_LoadDefaultCommunityAlignments;
			_003C_003EO._003C18_003E__CreatureCommunities_LoadDefaultCommunityAlignments = val19;
			obj19 = (object)val19;
		}
		CreatureCommunities.LoadDefaultCommunityAlignments += (hook_LoadDefaultCommunityAlignments)obj19;
		object obj20 = _003C_003EO._003C19_003E__CreatureCommunities_LikeOfPlayer;
		if (obj20 == null)
		{
			hook_LikeOfPlayer val20 = CreatureCommunities_LikeOfPlayer;
			_003C_003EO._003C19_003E__CreatureCommunities_LikeOfPlayer = val20;
			obj20 = (object)val20;
		}
		CreatureCommunities.LikeOfPlayer += (hook_LikeOfPlayer)obj20;
		object obj21 = _003C_003EO._003C20_003E__SlugcatStats_SlugcatFoodMeter;
		if (obj21 == null)
		{
			hook_SlugcatFoodMeter val21 = SlugcatStats_SlugcatFoodMeter;
			_003C_003EO._003C20_003E__SlugcatStats_SlugcatFoodMeter = val21;
			obj21 = (object)val21;
		}
		SlugcatStats.SlugcatFoodMeter += (hook_SlugcatFoodMeter)obj21;
		object obj22 = _003C_003EO._003C21_003E__DeathPersistentSaveData_CanUseUnlockedGates;
		if (obj22 == null)
		{
			hook_CanUseUnlockedGates val22 = DeathPersistentSaveData_CanUseUnlockedGates;
			_003C_003EO._003C21_003E__DeathPersistentSaveData_CanUseUnlockedGates = val22;
			obj22 = (object)val22;
		}
		DeathPersistentSaveData.CanUseUnlockedGates += (hook_CanUseUnlockedGates)obj22;
		object obj23 = _003C_003EO._003C22_003E__RainCycle_ctor;
		if (obj23 == null)
		{
			hook_ctor val23 = RainCycle_ctor;
			_003C_003EO._003C22_003E__RainCycle_ctor = val23;
			obj23 = (object)val23;
		}
		RainCycle.ctor += (hook_ctor)obj23;
		object obj24 = _003C_003EO._003C23_003E__SlugcatStats_AutoGrabBatflys;
		if (obj24 == null)
		{
			hook_AutoGrabBatflys val24 = SlugcatStats_AutoGrabBatflys;
			_003C_003EO._003C23_003E__SlugcatStats_AutoGrabBatflys = val24;
			obj24 = (object)val24;
		}
		SlugcatStats.AutoGrabBatflys += (hook_AutoGrabBatflys)obj24;
		object obj25 = _003C_003EO._003C24_003E__SlugcatStats_ctor;
		if (obj25 == null)
		{
			hook_ctor val25 = SlugcatStats_ctor;
			_003C_003EO._003C24_003E__SlugcatStats_ctor = val25;
			obj25 = (object)val25;
		}
		SlugcatStats.ctor += (hook_ctor)obj25;
		object obj26 = _003C_003EO._003C25_003E__SlugcatStats_SlugcatStartingKarma;
		if (obj26 == null)
		{
			hook_SlugcatStartingKarma val26 = SlugcatStats_SlugcatStartingKarma;
			_003C_003EO._003C25_003E__SlugcatStats_SlugcatStartingKarma = val26;
			obj26 = (object)val26;
		}
		SlugcatStats.SlugcatStartingKarma += (hook_SlugcatStartingKarma)obj26;
		object obj27 = _003C_003EO._003C26_003E__SaveState_ctor;
		if (obj27 == null)
		{
			hook_ctor val27 = SaveState_ctor;
			_003C_003EO._003C26_003E__SaveState_ctor = val27;
			obj27 = (object)val27;
		}
		SaveState.ctor += (hook_ctor)obj27;
		object obj28 = _003C_003EO._003C27_003E__WorldLoader_GeneratePopulation;
		if (obj28 == null)
		{
			hook_GeneratePopulation val28 = WorldLoader_GeneratePopulation;
			_003C_003EO._003C27_003E__WorldLoader_GeneratePopulation = val28;
			obj28 = (object)val28;
		}
		WorldLoader.GeneratePopulation += (hook_GeneratePopulation)obj28;
		object obj29 = _003C_003EO._003C28_003E__OverseerAbstractAI_SetAsPlayerGuide;
		if (obj29 == null)
		{
			hook_SetAsPlayerGuide val29 = OverseerAbstractAI_SetAsPlayerGuide;
			_003C_003EO._003C28_003E__OverseerAbstractAI_SetAsPlayerGuide = val29;
			obj29 = (object)val29;
		}
		OverseerAbstractAI.SetAsPlayerGuide += (hook_SetAsPlayerGuide)obj29;
		object obj30 = _003C_003EO._003C29_003E__WorldLoader_OverseerSpawnConditions;
		if (obj30 == null)
		{
			hook_OverseerSpawnConditions val30 = WorldLoader_OverseerSpawnConditions;
			_003C_003EO._003C29_003E__WorldLoader_OverseerSpawnConditions = val30;
			obj30 = (object)val30;
		}
		WorldLoader.OverseerSpawnConditions += (hook_OverseerSpawnConditions)obj30;
		object obj31 = _003C_003EO._003C30_003E__PlayerGraphics_DefaultSlugcatColor;
		if (obj31 == null)
		{
			hook_DefaultSlugcatColor val31 = PlayerGraphics_DefaultSlugcatColor;
			_003C_003EO._003C30_003E__PlayerGraphics_DefaultSlugcatColor = val31;
			obj31 = (object)val31;
		}
		PlayerGraphics.DefaultSlugcatColor += (hook_DefaultSlugcatColor)obj31;
		object obj32 = _003C_003EO._003C31_003E__SaveState_GetStoryDenPosition;
		if (obj32 == null)
		{
			hook_GetStoryDenPosition val32 = SaveState_GetStoryDenPosition;
			_003C_003EO._003C31_003E__SaveState_GetStoryDenPosition = val32;
			obj32 = (object)val32;
		}
		SaveState.GetStoryDenPosition += (hook_GetStoryDenPosition)obj32;
		object obj33 = _003C_003EO._003C32_003E__IntroRoll_ctor;
		if (obj33 == null)
		{
			Manipulator val33 = IntroRoll_ctor;
			_003C_003EO._003C32_003E__IntroRoll_ctor = val33;
			obj33 = (object)val33;
		}
		IntroRoll.ctor += (Manipulator)obj33;
		SlugBaseCharacter.Refreshed += Refreshed;
		WorldHooks.Apply();
	}

	private static void Refreshed(object sender, SlugBaseCharacter.RefreshEventArgs args)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Expected O, but got Unknown
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		SlugBasePlugin.Logger.LogDebug($"Refreshed: {args.ID}");
		RoomCamera[] cameras = args.Game.cameras;
		foreach (RoomCamera val in cameras)
		{
			foreach (SpriteLeaser spriteLeaser in val.spriteLeasers)
			{
				IDrawable drawableObject = spriteLeaser.drawableObject;
				PlayerGraphics val2 = (PlayerGraphics)(object)((drawableObject is PlayerGraphics) ? drawableObject : null);
				if (val2 != null && (ExtEnum<Name>)(object)val2.player.SlugCatClass == (ExtEnum<Name>)(object)args.ID)
				{
					((GraphicsModule)val2).ApplyPalette(spriteLeaser, val, val.currentPalette);
				}
			}
		}
		if (ModManager.MSC && args.Game.IsArenaSession)
		{
			SlugcatStats[] characterStats_Mplayer = args.Game.GetArenaGameSession.characterStats_Mplayer;
			for (int j = 0; j < characterStats_Mplayer.Length; j++)
			{
				if ((ExtEnum<Name>)(object)characterStats_Mplayer[j].name == (ExtEnum<Name>)(object)args.Character.Name)
				{
					characterStats_Mplayer[j] = new SlugcatStats(args.Character.Name, characterStats_Mplayer[j].malnourished);
				}
			}
		}
		if (ModManager.CoopAvailable && args.Game.IsStorySession)
		{
			SlugcatStats[] characterStatsJollyplayer = args.Game.GetStorySession.characterStatsJollyplayer;
			for (int k = 0; k < characterStatsJollyplayer.Length; k++)
			{
				if ((ExtEnum<Name>)(object)characterStatsJollyplayer[k].name == (ExtEnum<Name>)(object)args.Character.Name)
				{
					characterStatsJollyplayer[k] = new SlugcatStats(args.Character.Name, characterStatsJollyplayer[k].malnourished);
				}
			}
		}
		if ((ExtEnum<Name>)(object)args.Game.session.characterStats.name == (ExtEnum<Name>)(object)args.Character.Name)
		{
			args.Game.session.characterStats = new SlugcatStats(args.Character.Name, args.Game.session.characterStats.malnourished);
		}
	}

	private static void ProcessManager_PreSwitchMainProcess(orig_PreSwitchMainProcess orig, ProcessManager self, ProcessID ID)
	{
		orig.Invoke(self, ID);
		CustomTimeline.ReloadTimelineOrder();
	}

	private static void DreamsState_StaticEndOfCycleProgress(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		ILLabel val3 = default(ILLabel);
		if (val.TryGotoNext((MoveType)1, new Func<Instruction, bool>[2]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdarg(x, 0),
			(Instruction x) => ILPatternMatchingExt.MatchBrtrue(x, ref val3)
		}))
		{
			ILLabel val2 = val.DefineLabel();
			val.Emit(OpCodes.Ldarg_0);
			val.EmitDelegate<Func<SaveState, bool>>((Func<SaveState, bool>)((SaveState saveState) => saveState != null && SlugBaseCharacter.TryGet(saveState.saveStateNumber, out var character) && ((Feature<bool>)GameFeatures.UseDefaultDreams).TryGet(character, out bool value) && !value));
			val.Emit(OpCodes.Brfalse, (object)val2);
			val.Emit(OpCodes.Ret);
			val.MarkLabel(val2);
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook DreamsState_StaticEndOfCycleProgress failed!");
		}
	}

	private static LinkedList<Timeline> SlugcatStats_SlugcatTimelineOrder(orig_SlugcatTimelineOrder orig)
	{
		LinkedList<Timeline> linkedList = orig.Invoke();
		foreach (CustomTimeline orderedTimeline in CustomTimeline.OrderedTimelines)
		{
			if (linkedList.Contains(orderedTimeline.ID))
			{
				continue;
			}
			Timeline val = orderedTimeline.InsertAfter?.FirstOrDefault(linkedList.Contains);
			if (val != null)
			{
				linkedList.AddAfter(linkedList.Find(val), orderedTimeline.ID);
				continue;
			}
			Timeline val2 = orderedTimeline.InsertBefore?.FirstOrDefault(linkedList.Contains);
			if (val2 != null)
			{
				linkedList.AddBefore(linkedList.Find(val2), orderedTimeline.ID);
			}
		}
		return linkedList;
	}

	private static bool SlugcatStats_SlugcatCanMaul(orig_SlugcatCanMaul orig, Name name)
	{
		if (SlugBaseCharacter.TryGet(name, out var character) && ((Feature<bool>)PlayerFeatures.CanMaul).TryGet(character, out bool value))
		{
			return value;
		}
		return orig.Invoke(name);
	}

	public static bool Player_CanMaulCreature(orig_CanMaulCreature orig, Player player, Creature crit)
	{
		if (PlayerFeatures.MaulBlacklist.TryGet(player, out var value))
		{
			for (CreatureTemplate val = crit.Template; val != null; val = val.ancestor)
			{
				if (Array.IndexOf(value, crit.Template.type) != -1)
				{
					return false;
				}
			}
		}
		return orig.Invoke(player, crit);
	}

	public static void Player_GrabUpdate(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		if (val.TryGotoNext(new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdsfld<DamageType>(x, "Bite")
		}) && val.TryGotoNext(new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchCallOrCallvirt<Creature>(x, "Violence")
		}) && val.TryGotoPrev((MoveType)2, new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdcR4(x, 1f)
		}))
		{
			val.Emit(OpCodes.Ldarg_0);
			val.EmitDelegate<Func<float, Player, float>>((Func<float, Player, float>)((float baseDamage, Player player) => PlayerFeatures.MaulDamage.TryGet(player, out var value) ? value : baseDamage));
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook Player_GrabUpdate, damage, failed!");
		}
		val.Index = 0;
		if (val.TryGotoNext(new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchCallOrCallvirt<SlugcatStats>(x, "SlugcatCanMaul")
		}) && val.TryGotoPrev((MoveType)2, new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdsfld<ModManager>(x, "MSC")
		}))
		{
			val.Emit(OpCodes.Pop);
			val.Emit(OpCodes.Ldc_I4_1);
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook Player_GrabUpdate, remove MSC check, failed!");
		}
	}

	private static void Player_ctor(orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		orig.Invoke(self, abstractCreature, world);
		if (PlayerFeatures.BackSpear.TryGet(self, out var value) && value)
		{
			if (self.spearOnBack == null)
			{
				self.spearOnBack = new SpearOnBack(self);
			}
		}
	}

	private static void DreamScreen_Update(ILContext il)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		MethodInfo dreamsEqual = typeof(ExtEnum<SceneID>).GetMethod("op_Equality");
		if (val.TryGotoNext(new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdcI4(x, 35)
		}) && val.TryGotoPrev((MoveType)2, new Func<Instruction, bool>[2]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdsfld<SceneID>(x, "SleepScreen"),
			(Instruction x) => ILPatternMatchingExt.MatchCallOrCallvirt(x, (MethodBase)dreamsEqual)
		}))
		{
			val.Emit(OpCodes.Ldarg_0);
			val.EmitDelegate<Func<DreamScreen, bool>>((Func<DreamScreen, bool>)delegate(DreamScreen self)
			{
				MainLoopProcess currentMainLoop = ((MainLoopProcess)self).manager.currentMainLoop;
				RainWorldGame val2 = (RainWorldGame)(object)((currentMainLoop is RainWorldGame) ? currentMainLoop : null);
				Name name;
				if (val2 != null)
				{
					name = val2.StoryCharacter;
				}
				else
				{
					PlayerProgression progression = ((MainLoopProcess)self).manager.rainWorld.progression;
					name = ((progression != null) ? progression.PlayingAsSlugcat : null);
				}
				SlugBaseCharacter character;
				SceneID value;
				return SlugBaseCharacter.TryGet(name, out character) && ((Feature<SceneID>)GameFeatures.SleepScene).TryGet(character, out value) && (ExtEnum<SceneID>)(object)value == (ExtEnum<SceneID>)(object)((Menu)self).scene.sceneID;
			});
			val.Emit(OpCodes.Or);
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook DreamScreen_Update failed!");
		}
	}

	private static void MenuScene_ctor(orig_ctor orig, MenuScene self, Menu menu, MenuObject owner, SceneID sceneID)
	{
		MainLoopProcess currentMainLoop = ((MainLoopProcess)menu).manager.currentMainLoop;
		RainWorldGame val = (RainWorldGame)(object)((currentMainLoop is RainWorldGame) ? currentMainLoop : null);
		Name val2;
		if (val != null)
		{
			val2 = val.StoryCharacter;
		}
		else
		{
			PlayerProgression progression = ((MainLoopProcess)menu).manager.rainWorld.progression;
			val2 = ((progression != null) ? progression.PlayingAsSlugcat : null);
		}
		if ((ExtEnum<Name>)(object)val2 != (ExtEnum<Name>)null && SlugBaseCharacter.TryGet(val2, out var character) && (((ExtEnum<SceneID>)(object)sceneID == (ExtEnum<SceneID>)(object)SceneID.SleepScreen && ((Feature<SceneID>)GameFeatures.SleepScene).TryGet(character, out SceneID value)) || ((ExtEnum<SceneID>)(object)sceneID == (ExtEnum<SceneID>)(object)SceneID.NewDeath && ((Feature<SceneID>)GameFeatures.DeathScene).TryGet(character, out value)) || ((ExtEnum<SceneID>)(object)sceneID == (ExtEnum<SceneID>)(object)SceneID.StarveScreen && ((Feature<SceneID>)GameFeatures.StarveScene).TryGet(character, out value))) && ((ExtEnumBase)value).Index != -1)
		{
			sceneID = value;
		}
		orig.Invoke(self, menu, owner, sceneID);
	}

	private static List<string> PlayerGraphics_DefaultBodyPartColorHex(orig_DefaultBodyPartColorHex orig, Name slugcatID)
	{
		List<string> list = orig.Invoke(slugcatID);
		if (SlugBaseCharacter.TryGet(slugcatID, out var character) && ((Feature<ColorSlot[]>)PlayerFeatures.CustomColors).TryGet(character, out ColorSlot[] value))
		{
			list.Clear();
			list.AddRange(value.Select((ColorSlot slot) => Custom.colorToHex(slot.Default)));
		}
		return list;
	}

	private static List<string> PlayerGraphics_ColoredBodyPartList(orig_ColoredBodyPartList orig, Name slugcatID)
	{
		List<string> list = orig.Invoke(slugcatID);
		if (SlugBaseCharacter.TryGet(slugcatID, out var character) && ((Feature<ColorSlot[]>)PlayerFeatures.CustomColors).TryGet(character, out ColorSlot[] value))
		{
			list.Clear();
			list.AddRange(value.Select((ColorSlot slot) => slot.Name));
		}
		return list;
	}

	private static void PlayerGraphics_DrawSprites(orig_DrawSprites orig, PlayerGraphics self, SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
		if (sLeaser.sprites.Length <= 9 || sLeaser.sprites[9] == null)
		{
			return;
		}
		Color? color = PlayerColor.Eyes.GetColor(self);
		if (color.HasValue)
		{
			Color valueOrDefault = color.GetValueOrDefault();
			if (true)
			{
				sLeaser.sprites[9].color = valueOrDefault;
				return;
			}
		}
		if ((ExtEnum<Name>)(object)self.player.slugcatStats.name == (ExtEnum<Name>)(object)Name.Night)
		{
			sLeaser.sprites[9].color = Color.Lerp(new Color(1f, 1f, 1f), rCam.currentPalette.blackColor, 0.3f);
		}
	}

	/// CustomColors: Apply body color override
	private static void PlayerGraphics_ApplyPalette(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		if (val.TryGotoNext((MoveType)1, new Func<Instruction, bool>[3]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdarg(x, 0),
			(Instruction x) => ILPatternMatchingExt.MatchLdloc(x, 1),
			(Instruction x) => ILPatternMatchingExt.MatchCallOrCallvirt<GraphicsModule>(x, "HypothermiaColorBlend")
		}))
		{
			val.Emit(OpCodes.Ldarg_0);
			val.Emit(OpCodes.Ldloc_1);
			val.Emit(OpCodes.Ldarg_3);
			val.EmitDelegate<Func<PlayerGraphics, Color, RoomPalette, Color>>((Func<PlayerGraphics, Color, RoomPalette, Color>)delegate(PlayerGraphics self, Color color, RoomPalette palette)
			{
				Color? color2 = PlayerColor.Body.GetColor(self);
				if (color2.HasValue)
				{
					Color valueOrDefault = color2.GetValueOrDefault();
					if (true)
					{
						Color b = Color.Lerp(valueOrDefault, Color.gray, 0.4f);
						float t = (self.player.Malnourished ? self.malnourished : Mathf.Max(0f, self.malnourished - 0.005f));
						return Color.Lerp(valueOrDefault, b, t);
					}
				}
				return color;
			});
			val.Emit(OpCodes.Stloc_1);
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook PlayerGraphics_ApplyPalette failed!");
		}
	}

	private static bool Player_CanEatMeat(orig_CanEatMeat orig, Player self, Creature crit)
	{
		int result;
		if (SlugBaseCharacter.TryGet(self.SlugCatClass, out var character) && ((Feature<Diet>)PlayerFeatures.Diet).TryGet(character, out Diet value))
		{
			if (value.GetMeatMultiplier(self, crit) > 0f)
			{
				IPlayerEdible val = (IPlayerEdible)(object)((crit is IPlayerEdible) ? crit : null);
				if (val == null || !val.Edible)
				{
					result = (crit.dead ? 1 : 0);
					goto IL_004f;
				}
			}
			result = 0;
			goto IL_004f;
		}
		return orig.Invoke(self, crit);
		IL_004f:
		return (byte)result != 0;
	}

	private static void Player_EatMeatUpdate(ILContext il)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		ILLabel foodAdded = null;
		if (val.TryGotoNext(new Func<Instruction, bool>[3]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdarg(x, 0),
			(Instruction x) => ILPatternMatchingExt.MatchCallOrCallvirt<Player>(x, "AddQuarterFood"),
			(Instruction x) => ILPatternMatchingExt.MatchBr(x, ref foodAdded)
		}) && val.TryGotoNext((MoveType)1, new Func<Instruction, bool>[3]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdarg(x, 0),
			(Instruction x) => ILPatternMatchingExt.MatchLdcI4(x, 1),
			(Instruction x) => ILPatternMatchingExt.MatchCallOrCallvirt<Player>(x, "AddFood")
		}))
		{
			val.Emit(OpCodes.Ldarg_0);
			val.Emit(OpCodes.Ldarg_1);
			val.EmitDelegate<Func<Player, int, bool>>((Func<Player, int, bool>)delegate(Player self, int graspIndex)
			{
				if (SlugBaseCharacter.TryGet(self.SlugCatClass, out var character) && ((Feature<Diet>)PlayerFeatures.Diet).TryGet(character, out Diet value))
				{
					PhysicalObject grabbed = ((Creature)self).grasps[graspIndex].grabbed;
					Creature val2 = (Creature)(object)((grabbed is Creature) ? grabbed : null);
					if (val2 != null)
					{
						float meatMultiplier = value.GetMeatMultiplier(self, val2);
						int num;
						for (num = Mathf.RoundToInt(meatMultiplier * 4f); num >= 4; num -= 4)
						{
							self.AddFood(1);
						}
						while (num >= 1)
						{
							self.AddQuarterFood();
							num--;
						}
						return true;
					}
				}
				return false;
			});
			val.Emit(OpCodes.Brtrue, (object)foodAdded);
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook Player_EatMeatUpdate failed!");
		}
	}

	private static void Player_ObjectEaten(orig_ObjectEaten orig, Player self, IPlayerEdible edible)
	{
		if (SlugBaseCharacter.TryGet(self.SlugCatClass, out var character) && ((Feature<Diet>)PlayerFeatures.Diet).TryGet(character, out Diet _) && SlugcatStats.NourishmentOfObjectEaten(self.SlugCatClass, edible) == -1)
		{
			GraphicsModule graphicsModule = ((PhysicalObject)self).graphicsModule;
			GraphicsModule obj = ((graphicsModule is PlayerGraphics) ? graphicsModule : null);
			if (obj != null)
			{
				((PlayerGraphics)obj).LookAtNothing();
			}
			((Creature)self).Stun(60);
		}
		else
		{
			orig.Invoke(self, edible);
		}
	}

	private static int SlugcatStats_NourishmentOfObjectEaten(orig_NourishmentOfObjectEaten orig, Name slugcatIndex, IPlayerEdible eatenobject)
	{
		int num = orig.Invoke(slugcatIndex, eatenobject);
		if (SlugBaseCharacter.TryGet(slugcatIndex, out var character) && ((Feature<Diet>)PlayerFeatures.Diet).TryGet(character, out Diet value))
		{
			PhysicalObject val = (PhysicalObject)(object)((eatenobject is PhysicalObject) ? eatenobject : null);
			if (val != null)
			{
				float foodMultiplier = value.GetFoodMultiplier(val);
				num = ((!(foodMultiplier >= 0f)) ? (-1) : Mathf.RoundToInt((float)num * foodMultiplier));
			}
		}
		return num;
	}

	private static void SlugcatPage_AddImage(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		if (val.TryGotoNext((MoveType)2, new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchStloc(x, 0)
		}))
		{
			val.Emit(OpCodes.Ldarg_0);
			val.Emit(OpCodes.Ldarg_1);
			val.Emit(OpCodes.Ldloc_0);
			val.EmitDelegate<Func<SlugcatPage, bool, SceneID, SceneID>>((Func<SlugcatPage, bool, SceneID, SceneID>)delegate(SlugcatPage self, bool ascended, SceneID sceneID)
			{
				if (SlugBaseCharacter.TryGet(self.slugcatNumber, out var character))
				{
					Menu menu = ((MenuObject)self).menu;
					SlugcatSelectMenu val2 = (SlugcatSelectMenu)(object)((menu is SlugcatSelectMenu) ? menu : null);
					if (val2 != null && val2.saveGameData.TryGetValue(self.slugcatNumber, out var value) && value != null && MinedSaveData.Data.TryGetValue(value, out var value2))
					{
						SceneID selectMenuScene = value2.SelectMenuScene;
						if (selectMenuScene != null && ((ExtEnumBase)selectMenuScene).Index > -1)
						{
							sceneID = value2.SelectMenuScene;
							goto IL_00b5;
						}
					}
					SceneID value4;
					if (ascended && ((Feature<SceneID>)GameFeatures.SelectMenuSceneAscended).TryGet(character, out SceneID value3))
					{
						sceneID = value3;
					}
					else if (((Feature<SceneID>)GameFeatures.SelectMenuScene).TryGet(character, out value4))
					{
						sceneID = value4;
					}
				}
				goto IL_00b5;
				IL_00b5:
				if (CustomScene.Registry.TryGet(sceneID, out var value5))
				{
					self.markOffset = value5.MarkPos ?? self.markOffset;
					self.glowOffset = value5.GlowPos ?? self.glowOffset;
					self.sceneOffset = value5.SelectMenuOffset ?? self.sceneOffset;
					self.slugcatDepth = value5.SlugcatDepth ?? self.slugcatDepth;
				}
				return sceneID;
			});
			val.Emit(OpCodes.Stloc_0);
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook SlugcatPage_AddImage failed!");
		}
	}

	private static void CreatureCommunities_LoadDefaultCommunityAlignments(orig_LoadDefaultCommunityAlignments orig, CreatureCommunities self, Name saveStateNumber)
	{
		orig.Invoke(self, saveStateNumber);
		if (!SlugBaseCharacter.TryGet(saveStateNumber, out var character) || !((Feature<Dictionary<CommunityID, RepOverride>>)PlayerFeatures.CommunityAlignments).TryGet(character, out Dictionary<CommunityID, RepOverride> value))
		{
			return;
		}
		foreach (KeyValuePair<CommunityID, RepOverride> item in value)
		{
			for (int i = 0; i < self.playerOpinions.GetLength(1); i++)
			{
				for (int j = 0; j < self.playerOpinions.GetLength(2); j++)
				{
					int num = (int)(ExtEnum<CommunityID>)(object)item.Key - 1;
					if (num >= 0 && num < self.playerOpinions.GetLength(0))
					{
						self.playerOpinions[num, i, j] = Mathf.Lerp(self.playerOpinions[num, i, j], item.Value.Target, item.Value.Strength);
					}
				}
			}
		}
	}

	private static float CreatureCommunities_LikeOfPlayer(orig_LikeOfPlayer orig, CreatureCommunities self, CommunityID commID, int region, int playerNumber)
	{
		List<AbstractCreature> list = self.session?.Players;
		if (list != null && playerNumber >= 0 && playerNumber < list.Count)
		{
			PhysicalObject obj = ((AbstractPhysicalObject)(list[playerNumber]?)).realizedObject;
			Player val = (Player)(object)((obj is Player) ? obj : null);
			if (val != null && PlayerFeatures.CommunityAlignments.TryGet(val, out var value) && value.TryGetValue(commID, out var value2) && value2.Locked)
			{
				return value2.Target;
			}
		}
		return orig.Invoke(self, commID, region, playerNumber);
	}

	private static IntVector2 SlugcatStats_SlugcatFoodMeter(orig_SlugcatFoodMeter orig, Name slugcat)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		IntVector2 result = orig.Invoke(slugcat);
		if (SlugBaseCharacter.TryGet(slugcat, out var character))
		{
			if (((Feature<int>)GameFeatures.FoodMin).TryGet(character, out int value))
			{
				result.y = value;
			}
			if (((Feature<int>)GameFeatures.FoodMax).TryGet(character, out int value2))
			{
				result.x = value2;
			}
		}
		return result;
	}

	private static bool DeathPersistentSaveData_CanUseUnlockedGates(orig_CanUseUnlockedGates orig, DeathPersistentSaveData self, Name slugcat)
	{
		if (SlugBaseCharacter.TryGet(slugcat, out var character) && ((Feature<bool>)GameFeatures.PermaUnlockGates).TryGet(character, out bool value))
		{
			return value;
		}
		return orig.Invoke(self, slugcat);
	}

	private static void RainCycle_ctor(orig_ctor orig, RainCycle self, World world, float minutes)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		float value;
		bool flag = GameFeatures.CycleLengthMin.TryGet(world.game, out value);
		float value2;
		bool flag2 = GameFeatures.CycleLengthMax.TryGet(world.game, out value2);
		if (flag || flag2)
		{
			if (!flag)
			{
				value = (float)world.game.setupValues.cycleTimeMin / 60f;
			}
			if (!flag2)
			{
				value2 = (float)world.game.setupValues.cycleTimeMax / 60f;
			}
			minutes = Mathf.Lerp(value, value2, UnityEngine.Random.value);
		}
		orig.Invoke(self, world, minutes);
	}

	private static bool SlugcatStats_AutoGrabBatflys(orig_AutoGrabBatflys orig, Name slugcatNum)
	{
		if (SlugBaseCharacter.TryGet(slugcatNum, out var character) && ((Feature<bool>)PlayerFeatures.AutoGrabFlies).TryGet(character, out bool value))
		{
			return value;
		}
		return orig.Invoke(slugcatNum);
	}

	private static void SlugcatStats_ctor(orig_ctor orig, SlugcatStats self, Name slugcat, bool malnourished)
	{
		orig.Invoke(self, slugcat, malnourished);
		if (SlugBaseCharacter.TryGet(slugcat, out var character))
		{
			if (((Feature<float[]>)PlayerFeatures.WeightMul).TryGet(character, out float[] value))
			{
				self.bodyWeightFac = ApplyStarve<float>(value, Mathf.Min(value[0], 0.9f));
			}
			if (((Feature<float[]>)PlayerFeatures.TunnelSpeedMul).TryGet(character, out float[] value2))
			{
				self.corridorClimbSpeedFac = ApplyStarve<float>(value2, 0.86f);
			}
			if (((Feature<float[]>)PlayerFeatures.ClimbSpeedMul).TryGet(character, out float[] value3))
			{
				self.poleClimbSpeedFac = ApplyStarve<float>(value3, 0.8f);
			}
			if (((Feature<float[]>)PlayerFeatures.WalkSpeedMul).TryGet(character, out float[] value4))
			{
				self.runspeedFac = ApplyStarve<float>(value4, 0.875f);
			}
			if (((Feature<float[]>)PlayerFeatures.CrouchStealth).TryGet(character, out float[] value5))
			{
				self.visualStealthInSneakMode = ApplyStarve<float>(value5, value5[0]);
			}
			if (((Feature<int[]>)PlayerFeatures.ThrowSkill).TryGet(character, out int[] value6))
			{
				self.throwingSkill = ApplyStarve<int>(value6, 0);
			}
			if (((Feature<float[]>)PlayerFeatures.LungsCapacityMul).TryGet(character, out float[] value7))
			{
				self.lungsFac = 1f / ApplyStarve<float>(value7, value7[0]);
			}
			if (((Feature<float[]>)PlayerFeatures.LoudnessMul).TryGet(character, out float[] value8))
			{
				self.loudnessFac = ApplyStarve<float>(value8, value8[0]);
			}
		}
		T ApplyStarve<T>(T[] values, T starveDefault)
		{
			if (malnourished)
			{
				return (values.Length > 1) ? values[1] : starveDefault;
			}
			return values[0];
		}
	}

	private static int SlugcatStats_SlugcatStartingKarma(orig_SlugcatStartingKarma orig, Name slugcatNum)
	{
		if (SlugBaseCharacter.TryGet(slugcatNum, out var character) && ((Feature<int>)GameFeatures.KarmaCap).TryGet(character, out int value))
		{
			return value;
		}
		return orig.Invoke(slugcatNum);
	}

	private static void SaveState_ctor(orig_ctor orig, SaveState self, Name saveStateNumber, PlayerProgression progression)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		orig.Invoke(self, saveStateNumber, progression);
		if (SlugBaseCharacter.TryGet(saveStateNumber, out var character))
		{
			bool value = default(bool);
			if (self.dreamsState == null && ((Feature<bool>)GameFeatures.HasDreams).TryGet(character, out value) && value)
			{
				self.dreamsState = new DreamsState();
			}
			if (((Feature<int>)GameFeatures.KarmaCap).TryGet(character, out int value2))
			{
				self.deathPersistentSaveData.karmaCap = value2;
			}
			if (((Feature<int>)GameFeatures.Karma).TryGet(character, out int value3))
			{
				self.deathPersistentSaveData.karma = value3;
			}
			if (((Feature<bool>)GameFeatures.TheMark).TryGet(character, out bool value4) && value4)
			{
				self.deathPersistentSaveData.theMark = true;
			}
			if (((Feature<bool>)GameFeatures.TheGlow).TryGet(character, out bool value5) && value5)
			{
				self.theGlow = true;
			}
		}
	}

	private static void WorldLoader_GeneratePopulation(orig_GeneratePopulation orig, WorldLoader self, bool fresh)
	{
		try
		{
			_generatingPopulation = true;
			orig.Invoke(self, fresh);
		}
		finally
		{
			_generatingPopulation = false;
		}
	}

	private static void OverseerAbstractAI_SetAsPlayerGuide(orig_SetAsPlayerGuide orig, OverseerAbstractAI self, int ownerOverride)
	{
		if (_generatingPopulation && GameFeatures.GuideOverseer.TryGet(((AbstractCreatureAI)self).world.game, out var value))
		{
			ownerOverride = value;
		}
		orig.Invoke(self, ownerOverride);
	}

	private static bool WorldLoader_OverseerSpawnConditions(orig_OverseerSpawnConditions orig, WorldLoader self, Name character)
	{
		RainWorldGame game = self.game;
		if (SlugBaseCharacter.TryGet(((game != null) ? game.StoryCharacter : null) ?? character, out var character2) && !((Feature<int>)GameFeatures.GuideOverseer).TryGet(character2, out int _))
		{
			return false;
		}
		return orig.Invoke(self, character);
	}

	private static Color PlayerGraphics_DefaultSlugcatColor(orig_DefaultSlugcatColor orig, Name i)
	{
		if (SlugBaseCharacter.TryGet(i, out var character) && ((Feature<Color>)PlayerFeatures.SlugcatColor).TryGet(character, out Color value))
		{
			return value;
		}
		return orig.Invoke(i);
	}

	private static string SaveState_GetStoryDenPosition(orig_GetStoryDenPosition orig, Name slugcat, out bool isVanilla)
	{
		if (SlugBaseCharacter.TryGet(slugcat, out var character) && character.Features.TryGet(GameFeatures.StartRoom, out var value))
		{
			string[] array = value;
			foreach (string text in array)
			{
				if (WorldLoader.FindRoomFile(text, false, ".txt", true) != null)
				{
					string path = Custom.RootFolderDirectory();
					string text2 = "";
					if (text.Contains("_"))
					{
						text2 = text.Split('_')[0];
					}
					isVanilla = File.Exists(Path.Combine(path, "World", text2 + "-Rooms", text + ".txt")) || File.Exists(Path.Combine(path, "World", "Gate Shelters", text + ".txt"));
					return text;
				}
			}
		}
		return orig.Invoke(slugcat, ref isVanilla);
	}

	private static void IntroRoll_ctor(ILContext il)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		ILLabel introRollCAdded = null;
		ILLabel val2 = default(ILLabel);
		if (!val.TryGotoNext((MoveType)1, new Func<Instruction, bool>[2]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdsfld<ModManager>(x, "Watcher"),
			(Instruction x) => ILPatternMatchingExt.MatchBrfalse(x, ref val2)
		}) || !val.Clone().TryGotoNext(new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchBr(x, ref introRollCAdded)
		}))
		{
			return;
		}
		val.Emit(OpCodes.Ldarg_0);
		val.EmitDelegate<Func<IntroRoll, bool>>((Func<IntroRoll, bool>)delegate(IntroRoll self)
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			Name currentlySelectedSinglePlayerSlugcat = ((MainLoopProcess)self).manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat;
			if (SlugBaseCharacter.TryGet(currentlySelectedSinglePlayerSlugcat, out var character) && ((Feature<string>)GameFeatures.TitleCard).TryGet(character, out string value))
			{
				self.illustrations[2] = new MenuIllustration((Menu)(object)self, (MenuObject)(object)((Menu)self).pages[0], "", value, new Vector2(0f, 0f), true, false);
				return true;
			}
			return false;
		});
		val.Emit(OpCodes.Brtrue, (object)introRollCAdded);
	}
}
