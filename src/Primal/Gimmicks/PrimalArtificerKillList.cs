using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using Menu;
using MoreSlugcats;

namespace Primal.Gimmicks
{
    // PRIMAL ARTIFICER: FULL KILL LIST ON THE DEATH / SLEEP SCREEN
    //
    // The kill list shown when you die or sleep is NOT the saved KILLS list. Verified from the game's code:
    //   SleepAndDeathScreen.GetDataFromGame does  new SleepScreenKills(menu, owner, pos, package.sessionRecord.kills)
    // and sessionRecord.kills only holds the kills of THIS cycle. (The saved cumulative list, SaveState.kills, is only
    // shown by the end-of-campaign StoryGameStatisticsScreen, which is why editing the save file changed nothing here.)
    //
    // So this patch rewrites that constructor's `killsData` argument: on a Primal slot, while playing Artificer, it
    // appends one kill of every creature type that THIS cycle's kills don't already contain, except Rain Deer ("Deer")
    // and Guardian ("TempleGuard"). The screen itself still decides what is drawable (SleepScreenKills only makes a
    // trophy for creatures where CreatureSymbol.DoesCreatureEarnATrophy is true).
    //
    // The list handed to the screen is a COPY. The real sessionRecord.kills is never touched, so the fake kills can't
    // leak into SaveState.AppendKills and get written into the save.
    internal static class PrimalArtificerKillList
    {
        internal static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("PrimalArtificerKillList");

        // Creature types of vanilla + More Slugcats (+ its shared-DLC list), from the game's own enums.
        // Deliberately not here: non-creature templates (StandardGroundCreature, Slugcat, LizardTemplate), SlugNPC,
        // Watcher-only creatures, and the two excluded below.
        private static readonly string[] Names =
        {
            "PinkLizard", "GreenLizard", "BlueLizard", "YellowLizard", "WhiteLizard", "RedLizard", "BlackLizard",
            "Salamander", "CyanLizard", "Fly", "Leech", "SeaLeech", "Snail", "Vulture", "GarbageWorm", "LanternMouse",
            "CicadaA", "CicadaB", "Spider", "JetFish", "BigEel", "TubeWorm", "DaddyLongLegs", "BrotherLongLegs",
            "TentaclePlant", "PoleMimic", "MirosBird", "Centipede", "RedCentipede", "Centiwing", "SmallCentipede",
            "Scavenger", "Overseer", "VultureGrub", "EggBug", "BigSpider", "SpitterSpider", "SmallNeedleWorm",
            "BigNeedleWorm", "DropBug", "KingVulture", "Hazer",
            "MirosVulture", "SpitLizard", "EelLizard", "MotherSpider", "TerrorLongLegs", "AquaCenti", "StowawayBug",
            "ScavengerElite", "Inspector", "Yeek", "BigJelly", "JungleLeech", "ZoopLizard",
            "HunterDaddy", "FireBug", "ScavengerKing", "TrainLizard",
        };

        // Rain Deer and Guardian: left out on purpose.
        private static readonly HashSet<string> Excluded = new HashSet<string> { "Deer", "TempleGuard" };

        private static readonly FieldInfo SaveStateField = AccessTools.Field(typeof(KarmaLadderScreen), "saveState");
        private static readonly FieldInfo StateNumberField = AccessTools.Field(typeof(SaveState), "saveStateNumber");
        private static readonly FieldInfo SymbolDataField = AccessTools.Field(typeof(PlayerSessionRecord.KillRecord), "symbolData");
        private static readonly FieldInfo CritTypeField = AccessTools.Field(typeof(IconSymbol.IconSymbolData), "critType");
        private static readonly FieldInfo IntDataField = AccessTools.Field(typeof(IconSymbol.IconSymbolData), "intData");

        private static List<IconSymbol.IconSymbolData> _extras;
        private static bool _loggedFirstUse;

