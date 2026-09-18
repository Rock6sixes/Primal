using System;
using System.Collections.Generic;
using HarmonyLib;
using Menu.Remix.MixedUI;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "No Hold" gimmick -- a generalization of the old NoSpearsGimmick (removed 2026-09-18;
    /// see git history if the original single-purpose version is ever needed for reference) to a
    /// full category picker: the character it's assigned to can be individually blocked from
    /// picking up Spears, Rocks, Bombs, Lanterns, Fruits, Batflies, Squidcadas, Yeeks, Centipedes,
    /// and/or "Other" (everything else grabbable -- corpses, unique items, anything not covered by
    /// the first nine). Unlike NoSpears, which let each of the three spear sub-types be
    /// individually re-allowed, Spears here is a single category -- that finer-grained control was
    /// explicitly dropped per the user's own request when this gimmick was generalized.
    ///
    /// Same mechanism every other No* gimmick in this file uses (NoYeeksGimmick, NoCicadasGimmick):
    /// Postfixes Player.Grabability(PhysicalObject) -- an instance method ON THE PLAYER, so it can
    /// be scoped to exactly one character without touching anyone else's grab logic -- and forces
    /// the result to Player.ObjectGrabability.CantGrab for any object matching a category this
    /// character currently has blocked.
    ///
    /// Category membership is decided the same way NoSpears/NoYeeks/NoCicadas already did it: a
    /// plain C# `is` check against the real vanilla class (which also matches every subclass, e.g.
    /// `obj is Spear` also catches ExplosiveSpear/MoreSlugcats.ElectricSpear, `obj is Rock` also
    /// catches WaterNut) -- not CreatureTemplate.Type, and not a hand-maintained list of every
    /// concrete item class. Categories mapped from the reference game source
    /// (reference/GameSource), confirmed class-by-class rather than guessed:
    ///   - Spears: Spear (covers ExplosiveSpear, MoreSlugcats.ElectricSpear).
    ///   - Rocks: Rock (covers WaterNut -- the un-popped Bubble Fruit, which extends Rock in
    ///     vanilla and is thrown/used exactly like one; the popped, edible SwollenWaterNut is a
    ///     separate class that does NOT extend WaterNut/Rock, and is filed under Fruits instead).
    ///   - Bombs: every vanilla class whose own name literally is "*Bomb*" -- ScavengerBomb,
    ///     FlareBomb, GraffitiBomb, MoreSlugcats.SingularityBomb. Deliberately does NOT include
    ///     PuffBall or FirecrackerPlant (explosion-adjacent but not actually named/classed as
    ///     bombs) -- those fall under Other instead, same as every other unlisted item.
    ///   - Lanterns: Lantern (the carryable light stick) and LanternMouse (the friendly
    ///     lantern-carrying creature) -- LanternStick, the visual candle-stub object, is not a
    ///     PhysicalObject at all (UpdatableAndDeletable only) and was never grabbable to begin
    ///     with, so there's nothing to block there.
    ///   - Fruits: the vanilla food-plant items -- DangleFruit, Mushroom, SlimeMold,
    ///     SwollenWaterNut, MoreSlugcats.GlowWeed, MoreSlugcats.DandelionPeach,
    ///     MoreSlugcats.GooieDuck. Deliberately excludes EggBugEgg and KarmaFlower (edible per
    ///     IPlayerEdible, but neither is a "fruit") -- both fall under Other.
    ///   - Batflies: Fly (the small flying swarm creature -- "Batfly" is this game's own
    ///     community's nickname for it, same as "Squidcadas" below for Cicada; there is no
    ///     separate "Batfly" class).
    ///   - Squidcadas: Cicada (covers both the CicadaA/CicadaB CreatureTemplate.Type variants,
    ///     which are the one C# class -- same nickname and same class already used by
    ///     NoCicadasGimmick/ChainsGimmick's own excludeSquids preset flag).
    ///   - Yeeks: MoreSlugcats.Yeek -- same class NoYeeksGimmick already blocks outright.
    ///   - Centipedes: Centipede (one class shared by the Centipede/RedCentipede/SmallCentipede
    ///     CreatureTemplate.Type variants, same pattern as Cicada above).
    ///   - Other: computed, not a class list -- true for any grabbable object that didn't match
    ///     any of the nine categories above. This is what makes "corpses, unique items, anything
    ///     else" actually complete without having to enumerate every remaining vanilla class.
    ///
    /// Does NOT interfere with OneSpearGimmick's tail-pull (see NoSpearsGimmick's old header
    /// comment, preserved in git history, for why -- SlugcatGrab bypasses Grabability entirely) --
    /// a character with both OneSpear and NoHold's Spears category blocked (Spearmaster's own
    /// default, migrated from NoSpears below) can still complete their tail-pull(s).
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.Grabability), new Type[] { typeof(PhysicalObject) })]
    internal static class NoHoldGimmick
    {
        public const string ID = "NoHold";

        private static void Postfix(Player __instance, PhysicalObject obj, ref Player.ObjectGrabability __result)
        {
            if (__result == Player.ObjectGrabability.CantGrab)
            {
                // Already blocked by vanilla's own logic -- nothing to add.
                return;
            }

            if (!GimmickGate.IsActiveFor(__instance, ID, out string characterID))
            {
                return;
            }

            if (!NoHoldSettings.IsBlocked(characterID, obj))
            {
                return;
            }

            __result = Player.ObjectGrabability.CantGrab;
        }
    }

    /// <summary>Per-character settings for NoHoldGimmick: which of the ten categories this
    /// character currently can't pick up. All default to unblocked EXCEPT Spears, which defaults
    /// to blocked -- this is what NoSpears itself always defaulted to (every spear sub-type
    /// unallowed out of the box), preserved here so Spearmaster and Inv's existing default
    /// assignments (see GimmickDefaults.cs, migrated from NoSpears to NoHold the same day this
    /// gimmick was written) keep behaving exactly like before once NoSpears is gone.</summary>
    internal static class NoHoldSettings
    {
        private readonly struct CategoryDef
        {
            public readonly string Key;
            public readonly string Label;
            public readonly bool DefaultBlocked;
            public readonly Func<PhysicalObject, bool> Matches;

            public CategoryDef(string key, string label, bool defaultBlocked, Func<PhysicalObject, bool> matches)
            {
                Key = key;
                Label = label;
                DefaultBlocked = defaultBlocked;
                Matches = matches;
            }
        }

        // Order here is also the UI's checkbox order (2 columns x 5 rows) below.
        private static readonly CategoryDef[] Categories =
        {
            new CategoryDef("Spears", "Spears", true, obj => obj is Spear),
            new CategoryDef("Rocks", "Rocks", false, obj => obj is Rock),
            new CategoryDef("Bombs", "Bombs", false, obj =>
                obj is ScavengerBomb || obj is FlareBomb || obj is GraffitiBomb || obj is MoreSlugcats.SingularityBomb),
            new CategoryDef("Lanterns", "Lanterns", false, obj => obj is Lantern || obj is LanternMouse),
            new CategoryDef("Fruits", "Fruits", false, obj =>
                obj is DangleFruit || obj is Mushroom || obj is SlimeMold || obj is SwollenWaterNut ||
                obj is MoreSlugcats.GlowWeed || obj is MoreSlugcats.DandelionPeach || obj is MoreSlugcats.GooieDuck),
            new CategoryDef("Batflies", "Batflies", false, obj => obj is Fly),
            new CategoryDef("Squidcadas", "Squidcadas (cicadas)", false, obj => obj is Cicada),
            new CategoryDef("Yeeks", "Yeeks", false, obj => obj is MoreSlugcats.Yeek),
            new CategoryDef("Centipedes", "Centipedes", false, obj => obj is Centipede),
        };

        private static readonly Dictionary<string, Dictionary<string, Configurable<bool>>> _blocked = new Dictionary<string, Dictionary<string, Configurable<bool>>>();
        private static readonly Dictionary<string, Configurable<bool>> _blockOther = new Dictionary<string, Configurable<bool>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why (Configurable<T> can only ever be bound to one
        // UIconfig element at a time).
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            var perCategory = new Dictionary<string, Configurable<bool>>();
            foreach (CategoryDef category in Categories)
            {
                perCategory[category.Key] = owner.config.Bind(
                    characterID + "_NoHold_" + category.Key,
                    category.DefaultBlocked,
                    new ConfigurableInfo("Checked: this character can no longer pick up " + category.Label + "."));
            }
            _blocked[characterID] = perCategory;

            _blockOther[characterID] = owner.config.Bind(
                characterID + "_NoHold_Other",
                false,
                new ConfigurableInfo("Checked: this character can no longer pick up anything NOT covered by the categories above -- corpses, unique items, everything else grabbable."));
        }

        /// <summary>True if this character currently has the category obj belongs to blocked. An
        /// object matching none of the ten named categories falls to the "Other" toggle.</summary>
        public static bool IsBlocked(string characterID, PhysicalObject obj)
        {
            if (!_blocked.TryGetValue(characterID, out Dictionary<string, Configurable<bool>> perCategory))
            {
                return false;
            }

            foreach (CategoryDef category in Categories)
            {
                if (!category.Matches(obj))
                {
                    continue;
                }
                return perCategory.TryGetValue(category.Key, out Configurable<bool> c) && c.Value;
            }

            return _blockOther.TryGetValue(characterID, out Configurable<bool> other) && other.Value;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            if (!_blocked.TryGetValue(characterID, out Dictionary<string, Configurable<bool>> perCategory) ||
                !_blockOther.TryGetValue(characterID, out Configurable<bool> other))
            {
                // Not bound yet -- don't cache an incomplete/empty result (see ChainsSettings for
                // the same guard and why).
                return new List<UIelement>();
            }

            var items = new List<UIelement>();
            items.Add(new OpLabel(20f, topY, "This character can no longer pick up (checked = blocked):", false));

            const float rowHeight = 36f;
            const float columnAX = 20f, columnALabelX = 56f;
            const float columnBX = 310f, columnBLabelX = 346f;
            const int rowsPerColumn = 5;
            float gridTop = topY - 36f;

            for (int i = 0; i < Categories.Length; i++)
            {
                CategoryDef category = Categories[i];
                if (!perCategory.TryGetValue(category.Key, out Configurable<bool> config))
                {
                    continue;
                }

                int column = i / rowsPerColumn;
                int row = i % rowsPerColumn;
                float x = column == 0 ? columnAX : columnBX;
                float labelX = column == 0 ? columnALabelX : columnBLabelX;
                float y = gridTop - (row * rowHeight);

                items.Add(new OpCheckBox(config, x, y));
                items.Add(new OpLabel(labelX, y + 4f, category.Label, false));
            }

            float otherY = gridTop - (rowsPerColumn * rowHeight) - 8f;
            items.Add(new OpCheckBox(other, columnAX, otherY));
            items.Add(new OpLabel(columnALabelX, otherY + 4f, "Other (everything else grabbable)", false));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
