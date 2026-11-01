using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using MoreSlugcats;
using Watcher;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Added 2026-09-17: swaps a character's select-screen portrait between two mod-supplied asset
    /// sets -- "primal-&lt;character&gt;-default" and "primal-&lt;character&gt;-complete" -- while
    /// playing on this mod's reserved slot with the Remix screen set to read-only Primal mode
    /// (both conditions, per explicit request). Neither folder needs to exist yet: a missing
    /// default falls back to vanilla's own normal portrait; a missing complete falls back to
    /// vanilla's own "story ending" portrait specifically (not the void-sea/ascension one -- see
    /// below for why that distinction turned out to matter), and a handful of characters with no
    /// distinct story-ending asset of their own fall back to the ascension one instead as a last
    /// resort.
    ///
    /// **What "complete" means here, and why it needed its own new save marker.** This is
    /// deliberately NOT vanilla's own `ascended`/`altEnding` flags -- those track whether the REAL
    /// game was beaten, unrelated to this mod's own Requirements. "Complete" means: every
    /// Requirement currently ENABLED for this character (via the Requirements tab, same as
    /// anywhere else in this mod) has been satisfied at least once, ever, on this save --
    /// `RequirementCompletionSummary` (this file) computes that live, the instant it can (any time
    /// `RequirementCompletion.OnSatisfied` fires), and persists it as its own permanent
    /// "PrimalAllRequirementsComplete" marker via the same PrimalSaveMarkers mechanism every other
    /// per-character flag in this mod already uses -- "once you did [complete requirements], it's
    /// stored," per explicit request, rather than recomputed from scratch every time.
    ///
    /// **Why a stored marker, not a live recomputation, is what makes this menu-screen feature
    /// possible at all.** The character-select screen shows every real character at once, but
    /// vanilla itself only ever has ONE character's own save data fully loaded into a live object
    /// at a time (`RainWorld.progression.currentSaveState`) -- for every OTHER character on the
    /// same screen, vanilla's own code (`SlugcatSelectMenu.SaveGameData.MineForSaveData`, confirmed
    /// via decompilation) falls back to regex-scraping the RAW save-file text in memory
    /// (`PlayerProgression.GetProgLinesFromMemory()`) for a fixed whitelist of known field names --
    /// our own Requirement markers aren't on that whitelist. Rather than teach that scraper a whole
    /// new field schema, this reduces the menu-time question to the simplest possible one a plain
    /// substring search can answer: does this character's own raw save-state text contain the
    /// literal "PrimalAllRequirementsComplete" marker string? `PrimalSaveMarkers.Add` writes markers
    /// as plain strings embedded verbatim in that raw text (confirmed via decompiling
    /// DeathPersistentSaveData.ToString()/.FromString()), so this works for EVERY character on the
    /// slot, not just whichever one happens to be "active" -- exactly what was asked for.
    ///
    /// **The three vanilla portraits, and why "the non-void-sea ending" needed correcting.** Every
    /// character actually has up to three distinct select-screen portraits, confirmed via
    /// decompiling Menu/SlugcatSelectMenu.cs and Menu/MenuScene.cs: a default (mid-run) one, a
    /// "story"/alternate-ending one (`SlugcatPage.AddAltEndingImage()`, vanilla's own "Slugcat
    /// End_B - X" / "AltEnd_X" scenes -- shown for Survivor, Monk, Gourmand, Artificer, Rivulet,
    /// and Spearmaster), and the true void-sea/ascension one (`SlugcatPage.AddImage(true)`,
    /// "Ghost_X"/"Slugcat End - X"). Hunter, Saint, Inv, and Watcher have no distinct story-ending
    /// asset of their own in vanilla, so their own "complete, but primal-X-complete wasn't
    /// supplied" fallback uses the ascension asset instead, as the closest available "you finished"
    /// portrait.
    ///
    /// **How the redirect actually works, mechanically.** Every layer of a vanilla portrait scene
    /// is its own separate PNG file, loaded via `MenuIllustration`/`MenuDepthIllustration`'s shared
    /// `LoadFile`, which calls `AssetManager.ResolveFilePath(string)` synchronously, inline, at
    /// CONSTRUCTION time (confirmed via decompilation -- not lazily deferred to a later frame) --
    /// and that call is the ONE place every such load funnels through, already supporting mod-
    /// folder overrides. This file patches that method directly (Prefix, rewrites `path` before the
    /// original runs) rather than trying to rewrite `MenuScene`'s own giant per-character if/else
    /// chain of hardcoded folder/layer names -- doing that would mean reimplementing which files,
    /// depths, and shaders each character's own scene actually needs, and would still have to
    /// tolerate a mod-supplied folder with a DIFFERENT number of layer files than vanilla's own
    /// (confirmed as an explicit possibility, not yet a concrete list) -- letting vanilla's own
    /// scene-building code run completely unmodified and just swapping which folder its file loads
    /// resolve against is far more robust to that.
    ///
    /// Since `AssetManager.ResolveFilePath` has zero context of "which character's portrait is
    /// currently being built" (it's a bare path string), a second pair of patches on
    /// `SlugcatPage.AddImage`/`AddAltEndingImage` themselves brackets an "ambient context" (which
    /// character, complete or not) for exactly the duration of that one synchronous scene-build
    /// call -- safe specifically because the file loading is confirmed synchronous and nested
    /// entirely inside that call, not deferred. Those same two patches also FORCE which of the two
    /// vanilla methods actually runs, and with what `ascended` value, based purely on our own
    /// "complete" decision -- deliberately overriding vanilla's own ascended/altEnding dispatch
    /// entirely while gated, since this mod's whole framework is its own independent two-state
    /// (default/complete) system layered on top, not a supplement to vanilla's three-state one.
    ///
    /// **Known simplifications, flagged explicitly:**
    ///  - Hunter's "Dead" scene variant (a third, cosmetic default-state sub-variant) is treated the
    ///    same as its normal default scene -- not distinguished.
    ///  - Watcher has four distinct vanilla endings (A/B/C/D); this uses the "A" ending's own folder
    ///    as the single ascension-fallback candidate rather than picking per watcherEndingID.
    ///  - Inv's own scene splits its layers across TWO vanilla folders ("Slugcat - White", reused
    ///    from Survivor, for background/environment layers, plus a separate "Inv Screen" folder for
    ///    just its own body sprite) -- both redirect to the SAME "primal-Inv-*" folder, so a single
    ///    custom folder is expected to supply every layer Inv's own scene needs, environment and
    ///    body alike, rather than mirroring vanilla's own two-folder split.
    ///
    /// **Not yet built or tested** -- no .NET toolchain in this container, and this is easily the
    /// most speculative patch in this mod: it touches a Harmony-patched static utility method
    /// (AssetManager.ResolveFilePath) called constantly throughout the entire game, not just this
    /// one screen, so its own "ambient context inactive" fast-path bailout needs particular
    /// scrutiny on the next real build.
    /// </summary>
    internal static class PrimalPortraitOverride
    {
        // slugcatNumber is genuinely new surface for this codebase (nothing else reads it) --
        // resolved via reflection per Gotcha #1 discipline rather than trusted directly, the same
        // caution already applied to other narrow, previously-untouched fields on decompiled game
        // classes elsewhere in this mod (see PrimalMenuAccess.cs's own SaveStateField).
        private static readonly FieldInfo SlugcatNumberField =
            AccessTools.Field(typeof(Menu.SlugcatSelectMenu.SlugcatPage), "slugcatNumber");

        internal static SlugcatStats.Name SlugcatNumberOf(Menu.SlugcatSelectMenu.SlugcatPage page)
        {
            return SlugcatNumberField?.GetValue(page) as SlugcatStats.Name;
        }

        // One or more vanilla folder names (relative to "Scenes") used for one particular
        // portrait state of one character -- more than one only for Inv (see this class's own
        // header comment on its two-folder split). Null means "no distinct vanilla asset for this
        // state" -- callers fall back to whichever other state's own folder is appropriate.
        private sealed class CharacterFolders
        {
            public readonly string[] Default;
            public readonly string[] StoryEnding;
            public readonly string[] Ascended;

            public CharacterFolders(string[] defaultFolders, string[] storyEndingFolders, string[] ascendedFolders)
            {
                Default = defaultFolders;
                StoryEnding = storyEndingFolders;
                Ascended = ascendedFolders;
            }

            // The vanilla fallback candidates for "complete but no primal-X-complete folder
            // supplied" -- the story ending if this character has one, else the ascension one.
            public string[] CompleteFallback => StoryEnding ?? Ascended;
        }

        // Built lazily (not a static field initializer) because MoreSlugcatsEnums/WatcherEnums
        // values are only meaningfully registered while their owning DLC is actually enabled --
        // see PrimalOptions.BuildAllCharacters's own identical reasoning and the bug it was fixing.
        private static Dictionary<string, CharacterFolders> _folders;

        private static Dictionary<string, CharacterFolders> Folders()
        {
            if (_folders != null)
            {
                return _folders;
            }

            _folders = new Dictionary<string, CharacterFolders>
            {
                [SlugcatStats.Name.White.value] = new CharacterFolders(
                    new[] { "Slugcat - White" },
                    new[] { "Slugcat End_B - White" },
                    new[] { "White Ghost Slugcat" }),
                [SlugcatStats.Name.Yellow.value] = new CharacterFolders(
                    new[] { "Slugcat - Yellow" },
                    new[] { "Slugcat End_B - Yellow" },
                    new[] { "Yellow Ghost Slugcat" }),
                [SlugcatStats.Name.Red.value] = new CharacterFolders(
                    new[] { "Slugcat - Red" },
                    null,
                    new[] { "Red Ghost Slugcat" }),
            };

            if (ModManager.MSC)
            {
                _folders[MoreSlugcatsEnums.SlugcatStatsName.Gourmand.value] = new CharacterFolders(
                    new[] { "Slugcat - Gourmand" },
                    new[] { "Slugcat End_B - Gourmand" },
                    new[] { "Slugcat End - Gourmand" });
                _folders[MoreSlugcatsEnums.SlugcatStatsName.Artificer.value] = new CharacterFolders(
                    new[] { "Slugcat - Artificer" },
                    new[] { "Slugcat End_B - Artificer" },
                    new[] { "Slugcat End - Artificer" });
                _folders[MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value] = new CharacterFolders(
                    new[] { "Slugcat - Rivulet" },
                    new[] { "Slugcat End_B - Rivulet" },
                    new[] { "Slugcat End - Rivulet" });
                _folders[MoreSlugcatsEnums.SlugcatStatsName.Saint.value] = new CharacterFolders(
                    new[] { "Slugcat - Saint" },
                    null,
                    new[] { "Slugcat End - Saint" });
                _folders[MoreSlugcatsEnums.SlugcatStatsName.Spear.value] = new CharacterFolders(
                    new[] { "Slugcat - Spearmaster" },
                    new[] { "Slugcat End_B - Spearmaster" },
                    new[] { "Slugcat End - Spearmaster" });
                // Inv -- see this class's own header comment on the two-folder split.
                _folders[MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel.value] = new CharacterFolders(
                    new[] { "Slugcat - White", "Inv Screen" },
                    null,
                    new[] { "White Ghost Slugcat", "Inv Screen" });
            }

            if (ModManager.Watcher)
            {
                // Only the "A" ending's own folder is mapped -- see this class's own header
                // comment on why the other three (B/C/D) aren't distinguished here.
                _folders[WatcherEnums.SlugcatStatsName.Watcher.value] = new CharacterFolders(
                    new[] { "Slugcat - Watcher" },
                    null,
                    new[] { "slugcat end_a - watcher" });
            }

            return _folders;
        }

        // ---- the "ambient context" a synchronous scene-build call runs inside -------------------

        private static bool _active;
        private static string _characterID;
        private static bool _complete;

        internal static bool ContextActive => _active;

        internal static void BeginContext(SlugcatStats.Name character, bool complete)
        {
            _active = true;
            _characterID = character?.value;
            _complete = complete;
        }

        internal static void EndContext()
        {
            _active = false;
            _characterID = null;
        }

        // ---- the gate: reserved Primal slot AND Remix set to read-only Primal mode --------------

        internal static bool IsGated(RainWorld rainWorld)
        {
            return rainWorld != null
                && PrimalSlot.IsActive(rainWorld)
                && PrimalOptions.Instance != null
                && PrimalOptions.Instance.IsReadOnly;
        }

        // ---- "has this character satisfied everything currently assigned to it, ever" ----------

        // Finds this character's own raw save-state progress line for the CURRENTLY ACTIVE
        // PROGRESSION (i.e. whichever slot is presently loaded -- the reserved Primal slot, once
        // IsGated has already confirmed that) among every character on it -- not just whichever
        // one is "active" in the fuller sense vanilla's own SaveGameData.MineForSaveData
        // distinguishes. Mirrors that same method's own slow-path line-matching (Regex.Split by
        // "<progDivB>", check "SAVE STATE" then BackwardsCompatibilityRemix.ParseSaveNumber) since
        // that's the format GetProgLinesFromMemory's own entries are confirmed to use.
        private static bool TryFindProgressLine(RainWorld rainWorld, SlugcatStats.Name character, out string line)
        {
            line = null;
            string[] lines = rainWorld?.progression?.GetProgLinesFromMemory();
            if (lines == null)
            {
                return false;
            }

            foreach (string candidate in lines)
            {
                string[] parts = Regex.Split(candidate, "<progDivB>");
                if (parts.Length == 2 && parts[0] == "SAVE STATE" && BackwardsCompatibilityRemix.ParseSaveNumber(parts[1]) == character)
                {
                    line = candidate;
                    return true;
                }
            }
            return false;
        }

        internal static bool IsComplete(RainWorld rainWorld, SlugcatStats.Name character)
        {
            if (!TryFindProgressLine(rainWorld, character, out string line))
            {
                return false;
            }
            return line.Contains(RequirementCompletionSummary.MarkerFor(character.value));
        }

        // ---- the actual folder-name substitution, applied to AssetManager.ResolveFilePath's own path ----

        internal static string TryRedirect(string path)
        {
            if (!_active || _characterID == null || path == null || !Folders().TryGetValue(_characterID, out CharacterFolders folders))
            {
                return null;
            }

            string[] candidates = _complete ? folders.CompleteFallback : folders.Default;
            if (candidates == null)
            {
                return null;
            }

            string normalizedPath = path.Replace('/', Path.DirectorySeparatorChar);
            foreach (string vanillaFolder in candidates)
            {
                string vanillaPrefix = "Scenes" + Path.DirectorySeparatorChar + vanillaFolder + Path.DirectorySeparatorChar;
                if (!normalizedPath.StartsWith(vanillaPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string suffix = normalizedPath.Substring(vanillaPrefix.Length);
                string targetFolder = "primal-" + _characterID + (_complete ? "-complete" : "-default");
                string targetRelative = "Scenes" + Path.DirectorySeparatorChar + targetFolder;
                if (Directory.Exists(AssetManager.ResolveDirectory(targetRelative)))
                {
                    return targetRelative + Path.DirectorySeparatorChar + suffix;
                }
                // Matched this character's own vanilla folder, but no custom art was supplied for
                // this state -- fall through to vanilla's own (unmodified) path rather than
                // redirect to a folder that doesn't exist.
                return null;
            }

            return null;
        }
    }

    /// <summary>Persists, once true forever, whether a character has satisfied every Requirement
    /// currently assigned to it -- see PrimalPortraitOverride's own header comment for the full
    /// "why" (this is what makes the menu-time completion check a cheap substring search instead
    /// of needing live access to Remix config + every requirement's own save markers). Deliberately
    /// its own class, not folded into RequirementCompletion itself: this is a CONSUMER of that
    /// class's own OnSatisfied event, exactly like CommunityChallengesNotifications is, not part of
    /// the Requirements layer's own core contract.</summary>
    internal static class RequirementCompletionSummary
    {
        private const string CompleteKey = "PrimalAllRequirementsComplete";

        private static bool _initialized;

        /// <summary>Idempotent, matching this mod's other one-shot Init() methods (see
        /// RainWorldHooks.Postfix).</summary>
        internal static void Init()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;
            RequirementCompletion.OnSatisfied += OnAnyRequirementSatisfied;
        }

        internal static string MarkerFor(string characterID)
        {
            return PrimalSaveMarkers.Marker(CompleteKey, characterID);
        }

        // Runs on every single requirement completion, for every character -- checks whether THIS
        // specific character (the one the just-satisfied requirement belongs to) now has every one
        // of its own currently-ENABLED requirements satisfied, and if so, permanently marks it.
        // "Currently enabled" is read live (GimmickAssignments.IsEnabled), matching this mod's own
        // existing "requirements can be freely added/removed between runs" model -- once this
        // marker is written, though, it's permanent (PrimalSaveMarkers.Add's own idempotency),
        // exactly like the user's own framing ("stored as a saved value once you did") -- later
        // unassigning a requirement doesn't retroactively un-complete a character.
        private static void OnAnyRequirementSatisfied(Player player, string characterID, string requirementID, bool isLive, DeathPersistentSaveData saveData)
        {
            if (characterID == null || saveData == null)
            {
                return;
            }

            bool anyEnabled = false;
            foreach (Requirement requirement in RequirementRegistry.All)
            {
                if (!GimmickAssignments.IsEnabled(characterID, requirement.ID))
                {
                    continue;
                }
                anyEnabled = true;
                if (!RequirementCompletion.IsCompleted(saveData, characterID, requirement.ID))
                {
                    return;
                }
            }

            // A character with nothing assigned at all isn't vacuously "complete" -- there's
            // nothing to have completed.
            if (anyEnabled)
            {
                PrimalSaveMarkers.Add(saveData, MarkerFor(characterID));
            }
        }
    }

    /// <summary>Forces which of AddImage(bool)/AddAltEndingImage() actually builds this character's
    /// select-screen portrait, and with what `ascended` value, based purely on
    /// PrimalPortraitOverride's own "complete" decision -- overriding vanilla's own ascended/
    /// altEnding dispatch entirely while gated. See PrimalPortraitOverride's own header comment for
    /// the full mechanism and why this is safe (the whole scene build is confirmed synchronous).</summary>
    [HarmonyPatch(typeof(Menu.SlugcatSelectMenu.SlugcatPage), nameof(Menu.SlugcatSelectMenu.SlugcatPage.AddImage))]
    internal static class PrimalPortraitAddImageHook
    {
        private static bool Prefix(Menu.SlugcatSelectMenu.SlugcatPage __instance, ref bool ascended)
        {
            RainWorld rainWorld = __instance.menu?.manager?.rainWorld;
            SlugcatStats.Name character = PrimalPortraitOverride.SlugcatNumberOf(__instance);
            if (!PrimalPortraitOverride.IsGated(rainWorld) || character == null)
            {
                return true;
            }

            if (PrimalPortraitOverride.IsComplete(rainWorld, character))
            {
                // AddAltEndingImage's own Prefix/Postfix (below) brackets its own ambient context
                // for this nested call -- nothing to set up here for the complete branch.
                __instance.AddAltEndingImage();
                return false; // skip original AddImage entirely -- the portrait is already built.
            }

            PrimalPortraitOverride.BeginContext(character, complete: false);
            ascended = false; // force the default state regardless of vanilla's own real flag.
            return true; // let the original AddImage(false) run; this method's own Postfix cleans up.
        }

        private static void Postfix()
        {
            PrimalPortraitOverride.EndContext();
        }
    }

    /// <summary>Brackets ambient context for AddAltEndingImage specifically -- covers both the
    /// "forced" call from PrimalPortraitAddImageHook's own Prefix above, and the case where vanilla
    /// calls this method directly on its own (its own real altEnding condition, unrelated to this
    /// mod). See PrimalPortraitOverride's own header comment.</summary>
    [HarmonyPatch(typeof(Menu.SlugcatSelectMenu.SlugcatPage), nameof(Menu.SlugcatSelectMenu.SlugcatPage.AddAltEndingImage))]
    internal static class PrimalPortraitAddAltEndingImageHook
    {
        private static void Prefix(Menu.SlugcatSelectMenu.SlugcatPage __instance)
        {
            if (PrimalPortraitOverride.ContextActive)
            {
                // Already bracketed by PrimalPortraitAddImageHook's own Prefix, which called this
                // method directly -- nothing more to do.
                return;
            }

            RainWorld rainWorld = __instance.menu?.manager?.rainWorld;
            SlugcatStats.Name character = PrimalPortraitOverride.SlugcatNumberOf(__instance);
            if (!PrimalPortraitOverride.IsGated(rainWorld) || character == null)
            {
                return;
            }

            PrimalPortraitOverride.BeginContext(character, PrimalPortraitOverride.IsComplete(rainWorld, character));
        }

        private static void Postfix()
        {
            PrimalPortraitOverride.EndContext();
        }
    }

    /// <summary>The actual folder-name substitution -- see PrimalPortraitOverride.TryRedirect and
    /// this file's own header comment for why patching this one method covers every layer file
    /// every portrait scene loads (MenuIllustration/MenuDepthIllustration's own shared LoadFile,
    /// called synchronously at construction). Runs constantly, for every file load in the entire
    /// game, not just this screen -- TryRedirect's own first check (ambient context inactive) is
    /// the fast-path bailout that keeps this cheap everywhere else.</summary>
    [HarmonyPatch(typeof(AssetManager), nameof(AssetManager.ResolveFilePath), new Type[] { typeof(string) })]
    internal static class PrimalPortraitResolveFilePathHook
    {
        private static void Prefix(ref string path)
        {
            string redirected = PrimalPortraitOverride.TryRedirect(path);
            if (redirected != null)
            {
                path = redirected;
            }
        }
    }
}