        // Is the death/sleep screen currently showing an Artificer run, on a Primal slot?
        internal static bool ShouldApply(Menu.Menu menu)
        {
            if (menu == null || menu.manager == null || menu.manager.rainWorld == null) return false;
            if (!PrimalSlot.IsActive(menu.manager.rainWorld)) return false;
            if (SaveStateField == null || StateNumberField == null) return false;
            object saveState = SaveStateField.GetValue(menu);
            if (saveState == null) return false;
            SlugcatStats.Name number = StateNumberField.GetValue(saveState) as SlugcatStats.Name;
            return number != null && number == MoreSlugcatsEnums.SlugcatStatsName.Artificer;
        }

        // One IconSymbolData per creature type that is registered in this install (so DLC types vanish if the DLC is off).
        private static List<IconSymbol.IconSymbolData> GetExtras()
        {
            if (_extras != null) return _extras;
            var list = new List<IconSymbol.IconSymbolData>();
            foreach (string name in Names)
            {
                if (Excluded.Contains(name)) continue;
                CreatureTemplate.Type type = new CreatureTemplate.Type(name, false);
                if (type.Index == -1) continue;                          // not registered in this install
                if (name == "Centipede")                                 // the three Centipede sizes are separate kills
                    for (int size = 1; size <= 3; size++)
                        list.Add(new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, size));
                else
                    list.Add(new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, 0));
            }
            _extras = list;
            Log.LogInfo("Primal Artificer full kill list: " + list.Count + " creature entries available.");
            return list;
        }

        private static string KeyOf(IconSymbol.IconSymbolData data)
        {
            object crit = CritTypeField != null ? CritTypeField.GetValue(data) : null;
            object intData = IntDataField != null ? IntDataField.GetValue(data) : null;
            return (crit != null ? crit.ToString() : "?") + "|" + (intData != null ? intData.ToString() : "?");
        }

        // Returns a NEW list: this cycle's real kills, then one entry for every type not already in it.
        internal static List<PlayerSessionRecord.KillRecord> Build(List<PlayerSessionRecord.KillRecord> real)
        {
            var result = real != null ? new List<PlayerSessionRecord.KillRecord>(real) : new List<PlayerSessionRecord.KillRecord>();
            var have = new HashSet<string>();
            if (SymbolDataField != null)
                foreach (PlayerSessionRecord.KillRecord rec in result)
                    have.Add(KeyOf((IconSymbol.IconSymbolData)SymbolDataField.GetValue(rec)));

            int added = 0;
            foreach (IconSymbol.IconSymbolData data in GetExtras())
            {
                if (have.Contains(KeyOf(data))) continue;
                result.Add(new PlayerSessionRecord.KillRecord(data, default(EntityID), false));
                added++;
            }
            if (!_loggedFirstUse)
            {
                _loggedFirstUse = true;
                Log.LogInfo("Death/sleep screen kill list: " + (result.Count - added) + " real kill(s) this cycle + " + added + " added.");
            }
            return result;
        }
    }

    [HarmonyPatch]
    internal static class PrimalArtificerKillListHook
    {
        // SleepScreenKills has exactly one instance constructor; Harmony can't infer a constructor from a bare type.
        private static MethodBase TargetMethod()
        {
            foreach (ConstructorInfo c in AccessTools.GetDeclaredConstructors(typeof(SleepScreenKills)))
                if (!c.IsStatic) return c;
            return null;
        }

        // Parameter names (menu, owner, pos, killsData) match the real constructor, which is how Harmony binds them.
        private static void Prefix(Menu.Menu menu, ref List<PlayerSessionRecord.KillRecord> killsData)
        {
            try
            {
                if (!PrimalArtificerKillList.ShouldApply(menu)) return;
                killsData = PrimalArtificerKillList.Build(killsData);
            }
            catch (Exception e)
            {
                PrimalArtificerKillList.Log.LogError("Full kill list failed (the normal list will show): " + e);
            }
        }
    }
}
