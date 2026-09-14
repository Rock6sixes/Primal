using System;
using System.Collections.Generic;
using Menu.Remix.MixedUI;

namespace AltCharacters.Gimmicks
{
    /// <summary>
    /// Static metadata describing one gimmick: its stable config-key ID, the label shown in the
    /// Remix menu, a short description, and (optionally) its own per-character settings UI.
    /// Gimmicks themselves are implemented as separate Harmony patch classes (see
    /// MiseryGimmick.cs, SlugpupGimmick.cs) that each check
    /// GimmickAssignments.IsEnabled(characterID, ID) before doing anything -- this class only
    /// carries the bookkeeping needed to list, assign, and (for gimmicks that have them) configure
    /// them from AltCharactersOptions's per-character drill-down menu.
    ///
    /// A gimmick with settings beyond plain enabled/disabled (e.g. TimeLimitGimmick's minutes and
    /// cycle/run toggle) supplies BindSettings + BuildSettingsUI; a gimmick with nothing to
    /// configure (Misery, Slugpup, as of this writing) leaves both null, and
    /// AltCharactersOptions shows a plain "no settings" message in its place instead.
    /// </summary>
    internal sealed class GimmickDef
    {
        /// <summary>Stable identifier used as part of the Remix config key. Letters/digits only
        /// (no spaces or punctuation) -- see OptionInterface.ConfigHolder.Bind's key validation.</summary>
        public readonly string ID;

        /// <summary>Short label shown as this gimmick's button text in the per-character gimmick
        /// table, and in the "add a gimmick" picker.</summary>
        public readonly string DisplayName;

        /// <summary>One or two sentences shown as a tooltip description.</summary>
        public readonly string Description;

        /// <summary>
        /// Optional. Called once per real vanilla character during AltCharactersOptions.Initialize
        /// (regardless of whether that character currently has this gimmick assigned -- exactly
        /// like the plain enabled/disabled flag, every setting is bound eagerly up front so it
        /// exists before it's ever shown). Use this to config.Bind(...) any extra per-character
        /// Configurable&lt;T&gt; values this gimmick's settings need, and stash them somewhere
        /// this gimmick's own Harmony patches and BuildSettingsUI can both read them back from
        /// (a static Dictionary&lt;string /*characterID*/, Configurable&lt;T&gt;&gt; per setting,
        /// by convention -- see TimeLimitGimmick.cs's TimeLimitSettings for the pattern).
        /// Leave null if this gimmick has no settings.
        /// </summary>
        public readonly Action<OptionInterface, string> BindSettings;

        /// <summary>
        /// Optional. Builds this gimmick's settings sub-view for one specific character, as a
        /// flat list of already-positioned UIelements starting at the given top Y coordinate
        /// (elements should lay out downward from there, same convention as the rest of this
        /// mod's Remix screen). Leave null if this gimmick has no settings.
        ///
        /// IMPORTANT -- must build once per character and cache: a Configurable&lt;T&gt; can only
        /// ever be bound to ONE UIconfig-derived element (OpCheckBox, OpUpdown, etc.) at a time --
        /// constructing a second one against a Configurable that's already bound throws
        /// Menu.Remix.MultiuseConfigurableException. AltCharactersOptions calls this every time the
        /// settings sub-view for this character is (re)opened (it removes the old elements from the
        /// tab first via OpTab.RemoveItems, but that does NOT unbind them from their Configurable),
        /// so implementations must return the exact same element instances on every call for a
        /// given character rather than constructing fresh ones -- lazily build once into a
        /// per-character cache (e.g. Dictionary&lt;string, List&lt;UIelement&gt;&gt;) and return the
        /// cached list thereafter. Plain elements that aren't backed by a Configurable (OpLabel) are
        /// safe to rebuild, but for simplicity every existing *Settings class caches its whole
        /// returned list together. (This was found the hard way -- see AI_CONTEXT.md's Gotcha #4.)
        /// </summary>
        public readonly Func<string, float, List<UIelement>> BuildSettingsUI;

        public GimmickDef(
            string id,
            string displayName,
            string description,
            Action<OptionInterface, string> bindSettings = null,
            Func<string, float, List<UIelement>> buildSettingsUI = null)
        {
            ID = id;
            DisplayName = displayName;
            Description = description;
            BindSettings = bindSettings;
            BuildSettingsUI = buildSettingsUI;
        }

        /// <summary>Whether this gimmick has a settings sub-view at all (vs. a plain "no
        /// settings" message).</summary>
        public bool HasSettings => BuildSettingsUI != null;
    }
}
