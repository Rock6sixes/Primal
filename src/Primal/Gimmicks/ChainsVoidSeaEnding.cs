using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MoreSlugcats;
using UnityEngine;
using VoidSea;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Added 2026-11-14, per direct request: a purely cosmetic rewrite of Primal Artificer's
    /// void-sea (ascension) ending, active only when the Chains gimmick is enabled for Artificer
    /// on the reserved Primal slot. Nothing here touches the Chains gimmick's own room-lock logic,
    /// any other character, any other ending, or any ordinary save slot.
    ///
    /// **The sequence, as requested (revised after the second in-game test, 2026-11-14):**
    ///  1. When Artificer has swum down past the lit gold band of the void sea (main body chunk
    ///     more than TriggerDepth -- ~10 screens -- below the scene origin, SB_L01's bottom edge),
    ///     InitialChains void chains attach to her body with the Chains gimmick's own room-locked
    ///     stinger (Chain_Lock), each running from her down to an anchor far below the bottom of
    ///     the screen. They are the loose, drifting, slowly-following kind the gimmick draws (same
    ///     VoidChain class, same colour pulse).
    ///  2. After LooseTicks (~10 s) every chain quickly eases into being rigid (StiffenTicks).
    ///  3. StiffHoldTicks (~1 s) later the chains start pulling her down -- not straight down but
    ///     with a slowly wandering sideways component the chains lean along with -- at 100 px/tick
    ///     until the main worm has reached her, ~50 while it hovers beside her looking, and 400
    ///     once it dives away (see the PullSpeed* constants). She keeps her own controls and can
    ///     swim around on top of the pull. One more chain is added every ExtraChainInterval ticks
    ///     (capped at MaxChains for sprite-count sanity).
    ///  4. When vanilla's dissolve begins (DissolveStartTimer, "starts breaking apart") the chains
    ///     stop being added and are released one at a time, evenly spaced, each with the gimmick's
    ///     ordinary room-clear stinger (Chain_Break), the pull continuing on the remaining ones.
    ///  5. One second before the slideshow (ExitTimer - FinalReleaseLead) the last chain goes with
    ///     a louder, layered Chain_Break. Vanilla's black fade and slideshow exit proceed untouched.
    ///
    /// **How vanilla's Artificer void-sea route works, from the decompiled source (not guessed),
    /// and where this hooks in.** Room.cs adds a VoidSeaScene to any room with the VoidSea room
    /// effect (SB_L01 for everyone but Saint). Once the player is below -25000 the main worm's
    /// MainWormBehavior goes GetToPlayer -> Looking (600 ticks) -> for Artificer specifically,
    /// SwimDown (VoidWorm.cs:905) instead of AttachingString -- it dives away from her with
    /// SuperSwim(-40 * ...) and, below -35000, sets VoidSeaScene.fadeOutLights (VoidWorm.cs:994).
    /// VoidSeaScene.Update then counts fadeOutLightsTimer: tick 1 destroys the ceiling, tick 1000
    /// destroys every other worm, and from then on calls ArtificerEndUpdate(fadeOutLightsTimer -
    /// 1000): timer 720 is Player.InitiateDissolve, 960..2360 ramps Player.dissolved (the
    /// "breaking apart" animation), 2360 starts the black FadeOut, and once that is done
    /// ExitToVoidSeaSlideShow -> BeatGameMode(standardVoidSea: true) -> ArtificerOutro. This file
    /// only reads those public fields (fadeOutLights, fadeOutLightsTimer, worms, sceneOrigo) from a
    /// Postfix on VoidSeaScene.Update; it never changes the worm's state machine or the timers,
    /// so the ending fires exactly when vanilla would have fired it. The pull just moves her
    /// further down meanwhile -- the void sea has no floor (VoidSeaTreatment sets
    /// restrictInRoomRange to float.MaxValue), nothing on the Artificer path keys off absolute
    /// depth once the worm is diving, and vanilla itself drags other slugcats down at the worm's
    /// full speed on its thread, so the speeds involved are ones the player physics already sees.
    ///
    /// **Why the chains anchor relative to Artificer rather than at one fixed deep point.** She
    /// is pulled for roughly two minutes at 40+ px/tick -- well over 100,000 px -- so a fixed
    /// anchor "deeper than she will go" would make each of VoidChain's 128 segments hundreds of
    /// pixels long and the chain texture a smeared straight line. Each anchor instead sits a few
    /// thousand pixels below her (AnchorDepth), far past the bottom of the screen, and moves with
    /// her: the chain always visibly comes up from the deep, and the loose phase keeps the
    /// drifting, lagging-behind look because only the two endpoints are pinned.
    ///
    /// **Why PullChain re-implements VoidChain.Update instead of calling it.** Stiffening has to
    /// run between the verlet/constraint pass and the graphic's own position copy, or the drawn
    /// chain would lag the stiffened positions by a tick. VoidChain's physics is ~20 lines
    /// (pushApart, sinusoidal drift, verlet, two Connect passes) and every field it touches is
    /// public, so it is copied with the loose-only terms scaled by (1 - stiffness) and a final
    /// per-segment lerp toward the straight anchor-to-body line. The chain is also drawn with
    /// its endpoints swapped relative to the gimmick's use: VoidChain's ChainGraphic fades alpha
    /// toward stuckPosB, so stuckPosA is the body (bright, where the chain visibly attaches) and
    /// stuckPosB the deep anchor (fading off-screen). VoidChain's Halo is drawn at stuckPosA, which
    /// would stack a dozen glyph rings on Artificer, so every chain hides it (ringsActive = 0 turns
    /// off every one of Halo.DrawSprites' visibility conditions).
    ///
    /// **Why the break uses its own fragment class.** VoidChain.Destroy scatters vanilla
    /// ChainFragments, but ChainFragment.Update deletes itself the moment pos.y &lt; -100 -- i.e.
    /// instantly, anywhere in the void sea. PullChainFragment below is the same gold "ChainFrag"
    /// sprite with a plain drift-and-fade and no terrain collision (there is no terrain down there
    /// to collide with).
    ///
    /// **In-game status.** First test (2026-11-14): the chains attached at y=-804 as intended, then
    /// the game crashed one screen later with "MethodAccessException: Method
    /// MoreSlugcats.VoidChain.ConnectToWalls() is inaccessible from PullChain.Update" -- Gotcha #1
    /// exactly. Fixed by re-typing the two private helpers and reflecting VoidWorm.behavior (see
    /// WormBehaviorField); the rest of the sequence has not yet been seen running.
    /// </summary>
    internal static class ChainsVoidSeaEnding
    {
        /// <summary>How far below the scene origin (SB_L01's bottom edge) Artificer has to be
        /// before the chains appear. Raised 2026-11-14 (second test) from 800 to ~10 screens per
        /// explicit request: "wait for arti to go past the yellow background" -- the lit, gold
        /// upper band of the void sea, which vanilla's own player light fades out across
        /// -2000..-8000 (VoidSeaTreatment's lightSource LerpMap). 10 x 768 px rounded up.</summary>
        private const float TriggerDepth = 8000f;

        private const int InitialChains = 10;
        private const int MaxChains = 30;

        /// <summary>Ticks (40/s) the chains stay loose before stiffening: ~10 s.</summary>
        private const int LooseTicks = 400;
        /// <summary>How quickly they ease into being rigid: 3/4 s.</summary>
        private const int StiffenTicks = 30;
        /// <summary>Pause between fully stiff and the pull starting: ~1 s.</summary>
        private const int StiffHoldTicks = 40;
        /// <summary>One more chain every 2.5 s while pulling.</summary>
        private const int ExtraChainInterval = 100;

        /// <summary>"Only 98% stiff" (explicit request, third test) -- a sliver of give so the
        /// lines don't read as drawn rulers.</summary>
        private const float StiffnessMax = 0.98f;
        /// <summary>Extra rest length (x1.3) given to the loose chains so they sag and drift
        /// instead of hanging taut.</summary>
        private const float LooseSlack = 1.3f;

        /// <summary>Pull speeds, px/tick, staged around the main worm's own state machine (third
        /// in-game test, 2026-11-14: at a flat 400 the worm never caught up, so the ending never
        /// started). Approach: until the worm has reached her. Looking: while it hovers beside her
        /// for its 600-tick look -- its Float() movement tops out around 50 px/tick, so anything
        /// faster leaves it behind. Dive: once it has gone (SwimDown / fadeOutLights), "quickly
        /// ramp up to 400". Each is approached with PullSpeedEase per tick.</summary>
        private const float PullSpeedApproach = 100f;
        private const float PullSpeedLooking = 50f;
        private const float PullSpeedDive = 400f;
        private const float PullSpeedEase = 0.1f;

        /// <summary>"Not directly down but a little random": a slowly wandering sideways component,
        /// as a fraction of the vertical speed. A new target in [-WanderMax, WanderMax] is rolled
        /// every WanderRerollMin..Max ticks and eased toward, and the chains lean the same way.</summary>
        private const float WanderMax = 0.35f;
        private const int WanderRerollMin = 40;
        private const int WanderRerollMax = 140;
        private const float WanderEase = 0.03f;
        /// <summary>How hard the sideways pull overrides her own swimming: the sideways velocity is
        /// eased toward the wander target by this much per tick, leaving her input room to nudge.</summary>
        private const float LateralEase = 0.06f;

        /// <summary>Vertical distance below Artificer each chain's deep anchor sits (randomised a
        /// little per chain) -- several screens below the bottom edge.</summary>
        private const float AnchorDepth = 3000f;
        private const float AnchorSpreadX = 900f;

        /// <summary>ArtificerEndUpdate timer values (its argument is fadeOutLightsTimer - 1000):
        /// 720 is Player.InitiateDissolve -- "artificer starts breaking apart"; 960 + 1400 = 2360
        /// is where the 200-tick black FadeOut is created (MoreSlugcats.FadeOut steps fade by
        /// 1/duration per tick), so the slideshow exit lands at 2560. The final chain goes one
        /// second (FinalReleaseLead) before that.</summary>
        private const int DissolveStartTimer = 720;
        private const int ExitTimer = 960 + 1400 + 200;
        private const int FinalReleaseLead = 40;

        /// <summary>Only chain segments within this distance of Artificer spawn fragments on break
        /// (the rest of each chain is off-screen anyway).</summary>
        private const float FragmentRadius = 1400f;

        private enum Phase { Waiting, Loose, Stiffening, Stiff, Pulling, Releasing, Done }

        private sealed class State
        {
            public Phase Phase = Phase.Waiting;
            public int Ticks;
            public int ExtraChainTimer;
            public float Stiffness;
            public float PullSpeed;
            public readonly List<PullChain> Chains = new List<PullChain>();

            /// <summary>Latched the first tick the main worm is seen diving away (SwimDown) or the
            /// scene's light fade has begun -- from then on the pull targets PullSpeedDive.</summary>
            public bool WormLeft;

            /// <summary>Sideways wander: current eased value, the target it eases toward, and ticks
            /// until the next re-roll.</summary>
            public float Wander;
            public float WanderTarget;
            public int WanderRerollIn;

            /// <summary>Releasing phase: ArtificerEndUpdate timer value at which the next
            /// single chain lets go, and the spacing between releases (computed once when the
            /// dissolve starts so the chains run out just before the final one).</summary>
            public int NextReleaseAt;
            public int ReleaseInterval;
        }

        private static readonly ConditionalWeakTable<VoidSeaScene, State> States = new ConditionalWeakTable<VoidSeaScene, State>();

        /// <summary>Real accessibility, checked by reflecting the game's own Assembly-CSharp.dll
        /// (not the publicized reference copy) after the first in-game test crashed: of everything
        /// this file touches, only VoidChain.ConnectToWalls, VoidChain.Connect and VoidWorm.behavior
        /// are private. The first two are re-typed in PullChain; this handle covers the third.</summary>
        private static readonly System.Reflection.FieldInfo WormBehaviorField = AccessTools.Field(typeof(VoidWorm), "behavior");

        private static bool IsEligible(RainWorldGame game, VoidSeaScene scene)
        {
            return ModManager.MSC
                && game.IsStorySession
                && game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer
                && !scene.Inverted
                && GimmickGate.IsEnabledForCharacter(game.rainWorld, MoreSlugcatsEnums.SlugcatStatsName.Artificer.value, ChainsGimmick.ID);
        }

        internal static void Update(VoidSeaScene scene)
        {
            Room room = scene?.room;
            RainWorldGame game = room?.game;
            if (game == null)
            {
                return;
            }

            State state = States.GetValue(scene, _ => new State());
            if (state.Phase == Phase.Done)
            {
                return;
            }

            // Same player the main worm itself targets (VoidWorm.MainWormBehavior.Update).
            Player player = ModManager.CoopAvailable ? game.RealizedPlayerFollowedByCamera : game.FirstRealizedPlayer;
            if (player == null || player.room != room || player.dead)
            {
                if (state.Phase != Phase.Waiting)
                {
                    Abort(state, player, "player left the void sea room");
                }
                return;
            }

            if (state.Phase == Phase.Waiting)
            {
                if (!IsEligible(game, scene) || !player.inVoidSea)
                {
                    return;
                }
                if (player.mainBodyChunk.pos.y > scene.sceneOrigo.y - TriggerDepth)
                {
                    return;
                }

                for (int i = 0; i < InitialChains; i++)
                {
                    AddChain(state, room, player, i);
                }
                // The Chains gimmick's own "room locked" stinger (LockNow's exact call), per request.
                room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Lock, 0f, 1f, UnityEngine.Random.value * 0.5f + 0.8f);
                state.Phase = Phase.Loose;
                state.Ticks = 0;
                Plugin.Log?.LogInfo($"[ChainsVoidSeaEnding] Artificer past the lit band (y={player.mainBodyChunk.pos.y:0}) -- {InitialChains} chains attached.");
                return;
            }

            state.Ticks++;

            switch (state.Phase)
            {
                case Phase.Loose:
                    state.Stiffness = 0f;
                    if (state.Ticks >= LooseTicks)
                    {
                        state.Phase = Phase.Stiffening;
                        state.Ticks = 0;
                        Plugin.Log?.LogInfo("[ChainsVoidSeaEnding] chains stiffening.");
                    }
                    break;

                case Phase.Stiffening:
                    state.Stiffness = StiffnessMax * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(state.Ticks / (float)StiffenTicks));
                    if (state.Ticks >= StiffenTicks)
                    {
                        state.Phase = Phase.Stiff;
                        state.Ticks = 0;
                    }
                    break;

                case Phase.Stiff:
                    state.Stiffness = StiffnessMax;
                    if (state.Ticks >= StiffHoldTicks)
                    {
                        state.Phase = Phase.Pulling;
                        state.Ticks = 0;
                        state.ExtraChainTimer = 0;
                        state.PullSpeed = 0f;
                        state.WanderRerollIn = 0;
                        // Control deliberately stays with the player (third test reversed the
                        // earlier NullController hand-off: "give artificer back control ... allow
                        // her to move around") -- only the vertical velocity is owned by the pull.
                        Plugin.Log?.LogInfo("[ChainsVoidSeaEnding] pull started.");
                    }
                    break;

                case Phase.Pulling:
                    state.Stiffness = StiffnessMax;
                    Pull(state, scene, player);

                    state.ExtraChainTimer++;
                    if (state.ExtraChainTimer >= ExtraChainInterval && state.Chains.Count < MaxChains)
                    {
                        state.ExtraChainTimer = 0;
                        AddChain(state, room, player, state.Chains.Count);
                    }

                    // Vanilla's dissolve has begun ("artificer starts breaking apart"): stop adding
                    // and start letting go, one chain at a time, spaced so the last-but-one has gone
                    // shortly before the final release at ExitTimer - FinalReleaseLead.
                    if (scene.fadeOutLights && scene.fadeOutLightsTimer - 1000 >= DissolveStartTimer)
                    {
                        int endTimer = scene.fadeOutLightsTimer - 1000;
                        int releasesBeforeFinal = Mathf.Max(1, state.Chains.Count - 1);
                        int window = (ExitTimer - FinalReleaseLead) - endTimer;
                        state.ReleaseInterval = Mathf.Max(10, window / (releasesBeforeFinal + 1));
                        state.NextReleaseAt = endTimer + state.ReleaseInterval;
                        state.Phase = Phase.Releasing;
                        Plugin.Log?.LogInfo($"[ChainsVoidSeaEnding] dissolve started -- releasing {state.Chains.Count} chains, one every {state.ReleaseInterval} ticks.");
                    }
                    break;

                case Phase.Releasing:
                    state.Stiffness = StiffnessMax;
                    Pull(state, scene, player);
                    {
                        int endTimer = scene.fadeOutLightsTimer - 1000;
                        if (endTimer >= ExitTimer - FinalReleaseLead || scene.endingSavedFlag)
                        {
                            FinalRelease(state, room, player);
                        }
                        else if (state.Chains.Count > 1 && endTimer >= state.NextReleaseAt)
                        {
                            state.NextReleaseAt += state.ReleaseInterval;
                            ReleaseOne(state, room, player);
                        }
                    }
                    break;
            }

            foreach (PullChain chain in state.Chains)
            {
                chain.Stiffness = state.Stiffness;
                chain.Lean = state.Wander;
            }
        }

        private static void AddChain(State state, Room room, Player player, int index)
        {
            // Alternate between the two body chunks so the pull reads on the whole body, and
            // fan the deep anchors out horizontally so the chains don't overlap into one line.
            int chunkIndex = index % player.bodyChunks.Length;
            Vector2 anchorOffset = new Vector2(
                Mathf.Lerp(-AnchorSpreadX, AnchorSpreadX, UnityEngine.Random.value),
                -AnchorDepth * Mathf.Lerp(0.9f, 1.2f, UnityEngine.Random.value));
            PullChain chain = new PullChain(room, player, chunkIndex, anchorOffset, LooseSlack);
            chain.Stiffness = state.Stiffness;
            room.AddObject(chain);
            state.Chains.Add(chain);
        }

        private static void Pull(State state, VoidSeaScene scene, Player player)
        {
            // --- vertical speed, staged on what the main worm is doing (see the constants) ---
            float target = PullSpeedApproach;
            if (!state.WormLeft && scene.fadeOutLights)
            {
                state.WormLeft = true;
            }
            if (!state.WormLeft && scene.worms != null)
            {
                foreach (VoidWorm worm in scene.worms)
                {
                    if (worm == null || !worm.mainWorm)
                    {
                        continue;
                    }
                    // VoidWorm.behavior is private in the real assembly (Gotcha #1) -- reflected, cached.
                    if (WormBehaviorField?.GetValue(worm) is VoidWorm.MainWormBehavior main)
                    {
                        if (main.phase == VoidWorm.MainWormBehavior.Phase.SwimDown
                            || main.phase == VoidWorm.MainWormBehavior.Phase.DepthReached
                            || main.phase == VoidWorm.MainWormBehavior.Phase.SwimBackUp)
                        {
                            state.WormLeft = true;
                            Plugin.Log?.LogInfo("[ChainsVoidSeaEnding] worm has left -- ramping to full pull speed.");
                        }
                        else if (main.phase == VoidWorm.MainWormBehavior.Phase.Looking
                            || main.phase == VoidWorm.MainWormBehavior.Phase.AttachingString
                            || main.phase == VoidWorm.MainWormBehavior.Phase.StringAttached)
                        {
                            target = PullSpeedLooking;
                        }
                    }
                    break;
                }
            }
            if (state.WormLeft)
            {
                target = PullSpeedDive;
            }
            state.PullSpeed = Mathf.Lerp(state.PullSpeed, target, PullSpeedEase);

            // --- sideways wander ---
            if (state.WanderRerollIn <= 0)
            {
                state.WanderTarget = Mathf.Lerp(-WanderMax, WanderMax, UnityEngine.Random.value);
                state.WanderRerollIn = UnityEngine.Random.Range(WanderRerollMin, WanderRerollMax + 1);
            }
            state.WanderRerollIn--;
            state.Wander = Mathf.Lerp(state.Wander, state.WanderTarget, WanderEase);

            float lateral = state.Wander * state.PullSpeed;
            for (int i = 0; i < player.bodyChunks.Length; i++)
            {
                BodyChunk chunk = player.bodyChunks[i];
                chunk.vel.y = -state.PullSpeed;
                // Eased, not assigned: her own swimming input still adds on top each tick, so she
                // can move around while being dragged.
                chunk.vel.x = Mathf.Lerp(chunk.vel.x, lateral, LateralEase);
            }
        }

        /// <summary>Lets one chain go (the most recently added, so the original ten linger
        /// longest) with the Chains gimmick's ordinary room-clear stinger -- FreePlayer's exact call.</summary>
        private static void ReleaseOne(State state, Room room, Player player)
        {
            int last = state.Chains.Count - 1;
            if (last < 0)
            {
                return;
            }
            state.Chains[last].Break(player.mainBodyChunk.pos, FragmentRadius);
            state.Chains.RemoveAt(last);
            room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Break, 0f, 1f, UnityEngine.Random.value * 0.5f + 0.8f);
        }

        /// <summary>The last chain(s) go a second before the slideshow, with the louder stinger.</summary>
        private static void FinalRelease(State state, Room room, Player player)
        {
            Vector2 focus = player.mainBodyChunk.pos;
            foreach (PullChain chain in state.Chains)
            {
                chain.Break(focus, FragmentRadius);
            }
            state.Chains.Clear();
            state.Phase = Phase.Done;

            // "A louder version of the room clear sound": per-source volume is capped at 1, so
            // loudness comes from layering three copies of the gimmick's own Chain_Break stinger
            // at different pitches (the low one adds weight the single 0.8-1.3 pitch lacks).
            room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Break, 0f, 1f, 0.6f);
            room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Break, 0f, 1f, 0.85f);
            room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Break, 0f, 1f, 1.1f);
            Plugin.Log?.LogInfo("[ChainsVoidSeaEnding] final chain released.");
        }

        private static void Abort(State state, Player player, string why)
        {
            foreach (PullChain chain in state.Chains)
            {
                chain.Destroy();
            }
            state.Chains.Clear();
            state.Phase = Phase.Done;
            Plugin.Log?.LogInfo("[ChainsVoidSeaEnding] aborted: " + why);
        }
    }

    [HarmonyPatch(typeof(VoidSeaScene), nameof(VoidSeaScene.Update))]
    internal static class ChainsVoidSeaEndingUpdateHook
    {
        private static bool _loggedError;

        private static void Postfix(VoidSeaScene __instance)
        {
            try
            {
                ChainsVoidSeaEnding.Update(__instance);
            }
            catch (Exception e)
            {
                if (!_loggedError)
                {
                    _loggedError = true;
                    Plugin.Log?.LogError("[ChainsVoidSeaEnding] update failed (vanilla ending unaffected): " + e);
                }
            }
        }
    }

    /// <summary>One chain from one of Artificer's body chunks down to a deep anchor that moves
    /// with her. See ChainsVoidSeaEnding's header for why Update is re-implemented rather than
    /// inherited, why the endpoints are the way round they are, and why the halo is hidden.</summary>
    internal sealed class PullChain : VoidChain
    {
        private readonly Player _player;
        private readonly int _chunkIndex;
        private readonly Vector2 _anchorOffset;
        private int _activeTime;

        /// <summary>0 = the gimmick's ordinary loose, drifting chain; 1 = a straight rigid line
        /// from body to anchor. Set every tick by ChainsVoidSeaEnding.</summary>
        public float Stiffness;

        /// <summary>Sideways lean of the deep anchor as a fraction of its depth -- the pull's own
        /// wander value, so the chains point where she is actually being dragged.</summary>
        public float Lean;

        public PullChain(Room room, Player player, int chunkIndex, Vector2 anchorOffset, float slack)
            : base(room, player.bodyChunks[chunkIndex].pos, player.bodyChunks[chunkIndex].pos + anchorOffset)
        {
            _player = player;
            _chunkIndex = chunkIndex;
            _anchorOffset = anchorOffset;
            conRad *= slack;
            halo.ringsActive = 0;
            proximityAlpha = 0.5f;
        }

        public override void Update(bool eu)
        {
            evenUpdate = eu;
            if (room == null || _player == null || _player.room != room || _chunkIndex >= _player.bodyChunks.Length)
            {
                Destroy();
                return;
            }

            Vector2 bodyPos = _player.bodyChunks[_chunkIndex].pos;
            stuckPosA = bodyPos;
            stuckPosB = bodyPos + _anchorOffset + new Vector2(Lean * -_anchorOffset.y, 0f);

            float loose = 1f - Stiffness;
            int count = segments.GetLength(0);

            // --- VoidChain.Update's own physics, with the loose-only terms scaled by (1 - stiffness) ---
            driftTime += 1f;
            for (int i = 2; i < count; i++)
            {
                Vector2 dir = (segments[i, 0] - segments[i - 2, 0]).normalized;
                segments[i - 2, 2] -= dir * (pushApart * loose);
                segments[i, 2] += dir * (pushApart * loose);
            }
            Vector2 drift = new Vector2(Mathf.Sin((float)Math.PI * (driftTime / 1200f)), Mathf.Cos((float)Math.PI * (driftTime / 1200f))) * (0.2f * loose);
            for (int j = 0; j < count; j++)
            {
                segments[j, 2] += drift;
                segments[j, 1] = segments[j, 0];
                segments[j, 0] += segments[j, 2];
                segments[j, 2] *= 0.599f;
            }
            PinEnds();
            for (int k = count - 1; k > 0; k--)
            {
                Link(k, k - 1);
            }
            PinEnds();
            for (int k = 1; k < count; k++)
            {
                Link(k, k - 1);
            }
            PinEnds();

            // --- stiffening: pull every segment toward the straight body-to-anchor line ---
            if (Stiffness > 0f)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector2 target = Vector2.Lerp(stuckPosA, stuckPosB, i / (float)(count - 1));
                    segments[i, 0] = Vector2.Lerp(segments[i, 0], target, Stiffness);
                    segments[i, 2] *= loose;
                }
            }

            // Same slow colour pulse the Chains gimmick's MultiChain gives its chains while loose;
            // a rigid chain glows at full gold instead.
            float phase = Mathf.Sin((float)Math.PI * (_activeTime / 400f) - (float)Math.PI);
            float looseFlash = phase * 0.25f + 0.25f + 0.5f * Mathf.Min(1f, _activeTime / 80f);
            colorFlash = Mathf.Lerp(looseFlash, 1f, Stiffness);
            proximityAlpha = Mathf.Lerp(0.5f, 0.75f, Stiffness);
            _activeTime++;

            graphic.Update();
            halo.Update();
        }

        /// <summary>VoidChain.ConnectToWalls, re-typed here because the real one is PRIVATE in the
        /// game assembly (the publicized reference DLL and the decompiled source both show it as
        /// public -- Gotcha #1; calling it crashed the game with MethodAccessException on the first
        /// real test). Pins the two end segments to the endpoints and kills their velocity.</summary>
        private void PinEnds()
        {
            int last = segments.GetLength(0) - 1;
            segments[0, 0] = stuckPosA;
            segments[0, 2] *= 0f;
            segments[last, 0] = stuckPosB;
            segments[last, 2] *= 0f;
        }

        /// <summary>VoidChain.Connect, re-typed for the same reason as PinEnds: a spring between
        /// two neighbouring segments toward rest length conRad.</summary>
        private void Link(int a, int b)
        {
            Vector2 delta = segments[a, 0] - segments[b, 0];
            float dist = delta.magnitude;
            if (dist <= 0f)
            {
                return;
            }
            Vector2 normalized = delta / dist;
            float strength = Mathf.InverseLerp(0f, conRad, dist);
            Vector2 push = normalized * ((conRad - dist) * 0.5f * strength);
            segments[a, 0] += push;
            segments[a, 2] += push;
            segments[b, 0] -= push;
            segments[b, 2] -= push;
        }

        /// <summary>Shatter: scatter fragments from the on-screen part of the chain, then remove it.</summary>
        public void Break(Vector2 focus, float radius)
        {
            if (room != null)
            {
                for (int i = 0; i < segments.GetLength(0); i += 4)
                {
                    if ((segments[i, 0] - focus).sqrMagnitude < radius * radius)
                    {
                        room.AddObject(new PullChainFragment(segments[i, 0], UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(2f, 9f)));
                    }
                }
            }
            Destroy();
        }

        /// <summary>Skips VoidChain.Destroy's vanilla ChainFragment spawn (those delete themselves
        /// below y = -100, i.e. instantly down here) and just slates the object, which is all
        /// UpdatableAndDeletable.Destroy does.</summary>
        public override void Destroy()
        {
            slatedForDeletetion = true;
        }
    }

    /// <summary>A gold chain-link shard that drifts, spins and fades -- the same "ChainFrag"
    /// sprite vanilla's ChainFragment uses, minus its terrain collision and its in-room bounds
    /// check (see ChainsVoidSeaEnding's header).</summary>
    internal sealed class PullChainFragment : CosmeticSprite
    {
        private const int Lifetime = 70;

        private float _rotation;
        private float _lastRotation;
        private readonly float _rotVel;
        private int _age;

        public PullChainFragment(Vector2 pos, Vector2 vel)
        {
            this.pos = pos;
            lastPos = pos;
            this.vel = vel;
            _rotation = UnityEngine.Random.value * 360f;
            _lastRotation = _rotation;
            _rotVel = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Lerp(4f, 20f, Mathf.InverseLerp(0f, 9f, vel.magnitude));
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            vel *= 0.94f;
            _lastRotation = _rotation;
            _rotation += _rotVel;
            _age++;
            if (_age >= Lifetime)
            {
                Destroy();
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("ChainFrag");
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 drawPos = Vector2.Lerp(lastPos, pos, timeStacker);
            sLeaser.sprites[0].x = drawPos.x - camPos.x;
            sLeaser.sprites[0].y = drawPos.y - camPos.y;
            sLeaser.sprites[0].rotation = Mathf.Lerp(_lastRotation, _rotation, timeStacker);
            sLeaser.sprites[0].color = RainWorld.SaturatedGold;
            sLeaser.sprites[0].alpha = 1f - Mathf.Clamp01(_age / (float)Lifetime);
            sLeaser.sprites[0].scale = 0.75f;
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
    }
}
