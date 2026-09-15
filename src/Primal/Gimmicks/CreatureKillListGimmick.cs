using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "Kill List" gimmick: once assigned to a character, every creature that character
    /// personally kills is checked off a fixed, curated roster of 56 tracked species, persisted
    /// for the rest of that save file. For now this only tracks progress -- there's no win
    /// condition or reward yet, per the user's own framing ("for now the goal will be to track
    /// every creature"). See KillListRegistry.All below for the full roster, and AI_CONTEXT.md's
    /// "Kill List gimmick, in detail" for how it was assembled (curated by the user from every
    /// CreatureTemplate.Type this game registers, cross-checked against the game's own in-game
    /// display-name strings for anything ambiguous) and exactly what was deliberately left off it.
    ///
    /// "Killed by [character]", precisely: vanilla's OWN Creature.killTag mechanism -- the same
    /// field the game itself already uses for Arena scoring and social-event bookkeeping (see
    /// Creature.SetKillTag/Die, both read from the decompiled reference source for this feature)
    /// -- rather than reimplementing kill attribution from scratch. It's set by the attacker's own
    /// code (Player.cs's melee/grab/thrown-weapon paths, Weapon.cs, Explosion.cs, SporeCloud.cs --
    /// all call target.SetKillTag(attacker)) shortly before Die() runs, and is read here from a
    /// Postfix on Die() itself, which already reads killTag at the very top of its own body for
    /// the same reason (confirmed via decompilation, not guessed). A creature that dies from
    /// something other than direct player action -- starvation, drowning, eaten by another
    /// creature, environmental hazards -- has no killTag pointing at a player and is correctly NOT
    /// counted: this is a "you killed it" list, not a "this died somewhere near you" list.
    ///
    /// Patched on Creature.Die() itself (the base class method) rather than Creature.Violence()
    /// deliberately -- checked first, and found that the vast majority of creature subclasses
    /// (Lizard, Scavenger, Vulture, ~35 files total) fully reimplement Violence() without ever
    /// calling base.Violence(), which would make a patch there silently miss most kills. Die()
    /// overrides, by contrast, consistently call base.Die() after their own extra cleanup
    /// (spot-checked ~15 of the ~36 files that override it) -- so a single patch on the base
    /// method gets real, broad coverage instead of needing one patch per creature subclass.
    ///
    /// Persisted the same way OneSpearGimmick.cs persists its pull count: one marker per
    /// (character, creature type) added to DeathPersistentSaveData.unrecognizedSaveStrings the
    /// first time that pair is recorded, round-tripping through save/load forever after via that
    /// class's own designed-for-this passthrough mechanism. Unlike OneSpear's indexed markers
    /// (which track a repeatable count), this only ever needs one marker per pair -- a kill either
    /// happened by now or it didn't.
    /// </summary>
    [HarmonyPatch(typeof(Creature), nameof(Creature.Die))]
    internal static class CreatureKillListGimmick
    {
        public const string ID = "CreatureKillList";

        // Creature.killTag is genuinely new surface for this codebase (nothing else reads it) --
        // resolved via reflection per Gotcha #1 discipline rather than trusted directly, unlike
        // e.g. Creature.dead/creatureTemplate.type/realizedCreature, which are already used
        // directly elsewhere in this codebase (ChainsGimmick.cs) and treated as proven.
        private static readonly FieldInfo KillTagField = AccessTools.Field(typeof(Creature), "killTag");

        // Guards against double-counting: Die() can run more than once on the same instance (the
        // base method's own body is guarded by "if (!dead) { ...one-time death logic... }", but a
        // Harmony Postfix still fires on every call regardless) -- same once-per-instance guard
        // shape TimeLimitGimmick.cs already uses, applied there to a player instead of an
        // arbitrary creature.
        private static readonly ConditionalWeakTable<Creature, object> _recorded = new ConditionalWeakTable<Creature, object>();

        private static void Postfix(Creature __instance)
        {
            if (__instance == null || !__instance.dead)
            {
                // Die() ran but this wasn't an actual (new) death this call.
                return;
            }
            if (_recorded.TryGetValue(__instance, out _))
            {
                return;
            }

            AbstractCreature killer = KillTagField?.GetValue(__instance) as AbstractCreature;
            Player player = killer?.realizedCreature as Player;
            if (player?.playerState == null || player.room?.game == null || !player.room.game.IsStorySession)
            {
                // Not killed by a player, or killed outside a real story-mode save (Arena/Sandbox
                // has no DeathPersistentSaveData to persist into, and PrimalSlot's whole design is
                // story-mode-only anyway).
                return;
            }

            RainWorld rainWorld = player.room.game.rainWorld;
            if (!PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            string characterID = player.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            CreatureTemplate.Type type = __instance.abstractCreature?.creatureTemplate?.type;
            if (type == null || !KillListRegistry.ByID.ContainsKey(type.value))
            {
                // Not one of the tracked species.
                return;
            }

            DeathPersistentSaveData saveData = player.room.game.GetStorySession?.saveState?.deathPersistentSaveData;
            if (saveData == null)
            {
                return;
            }

            // Marked recorded regardless of whether this was actually a new kill (RecordKill is
            // itself idempotent) -- the point of this guard is purely "don't do this whole lookup
            // twice for the same Die() call ever firing more than once", not to gate the save
            // write itself.
            _recorded.Add(__instance, null);

            if (KillListTracker.RecordKill(saveData, characterID, type.value))
            {
                Plugin.Log?.LogInfo($"[CreatureKillList] {characterID} killed a {type.value} for the first time");
            }
        }
    }

    /// <summary>One entry in the fixed, curated roster this gimmick tracks -- see
    /// CreatureKillListGimmick's own header comment for where the list came from and
    /// AI_CONTEXT.md for the full account of every name that needed disambiguating.</summary>
    internal readonly struct KillListEntry
    {
        public readonly string ID;
        public readonly string DisplayName;

        public KillListEntry(CreatureTemplate.Type type, string displayName)
        {
            ID = type?.value;
            DisplayName = displayName;
        }
    }

    internal static class KillListRegistry
    {
        // 56 entries, curated by the user from the full set of CreatureTemplate.Type values this
        // game registers (92 total across vanilla + every DLC) -- see AI_CONTEXT.md for the full
        // back-and-forth on what got matched to what and why. Deliberately excludes: the three
        // internal-only "ancestor" types (Slugcat, LizardTemplate, StandardGroundCreature) that no
        // real creature is ever actually spawned as; a handful of MSC/DLC-shared types not on the
        // user's list (HunterDaddy, FireBug, TrainLizard, TerrorLongLegs, BigJelly); and nearly
        // all of Watcher's own roster, none of which was requested. Easy to extend later -- just
        // add another entry below plus a corresponding KillListRegistry.All array slot.
        //
        // Display names mostly match the user's own preferred community terms (e.g. "Dropwig",
        // "Leviathan", "Guardian") rather than the internal CreatureTemplate.Type identifier or
        // even the game's own official translated name where the two differ (e.g. official name
        // for SpitLizard is "Caramel Lizard" -- matches here; official name for RotLoach is "Rot
        // Behemoth" -- not on this list at all, different creature).
        public static readonly KillListEntry[] All =
        {
            // Vanilla (43)
            new KillListEntry(CreatureTemplate.Type.GreenLizard, "Green Lizard"),
            new KillListEntry(CreatureTemplate.Type.PinkLizard, "Pink Lizard"),
            new KillListEntry(CreatureTemplate.Type.WhiteLizard, "White Lizard"),
            new KillListEntry(CreatureTemplate.Type.BlueLizard, "Blue Lizard"),
            new KillListEntry(CreatureTemplate.Type.YellowLizard, "Yellow Lizard"),
            new KillListEntry(CreatureTemplate.Type.RedLizard, "Red Lizard"),
            new KillListEntry(CreatureTemplate.Type.BlackLizard, "Black Lizard"),
            new KillListEntry(CreatureTemplate.Type.CyanLizard, "Cyan Lizard"),
            new KillListEntry(CreatureTemplate.Type.Salamander, "Salamander"),
            new KillListEntry(CreatureTemplate.Type.Vulture, "Vulture"),
            new KillListEntry(CreatureTemplate.Type.KingVulture, "King Vulture"),
            new KillListEntry(CreatureTemplate.Type.MirosBird, "Miros Bird"),
            new KillListEntry(CreatureTemplate.Type.SmallCentipede, "Small Centipede"),
            new KillListEntry(CreatureTemplate.Type.Centipede, "Centipede"),
            new KillListEntry(CreatureTemplate.Type.RedCentipede, "Red Centipede"),
            new KillListEntry(CreatureTemplate.Type.Centiwing, "Centiwing"),
            new KillListEntry(CreatureTemplate.Type.BigSpider, "Big Spider"),
            new KillListEntry(CreatureTemplate.Type.SpitterSpider, "Spitter Spider"),
            new KillListEntry(CreatureTemplate.Type.DropBug, "Dropwig"),
            new KillListEntry(CreatureTemplate.Type.BrotherLongLegs, "Brother Long Legs"),
            new KillListEntry(CreatureTemplate.Type.DaddyLongLegs, "Daddy Long Legs"),
            new KillListEntry(CreatureTemplate.Type.PoleMimic, "Pole Plant"),
            new KillListEntry(CreatureTemplate.Type.TentaclePlant, "Monster Kelp"),
            new KillListEntry(CreatureTemplate.Type.BigNeedleWorm, "Adult Noodlefly"),
            new KillListEntry(CreatureTemplate.Type.SmallNeedleWorm, "Infant Noodlefly"),
            new KillListEntry(CreatureTemplate.Type.Snail, "Snail"),
            new KillListEntry(CreatureTemplate.Type.Scavenger, "Scavenger"),
            new KillListEntry(CreatureTemplate.Type.EggBug, "Eggbug"),
            new KillListEntry(CreatureTemplate.Type.Leech, "Red Leech"),
            new KillListEntry(CreatureTemplate.Type.SeaLeech, "Sea Leech"),
            new KillListEntry(CreatureTemplate.Type.TubeWorm, "Grapple Worm"),
            new KillListEntry(CreatureTemplate.Type.Overseer, "Overseer"),
            new KillListEntry(CreatureTemplate.Type.Fly, "Batfly"),
            new KillListEntry(CreatureTemplate.Type.VultureGrub, "Vulture Grub"),
            new KillListEntry(CreatureTemplate.Type.LanternMouse, "Lantern Mouse"),
            new KillListEntry(CreatureTemplate.Type.Hazer, "Hazer"),
            new KillListEntry(CreatureTemplate.Type.TempleGuard, "Guardian"),
            new KillListEntry(CreatureTemplate.Type.JetFish, "Jetfish"),
            new KillListEntry(CreatureTemplate.Type.GarbageWorm, "Garbage Worm"),
            new KillListEntry(CreatureTemplate.Type.Deer, "Rain Deer"),
            new KillListEntry(CreatureTemplate.Type.BigEel, "Leviathan"),
            new KillListEntry(CreatureTemplate.Type.CicadaA, "White Squid"),
            new KillListEntry(CreatureTemplate.Type.CicadaB, "Black Squid"),

            // DLC-shared (11)
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.ScavengerElite, "Elite Scavenger"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.MotherSpider, "Mother Spider"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.MirosVulture, "Miros Vulture"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.StowawayBug, "Stowaway"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.Inspector, "Inspector"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.Yeek, "Yeek"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.JungleLeech, "Jungle Leech"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.EelLizard, "Eel Lizard"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.SpitLizard, "Caramel Lizard"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.ZoopLizard, "Strawberry Lizard"),
            new KillListEntry(DLCSharedEnums.CreatureTemplateType.AquaCenti, "Coalescipede"),

            // MSC (2)
            new KillListEntry(MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing, "Chieftain Scavenger"),
            new KillListEntry(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, "Slugpup"),
        };

        public static readonly Dictionary<string, KillListEntry> ByID = BuildByID();

        private static Dictionary<string, KillListEntry> BuildByID()
        {
            var dict = new Dictionary<string, KillListEntry>();
            foreach (KillListEntry entry in All)
            {
                if (entry.ID != null)
                {
                    dict[entry.ID] = entry;
                }
            }
            return dict;
        }
    }

    /// <summary>Read/write access to which (character, creature type) kills have been recorded so
    /// far on the current save file. See CreatureKillListGimmick's own header comment for the
    /// persistence mechanism.</summary>
    internal static class KillListTracker
    {
        private const string SaveMarkerKey = "PRIMAL_KILLLIST";

        private static string Marker(string characterID, string creatureID)
        {
            return SaveMarkerKey + "<dpB>" + characterID + "<dpB>" + creatureID;
        }

        public static bool IsKilled(DeathPersistentSaveData saveData, string characterID, string creatureID)
        {
            if (saveData?.unrecognizedSaveStrings == null)
            {
                return false;
            }
            string marker = Marker(characterID, creatureID);
            foreach (string entry in saveData.unrecognizedSaveStrings)
            {
                if (entry == marker)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Records the kill if it isn't already recorded. Returns true only the first
        /// time this exact (character, creature) pair is ever recorded.</summary>
        public static bool RecordKill(DeathPersistentSaveData saveData, string characterID, string creatureID)
        {
            if (saveData == null || IsKilled(saveData, characterID, creatureID))
            {
                return false;
            }
            saveData.unrecognizedSaveStrings.Add(Marker(characterID, creatureID));
            return true;
        }
    }

    /// <summary>Read-only progress display for the Kill List gimmick's settings sub-view -- no
    /// Configurable&lt;T&gt; values at all (nothing here is a player-editable setting, it's a
    /// live status readout), so BindSettings is left null in GimmickRegistry.cs and this class
    /// only ever supplies BuildSettingsUI.</summary>
    internal static class KillListSettings
    {
        // PlayerProgression.currentSaveState and SaveState.saveStateNumber are both genuinely new
        // surface for this codebase -- resolved via reflection per Gotcha #1 discipline.
        // SaveState.deathPersistentSaveData, by contrast, is NOT new: OneSpearGimmick.cs already
        // reads it directly via a different path (game.GetStorySession.saveState.
        // deathPersistentSaveData) and is treated as proven, so it's read directly here too.
        private static readonly FieldInfo CurrentSaveStateField = AccessTools.Field(typeof(PlayerProgression), "currentSaveState");
        private static readonly FieldInfo SaveStateNumberField = AccessTools.Field(typeof(SaveState), "saveStateNumber");

        // Deliberately NOT cached per GimmickDef.BuildSettingsUI's usual "build once, cache
        // forever" contract (see GimmickDef.cs's own doc comment / Gotcha #4) -- that requirement
        // exists only for Configurable<T>-backed elements (OpCheckBox, OpUpdown, ...), which can
        // never be rebound to a second instance. Every element here is a plain OpLabel with no
        // Configurable behind it at all, so rebuilding fresh on every call is not just safe but
        // necessary -- it's the only way this view can ever show up-to-date kill progress instead
        // of a permanent snapshot from whenever it first happened to be opened.
        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            var items = new List<UIelement>();

            DeathPersistentSaveData saveData = TryGetCurrentSaveData(characterID);
            if (saveData == null)
            {
                items.Add(new OpLabel(20f, topY, "Load into a run on this character (on the Primal slot) to see kill progress here.", false));
                return items;
            }

            // 4 columns x 14 rows fits exactly 56 -- see AI_CONTEXT.md for the exact column
            // widths/whether the longest names (e.g. "Chieftain Scavenger") clip, unverified as of
            // this doc since this container has no way to render it live.
            const int columns = 4;
            const float colWidth = 130f;
            const float rowH = 20f;
            for (int i = 0; i < KillListRegistry.All.Length; i++)
            {
                KillListEntry entry = KillListRegistry.All[i];
                int col = i % columns;
                int row = i / columns;
                float x = 20f + col * colWidth;
                float y = topY - row * rowH;
                bool killed = KillListTracker.IsKilled(saveData, characterID, entry.ID);
                items.Add(new OpLabel(x, y, (killed ? "[X] " : "[ ] ") + entry.DisplayName, false));
            }

            return items;
        }

        // Only ever returns non-null when a save is ACTUALLY currently mounted for this exact
        // character -- a Rain World save slot holds one SaveState per character, but
        // PlayerProgression only ever keeps ONE of them loaded at a time (whichever character was
        // last selected/played), not all of them simultaneously. Returning the wrong character's
        // data would be actively misleading, so this returns null (falling back to the "load into
        // a run" placeholder above) rather than guess. Wrapped in try/catch, same reasoning as
        // PrimalOptions.GetHomeCharacterColor -- this runs while building the Remix menu itself,
        // so a failed reflection lookup should just fall back to the placeholder, not crash the
        // whole screen open.
        private static DeathPersistentSaveData TryGetCurrentSaveData(string characterID)
        {
            try
            {
                RainWorld rainWorld = Custom.rainWorld;
                PlayerProgression progression = rainWorld?.progression;
                if (progression == null)
                {
                    return null;
                }

                SaveState saveState = CurrentSaveStateField?.GetValue(progression) as SaveState;
                if (saveState == null)
                {
                    return null;
                }

                SlugcatStats.Name saveCharacter = SaveStateNumberField?.GetValue(saveState) as SlugcatStats.Name;
                if (saveCharacter?.value != characterID)
                {
                    return null;
                }

                return saveState.deathPersistentSaveData;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"KillListSettings.TryGetCurrentSaveData threw while reading progress for {characterID}: {ex}");
                return null;
            }
        }
    }
}
