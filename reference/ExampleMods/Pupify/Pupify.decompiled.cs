using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using HUD;
using HarmonyLib;
using IL;
using JollyCoop;
using JollyCoop.JollyMenu;
using Menu;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using Microsoft.CodeAnalysis;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using On;
using On.Menu;
using On.MoreSlugcats;
using Pupify.Hooks;
using RWCustom;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyCompany("Pupify")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+61430a61a5c9d729f99ff06ed79b53a1c52c5534")]
[assembly: AssemblyProduct("Pupify")]
[assembly: AssemblyTitle("Pupify")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.0.0")]
[module: UnverifiableCode]
[module: RefSafetyRules(11)]
[module: UnverifiableCode]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace Pupify
{
	public static class MultiPlayer
	{
		[CompilerGenerated]
		private static class <>O
		{
			public static hook_ctor <0>__StoryGameSession_ctor;

			public static hook_ctor <1>__ArenaGameSession_ctor;

			public static hook_Update <2>__MultiplayerMenu_Update;

			public static Manipulator <3>__StoryGameSession_CreateJollySlugStats;
		}

		public static GameSession Session;

		public static readonly List<SlugcatStats> playerStats = new List<SlugcatStats>();

		public static List<bool> makePup = new List<bool>();

		public static int startingIncrement;

		public static int currentIndex;

		public static int playerToLoad;

		public static bool onlyPupsLeft;

		public static int numAccessed;

		private static bool rightHeld;

		public static bool reorganized;

		public static void Init()
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
			object obj = <>O.<0>__StoryGameSession_ctor;
			if (obj == null)
			{
				hook_ctor val = StoryGameSession_ctor;
				<>O.<0>__StoryGameSession_ctor = val;
				obj = (object)val;
			}
			StoryGameSession.ctor += (hook_ctor)obj;
			object obj2 = <>O.<1>__ArenaGameSession_ctor;
			if (obj2 == null)
			{
				hook_ctor val2 = ArenaGameSession_ctor;
				<>O.<1>__ArenaGameSession_ctor = val2;
				obj2 = (object)val2;
			}
			ArenaGameSession.ctor += (hook_ctor)obj2;
			object obj3 = <>O.<2>__MultiplayerMenu_Update;
			if (obj3 == null)
			{
				hook_Update val3 = MultiplayerMenu_Update;
				<>O.<2>__MultiplayerMenu_Update = val3;
				obj3 = (object)val3;
			}
			MultiplayerMenu.Update += (hook_Update)obj3;
			object obj4 = <>O.<3>__StoryGameSession_CreateJollySlugStats;
			if (obj4 == null)
			{
				Manipulator val4 = StoryGameSession_CreateJollySlugStats;
				<>O.<3>__StoryGameSession_CreateJollySlugStats = val4;
				obj4 = (object)val4;
			}
			StoryGameSession.CreateJollySlugStats += (Manipulator)obj4;
		}

		public static SlugcatStats GetCurrentPlayer(Name self)
		{
			if (!ModManager.CoopAvailable && Session is StoryGameSession)
			{
				return Plugin.currentSlugcat;
			}
			int num = ((Session == null) ? 2 : ((!(Session is StoryGameSession)) ? 1 : 2));
			GameSession session = Session;
			StoryGameSession val = (StoryGameSession)(object)((session is StoryGameSession) ? session : null);
			if (val != null && ModManager.CoopAvailable)
			{
				int num2 = ((GameSession)val).game.rainWorld.options.jollyPlayerOptionsArray.Count((JollyPlayerOptions player) => player.isPup);
				while ((ExtEnum<Name>)(object)self == (ExtEnum<Name>)(object)SlugcatStatsName.Slugpup && num2 > 0 && Plugin.playersCreated && startingIncrement > 1 && !((GameSession)val).game.rainWorld.options.jollyPlayerOptionsArray[currentIndex].isPup)
				{
					currentIndex++;
				}
			}
			SlugcatStats val2 = ((startingIncrement >= num) ? playerStats[currentIndex] : Plugin.currentSlugcat);
			if (startingIncrement < num)
			{
				startingIncrement++;
				if (startingIncrement == num)
				{
					val2 = null;
				}
			}
			else
			{
				currentIndex++;
			}
			if (!(Session is ArenaGameSession) && currentIndex >= Session?.Players.Count)
			{
				GameSession session2 = Session;
				if (session2 == null || session2.Players.Count != 0)
				{
					goto IL_0245;
				}
			}
			GameSession session3 = Session;
			ArenaGameSession val3 = (ArenaGameSession)(object)((session3 is ArenaGameSession) ? session3 : null);
			if (val3 != null && currentIndex >= val3.arenaSitting?.players.Count)
			{
				ArenaSitting arenaSitting = val3.arenaSitting;
				if (arenaSitting == null || arenaSitting.players.Count != 0)
				{
					goto IL_0245;
				}
			}
			goto IL_02e8;
			IL_0245:
			int num3 = -1;
			if (Session is StoryGameSession && ModManager.CoopAvailable)
			{
				num3 = Session.game.rainWorld.options.jollyPlayerOptionsArray.Count((JollyPlayerOptions player) => player.isPup);
			}
			if (Plugin.playersCreated && (num3 == -1 || num3 == 0 || (num3 > 0 && (ExtEnum<Name>)(object)self == (ExtEnum<Name>)(object)SlugcatStatsName.Slugpup)))
			{
				onlyPupsLeft = true;
			}
			currentIndex = 0;
			Plugin.playersCreated = true;
			goto IL_02e8;
			IL_02e8:
			if (Plugin.playersCreated)
			{
				GameSession session4 = Session;
				StoryGameSession val4 = (StoryGameSession)(object)((session4 is StoryGameSession) ? session4 : null);
				if (val4 != null && (ExtEnum<Name>)(object)self == (ExtEnum<Name>)(object)SlugcatStatsName.Slugpup)
				{
					int num4 = -1;
					for (int num5 = val4.characterStatsJollyplayer.Length - 1; num5 > 0; num5--)
					{
						if (((GameSession)val4).game.rainWorld.options.jollyPlayerOptionsArray[num5].isPup)
						{
							num4 = num5;
							break;
						}
					}
					if (num4 != -1 && val4.characterStatsJollyplayer[num4] == val2)
					{
						onlyPupsLeft = true;
					}
				}
			}
			return val2;
		}

		public static SlugcatStats GetSpecificPlayer(int index)
		{
			if (!reorganized)
			{
				while (index >= playerStats.Count)
				{
					playerStats.Add(null);
				}
				for (int i = 0; i <= index; i++)
				{
					GameSession session = Session;
					GameSession val = session;
					CompetitiveGameSession val2 = (CompetitiveGameSession)(object)((val is CompetitiveGameSession) ? val : null);
					if (val2 == null)
					{
						SandboxGameSession val3 = (SandboxGameSession)(object)((val is SandboxGameSession) ? val : null);
						if (val3 != null && playerStats[i] != ((ArenaGameSession)val3).characterStats_Mplayer[i])
						{
							playerStats[i] = ((ArenaGameSession)val3).characterStats_Mplayer[i];
						}
					}
					else if (playerStats[i] != ((ArenaGameSession)val2).characterStats_Mplayer[i])
					{
						playerStats[i] = ((ArenaGameSession)val2).characterStats_Mplayer[i];
					}
				}
				reorganized = true;
			}
			numAccessed++;
			return playerStats[index];
		}

		public static void AddPlayer(SlugcatStats self)
		{
			if (playerStats.Count == Session.Players.Count || Session is ArenaGameSession)
			{
				GameSession session = Session;
				ArenaGameSession val = (ArenaGameSession)(object)((session is ArenaGameSession) ? session : null);
				if (val == null || !(playerStats.Count < val.arenaSitting?.players.Count))
				{
					return;
				}
			}
			playerStats.Add(self);
		}

		public static void ClearPlayers()
		{
			playerStats.Clear();
		}

		private static void StoryGameSession_ctor(orig_ctor orig, StoryGameSession self, Name saveStateNumber, RainWorldGame game)
		{
			Session = (GameSession)(object)self;
			orig.Invoke(self, saveStateNumber, game);
		}

		private static void ArenaGameSession_ctor(orig_ctor orig, ArenaGameSession self, RainWorldGame game)
		{
			Session = (GameSession)(object)self;
			orig.Invoke(self, game);
		}

		private static void MultiplayerMenu_Update(orig_Update orig, MultiplayerMenu self)
		{
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			orig.Invoke(self);
			for (int i = 0; i < self.playerJoinButtons?.Length; i++)
			{
				if (makePup.Count < i + 1)
				{
					makePup.Add(item: false);
				}
				if ((((MenuObject)self.playerJoinButtons[i]).Selected || ((MenuObject)self.playerClassButtons[i]).Selected) && rightHeld && !Input.GetKey((KeyCode)324) && !makePup[i])
				{
					if (self.playerJoinButtons[i].Joined)
					{
						self.playerJoinButtons[i].roundedRect.borderColor = new HSLColor(0f, 1f, 0.5f);
					}
					else
					{
						self.playerJoinButtons[i].roundedRect.borderColor = new HSLColor(0.0055f, 1f, 0.2239f);
					}
					self.playerClassButtons[i].roundedRect.borderColor = new HSLColor(0f, 1f, 0.5f);
					((Menu)self).PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
					makePup[i] = true;
				}
				else if ((((MenuObject)self.playerJoinButtons[i]).Selected || ((MenuObject)self.playerClassButtons[i]).Selected) && rightHeld && !Input.GetKey((KeyCode)324) && makePup[i])
				{
					self.playerJoinButtons[i].roundedRect.borderColor = null;
					self.playerClassButtons[i].roundedRect.borderColor = null;
					((Menu)self).PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
					makePup[i] = false;
				}
				else if (makePup[i])
				{
					if (self.playerJoinButtons[i].Joined)
					{
						self.playerJoinButtons[i].roundedRect.borderColor = new HSLColor(0f, 1f, 0.5f);
					}
					else
					{
						self.playerJoinButtons[i].roundedRect.borderColor = new HSLColor(0.0055f, 1f, 0.2239f);
					}
					self.playerClassButtons[i].roundedRect.borderColor = new HSLColor(0f, 1f, 0.5f);
				}
			}
			rightHeld = Input.GetKey((KeyCode)324);
			if (MenuHooks.challengePupButton == null)
			{
				return;
			}
			if (((SymbolButtonToggle)MenuHooks.challengePupButton).isToggled != MenuHooks.arenaPreviousState)
			{
				((Menu)self).PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				if (makePup.Count > 0)
				{
					makePup[0] = ((SymbolButtonToggle)MenuHooks.challengePupButton).isToggled;
				}
			}
			MenuHooks.arenaPreviousState = ((SymbolButtonToggle)MenuHooks.challengePupButton).isToggled;
		}

		private static void StoryGameSession_CreateJollySlugStats(ILContext il)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Expected O, but got Unknown
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			ILCursor val = new ILCursor(il);
			try
			{
				while (val.TryGotoNext((MoveType)0, new Func<Instruction, bool>[1]
				{
					(Instruction instr) => ILPatternMatchingExt.MatchStfld<SlugcatStats>(instr, "foodToHibernate")
				}))
				{
					val.Remove();
					val.Emit(OpCodes.Pop);
					val.Emit(OpCodes.Pop);
				}
				val.Index = 0;
				while (val.TryGotoNext((MoveType)0, new Func<Instruction, bool>[1]
				{
					(Instruction instr) => ILPatternMatchingExt.MatchStfld<SlugcatStats>(instr, "maxFood")
				}))
				{
					val.Remove();
					val.Emit(OpCodes.Pop);
					val.Emit(OpCodes.Pop);
				}
				val.Index = 0;
				while (val.TryGotoNext((MoveType)0, new Func<Instruction, bool>[1]
				{
					(Instruction instr) => ILPatternMatchingExt.MatchStfld<SlugcatStats>(instr, "bodyWeightFac")
				}))
				{
					val.Remove();
					val.Emit(OpCodes.Pop);
					val.Emit(OpCodes.Pop);
				}
			}
			catch (Exception)
			{
			}
		}
	}
	public class Options : OptionInterface
	{
		private readonly ManualLogSource Logger;

		public readonly Configurable<bool> onlyCosmetic;

		public readonly Configurable<int> foodOption;

		public readonly Configurable<int> statsOption;

		public readonly Configurable<bool> bothHands;

		public readonly Configurable<bool> letStomach;

		public readonly Configurable<bool> letEatMeat;

		public readonly Configurable<bool> letMaul;

		public readonly Configurable<bool> holdHands;

		public readonly Configurable<bool> letPickup;

		public readonly Configurable<bool> overrideFood;

		public readonly Configurable<float> foodToHibernate;

		public readonly Configurable<float> maxFood;

		private UIelement[] UIArrPlayerOptions;

		private UIelement[] UIArrPlayerModify;

		private OpRadioButtonGroup foodGroup;

		private OpRadioButtonGroup statGroup;

		private static readonly FContainer foodBar = new FContainer();

		private FSprite[] circleSprites;

		private FSprite[] pipSprites;

		private FSprite staff;

		public Options(Plugin modInstance, ManualLogSource loggerSource)
		{
			Logger = loggerSource;
			onlyCosmetic = base.config.Bind<bool>("onlyCosmetic", false, (ConfigurableInfo)null);
			foodOption = base.config.Bind<int>("foodOption", 0, (ConfigurableInfo)null);
			statsOption = base.config.Bind<int>("statsOption", 0, (ConfigurableInfo)null);
			bothHands = base.config.Bind<bool>("bothHands", false, (ConfigurableInfo)null);
			letStomach = base.config.Bind<bool>("letStomach", false, (ConfigurableInfo)null);
			letMaul = base.config.Bind<bool>("letMaul", false, (ConfigurableInfo)null);
			letEatMeat = base.config.Bind<bool>("letEatMeat", false, (ConfigurableInfo)null);
			holdHands = base.config.Bind<bool>("holdHands", false, (ConfigurableInfo)null);
			letPickup = base.config.Bind<bool>("letPickup", false, (ConfigurableInfo)null);
			overrideFood = base.config.Bind<bool>("overrideFood", false, (ConfigurableInfo)null);
			foodToHibernate = base.config.Bind<float>("foodToHibernate", 2f, (ConfigurableInfo)null);
			maxFood = base.config.Bind<float>("maxFood", 3f, (ConfigurableInfo)null);
		}

		public override void Initialize()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Expected O, but got Unknown
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Expected O, but got Unknown
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Expected O, but got Unknown
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Expected O, but got Unknown
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Expected O, but got Unknown
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Expected O, but got Unknown
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Expected O, but got Unknown
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Expected O, but got Unknown
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Expected O, but got Unknown
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Expected O, but got Unknown
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Expected O, but got Unknown
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Expected O, but got Unknown
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Unknown result type (might be due to invalid IL or missing references)
			//IL_024c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Expected O, but got Unknown
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0289: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Expected O, but got Unknown
			//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d0: Expected O, but got Unknown
			//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Expected O, but got Unknown
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_030b: Expected O, but got Unknown
			//IL_031d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0323: Expected O, but got Unknown
			//IL_032f: Unknown result type (might be due to invalid IL or missing references)
			//IL_033e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0348: Unknown result type (might be due to invalid IL or missing references)
			//IL_034e: Expected O, but got Unknown
			//IL_0360: Unknown result type (might be due to invalid IL or missing references)
			//IL_0366: Expected O, but got Unknown
			//IL_0391: Unknown result type (might be due to invalid IL or missing references)
			//IL_0397: Expected O, but got Unknown
			//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c1: Expected O, but got Unknown
			//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03eb: Expected O, but got Unknown
			//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0407: Unknown result type (might be due to invalid IL or missing references)
			//IL_0411: Unknown result type (might be due to invalid IL or missing references)
			//IL_0417: Expected O, but got Unknown
			//IL_0467: Unknown result type (might be due to invalid IL or missing references)
			//IL_046d: Expected O, but got Unknown
			//IL_0480: Unknown result type (might be due to invalid IL or missing references)
			//IL_0486: Expected O, but got Unknown
			//IL_0499: Unknown result type (might be due to invalid IL or missing references)
			//IL_049f: Expected O, but got Unknown
			//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b8: Expected O, but got Unknown
			//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04dc: Expected O, but got Unknown
			//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ff: Expected O, but got Unknown
			//IL_0512: Unknown result type (might be due to invalid IL or missing references)
			//IL_0518: Expected O, but got Unknown
			//IL_052b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0530: Unknown result type (might be due to invalid IL or missing references)
			//IL_0540: Unknown result type (might be due to invalid IL or missing references)
			//IL_0545: Unknown result type (might be due to invalid IL or missing references)
			//IL_054b: Expected O, but got Unknown
			//IL_0577: Unknown result type (might be due to invalid IL or missing references)
			//IL_057d: Expected O, but got Unknown
			//IL_058f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0594: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05af: Expected O, but got Unknown
			//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c7: Expected O, but got Unknown
			//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f2: Expected O, but got Unknown
			//IL_0604: Unknown result type (might be due to invalid IL or missing references)
			//IL_060a: Expected O, but got Unknown
			//IL_061c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0621: Unknown result type (might be due to invalid IL or missing references)
			//IL_062d: Expected O, but got Unknown
			//IL_063f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0645: Expected O, but got Unknown
			//IL_0657: Unknown result type (might be due to invalid IL or missing references)
			//IL_065c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0668: Expected O, but got Unknown
			//IL_067a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0680: Expected O, but got Unknown
			//IL_0693: Unknown result type (might be due to invalid IL or missing references)
			//IL_0698: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a4: Expected O, but got Unknown
			//IL_06b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_06bd: Expected O, but got Unknown
			//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e1: Expected O, but got Unknown
			//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_06fa: Expected O, but got Unknown
			//IL_070d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0712: Unknown result type (might be due to invalid IL or missing references)
			//IL_071e: Expected O, but got Unknown
			//IL_072b: Unknown result type (might be due to invalid IL or missing references)
			//IL_073a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0744: Unknown result type (might be due to invalid IL or missing references)
			//IL_074a: Expected O, but got Unknown
			//IL_075d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0763: Expected O, but got Unknown
			//IL_0770: Unknown result type (might be due to invalid IL or missing references)
			//IL_077f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0789: Unknown result type (might be due to invalid IL or missing references)
			//IL_078f: Expected O, but got Unknown
			//IL_07a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b8: Expected O, but got Unknown
			//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d1: Expected O, but got Unknown
			((FContainer)Futile.stage).AddChild((FNode)(object)foodBar);
			circleSprites = (FSprite[])(object)new FSprite[20];
			pipSprites = (FSprite[])(object)new FSprite[20];
			staff = new FSprite("pixel", true)
			{
				scaleX = 2f,
				scaleY = 34.5f,
				y = 230f
			};
			for (int i = 0; i < 20; i++)
			{
				pipSprites[i] = new FSprite(Futile.atlasManager.GetElementWithName("FoodCircleB"), true)
				{
					y = 230f
				};
				circleSprites[i] = new FSprite(Futile.atlasManager.GetElementWithName("FoodCircleA"), true)
				{
					y = 230f
				};
				foodBar.AddChild((FNode)(object)pipSprites[i]);
				foodBar.AddChild((FNode)(object)circleSprites[i]);
			}
			foodBar.AddChild((FNode)(object)staff);
			OpTab val = new OpTab((OptionInterface)(object)this, "Options");
			OpTab val2 = new OpTab((OptionInterface)(object)this, "Modify");
			base.Tabs = (OpTab[])(object)new OpTab[2] { val, val2 };
			foodGroup = new OpRadioButtonGroup(foodOption);
			foodGroup.SetButtons((OpRadioButton[])(object)new OpRadioButton[3]
			{
				new OpRadioButton(100f, 403f)
				{
					description = "All food meters are scaled down to what might be expected from that slugcat as a pup.\nAs the basis of the scaling, survivor has the same food meter as a base slugpup."
				},
				new OpRadioButton(300f, 403f)
				{
					description = "The food meter for the current slugcat will remain unaltered."
				},
				new OpRadioButton(500f, 403f)
				{
					description = "Use the base slugpup food meter.\nThe player will need their bar at least two thirds full to hibernate."
				}
			});
			statGroup = new OpRadioButtonGroup(statsOption);
			statGroup.SetButtons((OpRadioButton[])(object)new OpRadioButton[3]
			{
				new OpRadioButton(100f, 303f)
				{
					description = "All stats are scaled down to what might be expected from that slugcat as a pup.\nAs the basis of the scaling, survivor has the same stats as an non-randomized base slugpup."
				},
				new OpRadioButton(300f, 303f)
				{
					description = "The stats of the current slugcat will remain unaltered.\nNote that, as it isn't a stat, jumping will still be worse. This can only be changed using cosmetic mode."
				},
				new OpRadioButton(500f, 303f)
				{
					description = "All slugcats will have the same stats as a a base non-randomized slugpup.\nNon-randomized stats (put very simply) are the middle value of each stat in the range that a slugpup could have for it. Not being randomized is not a penalty."
				}
			});
			OpUpdown val3 = new OpUpdown(foodToHibernate, new Vector2(160f, 152f), 60f, (byte)0)
			{
				description = "How much must be consumed before hibernation is allowed?",
				_fMax = 20f,
				_fMin = 1f
			};
			OpUpdown val4 = new OpUpdown(maxFood, new Vector2(390f, 152f), 60f, (byte)0)
			{
				description = "How much may be consumed in total?",
				_fMax = 20f,
				_fMin = 1f
			};
			UIArrPlayerOptions = (UIelement[])(object)new UIelement[28]
			{
				(UIelement)new OpLabel(10f, 550f, "Options", true),
				(UIelement)new OpLabel(10f, 520f, "Cosmetic Mode ", false)
				{
					description = "There will be no gameplay alterations. You just look like a slugpup."
				},
				(UIelement)new OpCheckBox(onlyCosmetic, 105f, 520f),
				(UIelement)new OpLabel(10f, 450f, "Food Values:", true),
				(UIelement)new OpRect(new Vector2(30f, 390f), new Vector2(525f, 50f), 0.3f),
				(UIelement)new OpLabel(50f, 403f, "Scaled", false),
				(UIelement)foodGroup,
				(UIelement)foodGroup.buttons[0],
				(UIelement)new OpLabel(225f, 403f, "Base Game", false),
				(UIelement)foodGroup.buttons[1],
				(UIelement)new OpLabel(440f, 403f, "Slugpup", false),
				(UIelement)foodGroup.buttons[2],
				(UIelement)new OpLabel(10f, 350f, "Stat Values:", true),
				(UIelement)new OpRect(new Vector2(30f, 290f), new Vector2(525f, 50f), 0.3f),
				(UIelement)statGroup,
				(UIelement)statGroup.buttons[0],
				(UIelement)statGroup.buttons[1],
				(UIelement)statGroup.buttons[2],
				(UIelement)new OpLabel(50f, 303f, "Scaled", false),
				(UIelement)new OpLabel(225f, 303f, "Base Game", false),
				(UIelement)new OpLabel(440f, 303f, "Slugpup", false),
				(UIelement)new OpLabel(10f, 200f, "Override Food Values:", true),
				(UIelement)new OpCheckBox(overrideFood, 230f, 200f)
				{
					description = "Recommended for Coop \n Allows you to customize the limits of the black hole in a slugcats stomach.\nNote: This will apply even if you are an adult."
				},
				(UIelement)val3,
				(UIelement)val4,
				(UIelement)new OpLabel(50f, 160f, "Hibernation Limit", false),
				(UIelement)new OpLabel(310f, 160f, "Meter Limit", false),
				(UIelement)new OpLabel(10f, 400f, "Great! You don't need any other options.", true)
				{
					color = new Color(0.85f, 0.2f, 0.4f)
				}
			};
			val.AddItems(UIArrPlayerOptions);
			UIArrPlayerModify = (UIelement[])(object)new UIelement[19]
			{
				(UIelement)new OpLabel(295f, 420f, "All options will only affect slugcats who naturally \nhave access to these abilities already.\n\nThese will not give other slugcats new abilities.", false),
				(UIelement)new OpLabel(295f, 420f, "You are currently in cosmetic mode. \n\nNone of these options will have any effect on \nthe game.", false)
				{
					color = new Color(0.85f, 0.2f, 0.4f)
				},
				(UIelement)new OpLabel(10f, 570f, "Modify", true),
				(UIelement)new OpRect(new Vector2(0f, 223f), new Vector2(257f, 340f), 0.3f),
				(UIelement)new OpLabel(15f, 500f, "Stomach Items", false),
				(UIelement)new OpCheckBox(letStomach, 120f, 500f)
				{
					description = "Access Stomach for storage & crafting. \nWarning: Will trap current item in stomach if turned off."
				},
				(UIelement)new OpLabel(15f, 440f, "Carnivorous Diet", false),
				(UIelement)new OpCheckBox(letEatMeat, 120f, 440f)
				{
					description = "What kind of child are you? You aren't meant to eat vultures."
				},
				(UIelement)new OpLabel(15f, 380f, "Mauling", false),
				(UIelement)new OpCheckBox(letMaul, 120f, 380f)
				{
					description = "This kitty(slug) has claws."
				},
				(UIelement)new OpLabel(15f, 320f, "Hold Hands", false),
				(UIelement)new OpCheckBox(holdHands, 120f, 320f)
				{
					description = "Seek companionship in this harsh world.\nWarning: Looks weird, but worth it for those invested."
				},
				(UIelement)new OpLabel(15f, 260f, "Pickup Slugpups", false),
				(UIelement)new OpCheckBox(letPickup, 120f, 260f)
				{
					description = "Time for some piggyback rides.\nNote: Requires \"Hold Hands\" to work."
				},
				(UIelement)new OpRect(new Vector2(282f, 375f), new Vector2(300f, 188f), 0.3f),
				(UIelement)new OpLabel(295f, 510f, "Slugpups generally lose access to major abilities. \n\n\nThey can be re-enabled here.", false),
				(UIelement)new OpRect(new Vector2(282f, 223f), new Vector2(196f, 105f), 0.3f),
				(UIelement)new OpCheckBox(bothHands, new Vector2(368f, 260f))
				{
					description = "Unlock the use of your second set of digits."
				},
				(UIelement)new OpLabel(298f, 295f, "Allow Two Hands", true)
			};
			val2.AddItems(UIArrPlayerModify);
		}

		public override void Update()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_032c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0336: Expected O, but got Unknown
			//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_0313: Unknown result type (might be due to invalid IL or missing references)
			//IL_0413: Unknown result type (might be due to invalid IL or missing references)
			//IL_041d: Expected O, but got Unknown
			int num = (int)Mathf.Round(((OpTextBox)(OpUpdown)UIArrPlayerOptions[24]).valueFloat);
			int num2 = (int)Mathf.Round(((OpTextBox)(OpUpdown)UIArrPlayerOptions[23]).valueFloat);
			if (!UIArrPlayerOptions[0].IsInactive && UIArrPlayerOptions[UIArrPlayerOptions.Length - 1].IsInactive)
			{
				((FNode)foodBar).isVisible = true;
				((FNode)staff).x = 555 + 30 * num2 + 300 - 15 * num;
				for (int i = 0; i < 20; i++)
				{
					if (i >= num)
					{
						((FNode)circleSprites[i]).isVisible = false;
						((FNode)pipSprites[i]).isVisible = false;
						continue;
					}
					((FNode)circleSprites[i]).isVisible = true;
					((FNode)circleSprites[i]).x = 570 + 30 * i + 300 - 15 * num;
					if (i < num2)
					{
						FSprite obj = circleSprites[i];
						((FNode)obj).x = ((FNode)obj).x - 8f;
					}
					else
					{
						FSprite obj2 = circleSprites[i];
						((FNode)obj2).x = ((FNode)obj2).x + 8f;
					}
					if (i < num2)
					{
						((FNode)pipSprites[i]).isVisible = true;
						((FNode)pipSprites[i]).x = 570 + 30 * i + 300 - 15 * num - 8;
					}
					else
					{
						((FNode)pipSprites[i]).isVisible = false;
					}
				}
			}
			else
			{
				((FNode)foodBar).isVisible = false;
			}
			((UIelement)(OpUpdown)UIArrPlayerOptions[23]).InScrollBox = false;
			((UIelement)(OpUpdown)UIArrPlayerOptions[24]).InScrollBox = false;
			if (((UIelement)(OpUpdown)UIArrPlayerOptions[23]).MouseOver && ((OpTextBox)(OpUpdown)UIArrPlayerOptions[23]).valueFloat > ((OpTextBox)(OpUpdown)UIArrPlayerOptions[24]).valueFloat)
			{
				((OpUpdown)UIArrPlayerOptions[24]).SetValueFloat(((OpTextBox)(OpUpdown)UIArrPlayerOptions[23]).valueFloat);
			}
			else if (((UIelement)(OpUpdown)UIArrPlayerOptions[24]).MouseOver && ((OpTextBox)(OpUpdown)UIArrPlayerOptions[24]).valueFloat < ((OpTextBox)(OpUpdown)UIArrPlayerOptions[23]).valueFloat)
			{
				((OpUpdown)UIArrPlayerOptions[23]).SetValueFloat(((OpTextBox)(OpUpdown)UIArrPlayerOptions[24]).valueFloat);
			}
			else if (((OpTextBox)(OpUpdown)UIArrPlayerOptions[23]).valueFloat > ((OpTextBox)(OpUpdown)UIArrPlayerOptions[24]).valueFloat)
			{
				((OpUpdown)UIArrPlayerOptions[24]).SetValueFloat(((OpTextBox)(OpUpdown)UIArrPlayerOptions[23]).valueFloat);
			}
			if (ValueExt.GetValueBool((IValueBool)(OpCheckBox)UIArrPlayerOptions[2]))
			{
				for (int j = 3; j < UIArrPlayerOptions.Length; j++)
				{
					UIArrPlayerOptions[j].Hide();
				}
				UIArrPlayerModify[1].Show();
				UIArrPlayerModify[0].Hide();
				UIArrPlayerOptions[UIArrPlayerOptions.Length - 1].Show();
			}
			else
			{
				for (int k = 3; k < UIArrPlayerOptions.Length; k++)
				{
					UIArrPlayerOptions[k].Show();
				}
				UIArrPlayerModify[1].Hide();
				UIArrPlayerModify[0].Show();
				UIArrPlayerOptions[UIArrPlayerOptions.Length - 1].Hide();
			}
			for (int l = 3; l <= 11; l++)
			{
				if (ValueExt.GetValueBool((IValueBool)(OpCheckBox)UIArrPlayerOptions[22]))
				{
					UIelement obj3 = UIArrPlayerOptions[l];
					UIfocusable val = (UIfocusable)(object)((obj3 is UIfocusable) ? obj3 : null);
					if (val != null)
					{
						val.greyedOut = true;
					}
				}
				else
				{
					UIelement obj4 = UIArrPlayerOptions[l];
					UIfocusable val2 = (UIfocusable)(object)((obj4 is UIfocusable) ? obj4 : null);
					if (val2 != null)
					{
						val2.greyedOut = false;
					}
				}
			}
		}

		public static void TurnOffFoodBar()
		{
			FContainer val = foodBar;
			if (val != null && ((FNode)val).isVisible)
			{
				((FNode)foodBar).isVisible = false;
				Futile.atlasManager.UnloadAtlas("pixel");
				Futile.atlasManager.UnloadAtlas("FoodCircleA");
				Futile.atlasManager.UnloadAtlas("FoodCircleB");
			}
		}
	}
	[BepInPlugin("elumenix.pupify", "Pupify", "1.0.0")]
	public class Plugin : BaseUnityPlugin
	{
		public static Options options;

		public static SlugcatStats currentSlugcat;

		public static bool playersCreated;

		public static bool foodBarsEnabled;

		private bool IsInit;

		public Plugin()
		{
			try
			{
				options = new Options(this, ((BaseUnityPlugin)this).Logger);
			}
			catch (Exception ex)
			{
				((BaseUnityPlugin)this).Logger.LogError((object)ex);
				throw;
			}
		}

		private void OnEnable()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			RainWorld.OnModsInit += new hook_OnModsInit(RainWorld_OnModsInit);
		}

		private void RainWorld_OnModsInit(orig_OnModsInit orig, RainWorld self)
		{
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Expected O, but got Unknown
			orig.Invoke(self);
			try
			{
				if (IsInit)
				{
					return;
				}
				MiscHooks.saved = new List<FAtlas>
				{
					Futile.atlasManager.LoadImage("atlases/face_pup_off"),
					Futile.atlasManager.LoadImage("atlases/face_pup_on"),
					Futile.atlasManager.LoadImage("atlases/pup_off"),
					Futile.atlasManager.LoadImage("atlases/pup_on")
				};
				RainWorldGame.ShutDownProcess += new hook_ShutDownProcess(RainWorldGame_ShutDownProcess);
				GameSession.ctor += new hook_ctor(GameSession_ctor);
				PlayerHooks.Init();
				SceneHooks.Init();
				MiscHooks.Init();
				MultiPlayer.Init();
				MenuHooks.Init();
				MachineConnector.SetRegisteredOI("elumenix.pupify", (OptionInterface)(object)options);
				IsInit = true;
			}
			catch (Exception ex)
			{
				((BaseUnityPlugin)this).Logger.LogError((object)ex);
				throw;
			}
			foreach (Mod activeMod in ModManager.ActiveMods)
			{
				if (activeMod.id == "sprobgik.individualfoodbars")
				{
					foodBarsEnabled = true;
				}
			}
		}

		private void Player_Update(orig_Update orig, Player self, bool eu)
		{
			orig.Invoke(self, eu);
		}

		private void RainWorldGame_ShutDownProcess(orig_ShutDownProcess orig, RainWorldGame self)
		{
			orig.Invoke(self);
			ClearMemory();
		}

		private void GameSession_ctor(orig_ctor orig, GameSession self, RainWorldGame game)
		{
			orig.Invoke(self, game);
			ClearMemory();
		}

		private void ClearMemory()
		{
		}

		public static bool MakeChanges(Player self = null, int id = -1)
		{
			if (MultiPlayer.makePup == null || (self != null && self.isNPC))
			{
				return false;
			}
			if (!options.onlyCosmetic.Value && (!(MultiPlayer.Session is StoryGameSession) || ModManager.CoopAvailable || MenuHooks.pupButton == null || ((SymbolButtonToggle)MenuHooks.pupButton).isToggled) && (self == null || MultiPlayer.Session is StoryGameSession || MultiPlayer.makePup[((AbstractWorldEntity)((Creature)self).abstractCreature).ID.number]) && (id == -1 || MultiPlayer.Session is StoryGameSession || MultiPlayer.makePup[id]))
			{
				if (id != -1)
				{
					GameSession session = MultiPlayer.Session;
					StoryGameSession val = (StoryGameSession)(object)((session is StoryGameSession) ? session : null);
					if (val != null && ModManager.CoopAvailable && !((GameSession)val).game.rainWorld.options.jollyPlayerOptionsArray[id].isPup)
					{
						goto IL_0137;
					}
				}
				if (self != null)
				{
					GameSession session2 = MultiPlayer.Session;
					StoryGameSession val2 = (StoryGameSession)(object)((session2 is StoryGameSession) ? session2 : null);
					if (val2 != null && ModManager.CoopAvailable && !((GameSession)val2).game.rainWorld.options.jollyPlayerOptionsArray[((AbstractWorldEntity)((Creature)self).abstractCreature).ID.number].isPup)
					{
						goto IL_0137;
					}
				}
				return true;
			}
			goto IL_0137;
			IL_0137:
			return false;
		}
	}
}
namespace Pupify.Hooks
{
	public static class MenuHooks
	{
		[CompilerGenerated]
		private static class <>O
		{
			public static hook_ctor <0>__ArenaSettingsInterface_ctor;

