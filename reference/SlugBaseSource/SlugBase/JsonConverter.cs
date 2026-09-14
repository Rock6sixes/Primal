using System;
using System.Collections.Generic;

namespace SlugBase;

/// <summary>
/// Converts to and from JSON values.
/// </summary>
public static class JsonConverter
{
	/// <summary>Create a read-only <see cref="T:SlugBase.JsonObject" /> from a copy of <paramref name="obj" />.</summary>
	/// <remarks>If <c>null</c> values are valid, use <see cref="M:SlugBase.JsonConverter.ToJsonAny(System.Object)" /> instead.</remarks>
	/// <exception cref="T:System.ArgumentException">An object in <paramref name="obj" /> could not be converted to JSON.</exception>
	/// <exception cref="T:System.ArgumentNullException"><paramref name="obj" /> was <c>null</c>.</exception>
	public static JsonObject ToJson(Dictionary<string, object> obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		return new JsonObject(DeepClone(obj), null);
	}

	/// <summary>Create a read-only <see cref="T:SlugBase.JsonList" /> from a copy of <paramref name="list" />.</summary>
	/// <remarks>If <c>null</c> values are valid, use <see cref="M:SlugBase.JsonConverter.ToJsonAny(System.Object)" /> instead.</remarks>
	/// <exception cref="T:System.ArgumentException">An object in <paramref name="list" /> could not be converted to JSON.</exception>
	/// <exception cref="T:System.ArgumentNullException"><paramref name="list" /> was <c>null</c>.</exception>
	public static JsonList ToJson(List<object> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return new JsonList(DeepClone(list), null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.String" />.</summary>
	public static JsonAny ToJson(string value)
	{
		return new JsonAny(value, null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.Boolean" />.</summary>
	public static JsonAny ToJson(bool value)
	{
		return new JsonAny(value, null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.Double" />.</summary>
	public static JsonAny ToJson(double value)
	{
		return new JsonAny(value, null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.Single" />.</summary>
	public static JsonAny ToJson(float value)
	{
		return new JsonAny((double)value, null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.Int32" />.</summary>
	public static JsonAny ToJson(int value)
	{
		return new JsonAny((double)value, null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.UInt32" />.</summary>
	public static JsonAny ToJson(uint value)
	{
		return new JsonAny((double)value, null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.Int16" />.</summary>
	public static JsonAny ToJson(short value)
	{
		return new JsonAny((double)value, null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.UInt16" />.</summary>
	public static JsonAny ToJson(ushort value)
	{
		return new JsonAny((double)(int)value, null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.Int64" />. <paramref name="value" /> is converted to a <see cref="T:System.Double" />, so very large numbers may lose precision.</summary>
	public static JsonAny ToJson(long value)
	{
		return new JsonAny((double)value, null);
	}

	/// <summary>Create a <see cref="T:SlugBase.JsonAny" /> from a single <see cref="T:System.UInt64" />. <paramref name="value" /> is converted to a <see cref="T:System.Double" />, so very large numbers may lose precision.</summary>
	public static JsonAny ToJson(ulong value)
	{
		return new JsonAny((double)value, null);
	}

	/// <summary>Create a read-only <see cref="T:SlugBase.JsonAny" /> from a copy of <paramref name="value" />.</summary>
	/// <exception cref="T:System.ArgumentException">An object in the list or dictionary could not be converted to JSON.</exception>
	public static JsonAny ToJsonAny(object value)
	{
		return new JsonAny(DeepClone(value), null);
	}

	/// <summary>
	/// Create a mutable copy of a <see cref="T:SlugBase.JsonObject" />.
	/// </summary>
	/// <param name="value">The <see cref="T:SlugBase.JsonObject" /> to copy.</param>
	/// <returns>A copy of <paramref name="value" /> as a <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
	public static Dictionary<string, object> ToDictionary(JsonObject value)
	{
		return DeepClone(value._dict);
	}

	/// <summary>
	/// Create a mutable copy of a <see cref="T:SlugBase.JsonList" />.
	/// </summary>
	/// <param name="value">The <see cref="T:SlugBase.JsonList" /> to copy.</param>
	/// <returns>A copy of <paramref name="value" /> as a <see cref="T:System.Collections.Generic.List`1" />.</returns>
	public static List<object> ToList(JsonList value)
	{
		return DeepClone(value._list);
	}

	/// <summary>
	/// Create a mutable copy of a <see cref="T:SlugBase.JsonAny" />.
	/// </summary>
	/// <param name="value">The <see cref="T:SlugBase.JsonAny" /> to copy.</param>
	/// <returns>A copy of <paramref name="value" /> as a <see cref="T:System.Collections.Generic.List`1" />, <see cref="T:System.Collections.Generic.Dictionary`2" />, <see cref="T:System.String" />, <see cref="T:System.Boolean" />, <see cref="T:System.Double" />, or <c>null</c>.</returns>
	public static object ToObject(JsonAny value)
	{
		return DeepClone(value._object);
	}

	private static Dictionary<string, object> DeepClone(Dictionary<string, object> src)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>(src);
		foreach (KeyValuePair<string, object> item in src)
		{
			dictionary[item.Key] = DeepClone(item.Value);
		}
		return dictionary;
	}

	private static List<object> DeepClone(List<object> src)
	{
		List<object> list = new List<object>(src);
		for (int i = 0; i < src.Count; i++)
		{
			list[i] = DeepClone(src[i]);
		}
		return list;
	}

	private static object DeepClone(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("JSON values may not be null!");
		}
		if (1 == 0)
		{
		}
		object result;
		if (!(obj is string text))
		{
			if (!(obj is bool flag))
			{
				if (!(obj is double num))
				{
					if (!(obj is float num2))
					{
						if (!(obj is int num3))
						{
							if (!(obj is uint num4))
							{
								if (!(obj is short num5))
								{
									if (!(obj is ushort num6))
									{
										if (!(obj is long num7))
										{
											if (!(obj is ulong num8))
											{
												if (!(obj is Dictionary<string, object> src))
												{
													if (!(obj is List<object> src2))
													{
														if (obj != null)
														{
															throw new ArgumentException("Type could not be converted to JSON: " + obj.GetType().Name);
														}
														result = null;
													}
													else
													{
														result = DeepClone(src2);
													}
												}
												else
												{
													result = DeepClone(src);
												}
											}
											else
											{
												result = (double)num8;
											}
										}
										else
										{
											result = (double)num7;
										}
									}
									else
									{
										result = (double)(int)num6;
									}
								}
								else
								{
									result = (double)num5;
								}
							}
							else
							{
								result = (double)num4;
							}
						}
						else
						{
							result = (double)num3;
						}
					}
					else
					{
						result = (double)num2;
					}
				}
				else
				{
					result = num;
				}
			}
			else
			{
				result = flag;
			}
		}
		else
		{
			result = text;
		}
		if (1 == 0)
		{
		}
		return result;
	}
}
