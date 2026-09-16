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
    /// **Deliberately NOT attempted here: Lantern Mouse purple/green.** 2026-09-16: traced the full
    /// formula this time (`LanternMouse.GenerateIVars`, not just confirmed its existence): hue is
    /// `UnityEngine.Random.value &lt; 0.01f` a fully uniform-random hue, else (roughly 50/50) either
    /// `Lerp(0.5, 0.65, random)` -- a cyan-through-blue/violet arc, the most plausible source of
    /// what the community calls "purple" -- or `Lerp(0, 0.1, random)` -- red/orange, not green at
    /// all. There is no dedicated "green" cluster in the ordinary (99%) case; a true green hue
    /// (~0.33) only ever arises from the rare 1% fully-random roll. This is genuinely NOT a clean
    /// two-value discrete split (it's one continuous float with two common clusters and a rare
    /// wildcard) -- bucketing it into "purple" vs. "green" is exactly the judgment call the
    /// challenge's own notes defer to a human ("test colours using a colour picker and ask if
    /// unsure") rather than resolve themselves. Automating a guess here risks disagreeing with
    /// however that's actually adjudicated, for the benefit of exactly one challenge (Offering) --
    /// closer in spirit to this mod's other honor-system carve-outs (see COMMUNITY_CHALLENGES.md's
    /// own scoping note on technique-legality clauses) than to a clean state check. Left
    /// unimplemented on purpose, now with the real formula recorded here for whoever revisits it.
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