			public static hook_InitiateGameTypeSpecificButtons <1>__MultiplayerMenu_InitiateGameTypeSpecificButtons;

			public static hook_ctor <2>__SlugcatSelectMenu_ctor;

			public static hook_Update <3>__SlugcatSelectMenu_Update;

			public static hook_Singal <4>__ModdingMenu_Singal;

			public static hook_ctor <5>__SlugcatPageContinue_ctor;
		}

		public static SymbolButtonTogglePupButton pupButton;

		private static bool previousState;

		public static bool arenaPreviousState;

		public static SymbolButtonTogglePupButton challengePupButton;

		public static void Init()
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
			object obj = <>O.<0>__ArenaSettingsInterface_ctor;
			if (obj == null)
			{
				hook_ctor val = ArenaSettingsInterface_ctor;
				<>O.<0>__ArenaSettingsInterface_ctor = val;
				obj = (object)val;
			}
			ArenaSettingsInterface.ctor += (hook_ctor)obj;
			object obj2 = <>O.<1>__MultiplayerMenu_InitiateGameTypeSpecificButtons;
			if (obj2 == null)
			{
				hook_InitiateGameTypeSpecificButtons val2 = MultiplayerMenu_InitiateGameTypeSpecificButtons;
				<>O.<1>__MultiplayerMenu_InitiateGameTypeSpecificButtons = val2;
				obj2 = (object)val2;
			}
			MultiplayerMenu.InitiateGameTypeSpecificButtons += (hook_InitiateGameTypeSpecificButtons)obj2;
			object obj3 = <>O.<2>__SlugcatSelectMenu_ctor;
			if (obj3 == null)
			{
				hook_ctor val3 = SlugcatSelectMenu_ctor;
				<>O.<2>__SlugcatSelectMenu_ctor = val3;
				obj3 = (object)val3;
			}
			SlugcatSelectMenu.ctor += (hook_ctor)obj3;
			object obj4 = <>O.<3>__SlugcatSelectMenu_Update;
			if (obj4 == null)
			{
				hook_Update val4 = SlugcatSelectMenu_Update;
				<>O.<3>__SlugcatSelectMenu_Update = val4;
				obj4 = (object)val4;
			}
			SlugcatSelectMenu.Update += (hook_Update)obj4;
			object obj5 = <>O.<4>__ModdingMenu_Singal;
			if (obj5 == null)
			{
				hook_Singal val5 = ModdingMenu_Singal;
				<>O.<4>__ModdingMenu_Singal = val5;
				obj5 = (object)val5;
			}
			ModdingMenu.Singal += (hook_Singal)obj5;
			object obj6 = <>O.<5>__SlugcatPageContinue_ctor;
			if (obj6 == null)
			{
				hook_ctor val6 = SlugcatPageContinue_ctor;
				<>O.<5>__SlugcatPageContinue_ctor = val6;
				obj6 = (object)val6;
			}
			SlugcatPageContinue.ctor += (hook_ctor)obj6;
		}

