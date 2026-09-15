using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Slugpup" gimmick: play the character as a slugpup instead of an adult slugcat.
    ///
    /// **2026-09-15 rewrite -- see the bottom of this doc comment for why.** The original
    /// implementation (kept in git history) flipped Player.SlugCatClass to Slugpup outright.
    /// That is simple and does make Player.isSlugpup return true (routing stats through
    /// npcCharacterStats, correctly triggering every isSlugpup-gated behavior -- jump height,
    /// grabbing/two-handed-item rules, body position offsets, etc., all confirmed via
    /// reference/GameSource/Player.cs to read Player.isSlugpup, not SlugCatClass, at every one of
    /// their call sites) -- but it has a serious side effect nothing in the first pass caught:
    /// SlugCatClass is *also* the same field dozens of unrelated vanilla systems read directly to
    /// recognize "this player is playing as character X" -- PlayerGraphics' own Gourmand/
    /// Spearmaster-specific body-fatness scale, diet/food-value lookups, and (more importantly
    /// for anyone actually playing this gimmick) every gameplay-relevant per-character ability
    /// check elsewhere in the codebase. Flipping it away from the real character silently breaks
    /// all of those for as long as the flip is in effect -- exactly the opposite of what this
    /// gimmick is supposed to do (the user's own framing when this was first requested: "play as
    /// a slugpup while also changing behavior so that you can use that character's abilities as a
    /// slugpup").
    ///
    /// **The fix, ported properly from Pupify this time.** Re-reading Pupify's own decompiled
    /// hooks (reference/ExampleMods/Pupify/Pupify.decompiled.cs) with this specific question in
    /// mind found `PlayerHooks.Player_isSlugpup` (~line 2679): a Prefix/Postfix-style hook on
    /// `Player.isSlugpup`'s *getter itself* that forces it to return true for a pup-ified player,
    /// while *never touching SlugCatClass at all*. That is the actual trick Pupify uses to get a
    /// companion pup that looks and acts like the real underlying character, just pup-sized: every
    /// isSlugpup-gated behavior (the ones this gimmick actually wants) fires correctly because the
    /// getter says true, while every SlugCatClass-gated behavior (fatness scale, diet, abilities --
    /// the ones this gimmick was accidentally breaking) keeps reading the real, untouched
    /// character identity. This patch reproduces exactly that: a Postfix on
    /// `Player.isSlugpup`'s getter (MethodType.Getter -- same pattern already used by
    /// MiseryGimmick's PainJumps hook in this same codebase) that ORs in `true` when the real
    /// character has this gimmick assigned, and otherwise leaves the original result untouched.
    /// SlugCatClass is never written by this gimmick at all anymore.
    ///
    /// This runs very early in Player's own construction (isSlugpup is read at
    /// "if (isSlugpup) npcStats = new NPCStats(this);", right after GetInitialSlugcatClass, before
    /// the player is placed in a room), so -- same as the getter reads elsewhere in this file --
    /// it reads the live RainWorld off the abstract creature's world (already set at this point --
    /// see Creature's constructor) rather than through room, which isn't assigned yet.
    ///
    /// See SlugpupStatsGimmick and SlugpupColorGimmick below for the rest of this gimmick.
    /// SlugpupStatsGimmick's own logic (rescaling npcCharacterStats from the real character's
    /// stats) didn't need to change conceptually, just one now-obsolete guard removed (see its own
    /// doc comment). SlugpupColorGimmick's *scope* shrank a lot: with SlugCatClass never flipped,
    /// PlayerGraphics.ApplyPalette's own "if (SlugCatClass == Slugpup)" branch is simply never
    /// taken for this gimmick anymore, so the player's main body color is now correct automatically,
    /// no patching needed -- but Player.ShortCutColor() (used for actual shortcut/pipe-transition
    /// tinting, not just body color) branches on `!isSlugpup` in its own body, so once isSlugpup is
    /// forced true here, that one specific, narrower use still needs the fix. See its own doc
    /// comment for the full account.
    ///
    /// A separate, previously-undiscovered bug was also found and fixed in the same pass: see
    /// SlugpupMalnourishmentStatsGimmick below for why Player.SetMalnourished needed its own patch
    /// too, or the pup-scale rescale would have silently reverted the first time malnourishment
    /// changed after spawning.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.isSlugpup), MethodType.Getter)]
    internal static class SlugpupGimmick
    {
        public const string ID = "Slugpup";

        private static void Postfix(Player __instance, ref bool __result)
        {
            if (__result || __instance == null || __instance.isNPC)
            {
                return;
            }

            Player self = __instance;
            RainWorldGame game = self.abstractCreature?.world?.game;
            RainWorld rainWorld = game?.rainWorld;
            if (self.playerState == null || rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            if (SlugpupEndingMarker.HasEnded(game, characterID))
            {
                // Reached the Outer Expanse ending with this setting on -- permanently stopped
                // for this character on this save file. See SlugpupEndingHook's doc comment.
                return;
            }

            __result = true;
        }
    }

    /// <summary>
    /// The second part of the Slugpup gimmick: re-derives the pup's physical stats from the real
    /// character's own stats, instead of leaving them as vanilla's generic, randomized pup stats.
    ///
    /// Why this is needed: SlugpupGimmick above forces Player.isSlugpup to return true (without
    /// touching SlugCatClass -- see its own doc comment for the 2026-09-15 rewrite that changed
    /// this). isSlugpup being true changes what Player.slugcatStats (the getter everything else --
    /// body mass, run speed, visibility, lung capacity, etc. -- actually reads) returns: instead of
    /// the game's per-character SlugcatStats (session-wide, built once from the real character's
    /// own identity), it returns Player.npcCharacterStats instead -- a completely separate,
    /// per-Player, private field.
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
    /// bookkeeping (which Primal has no use for -- there's always exactly one real
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
        // player is NPCStats's own constructor argument -- the same Player instance whose
        // npcCharacterStats is about to be populated as a side effect of this very constructor
        // call, one line before Player's own constructor resumes and creates its BodyChunks from
        // it. See this class's doc comment above for why that timing matters.
        private static void Postfix(Player player)
        {
            Player self = player;
            RainWorld rainWorld = self?.abstractCreature?.world?.game?.rainWorld;
            if (self?.playerState == null || rainWorld == null || !PrimalSlot.IsActive(rainWorld) || self.isNPC)
            {
                return;
            }

            // 2026-09-15: SlugCatClass is no longer flipped by this gimmick at all (see
            // SlugpupGimmick's own doc comment above for why) -- the real identity always comes
            // from playerState, and the fact that NPCStats is being constructed at all already
            // proves isSlugpup was true for this player (either genuinely, or because
            // SlugpupGimmick's own Postfix forced it), so there's nothing left to double-check
            // via SlugCatClass here.
            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, SlugpupGimmick.ID))
            {
                return;
            }

            SlugpupStatsScaling.Rescale(self, characterID);
        }
    }

    /// <summary>
    /// 2026-09-15: a second bug found while tracing through this gimmick for the "not fat enough"
    /// report, unrelated to the SlugCatClass-flip rewrite above but uncovered by the same closer
    /// read of Player.cs. Player.SetMalnourished(bool, bool) -- called by vanilla any time a
    /// player's malnourished status actually changes (food count crossing the threshold in either
    /// direction, a routine, frequent in-game event, not an edge case) -- when isSlugpup is true,
    /// unconditionally *replaces* npcCharacterStats outright: "npcCharacterStats = new
    /// SlugcatStats(SlugCatClass, flag);". That is a fresh, un-rescaled SlugcatStats built from
    /// scratch, discarding whatever SlugpupStatsGimmick's own NPCStats-constructor Postfix had
    /// carefully rescaled at spawn time -- so the very first malnourishment change after spawning
    /// would have silently reverted the pup back to un-rescaled stats (vanilla generic-Slugpup
    /// sized, before this rewrite; full-adult-real-character sized, after it -- either way wrong)
    /// for the rest of that cycle. This existed since the very first version of the stats fix and
    /// was never caught before because it requires an in-game session where malnourishment
    /// actually toggles, which this container cannot test.
    ///
    /// Fix: the exact same pattern as SlugpupStatsGimmick above, just re-applied after
    /// SetMalnourished's own rebuild instead of after NPCStats's constructor -- a Postfix that
    /// re-runs the identical rescale (now shared via SlugpupStatsScaling.Rescale so the two
    /// patches can't drift apart) any time this method fires for a gimmick-enabled, currently
    /// pup-ified player.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.SetMalnourished))]
    internal static class SlugpupMalnourishmentStatsGimmick
    {
        private static void Postfix(Player __instance)
        {
            Player self = __instance;
            RainWorld rainWorld = self?.abstractCreature?.world?.game?.rainWorld;
            if (self?.playerState == null || rainWorld == null || !PrimalSlot.IsActive(rainWorld) || self.isNPC || !self.isSlugpup)
            {
                return;
            }

            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, SlugpupGimmick.ID))
            {
                return;
            }

            SlugpupStatsScaling.Rescale(self, characterID);
        }
    }

    /// <summary>
    /// The rescale formula itself (Pupify's own literal constants -- see SlugpupStatsGimmick's doc
    /// comment above for the full derivation), shared by every place that needs to (re)populate
    /// npcCharacterStats from the real character's own stats: SlugpupStatsGimmick (first build, at
    /// NPCStats construction time) and SlugpupMalnourishmentStatsGimmick (re-applied after
    /// Player.SetMalnourished's own unconditional rebuild). Kept as one shared implementation so
    /// the two call sites can't silently drift apart.
    /// </summary>
    internal static class SlugpupStatsScaling
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

        public static void Rescale(Player self, string characterID)
        {
            SlugcatStats pupStats = NpcCharacterStatsField.GetValue(self) as SlugcatStats;
            if (pupStats == null)
            {
                // Nothing built yet to rescale (shouldn't happen at either call site, but don't
                // fabricate a SlugcatStats object just to immediately overwrite it).
                return;
            }

            // A throwaway reference: the real character's own stats, completely unrelated to the
            // generic-random ones vanilla already put into pupStats. Matches Pupify's own use of
            // an "original character" reference to rescale from, rather than vanilla's baseline
            // generic Slugpup. malnourished is deliberately false here -- this is purely a
            // body-proportion reference, not a tracked ongoing nourishment state (the caller is
            // responsible for whatever malnourished flag pupStats itself should end up with).
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
    /// The third part of the Slugpup gimmick: makes a pup-ified character's shortcut/pipe-
    /// transition tint use their own real color instead of a random one.
    ///
    /// **Original scope (2026-09-14), now narrowed by the 2026-09-15 rewrite.** User report: "the
    /// gourmand pup i play as is the wrong color." Ground-truthed by disassembling
    /// PlayerGraphics.ApplyPalette (the method that decides the player's *main body* color every
    /// frame): `if (SlugCatClass == Slugpup) baseColor = player.ShortCutColor(); else baseColor =
    /// PlayerGraphics.SlugcatColor(CharacterForColor);`. Back when SlugpupGimmick flipped
    /// SlugCatClass to Slugpup outright, this branch was taken for the *main body color too*,
    /// which is why this class originally patched ShortCutColor to cover both cases at once.
    ///
    /// Now that SlugpupGimmick no longer touches SlugCatClass (see its own doc comment for why),
    /// ApplyPalette's SlugCatClass check is never true for this gimmick anymore, so the main body
    /// color already comes from `SlugcatColor(CharacterForColor)` -- the real character's own
    /// palette -- automatically, with no patching needed at all. **What's left, and still real:**
    /// `Player.ShortCutColor()` has its own separate, narrower use for actual shortcut/pipe-
    /// transition tinting (a brief visual effect, not the main body color), and its own body
    /// branches on `!isSlugpup` directly -- not SlugCatClass. Since SlugpupGimmick now forces
    /// isSlugpup to return true, ShortCutColor() would still take its "pup" branch for that one
    /// narrower purpose: a few hardcoded special-case colors keyed off the creature's own RNG seed
    /// (for specific named vanilla pups), and otherwise Player.npcStats's own H/S/L fields (a
    /// different field from npcCharacterStats, but populated by that same NPCStats constructor)
    /// converted to RGB -- themselves random, rolled from the creature's ID the same way the
    /// personality stats are (see SlugpupStatsGimmick's doc comment). This patch is what still
    /// fixes that one remaining case.
    ///
    /// Note this fix is Primal's own, not ported from Pupify -- Pupify's own decompiled hooks
    /// never touch color at all, so pup companions in that mod render with this same random
    /// shortcut-tint color. The fix here just uses vanilla's own existing
    /// SlugcatColor(SlugcatStats.Name) lookup directly, the same one non-pup-flagged characters
    /// already use, applied to the real identity -- no new color logic invented.
    ///
    /// PlayerGraphics.SlugcatColor is invoked via reflection rather than a direct call, consistent
    /// with this file's other patches -- see Gotcha #1 in AI_CONTEXT.md: this mod's reference
    /// assembly is publicized (shows everything as public), so there's no reliable way to tell
    /// from it alone whether a rarely-modded utility method like this one is actually public in
    /// the real assembly without a live test.
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
            if (self?.playerState == null || rainWorld == null || !PrimalSlot.IsActive(rainWorld) || self.isNPC)
            {
                return;
            }

            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, SlugpupGimmick.ID))
            {
                return;
            }

            if (!self.isSlugpup)
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

    /// <summary>
    /// The fourth part of the Slugpup gimmick (2026-09-15): once a character reaches the standard
    /// Outer Expanse ending -- walking all the way to the region's western edge -- stop applying
    /// this gimmick to them for the rest of that save file, if this setting (default on, see
    /// SlugpupEndingSettings) is enabled for them.
    ///
    /// Requested as: "if a given character with this setting reaches the ending sequence in outer
    /// expanse, after the credits if they come back to the run, they will stop being a slugpup --
    /// the gimmick will effectively be removed. its still applied, just doesnt do anything." The
    /// "still applied, just doesn't do anything" phrasing is deliberate and matches how this is
    /// implemented: GimmickAssignments.IsEnabled keeps returning true for this character forever
    /// (the Remix checkbox stays checked, nothing is unassigned) -- a separate, permanent save
    /// marker is what actually turns the gimmick's own effects off, read by SlugpupGimmick's own
    /// isSlugpup-getter Postfix (the single choke point everything else in this file already
    /// depends on -- see its own doc comment). Once isSlugpup naturally goes back to returning its
    /// original (false) result, NPCStats is never constructed for this player again (so
    /// SlugpupStatsGimmick simply never fires), Player.SetMalnourished's own isSlugpup check also
    /// goes the other way (so SlugpupMalnourishmentStatsGimmick never fires either), and
    /// SlugpupColorGimmick's own explicit `if (!self.isSlugpup) return;` guard catches it directly
    /// -- so this one check in one place is enough to turn off the whole gimmick's effects, with no
    /// need to duplicate an ending check into every other patch in this file.
    ///
    /// **CORRECTION (same day): the first version of this patch hooked the wrong thing.** It
    /// targeted `RainWorldGame.BeatGameMode(game, standardVoidSea: true)`, reasoning that its one
    /// call site (`RainWorldGame.ExitToVoidSeaSlideShow()`, from `VoidSea/VoidSeaScene.cs`'s rising-
    /// water sequence) was "the" Outer Expanse ending. The user corrected this directly: "the
    /// standard outer expanse ending is them reaching the outer expanse region and heading all the
    /// way to the left. what you described is the void or ascension ending -- that is why its
    /// called standardVoidSea." In other words `standardVoidSea` names a *specific*, separate
    /// ascension/void-sea outcome, not "the generic Outer Expanse ending" as first assumed.
    ///
    /// **Re-traced with that correction in mind.** Every room-specific script in
    /// `MoreSlugcats/MSCRoomSpecificScript.cs` was checked for what actually happens when a
    /// character simply walks to Outer Expanse's western edge, rather than trusting the first
    /// plausible-looking method name found. Room `"OE_FINAL03"` (hooked unconditionally for any
    /// `IsStorySession`, no character restriction at all) spawns `OE_GourmandEnding` -- a
    /// misleadingly-named class (presumably built for Gourmand first, then generalized) whose
    /// `Update` tracks *any* story character's `firstChunk.pos.x` dropping below a threshold
    /// (`1020f`, held for `80`-or-`200` ticks depending on character) as they walk left, explicitly
    /// handles `SlugcatStats.Name.White` (Survivor) inside the same method, uses
    /// `RoomCamera.CameraCutsceneType.EndingOE` for its cutscene, fades out, and only then calls
    /// `RainWorldGame.BeatGameMode(room.game, standardVoidSea: false)` -- `false`, not `true`. This
    /// is "walking to the far west of Outer Expanse," precisely matching the user's own
    /// description, and it applies to every character generically (not gated to Gourmand at the
    /// room level), unlike the several genuinely character-*specific* alt endings elsewhere
    /// (Rivulet's Sky Islands resolution, Artificer's `LC_FINAL`, Saint's, Spearmaster's `SI_A07`
    /// pearl sequence) that also happen to pass `standardVoidSea: false` but are unrelated to Outer
    /// Expanse entirely -- which is exactly why patching `BeatGameMode` directly (keying only on
    /// the bool) was never going to work cleanly even after fixing the true/false confusion: that
    /// one method is the shared endpoint for half a dozen unrelated endings, with no room/region
    /// info of its own to distinguish them. Patching `OE_GourmandEnding.Update` itself sidesteps
    /// that ambiguity entirely by hooking the one class that's specific to this exact ending.
    ///
    /// A `ConditionalWeakTable` guards against re-firing on every later frame once
    /// `doneFinalSave` is set (vanilla's own logic already returns early once it's true, so this is
    /// belt-and-suspenders, matching the same double-count-guard pattern the Kill List gimmick uses
    /// for its own Creature.Die() Postfix). `OE_GourmandEnding` and its `room`/`doneFinalSave`
    /// fields are all plain `public` members on an ordinary room-behavior script (not a
    /// tightly-encapsulated simulation object like `Player`/`PlayerGraphics`), so this is patched
    /// directly rather than through reflection -- the same judgment call already applied to
    /// `Menu.SlugcatSelectMenu.SlugcatPageNewGame` elsewhere in this codebase (see Gotcha #1 in
    /// AI_CONTEXT.md).
    ///
    /// Confirmed this doesn't end the save file (matching the user's own framing, "after the
    /// credits if they come back to the run"): nothing in `OE_GourmandEnding` deletes or locks
    /// anything -- picking the same character on the same slot again afterward resumes from the
    /// same den, same as vanilla's own well-known "keep playing after the ending" behavior.
    /// </summary>
    [HarmonyPatch(typeof(MSCRoomSpecificScript.OE_GourmandEnding), nameof(MSCRoomSpecificScript.OE_GourmandEnding.Update))]
    internal static class SlugpupEndingHook
    {
        private static readonly ConditionalWeakTable<MSCRoomSpecificScript.OE_GourmandEnding, object> _handled =
            new ConditionalWeakTable<MSCRoomSpecificScript.OE_GourmandEnding, object>();

        private static void Postfix(MSCRoomSpecificScript.OE_GourmandEnding __instance)
        {
            if (__instance == null || !__instance.doneFinalSave || _handled.TryGetValue(__instance, out _))
            {
                return;
            }

            _handled.Add(__instance, null);

            RainWorldGame game = __instance.room?.game;
            if (game == null || !game.IsStorySession || !PrimalSlot.IsActive(game.rainWorld))
            {
                return;
            }

            string characterID = game.StoryCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, SlugpupGimmick.ID))
            {
                return;
            }

            if (!SlugpupEndingSettings.EndsAtOuterExpanse(characterID))
            {
                return;
            }

            if (SlugpupEndingMarker.RecordEnded(game, characterID))
            {
                Plugin.Log?.LogInfo($"[Slugpup] {characterID}: reached the Outer Expanse ending -- the gimmick stays assigned but stops applying for the rest of this save file.");
            }
        }
    }

    /// <summary>
    /// The permanent, per-character, per-save-file marker SlugpupEndingHook writes and
    /// SlugpupGimmick's own isSlugpup Postfix reads -- same DeathPersistentSaveData.
    /// unrecognizedSaveStrings passthrough mechanism already established by OneSpearGimmick (see
    /// that file's own doc comment) and the Kill List gimmick, reused here for a plain one-time
    /// flag rather than a counted/indexed marker, since this only ever needs to be recorded once.
    /// </summary>
    internal static class SlugpupEndingMarker
    {
        private const string SaveMarkerKey = "PRIMAL_SLUGPUP_ENDED";

        private static string Marker(string characterID)
        {
            return SaveMarkerKey + "<dpB>" + characterID;
        }

        public static bool HasEnded(RainWorldGame game, string characterID)
        {
            if (game == null || characterID == null || !game.IsStorySession)
            {
                return false;
            }

            List<string> markers = game.GetStorySession?.saveState?.deathPersistentSaveData?.unrecognizedSaveStrings;
            return markers != null && markers.Contains(Marker(characterID));
        }

        // Returns true if this call is what actually recorded the marker (false if it was already
        // present, or if it couldn't be written) -- so callers can log only on the real transition.
        public static bool RecordEnded(RainWorldGame game, string characterID)
        {
            List<string> markers = game?.GetStorySession?.saveState?.deathPersistentSaveData?.unrecognizedSaveStrings;
            if (markers == null || characterID == null)
            {
                return false;
            }

            string marker = Marker(characterID);
            if (markers.Contains(marker))
            {
                return false;
            }

            markers.Add(marker);
            return true;
        }
    }

    /// <summary>Per-character settings for the Slugpup gimmick: whether reaching the Outer Expanse
    /// ending permanently stops it (see SlugpupEndingHook). Defaults to on, per the user's own
    /// request that this behavior ship as the default rather than an opt-in.</summary>
    internal static class SlugpupEndingSettings
    {
        private const bool DefaultEndsAtOuterExpanse = true;

        private static readonly Dictionary<string, Configurable<bool>> _endsAtOuterExpanse = new Dictionary<string, Configurable<bool>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why (Configurable<T> can only ever be bound to one
        // UIconfig element at a time; building a fresh OpCheckBox every time this sub-view opened
        // threw Menu.Remix.MultiuseConfigurableException the second time around).
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            _endsAtOuterExpanse[characterID] = owner.config.Bind(
                characterID + "_Slugpup_endsAtOuterExpanse",
                DefaultEndsAtOuterExpanse,
                new ConfigurableInfo("Once this character reaches the standard Outer Expanse ending, stop being a slugpup for the rest of this save file. The gimmick stays assigned -- it just no longer does anything."));
        }

        public static bool EndsAtOuterExpanse(string characterID)
        {
            return _endsAtOuterExpanse.TryGetValue(characterID, out Configurable<bool> c) ? c.Value : DefaultEndsAtOuterExpanse;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            var items = new List<UIelement>();
            if (!_endsAtOuterExpanse.TryGetValue(characterID, out Configurable<bool> endsAtOuterExpanse))
            {
                // Not bound yet -- don't cache an incomplete/empty result.
                return items;
            }

            items.Add(new OpCheckBox(endsAtOuterExpanse, 20f, topY - 40f));
            items.Add(new OpLabel(56f, topY - 36f, "Stop being a slugpup after the Outer Expanse ending", false));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
