using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HUD;
using IL;
using IL.HUD;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using On;

namespace SlugBase.Features;

internal static class WorldHooks
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static Manipulator _003C0_003E__InheritMapFile_Map;

		public static Manipulator _003C1_003E__InheritMapFile_World;

		public static Manipulator _003C2_003E__PropertiesConfig;

		public static hook_ctor_string_int_int_Timeline _003C3_003E__Region_ctor;

		public static hook_SlugcatToTimeline _003C4_003E__SlugcatStats_SlugcatToTimeline;

		public static hook_SlugcatOptionalRegions _003C5_003E__SlugcatStats_SlugcatOptionalRegions;

		public static hook_SlugcatStoryRegions _003C6_003E__SlugcatStats_SlugcatStoryRegions;

		public static hook_GetProperRegionAcronym_Timeline_string _003C7_003E__Region_GetProperRegionAcronym;

		public static Manipulator _003C8_003E__WorldLoader_ctor_ILHook;

		public static hook_ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues _003C9_003E__WorldLoader_ctor;

		public static hook_CreatingWorld _003C10_003E__WorldLoader_CreatingWorld;

		public static hook_GetRegionFullName _003C11_003E__Region_GetRegionFullName;

		public static hook_ctor_Room_string_Region_bool_bool_Timeline_RainWorldGame _003C12_003E__RoomSettings_ctor;

		public static hook_setCustomFlags _003C13_003E__AbstractCreature_setCustomFlags;

		public static hook_RefreshTimelineList _003C14_003E__FilterData_RefreshTimelineList;

		public static Func<Timeline, IEnumerable<Name>> _003C15_003E__GetSlugcats;
	}

	[CompilerGenerated]
	private sealed class _003CGetSlugcats_003Ed__12 : IEnumerable<Name>, IEnumerable, IEnumerator<Name>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Name _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Timeline timeline;

		public Timeline _003C_003E3__timeline;

		private List<string>.Enumerator _003C_003Es__1;

		private string _003Cname_003E5__2;

		private Name _003Cslugcat_003E5__3;

		Name IEnumerator<Name>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetSlugcats_003Ed__12(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003Es__1 = default(List<string>.Enumerator);
			_003Cname_003E5__2 = null;
			_003Cslugcat_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -3;
					goto IL_00a5;
				}
				_003C_003E1__state = -1;
				_003C_003Es__1 = ExtEnum<Name>.values.entries.GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_00b4;
				IL_00a5:
				_003Cslugcat_003E5__3 = null;
				_003Cname_003E5__2 = null;
				goto IL_00b4;
				IL_00b4:
				if (_003C_003Es__1.MoveNext())
				{
					_003Cname_003E5__2 = _003C_003Es__1.Current;
					_003Cslugcat_003E5__3 = new Name(_003Cname_003E5__2, false);
					if ((ExtEnum<Timeline>)(object)SlugcatStats.SlugcatToTimeline(_003Cslugcat_003E5__3) == (ExtEnum<Timeline>)(object)timeline)
					{
						_003C_003E2__current = _003Cslugcat_003E5__3;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_00a5;
				}
				_003C_003Em__Finally1();
				_003C_003Es__1 = default(List<string>.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003Es__1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Name> IEnumerable<Name>.GetEnumerator()
		{
			_003CGetSlugcats_003Ed__12 _003CGetSlugcats_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetSlugcats_003Ed__ = this;
			}
			else
			{
				_003CGetSlugcats_003Ed__ = new _003CGetSlugcats_003Ed__12(0);
			}
			_003CGetSlugcats_003Ed__.timeline = _003C_003E3__timeline;
			return _003CGetSlugcats_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Name>)this).GetEnumerator();
		}
	}

	private static ConditionalWeakTable<WorldLoader, StrongBox<Timeline>> _actualTimeline = new ConditionalWeakTable<WorldLoader, StrongBox<Timeline>>();

	/// <summary>Path.DirectorySeparatorChar but shorter, because it takes too much space</summary>
	public static char Separator => Path.DirectorySeparatorChar;

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
		object obj = _003C_003EO._003C0_003E__InheritMapFile_Map;
		if (obj == null)
		{
			Manipulator val = InheritMapFile_Map;
			_003C_003EO._003C0_003E__InheritMapFile_Map = val;
			obj = (object)val;
		}
		Map.LoadConnectionPositions += (Manipulator)obj;
		object obj2 = _003C_003EO._003C0_003E__InheritMapFile_Map;
		if (obj2 == null)
		{
			Manipulator val2 = InheritMapFile_Map;
			_003C_003EO._003C0_003E__InheritMapFile_Map = val2;
			obj2 = (object)val2;
		}
		Map.Update += (Manipulator)obj2;
		object obj3 = _003C_003EO._003C1_003E__InheritMapFile_World;
		if (obj3 == null)
		{
			Manipulator val3 = InheritMapFile_World;
			_003C_003EO._003C1_003E__InheritMapFile_World = val3;
			obj3 = (object)val3;
		}
		World.LoadWorldForFastTravel_Timeline_List1_Int32Array_Int32Array_Int32Array += (Manipulator)obj3;
		object obj4 = _003C_003EO._003C1_003E__InheritMapFile_World;
		if (obj4 == null)
		{
			Manipulator val4 = InheritMapFile_World;
			_003C_003EO._003C1_003E__InheritMapFile_World = val4;
			obj4 = (object)val4;
		}
		World.LoadMapConfig_Timeline += (Manipulator)obj4;
		object obj5 = _003C_003EO._003C2_003E__PropertiesConfig;
		if (obj5 == null)
		{
			Manipulator val5 = PropertiesConfig;
			_003C_003EO._003C2_003E__PropertiesConfig = val5;
			obj5 = (object)val5;
		}
		World.LoadMapConfig_Timeline += (Manipulator)obj5;
		object obj6 = _003C_003EO._003C3_003E__Region_ctor;
		if (obj6 == null)
		{
			hook_ctor_string_int_int_Timeline val6 = Region_ctor;
			_003C_003EO._003C3_003E__Region_ctor = val6;
			obj6 = (object)val6;
		}
		Region.ctor_string_int_int_Timeline += (hook_ctor_string_int_int_Timeline)obj6;
		object obj7 = _003C_003EO._003C4_003E__SlugcatStats_SlugcatToTimeline;
		if (obj7 == null)
		{
			hook_SlugcatToTimeline val7 = SlugcatStats_SlugcatToTimeline;
			_003C_003EO._003C4_003E__SlugcatStats_SlugcatToTimeline = val7;
			obj7 = (object)val7;
		}
		SlugcatStats.SlugcatToTimeline += (hook_SlugcatToTimeline)obj7;
		object obj8 = _003C_003EO._003C5_003E__SlugcatStats_SlugcatOptionalRegions;
		if (obj8 == null)
		{
			hook_SlugcatOptionalRegions val8 = SlugcatStats_SlugcatOptionalRegions;
			_003C_003EO._003C5_003E__SlugcatStats_SlugcatOptionalRegions = val8;
			obj8 = (object)val8;
		}
		SlugcatStats.SlugcatOptionalRegions += (hook_SlugcatOptionalRegions)obj8;
		object obj9 = _003C_003EO._003C6_003E__SlugcatStats_SlugcatStoryRegions;
		if (obj9 == null)
		{
			hook_SlugcatStoryRegions val9 = SlugcatStats_SlugcatStoryRegions;
			_003C_003EO._003C6_003E__SlugcatStats_SlugcatStoryRegions = val9;
			obj9 = (object)val9;
		}
		SlugcatStats.SlugcatStoryRegions += (hook_SlugcatStoryRegions)obj9;
		object obj10 = _003C_003EO._003C7_003E__Region_GetProperRegionAcronym;
		if (obj10 == null)
		{
			hook_GetProperRegionAcronym_Timeline_string val10 = Region_GetProperRegionAcronym;
			_003C_003EO._003C7_003E__Region_GetProperRegionAcronym = val10;
			obj10 = (object)val10;
		}
		Region.GetProperRegionAcronym_Timeline_string += (hook_GetProperRegionAcronym_Timeline_string)obj10;
		object obj11 = _003C_003EO._003C8_003E__WorldLoader_ctor_ILHook;
		if (obj11 == null)
		{
			Manipulator val11 = WorldLoader_ctor_ILHook;
			_003C_003EO._003C8_003E__WorldLoader_ctor_ILHook = val11;
			obj11 = (object)val11;
		}
		WorldLoader.ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues += (Manipulator)obj11;
		object obj12 = _003C_003EO._003C9_003E__WorldLoader_ctor;
		if (obj12 == null)
		{
			hook_ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues val12 = WorldLoader_ctor;
			_003C_003EO._003C9_003E__WorldLoader_ctor = val12;
			obj12 = (object)val12;
		}
		WorldLoader.ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues += (hook_ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues)obj12;
		object obj13 = _003C_003EO._003C10_003E__WorldLoader_CreatingWorld;
		if (obj13 == null)
		{
			hook_CreatingWorld val13 = WorldLoader_CreatingWorld;
			_003C_003EO._003C10_003E__WorldLoader_CreatingWorld = val13;
			obj13 = (object)val13;
		}
		WorldLoader.CreatingWorld += (hook_CreatingWorld)obj13;
		object obj14 = _003C_003EO._003C11_003E__Region_GetRegionFullName;
		if (obj14 == null)
		{
			hook_GetRegionFullName val14 = Region_GetRegionFullName;
			_003C_003EO._003C11_003E__Region_GetRegionFullName = val14;
			obj14 = (object)val14;
		}
		Region.GetRegionFullName += (hook_GetRegionFullName)obj14;
		object obj15 = _003C_003EO._003C12_003E__RoomSettings_ctor;
		if (obj15 == null)
		{
			hook_ctor_Room_string_Region_bool_bool_Timeline_RainWorldGame val15 = RoomSettings_ctor;
			_003C_003EO._003C12_003E__RoomSettings_ctor = val15;
			obj15 = (object)val15;
		}
		RoomSettings.ctor_Room_string_Region_bool_bool_Timeline_RainWorldGame += (hook_ctor_Room_string_Region_bool_bool_Timeline_RainWorldGame)obj15;
		object obj16 = _003C_003EO._003C13_003E__AbstractCreature_setCustomFlags;
		if (obj16 == null)
		{
			hook_setCustomFlags val16 = AbstractCreature_setCustomFlags;
			_003C_003EO._003C13_003E__AbstractCreature_setCustomFlags = val16;
			obj16 = (object)val16;
		}
		AbstractCreature.setCustomFlags += (hook_setCustomFlags)obj16;
		object obj17 = _003C_003EO._003C14_003E__FilterData_RefreshTimelineList;
		if (obj17 == null)
		{
			hook_RefreshTimelineList val17 = FilterData_RefreshTimelineList;
			_003C_003EO._003C14_003E__FilterData_RefreshTimelineList = val17;
			obj17 = (object)val17;
		}
		FilterData.RefreshTimelineList += (hook_RefreshTimelineList)obj17;
	}

	private static void FilterData_RefreshTimelineList(orig_RefreshTimelineList orig, FilterData self)
	{
		orig.Invoke(self);
		foreach (CustomTimeline value in CustomTimeline.Registry.Values)
		{
			if (self.availableOnTimelines.Contains(value.ID) && !value.Priorities.All(self.availableOnTimelines.Contains))
			{
				self.availableOnTimelines.Remove(value.ID);
			}
		}
	}

	private static void InheritMapFile_Map(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		try
		{
			val.GotoNext(new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchLdstr(x, "map_")
			});
			val.GotoNext((MoveType)2, new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchCall<AssetManager>(x, "ResolveFilePath")
			});
			val.Emit(OpCodes.Ldarg_0);
			val.EmitDelegate<Func<string, Map, string>>((Func<string, Map, string>)delegate(string origPath, Map map)
			{
				Timeline id = SlugcatStats.SlugcatToTimeline(((HudPart)map).hud.rainWorld.progression.PlayingAsSlugcat);
				if (CustomTimeline.Registry.TryGet(id, out var value))
				{
					string prefix = "World" + Separator + map.RegionName + Separator + "map_" + map.UseMapName() + "-";
					string suffix = ".png";
					return FindWorldFileOverride(value, prefix, suffix) ?? origPath;
				}
				return origPath;
			});
		}
		catch (Exception data)
		{
			SlugBasePlugin.Logger.LogError("Failed to hook InheritMapFile_Map!");
			SlugBasePlugin.Logger.LogError(data);
		}
	}

	private static void InheritMapFile_World(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		try
		{
			val.GotoNext(new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchLdstr(x, "-")
			});
			val.GotoNext((MoveType)2, new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchCall<AssetManager>(x, "ResolveFilePath")
			});
			val.Emit(OpCodes.Ldarg_0);
			val.Emit(OpCodes.Ldarg_1);
			val.EmitDelegate<Func<string, World, Timeline, string>>((Func<string, World, Timeline, string>)delegate(string origPath, World world, Timeline baseTimeline)
			{
				if (CustomTimeline.Registry.TryGet(baseTimeline, out var value))
				{
					string prefix = "World" + Separator + world.name + Separator + "map_" + WorldLoader.MapNameManipulator(world.name, world.game) + "-";
					string suffix = ".txt";
					return FindWorldFileOverride(value, prefix, suffix) ?? origPath;
				}
				return origPath;
			});
		}
		catch (Exception data)
		{
			SlugBasePlugin.Logger.LogError("Failed to hook InheritMapFile_World!");
			SlugBasePlugin.Logger.LogError(data);
		}
	}

	private static void PropertiesConfig(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		try
		{
			val.GotoNext(new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchLdstr(x, "Properties-")
			});
			val.GotoNext((MoveType)2, new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchCall<AssetManager>(x, "ResolveFilePath")
			});
			val.Emit(OpCodes.Ldarg_0);
			val.Emit(OpCodes.Ldarg_1);
			val.EmitDelegate<Func<string, World, Timeline, string>>((Func<string, World, Timeline, string>)delegate(string origPath, World world, Timeline baseTimeline)
			{
				if (CustomTimeline.Registry.TryGet(baseTimeline, out var value2))
				{
					string prefix = $"World{Separator}{world.name}{Separator}Properties-";
					string suffix = ".txt";
					return FindWorldFileOverride(value2, prefix, suffix) ?? origPath;
				}
				return origPath;
			});
			int partsLoc = -1;
			val.GotoNext(new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchLdstr(x, "Broken Shelters")
			});
			ILLabel val3 = default(ILLabel);
			val.GotoNext(new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchBrfalse(x, ref val3)
			});
			val.GotoNext(new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchLdloc(x, ref partsLoc)
			});
			ILLabel val2 = default(ILLabel);
			val.GotoNext(new Func<Instruction, bool>[1]
			{
				(Instruction x) => ILPatternMatchingExt.MatchBrfalse(x, ref val2)
			});
			val.Emit(OpCodes.Ldarg_1);
			val.Emit(OpCodes.Ldloc, partsLoc);
			val.EmitDelegate<Func<bool, Timeline, string[], bool>>((Func<bool, Timeline, string[], bool>)delegate(bool result, Timeline timeline, string[] parts)
			{
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Expected O, but got Unknown
				if (!result && CustomTimeline.Registry.TryGet(timeline, out var value))
				{
					Timeline item = new Timeline(parts[1].Trim(), false);
					result = value.Priorities.Contains(item);
				}
				return result;
			});
		}
		catch (Exception data)
		{
			SlugBasePlugin.Logger.LogError("Failed to hook PropertiesConfig!");
			SlugBasePlugin.Logger.LogError(data);
		}
	}

	private static void Region_ctor(orig_ctor_string_int_int_Timeline orig, Region self, string name, int firstRoomIndex, int regionNumber, Timeline timeline)
	{
		if (CustomTimeline.Registry.TryGet(timeline, out var value))
		{
			foreach (Timeline priority in value.Priorities)
			{
				string path = AssetManager.ResolveFilePath($"World{Separator}{name}{Separator}properties-{((ExtEnumBase)priority).value}.txt");
				if (File.Exists(path))
				{
					timeline = priority;
					break;
				}
			}
		}
		orig.Invoke(self, name, firstRoomIndex, regionNumber, timeline);
	}

	private static string FindWorldFileOverride(CustomTimeline customTimeline, string prefix, string suffix)
	{
		foreach (Timeline priority in customTimeline.Priorities)
		{
			string text = AssetManager.ResolveFilePath(prefix + ((ExtEnumBase)priority).value + suffix);
			if (File.Exists(text))
			{
				return text;
			}
		}
		return null;
	}

	private static Timeline SlugcatStats_SlugcatToTimeline(orig_SlugcatToTimeline orig, Name slugcat)
	{
		if (SlugBaseCharacter.TryGet(slugcat, out var character) && ((Feature<Timeline[]>)GameFeatures.WorldState).TryGet(character, out Timeline[] value))
		{
			return Utils.FirstValidEnum<Timeline>((IEnumerable<Timeline>)value);
		}
		return orig.Invoke(slugcat);
	}

	private static List<string> SlugcatStats_SlugcatOptionalRegions(orig_SlugcatOptionalRegions orig, Name i)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		if (!SlugBaseCharacter.TryGet(i, out var _))
		{
			return orig.Invoke(i);
		}
		Timeline val = SlugcatStats.SlugcatToTimeline(i);
		CustomTimeline value;
		List<Timeline> list = ((!CustomTimeline.Registry.TryGet(val, out value)) ? new List<Timeline> { val } : value.Priorities);
		HashSet<Name> hashSet = new HashSet<Name>();
		foreach (string entry in ExtEnum<Name>.values.entries)
		{
			Name val2 = new Name(entry, false);
			if (list.Contains(SlugcatStats.SlugcatToTimeline(val2)))
			{
				hashSet.Add(val2);
			}
		}
		HashSet<string> hashSet2 = new HashSet<string>();
		foreach (Name item in hashSet)
		{
			foreach (string item2 in orig.Invoke(item))
			{
				hashSet2.Add(item2);
			}
			if (!((ExtEnum<Name>)(object)item != (ExtEnum<Name>)(object)i))
			{
				continue;
			}
			foreach (string item3 in SlugcatStats.SlugcatStoryRegions(item))
			{
				hashSet2.Add(item3);
			}
		}
		foreach (string item4 in SlugcatStats.SlugcatStoryRegions(i))
		{
			if (hashSet2.Contains(item4))
			{
				hashSet2.Remove(item4);
			}
		}
		List<string> fullRegionOrder = Region.GetFullRegionOrder();
		for (int num = fullRegionOrder.Count - 1; num >= 0; num--)
		{
			if (!hashSet2.Contains(fullRegionOrder[num]))
			{
				fullRegionOrder.RemoveAt(num);
			}
		}
		return fullRegionOrder;
	}

	private static List<string> SlugcatStats_SlugcatStoryRegions(orig_SlugcatStoryRegions orig, Name i)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		if (!SlugBaseCharacter.TryGet(i, out var character))
		{
			return orig.Invoke(i);
		}
		List<string> list = null;
		Timeline val = SlugcatStats.SlugcatToTimeline(i);
		CustomTimeline value;
		List<Timeline> source = (CustomTimeline.Registry.TryGet(val, out value) ? value.Priorities : new List<Timeline> { val });
		CustomTimeline value3;
		Name val2 = source.Where((Timeline x) => !CustomTimeline.Registry.TryGet(x, out value3)).SelectMany(GetSlugcats).FirstOrDefault();
		if ((ExtEnum<Name>)(object)val2 != (ExtEnum<Name>)null)
		{
			list = SlugcatStats.SlugcatStoryRegions(val2);
			List<string> list2 = orig.Invoke(new Name("", false));
			foreach (string item in orig.Invoke(i))
			{
				if (!list2.Contains(item) && !list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		if (list == null || list.Count == 0)
		{
			list = orig.Invoke(i);
		}
		if (((Feature<string[]>)GameFeatures.StoryRegions).TryGet(character, out string[] value2))
		{
			string[] array = value2;
			foreach (string text in array)
			{
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				if (text.StartsWith("-"))
				{
					if (text.Length > 2 && list.Contains(text.Substring(1)))
					{
						list.Remove(text.Substring(1));
					}
				}
				else if (!list.Contains(text))
				{
					list.Add(text);
				}
			}
		}
		return list;
	}

	[IteratorStateMachine(typeof(_003CGetSlugcats_003Ed__12))]
	private static IEnumerable<Name> GetSlugcats(Timeline timeline)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetSlugcats_003Ed__12(-2)
		{
			_003C_003E3__timeline = timeline
		};
	}

	private static string Region_GetProperRegionAcronym(orig_GetProperRegionAcronym_Timeline_string orig, Timeline timeline, string baseAcronym)
	{
		if (CustomTimeline.Registry.TryGet(timeline, out var value))
		{
			timeline = FindFirstEquivalences(value.Priorities, baseAcronym);
		}
		return orig.Invoke(timeline, baseAcronym);
	}

	private static Timeline FindFirstEquivalences(IEnumerable<Timeline> names, string regionName)
	{
		string[] array = AssetManager.ListDirectory("World", true, false);
		foreach (Timeline name in names)
		{
			if (!CustomTimeline.Registry.TryGet(name, out var _))
			{
				return name;
			}
			for (int i = 0; i < array.Length; i++)
			{
				string path = AssetManager.ResolveFilePath($"World{Separator}{Path.GetFileName(array[i])}{Separator}equivalences.txt");
				if (!File.Exists(path))
				{
					continue;
				}
				string[] array2 = File.ReadAllText(path).Trim().Split(',');
				for (int j = 0; j < array2.Length; j++)
				{
					string text = null;
					string text2 = array2[j];
					if (array2[j].Contains("-"))
					{
						text2 = array2[j].Split('-')[0];
						text = array2[j].Split('-')[1];
					}
					if (text2 == regionName && text != null && ((ExtEnumBase)name).value.ToLower() == text.ToLower())
					{
						return name;
					}
				}
			}
		}
		return names.First();
	}

	public static StrongBox<Timeline> ActualTimeline(this WorldLoader p)
	{
		return _actualTimeline.GetValue(p, (WorldLoader _) => new StrongBox<Timeline>(null));
	}

	private static void WorldLoader_ctor(orig_ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues orig, WorldLoader self, RainWorldGame game, Name playerCharacter, Timeline timelinePosition, bool singleRoomWorld, string worldName, Region region, SetupValues setupValues)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		self.ActualTimeline().Value = timelinePosition;
		if (!singleRoomWorld && CustomTimeline.Registry.TryGet(timelinePosition, out var value) && value.Base.Length != 0)
		{
			string text = $"World{Separator}{worldName}{Separator}world_{worldName}.txt";
			string[] source = File.ReadAllLines(AssetManager.ResolveFilePath(text));
			timelinePosition = FirstTimelineMentionedInWorld(source.ToList(), value.Priorities);
		}
		orig.Invoke(self, game, playerCharacter, timelinePosition, singleRoomWorld, worldName, region, setupValues);
	}

	private static void WorldLoader_ctor_ILHook(ILContext il)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		ILCursor val = new ILCursor(il);
		if (val.TryGotoNext((MoveType)2, new Func<Instruction, bool>[1]
		{
			(Instruction x) => ILPatternMatchingExt.MatchStfld<WorldLoader>(x, "simulateUpdateTicks")
		}))
		{
			val.Emit(OpCodes.Ldarg_0);
			val.EmitDelegate<Action<WorldLoader>>((Action<WorldLoader>)delegate(WorldLoader self)
			{
				if (!self.singleRoomWorld && self.lines != null && self.lines.Count != 0)
				{
					int num = self.lines.IndexOf("CREATURES");
					int num2 = self.lines.IndexOf("END CREATURES");
					if (num != -1 && num2 != -1)
					{
						CustomTimeline value;
						IEnumerable<Timeline> enumerable = (IEnumerable<Timeline>)(CustomTimeline.Registry.TryGet(self.ActualTimeline().Value, out value) ? ((IList)value.Priorities) : ((IList)((!((ExtEnum<Timeline>)(object)self.ActualTimeline().Value != (ExtEnum<Timeline>)null)) ? new Timeline[0] : new Timeline[1] { self.ActualTimeline().Value })));
						foreach (Timeline item in enumerable)
						{
							string path = AssetManager.ResolveFilePath($"World{Separator}{self.worldName}{Separator}spawns_{self.worldName}-{((ExtEnumBase)item).value}.txt");
							if (File.Exists(path))
							{
								List<string> collection = (from x in File.ReadAllLines(path)
									where !x.StartsWith("//") && !string.IsNullOrEmpty(x) && x.Contains(":")
									select x).ToList();
								self.lines.RemoveRange(num + 1, num2 - num - 1);
								self.lines.InsertRange(num + 1, collection);
								break;
							}
						}
					}
				}
			});
		}
		else
		{
			SlugBasePlugin.Logger.LogError("Failed to IL hook WorldLoader.ctor!");
		}
	}

	private static Timeline FirstTimelineMentionedInWorld(List<string> lines, IEnumerable<Timeline> timelines)
	{
		int num = lines.IndexOf("CONDITIONAL LINKS");
		int num2 = lines.IndexOf("END CONDITIONAL LINKS");
		foreach (Timeline timeline in timelines)
		{
			if (num != -1 && num2 != -1)
			{
				foreach (string item in lines.GetRange(num, num2 - num))
				{
					if (!string.IsNullOrEmpty(item) && item.Contains(" : ") && Regex.Split(item, " : ")[0] == ((ExtEnumBase)timeline).value)
					{
						return timeline;
					}
				}
			}
			foreach (string line in lines)
			{
				if (string.IsNullOrEmpty(line) || line[0] != '(' || line.IndexOf(")") <= 0)
				{
					continue;
				}
				string text = line.Substring(1, line.IndexOf(")") - 1);
				if (text.StartsWith("X-"))
				{
					text = text.Substring(2);
				}
				string[] array = text.Split(',');
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (text2 == ((ExtEnumBase)timeline).value)
					{
						return timeline;
					}
				}
			}
		}
		return timelines.FirstOrDefault() ?? Timeline.White;
	}

	private static void WorldLoader_CreatingWorld(orig_CreatingWorld orig, WorldLoader self)
	{
		Timeline timelinePosition = self.timelinePosition;
		self.timelinePosition = self.ActualTimeline().Value;
		orig.Invoke(self);
		self.timelinePosition = timelinePosition;
	}

	private static void AbstractCreature_setCustomFlags(orig_setCustomFlags orig, AbstractCreature self)
	{
		orig.Invoke(self);
		if (ModManager.MSC && ((AbstractWorldEntity)self).Room.world.game.IsStorySession && CustomTimeline.Registry.TryGet(((AbstractWorldEntity)self).Room.world.game.TimelinePoint, out var value) && value.InheritsFrom(Timeline.Saint))
		{
			if (self.creatureTemplate.BlizzardAdapted)
			{
				self.HypothermiaImmune = true;
			}
			if (self.creatureTemplate.BlizzardWanderer)
			{
				self.ignoreCycle = true;
			}
		}
	}

	private static void RoomSettings_ctor(orig_ctor_Room_string_Region_bool_bool_Timeline_RainWorldGame orig, RoomSettings self, Room room, string name, Region region, bool template, bool firstTemplate, Timeline timelinePoint, RainWorldGame game)
	{
		if (CustomTimeline.Registry.TryGet(timelinePoint, out var value))
		{
			foreach (Timeline priority in value.Priorities)
			{
				string path = WorldLoader.FindRoomFile(name, false, "_settings-" + ((ExtEnumBase)priority).value + ".txt", true);
				if (File.Exists(path))
				{
					timelinePoint = priority;
					break;
				}
			}
		}
		orig.Invoke(self, room, name, region, template, firstTemplate, timelinePoint, game);
	}

	private static string Region_GetRegionFullName(orig_GetRegionFullName orig, string regionAcro, Name slugcatIndex)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		Timeline id = SlugcatStats.SlugcatToTimeline(slugcatIndex);
		if (CustomTimeline.Registry.TryGet(id, out var value))
		{
			foreach (Timeline priority in value.Priorities)
			{
				string path = AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{regionAcro}{Path.DirectorySeparatorChar}DisplayName-{((ExtEnumBase)priority).value}.txt");
				if (File.Exists(path))
				{
					slugcatIndex = new Name(((ExtEnumBase)priority).value, false);
					break;
				}
			}
		}
		return orig.Invoke(regionAcro, slugcatIndex);
	}
}
