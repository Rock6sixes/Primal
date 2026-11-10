using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Friend's own "tame each Lizard type" need: a LIVE "just got tamed" signal, not just a
    /// reactive read of the current `friendTracker.friend` field (which only ever shows the MOST
    /// RECENT tame -- a permanent per-type record needs to know the exact instant a NEW taming
    /// happens, not just whatever's currently true).
    ///
    /// Confirmed via decompiling FriendTracker.cs: `FriendTracker.Update()` is the exact method
    /// that transitions `friend` from null to a real Creature, the moment a relationship crosses
    /// the real taming threshold (`like &gt; 0.5f &amp;&amp; tempLike &gt; 0.5f`). `friendTracker
    /// .friend` itself is already trusted directly elsewhere in this mod (ChainsGimmick.cs,
    /// SleepoverTracker) -- this reuses that same field, just watched for the transition instead
    /// of read reactively.
    ///
    /// A Prefix/Postfix pair using the standard wasNull/isNotNull transition trick (the same shape
    /// as every other "X just happened" detection in this event bus) -- checking `friend != null`
    /// in a Postfix alone would re-fire every single tick the creature stays tamed, not just the
    /// instant it happens.
    /// </summary>
    internal static class SurvivorTaming
    {
        /// <summary>Fires the instant any creature becomes tamed by anyone -- filtered to `friend
        /// is Player` by subscribers that only care about player-caused taming (a creature CAN be
        /// tamed by another creature via the same relationship mechanism, per FriendTracker's own
        /// code, though that's rare outside NPC-companion scenarios this mod doesn't target).</summary>
        public static event System.Action<Creature, Creature> OnCreatureTamed;

        internal static bool HasSubscribers => OnCreatureTamed != null;

        internal static void Raise(Creature tamer, Creature tamed) => OnCreatureTamed?.Invoke(tamer, tamed);
    }

    [HarmonyPatch(typeof(FriendTracker), nameof(FriendTracker.Update))]
    internal static class SurvivorTamingHook
    {
        private static void Prefix(FriendTracker __instance, out bool __state)
        {
            __state = __instance != null && __instance.friend != null;
        }

        private static void Postfix(FriendTracker __instance, bool __state)
        {
            if (__state || !SurvivorTaming.HasSubscribers || __instance?.friend == null)
            {
                // __state true means this creature was already tamed before this call -- not a
                // NEW taming this call caused.
                return;
            }

            Creature tamed = __instance.creature;
            if (tamed != null)
            {
                SurvivorTaming.Raise(__instance.friend, tamed);
            }
        }
    }
}
