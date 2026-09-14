namespace AltCharacters
{
    /// <summary>
    /// The one save slot this mod reserves for its gimmick-modified playthroughs: which slot
    /// number it is, and whether the game is currently sitting on it.
    ///
    /// This mod's design: there are no separate "alt" SlugBase characters at all -- the
    /// character select screen always shows the real, ordinary vanilla roster, on every save
    /// slot. What differs on this one reserved slot is that this mod's gimmicks (Misery,
    /// Slugpup, OneSpear -- see the Gimmicks folder) are allowed to modify whichever vanilla
    /// character you're actually playing, on top of everything vanilla about that character
    /// being completely intact: same SlugcatStats.Name, same StoryGameSession identity, same
    /// cutscenes, same gate permissions, same everything -- because nothing is ever faked or
    /// substituted. Every other slot is 100% vanilla: none of this mod's gimmick code ever
    /// activates there.
    /// </summary>
    internal static class AltSlot
    {
        /// <summary>
        /// Save slot reserved for this mod's gimmick-modified playthroughs. Verified against
        /// RainWorld's own Options/PlayerProgression code: saveSlot is only ever used to build a
        /// filename string ("sav" + (saveSlot + 1), or "SJ_" + saveSlot for a Jolly folder) --
        /// never as an array index or anything else that would care about its magnitude -- so an
        /// arbitrarily large slot number is completely safe numerically. Deliberately chosen far
        /// outside anything Rain World's own UI, a platform's save-slot picker, or any other mod
        /// is ever plausibly going to generate (those are always small integers -- 0/1/2 for
        /// story slots, small negative numbers for Expedition) so an accidental collision with
        /// something else is not a realistic risk.
        /// </summary>
        public const int ReservedSlot = 666666;

        /// <summary>True while the given RainWorld instance currently has the reserved slot
        /// mounted -- the single gate every gimmick patch checks before doing anything.</summary>
        public static bool IsActive(RainWorld rainWorld)
        {
            return rainWorld?.options != null && rainWorld.options.saveSlot == ReservedSlot;
        }
    }
}
