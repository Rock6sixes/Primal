using System;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared mechanism #6 from COMMUNITY_CHALLENGES.md: a reusable "checkpoint speedrun segment" --
    /// start a timer at a shelter, optionally require passing through named rooms in order, and
    /// check the elapsed in-game time once back at the start. Covers Circuit (3 named checkpoint
    /// rooms, 3m45s limit) and U-Turn (no named checkpoints, just a loop-and-return, 1 minute
    /// limit) as two instances of the same small state machine rather than two bespoke ones.
    ///
    /// Timed against RainWorldGame.clock, the same per-tick counter TimeLimitGimmick already
    /// relies on for its own "per-run" time limit -- confirmed via decompilation to increment
    /// exactly once per RawUpdate call at a nominal 40 ticks/second (RainWorldGame.cs:
    /// `framesPerSecond = 40` and the single `clock++` in its own update loop), which is what
    /// TicksToMinutes below converts against. Not the MoreSlugcats-namespaced SpeedRunTimer HUD
    /// class some of these challenges' own rules reference for Ascension -- that class lives under
    /// the `MoreSlugcats` namespace the same way the DLC-gated enums that broke
    /// PrimalOptions.AllCharacters did (see that fix a few commits back), which strongly suggests it
    /// may not function with MSC disabled -- and every one of these challenges requires MSC
    /// disabled. RainWorldGame.clock is a plain vanilla field with no such risk.
    ///
    /// Not a static registry like SurvivorCreatureTracking -- each in-progress attempt is expected
    /// to own one instance of this class itself (most naturally as a per-character dictionary entry
    /// in whatever future tracker uses it), since there's no cross-cutting reason for this one to be
    /// looked up by a string key the way the shelter/creature/toll mechanisms are.
    /// </summary>
    internal sealed class SurvivorCheckpointRun
    {
        private readonly string[] _checkpointRoomsInOrder;
        private int _startClock = -1;
        private int _nextCheckpoint;

        /// <summary>`checkpointRoomsInOrder` may be empty (U-Turn's case: no named checkpoints,
        /// just a timed loop) or populated (Circuit's SS_D04/SS_I03/SS_A03, in the order they must
        /// be reached).</summary>
        public SurvivorCheckpointRun(params string[] checkpointRoomsInOrder)
        {
            _checkpointRoomsInOrder = checkpointRoomsInOrder ?? Array.Empty<string>();
        }

        public bool IsRunning => _startClock >= 0;

        public bool AllCheckpointsHit => _nextCheckpoint >= _checkpointRoomsInOrder.Length;

        /// <summary>Starts (or restarts) the timer from the current tick.</summary>
        public void Begin(RainWorldGame game)
        {
            _startClock = game?.clock ?? -1;
            _nextCheckpoint = 0;
        }

        public void Abort()
        {
            _startClock = -1;
            _nextCheckpoint = 0;
        }

        /// <summary>Call whenever the player enters a new room while a run is in progress. Only
        /// the NEXT required checkpoint (in order) advances the index -- passing back through an
        /// already-hit checkpoint, or through a room that isn't one at all, does nothing, so
        /// wandering through unrelated rooms between checkpoints doesn't break the run.</summary>
        public void NotifyRoomEntered(string roomName)
        {
            if (!IsRunning || AllCheckpointsHit || roomName == null)
            {
                return;
            }
            if (string.Equals(roomName, _checkpointRoomsInOrder[_nextCheckpoint], StringComparison.OrdinalIgnoreCase))
            {
                _nextCheckpoint++;
            }
        }

        /// <summary>Elapsed ticks since Begin() if the run is complete (every checkpoint hit, in
        /// order) and the player is back at the start -- callers are expected to call this only
        /// once they've independently confirmed "player is in the start shelter room" themselves,
        /// since that check is challenge-specific (a hardcoded room name) rather than anything this
        /// class tracks. Null if the run was never started or the checkpoints aren't all hit yet --
        /// callers should treat null as "not done", not as a failure to reset by itself; call
        /// Abort() explicitly once a null result means the attempt is over (e.g. the cycle ended).</summary>
        public int? ElapsedTicksIfComplete(RainWorldGame game)
        {
            if (!IsRunning || !AllCheckpointsHit || game == null)
            {
                return null;
            }
            return game.clock - _startClock;
        }

        /// <summary>Ticks-per-minute conversion, per this class's own header comment (40 ticks/sec
        /// nominal).</summary>
        public static float TicksToMinutes(int ticks) => ticks / 2400f;
    }
}
