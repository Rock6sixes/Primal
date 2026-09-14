using System;
using System.Collections.Generic;
using System.Globalization;
using RWCustom;
using UnityEngine;

public class FFont
{
	public const int ASCII_NEWLINE = 10;

	public const int ASCII_SPACE = 32;

	public const int ASCII_HYPHEN_MINUS = 45;

	public const int ASCII_LINEHEIGHT_REFERENCE = 77;

	public string _name;

	public FAtlasElement _element;

	public string _configPath;

	public FCharInfo[] _charInfos;

	public Dictionary<uint, FCharInfo> _charInfosByID;

	public FKerningInfo[] _kerningInfos;

	public int _kerningCount;

	public FKerningInfo _nullKerning = new FKerningInfo();

	public float _lineHeight;

	public int _configWidth;

	public float _configRatio;

	public float _maxCharWidth;

	public FTextParams _textParams;

	public float _offsetX;

	public float _offsetY;

	public string name => _name;

	public FAtlasElement element => _element;

	public FTextParams textParams => _textParams;

	public float offsetX => _offsetX;

	public float offsetY => _offsetY;

	public float maxCharWidth => _maxCharWidth;

	public float lineHeight => _lineHeight;

	public FFont(string name, FAtlasElement element, string configPath, float offsetX, float offsetY, FTextParams textParams, float fontScale)
	{
		_name = name;
		_element = element;
		_configPath = configPath;
		_textParams = textParams;
		_offsetX = offsetX * Futile.displayScale / Futile.resourceScale;
		_offsetY = offsetY * Futile.displayScale / Futile.resourceScale;
		LoadAndParseConfigFile(fontScale);
	}

