namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared mechanism #8 from COMMUNITY_CHALLENGES.md: several challenges turn out to just need a
    /// read of state vanilla already tracks itself, rather than any new tracking logic. Four such
    /// reads, each confirmed via decompilation:
    ///
    ///  - **Collector** (all Sandbox unlocks) -- PlayerProgression.miscProgressionData
    ///    .GetTokenCollected(SandboxUnlockID), the same method vanilla's own Sandbox unlock-popup
    ///    logic calls. HasSandboxUnlock below is a direct passthrough.
    ///  - **Passages** (all Steam achievements) -- RainWorld.AchievementAlreadyDisplayed
    ///    (AchievementID), which despite its name is vanilla's own real completion check (it calls
    ///    through to processManager.mySteamManager.HasAchievement). Steam-only for now -- GOG has
    ///    a separate AchievementAlreadyDisplayedGOG method this doesn't call, and there's a third,
    ///    fully offline path (the local `Achievements` class) neither covers; deferred since the
    ///    user's own deployment is a Steam install (see AI_CONTEXT.md's "Deployment layout").
    ///  - **Devotee** ("visited all 6 Echoes") -- DeathPersistentSaveData.ghostsTalkedTo, a
    ///    Dictionary&lt;GhostWorldPresence.GhostID, int&gt; vanilla itself writes to (SaveState
    ///    .GhostEncounter sets `[ghost] = 2`) the moment a player actually meets one. HasMetGhost
    ///    below treats presence-with-value-2 as "met" -- the exact value SaveState.GhostEncounter
    ///    itself writes -- rather than guessing at what a lower value might mean.
    ///  - **Massacre/Outlaw/Ranger** (kill-point scoring) -- the "creature point values" table these
    ///    challenges' own notes reference is vanilla's own Sandbox/Arena kill-score table
    ///    (Menu.SandboxSettingsInterface.DefaultKillScores), indexed by MultiplayerUnlocks
    ///    .SandboxUnlockID. KillPointValue below reconstructs the right SandboxUnlockID from a
    ///    CreatureTemplate.Type by round-tripping its string name (SandboxUnlockID's own registered
    ///    names match creature type names 1:1 for every creature checked so far -- "GreenLizard",
    ///    "PinkLizard", etc. -- the same ExtEnum-round-trip-by-string-value pattern
    ///    CreatureKillListGimmick's own KillListEntry already uses) rather than hand-copying the
    ///    whole table as a second, driftable copy of vanilla's own data. **Unconfirmed:** whether a
    ///    Story-mode Hunter run's own in-game score display reads this exact table, or a separate
    ///    Hunter-specific one -- Outlaw/Massacre only need OUR OWN point tally to reach the
    ///    thresholds their community rules define, which this table is explicitly designed for
    ///    regardless of what vanilla's own Hunter score screen happens to use internally.
    /// </summary>
    internal static class SurvivorVanillaProgress
    {
        public static bool HasSandboxUnlock(RainWorld rainWorld, MultiplayerUnlocks.SandboxUnlockID unlock)
        {
            return rainWorld?.progression?.miscProgressionData?.GetTokenCollected(unlock) ?? false;
        }

        public static bool HasSteamAchievement(RainWorld rainWorld, RainWorld.AchievementID achievement)
        {
            return rainWorld != null && rainWorld.AchievementAlreadyDisplayed(achievement);
        }

        /// <summary>True only if vanilla's own GhostEncounter has actually run for this Echo (value
        /// 2 -- see this class's own header comment for why that specific value, not just "key
        /// present").</summary>
        public static bool HasMetGhost(DeathPersistentSaveData saveData, GhostWorldPresence.GhostID ghost)
        {
            return saveData?.ghostsTalkedTo != null
                && saveData.ghostsTalkedTo.TryGetValue(ghost, out int value)
                && value >= 2;
        }

        // Generously sized rather than precisely sized to MultiplayerUnlocks.SandboxUnlockID's own
        // registered count (which this file doesn't have a confirmed way to read directly) --
        // DefaultKillScores only ever indexes into the low double digits as of this pass, so 256
        // has ample headroom, and an out-of-range index would throw during this array's own one-
        // time construction rather than silently corrupting anything.
        private static int[] _killScores;

        private static int[] KillScores()
        {
            if (_killScores == null)
            {
                int[] scores = new int[256];
                Menu.SandboxSettingsInterface.DefaultKillScores(ref scores);
                _killScores = scores;
            }
            return _killScores;
        }

        /// <summary>Vanilla's own Sandbox/Arena kill-point value for this creature type, or null if
        /// it doesn't have a matching SandboxUnlockID (some CreatureTemplate.Type values -- non-
        /// creature or otherwise-unscored ones -- may not round-trip; see this class's own header
        /// comment on the round-trip assumption).</summary>
        public static int? KillPointValue(CreatureTemplate.Type type)
        {
            if (type?.value == null)
            {
                return null;
            }

            // register: false -- confirmed via ExtEnum's own base constructor that this leaves
            // `index` at -1, with no exception, when `type.value` doesn't match an already-
            // registered SandboxUnlockID name, rather than registering a bogus new one just to
            // look it up.
            var unlockID = new MultiplayerUnlocks.SandboxUnlockID(type.value, register: false);
            int index = (int)unlockID;
            int[] scores = KillScores();
            return index >= 0 && index < scores.Length ? scores[index] : (int?)null;
        }
    }
}
