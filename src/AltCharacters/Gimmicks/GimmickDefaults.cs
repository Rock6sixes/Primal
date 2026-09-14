using MoreSlugcats;

namespace AltCharacters.Gimmicks
{
    /// <summary>
    /// The mod's built-in default gimmick assignments -- what a fresh install (or a Remix
    /// "reset to defaults") gives you before the player touches anything, per the mod's design:
    /// "certain gimmicks will be applied to certain characters by default, but this will be
    /// editable... in the remix settings". These only ever take effect on the reserved save
    /// slot (AltSlot.ReservedSlot); every other slot plays every character 100% vanilla.
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

            // Spearmaster gets OneSpear, TimeLimit, and NoSpears by default on the reserved slot:
            // his usually-unlimited spear-pull becomes just one for the whole save file (OneSpear),
            // he can never pick up a spear from the ground or from anyone else (NoSpears -- his one
            // tail-pull still works, see that gimmick's own doc comment for why), and the run ends
            // in death after one real-world hour of play (TimeLimit).
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Spear.value, GimmickRegistry.OneSpear.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Spear.value, GimmickRegistry.TimeLimit.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Spear.value, GimmickRegistry.NoSpears.ID, true);

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

            // Inv ("Sofanthiel" internally -- see AltCharactersOptions.cs's AllCharacters comment)
            // gets the same three plus NoSpears by default, per explicit request (2026-09-12).
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value, GimmickRegistry.NoYeeks.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value, GimmickRegistry.NoCicadas.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value, GimmickRegistry.NoMauling.ID, true);
            GimmickAssignments.SetDefault(MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value, GimmickRegistry.NoSpears.ID, true);
        }
    }
}
