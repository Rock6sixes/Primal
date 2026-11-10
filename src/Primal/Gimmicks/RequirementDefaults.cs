using MoreSlugcats;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The Requirements-side counterpart to GimmickDefaults.cs, added 2026-09-18 (later still,
    /// explicit request covering several requirement default-assignments in one message). Same
    /// contract, same reasoning: GimmickAssignments.SetDefault is genuinely generic over
    /// (characterID, ID) regardless of whether ID names a gimmick or a requirement (see
    /// Requirement.cs's own header comment -- "a requirement ID and a gimmick ID are both just
    /// strings to it"), so this file just calls the exact same method with requirement IDs instead.
    /// Split into its own file rather than folded into GimmickDefaults.cs so each file stays a
    /// single, easy-to-scan list for its own half of the (character, ID) picture.
    ///
    /// Everything here is still fully overridable per character via the mod's Remix menu, same as
    /// every gimmick default. Only ever takes effect on the reserved save slot
    /// (PrimalSlot.ReservedSlot) -- every other slot plays every character 100% vanilla, same as
    /// GimmickDefaults.
    ///
    /// **Deliberately NOT included here (flagging rather than silently assuming):** the original
    /// design table (CHARACTER_DESIGN_STATUS.md) also implies SpearmasterFastEndingRequirement ->
    /// Spearmaster and UniqueKillsRequirement -> Artificer as default assignments, but neither was
    /// explicitly re-requested in the message that asked for everything else in this file, so
    /// neither is defaulted here -- both remain fully assignable by hand via Remix in the meantime.
    /// </summary>
    internal static class RequirementDefaults
    {
        public static void Apply()
        {
            // ReachEndingRequirement ("Ending" -- see Requirements.cs's own doc comment for the
            // rename and the "Either" option added the same round): Monk and Hunter by default,
            // each with the select itself defaulting to "Ascension" (ReachEndingSettings' own
            // PerCharacterDefaultEnding table -- not something SetDefault carries, see
            // GimmickDefaults.cs's own Saint/RainTimer comment for the same split between
            // "gimmick/requirement assigned at all" and "this setting's own starting value").
            GimmickAssignments.SetDefault(SlugcatStats.Name.Yellow.value, ReachEndingRequirement_ID, true); // Monk
            GimmickAssignments.SetDefault(SlugcatStats.Name.Red.value, ReachEndingRequirement_ID, true); // Hunter

            // Also assigned to Gourmand, Spearmaster, Rivulet, and Saint -- "the rest" of the
            // design table's own "Story ending"/"Ascend" rows the user's own message referred to.
            // ReachEndingSettings' select defaults to "Story" (its own generic fallback) for every
            // one of these four, since none of them has a PerCharacterDefaultEnding override.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Gourmand.value, ReachEndingRequirement_ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Spear.value, ReachEndingRequirement_ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value, ReachEndingRequirement_ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Saint.value, ReachEndingRequirement_ID, true);

            // OutlawRequirement ("100k points" -- OutlawSettings' own default score threshold is
            // already 100000, see Requirements.cs) defaults onto Hunter, the only character it's
            // thematically tied to (checked via SlugcatStats.Name.Red inside CheckScoreScreen
            // itself regardless of assignment, but assigning it here means it's on out of the box).
            GimmickAssignments.SetDefault(SlugcatStats.Name.Red.value, OutlawRequirement.RequirementID, true);

            // GourmandFoodQuestRequirement ("Food Quest," renamed the same round from "Food Quest &
            // Save") defaults onto Gourmand, its own HomeCharacter.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Gourmand.value, GourmandFoodQuestRequirement_ID, true);

            // InvHighestPointRequirement ("King," renamed the same round from "Highest Point")
            // defaults onto Inv (Sofanthiel), its own HomeCharacter -- all four of its new
            // condition checkboxes (InvHighestPointSettings) stay at their own default of checked,
            // reproducing the original "all four required" design exactly.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value, InvHighestPointRequirement.RequirementID, true);

            // Rivulet's shelter requirement (2026-11-05, explicit request): must SLEEP in every
            // shelter, not just visit it -- SleepEveryShelterRequirement replaces
            // VisitShelterRequirement as Rivulet's default. Visit Shelter stays registered and
            // assignable by hand in Remix; it's just no longer on by default for anyone.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value, SleepEveryShelterRequirement.RequirementID, true);

            // Survivor (2026-11-10, explicit request): complete N of the 50 community challenges
            // (default 45) is Survivor's only requirement, and Survivor has no default gimmicks.
            GimmickAssignments.SetDefault(SlugcatStats.Name.White.value, CommunityChallengeCountRequirement.RequirementID, true);
        }

        // Plain string literals mirroring each requirement's own ID property -- there's no shared
        // "RequirementRegistry.SomeRequirement.ID" static surface to reference instead (unlike
        // OutlawRequirement.RequirementID/InvHighestPointRequirement.RequirementID above, which
        // already exist as public consts for their own static-method reasons -- see each one's own
        // doc comment in Requirements.cs), so these two are named here explicitly to avoid a bare
        // magic string at each call site above.
        private const string ReachEndingRequirement_ID = "ReachEnding";
        private const string GourmandFoodQuestRequirement_ID = "GourmandFoodQuest";
    }
}
