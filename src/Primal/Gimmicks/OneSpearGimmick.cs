using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using UnityEngine;

namespace Primal.Gimmicks
{
    // The "OneSpear" gimmick (shown in the UI as "Master Spear" -- see GimmickRegistry.cs; the
    // internal ID stayed "OneSpear" to avoid disturbing existing default assignments and the save
    // markers below, which are keyed off it): grants vanilla Spearmaster's real spear-pull to
    // whichever character it's assigned to, but usable only a limited number of times per save
    // file, ever (default 1, configurable per character -- see OneSpearSettings). Assigned to real
    // vanilla Spearmaster himself, it's a challenge modifier (his usually-unlimited pulls become
    // just N for the whole game); assigned to any other character, it's what grants them the
    // mechanic in the first place, since none of them have it by default.
    //
    // Background: vanilla Spearmaster's actual means of getting a weapon is a mechanic no other
    // character has: hold pickup with a free hand (Player.Update(), guarded by "SlugCatClass ==
    // MoreSlugcatsEnums.SlugcatStatsName.Spear") and a spear visibly grows out of his tail
    // (PlayerGraphics.TailSpeckles.spearProg counts 0..1 while charging, driven entirely through
    // TailSpeckles.setSpearProgress); once it hits exactly 1f, Player.Update() spawns a real
    // AbstractSpear and puts it in his hand.
    //
    // That whole mechanic is gated on the literal vanilla identity SlugCatClass ==
    // SlugcatStatsName.Spear, so it doesn't fire for any other character by default.
    // SpearIdentity/its two hooks below grant it, using the same SlugCatClass-flip trick as
    // SlugpupGimmick (see that file for why flipping this one field is enough to light up dozens
    // of hardcoded vanilla checks at once, and the tradeoff around vanilla's own diet lookups
    // also keying off SlugCatClass -- here that mostly just means the character's own diet
    // calculation gets swapped out for vanilla Spearmaster's native one (all-negative from
    // corpses, meat and plants alike) for as long as the flip is in effect).
    //
    // OneSpearGimmick itself is the actual nerf: it Prefixes setSpearProgress -- the only
    // setter for spearProg, and the sole choke point every completion (whether reached by the
    // normal per-tick Lerp toward 1, or the explicit "snap to exactly 1f once close" call) has
    // to pass through -- and once this character has used up their configured limit of lifetime
    // pulls, clamps the incoming value below 1 so spearProg can never hit exactly 1f again. Each
    // completion moment doubles as the write: every time a call arrives at (essentially) 1, before
    // blocking anything, it permanently records one more use against this save file, up to the
    // configured limit.

    /// <summary>
    /// Applies the SlugCatClass flip for the OneSpear gimmick. Two separate patches call into
    /// this, not just one:
    ///
    ///  - a Postfix on GetInitialSlugcatClass, the vanilla method that assigns SlugCatClass in
    ///    the first place (early in Player's own constructor) -- this is the "obvious" spot, and
    ///    is what everything else that reads SlugCatClass during normal gameplay relies on.
    ///  - a Prefix on InitiateGraphicsModule, called later (separately, once the creature is
    ///    actually realized) to build the player's PlayerGraphics. PlayerGraphics's own
    ///    constructor only allocates TailSpeckles (the tail-charge sprites the whole spear-pull
    ///    depends on) when it sees SlugCatClass == Spear *at that exact moment*; if that check
    ///    ran even one tick before GetInitialSlugcatClass's postfix had taken effect,
    ///    TailSpeckles would be permanently null for that PlayerGraphics instance (it's never
    ///    retroactively created later), and Player.Update()'s tail-charge code -- which only
    ///    runs while holding pickup -- would throw a null reference every tick the button is
    ///    held. The game's per-creature update loop swallows that quietly rather than crashing:
    ///    the creature just stops responding to input entirely (no movement, holding grab does
    ///    nothing) with nothing obviously wrong in BepInEx's own log. Re-applying the flip
    ///    immediately before InitiateGraphicsModule runs closes that timing gap for good,
    ///    regardless of whichever order the two methods actually run in for a given spawn.
    /// </summary>
    internal static class SpearIdentity
    {
        public static void ApplyIfEnabled(Player player, string caller)
        {
            RainWorld rainWorld = player?.abstractCreature?.world?.game?.rainWorld;
            if (player?.playerState == null || rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            string characterID = player.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, OneSpearGimmick.ID))
            {
                return;
            }

            bool changed = player.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear;
            player.SlugCatClass = MoreSlugcatsEnums.SlugcatStatsName.Spear;

