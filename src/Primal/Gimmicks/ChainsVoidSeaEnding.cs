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
    /// **The sequence, as requested:**
    ///  1. When Artificer swims down out of SB_L01 into the void sea proper (main body chunk more
    ///     than TriggerDepth below the scene origin -- the room's bottom edge, so the room itself
    ///     has scrolled off the top of the screen), InitialChains void chains attach to her body,
    ///     each running from her down to an anchor far below the bottom of the screen. They are
    ///     the loose, drifting, slowly-following kind the Chains gimmick draws (same VoidChain
    ///     class, same colour pulse).
    ///  2. After LooseTicks (~10 s) every chain quickly eases into being rigid (StiffenTicks).
    ///  3. StiffHoldTicks (~1 s) later the chains start pulling her straight down at the void
    ///     worm's own swim-down speed, and keep adding one more chain every ExtraChainInterval
    ///     ticks while the pull lasts (capped at MaxChains for sprite-count sanity).
    ///  4. The moment vanilla's own dissolve animation finishes (see DissolveEndTimer), every
    ///     chain shatters into fragments and a louder, layered version of the Chains gimmick's
    ///     room-clear sound (MSCSoundID.Chain_Break) plays. Vanilla's black fade and slideshow
    ///     exit then proceed untouched.
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
    /// **Not yet confirmed in-game** -- compiles against the reference assembly; every vanilla
    /// line referenced above was read from reference/GameSource, not assumed.
    /// </summary>
    internal static class ChainsVoidSeaEnding
    {
        /// <summary>How far below the scene origin (SB_L01's bottom edge) Artificer has to be
        /// before the chains appear. The void-sea camera sits ~400 px above the player, so at this
        /// depth the room itself is already off the top of the screen.</summary>
        private const float TriggerDepth = 800f;

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

        /// <summary>Fully "very stiff" still leaves a sliver of give so the lines don't read as
        /// drawn rulers.</summary>
        private const float StiffnessMax = 0.92f;
        /// <summary>Extra rest length (x1.3) given to the loose chains so they sag and drift
        /// instead of hanging taut.</summary>
        private const float LooseSlack = 1.3f;

        /// <summary>Baseline pull speed, px/tick: the worm's own SuperSwim(-40) thrust. While the
        /// main worm is actually in its SwimDown phase its live head speed is used instead when
        /// faster (capped at MaxPullSpeed), so "as fast as the void worm" tracks the real worm.</summary>
        private const float BasePullSpeed = 40f;
        private const float MaxPullSpeed = 160f;

        /// <summary>Vertical distance below Artificer each chain's deep anchor sits (randomised a
        /// little per chain) -- several screens below the bottom edge.</summary>
        private const float AnchorDepth = 3000f;
        private const float AnchorSpreadX = 900f;

        /// <summary>ArtificerEndUpdate's own num + num2 (960 + 1400): the timer value at which
        /// Player.dissolved stops ramping and the black fade is created -- "finishes the breaking
        /// apart animation".</summary>
        private const int DissolveEndTimer = 960 + 1400;

        /// <summary>Only chain segments within this distance of Artificer spawn fragments on break
        /// (the rest of each chain is off-screen anyway).</summary>
        private const float FragmentRadius = 1400f;

        private enum Phase { Waiting, Loose, Stiffening, Stiff, Pulling, Done }

        private sealed class State
        {
            public Phase Phase = Phase.Waiting;
            public int Ticks;
            public int ExtraChainTimer;
            public float Stiffness;
            public float PullSpeed;
            public readonly List<PullChain> Chains = new List<PullChain>();
        }

        private static readonly ConditionalWeakTable<VoidSeaScene, State> States = new ConditionalWeakTable<VoidSeaScene, State>();

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
                    Abort(state, "player left the void sea room");
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
                state.Phase = Phase.Loose;
                state.Ticks = 0;
                Plugin.Log?.LogInfo($"[ChainsVoidSeaEnding] Artificer passed below SB_L01 (y={player.mainBodyChunk.pos.y:0}) -- {InitialChains} chains attached.");
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

                    // Vanilla's dissolve has run its course (or, as a safety net, the exit has
                    // already been requested): shatter everything.
                    if ((scene.fadeOutLights && scene.fadeOutLightsTimer - 1000 >= DissolveEndTimer) || scene.endingSavedFlag)
                    {
                        BreakAll(state, room, player);
                    }
                    break;
            }

            foreach (PullChain chain in state.Chains)
            {
                chain.Stiffness = state.Stiffness;
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
            float target = BasePullSpeed;
            if (scene.worms != null)
            {
                foreach (VoidWorm worm in scene.worms)
                {
                    if (worm == null || !worm.mainWorm || worm.chunks == null || worm.chunks.Length == 0)
                    {
                        continue;
                    }
                    if (worm.behavior is VoidWorm.MainWormBehavior main && main.phase == VoidWorm.MainWormBehavior.Phase.SwimDown)
                    {
                        float wormDownSpeed = -worm.chunks[0].vel.y;
                        if (wormDownSpeed > target)
                        {
                            target = Mathf.Min(wormDownSpeed, MaxPullSpeed);
                        }
                    }
                }
            }

            // Ease into the pull over roughly half a second rather than snapping to full speed.
            state.PullSpeed = Mathf.Lerp(state.PullSpeed, target, 0.08f);
            for (int i = 0; i < player.bodyChunks.Length; i++)
            {
                BodyChunk chunk = player.bodyChunks[i];
                chunk.vel.y = -state.PullSpeed;
                chunk.vel.x *= 0.85f;
            }
        }

        private static void BreakAll(State state, Room room, Player player)
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
            Plugin.Log?.LogInfo("[ChainsVoidSeaEnding] dissolve finished -- all chains broken.");
        }

        private static void Abort(State state, string why)
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
            stuckPosB = bodyPos + _anchorOffset;

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
            ConnectToWalls();
            for (int k = count - 1; k > 0; k--)
            {
                Connect(k, k - 1);
            }
            ConnectToWalls();
            for (int k = 1; k < count; k++)
            {
                Connect(k, k - 1);
            }
            ConnectToWalls();

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
