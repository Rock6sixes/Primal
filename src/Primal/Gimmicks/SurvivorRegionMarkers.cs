namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared mechanism #1 from COMMUNITY_CHALLENGES.md: "region-scoped death-cause markers".
    ///
    /// Descension ("die in all 12 regions by floor-collision fall damage") is naturally suited to
    /// this mod's existing once-per-(character, key) marker pattern (PrimalSaveMarkers) rather than
    /// anything tracked only for the current cycle: the challenge is inherently multi-death (12
    /// separate deaths, almost certainly across many separate cycles/lives), so what has to persist
    /// is "which regions has this character ever died this way in", forever, the same shape as the
    /// Kill List's own "which species has this character ever killed" set.
    ///
    /// This file is intentionally thin: SurvivorEvents.OnFallDamageDeath already does the hard part
    /// (proving a death was specifically a fatal fall, not some other cause -- see that file's own
    /// doc comment), and PrimalSaveMarkers already does the hard part of persistence (the <dpB>
    /// format, the cache). All that's left here is wiring the two together and giving the eventual
    /// Survivor gimmick a clean read API, the same relationship SlugpupEndingMarker already has to
    /// PrimalSaveMarkers.
    /// </summary>
    internal static class SurvivorRegionMarkers
    {
        private const string FallDeathKey = "SurvivorFallDeath";

        private static bool _initialized;

        /// <summary>Subscribes to SurvivorEvents.OnFallDamageDeath. Idempotent -- safe to call more
        /// than once (RainWorldHooks.Postfix, which calls this, itself runs once per
        /// RainWorld.OnModsInit, which per that file's own doc comment is a one-shot setup point in
        /// practice, but costs nothing to guard anyway). Not a Harmony patch itself -- this is a
        /// plain C# event subscription, so it needs an explicit call site rather than PatchAll
        /// picking it up on its own, exactly like GimmickDefaults.Apply()/EnsureConfigBound() are
        /// already explicitly called from RainWorldHooks.Postfix instead of relying on some
        /// attribute-driven discovery that doesn't exist for ordinary method calls.</summary>
        public static void Init()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            SurvivorEvents.OnFallDamageDeath += (player, characterID, region) =>
            {
                DeathPersistentSaveData saveData = PrimalSaveMarkers.SaveDataFor(GimmickGate.GameFor(player));
                if (saveData != null)
                {
                    PrimalSaveMarkers.Add(saveData, PrimalSaveMarkers.Marker(FallDeathKey, characterID, region));
                }
            };
        }

        /// <summary>Has this character ever died to fall damage in this region (World.name
        /// acronym, e.g. "SU"), on this save file?</summary>
        public static bool HasDiedByFallIn(DeathPersistentSaveData saveData, string characterID, string regionAcronym)
        {
            return PrimalSaveMarkers.Has(saveData, PrimalSaveMarkers.Marker(FallDeathKey, characterID, regionAcronym));
        }
    }
}
