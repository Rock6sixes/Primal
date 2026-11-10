using MoreSlugcats;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The mod's built-in default gimmick assignments -- what a fresh install (or a Remix
    /// "reset to defaults") gives you before the player touches anything, per the mod's design:
    /// "certain gimmicks will be applied to certain characters by default, but this will be
    /// editable... in the remix settings". These only ever take effect on the reserved save
    /// slot (PrimalSlot.ReservedSlot); every other slot plays every character 100% vanilla.
    ///
    /// Everything here is still fully overridable per character via the mod's Remix menu; these
    /// are only the starting values. Add more GimmickAssignments.SetDefault(character.value,
    /// GimmickID, true) calls below as more character/gimmick pairings are decided.
    /// </summary>
    internal static class GimmickDefaults
    {
        public static void Apply()
        {
            // Gourmand gets both currently-implemented gimmicks by default on the reserved slot:
            // Misery (traced from the "Gourmand Stamina Display" mod's own Misery Mode -- every
            // jump maxes out exhaustion and comes out weak) and Slugpup (small body, pup physics,
            // and every creature treats him as a pup).
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Gourmand.value, GimmickRegistry.Misery.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Gourmand.value, GimmickRegistry.Slugpup.ID, true);

            // Spearmaster gets OneSpear, TimeLimit, and NoHold by default on the reserved slot:
            // his usually-unlimited spear-pull becomes just one for the whole save file (OneSpear),
            // he can never pick up a spear from the ground or from anyone else (NoHold -- his own
            // entry in NoHoldSettings.PerCharacterDefaultBlocked blocks only Spears, migrated
            // 2026-09-18 from the old NoSpears gimmick this replaced; his own tail-pull still
            // works, see NoHoldGimmick.cs's doc comment for why), and the run ends in death after
            // one real-world hour of play (TimeLimit).
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Spear.value, GimmickRegistry.OneSpear.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Spear.value, GimmickRegistry.TimeLimit.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Spear.value, GimmickRegistry.NoHold.ID, true);

            // Artificer gets Chains by default on the reserved slot -- ported from a local mod
            // the user already plays this way on Artificer specifically (see ChainsGimmick.cs);
            // every room with living creatures in it locks down until it's cleared.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Artificer.value, GimmickRegistry.Chains.ID, true);

            // Also defaulted to Artificer (2026-09-12), on top of Chains: NoYeeks, NoCicadas, and
            // NoMauling -- three more restrictions in the same "Primal Artificer" vein as
            // Spearmaster's NoSpears. Assumption, not explicitly specified when these three were
            // requested: NoMauling in particular only ever does anything for a character playing
            // under Artificer's own SlugCatClass in the first place (see NoMaulingGimmick.cs), so
            // Artificer is the only default assignment that makes sense for it; NoYeeks/NoCicadas
            // could reasonably go on any character but were grouped here to match. All three are
            // fully overridable per character via the mod's Remix menu like everything else.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Artificer.value, GimmickRegistry.NoYeeks.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Artificer.value, GimmickRegistry.NoCicadas.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Artificer.value, GimmickRegistry.NoMauling.ID, true);

            // Inv ("Sofanthiel" internally -- see PrimalOptions.cs's AllCharacters comment)
            // gets the same three plus NoHold by default, per explicit request (2026-09-12; NoHold
            // migrated from NoSpears 2026-09-18, see the Spearmaster comment above). Inv's own
            // entry in NoHoldSettings.PerCharacterDefaultBlocked (added 2026-09-18, later still)
            // blocks Spears, Yeeks, and Squidcadas -- per CHARACTER_DESIGN_STATUS.md's design table.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value, GimmickRegistry.NoYeeks.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value, GimmickRegistry.NoCicadas.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value, GimmickRegistry.NoMauling.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value, GimmickRegistry.NoHold.ID, true);

            // Monk gets ThreatPanic by default (2026-09-13, explicit request: "call it threat
            // panic gimmick, add it to monk"). ThreatSlowdown is NOT defaulted onto Monk here --
            // the user assigned it to Monk themselves via the Remix menu while testing, which is
            // independent of this hardcoded default and unaffected by it either way.
            GimmickAssignments.SetDefault(SlugcatStats.Name.Yellow.value, GimmickRegistry.ThreatPanic.ID, true);

            // Saint gets Rain Timer and Blizzard Timer by default (2026-09-18, explicit request:
            // "set the rain timer to be 0 - it needs to start immediately... blizzard timer set to
            // 1 hour"). The actual 0%/60-minute values are each gimmick's own per-character setting
            // default (RainTimerSettings.PerCharacterDefaultPercent, BlizzardTimerSettings
            // .SaintDefaultMinutes), not something SetDefault itself carries -- this call only makes
            // the gimmicks assigned to Saint out of the box; see those two classes' own Bind()
            // methods for the character-specific default values.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Saint.value, GimmickRegistry.RainTimer.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Saint.value, GimmickRegistry.BlizzardTimer.ID, true);

            // Saint also gets No Hold by default (2026-09-18, later still) -- its own entry in
            // NoHoldSettings.PerCharacterDefaultBlocked blocks everything (all ten categories plus
            // Other). NoHold's own "inert until cycle N" setting (see NoHoldGimmick.cs's header
            // comment) stays at its generic default (0, active immediately) for Saint -- the user
            // introduced that setting as a new general NoHold concept, not as a specific requested
            // starting value for Saint, so no cycle count is assumed here; it's fully available to
            // set per-character via Remix.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Saint.value, GimmickRegistry.NoHold.ID, true);

            // Rivulet gets Rain Timer by default too (2026-09-18, later still, explicit request) --
            // its own entry in RainTimerSettings.PerCharacterDefaultPercent is 0%, same "starts
            // essentially immediately" treatment as Saint.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value, GimmickRegistry.RainTimer.ID, true);

            // Rivulet also gets Permanent Shelter Failure by default (2026-11-05, explicit
            // request): together with SleepEveryShelterRequirement (RequirementDefaults.cs) the
            // campaign becomes a one-way route through every shelter.
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value, GimmickRegistry.PermanentShelterFailure.ID, true);
        }
    }
}
