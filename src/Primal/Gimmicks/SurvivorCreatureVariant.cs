using System.Reflection;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared mechanism #4 from COMMUNITY_CHALLENGES.md: creature color/variant identification --
    /// needed by Feast, Safari, Sleepover, Offering, Chef, and Pilgrimage.
    ///
    /// **Turns out most of this mechanism doesn't need to exist.** Confirmed via decompilation:
    /// Lizard colors and Centipede's red variant are each their OWN CreatureTemplate.Type value --
    /// CreatureTemplate.Type.RedLizard, .CyanLizard, .PinkLizard, etc. (CreatureTemplate.cs) and
    /// .RedCentipede (also its own Type, distinct from plain .Centipede) -- exactly the same enum
    /// type this mod's Kill List gimmick already compares directly. So Feast's Red Centipede,
    /// Sleepover's Red Lizard, Pilgrimage's Cyan Lizard, and two of Safari's three creatures need
    /// NO new mechanism at all: `creature.creatureTemplate.type == CreatureTemplate.Type.RedLizard`
    /// (etc.) is already sufficient, the same one-line check every other type-based gimmick in this
    /// mod already uses. This revises those five challenges' original "4 -- creature variant
    /// research needed" ratings down; see COMMUNITY_CHALLENGES.md's own updated notes.
    ///
    /// What's left, and what this file actually provides, is the minority of species where the
    /// variant genuinely is a runtime field rather than a distinct Type: BigEel (Leviathan) and
    /// JetFish both have a real, confirmed `public bool albino` field (BigEel.cs/JetFish.cs), set
    /// once at spawn from `world.region.regionParams.albinos`. Read through AccessTools rather than
    /// directly, per Gotcha #1 in AI_CONTEXT.md -- same reasoning as PrimalMenuAccess: a field
    /// showing public in the decompiled/publicized reference proves nothing about the real runtime
    /// assembly's accessibility.
    ///
    /// **Deliberately NOT attempted here: Lantern Mouse purple/green.** Its variant is a genuinely
    /// continuous HSLColor hue (LanternMouse.iVars.color.hue, a float from 0 up to but excluding
    /// 1), not a discrete
    /// flag -- bucketing that into "purple" vs. "green" is a judgment call, and the challenge's own
    /// notes tell human reviewers to "test colours using a colour picker and ask if unsure" rather
    /// than giving a hue range themselves. Automating a guess here risks disagreeing with however
    /// that's actually adjudicated, for the benefit of exactly one challenge (Offering) -- closer in
    /// spirit to this mod's other honor-system carve-outs (see COMMUNITY_CHALLENGES.md's own scoping
    /// note on technique-legality clauses) than to a clean state check. Left for a future pass, once
    /// there's a concrete hue range to check against rather than a guess.
    /// </summary>
    internal static class SurvivorCreatureVariant
    {
        private static readonly FieldInfo BigEelAlbinoField = AccessTools.Field(typeof(BigEel), "albino");
        private static readonly FieldInfo JetFishAlbinoField = AccessTools.Field(typeof(JetFish), "albino");

        /// <summary>True if this Leviathan is the albino variant, false if it's a confirmed
        /// non-albino, null if `creature` isn't a BigEel at all or the field couldn't be read.</summary>
        public static bool? IsAlbinoLeviathan(Creature creature)
        {
            if (!(creature is BigEel eel) || BigEelAlbinoField == null)
            {
                return null;
            }
            return BigEelAlbinoField.GetValue(eel) as bool?;
        }

        /// <summary>True if this Jetfish is the albino variant, false if it's a confirmed
        /// non-albino, null if `creature` isn't a JetFish at all or the field couldn't be read.</summary>
        public static bool? IsAlbinoJetfish(Creature creature)
        {
            if (!(creature is JetFish fish) || JetFishAlbinoField == null)
            {
                return null;
            }
            return JetFishAlbinoField.GetValue(fish) as bool?;
        }
    }
}
