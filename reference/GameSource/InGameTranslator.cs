using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Kittehface.Build;
using Kittehface.Framework20;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

public class InGameTranslator
{
	public class LanguageID : ExtEnum<LanguageID>
	{
		public static LanguageID English = new LanguageID("English", register: true);

		public static LanguageID French = new LanguageID("French", register: true);

		public static LanguageID Italian = new LanguageID("Italian", register: true);

		public static LanguageID German = new LanguageID("German", register: true);

		public static LanguageID Spanish = new LanguageID("Spanish", register: true);

		public static LanguageID Portuguese = new LanguageID("Portuguese", register: true);

		public static LanguageID Japanese = new LanguageID("Japanese", register: true);

		public static LanguageID Korean = new LanguageID("Korean", register: true);

		public static LanguageID Russian = new LanguageID("Russian", register: true);

		public static LanguageID Chinese = new LanguageID("Chinese", register: true);

		public static LanguageID TraditionalChinese = new LanguageID("TraditionalChinese", register: true);

		public static LanguageID Thai = new LanguageID("Thai", register: true);

		public LanguageID(string value, bool register = false)
			: base(value, register)
		{
		}

		public static LanguageID Parse(int index)
		{
			if (index < 0 || index >= ExtEnum<LanguageID>.values.entries.Count)
			{
				return null;
			}
			return new LanguageID(ExtEnum<LanguageID>.values.entries[index]);
		}

		public static int EncryptIndex(LanguageID lang)
		{
			if (lang == English)
			{
				return 0;
			}
			if (lang == French)
			{
				return 1;
			}
			if (lang == Italian)
			{
				return 2;
			}
			if (lang == German)
			{
				return 3;
			}
			if (lang == Spanish)
			{
				return 4;
			}
			if (lang == Portuguese)
			{
				return 5;
			}
			if (lang == Japanese)
			{
				return 6;
			}
			if (lang == Korean)
			{
				return 7;
			}
			if (lang == Russian)
			{
				return 8;
			}
			if (lang == Chinese)
			{
				return 9;
			}
			if (lang == TraditionalChinese)
			{
				return 10;
			}
			if (lang == Thai)
			{
				return 11;
			}
			return 0;
		}

		public static bool UsesCapitals(LanguageID lang)
		{
			if (lang != Japanese && lang != Korean && lang != Chinese && lang != TraditionalChinese)
			{
				return lang != Thai;
			}
			return false;
		}

		public static bool UsesLargeFont(LanguageID lang)
		{
			if (!(lang == Japanese) && !(lang == Korean) && !(lang == Chinese) && !(lang == TraditionalChinese))
			{
				return lang == Thai;
			}
			return true;
		}

		public static bool UsesSpaces(LanguageID lang)
		{
			if (lang != Japanese && lang != Chinese && lang != TraditionalChinese)
			{
				return lang != Thai;
			}
			return false;
		}

		public static bool WordWrappingAllowed(LanguageID lang)
		{
			return lang != Japanese;
		}
	}

	public RainWorld rainWorld;

	public LanguageID lastLanguage;

	public bool loadedAOC;

	public Dictionary<string, string> shortStrings;

	public LanguageID currentLanguage => rainWorld.options.language;

