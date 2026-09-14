using System;
using System.Reflection;
using HarmonyLib;
using MoreSlugcats;
using UnityEngine;

namespace AltCharacters.Gimmicks
{
    /// <summary>
    /// The "Slugpup" gimmick: play the character as a slugpup instead of an adult slugcat.
    ///
    /// Vanilla decides "is this player currently a pup" entirely through one field,
    /// Player.SlugCatClass -- not through the player's true save-slot identity
    /// (Player.playerState.slugcatCharacter). Every place that treats a player differently for
    /// being a pup (body size and jump height via Player.isSlugpup/npcCharacterStats, grabbing
    /// and two-handed-item rules, AI perception of the player as prey-sized, and every visual
    /// difference in PlayerGraphics) reads SlugCatClass, not the save-slot identity. Vanilla
    /// itself already exploits exactly this split for Jolly Co-op's own "make this player a
    /// pup" checkbox, which simply assigns SlugCatClass = SlugcatStatsName.Slugpup for that
    /// player slot while the slot's real identity is untouched.
    ///
    /// This patch does the same thing for a real vanilla character, only while playing on this
    /// mod's reserved save slot (AltSlot.ReservedSlot -- see that class's doc): it runs right
    /// after Player.GetInitialSlugcatClass() -- the vanilla method that first assigns
    /// SlugCatClass (see Player.cs; the relevant branch there is "SlugCatClass =
    /// playerState.slugcatCharacter" for ordinary single-player) -- and overrides it to Slugpup
    /// when the player's real character has the gimmick assigned. Everything downstream that
    /// reads SlugCatClass (which GetInitialSlugcatClass's own caller already accounts for
    /// immediately afterwards, e.g. it constructs npcCharacterStats only if isSlugpup is now
    /// true) then treats the player as a genuine slugpup automatically, with no further patching
    /// needed.
    ///
    /// One tradeoff worth calling out: vanilla's own diet/food-value lookups
    /// (SlugcatStats.NourishmentOfObjectEaten and friends) key off SlugCatClass too, so once this
    /// override is in effect the character's own vanilla diet no longer applies for as long as
    /// it's flagged as a pup -- it eats like a vanilla slugpup instead. That matches how vanilla
    /// pups behave, so it's left as-is rather than special-cased.
    ///
    /// This runs very early in Player's own construction, before it's placed in a room, so it
    /// reads the live RainWorld off the abstract creature's world (already set at this point --
    /// see Creature's constructor) rather than through room, which isn't assigned yet.
    ///
    /// IMPORTANT -- this flip alone is NOT the whole story. See SlugpupStatsGimmick and
    /// SlugpupColorGimmick below for the rest of this gimmick (why a naive SlugCatClass flip
    /// alone made e.g. Primal Gourmand "look a little thin" and render in the wrong color, and
    /// how the fixes for both were found).
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.GetInitialSlugcatClass))]
    internal static class SlugpupGimmick
    {
        public const string ID = "Slugpup";

        private static void Postfix(Player __instance)
        {
            Player self = __instance;
            RainWorld rainWorld = self?.abstractCreature?.world?.game?.rainWorld;
            if (self?.playerState == null || rainWorld == null || !AltSlot.IsActive(rainWorld))
            {
                return;
            }

            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            self.SlugCatClass = MoreSlugcatsEnums.SlugcatStatsName.Slugpup;
        }
    }

    /// <summary>
    /// The second part of the Slugpup gimmick: re-derives the pup's physical stats from the real
    /// character's own stats, instead of leaving them as vanilla's generic, randomized pup stats.
    ///
    /// Why this is needed: SlugpupGimmick above only flips Player.SlugCatClass to Slugpup. That's
    /// enough to make Player.isSlugpup return true (vanilla: "ModManager.MSC &amp;&amp;
    /// SlugCatClass == Slugpup"), but isSlugpup being true changes what Player.slugcatStats
    /// (the getter everything else -- body mass, run speed, visibility, lung capacity, etc. --
    /// actually reads) returns: instead of the game's per-character SlugcatStats (session-wide,
    /// built once from the real character's own identity), it returns Player.npcCharacterStats
    /// instead -- a completely separate, per-Player, private field.
    ///
    /// npcCharacterStats is built exactly once, inside Player's own constructor, immediately
    /// after GetInitialSlugcatClass runs: "if (isSlugpup) npcStats = new NPCStats(this);", and
    /// NPCStats's constructor (as a side effect of building itself) also populates
    /// npcCharacterStats. Ground-truthed by disassembling the real game assembly (no decompiler
    /// was available in the environment this was reverse-engineered in, so a small hand-rolled
    /// ECMA-335 metadata/IL reader was used instead): NPCStats's constructor seeds Random with a
    /// per-creature RNG seed (creature.ID), rolls five independent random "personality" values
    /// (Bal/Met/Stealth/Size/Wideness, each Random.Range(0,1) raised to the 1.5 power), builds a
    /// throwaway `new SlugcatStats(SlugcatStatsName.Slugpup, malnourished)` -- the plain, generic
    /// baseline Slugpup stats, with NO reference to whatever character is actually being played --
    /// and multiplies that baseline's fields by small personality-driven factors centered near
    /// 1.0 (e.g. bodyWeightFac *= (0.85 + 0.15*Wideness) + 0.10*Met). The result is stored into
    /// npcCharacterStats.
    ///
    /// In other words: vanilla's own isSlugpup path was never designed to preserve "which
    /// character is this, really" -- it was built for actual generic NPC slugpups (and Jolly
    /// Co-op's "play as a nameless pup" mode), where there IS no other identity to preserve. Once
    /// SlugpupGimmick routes a real vanilla character (e.g. Primal Gourmand, whose own
    /// bodyWeightFac is considerably larger than generic Slugpup's) down this same path, that
    /// character's real proportions are simply discarded and replaced with a randomized
    /// near-1.0x variation of the generic pup's -- hence "looks a little thin".
    ///
    /// The fix (and the "extra logic" this was missing) is ported from the third-party "Pupify"
    /// mod (Steam Workshop item 3005429221), which exists specifically to let any character play
    /// as a pup companion in Jolly Co-op while keeping their own build. Pupify hooks
    /// SlugcatStats's constructor itself and, for the SlugcatStats object about to become a
    /// player's pup stats, overwrites several of its fields as fixed fractions of a freshly-built
    /// reference SlugcatStats for the real underlying character -- not vanilla's randomized
    /// generic-pup version. Reverse-engineered from Pupify.dll via the same hand-rolled IL reader
    /// (PlayerHooks.SlugcatStats_ctor), its "full" scaling branch applies, relative to that real
    /// reference (all multipliers are Pupify's own literal float constants, kept exact):
    ///
    ///   runspeedFac              *= 0.8f * 0.9523810148239136f   (~x0.762)
    ///   bodyWeightFac             *= 0.6499999761581421f * 1.0256409645080566f   (~x0.667)
    ///   generalVisibilityBonus   -=  0.20000000298023224f
    ///   visualStealthInSneakMode *=  1.2000000476837158f
    ///   loudnessFac              *=  0.5f
    ///   lungsFac                 *=  0.800000011920929f
    ///   poleClimbSpeedFac        *=  0.800000011920929f * 0.9569377899169922f    (~x0.766)
    ///   corridorClimbSpeedFac    *=  0.800000011920929f * 0.9523810148239136f    (~x0.762)
    ///   throwingSkill             -= 1   (floored at 0; throwingSkill is an int, not a float)
    ///
    /// This patch reproduces that same relationship, but through this mod's own simpler,
    /// single-player-appropriate hook point rather than Pupify's own Jolly Co-op multiplayer
    /// bookkeeping (which AltCharacters has no use for -- there's always exactly one real
    /// character in play, on one reserved save slot).
    ///
    /// FIRST ATTEMPT (wrong, corrected below): Postfixing Player's own constructor. That runs
    /// after npcCharacterStats has been built, so the field values themselves DID get rescaled
    /// correctly -- but tested in-game and reported as "no change". Root cause, found by tracing
    /// Player's constructor IL further: the two main BodyChunks are created a little further down
    /// in that SAME constructor, and their mass is computed from slugcatStats.bodyWeightFac
    /// *right there at chunk-creation time* -- i.e. from whatever npcCharacterStats held at that
    /// exact moment, not whatever it holds once the whole constructor finishes. A Postfix on the
    /// entire constructor necessarily runs too late: by the time it fires, both BodyChunks
    /// already have their (wrong, generic-random) mass permanently baked in, and nothing
    /// afterwards re-reads bodyWeightFac to redo that (Player.setPupStatus does exactly this
    /// re-derivation, but only when PlayerState.isPup is set, a separate flag this gimmick
    /// doesn't touch).
    ///
    /// THE FIX: patch NPCStats's own constructor instead -- `NPCStats(Player player)`, the thing
    /// that actually builds npcCharacterStats as a side effect of building itself (see above).
    /// Harmony's Postfix on it fires the instant that nested call returns, which is BEFORE
    /// Player's constructor resumes on to create the BodyChunks -- closing the timing gap for
    /// good. This mirrors the exact lesson already documented for OneSpearGimmick's two-hook-point
    /// pattern: a single hook one step too late can look like "nothing happened" instead of
    /// throwing, because nothing about it is actually broken -- it's just observed too late.
    ///
    /// One more guard this needs that the naive version didn't: NPCStats's constructor is NOT
    /// exclusive to real players wearing this gimmick -- it's the same constructor genuine
    /// AI-controlled NPC slugpups (Player.isNPC == true; they're implemented as ordinary Player
    /// instances) go through too. Explicitly excluding isNPC keeps this patch from ever touching
    /// an actual wandering/companion pup's stats, even in the unlikely case one existed on the
    /// reserved slot.
    ///
    /// Both npcCharacterStats itself (Player) and every field read/written above (SlugcatStats)
    /// are non-public in the real game assembly (this mod's compile-time reference assembly is
    /// "publicized" -- see Gotcha #1 in AI_CONTEXT.md -- so direct access compiles fine but would
    /// throw FieldAccessException at runtime); AccessTools reflection is used throughout for
    /// exactly that reason, following the same precedent as OneSpearGimmick's PlayerGraphics
    /// field access.
    /// </summary>
    // NPCStats is a type nested inside Player (confirmed via the game assembly's metadata --
    // it's not in the global namespace despite the doc comments above referring to it as plain
    // "NPCStats" for readability), so Player.NPCStats is its real, compilable name.
    [HarmonyPatch(typeof(Player.NPCStats), MethodType.Constructor, new Type[] { typeof(Player) })]
    internal static class SlugpupStatsGimmick
    {
        private static readonly FieldInfo NpcCharacterStatsField =
            AccessTools.Field(typeof(Player), "npcCharacterStats");

        private static readonly FieldInfo RunspeedFacField = AccessTools.Field(typeof(SlugcatStats), "runspeedFac");
        private static readonly FieldInfo BodyWeightFacField = AccessTools.Field(typeof(SlugcatStats), "bodyWeightFac");
        private static readonly FieldInfo GeneralVisibilityBonusField = AccessTools.Field(typeof(SlugcatStats), "generalVisibilityBonus");
        private static readonly FieldInfo VisualStealthInSneakModeField = AccessTools.Field(typeof(SlugcatStats), "visualStealthInSneakMode");
        private static readonly FieldInfo LoudnessFacField = AccessTools.Field(typeof(SlugcatStats), "loudnessFac");
        private static readonly FieldInfo LungsFacField = AccessTools.Field(typeof(SlugcatStats), "lungsFac");
        private static readonly FieldInfo PoleClimbSpeedFacField = AccessTools.Field(typeof(SlugcatStats), "poleClimbSpeedFac");
        private static readonly FieldInfo CorridorClimbSpeedFacField = AccessTools.Field(typeof(SlugcatStats), "corridorClimbSpeedFac");
        private static readonly FieldInfo ThrowingSkillField = AccessTools.Field(typeof(SlugcatStats), "throwingSkill");

        // player is NPCStats's own constructor argument -- the same Player instance whose
        // npcCharacterStats is about to be populated as a side effect of this very constructor
        // call, one line before Player's own constructor resumes and creates its BodyChunks from
        // it. See this class's doc comment above for why that timing matters.
        private static void Postfix(Player player)
        {
            Player self = player;
            RainWorld rainWorld = self?.abstractCreature?.world?.game?.rainWorld;
            if (self?.playerState == null || rainWorld == null || !AltSlot.IsActive(rainWorld) || self.isNPC)
            {
                return;
            }

            // SlugCatClass has already been overridden to Slugpup (if this gimmick applies) by
            // SlugpupGimmick's own Postfix on GetInitialSlugcatClass, which runs earlier in
            // Player's constructor, before NPCStats is ever constructed -- so the real identity
            // has to come from playerState instead.
            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, SlugpupGimmick.ID))
            {
                return;
            }

            if (self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
            {
                // Gimmick is assigned but something upstream didn't actually flip the class
                // (shouldn't happen -- NPCStats is only ever constructed when isSlugpup is
                // already true -- but don't touch npcCharacterStats on a wrong assumption).
                return;
            }

            SlugcatStats pupStats = NpcCharacterStatsField.GetValue(self) as SlugcatStats;
            if (pupStats == null)
            {
                // NPCStats's own constructor hasn't stored it yet at the point this runs if it
                // ever changes order internally -- if it's missing there's nothing to rescale.
                return;
            }

            // A throwaway reference: the real character's own stats, completely unrelated to the
            // generic-random ones vanilla already put into pupStats. Matches Pupify's own use of
            // an "original character" reference to rescale from, rather than vanilla's baseline
            // generic Slugpup. malnourished is deliberately false here -- this is a fresh
            // spawn-time reference purely for its body-proportion fields, not a tracked ongoing
            // nourishment state.
            SlugcatStats real = new SlugcatStats(self.playerState.slugcatCharacter, false);

            RunspeedFacField.SetValue(pupStats, (float)RunspeedFacField.GetValue(real) * 0.8f * 0.9523810148239136f);
            BodyWeightFacField.SetValue(pupStats, (float)BodyWeightFacField.GetValue(real) * 0.6499999761581421f * 1.0256409645080566f);
            GeneralVisibilityBonusField.SetValue(pupStats, (float)GeneralVisibilityBonusField.GetValue(real) - 0.20000000298023224f);
            VisualStealthInSneakModeField.SetValue(pupStats, (float)VisualStealthInSneakModeField.GetValue(real) * 1.2000000476837158f);
            LoudnessFacField.SetValue(pupStats, (float)LoudnessFacField.GetValue(real) * 0.5f);
            LungsFacField.SetValue(pupStats, (float)LungsFacField.GetValue(real) * 0.800000011920929f);
            PoleClimbSpeedFacField.SetValue(pupStats, (float)PoleClimbSpeedFacField.GetValue(real) * 0.800000011920929f * 0.9569377899169922f);
            CorridorClimbSpeedFacField.SetValue(pupStats, (float)CorridorClimbSpeedFacField.GetValue(real) * 0.800000011920929f * 0.9523810148239136f);

            int throwingSkill = (int)ThrowingSkillField.GetValue(real) - 1;
            if (throwingSkill < 0)
            {
                throwingSkill = 0;
            }
            ThrowingSkillField.SetValue(pupStats, throwingSkill);

            Plugin.Log?.LogInfo($"[Slugpup] {characterID}: npcCharacterStats rescaled from real stats (bodyWeightFac {(float)BodyWeightFacField.GetValue(real):F2} -> {(float)BodyWeightFacField.GetValue(pupStats):F2})");
        }
    }

    /// <summary>
    /// The third part of the Slugpup gimmick: makes a pup-ified character render in their own
    /// real color instead of a random one.
    ///
    /// User report: "the gourmand pup i play as is the wrong color." Ground-truthed the same way
    /// as the stats issue, by disassembling PlayerGraphics.ApplyPalette (the method that decides
    /// the player's base body-part color every frame):
    ///
    ///   if (SlugCatClass == Slugpup) baseColor = player.ShortCutColor();
    ///   else                          baseColor = PlayerGraphics.SlugcatColor(CharacterForColor);
    ///
    /// (CharacterForColor -- a separate PlayerGraphics property, not used at all when the
    /// SlugCatClass branch is taken -- correctly reads playerState.slugcatCharacter, the real
    /// identity, and PlayerGraphics.SlugcatColor(name) is exactly vanilla's own per-character
    /// palette: Gourmand's own tan/pink, Rivulet's own blue, etc. That's the path every
    /// non-pup-flagged character uses, and it would already do exactly the right thing here if
    /// it were reached.)
    ///
    /// So once SlugpupGimmick flips SlugCatClass, ApplyPalette stops calling SlugcatColor
    /// entirely and instead colors the body from Player.ShortCutColor() -- vanilla's per-creature
    /// shortcut/pipe-transition tint. Disassembling that method found why it looks "wrong": for
    /// isSlugpup == true, ShortCutColor() has a few hardcoded special-case colors keyed off the
    /// creature's own RNG seed (used for specific named vanilla pups), and otherwise falls back
    /// to converting Player.npcStats's own H/S/L fields (a different field from npcCharacterStats
    /// above, but populated by that same NPCStats constructor) to RGB -- and those H/S/L values
    /// are themselves random, rolled from the creature's ID the same way the personality stats
    /// are (see SlugpupStatsGimmick's doc comment). Two real players on the same character can
    /// get two different "wrong" colors, and reloading a save can change it again, because it's
    /// tied to a per-spawn creature ID, not to anything about the real character.
    ///
    /// Note this fix is AltCharacters' own, not ported from Pupify -- Pupify's own decompiled
    /// hooks never touch color at all, so pup companions in that mod render with this same random
    /// color. The fix here just uses vanilla's own existing SlugcatColor(SlugcatStats.Name)
    /// lookup directly, the same one non-pup-flagged characters already use, applied to the real
    /// identity -- no new color logic invented.
    ///
    /// Implemented as a Postfix on Player.ShortCutColor() itself (rather than trying to patch
    /// ApplyPalette, which has many other unrelated branches) so every caller of it for this
    /// player -- the body palette above, and actual shortcut/pipe-transition tinting -- sees the
    /// real character's color consistently. PlayerGraphics.SlugcatColor is invoked via reflection
    /// rather than a direct call, consistent with this file's other patches -- see Gotcha #1 in
    /// AI_CONTEXT.md: this mod's reference assembly is publicized (shows everything as public),
    /// so there's no reliable way to tell from it alone whether a rarely-modded utility method
    /// like this one is actually public in the real assembly without a live test.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.ShortCutColor))]
    internal static class SlugpupColorGimmick
    {
        private static readonly MethodInfo SlugcatColorMethod =
            AccessTools.Method(typeof(PlayerGraphics), "SlugcatColor", new Type[] { typeof(SlugcatStats.Name) });

        private static void Postfix(Player __instance, ref Color __result)
        {
            Player self = __instance;
            RainWorld rainWorld = self?.abstractCreature?.world?.game?.rainWorld;
            if (self?.playerState == null || rainWorld == null || !AltSlot.IsActive(rainWorld) || self.isNPC)
            {
                return;
            }

            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, SlugpupGimmick.ID))
            {
                return;
            }

            if (self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
            {
                return;
            }

            object color = SlugcatColorMethod?.Invoke(null, new object[] { self.playerState.slugcatCharacter });
            if (color is Color realColor)
            {
                __result = realColor;
            }
        }
    }
}
