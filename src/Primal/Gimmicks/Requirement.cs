using System;
using System.Collections.Generic;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Added 2026-09-17, generalizing what used to be ChallengeTracker/ChallengeTrackerRegistry/
    /// ChallengeCompletion (all three folded into this file's own Requirement/RequirementRegistry/
    /// RequirementCompletion) -- per the user's own design discussion: tracking mechanics (the
    /// SurvivorEvents/PrimalKillTracker/etc. shared mechanisms) should stay global and gimmick-
    /// agnostic, "gameplay altering" or "adds a UI surface" gimmicks (Misery, Slugpup, Chains, Kill
    /// List) should stay focused on their own one job, and what sits between them -- "a named,
    /// persistent, per-character condition that becomes true and stays true" -- deserves to be its
    /// own layer instead of being hardwired to one specific gimmick (Community Challenges).
    ///
    /// **Why this used to be Community-Challenges-specific, and why that was wrong.** The original
    /// design required a character to have the CommunityChallenges gimmick assigned AND the
    /// specific challenge checked in that gimmick's own 50-checkbox settings sub-view -- two nested
    /// layers for what's conceptually one choice ("track this for this character"). That shape
    /// only existed because Requirements were invented as part of building Community Challenges,
    /// not because a real gimmick-gating relationship exists between them. A Requirement someone
    /// wants for a reason OTHER than the Community Challenges scoreboard (e.g. a future MasterSpear
    /// gimmick gating its own effect on "has this character reached a given story% or died zero
    /// times this run") has nothing to do with Community Challenges at all, so it shouldn't need
    /// that gimmick assigned first.
    ///
    /// **The fix: a Requirement is directly, individually enabled per character**, through the
    /// exact same (characterID, ID) -&gt; Configurable&lt;bool&gt; store gimmick assignment already
    /// uses (GimmickAssignments -- genuinely generic under the hood despite its name; a requirement
    /// ID and a gimmick ID are both just strings to it) -- see PrimalOptions.cs's own "Requirements"
    /// tab, a sibling to its existing "Gimmicks" tab, for where a player actually flips these. This
    /// also means GimmickGate.IsActiveFor/IsEnabledForCharacter (already fully generic over any ID
    /// string) work as this class's own gate with no new plumbing -- see TryGetEnabledCharacter
    /// below.
    ///
    /// **Community Challenges is now a CONSUMER, not the owner.** CommunityChallengesGimmick.cs
    /// keeps the cycle-end CHALLENGES button and its summary dialog, but they now just curate a
    /// fixed list of 50 Requirement IDs and read RequirementCompletion's own state for them --
    /// exactly the same relationship a future "Achievements" screen, or a future MasterSpear gate,
    /// would have to any OTHER Requirement. Nothing about detection, persistence, or the
    /// satisfied/not-satisfied model is specific to Community Challenges anymore.
    ///
    /// **Many consumers can watch one Requirement.** RequirementCompletion.OnSatisfied is a plain
    /// multicast event -- Community Challenges' own popup is one subscriber; a future MasterSpear
    /// gimmick reacting the instant its own prerequisite becomes true would be another, entirely
    /// independent subscriber to the exact same event for a different ID. Nothing about this class
    /// assumes there's only ever one interested party per Requirement.
    ///
    /// **What did NOT change:** every concrete Requirement (see Requirements.cs, renamed from
    /// ChallengeTrackers.cs) still just subscribes to whatever SurvivorEvents/PrimalKillTracker/
    /// etc. shared mechanism its own detection needs, the same as before -- this class deliberately
    /// still does not standardize HOW a Requirement detects its own completion, only what happens
    /// once it does.
    /// </summary>
    internal abstract class Requirement
    {
        /// <summary>The stable, config-safe ID this requirement is known by -- used as the
        /// GimmickAssignments key, the RequirementCompletion persistence key, and (by default) the
        /// display name. Must be unique across every registered Requirement.</summary>
        public abstract string ID { get; }

        /// <summary>The human-readable name shown in the Requirements tab and the Community
        /// Challenges summary dialog. Defaults to ID -- override only when the two genuinely need
        /// to differ (e.g. UTurnRequirement's ID "UTurn" vs. the community's own "U-Turn"
        /// spelling, which isn't valid in a bare identifier-derived config key).</summary>
        public virtual string DisplayName => ID;

        /// <summary>Called once, at mod startup (RequirementRegistry.RegisterAll, called from
        /// RainWorldHooks.Postfix alongside this mod's other one-shot setup steps) -- subscribe to
        /// whatever SurvivorEvents/mechanism(s) this requirement's own detection needs here.</summary>
        public abstract void Register();

        /// <summary>The one gate every requirement's own detection code should check before doing
        /// any real work: is `player` on a reserved slot, with THIS requirement directly enabled
        /// for their real character. GimmickAssignments.IsEnabledForAnyCharacter runs first (see
        /// that method's own doc comment) so the common "nobody's tracking this" case costs one
        /// dictionary lookup instead of running GimmickGate's own per-player resolution for
        /// nothing.</summary>
        protected bool TryGetEnabledCharacter(Player player, out string characterID)
        {
            characterID = null;
            return GimmickAssignments.IsEnabledForAnyCharacter(ID) && GimmickGate.IsActiveFor(player, ID, out characterID);
        }

        /// <summary>Reports this requirement as satisfied for `characterID` -- see
        /// RequirementCompletion.Report for the full contract (idempotent; `isLive` controls
        /// whether subscribers treat this as a live, mid-cycle detection).</summary>
        protected void ReportCompletion(Player player, string characterID, bool isLive)
        {
            RequirementCompletion.Report(player, characterID, ID, isLive);
        }

        /// <summary>The menu-context counterpart to ReportCompletion, for a requirement whose own
        /// detection point has no live Player (see OutlawRequirement). Always reports non-live --
        /// there's no HUD to show a popup on at a menu screen.</summary>
        protected void ReportCompletionForSave(DeathPersistentSaveData saveData, string characterID)
        {
            RequirementCompletion.ReportForSave(saveData, characterID, ID);
        }
    }

    /// <summary>
    /// Where each concrete Requirement subclass registers itself. Add one entry per implemented
    /// requirement (see Requirements.cs); RegisterAll() (already wired into RainWorldHooks.Postfix)
    /// picks up everything listed here automatically, no other integration point needed.
    /// </summary>
    internal static class RequirementRegistry
    {
        public static readonly Requirement[] All =
        {
            new DescensionRequirement(),
            new RangerRequirement(),
            new SpearfisherRequirement(),
            new FoeRequirement(),
            new MassacreRequirement(),
            new MotherRequirement(),
            new SleepoverRequirement(),
            new PassagesRequirement(),
            new TastyRequirement(),
            new LeukocyteRequirement(),
            new VisionsRequirement(),
            new MycologistRequirement(),
            new UsurperRequirement(),
            new PerseveranceRequirement(),
            new FeastRequirement(),
            new SafariRequirement(),
            new PilgrimageRequirement(),
            new ChefRequirement(),
            new OfferingRequirement(),
            new OrnamentsRequirement(),
            new CollectorRequirement(),
            new DevoteeRequirement(),
            new MartyrRequirement(),
            new GuardianRequirement(),
            new DespicableRequirement(),
            new EnlightenmentRequirement(),
            new FlytrapRequirement(),
            new ManslaughterRequirement(),
            new ChieftainRequirement(),
            new CriminalRequirement(),
            new NomadRequirement(),
            new CircuitRequirement(),
            new DesperadoRequirement(),
            new UTurnRequirement(),
            new SymbioteRequirement(),
            new ChargedRequirement(),
            new CatalogueRequirement(),
            new AscensionRequirement(),
            new FriendRequirement(),
            new ScholarRequirement(),
            new ElderRequirement(),
            new HeistRequirement(),
            new TriplopiaRequirement(),
            new OutlawRequirement(),
            new JourneyRequirement(),
            new VisitorRequirement(),
            new FishtankRequirement(),
            new VeganRequirement(),
            new SaintRequirement(),
            new BroadcastRequirement(),
        };

        private static bool _registered;

        /// <summary>Idempotent, matching SurvivorRegionMarkers.Init()'s own guard and for the same
        /// reason: without it, RainWorld.OnModsInit firing more than once would double-subscribe
        /// every requirement's own event handler.</summary>
        public static void RegisterAll()
        {
            if (_registered)
            {
                return;
            }
            _registered = true;

            foreach (Requirement requirement in All)
            {
                requirement.Register();
            }
        }

        /// <summary>Looks up one registered requirement by its ID -- null if `id` doesn't match
        /// anything here, which would only ever mean a typo somewhere.</summary>
        public static Requirement Find(string id)
        {
            foreach (Requirement requirement in All)
            {
                if (requirement.ID == id)
                {
                    return requirement;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Persists which (character, requirement) pairs have ever been satisfied, and tracks which of
    /// those are "fresh" -- new since some consumer last checked -- generalized from
    /// ChallengeCompletion (see that class's own former header comment, preserved in git history,
    /// for the original two-set "why" this still follows).
    ///
    /// Renamed persistence key (PrimalRequirementCompleted, not the old SurvivorChallengeCompleted)
    /// -- a deliberate, clean break rather than a compatibility shim: nothing has shipped yet (no
    /// real save file anywhere has the old key written to it), so there's no data to preserve
    /// compatibility with, and carrying the old, now-inaccurate name forward would cost real
    /// clarity for zero benefit.
    /// </summary>
    internal static class RequirementCompletion
    {
        private const string CompletedKey = "PrimalRequirementCompleted";

        private static readonly Dictionary<string, HashSet<string>> _accumulating =
            new Dictionary<string, HashSet<string>>();

        private static readonly Dictionary<string, HashSet<string>> _lastCheckSnapshot =
            new Dictionary<string, HashSet<string>>();

        /// <summary>Fires the first time (ever, on this save) `requirementID` is satisfied for
        /// `characterID` -- never again after that (RecordCompletion's own idempotency guards
        /// this). `player` is null for a ReportForSave-driven completion (no live Player exists at
        /// that detection point) -- subscribers that need a Player for their own reaction (e.g. a
        /// live popup) should treat `isLive` as the signal for whether one is actually available,
        /// not just check `player != null` on its own. Any number of independent consumers may
        /// subscribe to the same requirement ID -- Community Challenges' own popup is one; nothing
        /// here assumes it's the only one.</summary>
        public static event Action<Player, string, string, bool> OnSatisfied;

        /// <summary>Reports `requirementID` as satisfied for `characterID`. `isLive` says whether
        /// this was detected mid-cycle during active gameplay (true) or only confirmable at the
        /// exact moment a cycle ended (false) -- subscribers decide what, if anything, to do
        /// differently for each (e.g. Community Challenges only plays its popup+sound for a live
        /// completion).</summary>
        public static void Report(Player player, string characterID, string requirementID, bool isLive)
        {
            if (characterID == null || requirementID == null)
            {
                return;
            }

            DeathPersistentSaveData saveData = PrimalSaveMarkers.SaveDataFor(GimmickGate.GameFor(player));
            if (!RecordCompletion(saveData, characterID, requirementID))
            {
                return;
            }

            OnSatisfied?.Invoke(player, characterID, requirementID, isLive);
        }

        /// <summary>The menu-context counterpart to Report, for a requirement whose own detection
        /// point has no live Player or RainWorldGame (see OutlawRequirement's own doc comment).
        /// Always raises OnSatisfied with isLive: false and a null Player.</summary>
        public static void ReportForSave(DeathPersistentSaveData saveData, string characterID, string requirementID)
        {
            if (characterID == null || requirementID == null || !RecordCompletion(saveData, characterID, requirementID))
            {
                return;
            }

            OnSatisfied?.Invoke(null, characterID, requirementID, false);
        }

        /// <summary>The part shared by both overloads above: idempotently persist the marker and
        /// update the live accumulator. Returns true only the first time this requirement is ever
        /// satisfied by this character on this save.</summary>
        private static bool RecordCompletion(DeathPersistentSaveData saveData, string characterID, string requirementID)
        {
            if (saveData == null)
            {
                return false;
            }

            bool firstTimeEver = PrimalSaveMarkers.Add(
                saveData,
                PrimalSaveMarkers.Marker(CompletedKey, characterID, requirementID));
            if (!firstTimeEver)
            {
                return false;
            }

            GetOrCreate(_accumulating, characterID).Add(requirementID);
            return true;
        }

        /// <summary>Has this character ever satisfied this requirement, on this save file?</summary>
        public static bool IsCompleted(DeathPersistentSaveData saveData, string characterID, string requirementID)
        {
            return PrimalSaveMarkers.Has(saveData, PrimalSaveMarkers.Marker(CompletedKey, characterID, requirementID));
        }

        /// <summary>Freezes this character's own newly-satisfied requirements since the last call
        /// into a snapshot WasSatisfiedSinceLastCheck reads, and resets the live accumulator.
        /// Consumer-agnostic: Community Challenges' own cycle-end button calls this once per screen
        /// build (see that class's own doc comment for why it has to happen at screen-build time,
        /// not dialog-open time) -- a future consumer with a different notion of "since when" would
        /// call it on its own schedule instead, independently.</summary>
        public static void SnapshotAndResetForNewCheck(string characterID)
        {
            HashSet<string> current = _accumulating.TryGetValue(characterID, out HashSet<string> set)
                ? set
                : new HashSet<string>();
            _lastCheckSnapshot[characterID] = current;
            _accumulating[characterID] = new HashSet<string>();
        }

        /// <summary>Whether `requirementID` was newly satisfied as of this character's most recent
        /// SnapshotAndResetForNewCheck call.</summary>
        public static bool WasSatisfiedSinceLastCheck(string characterID, string requirementID)
        {
            return _lastCheckSnapshot.TryGetValue(characterID, out HashSet<string> set) && set.Contains(requirementID);
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
    }
}