	public void LoadAndParseConfigFile(float fontScale)
	{
		TextAsset textAsset = (TextAsset)Resources.Load(_configPath, typeof(TextAsset));
		if (textAsset == null)
		{
			throw new FutileException("Couldn't find font config file " + _configPath);
		}
		string[] array = new string[1] { "\n" };
		string[] array2 = textAsset.text.Split(array, StringSplitOptions.RemoveEmptyEntries);
		if (array2.Length <= 1)
		{
			array[0] = "\r\n";
			array2 = textAsset.text.Split(array, StringSplitOptions.RemoveEmptyEntries);
		}
		if (array2.Length <= 1)
		{
			array[0] = "\r";
			array2 = textAsset.text.Split(array, StringSplitOptions.RemoveEmptyEntries);
		}
		if (array2.Length <= 1)
		{
			throw new FutileException("Your font file is messed up");
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		_charInfosByID = new Dictionary<uint, FCharInfo>(127);
		FCharInfo value = new FCharInfo();
		_charInfosByID[0u] = value;
		float resourceScaleInverse = Futile.resourceScaleInverse;
		Vector2 textureSize = _element.atlas.textureSize;
		bool flag = false;
		int num4 = array2.Length;
		for (int i = 0; i < num4; i++)
		{
			string[] array3 = array2[i].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array3[0] == "common")
			{
				_configWidth = int.Parse(array3[3].Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				_configRatio = _element.sourcePixelSize.x / (float)_configWidth;
				_lineHeight = (float)int.Parse(array3[1].Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture) * _configRatio * resourceScaleInverse;
			}
			else if (array3[0] == "chars")
			{
				int num5 = int.Parse(array3[1].Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				_charInfos = new FCharInfo[num5 + 1];
			}
			else if (array3[0] == "char")
			{
				FCharInfo fCharInfo = new FCharInfo();
				num = array3.Length;
				for (int j = 1; j < num; j++)
				{
					string[] array4 = array3[j].Split('=');
					string text = array4[0];
					if (text == "letter")
					{
						if (array4[1].Length >= 3)
						{
							fCharInfo.letter = array4[1].Substring(1, 1);
						}
					}
					else if (!(text == "\r"))
					{
						int num6 = int.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						float num7 = num6;
						switch (text)
						{
						case "id":
							fCharInfo.charID = num6;
							break;
						case "x":
							fCharInfo.x = num7 * _configRatio - _element.sourceRect.x * Futile.resourceScale;
							break;
						case "y":
							fCharInfo.y = num7 * _configRatio - _element.sourceRect.y * Futile.resourceScale;
							break;
						case "width":
							fCharInfo.width = num7 * _configRatio;
							break;
						case "height":
							fCharInfo.height = num7 * _configRatio;
							break;
						case "xoffset":
							fCharInfo.offsetX = num7 * _configRatio;
							break;
						case "yoffset":
							fCharInfo.offsetY = num7 * _configRatio;
							break;
						case "xadvance":
							fCharInfo.xadvance = num7 * _configRatio;
							break;
						case "page":
							fCharInfo.page = num6;
							break;
						}
					}
				}
				Rect rect = (fCharInfo.uvRect = new Rect(_element.uvRect.x + fCharInfo.x / textureSize.x, (textureSize.y - fCharInfo.y - fCharInfo.height) / textureSize.y - (1f - _element.uvRect.yMax), fCharInfo.width / textureSize.x, fCharInfo.height / textureSize.y));
				fCharInfo.uvTopLeft.Set(rect.xMin, rect.yMax);
				fCharInfo.uvTopRight.Set(rect.xMax, rect.yMax);
				fCharInfo.uvBottomRight.Set(rect.xMax, rect.yMin);
				fCharInfo.uvBottomLeft.Set(rect.xMin, rect.yMin);
				fCharInfo.width *= resourceScaleInverse * fontScale;
				fCharInfo.height *= resourceScaleInverse * fontScale;
				fCharInfo.offsetX *= resourceScaleInverse * fontScale;
				fCharInfo.offsetY *= resourceScaleInverse * fontScale;
				fCharInfo.xadvance *= resourceScaleInverse * fontScale;
				_charInfosByID[(uint)fCharInfo.charID] = fCharInfo;
				_charInfos[num2] = fCharInfo;
				num2++;
			}
			else if (array3[0] == "kernings")
			{
				flag = true;
				int num8 = int.Parse(array3[1].Split('=')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				_kerningInfos = new FKerningInfo[num8 + 100];
			}
			else
			{
				if (!(array3[0] == "kerning"))
				{
					continue;
				}
				FKerningInfo fKerningInfo = new FKerningInfo();
				fKerningInfo.first = -1;
				num = array3.Length;
				for (int k = 1; k < num; k++)
				{
					string[] array5 = array3[k].Split('=');
					if (array5.Length >= 2)
					{
						string text2 = array5[0];
						int num9 = int.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						switch (text2)
						{
						case "first":
							fKerningInfo.first = num9;
							break;
						case "second":
							fKerningInfo.second = num9;
							break;
						case "amount":
							fKerningInfo.amount = (float)num9 * _configRatio * resourceScaleInverse;
							break;
						}
					}
				}
				if (fKerningInfo.first != -1)
				{
					_kerningInfos[num3] = fKerningInfo;
				}
				num3++;
			}
		}
		_kerningCount = num3;
		if (!flag)
		{
			_kerningInfos = new FKerningInfo[0];
		}
		if (_charInfosByID.ContainsKey(32u))
		{
			_charInfosByID[32u].offsetX = 0f;
			_charInfosByID[32u].offsetY = 0f;
		}
		for (int l = 0; l < _charInfos.Length; l++)
		{
			if (_charInfos[l] != null && _charInfos[l].width > _maxCharWidth)
			{
				_maxCharWidth = _charInfos[l].width;
			}
		}
	}

	public bool IsThaiUpperVowel(char c)
	{
		if (c != '\u0e31' && c != 'ำ' && c != '\u0e34' && c != '\u0e35' && c != '\u0e36')
		{
			return c == '\u0e37';
		}
		return true;
	}

	public bool IsThaiTallStemChar(char c)
	{
		if (c != 'ป' && c != 'ฝ' && c != 'ฟ' && c != 'ฬ' && c != 'โ' && c != 'ใ')
		{
			return c == 'ไ';
		}
		return true;
	}

	public bool IsThaiToneMark(char c)
	{
		if (c >= '\u0e47')
		{
			return c <= '\u0e4e';
		}
		return false;
	}

	public bool IsThaiConsonant(char c)
	{
		if (c >= 'ก')
		{
			return c <= 'ฮ';
		}
		return false;
	}

	public FLetterQuadLine[] GetQuadInfoForText(string text, FTextParams labelTextParams)
	{
		int num = 0;
		int num2 = 0;
		bool flag = Custom.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Thai;
		float num3 = 0f;
		if (flag && _charInfosByID.ContainsKey(3588u))
		{
			num3 = _charInfosByID[3588u].width;
		}
		char[] array = text.ToCharArray();
		List<FLetterQuadLine> list = new List<FLetterQuadLine>();
		int num4 = array.Length;
		for (int i = 0; i < num4; i++)
		{
			if (array[i] == '\n')
			{
				FLetterQuadLine item = default(FLetterQuadLine);
				item.letterCount = num2;
				item.quads = new FLetterQuad[num2];
				list.Add(item);
				num++;
				num2 = 0;
			}
			else
			{
				num2++;
			}
		}
		FLetterQuadLine item2 = default(FLetterQuadLine);
		item2.letterCount = num2;
		item2.quads = new FLetterQuad[num2];
		list.Add(item2);
		FLetterQuadLine[] array2 = list.ToArray();
		FLetterQuadLine[] array3 = new FLetterQuadLine[num + 1];
		for (int j = 0; j < num + 1; j++)
		{
			array3[j] = array2[j];
		}
		num = 0;
		num2 = 0;
		float num5 = 0f;
		float num6 = 0f;
		FCharInfo fCharInfo = null;
		char c = '\0';
		float num7 = float.MaxValue;
		float num8 = float.MinValue;
		float num9 = float.MaxValue;
		float num10 = float.MinValue;
		float num11 = _lineHeight + labelTextParams.scaledLineHeightOffset + _textParams.scaledLineHeightOffset;
		for (int k = 0; k < num4; k++)
		{
			char c2 = array[k];
			char c3 = ((k < num4 - 1) ? array[k + 1] : '\0');
			char c4 = ((k >= 2) ? array[k - 2] : '\0');
			if (c2 == '\n')
			{
				if (fCharInfo != null && fCharInfo.specificLineHeight != 0f)
				{
					num11 = fCharInfo.specificLineHeight + labelTextParams.scaledLineHeightOffset + _textParams.scaledLineHeightOffset;
				}
				if (num2 == 0)
				{
					array3[num].bounds = new Rect(0f, 0f, num6, num6 - num11);
				}
				else
				{
					array3[num].bounds = new Rect(num7, num9, num8 - num7, num10 - num9);
				}
				num7 = float.MaxValue;
				num8 = float.MinValue;
				num9 = float.MaxValue;
				num10 = float.MinValue;
				num5 = 0f;
				num6 -= num11;
				num++;
				num2 = 0;
			}
			else
			{
				FKerningInfo fKerningInfo = _nullKerning;
				for (int l = 0; l < _kerningCount; l++)
				{
					FKerningInfo fKerningInfo2 = _kerningInfos[l];
					if (fKerningInfo2.first == c && fKerningInfo2.second == c2)
					{
						fKerningInfo = fKerningInfo2;
					}
				}
				FLetterQuad fLetterQuad = default(FLetterQuad);
				bool flag2 = false;
				FCharInfo fCharInfo2;
				if (_charInfosByID.ContainsKey(c2))
				{
					fCharInfo2 = _charInfosByID[c2];
					flag2 = true;
				}
				else
				{
					fCharInfo2 = _charInfosByID[0u];
				}
				float num12 = fKerningInfo.amount + labelTextParams.scaledKerningOffset + _textParams.scaledKerningOffset;
				num5 = ((num2 != 0) ? (num5 + num12) : (0f - fCharInfo2.offsetX));
				fLetterQuad.charInfo = fCharInfo2;
				float num13 = 0f;
				float num14 = 0f;
				if (flag)
				{
					bool num15 = IsThaiToneMark(c2);
					if (num15 && (IsThaiUpperVowel(c) || c3 == 'ำ'))
					{
						num14 = fCharInfo2.offsetY;
					}
					if ((num15 || IsThaiUpperVowel(c2)) && (IsThaiTallStemChar(c) || (!IsThaiConsonant(c) && IsThaiTallStemChar(c4))))
					{
						num13 = -(int)(num3 / 2f);
					}
				}
				Rect rect = (fLetterQuad.rect = new Rect(num5 + fCharInfo2.offsetX + num13, num6 - fCharInfo2.offsetY + num14 - fCharInfo2.height, fCharInfo2.width, fCharInfo2.height));
				array3[num].quads[num2] = fLetterQuad;
				num7 = Math.Min(num7, rect.xMin);
				num8 = Math.Max(num8, rect.xMax);
				num9 = Math.Min(num9, num6 - num11);
				num10 = Math.Max(num10, num6);
				num5 += fCharInfo2.xadvance;
				num2++;
				if (flag2)
				{
					fCharInfo = fCharInfo2;
				}
			}
			c = c2;
		}
		if (num2 == 0)
		{
			array3[num].bounds = new Rect(0f, 0f, num6, num6 - num11);
		}
		else
		{
			array3[num].bounds = new Rect(num7, num9, num8 - num7, num10 - num9);
		}
		return array3;
	}

	public float LineHeight()
	{
		return _lineHeight;
	}

	public void CombineWithFont(FFont addFont, float offsetY)
	{
		List<FCharInfo> list = null;
		foreach (KeyValuePair<uint, FCharInfo> item in addFont._charInfosByID)
		{
			if (!_charInfosByID.ContainsKey(item.Key))
			{
				FCharInfo value = item.Value;
				value.specificLineHeight = addFont.LineHeight();
				value.offsetY += (_lineHeight - addFont.LineHeight()) / 2f + offsetY;
				_charInfosByID.Add(item.Key, value);
				if (list == null)
				{
					list = new List<FCharInfo>();
				}
				list.Add(value);
			}
		}
		if (list != null)
		{
			FCharInfo[] array = new FCharInfo[_charInfos.Length + list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				if (i < _charInfos.Length)
				{
					array[i] = _charInfos[i];
				}
				else
				{
					array[i] = list[i - _charInfos.Length];
				}
			}
		}
		if (addFont._maxCharWidth > _maxCharWidth)
		{
			_maxCharWidth = addFont._maxCharWidth;
		}
	}
}
