using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Menu.Remix.MixedUI;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Primal.Gimmicks
{
    // The "Chains" gimmick: entering a room that has any (non-excluded) living creature in it
    // seals every one of that room's ordinary exits shut and traps you there -- visualized as
    // glowing void-chains running from every sealed doorway to a handful of the room's
    // creatures -- until every one of them is dead. Requested by the user as "the artificer
    // chains mod": a local, not-on-the-Workshop BepInEx plugin already installed on their own
    // machine (RainWorld_Data/StreamingAssets/mods/artificerhell/plugins/
    // artificer_rubicon_hell_bonanza.dll, mod id "lockeveryroom", display name "Artificer
    // Rubicon Hell Bonanza (STUPID HALF BAKED VERSION)", author "dannad") -- not anything found
    // on the Steam Workshop, which is why the initial search for a mod literally titled
    // "Artificer Chains" came up empty. Despite the name, nothing in the mod's own compiled code
    // gates it to Artificer specifically -- it hooks Player.NewRoom/Player.Update globally for
    // whichever character is currently active, so "Artificer" in its name/id is just this user's
    // own personal use-case (they play it on Artificer in a Rubicon run), not a real constraint.
    // That made porting it into Primal' own per-character gimmick-assignment system (this
    // file) a straightforward generalization rather than something that needed unpicking.
    //
    // Reverse-engineered the same way every other gimmick in this codebase has been: no
    // decompiler was available in this container (no dotnet/mono/ikdasm, pip installs blocked --
    // see AI_CONTEXT.md's "Container environment notes"), so the mod's own DLL
    // (artificer_rubicon_hell_bonanza.dll, staged from the user's machine via the device bridge)
    // was disassembled by hand with this session's hand-rolled ECMA-335 metadata/IL reader
    // (scratchpad/dnparse/dnmeta.py) -- reading its TypeDef/MethodDef/FieldDef tables and
    // disassembling every method on its two real logic classes, "MultiChain" (the chain-visual
    // wrapper) and "CustomHRGuardManager" (the lock/unlock state machine), plus its top-level
    // "Artificer_Hell" BepInEx plugin class (the six Harmony/MonoMod hooks that drive the other
    // two). Unlike SlugpupGimmick's stats-rescaling trace, nearly everything this gimmick touches
    // turned out to be genuinely public in the real game assembly (confirmed field-by-field and
    // method-by-method against the metadata's own accessibility flags, not assumed from the
    // publicized reference DLL -- see AI_CONTEXT.md's Gotcha #1 for why that distinction matters
    // here) -- Room.shortcuts, ShortcutData and its nested Type enum, MoreSlugcats.VoidChain and
    // its constructor, CreatureTemplate and its nested Type enum, AbstractCreatureAI, and
    // MoreSlugcatsEnums.MSCSoundID (itself a nested type -- MoreSlugcats.MoreSlugcatsEnums.
    // MSCSoundID -- exactly the same "nested enclosing type" gotcha SlugpupGimmick hit with
    // Player.NPCStats, caught here up front by checking the NestedClass table for every type this
    // file references before writing a single line of C#). The one real surprise: the "chains
    // should be Artificer-specific" assumption baked into the mod's own name doesn't hold up --
    // its EelLizard-banlist entry uses DLCSharedEnums.CreatureTemplateType.EelLizard, not
    // MoreSlugcatsEnums's own (different) CreatureTemplateType.EelLizard-shaped enum, which is
    // exactly the kind of easy-to-guess-wrong, easy-to-verify-against-metadata mistake this
    // project's whole disassemble-first workflow exists to catch.
    //
    // What got ported, faithfully, from CustomHRGuardManager/MultiChain:
    //  - On entering a room (detected here via a Player.Update Postfix comparing the player's
    //    current room against last tick's, rather than a separate Player.NewRoom hook -- see
    //    TimeLimitGimmick.cs for why this same single-hook-with-a-remembered-previous-value shape
    //    is already this codebase's convention for "notice a room change"): collect every living,
    //    non-in-shortcut, non-banned creature in the room (GetCreatureList). If there are any and
    //    the room isn't already locked, seal every room-to-room pipe and every creature den in the
    //    room (IsSealableExit, below -- RoomExit doors, the RegionTransportation pipe system, and
    //    CreatureHole dens; see this file's 2026-09-13 header note for the full room-to-room-vs-
    //    within-room breakdown) to ShortcutData.Type.DeadEnd (remembering each sealed index's own
    //    original type, not just its index), attach a chain visual (MultiChain, below) to up to
    //    MaxSimultaneousChains of those creatures, play the
    //    lock sound, and redirect ("confuse") any of them currently pathing to leave the room, or
    //    mid-way through fleeing into a shortcut back to their own den, so they can't just
    //    wander off screen and never die.
    //  - Every tick while locked: forcibly cancel the player's own attempt to walk into any of the
    //    sealed shortcuts (Creature.enteringShortCut), shoving them back into the room -- flipping
    //    the shortcut's type alone stops it functioning as a connection, but does not stop the
    //    player's body from physically entering the hole trigger and getting stuck, so this
    //    belt-and-suspenders check is load-bearing, not decorative (ported as-is). Then purge any
    //    creature that has died (or reached its own den, which counts as a forced death here too,
    //    same as the source mod -- otherwise a creature could despawn into its den and make the
    //    room permanently uncompletable) from the tracked list, disposing and reassigning chains
    //    as needed to keep MaxSimultaneousChains of the survivors visibly chained. Once the list is
    //    empty, or the (this port's own addition, see ChainsSettings below) configurable escape
    //    timer runs out, unlock: restore every sealed shortcut back to its own original type,
    //    refresh the room's shortcut graphics, dispose every chain visual, and play the unlock
    //    sound.
    //
    // 2026-09-13 correction: which shortcut types actually get sealed. First pass (below, since
    // corrected) only flipped ShortcutData.Type.RoomExit -- exactly matching the source mod's own
    // LockPlayer, re-disassembled to check rather than assumed -- but the user reported a room's
    // "internal" pipe-looking shortcuts (marked with a small triangle/arrow, as opposed to a plain
    // hole) stayed open and let the player and creatures walk straight out. Re-checked the source
    // mod's ENTIRE disassembly (every method on Artificer_Hell/CustomHRGuardManager/MultiChain, not
    // just LockPlayer) for every ShortcutData.Type it ever touches: it is, in fact, RoomExit and
    // DeadEnd only -- the fine-grained per-type distinction below is NOT something recovered from
    // the source mod's own compiled code (worth being explicit about: the source mod itself has the
    // exact same exploitable gap this port originally inherited). The categorization below is the
    // user's own, given directly after seeing the first fix still wasn't complete, and is what's
    // actually implemented now:
    //   room-to-room PIPES              -> LOCKED   (RoomExit, RegionTransportation)
    //   within-room SHORTCUTS           -> unlocked (Normal)
    //   creature DENS                   -> LOCKED   (CreatureHole)
    //   within-room TRANSPORT for creatures -> unlocked (NPCTransportation)
    // Dumped ShortcutData's nested Type enum directly from the game's own metadata (Field/Constant
    // tables, not guessed) to get the real, complete 6-value list (the fifth, DeadEnd, is the sealed
    // state itself and is never an input to this decision). Normal never leaves the room (a purely
    // in-room shortcut connecting two points inside the same room, used to travel across a big room
    // faster), so sealing it would do nothing for containment and would needlessly break ordinary
    // in-room traversal while locked. RegionTransportation is Rain World's long-distance pipe/tube
    // system -- the "internal-looking pipe that actually leaves the room" the user was pointing at
    // -- and does need sealing, same as an ordinary RoomExit door. NPCTransportation, despite the
    // similar name, is the *within-room* creature-transport variant (never leaves the room, purely
    // an AI/pathing convenience) and correctly stays open, same reasoning as Normal. CreatureHole is
    // a den entrance and DOES need sealing: relying only on UpdateLock's per-tick creature-state
    // belt-and-suspenders (doesNotUseDens, enteringShortCut cancellation, force-death on InDen) only
    // protects creatures already in state.CreatureList -- sealing the shortcut itself at the room
    // level closes the den to every creature in the room, tracked or not, the same "flip the type,
    // then also defend at the point of entry" redundancy this file already uses for RoomExit/player
    // shortcuts. IsSealableExit (below) now gates LockNow's seal loop on
    // RoomExit/RegionTransportation/CreatureHole. FreePlayer no longer hardcodes every restored
    // shortcut back to RoomExit either (that was only ever correct while RoomExit was the only type
    // ever sealed) -- ChainState remembers each sealed index's own original Type
    // (ChainedShortcutOriginalTypes) and restores that exact type on unlock, so a sealed region pipe
    // or den hole goes back to being exactly what it was, not a generic door. Verified
    // ShorcutEntranceHoleDirection (used by both UpdateLock's belt-and-suspenders shove and
    // MultiChain's own anchor placement) is purely geometric -- four-direction wall/terrain probing
    // around a tile, no ShortcutData.Type check anywhere in its own disassembly -- so sealing these
    // additional types carries no risk of breaking either call site.
    //
    // 2026-09-12 addition: creatures arriving after the room is already being tracked. The user
    // reported the exact gap this file originally documented as a deliberate omission -- a room
    // that was empty (or already cleared) when entered doesn't catch a creature that wanders in
    // later while the player is still there -- and asked for the source mod's own fix to be
    // reused rather than reinvented, since (per the user) it went through a lot of its own
    // iteration to get this right. Re-disassembled and ported `Artificer_Hell.Creature_NewRoom`
    // (the hook that reacts to any creature finishing a room transition) and
    // `CustomHRGuardManager.AddNewCreature` (what it calls) faithfully -- see
    // ChainsCreatureArrivalHook and ChainsGimmick.TryAddArrivingCreature below for the full trace,
    // including one genuine bug found in the source mod's own GetCreatureList (never ported: it's
    // missing a List.Clear() before re-scanning, so repeated calls would silently duplicate
    // already-tracked entries) that this port fixes rather than reproduces. The source mod's own
    // separate "genocideMode" Remix toggle, which gates that whole feature there, is now ported
    // too -- see the 2026-09-12 settings-import paragraph below.
    //
    // 2026-09-12 settings import: the source mod's full Remix options screen (Artificer_HellOptions
    // in its own compiled code), not just the lock/unlock mechanic. Requested by the user once the
    // core mechanic and the arrival-handling fix above were both confirmed working, specifically
    // calling out the source's own creature-allowlist selection as worth carrying over. Ported: its
    // four named banlist presets ("Funny"/"Hardcore"/"Casual"/"NoCritters") plus its separate
    // excludeVultures/excludeSquids toggles, collapsed into one checkbox per creature type
    // (ChainsSettings.BanOptions) rather than a preset dropdown; its "genocideMode" toggle, now
    // ChainsSettings.CatchLateArrivals, gating the arrival-handling feature above (source default
    // off, this port defaults it on -- see that field's own doc comment for why); and its
    // "huntedMode" toggle (ChainsSettings.FauxHuntedBurden), a genuinely global, not-lock-scoped
    // effect ported as a new ChainsHuntedModeHook patch on ArtificialIntelligence.Update. See
    // ChainsSettings' own doc comment for the full trace of every value each of these used to (and
    // now can) touch. Deliberately NOT ported: the source's own "logging" toggle (its internal
    // debug-log verbosity switch) -- no other gimmick in this codebase exposes a per-gimmick
    // logging option, and Plugin.Log already runs unconditionally everywhere else, so adding one
    // here would be new surface area nobody asked for, not a faithful port of existing behavior.
    //
    // Deliberately NOT ported, to keep this port's scope bounded and its behavior easy to reason
    // about (documented gaps, not oversights):
    //  - VultureAI.CreatureSpotted -- the source mod's OTHER creature-arrival path, which lets an
    //    already-circling vulture that merely *spots* the player (without itself entering the
    //    room) start or join a lock. Not ported; vultures/king vultures are still ordinary banned
    //    types (the "Vultures" checkbox, on by default) for exactly this reason, so behavior stays
    //    internally consistent regardless of which BanOptions checkboxes the player has changed.
    //  - The escape-timer safety net below is this port's own addition, not present in the source
    //    mod's default configuration (its own timer option defaults to disabled, i.e. no
    //    time-based way out at all). Added specifically because this is freshly-ported, not yet
    //    in-game-confirmed code, and an unconfirmed bug in creature/chain bookkeeping turning into
    //    a genuine, permanent soft-lock (sealed exits, forever) is a much worse failure mode than a
    //    generous timer occasionally letting the player off easy. See AI_CONTEXT.md's own running
    //    skepticism about every gimmick in this codebase staying unconfirmed until the user has
    //    actually tested it in-game.
    [HarmonyPatch(typeof(Player), nameof(Player.Update), new Type[] { typeof(bool) })]
    internal static class ChainsGimmick
    {
        public const string ID = "Chains";

        /// <summary>How many of a locked room's creatures get a visible chain at once. Ported
        /// verbatim from the source mod's own hardcoded cap (its AssignNewChain always keeps
        /// min(3, creatureList.Count) chains active) -- purely cosmetic/performance, every
        /// creature in the list still counts toward "room isn't cleared yet" whether or not it
        /// currently has a visible chain.</summary>
        private const int MaxSimultaneousChains = 3;

        // The fixed BannedTypes set that used to live here (modeled on the source mod's
        // "NoCritters" preset) was replaced 2026-09-12 by ChainsSettings' own per-character,
        // per-creature-type checkboxes (ChainsSettings.IsBanned) -- see ChainsSettings' doc
        // comment for the full trace of every creature type the source mod's four named presets
        // and two exclude-toggles ever touched, now individually configurable instead of fixed.

        private sealed class ChainState
        {
            /// <summary>Which room this state currently reflects -- compared against
            /// Player.room every tick to detect a room change, the same way TimeLimitGimmick's
            /// neighbor gimmicks in this file notice state changes without a dedicated
            /// Player.NewRoom hook.</summary>
            public Room Room;
            public readonly List<AbstractCreature> CreatureList = new List<AbstractCreature>();
            public readonly Dictionary<AbstractCreature, MultiChain> Chains = new Dictionary<AbstractCreature, MultiChain>();
            public readonly List<int> ChainedShortcuts = new List<int>();

            /// <summary>Each sealed index's Type as it was *before* LockNow flipped it to DeadEnd --
            /// keyed by the same index stored in ChainedShortcuts. FreePlayer restores each one to
            /// its own remembered type here instead of assuming they were all RoomExit (see this
            /// file's 2026-09-13 header note: that assumption stopped holding the moment
            /// RegionTransportation pipes and CreatureHole dens became sealable too).</summary>
            public readonly Dictionary<int, ShortcutData.Type> ChainedShortcutOriginalTypes = new Dictionary<int, ShortcutData.Type>();
            public bool Triggered;
            public int ActiveTime;

            /// <summary>Ported from CustomHRGuardManager.CreaturesToAdd: a newly-arriving creature
            /// spotted while the player is themselves mid-shortcut-transition is buffered here
            /// instead of processed immediately (see TryAddArrivingCreature's own comment for why),
            /// and flushed the next tick the player isn't mid-transition (FlushArrivalBuffer).
            /// </summary>
            public readonly List<Creature> CreaturesToAdd = new List<Creature>();
        }

        private static readonly ConditionalWeakTable<Player, ChainState> States = new ConditionalWeakTable<Player, ChainState>();

        private static void Postfix(Player __instance)
        {
            Player self = __instance;
            // Not GimmickGate.IsActiveFor: this gimmick needs the "assigned" answer as a value
            // rather than an early return, because turning it OFF mid-run has to actively release
            // a player it had already locked in a room (see below).
            if (!GimmickGate.TryGetCharacter(self, out string characterID))
            {
                return;
            }

            ChainState state = States.GetOrCreateValue(self);
            bool enabled = GimmickAssignments.IsEnabled(characterID, ID);

            if (!enabled)
            {
                if (state.Triggered)
                {
                    FreePlayer(self, state);
                }
                state.Room = null;
                return;
            }

            if (self.room == null)
            {
                return;
            }

            // 2026-09-13 audit addition: CustomHRGuardManager.Update's very first real check
            // (after an always-false, never-actually-set `wantsToDispose` flag -- confirmed dead
            // code, never assigned anywhere in the source's own compiled IL) is
            // `if (room.game.IsArenaSession) return;` -- the source disables itself completely
            // outside a story run. Not previously ported: this gimmick's own gate is
            // PrimalSlot.IsActive (rainWorld.options.saveSlot == ReservedSlot), which is a
            // *persistent* option that survives switching from a story campaign straight into
            // Arena/Sandbox without restarting the game -- so without this check, a room entered
            // in Arena mode while the last-used story slot happened to be Primal's reserved one
            // could still trigger a lock. Ported here as a hard early-out, matching the source's
            // own placement (before any room-change handling at all).
            if (self.room.game != null && self.room.game.IsArenaSession)
            {
                return;
            }

            if (!ReferenceEquals(state.Room, self.room))
            {
                // Room changed (or this is the first tick this gimmick has seen a room for this
                // player at all) -- whatever the previous room had locked is no longer relevant;
                // release it before evaluating the new one. Matches the source mod's own
                // Player_NewRoom hook disposing the old manager outright on every room change.
                if (state.Triggered)
                {
                    FreePlayer(self, state);
                }
                state.Room = self.room;

                // 2026-09-13 audit addition, then removed at the user's explicit request: the
                // source's Player_NewRoom hardcodes an exclusion for one specific vanilla room
                // ("GW_A24", Garbage Wastes), left completely unexplained in the source's own code.
                // Ported faithfully at first (see AI_CONTEXT.md's Chains section for the full
                // trace), then deliberately dropped -- the user doesn't want an unexplained,
                // hardcoded room-name special case in this mod, so every room is treated the same
                // regardless of name.
                TryLockRoom(self, state, characterID);
            }

            // Ported from Player_NewRoom's own CreaturesToAdd flush -- see
            // ChainsCreatureArrivalHook/TryAddArrivingCreature for why creatures can land in this
            // buffer at all. Checked every tick (rather than only on a room change) since nothing
            // about *this* player's own room state needs to change for a buffered entry to become
            // safe to process -- only the player no longer being mid-shortcut does.
            FlushArrivalBuffer(self, self.room, state, characterID);

            if (state.Triggered)
            {
                UpdateLock(self, state, characterID);
            }
        }

        private static void TryLockRoom(Player self, ChainState state, string characterID)
        {
            Room room = state.Room;
            if (room?.abstractRoom?.creatures == null)
            {
                return;
            }

            state.CreatureList.Clear();
            GetCreatureList(self, room, state.CreatureList, characterID);
            if (state.CreatureList.Count == 0)
            {
                return;
            }

            LockNow(room, state, characterID);
        }

        /// <summary>True for a shortcut Type that should be sealed while a room is locked: an
        /// ordinary room-to-room RoomExit door, Rain World's long-distance RegionTransportation
        /// pipe system (also a real room/region-to-room/region connection, just visually reading as
        /// an "internal" pipe fixture), and CreatureHole den entrances (a real place a creature can
        /// disappear to, even though it's the player who'd normally use a door). Deliberately
        /// excludes Normal (a purely in-room shortcut, connects two points in the same room, never
        /// leaves it) and NPCTransportation (despite the similarly "transportation"-sounding name,
        /// this is Rain World's *within-room* creature-transport shortcut, not a real exit) --
        /// sealing either would do nothing for containment and would just needlessly break in-room
        /// traversal/AI pathing while locked. See this file's 2026-09-13 header note for the full
        /// room-to-room-vs-within-room breakdown this is built from.</summary>
        private static bool IsSealableExit(ShortcutData.Type type)
        {
            return type == ShortcutData.Type.RoomExit
                || type == ShortcutData.Type.RegionTransportation
                || type == ShortcutData.Type.CreatureHole;
        }

        /// <summary>
        /// Seals every exit-like shortcut in the room (IsSealableExit, above), assigns chain
        /// visuals, plays the lock stinger, marks the room triggered, and confuses the tracked
        /// creatures -- factored out of TryLockRoom (which calls this after a full fresh
        /// GetCreatureList scan) so TryAddArrivingCreature can trigger the exact same lock sequence
        /// the instant a room that started with zero qualifying creatures gets its first one, using
        /// the CreatureList it has already built up to that point rather than re-scanning the whole
        /// room from scratch.
        /// </summary>
        private static void LockNow(Room room, ChainState state, string characterID)
        {
            state.ChainedShortcuts.Clear();
            state.ChainedShortcutOriginalTypes.Clear();
            for (int i = 0; i < room.shortcuts.Length; i++)
            {
                ShortcutData.Type type = room.shortcuts[i].shortCutType;
                if (IsSealableExit(type))
                {
                    state.ChainedShortcuts.Add(i);
                    state.ChainedShortcutOriginalTypes[i] = type;
                    room.shortcuts[i].shortCutType = ShortcutData.Type.DeadEnd;
                }
            }

            AssignNewChain(room, state);
            room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Lock, 0f, 1f, UnityEngine.Random.value * 0.5f + 0.8f);
            state.Triggered = true;
            state.ActiveTime = 0;
            ConfuseCreatures(room, state);

            Plugin.Log?.LogInfo($"[Chains] {characterID}: locked room {room.abstractRoom.name} -- {state.CreatureList.Count} creature(s), {state.ChainedShortcuts.Count} exit(s) sealed");
        }

        /// <summary>
        /// Entry point for ChainsCreatureArrivalHook: a creature (never the player) just finished
        /// entering `room`. Gated on ChainsSettings.CatchLateArrivals -- ported from the source
        /// mod's own "genocideMode" toggle, which gated this whole feature there too (defaulted
        /// off in the source; defaulted ON here instead, since this is now the confirmed, expected
        /// behavior -- see ChainsSettings' own doc comment on that field for why). If this player
        /// is currently tracking that exact room (whether or not it's triggered yet -- an
        /// untriggered, empty-so-far room is exactly the case the user originally reported as
        /// broken), hand it to TryAddArrivingCreature. Ported from Artificer_Hell.Creature_NewRoom's
        /// own `manager.room == room` gate.
        /// </summary>
        internal static void HandleCreatureArrival(Player self, Room room, Creature creature, string characterID)
        {
            if (!ChainsSettings.CatchLateArrivals(characterID))
            {
                return;
            }

            ChainState state = States.GetOrCreateValue(self);
            if (!ReferenceEquals(state.Room, room))
            {
                return;
            }

            TryAddArrivingCreature(self, room, state, creature, characterID);
        }

        /// <summary>
        /// Ported from CustomHRGuardManager.AddNewCreature. Deliberately NOT ported: that method's
        /// own GetCreatureList-adjacent duplicate-entry bug (repeated calls there never clear the
        /// list first) -- this port's own CreatureList additions are always guarded by a Contains
        /// check, so the same creature can never be double-counted.
        /// </summary>
        private static void TryAddArrivingCreature(Player self, Room room, ChainState state, Creature creature, string characterID)
        {
            AbstractCreature abstractCreature = creature?.abstractCreature;
            if (abstractCreature == null || creature.room != room || creature.dead)
            {
                return;
            }
            if (state.CreatureList.Contains(abstractCreature))
            {
                return;
            }

            // Same banlist ChainsSettings.IsBanned/GetCreatureList uses for the initial room scan
            // (per-character, user-configurable -- see ChainsSettings) -- the source mod's own
            // banlist HashSet is likewise shared between its GetCreatureList and AddNewCreature, so
            // whatever's excluded from triggering a lock is equally excluded from joining one
            // already in progress. Vultures/king vultures land here as an ordinary banned type by
            // default (the "Vultures" checkbox, on by default) rather than a hardcoded special
            // case, since VultureAI.CreatureSpotted (their other, separate recruitment path in the
            // source mod) still isn't ported -- see this file's header comment.
            CreatureTemplate template = abstractCreature.creatureTemplate;
            if (template == null || ChainsSettings.IsBanned(characterID, template.type))
            {
                return;
            }

            if (self.inShortcut)
            {
                // Ported as-is from AddNewCreature's own check (on the PLAYER's inShortcut, not
                // the arriving creature's) -- the player's own room transition may still be
                // settling in the same tick a creature arrives, so defer rather than act on a
                // room reference that could change again a moment later. Flushed by
                // FlushArrivalBuffer once the player is no longer mid-transition.
                if (!state.CreaturesToAdd.Contains(creature))
                {
                    state.CreaturesToAdd.Add(creature);
                }
                return;
            }

            if (creature is Lizard lizard && lizard.AI != null && lizard.AI.friendTracker?.friend == self)
            {
                // A tamed/friendly lizard -- ported as-is, don't lock the room over your own ally.
                return;
            }

            // Distinct, higher-pitched chime for "one more creature joined the lock" -- ported
            // as-is, deliberately different from LockNow's own full lock-down stinger.
            room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Lock, 0f, 0.8f, UnityEngine.Random.value * 0.5f + 5f);

            state.CreatureList.Add(abstractCreature);
            Plugin.Log?.LogInfo($"[Chains] {characterID}: {template.name} entered tracked room {room.abstractRoom.name} -- added ({state.CreatureList.Count} total)");

            if (!state.Triggered)
            {
                // The room was empty (or already cleared) when entered -- this arrival is what
                // starts the lock now, exactly the case the user reported as not working.
                LockNow(room, state, characterID);
            }
            else
            {
                AssignNewChain(room, state);
            }
        }

        private static void FlushArrivalBuffer(Player self, Room room, ChainState state, string characterID)
        {
            if (state.CreaturesToAdd.Count == 0 || self.inShortcut)
            {
                return;
            }

            // Snapshot first: TryAddArrivingCreature can re-buffer an entry (harmlessly) if the
            // player is somehow back in a shortcut by the time it runs, which would otherwise
            // mutate CreaturesToAdd while this loop is iterating it.
            List<Creature> pending = new List<Creature>(state.CreaturesToAdd);
            state.CreaturesToAdd.Clear();
            foreach (Creature creature in pending)
            {
                if (creature != null && creature.room == room)
                {
                    TryAddArrivingCreature(self, room, state, creature, characterID);
                }
            }
        }

        private static void GetCreatureList(Player self, Room room, List<AbstractCreature> into, string characterID)
        {
            foreach (AbstractCreature creature in room.abstractRoom.creatures)
            {
                if (creature?.state == null || creature.state.dead)
                {
                    continue;
                }
                // A creature in a currently-realized room's own abstractRoom.creatures list is
                // always itself realized too (realization happens per-room, not per-creature) --
                // same assumption the source mod's GetCreatureList makes (it calls
                // .realizedCreature.inShortcut unconditionally, with no null check at all).
                // Skipped defensively anyway rather than assumed, since a null here would
                // otherwise surface as a silent per-tick exception deep in Player.Update's own
                // patch chain (see AI_CONTEXT.md's Gotcha #1 on how quietly those get swallowed).
                if (creature.realizedCreature == null || creature.realizedCreature.inShortcut)
                {
                    continue;
                }
                CreatureTemplate template = creature.creatureTemplate;
                if (template == null || ChainsSettings.IsBanned(characterID, template.type))
                {
                    continue;
                }

                // 2026-09-13 audit addition: the source mod's own GetCreatureList never checks
                // for a friendly/tamed lizard at all -- only its separate AddNewCreature (this
                // file's TryAddArrivingCreature, which already has this exact check) and its
                // periodic DoRedundancyCreatureCheck sweep (not ported -- see this file's header
                // comment on why the rest of that method's self-healing machinery was skippable)
                // do. That's a real gap in the source itself: a lizard that's already your friend
                // BEFORE you even enter the room gets swept into the initial lock same as any
                // hostile creature, and only gets quietly dropped up to ~10 seconds later once the
                // redundancy sweep runs (400 ticks). Rather than port that whole belated-cleanup
                // mechanism just to close this one gap, apply the same exclusion TryAddArrivingCreature
                // already uses right here too, so a friendly lizard is never added in the first place.
                if (creature.realizedCreature is Lizard lizard && lizard.AI != null && lizard.AI.friendTracker?.friend == self)
                {
                    continue;
                }

                into.Add(creature);
            }
        }

        private static void ConfuseCreatures(Room room, ChainState state)
        {
            foreach (AbstractCreature creature in state.CreatureList)
            {
                if (creature.abstractAI != null && creature.abstractAI.destination.room != room.abstractRoom.index)
                {
                    creature.abstractAI.SetDestination(room.ToWorldCoordinate(room.RandomPos()));
                }

                Creature realized = creature.realizedCreature;
                if (realized != null
                    && creature.spawnDen.room == room.abstractRoom.index
                    && realized.enteringShortCut.HasValue)
                {
                    // About to flee into a shortcut back toward its own den -- cancel the
                    // transition so it stays in the room instead of despawning out of reach.
                    realized.enteringShortCut = null;
                }
            }
        }

        /// <summary>True if `tile` is the entrance tile of a shortcut this gimmick has sealed
        /// (i.e. its index is in state.ChainedShortcuts). Needed because Creature.enteringShortCut
        /// is a Nullable&lt;IntVector2&gt; -- the tile the body is entering through -- not an index
        /// into room.shortcuts (confirmed by dumping Creature's own field signature straight from
        /// the game assembly's metadata after `state.ChainedShortcuts.Contains(enteringShortCut
        /// .Value)` failed to compile with "cannot convert from IntVector2 to int": ChainedShortcuts
        /// is a List&lt;int&gt; of indices, a type mismatch the compiler caught immediately).
        /// ShortcutData.StartTile (also IntVector2, confirmed the same way) is the matching
        /// half -- same tile MultiChain already anchors its chain-visual off of for these same
        /// indices, just above.</summary>
        private static bool IsSealedShortcutTile(Room room, ChainState state, IntVector2 tile)
        {
            foreach (int index in state.ChainedShortcuts)
            {
                if (index >= 0 && index < room.shortcuts.Length && room.shortcuts[index].StartTile == tile)
                {
                    return true;
                }
            }
            return false;
        }

        private static void AssignNewChain(Room room, ChainState state)
        {
            int target = Math.Min(MaxSimultaneousChains, state.CreatureList.Count);
            for (int i = 0; i < state.CreatureList.Count && state.Chains.Count < target; i++)
            {
                AbstractCreature creature = state.CreatureList[i];
                if (state.Chains.ContainsKey(creature))
                {
                    continue;
                }
                state.Chains[creature] = new MultiChain(creature, room, state.ChainedShortcuts);
            }
        }

        private static void UpdateLock(Player self, ChainState state, string characterID)
        {
            Room room = state.Room;
            if (room == null)
            {
                FreePlayer(self, state);
                return;
            }

            // Belt-and-suspenders: flipping a shortcut's type to DeadEnd stops it functioning as
            // a room connection, but does not stop the player's body from physically entering the
            // hole trigger first -- so every tick, forcibly cancel a shortcut entry currently in
            // progress and shove the player back into the room instead. Ported as-is from
            // CustomHRGuardManager.Update (see this file's header comment) EXCEPT for the
            // IsSealedShortcutTile guard just below, which the source mod's own equivalent line
            // doesn't have: the source only ever seals RoomExit, so in its world "the player is
            // entering *a* shortcut" and "the player is entering *a sealed* shortcut" were the same
            // thing. That stopped being true here the moment Normal and NPCTransportation shortcuts
            // stayed open on purpose (2026-09-13 fix) -- without this guard, this cancelled EVERY
            // shortcut entry in a locked room indiscriminately, including the still-unlocked
            // within-room ones, which is why the user reported "normal shortcuts" (and,
            // symmetrically, NPCTransportation) staying unusable while locked even though
            // IsSealableExit correctly left them untouched at the room level.
            if (self.enteringShortCut.HasValue && IsSealedShortcutTile(room, state, self.enteringShortCut.Value))
            {
                IntVector2 holeDirection = room.ShorcutEntranceHoleDirection(self.enteringShortCut.Value);
                self.firstChunk.vel = holeDirection.ToVector2() * 4f;
                self.enteringShortCut = null;
            }

            state.ActiveTime++;

            int escapeTimerSeconds = ChainsSettings.GetEscapeTimerSeconds(characterID);
            if (escapeTimerSeconds > 0 && state.ActiveTime >= escapeTimerSeconds * 40)
            {
                Plugin.Log?.LogInfo($"[Chains] {characterID}: escape timer elapsed -- freeing room {room.abstractRoom.name} unfinished");
                FreePlayer(self, state);
                return;
            }

            for (int i = state.CreatureList.Count - 1; i >= 0; i--)
            {
                AbstractCreature creature = state.CreatureList[i];
                bool goneForGood = creature.state.dead;
                if (!goneForGood)
                {
                    // Ported as-is from CustomHRGuardManager.Update, applied every tick to every
                    // still-alive tracked creature (not just the player, and not just once at
                    // lock time): forbid it from retreating to a den at all -- this flag only
                    // ever affects den-seeking AI behavior, not shortcut usability in general, so
                    // it stays unconditional even now that not every shortcut type is sealed.
                    // Belt-and-suspenders alongside LockNow now also sealing every CreatureHole in
                    // the room outright (see this file's 2026-09-13 header note): that stops den
                    // shortcuts from functioning as a connection at all, room-wide, for tracked and
                    // untracked creatures alike, while this flag is what stops a TRACKED creature's
                    // own AI from choosing to path toward a den in the first place.
                    creature.creatureTemplate.doesNotUseDens = true;

                    // Same IsSealedShortcutTile guard as the player's own cancellation above, and
                    // for the same reason (see that comment): cancel a creature's shortcut entry
                    // ONLY when it's mid-way through one this gimmick actually sealed. Without this
                    // guard every tracked creature's use of a still-open Normal or NPCTransportation
                    // shortcut got cancelled every single tick too, same bug as the player's, just
                    // less visible since it reads as "stuck AI" rather than "can't leave through an
                    // obviously-still-open hole."
                    if (creature.realizedCreature != null
                        && creature.realizedCreature.enteringShortCut.HasValue
                        && IsSealedShortcutTile(room, state, creature.realizedCreature.enteringShortCut.Value))
                    {
                        creature.realizedCreature.enteringShortCut = null;
                    }

                    if (creature.InDen)
                    {
                        // Reached its own den anyway (already mid-transition when the lock
                        // triggered) -- force it dead outright rather than let it vanish into a
                        // den this gimmick can no longer see.
                        creature.Die();
                        goneForGood = true;
                    }
                }

                if (!goneForGood)
                {
                    continue;
                }

                state.CreatureList.RemoveAt(i);
                if (state.Chains.TryGetValue(creature, out MultiChain chain))
                {
                    chain.Dispose();
                    state.Chains.Remove(creature);
                }
            }

            if (state.CreatureList.Count == 0)
            {
                Plugin.Log?.LogInfo($"[Chains] {characterID}: room {room.abstractRoom.name} cleared -- freeing");
                FreePlayer(self, state);
                return;
            }

            AssignNewChain(room, state);
            foreach (MultiChain chain in state.Chains.Values)
            {
                chain.Update();
            }
        }

        private static void FreePlayer(Player self, ChainState state)
        {
            Room room = state.Room;
            if (room != null)
            {
                foreach (int shortcutIndex in state.ChainedShortcuts)
                {
                    if (shortcutIndex >= 0 && shortcutIndex < room.shortcuts.Length)
                    {
                        // Restore whatever this index actually was before LockNow sealed it --
                        // RoomExit for an ordinary door, RegionTransportation for a pipe, or
                        // CreatureHole for a den, never just hardcoded to RoomExit (see this file's
                        // 2026-09-13 header note for why that used to be silently wrong for pipes
                        // and dens alike).
                        ShortcutData.Type originalType = state.ChainedShortcutOriginalTypes.TryGetValue(shortcutIndex, out ShortcutData.Type t)
                            ? t
                            : ShortcutData.Type.RoomExit;
                        room.shortcuts[shortcutIndex].shortCutType = originalType;
                    }
                }
                if (state.ChainedShortcuts.Count > 0)
                {
                    room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Chain_Break, 0f, 1f, UnityEngine.Random.value * 0.5f + 0.8f);
                    RoomCamera[] cameras = room.game?.cameras;
                    if (cameras != null && cameras.Length > 0 && cameras[0]?.shortcutGraphics != null)
                    {
                        cameras[0].shortcutGraphics.NewRoom();
                    }
                }
            }

            foreach (MultiChain chain in state.Chains.Values)
            {
                chain.Dispose();
            }
            state.Chains.Clear();
            state.CreatureList.Clear();
            state.ChainedShortcuts.Clear();
            state.ChainedShortcutOriginalTypes.Clear();
            state.Triggered = false;
            state.ActiveTime = 0;
        }

        /// <summary>
        /// The chain visual for one trapped creature: one MoreSlugcats.VoidChain per sealed
        /// shortcut in the room, each running from that shortcut's entrance out to the creature's
        /// current position -- exactly MultiChain from the source mod (same class name, kept for
        /// traceability back to the disassembly), just re-typed into normal C# now that every
        /// member it touches has been confirmed genuinely public (see this file's header
        /// comment).
        /// </summary>
        private sealed class MultiChain
        {
            private readonly AbstractCreature _creature;
            private readonly Room _room;
            private readonly List<VoidChain> _chains = new List<VoidChain>();
            private int _activeTime;

            public MultiChain(AbstractCreature creature, Room room, List<int> chainedShortcuts)
            {
                _creature = creature;
                _room = room;

                // GetCreatureList only ever adds creatures with a non-null realizedCreature (see
                // its own comment for why that's safe to assume for any creature already in a
                // realized room's creature list), so this is never constructed for one that lacks
                // one.
                Vector2 creaturePos = creature.realizedCreature.mainBodyChunk.pos + new Vector2(0.01f, 0.01f);

                foreach (int shortcutIndex in chainedShortcuts)
                {
                    IntVector2 startTile = room.shortcuts[shortcutIndex].StartTile;
                    Vector2 anchor = room.MiddleOfTile(startTile) + room.ShorcutEntranceHoleDirection(startTile).ToVector2() * 15f;
                    VoidChain chain = new VoidChain(room, anchor, creaturePos);
                    room.AddObject(chain);
                    _chains.Add(chain);
                }
            }

            public void Update()
            {
                Creature realized = _creature.realizedCreature;
                if (realized == null)
                {
                    return;
                }

                Vector2 creaturePos = realized.mainBodyChunk.pos;
                // Same cosmetic pulse as the source mod: colorFlash/proximityAlpha breathe on a
                // slow sine wave keyed off how long this chain has been active, so a freshly
                // triggered lock visibly "tightens" rather than snapping to full brightness.
                float phase = Mathf.Sin(3.14159274f * (_activeTime / 400f) - 3.14159274f);
                float colorFlash = phase * 0.25f + 0.25f + 0.5f * Mathf.Min(1f, _activeTime / 80f);
                foreach (VoidChain chain in _chains)
                {
                    chain.stuckPosB = creaturePos;
                    chain.colorFlash = colorFlash;
                    chain.proximityAlpha = 0.5f;
                }
                _activeTime++;
            }

            public void Dispose()
            {
                foreach (VoidChain chain in _chains)
                {
                    _room.RemoveObject(chain);
                }
                _chains.Clear();
            }
        }
    }

    /// <summary>
    /// Ported from Artificer_Hell.Creature_NewRoom. Fires whenever ANY creature finishes entering
    /// a room -- this is the hook the rest of ChainsGimmick was missing: without it, a creature
    /// that wanders into a room after the player already entered it (and found it empty, so no
    /// lock ever triggered) was never noticed at all, exactly what the user reported as broken.
    /// Slugcats/players are skipped here (they have their own room-change handling driven by
    /// Player.Update's own Postfix above, matching Creature_NewRoom's own identical skip) --
    /// every other creature gets forwarded to ChainsGimmick.HandleCreatureArrival for each player
    /// in the room who currently has Chains enabled, which does the real work
    /// (TryAddArrivingCreature, ported from CustomHRGuardManager.AddNewCreature).
    /// </summary>
    [HarmonyPatch(typeof(Creature), nameof(Creature.NewRoom))]
    internal static class ChainsCreatureArrivalHook
    {
        private static void Postfix(Creature __instance, Room newRoom)
        {
            if (__instance == null || newRoom?.game?.Players == null)
            {
                return;
            }

            CreatureTemplate.Type type = __instance.abstractCreature?.creatureTemplate?.type
                ?? CreatureTemplate.Type.Slugcat;
            if (type == CreatureTemplate.Type.Slugcat)
            {
                return;
            }

            foreach (AbstractCreature abstractPlayer in newRoom.game.Players)
            {
                if (!(abstractPlayer?.realizedCreature is Player player) || player.room != newRoom)
                {
                    continue;
                }

                if (!GimmickGate.IsActiveFor(player, ChainsGimmick.ID, out string characterID))
                {
                    continue;
                }

                ChainsGimmick.HandleCreatureArrival(player, newRoom, __instance, characterID);
            }
        }
    }

    /// <summary>
    /// Ported from Artificer_Hell's own `On.ArtificialIntelligence.Update` hook (internal Remix
    /// toggle "huntedMode", shown as "Faux-hunted burden:") -- confirmed via disassembly to be a
    /// genuinely global, always-on-regardless-of-lock-state effect in the source, not something
    /// scoped only to a room Chains has currently sealed: every tick, for every creature's AI,
    /// walk the player list IN ORDER; the moment ANY player already shares this creature's room,
    /// stop entirely and do nothing (ordinary in-room AI/vision already handles a colocated
    /// player -- there's nothing for "hunted mode" to add). Otherwise, force the FIRST
    /// still-valid, different-room player onto this creature's Tracker (Tracker.SeeCreature),
    /// bypassing stealth/line-of-sight/distance entirely, then stop -- i.e. this is what makes
    /// creatures elsewhere on the map hunt the player across room boundaries, the actual point of
    /// "hunted mode". VultureAI is skipped, exactly as the source does (unexplained in the source
    /// itself, but kept as-is rather than second-guessed, per this project's own "port faithfully"
    /// rule for already-debugged behavior).
    ///
    /// 2026-09-13 audit correction: the room comparison below used to be backwards --
    /// `player.room != room` as the *skip* condition, i.e. this used to act ONLY on players
    /// already sharing the creature's room and silently do nothing for every other player. That
    /// is the exact inverse of the source's own `shortCutType`-unrelated but very deliberate
    /// `if (creature.Room == player.Room) return;` (confirmed via disassembly of
    /// ArtificialIntelligence_Update: same-room compares with `beq` branching straight to the
    /// method's own `ret`, before the VultureAI check or the SeeCreature call are ever reached).
    /// With the old, inverted check, this feature was never doing what "Faux-hunted burden" says
    /// it does -- it was closer to a no-op (instantly noticing a player already in the same room,
    /// which ordinary vision mostly already covers) instead of the intended "the whole area is
    /// secretly hunting you, even out of sight, out of room" pressure. Fixed to match the source's
    /// actual control flow: bail out entirely on any colocated player, act on the first
    /// still-qualifying player in a *different* room, and stop there (not "for every matching
    /// player", which the source doesn't do either -- SeeCreature fires once and `ret`s).
    ///
    /// Off by default (ChainsSettings.FauxHuntedBurden), matching the source's own default -- and
    /// gated on PrimalSlot/GimmickAssignments same as everywhere else, so it only ever fires for a
    /// player who both has Chains assigned AND has this specific checkbox on, not just from having
    /// Chains enabled at all. That per-player gate is this port's own addition (the source has a
    /// single global toggle, no per-character concept at all) -- deliberately checked only at the
    /// "act on this player" step, not at the "bail out on a colocated player" step, since the
    /// source's own bail-out is unconditional on any player sharing the room regardless of that
    /// player's own settings.
    /// </summary>
    [HarmonyPatch(typeof(ArtificialIntelligence), nameof(ArtificialIntelligence.Update))]
    internal static class ChainsHuntedModeHook
    {
        private static void Postfix(ArtificialIntelligence __instance)
        {
            if (__instance is VultureAI)
            {
                return;
            }

            AbstractCreature owner = __instance.creature;
            RainWorldGame game = owner?.world?.game;

            // Cheapest possible global gate, FIRST. This Postfix is the hottest patch in the mod --
            // ArtificialIntelligence.Update runs for every AI-driven creature in every realized
            // room, every tick, so in a busy room that's dozens of calls per tick. Until 2026-09-15
            // the PrimalSlot check lived down inside the per-player loop below, meaning every one
            // of those calls walked the room and iterated the whole player list before discovering
            // the mod wasn't even active. One comparison up here short-circuits all of it.
            if (game == null || !PrimalSlot.IsActive(game.rainWorld))
            {
                return;
            }

            // AbstractWorldEntity.Room (capital R) returns an AbstractRoom, not the realized Room
            // this needs -- the realized Creature's own `.room` field (same one TryAddArrivingCreature
            // and GetCreatureList already key off elsewhere in this file) is what's actually comparable
            // to another realized creature's room.
            Room room = owner.realizedCreature?.room;
            List<AbstractCreature> players = game.Players;
            if (room == null || players == null)
            {
                return;
            }

            foreach (AbstractCreature abstractPlayer in players)
            {
                if (!(abstractPlayer?.realizedCreature is Player player) || player.dead)
                {
                    continue;
                }

                if (player.room == room)
                {
                    // This creature already naturally shares a room with a player -- matches the
                    // source's own unconditional bail-out here (see this class's header comment):
                    // stop for this tick entirely rather than force-hunting a different, farther
                    // player instead.
                    return;
                }

                if (!GimmickGate.IsActiveFor(player, ChainsGimmick.ID, out string characterID)
                    || !ChainsSettings.FauxHuntedBurden(characterID))
                {
                    continue;
                }

                __instance.tracker?.SeeCreature(abstractPlayer);
                return;
            }
        }
    }

    /// <summary>
    /// Per-character settings for ChainsGimmick. Ported 2026-09-12 from the source mod's own full
    /// Remix settings screen (Artificer_HellOptions, disassembled the same way as everything else
    /// in this file) at the user's request ("the local artificer hell mod had a lot of development
    /// to fix a lot of bugs -- please reuse as much of that code as possible"), after the first
    /// round of in-game testing confirmed the core mechanic and the request specifically called
    /// out the source's own creature-allowlist selection as something worth carrying over too.
    ///
    /// The source's own option surface (`Artificer_HellOptions`, its `.ctor`/`Initialize`
    /// disassembled directly): `timer` (int, default -1 = off, range -1..600 -- this port's own
    /// escape timer, below, already covers the same ground with 0 = off instead of -1, so it
    /// wasn't rebound under a new name); `logging` (bool, default true -- the mod's own internal
    /// debug-log verbosity toggle; NOT ported, since no other gimmick in this codebase exposes a
    /// per-gimmick logging switch and Plugin.Log already runs unconditionally everywhere else --
    /// adding one here just for Chains would be new, un-asked-for surface area); `genocideMode`
    /// (bool, default false -- ported below as CatchLateArrivals, default flipped to true, see its
    /// own doc comment); `selectedBanlist` (string, default "Hardcore", one of four named presets:
    /// "Funny"/"Hardcore"/"Casual"/"NoCritters") plus `excludeVultures`/`excludeSquids` (bool, both
    /// default true) -- together these decide which CreatureTemplate.Type values can never trigger
    /// or join a lock; ported below as BanOptions, one checkbox per creature type instead of a
    /// preset dropdown (see BanOptions' own doc comment for why and for the exact preset contents
    /// this was reverse-engineered from); `huntedMode` (bool, default false) -- ported below as
    /// FauxHuntedBurden, wired to a new global ArtificialIntelligence.Update patch
    /// (ChainsHuntedModeHook) exactly matching the source's own On.ArtificialIntelligence.Update
    /// hook, since that's a genuinely separate, always-on-regardless-of-lock-state effect in the
    /// source, not something scoped to a currently-locked room.
    ///
    /// See GimmickDef.cs for how BindSettings/BuildSettingsUI plug into PrimalOptions's
    /// drill-down menu, and this file's own header comment for the full reverse-engineering trace.
    /// </summary>
    internal static class ChainsSettings
    {
        private const int DefaultEscapeTimerSeconds = 90;
        private const int MinEscapeTimerSeconds = 0;
        private const int MaxEscapeTimerSeconds = 600;

        private static readonly Dictionary<string, Configurable<int>> _escapeTimerSeconds = new Dictionary<string, Configurable<int>>();

        /// <summary>
        /// Ported from the source's "Total Genocide Kill Everybody FUCK YEAH!!!!:" checkbox
        /// (internal field name "genocideMode") -- in the source this gates its entire
        /// Creature.NewRoom/AddNewCreature arrival-handling path, exactly like it gates
        /// ChainsGimmick.HandleCreatureArrival here. The source itself defaults this OFF; this
        /// port deliberately defaults it ON instead, since (as of 2026-09-12) that behavior is the
        /// confirmed, working, expected default for this gimmick, not an opt-in extra -- the
        /// checkbox exists purely as an escape hatch back to the original "only what's already in
        /// the room when you enter counts" behavior, for anyone who wants that instead.
        /// </summary>
        private static readonly Dictionary<string, Configurable<bool>> _catchLateArrivals = new Dictionary<string, Configurable<bool>>();

        /// <summary>
        /// Ported from the source's "Faux-hunted burden:" checkbox (internal field name
        /// "huntedMode") -- see ChainsHuntedModeHook. Off by default, matching the source exactly.
        /// </summary>
        private static readonly Dictionary<string, Configurable<bool>> _fauxHuntedBurden = new Dictionary<string, Configurable<bool>>();

        /// <summary>
        /// One Configurable&lt;bool&gt; per BanOptions entry -- see that field's own doc comment.
        /// Keyed by BanOption.Key rather than CreatureTemplate.Type directly since a couple of
        /// entries (Vultures, Cicadas) each cover more than one Type with a single checkbox.
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, Configurable<bool>>> _typeBans = new Dictionary<string, Dictionary<string, Configurable<bool>>>();

        /// <summary>
        /// The quick-preset select menu, brought back 2026-09-12 at the user's request after the
        /// per-type checkboxes below replaced it outright in the original settings-import pass --
        /// see BanOptions' own doc comment for why a dropdown was skipped the first time, and
        /// PresetContents just below for how picking one here maps onto those same checkboxes.
        /// Ctor confirmed by disassembling the real `Menu.Remix.MixedUI.OpListBox` in the game's
        /// own assembly (four overloads found; this is the one taking a plain `string[]` of
        /// options directly rather than a `List&lt;ListItem&gt;` or relying on the bound
        /// Configurable's own acceptable-range, since that overload was the simplest to verify
        /// correct from metadata alone): `OpListBox(Configurable&lt;string&gt; config, Vector2 pos,
        /// float width, string[] array, ushort lineCount, bool downward)`. First attempt at this
        /// wrote the fifth parameter's type down as `int` -- a genuine one-off ECMA-335 element-type
        /// mapping mistake in the original hand-decode (0x07 is ELEMENT_TYPE_U2/ushort, not I4/int;
        /// this file's earlier, more careful signature reads got that table right, this one didn't)
        /// -- which silently knocked the intended overload out of resolution entirely and made the
        /// compiler report a confusing mismatch against a *different* overload's List&lt;ListItem&gt;
        /// parameter instead of the real problem. Fixed by casting `PresetNames.Length` to
        /// `(ushort)` at the call site below; re-verified this whole signature afterward with a
        /// proper from-scratch signature decoder (scratchpad/dnparse/sigdecode.py) instead of by
        /// eye, specifically to catch exactly this class of mistake before it reaches the user again.
        /// </summary>
        private static readonly Dictionary<string, Configurable<string>> _banlistPreset = new Dictionary<string, Configurable<string>>();

        private static readonly string[] PresetNames = { "Funny", "Hardcore", "Casual", "NoCritters" };

        /// <summary>
        /// Which of the 10 individually-togglable (non-grouped) BanOptions keys each named preset
        /// turns on -- everything else among those 10 is turned off. Deliberately does NOT touch
        /// the "Vultures"/"Cicadas" checkboxes: in the source mod those come from separate,
        /// preset-independent toggles (excludeVultures/excludeSquids), not from selectedBanlist
        /// itself, so picking a preset here leaves them exactly as the player last set them, same
        /// as the source's own behavior.
        /// </summary>
        private static readonly Dictionary<string, string[]> PresetContents = new Dictionary<string, string[]>
        {
            ["Funny"] = new string[0],
            ["Hardcore"] = new[] { "GarbageWorm", "VultureGrub", "EelLizard" },
            ["Casual"] = new[] { "GarbageWorm", "VultureGrub", "EelLizard", "TempleGuard", "BigEel", "Deer" },
            ["NoCritters"] = new[] { "GarbageWorm", "VultureGrub", "EelLizard", "TempleGuard", "BigEel", "Deer", "Overseer", "Spider", "Leech", "Fly" },
        };

        private static readonly HashSet<string> GroupedBanOptionKeys = new HashSet<string> { "Vultures", "Cicadas" };

        private static readonly Dictionary<string, List<UIelement>> _uiCache = new Dictionary<string, List<UIelement>>();

        private readonly struct BanOption
        {
            public readonly string Key;
            public readonly string Label;
            public readonly bool DefaultOn;
            public readonly CreatureTemplate.Type[] Types;

            public BanOption(string key, string label, bool defaultOn, params CreatureTemplate.Type[] types)
            {
                Key = key;
                Label = label;
                DefaultOn = defaultOn;
                Types = types;
            }
        }

        /// <summary>
        /// Every creature type any of the source mod's four named banlist presets or its two
        /// exclude-toggles ever touched (Slugcat excluded from this list -- it's a permanent,
        /// non-configurable ban in every one of the source's own presets too, applied directly in
        /// IsBanned below, since locking a room over just yourself is meaningless). Reconstructed
        /// by disassembling CustomHRGuardManager's own constructor, which builds a
        /// Dictionary&lt;string, HashSet&lt;CreatureTemplate.Type&gt;&gt; keyed by preset name:
        ///   Funny:      { }                                        (no bans beyond Slugcat)
        ///   Hardcore:   { GarbageWorm, VultureGrub, EelLizard }
        ///   Casual:     Hardcore + { TempleGuard, BigEel, Deer }
        ///   NoCritters: Casual + { Overseer, Spider, Leech, Fly }
        /// ...then, regardless of preset, UnionWith { Vulture, KingVulture } if excludeVultures,
        /// and UnionWith { CicadaA, CicadaB } if excludeSquids (confirmed via metadata -- the
        /// source's own "squids" checkbox label is community slang for cicadas, not any
        /// vanilla-named squid creature; there isn't one).
        ///
        /// Collapsed here into one independent checkbox per type, PLUS the quick-preset select
        /// menu above (`_banlistPreset`/`PresetContents`) that sets several of them at once --
        /// strictly more expressive than the source's four fixed combinations alone, since any
        /// individual checkbox can still be fine-tuned after picking a preset, while still keeping
        /// the source's own "just pick one of four" convenience front and center rather than
        /// buried behind 12 individual checkboxes. Every DefaultOn value below reproduces exactly
        /// what the source's own real defaults (selectedBanlist="Hardcore", excludeVultures=true,
        /// excludeSquids=true) ban by default -- not the "safest" NoCritters preset this port
        /// originally (incorrectly) assumed as a stand-in default before the first settings-import
        /// pass looked at the actual bind call.
        /// </summary>
        private static readonly BanOption[] BanOptions =
        {
            new BanOption("GarbageWorm", "Garbage Worms", true, CreatureTemplate.Type.GarbageWorm),
            new BanOption("VultureGrub", "Vulture Grubs", true, CreatureTemplate.Type.VultureGrub),
            new BanOption("EelLizard", "Eel Lizards", true, DLCSharedEnums.CreatureTemplateType.EelLizard),
            new BanOption("Vultures", "Vultures", true, CreatureTemplate.Type.Vulture, CreatureTemplate.Type.KingVulture),
            new BanOption("Cicadas", "Cicadas (\"squids\")", true, CreatureTemplate.Type.CicadaA, CreatureTemplate.Type.CicadaB),
            new BanOption("TempleGuard", "Temple Guards", false, CreatureTemplate.Type.TempleGuard),
            new BanOption("BigEel", "Big Eels", false, CreatureTemplate.Type.BigEel),
            new BanOption("Deer", "Rain Deer", false, CreatureTemplate.Type.Deer),
            new BanOption("Overseer", "Overseers", false, CreatureTemplate.Type.Overseer),
            new BanOption("Spider", "Spiders", false, CreatureTemplate.Type.Spider),
            new BanOption("Leech", "Leeches", false, CreatureTemplate.Type.Leech),
            new BanOption("Fly", "Batflies", false, CreatureTemplate.Type.Fly),
        };

        public static void Bind(OptionInterface owner, string characterID)
        {
            _escapeTimerSeconds[characterID] = owner.config.Bind(
                characterID + "_Chains_escapeTimerSeconds",
                DefaultEscapeTimerSeconds,
                new ConfigurableInfo(
                    "Seconds before a locked room gives up and releases you even if it isn't cleared. 0 disables the timer -- you must kill everything in the room to leave.",
                    new ConfigAcceptableRange<int>(MinEscapeTimerSeconds, MaxEscapeTimerSeconds)));

            _catchLateArrivals[characterID] = owner.config.Bind(
                characterID + "_Chains_catchLateArrivals",
                true,
                new ConfigurableInfo("If checked, a creature that wanders into an already-tracked room after you enter it can start or join the lock too -- not just whatever was already there when you walked in."));

            _fauxHuntedBurden[characterID] = owner.config.Bind(
                characterID + "_Chains_fauxHuntedBurden",
                false,
                new ConfigurableInfo("If checked, every creature sharing a room with you is forced to instantly notice you, bypassing stealth entirely -- whether or not that room is currently locked."));

            var typeBans = new Dictionary<string, Configurable<bool>>();
            foreach (BanOption option in BanOptions)
            {
                typeBans[option.Key] = owner.config.Bind(
                    characterID + "_Chains_ban_" + option.Key,
                    option.DefaultOn,
                    new ConfigurableInfo("Checked: never counts toward triggering or joining a room lock, and can't be chained."));
            }
            _typeBans[characterID] = typeBans;

            // Bound after typeBans is populated above, so ApplyPreset (which reads _typeBans) has
            // something to act on even if OnChange somehow fired the instant this is bound.
            Configurable<string> preset = owner.config.Bind(
                characterID + "_Chains_banlistPreset",
                "Hardcore",
                new ConfigurableInfo("Quick preset -- picking one immediately sets the individual creature checkboxes below to match it (Vultures/Cicadas are untouched by this, they stay independent). You can still fine-tune any checkbox afterward."));
            preset.OnChange += () => ApplyPreset(characterID);
            _banlistPreset[characterID] = preset;
        }

        /// <summary>
        /// Fired by the quick-preset OpListBox's own Configurable&lt;string&gt;.OnChange -- sets
        /// every non-grouped BanOptions checkbox to match PresetContents[presetName], leaving the
        /// grouped ones (Vultures, Cicadas) exactly as they were, matching the source mod's own
        /// selectedBanlist behavior (never touches excludeVultures/excludeSquids).
        /// </summary>
        private static void ApplyPreset(string characterID)
        {
            if (!_banlistPreset.TryGetValue(characterID, out Configurable<string> preset) ||
                !_typeBans.TryGetValue(characterID, out Dictionary<string, Configurable<bool>> bans) ||
                !PresetContents.TryGetValue(preset.Value, out string[] onKeys))
            {
                return;
            }

            var onSet = new HashSet<string>(onKeys);
            foreach (BanOption option in BanOptions)
            {
                if (GroupedBanOptionKeys.Contains(option.Key))
                {
                    continue;
                }
                if (bans.TryGetValue(option.Key, out Configurable<bool> config))
                {
                    config.Value = onSet.Contains(option.Key);
                }
            }
        }

        public static int GetEscapeTimerSeconds(string characterID)
        {
            return _escapeTimerSeconds.TryGetValue(characterID, out Configurable<int> c) ? c.Value : DefaultEscapeTimerSeconds;
        }

        public static bool CatchLateArrivals(string characterID)
        {
            // Defaults true if this character's settings somehow haven't been bound yet -- matches
            // the field's own default and keeps the arrival-handling feature "just on" rather than
            // silently disabled by a missing bind.
            return !_catchLateArrivals.TryGetValue(characterID, out Configurable<bool> c) || c.Value;
        }

        public static bool FauxHuntedBurden(string characterID)
        {
            return _fauxHuntedBurden.TryGetValue(characterID, out Configurable<bool> c) && c.Value;
        }

        /// <summary>
        /// True if this creature type should never count toward, or be added to, a room's lock for
        /// this character -- Slugcat is always banned (hardcoded, not configurable, matching every
        /// one of the source's own presets), everything else follows whichever BanOptions checkbox
        /// (if any) covers that type.
        /// </summary>
        public static bool IsBanned(string characterID, CreatureTemplate.Type type)
        {
            if (type == CreatureTemplate.Type.Slugcat)
            {
                return true;
            }

            if (!_typeBans.TryGetValue(characterID, out Dictionary<string, Configurable<bool>> bans))
            {
                return false;
            }

            foreach (BanOption option in BanOptions)
            {
                if (Array.IndexOf(option.Types, type) < 0)
                {
                    continue;
                }
                return bans.TryGetValue(option.Key, out Configurable<bool> c) && c.Value;
            }

            return false;
        }

        public static List<UIelement> BuildUI(string characterID, float topY)
        {
            if (_uiCache.TryGetValue(characterID, out List<UIelement> cached))
            {
                return cached;
            }

            var items = new List<UIelement>();
            if (!_escapeTimerSeconds.TryGetValue(characterID, out Configurable<int> escapeTimer) ||
                !_catchLateArrivals.TryGetValue(characterID, out Configurable<bool> catchLate) ||
                !_fauxHuntedBurden.TryGetValue(characterID, out Configurable<bool> hunted) ||
                !_banlistPreset.TryGetValue(characterID, out Configurable<string> preset) ||
                !_typeBans.TryGetValue(characterID, out Dictionary<string, Configurable<bool>> bans))
            {
                // Not bound yet -- don't cache an incomplete/empty result, in case Bind() hasn't
                // run for this character yet (see NoSpearsSettings for the same guard and why).
                return items;
            }

            items.Add(new OpLabel(20f, topY, "Escape timer, in seconds (0 = must clear room):", false));
            items.Add(new OpUpdown(escapeTimer, new Vector2(280f, topY - 6f), 80f));

            items.Add(new OpCheckBox(catchLate, 20f, topY - 44f));
            items.Add(new OpLabel(56f, topY - 40f, "Catch creatures that wander in after you enter", false));

            items.Add(new OpCheckBox(hunted, 20f, topY - 80f));
            items.Add(new OpLabel(56f, topY - 76f, "Faux-hunted burden (forces nearby creatures to notice you)", false));

            // Quick preset select menu -- picking one immediately sets the individual checkboxes
            // below (ApplyPreset, wired via Configurable<string>.OnChange in Bind). Positioned to
            // occupy roughly the same footprint as a 4-line dropdown (~96px tall); exact pixel fit
            // is unverified in-game as of this pass -- see AI_CONTEXT.md.
            float presetY = topY - 112f;
            items.Add(new OpLabel(20f, presetY, "Quick preset:", false));
            items.Add(new OpListBox(preset, new Vector2(140f, presetY), 120f, PresetNames, (ushort)PresetNames.Length, true));

            items.Add(new OpLabel(20f, presetY - 116f, "Never lock a room over these (checked = excluded):", false));

            const float rowHeight = 32f;
            const float columnAX = 20f, columnALabelX = 56f;
            const float columnBX = 310f, columnBLabelX = 346f;
            const int rowsPerColumn = 6;
            float gridTop = presetY - 152f;
            for (int i = 0; i < BanOptions.Length; i++)
            {
                BanOption option = BanOptions[i];
                if (!bans.TryGetValue(option.Key, out Configurable<bool> config))
                {
                    continue;
                }

                int column = i / rowsPerColumn;
                int row = i % rowsPerColumn;
                float x = column == 0 ? columnAX : columnBX;
                float labelX = column == 0 ? columnALabelX : columnBLabelX;
                float y = gridTop - (row * rowHeight);

                items.Add(new OpCheckBox(config, x, y));
                items.Add(new OpLabel(labelX, y + 4f, option.Label, false));
            }

            _uiCache[characterID] = items;
            return items;
        }
    }
}
