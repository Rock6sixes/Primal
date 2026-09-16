using System.Collections.Generic;
using HarmonyLib;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Survivor" project's first real piece, per the user's own scoping: a single gimmick
    /// that will eventually track as many of the ~50 informal Rain World community challenges as
    /// possible in one run (see COMMUNITY_CHALLENGES.md for the full feasibility pass, and
    /// SurvivorEvents.cs/SurvivorRegionMarkers.cs/etc. for the shared tracking mechanisms already
    /// built ahead of this).
    ///
    /// **This pass is settings-only, by explicit request: "Dont yet implement anything relating to
    /// gameplay, just tracking what the user checks to persist across restarts."** Unlike every
    /// other gimmick in this file's siblings (Misery, Slugpup, Chains, ...), CommunityChallenges
    /// has NO Harmony patches at all yet -- nothing here reads game state or changes behavior. It
    /// exists purely so the character/gimmick assignment grid has something to assign, and so its
    /// own settings sub-view (CommunityChallengesSettings.BuildUI) has 50 checkboxes -- one per
    /// challenge -- for deciding which challenges to track once actual tracking exists.
    ///
    /// **Why these 50 toggles are Remix Configurables, not save-file state (answering the user's
    /// own question directly).** Remix config lives in `ModConfigs\rokas.primal.txt`, entirely
    /// outside any save file, keyed only by characterID -- confirmed empirically earlier this
    /// session, not just in theory: a stray "Kill List" assignment made during live UI testing
    /// survived a full game restart specifically because it was written there, not to any
    /// DeathPersistentSaveData. Every existing per-character setting in this mod (gimmick
    /// assignment itself, ChainsSettings, OneSpearSettings, TimeLimitSettings, ...) already works
    /// this way, and it's exactly the behavior wanted here: "if you change what gimmicks Primal
    /// Gourmand has, it should apply to all save slots" is already true today for gimmick
    /// assignment, and these 50 challenge-selection checkboxes inherit that same property for
    /// free by being built the same way -- no new persistence mechanism needed. This is
    /// deliberately DIFFERENT from actual in-run challenge PROGRESS (once that exists): completing
    /// "Flytrap" on one save file has no bearing on another save file's own attempt, so that half
    /// will need to go through DeathPersistentSaveData (via PrimalSaveMarkers, the same per-save
    /// mechanism Kill List/OneSpear/Slugpup's ending flag already use) once it's built -- not
    /// through Remix config the way these selection toggles are.
    ///
    /// **Not yet built or tested** -- no .NET toolchain in this container.
    /// </summary>
    internal static class CommunityChallengesGimmick
    {
        public const string ID = "CommunityChallenges";
    }

    /// <summary>One community challenge's stable config-key ID (letters/digits only, matching
    /// GimmickDef.ID's own key-validation constraint -- "U-Turn" is the one name here that isn't
    /// already alphanumeric on its own, hence UTurn/"U-Turn" being split into ID/DisplayName the
    /// same way OneSpearGimmick's internal ID and "Master Spear" display name already are) and its
    /// display name exactly as the community names it.</summary>
    internal sealed class ChallengeEntry
    {
        public readonly string ID;
        public readonly string DisplayName;

        public ChallengeEntry(string id, string displayName)
        {
            ID = id;
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// The full 50-challenge roster, in the same difficulty-ascending order the user's own source
    /// list was already given in (and COMMUNITY_CHALLENGES.md's table preserves) -- not
    /// alphabetical, not tracking-difficulty order, just the order the community itself ranks them
    /// in. No other metadata yet (no per-challenge tracking state, no difficulty rating carried
    /// over from COMMUNITY_CHALLENGES.md) -- this is purely the list needed to build 50 checkboxes
    /// today; a future pass wiring up real tracking is expected to extend ChallengeEntry itself
    /// rather than keep a second, parallel list in sync with this one.
    /// </summary>
    internal static class CommunityChallengesRegistry
    {
        public static readonly ChallengeEntry[] All =
        {
            new ChallengeEntry("Flytrap", "Flytrap"),
            new ChallengeEntry("Visitor", "Visitor"),
            new ChallengeEntry("Chieftain", "Chieftain"),
            new ChallengeEntry("Manslaughter", "Manslaughter"),
            new ChallengeEntry("Tasty", "Tasty"),
            new ChallengeEntry("Leukocyte", "Leukocyte"),
            new ChallengeEntry("Heist", "Heist"),
            new ChallengeEntry("Fishtank", "Fishtank"),
            new ChallengeEntry("Descension", "Descension"),
            new ChallengeEntry("Visions", "Visions"),
            new ChallengeEntry("Mycologist", "Mycologist"),
            new ChallengeEntry("Catalogue", "Catalogue"),
            new ChallengeEntry("Despicable", "Despicable"),
            new ChallengeEntry("Elder", "Elder"),
            new ChallengeEntry("Feast", "Feast"),
            new ChallengeEntry("Desperado", "Desperado"),
            new ChallengeEntry("Symbiote", "Symbiote"),
            new ChallengeEntry("Vegan", "Vegan"),
            new ChallengeEntry("Friend", "Friend"),
            new ChallengeEntry("Collector", "Collector"),
            new ChallengeEntry("Nomad", "Nomad"),
            new ChallengeEntry("Enlightenment", "Enlightenment"),
            new ChallengeEntry("Journey", "Journey"),
            new ChallengeEntry("Charged", "Charged"),
            new ChallengeEntry("Passages", "Passages"),
            new ChallengeEntry("Scholar", "Scholar"),
            new ChallengeEntry("Martyr", "Martyr"),
            new ChallengeEntry("Devotee", "Devotee"),
            new ChallengeEntry("Offering", "Offering"),
            new ChallengeEntry("Sleepover", "Sleepover"),
            new ChallengeEntry("Saint", "Saint"),
            new ChallengeEntry("Safari", "Safari"),
            new ChallengeEntry("Usurper", "Usurper"),
            new ChallengeEntry("Triplopia", "Triplopia"),
            new ChallengeEntry("Ranger", "Ranger"),
            new ChallengeEntry("Spearfisher", "Spearfisher"),
            new ChallengeEntry("Ascension", "Ascension"),
            new ChallengeEntry("Outlaw", "Outlaw"),
            new ChallengeEntry("Circuit", "Circuit"),
            new ChallengeEntry("Mother", "Mother"),
            new ChallengeEntry("Chef", "Chef"),
            new ChallengeEntry("Foe", "Foe"),
            new ChallengeEntry("Ornaments", "Ornaments"),
            new ChallengeEntry("Broadcast", "Broadcast"),
            new ChallengeEntry("Guardian", "Guardian"),
            new ChallengeEntry("Massacre", "Massacre"),
            new ChallengeEntry("Pilgrimage", "Pilgrimage"),
            new ChallengeEntry("Criminal", "Criminal"),
            new ChallengeEntry("UTurn", "U-Turn"),
            new ChallengeEntry("Perseverance", "Perseverance"),
        };
    }

    /// <summary>
    /// One Configurable&lt;bool&gt; per (character, challenge) -- 50 per character, following the
    /// exact Bind/BuildUI/per-character-cache shape every other *Settings class in this file's
    /// siblings already uses (ChainsSettings' own per-type ban checkboxes are the closest existing
    /// precedent: same Dictionary&lt;string, Dictionary&lt;string, Configurable&lt;bool&gt;&gt;&gt;
    /// shape, same "build once into a cached List&lt;UIelement&gt;" requirement from Gotcha #4).
    /// </summary>
    internal static class CommunityChallengesSettings
    {
        private static readonly Dictionary<string, Dictionary<string, Configurable<bool>>> _enabled =
            new Dictionary<string, Dictionary<string, Configurable<bool>>>();

        private static readonly Dictionary<string, List<UIelement>> _uiCache =
            new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            var perChallenge = new Dictionary<string, Configurable<bool>>();
            foreach (ChallengeEntry challenge in CommunityChallengesRegistry.All)
            {
                perChallenge[challenge.ID] = owner.config.Bind(
                    characterID + "_CommunityChallenges_" + challenge.ID,
                    // Default true -- "for the time being check everything as enabled" (explicit
                    // request), since there's no bulk-select control on the settings grid itself
                    // and real tracking needs something to actually exercise it against. Revisit
                    // once per-challenge selection is something a player is expected to curate.
                    true,
                    new ConfigurableInfo("Track the \"" + challenge.DisplayName + "\" community challenge for this character."));
            }
            _enabled[characterID] = perChallenge;
        }

        /// <summary>Whether this character currently has this challenge selected for tracking.
        /// Not consumed by anything yet -- exposed now so the next pass (real tracking) has a
        /// ready-made read API rather than reaching back into this class's private dictionary.</summary>
        public static bool IsChallengeEnabled(string characterID, string challengeID)
        {
            return _enabled.TryGetValue(characterID, out Dictionary<string, Configurable<bool>> perChallenge)
                && perChallenge.TryGetValue(challengeID, out Configurable<bool> config)
                && config.Value;
        }

        /// <summary>
        /// 50 checkboxes, 5 columns of 10 rows, column-major (column 0 gets challenges 0-9, column
        /// 1 gets 10-19, ...) -- the same fill order ChainsSettings' own 2-column banlist grid
        /// already uses, just with 5 columns instead of 2. Column width (116px) and row height
        /// (32px) chosen to mirror already-proven numbers elsewhere on this screen (the permanent
        /// character grid itself is 5 columns of 106px-wide buttons plus a 10px gap -- 116px here
        /// leaves the same margin for a checkbox+label pair instead of a button; ChainsSettings'
        /// own banlist rows are 32px tall). **Exact pixel fit for the longest names
        /// ("Enlightenment", 13 characters, the longest in this roster) at 5-column width is
        /// unverified in-game as of this pass** -- same honest caveat this file's ChainsSettings
        /// sibling already carries for its own preset-menu footprint.
        /// </summary>
        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            var items = new List<UIelement>();
            if (!_enabled.TryGetValue(characterID, out Dictionary<string, Configurable<bool>> perChallenge))
            {
                // Not bound yet -- don't cache an incomplete/empty result, same guard every other
                // *Settings.BuildUI in this mod uses (see ChainsSettings.BuildUI).
                return items;
            }

            items.Add(new OpLabel(20f, topY, "Community challenges to track for this character:", false));

            const int Rows = 10;
            const float ColumnWidth = 116f;
            const float RowHeight = 32f;
            const float CheckboxLabelGap = 20f;
            float gridTop = topY - 36f;

            ChallengeEntry[] all = CommunityChallengesRegistry.All;
            for (int i = 0; i < all.Length; i++)
            {
                ChallengeEntry challenge = all[i];
                if (!perChallenge.TryGetValue(challenge.ID, out Configurable<bool> config))
                {
                    continue;
                }

                int column = i / Rows;
                int row = i % Rows;
                float x = 20f + column * ColumnWidth;
                float y = gridTop - row * RowHeight;

                items.Add(new OpCheckBox(config, x, y));
                items.Add(new OpLabel(x + CheckboxLabelGap, y + 4f, challenge.DisplayName, false));
            }

            _uiCache[characterID] = items;
            return items;
        }
    }

    /// <summary>
    /// Persists which (character, challenge) pairs have ever been completed, and tracks which of
    /// those completions are "fresh" -- new since the last cycle-end screen -- for the summary
    /// dialog's bold-white highlighting. Two distinct concerns, both explicitly requested:
    ///
    /// **Permanent completion** -- once true, always true, forever, on this save file. Reuses
    /// PrimalSaveMarkers verbatim (same shape as Kill List's own permanent kill records), keyed on
    /// (characterID, challengeID). ReportCompletion is idempotent: calling it again for an
    /// already-completed challenge is a silent no-op -- no re-notification, no re-marking "fresh".
    ///
    /// **"Completed this cycle" (fresh) tracking, and why it needs TWO sets, not one.** The
    /// summary dialog is opened on demand (a button click) -- it might be opened once, several
    /// times, or never for any given cycle. What "fresh" has to mean is "newly completed since the
    /// last cycle-end screen was BUILT for this character" (matching the very existence of that
    /// screen, not whether the player happened to click through to the dialog). So:
    ///  - `_accumulating` is a live set that ReportCompletion adds newly-completed challenge IDs
    ///    into throughout a cycle (both from live, mid-cycle detections AND from anything reported
    ///    at Win time -- both happen strictly before the corresponding SleepAndDeathScreen is ever
    ///    built, since Win/GameOver's own processing always completes before that screen's
    ///    AddSubObjects runs).
    ///  - `SnapshotAndResetForNewScreen`, called exactly once per cycle from
    ///    CommunityChallengesButtonHook's own AddSubObjects Postfix (i.e. the moment that screen is
    ///    built, whether or not its button ever gets clicked), freezes whatever accumulated during
    ///    the cycle that just ended into `_lastScreenSnapshot` and resets `_accumulating` to empty
    ///    -- so the NEXT cycle's own completions start from a clean slate instead of inheriting
    ///    this cycle's.
    ///  - `WasCompletedThisCycle` reads only the frozen snapshot, so opening the dialog multiple
    ///    times off the same screen keeps showing the same "fresh" set instead of it draining away
    ///    after the first read.
    /// </summary>
    internal static class ChallengeCompletion
    {
        private const string CompletedKey = "SurvivorChallengeCompleted";

        private static readonly Dictionary<string, HashSet<string>> _accumulating =
            new Dictionary<string, HashSet<string>>();

        private static readonly Dictionary<string, HashSet<string>> _lastScreenSnapshot =
            new Dictionary<string, HashSet<string>>();

        /// <summary>Reports `challengeID` as completed for `characterID`. `isLive` says whether
        /// this was detected mid-cycle during active gameplay (true -- triggers the live
        /// popup+sound via ChallengeNotification) or only confirmable at the exact moment a cycle
        /// ended (false -- e.g. anything driven off SurvivorEvents.OnSuccessfulHibernation, since
        /// by the time that fires the game is already mid-transition to the cycle-end screen, not
        /// "during" play from the player's own perspective -- see the user's own framing: "whether
        /// the challenge was completed during the cycle or once you successfully saved").</summary>
        public static void ReportCompletion(Player player, string characterID, string challengeID, bool isLive)
        {
            if (characterID == null || challengeID == null)
            {
                return;
            }

            DeathPersistentSaveData saveData = PrimalSaveMarkers.SaveDataFor(GimmickGate.GameFor(player));
            if (saveData == null)
            {
                return;
            }

            bool firstTimeEver = PrimalSaveMarkers.Add(
                saveData,
                PrimalSaveMarkers.Marker(CompletedKey, characterID, challengeID));
            if (!firstTimeEver)
            {
                // Already completed on some earlier cycle -- idempotent, nothing more to do.
                return;
            }

            GetOrCreate(_accumulating, characterID).Add(challengeID);

            if (isLive)
            {
                ChallengeNotification.ShowChallengeCompleted(player, DisplayNameFor(challengeID));
            }
        }

        /// <summary>Has this character ever completed this challenge, on this save file?</summary>
        public static bool IsCompleted(DeathPersistentSaveData saveData, string characterID, string challengeID)
        {
            return PrimalSaveMarkers.Has(saveData, PrimalSaveMarkers.Marker(CompletedKey, characterID, challengeID));
        }

        /// <summary>Freezes this cycle's own newly-completed challenges into the snapshot the
        /// summary dialog reads, and resets the live accumulator for the next cycle. See this
        /// class's own header comment for why this has to run at cycle-end-SCREEN-build time
        /// (CommunityChallengesButtonHook), not at dialog-open time.</summary>
        public static void SnapshotAndResetForNewScreen(string characterID)
        {
            HashSet<string> current = _accumulating.TryGetValue(characterID, out HashSet<string> set)
                ? set
                : new HashSet<string>();
            _lastScreenSnapshot[characterID] = current;
            _accumulating[characterID] = new HashSet<string>();
        }

        /// <summary>Whether `challengeID` was newly completed as of the most recent cycle-end
        /// screen built for this character -- what the summary dialog uses to decide bold-white
        /// vs. plain/darkened.</summary>
        public static bool WasCompletedThisCycle(string characterID, string challengeID)
        {
            return _lastScreenSnapshot.TryGetValue(characterID, out HashSet<string> set) && set.Contains(challengeID);
        }

        private static HashSet<string> GetOrCreate(Dictionary<string, HashSet<string>> dict, string key)
        {
            if (!dict.TryGetValue(key, out HashSet<string> set))
            {
                set = new HashSet<string>();
                dict[key] = set;
            }
            return set;
        }

        private static string DisplayNameFor(string challengeID)
        {
            foreach (ChallengeEntry entry in CommunityChallengesRegistry.All)
            {
                if (entry.ID == challengeID)
                {
                    return entry.DisplayName;
                }
            }
            return challengeID;
        }
    }

    /// <summary>
    /// The live, mid-cycle "challenge complete" notification -- explicitly requested to reuse "a
    /// quiet sound heard after finishing a cycle and seeing your karma up or after eating a karma
    /// flower and the karma gets protected by the karma shield". Confirmed via decompiling
    /// HUD/KarmaMeter.cs that both of those moments are, in fact, the same underlying effect: its
    /// `reinforceAnimation` state machine plays a 3-stage sound sequence --
    /// HUD_Karma_Reinforce_Flicker, then _Small_Circle + _Contract, then (as the animation
    /// settles, right when `showAsReinforced` gets read) _Bump -- triggered by karma-meter changes
    /// generally, not exclusively the karma-flower-shield case.
    ///
    /// This reuses only _Bump, the last of the three, rather than replaying the whole ~140-frame
    /// (~3.5s) animated sequence: it's the one confirmed to land right at the animation's own
    /// "settled/confirmed" beat, and a single one-shot cue suits a plain popup message better than
    /// re-running a HUD widget's entire multi-stage timeline outside of that widget. **Which exact
    /// sound reads best for this is a judgment call, not yet confirmed by actually hearing it in
    /// game** -- flagged here the same as this mod's other unverified-by-ear choices.
    ///
    /// The message itself uses HUD.TextPrompt.AddMessage -- vanilla's own on-screen text-popup
    /// mechanism (confirmed public, used for things like the "SURVIVOR" title card and shelter
    /// tutorials) -- rather than building a custom UI element, for the same "reuse what vanilla
    /// already has for this" reasoning used throughout this mod.
    /// </summary>
    internal static class ChallengeNotification
    {
        private const int MessageWaitTicks = 0;
        private const int MessageDurationTicks = 100; // ~2.5s at the nominal 40 ticks/second.

        public static void ShowChallengeCompleted(Player player, string challengeDisplayName)
        {
            RainWorldGame game = GimmickGate.GameFor(player);
            RoomCamera[] cameras = game?.cameras;
            HUD.HUD hud = cameras != null && cameras.Length > 0 ? cameras[0].hud : null;
            if (hud == null)
            {
                return;
            }

            hud.PlaySound(SoundID.HUD_Karma_Reinforce_Bump);
            hud.textPrompt?.AddMessage("Challenge complete: " + challengeDisplayName, MessageWaitTicks, MessageDurationTicks, false, false);
        }
    }

    /// <summary>
    /// The base every one of the eventual 50 per-challenge tracking implementations is meant to
    /// build on -- requested directly ("a good abstract class to work with for all of the tracker
    /// implementations") ahead of writing any of them. Deliberately does NOT standardize HOW a
    /// challenge detects its own completion -- the shared mechanisms this was built on top of
    /// (SurvivorEvents, SurvivorRegionMarkers, SurvivorShelterContents, SurvivorCreatureTracking,
    /// SurvivorCreatureVariant, SurvivorTollState, SurvivorCheckpointRun, SurvivorVanillaProgress)
    /// already cover wildly different shapes of detection (an event subscription, a per-tick
    /// check, a read of already-tracked vanilla state), and forcing all of that through one
    /// abstract method would either be so generic it adds nothing, or would fit some challenges
    /// and fight the rest. What IS common to every one of them, and what this class standardizes,
    /// is everything AFTER detection: reporting completion through one shared, correctly-gated
    /// pipeline (persistence, idempotency, the live notification, the cycle-end summary's fresh-
    /// vs-not-fresh distinction) rather than each of the 50 reinventing it.
    ///
    /// A concrete tracker is expected to: implement Register() to subscribe to whatever
    /// SurvivorEvents/mechanism(s) its own detection needs; inside that subscription, check
    /// TryGetEnabledCharacter(player, out characterID) before doing any real work (the same
    /// self-gating discipline every other per-character gimmick patch in this mod already follows
    /// -- this base class can't do that gating FOR a subclass, since it doesn't know when/how the
    /// subclass's own detection runs); and call ReportCompletion once its own condition is
    /// actually met.
    /// </summary>
    internal abstract class ChallengeTracker
    {
        /// <summary>The roster entry (config-safe ID + display name) this tracker is responsible
        /// for -- must match one of CommunityChallengesRegistry.All's own entries.</summary>
        public abstract ChallengeEntry Challenge { get; }

        /// <summary>Called once, at mod startup (ChallengeTrackerRegistry.RegisterAll, itself
        /// called from RainWorldHooks.Postfix alongside this mod's other one-shot setup steps) --
        /// subscribe to whatever SurvivorEvents/mechanism(s) this challenge's own detection needs
        /// here.</summary>
        public abstract void Register();

        /// <summary>The one gate every tracker's own detection code should check before doing any
        /// real work: is `player` on a reserved slot, with the CommunityChallenges GIMMICK itself
        /// assigned to their real character (GimmickGate.IsActiveFor -- this is the same
        /// PrimalSlot+character-resolution+assignment check every other gimmick's patches already
        /// use), AND is this SPECIFIC challenge checked in that character's own settings.
        ///
        /// Both halves matter -- checking only CommunityChallengesSettings.IsChallengeEnabled
        /// would be wrong on its own: per-challenge Configurables are bound eagerly for EVERY real
        /// character (Gotcha #7 -- an unbound Configurable is silently skipped by Remix's own
        /// config-load pass), regardless of whether that character has the CommunityChallenges
        /// gimmick assigned at all, and they default true (see CommunityChallengesSettings.Bind).
        /// So a character who has never touched this gimmick would still read every challenge as
        /// "enabled" by that check alone -- this method's own GimmickGate.IsActiveFor half is what
        /// actually keeps tracking scoped to characters that opted in.</summary>
        protected bool TryGetEnabledCharacter(Player player, out string characterID)
        {
            return GimmickGate.IsActiveFor(player, CommunityChallengesGimmick.ID, out characterID)
                && CommunityChallengesSettings.IsChallengeEnabled(characterID, Challenge.ID);
        }

        /// <summary>Reports this tracker's own challenge as completed for `characterID` -- see
        /// ChallengeCompletion.ReportCompletion for the full contract (idempotent; `isLive`
        /// controls whether the live popup+sound fires).</summary>
        protected void ReportCompletion(Player player, string characterID, bool isLive)
        {
            ChallengeCompletion.ReportCompletion(player, characterID, Challenge.ID, isLive);
        }
    }

    /// <summary>
    /// Where each concrete ChallengeTracker subclass registers itself, once written. Empty as of
    /// this pass -- per explicit request, this pass builds the harness (this registry, the
    /// abstract base, completion/notification/summary plumbing) and NOT any of the 50 trackers
    /// themselves. Add one entry per implemented challenge; RegisterAll() (already wired into
    /// RainWorldHooks.Postfix) picks up everything listed here automatically, no other
    /// integration point needed.
    /// </summary>
    internal static class ChallengeTrackerRegistry
    {
        public static readonly ChallengeTracker[] All = { };

        public static void RegisterAll()
        {
            foreach (ChallengeTracker tracker in All)
            {
                tracker.Register();
            }
        }
    }

    /// <summary>
    /// Adds a "CHALLENGES" button directly above the vanilla CONTINUE button on the cycle-end
    /// screen (Menu.SleepAndDeathScreen) -- explicitly requested placement, distinct from Kill
    /// List's own button (which sits above EXIT instead, one lane over). Same
    /// AddSubObjects/Singal mechanism KillListButtonHook/KillListSignalHook already established
    /// for this screen -- see that pair's own doc comment in CreatureKillListGimmick.cs for why
    /// it's safe (a Postfix here can't interfere with any of the screen's own vanilla signal
    /// handling).
    ///
    /// CONTINUE's own position, confirmed via decompiling Menu/KarmaLadderScreen.cs (the base
    /// class SleepAndDeathScreen extends): `ContinueAndExitButtonsXPos - 180f - SafeScreenOffset.x`,
    /// `Mathf.Max(SafeScreenOffset.y, 15f)` -- this button reuses that same X (its own lane) and
    /// adds +40f to Y, the identical "+40f, stack directly above" offset KillListButtonHook already
    /// uses one lane over (-320f, EXIT's own lane) rather than reading either button's `.pos` back.
    /// </summary>
    [HarmonyPatch(typeof(Menu.SleepAndDeathScreen), nameof(Menu.SleepAndDeathScreen.AddSubObjects))]
    internal static class CommunityChallengesButtonHook
    {
        internal const string SignalText = "PRIMAL_COMMUNITYCHALLENGES";

        private static void Postfix(Menu.SleepAndDeathScreen __instance)
        {
            RainWorld rainWorld = __instance.manager?.rainWorld;
            string characterID = PrimalMenuAccess.GetCharacterID(__instance);
            if (!GimmickGate.IsEnabledForCharacter(rainWorld, characterID, CommunityChallengesGimmick.ID))
            {
                return;
            }

            // Freeze this cycle's own fresh completions for the dialog to read if the player
            // clicks this button -- see ChallengeCompletion's own header comment for why this has
            // to happen HERE (screen build), not lazily on click.
            ChallengeCompletion.SnapshotAndResetForNewScreen(characterID);

            float x = __instance.ContinueAndExitButtonsXPos - 180f - rainWorld.options.SafeScreenOffset.x;
            float y = Mathf.Max(rainWorld.options.SafeScreenOffset.y, 15f) + 40f;

            var button = new Menu.SimpleButton(__instance, __instance.pages[0], "CHALLENGES", SignalText, new Vector2(x, y), new Vector2(110f, 30f));
            __instance.pages[0].subObjects.Add(button);
        }
    }

    [HarmonyPatch(typeof(Menu.SleepAndDeathScreen), nameof(Menu.SleepAndDeathScreen.Singal))]
    internal static class CommunityChallengesSignalHook
    {
        private static void Postfix(Menu.SleepAndDeathScreen __instance, Menu.MenuObject sender, string message)
        {
            if (message != CommunityChallengesButtonHook.SignalText)
            {
                return;
            }

            SaveState saveState = PrimalMenuAccess.GetSaveState(__instance);
            string characterID = saveState?.saveStateNumber?.value;
            if (characterID == null || __instance.manager == null)
            {
                return;
            }

            __instance.manager.ShowDialog(new CommunityChallengesDialog(__instance.manager, characterID, saveState.deathPersistentSaveData));
        }
    }

    /// <summary>
    /// The cycle-end challenge-progress popup. Plain text list (no icons -- unlike KillListDialog,
    /// there's no vanilla sprite per challenge to reuse), built ONLY from challenges currently
    /// selected in this character's own settings (CommunityChallengesSettings.IsChallengeEnabled --
    /// "The list should overall be built from the checked challenges in the settings"), each shown
    /// in one of three styles per explicit request:
    ///  - Completed THIS cycle (ChallengeCompletion.WasCompletedThisCycle) -- bold (MenuLabel's
    ///    own bigText, i.e. Custom.GetDisplayFont() instead of Custom.GetFont() -- confirmed via
    ///    decompiling Menu/MenuLabel.cs to be the actual mechanism behind "bigText", there's no
    ///    separate bold-weight font in this engine) and pure white.
    ///  - Completed on some earlier cycle -- left at MenuLabel's own default (unset) color, i.e.
    ///    "default grey as with all UI elements" is simply not overriding it, the same as every
    ///    other plain label on this mod's own screens.
    ///  - Never completed -- darkened, reusing the exact dimming color (Menu.Menu.MenuRGB
    ///    (Menu.Menu.MenuColors.MediumGrey)) KillListDialog already uses for its own "not yet
    ///    killed" entries.
    ///
    /// Built as a Menu.Dialog subclass, same reasoning as KillListDialog (the full-screen
    /// backdrop/rounded panel/Continue-blocking side-process machinery "for free", see that
    /// class's own doc comment) -- row-major fill (row = i / Columns, col = i % Columns), matching
    /// KillListDialog's own RenderSection fill order, rather than the settings grid's column-major
    /// one (CommunityChallengesSettings.BuildUI): no strict convention either way in this codebase
    /// as of this pass, and row-major is the more directly comparable precedent for another
    /// Menu.Dialog-based roster display.
    ///
    /// **Not yet built or tested in-game** -- no .NET toolchain in this container, and no concrete
    /// ChallengeTracker exists yet to actually populate a "completed" or "completed this cycle"
    /// entry to look at.
    /// </summary>
    internal sealed class CommunityChallengesDialog : Menu.Dialog
    {
        private const int Columns = 5;
        private const float ColumnWidth = 150f;
        private const float RowHeight = 32f;
        private const float Margin = 24f;
        private const float TitleHeight = 34f;
        private const float CloseButtonAreaHeight = 60f;
        private const string CloseSignal = "PRIMAL_COMMUNITYCHALLENGES_CLOSE";

        private readonly string _characterID;
        private readonly DeathPersistentSaveData _saveData;

        public CommunityChallengesDialog(ProcessManager manager, string characterID, DeathPersistentSaveData saveData)
            : base(string.Empty, ComputeSize(characterID), manager)
        {
            _characterID = characterID;
            _saveData = saveData;

            List<ChallengeEntry> enabled = EnabledChallenges(characterID);
            int completedCount = 0;
            foreach (ChallengeEntry entry in enabled)
            {
                if (ChallengeCompletion.IsCompleted(saveData, characterID, entry.ID))
                {
                    completedCount++;
                }
            }

            float cursorY = pos.y + size.y - Margin;
            var title = new Menu.MenuLabel(this, pages[0], $"COMMUNITY CHALLENGES -- {completedCount}/{enabled.Count}", new Vector2(pos.x, cursorY - TitleHeight), new Vector2(size.x, TitleHeight), true);
            pages[0].subObjects.Add(title);
            cursorY -= TitleHeight;

            if (enabled.Count == 0)
            {
                var placeholder = new Menu.MenuLabel(this, pages[0], "(no challenges selected in this character's settings)", new Vector2(pos.x + Margin, cursorY - RowHeight), new Vector2(size.x - Margin * 2f, RowHeight), false);
                placeholder.label.alignment = FLabelAlignment.Left;
                pages[0].subObjects.Add(placeholder);
            }
            else
            {
                for (int i = 0; i < enabled.Count; i++)
                {
                    int row = i / Columns;
                    int col = i % Columns;
                    float cellX = pos.x + Margin + col * ColumnWidth;
                    float cellCenterY = cursorY - row * RowHeight - RowHeight * 0.5f;
                    RenderEntry(enabled[i], cellX, cellCenterY);
                }
            }

            var closeButton = new Menu.SimpleButton(this, pages[0], "CLOSE", CloseSignal, new Vector2(pos.x + (size.x - 110f) * 0.5f, pos.y + Mathf.Max(size.y * 0.02f, 10f)), new Vector2(110f, 30f));
            pages[0].subObjects.Add(closeButton);
        }

        public override void Singal(Menu.MenuObject sender, string message)
        {
            if (message == CloseSignal)
            {
                manager.StopSideProcess(this);
                return;
            }
            base.Singal(sender, message);
        }

        private void RenderEntry(ChallengeEntry challenge, float cellX, float cellCenterY)
        {
            bool completedThisCycle = ChallengeCompletion.WasCompletedThisCycle(_characterID, challenge.ID);
            bool completedEver = completedThisCycle || ChallengeCompletion.IsCompleted(_saveData, _characterID, challenge.ID);

            var label = new Menu.MenuLabel(this, pages[0], challenge.DisplayName, new Vector2(cellX, cellCenterY - 10f), new Vector2(0f, 20f), completedThisCycle);
            label.label.alignment = FLabelAlignment.Left;

            if (completedThisCycle)
            {
                label.label.color = Color.white;
            }
            else if (!completedEver)
            {
                label.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            }
            // else: completed on an earlier cycle -- leave MenuLabel's own default color alone
            // ("default grey as with all UI elements").

            pages[0].subObjects.Add(label);
        }

        private static List<ChallengeEntry> EnabledChallenges(string characterID)
        {
            var enabled = new List<ChallengeEntry>();
            foreach (ChallengeEntry entry in CommunityChallengesRegistry.All)
            {
                if (CommunityChallengesSettings.IsChallengeEnabled(characterID, entry.ID))
                {
                    enabled.Add(entry);
                }
            }
            return enabled;
        }

        private static Vector2 ComputeSize(string characterID)
        {
            int count = EnabledChallenges(characterID).Count;
            int rows = count > 0 ? Mathf.CeilToInt(count / (float)Columns) : 1;

            float width = Columns * ColumnWidth + Margin * 2f;
            float height = Margin * 2f + TitleHeight + CloseButtonAreaHeight + rows * RowHeight;
            return new Vector2(width, height);
        }
    }
}
