namespace SlugBase.SaveData;

/// <summary>
/// Extensions to generate the <see cref="T:SlugBase.SaveData.SlugBaseSaveData" /> helper from the game's save data.
/// </summary>
public static class SaveDataExtension
{
	/// <summary>
	/// Gets a <see cref="T:SlugBase.SaveData.SlugBaseSaveData" /> from the game's <see cref="T:DeathPersistentSaveData" />.
	/// </summary>
	/// <param name="data">The <see cref="T:DeathPersistentSaveData" /> instance.</param>
	/// <returns>A <see cref="T:SlugBase.SaveData.SlugBaseSaveData" /> bound to the <see cref="T:DeathPersistentSaveData" />.</returns>
	public static SlugBaseSaveData GetSlugBaseData(this DeathPersistentSaveData data)
	{
		return SlugBaseSaveData.DeathPersistentData.GetValue(data, (DeathPersistentSaveData dpsd) => new SlugBaseSaveData(dpsd.unrecognizedSaveStrings));
	}

	/// <summary>
	/// Gets a <see cref="T:SlugBase.SaveData.SlugBaseSaveData" /> from the game's <see cref="T:MiscWorldSaveData" />.
	/// </summary>
	/// <param name="data">The <see cref="T:MiscWorldSaveData" /> instance.</param>
	/// <returns>A <see cref="T:SlugBase.SaveData.SlugBaseSaveData" /> bound to the <see cref="T:MiscWorldSaveData" /> instance.</returns>
	public static SlugBaseSaveData GetSlugBaseData(this MiscWorldSaveData data)
	{
		return SlugBaseSaveData.WorldData.GetValue(data, (MiscWorldSaveData mwsd) => new SlugBaseSaveData(mwsd.unrecognizedSaveStrings));
	}

	public static SlugBaseSaveData GetSlugBaseData(this MiscProgressionData data)
	{
		return SlugBaseSaveData.ProgressionData.GetValue(data, (MiscProgressionData mpd) => new SlugBaseSaveData(mpd.unrecognizedSaveStrings));
	}
}
