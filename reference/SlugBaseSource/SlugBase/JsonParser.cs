using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SlugBase;

internal static class JsonParser
{
	private struct Token
	{
		public int Start;

		public int Length;

		public TokenType Type;
	}

	private enum TokenType
	{
		Space,
		BeginArray,
		EndArray,
		BeginObject,
		EndObject,
		NameSeparator,
		ValueSeparator,
		String,
		Number,
		Literal,
		EndOfFile
	}

	[CompilerGenerated]
	private sealed class _003CGetTokens_003Ed__9 : IEnumerable<Token>, IEnumerable, IEnumerator<Token>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Token _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string text;

		public string _003C_003E3__text;

		private int _003Ci_003E5__1;

		private Token _003Ctoken_003E5__2;

		Token IEnumerator<Token>.Current
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
		public _003CGetTokens_003Ed__9(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__1 = 0;
				goto IL_0096;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0096;
			case 2:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0096:
				while (_003Ci_003E5__1 < text.Length)
				{
					_003Ctoken_003E5__2 = NextToken(text, _003Ci_003E5__1);
					_003Ci_003E5__1 += _003Ctoken_003E5__2.Length;
					if (_003Ctoken_003E5__2.Type != 0)
					{
						_003C_003E2__current = _003Ctoken_003E5__2;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003E2__current = new Token
				{
					Start = text.Length,
					Length = 0,
					Type = TokenType.EndOfFile
				};
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Token> IEnumerable<Token>.GetEnumerator()
		{
			_003CGetTokens_003Ed__9 _003CGetTokens_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetTokens_003Ed__ = this;
			}
			else
			{
				_003CGetTokens_003Ed__ = new _003CGetTokens_003Ed__9(0);
			}
			_003CGetTokens_003Ed__.text = _003C_003E3__text;
			return _003CGetTokens_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Token>)this).GetEnumerator();
		}
	}

	private static readonly Regex _numberFormat = new Regex("-?\\d+(?:\\.\\d+)?(?:[eE][+-]?\\d+)?");

	private static readonly string[] _literals = new string[3] { "true", "false", "null" };

	public static object Parse(string text)
	{
		IEnumerator<Token> enumerator = GetTokens(text).GetEnumerator();
		enumerator.MoveNext();
		object result = Parse(text, enumerator);
		AssertType(text, enumerator.Current, TokenType.EndOfFile);
		return result;
	}

	private static object Parse(string text, IEnumerator<Token> ts)
	{
		TokenType type = ts.Current.Type;
		if (1 == 0)
		{
		}
		object result = type switch
		{
			TokenType.BeginArray => ParseArray(text, ts), 
			TokenType.BeginObject => ParseObject(text, ts), 
			TokenType.Literal => ParseLiteral(text, ts), 
			TokenType.Number => ParseNumber(text, ts), 
			TokenType.String => ParseString(text, ts), 
			_ => throw new JsonParseException(text, ts.Current.Start, "Unexpected token: " + GetTokenName(ts.Current.Type)), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	private static List<object> ParseArray(string text, IEnumerator<Token> ts)
	{
		List<object> list = new List<object>();
		do
		{
			ts.MoveNext();
			if (ts.Current.Type != TokenType.EndArray)
			{
				list.Add(Parse(text, ts));
			}
		}
		while (ts.Current.Type == TokenType.ValueSeparator);
		AssertType(text, ts.Current, TokenType.EndArray);
		ts.MoveNext();
		return list;
	}

	private static Dictionary<string, object> ParseObject(string text, IEnumerator<Token> ts)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		do
		{
			ts.MoveNext();
			if (ts.Current.Type != TokenType.EndObject)
			{
				AssertType(text, ts.Current, TokenType.String);
				string key = ParseString(text, ts);
				AssertType(text, ts.Current, TokenType.NameSeparator);
				ts.MoveNext();
				object value = Parse(text, ts);
				dictionary.Add(key, value);
			}
		}
		while (ts.Current.Type == TokenType.ValueSeparator);
		AssertType(text, ts.Current, TokenType.EndObject);
		ts.MoveNext();
		return dictionary;
	}

	private static object ParseLiteral(string text, IEnumerator<Token> ts)
	{
		string text2 = text.Substring(ts.Current.Start, ts.Current.Length);
		if (1 == 0)
		{
		}
		object obj = text2 switch
		{
			"null" => null, 
			"true" => true, 
			"false" => false, 
			_ => throw new JsonParseException(text, ts.Current.Start, "Unknown literal: " + text2), 
		};
		if (1 == 0)
		{
		}
		object result = obj;
		ts.MoveNext();
		return result;
	}

	private static double ParseNumber(string text, IEnumerator<Token> ts)
	{
		double result = double.Parse(text.Substring(ts.Current.Start, ts.Current.Length));
		ts.MoveNext();
		return result;
	}

	private static string ParseString(string text, IEnumerator<Token> ts)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = ts.Current.Start + 1;
		int i = num;
		int num2;
		for (num2 = ts.Current.Start + ts.Current.Length - 1; i < num2; i++)
		{
			if (text[i] == '\\')
			{
				stringBuilder.Append(text, num, i - num);
				i++;
				if (i >= num2)
				{
					throw new JsonParseException(text, i, "Unexpected end of string escape sequence!");
				}
				switch (text[i])
				{
				case '"':
					stringBuilder.Append('"');
					break;
				case '\\':
					stringBuilder.Append('\\');
					break;
				case 'b':
					stringBuilder.Append('\b');
					break;
				case 'f':
					stringBuilder.Append('\f');
					break;
				case 'n':
					stringBuilder.Append('\n');
					break;
				case 'r':
					stringBuilder.Append('\r');
					break;
				case 't':
					stringBuilder.Append('\t');
					break;
				case 'u':
				{
					if (i + 4 >= num2)
					{
						throw new JsonParseException(text, num2, "Unexpected end of string in unicode escape sequence!");
					}
					for (int j = 0; j < 4; j++)
					{
						char c = text[i + 1 + j];
						if ((c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
						{
							throw new JsonParseException(text, i + 1 + j, $"Unexpected character in unicode escape sequence: {c}");
						}
					}
					int num3 = int.Parse(text.Substring(i + 1, 4), NumberStyles.HexNumber);
					stringBuilder.Append((char)num3);
					i += 4;
					break;
				}
				}
				num = i + 1;
			}
			else if (text[i] <= '\u001f')
			{
				bool flag = text[i] == '\r' || text[i] == '\n';
				throw new JsonParseException(text, i, "Unexpected " + (flag ? "line break" : "control character") + " in string!");
			}
		}
		stringBuilder.Append(text, num, num2 - num);
		ts.MoveNext();
		return stringBuilder.ToString();
	}

	private static void AssertType(string text, Token token, TokenType expectedType)
	{
		if (token.Type != expectedType)
		{
			throw new JsonParseException(text, token.Start, "Expected " + GetTokenName(expectedType) + ", found " + GetTokenName(token.Type) + "!");
		}
	}

	private static string GetTokenName(TokenType token)
	{
		if (1 == 0)
		{
		}
		string result = token switch
		{
			TokenType.BeginArray => "'['", 
			TokenType.EndArray => "']'", 
			TokenType.BeginObject => "'{'", 
			TokenType.EndObject => "'}'", 
			TokenType.Literal => "literal", 
			TokenType.NameSeparator => "':'", 
			TokenType.Number => "number", 
			TokenType.Space => "whitespace", 
			TokenType.String => "string", 
			TokenType.ValueSeparator => "','", 
			TokenType.EndOfFile => "end of file", 
			_ => "unknown", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[IteratorStateMachine(typeof(_003CGetTokens_003Ed__9))]
	private static IEnumerable<Token> GetTokens(string text)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetTokens_003Ed__9(-2)
		{
			_003C_003E3__text = text
		};
	}

	private static bool IsSpace(char c)
	{
		return c == ' ' || c == '\t' || c == '\r' || c == '\n';
	}

	private static Token NextToken(string text, int i)
	{
		Token result = default(Token);
		result.Start = i;
		result.Length = 1;
		char c = text[i];
		switch (c)
		{
		case '[':
			result.Type = TokenType.BeginArray;
			break;
		case ']':
			result.Type = TokenType.EndArray;
			break;
		case '{':
			result.Type = TokenType.BeginObject;
			break;
		case '}':
			result.Type = TokenType.EndObject;
			break;
		case ':':
			result.Type = TokenType.NameSeparator;
			break;
		case ',':
			result.Type = TokenType.ValueSeparator;
			break;
		default:
		{
			if (IsSpace(c))
			{
				result.Type = TokenType.Space;
				while (i < text.Length && IsSpace(text[i]))
				{
					i++;
				}
				result.Length = i - result.Start;
				break;
			}
			if (char.IsDigit(c) || c == '-')
			{
				result.Type = TokenType.Number;
				result.Length = _numberFormat.Match(text, i).Length;
				break;
			}
			if (c == '"')
			{
				result.Type = TokenType.String;
				i++;
				while (i < text.Length && text[i] != '"')
				{
					i = ((text[i] != '\\') ? (i + 1) : (i + 2));
				}
				i++;
				if (i > text.Length)
				{
					throw new JsonParseException(text, text.Length, "Unterminated string!");
				}
				result.Length = i - result.Start;
				break;
			}
			bool flag = false;
			string[] literals = _literals;
			foreach (string text2 in literals)
			{
				if (text.Length - i >= text2.Length && text.Substring(i, text2.Length).Equals(text2, StringComparison.Ordinal))
				{
					result.Type = TokenType.Literal;
					result.Length = text2.Length;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			throw new JsonParseException(text, i, $"Unexpected character: {c}");
		}
		}
		return result;
	}
}
