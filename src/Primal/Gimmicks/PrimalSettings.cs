using Menu.Remix.MixedUI;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Mod-wide settings -- the ones that aren't per (character, gimmick) and so don't belong in
    /// any GimmickDef's own BindSettings/BuildSettingsUI.
    ///
    /// Right now that's exactly one: which key switches between a real save slot and its Primal
    /// counterpart on the character-select screen. That was a hardcoded KeyCode.Tab in
    /// PrimalSlotTabSwitchHook until 2026-09-15, which made it the only player-facing behavior in
    /// the whole mod that couldn't be configured -- and Tab is a plausible thing to collide with
    /// another mod's own binding, with no way out.
    ///
    /// Bound from PrimalOptions.EnsureConfigBound (alongside every gimmick's own settings, and for
    /// the same reason -- see Gotcha #7 in AI_CONTEXT.md: a Configurable that is never Bound is
    /// silently skipped by Remix's own config-load pass, so the saved value would never come back
    /// after a restart) and shown in the header row of this mod's Remix screen.
    /// </summary>
    internal static class PrimalSettings
    {
        public const KeyCode DefaultSlotSwitchKey = KeyCode.Tab;

        private static Configurable<KeyCode> _slotSwitchKey;

        // Built once and reused -- a Configurable<T> can only ever be bound to one UIconfig
        // element for the life of the OptionInterface (Gotcha #4 in AI_CONTEXT.md), and this
        // screen's header is rebuilt whenever Initialize runs.
        private static OpKeyBinder _slotSwitchBinder;

        public static void Bind(OptionInterface owner)
        {
            _slotSwitchKey = owner.config.Bind(
                "Primal_slotSwitchKey",
                DefaultSlotSwitchKey,
                new ConfigurableInfo("Key that switches between a save slot and its Primal counterpart on the character-select screen."));

            // A re-Bind means a NEW OptionInterface instance with a NEW ConfigHolder, so the
            // Configurable above is a different object than whatever the cached binder is attached
            // to. Drop the cached binder so the next BuildSlotSwitchBinder builds one against the
            // live Configurable -- keeping the old one would leave a control wired to a discarded
            // Configurable on the new screen. (Within a SINGLE instance this never fires:
            // PrimalOptions.EnsureConfigBound is idempotent, so the binder stays cached and gets
            // reused, which is what Gotcha #4 actually requires.)
            _slotSwitchBinder = null;
        }

        /// <summary>The currently-configured switch key, falling back to Tab if the Configurable
        /// hasn't been bound yet (i.e. before PrimalOptions has run at all).</summary>
        public static KeyCode SlotSwitchKey => _slotSwitchKey?.Value ?? DefaultSlotSwitchKey;

        /// <summary>The key-binder control for this setting, built lazily once. Null before Bind
        /// has run.</summary>
        public static OpKeyBinder BuildSlotSwitchBinder(Vector2 pos, Vector2 size)
        {
            if (_slotSwitchKey == null)
            {
                return null;
            }
            return _slotSwitchBinder ?? (_slotSwitchBinder = new OpKeyBinder(_slotSwitchKey, pos, size));
        }
    }
}
