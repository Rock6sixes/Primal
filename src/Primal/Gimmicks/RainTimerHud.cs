using System.Collections.Generic;
using HarmonyLib;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The on-screen readout for RainTimerGimmick (2026-11-16, explicit request): a second,
    /// slightly smaller and slightly bluer timer line sitting directly under vanilla's own
    /// campaign timer, showing how far into the current rain cycle you are -- and, while a
    /// precycle is still running, how long is left BEFORE the rain timer even starts, as a
    /// negative number counting up to zero (-5s, -1s, 0s, 1s, 5s, ...).
    ///
    /// WHAT "THE CURRENT CAMPAIGN TIMER" ACTUALLY IS: `MoreSlugcats.SpeedRunTimer`, a HudPart
    /// added by `HUD.InitSinglePlayerHud` under `if (ModManager.MMF &amp;&amp;
    /// MMF.cfgSpeedrunTimer.Value)` -- the in-game total-playtime readout (`000h:00m:00s:000ms`,
    /// `Custom.GetDisplayFont()`, left-aligned, top-centre-ish). Read directly from
    /// `reference/GameSource/MoreSlugcats/SpeedRunTimer.cs` rather than guessed at, which is also
    /// where this part's own position/fade/alpha rules come from, so the two lines move, fade and
    /// dim together instead of drifting apart.
    ///
    /// HOW IT ANCHORS: if that vanilla part exists in `hud.parts` (i.e. the player has the
    /// speedrun timer turned on), this part reads its live `DrawPos(timeStacker)` and sits exactly
    /// one font line-height below it -- so it tracks the campaign timer's own fade-driven vertical
    /// shift (`ScreenSize.y - (30f + 10f * fade)`) for free, with no duplicated assumptions. If
    /// the campaign timer is turned OFF, there is nothing to anchor to, so this part falls back to
    /// recomputing that same vanilla formula itself and sits where the second line would have
    /// been -- the rain timer is this mod's own feature and shouldn't silently disappear just
    /// because an unrelated vanilla HUD option is off.
    ///
    /// WHY THE ANCHORING HAPPENS IN `Draw`, NOT `Update`: `HUD.Update` walks `parts` BACKWARDS
    /// (`for (int num = parts.Count - 1; num >= 0; num--)`, see HUD.cs), and this part is added
    /// last, so it Updates BEFORE the campaign timer does -- reading SpeedRunTimer's `pos` from
    /// `Update` would always be one tick stale and lag the campaign timer by a pixel or so while
    /// either line is fading. `HUD.Draw` is a separate, later pass over the same list, by which
    /// point every part's Update has run, so `SpeedRunTimer.DrawPos(timeStacker)` there is exactly
    /// the position the campaign timer's own label is being drawn at this frame. That also means
    /// this part needs no position interpolation of its own: it inherits the campaign timer's.
    ///
    /// WHAT THE NUMBER IS: `RainCycle.timer - RainCycle.preTimer`, in ticks (40/second). Both are
    /// plain public ints on RainCycle. The subtraction is what makes the negative half work with
    /// no special-casing, and it's exact rather than a convenient approximation, because of how
    /// `RainCycle.Update` actually sequences the two (traced in
    /// `reference/GameSource/RainCycle.cs`): `preTimer` counts DOWN from `maxPreTimer` to 0, and
    /// for as long as it is above 0 the `timer++` branch is skipped entirely -- so during a
    /// precycle `timer` is still exactly 0 and the expression is exactly `-preTimer`, and once the
    /// precycle ends `preTimer` is exactly 0 and the expression is exactly `timer`. There is no
    /// window where both are nonzero. (`maxPreTimer`/`preTimer` are 0 whenever there is no
    /// precycle at all, including when the Precycle Module is off, so the same expression is just
    /// `timer` in that case.)
    ///
    /// Seconds are FLOORED on the positive side (ordinary elapsed-time counting: "1s" means one
    /// full second has passed) and CEILINGed on the negative side, so the readout holds at "-1s"
    /// until the precycle genuinely ends and only shows "0s" at the exact tick the rain timer
    /// starts -- rather than hitting "0s" up to 39 ticks early, which a plain floor of a negative
    /// division would do.
    ///
    /// Gated on PrimalSlot, on the Rain Timer gimmick being assigned to this character, and on
    /// RainTimerSettings' own "Show rain timer on HUD" checkbox (default ON). The gate is
    /// re-checked every Update, not just at construction, so toggling the Remix checkbox mid-
    /// session takes effect immediately instead of needing a HUD rebuild.
    /// </summary>
    internal class RainTimerHudPart : global::HUD.HudPart
    {
        /// <summary>Slightly smaller than the campaign timer above it, per the request. Scales the
        /// FLabel as a whole (FNode.scale); the font's own line height is unaffected, which is
        /// what the FontLineHeight spacing below deliberately relies on.</summary>
        private const float LabelScale = 0.85f;

        /// <summary>Slightly bluer than the campaign timer's plain white, per the request --
        /// deliberately a desaturated blue-white rather than a saturated blue, so it still reads
        /// as the same kind of HUD text and stays legible against a bright sky.</summary>
        private static readonly Color LabelColor = new Color(0.65f, 0.78f, 1f);

        /// <summary>Only used if `FLabel.FontLineHeight` comes back as 0 (the display font failing
        /// to report a line height at all, which shouldn't happen but would collapse the two lines
        /// on top of each other if it did). 15f is HUD.DialogBox's own `lineHeight` constant for
        /// the same non-large-font case.</summary>
        private const float RowSpacingFallback = 15f;

        private readonly FLabel _timeLabel;

        private float _fade;
        private float _lastFade;
        private bool _show;

        public RainTimerHudPart(global::HUD.HUD hud, FContainer container)
            : base(hud)
        {
            _timeLabel = new FLabel(Custom.GetDisplayFont(), "")
            {
                scale = LabelScale,
                color = LabelColor,
                alignment = FLabelAlignment.Left,
                alpha = 0f,
            };
            container.AddChild(_timeLabel);
        }

        /// <summary>One font line below wherever the campaign timer's label is actually being drawn
        /// this frame -- see this class's header comment, HOW IT ANCHORS (and WHY THE ANCHORING
        /// HAPPENS IN `Draw`), including the fallback for when the campaign timer isn't present at
        /// all. `timeStacker` is the same frame-interpolation fraction HudPart.Draw is handed.</summary>
        private Vector2 AnchorPos(float timeStacker)
        {
            float rowSpacing = _timeLabel.FontLineHeight > 0f ? _timeLabel.FontLineHeight : RowSpacingFallback;

            SpeedRunTimer campaignTimer = FindCampaignTimer();
            if (campaignTimer != null)
            {
                // `pos`/`lastPos` (and so DrawPos) already have SpeedRunTimer.Update's own trailing
                // `pos.x -= 95f` applied, so this is the label's real drawn position, not the
                // pre-offset anchor.
                return campaignTimer.DrawPos(timeStacker) - new Vector2(0f, rowSpacing);
            }

            // Same expression SpeedRunTimer.Update uses, reproduced for the campaign-timer-off
            // case. Vanilla computes that from its current `fade` and then interpolates the
            // resulting position; the formula is linear in `fade`, so feeding it the interpolated
            // fade here is equivalent and needs no second position to interpolate against.
            Vector2 screenSize = hud.rainWorld.options.ScreenSize;
            float fade = Mathf.Lerp(_lastFade, _fade, timeStacker);
            return new Vector2(
                (float)(int)(screenSize.x / 2f) + 0.2f - 95f,
                (float)(int)(screenSize.y - (30f + 10f * fade)) + 0.2f - rowSpacing);
        }

        private SpeedRunTimer FindCampaignTimer()
        {
            List<global::HUD.HudPart> parts = hud.parts;
            if (parts == null)
            {
                return null;
            }

            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i] is SpeedRunTimer campaignTimer)
                {
                    return campaignTimer;
                }
            }
            return null;
        }

        /// <summary>Whether this readout applies at all right now: the Primal slot, the Rain Timer
        /// gimmick assigned to the character actually being played, and that character's own "Show
        /// rain timer on HUD" checkbox.</summary>
        private bool ShouldShow(out RainCycle rainCycle)
        {
            rainCycle = null;

            if (!(hud.owner is Player player))
            {
                return false;
            }

            World world = player.abstractCreature?.world;
            RainWorldGame game = world?.game;
            if (game == null || !game.IsStorySession || !PrimalSlot.IsActive(game.rainWorld))
            {
                return false;
            }

            string characterID = game.StoryCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, RainTimerGimmick.ID) ||
                !RainTimerSettings.ShowHud(characterID))
            {
                return false;
            }

            rainCycle = world.rainCycle;
            return rainCycle != null;
        }

        /// <summary>Signed cycle ticks (40/second) formatted the way the campaign timer above
        /// formats its own time, except that all-zero leading segments are dropped -- so a short
        /// precycle reads exactly "-5s" rather than "-000h:00m:05s", while a long cycle still
        /// reads "1h:04m:30s". The sign and rounding rules are the whole point of this part, so
        /// they live in one static, separately-checkable method.</summary>
        internal static string FormatCycleTicks(int ticks)
        {
            int ticksPerSecond = TimeLimitGimmick.TicksPerSecond;
            bool negative = ticks < 0;

            // Ceiling while negative so "0s" appears exactly when preTimer reaches 0, not up to a
            // second early; plain floor while positive, i.e. ordinary elapsed-time counting.
            int magnitudeTicks = negative ? -ticks : ticks;
            int totalSeconds = negative
                ? (magnitudeTicks + ticksPerSecond - 1) / ticksPerSecond
                : magnitudeTicks / ticksPerSecond;

            int hours = totalSeconds / 3600;
            int minutes = totalSeconds / 60 % 60;
            int seconds = totalSeconds % 60;
            string sign = negative && totalSeconds > 0 ? "-" : "";

            if (hours > 0)
            {
                return $"{sign}{hours}h:{minutes:D2}m:{seconds:D2}s";
            }
            if (minutes > 0)
            {
                return $"{sign}{minutes}m:{seconds:D2}s";
            }
            return $"{sign}{seconds}s";
        }

        public override void Update()
        {
            _lastFade = _fade;
            _show = ShouldShow(out RainCycle rainCycle);
            if (_show)
            {
                _timeLabel.text = FormatCycleTicks(rainCycle.timer - rainCycle.preTimer);
            }

            // Mirrors SpeedRunTimer.Update's own fade rules (including the foodMeter floor and the
            // HideGeneralHud override) so the two lines dim and shift in lockstep, with an added
            // null guard on foodMeter that vanilla's own version does without.
            if (_show)
            {
                float foodMeterFade = hud.foodMeter != null ? hud.foodMeter.fade : 0f;
                _fade = Mathf.Max(Mathf.Min(1f, _fade + 0.1f), foodMeterFade);
            }
            else
            {
                _fade = Mathf.Max(0f, _fade - 0.1f);
            }
            if (hud.HideGeneralHud)
            {
                _fade = 0f;
            }
        }

        public override void Draw(float timeStacker)
        {
            if (!_show && _fade <= 0f)
            {
                _timeLabel.alpha = 0f;
                return;
            }

            Vector2 drawPos = AnchorPos(timeStacker);
            _timeLabel.x = drawPos.x;
            _timeLabel.y = drawPos.y;

            // Same shape as SpeedRunTimer's own alpha, except its 0.2f floor only applies while
            // this part is actually showing -- so it fades out completely when the gimmick or the
            // checkbox is off, instead of leaving a faint ghost line behind forever.
            float fade = Mathf.Max(0f, Mathf.Lerp(_lastFade, _fade, timeStacker));
            _timeLabel.alpha = _show ? Mathf.Max(0.2f, Mathf.Pow(fade, 1.5f)) : Mathf.Pow(fade, 1.5f);
        }

        public override void ClearSprites()
        {
            base.ClearSprites();
            _timeLabel.RemoveFromContainer();
        }
    }

    /// <summary>
    /// Adds RainTimerHudPart to the single-player HUD. Postfixes `HUD.InitSinglePlayerHud` -- the
    /// one method that adds every in-game HUD element, and where vanilla itself adds
    /// `MoreSlugcats.SpeedRunTimer` (see RainTimerHudPart's header comment) -- so this part is
    /// constructed last and therefore Updates/Draws after the campaign timer it anchors to has
    /// already moved this frame.
    ///
    /// `fContainers[1]` is the same container vanilla hands SpeedRunTimer and every other
    /// text/meter element, so this label lands in the same draw layer.
    ///
    /// The `[HarmonyPatch]` argument types are pinned explicitly even though
    /// `InitSinglePlayerHud` currently has no overloads: an unpinned, ambiguous patch target in
    /// this mod took down `Harmony.PatchAll()` -- and with it the entire plugin, Remix config
    /// included -- once already (see commit "Fix: Harmony PatchAll crash", and
    /// PermanentShelterFailureGimmick.cs's own note), and PatchAll has no per-patch isolation.
    ///
    /// Deliberately NOT gated here: the part is added for every character and checks PrimalSlot /
    /// the gimmick assignment / the checkbox itself, every Update (see
    /// RainTimerHudPart.ShouldShow). One idle HudPart that renders nothing costs essentially
    /// nothing, and it means flipping the Remix checkbox mid-session works without rebuilding the
    /// HUD.
    /// </summary>
    [HarmonyPatch(typeof(global::HUD.HUD), nameof(global::HUD.HUD.InitSinglePlayerHud), new System.Type[] { typeof(RoomCamera) })]
    internal static class RainTimerHudHook
    {
        private static void Postfix(global::HUD.HUD __instance)
        {
            if (__instance?.fContainers == null || __instance.fContainers.Length < 2)
            {
                return;
            }

            __instance.AddPart(new RainTimerHudPart(__instance, __instance.fContainers[1]));
        }
    }
}
