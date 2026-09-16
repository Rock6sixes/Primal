using System.Collections.Generic;
using HarmonyLib;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared mechanism #5 from COMMUNITY_CHALLENGES.md: Scavenger toll/reputation/payment state --
    /// needed by Heist, Manslaughter, Elder, Usurper, and Criminal. All five turned out to share one
    /// class, ScavengerOutpost (confirmed via decompilation, not guessed), which already tracks
    /// everything these challenges need to read:
    ///
    ///  - `team` (List&lt;Scavenger&gt;) -- the outpost's own guard roster, confirmed public.
    ///  - `worldOutpost.feePayed` (int) -- vanilla's own running payment total for this outpost.
    ///    Confirmed this is the real "has paid enough" signal, not guessed: ScavengerOutpost
    ///    .PlayerTracker.AllowedToPass (vanilla's own gate for whether a player may cross) reads
    ///    exactly `outpost.worldOutpost.feePayed > 9`, so this class reuses that same threshold
    ///    rather than inventing its own.
    ///  - `playerTrackers` (List&lt;PlayerTracker&gt;), each with a `killOrder` bool -- vanilla's
    ///    own per-player hostility flag. Set (when not blocked by a grace-period check) from
    ///    ScavengerReportTransgression(Player), which SurvivorOutpostTransgressionHook (this
    ///    file, below) patches directly rather than trying to infer hostility from anything
    ///    indirect.
    ///
    /// **2026-09-16: the "stole vs. attacked" gap flagged above is closed, plus two more pieces
    /// Heist/Criminal/Manslaughter actually need.** Traced ScavengerReportTransgression's real
    /// call sites (ScavengerAI.cs): both live inside `ScavengerAI.SocialEvent(SocialEventRecognizer
    /// .EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)`, which
    /// receives the specific `EventID` (Theft, NonLethalAttack, LethalAttack, ...) and the actual
    /// `involvedItem` BEFORE deciding whether to call ScavengerReportTransgression at all --
    /// exactly the "what actually happened" detail ScavengerReportTransgression's own signature
    /// throws away. Rather than re-deriving SocialEvent's own `num > 0.5f` reporting threshold
    /// (which mixes relationship state, distance, and item value in ways not worth duplicating),
    /// OnItemStolen (below) is its own independent signal off SocialEvent directly, firing on
    /// every genuine `EventID.Theft` regardless of whether it happened to also cross that
    /// hostility threshold -- for Heist's purposes ("stole a pearl without paying"), the item
    /// changing hands is the actual signal, not how offended the Scavenger got about it.
    ///
    /// Also added: OnTollPassed, a Postfix on Player.NewRoom (the same vanilla hook
    /// SurvivorEvents.OnPlayerEnteredRoom already patches independently -- two unrelated Postfixes
    /// on the same method, no ordering dependency between them) that fires once a player's room
    /// changes away from a toll room they were just in, handing back whether they'd paid
    /// (HasPaidToll) at that moment -- Criminal's own "passed 3 tolls without paying" need.
    /// </summary>
    internal static class SurvivorTollState
    {
        /// <summary>The ScavengerOutpost governing this room, if it's a toll room at all. Room
        /// .updateList (confirmed public property) is the same list every UpdatableAndDeletable in
        /// the room lives in, ScavengerOutpost included.</summary>
        public static ScavengerOutpost OutpostIn(Room room)
        {
            List<UpdatableAndDeletable> updateList = room?.updateList;
            if (updateList == null)
            {
                return null;
            }

            foreach (UpdatableAndDeletable obj in updateList)
            {
                if (obj is ScavengerOutpost outpost)
                {
                    return outpost;
                }
            }
            return null;
        }

        /// <summary>The outpost's own guard roster (Scavenger, the realized creature type) --
        /// Manslaughter/Elder both need to enumerate every guard, including ones replaced mid-
        /// attempt after an original dies (this returns the LIVE list, so a replacement scavenger
        /// added to `team` shows up automatically on the next call, no extra tracking needed).</summary>
        public static IReadOnlyList<Scavenger> GuardsOf(ScavengerOutpost outpost)
        {
            return (IReadOnlyList<Scavenger>)outpost?.team ?? System.Array.Empty<Scavenger>();
        }

        /// <summary>Vanilla's own "has paid enough to be let through" check (see this class's own
        /// header comment for where the `&gt; 9` threshold comes from) -- Heist and Criminal both
        /// need "did NOT pay", i.e. the negation of this.</summary>
        public static bool HasPaidToll(ScavengerOutpost outpost)
        {
            return outpost?.worldOutpost != null && outpost.worldOutpost.feePayed > 9;
        }

        /// <summary>Whether this outpost currently has a standing kill order against this specific
        /// player -- vanilla's own per-player hostility flag, confirmed distinct from a global
        /// "outpost is hostile" state (each player tracked separately, matching how reputation is
        /// genuinely per-player in vanilla, e.g. co-op).</summary>
        public static bool IsHostileToPlayer(ScavengerOutpost outpost, Player player)
        {
            if (outpost?.playerTrackers == null || player?.abstractCreature == null)
            {
                return false;
            }

            foreach (ScavengerOutpost.PlayerTracker tracker in outpost.playerTrackers)
            {
                if (tracker.player == player.abstractCreature)
                {
                    return tracker.killOrder;
                }
            }
            return false;
        }

        /// <summary>Fires whenever an outpost reports a transgression against a specific player --
        /// a Postfix on ScavengerOutpost.ScavengerReportTransgression (SurvivorOutpostTransgression
        /// Hook, below), checked afterwards against IsHostileToPlayer rather than assuming the call
        /// always takes effect, since the original method silently no-ops during the player's own
        /// scavengerImmunity grace period. Handed `becameHostile` precomputed so subscribers don't
        /// each have to re-derive it.</summary>
        public static event System.Action<ScavengerOutpost, Player, bool> OnOutpostTransgression;

        /// <summary>Fires on every genuine item-theft SocialEvent a Scavenger recognizes against a
        /// player -- see this class's own header comment for why this is its own independent
        /// signal off ScavengerAI.SocialEvent rather than piggybacking on
        /// ScavengerReportTransgression. `outpost` is the offended Scavenger's own outpost
        /// assignment (ScavengerAI.outpostModule.outpost, confirmed public) if it's a toll guard at
        /// all -- null for an ordinary wandering Scavenger, since theft-from-a-toll and
        /// theft-from-any-Scavenger are the same underlying vanilla event. `item` is whatever was
        /// actually taken (a DataPearl for Heist's own purposes, but not narrowed here -- narrowing
        /// to a specific item type is each subscriber's own job, the same way TryGetEnabledCharacter
        /// leaves per-challenge specifics to each ChallengeTracker).</summary>
        public static event System.Action<ScavengerOutpost, Player, PhysicalObject> OnItemStolen;

        /// <summary>Fires once a player's room changes away from a room that had a
        /// ScavengerOutpost in it -- Criminal's own "passed a toll" moment. `paid` is
        /// HasPaidToll(outpost) evaluated at the moment of leaving, so a subscriber doesn't need to
        /// separately re-query stale state afterward. Doesn't fire while the player stays in (or
        /// re-enters) the SAME outpost's room -- only on an actual departure to somewhere without
        /// that same outpost.</summary>
        public static event System.Action<ScavengerOutpost, Player, bool> OnTollPassed;

        internal static bool HasOutpostTransgressionSubscribers => OnOutpostTransgression != null;
        internal static bool HasItemStolenSubscribers => OnItemStolen != null;
        internal static bool HasTollPassedSubscribers => OnTollPassed != null;

        internal static void RaiseOutpostTransgression(ScavengerOutpost outpost, Player player, bool becameHostile)
            => OnOutpostTransgression?.Invoke(outpost, player, becameHostile);

        internal static void RaiseItemStolen(ScavengerOutpost outpost, Player player, PhysicalObject item)
            => OnItemStolen?.Invoke(outpost, player, item);

        internal static void RaiseTollPassed(ScavengerOutpost outpost, Player player, bool paid)
            => OnTollPassed?.Invoke(outpost, player, paid);

        // In-memory only, per characterID -- which outpost's room (if any) this character was
        // last known to be in, for SurvivorTollPassageHook to detect a genuine departure from.
        internal static readonly Dictionary<string, ScavengerOutpost> LastOutpostByCharacter =
            new Dictionary<string, ScavengerOutpost>();
    }

    /// <summary>Top-level patch class -- see SurvivorEvents.cs's header comment for why every patch
    /// in this Survivor-mechanisms batch is a top-level class rather than nested inside the static
    /// helper it serves.</summary>
    [HarmonyPatch(typeof(ScavengerOutpost), nameof(ScavengerOutpost.ScavengerReportTransgression))]
    internal static class SurvivorOutpostTransgressionHook
    {
        private static void Postfix(ScavengerOutpost __instance, Player player)
        {
            if (!SurvivorTollState.HasOutpostTransgressionSubscribers || __instance == null || player == null)
            {
                return;
            }
            if (!GimmickGate.TryGetCharacter(player, out _))
            {
                // Cheap-ish gate (one PrimalSlot check via TryGetCharacter) -- transgressions are
                // rare events, not a hot path, so this doesn't need GimmickGate's Player-overload
                // gymnastics, just the same "only on a reserved slot" rule as everything else in
                // this file.
                return;
            }
            SurvivorTollState.RaiseOutpostTransgression(__instance, player, SurvivorTollState.IsHostileToPlayer(__instance, player));
        }
    }

    /// <summary>See SurvivorTollState.OnItemStolen's own doc comment for the full trace of why
    /// this patches ScavengerAI.SocialEvent directly rather than ScavengerReportTransgression.</summary>
    [HarmonyPatch(typeof(ScavengerAI), nameof(ScavengerAI.SocialEvent))]
    internal static class SurvivorItemStolenHook
    {
        private static void Postfix(ScavengerAI __instance, SocialEventRecognizer.EventID ID, Creature subjectCrit, PhysicalObject involvedItem)
        {
            if (!SurvivorTollState.HasItemStolenSubscribers
                || ID != SocialEventRecognizer.EventID.Theft
                || involvedItem == null
                || !(subjectCrit is Player player))
            {
                return;
            }
            if (!GimmickGate.TryGetCharacter(player, out _))
            {
                return;
            }

            ScavengerOutpost outpost = __instance.outpostModule?.outpost;
            SurvivorTollState.RaiseItemStolen(outpost, player, involvedItem);
        }
    }

    /// <summary>See SurvivorTollState.OnTollPassed's own doc comment. An independent Postfix on
    /// Player.NewRoom -- SurvivorEvents.OnPlayerEnteredRoom already patches this same method
    /// separately, but that event only hands back plain name strings, not the Room object
    /// SurvivorTollState.OutpostIn needs, so this keeps its own dedicated hook rather than
    /// reshaping that already-documented event's signature for one consumer.</summary>
    [HarmonyPatch(typeof(Player), nameof(Player.NewRoom))]
    internal static class SurvivorTollPassageHook
    {
        private static void Postfix(Player __instance, Room newRoom)
        {
            if (newRoom == null || !GimmickGate.TryGetCharacter(__instance, out string characterID))
            {
                return;
            }

            ScavengerOutpost currentOutpost = SurvivorTollState.OutpostIn(newRoom);

            if (SurvivorTollState.LastOutpostByCharacter.TryGetValue(characterID, out ScavengerOutpost previous)
                && previous != null && previous != currentOutpost)
            {
                if (SurvivorTollState.HasTollPassedSubscribers)
                {
                    SurvivorTollState.RaiseTollPassed(previous, __instance, SurvivorTollState.HasPaidToll(previous));
                }
            }

            if (currentOutpost != null)
            {
                SurvivorTollState.LastOutpostByCharacter[characterID] = currentOutpost;
            }
            else
            {
                SurvivorTollState.LastOutpostByCharacter.Remove(characterID);
            }
        }
    }
}
