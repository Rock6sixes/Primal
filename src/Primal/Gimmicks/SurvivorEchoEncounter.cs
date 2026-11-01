using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Devotee's own need for a LIVE "just met this Echo" moment, not just a reactive read of the
    /// permanent SaveState.ghostsTalkedTo flag (SurvivorVanillaProgress.HasMetGhost). A reactive
    /// check alone can't tell an Echo met long ago in an unrelated attempt apart from one met just
    /// now as part of a specific same-shelter chain -- the underlying save flag has no timestamp or
    /// shelter tied to it, and once set it never resets. A Postfix on SaveState.GhostEncounter
    /// (confirmed via decompilation to unconditionally set
    /// `deathPersistentSaveData.ghostsTalkedTo[ghost] = 2` right at the top of its own body -- the
    /// exact moment a real encounter registers) gives the live signal instead, so DevoteeTracker
    /// can credit an encounter only to whichever same-shelter attempt is currently active for that
    /// character.
    ///
    /// Gated minimally at the source (just "is this a real character ID at all") rather than a
    /// full PrimalSlot check -- SaveState doesn't carry a live RainWorld reference to check
    /// against here, and the one real consumer (DevoteeTracker) already re-gates properly via
    /// TryGetEnabledCharacter against a live Player before doing anything with this, so an
    /// ungated raise on a non-Primal save costs nothing beyond a no-op lookup there.
    /// </summary>
    internal static class SurvivorEchoEncounter
    {
        public static event System.Action<string, GhostWorldPresence.GhostID> OnGhostEncountered;

        internal static bool HasSubscribers => OnGhostEncountered != null;

        internal static void Raise(string characterID, GhostWorldPresence.GhostID ghost)
            => OnGhostEncountered?.Invoke(characterID, ghost);
    }

    [HarmonyPatch(typeof(SaveState), nameof(SaveState.GhostEncounter))]
    internal static class SurvivorEchoEncounterHook
    {
        private static void Postfix(SaveState __instance, GhostWorldPresence.GhostID ghost)
        {
            if (!SurvivorEchoEncounter.HasSubscribers)
            {
                return;
            }

            string characterID = __instance?.saveStateNumber?.value;
            if (characterID != null)
            {
                SurvivorEchoEncounter.Raise(characterID, ghost);
            }
        }
    }
}
