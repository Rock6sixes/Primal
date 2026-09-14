using System.Collections.Generic;

namespace AltCharacters.Gimmicks
{
    /// <summary>
    /// Runtime source of truth for "is gimmick G enabled on character C". Every gimmick's
    /// Harmony patch calls IsEnabled() to decide whether to do anything for a given player.
    ///
    /// There are two layers, cheapest first:
    ///  1. A Remix-configured Configurable&lt;bool&gt;, if AltCharactersOptions has built one for
    ///     this (character, gimmick) pair and the player has opened Remix at least once this
    ///     session (Configurable defaults to its bound default value either way).
    ///  2. The mod's own hardcoded default (SetDefault), used before Remix has bound anything
    ///     (e.g. if the options screen is never opened) and as the Configurable's initial value.
    ///
    /// Register()/SetDefault() are called once at load (AltCharactersOptions.Initialize and
    /// this class's static constructor, respectively); IsEnabled() is called every frame per
    /// player by the gimmick patches, so it deliberately does no allocation.
    /// </summary>
    internal static class GimmickAssignments
    {
        private static readonly Dictionary<string, bool> _defaults = new Dictionary<string, bool>();
        private static readonly Dictionary<string, Configurable<bool>> _overrides = new Dictionary<string, Configurable<bool>>();

        private static string Key(string characterID, string gimmickID) => characterID + "|" + gimmickID;

        /// <summary>Sets the mod-defined default for whether a gimmick is enabled on a
        /// character absent any Remix configuration. Call during startup, before any
        /// AltCharactersOptions instance is constructed (its checkboxes read this as their
        /// initial value).</summary>
        public static void SetDefault(string characterID, string gimmickID, bool enabled)
        {
            _defaults[Key(characterID, gimmickID)] = enabled;
        }

        public static bool GetDefault(string characterID, string gimmickID)
        {
            return _defaults.TryGetValue(Key(characterID, gimmickID), out bool value) && value;
        }

        /// <summary>Called by AltCharactersOptions once it has bound a Remix checkbox for this
        /// pair, so IsEnabled() can start reading the live, user-editable value.</summary>
        public static void Register(string characterID, string gimmickID, Configurable<bool> configurable)
        {
            _overrides[Key(characterID, gimmickID)] = configurable;
        }

        /// <summary>Whether the given gimmick is currently enabled for the given character.
        /// Safe to call every frame; does not allocate.</summary>
        public static bool IsEnabled(string characterID, string gimmickID)
        {
            if (characterID == null || gimmickID == null)
            {
                return false;
            }
            string key = Key(characterID, gimmickID);
            if (_overrides.TryGetValue(key, out Configurable<bool> configurable))
            {
                return configurable.Value;
            }
            return _defaults.TryGetValue(key, out bool value) && value;
        }
    }
}
