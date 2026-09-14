using Menu;

namespace SlugBase.Features;

/// <summary>
/// Built-in <see cref="T:SlugBase.Features.Feature" />s describing the game.
/// </summary>
public static class GameFeatures
{
	/// <summary>"karma": Initial karma.</summary>
	public static readonly GameFeature<int> Karma = FeatureTypes.GameInt("karma");

	/// <summary>"karma_cap": Initial karma cap.</summary>
	public static readonly GameFeature<int> KarmaCap = FeatureTypes.GameInt("karma_cap");

	/// <summary>"the_mark": Start with mark of communication.</summary>
	public static readonly GameFeature<bool> TheMark = FeatureTypes.GameBool("the_mark");

	/// <summary>"the_glow": Start glowing.</summary>
	public static readonly GameFeature<bool> TheGlow = FeatureTypes.GameBool("the_glow");

	/// <summary>"start_room": Initial room, plus backups from highest to lowest priority.</summary>
	public static readonly GameFeature<string[]> StartRoom = FeatureTypes.GameStrings("start_room", 1);

	/// <summary>"guide_overseer": Player guide overseer color index.</summary>
	public static readonly GameFeature<int> GuideOverseer = FeatureTypes.GameInt("guide_overseer");

	/// <summary>"has_dreams": Whether or not to track dream state.</summary>
	public static readonly GameFeature<bool> HasDreams = FeatureTypes.GameBool("has_dreams");

	/// <summary>"use_default_dreams": Whether or not to show Survivor's dreams.</summary>
	public static readonly GameFeature<bool> UseDefaultDreams = FeatureTypes.GameBool("use_default_dreams");

	/// <summary>"cycle_length_min": Minimum cycle length in minutes.</summary>
	public static readonly GameFeature<float> CycleLengthMin = FeatureTypes.GameFloat("cycle_length_min");

	/// <summary>"cycle_length_max": Maximum cycle length in minutes.</summary>
	public static readonly GameFeature<float> CycleLengthMax = FeatureTypes.GameFloat("cycle_length_max");

	/// <summary>"perma_unlock_gates": Permanently unlock gates once used.</summary>
	public static readonly GameFeature<bool> PermaUnlockGates = FeatureTypes.GameBool("perma_unlock_gates");

	/// <summary>"food_min": Food needed to sleep.</summary>
	public static readonly GameFeature<int> FoodMin = FeatureTypes.GameInt("food_min");

	/// <summary>"food_max": Maximum food stored during a cycle.</summary>
	public static readonly GameFeature<int> FoodMax = FeatureTypes.GameInt("food_max");

	/// <summary>"select_menu_scene": The scene for this slugcat on the select menu.</summary>
	public static readonly GameFeature<SceneID> SelectMenuScene = FeatureTypes.GameExtEnum<SceneID>("select_menu_scene");

	/// <summary>"select_menu_scene_ascended": The scene for this slugcat on the select menu when ascended.</summary>
	public static readonly GameFeature<SceneID> SelectMenuSceneAscended = FeatureTypes.GameExtEnum<SceneID>("select_menu_scene_ascended");

	/// <summary>"sleep_scene": The scene for this slugcat when hibernating.</summary>
	public static readonly GameFeature<SceneID> SleepScene = FeatureTypes.GameExtEnum<SceneID>("sleep_scene");

	/// <summary>"starve_scene": The scene for this slugcat when losing from starvation.</summary>
	public static readonly GameFeature<SceneID> StarveScene = FeatureTypes.GameExtEnum<SceneID>("starve_scene");

	/// <summary>"death_scene": The scene for this slugcat when losing from a non-starvation death.</summary>
	public static readonly GameFeature<SceneID> DeathScene = FeatureTypes.GameExtEnum<SceneID>("death_scene");

	/// <summary>"intro_scene": Add an intro scene in the style of Survivor or Gourmand.</summary>
	public static readonly GameFeature<SlideShowID> IntroScene = FeatureTypes.GameExtEnum<SlideShowID>("intro_slideshow");

	/// <summary>"outro_scene": Add an outro scene for a slugbase slugcat. </summary>
	public static readonly GameFeature<SlideShowID> OutroScene = FeatureTypes.GameExtEnum<SlideShowID>("outro_slideshow");

	/// <summary>"world_state": The timeline to use for creature spawns and room connections.</summary>
	public static readonly GameFeature<Timeline[]> WorldState = FeatureTypes.GameExtEnums<Timeline>("world_state", 0, int.MaxValue);

	/// <summary>"story_regions": The new or removed story regions from an inherited world_state.</summary>
	public static readonly GameFeature<string[]> StoryRegions = FeatureTypes.GameStrings("story_regions");

	/// <summary>"timeline_before": The next timeline after this character's.</summary>
	public static readonly GameFeature<Name[]> TimelineBefore = FeatureTypes.GameSlugcatNames("timeline_before", 1);

	/// <summary>"timeline_after": The previous timeline before this character's.</summary>
	public static readonly GameFeature<Name[]> TimelineAfter = FeatureTypes.GameSlugcatNames("timeline_after", 1);

	/// <summary>"title_card": Add a intro titlecard to be randomly selected. Must be 1366x768 pixels big to display correctly.</summary>
	public static readonly GameFeature<string> TitleCard = FeatureTypes.GameString("title_card");

	/// <summary>"expedition_enabled": Enable Expedition mode for this character.</summary>
	public static readonly GameFeature<bool> ExpeditionEnabled = FeatureTypes.GameBool("expedition_enabled");

	/// <summary>"expedition_description": Character description in Expedition mode.</summary>
	public static readonly GameFeature<string> ExpeditionDescription = FeatureTypes.GameString("expedition_description");
}
