using System.Runtime.CompilerServices;
using Menu;
using On;
using On.Menu;

namespace SlugBase.SaveData;

internal static class SaveDataHooks
{
	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static hook_MineForSaveData _003C0_003E__SlugcatSelectMenu_MineForSaveData;

		public static hook_SaveToString _003C1_003E__DeathPersistentSaveData_SaveToString;

		public static hook_ToString _003C2_003E__MiscWorldSaveData_ToString;

		public static hook_ToString _003C3_003E__MiscProgressionData_ToString;
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
		object obj = _003C_003EO._003C0_003E__SlugcatSelectMenu_MineForSaveData;
		if (obj == null)
		{
			hook_MineForSaveData val = SlugcatSelectMenu_MineForSaveData;
			_003C_003EO._003C0_003E__SlugcatSelectMenu_MineForSaveData = val;
			obj = (object)val;
		}
		SlugcatSelectMenu.MineForSaveData += (hook_MineForSaveData)obj;
		object obj2 = _003C_003EO._003C1_003E__DeathPersistentSaveData_SaveToString;
		if (obj2 == null)
		{
			hook_SaveToString val2 = DeathPersistentSaveData_SaveToString;
			_003C_003EO._003C1_003E__DeathPersistentSaveData_SaveToString = val2;
			obj2 = (object)val2;
		}
		DeathPersistentSaveData.SaveToString += (hook_SaveToString)obj2;
		object obj3 = _003C_003EO._003C2_003E__MiscWorldSaveData_ToString;
		if (obj3 == null)
		{
			hook_ToString val3 = MiscWorldSaveData_ToString;
			_003C_003EO._003C2_003E__MiscWorldSaveData_ToString = val3;
			obj3 = (object)val3;
		}
		MiscWorldSaveData.ToString += (hook_ToString)obj3;
		object obj4 = _003C_003EO._003C3_003E__MiscProgressionData_ToString;
		if (obj4 == null)
		{
			hook_ToString val4 = MiscProgressionData_ToString;
			_003C_003EO._003C3_003E__MiscProgressionData_ToString = val4;
			obj4 = (object)val4;
		}
		MiscProgressionData.ToString += (hook_ToString)obj4;
	}

	private static SaveGameData SlugcatSelectMenu_MineForSaveData(orig_MineForSaveData orig, ProcessManager manager, Name slugcat)
	{
		SaveGameData val = orig.Invoke(manager, slugcat);
		if (val != null)
		{
			MinedSaveData.Data.Add(val, new MinedSaveData(manager.rainWorld, slugcat));
		}
		return val;
	}

	private static string MiscProgressionData_ToString(orig_ToString orig, MiscProgressionData self)
	{
		self.GetSlugBaseData().SaveToStrings(self.unrecognizedSaveStrings);
		return orig.Invoke(self);
	}

	private static string MiscWorldSaveData_ToString(orig_ToString orig, MiscWorldSaveData self)
	{
		self.GetSlugBaseData().SaveToStrings(self.unrecognizedSaveStrings);
		return orig.Invoke(self);
	}

	private static string DeathPersistentSaveData_SaveToString(orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
	{
		self.GetSlugBaseData().SaveToStrings(self.unrecognizedSaveStrings);
		return orig.Invoke(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
	}
}