	public static LanguageID systemLanguage
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Invalid comparison between Unknown and I4
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Invalid comparison between Unknown and I4
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Invalid comparison between Unknown and I4
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Expected I4, but got Unknown
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected I4, but got Unknown
			Language val = Platform.systemLanguage;
			if ((int)val <= 25)
			{
				if ((int)val != 3)
				{
					if ((int)val != 6)
					{
						switch (val - 11)
						{
						case 3:
							break;
						case 10:
							return LanguageID.Italian;
						case 4:
							return LanguageID.German;
						case 11:
							return LanguageID.Japanese;
						case 12:
							return LanguageID.Korean;
						case 0:
						case 13:
						case 14:
							goto IL_00d9;
						default:
							goto IL_00df;
						}
						goto IL_00a9;
					}
					goto IL_00d3;
				}
			}
			else
			{
				if ((int)val > 30)
				{
					if ((int)val != 34)
					{
						switch (val - 38)
						{
						case 7:
							break;
						case 8:
							goto IL_00bb;
						case 9:
							goto IL_00c1;
						case 2:
						case 3:
							goto IL_00d3;
						case 0:
							goto IL_00d9;
						default:
							goto IL_00df;
						}
						goto IL_00a9;
					}
					goto IL_00bb;
				}
				if ((int)val == 28)
				{
					goto IL_00c1;
				}
				if ((int)val != 30)
				{
					goto IL_00df;
				}
			}
			goto IL_00d9;
			IL_00df:
			return LanguageID.English;
			IL_00a9:
			return LanguageID.French;
			IL_00d9:
			return LanguageID.Russian;
			IL_00bb:
			return LanguageID.Spanish;
			IL_00d3:
			return LanguageID.Chinese;
			IL_00c1:
			return LanguageID.Portuguese;
		}
	}

	public InGameTranslator(RainWorld rainWorld)
	{
		this.rainWorld = rainWorld;
		shortStrings = new Dictionary<string, string>();
	}

	public static void LoadFonts(LanguageID lang, global::Menu.Menu menu)
	{
		string text = LocalizationTranslator.LangShort(lang);
		if (lang == LanguageID.TraditionalChinese)
		{
			text = LocalizationTranslator.LangShort(LanguageID.Chinese);
		}
		if (ModManager.NonPrepackagedModsInstalled && ModManager.InitializationScreenFinished && (lang == LanguageID.Japanese || lang == LanguageID.Korean || lang == LanguageID.Chinese || lang == LanguageID.TraditionalChinese))
		{
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text + "Full"))
			{
				Futile.atlasManager.LoadAtlasFromAddress("Atlases/fontAtlas" + text + "Full", "Assets/FullFontAtlases/fontAtlas" + text + "Full.png");
			}
			if (!Futile.atlasManager.DoesContainFontWithName("font" + text + "Full"))
			{
				Futile.atlasManager.LoadFont("font" + text + "Full", "font" + text + "Full", "Atlases/font" + text + "Full", 0f, 4f, new FTextParams(), 0.5f);
				Futile.atlasManager.CombineFonts("font" + text + "Full", "fontSolo", "fontSolo", "Atlases/font", 4f, new FTextParams(), 1f);
				Futile.atlasManager.CombineFonts("font" + text + "Full", "ps4GlyphsAtlas", "ps4GlyphsAtlas", "Atlases/ps4Glyphs", 0f, new FTextParams(), 0.5f);
			}
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/displayFontAtlas" + text + "Full"))
			{
				Futile.atlasManager.LoadAtlasFromAddress("Atlases/displayFontAtlas" + text + "Full", "Assets/FullFontAtlases/displayFontAtlas" + text + "Full.png");
			}
			if (!Futile.atlasManager.DoesContainFontWithName("DisplayFont" + text + "Full"))
			{
				Futile.atlasManager.LoadFont("DisplayFont" + text + "Full", "DisplayFont" + text + "Full", "Atlases/DisplayFont" + text + "Full", 0f, 0f, new FTextParams(), 0.5f);
				Futile.atlasManager.CombineFonts("DisplayFont" + text + "Full", "DisplayFontSolo", "DisplayFontSolo", "Atlases/DisplayFont", 0f, new FTextParams(), 1f);
			}
		}
		else if (lang == LanguageID.Japanese || lang == LanguageID.Korean || lang == LanguageID.Chinese || lang == LanguageID.TraditionalChinese)
		{
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text))
			{
				Futile.atlasManager.LoadAtlas("Atlases/fontAtlas" + text);
			}
			if (!Futile.atlasManager.DoesContainFontWithName("font" + text))
			{
				Futile.atlasManager.LoadFont("font" + text, "font" + text, "Atlases/font" + text, 0f, -1f, new FTextParams(), 0.5f);
				Futile.atlasManager.CombineFonts("font" + text, "fontSolo", "fontSolo", "Atlases/font", -1f, new FTextParams(), 1f);
				Futile.atlasManager.CombineFonts("font" + text, "ps4GlyphsAtlas", "ps4GlyphsAtlas", "Atlases/ps4Glyphs", 0f, new FTextParams(), 0.5f);
			}
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/displayFontAtlas" + text))
			{
				Futile.atlasManager.LoadAtlas("Atlases/displayFontAtlas" + text);
			}
			if (!Futile.atlasManager.DoesContainFontWithName("DisplayFont" + text))
			{
				Futile.atlasManager.LoadFont("DisplayFont" + text, "DisplayFont" + text, "Atlases/DisplayFont" + text, 0f, -4f, new FTextParams(), 0.5f);
				Futile.atlasManager.CombineFonts("DisplayFont" + text, "DisplayFontSolo", "DisplayFontSolo", "Atlases/DisplayFont", -4f, new FTextParams(), 1f);
			}
		}
		else
		{
			string text2 = ((lang == LanguageID.Russian || lang == LanguageID.Thai) ? text : "");
			if (!Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text2))
			{
				Futile.atlasManager.LoadAtlas("Atlases/fontAtlas" + text2);
			}
			if (lang == LanguageID.Russian || lang == LanguageID.Thai)
			{
				Futile.atlasManager.LoadAtlas("Atlases/displayFontAtlas" + text2);
			}
			if (!Futile.atlasManager.DoesContainFontWithName("font" + text2))
			{
				Futile.atlasManager.LoadFont("font" + text2, "font" + text2, "Atlases/font" + text2, 0f, 0f);
				Futile.atlasManager.CombineFonts("font" + text2, (text2 == "") ? "ps4Glyphs" : "ps4GlyphsAtlas", (text2 == "") ? "ps4Glyphs" : "ps4GlyphsAtlas", "Atlases/ps4Glyphs", 0f, new FTextParams(), 0.5f);
			}
			if (!Futile.atlasManager.DoesContainFontWithName("DisplayFont" + text2))
			{
				Futile.atlasManager.LoadFont("DisplayFont" + text2, "DisplayFont" + text2, "Atlases/DisplayFont" + text2, 0f, 0f);
			}
		}
		if (menu != null)
		{
			LabelTest.Initialize(menu);
		}
	}

	public static void UnloadFonts(LanguageID lang)
	{
		string text = ((lang == LanguageID.Japanese || lang == LanguageID.Korean || lang == LanguageID.Chinese || lang == LanguageID.Russian || lang == LanguageID.Thai) ? LocalizationTranslator.LangShort(lang) : "");
		if (lang == LanguageID.TraditionalChinese)
		{
			text = LocalizationTranslator.LangShort(LanguageID.Chinese);
		}
		Futile.atlasManager.UnloadFont("font" + text);
		Futile.atlasManager.UnloadFont("DisplayFont" + text);
		if (Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text))
		{
			Futile.atlasManager.UnloadAtlas("Atlases/fontAtlas" + text);
		}
		if (Futile.atlasManager.DoesContainAtlas("Atlases/displayFontAtlas" + text))
		{
			Futile.atlasManager.UnloadAtlas("Atlases/displayFontAtlas" + text);
		}
		if (ModManager.NonPrepackagedModsInstalled && ModManager.InitializationScreenFinished && (lang == LanguageID.Japanese || lang == LanguageID.Korean || lang == LanguageID.Chinese || lang == LanguageID.TraditionalChinese))
		{
			text += "Full";
			Futile.atlasManager.UnloadFont("font" + text);
			Futile.atlasManager.UnloadFont("DisplayFont" + text);
			if (Futile.atlasManager.DoesContainAtlas("Atlases/fontAtlas" + text))
			{
				Futile.atlasManager.UnloadAtlas("Atlases/fontAtlas" + text);
			}
			if (Futile.atlasManager.DoesContainAtlas("Atlases/displayFontAtlas" + text))
			{
				Futile.atlasManager.UnloadAtlas("Atlases/displayFontAtlas" + text);
			}
		}
	}

	public string Translate(string s)
	{
		if (lastLanguage == null || lastLanguage != currentLanguage)
		{
			shortStrings.Clear();
		}
		lastLanguage = currentLanguage;
		if (!loadedAOC && ModManager.ActiveMods.Count > 0)
		{
			shortStrings.Clear();
			LoadShortStrings();
			loadedAOC = true;
		}
		if (shortStrings.Count == 0)
		{
			LoadShortStrings();
		}
		if (shortStrings.ContainsKey(s))
		{
			return shortStrings[s];
		}
		if (rainWorld.buildType == RainWorld.BuildType.Distribution)
		{
			return s;
		}
		if (!Utilities.isDebugBuild)
		{
			return s;
		}
		Custom.LogWarning("UNABLE TO TRANSLATE:", s, "(", s.Length.ToString(), "chars)");
		if (s.Length > 0 && s[0] == '!')
		{
			return "DOUBLE TRANSLATION";
		}
		return "!NO TRANSLATION!";
	}

	public bool HasShortstringTranslation(string s)
	{
		if (lastLanguage == null || lastLanguage != currentLanguage)
		{
			shortStrings.Clear();
		}
		lastLanguage = currentLanguage;
		if (shortStrings.Count == 0)
		{
			LoadShortStrings();
		}
		if (shortStrings.ContainsKey(s))
		{
			return true;
		}
		return false;
	}

	public bool TryTranslate(string text, out string res)
	{
		res = Translate(text);
		if (string.IsNullOrEmpty(res) || res == "!NO TRANSLATION!")
		{
			return false;
		}
		return res != text;
	}

	public static string EvenSplit(string s, int splits)
	{
		if (splits < 1)
		{
			return s;
		}
		float num = (float)s.Length / (float)(1 + splits);
		List<int> list = new List<int>();
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == ' ')
			{
				list.Add(i);
			}
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < splits; j++)
		{
			float num2 = num * (float)(1 + j);
			float num3 = float.MaxValue;
			int num4 = -1;
			for (int k = 0; k < list.Count; k++)
			{
				float num5 = Mathf.Abs(num2 - (float)list[k]);
				if (num5 < num3)
				{
					num3 = num5;
					num4 = list[k];
				}
			}
			if (num4 > -1 && !list2.Contains(num4))
			{
				list2.Add(num4);
			}
		}
		if (list2.Count < 1)
		{
			return s;
		}
		int num6 = 0;
		foreach (int item in list2)
		{
			int length = s.Length;
			int num7 = item + num6;
			s = s.Substring(0, num7) + "\r\n" + s.Substring(num7 + 1, s.Length - (num7 + 1));
			num6 += s.Length - length;
		}
		return s;
	}

	public void LoadShortStrings()
	{
		string[] array = ShortStringsFilePaths(LanguageID.English);
		if (!File.Exists(array[0]))
		{
			return;
		}
		LoadShortStrings(array);
		if (!(currentLanguage == LanguageID.English))
		{
			array = ShortStringsFilePaths(currentLanguage);
			if (File.Exists(array[0]))
			{
				LoadShortStrings(array);
			}
		}
		void LoadShortStrings(string[] paths)
		{
			for (int i = 0; i < paths.Length; i++)
			{
				string text = File.ReadAllText(paths[i], Encoding.UTF8);
				if (text[0] == '1')
				{
					text = Custom.xorEncrypt(text, 12467);
				}
				else if (text[0] == '0')
				{
					text = text.Remove(0, 1);
				}
				string[] array2 = Regex.Split(text, "\r\n");
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j].Contains("///"))
					{
						array2[j] = array2[j].Split('/')[0].TrimEnd();
					}
					string[] array3 = array2[j].Split('|');
					if (array3.Length >= 2 && !string.IsNullOrEmpty(array3[1]))
					{
						shortStrings[array3[0]] = array3[1];
					}
				}
			}
		}
		static string[] ShortStringsFilePaths(LanguageID id)
		{
			List<string> list = new List<string> { Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(id) + Path.DirectorySeparatorChar + "strings.txt").ToLowerInvariant() };
			for (int k = 0; k < ModManager.ActiveMods.Count; k++)
			{
				string text2 = (ModManager.ActiveMods[k].path + Path.DirectorySeparatorChar + "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(id) + Path.DirectorySeparatorChar + "strings.txt").ToLowerInvariant();
				if (File.Exists(text2))
				{
					list.Add(text2);
				}
			}
			return list.ToArray();
		}
	}

	public string SpecificTextFolderDirectory()
	{
		return "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(currentLanguage);
	}

	public string SpecificTextFolderDirectory(LanguageID targetLanguage)
	{
		return "Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(targetLanguage);
	}

	public static string EncryptDecryptFile(string path, bool encryptMode, bool returnOnly = false)
	{
		if (path.ToLowerInvariant().Contains("strings.txt"))
		{
			return File.ReadAllText(path, Encoding.UTF8);
		}
		string text = File.ReadAllText(path, Encoding.UTF8);
		if (text[0] == (encryptMode ? 48 : 49))
		{
			string text2 = Path.GetDirectoryName(path).ToLowerInvariant();
			LanguageID languageID = null;
			if (text2.Contains("text_"))
			{
				string value = text2.Substring(text2.LastIndexOf("text_") + 5, 3);
				for (int i = 0; i < ExtEnum<LanguageID>.values.Count; i++)
				{
					if (ExtEnum<LanguageID>.values.entries[i].ToLowerInvariant().StartsWith(value))
					{
						languageID = new LanguageID(ExtEnum<LanguageID>.values.entries[i]);
						break;
					}
				}
			}
			string text3 = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
			string s = text3;
			if (text3.Contains("-"))
			{
				s = text3.Substring(0, text3.IndexOf("-"));
			}
			if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				string text4 = null;
				if (path.ToLowerInvariant().Contains("strings.txt"))
				{
					text4 = Custom.xorEncrypt(text, 12467 - result);
				}
				else if (languageID != null)
				{
					text4 = Custom.xorEncrypt(text, 54 + result + LanguageID.EncryptIndex(languageID) * 7);
				}
				if (text4 != null)
				{
					text4 = (encryptMode ? '1' : '0') + text4.Remove(0, 1);
					if (!returnOnly)
					{
						File.WriteAllText(path, text4, Encoding.UTF8);
					}
					return text4;
				}
			}
			else if (languageID != null)
			{
				int num = 0;
				for (int j = 0; j < text3.Length; j++)
				{
					num += text3[j] - 48;
				}
				string text5 = null;
				text5 = string.Concat(str1: ((!path.ToLowerInvariant().Contains("strings.txt")) ? Custom.xorEncrypt(text, 54 + num + LanguageID.EncryptIndex(languageID) * 7) : Custom.xorEncrypt(text, 12467)).Remove(0, 1), str0: (encryptMode ? '1' : '0').ToString());
				if (!returnOnly)
				{
					File.WriteAllText(path, text5, Encoding.UTF8);
				}
				return text5;
			}
		}
		return null;
	}
}
