using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using IL;
using IL.Menu;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using On.Menu;
using SlugBase.Features;

namespace SlugBase.Assets;

internal static class AssetHooks
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static Manipulator _003C0_003E__MenuIllustration_LoadFile_string;

		public static hook_BuildScene _003C1_003E__MenuScene_BuildScene;

		public static Manipulator _003C2_003E__SlugcatSelectMenu_StartGame;

		public static Manipulator _003C3_003E__RainWorldGame_ExitToVoidSeaSlideShow;

		public static Manipulator _003C4_003E__SlideShow_ctor;

		public static hook_ctor _003C5_003E__SlideShowMenuScene_ctor;
	}

	private const string SLUGBASE_FOLDER = "SlugBase Assets";

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
		object obj = _003C_003EO._003C0_003E__MenuIllustration_LoadFile_string;
		if (obj == null)
		{
			Manipulator val = MenuIllustration_LoadFile_string;
			_003C_003EO._003C0_003E__MenuIllustration_LoadFile_string = val;
			obj = (object)val;
		}
		MenuIllustration.LoadFile_string += (Manipulator)obj;
		object obj2 = _003C_003EO._003C1_003E__MenuScene_BuildScene;
		if (obj2 == null)
		{
			hook_BuildScene val2 = MenuScene_BuildScene;
			_003C_003EO._003C1_003E__MenuScene_BuildScene = val2;
			obj2 = (object)val2;
		}
		MenuScene.BuildScene += (hook_BuildScene)obj2;
		object obj3 = _003C_003EO._003C2_003E__SlugcatSelectMenu_StartGame;
		if (obj3 == null)
		{
			Manipulator val3 = SlugcatSelectMenu_StartGame;
			_003C_003EO._003C2_003E__SlugcatSelectMenu_StartGame = val3;
			obj3 = (object)val3;
		}
		SlugcatSelectMenu.StartGame += (Manipulator)obj3;
		object obj4 = _003C_003EO._003C3_003E__RainWorldGame_ExitToVoidSeaSlideShow;
		if (obj4 == null)
		{
			Manipulator val4 = RainWorldGame_ExitToVoidSeaSlideShow;
			_003C_003EO._003C3_003E__RainWorldGame_ExitToVoidSeaSlideShow = val4;
			obj4 = (object)val4;
		}
		RainWorldGame.ExitToVoidSeaSlideShow += (Manipulator)obj4;
		object obj5 = _003C_003EO._003C4_003E__SlideShow_ctor;
		if (obj5 == null)
		{
			Manipulator val5 = SlideShow_ctor;
			_003C_003EO._003C4_003E__SlideShow_ctor = val5;
			obj5 = (object)val5;
		}
		SlideShow.ctor += (Manipulator)obj5;
		object obj6 = _003C_003EO._003C5_003E__SlideShowMenuScene_ctor;
		if (obj6 == null)
		{
			hook_ctor val6 = SlideShowMenuScene_ctor;
			_003C_003EO._003C5_003E__SlideShowMenuScene_ctor = val6;
			obj6 = (object)val6;
		}
		SlideShowMenuScene.ctor += (hook_ctor)obj6;
		CustomDreams.Apply();
	}

	private static void MenuIllustration_LoadFile_string(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		ILCursor val = new ILCursor(il);
		if (val.TryGotoNext((MoveType)0, new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchCallOrCallvirt<AssetManager>(x, "ResolveFilePath")
		}))
		{
			val.EmitDelegate<Func<string, string>>((Func<string, string>)((string path) => path.StartsWith("SlugBase Assets" + Path.DirectorySeparatorChar) ? path.Substring("SlugBase Assets".Length + 1) : path));
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook MenuIllustration_LoadFile_string failed!");
		}
	}

	private static void MenuScene_BuildScene(orig_BuildScene orig, MenuScene self)
	{
		orig.Invoke(self);
		Menu menu = ((MenuObject)self).menu;
		SlideShow val = (SlideShow)(object)((menu is SlideShow) ? menu : null);
		CustomScene scene2;
		if (val != null && CustomSlideshow.Registry.TryGet(val.slideShowID, out var value))
		{
			scene2 = value.GetScene(self.sceneID);
			if (scene2 != null)
			{
				self.sceneFolder = scene2.SceneFolder ?? value.SlideshowFolder ?? "";
				AddImages(self, scene2);
			}
			else
			{
				SlugBasePlugin.Logger.LogError($"Could not find slideshow scene with ID {self.sceneID}!");
			}
		}
		else if (CustomScene.Registry.TryGet(self.sceneID, out scene2))
		{
			self.sceneFolder = scene2.SceneFolder ?? "";
			AddImages(self, scene2);
		}
		static void AddImages(MenuScene self, CustomScene scene)
		{
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Expected O, but got Unknown
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Expected O, but got Unknown
			bool flag = self.flatMode && scene.Images.Any((CustomScene.Image img) => img.Flatmode);
			CustomScene.Image[] images = scene.Images;
			foreach (CustomScene.Image image in images)
			{
				if (image.Flatmode == flag)
				{
					if (image.Depth != -1f)
					{
						self.AddIllustration((MenuIllustration)new MenuDepthIllustration(((MenuObject)self).menu, (MenuObject)(object)self, "SlugBase Assets", Path.Combine(self.sceneFolder, image.Name), image.Position, image.Depth, image.Shader));
					}
					else
					{
						self.AddIllustration(new MenuIllustration(((MenuObject)self).menu, (MenuObject)(object)self, "SlugBase Assets", Path.Combine(self.sceneFolder, image.Name), image.Position, false, false));
					}
				}
			}
			((InteractiveMenuScene)(((self is InteractiveMenuScene) ? self : null)?)).idleDepths.AddRange(scene.IdleDepths);
		}
	}

	public static void SlugcatSelectMenu_StartGame(ILContext il)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		ILLabel postNameCheck = null;
		if (val.TryGotoNext((MoveType)2, new Func<Instruction, bool>[3]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdsfld<Name>(x, "White"),
			(Instruction x) => ILPatternMatchingExt.MatchCall(x, (MethodBase)typeof(ExtEnum<Name>).GetMethod("op_Inequality")),
			(Instruction x) => ILPatternMatchingExt.MatchBrfalse(x, ref postNameCheck)
		}))
		{
			val.Emit(OpCodes.Ldarg_1);
			val.EmitDelegate<Func<Name, bool>>((Func<Name, bool>)((Name storyGameCharacter) => SlugBaseCharacter.TryGet(storyGameCharacter, out var character2) && ((Feature<SlideShowID>)GameFeatures.IntroScene).TryGet(character2, out SlideShowID _)));
			val.Emit(OpCodes.Brtrue, (object)postNameCheck);
			if (val.TryGotoNext((MoveType)2, new Func<Instruction, bool>[2]
			{
				(Instruction x) => ILPatternMatchingExt.MatchLdsfld<SlideShowID>(x, "WhiteIntro"),
				(Instruction x) => ILPatternMatchingExt.MatchStfld<ProcessManager>(x, "nextSlideshow")
			}))
			{
				val.Emit(OpCodes.Ldarg_0);
				val.Emit(OpCodes.Ldarg_1);
				val.EmitDelegate<Action<SlugcatSelectMenu, Name>>((Action<SlugcatSelectMenu, Name>)delegate(SlugcatSelectMenu self, Name storyGameCharacter)
				{
					if (SlugBaseCharacter.TryGet(storyGameCharacter, out var character) && ((Feature<SlideShowID>)GameFeatures.IntroScene).TryGet(character, out SlideShowID value))
					{
						((MainLoopProcess)self).manager.nextSlideshow = value;
					}
				});
			}
			else
			{
				SlugBasePlugin.Logger.LogError("IL hook SlugcatSelectMenu_StartGame, set next slideshow, failed!");
			}
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook SlugcatSelectMenu_StartGame, match name, failed!");
		}
	}

	public static void RainWorldGame_ExitToVoidSeaSlideShow(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		if (val.TryGotoNext(new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchCallOrCallvirt<ProcessManager>(x, "RequestMainProcessSwitch")
		}) && val.TryGotoPrev(new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchLdarg(x, 0)
		}))
		{
			val.Emit(OpCodes.Ldarg_0);
			val.EmitDelegate<Action<RainWorldGame>>((Action<RainWorldGame>)delegate(RainWorldGame self)
			{
				if (SlugBaseCharacter.TryGet(self.StoryCharacter, out var character) && ((Feature<SlideShowID>)GameFeatures.OutroScene).TryGet(character, out SlideShowID value))
				{
					((MainLoopProcess)self).manager.nextSlideshow = value;
				}
			});
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook RainWorldGame_ExitToVoidSeaSlideShow failed!");
		}
	}

	public static void SlideShow_ctor(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		if (val.TryGotoNext((MoveType)2, new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchStfld<SlideShow>(x, "playList")
		}))
		{
			val.Emit(OpCodes.Ldarg_0);
			val.Emit(OpCodes.Ldarg_1);
			val.Emit(OpCodes.Ldarg_2);
			val.EmitDelegate<Action<SlideShow, ProcessManager, SlideShowID>>((Action<SlideShow, ProcessManager, SlideShowID>)delegate(SlideShow self, ProcessManager manager, SlideShowID slideShowID)
			{
				//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cb: Expected O, but got Unknown
				if (CustomSlideshow.Registry.TryGet(slideShowID, out var value))
				{
					try
					{
						if (manager.musicPlayer != null && value.Music != null)
						{
							self.waitForMusic = value.Music.Name;
							self.stall = false;
							manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, value.Music.FadeIn);
						}
					}
					catch (Exception arg)
					{
						SlugBasePlugin.Logger.LogError($"Failed to load slideshow music!\n{arg}");
					}
					CustomSlideshow.CustomSlideshowScene[] scenes = value.Scenes;
					foreach (CustomSlideshow.CustomSlideshowScene customSlideshowScene in scenes)
					{
						self.playList.Add(new Scene(customSlideshowScene.ID, customSlideshowScene.StartAt, customSlideshowScene.FadeInDoneAt, customSlideshowScene.FadeOutStartAt));
					}
					self.processAfterSlideShow = value.Process;
				}
			});
		}
		else
		{
			SlugBasePlugin.Logger.LogError("IL hook SlideShow_ctor failed!");
		}
	}

	public static void SlideShowMenuScene_ctor(orig_ctor orig, SlideShowMenuScene self, Menu menu, MenuObject owner, SceneID sceneID)
	{
		orig.Invoke(self, menu, owner, sceneID);
		if (((MenuScene)self).flatMode)
		{
			return;
		}
		Menu menu2 = ((MenuObject)self).menu;
		SlideShow val = (SlideShow)(object)((menu2 is SlideShow) ? menu2 : null);
		if (val != null && CustomSlideshow.Registry.TryGet(val.slideShowID, out var value))
		{
			CustomSlideshow.CustomSlideshowScene scene = value.GetScene(sceneID);
			if (scene != null)
			{
				self.cameraMovementPoints.AddRange(scene.CameraMovement);
			}
			else
			{
				SlugBasePlugin.Logger.LogError($"Could not find slideshow scene with ID {sceneID}!");
			}
		}
	}
}
