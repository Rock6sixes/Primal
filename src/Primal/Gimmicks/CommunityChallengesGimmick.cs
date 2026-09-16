using System.Collections.Generic;
using Menu.Remix.MixedUI;

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
                    false,
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
}
