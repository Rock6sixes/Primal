using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;

namespace SlugBase;

/// <summary>
/// A timeline added by SlugBase.
/// </summary>
public class CustomTimeline
{
	private static List<CustomTimeline> orderedTimelines;

	/// <summary>
	/// Stores all registered <see cref="T:SlugBase.CustomTimeline" />s.
	/// </summary>
	public static JsonRegistry<Timeline, CustomTimeline> Registry { get; } = new JsonRegistry<Timeline, CustomTimeline>((Timeline key, JsonObject json) => new CustomTimeline(key, json));


	/// <summary>
	/// All registered <see cref="T:SlugBase.CustomTimeline" />s to be added to <see cref="M:SlugcatStats.SlugcatTimelineOrder" />.
	/// </summary>
	/// <remarks>
	/// This list is topologically sorted. Each timeline may reference those ahead of it in <see cref="P:SlugBase.CustomTimeline.InsertBefore" /> or <see cref="P:SlugBase.CustomTimeline.InsertAfter" />.
	/// </remarks>
	internal static List<CustomTimeline> OrderedTimelines
	{
		get
		{
			if (orderedTimelines == null)
			{
				orderedTimelines = GetOrderedTimelines();
			}
			return orderedTimelines;
		}
	}

	/// <summary>
	/// This timeline's unique ID.
	/// </summary>
	public Timeline ID { get; }

	/// <summary>
	/// An array of timelines this timeline inherits from.
	/// </summary>
	/// <remarks>
	/// If a file specific to this timeline isn't found, these are checked in order before using the default.
	/// </remarks>
	public Timeline[] Base { get; set; }

	/// <summary>
	/// The timeline that comes after this.
	/// </summary>
	/// <remarks>
	/// When determining the order of timelines, this one will be inserted immediately before
	/// the first registered element in <see cref="P:SlugBase.CustomTimeline.InsertBefore" />.
	/// </remarks>
	/// <seealso cref="P:SlugBase.CustomTimeline.InsertAfter" />
	public Timeline[] InsertBefore { get; set; }

	/// <summary>
	/// The timeline that comes before this.
	/// </summary>
	/// <remarks>
	/// When determining the order of timelines, this one will be inserted immediately after
	/// the first registered element in <see cref="P:SlugBase.CustomTimeline.InsertBefore" />.
	/// </remarks>
	/// <seealso cref="P:SlugBase.CustomTimeline.InsertAfter" />
	public Timeline[] InsertAfter { get; set; }

	/// <summary>
	/// All valid options to use for timeline inheritance, starting with <see cref="P:SlugBase.CustomTimeline.ID" /> and continuing through <see cref="P:SlugBase.CustomTimeline.Base" />.
	/// This includes parent timelines from <see cref="T:SlugBase.CustomTimeline" />s this inherits from.
	/// </summary>
	internal List<Timeline> Priorities => AddFlattenedPriorities(new List<Timeline>());

	/// <summary>
	/// Reload the order of custom timelines text time <see cref="P:SlugBase.CustomTimeline.OrderedTimelines" /> is accessed.
	/// </summary>
	internal static void ReloadTimelineOrder()
	{
		orderedTimelines = null;
	}

	private static List<CustomTimeline> GetOrderedTimelines()
	{
		List<CustomTimeline> list = Registry.Values.Where((CustomTimeline timeline) => timeline.InsertBefore.Length != 0 || timeline.InsertAfter.Length != 0).OrderBy((CustomTimeline timeline) => ((ExtEnumBase)timeline.ID).value, StringComparer.InvariantCulture).ToList();
		if (list.Any())
		{
			Dictionary<CustomTimeline, HashSet<CustomTimeline>> dependencies = new Dictionary<CustomTimeline, HashSet<CustomTimeline>>();
			foreach (CustomTimeline item in list)
			{
				HashSet<CustomTimeline> hashSet = new HashSet<CustomTimeline>();
				foreach (Timeline item2 in item.InsertAfter.Concat(item.InsertBefore))
				{
					if (Registry.TryGet(item2, out var value))
					{
						hashSet.Add(value);
					}
				}
				dependencies[item] = hashSet;
			}
			list = Utility.TopologicalSort(list, (CustomTimeline timeline) => dependencies[timeline]).ToList();
		}
		return list;
	}

	private CustomTimeline(Timeline timeline, JsonObject json)
	{
		ID = timeline;
		JsonAny? jsonAny = json.TryGet("base");
		if (jsonAny.HasValue)
		{
			JsonAny valueOrDefault = jsonAny.GetValueOrDefault();
			if (true)
			{
				Base = GetTimelines(valueOrDefault);
				goto IL_0051;
			}
		}
		Base = (Timeline[])(object)new Timeline[0];
		goto IL_0051;
		IL_0051:
		InsertBefore = (Timeline[])(object)new Timeline[0];
		InsertAfter = (Timeline[])(object)new Timeline[0];
		jsonAny = json.TryGet("insert_before");
		if (jsonAny.HasValue)
		{
			JsonAny valueOrDefault2 = jsonAny.GetValueOrDefault();
			if (true)
			{
				if (json.TryGet("insert_after").HasValue)
				{
					throw new JsonException("Only one of \"insert_before\" or \"insert_after\" may be specified!", json.Get("insert_after"));
				}
				InsertBefore = GetTimelines(valueOrDefault2);
				return;
			}
		}
		jsonAny = json.TryGet("insert_after");
		if (jsonAny.HasValue)
		{
			JsonAny valueOrDefault3 = jsonAny.GetValueOrDefault();
			if (true)
			{
				InsertAfter = GetTimelines(valueOrDefault3);
			}
		}
	}

	private List<Timeline> AddFlattenedPriorities(List<Timeline> prios)
	{
		if (prios.Contains(ID))
		{
			return prios;
		}
		prios.Add(ID);
		Timeline[] @base = Base;
		foreach (Timeline val in @base)
		{
			if ((int)(ExtEnum<Timeline>)(object)val != -1)
			{
				if (Registry.TryGet(val, out var value))
				{
					value.AddFlattenedPriorities(prios);
				}
				else if (!prios.Contains(val))
				{
					prios.Add(val);
				}
			}
		}
		return prios;
	}

	public bool InheritsFrom(Timeline parent)
	{
		return (ExtEnum<Timeline>)(object)parent == (ExtEnum<Timeline>)(object)ID || Array.IndexOf(Base, parent) >= 0;
	}

	/// <summary>
	/// Converts a string or list of strings to a list of timelines.
	/// </summary>
	private static Timeline[] GetTimelines(JsonAny json)
	{
		JsonList? jsonList = json.TryList();
		if (jsonList.HasValue)
		{
			JsonList valueOrDefault = jsonList.GetValueOrDefault();
			if (true)
			{
				Timeline[] array = (Timeline[])(object)new Timeline[valueOrDefault.Count];
				for (int i = 0; i < valueOrDefault.Count; i++)
				{
					array[i] = GetTimeline(valueOrDefault.GetString(i));
				}
				return array;
			}
		}
		return (Timeline[])(object)new Timeline[1] { GetTimeline(json.AsString()) };
	}

	/// <summary>
	/// Converts a string to a timeline, ignoring case.
	/// </summary>
	private static Timeline GetTimeline(string name)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		return new Timeline(Utils.MatchCaseInsensitiveEnum<Timeline>(name), false);
	}
}
