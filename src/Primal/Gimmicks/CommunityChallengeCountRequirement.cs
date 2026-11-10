using System;
using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// "Community Challenges" count requirement (added 2026-11-10, explicit request): complete at
    /// least N of the 50 community challenges, N set per character in Remix (1-50, default 45).
    /// Survivor's only default requirement; Survivor has no default gimmicks.
    ///
    /// The 50 challenges stay their own individual Requirements (CommunityChallengesRoster). The
    /// difference is how they get tracked: with this requirement assigned, every one of the 50 is
    /// tracked for that character without assigning them one by one --
    /// GimmickAssignments.IsTracked / IsTrackedForAnyCharacter treat a roster ID as active when
    /// this requirement is. The individual challenges are still NOT "assigned", so the
    /// all-requirements-complete check (PrimalPortraitOverride) only waits on this one, not on all
    /// 50.
    ///
    /// Completion is re-checked (a) whenever any roster challenge is satisfied
    /// (RequirementCompletion.OnSatisfied), and (b) on every successful sleep, so lowering N in Remix
    /// mid-run completes it on the next sleep instead of waiting for another challenge.
    /// </summary>
    internal sealed class CommunityChallengeCountRequirement : Requirement
    {
        internal const string RequirementID = "CommunityChallengeCount";

        public override string ID => RequirementID;
        public override string DisplayName => "Community Challenges";

        public override Action<OptionInterface, string> BindSettings => CommunityChallengeCountSettings.Bind;
        public override Func<string, float, List<UIelement>> BuildSettingsUI => CommunityChallengeCountSettings.BuildUI;

        public override void Register()
        {
            RequirementCompletion.OnSatisfied += (player, characterID, requirementID, isLive, saveData) =>
            {
                if (!CommunityChallengesRoster.Contains(requirementID))
                {
                    return;
                }
                if (!GimmickAssignments.IsEnabled(characterID, RequirementID))
                {
                    return;
                }
                CheckAndReport(player, characterID, saveData, isLive);
            };

            SurvivorEvents.OnSuccessfulHibernation += (game, player, eventCharacterID) =>
            {
                if (!TryGetEnabledCharacter(player, out string characterID))
                {
                    return;
                }
                CheckAndReport(player, characterID, PrimalSaveMarkers.SaveDataFor(game), isLive: false);
            };
        }

        /// <summary>How many of the 50 this character has completed on this save.</summary>
        public static int CountCompleted(DeathPersistentSaveData saveData, string characterID)
        {
            int count = 0;
            if (saveData == null || characterID == null)
            {
                return 0;
            }
            foreach (string id in CommunityChallengesRoster.IDs)
            {
                if (RequirementCompletion.IsCompleted(saveData, characterID, id))
                {
                    count++;
                }
            }
            return count;
        }

        private static void CheckAndReport(Player player, string characterID, DeathPersistentSaveData saveData, bool isLive)
        {
            if (saveData == null || RequirementCompletion.IsCompleted(saveData, characterID, RequirementID))
            {
                return;
            }

            int completed = CountCompleted(saveData, characterID);
            int required = CommunityChallengeCountSettings.GetRequiredCount(characterID);
            if (completed < required)
            {
                return;
            }

            Plugin.Log?.LogInfo($"[CommunityChallengeCount] {characterID}: {completed}/{required} community challenges -- requirement complete");
            if (player != null)
            {
                RequirementCompletion.Report(player, characterID, RequirementID, isLive);
            }
            else
            {
                RequirementCompletion.ReportForSave(saveData, characterID, RequirementID);
            }
        }
    }

    /// <summary>Per-character setting for CommunityChallengeCountRequirement: how many of the 50
    /// must be completed. Default 45, range 1-50.</summary>
    internal static class CommunityChallengeCountSettings
    {
        private const int DefaultRequiredCount = 45;
        private const int MinRequiredCount = 1;
        private const int MaxRequiredCount = 50;

        private static readonly Dictionary<string, Configurable<int>> _requiredCount = new Dictionary<string, Configurable<int>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why.
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            _requiredCount[characterID] = owner.config.Bind(
                characterID + "_CommunityChallengeCount_required",
                DefaultRequiredCount,
                new ConfigurableInfo(
                    "How many of the 50 community challenges this character must complete. All 50 are tracked automatically while this requirement is assigned.",
                    new ConfigAcceptableRange<int>(MinRequiredCount, MaxRequiredCount)));
        }

        public static int GetRequiredCount(string characterID)
        {
            return _requiredCount.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultRequiredCount;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            if (!_requiredCount.TryGetValue(characterID, out Configurable<int> requiredCount))
            {
                return new List<UIelement>();
            }

            var items = new List<UIelement>();
            items.Add(new OpLabel(20f, topY, "Challenges required (out of 50):", false));
            items.Add(new OpUpdown(requiredCount, new Vector2(280f, topY - 6f), 80f));
            items.Add(new OpLabel(20f, topY - 40f, "All 50 challenges are tracked automatically -- no need to add them one by one.", false));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
