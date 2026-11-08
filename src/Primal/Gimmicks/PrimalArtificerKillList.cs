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
    // and sessionRecord.kills only holds the kills of THIS cycle -- the real, persistent, cumulative record is
    // SaveState.kills, only otherwise shown by the end-of-campaign StoryGameStatisticsScreen.
    //
    // 2026-11-08 rewrite -- the original version of this patch (preserved in git history) injected one FAKE kill
    // of every known creature type (except Rain Deer and Guardian) into this cycle's own kills list, so the
    // sleep/death screen would show every tracked species as "killed" regardless of whether that was ever true.
    // That's exactly what produced the bug reported directly: "end of cycle kills" showing everything killed,
    // which the user correctly identified as this mod "forcing the kill list" rather than reflecting reality --
    // see CreatureKillListGimmick.cs's own header comment for the parallel story on the separate "KILL LIST"
    // button/popup this mod adds, which hit (and was fixed from) the exact same root confusion: real,
    // cumulative, persistent kill data lives in SaveState.kills, not anything this mod should be faking.
    //
    // Fixed the same way: this patch now merges in REAL entries from SaveState.kills (every creature type this
    // character has ACTUALLY killed at least once, ever) that this cycle's own kills don't already contain --
    // no fake injection, no hardcoded "every known creature type" list, no Rain Deer/Guardian special case
    // needed at all. If SaveState.kills genuinely has zero entries for Rain Deer/Guardian (confirmed true for
    // the user's own save as of the previous round's investigation), this screen simply won't show them killed
    // either, for the same reason it's accurate for everything else: it's reading the real record, not faking
    // anything.
    //
    // The list handed to the screen is still a COPY -- the real sessionRecord.kills is never touched, so
    // nothing this patch adds can leak back into SaveState.AppendKills and double-count anything.
    internal static class PrimalArtificerKillList
    {
        internal static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("PrimalArtificerKillList");

        private static readonly FieldInfo SymbolDataField = AccessTools.Field(typeof(PlayerSessionRecord.KillRecord), "symbolData");

        private static bool _loggedFirstUse;

        // Is the death/sleep screen currently showing an Artificer run, on a Primal slot?
        internal static bool ShouldApply(Menu.Menu menu, out SaveState saveState)
        {
            saveState = null;
            if (menu == null || menu.manager == null || menu.manager.rainWorld == null) return false;
            if (!PrimalSlot.IsActive(menu.manager.rainWorld)) return false;
            if (!(menu is Menu.KarmaLadderScreen karmaLadderScreen)) return false;
            saveState = PrimalMenuAccess.GetSaveState(karmaLadderScreen);
            return saveState?.saveStateNumber != null && saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer;
        }

        // Same "critType|intData" key both this file and CreatureKillListGimmick.cs's own GetKillCount use to
        // identify a kills-list entry -- see that file's own header comment for why intData has to be part of
        // the key (plain Centipede kills split across several entries differing only by a cosmetic size
        // variant) rather than matching on critType alone.
        private static string KeyOf(IconSymbol.IconSymbolData data)
        {
            return data.critType?.value + "|" + data.intData;
        }

        private static string KeyOf(PlayerSessionRecord.KillRecord record)
        {
            if (SymbolDataField == null)
            {
                return null;
            }
            return KeyOf((IconSymbol.IconSymbolData)SymbolDataField.GetValue(record));
        }

        // Returns a NEW list: this cycle's real kills, then one real entry (from SaveState.kills, the
        // permanent record) for every creature type this cycle doesn't already show, that the character has
        // genuinely killed at least once, ever.
        internal static List<PlayerSessionRecord.KillRecord> Build(List<PlayerSessionRecord.KillRecord> real, SaveState saveState)
        {
            var result = real != null ? new List<PlayerSessionRecord.KillRecord>(real) : new List<PlayerSessionRecord.KillRecord>();
            var have = new HashSet<string>();
            if (SymbolDataField != null)
            {
                foreach (PlayerSessionRecord.KillRecord rec in result)
                {
                    have.Add(KeyOf(rec));
                }
            }

            int added = 0;
            List<KeyValuePair<IconSymbol.IconSymbolData, int>> permanentKills = saveState?.kills;
            if (permanentKills != null)
            {
                foreach (KeyValuePair<IconSymbol.IconSymbolData, int> kv in permanentKills)
                {
                    if (kv.Value <= 0)
                    {
                        continue;
                    }
                    string key = KeyOf(kv.Key);
                    if (have.Contains(key))
                    {
                        continue;
                    }
                    result.Add(new PlayerSessionRecord.KillRecord(kv.Key, default(EntityID), false));
                    have.Add(key);
                    added++;
                }
            }

            if (!_loggedFirstUse)
            {
                _loggedFirstUse = true;
                Log.LogInfo("Death/sleep screen kill list: " + (result.Count - added) + " real kill(s) this cycle + " + added + " from the permanent record (SaveState.kills).");
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
                if (!PrimalArtificerKillList.ShouldApply(menu, out SaveState saveState)) return;
                killsData = PrimalArtificerKillList.Build(killsData, saveState);
            }
            catch (Exception e)
            {
                PrimalArtificerKillList.Log.LogError("Full kill list failed (the normal list will show): " + e);
            }
        }
    }
}