            if (changed)
            {
                Plugin.Log?.LogInfo($"[OneSpear] {characterID}: SlugCatClass -> Spear (via {caller})");
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetInitialSlugcatClass))]
    internal static class SpearIdentityHook
    {
        private static void Postfix(Player __instance)
        {
            SpearIdentity.ApplyIfEnabled(__instance, nameof(Player.GetInitialSlugcatClass));
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.InitiateGraphicsModule))]
    internal static class SpearIdentityGraphicsHook
    {
        private static void Prefix(Player __instance)
        {
            SpearIdentity.ApplyIfEnabled(__instance, nameof(Player.InitiateGraphicsModule));
        }
    }

    [HarmonyPatch(typeof(PlayerGraphics.TailSpeckles), nameof(PlayerGraphics.TailSpeckles.setSpearProgress))]
    internal static class OneSpearGimmick
    {
        public const string ID = "OneSpear";

        // A save key that will never match any of DeathPersistentSaveData.FromString's known
        // "<dpB>"-delimited keys, so it always lands (and round-trips forever after) in that
        // class's own designed-for-this unrecognizedSaveStrings passthrough list -- see
        // DeathPersistentSaveData.SaveToString/FromString. One marker PER USE per character (see
        // UseMarker below), so uses are tracked independently per character, not shared mod-wide,
        // and the count of them already present is how many pulls this save file has spent.
        //
        // NOTE: earlier versions of this gimmick used a single un-indexed marker
        // ("ALTCHAR_ONESPEAR<dpB><characterID>") for the old hardcoded "exactly one pull, ever"
        // behavior. That marker is no longer written or read -- a save file that had already used
        // its one pull under the old scheme will show as having 0 uses under this one. Not worth
        // migrating for a mod still in active development with no real save files at stake.
        private const string SaveMarkerKey = "ALTCHAR_ONESPEAR";

        private static string UseMarker(string characterID, int useIndex)
        {
            return SaveMarkerKey + "<dpB>" + characterID + "<dpB>" + useIndex;
        }

        private static int CountUsesSoFar(List<string> unrecognizedSaveStrings, string characterID)
        {
            string prefix = SaveMarkerKey + "<dpB>" + characterID + "<dpB>";
            int count = 0;
            foreach (string entry in unrecognizedSaveStrings)
            {
                if (entry != null && entry.StartsWith(prefix))
                {
                    count++;
                }
            }
            return count;
        }

        // setSpearProgress clamps its argument into [0, 1] internally; we only ever care about
        // calls that are trying to reach (functionally) the top of that range.
        private const float CompletionThreshold = 0.999f;

        // What we clamp a blocked completion attempt down to -- comfortably below 1f (so
        // Player.Update()'s "if (spearProg == 1f)" spear-creation check never fires) but high
        // enough that the tail visibly stays mid-charge rather than snapping back down, so a
        // spent pull reads as "this fizzled" rather than "nothing happened".
        private const float BlockedCeiling = 0.99f;

        // PlayerGraphics.player is a non-public field. The compile-time reference assembly this
        // mod builds against is a "publicized" copy with accessibility stripped, so direct field
        // access (`__instance.pGraphics.player`) compiles cleanly -- but the real game assembly
        // installed on a player's machine still enforces the original (non-public) accessibility,
        // and Harmony/MonoMod JITs this patch against THAT assembly. The result was a
        // FieldAccessException thrown every tick this Prefix ran (i.e. every tick while charging
        // a spear-pull past ~0.999), which the game's per-creature update loop swallows silently
        // -- surfacing as "the character just stops responding to input" with nothing obviously
        // wrong in BepInEx's own log (confirmed via Unity's exceptionLog.txt). Reflection sidesteps
        // the JIT-level accessibility check entirely, so it works against both the publicized
        // reference assembly and the real one.
        private static readonly FieldInfo PlayerGraphicsPlayerField =
            AccessTools.Field(typeof(PlayerGraphics), "player");

        private static void Prefix(PlayerGraphics.TailSpeckles __instance, ref float p)
        {
            if (p < CompletionThreshold)
            {
                return;
            }

            PlayerGraphics pGraphics = __instance?.pGraphics;
            Player player = pGraphics != null ? PlayerGraphicsPlayerField.GetValue(pGraphics) as Player : null;
            if (player?.playerState == null || player.room?.game == null || !player.room.game.IsStorySession)
            {
                return;
            }

            if (!PrimalSlot.IsActive(player.room.game.rainWorld))
            {
                return;
            }

            string characterID = player.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            DeathPersistentSaveData saveData = player.room.game.GetStorySession.saveState.deathPersistentSaveData;
            int limit = OneSpearSettings.GetLimit(characterID);
            int usesSoFar = CountUsesSoFar(saveData.unrecognizedSaveStrings, characterID);

            if (usesSoFar >= limit)
            {
                // Already spent this character's configured allotment -- never let it complete again.
                p = BlockedCeiling;
                return;
            }

            // A pull is still available: let this completion through untouched (it'll finish
            // this tick and Player.Update() will spawn the spear), and permanently record that
            // it happened, indexed so CountUsesSoFar can tell how many have been spent so far.
            saveData.unrecognizedSaveStrings.Add(UseMarker(characterID, usesSoFar));
            Plugin.Log?.LogInfo($"[OneSpear] {characterID}: pull #{usesSoFar + 1}/{limit} used");
        }
    }

    /// <summary>Per-character settings for OneSpearGimmick: how many lifetime pulls this
    /// character gets (default 1, matching the gimmick's original hardcoded behavior). See
    /// GimmickDef.cs for how BindSettings/BuildSettingsUI plug into PrimalOptions's
    /// drill-down menu.</summary>
    internal static class OneSpearSettings
    {
        private const int DefaultLimit = 1;
        private const int MinLimit = 1;
        private const int MaxLimit = 50;

        private static readonly Dictionary<string, Configurable<int>> _limit = new Dictionary<string, Configurable<int>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why (Configurable<T> can only ever be bound to one
        // UIconfig element at a time; building a fresh OpUpdown every time this sub-view opened
        // threw Menu.Remix.MultiuseConfigurableException the second time around).
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            _limit[characterID] = owner.config.Bind(
                characterID + "_OneSpear_limit",
                DefaultLimit,
                new ConfigurableInfo("How many spears this character can ever pull, for the lifetime of a save file.", new ConfigAcceptableRange<int>(MinLimit, MaxLimit)));
        }

        public static int GetLimit(string characterID)
        {
            return _limit.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultLimit;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            var items = new List<UIelement>();
            if (!_limit.TryGetValue(characterID, out Configurable<int> limit))
            {
                // Not bound yet -- don't cache an incomplete/empty result.
                return items;
            }

            items.Add(new OpLabel(20f, topY, "Lifetime spear pulls allowed:", false));
            items.Add(new OpUpdown(limit, new Vector2(220f, topY - 6f), 80f));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