		private static void ArenaSettingsInterface_ctor(orig_ctor orig, ArenaSettingsInterface self, Menu menu, MenuObject owner)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Expected O, but got Unknown
			orig.Invoke(self, menu, owner);
			MenuLabel val = new MenuLabel(menu, owner, "Right click on their profile to Pupify a slugcat:", new Vector2(746f, 590f), new Vector2(350f, 20f), true, (FTextParams)null);
			val.label.color = Color.red;
			MenuLabel item = val;
			((MenuObject)self).subObjects.Add((MenuObject)(object)item);
		}

		private static void MultiplayerMenu_InitiateGameTypeSpecificButtons(orig_InitiateGameTypeSpecificButtons orig, MultiplayerMenu self)
		{
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Expected O, but got Unknown
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Expected O, but got Unknown
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Expected O, but got Unknown
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Expected O, but got Unknown
			orig.Invoke(self);
			if ((ExtEnum<GameTypeID>)(object)self.currentGameType == (ExtEnum<GameTypeID>)(object)GameTypeID.Challenge)
			{
				if (challengePupButton == null)
				{
					bool flag = MultiPlayer.makePup.Count >= 1 && MultiPlayer.makePup[0];
					if (MultiPlayer.makePup.Count == 0)
					{
						if (pupButton != null)
						{
							flag = ((SymbolButtonToggle)pupButton).isToggled;
						}
						MultiPlayer.makePup.Add(flag);
					}
					if (!ModManager.JollyCoop)
					{
						challengePupButton = new SymbolButtonTogglePupButton((Menu)(object)self, ((Menu)self).backObject, "toggle_pup_0", new Vector2(920f, 500f), new Vector2(45f, 45f), "atlases/pup_on", "atlases/pup_off", flag, (string)null, (string)null);
					}
					else
					{
						challengePupButton = new SymbolButtonTogglePupButton((Menu)(object)self, ((Menu)self).backObject, "toggle_pup_0", new Vector2(920f, 500f), new Vector2(45f, 45f), "pup_on", "pup_off", flag, (string)null, (string)null);
					}
					((Menu)self).backObject.subObjects.Add((MenuObject)(object)challengePupButton);
					arenaPreviousState = ((SymbolButtonToggle)challengePupButton).isToggled;
				}
				else
				{
					bool flag2 = MultiPlayer.makePup.Count >= 1 && MultiPlayer.makePup[0];
					if (!ModManager.JollyCoop)
					{
						challengePupButton = new SymbolButtonTogglePupButton((Menu)(object)self, ((Menu)self).backObject, "toggle_pup_0", new Vector2(920f, 500f), new Vector2(45f, 45f), "atlases/pup_on", "atlases/pup_off", flag2, (string)null, (string)null);
					}
					else
					{
						challengePupButton = new SymbolButtonTogglePupButton((Menu)(object)self, ((Menu)self).backObject, "toggle_pup_0", new Vector2(920f, 500f), new Vector2(45f, 45f), "pup_on", "pup_off", flag2, (string)null, (string)null);
					}
					((Menu)self).backObject.subObjects.Add((MenuObject)(object)challengePupButton);
					arenaPreviousState = ((SymbolButtonToggle)challengePupButton).isToggled;
				}
			}
			else if (challengePupButton != null)
			{
				((PositionedMenuObject)challengePupButton).pos.x = 3000f;
			}
		}

		private static void SlugcatSelectMenu_ctor(orig_ctor orig, SlugcatSelectMenu self, ProcessManager manager)
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Expected O, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			orig.Invoke(self, manager);
			bool flag = pupButton == null || ((SymbolButtonToggle)pupButton).isToggled;
			if (!ModManager.JollyCoop)
			{
				pupButton = new SymbolButtonTogglePupButton((Menu)(object)self, ((Menu)self).backObject, "toggle_pup_0", new Vector2(890f, -10f), new Vector2(45f, 45f), "atlases/pup_on", "atlases/pup_off", flag, (string)null, (string)null);
			}
			else
			{
				pupButton = new SymbolButtonTogglePupButton((Menu)(object)self, ((Menu)self).backObject, "toggle_pup_0", new Vector2(890f, -10f), new Vector2(45f, 45f), "pup_on", "pup_off", flag, (string)null, (string)null);
			}
			((Menu)self).backObject.subObjects.Add((MenuObject)(object)pupButton);
			previousState = ((SymbolButtonToggle)pupButton).isToggled;
		}

		private static void SlugcatSelectMenu_Update(orig_Update orig, SlugcatSelectMenu self)
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Expected O, but got Unknown
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Expected O, but got Unknown
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Expected O, but got Unknown
			orig.Invoke(self);
			bool jollyCoop = ModManager.JollyCoop;
			bool flag = jollyCoop;
			if (flag)
			{
				int slugcatPageIndex = self.slugcatPageIndex;
				bool flag2 = (uint)slugcatPageIndex <= 2u;
				flag = flag2;
			}
			if (flag)
			{
				((PositionedMenuObject)pupButton).pos.x = -6000f;
				((PositionedMenuObject)pupButton).pos.y = 3000f;
			}
			else
			{
				((PositionedMenuObject)pupButton).pos.x = 890f;
				((PositionedMenuObject)pupButton).pos.y = -10f;
			}
			pupButton.faceSymbol.sprite.color = Color.black;
			if (((SymbolButtonToggle)pupButton).isToggled != previousState)
			{
				((Menu)self).PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
				for (int i = 0; i < self.slugcatColorOrder.Count; i++)
				{
					if (self.saveGameData[self.slugcatColorOrder[i]] != null)
					{
						((MenuObject)self.slugcatPages[i]).RemoveSprites();
						self.slugcatPages[i] = (SlugcatPage)new SlugcatPageContinue((Menu)(object)self, (MenuObject)null, i + 1, self.slugcatColorOrder[i]);
					}
					else
					{
						((MenuObject)self.slugcatPages[i]).RemoveSprites();
						self.slugcatPages[i] = (SlugcatPage)new SlugcatPageNewGame((Menu)(object)self, (MenuObject)null, i + 1, self.slugcatColorOrder[i]);
					}
					((Menu)self).pages[i + 1] = (Page)(object)self.slugcatPages[i];
				}
				((MenuObject)((Menu)self).pages[0]).RemoveSubObject((MenuObject)(object)self.startButton);
				((MenuObject)self.startButton).RemoveSprites();
				self.startButton = new HoldButton((Menu)(object)self, (MenuObject)(object)((Menu)self).pages[0], "", "START", new Vector2(683f, 85f), 40f);
				((MenuObject)((Menu)self).pages[0]).subObjects.Add((MenuObject)(object)self.startButton);
				self.UpdateStartButtonText();
			}
			previousState = ((SymbolButtonToggle)pupButton).isToggled;
		}

		private static void ModdingMenu_Singal(orig_Singal orig, ModdingMenu self, MenuObject sender, string message)
		{
			orig.Invoke(self, sender, message);
			Options.TurnOffFoodBar();
		}

		private static void SlugcatPageContinue_ctor(orig_ctor orig, SlugcatPageContinue self, Menu menu, MenuObject owner, int pageIndex, Name slugcatNumber)
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Expected O, but got Unknown
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Expected O, but got Unknown
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Expected O, but got Unknown
			//IL_0375: Unknown result type (might be due to invalid IL or missing references)
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_038f: Expected O, but got Unknown
			//IL_0480: Unknown result type (might be due to invalid IL or missing references)
			//IL_048d: Unknown result type (might be due to invalid IL or missing references)
			//IL_049d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0592: Unknown result type (might be due to invalid IL or missing references)
			//IL_0450: Unknown result type (might be due to invalid IL or missing references)
			//IL_045a: Expected O, but got Unknown
			//IL_0690: Unknown result type (might be due to invalid IL or missing references)
			//IL_069a: Expected O, but got Unknown
			//IL_05f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0604: Unknown result type (might be due to invalid IL or missing references)
			//IL_0610: Unknown result type (might be due to invalid IL or missing references)
			//IL_061a: Expected O, but got Unknown
			//IL_0551: Unknown result type (might be due to invalid IL or missing references)
			//IL_055b: Expected O, but got Unknown
			//IL_07dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_07eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_07f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0809: Expected O, but got Unknown
			if (Plugin.options.onlyCosmetic.Value || (pupButton != null && !((SymbolButtonToggle)pupButton).isToggled && !Plugin.options.overrideFood.Value))
			{
				orig.Invoke(self, menu, owner, pageIndex, slugcatNumber);
				return;
			}
			((MenuObject)self).menu = menu;
			((MenuObject)self).owner = owner;
			((MenuObject)self).subObjects = new List<MenuObject>();
			((MenuObject)self).nextSelectable = (MenuObject[])(object)new MenuObject[4];
			((PositionedMenuObject)self).pos = new Vector2(1f / 3f, 1f / 3f);
			((PositionedMenuObject)self).lastPos = new Vector2(1f / 3f, 1f / 3f);
			((Page)self).name = "Slugcat_Page_" + (((ExtEnum<Name>)(object)slugcatNumber != (ExtEnum<Name>)null) ? ((object)slugcatNumber).ToString() : null);
			((Page)self).index = pageIndex;
			((Page)self).selectables = new List<SelectableMenuObject>();
			((Page)self).mouseCursor = new MouseCursor(menu, (MenuObject)(object)self, new Vector2(-100f, -100f));
			((MenuObject)self).subObjects.Add((MenuObject)(object)((Page)self).mouseCursor);
			((SlugcatPage)self).slugcatNumber = slugcatNumber;
			((SlugcatPage)self).effectColor = PlayerGraphics.DefaultSlugcatColor(slugcatNumber);
			if ((ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)Name.Red)
			{
				((SlugcatPage)self).effectColor = Color.Lerp(((SlugcatPage)self).effectColor, Color.red, 0.2f);
			}
			if (ModManager.MSC && self.saveGameData.altEnding && (((ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)Name.White && ((MainLoopProcess)menu).manager.rainWorld.progression.miscProgressionData.survivorEndingID > 1) || ((ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)Name.Yellow && ((MainLoopProcess)menu).manager.rainWorld.progression.miscProgressionData.monkEndingID > 1) || ((ExtEnum<Name>)(object)slugcatNumber != (ExtEnum<Name>)(object)Name.White && (ExtEnum<Name>)(object)slugcatNumber != (ExtEnum<Name>)(object)Name.Yellow && (ExtEnum<Name>)(object)slugcatNumber != (ExtEnum<Name>)(object)Name.Red)))
			{
				((SlugcatPage)self).AddAltEndingImage();
			}
			else
			{
				((SlugcatPage)self).AddImage(self.saveGameData.ascended);
			}
			self.hudContainers = (FContainer[])(object)new FContainer[2];
			for (int i = 0; i < self.hudContainers.Length; i++)
			{
				self.hudContainers[i] = new FContainer();
				((MenuObject)self).Container.AddChild((FNode)(object)self.hudContainers[i]);
			}
			self.hud = new HUD(self.hudContainers, ((MainLoopProcess)menu).manager.rainWorld, (IOwnAHUD)(object)self);
			self.saveGameData.karma = Custom.IntClamp(self.saveGameData.karma, 0, self.saveGameData.karmaCap);
			if (ModManager.MSC && (ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)SlugcatStatsName.Artificer && self.saveGameData.altEnding && ((MainLoopProcess)menu).manager.rainWorld.progression.miscProgressionData.artificerEndingID != 1)
			{
				self.saveGameData.karma = 0;
				self.saveGameData.karmaCap = 0;
			}
			if (ModManager.MSC && (ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)SlugcatStatsName.Saint && self.saveGameData.ascended)
			{
				self.saveGameData.karma = 1;
				self.saveGameData.karmaCap = 1;
			}
			self.hud.AddPart((HudPart)new KarmaMeter(self.hud, self.hudContainers[1], new IntVector2(self.saveGameData.karma, self.saveGameData.karmaCap), self.saveGameData.karmaReinforced));
			if (Plugin.options.overrideFood.Value)
			{
				if ((ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)SlugcatStatsName.Artificer && Plugin.options.letEatMeat.Value && self.saveGameData.cycle == 0)
				{
					self.saveGameData.food = ((4 < Mathf.RoundToInt(Plugin.options.maxFood.Value)) ? 4 : Mathf.RoundToInt(Plugin.options.maxFood.Value));
				}
				self.hud.AddPart((HudPart)new FoodMeter(self.hud, Mathf.RoundToInt(Plugin.options.maxFood.Value), Mathf.RoundToInt(Plugin.options.foodToHibernate.Value), (Player)null, 0));
			}
			else if (Plugin.options.foodOption.Value == 0)
			{
				float num = (float)SlugcatStats.SlugcatFoodMeter(slugcatNumber).y / (float)SlugcatStats.SlugcatFoodMeter(slugcatNumber).x;
				int num2 = Mathf.RoundToInt((float)SlugcatStats.SlugcatFoodMeter(slugcatNumber).x * 0.42857143f);
				int num3 = Mathf.RoundToInt((float)num2 * num * 1.1666666f);
				if (num3 > num2)
				{
					num3 = num2;
				}
				self.saveGameData.food = Custom.IntClamp(self.saveGameData.food, 0, num2 - num3);
				if ((ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)SlugcatStatsName.Artificer && Plugin.options.letEatMeat.Value && self.saveGameData.cycle == 0)
				{
					self.saveGameData.food = 4;
				}
				self.hud.AddPart((HudPart)new FoodMeter(self.hud, num2, num3, (Player)null, 0));
			}
			else if (Plugin.options.foodOption.Value == 1)
			{
				self.saveGameData.food = Custom.IntClamp(self.saveGameData.food, 0, SlugcatStats.SlugcatFoodMeter(slugcatNumber).y);
				if ((ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)SlugcatStatsName.Artificer && Plugin.options.letEatMeat.Value && self.saveGameData.cycle == 0)
				{
					self.saveGameData.food = 4;
				}
				self.hud.AddPart((HudPart)new FoodMeter(self.hud, SlugcatStats.SlugcatFoodMeter(slugcatNumber).x, SlugcatStats.SlugcatFoodMeter(slugcatNumber).y, (Player)null, 0));
			}
			else
			{
				self.saveGameData.food = Custom.IntClamp(self.saveGameData.food, 0, 1);
				if ((ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)SlugcatStatsName.Artificer && Plugin.options.letEatMeat.Value && self.saveGameData.cycle == 0)
				{
					self.saveGameData.food = 3;
				}
				self.hud.AddPart((HudPart)new FoodMeter(self.hud, 3, 2, (Player)null, 0));
			}
			string text = "";
			string shelterName = self.saveGameData.shelterName;
			if (shelterName != null && shelterName.Length > 2)
			{
				text = Region.GetRegionFullName(self.saveGameData.shelterName.Substring(0, 2), slugcatNumber);
				if (text.Length > 0)
				{
					text = menu.Translate(text);
					text = text + " - " + menu.Translate("Cycle") + " " + (((ExtEnum<Name>)(object)slugcatNumber == (ExtEnum<Name>)(object)Name.Red) ? (RedsIllness.RedsCycles(self.saveGameData.redsExtraCycles) - self.saveGameData.cycle) : self.saveGameData.cycle);
					if (ModManager.MMF)
					{
						TimeSpan timeSpan = TimeSpan.FromSeconds((double)self.saveGameData.gameTimeAlive + (double)self.saveGameData.gameTimeDead);
						string text2 = text;
						TimeSpan timeSpan2 = timeSpan;
						text = text2 + " (" + timeSpan2.ToString() + ")";
					}
				}
			}
			MenuLabel val = new MenuLabel(menu, (MenuObject)(object)self, text, new Vector2(-1000f, ((SlugcatPage)self).imagePos.y - 249f), new Vector2(200f, 30f), true, (FTextParams)null);
			val.label.alignment = (FLabelAlignment)0;
			self.regionLabel = val;
			((MenuObject)self).subObjects.Add((MenuObject)(object)self.regionLabel);
		}
	}
	public static class MiscHooks
	{
		[CompilerGenerated]
		private static class <>O
		{
			public static hook_SafeWWWLoadTexture <0>__AssetManager_SafeWWWLoadTexture;

			public static hook_PostSwitchMainProcess <1>__ProcessManager_PostSwitchMainProcess;
		}

		public static List<FAtlas> saved;

		public static void Init()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			object obj = <>O.<0>__AssetManager_SafeWWWLoadTexture;
			if (obj == null)
			{
				hook_SafeWWWLoadTexture val = AssetManager_SafeWWWLoadTexture;
				<>O.<0>__AssetManager_SafeWWWLoadTexture = val;
				obj = (object)val;
			}
			AssetManager.SafeWWWLoadTexture += (hook_SafeWWWLoadTexture)obj;
			object obj2 = <>O.<1>__ProcessManager_PostSwitchMainProcess;
			if (obj2 == null)
			{
				hook_PostSwitchMainProcess val2 = ProcessManager_PostSwitchMainProcess;
				<>O.<1>__ProcessManager_PostSwitchMainProcess = val2;
				obj2 = (object)val2;
			}
			ProcessManager.PostSwitchMainProcess += (hook_PostSwitchMainProcess)obj2;
		}

		private static Texture2D AssetManager_SafeWWWLoadTexture(orig_SafeWWWLoadTexture orig, ref Texture2D texture2d, string path, bool clampWrapMode, bool crispPixels)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			if (path.Contains("face_atlases/pup_on"))
			{
				return (Texture2D)saved[1].texture;
			}
			if (path.Contains("face_atlases/pup_off"))
			{
				return (Texture2D)saved[0].texture;
			}
			return orig.Invoke(ref texture2d, path, clampWrapMode, crispPixels);
		}

		private static void ProcessManager_PostSwitchMainProcess(orig_PostSwitchMainProcess orig, ProcessManager self, ProcessID ID)
		{
			if (((ExtEnum<ProcessID>)(object)ID == (ExtEnum<ProcessID>)(object)ProcessID.Game && (ExtEnum<ProcessID>)(object)self.oldProcess.ID != (ExtEnum<ProcessID>)(object)ProcessID.MultiplayerMenu) || (ExtEnum<ProcessID>)(object)ID == (ExtEnum<ProcessID>)(object)ProcessID.MultiplayerMenu)
			{
				Plugin.currentSlugcat = null;
				Plugin.playersCreated = false;
				MultiPlayer.Session = null;
				MultiPlayer.onlyPupsLeft = false;
				MultiPlayer.startingIncrement = 0;
				MultiPlayer.currentIndex = 0;
				MultiPlayer.numAccessed = 0;
				MultiPlayer.playerToLoad = 0;
				MultiPlayer.reorganized = false;
				MultiPlayer.ClearPlayers();
				MenuHooks.challengePupButton = null;
			}
			orig.Invoke(self, ID);
		}
	}
	public static class PlayerHooks
	{
		[CompilerGenerated]
		private static class <>O
		{
			public static hook_ctor <0>__Player_ctor;

			public static hook_FreeHand <1>__Player_FreeHand;

			public static hook_GrabUpdate <2>__Player_GrabUpdate;

			public static hook_SlugcatGrab <3>__Player_SlugcatGrab;

			public static hook_Grabability <4>__Player_Grabability;

			public static hook_CanEatMeat <5>__Player_CanEatMeat;

			public static hook_GetInitialSlugcatClass <6>__Player_GetInitialSlugcatClass;

			public static hook_ctor <7>__SlugcatStats_ctor;

			public static hook_SetMalnourished <8>__Player_SetMalnourished;

			public static hook_DoorClosed <9>__ShelterDoor_DoorClosed;

			public static hook_ctor <10>__PlayerGraphics_ctor;

			public static hook_DrawSprites <11>__PlayerGraphics_DrawSprites;

			public static hook_ShortCutColor <12>__Player_ShortCutColor;

			public static hook_Update <13>__SlugcatHandOnUpdate;
		}

		public static void Init()
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
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Expected O, but got Unknown
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Expected O, but got Unknown
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Expected O, but got Unknown
			object obj = <>O.<0>__Player_ctor;
			if (obj == null)
			{
				hook_ctor val = Player_ctor;
				<>O.<0>__Player_ctor = val;
				obj = (object)val;
			}
			Player.ctor += (hook_ctor)obj;
			object obj2 = <>O.<1>__Player_FreeHand;
			if (obj2 == null)
			{
				hook_FreeHand val2 = Player_FreeHand;
				<>O.<1>__Player_FreeHand = val2;
				obj2 = (object)val2;
			}
			Player.FreeHand += (hook_FreeHand)obj2;
			object obj3 = <>O.<2>__Player_GrabUpdate;
			if (obj3 == null)
			{
				hook_GrabUpdate val3 = Player_GrabUpdate;
				<>O.<2>__Player_GrabUpdate = val3;
				obj3 = (object)val3;
			}
			Player.GrabUpdate += (hook_GrabUpdate)obj3;
			object obj4 = <>O.<3>__Player_SlugcatGrab;
			if (obj4 == null)
			{
				hook_SlugcatGrab val4 = Player_SlugcatGrab;
				<>O.<3>__Player_SlugcatGrab = val4;
				obj4 = (object)val4;
			}
			Player.SlugcatGrab += (hook_SlugcatGrab)obj4;
			object obj5 = <>O.<4>__Player_Grabability;
			if (obj5 == null)
			{
				hook_Grabability val5 = Player_Grabability;
				<>O.<4>__Player_Grabability = val5;
				obj5 = (object)val5;
			}
			Player.Grabability += (hook_Grabability)obj5;
			object obj6 = <>O.<5>__Player_CanEatMeat;
			if (obj6 == null)
			{
				hook_CanEatMeat val6 = Player_CanEatMeat;
				<>O.<5>__Player_CanEatMeat = val6;
				obj6 = (object)val6;
			}
			Player.CanEatMeat += (hook_CanEatMeat)obj6;
			object obj7 = <>O.<6>__Player_GetInitialSlugcatClass;
			if (obj7 == null)
			{
				hook_GetInitialSlugcatClass val7 = Player_GetInitialSlugcatClass;
				<>O.<6>__Player_GetInitialSlugcatClass = val7;
				obj7 = (object)val7;
			}
			Player.GetInitialSlugcatClass += (hook_GetInitialSlugcatClass)obj7;
			object obj8 = <>O.<7>__SlugcatStats_ctor;
			if (obj8 == null)
			{
				hook_ctor val8 = SlugcatStats_ctor;
				<>O.<7>__SlugcatStats_ctor = val8;
				obj8 = (object)val8;
			}
			SlugcatStats.ctor += (hook_ctor)obj8;
			object obj9 = <>O.<8>__Player_SetMalnourished;
			if (obj9 == null)
			{
				hook_SetMalnourished val9 = Player_SetMalnourished;
				<>O.<8>__Player_SetMalnourished = val9;
				obj9 = (object)val9;
			}
			Player.SetMalnourished += (hook_SetMalnourished)obj9;
			object obj10 = <>O.<9>__ShelterDoor_DoorClosed;
			if (obj10 == null)
			{
				hook_DoorClosed val10 = ShelterDoor_DoorClosed;
				<>O.<9>__ShelterDoor_DoorClosed = val10;
				obj10 = (object)val10;
			}
			ShelterDoor.DoorClosed += (hook_DoorClosed)obj10;
			object obj11 = <>O.<10>__PlayerGraphics_ctor;
			if (obj11 == null)
			{
				hook_ctor val11 = PlayerGraphics_ctor;
				<>O.<10>__PlayerGraphics_ctor = val11;
				obj11 = (object)val11;
			}
			PlayerGraphics.ctor += (hook_ctor)obj11;
			object obj12 = <>O.<11>__PlayerGraphics_DrawSprites;
			if (obj12 == null)
			{
				hook_DrawSprites val12 = PlayerGraphics_DrawSprites;
				<>O.<11>__PlayerGraphics_DrawSprites = val12;
				obj12 = (object)val12;
			}
			PlayerGraphics.DrawSprites += (hook_DrawSprites)obj12;
			object obj13 = <>O.<12>__Player_ShortCutColor;
			if (obj13 == null)
			{
				hook_ShortCutColor val13 = Player_ShortCutColor;
				<>O.<12>__Player_ShortCutColor = val13;
				obj13 = (object)val13;
			}
			Player.ShortCutColor += (hook_ShortCutColor)obj13;
			object obj14 = <>O.<13>__SlugcatHandOnUpdate;
			if (obj14 == null)
			{
				hook_Update val14 = SlugcatHandOnUpdate;
				<>O.<13>__SlugcatHandOnUpdate = val14;
				obj14 = (object)val14;
			}
			SlugcatHand.Update += (hook_Update)obj14;
			Harmony val15 = new Harmony("PupifyHarmony");
			MethodInfo methodInfo = typeof(Player).GetProperty("isSlugpup")?.GetGetMethod();
			MethodInfo method = typeof(PlayerHooks).GetMethod("Player_isSlugpup");
			val15.Patch((MethodBase)methodInfo, new HarmonyMethod(method), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		}

		private static void Player_ctor(orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
		{
			//IL_0311: Unknown result type (might be due to invalid IL or missing references)
			//IL_031b: Expected O, but got Unknown
			orig.Invoke(self, abstractCreature, world);
			if (Plugin.options.onlyCosmetic.Value && !self.isNPC && ((!(MultiPlayer.Session is StoryGameSession) && MultiPlayer.makePup[((AbstractWorldEntity)((Creature)self).abstractCreature).ID.number]) || (MultiPlayer.Session is StoryGameSession && !ModManager.CoopAvailable && MenuHooks.pupButton != null && ((SymbolButtonToggle)MenuHooks.pupButton).isToggled)))
			{
				self.setPupStatus(true);
			}
			if (Plugin.options.onlyCosmetic.Value)
			{
				return;
			}
			if (self.isNPC)
			{
				SlugcatStats slugcatStats = self.slugcatStats;
				slugcatStats.runspeedFac *= 0.85f + 0.15f * self.npcStats.Met + 0.15f * (1f - self.npcStats.Bal) + 0.1f * (1f - self.npcStats.Stealth);
				SlugcatStats slugcatStats2 = self.slugcatStats;
				slugcatStats2.bodyWeightFac *= 0.85f + 0.15f * self.npcStats.Wideness + 0.1f * self.npcStats.Met;
				SlugcatStats slugcatStats3 = self.slugcatStats;
				slugcatStats3.generalVisibilityBonus *= 0.8f + 0.2f * (1f - self.npcStats.Stealth) + 0.2f * self.npcStats.Met;
				SlugcatStats slugcatStats4 = self.slugcatStats;
				slugcatStats4.visualStealthInSneakMode *= 0.75f + 0.35f * self.npcStats.Stealth + 0.15f * (1f - self.npcStats.Met);
				SlugcatStats slugcatStats5 = self.slugcatStats;
				slugcatStats5.loudnessFac *= 0.8f + 0.2f * self.npcStats.Wideness + 0.2f * (1f - self.npcStats.Stealth);
				SlugcatStats slugcatStats6 = self.slugcatStats;
				slugcatStats6.lungsFac *= 0.8f + 0.2f * (1f - self.npcStats.Met) + 0.2f * (1f - self.npcStats.Stealth);
				SlugcatStats slugcatStats7 = self.slugcatStats;
				slugcatStats7.poleClimbSpeedFac *= 0.85f + 0.15f * self.npcStats.Met + 0.15f * self.npcStats.Bal + 0.1f * (1f - self.npcStats.Stealth);
				SlugcatStats slugcatStats8 = self.slugcatStats;
				slugcatStats8.corridorClimbSpeedFac *= 0.85f + 0.15f * self.npcStats.Met + 0.15f * (1f - self.npcStats.Bal) + 0.1f * (1f - self.npcStats.Stealth);
			}
			else
			{
				if (Plugin.options.letPickup.Value)
				{
					self.slugOnBack = new SlugOnBack(self);
				}
				if (self.playerState.foodInStomach > self.slugcatStats.maxFood - self.slugcatStats.foodToHibernate)
				{
					self.playerState.foodInStomach = self.slugcatStats.maxFood - self.slugcatStats.foodToHibernate;
				}
			}
		}

		private static int Player_FreeHand(orig_FreeHand orig, Player self)
		{
			if (Plugin.MakeChanges(self) && Plugin.options.bothHands.Value && ((ExtEnumBase)self.SlugCatClass).value != "Slugpup")
			{
				if (((Creature)self).grasps[0] != null && self.HeavyCarry(((Creature)self).grasps[0].grabbed))
				{
					return -1;
				}
				for (int i = 0; i < ((Creature)self).grasps.Length; i++)
				{
					if (((Creature)self).grasps[i] == null)
					{
						return i;
					}
				}
				return -1;
			}
			return orig.Invoke(self);
		}

		private static void Player_GrabUpdate(orig_GrabUpdate orig, Player self, bool eu)
		{
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Invalid comparison between Unknown and I4
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Invalid comparison between Unknown and I4
			if (Plugin.MakeChanges(self) && Plugin.options.bothHands.Value && ((ExtEnumBase)self.SlugCatClass).value != "Slugpup" && self.input[0].pckp && !self.input[1].pckp && self.switchHandsProcess == 0f)
			{
				bool flag = ((Creature)self).grasps[0] != null || ((Creature)self).grasps[1] != null;
				if (((Creature)self).grasps[0] != null && ((int)self.Grabability(((Creature)self).grasps[0].grabbed) == 3 || (int)self.Grabability(((Creature)self).grasps[0].grabbed) == 4))
				{
					flag = false;
				}
				if (flag)
				{
					if (self.switchHandsCounter == 0)
					{
						self.switchHandsCounter = 15;
					}
					else
					{
						((UpdatableAndDeletable)self).room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, ((Creature)self).mainBodyChunk);
						self.switchHandsProcess = 0.01f;
						self.wantToPickUp = 0;
						self.noPickUpOnRelease = 20;
					}
				}
				else
				{
					self.switchHandsProcess = 0f;
				}
			}
			orig.Invoke(self, eu);
			if (Plugin.MakeChanges(self) && !Plugin.options.letStomach.Value && !Plugin.options.letMaul.Value)
			{
				self.swallowAndRegurgitateCounter = 0;
			}
			else if (Plugin.MakeChanges(self))
			{
				if (!Plugin.options.letStomach.Value && self.maulTimer == 0)
				{
					self.swallowAndRegurgitateCounter = 0;
				}
				else if (!Plugin.options.letMaul.Value)
				{
					self.maulTimer = 0;
				}
			}
		}

		private static void Player_SlugcatGrab(orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
		{
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Invalid comparison between Unknown and I4
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			if (Plugin.MakeChanges(self) && Plugin.options.bothHands.Value && ((ExtEnumBase)self.SlugCatClass).value != "Slugpup")
			{
				if (obj is IPlayerEdible)
				{
					if (ModManager.MMF)
					{
						Creature val = (Creature)(object)((obj is Creature) ? obj : null);
						if (val == null || !val.dead)
						{
							Centipede val2 = (Centipede)(object)((obj is Centipede) ? obj : null);
							if (val2 != null && !val2.Small)
							{
								goto IL_0093;
							}
						}
					}
					((Creature)self).Grab(obj, graspUsed, 0, Shareability.CanOnlyShareWithNonExclusive, 0.5f, false, true);
				}
				goto IL_0093;
			}
			orig.Invoke(self, obj, graspUsed);
			return;
			IL_0093:
			int num = 0;
			if ((int)self.Grabability(obj) == 4)
			{
				float num2 = float.MaxValue;
				for (int i = 0; i < obj.bodyChunks.Length; i++)
				{
					if (Custom.DistLess(((Creature)self).mainBodyChunk.pos, obj.bodyChunks[i].pos, num2))
					{
						num2 = Vector2.Distance(((Creature)self).mainBodyChunk.pos, obj.bodyChunks[i].pos);
						num = i;
					}
				}
			}
			self.switchHandsCounter = 0;
			self.wantToPickUp = 0;
			self.noPickUpOnRelease = 20;
			bool flag = true;
			Creature val3 = (Creature)(object)((obj is Creature) ? obj : null);
			if (val3 != null)
			{
				if (self.IsCreatureImmuneToPlayerGrabStun(val3))
				{
					flag = false;
				}
				else if (!val3.dead && !self.IsCreatureLegalToHoldWithoutStun(val3))
				{
					flag = false;
				}
			}
			((Creature)self).Grab(obj, graspUsed, num, Shareability.CanOnlyShareWithNonExclusive, 0.5f, true, (ModManager.MMF || ModManager.CoopAvailable) ? flag : (!(obj is Cicada) && !(obj is JetFish)));
		}

		private static ObjectGrabability Player_Grabability(orig_Grabability orig, Player self, PhysicalObject obj)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			ObjectGrabability result = orig.Invoke(self, obj);
			if (ModManager.CoopAvailable)
			{
				Player val = (Player)(object)((obj is Player) ? obj : null);
				if (val != null && val != self && !val.standing && (!self.isSlugpup || Plugin.options.letPickup.Value))
				{
					PlayerState playerState = self.playerState;
					if (playerState == null || !playerState.isGhost)
					{
						JollyPlayerOptions jollyOption = val.JollyOption;
						if (jollyOption != null && jollyOption.isPup)
						{
							return (ObjectGrabability)1;
						}
						return (ObjectGrabability)2;
					}
				}
			}
			if (Plugin.MakeChanges(self) && Plugin.options.holdHands.Value && !self.isNPC)
			{
				Creature val2 = (Creature)(object)((obj is Creature) ? obj : null);
				if (val2 == null || val2.Template.smallCreature || (!val2.dead && (!SlugcatStats.SlugcatCanMaul(self.SlugCatClass) || self.dontGrabStuff >= 1 || val2 == self || val2.Consious)))
				{
					Player val3 = (Player)(object)((obj is Player) ? obj : null);
					if (val3 != null && val3 != self && (ExtEnum<Name>)(object)val3.SlugCatClass == (ExtEnum<Name>)(object)SlugcatStatsName.Slugpup && !val3.playerState.forceFullGrown)
					{
						result = (ObjectGrabability)1;
					}
				}
			}
			return result;
		}

		private static bool Player_CanEatMeat(orig_CanEatMeat orig, Player self, Creature crit)
		{
			if (Plugin.MakeChanges(self))
			{
				if (ModManager.MSC && ((ExtEnum<Name>)(object)self.SlugCatClass == (ExtEnum<Name>)(object)SlugcatStatsName.Saint || (ExtEnum<Name>)(object)self.SlugCatClass == (ExtEnum<Name>)(object)SlugcatStatsName.Spear))
				{
					return false;
				}
				if (self.EatMeatOmnivoreGreenList(crit) && crit.dead)
				{
					return !ModManager.MSC || self.pyroJumpCooldown <= 60f;
				}
				return Plugin.options.letEatMeat.Value && (orig.Invoke(self, crit) || (!(crit is IPlayerEdible) && crit.dead && ((ExtEnum<Name>)(object)self.SlugCatClass == (ExtEnum<Name>)(object)Name.Red || (ModManager.MSC && ((ExtEnum<Name>)(object)self.SlugCatClass == (ExtEnum<Name>)(object)SlugcatStatsName.Artificer || (ExtEnum<Name>)(object)self.SlugCatClass == (ExtEnum<Name>)(object)SlugcatStatsName.Gourmand || (ExtEnum<Name>)(object)self.SlugCatClass == (ExtEnum<Name>)(object)SlugcatStatsName.Sofanthiel))) && (!ModManager.CoopAvailable || !(crit is Player)) && (!ModManager.MSC || self.pyroJumpCooldown <= 60f)));
			}
			return orig.Invoke(self, crit);
		}

		private static void Player_GetInitialSlugcatClass(orig_GetInitialSlugcatClass orig, Player self)
		{
			if (!self.isNPC && (!ModManager.CoopAvailable || !((AbstractWorldEntity)((Creature)self).abstractCreature).Room.world.game.IsStorySession) && ModManager.MSC && !((AbstractWorldEntity)((Creature)self).abstractCreature).Room.world.game.IsStorySession && self.slugcatStats == null)
			{
				if (!(MultiPlayer.Session is StoryGameSession))
				{
					self.SlugCatClass = MultiPlayer.GetSpecificPlayer(((AbstractWorldEntity)((Creature)self).abstractCreature).ID.number).name;
					MultiPlayer.playerToLoad = ((AbstractWorldEntity)((Creature)self).abstractCreature).ID.number;
				}
				else
				{
					self.SlugCatClass = Plugin.currentSlugcat.name;
				}
			}
			else
			{
				orig.Invoke(self);
			}
		}

		private static void SlugcatStats_ctor(orig_ctor orig, SlugcatStats self, Name slugcat, bool malnourished)
		{
			if (MultiPlayer.startingIncrement >= 2 || (MultiPlayer.Session != null && !(MultiPlayer.Session is StoryGameSession) && MultiPlayer.startingIncrement >= 1))
			{
				MultiPlayer.AddPlayer(self);
			}
			SlugcatStats val = ((MultiPlayer.Session is ArenaGameSession && Plugin.playersCreated && MultiPlayer.startingIncrement != 4) ? MultiPlayer.GetSpecificPlayer(MultiPlayer.playerToLoad) : ((!ModManager.CoopAvailable && !(MultiPlayer.Session is ArenaGameSession) && !(MultiPlayer.Session is SandboxGameSession)) ? (Plugin.playersCreated ? self : (Plugin.currentSlugcat ?? self)) : ((!MultiPlayer.onlyPupsLeft && MultiPlayer.startingIncrement != 4) ? (MultiPlayer.GetCurrentPlayer(slugcat) ?? self) : self)));
			orig.Invoke(self, slugcat, (self != null && self.malnourished) || (val?.malnourished ?? false) || malnourished);
			if ((MultiPlayer.onlyPupsLeft && !ModManager.CoopAvailable) || MultiPlayer.startingIncrement == 4)
			{
				return;
			}
			int id = -1;
			GameSession session = MultiPlayer.Session;
			ArenaGameSession val2 = (ArenaGameSession)(object)((session is ArenaGameSession) ? session : null);
			if (val2 != null && (MultiPlayer.currentIndex != 0 || (MultiPlayer.currentIndex == 0 && Plugin.playersCreated)) && MultiPlayer.playerStats.Count <= val2.arenaSitting.players.Count)
			{
				id = val2.characterStats_Mplayer.Count((SlugcatStats item) => item != null);
				id = ((id != val2.arenaSitting.players.Count) ? val2.arenaSitting.players[id].playerNumber : (-1));
			}
			else
			{
				GameSession session2 = MultiPlayer.Session;
				StoryGameSession val3 = (StoryGameSession)(object)((session2 is StoryGameSession) ? session2 : null);
				if (val3 != null && MultiPlayer.startingIncrement >= 2 && (!MultiPlayer.onlyPupsLeft || val3.characterStatsJollyplayer[val3.characterStatsJollyplayer.Length - 1] == val) && MultiPlayer.startingIncrement == 2)
				{
					int jollyPlayerCount = ((GameSession)val3).game.rainWorld.options.JollyPlayerCount;
					id = (MultiPlayer.currentIndex + jollyPlayerCount - 1) % jollyPlayerCount;
					SlugcatStats[] characterStatsJollyplayer = val3.characterStatsJollyplayer;
					if (((characterStatsJollyplayer != null) ? characterStatsJollyplayer[0] : null) == null)
					{
						id = -1;
					}
				}
			}
			if (!Plugin.MakeChanges(null, id) || (Plugin.playersCreated && !ModManager.CoopAvailable && !(MultiPlayer.Session is ArenaGameSession)) || MultiPlayer.startingIncrement == 4)
			{
				if (MultiPlayer.startingIncrement != 4)
				{
					MultiPlayer.numAccessed += 2;
				}
				if (!MultiPlayer.onlyPupsLeft)
				{
					GameSession session3 = MultiPlayer.Session;
					ArenaGameSession val4 = (ArenaGameSession)(object)((session3 is ArenaGameSession) ? session3 : null);
					if (val4 == null || val4.arenaSitting == null || val4.arenaSitting.players.Count == 0 || MultiPlayer.numAccessed != val4.arenaSitting.players.Count * 2)
					{
						goto IL_0326;
					}
				}
				MultiPlayer.startingIncrement = 4;
				goto IL_0326;
			}
			if (!MultiPlayer.onlyPupsLeft)
			{
				GameSession session4 = MultiPlayer.Session;
				ArenaGameSession val5 = (ArenaGameSession)(object)((session4 is ArenaGameSession) ? session4 : null);
				if (val5 == null || val5.arenaSitting == null || val5.arenaSitting.players.Count == 0 || MultiPlayer.numAccessed != val5.arenaSitting.players.Count * 2)
				{
					goto IL_03e6;
				}
			}
			MultiPlayer.startingIncrement = 4;
			goto IL_03e6;
			IL_0326:
			if (!Plugin.options.onlyCosmetic.Value && Plugin.options.overrideFood.Value)
			{
				self.foodToHibernate = Mathf.RoundToInt(Plugin.options.foodToHibernate.Value);
				self.maxFood = Mathf.RoundToInt(Plugin.options.maxFood.Value);
			}
			return;
			IL_03e6:
			if (!ModManager.CoopAvailable && !(MultiPlayer.Session is ArenaGameSession) && !(MultiPlayer.Session is SandboxGameSession) && Plugin.currentSlugcat != null)
			{
				Plugin.playersCreated = true;
			}
			if (val == self)
			{
				if (Plugin.currentSlugcat == null)
				{
					Plugin.currentSlugcat = self;
				}
				if (Plugin.options.overrideFood.Value && self == Plugin.currentSlugcat)
				{
					self.foodToHibernate = Mathf.RoundToInt(Plugin.options.foodToHibernate.Value);
					self.maxFood = Mathf.RoundToInt(Plugin.options.maxFood.Value);
				}
				else
				{
					switch (Plugin.options.foodOption.Value)
					{
					case 0:
					{
						float num = (float)self.foodToHibernate / (float)self.maxFood;
						self.maxFood = Mathf.RoundToInt((float)self.maxFood * 0.42857143f);
						self.foodToHibernate = Mathf.RoundToInt((float)self.maxFood * num * 1.1666666f);
						if (self.foodToHibernate > self.maxFood)
						{
							self.foodToHibernate = self.maxFood;
						}
						break;
					}
					case 2:
						if (malnourished)
						{
							self.foodToHibernate = 3;
							self.maxFood = 3;
						}
						else
						{
							self.foodToHibernate = 2;
							self.maxFood = 3;
						}
						break;
					}
				}
			}
			else if (!ModManager.JollyCoop)
			{
				self.foodToHibernate = val.foodToHibernate;
				self.maxFood = val.maxFood;
			}
			else
			{
				if (Plugin.currentSlugcat == null)
				{
					Plugin.currentSlugcat = self;
				}
				if (!Plugin.foodBarsEnabled || Plugin.options.overrideFood.Value)
				{
					self.foodToHibernate = Plugin.currentSlugcat.foodToHibernate;
					self.maxFood = Plugin.currentSlugcat.maxFood;
				}
				else
				{
					self.foodToHibernate = val.foodToHibernate;
					self.maxFood = val.maxFood;
				}
			}
			switch (Plugin.options.statsOption.Value)
			{
			case 0:
				if (Plugin.currentSlugcat == null)
				{
					Plugin.currentSlugcat = self;
					val = self;
				}
				if (val == self || (!Plugin.playersCreated && ModManager.CoopAvailable))
				{
					self.runspeedFac = val.runspeedFac * 0.8f * 0.952381f;
					self.bodyWeightFac = val.bodyWeightFac * 0.65f * 1.025641f;
					self.generalVisibilityBonus = val.generalVisibilityBonus - 0.2f;
					self.visualStealthInSneakMode = val.visualStealthInSneakMode * 1.2f;
					self.loudnessFac = val.loudnessFac * 0.5f;
					self.lungsFac = val.lungsFac * 0.8f;
					self.poleClimbSpeedFac = val.poleClimbSpeedFac * 0.8f * 0.9569378f;
					self.corridorClimbSpeedFac = val.corridorClimbSpeedFac * 0.8f * 0.952381f;
					self.throwingSkill = val.throwingSkill - 1;
					if (self.throwingSkill < 0)
					{
						self.throwingSkill = 0;
					}
				}
				else
				{
					self.runspeedFac = val.runspeedFac;
					self.bodyWeightFac = val.bodyWeightFac;
					self.generalVisibilityBonus = val.generalVisibilityBonus;
					self.visualStealthInSneakMode = val.visualStealthInSneakMode;
					self.loudnessFac = val.loudnessFac;
					self.lungsFac = val.lungsFac;
					self.poleClimbSpeedFac = val.poleClimbSpeedFac;
					self.corridorClimbSpeedFac = val.corridorClimbSpeedFac;
					self.throwingSkill = val.throwingSkill;
				}
				break;
			case 1:
				if (val == null || val == self)
				{
					self.runspeedFac *= 0.952381f;
					self.bodyWeightFac *= 1.025641f;
					self.poleClimbSpeedFac *= 0.9569378f;
					self.corridorClimbSpeedFac *= 0.952381f;
					break;
				}
				self.runspeedFac = val.runspeedFac;
				self.bodyWeightFac = val.bodyWeightFac;
				self.generalVisibilityBonus = val.generalVisibilityBonus;
				self.visualStealthInSneakMode = val.visualStealthInSneakMode;
				self.loudnessFac = val.loudnessFac;
				self.lungsFac = val.lungsFac;
				self.poleClimbSpeedFac = val.poleClimbSpeedFac;
				self.corridorClimbSpeedFac = val.corridorClimbSpeedFac;
				self.throwingSkill = val.throwingSkill;
				break;
			default:
				self.runspeedFac *= 0.952381f;
				self.bodyWeightFac *= 1.025641f;
				self.poleClimbSpeedFac *= 0.9569378f;
				self.corridorClimbSpeedFac *= 0.952381f;
				break;
			}
		}

		private static void Player_SetMalnourished(orig_SetMalnourished orig, Player self, bool m, bool malnourishedbycreature)
		{
			if (self.npcCharacterStats != null && !m && self.isSlugpup)
			{
				float bodyWeightFac = self.npcCharacterStats.bodyWeightFac;
				orig.Invoke(self, false, malnourishedbycreature);
				self.npcCharacterStats.bodyWeightFac = bodyWeightFac;
				self.slugcatStats.bodyWeightFac = bodyWeightFac;
				float num = 0.7f * self.slugcatStats.bodyWeightFac;
				((PhysicalObject)self).bodyChunks[0].mass = num / 2f;
				((PhysicalObject)self).bodyChunks[1].mass = num / 2f;
			}
			else
			{
				orig.Invoke(self, m, malnourishedbycreature);
			}
		}

		private static void ShelterDoor_DoorClosed(orig_DoorClosed orig, ShelterDoor self)
		{
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			if (!Plugin.options.onlyCosmetic.Value && !Plugin.foodBarsEnabled && ModManager.CoopAvailable)
			{
				List<PhysicalObject> list = (from x in ((UpdatableAndDeletable)self).room.physicalObjects.SelectMany((List<PhysicalObject> x) => x)
					where x is Player
					select x).ToList();
				int count = list.Count;
				int num = 0;
				int foodToHibernate = Plugin.currentSlugcat.foodToHibernate;
				bool flag = count >= ((UpdatableAndDeletable)self).room.game.PlayersToProgressOrWin.Count;
				JollyCustom.Log("Player(s) in shelter: " + count + " Survived: " + flag, false);
				if (flag)
				{
					foreach (PhysicalObject item in list)
					{
						num = Math.Max(((Player)item).FoodInRoom(((UpdatableAndDeletable)self).room, false), num);
					}
					JollyCustom.Log("Survived!, food in room " + num, false);
					foreach (AbstractCreature player in ((UpdatableAndDeletable)self).room.game.Players)
					{
						if (((AbstractWorldEntity)player).Room != ((UpdatableAndDeletable)self).room.abstractRoom)
						{
							try
							{
								JollyCustom.WarpAndRevivePlayer(player, ((UpdatableAndDeletable)self).room.abstractRoom, ((UpdatableAndDeletable)self).room.LocalCoordinateOfNode(0));
							}
							catch (Exception arg)
							{
								JollyCustom.Log($"Could not warp and revive player {player} [{arg}]", false);
							}
						}
					}
					((UpdatableAndDeletable)self).room.game.Win(num < foodToHibernate, false);
				}
				else
				{
					((UpdatableAndDeletable)self).room.game.GoToDeathScreen();
				}
			}
			else
			{
				orig.Invoke(self);
			}
		}

		private static void PlayerGraphics_ctor(orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
		{
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Expected O, but got Unknown
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Expected O, but got Unknown
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Expected O, but got Unknown
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Expected O, but got Unknown
			orig.Invoke(self, ow);
			if (Plugin.MakeChanges(self.player) && !self.player.isNPC && !((ExtEnum<Name>)(object)self.player.SlugCatClass == (ExtEnum<Name>)(object)SlugcatStatsName.Spear))
			{
				float num = 0.85f + 0.3f * Mathf.Lerp(self.player.npcStats.Wideness, 0.5f, self.player.playerState.isPup ? 0.5f : 0f);
				float num2 = (0.75f + 0.5f * self.player.npcStats.Size) * (self.player.playerState.isPup ? 0.5f : 1f);
				self.tail[0] = new TailSegment((GraphicsModule)(object)self, 6f * num, 4f * num2, (TailSegment)null, 0.85f, 1f, 1f, true);
				self.tail[1] = new TailSegment((GraphicsModule)(object)self, 4f * num, 7f * num2, self.tail[0], 0.85f, 1f, 0.5f, true);
				self.tail[2] = new TailSegment((GraphicsModule)(object)self, 2.5f * num, 7f * num2, self.tail[1], 0.85f, 1f, 0.5f, true);
				self.tail[3] = new TailSegment((GraphicsModule)(object)self, 1f * num, 7f * num2, self.tail[2], 0.85f, 1f, 0.5f, true);
			}
		}

		private static void PlayerGraphics_DrawSprites(orig_DrawSprites orig, PlayerGraphics self, SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_037e: Unknown result type (might be due to invalid IL or missing references)
			//IL_038b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0392: Unknown result type (might be due to invalid IL or missing references)
			//IL_0397: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_0431: Unknown result type (might be due to invalid IL or missing references)
			//IL_0433: Unknown result type (might be due to invalid IL or missing references)
			//IL_0435: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03db: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0410: Unknown result type (might be due to invalid IL or missing references)
			//IL_041a: Unknown result type (might be due to invalid IL or missing references)
			//IL_041f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0424: Unknown result type (might be due to invalid IL or missing references)
			orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
			if ((ExtEnum<Name>)(object)self.player.SlugCatClass == (ExtEnum<Name>)(object)SlugcatStatsName.Saint)
			{
				float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * (float)Math.PI * 2f);
				Vector2 val = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
				Vector2 val2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
				Vector2 val3 = Vector2.Lerp(((BodyPart)self.head).lastPos, ((BodyPart)self.head).pos, timeStacker);
				if (self.player.aerobicLevel > 0.5f)
				{
					val += Custom.DirVec(val2, val) * Mathf.Lerp(-1f, 1f, num) * Mathf.InverseLerp(0.5f, 1f, self.player.aerobicLevel) * 0.5f;
					val3 -= Custom.DirVec(val2, val) * Mathf.Lerp(-1f, 1f, num) * Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, self.player.aerobicLevel), 1.5f) * 0.75f;
				}
				float num2 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(val2, val, 0.5f), val3);
				int num3 = Mathf.RoundToInt(Mathf.Abs(num2 / 360f * 34f));
				if (self.player.sleepCurlUp > 0f)
				{
					num3 = 7;
					num3 = Custom.IntClamp((int)Mathf.Lerp((float)num3, 4f, self.player.sleepCurlUp), 0, 8);
				}
				if (self.player.sleepCurlUp <= 0f && ((UpdatableAndDeletable)((GraphicsModule)self).owner).room != null && ((GraphicsModule)self).owner.EffectiveRoomGravity == 0f)
				{
					num3 = 0;
				}
				else if (((Creature)self.player).Consious)
				{
					if (((ExtEnum<BodyModeIndex>)(object)self.player.bodyMode == (ExtEnum<BodyModeIndex>)(object)BodyModeIndex.Stand && self.player.input[0].x != 0) || (ExtEnum<BodyModeIndex>)(object)self.player.bodyMode == (ExtEnum<BodyModeIndex>)(object)BodyModeIndex.Crawl)
					{
						num3 = (((ExtEnum<BodyModeIndex>)(object)self.player.bodyMode == (ExtEnum<BodyModeIndex>)(object)BodyModeIndex.Crawl) ? 7 : 6);
					}
				}
				else
				{
					num3 = 0;
				}
				((FFacetElementNode)sLeaser.sprites[3]).element = Futile.atlasManager.GetElementWithName("HeadB" + num3);
			}
			if (!Plugin.MakeChanges(self.player) || self.player.isNPC)
			{
				return;
			}
			if ((ExtEnum<Name>)(object)self.player.SlugCatClass != (ExtEnum<Name>)(object)SlugcatStatsName.Gourmand && (ExtEnum<Name>)(object)self.player.SlugCatClass != (ExtEnum<Name>)(object)SlugcatStatsName.Spear)
			{
				float num4 = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * (float)Math.PI * 2f);
				Vector2 val4 = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
				Vector2 val5 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
				if (self.player.aerobicLevel > 0.5f)
				{
					val4 += Custom.DirVec(val5, val4) * Mathf.Lerp(-1f, 1f, num4) * Mathf.InverseLerp(0.5f, 1f, self.player.aerobicLevel) * 0.5f;
				}
				float num5 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(Custom.DirVec(val5, val4).y));
				((FNode)sLeaser.sprites[0]).scaleX = 0.9f + 0.2f * Mathf.Lerp(self.player.npcStats.Wideness, 0.5f, self.player.playerState.isPup ? 0.5f : 0f) + self.player.sleepCurlUp * 0.2f + 0.05f * num4 - 0.05f * self.malnourished;
				((FNode)sLeaser.sprites[1]).scaleX = 0.9f + 0.2f * Mathf.Lerp(self.player.npcStats.Wideness, 0.5f, self.player.playerState.isPup ? 0.5f : 0f) + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, self.malnourished), 0.05f, num4) * num5, 0.15f, self.player.sleepCurlUp);
				if (ModManager.CoopAvailable && self.player.bool1)
				{
					FSprite obj = sLeaser.sprites[0];
					((FNode)obj).scaleX = ((FNode)obj).scaleX + 0.35f;
				}
			}
			if ((ExtEnum<Name>)(object)self.player.SlugCatClass != (ExtEnum<Name>)(object)SlugcatStatsName.Spear)
			{
				FSprite obj2 = sLeaser.sprites[3];
				((FNode)obj2).scaleX = ((FNode)obj2).scaleX * (0.9f + 0.2f * Mathf.Lerp(self.player.npcStats.Wideness, 0.5f, self.player.playerState.isPup ? 0.5f : 0f));
			}
		}

		private static Color Player_ShortCutColor(orig_ShortCutColor orig, Player self)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			int randomSeed = ((EntityID)(ref ((AbstractWorldEntity)((Creature)self).abstractCreature).ID)).RandomSeed;
			if (!ModManager.CoopAvailable && !Plugin.options.onlyCosmetic.Value && !self.isNPC && !self.isNPC && randomSeed != 1000 && randomSeed != 1001 && randomSeed != 1002)
			{
				return PlayerGraphics.SlugcatColor(self.SlugCatClass);
			}
			return orig.Invoke(self);
		}

		private static void SlugcatHandOnUpdate(orig_Update orig, SlugcatHand self)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			//IL_02af: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			//IL_0312: Unknown result type (might be due to invalid IL or missing references)
			//IL_0345: Unknown result type (might be due to invalid IL or missing references)
			//IL_0374: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0530: Unknown result type (might be due to invalid IL or missing references)
			//IL_0560: Unknown result type (might be due to invalid IL or missing references)
			//IL_0549: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_080b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0827: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0469: Unknown result type (might be due to invalid IL or missing references)
			//IL_0479: Unknown result type (might be due to invalid IL or missing references)
			//IL_049d: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a63: Unknown result type (might be due to invalid IL or missing references)
			//IL_040b: Unknown result type (might be due to invalid IL or missing references)
			//IL_041d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aba: Unknown result type (might be due to invalid IL or missing references)
			//IL_058b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b09: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b13: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b19: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b1f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c28: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c3d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c53: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c5e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c63: Unknown result type (might be due to invalid IL or missing references)
			//IL_0873: Unknown result type (might be due to invalid IL or missing references)
			//IL_0890: Unknown result type (might be due to invalid IL or missing references)
			//IL_089a: Unknown result type (might be due to invalid IL or missing references)
			//IL_089f: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_08f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0908: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_060a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b3c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b41: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b46: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b4d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b5d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b67: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b71: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b76: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b7b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b86: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b97: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ba1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ba7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bac: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bb6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bc0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bd1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bdc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bf6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bfc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c01: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c0b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c15: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c1a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c1f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0942: Unknown result type (might be due to invalid IL or missing references)
			//IL_096f: Unknown result type (might be due to invalid IL or missing references)
			//IL_098f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0994: Unknown result type (might be due to invalid IL or missing references)
			//IL_099c: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_09b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_09c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0636: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a06: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a11: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0708: Unknown result type (might be due to invalid IL or missing references)
			//IL_071e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0723: Unknown result type (might be due to invalid IL or missing references)
			//IL_0728: Unknown result type (might be due to invalid IL or missing references)
			//IL_0733: Unknown result type (might be due to invalid IL or missing references)
			//IL_073e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0743: Unknown result type (might be due to invalid IL or missing references)
			//IL_074d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0757: Unknown result type (might be due to invalid IL or missing references)
			//IL_076d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0772: Unknown result type (might be due to invalid IL or missing references)
			//IL_0777: Unknown result type (might be due to invalid IL or missing references)
			//IL_078f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0794: Unknown result type (might be due to invalid IL or missing references)
			//IL_079e: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_07be: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_065a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0676: Unknown result type (might be due to invalid IL or missing references)
			//IL_067b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a30: Unknown result type (might be due to invalid IL or missing references)
			orig.Invoke(self);
			if (((Player)((BodyPart)self).owner.owner).isNPC || !Plugin.MakeChanges((Player)((BodyPart)self).owner.owner) || Plugin.options.bothHands.Value || (((Creature)(Player)((BodyPart)self).owner.owner).Consious && ((PhysicalObject)(Player)((BodyPart)self).owner.owner).grabbedBy.Count > 0 && ((PhysicalObject)(Player)((BodyPart)self).owner.owner).grabbedBy[0]?.grabber is Player && Mathf.Sign(((PhysicalObject)(Player)((BodyPart)self).owner.owner).firstChunk.pos.x - ((PhysicalObject)((PhysicalObject)(Player)((BodyPart)self).owner.owner).grabbedBy[0].grabber).firstChunk.pos.x) == (float)((((Limb)self).limbNumber != 0) ? 1 : (-1))) || (((Creature)(Player)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber] != null && ((Creature)(Player)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber].grabbed is Player && ((Creature)(Player)((Creature)(Player)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber].grabbed).Consious))
			{
				return;
			}
			bool flag = !self.reachingForObject && self.EngageInMovement();
			if (ModManager.MMF)
			{
				if (((Creature)(Player)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber] != null && ((Player)((BodyPart)self).owner.owner).HeavyCarry(((Creature)(Player)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber].grabbed))
				{
					flag = true;
				}
			}
			else if (((Limb)self).limbNumber == 0 && ((Creature)(Player)((BodyPart)self).owner.owner).grasps[0] != null && ((Player)((BodyPart)self).owner.owner).HeavyCarry(((Creature)(Player)((BodyPart)self).owner.owner).grasps[0].grabbed))
			{
				flag = true;
			}
			if (!flag || (((Creature)(Player)((BodyPart)self).owner.owner).grasps[0] != null && ((Player)((BodyPart)self).owner.owner).HeavyCarry(((Creature)(Player)((BodyPart)self).owner.owner).grasps[0].grabbed)) || (ModManager.MMF && ((Creature)(Player)((BodyPart)self).owner.owner).grasps[1] != null && ((Player)((BodyPart)self).owner.owner).HeavyCarry(((Creature)(Player)((BodyPart)self).owner.owner).grasps[1].grabbed)) || ((Creature)(Player)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber] == null)
			{
				return;
			}
			((Limb)self).relativeHuntPos.x = ((Player)((BodyPart)self).owner.owner).ThrowDirection * 3;
			((Limb)self).relativeHuntPos.y = -12f;
			if (((Player)((BodyPart)self).owner.owner).eatCounter < 40)
			{
				int num = -1;
				int num2 = 0;
				while (num < 0 && num2 < 2)
				{
					if (((Creature)(Player)((BodyPart)self).owner.owner).grasps[num2] != null && ((Creature)(Player)((BodyPart)self).owner.owner).grasps[num2].grabbed is IPlayerEdible && ((IPlayerEdible)((Creature)(Player)((BodyPart)self).owner.owner).grasps[num2].grabbed).Edible)
					{
						num = num2;
					}
					num2++;
				}
				if (num == ((Limb)self).limbNumber)
				{
					((Limb)self).relativeHuntPos = ((Limb)self).relativeHuntPos * Custom.LerpMap((float)((Player)((BodyPart)self).owner.owner).eatCounter, 40f, 20f, 0.9f, 0.7f);
					((Limb)self).relativeHuntPos.y += Custom.LerpMap((float)((Player)((BodyPart)self).owner.owner).eatCounter, 40f, 20f, 2f, 4f);
					((Limb)self).relativeHuntPos.x *= Custom.LerpMap((float)((Player)((BodyPart)self).owner.owner).eatCounter, 40f, 20f, 1f, 1.2f);
				}
			}
			if ((((Player)((BodyPart)self).owner.owner).swallowAndRegurgitateCounter > 10 && ((Player)((BodyPart)self).owner.owner).objectInStomach == null) || ((Player)((BodyPart)self).owner.owner).craftingObject)
			{
				int num3 = -1;
				int num4 = 0;
				while (num3 < 0 && num4 < 2)
				{
					if (((Creature)(Player)((BodyPart)self).owner.owner).grasps[num4] != null && ((Player)((BodyPart)self).owner.owner).CanBeSwallowed(((Creature)(Player)((BodyPart)self).owner.owner).grasps[num4].grabbed))
					{
						num3 = num4;
					}
					num4++;
				}
				if (num3 == ((Limb)self).limbNumber || ((Player)((BodyPart)self).owner.owner).craftingObject)
				{
					float num5 = Mathf.InverseLerp(10f, 90f, (float)((Player)((BodyPart)self).owner.owner).swallowAndRegurgitateCounter);
					if (num5 < 0.5f)
					{
						((Limb)self).relativeHuntPos = ((Limb)self).relativeHuntPos * Mathf.Lerp(0.9f, 0.7f, num5 * 2f);
						((Limb)self).relativeHuntPos.y += Mathf.Lerp(2f, 4f, num5 * 2f);
						((Limb)self).relativeHuntPos.x *= Mathf.Lerp(1f, 1.2f, num5 * 2f);
					}
					else
					{
						((PlayerGraphics)((BodyPart)self).owner).blink = 5;
						((Limb)self).relativeHuntPos = new Vector2(0f, -4f) + Custom.RNV() * 2f * Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
						GenericBodyPart head = ((PlayerGraphics)((BodyPart)self).owner).head;
						((BodyPart)head).vel = ((BodyPart)head).vel + Custom.RNV() * 2f * Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
						BodyChunk obj = ((BodyPart)self).owner.owner.bodyChunks[0];
						obj.vel += Custom.RNV() * 0.2f * Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
					}
				}
			}
			((Limb)self).relativeHuntPos.x *= 1f - Mathf.Sin(((Player)((BodyPart)self).owner.owner).switchHandsProcess * (float)Math.PI);
			if (((PlayerGraphics)((BodyPart)self).owner).spearDir != 0f && (ExtEnum<BodyModeIndex>)(object)((Player)((BodyPart)self).owner.owner).bodyMode == (ExtEnum<BodyModeIndex>)(object)BodyModeIndex.Stand)
			{
				Vector2 val = Custom.DegToVec(180f + ((((Limb)self).limbNumber == 0) ? (-1f) : 1f) * 8f + (float)((Player)((BodyPart)self).owner.owner).input[0].x * 4f) * 12f;
				val.y += Mathf.Sin((float)((Player)((BodyPart)self).owner.owner).animationFrame / 6f * 2f * (float)Math.PI) * 2f;
				val.x -= Mathf.Cos((float)(((Player)((BodyPart)self).owner.owner).animationFrame + ((!((Player)((BodyPart)self).owner.owner).leftFoot) ? 6 : 0)) / 12f * 2f * (float)Math.PI) * 4f * (float)((Player)((BodyPart)self).owner.owner).input[0].x;
				val.x += (float)((Player)((BodyPart)self).owner.owner).input[0].x * 2f;
				((Limb)self).relativeHuntPos = Vector2.Lerp(((Limb)self).relativeHuntPos, val, Mathf.Abs(((PlayerGraphics)((BodyPart)self).owner).spearDir));
				if (((Creature)(Player)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber].grabbed is Weapon)
				{
					((Weapon)((Creature)(Player)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber].grabbed).ChangeOverlap((((PlayerGraphics)((BodyPart)self).owner).spearDir > -0.4f && ((Limb)self).limbNumber == 0) || (((PlayerGraphics)((BodyPart)self).owner).spearDir < 0.4f && ((Limb)self).limbNumber == 1));
				}
			}
			PhysicalObject grabbed = ((Creature)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber].grabbed;
			PhysicalObject val2 = grabbed;
			if (!(val2 is Fly))
			{
				if (val2 is VultureMask)
				{
					((Limb)self).relativeHuntPos = ((Limb)self).relativeHuntPos * (1f - ((VultureMask)((Creature)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber].grabbed).donned);
				}
			}
			else if (!((Creature)(Fly)((Creature)((BodyPart)self).owner.owner).grasps[((Limb)self).limbNumber].grabbed).dead)
			{
				((Limb)self).huntSpeed = Random.value * 5f;
				((Limb)self).quickness = Random.value * 0.3f;
				((BodyPart)self).vel = ((BodyPart)self).vel + Custom.DegToVec(Random.value * 360f) * Random.value * Random.value * (Custom.DistLess(((Limb)self).absoluteHuntPos, ((BodyPart)self).pos, 7f) ? 4f : 1.5f);
				((BodyPart)self).pos = ((BodyPart)self).pos + Custom.DegToVec(Random.value * 360f) * Random.value * 4f;
				((PlayerGraphics)((BodyPart)self).owner).NudgeDrawPosition(0, Custom.DirVec(((Creature)((BodyPart)self).owner.owner).mainBodyChunk.pos, ((BodyPart)self).pos) * 3f * Random.value);
				GenericBodyPart head2 = ((PlayerGraphics)((BodyPart)self).owner).head;
				((BodyPart)head2).vel = ((BodyPart)head2).vel + Custom.DirVec(((Creature)((BodyPart)self).owner.owner).mainBodyChunk.pos, ((BodyPart)self).pos) * 2f * Random.value;
			}
			self.retractCounter -= 10;
			if (self.retractCounter < 0)
			{
				self.retractCounter = 0;
			}
		}

		public static bool Player_isSlugpup(Player __instance, ref bool __result)
		{
			if (!__instance.isNPC && !Plugin.MakeChanges(__instance))
			{
				return true;
			}
			__result = true;
			return false;
		}
	}
	public static class SceneHooks
	{
		[CompilerGenerated]
		private static class <>O
		{
			public static hook_SinglePlayerUpdate <0>__HardmodeStart_SinglePlayerUpdate;

			public static hook_Update <1>__HardmodePlayer_Update;

			public static hook_Update <2>__SpearmasterGateLocation_Update;

			public static hook_Update <3>__CutsceneArtificer_Update;

			public static hook_SceneSetup <4>__ArtificerDream_1_SceneSetup;

			public static hook_GetInput <5>__ArtificerDream_1_GetInput;

			public static hook_SceneSetup <6>__ArtificerDream_4_SceneSetup;

			public static hook_GetInput <7>__ArtificerDream_4_GetInput;

			public static hook_SceneSetup <8>__ArtificerDream_5_SceneSetup;
		}

		public static void Init()
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
			object obj = <>O.<0>__HardmodeStart_SinglePlayerUpdate;
			if (obj == null)
			{
				hook_SinglePlayerUpdate val = HardmodeStart_SinglePlayerUpdate;
				<>O.<0>__HardmodeStart_SinglePlayerUpdate = val;
				obj = (object)val;
			}
			HardmodeStart.SinglePlayerUpdate += (hook_SinglePlayerUpdate)obj;
			object obj2 = <>O.<1>__HardmodePlayer_Update;
			if (obj2 == null)
			{
				hook_Update val2 = HardmodePlayer_Update;
				<>O.<1>__HardmodePlayer_Update = val2;
				obj2 = (object)val2;
			}
			HardmodePlayer.Update += (hook_Update)obj2;
			object obj3 = <>O.<2>__SpearmasterGateLocation_Update;
			if (obj3 == null)
			{
				hook_Update val3 = SpearmasterGateLocation_Update;
				<>O.<2>__SpearmasterGateLocation_Update = val3;
				obj3 = (object)val3;
			}
			SpearmasterGateLocation.Update += (hook_Update)obj3;
			object obj4 = <>O.<3>__CutsceneArtificer_Update;
			if (obj4 == null)
			{
				hook_Update val4 = CutsceneArtificer_Update;
				<>O.<3>__CutsceneArtificer_Update = val4;
				obj4 = (object)val4;
			}
			CutsceneArtificer.Update += (hook_Update)obj4;
			object obj5 = <>O.<4>__ArtificerDream_1_SceneSetup;
			if (obj5 == null)
			{
				hook_SceneSetup val5 = ArtificerDream_1_SceneSetup;
				<>O.<4>__ArtificerDream_1_SceneSetup = val5;
				obj5 = (object)val5;
			}
			ArtificerDream_1.SceneSetup += (hook_SceneSetup)obj5;
			object obj6 = <>O.<5>__ArtificerDream_1_GetInput;
			if (obj6 == null)
			{
				hook_GetInput val6 = ArtificerDream_1_GetInput;
				<>O.<5>__ArtificerDream_1_GetInput = val6;
				obj6 = (object)val6;
			}
			ArtificerDream_1.GetInput += (hook_GetInput)obj6;
			object obj7 = <>O.<6>__ArtificerDream_4_SceneSetup;
			if (obj7 == null)
			{
				hook_SceneSetup val7 = ArtificerDream_4_SceneSetup;
				<>O.<6>__ArtificerDream_4_SceneSetup = val7;
				obj7 = (object)val7;
			}
			ArtificerDream_4.SceneSetup += (hook_SceneSetup)obj7;
			object obj8 = <>O.<7>__ArtificerDream_4_GetInput;
			if (obj8 == null)
			{
				hook_GetInput val8 = ArtificerDream_4_GetInput;
				<>O.<7>__ArtificerDream_4_GetInput = val8;
				obj8 = (object)val8;
			}
			ArtificerDream_4.GetInput += (hook_GetInput)obj8;
			object obj9 = <>O.<8>__ArtificerDream_5_SceneSetup;
			if (obj9 == null)
			{
				hook_SceneSetup val9 = ArtificerDream_5_SceneSetup;
				<>O.<8>__ArtificerDream_5_SceneSetup = val9;
				obj9 = (object)val9;
			}
			ArtificerDream_5.SceneSetup += (hook_SceneSetup)obj9;
		}

		private static void HardmodeStart_SinglePlayerUpdate(orig_SinglePlayerUpdate orig, HardmodeStart self)
		{
			orig.Invoke(self);
			if (Plugin.options.onlyCosmetic.Value)
			{
				return;
			}
			Creature realizedCreature = ((UpdatableAndDeletable)self).room.game.Players[0].realizedCreature;
			Player val = (Player)(object)((realizedCreature is Player) ? realizedCreature : null);
			if (val == null || val.playerState == null)
			{
				return;
			}
			if (Plugin.options.overrideFood.Value)
			{
				val.playerState.foodInStomach = Mathf.RoundToInt(Plugin.options.foodToHibernate.Value) - 1;
				return;
			}
			PlayerState playerState = val.playerState;
			int value = Plugin.options.foodOption.Value;
			if (1 == 0)
			{
			}
			int foodInStomach = value switch
			{
				0 => 2, 
				1 => 5, 
				2 => 1, 
				_ => 0, 
			};
			if (1 == 0)
			{
			}
			playerState.foodInStomach = foodInStomach;
		}

		private static void HardmodePlayer_Update(orig_Update orig, HardmodePlayer self)
		{
			float runspeedFac = 1.2f;
			if (self.Player != null)
			{
				runspeedFac = self.Player.slugcatStats.runspeedFac;
			}
			orig.Invoke(self);
			if (self.Player != null)
			{
				self.Player.slugcatStats.runspeedFac = runspeedFac;
			}
			if (Plugin.options.onlyCosmetic.Value)
			{
				return;
			}
			Player player = self.Player;
			Creature obj = ((player != null) ? ((UpdatableAndDeletable)player).room.game.Players[0].realizedCreature : null);
			Player val = (Player)(object)((obj is Player) ? obj : null);
			if (val == null || val.playerState == null)
			{
				return;
			}
			if (Plugin.options.overrideFood.Value)
			{
				val.playerState.foodInStomach = Mathf.RoundToInt(Plugin.options.foodToHibernate.Value) - 1;
				return;
			}
			PlayerState playerState = val.playerState;
			int value = Plugin.options.foodOption.Value;
			if (1 == 0)
			{
			}
			int foodInStomach = value switch
			{
				0 => 2, 
				1 => 5, 
				2 => 1, 
				_ => 0, 
			};
			if (1 == 0)
			{
			}
			playerState.foodInStomach = foodInStomach;
		}

		private static void SpearmasterGateLocation_Update(orig_Update orig, SpearmasterGateLocation self, bool eu)
		{
			orig.Invoke(self, eu);
			if (Plugin.options.onlyCosmetic.Value)
			{
				return;
			}
			Creature realizedCreature = ((UpdatableAndDeletable)self).room.game.Players[0].realizedCreature;
			Player val = (Player)(object)((realizedCreature is Player) ? realizedCreature : null);
			if (val == null || val.playerState == null)
			{
				return;
			}
			if (Plugin.options.overrideFood.Value)
			{
				val.playerState.foodInStomach = Mathf.RoundToInt(Plugin.options.foodToHibernate.Value) - 1;
				return;
			}
			PlayerState playerState = val.playerState;
			int value = Plugin.options.foodOption.Value;
			if (1 == 0)
			{
			}
			int foodInStomach = value switch
			{
				0 => 1, 
				1 => 4, 
				2 => 1, 
				_ => 0, 
			};
			if (1 == 0)
			{
			}
			playerState.foodInStomach = foodInStomach;
		}

		private static void CutsceneArtificer_Update(orig_Update orig, CutsceneArtificer self, bool eu)
		{
			orig.Invoke(self, eu);
			if (Plugin.options.onlyCosmetic.Value)
			{
				return;
			}
			Creature realizedCreature = ((UpdatableAndDeletable)self).room.game.Players[0].realizedCreature;
			Player val = (Player)(object)((realizedCreature is Player) ? realizedCreature : null);
			if (val == null || val.playerState == null)
			{
				return;
			}
			if (Plugin.options.letEatMeat.Value)
			{
				if (self.player.myRobot == null || (self.player.myRobot != null && ((UpdatableAndDeletable)self).room.world.rainCycle.timer >= 400))
				{
					return;
				}
				if (Plugin.options.overrideFood.Value)
				{
					int num = 4;
					int num2 = Mathf.RoundToInt(Plugin.options.maxFood.Value) - Mathf.RoundToInt(Plugin.options.foodToHibernate.Value);
					if (num > num2)
					{
						num = num2;
					}
					self.player.playerState.foodInStomach = num;
				}
				else
				{
					self.player.playerState.foodInStomach = ((Plugin.options.foodOption.Value != 2) ? 4 : 3);
				}
			}
			else
			{
				self.player.playerState.foodInStomach = 0;
			}
		}

		private static void ArtificerDream_1_SceneSetup(orig_SceneSetup orig, ArtificerDream_1 self)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Expected O, but got Unknown
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Expected O, but got Unknown
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Expected O, but got Unknown
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
			if (self.artificerPuppet == null)
			{
				self.artificerPuppet = new AbstractCreature(((UpdatableAndDeletable)self).room.world, StaticWorld.GetCreatureTemplate(Type.Slugcat), (Creature)null, new WorldCoordinate(((UpdatableAndDeletable)self).room.abstractRoom.index, 87, 8, -1), ((UpdatableAndDeletable)self).room.game.GetNewID());
				self.artificerPuppet.state = (CreatureState)new PlayerState(self.artificerPuppet, 0, SlugcatStatsName.Artificer, true);
				((UpdatableAndDeletable)self).room.abstractRoom.AddEntity((AbstractWorldEntity)(object)self.artificerPuppet);
				self.pup2Puppet = new AbstractCreature(((UpdatableAndDeletable)self).room.world, StaticWorld.GetCreatureTemplate(CreatureTemplateType.SlugNPC), (Creature)null, new WorldCoordinate(((UpdatableAndDeletable)self).room.abstractRoom.index, 87, 8, -1), ((UpdatableAndDeletable)self).room.game.GetNewID());
				((EntityID)(ref ((AbstractWorldEntity)self.pup2Puppet).ID)).setAltSeed(1001);
				self.pup2Puppet.state = (CreatureState)new PlayerNPCState(self.pup2Puppet, 0);
				((UpdatableAndDeletable)self).room.abstractRoom.AddEntity((AbstractWorldEntity)(object)self.pup2Puppet);
				((AbstractPhysicalObject)self.artificerPuppet).RealizeInRoom();
				((AbstractPhysicalObject)self.pup2Puppet).RealizeInRoom();
			}
			if (self.artyPlayerPuppet == null && self.artificerPuppet.realizedCreature != null)
			{
				ref Player artyPlayerPuppet = ref self.artyPlayerPuppet;
				Creature realizedCreature = self.artificerPuppet.realizedCreature;
				artyPlayerPuppet = (Player)(object)((realizedCreature is Player) ? realizedCreature : null);
			}
			if (self.pupPlayerPuppet == null && self.pup2Puppet.realizedCreature != null)
			{
				ref Player pupPlayerPuppet = ref self.pupPlayerPuppet;
				Creature realizedCreature2 = self.pup2Puppet.realizedCreature;
				pupPlayerPuppet = (Player)(object)((realizedCreature2 is Player) ? realizedCreature2 : null);
			}
			AbstractCreature firstAlivePlayer = ((UpdatableAndDeletable)self).room.game.FirstAlivePlayer;
			if (firstAlivePlayer == null || self.artyPlayerPuppet == null || self.pupPlayerPuppet == null || firstAlivePlayer.realizedCreature == null)
			{
				return;
			}
			self.SpawnAmbientCritters();
			self.pup2Puppet.state.socialMemory.GetOrInitiateRelationship(((AbstractWorldEntity)firstAlivePlayer).ID).InfluenceLike(1f);
			self.pup2Puppet.state.socialMemory.GetOrInitiateRelationship(((AbstractWorldEntity)firstAlivePlayer).ID).InfluenceTempLike(1f);
			self.artyPlayerPuppet.controller = (PlayerController)new StartController((ArtificerDream)(object)self, 0);
			self.artyPlayerPuppet.standing = true;
			self.artyPlayerPuppet.slugcatStats.visualStealthInSneakMode = 2f;
			if (firstAlivePlayer.realizedCreature != null)
			{
				Creature realizedCreature3 = firstAlivePlayer.realizedCreature;
				Creature obj = ((realizedCreature3 is Player) ? realizedCreature3 : null);
				if (obj != null)
				{
					((Player)obj).SuperHardSetPosition(((PhysicalObject)self.artyPlayerPuppet).firstChunk.pos);
				}
				((AbstractWorldEntity)firstAlivePlayer).pos = ((AbstractWorldEntity)((Creature)self.artyPlayerPuppet).abstractCreature).pos;
			}
			((ArtificerDream)self).sceneStarted = true;
		}

		private static InputPackage ArtificerDream_1_GetInput(orig_GetInput orig, ArtificerDream_1 self, int index)
		{
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_076d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Expected O, but got Unknown
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0322: Unknown result type (might be due to invalid IL or missing references)
			//IL_0327: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0354: Unknown result type (might be due to invalid IL or missing references)
			//IL_037c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0381: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0400: Unknown result type (might be due to invalid IL or missing references)
			//IL_0405: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0480: Unknown result type (might be due to invalid IL or missing references)
			//IL_0485: Unknown result type (might be due to invalid IL or missing references)
			//IL_0431: Unknown result type (might be due to invalid IL or missing references)
			//IL_0443: Unknown result type (might be due to invalid IL or missing references)
			//IL_0457: Unknown result type (might be due to invalid IL or missing references)
			//IL_045c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0467: Unknown result type (might be due to invalid IL or missing references)
			//IL_04de: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0762: Unknown result type (might be due to invalid IL or missing references)
			//IL_0768: Unknown result type (might be due to invalid IL or missing references)
			//IL_0769: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0757: Unknown result type (might be due to invalid IL or missing references)
			//IL_075c: Unknown result type (might be due to invalid IL or missing references)
			AbstractCreature firstAlivePlayer = ((UpdatableAndDeletable)self).room.game.FirstAlivePlayer;
			if (((ArtificerDream)self).sceneTimer < 302)
			{
				((PhysicalObject)firstAlivePlayer.realizedCreature).bodyChunks[0].vel.x += 1.75f;
			}
			if (((ArtificerDream)self).sceneTimer < 160)
			{
				Creature obj = ((firstAlivePlayer != null) ? firstAlivePlayer.realizedCreature : null);
				Creature obj2 = ((obj is Player) ? obj : null);
				if (obj2 != null)
				{
					((Player)obj2).SuperHardSetPosition(((PhysicalObject)self.artyPlayerPuppet).firstChunk.pos);
				}
				return default(InputPackage);
			}
			if (((ArtificerDream)self).sceneTimer == 160)
			{
				BodyChunk obj3 = ((PhysicalObject)self.artyPlayerPuppet).bodyChunks[0];
				obj3.vel *= 0f;
				BodyChunk obj4 = ((PhysicalObject)self.artyPlayerPuppet).bodyChunks[1];
				obj4.vel *= 0f;
				((PhysicalObject)self.artyPlayerPuppet).bodyChunks[0].pos = new Vector2(1900f, 340f);
				((PhysicalObject)self.artyPlayerPuppet).bodyChunks[1].pos = new Vector2(1900f, 320f);
				return new InputPackage(false, Preset.None, 1, 1, false, false, true, false, false);
			}
			if (((ArtificerDream)self).sceneTimer == 165)
			{
				return new InputPackage(false, Preset.None, 1, 1, false, false, true, false, false);
			}
			if (((ArtificerDream)self).sceneTimer < 166)
			{
				return new InputPackage(false, Preset.None, 1, 1, false, false, true, false, false);
			}
			if (((ArtificerDream)self).sceneTimer == 166)
			{
				BodyChunk obj5 = ((PhysicalObject)self.artyPlayerPuppet).bodyChunks[0];
				obj5.vel += new Vector2(10f, 13f);
				BodyChunk obj6 = ((PhysicalObject)self.artyPlayerPuppet).bodyChunks[1];
				obj6.vel += new Vector2(10f, 13f);
				((UpdatableAndDeletable)self).room.AddObject((UpdatableAndDeletable)new ExplosionSpikes(((UpdatableAndDeletable)self).room, ((PhysicalObject)self.artyPlayerPuppet).bodyChunks[0].pos + new Vector2(0f, 0f - ((PhysicalObject)self.artyPlayerPuppet).bodyChunks[0].rad), 8, 7f, 5f, 5.5f, 40f, new Color(1f, 1f, 1f, 0.5f)));
				((UpdatableAndDeletable)self).room.PlaySound(SoundID.Slugcat_Rocket_Jump, ((PhysicalObject)self.artyPlayerPuppet).bodyChunks[0], false, 1f, 1f);
				return new InputPackage(false, Preset.None, 1, 1, true, false, true, false, false);
			}
			if (((ArtificerDream)self).sceneTimer <= 190)
			{
				return new InputPackage(false, Preset.None, 1, 1, true, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer <= 210)
			{
				return new InputPackage(false, Preset.None, 0, 1, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer == 211)
			{
				return new InputPackage(false, Preset.None, 0, 0, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer == 212)
			{
				return new InputPackage(false, Preset.None, 0, 1, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer <= 239)
			{
				return default(InputPackage);
			}
			if (((ArtificerDream)self).sceneTimer <= 300)
			{
				return new InputPackage(false, Preset.None, 1, 1, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer == 301)
			{
				return new InputPackage(false, Preset.None, 0, 0, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer == 302)
			{
				if (firstAlivePlayer != null)
				{
					((Creature)(Player)firstAlivePlayer.realizedCreature).Stun(5);
					((PhysicalObject)(Player)firstAlivePlayer.realizedCreature).firstChunk.vel = new Vector2(5f, 5f);
					((Player)firstAlivePlayer.realizedCreature).standing = true;
				}
				return new InputPackage(false, Preset.None, 0, 0, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer <= 304)
			{
				return new InputPackage(false, Preset.None, 1, 0, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer <= 325)
			{
				self.ArtyGoalPos = ((UpdatableAndDeletable)self).room.MiddleOfTile(116, 18);
				return new InputPackage(false, Preset.None, 1, 1, true, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer > 325)
			{
				bool flag = false;
				int num = 0;
				if (((PhysicalObject)self.artyPlayerPuppet).firstChunk.pos.x < self.ArtyGoalPos.x - 9f)
				{
					num = 1;
				}
				else if (((PhysicalObject)self.artyPlayerPuppet).firstChunk.pos.x > self.ArtyGoalPos.x + 9f)
				{
					num = -1;
					flag = ((ArtificerDream)self).sceneTimer % 20 <= 5 && (ExtEnum<BodyModeIndex>)(object)self.artyPlayerPuppet.bodyMode != (ExtEnum<BodyModeIndex>)(object)BodyModeIndex.ClimbingOnBeam;
				}
				int num2 = 0;
				if ((ExtEnum<BodyModeIndex>)(object)self.artyPlayerPuppet.bodyMode == (ExtEnum<BodyModeIndex>)(object)BodyModeIndex.ClimbingOnBeam)
				{
					if (((PhysicalObject)self.artyPlayerPuppet).firstChunk.pos.y < self.ArtyGoalPos.y - 5f)
					{
						num2 = ((!(((PhysicalObject)self.artyPlayerPuppet).firstChunk.pos.x > self.ArtyGoalPos.x + 9f) || ((ArtificerDream)self).sceneTimer % 20 > 5) ? 1 : 0);
					}
					else if (((PhysicalObject)self.artyPlayerPuppet).firstChunk.pos.y > self.ArtyGoalPos.y + 5f)
					{
						num2 = -1;
					}
				}
				else
				{
					num2 = Random.Range(0, 2);
				}
				if (firstAlivePlayer != null && Mathf.Abs(((UpdatableAndDeletable)self).room.cameraPositions[2].x + ((UpdatableAndDeletable)self).room.game.cameras[0].sSize.x / 2f - ((PhysicalObject)(Player)firstAlivePlayer.realizedCreature).firstChunk.pos.x) > 1000f && ((ArtificerDream)self).sceneTimer < 2000)
				{
					((ArtificerDream)self).sceneTimer = 1999;
				}
				else if (self.pup2Puppet.state.dead && ((ArtificerDream)self).sceneTimer < 2000)
				{
					((ArtificerDream)self).sceneTimer = 1999;
				}
				return new InputPackage(false, Preset.None, num, num2, flag, false, false, false, false);
			}
			return default(InputPackage);
		}

		private static void ArtificerDream_4_SceneSetup(orig_SceneSetup orig, ArtificerDream_4 self)
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Expected O, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Expected O, but got Unknown
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Expected O, but got Unknown
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Expected O, but got Unknown
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Expected O, but got Unknown
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Expected O, but got Unknown
			//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Expected O, but got Unknown
			if (self.artificerPuppet == null)
			{
				self.artificerPuppet = new AbstractCreature(((UpdatableAndDeletable)self).room.world, StaticWorld.GetCreatureTemplate(Type.Slugcat), (Creature)null, new WorldCoordinate(((UpdatableAndDeletable)self).room.abstractRoom.index, 52, 42, -1), ((UpdatableAndDeletable)self).room.game.GetNewID());
				self.artificerPuppet.state = (CreatureState)new PlayerState(self.artificerPuppet, 0, SlugcatStatsName.Artificer, true);
				((UpdatableAndDeletable)self).room.abstractRoom.AddEntity((AbstractWorldEntity)(object)self.artificerPuppet);
				((AbstractPhysicalObject)self.artificerPuppet).RealizeInRoom();
				self.pup2Puppet = new AbstractCreature(((UpdatableAndDeletable)self).room.world, StaticWorld.GetCreatureTemplate(Type.Slugcat), (Creature)null, new WorldCoordinate(((UpdatableAndDeletable)self).room.abstractRoom.index, 52, 42, -1), ((UpdatableAndDeletable)self).room.game.GetNewID());
				((EntityID)(ref ((AbstractWorldEntity)self.pup2Puppet).ID)).setAltSeed(1001);
				self.pup2Puppet.state = (CreatureState)new PlayerState(self.pup2Puppet, 0, SlugcatStatsName.Slugpup, true);
				((UpdatableAndDeletable)self).room.abstractRoom.AddEntity((AbstractWorldEntity)(object)self.pup2Puppet);
				((AbstractPhysicalObject)self.pup2Puppet).RealizeInRoom();
			}
			AbstractCreature firstAlivePlayer = ((UpdatableAndDeletable)self).room.game.FirstAlivePlayer;
			if (firstAlivePlayer != null && firstAlivePlayer.realizedCreature != null)
			{
				Player val = (Player)firstAlivePlayer.realizedCreature;
				Room room = ((UpdatableAndDeletable)self).room;
				WorldCoordinate val2 = new WorldCoordinate(((UpdatableAndDeletable)self).room.abstractRoom.index, 50, 44, -1);
				val.SuperHardSetPosition(room.MiddleOfTile(((WorldCoordinate)(ref val2)).Tile));
			}
			if (self.artyPlayerPuppet == null && self.artificerPuppet.realizedCreature != null)
			{
				ref Player artyPlayerPuppet = ref self.artyPlayerPuppet;
				Creature realizedCreature = self.artificerPuppet.realizedCreature;
				artyPlayerPuppet = (Player)(object)((realizedCreature is Player) ? realizedCreature : null);
			}
			if (self.pup2PlayerPuppet == null && self.pup2Puppet.realizedCreature != null)
			{
				ref Player pup2PlayerPuppet = ref self.pup2PlayerPuppet;
				Creature realizedCreature2 = self.pup2Puppet.realizedCreature;
				pup2PlayerPuppet = (Player)(object)((realizedCreature2 is Player) ? realizedCreature2 : null);
				self.pup2PlayerPuppet.controller = (PlayerController)new StartController((ArtificerDream)(object)self, 1);
			}
			if (firstAlivePlayer != null && self.artyPlayerPuppet != null && self.pup2PlayerPuppet != null && firstAlivePlayer.realizedCreature != null)
			{
				self.artyPlayerPuppet.controller = (PlayerController)new StartController((ArtificerDream)(object)self, 0);
				self.artyPlayerPuppet.standing = true;
				((Player)firstAlivePlayer.realizedCreature).controller = (PlayerController)new StartController((ArtificerDream)(object)self, 2);
				AbstractDataPearl val3 = new AbstractDataPearl(((UpdatableAndDeletable)self).room.world, AbstractObjectType.DataPearl, (PhysicalObject)null, new WorldCoordinate(((UpdatableAndDeletable)self).room.abstractRoom.index, 50, 42, -1), ((UpdatableAndDeletable)self).room.game.GetNewID(), ((UpdatableAndDeletable)self).room.abstractRoom.index, -1, (ConsumableObjectData)null, DataPearlType.Misc2);
				((AbstractPhysicalObject)val3).RealizeInRoom();
				((ArtificerDream)self).sceneStarted = true;
			}
		}

		private static InputPackage ArtificerDream_4_GetInput(orig_GetInput orig, ArtificerDream_4 self, int index)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_062c: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_028f: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0401: Unknown result type (might be due to invalid IL or missing references)
			//IL_0368: Unknown result type (might be due to invalid IL or missing references)
			//IL_036d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0389: Unknown result type (might be due to invalid IL or missing references)
			//IL_038e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_03af: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_044a: Unknown result type (might be due to invalid IL or missing references)
			//IL_044f: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0620: Unknown result type (might be due to invalid IL or missing references)
			//IL_0626: Unknown result type (might be due to invalid IL or missing references)
			//IL_0628: Unknown result type (might be due to invalid IL or missing references)
			//IL_0615: Unknown result type (might be due to invalid IL or missing references)
			//IL_061a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0578: Unknown result type (might be due to invalid IL or missing references)
			//IL_057d: Unknown result type (might be due to invalid IL or missing references)
			int num = 219;
			int num2 = num + 60;
			int num3 = 400;
			if (index == 2)
			{
				AbstractCreature firstAlivePlayer = ((UpdatableAndDeletable)self).room.game.FirstAlivePlayer;
				if (((ArtificerDream)self).sceneTimer < num - 109)
				{
					return default(InputPackage);
				}
				if (((ArtificerDream)self).sceneTimer != num2 + 6)
				{
					if (firstAlivePlayer != null)
					{
						((Player)firstAlivePlayer.realizedCreature).standing = true;
					}
					return new InputPackage(false, Preset.None, 1, 0, false, false, false, false, false);
				}
				if (firstAlivePlayer != null)
				{
					((Player)firstAlivePlayer.realizedCreature).controller = null;
				}
			}
			if (((ArtificerDream)self).sceneTimer < num - 50 && (index == 0 || index == 2))
			{
				int pyroJumpCounter = 1;
				self.artyPlayerPuppet.standing = true;
				self.artyPlayerPuppet.pyroJumpCounter = pyroJumpCounter;
				return new InputPackage(false, Preset.None, 1, 0, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer < num + 25)
			{
				self.artyPlayerPuppet.standing = true;
				self.pup2PlayerPuppet.standing = true;
				if (index == 0 && ((ArtificerDream)self).sceneTimer > num - 10)
				{
					if (((ArtificerDream)self).sceneTimer >= num && ((ArtificerDream)self).sceneTimer < num + 10)
					{
						return new InputPackage(false, Preset.None, 1, 1, true, false, false, false, false);
					}
					if (((ArtificerDream)self).sceneTimer == num + 10)
					{
						return new InputPackage(false, Preset.None, 1, 1, false, false, true, false, false);
					}
					if (((ArtificerDream)self).sceneTimer == num + 11)
					{
						return new InputPackage(false, Preset.None, 1, 1, true, false, true, false, false);
					}
					if (((ArtificerDream)self).sceneTimer > num + 11)
					{
						return new InputPackage(false, Preset.None, 1, 1, true, false, true, false, false);
					}
					return new InputPackage(false, Preset.None, 1, 1, false, false, false, false, false);
				}
			}
			if (((ArtificerDream)self).sceneTimer < num2 && index == 0)
			{
				return new InputPackage(false, Preset.None, 0, 1, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer == num2 && index == 0)
			{
				return new InputPackage(false, Preset.None, 1, 1, true, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer <= num2 + 15 && index == 0)
			{
				return new InputPackage(false, Preset.None, 1, 1, true, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer == num2 + 16 && index == 0)
			{
				return new InputPackage(false, Preset.None, 1, 1, false, false, true, false, false);
			}
			if (((ArtificerDream)self).sceneTimer <= num2 + 20 && index == 0)
			{
				if (((ArtificerDream)self).sceneTimer == num2 + 17)
				{
				}
				if (((ArtificerDream)self).sceneTimer == num2 + 18)
				{
					((Creature)self.pup2PlayerPuppet).Stun(60);
					((PhysicalObject)self.pup2PlayerPuppet).bodyChunks[0].vel = new Vector2(14f, 16f);
					((PhysicalObject)self.pup2PlayerPuppet).bodyChunks[1].vel = new Vector2(14f, 16f);
					((PhysicalObject)self.artyPlayerPuppet).bodyChunks[0].vel = new Vector2(17f, 38f);
					((PhysicalObject)self.artyPlayerPuppet).bodyChunks[1].vel = new Vector2(17f, 38f);
				}
				self.artyPlayerPuppet.playerState.permanentDamageTracking = 0.5;
				return new InputPackage(false, Preset.None, 1, 1, true, false, true, false, false);
			}
			if (((ArtificerDream)self).sceneTimer < num2 + 100 && index == 1)
			{
				self.pup2PlayerPuppet.standing = true;
				return new InputPackage(false, Preset.None, 1, (((ArtificerDream)self).sceneTimer < num2 + 102) ? 1 : 0, true, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer > num3 && ((ArtificerDream)self).sceneTimer < 800 && index == 0)
			{
				if (((PhysicalObject)self.artyPlayerPuppet).firstChunk.pos.x < 1939f)
				{
					BodyChunk firstChunk = ((PhysicalObject)self.artyPlayerPuppet).firstChunk;
					firstChunk.vel += new Vector2(0f, 1f);
					self.artyPlayerPuppet.playerState.permanentDamageTracking = 0.0;
				}
				else
				{
					self.artyPlayerPuppet.playerState.permanentDamageTracking = 0.5;
				}
				int num4 = ((((ArtificerDream)self).sceneTimer < num3 + 2) ? 1 : 0);
				if (!self.artyPlayerPuppet.standing && (double)Random.value < 0.1)
				{
					num4 = 1;
				}
				if (!self.pup2PlayerPuppet.standing && (double)Random.value < 0.1)
				{
					num4 = 1;
				}
				return new InputPackage(false, Preset.None, 1, num4, false, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer > num2 + 100 && ((ArtificerDream)self).sceneTimer < 700 && index == 1)
			{
				return new InputPackage(false, Preset.None, 1, 0, true, false, false, false, false);
			}
			if (((ArtificerDream)self).sceneTimer > num2 + 101 && ((ArtificerDream)self).sceneTimer < 700 && index == 1)
			{
				return new InputPackage(false, Preset.None, 1, 0, Mathf.Floor(Mathf.Pow(Random.value + 0.5f, 4f)) == 1f, false, false, false, false);
			}
			return default(InputPackage);
		}

		private static void ArtificerDream_5_SceneSetup(orig_SceneSetup orig, ArtificerDream_5 self)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected O, but got Unknown
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Expected O, but got Unknown
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Expected O, but got Unknown
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Expected O, but got Unknown
			self.UpdateCycle(0);
			if (self.artificerPuppet == null)
			{
				self.artificerPuppet = new AbstractCreature(((UpdatableAndDeletable)self).room.world, StaticWorld.GetCreatureTemplate(Type.Slugcat), (Creature)null, new WorldCoordinate(((UpdatableAndDeletable)self).room.abstractRoom.index, 120, 46, -1), ((UpdatableAndDeletable)self).room.game.GetNewID());
				self.artificerPuppet.state = (CreatureState)new PlayerState(self.artificerPuppet, 0, SlugcatStatsName.Artificer, true);
				((UpdatableAndDeletable)self).room.abstractRoom.AddEntity((AbstractWorldEntity)(object)self.artificerPuppet);
				((AbstractPhysicalObject)self.artificerPuppet).RealizeInRoom();
			}
			if (self.artyPlayerPuppet == null && self.artificerPuppet.realizedCreature != null)
			{
				ref Player artyPlayerPuppet = ref self.artyPlayerPuppet;
				Creature realizedCreature = self.artificerPuppet.realizedCreature;
				artyPlayerPuppet = (Player)(object)((realizedCreature is Player) ? realizedCreature : null);
			}
			AbstractCreature firstAlivePlayer = ((UpdatableAndDeletable)self).room.game.FirstAlivePlayer;
			if (firstAlivePlayer != null && self.artyPlayerPuppet != null && firstAlivePlayer.realizedCreature != null)
			{
				self.artyPlayerPuppet.controller = (PlayerController)new StartController((ArtificerDream)(object)self, 0);
				self.artyPlayerPuppet.standing = true;
				((Player)firstAlivePlayer.realizedCreature).SuperHardSetPosition(((PhysicalObject)self.artyPlayerPuppet).firstChunk.pos);
				((Player)firstAlivePlayer.realizedCreature).controller = (PlayerController)new StartController((ArtificerDream)(object)self, 1);
				((ArtificerDream)self).sceneStarted = true;
				self.SpawnLeeches();
			}
		}
	}
}
