namespace Primal
{
    /// <summary>
    /// The range of save slots this mod reserves for its gimmick-modified playthroughs: one per
    /// real vanilla slot, which slot number each one is, and whether the game is currently
    /// sitting on any of them.
    ///
    /// This mod's design: there are no separate "alt" SlugBase characters at all -- the
    /// character select screen always shows the real, ordinary vanilla roster, on every save
    /// slot. What differs on a reserved slot is that this mod's gimmicks (Misery, Slugpup,
    /// OneSpear -- see the Gimmicks folder) are allowed to modify whichever vanilla character
    /// you're actually playing, on top of everything vanilla about that character being
    /// completely intact: same SlugcatStats.Name, same StoryGameSession identity, same
    /// cutscenes, same gate permissions, same everything -- because nothing is ever faked or
    /// substituted. Every real (non-reserved) slot is 100% vanilla: none of this mod's gimmick
    /// code ever activates there.
    ///
    /// MULTIPLE PRIMAL SLOTS (2026-09-13 addition): originally there was exactly one reserved
    /// slot shared no matter which real slot you switched from, so Tab always landed on the same
    /// Primal save regardless of whether you started from real slot 1, 2, or 3. Requested change:
    /// "switch between slot 1 and primal slot 1 (666666)... when in slot 2 you switch between
    /// slot 2 and primal slot 2 (666667)... and so for slot 3" -- i.e. each real slot gets its
    /// own independent Primal counterpart. Rather than a fixed single ReservedSlot plus a
    /// separately-tracked "which real slot to return to" variable (the old LastRealSlot field,
    /// now removed), the reserved slot for real slot N is simply ReservedSlot + N
    /// (PrimalSlotFor/RealSlotFor below) -- a pure, stateless mapping in both directions, so
    /// there's nothing to get out of sync and no reason to special-case which real slot a given
    /// Primal slot belongs to. IsActive is now a range check (saveSlot >= ReservedSlot) instead
    /// of an exact-equality check, for the same "far outside anything else generates" reason the
    /// original constant was chosen: nothing else in the game or any other mod plausibly
    /// generates save-slot numbers anywhere near this range, at any width.
    /// </summary>
    internal static class PrimalSlot
    {
        /// <summary>
        /// First save slot reserved for this mod's gimmick-modified playthroughs -- real slot 0's
        /// own Primal counterpart; real slot 1's is ReservedSlot + 1, real slot 2's is
        /// ReservedSlot + 2, and so on (see PrimalSlotFor/RealSlotFor). Verified against
        /// RainWorld's own Options/PlayerProgression code: saveSlot is only ever used to build a
        /// filename string ("sav" + (saveSlot + 1), or "SJ_" + saveSlot for a Jolly folder) --
        /// never as an array index or anything else that would care about its magnitude -- so an
        /// arbitrarily large slot number is completely safe numerically. Deliberately chosen far
        /// outside anything Rain World's own UI, a platform's save-slot picker, or any other mod
        /// is ever plausibly going to generate (those are always small integers -- 0/1/2 for
        /// story slots, small negative numbers for Expedition) so an accidental collision with
        /// something else is not a realistic risk, at any width of reserved range.
        /// </summary>
        public const int ReservedSlot = 666666;

        /// <summary>True while the given RainWorld instance currently has any Primal-reserved
        /// slot mounted (real slot 0/1/2/...'s own Primal counterpart alike) -- the single gate
        /// every gimmick patch checks before doing anything. A plain >= check rather than
        /// equality against one fixed slot number, now that there's one reserved slot per real
        /// slot -- see this class's header comment, MULTIPLE PRIMAL SLOTS.</summary>
        public static bool IsActive(RainWorld rainWorld)
        {
            return rainWorld?.options != null && rainWorld.options.saveSlot >= ReservedSlot;
        }

        /// <summary>The dedicated Primal-gimmicks slot number for real save slot `realSlot` (0/1/2
        /// for vanilla's three save slots): ReservedSlot itself for slot 0, ReservedSlot + 1 for
        /// slot 1, and so on. Used by Gimmicks/PrimalSlotTabSwitchHook.cs and
        /// Gimmicks/PrimalSlotOptionsMenuHooks.cs instead of a single shared reserved slot, so
        /// each real slot's Primal playthrough stays independent of the others'. See this class's
        /// header comment, MULTIPLE PRIMAL SLOTS.</summary>
        public static int PrimalSlotFor(int realSlot)
        {
            return ReservedSlot + realSlot;
        }

        /// <summary>The inverse of PrimalSlotFor: which real slot a given Primal slot belongs to.
        /// Only meaningful when the slot being converted is already known to satisfy IsActive
        /// (i.e. saveSlot >= ReservedSlot) -- callers that switch away from a Primal slot always
        /// check that first.</summary>
        public static int RealSlotFor(int primalSlot)
        {
            return primalSlot - ReservedSlot;
        }
    }
}
