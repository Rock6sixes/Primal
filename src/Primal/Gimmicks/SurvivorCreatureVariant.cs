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
    /// Sleepover's Red Lizard, and Pilgrimage's Cyan Lizard need NO new mechanism at all:
    /// `creature.creatureTemplate.type == CreatureTemplate.Type.RedLizard` (etc.) is already
    /// sufficient, the same one-line check every other type-based gimmick in this mod already uses.
    ///
    /// **2026-09-16: Safari's third creature confirmed too.** `CreatureTemplate.Type.SpitterSpider`
    /// is also its own distinct registered Type (CreatureTemplate.cs) -- so all three of Safari's
    /// creatures (Red Lizard, Red Centipede, Spitter Spider) are plain Type comparisons, closing
    /// out that challenge's own variant-identification need entirely. This revises those six
    /// challenges' original "4 -- creature variant research needed" ratings down; see
    /// COMMUNITY_CHALLENGES.md's own updated notes.
    ///
    /// What's left, and what this file actually provides, is the minority of species where the
    /// variant genuinely is a runtime field rather than a distinct Type: BigEel (Leviathan) and
    /// JetFish both have a real, confirmed `public bool albino` field (BigEel.cs/JetFish.cs), set
    /// once at spawn from `world.region.regionParams.albinos`. Read through AccessTools rather than
    /// directly, per Gotcha #1 in AI_CONTEXT.md -- same reasoning as PrimalMenuAccess: a field
    /// showing public in the decompiled/publicized reference proves nothing about the real runtime
    /// assembly's accessibility.
    ///
    /// **2026-09-16, later: Lantern Mouse purple/green implemented after all**, at the user's own
    /// explicit direction ("you should be able to find the hue") overriding the earlier honor-
    /// system deferral below (kept for the record, since the underlying uncertainty it describes
    /// is still real -- this is a judgment call, not a rediscovered certainty).
    ///
    /// `LanternMouse.GenerateIVars` (traced via decompilation, not guessed): hue is
    /// `UnityEngine.Random.value &lt; 0.01f` a fully uniform-random roll, else (roughly 50/50)
    /// either `Lerp(0.5, 0.65, random)` or `Lerp(0, 0.1, random)`. Bucketed as: PURPLE = the
    /// common `[0.5, 0.65]` cluster (standard HSL puts this cyan-to-blue, but at this formula's own
    /// saturation/lightness (1, 0.8) it renders closer to a blue-violet the community plausibly
    /// calls "purple" -- this IS the common cool-color case the formula actually produces, so it's
    /// the best available match). GREEN = the standard HSL green band `[0.25, 0.45]` -- NOT
    /// reachable via the formula's common (99%) path at all (which only ever produces the cool
    /// cluster above or the warm `[0, 0.1]` red/orange one), so a green Lantern Mouse can only ever
    /// come from the rare 1% fully-random roll landing in that band. Both accepted since the
    /// challenge itself accepts either color -- this doesn't try to guess which is "more likely
    /// correct", just whether the hue falls in either named band. `iVars`/`IndividualVariations`/
    /// `HSLColor.hue` are all confirmed public (LanternMouse.cs, HSLColor.cs) -- trusted directly,
    /// matching the precedent already set for `VultureState.mask`/`FriendTracker.friend`.
    ///
    /// **Original 2026-09-16 deferral, kept for context:** this is genuinely a continuous float
    /// with two common clusters and a rare wildcard, not a clean two-value discrete split --
    /// exactly the judgment call the challenge's own notes defer to a human for ("test colours
    /// using a colour picker and ask if unsure"). The hue-band choice above is Primal's own best
    /// guess at automating that judgment call, not a claim that this is definitively how a human
    /// reviewer would rule on every borderline case near either band's edges.
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

        // See this class's own header comment for exactly where these bands come from and why.
        private const float PurpleHueMin = 0.5f;
        private const float PurpleHueMax = 0.65f;
        private const float GreenHueMin = 0.25f;
        private const float GreenHueMax = 0.45f;

        /// <summary>True if this Lantern Mouse's generated hue falls in either the "purple" or
        /// "green" band this class's own header comment derives from `LanternMouse.GenerateIVars`
        /// -- Offering's own color requirement. Null if `creature` isn't a LanternMouse at all.</summary>
        public static bool? IsPurpleOrGreenLanternMouse(Creature creature)
        {
            if (!(creature is LanternMouse mouse))
            {
                return null;
            }

            float hue = mouse.iVars.color.hue;
            bool purple = hue >= PurpleHueMin && hue <= PurpleHueMax;
            bool green = hue >= GreenHueMin && hue <= GreenHueMax;
            return purple || green;
        }
    }
}
