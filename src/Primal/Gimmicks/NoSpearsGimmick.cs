using System;
using System.Collections.Generic;
using HarmonyLib;
using Menu.Remix.MixedUI;

namespace Primal.Gimmicks
{
    /// <summary>
    /// The "No Spears" gimmick: spears become impossible to pick up for the character it's
    /// assigned to -- by default none of the three spear types the game has can be picked up, but
    /// each can be individually re-allowed in this gimmick's settings (see NoSpearsSettings).
    ///
    /// Vanilla decides whether a player can pick a given object up through
    /// Player.Grabability(PhysicalObject) -- an instance method ON THE PLAYER (not a property on
    /// the object itself), called from every grab-related spot in Player's own class
    /// (GrabUpdate, the throw/swap logic, etc.) to decide what kind of grab (if any) is allowed.
    /// Because it's keyed on which player is asking, it can be overridden for exactly one
    /// character without touching how anyone else interacts with spears.
    ///
    /// This Postfixes that method: if the object being asked about "is Spear" (the base type,
    /// which also covers everything that extends it) and the asking player has this gimmick
    /// active, the specific spear TYPE is checked against NoSpearsSettings -- if that type isn't
    /// allowed, the result is forced to Player.ObjectGrabability.CantGrab, overriding whatever
    /// vanilla would have said (typically OneHand for a plain spear).
    ///
    /// The game has exactly three concrete spear types (confirmed by disassembling the reference
    /// assembly): the base "Spear" class (plain wood/rock spears, and also Spearmaster's own
    /// tail-pull needle spear -- see spearmasterNeedle on Spear itself, a flag on a regular Spear
    /// rather than a separate type), "ExplosiveSpear", and "MoreSlugcats.ElectricSpear" (both of
    /// which extend Spear). This gimmick treats those three as the selectable categories.
    ///
    /// This does NOT interfere with OneSpearGimmick's tail-pull mechanic: that creates the spear
    /// and attaches it via Player.SlugcatGrab(...), a separate, lower-level "put this in the
    /// creature's hand" method that vanilla itself uses for forced pickups -- it never calls
    /// Grabability at all. So a character with both OneSpear and NoSpears assigned (as
    /// Spearmaster does by default) can still complete their tail-pull(s), but can never pick up
    /// a spear lying on the ground or thrown by someone else, before or after.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.Grabability), new Type[] { typeof(PhysicalObject) })]
    internal static class NoSpearsGimmick
    {
        public const string ID = "NoSpears";

        private static void Postfix(Player __instance, PhysicalObject obj, ref Player.ObjectGrabability __result)
        {
            if (__result == Player.ObjectGrabability.CantGrab)
            {
                // Already blocked by vanilla's own logic -- nothing to add.
                return;
            }

            if (!(obj is Spear))
            {
                return;
            }

            Player self = __instance;
            if (self?.playerState == null)
            {
                return;
            }

            RainWorld rainWorld = self.room?.game?.rainWorld;
            if (rainWorld == null || !PrimalSlot.IsActive(rainWorld))
            {
                return;
            }

            string characterID = self.playerState.slugcatCharacter?.value;
            if (!GimmickAssignments.IsEnabled(characterID, ID))
            {
                return;
            }

            if (NoSpearsSettings.IsAllowed(characterID, obj))
            {
                return;
            }

            __result = Player.ObjectGrabability.CantGrab;
        }
    }

    /// <summary>Per-character settings for NoSpearsGimmick: which of the three spear types are
    /// still allowed to be picked up. All three default to "not allowed", matching the gimmick's
    /// original blanket behavior.</summary>
    internal static class NoSpearsSettings
    {
        private static readonly Dictionary<string, Configurable<bool>> _allowNormal = new Dictionary<string, Configurable<bool>>();
        private static readonly Dictionary<string, Configurable<bool>> _allowExplosive = new Dictionary<string, Configurable<bool>>();
        private static readonly Dictionary<string, Configurable<bool>> _allowElectric = new Dictionary<string, Configurable<bool>>();

        // Built lazily, once per character, and reused on every later call -- see GimmickDef.cs's
        // BuildSettingsUI doc comment for why (Configurable<T> can only ever be bound to one
        // UIconfig element at a time; building fresh OpCheckBox instances every time this sub-view
        // opened threw Menu.Remix.MultiuseConfigurableException the second time around).
        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        public static void Bind(OptionInterface owner, string characterID)
        {
            _allowNormal[characterID] = owner.config.Bind(
                characterID + "_NoSpears_allowNormal",
                false,
                new ConfigurableInfo("Allow picking up plain (non-explosive, non-electric) spears."));

            _allowExplosive[characterID] = owner.config.Bind(
                characterID + "_NoSpears_allowExplosive",
                false,
                new ConfigurableInfo("Allow picking up explosive spears."));

            _allowElectric[characterID] = owner.config.Bind(
                characterID + "_NoSpears_allowElectric",
                false,
                new ConfigurableInfo("Allow picking up electric spears."));
        }

        public static bool IsAllowed(string characterID, PhysicalObject obj)
        {
            if (obj is ExplosiveSpear)
            {
                return GetOrDefault(_allowExplosive, characterID);
            }
            if (obj is MoreSlugcats.ElectricSpear)
            {
                return GetOrDefault(_allowElectric, characterID);
            }
            return GetOrDefault(_allowNormal, characterID);
        }

        private static bool GetOrDefault(Dictionary<string, Configurable<bool>> table, string characterID)
        {
            return table.TryGetValue(characterID, out Configurable<bool> c) && c.Value;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            var items = new List<UIelement>();
            if (!_allowNormal.TryGetValue(characterID, out Configurable<bool> normal) ||
                !_allowExplosive.TryGetValue(characterID, out Configurable<bool> explosive) ||
                !_allowElectric.TryGetValue(characterID, out Configurable<bool> electric))
            {
                // Not bound yet -- don't cache an incomplete/empty result.
                return items;
            }

            items.Add(new OpLabel(20f, topY, "Spear types this character can still pick up:", false));

            items.Add(new OpCheckBox(normal, 20f, topY - 40f));
            items.Add(new OpLabel(56f, topY - 36f, "Plain spears", false));

            items.Add(new OpCheckBox(explosive, 20f, topY - 76f));
            items.Add(new OpLabel(56f, topY - 72f, "Explosive spears", false));

            items.Add(new OpCheckBox(electric, 20f, topY - 112f));
            items.Add(new OpLabel(56f, topY - 108f, "Electric spears", false));

            _uiCache[characterID] = items;
            return items;
        }
    }
}
